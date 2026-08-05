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

namespace MeiZhouHeiBao_3993
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
        public new const string pkgName = "MeiZhouHeiBao";
        public new const string resName = "PageGameMain";

        private const string PrefabPath = "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PageGameMain/";
        // --------------------------------------------- 通用变量 -----------------------------------------------
        // 资源加载、UI、eventData
        private int _totalCount = -1;
        private GComponent _gOwnerPanel;
        private EventData _openData;
        private bool _isInitPool;

     
        // 游戏控制器
        private GameObject _goGameCtrl;
        private MonoHelper _monoHelper;
        private Controller _pageController;
        private FguiPoolHelper _fGuiPoolHelper;
        private PanelController3993 _panelController;
        private FguiGObjectPoolHelper _fGuiGObjectPoolHelper;
        private GameSoundController3993 _gameSoundController;
        private SlotMachineController3993 _slotMachineController;
        private GComponent _lastAnchorPanelForDispatch;
        // 彩金
        private readonly MiniReelGroup uiJpMajorCtrl = new MiniReelGroup();
        private readonly MiniReelGroup uiJpMinorCtrl = new MiniReelGroup();
        private readonly MiniReelGroup uiJpMiniCtrl = new MiniReelGroup();

        // 玩家押注
        private long TotalBet => MainModel.Instance.contentMD.totalBet;

        private bool IsAddCreditAnim =>
            !(_slotMachineController.isStopImmediately == true || SBoxModel.Instance.isCoinOutImmediately);

        // 说明书
        private List<GComponent> _lstPayTable;
        private readonly PayTableController3993 _payTableController = new PayTableController3993();
        private bool _isStopButtonLocked, _tipCoinIn, _isStoppedSlotMachine;

        // 游戏中协程
        private Coroutine _corGameIdle, _corGameAuto, _corGameOnce, _corReelsTurn;

        // --------------------------------------------- 免费游戏 -----------------------------------------------
        private GComponent _freeFrameCom;
        private GTextField _freeSpinsNumber;
        private FreeSpinTimeController _freeSpinTimeController;
        private long _allWinCredit = 0;

        /// <summary> 免费游戏触发局数据记录 </summary>
        private readonly Stack<Dictionary<string, object>> _freeSaveStack = new Stack<Dictionary<string, object>>();

        // --------------------------------------------- 彩金游戏 -----------------------------------------------

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            // ---------- 1. 加载common,普通游戏,免费游戏,彩金游戏预制体到内存 ----------
            _totalCount = 1;
            // Common 预制体
            if (UIPackage.GetByName("Common") == null)
            {
                ResourceManager02.Instance.LoadAssetBundleAsync("Assets/GameRes/Games/Common/FGUIs", (bundle) =>
                {
                    _totalCount++;
                    ResLoadedCallback();
                });
            }

            ResourceManager02.Instance.LoadAsset<GameObject>("Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Game Controller/Slot Game Main Controller.prefab", (clone) =>
            {
                _goGameCtrl = Object.Instantiate(clone, null);
                _goGameCtrl.name = "Slot Game Main Controller 3993";
                _goGameCtrl.transform.SetParent(null);
                _slotMachineController =
                    _goGameCtrl.transform.Find("Slot Machine").GetComponent<SlotMachineController3993>();
                _monoHelper = _goGameCtrl.transform.GetComponent<MonoHelper>();
                _fGuiPoolHelper = _goGameCtrl.transform.Find("Pool").GetComponent<FguiPoolHelper>();
                _fGuiGObjectPoolHelper =
                    _goGameCtrl.transform.Find("GObject Pool").GetComponent<FguiGObjectPoolHelper>();
                _panelController = _goGameCtrl.transform.Find("Panel").GetComponent<PanelController3993>();
                ResLoadedCallback();
            });
            // 普通预制体

            // 免费预制体

            // 彩金预制体

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

        private void InitParam(EventData eventData)
        {
            if (eventData != null) _openData = eventData;
            if (!isInit) return;

            // ---------- 1. MainModel、PayTable、本地 JSON ----------
            MainModel.Instance.lineNum = 25;
            MainModel.Instance.gameID = 3993;
            MainModel.Instance.gameName = "MeiZhouHeiBao3993";
            MainModel.Instance.displayName = "MeiZhouHeiBao_3993";
            MainModel.Instance.contentMD = ContentModel.Instance;
            MainModel.Instance.cutomMD = CustomModel.Instance;
            MainModel.Instance.contentMD.betIndex = 0;
            ContentModel.Instance.totalBet = SBoxModel.Instance.betList[ContentModel.Instance.betIndex];

            //_lstPayTable = new List<GComponent>();
            //foreach (string url in CustomModel.Instance.payTable)
            //{
            //    GComponent payTable = UIPackage.CreateObjectFromURL(url).asCom;
            //    //payTable.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().InitParam(payTable);
            //    _lstPayTable.Add(payTable);
            //   // payTable.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount++;
            //}

            //ContentModel.Instance.goPayTableLst = _lstPayTable.ToArray();
            //_payTableController.Init(_lstPayTable);

            //// ---------- 2. FairyGUI 对象池（须先于滚轮 Init） ----------
            //if (_fGuiPoolHelper == null || _isInitPool) return;
            //_isInitPool = true;
            //_fGuiPoolHelper.Add(TagPoolObject.SymbolHit, CustomModel.Instance.symbolHitEffect.Values.ToList(),
            //    "symbol_hit#", 5);
            //_fGuiPoolHelper.PreLoad(TagPoolObject.SymbolHit); // 中奖动画
            //_fGuiPoolHelper.Add(TagPoolObject.SymbolBorder, CustomModel.Instance.borderEffect, "border#", 5);
            //_fGuiPoolHelper.PreLoad(TagPoolObject.SymbolBorder); // 边框
            //_fGuiPoolHelper.Add(TagPoolObject.SymbolAppear, CustomModel.Instance.symbolAppearEffect.Values.ToList(),
            //    "symbol_appear#", 10);
            //_fGuiPoolHelper.PreLoad(TagPoolObject.SymbolAppear); // 落下后图标静止动画 

            // ---------- 3.滚轮控制器 ----------
            GComponent gSlotMachine = contentPane.GetChild("slotMachine").asCom;
            GComponent gReels = gSlotMachine.GetChild("reels").asCom;
            GComponent gSlotCover = gSlotMachine.asCom.GetChild("slotCover").asCom;
            GComponent gPlayLines = gSlotMachine.asCom.GetChild("playLines").asCom;
            GComponent gFrame = contentPane.GetChild("anchorFrame").asCom;
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
            _gameSoundController = new GameSoundController3993();
            _pageController = contentPane.GetController("gameController");

            // ---------- 6.初始化FairyGUI组件 --------
            uiJpMajorCtrl.Init("Major", contentPane.GetChild("jpMajor").asCom.GetChild("reels").asList, "N0");
            uiJpMinorCtrl.Init("Minor", contentPane.GetChild("jpMinor").asCom.GetChild("reels").asList, "N0");
            uiJpMiniCtrl.Init("Mini", contentPane.GetChild("jpMini").asCom.GetChild("reels").asList, "N0");
            uiJpMajorCtrl.SetReelWidth(30);
            uiJpMinorCtrl.SetReelWidth(30);
            uiJpMiniCtrl.SetReelWidth(30);
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

                uiJpMajorCtrl.SetData((int)jsonNode["major"]);
                uiJpMinorCtrl.SetData((int)jsonNode["minor"]);
                uiJpMiniCtrl.SetData((int)jsonNode["mini"]);
            });
            //_freeSpinTimeController = new FreeSpinTimeController();
            //_freeFrameCom = contentPane.GetChild("freeFrame").asCom;
            //_freeSpinsNumber = _freeFrameCom.GetChild("FreeSpinsNumber").asTextField;
            //_freeSpinTimeController.InitParam(_freeSpinsNumber);

            //---------- 7.Clone预制体到UI锚点上 --------

            //---------- 8.特效功能制作 -----------------

            isReady = true;
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnClickSpinButton);

            InitParam(eventData);
           // EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT, new EventData(Game3993AudioEvent.BgmRegularGame));
        }

        public override void OnClose(EventData eventData = null)
        {
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnClickSpinButton);
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_EVENT,
                OnBottomPanelReadyForPreload);

            base.OnClose(eventData);
            _freeSpinTimeController.Dispose();
            _gameSoundController?.Dispose();
            _gameSoundController = null;
            _monoHelper.updateHandle.RemoveAllListeners();
        }

        protected override void OnLanguageChange(I18nLang lang)
        {
            FguiI18nTextAssistant.Instance.DisposeAllTranslate(contentPane);
            contentPane.Dispose(); // 释放当前UI
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            InitParam(null);
        }

        #region 资源加载

        private void ResLoadedCallback()
        {
            if (--_totalCount != 0) return;
            isInit = true;
            InitParam(null);
        }

        /// <summary>3993：底部 Panel 异步就绪后触发 PageManager 的 preLoadedCallback。</summary>
        private void OnBottomPanelReadyForPreload(EventData res)
        {
            if (res == null || res.name != PanelEvent.BottomPanelReady) return;
            int gameId = Convert.ToInt32(res.value);
            if (gameId != 3993) return;

            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_EVENT,
                OnBottomPanelReadyForPreload);
            preLoadedCallback?.Invoke();
        }

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
            // if (SBoxModel.Instance.isUseIot && _tipCoinIn) { ... }
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
            JackpotRes info = ContentModel.Instance.jpGameRes;

            ContentModel.Instance.uiMajorJP.nowCredit = uiJpMajorCtrl.nowData;
            ContentModel.Instance.uiMinorJP.nowCredit = uiJpMinorCtrl.nowData;
            ContentModel.Instance.uiMiniJP.nowCredit = uiJpMiniCtrl.nowData;

            ContentModel.Instance.uiMajorJP.curCredit = info.curJackpotMajor;
            ContentModel.Instance.uiMinorJP.curCredit = info.curJackpotMinior;
            ContentModel.Instance.uiMiniJP.curCredit = info.curJackpotMini;

            uiJpMajorCtrl.SetData(info.curJackpotMajor);
            uiJpMinorCtrl.SetData(info.curJackpotMinior);
            uiJpMiniCtrl.SetData(info.curJackpotMini);
        }

        private void OnGameReset()
        {
            _isStoppedSlotMachine = false;
            _slotMachineController.CloseSlotCover();
            _slotMachineController.SkipWinLine(true);
            if (_corGameIdle != null) _monoHelper.StopCoroutine(_corGameIdle);
            // if (_corEffectSlowMotion != null) _monoHelper.StopCoroutine(_corEffectSlowMotion);
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
                Debug.LogError("进入断电重连");
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
                        DebugUtils.LogError($"[G3993] 设置展会模式结果失败，deck={currentDeck}");
                        DebugUtils.LogException(e);
                    }
                }
            }

            // ----------------- 获取本局滚动结果 ---------------
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
                errorCallback?.Invoke(errMsg);
                yield break;
            }

            // ----------------- 卷轴滚动 ---------------
            _slotMachineController.BeginSpin();
            if (ContentModel.Instance.isReelsSlowMotion)
                _slotMachineController.ShowSymbolAppearEffectAfterReelStop(true);
            else
                _slotMachineController.ShowSymbolAppearEffectAfterReelStop(ContentModel.Instance.winList.Count == 0);
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
            long allWinCredit = 0;
            // ----------------- normal win ---------------
            if (winList.Count > 0 || ContentModel.Instance.bonusResult != null)
            {
                // Todo:中奖特效
                //if (_spinWEMD.Instance.isSingleWin){...}else{...}
                long totalWinLineCredit = 0;
                totalWinLineCredit = _slotMachineController.GetTotalWinCredit(winList);
                allWinCredit = totalWinLineCredit;

                _slotMachineController.SendTotalWinCreditEvent(allWinCredit); // 积分同步和退币处理
                MainBlackboardController.Instance.AddMyTempCredit(allWinCredit, true); // 加钱动画
                MainBlackboardController.Instance.SyncMyTempCreditToReal(true); // 同步玩家真实金币
            }

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
                _slotMachineController.SkipWinLine(true);
                _slotMachineController.ShowSymbolEffect(TagPoolObject.SymbolHit, new List<int>() { 10 }, true, 10,
                    true);
                yield return _slotMachineController.SlotWaitForSeconds(1.333f);
                yield return FreeSpinTrigger(null, errorCallback);
            }

            // ----------------- small win ---------------
            if (ContentModel.Instance.isSmallGameTrigger)
            {
                _slotMachineController.SkipWinLine(true);
                _slotMachineController.ShowSymbolEffect(TagPoolObject.SymbolHit, new List<int>() { 11 }, true, 10,
                    true);
                yield return _slotMachineController.SlotWaitForSeconds(2.533f);
                yield return SmallGameTrigger(null, null);
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

            // 检查是否因为错误而中断
            if (isBreak) yield break;
            if (ContentModel.Instance.gameState != GameState.FreeSpin)
                MainBlackboardController.Instance.MinusMyTempCredit(totalBet, true, false);
            MachineDataController3993.Instance.ParseSlotSpin(totalBet, resNode, null);
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
            MachineDataController3993.Instance.ParseSlotSpin(totalBet, resNode, sBoxJackpotData);
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
            for (int i = 0; i < winMultipleList.Count; i++)
            {
                if (baseGameWinCredit > totalBet * winMultipleList[i].multiple)
                {
                    winLevelType = winMultipleList[i].winLevelType;
                }
            }

            return winLevelType;
        }

        private IEnumerator BigWinPopup(WinLevelType winLevelType, long winCredit, Action callback = null)
        {
            bool isNext = false;
            PageManager.Instance.OpenPageAsync(PageName.MeiZhouHeiBaoPopupBigWin,
                new EventData<Dictionary<string, object>>("", new Dictionary<string, object> { }),
                (res) => { isNext = true; });
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

        /// <summary>断电重连恢复免费局：点击一次开始后自动跑完整段免费，并统一结算与切回普通游戏。</summary>
        private IEnumerator GameFreeSpinFromReconnect(Action successCallback, Action<string> errorCallback)
        {
            yield break;
        }

        private IEnumerator FreeSpinTrigger(Action successCallback, Action<string> errorCallback)
        {
            _slotMachineController.BeginBonusFreeSpin(); // 关闭展会模式
            ContentModel.Instance.isFreeSpinTrigger = false;

            bool isNext = false;
            InputStackContextFreeSpin((context) => { });
            PageManager.Instance.OpenPageAsync(PageName.MeiZhouHeiBaoPopupFreeSpinTrigger,
                new EventData<Dictionary<string, object>>("", new Dictionary<string, object>()
                {
                    // [""]=CustomModel.Instance.
                }), (ed) =>
                {
                    _pageController.selectedPage = "free";
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
            PageManager.Instance.OpenPageAsync(PageName.MeiZhouHeiBaoPopupFreeSpinResult, null, (ed) =>
            {
                _pageController.selectedPage = "normal";
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

            // ----------------- normal win ----------------
            List<SymbolWin> winList = ContentModel.Instance.winList;
            if (winList.Count > 0 || ContentModel.Instance.bonusResult != null)
            {
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
            //EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT,new EventData(Game3993AudioEvent.BgmFreeSpinGame));
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
        }

        #endregion

        #region 彩金游戏

        private IEnumerator SmallGameTrigger(Action successCallback, Action<string> errorCallback)
        {
            _slotMachineController.BeginBonusFreeSpin();
            ContentModel.Instance.isSmallGameTrigger = false;
            ContentModel.Instance.isSmallGameSpin = true;

            bool isNext = false;
            PageManager.Instance.OpenPageAsync(PageName.MeiZhouHeiBaoPopupSmallGameTrigger, null, (ed) =>
            {
                _pageController.selectedPage = "small";
                _panelController.ChangButtonNo(true);
                ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
                //EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT, new EventData(Game3993AudioEvent.BgmBonusGame));
                isNext = true;
            });
            yield return new WaitUntil(() => isNext == true);

            yield return new WaitUntil(() => ContentModel.Instance.isSmallGameFinish == true);
            PageManager.Instance.OpenPageAsync(PageName.MeiZhouHeiBaoPopupSmallGameResult, null, (ed) =>
            {
                _pageController.selectedPage = "normal";
                _slotMachineController.CloseSlotCover();
                _panelController.ChangButtonNo(false);
                ContentModel.Instance.isSmallGameFinish = false;
                ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
                //EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT,new EventData(Game3993AudioEvent.BgmRegularGame));
            });
        }

        private IEnumerator SmallGameSpin(Action successCallback, Action<string> errorCallback)
        {
            yield break;
        }

        private IEnumerator SmallGamePlay(Action successCallback, Action<string> errorCallback)
        {
            yield break;
        }

        private IEnumerator SmallGameResult(Action successCallback, Action<string> errorCallback)
        {
            yield break;
        }

        #endregion
    }
}