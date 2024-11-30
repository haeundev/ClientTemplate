using System;
using System.Collections.Generic;
using System.Linq;
using LiveLarson.Plugins.UIs;
using UnityEngine;
using UnityEngine.UI;

namespace LiveLarson.UIManagement.UISystem
{
    public class UIControllerContainer
    {
        public readonly Dictionary<int, UIController> Controllers;
        public UIType UIType = UIType.None;

        public UIControllerContainer()
        {
            Controllers = new Dictionary<int, UIController>();
        }

        public bool IsHasBodies(int id)
        {
            return Controllers.ContainsKey(id);
        }

        public UIController GetBody(int id)
        {
            return Controllers.GetValueOrDefault(id);
        }

        public void RegisterBody(int id, UIController body)
        {
            if (Controllers.TryAdd(id, body))
            {
                UIType = body.UIType;
                UIWindowService.CreatedControllers.Add(body);
            }
        }

        public int RemoveBody(int id)
        {
            if (Controllers.ContainsKey(id))
            {
                UIWindowService.CreatedControllers.Remove(Controllers[id]);
                Controllers.Remove(id);
            }

            return Controllers.Count;
        }

        public List<UIController> CloseAll()
        {
            var bodiesList = new List<UIController>();
            foreach (var windowBody in Controllers) bodiesList.Add(windowBody.Value);
            return bodiesList;
        }

        public void HideAll()
        {
            foreach (var windowBody in Controllers) windowBody.Value.Hide();
        }

        public void ShowAll()
        {
            foreach (var windowBody in Controllers) windowBody.Value.Show();
        }
    }

    public static class UIWindowService
    {
        public static readonly List<UIController> CreatedControllers = new();
        public static readonly Dictionary<Type, UIControllerContainer> CreatedControllerContainer = new();

        public static void OpenWindow<T>(Action<T> receivewindowBody = null, int id = 0,
            WindowOption option = WindowOption.None)
            where T : UIController, new()
        {
            var isAlreadyBody = false;
            if (CreatedControllerContainer.ContainsKey(typeof(T)))
                isAlreadyBody = CreatedControllerContainer[typeof(T)].IsHasBodies(id);
            else
                CreatedControllerContainer[typeof(T)] = new UIControllerContainer();

            if (isAlreadyBody)
            {
                var body = CreatedControllerContainer[typeof(T)].GetBody(id);
                receivewindowBody?.Invoke(body as T);
                body.OnOpen();
            }

            var windowController = new T();
            CreatedControllerContainer[typeof(T)].RegisterBody(id, windowController);
            windowController.SetWindowOption(id, body =>
            {
                receivewindowBody?.Invoke(body as T);
                body.OnOpen();
            }, option);
        }

        public static void GetWindow<T>(Action<T> windowBody = null, int id = 0,
            WindowOption option = WindowOption.None)
            where T : UIController, new()
        {
            if (CreatedControllerContainer.ContainsKey(typeof(T)))
            {
                if (CreatedControllerContainer[typeof(T)].IsHasBodies(id))
                {
                    var body = CreatedControllerContainer[typeof(T)].GetBody(id);
                    windowBody?.Invoke(body as T);
                }
            }
            else
            {
                OpenWindow(windowBody, id, option);
            }
        }

        public static T GetUIController<T>(int id = 0) where T : UIController
        {
            if (CreatedControllerContainer.ContainsKey(typeof(T)))
            {
                var getBody = CreatedControllerContainer[typeof(T)].GetBody(id);
                return getBody as T;
            }

            return null;
        }

        public static string GetPath<T>() where T : UIWindow
        {
            if (UIWindowManager.Instance)
                return UIWindowManager.GetUIPath<T>();
            return null;
        }

        public static bool IsOpenedController<T>(int id = 0) where T : UIController
        {
            if (CreatedControllerContainer.ContainsKey(typeof(T)))
            {
                var getBody = CreatedControllerContainer[typeof(T)].GetBody(id);
                return getBody.IsOpenedWindow;
            }

            return false;
        }

        public static void Close<T>(int id = 0) where T : UIController
        {
            if (CreatedControllerContainer.ContainsKey(typeof(T)))
            {
                var getBody = CreatedControllerContainer[typeof(T)].GetBody(id);
                if (getBody != null)
                {
                    getBody.CloseAction();
                    var bodyCount = CreatedControllerContainer[typeof(T)].RemoveBody(id);

                    if (bodyCount <= 0) CreatedControllerContainer.Remove(typeof(T));
                    getBody.OnDestroy();
                }
            }
        }

        public static void Close(UIController controller)
        {
            if (CreatedControllerContainer.ContainsKey(controller.GetType()))
            {
                var getBody = CreatedControllerContainer[controller.GetType()].GetBody(controller.ID);
                if (getBody != null)
                {
                    getBody.CloseAction();
                    var bodyCount = CreatedControllerContainer[controller.GetType()].RemoveBody(controller.ID);

                    if (bodyCount <= 0) CreatedControllerContainer.Remove(controller.GetType());
                    getBody.OnDestroy();
                }
            }
        }

        public static void CloseByType(UIType uiType)
        {
            if (uiType == UIType.None)
                return; // none 타입은 끄지 말 것 !

            var closeTargetBodies = new List<UIController>();

            foreach (var container in CreatedControllerContainer)
                if (container.Value.UIType == uiType)
                {
                    var bodiesList = container.Value.CloseAll();
                    for (var i = 0; i < bodiesList.Count; i++) closeTargetBodies.Add(bodiesList[i]);
                }

            for (var i = 0; i < closeTargetBodies.Count; i++) Close(closeTargetBodies[i]);
        }

        public static void CloseAll()
        {
            var closeTargetBodies = new List<UIController>();

            foreach (var container in CreatedControllerContainer)
            {
                var bodiesList = container.Value.CloseAll();
                for (var i = 0; i < bodiesList.Count; i++) closeTargetBodies.Add(bodiesList[i]);
            }

            for (var i = 0; i < closeTargetBodies.Count; i++) Close(closeTargetBodies[i]);
        }

        public static void ShowWindow<T>(int id = 0) where T : UIController
        {
            if (CreatedControllerContainer.ContainsKey(typeof(T)))
            {
                var getBody = CreatedControllerContainer[typeof(T)].GetBody(id);
                getBody.Show();
            }
        }

        public static void HideWindow<T>(int id = 0) where T : UIController
        {
            if (CreatedControllerContainer.ContainsKey(typeof(T)))
            {
                var getBody = CreatedControllerContainer[typeof(T)].GetBody(id);
                getBody.Hide();
            }
        }

        public static void ChangeStartScene()
        {
            UIWindowManager.ResetSortingOrder();
            DestroyChangeSceneDestroyController();
        }

        public static void ChangeCompleteScene()
        {
            UIWindowManager.ResetUICameraAllUIWindowInCanvas();
        }

        private static void DestroyChangeSceneDestroyController()
        {
            var DestroyWindow = new List<UIController>();
            foreach (var controller in CreatedControllers)
                if (controller.IsSceneChangedDestroyBody)
                    DestroyWindow.Add(controller);

            for (var i = 0; i < DestroyWindow.Count; i++) Close(DestroyWindow[i]);
        }

        public static UIController Find(Func<UIController, bool> func)
        {
            return CreatedControllers.FirstOrDefault(func);
        }
    }

    public static class UIExtensions
    {
        [Flags]
        public enum GrayscaleApplyingOption
        {
            None = 0,
            IncludeInactive = 1 << 0,
            UseKeptTargets = 1 << 1
        }

        [Flags]
        public enum GrayscaleUnapplyingOption
        {
            None = 0,
            KeepTargets = 1 << 0
        }

        public static void SetGray(this Image image, bool isSet)
        {
            image.material = isSet ? UIWindowManager.GrayscaleMaterial : null;
        }

        public static void ApplyGrayscaleIncludingChildren(this GameObject gameObject,
            GrayscaleApplyingOption option = GrayscaleApplyingOption.IncludeInactive)
        {
            GrayscaleSetter grayscaleSetter;

            if (option.HasFlag(GrayscaleApplyingOption.UseKeptTargets))
            {
                grayscaleSetter = gameObject.GetComponent<GrayscaleSetter>();
                if (!grayscaleSetter)
                {
                    grayscaleSetter = gameObject.AddComponent<GrayscaleSetter>();
                    grayscaleSetter.ClearAllLists();
                    grayscaleSetter.CollectLayoutElementsAndApplyGrayscaleIncludingChildren(option);
                    return;
                }

                grayscaleSetter.ApplyGrayscaleToLayoutElementsListedPreviously(option);
                return;
            }

            grayscaleSetter = gameObject.GetComponent<GrayscaleSetter>();

            if (!grayscaleSetter)
            {
                grayscaleSetter = gameObject.AddComponent<GrayscaleSetter>();
                grayscaleSetter.ClearAllLists();
            }

            grayscaleSetter.CollectLayoutElementsAndApplyGrayscaleIncludingChildren(option);
        }

        public static void UnapplyGrayscaleIncludingChildren(this GameObject gameObject,
            GrayscaleUnapplyingOption option = GrayscaleUnapplyingOption.None)
        {
            var grayscaleSetter = gameObject.GetComponent<GrayscaleSetter>();

            if (!grayscaleSetter)
                return;

            grayscaleSetter.UnapplyGrayscaleIncludingChildren(option);
        }
    }
}