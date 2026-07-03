using FairyGUI;
using GameMaker;
using System;
using System.Collections;
using UnityEngine;

namespace SlotZhuZaiJinBi1700
{
    public class PopupGameLoading : MachinePageBase
    {
        public const string pkgName = "SlotZhuZaiJinBi1700";
        public const string resName = "PopupGameLoading";

        private GameObject _goLoadingBg, _goLoadingTitle;
        private GameObject _cloneLoadingBg, _cloneLoadingTitle;
        private GComponent _anchorBg, _anchorTitle;
        private GProgressBar _progressBar;
        private Animator _animatorLoadingTitle;

        private bool _isInit;
        private int _preloadTotal;
        private int _preloadCompleted;
        private int _pagPreloadTotal;
        private int _pagPreloadCompleted;
        private bool _pagPreloadFinished;

        /// <summary>从进入并行预加载起算，界面至少展示此时长（秒）；预加载更久则按实际结束。</summary>
        private const float MinLoadingDisplaySeconds = 5f;

        private float _preloadStartRealtime;
        private TimerCallback _pendingMinDisplayCallback;
        /// <summary>Loading 阶段 PAG 预热协程；关页前须全部完成，异常关页时 Stop 清理。</summary>
        private Coroutine _pagPreloadCoroutine;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            int count = 2;
            Action loadComplete = () =>
            {
                if (--count == 0)
                {
                    _isInit = true;
                    InitParam();
                }
            };

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/PopupGameLoading/Loading_bg",
                clone =>
                {
                    _goLoadingBg = clone;
                    loadComplete();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/PopupGameLoading/Loading_Title",
                clone =>
                {
                    _goLoadingTitle = clone;
                    loadComplete();
                });
        }

        protected override void OnLanguageChange(I18nLang lang)
        {
            FguiI18nTextAssistant.Instance.DisposeAllTranslate(contentPane);
            contentPane.Dispose();
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            InitParam();
        }

        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            InitParam();
        }

        public override void OnClose(EventData data = null)
        {
            if (_pendingMinDisplayCallback != null)
            {
                Timers.inst.Remove(_pendingMinDisplayCallback);
                _pendingMinDisplayCallback = null;
            }

            StopPagPreloadCoroutine();
            DisposeLoadingWrappers();
            base.OnClose(data);
        }

        /// <summary>
        /// 关闭时释放 Loading Spine GoWrapper，避免 clone 仍 active 被 SpineStatsCounter 计为在屏渲染。
        /// </summary>
        private void DisposeLoadingWrappers()
        {
            if (_animatorLoadingTitle != null)
            {
                _animatorLoadingTitle.enabled = false;
                _animatorLoadingTitle = null;
            }

            GameCommon.FguiUtils.DeleteWrapper(_anchorBg);
            GameCommon.FguiUtils.DeleteWrapper(_anchorTitle);

            _cloneLoadingBg = null;
            _cloneLoadingTitle = null;
            _anchorBg = null;
            _anchorTitle = null;
        }

        public override void InitParam()
        {
            if (!_isInit) return;

            GComponent localAnchorLoadingBg = contentPane.GetChild("anchorLoadingBG").asCom;
            if (_anchorBg != localAnchorLoadingBg)
            {
                GameCommon.FguiUtils.DeleteWrapper(_anchorBg);
                _cloneLoadingBg = GameObject.Instantiate(_goLoadingBg);
                _anchorBg = localAnchorLoadingBg;
                GameCommon.FguiUtils.AddWrapper(_anchorBg, _cloneLoadingBg);
            }

            GComponent localAnchorLoadingTitle = contentPane.GetChild("anchorLoadingTitle").asCom;
            if (_anchorTitle != localAnchorLoadingTitle)
            {
                GameCommon.FguiUtils.DeleteWrapper(_anchorTitle);
                _cloneLoadingTitle = GameObject.Instantiate(_goLoadingTitle);
                _animatorLoadingTitle = _cloneLoadingTitle.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                _animatorLoadingTitle.enabled = false;
                _anchorTitle = localAnchorLoadingTitle;
                GameCommon.FguiUtils.AddWrapper(_anchorTitle, _cloneLoadingTitle);
            }

            _progressBar = contentPane.GetChild("Slider").asProgress;
            _progressBar.value = _progressBar.min;

            preLoadedCallback?.Invoke();

            if (PageManager.Instance.IndexOf(PageName.SlotZhuZaiJinBiPopupGameLoading) == 0)
                StartPreloadGamePagesThenOpenMain();
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
                PageName.SlotZhuZaiJinPageTest,
            };

            _preloadTotal = pages.Length;
            _preloadCompleted = 0;
            _pagPreloadTotal = PagPathHelper.DefaultGamePagPreloadFiles.Length;
            _pagPreloadCompleted = 0;
            _pagPreloadFinished = false;
            RefreshLoadingProgressVisual();

            if (_animatorLoadingTitle != null)
                _animatorLoadingTitle.enabled = true;

            // 与 PageManager.PreloadPage 并行：利用 Loading 窗口预热 PAG 磁盘缓存与 composition
            StartPagPreloadInBackground();

            for (int i = 0; i < pages.Length; i++)
                PageManager.Instance.PreloadPage(pages[i], OnOnePreloadPageDone);
        }

        /// <summary>利用 Loading 窗口并行预热 PAG 磁盘缓存 + Java composition 解码。</summary>
        private void StartPagPreloadInBackground()
        {
            if (_pagPreloadCoroutine != null && !_pagPreloadFinished)
            {
                return;
            }

            StopPagPreloadCoroutine();
            PagCallbackHub.EnsureInstance();
            PagController.EnsureInit();
            _pagPreloadCoroutine = PagCallbackHub.Instance.RunCoroutine(PagPreloadCoroutine());
        }

        /// <summary>关闭 Loading 时中断 PAG 预热协程，避免 PagCallbackHub 上残留 RunCoroutine。</summary>
        private void StopPagPreloadCoroutine()
        {
            if (_pagPreloadCoroutine == null)
            {
                return;
            }

            PagCallbackHub.Instance.StopRunCoroutine(_pagPreloadCoroutine);
            _pagPreloadCoroutine = null;
        }

        /// <summary>
        /// 预热 1700 核心 Pag + 3997Npc（共 40，LRU 上限 40）：
        /// AB 解压到 PagCache + Java composition 解码，缩短进局后首次 Play 耗时。
        /// </summary>
        private IEnumerator PagPreloadCoroutine()
        {
            Debug.Log("[1700 Loading] PAG preload start");
            yield return PagPathHelper.PreloadCompositionsCoroutine(
                PagPathHelper.DefaultGamePagPreloadFiles,
                PagPathHelper.DefaultGamePagFolder,
                (done, total) =>
                {
                    _pagPreloadCompleted = done;
                    _pagPreloadTotal = total;
                    RefreshLoadingProgressVisual();
                    Debug.Log($"[1700 Loading] PAG preload progress {done}/{total}");
                });
            _pagPreloadFinished = true;
            _pagPreloadCompleted = _pagPreloadTotal;
            RefreshLoadingProgressVisual();
            Debug.Log("[1700 Loading] PAG preload finished");
            TryFinishLoadingAfterPreloads();
            _pagPreloadCoroutine = null;
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
            SetProgressByPreloadNormalized(GetDisplayNormalizedProgress());
        }

        private bool CanCompleteLoadingTransition()
        {
            return _preloadCompleted >= _preloadTotal
                && _pagPreloadFinished
                && Time.realtimeSinceStartup - _preloadStartRealtime >= MinLoadingDisplaySeconds;
        }

        private void TryFinishLoadingAfterPreloads()
        {
            if (CanCompleteLoadingTransition())
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
            if (CanCompleteLoadingTransition())
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

            SetProgressByPreloadNormalized(1f);
            CloseSelf(null);

            if (PlayerPrefsUtils.isPauseAtPopupGameLoadingOnce)
            {
                PlayerPrefsUtils.isPauseAtPopupGameLoadingOnce = false;
            }
            else
            {
                PageManager.Instance.OpenPage(PageName.SlotZhuZaiJinBiPageGameMain);
                //PageManager.Instance.OpenPage(PageName.SlotZhuZaiJinPageTest);
            }
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

        /// <summary>
        /// 进度条取「预加载完成度」与「最短展示时间」的较小值，避免未满最短时间条已 100%。
        /// </summary>
        private float GetDisplayNormalizedProgress()
        {
            return Mathf.Min(GetPreloadRatio(), GetTimeCapRatio());
        }

        private float GetPreloadRatio()
        {
            int pageTotal = Mathf.Max(1, _preloadTotal);
            int pagTotal = Mathf.Max(1, _pagPreloadTotal);
            float pageRatio = (float)_preloadCompleted / pageTotal;
            float pagRatio = (float)_pagPreloadCompleted / pagTotal;
            return (pageRatio + pagRatio) * 0.5f;
        }

        private float GetTimeCapRatio()
        {
            return Mathf.Clamp01((Time.realtimeSinceStartup - _preloadStartRealtime) / MinLoadingDisplaySeconds);
        }
    }
}
