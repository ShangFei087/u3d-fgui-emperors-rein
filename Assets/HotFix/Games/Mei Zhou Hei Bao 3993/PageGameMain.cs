using FairyGUI;
using GameMaker;
using Newtonsoft.Json;
using PusherEmperorsRein;
using SBoxApi;
using SimpleJSON;
using SlotMaker;
using SlotZhuZaiJinBi1700;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using _spinWEMD = SlotMaker.SpinWinEffectSettingModel;
using Object = UnityEngine.Object;

namespace MeiZhouHeiBao_3993
{
    /// <summary>游戏配置 JSON 根节点（赔率表、线数、赢钱倍数等）。</summary>
    public class GameConfigRoot
    {
        /// <summary>游戏 ID。</summary>
        [JsonProperty("game_id")] public int GameId;

        /// <summary>内部游戏名。</summary>
        [JsonProperty("game_name")] public string GameName;

        /// <summary>展示用游戏名。</summary>
        [JsonProperty("display_name")] public string DisplayName;

        /// <summary>赔付线数量。</summary>
        [JsonProperty("line_num")] public int LineNum;

        /// <summary>大奖档位对应的赢钱倍数。</summary>
        [JsonProperty("win_level_multiple")] public Dictionary<string, long> WinLevelMultiple { get; set; }

        /// <summary>符号赔率表。</summary>
        [JsonProperty("symbol_paytable")]
        public Dictionary<string, PayTableSymbolInfo> SymbolPaytable { get; set; }

        /// <summary>支付线坐标配置。</summary>
        [JsonProperty("pay_lines")] public List<List<int>> pay_lines { get; set; }
    }

    /// <summary>3993 主游戏页：普通局、免费局、大奖小游戏流程与 UI 绑定。</summary>
    public class PageGameMain : MachinePageBase
    {
        /// <summary>FairyGUI 包名。</summary>
        public new const string pkgName = "MeiZhouHeiBao";
        /// <summary>主界面组件名。</summary>
        public new const string resName = "PageGameMain";
        /// <summary>PAG 资源目录（相对 Streaming/AB）。</summary>
        private const string PagPath = "Games/Mei Zhou Hei Bao 3993/Pag";
        /// <summary>预制体根路径。</summary>
        private const string PrefabPath = "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs";

        // --------------------------------------------- 通用变量 -----------------------------------------------
        /// <summary>OnInit 待加载预制体计数，减到 0 后 InitParam。</summary>
        private int _totalCount = -1;
        /// <summary>底部 Panel 根节点。</summary>
        private GComponent _gOwnerPanel;
        /// <summary>对象池是否已初始化，避免重复 Add/PreLoad。</summary>
        private bool _isInitPool = false;
        /// <summary>底部 Panel 是否已就绪（BottomPanelReady）。</summary>
        private bool _isBottomPanelReady;
        /// <summary>对象池 DoTask 是否已全部完成。</summary>
        private bool _isPoolPreloadDone;
        /// <summary>是否已向 PageManager 派发过 preLoadedCallback。</summary>
        private bool _hasNotifiedPagePreloaded;

        /// <summary>Slot Game Main Controller 根物体。</summary>
        private GameObject _goGameCtrl;
        /// <summary>协程与 Update 托管。</summary>
        private MonoHelper _monoHelper;
        /// <summary>FGUI pageControl：normalGame / freeGame / bonusGame。</summary>
        private Controller _pageController;
        /// <summary>符号 Hit/边框/Appear 对象池。</summary>
        private FguiPoolHelper _fGuiPoolHelper;
        /// <summary>底部按钮与赢分框控制器。</summary>
        private PanelController3993 _panelController;
        /// <summary>GObject 对象池。</summary>
        private FguiGObjectPoolHelper _fGuiGObjectPoolHelper;
        /// <summary>本局音效控制器。</summary>
        private GameSoundController3993 _gameSoundController;
        /// <summary>主盘滚轮控制器。</summary>
        private SlotMachineController3993 _slotMachineController;
        /// <summary>上次派发 AnchorPanelChange 的 Panel，避免重复触发。</summary>
        private GComponent _lastAnchorPanelForDispatch;
        /// <summary>顶部 Major 彩金数字滚轮。</summary>
        private readonly MiniReelGroup uiJpMajorCtrl = new MiniReelGroup();
        /// <summary>顶部 Minor 彩金数字滚轮。</summary>
        private readonly MiniReelGroup uiJpMinorCtrl = new MiniReelGroup();
        /// <summary>顶部 Mini 彩金数字滚轮。</summary>
        private readonly MiniReelGroup uiJpMiniCtrl = new MiniReelGroup();

        /// <summary>当前总押注。</summary>
        private long TotalBet => MainModel.Instance.contentMD.totalBet;

        /// <summary>是否播放加分滚动动画（急停或立即吐币时跳过）。</summary>
        private bool IsAddCreditAnim => !(_slotMachineController.isStopImmediately == true || SBoxModel.Instance.isCoinOutImmediately);

        /// <summary>赔付表页面列表（当前未使用）。</summary>
        private List<GComponent> _lstPayTable;
        /// <summary>说明书控制器。</summary>
        private PayTableController3993 _payTableController = new PayTableController3993();
        /// <summary>停轮按钮是否置灰锁定。</summary>
        private bool _isStopButtonLocked;
        /// <summary>积分不足时是否已提示投币。</summary>
        private bool _tipCoinIn;
        /// <summary>本把滚轮是否已全部停稳。</summary>
        private bool _isStoppedSlotMachine;

        /// <summary>Idle 循环中奖展示协程。</summary>
        private Coroutine _corGameIdle;
        /// <summary>自动旋转协程。</summary>
        private Coroutine _corGameAuto;
        /// <summary>单次旋转协程。</summary>
        private Coroutine _corGameOnce;
        /// <summary>滚轮转动协程（预留）。</summary>
        private Coroutine _corReelsTurn;

        // --------------------------------------------- 普通游戏 -----------------------------------------------
        /// <summary>普通局 NPC 预制体。</summary>
        GameObject goNormalNpc;
        /// <summary>大奖局 NPC 预制体。</summary>
        GameObject goSmallGameNpc;
        /// <summary>NPC 挂点。</summary>
        GComponent anchorNpc;
        /// <summary>当前场景中的普通 NPC 实例。</summary>
        GameObject clonegoNormalNpc;
        /// <summary>当前场景中的大奖 NPC 实例。</summary>
        GameObject clonegoSmallGameNpc;
        /// <summary>当前 NPC AnimPlayer（普通/大奖共用，同时只存在一个）。</summary>
        private AnimPlayer _animNormalNpc;
        /// <summary>连续未中奖局数，满 NpcNoWinIdle2Count 播 Idle2。</summary>
        private int _notHitSpinCount;
        /// <summary>连续未中奖达到该次数后播 NPC Idle2。</summary>
        private const int NpcNoWinIdle2Count = 5;
        /// <summary>黑豹符号 ID。</summary>
        private const int NpcPantherSymbolId = 9;
        /// <summary>Bonus 符号 ID。</summary>
        private const int NpcBonusSymbolId = 12;
        /// <summary>免费加速框预制体。</summary>
        GameObject goFreeSpeedBorder;
        /// <summary>Bonus 加速框预制体。</summary>
        GameObject goBonusSpeedBorder;
        /// <summary>免费加速框挂点。</summary>
        GComponent anchorFreeSpeedBorder;
        /// <summary>Bonus 加速框挂点。</summary>
        GComponent anchorBonusSpeedBorder;
        /// <summary>免费加速框实例。</summary>
        GameObject clonegoFreeSpeedBorder;
        /// <summary>Bonus 加速框实例。</summary>
        GameObject clonegoBonusSpeedBorder;
        /// <summary>即将停轴的目标列（加速框跟随）。</summary>
        private int _speedUpTargetCol = -1;
        /// <summary>加速框跟随协程。</summary>
        private Coroutine _corEffectSlowMotion;
        /// <summary>全屏 PAG：转场、爪子、咆哮共用。</summary>
        private PagSlotBinding pagFade;
        /// <summary>全屏 PAG 挂点。</summary>
        GComponent anchorNormalFadeFree;

        // --------------------------------------------- 免费游戏 -----------------------------------------------
        /// <summary>免费收集盒根节点。</summary>
        private GComponent cptBoxFreeCollet;
        /// <summary>剩余免费次数文本。</summary>
        private GTextField txtRemainFreeTime;
        /// <summary>总免费次数文本。</summary>
        private GTextField txtTotalFreeTime;
        /// <summary>已收集黑豹数量文本。</summary>
        private GTextField txtFreeCollect;
        /// <summary>免费局累计赢分。</summary>
        private long _allWinCredit = 0;
        /// <summary>拖尾父节点。</summary>
        private GComponent _anchorEffectFrame;
        /// <summary>拖尾模板节点（池化复制用）。</summary>
        private GComponent _templateAnchorTrails;
        /// <summary>收集盒星光挂点。</summary>
        private GComponent _anchorEffFgStar;
        /// <summary>免费收集拖尾预制体。</summary>
        private GameObject _goEffFgTuowei;
        /// <summary>收集盒星光预制体。</summary>
        private GameObject _goEffFgStar;
        /// <summary>免费黑豹拖尾飞行时长。</summary>
        private const float PantherTrailDuration = 0.4f;
        /// <summary>多只黑豹拖尾错开间隔。</summary>
        private const float PantherTrailStagger = 0.05f;
        /// <summary>免费转豹 Spine 状态名。</summary>
        private const string FreeConvertAnim = "collect";
        /// <summary>读不到 Animator 时长时的转豹等待。</summary>
        private const float FreeConvertFallbackDuration = 1.5f;
        /// <summary>星光展示时长。</summary>
        private const float PantherStarDuration = 0.4f;
        /// <summary>免费收集进度框预制体。</summary>
        GameObject goFreeCollectBorder;
        /// <summary>4 档收集进度框挂点。</summary>
        GComponent[] _anchorFreeCollectBorders = new GComponent[4];
        /// <summary>4 档收集进度框实例。</summary>
        GameObject[] _clonegoFreeCollectBorders = new GameObject[4];
        /// <summary>免费触发局快照栈，用于嵌套免费/重连。</summary>
        private readonly Stack<Dictionary<string, object>> _freeSaveStack = new Stack<Dictionary<string, object>>();

        // --------------------------------------------- 彩金游戏 -----------------------------------------------
        /// <summary>大奖 15 轴流程管理。</summary>
        private RewardMgr3993 _rewardMgr;
        /// <summary>大奖收集光效预制体。</summary>
        private GameObject _goEffSgGlow;
        /// <summary>大奖收集拖尾预制体。</summary>
        private GameObject _goEffSgTrails;
        /// <summary>普通局豹头收集拖尾预制体。</summary>
        private GameObject _goEffNgTrails;
        /// <summary>豹头 Hit 停留时长。</summary>
        private const float PantherWinHitHold = 0.4f;
        /// <summary>普通局豹头拖尾飞行时长。</summary>
        private const float PantherWinTrailDuration = 0.4f;
        /// <summary>普通局豹头拖尾间隔。</summary>
        private const float PantherWinTrailGap = 0.15f;
        /// <summary>右爪 PAG。</summary>
        private const string PagZhuaziYou = "eff_zhuazi_bmp/eff_zhuazi_you";
        /// <summary>左爪 PAG。</summary>
        private const string PagZhuaziZuo = "eff_zhuazi_bmp/eff_zhuazi_zuo";
        /// <summary>中爪 PAG。</summary>
        private const string PagZhuaziZhong = "eff_zhuazi_bmp/eff_zhuazi_zhong";

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            // ---------- 1. 加载common,普通游戏,免费游戏,彩金游戏预制体到内存 ----------
            _totalCount = 11;
            if (UIPackage.GetByName("Common") == null)
            {
                ResourceManager02.Instance.LoadAssetBundleAsync("Assets/GameRes/Games/Common/FGUIs", (bundle) =>
                {
                    UIPackage.AddPackage(bundle);
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
            ResourceManager02.Instance.LoadAsset<GameObject>("Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PageGameMain/NormalNpc.prefab",
              (GameObject clone) =>
              {
                  goNormalNpc = clone;
                  ResLoadedCallback();
              });
            ResourceManager02.Instance.LoadAsset<GameObject>("Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/Border/ScatterSpeedBorder.prefab",
              (GameObject clone) =>
              {
                  goFreeSpeedBorder = clone;
                  ResLoadedCallback();
              });
            ResourceManager02.Instance.LoadAsset<GameObject>("Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/Border/BonusSpeedBorder.prefab",
              (GameObject clone) =>
              {
                  goBonusSpeedBorder = clone;
                  ResLoadedCallback();
              });

            // 免费预制体

            ResourceManager02.Instance.LoadAsset<GameObject>("Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/Border/FreeCollectBorder.prefab",
              (GameObject clone) =>
              {
                  goFreeCollectBorder = clone;
                  ResLoadedCallback();
              });
            ResourceManager02.Instance.LoadAsset<GameObject>("Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Effect/Eff_fg_tuowei.prefab",
              (GameObject clone) =>
              {
                  _goEffFgTuowei = clone;
                  ResLoadedCallback();
              });
            ResourceManager02.Instance.LoadAsset<GameObject>("Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Effect/Eff_fg_star.prefab",
              (GameObject clone) =>
              {
                  _goEffFgStar = clone;
                  ResLoadedCallback();
              });

            // 彩金预制体
            ResourceManager02.Instance.LoadAsset<GameObject>("Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PageGameMain/SmallGameNpc.prefab",
              (GameObject clone) =>
              {
                  goSmallGameNpc = clone;
                  ResLoadedCallback();
              });
            ResourceManager02.Instance.LoadAsset<GameObject>("Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Effect/Effect_sg_glow.prefab",
              (GameObject clone) =>
              {
                  _goEffSgGlow = clone;
                  ResLoadedCallback();
              });
            ResourceManager02.Instance.LoadAsset<GameObject>("Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Effect/Effect_sg_trails.prefab",
              (GameObject clone) =>
              {
                  _goEffSgTrails = clone;
                  ResLoadedCallback();
              });
            ResourceManager02.Instance.LoadAsset<GameObject>("Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Effect/Effect_ng_trails.prefab",
              (GameObject clone) =>
              {
                  _goEffNgTrails = clone;
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
                        // 机台短按不走屏幕 Touch，需单独播短按特效。
                        _panelController.PlaySpinShortPressEffect();
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
                        // 长按已成立，关掉按住预览，随后进入 Auto。
                        _panelController.StopSpinLongPressEffect();
                        EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, true);
                        OnClickSpinButton(res);
                    }
                },
                downClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        if (PanelBaseController.ShouldBlockPhysicalSpinInput)
                            return;
                        if (!isReady) return;
                        // 按下后 0.4s 预览长按循环特效，与屏幕按钮一致。
                        _panelController.NotifySpinPressBegin();
                    }
                },
                upClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        // 抬起即关闭长按循环特效，避免特效残留。
                        _panelController.NotifySpinPressEnd();
                    }
                }
            };
        }

        public override void InitParam()
        {
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

            List<GComponent> lstPayTable = new List<GComponent>();
            foreach (string url in CustomModel.Instance.payTable)
            {
                GComponent paytable = UIPackage.CreateObjectFromURL(url).asCom;
                lstPayTable.Add(paytable);
            }
            ContentModel.Instance.goPayTableLst = lstPayTable.ToArray();

            // ---------- 2. FairyGUI 对象池（须先于滚轮 Init） ----------
            if (_fGuiPoolHelper != null && !_isInitPool)
            {
                _isInitPool = true;
                _fGuiPoolHelper.Add(TagPoolObject.SymbolHit, CustomModel.Instance.symbolHitEffect.Values.ToList(), "symbol_hit#", 5);
                _fGuiPoolHelper.PreLoad(TagPoolObject.SymbolHit); // 中奖动画
                _fGuiPoolHelper.Add(TagPoolObject.SymbolBorder, CustomModel.Instance.borderEffect, "border#", 5);
                _fGuiPoolHelper.Add(TagPoolObject.SymbolBorder, CustomModel.Instance.pantherNormalBorderEffect, "border#", 5);
                _fGuiPoolHelper.PreLoad(TagPoolObject.SymbolBorder); // 边框
                _fGuiPoolHelper.Add(TagPoolObject.SymbolAppear, CustomModel.Instance.symbolAppearEffect.Values.ToList(), "symbol_appear#", 3);
                _fGuiPoolHelper.Add(TagPoolObject.SymbolAppear, CustomModel.Instance.symbolRewardBonusEffect, "symbol_reward_bonus#", 6);
                _fGuiPoolHelper.Add(TagPoolObject.SymbolAppear, CustomModel.Instance.symbolRewardJpMajorEffect, "symbol_reward_jp#", 1);
                _fGuiPoolHelper.Add(TagPoolObject.SymbolAppear, CustomModel.Instance.symbolRewardJpMinorEffect, "symbol_reward_jp#", 1);
                _fGuiPoolHelper.Add(TagPoolObject.SymbolAppear, CustomModel.Instance.symbolRewardJpMiniEffect, "symbol_reward_jp#", 1);
                _fGuiPoolHelper.Add(TagPoolObject.SymbolAppear, CustomModel.Instance.symbolFreeConvertEffect.Values.ToList(), "symbol_free#", 3);
                _fGuiPoolHelper.PreLoad(TagPoolObject.SymbolAppear); // 落下后图标静止动画 / 免费转豹
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
            MachineDataController3993.Instance.init();
            _slotMachineController.Init(gSlotCover, gPlayLines, gReels, gFrame, _fGuiPoolHelper,_fGuiGObjectPoolHelper);

            GComponent gRewardSlotMachine = contentPane.GetChild("rewardSlotMachine")?.asCom;
            if (gRewardSlotMachine != null)
            {
                _rewardMgr ??= new RewardMgr3993();
                _rewardMgr.Init(gRewardSlotMachine, contentPane, _monoHelper, _fGuiPoolHelper, gFrame);
            }

            _anchorEffectFrame = contentPane.GetChild("anchorEffectFrame") as GComponent;
            _templateAnchorTrails = _anchorEffectFrame?.GetChild("anchorTrails") as GComponent
                                    ?? contentPane.GetChild("anchorTrails") as GComponent;
            if (_templateAnchorTrails != null)
                _templateAnchorTrails.visible = false;
            _rewardMgr?.SetCollectContext(_anchorEffectFrame, _templateAnchorTrails, _goEffSgGlow, _goEffSgTrails,
                _panelController);

            // ---------- 4. 底部菜单 Panel ----------
            _gOwnerPanel = contentPane.GetChild("panel").asCom;
            ContentModel.Instance.goAnthorPanel = _gOwnerPanel;
            MainModel.Instance.contentMD.goAnthorPanel = _gOwnerPanel;
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_EVENT,OnBottomPanelReadyForPreload);
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_EVENT, OnBottomPanelReadyForPreload);
            TryTriggerAnchorPanelChange();
            if (!isOpen) return;

            // ---------- 5.音乐和界面控制 ----------
            _gameSoundController = new GameSoundController3993();
            _pageController = contentPane.GetController("pageControl");

            // ---------- 6.初始化FairyGUI组件 --------
            uiJpMajorCtrl.Init("Major", contentPane.GetChild("jpMajor").asCom.GetChild("reels").asList, "N0");
            uiJpMinorCtrl.Init("Minor", contentPane.GetChild("jpMinor").asCom.GetChild("reels").asList, "N0");
            uiJpMiniCtrl.Init("Mini", contentPane.GetChild("jpMini").asCom.GetChild("reels").asList, "N0");
            uiJpMajorCtrl.SetReelWidth(40);
            uiJpMinorCtrl.SetReelWidth(35);
            uiJpMiniCtrl.SetReelWidth(35);
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

            //---------- 7.Clone预制体到UI锚点上 --------
            GComponent localNpc = contentPane.GetChild("anchorNpc").asCom;
            if (anchorNpc != localNpc)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorNpc);
                clonegoNormalNpc = GameObject.Instantiate(goNormalNpc);
                anchorNpc = localNpc;
                GameCommon.FguiUtils.AddWrapper(anchorNpc, clonegoNormalNpc);
                BindNormalNpc(clonegoNormalNpc);
            }

            GComponent localScatterSpeedBorder = contentPane.GetChild("anchorScatterSpeedBorder").asCom;
            if (anchorFreeSpeedBorder != localScatterSpeedBorder)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorFreeSpeedBorder);
                clonegoFreeSpeedBorder = GameObject.Instantiate(goFreeSpeedBorder);
                anchorFreeSpeedBorder = localScatterSpeedBorder;
                GameCommon.FguiUtils.AddWrapper(localScatterSpeedBorder, clonegoFreeSpeedBorder);
                localScatterSpeedBorder.visible = false;
            }

            GComponent localBonusSpeedBorder = contentPane.GetChild("anchorBonusSpeedBorder").asCom;
            if (anchorBonusSpeedBorder != localBonusSpeedBorder)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorBonusSpeedBorder);
                clonegoBonusSpeedBorder = GameObject.Instantiate(goBonusSpeedBorder);
                anchorBonusSpeedBorder = localBonusSpeedBorder;
                GameCommon.FguiUtils.AddWrapper(localBonusSpeedBorder, clonegoBonusSpeedBorder);
                localBonusSpeedBorder.visible = false;
            }


            GComponent localboxFreeCollect = contentPane.GetChild("boxFreeCollet").asCom;
            for (int i = 0; i < 4; i++)
            {
                GComponent localAnchor = localboxFreeCollect.GetChild("anchorFreeCollectBorder" + (i + 1)).asCom;
                if (_anchorFreeCollectBorders[i] == localAnchor) continue;

                GameCommon.FguiUtils.DeleteWrapper(_anchorFreeCollectBorders[i]);
                _clonegoFreeCollectBorders[i] = GameObject.Instantiate(goFreeCollectBorder);
                _anchorFreeCollectBorders[i] = localAnchor;
                GameCommon.FguiUtils.AddWrapper(localAnchor, _clonegoFreeCollectBorders[i]);
                _anchorFreeCollectBorders[i].visible = false;
            }

            //---------- 8.特效功能制作 -----------------
            //免费游戏
            anchorNormalFadeFree = contentPane.GetChild("anchorFadePag").asCom;
            if (pagFade == null)
                pagFade = new PagSlotBinding("3993PagFade", PagPath);
            pagFade.EnsureSlot(anchorNormalFadeFree);
            _rewardMgr?.SetRoarPag(pagFade);
            cptBoxFreeCollet= localboxFreeCollect;
            cptBoxFreeCollet.GetTransition("exitFree").Play();
            //txtRemainFreeTime = contentPane.GetChild("freeOutFrame").asCom.GetChild("txtRemainFreeTime").asTextField;
            //txtTotalFreeTime = contentPane.GetChild("freeOutFrame").asCom.GetChild("txtTotalFreeTime").asTextField;
            txtFreeCollect = cptBoxFreeCollet.GetChild("txtFreeCollect").asTextField;

            _anchorEffFgStar = cptBoxFreeCollet.GetChild("anchorEff_fg_star") as GComponent;
            if (_anchorEffFgStar != null && _goEffFgStar != null)
            {
                GameCommon.FguiUtils.DeleteWrapper(_anchorEffFgStar);
                GameCommon.FguiUtils.AddWrapper(_anchorEffFgStar, Object.Instantiate(_goEffFgStar));
                _anchorEffFgStar.visible = false;
            }

            _anchorEffectFrame = contentPane.GetChild("anchorEffectFrame") as GComponent;
            _templateAnchorTrails = _anchorEffectFrame?.GetChild("anchorTrails") as GComponent
                                    ?? contentPane.GetChild("anchorTrails") as GComponent;
            if (_templateAnchorTrails != null)
                _templateAnchorTrails.visible = false;

            _rewardMgr?.SetCollectContext(_anchorEffectFrame, _templateAnchorTrails, _goEffSgGlow, _goEffSgTrails,
                _panelController);

            isReady = true;
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            if (_goGameCtrl != null && !_goGameCtrl.activeSelf)
                _goGameCtrl.SetActive(true);
            base.OnOpen(currentPageName, eventData);
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnClickSpinButton);
            EventCenter.Instance.AddEventListener<CoinPushSpinParseEventArgs>(SBoxEventHandle.SBOX_COIN_PUSH_SPIN_PARSE, OnCoinPushSpinResultParse);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnStopSlot);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT, OnSlotDetailEvent);
            InitParam();
           // EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT, new EventData(Game3993AudioEvent.BgmRegularGame));
        }

        public override void OnClose(EventData eventData = null)
        {
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnClickSpinButton);
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_EVENT,OnBottomPanelReadyForPreload);
            EventCenter.Instance.RemoveEventListener<CoinPushSpinParseEventArgs>(SBoxEventHandle.SBOX_COIN_PUSH_SPIN_PARSE, OnCoinPushSpinResultParse);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnStopSlot);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT, OnSlotDetailEvent);
            if (_slotMachineController != null)
                OnGameReset();
            StopAllGameCoroutines();
            _slotMachineController?.ClearBonusScoreBinds();
            _speedUpTargetCol = -1;
            if (anchorFreeSpeedBorder != null) anchorFreeSpeedBorder.visible = false;
            if (anchorBonusSpeedBorder != null) anchorBonusSpeedBorder.visible = false;
            ClearPantherTrails();
            HideCollectStar();
            _rewardMgr?.Dispose();
            pagFade?.Dispose();
            pagFade = null;
            _animNormalNpc?.DetachAll();
            _animNormalNpc = null;
            if (_monoHelper != null)
                _monoHelper.updateHandle.RemoveAllListeners();
            if (_goGameCtrl != null && _goGameCtrl.activeSelf)
                _goGameCtrl.SetActive(false);
            _lastAnchorPanelForDispatch = null;
            base.OnClose(eventData);
        }

        protected override void OnLanguageChange(I18nLang lang)
        {
            pagFade?.StopWithDefaults();
            // PageBase.OnChangeLanguageBase 已 Dispose 并重建 contentPane，这里不要再拆一次。
            // 关着页时不 Init：底部 Panel 当时 inactive，绑不上新 gOwnerPanel。
            if (!isOpen)
                return;
            InitParam();
        }

        /// <summary>停止 Idle / Auto / Once / 加速框等游戏协程。</summary>
        private void StopAllGameCoroutines()
        {
            if (_monoHelper == null) return;
            if (_corGameIdle != null) { _monoHelper.StopCoroutine(_corGameIdle); _corGameIdle = null; }
            if (_corGameAuto != null) { _monoHelper.StopCoroutine(_corGameAuto); _corGameAuto = null; }
            if (_corGameOnce != null) { _monoHelper.StopCoroutine(_corGameOnce); _corGameOnce = null; }
            if (_corReelsTurn != null) { _monoHelper.StopCoroutine(_corReelsTurn); _corReelsTurn = null; }
            if (_corEffectSlowMotion != null) { _monoHelper.StopCoroutine(_corEffectSlowMotion); _corEffectSlowMotion = null; }
        }

        /// <summary>
        /// 解析 Spin 回包
        /// </summary>
        /// <param name="e"> Spin 解析事件参数</param>
        private void OnCoinPushSpinResultParse(CoinPushSpinParseEventArgs e)
        {
            e.Result = MachineDataController3993.ParseCoinPushSpinPayload(e.Data, e.StartPos);
        }

        #region 资源加载

        /// <summary>单个预制体加载完成；全部完成后标记 isInit 并 InitParam。</summary>
        private void ResLoadedCallback()
        {
            if (--_totalCount != 0) return;
            isInit = true;
            InitParam();
        }

        /// <summary>3993：底部 Panel 异步就绪后，与对象池空闲一起触发 PageManager 的 preLoadedCallback。</summary>
        private void OnBottomPanelReadyForPreload(EventData res)
        {
            if (res == null || res.name != PanelEvent.BottomPanelReady) return;
            int gameId = Convert.ToInt32(res.value);
            if (gameId != 3993) return;

            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_EVENT,OnBottomPanelReadyForPreload);
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

        /// <summary>向 PageManager 派发当前底部 Panel 锚点变更（仅变化时）。</summary>
        private void TryTriggerAnchorPanelChange()
        {
            if (_gOwnerPanel == null) return;
            if (ReferenceEquals(_lastAnchorPanelForDispatch, _gOwnerPanel)) return;

            _lastAnchorPanelForDispatch = _gOwnerPanel;
            EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
                new EventData<GComponent>(PanelEvent.AnchorPanelChange, _gOwnerPanel));
        }

        #endregion

        #region 按钮
        /// <summary>处理屏幕/机台 Spin 点击：开转、急停、自动，以及列点动。</summary>
        private void OnClickSpinButton(EventData eventData)
        {
            switch (eventData.name)
            {
                case PanelEvent.SpinButtonClick:
                    {
                        if (IsSpinButtonLocked()) return;

                        bool isLongClick = (bool)eventData.value;
                        ContentModel.Instance.isAuto = TestManager.Instance.IsAutoModeRunning;
                        switch (ContentModel.Instance.btnSpinState)
                        {
                            case SpinButtonState.Stop:
                                {
                                    if (ContentModel.Instance.isSpin) return;
                                    ContentModel.Instance.isSpin = true;
                                    if (ContentModel.Instance.isSmallGameSpin)
                                    {
                                        _rewardMgr.StartRoll();
                                        break;
                                    }

                                    LockStopButton();
                                    if (isLongClick)
                                    {
                                        ContentModel.Instance.isAuto = true;
                                        StartGameAuto(ContinueGameWhenCompleted, StopGameWhenError);
                                    }
                                    else
                                    {
                                        StartGameOnce(ContinueGameWhenCompleted, StopGameWhenError);
                                    }
                                }
                                break;
                            case SpinButtonState.Spin:
                                {
                                    if (!ContentModel.Instance.isSpin) return;

                                    if (ContentModel.Instance.isSmallGameSpin)
                                    {
                                        if (_rewardMgr == null || !_rewardMgr.IsRolling)
                                            break;
                                        _rewardMgr.StartStop(false);
                                        break;
                                    }

                                    LockStopButton();
                                    _slotMachineController.isStopImmediately = true;
                                }
                                break;
                            case SpinButtonState.Auto:
                                {
                                    if (TestManager.Instance.IsAutoModeRunning)
                                    {
                                        _slotMachineController.isStopImmediately = true;
                                        break;
                                    }
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

        /// <summary> 屏幕或机台物理键在置灰期间都应忽略。 </summary>
        private bool IsSpinButtonLocked()
        {
            if (_isStopButtonLocked) return true;
            return MainModel.Instance.panel is PanelBaseController panel && panel.IsSpinStopButtonLocked;
        }

        /// <summary> 点击后 / 停轮后：置灰不可点。 </summary>
        private void LockStopButton()
        {
            _isStopButtonLocked = true;
            if (MainModel.Instance.panel is PanelBaseController panelBaseController)
                panelBaseController.SetSpinButtonLocked(true);
        }

        /// <summary> Spin按钮解锁 </summary>
        private void UnlockStopButton()
        {
            _isStopButtonLocked = false;
            if (MainModel.Instance.panel is PanelBaseController panelBaseController)
                panelBaseController.SetSpinButtonLocked(false);
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
            _panelController?.ChangButtonNo(true);
        }

        /// <summary> 旋转成功，重置状态 </summary>
        private void ContinueGameWhenCompleted()
        {
            DebugUtils.Log("游戏结束");
            ContentModel.Instance.isSpin = false;
            ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
            UnlockStopButton();
            ContentModel.Instance.gameState = GameState.Idle;
        }

        /// <summary> 旋转失败，抛出错误 </summary>
        private void StopGameWhenError(string msg)
        {
            ContentModel.Instance.isSpin = false;
            ContentModel.Instance.isAuto = false;
            ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
            UnlockStopButton();
            ContentModel.Instance.gameState = GameState.Idle;


            // TODO: 未来如果需要启用“有好酷优先用好酷”逻辑，需恢复以下条件判断：
            // if (SBoxModel.Instance.isUseIot && _tipCoinIn) { ... }
            if (string.IsNullOrEmpty(msg)) return;
            string message = I18nMgr.T(msg);
            TipPopupHandler.Instance.OpenPopupOnce(message);
        }
        #endregion

        #region 普通游戏
        /// <summary>启动自动旋转协程。</summary>
        private void StartGameAuto(Action successCallback = null, Action<string> errorCallback = null)
        {
            if (_corGameAuto != null) _monoHelper.StopCoroutine(_corGameAuto);
            _corGameAuto = _monoHelper.StartCoroutine(GameAuto(successCallback, errorCallback));
        }

        /// <summary>启动单次旋转协程。</summary>
        private void StartGameOnce(Action successCallback = null, Action<string> errorCallback = null)
        {
            ContentModel.Instance.totalPlaySpins = 1;
            ContentModel.Instance.remainPlaySpins = 1;
            if (_corGameOnce != null) _monoHelper.StopCoroutine(_corGameOnce);
            _corGameOnce = _monoHelper.StartCoroutine(GameOnce(successCallback, errorCallback));
        }

        /// <summary>把当前可视区符号拼成 row,col 字符串（# 分行）。</summary>
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

        /// <summary>用服务器彩金值刷新顶部 Major/Minor/Mini 数字滚轮。</summary>
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

        /// <summary>重置本把滚轮状态：关盖、停 Idle/加速框。</summary>
        private void OnGameReset()
        {
            _isStoppedSlotMachine = false;
            _slotMachineController.isStopImmediately = false;
            _slotMachineController.CloseSlotCover();
            _slotMachineController.SkipWinLine(true);
            if (_corGameIdle != null) _monoHelper.StopCoroutine(_corGameIdle);
            if (_corEffectSlowMotion != null)
            {
                _monoHelper.StopCoroutine(_corEffectSlowMotion);
                _corEffectSlowMotion = null;
            }
            _speedUpTargetCol = -1;
            if (anchorFreeSpeedBorder != null) anchorFreeSpeedBorder.visible = false;
            if (anchorBonusSpeedBorder != null) anchorBonusSpeedBorder.visible = false;
        }

        /// <summary>自动模式循环调用 GameOnce，直到取消自动或请求停止。</summary>
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

        /// <summary>普通局单次旋转：校验、开转、请求结果、停轮、中奖/免费/大奖分支。</summary>
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

            if (ContentModel.Instance.freeSpinTotalTimes > 0 &&
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
            LockStopButton();
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
            if (TestManager.Instance.IsAutoModeRunning)
            {
                _slotMachineController.isStopImmediately = true;
                TestManager.Instance.RecordAutoModeSpin();
            }

            _slotMachineController.BeginSpin();
            SetSpinButtonRolling();
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
                _corReelsTurn = _monoHelper.StartCoroutine(_slotMachineController.TurnReelsNormal(ContentModel.Instance.strDeckRowCol,() => { isNext = true; }));
                yield return new WaitUntil(() => isNext == true || _slotMachineController.isStopImmediately == true);
                isNext = false;

                // 等待移动结束
                if (_slotMachineController.isStopImmediately && isNext == false)
                {
                    if (_corReelsTurn != null) _monoHelper.StopCoroutine(_corReelsTurn);
                    _corReelsTurn =_monoHelper.StartCoroutine(_slotMachineController.ReelsToStopOrTurnOnce(() => { isNext = true; }));
                    yield return new WaitUntil(() => isNext == true);
                    isNext = false;
                }
            }

            UnlockStopButton();
            SetSpinButtonSpinGray();

            List<SymbolWin> winList = ContentModel.Instance.winList;
            long pantherWin = ContentModel.Instance.isPantherWin ? ContentModel.Instance.pantherBonusWin : 0;
            TryPlayNormalNpcAfterSpin(winList);

            if (ContentModel.Instance.isPantherWin)
            {
                yield return PlayPantherWin();
            } 

            long allWinCredit = pantherWin;
            // ----------------- normal win ---------------
            if (winList.Count > 0 )
            {
                // Todo:中奖特效
                long totalWinLineCredit = _slotMachineController.GetTotalWinCredit(winList);
                allWinCredit += totalWinLineCredit;

                _slotMachineController.SendTotalWinCreditEvent(allWinCredit); // 积分同步和退币处理
                MainBlackboardController.Instance.AddMyTempCredit(totalWinLineCredit, true); // 加钱动画
                yield return _slotMachineController.ShowSymbolWinBySetting(_slotMachineController.GetTotalSymbolWin(winList), true, PusherEmperorsRein.SpinWinEvent.TotalWinLine);
                MainBlackboardController.Instance.SyncMyTempCreditToReal(true); // 同步玩家真实金币
                yield return _slotMachineController.SlotWaitForSeconds(0.5f);
            }
            else if (pantherWin > 0)
            {
                _slotMachineController.SendTotalWinCreditEvent(allWinCredit);
                MainBlackboardController.Instance.SyncMyTempCreditToReal(true);
            }

            // ----------------- big win ---------------
            WinLevelType winLevelType = GetBigWinType();
            if (winLevelType != WinLevelType.None)
            {
                yield return BigWinPopup(winLevelType, ContentModel.Instance.baseGameWinCredit);
                if (winList.Count > 0) _slotMachineController.ShowSymbolWinDeck(_slotMachineController.GetTotalSymbolWin(winList), true);
            }

            // ----------------- free win ---------------
            if (ContentModel.Instance.isFreeSpinTrigger)
            {
                _slotMachineController.SkipWinLine(true);
                _slotMachineController.ShowSymbolEffect(TagPoolObject.SymbolHit, new List<int>() { 11 }, true, 11,true);
                yield return _slotMachineController.SlotWaitForSeconds(1.333f);
                yield return FreeSpinTrigger(null, errorCallback);
            }

            // ----------------- small win ---------------
            if (ContentModel.Instance.isSmallGameTrigger)
            {
                _slotMachineController.SkipWinLine(true);
                _slotMachineController.ShowSymbolEffect(TagPoolObject.SymbolHit, new List<int>() { 12 }, true, 12,true);
                yield return _slotMachineController.SlotWaitForSeconds(2.533f);
                yield return SmallGameTrigger(null, null);
            }
            
            DebugUtils.Log("进入空闲模式！！！");
            // 本剧同步玩家金钱
            _panelController.ChangButtonNo(false);
            MainBlackboardController.Instance.SyncMyTempCreditToReal(true);
            ContentModel.Instance.gameState = GameState.Idle;
            if ( !ContentModel.Instance.isAuto && !ContentModel.Instance.isFreeSpinTrigger)
            {

                if (ContentModel.Instance.isPantherWin)
                {
                    _slotMachineController.ShowPantherWinHit();
                }
                if (winList.Count > 0)
                {
                    if (_corGameIdle != null) _monoHelper.StopCoroutine(_corGameIdle);
                    _corGameIdle = _monoHelper.StartCoroutine(GameIdle(winList));
                }
            }

            _slotMachineController.isStopImmediately = false;
            successCallback?.Invoke();
        }

        /// <summary>停轮后循环播放中奖线（无中奖则直接结束）。</summary>
        private IEnumerator GameIdle(List<SymbolWin> winList)
        {
            if (winList.Count == 0) yield break;
            SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.GameIdle);
            yield return _slotMachineController.ShowWinListAwayDuringIdle(winList);
        }

        /// <summary>滚轮全部停稳回调。</summary>
        private void OnStopSlot(EventData res)
        {
            if (res.name == SlotMachineEvent.StoppedSlotMachine)
                _isStoppedSlotMachine = true;
        }

        /// <summary>即将停轴时启动加速框跟随该列。</summary>
        private void OnSlotDetailEvent(EventData res)
        {
            if (res.name != SlotMachineEvent.PrepareStoppedReel) return;
            if (_slotMachineController.isStopImmediately) return;

            int col = (int)res.value; // 3993 已是下一列
            if (col < 0 || col >= _slotMachineController.column) return;

            _speedUpTargetCol = col;
            if (_corEffectSlowMotion == null)
                _corEffectSlowMotion = _monoHelper.StartCoroutine(ShowEffectReelsSlowMotion());
        }

        /// <summary>Scatter/Bonus 加速框跟随即将停轴的列，全部停稳后隐藏。</summary>
        private IEnumerator ShowEffectReelsSlowMotion()
        {
            yield return new WaitUntil(() => _speedUpTargetCol >= 0 || _isStoppedSlotMachine);
            if (_isStoppedSlotMachine || _speedUpTargetCol < 0)
            {
                _corEffectSlowMotion = null;
                yield break;
            }

            GComponent box = ContentModel.Instance.isFreeSlotTip ? anchorFreeSpeedBorder : anchorBonusSpeedBorder;
            int shownCol = _speedUpTargetCol;

            box.visible = false;
            box.xy = _slotMachineController.SymbolCenterToNodeLocalPos(shownCol, 1, box.parent);
            box.visible = true;
            EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(ContentModel.Instance.isFreeSlotTip
                    ? SlotMachineEvent.FreeRollingBox
                    : SlotMachineEvent.BonusRollingBox));

            while (!_isStoppedSlotMachine)
            {
                int target = _speedUpTargetCol;
                if (target != shownCol && target >= 0)
                {
                    box.TweenMove(_slotMachineController.SymbolCenterToNodeLocalPos(target, 1, box.parent), 0.5f);
                    shownCol = target;
                    EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT,
                        new EventData(ContentModel.Instance.isFreeSlotTip
                            ? SlotMachineEvent.FreeRollingBox
                            : SlotMachineEvent.BonusRollingBox));
                }
                yield return null;
            }

            box.visible = false;
            _speedUpTargetCol = -1;
            _corEffectSlowMotion = null;
        }

        #endregion

        #region 游戏结果申请

        /// <summary>Mock 模式请求一次 Spin 结果。</summary>
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

            if (MachineDataController3993.Instance.ParseSlotSpin(totalBet, resNode, null))
            {
                successCallback?.Invoke();
            }
            else
            {
                errorCallback?.Invoke(null);
            }
               
        }

        /// <summary>Machine模式请求一次 Spin 结果。</summary>
        private IEnumerator RequestSlotSpinFromMachine(Action successCallback = null,Action<string> errorCallback = null)
        {
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
                JSONNode data = JSONNode.Parse((string)res);
                int code = (int)data["code"];

                if (0 != code)
                {
                    DebugUtils.LogError($"请求贡献值报错。 code: {code}");
                    return;
                }

                int majorBet = (int)data["major"];
                int minorBet = (int)data["minor"];
                int miniBet = (int)data["mini"];
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
            if (ContentModel.Instance.gameState != GameState.FreeSpin)MainBlackboardController.Instance.MinusMyTempCredit(totalBet, true, false);

            if (MachineDataController3993.Instance.ParseSlotSpin(totalBet, resNode, sBoxJackpotData))
            {
                SetUIJackpotGameReel();
                successCallback?.Invoke();
                Debug.Log("获取滚轮成功");
            }
            else
            {
                errorCallback.Invoke(null);
            }
        }
        #endregion

        #region BigWin弹窗

        /// <summary>按本局赢分与押注倍数计算 Big/Huge/Massive。</summary>
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

        /// <summary>打开大奖弹窗，关闭后继续后续流程。</summary>
        private IEnumerator BigWinPopup(WinLevelType winLevelType, long winCredit, Action callback = null)
        {
            bool isNext = false;
            _slotMachineController.CloseSlotCover();
            _slotMachineController.SkipWinLine(true);

            PageManager.Instance.OpenPageAsync(PageName.MeiZhouHeiBaoPopupBigWin,
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
            callback?.Invoke();
        }

        #endregion

        #region Panther游戏
        /// <summary>
        /// 普通局豹头收集：等 NPC win3 结束 → 串播爪子 PAG → 亮豹头/Bonus + PantherNormalBorder，再逐个飞 Effect_ng_trails 到 anchorWinBorder 加分。
        /// 结束后只清拖尾和 Panel 赢分框；豹头 Hit/框保留，已收集 Bonus 保持 idle。
        /// </summary>
        private IEnumerator PlayPantherWin()
        {
            _notHitSpinCount = 0;
            yield return _slotMachineController.SlotWaitForSeconds(1.0f);
            yield return PlayPantherZhuaziPag();
            // GameSoundHelper3993.Instance.PlaySoundEff(SoundKey.BonusWin);
            //播放图标特效
            _slotMachineController.ShowPantherWinHit();
            yield return _slotMachineController.SlotWaitForSeconds(1.0f);

            //创建并且移动拖尾
            List<Cell> bonusCells = _slotMachineController.GetVisibleCellsBySymbol(NpcBonusSymbolId);
            Vector2 to = GetWinBorderLocalPos();
            for (int i = 0; i < bonusCells.Count; i++)
            {
                Cell cel = bonusCells[i];
                int score = GetPantherBonusScore(cel.row, cel.column);
                GComponent trail = CreateAnchorTrails();

                _anchorEffectFrame.AddChild(trail);
                trail.SetPivot(0.5f, 0.5f, true);
                trail.xy = _slotMachineController.SymbolCenterToNodeLocalPos(cel.column, cel.row, _anchorEffectFrame);
                if (_goEffNgTrails != null)
                    GameCommon.FguiUtils.AddWrapper(trail, Object.Instantiate(_goEffNgTrails));

                bool arrived = false;
                GComponent captured = trail;
                Cell capturedCell = cel;
                int capturedScore = score;
                trail.TweenMove(to, 0.5f).OnComplete(() =>
                {
                    GameCommon.FguiUtils.DeleteWrapper(captured);
                    captured.Dispose();
                    OnPantherBonusArrived(capturedScore, capturedCell);
                    arrived = true;
                });

                yield return new WaitUntil(() => arrived);
                yield return new WaitForSeconds(PantherWinTrailGap);
            }

            yield return _slotMachineController.SlotWaitForSeconds(PantherWinHitHold);
            ClearPantherTrails();
            _panelController.HideWinBorders();
        }

        /// <summary>爪子 PAG： 中 ，播完再继续收集。</summary>
        private IEnumerator PlayPantherZhuaziPag()
        {
            if (pagFade == null) yield break;

            bool finished = false;
            pagFade.StopWithDefaults();
            bool started = pagFade.Play(new PagSequencePlay(
                new[]
                {
                    new PagSegment(PagZhuaziZhong, 1),
                },
                PagPlayLayout.Center,
                PagPresentationDefaults.DisplayScale,
                useGpuSyncGroup: false,
                callbacks: new PagPlayCallbacks(
                    onFinished: () =>
                    {
                        pagFade?.StopWithDefaults();
                        finished = true;
                    },
                    onFailed: () => finished = true,
                    stopAfterFinished: true)));

            if (!started) yield break;
            yield return new WaitUntil(() => finished);
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

        /// <summary>免费触发：弹窗、切免费 UI、跑完整段免费、结算弹窗后回到普通局。</summary>
        private IEnumerator FreeSpinTrigger(Action successCallback, Action<string> errorCallback)
        {
            _slotMachineController.BeginBonusFreeSpin(); // 关闭展会模式
            ContentModel.Instance.isFreeSpinTrigger = false;

            bool isNext = false;
            InputStackContextFreeSpin((context) => { });
            PageManager.Instance.OpenPageAsync(PageName.MeiZhouHeiBaoPopupFreeSpinTrigger,
                new EventData<Dictionary<string, object>>("", new Dictionary<string, object>()),
                (ed) =>
                {
                    pagFade.StopWithDefaults();
                    pagFade.Play(new PagSequencePlay(
                        new[] { new PagSegment("jp_Transition2_NgToFg/NgToFg", 1) },
                        PagPlayLayout.Center,
                        PagPresentationDefaults.DisplayScale,
                        useGpuSyncGroup: false));
                    isNext = true;
                });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            yield return _slotMachineController.SlotWaitForSeconds(0.5f*Time.timeScale);
            //进入免费奖前准备
            EnterFreeSpit();
            yield return _slotMachineController.SlotWaitForSeconds(3f * Time.timeScale);
            cptBoxFreeCollet.GetTransition("enterFree").Play();
            yield return _slotMachineController.SlotWaitForSeconds(1.0f * Time.timeScale);
            //开始免费
            yield return FreeGameSpin(successCallback, errorCallback);

            OutputStackContextFreeSpin((context) =>
            {
                SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.Default);
                _slotMachineController.SetReelsDeck((string)context["./strDeckRowCol"]);
                _slotMachineController.ApplyAllWildStaticIcons();
                _spinWEMD.Instance.SelectData(_spinWEMD.SPIN_WIN_EFFECT_FREE_SPIN_TRIGGER);
                SymbolWin sw = (SymbolWin)context["./winFreeSpinTriggerOrAddCopy"];
                if (sw != null && sw.cells.Count > 0) _slotMachineController.ShowSymbolWinDeck(sw, true);
            });

            PageManager.Instance.OpenPageAsync(PageName.MeiZhouHeiBaoPopupFreeSpinResult, null,
                (ed) =>
                {
                    pagFade.StopWithDefaults();
                    pagFade.Play(new PagSequencePlay(
                        new[] { new PagSegment("jp_Transition2_NgToFg/NgToFg", 1) },
                        PagPlayLayout.Center,
                        PagPresentationDefaults.DisplayScale,
                        useGpuSyncGroup: false));
               
                isNext = true;
                });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;
            yield return _slotMachineController.SlotWaitForSeconds(0.5f * Time.timeScale);
            //离开免费重置
            ExitFreeSpin();
            yield return _slotMachineController.SlotWaitForSeconds(3.0f * Time.timeScale);
        }

        /// <summary>免费局单次旋转：开转、请求、停轮、收集黑豹、可能变豹与加次数。</summary>
        private IEnumerator FreeSpinOnce(Action successCallback, Action<string> errorCallback)
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

            if (TestManager.Instance.IsAutoModeRunning)
                _slotMachineController.isStopImmediately = true;

            _slotMachineController.BeginSpin();
            SetSpinButtonRolling();
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

            SetSpinButtonSpinGray();

            //转换黑豹
            yield return ConvertEligibleSymbolsToPanther();
            //收集黑豹图标（拖尾飞入后再刷新个数）
            yield return CollectPantherSymbols();

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

                _slotMachineController.ShowSymbolWinDeck(_slotMachineController.GetTotalSymbolWin(winList), true);
            }

            ContentModel.Instance.gameState = GameState.Idle;
            SetSpinButtonSpinGray();
            successCallback?.Invoke();
        }

        /// <summary>循环执行剩余免费次数。</summary>
        private IEnumerator FreeGameSpin(Action successCallback, Action<string> errorCallback)
        {
            //EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT,new EventData(Game3993AudioEvent.BgmFreeSpinGame));
            while (ContentModel.Instance.nextReelStripsIndex == "FS")
            {
                yield return FreeSpinOnce(null, errorCallback);
                txtRemainFreeTime.text = (ContentModel.Instance.freeSpinTotalTimes - ContentModel.Instance.freeSpinPlayTimes).ToString();
                yield return _slotMachineController.SlotWaitForSeconds(1);
            }

            successCallback?.Invoke();
        }

        /// <summary>展示中奖线后收盖、关盖，进入加分倒计时等待。</summary>
        private IEnumerator ShowWinListCoinCountDown(List<SymbolWin> winList, long totalWinLineCredit,bool isHitJackpot)
        {
            if (!isHitJackpot)
                _slotMachineController.ShowSymbolWinDeck(_slotMachineController.GetTotalSymbolWin(winList), true);
            yield return new WaitForSeconds(1.5f);
            _slotMachineController.SkipWinLine(false);
            _slotMachineController.CloseSlotCover();
        }

        /// <summary>退出免费：切回普通页、清收集、恢复 NPC Idle1。</summary>
        private void ExitFreeSpin()
        {
            _pageController.selectedPage = "normalGame";
            PlayNormalNpc("Idle1", true);
            ContentModel.Instance.freeSpinTotalTimes = 0;
            ContentModel.Instance.freeSpinPlayTimes = 0;
            cptBoxFreeCollet.GetTransition("exitFree").Play();
            _slotMachineController.EndBonusFreeSpin();
            OnGameReset();
            ClearPantherTrails();
            HideCollectStar();
            _allWinCredit = 0;
            SetSpinButtonSpinGray();
        }

        /// <summary>进入免费：切页、绑定剩余/总次数与收集盒。</summary>
        private void EnterFreeSpit()
        {
            _pageController.selectedPage = "freeGame";
            ClearPantherTrails();
            HideCollectStar();
            GObject outFrameObj = contentPane.GetChild("OutFrame");
            GLoader loader = outFrameObj as GLoader;
            GComponent frame = loader != null ? loader.component : outFrameObj as GComponent;
            if (frame != null)
            {
                txtRemainFreeTime = frame.asCom.GetChild("txtRemainFreeTime").asTextField;
                txtTotalFreeTime = frame.asCom.GetChild("txtTotalFreeTime").asTextField;
            }

            txtRemainFreeTime.text = ContentModel.Instance.freeSpinTotalTimes.ToString();
            txtTotalFreeTime.text = ContentModel.Instance.freeSpinTotalTimes.ToString();
            txtFreeCollect.text = "0";
            ContentModel.Instance.totalPantherSymbolCount = 0;
            if (txtFreeCollect != null) txtFreeCollect.text = "0";
            for (int i = 0; i < _anchorFreeCollectBorders.Length; i++)
            {
                if (_anchorFreeCollectBorders[i] != null)
                    _anchorFreeCollectBorders[i].visible = false;
            }
        }

        /// <summary>
        /// 本局可视区黑豹飞向收集盒：每只一个 anchorTrails（父节点 anchorEffectFrame）。
        /// 到点后显示 boxFreeCollet.anchorEff_fg_star 上那一份 Eff_fg_star，并刷新个数。
        /// </summary>
        private IEnumerator CollectPantherSymbols()
        {
            List<Cell> cells = GetVisiblePantherCells();
            if (cells.Count == 0)
                yield break;

            if (_anchorEffectFrame == null || _slotMachineController == null)
            {
                AddCollectCount(cells.Count);
                yield break;
            }

            ClearPantherTrails();
            HideCollectStar();
            Vector2 to = GetCollectTargetLocalPos();
            int pending = cells.Count;

            for (int i = 0; i < cells.Count; i++)
            {
                Cell cel = cells[i];
                GComponent trail = CreateAnchorTrails();
                if (trail == null)
                {
                    AddCollectCount(1);
                    pending--;
                    continue;
                }

                _anchorEffectFrame.AddChild(trail);
                trail.SetPivot(0.5f, 0.5f, true);
                trail.xy = _slotMachineController.SymbolCenterToNodeLocalPos(cel.column, cel.row, _anchorEffectFrame);

                if (_goEffFgTuowei != null)
                    GameCommon.FguiUtils.AddWrapper(trail, Object.Instantiate(_goEffFgTuowei));

                //GameSoundHelper3993.Instance.PlaySoundEff(SoundKey.WildTail);

                GComponent captured = trail;
                float delay = i * PantherTrailStagger;
                trail.TweenMove(to, PantherTrailDuration).SetDelay(delay).OnComplete(() =>
                {
                    GameCommon.FguiUtils.DeleteWrapper(captured);
                    captured.Dispose();
                    ShowCollectStar();
                    AddCollectCount(1);
                    pending--;
                });
            }

            yield return new WaitUntil(() => pending <= 0);
            yield return _slotMachineController.SlotWaitForSeconds(PantherStarDuration);
            ClearPantherTrails();
            HideCollectStar();
        }

        /// <summary>收集可视区内所有黑豹符号坐标。</summary>
        private List<Cell> GetVisiblePantherCells()
        {
            List<Cell> cells = new List<Cell>();
            int pantherId = CustomModel.Instance.symbolNumber[9];
            int rows = _slotMachineController.row;
            int cols = _slotMachineController.column;
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    SymbolBase symbol = _slotMachineController.GetVisibleSymbolFromDeck(col, row);
                    if (symbol != null && symbol.number == pantherId)
                        cells.Add(new Cell(col, row));
                }
            }
            return cells;
        }

        /// <summary>收集盒星光中心在拖尾父节点下的本地坐标。</summary>
        private Vector2 GetCollectTargetLocalPos()
        {
            if (_anchorEffectFrame == null)
                return Vector2.zero;

            GComponent target = _anchorEffFgStar != null ? _anchorEffFgStar : cptBoxFreeCollet;
            if (target == null)
                return Vector2.zero;

            Vector2 global = target.LocalToGlobal(new Vector2(target.width * 0.5f, target.height * 0.5f));
            return _anchorEffectFrame.GlobalToLocal(global);
        }

        /// <summary>从包内或模板复制一条拖尾节点。</summary>
        private GComponent CreateAnchorTrails()
        {
            GComponent trail = UIPackage.CreateObject(pkgName, "anchorTrails")?.asCom;
            if (trail == null && _templateAnchorTrails != null && !string.IsNullOrEmpty(_templateAnchorTrails.resourceURL))
                trail = UIPackage.CreateObjectFromURL(_templateAnchorTrails.resourceURL)?.asCom;

            return trail;
        }

        /// <summary>显示并重播收集盒星光粒子。</summary>
        private void ShowCollectStar()
        {
            if (_anchorEffFgStar == null)
                return;

            _anchorEffFgStar.visible = false;
            _anchorEffFgStar.visible = true;
            GameCommon.FguiUtils.RefreshWrapper(_anchorEffFgStar);
            GameObject go = GameCommon.FguiUtils.GetWrapperTarget(_anchorEffFgStar);
            if (go == null)
                return;
            ParticleSystem[] particles = go.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particles.Length; i++)
                particles[i].Play(true);
        }

        /// <summary>隐藏收集盒星光。</summary>
        private void HideCollectStar()
        {
            if (_anchorEffFgStar != null)
                _anchorEffFgStar.visible = false;
        }

        /// <summary>清除拖尾父节点下除模板外的所有子节点。</summary>
        private void ClearPantherTrails()
        {
            if (_anchorEffectFrame == null)
                return;

            for (int i = _anchorEffectFrame.numChildren - 1; i >= 0; i--)
            {
                GObject child = _anchorEffectFrame.GetChildAt(i);
                if (_templateAnchorTrails != null && child == _templateAnchorTrails)
                    continue;

                GComponent com = child as GComponent;
                if (com != null)
                    GameCommon.FguiUtils.DeleteWrapper(com);
                child.Dispose();
            }
        }

        /// <summary>增加免费收集数量，跨档时点亮对应进度框。</summary>
        private void AddCollectCount(int add)
        {
            if (add <= 0) return;
            int oldCount = ContentModel.Instance.totalPantherSymbolCount;
            int newCount = oldCount + add;
            ContentModel.Instance.totalPantherSymbolCount = newCount;
            if (txtFreeCollect != null)
                txtFreeCollect.text = newCount.ToString();

            int[] thresholds = { 4, 10, 18, 28 };
            for (int i = 0; i < thresholds.Length; i++)
            {
                if (oldCount < thresholds[i] && newCount >= thresholds[i]
                    && _anchorFreeCollectBorders[i] != null)
                    _anchorFreeCollectBorders[i].visible = true;
            }
        }

        /// <summary>按当前收集数量取可变成黑豹的符号 ID 集合。</summary>
        private HashSet<int> GetReplaceableSymbolIds()
        {
            return CustomModel.Instance.GetFreePantherReplaceIds(ContentModel.Instance.totalPantherSymbolCount);
        }

        /// <summary>符合档位的动物播 SymbolFree 转换，同时静态图切黑豹，播完再收集。</summary>
        private IEnumerator ConvertEligibleSymbolsToPanther()
        {
            HashSet<int> replaceIds = GetReplaceableSymbolIds();
            if (replaceIds.Count == 0)
                yield break;

            int pantherId = CustomModel.Instance.symbolNumber[9];
            GComponent anchorFrame = contentPane.GetChild("anchorFrame")?.asCom;
            var played = new List<(SymbolBase symbol, string poolKey)>();
            float wait = 0f;
            bool any = false;

            for (int row = 0; row < _slotMachineController.row; row++)
            {
                for (int col = 0; col < _slotMachineController.column; col++)
                {
                    SymbolBase symbol = _slotMachineController.GetVisibleSymbolFromDeck(col, row);
                    if (symbol == null)
                        continue;
                    int fromId = symbol.GetSymbolNumber();
                    if (!replaceIds.Contains(fromId))
                        continue;

                    string prefabPath = CustomModel.Instance.GetFreeConvertPrefab(fromId);
                    symbol.SetSymbolImage(pantherId);
                    any = true;

                    float duration = PlayFreeConvertEffect(symbol, prefabPath, anchorFrame);
                    if (duration > wait)
                        wait = duration;
                    if (duration > 0f)
                        played.Add((symbol, PoolKeyFromPath(prefabPath)));
                }
            }

            if (!any)
                yield break;

            ContentModel.Instance.strDeckRowCol = GetCurrentVisibleDeckRowCol();
            if (wait <= 0.01f)
                wait = 0.4f;
            yield return _slotMachineController.SlotWaitForSeconds(wait);

            for (int i = 0; i < played.Count; i++)
            {
                RecycleFreeConvertEffect(played[i].symbol, played[i].poolKey);
                played[i].symbol?.HideBaseSymbolIcon(false);
                if (played[i].symbol?.goOwnerSymbol != null)
                    FguiSortingOrderManager.Instance.ReturnSortingOrder(played[i].symbol.goOwnerSymbol);
            }
        }

        /// <summary>挂 SymbolFree 并播 collect，返回动画时长。</summary>
        private float PlayFreeConvertEffect(SymbolBase symbol, string prefabPath, GComponent anchorFrame)
        {
            if (symbol == null || string.IsNullOrEmpty(prefabPath) || _fGuiPoolHelper == null)
                return 0f;

            GComponent effectCom = _fGuiPoolHelper.GetObject(TagPoolObject.SymbolAppear, prefabPath)?.asCom;
            if (effectCom == null)
                return 0f;

            symbol.AddSymbolEffect(effectCom, isAmin: false);
            symbol.HideBaseSymbolIcon(true);
            if (anchorFrame != null && symbol.goOwnerSymbol != null)
                FguiSortingOrderManager.Instance.ChangeSortingOrder(symbol.goOwnerSymbol, anchorFrame);

            GameObject goRoot = GameCommon.FguiUtils.GetWrapperTarget(effectCom);
            if (goRoot == null)
                return FreeConvertFallbackDuration;

            GameCommon.FguiUtils.RefreshWrapper(effectCom);
            AnimPlayer player = new AnimPlayer(goRoot);
            player.Play(FreeConvertAnim);
            Animator animator = player.Animator;
            if (animator == null)
                return FreeConvertFallbackDuration;

            animator.Update(0f);
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            return info.length > 0.01f ? info.length : FreeConvertFallbackDuration;
        }

        /// <summary>还掉本次挂上的免费转豹 Spine。</summary>
        private void RecycleFreeConvertEffect(SymbolBase symbol, string poolKey)
        {
            if (symbol?.goOwnerSymbol == null || _fGuiPoolHelper == null || string.IsNullOrEmpty(poolKey))
                return;
            GComponent animator = symbol.goOwnerSymbol.GetChild("animator")?.asCom;
            if (animator == null)
                return;
            _fGuiPoolHelper.ReturnToPool(TagPoolObject.SymbolAppear, poolKey, animator);
        }

        private static string PoolKeyFromPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;
            string[] parts = path.Replace('\\', '/').Split('/');
            string file = parts[parts.Length - 1];
            int dot = file.LastIndexOf('.');
            return dot > 0 ? file.Substring(0, dot) : file;
        }
        #endregion


        #region NPC
        /// <summary>在普通 NPC 与大奖 NPC 预制体之间切换并重新挂到 anchorNpc。</summary>
        private void SwitchNpc(bool isSmallGame)
        {
            if (anchorNpc == null) return;

            _rewardMgr?.SetSmallGameNpc(null);
            _animNormalNpc?.DetachAll();
            _animNormalNpc = null;
            GameCommon.FguiUtils.DeleteWrapper(anchorNpc);
            clonegoNormalNpc = null;
            clonegoSmallGameNpc = null;

            GameObject prefab = isSmallGame ? goSmallGameNpc : goNormalNpc;
            if (prefab == null) return;

            GameObject clone = GameObject.Instantiate(prefab);
            if (isSmallGame)
            {
                clonegoSmallGameNpc = clone;
                BindSmallGameNpc(clone);
            }
            else
            {
                clonegoNormalNpc = clone;
                BindNormalNpc(clone);
            }

            GameCommon.FguiUtils.AddWrapper(anchorNpc, clone);
        }

        /// <summary>绑定普通局 NPC 并循环播 Idle1。</summary>
        private void BindNormalNpc(GameObject clone)
        {
            _animNormalNpc = clone != null ? new AnimPlayer(clone) : null;
            PlayNormalNpc("Idle1", true);
        }

        /// <summary>绑定大奖 NPC、播 Idle1，并交给 RewardMgr 驱动。</summary>
        private void BindSmallGameNpc(GameObject clone)
        {
            _animNormalNpc = clone != null ? new AnimPlayer(clone) : null;
            PlayNormalNpc("Idle1", true);
            _rewardMgr?.SetSmallGameNpc(_animNormalNpc);
        }

        /// <summary>按状态名播放当前 NPC 动画。</summary>
        private void PlayNormalNpc(string animName, bool loop = false)
        {
            if (_animNormalNpc == null || string.IsNullOrEmpty(animName)) return;
            _animNormalNpc.Play(animName, loop);
        }

        /// <summary>豹头拖尾到达赢分框：亮框、Bonus idle、加临时分。</summary>
        private void OnPantherBonusArrived(int score, Cell cel)
        {
            //win框特效
            _panelController.ShowNormalWinBorder();
            if (cel != null)
                _slotMachineController.SetPantherBonusCollectedIdle(cel.row, cel.column);

            if (score <= 0)
                return;

            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_WIN_EVENT,
                new EventData<long>(SlotMachineEvent.SingleWinBonus, score));
            MainBlackboardController.Instance.AddMyTempCredit(score, true);
        }

        /// <summary>Panel 赢分框中心在拖尾父节点下的本地坐标。</summary>
        private Vector2 GetWinBorderLocalPos()
        {
            if (_anchorEffectFrame == null)
                return Vector2.zero;

            GComponent target = _panelController?.AnchorWinBorder;
            if (target == null)
                return Vector2.zero;

            Vector2 global = target.LocalToGlobal(new Vector2(target.width * 0.5f, target.height * 0.5f));
            return _anchorEffectFrame.GlobalToLocal(global);
        }

        /// <summary>按行列取 BonusData 上对应格子的豹头分值。</summary>
        private static int GetPantherBonusScore(int row, int col)
        {
            int index = row * CustomModel.Instance.column + col;
            int[] data = ContentModel.Instance.BonusData;
            if (data != null && index >= 0 && index < data.Length)
                return ContentModel.GetDisplayScore(data[index]);
            return 0;
        }

        /// <summary>
        /// 普通局停轮后播 NPC。优先级：trig（免费/Bonus）&gt; win3（黑豹+Bonus同屏）&gt; win2（动物线赢）&gt; win1（扑克牌线赢）&gt; Idle2（连续 5 局未中奖）。
        /// Controller 播完会回到 Idle1。免费局由 FGUI 隐藏 NPC，不走这里。
        /// </summary>
        private void TryPlayNormalNpcAfterSpin(List<SymbolWin> winList)
        {
            if (_animNormalNpc == null) return;

            bool isFeatureTrigger = ContentModel.Instance.isFreeSpinTrigger || ContentModel.Instance.isSmallGameTrigger;
            bool hasLineWin = winList != null && winList.Count > 0;
            if (isFeatureTrigger || hasLineWin)
                _notHitSpinCount = 0;
            else
                _notHitSpinCount++;

            if (isFeatureTrigger)
            {
                PlayNormalNpc("trig");
                return;
            }

            if (ContentModel.Instance.isPantherWin)
            {
                PlayNormalNpc("win3");
                return;
            }

            if (HasWinSymbolInRange(winList, 5, 9))
            {
                PlayNormalNpc("win2");
                return;
            }

            if (HasWinSymbolInRange(winList, 0, 4))
            {
                PlayNormalNpc("win1");
                return;
            }

            if (_notHitSpinCount >= NpcNoWinIdle2Count)
            {
                PlayNormalNpc("Idle2");
                _notHitSpinCount = 0;
            }
        }

        /// <summary>中奖列表是否包含指定符号 ID 区间。</summary>
        private static bool HasWinSymbolInRange(List<SymbolWin> winList, int minInclusive, int maxInclusive)
        {
            if (winList == null) return false;
            for (int i = 0; i < winList.Count; i++)
            {
                int symbolNumber = winList[i].symbolNumber;
                if (symbolNumber >= minInclusive && symbolNumber <= maxInclusive)
                    return true;
            }
            return false;
        }
        #endregion

        #region 彩金游戏
        /// <summary>大奖触发：弹窗、切 bonusGame、跑 RewardMgr，结算弹窗后切回普通局。</summary>
        private IEnumerator SmallGameTrigger(Action successCallback, Action<string> errorCallback)
        {
            _slotMachineController.BeginBonusFreeSpin();
            ContentModel.Instance.isSmallGameTrigger = false;
            ContentModel.Instance.isSmallGameSpin = true;

            bool isNext = false;
            PageManager.Instance.OpenPageAsync(PageName.MeiZhouHeiBaoPopupSmallGameTrigger, null, (ed) =>
            {
                pagFade.StopWithDefaults();
                pagFade.Play(new PagSequencePlay(
                    new[] { new PagSegment("Transition_JPTONG-out_bmp/Transition_JPTONG-out_bmp", 1) },
                    PagPlayLayout.Center,
                    PagPresentationDefaults.DisplayScale,
                    useGpuSyncGroup: false,
                    callbacks: new PagPlayCallbacks(
                    onFinished: () => pagFade?.StopWithDefaults(),
                    stopAfterFinished: true)));
               
                isNext = true;
            });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            yield return _slotMachineController.SlotWaitForSeconds(1.5f * Time.timeScale);
            _pageController.selectedPage = "bonusGame";
            SwitchNpc(true);
            _panelController.ChangButtonNo(true);
            _panelController.HideWinBorders();
            ContentModel.Instance.isSpin = false;
            if (!TestManager.Instance.IsAutoModeRunning)
                ContentModel.Instance.isAuto = false;
            ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
            _panelController.ChangButtonNo(true);  // 放在 btnSpinState 之后，避免 Stop 分支里的 ChangButtonNo(false) 把押注按钮又打开
            _slotMachineController.SkipWinLine(true);

            yield return _slotMachineController.SlotWaitForSeconds(5.0f * Time.timeScale);
            //EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT, new EventData(Game3993AudioEvent.BgmBonusGame));
            List<int> matrix = SlotTool.GetDeckRowCol(ContentModel.Instance.strDeckRowCol);
            //大奖主逻辑
            _rewardMgr.Enter(matrix, ContentModel.Instance.BonusData);
            UnlockStopButton();
            if (TestManager.Instance.IsAutoModeRunning)
                _rewardMgr.StartRoll();
   
            yield return new WaitUntil(() => ContentModel.Instance.isSmallGameFinish == true);
            SetSpinButtonSpinGray();
            ContentModel.Instance.isSpin = false;

            PageManager.Instance.OpenPageAsync(PageName.MeiZhouHeiBaoPopupSmallGameResult, null, (ed) =>
            {
                pagFade.StopWithDefaults();
                pagFade.Play(new PagSequencePlay(
                    new[] { new PagSegment("Transition_JPTONG-out_bmp/Transition_JPTONG-out_bmp", 1) },
                    PagPlayLayout.Center,
                    PagPresentationDefaults.DisplayScale,
                    useGpuSyncGroup: false,
                    callbacks: new PagPlayCallbacks(
                    onFinished: () => pagFade?.StopWithDefaults(),
                    stopAfterFinished: true)));

                isNext = true;
            });

            yield return new WaitUntil(() => isNext == true);
            yield return _slotMachineController.SlotWaitForSeconds(1.5f * Time.timeScale);
            _pageController.selectedPage = "normalGame";
            SwitchNpc(false);
            _slotMachineController.CloseSlotCover();
            _slotMachineController.SkipWinLine(false);
            _slotMachineController.ClearBonusScoreBinds();
            _slotMachineController.EndBonusFreeSpin();
            _panelController.ChangButtonNo(false);
            _panelController.HideWinBorders();
            ContentModel.Instance.isSmallGameFinish = false;
            ContentModel.Instance.isSmallGameSpin = false;
            ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
            ContentModel.Instance.BonusBet = 0;
            ContentModel.Instance.bonusSpinTime = 3;
            ContentModel.Instance.isJackpotGame = false;
            ContentModel.Instance.TotalJackpotBet = 0;
            ContentModel.Instance.JPTypeArray = Array.Empty<int>();
            ContentModel.Instance.JPBetArray = Array.Empty<int>();
            ContentModel.Instance.BonusRound?.Clear();
            if (ContentModel.Instance.BonusData != null) Array.Clear(ContentModel.Instance.BonusData, 0, ContentModel.Instance.BonusData.Length);
            yield return _slotMachineController.SlotWaitForSeconds(6.0f * Time.timeScale);
            //EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT,new EventData(Game3993AudioEvent.BgmRegularGame));
        }

        /// <summary>大奖旋转循环（逻辑已并入 RewardMgr，此处预留）。</summary>
        private IEnumerator SmallGameSpin(Action successCallback, Action<string> errorCallback)
        {
            yield break;
        }

        /// <summary>大奖单次旋转（逻辑已并入 RewardMgr，此处预留）。</summary>
        private IEnumerator SmallSpinOnce(Action successCallback, Action<string> errorCallback)
        {
          
            yield break;
        }

        /// <summary>大奖结算（逻辑已并入 SmallGameTrigger 弹窗，此处预留）。</summary>
        private IEnumerator SmallGameResult(Action successCallback, Action<string> errorCallback)
        {
            yield break;
        }

        #endregion
    }
}