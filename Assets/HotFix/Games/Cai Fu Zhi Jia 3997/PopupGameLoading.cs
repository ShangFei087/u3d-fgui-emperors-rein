using FairyGUI;
using GameMaker;
using UnityEngine;

namespace CaiFuZhiJia_3997
{
    public class PopupGameLoading : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiJia";
        public new const string resName = "PopupGameLoading";

        private const string SpinePrefabsPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupGameLoading/SpinePrefabs/";

        private int _totalResCount = -1;
        private bool _isInitialized = false;

        private GSlider _loadingSlider;
        private GameObject _npcObj, _titleObj;
        private GameObject _cloneNpcObj, _cloneTitleObj;
        private GComponent _compareNpc, _compareTitle;

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

            _totalResCount = 2;

            // 加载Spine动画
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "Trader.prefab", (cloneObj) =>
            {
                _npcObj = cloneObj;
                ResLoadedCallback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "GameTitle.prefab", (cloneObj) =>
            {
                _titleObj = cloneObj;
                ResLoadedCallback();
            });
        }

        public override void InitParam()
        {
            if (!_isInitialized) return;
            // 初始化进度条Slider
            _loadingSlider = contentPane.GetChild("loadingSlider").asSlider;

            // 绑定预制体到UI锚点
            GComponent currentCom = contentPane.GetChild("anchorLoadingNpc").asCom;
            if (_compareNpc != currentCom)
            {
                _cloneNpcObj = Object.Instantiate(_npcObj);
                GameCommon.FguiUtils.DeleteWrapper(_compareNpc);
                _compareNpc = currentCom;
                GameCommon.FguiUtils.AddWrapper(_compareNpc, _cloneNpcObj);
            }

            currentCom = contentPane.GetChild("anchorLoadingTitle").asCom;
            if (currentCom != _compareTitle)
            {
                _cloneTitleObj = Object.Instantiate(_titleObj);
                GameCommon.FguiUtils.DeleteWrapper(_compareTitle);
                _compareTitle = currentCom;
                GameCommon.FguiUtils.AddWrapper(_compareTitle, _cloneTitleObj);
            }

            preLoadedCallback?.Invoke();
            
            if (PageManager.Instance.IndexOf(PageName.CaiFuZhiJiaPopupGameLoading) == 0)
                StartPreloadGamePagesThenOpenMain();
        }
        
        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);
            InitParam();
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
            if (--_totalResCount != 0) return;
            _isInitialized = true;
            InitParam();
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
                PageName.CaiFuZhiJiaPopupOverWin,
                PageName.CaiFuZhiJiaPopupJackpotWin,
                PageName.CaiFuZhiJiaPopupFreeSpinTrigger,
                PageName.CaiFuZhiJiaPopupFreeSpinResult,
                PageName.CaiFuZhiJiaPopupSmallGameTrigger,
                PageName.CaiFuZhiJiaPopupSmallGameResult,
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
            if (_loadingSlider == null)
                return;
            normalized01 = Mathf.Clamp01(normalized01);
            double span = _loadingSlider.max - _loadingSlider.min;
            if (span <= 0)
                span = 1;
            _loadingSlider.value = _loadingSlider.min + span * normalized01;
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
    }
}