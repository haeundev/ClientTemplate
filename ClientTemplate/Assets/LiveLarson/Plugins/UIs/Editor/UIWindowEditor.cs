using System.IO;
using UnityEditor;
using UnityEngine;

namespace LiveLarson.Plugins.UIs.Editor
{
    [CustomEditor(typeof(UIWindow), true)]
    public class UIWindowEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var window = (UIWindow)target;
            var isInitialized = window.GetUICount != 0;
            if (isInitialized)
            {
                base.OnInspectorGUI();

                // Set custom button color
                var originalColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.cyan;

                if (GUILayout.Button("Create Controller", GUILayout.Height(40)))
                {
                    CreateController(window);
                }

                // Restore original color
                GUI.backgroundColor = originalColor;

                return;
            }

            // button to open the window
            if (GUILayout.Button("Create Window", GUILayout.Height(30))) UIWindowScriptMaker.OpenWindow(window);
        }

        private void CreateController(UIWindow window)
        {
            // Ensure the target directory exists
            const string scriptPath = "Assets/Scripts/UI/UIControllers/";
            if (!AssetDatabase.IsValidFolder(scriptPath))
                AssetDatabase.CreateFolder("Assets/Scripts/UI", "UIControllers");

            // Generate the controller class name
            var className = $"{window.name}_Controller";

            // Generate the main controller script
            WriteMainControllerFile(window, scriptPath, className);

            // Generate the auto-generated controller script
            WriteGeneratorControllerFile(window, scriptPath, className);

            AssetDatabase.Refresh();
            Debug.Log($"Controller scripts for {className} have been created.");
        }

        private void WriteMainControllerFile(UIWindow window, string scriptPath, string className)
        {
            var path = $"{scriptPath}{className}.cs";
            using var writer = new StreamWriter(path);
            writer.WriteLine("using System;");
            writer.WriteLine("using LiveLarson.UIManagement.UISystem;");
            writer.WriteLine("");
            writer.WriteLine("namespace UI");
            writer.WriteLine("{");
            writer.WriteLine($"    public partial class {className} : UIController");
            writer.WriteLine("    {");
            writer.WriteLine("        protected override void Awake()");
            writer.WriteLine("        {");
            writer.WriteLine("            Show();");
            writer.WriteLine("            CompleteWindowSetting?.Invoke(this);");
            writer.WriteLine("        }");
            writer.WriteLine("    }");
            writer.WriteLine("}");
        }

        private void WriteGeneratorControllerFile(UIWindow window, string scriptPath, string className)
        {
            var path = $"{scriptPath}{className}_Generated.cs";
            using var writer = new StreamWriter(path);
            writer.WriteLine("//This is Auto Generated Code, Don't modify this script.");
            writer.WriteLine("using System;");
            writer.WriteLine("using LiveLarson.Plugins.UIs;");
            writer.WriteLine("using LiveLarson.UIManagement.UISystem;");
            writer.WriteLine("");
            writer.WriteLine("namespace UI");
            writer.WriteLine("{");
            writer.WriteLine($"    public partial class {className} : UIController");
            writer.WriteLine("    {");
            writer.WriteLine($"        public override System.Type UIWindowType => typeof({window.GetType().Name});");
            writer.WriteLine($"        public {window.GetType().Name} Window => _window as {window.GetType().Name};");
            writer.WriteLine(
                "        public override void SetWindowOption(int id, Action<UIController> completeWindowSetting, WindowOption option)");
            writer.WriteLine("        {");
            writer.WriteLine("            base.SetWindowOption(id, completeWindowSetting, option);");
            writer.WriteLine($"           CreateWindow<{window.GetType().Name}>();");
            writer.WriteLine("        }");
            writer.WriteLine("    }");
            writer.WriteLine("}");
        }
    }


    [CustomPropertyDrawer(typeof(UIWindow.UIComponent))]
    public class UIWindowComponentDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("component"), GUIContent.none);
            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(UIWindowScriptMaker.UIComponent))]
    public class UIWindowScriptMakerComponentDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.indentLevel++;
            var enable = property.FindPropertyRelative("enable");
            EditorGUI.PropertyField(position, enable, GUIContent.none);
            EditorGUI.BeginDisabledGroup(!enable.boolValue);
            EditorGUI.PropertyField(position, property.FindPropertyRelative("uiComponent"), GUIContent.none);
            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}