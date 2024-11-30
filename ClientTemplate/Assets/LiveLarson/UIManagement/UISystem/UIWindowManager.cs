using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LiveLarson.CameraManagement;
using LiveLarson.Plugins.UIs;
using LiveLarson.Util;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Type = System.Type;

namespace LiveLarson.UIManagement.UISystem
{
    public class UIBodyContainer
    {
        public Dictionary<int, UIWindow> Windows;

        public UIBodyContainer()
        {
            Windows = new Dictionary<int, UIWindow>();
        }

        public bool IsHasWindow(int id)
        {
            return Windows.ContainsKey(id);
        }

        public UIWindow GetWindow(int id)
        {
            return Windows.GetValueOrDefault(id);
        }

        public void RegisterWindow(int id, UIWindow window)
        {
            Windows.TryAdd(id, window);
        }

        public int RemoveWindow(int id)
        {
            Windows.Remove(id);
            return Windows.Count;
        }
    }

    public class UIWindowManager : MonoBehaviour, IUIWindowManager
    {
        public UIWindowContainer uiContainer;

        [SerializeField] private Material grayscaleMaterial;
        private readonly Dictionary<Type, Dictionary<int, List<Action<UIWindow>>>> _createAction = new();

        private readonly Dictionary<Type, UIBodyContainer> _createdWindow = new();

        private readonly List<UIWindow> _mCreatedWindow = new();
        public static UIWindowManager Instance { get; private set; }

        public static Material GrayscaleMaterial => Instance.grayscaleMaterial;

        public static float DefaultPlaneDistance => 30;

        private void Awake()
        {
            Instance = this;
            UIWindow.manager = this;
            InitPool();
        }

        #region SetCamera

        public static void ResetUICameraAllUIWindowInCanvas()
        {
            foreach (var window in Instance._mCreatedWindow) Instance.SetUICamera(window);
        }

        #endregion


        #region Open

        public static void OpenWindow<T>(UIController body, Action<T> openAction = null, int windowID = 0,
            int chileCanvasCount = 0, WindowOption option = WindowOption.None) where T : UIWindow
        {
            var isAlreadyWindow = false;
            if (Instance._createdWindow.ContainsKey(body.GetType()))
                isAlreadyWindow = Instance._createdWindow[body.GetType()].IsHasWindow(windowID);
            else
                Instance._createdWindow[body.GetType()] = new UIBodyContainer();

            if (isAlreadyWindow)
                openAction?.Invoke(Instance._createdWindow[body.GetType()].GetWindow(windowID) as T);
            else
                Instance.StartCoroutine(Instance.CreateWindow<T>(window =>
                {
                    var isKeepSortingOrder = option.HasFlag(WindowOption.KeepSortingOrder);
                    Instance.GetSortingOrder(window, chileCanvasCount, isKeepSortingOrder);
                    Instance._createdWindow.GetOrCreate(body.GetType())
                        .RegisterWindow(windowID, window);

                    openAction?.Invoke(window as T);
                }, windowID, option));
        }

        #endregion

        #region Sorting

        private int _lastSortingOrder = 1;
        private const int AddSortingOrder = 10;

        /// <summary>
        ///     생성된 창들에서 최대 sorting order 값을 찾아 반환합니다. Legacy 매니저로 생성된 창들은 포함하지 않습니다.
        /// </summary>
        /// <param name="includeKeepSortingOrderWindows">고정 sorting order 값으로 열린 창을 포함하는지 여부</param>
        /// <returns>최대 sorting order 값</returns>
        public int GetMaxSortingOrder(bool includeKeepSortingOrderWindows)
        {
            if (!_mCreatedWindow.Any())
                return 0;

            return includeKeepSortingOrderWindows
                ? _mCreatedWindow.Max(window => window.SortingOrder)
                : _mCreatedWindow.Where(window => !window.IsKeepSortingOrderWindow).Max(window => window.SortingOrder);
        }

        private delegate void ReleaseSortingOrderDelegate(int standardSortingOrder, int order);

        private event ReleaseSortingOrderDelegate ReleaseSortingOrderEvent;

        private void GetSortingOrder(UIWindow window, int chileCanvasCount = 0, bool isKeepSortingOrder = false)
        {
            window.IsKeepSortingOrderWindow = isKeepSortingOrder;
            if (isKeepSortingOrder) return;
            window.SortingOrder = _lastSortingOrder;
            _lastSortingOrder += chileCanvasCount + AddSortingOrder;
            ReleaseSortingOrderEvent += window.UpdateCanvasSortingOrder;
        }

        public static void ReleaseSortingOrder(UIController body)
        {
            if (body.WindowOption.HasFlag(WindowOption.KeepSortingOrder))
                return;

            Instance._lastSortingOrder -= body.SubWindowCount + AddSortingOrder;
            if (Instance._lastSortingOrder < 1) Instance._lastSortingOrder = 1;
            if (Instance.ReleaseSortingOrderEvent != null)
                Instance.ReleaseSortingOrderEvent(body.SortingOrder, body.SubWindowCount);
        }

        public static void ResetSortingOrder()
        {
            Instance._lastSortingOrder = 1;
            Instance.ReleaseSortingOrderEvent = null;
        }

        #endregion

        #region Close

        public void Close(UIWindow window)
        {
            CloseWindow(window);
        }

        public void DestroyWindow(UIWindow window)
        {
            ReleaseSortingOrderEvent -= window.UpdateCanvasSortingOrder;
            if (_mCreatedWindow.Contains(window)) _mCreatedWindow.Remove(window);
        }

        public static void CloseWindow(UIWindow window)
        {
            if (window.Nullable()?.gameObject == null) return;
            Destroy(window.gameObject);
        }

        public static void CloseUIWindowBody(UIController body)
        {
            if (Instance._createdWindow.ContainsKey(body.GetType()))
            {
                var createdBodyInWindowCount = Instance._createdWindow[body.GetType()].RemoveWindow(body.ID);
                if (createdBodyInWindowCount <= 0) Instance._createdWindow.Remove(body.GetType());
            }

            ReleaseSortingOrder(body);
            CloseFullScreen(body);
        }

        #endregion

        #region Tree Open

        public static void GetSubWindows(Action onComplete, UIController root)
        {
            Instance.StartCoroutine(Instance.GetTreeWindow(onComplete, root));
        }

        private IEnumerator GetTreeWindow(Action onComplete, UIController root)
        {
            var addSortingOrder = 0;
            foreach (var body in root.ChildrenBodies)
            {
                yield return CreateSubWindow(body.UIWindowType, body.InitWindow, root.SortingOrder + addSortingOrder,
                    root.WindowTransform, root.WindowOption);
                yield return GetTreeWindow(() => { }, body);
                addSortingOrder++;
            }

            onComplete?.Invoke();
        }

        #endregion

        #region Create Window

        private const float InstantiateWaitTime = 5f;

        public static string GetUIPath<T>() where T : UIWindow
        {
            return Instance.uiContainer.Find<T>();
        }

        private static string GetUIPath(Type type)
        {
            return Instance.uiContainer.Find(type);
        }

        private IEnumerator CreateWindow<T>(Action<UIWindow> resultWindow, int id,
            WindowOption option = WindowOption.None) where T : UIWindow
        {
            var requestTime = Time.deltaTime;
            Debug.Log($"[New] Create Window Request [Type] : {typeof(T)} - Request Time : {requestTime}");
            if (_createAction.ContainsKey(typeof(T)))
            {
                if (_createAction[typeof(T)].ContainsKey(id))
                {
                    _createAction[typeof(T)][id].Add(resultWindow);
                    yield break;
                }
            }
            else
            {
                _createAction[typeof(T)] = new Dictionary<int, List<Action<UIWindow>>>();
            }

            _createAction[typeof(T)][id] = new List<Action<UIWindow>> { resultWindow };

            var path = GetUIPath<T>();
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("Don't Created Window Because Path is null or empty");
                yield break;
            }

            var currentWaitTime = 0f;
            var windowCreate = false;

            UIWindow window = null;


            Addressables.LoadAssetAsync<GameObject>(path).Completed += op =>
            {
                Debug.Log("[UI Window] Complete load Async UI - path: " + path);
                var windowObject = Instantiate(op.Result, transform);
                window = windowObject.GetComponent<UIWindow>();
                window.gameObject.SetActive(false);
                window.windowOption = option;
                window.AddressableHandle = op;
                windowCreate = true;
            };


            while (!windowCreate)
            {
                currentWaitTime += Time.deltaTime;
                yield return YieldInstructionCache.WaitForEndOfFrame;
                if (currentWaitTime > InstantiateWaitTime)
                {
                    Debug.LogError("[New]Open UIWindow very long delay [Type] : " + typeof(T) + "RequestTime : " +
                                   requestTime
                                   + " delay Time :" + Time.time + "Delta :" + (Time.time - requestTime));
                    ShowDelayLoading();
                }
            }

            _mCreatedWindow.Add(window);
            SetUICamera(window);
            window.gameObject.SetActive(false);
            foreach (var windowAction in _createAction[typeof(T)][id]) windowAction?.Invoke(window);
            _createAction[typeof(T)].Remove(id);
            if (_createAction[typeof(T)].Count <= 0) _createAction.Remove(typeof(T));
            CloseDelayLoading();
        }

        private IEnumerator CreateSubWindow(Type type, Action<UIWindow> resultWindow, int sortingOrder,
            Transform parent = null, WindowOption option = WindowOption.None)
        {
            var requestTime = Time.deltaTime;
            Debug.Log(string.Format("[New - sub] Create Sub Window Request [Type] : {0} - Request Time : {1}", type,
                requestTime));
            var path = GetUIPath(type);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("Don't Created Window Because Path is null or empty");
                yield break;
            }

            var currentWaitTime = 0f;
            var windowCreate = false;

            UIWindow window = null;

            Addressables.LoadAssetAsync<GameObject>(path).Completed += op =>
            {
                var windowObject = Instantiate(op.Result, transform);
                window = windowObject.GetComponent<UIWindow>();
                window.gameObject.transform.SetParent(parent, false);
                window.gameObject.SetActive(false);
                window.SortingOrder = sortingOrder;
                window.isFullScreen = false;
                window.AddressableHandle = op;
                if ((option & WindowOption.KeepSortingOrder) == 0)
                    ReleaseSortingOrderEvent += window.UpdateCanvasSortingOrder;

                if (parent != null && parent.GetComponent<Canvas>() != null &&
                    window.GetComponent<Canvas>() != null)
                {
                    RemoveCanvas(window);
                    SetAnchor(window.GetComponent<RectTransform>());
                }

                windowCreate = true;
            };

            while (!windowCreate)
            {
                currentWaitTime += Time.deltaTime;
                yield return YieldInstructionCache.WaitForEndOfFrame;
                if (currentWaitTime > InstantiateWaitTime)
                {
                    // Debug.LogError("It's been too long since the UI was instantiate. So stooped instancing the UI \n" +
                    //                " [Solution] Check the UI Path or Ui Window Manager.cs Change instantiate Wait Time. \n " +
                    //                " [Path] : " + path + "/n limit Time : "+ m_instantiateWaitTime);
                    Debug.LogError("[New - sub] Open UIWindow very long delay [Type] : " + type + "RequestTime : " +
                                   requestTime
                                   + " delay Time :" + Time.time + "Delta :" + (Time.time - requestTime));
                    ShowDelayLoading();
                }
            }

            _mCreatedWindow.Add(window);
            SetUICamera(window);
            window.gameObject.SetActive(false);
            resultWindow?.Invoke(window);
            CloseDelayLoading();
        }

        private void RemoveCanvas(UIWindow window)
        {
            var windowObject = window.transform;

            var raycaster = windowObject.GetComponent<GraphicRaycaster>();
            if (raycaster != null) Destroy(raycaster);
            var canvasScaler = windowObject.GetComponent<CanvasScaler>();
            if (canvasScaler != null) Destroy(canvasScaler);
            var canvas = windowObject.GetComponent<Canvas>();
            if (canvas != null) Destroy(canvas);
        }

        private void SetAnchor(RectTransform rectTr)
        {
            rectTr.anchorMax = Vector2.one;
            rectTr.anchorMin = Vector2.zero;
            rectTr.pivot = new Vector2(0.5f, 0.5f);
            rectTr.localScale = Vector3.one;
            rectTr.offsetMax = Vector2.zero;
            rectTr.offsetMin = Vector2.zero;
        }


        private void SetUICamera(UIWindow window)
        {
            if (CameraManager.UI == null)
                return;

            if (!window) return;
            if (!window.isUsedUICamera) return;
            var canvas = window.GetComponent<Canvas>();
            if (canvas == null) return;

            if (window.gameObject.tag.Equals("CampOverlay")) return;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = CameraManager.UI;
            canvas.planeDistance = DefaultPlaneDistance;
        }

        #endregion

        #region UI Sub Pool

        private Transform _pool;

        private readonly List<Transform> _subWindows = new();


        private void InitPool()
        {
            if (_pool != null) return;
            var emptyObject = new GameObject();
            emptyObject.transform.SetParent(transform);
            emptyObject.name = "Sub Window Pool";
            _pool = emptyObject.transform;
        }

        public void InitSubWindow(Transform window)
        {
            Instance._subWindows.Add(window);
            window.SetParent(Instance._pool, false);
        }

        public void ReleaseSubWindow(Transform window)
        {
            if (window == null) return;
            if (_subWindows.Contains(window)) _subWindows.Remove(window);
            Destroy(window.gameObject);
        }

        #endregion

        #region Full Screen

        public static bool IsOpenedFullScreenWindow =>
            Instance._mFullScreenWindowBody.Count > 0
            || Instance._mLastOpenedFullScreenWindow != null;

        private readonly List<UIController> _mLowSoringOrderBodies = new();
        private readonly List<UIController> _mFullScreenWindowBody = new();


        public static void SetFullScreen(UIController body)
        {
            if (!Instance._mFullScreenWindowBody.Contains(body))
                Instance._mFullScreenWindowBody.Add(body);
            Instance.FoundLowSortingOrderBodiesAndHide(body.SortingOrder);
            SetFullScreenOption(body);
        }

        private static void SetFullScreenOption(UIController body)
        {
            CullingCameraMask();
            PauseForFullScreen();
        }

        public static void CloseFullScreen(UIController closeBody)
        {
            if (Instance._mFullScreenWindowBody.Contains(closeBody)) Instance._mFullScreenWindowBody.Remove(closeBody);

            if (Instance._mLowSoringOrderBodies.Contains(closeBody)) Instance._mLowSoringOrderBodies.Remove(closeBody);

            if (Instance._mLastOpenedFullScreenWindow == null)
            {
                if (Instance._mFullScreenWindowBody.Count <= 0 && Instance._mHideFullScreenWindows.Count <= 0)
                {
                    RestoreCullingCameraMask();
                    foreach (var body in Instance._mLowSoringOrderBodies) body.ReturnPrevOpenState();

                    Instance._mLowSoringOrderBodies.Clear();
                    ShowPrevNotFullScreenWindows();
                    ResumeForFullScreen();
                }
                else
                {
                    Instance.FindLowUIWindow();
                }
            }
        }

        private void FoundLowSortingOrderBodiesAndHide(int fullScreenSortingOrder)
        {
            if (_mLastOpenedFullScreenWindow != null)
            {
                var canvas = _mLastOpenedFullScreenWindow.GetComponent<Canvas>();
                if (canvas.sortingOrder < fullScreenSortingOrder)
                {
                    _mLastOpenedFullScreenWindow.HideLowSortingOrder();
                    if (!Instance._mHideFullScreenWindows.Contains(_mLastOpenedFullScreenWindow))
                        Instance._mHideFullScreenWindows.Add(_mLastOpenedFullScreenWindow);
                    _mLastOpenedFullScreenWindow = null;
                }
            }

            foreach (var body in UIWindowService.CreatedControllers)
                if (body.SortingOrder < fullScreenSortingOrder)
                    if (!Instance._mLowSoringOrderBodies.Contains(body))
                    {
                        _mLowSoringOrderBodies.Add(body);
                        body.TemporaryHide();
                    }
        }

        #region Camera Mask Culling

        private int _mMainCameraPrevCullingMask;

        private static int MainCameraPrevCullingMask
        {
            get => Instance._mMainCameraPrevCullingMask;
            set => Instance._mMainCameraPrevCullingMask = value;
        }

        private bool _mIsCulling;

        private static bool IsCulling
        {
            get => Instance._mIsCulling;
            set => Instance._mIsCulling = value;
        }

        private static void CullingCameraMask()
        {
            if (IsCulling) return;
            MainCameraPrevCullingMask = CameraManager.Main.cullingMask;
            CameraManager.Main.cullingMask = 0;
            IsCulling = true;
        }

        private static void RestoreCullingCameraMask()
        {
            if (!IsCulling) return;
            CameraManager.Main.cullingMask = MainCameraPrevCullingMask;
            MainCameraPrevCullingMask = 0;
            IsCulling = false;
        }

        #endregion

        #region Pause

        private bool _isPause;

        private static bool IsPause
        {
            get => Instance._isPause;
            set => Instance._isPause = value;
        }

        public static void PauseForFullScreen()
        {
            if (IsPause) return;
            // GamePauseManager.Pause(Pause.FullScreen);
            IsPause = true;
        }

        public static void ResumeForFullScreen()
        {
            if (!IsPause) return;
            // GamePauseManager.Resume(Pause.FullScreen);
            IsPause = false;
        }

        #endregion

        #region Lagacy Manager FullScreen Controll

        private readonly List<UIWindow> _mLowSortingOrderWindow = new();
        private readonly List<UIWindow> _mHideFullScreenWindows = new();

        private UIWindow _mLastOpenedFullScreenWindow;

        public static void ShowFullScreenWindow(UIWindow window)
        {
            var canvas = window.GetComponent<Canvas>();
            Instance.FoundLowSortingOrderBodiesAndHide(canvas.sortingOrder);
            Instance._mLastOpenedFullScreenWindow = window;
        }

        public static void LowSortOrderWindow(UIWindow window)
        {
            window.HideLowSortingOrder();
            if (window.isFullScreen)
            {
                if (Instance._mHideFullScreenWindows.Contains(window)) return;
                Instance._mHideFullScreenWindows.Add(window);
            }
            else
            {
                if (Instance._mLowSortingOrderWindow.Contains(window)) return;
                Instance._mLowSortingOrderWindow.Add(window);
            }
        }

        public static void ShowPrevNotFullScreenWindows()
        {
            foreach (var window in Instance._mLowSortingOrderWindow) window.Nullable()?.PrevVisibleState();
            Instance._mLowSortingOrderWindow.Clear();
        }

        public static void CloseFullScreenWindow(UIWindow window)
        {
            if (Instance._mHideFullScreenWindows.Contains(window)) Instance._mHideFullScreenWindows.Remove(window);

            if (Instance._mLowSortingOrderWindow.Contains(window)) Instance._mLowSortingOrderWindow.Remove(window);

            if (Instance._mLastOpenedFullScreenWindow == window) Instance._mLastOpenedFullScreenWindow = null;

            CloseFullScreen(null);
        }

        private void FindLowUIWindow()
        {
            var min = int.MaxValue;
            UIController selectBody = null;
            foreach (var body in _mFullScreenWindowBody)
                if (body.SortingOrder < min)
                {
                    selectBody = body;
                    min = body.SortingOrder;
                }

            min = int.MaxValue;
            UIWindow window = null;
            foreach (var fullScreenWindow in _mHideFullScreenWindows)
            {
                var canvas = fullScreenWindow.GetComponent<Canvas>();
                if (canvas.sortingOrder < min)
                {
                    window = fullScreenWindow;

                    min = canvas.sortingOrder;
                }
            }

            if (selectBody != null && selectBody.SortingOrder > min)
            {
                var lastFullScreenBody = Instance._mFullScreenWindowBody.Last();
                SetFullScreenOption(lastFullScreenBody);
                lastFullScreenBody.ReturnPrevOpenState();
            }
            else if (window != null)
            {
                window.PrevVisibleState();
                ResumeForFullScreen();
            }
        }

        #endregion

        #endregion

        #region Delay Loading

        private Coroutine _delayCoroutine;
        private bool _isDelayShow;

        private void ShowDelayLoading()
        {
            if (_delayCoroutine != null)
            {
                StopCoroutine(_delayCoroutine);
                _delayCoroutine = null;
                _isDelayShow = false;
            }

            _delayCoroutine = StartCoroutine(DelayLoadingTwinkle());
        }

        private IEnumerator DelayLoadingTwinkle()
        {
            if (!_isDelayShow) _isDelayShow = true;
            // ToastLoading.Instance.Show();
            // ToastLoading.Instance.Open("잠시 기다려 주세요");
            yield return YieldInstructionCache.WaitForSeconds(3.5f);
            _isDelayShow = false;
            // ToastLoading.Instance.Hide();
            yield return YieldInstructionCache.WaitForSeconds(5f);
            if (_createAction.Count > 0) ShowDelayLoading();
        }

        private void CloseDelayLoading()
        {
            if (_createAction.Count < 0)
                if (_delayCoroutine != null)
                {
                    StopCoroutine(_delayCoroutine);
                    _delayCoroutine = null;
                    _isDelayShow = false;
                }
            // ToastLoading.Instance.Hide();
        }

        #endregion
    }
}