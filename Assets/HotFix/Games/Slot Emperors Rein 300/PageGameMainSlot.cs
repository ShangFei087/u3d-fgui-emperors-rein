using FairyGUI;
using GameMaker;
using GlobalJackpotConsole;
using Newtonsoft.Json;
using PusherEmperorsRein;
using SBoxApi;
using SimpleJSON;
using Sirenix.OdinInspector;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using _spinWEMD = SlotMaker.SpinWinEffectSettingModel;
using ContentModel = SlotEmperorsRein.ContentModel;


namespace SlotEmperorsRein
{


    public class GameConfigRoot1
    {
        [JsonProperty("game_id")] public int GameId;

        [JsonProperty("game_name")] public string GameName;

        [JsonProperty("display_name")] public string DisplayName;

        [JsonProperty("win_level_multiple")] public Dictionary<string, long> WinLevelMultiple { get; set; }

        [JsonProperty("symbol_paytable")] public Dictionary<string, PayTableSymbolInfo> SymbolPaytable { get; set; }

        [JsonProperty("pay_lines")] public List<List<int>> pay_lines { get; set; }
    }

    public class PageGameMainSlot : PageBase
    {
        public const string pkgName = "EmperorsRein";
        public const string resName = "PageGameMainSlot";


        private GComponent gOwnerPanel;
        long TotalBet => (long)ContentModel.Instance.totalBet;



        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);

            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnClickSpinButton);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT, OnSlotDetailEvent);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnStopSlot);

            EventCenter.Instance.AddEventListener<string>(MachineDataManager02.RpcNameJackpotOnLine, OnJackpotOnLine);

            InitParam();
            //Dictionary<string, object> args = new Dictionary<string, object>()
            //{
            //    ["isCommon"] = false,
            //};
            //PageManager.Instance.OpenPage(PageName.SlotEmperorsReinPageFreeBonusGame1,
            //    new EventData<Dictionary<string, object>>("", args));
        }

        public override void OnClose(EventData data = null)
        {
            winTipCtrl.Dispose();

            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnClickSpinButton);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT,
                OnSlotDetailEvent);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnStopSlot);
            EventCenter.Instance.RemoveEventListener<string>(MachineDataManager02.RpcNameJackpotOnLine, OnJackpotOnLine);
            base.OnClose(data);
        }

        // public override void OnTop() { DebugUtils.Log($"i am top {this.name}"); }

        private void ReadJsonBet()
        {

            ResourceManager02.Instance.LoadAsset<TextAsset>(
                "Assets/GameRes/_Common/Game Maker/ABs/G300/Datas/game_info_g300.json", (txt) =>
                {
                    GameConfigRoot1 config = JsonConvert.DeserializeObject<GameConfigRoot1>(txt.text);
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
                                // 3. 为列表中对应索引的元素赋值
                                var targetItem = MainModel.Instance.contentMD.payTableSymbolWin[index];
                                targetItem.x3 = jsonData1.x3; // 假设jsonData的属性是X3（根据实际定义调整）
                                targetItem.x4 = jsonData1.x4;
                                targetItem.x5 = jsonData1.x5;
                                // 若需要同步symbol字段（可选，确保一致）
                                targetItem.symbol = index;
                            }
                        }
                        else
                        {
                            DebugUtils.LogWarning($"无效的符号键：{symbolKey}，无法解析索引");
                        }
                    }
                    if (ContentModel.Instance.payLines == null)
                    {
                        ContentModel.Instance.payLines = new List<List<int>>() { };
                    }

                    foreach (var item in config.pay_lines)
                    {
                        ContentModel.Instance.payLines.Add(item);
                    }

                    payTableController.OnPropertyChangeTotalBet();
                });
        }



        protected override void OnInit()
        {
            
            base.OnInit();

            int count = 10;

            Action callback = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam();
                }
            };

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Game Controller/Slot Game Main Controller.prefab",
                (GameObject clone) =>
                {
                    goGameCtrl = GameObject.Instantiate(clone);
                    goGameCtrl.name = "Game Main Controller";
                    goGameCtrl.transform.SetParent(null);

                    slotMachineCtrl = goGameCtrl.transform.Find("Slot Machine").GetComponent<SlotMachineController300>();

                    mono = goGameCtrl.transform.GetComponent<MonoHelper>();
                    DebugUtils.Log(mono);

                    DebugUtils.LogWarning("i am Game Controller");

                    fguiPoolHelper = goGameCtrl.transform.Find("Pool").GetComponent<FguiPoolHelper>();


                    gObjectPoolHelper = goGameCtrl.transform.Find("GObject Pool").GetComponent<FguiGObjectPoolHelper>();

                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/PageERGameMain/KingLion.prefab",
                (GameObject clone) =>
                {
                    goNpc = GameObject.Instantiate(clone);
                    animatorKingLion = goNpc.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                    callback();
                });



            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/PageERGameMain/anchorNPCQiang.prefab",
                (GameObject clone) =>
                {
                    goNpc2 = GameObject.Instantiate(clone);
                    animatorKing = goNpc2.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                    callback();
                });



            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/PageERGameMain/BgQiang.prefab",
                (GameObject clone) =>
                {
                    goBg2 = GameObject.Instantiate(clone);
                    callback();
                });




            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/PageERGameMain/Bg0.prefab",
                (GameObject clone) =>
                {
                    goBg = GameObject.Instantiate(clone);
                    callback();
                });



            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/PageERGameMain/Qi.prefab",
                (GameObject clone) =>
                {
                    goQi = GameObject.Instantiate(clone);
                    callback();
                });



            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/PageERGameMain/anchorWin02.prefab",
                (GameObject clone) =>
                {
                    goGoldCoin = GameObject.Instantiate(clone);
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/PageERGameMain/anchorWin01.prefab",
                (GameObject clone) =>
                {
                    goDi = GameObject.Instantiate(clone);
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/PageERGameMain/ReelEffect.prefab",
                (GameObject clone) =>
                {
                    goReelEffcet = GameObject.Instantiate(clone);
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/PageERGameMain/FsEffect01.prefab",
                (GameObject clone) =>
                {
                    goFsEffect01 = GameObject.Instantiate(clone);
                    callback();
                });




        }

        PayTableController payTableController = new PayTableController();
        GameObject goGameCtrl;
        GComponent gSlotCover, gPlayLines, gFrame;
        GLabel labWinTip;
        Animator animatorKingLion, animatorKing;
        GComponent anchorExpectation;
        GComponent ComQi, ComEFDropGoldCoin, ComGoldDi, ComReelEffect2, ComFsEffect01;
        List<GComponent> Bonuslist = new List<GComponent>();
        GameObject goNpc, goNpc2, goBg, goBg2, goQi, goGoldCoin, goDi, goReelEffcet, goFsEffect01, goGoldBack;



        WinTipController winTipCtrl = new WinTipController();

        MiniReelGroup uiJPGrangCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMajorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMinorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMiniCtrl = new MiniReelGroup();

        
        bool isInitPool = false;

        FguiPoolHelper fguiPoolHelper;
        FguiGObjectPoolHelper gObjectPoolHelper;

        SlotMachineController300 slotMachineCtrl;
        MonoHelper mono;
        bool isStoppedSlotMachine = false;
        bool isEffectSlowMotion2 = false;
        bool isEffectSlowMotion3 = false;
        bool isEffectSlowMotion4 = false;
        Coroutine corInit, corGameOnce, corGameAuto, corGameIdel, corEffectSlowMotion, corReelsTurn;

        bool tipCoinIn = false;
        List<BonusWin> bonusWins = new List<BonusWin>();
        bool isAddCreditAnim => !(slotMachineCtrl.isStopImmediately == true || SBoxModel.Instance.isCoinOutImmediately);

        void TestScreen()
        {
            /*
            // 获取FairyGUI的StageCamera
            Camera stageCamera = GRoot.inst.GetCamera();
            // 获取视口分辨率
            float viewportWidth = stageCamera.pixelRect.width;
            float viewportHeight = stageCamera.pixelRect.height;
            // 打印分辨率
            DebugUtils.Log($"当前视口分辨率: stageCamera.pixelRect.width x height = {viewportWidth} x {viewportHeight}");
            */

            Stage stage = Stage.inst;
            // 获取视口大小
            float viewportWidth1 = stage.width;
            float viewportHeight1 = stage.height;
            // 打印分辨率
            DebugUtils.Log($"当前视口分辨率: stage.width x height = {viewportWidth1} x {viewportHeight1}");


            // 获取屏幕分辨率（可能受UI缩放影响）
            float screenWidth = GRoot.inst.width;
            float screenHeight = GRoot.inst.height;
            // 打印分辨率
            DebugUtils.Log($"当前UI视口分辨率: GRoot.inst.width x height = {screenWidth} x {screenHeight}");


            // 获取屏幕的宽度和高度（物理像素）
            int screenWidth1 = Screen.width;
            int screenHeight1 = Screen.height;
            // 打印分辨率
            DebugUtils.Log($"当前屏幕分辨率: Screen.width x height = {screenWidth1} x {screenHeight1}");

            /*
            // 获取当前屏幕设置的分辨率（物理像素）
            Resolution currentRes = Screen.currentResolution;
            // 打印分辨率和刷新率
            DebugUtils.Log($"当前屏幕设置分辨率: Screen.currentResolution.width x height = {currentRes.width} x {currentRes.height} @ {currentRes.refreshRate}Hz");
            */
        }



        public override void InitParam()
        {
  
            if (!isInit) return;

            if (!isOpen) return;

            TestScreen();


            MainModel.Instance.contentMD = ContentModel.Instance;

            List<GComponent> lstPayTable = new List<GComponent>();
            foreach (string url in CustomModel.Instance.payTable)
            {
                GComponent paytable = UIPackage.CreateObjectFromURL(url).asCom;
                lstPayTable.Add(paytable);
            }

            ContentModel.Instance.goPayTableLst = lstPayTable.ToArray();
            payTableController.Init(lstPayTable);

            ReadJsonBet();


            MainBlackboardController.Instance.SetMyRealCredit(10000);
            MainBlackboardController.Instance.SyncMyTempCreditToReal(true);



            GComponent lodAnchorNPC = this.contentPane.GetChild("anchorNPC").asCom;
            GameCommon.FguiUtils.AddWrapper(lodAnchorNPC, goNpc);

            GComponent lodAnchorNPCQiang = this.contentPane.GetChild("anchorNPCQiang").asCom;
            GameCommon.FguiUtils.AddWrapper(lodAnchorNPCQiang, goNpc2);

            GComponent lodAnchorBGQiang = this.contentPane.GetChild("anchorBGQiang").asCom;
            GameCommon.FguiUtils.AddWrapper(lodAnchorBGQiang, goBg2);

            GComponent lodAnchorBG = this.contentPane.GetChild("anchorBG").asCom;
            GameCommon.FguiUtils.AddWrapper(lodAnchorBG, goBg);

            ComQi = this.contentPane.GetChild("anchorQi").asCom;
            GameCommon.FguiUtils.AddWrapper(ComQi, goQi);

            ComEFDropGoldCoin = this.contentPane.GetChild("anchorEFDropGoldCoin").asCom;
            GameCommon.FguiUtils.AddWrapper(ComEFDropGoldCoin, goGoldCoin);


            ComGoldDi = this.contentPane.GetChild("anchorGoldDi").asCom;
            GameCommon.FguiUtils.AddWrapper(ComGoldDi, goDi);

            ComReelEffect2 = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            GameCommon.FguiUtils.AddWrapper(ComReelEffect2, goReelEffcet);
            ComReelEffect2.visible = false;
            anchorExpectation = this.contentPane.GetChild("anchorReel2Effect").asCom;
            anchorExpectation.AddChild(ComReelEffect2);
            anchorExpectation.visible = true;


            //this.contentPane.GetController("c1").selectedPage = "FS";

            ComFsEffect01 = this.contentPane.GetChild("FSFrame").asCom.GetChild("anchorFsEffect01").asCom;
            GameCommon.FguiUtils.AddWrapper(ComFsEffect01, goFsEffect01);

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








            uiJPGrangCtrl.Init("Grand", this.contentPane.GetChild("jpGrand").asCom.GetChild("reels").asList, "N0");
          //  uiJPMegaCtrl.Init("Mega", this.contentPane.GetChild("jpMega").asCom.GetChild("reels").asList, "N0");
            uiJPMajorCtrl.Init("Major", this.contentPane.GetChild("jpMajor").asCom.GetChild("reels").asList, "N0");
            uiJPMinorCtrl.Init("Minor", this.contentPane.GetChild("jpMinor").asCom.GetChild("reels").asList, "N0");
            uiJPMiniCtrl.Init("Mini", this.contentPane.GetChild("jpMini").asCom.GetChild("reels").asList, "N0");

            uiJPGrangCtrl.SetData(50000);
          //  uiJPMegaCtrl.SetData(40000);
            uiJPMajorCtrl.SetData(30000);
            uiJPMinorCtrl.SetData(200);
            uiJPMiniCtrl.SetData(10);

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
                    CustomModel.Instance.symbolAppearEffect.Values.ToList(), "symbol_appear#", 5);
                fguiPoolHelper.PreLoad(TagPoolObject.SymbolAppear);

                //fguiPoolHelper.Init(CustomModel.Instance.symbolHitEffect,CustomModel.Instance.symbolAppearEffect, null,CustomModel.Instance.borderEffect);
            }

            //CoroutineAssistant.DoCo("COR_GAME_MAIN_INIT", InitParam02(this.contentPane));


            GComponent gSlotMachine = contentPane.GetChild("slotMachine").asCom;
            GComponent gReels = gSlotMachine.GetChild("reels").asCom;
            gSlotCover = gSlotMachine.asCom.GetChild("slotCover").asCom;
            gPlayLines = gSlotMachine.asCom.GetChild("playLines").asCom;
            gFrame = contentPane.GetChild("BSFrame").asCom;
            slotMachineCtrl.Init(gSlotCover, gPlayLines, gReels, gFrame, fguiPoolHelper, gObjectPoolHelper);

            /*
                    MachineDataManager02.Instance.RequestSetPlayerInfo(new SBoxPlayerAccount()
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





            preLoadedCallback?.Invoke();
        }



        /*
        IEnumerator InitParam02(GComponent contentPane)
        {

            yield return new WaitForSeconds(1f);
            while (slotMachineCtrl == null)
            {
                yield return null;
            }
            GComponent gSlotMachine = contentPane.GetChild("slotMachine").asCom;
            GComponent gReels = gSlotMachine.GetChild("reels").asCom;
            gSlotCover = gSlotMachine.asCom.GetChild("slotCover").asCom;
            gPlayLines = gSlotMachine.asCom.GetChild("playLines").asCom;
            gFrame = contentPane.GetChild("BSFrame").asCom;
            slotMachineCtrl.Init(gSlotCover, gPlayLines, gReels, gFrame,fguiPoolHelper);
        }
        */


        void OnJackpotOnLine(string res)
        {

            WinJackpotInfo winJPInfo = JsonConvert.DeserializeObject<WinJackpotInfo>(res);
            ContentModel.Instance.jpOnlineWin.Add(winJPInfo);

            // 投币个数入库
            // 彩金数据入库
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

                float time = Time.time;
                while (Time.time - time < 1f) //�ȴ�1��
                {
                    yield return new WaitForSeconds(0.1f);
                    if (!ContentModel.Instance.isAuto)
                        break;
                }
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

            if (isBreak)
            {

                if (errorCallback != null)
                    errorCallback.Invoke(errMsg);
                yield break;
            }


            slotMachineCtrl.BeginSpin();


            if (ContentModel.Instance.isReelsSlowMotion)
            {
                if (corEffectSlowMotion != null) mono.StopCoroutine(corEffectSlowMotion);
                corEffectSlowMotion = mono.StartCoroutine(ShowEffectReelsSlowMotion());

                slotMachineCtrl.ShowSymbolAppearEffectAfterReelStop(true);
            }
            else
            {
                slotMachineCtrl.ShowSymbolAppearEffectAfterReelStop(ContentModel.Instance.winList.Count == 0);
            }


            if (slotMachineCtrl.isStopImmediately)
            {
                //reelsTurnType = ReelsTurnType.Once;

                if (corReelsTurn != null) mono.StopCoroutine(corReelsTurn);
                corReelsTurn = mono.StartCoroutine(slotMachineCtrl.TurnReelsOnce(ContentModel.Instance.strDeckRowCol,
                    () =>
                    {
                        isNext = true;
                    }));

                isNext = false;
                yield return new WaitUntil(() => isNext == true);
            }
            else
            {
                //reelsTurnType = ReelsTurnType.Normal;
                if (corReelsTurn != null) mono.StopCoroutine(corReelsTurn);
                corReelsTurn = mono.StartCoroutine(slotMachineCtrl.TurnReelsNormal(ContentModel.Instance.strDeckRowCol,
                    () =>
                    {
                        isNext = true;
                    }));

                isNext = false;
                yield return new WaitUntil(() => isNext == true || slotMachineCtrl.isStopImmediately == true);

                // 等待移动结束
                if (slotMachineCtrl.isStopImmediately && isNext == false)
                {
                    if (corReelsTurn != null) mono.StopCoroutine(corReelsTurn);
                    corReelsTurn = mono.StartCoroutine(slotMachineCtrl.ReelsToStopOrTurnOnce(() =>
                    {
                        isNext = true;
                    }));
                    isNext = false;
                    yield return new WaitUntil(() => isNext == true);
                }
            }


            List<SymbolWin> winList = ContentModel.Instance.winList;
            long allWinCredit = 0;

            #region Win

            if (winList.Count > 0 || ContentModel.Instance.bonusResult != null)
            {


                if (_spinWEMD.Instance.isSingleWin)
                {
                    mono.StartCoroutine(PlayKing(1f));
                }
                else
                {
                    mono.StartCoroutine(PlayKing(2f));
                }

                long totalWinLineCredit = 0;
                if (winList.Count > 0)
                {
                    totalWinLineCredit = slotMachineCtrl.GetTotalWinCredit(winList);
                    allWinCredit = totalWinLineCredit;
                    yield return ShowWinListOnceAtNormalSpin009(winList);
                }

                if (ContentModel.Instance.bonusResult.Count > 0)
                {


                    yield return BonusBegin();
                    //totalWinLineCredit = slotMachineCtrl.GetTotalBonusCredit(bonusWins);
                    allWinCredit = totalWinLineCredit;
                    yield return ShowSymbolBonusEffect(bonusWins);
                }

                //yield return ShowSingleWinListOnceAtFreeSpin009(winList);  // 变线、5连线

                // 播大奖弹窗
                WinLevelType winLevelType = GetBigWinType();
                if (winLevelType != WinLevelType.None)
                {
                    slotMachineCtrl.ShowSymbolWinDeck(slotMachineCtrl.GetTotalSymbolWin(winList), true);

                    // 大奖弹窗
                    yield return WinPopup(winLevelType, ContentModel.Instance.baseGameWinCredit);

                    slotMachineCtrl.CloseSlotCover();

                    slotMachineCtrl.SkipWinLine(false);
                }
                else
                {
                    // 总线赢分（同步？？）
                    bool isAddToCredit = totalWinLineCredit > ContentModel.Instance.totalBet * 4;
                    slotMachineCtrl.SendPrepareTotalWinCreditEvent(totalWinLineCredit, isAddToCredit);
                }

                if (_spinWEMD.Instance.isSingleWin)
                {
                    mono.StartCoroutine(PlayTotalWinEffectFS());
                }

                // 总线赢分事件
                slotMachineCtrl.SendTotalWinCreditEvent(totalWinLineCredit);

                //加钱动画
                MainBlackboardController.Instance.AddMyTempCredit(totalWinLineCredit, true, isAddCreditAnim);



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

                isNext = false;
                yield return new WaitUntil(() => isNext == true);

                // 总线赢分事件
                slotMachineCtrl.SendTotalWinCreditEvent(allWinCredit);

                MainBlackboardController.Instance.AddMyTempCredit((long)jpWin.winCredit, true, isAddCreditAnim);
            }

            #endregion


            // 【取消】大厅彩金

            // 小游戏


            // 本剧同步玩家金钱
            MainBlackboardController.Instance.SyncMyTempCreditToReal(false);

            // 即中即退
            yield return CoinOutImmediately(allWinCredit);


            #region 免费游戏中，添加额外免费游戏

            if (ContentModel.Instance.isFreeSpinAdd)
            {

                slotMachineCtrl.BeginBonusFreeSpinAdd();
                PageManager.Instance.OpenPageAsync(PageName.PusherEmperorsReinPopupFreeSpinTrigger,
                    new EventData<Dictionary<string, object>>("",
                        new Dictionary<string, object>()
                        {
                            ["freeSpinCount"] = ContentModel.Instance.freeSpinAddNum,
                            ["isAddFreeGame"] = true,
                        }),
                    (ed) =>
                    {
                        isNext = true;
                    });

                isNext = false;
                yield return new WaitUntil(() => isNext == true);


                // 【待修改】重置剩余的局数 
                ContentModel.Instance.showFreeSpinRemainTime = ContentModel.Instance.showFreeSpinRemainTime + 7;


                yield return slotMachineCtrl.SlotWaitForSeconds(1.5f);
                slotMachineCtrl.EndBonusFreeSpinAdd();

            }

            #endregion


            ContentModel.Instance.gameState = GameState.Idle;
            // 先结算主游戏，再进入“免费游戏”或“小游戏”，则每局都可以同步玩家真实金钱金额

            if (successCallback != null)
                successCallback.Invoke();
        }

        IEnumerator GameOnce(Action successCallback, Action<string> errorCallback)
        {


            if (!SBoxModel.Instance.isMachineActive)
            {
                errorCallback?.Invoke("<size=24>Machine not activated!</size>");
                yield break;
            }

            if (SBoxModel.Instance.myCredit < ContentModel.Instance.totalBet)
            {
                tipCoinIn = true;
                errorCallback?.Invoke("<size=15>Balance is insufficient, please recharge first</size>");
                yield break;
            }

            OnGameReset();

            ContentModel.Instance.gameState = GameState.Spin;

            slotMachineCtrl.BeginTurn();

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
                            bet = (int)_contentBB.Instance.totalBet,  // 总压注
                            betPercent = 100, // 固定死
                            scoreRate =  _consoleBB.Instance.jackpotScoreRate,      //10000,  // 1 除以 币值 乘以 1000 整形   （联网彩金分值比 ：只能该币值）
                            JPPercent =  _consoleBB.Instance.jackpotPercent,    //5  // 千分之几（1 - 100 可调 ；名称： 联网彩金比（千分）  ）
                        },
                        null, null
                    );
                    */
                }
            }



            slotMachineCtrl.BeginSpin();


            if (ContentModel.Instance.isReelsSlowMotion)
            {
                if (corEffectSlowMotion != null) mono.StopCoroutine(corEffectSlowMotion);
                corEffectSlowMotion = mono.StartCoroutine(ShowEffectReelsSlowMotion());
                slotMachineCtrl.ShowSymbolAppearEffectAfterReelStop(true);
            }
            else
            {
                slotMachineCtrl.ShowSymbolAppearEffectAfterReelStop(ContentModel.Instance.winList.Count == 0);
            }


            if (slotMachineCtrl.isStopImmediately)
            {
                //reelsTurnType = ReelsTurnType.Once;

                if (corReelsTurn != null) mono.StopCoroutine(corReelsTurn);
                corReelsTurn = mono.StartCoroutine(slotMachineCtrl.TurnReelsOnce(ContentModel.Instance.strDeckRowCol,
                    () =>
                    {
                        isNext = true;
                    }));

                isNext = false;
                yield return new WaitUntil(() => isNext == true);
            }
            else
            {

                if (corReelsTurn != null) mono.StopCoroutine(corReelsTurn);
                corReelsTurn = mono.StartCoroutine(slotMachineCtrl.TurnReelsNormal(ContentModel.Instance.strDeckRowCol,
                    () =>
                    {
                        isNext = true;
                    }));

                isNext = false;
                yield return new WaitUntil(() => isNext == true || slotMachineCtrl.isStopImmediately == true);

                // 等待移动结束
                if (slotMachineCtrl.isStopImmediately && isNext == false)
                {
                    if (corReelsTurn != null) mono.StopCoroutine(corReelsTurn);
                    corReelsTurn = mono.StartCoroutine(slotMachineCtrl.ReelsToStopOrTurnOnce(() =>
                    {
                        isNext = true;
                    }));

                    isNext = false;
                    yield return new WaitUntil(() => isNext == true);
                }
            }


            List<SymbolWin> winList = ContentModel.Instance.winList;
            long allWinCredit = 0;
            if (winList.Count > 0 || ContentModel.Instance.bonusResult != null)
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
                if (winList.Count > 0)
                {
                    totalWinLineCredit = slotMachineCtrl.GetTotalWinCredit(winList);
                    allWinCredit = totalWinLineCredit;
                    yield return ShowWinListOnceAtNormalSpin009(winList);
                }

                if (ContentModel.Instance.bonusResult.Count > 0)
                {

                    yield return BonusBegin();
                    //totalWinLineCredit = slotMachineCtrl.GetTotalBonusCredit(bonusWins);
                    allWinCredit = totalWinLineCredit;
                    yield return ShowSymbolBonusEffect(bonusWins);
                }

                WinLevelType winLevelType = GetBigWinType();
                if (winLevelType != WinLevelType.None)
                {
                    slotMachineCtrl.ShowSymbolWinDeck(slotMachineCtrl.GetTotalSymbolWin(winList), true);

                    // 大奖弹窗
                    yield return WinPopup(winLevelType, ContentModel.Instance.baseGameWinCredit);

                    slotMachineCtrl.CloseSlotCover();

                    slotMachineCtrl.SkipWinLine(false);

                }
                else
                {
                    bool isAddToCredit = totalWinLineCredit > ContentModel.Instance.totalBet * 4;
                    slotMachineCtrl.SendPrepareTotalWinCreditEvent(totalWinLineCredit, isAddToCredit);
                }





                slotMachineCtrl.SendTotalWinCreditEvent(allWinCredit);

                //加钱动画
                MainBlackboardController.Instance.AddMyTempCredit(totalWinLineCredit, true, isAddCreditAnim);


                if (_spinWEMD.Instance.isSingleWin)
                {
                    yield return PlayTotalWinEffectBS();
                }


            }


            #region 中游戏彩金

            bool isHitJackpot = ContentModel.Instance.jpGameWinLst.Count > 0;
            List<JackpotWinInfo> jpRes = ContentModel.Instance.jpGameWinLst;
            List<float> jpCredit = ContentModel.Instance.jpGameWhenCreditLst;
            if (jpRes.Count > 0)
            {
                slotMachineCtrl.SkipWinLine(true);
                slotMachineCtrl.ShowSymbolEffect(TagPoolObject.SymbolHit, new List<int>() { 12 }, true, 12, true);
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

                isNext = false;
                yield return new WaitUntil(() => isNext == true);
                isNext = false;

                // 总线赢分（同步？？）
                slotMachineCtrl.SendTotalWinCreditEvent(allWinCredit);

                MainBlackboardController.Instance.AddMyTempCredit((long)jpWin.winCredit, true, isAddCreditAnim);

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
                        ["toCredit"] = winCredit,
                        ["jackpotType"] = data.jackpotId,
                        //["fromCredit"] = (long)fromCredit
                    }),
                    (res) =>
                    {
                        isNext = true;
                    });
                isNext = false;
                yield return new WaitUntil(() => isNext == true);

                // 总线赢分（同步？？）
                slotMachineCtrl.SendTotalWinCreditEvent(allWinCredit);

                MainBlackboardController.Instance.AddMyTempCredit(winCredit, true, isAddCreditAnim);
            }

            #endregion


            // 小游戏Bonus


            // 本剧同步玩家金钱
            MainBlackboardController.Instance.SyncMyTempCreditToReal(false);

            // 即中即退
            yield return CoinOutImmediately(allWinCredit);

            // Free Spin
            if (ContentModel.Instance.isFreeSpinTrigger)
            {
                yield return FreeSpinTrigger(null, errorCallback);
            }

            // 进入空闲模式
            ContentModel.Instance.gameState = GameState.Idle;
            if (winList.Count > 0 && !ContentModel.Instance.isAuto && !ContentModel.Instance.isFreeSpinTrigger)
            {
                if (corGameIdel != null) mono.StopCoroutine(corGameIdel);
                corGameIdel = mono.StartCoroutine(GameIdle(winList));
            }

            if (successCallback != null)
                successCallback.Invoke();
        }


        IEnumerator FreeSpinTrigger(Action successCallback, Action<string> errorCallback)
        {
            bool isNext = false;

            yield return new WaitForSeconds(3f);

            InputStackContextFreeSpin((context) =>
            {
                //goBgRegular.SetActive(false);
                //goBgFreeSpin.SetActive(true);
                //if (goPanelFreeSpin != null) goPanelFreeSpin.SetActive(true);
                // Transform tfm = goAnchorMain.GetComponent<Transform>();
                // tfm.localPosition = new Vector3(0, 75, 0);

                this.contentPane.GetController("c1").selectedPage = "FS";
                this.contentPane.GetChild("FSFrame").asCom.GetChild("n7").asTextField.text =
                    ContentModel.Instance.freeSpinTotalTimes.ToString();
            });


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

            slotMachineCtrl.BeginBonusFreeSpin();


            yield return GameFreeSpin(null, errorCallback);


            OutputStackContextFreeSpin(
                (context) =>
                {
                    SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.Default);

                    slotMachineCtrl.SetReelsDeck((string)context["./strDeckRowCol"]);

                    _spinWEMD.Instance.SelectData(_spinWEMD.SPIN_WIN_EFFECT_FREE_SPIN_TRIGGER);
                    slotMachineCtrl.ShowSymbolWinDeck((SymbolWin)context["./winFreeSpinTriggerOrAddCopy"], true);

                    //goSlotCover.SetActive(true);
                    //goBgRegular.SetActive(true);
                    //goBgFreeSpin.SetActive(false);
                    //if (goPanelFreeSpin != null) goPanelFreeSpin.SetActive(false);
                    // Transform tfm = goAnchorMain.GetComponent<Transform>();
                    // tfm.localPosition = new Vector3(0, 0, 0);

                    this.contentPane.GetController("c1").selectedPage = "BS";
                });

            slotMachineCtrl.EndBonusFreeSpin();



            yield return WinPopup(GetBigWinType(), ContentModel.Instance.baseGameWinCredit);

            yield return slotMachineCtrl.SlotWaitForSeconds(1.5f);
        }

        public IEnumerator ShowSymbolBonusEffect(List<BonusWin> bonusWins)
        {
            yield return new WaitForSeconds(1f);
            for (int i = 0; i < Bonuslist.Count; i++)
            {
                Bonuslist[i]
                    .TweenMove(
                        slotMachineCtrl.SymbolCenterToNodeLocalPos(2, 2, anchorExpectation) + new Vector2(0, 212),
                        0.6f);
                yield return new WaitForSeconds(1f);
                Bonuslist[i].visible = false;
                // MainBlackboardController.Instance.AddMyTempCredit(bonusWins[i].earnCredit, true, isAddCreditAnim);
             //   slotMachineCtrl.SendTotalBonusCreditEvent(bonusWins[i].earnCredit);

            }
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
                ["./totalEarnCredit"] = ContentModel.Instance.totalEarnCredit,
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
            ContentModel.Instance.totalEarnCredit = (long)context["./totalEarnCredit"];
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

            yield return new WaitForSeconds(3f);

            ComQi.visible = false;
            ComEFDropGoldCoin.visible = false;
            ComGoldDi.visible = false;
        }

        IEnumerator PlayKingLion(float Speed)
        {

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
            yield return new WaitForSeconds(3f);
            ComGoldDi.visible = false;
        }

        IEnumerator PlayKing(float Speed)
        {
            animatorKing.speed = Speed;
            animatorKing.Rebind(); // 重置状态机和参数
            animatorKing.Play("win", -1, 0f); // 从第0帧开始播放
            animatorKing.Update(0f); // 立即刷新状态
            yield return new WaitForSeconds(2 / Speed);
            animatorKing.Rebind(); // 重置状态机和参数
            animatorKing.Play("idle", -1, 0f); // 从第0帧开始播放
            animatorKing.Update(0f); // 立即刷新状态
        }


        void OnGameReset()
        {
            if (corGameIdel != null) mono.StopCoroutine(corGameIdel);
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

            //yield return new WaitForSeconds(3f);

            yield return slotMachineCtrl.ShowWinListAwayDuringIdle009(winList);
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
                });
            yield return new WaitUntil(() => isNext == true);
            yield return null;
        }


        public IEnumerator ShowEffectReelsSlowMotion()
        {
            yield return new WaitUntil(() => isEffectSlowMotion2 == true);
            // 打开Expectation


            ComReelEffect2.xy = slotMachineCtrl.SymbolCenterToNodeLocalPos(2, 1, anchorExpectation);
            ComReelEffect2.visible = true;

            yield return new WaitUntil(() => isEffectSlowMotion3 == true);


            ComReelEffect2.TweenMove(slotMachineCtrl.SymbolCenterToNodeLocalPos(3, 1, anchorExpectation), 1);

            yield return new WaitUntil(() => isEffectSlowMotion4 == true);

            ComReelEffect2.TweenMove(slotMachineCtrl.SymbolCenterToNodeLocalPos(4, 1, anchorExpectation), 1);
            //ComReelEffect2.TweenMove(slotMachineCtrl.SymbolCenterToNodeLocalPos(4, 1, anchorExpectation), 1);
            yield return new WaitUntil(() => isStoppedSlotMachine == true);
            // 关闭Expectation
            ComReelEffect2.visible = false;

            if (ContentModel.Instance.isFreeSpinTrigger)
            {
            }
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


        /// <summary>
        /// 免费游戏，变图标、5连线特效
        /// </summary>
        /// <param name="winList"></param>
        /// <returns></returns>
        IEnumerator ShowSingleWinListOnceAtFreeSpin009(List<SymbolWin> winList)
        {
            int idx = 0;
            while (idx < winList.Count)
            {
                SymbolWin curSymbolWin = winList[idx];
                bool isUserMyselfSymbolIndex = true;

                //停止特效显示
                slotMachineCtrl.SkipWinLine(false);


                //是否改变图标
                if (curSymbolWin.customData == "change" || slotMachineCtrl.CheckHasSymbolChange(curSymbolWin))
                {
                    _spinWEMD.Instance.SelectData(_spinWEMD.SPIN_WIN_EFFECT_CHANGE_SYMBOL);

                    //GameSoundHelper.Instance.PlaySound(SoundKey.FreeSpinChangeSymbol);
                    yield return slotMachineCtrl.ShowSymbolChangeBySetting(curSymbolWin, "Symbol Change");
                    isUserMyselfSymbolIndex = false;

                    _spinWEMD.Instance.SelectData(_spinWEMD.SPIN_WIN_EFFECT_FREE_SPIN);
                }

                //是否是五连线
                if (slotMachineCtrl.Check5kind(curSymbolWin))
                {
                    yield return Show5KindPoup(curSymbolWin);
                }

                yield return slotMachineCtrl.ShowSymbolWinBySetting(curSymbolWin, isUserMyselfSymbolIndex,
                    SpinWinEvent.SingleWinLine);

                ++idx;
            }

            slotMachineCtrl.CloseSlotCover();

            //停止特效显示
            slotMachineCtrl.SkipWinLine(false);
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
            if (ContentModel.Instance.bonusResult.ContainsKey(211))
            {
                bonusWins = new List<BonusWin>();

                JSONNode nodBonus = ContentModel.Instance.bonusResult[211];

                foreach (JSONNode item in nodBonus["data"])
                {

                    BonusWin bw = new BonusWin
                    {
                        earnCredit = item["credit"],
                        cell = new Cell((item["pos_xy"] / 10) - 1, (item["pos_xy"] % 10) - 1),
                    };
                    bonusWins.Add(bw);
                    Bonuslist.Add(UIPackage.CreateObject("Common", "AnchorRootDefault").asCom);

                    anchorExpectation.AddChild(Bonuslist[Bonuslist.Count - 1]);
                    Bonuslist[Bonuslist.Count - 1].xy =
                        slotMachineCtrl.SymbolCenterToNodeLocalPos((item["pos_xy"] / 10) - 1, (item["pos_xy"] % 10) - 1,
                            anchorExpectation);
                    Bonuslist[Bonuslist.Count - 1].visible = true;
                    GameCommon.FguiUtils.AddWrapper(Bonuslist[Bonuslist.Count - 1], GameObject.Instantiate(goGoldBack));
                }

                yield return null;
           //     yield return slotMachineCtrl.ShowSymbolBonusBySetting(bonusWins, true);
            }
            //return bounsSroce;
        }





        WinLevelType GetBigWinType()
        {
            long baseGameWinCredit = ContentModel.Instance.baseGameWinCredit;
            List<WinMultiple> winMultipleList = ContentModel.Instance.winLevelMultiple;
            long totalBet = ContentModel.Instance.totalBet;
            WinLevelType winLevelType = WinLevelType.None;
            for (int i = 0; i < winMultipleList.Count; i++)
            {
                if (baseGameWinCredit > totalBet * winMultipleList[i].multiple)
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
                case 1:
                    uiJPGrangCtrl.SetData(jpWin.curCredit);
                    break;
                case 2:
                    uiJPMajorCtrl.SetData(jpWin.curCredit);
                    break;
                case 3:
                    uiJPMinorCtrl.SetData(jpWin.curCredit);
                    break;
                case 4:
                    uiJPMiniCtrl.SetData(jpWin.curCredit);
                    break;
            }
        }


        IEnumerator RequestSlotSpinFromMock02(Action successCallback = null, Action<string> errorCallback = null)
        {
            bool isNext = false;
            bool isBreak = false;

            long totalBet = TotalBet;

            JSONNode resNode = null;

            MachineDataG300Controller.Instance.RequestSlotSpinFromMock(TotalBet, (res) =>
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

            if (isBreak) yield break;


            SBoxJackpotData sboxJackpotData = null;

            // 获取彩金贡献值
            int cacheTotalJpMajor = SQLitePlayerPrefs03.Instance.GetInt(CACHE_TOTAL_JP_MAJOR_CONTRIBUTION, 0);
            int cacheTotalJpGrand = SQLitePlayerPrefs03.Instance.GetInt(CACHE_TOTAL_JP_GRAND_CONTRIBUTION, 0);

            ERPushMachineDataManager02.Instance.RequestGetJpMajorGrandContribution((res) =>
            {
                JSONNode data = JSONNode.Parse((string)res);

                if (0 != (int)data["code"])
                {
                    errorCallback?.Invoke("请求贡献值报错");
                    isNext = true;
                    isBreak = true;
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

                // 没有联网彩金
                if (!ClientWS.Instance.IsConnected && !ApplicationSettings.Instance.isMock)
                {
                    isNext = true;
                    return;
                }

                NetClientHelper02.Instance.RequestJackBetMajorGrand(info, (res) =>
                {
                    // 【联网彩金，请求成功 ，删除数据】
                    cacheTotalJpMajor -= majorBet;
                    cacheTotalJpGrand -= grandBet;
                    SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_MAJOR_CONTRIBUTION, cacheTotalJpMajor);
                    SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_GRAND_CONTRIBUTION, cacheTotalJpGrand);

                    sboxJackpotData = res as SBoxJackpotData;

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
                    errorCallback?.Invoke(err.msg);
                    isNext = true;
                    isBreak = true;
                });

            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            if (isBreak) yield break;


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
            MachineDataG300Controller.Instance.ParseSlotSpin(totalBet, resNode, sboxJackpotData);

            // 数据入库

            // 游戏彩金滚轮
            SetUIJackpotGameReel();

            if (successCallback != null)
                successCallback.Invoke();
        }




        const string CACHE_TOTAL_JP_MAJOR_CONTRIBUTION = "CACHE_TOTAL_JP_MAJOR_CONTRIBUTION";
        const string CACHE_TOTAL_JP_GRAND_CONTRIBUTION = "CACHE_TOTAL_JP_GRAND_CONTRIBUTION";
        IEnumerator RequestSlotSpinFromMachine(Action successCallback = null, Action<string> errorCallback = null)
        {
            long totalBet = TotalBet;
            bool isBreak = false;
            bool isNext = false;
            bool isGetMyCredit = false;

            JSONNode resNode = null;
            int myCredit = -1;

            ERPushMachineDataManager02.Instance.RequestCoinPushSpin((res) =>
            {
                resNode = JSONNode.Parse((string)res);
                isNext = true;
            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            // 获取玩家金额
            while (!isGetMyCredit)
            {
                GetMyCredit((credit) =>
                {
                    myCredit = credit;
                    isGetMyCredit = true;
                    isNext = true;
                }, (errMsg) =>
                {
                    isNext = true;
                });

                yield return new WaitUntil(() => isNext == true);
                isNext = false;
            }

            SBoxJackpotData sboxJackpotData = null;

            // 获取彩金贡献值
            int cacheTotalJpMajor = SQLitePlayerPrefs03.Instance.GetInt(CACHE_TOTAL_JP_MAJOR_CONTRIBUTION, 0);
            int cacheTotalJpGrand = SQLitePlayerPrefs03.Instance.GetInt(CACHE_TOTAL_JP_GRAND_CONTRIBUTION, 0);



            ERPushMachineDataManager02.Instance.RequestGetJpMajorGrandContribution((res) =>
            {
                JSONNode data = JSONNode.Parse((string)res);

                if (0 != (int)data["code"])
                {
                    errorCallback?.Invoke("请求贡献值报错");
                    isNext = true;
                    isBreak = true;
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

                // 没有联网彩金
                if (!ClientWS.Instance.IsConnected && !ApplicationSettings.Instance.isMock)
                {
                    isNext = true;
                    return;
                }

                NetClientHelper02.Instance.RequestJackBetMajorGrand(info, (res) =>
                {
                    // 【联网彩金，请求成功 ，删除数据】
                    cacheTotalJpMajor -= majorBet;
                    cacheTotalJpGrand -= grandBet;
                    SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_MAJOR_CONTRIBUTION, cacheTotalJpMajor);
                    SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_GRAND_CONTRIBUTION, cacheTotalJpGrand);

                    sboxJackpotData = res as SBoxJackpotData;

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
                    errorCallback?.Invoke(err.msg);
                    isNext = true;
                    isBreak = true;
                });

            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            if (isBreak) yield break;

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

            int code = (int)resNode["code"]; //:0表示成功，-1表示传参失败

            if (code != 0)
            {
                errorCallback?.Invoke($"Spin数据有误");
                DebugUtils.LogError($"Spin数据有误： {resNode.ToString()}");
                yield break;
            }

            resNode["creditAfter"] = myCredit;

            // 解析数据
            MachineDataG200Controller.Instance.ParseSlotSpin(totalBet, resNode, sboxJackpotData);


            // 数据入库

            // ui 彩金
            SetUIJackpotGameReel();


            if (successCallback != null)
                successCallback.Invoke();
        }



        public void SetUIJackpotGameReel()
        {
            JackpotRes info = ContentModel.Instance.jpGameRes;

            ContentModel.Instance.uiGrandJP.nowCredit = uiJPGrangCtrl.nowData;
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
            uiJPGrangCtrl.SetData(ContentModel.Instance.jpGameWhenCreditLst[0]);
            //uiJPMegaCtrl.SetData(ContentModel.Instance.jpGameWhenCreditLst[1]);
            uiJPMajorCtrl.SetData(ContentModel.Instance.jpGameWhenCreditLst[1]);
            uiJPMinorCtrl.SetData(ContentModel.Instance.jpGameWhenCreditLst[2]);
            uiJPMiniCtrl.SetData(ContentModel.Instance.jpGameWhenCreditLst[3]);

        }



        #region spine 按钮

        void OnClickSpinButton(EventData res)
        {
            if(res.name== "SpinButtonClick")
            {
                bool isLongClick = (bool)res.value;
                switch (ContentModel.Instance.btnSpinState)
                {
                    case SpinButtonState.Stop:
                        {
                            if (ContentModel.Instance.isSpin) return; // 已经开始玩直接退出

                            ContentModel.Instance.isSpin = true;

                            Action successCallback = () =>
                            {
                                ContentModel.Instance.isSpin = false;
                                ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
                                ContentModel.Instance.gameState = GameState.Idle;
                                //DebugUtils.Log("游戏结束");
                            };

                            if (isLongClick)
                            {
                                ContentModel.Instance.isAuto = true;
                                ContentModel.Instance.btnSpinState = SpinButtonState.Auto;
                                StartGameAuto(successCallback, StopGameWhenError); //自动玩
                            }
                            else
                            {
                                ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                                StartGameOnce(successCallback, StopGameWhenError); //开始玩
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
                {}
                    DeviceIOTPayment.Instance.DoQrCoinIn();
                }
                return;
                */
            }
            else
            {
                string massage = I18nMgr.T(msg);
                TipPopupHandler.Instance.OpenPopupOnce(I18nMgr.T(msg));
            }
        }


        void StartGameOnce(Action successCallback = null, Action<string> errorCallback = null)
        {
            if (corGameOnce != null) mono.StopCoroutine(corGameOnce);
            corGameOnce = mono.StartCoroutine(GameOnce(successCallback, errorCallback));
        }

        void StartGameAuto(Action successCallback = null, Action<string> errorCallback = null)
        {
            if (corGameAuto != null) mono.StopCoroutine(corGameAuto);
            corGameAuto = mono.StartCoroutine(GameAuto(successCallback, errorCallback));
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




    }
}