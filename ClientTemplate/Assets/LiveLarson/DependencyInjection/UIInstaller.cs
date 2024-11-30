using System;
using System.Collections.Generic;
using LiveLarson.UIManagement;
using UnityEngine;
using Zenject;

namespace LiveLarson.DependencyInjection
{
    public class UIInstaller : MonoInstaller
    {
        [SerializeField] private List<UIPrefabEntry> uiPrefabs;

        public override void InstallBindings()
        {
            // Create a dictionary of prefab types to prefabs
            var prefabDictionary = new Dictionary<Type, UIWindow>();
            foreach (var entry in uiPrefabs)
            {
                var type = entry.prefab.GetType();
                if (!prefabDictionary.ContainsKey(type))
                    prefabDictionary[type] = entry.prefab;
                else
                    Debug.LogWarning($"Duplicate UI prefab of type {type} detected.");
            }

            // Bind the prefab dictionary to the container
            Container.Bind<Dictionary<Type, UIWindow>>().FromInstance(prefabDictionary).AsSingle();

            // Bind UIManager
            Container.Bind<UIManager>().FromComponentInHierarchy().AsSingle();
        }

        [Serializable]
        private struct UIPrefabEntry
        {
            public string windowName; // For editor-friendly naming
            public UIWindow prefab; // Prefab reference
        }
    }
}