using UnityEditor;
using UnityEngine;

namespace LiveLarson.Plugins.UIs.Editor
{
    [CustomEditor(typeof(TransText))]
    public class TransTextEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Collect"))
            {
                var text = target as TransText;
                text.Collect();
                EditorUtility.SetDirty(text);
            }
        }
    }
}

