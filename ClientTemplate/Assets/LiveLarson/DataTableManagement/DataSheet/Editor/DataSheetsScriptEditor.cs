using System;
using UnityEditor;
using UnityEngine;

namespace LiveLarson.DataTableManagement.DataSheet.Editor
{
    public abstract class DataSheetsScriptEditor : UnityEditor.Editor
    {
        public abstract string FileID { get; }
        public virtual string[] SheetNames => default;
        private bool _editForTest;
        private bool _tryRebuild;
        public abstract Type SubClassType { get; }
        public virtual DataScript.DataType DataType => DataScript.DataType.Table;

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(!_editForTest);
            base.OnInspectorGUI();
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(_tryRebuild);
            if (GUILayout.Button("Download"))
            {
                StartDownload();
            }
            OnAdditiveInspectorGUI();
            EditorGUI.EndDisabledGroup();
            _editForTest = GUILayout.Toggle(_editForTest, "Edit for test.");
        }

        protected virtual void OnAdditiveInspectorGUI()
        {
        }

        public abstract void ClearAssetData();
        public abstract void SetAssetData(string sheetName, string json);

        public void StartRebuild()
        {
            _tryRebuild = true;
            OpenRequest(Rebuild);
        }

        public void StartDownload()
        {
            OpenRequest(DownLoad);
        }

        public void Rebuild(ClassBuilder builder, string sheetName, string json)
        {
            _tryRebuild = false;
            var type = SubClassType;
            // builder.GenerateClasses(type.Namespace, Editors.DataScriptMaker.GetScriptDirectory(builder.DataType));
            Debug.Log($"Rebuild Done. ({SheetNames})");
        }

        public void DownLoad(ClassBuilder builder, string sheetName, string json)
        {
            SetAssetData(sheetName, json);
            var type = target.GetType();
            var method = type.GetMethod("OnDownloaded");
            method?.Invoke(target, null);
            DataScriptMaker.SaveAsset(target as ScriptableObject);
            EditorUtility.SetDirty(target as ScriptableObject);
            Debug.Log($"DownLoad Done. ({SheetNames})");
        }

        public void OpenRequest(Action<ClassBuilder, string, string> action)
        {
            ClearAssetData();
            
            DataScript.OpenRequest(FileID, target.GetType().ToString(), SheetNames, action);
        }
    }
}
