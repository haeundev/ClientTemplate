using System;
using System.Collections.Generic;
using LiveLarson.Plugins.UIs;
using UnityEngine;

namespace LiveLarson.UIManagement.UISystem
{
    public class UIWindowContainer : MonoBehaviour
    {
        [SerializeField] private UIContainer uiContainer;

        private Dictionary<Type, string> _pathContainer;


        private void Awake()
        {
            InitContainer();
        }

        public string Find<T>() where T : UIWindow
        {
            _pathContainer.TryGetValue(typeof(T), out var windowName);
            return windowName;
        }

        public string Find(Type type)
        {
            _pathContainer.TryGetValue(type, out var windowName);
            return windowName;
        }

        private void InitContainer()
        {
            _pathContainer = new Dictionary<Type, string>();

            var index = 0;
            try
            {
                foreach (var uiKeyValue in uiContainer.uiList)
                {
                    index++;
                    _pathContainer[uiKeyValue.Window.GetType()] = uiKeyValue.Path;
                }
            }
            catch
            {
                Debug.LogError("UIContainer Error Index : " + index);
            }
        }
    }
}