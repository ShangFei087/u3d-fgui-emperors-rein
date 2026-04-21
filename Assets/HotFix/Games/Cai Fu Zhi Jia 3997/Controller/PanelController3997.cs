using FairyGUI;
using GameMaker;
using PusherEmperorsRein;
using SBoxApi;
using SlotMaker;
using System;
using UnityEngine;

namespace CaiFuZhiJia_3997
{
    public class PanelController3997 : PanelBaseController
    {
        private GameObject _goSpin;

        public override void Init(EventData res = null)
        {
            GComponent goAnchorPanel = null;
            if (res != null)
                goAnchorPanel = res.value as GComponent;
            else if (MainModel.Instance.contentMD != null)
                goAnchorPanel = MainModel.Instance.contentMD.goAnthorPanel;

            if (goAnchorPanel == null) return;

            int count = 2;
            Action loadComplete = () =>
            {
                if (--count == 0) InitParam();
            };

            if (gOwnerPanel != goAnchorPanel && goAnchorPanel != null)
            {
                if (UIPackage.GetByName("CaiFuZhiJia") == null)
                {
                    ResourceManager02.Instance.LoadAssetBundleAsync("Assets/GameRes/Games/Cai Fu Zhi Jia 3997/FGUIs",
                        (ab) =>
                        {
                            UIPackage.AddPackage(ab);
                            GLoader anchorPanel = goAnchorPanel.GetChild("icon").asLoader;
                            anchorPanel.url = "ui://CaiFuZhiJia/Panel";
                            gOwnerPanel = goAnchorPanel.GetChild("icon").asLoader.component;
                            gOwnerPanel.visible = true;
                            loadComplete();
                        });
                }
                else
                {
                    GLoader anchorPanel = goAnchorPanel.GetChild("icon").asLoader;
                    anchorPanel.url = "ui://CaiFuZhiJia/Panel";

                    gOwnerPanel = goAnchorPanel.GetChild("icon").asLoader.component;
                    loadComplete();
                }
            }

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Panel01/Prefabs/Slot_btn_Spin.prefab",
                (clone) =>
                {
                    _goSpin = clone;
                    loadComplete();
                });
        }

        protected override void InitParam()
        {
            Debug.Log("初始化财富之家_3997菜单Ui");
            gOwnerPanel = MainModel.Instance.contentMD.goAnthorPanel.asCom.GetChild("icon").asLoader.component;
            setPanel = gOwnerPanel.GetChild("setPanel").asCom;
            gOwnerPanel.GetChild("credit").asTextField.text =
                MainModel.Instance.myCredit.ToString();
            win = gOwnerPanel.GetChild("win").asTextField;
            win.text = 0.ToString();
            btnBetUp = gOwnerPanel.GetChild("btnBetUp").asButton;
            btnBetUp.onClick.Clear();
            btnBetUp.onClick.Add(OnClickButtonBetUp);
            btnBetDown = gOwnerPanel.GetChild("btnBetDown").asButton;
            btnBetDown.touchable = false;
            // btnBetDown.GetChild("untouch").visible = true;
            btnBetDown.onClick.Clear();
            btnBetDown.onClick.Add(OnClickButtonBetDown);
            bet = gOwnerPanel.GetChild("bet").asTextField;
            bet.text = SBoxModel.Instance.betList[MainModel.Instance.contentMD.betIndex].ToString();

            singleLine = gOwnerPanel.GetChild("singleLine").asTextField;
            singleLine.text = "";

            SBoxPlayerBetsData sBoxPlayerBetsData = new SBoxPlayerBetsData()
            {
                PlayerId = SBoxModel.Instance.pid, balance = 0, rfu = 0
            };

            sBoxPlayerBetsData.Bets[0] = (int)SBoxModel.Instance.betList[MainModel.Instance.contentMD.betIndex];
            // 设置押注
            ERPushMachineDataManager02.Instance.RequestSetBet(sBoxPlayerBetsData, (res) =>
            {
                ChangeBetButtonInteractable(MainModel.Instance.contentMD.betIndex, SBoxModel.Instance.betList.Count);
            });

            spinBtnCtrl.InitParam(gOwnerPanel.GetChild("btnSpin").asCom, "Stop", OnClickSpinButton, _goSpin);
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
            btnSound.onTouchBegin.Add(() =>
            {
                _isSoundBtnPressed = true;
                btnSound.SetScale(0.8f, 0.8f);
            });
            btnSound.onTouchEnd.Clear();
            btnSound.onTouchEnd.Add(() =>
            {
                _isSoundBtnPressed = false;
                btnSound.SetScale(1f, 1f);
            });
            btnSound.onClick.Clear();
            btnSound.onClick.Add(OnClickSoundButton);
            //鼠标抬起事件
            Stage.inst.onTouchEnd.Remove(OnStageTouchEndResetSoundButton);
            Stage.inst.onTouchEnd.Add(OnStageTouchEndResetSoundButton);
            btnSound.GetController("button").selectedIndex = 3;

            //btnHome
            btnHome = setPanel.GetChild("btnHome").asButton;
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

        protected override void Help()
        {
            btnHelp.SetScale(1f, 1f);
            isSet = !isSet;
            if (isSet)
            {
                setPanel.visible = true;
                // btnHelp.GetController("button").selectedPage = "Back";
                gOwnerPanel.GetChild("mash").asGraph.visible = true;
                spinBtnCtrl.goOwnerSpin.GetController("button").selectedPage = "hui";
                spinBtnCtrl.goOwnerSpin.touchable = false;
            }
            else
            {
                setPanel.visible = false;
                gIntroducePanel.visible = false;
                gOwnerPanel.GetChild("mash").asGraph.visible = false;
                // btnHelp.GetController("button").selectedPage = "Help";
                spinBtnCtrl.goOwnerSpin.GetController("button").selectedPage = "stop";
                spinBtnCtrl.goOwnerSpin.touchable = true;
            }
        }

        protected override void ChangeBetButtonInteractable(int? betIndex01 = null, int? betListCount01 = null)
        {
            if (betIndex01 != null && betListCount01 != null)
            {
                curBetIndex = (int)betIndex01;
                curBetListCount = (int)betListCount01;
            }

            MainModel.Instance.contentMD.betIndex = curBetIndex;
            //下注倍数现在硬数据,之后在改动  
            MainModel.Instance.contentMD.betmultiple =
                (int)MainModel.Instance.contentMD.totalBet / MainModel.Instance.lineNum;
            bet.text = MainModel.Instance.contentMD.totalBet.ToString();
            btnBetDown.touchable = curBetIndex > 0;
            // btnBetDown.GetChild("untouch").visible = btnBetDown.touchable ? false : true;
            btnBetUp.touchable = curBetIndex < curBetListCount - 1;
            // btnBetUp.GetChild("untouch").visible = btnBetUp.touchable ? false : true;
        }


        public override void ChangButtonNo(bool can)
        {
            if (can)
            {
                //gOwnerPanel.GetChild("ButtonPRIZE").asButton.GetChild("n1").visible = true;
                //gOwnerPanel.GetChild("ButtonPRIZE").asButton.touchable = false;
                // btnHelp.GetChild("untouch").visible = true;
                btnHelp.touchable = false;
                // btnBetUp.GetChild("untouch").visible = true;
                btnBetUp.touchable = false;
                // btnBetDown.GetChild("untouch").visible = true;
                btnBetDown.touchable = false;

                // ChangeBetButtonInteractable(MainModel.Instance.contentMD.betIndex, SBoxModel.Instance.betList.Count);
            }
            else
            {
                //gOwnerPanel.GetChild("ButtonPRIZE").asButton.GetChild("n1").visible = false;
                //gOwnerPanel.GetChild("ButtonPRIZE").asButton.touchable = true;
                // btnHelp.GetChild("untouch").visible = false;
                btnHelp.touchable = true;
                // btnBetUp.GetChild("untouch").visible = false;
                btnBetUp.touchable = true;
                // btnBetDown.GetChild("untouch").visible = false;
                btnBetDown.touchable = true;

                if (MainModel.Instance.contentMD.betIndex == 0)
                {
                    // btnBetDown.GetChild("untouch").visible = true;
                    btnBetDown.touchable = false;
                }

                if (MainModel.Instance.contentMD.betIndex == 7)
                {
                    // btnBetUp.GetChild("untouch").visible = true;
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

                        ChangButtonNo(true);
                    }
                    break;
                case SpinButtonState.Auto:
                    {
                        spinBtnCtrl.State = "Auto";

                        ChangButtonNo(true);
                    }
                    break;
            }
        }

        protected override void OnPropertyGameState(EventData res = null)
        {
            string gameState = (string)res?.value;

            if (gameState == GameState.Spin)
            {
                win.text = 0.ToString();
                ClearSingleLineText();
            }
            else if (gameState == GameState.FreeSpin)
            {
                ClearSingleLineText();
            }
        }
    }
}