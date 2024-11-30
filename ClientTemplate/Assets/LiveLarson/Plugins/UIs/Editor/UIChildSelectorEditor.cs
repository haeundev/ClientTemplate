using UnityEditor;
using UnityEngine;

namespace LiveLarson.Plugins.UIs.Editor
{
    [CustomEditor(typeof(UIChildSelector))]
    public class UIChildSelectorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Collect"))
            {
                var selector = target as UIChildSelector;
                selector.Collect();
                EditorUtility.SetDirty(selector);
            }
        }
    }
}

