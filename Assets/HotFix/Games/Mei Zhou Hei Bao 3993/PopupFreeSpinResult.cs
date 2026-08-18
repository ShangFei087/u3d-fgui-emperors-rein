using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    public class PopupFreeSpinResult : MachinePageBase
    {
        public new const string pkgName = "MeiZhouHeiBao";
        public new const string resName = "PopupFreeSpinResult";

        private const string PrefabPath = "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PopupFreeSpinResult/PopupFreeSpinResult";
        private const string PagPath = "Games/Mei Zhou Hei Bao 3993/Pag";

        //弹窗
        private GameObject goFreeResult;
        private GComponent anchorFreeResult;
        private GameObject clonegoFreeResult;
        private AnimPlayer _animFreeResult;
        private TimerCallback _delayCloseCallback;
        //pag
        private GComponent anchorPagFreeResult;
        private PagSlotBinding pagFreeResult;

        private GButton btnCollect;
        private GTextField txtTotalFreeTime, txtScoreWin;

        private bool _isClicked;
        private GameSoundController3993 _gameSoundController;

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
                 goFreeResult = clone;
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

            anchorPagFreeResult = contentPane.GetChild("anchorFreeResultPag").asCom;
            if (pagFreeResult == null) pagFreeResult = new PagSlotBinding("3993pagFreeResult", PagPath);
            pagFreeResult.EnsureSlot(anchorPagFreeResult);

            GComponent localFreeResult = contentPane.GetChild("anchorFreeResult").asCom;
            if (anchorFreeResult != localFreeResult)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorFreeResult);
                clonegoFreeResult = GameObject.Instantiate(goFreeResult);
                anchorFreeResult = localFreeResult;
                GameCommon.FguiUtils.AddWrapper(anchorFreeResult, clonegoFreeResult);
                _animFreeResult = new AnimPlayer(clonegoFreeResult);
            }
            _animFreeResult.PlayThen("in", "idle", true);

            btnCollect = contentPane.GetChild("btnCollect").asButton;
            btnCollect.touchable = false;
            btnCollect.onClick.Clear();
            btnCollect.onClick.Add(() => OnCloseBtn());

            txtTotalFreeTime = contentPane.GetChild("txtTotalFreeTime").asTextField;
            txtTotalFreeTime.text = ContentModel.Instance.freeSpinTotalTimes.ToString();

            txtScoreWin = contentPane.GetChild("txtScoreWin").asTextField;
            txtScoreWin.text ="0";


            Timers.inst.Add(0.5f, 1, obj =>
            {
                NumberAnimation.Instance.AnimateNumber(txtScoreWin, 0, ContentModel.Instance.freeSpinTotalWinCredit, 3.0f, EaseType.Linear, () => { });
            });
            Timers.inst.Add(3.5f, 1, obj =>
            {
                if (btnCollect != null) btnCollect.touchable = true;
            });
            const string rootFreeResultPath = "Anchor/Spine Mecanim GameObject (fg_pup_CollectFrame)/SkeletonUtility-SkeletonRoot/root/all";
            _animFreeResult.Attach(
                btnCollect,
                rootFreeResultPath + "/button",
                localPos: new Vector3(),
                localScale: new Vector3(0.01f, 0.01f, 0.01f),
                localRot: Quaternion.identity);

            _animFreeResult.Attach(
                txtTotalFreeTime,
                rootFreeResultPath + "/Base plate/fg_img_FREE GAMES",
                localPos: new Vector3(-2.27f, 0.52f, 0.0f),
                localScale: new Vector3(0.01f, 0.01f, 0.01f),
                localRot: Quaternion.identity);

            _animFreeResult.Attach(
                txtScoreWin,
                rootFreeResultPath + "/Base plate/fg_img_FREE GAMES",
                localPos: new Vector3(-5.81f, 2.9f, 0.0f),
                localScale: new Vector3(0.01f, 0.01f, 0.01f),
                localRot: Quaternion.identity);
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);

            //_gameSoundController = new GameSoundController3993();
            //EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT, new EventData(Game3993AudioEvent.BgmFreeSpinResult));
            InitParam();
        }

        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);
            _isClicked = false;
            RemoveTimer(ref _delayCloseCallback);
            _animFreeResult.DetachAll();
            //_gameSoundController?.Dispose();
            //_gameSoundController = null;
        }

        private void OnCloseBtn(EventData eventData = null)
        {
            if (_isClicked) return;
            _isClicked = true;

            btnCollect.touchable = false;
            _animFreeResult.Play("out");

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