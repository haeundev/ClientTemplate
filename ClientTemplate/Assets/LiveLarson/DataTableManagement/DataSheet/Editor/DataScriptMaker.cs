using System;
using System.IO;
using LiveLarson.Util;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Type = LiveLarson.Util.Type;

namespace LiveLarson.DataTableManagement.DataSheet.Editor
{
    public class DataScriptMaker : EditorWindow
    {
        private const string DEFAULT_NAMESPACE_NAME = "DataTables";
        private string BaseDirectory => GetBaseDirectory(_dataType);
        private string ScriptDirectory => GetScriptDirectory(_dataType);

        [MenuItem("Window/Data Script Maker")]
        public static void ShowWindow()
        {
            GetWindow(typeof(DataScriptMaker));
        }

        private string _jsonString;
        private ClassBuilder _classBuilder;
        private bool _isBuilt;
        private string _fileID = "Insert File ID";
        private string _className = "Insert Sheet Name";
        private DataScript.DataType _dataType = DataScript.DataType.Table;
        private string _mainClassName = "";

        private static string GetBaseDirectory(DataScript.DataType dataType)
        {
            return dataType switch
            {
                DataScript.DataType.Table => "Assets/LiveLarson/Data/Table",
                DataScript.DataType.Const => "Assets/LiveLarson/Data/Const",
            };
        }

        public static string GetScriptDirectory(DataScript.DataType dataType)
        {
            return GetBaseDirectory(dataType) + "/Script";
        }

        private void OnGUI()
        {
            GuiLoadFile();
            GuiBuild();
            GuiMakeAsset();
        }

        private void GuiLoadFile()
        {
            GUILayout.Label("Load file", EditorStyles.boldLabel);
            _fileID = GUILayout.TextField(_fileID);
            _dataType = (DataScript.DataType)EditorGUILayout.EnumPopup(_dataType);
            GUILayout.BeginHorizontal();
            _className = GUILayout.TextField(_className);
            if (GUILayout.Button("Load"))
                DataScript.OpenRequest(_fileID, _className, _dataType, (builder, str, rawStr) =>
                {
                    _classBuilder = builder;
                    _jsonString = str;
                    Debug.Log("Load Done. Press [Build]");
                });
            GUILayout.EndHorizontal();
        }

        private void GuiBuild()
        {
            if (string.IsNullOrEmpty(_jsonString))
                return;

            GUILayout.Space(10);
            GUILayout.Label("Build", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Build"))
            {
                Debug.Log("Building... please wait");
                _mainClassName = GetMainClassName(_className);
                Build(_className);
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                CompilationPipeline.RequestScriptCompilation();
                Debug.Log("Build Done. Press [Make Asset]");
                _isBuilt = true;
            }

            GUILayout.EndHorizontal();
        }

        private string GetMainClassName(string className)
        {
            return _dataType == DataScript.DataType.Table ? className.ToPlural() : className;
        }

        private void GuiMakeAsset()
        {
            if (_isBuilt == false)
                return;

            GUILayout.Space(10);
            GUILayout.Label("Make Asset", EditorStyles.boldLabel);
            if (GUILayout.Button("Make"))
                try
                {
                    MakeAsset();
                    Debug.Log("Make Asset Done.");
                }
                catch (Exception e)
                {
                    Debug.LogError("Make Asset Failed.\n" + e);
                }
        }

        private void Build(string className)
        {
            if (string.IsNullOrEmpty(_jsonString))
                return;

            var baseDirectory = BaseDirectory;
            var scriptDirectory = ScriptDirectory;
            Directory.CreateDirectory(baseDirectory);
            Directory.CreateDirectory(scriptDirectory);

            _classBuilder.GenerateClasses(DEFAULT_NAMESPACE_NAME, scriptDirectory);

            var targetPath = Path.Combine(scriptDirectory, _mainClassName + ".cs");
            var targetEditorPath = Path.Combine(scriptDirectory, "Editor", _mainClassName + "Editor.cs");

            Directory.CreateDirectory(Path.Combine(scriptDirectory, "Editor"));
            if (_dataType == DataScript.DataType.Table)
            {
                if (!File.Exists(targetPath))
                    WriteMainFile(targetPath);
                WriteEditorFile(targetEditorPath);
            }
            else if (_dataType == DataScript.DataType.Const)
            {
                WriteConstEditorFile(targetEditorPath);
            }
        }

        private void WriteMainFile(string path)
        {
            var templatePath = "Assets/LiveLarson/DataTableManagement/DataSheet/Editor/root_class.templete";
            var contents = File.ReadAllText(templatePath);
            contents = contents.Replace("{namespace_name}", DEFAULT_NAMESPACE_NAME);
            contents = contents.Replace("{root_class_name}", _mainClassName);
            contents = contents.Replace("{sub_class_name}", _className);
            File.WriteAllText(path, contents);
        }

        private void WriteEditorFile(string path)
        {
            var templatePath = "Assets/LiveLarson/DataTableManagement/DataSheet/Editor/root_class_editor.templete";
            var contents = File.ReadAllText(templatePath);
            contents = contents.Replace("{namespace_name}", DEFAULT_NAMESPACE_NAME);
            contents = contents.Replace("{sheet_name_string}", _className);
            contents = contents.Replace("{root_class_name}", _mainClassName);
            contents = contents.Replace("{sub_class_name}", _className);
            contents = contents.Replace("{file_id_string}", _fileID);
            File.WriteAllText(path, contents);
        }

        private void WriteConstEditorFile(string path)
        {
            var templatePath = "Assets/LiveLarson/DataTableManagement/DataSheet/Editor/const_class_editor.templete";
            var contents = File.ReadAllText(templatePath);
            contents = contents.Replace("{namespace_name}", DEFAULT_NAMESPACE_NAME);
            contents = contents.Replace("{sheet_name_string}", _className);
            contents = contents.Replace("{root_class_name}", _mainClassName);
            contents = contents.Replace("{sub_class_name}", $"{_className}.DataClass");
            contents = contents.Replace("{file_id_string}", _fileID);
            File.WriteAllText(path, contents);
        }

        private string GetIndentString(int indentLevel)
        {
            return new string(' ', indentLevel * 4);
        }

        private void MakeAsset()
        {
            var type = Type.GetCustomType($"{DEFAULT_NAMESPACE_NAME}.{_mainClassName}");
            var asset = CreateInstance(type);
            AssetDatabase.CreateAsset(asset, $"{BaseDirectory}/{_mainClassName}.asset");
            AssetDatabase.Refresh();
        }

        public static void SaveAsset(ScriptableObject asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}