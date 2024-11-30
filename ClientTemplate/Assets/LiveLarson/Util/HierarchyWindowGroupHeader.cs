#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace LiveLarson.Util
{
    /// <summary>
    ///     Hierarchy Window Group Header
    ///     http://diegogiacomelli.com.br/unitytips-hierarchy-window-group-header
    /// </summary>
    [InitializeOnLoad]
    public static class HierarchyWindowGroupHeader
    {
        static HierarchyWindowGroupHeader()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
        }

        private static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            if (gameObject != null && gameObject.name.StartsWith("---", StringComparison.Ordinal))
            {
                EditorGUI.DrawRect(selectionRect, Color.gray);
                EditorGUI.DropShadowLabel(selectionRect, gameObject.name.Replace("-", "").ToUpperInvariant());
            }
        }
    }
}
#endif