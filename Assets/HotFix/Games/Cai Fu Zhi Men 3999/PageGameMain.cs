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

namespace CaiFuZhiMen_3999
{
    public class GameConfigRoot
    {
        [JsonProperty("game_id")] public int gameId; //游戏 ID

        [JsonProperty("game_name")] public string gameName; //名称

        [JsonProperty("display_name")] public string displayName; //显示名称

        [JsonProperty("win_level_multiple")] public Dictionary<string, long> WinLevelMultiple { get; set; } //赢钱倍数

        [JsonProperty("symbol_paytable")]
        public Dictionary<string, PayTableSymbolInfo> SymbolPayTable { get; set; } //符号赔率表

        [JsonProperty("pay_lines")] public List<List<int>> PayLines { get; set; } //支付钱
    }

    public class PageGameMain : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiMen";
        public new const string resName = "PageGameMain";

        #region 资源路径

        private const string GameControllerPath = "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/Game Controller/";
        private const string SpinesPath = "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/PageGameMain/SpinePrefabs/";

        private const string EffectsPath =
            "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/PageGameMain/EffectPrefabs/";

        #endregion

        #region 资源加载

        private int _resCount = -1;
        private TextAsset _gameInfo;

        private MonoHelper _monoHelper;
        private FguiPoolHelper _fGuiPoolHelper;
        private FguiGObjectPoolHelper _gfGuiObjectPoolHelper;
        private SlotMachineController3999 _slotMachineController;

        private GameObject _freeBorderEffectObj, _bonusBorderEffectObj, _redRaySpineObj;
        // private GComponent _compareFreeBorderEffectCom, _compareBonusBorderEffectCom, _compareRedRaySpineCom;

        #endregion

        #region 刷新界面

        private bool _isInitPool;

        private GComponent _gOwnerPanel;

        private List<GComponent> _lstPayTable;
        private FairyGUI.Controller _gameController;
        private GTextField _freeRoundText;
        private readonly PayTableController3999 _payTableController = new PayTableController3999();

        //private readonly MiniReelGroup _uiJPGrandCtrl = new MiniReelGroup();
        private readonly MiniReelGroup _uiJpMajorCtrl = new MiniReelGroup();
        private readonly MiniReelGroup _uiJpMinorCtrl = new MiniReelGroup();
        private readonly MiniReelGroup _uiJpMiniCtrl = new MiniReelGroup();

        #endregion

        #region 玩法逻辑

        private bool _tipCoinIn = false;
        private FreeSpinTimeController3999 _freeSpinTimeController;
        private Coroutine _corGameOnce, _corGameIdle, _corReelsTurn, _corShowFreeSymbol, _corShowBonusSymbol;
        private long TotalBet => SBoxModel.Instance.CoinInScale;

        private readonly List<Dictionary<string, object>> _stackContext = new List<Dictionary<string, object>>();

        #endregion

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            _resCount = 5;
            LoadAsyncPrefabRes();
            _gameController = contentPane.GetController("gameControl");
            _freeRoundText = contentPane.GetChild("FSFrame").asCom.GetChild("freeRoundText")
                .asTextField;
        }

        public override void InitParam()
        {
            if (!isInit) return;

            MainModel.Instance.contentMD = ContentModel.Instance;
            // ShowPayTable();
            ParseGameInfo();
            InitUIPool();
            LoadPanel();
            InitSlotReelView();

            preLoadedCallback?.Invoke();
            if (!isOpen) return;

            BindPrefabsToUI();
            RefreshCredit();
            ShowJackpotData();
            ContentModel.Instance.betIndex = 0; // 总押注初始化
            ContentModel.Instance.totalBet = SBoxModel.Instance.betList[ContentModel.Instance.betIndex];
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);

            InitFreeSpinUIAndController();
            GameSoundHelper3999.Instance.PlayMusicSingle(SoundKey.RegularBG);
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnPanelInputEvent);

            InitParam();
        }

        public override void OnClose(EventData eventData = null)
        {
            _freeSpinTimeController.Dispose();
            GameSoundHelper3999.Instance.StopSound(SoundKey.RegularBG);
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnPanelInputEvent);

            base.OnClose(eventData);
        }

        #region 资源加载

        private void LoadAsyncPrefabRes()
        {
            // 加载公共资源包
            if (UIPackage.GetByName("Common") == null)
            {
                ResourceManager02.Instance.LoadAssetBundleAsync("Assets/GameRes/Games/Common/FGUIs", (bundle) =>
                {
                    _resCount++;
                    ResPreLoadCallBack();
                });
            }

            // 加载控制器
            ResourceManager02.Instance.LoadAsset<GameObject>(
                GameControllerPath + "Slot Game Main Controller.prefab",
                (clone) =>
                {
                    GameObject goGameCtrl = Object.Instantiate(clone, null);
                    goGameCtrl.name = "Game Main Controller";
                    goGameCtrl.transform.SetParent(null);

                    _slotMachineController = goGameCtrl.transform.Find("Slot Machine")
                        .GetComponent<SlotMachineController3999>();
                    _monoHelper = goGameCtrl.transform.GetComponent<MonoHelper>();
                    _fGuiPoolHelper = goGameCtrl.transform.Find("Pool").GetComponent<FguiPoolHelper>();
                    _gfGuiObjectPoolHelper =
                        goGameCtrl.transform.Find("GObject Pool").GetComponent<FguiGObjectPoolHelper>();

                    ResPreLoadCallBack();
                });

            // 加载配置文件
            ResourceManager02.Instance.LoadAsset<TextAsset>(
                ConfigUtils.GetGameInfoURL(3999), (txt) =>
                {
                    _gameInfo = txt;
                    ResPreLoadCallBack();
                });

            // 加载Spine动画
            ResourceManager02.Instance.LoadAsset<GameObject>(
                SpinesPath + "RedRay.prefab",
                (clone) =>
                {
                    _redRaySpineObj = clone;
                    ResPreLoadCallBack();
                });

            // 加载Effect特效
            ResourceManager02.Instance.LoadAsset<GameObject>(
                EffectsPath + "FreeBorderEffect.prefab",
                (clone) =>
                {
                    _freeBorderEffectObj = clone;
                    ResPreLoadCallBack();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                EffectsPath + "BonusBorderEffect.prefab",
                (clone) =>
                {
                    _bonusBorderEffectObj = clone;
                    ResPreLoadCallBack();
                });
        }

        private void ResPreLoadCallBack()
        {
            if (--_resCount != 0)
            {
                return;
            }

            isInit = true;
            InitParam();
        }

        #endregion

        #region 刷新界面

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

        private void ParseGameInfo()
        {
            GameConfigRoot config = JsonConvert.DeserializeObject<GameConfigRoot>(_gameInfo.text);
            if (config?.SymbolPayTable == null)
            {
                DebugUtils.LogError("解析symbol_paytable失败，数据为空");
                return;
            }

            MainModel.Instance.gameID = config.gameId;
            MainModel.Instance.gameName = config.gameName;
            MainModel.Instance.displayName = config.displayName;
            foreach (var item in config.WinLevelMultiple)
            {
                string winKey = item.Key;
                long winValue = item.Value;
                MainModel.Instance.contentMD.winLevelMultiple.Add(new WinMultiple(winKey, winValue));
            }

            foreach (var kvp in config.SymbolPayTable)
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

            // if (ContentModel.Instance.payLines == null)
            //     ContentModel.Instance.payLines = new List<List<int>>() { };
            foreach (var item in config.PayLines)
                ContentModel.Instance.payLines.Add(item);
            // payTableController.OnPropertyChangeTotalBet();
        }

        private void InitUIPool()
        {
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
        }

        private void LoadPanel()
        {
            _gOwnerPanel = contentPane.GetChild("panel").asCom;
            MainModel.Instance.contentMD = ContentModel.Instance;
            ContentModel.Instance.goAnthorPanel = _gOwnerPanel;
            MainModel.Instance.contentMD.goAnthorPanel = _gOwnerPanel;
            EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
                new EventData<GComponent>(PanelEvent.AnchorPanelChange, _gOwnerPanel));
        }

        private void InitSlotReelView()
        {
            GComponent gSlotMachine = contentPane.GetChild("slotMachine").asCom;
            GComponent gReels = gSlotMachine.GetChild("reels").asCom;
            GComponent gSlotCover = gSlotMachine.asCom.GetChild("slotCover").asCom;
            GComponent gPlayLines = gSlotMachine.asCom.GetChild("payLines").asCom; // 替换原来的playLines
            GComponent gFrame = contentPane.GetChild("anchor_Effect").asCom; // 替换anchorFrame
            _slotMachineController.Init(gSlotCover, gPlayLines, gReels, gFrame, _fGuiPoolHelper,
                _gfGuiObjectPoolHelper);
        }

        private void BindPrefabsToUI()
        {
            // 实例化Spine
            // GComponent currentCom = contentPane.GetChild("spine").asCom;
        }

        private void RefreshCredit()
        {
            //同步积分和押注
            MachineDataManager02.Instance.RequestGetPlayerInfo((res) =>
            {
                SBoxAccount sBoxAccount = (SBoxAccount)res;
                int pid = SBoxModel.Instance.pid;
                List<SBoxPlayerAccount> playerAccountList = sBoxAccount.PlayerAccountList;
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

        private void ShowJackpotData()
        {
            //彩金
            //_uiJPGrandCtrl.Init("Grand", this.contentPane.GetChild("jpGrand").asCom.GetChild("reels").asList, "N0");
            _uiJpMajorCtrl.Init("Major",
                this.contentPane.GetChild("jpMajor").asCom.GetChild("reels").asCom.GetChild("n0").asList, "N0");
            _uiJpMinorCtrl.Init("Minor",
                this.contentPane.GetChild("jpMinor").asCom.GetChild("reels").asCom.GetChild("n0").asList, "N0");
            _uiJpMiniCtrl.Init("Mini",
                this.contentPane.GetChild("jpMini").asCom.GetChild("reels").asCom.GetChild("n0").asList, "N0");

            _uiJpMajorCtrl.SetReelWidth(20);
            _uiJpMinorCtrl.SetReelWidth(20);
            _uiJpMiniCtrl.SetReelWidth(20);

            if (ApplicationSettings.Instance.isMock)
            {
                //_uiJPGrandCtrl.SetData(50000);
                _uiJpMajorCtrl.SetData(1000);
                _uiJpMinorCtrl.SetData(30000);
                _uiJpMiniCtrl.SetData(500);
            } //模拟环境，方便当前的运行
            else
            {
                //获取彩金贡献值
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

                    int majorBet = (int)jsonNode["major"];
                    int minorBet = (int)jsonNode["minor"];
                    int miniBet = (int)jsonNode["mini"];

                    _uiJpMajorCtrl.SetData(minorBet);
                    _uiJpMinorCtrl.SetData(majorBet);
                    _uiJpMiniCtrl.SetData(miniBet);
                });
            }
        }

        #endregion

        #region 玩法逻辑

        private void OnPanelInputEvent(EventData res)
        {
            switch (res.name)
            {
                case PanelEvent.SpinButtonClick:
                    OnClickSpinButton(res);
                    break;
                case PanelEvent.TotalSpinsButtonClick:
                    break;
            }
        }

        private void OnClickSpinButton(EventData res)
        {
            if (res.name != PanelEvent.SpinButtonClick) return;

            bool isLongClick = (bool)res.value;
            switch (ContentModel.Instance.btnSpinState)
            {
                case SpinButtonState.Stop:
                    if (ContentModel.Instance.isSpin) return;
                    ContentModel.Instance.isSpin = true;

                    if (isLongClick)
                    {
                        ContentModel.Instance.isAuto = true;
                        ContentModel.Instance.btnSpinState = SpinButtonState.Auto;
                        StartGameAuto(StopGameWhenSuccess, StopGameWhenError);
                    }
                    else
                    {
                        ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                        StartGameTotalSpins(StopGameWhenSuccess, StopGameWhenError);
                    }

                    break;

                case SpinButtonState.Spin:
                    if (!ContentModel.Instance.isSpin) return;
                    _slotMachineController.isStopImmediately = true;
                    SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.StopImmediately);
                    break;

                case SpinButtonState.Auto:
                    ContentModel.Instance.isSpin = true;
                    ContentModel.Instance.isAuto = false;
                    ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                    break;
            }
        }

        private void InitFreeSpinUIAndController()
        {
            _freeSpinTimeController = new FreeSpinTimeController3999();
            _freeSpinTimeController.InitParam(_freeRoundText);
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

        private void OnGameReset()
        {
            if (_corGameIdle != null) _monoHelper.StopCoroutine(_corGameIdle);
            _slotMachineController.isStopImmediately = false;
            _slotMachineController.CloseSlotCover();
            _slotMachineController.SkipWinLine(true);
        }

        private List<List<int>> ParseVertical(string raw,
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

        private void SetUIJackpotGameReel()
        {
            JackpotRes info = ContentModel.Instance.jpGameRes;

            //ContentModel.Instance.uiGrandJP.nowCredit = uiJPGrandCtrl.nowData;
            //ContentModel.Instance.uiMegaJP.nowCredit = uiJPMegaCtrl.nowData;
            ContentModel.Instance.uiMajorJP.nowCredit = _uiJpMajorCtrl.nowData;
            ContentModel.Instance.uiMinorJP.nowCredit = _uiJpMinorCtrl.nowData;
            ContentModel.Instance.uiMiniJP.nowCredit = _uiJpMiniCtrl.nowData;

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

            _uiJpMajorCtrl.SetData(info.curJackpotMinior);
            _uiJpMinorCtrl.SetData(info.curJackpotMajor);
            _uiJpMiniCtrl.SetData(info.curJackpotMini);
        }

        private void StopGameWhenSuccess()
        {
            ContentModel.Instance.isSpin = false;
            ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
            ContentModel.Instance.gameState = GameState.Idle;
        }

        private void StopGameWhenError(string msg)
        {
            ContentModel.Instance.isSpin = false;
            ContentModel.Instance.isAuto = false;
            ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
            ContentModel.Instance.gameState = GameState.Idle;

            // 有好酷优先用好酷
            if (false && SBoxModel.Instance.isUseIot && _tipCoinIn) { }
            else
            {
                string massage = I18nMgr.T(msg);
                TipPopupHandler.Instance.OpenPopupOnce(I18nMgr.T(msg));
            }
        }

        private void StartGameTotalSpins(Action successCallback = null, Action<string> errorCallback = null)
        {
            if (_corGameOnce != null) _monoHelper.StopCoroutine(_corGameOnce);
            _corGameOnce = _monoHelper.StartCoroutine(GameTotalSpins(successCallback, errorCallback));
        }

        private void StartGameAuto(Action successCallback = null, Action<string> errorCallback = null)
        {
            if (_corGameOnce != null) _monoHelper.StopCoroutine(_corGameOnce);
            _corGameOnce = _monoHelper.StartCoroutine(GameAuto(successCallback, errorCallback));
        }

        private IEnumerator GameTotalSpins(Action successCallback = null, Action<string> errorCallback = null)
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

                float time = Time.time;
                while (Time.time - time < 1f)
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

        private IEnumerator GameOnce(Action successCallback, Action<string> errorCallback)
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
                SBoxAccount sBoxAccount = (SBoxAccount)res;
                int pid = SBoxModel.Instance.pid;
                List<SBoxPlayerAccount> playerAccountList = sBoxAccount.PlayerAccountList;
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

            // 重置游戏状态
            OnGameReset();
            ContentModel.Instance.gameState = GameState.Spin;
            _slotMachineController.BeginTurn();

            // 标记当前任务是否完成与报错信息输出
            bool isNext = false;
            bool isBreak = false;
            string errMsg = "";

            if (ApplicationSettings.Instance.isMock) //模拟环境，方便当前的运行
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
            else // 真机测试
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

            // 等待完成之后会重置
            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            // 输出错误日志
            if (isBreak)
            {
                errorCallback?.Invoke(errMsg);
                yield break;
            }

            // 开始滚动
            _slotMachineController.BeginSpin();
            if (ContentModel.Instance.isReelsSlowMotion) // 开启滚轮慢动作的话 滚轮停止之后播放特效
            {
                _slotMachineController.ShowSymbolAppearEffectAfterReelStop(true);
            }
            else // 否则没中奖才播放特效
                _slotMachineController.ShowSymbolAppearEffectAfterReelStop(ContentModel.Instance.winList.Count == 0);

            // 立即停止
            if (_slotMachineController.isStopImmediately)
            {
                if (_corReelsTurn != null) _monoHelper.StopCoroutine(_corReelsTurn);
                _corReelsTurn = _monoHelper.StartCoroutine(_slotMachineController.TurnReelsOnce(
                    ContentModel.Instance.strDeckRowCol,
                    () => { isNext = true; }));

                yield return new WaitUntil(() => isNext == true);
                isNext = false;
            }
            else // 正常滚动停止
            {
                if (_corReelsTurn != null) _monoHelper.StopCoroutine(_corReelsTurn);
                _corReelsTurn = _monoHelper.StartCoroutine(_slotMachineController.TurnReelsNormal(
                    ContentModel.Instance.strDeckRowCol,
                    () => { isNext = true; }));

                yield return new WaitUntil(() => isNext == true || _slotMachineController.isStopImmediately == true);
                isNext = false;

                // 等待移动结束  中途停止，强制让滚轮回到指定位置
                if (_slotMachineController.isStopImmediately && isNext == false)
                {
                    if (_corReelsTurn != null) _monoHelper.StopCoroutine(_corReelsTurn);
                    _corReelsTurn = _monoHelper.StartCoroutine(_slotMachineController.ReelsToStopOrTurnOnce(() =>
                    {
                        isNext = true;
                    }));

                    yield return new WaitUntil(() => isNext == true);
                    isNext = false;
                }
            }

            // 普通奖金计算
            List<SymbolWin> winList = ContentModel.Instance.winList;
            long allWinCredit = 0;
            if (winList.Count > 0)
            {
                // 计算总奖金 并判断中奖类型
                long totalWinLineCredit = 0;
                totalWinLineCredit = _slotMachineController.GetTotalWinCredit(winList) / winList.Count;
                allWinCredit = totalWinLineCredit;
                _slotMachineController.SendTotalWinCreditEvent(allWinCredit); // 发送总奖金事件
                // 本剧同步玩家金钱
                MainBlackboardController.Instance.SyncMyTempCreditToReal(true);
            }

            isNext = false;
            // 显示中奖线
            if (winList.Count > 0 /*|| isHitJackpot*/)
            {
                yield return new WaitForSeconds(1);
                yield return ShowWinListCoinCountDown(winList, allWinCredit);
            }

            // 免费游戏触发
            if (ContentModel.Instance.isFreeSpinTrigger)
            {
                if (_corShowFreeSymbol != null) _monoHelper.StopCoroutine(_corShowFreeSymbol);
                _corShowFreeSymbol = _monoHelper.StartCoroutine(ShowWinSymbol(10));
                yield return new WaitForSeconds(1.6f);
                yield return FreeSpinTrigger(null, errorCallback);
            }

            // 彩金游戏触发
            if (ContentModel.Instance.IsBonusTrigger)
            {
                if (_corShowBonusSymbol != null) _monoHelper.StopCoroutine(_corShowBonusSymbol);
                _corShowBonusSymbol = _monoHelper.StartCoroutine(ShowWinSymbol(11));
                yield return new WaitForSeconds(1.6f);

                PageManager.Instance.OpenPageAsync(PageName.CaiFuZhiMenPopupJackpotGameTrigger,
                    new EventData<Dictionary<string, object>>("", new Dictionary<string, object> { }),
                    (res) =>
                    {
                        ContentModel.Instance.IsBonusTrigger = false;
                        ContentModel.Instance.BonusSymbolCount = 0;
                        _slotMachineController.CloseSlotCover();
                        _slotMachineController.SkipWinLine(false);
                        isNext = true;
                    });

                yield return new WaitUntil(() => isNext == true);
                isNext = false;
            }

            //核对前后端积分
            ERPushMachineDataManager02.Instance.RequestCoinPushSpinEnd(res1 =>
            {
                JSONNode jsonNode = JSONObject.Parse((string)res1);

                int code = (int)jsonNode["code"];
                int credit = (int)jsonNode["credit"];

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

            // 本剧同步玩家金钱
            MainBlackboardController.Instance.SyncMyTempCreditToReal(true);

            // 进入空闲状态
            ContentModel.Instance.gameState = GameState.Idle;
            if (winList.Count > 0 && !ContentModel.Instance.isAuto && !ContentModel.Instance.isFreeSpinTrigger)
            {
                if (_corGameIdle != null) _monoHelper.StopCoroutine(_corGameIdle);
                _corGameIdle = _monoHelper.StartCoroutine(GameIdle(winList));
            }

            successCallback?.Invoke();
        }

        private IEnumerator FreeSpinTrigger(Action successCallback, Action<string> errorCallback)
        {
            bool isNext = false;
            InputStackContextFreeSpin((context) =>
            {
                _freeRoundText.text = ContentModel.Instance.FreeSpinTotalTimes.ToString() + "/" +
                                      ContentModel.Instance.FreeSpinTotalTimes.ToString();
                _gameController.name = "free";
            });

            PageManager.Instance.OpenPageAsync(PageName.CaiFuZhiMenPopupFreeSpinTrigger,
                new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object>() { ["freeSpinCount"] = ContentModel.Instance.FreeSpinTotalTimes, }),
                (ed) =>
                {
                    DebugUtils.Log("回调执行！isNext = true"); // 加日志
                    isNext = true;
                });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            _slotMachineController.BeginBonusFreeSpin();
            yield return GameFreeSpin(null, errorCallback);

            OutputStackContextFreeSpin(
                (context) =>
                {
                    SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.Default);
                    _slotMachineController.SetReelsDeck((string)context["./strDeckRowCol"]);
                    _spinWEMD.Instance.SelectData(_spinWEMD.SPIN_WIN_EFFECT_FREE_SPIN_TRIGGER);

                    SymbolWin sw = (SymbolWin)context["./winFreeSpinTriggerOrAddCopy"];
                    if (sw != null && sw.cells.Count > 0)
                        _slotMachineController.ShowSymbolWinDeck(sw, true);
                    _slotMachineController.CloseSlotCover();
                });

            _slotMachineController.EndBonusFreeSpin();
            PageManager.Instance.OpenPageAsync(PageName.CaiFuZhiMenPopupFreeSpinResult,
                new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object>()
                    {
                        ["baseGameWinCredit"] = ContentModel.Instance.freeSpinTotalWinCoins,
                    }),
                (ed) =>
                {
                    DebugUtils.Log("回调执行！isNext = true"); // 加日志

                    _gameController.name = "normal";
                    isNext = true;
                });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            yield return _slotMachineController.SlotWaitForSeconds(1.5f);
        }

        private IEnumerator GameFreeSpin(Action successCallback, Action<string> errorCallback)
        {
            while (ContentModel.Instance.nextReelStripsIndex == "FS")
            {
                yield return GameFreeSpinOnce(null, errorCallback);
                yield return _slotMachineController.SlotWaitForSeconds(1);
            }

            successCallback?.Invoke();
        }

        private IEnumerator GameFreeSpinOnce(Action successCallback, Action<string> errorCallback)
        {
            OnGameReset();
            ContentModel.Instance.gameState = GameState.FreeSpin;

            bool isNext = false;
            bool isBreak = false;
            string errMsg = "";

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

            _slotMachineController.BeginSpin();

            if (_slotMachineController.isStopImmediately)
            {
                if (_corReelsTurn != null) _monoHelper.StopCoroutine(_corReelsTurn);
                _corReelsTurn = _monoHelper.StartCoroutine(_slotMachineController.TurnReelsOnce(
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
                _corReelsTurn = _monoHelper.StartCoroutine(_slotMachineController.TurnReelsNormal(
                    ContentModel.Instance.strDeckRowCol,
                    () =>
                    {
                        isNext = true;
                    }));

                yield return new WaitUntil(() => isNext == true || _slotMachineController.isStopImmediately == true);
                isNext = false;


                // 等待移动结束
                if (_slotMachineController.isStopImmediately && isNext == false)
                {
                    if (_corReelsTurn != null) _monoHelper.StopCoroutine(_corReelsTurn);
                    _corReelsTurn = _monoHelper.StartCoroutine(_slotMachineController.ReelsToStopOrTurnOnce(() =>
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
                long totalWinLineCredit = _slotMachineController.GetTotalWinCredit(winList);
                allWinCredit = totalWinLineCredit;

                bool isAddToCredit = totalWinLineCredit > TotalBet * 4;
                _slotMachineController.SendPrepareTotalWinCreditEvent(totalWinLineCredit, isAddToCredit);

                // 总线赢分事件
                _slotMachineController.SendTotalWinCreditEvent(totalWinLineCredit);

                //加钱动画
                ContentModel.Instance.FreeOnceCredit = totalWinLineCredit;
            }

            #endregion

            // 本剧同步玩家金钱
            MainBlackboardController.Instance.SyncMyTempCreditToReal(true);
            isNext = false;

            if (winList.Count > 0 || false) // isHitJackpot
            {
                yield return new WaitForSeconds(1);
                yield return ShowWinListCoinCountDown(winList, allWinCredit, false);
            }

            #region 免费游戏中，添加额外免费游戏

            if (ContentModel.Instance.isFreeSpinAdd)
            {
                _slotMachineController.BeginBonusFreeSpinAdd();
                _monoHelper.StartCoroutine(ShowWinSymbol(10, () =>
                {
                    isNext = true;
                }));

                yield return new WaitUntil(() => isNext == true);
                isNext = false;

                // 【待修改】重置剩余的局数 
                ContentModel.Instance.ShowFreeSpinRemainTime =
                    ContentModel.Instance.FreeSpinTotalTimes - ContentModel.Instance.FreeSpinPlayTimes;

                yield return _slotMachineController.SlotWaitForSeconds(1.5f);
                _slotMachineController.EndBonusFreeSpinAdd();
                ContentModel.Instance.isFreeSpinAdd = false;
            }

            #endregion

            ContentModel.Instance.gameState = GameState.Idle;
            successCallback?.Invoke();
        }

        private IEnumerator RequestSlotSpinFromMock(Action successCallback = null, Action<string> errorCallback = null)
        {
            bool isNext = false;
            bool isBreak = false;
            long totalBet = TotalBet;
            JSONNode resNode = null;
            //请求结果
            MachineDataController3999.Instance.RequestSlotSpinFromMock(TotalBet, (res) =>
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

            //赠送局不用扣分
            if (ContentModel.Instance.gameState != GameState.FreeSpin)
            {
                MainBlackboardController.Instance.MinusMyTempCredit(totalBet, true, false);
            }

            // 解析数据
            MachineDataController3999.Instance.ParseSlotSpin02(totalBet, resNode, sboxJackpotData);

            successCallback?.Invoke();
        }

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

            // 解析数据
            MachineDataController3999.Instance.ParseSlotSpin02(totalBet, resNode, sboxJackpotData);

            // ui 彩金
            SetUIJackpotGameReel();

            successCallback?.Invoke();
        }

        private IEnumerator ShowWinListCoinCountDown(List<SymbolWin> winList, long totalWinLineCredit,
            bool isHitJackpot = false)
        {
            if (!isHitJackpot)
                _slotMachineController.ShowSymbolWinDeck(_slotMachineController.GetTotalSymbolWin(winList), true);

            yield return new WaitForSeconds(1.5f);

            //停止特效显示
            _slotMachineController.SkipWinLine(false);
            //显示遮罩
            _slotMachineController.CloseSlotCover();
        }

        private IEnumerator GameIdle(List<SymbolWin> winList)
        {
            if (winList.Count == 0)
                yield break;
            SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.GameIdle);
            yield return _slotMachineController.ShowWinListAwayDuringIdle(winList);
        }

        private IEnumerator ShowWinSymbol(int number, Action callback = null)
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

            yield return _slotMachineController.ShowSymbolWinBySetting(curSymbolWin, true,
                PusherEmperorsRein.SpinWinEvent.SingleWinLine);
            callback?.Invoke();
        }

        #endregion
    }
}