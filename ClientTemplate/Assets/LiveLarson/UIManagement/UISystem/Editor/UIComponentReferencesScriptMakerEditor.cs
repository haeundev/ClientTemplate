#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LiveLarson.UIManagement.UISystem.Editor
{
    [CustomPropertyDrawer(typeof(UIComponentReferencesScriptMaker.UIComponent))]
    public class IComponentReferencesScriptMakerComponentDrawer : PropertyDrawer
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
#endif