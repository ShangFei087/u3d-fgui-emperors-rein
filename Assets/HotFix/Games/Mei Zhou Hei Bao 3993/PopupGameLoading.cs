using FairyGUI;
using GameMaker;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    public class PopupGameLoading : MachinePageBase
    {
        public new const string pkgName = "MeiZhouHeiBao";
        public new const string resName = "PopupGameLoading";

        private const string PrefabPath = "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PopupGameLoading/";

        private int _totalResCount = -1;
        private GSlider _loadingSlider;

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
        }

        private void InitParam(EventData eventData)
        {
            if (!isInit) return;


            preLoadedCallback?.Invoke();
            if (PageManager.Instance.IndexOf(PageName.MeiZhouHeiBaoPopupGameLoading) == 0) return;
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);
            InitParam(eventData);
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

        private void ResLoadedCallBack(EventData eventData = null)
        {
            if (--_totalResCount == 0) return;
            isInit = true;
            InitParam(eventData);
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

            PageName[] pages = { PageName.MeiZhouHeiBaoPageGameMain, PageName.MeiZhouHeiBaoPopupBigWin, PageName.MeiZhouHeiBaoPopupSmallGameJackpotWin, PageName.MeiZhouHeiBaoPopupFreeSpinTrigger, PageName.MeiZhouHeiBaoPopupFreeSpinResult, PageName.MeiZhouHeiBaoPopupSmallGameTrigger, PageName.MeiZhouHeiBaoPopupSmallGameResult, };

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
                PageManager.Instance.OpenPage(PageName.MeiZhouHeiBaoPageGameMain);
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