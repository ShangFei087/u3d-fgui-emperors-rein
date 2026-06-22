using FairyGUI;
using GameMaker;
using System;
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

        /// <summary>从进入并行预加载起算，界面至少展示此时长（秒）；预加载更久则按实际结束。</summary>
        private const float MinLoadingDisplaySeconds = 5f;

        private float _preloadStartRealtime;
        private TimerCallback _pendingMinDisplayCallback;

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

            base.OnClose(data);
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
                PageName.SlotZhuZaiJinBiPageGameMain,
            };

            _preloadTotal = pages.Length;
            _preloadCompleted = 0;
            RefreshLoadingProgressVisual();

            if (_animatorLoadingTitle != null)
                _animatorLoadingTitle.enabled = true;

            for (int i = 0; i < pages.Length; i++)
                PageManager.Instance.PreloadPage(pages[i], OnOnePreloadPageDone);
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

            SetProgressByPreloadNormalized(1f);
            CloseSelf(null);

            if (PlayerPrefsUtils.isPauseAtPopupGameLoadingOnce)
            {
                PlayerPrefsUtils.isPauseAtPopupGameLoadingOnce = false;
            }
            else
            {
                PageManager.Instance.OpenPage(PageName.SlotZhuZaiJinPageTest);
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
            return _preloadTotal > 0 ? (float)_preloadCompleted / _preloadTotal : 1f;
        }

        private float GetTimeCapRatio()
        {
            return Mathf.Clamp01((Time.realtimeSinceStartup - _preloadStartRealtime) / MinLoadingDisplaySeconds);
        }
    }
}
