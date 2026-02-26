using ConsoleCoinPusher01;
using FairyGUI;
using GameMaker;
using GlobalJackpotConsole;
using Newtonsoft.Json;
using SBoxApi;
using SimpleJSON;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using _spinWEMD = SlotMaker.SpinWinEffectSettingModel;

namespace PusherEmperorsRein
{
   
    public class GameConfigRoot
    {
        [JsonProperty("game_id")] public int GameId;

        [JsonProperty("game_name")] public string GameName;

        [JsonProperty("display_name")] public string DisplayName;

        [JsonProperty("win_level_multiple")] public Dictionary<string, long> WinLevelMultiple { get; set; }

        [JsonProperty("symbol_paytable")] public Dictionary<string, PayTableSymbolInfo> SymbolPaytable { get; set; }

        [JsonProperty("pay_lines")] public List<List<int>> pay_lines { get; set; }
    }

    public class PageGameMain : MachinePageBase //: PageBase
    {
        public const string pkgName = "EmperorsRein";
        public const string resName = "PageGameMain";



        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnPanelInputEvent);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT, OnSlotDetailEvent);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnStopSlot);

            EventCenter.Instance.AddEventListener<string>(MachineDataManager02.RpcNameJackpotOnLine, OnJackpotOnLine);

            
            EventCenter.Instance.AddEventListener<SBoxIdeaInfo>(SBoxEventHandle.SBOX_IDEA_INFO, OnSboxIdeaInfo);


            EventCenter.Instance.AddEventListener<string>(GlobalEvent.ON_TEST_EVENT, OnTestEvent);




            EventCenter.Instance.AddEventListener<EventData>(GlobalEvent.ON_GAME_COIN_PUSH_EVENT, OnGetJackpotGameShow);





            GameSoundHelper.Instance.PlayMusicSingle(SoundKey.RegularBG);
            GameSoundHelper.Instance.PlaySoundEff(SoundKey.SpinBGIdle);



            InitParam();
        }



        public override void OnClose(EventData data = null)
        {
            Timers.inst.Remove(GetMyCreditRepeat);

           //winTipCtrl.Dispose();
           //FreeSpinTimeController.Dispose();
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnPanelInputEvent);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT,
                OnSlotDetailEvent);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnStopSlot);
            EventCenter.Instance.RemoveEventListener<string>(MachineDataManager02.RpcNameJackpotOnLine,
                OnJackpotOnLine);
            EventCenter.Instance.RemoveEventListener<SBoxIdeaInfo>(SBoxEventHandle.SBOX_IDEA_INFO, OnSboxIdeaInfo);



            EventCenter.Instance.RemoveEventListener<string>(GlobalEvent.ON_TEST_EVENT, OnTestEvent);

            EventCenter.Instance.RemoveEventListener<EventData>(GlobalEvent.ON_GAME_COIN_PUSH_EVENT, OnGetJackpotGameShow);

            base.OnClose(data);
        }


        protected override void OnInit()
        {
            
            base.OnInit();

            PageConsoleCheckHardware02.InitHardwaveTest();

            int count = 14;

            Action callback = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam();
                }
            };


            if (UIPackage.GetByName("Common") == null)
            {
                count++;
                ResourceManager02.Instance.LoadAssetBundleAsync("Assets/GameRes/Games/Common/FGUIs", (bundle) =>
                {
                    callback();
                });
            }

            // Diction<string,Action> 加载资源



            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Game Controller/Push Game Main Controller.prefab",
                (GameObject clone) =>
                {
                    goGameCtrl = GameObject.Instantiate(clone);

                    //Debug.LogError("创建 Push Game Main Controller");

                    goGameCtrl.name = "Game Main Controller";
                    goGameCtrl.transform.SetParent(null);

                    slotMachineCtrl = goGameCtrl.transform.Find("Slot Machine").GetComponent<SlotMachineController>();

                    mono = goGameCtrl.transform.GetComponent<MonoHelper>();
                    //DebugUtils.Log(mono);
                    //DebugUtils.LogWarning("i am Game Controller");


                   // DebugUtils.LogError("A ContentModel = " + goGameCtrl.transform.Find("Blackboard/Content Model").GetComponent<ContentModel>().transform.name);



                    fguiPoolHelper = goGameCtrl.transform.Find("Pool").GetComponent<FguiPoolHelper>();

                    gObjectPoolHelper = goGameCtrl.transform.Find("GObject Pool").GetComponent<FguiGObjectPoolHelper>();

                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/PageERGameMain/KingLion.prefab",
                (GameObject clone) =>
                {
                    goNpc = clone;
                    //animatorKingLion = goNpc.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                    callback();
                });



            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/PageERGameMain/anchorNPCQiang.prefab",
                (GameObject clone) =>
                {
                    goNpc2 = clone;
                    //  animatorKing = goNpc2.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                    callback();
                });



            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/PageERGameMain/BgQiang.prefab",
                (GameObject clone) =>
                {
                    goBg2 = clone;
                    callback();
                });




            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/PageERGameMain/Bg0.prefab",
                (GameObject clone) =>
                {
                    goBg = clone;
                    callback();
                });



            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/PageERGameMain/Qi.prefab",
                (GameObject clone) =>
                {
                    goQi = clone;
                    callback();
                });



            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/PageERGameMain/anchorWin02.prefab",
                (GameObject clone) =>
                {
                    goGoldCoin = clone;
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/PageERGameMain/anchorWin01.prefab",
                (GameObject clone) =>
                {
                    goDi = clone;
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/PageERGameMain/ReelEffect.prefab",
                (GameObject clone) =>
                {
                    goReelEffcet = clone;
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/PageERGameMain/FsEffect01.prefab",
                (GameObject clone) =>
                {
                    goFsEffect01 = clone;
                    callback();
                });


            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/PageERGameMain/GoldBack.prefab",
                (GameObject clone) =>
                {
                    goGoldBack = clone;
                    callback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(
              "Assets/GameRes/Games/Emperors Rein 200/Prefabs/PageERGameMain/CollectGoldCoin.prefab",
              (GameObject clone) =>
              {
                  goCollectGoldCoin = clone;
                  callback();
              });


            ResourceManager02.Instance.LoadAsset<TextAsset>(
                ConfigUtils.GetGameInfoURL(200), (txt) =>
                {
                    gameInfo = txt;
                    //DebugUtils.LogWarning($"游戏数据  = {txt.text} ");
                    callback();
                });


            // 主动获取玩家数据(获取一次)
            ERPushMachineDataManager02.Instance.RequestGetMyCredit((res) =>
            {
                try
                {
                    int myCredit = (int)res;
                    SBoxModel.Instance.myCredit = myCredit;

                    //新加
                    MainBlackboardController.Instance.SyncMyTempCreditToReal(true);
                }
                catch (Exception ex)
                {
                    DebugUtils.LogError(ex);
                }

                callback();

            });



            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                //shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                //{
                //    [MachineButtonKey.BtnSpin] = (info) =>
                //    {
                //        if (PanelController02.isOpenIntroduce == true)
                //        {
                //            return;
                //        }

                //        //DebugUtils.LogError("游戏接受到机台短按的数据：Spin");
                //        EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, false); // isLongClick
                //        OnClickSpinButton(res);

                //    },
                //    [MachineButtonKey.BtnSwitch] = (info) =>
                //    {
                //        //DebugUtils.LogError("游戏接受到机台短按的数据：Switch");
                //        NetClineBiz.LoginJpConsoleBiz();
                //    }
                //},
                downClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnPayTable] = (info) =>
                    {
                        MainModel.Instance.panel?.OnDownClickHandler(MachineButtonKey.BtnPayTable);
                    },
                    [MachineButtonKey.BtnExit] = (info) =>
                    {
                        MainModel.Instance.panel?.OnDownClickHandler(MachineButtonKey.BtnExit);
                    },
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        MainModel.Instance.panel?.OnDownClickHandler(MachineButtonKey.BtnSpin);
                    },
                    [MachineButtonKey.BtnPrev] = (info) =>
                    {
                        MainModel.Instance.panel?.OnDownClickHandler(MachineButtonKey.BtnPrev);
                    },
                    [MachineButtonKey.BtnNext] = (info) =>
                    {
                        MainModel.Instance.panel?.OnDownClickHandler(MachineButtonKey.BtnNext);
                    },
                    [MachineButtonKey.BtnBetDown] = (info) =>
                    {
                        MainModel.Instance.panel?.OnDownClickHandler(MachineButtonKey.BtnBetDown);
                    },
                    [MachineButtonKey.BtnBetUp] = (info) =>
                    {
                        MainModel.Instance.panel?.OnDownClickHandler(MachineButtonKey.BtnBetUp);
                    },
                },
                upClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnPayTable] = (info) =>
                    {
                        MainModel.Instance.panel?.OnUpClickHandler(MachineButtonKey.BtnPayTable);
                    },
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        MainModel.Instance.panel?.OnUpClickHandler(MachineButtonKey.BtnSpin);
                    },
                    [MachineButtonKey.BtnExit] = (info) =>
                    {
                        MainModel.Instance.panel?.OnUpClickHandler(MachineButtonKey.BtnExit);
                    },
                    [MachineButtonKey.BtnPrev] = (info) =>
                    {
                        MainModel.Instance.panel?.OnUpClickHandler(MachineButtonKey.BtnPrev);
                    },
                    [MachineButtonKey.BtnNext] = (info) =>
                    {
                        MainModel.Instance.panel?.OnUpClickHandler(MachineButtonKey.BtnNext);
                    },
                    [MachineButtonKey.BtnBetUp] = (info) =>
                    {
                        MainModel.Instance.panel?.OnUpClickHandler(MachineButtonKey.BtnBetUp);
                    },
                    [MachineButtonKey.BtnBetDown] = (info) =>
                    {
                        MainModel.Instance.panel?.OnUpClickHandler(MachineButtonKey.BtnBetDown);
                    },
                },

                //longClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                //{
                //    [MachineButtonKey.BtnSpin] = (info) =>
                //    {
                //        DebugUtils.LogError("游戏接受到机台长按的数据：Spin");
                //        EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, true); // isLongClick
                //        OnClickSpinButton(res);
                //    }
                //}
            };


            /*
            int machineId = int.Parse(SBoxModel.Instance.MachineId);
            LoginInfo loginInfo = new LoginInfo()
            {
                gameType = (int)GameType.CoinPush,
                macId = machineId,
            };

            NetClientManager.Instance.RequestLogin(loginInfo, (res) =>
            {
                DebugUtils.LogError("【彩金后台】，登录成功");

                ReceiveBaseInfo req = new ReceiveBaseInfo()
                {
                    gameType = (int)GameType.CoinPush,
                };
                NetClientManager.Instance.RequestReadConf(req ,(res) =>
                {
                    SBoxConfData data = res as SBoxConfData;

                    DebugUtils.LogError($"获取的【彩金后台】配置，{JsonConvert.SerializeObject(data)}");

                }, (err) =>
                {
                    DebugUtils.LogError("获取的配置，失败");

                });


            }, (err) =>
            {
                DebugUtils.LogError("彩金后台，登录成功");
            });
            */
        }


        public override void OnTop()
        {
            base.OnTop();

            // 检查机台是否激活
            EventCenter.Instance.EventTrigger<EventData>(GlobalEvent.ON_DEVICE_EVENT, new EventData(GlobalEvent.CheckMachineActiveRepeat));
        }


        private GComponent gOwnerPanel;
        FreeSpinTimeController FreeSpinTimeController = new FreeSpinTimeController();
        CollectGoldCoinsController CollectGoldCoinsController = new CollectGoldCoinsController();




        TextAsset gameInfo = null;


        PayTableController payTableController = new PayTableController();
        GameObject goGameCtrl;
        GComponent gSlotCover, gPlayLines, gFrame;
        GLabel labWinTip;
        Animator animatorKingLion, animatorKing;
        GComponent anchorExpectation;

        GComponent ComQi,
            ComEFDropGoldCoin,
            ComGoldDi,
            ComReelEffect2,
            ComFsEffect01,
            ComGoldBack,
            ComNpcBs,
            ComNpcFs,
            ComBgBS,
            ComBgFs;

        List<GComponent> Bonuslist = new List<GComponent>();
        GameObject goNpc, goNpc2, goBg, goBg2, goQi, goGoldCoin, goDi, goReelEffcet, goFsEffect01, goGoldBack,goCollectGoldCoin;

        GameObject CloneNpc,
            CloneNpc2,
            CloneBg,
            CloneBg2,
            CloneQi,
            CloneGoldCoin,
            CloneDi,
            CloneReelEffcet,
            CloneFsEffect01,
            CloneGoldBack;


        GameSoundController gameSoundCtrl = new GameSoundController();
        WinTipController winTipCtrl = new WinTipController();

        MiniReelGroup uiJPGrandCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMajorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMinorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMiniCtrl = new MiniReelGroup();


        
        bool isInitPool = false;
        FguiPoolHelper fguiPoolHelper;
        FguiGObjectPoolHelper gObjectPoolHelper;

        SlotMachineController slotMachineCtrl;
        MonoHelper mono;
        bool isStoppedSlotMachine = false;
        bool isEffectSlowMotion2 = false;
        bool isEffectSlowMotion3 = false;
        bool isEffectSlowMotion4 = false;
        Coroutine coInit, coGameOnce, coGameAuto, coGameIdel, coEffectSlowMotion, coReelsTurn;

        bool tipCoinIn = false;

        string lastLanguage = "";

        List<SymbolInclude> bonusWins = new List<SymbolInclude>();
        bool isAddCreditAnim => !(slotMachineCtrl.isStopImmediately == true || SBoxModel.Instance.isCoinOutImmediately);


        long TotalBet => (long)SBoxModel.Instance.CoinInScale; // ContentModel.Instance.totalBet



        GComponent lodAnchorNPC;
        List<GComponent> lstPayTable;


        public override void InitParam()
        {
     
            if (!isInit) return;

            if (!isOpen) return;

            #region 销毁重复创建的对象

            if (lstPayTable != null)
            {
                for (int i = 0; i < lstPayTable.Count; i++)
                {
                    lstPayTable[i].Dispose();
                }
                lstPayTable = null;
            }

            #endregion


            MainModel.Instance.contentMD = ContentModel.Instance;

            // 同步玩家金额
            //MainBlackboardController.Instance.AutoSyncMyCreditToReel();

            FguiUtils.TestScreen();

            lstPayTable = new List<GComponent>();

            foreach (string url in CustomModel.Instance.payTable)
            {
                GComponent paytable = UIPackage.CreateObjectFromURL(url).asCom;
                paytable.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().InitParam(paytable);

                lstPayTable.Add(paytable);
                paytable.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount++;
            }

            ContentModel.Instance.goPayTableLst = lstPayTable.ToArray();
            payTableController.Init(lstPayTable);
            //ContentModel.Instance.payLines = new List<List<int>>();

            ParseGameInfo();

            lodAnchorNPC = this.contentPane.GetChild("anchorNPC").asCom;
            if (ComNpcBs != lodAnchorNPC)
            {
                GameCommon.FguiUtils.DeleteWrapper(ComNpcBs);
                ComNpcBs = lodAnchorNPC;
                CloneNpc = GameObject.Instantiate(goNpc);
                animatorKingLion = CloneNpc.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                GameCommon.FguiUtils.AddWrapper(ComNpcBs, CloneNpc);
            }


            GComponent lodAnchorNPCQiang = this.contentPane.GetChild("anchorNPCQiang").asCom;
            if (ComNpcFs != lodAnchorNPCQiang)
            {
                GameCommon.FguiUtils.DeleteWrapper(ComNpcFs);
                ComNpcFs = lodAnchorNPCQiang;
                CloneNpc2 = GameObject.Instantiate(goNpc2);
                animatorKing = CloneNpc2.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                GameCommon.FguiUtils.AddWrapper(ComNpcFs, CloneNpc2);
            }

            GComponent lodAnchorBGQiang = this.contentPane.GetChild("anchorBGQiang").asCom;
            if (ComBgFs != lodAnchorBGQiang)
            {
                GameCommon.FguiUtils.DeleteWrapper(ComBgFs);
                ComBgFs = lodAnchorBGQiang;
                CloneBg2 = GameObject.Instantiate(goBg2);
                GameCommon.FguiUtils.AddWrapper(ComBgFs, CloneBg2);
            }

            GComponent lodAnchorBG = this.contentPane.GetChild("anchorBG").asCom;
            if (ComBgBS != lodAnchorBG)
            {
                GameCommon.FguiUtils.DeleteWrapper(ComBgBS);
                ComBgBS = lodAnchorBG;
                CloneBg = GameObject.Instantiate(goBg);
                GameCommon.FguiUtils.AddWrapper(ComBgBS, CloneBg);
            }

            GComponent lodAnchorQi = this.contentPane.GetChild("anchorQi").asCom;
            if (ComQi != lodAnchorQi)
            {
                GameCommon.FguiUtils.DeleteWrapper(ComQi);
                ComQi = lodAnchorQi;
                CloneQi = GameObject.Instantiate(goQi);
                GameCommon.FguiUtils.AddWrapper(ComQi, CloneQi);
            }

            GComponent lodAnchorComGoldCoin = this.contentPane.GetChild("anchorEFDropGoldCoin").asCom; //anchorDropGoldCoin
            if (ComEFDropGoldCoin != lodAnchorComGoldCoin)
            {
                GameCommon.FguiUtils.DeleteWrapper(ComEFDropGoldCoin);
                ComEFDropGoldCoin = lodAnchorComGoldCoin;
                CloneGoldCoin = GameObject.Instantiate(goGoldCoin);
                GameCommon.FguiUtils.AddWrapper(ComEFDropGoldCoin, CloneGoldCoin);
            }


            GComponent lodAnchorComGoldDi = this.contentPane.GetChild("anchorGoldDi").asCom;
            if (ComGoldDi != lodAnchorComGoldDi)
            {
                GameCommon.FguiUtils.DeleteWrapper(ComGoldDi);
                ComGoldDi = lodAnchorComGoldDi;
                CloneDi = GameObject.Instantiate(goDi);
                GameCommon.FguiUtils.AddWrapper(ComGoldDi, CloneDi);
            }

            if (ComReelEffect2 != null)
            {
                ComReelEffect2.Dispose();
            }

            ComReelEffect2 = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            GameCommon.FguiUtils.DeleteWrapper(ComReelEffect2);
            GameCommon.FguiUtils.AddWrapper(ComReelEffect2, GameObject.Instantiate(goReelEffcet));
            ComReelEffect2.visible = false;

            //this.contentPane.GetController("c1").selectedPage = "FS";

            GComponent lodAnchorComFsEffect01 =
                this.contentPane.GetChild("FSFrame").asCom.GetChild("anchorFsEffect01").asCom;
            if (ComFsEffect01 != lodAnchorComFsEffect01)
            {
                GameCommon.FguiUtils.DeleteWrapper(ComFsEffect01);
                ComFsEffect01 = lodAnchorComFsEffect01;
                CloneFsEffect01 = GameObject.Instantiate(goFsEffect01);
                GameCommon.FguiUtils.AddWrapper(ComFsEffect01, CloneFsEffect01);
            }


            // MainModel.Instance.contentMD.mainPanel= this.contentPane;
            gOwnerPanel = this.contentPane.GetChild("panel").asCom;
            // gOwnerPanel = this.contentPane.AddChild(MainModel.Instance.contentMD.goAnthorPanel).asCom;
            ContentModel.Instance.goAnthorPanel = gOwnerPanel;
            MainModel.Instance.contentMD.goAnthorPanel = gOwnerPanel;
            // 事件放出
            //goGameCtrl.transform.Find("Panel").GetComponent<PanelController01>().Init();
            EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
                new EventData<GComponent>(PanelEvent.AnchorPanelChange, gOwnerPanel));


            labWinTip = this.contentPane.GetChild("winTip").asLabel;
            winTipCtrl.InitParam(labWinTip);
            GComponent collectGold = this.contentPane.GetChild("CollectGold").asCom;
            GameCommon.FguiUtils.DeleteWrapper(this.contentPane.GetChild("CollectGold").asCom.GetChild("anchorCollectGold").asCom);
            GameCommon.FguiUtils.AddWrapper(this.contentPane.GetChild("CollectGold").asCom.GetChild("anchorCollectGold").asCom, GameObject.Instantiate(goCollectGoldCoin));
            CollectGoldCoinsController.InitParam(collectGold);


            // 免费游戏
            FreeSpinTimeController.InitParam(this.contentPane.GetChild("FSFrame").asCom.GetChild("n7").asTextField,
                this.contentPane.GetChild("FSFrame").asCom.GetChild("ERCredits").asTextField);


            //游戏彩金ui
            uiJPGrandCtrl.Init("Grand", this.contentPane.GetChild("jpGrand").asCom.GetChild("reels").asList, "N0");
            uiJPMajorCtrl.Init("Major", this.contentPane.GetChild("jpMajor").asCom.GetChild("reels").asList, "N0");
            uiJPMinorCtrl.Init("Minor", this.contentPane.GetChild("jpMinor").asCom.GetChild("reels").asList, "N0");
            uiJPMiniCtrl.Init("Mini", this.contentPane.GetChild("jpMini").asCom.GetChild("reels").asList, "N0");

            uiJPGrandCtrl.SetData(3000);
            uiJPMajorCtrl.SetData(2000);
            uiJPMinorCtrl.SetData(1000);
            uiJPMiniCtrl.SetData(500);


#if false
            if (fguiPoolHelper != null && isInitPool == false)
            {
                isInitPool = true;

                fguiPoolHelper.Add(TagPoolObject.SymbolHit,
                    CustomModel.Instance.symbolHitEffect.Values.ToList(), "symbol_hit#", 5);
                fguiPoolHelper.PreLoad(TagPoolObject.SymbolHit);

                fguiPoolHelper.Add(TagPoolObject.SymbolBorder,
                    CustomModel.Instance.borderEffect, "border#", 5);
                fguiPoolHelper.PreLoad(TagPoolObject.SymbolBorder);

                fguiPoolHelper.Add(TagPoolObject.SymbolAppear,
                    CustomModel.Instance.symbolAppearEffect.Values.ToList(), "symbol_appear#", 10);
                fguiPoolHelper.PreLoad(TagPoolObject.SymbolAppear);


                //fguiPoolHelper.Init(CustomModel.Instance.symbolHitEffect,CustomModel.Instance.symbolAppearEffect, null,CustomModel.Instance.borderEffect);
            }
#endif

            // 推币机复位
            SBoxIdea.CoinPushReset(0);

            /* 主动获取玩家数据
            //SBoxModel.Instance.myCredit = 9900;
            foreach (SBoxPlayerScoreInfo item in SBoxIdea.sBoxInfo.PlayerScoreInfoList)
            {
                if (item.PlayerId == 1)
                {
                    SBoxModel.Instance.myCredit = item.Score;
                    MainBlackboardController.Instance.AutoSyncMyCreditToReel();
                }
            }*/



            ContentModel.Instance.betIndex = 0;
            ContentModel.Instance.totalBet = SBoxModel.Instance.betList[ContentModel.Instance.betIndex];


            if (PlayerPrefsUtils.isTestDeleteReels)
            {
                GObject gSlotMachine = this.contentPane.GetChild("slotMachine");
                if (gSlotMachine != null)
                {
                    gSlotMachine.Dispose();
                }
            }
            else
            {
                GComponent gSlotMachine = this.contentPane.GetChild("slotMachine").asCom;
                GComponent gReels = gSlotMachine.GetChild("reels").asCom;
                gSlotCover = gSlotMachine.asCom.GetChild("slotCover").asCom;
                gPlayLines = gSlotMachine.asCom.GetChild("playLines").asCom;
                gFrame = contentPane.GetChild("anchorFrame").asCom;
                slotMachineCtrl.Init(gSlotCover, gPlayLines, gReels, gFrame, fguiPoolHelper, gObjectPoolHelper);


            }

            // 初始化彩金值
            OnInitPool();



#if false
            GComponent gSlotMachine = contentPane.GetChild("slotMachine").asCom;
            GComponent gReels = gSlotMachine.GetChild("reels").asCom;
            gSlotCover = gSlotMachine.asCom.GetChild("slotCover").asCom;
            gPlayLines = gSlotMachine.asCom.GetChild("playLines").asCom;
            gFrame = contentPane.GetChild("anchorFrame").asCom;
            slotMachineCtrl.Init(gSlotCover, gPlayLines, gReels, gFrame, fguiPoolHelper, gObjectPoolHelper);
            /*
            ERPushMachineDataManager02.Instance.RequestSetPlayerInfo(new SBoxPlayerAccount()
            {
                PlayerId = 0,
                ScoreIn = 1000000,
            }, (obj) =>
            {
                SBoxModel.Instance.myCredit = 1000000;
                MainBlackboardController.Instance.AutoSyncMyCreditToReel();

            });*/
            ContentModel.Instance.betIndex = 0;
            ContentModel.Instance.totalBet = SBoxModel.Instance.betList[ContentModel.Instance.betIndex];

            //Dictionary<string, object> args = new Dictionary<string, object>()
            //{
            //    ["isCommon"] = false
            //};
            //PageManager.Instance.OpenPage(PageName.EmperorsReinPageFreeBonusGame1,
            //    new EventData<Dictionary<string, object>>("", args));


            SBoxIdea.CoinPushReset(0);

            //SBoxModel.Instance.myCredit = 9900;
            foreach (SBoxPlayerScoreInfo item in SBoxIdea.sBoxInfo.PlayerScoreInfoList)
            {
                if (item.PlayerId == 1)
                {
                    SBoxModel.Instance.myCredit = item.Score;

                }
            }



            //MainBlackboardController.Instance.SyncMyTempCreditToReal(true);
            //

            //## Timers.inst.Remove(GetMyCreditRepeat);
            //## Timers.inst.Add(2f, 0, GetMyCreditRepeat);


            // 日志保存
            //DebugUtils.Log("【Log】666777888");
            //DebugUtils.Save("***********************************************", LogType.Warning);
            //DebugUtils.Save("askdouhiwefihninuef");

            //DebugUtils.LogError($"666777888 - {Time.unscaledTime}");



            InitJackpotGameMajorGrand();

#endif

            preLoadedCallback?.Invoke();

        }

        void OnTestEvent(string res) {

            if (res == GlobalEvent.DestroyPanel)
            {
                GObject gPanel = this.contentPane.GetChild("panel").asCom;
                if (gPanel != null)
                {
                    gPanel.Dispose();
                }
            }
            else if (res == GlobalEvent.DestroyReels)
            {
                GObject gSlotMachine = this.contentPane.GetChild("slotMachine");
                if (gSlotMachine != null)
                {
                    gSlotMachine.Dispose();
                }
            }
        }


        void OnGetJackpotGameShow(EventData res)
        {
            if (res.name == GlobalEvent.GetJackpotGameShow)
            {
                JackpotGameShowInfoR jpGameShowR = res.value as JackpotGameShowInfoR;

                if(ContentModel.Instance.isAuto == false && ContentModel.Instance.isSpin == false)
                {
                    if(jpGameShowR.code == 0)
                    {
                        try
                        {

                            float curJackpotGrand = jpGameShowR.curJackpotOut[0] / 100;
                            float curJackpotMajor = jpGameShowR.curJackpotOut[1] / 100;
                            //ContentModel.Instance.jpGameRes.curJackpotGrand = jpGameShowR.curJackpotOut[0] / 100;
                            //ContentModel.Instance.jpGameRes.curJackpotMajor = jpGameShowR.curJackpotOut[1] / 100;
                            uiJPGrandCtrl.SetData(curJackpotGrand);
                            uiJPMajorCtrl.SetData(curJackpotMajor);
                        }
                        catch (Exception ex)
                        {
                            DebugUtils.Log($"Error : {JsonConvert.SerializeObject(jpGameShowR)}");
                        }

                    }
                    else
                    {
                        uiJPGrandCtrl.SetData(0);
                        uiJPMajorCtrl.SetData(0);
                    }
                }
            }
        }

        protected override void OnBeforetLanguageChange(I18nLang lang) {

            
#if false
            // 可以用但是不会退出空闲模式
            if (slotMachineCtrl != null)
                slotMachineCtrl.SkipWinLine(true);
#else
            OnGameReset();
#endif

            FguiSortingOrderManager.Instance.ClearAll();

            if (gObjectPoolHelper != null)
                gObjectPoolHelper.Clear();

            if (fguiPoolHelper != null)
                fguiPoolHelper.ClearAll();

        }


        void OnInitPool()
        {
            // 检查多语言
            if (lastLanguage != SBoxModel.Instance.language)
            {
                lastLanguage = SBoxModel.Instance.language;
                isInitPool = false;
            }

            /*
            // 释放对象池的引用
            if (isInitPool == false)
            {
                if (slotMachineCtrl != null)
                    slotMachineCtrl.SkipWinLine(true);
            }

            // 清空对象池
            if (isInitPool == false)
            {
                if (gObjectPoolHelper != null)
                    gObjectPoolHelper.Clear();
            }*/

            // 清空对象池
            if (isInitPool == false)
            {
                //if (fguiPoolHelper != null) fguiPoolHelper.ClearAll();
              
                fguiPoolHelper.Add(TagPoolObject.SymbolHit,
                    CustomModel.Instance.symbolHitEffect.Values.ToList(), "symbol_hit#", 5);
                fguiPoolHelper.PreLoad(TagPoolObject.SymbolHit);

                fguiPoolHelper.Add(TagPoolObject.SymbolBorder,
                    CustomModel.Instance.borderEffect, "border#", 5);
                fguiPoolHelper.PreLoad(TagPoolObject.SymbolBorder);

                fguiPoolHelper.Add(TagPoolObject.SymbolAppear,
                    CustomModel.Instance.symbolAppearEffect.Values.ToList(), "symbol_appear#", 10);
                fguiPoolHelper.PreLoad(TagPoolObject.SymbolAppear);

                //fguiPoolHelper.Init(CustomModel.Instance.symbolHitEffect,CustomModel.Instance.symbolAppearEffect, null,CustomModel.Instance.borderEffect);
            }


            isInitPool = true;
        }

        void OnJackpotOnLine(string res)
        {

            WinJackpotInfo winJPInfo = JsonConvert.DeserializeObject<WinJackpotInfo>(res);
            ContentModel.Instance.jpOnlineWin.Add(winJPInfo);

            // 投币个数入库
            // 彩金数据入库
        }


        IEnumerator GameTotalSpins(Action successCallback, Action<string> errorCallback)
        {
            bool isErr = false;
            Action<string> errFunc = (err) =>
            {
                isErr = true;
                errorCallback?.Invoke(err);
            };

            while (--ContentModel.Instance.remainPlaySpins >= 0 && !ContentModel.Instance.isRequestToStop)
            {
                yield return GameOnce(null, errFunc);

                if (isErr)
                    yield break;

                if (ContentModel.Instance.remainPlaySpins == 0)
                    break;

                yield return new WaitForSeconds(1f);
            }

            ContentModel.Instance.remainPlaySpins = ContentModel.Instance.totalPlaySpins;
            ContentModel.Instance.isRequestToStop = false;

            /*以下在successCallback中调用过
            ContentModel.Instance.isRequestToStop = false;
            ContentModel.Instance.isAuto = false;
            ContentModel.Instance.isSpin = false;
            */
            if (successCallback != null)
                successCallback.Invoke();
        }


        IEnumerator GameAuto(Action successCallback, Action<string> errorCallback)
        {
            bool isErr = false;
            Action<string> errFunc = (err) =>
            {
                isErr = true;
                errorCallback?.Invoke(err);
            };

            while (ContentModel.Instance.isAuto && !ContentModel.Instance.isRequestToStop)
            {
                yield return GameOnce(null, errFunc);

                if (isErr)
                    yield break;

                /*
                float time = Time.time;
                while (Time.time - time < 1f)
                {
                    yield return new WaitForSeconds(0.1f);
                    if (!ContentModel.Instance.isAuto)
                        break;
                }*/

                yield return new WaitForSeconds(0.1f);

                if (!ContentModel.Instance.isAuto)
                    break;
            }

            if (ContentModel.Instance.isRequestToStop)
            {
                ContentModel.Instance.isRequestToStop = false;
                ContentModel.Instance.isAuto = false;
            }

            if (successCallback != null)
                successCallback.Invoke();
        }

        IEnumerator GameFreeSpin(Action successCallback, Action<string> errorCallback)
        {

            while (ContentModel.Instance.nextReelStripsIndex == "FS")
            {

                yield return GameFreeSpinOnce(null, errorCallback);

                yield return slotMachineCtrl.SlotWaitForSeconds(1);
            }

            if (successCallback != null)
                successCallback.Invoke();
        }

        IEnumerator GameFreeSpinOnce(Action successCallback, Action<string> errorCallback)
        {

            OnGameReset();

            ContentModel.Instance.gameState = GameState.FreeSpin;


            bool isNext = false;
            bool isBreak = false;
            string errMsg = "";

            if (ApplicationSettings.Instance.isMock)
            {
                yield return RequestSlotSpinFromMock02(() =>
                {
                    isNext = true;
                }, (err) =>
                {
                    errMsg = err;
                    isNext = true;
                    isBreak = true;
                });
            }
            else
            {
                yield return RequestSlotSpinFromMachine(() =>
                {
                    isNext = true;
                }, (err) =>
                {
                    errMsg = err;
                    isNext = true;
                    isBreak = true;
                });
            }



            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            if (isBreak)
            {

                if (errorCallback != null)
                    errorCallback.Invoke(errMsg);
                yield break;
            }


            slotMachineCtrl.BeginSpin();


            if (ContentModel.Instance.isReelsSlowMotion)
            {
                if (coEffectSlowMotion != null) mono.StopCoroutine(coEffectSlowMotion);
                coEffectSlowMotion = mono.StartCoroutine(ShowEffectReelsSlowMotion());

                slotMachineCtrl.ShowSymbolAppearEffectAfterReelStop(true);
            }
            else
            {
                slotMachineCtrl.ShowSymbolAppearEffectAfterReelStop(ContentModel.Instance.winList.Count == 0);
            }

            if (slotMachineCtrl.isStopImmediately)
            {

                if (coReelsTurn != null) mono.StopCoroutine(coReelsTurn);
                coReelsTurn = mono.StartCoroutine(slotMachineCtrl.TurnReelsOnce(ContentModel.Instance.strDeckRowCol,
                    () =>
                    {
                        isNext = true;
                    }));

                yield return new WaitUntil(() => isNext == true);
                isNext = false;

            }
            else
            {

                if (coReelsTurn != null) mono.StopCoroutine(coReelsTurn);
                coReelsTurn = mono.StartCoroutine(slotMachineCtrl.TurnReelsNormal(ContentModel.Instance.strDeckRowCol,
                    () =>
                    {
                        isNext = true;
                    }));

                yield return new WaitUntil(() => isNext == true || slotMachineCtrl.isStopImmediately == true);
                isNext = false;

                // 等待移动结束
                if (slotMachineCtrl.isStopImmediately && isNext == false)
                {
                    if (coReelsTurn != null) mono.StopCoroutine(coReelsTurn);
                    coReelsTurn = mono.StartCoroutine(slotMachineCtrl.ReelsToStopOrTurnOnce(() =>
                    {
                        isNext = true;
                    }));

                    yield return new WaitUntil(() => isNext == true);
                    isNext = false;
                }
            }


            List<SymbolWin> winList = ContentModel.Instance.winList;
            long allWinCredit = 0;


            #region Win

            if (winList.Count > 0 || ContentModel.Instance.bonusResults != null)
            {

                mono.StartCoroutine(PlayKing(1f));

                long totalWinLineCredit = slotMachineCtrl.GetTotalWinCredit(winList);
                allWinCredit = totalWinLineCredit;

                // 播大奖弹窗
                WinLevelType winLevelType = GetBigWinType();

                /* 免费游戏结束时，才掉币*/
                if (winList.Count > 0 && winLevelType == WinLevelType.None)
                {
                    yield return ShowWinListOnceAtNormalSpin009(winList);
                }

                if (winLevelType != WinLevelType.None)
                {
                    slotMachineCtrl.ShowSymbolWinDeck(slotMachineCtrl.GetTotalSymbolWin(winList), true);

                    yield return new WaitForSeconds(1f);

                    // 大奖弹窗
                    yield return WinPopup(winLevelType, ContentModel.Instance.baseGameWinCoins);

                    slotMachineCtrl.CloseSlotCover();

                    slotMachineCtrl.SkipWinLine(false);
                }
                else
                {

                    // 总线赢分（同步？？）
                    bool isAddToCredit = totalWinLineCredit > TotalBet * 4;
                    slotMachineCtrl.SendPrepareTotalWinCreditEvent(totalWinLineCredit, isAddToCredit);
                }


                DebugUtils.Log($"免费游戏总赢(币)： {ContentModel.Instance.freeSpinTotalWinCoins}");
                // 总线赢分事件
                slotMachineCtrl.SendTotalWinCreditEvent(totalWinLineCredit);

                //加钱动画
                //MainBlackboardController.Instance.AddMyTempCredit(totalWinLineCredit, true, isAddCreditAnim);

                yield return PlayTotalWinEffectFS();

            }

            #endregion



            /* 先结算“免费游戏”或“小游戏”再回主游戏结算主游戏，则每局不能同步玩家真实金钱金额
            MainBlackboardController.Instance.SyncMyCreditToReal(false);*/

            /*
             if (ContentModel.Instance.isFreeSpinTrigger"))
             {
                 //增加免费游戏？？
             }*/


            #region 中游戏彩金

            bool isHitJackpot = ContentModel.Instance.jpGameWinLst.Count > 0;
            List<JackpotWinInfo> jpRes = ContentModel.Instance.jpGameWinLst;
            while (jpRes.Count > 0)
            {
                JackpotWinInfo jpWin = jpRes[0];
                jpRes.RemoveAt(0);

                Action onJPPoolSubCredit = () =>
                {
                    SetJPCurCredit(jpWin);
                };

                allWinCredit += (long)jpWin.winCredit;

                PageManager.Instance.OpenPageAsync(PageName.PusherEmperorsReinPopupJackpotGame,
                    new EventData<Dictionary<string, object>>("", new Dictionary<string, object>
                    {
                        ["jackpotType"] = jpWin.name,
                        ["totalEarnCredit"] = jpWin.winCredit,
                        ["onJPPoolSubCredit"] = onJPPoolSubCredit,
                        ["jpCredit"] = ContentModel.Instance.jpGameWhenCreditLst,
                    }),
                    (res) =>
                    {
                        isNext = true;
                    });

                yield return new WaitUntil(() => isNext == true);
                isNext = false;

                // 总线赢分事件
                slotMachineCtrl.SendTotalWinCreditEvent(allWinCredit);

                //MainBlackboardController.Instance.AddMyTempCredit((long)jpWin.winCredit, true, isAddCreditAnim);
            }

            #endregion




            // 【取消】大厅彩金

            // 小游戏


            // 本剧同步玩家金钱
            //MainBlackboardController.Instance.SyncMyTempCreditToReal(false);



            /* 免费游戏最后一句一起掉币

            // 本局掉币
            ERPushMachineDataManager02.Instance.RequestCoinPushSpinEnd(res1 =>
            {
                isNext = true;
            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            if (winList.Count > 0 || isHitJackpot)
            {
                yield return ShowWinListCoinCountDown(winList, allWinCredit, isHitJackpot);
            }
            */


#if false
            // 即中即退
            yield return CoinOutImmediately(allWinCredit);


            //免费游戏中，添加额外免费游戏
            if (ContentModel.Instance.isFreeSpinAdd)
            {
                slotMachineCtrl.BeginBonusFreeSpinAdd();
                PageManager.Instance.OpenPageAsync(PageName.PusherEmperorsReinPopupFreeSpinTrigger,
                    new EventData<Dictionary<string, object>>("",
                        new Dictionary<string, object>()
                        {
                            ["freeSpinCount"] = ContentModel.Instance.freeSpinAddNum, ["isAddFreeGame"] = true,
                        }),
                    (ed) =>
                    {
                        isNext = true;
                    });


                yield return new WaitUntil(() => isNext == true);
                isNext = false;

                // 【待修改】重置剩余的局数 
                ContentModel.Instance.showFreeSpinRemainTime =
                    ContentModel.Instance.freeSpinTotalTimes - ContentModel.Instance.freeSpinPlayTimes;

                yield return slotMachineCtrl.SlotWaitForSeconds(1.5f);
                slotMachineCtrl.EndBonusFreeSpinAdd();
            }
#endif


            ContentModel.Instance.gameState = GameState.Idle;
            // 先结算主游戏，再进入“免费游戏”或“小游戏”，则每局都可以同步玩家真实金钱金额

            if (successCallback != null)
                successCallback.Invoke();
        }

        IEnumerator GameOnce(Action successCallback, Action<string> errorCallback)
        {
            bool isNext = false;
            bool isBreak = false;
            string errMsg = "";

            // 普通游戏要调用，免费游戏不用，传入固定值：1,3,5
            //SBoxIdea.CoinPushSpin(0, 1);  // 1 3 5
            GameSoundHelper.Instance.StopSound(SoundKey.SpinBGIdle);
            ERPushMachineDataManager02.Instance.RequesBeginTurn((res) =>
            {
                int data = (int)res;

                if (data != 0)
                {
                    DebugUtils.LogError($"Begin Turn Error(20100)   code: {data}");
                    errMsg = "";

                    switch (data)
                    {
                        case -2:
                            {
                                errMsg = I18nMgr.language == I18nLang.cn ? "积分不足，请先充值"
                   : "<size=15>Balance is insufficient, please recharge first</size>";
                            }
                            break;

                        default:
                            {
                                //errMsg = ""; //不显示该错误
                                errMsg = string.Format(I18nMgr.T("Request failed: [{0}]"), "Begin Turn "+ data);
                            }
                            break;
                    }
                    isBreak = true;

                    TestManager.Instance.ShowTip($"Begin Turn Error(20100)   code: {data}");
                }

                // -1 参数格式错误
                // -2 钱不够
                // -3 算法卡状态不对

                isNext = true;
            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            if (isBreak)
            {
                errorCallback?.Invoke(errMsg);
                yield break;
            }


            if (!SBoxModel.Instance.isMachineActive)
            {
               
                errorCallback?.Invoke(I18nMgr.language == I18nLang.cn ? "请激活机台"
                    :"<size=24>Machine not activated!</size>");
                /* */
                /*errorCallback?.Invoke( "<size=24>Machine not activated!</size>");*/
                yield break;
            }


            if (SBoxModel.Instance.myCredit < TotalBet)
            {
                tipCoinIn = true;
                errorCallback?.Invoke(
                    I18nMgr.language == I18nLang.cn ?  "积分不足，请先充值"
                    :"<size=15>Balance is insufficient, please recharge first</size>");/**/
                /*errorCallback?.Invoke("<size=15>Balance is insufficient, please recharge first</size>");*/
                yield break;
            }


            OnGameReset();

            ContentModel.Instance.gameState = GameState.Spin;

            slotMachineCtrl.BeginTurn();



            TestManager.Instance.ShowTip("请求游戏数据");

            if (ApplicationSettings.Instance.isMock)
            {

                yield return RequestSlotSpinFromMock02(() =>
                {
                    isNext = true;
                }, (err) =>
                {
                    errMsg = err;
                    isNext = true;
                    isBreak = true;
                });

            }
            else
            {
                yield return RequestSlotSpinFromMachine(() =>
                {
                    isNext = true;
                }, (err) =>
                {
                    errMsg = err;
                    isNext = true;
                    isBreak = true;
                });
            }

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            if (isBreak)
            {
                if (errorCallback != null)
                    errorCallback.Invoke(errMsg);
                yield break;
            }

            if (SBoxModel.Instance.isJackpotOnLine)
            {
                if (ApplicationSettings.Instance.isMock)
                {
                    // 模拟在线彩金中奖数据
                    MachineDataManager02.Instance.RequestJackpotOnLine();
                }
                else
                {
                    /*
                    JackpotOnLineManager.Instance.RequestsJackpotOnLineData(
                        new JackBetInfo
                        {
                            seat = 1,  // 固定死
                            bet = (int)ContentModel.Instance.totalBet,  // 总压注
                            betPercent = 100, // 固定死
                            scoreRate =  _consoleBB.Instance.jackpotScoreRate,      //10000,  // 1 除以 币值 乘以 1000 整形   （联网彩金分值比 ：只能该币值）
                            JPPercent =  _consoleBB.Instance.jackpotPercent,    //5  // 千分之几（1 - 100 可调 ；名称： 联网彩金比（千分）  ）
                        },
                        null, null
                    );
                    */
                }
            }


            TestManager.Instance.ShowTip("滚轮开始滚动");

            slotMachineCtrl.BeginSpin();


            if (ContentModel.Instance.isReelsSlowMotion)
            {
                if (coEffectSlowMotion != null) mono.StopCoroutine(coEffectSlowMotion);
                coEffectSlowMotion = mono.StartCoroutine(ShowEffectReelsSlowMotion());
                slotMachineCtrl.ShowSymbolAppearEffectAfterReelStop(true);
            }
            else
            {
                slotMachineCtrl.ShowSymbolAppearEffectAfterReelStop(ContentModel.Instance.winList.Count == 0);
            }


            if (slotMachineCtrl.isStopImmediately)
            {
                //reelsTurnType = ReelsTurnType.Once;

                if (coReelsTurn != null) mono.StopCoroutine(coReelsTurn);
                coReelsTurn = mono.StartCoroutine(slotMachineCtrl.TurnReelsOnce(ContentModel.Instance.strDeckRowCol,
                    () =>
                    {
                        isNext = true;
                    }));


                yield return new WaitUntil(() => isNext == true);
                isNext = false;
            }
            else
            {

                if (coReelsTurn != null) mono.StopCoroutine(coReelsTurn);
                coReelsTurn = mono.StartCoroutine(slotMachineCtrl.TurnReelsNormal(ContentModel.Instance.strDeckRowCol,
                    () =>
                    {
                        isNext = true;
                    }));


                yield return new WaitUntil(() => isNext == true || slotMachineCtrl.isStopImmediately == true);
                isNext = false;

                // 等待移动结束
                if (slotMachineCtrl.isStopImmediately && isNext == false)
                {
                    if (coReelsTurn != null) mono.StopCoroutine(coReelsTurn);
                    coReelsTurn = mono.StartCoroutine(slotMachineCtrl.ReelsToStopOrTurnOnce(() =>
                    {
                        isNext = true;
                    }));


                    yield return new WaitUntil(() => isNext == true);
                    isNext = false;
                }
            }






            List<SymbolWin> winList = ContentModel.Instance.winList;
            long allWinCredit = 0;
            if (winList.Count > 0) //if (winList.Count > 0 || ContentModel.Instance.bonusResult.Count > 0)
            {

                if (_spinWEMD.Instance.isSingleWin)
                {
                    mono.StartCoroutine(PlayKingLion(1f));
                }
                else
                {
                    mono.StartCoroutine(PlayKingLion(2f));
                }

                long totalWinLineCredit = 0;
                totalWinLineCredit = slotMachineCtrl.GetTotalWinCredit(winList);
                allWinCredit = totalWinLineCredit;



                WinLevelType winLevelType = GetBigWinType();
                
                if (winList.Count > 0 && winLevelType == WinLevelType.None)
                {
                    yield return ShowWinListOnceAtNormalSpin009(winList);
                }


                if (winLevelType != WinLevelType.None)
                {
                    slotMachineCtrl.ShowSymbolWinDeck(slotMachineCtrl.GetTotalSymbolWin(winList), true);
                    // 大奖弹窗
                    yield return WinPopup(winLevelType, ContentModel.Instance.baseGameWinCoins);

                    slotMachineCtrl.CloseSlotCover();

                    slotMachineCtrl.SkipWinLine(false);

                }
                else
                {
                    bool isAddToCredit = totalWinLineCredit > TotalBet * 4;
                    slotMachineCtrl.SendPrepareTotalWinCreditEvent(totalWinLineCredit, isAddToCredit);
                }



                slotMachineCtrl.SendTotalWinCreditEvent(allWinCredit);

                //加钱动画
                // MainBlackboardController.Instance.AddMyTempCredit(allWinCredit, true, isAddCreditAnim);

                if (_spinWEMD.Instance.isSingleWin)
                {
                    yield return PlayTotalWinEffectBS();

                    DebugUtils.Log("庆祝动画结束");
                }

            }



            #region 中游戏彩金

            bool isHitJackpot = ContentModel.Instance.jpGameWinLst.Count > 0;
            List<JackpotWinInfo> jpRes = ContentModel.Instance.jpGameWinLst;
            List<float> jpCredit = ContentModel.Instance.jpGameWhenCreditLst;
            if (jpRes.Count > 0)
            {
                slotMachineCtrl.SkipWinLine(true);
                slotMachineCtrl.ShowSymbolEffect(TagPoolObject.SymbolHit,new List<int>() {12},true,12,true);
                yield return slotMachineCtrl.SlotWaitForSeconds(1.5f);
                slotMachineCtrl.SkipWinLine(true);
                slotMachineCtrl.ShowSymbolEffect(TagPoolObject.SymbolAppear, new List<int>() { 12 }, true, 12, true);
            }
            while (jpRes.Count > 0)
            {
                JackpotWinInfo jpWin = jpRes[0];
                jpRes.RemoveAt(0);

                Action onJPPoolSubCredit = () =>
                {
                    SetJPCurCredit(jpWin);
                };

                allWinCredit += (long)jpWin.winCredit;

                DebugUtils.LogError($"中游戏彩金数据： JackpotWinInfo = {JsonConvert.SerializeObject(jpWin)} ");
                PageManager.Instance.OpenPageAsync(PageName.PusherEmperorsReinPopupJackpotGame,
                    new EventData<Dictionary<string, object>>("", new Dictionary<string, object>
                    {
                        ["jackpotType"] = jpWin.name,
                        ["totalEarnCredit"] = jpWin.winCredit,
                        ["onJPPoolSubCredit"] = onJPPoolSubCredit,
                        ["jpCredit"] = jpCredit,
                    }),
                    (res) =>
                    {
                        isNext = true;
                    });


                yield return new WaitUntil(() => isNext == true);
                isNext = false;

                // 总线赢分（同步？？）
                slotMachineCtrl.SendTotalWinCreditEvent(allWinCredit);

                //MainBlackboardController.Instance.AddMyTempCredit((long)jpWin.winCredit, true, isAddCreditAnim);
            }

            #endregion



            // 中大厅彩金

            #region 中大厅彩金


            while (ContentModel.Instance.jpOnlineWin.Count > 0)
            {
                WinJackpotInfo data = ContentModel.Instance.jpOnlineWin[0];
                ContentModel.Instance.jpOnlineWin.RemoveAt(0);

                //long fromCredit = data.win < 1000 ? data.win : data.win - 1000;

                long winCredit = SBoxModel.Instance.CoinInScale * data.win;
                allWinCredit += winCredit;

                PageManager.Instance.OpenPageAsync(PageName.PusherEmperorsReinPopupJackpotOnline,
                    new EventData<Dictionary<string, object>>("", new Dictionary<string, object>
                    {
                        ["toCredit"] = winCredit, ["jackpotType"] = data.jackpotId,
                        //["fromCredit"] = (long)fromCredit
                    }),
                    (res) =>
                    {
                        isNext = true;
                    });

                yield return new WaitUntil(() => isNext == true);
                isNext = false;

                // 总线赢分（同步？？）
                slotMachineCtrl.SendTotalWinCreditEvent(allWinCredit);

                //MainBlackboardController.Instance.AddMyTempCredit(winCredit, true, isAddCreditAnim);
            }

            #endregion






            // 小游戏Bonus

            // 本剧同步玩家金钱
            //MainBlackboardController.Instance.SyncMyTempCreditToReal(false);
            //MainBlackboardController.Instance.SyncMyTempCreditToReal(true);

            //DebugUtils.LogWarning($"结束分数： SBoxModel Credit = {SBoxModel.Instance.myCredit}   uiCredit={MainModel.Instance.myCredit}");

            // 本局掉币



            ERPushMachineDataManager02.Instance.RequestCoinPushSpinEnd(res1 =>
            {

                JSONNode data = JSONObject.Parse((string)res1);

                int code = (int)data["code"];

                if(code != 0)
                {
                    DebugUtils.LogError($" CoinPushSpinEnd(20102) : [0]= {code}");
                }


                isNext = true;
            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            if (winList.Count > 0 || isHitJackpot)
            {
                yield return ShowWinListCoinCountDown(winList, allWinCredit, isHitJackpot);
            }


            // 即中即退
            yield return CoinOutImmediately(allWinCredit);

            // Free Spin
            if (ContentModel.Instance.isFreeSpinTrigger)
            {

                GameSoundHelper.Instance.PlaySoundEff(SoundKey.FreeSpinTriggerBG);
                yield return new WaitForSeconds(2.6f);
                yield return FreeSpinTrigger(null, errorCallback);
            }

            DebugUtils.Log("进入空闲模式！！！");
            // 进入空闲模式
            ContentModel.Instance.gameState = GameState.Idle;
            animatorKingLion.speed = 1f;
            animatorKing.speed = 1f;
            GameSoundHelper.Instance.PlaySoundEff(SoundKey.SpinBGIdle);
            if (winList.Count > 0 && !ContentModel.Instance.isAuto && !ContentModel.Instance.isFreeSpinTrigger)
            {
                if (coGameIdel != null) mono.StopCoroutine(coGameIdel);
                coGameIdel = mono.StartCoroutine(GameIdle(winList));
            }

            if (successCallback != null)
                successCallback.Invoke();
        }


        IEnumerator FreeSpinTrigger(Action successCallback, Action<string> errorCallback)
        {
            //FreeSpinTimeController.Score = 0;

            bool isNext = false;


            PageManager.Instance.OpenPageAsync(PageName.PusherEmperorsReinPopupFreeSpinTrigger,
            new EventData<Dictionary<string, object>>("",
                new Dictionary<string, object>()
                {
                    //["autoCloseTimeS"] = 3f,
                    ["freeSpinCount"] = ContentModel.Instance.freeSpinTotalTimes,
                }),
            (ed) =>
            {
                DebugUtils.Log("回调执行！isNext = true"); // 加日志
                isNext = true;
            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            //  yield return new WaitForSeconds(3f);

            InputStackContextFreeSpin((context) =>
            {
                //goBgRegular.SetActive(false);
                //goBgFreeSpin.SetActive(true);
                //if (goPanelFreeSpin != null) goPanelFreeSpin.SetActive(true);
                // Transform tfm = goAnchorMain.GetComponent<Transform>();
                // tfm.localPosition = new Vector3(0, 75, 0);

                this.contentPane.GetController("c1").selectedPage = "FS";
                //this.contentPane.GetChild("FSFrame").asCom.GetChild("n7").asTextField.text = ContentModel.Instance.freeSpinTotalTimes.ToString();
            });


            slotMachineCtrl.BeginBonusFreeSpin();


            yield return GameFreeSpin(null, errorCallback);


            OutputStackContextFreeSpin(
                (context) =>
                {
                    SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.Default);

                    slotMachineCtrl.SetReelsDeck((string)context["./strDeckRowCol"]);

                    _spinWEMD.Instance.SelectData(_spinWEMD.SPIN_WIN_EFFECT_FREE_SPIN_TRIGGER);


                    SymbolWin sw = (SymbolWin)context["./winFreeSpinTriggerOrAddCopy"];
                    if (sw != null && sw.cells.Count > 0)
                    {
                        slotMachineCtrl.ShowSymbolWinDeck(sw, true);
                    }

                    //goSlotCover.SetActive(true);
                    //goBgRegular.SetActive(true);
                    //goBgFreeSpin.SetActive(false);
                    //if (goPanelFreeSpin != null) goPanelFreeSpin.SetActive(false);
                    // Transform tfm = goAnchorMain.GetComponent<Transform>();
                    // tfm.localPosition = new Vector3(0, 0, 0);

                    this.contentPane.GetController("c1").selectedPage = "BS";
                });

            slotMachineCtrl.EndBonusFreeSpin();

            DebugUtils.Log($"@@ 免费游戏总赢(币)： {ContentModel.Instance.freeSpinTotalWinCoins}");
            PageManager.Instance.OpenPageAsync(PageName.PusherEmperorsReinPopupFreeSpinResult,
                new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object>()
                    {
                        ["freeSpinTotalWin"] = ContentModel.Instance.freeSpinTotalWinCoins,
                    }),
                (ed) =>
                {
                    DebugUtils.Log("回调执行！isNext = true"); // 加日志
                    isNext = true;
                });


            yield return new WaitUntil(() => isNext == true);
            isNext = false;
            // MainBlackboardController.Instance.AddMyTempCredit((long)ContentModel.Instance.freeSpinTotalWinCredit, true, isAddCreditAnim);
            // MainBlackboardController.Instance.SyncMyTempCreditToReal(false);



            // 本局掉币
            ERPushMachineDataManager02.Instance.RequestCoinPushSpinEnd(res1 =>
            {
                isNext = true;
            });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            if (ContentModel.Instance.freeSpinTotalWinCoins > 0)
            {
                yield return ShowCoinCountDown(ContentModel.Instance.freeSpinTotalWinCoins);
            }

            

            yield return slotMachineCtrl.SlotWaitForSeconds(1.5f);
        }

        public IEnumerator ShowSymbolBonusEffect(List<SymbolInclude> bonusWins)
        {
            yield return new WaitForSeconds(1.5f);
            //for (int i = 0; i < Bonuslist.Count; i++)
            //{
            //    Bonuslist[i].TweenMove(slotMachineCtrl.SymbolCenterToNodeLocalPos(2, 2, anchorExpectation) + new Vector2(0, 212), 0.6f);
            //    yield return new WaitForSeconds(1f);
            //    Bonuslist[i].visible = false;
            //    // MainBlackboardController.Instance.AddMyTempCredit(bonusWins[i].earnCredit, true, isAddCreditAnim);
            //    slotMachineCtrl.SendTotalBonusCreditEvent(bonusWins[i].earnCredit);

            //}
        }


        List<Dictionary<string, object>> stackContext = new List<Dictionary<string, object>>();

        void InputStackContextFreeSpin(Action<Dictionary<string, object>> inputStackCallBack)
        {
            Dictionary<string, object> context = new Dictionary<string, object>()
            {
                ["name"] = "FreeSpinTrigger",
                ["modifyTime"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                ["./gameState"] = ContentModel.Instance.gameState,
                ["./winList"] = ContentModel.Instance.winList,
                ["./response"] = ContentModel.Instance.response,
                ["./winFreeSpinTriggerOrAddCopy"] = ContentModel.Instance.winFreeSpinTriggerOrAddCopy,
                //["./win5Kind"] = ContentModel.Instance.win5Kind,
                //["./isWin5Kind"] = ContentModel.Instance.isWin5Kind,
                ["./strDeckRowCol"] = ContentModel.Instance.strDeckRowCol,
                //["./middleIndexList"] = ContentModel.Instance.middleIndexList,
                ["./curReelStripsIndex"] = ContentModel.Instance.curReelStripsIndex,
                ["./nextReelStripsIndex"] = ContentModel.Instance.nextReelStripsIndex,
                ["./totalEarnCredit"] = ContentModel.Instance.totalEarnCoins,
                ["./isReelsSlowMotion"] = ContentModel.Instance.isReelsSlowMotion,
                ["./isFreeSpinTrigger"] = ContentModel.Instance.isFreeSpinTrigger,
                //["./customDataName"] = ContentModel.Instance.customDataName,
                //["./shufflingList"] = ContentModel.Instance.shufflingList,

                ["./curGameNumber"] = ContentModel.Instance.curGameNumber,
                ["./curGameCreatTimeMS"] = ContentModel.Instance.curGameCreatTimeMS,
                ["./curGameGuid"] = ContentModel.Instance.curGameGuid,
            };
            stackContext.Insert(0, context);

            //=====================
            inputStackCallBack?.Invoke(context);
        }

        void OutputStackContextFreeSpin(Action<Dictionary<string, object>> outputStackCallBack)
        {
            Dictionary<string, object> context = stackContext[0];
            stackContext.RemoveAt(0);

            ContentModel.Instance.gameState = (string)context["./gameState"];


            ContentModel.Instance.winList = (List<SymbolWin>)context["./winList"];
            ContentModel.Instance.response = (string)context["./response"];
            ContentModel.Instance.winFreeSpinTriggerOrAddCopy = (SymbolWin)context["./winFreeSpinTriggerOrAddCopy"];
            // ContentModel.Instance.win5Kind = (SymbolWin)context["./win5Kind"];
            ContentModel.Instance.strDeckRowCol = (string)context["./strDeckRowCol"];
            //ContentModel.Instance.middleIndexList = (List<int>)context["./middleIndexList"];
            ContentModel.Instance.curReelStripsIndex = (string)context["./curReelStripsIndex"];
            ContentModel.Instance.nextReelStripsIndex = (string)context["./nextReelStripsIndex"];
            ContentModel.Instance.totalEarnCoins = (long)context["./totalEarnCredit"];
            //ContentModel.Instance.isWin5Kind = (bool)context["./isWin5Kind"];
            ContentModel.Instance.isReelsSlowMotion = (bool)context["./isReelsSlowMotion"];
            ContentModel.Instance.isFreeSpinTrigger = (bool)context["./isFreeSpinTrigger"];
            //ContentModel.Instance.customDataName = (string)context["./customDataName"];
            //ContentModel.Instance.shufflingList = (List<List<int>>)context["./shufflingList"];


            ContentModel.Instance.curGameNumber = (long)context["./curGameNumber"];
            ContentModel.Instance.curGameCreatTimeMS = (long)context["./curGameCreatTimeMS"];
            ContentModel.Instance.curGameGuid = (string)context["./curGameGuid"];


            //=====================
            outputStackCallBack?.Invoke(context);
        }


        IEnumerator PlayTotalWinEffectBS()
        {
            ComQi.visible = true;
            ComEFDropGoldCoin.visible = true;
            ComGoldDi.visible = true;
            GameSoundHelper.Instance.PlaySoundEff(SoundKey.MainWinEffect);
            yield return new WaitForSeconds(3f);

            ComQi.visible = false;
            ComEFDropGoldCoin.visible = false;
            ComGoldDi.visible = false;
        }

        IEnumerator PlayKingLion(float Speed)
        {
            GameSoundHelper.Instance.PlaySoundEff(SoundKey.MainWinAnim);
            animatorKingLion.speed = Speed;
            animatorKingLion.Rebind(); // 重置状态机和参数
            animatorKingLion.Play("win", -1, 0f); // 从第0帧开始播放
            animatorKingLion.Update(0f); // 立即刷新状态
            yield return new WaitForSeconds(2 / Speed);
            animatorKingLion.Rebind(); // 重置状态机和参数
            animatorKingLion.Play("idle", -1, 0f); // 从第0帧开始播放
            animatorKingLion.Update(0f); // 立即刷新状态
        }


        IEnumerator PlayTotalWinEffectFS()
        {
            ComGoldDi.visible = true;
            GameSoundHelper.Instance.PlaySoundEff(SoundKey.FreeGameEffect);
            yield return new WaitForSeconds(3f);
            ComGoldDi.visible = false;
        }

        IEnumerator PlayKing(float Speed)
        {
            mono.StartCoroutine(PlayMusic());
            animatorKing.speed = Speed;
            animatorKing.Rebind(); // 重置状态机和参数
            animatorKing.Play("win", -1, 0f); // 从第0帧开始播放
            animatorKing.Update(0f); // 立即刷新状态
            yield return new WaitForSeconds(2 / Speed);
            animatorKing.Rebind(); // 重置状态机和参数
            animatorKing.Play("idle", -1, 0f); // 从第0帧开始播放
            animatorKing.Update(0f); // 立即刷新状态
        }



        IEnumerator PlayMusic()
        {
            yield return new WaitForSeconds(0.75f);
            GameSoundHelper.Instance.PlaySoundEff(SoundKey.FreeGameNpc);
        }






        void OnGameReset()
        {
            if (coGameIdel != null) mono.StopCoroutine(coGameIdel);
            //mono.StopCoroutine(corEffectSlowMotion);

            slotMachineCtrl.isStopImmediately = false;
            slotMachineCtrl.CloseSlotCover();
            isEffectSlowMotion2 = false;
            isStoppedSlotMachine = false;
            //goExpectation.SetActive(false);
            ComReelEffect2.visible = false;
            slotMachineCtrl.SkipWinLine(true);
        }



        private IEnumerator GameIdle(List<SymbolWin> winList)
        {
            if (winList.Count == 0)
            {
                yield break;
            }

            SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.GameIdle);

            yield return new WaitForSeconds(3f);

            yield return slotMachineCtrl.ShowWinListAwayDuringIdle(winList);
        }




        IEnumerator CoinOutImmediately(long totalWinCredit)
        {
            if (SBoxModel.Instance.isCoinOutImmediately && totalWinCredit > 0)
            {
                /*##int coinOutCount = DeviceUtils.GetCoinOutNum((int)totalWinCredit);
                if(coinOutCount > 0) //退票个数大于0
                {
                    isCoinOutImmediatelyFinish = false;
                    MachineDeviceCommonBiz.Instance.DoCoinOutImmediately((int)totalWinCredit);
                    yield return new WaitUntil(() => isCoinOutImmediatelyFinish == true);
                }*/
                yield return null;
            }
        }

        IEnumerator WinPopup(WinLevelType winLevelType, long winCredit)
        {
            bool isNext = false;
            PageManager.Instance.OpenPageAsync(PageName.PusherEmperorsReinPopupBigWin,
                new EventData<Dictionary<string, object>>("", new Dictionary<string, object>
                {
                    ["baseGameWinCredit"] = winCredit, //ContentModel.Instance.baseGameWinCredit,
                    ["WinType"] = winLevelType,
                }),
                (res) =>
                {
                    isNext = true;
                   // (PageManager.Instance.pageCacheDict[PageName.PushEmperorsReinPopupBigWin] as PopupBigWin).DongHuang();
                });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;

        }


        public IEnumerator ShowEffectReelsSlowMotion()
        {
            yield return new WaitUntil(() => isEffectSlowMotion2 == true);
            // 打开Expectation

            ComReelEffect2.xy = slotMachineCtrl.SymbolCenterToNodeLocalPos(2, 1, anchorExpectation);
            ComReelEffect2.visible = true;
            GameSoundHelper.Instance.PlaySoundEff(SoundKey.SlowMotionEffect);
            yield return new WaitUntil(() => isEffectSlowMotion3 == true);

            GameSoundHelper.Instance.PlaySoundEff(SoundKey.SlowMotionEffect);
            ComReelEffect2.TweenMove(slotMachineCtrl.SymbolCenterToNodeLocalPos(3, 1, anchorExpectation), 1);

            yield return new WaitUntil(() => isEffectSlowMotion4 == true);    
            
            ComReelEffect2.TweenMove(slotMachineCtrl.SymbolCenterToNodeLocalPos(4, 1, anchorExpectation), 1);
            GameSoundHelper.Instance.PlaySoundEff(SoundKey.SlowMotionEffect);

            //ComReelEffect2.TweenMove(slotMachineCtrl.SymbolCenterToNodeLocalPos(4, 1, anchorExpectation), 1);
            yield return new WaitUntil(() => isStoppedSlotMachine == true);
            // 关闭Expectation
            ComReelEffect2.visible = false;

            if (ContentModel.Instance.isFreeSpinTrigger)
            {
            }
        }


        IEnumerator ShowWinListCoinCountDown(List<SymbolWin> winList, long totalWinLineCredit, bool isHitJackpot)
        {
            bool isNext = false;

            if (!isHitJackpot)
                slotMachineCtrl.ShowSymbolWinDeck(slotMachineCtrl.GetTotalSymbolWin(winList), true);

            yield return ShowCoinCountDown(totalWinLineCredit);

            //停止特效显示
            slotMachineCtrl.SkipWinLine(false);
            //显示遮罩
            slotMachineCtrl.CloseSlotCover();

        }

        IEnumerator ShowCoinCountDown(long totalWinLineCredit)
        {
            bool isNext = false;
            int curCoinCountDown = (int)totalWinLineCredit;
            int lastCoinCountDown = curCoinCountDown;
            float lastRunTimeS = Time.unscaledTime;


            bool isStart = true;
            int tryGetStartCount = 50;

            // 防止死循环
            //yield return new WaitForSecondsRealtime(0.3f);  // 延时读，避免读到旧数据

            while (Time.unscaledTime - lastRunTimeS  < 10) // 10秒     //1800 = 60 * 30 = 30分钟
            {

                // 防止第一次读到 0 立马退出。（算法卡掉币赋值有延时）
                while (isStart && --tryGetStartCount>0)
                {
                    ERPushMachineDataManager02.Instance.RequestCoinCountDown((int)totalWinLineCredit, (result) =>
                    {
                        curCoinCountDown = (int)result;

                        if (curCoinCountDown != 0)  // 首次读到币
                        {
                            isStart = false;
                            //DebugUtils.Save($" CoinCountDown start cout : {curCoinCountDown}");
                        }

                        // 金币不发生变化，延时10秒退出循环
                        if (lastCoinCountDown != curCoinCountDown)
                        {
                            lastCoinCountDown = curCoinCountDown;
                            lastRunTimeS = Time.unscaledTime;
                        }

                        CollectGoldCoinsController.DiaoJinBin(curCoinCountDown);
                        isNext = true;
                    });

                    yield return new WaitUntil(() => isNext == true);
                    isNext = false;

                    //yield return new WaitForSecondsRealtime(0.01f);
                }



                ERPushMachineDataManager02.Instance.RequestCoinCountDown((int)totalWinLineCredit, (result) =>
                {
                    curCoinCountDown = (int)result;

                    // 金币不发生变化，延时10秒退出循环
                    if (lastCoinCountDown != curCoinCountDown)
                    {
                        lastCoinCountDown = curCoinCountDown;
                        lastRunTimeS = Time.unscaledTime;
                    }

                    CollectGoldCoinsController.DiaoJinBin(curCoinCountDown);
                    isNext = true;
                });

                yield return new WaitUntil(() => isNext == true);
                isNext = false;

                if (curCoinCountDown == 0)
                    break;
            }

            //DebugUtils.Save($" CoinCountDown end cout : {curCoinCountDown}");  // 记录结束币数

            if (curCoinCountDown > 0) 
            {
                DebugUtils.LogError($"掉币数量没刷新，数值停留在： {curCoinCountDown}");
            }


            CollectGoldCoinsController.Close();
        }


        IEnumerator ShowWinListOnceAtNormalSpin009(List<SymbolWin> winList)
        {
            if (_spinWEMD.Instance.isTotalWin)
            {
                yield return slotMachineCtrl.ShowSymbolWinBySetting(
                    slotMachineCtrl.GetTotalSymbolWin(winList), true, SpinWinEvent.TotalWinLine);
            }
            else
            {


                //停止特效显示
                slotMachineCtrl.SkipWinLine(false);

                int idx = 0;
                while (idx < winList.Count)
                {
                    SymbolWin curSymbolWin = winList[idx];
                    /*## 是否是五连线
                    if (slotMachineCtrl.Check5kind(curSymbolWin) && !slotMachineCtrl.isStopImmediately)
                    {
                        yield return Show5KindPoup(curSymbolWin);
                    }*/
                    yield return slotMachineCtrl.ShowSymbolWinBySetting(curSymbolWin, true, SpinWinEvent.SingleWinLine);
                    ++idx;
                }

            }

            //停止特效显示
            slotMachineCtrl.SkipWinLine(false);

            slotMachineCtrl.CloseSlotCover();

        }



        public IEnumerator Show5KindPoup(SymbolWin win5Kind)
        {

            slotMachineCtrl.SkipWinLine(false);
            slotMachineCtrl.CloseSlotCover();

            //GameSoundHelper.Instance.PlaySound(SoundKey.FiveLine);

            //goEffect5Kind.SetActive(true);
            yield return new WaitForSeconds(1f);
            //goEffect5Kind.SetActive(false);

            yield return slotMachineCtrl.SlotWaitForSeconds(0.3f);

        }


        IEnumerator BonusBegin()
        {
            Bonuslist.Clear();
            bonusWins = MachineDataG200Controller.Instance.symbolInclude;

            foreach (SymbolInclude item in bonusWins)
            {

                Bonuslist.Add(UIPackage.CreateObject("Common", "AnchorRootDefault").asCom);
                anchorExpectation.AddChild(Bonuslist[Bonuslist.Count - 1]);
                Bonuslist[Bonuslist.Count - 1].xy =
                    slotMachineCtrl.SymbolCenterToNodeLocalPos(item.colIdx, item.rowIdx, anchorExpectation);
                Bonuslist[Bonuslist.Count - 1].visible = true;
                GameCommon.FguiUtils.AddWrapper(Bonuslist[Bonuslist.Count - 1], GameObject.Instantiate(goGoldBack));
            }

            yield return slotMachineCtrl.ShowSymbolBonusBySetting(bonusWins, true);
            //return bounsSroce;
        }





        WinLevelType GetBigWinType(long? winCredit = null)
        {
            long baseGameWinCredit = winCredit != null ? (long)winCredit : ContentModel.Instance.baseGameWinCoins;
            List<WinMultiple> winMultipleList = ContentModel.Instance.winLevelMultiple;

            WinLevelType winLevelType = WinLevelType.None;
            for (int i = 0; i < winMultipleList.Count; i++)
            {
                if (baseGameWinCredit > winMultipleList[i].multiple)
                {
                    winLevelType = winMultipleList[i].winLevelType;
                }
            }

            return winLevelType;
        }

        void SetJPCurCredit(JackpotWinInfo jpWin)
        {
            switch (jpWin.id)
            {
               /* case 0:
                    uiJPGrangCtrl.SetData(jpWin.curCredit);
                    break;*/
                case 0:
                    uiJPGrandCtrl.SetData(jpWin.curCredit);
                    break;
                case 1:
                    uiJPMajorCtrl.SetData(jpWin.curCredit);
                    break;
                case 2:
                    uiJPMinorCtrl.SetData(jpWin.curCredit, null, false);
                    break;
                case 3:
                    uiJPMiniCtrl.SetData(jpWin.curCredit, null, false);
                    break;
            }
        }






#if false
        IEnumerator RequestSlotSpinFromMock(Action successCallback = null, Action<string> errorCallback = null)
        {
            bool isNext = false;
            bool isBreak = false;
            long totalBet = TotalBet;

            JSONNode resNode = null;

            MockDataG200Controller.Instance.RequestSlotSpin(totalBet,
                (JSONNode res) =>
                {
                    resNode = res;
                    isNext = true;
                },
                (BagelCodeError err) =>
                {
                    if (errorCallback != null)
                        errorCallback.Invoke(err.msg);

                    isNext = true;
                    isBreak = true;
                });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            if (isBreak) yield break;


            // 彩金数据
            ERPushMachineDataManager02.Instance.RequestJackpotGame((res) =>
            {
                JackpotRes info = res as JackpotRes;
                ContentModel.Instance.jpGameRes = info;
                isNext = true;
            }, (err) =>
            {
                if (errorCallback != null)
                    errorCallback.Invoke(err.msg);
                isNext = true;
                isBreak = true;
            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            if (isBreak)  yield break;


            //解析数据
            MockDataG200Controller.Instance.ParseSlotSpin(totalBet, resNode);

            // 数据保存

            // 刷新游戏jp滚轮
            SetUIJackpotGameReel();

            if (successCallback != null)
                successCallback.Invoke();

        }
#endif



        IEnumerator RequestSlotSpinFromMock02(Action successCallback = null, Action<string> errorCallback = null)
        {
            bool isNext = false;
            bool isBreak = false;   

            long totalBet = TotalBet;
 
            JSONNode resNode = null;

            MachineDataG200Controller.Instance.RequestSlotSpinFromMock(TotalBet, (res) =>
            {
                resNode = res;
                isNext = true;
            }, (err) =>
            {
                errorCallback?.Invoke(err.msg);
                isNext = true;
                isBreak = true;
            });


            yield return new WaitUntil(() => isNext == true);
            isNext = false;


            int code = (int)resNode["code"]; //:0表示成功，-1表示传参失败

            if (code != 0)
            {
                TestManager.Instance.ShowTip($"Spin数据有误  code:{code}");
                DebugUtils.LogError($"Spin数据有误  code:{code}");
                errorCallback?.Invoke($"Spin数据有误 code:{code}");
                yield break;
            }


            if (isBreak) yield break;


            SBoxJackpotData sboxJackpotData = null;

            // 获取彩金贡献值
            int cacheTotalJpMajor = SQLitePlayerPrefs03.Instance.GetInt(CACHE_TOTAL_JP_MAJOR_CONTRIBUTION, 0);
            int cacheTotalJpGrand = SQLitePlayerPrefs03.Instance.GetInt(CACHE_TOTAL_JP_GRAND_CONTRIBUTION, 0);

            TestManager.Instance.ShowTip("请求彩金贡献值");

            ERPushMachineDataManager02.Instance.RequestGetJpMajorGrandContribution((res) =>
            {
                JSONNode data = JSONNode.Parse((string)res);

                int code = (int)data["code"];
                if (0 != code)
                {
                    //errorCallback?.Invoke("请求贡献值报错"); 不开流程

                    TestManager.Instance.ShowTip($"请求彩金贡献值。  code: {code}");

                    isNext = true;
                    return;
                }

                int majorBet = (int)data["major"];
                int grandBet = (int)data["grand"];

                // 【保存数据，等下行时，删除数据】。
                cacheTotalJpMajor += majorBet;
                cacheTotalJpGrand += grandBet;
                SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_MAJOR_CONTRIBUTION, cacheTotalJpMajor);
                SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_GRAND_CONTRIBUTION, cacheTotalJpGrand);

                JackBetInfoCoinPush info = new JackBetInfoCoinPush()
                {
                    gameType = 1,
                    seat = SBoxModel.Instance.seatId,
                    betPercent = 1 * 100,
                    scoreRate = 1 * 1000,
                    JPPercent = 1 * 1000,
                    majorBet = majorBet * 100,
                    grandBet = grandBet * 100,
                };

                // 没有登录后台彩金
                if (!NetClineBiz.Instance.isLoginSuccess)
                {
                    isNext = true;
                    return;
                }

                // 没有后台彩金
                if (!ClientWS.Instance.IsConnected  && NetClientHelper02.Instance.isUseReelData)  
                {
                    isNext = true;
                    return;
                }

                TestManager.Instance.ShowTip("请求后台彩金数据");

                NetClientHelper02.Instance.RequestJackBetMajorGrand(info, (res) =>
                {
                    //#seaweed#>>>

                    JackBetInfoCoinPushR result = res as JackBetInfoCoinPushR;
                    
                    if(result.code != 0)
                    {
                        // 保留报错信息   info01.msg
                        isNext = true;
                        return;
                    }


                    //#seaweed#<<<
                    sboxJackpotData = result.sboxJackpotData;
                    //sboxJackpotData = res as SBoxJackpotData;  //#seaweed#


                    // 【联网彩金，请求成功 ，删除数据】
                    cacheTotalJpMajor -= majorBet;
                    cacheTotalJpGrand -= grandBet;
                    SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_MAJOR_CONTRIBUTION, cacheTotalJpMajor);
                    SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_GRAND_CONTRIBUTION, cacheTotalJpGrand);

         

                    for (int i = 0; i < sboxJackpotData.Jackpotlottery.Length; i++)
                        sboxJackpotData.Jackpotlottery[i] = sboxJackpotData.Jackpotlottery[i] / 100;

                    for (int i = 0; i < sboxJackpotData.JackpotOut.Length; i++)
                        sboxJackpotData.JackpotOut[i] = sboxJackpotData.JackpotOut[i] / 100;

                    for (int i = 0; i < sboxJackpotData.JackpotOld.Length; i++)
                        sboxJackpotData.JackpotOld[i] = sboxJackpotData.JackpotOld[i] / 100;

                    // 【如果获取到联网彩金-通知算法卡】
                    if (sboxJackpotData.Lottery[0] == 1)
                    {
                        ERPushMachineDataManager02.Instance.RequestSetMajorGrandWin(sboxJackpotData.Jackpotlottery[0], (res) =>
                        {

                        });
                    }
                    if (sboxJackpotData.Lottery[1] == 1)
                    {
                        ERPushMachineDataManager02.Instance.RequestSetMajorGrandWin(sboxJackpotData.Jackpotlottery[1], (res) =>
                        {

                        });
                    }
                    isNext = true;

                }, (err) => // 联网彩金，请求失败
                {
                    //errorCallback?.Invoke(err.msg); // 不卡流程
                    isNext = true;
                });
                
            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;



            //if (isBreak) yield break;


            // 【贡献返回给算法卡】
            if (cacheTotalJpMajor > 10 || cacheTotalJpGrand > 10)
            {
                ERPushMachineDataManager02.Instance.RequestReturnMajorGrandContribution(
                    cacheTotalJpMajor > 10 ? cacheTotalJpMajor : 0,
                    cacheTotalJpGrand > 10 ? cacheTotalJpGrand : 0,
                    (res) => {

                        if ((int)res == 0)
                        {
                            if (cacheTotalJpMajor > 10)
                            {
                                cacheTotalJpMajor = 0;
                                SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_MAJOR_CONTRIBUTION, 0);
                            }

                            if (cacheTotalJpGrand > 10)
                            {
                                cacheTotalJpGrand = 0;
                                SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_GRAND_CONTRIBUTION, 0);
                            }
                        }

                        isNext = true;
                    });

                yield return new WaitUntil(() => isNext == true);
                isNext = false;
            }


            // 解析数据
            MachineDataG200Controller.Instance.ParseSlotSpin(totalBet, resNode, sboxJackpotData);

            // 数据入库
            MachineDataG200Controller.Instance.TestRecord();

            // 游戏彩金滚轮
            SetUIJackpotGameReel();


            // 数据上报
            MachineDataG200Controller.Instance.Report();

            if (successCallback != null)
                successCallback.Invoke();
        }




        const string CACHE_TOTAL_JP_MAJOR_CONTRIBUTION = "CACHE_TOTAL_JP_MAJOR_CONTRIBUTION";
        const string CACHE_TOTAL_JP_GRAND_CONTRIBUTION = "CACHE_TOTAL_JP_GRAND_CONTRIBUTION";
        IEnumerator RequestSlotSpinFromMachine(Action successCallback = null, Action<string> errorCallback = null)
        {
            long totalBet = TotalBet;
            ///bool isBreak = false;
            bool isNext = false;

            // int errorCode = 0;

            JSONNode resNode = null;

            ERPushMachineDataManager02.Instance.RequestCoinPushSpin((res) =>
            {
                resNode = JSONNode.Parse((string)res);
                isNext = true;
            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;


            int code = (int)resNode["code"]; //:0表示成功，-1表示传参失败
            if (code != 0)
            {
                TestManager.Instance.ShowTip($"Spin数据有误 code:{code}");
                DebugUtils.LogError($"Spin数据有误  code:{code}");
                errorCallback?.Invoke($"Spin数据有误 code:{code}");
                yield break;
            }



            SBoxJackpotData sboxJackpotData = null;

            // 获取彩金贡献值
            int cacheTotalJpMajor = SQLitePlayerPrefs03.Instance.GetInt(CACHE_TOTAL_JP_MAJOR_CONTRIBUTION, 0);
            int cacheTotalJpGrand = SQLitePlayerPrefs03.Instance.GetInt(CACHE_TOTAL_JP_GRAND_CONTRIBUTION, 0);

            TestManager.Instance.ShowTip("请求彩金贡献值");

            ERPushMachineDataManager02.Instance.RequestGetJpMajorGrandContribution((res) =>
            {
                JSONNode data = JSONNode.Parse((string)res);

                int code = (int)data["code"];
                if (0 != code)
                {

                    DebugUtils.LogError($"请求贡献值报错。 code: {code}");

                    //  errorCallback?.Invoke("请求贡献值报错");  // 不开流程
                    isNext = true;
                    return;
                }

                int majorBet = (int)data["major"];
                int grandBet = (int)data["grand"];

                // 【保存数据，等下行时，删除数据】。
                cacheTotalJpMajor += majorBet;
                cacheTotalJpGrand += grandBet;
                SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_MAJOR_CONTRIBUTION, cacheTotalJpMajor);
                SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_GRAND_CONTRIBUTION, cacheTotalJpGrand);

                JackBetInfoCoinPush info = new JackBetInfoCoinPush()
                {
                    gameType = 1,
                    seat = SBoxModel.Instance.seatId,
                    betPercent = 1 * 100,
                    scoreRate = 1 * 1000,
                    JPPercent = 1 * 1000,
                    majorBet = majorBet * 100,
                    grandBet = grandBet * 100,
                };


                // 没有登录后台彩金
                if (!NetClineBiz.Instance.isLoginSuccess)
                {
                    isNext = true;
                    return;
                }

                /*
                // 没有联网彩金， 且非mock模式
                if (!ClientWS.Instance.IsConnected && !ApplicationSettings.Instance.isMock)
                {
                    isNext = true;
                    return;
                }
                */

                if (!ClientWS.Instance.IsConnected )
                {
                    isNext = true;
                    return;
                }

                TestManager.Instance.ShowTip("请求后台彩金数据");

                NetClientHelper02.Instance.RequestJackBetMajorGrand(info, (res) =>
                {

                    //#seaweed#>>>

                    JackBetInfoCoinPushR result = res as JackBetInfoCoinPushR;

                    if (result.code != 0)
                    {
                        // 保留报错信息   info01.msg
                        isNext = true;
                        return;
                    }

                    //sboxJackpotData = res as SBoxJackpotData;
                    sboxJackpotData = result.sboxJackpotData;

                    // 【联网彩金，请求成功 ，删除数据】
                    cacheTotalJpMajor -= majorBet;
                    cacheTotalJpGrand -= grandBet;
                    SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_MAJOR_CONTRIBUTION, cacheTotalJpMajor);
                    SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_GRAND_CONTRIBUTION, cacheTotalJpGrand);


                    for (int i = 0; i < sboxJackpotData.Jackpotlottery.Length; i++)
                        sboxJackpotData.Jackpotlottery[i] = sboxJackpotData.Jackpotlottery[i] / 100;

                    for (int i = 0; i < sboxJackpotData.JackpotOut.Length; i++)
                        sboxJackpotData.JackpotOut[i] = sboxJackpotData.JackpotOut[i] / 100;

                    for (int i = 0; i < sboxJackpotData.JackpotOld.Length; i++)
                        sboxJackpotData.JackpotOld[i] = sboxJackpotData.JackpotOld[i] / 100;

                    // 【如果获取到联网彩金-通知算法卡】
                    if (sboxJackpotData.Lottery[0] == 1)
                    {
                        ERPushMachineDataManager02.Instance.RequestSetMajorGrandWin(sboxJackpotData.Jackpotlottery[0], (res) =>
                        {
                            int code = (int)res;
                            if (0 != code)
                                DebugUtils.LogError($"存入Major、Grand赢分时，报错。 code: {code}");       
                        });
                    }
                    if (sboxJackpotData.Lottery[1] == 1)
                    {
                        ERPushMachineDataManager02.Instance.RequestSetMajorGrandWin(sboxJackpotData.Jackpotlottery[1], (res) =>
                        {
                            int code = (int)res;
                            if (0 != code)
                                DebugUtils.LogError($"存入Major、Grand赢分时，报错。 code: {code}");
                        });
                    }
                    isNext = true;

                }, (err) => // 联网彩金，请求失败
                {
                    // 算法卡请求失败
                    //errorCallback?.Invoke(err.msg + $"[{ErrorCode.JACKPOT_GAME_REQUEST_ERR}]");  // 不卡流程
                    isNext = true;
                });

            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            // 请求算法卡失败，请求后台彩金失败 都不要卡流程
            //if (isBreak) yield break;

            // 【贡献值返还给算法卡】
            if (cacheTotalJpMajor > 10 || cacheTotalJpGrand > 10)
            {
                TestManager.Instance.ShowTip("贡献值返还给算法卡");
                ERPushMachineDataManager02.Instance.RequestReturnMajorGrandContribution(
                    cacheTotalJpMajor > 10? cacheTotalJpMajor: 0,
                    cacheTotalJpGrand > 10? cacheTotalJpGrand: 0,
                    (res) =>{
                    
                        if((int)res == 0)
                        {
                            if(cacheTotalJpMajor > 10)
                            {
                                cacheTotalJpMajor = 0;
                                SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_MAJOR_CONTRIBUTION, 0);
                            }

                            if (cacheTotalJpGrand > 10)
                            {
                                cacheTotalJpGrand = 0;
                                SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_GRAND_CONTRIBUTION, 0);
                            }
                        }

                        isNext = true;
                 });

                yield return new WaitUntil(() => isNext == true);
                isNext = false;

            }

            
            try
            {
                // 解析数据
                MachineDataG200Controller.Instance.ParseSlotSpin(totalBet, resNode, sboxJackpotData);


                // 数据入库
                MachineDataG200Controller.Instance.TestRecord();

                // ui 彩金
                SetUIJackpotGameReel();

                // 数据上报
                MachineDataG200Controller.Instance.Report();
            }
            catch (Exception ex)
            { }


            if (successCallback != null)
                successCallback.Invoke();
        }

        public void SetUIJackpotGameReel()
        {
            JackpotRes info = ContentModel.Instance.jpGameRes;

            ContentModel.Instance.uiGrandJP.nowCredit = uiJPGrandCtrl.nowData;
            //ContentModel.Instance.uiMegaJP.nowCredit = uiJPMegaCtrl.nowData;
            ContentModel.Instance.uiMajorJP.nowCredit = uiJPMajorCtrl.nowData;
            ContentModel.Instance.uiMinorJP.nowCredit = uiJPMinorCtrl.nowData;
            ContentModel.Instance.uiMiniJP.nowCredit = uiJPMiniCtrl.nowData;

            ContentModel.Instance.uiGrandJP.curCredit = info.curJackpotGrand;
            //ContentModel.Instance.uiMegaJP.curCredit = info.curJackpotMega;
            ContentModel.Instance.uiMajorJP.curCredit = info.curJackpotMajor;
            ContentModel.Instance.uiMinorJP.curCredit = info.curJackpotMinior;
            ContentModel.Instance.uiMiniJP.curCredit = info.curJackpotMini;

            // 游戏滚轮显示
            uiJPGrandCtrl.SetData(ContentModel.Instance.jpGameWhenCreditLst[0]);
            //uiJPMegaCtrl.SetData(ContentModel.Instance.jpGameWhenCreditLst[1]);
            uiJPMajorCtrl.SetData(ContentModel.Instance.jpGameWhenCreditLst[1]);
            uiJPMinorCtrl.SetData(ContentModel.Instance.jpGameWhenCreditLst[2]);
            uiJPMiniCtrl.SetData(ContentModel.Instance.jpGameWhenCreditLst[3]);

        }

        #region spine 、 TotalSpins按钮


        void OnPanelInputEvent(EventData res)
        {
            switch (res.name)
            {
                case PanelEvent.SpinButtonClick:
                    {
                        OnClickSpinButton(res);
                    }
                    break;
                case PanelEvent.TotalSpinsButtonClick:
                    {
                        OnClickTotalSpinsButtonClick(res);
                    }
                    break;
            }
        }


        void OnClickTotalSpinsButtonClick(EventData res)
        {
            if (ContentModel.Instance.isSpin || ContentModel.Instance.isAuto)
                return;

            int num = (int)res.value;
            if (num != -1)
            {
                ContentModel.Instance.totalPlaySpins = num;
            }
            else{
                switch (ContentModel.Instance.totalPlaySpins)
                {
                    case 1:
                        ContentModel.Instance.totalPlaySpins = 3;
                        break;
                    case 3:
                        ContentModel.Instance.totalPlaySpins = 5;
                        break;
                    case 5:
                    default:
                        ContentModel.Instance.totalPlaySpins = 1;
                        break;
                }
            }
            ContentModel.Instance.remainPlaySpins = ContentModel.Instance.totalPlaySpins;
        }

        void OnClickSpinButton(EventData res)
        {
            if (res.name != PanelEvent.SpinButtonClick) return;

            bool isLongClick = (bool)res.value;
            switch (ContentModel.Instance.btnSpinState)
            {
                case SpinButtonState.Stop:
                    {
                        if (ContentModel.Instance.isSpin) return; // 已经开始玩直接退出

                        ContentModel.Instance.isSpin = true;

                        Action successCallback = () =>
                        {
                            DebugUtils.Log("游戏结束");
                            ContentModel.Instance.isSpin = false;
                            ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
                            ContentModel.Instance.gameState = GameState.Idle;
                        };

                        if (isLongClick)
                        {
                            TestManager.Instance.ShowTip("Spin按钮 - 长按");

                            ContentModel.Instance.isAuto = true;
                            ContentModel.Instance.btnSpinState = SpinButtonState.Auto;
                            
                            StartGameAuto(successCallback, StopGameWhenError); //自动玩
                        }
                        else
                        {
                            TestManager.Instance.ShowTip("Spin按钮 - 短按");

                            //ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                            //StartGameOnce(successCallback, StopGameWhenError);//开始玩

                            ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                            StartGameTotalSpins(successCallback, StopGameWhenError); //开始玩
                        }


                    }
                    break;

                case SpinButtonState.Spin:
                    {
                        // 已经在游戏时，去停止游戏
                        if (!ContentModel.Instance.isSpin) return; // 已经停止直接退出

                        slotMachineCtrl.isStopImmediately = true; // 去停止游戏  

                        SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.StopImmediately);
                    }
                    break;
                case SpinButtonState.Auto:
                    {
                        //停止自动玩
                        ContentModel.Instance.isSpin = true;
                        ContentModel.Instance.isAuto = false;
                        ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                    }
                    break;
            }
        }

        #endregion




        private void StopGameWhenError(string msg)
        {
            ContentModel.Instance.isSpin = false;
            ContentModel.Instance.isAuto = false;
            ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
            ContentModel.Instance.gameState = GameState.Idle;


            // 有好酷优先用好酷
            if (false && SBoxModel.Instance.isUseIot && tipCoinIn)
            {
                /*
                tipCoinIn = false;

                if (!DeviceIOTPayment.Instance.isIOTConneted)
                {
                    TipPopupHandler.Instance.OpenPopupOnce(string.Format(I18nMgr.T("IOT connection failed [{0}]"), Code.DEVICE_IOT_MQTT_NOT_CONNECT));
                }
                else if (!DeviceIOTPayment.Instance.isIOTSignInGetQRCode)
                {
                    TipPopupHandler.Instance.OpenPopupOnce(string.Format(I18nMgr.T("IOT connection failed [{0}]"), Code.DEVICE_IOT_NOT_SIGN_IN));
                }
                else
                {
                    DeviceIOTPayment.Instance.DoQrCoinIn();
                }
                return;
                */
            }
            else
            {
                if (!string.IsNullOrEmpty(msg))
                {
                    string massage = I18nMgr.T(msg);
                    TipPopupHandler.Instance.OpenPopupOnce(massage);
                }
            }

        }


        /*void StartGameOnce(Action successCallback = null, Action<string> errorCallback = null)
        {
            if (corGameOnce != null) mono.StopCoroutine(corGameOnce);
            corGameOnce = mono.StartCoroutine(GameOnce(successCallback, errorCallback));
        }*/

        void StartGameOnce(Action successCallback = null, Action<string> errorCallback = null)
        {
            ContentModel.Instance.totalPlaySpins = 1;
            ContentModel.Instance.remainPlaySpins = 1;
            StartGameTotalSpins(successCallback, errorCallback);
        }

        void StartGameTotalSpins(Action successCallback = null, Action<string> errorCallback = null)
        {
            if (coGameAuto != null) mono.StopCoroutine(coGameAuto);
            coGameAuto = mono.StartCoroutine(GameTotalSpins(successCallback, errorCallback));
        }


        void StartGameAuto(Action successCallback = null, Action<string> errorCallback = null)
        {
            if (coGameAuto != null) mono.StopCoroutine(coGameAuto);
            coGameAuto = mono.StartCoroutine(GameAuto(successCallback, errorCallback));
        }


        void OnSlotDetailEvent(EventData res)
        {

            switch (res.name)
            {
                case SlotMachineEvent.PrepareStoppedReel:
                    {
                        if (ContentModel.Instance.isReelsSlowMotion)
                        {
                            int colIndex = (int)res.value;
                            if (colIndex == 1)
                            {
                                isEffectSlowMotion2 = true;
                                isEffectSlowMotion3 = false;
                                isEffectSlowMotion4 = false;
                            }
                            else if (colIndex == 2)
                            {

                                isEffectSlowMotion2 = false;
                                isEffectSlowMotion3 = true;
                                isEffectSlowMotion4 = false;
                            }
                            else if (colIndex == 3)
                            {

                                isEffectSlowMotion2 = false;
                                isEffectSlowMotion3 = false;
                                isEffectSlowMotion4 = true;
                            }
                        }
                    }
                    break;
            }

        }


        //IEnumerator CollectGoldDown( List<SymbolWin> winList,int credit,bool isNext)
        //{

        //}



        void OnStopSlot(EventData res)
        {
            switch (res.name)
            {
                case SlotMachineEvent.StoppedSlotMachine:
                    {
                        isStoppedSlotMachine = true;
                    }
                    break;
            }
        }


        #region 实时同步玩家彩分

        bool isGetCrediting = false;

        //Queue<Action<int>> GetMyCreditSuccessQueque = new Queue<Action<int>>();
        //Queue<Action<string>> GetMyCreditFailQueque = new Queue<Action<string>>();

        //Action<int> onGetMyCreditSuccessCallback;
        //Action<string> onGetMyCreditErrorCallback;

        void GetMyCredit(Action<int> onSuccessCallback, Action<string> onErrorCallback)
        {
            //GetMyCreditSuccessQueque.Enqueue(onSuccessCallback);
            //GetMyCreditFailQueque.Enqueue(onErrorCallback);
            // onGetMyCreditSuccessCallback = onSuccessCallback;
            // onGetMyCreditErrorCallback = onErrorCallback;
            //if (isGetCrediting == true) return;

            isGetCrediting = true;

            ERPushMachineDataManager02.Instance.RequestGetMyCredit((res) =>
            {
                isGetCrediting = false;
                try
                {
                    int myCredit = (int)res;

                    /*while (GetMyCreditSuccessQueque.Count > 0)
                    {
                        Action<int>  func = GetMyCreditSuccessQueque.Dequeue();
                        func?.Invoke(myCredit);
                    }*/

                    //onGetMyCreditSuccessCallback?.Invoke(myCredit);

                    onSuccessCallback?.Invoke(myCredit);
                }
                catch (Exception ex)
                {
                    DebugUtils.LogError(ex);
                    DebugUtils.LogError(res);

                    /*while (GetMyCreditFailQueque.Count > 0)
                    {
                        Action<string> func = GetMyCreditFailQueque.Dequeue();
                        func?.Invoke(ex.Message);
                    }*/

                    //onGetMyCreditErrorCallback?.Invoke(ex.Message);

                    onErrorCallback?.Invoke(ex.Message);
                }

            });
        }




        void OnSboxIdeaInfo(SBoxIdeaInfo info)
        {

            int myCredit = 0;

            foreach (SBoxPlayerScoreInfo item in info.PlayerScoreInfoList) // SBoxIdea.sBoxInfo.PlayerScoreInfoList)
            {
                if (item.PlayerId == 1)
                {
                    myCredit = item.Score;
                }
            }
            if (SBoxModel.Instance.myCredit != myCredit)
            {
                DebugUtils.LogWarning($"定时同步玩家分数： {myCredit}");
                SBoxModel.Instance.myCredit = myCredit;
                MainBlackboardController.Instance.SyncMyTempCreditToReal(true);
            }

        }

        void GetMyCreditRepeat(object arg)
        {
            if (isGetCrediting)
                return;

            GetMyCredit((myCredit) =>
                {
                    if (SBoxModel.Instance.myCredit != myCredit)
                    {
                        DebugUtils.LogWarning($"定时同步玩家分数： {myCredit}");
                        SBoxModel.Instance.myCredit = myCredit;
                        MainBlackboardController.Instance.SyncMyTempCreditToReal(true);
                    }
                },
                (errStr) =>
                {

                });
            /*
            ERPushMachineDataManager02.Instance.RequesGetMyCredit((res) =>
            {
                try
                {
                    int myCredit = (int)res;
                    if (SBoxModel.Instance.myCredit != myCredit)
                    {
                        DebugUtils.LogWarning($"同步玩家分数： {myCredit}");
                        SBoxModel.Instance.myCredit = myCredit;
                        MainBlackboardController.Instance.SyncMyTempCreditToReal(true);
                    }
                }
                catch (Exception ex)
                {
                    DebugUtils.LogError(ex);
                    DebugUtils.LogError(res);
                    //throw ex;
                }

            });*/
        }
        #endregion






        void ParseGameInfo()
        {
            GameConfigRoot config = JsonConvert.DeserializeObject<GameConfigRoot>(gameInfo.text);
            if (config?.SymbolPaytable == null)
            {
                DebugUtils.LogError("解析symbol_paytable失败，数据为空");
                return;
            }

            MainModel.Instance.gameID = config.GameId;
            MainModel.Instance.gameName = config.GameName;
            MainModel.Instance.displayName = config.DisplayName;
            foreach (var item in config.WinLevelMultiple)
            {
                string winKey = item.Key;
                long winValue = item.Value;
                MainModel.Instance.contentMD.winLevelMultiple.Add(new WinMultiple(winKey, winValue));
            }

            ContentModel.Instance.payTableSymbolWin = new List<PayTableSymbolInfo>();
            foreach (var kvp in config.SymbolPaytable)
            {

                string symbolKey = kvp.Key; // 如 "s0"、"s1"、"s2"
                var jsonData1 = kvp.Value; // 对应x3、x4、x5的数据

                // 1. 从symbolKey中提取索引（如"s0" → 0，"s1" → 1）
                if (int.TryParse(symbolKey.Replace("s", ""), out int index))
                {
                    // 2. 检查索引是否在列表有效范围内
                    if (index >= 0)
                    {
                        PayTableSymbolInfo item = new PayTableSymbolInfo()
                        {
                            symbol = index,
                            x3 = jsonData1.x3,
                            x4 = jsonData1.x4,
                            x5 = jsonData1.x5,
                        };
                        ContentModel.Instance.payTableSymbolWin.Add(item);
                    }
                }
                else
                {
                    DebugUtils.LogError($"无效的符号键：{symbolKey}，无法解析索引");
                }
            }
        

           // DebugUtils.LogError($"payTableSymbolWin ({ContentModel.Instance.name}) = ：{JsonConvert.SerializeObject(ContentModel.Instance.payTableSymbolWin)}");

            ContentModel.Instance.payLines = new List<List<int>>() { };
            foreach (var item in config.pay_lines)
            {
                ContentModel.Instance.payLines.Add(item);
            }

            payTableController.OnPropertyChangeTotalBet();
        }



    }
}