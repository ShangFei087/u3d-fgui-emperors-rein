using FairyGUI;
using GameMaker;
using System;
using System.Collections;
using UnityEngine;

namespace SlotZhuZaiJinBi1700
{
    /// <summary>
    /// 1700 进局 Loading 弹窗：并行预热 PAG、预加载 PageTest/PageGameMain，全部就绪后再关页进入主界面。
    /// </summary>
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

        /// <summary>相对 GameRes 的本游戏 PAG 目录。</summary>
        private const string GamePagFolder = "Games/Slot Zhu Zai Jin Bi 1700/Pag";

        /// <summary>Loading 预热： Pag </summary>
        private static readonly string[] PagPreloadFiles =
        {
            "BigWin_1080.pag",
            "Fade.pag",
            "Fire.pag",
            "FeiZhou.pag",
            "Dragon.pag",
            "CaiHongFeiDie.pag",
            "XingXing1.pag",
            "XingXing2.pag",
            "XingXing3.pag",
            "Lopp/glow_loop_720.pag",
            "Lopp/glow_loop_half_1920.pag",
            "Lopp/glow_loop_full_1920.pag",
            "Lopp/glow_in_half_1920.pag",
            "Lopp/glow_in_full_1920.pag",
            "3997Npc/BigWinNPC/bigwin_start1.pag",
            "3997Npc/BigWinNPC/bigwin_idle1.pag",
            "3997Npc/BigWinNPC/supwin_start1.pag",
            "3997Npc/BigWinNPC/supwin_idle1.pag",
            "3997Npc/BigWinNPC/megawin_start1.pag",
            "3997Npc/BigWinNPC/megawin_idle1.pag",
            "3997Npc/NormalNPC/Wealth_ng_npc_idle01.pag",
            "3997Npc/NormalNPC/Wealth_ng_npc_idle02.pag",
            "3997Npc/NormalNPC/Wealth_ng_npc_atmosphere.pag",
            "3997Npc/NormalNPC/Wealth_ng_npc_not triggered.pag",
            "3997Npc/NormalNPC/Wealth_ng_npc_not winning.pag",
            "3997Npc/NormalNPC/Wealth_ng_npc_trigger fg.pag",
            "3997Npc/NormalNPC/Wealth_ng_npc_trigger sg.pag",
            "3997Npc/NormalNPC/Wealth_ng_npc_win1.pag",
            "3997Npc/NormalNPC/Wealth_ng_npc_win2.pag",
            "3997Npc/NormalNPC/Wealth_ng_npc_win3.pag",
            "3997Npc/FreeNPC/Wealth_fg_npc_settlement.pag",
            "3997Npc/FreeNPC/Wealth_fg_npc_upgrade1.pag",
            "3997Npc/FreeNPC/Wealth_fg_npc_upgrade2.pag",
            "3997Npc/RewardNPC/Wealth_sg_npc_appear.pag",
            "3997Npc/RewardNPC/Wealth_sg_npc_idle1.pag",
            "3997Npc/RewardNPC/Wealth_sg_npc_idle2.pag",
            "3997Npc/RewardNPC/Wealth_sg_npc_reset.pag",
            "3997Npc/RewardNPC/Wealth_sg_npc_settlement1.pag",
            "3997Npc/RewardNPC/Wealth_sg_npc_settlement2.pag",
            "3997Npc/RewardNPC/Wealth_sg_npc_settlement3.pag",
        };

        /// <summary>从进入并行预加载起算，界面至少展示此时长（秒）；预加载更久则按实际结束。</summary>
        private const float MinLoadingDisplaySeconds = 5f;
        /// <summary>开始并行预加载那一刻的时间戳。</summary>
        private float _preloadStartRealtime;
        /// <summary>FairyGUI 定时器回调，用于最短展示时间内刷新进度条。</summary>
        private TimerCallback _pendingMinDisplayCallback;
        /// <summary>Loading 阶段 PAG 预热协程；关页前须全部完成，异常关页时 Stop 清理。</summary>
        private Coroutine _pagPreloadCoroutine;

        /// <summary>创建 FGUI 界面并异步加载 Loading 背景/标题 Spine Prefab。</summary>
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

        /// <summary>语言切换时重建 contentPane 并重新初始化。</summary>
        protected override void OnLanguageChange(I18nLang lang)
        {
            FguiI18nTextAssistant.Instance.DisposeAllTranslate(contentPane);
            contentPane.Dispose();
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            InitParam();
        }

        /// <summary>打开 Loading 时绑定 UI 并启动并行预加载。</summary>
        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            InitParam();
        }

        /// <summary>关闭时停止 PAG 预热协程、清理定时器与 GoWrapper。</summary>
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

        /// <summary>绑定进度条与 Spine 锚点，通知上层后启动子页与 PAG 并行预加载。</summary>
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
            StartPreloadGamePagesThenOpenMain();
        }

        /// <summary>
        /// 并行预加载 PageTest、PageGameMain 与 PAG composition；进度条按完成个数增长，全部完成后关页进主界面。
        /// </summary>
        private void StartPreloadGamePagesThenOpenMain()
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

            PageName[] pages =
            {
                PageName.SlotZhuZaiJinPageTest,
                PageName.SlotZhuZaiJinBiPageGameMain,
            };

            _preloadTotal = pages.Length;
            _preloadCompleted = 0;
            _pagPreloadTotal = PagPreloadFiles.Length;
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
            PagBootstrap.EnsureReady();
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
                PagPreloadFiles,
                GamePagFolder,
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
            Debug.Log($"[1700 Loading] preload state pages={_preloadCompleted}/{_preloadTotal} pagDone={_pagPreloadFinished}");
            TryFinishLoadingAfterPreloads();
            _pagPreloadCoroutine = null;
        }

        /// <summary>单个子页 PreloadPage 完成时累加计数，全部完成后尝试关页。</summary>
        private void OnOnePreloadPageDone()
        {
            _preloadCompleted++;
            RefreshLoadingProgressVisual();
            Debug.Log($"[1700 Loading] page preload done {_preloadCompleted}/{_preloadTotal}");

            if (_preloadCompleted < _preloadTotal) return;

            TryFinishLoadingAfterPreloads();
        }

        /// <summary>根据当前预加载比例刷新进度条显示。</summary>
        private void RefreshLoadingProgressVisual()
        {
            SetProgressByPreloadNormalized(GetDisplayNormalizedProgress());
        }

        /// <summary>是否满足关页条件：双页预加载完成、PAG 预热完成且已过最短展示时间。</summary>
        private bool CanCompleteLoadingTransition()
        {
            return _preloadCompleted >= _preloadTotal
                && _pagPreloadFinished
                && Time.realtimeSinceStartup - _preloadStartRealtime >= MinLoadingDisplaySeconds;
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

            if (_pendingMinDisplayCallback != null)
                return;

            RefreshLoadingProgressVisual();
            _pendingMinDisplayCallback = OnLoadingProgressPadTick;
            Timers.inst.Add(0.05f, 0, _pendingMinDisplayCallback);
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
                PageManager.Instance.OpenPage(PageName.SlotZhuZaiJinBiPageGameMain);
            }

            CloseSelf(null);
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
    }
}
