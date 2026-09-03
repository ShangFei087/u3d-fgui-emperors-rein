using CaiFuHuoChe_3996;
using FairyGUI;
using GameMaker;
using Newtonsoft.Json;
using PusherEmperorsRein;
using SBoxApi;
using SimpleJSON;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using UnityEngine;

using _spinWEMD = SlotMaker.SpinWinEffectSettingModel;

namespace HuoYanGongNiu_3995
{
    public class GameConfigRoot
    {
        [JsonProperty("game_id")] public int GameId;

        [JsonProperty("game_name")] public string GameName;

        [JsonProperty("display_name")] public string DisplayName;

        [JsonProperty("bet_lst")] public int[] BetList;

        [JsonProperty("Wheel_Data")] public int[] InitalWheel;

        [JsonProperty("win_level_multiple")] public Dictionary<string, long> WinLevelMultiple { get; set; }

        [JsonProperty("symbol_paytable")] public Dictionary<string, PayTableSymbolInfo> SymbolPaytable { get; set; }

        [JsonProperty("pay_lines")] public List<List<int>> pay_lines { get; set; }
    }


    public class PageGameMain : MachinePageBase //: PageBase
    {
        public const string pkgName = "HuoYanGongNiu_3995";
        public const string resName = "PageGameMain";


        private SlotMachineController3995 slotMachineCtrl;
        private GComponent slotCover, gOwnerPanel, gPlayLines, gFrame;

        private GameObject goGameCtrl;

        PayTableController payTableController = new PayTableController();
        Coroutine corReelsTurn, corGameIdel, corGameOnce, corEffectSlowMotion, corRewardEffect, corWheelSpin;

        //游戏控制
        private MonoHelper mono;
        private FguiPoolHelper fguiPoolHelper;
        private FguiGObjectPoolHelper gObjectPoolHelper;

        long TotalBet => (long)MainModel.Instance.contentMD.totalBet;

        private new bool isInit = false;        //是否初始化
        private bool isInitPool = false;
        private bool tipCoinIn = false; //提示硬币输入
        private bool isStoppedSlotMachine = false;


        bool isAddCreditAnim => !(slotMachineCtrl.isStopImmediately == true || SBoxModel.Instance.isCoinOutImmediately);
        private EventData _data = null;

        const string CACHE_TOTAL_JP_MAJOR_CONTRIBUTION = "CACHE_TOTAL_JP_MAJOR_CONTRIBUTION";
        const string CACHE_TOTAL_JP_GRAND_CONTRIBUTION = "CACHE_TOTAL_JP_GRAND_CONTRIBUTION";

        //转盘旋转的内容
        private GComponent gWheel, gWheelEffect;
        private Transform showTime, Win, Idle, waitSpin;

        //转盘旋转相关参数
        private float segmentAngle = 30f; //     360 / 12 = 30°
        private float extralyAngle = 0;  //因为转盘分区角度不同，可能需要额外补充一些角度

        //NPC相关
        private GameObject npcObj, npcPre;
        private GComponent npcAnchor;
        private Animator npcAnim;

        //转盘游戏开始时下面出现的提示框
        private GameObject wheelTip, wheelTip2, wheelTip3, wheelEffect, wheelEffObj;
        private GameObject wheelTipObj, wheelTip2Obj, wheelTip3Obj;
        private GComponent gWheelTip, gWheelTip2, gWheelTip3;
        private Animator wheelTipAnim, wheelTipAnim2, wheelTipAnim3;
        private GTextField wheelWinCredit, collectedNums, wheelSpinTimesTxt;
        private bool isStartSpin, isEndSpin;
        private Transition freeTiggerInWheel;

        //转盘上的特效组件
        private List<GLoader> gBulls = new List<GLoader>();
        private List<GTextField> gTexts = new List<GTextField>();
        private int wheelOnceWin = 0, wheelWinGoldBull = 0;

        //转盘上的触发按钮
        private GButton wheelSpinBtn;
        private bool isWheelSpin = false;
        private int wheelSpinTimes = 0;


        //轮盘游戏中对不同的种类数据进行存储(其实只需要对金额进行存储，其他的可以直接初始化)
        //private List<int> wheelCredit = new List<int>();

        //公牛特效
        private Transform shoutTrans, smokeTrans;


        //加速框配置
        private GComponent anchorExpectation, ComReelEffect;
        private GameObject goFreeReelEffcetPre, goFreeReelEffcetObj;

        //免费游戏移动特效
        private GComponent anchorFreeStart, ComRewardEffect1, ComRewardEffect2, ComRewardEffect3;
        private GameObject goRewardEffectPre, goCollectionPre, goCollectionEff, goRewardEffectObj1, goRewardEffectObj2, goRewardEffectObj3, goCollectionObj, goCollectionEffObj;
        private Transform freeCollectEff;


        //免费游戏上面两个提示框的数据，因为设为了高级组要先获取组,之后获取里面的数字，图标装载器等
        private GGroup freeTipLeft, freeTipRight;
        private GTextField freeTipLeftNum, freeTipRightNum;
        private GComponent deerIcon, wolfIcon, leopardIcon, eagleIcon, targetIcon;
        private Transition enterFreeGame;


        //免费游戏开始之前的特效增加分数或者金牛数量
        private GComponent creditsReward, collectedReward;
        private GComponent creditsStart, creditsTarget, collectedStart, collectedTarget;
        private GameObject creditsEffPrefab, creditsEff, collectedEffPrefab, collectEff;

        //免费游戏当中免费次数和总次数
        private GTextField freeRemainTimes, freeTotalTimes;

        //免费游戏金牛数量达标时播放动效
        private Transition StartChangeIcon, EndChangeIcon;
        private GComponent anchorChangeIconAnim;
        private GameObject changeIconAnim, changeIconAnimPre, eagleAnim, eagleAnimPre, leopardAnim, leopardAnimPre, wolfAnim, wolfAnimPre, deerAnim, deerAnimPre;

        //免费游戏轮盘相关的动效
        private Transition resetWheelTran, showWheelTran;

        //彩金游戏中的次数
        private GTextField jackpotTimes;

        //彩金游戏背景上的火山喷发的spine
        private GComponent anchorJpBgEff;
        private GameObject anchorJpBgEffPre, anchorJpBgEffObj;
        private Animator anchorJpBgAnim;

        /// <summary>底部 Panel 是否已就绪（BottomPanelReady）。</summary>
        private bool _isBottomPanelReady;
        /// <summary>对象池 DoTask 是否已全部完成。</summary>
        private bool _isPoolPreloadDone;
        /// <summary>是否已向 PageManager 派发过 preLoadedCallback。</summary>
        private bool _hasNotifiedPagePreloaded;

        ////Pag播放
        //private const string GamePagFolder = "Games/Huo Yan Gong Niu 3995/Pag/jp_huoshan_bmp";
        //private PagSlotBinding effectPag;
        //private string[] stageName = { "jp_huoshan_dabaofa_start.pag", "jp_huoshan_dabaofa_idle.pag" };


        //测试按钮
        private GButton testBtn;

        protected override void OnInit()
        {
            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            int count = 17;

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
            "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Game Controller/Slot Game Main ControllerClone.prefab",
            (GameObject clone) =>
            {
                if (goGameCtrl != null) //防止重复加载
                {
                    return;
                }
                goGameCtrl = GameObject.Instantiate(clone);
                goGameCtrl.name = "Slot Game Main Controller3995";
                goGameCtrl.transform.SetParent(null);
                //获取组件引用
                slotMachineCtrl = goGameCtrl.transform.Find("Slot Machine").GetComponent<SlotMachineController3995>();
                mono = goGameCtrl.transform.GetComponent<MonoHelper>();

                fguiPoolHelper = goGameCtrl.transform.Find("Pool").GetComponent<FguiPoolHelper>();
                gObjectPoolHelper = goGameCtrl.transform.Find("GObject Pool").GetComponent<FguiGObjectPoolHelper>();
                callback();
            });


            //ResourceManager02.Instance.LoadAssetBundleAsync("Assets/GameRes/Games/Emperors Rein 200/FGUIs", (ab) =>
            //{
            //    UIPackage.AddPackage(ab);
            //    callback();
            //});

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/PageGameMain/NPC",
                (GameObject clone) =>
                {
                    npcPre = clone;
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/PageGameMain/wheelTip",
                (GameObject clone) =>
                {
                    wheelTip = clone;
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/PageGameMain/wheelTip2",
                (GameObject clone) =>
                {
                    wheelTip2 = clone;
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/PageGameMain/wheelTip3",
                (GameObject clone) =>
                {
                    wheelTip3 = clone;
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/PageGameMain/wheelEffect",
                (GameObject clone) =>
                {
                    wheelEffect = clone;
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Effect/CreditsEffect",
                (GameObject clone) =>
                {
                    creditsEffPrefab = clone;
                    collectedEffPrefab = clone;
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Symbols/SymbolHit/SymbolHit10",
                (GameObject clone) =>
                {
                    changeIconAnimPre = clone;
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Symbols/SymbolHit/SymbolHit9",
                (GameObject clone) =>
                {
                    eagleAnimPre = clone;
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Symbols/SymbolHit/SymbolHit8",
                (GameObject clone) =>
                {
                    leopardAnimPre = clone;
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Symbols/SymbolHit/SymbolHit7",
                (GameObject clone) =>
                {
                    wolfAnimPre = clone;
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Symbols/SymbolHit/SymbolHit6",
                (GameObject clone) =>
                {
                    deerAnimPre = clone;
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Effect/FreeReelEffect.prefab",
            (GameObject clone) =>
            {
                goFreeReelEffcetPre = clone;
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Effect/RewardEffect.prefab",
            (GameObject clone) =>
            {
                goRewardEffectPre = clone;
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/PopupGameJackpot/jackpotSpine.prefab",
            (GameObject clone) =>
            {
                _jackpotHitObj = clone;
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/PopupFreeGame/Collection.prefab",
                (GameObject clone) =>
                {
                    goCollectionPre = clone;
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
               "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/PopupGameJackpot/brushEff.prefab",
               (GameObject clone) =>
               {
                   anchorJpBgEffPre = clone;
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

        public override void OnOpen(PageName name, EventData data)
        {
            if (isOpen) return;
            if (goGameCtrl != null && !goGameCtrl.activeSelf)
            {
                goGameCtrl.SetActive(true);
            }
            base.OnOpen(name, data); 
            EventCenter.Instance.AddEventListener<CoinPushSpinParseEventArgs>(SBoxEventHandle.SBOX_COIN_PUSH_SPIN_PARSE, OnCoinPushSpinResultParse);
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnClickSpinButton);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnStopSlot);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT, OnSlotDetailEvent);
            //EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT, OnSlotDetailEvent);
            //EventCenter.Instance.AddEventListener<EventData>("JackpotWinCredit", OnJackpotWinEvent);

            InitParam(null);
        }

        public override void OnClose(EventData data = null)
        {
            slotMachineCtrl.SkipWinLine(true);
            OnGameReset();

            EventCenter.Instance.RemoveEventListener<CoinPushSpinParseEventArgs>(SBoxEventHandle.SBOX_COIN_PUSH_SPIN_PARSE, OnCoinPushSpinResultParse);
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnClickSpinButton);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnStopSlot);
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_EVENT, OnBottomPanelReadyForPreload);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT, OnSlotDetailEvent);
            //EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT, OnSlotDetailEvent);
            //EventCenter.Instance.RemoveEventListener<EventData>("JackpotWinCredit", OnJackpotWinEvent);
            if (goGameCtrl != null && goGameCtrl.activeSelf)
            {
                goGameCtrl.SetActive(false);
            }
            mono.updateHandle.RemoveAllListeners();
            base.OnClose(data);
        }


        private void OnClickSpinButton(EventData res)
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

                        if (isWheelSpin)
                        {
                            mono.updateHandle.RemoveListener(WheelTrun);
                            StopEffectAnim(Win);
                            ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                            ContentModel.Instance.curBtnSpinState = SpinButtonState.Spin;
                            StartWheelSpinOnce(ContentModel.Instance.wheelData[wheelSpinTimes]);
                            return;
                        }

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

                            ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                            StartGameOnce(successCallback, StopGameWhenError); //开始玩
                        }


                    }
                    break;

                case SpinButtonState.Spin:
                    {
                        // 已经在游戏时，去停止游戏
                        if (!ContentModel.Instance.isSpin || isWheelSpin) return; // 已经停止直接退出

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


        public void InitParam(EventData data)
        {
            if (data != null) _data = data;
            if (!isInit) return;

            // ---------- 1. MainModel、Paytable、本地 JSON ----------
            MainModel.Instance.lineNum = 50;
            MainModel.Instance.gameID = 3995;
            MainModel.Instance.gameName = "HuoYanGongNiu3995";
            MainModel.Instance.displayName = "HuoYanGongNiu_3995";
            MainModel.Instance.contentMD = ContentModel.Instance;
            MainModel.Instance.cutomMD = CustomModel.Instance;
            MainModel.Instance.contentMD.betIndex = 0;
            ContentModel.Instance.totalBet = SBoxModel.Instance.betList[ContentModel.Instance.betIndex];

            //说明书
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
                fguiPoolHelper.WhenIdle(() =>
                {
                    _isPoolPreloadDone = true;
                    TryNotifyPagePreloaded();
                });
            }
            else if (fguiPoolHelper == null)
            {
                _isPoolPreloadDone = true;
            }



            // ---------- 3.滚轮控制器 ----------
            //初始化UI组件
            GComponent gSlotMachine = contentPane.GetChild("slotMachine").asCom;
            GComponent gReels = gSlotMachine.GetChild("reels").asCom;
            slotCover = gSlotMachine.asCom.GetChild("slotCover").asCom;
            gPlayLines = gSlotMachine.asCom.GetChild("playLines").asCom;
            gFrame = contentPane.GetChild("anchorFrame").asCom;
            slotMachineCtrl.Init(slotCover, gPlayLines, gReels, gFrame, fguiPoolHelper, gObjectPoolHelper);



            // ---------- 4. 底部菜单 Panel ----------

            //初始化轮盘元素
            gWheel = contentPane.GetChild("wheel").asCom.GetChild("wheelContent").asCom;

            ResetWheel();
            InitWheelItem();
            //EnsureMainPagSlot();

            //初始化菜单ui
            gOwnerPanel = this.contentPane.GetChild("panel").asCom;
            ContentModel.Instance.goAnthorPanel = gOwnerPanel;
            MainModel.Instance.contentMD.goAnthorPanel = gOwnerPanel;
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_EVENT, OnBottomPanelReadyForPreload);
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_EVENT, OnBottomPanelReadyForPreload);
            EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT, new EventData<GComponent>(PanelEvent.AnchorPanelChange, gOwnerPanel));

            // ---------- 5.音乐控制 ----------




            // ---------- 6.初始化FGUI组件 ----------
            GComponent loadWheelTip = contentPane.GetChild("wheelTip").asCom.GetChild("wheelTip").asCom;
            GComponent loadWheelTip2 = contentPane.GetChild("wheelTip").asCom.GetChild("wheelTip2").asCom;
            GComponent loadWheelTip3 = contentPane.GetChild("wheelTip").asCom.GetChild("wheelTip3").asCom;
            GComponent loadWheelEff = contentPane.GetChild("wheel").asCom.GetChild("wheelEffect").asCom;
            GComponent loadChangeIconAnim = contentPane.GetChild("anchorChangeIconAnim").asCom;
            GComponent loadNpc = contentPane.GetChild("anchorNpc").asCom;
            GComponent loadCollectEff = contentPane.GetChild("anchorCollectedTarget").asCom;

            wheelWinCredit = contentPane.GetChild("wheelWinCredit").asTextField;
            collectedNums = contentPane.GetChild("CollectedNums").asTextField;
            wheelSpinTimesTxt = contentPane.GetChild("wheelSpinTimes").asCom.GetChild("spinTimes").asTextField;

            freeTipLeft = contentPane.GetChild("freeLeft").asGroup;
            freeTipLeftNum = contentPane.GetChildInGroup(freeTipLeft, "goldBullNums").asTextField;

            freeTipRight = contentPane.GetChild("freeRight").asGroup;
            freeTipRightNum = contentPane.GetChildInGroup(freeTipRight, "collectNum").asTextField;
            GComponent loadDeerIcon = contentPane.GetChildInGroup(freeTipRight, "anchorDeer").asCom;
            GComponent loadWolfIcon = contentPane.GetChildInGroup(freeTipRight, "anchorWolf").asCom;
            GComponent loadLeopardIcon = contentPane.GetChildInGroup(freeTipRight, "anchorLeopard").asCom;
            GComponent loadEagleIcon = contentPane.GetChildInGroup(freeTipRight, "anchorEagle").asCom;
            targetIcon = contentPane.GetChild("targetIcon").asCom;

            enterFreeGame = contentPane.GetTransition("EnterFreeGame");

            freeRemainTimes = contentPane.GetChild("freeSpinTImes").asCom.GetChild("freeSpinTimes").asTextField;
            freeTotalTimes = contentPane.GetChild("freeSpinTImes").asCom.GetChild("freeTotalTimes").asTextField;
            freeTiggerInWheel = contentPane.GetTransition("FreeTigger");

            resetWheelTran = contentPane.GetTransition("WheelReset");
            showWheelTran = contentPane.GetTransition("ShowWheel");

            resetWheelTran.Play();

            GComponent loadAnchorJpBgEff = contentPane.GetChild("anchorJpEff").asCom;

            if (!isOpen) return;

            // ---------- 7.预制体挂到 FGUI 锚点 ----------

            //加速框部分
            if (ComReelEffect != null)
            {
                GameCommon.FguiUtils.DeleteWrapper(ComReelEffect);
                ComReelEffect.Dispose();
                ComReelEffect = null;
            }
            ComReelEffect = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            if(goFreeReelEffcetObj == null)
            {
                goFreeReelEffcetObj = GameObject.Instantiate(goFreeReelEffcetPre);
            }
            GameCommon.FguiUtils.AddWrapper(ComReelEffect, goFreeReelEffcetObj);
            ComReelEffect.visible = false;
            anchorExpectation = this.contentPane.GetChild("anchorReelEffect").asCom;
            anchorExpectation.AddChild(ComReelEffect);
            anchorExpectation.visible = true;

            //免费游戏中金牛时的移动特效部分
            if(ComRewardEffect1 != null)
            {
                GameCommon.FguiUtils.DeleteWrapper(ComRewardEffect1);
                ComRewardEffect1.Dispose();
                ComRewardEffect1 = null;
            }
            if(ComRewardEffect2 != null)
            {
                GameCommon.FguiUtils.DeleteWrapper(ComRewardEffect2);
                ComRewardEffect2.Dispose();
                ComRewardEffect2 = null;
            }
            if(ComRewardEffect3 != null)
            {
                GameCommon.FguiUtils.DeleteWrapper(ComRewardEffect3);
                ComRewardEffect3.Dispose();
                ComRewardEffect3 = null;
            }
            ComRewardEffect1 = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            ComRewardEffect2 = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            ComRewardEffect3 = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            if(goRewardEffectObj1 == null && goRewardEffectObj2 == null && goRewardEffectObj3 == null)
            {
                goRewardEffectObj1 = GameObject.Instantiate(goRewardEffectPre);
                goRewardEffectObj2 = GameObject.Instantiate(goRewardEffectPre);
                goRewardEffectObj3 = GameObject.Instantiate(goRewardEffectPre);
            }
            GameCommon.FguiUtils.AddWrapper(ComRewardEffect1, goRewardEffectObj1);
            GameCommon.FguiUtils.AddWrapper(ComRewardEffect2, goRewardEffectObj2);
            GameCommon.FguiUtils.AddWrapper(ComRewardEffect3, goRewardEffectObj3);
            ComRewardEffect1.visible = false;
            ComRewardEffect2.visible = false;
            ComRewardEffect3.visible = false;
            anchorFreeStart = contentPane.GetChild("anchorFreeEffect").asCom;
            anchorFreeStart.AddChild(ComRewardEffect1);
            anchorFreeStart.AddChild(ComRewardEffect2);
            anchorFreeStart.AddChild(ComRewardEffect3);
            anchorFreeStart.visible = true;

            smallGameReels = contentPane.GetChild("smallGameReels").asCom;
            jackpotTimes = contentPane.GetChild("jackpotGroup").asCom.GetChild("jackpotTimes").asTextField;

            #region 预制体和GComponent绑定
            if (npcAnchor != loadNpc)
            {
                GameCommon.FguiUtils.DeleteWrapper(npcAnchor);
                npcAnchor = loadNpc;
                npcObj = GameObject.Instantiate(npcPre);
                npcAnim = npcObj.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                shoutTrans = npcObj.transform.GetChild(1).GetChild(0);
                smokeTrans = npcObj.transform.GetChild(1).GetChild(1);
                GameCommon.FguiUtils.AddWrapper(npcAnchor, npcObj);
            }

            if (gWheelTip != loadWheelTip)
            {
                GameCommon.FguiUtils.DeleteWrapper(gWheelTip);
                gWheelTip = loadWheelTip;
                wheelTipObj = GameObject.Instantiate(wheelTip);
                wheelTipAnim = wheelTipObj.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                GameCommon.FguiUtils.AddWrapper(gWheelTip, wheelTipObj);
                wheelTipObj.SetActive(false);
            }

            if (gWheelTip2 != loadWheelTip2)
            {
                GameCommon.FguiUtils.DeleteWrapper(gWheelTip2);
                gWheelTip2 = loadWheelTip2;
                wheelTip2Obj = GameObject.Instantiate(wheelTip2);
                wheelTipAnim2 = wheelTip2Obj.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                GameCommon.FguiUtils.AddWrapper(gWheelTip2, wheelTip2Obj);
                wheelTip2Obj.SetActive(false);
            }

            if (gWheelTip3 != loadWheelTip3)
            {
                GameCommon.FguiUtils.DeleteWrapper(gWheelTip3);
                gWheelTip3 = loadWheelTip3;
                wheelTip3Obj = GameObject.Instantiate(wheelTip3);
                wheelTipAnim3 = wheelTip3Obj.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                GameCommon.FguiUtils.AddWrapper(gWheelTip3, wheelTip3Obj);
                wheelTip3Obj.SetActive(false);
            }

            if (gWheelEffect != loadWheelEff)
            {
                GameCommon.FguiUtils.DeleteWrapper(gWheelEffect);
                gWheelEffect = loadWheelEff;
                wheelEffObj = GameObject.Instantiate(wheelEffect);
                showTime = wheelEffObj.transform.GetChild(0);
                Win = wheelEffObj.transform.GetChild(1);
                Idle = wheelEffObj.transform.GetChild(2);
                waitSpin = wheelEffObj.transform.GetChild(3);
                GameCommon.FguiUtils.AddWrapper(gWheelEffect, wheelEffObj);
            }

            if(anchorChangeIconAnim != loadChangeIconAnim)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorChangeIconAnim);
                anchorChangeIconAnim = loadChangeIconAnim;
                changeIconAnim = GameObject.Instantiate(changeIconAnimPre);
                GameCommon.FguiUtils.AddWrapper(anchorChangeIconAnim, changeIconAnim);
                anchorChangeIconAnim.visible = false;

                StartChangeIcon = contentPane.GetTransition("StartChangeIcon");
                EndChangeIcon = contentPane.GetTransition("EndChangeIcon");
            }

            if(eagleIcon != loadEagleIcon)
            {
                GameCommon.FguiUtils.DeleteWrapper(eagleIcon);
                eagleIcon = loadEagleIcon;  
                eagleAnim = GameObject.Instantiate(eagleAnimPre);
                GameCommon.FguiUtils.AddWrapper(eagleIcon, eagleAnim);
                eagleAnim.SetActive(false);
            }

            if(leopardIcon != loadLeopardIcon)
            {
                GameCommon.FguiUtils.DeleteWrapper(leopardIcon);
                leopardIcon = loadLeopardIcon;  
                leopardAnim = GameObject.Instantiate(leopardAnimPre);
                GameCommon.FguiUtils.AddWrapper(leopardIcon, leopardAnim);
                leopardAnim.SetActive(false);
            }

            if(wolfIcon != loadWolfIcon)
            {
                GameCommon.FguiUtils.DeleteWrapper(wolfIcon);
                wolfIcon = loadWolfIcon;  
                wolfAnim = GameObject.Instantiate(wolfAnimPre);
                GameCommon.FguiUtils.AddWrapper(wolfIcon, wolfAnim);
                wolfAnim.SetActive(false);
            }

            if (deerIcon != loadDeerIcon)
            {
                GameCommon.FguiUtils.DeleteWrapper(deerIcon);
                deerIcon = loadDeerIcon;  
                deerAnim = GameObject.Instantiate(deerAnimPre);
                GameCommon.FguiUtils.AddWrapper(deerIcon, deerAnim);
                deerAnim.SetActive(false);
            }

            if(collectedTarget != loadCollectEff)
            {
                GameCommon.FguiUtils.DeleteWrapper(collectedTarget);
                collectedTarget = loadCollectEff;
                goCollectionEff = GameObject.Instantiate(goCollectionPre);
                freeCollectEff = goCollectionEff.transform.GetChild(0).GetChild(0);
                GameCommon.FguiUtils.AddWrapper(collectedTarget, goCollectionEff);
            }

            if (creditsReward == null)
            {
                creditsReward = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
                GameCommon.FguiUtils.DeleteWrapper(creditsReward);
                creditsEff = GameObject.Instantiate(creditsEffPrefab);
                GameCommon.FguiUtils.AddWrapper(creditsReward, creditsEff);
                creditsStart = contentPane.GetChild("anchorCreditStart").asCom;
                creditsTarget = contentPane.GetChild("anchorCreditTarget").asCom;
            }

            if (collectedReward == null)
            {
                collectedReward = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
                GameCommon.FguiUtils.DeleteWrapper(collectedReward);
                collectEff = GameObject.Instantiate(collectedEffPrefab);
                GameCommon.FguiUtils.AddWrapper(collectedReward, collectEff);
                collectedStart = contentPane.GetChild("anchorCollectedStart").asCom;
                
            }

            if(anchorJpBgEff != loadAnchorJpBgEff)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorJpBgEff);
                anchorJpBgEff = loadAnchorJpBgEff;
                anchorJpBgEffObj = GameObject.Instantiate(anchorJpBgEffPre);
                anchorJpBgAnim = anchorJpBgEffObj.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                GameCommon.FguiUtils.AddWrapper(anchorJpBgEff, anchorJpBgEffObj);
            }

            #endregion

            MainBlackboardController.Instance.SyncMyTempCreditToReal(true);

        }

        private void EnsureMainPagSlot()
        {
            //GComponent anchor = contentPane.GetChild("anchorJpPag")?.asCom;
            //if (anchor == null) return;

            //if (effectPag == null)
            //    effectPag = new PagSlotBinding("JpBg", GamePagFolder);
            //effectPag.EnsureSlot(anchor, "pagEffect");
            //GLoader anchorPag = anchor.GetChild("pagEffect").asLoader;
        }

        private void OnCoinPushSpinResultParse(CoinPushSpinParseEventArgs e)
        {
            e.Result = MachineDataG3995Controller.ParseCoinPushSpinPayload(e.Data, e.StartPos);
        }

        //private void ReadJsonBet()
        //{
        //    //资源加载
        //    ResourceManager02.Instance.LoadAsset<TextAsset>(
        //        "Assets/GameRes/_Common/Game Maker/ABs/G3995/Data/game_info_g3995.json", (txt) =>
        //        {
        //            //JSON解析与错误处理
        //            GameConfigRoot config = JsonConvert.DeserializeObject<GameConfigRoot>(txt.text);
        //            if (config?.SymbolPaytable == null)
        //            {
        //                Debug.LogError("解析symbol_paytable失败，数据为空");
        //                return;
        //            }

        //            MainModel.Instance.gameID = config.GameId;
        //            MainModel.Instance.gameName = config.GameName;
        //            MainModel.Instance.displayName = config.DisplayName;

        //            //赢钱倍数处理
        //            foreach (var item in config.WinLevelMultiple)
        //            {
        //                string winKey = item.Key;
        //                long winValue = item.Value;
        //                CustomModel.Instance.winLevelMultiple.Add(new WinMultiple(winKey, winValue));
        //            }

        //            //轮盘数据存储
        //            wheelCredit.Clear();
        //            foreach (var item in config.InitalWheel)
        //            {
        //                // 万位代表种类 1:金牛,2:奖金 3:免费游戏。千百十个位是携带的个数或者金额
        //                int count = item % 10000;
        //                int kind = item / 10000;

        //                if (kind == 2)
        //                {
        //                    wheelCredit.Add(count);
        //                }
        //            }

        //            //符号支付表处理
        //            foreach (var kvp in config.SymbolPaytable)
        //            {
        //                string symbolKey = kvp.Key; // 如 "s0"、"s1"、"s2"
        //                var jsonData1 = kvp.Value; // 对应x3、x4、x5的数据

        //                // 1. 从symbolKey中提取索引（如"s0" → 0，"s1" → 1）
        //                if (int.TryParse(symbolKey.Replace("s", ""), out int index))
        //                {
        //                    // 2. 检查索引是否在列表有效范围内
        //                    if (index >= 0)
        //                    {
        //                        // 3. 为列表中对应索引的元素赋值
        //                        var targetItem = CustomModel.Instance.payTableSymbolWin[index];
        //                        targetItem.x3 = jsonData1.x3; // 假设jsonData的属性是X3（根据实际定义调整）
        //                        targetItem.x4 = jsonData1.x4;
        //                        targetItem.x5 = jsonData1.x5;
        //                        // 若需要同步symbol字段（可选，确保一致）
        //                        targetItem.symbol = index;
        //                    }
        //                }
        //                else
        //                {
        //                    Debug.LogWarning($"无效的符号键：{symbolKey}，无法解析索引");
        //                }
        //            }

        //            //支付线处理
        //            if (ContentModel.Instance.payLines == null)
        //            {
        //                ContentModel.Instance.payLines = new List<List<int>>() { };
        //            }
        //            foreach (var item in config.pay_lines)
        //            {
        //                ContentModel.Instance.payLines.Add(item);
        //            }
        //        });
        //}

        void StartGameOnce(Action successCallback = null, Action<string> errorCallback = null)
        {
            ContentModel.Instance.totalPlaySpins = 1;
            ContentModel.Instance.remainPlaySpins = 1;
            corGameOnce = mono.StartCoroutine(GameOnce(successCallback, errorCallback));
        }

        void StartGameAuto(Action successCallback = null, Action<string> errorCallback = null)
        {
            mono.StartCoroutine(GameAuto(successCallback, errorCallback));
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


        //判断普通赢分时公牛播放的动画
        bool playWin = false;

        //中奖其他奖励时存储普通游戏的临时赢分
        long tempWin = 0;

        private IEnumerator GameOnce(Action successCallback, Action<string> errorCallback)
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
            ContentModel.Instance.betNum = (int)ContentModel.Instance.totalBet;
            slotMachineCtrl.BeginTurn();
            playWin = false;
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
            //普通赢
            if (winList.Count > 0)
            {
                playWin = true;
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
                tempWin += allWinCredit;

                #region 普通赢分播放动画
                foreach(SymbolWin sw in winList)
                {
                    if(sw.symbolNumber > 5)
                    {
                        playWin = true;
                        break;
                    }
                }

                if (!playWin)
                {
                    PlayAnim(npcAnim, "win1");
                }
                else
                {
                    PlayAnim(npcAnim, "win2");
                }

                #endregion

                yield return slotMachineCtrl.ShowSymbolWinBySetting(slotMachineCtrl.GetTotalSymbolWin(winList), true, PusherEmperorsRein.SpinWinEvent.TotalWinLine);

                ////检查bigwin类型
                WinLevelType winLevelType = GetBigWinType();
                ////bigwin弹窗
                if (winLevelType != WinLevelType.None)
                {
                    //显示全部中奖图标和中奖线
                    slotMachineCtrl.ShowSymbolWinDeck(slotMachineCtrl.GetTotalSymbolWin(winList), true);
                    //bigwin弹窗
                    yield return BigWinPopup(winLevelType, ContentModel.Instance.baseGameWinCredit);
                }
                else
                {
                    // 普通赢钱处理
                    bool isAddToCredit = totalWinLineCredit > ContentModel.Instance.totalBet * 4;
                    slotMachineCtrl.SendPrepareTotalWinCreditEvent(totalWinLineCredit, isAddToCredit);
                }

                //积分同步和退币处理
                slotMachineCtrl.SendTotalWinCreditEvent(allWinCredit);

                //加钱动画
                //MainBlackboardController.Instance.AddMyTempCredit(totalWinLineCredit, true, isAddCreditAnim);
            }
            #endregion

            // 即中即退
            // yield return CoinOutImmediately(allWinCredit);


            //免费奖
            if (ContentModel.Instance.isFreeSpinTrigger)
            {
                //显示中奖动画
                slotMachineCtrl.SkipWinLine(true);
                slotMachineCtrl.ShowSymbolEffect(TagPoolObject.SymbolHit, new List<int>() { 12 }, true, 12, true);
                yield return slotMachineCtrl.SlotWaitForSeconds(1f);
                isNext = false;

                slotMachineCtrl.SkipWinLine(true);
                yield return FreeSpinTrigger(() => isNext = true, errorCallback);

                yield return new WaitUntil(() => isNext == true);
                isNext = false;
            }

            //中游戏大奖
            if (ContentModel.Instance.isJackpotSpinTrigger)
            {
                if (winList.Count > 0)
                {
                    yield return new WaitForSeconds(1);
                }
                isNext = false;

                //显示中奖动画
                slotMachineCtrl.SkipWinLine(true);
                slotMachineCtrl.ShowSymbolEffect(TagPoolObject.SymbolHit, new List<int>() { 13 }, true, 13, true);
                yield return slotMachineCtrl.SlotWaitForSeconds(1.5f);

                //播放动画
                slotMachineCtrl.SkipWinLine(true);

                yield return new WaitForSeconds(1.8f);

                yield return SmallGameTrigger(() => isNext = true);

                yield return new WaitUntil(() => isNext == true);
                isNext = false;
            }


            // 本剧同步玩家金钱
            MainBlackboardController.Instance.SyncMyTempCreditToReal(true);


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



        IEnumerator FreeSpinTrigger(Action successCallback, Action<string> errorCallback)
        {
            FreeGameInit();

            slotMachineCtrl.SkipWinLine(false);
            slotMachineCtrl.CloseSlotCover();

            wheelTipObj.SetActive(true);
            PlayAnim(wheelTipAnim, "Wheel_in");

            yield return new WaitForSeconds(2f);

            PlayAnim(wheelTipAnim, "Wheel_out");

            yield return new WaitForSeconds(1f);

            wheelTipObj.SetActive(false);
            wheelTip2Obj.SetActive(true); 
            PlayAnim(wheelTipAnim2, "Collect_in");
            isStartSpin = false;
            isEndSpin = false;
            mono.updateHandle.AddListener(WheelTrun);
            PlayAnim(npcAnim, "Summoning Wheel");
            PlayEffectAnim(shoutTrans);

            yield return new WaitForSeconds(1f);
            StopEffectAnim(shoutTrans);
            freeTiggerInWheel.Play();
            wheelSpinTimesTxt.text = "0";
            isWheelSpin = true;

            //轮盘掉落
            showWheelTran.Play();
            yield return new WaitForSeconds(0.4f);
            PlayEffectAnim(showTime);
            yield return new WaitForSeconds(0.2f);
            StopEffectAnim(showTime);
            PlayEffectAnim(Idle);
            PlayEffectAnim(waitSpin);

            ContentModel.Instance.isSpin = false;
            ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
            ContentModel.Instance.gameState = GameState.Idle;

            yield return new WaitUntil(() => isStartSpin == true);

            PlayAnim(wheelTipAnim2, "Collect_out");

            yield return new WaitForSeconds(1f);
            wheelTip2Obj.SetActive(false);

            yield return new WaitUntil(() => isEndSpin == true);
            collectedNums.alpha = 1;
            wheelWinCredit.alpha = 1;

            wheelTip3Obj.SetActive(true); 
            PlayAnim(wheelTipAnim3, "3X_in");

            yield return new WaitUntil(() => isWheelSpin == false);

            yield return new WaitForSeconds(0.2f);

            bool isNext = false;

            StopEffectAnim(Win);
            freeTotalTimes.text = ContentModel.Instance.freeSpinTotalTimes.ToString();
            freeRemainTimes.text = ContentModel.Instance.freeSpinPlayTimes.ToString();

            PageManager.Instance.OpenPageAsync(PageName.HuoYanGongNiuPopupFreeSpinTrigger,
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


            InputStackContextFreeSpin((context) =>
            {
            });

            ChangeBGPanel(1);
            enterFreeGame.Play();
            collectedNums.alpha = 1;
            wheelWinCredit.alpha = 1;
            yield return new WaitForSeconds(2);


            #region 如果有分数和金牛就先放动画在退场
            if (wheelOnceWin != 0)
            {
                creditsReward.visible = true;

                Vector2 startPos = TransFormParentNode(creditsReward, creditsTarget);
                if (creditsReward.parent != null) creditsReward.parent.RemoveChild(creditsReward);
                creditsTarget.AddChild(creditsReward);
                creditsReward.xy = startPos;

                yield return MoveToZeroOverTime(creditsReward, startPos, 0.7f, () =>
                {
                    tempWin += wheelOnceWin;
                    slotMachineCtrl.SendTotalWinCreditEvent(tempWin);
                    creditsReward.visible = false;
                });

                yield return new WaitForSeconds(0.3f);
            }

            if (wheelWinGoldBull != 0)
            {
                collectedReward.visible = true;

                Vector2 startPos = TransFormParentNode(collectedReward, collectedTarget);
                if (collectedReward.parent != null) collectedReward.parent.RemoveChild(collectedReward);
                collectedTarget.AddChild(collectedReward);
                collectedReward.xy = startPos;

                yield return MoveToZeroOverTime(collectedReward, startPos, 0.8f, () =>
                {
                    PlayEffectAnim(freeCollectEff);
                    freeTipLeftNum.text = wheelWinGoldBull.ToString();
                    collectedReward.visible = false;
                });

                if(wheelWinGoldBull >= ContentModel.Instance.goldBullNums[0])
                {
                    StartChangeIcon.Play();

                    yield return new WaitForSeconds(0.85f);

                    anchorChangeIconAnim.visible = true;
                    int addTimes = wheelWinGoldBull / 4;
                    while(ContentModel.Instance.stageIndex < addTimes)
                    {
                        ContentModel.Instance.stageIndex++;
                        ShowChangeIcon();
                    }

                    yield return new WaitForSeconds(1.3f);

                    anchorChangeIconAnim.visible = false;
                    EndChangeIcon.Play();
                    yield return new WaitForSeconds(0.85f);
                }

                freeTipRightNum.text = (ContentModel.Instance.goldBullNums[ContentModel.Instance.stageIndex] - wheelWinGoldBull).ToString();
            }

            #endregion

            collectedNums.alpha = 0;
            wheelWinCredit.alpha = 0;
            PlayAnim(wheelTipAnim3, "3X_out");
            StopEffectAnim(Win);
            yield return new WaitForSeconds(0.85f);
            wheelTip3Obj.SetActive(false);

            slotMachineCtrl.BeginBonusFreeSpin();

            yield return GameFreeSpin(null, errorCallback);


            resetWheelTran.Play(); 
            freeTotalTimes.text = ContentModel.Instance.freeSpinTotalTimes.ToString();
            freeRemainTimes.text = (ContentModel.Instance.freeSpinPlayTimes + 1).ToString();

            PageManager.Instance.OpenPageAsync(PageName.HuoYanGongNiuPopupFreeSpinExit,
                new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object>()
                    {
                        ["baseGameWinCredit"] = tempWin,
                    }),
                (ed) =>
                {
                    DebugUtils.Log("回调执行！isNext = true"); // 加日志
                    isNext = true;
                });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

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

            ChangeBGPanel(0); 
            PlayAnim(npcAnim, "idle");

            slotMachineCtrl.SkipWinLine(true);
            successCallback?.Invoke();
        }


        //开始免费游戏
        IEnumerator GameFreeSpin(Action successCallback, Action<string> errorCallback)
        {
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

            ContentModel.Instance.gameState = GameState.FreeSpin;

            freeTotalTimes.text = ContentModel.Instance.freeSpinTotalTimes.ToString();
            freeRemainTimes.text = (ContentModel.Instance.freeSpinPlayTimes + 1).ToString();

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

            ContentModel.Instance.haveFreeSpecialIcon = ContentModel.Instance.SpecialBullIcon.Count > 0;

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
                yield return ShowGoldBullDestroy(ContentModel.Instance.SpecialBullIcon);
                yield return new WaitForSeconds(1f);
            }

            List<SymbolWin> winList = ContentModel.Instance.winList;
            long allWinCredit = 0;


            #region Win

            if (winList.Count > 0)
            {
                long totalWinLineCredit = slotMachineCtrl.GetTotalWinCredit(winList);
                if (ContentModel.Instance.newFreeOnceCredit.Count > ContentModel.Instance.freeSpinPlayTimes - 1)
                {
                    totalWinLineCredit = ContentModel.Instance.newFreeOnceCredit[ContentModel.Instance.freeSpinPlayTimes - 1];
                }

                tempWin += totalWinLineCredit;


                if (winList.Count > 0)
                {
                    slotMachineCtrl.SkipIdle(true);
                    slotMachineCtrl.SkipWinLine(true);
                    yield return slotMachineCtrl.ShowSymbolWinBySetting(slotMachineCtrl.GetTotalSymbolWin(winList), true, PusherEmperorsRein.SpinWinEvent.TotalWinLine);
                }

                // 播大奖弹窗
                WinLevelType winLevelType = GetBigWinType();
                if (winLevelType != WinLevelType.None)
                {
                    slotMachineCtrl.ShowSymbolWinDeck(slotMachineCtrl.GetTotalSymbolWin(winList), true);

                    // 大奖弹窗
                    yield return BigWinPopup(winLevelType, ContentModel.Instance.baseGameWinCredit);

                    slotMachineCtrl.CloseSlotCover();

                    slotMachineCtrl.SkipWinLine(false);
                }
                else
                {
                    // 总线赢分（同步？？）
                    bool isAddToCredit = totalWinLineCredit > TotalBet * 4;
                    slotMachineCtrl.SendPrepareTotalWinCreditEvent(totalWinLineCredit, isAddToCredit);
                }



                // 总线赢分事件
                slotMachineCtrl.SendTotalWinCreditEvent(tempWin);

                //加钱动画
                //MainBlackboardController.Instance.AddMyTempCredit(totalWinLineCredit, true, isAddCreditAnim);
                ContentModel.Instance.freeOnceCredit = totalWinLineCredit;

            }

            #endregion

            /* 先结算“免费游戏”或“小游戏”再回主游戏结算主游戏，则每局不能同步玩家真实金钱金额
           MainBlackboardController.Instance.SyncMyCreditToReal(false);*/

            if (successCallback != null)
                successCallback.Invoke();
        }


        void OnGameReset()
        {
            if (corGameIdel != null) mono.StopCoroutine(corGameIdel);
            if (corEffectSlowMotion != null) mono.StopCoroutine(corEffectSlowMotion);

            slotMachineCtrl.isStopImmediately = false;
            slotMachineCtrl.CloseSlotCover();
            isStoppedSlotMachine = false;

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
            int totalTimes = 0;
            while (i < totalTimes && !slotMachineCtrl.isStopImmediately)
            {
                i++;
                yield return slotMachineCtrl.ShowSymbolWinBySetting(slotMachineCtrl.GetTotalSymbolWin(winList), true, PusherEmperorsRein.SpinWinEvent.TotalWinLine);
            }
            yield return slotMachineCtrl.ShowWinListAwayDuringIdle(winList);
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
            slotMachineCtrl.SkipWinLine(false);

            bool isNext = false;
            PageManager.Instance.OpenPageAsync(PageName.HuoYanGongNiuPopupBigWin,
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
            MachineDataG3995Controller.Instance.RequestSlotSpinFromMock(totalBet, (res) =>
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

            // 解析数据
            MachineDataG3995Controller.Instance.ParseSlotSpin(totalBet, resNode, sboxJackpotData);

            // 数据入库

            // 游戏彩金滚轮
            //SetUIJackpotGameReel();

            if (successCallback != null)
                successCallback.Invoke();
        }


        //请求算法结果
        IEnumerator RequestSlotSpinFromMachine(Action successCallback = null, Action<string> errorCallback = null)
        {
            Debug.Log("请求算法结果");
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
            MachineDataG3995Controller.Instance.ParseSlotSpin(totalBet, resNode, sboxJackpotData);

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


        ////显示加速框
        public IEnumerator ShowEffectReelsSlowMotion(int colIdx)
        {
            ComReelEffect.visible = false;
            ComReelEffect.xy = slotMachineCtrl.SymbolCenterToNodeLocalPos(colIdx, 1, anchorExpectation);
            ComReelEffect.visible = true;
            // GameSoundHelper.Instance.PlaySoundEff(SoundKey.SlowMotionEffect);

            yield return new WaitUntil(() => isStoppedSlotMachine == true);
            // 关闭Expectation
            ComReelEffect.visible = false;
        }


        int rewardEffectIndex = 0;
        long allWinCredit = 0;
        //显示中奖后飞行粒子特效
        public IEnumerator ShowRewardEffect(int colIdx, int rowIdx, GComponent toNode)
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

                yield return MoveToZeroOverTime(rewardEffect, slotMachineCtrl.SymbolCenterToNodeLocalPos(colIdx, rowIdx, toNode));
            }

            //记录并显示累计分数
            allWinCredit += ContentModel.Instance.jackpotSpinWinCredit;
            slotMachineCtrl.SendTotalWinCreditEvent(allWinCredit);
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

            successCallback?.Invoke();
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
            if(particle != null) particle.Play();

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
            if (particle != null)
            {
                particle.Stop(true);
                particle.Clear();
            }

            // 递归播放所有子物体的粒子系统
            foreach (Transform child in effect)
            {
                StopEffectAnim(child);
            }
        }



        private void FreeGameInit()
        {
            wheelSpinTimes = 0;
            wheelWinGoldBull = 0;
            wheelOnceWin = 0;
            ContentModel.Instance.stageIndex = 0;
            InitWheelItem();

            eagleIcon.GetChild("example").asLoader.url = "ui://x2aorbwjq4s82k";
            eagleAnim.SetActive(false);
            leopardIcon.GetChild("example").asLoader.url = "ui://x2aorbwjq4s82i";
            leopardAnim.SetActive(false);
            wolfIcon.GetChild("example").asLoader.url = "ui://x2aorbwjq4s82j";
            wolfAnim.SetActive(false);
            deerIcon.GetChild("example").asLoader.url = "ui://x2aorbwjq4s82h";
            deerAnim.SetActive(false);
            targetIcon.GetChild("example").asLoader.url = "ui://x2aorbwjq4s824";

            creditsReward.visible = false;
            if (creditsReward.parent != null) creditsReward.parent.RemoveChild(creditsReward);
            creditsStart.AddChild(creditsReward);
            creditsReward.xy = Vector2.zero;

            collectedReward.visible = false;
            if (collectedReward.parent != null) collectedReward.parent.RemoveChild(collectedReward);
            collectedStart.AddChild(collectedReward);
            collectedReward.xy = Vector2.zero;

            wheelWinCredit.text = wheelOnceWin.ToString();
            collectedNums.text = wheelWinGoldBull.ToString(); 
            freeTipLeftNum.text = wheelWinGoldBull.ToString();
            freeTipRightNum.text = ContentModel.Instance.goldBullNums[ContentModel.Instance.stageIndex].ToString();
        }

        #region 转盘相关的方法
        //初始化/重置 轮盘元素
        private void ResetWheel()
        {
            gBulls.Clear();
            gTexts.Clear();
            for (int i = 0; i < 12; i++)
            {
                if (i % 2 == 1)
                {
                    GTextField gText = gWheel.asCom.GetChild("item" + i).asTextField;
                    gText.text = CustomModel.Instance.wheelCredit[0][i / 2].ToString();
                    gTexts.Add(gText);
                }
                else if (i % 4 == 0)
                {
                    GLoader goldBull = gWheel.asCom.GetChild("item" + i).asLoader;
                    goldBull.url = CustomModel.Instance.wheelGoldBull[0];
                    gBulls.Add(goldBull);
                }
            }
        }


        private void OnClickWheelSpinBtn()
        {
            if (!isWheelSpin)
            {
                return;
            }
            EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, false); // isLongClick
            OnClickSpinButton(res);
        }


        private void StartWheelSpinOnce(int targetIndex)
        {
            Action successCallback = () =>
            {
                ContentModel.Instance.isSpin = false;
                ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
                ContentModel.Instance.gameState = GameState.Idle;
                StopEffectAnim(Idle);
                StopEffectAnim(waitSpin);
                PlayEffectAnim(Win);

                isEndSpin = true;

                if (targetIndex % 2 == 1)
                {
                    wheelOnceWin += CustomModel.Instance.wheelCredit[wheelSpinTimes - 1][targetIndex / 2] * ContentModel.Instance.betNum;
                    wheelWinCredit.text = wheelOnceWin.ToString();
                }
                if (targetIndex % 4 == 0)
                {
                    wheelWinGoldBull += wheelSpinTimes;
                    collectedNums.text = wheelWinGoldBull.ToString();
                }
                if (wheelSpinTimes >= ContentModel.Instance.wheelData.Count)
                {
                    ContentModel.Instance.isSpin = true;
                    ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                    isWheelSpin = false;
                }
            };

            InitWheelItem();
            wheelSpinTimes++;
            wheelSpinTimesTxt.text = wheelSpinTimes.ToString();

            isStartSpin = true;
            if (corWheelSpin != null) mono.StopCoroutine(corWheelSpin);
            corWheelSpin = mono.StartCoroutine(SpinWheelToTarget(targetIndex, successCallback, null));
        }

        private void InitWheelItem()
        {
            foreach (GLoader item in gBulls)
            {
                item.url = CustomModel.Instance.wheelGoldBull[(wheelSpinTimes) % 5];
            }

            for(int i = 0; i < gTexts.Count; i++)
            {
                gTexts[i].text = (CustomModel.Instance.wheelCredit[(wheelSpinTimes)][i] * ContentModel.Instance.betNum).ToString();
            }
        }


        //转盘转速控制
        private float rotateSpeed = 8;
        //转盘转动控制
        private void WheelTrun()
        {
            gWheel.rotation += rotateSpeed * Time.deltaTime;
            if (gWheel.rotation >= 360)
            {
                gWheel.rotation = 0;
            }
        }



        //轮盘旋转方法
        private IEnumerator SpinWheelToTarget(int targetIndex, Action successCallback, Action<string> errorCallback = null)
        {
            float currentAngle = NormalizeAngle(gWheel.rotation);
            float targetAngleCenter = 360 - (targetIndex * segmentAngle);

            int minCircles = 2;
            int extraCircles = UnityEngine.Random.Range(3, 7);
            int totalCircles = minCircles + extraCircles;

            // ========================
            // 重点：彻底删掉 +10，纯公式
            // ========================
            float totalRotation = totalCircles * 360f + (targetAngleCenter - currentAngle);
            if (totalRotation < 0) totalRotation += 360f;

            float speed = 100f;
            float maxSpeed = 1280f;
            float accelerateTime = 1f;
            float decelerateTime = 2f;

            float elapsed = 0f;
            float rotated = 0f;

            // 阶段1：加速（原样）
            while (elapsed < accelerateTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / accelerateTime;
                speed = Mathf.Lerp(100f, maxSpeed, t * t);

                float deltaRot = speed * Time.deltaTime;
                gWheel.rotation += deltaRot;
                rotated += deltaRot;

                yield return null;
            }

            speed = maxSpeed;

            // 阶段2：匀速（原样）
            float accelerateDistance = 0.5f * (100f + maxSpeed) * accelerateTime;
            float decelerateDistance = 0.5f * maxSpeed * decelerateTime;
            float constantDistance = totalRotation - accelerateDistance - decelerateDistance;
            float constantTime = constantDistance / maxSpeed;

            elapsed = 0f;
            while (elapsed < constantTime)
            {
                elapsed += Time.deltaTime;
                float deltaRot = speed * Time.deltaTime;
                gWheel.rotation += deltaRot;
                rotated += deltaRot;

                yield return null;
            }

            // ================================
            // 阶段3：匀减速 → 但最后自动对齐
            // ================================
            float remainingRotation = totalRotation - rotated;
            float startSpeed = speed;
            float deceleration = (startSpeed * startSpeed) / (2 * remainingRotation);

            // 先减速到速度很低，但不追求完全走完
            while (speed > 200f)
            {
                speed -= deceleration * Time.deltaTime;
                float deltaRot = speed * Time.deltaTime;

                gWheel.rotation += deltaRot;
                remainingRotation -= deltaRot;

                yield return null;
            }

            // ================================
            // 关键：剩下角度平滑滑过去（跨设备稳定）
            // ================================
            float slideTime = 0.8f;
            float slideElapsed = 0f;
            float startRot = gWheel.rotation;
            float targetRot = startRot + remainingRotation;

            while (slideElapsed < slideTime)
            {
                slideElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(slideElapsed / slideTime);

                // 关键：这个曲线就是真实转盘“越转越慢”的效果
                t = 1 - Mathf.Pow(1 - t, 3); // 缓动曲线：Out Cubic（最强物理感）

                gWheel.rotation = Mathf.Lerp(startRot, targetRot, t);
                yield return null;
            }

            gWheel.rotation = targetRot;

            successCallback?.Invoke();
        }

        // 辅助函数：规范化角度到0-360
        private float NormalizeAngle(float angle)
        {
            angle %= 360f;
            if (angle < 0) angle += 360f;
            return angle;
        }
        #endregion

        public Vector2 TransFormParentNode(GComponent node, GComponent targetNode)
        {
            Vector2 worldPos = node.LocalToGlobal(Vector2.zero);
            return targetNode.GlobalToLocal(worldPos);
        }




        private IEnumerator ShowGoldBullDestroy(List<Cell> symbolWin)
        {
            //停止特效显示
            slotMachineCtrl.SkipWinLine(false);

            //// 立马停止时，不播放赢分环节？
            //if (isStopImmediately && _spinWEMD.Instance.isSkipAtStopImmediately)
            //    yield break;


            foreach (Cell cel in symbolWin)
            {
                Symbol01 symble = (Symbol01)slotMachineCtrl.GetVisibleSymbolFromDeck(cel.column, cel.row);

                int symbolNumber = symble.number;

                string symbolName = CustomModel.Instance.symbolHitEffect[$"{symbolNumber}"];  // wild  or symbol;

                // 图标动画  
                GComponent goSymbolHit = fguiPoolHelper.GetObject(TagPoolObject.SymbolHit, symbolName).asCom;
                symble.AddSymbolEffect(goSymbolHit, false);

                yield return new WaitForSeconds(1f);
                wheelWinGoldBull++;
                freeTipLeftNum.text = wheelWinGoldBull.ToString();
                AddFreeRight();

                slotMachineCtrl.SkipWinLine(false);
                symble.SetSymbolImage(10);

                while (ContentModel.Instance.stageIndex < 4 && ContentModel.Instance.goldBullNums[ContentModel.Instance.stageIndex] - wheelWinGoldBull <= 0)
                {
                    StartChangeIcon.Play();

                    yield return new WaitForSeconds(0.85f);

                    anchorChangeIconAnim.visible = true;
                    ContentModel.Instance.stageIndex++;
                    ShowChangeIcon();
                    yield return new WaitForSeconds(1.85f);

                    AddFreeRight();

                    anchorChangeIconAnim.visible = false;
                    
                    EndChangeIcon.Play();
                    yield return new WaitForSeconds(0.85f);
                }
            }

        }


        /// <summary>3995：底部 Panel 异步就绪后触发 PageManager 的 preLoadedCallback。</summary>
        private void OnBottomPanelReadyForPreload(EventData res)
        {
            if (res == null || res.name != PanelEvent.BottomPanelReady)
                return;

            int gameId = Convert.ToInt32(res.value);
            if (gameId != 3995)
                return;

            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_EVENT, OnBottomPanelReadyForPreload);
            _isBottomPanelReady = true;
            TryNotifyPagePreloaded();
        }

        /// <summary>底部 Panel 与对象池均就绪后，才通知 Loading 本页预加载完成。</summary>
        private void TryNotifyPagePreloaded()
        {
            if (!_isBottomPanelReady || !_isPoolPreloadDone) return;
            if (_hasNotifiedPagePreloaded) return;
            _hasNotifiedPagePreloaded = true;
            preLoadedCallback?.Invoke();
        }



        private void ShowChangeIcon()
        {
            switch (ContentModel.Instance.stageIndex - 1) 
            {
                case 0:
                    eagleIcon.GetChild("example").asLoader.url = "ui://x2aorbwjq4s824";
                    eagleAnim.SetActive(true);
                    targetIcon.GetChild("example").asLoader.url = "ui://x2aorbwjq4s820";
                    break;
                case 1:
                    leopardIcon.GetChild("example").asLoader.url = "ui://x2aorbwjq4s820";
                    leopardAnim.SetActive(true);
                    targetIcon.GetChild("example").asLoader.url = "ui://x2aorbwjq4s82e";
                    break;
                case 2:
                    wolfIcon.GetChild("example").asLoader.url = "ui://x2aorbwjq4s82e";
                    wolfAnim.SetActive(true);
                    targetIcon.GetChild("example").asLoader.url = "ui://x2aorbwjq4s82d";
                    break;
                case 3:
                    deerIcon.GetChild("example").asLoader.url = "ui://x2aorbwjq4s82d";
                    deerAnim.SetActive(true);
                    break;
                default: 
                    break;
            }

        }

        private void AddFreeRight()
        {
            if (ContentModel.Instance.stageIndex < 4)
            {
                freeTipRightNum.text = (ContentModel.Instance.goldBullNums[ContentModel.Instance.stageIndex] - wheelWinGoldBull).ToString();
            }
            else
            {
                freeTipRightNum.text = string.Empty;
            }
        }



        private GComponent smallGameReels;

        private readonly string _moneyUrl = "ui://HuoYanGongNiu_3995/ng_sym_Bonus_grand";

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
            "ui://CaiFuHuoChe_3996/ng_sym_Bonus_major",
            "ui://CaiFuHuoChe_3996/ng_sym_Bonus_minor",
            "ui://CaiFuHuoChe_3996/ng_sym_Bonus_mini",
        };


        private IEnumerator SmallGameTrigger(Action successCallback = null)
        {
            ContentModel.Instance.jackpotSpinWinCredit = 0;
            allWinCredit = 0;
            slotMachineCtrl.BeginBonusFreeSpin();
            InitSmallGame();
            bool isNext = false;

            PlayAnim(npcAnim, "bonus");
            yield return new WaitForSeconds(0.5f);

            PlayEffectAnim(smokeTrans);

            yield return new WaitForSeconds(6f);
            StopEffectAnim(smokeTrans);

            PageManager.Instance.OpenPageAsync(PageName.HuoYanGongNiuPopupJackpotTrigger,
                new EventData<Dictionary<string, object>>("", new Dictionary<string, object>()
                {
                    ["SpinTimes"] = 3,
                    ["Callback"] = new Action(() =>
                    {
                        ChangeBGPanel(2);
                        jackpotTimes.text = ContentModel.Instance.jackpotSpinTotalTimes.ToString();
                    })
                }),
            (ed) =>
            {
                Debug.Log("回调执行！isNext = true"); // 加日志
                isNext = true;
            });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            //effectPag.StopWithDefaults();
            //effectPag.Play(new PagSequencePlay(PagPlaySpecs.IntroLoop(stageName[0], stageName[1]), PagPlayLayout.Center, useGpuSyncGroup: false));

            PlayAnim(anchorJpBgAnim, "in");

            yield return new WaitForSeconds(3f);

            //------------------------  此处补充正式游戏 和 结算分数逻辑  ------------------------
            yield return SmallGameSpin(() => isNext = true);

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            slotMachineCtrl.SkipIdle(true);
            slotMachineCtrl.SkipWinLine(true);

            //effectPag.StopWithDefaults();
            anchorJpBgAnim.Rebind();
            anchorJpBgAnim.Update(0f);

            PageManager.Instance.OpenPageAsync(PageName.HuoYanGongNiuPopupJackpotExit,
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

            ChangeBGPanel(0);

            successCallback?.Invoke();
        }

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
                int index = strNum[i];

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

            int type = currentBet / 10000;
            int value = currentBet % 10000;

            if (type < 4)
            {
                info.type = SmallResultType.Money;
                info.rewardValue = value * ContentModel.Instance.betmultiple;
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

        private IEnumerator SmallGameSpin(Action successCallback = null)
        {
            yield return SmallGameLoop();
            yield return new WaitForSeconds(0.5f);
            yield return SmallGameResult();
            successCallback?.Invoke();
        }

        private void UpdateRollCountUI(int count)
        {
            if (jackpotTimes != null)
                jackpotTimes.text = count.ToString();
        }


        private int GetJackpotValue(int jackpotType, Dictionary<int, int> jackpotSocre)
        {
            return jackpotSocre[jackpotType];
        }

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

        private double CalculateRevealRate()
        {
            double rate = 0.7;
            rate += (3 - _remainingRolls) * 0.1;

            if (_unrevealedHits.Count <= 2)
                rate += 0.2;

            return Math.Min(1.0, rate);
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
                    yield return ShowJackpotSettlement(SmallResultType.Money, index, t.iconUrl, t.rewardText, creditsTarget, t.col, t.row);
                }
                else if (t.type == SmallResultType.Jackpot)
                {
                    yield return ShowJackpotSettlement(SmallResultType.Jackpot, index, t.iconUrl, t.rewardText, creditsTarget, t.col, t.row);
                    bool isNext = false;
                    PageManager.Instance.OpenPageAsync(PageName.HuoYanGongNiuPopupJackpotResult,
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
                tempWin += long.Parse(rewardText);
                slotMachineCtrl.SendTotalWinCreditEvent(tempWin);
            }
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
            }

            yield return new WaitUntil(() => isFinish);
            yield return new WaitForSeconds(0.5f);

        }
    }
}