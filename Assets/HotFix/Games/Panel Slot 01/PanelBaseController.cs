using FairyGUI;
using GameMaker;
using Newtonsoft.Json;
using PusherEmperorsRein;
using System;
using System.Collections.Generic;
using TestHall;
using UnityEngine;
using static NetButtonManager;
using SoundKey = GameMaker.SoundKey;


enum PopState
{
    None,
    Change,
    Help,
    Bet,
    payTable,
}

namespace SlotMaker
{
    public class PanelBaseController : MonoBehaviour, IPanel
    {
        PopState popState = PopState.None;
        protected GComponent gOwnerPanel, gIntroducePanel, setPanel, btnSound, btnHelp, Introduce;
        protected SpinButtonBaseController spinBtnCtrl = new SpinButtonBaseController();
        protected GButton btnPayTable, btnPrev, btnNext,btnHome;
        protected GTextField bet,win;
        protected bool isSet;
        protected int PayTableLength =0;

        GameObject goSpin;
        bool isInit;
        /// <summary> 浠嬬粛椤电储寮?</summary>
        public int IntroduceIndex;

        /// <summary> 闊抽噺绛夌骇 </summary>
        public int VolumeLevel;

        protected virtual int IntroduceIndexMax => 6;

        protected virtual void OnEnable()
        {
            EventCenter.Instance.AddEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, OnPropertyChange);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_WIN_EVENT, OnTotalWinCredit);
            EventCenter.Instance.AddEventListener<EventData>(MetaUIEvent.ON_CREDIT_EVENT, OnUpdateNaviCredit);
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_EVENT, OnPanelEventAnchorPanelChange);

            MainModel.Instance.panel = this;

            Init();
        }


        protected virtual void OnDisable()
        {
            EventCenter.Instance.RemoveEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, OnPropertyChange);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_WIN_EVENT, OnTotalWinCredit);
            EventCenter.Instance.RemoveEventListener<EventData>(MetaUIEvent.ON_CREDIT_EVENT, OnUpdateNaviCredit);
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_EVENT, OnPanelEventAnchorPanelChange);
            gOwnerPanel.visible = false;
        }




        public virtual void Init(EventData res = null)
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

            int count = 2;
            Action loadComplete = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam();
                }
            };


            if (gOwnerPanel != _goAnchorPanel && _goAnchorPanel != null)
            {
                if (UIPackage.GetByName("Panel01") == null)
                {
                    ResourceManager02.Instance.LoadAssetBundleAsync("Assets/GameRes/Games/Panel01/FGUIs", (ab) =>
                    {
                        UIPackage.AddPackage(ab);
                        GLoader anchorPanel = _goAnchorPanel.GetChild("icon").asLoader;
                        anchorPanel.url = "ui://Panel01/Panel";
                        gOwnerPanel = _goAnchorPanel.GetChild("icon").asLoader.component;
                        gOwnerPanel.visible = true;
                        loadComplete();
                    });

                }
                else
                {
                    GLoader anchorPanel = _goAnchorPanel.GetChild("icon").asLoader;
                    anchorPanel.url = "ui://Panel01/Panel";

                    gOwnerPanel = _goAnchorPanel.GetChild("icon").asLoader.component;
                    loadComplete();
                }
            }

            ResourceManager02.Instance.LoadAsset<GameObject>("Assets/GameRes/Games/Panel01/Prefabs/Slot_btn_Spin.prefab",
              (GameObject clone) =>
              {
                  goSpin = clone;
                  loadComplete();
              });


        }



        protected virtual void InitParam()
        {
            Debug.Log("初始化菜单Ui");
            gOwnerPanel = MainModel.Instance.contentMD.goAnthorPanel.asCom.GetChild("icon").asLoader.component;
            setPanel = gOwnerPanel.GetChild("setPanel").asCom;
            gOwnerPanel.GetChild("credit").asTextField.text = MainModel.Instance.myCredit.ToString(); //SBoxModel.Instance.myCredit.ToString();
            win = gOwnerPanel.GetChild("win").asTextField;
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
            spinBtnCtrl.InitParam(gOwnerPanel.GetChild("btnSpin").asCom, "Stop", OnClickSpinButton, goSpin);
            btnHelp = gOwnerPanel.GetChild("btnHelp").asCom;
            gIntroducePanel = gOwnerPanel.GetChild("payTable").asCom;
            btnHelp.onTouchBegin.Clear();
            btnHelp.onTouchBegin.Add(() => { btnHelp.SetScale(0.8f, 0.8f); });
            btnHelp.onClick.Clear();
            btnHelp.onClick.Add(() =>
            {
                Help();

            });
            btnPayTable = setPanel.GetChild("btnPayTable").asButton;
            Introduce = gIntroducePanel.GetChild("payTable").asCom;
            //Introduce.AddChild(MainModel.Instance.contentMD.goPayTableLst[IntroduceIndex]);
            btnPrev = gIntroducePanel.GetChild("btnController").asCom.GetChild("btnPrev").asButton;
            btnNext = gIntroducePanel.GetChild("btnController").asCom.GetChild("btnNext").asButton;
            btnPrev.onClick.Clear();
            btnPrev.onClick.Add(OnClickIntroduceL);
            btnNext.onClick.Clear();
            btnNext.onClick.Add(OnClickIntroduceR);
            PayTableLength = MainModel.Instance.contentMD.goPayTableLst.Length;

            btnPayTable.onClick.Clear();
            btnPayTable.onClick.Add(() =>
            {
                IntroduceInit();
                GlobalSoundHelper.Instance.PlaySoundEff(GameMaker.SoundKey.PopupOpen);
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

            //btnHome
            btnHome=setPanel.GetChild("btnHome").asButton;
            btnHome.onClick.Clear();
            btnHome.onClick.Add(() =>
            {
                setPanel.visible = false;
                Help();
                BackHall();
            });
            OnPropertyChangeBetList();
            OnPropertyChangeTotalBet();
            OnPropertyChangeBtnSpinState();
            OnPropertyIsConnectMoneyBox();


        }


        protected virtual void Help()
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

        protected virtual void IntroduceInit()
        {
            IntroduceIndex = 0;
            Introduce.RemoveChildren();
            Introduce.AddChild(MainModel.Instance.contentMD.goPayTableLst[IntroduceIndex]);
            btnPrev.touchable = false;
            btnPrev.GetChild("untouch").visible = true;
            btnNext.touchable = true;
            btnNext.GetChild("untouch").visible = false;
            gIntroducePanel.GetChild("btnController").asCom.GetController("c1").selectedIndex = IntroduceIndex;
        }

        //返回大厅
        protected virtual void BackHall()
        {
            Debug.Log("返回大厅:");
            Debug.Log(MainModel.Instance.gameID);

            switch (MainModel.Instance.gameID)
            {
                case 1700:
                    PageManager.Instance.ClosePage(PageName.SlotZhuZaiJinBiPageGameMain);
                    break;
                case 3999:
                    PageManager.Instance.ClosePage(PageName.CaiFuZhiMenPageGameMain);
                    break;
                case 3998:
                    PageManager.Instance.ClosePage(PageName.XingYunZhiLunPageGameMain);
                    break;
                case 3997:
                    PageManager.Instance.ClosePage(PageName.CaiFuZhiJiaPageGameMain);
                    break;
                case 3996:
                    PageManager.Instance.ClosePage(PageName.CaiFuHuoChePageGameMain);
                    break;
            }

            if (!ApplicationSettings.Instance.isMock)
            {
                PageManager.Instance.OpenPage(PageName.Hall01);
            }
            else
            {
                PageManager.Instance.OpenPage(PageName.HallMain);
            }
              
        }

        protected virtual void OnClickIntroduceL()
        {
            IntroduceChange(false);
            if (IntroduceIndex == 0)
            {
                btnPrev.touchable = false;
                btnPrev.GetChild("untouch").visible = true;


            }
            else
            {

                btnNext.GetChild("untouch").visible = false;
                btnNext.touchable = true;
            }
        }

        protected virtual void OnClickIntroduceR()
        {
            IntroduceChange(true);
            if (IntroduceIndex == PayTableLength-1)
            {
                btnNext.touchable = false;
                btnNext.GetChild("untouch").visible = true;

            }
            else
            {

                btnPrev.touchable = true;
                btnPrev.GetChild("untouch").visible = false;
            }
        }

        protected virtual void IntroduceChange(bool jia)
        {
            if (jia)
            {
                IntroduceIndex += 1;
            }
            else
            {
                IntroduceIndex -= 1;
            }

            if (IntroduceIndex < PayTableLength)
            {
                Introduce.RemoveChildren();
                Introduce.AddChild(MainModel.Instance.contentMD.goPayTableLst[IntroduceIndex]);
                gIntroducePanel.GetChild("btnController").asCom.GetController("c1").selectedIndex = IntroduceIndex;
            }
                
           
           
        }
        protected virtual void OnPropertyChange(EventData res = null)
        {
            //ContentModel
            string name = res.name;
            switch (name)
            {
                case "ContentModel/totalBet":
                    OnPropertyChangeTotalBet(res);
                    break;
                case "SBoxModel/betList":
                    OnPropertyChangeBetList(res);
                    break;
                case "ContentModel/btnSpinState":
                    OnPropertyChangeBtnSpinState(res);
                    break;
                case "ContentModel/gameState":
                    OnPropertyGameState(res);
                    break;
                case "SBoxModel/isConnectMoneyBox":
                    OnPropertyIsConnectMoneyBox(res);
                    break;
            }
        }


        //  panel ctl  --> game ctl  --> model -->  panel ctl
        protected virtual void OnPropertyChangeTotalBet(EventData res = null)
        {
            //long totalBet = (long)res?.value;

            //if (totalBet == null)
            //    totalBet = MainModel.Instance.contentMD.totalBet;
        }

        protected virtual void OnPropertyChangeBetList(EventData res = null)
        {
            List<long> betList = (List<long>)res?.value;

            if (betList == null)
                betList = SBoxModel.Instance.betList;
        }
        protected virtual void OnPropertyChangeBtnSpinState(EventData res = null)
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
                        //if (gOwnerPanel != null)
                        //{
                        //    gOwnerPanel.GetChild("goodLuck").asLoader.visible = false;
                        //    gOwnerPanel.GetChild("win").asCom.visible = true;
                        //}
                        ChangButtonNo(true);
                    }
                    break;
                case SpinButtonState.Auto:
                    {
                        spinBtnCtrl.State = "Auto";
                        //if (gOwnerPanel != null)
                        //{
                        //    gOwnerPanel.GetChild("goodLuck").asLoader.visible = false;
                        //    gOwnerPanel.GetChild("win").asCom.visible = true;
                        //}
                        ChangButtonNo(true);
                    }
                    break;
            }


        }
        protected virtual void OnPropertyGameState(EventData res = null)
        {
            string gameState = (string)res?.value;

            if (gameState == GameState.Spin || gameState == GameState.FreeSpin)
            {
                win.text = 0.ToString();
            }
        }
        protected virtual void OnPropertyIsConnectMoneyBox(EventData res = null)
        {

        }


        protected virtual void OnPanelEventAnchorPanelChange(EventData res = null)
        {
            if (res.name == PanelEvent.AnchorPanelChange)
            {
                Init();
            }
        }


        protected virtual void OnTotalWinCredit(EventData receivedEvent)
        {
            if (receivedEvent.name == SlotMachineEvent.TotalWinCredit)
            {
                long totalWinCredit = (long)receivedEvent.value;
                //gOwnerPanel.GetChild("win").asCom.GetChild("winSound").asTextField.text = totalWinCredit.ToString();
                win.text = totalWinCredit.ToString();
                // uiWin.SetToCredit(totalWinCredit);
            }
            else if (receivedEvent.name == SlotMachineEvent.SingleWinBonus)
            {
                long totalWinCredit = (long)receivedEvent.value;
                NumberAnimation.Instance.AnimateNumber(win,
                                                       long.Parse(win.text),
                                                       totalWinCredit + long.Parse(win.text),
                                                       0.4f);

            }


        }
        protected virtual void OnUpdateNaviCredit(EventData receivedEvent = null)
        {
            if (gOwnerPanel == null) return;

            bool isAmin = false;
            long fromCredit = 0;
            long toCredit = 0;
            if (receivedEvent == null || receivedEvent.value == null)
            {
                isAmin = false;
                toCredit = MainBlackboardController.Instance.myTempCredit;
            }
            else
            {
                UpdateNaviCredit data = (UpdateNaviCredit)receivedEvent.value;

                isAmin = data.isAnim;
                fromCredit = data.fromCredit;
                toCredit = data.toCredit;
            }


            if (isAmin)
            {
                NumberAnimation.Instance.AnimateNumber(gOwnerPanel.GetChild("credit").asTextField, fromCredit, toCredit);
            }
            else
            {
                NumberAnimation.Instance.PauseTextFieldAnimation(gOwnerPanel.GetChild("credit").asTextField);
                if (gOwnerPanel.GetChild("credit").asTextField != null)
                {
                    gOwnerPanel.GetChild("credit").asTextField.text = toCredit.ToString();
                }
            }
        }



        // Spin鎸夐挳
        //public void OnLongClickSpinButton(string customDataOrState) => OnClickSpinButton(true);
        //public void OnShortClickSpinButton(string customDataOrState) => OnClickSpinButton(false);


        int i = 0;
        public void OnClickSpinButton(bool isLong)
        {

            EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT,
               new EventData<bool>(PanelEvent.SpinButtonClick, isLong));

        }

        #region 置灰
        public virtual void ChangButtonNo(bool can)
        {
            if (can)
            {
                //gOwnerPanel.GetChild("ButtonPRIZE").asButton.GetChild("n1").visible = true;
                //gOwnerPanel.GetChild("ButtonPRIZE").asButton.touchable = false;
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
                //gOwnerPanel.GetChild("ButtonPRIZE").asButton.GetChild("n1").visible = false;
                //gOwnerPanel.GetChild("ButtonPRIZE").asButton.touchable = true;
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
        #endregion


        protected virtual void OnClickButtonBetUp()
        {
            GlobalSoundHelper.Instance.PlaySoundEff(SoundKey.BetUp);

            //soundHelper.PlaySoundEff(GameMaker.SoundKey.BetUp);
            List<long> betList = SBoxModel.Instance.betList;
            int betIndex = MainModel.Instance.contentMD.betIndex;
            if (++betIndex >= betList.Count)
            {

                betIndex = betList.Count - 1;
            }
            MainModel.Instance.contentMD.totalBet = betList[betIndex];
            ChangeBetButtonInteractable(betIndex, betList.Count);
        }

        protected virtual void OnClickButtonBetDown()
        {
            GlobalSoundHelper.Instance.PlaySoundEff(SoundKey.BetDown);

            //soundHelper.PlaySoundEff(GameMaker.SoundKey.BetDown);
            List<long> betList = SBoxModel.Instance.betList;

            int betIndex = MainModel.Instance.contentMD.betIndex;
            if (--betIndex < 0)
            {
                betIndex = 0;
            }
            MainModel.Instance.contentMD.totalBet = betList[betIndex];

            ChangeBetButtonInteractable(betIndex, betList.Count);
        }
        protected GButton btnBetDown, btnBetUp;

        protected int curBetIndex = 0;
        protected int curBetListCount = 1;
        protected virtual void ChangeBetButtonInteractable(int? betIndex01 = null, int? betListCount01 = null)
        {

            if (betIndex01 != null && betListCount01 != null)
            {
                curBetIndex = (int)betIndex01;
                curBetListCount = (int)betListCount01;
            }
            MainModel.Instance.contentMD.betIndex = curBetIndex;

            bet.text = MainModel.Instance.contentMD.totalBet.ToString();
            btnBetDown.touchable = curBetIndex > 0;
            btnBetDown.GetChild("untouch").visible = btnBetDown.touchable ? false : true;
            btnBetUp.touchable = curBetIndex < curBetListCount - 1;
            btnBetUp.GetChild("untouch").visible = btnBetUp.touchable ? false : true;
        }

        public virtual void OnLongClickHandler(MachineButtonKey machineButtonKey) { }

        public virtual void OnShortClickHandler(MachineButtonKey machineButtonKey) { }

        public virtual void OnDownClickHandler(MachineButtonKey machineButtonKey)
        {
            switch (machineButtonKey)
            {
                case MachineButtonKey.BtnSpin:
                    {

                    }
                    break;
            }
        }

        public virtual void OnUpClickHandler(MachineButtonKey machineButtonKey)
        {
            switch (machineButtonKey)
            {
                case MachineButtonKey.BtnSpin:
                    {

                    }
                    break;
            }
        }
    }
}
