using FairyGUI;
using GameMaker;
using Mono.Data.Sqlite;
using Newtonsoft.Json;
using PusherEmperorsRein;
using SBoxApi;
using SimpleJSON;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
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

        private const string SpinePrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PageGameMain/SpinePrefabs/";

        private const string EffectPrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PageGameMain/EffectPrefabs/";

        private const string ModelPrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Npc/";

        // 界面初始化
        private bool _isInitPool = false;
        private int _totalCount = -1;
        private GComponent _gOwnerPanel;
        private GComponent _lastAnchorPanelForDispatch;
        private TextAsset _gameInfo = null;

        // 游戏控制器
        private GameObject _goGameCtrl;
        private MonoHelper _monoHelper;
        private FguiPoolHelper _fGuiPoolHelper;
        private SlotMachineController3997 _slotMachineCtrl;
        private FguiGObjectPoolHelper _fGuiGObjectPoolHelper;
        private Controller _pageController; // FairyGUI的控制器
        private PanelController3997 _panelController3997;

        // 免费游戏
        private FreeSpinTimeController _freeSpinTimeController; // 免费游戏次数管理器
        private GComponent _freeFrameCom;
        private GTextField _freeSpinsNumber;
        private GTextField _multipleNumber;
        private GComponent _compareRadar;
        private GameObject _radarObj, _cloneRadarObj;
        private Transform _radarEffectParent;
        private Animator _radarAnimator;

        // 开始游戏
        private bool _tipCoinIn = false, _isStoppedSlotMachine = false;
        private bool _isStopButtonLocked;

        private Coroutine _corGameAuto = null,
            _corReelsTurn = null,
            _corGameIdle = null,
            _corShowFreeSymbol = null,
            _corShowBonusSymbol = null,
            _corEffectSlowMotion = null,
            _corGameOnce = null,
            _corRewardEffect,
            _corLightningEffect;

        // 免费游戏倍数增加特效制作
        private int _freeMultiplier = 2; // 显示在免费游戏text上的倍率 不用ContentModel中的了
        private GameObject _goRewardEffect, _wildBoomEffect;
        private GComponent _rewardEffectCom, _wildBoomCom, _freeParticleEffectParent;

        // 收音机中的倍数遮罩和火焰特效
        private GComponent _compareMaskEffect, _compareFireEffect;
        private GameObject _goFireEffect, _goMaskEffect, _cloneFireEffect, _cloneMaskEffect;

        // 收音机的闪电特效
        private GComponent _lightningParentCom;
        private GameObject _lightningObj, _cloneLightningObj;
        private List<GComponent> _lightningEffectList = new List<GComponent>();

        // 加速框制作
        private GComponent _anchorExpectation;
        private GComponent _anchorFreeExpectation;
        private GComponent _anchorBonusExpectation;
        private GameObject _freeBorderObj = null; // 免费加速特效
        private GameObject _bonusBorderObj = null; // 彩金加速特效
        private readonly List<int> _specialSymbols = new List<int> { 10, 11 };

        readonly List<Dictionary<string, object>> _stackContext = new List<Dictionary<string, object>>();

        private bool _isMain = true;
        long TotalBet => MainModel.Instance.contentMD.totalBet;

        // 机器人Spine动画
        private GameObject _robotObj = null; // 物体模板
        private GameObject _cloneRobotObj = null; // 克隆的物体
        private GComponent _compareRobot = null; // 多分支对照的UI组件

        // 商人3D模型
        private GComponent _compareNpc;
        private GameObject _npcObj, _cloneNpcObj;
        private Animator _traderAnimator = null; // 商人动画

        //当前游戏触发加速框后是否中奖
        private bool _isTriggerFrame = false;
        private bool _isWinFreeOrBonus = false;

        private Transform _npcEffectParent;

        //彩金
        MiniReelGroup uiJPMajorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMinorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMiniCtrl = new MiniReelGroup();

        // 说明书
        private List<GComponent> _lstPayTable;
        private readonly PayTableController3997 _payTableController = new PayTableController3997();

        private bool IsAddCreditAnim =>
            !(_slotMachineCtrl.isStopImmediately == true || SBoxModel.Instance.isCoinOutImmediately);

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();
            _pageController = contentPane.GetController("PageController");
            InitFreeSpinUIAndController();
            LoadAsyncRes();

            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        if (PanelController02.isOpenIntroduce == true)
                            return;

                        EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, false);
                        OnClickSpinButton(res);
                    },
                },
                longClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, true);
                        OnClickSpinButton(res);
                    }
                }
            };
            ReadJsonBet();
        }

        public override void InitParam()
        {
            if (!isInit) return;

            MainModel.Instance.contentMD = ContentModel.Instance;
            MainModel.Instance.cutomMD = CustomModel.Instance;
            // ReadJsonBet();

            // 初始化对象池，通过配置文件读取出中奖特效等
            if (_fGuiPoolHelper != null && _isInitPool == false)
            {
                _isInitPool = true;

                _fGuiPoolHelper.Add(TagPoolObject.SymbolHit,
                    CustomModel.Instance.symbolHitEffect.Values.ToList(), "symbol_hit#", 5);
                _fGuiPoolHelper.PreLoad(TagPoolObject.SymbolHit);

                _fGuiPoolHelper.Add(TagPoolObject.SymbolBorder,
                    CustomModel.Instance.borderEffect, "border#", 5);
                _fGuiPoolHelper.PreLoad(TagPoolObject.SymbolBorder);

                _fGuiPoolHelper.Add(TagPoolObject.SymbolAppear,
                    CustomModel.Instance.symbolAppearEffect.Values.ToList(), "symbol_appear#", 10);
                _fGuiPoolHelper.PreLoad(TagPoolObject.SymbolAppear);
            }

            ShowPayTable();
            // 加载Panel面板
            _gOwnerPanel = contentPane.GetChild("panel").asCom;
            MainModel.Instance.contentMD = ContentModel.Instance;
            MainModel.Instance.cutomMD = CustomModel.Instance;
            ContentModel.Instance.goAnthorPanel = _gOwnerPanel;
            MainModel.Instance.contentMD.goAnthorPanel = _gOwnerPanel;
            TryTriggerAnchorPanelChange();

            // 初始化滚轴界面
            GComponent gSlotMachine = contentPane.GetChild("slotMachine").asCom;
            GComponent gReels = gSlotMachine.GetChild("reels").asCom;
            GComponent gSlotCover = gSlotMachine.asCom.GetChild("slotCover").asCom;
            GComponent gPlayLines = gSlotMachine.asCom.GetChild("playLines").asCom;
            GComponent gFrame = contentPane.GetChild("anchorFrame").asCom;
            _slotMachineCtrl.Init(gSlotCover, gPlayLines, gReels, gFrame, _fGuiPoolHelper, _fGuiGObjectPoolHelper);

            if (_wildBoomCom != null)
                _wildBoomCom.Dispose();
            if (_rewardEffectCom != null)
                _rewardEffectCom.Dispose();

            // 粒子特效功能制作
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
            for (int i = 0; i < _lightningEffectList.Count; i++)
            {
                if (_lightningEffectList[i] != null)
                    _lightningEffectList[i].Dispose();
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

            BindSpinesToUI();

            preLoadedCallback?.Invoke();
            if (!isOpen) return;

            if (_anchorFreeExpectation != null)
                _anchorFreeExpectation.Dispose();
            if (_anchorBonusExpectation != null)
                _anchorBonusExpectation.Dispose();
            // 加速框
            _anchorExpectation = contentPane.GetChild("anchorReelEffect").asCom;
            _anchorFreeExpectation = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            _anchorBonusExpectation = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            GameCommon.FguiUtils.DeleteWrapper(_anchorFreeExpectation);
            GameCommon.FguiUtils.AddWrapper(_anchorFreeExpectation, Object.Instantiate(_freeBorderObj));
            _anchorFreeExpectation.visible = false;
            GameCommon.FguiUtils.DeleteWrapper(_anchorBonusExpectation);
            GameCommon.FguiUtils.AddWrapper(_anchorBonusExpectation, Object.Instantiate(_bonusBorderObj));
            _anchorBonusExpectation.visible = false;
            _anchorExpectation.AddChild(_anchorFreeExpectation);
            _anchorExpectation.AddChild(_anchorBonusExpectation);
            _anchorExpectation.visible = true;

            //彩金
            uiJPMajorCtrl.Init("Major", contentPane.GetChild("jpMajor").asCom.GetChild("n1").asList, "N0");
            uiJPMinorCtrl.Init("Minor", contentPane.GetChild("jpMinor").asCom.GetChild("n1").asList, "N0");
            uiJPMiniCtrl.Init("Mini", contentPane.GetChild("jpMini").asCom.GetChild("n1").asList, "N0");

            uiJPMajorCtrl.SetReelWidth(30);
            uiJPMinorCtrl.SetReelWidth(30);
            uiJPMiniCtrl.SetReelWidth(30);

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

                    uiJPMajorCtrl.SetData(minorBet);
                    uiJPMinorCtrl.SetData(majorBet);
                    uiJPMiniCtrl.SetData(miniBet);
                });
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
                        MainBlackboardController.Instance.SetMyRealCredit(playerAccountList[i].Credit);
                        break;
                    }
                }
            }, (err) =>
            {
                DebugUtils.Log(err.msg);
            });
            MainBlackboardController.Instance.SyncMyTempCreditToReal(true);

            // 总押注初始化
            ContentModel.Instance.betIndex = 0;
            ContentModel.Instance.totalBet = SBoxModel.Instance.betList[ContentModel.Instance.betIndex];
            TryRestoreFreeSpinSession();
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            GameSoundHelper3997.Instance.PlayMusicSingle(SoundKey.RegularBG);
            base.OnOpen(currentPageName, eventData);
            _isMain = true;
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnPanelInputEvent);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT, OnSlotDetailEvent);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnStopSlot);
            EventCenter.Instance.AddEventListener<CoinPushSpinParseEventArgs>(SBoxEventHandle.SBOX_COIN_PUSH_SPIN_PARSE,
                OnCoinPushSpinResultParse);
            EventCenter.Instance.AddEventListener<WinJackpotInfo>(GlobalEvent.JackpotOnlineWin, OnJackpotOnLine);
            InitParam();
        }

        public override void OnClose(EventData eventData = null)
        {
            UnlockStopButton();
            OnGameReset();
            _lastAnchorPanelForDispatch = null;

            GameSoundHelper3997.Instance.StopSound(SoundKey.RegularBG);
            EventCenter.Instance.RemoveEventListener<CoinPushSpinParseEventArgs>(
                SBoxEventHandle.SBOX_COIN_PUSH_SPIN_PARSE, OnCoinPushSpinResultParse);
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnPanelInputEvent);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT,
                OnSlotDetailEvent);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnStopSlot);
            EventCenter.Instance.RemoveEventListener<WinJackpotInfo>(GlobalEvent.JackpotOnlineWin, OnJackpotOnLine);
            base.OnClose(eventData);
            _freeSpinTimeController.Dispose();
            // _isReady = false;
        }

        private void TryTriggerAnchorPanelChange()
        {
            if (_gOwnerPanel == null)
            {
                return;
            }

            if (ReferenceEquals(_lastAnchorPanelForDispatch, _gOwnerPanel))
            {
                return;
            }

            _lastAnchorPanelForDispatch = _gOwnerPanel;
            EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
                new EventData<GComponent>(PanelEvent.AnchorPanelChange, _gOwnerPanel));
        }

        private void OnCoinPushSpinResultParse(CoinPushSpinParseEventArgs e)
        {
            e.Result = MachineDataController3997.ParseCoinPushSpinPayload(e.Data, e.StartPos);
        }

        protected override void OnLanguageChange(I18nLang lang)
        {
            FguiI18nTextAssistant.Instance.DisposeAllTranslate(contentPane);
            contentPane.Dispose(); // 释放当前UI
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            _pageController = contentPane.GetController("PageController");
            InitFreeSpinUIAndController();
            InitParam();
            Debug.LogError("语言切换");
        }

        #region 初始化 (预制体资源、配置文件以及第一次显示界面)

        private void LoadAsyncRes()
        {
            _totalCount = 12;

            // 加载公共资源包
            if (UIPackage.GetByName("Common") == null)
            {
                ResourceManager02.Instance.LoadAssetBundleAsync("Assets/GameRes/Games/Common/FGUIs", (bundle) =>
                {
                    _totalCount++;
                    ResLoadedCallback();
                });
            }

            // 加载游戏控制器
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
                    _panelController3997 = _goGameCtrl.GetComponentInChildren<PanelController3997>();
                    ResLoadedCallback();
                });

            // 加载游戏配置文件
            ResourceManager02.Instance.LoadAsset<TextAsset>(
                ConfigUtils.GetGameInfoURL(3997), (txt) =>
                {
                    _gameInfo = txt;
                    ResLoadedCallback();
                });

            // 加载Spine动画
            ResourceManager02.Instance.LoadAsset<GameObject>(
                SpinePrefabPath + "Radar.prefab",
                (clone) =>
                {
                    _radarObj = clone;
                    ResLoadedCallback();
                });


            // 加载特效
            ResourceManager02.Instance.LoadAsset<GameObject>(
                EffectPrefabPath + "FreeAccelerateBorder.prefab",
                (clone) =>
                {
                    _freeBorderObj = clone;
                    ResLoadedCallback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                EffectPrefabPath + "BonusAccelerateBorder.prefab",
                (clone) =>
                {
                    _bonusBorderObj = clone;
                    ResLoadedCallback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                EffectPrefabPath + "robotObj.prefab",
                (clone) =>
                {
                    _robotObj = clone;
                    ResLoadedCallback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                EffectPrefabPath + "RewardEffect.prefab",
                (clone) =>
                {
                    _goRewardEffect = clone;
                    ResLoadedCallback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                EffectPrefabPath + "WildBoomEffect.prefab",
                (clone) =>
                {
                    _wildBoomEffect = clone;
                    ResLoadedCallback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                EffectPrefabPath + "FireEffect.prefab",
                (clone) =>
                {
                    _goFireEffect = clone;
                    ResLoadedCallback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                EffectPrefabPath + "MaskEffect.prefab",
                (clone) =>
                {
                    _goMaskEffect = clone;
                    ResLoadedCallback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                EffectPrefabPath + "Lightning.prefab",
                (clone) =>
                {
                    _lightningObj = clone;
                    ResLoadedCallback();
                });

            // Todo:等3D模型绑定完成之后处理的逻辑
            // 加载3D动画  
            ResourceManager02.Instance.LoadAsset<GameObject>(
                ModelPrefabPath + "Wealth_ng_npc.prefab",
                (clone) =>
                {
                    _npcObj = clone;
                    ResLoadedCallback();
                });
        }

        private void ResLoadedCallback()
        {
            if (--_totalCount == 0)
            {
                isInit = true;
                InitParam();
            }
        }

        private void ReadJsonBet()
        {
            // //资源加载
            // ResourceManager02.Instance.LoadAsset<TextAsset>(
            //     "Assets/GameRes/_Common/Game Maker/ABs/G3997/Datas/game_info_g3997.json", (txt) =>
            //     {
            //         //JSON解析与错误处理
            //         GameConfigRoot config = JsonConvert.DeserializeObject<GameConfigRoot>(txt.text);
            //         if (config?.SymbolPaytable == null)
            //         {
            //             Debug.LogError("解析symbol_paytable失败，数据为空");
            //             return;
            //         }
            //
            //         MainModel.Instance.lineNum = config.LineNum;
            //         MainModel.Instance.gameID = config.GameId;
            //         MainModel.Instance.gameName = config.GameName;
            //         MainModel.Instance.displayName = config.DisplayName;
            //     });

            MainModel.Instance.lineNum = 20;
            MainModel.Instance.gameID = 3997;
            MainModel.Instance.gameName = "CaiFuZhiJia3997";
            MainModel.Instance.displayName = "CaiFuZhiJia_3997";
        }

        private void BindSpinesToUI()
        {
            // 免费游戏的机器人投影特效
            GComponent currentCom = contentPane.GetChild("anchorRobot").asCom;
            if (currentCom != _compareRobot)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareRobot);
                _compareRobot = currentCom;
                _cloneRobotObj = Object.Instantiate(_robotObj);
                GameCommon.FguiUtils.AddWrapper(currentCom, _cloneRobotObj);
            }

            // Todo: 绑定商人3D模型
            currentCom = contentPane.GetChild("anchorPlayer").asCom;
            if (currentCom != _compareNpc)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareNpc);
                _compareNpc = currentCom;
                _cloneNpcObj = Object.Instantiate(_npcObj);
                _traderAnimator = _cloneNpcObj.GetComponentInChildren<Animator>();
                _npcEffectParent = _cloneNpcObj.transform.Find("Effect");
                GameCommon.FguiUtils.AddWrapper(currentCom, _cloneNpcObj);
            }

            currentCom = contentPane.GetChild("freeGameBg").asCom.GetChild("anchorFire").asCom;
            if (currentCom != _compareFireEffect)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareFireEffect);
                _compareFireEffect = currentCom;
                _cloneFireEffect = Object.Instantiate(_goFireEffect);
                _cloneFireEffect.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(_compareFireEffect, _cloneFireEffect);
            }

            currentCom = contentPane.GetChild("freeGameBg").asCom.GetChild("anchorMask").asCom;
            if (currentCom != _compareMaskEffect)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareMaskEffect);
                _compareMaskEffect = currentCom;
                _cloneMaskEffect = Object.Instantiate(_goMaskEffect);
                _cloneMaskEffect.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(_compareMaskEffect, _cloneMaskEffect);
            }

            // 免费游戏收音机
            currentCom = contentPane.GetChild("freeGameBg").asCom.GetChild("anchorVideo").asCom;
            if (currentCom != _compareRadar)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareRadar);
                _compareRadar = currentCom;
                _cloneRadarObj = Object.Instantiate(_radarObj);
                _radarAnimator = _cloneRadarObj.GetComponentInChildren<Animator>();
                _radarEffectParent = _cloneRadarObj.transform.Find("Effect").transform.Find("eff_fg_img_multiple9");
                GameCommon.FguiUtils.AddWrapper(_compareRadar, _cloneRadarObj);
            }
        }

        private void InitFreeSpinUIAndController()
        {
            _freeSpinTimeController = new FreeSpinTimeController();
            _freeFrameCom = contentPane.GetChild("FSFrame").asCom;
            _freeSpinsNumber = _freeFrameCom.GetChild("FreeSpinsNumber").asTextField;
            _multipleNumber = contentPane.GetChild("freeGameBg").asCom.GetChild("multipleNumber").asTextField;
            _freeParticleEffectParent = contentPane.GetChild("anchor_EffectParent").asCom;
            _lightningParentCom = contentPane.GetChild("lightningEffectParent").asCom;
            _freeSpinTimeController.InitParam(_freeSpinsNumber);
        }

        private void ShowPayTable()
        {
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
        }

        #endregion

        #region 游戏主逻辑 (Normal Game、Free Game 以及Bonus Game等)

        /// <summary>
        /// Panel点击事件
        /// </summary>
        /// <param name="res"></param>
        void OnPanelInputEvent(EventData res)
        {
            if (_isMain)
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
        }

        /// <summary>
        /// 自动旋转次数
        /// </summary>
        /// <param name="res"></param>
        void OnClickTotalSpinsButtonClick(EventData res)
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

        /// <summary>
        /// 单次点击Spin按钮的逻辑
        /// </summary>
        /// <param name="res"></param>
        void OnClickSpinButton(EventData res)
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
                                //StartGameTotalSpins(successCallback, StopGameWhenError); //开始玩
                                StartGameOnce(successCallback, StopGameWhenError); //开始玩
                            }
                        }
                        break;

                    case SpinButtonState.Spin:
                        {
                            if (!ContentModel.Instance.isSpin) return; // 已经停止直接退出
                            if (_isStopButtonLocked) return;
                            LockStopButton();
                            _slotMachineCtrl.isStopImmediately = true; // 去停止游戏  
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
                if (_corGameIdle != null) _monoHelper.StopCoroutine(_corGameIdle);
                _monoHelper.StartCoroutine(_slotMachineCtrl.NudgeReelOneStep(col, null, false, ReelNudgeDirection.Up));
            }

            if (res.name == "ColDownButtonClick")
            {
                int col = (int)res.value;
                if (_corGameIdle != null) _monoHelper.StopCoroutine(_corGameIdle);
                _monoHelper.StartCoroutine(
                    _slotMachineCtrl.NudgeReelOneStep(col, null, false, ReelNudgeDirection.Down));
            }
        }

        void OnSlotDetailEvent(EventData res)
        {
            switch (res.name)
            {
                case SlotMachineEvent.PrepareStoppedReel:
                    {
                        if (ContentModel.Instance.isReelsSlowMotion && !_slotMachineCtrl.isStopImmediately)
                        {
                            AnimatorStateInfo temp = _traderAnimator.GetCurrentAnimatorStateInfo(0);
                            if (!temp.IsName("Wealth_ng_npc_atmosphere"))
                                PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_atmosphere");
                            int colIndex = (int)res.value;
                            if (colIndex == 1)
                            {
                                if (_corEffectSlowMotion != null) _monoHelper.StopCoroutine(_corEffectSlowMotion);
                                _corEffectSlowMotion = _monoHelper.StartCoroutine(ShowEffectReelsSlowMotion(1));
                            }
                            else if (colIndex == 2)
                            {
                                if (_corEffectSlowMotion != null) _monoHelper.StopCoroutine(_corEffectSlowMotion);
                                _corEffectSlowMotion = _monoHelper.StartCoroutine(ShowEffectReelsSlowMotion(2));
                            }
                            else if (colIndex == 3)
                            {
                                if (_corEffectSlowMotion != null) _monoHelper.StopCoroutine(_corEffectSlowMotion);
                                _corEffectSlowMotion = _monoHelper.StartCoroutine(ShowEffectReelsSlowMotion(3));
                            }
                            else if (colIndex == 4)
                            {
                                if (_corEffectSlowMotion != null) _monoHelper.StopCoroutine(_corEffectSlowMotion);
                                _corEffectSlowMotion = _monoHelper.StartCoroutine(ShowEffectReelsSlowMotion(4));
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
                    _isStoppedSlotMachine = true;
                    UnlockStopButton();
                    break;
            }
        }

        //下注时向大厅彩金主机发送当前下注
        void RequestOnlineJackpotBetByCurrentBet()
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

        private static string GetOnlineJackpotName(int jackpotId)
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
                }, (BagelCodeError err) =>
                {
                    DebugUtils.Log(err.msg);
                });
            }
            catch (Exception ex)
            {
                DebugUtils.LogError($"处理大厅彩金中奖下发失败: {ex.Message}");
            }
        }

        private IEnumerator ShowEffectReelsSlowMotion(int colIdx)
        {
            _isTriggerFrame = true;
            GComponent comReelEffect = _anchorBonusExpectation;
            if (ContentModel.Instance.isFreeSlotTip)
            {
                comReelEffect = _anchorFreeExpectation;
            }

            comReelEffect.visible = false;
            comReelEffect.xy = _slotMachineCtrl.SymbolCenterToNodeLocalPos(colIdx, 1, _anchorExpectation);
            comReelEffect.visible = true;
            // GameSoundHelper.Instance.PlaySoundEff(SoundKey.SlowMotionEffect);

            yield return new WaitUntil(() => _isStoppedSlotMachine == true);
            comReelEffect.visible = false;
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

        #region Cwy_Custom

        IEnumerator ShowWinSymbol(int number, Action callback = null)
        {
            SymbolWin curSymbolWin = new SymbolWin();
            curSymbolWin.symbolNumber = number;
            List<List<int>> colRowLst = ParseVertical(ContentModel.Instance.strDeckRowCol);
            int count = 0;
            for (int col = 0; col < colRowLst.Count; col++)
            {
                for (int row = 0; row < colRowLst[col].Count; row++)
                {
                    if (colRowLst[col][row] == number)
                    {
                        curSymbolWin.cells.Add(new Cell(col, row));
                        count++;
                    }
                }
            }

            yield return _slotMachineCtrl.ShowSymbolWinBySetting(curSymbolWin, true,
                SpinWinEvent.SingleWinLine);
            callback?.Invoke();
        }

        IEnumerator ProcessWildList(Action callback)
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
                if (_freeMultiplier > 4)
                {
                    _cloneFireEffect.SetActive(true);
                    _radarEffectParent.Find("effect1").gameObject.SetActive(true);
                    PlayAnimationByName(_radarAnimator, "idle2");
                }

                if (_freeMultiplier > 7)
                {
                    _radarEffectParent.Find("effect2").gameObject.SetActive(true);
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

        IEnumerator ProcessLightningList(Action callback)
        {
            var coroutines = new List<Coroutine>();
            for (int i = 0; i < ContentModel.Instance.currentWildList.Count; i++)
            {
                Cell temp = ContentModel.Instance.currentWildList[i];
                Coroutine c = _monoHelper.StartCoroutine(ShowLighningEffect(
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


        private IEnumerator ShowLighningEffect(GComponent startPosCom, GComponent toNode, int colIdx, int rowIdx)
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

        private IEnumerator MoveToZeroOverTime(GComponent effect, Vector2 startPosition, float duration = 1f,
            Action successCallback = null)
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

        private IEnumerator MoveToEndPosTime(GComponent effect, Vector2 endPos, float duration = 1f,
            Action successCallback = null)
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

        private static List<List<int>> ParseVertical(string raw,
            int expectedCols = 5) // 已知 5 列可写死，也可调用时传
        {
            var result = new List<List<int>>();

            if (string.IsNullOrEmpty(raw)) return result;

            // 1. 横排拆成二维
            var rows = raw
                .Split('#')
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => r.Split(',')
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Select(c => int.Parse(c.Trim()))
                    .ToList())
                .ToList();

            if (rows.Count == 0) return result;

            // 2. 简单校验：每行列数必须一致
            int colCount = rows[0].Count;
            for (int i = 1; i < rows.Count; i++)
            {
                if (rows[i].Count != colCount)
                {
                    Debug.LogError($"第{i}行列数不一致，期望{colCount}，实际{rows[i].Count}");
                    return result;
                }
            }

            // 3. 竖着转置
            for (int c = 0; c < colCount; c++)
            {
                var oneCol = new List<int>(rows.Count);
                for (int r = 0; r < rows.Count; r++)
                    oneCol.Add(rows[r][c]);
                result.Add(oneCol);
            }

            return result;
        }

        #endregion

        /// 触发免费游戏以及免费游戏一整个流程的执行
        /// <param name="successCallback"></param>
        /// <param name="errorCallback"></param>
        /// <returns></returns>
        IEnumerator FreeSpinTrigger(Action successCallback, Action<string> errorCallback)
        {
            bool isNext = false;
            InputStackContextFreeSpin((context) =>
            {
                _freeSpinsNumber.text = ContentModel.Instance.FreeSpinTotalTimes.ToString();
            });
            PageManager.Instance.OpenPageAsync(PageName.CaiFuZhiJiaPopupFreeSpinTrigger,
                new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object>() { ["freeSpinCount"] = ContentModel.Instance.FreeSpinTotalTimes, }),
                (ed) =>
                {
                    _slotMachineCtrl.SendTotalWinCreditEvent(0);
                    _pageController.selectedPage = "FreeGame";
                    // PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_idle01");
                    isNext = true;
                });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            _slotMachineCtrl.BeginBonusFreeSpin();
            yield return GameFreeSpin(null, errorCallback);


            PlayAnimationByName(_radarAnimator, "Settlement");
            PlayAnimationByName(_traderAnimator, "Wealth_fg_npc_settlement");
            _cloneRadarObj.transform.Find("Effect").transform.Find("eff_fg_img_multiple11").gameObject.SetActive(true);
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
                    ContentModel.Instance.isFreeSpinTrigger = false;
                });

            _slotMachineCtrl.EndBonusFreeSpin();
            PageManager.Instance.OpenPageAsync(PageName.CaiFuZhiJiaPopupFreeSpinResult,
                new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object>()
                    {
                        ["baseGameWinCredit"] = ContentModel.Instance.freeSpinTotalWinCoins
                    }),
                (ed) =>
                {
                    _pageController.selectedPage = "NormalGame";
                    ContentModel.Instance.freeGameScoreMultiply = 2;
                    _multipleNumber.text = "x2";
                    _freeMultiplier = 2;
                    for (int i = 0; i < _lightningEffectList.Count; i++)
                        _lightningEffectList[i].visible = false;
                    _wildBoomCom.visible = false;
                    _rewardEffectCom.visible = false;
                    _cloneFireEffect.SetActive(false);
                    _radarEffectParent.Find("effect1").gameObject.SetActive(false);
                    _radarEffectParent.Find("effect2").gameObject.SetActive(false);
                    MainBlackboardController.Instance.AddMyTempCredit(_allWinCredit, true, IsAddCreditAnim); //加钱动画
                    MainBlackboardController.Instance.SyncMyTempCreditToReal(true);
                    _allWinCredit = 0;
                    ContentModel.Instance.FreeSpinTotalTimes = 0; // 免费游戏结束之后，把免费游戏局数重置
                    _cloneRadarObj.transform.Find("Effect").transform.Find("eff_fg_img_multiple11").gameObject
                        .SetActive(false);

                    // 重新注册
                    ContentModel.Instance.goAnthorPanel = _gOwnerPanel;
                    MainModel.Instance.contentMD.goAnthorPanel = _gOwnerPanel;
                    TryTriggerAnchorPanelChange();


                    isNext = true;
                });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;
            yield return _slotMachineCtrl.SlotWaitForSeconds(1.5f);
        }

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

        void OutputStackContextFreeSpin(Action<Dictionary<string, object>> outputStackCallBack)
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

        private void StartGameAuto(Action successCallback = null, Action<string> errorCallback = null)
        {
            if (_corGameAuto != null) _monoHelper.StopCoroutine(_corGameAuto);
            _corGameAuto = _monoHelper.StartCoroutine(GameAuto(successCallback, errorCallback));
        }

        void StartGameTotalSpins(Action successCallback = null, Action<string> errorCallback = null)
        {
            if (_corGameAuto != null) _monoHelper.StopCoroutine(_corGameAuto);
            _corGameAuto = _monoHelper.StartCoroutine(GameTotalSpins(successCallback, errorCallback));
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

            if (successCallback != null)
                successCallback.Invoke();
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

            if (successCallback != null)
                successCallback.Invoke();
        }

        void StartGameOnce(Action successCallback = null, Action<string> errorCallback = null)
        {
            ContentModel.Instance.totalPlaySpins = 1;
            ContentModel.Instance.remainPlaySpins = 1;
            _corGameOnce = _monoHelper.StartCoroutine(GameOnce(successCallback, errorCallback));
        }


        IEnumerator GameFreeSpin(Action successCallback, Action<string> errorCallback)
        {
            while (ContentModel.Instance.nextReelStripsIndex == "FS")
            {
                yield return GameFreeSpinOnce(null, errorCallback);
                yield return _slotMachineCtrl.SlotWaitForSeconds(1);
            }

            if (successCallback != null)
                successCallback.Invoke();
        }

        long _allWinCredit = 0;

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
                errorCallback?.Invoke(errMsg);
                yield break;
            }

            _slotMachineCtrl.BeginSpin();


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
                _corReelsTurn = _monoHelper.StartCoroutine(_slotMachineCtrl.TurnReelsNormal(_specialSymbols,
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

            if (ContentModel.Instance.isHaveWildSymbol)
            {
                if (_freeMultiplier < 5)
                {
                    PlayAnimationByName(_traderAnimator, "Wealth_fg_npc_upgrade1");
                    PlayAnimationByName(_radarAnimator, "upgrade");
                    yield return new WaitForSeconds(3f);
                    PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_idle01");
                }
                else
                {
                    PlayAnimationByName(_traderAnimator, "Wealth_fg_npc_upgrade2");
                    PlayAnimationByName(_radarAnimator, "upgrade");
                    yield return new WaitForSeconds(4.3f);
                    PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_idle01");
                }

                isNext = false;
                if (_corLightningEffect != null) _monoHelper.StopCoroutine(_corLightningEffect);
                _corLightningEffect = _monoHelper.StartCoroutine(ProcessLightningList(() => isNext = true));
                yield return new WaitUntil(() => isNext == true);
                yield return new WaitForSeconds(0.5f);
                isNext = false;
                if (_corRewardEffect != null) _monoHelper.StopCoroutine(_corRewardEffect);
                _corRewardEffect = _monoHelper.StartCoroutine(ProcessWildList(() => isNext = true));
                yield return new WaitUntil(() => isNext == true);
                isNext = false;
                ContentModel.Instance.isHaveWildSymbol = false;
            }

            // 线赢的数据
            List<SymbolWin> winList = ContentModel.Instance.winList;

            #region Win

            if (winList.Count > 0 || ContentModel.Instance.BonusResults != null)
            {
                long totalWinLineCredit = _slotMachineCtrl.GetTotalWinCredit(winList); // 新增倍率
                if (ContentModel.Instance.isPowerTrigger)
                {
                    _allWinCredit += ContentModel.Instance.freeSpinTotalWinCoins - totalWinLineCredit; // 测试
                    ContentModel.Instance.isPowerTrigger = false;
                }


                // 播放3D人物动画
                if (totalWinLineCredit < TotalBet * 2 && totalWinLineCredit > 0)
                {
                    PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_win1");
                    // yield return new WaitForSeconds(2.667f);
                    // PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_idle01");
                    Timers.inst.Add(2.667f, 1, (obj) => PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_idle01"));
                }
                else if (totalWinLineCredit >= TotalBet * 2 && totalWinLineCredit < TotalBet * 3)
                {
                    PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_win2");
                    _npcEffectParent.Find("npc_ng_npc_win2").gameObject.SetActive(true);
                    // yield return new WaitForSeconds(4.167f);
                    // PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_idle01");
                    Timers.inst.Add(4.167f, 1, (obj) =>
                    {
                        PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_idle01");
                        _npcEffectParent.Find("npc_ng_npc_win2").gameObject.SetActive(false);
                    });
                }
                else if (totalWinLineCredit >= TotalBet * 3 && totalWinLineCredit < TotalBet * 5)
                {
                    PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_win3");
                    _npcEffectParent.Find("npc_ng_npc_win3").gameObject.SetActive(true);
                    // yield return new WaitForSeconds(3.167f);
                    // PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_idle01");
                    Timers.inst.Add(3.167f, 1, (obj) =>
                    {
                        _npcEffectParent.Find("npc_ng_npc_win3").gameObject.SetActive(false);
                        PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_idle01");
                    });
                }
                else if (totalWinLineCredit >= 5)
                {
                    PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_win3");
                    _npcEffectParent.Find("npc_ng_npc_win3").gameObject.SetActive(true);
                    yield return new WaitForSeconds(3.167f);
                    PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_idle01");
                    _npcEffectParent.Find("npc_ng_npc_win3").gameObject.SetActive(false);
                }

                // 新增BigWin
                WinLevelType winLevelType = GetBigWinType();
                if (winLevelType != WinLevelType.None)
                {
                    _compareNpc.visible = false;
                    yield return BigWinPopup(winLevelType, ContentModel.Instance.baseGameWinCredit);

                    _slotMachineCtrl.CloseSlotCover();
                    _slotMachineCtrl.SkipWinLine(false);
                }

                if (winLevelType == WinLevelType.BIG)
                {
                    Timers.inst.Add(4, 1, (obj) => _compareNpc.visible = true);
                }
                else if (winLevelType == WinLevelType.HUGE)
                {
                    Timers.inst.Add(7, 1, (obj) => _compareNpc.visible = true);
                }
                else if (winLevelType == WinLevelType.MASSIVE)
                {
                    Timers.inst.Add(10, 1, (obj) => _compareNpc.visible = true);
                }

                _allWinCredit += totalWinLineCredit;
                // Debug.LogError("_allWinCredit:" + _allWinCredit + "          totalWinLineCredit: " +
                //                totalWinLineCredit + "        ContentModel.Instance.freeSpinTotalWinCoins: " +
                //                ContentModel.Instance.freeSpinTotalWinCoins);
                _slotMachineCtrl.SendTotalWinCreditEvent(_allWinCredit); // 总线赢分事件
            }

            #endregion

            isNext = false;

            if (winList.Count > 0 || false)
            {
                yield return ShowWinListCoinCountDown(winList, _allWinCredit, false);
            }

            ContentModel.Instance.gameState = GameState.Idle;
            successCallback?.Invoke();
        }

        /// <summary> 从本地快照恢复未完成的免费局（不自动请求 Spin，由玩家点转）。 </summary>
        void TryRestoreFreeSpinSession()
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
                _pageController.selectedPage = "FreeGame";
                _multipleNumber.text = "x" + ContentModel.Instance.freeGameScoreMultiply;
                _freeSpinsNumber.text =
                    (ContentModel.Instance.FreeSpinTotalTimes - ContentModel.Instance.FreeSpinPlayTimes).ToString();
            }


            _slotMachineCtrl.SendTotalWinCreditEvent(cm.freeSpinTotalWinCoins);
            DebugUtils.Log(
                $"[G3997] 已恢复免费局快照：剩余 {cm.ShowFreeSpinRemainTime} / 总 {cm.FreeSpinTotalTimes}，待首局 Spin 与算法校验。");
        }

        private bool _isCurrentWin; // 用作判断出现加速框但未重彩金或免费游戏，但是中了普通奖的先后顺序的

        IEnumerator GameOnce(Action successCallback, Action<string> errorCallback)
        {
            // 检测机台是否激活
            if (!SBoxModel.Instance.isMachineActive)
            {
                errorCallback?.Invoke(I18nMgr.language == I18nLang.cn
                    ? "请激活机台"
                    : "<size=24>Machine not activated!</size>");
                yield break;
            }

            if (ContentModel.Instance.FreeSpinTotalTimes > 0 && ContentModel.Instance.nextReelStripsIndex == "FS")
            {
                Debug.LogError("进入断电重连");
                yield return GameFreeSpinFromReconnect(successCallback, errorCallback);
                yield break;
            }

            // 检测玩家积分是否足够
            if (SBoxModel.Instance.myCredit < TotalBet)
            {
                _tipCoinIn = true;
                errorCallback?.Invoke(
                    I18nMgr.language == I18nLang.cn
                        ? "积分不足，请先充值"
                        : "<size=15>Balance is insufficient, please recharge first</size>");
                yield break;
            }

            // 检查算法积分
            MachineDataManager02.Instance.RequestGetPlayerInfo((res) =>
            {
                SBoxAccount data = (SBoxAccount)res;
                int pid = SBoxModel.Instance.pid;
                List<SBoxPlayerAccount> playerAccountList = data.PlayerAccountList;
                for (int i = 0; i < playerAccountList.Count; i++)
                {
                    if (playerAccountList[i].PlayerId != pid)
                        continue;

                    DebugUtils.Log("前一局算法卡CoinIn==" + playerAccountList[i].CoinIn);
                    DebugUtils.Log("前一局算法卡Bet==" + playerAccountList[i].Bets);
                    DebugUtils.Log("前一局算法卡Credit==" + playerAccountList[i].Credit);
                    break;
                }
            }, (err) => DebugUtils.Log(err.msg));


            // 重置游戏状态，开始旋转准备
            OnGameReset();
            ContentModel.Instance.gameState = GameState.Spin;
            _slotMachineCtrl.BeginTurn();
            bool isNext = false;
            bool isBreak = false;
            string errMsg = "";

            _isWinFreeOrBonus = false;
            _isTriggerFrame = false;

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
                            wheelChessNum = deckData.Length, data = deckData
                        };
                        SBoxIdea.SetExhibitionData(sBoxExhibitionData);
                    }
                    catch (Exception e)
                    {
                        DebugUtils.LogError($"[G1700] 设置展会模式结果失败，deck={currentDeck}");
                        DebugUtils.LogException(e);
                    }
                }
            }

            if (ApplicationSettings.Instance.isMock) // 模拟结果
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
            else // 真实结果
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

            // 检查是否启用在线彩金,请求彩金数据
            if (SBoxModel.Instance.isJackpotOnLine)
                RequestOnlineJackpotBetByCurrentBet();

            // 开始滚动
            _slotMachineCtrl.BeginSpin();
            if (ContentModel.Instance.isReelsSlowMotion) // 开启滚轮慢动作的话 滚轮停止之后播放特效
                _slotMachineCtrl.ShowSymbolAppearEffectAfterReelStop(true);
            else // 否则没中奖才播放特效
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
            }
            else
            {
                if (_corReelsTurn != null) _monoHelper.StopCoroutine(_corReelsTurn);
                _corReelsTurn = _monoHelper.StartCoroutine(_slotMachineCtrl.TurnReelsNormal(_specialSymbols,
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


            // 线赢的数据
            List<SymbolWin> winList = ContentModel.Instance.winList;
            long allWinCredit = 0;


            // 普通赢
            if (winList.Count > 0)
            {
                _isCurrentWin = true;
                long totalWinLineCredit = 0;
                totalWinLineCredit = _slotMachineCtrl.GetTotalWinCredit(winList);
                allWinCredit = totalWinLineCredit;

                // 播放3D人物动画
                if (totalWinLineCredit < TotalBet * 2 && totalWinLineCredit > 0)
                {
                    PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_win1");
                    // yield return new WaitForSeconds(2.667f);
                    // PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_idle01");
                    Timers.inst.Add(2.667f, 1, (obj) => PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_idle01"));
                }
                else if (totalWinLineCredit >= TotalBet * 2 && totalWinLineCredit < TotalBet * 3)
                {
                    PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_win2");
                    _npcEffectParent.Find("npc_ng_npc_win2").gameObject.SetActive(true);
                    // yield return new WaitForSeconds(4.167f);
                    // PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_idle01");
                    Timers.inst.Add(4.167f, 1, (obj) =>
                    {
                        _npcEffectParent.Find("npc_ng_npc_win2").gameObject.SetActive(false);
                        PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_idle01");
                    });
                }
                else if (totalWinLineCredit >= TotalBet * 3 && totalWinLineCredit < TotalBet * 5)
                {
                    PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_win3");
                    _npcEffectParent.Find("npc_ng_npc_win3").gameObject.SetActive(true);
                    // yield return new WaitForSeconds(3.167f);
                    // PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_idle01");
                    Timers.inst.Add(3.167f, 1, (obj) =>
                    {
                        _npcEffectParent.Find("npc_ng_npc_win3").gameObject.SetActive(false);
                        PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_idle01");
                    });
                }
                else if (totalWinLineCredit >= 5)
                {
                    PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_win3");
                    _npcEffectParent.Find("npc_ng_npc_win3").gameObject.SetActive(true);
                    yield return new WaitForSeconds(3.167f);
                    PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_idle01");
                    _npcEffectParent.Find("npc_ng_npc_win3").gameObject.SetActive(false);
                }

                // 新增BigWin
                WinLevelType winLevelType = GetBigWinType();
                // Debug.LogError("winLevelType:" + winLevelType);
                if (winLevelType != WinLevelType.None)
                {
                    // _cloneNpcObj.SetActive(false);
                    _compareNpc.visible = false;
                    yield return BigWinPopup(winLevelType, ContentModel.Instance.baseGameWinCredit);

                    _slotMachineCtrl.CloseSlotCover();
                    _slotMachineCtrl.SkipWinLine(false);
                }

                if (winLevelType == WinLevelType.BIG)
                {
                    Timers.inst.Add(4, 1, (obj) => _compareNpc.visible = true);
                }
                else if (winLevelType == WinLevelType.HUGE)
                {
                    Timers.inst.Add(7, 1, (obj) => _compareNpc.visible = true);
                }
                else if (winLevelType == WinLevelType.MASSIVE)
                {
                    Timers.inst.Add(10, 1, (obj) => _compareNpc.visible = true);
                }

                // 计数没到五局中奖之后刷新计数
                if (ContentModel.Instance.noWinCount < 5)
                    ContentModel.Instance.noWinCount = 0;

                _slotMachineCtrl.SendTotalWinCreditEvent(allWinCredit);
                MainBlackboardController.Instance.AddMyTempCredit(allWinCredit, true);
                MainBlackboardController.Instance.SyncMyTempCreditToReal(true);
            }
            else
                ContentModel.Instance.noWinCount++;

            // 连续五局没中奖播放动画
            if (ContentModel.Instance.noWinCount >= 5)
            {
                PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_not winning");
                yield return new WaitForSeconds(2.667f);
                PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_idle01");
                ContentModel.Instance.noWinCount = 0;
            }

            isNext = false;
            if (winList.Count > 0 || false)
            {
                yield return ShowWinListCoinCountDown(winList, allWinCredit, false);
            }

            // Free Spin
            if (ContentModel.Instance.isFreeSpinTrigger)
            {
                // Timers.inst.Add(2.833f, 1, (obj) => );
                _isWinFreeOrBonus = true;
                if (_corShowFreeSymbol != null) _monoHelper.StopCoroutine(_corShowFreeSymbol);
                _corShowFreeSymbol = _monoHelper.StartCoroutine(ShowWinSymbol(10));
                PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_trigger fg");
                _npcEffectParent.Find("npc_ng_trigger_fg").gameObject.SetActive(true);
                // 免费触发，关闭展会模式
                if (MainModel.Instance.isExhibitionModeMode)
                {
                    _panelController3997.OnClickExhibition();
                }

                Timers.inst.Add(1.3f, 1, (obj) => {_npcEffectParent.Find("npc_ng_trigger_fg").gameObject.SetActive(false); });
                yield return new WaitForSeconds(2f);
                // PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_idle01");
                

                //停止特效显示
                _slotMachineCtrl.SkipWinLine(false);
                yield return FreeSpinTrigger(null, errorCallback);
            }

            // 彩金游戏
            if (ContentModel.Instance.IsBonusTrigger)
            {
                _isWinFreeOrBonus = true;
                PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_trigger sg");
                _npcEffectParent.Find("npc_ng_trigger_sg").gameObject.SetActive(true);
                Timers.inst.Add(5.3f, 1, (obj) =>
                {
                    PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_idle01");
                    _npcEffectParent.Find("npc_ng_trigger_sg").gameObject.SetActive(false);
                });
                // 显示中奖图标
                if (_corShowBonusSymbol != null) _monoHelper.StopCoroutine(_corShowBonusSymbol);
                _corShowBonusSymbol = _monoHelper.StartCoroutine(ShowWinSymbol(11));

                // 彩金触发，关闭展会模式
                if (MainModel.Instance.isExhibitionModeMode)
                {
                    _panelController3997.OnClickExhibition();
                }

                yield return new WaitForSeconds(4f);

                _isMain = false;
                _slotMachineCtrl.SkipWinLine(false);
                // 切换状态
                PageManager.Instance.OpenPageAsync(PageName.CaiFuZhiJiaPopupJackpotTrigger,
                    new EventData<Dictionary<string, object>>("", new Dictionary<string, object> { }),
                    (res) =>
                    {
                        ContentModel.Instance.IsBonusTrigger = false;
                        ContentModel.Instance.IsJackpotTrigger = false;
                        isNext = true;
                    });

                yield return new WaitUntil(() => isNext == true);
                ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
                _slotMachineCtrl.CloseSlotCover();
            }


            if (_isTriggerFrame && !_isWinFreeOrBonus && !_isCurrentWin)
            {
                PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_not triggered");
                Timers.inst.Add(3.5f, 1, (obj) => PlayAnimationByName(_traderAnimator, "Wealth_ng_npc_idle01"));
            }

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
                    if (credit != SBoxModel.Instance.myCredit)
                    {
                    }

                    isNext = true;
                }
            });
            // 本剧同步玩家金钱
            MainBlackboardController.Instance.SyncMyTempCreditToReal(true);
            DebugUtils.Log("进入空闲模式！！！");
            ContentModel.Instance.gameState = GameState.Idle;
            if (winList.Count > 0 && !ContentModel.Instance.isAuto && !ContentModel.Instance.isFreeSpinTrigger)
            {
                if (_corGameIdle != null) _monoHelper.StopCoroutine(_corGameIdle);
                _corGameIdle = _monoHelper.StartCoroutine(GameIdle(winList));
            }

            _isCurrentWin = false;
            _slotMachineCtrl.isStopImmediately = false;

            successCallback?.Invoke();
        }

        IEnumerator BigWinPopup(WinLevelType winLevelType, long winCredit, Action callback = null)
        {
            bool isNext = false;

            PageManager.Instance.OpenPage(PageName.CaiFuZhiJiaPopupOverWin,
                new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object> { ["baseGameWinCredit"] = winCredit, ["WinType"] = winLevelType, }),
                (res) =>
                {
                    isNext = true;
                    // Debug.LogError("1111");
                });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;
            callback?.Invoke();
        }

        IEnumerator ShowWinListCoinCountDown(List<SymbolWin> winList, long totalWinLineCredit, bool isHitJackpot)
        {
            bool isNext = false;

            if (!isHitJackpot)
                _slotMachineCtrl.ShowSymbolWinDeck(_slotMachineCtrl.GetTotalSymbolWin(winList), true);
            yield return new WaitForSeconds(1.5f);
            isNext = false;

            _slotMachineCtrl.SkipWinLine(false);
            _slotMachineCtrl.CloseSlotCover();
        }

        IEnumerator RequestSlotSpinFromMock02(Action successCallback = null, Action<string> errorCallback = null)
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

            ERPushMachineDataManager02.Instance.RequestCoinPushSpin((res) =>
            {
                resNode = JSONNode.Parse((string)res);
                isNext = true;
                Debug.Log("算法结果：" + (string)res + "     上一局免费倍率：" + ContentModel.Instance.freeGameScoreMultiply);
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
                MainBlackboardController.Instance.MinusMyTempCredit(totalBet, true, false);

            Debug.Log("解析数据");
            MachineDataController3997.Instance.ParseSlotSpin02(totalBet, resNode, sboxJackpotData);
            SetUIJackpotGameReel();
            Debug.Log("获取滚轮成功");

            if (successCallback != null)
                successCallback.Invoke();
        }

        private IEnumerator GameIdle(List<SymbolWin> winList)
        {
            if (winList.Count == 0)
                yield break;
            SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.GameIdle);
            yield return _slotMachineCtrl.ShowWinListAwayDuringIdle(winList);
        }

        /// <summary>
        /// 断电重连恢复免费局：点击一次开始后自动跑完整段免费，并统一结算与切回普通游戏。
        /// </summary>
        IEnumerator GameFreeSpinFromReconnect(Action successCallback, Action<string> errorCallback)
        {
            ContentModel.Instance.isPowerTrigger = true;
            ContentModel.Instance.isFreeSpinTrigger = true;
            _multipleNumber.text = "x" + ContentModel.Instance.freeGameScoreMultiply;

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
                        ["baseGameWinCredit"] = ContentModel.Instance.freeSpinTotalWinCoins
                    }),
                (ed) =>
                {
                    _allWinCredit = 0;
                    _pageController.selectedPage = "NormalGame";
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

        #endregion

        #region 彩金游戏

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

        #endregion

        #region BigWin

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

        #endregion

        //读取当前滚轴显示的图标
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

        private void PlayAnimationByName(Animator animator, string aniName, Action callback = null)
        {
            animator.Rebind();
            animator.Play(aniName);
            animator.Update(0f);
            callback?.Invoke();
        }

        private void OnGameReset()
        {
            if (_corGameIdle != null) _monoHelper.StopCoroutine(_corGameIdle);
            if (_corEffectSlowMotion != null) _monoHelper.StopCoroutine(_corEffectSlowMotion);


            _slotMachineCtrl.CloseSlotCover();
            _isStoppedSlotMachine = false;
            _anchorFreeExpectation.visible = false;
            _anchorBonusExpectation.visible = false;
            _slotMachineCtrl.SkipWinLine(true);
        }
    }
}