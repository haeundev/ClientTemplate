using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiveLarson.Util;
using TMPro;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.UI;
using Type = LiveLarson.Util.Type;

namespace LiveLarson.Plugins.UIs.Editor
{
    public class UIWindowScriptMaker : EditorWindow
    {
        private const string BaseDirectory = "Assets/Scripts/UI";
        private readonly string _defaultNamespaceName = "UI";
        private bool _analyzeFold;
        private bool _forceGenerate;

        private GameObjectTree _gameObjectTree;
        private bool _includeInactiveObject;
        private bool _includeUIWindowObject;
        private Vector2 _scrollPos;
        private UIDataHolder _uiDataHolder;

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            GuiTargetTree();
            GuiAnalyze();
            GuiBuild();
            GuiAttach();
            EditorGUILayout.EndScrollView();
        }

        [MenuItem("Window/UI Window Script Maker (Old)")]
        public static void ShowWindow()
        {
            GetWindow(typeof(UIWindowScriptMaker));
        }

        public static void OpenWindow(UIWindow window)
        {
            var maker = GetWindow(typeof(UIWindowScriptMaker)) as UIWindowScriptMaker;
            if (maker._gameObjectTree == null)
                maker._gameObjectTree = CreateInstance<GameObjectTree>();
            maker._gameObjectTree.Root.target = window.gameObject;
            foreach (var sub in window.SubWindowSources) maker._gameObjectTree.Root.AddChild().target = sub.gameObject;
            maker._includeInactiveObject = true;
            maker.Analyze();
            maker._analyzeFold = true;
            var coms = maker._uiDataHolder.uiData[0].components;
            coms.ForEach(p => p.enable = false);
            for (var i = 0; i < window.GetUICount; i++)
            {
                var ui = window.GetUI(i);
                if (ui == null)
                    continue;
                var target = coms.FirstOrDefault(p => p.uiComponent.component.gameObject == ui.gameObject);
                if (target != null)
                    target.enable = true;
            }
        }

        private void GuiTargetTree()
        {
            if (_gameObjectTree == null)
                _gameObjectTree = CreateInstance<GameObjectTree>();

            GUILayout.Label("Target Settings", EditorStyles.boldLabel);
            var editor = UnityEditor.Editor.CreateEditor(_gameObjectTree);
            editor.OnInspectorGUI();
        }

        private void GuiAnalyze()
        {
            if ((_uiDataHolder?.uiData?.Count ?? 0) == 0)
                _analyzeFold = false;

            GUILayout.Space(10);
            GUILayout.Label("Analyze", EditorStyles.boldLabel);
            _includeInactiveObject = GUILayout.Toggle(_includeInactiveObject, "Include inactive gameobjects");
            _includeUIWindowObject = GUILayout.Toggle(_includeUIWindowObject, "Include UI window gameobjects");
            if (GUILayout.Button("Analyze"))
            {
                Analyze();
                EditorUtility.SetDirty(_uiDataHolder);
                _analyzeFold = true;
            }

            _analyzeFold = EditorGUILayout.Foldout(_analyzeFold, "--- analyzed result ---");
            if (_analyzeFold && _uiDataHolder != null)
            {
                var uiEditor = UnityEditor.Editor.CreateEditor(_uiDataHolder);
                uiEditor.DrawDefaultInspectorWithoutScriptField();
            }
        }

        private void GuiBuild()
        {
            GUILayout.Space(10);
            GUILayout.Label("Build", EditorStyles.boldLabel);
            _forceGenerate = GUILayout.Toggle(_forceGenerate, "Force Regenerate All Files");

            if (GUILayout.Button("Build"))
            {
                Build();
                EditorUtility.SetDirty(_uiDataHolder);
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                CompilationPipeline.RequestScriptCompilation();
                Debug.Log("Build Done.");
            }
        }

        private void GuiAttach()
        {
            GUILayout.Space(10);
            GUILayout.Label("Attach", EditorStyles.boldLabel);
            if (GUILayout.Button("Attach"))
                try
                {
                    Attach();
                    Debug.Log("Attach Done.");
                }
                catch (Exception e)
                {
                    Debug.LogError("Attach Failed.\n" + e);
                }
        }

        private void Analyze()
        {
            if (_uiDataHolder == null)
                _uiDataHolder = CreateInstance<UIDataHolder>();

            _uiDataHolder.uiData = new List<UIData>();

            var node = _gameObjectTree.Root;

            Analyze(node, _uiDataHolder.uiData, _includeInactiveObject, _includeUIWindowObject);
        }

        private static void Analyze(GameObjectTree.Node node, List<UIData> uiDatas, bool includeInactives,
            bool includeUIWindowChild)
        {
            var uiData = new UIData();
            uiDatas.Add(uiData);

            uiData.target = node.target;
            var ignores = node.children.Select(p => p.target).ToList();
            CollectUIComponents(node.target.transform, uiData, ignores, includeInactives, includeUIWindowChild);

            foreach (var child in node.children)
                if (includeUIWindowChild || child.target.GetComponent<UIWindow>() == null)
                    Analyze(child, uiDatas, includeInactives, includeUIWindowChild);
        }

        public static void CollectUIComponents(Transform tran, UIData data, List<GameObject> ignores,
            bool includeInactives, bool includeUIWindowChild)
        {
            var ui = FindUICompoenet(tran);
            if (ui != null && tran.gameObject != data.target)
            {
                data.components.Add(ui);
                if (includeUIWindowChild == false && ui.uiComponent.type == UIWindow.UIComponent.Types.UIWindow)
                    return;
            }

            var count = tran.childCount;
            for (var i = 0; i < count; i++)
            {
                var child = tran.GetChild(i);
                if (child != null)
                {
                    var obj = child.gameObject;
                    if (obj.activeInHierarchy == false && !includeInactives)
                        continue;
                    if (ignores.Find(p => p.Equals(obj)))
                        continue;
                    CollectUIComponents(obj.transform, data, ignores, includeInactives, includeUIWindowChild);
                }
            }
        }

        public static UIComponent FindUICompoenet(Transform tran)
        {
            var components = tran.GetComponents<Component>();
            UIComponent component = null;
            foreach (var com in components)
            {
                var temp = CreateUIComponent(com);
                if (temp == null)
                    continue;

                var type = temp.uiComponent.type;
                if (type == UIWindow.UIComponent.Types.Image
                    || type == UIWindow.UIComponent.Types.RawImage
                    || type == UIWindow.UIComponent.Types.Animator)
                {
                    if (component == null || type > component.uiComponent.type)
                        component = temp;
                    continue;
                }

                component = temp;
                break;
            }

            return component;
        }

        public static UIComponent CreateUIComponent(Component component)
        {
            switch (component)
            {
                case Image ui: return new UIComponent(UIWindow.UIComponent.Types.Image, ui);
                case RawImage ui: return new UIComponent(UIWindow.UIComponent.Types.RawImage, ui);
                case Text ui: return new UIComponent(UIWindow.UIComponent.Types.Text, ui);
                case TextMeshProUGUI ui: return new UIComponent(UIWindow.UIComponent.Types.TextMeshProUGUI, ui);
                case Button ui: return new UIComponent(UIWindow.UIComponent.Types.Button, ui);
                case Toggle ui: return new UIComponent(UIWindow.UIComponent.Types.Toggle, ui);
                case Slider ui: return new UIComponent(UIWindow.UIComponent.Types.Slider, ui);
                case Scrollbar ui: return new UIComponent(UIWindow.UIComponent.Types.Scrollbar, ui);
                case Dropdown ui: return new UIComponent(UIWindow.UIComponent.Types.Dropdown, ui);
                case TMP_Dropdown ui: return new UIComponent(UIWindow.UIComponent.Types.TMPDropdown, ui);
                case InputField ui: return new UIComponent(UIWindow.UIComponent.Types.InputField, ui);
                case ScrollRect ui: return new UIComponent(UIWindow.UIComponent.Types.ScrollRect, ui);
                case UIGameObject ui: return new UIComponent(UIWindow.UIComponent.Types.UIGameObject, ui);
                case TransText ui: return new UIComponent(UIWindow.UIComponent.Types.TransText, ui);
                case UIChildSelector ui: return new UIComponent(UIWindow.UIComponent.Types.UIChildSelector, ui);
                case UITweenObject ui: return new UIComponent(UIWindow.UIComponent.Types.UITweenObject, ui);
                case UIWindow ui: return new UIComponent(UIWindow.UIComponent.Types.UIWindow, ui);
                case Animator ui: return new UIComponent(UIWindow.UIComponent.Types.Animator, ui);
                case UIComponentReferences ui:
                    return new UIComponent(UIWindow.UIComponent.Types.UIComponentReferences, ui);
            }

            return null;
        }

        private void Build()
        {
            if (_uiDataHolder.uiData.Count == 0)
                return;

            BuildClass(_gameObjectTree.Root);
        }

        private void BuildClass(GameObjectTree.Node node)
        {
            var className = node.target.name;
            var targetDirectory = BaseDirectory;
            var targetPath = Path.Combine(targetDirectory, className + ".cs");
            var targetGeneratedPath = Path.Combine(targetDirectory, className + "_generated.cs");
            Directory.CreateDirectory(targetDirectory);
            WriteFile(node, targetGeneratedPath, true);
            if (_forceGenerate || File.Exists(targetPath) == false)
                WriteFile(node, targetPath, false);
            node.children.ForEach(p =>
            {
                if (p.target.GetComponent<UIWindow>() == null)
                    BuildClass(p);
            });
        }

        private void WriteFile(GameObjectTree.Node node, string path, bool isGenerated)
        {
            var indentLevel = 1;
            using (var writer = new StreamWriter(path))
            {
                if (isGenerated)
                    writer.WriteLine("//Generated file. Don't modify this script.");

                writer.WriteLine("using UnityEngine;");
                writer.WriteLine("using UnityEngine.UI;");
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine("using LiveLarson.Plugins.UIs;");
                writer.WriteLine("using TMPro;");
                writer.WriteLine("");
                writer.WriteLine("namespace " + _defaultNamespaceName);
                writer.WriteLine("{");
                WriteClass(writer, node, indentLevel, isGenerated);
                writer.WriteLine("}");
                writer.WriteLine("");
                writer.Close();
            }
        }

        private UIData GetUIData(GameObjectTree.Node node)
        {
            var result = _uiDataHolder.uiData.Find(p => p.target.Equals(node.target));
            return result;
        }

        private void WriteClass(StreamWriter writer, GameObjectTree.Node node, int indentLevel, bool isGenerated)
        {
            var className = node.target.name;
            var indent = GetIndentString(indentLevel);
            writer.WriteLine($"{indent}public partial class {className} : UIWindow");
            writer.WriteLine($"{indent}{{");
            if (isGenerated) WriteComponentEnumAndProperty(writer, node, indentLevel + 1);
            if ((node.children?.Count ?? 0) > 0)
                if (isGenerated)
                {
                    WriteSubWindowEnum(writer, node, indentLevel + 1);
                    WriteSubWindowList(writer, node, indentLevel + 1);
                    WriteSubWindowAdder(writer, node, indentLevel + 1);
                }

            writer.WriteLine($"{indent}}}");
            writer.WriteLine("");
        }

        private void WriteComponentEnumAndProperty(StreamWriter writer, GameObjectTree.Node node, int indentLevel)
        {
            var indent = GetIndentString(indentLevel);
            var uiData = GetUIData(node);
            var uiComponets = uiData.components;
            writer.WriteLine($"{indent}public enum UIComponents : int");
            writer.WriteLine($"{indent}{{");
            foreach (var component in uiComponets)
            {
                if (component.enable == false)
                    continue;

                writer.WriteLine($"{indent}    {component.uiComponent.component.name},");
            }

            writer.WriteLine($"{indent}}}");
            writer.WriteLine("");
            var index = 0;
            foreach (var component in uiComponets)
            {
                if (component.enable == false)
                    continue;

                var type = component.uiComponent.type.ToString();
                if (component.uiComponent.type == UIWindow.UIComponent.Types.UIWindow || component.uiComponent.type ==
                    UIWindow.UIComponent.Types.UIComponentReferences)
                    type = component.uiComponent.component.GetType().Name;
                writer.WriteLine(
                    $"{indent}public {type} {component.uiComponent.component.name} => GetUI({index}) as {type};");
                index++;
            }
        }

        private void WriteSubWindowEnum(StreamWriter writer, GameObjectTree.Node node, int indentLevel)
        {
            var indent = GetIndentString(indentLevel);
            var subWindows = node.children;
            writer.WriteLine($"{indent}private enum SubWindows : int");
            writer.WriteLine($"{indent}{{");
            foreach (var window in subWindows) writer.WriteLine($"{indent}    {window.target.name},");

            writer.WriteLine($"{indent}}}");
            writer.WriteLine("");
        }

        private void WriteSubWindowList(StreamWriter writer, GameObjectTree.Node node, int indentLevel)
        {
            var indent = GetIndentString(indentLevel);
            var subWindows = node.children;
            foreach (var window in subWindows)
                writer.WriteLine(
                    $"{indent}public List<{window.target.name}> {window.target.name}s = new List<{window.target.name}>();");
        }

        private void WriteSubWindowAdder(StreamWriter writer, GameObjectTree.Node node, int indentLevel)
        {
            var indent = GetIndentString(indentLevel);
            var subWindows = node.children;
            foreach (var window in subWindows)
            {
                writer.WriteLine($"{indent}public {window.target.name} Add{window.target.name}()");
                writer.WriteLine($"{indent}{{");
                writer.WriteLine(
                    $"{indent}    var window = AddSubWindow((int)SubWindows.{window.target.name}) as {window.target.name};");
                writer.WriteLine($"{indent}    {window.target.name}s.Add(window);");
                writer.WriteLine($"{indent}    return window;");
                writer.WriteLine($"{indent}}}");
                writer.WriteLine("");
            }
        }

        private string GetIndentString(int indentLevel)
        {
            return new string(' ', indentLevel * 4);
        }

        private void Attach()
        {
            var baseName = _defaultNamespaceName + ".";
            var node = _gameObjectTree.Root;
            var typeName = baseName + node.target.name;
            var t = Type.GetType(typeName);
            if (t != null)
            {
                Attach(baseName, node);
                DestroyImmediate(node.target.GetComponent<UIWindow>()); // remove previous UIWindow component
                Debug.Log("Attach Done.");
                EditorUtility.SetDirty(node.target);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private void Attach(string baseName, GameObjectTree.Node node)
        {
            var uiData = GetUIData(node);
            if (uiData == null)
                return;

            var target = node.target;
            var typeName = baseName + target.name;
            var t = Type.GetType(typeName);
            var com = target.GetComponent(t);
            if (com == null)
            {
                com = target.AddComponent(t);
            }

            var window = com as UIWindow;
            window.SetUIComponents(uiData.components.Where(p => p.enable).Select(p => p.uiComponent).ToList());
            window.SetSubWindowTransforms(new TransformList(node.children.Select(p => p.target.transform).ToList()));
            window.SetSubWindowParents(
                new TransformList(node.children.Select(p => p.target.transform.parent).ToList()));

            //baseName = typeName + ".";
            foreach (var child in node.children) Attach(baseName, child);
        }

        [Serializable]
        public class UIComponent
        {
            public bool enable = true;
            public UIWindow.UIComponent uiComponent;

            public UIComponent(UIWindow.UIComponent.Types type, Component component)
            {
                enable = true;
                uiComponent = new UIWindow.UIComponent(type, component);
            }
        }

        [Serializable]
        public class UIData
        {
            public GameObject target;
            public List<UIComponent> components = new();
        }

        public class UIDataHolder : ScriptableObject
        {
            public List<UIData> uiData;
        }
    }
}