using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    public class PopupSmallGameTrigger : MachinePageBase
    {
        public new const string pkgName = "MeiZhouHeiBao";
        public new const string resName = "PopupSmallGameTrigger";
        private const string PrefabPath = "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PopupSmallGameTrigger/PopupSmallGameTrigger/";
        private const string PagPath = "Games/Mei Zhou Hei Bao 3993/Pag";
        //弹窗
        private GameObject goBonusTrigger;
        private GComponent anchorBonusTrigger;
        private GameObject clonegoBonusTrigger;
        private AnimPlayer _animBonusTrigger;
        private TimerCallback _delayCloseCallback;
        //pag
        private GComponent anchorPagBonusTrigger;
        private PagSlotBinding pagBonusTrigger;

        private GButton btnStart;
        private bool _isClicked;
        //private GameSoundController3993 _gameSoundController;
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
            ResourceManager02.Instance.LoadAsset<GameObject>(
            PrefabPath,
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

        public override void InitParam()
        {
            if (!isInit) return;
            preLoadedCallback?.Invoke();
            if (!isOpen) return;
            _isClicked = false;

            RemoveTimer(ref _delayCloseCallback);
            anchorPagBonusTrigger = contentPane.GetChild("anchorBonusTriggerPag").asCom;
            if (pagBonusTrigger == null) pagBonusTrigger = new PagSlotBinding("3993pagBonusTrigger", PagPath);
            pagBonusTrigger.EnsureSlot(anchorPagBonusTrigger);

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

            btnStart = contentPane.GetChild("BtnStart").asButton;

            btnStart.touchable = false;
            Timers.inst.Add(0.5f, 1, obj =>
            {
                if (btnStart != null) btnStart.touchable = true;
            });
            btnStart.onClick.Clear();
            btnStart.onClick.Add(() => OnCloseBtn());

            const string rootBonusTriggerPath = "Anchor/Spine Mecanim GameObject (jp_pup_TipFrame)/SkeletonUtility-SkeletonRoot/root/all";
            _animBonusTrigger.Attach(
                btnStart,
                rootBonusTriggerPath + "/fg_img_Button",
                localPos: new Vector3(),
                localScale: new Vector3(0.01f, 0.01f, 0.01f),
                localRot: Quaternion.identity);

        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);

            //_gameSoundController = new GameSoundController3993();
            //EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT, new EventData(Game3993AudioEvent.BgmBonusTrigger));
            InitParam();
        }

        public override void OnClose(EventData eventData = null)
        {
            RemoveTimer(ref _delayCloseCallback);
            _animBonusTrigger.DetachAll();

            base.OnClose(eventData);

            //_gameSoundController?.Dispose();
            //_gameSoundController = null;
        }


        private void OnCloseBtn(EventData eventData = null)
        {
            if (_isClicked) return;
            _isClicked = true;

            btnStart.touchable = false;
            _animBonusTrigger.Play("out");

            RemoveTimer(ref _delayCloseCallback);
            _delayCloseCallback = obj =>
            {
                if (isOpen) CloseSelf(eventData);
                _delayCloseCallback = null;
            };
            Timers.inst.Add(1.0f, 1, _delayCloseCallback);
        }

        private void RemoveTimer(ref TimerCallback timerCallback)
        {
            if (timerCallback == null) return;
            Timers.inst.Remove(timerCallback);
            timerCallback = null;
        }
    }
}