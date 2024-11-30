using LiveLarson.UIManagement;
using LiveLarson.UIManagement.UISystem;
using UnityEngine;
using Zenject;
using System;
using System.Collections.Generic;
using LiveLarson.Plugins.UIs;

namespace LiveLarson.DependencyInjection
{
    public class UIInstaller : MonoInstaller
    {
        [SerializeField] private UIContainer uiContainer;

        public override void InstallBindings()
        {
            // Bind the UIManager instance in the scene
            Container.Bind<UIManager>().FromComponentInHierarchy().AsSingle().NonLazy();

            // Bind the prefab dictionary
            Container.Bind<Dictionary<Type, UIWindow>>()
                .FromMethod(CreateUIPrefabDictionary)
                .AsSingle();

            // Bind the UIContainer
            Container.BindInstance(uiContainer).AsSingle();
        }

        private Dictionary<Type, UIWindow> CreateUIPrefabDictionary()
        {
            var prefabDict = new Dictionary<Type, UIWindow>();

            foreach (var uiKeyValue in uiContainer.uiList)
            {
                if (uiKeyValue.Window is UIWindow uiWindow)
                {
                    var type = uiWindow.GetType();
                    if (!prefabDict.TryAdd(type, uiWindow))
                    {
                        Debug.LogWarning($"Duplicate UIWindow type detected: {type}. Skipping.");
                    }
                }
                else
                {
                    Debug.LogWarning($"Invalid UIWindow in UIContainer: {uiKeyValue.Path}");
                }
            }

            return prefabDict;
        }
    }
}