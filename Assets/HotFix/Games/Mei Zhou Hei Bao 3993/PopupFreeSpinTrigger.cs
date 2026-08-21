using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    public class PopupFreeSpinTrigger : MachinePageBase
    {
        public new const string pkgName = "MeiZhouHeiBao";
        public new const string resName = "PopupFreeSpinTrigger";

        private const string PrefabPath = "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PopupFreeSpinTrigger/PopupFreeSpinTrigger.prefab";
        private const string PagPath = "Games/Mei Zhou Hei Bao 3993/Pag";
        private const string PagFgPupIn = "fg_pup/fg_pup_in";
        private const string PagFgPupIdle = "fg_pup/fg_pup_idle";
        private const string PagFgPupOut = "fg_pup/fg_pup_out";
  
        //弹窗
        private GameObject goFreeTrigger;
        private GComponent anchorFreeTrigger;
        private GameObject clonegoFreeTrigger;
        private AnimPlayer _animFreeTrigger;
        private TimerCallback _delayCloseCallback;
        private TimerCallback _autoClickCallback;
        //pag
        private GComponent anchorPagFreeTrigger;
        private PagSlotBinding pagFreeTrigger;
      
        private GButton btnStart;
        private GTextField txtFreeTime;

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
                 goFreeTrigger = clone;
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
            RemoveTimer(ref _autoClickCallback);

            anchorPagFreeTrigger= contentPane.GetChild("anchorFreeTriggerPag").asCom;
            if (pagFreeTrigger == null) pagFreeTrigger = new PagSlotBinding("3993pagFreeTrigger", PagPath);
            pagFreeTrigger.EnsureSlot(anchorPagFreeTrigger);
            PlayFgPupInIdle();

            GComponent localFreeTrigger = contentPane.GetChild("anchorFreeTrigger").asCom;
            if (anchorFreeTrigger != localFreeTrigger)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorFreeTrigger);
                clonegoFreeTrigger = GameObject.Instantiate(goFreeTrigger);
                anchorFreeTrigger = localFreeTrigger;
                GameCommon.FguiUtils.AddWrapper(anchorFreeTrigger, clonegoFreeTrigger);
                _animFreeTrigger = new AnimPlayer(clonegoFreeTrigger);
            }
            _animFreeTrigger.PlayThen("in", "idle", true);

            btnStart = contentPane.GetChild("BtnStart").asButton;
            txtFreeTime = contentPane.GetChild("FreeTimeText").asTextField;
            txtFreeTime.text = ContentModel.Instance.freeSpinTotalTimes.ToString();
            btnStart.touchable = false;
            Timers.inst.Add(0.5f, 1, obj =>
            {
                if (btnStart != null) btnStart.touchable = true;
            });
            btnStart.onClick.Clear();
            btnStart.onClick.Add(() => OnCloseBtn());

            const string rootFreeTriggerPath = "Anchor/Spine Mecanim GameObject (fg_pup_TipFrame)/SkeletonUtility-SkeletonRoot/root/all";
            _animFreeTrigger.Attach(
                btnStart,
                rootFreeTriggerPath + "/fg_START_01",
                localPos: new Vector3(-2.213f, 0.561f),
                localScale: new Vector3(0.01f, 0.01f, 0.01f),
                localRot: Quaternion.identity);
            _animFreeTrigger.Attach(
                txtFreeTime,
                rootFreeTriggerPath + "/Base plate/fg_img_FREE GAMES",
                localPos: new Vector3(-4.19f, 4.0f, 0.0f),
                localScale: new Vector3(0.01f, 0.01f, 0.01f),
                localRot: Quaternion.identity);

            ScheduleAutoModeClick(3.0f);
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);

            //_gameSoundController = new GameSoundController3993();
            //EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT, new EventData(Game3993AudioEvent.BgmFreeSpinTrigger));
            InitParam();
        }

        public override void OnClose(EventData eventData = null)
        {
            RemoveTimer(ref _delayCloseCallback);
            RemoveTimer(ref _autoClickCallback);
            _animFreeTrigger.DetachAll();
            pagFreeTrigger?.StopWithDefaults();

            base.OnClose(eventData);

            //_gameSoundController?.Dispose();
            //_gameSoundController = null;
        }

        private void OnCloseBtn(EventData eventData = null)
        {
            if (_isClicked) return;
            _isClicked = true;

            btnStart.touchable = false;
            _animFreeTrigger.Play("out");
            PlayFgPupOut();

            RemoveTimer(ref _delayCloseCallback);
            _delayCloseCallback = obj =>
            {
                if (isOpen) CloseSelf(eventData);
                _delayCloseCallback = null;
            };
            Timers.inst.Add(1.0f, 1, _delayCloseCallback);
        }

        private void PlayFgPupInIdle()
        {
            if (pagFreeTrigger == null) return;
            pagFreeTrigger.StopWithDefaults();
            pagFreeTrigger.Play(new PagSequencePlay(
                PagPlaySpecs.IntroLoop(PagFgPupIn, PagFgPupIdle),
                PagPlayLayout.Center,
                PagPresentationDefaults.DisplayScale,
                useGpuSyncGroup: false));
        }

        private void PlayFgPupOut()
        {
            if (pagFreeTrigger == null) return;
            pagFreeTrigger.StopWithDefaults();
            pagFreeTrigger.Play(new PagSequencePlay(
                new[] { new PagSegment(PagFgPupOut, 1) },
                PagPlayLayout.Center,
                PagPresentationDefaults.DisplayScale,
                useGpuSyncGroup: false,
                callbacks: new PagPlayCallbacks(
                    onFinished: () => pagFreeTrigger?.StopWithDefaults(),
                    stopAfterFinished: true)));
        }

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

        private void RemoveTimer(ref TimerCallback timerCallback)
        {
            if (timerCallback == null) return;
            Timers.inst.Remove(timerCallback);
            timerCallback = null;
        }
    }
}