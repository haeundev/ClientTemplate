#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LiveLarson.UIManagement.UISystem.Editor
{
    [CustomEditor(typeof(UISubWindowBody))]
    public class UISubWindowBodyEditor : UnityEditor.Editor
    {
        private UISubWindowBody bodyData = null;
        private float BasePlusButtonHeight = 22;
        private void OnEnable()
        {
            bodyData = target as UISubWindowBody;
        }
        
        public override void OnInspectorGUI()
        {
 
            Display(bodyData.nodes);
        }
        
        private void Display(List<UISubWindowBody.Node> nodes)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            foreach (var node in nodes)
            {
                if (NodeBox(node)) break;
            }
            EditorGUILayout.EndVertical();
            if (GUILayout.Button("Add", GUILayout.Width(100),GUILayout.Height(BasePlusButtonHeight * (nodes.Count+1) )))
            {
                bodyData.CreateNewNode();
                //    isButtonClick = true;
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private bool NodeBox(UISubWindowBody.Node node)
        {
            var isButtonClick = false;
            EditorGUILayout.BeginVertical("Box");
            {
                EditorGUILayout.BeginHorizontal();
                {
                    node.targetScript =
                       EditorGUILayout.ObjectField(node.targetScript, typeof(MonoScript), true);
                    
                    if (GUILayout.Button("-",GUILayout.Width(20),GUILayout.Height(20)))
                    {
                        bodyData.DeleteNode(node);
                        isButtonClick = true;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            return isButtonClick;
        }
    }

}
#endif