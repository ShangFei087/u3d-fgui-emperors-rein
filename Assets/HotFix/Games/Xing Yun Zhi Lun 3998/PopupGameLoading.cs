using FairyGUI;
using GameMaker;
using System;
using UnityEngine;

namespace XingYunZhiLun_3998
{
    public class PopupGameLoading : MachinePageBase
    {
        public new const string pkgName = "XingYunZhiLun_3998";
        public new const string resName = "PopupGameLoading";

        private readonly string[] _dots =
        {
            "",
            ".",
            "..",
            "..."
        };

        // 预制体
        private GameObject _goClone, _loadTitleGameObject;

        // FGUI 组件（进度条在包内名称为 n11）
        private GSlider _progressBar;
        private GTextField _loadText;
        private GComponent _anchorLoadText;

        private new bool isInit = false;
        private Animator _animator;

        private int _preloadTotal;
        private int _preloadCompleted;

        /// <summary>从进入并行预加载起算，界面至少展示此时长（秒）；预加载更久则按实际结束。</summary>
        private const float MinLoadingDisplaySeconds = 5f;

        private float _preloadStartRealtime;
        private TimerCallback _pendingMinDisplayCallback;

        protected override void OnInit()
        {
            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            int count = 1;
            Action callback = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam();
                }
            };

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/PopupGameLoading/Loading_Title",
                (GameObject clone) =>
                {
                    _goClone = clone;
                    callback();
                });
        }

        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            InitParam();
        }

        public override void InitParam()
        {
            if (!isInit) return;

            _progressBar = contentPane.GetChild("n11").asSlider;
            _progressBar.value = _progressBar.min;

            GObject loadObj = contentPane.GetChild("load");
            _loadText = loadObj != null ? loadObj.asTextField : null;

            GComponent localAnchorLoadingText = contentPane.GetChild("title").asCom;
            if (_anchorLoadText != localAnchorLoadingText)
            {
                GameCommon.FguiUtils.DeleteWrapper(_anchorLoadText);
                _loadTitleGameObject = GameObject.Instantiate(_goClone);
                _animator = _loadTitleGameObject.GetComponentInChildren<Animator>(true);
                _anchorLoadText = localAnchorLoadingText;
                GameCommon.FguiUtils.AddWrapper(_anchorLoadText, _loadTitleGameObject);
            }

            preLoadedCallback?.Invoke();

            if (PageManager.Instance.IndexOf(PageName.XingYunZhiLunPopupGameLoading) == 0)
            {
                StartPreloadGamePagesThenOpenMain();
            }
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
                PageName.XingYunZhiLunPageGameMain,
                PageName.XingYunZhiLunPopupJackpotGameTrigger,
                PageName.XingYunZhiLunPopupJackpotGameResult,
                PageName.XingYunZhiLunPopupJackpotGameExit,
                PageName.XingYunZhiLunPopupJackpotGameQuit,
                PageName.XingYunZhiLunPopupJackpotGameEnter,
                PageName.XingYunZhiLunPopupFreeSpinTrigger,
                PageName.XingYunZhiLunPopupFreeSpinResult,
                PageName.XingYunZhiLunPopupBigWin,
                PageName.XingYunZhiLunPopupZhuanPan,
            };

            _preloadTotal = pages.Length;
            _preloadCompleted = 0;
            RefreshLoadingProgressVisual();
            PlayAnimSafe("start");

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

        private float GetPreloadRatio()
        {
            return _preloadTotal > 0 ? (float)_preloadCompleted / _preloadTotal : 1f;
        }

        private float GetTimeCapRatio()
        {
            return Mathf.Clamp01((Time.realtimeSinceStartup - _preloadStartRealtime) / MinLoadingDisplaySeconds);
        }

        /// <summary>
        /// 进度条取「预加载完成度」与「最短展示时间」的较小值，避免未满最短时间条已 100%。
        /// </summary>
        private float GetDisplayNormalizedProgress()
        {
            return Mathf.Min(GetPreloadRatio(), GetTimeCapRatio());
        }

        private void RefreshLoadingProgressVisual()
        {
            float display = GetDisplayNormalizedProgress();
            SetSliderByPreloadNormalized(display);
            int dotIndex = ((int)(display * 100) / 4) % 4;
            SetLoadText($"加载中{_dots[dotIndex]}");
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
            SetLoadText("加载完成");
            CloseSelf(null);

            if (PlayerPrefsUtils.isPauseAtPopupGameLoadingOnce)
            {
                PlayerPrefsUtils.isPauseAtPopupGameLoadingOnce = false;
            }
            else
            {
                PageManager.Instance.OpenPage(PageName.XingYunZhiLunPageGameMain);
            }
        }

        private void SetLoadText(string text)
        {
            if (_loadText != null)
                _loadText.text = text;
        }

        /// <summary>
        /// 将 0~1 的预加载比例映射到 GSlider 的 min~max（FGUI 默认 max=100，直接写 0~1 会显示成约 1% 而非 71%）。
        /// </summary>
        private void SetSliderByPreloadNormalized(float normalized01)
        {
            if (_progressBar == null)
                return;
            normalized01 = Mathf.Clamp01(normalized01);
            double span = _progressBar.max - _progressBar.min;
            if (span <= 0)
                span = 1;
            _progressBar.value = _progressBar.min + span * normalized01;
        }

        private void PlayAnimSafe(string animName)
        {
            if (_animator == null)
                return;
            _animator.Rebind();
            _animator.Play(animName);
            _animator.Update(0f);
        }
    }
}
