using FairyGUI;
using GameMaker;
using Newtonsoft.Json;
using PusherEmperorsRein;
using SBoxApi;
using SimpleJSON;
using SlotMaker;
using System;
using System.Collections.Generic;
using UnityEngine;
using static SBoxApi.SBoxSandbox;
namespace Hall01
{
    public class Hall01GameMain : MachinePageBase
    {
        public const string pkgName = "Hall01";
        public const string resName = "GameMain";

        private GameObject goCard3999, goCard3998, goCard3997;
        private GComponent anchorCard3999, anchorCard3998, anchorCard3997;
        private GameObject ClonegoCard3999, ClonegoCard3998, ClonegoCard3997;
        private Animator animator3999, animator3998, animator3997;
        GButton btn3999, btn3998, btn3997;
        private GTextField hallCredit;

        //彩金
        MiniReelGroup uiJPMajorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMinorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMiniCtrl = new MiniReelGroup();
        protected override void OnInit()
        {

            base.OnInit();

            int count = 3;
            //GameSoundHelper.Instance.PlayMusicSingle(SoundKey.RegularBG);
            Action callback = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam();
                }
            };

            ResourceManager02.Instance.LoadAsset<GameObject>("Assets/GameRes/Halls/Hall01/Prefabs/card/card_3999",
               (GameObject clone) =>
               {
                    goCard3999 = clone;
                    callback();
               });

            // 异步加载资源
            ResourceManager02.Instance.LoadAsset<GameObject>("Assets/GameRes/Halls/Hall01/Prefabs/card/card_3998",
            (GameObject clone) =>
            {
                goCard3998 = clone;
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>("Assets/GameRes/Halls/Hall01/Prefabs/card/card_3997",
                (GameObject clone) =>
                {
                    goCard3997 = clone;
                    callback();
                });

            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnTicketOut] = (info) =>
                    {
                        if (PanelController02.isOpenIntroduce == true)
                        {
                            return;
                        }

                        Debug.LogError("游戏接受到机台短按的数据：BtnTicketOut");
                        //EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, false); // isLongClick
                        OnClickBtnTicketOut();
                    },
                },

            };
        }

        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            GameSoundHelper.Instance.PlayMusicSingle(SoundKey.RegularBG);
            // 添加事件监听 - 彩金贡献值
            EventCenter.Instance.AddEventListener<string>(SBoxEventHandle.SBOX_COIN_PUSH_GET_LOCAL_JP_CONTRIBUTION, OnGetJackpotGameShow);
            EventCenter.Instance.AddEventListener<SBoxIdeaInfo>(SBoxEventHandle.SBOX_IDEA_INFO, OnSboxIdeaInfo);
            EventCenter.Instance.AddEventListener<EventData>(MetaUIEvent.ON_CREDIT_EVENT, OnUpdateNaviCredit);
            InitParam();
        }

        public override void OnClose(EventData data = null)
        {
            EventCenter.Instance.RemoveEventListener<string>(SBoxEventHandle.SBOX_COIN_PUSH_GET_LOCAL_JP_CONTRIBUTION, OnGetJackpotGameShow);
            EventCenter.Instance.RemoveEventListener<SBoxIdeaInfo>(SBoxEventHandle.SBOX_IDEA_INFO, OnSboxIdeaInfo);
            EventCenter.Instance.RemoveEventListener<EventData>(MetaUIEvent.ON_CREDIT_EVENT, OnUpdateNaviCredit);
            GameSoundHelper.Instance.StopMusic();
            base.OnClose(data);
        }

        public override void InitParam()
        {

            if (!isInit) return;

            if (!isOpen) return;

            GComponent LocalCard3999 = this.contentPane.GetChild("card3999").asCom;
            if (anchorCard3999 != LocalCard3999)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorCard3999);
                ClonegoCard3999 = GameObject.Instantiate(goCard3999);
                animator3999 = ClonegoCard3999.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                anchorCard3999 = LocalCard3999;
                GameCommon.FguiUtils.AddWrapper(anchorCard3999, ClonegoCard3999);

            }

            GComponent LocalCard3998 = this.contentPane.GetChild("card3998").asCom;
            if (anchorCard3998 != LocalCard3998)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorCard3998);
                ClonegoCard3998 = GameObject.Instantiate(goCard3998);
                //animator3998 = ClonegoCard3998.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                anchorCard3998 = LocalCard3998;
                GameCommon.FguiUtils.AddWrapper(anchorCard3998, ClonegoCard3998);

            }


            GComponent LocalCard3997 = this.contentPane.GetChild("card3997").asCom;
            if (anchorCard3997 != LocalCard3997)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorCard3997);
                ClonegoCard3997 = GameObject.Instantiate(goCard3997);
                animator3997 = ClonegoCard3997.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                anchorCard3997 = LocalCard3997;
                GameCommon.FguiUtils.AddWrapper(anchorCard3997, ClonegoCard3997);
            }


            btn3999 = this.contentPane.GetChild("card3999").asCom.GetChild("btnCard3999").asButton;
            btn3999.onClick.Add(() =>
            {
                if (!ApplicationSettings.Instance.isMock)
                {
                    SBoxIdea.GameSwitch(3999);
                }
                PageManager.Instance.OpenPage(PageName.CaiFuZhiMenPopupGameLoading);
                CloseSelf(null);
            });

            btn3998 = this.contentPane.GetChild("card3998").asCom.GetChild("btnCard3998").asButton;
            btn3998.onClick.Add(() =>
            {
                if (!ApplicationSettings.Instance.isMock)
                {
                    SBoxIdea.GameSwitch(3998);
                }
                PageManager.Instance.OpenPage(PageName.XingYunZhiLunPopupGameLoading);
                CloseSelf(null);
            });

            btn3997 = this.contentPane.GetChild("card3997").asCom.GetChild("btnCard3997").asButton;
            btn3997.onClick.Add(() =>
            {
                if (!ApplicationSettings.Instance.isMock)
                {
                    SBoxIdea.GameSwitch(1700);
                }
                  
                PageManager.Instance.OpenPage(PageName.SlotZhuZaiJinBiPopupGameLoading);
                CloseSelf(null);
            });

            uiJPMajorCtrl.Init("Major", this.contentPane.GetChild("jpMajor").asCom.GetChild("reels").asList, "N0");
            uiJPMinorCtrl.Init("Minor", this.contentPane.GetChild("jpMinor").asCom.GetChild("reels").asList, "N0");
            uiJPMiniCtrl.Init("Mini", this.contentPane.GetChild("jpMini").asCom.GetChild("reels").asList, "N0");

            GComponent Credit = this.contentPane.GetChild("Credit").asCom;
            hallCredit = Credit.GetChild("credit").asTextField;

            InitJackpot();
            InitHallCredit();
        }

        /// <summary>
        /// 获取彩金贡献值（通过SBOX_COIN_PUSH_GET_JP_CONTRIBUTION事件）
        /// </summary>
        void OnGetJackpotGameShow(string res)
        {
            //Debug.LogError("大厅获取彩金贡献值");
        }

        public void InitJackpot()
        {
            if (ApplicationSettings.Instance.isMock)
            {
                uiJPMajorCtrl.SetData(30000);
                uiJPMinorCtrl.SetData(1000);
                uiJPMiniCtrl.SetData(500);
            }
            else
            {
                //获取彩金贡献值
                ERPushMachineDataManager02.Instance.RequestGetJpContribution((res) =>
                {
                   
                    JSONNode data = JSONNode.Parse((string)res);
                    Debug.Log(data);
                    int code = (int)data["code"];
                    if (0 != code)
                    {
                        DebugUtils.LogError($"请求贡献值报错。 code: {code}");
                        return;
                    }

                    int majorBet = (int)data["major"];
                    int minorBet = (int)data["minor"];
                    int miniBet = (int)data["mini"];

                    uiJPMajorCtrl.SetData(majorBet);
                    uiJPMinorCtrl.SetData(minorBet);
                    uiJPMiniCtrl.SetData(miniBet);

                });
            }
        }

        //积分监听
        protected virtual void OnUpdateNaviCredit(EventData receivedEvent = null)
        {

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
                NumberAnimation.Instance.AnimateNumber(hallCredit, fromCredit, toCredit);
            }
            else
            {
                NumberAnimation.Instance.PauseTextFieldAnimation(hallCredit);
                if (hallCredit != null)
                {
                    hallCredit.text = toCredit.ToString();
                }
            }
        }

        public void InitHallCredit()
        {
            //初始化积分与同步
            MachineDataManager02.Instance.RequestGetPlayerInfo((res) =>
            {
                SBoxAccount data = (SBoxAccount)res;
                int pid = SBoxModel.Instance.pid;
                List<SBoxPlayerAccount> playerAccountList = data.PlayerAccountList;
                for (int i = 0; i < playerAccountList.Count; i++)
                {
                    if (playerAccountList[i].PlayerId == pid)
                    {
                        MainBlackboardController.Instance.SetMyRealCredit(playerAccountList[i].Credit);
                        MainBlackboardController.Instance.SyncMyTempCreditToReal(false);
                        hallCredit.text = playerAccountList[i].Credit.ToString();
                        break;
                    }
                }

            }, (BagelCodeError err) =>
            {

                DebugUtils.Log(err.msg);
            });
          
        }

        public void GameSwitch(int gameid)
        {
            
        }

        void OnSboxIdeaInfo(SBoxIdeaInfo info)
        {

        }

        private void OnClickBtnTicketOut()
        {
            MachineDeviceCommonBiz.Instance.TestTicketOut();
        }
    }

}
