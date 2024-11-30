#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiveLarson.Plugins.UIs;
using LiveLarson.Util;
using TMPro;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.UI;

namespace LiveLarson.UIManagement.UISystem.Editor
{
    public class UIComponentReferencesScriptMaker : EditorWindow
    {
        private const string DefaultNamespaceName = "UI";
        private const string BaseDirectory = "Assets/Scripts/UI/UIComponentReferences";
        private bool _analyzeFold;
        private bool _doNotSetAnimatorUpdateModesToUnscaledTime;
        private bool _doNotUncheckEverySlidersInteractable;
        private bool _forceGenerate;

        private GameObjectTree _gameObjectTree;
        private UIDataHolder _uiDataHolder;
        private bool includeInactiveObject;
        private bool includeUIWindowObject;
        private Vector2 scrollPos;

        private void OnGUI()
        {
            scrollPos =
                EditorGUILayout.BeginScrollView(scrollPos);
            WindowMakerGUI();
            MoveToUIPathSettingScene();
            EditorGUILayout.EndScrollView();
        }

        [MenuItem("Tools/UI/3. UI Component References Script Maker", false, 3)]
        public static void ShowWindow()
        {
            GetWindow(typeof(UIComponentReferencesScriptMaker));
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
            public List<UIData> uiDatas;
        }

        #region Common

        private string ClassName = string.Empty;
        private const string EditorScenePath = "Assets/Scenes/UIPathSettingScene.unity";

        private void MoveToUIPathSettingScene()
        {
            if (GUILayout.Button("Open UI Path Setting Scene")) EditorSceneManager.OpenScene(EditorScenePath);
        }

        #endregion

        #region Window Maker

        private void WindowMakerGUI()
        {
            GuiTargetTree();
            GuiAnalyze();
            GuiBuild();
            GuiAttach();
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
            if ((_uiDataHolder?.uiDatas?.Count ?? 0) == 0)
                _analyzeFold = false;

            GUILayout.Space(10);
            GUILayout.Label("Analyze", EditorStyles.boldLabel);
            includeInactiveObject = GUILayout.Toggle(includeInactiveObject, "Include inactive gameobjects");
            includeUIWindowObject = GUILayout.Toggle(includeUIWindowObject, "Include UI window gameobjects");
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
            _forceGenerate = GUILayout.Toggle(_forceGenerate, "Overwrite existing file");
            if (GUILayout.Button("Build"))
            {
                Build();
                EditorUtility.SetDirty(_uiDataHolder);
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                CompilationPipeline.RequestScriptCompilation();

                AssetDatabase.Refresh();
                Debug.Log("Build Done.");
            }
        }

        private void GuiAttach()
        {
            GUILayout.Space(10);
            GUILayout.Label("Attach", EditorStyles.boldLabel);

            _doNotSetAnimatorUpdateModesToUnscaledTime = GUILayout.Toggle(_doNotSetAnimatorUpdateModesToUnscaledTime,
                "Do not set any Animator's Update Mode to UnscaledTime");
            _doNotUncheckEverySlidersInteractable = GUILayout.Toggle(_doNotUncheckEverySlidersInteractable,
                "Do not uncheck any Slider's Interactable");

            if (GUILayout.Button("Attach"))
                try
                {
                    if (!_doNotSetAnimatorUpdateModesToUnscaledTime)
                        AnimationPlayModeChange();
                    if (!_doNotUncheckEverySlidersInteractable)
                        UncheckEverySlidersInteractable();

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

            _uiDataHolder.uiDatas = new List<UIData>();

            var node = _gameObjectTree.Root;

            Analyze(node, _uiDataHolder.uiDatas, includeInactiveObject, includeUIWindowObject);
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
            if (_uiDataHolder.uiDatas.Count == 0)
                return;

            BuildClass(_gameObjectTree.Root);
        }

        private void BuildClass(GameObjectTree.Node node)
        {
            ClassName = node.target.name;
            var targetDirectory = BaseDirectory;
            var targetPath = Path.Combine(targetDirectory, ClassName + ".cs");

            Directory.CreateDirectory(targetDirectory);
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
                writer.WriteLine("//Generated file.");
                writer.WriteLine("using UnityEngine;");
                writer.WriteLine("using UnityEngine.UI;");
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine("using LiveLarson.Plugins.UIs;");
                writer.WriteLine("using TMPro;");
                writer.WriteLine("");
                writer.WriteLine("namespace " + DefaultNamespaceName);
                writer.WriteLine("{");
                WriteClass(writer, node, indentLevel);
                writer.WriteLine("}");
                writer.WriteLine("");
                writer.Close();
            }
        }

        private UIData GetUIData(GameObjectTree.Node node)
        {
            var result = _uiDataHolder.uiDatas.Find(p => p.target.Equals(node.target));
            return result;
        }

        private void WriteClass(StreamWriter writer, GameObjectTree.Node node, int indentLevel)
        {
            var className = node.target.name;
            var indent = GetIndentString(indentLevel);
            writer.WriteLine($"{indent}public class {className} : UIComponentReferences");
            writer.WriteLine($"{indent}{{");
            WriteContent(writer, node, indentLevel + 1);
            writer.WriteLine($"{indent}}}");
            writer.WriteLine("");
        }

        private void WriteContent(StreamWriter writer, GameObjectTree.Node node, int indentLevel)
        {
            var indent = GetIndentString(indentLevel);
            var uiData = GetUIData(node);
            var uiComponets = uiData.components;

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

        private string GetIndentString(int indentLevel)
        {
            return new string(' ', indentLevel * 4);
        }

        private void Attach()
        {
            var baseName = DefaultNamespaceName + ".";
            var node = _gameObjectTree.Root;
            var typeName = baseName + node.target.name;
            var t = Util.Type.GetType(typeName);
            if (t != null)
            {
                Attach(baseName, node);
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
            var t = Util.Type.GetType(typeName);
            var com = target.GetComponent(t);
            if (com == null)
                com = target.AddComponent(t);

            ((UIComponentReferences)com).SetUIComponents(uiData.components.Where(p => p.enable)
                .Select(p => p.uiComponent).ToList());

            //baseName = typeName + ".";
            foreach (var child in node.children) Attach(baseName, child);
        }

        private void AnimationPlayModeChange()
        {
            foreach (var animator in from data in _uiDataHolder.uiDatas
                     from uiComponent in data.components
                     where uiComponent.uiComponent.type == UIWindow.UIComponent.Types.Animator
                     select uiComponent.uiComponent.component as Animator)
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        private void UncheckEverySlidersInteractable()
        {
            foreach (var slider in from data in _uiDataHolder.uiDatas
                     from uiComponent in data.components
                     where uiComponent.uiComponent.type == UIWindow.UIComponent.Types.Slider
                     select uiComponent.uiComponent.component as Slider)
                slider.interactable = false;
        }

        #endregion
    }
}
#endif