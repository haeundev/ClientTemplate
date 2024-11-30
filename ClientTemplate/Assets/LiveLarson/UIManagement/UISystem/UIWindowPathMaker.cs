using System;
using System.Collections.Generic;
using LiveLarson.Plugins.UIs;
using UnityEditor;
using UnityEngine;

namespace LiveLarson.UIManagement.UISystem
{
    public class UIWindowPathMaker : MonoBehaviour
    {
#if UNITY_EDITOR
        public UIContainer containerSo;
        public List<UIContainer> joinContainerSOs;
        public List<UIWindow> windows = new();

        public void Collect()
        {
            if (containerSo == null) return;
            containerSo.uiList = new List<UIKeyValue>();
            foreach (var window in windows)
            {
                var uiKeyValue = new UIKeyValue();
                try
                {
                    uiKeyValue.Window = SetPrefab(window.gameObject, out uiKeyValue.Path);
                    if (FindKey(uiKeyValue.Window.GetType())) continue;
                    containerSo.uiList.Add(uiKeyValue);
                    window.gameObject.SetActive(false);
                }
                catch
                {
                    Debug.LogError("Checked Console");
                    break;
                }
            }

            EditorUtility.SetDirty(containerSo);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public void Join()
        {
            if (containerSo == null) return;
            if (joinContainerSOs == null) return;

            foreach (var container in joinContainerSOs)
            foreach (var joinKeyValue in container.uiList)
            {
                if (FindKey(joinKeyValue.Window.GetType())) continue;
                containerSo.uiList.Add(joinKeyValue);
            }

            EditorUtility.SetDirty(containerSo);
            AssetDatabase.SaveAssets();
        }

        public void CheckMissionComponent()
        {
            foreach (var uiWindow in windows)
                if (uiWindow == null)
                    Debug.LogError("UI Window Mission Component in [" + gameObject.name + "] Check UI Window");
        }

        private bool FindKey(Type type)
        {
            var index = 0;
            foreach (var keyValue in containerSo.uiList)
            {
                index++;
                if (keyValue.Window.GetType() == type)
                {
                    Debug.Log("***** 중첩된 UI Type 따라서 해당 UI는 컨테이너에 등록되지 않았습니다 : [Index : " + index + " ][ Type : " +
                              type + " ]");
                    return true;
                }
            }

            return false;
        }

        private UIWindow SetPrefab(GameObject windowObject, out string path)
        {
            Debug.Log("Window Path : " +
                      AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(windowObject)));
            path = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(windowObject));

            var FindPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(
                AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(windowObject)),
                typeof(GameObject));
            return FindPrefab.GetComponent<UIWindow>();
        }
#endif
    }


    [Serializable]
    public class UIKeyValue
    {
        public UIWindow Window;
        public string Path;
    }
}