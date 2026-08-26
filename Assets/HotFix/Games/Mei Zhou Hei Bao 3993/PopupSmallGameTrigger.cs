using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    /// <summary>大奖小游戏触发弹窗：Spine in/idle/out，点开始进入大奖盘。</summary>
    public class PopupSmallGameTrigger : MachinePageBase
    {
        /// <summary>FairyGUI 包名。</summary>
        public new const string pkgName = "MeiZhouHeiBao";
        /// <summary>弹窗组件名。</summary>
        public new const string resName = "PopupSmallGameTrigger";
        /// <summary>弹窗 Spine 预制体路径。</summary>
        private const string PrefabPath = "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PopupSmallGameTrigger/PopupSmallGameTrigger.prefab";
        /// <summary>PAG 资源目录。</summary>
        private const string PagPath = "Games/Mei Zhou Hei Bao 3993/Pag";
        /// <summary>大奖弹窗 PAG 入场。</summary>
        private const string PagSmallPupIn = "small_pup/small_pup_in";
        /// <summary>大奖弹窗 PAG 循环待机。</summary>
        private const string PagSmallPupIdle = "small_pup/small_pup_idle";
        /// <summary>大奖弹窗 PAG 离场。</summary>
        private const string PagSmallPupOut = "small_pup/small_pup_out";
        /// <summary>加载后的 Spine 预制体。</summary>
        private GameObject goBonusTrigger;
        /// <summary>Spine 挂点。</summary>
        private GComponent anchorBonusTrigger;
        /// <summary>场景中的 Spine 实例。</summary>
        private GameObject clonegoBonusTrigger;
        /// <summary>弹窗 Spine 播放器。</summary>
        private AnimPlayer _animBonusTrigger;
        /// <summary>播完 out 后延迟关页。</summary>
        private TimerCallback _delayCloseCallback;
        /// <summary>自动化测试自动点开始。</summary>
        private TimerCallback _autoClickCallback;
        /// <summary>入场后延迟点亮开始按钮。</summary>
        private TimerCallback _enableBtnCallback;
        /// <summary>PAG 挂点。</summary>
        private GComponent anchorPagBonusTrigger;
        /// <summary>大奖触发 PAG 槽。</summary>
        private PagSlotBinding pagBonusTrigger;

        /// <summary>开始按钮（挂到 Spine 骨骼）。</summary>
        private GButton btnStart;
        /// <summary>是否已点过关闭，防连点。</summary>
        private bool _isClicked;
        //private GameSoundController3993 _gameSoundController;
        /// <summary>创建 FGUI 并加载 Spine 预制体，注册机台短按 Spin 关页。</summary>
        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
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

            //1
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath,
             (GameObject clone) =>
             {
                 goBonusTrigger = clone;
                 callback();
             });

            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        if (PanelBaseController.ShouldBlockPhysicalSpinInput)
                            return;

                        Debug.LogError("游戏接受到机台短按的数据：Spin");
                        EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, false);
                        OnCloseBtn(res);
                    },
                }
            };
        }

        /// <summary>挂 Spine、绑定开始按钮、延迟可点，自动化则定时点击。</summary>
        public override void InitParam()
        {
            if (!isInit) return;
            preLoadedCallback?.Invoke();
            if (!isOpen) return;
            _isClicked = false;

            RemoveTimer(ref _delayCloseCallback);
            RemoveTimer(ref _autoClickCallback);
            RemoveTimer(ref _enableBtnCallback);
            anchorPagBonusTrigger = contentPane.GetChild("anchorBonusTriggerPag").asCom;
            if (pagBonusTrigger == null) pagBonusTrigger = new PagSlotBinding("3993pagBonusTrigger", PagPath);
            pagBonusTrigger.EnsureSlot(anchorPagBonusTrigger);
            pagBonusTrigger.StopWithDefaults();
            pagBonusTrigger.Play(new PagSequencePlay(
                PagPlaySpecs.IntroLoop(PagSmallPupIn, PagSmallPupIdle),
                PagPlayLayout.Center,
                PagPresentationDefaults.DisplayScale,
                useGpuSyncGroup: false));

            GComponent localBonusTrigger = contentPane.GetChild("anchorBonusTrigger").asCom;
            if (anchorBonusTrigger != localBonusTrigger)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorBonusTrigger);
                clonegoBonusTrigger = GameObject.Instantiate(goBonusTrigger);
                anchorBonusTrigger = localBonusTrigger;
                GameCommon.FguiUtils.AddWrapper(anchorBonusTrigger, clonegoBonusTrigger);
                _animBonusTrigger = new AnimPlayer(clonegoBonusTrigger);
            }
            _animBonusTrigger.PlayThen("in", "idle", true);

            btnStart = contentPane.GetChild("btnStart").asButton;

            btnStart.touchable = false;
            _enableBtnCallback = obj =>
            {
                if (btnStart != null) btnStart.touchable = true;
            };
            Timers.inst.Add(0.5f, 1, _enableBtnCallback);
            btnStart.onClick.Clear();
            btnStart.onClick.Add(() => OnCloseBtn());

            const string rootBonusTriggerPath = "Anchor/Spine Mecanim GameObject (jp_pup_TipFrame)/SkeletonUtility-SkeletonRoot/root/all";
            _animBonusTrigger.Attach(
                btnStart,
                rootBonusTriggerPath + "/fg_img_Button",
                localPos: new Vector3(-2.36f, 0.73f,0.0f),
                localScale: new Vector3(0.01f, 0.01f, 0.01f),
                localRot: Quaternion.identity);

            ScheduleAutoModeClick(3.0f);
        }

        /// <summary>开页后刷新绑定。</summary>
        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);

            //_gameSoundController = new GameSoundController3993();
            //EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT, new EventData(Game3993AudioEvent.BgmBonusTrigger));
            InitParam();
        }

        /// <summary>关页：清定时器、卸骨骼挂点、停 PAG。</summary>
        public override void OnClose(EventData eventData = null)
        {
            RemoveTimer(ref _delayCloseCallback);
            RemoveTimer(ref _autoClickCallback);
            RemoveTimer(ref _enableBtnCallback);
            _animBonusTrigger.DetachAll();
            pagBonusTrigger?.StopWithDefaults();

            base.OnClose(eventData);

            //_gameSoundController?.Dispose();
            //_gameSoundController = null;
        }


        /// <summary>点击开始：播 out，约 1 秒后关页。</summary>
        private void OnCloseBtn(EventData eventData = null)
        {
            if (_isClicked) return;
            _isClicked = true;

            btnStart.touchable = false;
            _animBonusTrigger.Play("out");

            pagBonusTrigger.StopWithDefaults();
            pagBonusTrigger.Play(new PagSequencePlay(
                new[] { new PagSegment(PagSmallPupOut, 1) },
                PagPlayLayout.Center,
                PagPresentationDefaults.DisplayScale,
                useGpuSyncGroup: false,
                callbacks: new PagPlayCallbacks(
                    onFinished: () => pagBonusTrigger?.StopWithDefaults(),
                    stopAfterFinished: true)));

            RemoveTimer(ref _delayCloseCallback);
            _delayCloseCallback = obj =>
            {
                if (isOpen) CloseSelf(eventData);
                _delayCloseCallback = null;
            };
            Timers.inst.Add(1.0f, 1, _delayCloseCallback);
        }

        /// <summary>自动化测试开启时，延迟后自动点开始。</summary>
        private void ScheduleAutoModeClick(float delaySeconds)
        {
            RemoveTimer(ref _autoClickCallback);
            if (!TestManager.Instance.IsAutoModeRunning) return;
            _autoClickCallback = obj =>
            {
                if (isOpen && !_isClicked)
                    OnCloseBtn();
                _autoClickCallback = null;
            };
            Timers.inst.Add(delaySeconds, 1, _autoClickCallback);
        }

        /// <summary>移除 FairyGUI 定时器并置空引用。</summary>
        private void RemoveTimer(ref TimerCallback timerCallback)
        {
            if (timerCallback == null) return;
            Timers.inst.Remove(timerCallback);
            timerCallback = null;
        }
    }
}