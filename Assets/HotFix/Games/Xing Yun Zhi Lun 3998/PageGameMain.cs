using FairyGUI;
using GameMaker;
using Newtonsoft.Json;
using PusherEmperorsRein;
using SBoxApi;
using SimpleJSON;
using SlotMaker;
using SlotZhuZaiJinBi1700;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using _spinWEMD = SlotMaker.SpinWinEffectSettingModel;
using Random = UnityEngine.Random;

namespace XingYunZhiLun_3998
{
    public class GameConfigRoot
    {
        [JsonProperty("game_id")] public int GameId;//游戏 ID

        [JsonProperty("game_name")] public string GameName;//名称

        [JsonProperty("display_name")] public string DisplayName;//显示名称

        [JsonProperty("win_level_multiple")] public Dictionary<string, long> WinLevelMultiple { get; set; }//赢钱倍数

        [JsonProperty("symbol_paytable")] public Dictionary<string, PayTableSymbolInfo> SymbolPaytable { get; set; }//符号赔率表

        [JsonProperty("pay_lines")] public List<List<int>> pay_lines { get; set; } //支付钱
    }

    public class PageGameMain : MachinePageBase
    {
        public new const string pkgName = "XingYunZhiLun_3998";
        public new const string resName = "PageGameMain";

        private SlotMachineController3998 slotMachineCtrl;
        private GComponent slotCover, gOwnerPanel, gPlayLines, gFrame, gWheel, gZhuanPan, gJackpotBg, gFireWork;
        private GList gList;
        private GImage gMask;
        private GTextField freeTimes;
        private Transition bsTofs, fsTobs;
        private Transform fireworkEffect, JackpotListEffect;

        private GameObject goGameCtrl, goFirework;
        private GameObject goFireworkEffect;

        private Transition startTransition, initTransition;

        private GameObject goHost, goGuest, goLight, goSpineHost, goSpineGuest, goSpineLight, wildObject;
        private GameObject[] goWild = new GameObject[5];
        private List<GComponent> gWild = new List<GComponent>(); 
        private GComponent loadAnchorHost, loadAnchorGuest, loadAnchorLight;

        PayTableController payTableController = new PayTableController();
        Coroutine corReelsTurn, corGameIdel, corGameOnce, corEffectSlowMotion;

        //游戏控制
        private MonoHelper mono;
        private FguiPoolHelper fguiPoolHelper;
        private FguiGObjectPoolHelper gObjectPoolHelper;

        //转盘转速控制
        private float rotateSpeed = 15;
        private bool isMain = true, isJackpot = false, JackpotFinish = false;
        private string JackpotType = "";
        private float winCredit = 0;

        //彩金
        //MiniReelGroup uiJPGrandCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMajorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMinorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMiniCtrl = new MiniReelGroup();


        private bool isReserve;

        long TotalBet => (long)SBoxModel.Instance.CoinInScale;

        private bool isInit = false;        //是否初始化
        private bool isInitPool = false; //资源池是否初始化
        private bool tipCoinIn = false; //提示硬币输入
        private bool isStoppedSlotMachine = false;
        private GComponent anchorExpectation, ComReelEffect2;
        private GameObject goReelEffcet;

        bool isAddCreditAnim => !(slotMachineCtrl.isStopImmediately == true || SBoxModel.Instance.isCoinOutImmediately);
        private EventData _data = null;

        const string CACHE_TOTAL_JP_MAJOR_CONTRIBUTION = "CACHE_TOTAL_JP_MAJOR_CONTRIBUTION";
        const string CACHE_TOTAL_JP_GRAND_CONTRIBUTION = "CACHE_TOTAL_JP_GRAND_CONTRIBUTION";

        protected override void OnInit()
        {
            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            int count = 8;

            Action callback = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam(null);
                }
            };

            if (UIPackage.GetByName("Common") == null)
            {
                ResourceManager02.Instance.LoadAssetBundleAsync("Assets/GameRes/Games/Common/FGUIs", (bundle) =>
                {
                    count++;
                    callback();
                });
            }

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Game Controller/Slot Game Main ControllerClone.prefab",
            (GameObject clone) =>
            {
                if (goGameCtrl != null) //防止重复加载
                {
                    return;
                }
                goGameCtrl = GameObject.Instantiate(clone);
                goGameCtrl.name = "Slot Game Main Controller3998";
                goGameCtrl.transform.SetParent(null);
                //获取组件引用
                slotMachineCtrl = goGameCtrl.transform.Find("Slot Machine").GetComponent<SlotMachineController3998>();
                mono = goGameCtrl.transform.GetComponent<MonoHelper>();

                fguiPoolHelper = goGameCtrl.transform.Find("Pool").GetComponent<FguiPoolHelper>();
                gObjectPoolHelper = goGameCtrl.transform.Find("GObject Pool").GetComponent<FguiGObjectPoolHelper>();
                callback();
            });

            ResourceManager02.Instance.LoadAssetBundleAsync("Assets/GameRes/Games/Emperors Rein 200/FGUIs", (ab) =>
            {
                UIPackage.AddPackage(ab);
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/PopupGameJackpot/PushJackpotGuest.prefab",
            (GameObject clone) =>
            {
                goSpineGuest = clone;
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/PopupGameJackpot/PushJackpotHost.prefab",
            (GameObject clone) =>
            {
                goSpineHost = clone;
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/PopupGameJackpot/PushJackpotLight.prefab",
            (GameObject clone) =>
            {
                goSpineLight = clone;
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/SymbolHit/SymbolHit11.prefab",
            (GameObject clone) =>
            {
                wildObject = clone;
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Effects/Firework.prefab",
            (GameObject clone) =>
            {
                goFirework = clone;
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Effects/ReelEffect.prefab",
                (GameObject clone) =>
                {
                    goReelEffcet = clone;
                    callback();
                });

            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        if (PanelController02.isOpenIntroduce == true)
                        {
                            return;
                        }

                        Debug.LogError("游戏接受到机台短按的数据：Spin");
                        EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, false); // isLongClick
                        OnClickSpinButton(res);
                    },
                },

            };
        }


        protected override void OnLanguageChange(I18nLang lang)
        {
            FguiI18nTextAssistant.Instance.DisposeAllTranslate(this.contentPane);
            this.contentPane.Dispose(); // 释放当前UI
            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            isInit = true;
            InitParam(null);
            //FguiI18nTextAssistant.Instance.TranslateComponent(this.contentPane);
        }

        public override void OnOpen(PageName name, EventData data)
        {
            if (isOpen) return;
            base.OnOpen(name, data);
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnClickSpinButton);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnStopSlot);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT, OnSlotDetailEvent);
            mono.updateHandle.AddListener(WheelTrun);

            if (isReserve)
            {
                GameSoundHelper.Instance.PlayMusicSingle(SoundKey.RegularBG);
            }
            else
            {
                isReserve = true;
            }
        }

        public override void OnClose(EventData data = null)
        {
            slotMachineCtrl.SkipWinLine(true);
            OnGameReset();

            GameSoundHelper.Instance.StopMusic();
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnClickSpinButton);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnStopSlot);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT,OnSlotDetailEvent);
            mono.updateHandle.RemoveAllListeners();

            base.OnClose(data);
        }

        public void InitParam(EventData data)
        {
            if (data != null) _data = data;
            if (!isInit) return;
            isInit = false;

            PageManager.Instance.PreloadPage(PageName.XingYunZhiLunPopupZhuanPan, null);

            //对象池初始化
            if (fguiPoolHelper != null && isInitPool == false)
            {
                isInitPool = true;
                //中奖动画
                fguiPoolHelper.Add(TagPoolObject.SymbolHit, CustomModel.Instance.symbolHitEffect.Values.ToList(), "symbol_hit#", 5);
                fguiPoolHelper.PreLoad(TagPoolObject.SymbolHit);
                //边框
                fguiPoolHelper.Add(TagPoolObject.SymbolBorder, CustomModel.Instance.borderEffect, "border#", 5);
                fguiPoolHelper.PreLoad(TagPoolObject.SymbolBorder);
                //落下后图标静止动画
                fguiPoolHelper.Add(TagPoolObject.SymbolAppear, CustomModel.Instance.symbolAppearEffect.Values.ToList(), "symbol_appear#", 5);
                fguiPoolHelper.PreLoad(TagPoolObject.SymbolAppear);

                //fguiPoolHelper.Init(CustomModel.Instance.symbolHitEffect,CustomModel.Instance.symbolAppearEffect, null,CustomModel.Instance.borderEffect);
            }

            //初始化UI组件
            GComponent gSlotMachine = contentPane.GetChild("slotMachine").asCom;
            GComponent gReels = gSlotMachine.GetChild("reels").asCom;
            slotCover = gSlotMachine.asCom.GetChild("slotCover").asCom;
            gPlayLines = gSlotMachine.asCom.GetChild("playLines").asCom;
            gFrame = contentPane.GetChild("anchorFrame").asCom;
            slotMachineCtrl.Init(slotCover, gPlayLines, gReels, gFrame, fguiPoolHelper, gObjectPoolHelper);


            gZhuanPan = contentPane.GetChild("ZhuanPan").asCom;
            gWheel = gZhuanPan.GetChild("Wheel").asCom;
            gWheel.rotation = 0;
            WheelInit(new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 });

            ////列表滚动游戏测试列表
            gList = contentPane.GetChild("JackpotList").asCom.GetChild("Symbols").asList;
            gList.alpha = 1;
            gJackpotBg = contentPane.GetChild("HostAndGuest").asCom;

            //进入彩金界面初始化
            JackpotBgOpen();

            freeTimes = contentPane.GetChild("FSFrame").asCom.GetChild("freeTimes").asTextField;


            if (ComReelEffect2 != null)
            {
                ComReelEffect2.Dispose();
            }

            ComReelEffect2 = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            GameCommon.FguiUtils.DeleteWrapper(ComReelEffect2);
            GameCommon.FguiUtils.AddWrapper(ComReelEffect2, GameObject.Instantiate(goReelEffcet));
            ComReelEffect2.visible = false;
            anchorExpectation = this.contentPane.GetChild("anchorReelEffect").asCom;
            anchorExpectation.AddChild(ComReelEffect2);
            anchorExpectation.visible = true;

            //说明书
            MainModel.Instance.contentMD = ContentModel.Instance;

            List<GComponent> lstPayTable = new List<GComponent>();

            foreach (string url in CustomModel.Instance.payTable)
            {
                GComponent paytable = UIPackage.CreateObjectFromURL(url).asCom;
                paytable.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().InitParam(paytable);

                lstPayTable.Add(paytable);
                paytable.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount++;
            }

            ContentModel.Instance.goPayTableLst = lstPayTable.ToArray();
            payTableController.Init(lstPayTable);
            ContentModel.Instance.payLines = new List<List<int>>();

            //读取json配置
            ReadJsonBet();

            //// 玩家积分初始化
            //SBoxModel.Instance.myCredit = 9900;
            //foreach (SBoxPlayerScoreInfo item in SBoxIdea.sBoxInfo.PlayerScoreInfoList)
            //{
            //    if (item.PlayerId == 1)
            //    {
            //        SBoxModel.Instance.myCredit = item.Score;
            //    }
            //}


            GComponent loadFirwork = contentPane.GetChild("anchorEffect").asCom;
            if(gFireWork != loadFirwork)
            {
                GameCommon.FguiUtils.DeleteWrapper(gFireWork);
                goFireworkEffect = GameObject.Instantiate(goFirework);
                gFireWork = loadFirwork;
                GameCommon.FguiUtils.AddWrapper(gFireWork, goFireworkEffect);
            }
            fireworkEffect = goFireworkEffect.transform.GetChild(0).GetChild(0).GetChild(0);

            bsTofs = contentPane.GetTransition("BSToFS");
            fsTobs = contentPane.GetTransition("FSToBS");

            //初始化菜单ui
            gOwnerPanel = this.contentPane.GetChild("panel").asCom;
            ContentModel.Instance.goAnthorPanel = gOwnerPanel;
            MainModel.Instance.contentMD.goAnthorPanel = gOwnerPanel;
            // 事件放出
            //goGameCtrl.transform.Find("Panel").GetComponent<PanelController01>().Init();
            EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
                new EventData<GComponent>(PanelEvent.AnchorPanelChange, gOwnerPanel));


            //同步积分和押注
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
                        //DebugUtils.Log("前一局算法卡CoinIn==" + playerAccountList[i].CoinIn);
                        // DebugUtils.Log("前一局算法卡Bet==" + playerAccountList[i].Bets);
                        // DebugUtils.Log("前一局算法卡Credit==" + );
                        break;
                    }
                }

            }, (BagelCodeError err) =>
            {
                DebugUtils.Log(err.msg);
            });
            MainBlackboardController.Instance.SyncMyTempCreditToReal(true);


            ContentModel.Instance.totalBet = SBoxModel.Instance.betList[ContentModel.Instance.betIndex];

            //彩金
            //uiJPGrangCtrl.Init("Grand", this.contentPane.GetChild("jpGrand").asCom.GetChild("reels").asList, "N0");
            uiJPMajorCtrl.Init("Major", this.contentPane.GetChild("jpMajor").asCom.GetChild("reels").asList, "N0");
            uiJPMinorCtrl.Init("Minor", this.contentPane.GetChild("jpMinor").asCom.GetChild("reels").asList, "N0");
            uiJPMiniCtrl.Init("Mini", this.contentPane.GetChild("jpMini").asCom.GetChild("reels").asList, "N0");


            if (ApplicationSettings.Instance.isMock)
            {
                //uiJPGrangCtrl.SetData(50000);
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

            ContentModel.Instance.totalBet = SBoxModel.Instance.betList[ContentModel.Instance.betIndex];
            GameSoundHelper.Instance.PlayMusicSingle(SoundKey.RegularBG);
        }


        private void OnClickSpinButton(EventData res)
        {
            if (!isMain)
            {
                return;
            }

            bool isLongClick = (bool)res.value;
            switch (ContentModel.Instance.btnSpinState)
            {
                case SpinButtonState.Stop:
                    if (ContentModel.Instance.isSpin) return; //已经开始玩直接退出？
                    ContentModel.Instance.isSpin = true;

                    Action successCallback = () =>
                    {
                        ContentModel.Instance.isSpin = false;
                        ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
                        ContentModel.Instance.curBtnSpinState = SpinButtonState.Stop;
                        ContentModel.Instance.gameState = GameState.Idle;
                        //DebugUtils.Log("游戏结束");
                    };

                    if (isJackpot)
                    {
                        ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                        ContentModel.Instance.curBtnSpinState = SpinButtonState.Spin;
                        StartRandomRoll(GetJackpotId()); //开始玩
                        return;
                    }

                    if (isLongClick)
                    {
                        ContentModel.Instance.isAuto = true;
                        ContentModel.Instance.btnSpinState = SpinButtonState.Auto;
                        ContentModel.Instance.curBtnSpinState = SpinButtonState.Auto;
                        StartGameAuto(successCallback, StopGameWhenError); //自动玩
                    }
                    else
                    {
                        ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                        ContentModel.Instance.curBtnSpinState = SpinButtonState.Spin;
                        StartGameOnce(successCallback, StopGameWhenError); //开始玩
                    }
                    break;
                case SpinButtonState.Spin:
                    {
                        // 已经在游戏时，去停止游戏
                        if (!ContentModel.Instance.isSpin) return; // 已经停止直接退出

                        slotMachineCtrl.isStopImmediately = true; // 去停止游戏  

                        EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_SLOT_EVENT,
                        new EventData(SlotMachineEvent.StoppedSlotMachine));

                        SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.StopImmediately);
                    }
                    break;
                case SpinButtonState.Auto:
                    {
                        if (isJackpot)
                        {
                            ContentModel.Instance.isSpin = true;
                            ContentModel.Instance.isAuto = false;
                            ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                            ContentModel.Instance.curBtnSpinState = SpinButtonState.Spin;
                            ContentModel.Instance.gameState = GameState.Spin;
                        }
                        else
                        {
                            //停止自动玩
                            ContentModel.Instance.isSpin = true;
                            ContentModel.Instance.isAuto = false;
                            ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                            ContentModel.Instance.curBtnSpinState = SpinButtonState.Spin;
                        }   
                    }
                    break;
            }
        }

        void StartGameAuto(Action successCallback = null, Action<string> errorCallback = null)
        {
            mono.StartCoroutine(GameAuto(successCallback, errorCallback));
        }

        void StartGameOnce(Action successCallback = null, Action<string> errorCallback = null)
        {
            ContentModel.Instance.totalPlaySpins = 1;
            ContentModel.Instance.remainPlaySpins = 1;
            corGameOnce = mono.StartCoroutine(GameOnce(successCallback, errorCallback));
        }


        List<int> winNumber = new List<int>();
        IEnumerator GameOnce(Action successCallback, Action<string> errorCallback)
        {
            if (!SBoxModel.Instance.isMachineActive)
            {
                errorCallback?.Invoke("<size=24>Machine not activated!</size>");
                yield break;
            }

            if (SBoxModel.Instance.myCredit < ContentModel.Instance.totalBet)
            {
                //tipCoinIn = true;
                errorCallback?.Invoke("<size=15>Balance is insufficient, please recharge first</size>");
                yield break;
            }

            //积分校验
            MachineDataManager02.Instance.RequestGetPlayerInfo((res) =>
            {

                SBoxAccount data = (SBoxAccount)res;
                int pid = SBoxModel.Instance.pid;
                List<SBoxPlayerAccount> playerAccountList = data.PlayerAccountList;
                for (int i = 0; i < playerAccountList.Count; i++)
                {
                    if (playerAccountList[i].PlayerId == pid)
                    {
                        DebugUtils.Log("前一局算法卡CoinIn==" + playerAccountList[i].CoinIn);
                        DebugUtils.Log("前一局算法卡Bet==" + playerAccountList[i].Bets);
                        DebugUtils.Log("前一局算法卡Credit==" + playerAccountList[i].Credit);
                        break;
                    }
                }

            }, (BagelCodeError err) =>
            {

                DebugUtils.Log(err.msg);
            });

            // 游戏状态重置和旋转请求
            OnGameReset();
            ContentModel.Instance.gameState = GameState.Spin;
            slotMachineCtrl.BeginTurn();
            bool isNext = false;
            bool isBreak = false;
            string errMsg = "";

            //模拟结果
            if (ApplicationSettings.Instance.isMock)
            {
                yield return RequestSlotSpinFromMock(() =>
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

            //请求结果失败
            if (isBreak)
            {
                if (errorCallback != null)
                    errorCallback.Invoke(errMsg);
                yield break;
            }

            //检查是否启用在线彩金
            //根据运行环境（模拟或实际）请求彩金数据
            if (SBoxModel.Instance.isJackpotOnLine)
            {
                if (ApplicationSettings.Instance.isMock)
                {
                    // 模拟在线彩金中奖数据
                    MachineDataManager.Instance.RequestJackpotOnLine();
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

            //开始滚动
            slotMachineCtrl.BeginSpin();

            //是否加速滚动
            if (ContentModel.Instance.isReelsSlowMotion)
            {
                slotMachineCtrl.ShowSymbolAppearEffectAfterReelStop(true);
            }
            else
            {
                slotMachineCtrl.ShowSymbolAppearEffectAfterReelStop(ContentModel.Instance.winList.Count == 0);
            }


            slotMachineCtrl.SkipWinLine(true);
            // 立即停止
            if (slotMachineCtrl.isStopImmediately)
            {
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
                // 正常旋转模式
                Debug.Log(ContentModel.Instance.strDeckRowCol);
                if (corReelsTurn != null) mono.StopCoroutine(corReelsTurn);
                corReelsTurn = mono.StartCoroutine(slotMachineCtrl.TurnReelsNormal(ContentModel.Instance.strDeckRowCol,
                    () =>
                    {
                        isNext = true;
                    }));

                isNext = false;
                yield return new WaitUntil(() => isNext == true || slotMachineCtrl.isStopImmediately == true);

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
            //线赢的数据
            List<SymbolWin> winList = ContentModel.Instance.winList;
            long allWinCredit = 0;


            #region Win
            if (ContentModel.Instance.isWild)
            {
                //显示中奖动画
                PageManager.Instance.PreloadPage(PageName.XingYunZhiLunPopupZhuanPan, null);
                slotMachineCtrl.SkipWinLine(true);
                slotMachineCtrl.ShowSymbolEffect(TagPoolObject.SymbolHit, new List<int>() { 9 }, true, 9, true);
                yield return slotMachineCtrl.SlotWaitForSeconds(1.5f);
                slotMachineCtrl.SkipWinLine(true);
                slotMachineCtrl.CloseSlotCover();
                //slotMachineCtrl.ShowSymbolEffect(TagPoolObject.SymbolAppear, new List<int>() { 8 }, true, 8, true);

                //过度动画
                isNext = false;
                isMain = false;
                PageManager.Instance.OpenPageAsync(PageName.XingYunZhiLunPopupZhuanPan,
                    new EventData<Dictionary<string, object>>("", new Dictionary<string, object>
                    {
                        ["jackpotType"] = "Wild",
                    }),
                    (res) =>
                    {
                        //恢复主页面按钮
                        ContentModel.Instance.goAnthorPanel = gOwnerPanel;
                        MainModel.Instance.contentMD.goAnthorPanel = gOwnerPanel;
                        EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
                            new EventData<GComponent>(PanelEvent.AnchorPanelChange, gOwnerPanel));

                        ContentModel.Instance.btnSpinState = SpinButtonState.Stop;

                        if (ContentModel.Instance.isAuto)
                        {
                            ContentModel.Instance.btnSpinState = SpinButtonState.Auto;
                        }
                        else
                        {
                            ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                        }

                        isMain = true;
                        isNext = true;
                    });

                yield return new WaitUntil(() => isNext == true);
                isNext = false;

                int[] cols = ContentModel.Instance.cols.ToArray();

                gWild.Clear();
                for(int i = 0; i < cols.Length; i++)
                {
                    GComponent wildEffect = contentPane.GetChild("slotMachine").asCom.GetChild("reels").asCom.GetChild("reel" + (cols[i] + 1)).asCom.GetChild("anchorWild").asCom;
                    gWild.Add(wildEffect);
                    goWild[cols[i]] = GameObject.Instantiate(wildObject);
                    GameCommon.FguiUtils.AddWrapper(wildEffect, goWild[cols[i]]);

                    // 设置层级
                    FguiSortingOrderManager.Instance.ChangeSortingOrder(wildEffect, gFrame);
                }

                yield return new WaitForSeconds(1.5f);

                for (int i = 0; i < cols.Length; i++)
                {
                    Animator animator = goWild[cols[i]].transform.GetChild(1).GetChild(0).GetComponent<Animator>();
                    Transform effect = goWild[cols[i]].transform.GetChild(0).GetChild(0).GetChild(0);
                    if (cols[i] < ContentModel.Instance.maxLink)
                    {
                        PlayAnim(animator, "win");
                        PlayEffectAnim(effect);
                    }
                    else
                    {
                        PlayAnim(animator, "idle");
                    }
                }

                //计算出礼盒的获得金额
                long totalWinLineCredit = 0;
                totalWinLineCredit = slotMachineCtrl.GetTotalWinCredit(winList);
                allWinCredit += totalWinLineCredit;

                //积分同步和退币处理
                slotMachineCtrl.SendTotalWinCreditEvent(allWinCredit);

                for(int i = 0; i < winList.Count; i++)
                {
                    winNumber.Add(winList[i].symbolNumber);
                }
                slotMachineCtrl.IsWildShowSymbolEffect(TagPoolObject.SymbolHit, winNumber, true, 0, true);

                //加钱动画
                //MainBlackboardController.Instance.AddMyTempCredit(totalWinLineCredit, true, isAddCreditAnim);
            }
            //礼盒游戏
            else if (ContentModel.Instance.isLihe)
            {
                int index = ContentModel.Instance.rewardIndex;

                //显示中奖动画
                PageManager.Instance.PreloadPage(PageName.XingYunZhiLunPopupZhuanPan, null);
                slotMachineCtrl.SkipWinLine(true);
                slotMachineCtrl.ShowSymbolEffect(TagPoolObject.SymbolHit, new List<int>() { 9 }, true, 9, true);
                yield return slotMachineCtrl.SlotWaitForSeconds(1.5f);
                slotMachineCtrl.SkipWinLine(true);
                slotMachineCtrl.CloseSlotCover();
                //slotMachineCtrl.ShowSymbolEffect(TagPoolObject.SymbolAppear, new List<int>() { 8 }, true, 8, true);

                //过度动画
                isNext = false;
                isMain = false;
                PageManager.Instance.OpenPageAsync(PageName.XingYunZhiLunPopupZhuanPan,
                    new EventData<Dictionary<string, object>>("", new Dictionary<string, object>
                    {
                        ["jackpotType"] = "Lihe",
                    }),
                    (res) =>
                    {
                        //恢复主页面按钮
                        ContentModel.Instance.goAnthorPanel = gOwnerPanel;
                        MainModel.Instance.contentMD.goAnthorPanel = gOwnerPanel;
                        EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
                            new EventData<GComponent>(PanelEvent.AnchorPanelChange, gOwnerPanel));

                        ContentModel.Instance.btnSpinState = SpinButtonState.Stop;

                        if (ContentModel.Instance.isAuto)
                        {
                            ContentModel.Instance.btnSpinState = SpinButtonState.Auto;
                        }
                        else
                        {
                            ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                        }

                        slotMachineCtrl.isStopImmediately = false;
                        isMain = true;
                        isNext = true;
                    });

                yield return new WaitUntil(() => isNext == true);
                isNext = false;

                //更换礼盒图标的方法
                //slotMachineCtrl.ChangeSymbolIcon(ContentModel.Instance.winningLines, 10);
                slotMachineCtrl.ChangeSymbolIcon(10);

                slotMachineCtrl.SkipWinLine(true);
                slotMachineCtrl.ShowSymbolEffect(TagPoolObject.SymbolHit, new List<int>() { 10 }, true, 10, true);
                yield return new WaitForSeconds(2f);

                slotMachineCtrl.SkipWinLine(true);
                slotMachineCtrl.ShowSymbolEffect(TagPoolObject.SymbolAppear, new List<int>() { 10 }, true, 10, true);
                yield return new WaitForSeconds(0.6f);

                slotMachineCtrl.SkipWinLine(true);
                slotMachineCtrl.ChangeSymbolIcon(10, index);

                //计算出礼盒的获得金额
                long totalWinLineCredit = 0;
                totalWinLineCredit = slotMachineCtrl.GetTotalWinCredit(winList);
                allWinCredit += totalWinLineCredit;

                //积分同步和退币处理
                slotMachineCtrl.SendTotalWinCreditEvent(allWinCredit);

                //加钱动画
                MainBlackboardController.Instance.AddMyTempCredit(totalWinLineCredit, true, isAddCreditAnim);
            }
            
            //普通赢
            else if (winList.Count > 0 || ContentModel.Instance.bonusResult != null)
            {

                //中奖特效
                if (_spinWEMD.Instance.isSingleWin)
                {
                    //mono.StartCoroutine(PlayKing(1f));
                }
                else
                {
                    //mono.StartCoroutine(PlayKing(2f));
                }

                long totalWinLineCredit = 0;
                totalWinLineCredit = slotMachineCtrl.GetTotalWinCredit(winList);
                allWinCredit += totalWinLineCredit;
                if (winList.Count > 0)
                {
                    //yield return ShowWinListOnceAtNormalSpin(winList);
                    yield return slotMachineCtrl.ShowSymbolWinBySetting(slotMachineCtrl.GetTotalSymbolWin(winList), true, PusherEmperorsRein.SpinWinEvent.TotalWinLine);
                }
                if (!ContentModel.Instance.isMult)
                {
                    //积分同步和退币处理
                    slotMachineCtrl.SendTotalWinCreditEvent(allWinCredit);
                    //加钱动画
                    MainBlackboardController.Instance.AddMyTempCredit(totalWinLineCredit, true, isAddCreditAnim);
                }
                else
                {
                    //积分同步和退币处理
                    slotMachineCtrl.SendTotalWinCreditEvent(allWinCredit);
                    //加钱动画
                    MainBlackboardController.Instance.AddMyTempCredit(allWinCredit);
                }
            }

            //// 本剧同步玩家金钱
            //MainBlackboardController.Instance.SyncMyTempCreditToReal(false);

            if (ContentModel.Instance.isMult)
            {
                //显示中奖动画
                PageManager.Instance.PreloadPage(PageName.XingYunZhiLunPopupZhuanPan, null);
                slotMachineCtrl.SkipWinLine(true);
                slotMachineCtrl.ShowSymbolEffect(TagPoolObject.SymbolHit, new List<int>() { 9 }, true, 9, true);
                yield return slotMachineCtrl.SlotWaitForSeconds(1.5f);
                slotMachineCtrl.SkipWinLine(true);
                slotMachineCtrl.CloseSlotCover();
                //slotMachineCtrl.ShowSymbolEffect(TagPoolObject.SymbolAppear, new List<int>() { 8 }, true, 8, true);

                //过度动画
                isNext = false;
                isMain = false;
                PageManager.Instance.OpenPageAsync(PageName.XingYunZhiLunPopupZhuanPan,
                    new EventData<Dictionary<string, object>>("", new Dictionary<string, object>
                    {
                        ["jackpotType"] = "Multiple",
                    }),
                    (res) =>
                    {
                        //恢复主页面按钮
                        ContentModel.Instance.goAnthorPanel = gOwnerPanel;
                        MainModel.Instance.contentMD.goAnthorPanel = gOwnerPanel;
                        EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
                            new EventData<GComponent>(PanelEvent.AnchorPanelChange, gOwnerPanel));

                        ContentModel.Instance.btnSpinState = SpinButtonState.Stop;

                        if (ContentModel.Instance.isAuto)
                        {
                            ContentModel.Instance.btnSpinState = SpinButtonState.Auto;
                        }
                        else
                        {
                            ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                        }


                        long totalWinLineCredit = 0;
                        totalWinLineCredit = slotMachineCtrl.GetTotalWinCredit(winList);

                        //积分同步和退币处理
                        //slotMachineCtrl.SendTotalWinCreditEvent(totalWinLineCredit * ContentModel.Instance.multiple);
                        //加钱动画
                        //MainBlackboardController.Instance.AddMyTempCredit(totalWinLineCredit, true, isAddCreditAnim);

                        isMain = true;
                        isNext = true;
                    });

                yield return new WaitUntil(() => isNext == true);
                isNext = false;

                slotMachineCtrl.SkipWinLine(true);
                slotMachineCtrl.ShowMultipleHit(new List<int>() { 9 }, true, 9, true);

                //积分同步和退币处理
                slotMachineCtrl.SendTotalWinCreditEvent(allWinCredit * ContentModel.Instance.multiple);
            }
            #endregion

            #region 中游戏彩金

            bool isHitJackpot = ContentModel.Instance.jpGameWinLst.Count > 0;
            List<JackpotWinInfo> jpRes = ContentModel.Instance.jpGameWinLst;

            //List<float> jpCredit = ContentModel.Instance.jpGameWhenCreditLst;
            if (ContentModel.Instance.jackpotWinCredit > 0)
            {
                if(winList.Count > 0)
                {
                    yield return new WaitForSeconds(0.7f);

                    slotMachineCtrl.SendTotalWinCreditEvent(0);
                }

                slotMachineCtrl.SkipWinLine(true);
                slotMachineCtrl.ShowSymbolEffect(TagPoolObject.SymbolHit, new List<int>() { 9 }, true, 9, true);
                yield return slotMachineCtrl.SlotWaitForSeconds(1.5f);
                slotMachineCtrl.SkipWinLine(true);
                slotMachineCtrl.CloseSlotCover();
                //slotMachineCtrl.ShowSymbolEffect(TagPoolObject.SymbolAppear, new List<int>() { 8 }, true, 8, true);
            }

            if (ContentModel.Instance.jackpotWinCredit > 0)
            {
                JackpotWinInfo jpWin = jpRes[0];
                jpRes.RemoveAt(0);

                isNext = false;
                isMain = false;
                JackpotType = jpWin.name;
                winCredit = ContentModel.Instance.jackpotWinCredit;

                PageManager.Instance.OpenPageAsync(PageName.XingYunZhiLunPopupZhuanPan,
                    new EventData<Dictionary<string, object>>("", new Dictionary<string, object>
                    {
                        ["jackpotType"] = "mini",
                        //["totalEarnCredit"] = jpWin.winCredit,
                        //["jpCredit"] = jpCredit,
                    }),
                    (res) =>
                    {
                        GameSoundHelper.Instance.StopMusic();
                        GameSoundHelper.Instance.PlayMusicSingle(SoundKey.JackpotBG);
                        isNext = true;
                    });

                yield return new WaitUntil(() => isNext == true);

                isNext = false;
                slotMachineCtrl.SkipWinLine(true);

                yield return JackpotGameTrigger(() => isNext = true);
                yield return new WaitUntil(() => isNext == true);
                isNext = false;


            }

            #endregion


            // 即中即退
            //yield return CoinOutImmediately(allWinCredit);

            #region 免费游戏
            // Free Spin
            if (ContentModel.Instance.isFreeSpinTrigger)
            {
                if (winList.Count > 0)
                {
                    yield return new WaitForSeconds(0.7f);

                    slotMachineCtrl.SendTotalWinCreditEvent(0);
                    // 本剧同步玩家金钱
                    MainBlackboardController.Instance.SyncMyTempCreditToReal(false);
                }

                //显示中奖动画
                slotMachineCtrl.SkipWinLine(true);
                slotMachineCtrl.ShowSymbolEffect(TagPoolObject.SymbolHit, new List<int>() { 9 }, true, 9, true);
                yield return slotMachineCtrl.SlotWaitForSeconds(1.5f);
                slotMachineCtrl.SkipWinLine(true);
                slotMachineCtrl.CloseSlotCover();
                //slotMachineCtrl.ShowSymbolEffect(TagPoolObject.SymbolAppear, new List<int>() { 8 }, true, 8, true);

                //过度动画
                isNext = false;
                isMain = false;
                PageManager.Instance.OpenPageAsync(PageName.XingYunZhiLunPopupZhuanPan,
                    new EventData<Dictionary<string, object>>("", new Dictionary<string, object>
                    {
                        ["jackpotType"] = "FreeGame",
                    }),
                    (res) =>
                    {
                        GameSoundHelper.Instance.StopMusic();
                        GameSoundHelper.Instance.PlayMusicSingle(SoundKey.FreeSpinBG);
                        isMain = true;
                        isNext = true;
                    });

                yield return new WaitUntil(() => isNext == true);
                isNext = false;

                yield return FreeSpinTrigger(() => isNext = true, errorCallback);


                yield return new WaitUntil(() => isNext == true);
                isNext = false;
            }

            #endregion


            //核对前后端积分
            ERPushMachineDataManager02.Instance.RequestCoinPushSpinEnd(res1 =>
            {

                JSONNode data = JSONObject.Parse((string)res1);

                int code = (int)data["code"];
                int credit = (int)data["credit"];

                if (code != 0)
                {
                    DebugUtils.LogError($" CoinPushSpinEnd(20102) : [0]= {code}");
                }
                else
                {

                    DebugUtils.Log("算法卡积分==" + credit);
                    DebugUtils.Log("机器积分==" + SBoxModel.Instance.myCredit);
                    if (credit != SBoxModel.Instance.myCredit)
                    {

                    }
                    isNext = true;
                }

            });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            // 本剧同步玩家金钱
            MainBlackboardController.Instance.SyncMyTempCreditToReal(true);


            // 进入空闲模式

            //播放游戏空闲音乐
            //GameSoundHelper.Instance.PlaySoundEff(SoundKey.SpinBGIdle);

            if ((winList.Count > 0 || ContentModel.Instance.isMult) && !ContentModel.Instance.isAuto && !ContentModel.Instance.isFreeSpinTrigger)
            {
                if (corGameIdel != null) mono.StopCoroutine(corGameIdel);
                corGameIdel = mono.StartCoroutine(GameIdle(winList));
            }

            if (successCallback != null)
                successCallback.Invoke();
        }

        IEnumerator GameAuto(Action successCallback = null, Action<string> errorCallback = null)
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
                while (Time.time - time < 1f) //应该是1秒之后自动抽取一次
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

        long allWinCredit = 0;
        //免费游戏触发
        IEnumerator FreeSpinTrigger(Action successCallback, Action<string> errorCallback)
        {
            bool isNext = false;
            isMain = false;
            freeTimes.text = (ContentModel.Instance.freeSpinTotalTimes - ContentModel.Instance.freeSpinPlayTimes).ToString();


            PageManager.Instance.OpenPageAsync(PageName.XingYunZhiLunPopupFreeSpinTrigger,
              new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object>()
                    {
                        //["autoCloseTimeS"] = 3f,
                        ["freeSpinCount"] = ContentModel.Instance.freeSpinTotalTimes,
                    }),
                (ed) =>
                {
                    Debug.Log("回调执行！isNext = true"); // 加日志
                    isNext = true;
                    isMain = true;

                    ContentModel.Instance.goAnthorPanel = gOwnerPanel;
                    MainModel.Instance.contentMD.goAnthorPanel = gOwnerPanel;
                    EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
                        new EventData<GComponent>(PanelEvent.AnchorPanelChange, gOwnerPanel));

                    ContentModel.Instance.btnSpinState = SpinButtonState.Stop;

                    ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            slotMachineCtrl.SkipWinLine(true);
            bsTofs.Play();

            bsTofs.SetHook("PlayEffect", () =>
            {
                PlayEffectAnim(fireworkEffect);
            });

            bsTofs.SetHook("End", () =>
            {
                ChangeBGPanel(1);
                isNext = true;
            });


            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            InputStackContextFreeSpin((context) =>
            {
            });

            yield return new WaitForSeconds(1.2f);

            slotMachineCtrl.ChangeSymbolIcon(9, 8);
            slotMachineCtrl.SkipWinLine(true);

            slotMachineCtrl.ShowSymbolTransform(new List<int>() { 8 }, true, 8, true);

            yield return new WaitForSecondsRealtime(1.5f);

            slotMachineCtrl.BeginBonusFreeSpin();

            yield return GameFreeSpin(null, errorCallback);

            slotMachineCtrl.SkipIdle(true);
            slotMachineCtrl.SkipWinLine(true);

            PageManager.Instance.OpenPageAsync(PageName.XingYunZhiLunPopupFreeSpinResult,
                new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object>()
                    {
                        ["baseGameWinCredit"] = ContentModel.Instance.freeSpinTotalWinCredit,
                    }),
                (ed) =>
                {
                    DebugUtils.Log("回调执行！isNext = true"); // 加日志
                    isNext = true;

                    ContentModel.Instance.goAnthorPanel = gOwnerPanel;
                    MainModel.Instance.contentMD.goAnthorPanel = gOwnerPanel;
                    EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
                        new EventData<GComponent>(PanelEvent.AnchorPanelChange, gOwnerPanel));

                    ContentModel.Instance.btnSpinState = SpinButtonState.Stop;

                    if (ContentModel.Instance.isAuto)
                    {
                        ContentModel.Instance.btnSpinState = SpinButtonState.Auto;
                    }
                    else
                    {
                        ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                    }
                });


            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            slotMachineCtrl.SkipWinLine(true);
            StopEffectAnim(fireworkEffect);
            fsTobs.Play();

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
                });

            slotMachineCtrl.EndBonusFreeSpin();


            fsTobs.SetHook("End", () =>
            {
                ChangeBGPanel(0);
                isNext = true;
            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            yield return slotMachineCtrl.SlotWaitForSeconds(0.5f);

            GameSoundHelper.Instance.StopMusic();
            GameSoundHelper.Instance.PlayMusicSingle(SoundKey.RegularBG);

            successCallback?.Invoke();
        }


        //通过控制器切换场景
        private void ChangeBGPanel(int type)
        {
            switch (type)
            {
                case 0:
                    this.contentPane.GetController("c1").selectedPage = "BS";
                    break;
                case 1:
                    this.contentPane.GetController("c1").selectedPage = "FS";
                    break;
                case 2:
                    this.contentPane.GetController("c1").selectedPage = "JS";
                    break;
            }
        }

        void OnSlotDetailEvent(EventData res)
        {

            switch (res.name)
            {
                case SlotMachineEvent.PrepareStoppedReel:
                    {
                        if (ContentModel.Instance.isReelsSlowMotion && !slotMachineCtrl.isStopImmediately)
                        {
                            int colIndex = (int)res.value;
                            if (colIndex == 1)
                            {
                                if (corEffectSlowMotion != null) mono.StopCoroutine(corEffectSlowMotion);
                                corEffectSlowMotion = mono.StartCoroutine(ShowEffectReelsSlowMotion(1));
                            }
                            else if (colIndex == 2)
                            {
                                if (corEffectSlowMotion != null) mono.StopCoroutine(corEffectSlowMotion);
                                corEffectSlowMotion = mono.StartCoroutine(ShowEffectReelsSlowMotion(2));
                            }
                            else if (colIndex == 3)
                            {
                                if (corEffectSlowMotion != null) mono.StopCoroutine(corEffectSlowMotion);
                                corEffectSlowMotion = mono.StartCoroutine(ShowEffectReelsSlowMotion(3));
                            }
                            else if(colIndex == 4)
                            {
                                if (corEffectSlowMotion != null) mono.StopCoroutine(corEffectSlowMotion);
                                corEffectSlowMotion = mono.StartCoroutine(ShowEffectReelsSlowMotion(4));
                            }
                        }
                    }
                    break;
            }

        }


        //开始免费游戏
        IEnumerator GameFreeSpin(Action successCallback, Action<string> errorCallback)
        {
            allWinCredit = 0;
            while (ContentModel.Instance.nextReelStripsIndex == "FS")
            {
                freeTimes.text = (ContentModel.Instance.freeSpinTotalTimes - ContentModel.Instance.freeSpinPlayTimes - 1).ToString();
                yield return GameFreeSpinOnce(null, errorCallback);
                yield return slotMachineCtrl.SlotWaitForSeconds(0.5f);
            }

            if (successCallback != null)
                successCallback.Invoke();
        }


        //一局免费游戏
        IEnumerator GameFreeSpinOnce(Action successCallback, Action<string> errorCallback)
        {
            OnGameReset();

            ContentModel.Instance.gameState = GameState.FreeSpin;

            bool isNext = false;
            bool isBreak = false;
            string errMsg = "";

            //获取结果
            if (ApplicationSettings.Instance.isMock)
            {
                yield return RequestSlotSpinFromMock(() =>
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

            //开始转动
            slotMachineCtrl.BeginSpin();

            slotMachineCtrl.SkipIdle(true);
            slotMachineCtrl.SkipWinLine(true);

            slotMachineCtrl.ShowSymbolIdle(new List<int> { 8 }, true, 8, true);

            if (slotMachineCtrl.isStopImmediately)
            {
                //reelsTurnType = ReelsTurnType.Once;

                if (corReelsTurn != null) mono.StopCoroutine(corReelsTurn);
                corReelsTurn = mono.StartCoroutine(slotMachineCtrl.TurnReelsOnce(ContentModel.Instance.strDeckRowCol,
                    () =>
                    {
                        isNext = true;
                    }));

                yield return new WaitUntil(() => isNext == true);
                isNext = false;
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

                yield return new WaitUntil(() => isNext == true || slotMachineCtrl.isStopImmediately == true);
                isNext = false;

                // 等待移动结束
                if (slotMachineCtrl.isStopImmediately && isNext == false)
                {
                    if (corReelsTurn != null) mono.StopCoroutine(corReelsTurn);
                    corReelsTurn = mono.StartCoroutine(slotMachineCtrl.ReelsToStopOrTurnOnce(() =>
                    {
                        isNext = true;
                    }));

                    yield return new WaitUntil(() => isNext == true);
                    isNext = false;
                }
            }

            List<SymbolWin> winList = ContentModel.Instance.winList;

            if (ContentModel.Instance.tempRows != null && ContentModel.Instance.tempRows.Count > 0 && !slotMachineCtrl.isStopImmediately)
            {
                yield return new WaitForSeconds(0.8f);
            }

            #region Win
            if (winList.Count > 0)
            {
                if (winList.Count > 0)
                {
                    slotMachineCtrl.SkipIdle(true);
                    slotMachineCtrl.SkipWinLine(true);

                    yield return slotMachineCtrl.ShowSymbolWinBySetting(slotMachineCtrl.GetTotalSymbolWin(winList), true, PusherEmperorsRein.SpinWinEvent.TotalWinLine);
                    //yield return ShowWinListOnceAtNormalSpin(winList);
                }

                // 播大奖弹窗
                //WinLevelType winLevelType = GetBigWinType();
                //if (winLevelType != WinLevelType.None)
                //{
                //    slotMachineCtrl.ShowSymbolWinDeck(slotMachineCtrl.GetTotalSymbolWin(winList), true);
                //    Debug.LogError(winLevelType.ToString());

                //    // 大奖弹窗
                //    //yield return BigWinPopup(winLevelType, ContentModel.Instance.baseGameWinCredit);

                //    //slotMachineCtrl.CloseSlotCover();

                //    slotMachineCtrl.SkipWinLine(false);
                //}
                //else
                //{

                //    // 总线赢分（同步？？）
                //    bool isAddToCredit = totalWinLineCredit > TotalBet * 4;
                //    slotMachineCtrl.SendPrepareTotalWinCreditEvent(totalWinLineCredit, isAddToCredit);
                //}
            }
            else
            {
                slotMachineCtrl.SkipIdle(true);
                slotMachineCtrl.SkipWinLine(true);

                slotMachineCtrl.ShowSymbolIdle(new List<int> { 8 }, true, 8, true);
            }

            // 本剧同步玩家金钱
            MainBlackboardController.Instance.SyncMyTempCreditToReal(true);

            long totalWinLineCredit = 0;
            if (ContentModel.Instance.newFreeOnceCredit.Count > ContentModel.Instance.freeSpinPlayTimes - 1)
            {
                totalWinLineCredit = ContentModel.Instance.newFreeOnceCredit[ContentModel.Instance.freeSpinPlayTimes - 1];
            }
            allWinCredit = totalWinLineCredit;
            slotMachineCtrl.SendTotalWinCreditEvent(allWinCredit);


            ContentModel.Instance.freeOnceCredit = totalWinLineCredit;

            #endregion


            // 本局掉币
            //ERPushMachineDataManager.Instance.RequestCoinPushSpinEnd(res1 =>
            //{
            //    isNext = true;
            //});

            //yield return new WaitUntil(() => isNext == true);
            //isNext = false;

            ContentModel.Instance.gameState = GameState.Idle;
            // 先结算主游戏，再进入“免费游戏”或“小游戏”，则每局都可以同步玩家真实金钱金额

            if (successCallback != null)
                successCallback.Invoke();
        }


        //显示加速框
        public IEnumerator ShowEffectReelsSlowMotion(int colIdx)
        {

            ComReelEffect2.xy = slotMachineCtrl.SymbolCenterToNodeLocalPos(colIdx, 1, anchorExpectation);
            ComReelEffect2.visible = true;
            GameSoundHelper.Instance.PlaySoundEff(SoundKey.SlowMotionEffect);

            yield return new WaitUntil(() => isStoppedSlotMachine == true);
            // 关闭Expectation
            ComReelEffect2.visible = false;
        }

        private WinLevelType GetBigWinType()
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


        //游戏状态闲置
        private IEnumerator GameIdle(List<SymbolWin> winList)
        {
            if (winList.Count == 0 && !ContentModel.Instance.isMult)
            {
                yield break;
            }

            SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.GameIdle);

            //yield return new WaitForSeconds(3f);

            if (ContentModel.Instance.isWild)
            {
                yield return WildShowWinListIdle(winList);
            }
            else if (ContentModel.Instance.isMult)
            {
                yield return MultShowWinListIdle();
            }
            //else if (ContentModel.Instance.isLihe)
            //{
            //    int i = 0;
            //    int index = ContentModel.Instance.rewardIndex;
            //    while (i < 3 && ContentModel.Instance.isLihe)
            //    {
            //        i++;
            //        slotMachineCtrl.SkipWinLine(true);
            //        slotMachineCtrl.ShowSymbolEffect(TagPoolObject.SymbolHit, new List<int>() { index }, true, index, true);
            //        yield return new WaitForSeconds(1.2f);
            //    }
            //    yield return slotMachineCtrl.ShowWinListAwayDuringIdle(winList);
            //}
            else
            {
                int i = 0;
                while (i < 3)
                {
                    // 立马停止时，不播放赢分环节？
                    if (slotMachineCtrl.isStopImmediately && _spinWEMD.Instance.isSkipAtStopImmediately)
                        yield break;
                    i++;
                    yield return slotMachineCtrl.ShowSymbolWinBySetting(slotMachineCtrl.GetTotalSymbolWin(winList), true, PusherEmperorsRein.SpinWinEvent.TotalWinLine);
                    yield return new WaitForSeconds(0.7f);
                }
                yield return slotMachineCtrl.ShowWinListAwayDuringIdle(winList);
            }
        }

        /// <summary>
        /// wild的空闲模式下-显示奖励
        /// </summary>
        /// <param name="winList"></param>
        /// <returns></returns>
        public IEnumerator WildShowWinListIdle(List<SymbolWin> winList)
        {
            int[] cols = ContentModel.Instance.cols.ToArray();
            while (ContentModel.Instance.isWild)
            {
                slotMachineCtrl.SkipWinLine(true);

                for (int i = 0; i < cols.Length; i++)
                {
                    GComponent wildEffect = contentPane.GetChild("slotMachine").asCom.GetChild("reels").asCom.GetChild("reel" + (cols[i] + 1)).asCom.GetChild("anchorWild").asCom;

                    // 设置层级
                    FguiSortingOrderManager.Instance.ChangeSortingOrder(wildEffect, gFrame);
                }

                for (int i = 0; i < cols.Length; i++)
                {
                    if (cols[i] < ContentModel.Instance.maxLink)
                    {
                        Animator animator = goWild[cols[i]].transform.GetChild(1).GetChild(0).GetComponent<Animator>();
                        Transform effect = goWild[cols[i]].transform.GetChild(0).GetChild(0).GetChild(0);
                        PlayAnim(animator, "win");
                        PlayEffectAnim(effect);
                    }
                    else
                    {
                        Animator animator = goWild[cols[i]].transform.GetChild(1).GetChild(0).GetComponent<Animator>();
                        Transform effect = goWild[cols[i]].transform.GetChild(0).GetChild(0).GetChild(0);
                        PlayAnim(animator, "idle");
                    }
                }

                slotMachineCtrl.IsWildShowSymbolEffect(TagPoolObject.SymbolHit, winNumber, true, 0, true);
                yield return new WaitForSeconds(1.6f);
            }
        }

        /// <summary>
        /// Multiple的空闲模式下-显示奖励
        /// </summary>
        /// <param name="winList"></param>
        /// <returns></returns>
        public IEnumerator MultShowWinListIdle()
        {
            while (ContentModel.Instance.isMult)
            {
                slotMachineCtrl.SkipWinLine(true);
                slotMachineCtrl.ShowMultipleHit(new List<int>() { 9 }, true, 9, true);
                yield return new WaitForSeconds(2f);
            }
        }

        private void OnGameReset()
        {
            if (corGameIdel != null) mono.StopCoroutine(corGameIdel);
            //mono.StopCoroutine(corEffectSlowMotion);
            slotMachineCtrl.isStopImmediately = false;
            slotMachineCtrl.CloseSlotCover();
            isStoppedSlotMachine = false;
            ComReelEffect2.visible = false;
            if(winNumber.Count > 0) winNumber.Clear();

            if (ContentModel.Instance.isWild)
            {
                int[] cols = ContentModel.Instance.cols.ToArray();
                for (int i = 0; i < cols.Length; i++)
                {
                    GComponent reel = contentPane.GetChild("slotMachine").asCom.GetChild("reels").asCom.GetChild("reel" + (cols[i] + 1)).asCom;
                    GComponent wildEffect = gWild[i];
                    wildEffect.RemoveFromParent();
                    reel.AddChildAt(wildEffect, reel.numChildren);

                    goWild[cols[i]].SetActive(false);
                    GameCommon.FguiUtils.DeleteWrapper(wildEffect);
                }
            }

            if (ContentModel.Instance.isMult)
            {
                ContentModel.Instance.isMult = false;
            }
        }

        private void OnStopSlot(EventData res)
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
                Debug.Log("请求算法结果");
                resNode = JSONNode.Parse((string)res);
                isNext = true;
            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            SBoxJackpotData sboxJackpotData = new SBoxJackpotData();
            // 初始化数组
            sboxJackpotData.Lottery = new int[3];
            sboxJackpotData.JackpotOut = new int[3];
            sboxJackpotData.Jackpotlottery = new int[3];
            sboxJackpotData.JackpotOld = new int[3];
            //获取彩金贡献值
            ERPushMachineDataManager02.Instance.RequestGetJpContribution((res) =>
            {
                Debug.Log("请求本地彩金贡献值");
                JSONNode data = JSONNode.Parse((string)res);
                Debug.Log(data);
                int code = (int)data["code"];

                if (0 != code)
                {
                    DebugUtils.LogError($"请求贡献值报错。 code: {code}");
                    isNext = true;
                    return;
                }

                int majorBet = (int)data["major"];
                int minorBet = (int)data["minor"];
                int miniBet = (int)data["mini"];

                Debug.Log("majorBet:" + majorBet);
                Debug.Log("minorBet:" + minorBet);
                Debug.Log("miniBet:" + miniBet);

                sboxJackpotData.Lottery[0] = 0;
                sboxJackpotData.Lottery[1] = 0;
                sboxJackpotData.Lottery[2] = 0;

                sboxJackpotData.JackpotOut[0] = majorBet;
                sboxJackpotData.JackpotOut[1] = minorBet;
                sboxJackpotData.JackpotOut[2] = miniBet;

                isNext = true;
            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            //赠送局不用扣分
            if (ContentModel.Instance.gameState != GameState.FreeSpin)
            {
                MainBlackboardController.Instance.MinusMyTempCredit(totalBet, true, false);
            }

            Debug.Log("解析数据");
            // 解析数据
            MachineDataG3998Controller.Instance.ParseSlotSpin02(totalBet, resNode, sboxJackpotData);
            // 数据入库
            //MachineDataG200Controller.Instance.TestRecord();
            // ui 彩金
            SetUIJackpotGameReel();
            Debug.Log("获取滚轮成功");

            if (successCallback != null)
                successCallback.Invoke();
        }

        bool isGetCrediting = false;
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

        IEnumerator RequestSlotSpinFromMock(Action successCallback = null, Action<string> errorCallback = null)
        {
            bool isNext = false;
            bool isBreak = false;
            long totalBet = TotalBet;
            JSONNode resNode = null;
            //请求结果
            MachineDataG3998Controller.Instance.RequestSlotSpinFromMock(TotalBet, (res) =>
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

            SlotG3998MachineDataManager.Instance.RequestGetJpMajorGrandContribution((res) =>
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
                    seat = SBoxModel.Instance.pid,
                    betPercent = 1 * 100,
                    scoreRate = 1 * 1000,
                    JPPercent = 1 * 1000,
                    majorBet = majorBet * 100,
                    grandBet = grandBet * 100,
                };

                // 没有联网彩金
                //if (!ClientWS.Instance.IsConnected && !ApplicationSettings.Instance.isMock)
                //{
                //    isNext = true;
                //    return;
                //}

                //NetClientManager.Instance.RequestJackMajorGrand(info, (res) =>
                //{
                //    // 【联网彩金，请求成功 ，删除数据】
                //    cacheTotalJpMajor -= majorBet;
                //    cacheTotalJpGrand -= grandBet;
                //    SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_MAJOR_CONTRIBUTION, cacheTotalJpMajor);
                //    SQLitePlayerPrefs03.Instance.SetInt(CACHE_TOTAL_JP_GRAND_CONTRIBUTION, cacheTotalJpGrand);

                //    sboxJackpotData = res as SBoxJackpotData;

                //    for (int i = 0; i < sboxJackpotData.Jackpotlottery.Length; i++)
                //        sboxJackpotData.Jackpotlottery[i] = sboxJackpotData.Jackpotlottery[i] / 100;

                //    for (int i = 0; i < sboxJackpotData.JackpotOut.Length; i++)
                //        sboxJackpotData.JackpotOut[i] = sboxJackpotData.JackpotOut[i] / 100;

                //    for (int i = 0; i < sboxJackpotData.JackpotOld.Length; i++)
                //        sboxJackpotData.JackpotOld[i] = sboxJackpotData.JackpotOld[i] / 100;

                //    // 【如果获取到联网彩金-通知算法卡】
                //    if (sboxJackpotData.Lottery[0] == 1)
                //    {
                //        ERPushMachineDataManager.Instance.RequestSetMajorGrandWin(sboxJackpotData.Jackpotlottery[0], (res) =>
                //        {

                //        });
                //    }
                //    if (sboxJackpotData.Lottery[1] == 1)
                //    {
                //        ERPushMachineDataManager.Instance.RequestSetMajorGrandWin(sboxJackpotData.Jackpotlottery[1], (res) =>
                //        {

                //        });
                //    }
                //    isNext = true;

                //}, (err) => // 联网彩金，请求失败
                //{
                //    errorCallback?.Invoke(err.msg);
                //    isNext = true;
                //    isBreak = true;
                //});

                isNext = true;
            });

            isNext = true;
            yield return new WaitUntil(() => isNext == true);
            isNext = false;
            if (isBreak) yield break;

            // 【贡献返回给算法卡】
            if (cacheTotalJpMajor > 10 || cacheTotalJpGrand > 10)
            {
                ERPushMachineDataManager.Instance.RequestReturnMajorGrandContribution(
                    cacheTotalJpMajor > 10 ? cacheTotalJpMajor : 0,
                    cacheTotalJpGrand > 10 ? cacheTotalJpGrand : 0,
                    (res) =>
                    {

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

            //赠送局不用扣分
            if (ContentModel.Instance.gameState != GameState.FreeSpin)
            {
                MainBlackboardController.Instance.MinusMyTempCredit(totalBet, true, false);
            }

            // 解析数据
            MachineDataG3998Controller.Instance.ParseSlotSpin02(totalBet, resNode, sboxJackpotData);

            // 数据入库

            // 游戏彩金滚轮
            //SetUIJackpotGameReel();

            if (successCallback != null)
                successCallback.Invoke();
        }

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

        private void ReadJsonBet()
        {
            //资源加载
            ResourceManager02.Instance.LoadAsset<TextAsset>(
                "Assets/GameRes/_Common/Game Maker/ABs/G3998/Data/game_info_g3998.json", (txt) =>
                {
                    //JSON解析与错误处理
                    GameConfigRoot config = JsonConvert.DeserializeObject<GameConfigRoot>(txt.text);
                    if (config?.SymbolPaytable == null)
                    {
                        Debug.LogError("解析symbol_paytable失败，数据为空");
                        return;
                    }

                    MainModel.Instance.gameID = config.GameId;
                    MainModel.Instance.gameName = config.GameName;
                    MainModel.Instance.displayName = config.DisplayName;

                    //赢钱倍数处理
                    foreach (var item in config.WinLevelMultiple)
                    {
                        string winKey = item.Key;
                        long winValue = item.Value;
                        MainModel.Instance.contentMD.winLevelMultiple.Add(new WinMultiple(winKey, winValue));
                    }

                    //符号支付表处理
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
                            Debug.LogWarning($"无效的符号键：{symbolKey}，无法解析索引");
                        }
                    }

                    //支付线处理
                    if (ContentModel.Instance.payLines == null)
                    {
                        ContentModel.Instance.payLines = new List<List<int>>() { };
                    }
                    foreach (var item in config.pay_lines)
                    {
                        ContentModel.Instance.payLines.Add(item);
                    }
                });
        }

        //显示线和中奖图标
        IEnumerator ShowWinListOnceAtNormalSpin(List<SymbolWin> winList)
        {
            //停止特效显示
            slotMachineCtrl.SkipWinLine(true);
            slotMachineCtrl.CloseSlotCover();

            //总线
            if (_spinWEMD.Instance.isTotalWin)
            {
                yield return slotMachineCtrl.ShowSymbolWinBySetting(slotMachineCtrl.GetTotalSymbolWin(winList), true, PusherEmperorsRein.SpinWinEvent.TotalWinLine);
            }
            else
            {
                //单线
                slotMachineCtrl.SkipWinLine(false);
                int idx = 0;
                while (idx < winList.Count)
                {
                    SymbolWin curSymvolWin = winList[idx];
                    yield return slotMachineCtrl.ShowSymbolWinBySetting(curSymvolWin, true, PusherEmperorsRein.SpinWinEvent.SingleWinLine);
                    ++idx;
                }

                yield return new WaitForSeconds(1.2f);
                //停止特效显示
                slotMachineCtrl.SkipWinLine(true);
                slotMachineCtrl.CloseSlotCover();
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


        //轮盘初始化图片
        private void WheelInit(List<int> wheelSymbolsIndex)
        {
            GComponent symbols = gWheel.GetChild("Symbols").asCom;
            for (int i = 0; i < symbols.numChildren; i++)
            {
                // 使用 GetChildAt 按索引获取，不需要知道具体名称
                GObject child = symbols.GetChildAt(i);

                if (child.asCom != null) // 确保是 GComponent
                {
                    GComponent symbol = child.asCom;
                    // 在这里处理每个 symbol
                    GLoader gLoader = symbol.GetChild("animator").asCom.GetChild("icon").asLoader;
                    gLoader.url = CustomModel.Instance.wheelSymbolIcon[wheelSymbolsIndex[UnityEngine.Random.Range(0, wheelSymbolsIndex.Count)].ToString()];
                }
            }
        }

        //转盘转动控制
        private void WheelTrun()
        {
            gWheel.rotation += rotateSpeed * Time.deltaTime;
            if (gWheel.rotation >= 360)
            {
                gWheel.rotation = 0;
            }
        }

        IEnumerator JackpotGameTrigger(Action successCallback, Action errorCallback = null)
        {
            bool isNext = false;
            PageManager.Instance.OpenPageAsync(PageName.XingYunZhiLunPopupJackpotGameTrigger,
                    new EventData<Dictionary<string, object>>("", new Dictionary<string, object>
                    {
                    }),
                    (res) =>
                    {
                        isNext = true;
                    });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;


            //初始化彩金界面
            InitScroll();
            ChangeBGPanel(2);
            SetJackpotMask(false);
            initTransition.Play();
            gJackpotBg.visible = true;
            slotMachineCtrl.isStopImmediately = false;

            //进入彩金游戏动画
            PageManager.Instance.OpenPageAsync(PageName.XingYunZhiLunPopupJackpotGameEnter,
                    new EventData<Dictionary<string, object>>("", new Dictionary<string, object>
                    {
                    }),
                    (res) =>
                    {
                        //恢复主页面按钮
                        ContentModel.Instance.goAnthorPanel = gOwnerPanel;
                        MainModel.Instance.contentMD.goAnthorPanel = gOwnerPanel;
                        EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
                            new EventData<GComponent>(PanelEvent.AnchorPanelChange, gOwnerPanel));

                        ContentModel.Instance.btnSpinState = SpinButtonState.Stop;

                        if (ContentModel.Instance.isAuto)
                        {
                            ContentModel.Instance.btnSpinState = SpinButtonState.Auto;
                        }
                        else
                        {
                            ContentModel.Instance.isSpin = false;
                            ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
                            ContentModel.Instance.gameState = GameState.Idle;
                            
                        }

                        isNext = true;
                        isMain = true;
                    });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            startTransition.Play();
            JackpotFinish = false;
            isJackpot = true;

            if (ContentModel.Instance.isAuto)
            {
                yield return new WaitForSeconds(1 / Time.timeScale);
                StartRandomRoll(GetJackpotId()); //开始玩
            }

            yield return new WaitUntil(() => JackpotFinish == true);

            isMain = false;

            //PageManager.Instance.OpenPageAsync(PageName.XingYunZhiLunPopupJackpotGameResult,
            //    new EventData<Dictionary<string, object>>("", new Dictionary<string, object>
            //    {
            //        ["jackpotType"] = JackpotType,
            //        ["totalEarnCredit"] = winCredit,
            //    }),
            //    (res) =>
            //    {
            //        isNext = true;

            //        //积分同步和退币处理
            //        slotMachineCtrl.SendTotalWinCreditEvent((long)winCredit);
            //    });
            //yield return new WaitUntil(() => isNext == true);
            //isNext = false;

            PageManager.Instance.OpenPageAsync(PageName.XingYunZhiLunPopupJackpotGameExit,
                new EventData<Dictionary<string, object>>("", new Dictionary<string, object>
                {
                    ["totalEarnCredit"] = winCredit,
                }),
                (res) =>
                {
                    isNext = true;
                });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            //切换回普通场景
            ChangeBGPanel(0);
            gJackpotBg.visible = false;

            //退出彩金游戏动画
            PageManager.Instance.OpenPageAsync(PageName.XingYunZhiLunPopupJackpotGameQuit,
                    new EventData<Dictionary<string, object>>("", new Dictionary<string, object>
                    {
                    }),
                    (res) =>
                    {
                        //恢复主页面按钮
                        ContentModel.Instance.goAnthorPanel = gOwnerPanel;
                        MainModel.Instance.contentMD.goAnthorPanel = gOwnerPanel;
                        EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
                            new EventData<GComponent>(PanelEvent.AnchorPanelChange, gOwnerPanel));

                        ContentModel.Instance.btnSpinState = SpinButtonState.Stop;

                        if (ContentModel.Instance.isAuto)
                        {
                            ContentModel.Instance.btnSpinState = SpinButtonState.Auto;
                        }
                        else
                        {
                            ContentModel.Instance.isSpin = true;
                            ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                            ContentModel.Instance.gameState = GameState.Spin;
                        }

                        isNext = true;
                        isMain = true;
                    });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            GameSoundHelper.Instance.StopMusic();
            GameSoundHelper.Instance.PlayMusicSingle(SoundKey.RegularBG);

            if (successCallback != null)
            {
                successCallback.Invoke();
            }
        }

        private void JackpotBgOpen()
        {
            GComponent loadAnchorHostTip = gJackpotBg.GetChild("anchorHost").asCom;
            if (loadAnchorHost != loadAnchorHostTip)
            {
                GameCommon.FguiUtils.DeleteWrapper(loadAnchorHost);
                loadAnchorHost = loadAnchorHostTip;
                goHost = GameObject.Instantiate(goSpineHost);
                hostAnimator = goHost.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                GameCommon.FguiUtils.AddWrapper(loadAnchorHost, goHost);
            }

            GComponent loadAnchorGuestTip = gJackpotBg.GetChild("anchorGuest").asCom;
            if (loadAnchorGuest != loadAnchorGuestTip)
            {
                GameCommon.FguiUtils.DeleteWrapper(loadAnchorGuest);
                loadAnchorGuest = loadAnchorGuestTip;
                goGuest = GameObject.Instantiate(goSpineGuest);
                GameCommon.FguiUtils.AddWrapper(loadAnchorGuest, goGuest);
            }

            GComponent loadAnchorLightTip = gJackpotBg.GetChild("mask").asCom.GetChild("anchorLight").asCom;
            if (loadAnchorLight != loadAnchorLightTip)
            {
                GameCommon.FguiUtils.DeleteWrapper(loadAnchorLight);
                loadAnchorLight = loadAnchorLightTip;
                goLight = GameObject.Instantiate(goSpineLight);
                GameCommon.FguiUtils.AddWrapper(loadAnchorLight, goLight);
            }

            startTransition = gJackpotBg.GetTransition("Start");
            initTransition = gJackpotBg.GetTransition("Init");
            gMask = gJackpotBg.GetChild("mask").asCom.GetChild("mask").asImage;

            SetJackpotMask(false);
        }


        private void SetJackpotMask(bool isVistual)
        {
            gMask.visible = isVistual;
        }


        public float itemWidth = 460f;        // 单个Item宽度
        public float itemSpacing = 110f;      // Item间距
        public float itemTotalWidth = 570f;   // 总Item宽度（包括间距）
        public float listWidth = 1590f;       // 列表总宽度

        // 滚动状态
        private float currentPos = 0f;
        private float currentSpeed = 0f;
        private bool isRolling = false;

        // 中奖数据
        private int finalWinnerIndex = -1;
        private Dictionary<int, string> specialIcons = new Dictionary<int, string>();

        // 协程
        private Coroutine rollCoroutine = null;

        private GameObject goEffect;
        private GComponent gEffect;
        private Animator listSymbolAnimator, hostAnimator;
        private bool toSetEffect;

        // 初始化方法
        public void InitScroll()
        {
            if (gList != null)
            {
                specialIcons.Clear();
                listWidth = gList.viewWidth;

                gList.SetVirtualAndLoop();
                gList.itemRenderer = RenderListItem;

                gList.numItems = 80;
                currentPos = gList.scrollPane.posX;

                DoSpecialEffect();
            }
        }

        // 开始随机滚动
        public void StartRandomRoll(int iconIndex)
        {
            if (gList == null || isRolling) return;

            isRolling = true;

            if (rollCoroutine != null)
                mono.StopCoroutine(rollCoroutine);

            Action successCallback = () =>{
                isJackpot = false;
                JackpotFinish = true;
                GameCommon.FguiUtils.DeleteWrapper(gEffect);

                GameObject.Destroy(goEffect);
                goEffect = null;
                gEffect = null;
                listSymbolAnimator = null;
            };

            rollCoroutine = mono.StartCoroutine(RandomPhysicsRoll(iconIndex, successCallback));
        }

        // 停止滚动
        public void StopRolling()
        {
            if (rollCoroutine != null)
            {
                mono.StopCoroutine(rollCoroutine);
                rollCoroutine = null;
            }

            isRolling = false;
            currentSpeed = 0f;
        }

        // 随机物理滚动协程
        private IEnumerator RandomPhysicsRoll(int iconIndex, Action successCallback = null)
        {
            float totalWidth = gList.numItems * itemTotalWidth;
            float actualCenter = GetActualListCenter();

            // 1. 保留你原有参数逻辑
            float initialSpeed = Random.Range(7500f, 9000f) * Time.timeScale;
            float friction = Random.Range(0.94f, 0.97f);

            // 2. 保留你原有加速阶段（仅新增deltaTime限制，不影响正常场景）
            currentSpeed = 0f;
            float accelTime = 0.2f / Time.timeScale;
            float accelElapsed = 0f;
            while (accelElapsed < accelTime)
            {
                float deltaTime = Time.deltaTime;
                if (Time.timeScale > 1f)
                    deltaTime = Mathf.Clamp(deltaTime, 0.001f, 0.03f / Time.timeScale); // 限制最小值0.001f
                accelElapsed += deltaTime;
                float t = accelElapsed / accelTime;
                t = 1f - Mathf.Pow(1f - t, 2);

                currentSpeed = Mathf.Lerp(0, initialSpeed, t);
                UpdateScrollPosition();
                yield return null;
            }
            currentSpeed = initialSpeed;

            // 3. 保留你原有高速旋转阶段（仅新增deltaTime限制）
            float spinTime = Random.Range(1.5f, 2.0f) / Time.timeScale;
            float spinElapsed = 0f;
            while (spinElapsed < spinTime)
            {
                float deltaTime = Time.deltaTime;
                if (Time.timeScale > 1f) deltaTime = Mathf.Min(deltaTime, 0.03f / Time.timeScale);
                spinElapsed += deltaTime;
                float speedVariation = 0.98f + Mathf.Sin(spinElapsed * 5f) * 0.02f;
                currentSpeed = initialSpeed * speedVariation;
                UpdateScrollPosition();

                if (slotMachineCtrl.isStopImmediately)
                {
                    break;
                }
                yield return null;
            }

            // 4. 保留你原有自然减速阶段（仅修改模拟步长，不改变逻辑）
            bool winnerDecided = false;
            float decelThreshold = 100f;
            if (Time.timeScale > 1f)
                decelThreshold = decelThreshold * Time.timeScale;
            while (currentSpeed > decelThreshold)
            {
                float dynamicFriction = friction;
                if (Time.timeScale > 1f)
                {
                    // 10倍速下摩擦系数降低（比如0.94→0.88），减速效率提升
                    dynamicFriction = friction - (Time.timeScale - 1) * 0.006f;
                    // 限制下限，避免减速过快失控
                    dynamicFriction = Mathf.Clamp(dynamicFriction, 0.88f, 0.97f);
                }
                currentSpeed *= dynamicFriction;

                if (!winnerDecided)
                {
                    float currentDisplay = gList.scrollPane.posX;
                    float normalizedCurrentDisplay = NormalizePosition(currentDisplay);
                    float currentScreenCenter = normalizedCurrentDisplay + GetActualListCenter();
                    float normalizedScreenCenter = NormalizePosition(currentScreenCenter);

                    // ========== 仅修改模拟步长（兼容倍速） ==========
                    float simSpeed = currentSpeed;
                    float simFriction = friction;
                    float simDistance = 0f;
                    int simulationSteps = 0;

                    while (simSpeed > 10f && simulationSteps < 60) // 保留60帧，不随倍速减少
                    {
                        simSpeed *= simFriction;
                        // 帧时间用「真实时间」（0.0167f），不乘Time.timeScale（避免模拟距离过大）
                        float frameTime = 0.0167f;
                        simDistance += simSpeed * frameTime;
                        simulationSteps++;
                    }
                    // ========== 模拟步长修改结束 ==========

                    float predictedCenter = normalizedScreenCenter + simDistance;
                    float normalizedPredictedCenter = NormalizePosition(predictedCenter);
                    float predictedAdjustedCenter = normalizedPredictedCenter - (itemWidth / 2f);
                    float normalizedAdjustedCenter = NormalizePosition(predictedAdjustedCenter);
                    finalWinnerIndex = Mathf.FloorToInt(normalizedAdjustedCenter / itemTotalWidth) % gList.numItems;

                    SetWinnerIcon(iconIndex);
                    winnerDecided = true;
                }
                else
                {
                    // 完全保留你原有距离计算逻辑
                    float currentDisplay = gList.scrollPane.posX;
                    float currentScreenCenter = currentDisplay + GetActualListCenter();
                    float targetCenter = finalWinnerIndex * itemTotalWidth + (itemWidth / 2f);
                    float distanceToTarget = targetCenter - currentScreenCenter;
                    float shortestDistance = distanceToTarget;
                    float totalWidth1 = gList.numItems * itemTotalWidth;

                    if (Mathf.Abs(distanceToTarget + totalWidth1) < Mathf.Abs(shortestDistance))
                        shortestDistance = distanceToTarget + totalWidth1;
                    if (Mathf.Abs(distanceToTarget - totalWidth1) < Mathf.Abs(shortestDistance))
                        shortestDistance = distanceToTarget - totalWidth1;

                    float absDistance = Mathf.Abs(shortestDistance);
                    if (absDistance < itemTotalWidth * 5f)
                    {
                        float idealSpeedForDistance = absDistance * 2f + 50f;
                        if (currentSpeed > idealSpeedForDistance * 1.5f)
                        {
                            currentSpeed *= 0.9f;
                        }
                        else if (currentSpeed < idealSpeedForDistance * 0.5f && absDistance > itemTotalWidth)
                        {
                            currentSpeed *= 1.05f;
                        }
                    }
                }
                UpdateScrollPosition();
                yield return null;
            }

            // 5. 保留你原有最后停止逻辑
            // 倍速下调整停止摩擦系数
            float stopFriction = 0.9f;
            if (Time.timeScale > 1f)
                stopFriction = 0.7f + (Time.timeScale - 1) * 0.02f; // 10倍速下≈0.88（更快减速）
            stopFriction = Mathf.Clamp(stopFriction, 0.7f, 0.9f);

            while (currentSpeed > 10f * Time.timeScale) // 阈值也随倍速提高
            {
                currentSpeed *= stopFriction;
                UpdateScrollPosition();
                yield return null;
            }
            currentSpeed = 0f;

            // 6. 保留你原有吸附前逻辑
            float stopDisplayPos = gList.scrollPane.posX;
            float stopScreenCenter = stopDisplayPos + actualCenter;
            float adjustedCenter = stopScreenCenter - (itemWidth / 2f);
            int actualCenterIndex = Mathf.RoundToInt(adjustedCenter / itemTotalWidth) % gList.numItems;
            int indexDiff = Mathf.Abs((actualCenterIndex - finalWinnerIndex + gList.numItems) % gList.numItems);

            stopDisplayPos = gList.scrollPane.posX;
            float normalizedStopDisplayPos = NormalizePosition(stopDisplayPos);
            stopScreenCenter = normalizedStopDisplayPos + GetActualListCenter();
            float normalizedStopScreenCenter = NormalizePosition(stopScreenCenter);
            float targetItemCenter = finalWinnerIndex * itemTotalWidth + (itemWidth / 2f);
            float pixelOffset = GetShortestDistance(normalizedStopScreenCenter, targetItemCenter);

            PlayAnim(hostAnimator, "start");
            if (Mathf.Abs(pixelOffset) > 5f)
            {
                yield return SmartAttachToCenter(pixelOffset, normalizedStopDisplayPos);
            }

            isRolling = false;

            if(JackpotListEffect != null)
            {
                goEffect.SetActive(true);
                PlayEffectAnim(JackpotListEffect);
            }

            // ========== 仅修改等待逻辑（兼容倍速） ==========
            // 正常场景用WaitForSeconds，倍速场景按比例缩短
            float waitTime = Time.timeScale > 1f ? 1.5f / Time.timeScale : 1.5f;
            //yield return new WaitForSeconds(waitTime);
            // ========== 等待逻辑修改结束 ==========

            if (listSymbolAnimator != null)
            {
                SetJackpotMask(true);
                PlayAnim(listSymbolAnimator, "win");
            }

            yield return new WaitForSeconds(waitTime);
            successCallback?.Invoke();
        }

        // 智能吸附方法
        private IEnumerator SmartAttachToCenter(float pixelOffset, float currentNormalizedPos)
        {
            float totalWidth = gList.numItems * itemTotalWidth;
            float actualCenter = GetActualListCenter();
            float targetDisplayPos = NormalizePosition(currentNormalizedPos + pixelOffset);

            float currentDisplay = currentNormalizedPos;
            float distance = GetShortestDistance(currentDisplay, targetDisplayPos);

            // 新增：吸附时间随倍速缩短
            float adjustTime = 0.15f;
            if (Time.timeScale > 1f)
                adjustTime = adjustTime / Time.timeScale;

            float elapsed = 0f;
            while (elapsed < adjustTime)
            {
                float deltaTime = Time.deltaTime;
                // 可选：同样限制步长范围
                if (Time.timeScale > 1f)
                    deltaTime = Mathf.Clamp(deltaTime, 0.001f, 0.03f / Time.timeScale);

                elapsed += deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / adjustTime);
                float newPos = currentDisplay + distance * t;
                float displayPos = NormalizePosition(newPos);

                gList.scrollPane.SetPosX(displayPos, false);
                DoSpecialEffect();

                yield return null;
            }

            float finalPos = NormalizePosition(targetDisplayPos);
            gList.scrollPane.SetPosX(finalPos, false);
        }

        // 获取实际的屏幕中间位置
        private float GetActualListCenter()
        {
            return listWidth / 2f;
        }

        // 设置中奖图标
        private void SetWinnerIcon(int iconIndex)
        {
            specialIcons[finalWinnerIndex] = iconIndex.ToString(); // iconIndex是中奖图标

            gList.RefreshVirtualList();
        }

        // 更新滚动位置
        private void UpdateScrollPosition()
        {
            float deltaTime = Time.deltaTime;
            // 仅限制单帧步长的「绝对最大值」，不随倍速缩小（避免过慢）
            // 正常场景：0.03f → 最大步长0.03；10倍速：0.03f → 最大步长0.03（而非0.003）
            if (Time.timeScale > 1f)
            {
                deltaTime = Mathf.Min(deltaTime, 0.03f); // 取消 /Time.timeScale
                                                         // 速度上限只小幅降低，不随倍速等比压（10倍速下上限6000，而非800）
                currentSpeed = Mathf.Clamp(currentSpeed, 0f, 8000f - (Time.timeScale - 1) * 200);
            }

            currentPos += currentSpeed * deltaTime;
            float displayPos = NormalizePosition(currentPos);

            gList.scrollPane.SetPosX(displayPos, false);
            DoSpecialEffect();
        }

        // 渲染列表项
        private void RenderListItem(int index, GObject obj)
        {
            GComponent listSymbols = obj.asCom.GetChild("Symbol").asCom.GetChild("anchorListSymbol").asCom;
            obj.asCom.SetPivot(0.5f, 0.5f);

            // 获取图标索引
            string iconIndex = GetIconIndexForItem(index, out toSetEffect);
            GLoader exampleLoader = listSymbols.GetChild("example").asLoader;
            exampleLoader.url = CustomModel.Instance.ListSymbolsIcon[iconIndex];

            GTextField sorceText = exampleLoader.component.GetChild("Socre").asTextField;
            if (sorceText != null)
            {
                sorceText.text = UnityEngine.Random.Range(50000, 500000).ToString();
            }

            if (toSetEffect)
            {
                sorceText.text = "114514";
                if(ContentModel.Instance.jackpotWinCredit != 0)
                {
                    sorceText.text = ContentModel.Instance.jackpotWinCredit.ToString();
                }

                string symbolName = CustomModel.Instance.jackpotHitEffect["10"];
                gEffect = obj.asCom.GetChild("Symbol").asCom.GetChild("anchorEffect").asCom;

                ResourceManager02.Instance.LoadAsset<GameObject>(
                symbolName,
                (GameObject clone) =>
                {
                    goEffect = GameObject.Instantiate(clone);
                    listSymbolAnimator = goEffect.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                    JackpotListEffect = goEffect.transform.GetChild(1).GetChild(0).GetChild(0);
                    GameCommon.FguiUtils.AddWrapper(gEffect, goEffect);
                    goEffect.SetActive(false);
                });

                //if (!strongTemp.ContainsKey(symbolName))
                //{
                //    ResourceManager02.Instance.LoadAsset<GameObject>(
                //    symbolName,
                //    (GameObject clone) =>
                //    {
                //        goEffect = GameObject.Instantiate(clone);
                //        strongTemp[symbolName] = goEffect;
                //        Debug.LogError(goEffect);
                //        listSymbolAnimator = goEffect.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                //        JackpotListEffect = goEffect.transform.GetChild(1).GetChild(0).GetChild(0);
                //        GameCommon.FguiUtils.AddWrapper(gEffect, goEffect);
                //        goEffect.SetActive(false);
                //    });
                //}
                //else
                //{
                //    goEffect = strongTemp[symbolName];
                //    listSymbolAnimator = goEffect.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                //    JackpotListEffect = goEffect.transform.GetChild(1).GetChild(0).GetChild(0);
                //    GameCommon.FguiUtils.AddWrapper(gEffect, goEffect);
                //    goEffect.SetActive(false);
                //}
            }
        }

        // 获取特定索引的图标
        private string GetIconIndexForItem(int index, out bool isWinner)
        {
            isWinner = false;
            // 如果是中奖索引，返回中奖图标
            if (specialIcons.ContainsKey(index))
            {
                isWinner = true;
                return specialIcons[index];
            }

            // 其他索引随机
            return Random.Range(5, 10).ToString();
        }

        // 放大效果
        private void DoSpecialEffect()
        {
            if (gList == null) return;

            float listCenter = gList.x + gList.scrollPane.posX + gList.viewWidth / 2;

            for (int i = 0; i < gList.numChildren; i++)
            {
                GObject item = gList.GetChildAt(i);

                float itemCenter = gList.x + item.x + item.width / 2;
                float itemWidthValue = item.width;
                float distance = Mathf.Abs(listCenter - itemCenter);

                if (distance < itemWidthValue)
                {
                    float distanceRange = 1 + (1 - distance / itemWidthValue) * 0.3f;
                    item.SetScale(distanceRange, distanceRange);
                }
                else
                {
                    item.SetScale(1, 1);
                }
            }
        }

        // 将任意坐标归一化到[0, totalWidth)范围内
        private float NormalizePosition(float position)
        {
            float totalWidth = gList.numItems * itemTotalWidth;
            float normalized = position % totalWidth;
            if (normalized < 0) normalized += totalWidth;
            return normalized;
        }

        // 获取两个位置之间的最短距离（考虑循环）
        private float GetShortestDistance(float fromPos, float toPos)
        {
            float totalWidth = gList.numItems * itemTotalWidth;
            if (totalWidth <= 0) return toPos - fromPos;

            float directDistance = toPos - fromPos;
            float shortest = directDistance;

            // 保留你原有逻辑
            if (Mathf.Abs(directDistance + totalWidth) < Mathf.Abs(shortest))
                shortest = directDistance + totalWidth;
            if (Mathf.Abs(directDistance - totalWidth) < Mathf.Abs(shortest))
                shortest = directDistance - totalWidth;

            // 仅倍速下新增容错（不影响正常场景）
            if (Time.timeScale > 1f)
            {
                // 倍速下缩小阈值，避免选反方向导致回拉
                float threshold = totalWidth / 3f;
                if (Mathf.Abs(shortest) > threshold)
                {
                    shortest = (shortest > 0) ? shortest - totalWidth : shortest + totalWidth;
                }
            }

            return shortest;
        }

        public void SetUIJackpotGameReel()
        {
            JackpotRes info = ContentModel.Instance.jpGameRes;

            //ContentModel.Instance.uiGrandJP.nowCredit = uiJPGrandCtrl.nowData;
            //ContentModel.Instance.uiMegaJP.nowCredit = uiJPMegaCtrl.nowData;
            ContentModel.Instance.uiMajorJP.nowCredit = uiJPMajorCtrl.nowData;
            ContentModel.Instance.uiMinorJP.nowCredit = uiJPMinorCtrl.nowData;
            ContentModel.Instance.uiMiniJP.nowCredit = uiJPMiniCtrl.nowData;

            // ContentModel.Instance.uiGrandJP.curCredit = info.curJackpotGrand;
            //ContentModel.Instance.uiMegaJP.curCredit = info.curJackpotMega;
            ContentModel.Instance.uiMajorJP.curCredit = info.curJackpotMajor;
            ContentModel.Instance.uiMinorJP.curCredit = info.curJackpotMinior;
            ContentModel.Instance.uiMiniJP.curCredit = info.curJackpotMini;

            // 游戏滚轮显示
            //uiJPGrandCtrl.SetData(ContentModel.Instance.jpGameWhenCreditLst[0]);
            //uiJPMegaCtrl.SetData(ContentModel.Instance.jpGameWhenCreditLst[1]);
            //uiJPMajorCtrl.SetData(ContentModel.Instance.jpGameWhenCreditLst[1]);
            //uiJPMinorCtrl.SetData(ContentModel.Instance.jpGameWhenCreditLst[2]);
            //uiJPMiniCtrl.SetData(ContentModel.Instance.jpGameWhenCreditLst[3]);

            uiJPMajorCtrl.SetData(info.curJackpotMajor);
            uiJPMinorCtrl.SetData(info.curJackpotMinior);
            uiJPMiniCtrl.SetData(info.curJackpotMini);

        }

        private int GetJackpotId()
        {
            int id = 0;
            //switch (JackpotType)
            //{
            //    case "mini":
            //        id = 0;
            //        break;
            //    case "minor":
            //        id = 1;
            //        break;
            //    case "major":
            //        id = 2; 
            //        break;
            //    case "grand":
            //        id = 3;
            //        break;

            //}

            id = UnityEngine.Random.Range(5, 10);
            return id;
        }

        //播放指定动画
        private void PlayAnim(Animator animator, string animName)
        {
            animator.Rebind();
            animator.Play(animName);
            animator.Update(0f);
        }

        //根据传入的节点依次播放粒子特效
        private void PlayEffectAnim(Transform effect)
        {
            ParticleSystem particle = effect.GetComponent<ParticleSystem>();
            particle.Play();

            // 递归播放所有子物体的粒子系统
            foreach (Transform child in effect)
            {
                PlayEffectAnim(child);
            }
        }

        //根据传入的节点依次停止粒子特效
        private void StopEffectAnim(Transform effect)
        {
            ParticleSystem particle = effect.GetComponent<ParticleSystem>();
            particle.Stop(true);

            // 递归播放所有子物体的粒子系统
            foreach (Transform child in effect)
            {
                StopEffectAnim(child);
            }
        }
    }

}