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
using UnityEngine;
using Object = UnityEngine.Object;
using _spinWEMD = SlotMaker.SpinWinEffectSettingModel;

namespace CaiFuZhiJia_3997
{
    public class GameConfigRoot
    {
        [JsonProperty("game_id")] public int GameId; //游戏 ID

        [JsonProperty("game_name")] public string GameName; //名称

        [JsonProperty("display_name")] public string DisplayName; //显示名称

        [JsonProperty("line_num")] public int LineNum; //线数

        [JsonProperty("win_level_multiple")] public Dictionary<string, long> WinLevelMultiple { get; set; } //赢钱倍数

        [JsonProperty("symbol_paytable")]
        public Dictionary<string, PayTableSymbolInfo> SymbolPaytable { get; set; } //符号赔率表

        [JsonProperty("pay_lines")] public List<List<int>> pay_lines { get; set; } //支付钱
    }

    public class PageGameMain : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiJia";
        public new const string resName = "PageGameMain";

        private const string GamePagFolder = "Games/Cai Fu Zhi Jia 3997/Pag";
        private const string PrefabPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PageGameMain/";


        // ------------------游戏中通用变量--------------------
        // 界面初始化
        private bool _isInitPool;
        private int _totalCount = -1;
        private GComponent _gOwnerPanel;
        private GComponent _lastAnchorPanelForDispatch;

        // 游戏控制器
        private GameObject _goGameCtrl;
        private MonoHelper _monoHelper;
        private FguiPoolHelper _fGuiPoolHelper;
        private SlotMachineController3997 _slotMachineCtrl;
        private FguiGObjectPoolHelper _fGuiGObjectPoolHelper;
        private Controller _pageController; // FairyGUI的控制器
        private GameSoundController3997 _gameSoundController; // 游戏声音控制器

        // 彩金
        private readonly MiniReelGroup _uiJpMajorCtrl = new MiniReelGroup();
        private readonly MiniReelGroup _uiJpMinorCtrl = new MiniReelGroup();

        private readonly MiniReelGroup _uiJpMiniCtrl = new MiniReelGroup();

        // 玩家押注
        private long TotalBet => MainModel.Instance.contentMD.totalBet;

        // 说明书
        private List<GComponent> _lstPayTable;
        private readonly PayTableController3997 _payTableController = new PayTableController3997();

        private bool _isStopButtonLocked, _tipCoinIn, _isStoppedSlotMachine;

        // 加钱结算
        private bool IsAddCreditAnim =>
            !(_slotMachineCtrl.isStopImmediately == true || SBoxModel.Instance.isCoinOutImmediately);

        // 免费游戏和彩金游戏触发加速框
        private GameObject _freeBorderObj, _bonusBorderObj;
        private GComponent _anchorAccelerateParent, _anchorFreeAccelerate, _anchorBonusAccelerate;

        // 游戏中的协程
        private Coroutine _corGameOnce,
            _corReelsTurn,
            _corGameIdle,
            _corEffectSlowMotion,
            _corGameAuto,
            _corLightningEffect,
            _corWildCollectEffect;

        // 记录未中奖局数
        private int _currentNotWinCount;

        /// <summary>底部 Panel 是否已就绪（BottomPanelReady）。</summary>
        private bool _isBottomPanelReady;

        /// <summary>对象池 DoTask 是否已全部完成。</summary>
        private bool _isPoolPreloadDone;

        /// <summary>是否已向 PageManager 派发过 preLoadedCallback。</summary>
        private bool _hasNotifiedPagePreloaded;

        //当前游戏触发加速框后是否中奖
        private bool _isTriggerFrame;

        // -------------------------------- 免费游戏 -------------------------------------
        private FreeSpinTimeController _freeSpinTimeController; // 免费游戏次数管理器
        private GComponent _freeFrameCom;
        private GTextField _freeSpinsNumber;
        private GTextField _multipleNumber;
        private GComponent _compareRadio;
        private GameObject _radioObj, _cloneRadioObj;
        private Transform _radioEffectParent;
        private Animator _radioAnimator;

        // 免费游戏倍数增加特效制作
        private int _freeMultiplier = 2; // 显示在免费游戏text上的倍率 不用ContentModel中的了 主要作用是实现倍数逐渐增加的效果
        private GameObject _goRewardEffect, _wildBoomEffect;
        private GComponent _rewardEffectCom, _wildBoomCom, _freeParticleEffectParent;

        // 收音机中的倍数遮罩和火焰特效
        private GComponent _compareMaskEffect, _compareFireEffect;
        private GameObject _goFireEffect, _goMaskEffect, _cloneFireEffect, _cloneMaskEffect;

        // 收音机的闪电特效
        private GComponent _lightningParentCom;
        private GameObject _lightningObj;
        private readonly List<GComponent> _lightningEffectList = new List<GComponent>();
        private long _allWinCredit;
        private readonly List<Dictionary<string, object>> _stackContext = new List<Dictionary<string, object>>();

        // -------------------------------- Small Game -------------------------------------
        // 提示灯物体
        private GComponent _compareSignage;
        private GameObject _signageObj, _cloneSignageObj;

        private Animator _signageAnimator;

        // 卷轴中滚动钻石模板
        private GameObject _jackpotSymbolObj;
        private GameObject _normalSymbolObj;
        private PanelController3997 _panelCtrl;

        // 彩金收集爆点
        private GComponent _compareBagBoomCom;
        private GameObject _bagBoomObj, _cloneBagBoomObj;

        // 判断条件按
        private bool _isSmallGamePlay; // 是否进入彩金游戏
        private bool _isSmallGameFinished; // 彩金游戏是否结束
        private bool _isStartSmallGame; // 是否开始彩金游戏

        private GTextField _rollCountText;
        private GComponent _smallGameReels;
        private GComponent _smallGameSettlement, _smallGameSettlementParent; // 彩金结算飞的特效的位置  彩金结算飞的终点
        private GameObject _settlementEffect; // 结算特效
        private Coroutine _corSettlement;

        private readonly int _initialRollCount = 3;

        /// <summary>滚轴错开延迟</summary>
        private readonly float _reelStaggerDelay = 0.05f;

        private readonly string _redDiamondUrl = "ui://CaiFuZhiJia/ng_sym13_jiaj";

        private readonly List<string> _jackpotUrls = new List<string>()
        {
            "ui://CaiFuZhiJia/ng_sym_14_major", "ui://CaiFuZhiJia/ng_sym_14_minor", "ui://CaiFuZhiJia/ng_sym14_mini"
        };

        /// <summary>15个格子控制器</summary>
        private List<SmallGameReelController> _elementBoxes;

        /// <summary>所有中奖结果</summary>
        private readonly List<SmallReelResultInfo> _allHitResults = new List<SmallReelResultInfo>();

        /// <summary>未揭示的中奖结果</summary>
        private readonly List<SmallReelResultInfo> _unrevealedHits = new List<SmallReelResultInfo>();

        /// <summary>剩余滚动次数</summary>
        private int _remainingRolls;

        /// <summary>游戏循环协程</summary>
        private Coroutine _gameLoopCoroutine;

        // 普通游戏 pag
        private readonly string wealth_ng_npc_win1 = "normal_game/wealth_ng_npc_win1.pag";
        private readonly string wealth_ng_npc_win2 = "normal_game/wealth_ng_npc_win2.pag";
        private readonly string wealth_ng_npc_win3 = "normal_game/wealth_ng_npc_win3.pag";
        private readonly string wealth_ng_npc_idle01 = "normal_game/wealth_ng_npc_idle01.pag";
        private readonly string wealth_ng_npc_idle02 = "normal_game/wealth_ng_npc_idle02.pag";
        private readonly string wealth_ng_npc_notwinning = "normal_game/wealth_ng_npc_not winning.pag";
        private readonly string wealth_ng_npc_trigger_fg = "normal_game/wealth_ng_npc_trigger fg.pag";
        private readonly string wealth_ng_npc_trigger_sg = "normal_game/wealth_ng_npc_trigger sg.pag";
        private readonly string wealth_ng_npc_atmosphere = "normal_game/wealth_ng_npc_atmosphere.pag";
        private readonly string wealth_ng_npc_nottriggered = "normal_game/wealth_ng_npc_not triggered.pag";
        private readonly string wealth_ng_npc_atmosphere_idle = "normal_game/wealth_ng_npc_atmosphere_idle.pag";

        // 免费游戏 pag
        private readonly string fg_img_bg_robot = "free_game/fg_img_bg_robot.pag";
        private readonly string wealth_fg_npc_upgrade1 = "free_game/wealth_fg_npc_upgrade1.pag";
        private readonly string wealth_fg_npc_upgrade2 = "free_game/wealth_fg_npc_upgrade2.pag";
        private readonly string wealth_fg_npc_settlement = "free_game/wealth_fg_npc_settlement.pag";

        // 彩金游戏 pag
        private readonly string wealth_sg_npc_idle1 = "small_game/wealth_sg_npc_idle1.pag";
        private readonly string wealth_sg_npc_idle2 = "small_game/wealth_sg_npc_idle2.pag";
        private readonly string wealth_sg_npc_reset = "small_game/wealth_sg_npc_reset.pag";
        private readonly string wealth_sg_npc_appear = "small_game/wealth_sg_npc_appear.pag";
        private readonly string wealth_sg_npc_settlement1 = "small_game/wealth_sg_npc_settlement1.pag";
        private readonly string wealth_sg_npc_settlement2 = "small_game/wealth_sg_npc_settlement2.pag";
        private readonly string wealth_sg_npc_settlement3 = "small_game/wealth_sg_npc_settlement3.pag";

        // pag 绑定
        private GComponent _npcCom, _robotCom;

        private PagSlotBinding _pagNpc, _pagRobot;

        /// <summary>3997：底部 Panel 异步就绪后触发 PageManager 的 preLoadedCallback。</summary>
        private void OnBottomPanelReadyForPreload(EventData res)
        {
            if (res == null || res.name != PanelEvent.BottomPanelReady)
                return;

            int gameId = Convert.ToInt32(res.value);
            if (gameId != 3997)
                return;

            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_EVENT,
                OnBottomPanelReadyForPreload);
            _isBottomPanelReady = true;
            TryNotifyPagePreloaded();
        }

        /// <summary>注册并 Prepare PAG 槽位（InitParam 时调用一次即可）。</summary>
        private void EnsureMainPagSlot()
        {
            // 绑定npc pag并默认播放idle动画
            _npcCom = contentPane.GetChild("anchorNpc").asCom;
            if (_npcCom == null) return;
            _pagNpc = new PagSlotBinding("npc", GamePagFolder);
            _pagNpc.EnsureSlot(_npcCom);
            _pagNpc.Play(new PagSequencePlay(
                new[] { new PagSegment(wealth_ng_npc_idle01, -1) }, PagPlayLayout.Center, useGpuSyncGroup: false));

            // 绑定robot pag并默认播放idle动画
            _robotCom = contentPane.GetChild("anchorRobot").asCom;
            if (_robotCom == null) return;
            _pagRobot = new PagSlotBinding("robot", GamePagFolder);
            _pagRobot.EnsureSlot(_robotCom);
        }

        /// <summary>底部 Panel 与对象池均就绪后，才通知 Loading 本页预加载完成。</summary>
        private void TryNotifyPagePreloaded()
        {
            if (!_isBottomPanelReady || !_isPoolPreloadDone) return;
            if (_hasNotifiedPagePreloaded) return;
            _hasNotifiedPagePreloaded = true;
            preLoadedCallback?.Invoke();
        }

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();
            _elementBoxes = new List<SmallGameReelController>();
            // ---------- 1. 加载common,普通游戏,免费游戏,彩金游戏预制体到内存 ----------
            _totalCount = 14;
            if (UIPackage.GetByName("Common") == null)
            {
                ResourceManager02.Instance.LoadAssetBundleAsync("Assets/GameRes/Games/Common/FGUIs", (bundle) =>
                {
                    _totalCount++;
                    ResLoadedCallback();
                });
            }

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Game Controller/Slot Game Main Controller.prefab",
                (clone) =>
                {
                    _goGameCtrl = Object.Instantiate(clone, null);
                    _goGameCtrl.name = "Slot Game Main Controller 3997";
                    _goGameCtrl.transform.SetParent(null);

                    _slotMachineCtrl = _goGameCtrl.transform.Find("Slot Machine")
                        .GetComponent<SlotMachineController3997>();
                    _monoHelper = _goGameCtrl.transform.GetComponent<MonoHelper>();
                    _fGuiPoolHelper = _goGameCtrl.transform.Find("Pool").GetComponent<FguiPoolHelper>();
                    _fGuiGObjectPoolHelper =
                        _goGameCtrl.transform.Find("GObject Pool").GetComponent<FguiGObjectPoolHelper>();
                    _panelCtrl = _goGameCtrl.transform.Find("Panel").GetComponent<PanelController3997>();
                    ResLoadedCallback();
                });

            // 普通游戏资源加载
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Effect_FreeTriggerBorder.prefab",
                (clone) =>
                {
                    _freeBorderObj = clone;
                    ResLoadedCallback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Effect_BonusTriggerBorder.prefab",
                (clone) =>
                {
                    _bonusBorderObj = clone;
                    ResLoadedCallback();
                });
            // 免费游戏资源加载
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Effect_FreeWildCollect.prefab",
                (clone) =>
                {
                    _goRewardEffect = clone;
                    ResLoadedCallback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Effect_FreeWildBoom.prefab",
                (clone) =>
                {
                    _wildBoomEffect = clone;
                    ResLoadedCallback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Effect_FreeFontFire.prefab",
                (clone) =>
                {
                    _goFireEffect = clone;
                    ResLoadedCallback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Effect_FreeRadarMask.prefab",
                (clone) =>
                {
                    _goMaskEffect = clone;
                    ResLoadedCallback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Effect_FreeLightning.prefab",
                (clone) =>
                {
                    _lightningObj = clone;
                    ResLoadedCallback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Spine_FreeRadio.prefab",
                (clone) =>
                {
                    _radioObj = clone;
                    ResLoadedCallback();
                });
            // 彩金游戏资源加载
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Effect_BonusSettlement.prefab",
                (clone) =>
                {
                    _settlementEffect = clone;
                    ResLoadedCallback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Effect_BonusBagBoom.prefab",
                (clone) =>
                {
                    _bagBoomObj = clone;
                    ResLoadedCallback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Spine_BonusSignage.prefab",
                (clone) =>
                {
                    _signageObj = clone;
                    ResLoadedCallback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Spine_BonusJackpotSymbol.prefab",
                (clone) =>
                {
                    _jackpotSymbolObj = clone;
                    ResLoadedCallback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Spine_BonusNormalSymbol.prefab",
                (clone) =>
                {
                    _normalSymbolObj = clone;
                    ResLoadedCallback();
                });

            // ---------- 2. 接收硬件按钮点击 ----------
            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        if (PanelBaseController.ShouldBlockPhysicalSpinInput)
                            return;
                        if (!isReady) return;

                        Debug.LogError("游戏接受到机台短按的数据：Spin");
                        EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, false);
                        OnClickSpinButton(res);
                    },
                },
                longClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        if (PanelBaseController.ShouldBlockPhysicalSpinInput)
                            return;
                        if (!isReady) return;

                        DebugUtils.LogError("游戏接受到机台长按的数据：Spin");
                        EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, true);
                        OnClickSpinButton(res);
                    }
                }
            };
        }

        private void InitParam(EventData currentEventData)
        {
            if (!isInit) return;

            // ---------- 1. MainModel、PayTable、本地 JSON ----------
            MainModel.Instance.lineNum = 20;
            MainModel.Instance.gameID = 3997;
            MainModel.Instance.gameName = "CaiFuZhiJia3997";
            MainModel.Instance.displayName = "CaiFuZhiJia_3997";
            MainModel.Instance.contentMD = ContentModel.Instance;
            MainModel.Instance.cutomMD = CustomModel.Instance;
            MainModel.Instance.contentMD.betIndex = 0;
            ContentModel.Instance.totalBet = SBoxModel.Instance.betList[ContentModel.Instance.betIndex];

            // 说明书
            _lstPayTable = new List<GComponent>();
            foreach (string url in CustomModel.Instance.payTable)
            {
                GComponent payTable = UIPackage.CreateObjectFromURL(url).asCom;
                payTable.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().InitParam(payTable);
                _lstPayTable.Add(payTable);
                payTable.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount++;
            }

            ContentModel.Instance.goPayTableLst = _lstPayTable.ToArray();
            _payTableController.Init(_lstPayTable);

            // ---------- 2. FairyGUI 对象池（须先于滚轮 Init） ----------
            if (_fGuiPoolHelper != null && _isInitPool == false)
            {
                _isInitPool = true;
                _fGuiPoolHelper.Add(TagPoolObject.SymbolHit,
                    CustomModel.Instance.symbolHitEffect.Values.ToList(), "symbol_hit#", 5);
                _fGuiPoolHelper.PreLoad(TagPoolObject.SymbolHit); // 中奖动画
                _fGuiPoolHelper.Add(TagPoolObject.SymbolBorder,
                    CustomModel.Instance.borderEffect, "border#", 5);
                _fGuiPoolHelper.PreLoad(TagPoolObject.SymbolBorder); // 边框
                _fGuiPoolHelper.Add(TagPoolObject.SymbolAppear,
                    CustomModel.Instance.symbolAppearEffect.Values.ToList(), "symbol_appear#", 10);
                _fGuiPoolHelper.PreLoad(TagPoolObject.SymbolAppear); // 落下后图标静止动画
                _fGuiPoolHelper.WhenIdle(() =>
                {
                    _isPoolPreloadDone = true;
                    TryNotifyPagePreloaded();
                });
            }
            else if (_fGuiPoolHelper == null)
            {
                _isPoolPreloadDone = true;
            }


            // ---------- 3.滚轮控制器 ----------
            GComponent gSlotMachine = contentPane.GetChild("slotMachine").asCom;
            GComponent gReels = gSlotMachine.GetChild("reels").asCom;
            GComponent gSlotCover = gSlotMachine.asCom.GetChild("slotCover").asCom;
            GComponent gPlayLines = gSlotMachine.asCom.GetChild("playLines").asCom;
            GComponent gFrame = contentPane.GetChild("anchorFrame").asCom;
            _slotMachineCtrl.Init(gSlotCover, gPlayLines, gReels, gFrame, _fGuiPoolHelper, _fGuiGObjectPoolHelper);

            // ---------- 4. 底部菜单 Panel ----------
            _gOwnerPanel = contentPane.GetChild("panel").asCom;
            ContentModel.Instance.goAnthorPanel = _gOwnerPanel;
            MainModel.Instance.contentMD.goAnthorPanel = _gOwnerPanel;
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_EVENT,
                OnBottomPanelReadyForPreload);
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_EVENT, OnBottomPanelReadyForPreload);
            TryTriggerAnchorPanelChange();
            if (!isOpen) return;

            // ---------- 5.音乐和界面控制 ----------
            _gameSoundController = new GameSoundController3997();
            _pageController = contentPane.GetController("gameController");

            // ---------- 6.初始化FairyGUI组件 --------
            _uiJpMajorCtrl.Init("Major", contentPane.GetChild("jpMajor").asCom.GetChild("n1").asList, "N0");
            _uiJpMinorCtrl.Init("Minor", contentPane.GetChild("jpMinor").asCom.GetChild("n1").asList, "N0");
            _uiJpMiniCtrl.Init("Mini", contentPane.GetChild("jpMini").asCom.GetChild("n1").asList, "N0");
            _uiJpMajorCtrl.SetReelWidth(30);
            _uiJpMinorCtrl.SetReelWidth(30);
            _uiJpMiniCtrl.SetReelWidth(30);
            ERPushMachineDataManager02.Instance.RequestGetJpContribution((res) =>
            {
                JSONNode jsonNode = JSONNode.Parse((string)res);
                Debug.Log(jsonNode);
                int code = (int)jsonNode["code"];
                if (0 != code)
                {
                    DebugUtils.LogError($"请求贡献值报错。 code: {code}");
                    return;
                }

                _uiJpMajorCtrl.SetData((int)jsonNode["major"]);
                _uiJpMinorCtrl.SetData((int)jsonNode["minor"]);
                _uiJpMiniCtrl.SetData((int)jsonNode["mini"]);
            });

            _freeSpinTimeController = new FreeSpinTimeController();
            _freeFrameCom = contentPane.GetChild("freeFrame").asCom;
            _freeSpinsNumber = _freeFrameCom.GetChild("FreeSpinsNumber").asTextField;
            _multipleNumber = contentPane.GetChild("freeOther").asCom.GetChild("multipleNumber").asTextField;
            _freeParticleEffectParent =
                contentPane.GetChild("effectParentPanel").asCom.GetChild("anchorWildParent").asCom;
            _lightningParentCom =
                contentPane.GetChild("effectParentPanel").asCom.GetChild("anchorLightningParent").asCom;
            _freeSpinTimeController.InitParam(_freeSpinsNumber);

            _smallGameReels = contentPane.GetChild("smallGameReels").asCom;
            _rollCountText =
                contentPane.GetChild("smallGameOther").asCom.GetChild("smallGameCount").asTextField;
            _smallGameSettlement = contentPane.GetChild("effectParentPanel").asCom.GetChild("smallGameSettlementEffect")
                .asCom;
            _smallGameSettlementParent =
                contentPane.GetChild("effectParentPanel").asCom.GetChild("anchorSmallGameResult").asCom;

            //---------- 7.Clone预制体到UI锚点上 --------
            GComponent currentCom = contentPane.GetChild("freeOther").asCom.GetChild("anchorFire").asCom;
            if (currentCom != _compareFireEffect)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareFireEffect);
                _compareFireEffect = currentCom;
                _cloneFireEffect = Object.Instantiate(_goFireEffect);
                _cloneFireEffect.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(_compareFireEffect, _cloneFireEffect);
            }

            currentCom = contentPane.GetChild("freeOther").asCom.GetChild("anchorMask").asCom;
            if (currentCom != _compareMaskEffect)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareMaskEffect);
                _compareMaskEffect = currentCom;
                _cloneMaskEffect = Object.Instantiate(_goMaskEffect);
                _cloneMaskEffect.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(_compareMaskEffect, _cloneMaskEffect);
            }

            currentCom = contentPane.GetChild("freeOther").asCom.GetChild("anchorVideo").asCom;
            if (currentCom != _compareRadio)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareRadio);
                _compareRadio = currentCom;
                _cloneRadioObj = Object.Instantiate(_radioObj);
                _radioAnimator = _cloneRadioObj.GetComponentInChildren<Animator>();
                _radioEffectParent = _cloneRadioObj.transform.Find("Effect").transform.Find("eff_fg_img_multiple9");
                GameCommon.FguiUtils.AddWrapper(_compareRadio, _cloneRadioObj);
            }

            currentCom = contentPane.GetChild("smallGameOther").asCom.GetChild("anchorWarn").asCom;
            if (currentCom != _compareSignage)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareSignage);
                _compareSignage = currentCom;
                _cloneSignageObj = Object.Instantiate(_signageObj);
                _signageAnimator = _cloneSignageObj.GetComponentInChildren<Animator>();
                GameCommon.FguiUtils.AddWrapper(_compareSignage, _cloneSignageObj);
            }

            currentCom = contentPane.GetChild("effectParentPanel").asCom.GetChild("anchorSmallGameBagEffect").asCom;
            if (currentCom != _compareBagBoomCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareBagBoomCom);
                _compareBagBoomCom = currentCom;
                _cloneBagBoomObj = Object.Instantiate(_bagBoomObj);
                _cloneBagBoomObj.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(_compareBagBoomCom, _cloneBagBoomObj);
            }

            EnsureMainPagSlot();

            //---------- 8.特效功能制作 --------
            // 加速框
            _anchorAccelerateParent =
                contentPane.GetChild("effectParentPanel").asCom.GetChild("anchorAccelerateParent").asCom;
            _anchorFreeAccelerate = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            _anchorBonusAccelerate = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            GameCommon.FguiUtils.DeleteWrapper(_anchorFreeAccelerate);
            GameCommon.FguiUtils.DeleteWrapper(_anchorBonusAccelerate);
            GameCommon.FguiUtils.AddWrapper(_anchorFreeAccelerate, Object.Instantiate(_freeBorderObj));
            GameCommon.FguiUtils.AddWrapper(_anchorBonusAccelerate, Object.Instantiate(_bonusBorderObj));
            _anchorAccelerateParent.AddChild(_anchorFreeAccelerate);
            _anchorAccelerateParent.AddChild(_anchorBonusAccelerate);
            _anchorFreeAccelerate.visible = false;
            _anchorBonusAccelerate.visible = false;
            _anchorAccelerateParent.visible = true;

            // wild图标搜集
            _wildBoomCom = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            _rewardEffectCom = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            GameCommon.FguiUtils.DeleteWrapper(_wildBoomCom);
            GameCommon.FguiUtils.DeleteWrapper(_rewardEffectCom);
            GameCommon.FguiUtils.AddWrapper(_rewardEffectCom, Object.Instantiate(_goRewardEffect));
            GameCommon.FguiUtils.AddWrapper(_wildBoomCom, Object.Instantiate(_wildBoomEffect));
            _wildBoomCom.visible = false;
            _rewardEffectCom.visible = false;
            _freeParticleEffectParent.AddChild(_wildBoomCom);
            _freeParticleEffectParent.AddChild(_rewardEffectCom);
            _freeParticleEffectParent.visible = true;

            // 闪电特效制作
            foreach (var t in _lightningEffectList)
            {
                if (t != null) t.Dispose();
            }

            _lightningEffectList.Clear();
            for (int i = 0; i < 5; i++)
            {
                GComponent tempCom = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
                GameCommon.FguiUtils.DeleteWrapper(tempCom);
                GameCommon.FguiUtils.AddWrapper(tempCom, Object.Instantiate(_lightningObj));
                tempCom.visible = false;
                _lightningParentCom.AddChild(tempCom);
                _lightningEffectList.Add(tempCom);
            }

            _lightningParentCom.visible = true;

            // 彩金结算特效
            GComponent anchorSmallGameSettlement = _smallGameSettlement.GetChild("anchorSmallGameDiamond").asCom;
            GameCommon.FguiUtils.DeleteWrapper(anchorSmallGameSettlement);
            GameCommon.FguiUtils.AddWrapper(anchorSmallGameSettlement, Object.Instantiate(_settlementEffect));
            _smallGameSettlement.visible = false;

            TryRestoreFreeSpinSession();
            isReady = true;
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnStopSlot);
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnPanelInputEvent);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT, OnSlotDetailEvent);
            EventCenter.Instance.AddEventListener<CoinPushSpinParseEventArgs>(SBoxEventHandle.SBOX_COIN_PUSH_SPIN_PARSE,
                OnCoinPushSpinResultParse);
            EventCenter.Instance.AddEventListener<WinJackpotInfo>(GlobalEvent.JackpotOnlineWin, OnJackpotOnLine);
            InitParam(eventData);
            EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(Game3997AudioEvent.BgmRegularGame));
        }

        public override void OnClose(EventData eventData = null)
        {
            UnlockStopButton();
            OnGameReset();
            _lastAnchorPanelForDispatch = null;

            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnStopSlot);
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnPanelInputEvent);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT,
                OnSlotDetailEvent);
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_EVENT,
                OnBottomPanelReadyForPreload);
            EventCenter.Instance.RemoveEventListener<CoinPushSpinParseEventArgs>(
                SBoxEventHandle.SBOX_COIN_PUSH_SPIN_PARSE, OnCoinPushSpinResultParse);
            EventCenter.Instance.RemoveEventListener<WinJackpotInfo>(GlobalEvent.JackpotOnlineWin, OnJackpotOnLine);

            base.OnClose(eventData);
            _freeSpinTimeController.Dispose();
            _gameSoundController?.Dispose();
            _gameSoundController = null;
            _monoHelper.updateHandle.RemoveAllListeners();

            _pagNpc.StopWithDefaults();
            _pagRobot.StopWithDefaults();
        }

        protected override void OnLanguageChange(I18nLang lang)
        {
            FguiI18nTextAssistant.Instance.DisposeAllTranslate(contentPane);
            contentPane.Dispose(); // 释放当前UI
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            _elementBoxes = new List<SmallGameReelController>();
            InitParam(null);
        }

        private void ResLoadedCallback()
        {
            if (--_totalCount == 0)
            {
                isInit = true;
                InitParam(null);
            }
        }

        private void TryTriggerAnchorPanelChange()
        {
            if (_gOwnerPanel == null) return;
            if (ReferenceEquals(_lastAnchorPanelForDispatch, _gOwnerPanel)) return;

            _lastAnchorPanelForDispatch = _gOwnerPanel;
            EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
                new EventData<GComponent>(PanelEvent.AnchorPanelChange, _gOwnerPanel));
        }

        private void OnPanelInputEvent(EventData res)
        {
            if (!_isSmallGamePlay)
                switch (res.name)
                {
                    case PanelEvent.SpinButtonClick:
                        OnClickSpinButton(res);
                        break;
                    case PanelEvent.TotalSpinsButtonClick:
                        OnClickTotalSpinsButtonClick(res);
                        break;
                    case PanelEvent.ColUpButtonClick:
                        int col = (int)res.value;
                        _monoHelper.StartCoroutine(
                            _slotMachineCtrl.NudgeReelOneStep(col, null, false, ReelNudgeDirection.Up));
                        break;
                    case PanelEvent.ColDownButtonClick:
                        col = (int)res.value;
                        _monoHelper.StartCoroutine(
                            _slotMachineCtrl.NudgeReelOneStep(col, null, false, ReelNudgeDirection.Down));
                        break;
                }
            else
            {
                if (_isStartSmallGame) return;
                _isStartSmallGame = true;
                ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                _monoHelper.StartCoroutine(SmallGameSpin());
            }
        }

        private void OnSlotDetailEvent(EventData res)
        {
            switch (res.name)
            {
                case SlotMachineEvent.PrepareStoppedReel:
                    {
                        if (!_slotMachineCtrl.isStopImmediately)
                        {
                            int colIndex = (int)res.value;
                            if (colIndex >= 0 && colIndex < _slotMachineCtrl.column)
                            {
                                if (_corEffectSlowMotion != null) _monoHelper.StopCoroutine(_corEffectSlowMotion);
                                _corEffectSlowMotion = _monoHelper.StartCoroutine(ShowEffectReelsSlowMotion(colIndex));
                            }
                        }
                    }
                    break;
            }
        }

        private void OnStopSlot(EventData res)
        {
            switch (res.name)
            {
                case SlotMachineEvent.StoppedSlotMachine:
                    _isStoppedSlotMachine = true;
                    // UnlockStopButton();
                    break;
            }
        }

        private void OnClickTotalSpinsButtonClick(EventData res)
        {
            if (ContentModel.Instance.isSpin || ContentModel.Instance.isAuto)
                return;

            int num = (int)res.value;
            if (num != -1)
            {
                ContentModel.Instance.totalPlaySpins = num;
            }
            else
            {
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

        private void OnClickSpinButton(EventData res)
        {
            if (res.name != PanelEvent.SpinButtonClick) return;

            if (res.name == "SpinButtonClick")
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
                            if (!ContentModel.Instance.isSpin) return;
                            if (_isStopButtonLocked) return;
                            LockStopButton();
                            _slotMachineCtrl.isStopImmediately = true;
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
            if (!_isStopButtonLocked) return;
            _isStopButtonLocked = false;
            if (MainModel.Instance.panel is PanelBaseController panelBaseController)
            {
                panelBaseController.SetSpinButtonLocked(false);
            }
        }

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

        private void StartGameOnce(Action successCallback = null, Action<string> errorCallback = null)
        {
            ContentModel.Instance.totalPlaySpins = 1;
            ContentModel.Instance.remainPlaySpins = 1;
            if (_corGameOnce != null) _monoHelper.StopCoroutine(_corGameOnce);
            _corGameOnce = _monoHelper.StartCoroutine(GameOnce(successCallback, errorCallback));
        }

        private void StartGameAuto(Action successCallback = null, Action<string> errorCallback = null)
        {
            if (_corGameAuto != null) _monoHelper.StopCoroutine(_corGameAuto);
            _corGameAuto = _monoHelper.StartCoroutine(GameAuto(successCallback, errorCallback));
        }

        private WinLevelType GetBigWinType(long baseGameWinCredit)
        {
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

        private void SetUIJackpotGameReel()
        {
            JackpotRes info = ContentModel.Instance.jpGameRes;

            ContentModel.Instance.uiMajorJP.nowCredit = _uiJpMajorCtrl.nowData;
            ContentModel.Instance.uiMinorJP.nowCredit = _uiJpMinorCtrl.nowData;
            ContentModel.Instance.uiMiniJP.nowCredit = _uiJpMiniCtrl.nowData;

            ContentModel.Instance.uiMajorJP.curCredit = info.curJackpotMajor;
            ContentModel.Instance.uiMinorJP.curCredit = info.curJackpotMinior;
            ContentModel.Instance.uiMiniJP.curCredit = info.curJackpotMini;

            _uiJpMajorCtrl.SetData(info.curJackpotMajor);
            _uiJpMinorCtrl.SetData(info.curJackpotMinior);
            _uiJpMiniCtrl.SetData(info.curJackpotMini);
        }

        private void PlayRobot()
        {
            // 播放机器人特效
            if (_pagRobot == null) return;
            _pagRobot.StopWithDefaults();
            _pagRobot.Play(fg_img_bg_robot, -1);
            _pagNpc.Play(new PagSequencePlay(
                new[] { new PagSegment(wealth_ng_npc_idle01, -1) }, PagPlayLayout.Center,
                useGpuSyncGroup: false));
        }

        private void OnGameReset()
        {
            _isTriggerFrame = false;
            _isStoppedSlotMachine = false;
            _slotMachineCtrl.isStopImmediately = false;
            _slotMachineCtrl.CloseSlotCover();
            _anchorFreeAccelerate.visible = false;
            _anchorBonusAccelerate.visible = false;
            _slotMachineCtrl.SkipWinLine(true);
            if (_corGameIdle != null) _monoHelper.StopCoroutine(_corGameIdle);
            if (_corEffectSlowMotion != null) _monoHelper.StopCoroutine(_corEffectSlowMotion);
        }

        /// <summary> 滚轮开始转：解锁并显示 Spin 或 Auto。 </summary>
        private void SetSpinButtonRolling()
        {
            ContentModel.Instance.btnSpinState = ContentModel.Instance.isAuto
                ? SpinButtonState.Auto
                : SpinButtonState.Spin;
            UnlockStopButton();
        }

        /// <summary> 滚轮停稳后到 Idle 前：保持 Spin 外观并置灰，押注保持锁定。 </summary>
        private void SetSpinButtonSpinGray()
        {
            ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
            LockStopButton();
            _panelCtrl?.ChangButtonNo(true);
        }

        //下注时向大厅彩金主机发送当前下注
        private void RequestOnlineJackpotBetByCurrentBet()
        {
            try
            {
                List<JackBetInfo> jackBetInfoList = new List<JackBetInfo>();

                JackBetInfo betInfo = new JackBetInfo()
                {
                    gameType = 300,
                    seat = 1,
                    bet = (int)TotalBet * 100,
                    betPercent = 100,
                    scoreRate = 1 * 1000,
                    JPPercent = 1 * 1000,
                };
                jackBetInfoList.Add(betInfo);
                NetMessageController.Instance.SendJackBet(jackBetInfoList);
            }
            catch (Exception ex)
            {
                DebugUtils.LogError($"请求大厅彩金下注失败: {ex.Message}");
            }
        }

        private readonly HashSet<long> _handledOnlineJackpotOrderIds = new HashSet<long>();

        private string GetOnlineJackpotName(int jackpotId)
        {
            switch (jackpotId)
            {
                case 0: return "Grand";
                case 1: return "Major";
                case 2: return "Minor";
                case 3: return "Mini";
                default: return "Unknown";
            }
        }

        //大厅彩金主机赢分数据
        private void OnJackpotOnLine(WinJackpotInfo winInfo)
        {
            try
            {
                if (winInfo == null)
                    return;

                // 订单去重，避免重复处理
                if (_handledOnlineJackpotOrderIds.Contains(winInfo.orderId))
                    return;

                _handledOnlineJackpotOrderIds.Add(winInfo.orderId);

                // 入队给业务层后续表现/结算使用
                ContentModel.Instance.jpOnlineWin.Add(winInfo);

                // 彩金数据入库
                int jpLevel = winInfo.jackpotId + 1;
                string jpName = GetOnlineJackpotName(winInfo.jackpotId);
                long winCredit = winInfo.win;
                long crcreditBefore = MainBlackboardController.Instance.myRealCredit;
                long creditAfter = MainBlackboardController.Instance.myRealCredit + winCredit;
                string gameUID = string.IsNullOrEmpty(ContentModel.Instance.curGameGuid)
                    ? "-1"
                    : ContentModel.Instance.curGameGuid;
                long createdAt = winInfo.time;
                TableJackpotRecordAsyncManager.Instance.AddJackpotRecord(jpLevel, jpName, winCredit, crcreditBefore,
                    creditAfter, gameUID, createdAt);

                //通知算法卡赢得联网彩金
                SBoxWinNetJackpotInfo sBoxWinNetJackpotInfo = new SBoxWinNetJackpotInfo()
                {
                    MachineId = int.Parse(SBoxModel.Instance.MachineId),
                    PlayerId = SBoxModel.Instance.SboxPlayerAccount.PlayerId,
                    JackpotType = jpLevel,
                    JackpotWins = winCredit,
                };
                MachineDataManager02.Instance.RequestJackpotOnline(sBoxWinNetJackpotInfo, (res) =>
                {
                    //算法卡加分后同步分数
                    Debug.Log("通知算法卡赢得联网彩金");
                    JSONNode data = JSONNode.Parse((string)res);
                    int JackpotWin = (int)data["JackpotWin"];
                    long creditBefore = MainBlackboardController.Instance.myRealCredit;
                    long creditAfter = creditBefore + JackpotWin;

                    MainBlackboardController.Instance.SetMyRealCredit(creditAfter);
                }, (err) => { DebugUtils.Log(err.msg); });
            }
            catch (Exception ex)
            {
                DebugUtils.LogError($"处理大厅彩金中奖下发失败: {ex.Message}");
            }
        }

        private string GetCurrentVisibleDeckRowCol()
        {
            if (_slotMachineCtrl == null)
            {
                return string.Empty;
            }

            List<string> rows = new List<string>(_slotMachineCtrl.row);
            for (int row = 0; row < _slotMachineCtrl.row; row++)
            {
                List<string> cols = new List<string>(_slotMachineCtrl.column);
                for (int col = 0; col < _slotMachineCtrl.column; col++)
                {
                    SymbolBase symbol = _slotMachineCtrl.GetVisibleSymbolFromDeck(col, row);
                    int symbolNumber = symbol != null ? symbol.GetSymbolNumber() : 0;
                    cols.Add(symbolNumber.ToString());
                }

                rows.Add(string.Join(",", cols));
            }

            return string.Join("#", rows);
        }

        /// <summary> 从本地快照恢复未完成的免费局（不自动请求 Spin，由玩家点转）。 </summary>
        private void TryRestoreFreeSpinSession()
        {
            if (ApplicationSettings.Instance.isMock || _slotMachineCtrl == null) return;
            if (!SQLitePlayerPrefs03.Instance.isInit) return;
            if (!isOpen) return;

            int pid = SBoxModel.Instance.pid;
            var snap = FreeSpinSessionStoreG3997.TryLoad(pid);
            if (snap == null) return;

            bool sessionStillValid = snap.FreeSpinTotalTimes > 0
                                     && (snap.FreeSpinPlayTimes < snap.FreeSpinTotalTimes
                                         || (snap.FreeSpinPlayTimes == 0 && snap.NextReelStripsIndex == "FS"));
            if (!sessionStillValid)
            {
                FreeSpinSessionStoreG3997.Clear(pid);
                return;
            }

            var cm = ContentModel.Instance;
            cm.FreeSpinTotalTimes = snap.FreeSpinTotalTimes;
            cm.FreeSpinPlayTimes = snap.FreeSpinPlayTimes;
            cm.freeSpinTotalWinCoins = snap.FreeSpinTotalWinCredit;
            cm.curReelStripsIndex = snap.CurReelStripsIndex;
            cm.nextReelStripsIndex = snap.NextReelStripsIndex;
            cm.gameNumberFreeSpinTrigger = snap.GameNumberFreeSpinTrigger;
            cm.isFreeSpinTrigger = false;
            cm.isFreeSpinResult = false;
            cm.isFreeSpinAdd = false;
            cm.freeSpinAddNum = 0;
            cm.freeGameScoreMultiply = snap.FreeGameScoreMultiply;
            cm.currentWinBet = snap.CurrentWinBet;

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
            cm.ShowFreeSpinRemainTime = cm.FreeSpinTotalTimes - cm.FreeSpinPlayTimes;
            cm.gameState = GameState.Idle;
            cm.PendingFreeSpinReconnectValidation = true;

            if (!string.IsNullOrEmpty(snap.StrDeckRowCol))
            {
                cm.strDeckRowCol = snap.StrDeckRowCol;
                _slotMachineCtrl.SetReelsDeck(snap.StrDeckRowCol);
            }

            if (cm.curReelStripsIndex == "FS" || cm.nextReelStripsIndex == "FS")
            {
                // Todo：免费游戏触发逻辑
                _pageController.selectedPage = "free";
                _multipleNumber.text = "x" + cm.freeGameScoreMultiply; // 修改，不使用ContentModel中的免费倍数
                _freeSpinsNumber.text =
                    (ContentModel.Instance.FreeSpinTotalTimes - ContentModel.Instance.FreeSpinPlayTimes).ToString();
            }


            _slotMachineCtrl.SendTotalWinCreditEvent(cm.freeSpinTotalWinCoins);
            DebugUtils.Log(
                $"[G3997] 已恢复免费局快照：剩余 {cm.ShowFreeSpinRemainTime} / 总 {cm.FreeSpinTotalTimes}，待首局 Spin 与算法校验。");
        }

        private void OnCoinPushSpinResultParse(CoinPushSpinParseEventArgs e)
        {
            e.Result = MachineDataController3997.ParseCoinPushSpinPayload(e.Data, e.StartPos);
        }

        private void InputStackContextFreeSpin(Action<Dictionary<string, object>> inputStackCallBack)
        {
            Dictionary<string, object> context = new Dictionary<string, object>()
            {
                ["name"] = "FreeSpinTrigger",
                ["modifyTime"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                ["./gameState"] = ContentModel.Instance.gameState,
                ["./winList"] = ContentModel.Instance.winList,
                ["./response"] = ContentModel.Instance.response,
                ["./winFreeSpinTriggerOrAddCopy"] = ContentModel.Instance.winFreeSpinTriggerOrAddCopy,
                ["./strDeckRowCol"] = ContentModel.Instance.strDeckRowCol,
                ["./curReelStripsIndex"] = ContentModel.Instance.curReelStripsIndex,
                ["./nextReelStripsIndex"] = ContentModel.Instance.nextReelStripsIndex,
                ["./totalEarnCredit"] = ContentModel.Instance.totalEarnCoins,
                ["./isReelsSlowMotion"] = ContentModel.Instance.isReelsSlowMotion,
                ["./isFreeSpinTrigger"] = ContentModel.Instance.isFreeSpinTrigger,
                ["./curGameNumber"] = ContentModel.Instance.curGameNumber,
                ["./curGameCreatTimeMS"] = ContentModel.Instance.curGameCreatTimeMS,
                ["./curGameGuid"] = ContentModel.Instance.curGameGuid,
            };
            _stackContext.Insert(0, context);
            inputStackCallBack?.Invoke(context);
        }

        private void OutputStackContextFreeSpin(Action<Dictionary<string, object>> outputStackCallBack)
        {
            Dictionary<string, object> context = _stackContext[0];
            _stackContext.RemoveAt(0);

            // ContentModel.Instance.gameState = (string)context["./gameState"];
            ContentModel.Instance.winList = (List<SymbolWin>)context["./winList"];
            ContentModel.Instance.response = (string)context["./response"];
            ContentModel.Instance.winFreeSpinTriggerOrAddCopy = (SymbolWin)context["./winFreeSpinTriggerOrAddCopy"];
            ContentModel.Instance.strDeckRowCol = (string)context["./strDeckRowCol"];
            ContentModel.Instance.curReelStripsIndex = (string)context["./curReelStripsIndex"];
            ContentModel.Instance.nextReelStripsIndex = (string)context["./nextReelStripsIndex"];
            ContentModel.Instance.totalEarnCoins = (long)context["./totalEarnCredit"];
            ContentModel.Instance.isReelsSlowMotion = (bool)context["./isReelsSlowMotion"];
            ContentModel.Instance.isFreeSpinTrigger = (bool)context["./isFreeSpinTrigger"];
            ContentModel.Instance.curGameNumber = (long)context["./curGameNumber"];
            ContentModel.Instance.curGameCreatTimeMS = (long)context["./curGameCreatTimeMS"];
            ContentModel.Instance.curGameGuid = (string)context["./curGameGuid"];

            outputStackCallBack?.Invoke(context);
        }

        private void PlayAnimationByName(Animator animator, string aniName, Action callback = null)
        {
            animator.Rebind();
            animator.Play(aniName);
            animator.Update(0f);
            callback?.Invoke();
        }

        private IEnumerator GameOnce(Action successCallback, Action<string> errorCallback)
        {
            // ----------------- 判断游戏运行的基础状态 ---------------
            if (!SBoxModel.Instance.isMachineActive) // 检测机台是否激活
            {
                errorCallback?.Invoke(I18nMgr.language == I18nLang.cn
                    ? "请激活机台"
                    : "<size=24>Machine not activated!</size>");
                yield break;
            }

            if (ContentModel.Instance.FreeSpinTotalTimes > 0 &&
                ContentModel.Instance.nextReelStripsIndex == "FS") // 断电重连判断
            {
                Debug.LogError("进入断电重连");
                yield return GameFreeSpinFromReconnect(successCallback, errorCallback);
                yield break;
            }

            if (SBoxModel.Instance.myCredit < TotalBet) // 检测玩家积分是否足够
            {
                _tipCoinIn = true;
                errorCallback?.Invoke(
                    I18nMgr.language == I18nLang.cn
                        ? "积分不足，请先充值"
                        : "<size=15>Balance is insufficient, please recharge first</size>");
                yield break;
            }

            // ----------------- 重置游戏状态 ---------------
            OnGameReset();
            ContentModel.Instance.gameState = GameState.Spin;
            _slotMachineCtrl.BeginTurn();
            bool isNext = false;
            bool isBreak = false;
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
                        SBoxExhibitionData sBoxExhibitionData =
                            new SBoxExhibitionData { wheelChessNum = deckData.Length, data = deckData };
                        SBoxIdea.SetExhibitionData(sBoxExhibitionData);
                    }
                    catch (Exception e)
                    {
                        DebugUtils.LogError($"[G3997] 设置展会模式结果失败，deck={currentDeck}");
                        DebugUtils.LogException(e);
                    }
                }
            }

            // ----------------- 获取本局滚动结果 ---------------
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
                errorCallback?.Invoke(errMsg);
                yield break;
            }

            // 检查是否启用在线彩金,请求彩金数据
            if (SBoxModel.Instance.isJackpotOnLine)
                RequestOnlineJackpotBetByCurrentBet();

            // ----------------- 卷轴滚动 ---------------
            _slotMachineCtrl.BeginSpin();
            if (ContentModel.Instance.isReelsSlowMotion)
                _slotMachineCtrl.ShowSymbolAppearEffectAfterReelStop(true);
            else
                _slotMachineCtrl.ShowSymbolAppearEffectAfterReelStop(ContentModel.Instance.winList.Count == 0);
            if (_slotMachineCtrl.isStopImmediately)
            {
                if (_corReelsTurn != null) _monoHelper.StopCoroutine(_corReelsTurn);
                _corReelsTurn = _monoHelper.StartCoroutine(_slotMachineCtrl.TurnReelsOnce(
                    ContentModel.Instance.strDeckRowCol,
                    () =>
                    {
                        isNext = true;
                    }));
                yield return new WaitUntil(() => isNext == true);
                isNext = false;
                _pagNpc.Play(new PagSequencePlay(
                    new[] { new PagSegment(wealth_ng_npc_idle01, -1) }, PagPlayLayout.Center, useGpuSyncGroup: false));
            }
            else
            {
                if (_corReelsTurn != null) _monoHelper.StopCoroutine(_corReelsTurn);
                _corReelsTurn = _monoHelper.StartCoroutine(_slotMachineCtrl.TurnReelsNormal(
                    ContentModel.Instance.strDeckRowCol,
                    () =>
                    {
                        isNext = true;
                    }));


                yield return new WaitUntil(() => isNext == true || _slotMachineCtrl.isStopImmediately == true);
                isNext = false;

                // 等待移动结束
                if (_slotMachineCtrl.isStopImmediately && isNext == false)
                {
                    if (_corReelsTurn != null) _monoHelper.StopCoroutine(_corReelsTurn);
                    _corReelsTurn = _monoHelper.StartCoroutine(_slotMachineCtrl.ReelsToStopOrTurnOnce(() =>
                    {
                        isNext = true;
                    }));


                    yield return new WaitUntil(() => isNext == true);
                    isNext = false;
                }
            }

            bool isWinFreeOrBonus = ContentModel.Instance.isFreeSpinTrigger || ContentModel.Instance.IsBonusTrigger;
            if (isWinFreeOrBonus == false && _isTriggerFrame)
            {
                _pagNpc.Play(new PagSequencePlay(
                    PagPlaySpecs.IntroLoop(wealth_ng_npc_nottriggered, wealth_ng_npc_idle01), PagPlayLayout.Center,
                    useGpuSyncGroup: false));
                yield return new WaitForSeconds(3.5f);
            }

            // 免费奖
            if (ContentModel.Instance.isFreeSpinTrigger)
            {
                if (_currentNotWinCount < 5)
                    _currentNotWinCount = 0;
                _slotMachineCtrl.SkipWinLine(true);
                _slotMachineCtrl.ShowSymbolEffect(TagPoolObject.SymbolHit, new List<int>() { 10 }, true, 10,
                    true);
                _pagNpc.Play(new PagSequencePlay(
                    PagPlaySpecs.IntroLoop(wealth_ng_npc_trigger_fg, wealth_ng_npc_idle01), PagPlayLayout.Center,
                    useGpuSyncGroup: false));
                // 等待Pag播放结束之后再进入免费游戏
                yield return new WaitForSeconds(3f);
                //停止特效显示
                _slotMachineCtrl.SkipWinLine(false);
                yield return FreeSpinTrigger(null, errorCallback);
            }

            // 彩金奖
            if (ContentModel.Instance.IsBonusTrigger)
            {
                if (_currentNotWinCount < 5)
                    _currentNotWinCount = 0;
                InitSmallGame();
                _pagNpc.Play(new PagSequencePlay(
                    PagPlaySpecs.IntroLoop(wealth_ng_npc_trigger_sg, wealth_ng_npc_idle01), PagPlayLayout.Center,
                    useGpuSyncGroup: false));
                _slotMachineCtrl.SkipWinLine(true);
                _slotMachineCtrl.ShowSymbolEffect(TagPoolObject.SymbolHit, new List<int>() { 11 }, true, 10,
                    true);
                yield return new WaitForSeconds(5f);
                _slotMachineCtrl.SkipWinLine(false);
                yield return SmallGameTrigger();
            }

            // 线赢的数据
            List<SymbolWin> winList = ContentModel.Instance.winList;
            long allWinCredit = 0;
            if (winList.Count > 0)
            {
                long totalWinLineCredit = _slotMachineCtrl.GetTotalWinCredit(winList);
                allWinCredit = totalWinLineCredit;

                // 播放3D人物动画
                if (totalWinLineCredit < TotalBet * 2 && totalWinLineCredit > 0)
                {
                    _pagNpc.Play(new PagSequencePlay(
                        PagPlaySpecs.IntroLoop(wealth_ng_npc_win1, wealth_ng_npc_idle01), PagPlayLayout.Center,
                        useGpuSyncGroup: false));
                }
                else if (totalWinLineCredit >= TotalBet * 2 && totalWinLineCredit < TotalBet * 3)
                {
                    _pagNpc.Play(new PagSequencePlay(
                        PagPlaySpecs.IntroLoop(wealth_ng_npc_win2, wealth_ng_npc_idle01), PagPlayLayout.Center,
                        useGpuSyncGroup: false));
                }
                else if (totalWinLineCredit >= TotalBet * 3 && totalWinLineCredit <= TotalBet * 5)
                {
                    _pagNpc.Play(new PagSequencePlay(
                        PagPlaySpecs.IntroLoop(wealth_ng_npc_win3, wealth_ng_npc_idle01), PagPlayLayout.Center,
                        useGpuSyncGroup: false));
                    yield return new WaitForSeconds(3.17f);
                }

                // 计数没到五局中奖之后刷新计数
                if (_currentNotWinCount < 5)
                    _currentNotWinCount = 0;

                long score = ContentModel.Instance.totalBonusReward + _allWinCredit;
                _slotMachineCtrl.SendTotalWinCreditEvent(score + allWinCredit);
                MainBlackboardController.Instance.AddMyTempCredit(allWinCredit, true);
                if (!ContentModel.Instance.IsBonusTrigger)
                {
                    MainBlackboardController.Instance.SyncMyTempCreditToReal(true);
                }
            }
            else
                _currentNotWinCount++;

            isNext = false;
            if (winList.Count > 0 || false)
            {
                yield return ShowWinListCoinCountDown(winList, allWinCredit, false);
            }

            // 新增BigWin
            WinLevelType winLevelType = GetBigWinType(allWinCredit);
            if (winLevelType != WinLevelType.None)
            {
                yield return BigWinPopup(winLevelType, allWinCredit); //ContentModel.Instance.baseGameWinCredit

                _slotMachineCtrl.CloseSlotCover();
                _slotMachineCtrl.SkipWinLine(false);
            }

            // 连续五局没中奖播放动画
            if (_currentNotWinCount >= 5)
            {
                Debug.LogError("连续五局不中");
                _pagNpc.Play(new PagSequencePlay(
                    PagPlaySpecs.IntroLoop(wealth_ng_npc_notwinning, wealth_ng_npc_idle01), PagPlayLayout.Center,
                    useGpuSyncGroup: false));

                _currentNotWinCount = 0;
            }

            DebugUtils.Log("进入空闲模式！！！");
            // 本剧同步玩家金钱
            MainBlackboardController.Instance.SyncMyTempCreditToReal(true);
            ContentModel.Instance.gameState = GameState.Idle;

            if (winList.Count > 0 && !ContentModel.Instance.isAuto && !ContentModel.Instance.isFreeSpinTrigger)
            {
                if (_corGameIdle != null) _monoHelper.StopCoroutine(_corGameIdle);
                _corGameIdle = _monoHelper.StartCoroutine(GameIdle(winList));
            }

            _allWinCredit = 0;
            ContentModel.Instance.totalBonusReward = 0;
            successCallback?.Invoke();
        }

        private IEnumerator RequestSlotSpinFromMock02(Action successCallback = null,
            Action<string> errorCallback = null)
        {
            bool isNext = false; // 请求是否完成
            bool isBreak = false; // 是否报错
            long totalBet = TotalBet; // 存储当前的总投注额
            JSONNode resNode = null; // 请求结果

            // 请求旋转数据结果
            MachineDataController3997.Instance.RequestSlotSpinFromMock(TotalBet, (res) =>
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

            // 检查是否因为错误而中断
            if (isBreak) yield break;
            if (ContentModel.Instance.gameState != GameState.FreeSpin)
                MainBlackboardController.Instance.MinusMyTempCredit(totalBet, true, false);
            MachineDataController3997.Instance.ParseSlotSpin02(totalBet, resNode, null);
            successCallback?.Invoke();
        }

        //请求算法结果
        private IEnumerator RequestSlotSpinFromMachine(Action successCallback = null,
            Action<string> errorCallback = null)
        {
            Debug.Log("请求算法结果");
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
                Debug.Log("算法结果：" + (string)res + "--上一局免费倍率：" + ContentModel.Instance.freeGameScoreMultiply);
            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            SBoxJackpotData sBoxJackpotData = new SBoxJackpotData
            {
                // 初始化数组
                Lottery = new int[3], JackpotOut = new int[3], Jackpotlottery = new int[3], JackpotOld = new int[3]
            };

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

                sBoxJackpotData.Lottery[0] = 0;
                sBoxJackpotData.Lottery[1] = 0;
                sBoxJackpotData.Lottery[2] = 0;

                sBoxJackpotData.JackpotOut[0] = majorBet;
                sBoxJackpotData.JackpotOut[1] = minorBet;
                sBoxJackpotData.JackpotOut[2] = miniBet;

                isNext = true;
            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            //赠送局不用扣分
            if (ContentModel.Instance.gameState != GameState.FreeSpin)
                MainBlackboardController.Instance.MinusMyTempCredit(totalBet, true, false);

            Debug.Log("解析数据");
            MachineDataController3997.Instance.ParseSlotSpin02(totalBet, resNode, sBoxJackpotData);
            SetUIJackpotGameReel();
            Debug.Log("获取滚轮成功");

            successCallback?.Invoke();
        }

        private IEnumerator GameAuto(Action successCallback, Action<string> errorCallback)
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

                yield return new WaitForSeconds(0.1f);

                if (!ContentModel.Instance.isAuto)
                    break;
            }

            if (ContentModel.Instance.isRequestToStop)
            {
                ContentModel.Instance.isRequestToStop = false;
                ContentModel.Instance.isAuto = false;
            }

            successCallback?.Invoke();
        }

        private IEnumerator ShowWinListCoinCountDown(List<SymbolWin> winList, long totalWinLineCredit,
            bool isHitJackpot)
        {
            if (!isHitJackpot)
                _slotMachineCtrl.ShowSymbolWinDeck(_slotMachineCtrl.GetTotalSymbolWin(winList), true);
            yield return new WaitForSeconds(1.5f);
            _slotMachineCtrl.SkipWinLine(false);
            _slotMachineCtrl.CloseSlotCover();
        }

        private IEnumerator GameIdle(List<SymbolWin> winList)
        {
            if (winList.Count == 0) yield break;
            SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.GameIdle);
            yield return _slotMachineCtrl.ShowWinListAwayDuringIdle(winList);
        }

        private IEnumerator ShowEffectReelsSlowMotion(int colIdx)
        {
            if (!_isTriggerFrame)
            {
                if (!ContentModel.Instance.isFreeSpin) // 免费游戏中，出现加速框不需要播放该Pag
                    _pagNpc.Play(new PagSequencePlay(
                        PagPlaySpecs.IntroLoop(wealth_ng_npc_atmosphere, wealth_ng_npc_atmosphere_idle),
                        PagPlayLayout.Center,
                        useGpuSyncGroup: false));
            }

            _isTriggerFrame = true;


            GComponent comReelEffect = _anchorBonusAccelerate;
            if (ContentModel.Instance.isFreeSlotTip)
            {
                comReelEffect = _anchorFreeAccelerate;
            }

            comReelEffect.visible = false;
            comReelEffect.xy = _slotMachineCtrl.SymbolCenterToNodeLocalPos(colIdx, 1, _anchorAccelerateParent);
            comReelEffect.visible = true;
            EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(SlotMachineEvent.FreeRollingBox));
            yield return new WaitUntil(() => _isStoppedSlotMachine == true);
            comReelEffect.visible = false;
        }

        private IEnumerator BigWinPopup(WinLevelType winLevelType, long winCredit, Action callback = null)
        {
            bool isNext = false;

            PageManager.Instance.OpenPageAsync(PageName.CaiFuZhiJiaPopupBigWin,
                new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object> { ["baseGameWinCredit"] = winCredit, ["WinType"] = winLevelType, }),
                (res) =>
                {
                    isNext = true;
                });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;
            callback?.Invoke();
        }

        /// 触发免费游戏以及免费游戏一整个流程的执行
        /// <param name="successCallback"></param>
        /// <param name="errorCallback"></param>
        /// <returns></returns>
        private IEnumerator FreeSpinTrigger(Action successCallback, Action<string> errorCallback)
        {
            _slotMachineCtrl.BeginBonusFreeSpin();
            ContentModel.Instance.isFreeSpinTrigger = false;

            bool isNext = false;
            InputStackContextFreeSpin((context) =>
            {
                _freeSpinsNumber.text = ContentModel.Instance.FreeSpinTotalTimes.ToString();
            });
            PageManager.Instance.OpenPageAsync(PageName.CaiFuZhiJiaPopupFreeSpinTrigger,
                new EventData<Dictionary<string, object>>("", new Dictionary<string, object>()
                {
                    { "freeSpinCount", ContentModel.Instance.FreeSpinTotalTimes },
                    {
                        "changeFreePage", new Action(() =>
                        {
                            _pageController.selectedPage = "free";
                        })
                    },
                    {
                        "changeNpcAnimationClip", new Action(() =>
                        {
                            // 播放机器人特效
                            if (_pagRobot == null) return;
                            _pagRobot.StopWithDefaults();
                            _pagRobot.Play(fg_img_bg_robot, -1);
                            _pagNpc.Play(new PagSequencePlay(
                                new[] { new PagSegment(wealth_ng_npc_idle01, -1) }, PagPlayLayout.Center,
                                useGpuSyncGroup: false));
                        })
                    },
                }),
                (ed) =>
                {
                    _slotMachineCtrl.SendTotalWinCreditEvent(0);
                    isNext = true;
                });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;
            yield return GameFreeSpin(null, errorCallback);

            PlayAnimationByName(_radioAnimator, "Settlement");
            _pagNpc.Play(new PagSequencePlay(
                new[] { new PagSegment(wealth_fg_npc_settlement, 1) }, PagPlayLayout.Center,
                useGpuSyncGroup: false));
            _cloneRadioObj.transform.Find("Effect").transform.Find("eff_fg_img_multiple11").gameObject.SetActive(true);
            yield return new WaitForSeconds(2.5f);

            OutputStackContextFreeSpin(
                (context) =>
                {
                    SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.Default);
                    _slotMachineCtrl.SetReelsDeck((string)context["./strDeckRowCol"]);
                    _spinWEMD.Instance.SelectData(_spinWEMD.SPIN_WIN_EFFECT_FREE_SPIN_TRIGGER);

                    SymbolWin sw = (SymbolWin)context["./winFreeSpinTriggerOrAddCopy"];
                    if (sw != null && sw.cells.Count > 0)
                        _slotMachineCtrl.ShowSymbolWinDeck(sw, true);
                });

            PageManager.Instance.OpenPageAsync(PageName.CaiFuZhiJiaPopupFreeSpinResult,
                new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object>()
                    {
                        { "baseGameWinCredit", ContentModel.Instance.freeSpinTotalWinCoins },
                        {
                            "changeNormalPage", new Action(() =>
                            {
                                _pageController.selectedPage = "normal";
                            })
                        },
                        {
                            "changeNpcAnimationClip", new Action(() =>
                            {
                                // _pagNpc.StopWithDefaults();
                                _pagNpc.Play(new PagSequencePlay(
                                    new[] { new PagSegment(wealth_ng_npc_idle01, -1) }, PagPlayLayout.Center,
                                    useGpuSyncGroup: false));
                            })
                        },
                    }),
                (ed) =>
                {
                    _pagRobot.StopWithDefaults();
                    ContentModel.Instance.freeGameScoreMultiply = 2;
                    _freeMultiplier = 2;
                    _multipleNumber.text = "x2";
                    for (int i = 0; i < _lightningEffectList.Count; i++)
                        _lightningEffectList[i].visible = false;
                    _wildBoomCom.visible = false;
                    _rewardEffectCom.visible = false;
                    _cloneFireEffect.SetActive(false);
                    _radioEffectParent.Find("effect1").gameObject.SetActive(false);
                    _radioEffectParent.Find("effect2").gameObject.SetActive(false);
                    MainBlackboardController.Instance.AddMyTempCredit(_allWinCredit, true, IsAddCreditAnim); //加钱动画
                    MainBlackboardController.Instance.SyncMyTempCreditToReal(true);
                    // _allWinCredit = 0;
                    ContentModel.Instance.FreeSpinTotalTimes = 0; // 免费游戏结束之后，把免费游戏局数重置
                    ContentModel.Instance.FreeSpinPlayTimes = 0;
                    _cloneRadioObj.transform.Find("Effect").transform.Find("eff_fg_img_multiple11").gameObject
                        .SetActive(false);

                    // 重新注册
                    ContentModel.Instance.goAnthorPanel = _gOwnerPanel;
                    MainModel.Instance.contentMD.goAnthorPanel = _gOwnerPanel;
                    TryTriggerAnchorPanelChange();

                    _slotMachineCtrl.EndBonusFreeSpin();
                    EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT,
                        new EventData(Game3997AudioEvent.BgmRegularGame));

                    isNext = true;
                });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;
            yield return _slotMachineCtrl.SlotWaitForSeconds(1.5f);
        }

        /// <summary>
        /// 断电重连恢复免费局：点击一次开始后自动跑完整段免费，并统一结算与切回普通游戏。
        /// </summary>
        private IEnumerator GameFreeSpinFromReconnect(Action successCallback, Action<string> errorCallback)
        {
            ContentModel.Instance.isPowerTrigger = true;
            ContentModel.Instance.isFreeSpinTrigger = true;
            _freeMultiplier = ContentModel.Instance.freeGameScoreMultiply;
            _multipleNumber.text = "x" + ContentModel.Instance.freeGameScoreMultiply;

            PlayRobot();

            yield return GameFreeSpin(null, errorCallback);

            long freeSpinTotalWinCredit = ContentModel.Instance.freeSpinTotalWinCoins;
            if (freeSpinTotalWinCredit > 0)
            {
                MainBlackboardController.Instance.AddMyTempCredit(freeSpinTotalWinCredit, true, IsAddCreditAnim);
            }

            PageManager.Instance.OpenPageAsync(PageName.CaiFuZhiJiaPopupFreeSpinResult,
                new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object>()
                    {
                        { "baseGameWinCredit", ContentModel.Instance.freeSpinTotalWinCoins },
                        {
                            "changeNormalPage", new Action(() =>
                            {
                                _pageController.selectedPage = "normal";
                            })
                        },
                        {
                            "changeNpcAnimationClip", new Action(() =>
                            {
                                _pagNpc.Play(new PagSequencePlay(
                                    new[] { new PagSegment(wealth_ng_npc_idle01, -1) }, PagPlayLayout.Center,
                                    useGpuSyncGroup: false));
                            })
                        },
                    }),
                (ed) =>
                {
                    // _allWinCredit = 0;
                    _pageController.selectedPage = "normal";
                    _freeMultiplier = 2;
                    _multipleNumber.text = "x2";
                    ContentModel.Instance.freeGameScoreMultiply = 2;
                    ContentModel.Instance.isFreeSpinTrigger = false;
                    ContentModel.Instance.freeSpinTotalWinCoins = 0;
                    ContentModel.Instance.FreeSpinTotalTimes = 0;
                    ContentModel.Instance.FreeSpinPlayTimes = 0;
                    ContentModel.Instance.ShowFreeSpinRemainTime = 0;
                    ContentModel.Instance.curReelStripsIndex = "BS";
                    ContentModel.Instance.nextReelStripsIndex = "BS";
                    ContentModel.Instance.isFreeSpinResult = false;
                    ContentModel.Instance.isFreeSpinAdd = false;
                    ContentModel.Instance.freeSpinAddNum = 0;
                    ContentModel.Instance.PendingFreeSpinReconnectValidation = false;
                    MainBlackboardController.Instance.AddMyTempCredit(_allWinCredit, true, IsAddCreditAnim); //加钱动画
                    MainBlackboardController.Instance.SyncMyTempCreditToReal(true);
                    FreeSpinSessionStoreG3997.Clear(SBoxModel.Instance.pid);

                    // 重新注册
                    ContentModel.Instance.goAnthorPanel = _gOwnerPanel;
                    MainModel.Instance.contentMD.goAnthorPanel = _gOwnerPanel;
                    TryTriggerAnchorPanelChange();
                });
            successCallback?.Invoke();
        }

        private IEnumerator GameFreeSpin(Action successCallback, Action<string> errorCallback)
        {
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(Game3997AudioEvent.BgmFreeSpinGame));
            while (ContentModel.Instance.nextReelStripsIndex == "FS")
            {
                yield return GameFreeSpinOnce(null, errorCallback);
                yield return _slotMachineCtrl.SlotWaitForSeconds(1);
            }

            successCallback?.Invoke();
        }

        private IEnumerator GameFreeSpinOnce(Action successCallback, Action<string> errorCallback)
        {
            OnGameReset();
            ContentModel.Instance.isSpin = true;
            LockStopButton();
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
                UnlockStopButton();
                ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
                errorCallback?.Invoke(errMsg);
                yield break;
            }

            _slotMachineCtrl.BeginSpin();
            SetSpinButtonRolling();

            if (_slotMachineCtrl.isStopImmediately)
            {
                if (_corReelsTurn != null) _monoHelper.StopCoroutine(_corReelsTurn);
                _corReelsTurn = _monoHelper.StartCoroutine(_slotMachineCtrl.TurnReelsOnce(
                    ContentModel.Instance.strDeckRowCol,
                    () =>
                    {
                        isNext = true;
                    }));

                yield return new WaitUntil(() => isNext == true);
                isNext = false;
            }
            else
            {
                if (_corReelsTurn != null) _monoHelper.StopCoroutine(_corReelsTurn);
                _corReelsTurn = _monoHelper.StartCoroutine(_slotMachineCtrl.TurnReelsNormal(
                    ContentModel.Instance.strDeckRowCol,
                    () =>
                    {
                        isNext = true;
                    }));

                yield return new WaitUntil(() => isNext == true || _slotMachineCtrl.isStopImmediately == true);
                isNext = false;

                // 等待移动结束
                if (_slotMachineCtrl.isStopImmediately && isNext == false)
                {
                    if (_corReelsTurn != null) _monoHelper.StopCoroutine(_corReelsTurn);
                    _corReelsTurn = _monoHelper.StartCoroutine(_slotMachineCtrl.ReelsToStopOrTurnOnce(() =>
                    {
                        isNext = true;
                    }));

                    yield return new WaitUntil(() => isNext == true);
                    isNext = false;
                }
            }

            SetSpinButtonSpinGray();

            if (ContentModel.Instance.isHaveWildSymbol)
            {
                if (ContentModel.Instance.freeGameScoreMultiply < 5)
                {
                    PlayAnimationByName(_radioAnimator, "upgrade");
                    _pagNpc.Play(new PagSequencePlay(
                        PagPlaySpecs.IntroLoop(wealth_fg_npc_upgrade1, wealth_ng_npc_idle01), PagPlayLayout.Center,
                        useGpuSyncGroup: false));
                    yield return new WaitForSeconds(3f);
                }
                else
                {
                    PlayAnimationByName(_radioAnimator, "upgrade");
                    _pagNpc.Play(new PagSequencePlay(
                        PagPlaySpecs.IntroLoop(wealth_fg_npc_upgrade2, wealth_ng_npc_idle01), PagPlayLayout.Center,
                        useGpuSyncGroup: false));
                    yield return new WaitForSeconds(4.3f);
                }

                isNext = false;
                if (_corLightningEffect != null) _monoHelper.StopCoroutine(_corLightningEffect);
                _corLightningEffect = _monoHelper.StartCoroutine(ProcessLightningList(() => isNext = true));
                yield return new WaitUntil(() => isNext == true);
                yield return new WaitForSeconds(0.5f);
                isNext = false;
                if (_corWildCollectEffect != null) _monoHelper.StopCoroutine(_corWildCollectEffect);
                _corWildCollectEffect = _monoHelper.StartCoroutine(ProcessWildList(() => isNext = true));
                yield return new WaitUntil(() => isNext == true);
                isNext = false;
                ContentModel.Instance.isHaveWildSymbol = false;
            }

            // 线赢的数据
            List<SymbolWin> winList = ContentModel.Instance.winList;

            #region Win

            long totalWinLineCredit = 0;
            if (winList.Count > 0 || ContentModel.Instance.BonusResults != null)
            {
                totalWinLineCredit = _slotMachineCtrl.GetTotalWinCredit(winList); // 新增倍率
                if (ContentModel.Instance.isPowerTrigger)
                {
                    _allWinCredit += ContentModel.Instance.freeSpinTotalWinCoins - totalWinLineCredit; // 测试
                    ContentModel.Instance.isPowerTrigger = false;
                }

                // 播放3D人物动画
                if (totalWinLineCredit < TotalBet * 2 && totalWinLineCredit > 0)
                {
                    _pagNpc.Play(new PagSequencePlay(
                        PagPlaySpecs.IntroLoop(wealth_ng_npc_win1, wealth_ng_npc_idle01), PagPlayLayout.Center,
                        useGpuSyncGroup: false));
                }
                else if (totalWinLineCredit >= TotalBet * 2 && totalWinLineCredit < TotalBet * 3)
                {
                    _pagNpc.Play(new PagSequencePlay(
                        PagPlaySpecs.IntroLoop(wealth_ng_npc_win2, wealth_ng_npc_idle01), PagPlayLayout.Center,
                        useGpuSyncGroup: false));
                }
                else if (totalWinLineCredit >= TotalBet * 3 && totalWinLineCredit <= TotalBet * 5)
                {
                    _pagNpc.Play(new PagSequencePlay(
                        PagPlaySpecs.IntroLoop(wealth_ng_npc_win3, wealth_ng_npc_idle01), PagPlayLayout.Center,
                        useGpuSyncGroup: false));
                }

                _allWinCredit += totalWinLineCredit;
                _slotMachineCtrl.SendTotalWinCreditEvent(_allWinCredit); // 总线赢分事件
            }

            #endregion

            isNext = false;

            if (winList.Count > 0 || false)
            {
                yield return ShowWinListCoinCountDown(winList, _allWinCredit, false);
            }

            // 新增BigWin
            WinLevelType winLevelType = GetBigWinType(totalWinLineCredit);
            if (winLevelType != WinLevelType.None)
            {
                yield return BigWinPopup(winLevelType, totalWinLineCredit); //ContentModel.Instance.baseGameWinCredit

                _slotMachineCtrl.CloseSlotCover();
                _slotMachineCtrl.SkipWinLine(false);
            }

            ContentModel.Instance.gameState = GameState.Idle;
            // SetSpinButtonSpinGray();
            successCallback?.Invoke();
        }

        private IEnumerator ProcessWildList(Action callback)
        {
            foreach (var cell in ContentModel.Instance.currentWildList)
            {
                yield return _monoHelper.StartCoroutine(ShowRewardEffect(cell.column, cell.row,
                    _freeParticleEffectParent));
                _cloneMaskEffect.SetActive(true);
                _freeMultiplier++;
                yield return new WaitForSeconds(0.5f); // 特效后延迟
                _cloneMaskEffect.SetActive(false);
                _multipleNumber.text = "x" + _freeMultiplier;
                if (ContentModel.Instance.freeGameScoreMultiply > 4)
                {
                    _cloneFireEffect.SetActive(true);
                    _radioEffectParent.Find("effect1").gameObject.SetActive(true);
                    PlayAnimationByName(_radioAnimator, "idle2");
                }

                if (ContentModel.Instance.freeGameScoreMultiply > 7)
                {
                    _radioEffectParent.Find("effect2").gameObject.SetActive(true);
                }
            }

            callback?.Invoke();
        }

        private IEnumerator ShowRewardEffect(int colIdx, int rowIdx, GComponent toNode)
        {
            GComponent rewardEffect = _rewardEffectCom;
            GComponent wildBoomEffect = _wildBoomCom;

            if (wildBoomEffect != null)
            {
                wildBoomEffect.parent.RemoveChild(wildBoomEffect);
                toNode.AddChild(wildBoomEffect);
                wildBoomEffect.visible = false;
                wildBoomEffect.xy = _slotMachineCtrl.SymbolCenterToNodeLocalPos(colIdx, rowIdx, toNode);
                wildBoomEffect.visible = true;

                yield return new WaitForSeconds(0.2f);
            }

            if (rewardEffect != null)
            {
                rewardEffect.parent.RemoveChild(rewardEffect);
                toNode.AddChild(rewardEffect);
                rewardEffect.visible = false;
                rewardEffect.xy = _slotMachineCtrl.SymbolCenterToNodeLocalPos(colIdx, rowIdx, toNode);
                rewardEffect.visible = true;

                yield return MoveToZeroOverTime(rewardEffect,
                    _slotMachineCtrl.SymbolCenterToNodeLocalPos(colIdx, rowIdx, toNode));
            }
        }

        private IEnumerator ProcessLightningList(Action callback)
        {
            var coroutines = new List<Coroutine>();
            for (int i = 0; i < ContentModel.Instance.currentWildList.Count; i++)
            {
                Cell temp = ContentModel.Instance.currentWildList[i];
                Coroutine c = _monoHelper.StartCoroutine(ShowLightningEffect(
                    _lightningEffectList[i],
                    _lightningParentCom,
                    temp.column,
                    temp.row));
                coroutines.Add(c);
            }

            for (int i = 0; i < coroutines.Count; i++)
            {
                yield return coroutines[i];
            }

            callback?.Invoke();
        }

        private IEnumerator ShowLightningEffect(GComponent startPosCom, GComponent toNode, int colIdx, int rowIdx)
        {
            if (startPosCom != null)
            {
                startPosCom.parent.RemoveChild(startPosCom);
                toNode.AddChild(startPosCom);
                startPosCom.visible = false;
                startPosCom.xy = _slotMachineCtrl.SymbolCenterToNodeLocalPos(colIdx, rowIdx, toNode);
                startPosCom.visible = true;

                yield return MoveToEndPosTime(startPosCom, startPosCom.xy);
            }
        }

        private IEnumerator MoveToZeroOverTime(GComponent effect, Vector2 startPosition, float duration = 1f)
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

        private IEnumerator MoveToEndPosTime(GComponent effect, Vector2 endPos, float duration = 1f)
        {
            Vector2 startPosition = Vector2.zero; // (0,0)
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
            effect.xy = endPos;
        }

        private IEnumerator SmallGameTrigger()
        {
            _slotMachineCtrl.BeginBonusFreeSpin();
            _isSmallGamePlay = true;

            bool isNext = false;
            PageManager.Instance.OpenPageAsync(PageName.CaiFuZhiJiaPopupSmallGameTrigger,
                new EventData<Dictionary<string, object>>("", new Dictionary<string, object>()
                {
                    { "changeSmallGamePage", new Action(() => _pageController.selectedPage = "smallGame") },
                    {
                        "changeNpcAnimationClip", new Action(() =>
                        {
                            _pagNpc.StopWithDefaults();
                            _pagNpc.Play(new PagSequencePlay(
                                new[] { new PagSegment(wealth_sg_npc_idle1, -1) }, PagPlayLayout.Center,
                                useGpuSyncGroup: false));
                        })
                    },
                }),
                (ed) =>
                {
                    _slotMachineCtrl.SendTotalWinCreditEvent(0);
                    SetSpinButtonRolling();
                    ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
                    EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT,
                        new EventData(Game3997AudioEvent.BgmBonusGame));

                    _panelCtrl.ChangButtonNo(true);
                    isNext = true;
                });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            yield return new WaitUntil(() => _isSmallGameFinished == true);
            PageManager.Instance.OpenPageAsync(PageName.CaiFuZhiJiaPopupSmallGameResult,
                new EventData<Dictionary<string, object>>("", new Dictionary<string, object>()
                {
                    { "changeNormalPage", new Action(() => _pageController.selectedPage = "normal") },
                    {
                        "changeNpcAnimationClip", new Action(() =>
                        {
                            _pagNpc.StopWithDefaults();
                            _pagNpc.Play(new PagSequencePlay(
                                new[] { new PagSegment(wealth_ng_npc_idle01, -1) }, PagPlayLayout.Center,
                                useGpuSyncGroup: false));
                        })
                    },
                }),
                (ed) =>
                {
                    EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT,
                        new EventData(Game3997AudioEvent.BgmRegularGame));
                    isNext = true;
                });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;
            _isSmallGamePlay = false;
            _isSmallGameFinished = false;
            _isStartSmallGame = false;
            _slotMachineCtrl.CloseSlotCover();
            _panelCtrl.ChangButtonNo(false);
            ContentModel.Instance.IsBonusTrigger = false;
            ContentModel.Instance.IsJackpotTrigger = false;
            ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
        }

        private IEnumerator SmallGameSpin()
        {
            if (_gameLoopCoroutine != null) _monoHelper.StopCoroutine(_gameLoopCoroutine);
            _gameLoopCoroutine = _monoHelper.StartCoroutine(SmallGameLoop());
            yield return _gameLoopCoroutine;
            yield return new WaitForSeconds(0.5f);
            bool isNext = false;
            yield return SmallGameResult(() => isNext = true);
            yield return new WaitUntil(() => isNext == true);
            _isSmallGameFinished = true;
        }

        private IEnumerator SmallGameLoop()
        {
            _remainingRolls = _initialRollCount;

            while (_remainingRolls > 0)
            {
                _remainingRolls--;
                UpdateRollCountUI(_remainingRolls);
                _monoHelper.StartCoroutine(PlayWarnAndNpcAni(_remainingRolls));

                // 每轮开始前：重置未揭示的格子的滚动元素
                foreach (var box in _elementBoxes)
                {
                    if (box.State != SmallReelState.Revealed)
                        box.PlayRollReset();
                }

                // === 先确定本轮中奖 ===
                List<SmallReelResultInfo> reveals = DrawReveals();

                // ========== 新增：限制最多6个reel滚动 ==========
                const int MAX_ROLLING_COUNT = 6;
                List<SmallReelResultInfo> selectedReveals = new List<SmallReelResultInfo>();
                List<SmallReelResultInfo> delayedReveals = new List<SmallReelResultInfo>();

                if (reveals.Count > MAX_ROLLING_COUNT)
                {
                    // 中奖数超过6个：随机选6个本轮揭示，其余延后
                    var shuffled = reveals.OrderBy(_ => UnityEngine.Random.value).ToList();
                    selectedReveals = shuffled.Take(MAX_ROLLING_COUNT).ToList();
                    delayedReveals = shuffled.Skip(MAX_ROLLING_COUNT).ToList();

                    // 被延后的中奖结果保留在 _unrevealedHits 中（不要从 _unrevealedHits 移除它们）
                    // 这样它们会继续参与后续轮次的 DrawReveals()
                }
                else
                {
                    selectedReveals = reveals;
                }
                // ================================================

                HashSet<int> hitIndices = new HashSet<int>(selectedReveals.Select(r => r.reelIndex));

                // 1. 分类reel：中奖的 vs 普通的
                List<int> hitReelIndices = new List<int>();
                List<int> normalReelIndices = new List<int>();

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

                // 2. 如果中奖数不足6个，从普通reel中随机补足到6个
                if (hitReelIndices.Count < MAX_ROLLING_COUNT && normalReelIndices.Count > 0)
                {
                    int needCount = MAX_ROLLING_COUNT - hitReelIndices.Count;
                    var shuffledNormal = normalReelIndices.OrderBy(_ => UnityEngine.Random.value).ToList();
                    normalReelIndices = shuffledNormal.Take(Math.Min(needCount, shuffledNormal.Count)).ToList();
                }
                else if (hitReelIndices.Count >= MAX_ROLLING_COUNT)
                {
                    // 中奖数已经达到或超过6个（理论上不会超过，因为上面已经截断了），普通reel不滚动
                    normalReelIndices.Clear();
                }

                // 3. 设置滚动视觉
                foreach (int idx in hitReelIndices)
                    _elementBoxes[idx].SetRollingVisual();
                foreach (int idx in normalReelIndices)
                    _elementBoxes[idx].SetRollingVisual();

                // 4. 所有reel一起开始滚动（中奖reel第一圈roll第二圈result，普通reel两圈roll）
                yield return _monoHelper.StartCoroutine(
                    PlayMixedRollSequence(hitReelIndices, normalReelIndices, selectedReveals,
                        _monoHelper));

                // 5. 处理结果（滚动已包含揭示，只需处理次数）
                if (selectedReveals.Count > 0)
                {
                    _remainingRolls = _initialRollCount;
                    _pagNpc.Play(new PagSequencePlay(
                        PagPlaySpecs.IntroLoop(wealth_sg_npc_reset, wealth_sg_npc_idle1), PagPlayLayout.Center,
                        useGpuSyncGroup: false));
                    yield return new WaitForSeconds(2f);
                    PlayAnimationByName(_signageAnimator, "idle3");
                    yield return new WaitForSeconds(0.833f);
                    UpdateRollCountUI(_remainingRolls);
                    PlayAnimationByName(_signageAnimator, "idle4");
                }

                yield return new WaitForSeconds(0.3f);
            }
        }

        private IEnumerator SmallGameResult(Action onCompleted)
        {
            _pagNpc.Play(new PagSequencePlay(
                PagPlaySpecs.IntroLoop(wealth_sg_npc_settlement1, wealth_sg_npc_settlement2), PagPlayLayout.Center,
                useGpuSyncGroup: false));
            yield return new WaitForSeconds(3.83f);
            if (_corSettlement != null) _monoHelper.StopCoroutine(_corSettlement);
            _corSettlement = _monoHelper.StartCoroutine(JackpotSettlementProcess(onCompleted));
        }

        #region SmallGame

        private void InitSmallGame()
        {
            foreach (var t in _elementBoxes)
                t.Reset();
            _elementBoxes.Clear();
            _allHitResults.Clear();
            _unrevealedHits.Clear();

            for (int i = 0; i < 15; i++)
            {
                int row = i / 5;
                int col = i % 5;

                GComponent boxNode = _smallGameReels.GetChild("elementBox_" + i).asCom;
                SmallGameReelController box = new SmallGameReelController(boxNode, i);
                _elementBoxes.Add(box);

                var info = ParseSmallGameData(i, row, col, int.Parse(ContentModel.Instance.currentBonusDataList[i]),
                    ContentModel.Instance.jpTypeArray, ContentModel.Instance.jpBetArray);

                if (info.type != SmallResultType.None)
                {
                    _allHitResults.Add(info);
                    _unrevealedHits.Add(info);
                    GameObject obj =
                        Object.Instantiate(info.type == SmallResultType.Jackpot ? _jackpotSymbolObj : _normalSymbolObj);
                    box.SetResultData(info, obj);
                }
            }

            UpdateRollCountUI(_initialRollCount);
        }

        private SmallReelResultInfo ParseSmallGameData(int index, int row, int col, int currentBet,
            List<string> jpTypeArray, List<string> jpBetArray)
        {
            var info = new SmallReelResultInfo { reelIndex = index, row = row, col = col, type = SmallResultType.None };

            if (currentBet == 0) return info;

            int type = currentBet / 1000;
            int value = currentBet % 1000;

            if (type < 4)
            {
                info.type = SmallResultType.RedDiamond;
                info.rewardValue = value * MainModel.Instance.contentMD.betmultiple;
                info.rewardText = info.rewardValue.ToString();
                info.iconUrl = _redDiamondUrl;
                info.anchorChildIndex = 0;
            }
            else
            {
                int jackpotType = value % 10;
                int jackpotValue = GetJackpotValue(jackpotType, jpTypeArray, jpBetArray);

                info.type = SmallResultType.Jackpot;
                info.jackpotType = jackpotType;
                info.rewardValue = jackpotValue;
                info.rewardText = info.rewardValue.ToString();

                info.iconUrl = _jackpotUrls[jackpotType];
                info.anchorChildIndex = jackpotType;
            }

            return info;
        }

        private int GetJackpotValue(int jackpotType, List<string> jpTypeArray, List<string> jpBetArray)
        {
            for (int i = 0; i < jpTypeArray.Count; i++)
            {
                if (int.Parse(jpTypeArray[i]) == jackpotType && i < jpBetArray.Count)
                    return int.Parse(jpBetArray[i]);
            }

            return 0;
        }

        private List<SmallReelResultInfo> DrawReveals()
        {
            List<SmallReelResultInfo> reveals = new List<SmallReelResultInfo>();

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

        private void UpdateRollCountUI(int count)
        {
            if (_rollCountText != null)
                _rollCountText.text = count.ToString();
        }

        private IEnumerator PlayWarnAndNpcAni(int count)
        {
            switch (count)
            {
                case 2:
                    PlayAnimationByName(_signageAnimator, "idle2");
                    _pagNpc.Play(new PagSequencePlay(
                        new[] { new PagSegment(wealth_sg_npc_idle1, -1) }, PagPlayLayout.Center,
                        useGpuSyncGroup: false));
                    yield return new WaitForSeconds(0.5f);
                    break;
                case 1:
                    PlayAnimationByName(_signageAnimator, "idle1");
                    _pagNpc.Play(new PagSequencePlay(
                        new[] { new PagSegment(wealth_sg_npc_idle1, -1) }, PagPlayLayout.Center,
                        useGpuSyncGroup: false));
                    yield return new WaitForSeconds(0.5f);
                    break;
                case 0:
                    PlayAnimationByName(_signageAnimator, "idle5");
                    yield return new WaitForSeconds(0.5f);
                    break;
            }
        }

        private IEnumerator PlayMixedRollSequence(List<int> hitIndices, List<int> normalIndices,
            List<SmallReelResultInfo> reveals, MonoHelper monoHelper)
        {
            int completedCount = 0;
            int totalCount = hitIndices.Count + normalIndices.Count;
            bool allComplete = false;
            bool isHitJackpot = false;

            // 中奖reel：第一圈roll，第二圈result
            foreach (int idx in hitIndices)
            {
                int captureIdx = idx;
                var revealInfo = reveals.First(r => r.reelIndex == captureIdx);
                _unrevealedHits.Remove(revealInfo);

                float delay = captureIdx * _reelStaggerDelay;

                monoHelper.StartCoroutine(DelayedAction(delay, () =>
                {
                    _elementBoxes[captureIdx].PlayHitRoll(1, 1, () =>
                    {
                        completedCount++;
                        if (completedCount >= totalCount && !allComplete)
                            allComplete = true;
                    }, () =>
                    {
                        _pagNpc.Play(new PagSequencePlay(
                            PagPlaySpecs.IntroLoop(wealth_sg_npc_appear, wealth_sg_npc_idle1), PagPlayLayout.Center,
                            useGpuSyncGroup: false));
                        isHitJackpot = true;
                    });
                }));
            }

            // 普通reel：两圈roll
            foreach (int idx in normalIndices)
            {
                int captureIdx = idx;
                float delay = captureIdx * _reelStaggerDelay;
                float speed = 1;

                monoHelper.StartCoroutine(DelayedAction(delay, () =>
                {
                    if (captureIdx < _elementBoxes.Count && _elementBoxes[captureIdx] != null)
                    {
                        _elementBoxes[captureIdx].PlayNormalRoll(speed, () =>
                        {
                            completedCount++;
                            if (completedCount >= totalCount && !allComplete)
                                allComplete = true;
                        });
                    }
                }));
            }

            yield return new WaitUntil(() => allComplete);
            if (isHitJackpot)
            {
                yield return new WaitForSeconds(4.83f);
            }
        }

        private IEnumerator DelayedAction(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }

        private IEnumerator JackpotSettlementProcess(Action onCompleted)
        {
            foreach (var t in _allHitResults)
            {
                int index = t.reelIndex;
                if (t.type == SmallResultType.RedDiamond)
                {
                    yield return ShowJackpotSettlement(SmallResultType.RedDiamond, _elementBoxes[index].result,
                        t.iconUrl, t.rewardText,
                        _smallGameSettlementParent, t.col,
                        t.row);
                }
                else if (t.type == SmallResultType.Jackpot)
                {
                    yield return ShowJackpotSettlement(SmallResultType.Jackpot, _elementBoxes[index].result, t.iconUrl,
                        t.rewardText,
                        _smallGameSettlementParent, t.col,
                        t.row);
                    bool isNext = false;
                    PageManager.Instance.OpenPageAsync(PageName.CaiFuZhiJiaPopupSmallGameWin,
                        new EventData<Dictionary<string, object>>("",
                            new Dictionary<string, object>()
                            {
                                ["jackpotWinBet"] = t.rewardValue, ["jackpotWinType"] = t.jackpotType
                            }), (res) =>
                        {
                            isNext = true;
                        });
                    yield return new WaitUntil(() => isNext == true);
                }
            }

            _pagNpc.Play(new PagSequencePlay(
                new[] { new PagSegment(wealth_sg_npc_settlement3, 1) }, PagPlayLayout.Center,
                useGpuSyncGroup: false));
            yield return new WaitForSeconds(2.47f);
            onCompleted?.Invoke();
        }

        private IEnumerator ShowJackpotSettlement(SmallResultType resultType, SmallGameSymbol result, string iconUrl,
            string rewardText,
            GComponent toNode, int colIdx,
            int rowIdx)
        {
            GComponent sms = _smallGameSettlement;

            if (sms != null)
            {
                sms.GetChild("element").asLoader.url = iconUrl;
                sms.GetChild("rewardText").asTextField.text = rewardText;
                if (resultType == SmallResultType.Jackpot)
                    sms.GetChild("rewardText").asTextField.text = String.Empty;
                sms.parent.RemoveChild(sms);
                toNode.AddChild(sms);
                sms.visible = false;
                sms.xy = _slotMachineCtrl.SymbolCenterToNodeLocalPos(colIdx, rowIdx, toNode);
                sms.visible = true;
                result.Clear();

                yield return MoveToZeroOverTime(sms, sms.xy);
                sms.visible = false;
                _cloneBagBoomObj.SetActive(true);
                yield return new WaitForSeconds(0.8f);
                _cloneBagBoomObj.SetActive(false);
                ContentModel.Instance.totalBonusReward += long.Parse(rewardText);
                _slotMachineCtrl.SendTotalWinCreditEvent(ContentModel.Instance.totalBonusReward);
            }
        }

        #endregion
    }
}