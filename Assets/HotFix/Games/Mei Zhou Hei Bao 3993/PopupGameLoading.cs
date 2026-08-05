using FairyGUI;
using GameMaker;
using System;
using UnityEditor.UIElements;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    public class PopupGameLoading : MachinePageBase
    {
        public new const string pkgName = "MeiZhouHeiBao";
        public new const string resName = "PopupGameLoading";
        private const string GamePagFolder = "Games/Mei Zhou Hei Bao 3993/Pag";
        /// <summary>Loading 预热： Pag </summary>
        private static readonly string[] PagPreloadFiles =
        {

        };
        /// <summary>Loading 预热： Page </summary>
        private static readonly PageName[] PagesPreload =
        {
            PageName.MeiZhouHeiBaoPageGameMain
        };
        // <summary> 加载条 </summary>
        private GSlider _progressBar;
        // <summary> 加载条 </summary>
        GTextField _txtload, _txtversion;
        // <summary>FairyGUI 定时器回调，用于最短展示时间内刷新进度条。</summary>
        private TimerCallback _pendingMinDisplayCallback;
        // <summary>开始并行预加载那一刻的时间戳。</summary>
        private float _preloadStartRealtime;
        /// <summary>从进入并行预加载起算，界面至少展示此时长（秒）；预加载更久则按实际结束。</summary>
        private const float MinLoadingDisplaySeconds = 5f;
        /// <summary>需要预加载的页面总数。</summary>
        private int _preloadTotal;
        /// <summary>已完成预加载的页面数。</summary>
        private int _preloadCompleted;
        /// <summary>需要预加载的 PAG 资源总数。</summary>
        private int _pagPreloadTotal;
        /// <summary>已完成预加载的 PAG 资源数。</summary>
        private int _pagPreloadCompleted;
        /// <summary>PAG 预加载是否全部完成。</summary>
        private bool _pagPreloadFinished;
        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            int count = 0;
            Action loadComplete = () =>
            {
                if (count == 0)
                {
                    isInit = true;
                    InitParam();
                }
            };

            loadComplete();
        }

        protected override void OnLanguageChange(I18nLang lang)
        {
            FguiI18nTextAssistant.Instance.DisposeAllTranslate(contentPane);
            contentPane.Dispose();
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            InitParam();
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);
            InitParam(eventData);
        }

        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);
        }

        private void InitParam(EventData eventData)
        {
            if (!isInit) return;

            _progressBar=contentPane.GetChild("Slider") as GSlider;
            _progressBar.value = _progressBar.min;

            preLoadedCallback?.Invoke();

            StartPreLoadandOpenMain();
        }

        /// <summary>
        /// 并行预加载 PageTest、PageGameMain 与 PAG composition；进度条按完成个数增长，全部完成后关页进主界面。
        /// </summary>
        private void StartPreLoadandOpenMain()
        {
            if (_pendingMinDisplayCallback != null)
            {
                Timers.inst.Remove(_pendingMinDisplayCallback);
                _pendingMinDisplayCallback = null;
            }

            _preloadStartRealtime = Time.realtimeSinceStartup;
#if UNITY_EDITOR
            Debug.Assert(PagPreloadFiles.Length <= PagPathHelper.MaxCompositionCacheSize,
                $"PagPreloadFiles count ({PagPreloadFiles.Length}) exceeds Java LRU limit ({PagPathHelper.MaxCompositionCacheSize})");
#endif

            _preloadTotal = PagesPreload.Length;
            _pagPreloadTotal = PagPreloadFiles.Length;
            _pagPreloadCompleted = 0;
            _pagPreloadFinished = false;
            RefreshLoadingProgressVisual();

            // 与 PageManager.PreloadPage 并行：利用 Loading 窗口预热 PAG 磁盘缓存与 composition
            StartPagPreloadInBackground();

            for (int i = 0; i < PagesPreload.Length; i++) PageManager.Instance.PreloadPage(PagesPreload[i], OnOnePreloadPageDone);
        }

        /// <summary>单个子页 PreloadPage 完成时累加计数，全部完成后尝试关页。</summary>
        private void OnOnePreloadPageDone()
        {
            ++_preloadCompleted;
            RefreshLoadingProgressVisual();

            if (_preloadCompleted < _preloadTotal) return;
            TryFinishLoadingAfterPreloads();
        }

        private void StartPagPreloadInBackground()
        {
            _pagPreloadFinished = true;
        }

        /// <summary>预加载进度更新后检查可否关页；未满最短展示时间则注册定时器继续等待。</summary>
        private void TryFinishLoadingAfterPreloads()
        {
            if (CanCompleteLoadingTransition())
            {
                RefreshLoadingProgressVisual();
                CompleteLoadingTransition();
                return;
            }

            if (_pendingMinDisplayCallback != null) return;
            RefreshLoadingProgressVisual();
            _pendingMinDisplayCallback = OnLoadingProgressPadTick;
            Timers.inst.Add(0.05f, 0, _pendingMinDisplayCallback);

        }

        /// <summary>先 OpenPage 主界面再 CloseSelf，避免关 Loading 与进局之间的闪帧。</summary>
        private void CompleteLoadingTransition()
        {
            if (_pendingMinDisplayCallback != null)
            {
                Timers.inst.Remove(_pendingMinDisplayCallback);
                _pendingMinDisplayCallback = null;
            }

            SetProgressByPreloadNormalized(1f);

            if (PlayerPrefsUtils.isPauseAtPopupGameLoadingOnce)
            {
                PlayerPrefsUtils.isPauseAtPopupGameLoadingOnce = false;
            }
            else
            {
                PageManager.Instance.OpenPage(PageName.MeiZhouHeiBaoPageGameMain);
            }

            CloseSelf(null);
        }

        /// <summary>最短展示时间内的定时 tick：刷新进度并在条件满足时执行关页过渡。</summary>
        private void OnLoadingProgressPadTick(object param)
        {
            RefreshLoadingProgressVisual();
            if (CanCompleteLoadingTransition())
            {
                Timers.inst.Remove(_pendingMinDisplayCallback);
                _pendingMinDisplayCallback = null;
                CompleteLoadingTransition();
            }
        }

        private bool CanCompleteLoadingTransition()
        {
            return _preloadCompleted >= _preloadTotal
                && _pagPreloadFinished
                && Time.realtimeSinceStartup - _preloadStartRealtime >= MinLoadingDisplaySeconds;
        }

        /// <summary>根据当前预加载比例刷新进度条显示。</summary>
        private void RefreshLoadingProgressVisual()
        {
            SetProgressByPreloadNormalized(GetDisplayNormalizedProgress());
        }

        /// <summary>
        /// 进度条取「预加载完成度」与「最短展示时间」的较小值，避免未满最短时间条已 100%。
        /// </summary>
        private float GetDisplayNormalizedProgress()
        {
            return Mathf.Min(GetPreloadRatio(), GetTimeCapRatio());
        }

        /// <summary>页面预加载与 PAG 预热各占 50% 权重的综合完成比例。</summary>
        private float GetPreloadRatio()
        {
            int pageTotal = Mathf.Max(1, _preloadTotal);
            int pagTotal = Mathf.Max(1, _pagPreloadTotal);
            float pageRatio = (float)_preloadCompleted / pageTotal;
            float pagRatio = (float)_pagPreloadCompleted / pagTotal;
            return (pageRatio + pagRatio) * 0.5f;
        }

        /// <summary>按最短展示时间折算的进度上限比例（0~1）。</summary>
        private float GetTimeCapRatio()
        {
            return Mathf.Clamp01((Time.realtimeSinceStartup - _preloadStartRealtime) / MinLoadingDisplaySeconds);
        }

        /// <summary>
        /// 将 0~1 的预加载比例映射到 GProgressBar 的 min~max。
        /// </summary>
        private void SetProgressByPreloadNormalized(float normalized01)
        {
            if (_progressBar == null)
                return;
            normalized01 = Mathf.Clamp01(normalized01);
            double span = _progressBar.max - _progressBar.min;
            if (span <= 0)
                span = 1;
            _progressBar.value = _progressBar.min + span * normalized01;
        }
    }
}