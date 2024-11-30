#if UNITY_EDITOR
using UnityEditor;

namespace LiveLarson.Util
{
    public static class EditorExtensions
    {
        public static void DrawDefaultInspectorWithoutScriptField(this UnityEditor.Editor inspector)
        {
            EditorGUI.BeginChangeCheck();
            inspector.serializedObject.Update();
            var iterator = inspector.serializedObject.GetIterator();
            iterator.NextVisible(true);
            while (iterator.NextVisible(false))
                EditorGUILayout.PropertyField(iterator, true);
            inspector.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndChangeCheck();
        }
    }
}
#endif