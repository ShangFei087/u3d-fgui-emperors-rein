using FairyGUI;
using GameMaker;
using HotFix.Games.Fei_Zhou_Hei_Xing_Xing_3994.Custom;
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

namespace FeiZhouHeiXingXing_3994
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

    public enum BonusUrlType
    {
        Empty,
        Mini,
        Minor,
        Major
    }

    public class PageGameMain : MachinePageBase
    {
        public new const string pkgName = "FeiZhouHeiXingXing";
        public new const string resName = "PageGameMain";

        private const string PrefabPath = "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Prefabs/PageGameMain/";

        private const string GameControllerObjPath =
            "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Prefabs/Game Controller/Slot Game Main Controller.prefab";

        // --------------------------------------------- 通用变量 -----------------------------------------------
        // 资源加载、UI、eventData
        private int _totalCount = -1;
        private GComponent _gOwnerPanel;
        private bool _isInitPool;

        // 游戏控制器
        private GameObject _goGameCtrl;
        private MonoHelper _monoHelper;
        private Controller _pageController;
        private FguiPoolHelper _fGuiPoolHelper;
        private PanelController3994 _panelController;
        private FguiGObjectPoolHelper _fGuiGObjectPoolHelper;
        private GameSoundController3994 _gameSoundController;
        private SlotMachineController3994 _slotMachineController;
        private GComponent _lastAnchorPanelForDispatch;

        // 彩金
        private readonly MiniReelGroup _uiJpMajorCtrl = new MiniReelGroup();
        private readonly MiniReelGroup _uiJpMinorCtrl = new MiniReelGroup();
        private readonly MiniReelGroup _uiJpMiniCtrl = new MiniReelGroup();

        // 玩家押注
        private long TotalBet => MainModel.Instance.contentMD.totalBet;

        private bool IsAddCreditAnim =>
            !(_slotMachineController.isStopImmediately == true || SBoxModel.Instance.isCoinOutImmediately);

        // 说明书
        private List<GComponent> _lstPayTable;
        private readonly PayTableController3994 _payTableController = new PayTableController3994();
        private bool _isStopButtonLocked, _tipCoinIn, _isStoppedSlotMachine;

        // 游戏中通用协程
        private Coroutine _corGameIdle, _corGameAuto, _corGameOnce, _corReelsTurn, _corEffectSlowMotion;

        // 游戏中定制协程
        private Coroutine _corFreeWild, _corChangeIcon;

        // --------------------------------------------- 资源加载 -----------------------------------------------
        private GComponent _compareNormalNpc, _compareFreeNpc, _compareSmallNpc;
        private Animator _normalNpcAnimator, _freeNpcAnimator, _smallNpcAnimator;

        private GameObject _normalNpcObj,
            _freeNpcObj,
            _smallNpcObj,
            _cloneNormalNpcObj,
            _cloneFreeNpcObj,
            _cloneSmallNpcObj;

        private GameObject _freeSpeedUpObj, _bonusSpeedUpObj;
        private GComponent _anchorSpeedUpParent, _freeSpeedUpCom, _bonusSpeedUpCom;
        private GComponent _anchorFreeEffectParent, _anchorSmallGameEffectParent;

        /// <summary>底部 Panel 是否已就绪（BottomPanelReady）。</summary>
        private bool _isBottomPanelReady;

        /// <summary>对象池 DoTask 是否已全部完成。</summary>
        private bool _isPoolPreloadDone;

        /// <summary>是否已向 PageManager 派发过 preLoadedCallback。</summary>
        private bool _hasNotifiedPagePreloaded;

        // --------------------------------------------- 普通游戏 -----------------------------------------------
        private int _notHitSpinCount; // 记录没有中奖局数

        // --------------------------------------------- 免费游戏 -----------------------------------------------
        // 免费游戏次数
        private GComponent _freeFrameCom;
        private GTextField _freeSpinsNumber;

        private FreeSpinTimeController _freeSpinTimeController;

        // 免费游戏分数在底部UI显示的得分
        private long _allWinCredit;

        // 免费游戏特效
        private GameObject _freeBigWildObj, _freeChangeIconObj;
        private const string FreeBigWildKey = "freeBigWild";
        private const string FreeChangeIconKey = "freeChangeIcon";

        /// <summary> 免费游戏触发局数据记录 </summary>
        private readonly Stack<Dictionary<string, object>> _freeSaveStack = new Stack<Dictionary<string, object>>();

        /// <summary> 游戏中的缓存池字典 </summary>
        private readonly Dictionary<string, Stack<GComponent>> _isUsedPoolDic =
            new Dictionary<string, Stack<GComponent>>()
            {
                { FreeBigWildKey, new Stack<GComponent> { } }, { FreeChangeIconKey, new Stack<GComponent> { } },
            };

        // --------------------------------------------- 彩金游戏 -----------------------------------------------
        // 彩金游戏进度条内容
        private GProgressBar _bonusCollectSlider;
        private GTextField _bonusGameCountText;
        private GComponent _compareMiniCom, _compareMinorCom, _compareMajorCom;
        private GLoader _miniLoader, _minorLoader, _majorLoader;

        private GameObject _miniBoxObj,
            _minorBoxObj,
            _majorBoxObj,
            _cloneMiniBoxObj,
            _cloneMinorBoxObj,
            _cloneMajorBoxObj;

        // 彩金游戏中奖内容
        private GameObject _bonusMonkeyObj, _bonusBananaObj;

        // 彩金游戏收集拖尾
        private GComponent _collectTailCom;
        private GameObject _bonusCollectTailObj;

        // 彩金游戏重置
        private ParticleSystem _resetEffect;
        private GComponent _compareResetCom;
        private GameObject _bonusResetTimeObj, _cloneBonusResetTimeObj;

        // 彩金UI
        private GComponent _smallGameReelCom;
        private GTextField _bonusCountText;
        private GComponent _bonusResultCom; // 结算特效父物体
        private int _currentBonusScore; // 本局彩金得分

        private List<int> _currentBonusDataList; // 本局彩金数据
        private int _monkeyCount; // 记录本次彩金游戏的神像出现次数，判断是否会触发彩金弹窗

        private bool _isStartSmallGame; // 避免多次点击触发彩金逻辑

        // 本局彩金数据
        private List<BonusReelController> _currentBonusReelList = new List<BonusReelController>();

        /// <summary> 用作还原彩金游戏收集箱子的Url </summary>
        private readonly Dictionary<BonusUrlType, string> _bonusBoxUrlDic = new Dictionary<BonusUrlType, string>()
        {
            { BonusUrlType.Empty, "ui://FeiZhouHeiXingXing/smallEmptyBox" },
            { BonusUrlType.Mini, "ui://FeiZhouHeiXingXing/sg_sym_box1" },
            { BonusUrlType.Minor, "ui://FeiZhouHeiXingXing/sg_sym_box2" },
            { BonusUrlType.Major, "ui://FeiZhouHeiXingXing/sg_sym_box3" },
        };

        private readonly List<GameObject> _bonusResultObjs = new List<GameObject>() { null, null };

        private readonly List<string> _bonusResultIcons = new List<string>()
        {
            "ui://FeiZhouHeiXingXing/ng_sym15_caijin", "ui://FeiZhouHeiXingXing/ng_sym14_sx"
        };

        private Dictionary<BonusResultType, int> _jackpotScoreDic;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            // ---------- 1. 加载common,普通游戏,免费游戏,彩金游戏预制体到内存 ----------
            _totalCount = 15;
            // Common prefab
            if (UIPackage.GetByName("Common") == null)
            {
                ResourceManager02.Instance.LoadAssetBundleAsync("Assets/GameRes/Games/Common/FGUIs", (bundle) =>
                {
                    _totalCount++;
                    ResLoadedCallback();
                });
            }

            // Game Main prefab
            ResourceManager02.Instance.LoadAsset<GameObject>(GameControllerObjPath, (clone) =>
            {
                _goGameCtrl = Object.Instantiate(clone, null);
                _goGameCtrl.name = "Slot Game Main Controller 3994";
                _goGameCtrl.transform.SetParent(null);
                _slotMachineController =
                    _goGameCtrl.transform.Find("Slot Machine").GetComponent<SlotMachineController3994>();
                _monoHelper = _goGameCtrl.transform.GetComponent<MonoHelper>();
                _fGuiPoolHelper = _goGameCtrl.transform.Find("Pool").GetComponent<FguiPoolHelper>();
                _fGuiGObjectPoolHelper =
                    _goGameCtrl.transform.Find("GObject Pool").GetComponent<FguiGObjectPoolHelper>();
                _panelController = _goGameCtrl.transform.Find("Panel").GetComponent<PanelController3994>();
                ResLoadedCallback();
            });

            // normal prefab  
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Spine_NormalNPC.prefab",
                (clone) =>
                {
                    _normalNpcObj = clone;
                    ResLoadedCallback();
                });

            // free prefab 
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Effect_FreeSpeedUp.prefab",
                (clone) =>
                {
                    _freeSpeedUpObj = clone;
                    ResLoadedCallback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Effect_FreeChangeIcon.prefab",
                (clone) =>
                {
                    _freeChangeIconObj = clone;
                    ResLoadedCallback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Spine_FreeBigWildObj.prefab",
                (clone) =>
                {
                    _freeBigWildObj = clone;
                    ResLoadedCallback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Spine_FreeNPC.prefab",
                (clone) =>
                {
                    _freeNpcObj = clone;
                    ResLoadedCallback();
                });

            // small prefab
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Effect_BonusSpeedUp.prefab",
                (clone) =>
                {
                    _bonusSpeedUpObj = clone;
                    ResLoadedCallback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Spine_SmallNPC.prefab",
                (clone) =>
                {
                    _smallNpcObj = clone;
                    ResLoadedCallback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Spine_BonusMiniBox.prefab",
                (clone) =>
                {
                    _miniBoxObj = clone;
                    ResLoadedCallback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Spine_BonusMinorBox.prefab",
                (clone) =>
                {
                    _minorBoxObj = clone;
                    ResLoadedCallback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Spine_BonusMajorBox.prefab",
                (clone) =>
                {
                    _majorBoxObj = clone;
                    ResLoadedCallback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Spine_BonusBanana.prefab",
                (clone) =>
                {
                    _bonusBananaObj = clone;
                    _bonusResultObjs[0] = _bonusBananaObj;
                    ResLoadedCallback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Spine_BonusMonkey.prefab",
                (clone) =>
                {
                    _bonusMonkeyObj = clone;
                    _bonusResultObjs[1] = _bonusMonkeyObj;
                    ResLoadedCallback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Effect_BonusCollectTail.prefab",
                (clone) =>
                {
                    _bonusCollectTailObj = clone;
                    ResLoadedCallback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Effect_BonusResetTime.prefab",
                (clone) =>
                {
                    _bonusResetTimeObj = clone;
                    ResLoadedCallback();
                });

            // ------------------------- 2. 接收硬件按钮点击 ----------------------------
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

        private void InitParam(EventData eventData = null)
        {
            if (!isInit) return;

            // ---------- 1. MainModel、PayTable、本地 JSON ----------
            MainModel.Instance.lineNum = 25;
            MainModel.Instance.gameID = 3994;
            MainModel.Instance.gameName = "FeiZhouHeiXingXing3994";
            MainModel.Instance.displayName = "FeiZhouHeiXingXing_3994";
            MainModel.Instance.contentMD = ContentModel.Instance;
            MainModel.Instance.cutomMD = CustomModel.Instance;
            MainModel.Instance.contentMD.betIndex = 0;
            ContentModel.Instance.totalBet = SBoxModel.Instance.betList[ContentModel.Instance.betIndex];

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
            GComponent gFrame = contentPane.GetChild("anchorParent").asCom.GetChild("anchorFrame").asCom;
            _slotMachineController.Init(gSlotCover, gPlayLines, gReels, gFrame, _fGuiPoolHelper,
                _fGuiGObjectPoolHelper);

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
            _gameSoundController = new GameSoundController3994();
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
            // 测试数据
            _uiJpMajorCtrl.SetData(1000);
            _uiJpMinorCtrl.SetData(500);
            _uiJpMiniCtrl.SetData(200);

            _freeSpinTimeController = new FreeSpinTimeController();
            _freeFrameCom = contentPane.GetChild("freeOther").asCom.GetChild("freeFrame").asCom;
            _freeSpinsNumber = _freeFrameCom.GetChild("FreeSpinsNumber").asTextField;
            _freeSpinTimeController.InitParam(_freeSpinsNumber);

            _bonusCollectSlider = contentPane.GetChild("smallGameOther").asCom.GetChild("sgSlider").asProgress;
            _bonusCollectSlider.value = 0;
            _bonusGameCountText = contentPane.GetChild("smallGameOther").asCom.GetChild("smallCount").asTextField;
            _bonusGameCountText.text = ContentModel.Instance.smallGameSpinCount.ToString();

            _jackpotScoreDic = new Dictionary<BonusResultType, int>()
            {
                { BonusResultType.Mini, (int)_uiJpMiniCtrl.nowData },
                { BonusResultType.Minor, (int)_uiJpMinorCtrl.nowData },
                { BonusResultType.Major, (int)_uiJpMajorCtrl.nowData }
            };
            //---------- 7.Clone预制体到UI锚点上 --------
            GComponent currentCom = contentPane.GetChild("normalOther").asCom.GetChild("anchorNpc").asCom;
            if (currentCom != _compareNormalNpc)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareNormalNpc);
                _compareNormalNpc = currentCom;
                _cloneNormalNpcObj = Object.Instantiate(_normalNpcObj);
                _normalNpcAnimator = _cloneNormalNpcObj.GetComponentInChildren<Animator>();
                GameCommon.FguiUtils.AddWrapper(currentCom, _cloneNormalNpcObj);
            }

            currentCom = contentPane.GetChild("freeOther").asCom.GetChild("anchorNpc").asCom;
            if (currentCom != _compareFreeNpc)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareFreeNpc);
                _compareFreeNpc = currentCom;
                _cloneFreeNpcObj = Object.Instantiate(_freeNpcObj);
                _freeNpcAnimator = _cloneFreeNpcObj.GetComponentInChildren<Animator>();
                GameCommon.FguiUtils.AddWrapper(currentCom, _cloneFreeNpcObj);
            }

            currentCom = contentPane.GetChild("smallGameOther").asCom.GetChild("anchorNpc").asCom;
            if (currentCom != _compareSmallNpc)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareSmallNpc);
                _compareSmallNpc = currentCom;
                _cloneSmallNpcObj = Object.Instantiate(_smallNpcObj);
                _smallNpcAnimator = _cloneSmallNpcObj.GetComponentInChildren<Animator>();
                GameCommon.FguiUtils.AddWrapper(currentCom, _cloneSmallNpcObj);
            }

            currentCom = contentPane.GetChild("smallGameOther").asCom.GetChild("anchorMini").asCom;
            _miniLoader = currentCom.GetChild("example").asLoader;
            if (currentCom != _compareMiniCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareMiniCom);
                _compareMiniCom = currentCom;
                _cloneMiniBoxObj = Object.Instantiate(_miniBoxObj);
                _cloneMiniBoxObj.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(currentCom, _cloneMiniBoxObj);
            }

            currentCom = contentPane.GetChild("smallGameOther").asCom.GetChild("anchorMinor").asCom;
            _minorLoader = currentCom.GetChild("example").asLoader;
            if (currentCom != _compareMinorCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareMinorCom);
                _compareMinorCom = currentCom;
                _cloneMinorBoxObj = Object.Instantiate(_minorBoxObj);
                _cloneMinorBoxObj.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(currentCom, _cloneMinorBoxObj);
            }

            currentCom = contentPane.GetChild("smallGameOther").asCom.GetChild("anchorMajor").asCom;
            _majorLoader = currentCom.GetChild("example").asLoader;
            if (currentCom != _compareMajorCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareMajorCom);
                _compareMajorCom = currentCom;
                _cloneMajorBoxObj = Object.Instantiate(_majorBoxObj);
                _cloneMajorBoxObj.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(currentCom, _cloneMajorBoxObj);
            }

            currentCom = contentPane.GetChild("smallGameOther").asCom.GetChild("anchorReset").asCom;
            if (currentCom != _compareResetCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareResetCom);
                _compareResetCom = currentCom;
                _cloneBonusResetTimeObj = Object.Instantiate(_bonusResetTimeObj);
                _resetEffect = _cloneBonusResetTimeObj.GetComponentInChildren<ParticleSystem>();
                _cloneBonusResetTimeObj.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(currentCom, _cloneBonusResetTimeObj);
            }

            //---------- 8.特效功能制作 -----------------
            // 普通游戏中使用加速框
            if (_freeSpeedUpCom != null) _freeSpeedUpCom.Dispose();
            if (_bonusSpeedUpCom != null) _bonusSpeedUpCom.Dispose();
            _freeSpeedUpCom = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            GameCommon.FguiUtils.DeleteWrapper(_freeSpeedUpCom);
            GameCommon.FguiUtils.AddWrapper(_freeSpeedUpCom, Object.Instantiate(_freeSpeedUpObj));
            _freeSpeedUpCom.visible = false;
            _bonusSpeedUpCom = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            GameCommon.FguiUtils.DeleteWrapper(_bonusSpeedUpCom);
            GameCommon.FguiUtils.AddWrapper(_bonusSpeedUpCom, Object.Instantiate(_bonusSpeedUpObj));
            _bonusSpeedUpCom.visible = false;
            _anchorSpeedUpParent = contentPane.GetChild("anchorParent").asCom.GetChild("anchorSpeedParent").asCom;
            _anchorSpeedUpParent.AddChild(_freeSpeedUpCom);
            _anchorSpeedUpParent.AddChild(_bonusSpeedUpCom);
            _anchorSpeedUpParent.visible = true;

            // 免费游戏使用缓存池特效
            CachePoolController.Instance.ClearPool();
            _anchorFreeEffectParent =
                contentPane.GetChild("anchorParent").asCom.GetChild("anchorFreeEffectParent").asCom;
            for (int i = 0; i < 4; i++)
                CachePoolController.Instance.PushCom(FreeBigWildKey, CachePoolFactory(_freeBigWildObj));
            for (int i = 0; i < 5; i++)
                CachePoolController.Instance.PushCom(FreeChangeIconKey, CachePoolFactory(_freeChangeIconObj));

            // 彩金游戏使用缓存池特效
            _anchorSmallGameEffectParent =
                contentPane.GetChild("anchorParent").asCom.GetChild("anchorSmallGameEffectParent").asCom;

            // 彩金游戏结算收集特效
            _collectTailCom = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            GameCommon.FguiUtils.DeleteWrapper(_collectTailCom);
            GameCommon.FguiUtils.AddWrapper(_collectTailCom, Object.Instantiate(_bonusCollectTailObj));
            _collectTailCom.visible = false;
            _anchorSmallGameEffectParent.AddChild(_collectTailCom);
            _anchorSmallGameEffectParent.visible = true;

            // 获取彩金滚轮框 并对彩金滚轮进行初始化
            _smallGameReelCom = contentPane.GetChild("smallGameReels").asCom;
            _bonusResultCom = contentPane.GetChild("anchorCollectEffectParent").asCom;
            _bonusCountText = contentPane.GetChild("smallGameOther").asCom.GetChild("smallCount").asTextField;

            TryRestoreFreeSpinSession();
            isReady = true;
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnStopSlot);
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnClickSpinButton);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT, OnSlotDetailEvent);
            EventCenter.Instance.AddEventListener<CoinPushSpinParseEventArgs>(SBoxEventHandle.SBOX_COIN_PUSH_SPIN_PARSE,
                OnCoinPushSpinResultParse);
            InitParam(eventData);
            EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(Game3994AudioEvent.BgmRegularGame));
        }

        public override void OnClose(EventData eventData = null)
        {
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnStopSlot);
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnClickSpinButton);
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_EVENT,
                OnBottomPanelReadyForPreload);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT,
                OnSlotDetailEvent);
            EventCenter.Instance.RemoveEventListener<CoinPushSpinParseEventArgs>(
                SBoxEventHandle.SBOX_COIN_PUSH_SPIN_PARSE,
                OnCoinPushSpinResultParse);
            base.OnClose(eventData);
            _freeSpinTimeController.Dispose();
            _gameSoundController?.Dispose();
            _gameSoundController = null;
            _monoHelper.updateHandle.RemoveAllListeners();
            _lastAnchorPanelForDispatch = null;
            OnGameReset();
        }

        protected override void OnLanguageChange(I18nLang lang)
        {
            FguiI18nTextAssistant.Instance.DisposeAllTranslate(contentPane);
            contentPane.Dispose(); // 释放当前UI
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            _currentBonusReelList = null;
            InitParam();
        }

        private void OnCoinPushSpinResultParse(CoinPushSpinParseEventArgs e)
        {
            e.Result = MachineDataController3994.ParseCoinPushSpinPayload(e.Data, e.StartPos);
        }

        /// <summary>底部 Panel 与对象池均就绪后，才通知 Loading 本页预加载完成。</summary>
        private void TryNotifyPagePreloaded()
        {
            if (!_isBottomPanelReady || !_isPoolPreloadDone) return;
            if (_hasNotifiedPagePreloaded) return;
            _hasNotifiedPagePreloaded = true;
            preLoadedCallback?.Invoke();
        }

        #region 资源加载

        private void ResLoadedCallback()
        {
            if (--_totalCount != 0) return;
            isInit = true;
            InitParam(null);
        }

        /// <summary>3994：底部 Panel 异步就绪后触发 PageManager 的 preLoadedCallback。</summary>
        private void OnBottomPanelReadyForPreload(EventData res)
        {
            if (res == null || res.name != PanelEvent.BottomPanelReady) return;
            int gameId = Convert.ToInt32(res.value);
            if (gameId != 3994) return;

            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_EVENT,
                OnBottomPanelReadyForPreload);
            _isBottomPanelReady = true;
            TryNotifyPagePreloaded();
        }

        /// <summary> 如果Panel进行切换，重新注册Panel </summary>
        private void TryTriggerAnchorPanelChange()
        {
            if (_gOwnerPanel == null) return;
            if (ReferenceEquals(_lastAnchorPanelForDispatch, _gOwnerPanel)) return;

            _lastAnchorPanelForDispatch = _gOwnerPanel;
            EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
                new EventData<GComponent>(PanelEvent.AnchorPanelChange, _gOwnerPanel));
        }

        #endregion

        #region 普通游戏

        private void OnClickSpinButton(EventData eventData)
        {
            if (!ContentModel.Instance.isSmallGameSpin)
                switch (eventData.name)
                {
                    case PanelEvent.SpinButtonClick:
                        {
                            bool isLongClick = (bool)eventData.value;
                            switch (ContentModel.Instance.btnSpinState)
                            {
                                case SpinButtonState.Stop:
                                    {
                                        if (ContentModel.Instance.isSpin) return;
                                        UnlockStopButton();
                                        ContentModel.Instance.isSpin = true;

                                        if (isLongClick)
                                        {
                                            ContentModel.Instance.isAuto = true;
                                            ContentModel.Instance.btnSpinState = SpinButtonState.Auto;
                                            StartGameAuto(ContinueGameWhenCompleted, StopGameWhenError);
                                        }
                                        else
                                        {
                                            ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                                            StartGameOnce(ContinueGameWhenCompleted, StopGameWhenError);
                                        }
                                    }
                                    break;
                                case SpinButtonState.Spin:
                                    {
                                        if (!ContentModel.Instance.isSpin) return;
                                        LockStopButton();
                                        _slotMachineController.isStopImmediately = true;
                                    }
                                    break;
                                case SpinButtonState.Auto:
                                    {
                                        ContentModel.Instance.isSpin = true;
                                        ContentModel.Instance.isAuto = false;
                                        ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                                    }
                                    break;
                            }
                        }
                        break;
                    case "ColUpButtonClick":
                        {
                            int reelIndex = (int)eventData.value;
                            if (_corGameIdle != null) _monoHelper.StopCoroutine(_corGameIdle);
                            _monoHelper.StartCoroutine(
                                _slotMachineController.NudgeReelOneStep(reelIndex, null, false, ReelNudgeDirection.Up));
                        }
                        break;
                    case "ColDownButtonClick":
                        {
                            int reelIndex = (int)eventData.value;
                            if (_corGameIdle != null) _monoHelper.StopCoroutine(_corGameIdle);
                            _monoHelper.StartCoroutine(_slotMachineController.NudgeReelOneStep(reelIndex));
                        }
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

        /// <summary> 点击两次Spin按钮，按钮置灰，上锁无法点击 </summary>
        private void LockStopButton()
        {
            if (_isStopButtonLocked) return;
            _isStopButtonLocked = true;
            if (MainModel.Instance.panel is PanelBaseController panelBaseController)
                panelBaseController.SetSpinButtonLocked(true);
        }

        /// <summary> Spin按钮解锁 </summary>
        private void UnlockStopButton()
        {
            if (!_isStopButtonLocked) return;
            _isStopButtonLocked = false;
            if (MainModel.Instance.panel is PanelBaseController panelBaseController)
                panelBaseController.SetSpinButtonLocked(false);
        }

        /// <summary> 旋转成功，重置状态 </summary>
        private void ContinueGameWhenCompleted()
        {
            DebugUtils.Log("游戏结束");
            UnlockStopButton();
            ContentModel.Instance.isSpin = false;
            ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
            ContentModel.Instance.gameState = GameState.Idle;
        }

        /// <summary> 旋转失败，抛出错误 </summary>
        private void StopGameWhenError(string msg)
        {
            UnlockStopButton();
            ContentModel.Instance.isSpin = false;
            ContentModel.Instance.isAuto = false;
            ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
            ContentModel.Instance.gameState = GameState.Idle;


            // TODO: 未来如果需要启用“有好酷优先用好酷”逻辑，需恢复以下条件判断：
            if (SBoxModel.Instance.isUseIot && _tipCoinIn) { }

            if (string.IsNullOrEmpty(msg)) return;
            string message = I18nMgr.T(msg);
            TipPopupHandler.Instance.OpenPopupOnce(message);
        }

        private void StartGameAuto(Action successCallback = null, Action<string> errorCallback = null)
        {
            if (_corGameAuto != null) _monoHelper.StopCoroutine(_corGameAuto);
            _corGameAuto = _monoHelper.StartCoroutine(GameAuto(successCallback, errorCallback));
        }

        private void StartGameOnce(Action successCallback = null, Action<string> errorCallback = null)
        {
            ContentModel.Instance.totalPlaySpins = 1;
            ContentModel.Instance.remainPlaySpins = 1;
            if (_corGameOnce != null) _monoHelper.StopCoroutine(_corGameOnce);
            _corGameOnce = _monoHelper.StartCoroutine(GameOnce(successCallback, errorCallback));
        }

        private string GetCurrentVisibleDeckRowCol()
        {
            if (_slotMachineController == null) return string.Empty;
            List<string> rows = new List<string>(_slotMachineController.row);
            for (int row = 0; row < _slotMachineController.row; row++)
            {
                List<string> cols = new List<string>(_slotMachineController.column);
                for (int col = 0; col < _slotMachineController.column; col++)
                {
                    SymbolBase symbol = _slotMachineController.GetVisibleSymbolFromDeck(col, row);
                    int symbolNumber = symbol?.GetSymbolNumber() ?? 0;
                    cols.Add(symbolNumber.ToString());
                }

                rows.Add(string.Join(",", cols));
            }

            return string.Join("#", rows);
        }

        private void SetUIJackpotGameReel()
        {
            JackpotRes info = ContentModel.Instance.JpGameRes;

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

        private void OnGameReset()
        {
            _isStoppedSlotMachine = false;
            _freeSpeedUpCom.visible = false;
            _bonusSpeedUpCom.visible = false;
            _slotMachineController.CloseSlotCover();
            _slotMachineController.SkipWinLine(true);
            if (_corGameIdle != null) _monoHelper.StopCoroutine(_corGameIdle);
            if (_corEffectSlowMotion != null) _monoHelper.StopCoroutine(_corEffectSlowMotion);
        }

        private void OnStopSlot(EventData res)
        {
            switch (res.name)
            {
                case SlotMachineEvent.StoppedSlotMachine:
                    {
                        _isStoppedSlotMachine = true;
                        UnlockStopButton();
                    }
                    break;
            }
        }

        private void OnSlotDetailEvent(EventData res)
        {
            switch (res.name)
            {
                case SlotMachineEvent.PrepareStoppedReel:
                    {
                        if (!_slotMachineController.isStopImmediately)
                        {
                            int colIndex = (int)res.value;
                            if (colIndex >= 0 && colIndex < _slotMachineController.column)
                            {
                                if (_corEffectSlowMotion != null) _monoHelper.StopCoroutine(_corEffectSlowMotion);
                                _corEffectSlowMotion = _monoHelper.StartCoroutine(ShowEffectReelsSlowMotion(colIndex));
                            }
                        }
                    }
                    break;
            }
        }

        /// <summary>通过动画名播放动画</summary>
        private void PlayAnimationByName(Animator animator, string aniName, Action callback = null)
        {
            animator.Rebind();
            animator.Play(aniName);
            animator.Update(0f);
            callback?.Invoke();
        }

        /// <summary>判断指定动画片段是否播放</summary>
        private bool IsDesignAniClipPlay(Animator ani, string aniName)
        {
            AnimatorStateInfo stateInfo = ani.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsName(aniName);
        }

        private IEnumerator ShowEffectReelsSlowMotion(int colIdx)
        {
            if (!IsDesignAniClipPlay(_normalNpcAnimator, "idle3"))
                PlayAnimationByName(_normalNpcAnimator, "idle3");
            GComponent comReelEffect = _bonusSpeedUpCom;
            if (ContentModel.Instance.isFreeSlotTip) comReelEffect = _freeSpeedUpCom;

            comReelEffect.visible = false;
            comReelEffect.xy = _slotMachineController.SymbolCenterToNodeLocalPos(colIdx, 1, _anchorSpeedUpParent);
            comReelEffect.visible = true;
            EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT,
                ContentModel.Instance.isFreeSlotTip
                    ? new EventData(SlotMachineEvent.FreeRollingBox)
                    : new EventData(SlotMachineEvent.BonusRollingBox));

            yield return new WaitUntil(() => _isStoppedSlotMachine == true);
            // 关闭加速框特效
            comReelEffect.visible = false;
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
                if (isErr) yield break;
                yield return new WaitForSeconds(0.1f);
                if (!ContentModel.Instance.isAuto) break;
            }

            if (ContentModel.Instance.isRequestToStop)
            {
                ContentModel.Instance.isRequestToStop = false;
                ContentModel.Instance.isAuto = false;
            }

            successCallback?.Invoke();
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
                yield return GameFreeSpinFromReconnect(successCallback, errorCallback);
                yield break;
            }

            if (SBoxModel.Instance.myCredit < TotalBet) // 检测玩家积分是否足够
            {
                _tipCoinIn = true;
                errorCallback?.Invoke(I18nMgr.language == I18nLang.cn
                    ? "积分不足，请先充值"
                    : "<size=15>Balance is insufficient, please recharge first</size>");
                yield break;
            }

            // --------------------- 重置游戏状态 --------------------------
            OnGameReset();
            ContentModel.Instance.gameState = GameState.Spin;
            _slotMachineController.BeginTurn();
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
                        DebugUtils.LogError($"[G3994] 设置展会模式结果失败，deck={currentDeck}");
                        DebugUtils.LogException(e);
                    }
                }
            }

            //--------------------- 获取本局滚动结果 --------------------------
            if (ApplicationSettings.Instance.isMock)
            {
                yield return RequestSlotSpinFromMock(() => { isNext = true; }, (err) =>
                {
                    errMsg = err;
                    isNext = true;
                    isBreak = true;
                });
            }
            else
            {
                yield return RequestSlotSpinFromMachine(() => { isNext = true; }, (err) =>
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

            //--------------------- 卷轴开始滚动 --------------------------
            _slotMachineController.BeginSpin();
            if (ContentModel.Instance.isReelsSlowMotion)
                _slotMachineController.ShowSymbolAppearEffectAfterReelStop();
            else
                _slotMachineController.ShowSymbolAppearEffectAfterReelStop(ContentModel.Instance.winList.Count == 0);
            if (_slotMachineController.isStopImmediately)
            {
                if (_corReelsTurn != null) _monoHelper.StopCoroutine(_corReelsTurn);
                _corReelsTurn = _monoHelper.StartCoroutine(_slotMachineController.TurnReelsOnce(
                    ContentModel.Instance.strDeckRowCol, () => { isNext = true; }));
                yield return new WaitUntil(() => isNext == true);
                isNext = false;
            }
            else
            {
                if (_corReelsTurn != null) _monoHelper.StopCoroutine(_corReelsTurn);
                _corReelsTurn = _monoHelper.StartCoroutine(
                    _slotMachineController.TurnReelsNormal(ContentModel.Instance.strDeckRowCol,
                        () => { isNext = true; }));
                yield return new WaitUntil(() => isNext == true || _slotMachineController.isStopImmediately == true);
                isNext = false;

                // 等待移动结束
                if (_slotMachineController.isStopImmediately && isNext == false)
                {
                    if (_corReelsTurn != null) _monoHelper.StopCoroutine(_corReelsTurn);
                    _corReelsTurn =
                        _monoHelper.StartCoroutine(
                            _slotMachineController.ReelsToStopOrTurnOnce(() => { isNext = true; }));
                    yield return new WaitUntil(() => isNext == true);
                    isNext = false;
                }
            }

            List<SymbolWin> winList = ContentModel.Instance.winList;
            // ----------------- normal win ---------------
            if (winList.Count > 0)
            {
                _notHitSpinCount = 0;
                long totalWinLineCredit = _slotMachineController.GetTotalWinCredit(winList);
                _slotMachineController.SendTotalWinCreditEvent(totalWinLineCredit); // 积分同步和退币处理
                MainBlackboardController.Instance.AddMyTempCredit(totalWinLineCredit, true); // 加钱动画
                MainBlackboardController.Instance.SyncMyTempCreditToReal(true); // 同步玩家真实金币
            }
            else
                _notHitSpinCount++;

            // ----------------- big win ---------------
            WinLevelType winLevelType = GetBigWinType();
            if (winLevelType != WinLevelType.None)
            {
                yield return BigWinPopup(winLevelType, ContentModel.Instance.baseGameWinCredit);
                _slotMachineController.CloseSlotCover();
                _slotMachineController.SkipWinLine(false);
            }

            // ----------------- free win ---------------
            if (ContentModel.Instance.isFreeSpinTrigger)
            {
                _notHitSpinCount = 0;
                _slotMachineController.SkipWinLine(true);
                _slotMachineController.ShowSymbolEffect(TagPoolObject.SymbolHit, new List<int>() { 10 }, true, 10,
                    true);
                yield return _slotMachineController.SlotWaitForSeconds(1.333f);
                yield return FreeSpinTrigger(null, errorCallback);
            }

            // ----------------- small win ---------------
            if (ContentModel.Instance.isSmallGameTrigger)
            {
                GetOnceBonusData();
                _notHitSpinCount = 0;
                _slotMachineController.ShowSymbolEffect(TagPoolObject.SymbolHit, new List<int>() { 11 }, true, 11,
                    true);
                yield return _slotMachineController.SlotWaitForSeconds(4f);
                _slotMachineController.SkipWinLine(true);
                yield return SmallGameTrigger();
            }

            // 连续五次未中奖
            if (_notHitSpinCount >= 5)
            {
                PlayAnimationByName(_normalNpcAnimator, "idle2");
                yield return new WaitForSeconds(2.667f);
                _notHitSpinCount = 0;
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

            _slotMachineController.isStopImmediately = false;
            successCallback?.Invoke();
        }

        /// <summary> 请求模拟算法结果 </summary>
        private IEnumerator RequestSlotSpinFromMock(Action successCallback = null, Action<string> errorCallback = null)
        {
            bool isNext = false; // 请求是否完成
            bool isBreak = false; // 是否报错
            long totalBet = TotalBet; // 存储当前的总投注额
            JSONNode resNode = null; // 请求结果

            // 请求旋转数据结果
            MachineDataController3994.Instance.RequestSlotSpinFromMock(TotalBet, (res) =>
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
            MachineDataController3994.Instance.ParseSlotSpin(totalBet, resNode, null);
            successCallback?.Invoke();
        }

        /// <summary> 请求真实算法结果 </summary>
        private IEnumerator RequestSlotSpinFromMachine(Action successCallback = null,
            Action<string> errorCallback = null)
        {
            Debug.Log("请求算法结果");
            long totalBet = TotalBet;
            bool isNext = false;
            JSONNode resNode = null;

            ERPushMachineDataManager02.Instance.RequestCoinPushSpin((res) =>
            {
                resNode = JSONNode.Parse((string)res);
                isNext = true;
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
                JSONNode jsonNode = JSONNode.Parse((string)res);
                Debug.Log(jsonNode);
                int code = (int)jsonNode["code"];

                if (0 != code)
                {
                    DebugUtils.LogError($"请求贡献值报错。 code: {code}");
                    isNext = true;
                    return;
                }

                int majorBet = (int)jsonNode["major"];
                int minorBet = (int)jsonNode["minor"];
                int miniBet = (int)jsonNode["mini"];

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
            MachineDataController3994.Instance.ParseSlotSpin(totalBet, resNode, sBoxJackpotData);
            SetUIJackpotGameReel();
            Debug.Log("获取滚轮成功");

            successCallback?.Invoke();
        }

        private IEnumerator GameIdle(List<SymbolWin> winList)
        {
            if (winList.Count == 0) yield break;
            SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.GameIdle);
            yield return _slotMachineController.ShowWinListAwayDuringIdle(winList);
        }

        #endregion

        #region 大奖弹窗

        private WinLevelType GetBigWinType()
        {
            long baseGameWinCredit = ContentModel.Instance.baseGameWinCredit;
            List<WinMultiple> winMultipleList = CustomModel.Instance.winLevelMultiple;
            long totalBet = ContentModel.Instance.totalBet;
            WinLevelType winLevelType = WinLevelType.None;
            foreach (var t in winMultipleList)
            {
                if (baseGameWinCredit > totalBet * t.multiple)
                {
                    winLevelType = t.winLevelType;
                }
            }

            return winLevelType;
        }

        private IEnumerator BigWinPopup(WinLevelType winLevelType, long winCredit, Action callback = null)
        {
            bool isNext = false;
            PageManager.Instance.OpenPageAsync(PageName.FeiZhouHeiXingXingPopupBigWin,
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

        #endregion

        #region 免费游戏

        /// <summary>记录免费触发局信息，压栈</summary>
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
            _freeSaveStack.Push(context);
            inputStackCallBack?.Invoke(context);
        }

        /// <summary>恢复免费触发局信息，弹栈</summary>
        private void OutputStackContextFreeSpin(Action<Dictionary<string, object>> outputStackCallBack)
        {
            if (_freeSaveStack.Count == 0)
            {
                DebugUtils.LogError("FreeSpin stack underflow!");
                return;
            }

            Dictionary<string, object> context = _freeSaveStack.Pop();
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

        private IEnumerator FreeSpinTrigger(Action successCallback, Action<string> errorCallback)
        {
            _slotMachineController.BeginBonusFreeSpin(); // 关闭展会模式
            ContentModel.Instance.isFreeSpinTrigger = false;

            bool isNext = false;
            InputStackContextFreeSpin((context) =>
            {
                _freeSpinsNumber.text = ContentModel.Instance.FreeSpinTotalTimes.ToString();
            });
            PageManager.Instance.OpenPageAsync(PageName.FeiZhouHeiXingXingPopupFreeSpinTrigger,
                new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object>()
                    {
                        { "freeSpinCount", ContentModel.Instance.FreeSpinTotalTimes },
                        { "changeFreePage", new Action(() => _pageController.selectedPage = "free") },
                    }),
                (ed) =>
                {
                    _slotMachineController.SendTotalWinCreditEvent(0);
                    isNext = true;
                });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            yield return FreeGameSpin(successCallback, errorCallback);

            OutputStackContextFreeSpin((context) =>
            {
                SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.Default);
                _slotMachineController.SetReelsDeck((string)context["./strDeckRowCol"]);
                _spinWEMD.Instance.SelectData(_spinWEMD.SPIN_WIN_EFFECT_FREE_SPIN_TRIGGER);
                SymbolWin sw = (SymbolWin)context["./winFreeSpinTriggerOrAddCopy"];
                if (sw != null && sw.cells.Count > 0) _slotMachineController.ShowSymbolWinDeck(sw, true);
            });
            PageManager.Instance.OpenPageAsync(PageName.FeiZhouHeiXingXingPopupFreeSpinResult,
                new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object>()
                    {
                        { "freeTotalScore", ContentModel.Instance.freeSpinTotalWinCoins },
                        { "changeNormalPage", new Action(() => _pageController.selectedPage = "normal") },
                    }),
                (ed) =>
                {
                    ContentModel.Instance.FreeSpinTotalTimes = 0;
                    isNext = true;
                });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;
            yield return _slotMachineController.SlotWaitForSeconds(1.5f);
        }

        private IEnumerator FreeSpinOnce(Action successCallback, Action<string> errorCallback)
        {
            OnGameReset();
            ContentModel.Instance.gameState = GameState.FreeSpin;

            bool isNext = false;
            bool isBreak = false;
            string errMsg = "";

            if (ApplicationSettings.Instance.isMock)
            {
                yield return RequestSlotSpinFromMock(() => { isNext = true; }, (err) =>
                {
                    errMsg = err;
                    isNext = true;
                    isBreak = true;
                });
            }
            else
            {
                yield return RequestSlotSpinFromMachine(() => { isNext = true; }, (err) =>
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

            _slotMachineController.BeginSpin();
            if (_slotMachineController.isStopImmediately)
            {
                if (_corReelsTurn != null) _monoHelper.StopCoroutine(_corReelsTurn);
                _corReelsTurn = _monoHelper.StartCoroutine(
                    _slotMachineController.TurnReelsOnce(ContentModel.Instance.strDeckRowCol,
                        () => { isNext = true; }));
                yield return new WaitUntil(() => isNext == true);
                isNext = false;
            }
            else
            {
                if (_corReelsTurn != null) _monoHelper.StopCoroutine(_corReelsTurn);
                _corReelsTurn = _monoHelper.StartCoroutine(
                    _slotMachineController.TurnReelsNormal(ContentModel.Instance.strDeckRowCol,
                        () => { isNext = true; }));
                yield return new WaitUntil(() => isNext == true || _slotMachineController.isStopImmediately == true);
                isNext = false;

                // 等待移动结束
                if (_slotMachineController.isStopImmediately && isNext == false)
                {
                    if (_corReelsTurn != null) _monoHelper.StopCoroutine(_corReelsTurn);
                    _corReelsTurn =
                        _monoHelper.StartCoroutine(
                            _slotMachineController.ReelsToStopOrTurnOnce(() => { isNext = true; }));
                    yield return new WaitUntil(() => isNext == true);
                    isNext = false;
                }
            }

            // ----------------- icon change ----------------
            if (_corChangeIcon != null) _monoHelper.StopCoroutine(_corChangeIcon);
            _corChangeIcon = _monoHelper.StartCoroutine(IconConversion(() => isNext = true));
            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            // ----------------- show wild ----------------
            if (_corFreeWild != null) _monoHelper.StopCoroutine(_corFreeWild);
            _corFreeWild = _monoHelper.StartCoroutine(ShowWildSpine(GetFreeMiddleData(), () => isNext = true));
            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            // ----------------- normal win ----------------
            List<SymbolWin> winList = ContentModel.Instance.winList;
            if (winList.Count > 0 || ContentModel.Instance.BonusResult != null)
            {
                PlayAnimationByName(_freeNpcAnimator, "win");
                long totalWinLineCredit = _slotMachineController.GetTotalWinCredit(winList);
                _allWinCredit += totalWinLineCredit;
                _slotMachineController.SendTotalWinCreditEvent(_allWinCredit); // 总线赢分事件
            }

            isNext = false;

            if (winList.Count > 0 || false)
            {
                yield return ShowWinListCoinCountDown(winList, _allWinCredit, false);
            }

            // ----------------- big win ----------------
            WinLevelType winLevelType = GetBigWinType();
            if (winLevelType != WinLevelType.None)
            {
                yield return BigWinPopup(winLevelType, ContentModel.Instance.baseGameWinCredit);
                _slotMachineController.CloseSlotCover();
                _slotMachineController.SkipWinLine(false);
            }

            ContentModel.Instance.gameState = GameState.Idle;
            successCallback?.Invoke();
        }

        private IEnumerator FreeGameSpin(Action successCallback, Action<string> errorCallback)
        {
            EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(Game3994AudioEvent.BgmFreeSpinGame));
            while (ContentModel.Instance.nextReelStripsIndex == "FS")
            {
                yield return FreeSpinOnce(null, errorCallback);
                yield return _slotMachineController.SlotWaitForSeconds(1);
            }

            successCallback?.Invoke();
        }

        private IEnumerator ShowWinListCoinCountDown(List<SymbolWin> winList, long totalWinLineCredit,
            bool isHitJackpot)
        {
            if (!isHitJackpot)
                _slotMachineController.ShowSymbolWinDeck(_slotMachineController.GetTotalSymbolWin(winList), true);
            yield return new WaitForSeconds(1.5f);
            _slotMachineController.SkipWinLine(false);
            _slotMachineController.CloseSlotCover();
            PushIsUsedComToPool();
        }

        /// <summary> 获取免费游戏中 中间位置图标的索引 </summary>
        private List<int> GetFreeMiddleData()
        {
            List<int> currentMiddleData = new List<int>();
            List<int> currentFreeData = SlotTool.GetDeckRowCol(ContentModel.Instance.strDeckRowCol);
            for (int i = 5; i < 10; i++)
            {
                currentMiddleData.Add(currentFreeData[i]);
                // Debug.LogError($"currentFreeData[{i}]的值是：{currentFreeData[i]}");
            }

            return currentMiddleData;
        }

        ///<summary>缓存池创建工厂，根据传入的预制体不同创建对应的UI物体</summary>
        private GComponent CachePoolFactory(GameObject cacheObj)
        {
            GComponent com = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            GameCommon.FguiUtils.DeleteWrapper(com);
            GameCommon.FguiUtils.AddWrapper(com, Object.Instantiate(cacheObj));
            return com;
        }

        ///<summary>第二列之后中间图标是wild的整列覆盖大Wild</summary>
        private IEnumerator ShowWildSpine(List<int> middleData, Action callback)
        {
            for (int i = 1; i < middleData.Count; i++)
            {
                if (middleData[i] != 9) continue;
                PlayAnimationByName(_freeNpcAnimator, "idle2");
                GComponent com = CachePoolController.Instance.PopCom(FreeBigWildKey, _anchorFreeEffectParent,
                    () => CachePoolFactory(_freeBigWildObj));
                _isUsedPoolDic[FreeBigWildKey].Push(com);
                com.xy = _slotMachineController.SymbolCenterToNodeLocalPos(i, 1, _anchorFreeEffectParent);
                com.visible = true;
            }

            yield return new WaitForSeconds(1f);
            callback?.Invoke();
        }

        ///<summary>高分图标替换低分图标：先播特效再切换图标。从8开始向下逐级传播，8→7→6→5→4，4不转3</summary>
        private IEnumerator IconConversion(Action callback)
        {
            string strDeck = ContentModel.Instance.strDeckRowCol;
            if (string.IsNullOrEmpty(strDeck))
            {
                callback?.Invoke();
                yield break;
            }

            // 1. 解析 strDeckRowCol 为 3行×5列 的二维数组
            string[] rows = strDeck.Split('#');
            int rowCount = rows.Length;
            int colCount = rows[0].Split(',').Length;

            int[,] grid = new int[rowCount, colCount];
            for (int r = 0; r < rowCount; r++)
            {
                string[] cols = rows[r].Split(',');
                for (int c = 0; c < colCount; c++)
                {
                    grid[r, c] = int.Parse(cols[c]);
                }
            }

            // 2. 找出所有需要被转换的位置（暂不修改 grid），从8向下逐级传播
            // 到4为止，4不将3转为4
            List<(int r, int c)> allChangedPositions = new List<(int, int)>();

            for (int sourceValue = 8; sourceValue >= 5; sourceValue--)
            {
                int targetValue = sourceValue - 1;
                HashSet<(int, int)> toUpgrade = new HashSet<(int, int)>();

                for (int r = 0; r < rowCount; r++)
                {
                    for (int c = 0; c < colCount; c++)
                    {
                        if (grid[r, c] != sourceValue) continue;

                        // 上
                        if (r > 0 && grid[r - 1, c] == targetValue)
                            toUpgrade.Add((r - 1, c));
                        // 下
                        if (r < rowCount - 1 && grid[r + 1, c] == targetValue)
                            toUpgrade.Add((r + 1, c));
                        // 左
                        if (c > 0 && grid[r, c - 1] == targetValue)
                            toUpgrade.Add((r, c - 1));
                        // 右
                        if (c < colCount - 1 && grid[r, c + 1] == targetValue)
                            toUpgrade.Add((r, c + 1));
                    }
                }

                // 标记升级（此时仅记录位置，不立即修改 grid，以免影响同级传播）
                foreach (var pos in toUpgrade)
                {
                    if (!allChangedPositions.Contains(pos))
                        allChangedPositions.Add(pos);
                    grid[pos.Item1, pos.Item2] = sourceValue;
                }
            }

            // 3. 先在转换位置播放特效，再切换图标
            if (allChangedPositions.Count > 0)
            {
                PlayAnimationByName(_freeNpcAnimator, "win2");
                // 3a. 播放转换特效
                foreach (var pos in allChangedPositions)
                {
                    GComponent com = CachePoolController.Instance.PopCom(FreeChangeIconKey, _anchorFreeEffectParent,
                        () => CachePoolFactory(_freeChangeIconObj));
                    _isUsedPoolDic[FreeChangeIconKey].Push(com);
                    com.xy = _slotMachineController.FreeGameSymbolCenterToNodeLocalPos(pos.Item2, pos.Item1,
                        _anchorFreeEffectParent);
                    com.visible = true;
                }

                yield return new WaitForSeconds(1.5f);

                // 3b. 特效播放完毕后，切换图标：更新 ContentModel 并刷新滚轮显示
                List<string> rowStrings = new List<string>();
                for (int r = 0; r < rowCount; r++)
                {
                    List<string> colStrings = new List<string>();
                    for (int c = 0; c < colCount; c++)
                    {
                        colStrings.Add(grid[r, c].ToString());
                    }

                    rowStrings.Add(string.Join(",", colStrings));
                }

                string newStrDeckRowCol = string.Join("#", rowStrings);

                ContentModel.Instance.strDeckRowCol = newStrDeckRowCol;
                _slotMachineController.SetReelsDeck(newStrDeckRowCol);

                // 3c. 等待图标切换完成
                yield return _slotMachineController.SlotWaitForSeconds(1.5f);
            }

            callback?.Invoke();
        }

        ///<summary>将每局使用的池子物体归还给池子</summary>
        private void PushIsUsedComToPool()
        {
            PushUsedComToPoolForKey(FreeBigWildKey);
            PushUsedComToPoolForKey(FreeChangeIconKey);
        }

        ///<summary>将指定 key 已使用的所有组件归还缓存池</summary>
        private void PushUsedComToPoolForKey(string key)
        {
            if (!_isUsedPoolDic.TryGetValue(key, out Stack<GComponent> stack) || stack.Count == 0)
                return;

            while (stack.Count > 0)
            {
                GComponent com = stack.Pop();
                CachePoolController.Instance.PushCom(key, com);
            }
        }

        #endregion

        #region 断电重连

        /// <summary>断电重连恢复免费局：点击一次开始后自动跑完整段免费，并统一结算与切回普通游戏。</summary>
        private IEnumerator GameFreeSpinFromReconnect(Action successCallback, Action<string> errorCallback)
        {
            yield return FreeGameSpin(null, errorCallback);
            if (ContentModel.Instance.freeSpinTotalWinCoins > 0)
                MainBlackboardController.Instance.AddMyTempCredit(ContentModel.Instance.freeSpinTotalWinCoins, true,
                    IsAddCreditAnim);

            PageManager.Instance.OpenPageAsync(PageName.FeiZhouHeiXingXingPopupFreeSpinResult,
                new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object>()
                    {
                        { "freeTotalScore", ContentModel.Instance.freeSpinTotalWinCoins },
                        { "changeNormalPage", new Action(() => { _pageController.selectedPage = "normal"; }) },
                    }),
                (ed) =>
                {
                    _allWinCredit = 0;
                    ContentModel.Instance.freeSpinTotalWinCoins = 0;
                    ContentModel.Instance.FreeSpinTotalTimes = 0;
                    ContentModel.Instance.FreeSpinPlayTimes = 0;
                    ContentModel.Instance.ShowFreeSpinRemainTime = 0;
                    ContentModel.Instance.curReelStripsIndex = "BS";
                    ContentModel.Instance.nextReelStripsIndex = "BS";
                    ContentModel.Instance.isFreeSpinFinish = false;
                    ContentModel.Instance.isFreeGameAdd = false;
                    ContentModel.Instance.freeSpinAddNum = 0;
                    ContentModel.Instance.PendingFreeSpinReconnectValidation = false;
                    MainBlackboardController.Instance.AddMyTempCredit(_allWinCredit, true, IsAddCreditAnim); //加钱动画
                    MainBlackboardController.Instance.SyncMyTempCreditToReal(true);
                    FreeSpinSessionStoreG3994.Clear(SBoxModel.Instance.pid);

                    // 重新注册
                    ContentModel.Instance.goAnthorPanel = _gOwnerPanel;
                    MainModel.Instance.contentMD.goAnthorPanel = _gOwnerPanel;
                    TryTriggerAnchorPanelChange();
                });
            successCallback?.Invoke();
        }

        /// <summary> 从本地快照恢复未完成的免费局（不自动请求 Spin，由玩家点转）。 </summary>
        private void TryRestoreFreeSpinSession()
        {
            if (ApplicationSettings.Instance.isMock || _slotMachineController == null) return;
            if (!SQLitePlayerPrefs03.Instance.isInit) return;
            if (!isOpen) return;

            int pid = SBoxModel.Instance.pid;
            var snap = FreeSpinSessionStoreG3994.TryLoad(pid);
            if (snap == null) return;

            bool sessionStillValid = snap.FreeSpinTotalTimes > 0
                                     && (snap.FreeSpinPlayTimes < snap.FreeSpinTotalTimes
                                         || (snap.FreeSpinPlayTimes == 0 && snap.NextReelStripsIndex == "FS"));
            if (!sessionStillValid)
            {
                FreeSpinSessionStoreG3994.Clear(pid);
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
            cm.isFreeSpinFinish = false;
            cm.isFreeGameAdd = false;
            cm.freeSpinAddNum = 0;

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
                _slotMachineController.SetReelsDeck(snap.StrDeckRowCol);
            }

            if (cm.curReelStripsIndex == "FS" || cm.nextReelStripsIndex == "FS")
            {
                // Todo：免费游戏触发逻辑
                _pageController.selectedPage = "free";
                _freeSpinsNumber.text =
                    (ContentModel.Instance.FreeSpinTotalTimes - ContentModel.Instance.FreeSpinPlayTimes).ToString();
            }


            _slotMachineController.SendTotalWinCreditEvent(cm.freeSpinTotalWinCoins);
            DebugUtils.Log(
                $"[G3994] 已恢复免费局快照：剩余 {cm.ShowFreeSpinRemainTime} / 总 {cm.FreeSpinTotalTimes}，待首局 Spin 与算法校验。");
        }

        #endregion

        #region 彩金游戏

        /// <summary> 重置彩金游戏收集进度条状态，box隐藏，url重置，重置彩金游戏次数 </summary>
        private void ResetCollectProcessState()
        {
            _monkeyCount = 0;
            _currentBonusScore = 0;
            _bonusCountText.text = "3";
            ContentModel.Instance.smallGameSpinCount = 3;

            _bonusCollectSlider.value = 0;
            _cloneMiniBoxObj.SetActive(false);
            _cloneMinorBoxObj.SetActive(false);
            _cloneMajorBoxObj.SetActive(false);
            _miniLoader.url = _bonusBoxUrlDic[BonusUrlType.Mini];
            _minorLoader.url = _bonusBoxUrlDic[BonusUrlType.Minor];
            _majorLoader.url = _bonusBoxUrlDic[BonusUrlType.Major];
        }

        /// <summary> 通过索引获取其在 3 行 5 列格子中所在的行和列；索引 0-4 为第 0 行，索引 5-9 为第 1 行，索引 10-14 为第 2 行。 </summary>
        private (int Row, int Col) GetRowColByIndex(int index)
        {
            if (index < 0 || index >= 15)
            {
                Debug.LogError($"GetRowColByIndex: index {index} out of range [0, 14]");
                return (-1, -1);
            }

            const int colCount = 5;
            int row = index / colCount;
            int col = index % colCount;
            return (row, col);
        }

        /// <summary> 获取一次Bonus游戏数据 </summary>
        private void GetOnceBonusData()
        {
            // 队列中没有剩余局数据时直接结束，避免对空队列 Dequeue 报错
            if (ContentModel.Instance.BonusDataQueue == null || ContentModel.Instance.BonusDataQueue.Count == 0)
            {
                // 队列消费完毕，视为小游戏结束
                ContentModel.Instance.smallGameSpinCount = -1;
                return;
            }

            _currentBonusDataList = ContentModel.Instance.BonusDataQueue.Dequeue();
            BonusGameController.Instance.ResetBonusData(_currentBonusReelList);
            List<BonusReelResultInfo> infos =
                BonusGameController.Instance.GetCurrentRoundResultInfo(
                    _currentBonusDataList, _bonusResultObjs, _bonusResultIcons);
            _currentBonusReelList =
                BonusGameController.Instance.InitBonusOnceData(_smallGameReelCom, CustomModel.Instance.symbolIcon,
                    infos);
        }

        private IEnumerator SmallGameTrigger()
        {
            _slotMachineController.CloseSlotCover(); // 关闭特效显示
            _slotMachineController.BeginBonusFreeSpin(); // 关闭展会模式
            // 记录彩金游戏状态
            ContentModel.Instance.isSmallGameTrigger = false;
            ContentModel.Instance.isSmallGameSpin = true;

            // 打开彩金触发界面
            bool isNext = false;
            PageManager.Instance.OpenPageAsync(PageName.FeiZhouHeiXingXingPopupSmallGameTrigger,
                new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object>()
                    {
                        { "changeSmallGamePage", new Action(() =>
                        {
                            _pageController.selectedPage = "small";
                           
                        }) },
                    }), (ed) =>
                {
                    ContentModel.Instance.btnSpinState = SpinButtonState.Stop; // 需要先重置按钮的状态，否则会置灰其他按钮失败
                    _panelController.ChangButtonNo(true);
                    EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT,
                        new EventData(Game3994AudioEvent.BgmBonusGame));
                    isNext = true;
                });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            yield return new WaitUntil(() => ContentModel.Instance.IsSmallGameFinish == true);
            yield return new WaitForSeconds(1);

            // 打开彩金结算界面
            PageManager.Instance.OpenPageAsync(PageName.FeiZhouHeiXingXingPopupSmallGameResult,
                new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object>()
                    {
                        { "changeNormalPage", new Action(() => _pageController.selectedPage = "normal") },
                        { "smallTotalScore", _currentBonusScore }
                    }), (ed) =>
                {
                    _panelController.ChangButtonNo(false);
                    ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
                    EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT,
                        new EventData(Game3994AudioEvent.BgmRegularGame));
                    _isStartSmallGame = false;

                    ResetCollectProcessState();
                    isNext = true;
                });
            yield return new WaitUntil(() => isNext == true);
        }

        private IEnumerator SmallGameSpin()
        {
            while (!ContentModel.Instance.IsSmallGameFinish)
            {
                _bonusResetTimeObj.SetActive(false);
                --ContentModel.Instance.smallGameSpinCount;
                yield return BonusGameController.Instance.BonusGameOnce(_currentBonusReelList, () =>
                {
                    BonusGameController.Instance.UpdateUIShow(_bonusCountText,
                        ContentModel.Instance.smallGameSpinCount);
                });
                yield return SmallGameResult(GetOnceBonusData);
            }

            PlayAnimationByName(_smallNpcAnimator, "3win2");
            yield return new WaitForSeconds(2.7f);


            ContentModel.Instance.smallGameSpinCount = -1;
            ContentModel.Instance.isSmallGameSpin = false;
        }

        private IEnumerator SmallGameResult(Action onCompleted)
        {
            for (int i = 0; i < _currentBonusReelList.Count; i++)
            {
                if (_currentBonusReelList[i].ResultInfo.Type == BonusResultType.None)
                {
                    continue;
                }

                int index = i;
                Animator ani = _currentBonusReelList[i].ResultObj.GetComponentInChildren<Animator>();
                switch (_currentBonusReelList[i].ResultInfo.Type)
                {
                    case BonusResultType.Bonus:
                        {
                            PlayAnimationByName(ani, "win");
                            PlayAnimationByName(_smallNpcAnimator, "2win");
                            (int row, int col) = GetRowColByIndex(i);
                            _currentBonusReelList[i].ResultSymbol.ScoreText.text = "";
                            yield return CollectScore(_currentBonusReelList[i].ResultInfo, col, row);
                        }
                        break;
                    case BonusResultType.Special:
                        {
                            PlayAnimationByName(ani, "collect");
                            yield return new WaitForSeconds(1);
                            if (ContentModel.Instance.smallGameSpinCount != 3)
                            {
                                _cloneBonusResetTimeObj.SetActive(true);
                                _resetEffect.Play();
                            }

                            ContentModel.Instance.smallGameSpinCount = 3;
                            BonusGameController.Instance.UpdateUIShow(_bonusCountText,
                                ContentModel.Instance.smallGameSpinCount);
                            _monkeyCount++;
                            _bonusCollectSlider.value = _monkeyCount;
                            _currentBonusReelList[index].ResultSymbol.IconLoader.url = _bonusResultIcons[1];
                            switch (_monkeyCount)
                            {
                                case 5:
                                    _cloneMiniBoxObj.SetActive(true);
                                    _miniLoader.url = "";
                                    PlayAnimationByName(_smallNpcAnimator, "2win2");
                                    yield return new WaitForSeconds(2.7f);
                                    yield return GetJackpotScore(BonusResultType.Mini,
                                        _jackpotScoreDic[BonusResultType.Mini]);
                                    break;
                                case 10:
                                    _cloneMinorBoxObj.SetActive(true);
                                    _minorLoader.url = "";
                                    PlayAnimationByName(_smallNpcAnimator, "2win2");
                                    yield return new WaitForSeconds(2.7f);
                                    yield return GetJackpotScore(BonusResultType.Minor,
                                        _jackpotScoreDic[BonusResultType.Minor]);
                                    break;
                                case 15:
                                    _cloneMajorBoxObj.SetActive(true);
                                    _majorLoader.url = "";
                                    PlayAnimationByName(_smallNpcAnimator, "2win2");
                                    yield return new WaitForSeconds(2.7f);
                                    yield return GetJackpotScore(BonusResultType.Major,
                                        _jackpotScoreDic[BonusResultType.Major]);
                                    break;
                            }
                        }
                        break;
                }
            }

            onCompleted?.Invoke();
        }

        private IEnumerator GetJackpotScore(BonusResultType type, int winScore)
        {
            bool isNext = false;
            PageManager.Instance.OpenPageAsync(PageName.FeiZhouHeiXingXingPopupSmallGameJackpotWin,
                new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object>() { { "resultType", type }, { "winScore", winScore } }), (ed) =>
                {
                    _currentBonusScore += winScore;
                    _slotMachineController.SendTotalWinCreditEvent(_currentBonusScore);
                    isNext = true;
                });
            yield return new WaitUntil(() => isNext == true);
        }

        private IEnumerator CollectScore(BonusReelResultInfo resultInfo, int row, int col)
        {
            if (_collectTailCom == null)
            {
                yield break;
            }

            _collectTailCom.parent.RemoveChild(_collectTailCom);
            _bonusResultCom.AddChild(_collectTailCom);
            _collectTailCom.visible = false;
            _collectTailCom.xy = _slotMachineController.SymbolCenterToNodeLocalPos(row, col, _bonusResultCom);
            _collectTailCom.visible = true;

            yield return MoveToZeroOverTime(_collectTailCom, _collectTailCom.xy);
            _collectTailCom.visible = false;
            _currentBonusScore += resultInfo.HitScore;
            _slotMachineController.SendTotalWinCreditEvent(_currentBonusScore);
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

        #endregion
    }
}