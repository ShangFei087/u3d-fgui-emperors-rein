using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using UnityEngine;
using Sirenix.OdinInspector;
namespace PusherEmperorsRein
{
    public class PanelController02 : PusherMaker.PanelBaseController
    {
        new SpinController02 spinBtnCtrl = new SpinController02();
        //bool isLoad;
        public override void Init(EventData res = null)
        {
            if (PlayerPrefsUtils.isTestDeletePanel)
                return;

            /*
            GComponent _goAnchorPanel = null;
            if (res != null)
                _goAnchorPanel = res.value as GComponent;
            else if (MainModel.Instance.contentMD != null)
                _goAnchorPanel = MainModel.Instance.contentMD.goAnthorPanel;

            if (_goAnchorPanel == null)
            {

                return;
            }
            */
            int count = 2;
            Action loadComplete = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam();
                }
            };

            if (UIPackage.GetByName("EmperorsRein") == null)
            {
                ResourceManager02.Instance.LoadAssetBundleAsync("Assets/GameRes/Games/Emperors Rein 200/FGUIs", (ab) =>
                {
                    UIPackage.AddPackage(ab);
                    //GLoader anchorPanel = _goAnchorPanel.GetChild("icon").asLoader;
                    //anchorPanel.url = "ui://EmperorsRein/Panel2";
                    //gOwnerPanel = anchorPanel.component;
                    loadComplete();
                });
            }
            else
            {
                //GLoader anchorPanel = _goAnchorPanel.GetChild("icon").asLoader;
                //anchorPanel.url = "ui://EmperorsRein/Panel2";
                //gOwnerPanel = anchorPanel.component;
                loadComplete();
            }


            ResourceManager02.Instance.LoadAsset<GameObject>("Assets/GameRes/Games/Emperors Rein 200/Prefabs/Spin/Push_btn_Spin.prefab",
           (GameObject clone) =>
           {
               goSpinClone = clone;
               loadComplete();
           });


            /*
            if (isLoad)
            {
                loadComplete();
            }
            else
            {
                ResourceManager02.Instance.LoadAsset<GameObject>("Assets/GameRes/Games/Emperors Rein 200/Prefabs/Spin/Push_btn_Spin.prefab",
                   (GameObject clone) =>
                   {
                       goSpinClone = clone;
                       loadComplete();
                       isLoad = true;
                   });
            }*/
        }



        [Button]
        void TestHasEmperorsRein()
        {
            string pkgName = "Emperors Rein 200";
            
            if (UIPackage.GetByName(pkgName) == null)
            {
                Debug.LogWarning($"{pkgName} is not find");
            }
            else
            {
                Debug.LogWarning($"{pkgName} is find");
            }

            pkgName = "EmperorsRein";

            if (UIPackage.GetByName(pkgName) != null)
            {
                Debug.LogWarning($"{pkgName} is find");
            }
            else
            {
                Debug.LogWarning($"{pkgName} is not find");
            }
        }


        protected override void OnEnable()
        {
            base.OnEnable();

            //DebugUtils.LogError("init GlobalEvent.ON_REMOTE_CONSOL_EVENT 2");//new EventData()
            EventCenter.Instance.AddEventListener<EventData>(GlobalEvent.ON_REMOTE_CONSOL_EVENT, OnRemoteConsoleWvent);
            AddNetCmdHandle();
        }

        protected override void OnDisable()
        {
            EventCenter.Instance.RemoveEventListener<EventData>(GlobalEvent.ON_REMOTE_CONSOL_EVENT, OnRemoteConsoleWvent);
            RemoveNetCmdHandle();

            base.OnDisable();

        }



        void OnRemoteConsoleWvent(EventData res) {
            if (res.name == GlobalEvent.GetRemoteConsoleConfigFinish)
            {
                if (gOwnerPanel != null)
                    gOwnerPanel.GetChild("bet").asTextField.text = $"{SBoxModel.Instance.CoinInScale.ToString()}:1";
            }
        }

        protected override void InitParam(EventData data = null)
        {

            if (PlayerPrefsUtils.isTestDeletePanel)
                return;


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


            if (goAnchorPanel != _goAnchorPanel || gOwnerPanel == null)
            {
                goAnchorPanel = _goAnchorPanel;

                GLoader anchorPanel = _goAnchorPanel.GetChild("icon").asLoader;
                anchorPanel.url = "ui://EmperorsRein/Panel2";
                gOwnerPanel = anchorPanel.component;
                gOwnerPanel.visible = true;
            }




            //DebugUtils.Log($"===================== 面板获取到的玩家分数：{SBoxModel.Instance.myCredit}");

            gOwnerPanel.GetChild("credit").asTextField.text = MainModel.Instance.myCredit.ToString();  //SBoxModel.Instance.myCredit.ToString();
            gOwnerPanel.GetChild("win").asTextField.text = 0.ToString();
            PlayTime = gOwnerPanel.GetChild("btnBet").asButton;
            PlayTime.onClick.Clear();
            PlayTime.onClick.Add(() =>
            {
                if (ApplicationSettings.Instance.isMock == false)
                    return;
                OnClickTotalSpinsButtonClick();
            });
            setPanel = gOwnerPanel.GetChild("SetPanel").asCom;

            //DebugUtils.LogError("GlobalEvent.ON_REMOTE_CONSOL_EVENT 55");

            gOwnerPanel.GetChild("bet").asTextField.text = $"{SBoxModel.Instance.CoinInScale.ToString()}:1";
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
            VolumeLevel = -1;
            btnPayTable.onClick.Add(() =>
            {
                /*
                if (Introduce == null)
                {
                    IntroduceInit();

                }*/
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
               /* VolumeLevel += 1;
                if (VolumeLevel == 4)
                {
                    VolumeLevel = 0;
                }
                switch (VolumeLevel)
                {
                    case 0:
                        SBoxModel.Instance.soundLevel = 0;
                        break;
                    case 1:
                        SBoxModel.Instance.soundLevel = 1;
                        break;
                    case 2:
                        SBoxModel.Instance.soundLevel = 2;
                        break;
                    case 3:
                        SBoxModel.Instance.soundLevel = 3;
                        break;
                }
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

        protected override void IntroduceInit()
        {
            Introduce = payTable.GetChild("payTable").asCom;
            Introduce.AddChild(MainModel.Instance.contentMD.goPayTableLst[IntroduceIndex]);
            btnPrev = payTable.GetChild("btnController").asCom.GetChild("btnPrev").asButton;
            btnNext = payTable.GetChild("btnController").asCom.GetChild("btnNext").asButton;
            btnPrev.onClick.Clear();
            btnPrev.onClick.Add(OnClickIntroduceL);
            btnNext.onClick.Clear();
            btnNext.onClick.Add(OnClickIntroduceR);
            btnExit = payTable.GetChild("btnExit").asButton;
            btnExit.onClick.Clear();
            btnExit.onClick.Add(() =>
            {
                spinBtnCtrl.State = SpinButtonState.Stop;
                GlobalSoundHelper.Instance.PlaySoundEff(GameMaker.SoundKey.PopupClose);
                isSet = !isSet;
                btnPayTable.touchable = true;
                gOwnerPanel.GetChild("payTable").asCom.visible = false;
                gOwnerPanel.GetChild("mash").asGraph.visible = false;
                btnHelp.GetController("button").selectedPage = "Help";
                gOwnerPanel.GetChild("btnSpin").asCom.touchable = true;
                isOpenIntroduce = false;
            });
        }

        public override void ChangButtonNo(bool can)
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


        protected override void OnPropertyChangeBtnSpinState(EventData res = null)
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
                case SpinButtonState.Auto:
                    {
                        spinBtnCtrl.State = "Auto";
                        ChangButtonNo(true);
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


        public override void OnDownClickHandler(MachineButtonKey machineButtonKey)
        {

            if (PlayerPrefsUtils.isTestDeletePanel)
                return;

            switch (machineButtonKey)
            {
                case MachineButtonKey.BtnPayTable:
                    {
                        if (btnPayTable.touchable == false || isOpenIntroduce == true)
                        {
                            return;
                        }
                        btnPayTable.SetScale(0.8f, 0.8f);
                    }
                    break;
                case MachineButtonKey.BtnSpin:
                    {
                        spinBtnCtrl.goOwnerSpin.onTouchBegin.Call();
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



        public override void OnUpClickHandler(MachineButtonKey machineButtonKey)
        {

            if (PlayerPrefsUtils.isTestDeletePanel)
                return;

            switch (machineButtonKey)
            {
                case MachineButtonKey.BtnPayTable:
                    {
                        if (btnPayTable.touchable == false || isOpenIntroduce == true)
                        {
                            return;
                        }
                        btnPayTable.SetScale(1, 1);
                        IntroduceRest();
                        payTable.visible = true;
                        gOwnerPanel.GetChild("mash").asGraph.visible = true;
                    }
                    break;
                case MachineButtonKey.BtnSpin:
                    {
                        if (isOpenIntroduce == true)
                        {
                            return;
                        }
                        spinBtnCtrl.goOwnerSpin.onTouchEnd.Call();
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


        protected override void Help()
        {
            GlobalSoundHelper.Instance.PlaySoundEff(GameMaker.SoundKey.Tab);
            btnHelp.SetScale(1f, 1f);
            isSet = !isSet;
            if (isSet)
            {
                setPanel.visible = true;
                btnHelp.GetController("button").selectedPage = "Back";
                gOwnerPanel.GetChild("mash").asGraph.visible = true;
                spinBtnCtrl.State = "Hui";
                gOwnerPanel.GetChild("btnSpin").asCom.touchable = false;
            }
            else
            {

                setPanel.visible = false;
                payTable.visible = false;
                gOwnerPanel.GetChild("mash").asGraph.visible = false;
                btnHelp.GetController("button").selectedPage = "Help";
                spinBtnCtrl.State = "Stop";
                gOwnerPanel.GetChild("btnSpin").asCom.touchable = true;
            }
        }








#if !false //网络命令

        const string MARK_PUSHER_PANEL_CONTROLLER = "MARK_PUSHER_PANEL_CONTROLLER";

        void AddNetCmdHandle()
        {
            NetCmdManager.Instance.AddHandles(new NetCmdHandle()
            {
                cmdName = NetCmdManager.CMD_TOTAL_SPINS,
                mark = MARK_PUSHER_PANEL_CONTROLLER,
                onInvoke = OnNetCmdTotalSpins,
            });

        }

        void RemoveNetCmdHandle() => NetCmdManager.Instance.ReomveHandles(MARK_PUSHER_PANEL_CONTROLLER);

        void OnNetCmdTotalSpins(NetCmdInfo info)
        {
            int totalSpinsCount = (int)info.data;

            /*
             if (MainModel.Instance.isSpin)
             {
                 info.onCallback(new object[] { 1, $"请求失败，推币机{SBoxModel.Instance.MachineId}在游玩中", totalSpinsCount });
             }
            */


            Timers.inst.Remove(DoTotalSpin);

            if (totalSpinsCount <= 0 || totalSpinsCount > 99)
            {
                info.onCallback(new object[] { 1, $"请求失败，num = {totalSpinsCount} 参数有误， 取整范围： 0~99", totalSpinsCount, });
            }
            else if (MainModel.Instance.isSpin)
            {
                MainModel.Instance.contentMD.isRequestToStop = true;

                Timers.inst.Add(0.1f, 0, DoTotalSpin , info);
            }
            else
            {
                //OnClickTotalSpinsButtonClick(totalSpinsCount);
                //info.onCallback(new object[] { 0, "", totalSpinsCount });
                DoTotalSpin(info);
            }
        }

        void DoTotalSpin(object data)
        {
            if (MainModel.Instance.contentMD.isSpin) return;

            Timers.inst.Remove(DoTotalSpin);

            NetCmdInfo info = data as NetCmdInfo;
            int totalSpinsCount = (int)info.data;
            OnClickTotalSpinsButtonClick(totalSpinsCount);
            info.onCallback(new object[] { 0, "", totalSpinsCount });

        }
    }

#endif



}

