using System.Collections.Generic;
using System.IO;
using LiveLarson.Plugins.UIs;
using UnityEditor;
using UnityEngine;

namespace LiveLarson.UIManagement.UISystem.Editor
{
    [CustomEditor(typeof(UIContainer))]
    public class UIContainerEditor : UnityEditor.Editor
    {
        private string searchDirectory = "Assets/Prefabs/UIWindows"; // Default directory to search

        public override void OnInspectorGUI()
        {
            // Reference to the target ScriptableObject
            var container = (UIContainer)target;

            // Draw default inspector fields
            DrawDefaultInspector();

            GUILayout.Space(10);
            GUILayout.Label("UI Prefab Collector", EditorStyles.boldLabel);

            // Directory field to specify the folder to search
            searchDirectory = EditorGUILayout.TextField("Search Directory", searchDirectory);

            // Button to collect prefabs
            if (GUILayout.Button("Collect UI Prefabs", GUILayout.Height(30)))
            {
                CollectUIPrefabs(container);
            }

            // Save changes to the ScriptableObject
            if (GUI.changed)
            {
                EditorUtility.SetDirty(container);
            }
        }

        /// <summary>
        /// Collects UI prefabs from the specified directory and updates the UI list in the container.
        /// </summary>
        private void CollectUIPrefabs(UIContainer container)
        {
            if (!Directory.Exists(searchDirectory))
            {
                Debug.LogError($"Directory not found: {searchDirectory}");
                return;
            }

            var prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { searchDirectory });
            var uiList = new List<UIKeyValue>();

            foreach (var guid in prefabGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab != null)
                {
                    var keyValue = new UIKeyValue
                    { 
                        Window = prefab.GetComponent<UIWindow>(),
                        Path = path
                    };
                    uiList.Add(keyValue);
                }
            }

            // Update the container's list
            container.uiList = uiList;
            Debug.Log($"Collected {uiList.Count} UI prefabs from {searchDirectory}.");
        }
    }
}
