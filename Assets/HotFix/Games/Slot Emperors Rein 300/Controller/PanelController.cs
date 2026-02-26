using FairyGUI;
using GameMaker;
using slotEmperorsRein;
using SlotMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SlotEmperorsRein
{
    public class PanelController : PanelBaseController
    {
        new SpinButtonController spinBtnCtrl = new SpinButtonController();

        protected override void OnEnable()
        {
            EventCenter.Instance.AddEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, OnPropertyChange);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_WIN_EVENT, OnTotalWinCredit);
            EventCenter.Instance.AddEventListener<EventData>(MetaUIEvent.ON_CREDIT_EVENT, OnUpdateNaviCredit);
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_EVENT, OnPanelEventAnchorPanelChange);

            MainModel.Instance.panel = this;

            Init();
        }
        public override void Init(EventData res = null)
        {
            GComponent _goAnchorPanel = null;
            if (res != null)
                _goAnchorPanel = res.value as GComponent;
            else if (MainModel.Instance.contentMD != null)
                _goAnchorPanel = MainModel.Instance.contentMD.goAnthorPanel;

            if (_goAnchorPanel == null)
            {

                return;
            }


            if (gOwnerPanel != _goAnchorPanel && _goAnchorPanel != null)
            {
                if (UIPackage.GetByName("EmperorsRein") == null)
                {
                    ResourceManager02.Instance.LoadAssetBundleAsync("Assets/GameRes/Games/Emperors Rein 200/FGUIs", (ab) =>
                    {
                        UIPackage.AddPackage(ab);
                        GLoader anchorPanel = _goAnchorPanel.GetChild("icon").asLoader;
                        anchorPanel.url = "ui://EmperorsRein/Panel";
                        gOwnerPanel = _goAnchorPanel.GetChild("icon").asLoader.component;
                        InitParam();
                    });

                }
                else
                {
                    GLoader anchorPanel = _goAnchorPanel.GetChild("icon").asLoader;
                    anchorPanel.url = "ui://EmperorsRein/Panel";

                    gOwnerPanel = _goAnchorPanel.GetChild("icon").asLoader.component;

                    InitParam();
                }
            }

        }


        protected override void InitParam()
        {
            gOwnerPanel = MainModel.Instance.contentMD.goAnthorPanel.asCom.GetChild("icon").asLoader.component;
            setPanel = gOwnerPanel.GetChild("setPanel").asCom;
            gOwnerPanel.GetChild("credit").asTextField.text = MainModel.Instance.myCredit.ToString();  //SBoxModel.Instance.myCredit.ToString();
            win = gOwnerPanel.GetChild("win").asCom.GetChild("win").asTextField;
            win.text = 0.ToString();
            btnBetUp = gOwnerPanel.GetChild("btnBetUp").asButton;
            btnBetUp.onClick.Clear();
            btnBetUp.onClick.Add(OnClickButtonBetUp);
            btnBetDown = gOwnerPanel.GetChild("btnBetDown").asButton;
            btnBetDown.touchable = false;
            btnBetDown.GetChild("untouch").visible = true;
            btnBetDown.onClick.Clear();
            btnBetDown.onClick.Add(OnClickButtonBetDown);
            bet = gOwnerPanel.GetChild("bet").asTextField;
            bet.text = SBoxModel.Instance.betList[MainModel.Instance.contentMD.betIndex].ToString();
            spinBtnCtrl.InitParam(gOwnerPanel.GetChild("btnSpin").asCom, "Stop", OnClickSpinButton);
            btnHelp = gOwnerPanel.GetChild("btnHelp").asCom;
            gIntroducePanel = gOwnerPanel.GetChild("payTable").asCom;
            btnHelp.onTouchBegin.Clear();
            btnHelp.onTouchBegin.Add(() => { btnHelp.SetScale(0.8f, 0.8f); });
            btnHelp.onClick.Clear();
            btnHelp.onClick.Add(() =>
            {
                Help();
            });
            Introduce = gIntroducePanel.GetChild("payTable").asCom;
            Introduce.AddChild(MainModel.Instance.contentMD.goPayTableLst[IntroduceIndex]);
            btnPrev = gIntroducePanel.GetChild("btnController").asCom.GetChild("btnPrev").asButton;
            btnNext = gIntroducePanel.GetChild("btnController").asCom.GetChild("btnNext").asButton;
            btnPrev.onClick.Clear();
            btnPrev.onClick.Add(OnClickIntroduceL);
            btnNext.onClick.Clear();
            btnNext.onClick.Add(OnClickIntroduceR);
            btnPayTable = setPanel.GetChild("btnPayTable").asButton;
            btnPayTable.onClick.Clear();
            btnPayTable.onClick.Add(() =>
            {
                IntroduceInit();
                gIntroducePanel.visible = true;
                setPanel.visible = false;
            });
            btnSound = setPanel.GetChild("btnSound").asCom;
            btnSound.onTouchBegin.Clear();
            btnSound.onTouchBegin.Add(() => { btnSound.SetScale(0.8f, 0.8f); });
            btnSound.onTouchEnd.Clear();
            btnSound.onTouchEnd.Add(() =>
            {
                btnSound.SetScale(1f, 1f);
                VolumeLevel += 1;
                if (VolumeLevel == 4)
                {
                    VolumeLevel = 0;
                }
                switch (VolumeLevel)
                {

                    case 0:
                        //闊抽噺涓夌骇
                        break;
                    case 1:
                        //闊抽噺浜岀骇
                        break;
                    case 2:
                        //闊抽噺涓€绾ustomModel.Instance.VolumeLevel
                        break;
                    case 3:
                        //鏃犻煶閲?
                        break;
                }

                btnSound.GetController("button").selectedIndex = VolumeLevel;
                GlobalSoundHelper.Instance.PlaySoundEff(GameMaker.SoundKey.NormalClick);
            });
            btnSound.GetController("button").selectedIndex = 3;

            OnPropertyChangeBetList();
            OnPropertyChangeTotalBet();
            OnPropertyChangeBtnSpinState();
            OnPropertyIsConnectMoneyBox();
        }

        protected override void Help()
        {
            btnHelp.SetScale(1f, 1f);
            isSet = !isSet;
            if (isSet)
            {
                setPanel.visible = true;
                btnHelp.GetController("button").selectedPage = "Back";
                gOwnerPanel.GetChild("mash").asGraph.visible = true;
                spinBtnCtrl.goOwnerSpin.GetController("button").selectedPage = "hui";
                spinBtnCtrl.goOwnerSpin.touchable = false;
            }
            else
            {

                setPanel.visible = false;
                gIntroducePanel.visible = false;
                gOwnerPanel.GetChild("mash").asGraph.visible = false;
                btnHelp.GetController("button").selectedPage = "Help";
                spinBtnCtrl.goOwnerSpin.GetController("button").selectedPage = "stop";
                spinBtnCtrl.goOwnerSpin.touchable = true;
            }
        }

        public override void ChangButtonNo(bool can)
        {
            if (can)
            {
                gOwnerPanel.GetChild("btnPRIZE").asButton.GetChild("untouch").visible = true;
                gOwnerPanel.GetChild("btnPRIZE").asButton.touchable = false;
                btnHelp.GetChild("untouch").visible = true;
                btnHelp.touchable = false;
                btnBetUp.GetChild("untouch").visible = true;
                btnBetUp.touchable = false;
                btnBetDown.GetChild("untouch").visible = true;
                btnBetDown.touchable = false;

                // ChangeBetButtonInteractable(MainModel.Instance.contentMD.betIndex, SBoxModel.Instance.betList.Count);
            }
            else
            {
                gOwnerPanel.GetChild("btnPRIZE").asButton.GetChild("untouch").visible = false;
                gOwnerPanel.GetChild("btnPRIZE").asButton.touchable = true;
                btnHelp.GetChild("untouch").visible = false;
                btnHelp.touchable = true;
                btnBetUp.GetChild("untouch").visible = false;
                btnBetUp.touchable = true;
                btnBetDown.GetChild("untouch").visible = false;
                btnBetDown.touchable = true;

                if (MainModel.Instance.contentMD.betIndex == 0)
                {
                    btnBetDown.GetChild("untouch").visible = true;
                    btnBetDown.touchable = false;
                }

                if (MainModel.Instance.contentMD.betIndex == 7)
                {
                    btnBetUp.GetChild("untouch").visible = true;
                    btnBetUp.touchable = false;
                }
            }
        }

        protected override void OnPropertyChangeBtnSpinState(EventData res = null)
        {
            string changeSpinState = (string)res?.value;

            if (changeSpinState == null)
                changeSpinState = "Stop";

            if (gOwnerPanel == null) return;


            switch (changeSpinState)
            {
                case SpinButtonState.Stop:
                    {
                        spinBtnCtrl.State = "Stop";
                        ChangButtonNo(false);
                    }
                    break;
                case SpinButtonState.Spin:
                    {
                        spinBtnCtrl.State = "Spin";
                        if (gOwnerPanel != null)
                        {
                            gOwnerPanel.GetChild("goodLuck").asLoader.visible = false;
                            gOwnerPanel.GetChild("win").asCom.visible = true;
                        }
                        ChangButtonNo(true);
                    }
                    break;
                case SpinButtonState.Auto:
                    {
                        spinBtnCtrl.State = "Auto";
                        if (gOwnerPanel != null)
                        {
                            gOwnerPanel.GetChild("goodLuck").asLoader.visible = false;
                            gOwnerPanel.GetChild("win").asCom.visible = true;
                        }
                        ChangButtonNo(true);
                    }
                    break;
            }
        }
    }
}

