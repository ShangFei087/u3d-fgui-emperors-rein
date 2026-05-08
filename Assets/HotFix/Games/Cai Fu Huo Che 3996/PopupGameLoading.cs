using FairyGUI;
using GameMaker;
using System;
using UnityEngine;

namespace CaiFuHuoChe_3996
{
    public class PopupGameLoading : MachinePageBase
    {
        public const string pkgName = "CaiFuHuoChe_3996";
        public const string resName = "PopupGameLoading";

        GTextField Load, version;
        GSlider ProgressBar;
        private string[] dots = new string[]
        {
            "",
            ".",
            "..",
            "..."
        };

        //预制体
        private GameObject go_clone, loadTitleGameObject, loadBarEffect, go_loadBar;
        private GComponent anchorLoadText, anchorEffect;
        private new bool isInit = false;
        private Animator animator;

        protected override void OnInit()
        {
            base.OnInit();

            int count = 2;
            Action callback = () =>
            {
                if (--count <= 0)
                {
                    isInit = true;
                    InitParam();
                }
            };

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/PopupGameLoading/Loading_Title",
            (GameObject clone) =>
            {
                go_clone = clone;
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/PopupGameLoading/Loading_Bar.prefab",
                (GameObject clone) =>
                {
                    go_loadBar = clone;
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

            Load = this.contentPane.GetChild("load").asTextField;
            //version = this.contentPane.GetChild("version").asTextField;
            //version.text = GlobalData.hotfixVersion;
            ProgressBar = this.contentPane.GetChild("Slider").asSlider;

            //初始化UI锚点
            GComponent LocalAnchorLoadingText = contentPane.GetChild("title").asCom;
            if (anchorLoadText != LocalAnchorLoadingText)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorLoadText);
                loadTitleGameObject = GameObject.Instantiate(go_clone);
                animator = loadTitleGameObject.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                anchorLoadText = LocalAnchorLoadingText;
                GameCommon.FguiUtils.AddWrapper(anchorLoadText, loadTitleGameObject);
            }

            GComponent localAnchorEffect = contentPane.GetChild("Slider").asSlider.GetChild("anchorEffect").asCom;
            if (anchorEffect != localAnchorEffect)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorEffect);
                anchorEffect = localAnchorEffect;
                loadBarEffect = GameObject.Instantiate(go_loadBar);
                GameCommon.FguiUtils.AddWrapper(anchorEffect, loadBarEffect);
            }


            if (PageManager.Instance.IndexOf(PageName.CaiFuHuoChePopupGameLoading) == 0)
            {
                StartPreloadGamePagesThenLoadingAnimation();
            }

            preLoadedCallback?.Invoke();
        }

        private int _preloadTotal;
        private int _preloadCompleted;

        /// <summary>从进入并行预加载起算，界面至少展示此时长（秒）；预加载更久则按实际结束。</summary>
        private const float MinLoadingDisplaySeconds = 5f;

        private float _preloadStartRealtime;
        private TimerCallback _pendingMinDisplayCallback;

        /// <summary>
        /// 并行预加载各子界面；进度条按完成个数增长，全部完成后进入主界面。
        /// </summary>
        private void StartPreloadGamePagesThenLoadingAnimation()
        {
            if (_pendingMinDisplayCallback != null)
            {
                Timers.inst.Remove(_pendingMinDisplayCallback);
                _pendingMinDisplayCallback = null;
            }

            _preloadStartRealtime = Time.realtimeSinceStartup;

            PageName[] pages =
            {
                PageName.CaiFuHuoChePageGameMain,
                PageName.CaiFuHuoChePopupBigWin,
                PageName.CaiFuHuoChePopupFreeSpinTrigger,
                PageName.CaiFuHuoChePopupFreeSpinResult,
                PageName.CaiFuHuoChePopupJackpotGameTrigger,
                PageName.CaiFuHuoChePopupJackpotResult,
                PageName.CaiFuHuoChePopupJackpotGameExit,
            };

            _preloadTotal = pages.Length;
            _preloadCompleted = 0;
            RefreshLoadingProgressVisual();
            PlayAnim("start");

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

        public override void OnClose(EventData data = null)
        {
            if (_pendingMinDisplayCallback != null)
            {
                Timers.inst.Remove(_pendingMinDisplayCallback);
                _pendingMinDisplayCallback = null;
            }

            base.OnClose(data);
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
            Load.text = $"加载中{dots[dotIndex]}";
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
            Load.text = "加载完成";
            CloseSelf(null);

            if (PlayerPrefsUtils.isPauseAtPopupGameLoadingOnce)
            {
                PlayerPrefsUtils.isPauseAtPopupGameLoadingOnce = false;
            }
            else
            {
                PageManager.Instance.OpenPage(PageName.CaiFuHuoChePageGameMain);
            }
        }

        /// <summary>
        /// 将 0~1 的预加载比例映射到 GSlider 的 min~max（FGUI 默认 max=100，直接写 0~1 会显示成约 1% 而非 71%）。
        /// </summary>
        private void SetSliderByPreloadNormalized(float normalized01)
        {
            if (ProgressBar == null)
                return;
            normalized01 = Mathf.Clamp01(normalized01);
            double span = ProgressBar.max - ProgressBar.min;
            if (span <= 0)
                span = 1;
            ProgressBar.value = ProgressBar.min + span * normalized01;
        }

        //播放指定动画
        private void PlayAnim(string animName)
        {
            animator.Rebind();
            animator.Play(animName);
            animator.Update(0f);
        }
    }
}
