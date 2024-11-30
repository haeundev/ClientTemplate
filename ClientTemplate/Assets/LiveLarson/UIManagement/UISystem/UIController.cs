using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LiveLarson.Plugins.UIs;
using LiveLarson.Util;
using UnityEngine;
using Type = System.Type;

namespace LiveLarson.UIManagement.UISystem
{
    public abstract class UIController
    {
        protected UIWindow _window;
        public int ID;

        public bool IsOpenedWindow;

        public readonly bool IsSceneChangedDestroyBody = true;

        protected Action<UIController> CompleteWindowSetting;

        protected bool IsLoadComplete;

        private bool _waitClose;

        protected UIController()
        {
            SetChildrenControllers();
        }

        public abstract Type UIWindowType { get; }
        public virtual UIType UIType => UIType.None;
        public Transform WindowTransform => _window.transform;
        public WindowOption WindowOption { get; private set; } = WindowOption.None;

        public bool LoadComplete => IsLoadComplete;

        public int SortingOrder
        {
            get
            {
                if (_window == null) return int.MaxValue;
                return _window.SortingOrder;
            }
        }

        public int SubWindowCount => m_childrenBodies.Count;


        public bool IsFullScreen { get; private set; }

        public virtual void SetWindowOption(int id, Action<UIController> completeWindowSetting, WindowOption option)
        {
            ID = id;
            CompleteWindowSetting = completeWindowSetting;
            WindowOption = option;
        }


        protected void CreateWindow<T>() where T : UIWindow
        {
            UIWindowManager.OpenWindow<T>(this, InitWindow, ID, SubWindowCount, WindowOption);
        }

        public virtual void InitWindow(UIWindow window)
        {
            _window = window;
            _window.SetSortingOrder();
            IsFullScreen = _window.isFullScreen;
            UIWindowManager.GetSubWindows(() =>
            {
                if (!_waitClose)
                {
                    Awake();
                    OnFullScreenWindow();
                }

                IsLoadComplete = true;
            }, this);
        }

        protected abstract void Awake();

        private void OnFullScreenWindow()
        {
            if (IsFullScreen)
                UIWindowManager.SetFullScreen(this);
        }

        public virtual void Hide()
        {
            if (_window == null) return;
            _window.gameObject.SetActive(false);
            OnHide();
        }

        public virtual void Show()
        {
            _window.gameObject.SetActive(true);
            OnShow();
        }

        public virtual void TemporaryHide()
        {
            _window.HideLowSortingOrder();
            OnHideByFullScreen();
        }

        public virtual void ReturnPrevOpenState()
        {
            _window.PrevVisibleState();
            if (_window.IsOpen)
                OnShowByFullScreen();
        }

        public virtual void Close()
        {
            UIWindowService.Close(this);
        }

        public void CloseAction()
        {
            _waitClose = true;
            if (IsLoadComplete)
            {
                foreach (var body in m_childrenBodies) body.CloseAction();
                OnDestroyAfterClose();
                _window.Close();
                OnClose();
            }
            else
            {
                CoroutineManager.ExecuteCoroutine(DelayClose());
            }
        }

        public void OnDestroyAfterClose()
        {
            UIWindowManager.CloseUIWindowBody(this);
        }

        private IEnumerator DelayClose()
        {
            yield return new WaitUntil(() => IsLoadComplete);
            CloseAction();
            _window.Close();
            OnClose();
        }

        #region Tree

        protected List<UIController> m_childrenBodies = new();
        public List<UIController> ChildrenBodies => m_childrenBodies;

        protected virtual void SetChildrenControllers()
        {
        }

        protected void AddChildBody(UIController body)
        {
            m_childrenBodies.Add(body);
        }

        #endregion

        #region Window Event

        public virtual void OnOpen()
        {
            if (IsLoadComplete)
                foreach (var body in m_childrenBodies)
                    body.OnOpen();
        }

        public virtual void OnClose()
        {
        }

        public virtual void OnShow()
        {
            if (IsLoadComplete)
                foreach (var body in m_childrenBodies)
                    if (body._window.IsOpen)
                        body.OnShow();

            IsOpenedWindow = true;
        }

        public virtual void OnHide()
        {
            if (IsLoadComplete)
                foreach (var body in m_childrenBodies)
                    if (body._window.IsOpen)
                        body.OnHide();

            IsOpenedWindow = false;
        }

        public virtual void OnDestroy()
        {
            if (IsLoadComplete)
                foreach (var body in m_childrenBodies)
                    body.OnDestroy();
            else
                CoroutineManager.ExecuteCoroutine(DelayOnDestroy());
        }

        private IEnumerator DelayOnDestroy()
        {
            yield return new WaitUntil(() => IsLoadComplete);
            foreach (var body in m_childrenBodies) body.OnDestroy();
        }

        public virtual void OnShowByFullScreen()
        {
            if (IsLoadComplete)
                foreach (var body in m_childrenBodies)
                    if (body._window.IsOpen)
                        body.OnShowByFullScreen();

            IsOpenedWindow = true;
        }

        public virtual void OnHideByFullScreen()
        {
            if (IsLoadComplete)
                foreach (var body in m_childrenBodies.Where(body => body._window.IsOpen))
                    body.OnHideByFullScreen();

            IsOpenedWindow = false;
        }

        #endregion
    }
}