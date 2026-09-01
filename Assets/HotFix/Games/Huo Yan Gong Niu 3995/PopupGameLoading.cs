using FairyGUI;
using GameMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace HuoYanGongNiu_3995
{
    public class PopupGameLoading : MachinePageBase
    {
        public const string pkgName = "HuoYanGongNiu_3995";
        public const string resName = "PopupGameLoading";

        GSlider ProgressBar;
        private float duration = 5f;


        //预制体
        private GameObject go_clone, loadTitleGameObject, loadBarEffect, go_loadBar;
        private GComponent anchorLoadText, anchorEffect;
        private new bool isInit = false;
        private Animator animator;

        /// <summary>Loading 预热： Pag </summary>
        private int _pagPreloadTotal;
        private int _pagPreloadCompleted;
        private bool _pagPreloadFinished;
        private const string GamePagFolder = "Games/Huo Yan Gong Niu 3995/Pag";

        /// <summary>Loading 阶段 PAG 预热协程；关页前须全部完成，异常关页时 Stop 清理。</summary>
        private Coroutine _pagPreloadCoroutine;

        private static readonly string[] PagPreloadFiles =
        {
            "fg_pup_Collect_bmp/fg_Collect_tran.pag",
            "fg_pup_Collect_bmp/fg_pup_Collect_idle_bmp.pag",
            "fg_pup_Collect_bmp/fg_pup_Collect_out_bmp.pag",
            "fg_pup_Collect_bmp/fg_pup_Collect_start_bmp.pag",
            "jp_huoshan_bmp/jp_huoshan_dabaofa_idle.pag",
            "jp_huoshan_bmp/jp_huoshan_dabaofa_start.pag",
            "jp_huoshan_bmp/jp_huoshan_idle.pag",
            "jp_huoshan_bmp/jp_huoshan_xiaobaofa.pag",
            "jp_pup_Collect_bmp/jp_pup_Collect_huoshan_idle_bmp.pag",
            "jp_pup_Collect_bmp/jp_pup_Collect_huoshan_start_bmp.pag",
            "jp_pup_Collect_bmp/jp_pup_Collect_idle_bmp.pag",
            "jp_pup_Collect_bmp/jp_pup_Collect_out_bmp.pag",
            "jp_pup_Collect_bmp/jp_pup_Collect_start_bmp.pag",
            "jp_tran_huoqiu_bmp/jp_tran_huoqiuda.pag",
            "jp_tran_huoqiu_bmp/jp_tran_huoqiuxiao.pag",
            "ng_pup_BigWin_bmp/ng_pup_BigWin_da_bmp.pag",
            "ng_pup_BigWin_bmp/ng_pup_BigWin_small_bmp.pag",
            "ng_pup_BigWin_bmp/ng_pup_BigWin_zhong_bmp.pag",
        };

        protected override void OnInit()
        {
            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            int count = 0;
            Action callback = () =>
            {
                if (--count <= 0)
                {
                    isInit = true;
                    InitParam();
                }
            };

            //ResourceManager02.Instance.LoadAsset<GameObject>(
            //"Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/PopupGameLoading/Loading_Title",
            //(GameObject clone) =>
            //{
            //    go_clone = clone;
            //    callback();
            //});

            //ResourceManager02.Instance.LoadAsset<GameObject>(
            //    "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/PopupGameLoading/Loading_Bar.prefab",
            //    (GameObject clone) =>
            //    {
            //        go_loadBar = clone;
            //        callback();
            //    });

            callback();
        }



        public override void OnOpen(PageName name, EventData data)
        {
            DebugUtils.LogError($"启动游戏");

            base.OnOpen(name, data);
            InitParam();
        }

        public override void InitParam()
        {
            if (!isInit) return;

            ProgressBar = this.contentPane.GetChild("Slider").asSlider;
            ProgressBar.value = 0;

            //初始化UI锚点
            //GComponent LocalAnchorLoadingText = contentPane.GetChild("title").asCom;
            //if (anchorLoadText != LocalAnchorLoadingText)
            //{
            //    GameCommon.FguiUtils.DeleteWrapper(anchorLoadText);
            //    loadTitleGameObject = GameObject.Instantiate(go_clone);
            //    animator = loadTitleGameObject.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
            //    anchorLoadText = LocalAnchorLoadingText;
            //    GameCommon.FguiUtils.AddWrapper(anchorLoadText, loadTitleGameObject);
            //}

            preLoadedCallback?.Invoke();
            if (!isOpen) return;

            if (PageManager.Instance.IndexOf(PageName.HuoYanGongNiuPopupGameLoading) == 0)
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
                PageName.HuoYanGongNiuPageGameMain,
                PageName.HuoYanGongNiuPopupFreeSpinTrigger,
                PageName.HuoYanGongNiuPopupFreeSpinExit,
                PageName.HuoYanGongNiuPopupJackpotTrigger,
                PageName.HuoYanGongNiuPopupJackpotResult,
                PageName.HuoYanGongNiuPopupJackpotExit,
                PageName.HuoYanGongNiuPopupBigWin,
            };

            _preloadTotal = pages.Length;
            _preloadCompleted = 0;
            RefreshLoadingProgressVisual();
            //PlayAnim("start");

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

        private void OnOnePreloadPageDone()
        {
            _preloadCompleted++;
            RefreshLoadingProgressVisual();

            if (_preloadCompleted < _preloadTotal) return;

            TryFinishLoadingAfterPreloads();
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
                PageManager.Instance.OpenPage(PageName.HuoYanGongNiuPageGameMain);
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
