using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace PusherMaker
{
   
    public class PanelBaseController : MonoBehaviour, IPanel
    {


        protected bool isInit = false;
        protected EventData _data = null;

        protected GComponent gOwnerPanel, payTable, Introduce, btnHelp, setPanel, btnSound;
        protected SpinButtonBaseController spinBtnCtrl = new SpinButtonBaseController();
        protected GButton btnPrev, btnNext, btnExit, PlayTime, btnPayTable;
        protected GameObject goSpinClone;
        protected bool isSet;
        /// <summary> 说明页索引?</summary>
        protected int IntroduceIndex, VolumeLevel;
        public static bool isOpenIntroduce;
        protected virtual int IntroduceIndexMax => 6;


        // 面板的锚点对象
        protected GComponent goAnchorPanel;

        protected void Awake()
        {
             Init();           
        }
        protected virtual void OnEnable()
        {
            //AddNetButtonHandle();

            EventCenter.Instance.AddEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, OnPropertyChange);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_WIN_EVENT, OnTotalWinCredit);
            EventCenter.Instance.AddEventListener<EventData>(MetaUIEvent.ON_CREDIT_EVENT, OnUpdateNaviCredit);
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_EVENT, OnPanelEventAnchorPanelChange);

            MainModel.Instance.panel = this;

        }


        protected virtual void OnDisable()
        {
            //RemoveNetButtonHandle();
            EventCenter.Instance.RemoveEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, OnPropertyChange);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_WIN_EVENT, OnTotalWinCredit);
            EventCenter.Instance.RemoveEventListener<EventData>(MetaUIEvent.ON_CREDIT_EVENT, OnUpdateNaviCredit);
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_EVENT, OnPanelEventAnchorPanelChange);
            gOwnerPanel.visible = false;
        }


        public virtual void Init(EventData res = null)
        {

            int count = 2;
            Action loadComplete = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam();
                }
            };

            if (UIPackage.GetByName("Panel02") == null)
            {
                ResourceManager02.Instance.LoadAssetBundleAsync("Assets/GameRes/Games/Panel02/FGUIs", (ab) =>
                {
                    UIPackage.AddPackage(ab);
                    //GLoader anchorPanel = _goAnchorPanel.GetChild("icon").asLoader;
                    //anchorPanel.url = "ui://Panel02/Panel";
                    //gOwnerPanel = anchorPanel.component;
                    //gOwnerPanel.visible = true;
                    loadComplete();
                });


            }
            else
            {
                //GLoader anchorPanel = _goAnchorPanel.GetChild("icon").asLoader;
                //anchorPanel.url = "ui://Panel02/Panel";
                //gOwnerPanel = anchorPanel.component;
                loadComplete();
            }

            ResourceManager02.Instance.LoadAsset<GameObject>("Assets/GameRes/Games/Panel01/Prefabs/Slot_btn_Spin.prefab",
            (GameObject clone) =>
            {
                goSpinClone = clone;
                loadComplete();
            });
        }


  
        protected virtual void InitParam(EventData data = null)
        {

            if (data != null) _data = data;

            if (!isInit) return;


            GComponent _goAnchorPanel = null;
            if (_data != null)
                _goAnchorPanel = _data.value as GComponent;
            else if (MainModel.Instance.contentMD != null)
                _goAnchorPanel = MainModel.Instance.contentMD.goAnthorPanel;

            if (_goAnchorPanel == null)
            {
                return;
            }

            if(goAnchorPanel != _goAnchorPanel || gOwnerPanel == null)
            {
                goAnchorPanel = _goAnchorPanel;

                GLoader anchorPanel = _goAnchorPanel.GetChild("icon").asLoader;
                anchorPanel.url = "ui://Panel02/Panel";  // 内存泄漏？？
                gOwnerPanel = anchorPanel.component;
                gOwnerPanel.visible = true;
            }



            //gOwnerPanel = MainModel.Instance.contentMD.goAnthorPanel.asCom.GetChild("icon").asLoader.component;
            gOwnerPanel.GetChild("credit").asTextField.text = MainModel.Instance.myCredit.ToString();     //SBoxModel.Instance.myCredit.ToString();
            gOwnerPanel.GetChild("win").asTextField.text = 0.ToString();
            //PlayTime = gOwnerPanel.GetChild("buttonBet").asButton;
            //PlayTime.onClick.Clear();
            //PlayTime.onClick.Add(() => {
            //    OnClickTotalSpinsButtonClick(false)
            //    ;
            //});
            setPanel = gOwnerPanel.GetChild("setPanel").asCom;
            gOwnerPanel.GetChild("bet").asTextField.text = SBoxModel.Instance.CoinInScale.ToString();
            spinBtnCtrl.InitParam(gOwnerPanel.GetChild("btnSpin").asCom, "Stop", OnClickSpinButton, goSpinClone);


            btnHelp = gOwnerPanel.GetChild("btnHelp").asCom;
            payTable = gOwnerPanel.GetChild("payTable").asCom;
            btnHelp.onTouchBegin.Clear();
            btnHelp.onTouchBegin.Add(() => { btnHelp.SetScale(0.8f, 0.8f); });
            btnHelp.onClick.Clear();
            btnHelp.onClick.Add(() =>
            {
                Help();
            });
            btnPayTable = setPanel.GetChild("btnPayTable").asButton;
            btnPayTable.onClick.Clear();
            btnPayTable.onClick.Add(() =>
            {
                IntroduceRest();
                GlobalSoundHelper.Instance.PlaySoundEff(GameMaker.SoundKey.PopupOpen);
                payTable.visible = true;
                setPanel.visible = false;
            });
            IntroduceInit();


            btnSound = setPanel.GetChild("btnSound").asCom;
            btnSound.onTouchBegin.Clear();
            btnSound.onTouchBegin.Add(() => { btnSound.SetScale(0.8f, 0.8f); });
            btnSound.onTouchEnd.Clear();
            btnSound.onTouchEnd.Add(() =>
            {
                btnSound.SetScale(1f, 1f);

                /*
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
                */

                if (++SBoxModel.Instance.soundLevel > 3)
                    SBoxModel.Instance.soundLevel = 0;
                btnSound.GetController("button").selectedIndex = SBoxModel.Instance.soundLevel;
                GlobalSoundHelper.Instance.PlaySoundEff(GameMaker.SoundKey.NormalClick);

            });
            btnSound.GetController("button").selectedIndex = SBoxModel.Instance.soundLevel;

            OnPropertyChangeBetList();
            OnPropertyChangeTotalBet();
            OnPropertyChangeBtnSpinState();
            OnPropertyIsConnectMoneyBox();
        }




        protected virtual void Help()
        {
            GlobalSoundHelper.Instance.PlaySoundEff(GameMaker.SoundKey.Tab);
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
                payTable.visible = false;
                gOwnerPanel.GetChild("mash").asGraph.visible = false;
                btnHelp.GetController("button").selectedPage = "Help";
                spinBtnCtrl.goOwnerSpin.GetController("button").selectedPage = "stop";
                spinBtnCtrl.goOwnerSpin.touchable = true;
            }
        }





        protected virtual void IntroduceInit()
        {
            Introduce = payTable.GetChild("payTable").asCom;
            Introduce.RemoveChildren();
            Introduce.AddChild(MainModel.Instance.contentMD.goPayTableLst[IntroduceIndex]);
            btnPrev = payTable.GetChild("btnController").asCom.GetChild("btnPrev").asButton;
            btnExit = payTable.GetChild("btnExit").asButton;
            btnExit.onClick.Clear();
            btnExit.onClick.Add(() =>
            {
                spinBtnCtrl.State = SpinButtonState.Stop;
                GlobalSoundHelper.Instance.PlaySoundEff(GameMaker.SoundKey.PopupClose);
                btnHelp.GetChild("untouch").visible = false;
                btnHelp.touchable = true;
                isSet = !isSet;
                gOwnerPanel.GetChild("payTable").asCom.visible = false;
                gOwnerPanel.GetChild("mash").asGraph.visible = false;
                btnHelp.GetController("button").selectedPage = "Help";
                isOpenIntroduce = false;
            });
            btnNext = payTable.GetChild("btnController").asCom.GetChild("btnNext").asButton;
            btnPrev.onClick.Clear();
            btnPrev.onClick.Add(OnClickIntroduceL);
            btnNext.onClick.Clear();
            btnNext.onClick.Add(OnClickIntroduceR);
        }

        //void ChangPlayTime()
        //{
        //    if (PlayTime.GetController("button").selectedIndex == 0)
        //    {
        //        PlayTime.GetController("button").SetSelectedIndex(1);
        //        ContentModel.Instance.remainPlaySpins = 3;
        //    }
        //    else if(PlayTime.GetController("button").selectedIndex == 1)
        //    {
        //        PlayTime.GetController("button").SetSelectedIndex(2);
        //        ContentModel.Instance.remainPlaySpins = 5;
        //    }
        //    else if (PlayTime.GetController("button").selectedIndex == 2)
        //    {
        //        PlayTime.GetController("button").SetSelectedIndex(0);
        //        ContentModel.Instance.remainPlaySpins = 1;
        //    }

        //}

        /// <summary>
        /// 重置打开说明页
        /// </summary>
        protected virtual void IntroduceRest()
        {
            IntroduceIndex = 0;
            isOpenIntroduce = true;
            Introduce.RemoveChildren();
            Introduce.AddChild(MainModel.Instance.contentMD.goPayTableLst[IntroduceIndex]);
            btnPrev.touchable = false;
            btnPrev.GetChild("untouch").visible = true;
            btnNext.touchable = true;
            btnNext.GetChild("untouch").visible = false;
            payTable.GetChild("btnController").asCom.GetController("c1").selectedIndex = IntroduceIndex;
        }

        protected virtual void OnClickIntroduceL()
        {

            IntroduceChange(false);
            GlobalSoundHelper.Instance.PlaySoundEff(GameMaker.SoundKey.NormalClick);
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
            GlobalSoundHelper.Instance.PlaySoundEff(GameMaker.SoundKey.NormalClick);
            if (IntroduceIndex == IntroduceIndexMax)
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
            Introduce.RemoveChildren();
            Introduce.AddChild(MainModel.Instance.contentMD.goPayTableLst[IntroduceIndex]);
            payTable.GetChild("btnController").asCom.GetController("c1").selectedIndex = IntroduceIndex;
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
            {
                changeSpinState = "Stop";
            }


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
                        ChangButtonNo(true);
                    }
                    break;
                case SpinButtonState.Hui:
                    {
                        spinBtnCtrl.State = "hui";
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
                gOwnerPanel.GetChild("win").asTextField.text = 0.ToString();
            }
        }
        protected virtual void OnPropertyIsConnectMoneyBox(EventData res = null)
        {

        }


        protected virtual void OnPanelEventAnchorPanelChange(EventData res = null)
        {
            if (res.name == PanelEvent.AnchorPanelChange)
            {
                //Init();
                InitParam(res);
            }
        }


        protected virtual void OnTotalWinCredit(EventData receivedEvent)
        {
            if (receivedEvent.name == SlotMachineEvent.TotalWinCredit)
            {
                long totalWinCredit = (long)receivedEvent.value;
                gOwnerPanel.GetChild("win").asTextField.text = totalWinCredit.ToString();
                // uiWin.SetToCredit(totalWinCredit);
            }
            else if (receivedEvent.name == SlotMachineEvent.SingleWinBonus)
            {
                long totalWinCredit = (long)receivedEvent.value;
                NumberAnimation.Instance.AnimateNumber(gOwnerPanel.GetChild("win").asTextField,
                                                       long.Parse(gOwnerPanel.GetChild("win").asTextField.text),
                                                       totalWinCredit + long.Parse(gOwnerPanel.GetChild("win").asTextField.text),
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



        public void OnClickSpinButton(bool isLong)
        {

            EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT,
               new EventData<bool>(PanelEvent.SpinButtonClick, isLong));
        }


        public void OnClickTotalSpinsButtonClick(int num = -1)
        {
            EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT,
               new EventData<int>(PanelEvent.TotalSpinsButtonClick, num));
        }

        #region 置灰
        public virtual void ChangButtonNo(bool can)
        {
            if (can)
            {
                btnHelp.GetChild("untouch").visible = true;
                btnHelp.touchable = false;
                //gOwnerPanel.GetChild("buttonBet").asButton.GetChild("n7").visible = true;
                //gOwnerPanel.GetChild("buttonBet").asButton.touchable = false;

            }
            else
            {
                btnHelp.GetChild("untouch").visible = false;
                btnHelp.touchable = true;
                //gOwnerPanel.GetChild("buttonBet").asButton.GetChild("n7").visible = false;
                //gOwnerPanel.GetChild("buttonBet").asButton.touchable = true;
            }

        }

        #endregion



        public void OnLongClickHandler(MachineButtonKey machineButtonKey) { }

        public void OnShortClickHandler(MachineButtonKey machineButtonKey)
        {


        }

        public virtual void OnDownClickHandler(MachineButtonKey machineButtonKey)
        {
            switch (machineButtonKey)
            {
                case MachineButtonKey.BtnPayTable:
                    {
                        if (btnHelp.touchable == false || isOpenIntroduce == true)
                        {
                            return;
                        }
                        btnHelp.SetScale(0.8f, 0.8f);
                    }
                    break;
                //case MachineButtonKey.BtnBetDown:
                //    {
                //        if (btnBetDown.touchable == false || isOpenIntroduce == true)
                //        {
                //            return;
                //        }
                //        btnBetDown.SetScale(0.8f, 0.8f);
                //    }
                //    break;
                //case MachineButtonKey.BtnBetUp:
                //    {
                //        if (btnBetUp.touchable == false || isOpenIntroduce == true)
                //        {
                //            return;
                //        }
                //        btnBetUp.SetScale(0.8f, 0.8f);
                //    }
                //    break;
                case MachineButtonKey.BtnPrev:
                    {
                        if (btnPrev.touchable == false)
                        {
                            return;
                        }
                        btnPrev.SetScale(0.8f, 0.8f);
                    }
                    break;
                case MachineButtonKey.BtnNext:
                    {
                        if (btnNext.touchable == false)
                        {
                            return;
                        }
                        btnNext.SetScale(0.8f, 0.8f);
                    }
                    break;
                case MachineButtonKey.BtnExit:
                    {
                        btnExit.SetScale(0.8f, 0.8f);
                    }
                    break;
                case MachineButtonKey.BtnPlayTime:
                    {
                        if (PlayTime.touchable == false)
                        {
                            return;
                        }
                        PlayTime.SetScale(1f, 1f);
                    }
                    break;
            }

        }

        public virtual void OnUpClickHandler(MachineButtonKey machineButtonKey)
        {
            switch (machineButtonKey)
            {
                case MachineButtonKey.BtnPayTable:
                    {
                        if (btnHelp.touchable == false || isOpenIntroduce == true)
                        {
                            return;
                        }
                        btnHelp.SetScale(1, 1);
                        Help();
                    }
                    break;
                case MachineButtonKey.BtnSpin:
                    {
                        if (isOpenIntroduce == true)
                        {
                            return;
                        }
                        GlobalSoundHelper.Instance.PlaySoundEff(GameMaker.SoundKey.SpinClick);
                    }
                    break;
                case MachineButtonKey.BtnPrev:
                    {
                        if (btnPrev.touchable == false)
                        {
                            return;
                        }
                        btnPrev.SetScale(1, 1);
                        btnPrev.FireClick(false, true);
                    }
                    break;
                case MachineButtonKey.BtnNext:
                    {
                        if (btnNext.touchable == false)
                        {
                            return;
                        }
                        btnNext.SetScale(1, 1);
                        btnNext.FireClick(false, true);
                    }
                    break;
                case MachineButtonKey.BtnExit:
                    {
                        btnExit.SetScale(1, 1);
                        btnExit.FireClick(false, true);
                    }
                    break;
                case MachineButtonKey.BtnPlayTime:
                    {
                        if (PlayTime.touchable == false)
                        {
                            return;
                        }
                        PlayTime.SetScale(1, 1);
                        PlayTime.FireClick(false, true);
                    }
                    break;

            }

        }



        #region 网络远程按钮
        const string MARK_NET_BTN_PUSHER_PANEL_BASE = "MARK_NET_BTN_PUSHER_PANEL_BASE";
        void AddNetButtonHandle()
        {
            NetButtonManager.Instance.AddHandles(new NetButtonHandle()
            {
                buttonName = NetButtonManager.BtnName.BtnPayTable,
                mark = MARK_NET_BTN_PUSHER_PANEL_BASE,
                onClick = OnNetBtnTable,
            });
            NetButtonManager.Instance.AddHandles(new NetButtonHandle()
            {
                buttonName = NetButtonManager.BtnName.BtnPrev,
                mark = MARK_NET_BTN_PUSHER_PANEL_BASE,
                onClick = OnNetBtnPrev,
            });
            NetButtonManager.Instance.AddHandles(new NetButtonHandle()
            {
                buttonName = NetButtonManager.BtnName.BtnNext,
                mark = MARK_NET_BTN_PUSHER_PANEL_BASE,
                onClick = OnNetBtnNext,
            });
            NetButtonManager.Instance.AddHandles(new NetButtonHandle()
            {
                buttonName = NetButtonManager.BtnName.BtnExit,
                mark = MARK_NET_BTN_PUSHER_PANEL_BASE,
                onClick = OnNetBtnExit,
            });
            NetButtonManager.Instance.AddHandles(new NetButtonHandle()
            {
                buttonName = NetButtonManager.BtnName.BtnSpin,
                mark = MARK_NET_BTN_PUSHER_PANEL_BASE,
                onClick = OnNetBtnSpin,
            });

            NetButtonManager.Instance.AddHandles(new NetButtonHandle()
            {
                buttonName = NetButtonManager.BtnName.BtnBetUp,
                mark = MARK_NET_BTN_PUSHER_PANEL_BASE,
                onClick = OnNetBtnBetUp,
            });
            /*
            NetButtonManager.Instance.AddHandles(new NetButtonHandle()
            {
                buttonName = NetButtonManager.BTN_BET_DOWN,
                mark = MARK_NET_BTN_PUSHER_PANEL_BASE,
                onClick = OnNetBtnBetDown,
            });

            NetButtonManager.Instance.AddHandles(new NetButtonHandle()
            {
                buttonName = NetButtonManager.BTN_AUTO,
                mark = MARK_NET_BTN_PUSHER_PANEL_BASE,
                onClick = OnNetBtnAuto,
            });*/

        }

        void RemoveNetButtonHandle() => NetButtonManager.Instance.ReomveHandles(MARK_NET_BTN_PUSHER_PANEL_BASE);


        void OnNetBtnSpin(NetButtonInfo info)
        {
            if (info.dataType != NetButtonManager.DATA_MACHINE_BUTTON_CONTROL) return;
            if (PageManager.Instance.IndexOf(MainModel.Instance.contentMD.pageName) != 0) return;


            NetButtonManager.Instance.ShowUIAminButtonClick(() =>
            {
                OnDownClickHandler(MachineButtonKey.BtnSpin);
            }, () => {
                OnUpClickHandler(MachineButtonKey.BtnSpin);
            }, MARK_NET_BTN_PUSHER_PANEL_BASE, NetButtonManager.BtnName.BtnSpin);
            // OnUpClickHandler(MachineButtonKey.BtnSpin);

            info.onCallback?.Invoke(true);
        }



        void OnNetBtnNext(NetButtonInfo info) { }

        void OnNetBtnTable(NetButtonInfo info) { }


        void OnNetBtnPrev(NetButtonInfo info) { }


        void OnNetBtnExit(NetButtonInfo info) { }

        void OnNetBtnBetUp(NetButtonInfo info) { }

        #endregion

    }
}
