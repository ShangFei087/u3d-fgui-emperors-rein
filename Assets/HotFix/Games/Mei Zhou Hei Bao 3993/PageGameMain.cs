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
using PanelBaseController = PusherMaker.PanelBaseController;
using _spinWEMD = SlotMaker.SpinWinEffectSettingModel;

namespace MeiZhouHeiBao_3993
{
    public class GameConfigRoot
    {
        [JsonProperty("game_id")] public int gameId; //游戏 ID
        [JsonProperty("game_name")] public string gameName; //名称
        [JsonProperty("display_name")] public string displayName; //显示名称
        [JsonProperty("line_num")] public int LineNum; //线数
        [JsonProperty("win_level_multiple")] public Dictionary<string, long> WinLevelMultiple { get; set; } //赢钱倍数


        [JsonProperty("symbol_paytable")]
        public Dictionary<string, PayTableSymbolInfo> SymbolPayTable { get; set; } //符号赔率表

        [JsonProperty("pay_lines")] public List<List<int>> PayLines { get; set; } //支付钱
    }

    public class PageGameMain : MachinePageBase
    {
        public new const string pkgName = "MeiZhouHeiBao";
        public new const string resName = "PageGameMain";

        private const string GameControllerPath = "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Game Controller/";

        private const string SpinesPath =
            "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PageGameMain/SpinePrefabs/";

        private const string EffectsPath =
            "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PageGameMain/EffectPrefabs/";

        private int _resCount = -1;
        private TextAsset _gameInfo;

        private GComponent _compareFgBgCom;
        private GameObject _fgBgObj, _cloneFgBgObj;

        private GComponent _compareBorderGlowCom,
            _compareBorderGlowCom1,
            _compareBorderGlowCom2,
            _compareBorderGlowCom3;

        private GameObject _borderGlowObj,
            _cloneBorderGlowObj,
            _cloneBorderGlowObj1,
            _cloneBorderGlowObj2,
            _cloneBorderGlowObj3;

        private MonoHelper _monoHelper;
        private FguiPoolHelper _fGuiPoolHelper;
        private FguiGObjectPoolHelper _gfGuiObjectPoolHelper;
        private SlotMachineController3993 _slotMachineController;
        private GameObject _goGameCtrl;
        private FreeSpinTimeController3993 _freeSpinTimeController;

        private GTextField _freeRoundText, _currentBootNumberText;
        private Controller _gameController;

        private bool _isInitPool;
        private GComponent _gOwnerPanel, _freeParticalEffectParent;
        private bool _tipCoinIn = false;

        // 免费游戏特效功能制作
        private GComponent _rewardEffectCom;
        private GameObject _goRewardEffect;

        private Coroutine _corGameOnce,
            _corGameIdle,
            _corReelsTurn,
            _corShowFreeSymbol,
            _corShowBonusSymbol,
            _corRewardEffect;

        private long TotalBet => MainModel.Instance.contentMD.totalBet;

        private bool IsAddCreditAnim =>
            !(_slotMachineController.isStopImmediately == true || SBoxModel.Instance.isCoinOutImmediately);

        private readonly List<Dictionary<string, object>> _stackContext = new List<Dictionary<string, object>>();

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();
            InitUICom();
            LoadAsyncPrefabRes();
            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        if (PanelBaseController.isOpenIntroduce == true)
                        {
                            return;
                        }

                        Debug.LogError("游戏接受到机台短按的数据：Spin");
                        EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, false); // isLongClick
                        OnPanelInputEvent(res);
                    },
                },
                longClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        DebugUtils.LogError("游戏接受到机台长按的数据：Spin");
                        EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, true); // isLongClick
                        OnPanelInputEvent(res);
                    }
                }
            };
        }

        public override void InitParam()
        {
            if (!isInit) return;
            MainModel.Instance.contentMD = ContentModel.Instance;

            ParseGameInfo();
            InitUIPool();
            LoadPanel();
            InitSlotReelView();

            preLoadedCallback?.Invoke();
            if (!isOpen) return;
            // 粒子特效功能制作
            _rewardEffectCom = UIPackage.CreateObject("Common", "AnchorRootDefault").asCom;
            GameCommon.FguiUtils.DeleteWrapper(_rewardEffectCom);
            GameCommon.FguiUtils.AddWrapper(_rewardEffectCom, Object.Instantiate(_goRewardEffect));
            _rewardEffectCom.visible = false;
            _freeParticalEffectParent.AddChild(_rewardEffectCom);
            _freeParticalEffectParent.visible = true;

            BindPrefabsToUI();
            RefreshCredit();
            _gameController = contentPane.GetController("gameController");
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            if (_goGameCtrl != null && !_goGameCtrl.activeSelf)
            {
                _goGameCtrl.SetActive(true);
            }
            base.OnOpen(currentPageName, eventData);
            InitFreeSpinUIAndController();
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnPanelInputEvent);
            InitParam();
        }

        public override void OnClose(EventData eventData = null)
        {
            OnGameReset();
            _freeSpinTimeController.Dispose();
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnPanelInputEvent);
            if (_goGameCtrl != null && _goGameCtrl.activeSelf)
            {
                _goGameCtrl.SetActive(false);
            }
            base.OnClose(eventData);
        }

        protected override void OnLanguageChange(I18nLang lang)
        {
            FguiI18nTextAssistant.Instance.DisposeAllTranslate(contentPane);
            contentPane.Dispose(); // 释放当前UI
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            InitUICom();
            InitFreeSpinUIAndController();
            InitParam();
            Debug.LogError("语言切换");
        }

        private void LoadAsyncPrefabRes()
        {
            _resCount = 5;
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
                    _goGameCtrl = Object.Instantiate(clone, null);
                    _goGameCtrl.name = "Slot Game Main Controller 3993";
                    _goGameCtrl.transform.SetParent(null);

                    _monoHelper = _goGameCtrl.GetComponentInChildren<MonoHelper>();
                    _fGuiPoolHelper = _goGameCtrl.GetComponentInChildren<FguiPoolHelper>();
                    _gfGuiObjectPoolHelper = _goGameCtrl.GetComponentInChildren<FguiGObjectPoolHelper>();
                    _slotMachineController = _goGameCtrl.GetComponentInChildren<SlotMachineController3993>();

                    ResPreLoadCallBack();
                });

            // 加载配置文件
            ResourceManager02.Instance.LoadAsset<TextAsset>(
                ConfigUtils.GetGameInfoURL(3993), (txt) =>
                {
                    _gameInfo = txt;
                    ResPreLoadCallBack();
                });

            // 加载Spine动画
            ResourceManager02.Instance.LoadAsset<GameObject>(
                SpinesPath + "fg_Background.prefab",
                (clone) =>
                {
                    _fgBgObj = clone;
                    ResPreLoadCallBack();
                });

            // 加载Effect动画
            ResourceManager02.Instance.LoadAsset<GameObject>(
                EffectsPath + "fg_eff_kuang_glow.prefab",
                (clone) =>
                {
                    _borderGlowObj = clone;
                    ResPreLoadCallBack();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                EffectsPath + "RewardEffect.prefab",
                (clone) =>
                {
                    _goRewardEffect = clone;
                    ResPreLoadCallBack();
                });
        }

        private void ResPreLoadCallBack()
        {
            if (--_resCount != 0)
                return;

            isInit = true;
            InitParam();
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
            MainModel.Instance.lineNum = config.LineNum;
            foreach (var item in config.WinLevelMultiple)
            {
                string winKey = item.Key;
                long winValue = item.Value;
                CustomModel.Instance.winLevelMultiple.Add(new WinMultiple(winKey, winValue));
            }

            foreach (var kvp in config.SymbolPayTable)
            {
                string symbolKey = kvp.Key;
                var jsonData1 = kvp.Value;

                if (int.TryParse(symbolKey.Replace("s", ""), out int index))
                {
                    if (index >= 0)
                    {
                        var targetItem = CustomModel.Instance.payTableSymbolWin[index];
                        targetItem.x3 = jsonData1.x3;
                        targetItem.x4 = jsonData1.x4;
                        targetItem.x5 = jsonData1.x5;
                        targetItem.symbol = index;
                    }
                }
                else
                    DebugUtils.LogWarning($"无效的符号键：{symbolKey}，无法解析索引");
            }

            foreach (var item in config.PayLines)
                CustomModel.Instance.payLines.Add(item);
        }

        private void InitFreeSpinUIAndController()
        {
            _freeSpinTimeController = new FreeSpinTimeController3993();
            _freeSpinTimeController.InitParam(_freeRoundText);
        }

        private void InitUICom()
        {
            _freeRoundText = contentPane.GetChild("freeFrame").asCom.GetChild("n16").asCom
                .GetChild("freeRoundText")
                .asTextField;
            _currentBootNumberText = contentPane.GetChild("freeFrame").asCom.GetChild("n25").asCom
                .GetChild("currentBootNumber").asTextField;
            _freeParticalEffectParent = contentPane.GetChild("freeFrame").asCom.GetChild("n25").asCom
                .GetChild("anchor_EffectParent").asCom;
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
            GComponent gPlayLines = gSlotMachine.asCom.GetChild("playLines").asCom;
            GComponent gFrame = contentPane.GetChild("anchor_Effect").asCom;
            _slotMachineController.Init(gSlotCover, gPlayLines, gReels, gFrame, _fGuiPoolHelper,
                _gfGuiObjectPoolHelper);
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

        private void BindPrefabsToUI()
        {
            // 绑定Spine动画
            GComponent currentCom = contentPane.GetChild("anchor_FgBackground").asCom;
            if (currentCom != _compareFgBgCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareFgBgCom);
                _compareFgBgCom = currentCom;
                _cloneFgBgObj = Object.Instantiate(_fgBgObj);
                GameCommon.FguiUtils.AddWrapper(_compareFgBgCom, _cloneFgBgObj);
            }

            // 绑定Effect特效
            GComponent freeFrameCom = contentPane.GetChild("freeFrame").asCom;
            currentCom = freeFrameCom.GetChild("n17").asCom.GetChild("anchor_fg_eff_kuang_glow").asCom;
            if (currentCom != _compareBorderGlowCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareBorderGlowCom);
                _compareBorderGlowCom = currentCom;
                _cloneBorderGlowObj = Object.Instantiate(_borderGlowObj);
                _cloneBorderGlowObj.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(_compareBorderGlowCom, _cloneBorderGlowObj);
            }

            currentCom = freeFrameCom.GetChild("n18").asCom.GetChild("anchor_fg_eff_kuang_glow").asCom;
            if (currentCom != _compareBorderGlowCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareBorderGlowCom1);
                _compareBorderGlowCom1 = currentCom;
                _cloneBorderGlowObj1 = Object.Instantiate(_borderGlowObj);
                _cloneBorderGlowObj1.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(_compareBorderGlowCom1, _cloneBorderGlowObj1);
            }

            currentCom = freeFrameCom.GetChild("n19").asCom.GetChild("anchor_fg_eff_kuang_glow").asCom;
            if (currentCom != _compareBorderGlowCom2)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareBorderGlowCom2);
                _compareBorderGlowCom2 = currentCom;
                _cloneBorderGlowObj2 = Object.Instantiate(_borderGlowObj);
                _cloneBorderGlowObj2.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(_compareBorderGlowCom2, _cloneBorderGlowObj2);
            }

            currentCom = freeFrameCom.GetChild("n20").asCom.GetChild("anchor_fg_eff_kuang_glow").asCom;
            if (currentCom != _compareBorderGlowCom3)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareBorderGlowCom3);
                _compareBorderGlowCom3 = currentCom;
                _cloneBorderGlowObj3 = Object.Instantiate(_borderGlowObj);
                _cloneBorderGlowObj3.SetActive(false);
                GameCommon.FguiUtils.AddWrapper(_compareBorderGlowCom3, _cloneBorderGlowObj3);
            }
        }

        private void OnPanelInputEvent(EventData res)
        {
            switch (res.name)
            {
                case PanelEvent.SpinButtonClick:
                    OnClickSpinButton(res);
                    break;
                case PanelEvent.TotalSpinsButtonClick:
                    OnClickTotalSpinsButtonClick(res);
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

        private void StartGameAuto(Action successCallback = null, Action<string> errorCallback = null)
        {
            if (_corGameOnce != null) _monoHelper.StopCoroutine(_corGameOnce);
            _corGameOnce = _monoHelper.StartCoroutine(GameAuto(successCallback, errorCallback));
        }


        private void StartGameTotalSpins(Action successCallback = null, Action<string> errorCallback = null)
        {
            if (_corGameOnce != null) _monoHelper.StopCoroutine(_corGameOnce);
            _corGameOnce = _monoHelper.StartCoroutine(GameTotalSpins(successCallback, errorCallback));
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

        private void OnGameReset()
        {
            if (_corGameIdle != null) _monoHelper.StopCoroutine(_corGameIdle);
            // if (_corEffectSlowMotion != null) _monoHelper.StopCoroutine(_corEffectSlowMotion);
            // if (_currentBorderCom != null) _currentBorderCom.visible = false;
            // _isStoppedSlotMachine = false;
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

        //下注时向大厅彩金主机发送当前下注
        void RequestOnlineJackpotBetByCurrentBet()
        {
            //if (!SBoxModel.Instance.isJackpotOnLine
            //    || !NetClineBiz.Instance.isLoginSuccess
            //    || !ClientWS.Instance.IsConnected)
            //    return;

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
                long winCredit = (long)winInfo.win;
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
                // 退还之前扣除的积分
                if (ContentModel.Instance.gameState != GameState.FreeSpin)
                    MainBlackboardController.Instance.AddMyTempCredit(TotalBet, true, false);

                errorCallback?.Invoke(errMsg);
                yield break;
            }

            // 检查是否启用在线彩金,请求彩金数据
            if (SBoxModel.Instance.isJackpotOnLine)
            {
                RequestOnlineJackpotBetByCurrentBet();
            }

            // 开始滚动
            _slotMachineController.BeginSpin();
            if (ContentModel.Instance.isReelsSlowMotion) // 开启滚轮慢动作的话 滚轮停止之后播放特效
                _slotMachineController.ShowSymbolAppearEffectAfterReelStop(true);
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
                _corReelsTurn = _monoHelper.StartCoroutine(_slotMachineController.TurnReelsNormal( /*_specialSymbols,*/
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
                totalWinLineCredit = _slotMachineController.GetTotalWinCredit(winList) *
                                     MainModel.Instance.contentMD.betmultiple; // 新增倍率
                allWinCredit = totalWinLineCredit;
                _slotMachineController.SendTotalWinCreditEvent(allWinCredit); // 发送总奖金事件
                //加钱动画
                MainBlackboardController.Instance.AddMyTempCredit(totalWinLineCredit, true, IsAddCreditAnim);
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

            #region Free

            // 免费游戏触发
            if (ContentModel.Instance.isFreeSpinTrigger)
            {
                if (_corShowFreeSymbol != null) _monoHelper.StopCoroutine(_corShowFreeSymbol);
                _corShowFreeSymbol = _monoHelper.StartCoroutine(ShowWinSymbol(5));
                ContentModel.Instance.currentBootCount = 3;
                _currentBootNumberText.text = "0" + ContentModel.Instance.currentBootCount;
                yield return new WaitForSeconds(1.6f);
                yield return FreeSpinTrigger(null, errorCallback);
            }

            #endregion

            #region Bonus

            // 彩金游戏触发
            if (ContentModel.Instance.IsBonusTrigger)
            {
                if (_corShowBonusSymbol != null) _monoHelper.StopCoroutine(_corShowBonusSymbol);
                _corShowBonusSymbol = _monoHelper.StartCoroutine(ShowWinSymbol(10));
                yield return new WaitForSeconds(1.6f);

                PageManager.Instance.OpenPageAsync(PageName.MeiZhouHeiBaoPopupJackpotTrigger,
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

            #endregion

            #region JpOnline

            while (ContentModel.Instance.jpOnlineWin.Count > 0)
            {
                WinJackpotInfo data = ContentModel.Instance.jpOnlineWin[0];
                ContentModel.Instance.jpOnlineWin.RemoveAt(0);

                long winCredit = data.win;
                allWinCredit += winCredit;

                PageManager.Instance.OpenPageAsync(PageName.MeiZhouHeiBaoPopupJackpotResult,
                    new EventData<Dictionary<string, object>>("",
                        new Dictionary<string, object>
                        {
                            ["winCredit"] = winCredit, ["jackpotType"] = data.jackpotId,
                        }),
                    (res) =>
                    {
                        isNext = true;
                    });

                yield return new WaitUntil(() => isNext == true);
                isNext = false;

                // 总线赢分（同步？？）
                _slotMachineController.SendTotalWinCreditEvent(allWinCredit);

                MainBlackboardController.Instance.AddMyTempCredit(winCredit, true, IsAddCreditAnim);
            }

            #endregion

            //核对前后端积分
            ERPushMachineDataManager02.Instance.RequestCoinPushSpinEnd(res1 =>
            {
                JSONNode jsonNode = JSONNode.Parse((string)res1);

                int code = (int)jsonNode["code"];
                int credit = (int)jsonNode["credit"];

                if (code != 0)
                {
                    DebugUtils.LogError($" CoinPushSpinEnd(20102) : [0]= {code}");
                }
                else
                {
                    if (credit != SBoxModel.Instance.myCredit)
                    {
                        DebugUtils.LogError($" 算法卡 :[0]= {credit}   前端:[0]={SBoxModel.Instance.myCredit}");
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

        private IEnumerator GameIdle(List<SymbolWin> winList, Action callback = null)
        {
            if (winList.Count == 0)
                yield break;
            SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.GameIdle);
            yield return _slotMachineController.ShowWinListAwayDuringIdle(winList, callback);
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
                SpinWinEvent.SingleWinLine);
            callback?.Invoke();
        }

        private IEnumerator RequestSlotSpinFromMock(Action successCallback = null, Action<string> errorCallback = null)
        {
            bool isNext = false;
            bool isBreak = false;
            long totalBet = TotalBet;
            JSONNode resNode = null;
            //请求结果
            MachineDataController3993.Instance.RequestSlotSpinFromMock(TotalBet, (res) =>
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

            // SBoxJackpotData sBoxJackpotData = null;

            //赠送局不用扣分
            if (ContentModel.Instance.gameState != GameState.FreeSpin)
                MainBlackboardController.Instance.MinusMyTempCredit(totalBet, true, false);
            MachineDataController3993.Instance.ParseSlotSpin02(totalBet, resNode, null); // 解析数据  sBoxJackpotData

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

            // 初始化本地彩金数据
            SBoxJackpotData sboxJackpotData = new SBoxJackpotData();
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

            if (ContentModel.Instance.gameState != GameState.FreeSpin) //赠送局不用扣分
                MainBlackboardController.Instance.MinusMyTempCredit(totalBet, true, false);
            MachineDataController3993.Instance.ParseSlotSpin02(totalBet, resNode, sboxJackpotData); // 解析数据

            successCallback?.Invoke();
        }

        private IEnumerator ShowWinListCoinCountDown(List<SymbolWin> winList, long totalWinLineCredit,
            bool isHitJackpot = false)
        {
            if (!isHitJackpot)
                _slotMachineController.ShowSymbolWinDeck(_slotMachineController.GetTotalSymbolWin(winList), true);

            yield return new WaitForSeconds(1.5f);
            _slotMachineController.SkipWinLine(false); //停止特效显示
            _slotMachineController.CloseSlotCover(); //显示遮罩
        }

        private IEnumerator FreeSpinTrigger(Action successCallback, Action<string> errorCallback)
        {
            bool isNext = false;
            InputStackContextFreeSpin((context) =>
            {
                _freeRoundText.text = ContentModel.Instance.FreeSpinTotalTimes.ToString();
                _gameController.selectedPage = "freeGame";
                _slotMachineController.SkipWinLine(false);
                _slotMachineController.CloseSlotCover();
            });

            PageManager.Instance.OpenPageAsync(PageName.MeiZhouHeiBaoPopupFreeSpinTrigger,
                new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object>() { ["freeSpinCount"] = ContentModel.Instance.FreeSpinTotalTimes, }),
                (ed) =>
                {
                    DebugUtils.Log("回调执行！isNext = true"); // 加日志
                    isNext = true;
                });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            PageManager.Instance.OpenPageAsync(PageName.MeiZhouHeiBaoPopupFreeGameLoading,
                null,
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
                    _gameController.selectedPage = "normalGame";
                    if (_corRewardEffect != null) _monoHelper.StopCoroutine(_corRewardEffect);
                    _cloneBorderGlowObj.SetActive(false);
                    _cloneBorderGlowObj1.SetActive(false);
                    _cloneBorderGlowObj2.SetActive(false);
                    _cloneBorderGlowObj3.SetActive(false);
                });

            _slotMachineController.EndBonusFreeSpin();

            PageManager.Instance.OpenPageAsync(PageName.MeiZhouHeiBaoPopupFreeSpinResult,
                new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object>()
                    {
                        ["baseGameWinCredit"] = ContentModel.Instance.freeSpinTotalWinCoins,
                    }),
                (ed) =>
                {
                    DebugUtils.Log("回调执行！isNext = true"); // 加日志
                    MainBlackboardController.Instance.AddMyTempCredit(_allWinCredit, true, IsAddCreditAnim); //加钱动画
                    MainBlackboardController.Instance.SyncMyTempCreditToReal(true);
                    _allWinCredit = 0;

                    isNext = true;
                });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            PageManager.Instance.OpenPageAsync(PageName.MeiZhouHeiBaoPopupFreeGameLoading,
                null,
                (ed) =>
                {
                    DebugUtils.Log("回调执行！isNext = true"); // 加日志
                    isNext = true;
                });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            yield return _slotMachineController.SlotWaitForSeconds(1f);
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

        private IEnumerator GameFreeSpin(Action successCallback, Action<string> errorCallback)
        {
            while (ContentModel.Instance.nextReelStripsIndex == "FS")
            {
                yield return GameFreeSpinOnce(null, errorCallback);
                yield return _slotMachineController.SlotWaitForSeconds(1);
            }

            successCallback?.Invoke();
        }

        long _allWinCredit = 0;

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
                    ContentModel.Instance.strDeckRowCol, () => { isNext = true; }));

                yield return new WaitUntil(() => isNext == true);
                isNext = false;
            }
            else
            {
                if (_corReelsTurn != null) _monoHelper.StopCoroutine(_corReelsTurn);
                _corReelsTurn = _monoHelper.StartCoroutine(_slotMachineController.TurnReelsNormal( /*_specialSymbols,*/
                    ContentModel.Instance.strDeckRowCol, () => { isNext = true; }));

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

            #region Win

            if (winList.Count > 0 || ContentModel.Instance.bonusResults != null)
            {
                long totalWinLineCredit = _slotMachineController.GetTotalWinCredit(winList) *
                                          MainModel.Instance.contentMD.betmultiple; // 新增倍率
                _allWinCredit += totalWinLineCredit;
                Debug.LogError("_allWinCredit：" + _allWinCredit + "              totalWinLineCredit：" +
                               totalWinLineCredit);
                _slotMachineController.SendTotalWinCreditEvent(_allWinCredit); // 总线赢分事件
            }

            #endregion

            isNext = false;

            if (winList.Count > 0 || false) // isHitJackpot
            {
                yield return new WaitForSeconds(1);
                yield return ShowWinListCoinCountDown(winList, _allWinCredit, false);
            }

            // 黑豹特效测试
            // OnRewardEffectEvent(() =>
            // {
            //     isNext = true;
            // });
            if (_corRewardEffect != null) _monoHelper.StopCoroutine(_corRewardEffect);
            _corRewardEffect = _monoHelper.StartCoroutine(ProcessBootList(() => isNext = true));
            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            ContentModel.Instance.gameState = GameState.Idle;
            successCallback?.Invoke();
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

        //显示中奖后飞行粒子特效
        void OnRewardEffectEvent(Action callback)
        {
            foreach (var cell in ContentModel.Instance.currentBootList)
            {
                _monoHelper.StartCoroutine(ShowRewardEffect(cell.column, cell.row, _freeParticalEffectParent));
                ContentModel.Instance.currentBootCount++;
                if (ContentModel.Instance.currentBootCount < 10)
                    _currentBootNumberText.text = "0" + ContentModel.Instance.currentBootCount;
                else
                    _currentBootNumberText.text = ContentModel.Instance.currentBootCount.ToString();

                if (ContentModel.Instance.currentBootCount >= 4)
                {
                    _cloneBorderGlowObj.SetActive(true);
                }

                if (ContentModel.Instance.currentBootCount >= 10)
                {
                    _cloneBorderGlowObj1.SetActive(true);
                }

                if (ContentModel.Instance.currentBootCount >= 18)
                {
                    _cloneBorderGlowObj2.SetActive(true);
                }

                if (ContentModel.Instance.currentBootCount >= 28)
                {
                    _cloneBorderGlowObj3.SetActive(true);
                }
            }

            callback?.Invoke();
        }

        IEnumerator ProcessBootList(Action callback)
        {
            foreach (var cell in ContentModel.Instance.currentBootList)
            {
                // 第一步：播放特效移动协程
                yield return _monoHelper.StartCoroutine(ShowRewardEffect(cell.column, cell.row,
                    _freeParticalEffectParent));
                yield return new WaitForSeconds(0.5f); // 特效后延迟

                // 第二步：更新文本内容
                ContentModel.Instance.currentBootCount++;
                if (ContentModel.Instance.currentBootCount < 10)
                    _currentBootNumberText.text = "0" + ContentModel.Instance.currentBootCount;
                else
                    _currentBootNumberText.text = ContentModel.Instance.currentBootCount.ToString();
                yield return new WaitForSeconds(0.5f); // 文本更新后延迟

                // 第三步：判断物体是否激活
                if (ContentModel.Instance.currentBootCount >= 4)
                    _cloneBorderGlowObj.SetActive(true);
                if (ContentModel.Instance.currentBootCount >= 10)
                    _cloneBorderGlowObj1.SetActive(true);
                if (ContentModel.Instance.currentBootCount >= 18)
                    _cloneBorderGlowObj2.SetActive(true);
                if (ContentModel.Instance.currentBootCount >= 28)
                    _cloneBorderGlowObj3.SetActive(true);
                yield return new WaitForSeconds(0.5f); // 物体激活后延迟
            }

            callback?.Invoke();
        }


        private IEnumerator ShowRewardEffect(int colIdx, int rowIdx, GComponent toNode)
        {
            GComponent rewardEffect = _rewardEffectCom;


            if (rewardEffect != null)
            {
                rewardEffect.parent.RemoveChild(rewardEffect);
                toNode.AddChild(rewardEffect);
                rewardEffect.visible = false;
                rewardEffect.xy = _slotMachineController.SymbolCenterToNodeLocalPos(colIdx, rowIdx, toNode);
                rewardEffect.visible = true;

                yield return MoveToZeroOverTime(rewardEffect,
                    _slotMachineController.SymbolCenterToNodeLocalPos(colIdx, rowIdx, toNode));
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
    }
}