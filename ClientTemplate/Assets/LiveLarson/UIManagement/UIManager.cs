using System;
using System.Collections.Generic;
using LiveLarson.Plugins.UIs;
using UnityEngine;
using Zenject;

namespace LiveLarson.UIManagement
{
    public class UIManager : MonoBehaviour
    {
        private Dictionary<Type, UIWindow> _instantiatedWindows = new Dictionary<Type, UIWindow>();
        private Dictionary<Type, UIWindow> _prefabDictionary = new Dictionary<Type, UIWindow>();

        [Inject]
        private void Construct(Dictionary<Type, UIWindow> prefabDict)
        {
            _prefabDictionary = prefabDict;
        }

        public void Open<T>() where T : UIWindow
        {
            var type = typeof(T);

            // Check if the window is already instantiated
            if (!_instantiatedWindows.TryGetValue(type, out var window))
            {
                // Instantiate from prefab if not already instantiated
                if (_prefabDictionary.TryGetValue(type, out var prefab))
                {
                    window = Instantiate(prefab, transform); // Parent to UIManager
                    _instantiatedWindows[type] = window;
                }
                else
                {
                    Debug.LogError($"No prefab registered for UIWindow of type {type}.");
                    return;
                }
            }

            window.Show();
        }

        public void Close<T>() where T : UIWindow
        {
            var type = typeof(T);

            if (_instantiatedWindows.TryGetValue(type, out var window))
            {
                window.Hide();
            }
            else
            {
                Debug.LogWarning($"UIWindow of type {type} is not currently instantiated.");
            }
        }
    }
}