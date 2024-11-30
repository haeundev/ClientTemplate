using System;
using System.Collections.Generic;
using GoogleSheetsAPI;
using LiveLarson.Util;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace LiveLarson.DataTableManagement.DataSheet.Editor
{
    public static class DataScript
    {
        public enum DataType
        {
            Table,
            Const
        }

        public delegate void OnOpenEvent(ClassBuilder builder, string json, string rawJson);

        public static void OpenRequest(string fileID, string sheetName, DataType dataType, OnOpenEvent onOpen)
        {
            var request = new Request("Assets/LiveLarson/DataTableManagement/DataSheet/Google/credentials.json", "Temp/token.json");
            var values = request.Open(fileID, sheetName);
            if (dataType == DataType.Const)
            {
                ///행렬 전환
                var iValues = new List<IList<object>> { new List<object>(), new List<object>(), new List<object>() };
                values.ForEach(p =>
                {
                    iValues[0].Add(p[0]);
                    iValues[1].Add(p[1]);
                    iValues[2].Add(p[2]);
                });
                values = iValues;
            }

            var builder = MakeBuilder(values, sheetName, dataType);
            var json = MakeJson(values, sheetName, builder);
            var rawJson = MakeRawJson(values, sheetName, fileID);
            onOpen(builder, json, rawJson);
        }

        public static void OpenRequest(string fileID, string tableName, string[] sheetNames,
            Action<ClassBuilder, string, string> onOpen)
        {
            var request = new Request("Assets/LiveLarson/DataTableManagement/DataSheet/Google/credentials.json", "Temp/token.json");
            sheetNames.ForEach(sheetName =>
            {
                IList<IList<object>> values;
                try
                {
                    values = request.Open(fileID, sheetName);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    return;
                }

                var builder = MakeBuilder(values, tableName, DataType.Table);
                var json = MakeJson(values, tableName, builder);
                onOpen(builder, sheetName, json);
            });
        }

        public static ListObject<T> MakeJsonListObjectString<T>(string json)
        {
            var jsonToParse = "{\"Values\":" + json + "}";
            var itemList = JsonConvert.DeserializeObject<ListObject<T>>(jsonToParse);
            return itemList;
        }


        // sentence table의 ML 항목에 " 들어 있어 처리하기 위한 전용 method.
        public static ListObject<T> MakeJsonListObjectStringForSentenceTable<T>(string json)
        {
            var jsonToParse = "{\"Values\":" + json + "}";
            Debug.Log($"parsed JSON: {jsonToParse}");

            //string path = "Assets/Resources/test.txt";
            //StreamWriter writer = new StreamWriter(path, true);
            //writer.WriteLine(jsonToParse);
            //writer.Close();

            var itemList = JsonConvert.DeserializeObject<ListObject<T>>(jsonToParse);
            return itemList;
        }

        public static T MakeObjectFromJsonString<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static ClassBuilder MakeBuilder(IList<IList<object>> values, string name, DataType dataType)
        {
            var classBuilder = new ClassBuilder();
            classBuilder.Build(values, name, dataType);
            return classBuilder;
        }

        public static string MakeJson(IList<IList<object>> values, string name, ClassBuilder builder)
        {
            var jsonBuilder = new JsonBuilder();
            return jsonBuilder.Build(builder.DataType, builder.RootClass, values, $"Temp/{name}.json");
        }

        public static string MakeRawJson(IList<IList<object>> values, string name, string fileID)
        {
            var jsonBuilder = new JsonBuilder();
            return jsonBuilder.BuildRaw(fileID, values, $"Temp/{name}_Raw.json");
        }
    }

    [Serializable]
    public class ListObject<T>
    {
        [FormerlySerializedAs("values")] public List<T> values;
    }
}