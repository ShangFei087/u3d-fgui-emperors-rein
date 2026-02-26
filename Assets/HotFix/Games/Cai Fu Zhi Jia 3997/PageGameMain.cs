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

        [JsonProperty("win_level_multiple")] public Dictionary<string, long> WinLevelMultiple { get; set; } //赢钱倍数

        [JsonProperty("symbol_paytable")]
        public Dictionary<string, PayTableSymbolInfo> SymbolPaytable { get; set; } //符号赔率表

        [JsonProperty("pay_lines")] public List<List<int>> pay_lines { get; set; } //支付钱
    }

    public class PageGameMain : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiJia";
        public new const string resName = "PageGameMain";

        // const string CACHE_TOTAL_JP_MAJOR_CONTRIBUTION = "CACHE_TOTAL_JP_MAJOR_CONTRIBUTION";
        // const string CACHE_TOTAL_JP_GRAND_CONTRIBUTION = "CACHE_TOTAL_JP_GRAND_CONTRIBUTION";

        private const string SpinePrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PageGameMain/SpinePrefabs/";

        private const string EffectPrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PageGameMain/EffectPrefabs/";

        // 界面初始化
        private bool _isInitialized = false;
        private bool _isInitPool = false;
        private int _totalCount = -1;
        private GComponent _gOwnerPanel;
        private TextAsset _gameInfo = null;

        // 游戏控制器
        private GameObject _goGameCtrl;
        private MonoHelper _monoHelper;
        private FguiPoolHelper _fGuiPoolHelper;
        private SlotMachineController3997 _slotMachineCtrl;
        private FguiGObjectPoolHelper _fGuiGObjectPoolHelper;
        private Controller _pageController; // FairyGUI的控制器

        // 免费游戏
        private FreeSpinTimeController _freeSpinTimeController; // 免费游戏次数管理器
        private GComponent _freeFrameCom;
        private GTextField _freeSpinsNumber;
        private GTextField _multipleNumber;

        // 开始游戏
        private bool _tipCoinIn = false, _isStoppedSlotMachine = false;

        private Coroutine _corGameAuto = null,
            _corReelsTurn = null,
            _corGameIdle = null,
            _corShowFreeSymbol = null,
            _corShowBonusSymbol = null,
            _corEffectSlowMotion = null;

        // 加速框制作
        private GComponent _comReelEffect2;
        private GComponent _anchorFreeExpectation;
        private GComponent _anchorBonusExpectation;
        private GameObject _freeBorderObj = null; // 免费加速特效
        private GameObject _bonusBorderObj = null; // 彩金加速特效
        readonly List<int> _specialSymbols = new List<int> { 10, 11 };
        private readonly List<GComponent> _speedUpEffectComs = new List<GComponent>();

        readonly List<Dictionary<string, object>> _stackContext = new List<Dictionary<string, object>>();

        private bool _isMain = true;
        long TotalBet => (long)SBoxModel.Instance.CoinInScale;

        // Spine动画
        private GameObject _reelBgSpineObj = null, _freeTreeSpineObj = null; // 物体模板

        private GameObject _cloneReelBgSpineObj = null, _cloneFreeTreeSpineObj = null; // 克隆的物体

        // private GComponent _reelBgSpineCom = null,_freeTreeSpineCom; // UI组件
        private GComponent _compareReelBgSpineCom = null, _compareFreeTreeSpineCom = null; // 多分支对照的UI组件


        //彩金
        //MiniReelGroup uiJPGrandCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMajorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMinorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMiniCtrl = new MiniReelGroup();

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();
            _pageController = contentPane.GetController("PageController");

            _totalCount = 6;
            LoadAsyncRes();
        }

        public override void InitParam()
        {
            if (!_isInitialized) return;
            InitGameView();
            BindSpinesToUI();

            if (_comReelEffect2 != null)
            {
                _comReelEffect2.Dispose();
            }

            // 加速框
            _speedUpEffectComs.Clear();
            _anchorFreeExpectation = contentPane.GetChild("anchorFreeReelEffect").asCom;
            _anchorBonusExpectation = contentPane.GetChild("anchorBonusReelEffect").asCom;
            _speedUpEffectComs.Add(CreateUIEffect(_freeBorderObj, _anchorFreeExpectation));
            _speedUpEffectComs.Add(CreateUIEffect(_bonusBorderObj, _anchorBonusExpectation));

            //彩金
            //uiJPGrangCtrl.Init("Grand", this.contentPane.GetChild("jpGrand").asCom.GetChild("reels").asList, "N0");
            uiJPMajorCtrl.Init("Major", this.contentPane.GetChild("jpMajor").asCom.GetChild("n1").asList, "N0");
            uiJPMinorCtrl.Init("Minor", this.contentPane.GetChild("jpMinor").asCom.GetChild("n1").asList, "N0");
            uiJPMiniCtrl.Init("Mini", this.contentPane.GetChild("jpMini").asCom.GetChild("n1").asList, "N0");

            // uiJPGrangCtrl.SetReelWidth(30);
            uiJPMajorCtrl.SetReelWidth(30);
            uiJPMinorCtrl.SetReelWidth(30);
            uiJPMiniCtrl.SetReelWidth(30);

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

                    // int grandBet = (int)data["grand"];
                    int majorBet = (int)data["major"];
                    int minorBet = (int)data["minor"];
                    int miniBet = (int)data["mini"];

                    // uiJPGrangCtrl.SetData(grandBet);
                    uiJPMajorCtrl.SetData(minorBet);
                    uiJPMinorCtrl.SetData(majorBet);
                    uiJPMiniCtrl.SetData(miniBet);
                });
            }


            ContentModel.Instance.totalBet = SBoxModel.Instance.betList[ContentModel.Instance.betIndex];
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnPanelInputEvent);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT, OnSlotDetailEvent);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnStopSlot);
            InitFreeSpinUIAndController();

            // InitParam();
        }

        public override void OnClose(EventData eventData = null)
        {
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnPanelInputEvent);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT,
                OnSlotDetailEvent);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnStopSlot);
            base.OnClose(eventData);
            _freeSpinTimeController.Dispose();
        }

        #region 初始化 (预制体资源、配置文件以及第一次显示界面)

        private void LoadAsyncRes()
        {
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
                    _goGameCtrl.name = "Game Main Controller";
                    _goGameCtrl.transform.SetParent(null);

                    _slotMachineCtrl = _goGameCtrl.transform.Find("Slot Machine")
                        .GetComponent<SlotMachineController3997>();
                    _monoHelper = _goGameCtrl.transform.GetComponent<MonoHelper>();
                    _fGuiPoolHelper = _goGameCtrl.transform.Find("Pool").GetComponent<FguiPoolHelper>();
                    _fGuiGObjectPoolHelper =
                        _goGameCtrl.transform.Find("GObject Pool").GetComponent<FguiGObjectPoolHelper>();

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
                SpinePrefabPath + "reelBgSpine.prefab",
                (clone) =>
                {
                    _reelBgSpineObj = clone;
                    ResLoadedCallback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                SpinePrefabPath + "freeTreeSpine.prefab",
                (clone) =>
                {
                    _freeTreeSpineObj = clone;
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
        }

        private void ResLoadedCallback()
        {
            if (--_totalCount == 0)
            {
                _isInitialized = true;
                InitParam();
            }
        }

        private void InitGameView()
        {
            MainModel.Instance.contentMD = ContentModel.Instance;
            ParseGameInfo();

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

            // 加载Panel面板
            _gOwnerPanel = contentPane.GetChild("panel").asCom;
            MainModel.Instance.contentMD = ContentModel.Instance;
            ContentModel.Instance.goAnthorPanel = _gOwnerPanel;
            MainModel.Instance.contentMD.goAnthorPanel = _gOwnerPanel;
            EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
                new EventData<GComponent>(PanelEvent.AnchorPanelChange, _gOwnerPanel));

            // 初始化滚轴界面
            GComponent gSlotMachine = contentPane.GetChild("slotMachine").asCom;
            GComponent gReels = gSlotMachine.GetChild("reels").asCom;
            GComponent gSlotCover = gSlotMachine.asCom.GetChild("slotCover").asCom;
            GComponent gPlayLines = gSlotMachine.asCom.GetChild("playLines").asCom;
            GComponent gFrame = contentPane.GetChild("anchorFrame").asCom;
            _slotMachineCtrl.Init(gSlotCover, gPlayLines, gReels, gFrame, _fGuiPoolHelper, _fGuiGObjectPoolHelper);

            // 总押注初始化
            ContentModel.Instance.betIndex = 0;

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
        }

        private void ParseGameInfo()
        {
            GameConfigRoot config = JsonConvert.DeserializeObject<GameConfigRoot>(_gameInfo.text);
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
                    DebugUtils.LogWarning($"无效的符号键：{symbolKey}，无法解析索引");
            }

            if (ContentModel.Instance.payLines == null)
                ContentModel.Instance.payLines = new List<List<int>>() { };
            foreach (var item in config.pay_lines)
                ContentModel.Instance.payLines.Add(item);
        }

        private void BindSpinesToUI()
        {
            // Reel框的钻石Spine动画
            GComponent currentCom = contentPane.GetChild("reelBgSpine").asCom;
            // _reelBgSpineCom = contentPane.GetChild("reelBgSpine").asCom;
            if (currentCom != _compareReelBgSpineCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareReelBgSpineCom);
                _compareReelBgSpineCom = currentCom;
                _cloneReelBgSpineObj = Object.Instantiate(_reelBgSpineObj);
                GameCommon.FguiUtils.AddWrapper(currentCom, _cloneReelBgSpineObj);
            }

            // 免费游戏的树Spine动画
            currentCom = contentPane.GetChild("freeTreeSpine").asCom;
            // _freeTreeSpineCom = contentPane.GetChild("freeTreeSpine").asCom;
            if (currentCom != _compareFreeTreeSpineCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareFreeTreeSpineCom);
                _compareFreeTreeSpineCom = currentCom;
                _cloneFreeTreeSpineObj = Object.Instantiate(_freeTreeSpineObj);
                GameCommon.FguiUtils.AddWrapper(currentCom, _cloneFreeTreeSpineObj);
            }
        }

        private void InitFreeSpinUIAndController()
        {
            _freeSpinTimeController = new FreeSpinTimeController();
            _freeFrameCom = contentPane.GetChild("FSFrame").asCom;
            _freeSpinsNumber = _freeFrameCom.GetChild("FreeSpinsNumber").asTextField;
            _multipleNumber = _freeFrameCom.GetChild("multipleNumber").asTextField;
            _freeSpinTimeController.InitParam(_freeSpinsNumber);
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
                            ContentModel.Instance.isAuto = true;
                            ContentModel.Instance.btnSpinState = SpinButtonState.Auto;
                            StartGameAuto(successCallback, StopGameWhenError); //自动玩
                        }
                        else
                        {
                            ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                            StartGameTotalSpins(successCallback, StopGameWhenError); //开始玩
                        }
                    }
                    break;

                case SpinButtonState.Spin:
                    {
                        // 已经在游戏时，去停止游戏
                        if (!ContentModel.Instance.isSpin) return; // 已经停止直接退出
                        _slotMachineCtrl.isStopImmediately = true; // 去停止游戏  
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

        void OnSlotDetailEvent(EventData res)
        {
            switch (res.name)
            {
                case SlotMachineEvent.PrepareStoppedReel:
                    {
                        if (ContentModel.Instance.isReelsSlowMotion && !_slotMachineCtrl.isStopImmediately)
                        {
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
                    break;
            }
        }

        public IEnumerator ShowEffectReelsSlowMotion(int colIdx)
        {
            Debug.LogError("_speedUpEffectComs.Count:" + _speedUpEffectComs.Count);
            Debug.LogError("_comReelEffect2是否存在:" + (_comReelEffect2 != null));
            if (ContentModel.Instance.IsBonusTrigger) // 新增是否是彩金游戏判断
            {
                _comReelEffect2 = _speedUpEffectComs[1];
                _comReelEffect2.xy = _slotMachineCtrl.SymbolCenterToNodeLocalPos(colIdx, 1, _anchorBonusExpectation);
            }
            else
            {
                _comReelEffect2 = _speedUpEffectComs[0];
                _comReelEffect2.xy = _slotMachineCtrl.SymbolCenterToNodeLocalPos(colIdx, 1, _anchorFreeExpectation);
            }

            Debug.LogError("_comReelEffect2.xy:" + _comReelEffect2.xy);

            _comReelEffect2.visible = true;
            yield return new WaitUntil(() => _isStoppedSlotMachine == true);
            // 关闭Expectation
            _comReelEffect2.visible = false;
        }

        /// <summary>
        /// 点击Spin按钮旋转失败的报错
        /// </summary>
        /// <param name="msg"></param>
        private void StopGameWhenError(string msg)
        {
            ContentModel.Instance.isSpin = false;
            ContentModel.Instance.isAuto = false;
            ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
            ContentModel.Instance.gameState = GameState.Idle;


            // 有好酷优先用好酷
            if (false && SBoxModel.Instance.isUseIot && _tipCoinIn)
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

        #region Cwy_Custom

        /// <summary>
        /// 创建特效UI组件
        /// </summary>
        /// <param name="effectPrefab">特效预制体</param>
        /// <param name="anchorReelEffectGCom">特效父物体组件</param>
        /// <param name="packageName">包名</param>
        /// <param name="componentName">组件名</param>
        /// <returns>特效UI组件</returns>
        private GComponent CreateUIEffect(GameObject effectPrefab, GComponent anchorReelEffectGCom,
            string packageName = "Common",
            string componentName = "AnchorRootDefault")
        {
            GComponent effectComponent = UIPackage.CreateObject(packageName, componentName).asCom;
            GameCommon.FguiUtils.DeleteWrapper(effectComponent);
            GameCommon.FguiUtils.AddWrapper(effectComponent, Object.Instantiate(effectPrefab));

            effectComponent.visible = false;
            anchorReelEffectGCom.AddChild(effectComponent);
            anchorReelEffectGCom.visible = true;

            return effectComponent;
        }

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

        public static List<List<int>> ParseVertical(string raw,
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
        /// </summary>
        /// <param name="successCallback"></param>
        /// <param name="errorCallback"></param>
        /// <returns></returns>
        IEnumerator FreeSpinTrigger(Action successCallback, Action<string> errorCallback)
        {
            PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupFreeSpinTrigger, null);
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
                    _pageController.selectedPage = "FreeGame";
                    isNext = true;
                });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            _slotMachineCtrl.BeginBonusFreeSpin();
            yield return GameFreeSpin(null, errorCallback);

            PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupFreeSpinResult, null);
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

            _slotMachineCtrl.EndBonusFreeSpin();
            PageManager.Instance.OpenPageAsync(PageName.CaiFuZhiJiaPopupFreeSpinResult,
                new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object>()
                    {
                        ["baseGameWinCredit"] =
                            ContentModel.Instance
                                .freeSpinTotalWinCoins /* *ContentModel.Instance.FreeGameScoreMultiply*/, // 免费游戏得分翻倍
                    }),
                (ed) =>
                {
                    _pageController.selectedPage = "NormalGame";
                    ContentModel.Instance.freeGameScoreMultiply = 2;
                    _multipleNumber.text = "2";
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

            ContentModel.Instance.gameState = (string)context["./gameState"];
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

            Debug.LogError("第" + ContentModel.Instance.FreeSpinPlayTimes + "免费局滚动结束");

            // 线赢的数据
            List<SymbolWin> winList = ContentModel.Instance.winList;
            long allWinCredit = 0;

            #region Win

            if (winList.Count > 0 || ContentModel.Instance.BonusResults != null)
            {
                long totalWinLineCredit = _slotMachineCtrl.GetTotalWinCredit(winList);
                allWinCredit = ContentModel.Instance.freeGameScoreMultiply * totalWinLineCredit; // 免费游戏得奖翻倍

                // 播大奖弹窗
                WinLevelType winLevelType = GetBigWinType();
                if (winLevelType != WinLevelType.None)
                {
                    _slotMachineCtrl.ShowSymbolWinDeck(_slotMachineCtrl.GetTotalSymbolWin(winList), true);
                    // 大奖弹窗
                    // yield return WinPopup(winLevelType, ContentModel.Instance.baseGameWinCoins);
                    _slotMachineCtrl.CloseSlotCover();
                    _slotMachineCtrl.SkipWinLine(false);
                }
                else
                {
                    // 总线赢分（同步？？）
                    bool isAddToCredit = totalWinLineCredit > TotalBet * 4;
                    _slotMachineCtrl.SendPrepareTotalWinCreditEvent(totalWinLineCredit, isAddToCredit);
                }

                // 总线赢分事件  处理当前局的得分情况
                _slotMachineCtrl.SendTotalWinCreditEvent(allWinCredit); //注意：原参数是这个totalWinLineCredit，但是后续因为要乘倍数

                //加钱动画
                ContentModel.Instance.freeOnceCredit = allWinCredit; // 注意：原参数也是这个totalWinLineCredit
            }

            #endregion

            #region 中游戏彩金

            // bool isHitJackpot = ContentModel.Instance.jpGameWinLst.Count > 0; // 先注释掉，暂时都是false
            // bool isHitJackpot = false;

            #endregion

            Debug.LogError("当前免费局得分结算结束");

            MainBlackboardController.Instance.SyncMyTempCreditToReal(true);
            isNext = false;

            if (winList.Count > 0 || false)
            {
                yield return ShowWinListCoinCountDown(winList, allWinCredit, false); // false 替换 isHitJackpot
            }

            Debug.LogError("当前免费局播放中奖线结束");

            // 即中即退
            // yield return CoinOutImmediately(allWinCredit);

            // 免费游戏积分倍数增加
            int currentWildCount = ContentModel.Instance.strDeckRowCol.Count(c => c == '9');
            if (currentWildCount > 0)
            {
                ContentModel.Instance.freeGameScoreMultiply += currentWildCount;
                _multipleNumber.text = ContentModel.Instance.freeGameScoreMultiply.ToString();
            }

            Debug.LogError("当前免费局进入Idle状态");
            ContentModel.Instance.gameState = GameState.Idle;
            // 先结算主游戏，再进入“免费游戏”或“小游戏”，则每局都可以同步玩家真实金钱金额

            if (successCallback != null)
                successCallback.Invoke();
        }

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

            // 重置游戏状态，开始旋转准备
            OnGameReset();
            ContentModel.Instance.gameState = GameState.Spin;
            _slotMachineCtrl.BeginTurn();
            bool isNext = false;
            bool isBreak = false;
            string errMsg = "";

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

            _slotMachineCtrl.BeginSpin();

            if (ContentModel.Instance.isReelsSlowMotion)
            {
            }
            else
            {
                _slotMachineCtrl.ShowSymbolAppearEffectAfterReelStop(ContentModel.Instance.winList.Count == 0);
            }

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
            if (winList.Count > 0) //if (winList.Count > 0 || ContentModel.Instance.bonusResult.Count > 0)
            {
                long totalWinLineCredit = 0;
                totalWinLineCredit = _slotMachineCtrl.GetTotalWinCredit(winList);
                allWinCredit = totalWinLineCredit;

                _slotMachineCtrl.SendTotalWinCreditEvent(allWinCredit);
                MainBlackboardController.Instance.AddMyTempCredit(allWinCredit, true);
            }

            if (winList.Count > 0 || false) // 用 false 替换 isHitJackpot
            {
                yield return ShowWinListCoinCountDown(winList, allWinCredit, false);
            }

            // 彩金游戏
            if (ContentModel.Instance.IsBonusTrigger)
            {
                PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupJackpotTrigger, null);
                // 显示中奖图标
                if (_corShowBonusSymbol != null) _monoHelper.StopCoroutine(_corShowBonusSymbol);
                _corShowBonusSymbol = _monoHelper.StartCoroutine(ShowWinSymbol(11));
                yield return new WaitForSeconds(2f);
                _isMain = false;
                _slotMachineCtrl.SkipWinLine(false);
                // 切换状态
                PageManager.Instance.OpenPageAsync(PageName.CaiFuZhiJiaPopupJackpotTrigger,
                    new EventData<Dictionary<string, object>>("", new Dictionary<string, object> { }),
                    (res) =>
                    {
                        PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupJackpotGame, null);
                        ContentModel.Instance.IsBonusTrigger = false;
                        isNext = true;
                    });

                yield return new WaitUntil(() => isNext == true);

                isNext = false;
                PageManager.Instance.OpenPageAsync(PageName.CaiFuZhiJiaPopupJackpotGame,
                    new EventData<Dictionary<string, object>>("", new Dictionary<string, object> { }),
                    (res) =>
                    {
                        _isMain = true;
                        // 加载Panel面板
                        _gOwnerPanel = contentPane.GetChild("panel").asCom;
                        MainModel.Instance.contentMD = ContentModel.Instance;
                        ContentModel.Instance.goAnthorPanel = _gOwnerPanel;
                        MainModel.Instance.contentMD.goAnthorPanel = _gOwnerPanel;
                        EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
                            new EventData<GComponent>(PanelEvent.AnchorPanelChange, _gOwnerPanel));
                        ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
                        ContentModel.Instance.isSpin = false;
                        ContentModel.Instance.remainPlaySpins = 1;
                        _slotMachineCtrl.CloseSlotCover();
                        PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPopupJackpotResult, null);
                        isNext = true;
                    });

                yield return new WaitUntil(() => isNext == true);

                isNext = false;
                PageManager.Instance.OpenPageAsync(PageName.CaiFuZhiJiaPopupJackpotResult,
                    new EventData<Dictionary<string, object>>("", new Dictionary<string, object> { }),
                    (res) =>
                    {
                        // ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
                        isNext = true;
                    });
                yield return new WaitUntil(() => isNext == true);
            }

            // Free Spin
            if (ContentModel.Instance.isFreeSpinTrigger)
            {
                if (_corShowFreeSymbol != null) _monoHelper.StopCoroutine(_corShowFreeSymbol);
                _corShowFreeSymbol = _monoHelper.StartCoroutine(ShowWinSymbol(10));
                yield return new WaitForSeconds(2f);
                //停止特效显示
                _slotMachineCtrl.SkipWinLine(false);
                yield return FreeSpinTrigger(null, errorCallback);
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
                    DebugUtils.Log("算法卡积分==" + credit);
                    DebugUtils.Log("机器积分==" + SBoxModel.Instance.myCredit);
                    if (credit != SBoxModel.Instance.myCredit)
                    {
                    }

                    isNext = true;
                }
            });
            // yield return new WaitUntil(() => isNext == true);
            // isNext = false;

            DebugUtils.Log("进入空闲模式！！！");
            // 进入空闲模式
            ContentModel.Instance.gameState = GameState.Idle;
            if (winList.Count > 0 && !ContentModel.Instance.isAuto && !ContentModel.Instance.isFreeSpinTrigger)
            {
                if (_corGameIdle != null) _monoHelper.StopCoroutine(_corGameIdle);
                _corGameIdle = _monoHelper.StartCoroutine(GameIdle(winList));
            }

            if (successCallback != null)
                successCallback.Invoke();
        }

        IEnumerator ShowWinListCoinCountDown(List<SymbolWin> winList, long totalWinLineCredit, bool isHitJackpot)
        {
            bool isNext = false;

            if (!isHitJackpot)
                _slotMachineCtrl.ShowSymbolWinDeck(_slotMachineCtrl.GetTotalSymbolWin(winList), true);
            yield return new WaitForSeconds(1.5f);
            isNext = false;

            //停止特效显示
            _slotMachineCtrl.SkipWinLine(false);
            //显示遮罩
            _slotMachineCtrl.CloseSlotCover();
        }

        IEnumerator RequestSlotSpinFromMock02(Action successCallback = null, Action<string> errorCallback = null)
        {
            bool isNext = false; // 请求是否完成
            bool isBreak = false; // 是否报错
            long totalBet = TotalBet; // 存储当前的总投注额
            JSONNode resNode = null; // 请求结果

            // 请求旋转数据结果
            MachineDataG3997Controller.Instance.RequestSlotSpinFromMock(TotalBet, (res) =>
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

            // 获取彩金贡献值
            SBoxJackpotData sboxJackpotData = null; // 用于存储彩金数据的本地变量

            //赠送局不用扣分
            if (ContentModel.Instance.gameState != GameState.FreeSpin)
            {
                MainBlackboardController.Instance.MinusMyTempCredit(totalBet, true, false);
            }

            MachineDataG3997Controller.Instance.ParseSlotSpin02(totalBet, resNode, sboxJackpotData);

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
                Debug.Log("算法结果");
                Debug.Log((string)res);
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

            // resNode["creditAfter"] = myCredit;
            Debug.Log("解析数据");
            // 解析数据
            MachineDataG3997Controller.Instance.ParseSlotSpin02(totalBet, resNode, sboxJackpotData);
            // 数据入库
            // ui 彩金
            SetUIJackpotGameReel();
            Debug.Log("获取滚轮成功");

            if (successCallback != null)
                successCallback.Invoke();
        }

        IEnumerator CoinOutImmediately(long totalWinCredit)
        {
            if (SBoxModel.Instance.isCoinOutImmediately && totalWinCredit > 0)
                yield return null;
        }

        private IEnumerator GameIdle(List<SymbolWin> winList)
        {
            if (winList.Count == 0)
            {
                yield break;
            }

            SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.GameIdle);
            yield return _slotMachineCtrl.ShowWinListAwayDuringIdle(winList);
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


        private void OnGameReset()
        {
            if (_corGameIdle != null) _monoHelper.StopCoroutine(_corGameIdle);
            if (_corEffectSlowMotion != null) _monoHelper.StopCoroutine(_corEffectSlowMotion);

            _slotMachineCtrl.isStopImmediately = false;
            _slotMachineCtrl.CloseSlotCover();
            // isEffectSlowMotion2 = false;
            _isStoppedSlotMachine = false;
            // //goExpectation.SetActive(false);
            // ComReelEffect2.visible = false;
            _slotMachineCtrl.SkipWinLine(true);
        }
    }
}