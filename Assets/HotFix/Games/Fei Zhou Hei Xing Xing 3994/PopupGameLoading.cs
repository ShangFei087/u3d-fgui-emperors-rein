using FairyGUI;
using GameMaker;
using System.Collections;
using UnityEngine;

namespace FeiZhouHeiXingXing_3994
{
    public class PopupGameLoading : MachinePageBase
    {
        public new const string pkgName = "FeiZhouHeiXingXing";
        public new const string resName = "PopupGameLoading";

        /// <summary>相对 GameRes 的本游戏 PAG 目录。</summary>
        private const string PagPath = "Games/Fei Zhou Hei Xing Xing 3994/Pag/";

        /// <summary>本游戏 Prefab 目录。</summary>
        private const string PrefabPath = "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Prefabs/PopupGameLoading/";

        private int _totalResCount = -1;
        private GSlider _loadingSlider;
        private GTextField _loadingText;

        private GameObject _npcObj,
            _titleObj,
            _tailObj,
            _leavesObj,
            _cloneNpcObj,
            _cloneTitleObj,
            _cloneTailObj,
            _cloneLeavesObj;

        private GComponent _compareNpc, _compareTitle, _compareTail, _compareLeaves;

        private int _preloadTotal;
        private int _preloadCompleted;
        private int _pagPreloadTotal;
        private int _pagPreloadCompleted;
        private bool _pagPreloadFinished;

        /// <summary>从进入并行预加载起算，界面至少展示此时长（秒）；预加载更久则按实际结束。</summary>
        private const float MinLoadingDisplaySeconds = 5f;

        /// <summary>开始并行预加载那一刻的时间戳。</summary>
        private float _preloadStartRealtime;

        /// <summary>FairyGUI 定时器回调，用于最短展示时间内刷新进度条。</summary>
        private TimerCallback _pendingMinDisplayCallback;

        /// <summary>Loading 阶段 PAG 预热协程；关页前须全部完成，异常关页时 Stop 清理。</summary>
        private Coroutine _pagPreloadCoroutine;

        /// <summary>Loading 预热： Pag </summary>
        private static readonly string[] PagPreloadFiles =
        {
            // bigWin pag
            "PopupBigWin/bigwin_720.pag",
            // freeTrigger pag
            "PopupFreeSpinTrigger/fade_1280.pag", "PopupFreeSpinTrigger/fade_1920.pag",
            // smallTrigger pag
            "PopupSmallGameTrigger/fade_1280.pag", "PopupSmallGameTrigger/fade_1920.pag",
        };

        /// <summary>创建 FGUI 界面并异步加载 Loading 背景/标题 Spine Prefab。</summary>
        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();
            _totalResCount = 4;
            // 加载Spine
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Spine_NPC.prefab", (cloneObj) =>
            {
                _npcObj = cloneObj;
                ResLoadCallback();
            });

            // 加载Effect
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Effect_GameTitle.prefab", (cloneObj) =>
            {
                _titleObj = cloneObj;
                ResLoadCallback();
            });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Effect_Leaves.prefab", (cloneObj) =>
            {
                _leavesObj = cloneObj;
                ResLoadCallback();
            });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Effect_Tail.prefab", (cloneObj) =>
            {
                _tailObj = cloneObj;
                ResLoadCallback();
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

        /// <summary>绑定进度条与 Spine 锚点，通知上层后启动子页与 PAG 并行预加载。</summary>
        public override void InitParam()
        {
            if (!isInit) return;
            _loadingSlider = contentPane.GetChild("loadingSlider").asSlider;
            _loadingText = _loadingSlider.GetChild("loadingPercent").asTextField;

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
            
            currentCom = contentPane.GetChild("anchorLeaves").asCom;
            if (currentCom != _compareLeaves)
            {
                _cloneLeavesObj = Object.Instantiate(_leavesObj);
                GameCommon.FguiUtils.DeleteWrapper(_compareLeaves);
                _compareLeaves = currentCom;
                GameCommon.FguiUtils.AddWrapper(_compareLeaves, _cloneLeavesObj);
            }
            
            currentCom = _loadingSlider.GetChild("grip").asCom.GetChild("anchorTail").asCom;
            if (currentCom != _compareTail)
            {
                _cloneTailObj = Object.Instantiate(_tailObj);
                GameCommon.FguiUtils.DeleteWrapper(_compareTail);
                _compareTail = currentCom;
                GameCommon.FguiUtils.AddWrapper(_compareTail, _cloneTailObj);
            }

            preLoadedCallback?.Invoke();
            if (PageManager.Instance.IndexOf(PageName.FeiZhouHeiXingXingPopupGameLoading) == 0)
                StartPreloadGamePagesThenOpenMain();
        }

        /// <summary>打开 Loading 时绑定 UI 并启动并行预加载。</summary>
        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);
            InitParam();
        }

        /// <summary>关闭时停止 PAG 预热协程、清理定时器与 GoWrapper。</summary>
        public override void OnClose(EventData eventData = null)
        {
            if (_pendingMinDisplayCallback != null)
            {
                Timers.inst.Remove(_pendingMinDisplayCallback);
                _pendingMinDisplayCallback = null;
            }

            StopPagPreloadCoroutine();
            DisposeLoadingWrappers();
            base.OnClose(eventData);
        }

        /// <summary>
        /// 关闭时释放 Loading Spine GoWrapper，避免 clone 仍 active 被 SpineStatsCounter 计为在屏渲染。
        /// </summary>
        private void DisposeLoadingWrappers()
        {
            GameCommon.FguiUtils.DeleteWrapper(_compareNpc);
            GameCommon.FguiUtils.DeleteWrapper(_compareTitle);
            GameCommon.FguiUtils.DeleteWrapper(_compareLeaves);
            GameCommon.FguiUtils.DeleteWrapper(_compareTail);

            _cloneNpcObj = null;
            _cloneTitleObj = null;
            _cloneLeavesObj = null;
            _cloneTailObj = null;
            _compareNpc = null;
            _compareTitle = null;
            _compareLeaves = null;
            _compareTail = null;
        }

        private void ResLoadCallback()
        {
            if (--_totalResCount != 0) return;
            isInit = true;
            InitParam();
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
                PageName.FeiZhouHeiXingXingPageGameMain,
                // PageName.FeiZhouHeiXingXingPopupBigWin,
                // PageName.FeiZhouHeiXingXingPopupSmallGameJackpotWin, 
                PageName.FeiZhouHeiXingXingPopupFreeSpinResult, PageName.FeiZhouHeiXingXingPopupFreeSpinTrigger,
                // PageName.FeiZhouHeiXingXingPopupSmallGameResult,
                // PageName.FeiZhouHeiXingXingPopupSmallGameTrigger,
            };

            _preloadTotal = pages.Length;
            _preloadCompleted = 0;
            _pagPreloadTotal = PagPreloadFiles.Length;
            _pagPreloadCompleted = 0;
            _pagPreloadFinished = false;
            RefreshLoadingProgressVisual();

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
        /// 预热 3994 核心 Pag + 3994Npc（共 40，LRU 上限 40）：
        /// AB 解压到 PagCache + Java composition 解码，缩短进局后首次 Play 耗时。
        /// </summary>
        private IEnumerator PagPreloadCoroutine()
        {
            Debug.Log("[3994 Loading] PAG preload start");
            yield return PagPathHelper.PreloadCompositionsCoroutine(
                PagPreloadFiles,
                PagPath,
                (done, total) =>
                {
                    _pagPreloadCompleted = done;
                    _pagPreloadTotal = total;
                    RefreshLoadingProgressVisual();
                });
            _pagPreloadFinished = true;
            _pagPreloadCompleted = _pagPreloadTotal;
            RefreshLoadingProgressVisual();
            Debug.Log("[3994 Loading] PAG preload finished");
            Debug.Log(
                $"[3994 Loading] preload state pages={_preloadCompleted}/{_preloadTotal} pagDone={_pagPreloadFinished}");
            TryFinishLoadingAfterPreloads();
            _pagPreloadCoroutine = null;
        }

        /// <summary>单个子页 PreloadPage 完成时累加计数，全部完成后尝试关页。</summary>
        private void OnOnePreloadPageDone()
        {
            _preloadCompleted++;
            RefreshLoadingProgressVisual();
            Debug.Log($"[3994 Loading] page preload done {_preloadCompleted}/{_preloadTotal}");

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
                PageManager.Instance.OpenPage(PageName.FeiZhouHeiXingXingPageGameMain);
            }

            CloseSelf(null);
        }

        /// <summary>
        /// 将 0~1 的预加载比例映射到 GProgressBar 的 min~max。
        /// </summary>
        private void SetProgressByPreloadNormalized(float normalized01)
        {
            if (_loadingSlider == null)
                return;
            normalized01 = Mathf.Clamp01(normalized01);
            double span = _loadingSlider.max - _loadingSlider.min;
            if (span <= 0)
                span = 1;
            _loadingSlider.value = _loadingSlider.min + span * normalized01;
            int percent = (int)_loadingSlider.value;
            _loadingText.text = percent + "%";
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