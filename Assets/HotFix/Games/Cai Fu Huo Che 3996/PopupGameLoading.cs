using FairyGUI;
using GameMaker;
using System;
using System.Collections;
using UnityEngine;
using System.IO;

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

        /// <summary>Loading 阶段 PAG 预热协程；关页前须全部完成，异常关页时 Stop 清理。</summary>
        private Coroutine _pagPreloadCoroutine;

        /// <summary>Loading 预热： Pag </summary>
        private int _pagPreloadTotal;
        private int _pagPreloadCompleted;
        private bool _pagPreloadFinished;
        private const string GamePagFolder = "Games/Cai Fu Huo Che 3996/Pag";

        private static readonly string[] PagPreloadFiles =
        {
            "InJackpot_bmp.pag",
            "OutJackpot_bmp.pag",
        };

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

            preLoadedCallback?.Invoke();
            if (!isOpen) return;

            if (PageManager.Instance.IndexOf(PageName.CaiFuHuoChePopupGameLoading) == 0)
            {
                StartPreloadGamePagesThenLoadingAnimation();
            }

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

            // 与 PageManager.PreloadPage 并行：利用 Loading 窗口预热 PAG 磁盘缓存与 composition
            _pagPreloadTotal = PagPreloadFiles.Length;
            _pagPreloadCompleted = 0;
            _pagPreloadFinished = false;
            StartPagPreloadInBackground();

            for (int i = 0; i < pages.Length; i++)
            {
                PageManager.Instance.PreloadPage(pages[i], OnOnePreloadPageDone);

            }
        }

        // <summary>利用 Loading 窗口并行预热 PAG 磁盘缓存 + Java composition 解码。</summary>
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
        /// 预热 3997 核心 Pag + 3997Npc（共 40，LRU 上限 40）：
        /// AB 解压到 PagCache + Java composition 解码，缩短进局后首次 Play 耗时。
        /// </summary>
        private IEnumerator PagPreloadCoroutine()
        {
            Debug.Log("[3998 Loading] PAG preload start");
            yield return PagPathHelper.PreloadCompositionsCoroutine(
                PagPreloadFiles,
                GamePagFolder,
                (done, total) =>
                {
                    _pagPreloadCompleted = done;
                    _pagPreloadTotal = total;
                    RefreshLoadingProgressVisual();
                    string currentFile = done > 0 && done <= PagPreloadFiles.Length ? PagPreloadFiles[done - 1] : "?";
                    string assetPath = $"Assets/GameRes/{GamePagFolder}/" + currentFile;
                    bool isExist = File.Exists(assetPath);
                    Debug.Log(
                        $"[3998 Loading] PAG preload progress {done}/{total}, file={assetPath}, isExist={isExist}");
                });
            _pagPreloadFinished = true;
            _pagPreloadCompleted = _pagPreloadTotal;
            RefreshLoadingProgressVisual();
            Debug.Log("[3998 Loading] PAG preload finished");
            Debug.Log(
                $"[3998 Loading] preload state pages={_preloadCompleted}/{_preloadTotal} pagDone={_pagPreloadFinished}");
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
