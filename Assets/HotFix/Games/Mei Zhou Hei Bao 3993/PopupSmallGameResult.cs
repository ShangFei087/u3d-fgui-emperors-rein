using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    public class PopupSmallGameResult : MachinePageBase
    {
        public new const string pkgName = "MeiZhouHeiBao";
        public new const string resName = "PopupSmallGameResult";
        private const string PrefabPath = "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PopupSmallGameResult/PopupSmallGameResult.prefab";
        private const string PagPath = "Games/Mei Zhou Hei Bao 3993/Pag";
        private const string PagSmallPupIn = "small_pup/small_pup_in";
        private const string PagSmallPupIdle = "small_pup/small_pup_idle";
        private const string PagSmallPupOut = "small_pup/small_pup_out";

        //弹窗
        private GameObject goSmallResult;
        private GComponent anchorSmallResult;
        private GameObject clonegoSmallResult;
        private AnimPlayer _animSmallResult;
        private TimerCallback _delayCloseCallback;
        private TimerCallback _autoClickCallback;
        //pag
        private GComponent anchorPagSmallResult;
        private PagSlotBinding pagSmallResult;

        private GButton btnCollect;
        private GTextField txtScoreWin;

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
                 goSmallResult = clone;
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
            //pag
            anchorPagSmallResult = contentPane.GetChild("anchorSmallResultPag").asCom;
            if (pagSmallResult == null) pagSmallResult = new PagSlotBinding("3993pagSmallResult", PagPath);
            pagSmallResult.EnsureSlot(anchorPagSmallResult);
            pagSmallResult.StopWithDefaults();
            pagSmallResult.Play(new PagSequencePlay(
                PagPlaySpecs.IntroLoop(PagSmallPupIn, PagSmallPupIdle),
                PagPlayLayout.Center,
                PagPresentationDefaults.DisplayScale,
                useGpuSyncGroup: false));

            //spine
            GComponent localSmallResult = contentPane.GetChild("anchorSmallResult").asCom;
            if (anchorSmallResult != localSmallResult)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorSmallResult);
                clonegoSmallResult = GameObject.Instantiate(goSmallResult);
                anchorSmallResult = localSmallResult;
                GameCommon.FguiUtils.AddWrapper(anchorSmallResult, clonegoSmallResult);
                _animSmallResult = new AnimPlayer(clonegoSmallResult);
            }
            _animSmallResult.PlayThen("in", "idle", true);

            btnCollect = contentPane.GetChild("btnCollect").asButton;
            btnCollect.touchable = false;
            btnCollect.onClick.Clear();
            btnCollect.onClick.Add(() => OnCloseBtn());


            txtScoreWin = contentPane.GetChild("txtScoreWin").asTextField;
            txtScoreWin.text = "0";


            Timers.inst.Add(0.5f, 1, obj =>
            {
                NumberAnimation.Instance.AnimateNumber(txtScoreWin, 0, ContentModel.Instance.BonusBet + ContentModel.Instance.TotalJackpotBet, 3.0f, EaseType.Linear, () => { });
            });
            Timers.inst.Add(3.5f, 1, obj =>
            {
                if (btnCollect != null) btnCollect.touchable = true;
            });
            const string rootSmallResultPath = "Anchor/Spine Mecanim GameObject (jp_pup_CollectFrame)/SkeletonUtility-SkeletonRoot/root/all";
            _animSmallResult.Attach(
                btnCollect,
                rootSmallResultPath + "/fg_img_Button",
                localPos: new Vector3(-2.3f, 0.75f,0.0f),
                localScale: new Vector3(0.01f, 0.01f, 0.01f),
                localRot: Quaternion.identity);

            _animSmallResult.Attach(
                txtScoreWin,
                rootSmallResultPath + "/base/number",
                localPos: new Vector3(-5.81f, 0.73f, 0.0f),
                localScale: new Vector3(0.01f, 0.01f, 0.01f),
                localRot: Quaternion.identity);

            ScheduleAutoModeClick(4.0f);
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);

            //_gameSoundController = new GameSoundController3993();
            //EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT, new EventData(Game3993AudioEvent.BgmBonusResult));
            InitParam();
        }
        
        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);
            _isClicked = false;
            RemoveTimer(ref _delayCloseCallback);
            RemoveTimer(ref _autoClickCallback);
            _animSmallResult.DetachAll();
            //_gameSoundController?.Dispose();
            //_gameSoundController = null;
        }


        private void OnCloseBtn(EventData eventData = null)
        {
            if (_isClicked) return;
            _isClicked = true;

            btnCollect.touchable = false;
            _animSmallResult.Play("out");
            pagSmallResult.StopWithDefaults();
            pagSmallResult.Play(new PagSequencePlay(
                new[] { new PagSegment(PagSmallPupOut, 1) },
                PagPlayLayout.Center,
                PagPresentationDefaults.DisplayScale,
                useGpuSyncGroup: false,
                callbacks: new PagPlayCallbacks(
                    onFinished: () => pagSmallResult?.StopWithDefaults(),
                    stopAfterFinished: true)));

            RemoveTimer(ref _delayCloseCallback);
            _delayCloseCallback = obj =>
            {
                if (isOpen) CloseSelf(eventData);
                _delayCloseCallback = null;
            };
            Timers.inst.Add(1.0f, 1, _delayCloseCallback);
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