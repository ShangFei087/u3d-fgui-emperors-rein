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
using System.Runtime.Remoting.Contexts;
using UnityEngine;

using _spinWEMD = SlotMaker.SpinWinEffectSettingModel;

namespace CaiFuHuoChe_3996
{
    public class GameConfigRoot
    {
        [JsonProperty("game_id")] public int GameId;

        [JsonProperty("game_name")] public string GameName;

        [JsonProperty("display_name")] public string DisplayName;

        [JsonProperty("line_num")] public int LineNum;//线数

        [JsonProperty("win_level_multiple")] public Dictionary<string, long> WinLevelMultiple { get; set; }

        [JsonProperty("symbol_paytable")] public Dictionary<string, PayTableSymbolInfo> SymbolPaytable { get; set; }

        [JsonProperty("pay_lines")] public List<List<int>> pay_lines { get; set; }
    }


    public class PageGameMain : MachinePageBase //: PageBase
    {
        public const string pkgName = "CaiFuHuoChe_3996";
        public const string resName = "PageGameMain";


        private SlotMachineController3996 slotMachineCtrl;
        private GComponent slotCover, gOwnerPanel, gPlayLines, gFrame, gTrain, gFreeCloude;

        private GameObject goGameCtrl;

        PayTableController payTableController = new PayTableController();
        Coroutine corReelsTurn, corGameIdel, corGameOnce, corEffectSlowMotion, corRewardEffect;

        //游戏控制
        private MonoHelper mono;
        private FguiPoolHelper fguiPoolHelper;
        private FguiGObjectPoolHelper gObjectPoolHelper;

        private string JackpotType = "";
        private float winCredit = 0;

        long TotalBet => (long)MainModel.Instance.contentMD.totalBet;

        private new bool isInit = false;        //是否初始化
        private bool isInitPool = false;
        private bool tipCoinIn = false; //提示硬币输入
        private bool isStoppedSlotMachine = false;

        //加速框
        private GComponent anchorExpectation, ComReelEffect2, ComReelEffect3;
        private GameObject goFreeReelEffcet, goJackpotReelEffect;

        //免费游戏以及彩金游戏中特殊奖时特效
        private GameObject goRewardEffect;
        private GComponent anchorFreeAdd, anchorJackpotAdd, anchorFill1, anchorFill2, anchorFill3, anchorFill4, ComRewardEffect1, ComRewardEffect2, ComRewardEffect3;

        //正常游戏和彩金游戏和免费游戏之间转场火车开门时特效
        private GameObject goOpenEffect;
        private GComponent anchorOpenEffect;
        private Transform fgToNor, norToFg;

        //火车预制体、动画
        private GameObject train, goTrain, freeCloude, goFreeCloude;
        private Animator trainAnim;

        //免费游戏和正常游戏直接的动效
        private Transition BsToFsTrans, FsToBsTrans, JsToBsTrans;
        //免费游戏中充能绿条
        private GImage fill1, fill2, fill3, fill4;
        //免费游戏的剩余次数和总次数
        private GTextField freeTimes, freeTotalTimes;

        //免费游戏火车
        private GameObject freeTrainPref, freeTrainObj;
        private GComponent freeAnchor;
        private Animator freeTrainAnim;
        private Transform idleEffect1, idleEffect2, idleEffect3, idleEffect4, idleEffect5;

        /// <summary>暂时关闭免费火车 idleEffect1~5 粒子；需恢复时改为 false。</summary>
        private const bool TempDisableFreeTrainIdleParticles = false;

        //游戏中的女生
        private GameObject girlPref, girlObj;
        private GComponent anchorGirl;
        private Animator girlAnim;


        //用于记录未中奖的次数
        private int noWinTimes = 0;
        //本局游戏中是否存在中奖
        private bool isWin = false;
        //当前游戏触发加速框后是否中奖
        private bool isTriggerFrame = false;
        private bool isWinFreeOrJacpot = false;

        // 开始游戏
        private bool _tipCoinIn = false, _isStoppedSlotMachine = false;
        private bool _isStopButtonLocked;

        MiniReelGroup uiJPMajorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMinorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMiniCtrl = new MiniReelGroup();

        /// <summary>
        /// 彩金游戏当中判断是否前面已经出现过彩金图标判断是否需要播放鼓掌动画
        /// </summary>
        private bool showClawAnim = false;

        private bool isConnectFreeSpin = false;

        private GameSoundController3996 _gameSoundController;

        bool isAddCreditAnim => !(slotMachineCtrl.isStopImmediately == true || SBoxModel.Instance.isCoinOutImmediately);
        private EventData _data = null;

        /// <summary>3996：底部 Panel 异步就绪后触发 PageManager 的 preLoadedCallback。</summary>
        private void OnBottomPanelReadyForPreload(EventData res)
        {
            if (res == null || res.name != PanelEvent.BottomPanelReady)
            {
                return;
            }

            int gameId = Convert.ToInt32(res.value);
            if (gameId != 3996)
            {
                return;
            }

            Debug.LogError("BottomPanelReadyForPreload:"+ gameId);
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_EVENT, OnBottomPanelReadyForPreload);
            preLoadedCallback?.Invoke();
        }

        const string CACHE_TOTAL_JP_MAJOR_CONTRIBUTION = "CACHE_TOTAL_JP_MAJOR_CONTRIBUTION";
        const string CACHE_TOTAL_JP_GRAND_CONTRIBUTION = "CACHE_TOTAL_JP_GRAND_CONTRIBUTION";

        protected override void OnInit()
        {
            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            int count = 11;

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
            "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/Game Controller/Slot Game Main ControllerClone.prefab",
            (GameObject clone) =>
            {
                if (goGameCtrl != null) //防止重复加载
                {
                    // 仍须计入 callback，否则异步重复回调会导致 count 无法归零、InitParam 永不执行
                    callback();
                    return;
                }
                goGameCtrl = GameObject.Instantiate(clone);
                goGameCtrl.name = "Slot Game Main Controller3996";
                goGameCtrl.transform.SetParent(null);
                //获取组件引用
                slotMachineCtrl = goGameCtrl.transform.Find("Slot Machine").GetComponent<SlotMachineController3996>();
                mono = goGameCtrl.transform.GetComponent<MonoHelper>();

                fguiPoolHelper = goGameCtrl.transform.Find("Pool").GetComponent<FguiPoolHelper>();
                gObjectPoolHelper = goGameCtrl.transform.Find("GObject Pool").GetComponent<FguiGObjectPoolHelper>();
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/PageGameMain/Train.prefab",
            (GameObject clone) =>
            {
                goTrain = clone;
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/PopupGameFree/FreeGameCloude.prefab",
            (GameObject clone) =>
            {
                goFreeCloude = clone;
                callback();
            });

            ResourceManager02.Instance.LoadAssetBundleAsync("Assets/GameRes/Games/Emperors Rein 200/FGUIs", (ab) =>
            {
                UIPackage.AddPackage(ab);
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/Effects/FreeReelEffect.prefab",
            (GameObject clone) =>
            {
                goFreeReelEffcet = clone;
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/Effects/JackpotReelEffect.prefab",
            (GameObject clone) =>
            {
                goJackpotReelEffect = clone;
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/Effects/RewardEffect.prefab",
            (GameObject clone) =>
            {
                goRewardEffect = clone;
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/PageGameMain/TransEffect.prefab",
            (GameObject clone) =>
            {
                goOpenEffect = clone;
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/PageGameMain/Girl.prefab",
            (GameObject clone) =>
            {
                girlPref = clone;
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
           "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/PopupGameFree/FreeGameTrain.prefab",
           (GameObject clone) =>
           {
               freeTrainPref = clone;
               callback();
           });

            ResourceManager02.Instance.LoadAsset<GameObject>(
           "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/PopupGameJackpot/jackpotSpine.prefab",
           (GameObject clone) =>
           {
               _jackpotHitObj = clone;
               callback();
           });


            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        if (PanelBaseController.ShouldBlockPhysicalSpinInput)
                        {
                            return;
                        }

                        if (!isReady)
                        {
                            return;
                        }

                        Debug.LogError("游戏接受到机台短按的数据：Spin");
                        EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, false); // isLongClick
                        OnClickSpinButton(res);
                    },
                },

                longClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        if (PanelBaseController.ShouldBlockPhysicalSpinInput)
                        {
                            return;
                        }

                        if (!isReady)
                        {
                            return;
                        }

                        DebugUtils.LogError("游戏接受到机台长按的数据：Spin");
                        EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, true); // isLongClick
                        CommonPopupHandler.Instance.ClosePopup();
                        OnClickSpinButton(res);
                    }
                }

            };
        }

        public override void OnOpen(PageName name, EventData data)
        {
            if (isOpen) return;

            base.OnOpen(name, data);
            EventCenter.Instance.AddEventListener<CoinPushSpinParseEventArgs>(SBoxEventHandle.SBOX_COIN_PUSH_SPIN_PARSE, OnCoinPushSpinResultParse);
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnClickSpinButton);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnStopSlot);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT, OnSlotDetailEvent);
            EventCenter.Instance.AddEventListener<EventData>("RewardAddEffect", OnRewardEffectEvent);
            EventCenter.Instance.AddEventListener<EventData>("JackpotWinCredit", OnJackpotWinEvent);
            EventCenter.Instance.AddEventListener<EventData>("PlayGirlClaw", OnPlayGirlClaw);
            InitParam(null);
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT, new EventData(Game3996AudioEvent.BgmRegularGame));
            PlayAnim(trainAnim, "fg_ng");
        }

        public override void OnClose(EventData data = null)
        {
            slotMachineCtrl.SkipWinLine(true);
            OnGameReset();
            UnlockStopButton();
            EventCenter.Instance.RemoveEventListener<CoinPushSpinParseEventArgs>(SBoxEventHandle.SBOX_COIN_PUSH_SPIN_PARSE, OnCoinPushSpinResultParse);
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnClickSpinButton);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnStopSlot);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT, OnSlotDetailEvent);
            EventCenter.Instance.RemoveEventListener<EventData>("RewardAddEffect", OnRewardEffectEvent);
            EventCenter.Instance.RemoveEventListener<EventData>("JackpotWinCredit", OnJackpotWinEvent);
            EventCenter.Instance.RemoveEventListener<EventData>("PlayGirlClaw", OnPlayGirlClaw);
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_EVENT, OnBottomPanelReadyForPreload);

            _gameSoundController?.Dispose();
            _gameSoundController = null;

            base.OnClose(data);
        }

        protected override void OnLanguageChange(I18nLang lang)
        {
            FguiI18nTextAssistant.Instance.DisposeAllTranslate(contentPane);
            contentPane.Dispose(); // 释放当前UI
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            InitParam();
        }


        private void OnClickSpinButton(EventData res)
        {
            if (res.name == PanelEvent.SpinButtonClick)
            {
                bool isLongClick = (bool)res.value;
                switch (ContentModel.Instance.btnSpinState)
                {
                    case SpinButtonState.Stop:
                        {
                            if (ContentModel.Instance.isSpin) return; // 已经开始玩直接退出
                            UnlockStopButton();
                            ContentModel.Instance.isSpin = true;

                            Action successCallback = () =>
                            {
                                DebugUtils.Log("游戏结束");
                                UnlockStopButton();
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
                                StartGameOnce(successCallback, StopGameWhenError); //开始玩
                            }


                        }
                        break;

                    case SpinButtonState.Spin:
                        {
                            // 已经在游戏时，去停止游戏
                            if (!ContentModel.Instance.isSpin) return; // 已经停止直接退出
                            LockStopButton();
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

            if (res.name == "ColUpButtonClick")
            {
                int col = (int)res.value;
                if (corGameIdel != null) mono.StopCoroutine(corGameIdel);
                mono.StartCoroutine(slotMachineCtrl.NudgeReelOneStep(col, null, false, ReelNudgeDirection.Up));
            }

            if (res.name == "ColDownButtonClick")
            {
                int col = (int)res.value;
                if (corGameIdel != null) mono.StopCoroutine(corGameIdel);
                mono.StartCoroutine(slotMachineCtrl.NudgeReelOneStep(col, null, false, ReelNudgeDirection.Down));
            }

        }


        public void InitParam(EventData data)
        {
            if (data != null) _data = data;
            if (!isInit) return;
            
            // ---------- 1. MainModel、Paytable、本地 JSON ----------
            MainModel.Instance.contentMD = ContentModel.Instance;
            MainModel.Instance.cutomMD = CustomModel.Instance;
            MainModel.Instance.lineNum = 30;
            MainModel.Instance.gameID = 3996;
            MainModel.Instance.gameName = "CaiFuHuoChe3996";
            MainModel.Instance.displayName = "CaiFuHuoChe_3996";
            ContentModel.Instance.totalBet = SBoxModel.Instance.betList[ContentModel.Instance.betIndex];
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

            // ---------- 2. FGUI 对象池（须先于滚轮 Init） ----------
            if (fguiPoolHelper != null && isInitPool == false)
            {
                isInitPool = true;
                fguiPoolHelper.Add(TagPoolObject.SymbolHit, CustomModel.Instance.symbolHitEffect.Values.ToList(), "symbol_hit#", 5);
                fguiPoolHelper.PreLoad(TagPoolObject.SymbolHit);
                fguiPoolHelper.Add(TagPoolObject.SymbolBorder, CustomModel.Instance.borderEffect, "border#", 5);
                fguiPoolHelper.PreLoad(TagPoolObject.SymbolBorder);
                fguiPoolHelper.Add(TagPoolObject.SymbolAppear, CustomModel.Instance.symbolAppearEffect.Values.ToList(), "symbol_appear#", 5);
                fguiPoolHelper.PreLoad(TagPoolObject.SymbolAppear);
            }

            // ---------- 3.滚轮控制器 ----------
            GComponent gSlotMachine = contentPane.GetChild("slotMachine").asCom;
            GComponent gReels = gSlotMachine.GetChild("reels").asCom;
            slotCover = gSlotMachine.asCom.GetChild("slotCover").asCom;
            gPlayLines = gSlotMachine.asCom.GetChild("playLines").asCom;
            gFrame = contentPane.GetChild("anchorFrame").asCom;
            slotMachineCtrl.Init(slotCover, gPlayLines, gReels, gFrame, fguiPoolHelper, gObjectPoolHelper);

            // ---------- 4. 底部菜单 Panel ----------
            //初始化菜单ui
            gOwnerPanel = this.contentPane.GetChild("panel").asCom;
            ContentModel.Instance.goAnthorPanel = gOwnerPanel;
            MainModel.Instance.contentMD.goAnthorPanel = gOwnerPanel;
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_EVENT, OnBottomPanelReadyForPreload);
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_EVENT, OnBottomPanelReadyForPreload);
            EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT, new EventData<GComponent>(PanelEvent.AnchorPanelChange, gOwnerPanel));
           
            if (!isOpen) return;

            // ---------- 5.音乐控制 ----------
            _gameSoundController = new GameSoundController3996();

            // ---------- 6.初始化FGUI组件 ----------
            BsToFsTrans = contentPane.GetTransition("BSToFSTransform");
            FsToBsTrans = contentPane.GetTransition("FSToBSTransform");
            JsToBsTrans = contentPane.GetTransition("JSToBSTransform");
            smallGameReels = contentPane.GetChild("smallGameReels").asCom;
            fill1 = contentPane.GetChild("fill1").asImage;
            fill2 = contentPane.GetChild("fill2").asImage;
            fill3 = contentPane.GetChild("fill3").asImage;
            fill4 = contentPane.GetChild("fill4").asImage;
            freeTimes = contentPane.GetChild("freeRemainTimes").asTextField;
            freeTotalTimes = contentPane.GetChild("freeTotalTimes").asTextField;

            uiJPMajorCtrl.Init("Major", this.contentPane.GetChild("major").asCom.GetChild("reels").asList, "N0");
            uiJPMinorCtrl.Init("Minor", this.contentPane.GetChild("minor").asCom.GetChild("reels").asList, "N0");
            uiJPMiniCtrl.Init("Mini", this.contentPane.GetChild("mini").asCom.GetChild("reels").asList, "N0");
            ERPushMachineDataManager02.Instance.RequestGetJpContribution((res) =>
            {
                JSONNode data = JSONNode.Parse((string)res);
                int code = (int)data["code"];
                if (0 != code)
                {
                    DebugUtils.LogError($"请求贡献值报错。 code: {code}");
                    return;
                }
                uiJPMajorCtrl.SetData((int)data["major"]);
                uiJPMinorCtrl.SetData((int)data["minor"]);
                uiJPMiniCtrl.SetData((int)data["mini"]);
            });

            // ---------- 7.预制体挂到 FGUI 锚点 ----------
            if (ComReelEffect2 != null) ComReelEffect2.Dispose();
            if (ComReelEffect3 != null) ComReelEffect3.Dispose();

            ComReelEffect2 = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            GameCommon.FguiUtils.DeleteWrapper(ComReelEffect2);
            GameCommon.FguiUtils.AddWrapper(ComReelEffect2, GameObject.Instantiate(goFreeReelEffcet));
            ComReelEffect2.visible = false;

            ComReelEffect3 = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            GameCommon.FguiUtils.DeleteWrapper(ComReelEffect3);
            GameCommon.FguiUtils.AddWrapper(ComReelEffect3, GameObject.Instantiate(goJackpotReelEffect));
            ComReelEffect3.visible = false;

            anchorExpectation = this.contentPane.GetChild("anchorReelEffect").asCom;
            anchorExpectation.AddChild(ComReelEffect2);
            anchorExpectation.AddChild(ComReelEffect3);
            anchorExpectation.visible = true;

            ComRewardEffect1 = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            ComRewardEffect2 = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            ComRewardEffect3 = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            GameCommon.FguiUtils.DeleteWrapper(ComRewardEffect1);
            GameCommon.FguiUtils.DeleteWrapper(ComRewardEffect2);
            GameCommon.FguiUtils.DeleteWrapper(ComRewardEffect3);
            GameCommon.FguiUtils.AddWrapper(ComRewardEffect1, GameObject.Instantiate(goRewardEffect));
            GameCommon.FguiUtils.AddWrapper(ComRewardEffect2, GameObject.Instantiate(goRewardEffect));
            GameCommon.FguiUtils.AddWrapper(ComRewardEffect3, GameObject.Instantiate(goRewardEffect));
            ComRewardEffect1.visible = false;
            ComRewardEffect2.visible = false;
            ComRewardEffect3.visible = false;
            anchorFreeAdd = contentPane.GetChild("freeAddPoint").asCom;
            anchorJackpotAdd = contentPane.GetChild("jackpotAddPoint").asCom;
            anchorFill1 = contentPane.GetChild("fill1Add").asCom;
            anchorFill2 = contentPane.GetChild("fill2Add").asCom;
            anchorFill3 = contentPane.GetChild("fill3Add").asCom;
            anchorFill4 = contentPane.GetChild("fill4Add").asCom;
            anchorFreeAdd.AddChild(ComRewardEffect1);
            anchorFreeAdd.AddChild(ComRewardEffect2);
            anchorFreeAdd.AddChild(ComRewardEffect3);
            anchorFreeAdd.visible = true;


            GComponent loadTrain = contentPane.GetChild("anchorTrain").asCom;
            if (gTrain != loadTrain)
            {
                GameCommon.FguiUtils.DeleteWrapper(gTrain);
                gTrain = loadTrain;
                train = GameObject.Instantiate(goTrain);
                trainAnim = train.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                GameCommon.FguiUtils.AddWrapper(gTrain, train);
            }

            GComponent loadFreeTrain = contentPane.GetChild("anchorFreeTrain").asCom;
            if (freeAnchor != loadFreeTrain)
            {
                GameCommon.FguiUtils.DeleteWrapper(freeAnchor);
                freeAnchor = loadFreeTrain;
                freeTrainObj = GameObject.Instantiate(freeTrainPref);
                freeTrainAnim = freeTrainObj.transform.GetChild(1).GetChild(0).GetComponent<Animator>();
                idleEffect1 = freeTrainObj.transform.GetChild(0).GetChild(0);
                idleEffect2 = freeTrainObj.transform.GetChild(0).GetChild(1);
                idleEffect3 = freeTrainObj.transform.GetChild(0).GetChild(2);
                idleEffect4 = freeTrainObj.transform.GetChild(0).GetChild(3);
                idleEffect5 = freeTrainObj.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(6).GetChild(0);
                GameCommon.FguiUtils.AddWrapper(freeAnchor, freeTrainObj);
            }

            GComponent loadGirl = contentPane.GetChild("anchorGirl").asCom;
            if (anchorGirl != loadGirl)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorGirl);
                anchorGirl = loadGirl;
                girlObj = GameObject.Instantiate(girlPref);
                girlAnim = girlObj.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                GameCommon.FguiUtils.AddWrapper(anchorGirl, girlObj);
                PlayAnim(girlAnim, "ng_idle1");
            }

            GComponent loadOpenEffect = contentPane.GetChild("JpEffect").asCom;
            if (anchorOpenEffect != loadOpenEffect)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorOpenEffect);
                anchorOpenEffect = loadOpenEffect;
                GameObject temp = GameObject.Instantiate(goOpenEffect);
                fgToNor = temp.transform.GetChild(0).GetChild(0);
                GameCommon.FguiUtils.AddWrapper(anchorOpenEffect, temp);
            }

            GComponent loadFreeCloude = contentPane.GetChild("anchorFreeCloude").asCom;
            if (gFreeCloude != loadFreeCloude)
            {
                GameCommon.FguiUtils.DeleteWrapper(gFreeCloude);
                gFreeCloude = loadFreeCloude;
                freeCloude = GameObject.Instantiate(goFreeCloude);
                GameCommon.FguiUtils.AddWrapper(gFreeCloude, freeCloude);
            }


            // ---------- 8.断电数据恢复 ----------
            TryRestoreFreeSpinSession();
            isReady = true;
        }

        void StartGameOnce(Action successCallback = null, Action<string> errorCallback = null)
        {
            ContentModel.Instance.totalPlaySpins = 1;
            ContentModel.Instance.remainPlaySpins = 1;
            if (corGameOnce != null) mono.StopCoroutine(corGameOnce);
            corGameOnce = mono.StartCoroutine(GameOnce(successCallback, errorCallback));
        }

        void StartGameAuto(Action successCallback = null, Action<string> errorCallback = null)
        {
            mono.StartCoroutine(GameAuto(successCallback, errorCallback));
        }

        void OnStopSlot(EventData res)
        {
            switch (res.name)
            {
                case SlotMachineEvent.StoppedSlotMachine:
                    {
                        isStoppedSlotMachine = true;
                        UnlockStopButton();
                    }
                    break;
            }
        }

        void OnSlotDetailEvent(EventData res)
        {
            switch (res.name)
            {
                case SlotMachineEvent.PrepareStoppedReel:
                    {
                        if (!slotMachineCtrl.isStopImmediately)
                        {
                            int colIndex = (int)res.value;
                            if (colIndex >= 0 && colIndex < slotMachineCtrl.column)
                            {
                                if (corEffectSlowMotion != null) mono.StopCoroutine(corEffectSlowMotion);
                                corEffectSlowMotion = mono.StartCoroutine(ShowEffectReelsSlowMotion(colIndex));
                            }
                        }
                    }
                    break;
            }

        }

        Dictionary<int, List<int>> tempResult;
        void OnRewardEffectEvent(EventData res)
        {
            tempResult = (Dictionary<int, List<int>>)res.value;
            int col = tempResult.Keys.First();

            //如果有需要跳过不播放中奖的线条特效可启用
            //if (slotMachineCtrl.isStopImmediately && res.name == "MultRewardEffect")
            //{
            //    SkipAddMult(tempResult[col].Count * 0.25f);
            //    return;
            //}

            switch (res.name)
            {
                case "FreeRewardEffect":
                    foreach (int row in tempResult[col])
                    {
                        mono.StartCoroutine(ShowRewardEffect(col, row, anchorFreeAdd, () =>
                        {
                            #region 免费游戏中，添加额外免费游戏

                            slotMachineCtrl.BeginBonusFreeSpinAdd();

                            // 【待修改】重置剩余的局数 
                            ContentModel.Instance.showFreeSpinRemainTime =
                                ContentModel.Instance.freeSpinTotalTimes - ContentModel.Instance.freeSpinPlayTimes;

                            ContentModel.Instance.freeSpinTotalTimes++;
                            if (ContentModel.Instance.nextReelStripsIndex == "BS")
                            {
                                ContentModel.Instance.nextReelStripsIndex = "FS";
                                ContentModel.Instance.isFreeSpinResult = false;
                            }

                            freeTotalTimes.text = ContentModel.Instance.freeSpinTotalTimes.ToString();

                            slotMachineCtrl.EndBonusFreeSpinAdd();

                            #endregion
                        }));
                    }
                    break;
                case "MultRewardEffect":
                    foreach (int row in tempResult[col])
                    {
                        if (fill1.fillAmount != 1)
                        {
                            mono.StartCoroutine(ShowRewardEffect(col, row, anchorFill1));
                        }
                        else if (fill2.fillAmount != 1)
                        {
                            mono.StartCoroutine(ShowRewardEffect(col, row, anchorFill2));
                        }
                        else if (fill3.fillAmount != 1)
                        {
                            mono.StartCoroutine(ShowRewardEffect(col, row, anchorFill3));
                        }
                        else if (fill4.fillAmount != 1)
                        {
                            mono.StartCoroutine(ShowRewardEffect(col, row, anchorFill4));
                        }
                        SkipAddMult(0.25f);
                    }
                    break;
            }
        }


        void OnJackpotWinEvent(EventData res)
        {
            Dictionary<int, int> tempPos = (Dictionary<int, int>)res.value;
            mono.StartCoroutine(ShowRewardEffect(tempPos.Keys.First(), tempPos.Values.First(), anchorJackpotAdd, null, true));
        }


        void SkipAddMult(float value)
        {
            float temp = 0;
            if (fill1.fillAmount != 1)
            {
                if (fill1.fillAmount + value <= 0.95f)
                {
                    fill1.fillAmount += value;
                    EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                        new EventData<int>(Game3996AudioEvent.FreeSpinMeterSound, 0));
                }
                else
                {
                    temp = fill1.fillAmount;
                    fill1.fillAmount = 1;
                    EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                        new EventData<int>(Game3996AudioEvent.FreeSpinMeterSound, 1));
                    SetFreeTrainState();
                    if (value + temp - 1 > 0) SkipAddMult(value + temp - 1);
                }
            }
            else if (fill2.fillAmount != 1)
            {
                if (fill2.fillAmount + value <= 0.95f)
                {
                    fill2.fillAmount += value;
                    EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                        new EventData<int>(Game3996AudioEvent.FreeSpinMeterSound, 0));
                }
                else
                {
                    temp = fill2.fillAmount;
                    fill2.fillAmount = 1;
                    EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                        new EventData<int>(Game3996AudioEvent.FreeSpinMeterSound, 1));
                    SetFreeTrainState();
                    PlayAnim(freeTrainAnim, "win2");
                    if (value + temp - 1 > 0) SkipAddMult(value + temp - 1);
                }
            }
            else if (fill3.fillAmount != 1)
            {
                if (fill3.fillAmount + value <= 0.95f)
                {
                    fill3.fillAmount += value;
                    EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                        new EventData<int>(Game3996AudioEvent.FreeSpinMeterSound, 0));
                }
                else
                {
                    temp = fill3.fillAmount;
                    fill3.fillAmount = 1;
                    EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                        new EventData<int>(Game3996AudioEvent.FreeSpinMeterSound, 1));
                    SetFreeTrainState();
                    PlayAnim(freeTrainAnim, "win");
                    if (value + temp - 1 > 0) SkipAddMult(value + temp - 1);
                }
            }
            else if (fill4.fillAmount != 1)
            {
                if (fill4.fillAmount + value <= 0.95f)
                {
                    fill4.fillAmount += value;
                    EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                        new EventData<int>(Game3996AudioEvent.FreeSpinMeterSound, 0));
                }
                else
                {
                    fill4.fillAmount = 1;
                    EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                        new EventData<int>(Game3996AudioEvent.FreeSpinMeterSound, 1));
                    EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                        new EventData<int>(Game3996AudioEvent.FreeSpinMeterSound, 2));
                }
            }
        }

        private IEnumerator GameOnce(Action successCallback, Action<string> errorCallback)
        {
            if (!SBoxModel.Instance.isMachineActive)
            {
                errorCallback?.Invoke("<size=24>Machine not activated!</size>");
                yield break;
            }

            if (ContentModel.Instance.freeSpinTotalTimes > 0 && ContentModel.Instance.nextReelStripsIndex == "FS")
            {
                yield return GameFreeSpinFromReconnect(successCallback, errorCallback);
                yield break;
            }

            if (SBoxModel.Instance.myCredit < ContentModel.Instance.totalBet)
            {
                tipCoinIn = true;
                errorCallback?.Invoke("<size=15>Balance is insufficient, please recharge first</size>");
                yield break;
            }

            //test 检查算法积分
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
            isWin = false;
            isWinFreeOrJacpot = false;
            isTriggerFrame = false;
            string errMsg = "";


            //展会模式
            if (ApplicationSettings.Instance.IsExpoMode() && MainModel.Instance.isExhibitionModeMode)
            {
                string currentDeck = GetCurrentVisibleDeckRowCol();
                if (!string.IsNullOrEmpty(currentDeck))
                {
                    try
                    {
                        int[] deckData = SlotTool.GetDeckRowCol(currentDeck).ToArray();
                        SBoxExhibitionData sBoxExhibitionData = new SBoxExhibitionData
                        {
                            wheelChessNum = deckData.Length,
                            data = deckData
                        };
                        SBoxIdea.SetExhibitionData(sBoxExhibitionData);
                    }
                    catch (Exception e)
                    {
                        DebugUtils.LogError($"[G3996] 设置展会模式结果失败，deck={currentDeck}");
                        DebugUtils.LogException(e);
                    }
                }
            }

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
                //if (corEffectSlowMotion != null) mono.StopCoroutine(corEffectSlowMotion);
                //corEffectSlowMotion = mono.StartCoroutine(ShowEffectReelsSlowMotion());
                slotMachineCtrl.ShowSymbolAppearEffectAfterReelStop(true);
            }
            else
            {
                slotMachineCtrl.ShowSymbolAppearEffectAfterReelStop(ContentModel.Instance.winList.Count == 0);
            }

            // 立即停止或正常旋转
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
            //普通赢
            if (winList.Count > 0)
            {
                isWin = true;
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
                    slotMachineCtrl.SkipWinLine(true);
                    yield return slotMachineCtrl.ShowSymbolWinBySetting(slotMachineCtrl.GetTotalSymbolWin(winList), true, PusherEmperorsRein.SpinWinEvent.TotalWinLine);
                }

                ////检查bigwin类型
                WinLevelType winLevelType = GetBigWinType();
                ////bigwin弹窗
                if (winLevelType != WinLevelType.None)
                {
                    // BigWin 播放期间停掉滚轴上的中奖图标/线，结束后再恢复
                    yield return BigWinPopup(winLevelType, ContentModel.Instance.baseGameWinCredit);

                    slotMachineCtrl.ShowSymbolWinDeck(slotMachineCtrl.GetTotalSymbolWin(winList), true);
                }
                else
                {
                    // 普通赢钱处理
                    bool isAddToCredit = totalWinLineCredit > ContentModel.Instance.totalBet * 4;
                    //积分同步和退币处理
                    slotMachineCtrl.SendPrepareTotalWinCreditEvent(totalWinLineCredit, isAddToCredit);
                }

                //积分同步和退币处理
                slotMachineCtrl.SendTotalWinCreditEvent(allWinCredit);
                //加钱动画
                MainBlackboardController.Instance.AddMyTempCredit(allWinCredit, true, isAddCreditAnim);
            }
            #endregion

            // 即中即退
            // yield return CoinOutImmediately(allWinCredit);

            //免费奖
            if (ContentModel.Instance.isFreeSpinTrigger)
            {
                isWin = false;
                isWinFreeOrJacpot = true;
                if (winList.Count > 0)
                {
                    // 本剧同步玩家金钱
                    MainBlackboardController.Instance.SyncMyTempCreditToReal(true);
                    yield return new WaitForSeconds(1);
                }

                //显示中奖动画
                slotMachineCtrl.SkipWinLine(true);
                slotMachineCtrl.ShowSymbolEffect(TagPoolObject.SymbolHit, new List<int>() { 10 }, true, 10, true);
                yield return slotMachineCtrl.SlotWaitForSeconds(1f);

                slotMachineCtrl.SkipWinLine(true);
                PlayAnim(girlAnim, "ng_trigger_fg");
                yield return new WaitForSeconds(5.5f);

                isNext = false;
                slotMachineCtrl.SkipWinLine(true);
                yield return FreeSpinTrigger(() => isNext = true, errorCallback);

                yield return new WaitUntil(() => isNext == true);
                isNext = false;
                ContentModel.Instance.nextReelStripsIndex = "BS";
            }

            //中游戏大奖
            if (ContentModel.Instance.isJackpotSpinTrigger)
            {
                isWin = true;
                isWinFreeOrJacpot = true;
                if (winList.Count > 0)
                {
                    yield return new WaitForSeconds(1);
                }
                isNext = false;

                //显示中奖动画
                slotMachineCtrl.SkipWinLine(true);
                slotMachineCtrl.ShowSymbolEffect(TagPoolObject.SymbolHit, new List<int>() { 11 }, true, 11, true);
                yield return slotMachineCtrl.SlotWaitForSeconds(1.5f);

                //播放动画
                slotMachineCtrl.SkipWinLine(true);
                PlayAnim(girlAnim, "ng_trigger_sg");
                yield return new WaitForSeconds(1.8f);

                //yield return jackpotSpinTrigger(() => isNext = true, errorCallback);
                yield return SmallGameTrigger(() => isNext = true);

                yield return new WaitUntil(() => isNext == true);
                isNext = false;
            }

            if (isTriggerFrame && !isWinFreeOrJacpot)
            {
                PlayAnim(girlAnim, "ng_not triggered");
            }

            if (!isWin)
            {
                noWinTimes++;
                if (noWinTimes >= 5)
                {
                    noWinTimes = 0;
                    PlayAnim(girlAnim, "ng_not win");
                }
            }
            else
            {
                noWinTimes = 0;
            }

            // 本剧同步玩家金钱
            MainBlackboardController.Instance.SyncMyTempCreditToReal(true);

            // 进入空闲模式
            ContentModel.Instance.gameState = GameState.Idle;
            slotMachineCtrl.SkipWinLine(true);
            if (winList.Count > 0 && !ContentModel.Instance.isAuto) // && !ContentModel.Instance.isFreeSpinTrigger
            {
                if (corGameIdel != null) mono.StopCoroutine(corGameIdel);
                corGameIdel = mono.StartCoroutine(GameIdle(winList));
            }

            slotMachineCtrl.isStopImmediately = false;

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

        //彩金游戏进入和退出
        IEnumerator jackpotSpinTrigger(Action successCallback, Action<string> errorCallback)
        {
            ContentModel.Instance.jackpotSpinWinCredit = 0;
            allWinCredit = 0;
            slotMachineCtrl.BeginBonusFreeSpin();
            bool isNext = false;
            PageManager.Instance.OpenPageAsync(PageName.CaiFuHuoChePopupJackpotGameTrigger,
                new EventData<Dictionary<string, object>>("", new Dictionary<string, object>()
                {
                    ["SpinTimes"] = 3,
                }),
            (ed) =>
            {
                Debug.Log("回调执行！isNext = true"); // 加日志
                isNext = true;
            });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(SlotMachineEvent.BonusGameFadeTransition));
            yield return new WaitForSeconds(0.9f);

            train.SetActive(false);
            yield return new WaitForSeconds(0.3f);

            ChangeBGPanel(2);
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(Game3996AudioEvent.BgmBonusGame));
            PlayAnim(girlAnim, "sg_idle1");
            freeTotalTimes.text = ContentModel.Instance.jackpotSpinTotalTimes.ToString();
            freeTimes.text = ContentModel.Instance.jackpotSpinTotalTimes.ToString();
            yield return new WaitForSeconds(0.4f);

            yield return new WaitForSeconds(0.2f);
            //PlayEffectAnim(startEffect);

            yield return GameJackpotSpin(null, errorCallback);

            yield return slotMachineCtrl.JackpotWinCredit(() => isNext = true);

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            slotMachineCtrl.SkipIdle(true);
            slotMachineCtrl.SkipWinLine(true);

            PlayAnim(girlAnim, "sg_settlement");
            yield return new WaitForSeconds(2);


            //StopEffectAnim(boxIdleEffect);
            yield return new WaitForSeconds(2.5f);

            PageManager.Instance.OpenPageAsync(PageName.CaiFuHuoChePopupJackpotGameExit,
                new EventData<Dictionary<string, object>>("", new Dictionary<string, object>()
                {
                    ["winCredit"] = allWinCredit,
                }),
            (ed) =>
            {
                Debug.Log("回调执行！isNext = true"); // 加日志
                isNext = true;
            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;
            slotMachineCtrl.EndBonusFreeSpin();
            //加钱动画
            MainBlackboardController.Instance.AddMyTempCredit(allWinCredit, true, isAddCreditAnim);

            ChangeBGPanel(0);
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(Game3996AudioEvent.BgmRegularGame));
            train.SetActive(true);
            JsToBsTrans.Play();
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(SlotMachineEvent.BonusGameFadeTransition));

            successCallback?.Invoke();
        }

        //开始彩金游戏
        IEnumerator GameJackpotSpin(Action successCallback, Action<string> errorCallback)
        {
            while (ContentModel.Instance.nextReelStripsIndex == "JS")
            {
                yield return slotMachineCtrl.SlotWaitForSeconds(1);
                yield return GameJackpotSpinOnce(null, errorCallback);
            }

            if (successCallback != null)
                successCallback.Invoke();
        }

        //一局彩金游戏
        IEnumerator GameJackpotSpinOnce(Action successCallback, Action<string> errorCallback)
        {
            OnGameReset();

            ContentModel.Instance.haveJackpotCredit = false;
            ContentModel.Instance.gameState = GameState.FreeSpin;
            showClawAnim = false;

            bool isNext = false;
            bool isBreak = false;
            string errMsg = "";

            //获取结果
            if (ApplicationSettings.Instance.isMock)
            {
                yield return JackpotRequestSlotSpinFromMock(() =>
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
                yield return JackpotRequestSlotSpinFromMock(() =>
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

            freeTimes.text = (ContentModel.Instance.jackpotSpinTotalTimes - ContentModel.Instance.jackpotSpinPlayTimes).ToString();

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

            if (ContentModel.Instance.haveJackpotCredit)
            {
                freeTimes.text = ContentModel.Instance.jackpotSpinTotalTimes.ToString();
                ContentModel.Instance.jackpotSpinPlayTimes = 0;
            }
        }


        int freeAllWin = 0;
        IEnumerator FreeSpinTrigger(Action successCallback, Action<string> errorCallback)
        {
            bool isNext = false;
            if (!isConnectFreeSpin)
            {
                PageManager.Instance.OpenPageAsync(PageName.CaiFuHuoChePopupFreeSpinTrigger,
                new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object>()
                    {
                        ["freeSpinCount"] = ContentModel.Instance.freeSpinTotalTimes,
                    }),
                (ed) =>
                {
                    Debug.Log("回调执行！isNext = true"); // 加日志
                    isNext = true;
                });

                yield return new WaitUntil(() => isNext == true);
                isNext = false;

                slotMachineCtrl.SkipWinLine(false);
                slotMachineCtrl.CloseSlotCover();

                FreeGameReset();
                PlayAnim(trainAnim, "ng_fg");
                BsToFsTrans.Play();
                EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                    new EventData(SlotMachineEvent.FreeGameFadeTransition));

                yield return new WaitForSeconds(1.3f);
                ChangeBGPanel(1);
            }
            else
            {
                isConnectFreeSpin = false;
            }

            SetFreeTrainState();

            InputStackContextFreeSpin((context) =>
            {
            });


            slotMachineCtrl.BeginBonusFreeSpin();

            yield return GameFreeSpin(null, errorCallback);

            OnGameReset();
            StopAllFreeTrainIdleEffects();
            PageManager.Instance.OpenPageAsync(PageName.CaiFuHuoChePopupFreeSpinResult,
                new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object>()
                    {
                        ["baseGameWinCredit"] = ContentModel.Instance.freeSpinTotalWinCredit,
                    }),
                (ed) =>
                {
                    DebugUtils.Log("回调执行！isNext = true"); // 加日志
                    isNext = true;
                    ContentModel.Instance.curFreeMult = 1;
                    ContentModel.Instance.nextReelStripsIndex = "BS";
                });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            PlayEffectAnim(fgToNor);
            yield return new WaitForSeconds(0.7f);

            StopAllFreeTrainIdleEffects();
            gTrain.visible = true;
            FsToBsTrans.Play();
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(SlotMachineEvent.FreeGameFadeTransition));

            yield return new WaitForSeconds(0.5f);
            PlayAnim(trainAnim, "fg_ng");

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

            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(Game3996AudioEvent.BgmRegularGame));
            ChangeBGPanel(0);
            ContentModel.Instance.nextReelStripsIndex = "BS";

            yield return new WaitForSeconds(1);

            slotMachineCtrl.SkipWinLine(true);
            successCallback?.Invoke();
        }


        //开始免费游戏
        IEnumerator GameFreeSpin(Action successCallback, Action<string> errorCallback)
        {
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(Game3996AudioEvent.BgmFreeSpinGame));
            while (ContentModel.Instance.nextReelStripsIndex == "FS")
            {
                yield return GameFreeSpinOnce(null, errorCallback);
                yield return slotMachineCtrl.SlotWaitForSeconds(1f);
            }

            if (successCallback != null)
                successCallback.Invoke();
        }


        //一局免费游戏
        IEnumerator GameFreeSpinOnce(Action successCallback, Action<string> errorCallback)
        {
            OnGameReset();
            ContentModel.Instance.haveFreeSpecialIcon = false;
            freeTimes.text = (ContentModel.Instance.freeSpinPlayTimes + 1).ToString();
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

            //停止特效显示
            slotMachineCtrl.SkipWinLine(true);
            slotMachineCtrl.CloseSlotCover();

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

            if (ContentModel.Instance.haveFreeSpecialIcon)
            {
                yield return new WaitForSeconds(1f);
            }

            List<SymbolWin> winList = ContentModel.Instance.winList;
            #region Win

            if (winList.Count > 0)
            {
                long totalWinLineCredit = slotMachineCtrl.GetTotalWinCredit(winList);
                if (ContentModel.Instance.newFreeOnceCredit.Count > ContentModel.Instance.freeSpinPlayTimes - 1)
                {
                    totalWinLineCredit = ContentModel.Instance.newFreeOnceCredit[ContentModel.Instance.freeSpinPlayTimes - 1];
                    freeAllWin += (int)totalWinLineCredit;
                }


                if (winList.Count > 0)
                {
                    yield return slotMachineCtrl.ShowSymbolWinBySetting(slotMachineCtrl.GetTotalSymbolWin(winList), true, PusherEmperorsRein.SpinWinEvent.TotalWinLine);
                }

                // 播大奖弹窗
                WinLevelType winLevelType = GetBigWinType();
                if (winLevelType != WinLevelType.None)
                {
                    slotMachineCtrl.CloseSlotCover();
                    slotMachineCtrl.SkipWinLine(true);
                    StopAllFreeTrainIdleEffects();
                    yield return BigWinPopup(winLevelType, ContentModel.Instance.baseGameWinCredit);
                    SetFreeTrainState();
                    slotMachineCtrl.ShowSymbolWinDeck(slotMachineCtrl.GetTotalSymbolWin(winList), true);
                }
            }
            // 总线赢分事件
            slotMachineCtrl.SendTotalWinCreditEvent(ContentModel.Instance.curFreeCredit);

            #endregion



            ContentModel.Instance.gameState = GameState.Idle;
            // 先结算主游戏，再进入“免费游戏”或“小游戏”，则每局都可以同步玩家真实金钱金额

            if (successCallback != null)
                successCallback.Invoke();
        }

        /// <summary> 从本地快照恢复未完成的免费局（不自动请求 Spin，由玩家点转）。 </summary>
        void TryRestoreFreeSpinSession()
        {
            if (ApplicationSettings.Instance.isMock || slotMachineCtrl == null) return;
            if (!SQLitePlayerPrefs03.Instance.isInit) return;

            int pid = SBoxModel.Instance.pid;
            var snap = FreeSpinSessionStoreG3996.TryLoad(pid);
            if (snap == null) return;

            bool sessionStillValid = snap.FreeSpinTotalTimes > 0
                && (snap.FreeSpinPlayTimes < snap.FreeSpinTotalTimes
                    || (snap.FreeSpinPlayTimes == 0 && snap.NextReelStripsIndex == "FS"));
            if (!sessionStillValid)
            {
                FreeSpinSessionStoreG3996.Clear(pid);
                return;
            }

            var cm = ContentModel.Instance;
            cm.freeSpinTotalTimes = snap.tempFreeTotalTimes;
            cm.freeSpinPlayTimes = snap.FreeSpinPlayTimes;
            cm.freeSpinTotalWinCredit = snap.FreeSpinTotalWinCredit;
            cm.curReelStripsIndex = snap.CurReelStripsIndex;
            cm.nextReelStripsIndex = snap.NextReelStripsIndex;
            cm.gameNumberFreeSpinTrigger = snap.GameNumberFreeSpinTrigger;
            cm.isFreeSpinTrigger = false;
            cm.isFreeSpinResult = false;
            cm.isFreeSpinAdd = false;
            cm.curFreeCredit = snap.curFreeCredit;
            cm.newFreeOnceCredit = snap.newFreeOnceCredit;
            cm.wildNums = snap.wildNum;
            cm.realCredit = snap.realCredit;
            MainBlackboardController.Instance.SetMyTempCredit(cm.realCredit - cm.curFreeCredit);

            int betIndex = (ContentModel.Instance.wildNums / 4) + 1;
            betIndex = betIndex > 4 ? 4 : betIndex;
            if (betIndex != ContentModel.Instance.curFreeMult) ContentModel.Instance.curFreeMult = betIndex;

            if (snap.BetIndex >= 0 && SBoxModel.Instance.betList != null
                                    && snap.BetIndex < SBoxModel.Instance.betList.Count)
            {
                cm.betIndex = snap.BetIndex;
                cm.totalBet = SBoxModel.Instance.betList[cm.betIndex];
            }
            else
            {
                cm.totalBet = snap.TotalBet;
            }

            cm.betmultiple = snap.BetMultiple;
            cm.showFreeSpinRemainTime = cm.freeSpinTotalTimes - cm.freeSpinPlayTimes;
            cm.gameState = GameState.Idle;
            cm.PendingFreeSpinReconnectValidation = true;

            if (!string.IsNullOrEmpty(snap.StrDeckRowCol))
            {
                cm.strDeckRowCol = snap.StrDeckRowCol;
                slotMachineCtrl.SetReelsDeck(snap.StrDeckRowCol);
            }

            if (cm.curReelStripsIndex == "FS" || cm.nextReelStripsIndex == "FS")
            {
                ChangeBGPanel(1);
                SetFillAmount();
                SetFreeTrainState();
                ContentModel.Instance.isSysCredit = true;
                freeTimes.text = (ContentModel.Instance.freeSpinPlayTimes).ToString();
                freeTotalTimes.text = ContentModel.Instance.freeSpinTotalTimes.ToString();
                isConnectFreeSpin = true;
            }


            slotMachineCtrl.SendTotalWinCreditEvent(snap.curFreeCredit);
            DebugUtils.Log($"[G3996] 已恢复免费局快照：剩余 {cm.showFreeSpinRemainTime} / 总 {cm.freeSpinTotalTimes}，待首局 Spin 与算法校验。");
        }

        void OnGameReset()
        {
            if (corGameIdel != null) mono.StopCoroutine(corGameIdel);
            if (corEffectSlowMotion != null) mono.StopCoroutine(corEffectSlowMotion);

            slotMachineCtrl.isStopImmediately = false;
            slotMachineCtrl.CloseSlotCover();
            isStoppedSlotMachine = false;

            ComReelEffect2.visible = false;
            ComReelEffect3.visible = false;
            if (!ContentModel.Instance.isJackpotSpin)
            {
                slotMachineCtrl.SkipWinLine(true);
            }
        }

        //游戏状态闲置
        private IEnumerator GameIdle(List<SymbolWin> winList)
        {
            if (winList.Count == 0)
            {
                yield break;
            }

            SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.GameIdle);

            int i = 0;
            while (i < 3 && !slotMachineCtrl.isStopImmediately)
            {
                i++;
                yield return slotMachineCtrl.ShowSymbolWinBySetting(slotMachineCtrl.GetTotalSymbolWin(winList), true, PusherEmperorsRein.SpinWinEvent.TotalWinLine);
            }
            yield return slotMachineCtrl.ShowWinListAwayDuringIdle(winList);
        }


        //免费游戏开始时重置
        private void FreeGameReset()
        {
            fill1.fillAmount = 0;
            fill2.fillAmount = 0;
            fill3.fillAmount = 0;
            fill4.fillAmount = 0;
            ContentModel.Instance.curFreeMult = 1;

            freeTotalTimes.text = ContentModel.Instance.freeSpinTotalTimes.ToString();
        }

        //检查bigwin类型
        WinLevelType GetBigWinType()
        {
            long baseGameWinCredit = ContentModel.Instance.baseGameWinCredit;
            List<WinMultiple> winMultipleList = CustomModel.Instance.winLevelMultiple;
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

        #region bigWin相关
        //bigwin弹窗
        IEnumerator BigWinPopup(WinLevelType winLevelType, long winCredit)
        {
            slotMachineCtrl.CloseSlotCover();
            slotMachineCtrl.SkipWinLine(true);
            bool isNext = false;
            PageManager.Instance.OpenPageAsync(PageName.CaiFuHuoChePopupBigWin,
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
            isNext = false;
        }

        #endregion


        IEnumerator RequestSlotSpinFromMock(Action successCallback = null, Action<string> errorCallback = null)
        {
            bool isNext = false;
            bool isBreak = false;
            long totalBet = TotalBet;
            JSONNode resNode = null;

            //请求结果
            MachineDataG3996Controller.Instance.RequestSlotSpinFromMock(totalBet, (res) =>
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

            // 检查余额通过后，立即扣除积分（提前扣分）
            if (ContentModel.Instance.gameState != GameState.FreeSpin)
            {
                MainBlackboardController.Instance.MinusMyTempCredit(TotalBet, true, false);
            }

            SBoxJackpotData sboxJackpotData = null;

            ////赠送局不用扣分
            //if (ContentModel.Instance.gameState != GameState.FreeSpin)
            //{
            //    MainBlackboardController.Instance.MinusMyTempCredit(totalBet, true, false);
            //}

            // 解析数据
            MachineDataG3996Controller.Instance.ParseSlotSpin(totalBet, resNode, sboxJackpotData);

            // 数据入库

            // 游戏彩金滚轮
            //SetUIJackpotGameReel();

            if (successCallback != null)
                successCallback.Invoke();
        }


        //请求算法结果
        IEnumerator RequestSlotSpinFromMachine(Action successCallback = null, Action<string> errorCallback = null)
        {
            long totalBet = TotalBet;
            bool isBreak = false;
            bool isNext = false;
            bool isGetMyCredit = false;

            JSONNode resNode = null;
            int myCredit = -1;

            if (!ContentModel.Instance.isUsedRes)
            {
                ERPushMachineDataManager02.Instance.RequestCoinPushSpin((res) =>
                {
                    Debug.Log("请求算法结果");
                    resNode = JSONNode.Parse((string)res);
                    isNext = true;
                });
                ContentModel.Instance.isUsedRes = false;
            }


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
            MachineDataG3996Controller.Instance.ParseSlotSpin(totalBet, resNode, sboxJackpotData);

            // 数据入库

            // ui 彩金
            //SetUIJackpotGameReel();
            // Debug.Log("获取滚轮成功");

            if (successCallback != null)
                successCallback.Invoke();
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
            }
        }

        private bool isGetCrediting = false;
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


        //显示加速框
        public IEnumerator ShowEffectReelsSlowMotion(int colIdx)
        {
            PlayAnim(girlAnim, "ng_atmosphere");
            isTriggerFrame = true;
            GComponent ComReelEffect = ComReelEffect3;
            if (ContentModel.Instance.isFreeSlotTip)
            {
                ComReelEffect = ComReelEffect2;
            }

            ComReelEffect.visible = false;
            ComReelEffect.xy = slotMachineCtrl.SymbolCenterToNodeLocalPos(colIdx, 1, anchorExpectation);
            ComReelEffect.visible = true;
            if (ContentModel.Instance.isFreeSlotTip)
                EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                    new EventData(SlotMachineEvent.FreeRollingBox));
            else
                EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                    new EventData(SlotMachineEvent.BonusRollingBox));

            yield return new WaitUntil(() => isStoppedSlotMachine == true);
            // 关闭Expectation
            ComReelEffect.visible = false;
        }


        int rewardEffectIndex = 0;
        long allWinCredit = 0;
        //显示中奖后飞行粒子特效
        public IEnumerator ShowRewardEffect(int colIdx, int rowIdx, GComponent toNode, Action successCallback = null, bool isJackpot = false)
        {
            GComponent rewardEffect = null;
            rewardEffectIndex = (rewardEffectIndex + 1) % 3;
            switch (rewardEffectIndex)
            {
                case 0:
                    rewardEffect = ComRewardEffect1;
                    break;
                case 1:
                    rewardEffect = ComRewardEffect2;
                    break;
                case 2:
                    rewardEffect = ComRewardEffect3;
                    break;
            }

            if (rewardEffect != null)
            {
                rewardEffect.parent.RemoveChild(rewardEffect);
                toNode.AddChild(rewardEffect);
                rewardEffect.visible = false;
                rewardEffect.xy = slotMachineCtrl.SymbolCenterToNodeLocalPos(colIdx, rowIdx, toNode);
                rewardEffect.visible = true;

                if (ContentModel.Instance.isFreeSpin &&
                    (toNode == anchorFill1 || toNode == anchorFill2 || toNode == anchorFill3 || toNode == anchorFill4))
                    EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                        new EventData(Game3996AudioEvent.FreeSpinWildChargeFly));

                yield return MoveToZeroOverTime(rewardEffect, slotMachineCtrl.SymbolCenterToNodeLocalPos(colIdx, rowIdx, toNode));
            }

            if (isJackpot)
            {
                //PlayEffectAnim(boxRewardEffect);
            }

            //记录并显示累计分数
            if (!(ContentModel.Instance.curReelStripsIndex == "FS"))
            {
                allWinCredit += ContentModel.Instance.jackpotSpinWinCredit;
                slotMachineCtrl.SendTotalWinCreditEvent(allWinCredit);
            }


            successCallback?.Invoke();
        }

        private void OnCoinPushSpinResultParse(CoinPushSpinParseEventArgs e)
        {
            e.Result = MachineDataG3996Controller.ParseCoinPushSpinPayload(e.Data, e.StartPos);
        }

        IEnumerator MoveToZeroOverTime(GComponent effect, Vector2 startPosition, float duration = 1f, Action successCallback = null)
        {
            Vector2 endPos = Vector2.zero; // (0,0)
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // 应用OutQuad缓动（更自然）
                float easedT = t * (2 - t);

                effect.xy = Vector2.Lerp(startPosition, endPos, easedT);
                yield return null;
            }

            // 确保最终位置准确
            effect.xy = Vector2.zero;
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

        //根据传入的父节点依次播放粒子特效
        private void PlayChildEffectAnim(Transform effect)
        {
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
            particle.Clear();
            // 递归播放所有子物体的粒子系统
            foreach (Transform child in effect)
            {
                StopEffectAnim(child);
            }
        }

        //根据传入的父节点依次停止粒子特效
        private void StopChildEffectAnim(Transform effect)
        {
            // 递归播放所有子物体的粒子系统
            foreach (Transform child in effect)
            {
                StopEffectAnim(child);
            }
        }


        IEnumerator JackpotRequestSlotSpinFromMock(Action successCallback = null, Action<string> errorCallback = null)
        {
            bool isNext = false;
            bool isBreak = false;
            long totalBet = TotalBet;
            JSONNode resNode = null;
            //请求结果
            MachineDataG3996Controller.Instance.RequestSlotSpinFromMock(TotalBet, (res) =>
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

            SlotG3996MachineDataManager.Instance.RequestGetJpMajorGrandContribution((res) =>
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
            MachineDataG3996Controller.Instance.JackpotSlotSpin(totalBet, resNode, sboxJackpotData);

            if (successCallback != null)
                successCallback.Invoke();
        }

        /// <summary>
        /// 断电重连恢复免费局：点击一次开始后自动跑完整段免费，并统一结算与切回普通游戏。
        /// </summary>
        IEnumerator GameFreeSpinFromReconnect(Action successCallback, Action<string> errorCallback)
        {
            yield return FreeSpinTrigger(null, errorCallback);

            long freeSpinTotalWinCredit = ContentModel.Instance.freeSpinTotalWinCredit;
            if (freeSpinTotalWinCredit > 0)
            {
                MainBlackboardController.Instance.AddMyTempCredit(freeSpinTotalWinCredit, true, isAddCreditAnim);
            }

            ChangeBGPanel(0);
            MainBlackboardController.Instance.SyncMyTempCreditToReal(true);

            if (successCallback != null)
                successCallback.Invoke();
        }


        void SetFillAmount()
        {
            fill1.fillAmount = 0;
            fill2.fillAmount = 0;
            fill3.fillAmount = 0;
            fill4.fillAmount = 0;

            if (ContentModel.Instance.wildNums >= 4)
            {
                fill1.fillAmount = 1;
                if (ContentModel.Instance.wildNums >= 8)
                {
                    fill2.fillAmount = 1;
                    if (ContentModel.Instance.wildNums >= 12)
                    {
                        fill3.fillAmount = 1;
                        if (ContentModel.Instance.wildNums >= 16)
                        {
                            fill4.fillAmount = 1;
                        }
                    }
                }
            }


            if (ContentModel.Instance.wildNums % 4 != 0)
            {
                int index = ContentModel.Instance.wildNums / 4;
                switch (index)
                {
                    case 0:
                        fill1.fillAmount += (ContentModel.Instance.wildNums % 4) / 4.0f;
                        break;
                    case 1:
                        fill2.fillAmount += (ContentModel.Instance.wildNums % 4) / 4.0f;
                        break;
                    case 3:
                        fill2.fillAmount += (ContentModel.Instance.wildNums % 4) / 4.0f;
                        break;
                    case 4:
                        fill4.fillAmount += (ContentModel.Instance.wildNums % 4) / 4.0f;
                        break;
                }
            }
        }

        private void StopAllFreeTrainIdleEffects()
        {
            if (idleEffect1 != null) StopChildEffectAnim(idleEffect1);
            if (idleEffect2 != null) StopChildEffectAnim(idleEffect2);
            if (idleEffect3 != null) StopChildEffectAnim(idleEffect3);
            if (idleEffect4 != null) StopChildEffectAnim(idleEffect4);
            if (idleEffect5 != null) StopChildEffectAnim(idleEffect5);
        }

        private void SetFreeTrainState()
        {
            if (fill3.fillAmount == 1)
            {
                EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                    new EventData<int>(Game3996AudioEvent.FreeSpinMeterSound, 2));
                PlayAnim(freeTrainAnim, "idle4");
                StopAllFreeTrainIdleEffects();
                if (!TempDisableFreeTrainIdleParticles)
                {
                    PlayChildEffectAnim(idleEffect4);
                    PlayChildEffectAnim(idleEffect5);
                }
            }
            else if (fill2.fillAmount == 1)
            {
                EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                    new EventData<int>(Game3996AudioEvent.FreeSpinMeterSound, 2));
                StopAllFreeTrainIdleEffects();
                if (!TempDisableFreeTrainIdleParticles)
                    PlayChildEffectAnim(idleEffect3);
                PlayAnim(freeTrainAnim, "idle3");
            }
            else if (fill1.fillAmount == 1)
            {
                EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                    new EventData<int>(Game3996AudioEvent.FreeSpinMeterSound, 2));
                PlayAnim(freeTrainAnim, "idle2");
                StopAllFreeTrainIdleEffects();
                if (!TempDisableFreeTrainIdleParticles)
                    PlayChildEffectAnim(idleEffect2);
            }
            else
            {
                PlayAnim(freeTrainAnim, "idle1");
                StopAllFreeTrainIdleEffects();
                if (!TempDisableFreeTrainIdleParticles)
                    PlayChildEffectAnim(idleEffect1);
            }
        }


        /// <summary>
        /// 点击Spin按钮旋转失败的报错
        /// </summary>
        /// <param name="msg"></param>
        private void StopGameWhenError(string msg)
        {
            UnlockStopButton();
            ContentModel.Instance.isSpin = false;
            ContentModel.Instance.isAuto = false;
            ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
            ContentModel.Instance.gameState = GameState.Idle;


            // 有好酷优先用好酷
            if (false && SBoxModel.Instance.isUseIot && _tipCoinIn)
            {
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

        private void LockStopButton()
        {
            if (_isStopButtonLocked)
            {
                return;
            }

            _isStopButtonLocked = true;
            if (MainModel.Instance.panel is PanelBaseController panelBaseController)
            {
                panelBaseController.SetSpinButtonLocked(true);
            }
        }

        private void UnlockStopButton()
        {
            if (!_isStopButtonLocked)
            {
                return;
            }

            _isStopButtonLocked = false;
            if (MainModel.Instance.panel is PanelBaseController panelBaseController)
            {
                panelBaseController.SetSpinButtonLocked(false);
            }
        }

        private void OnPlayGirlClaw(EventData res)
        {
            if (!showClawAnim)
            {
                showClawAnim = true;
                PlayAnim(girlAnim, "sg_appear");
            }
        }

        //读取当前滚轴显示的图标
        private string GetCurrentVisibleDeckRowCol()
        {
            if (slotMachineCtrl == null)
            {
                return string.Empty;
            }
            List<string> rows = new List<string>(slotMachineCtrl.row);
            for (int row = 0; row < slotMachineCtrl.row; row++)
            {
                List<string> cols = new List<string>(slotMachineCtrl.column);
                for (int col = 0; col < slotMachineCtrl.column; col++)
                {
                    SymbolBase symbol = slotMachineCtrl.GetVisibleSymbolFromDeck(col, row);
                    int symbolNumber = symbol != null ? symbol.GetSymbolNumber() : 0;
                    cols.Add(symbolNumber.ToString());
                }
                rows.Add(string.Join(",", cols));
            }
            return string.Join("#", rows);
        }




        #region 彩金游戏单个元素转动相关


        private GComponent smallGameReels;

        private readonly string _moneyUrl = "ui://CaiFuHuoChe_3996/symbol_13";

        /// <summary>15个格子控制器</summary>
        private readonly List<SmallGameReelController> _elementBoxes = new List<SmallGameReelController>();

        /// <summary>所有中奖结果</summary>
        private readonly List<SmallReelResultInfo> _allHitResults = new List<SmallReelResultInfo>();

        /// <summary>未揭示的中奖结果</summary>
        private readonly List<SmallReelResultInfo> _unrevealedHits = new List<SmallReelResultInfo>();

        private const int MAX_ROLLING_COUNT = 15;

        /// <summary>剩余滚动次数</summary>
        private int _remainingRolls;

        /// <summary>滚轴错开延迟</summary>
        private readonly float _reelStaggerDelay = 0.05f;

        private GameObject _jackpotHitObj;
        private GTextField rollCountText;

        private readonly int _initialRollCount = 3;

        private readonly List<string> _jackpotUrls = new List<string>()
        {
            "ui://CaiFuHuoChe_3996/symbol_16",
            "ui://CaiFuHuoChe_3996/symbol_15",
            "ui://CaiFuHuoChe_3996/symbol_14",
        };

        private void InitSmallGame()
        {
            foreach (var t in _elementBoxes)
                t.Reset();
            _elementBoxes.Clear();
            _allHitResults.Clear();
            _unrevealedHits.Clear();
            List<int> strNum = SlotTool.GetDeckRowCol(ContentModel.Instance.strDeckRowCol);

            for (int i = 0; i < 15; i++)
            {
                GComponent boxNode = smallGameReels.GetChild("elementBox_" + i).asCom;
                GComponent element = boxNode.GetChild("rollElement_4").asCom;
                int index = strNum[i];
                element.GetChild("element").asLoader.url = CustomModel.Instance.symbolIcon[index.ToString()];
                element.GetChild("rewardText").asTextField.visible = false;

                SmallGameReelController box = new SmallGameReelController(boxNode, i);
                _elementBoxes.Add(box);

                if (!ContentModel.Instance.jackpotWin.ContainsKey(i)) continue;
                int row = i / 5;
                int col = i % 5;

                var info = ParseSmallGameData(i, row, col, int.Parse(ContentModel.Instance.jackpotWin[i]), ContentModel.Instance.jackpotSocre);

                if (info.type != SmallResultType.None)
                {
                    _allHitResults.Add(info);
                    _unrevealedHits.Add(info);
                    GameObject jackpotHitObj = GameObject.Instantiate(_jackpotHitObj);
                    box.SetResultData(info, jackpotHitObj);
                }
            }

            UpdateRollCountUI(_initialRollCount);
        }



        private SmallReelResultInfo ParseSmallGameData(int index, int row, int col, int currentBet, Dictionary<int, int> jackpotSocre)
        {
            var info = new SmallReelResultInfo { reelIndex = index, row = row, col = col, type = SmallResultType.None };

            if (currentBet == 0) return info;

            int type = currentBet / 1000;
            int value = currentBet % 1000;

            if (type < 4)
            {
                info.type = SmallResultType.Money;
                info.rewardValue = value;
                info.rewardText = value.ToString();
                info.iconUrl = _moneyUrl;
                info.anchorChildIndex = 0;
            }
            else
            {
                int jackpotType = value % 10;
                int jackpotValue = GetJackpotValue(jackpotType, jackpotSocre);

                info.type = SmallResultType.Jackpot;
                info.jackpotType = jackpotType;
                info.rewardValue = jackpotValue;
                info.rewardText = jackpotValue.ToString();

                info.iconUrl = _jackpotUrls[jackpotType];
                info.anchorChildIndex = jackpotType + 1;
            }

            return info;
        }


        private void UpdateRollCountUI(int count)
        {
            if (freeTimes != null)
                freeTimes.text = count.ToString();
        }


        private int GetJackpotValue(int jackpotType, Dictionary<int, int> jackpotSocre)
        {
            return jackpotSocre[jackpotType];
        }



        private IEnumerator SmallGameTrigger(Action successCallback = null)
        {
            ContentModel.Instance.jackpotSpinWinCredit = 0;
            allWinCredit = 0;
            slotMachineCtrl.BeginBonusFreeSpin();
            InitSmallGame();
            bool isNext = false;
            PageManager.Instance.OpenPageAsync(PageName.CaiFuHuoChePopupJackpotGameTrigger,
                new EventData<Dictionary<string, object>>("", new Dictionary<string, object>()
                {
                    ["SpinTimes"] = 3,
                }),
            (ed) =>
            {
                Debug.Log("回调执行！isNext = true"); // 加日志
                isNext = true;
            });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(SlotMachineEvent.BonusGameFadeTransition));
            yield return new WaitForSeconds(0.9f);

            train.SetActive(false);
            yield return new WaitForSeconds(0.3f);

            ChangeBGPanel(2);
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(Game3996AudioEvent.BgmBonusGame));
            PlayAnim(girlAnim, "sg_idle1");
            freeTotalTimes.text = ContentModel.Instance.jackpotSpinTotalTimes.ToString();
            freeTimes.text = ContentModel.Instance.jackpotSpinTotalTimes.ToString();
            yield return new WaitForSeconds(0.4f);

            yield return new WaitForSeconds(0.2f);
            //PlayEffectAnim(startEffect);

            //PlayEffectAnim(boxIdleEffect);

            //------------------------  此处补充正式游戏 和 结算分数逻辑  ------------------------
            yield return SmallGameSpin(() => isNext = true);

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            slotMachineCtrl.SkipIdle(true);
            slotMachineCtrl.SkipWinLine(true);

            PlayAnim(girlAnim, "sg_settlement");
            yield return new WaitForSeconds(2);

            //StopEffectAnim(boxIdleEffect);
            yield return new WaitForSeconds(3.2f);

            PageManager.Instance.OpenPageAsync(PageName.CaiFuHuoChePopupJackpotGameExit,
                new EventData<Dictionary<string, object>>("", new Dictionary<string, object>()
                {
                    ["winCredit"] = allWinCredit,
                }),
            (ed) =>
            {
                Debug.Log("回调执行！isNext = true"); // 加日志
                isNext = true;
            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;
            slotMachineCtrl.EndBonusFreeSpin();
            //加钱动画
            MainBlackboardController.Instance.AddMyTempCredit(allWinCredit, true, isAddCreditAnim);

            ChangeBGPanel(0);
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(Game3996AudioEvent.BgmRegularGame));
            train.SetActive(true);
            JsToBsTrans.Play();
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(SlotMachineEvent.BonusGameFadeTransition));

            successCallback?.Invoke();
        }


        private IEnumerator SmallGameSpin(Action successCallback = null)
        {
            yield return SmallGameLoop();
            yield return new WaitForSeconds(0.5f);
            yield return SmallGameResult();
            successCallback?.Invoke();
        }

        private IEnumerator SmallGameResult(Action onCompleted = null)
        {
            yield return JackpotSettlementProcess(onCompleted);
        }

        private IEnumerator JackpotSettlementProcess(Action onCompleted)
        {
            foreach (var t in _allHitResults)
            {
                int index = t.reelIndex;
                if (t.type == SmallResultType.Money)
                {
                    yield return ShowJackpotSettlement(SmallResultType.Money, index, t.iconUrl, t.rewardText, anchorJackpotAdd, t.col, t.row);
                }
                else if (t.type == SmallResultType.Jackpot)
                {
                    yield return ShowJackpotSettlement(SmallResultType.Jackpot, index, t.iconUrl, t.rewardText, anchorJackpotAdd, t.col, t.row);
                    bool isNext = false;
                    PageManager.Instance.OpenPageAsync(PageName.CaiFuHuoChePopupJackpotResult,
                        new EventData<Dictionary<string, object>>("",
                            new Dictionary<string, object>()
                            {
                                ["jackpotType"] = t.jackpotType,
                                ["totalEarnCredit"] = t.rewardValue,
                            }), (res) =>
                            {
                                isNext = true;
                            });
                    yield return new WaitUntil(() => isNext == true);
                }
            }

            onCompleted?.Invoke();
        }

        private IEnumerator ShowJackpotSettlement(SmallResultType resultType, int index, string iconUrl, string rewardText, GComponent toNode, int colIdx, int rowIdx)
        {
            GComponent rewardEffect = null;
            rewardEffectIndex = (rewardEffectIndex + 1) % 3;
            switch (rewardEffectIndex)
            {
                case 0:
                    rewardEffect = ComRewardEffect1;
                    break;
                case 1:
                    rewardEffect = ComRewardEffect2;
                    break;
                case 2:
                    rewardEffect = ComRewardEffect3;
                    break;
            }

            if (rewardEffect != null)
            {
                _elementBoxes[index].PlayAnim("collect");
                yield return new WaitForSeconds(0.5f);
                rewardEffect.parent.RemoveChild(rewardEffect);
                toNode.AddChild(rewardEffect);
                rewardEffect.visible = false;
                rewardEffect.xy = slotMachineCtrl.SymbolCenterToNodeLocalPos(colIdx, rowIdx, toNode);
                rewardEffect.visible = true;
                _elementBoxes[index].mask.visible = false;
                _elementBoxes[index].result.SetMask(true);
                _elementBoxes[index].result.RemoveAnchor();


                yield return MoveToZeroOverTime(rewardEffect, rewardEffect.xy);
                rewardEffect.visible = false;
                allWinCredit += long.Parse(rewardText);
                slotMachineCtrl.SendTotalWinCreditEvent(allWinCredit);
            }
        }


        //private IEnumerator SmallGameLoop()
        //{
        //    _remainingRolls = _initialRollCount;
        //    UpdateRollCountUI(_remainingRolls);

        //    while (_remainingRolls > 0)
        //    {
        //        // 每轮开始前：重置未揭示的格子的滚动元素
        //        foreach (var box in _elementBoxes)
        //        {
        //            if (box.State != SmallReelState.Revealed)
        //                box.PlayRollReset();
        //        }

        //        // === 先确定本轮中奖 ===
        //        List<SmallReelResultInfo> reveals = DrawReveals();

        //        // ========== 新增：限制最多6个reel滚动 ==========
        //        List<SmallReelResultInfo> selectedReveals = new List<SmallReelResultInfo>();
        //        List<SmallReelResultInfo> delayedReveals = new List<SmallReelResultInfo>();

        //        if (reveals.Count > MAX_ROLLING_COUNT)
        //        {
        //            // 中奖数超过6个：随机选6个本轮揭示，其余延后
        //            var shuffled = reveals.OrderBy(_ => UnityEngine.Random.value).ToList();
        //            selectedReveals = shuffled.Take(MAX_ROLLING_COUNT).ToList();
        //            delayedReveals = shuffled.Skip(MAX_ROLLING_COUNT).ToList();

        //            // 被延后的中奖结果保留在 _unrevealedHits 中（不要从 _unrevealedHits 移除它们）
        //            // 这样它们会继续参与后续轮次的 DrawReveals()
        //        }
        //        else
        //        {
        //            selectedReveals = reveals;
        //        }
        //        // ================================================

        //        HashSet<int> hitIndices = new HashSet<int>(selectedReveals.Select(r => r.reelIndex));

        //        // 1. 分类reel：中奖的 vs 普通的
        //        List<int> hitReelIndices = new List<int>();
        //        List<int> normalReelIndices = new List<int>();

        //        for (int i = 0; i < _elementBoxes.Count; i++)
        //        {
        //            if (_elementBoxes[i].State == SmallReelState.Idle)
        //            {
        //                if (hitIndices.Contains(i))
        //                    hitReelIndices.Add(i);
        //                else
        //                    normalReelIndices.Add(i);
        //            }
        //        }

        //        // 2. 如果中奖数不足6个，从普通reel中随机补足到6个
        //        if (hitReelIndices.Count < MAX_ROLLING_COUNT && normalReelIndices.Count > 0)
        //        {
        //            int needCount = MAX_ROLLING_COUNT - hitReelIndices.Count;
        //            var shuffledNormal = normalReelIndices.OrderBy(_ => UnityEngine.Random.value).ToList();
        //            normalReelIndices = shuffledNormal.Take(Math.Min(needCount, shuffledNormal.Count)).ToList();
        //        }
        //        else if (hitReelIndices.Count >= MAX_ROLLING_COUNT)
        //        {
        //            // 中奖数已经达到或超过6个（理论上不会超过，因为上面已经截断了），普通reel不滚动
        //            normalReelIndices.Clear();
        //        }

        //        // 3. 设置滚动视觉
        //        foreach (int idx in hitReelIndices)
        //            _elementBoxes[idx].SetRollingVisual();
        //        foreach (int idx in normalReelIndices)
        //            _elementBoxes[idx].SetRollingVisual();

        //        // 4. 所有reel一起开始滚动（中奖reel第一圈roll第二圈result，普通reel两圈roll）
        //        yield return PlayMixedRollSequence(hitReelIndices, normalReelIndices, selectedReveals);

        //        // 5. 处理结果（滚动已包含揭示，只需处理次数）
        //        if (selectedReveals.Count > 0)
        //        {
        //            _remainingRolls = _initialRollCount;
        //            UpdateRollCountUI(_remainingRolls);
        //        }
        //        else
        //        {
        //            _remainingRolls--;
        //            UpdateRollCountUI(_remainingRolls);
        //        }

        //        yield return new WaitForSeconds(0.3f);
        //    }
        //}


        List<int> hitReelIndices = new List<int>();
        List<int> normalReelIndices = new List<int>();

        private IEnumerator SmallGameLoop()
        {
            _remainingRolls = _initialRollCount;
            UpdateRollCountUI(_remainingRolls);
            while (_remainingRolls > 0)
            {
                _remainingRolls--;
                UpdateRollCountUI(_remainingRolls);
                // 每轮开始前：重置未揭示的格子的滚动元素
                foreach (var box in _elementBoxes)
                {
                    if (box.State != SmallReelState.Revealed)
                        box.PlayRollReset();
                }

                // 1. 分类reel：中奖的 vs 普通的
                hitReelIndices.Clear();
                normalReelIndices.Clear();

                // === 先确定本轮中奖 ===
                List<SmallReelResultInfo> reveals = DrawReveals();
                HashSet<int> hitIndices = new HashSet<int>(reveals.Select(r => r.reelIndex));


                for (int i = 0; i < _elementBoxes.Count; i++)
                {
                    if (_elementBoxes[i].State == SmallReelState.Idle)
                    {
                        if (hitIndices.Contains(i))
                            hitReelIndices.Add(i);
                        else
                            normalReelIndices.Add(i);
                    }
                }

                // 2. 设置滚动视觉
                foreach (int idx in hitReelIndices)
                    _elementBoxes[idx].SetRollingVisual();
                foreach (int idx in normalReelIndices)
                    _elementBoxes[idx].SetRollingVisual();

                // 3. 所有reel一起开始滚动（中奖reel第一圈roll第二圈result，普通reel两圈roll）
                yield return PlayMixedRollSequence(hitReelIndices, normalReelIndices, reveals);

                // 4. 处理结果（滚动已包含揭示，只需处理次数）
                if (reveals.Count > 0)
                {
                    _remainingRolls = _initialRollCount;
                    UpdateRollCountUI(_remainingRolls);
                }

                yield return new WaitForSeconds(0.3f);
            }
        }


        List<SmallReelResultInfo> reveals = new List<SmallReelResultInfo>();
        private List<SmallReelResultInfo> DrawReveals()
        {
            reveals.Clear();
            if (_unrevealedHits.Count == 0) return reveals;

            double revealRate = CalculateRevealRate();
            bool shouldReveal = UnityEngine.Random.value < revealRate;

            if (!shouldReveal) return reveals;

            int max = Math.Min(3, _unrevealedHits.Count);
            int count = UnityEngine.Random.Range(1, max + 1);

            var shuffled = _unrevealedHits.OrderBy(x => UnityEngine.Random.value).ToList();
            for (int i = 0; i < count; i++)
                reveals.Add(shuffled[i]);

            return reveals;
        }


        private IEnumerator PlayMixedRollSequence(List<int> hitIndices, List<int> normalIndices, List<SmallReelResultInfo> reveals)
        {
            int completedCount = 0;
            int totalCount = hitIndices.Count + normalIndices.Count;
            bool isFinish = false;

            // 中奖reel：第一圈roll，第二圈result
            foreach (int idx in hitIndices)
            {
                int captureIdx = idx;
                var revealInfo = reveals.First(r => r.reelIndex == captureIdx);
                _unrevealedHits.Remove(revealInfo);

                //float delay = captureIdx * _reelStaggerDelay;
                //float rollSpeed = 2;

                _elementBoxes[captureIdx].PlayHitRoll(1, 1, () =>
                {
                    PlayAnim(girlAnim, "sg_appear");
                    //if (!isFinish)
                    //    isFinish = true;
                });
            }

            // 普通reel：两圈roll
            foreach (int idx in normalIndices)
            {
                int captureIdx = idx;
                //float delay = captureIdx * _reelStaggerDelay;
                // float speed = _rollSpeedList[captureIdx];
                float speed = 1;

                _elementBoxes[captureIdx].PlayNormalRoll(speed, () =>
                {
                    if (!isFinish)
                        isFinish = true;
                });

                //yield return DelayedAction(delay, () =>
                //{
                //    if (captureIdx < _elementBoxes.Count && _elementBoxes[captureIdx] != null)
                //    {
                //        _elementBoxes[captureIdx].PlayNormalRoll(speed, () =>
                //        {
                //            completedCount++;
                //            if (completedCount >= totalCount && !allComplete)
                //                allComplete = true;
                //        });
                //    }
                //});
            }

            yield return new WaitUntil(() => isFinish);
            yield return new WaitForSeconds(0.5f);

        }


        private double CalculateRevealRate()
        {
            double rate = 0.7;
            rate += (3 - _remainingRolls) * 0.1;

            if (_unrevealedHits.Count <= 2)
                rate += 0.2;

            return Math.Min(1.0, rate);
        }


        private IEnumerator DelayedAction(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }

        #endregion
    }
}
