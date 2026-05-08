using FairyGUI;
using GameMaker;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CaiFuZhiJia_3997
{
    public class PopupGameLoading : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiJia";
        public new const string resName = "PopupGameLoading";

        private const string SpinePrefabsPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupGameLoading/SpinePrefabs/";

        // 初始化
        private int _totalResCount = -1;
        private bool _isInitialized = false;
        // private bool _isFirstOpen = true;
        // private const float Duration = 8f;
        // private GTweener _loadingGTween;

        private GSlider _loadingBar;
        private GameObject _traderObj, _gameTitleObj;
        private GameObject _cloneTraderObj, _cloneGameTitleObj;
        private GComponent _compareTrader, _compareGameTitle;
        
        
        private int _preloadTotal;
        private int _preloadCompleted;
        /// <summary>从进入并行预加载起算，界面至少展示此时长（秒）；预加载更久则按实际结束。</summary>
        private const float MinLoadingDisplaySeconds = 5f;

        private float _preloadStartRealtime;
        private TimerCallback _pendingMinDisplayCallback;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            LoadResAsync();
        }

        public override void InitParam()
        {
            if (!_isInitialized) return;
            
            // 初始化进度条Slider
            _loadingBar = contentPane.GetChild("sliderLoading").asSlider;
            
            // 绑定预制体到UI锚点
            GComponent currentCom = contentPane.GetChild("anchorTrader").asCom;
            if (_compareTrader != currentCom)
            {
                _cloneTraderObj = Object.Instantiate(_traderObj);
                GameCommon.FguiUtils.DeleteWrapper(_compareTrader);
                _compareTrader = currentCom;
                GameCommon.FguiUtils.AddWrapper(_compareTrader, _cloneTraderObj);
            }

            currentCom = contentPane.GetChild("anchorGameTitle").asCom;
            if (currentCom != _compareGameTitle)
            {
                _cloneGameTitleObj = Object.Instantiate(_gameTitleObj);
                GameCommon.FguiUtils.DeleteWrapper(_compareGameTitle);
                _compareGameTitle = currentCom;
                GameCommon.FguiUtils.AddWrapper(_compareGameTitle, _cloneGameTitleObj);
            }
            
            preLoadedCallback?.Invoke();
            
            if (PageManager.Instance.IndexOf(PageName.CaiFuZhiJiaPopupGameLoading) == 0)
            {
                StartPreloadGamePagesThenOpenMain();
            }
            // if (!isOpen) return;

        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);

            InitParam();
            // StartLoading();
        }

        public override void OnClose(EventData eventData = null)
        {
            if (_pendingMinDisplayCallback != null)
            {
                Timers.inst.Remove(_pendingMinDisplayCallback);
                _pendingMinDisplayCallback = null;
            }
            base.OnClose(eventData);
        }

        private void ResLoadedCallback()
        {
            if (--_totalResCount == 0)
            {
                _isInitialized = true;
                InitParam();
            }
        }
        
        /// <summary>
        /// 并行预加载各子界面；进度条按完成个数增长，全部完成后进入主界面。
        /// </summary>
        private void StartPreloadGamePagesThenOpenMain()
        {
            if (_pendingMinDisplayCallback != null)
            {
                Timers.inst.Remove(_pendingMinDisplayCallback);
                _pendingMinDisplayCallback = null;
            }

            _preloadStartRealtime = Time.realtimeSinceStartup;

            PageName[] pages =
            {
                PageName.CaiFuZhiJiaPageGameMain,
                PageName.CaiFuZhiJiaPopupJackpotGame,
                PageName.CaiFuZhiJiaPopupOverWin,
                PageName.CaiFuZhiJiaPopupJackpotWin,
                PageName.CaiFuZhiJiaPopupFreeSpinTrigger,
                PageName.CaiFuZhiJiaPopupFreeSpinResult,
                PageName.CaiFuZhiJiaPopupJackpotTrigger,
                PageName.CaiFuZhiJiaPopupJackpotResult,
            };

            _preloadTotal = pages.Length;
            _preloadCompleted = 0;
            RefreshLoadingProgressVisual();

            for (int i = 0; i < pages.Length; i++)
            {
                PageManager.Instance.PreloadPage(pages[i], OnOnePreloadPageDone);
            }
        }
        
        private void OnOnePreloadPageDone()
        {
            _preloadCompleted++;
            RefreshLoadingProgressVisual();

            if (_preloadCompleted < _preloadTotal) return;

            TryFinishLoadingAfterPreloads();
        }
        
        private void RefreshLoadingProgressVisual()
        {
            float display = GetDisplayNormalizedProgress();
            SetSliderByPreloadNormalized(display);
        }
        
        private void TryFinishLoadingAfterPreloads()
        {
            float elapsed = Time.realtimeSinceStartup - _preloadStartRealtime;
            if (elapsed >= MinLoadingDisplaySeconds)
            {
                RefreshLoadingProgressVisual();
                CompleteLoadingTransition();
                return;
            }

            if (_pendingMinDisplayCallback != null)
                return;

            RefreshLoadingProgressVisual();
            _pendingMinDisplayCallback = OnLoadingProgressPadTick;
            Timers.inst.Add(0.05f, 0, _pendingMinDisplayCallback);
        }
        
        private void OnLoadingProgressPadTick(object param)
        {
            RefreshLoadingProgressVisual();
            float elapsed = Time.realtimeSinceStartup - _preloadStartRealtime;
            if (_preloadCompleted >= _preloadTotal && elapsed >= MinLoadingDisplaySeconds)
            {
                Timers.inst.Remove(_pendingMinDisplayCallback);
                _pendingMinDisplayCallback = null;
                CompleteLoadingTransition();
            }
        }
        
        private void CompleteLoadingTransition()
        {
            if (_pendingMinDisplayCallback != null)
            {
                Timers.inst.Remove(_pendingMinDisplayCallback);
                _pendingMinDisplayCallback = null;
            }

            SetSliderByPreloadNormalized(1f);
            CloseSelf(null);

            if (PlayerPrefsUtils.isPauseAtPopupGameLoadingOnce)
            {
                PlayerPrefsUtils.isPauseAtPopupGameLoadingOnce = false;
            }
            else
            {
                PageManager.Instance.OpenPage(PageName.CaiFuZhiJiaPageGameMain);
            }
        }
        
        /// <summary>
        /// 将 0~1 的预加载比例映射到 GSlider 的 min~max（FGUI 默认 max=100，直接写 0~1 会显示成约 1% 而非 71%）。
        /// </summary>
        private void SetSliderByPreloadNormalized(float normalized01)
        {
            if (_loadingBar == null)
                return;
            normalized01 = Mathf.Clamp01(normalized01);
            double span = _loadingBar.max - _loadingBar.min;
            if (span <= 0)
                span = 1;
            _loadingBar.value = _loadingBar.min + span * normalized01;
        }
        
        /// <summary>
        /// 进度条取「预加载完成度」与「最短展示时间」的较小值，避免未满最短时间条已 100%。
        /// </summary>
        private float GetDisplayNormalizedProgress()
        {
            return Mathf.Min(GetPreloadRatio(), GetTimeCapRatio());
        }
        
        private float GetPreloadRatio()
        {
            return _preloadTotal > 0 ? (float)_preloadCompleted / _preloadTotal : 1f;
        }

        private float GetTimeCapRatio()
        {
            return Mathf.Clamp01((Time.realtimeSinceStartup - _preloadStartRealtime) / MinLoadingDisplaySeconds);
        }
        
        private void LoadResAsync()
        {
            _totalResCount = 2;

            // 加载Spine动画
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "Trader.prefab", (cloneObj) =>
            {
                _traderObj = cloneObj;
                ResLoadedCallback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "GameTitle.prefab", (cloneObj) =>
            {
                _gameTitleObj = cloneObj;
                ResLoadedCallback();
            });
        }

        // private void StartLoading()
        // {
        //     if (_isFirstOpen)
        //     {
        //         PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPageGameMain, null);
        //         PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupJackpotGame, null);
        //         PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupOverWin, null);
        //         PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupJackpotWin, null);
        //         PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupFreeSpinTrigger, null);
        //         PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupFreeSpinResult, null);
        //         PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupJackpotTrigger, null);
        //         PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupJackpotResult, null);
        //         
        //         _isFirstOpen = false;
        //
        //         Debug.LogError("CaiFuZhiJia is Preloaded!");
        //     }
        //
        //     if (_loadingGTween != null) _loadingGTween.Kill();
        //     _loadingGTween = GTween.To(0, 100, Duration).SetEase(EaseType.Linear).OnUpdate((tween) =>
        //     {
        //         _loadingBar.value = tween.value.x;
        //     }).OnComplete(() =>
        //     {
        //         CloseSelf(null);
        //         PageManager.Instance.OpenPage(PageName.CaiFuZhiJiaPageGameMain);
        //     });
        // }

        

        // private void ResetPage()
        // {
        //     Object.Destroy(_cloneTraderObj);
        //     Object.Destroy(_cloneGameTitleObj);
        //     GameCommon.FguiUtils.DeleteWrapper(_compareTrader);
        //     GameCommon.FguiUtils.DeleteWrapper(_compareGameTitle);
        //
        //     _cloneTraderObj = null;
        //     _cloneGameTitleObj = null;
        //     _compareTrader = null;
        //     _compareGameTitle = null;
        // }
    }
}