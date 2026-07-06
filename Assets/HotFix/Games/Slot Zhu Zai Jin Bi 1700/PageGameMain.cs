using FairyGUI;
using GameMaker;
using Newtonsoft.Json;
using PusherEmperorsRein;
using SBoxApi;
using SimpleJSON;
using SlotMaker;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using _spinWEMD = SlotMaker.SpinWinEffectSettingModel;
namespace SlotZhuZaiJinBi1700
{
    public class GameConfigRoot
    {
        [JsonProperty("game_id")] public int GameId;//游戏 ID

        [JsonProperty("game_name")] public string GameName;//名称

        [JsonProperty("display_name")] public string DisplayName;//显示名称

        [JsonProperty("line_num")] public int LineNum;//线数

        [JsonProperty("win_level_multiple")] public Dictionary<string, long> WinLevelMultiple { get; set; }//赢钱倍数

        [JsonProperty("symbol_paytable")] public Dictionary<string, PayTableSymbolInfo> SymbolPaytable { get; set; }//符号赔率表

        [JsonProperty("pay_lines")] public List<List<int>> pay_lines { get; set; } //支付线
    }

    /// <summary>
    /// 1700 主游戏界面：老虎机逻辑、底部 Panel、PAG/Spine 测试区与彩金展示。
    /// PreloadPage 阶段（isOpen=false）会完成 AB 与视觉初始化，供 Loading 关页前预热。
    /// </summary>
    public class PageGameMain : MachinePageBase
    {
        public const string pkgName = "SlotZhuZaiJinBi1700";
        public const string resName = "PageGameMain";

        private bool isInitPool = false; //资源池是否初始化
        private bool tipCoinIn = false; //提示硬币输入
        bool isAddCreditAnim => !(slotMachineCtrl.isStopImmediately == true || SBoxModel.Instance.isCoinOutImmediately);
        Coroutine corReelsTurn,corGameIdel, corGameOnce, corEffectSlowMotion, coGameAuto;
        //加速框
        bool isEffectSlowMotion2 = false;
        bool isEffectSlowMotion3 = false;
        bool isEffectSlowMotion4 = false;
        EventData _data = null;
        //游戏控制
        private GameObject goGameCtrl;
        private SlotMachineController1700 slotMachineCtrl;
        private MonoHelper mono;
        FguiPoolHelper fguiPoolHelper;
        FguiGObjectPoolHelper gObjectPoolHelper;
        PayTableController payTableController = new PayTableController(); //说明书赔率配置控制
        //组件
        GComponent gSlotCover, gPlayLines, gFrame;              //滚轴组件
        private GComponent gOwnerPanel;                         //菜单
        private GComponent gNormalGameFrame, gFreeGameFrame;    //外框
        private GComponent gNormalInnerFrame, gFreeInnerFrame;  //内框
        private GComponent gNormalBg, gFreeBg;                  //背景
        //过度动画
        private GComponent anchorNormalFrame, anchorFreeFrame;
        private GameObject goNormalFrame, goFreeFrame;
        private GameObject CLonegoNormalFrame, ClonegoFreeFrame;
        private Animator animatorNormalFrame;
        private SkeletonMecanim SMNormalFrame;
        // --- PAG 测试（anchorPagTest + TestBigWin.prefab，详见 AnchorPagTest.xml）---
        /// <summary>FGUI 锚点 anchorPagTest，内含 pagEffect1~7 与 holder 布局节点。</summary>
        private GComponent _anchorPagTest;
        /// <summary>TestBigWin.prefab 模板，OnInit 异步加载。</summary>
        private GameObject _goPagTestPrefab;
        /// <summary>挂到 anchorPagTest 的实例，承载 Spine 对照节点。</summary>
        private GameObject _clonePagTest;
        /// <summary>PAG1~PAG4、Spine1~Spine5 测试按钮是否已绑定 onClick。</summary>
        private bool _pagTestButtonsBound;
        /// <summary>FGUI GLoader 名 pagEffect1~14：PAG1~4、旧组播槽、PAG5~12。</summary>
        private static readonly string[] PagTestLoaderNames =
        {
            "pagEffect1",   // PAG1 Fade
            "pagEffect2",   // PAG2
            "pagEffect3",   // PAG3
            "pagEffect4",   // PAG4 glow_loop_720
            "pagEffect5",   // PAG5 glow_loop_half_1920
            "pagEffect6",   // PAG6 glow_loop_half_1920 1.5x
            "pagEffect7",   // PAG7 glow_loop_full_1920
            "pagEffect8",  // PAG8 glow_in_full_1920->glow_loop_full_1920
            "pagEffect9",  // PAG9 BigWinNPC
            "pagEffect10",  // PAG10 FreeNPC
            "pagEffect11",  // PAG11 NormalNPC
            "pagEffect12",  // PAG12 RewardNPC
        };
        private const int PagTestLoaderCount = 12;
        private const int PagTestPagButtonCount = 12;
        private const int PagTestSpineCount = 5;
        private const int MaxPagTestNpcCount = 4;

        private struct PagTestSpineConfig
        {
            public string NodeName;
            public string PlayAnim;
        }

        /// <summary>Spine1~5 对照节点名与按钮播放动画名。</summary>
        private static readonly PagTestSpineConfig[] PagTestSpineConfigs =
        {
            new PagTestSpineConfig { NodeName = "Spine Mecanim GameObject (jp_pup_grand)", PlayAnim = "GRAND_in" },
            new PagTestSpineConfig { NodeName = "Spine Mecanim GameObject (ng_pop_bigWin)", PlayAnim = "bigwin_start" },
            new PagTestSpineConfig { NodeName = "Spine Mecanim GameObject (jp_pup_GRAND)", PlayAnim = "in" },
            new PagTestSpineConfig { NodeName = "Spine Mecanim GameObject (ng_bor_boom1)", PlayAnim = "start" },
            new PagTestSpineConfig { NodeName = "Spine Mecanim GameObject (ng_ic_bigwin)", PlayAnim = "bigwin_start" },
        };

        /// <summary>pagEffect1~14 对应 PagSlotBinding 实例标签。</summary>
        private static readonly string[] PagTestSlotInstanceLabels =
        {
            "PagTest1", "PagTest2", "PagTest3", "PagTest4", "PagTest5", "PagTest6", "PagTest7", "PagTest8", "PagTest9","PagTest10", "PagTest11", "PagTest12",
        };

        /// <summary>pagEffect1~14 槽位绑定。</summary>
        private readonly PagSlotBinding[] _pagTestSlotBindings = new PagSlotBinding[PagTestLoaderCount];
        /// <summary>PAG1~12 是否正在播放。</summary>
        private readonly bool[] _pagTestShowing = new bool[PagTestPagButtonCount];
        /// <summary>PAG1~9 composition 是否已预热。</summary>
        private readonly bool[] _pagTestCacheWarmed = new bool[PagTestPagButtonCount];
        private readonly Animator[] _pagTestSpineAnimators = new Animator[PagTestSpineCount];
        private readonly SkeletonMecanim[] _pagTestSpineMecanims = new SkeletonMecanim[PagTestSpineCount];
        /// <summary>Spine1~5 对照动画是否可见。</summary>
        private readonly bool[] _spineTestShowing = new bool[PagTestSpineCount];
        /// <summary>PAG1~PAG12 按钮各自持有的播放协程；OnClose 或再次点击时须 Stop。</summary>
        private readonly Coroutine[] _corPagTest = new Coroutine[PagTestPagButtonCount];
        /// <summary>PAG2 / 进局过渡用 PAG 文件名。</summary>
        private const string PagTestName1 = "BigWin_1080.pag";
        /// <summary>PAG1 按钮播放的 PAG 文件名。</summary>
        private const string PagTestName2 = "Fade.pag";
        /// <summary>PAG3 按钮播放的 PAG 文件名。</summary>
        private const string PagTestName3 = "XingXing2.pag";
     
        private const string NpcPagFolderPrefix = "3997Npc/";
        private static readonly string[] NpcBigWinSequence =
     {
             $"{NpcPagFolderPrefix}BigWinNPC/bigwin_start1.pag",
             $"{NpcPagFolderPrefix}BigWinNPC/bigwin_idle1.pag",
             $"{NpcPagFolderPrefix}BigWinNPC/supwin_start1.pag",
             $"{NpcPagFolderPrefix}BigWinNPC/supwin_idle1.pag",
             $"{NpcPagFolderPrefix}BigWinNPC/megawin_start1.pag",
             $"{NpcPagFolderPrefix}BigWinNPC/megawin_idle1.pag",
        };
        private static readonly string[] NpcFreeSequence =
        {
            $"{NpcPagFolderPrefix}FreeNPC/Wealth_fg_npc_upgrade1.pag",
            $"{NpcPagFolderPrefix}FreeNPC/Wealth_fg_npc_upgrade2.pag",
            $"{NpcPagFolderPrefix}FreeNPC/Wealth_fg_npc_settlement.pag",
        };
        private static readonly string[] NpcNormalSequence =
        {
            $"{NpcPagFolderPrefix}NormalNPC/Wealth_ng_npc_idle01.pag",
            $"{NpcPagFolderPrefix}NormalNPC/Wealth_ng_npc_idle02.pag",
            $"{NpcPagFolderPrefix}NormalNPC/Wealth_ng_npc_not winning.pag",
            $"{NpcPagFolderPrefix}NormalNPC/Wealth_ng_npc_atmosphere.pag",
            $"{NpcPagFolderPrefix}NormalNPC/Wealth_ng_npc_not triggered.pag",
            $"{NpcPagFolderPrefix}NormalNPC/Wealth_ng_npc_trigger sg.pag",
            $"{NpcPagFolderPrefix}NormalNPC/Wealth_ng_npc_trigger fg.pag",
            $"{NpcPagFolderPrefix}NormalNPC/Wealth_ng_npc_win1.pag",
            $"{NpcPagFolderPrefix}NormalNPC/Wealth_ng_npc_win2.pag",
            $"{NpcPagFolderPrefix}NormalNPC/Wealth_ng_npc_win3.pag",
        };
        private static readonly string[] NpcRewardSequence =
        {
            $"{NpcPagFolderPrefix}RewardNPC/Wealth_sg_npc_idle1.pag",
            $"{NpcPagFolderPrefix}RewardNPC/Wealth_sg_npc_idle2.pag",
            $"{NpcPagFolderPrefix}RewardNPC/Wealth_sg_npc_appear.pag",
            $"{NpcPagFolderPrefix}RewardNPC/Wealth_sg_npc_reset.pag",
            $"{NpcPagFolderPrefix}RewardNPC/Wealth_sg_npc_settlement1.pag",
            $"{NpcPagFolderPrefix}RewardNPC/Wealth_sg_npc_settlement2.pag",
            $"{NpcPagFolderPrefix}RewardNPC/Wealth_sg_npc_settlement3.pag",
        };
        private static readonly string[][] PagTestNpcSequences =
        {
           NpcBigWinSequence, NpcFreeSequence, NpcNormalSequence, NpcRewardSequence,
        };
        private static readonly string[] PagTestNpcLabels =
        {
           "bigwinNpc", "freeNpc", "normalNpc", "rewardNpc",
        };
        private const string PagGlowLoop720 = "Lopp/glow_loop_720.pag";
        private const string PagGlowLoopHalf = "Lopp/glow_loop_half_1920.pag";
        private const string PagGlowLoopFull = "Lopp/glow_loop_full_1920.pag";
        private const string PagGlowInFull = "Lopp/glow_in_full_1920.pag";
        /// <summary>PAG5 glow_loop_720 FGUI 显示倍率（720 合成放大 1.5 倍至 1080 宽）。</summary>
        private const float PagGlow720DisplayScale = 1f;

        private enum PagTestPlaybackKind
        {
            SingleLoop,
            IntroLoop,
            Sequence,
        }

        private struct PagTestPlaybackConfig
        {
            public PagTestPlaybackKind Kind;
            public string PagFile;
            public string IntroFile;
            public string LoopFile;
            public string[] Sequence;
            public float DisplayScale;
            public bool LazyBindSlot;
            public string Label;
        }

        /// <summary>FGUI 显示缩放；1=按合成尺寸×1 显示。</summary>
        private const float PagTestDisplayScale = 1f;
        /// <summary>false：按合成尺寸×displayScale；true：裁剪到 holder。</summary>
        private const bool PagTestClampDisplayToHolder = false;
        /// <summary>Overlay 模式下单次 PAG 播放时长兜底（秒）。</summary>
        private const float PagTestDuration = 8f;
        /// <summary>XingXing1 专用时长兜底（秒）。</summary>
        private const float PagTestNezaPagDuration = 8f;
        /// <summary>等待 Native PlayStarted 回调的超时（秒）。</summary>
        private const float PagTestPlayStartedTimeoutSec = 45f;
        /// <summary>NPC 序列单段时长兜底（秒）。</summary>
        private const float PagTestNpcSegmentDurationFallbackSec = 8f;
        /// <summary>true：所有 PAG 路径 Play / PlayFguiGpuSequence 均纳入 PagGpuSyncGroup。</summary>
        private const bool PagTestUseGpuSyncGroup = true;
        /// <summary>相对 GameRes 的本游戏 PAG 目录（与 PopupGameLoading.GamePagFolder 保持一致）。</summary>
        private const string GamePagFolder = "Games/Slot Zhu Zai Jin Bi 1700/Pag";
        private const string PagLogPrefix = "[1700 PagTest]";
        /// <summary>Phase0 A/B：true 时全屏播 PAG；Phase1 通过后保持 false，走 FGUI extra 对齐。</summary>
        private const bool PagTestDebugFullScreen = false;
        /// <summary>true 时交替循环播 XingYunZhiLun_1080 与 neza；仅按钮触发时保持 false。</summary>
        private const bool PagTestLoop = false;
        /// <summary>true：PAG 在 FGUI pagEffect（层级由 FGUI 配置）；false：Activity WM 浮层。</summary>
        private const bool PagTestUseFguiTexture = true;
        /// <summary>FguiTexture 离屏最大边；0=合成原尺寸不限制，512=降压缩屏（FGUI 仍按合成原尺寸显示）。</summary>
        private const int PagTestFguiMaxDisplaySide = 0;
        /// <summary>纹理模式出帧目标帧率，Play 开始后可能与 composition frameRate 对齐。</summary>
        private const int PagTestFguiFps = 30;
        /// <summary>Overlay 模式：true 时 native 立即 ImageView 软件出帧。</summary>
        private const bool PagTestOverlayFallback = false;
        private const string BorderMegaWinPrefabRoot = "Assets/GameRes/Games/Cai Fu Huo Che 3996/newProject/Effect/SmallGame/Art/Effects/Prefabs/";
        private static readonly string[] BorderMegaWinPrefabNames =
        {
            "eff_pop_border_megawin1",
            "eff_pop_border_megawin2",
            "eff_pop_border_megawin3",
            "eff_pop_border_megawin4",
            "eff_pop_border_megawin5",
        };
        private static readonly string[] BorderMegaWinHolderNames =
        {
            "holder1",
            "holder2",
            "holder3",
            "holder4",
            "holder5",
        };
        private const string BorderMegaWinLogPrefix = "[1700 BorderMegaWin]";
        private GameObject[] _goBorderMegaWinPrefabs = new GameObject[5];
        private GameObject[] _cloneBorderMegaWinEffects = new GameObject[5];
        private Transform[] _borderMegaWinEffectRoots = new Transform[5];
        private bool[] _borderMegaWinShowing = new bool[5];
        private bool _borderMegaWinButtonsBound;
        private bool _comboPlayControlsBound;
        private GComboBox _comboPagBox;
        private GComboBox _comboSpineBox;
        private GComboBox _comboEffectBox;
        private bool _comboPlayActive;
        private Coroutine _comboPlayCoroutine;
        private int _comboActivePagIndex;
        private int _comboActiveSpineIndex;
        private int _comboActiveEffectIndex;
        private static readonly string[] ComboPagDropdownItems =
        {
            "无", "PAG1", "PAG2", "PAG3", "PAG4", "PAG5", "PAG6", "PAG7", "PAG8", "PAG9", "PAG10", "PAG11", "PAG12",
        };
        private static readonly string[] ComboSpineDropdownItems =
        {
            "无", "Spine1", "Spine2", "Spine3", "Spine4", "Spine5",
        };
        private static readonly string[] ComboEffectDropdownItems =
        {
            "无", "Effect1", "Effect2", "Effect3", "Effect4", "Effect5",
        };
        //免费组件
        private GComponent gFreeTimeBox, gFreeWinBox;
        private GComponent gFreeSlotMachine;
        //彩金
        MiniReelGroup uiJPMajorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMinorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMiniCtrl = new MiniReelGroup();
        long TotalBet => (long)MainModel.Instance.contentMD.totalBet;

        /// <summary>
        /// 1700：底部 Panel 异步就绪后触发 PageManager 的 preLoadedCallback
        /// </summary>
        /// <param name="res"> 事件数据</param>
        private void OnBottomPanelReadyForPreload(EventData res)
        {
            if (res == null || res.name != PanelEvent.BottomPanelReady)
                return;

            int gameId = Convert.ToInt32(res.value);
            if (gameId != 1700)
                return;

            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_EVENT, OnBottomPanelReadyForPreload);
            preLoadedCallback?.Invoke();
        }

        /// <summary>
        /// 异步加载 GameController、FGUI 包、Frame/PAG Prefab 等；全部完成后 InitParam
        /// </summary>
        protected override void OnInit()
        {

            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            int count = 10;

            Action callback = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam(null);
                }
            };


            //1
            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/Game Controller/Slot Game Main ControllerClone.prefab",
            (GameObject clone) =>
            {
                if (goGameCtrl != null) //防止重复加载
                {
                    return;
                }
                goGameCtrl=GameObject.Instantiate(clone);
                goGameCtrl.name = "Slot Game Main Controller1700";
                goGameCtrl.transform.SetParent(null);
                //获取组件引用
                slotMachineCtrl=goGameCtrl.transform.Find("Slot Machine").GetComponent<SlotMachineController1700>();
                mono=goGameCtrl.transform.GetComponent<MonoHelper>();
                
                Debug.LogWarning("i am Game Controller");

                fguiPoolHelper = goGameCtrl.transform.Find("Pool").GetComponent<FguiPoolHelper>();
                gObjectPoolHelper = goGameCtrl.transform.Find("GObject Pool").GetComponent<FguiGObjectPoolHelper>();
                callback();
            });
            //2
            ResourceManager02.Instance.LoadAssetBundleAsync(
                "Assets/GameRes/Games/Emperors Rein 200/FGUIs", (ab) =>
            {
                UIPackage.AddPackage(ab);
                callback();
            });
            //3
            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/GameMain/NormalFrame.prefab",
             (GameObject clone) =>
             {
                 goNormalFrame = clone;
                 callback();
             });
            //4
            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/GameMain/FreeFrame.prefab",
            (GameObject clone) =>
            {
                goFreeFrame = clone;
                callback();
            });
            //5 — PAG 测试场景根（Spine 对照 + 挂载到 anchorPagTest 的 3D 层）
            ResourceManager02.Instance.LoadAsset<GameObject>(
          "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/TurnTable/TestBigWin.prefab",
          (GameObject clone) =>
          {
              _goPagTestPrefab = clone;
              callback();
          });
            //6-10 — 特效性能测试
            for (int i = 0; i < BorderMegaWinPrefabNames.Length; i++)
            {
                int capturedIndex = i;
                ResourceManager02.Instance.LoadAsset<GameObject>(
                    BorderMegaWinPrefabRoot + BorderMegaWinPrefabNames[capturedIndex] + ".prefab",
                    (GameObject clone) =>
                    {
                        _goBorderMegaWinPrefabs[capturedIndex] = clone;
                        callback();
                    });
            }

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
                        CommonPopupHandler.Instance.ClosePopup();
                        OnClickSpinButton(res);

                    },
                },

                longClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        DebugUtils.LogError("游戏接受到机台长按的数据：Spin");
                        EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, true); // isLongClick
                        CommonPopupHandler.Instance.ClosePopup();
                        OnClickSpinButton(res);
                    }
                }

            };

 
        }
        /// <summary>
        /// 语言切换时解绑测试按钮、重建 contentPane 并重新 InitParam
        /// </summary>
        /// <param name="lang"> 目标语言</param>
        protected override void OnLanguageChange(I18nLang lang)
        {
            ClearBorderMegaWinButtons();
            ClearComboPlayControls();
            ClearPagTestButtons(); // 语言切换重建 UI 前解绑 PAG 测试按钮
            FguiI18nTextAssistant.Instance.DisposeAllTranslate(this.contentPane);
            this.contentPane.Dispose(); // 释放当前UI
            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            InitParam(null);
            //FguiI18nTextAssistant.Instance.TranslateComponent(this.contentPane);
        }
        /// <summary>
        /// 打开主界面：激活 GameController、注册事件、播放背景音乐并 InitParam
        /// </summary>
        /// <param name="name"> 页面名</param>
        /// <param name="data"> 事件数据</param>
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
            EventCenter.Instance.AddEventListener<WinJackpotInfo>(GlobalEvent.JackpotOnlineWin, OnJackpotOnLine);
            GameSoundHelper.Instance.PlayMusicSingle(SoundKey.RegularBG);
            InitParam(data);
        }
        /// <summary>
        /// 关闭主界面：注销事件、停音乐、停止并释放 PAG/Spine/边框特效资源
        /// </summary>
        /// <param name="null"> null</param>
        public override void OnClose(EventData data = null)
        {
            EventCenter.Instance.RemoveEventListener<CoinPushSpinParseEventArgs>(SBoxEventHandle.SBOX_COIN_PUSH_SPIN_PARSE, OnCoinPushSpinResultParse);
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnClickSpinButton);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_SLOT_EVENT, OnStopSlot);
            EventCenter.Instance.RemoveEventListener<WinJackpotInfo>(GlobalEvent.JackpotOnlineWin, OnJackpotOnLine);
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_EVENT, OnBottomPanelReadyForPreload);
            GameSoundHelper.Instance.StopMusic();
            // PAG：先停协程与 Native 播放，再解绑按钮、Dispose PagSlotBinding
            StopAllPagTest();
            StopAllBorderMegaWinEffects();
            ClearBorderMegaWinButtons();
            ClearComboPlayControls();
            ClearPagTestButtons();
            DisposeBorderMegaWinEffects();
            DisposePagTestResources();
            if (goGameCtrl != null && goGameCtrl.activeSelf)
            {
                goGameCtrl.SetActive(false);
            }
            base.OnClose(data);
        }
        /// <summary>
        /// 解析 Spin 回包，委托 1700 专用 Payload 解析器
        /// </summary>
        /// <param name="e"> Spin 解析事件参数</param>
        private void OnCoinPushSpinResultParse(CoinPushSpinParseEventArgs e)
        {
            e.Result = MachineDataG1700Controller.ParseCoinPushSpinPayload(e.Data, e.StartPos);
        }

        /// <summary>
        /// /// 初始化主界面参数。isOpen=false（PreloadPage）时也会执行视觉层：对象池、Panel、Frame、PAG/Spine Attach； /// isOpen=true 时额外执行网络拉取、额度同步与 FreeSpin 恢复。 ///
        /// </summary>
        /// <param name="data"> 事件数据</param>
        public void InitParam(EventData data)
        {
            if (data != null) _data = data;
            if (!isInit) return;

            MainModel.Instance.gameID = 1700;
            MainModel.Instance.gameName = "ZhuZaiJinBi1700";
            MainModel.Instance.displayName = "ZhuZaiJinBi1700";
            MainModel.Instance.lineNum = 15;
            MainModel.Instance.contentMD = ContentModel.Instance;
            MainModel.Instance.cutomMD = CustomModel.Instance;

            List<GComponent> lstPayTable = new List<GComponent>();
            foreach (string url in CustomModel.Instance.payTable)
            {
                GComponent paytable = UIPackage.CreateObjectFromURL(url).asCom;
                lstPayTable.Add(paytable);
            }
            ContentModel.Instance.goPayTableLst = lstPayTable.ToArray();
            payTableController.Init(lstPayTable);

            GComponent gSlotMachine = contentPane.GetChild("slotMachine").asCom;
            GComponent gReels = gSlotMachine.GetChild("reels").asCom;
            gSlotCover = gSlotMachine.GetChild("slotCover").asCom;
            gPlayLines = gSlotMachine.asCom.GetChild("playLines").asCom;
            gFrame = contentPane.GetChild("anchorFrame").asCom;
            slotMachineCtrl.Init(gSlotCover, gPlayLines, gReels, gFrame, fguiPoolHelper, gObjectPoolHelper);

            if (fguiPoolHelper != null && isInitPool == false)
            {
                isInitPool = true;
                fguiPoolHelper.Add(TagPoolObject.SymbolHit, CustomModel.Instance.symbolHitEffect.Values.ToList(), "symbol_hit#", 5);
                fguiPoolHelper.PreLoad(TagPoolObject.SymbolHit);
                fguiPoolHelper.Add(TagPoolObject.SymbolBorder, CustomModel.Instance.borderEffect, "border#", 5);
                fguiPoolHelper.Add(TagPoolObject.SymbolAppear, CustomModel.Instance.symbolAppearEffect.Values.ToList(), "symbol_appear#", 5);
                fguiPoolHelper.PreLoad(TagPoolObject.SymbolAppear);
            }

            gOwnerPanel = contentPane.GetChild("panel").asCom;
            ContentModel.Instance.goAnthorPanel = gOwnerPanel;
            MainModel.Instance.contentMD.goAnthorPanel = gOwnerPanel;
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_EVENT, OnBottomPanelReadyForPreload);
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_EVENT, OnBottomPanelReadyForPreload);
            EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
                new EventData<GComponent>(PanelEvent.AnchorPanelChange, gOwnerPanel));

            gNormalBg = contentPane.GetChild("normalBG").asCom;
            gFreeBg = contentPane.GetChild("freeBG").asCom;
            gNormalGameFrame = contentPane.GetChild("normalGameframe").asCom;
            gFreeGameFrame = contentPane.GetChild("freeGameFrame").asCom;
            gNormalInnerFrame = contentPane.GetChild("normalInnerFrame").asCom;
            gFreeInnerFrame = contentPane.GetChild("freeInnerFrame").asCom;

            gFreeBg.visible = false;
            gFreeGameFrame.visible = false;
            gFreeInnerFrame.visible = false;

            GComponent localNormalFrame = contentPane.GetChild("anchorNormalFrame").asCom;
            if (anchorNormalFrame != localNormalFrame)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorNormalFrame);
                CLonegoNormalFrame = GameObject.Instantiate(goNormalFrame);
                animatorNormalFrame = CLonegoNormalFrame.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                SMNormalFrame = CLonegoNormalFrame.transform.GetChild(0).GetChild(0).GetComponent<SkeletonMecanim>();
                anchorNormalFrame = localNormalFrame;
                GameCommon.FguiUtils.AddWrapper(anchorNormalFrame, CLonegoNormalFrame);
            }

            GComponent localFreeFrame = contentPane.GetChild("anchorFreeFrame").asCom;
            if (anchorFreeFrame != localFreeFrame)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorFreeFrame);
                ClonegoFreeFrame = GameObject.Instantiate(goFreeFrame);
                anchorFreeFrame = localFreeFrame;
                GameCommon.FguiUtils.AddWrapper(anchorFreeFrame, ClonegoFreeFrame);
            }
            anchorNormalFrame.visible = false;
            anchorFreeFrame.visible = false;
            SMNormalFrame.Skeleton.SetColor(new Color(1, 1, 1, 0));

            gFreeTimeBox = contentPane.GetChild("freeTimeBox").asCom;
            gFreeWinBox = contentPane.GetChild("freeWinBox").asCom;
            gFreeSlotMachine = contentPane.GetChild("freeSlotMachine").asCom;
            gFreeTimeBox.visible = false;
            gFreeWinBox.visible = false;
            gFreeSlotMachine.visible = false;

            GComponent localPagTestAnchor = contentPane.GetChild("anchorPagTest").asCom;
            if (_anchorPagTest != localPagTestAnchor)
            {
                // anchor 变更（语言切换等）：重建 GoWrapper 并清空旧 PagSlotBinding
                GameCommon.FguiUtils.DeleteWrapper(_anchorPagTest);
                _clonePagTest = GameObject.Instantiate(_goPagTestPrefab);
                _anchorPagTest = localPagTestAnchor;
                GameCommon.FguiUtils.AddWrapper(_anchorPagTest, _clonePagTest);
                // _pagTestAttachBone = null;
                DisposeAllPagTestSlotBindings();
                ResetPagTestSpineRefs();
                DisposeBorderMegaWinEffects();
                EnsurePagTestSpines();
            }

            // if (_pagTestAttachBone == null)
            // {
            //     _pagTestAttachBone = FindPagTestAttachBone(_clonePagTest, "c_circle");
            //     AttachJpMajorToPagTestBone();
            // }

            EnsurePagTestSlots();   // 绑定 pagEffect1~6 到 PagSlotBinding
            EnsurePagTestSpines();  // 初始化 Spine 对照节点
            BindPagTestButtons();   // PAG1~12、Spine1~5 测试按钮
            EnsureBorderMegaWinEffects();
            BindBorderMegaWinButtons();
            BindComboPlayControls();

            uiJPMajorCtrl.Init("Major", contentPane.GetChild("jpMajor").asCom.GetChild("reels").asList, "N0");
            uiJPMinorCtrl.Init("Minor", contentPane.GetChild("jpMinor").asCom.GetChild("reels").asList, "N0");
            uiJPMiniCtrl.Init("Mini", contentPane.GetChild("jpMini").asCom.GetChild("reels").asList, "N0");

            uiJPMajorCtrl.SetData(0);
            uiJPMinorCtrl.SetData(0);
            uiJPMiniCtrl.SetData(0);
            ChangeBGPanel(0);
            ContentModel.Instance.totalBet = SBoxModel.Instance.betList[ContentModel.Instance.betIndex];

            if (!isOpen) return;

            MachineDataManager02.Instance.RequestGetPlayerInfo((res) =>
            {
                SBoxAccount account = (SBoxAccount)res;
                int pid = SBoxModel.Instance.pid;
                List<SBoxPlayerAccount> playerAccountList = account.PlayerAccountList;
                for (int i = 0; i < playerAccountList.Count; i++)
                {
                    if (playerAccountList[i].PlayerId == pid)
                    {
                        MainBlackboardController.Instance.SetMyRealCredit(playerAccountList[i].Credit);
                        break;
                    }
                }
            }, (BagelCodeError err) =>
            {
                DebugUtils.Log(err.msg);
            });
            MainBlackboardController.Instance.SyncMyTempCreditToReal(true);

            TryRestoreFreeSpinSession();
        }

        /// <summary>
        /// 绑定 PageGameMain 上 PAG1~12、Spine1~5 测试按钮（InitParam / 语言切换后）
        /// </summary>
        private void BindPagTestButtons()
        {
            if (_pagTestButtonsBound || contentPane == null)
            {
                return;
            }

            for (int pagIndex = 0; pagIndex < PagTestPagButtonCount; pagIndex++)
            {
                BindPagTestButton($"PAG{pagIndex + 1}", pagIndex);
            }

            GButton btnSpine1 = contentPane.GetChild("Spine1")?.asButton;
            if (btnSpine1 != null)
            {
                btnSpine1.onClick.Clear();
                btnSpine1.onClick.Add(OnClickSpineTest1Button);
            }
            else
            {
                Debug.LogWarning($"{PagLogPrefix} button missing: Spine1");
            }

            GButton btnSpine2 = contentPane.GetChild("Spine2")?.asButton;
            if (btnSpine2 != null)
            {
                btnSpine2.onClick.Clear();
                btnSpine2.onClick.Add(OnClickSpineTest2Button);
            }
            else
            {
                Debug.LogWarning($"{PagLogPrefix} button missing: Spine2");
            }

            BindPagTestSpineButton("Spine3", OnClickSpineTest3Button);
            BindPagTestSpineButton("Spine4", OnClickSpineTest4Button);
            BindPagTestSpineButton("Spine5", OnClickSpineTest5Button);

            _pagTestButtonsBound = true;
        }

        /// <summary>
        /// 校验 PAG 按钮下标是否有效
        /// </summary>
        /// <param name="pagIndex"> PAG 按钮下标（0~11）</param>
        private bool IsValidPagTestButtonIndex(int pagIndex)
        {
            return pagIndex >= 1 && pagIndex <= PagTestPagButtonCount;
        }

        /// <summary>
        /// 按 PAG 按钮下标获取播放配置
        /// </summary>
        /// <param name="pagIndex"> PAG 按钮下标（0~11）</param>
        /// <param name="config"> 输出播放配置</param>
        private bool TryGetPagTestPlaybackConfig(int pagIndex, out PagTestPlaybackConfig config)
        {
            config = default;
            if (!IsValidPagTestButtonIndex(pagIndex))
            {
                return false;
            }

            switch (pagIndex)
            {
                case 1:
                    config = new PagTestPlaybackConfig
                    {
                        Kind = PagTestPlaybackKind.SingleLoop,
                        PagFile = PagTestName2,
                        DisplayScale = PagTestDisplayScale,
                        LazyBindSlot = false,
                    };
                    return true;
                case 2:
                    config = new PagTestPlaybackConfig
                    {
                        Kind = PagTestPlaybackKind.SingleLoop,
                        PagFile = PagTestName1,
                        DisplayScale = PagTestDisplayScale,
                        LazyBindSlot = false,
                    };
                    return true;
                case 3:
                    config = new PagTestPlaybackConfig
                    {
                        Kind = PagTestPlaybackKind.SingleLoop,
                        PagFile = PagTestName3,
                        DisplayScale = PagTestDisplayScale,
                        LazyBindSlot = false,
                    };
                    return true;
                case 4:
                    config = new PagTestPlaybackConfig
                    {
                        Kind = PagTestPlaybackKind.SingleLoop,
                        PagFile = PagGlowLoop720,
                        DisplayScale = PagTestDisplayScale,
                        LazyBindSlot = true,
                    };
                    return true;
                case 5:
                    config = new PagTestPlaybackConfig
                    {
                        Kind = PagTestPlaybackKind.SingleLoop,
                        PagFile = PagGlowLoop720,
                        DisplayScale = PagGlow720DisplayScale,
                        LazyBindSlot = true,
                    };
                    return true;
                case 6:
                    config = new PagTestPlaybackConfig
                    {
                        Kind = PagTestPlaybackKind.SingleLoop,
                        PagFile = PagGlowLoopHalf,
                        DisplayScale = PagTestDisplayScale,
                        LazyBindSlot = true,
                    };
                    return true;
                case 7:
                    config = new PagTestPlaybackConfig
                    {
                        Kind = PagTestPlaybackKind.SingleLoop,
                        PagFile = PagGlowLoopFull,
                        DisplayScale = PagTestDisplayScale,
                        LazyBindSlot = true,
                    };
                    return true;
                case 8:
                    config = new PagTestPlaybackConfig
                    {
                        Kind = PagTestPlaybackKind.IntroLoop,
                        IntroFile = PagGlowInFull,
                        LoopFile = PagGlowLoopFull,
                        DisplayScale = PagTestDisplayScale,
                        LazyBindSlot = true,
                    };
                    return true;
                case 9:
                    config = new PagTestPlaybackConfig
                    {
                        Kind = PagTestPlaybackKind.Sequence,
                        Sequence = NpcBigWinSequence,
                        DisplayScale = PagTestDisplayScale,
                        Label = "BigWin",
                    };
                    return true;
                case 10:
                    config = new PagTestPlaybackConfig
                    {
                        Kind = PagTestPlaybackKind.Sequence,
                        Sequence = NpcFreeSequence,
                        DisplayScale = PagTestDisplayScale,
                        LazyBindSlot = true,
                        Label = PagTestNpcLabels[0],
                    };
                    return true;
                case 11:
                    config = new PagTestPlaybackConfig
                    {
                        Kind = PagTestPlaybackKind.Sequence,
                        Sequence = NpcNormalSequence,
                        DisplayScale = PagTestDisplayScale,
                        LazyBindSlot = true,
                        Label = PagTestNpcLabels[1],
                    };
                    return true;
                case 12:
                    config = new PagTestPlaybackConfig
                    {
                        Kind = PagTestPlaybackKind.Sequence,
                        Sequence = NpcRewardSequence,
                        DisplayScale = PagTestDisplayScale,
                        LazyBindSlot = true,
                        Label = PagTestNpcLabels[2],
                    };
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 按 loader 下标获取 PagSlotBinding
        /// </summary>
        /// <param name="loaderIndex"> pagEffect 数组下标（0~11）</param>
        /// <returns> 槽位绑定；无效下标时返回 null</returns>
        private PagSlotBinding GetPagTestSlotByPagIndex(int pagIndex)
        {
            return pagIndex >= 0 && pagIndex < PagTestLoaderCount ? _pagTestSlotBindings[pagIndex] : null;
        }

        /// <summary>
        /// 查询 PAG 是否处于播放中状态
        /// </summary>
        /// <param name="pagIndex"> PAG 按钮下标（0~11）</param>
        /// <returns> 正在播放时返回 true</returns>
        private bool IsPagTestShowing(int pagIndex)
        {
            return pagIndex >= 0 && pagIndex < PagTestPagButtonCount && _pagTestShowing[pagIndex];
        }

        /// <summary>
        /// 设置 PAG Showing 显示状态
        /// </summary>
        /// <param name="pagIndex"> PAG 按钮下标（0~11）</param>
        /// <param name="showing"> 是否正在播放/显示</param>
        private void SetPagTestShowing(int pagIndex, bool showing)
        {
            if (pagIndex >= 0 && pagIndex < PagTestPagButtonCount)
            {
                _pagTestShowing[pagIndex] = showing;
            }
        }

        /// <summary>
        /// 查询 PAG composition 是否已预热
        /// </summary>
        /// <param name="pagIndex"> PAG 按钮下标（0~11）</param>
        /// <returns> 已预热时返回 true</returns>
        private bool GetPagTestCacheWarmed(int pagIndex)
        {
            return pagIndex >= 0 && pagIndex < PagTestPagButtonCount && _pagTestCacheWarmed[pagIndex];
        }

        /// <summary>
        /// 设置 PAG composition 预热状态
        /// </summary>
        /// <param name="pagIndex"> PAG 按钮下标（0~11）</param>
        /// <param name="warmed"> 是否已预热</param>
        private void SetPagTestCacheWarmed(int pagIndex, bool warmed)
        {
            if (pagIndex >= 0 && pagIndex < PagTestPagButtonCount)
            {
                _pagTestCacheWarmed[pagIndex] = warmed;
            }
        }

        /// <summary>
        /// 获取 PAG 按钮当前持有的播放协程
        /// </summary>
        /// <param name="pagIndex"> PAG 按钮下标（0~11）</param>
        private Coroutine GetPagTestCoroutine(int pagIndex)
        {
            return pagIndex >= 0 && pagIndex < PagTestPagButtonCount ? _corPagTest[pagIndex] : null;
        }

        /// <summary>
        /// 设置 PAG 按钮当前持有的播放协程
        /// </summary>
        /// <param name="pagIndex"> PAG 按钮下标（0~11）</param>
        /// <param name="coroutine"> 协程引用</param>
        private void SetPagTestCoroutine(int pagIndex, Coroutine coroutine)
        {
            if (pagIndex >= 0 && pagIndex < PagTestPagButtonCount)
            {
                _corPagTest[pagIndex] = coroutine;
            }
        }

        /// <summary>
        /// 释放全部 PagSlotBinding 并清空引用
        /// </summary>
        private void DisposeAllPagTestSlotBindings()
        {
            for (int i = 0; i < PagTestLoaderCount; i++)
            {
                _pagTestSlotBindings[i]?.Dispose();
                _pagTestSlotBindings[i] = null;
            }
        }

        /// <summary>
        /// 重置 Spine 对照节点引用与显示状态
        /// </summary>
        private void ResetPagTestSpineRefs()
        {
            for (int i = 0; i < PagTestSpineCount; i++)
            {
                _pagTestSpineAnimators[i] = null;
                _pagTestSpineMecanims[i] = null;
                _spineTestShowing[i] = false;
            }
        }

        /// <summary>
        /// 重置 PAG 播放与预热状态数组
        /// </summary>
        private void ResetPagTestPlaybackState()
        {
            for (int i = 0; i < PagTestPagButtonCount; i++)
            {
                _pagTestShowing[i] = false;
                _pagTestCacheWarmed[i] = false;
            }
        }

        /// <summary>
        /// 创建并 Attach 指定 loader 的 PagSlotBinding
        /// </summary>
        /// <param name="loaderIndex"> pagEffect 数组下标（0~11）</param>
        private void EnsurePagTestSlotBinding(int loaderIndex)
        {
            if (loaderIndex < 0 || loaderIndex > PagTestLoaderCount)
            {
                return;
            }

            if (_pagTestSlotBindings[loaderIndex] == null)
            {
                _pagTestSlotBindings[loaderIndex] = new PagSlotBinding(PagTestSlotInstanceLabels[loaderIndex], GamePagFolder);
                Debug.Log($"{PagLogPrefix} PagSlotBinding created for {PagTestSlotInstanceLabels[loaderIndex]}");
            }

            GComponent anchor = GetPagTestAnchor();
            if (anchor == null)
            {
                Debug.LogWarning($"{PagLogPrefix} EnsurePagTestSlot skipped: anchor null, instance={PagTestSlotInstanceLabels[loaderIndex]}");
                return;
            }

            if (_pagTestSlotBindings[loaderIndex] == null)
            {
                Debug.LogWarning($"{PagLogPrefix} EnsurePagTestSlot skipped: slot null, instance={PagTestSlotInstanceLabels[loaderIndex]}");
                return;
            }

            _pagTestSlotBindings[loaderIndex].Attach(anchor, PagTestLoaderNames[loaderIndex]);
        }

        /// <summary>
        /// PAG1~12 统一点击入口：正在播则停，否则开
        /// </summary>
        /// <param name="pagIndex"> PAG 按钮下标（0~11）</param>
        private void TogglePagTestPlayback(int pagIndex)
        {
            if (!TryGetPagTestPlaybackConfig(pagIndex, out PagTestPlaybackConfig config))
            {
                return;
            }

            if (IsPagTestShowing(pagIndex))
            {
                Debug.Log($"{PagLogPrefix} PAG{pagIndex} clicked, stop {GetPagTestPlaybackLogLabel(config)}");
                StopPagTestPlayback(pagIndex);
                return;
            }

            Debug.Log($"{PagLogPrefix} PAG{pagIndex} clicked, play {GetPagTestPlaybackLogLabel(config)}");
            StartPagTestPlayback(pagIndex);
        }

        /// <summary>
        /// 启动 PAG1~12 播放（Combo 与按钮共用）
        /// </summary>
        /// <param name="pagIndex"> PAG 按钮下标（0~11）</param>
        private bool StartPagTestPlayback(int pagIndex)
        {
            if (!TryGetPagTestPlaybackConfig(pagIndex, out PagTestPlaybackConfig config))
            {
                Debug.LogWarning($"{PagLogPrefix} StartPagTestPlayback unsupported pagIndex={pagIndex}");
                return false;
            }

            if (mono == null)
            {
                Debug.LogWarning($"{PagLogPrefix} PAG{pagIndex} play skipped: mono is null");
                return false;
            }

            StopPagTestPlaybackCoroutineOnly(pagIndex); //停协程
            StopPagTest(pagIndex);//停pag

            SetPagTestShowing(pagIndex, true);

            SetPagTestCoroutine(pagIndex, mono.StartCoroutine(RunPagTestPlaybackCoroutine(pagIndex)));

            return true;
        }

        /// <summary>
        /// 停止 PAG1~12 播放（OnClose / Combo / 按钮二次点击）
        /// </summary>
        /// <param name="pagIndex"> PAG 按钮下标（0~11）</param>
        /// <param name="stopComboIfActive"> true 时若该路在 Combo 中会整组停止</param>
        private void StopPagTestPlayback(int pagIndex, bool stopComboIfActive = true)
        {
            if (!IsValidPagTestButtonIndex(pagIndex))
            {
                return;
            }

            if (stopComboIfActive && _comboPlayActive && _comboActivePagIndex == pagIndex)
            {
                StopComboPlayback();
                return;
            }

            SetPagTestShowing(pagIndex, false);
            StopPagTestPlaybackCoroutineOnly(pagIndex);
            StopPagTest(pagIndex);
        }

        /// <summary>
        /// 取消该 PAG 按钮对应的 Unity 协程并清空 _corPagTest 引用
        /// </summary>
        /// <param name="pagIndex"> PAG 按钮下标（0~11）</param>
        private void StopPagTestPlaybackCoroutineOnly(int pagIndex)
        {
            Coroutine coroutine = GetPagTestCoroutine(pagIndex);
            if (coroutine != null && mono != null)
            {
                mono.StopCoroutine(coroutine);
                SetPagTestCoroutine(pagIndex, null);
            }
        }

        /// <summary>
        /// 按播放配置种类分发 PAG 播放协程
        /// </summary>
        /// <param name="pagIndex"> PAG 按钮下标（1~12）</param>
        private IEnumerator RunPagTestPlaybackCoroutine(int pagIndex)
        {
            if (!TryGetPagTestPlaybackConfig(pagIndex, out PagTestPlaybackConfig config))
            {
                yield break;
            }

            switch (config.Kind)
            {
                case PagTestPlaybackKind.SingleLoop:
                    yield return StartPagTestSingleLoopCoroutine(pagIndex, config);
                    break;
                case PagTestPlaybackKind.Sequence:
                    yield return StartPagTestNpcSequencePlayback(pagIndex, config);
                    break;
                case PagTestPlaybackKind.IntroLoop:
                    yield return StartPagTestIntroLoopCoroutine(pagIndex, config);
                    break;
            }
        }

        /// <summary>
        /// 生成 PAG 播放日志标签
        /// </summary>
        /// <param name="config"> 播放配置</param>
        private string GetPagTestPlaybackLogLabel(PagTestPlaybackConfig config)
        {
            switch (config.Kind)
            {
                case PagTestPlaybackKind.SingleLoop:
                    return config.PagFile;
                case PagTestPlaybackKind.IntroLoop:
                    return $"{config.IntroFile} -> {config.LoopFile}";
                case PagTestPlaybackKind.Sequence:
                    return config.Label ?? "npc";
                default:
                    return "unknown";
            }
        }

        /// <summary>
        /// 绑定 PAG 测试按钮点击事件
        /// </summary>
        /// <param name="buttonName"> FGUI 按钮名</param>
        /// <param name="pagIndex"> PAG 按钮下标（0~11）</param>
        private void BindPagTestButton(string buttonName, int pagIndex)
        {
            GButton btn = contentPane.GetChild(buttonName)?.asButton;
            if (btn != null)
            {
                btn.onClick.Clear();
                btn.onClick.Add(() => TogglePagTestPlayback(pagIndex));
            }
            else
            {
                Debug.LogWarning($"{PagLogPrefix} button missing: {buttonName}");
            }
        }

        /// <summary>
        /// InitParam 时创建全部 PagSlotBinding 并 Attach 到 anchorPagTest 下 pagEffect1~12
        /// </summary>
        private void EnsurePagTestSlots()
        {
            PagConcurrentPlayback.Enabled = PagTestUseFguiTexture;
            PagController.AutoConcurrentGpuSync = PagTestUseGpuSyncGroup;

            if (GetPagTestAnchor() == null)
            {
                Debug.LogWarning($"{PagLogPrefix} EnsurePagTestSlots skipped: anchor null");
                return;
            }

            for (int i = 0; i < PagTestLoaderCount; i++)
            {
                EnsurePagTestSlotBinding(i);
            }
        }

        /// <summary>
        /// OnClose 时释放 PagSlotBinding 与 Spine 引用，重置播放/预热状态
        /// </summary>
        private void DisposePagTestResources()
        {
            DisposeAllPagTestSlotBindings();
            ResetPagTestSpineRefs();
            ResetPagTestPlaybackState();
            StopComboPlayback();
        }

        /// <summary>
        /// OnClose 时停止全部 PAG1~12 播放
        /// </summary>
        private void StopAllPagTest()
        {
            for (int pagIndex = 1; pagIndex <= PagTestPagButtonCount; pagIndex++)
            {
                StopPagTestPlayback(pagIndex, stopComboIfActive: false);
            }
        }

        private static readonly PagSlotBinding[] PagTestGroupTripleSlots = new PagSlotBinding[3];
        /// <summary>
        /// 获取 anchorPagTest FGUI 锚点
        /// </summary>
        private GComponent GetPagTestAnchor()
        {
            if (_anchorPagTest != null)
            {
                return _anchorPagTest;
            }

            return contentPane?.GetChild("anchorPagTest")?.asCom;
        }

        /// <summary>
        /// /// 将 anchorPagTest 区域换算为 Native overlay 的 extra（x,y,w,h 为相对屏幕 0~1）。 ///
        /// </summary>
        /// <param name="extra"> 输出 layout extra 字符串</param>
        /// <param name="debugReason"> 输出调试原因</param>
        private bool TryBuildPagTestLayoutExtra(out string extra, out string debugReason)
        {
            extra = null;
            debugReason = "unknown";

            GComponent anchor = GetPagTestAnchor();
            if (anchor == null)
            {
                debugReason = "anchorPagTest is null";
                return false;
            }

            GGraph holder = anchor.GetChild("holder")?.asGraph;
            GLoader example = anchor.GetChild("example")?.asLoader;

            float localW = holder != null && holder.width > 0f ? holder.width : (example != null ? example.width : 200f);
            float localH = holder != null && holder.height > 0f ? holder.height : (example != null ? example.height : 200f);
            if (localW <= 0f || localH <= 0f)
            {
                debugReason = $"invalid size holder={holder?.width}x{holder?.height} example={example?.width}x{example?.height}";
                return false;
            }

            float rootW = GRoot.inst.width;
            float rootH = GRoot.inst.height;
            if (rootW <= 0f || rootH <= 0f)
            {
                debugReason = $"invalid GRoot size {rootW}x{rootH}";
                return false;
            }

            float normW = Screen.width > 0f ? Screen.width : rootW;
            float normH = Screen.height > 0f ? Screen.height : rootH;

            GObject layoutTarget = holder != null && holder.width > 0f ? (GObject)holder : anchor;
            Rect globalRect = layoutTarget.LocalToGlobal(new Rect(0f, 0f, localW, localH));
            float x = Mathf.Clamp01(globalRect.xMin / normW);
            float y = Mathf.Clamp01(globalRect.yMin / normH);
            float w = Mathf.Clamp(globalRect.width / normW, 0.01f, 1f - x);
            float h = Mathf.Clamp(globalRect.height / normH, 0.01f, 1f - y);

            if (w * h < 0.01f)
            {
                debugReason = $"rect too small w={w:F4} h={h:F4}, use turntable fallback";
                return false;
            }

            extra = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "{0:F4},{1:F4},{2:F4},{3:F4}", x, y, w, h);
            debugReason = $"ok target={layoutTarget.name} global={globalRect} Screen={normW}x{normH} GRoot={rootW}x{rootH}";
            return true;
        }

        /// <summary>
        /// 检查 PagCache 磁盘文件与 Java composition 缓存是否均已就绪
        /// </summary>
        /// <param name="pagFileName"> PAG 文件名</param>
        private static bool IsPagCompositionReady(string pagFileName)
        {
            if (!PagPathHelper.IsCached(pagFileName, GamePagFolder))
            {
                return false;
            }

            string absPath = PagController.ResolvePagPath(pagFileName, GamePagFolder);
            return PagController.IsCompositionCached(absPath);
        }

        /// <summary>
        /// 停止单槽 PAG 播放并清除 showing 标志
        /// </summary>
        /// <param name="pagIndex"> PAG 按钮下标（0~11）</param>
        private void StopPagTest(int pagIndex)
        {
            PagSlotBinding slot = GetPagTestSlotByPagIndex(pagIndex);
            if (slot?.Controller == null)
            {
                Debug.LogWarning($"{PagLogPrefix} StopPagTest skipped: PagController is null, pagIndex={pagIndex}, instance={slot?.InstanceKey}");
                SetPagTestShowing(pagIndex, false);
                return;
            }

            slot.Stop(PagTestUseFguiTexture);
            SetPagTestShowing(pagIndex, false);

            Debug.Log($"{PagLogPrefix} StopPagTest pagIndex={pagIndex} instance={slot.InstanceKey}");
        }

        /// <summary>
        /// 轮询直到 PagController.PlayStarted 或超时
        /// </summary>
        /// <param name="slot"> PagSlotBinding 槽位</param>
        /// <param name="timeoutSec"> 超时时间（秒）</param>
        private IEnumerator WaitPagTestPlayStarted(PagSlotBinding slot, float timeoutSec)
        {
            EnsurePagTestSlots();
            PagController controller = slot?.Controller;
            if (controller == null)
            {
                yield break;
            }

            float deadline = Time.unscaledTime + timeoutSec;
            while (!controller.PlayStarted && Time.unscaledTime < deadline)
            {
                yield return null;
            }

            if (controller.PlayStarted)
            {
                Debug.Log($"{PagLogPrefix} Pag play started (within {timeoutSec}s), instance={slot.InstanceKey}");
            }
            else
            {
                Debug.LogWarning($"{PagLogPrefix} Pag play started timeout ({timeoutSec}s), instance={slot.InstanceKey}");
            }
        }

        /// <summary>
        /// 预热缓存后单次 Play + repeat=-1，由纹理模式 Native 路径无缝循环
        /// </summary>
        /// <param name="pagIndex"> PAG 按钮下标（0~11）</param>
        /// <param name="config"> 播放配置</param>
        private IEnumerator StartPagTestSingleLoopCoroutine(int pagIndex, PagTestPlaybackConfig config)
        {
            bool cacheWarmed = GetPagTestCacheWarmed(pagIndex);

            if (!IsPagTestShowing(pagIndex))
            {
                SetPagTestCoroutine(pagIndex, null);
                yield break;
            }

            PagSlotBinding slot = GetPagTestSlotByPagIndex(pagIndex);
            PlayPagTest(slot, config.PagFile, -1, config.DisplayScale);
            if (!Mathf.Approximately(config.DisplayScale, PagTestDisplayScale))
            {
                slot?.Controller?.SyncFguiDisplayLayoutFromComposition();
            }

            SetPagTestCoroutine(pagIndex, null);
            Debug.Log($"{PagLogPrefix} PAG{pagIndex}: native loop repeat=-1, {config.PagFile}, scale={config.DisplayScale}");
        }

        /// <summary>
        /// /// 单槽 PAG 播放入口（调用前须已通过 EnsurePagTestSlots 完成 Attach）： /// 解析路径 → 计算 Overlay layoutExtra → FGUI 或 Overlay 分支 → PlayPag。 ///
        /// </summary>
        /// <param name="slot"> PagSlotBinding 槽位</param>
        /// <param name="pagFileName"> PAG 文件名</param>
        /// <param name="repeatCount"> 重复次数；-1 = Native 无限循环</param>
        /// <param name="displayScale">FGUI 显示倍率（相对合成尺寸）</param>
        private void PlayPagTest(PagSlotBinding slot, string pagFileName, int repeatCount = 1, float displayScale = PagTestDisplayScale)
        {
            //控制器校验
            Debug.Log($"{PagLogPrefix} PlayPagTest start: instance={slot?.InstanceKey}, {pagFileName}, repeat={repeatCount}, scale={displayScale}");
            PagController controller = slot?.Controller;
            if (controller == null)
            {
                Debug.LogError($"{PagLogPrefix} PlayPagTest failed: PagController is null, instance={slot?.InstanceKey}");
                return;
            }
            //路径校验
            string resolvedPath = controller.ResolvePagPath(pagFileName);
            if (string.IsNullOrEmpty(resolvedPath))
            {
                Debug.LogError($"{PagLogPrefix} PlayPagTest failed: resolve path null, file={pagFileName}, instance={slot.InstanceKey}");
                return;
            }

            string positionType = "center";
            string layoutExtra = "";
            // Overlay 布局：全屏调试 / holder 换算 extra / turntable 自动布局回退
            if (PagTestDebugFullScreen)
            {
                positionType = "full";
                layoutExtra = "";
                Debug.Log($"{PagLogPrefix} debug fullscreen mode, skip layout extra");
            }
            else if (TryBuildPagTestLayoutExtra(out layoutExtra, out string layoutDebug))
            {
                Debug.Log($"{PagLogPrefix} layout extra: {layoutExtra} ({layoutDebug})");
            }
            else
            {
                Debug.LogWarning($"{PagLogPrefix} layout extra unavailable ({layoutDebug}), fallback LayoutPagAuto(turntable)");
                controller.LayoutPagAuto("turntable");
            }

            // 渲染目标：FGUI ExternalTexture（默认）或 Native Overlay + 可选软件出帧回退
            if (PagTestUseFguiTexture)
            {
                slot.SetFguiDisplayScale(displayScale);
                slot.SetFguiClampDisplayToHolder(PagTestClampDisplayToHolder);

                if (!slot.PreparePlay(true, PagTestFguiMaxDisplaySide, PagTestFguiFps))
                {
                    Debug.LogError($"{PagLogPrefix} PlayPagTest failed: FGUI slot not ready, pag={pagFileName}, instance={slot.InstanceKey}");
                    return;
                }

                Debug.Log($"{PagLogPrefix} FGUI frame config: maxSide={PagTestFguiMaxDisplaySide} fps={PagTestFguiFps} "
                    + $"displayScale={displayScale} pag={pagFileName} instance={slot.InstanceKey}");
            }
            else
            {
                slot.PreparePlay(false, 0, 0);
                controller.SetForceBitmapOverlayFallback(PagTestOverlayFallback);
            }

            //设置重复次数并播放
            controller.SetRepeatCount(repeatCount);
            bool playOk = controller.PlayPag(pagFileName, positionType, layoutExtra);
            if (playOk)
            {
                Debug.Log($"{PagLogPrefix} PlayPagTest success: {pagFileName}, instance={slot.InstanceKey}");
            }
            else
            {
                Debug.LogError($"{PagLogPrefix} PlayPagTest failed: {pagFileName}, instance={slot.InstanceKey}");
            }
        }

        /// <summary>
        /// PAG9：intro + loop 两段链
        /// </summary>
        /// <param name="pagIndex"> PAG 按钮下标（0~11）</param>
        /// <param name="config"> 播放配置</param>
        private IEnumerator StartPagTestIntroLoopCoroutine(int pagIndex, PagTestPlaybackConfig config)
        {
            //预加载校验
            bool cacheWarmed = GetPagTestCacheWarmed(pagIndex);
            if (!cacheWarmed || !IsPagCompositionReady(config.IntroFile))
            {
                yield return PagController.PreloadCompositionCoroutine(config.IntroFile, GamePagFolder);
            }

            if (!cacheWarmed || !IsPagCompositionReady(config.LoopFile))
            {
                yield return PagController.PreloadCompositionCoroutine(config.LoopFile, GamePagFolder);
            }
            SetPagTestCacheWarmed(pagIndex,IsPagCompositionReady(config.IntroFile) && IsPagCompositionReady(config.LoopFile));

            if (!IsPagTestShowing(pagIndex))
            {
                SetPagTestCoroutine(pagIndex, null);
                yield break;
            }

            PagSlotBinding slot = GetPagTestSlotByPagIndex(pagIndex);
            PagController controller = slot?.Controller;
            if (controller == null)
            {
                Debug.LogError($"{PagLogPrefix} PAG{pagIndex}: controller missing");
                StopPagTestPlayback(pagIndex, stopComboIfActive: false);
                yield break;
            }

            string positionType = "center";
            string layoutExtra = string.Empty;
            if (PagTestDebugFullScreen)
            {
                positionType = "full";
            }
            else if (TryBuildPagTestLayoutExtra(out layoutExtra, out string layoutDebug))
            {
                Debug.Log($"{PagLogPrefix} PAG{pagIndex} layout extra: {layoutExtra} ({layoutDebug})");
            }
            else
            {
                Debug.LogWarning($"{PagLogPrefix} PAG{pagIndex} layout fallback turntable");
                controller.LayoutPagAuto("turntable");
            }

            slot.SetFguiDisplayScale(config.DisplayScale);
            slot.SetFguiClampDisplayToHolder(PagTestClampDisplayToHolder);
            if (!slot.PreparePlay(true, PagTestFguiMaxDisplaySide, PagTestFguiFps))
            {
                Debug.LogError($"{PagLogPrefix} PAG{pagIndex} PreparePlay failed");
                StopPagTestPlayback(pagIndex, stopComboIfActive: false);
                yield break;
            }

            PagSegment[] segments =
            {
                new PagSegment(config.IntroFile, 1),
                new PagSegment(config.LoopFile, -1),
            };
            if (!controller.PlayFguiGpuSequence(segments, positionType, layoutExtra, PagTestUseGpuSyncGroup))
            {
                Debug.LogError($"{PagLogPrefix} PAG{pagIndex} PlayFguiGpuSequence failed");
                StopPagTestPlayback(pagIndex, stopComboIfActive: false);
                yield break;
            }

            yield return WaitPagTestPlayStarted(slot, PagTestPlayStartedTimeoutSec);
            controller = slot?.Controller;
            if (controller == null || !controller.PlayStarted)
            {
                Debug.LogError($"{PagLogPrefix} PAG{pagIndex} sequence did not start within {PagTestPlayStartedTimeoutSec}s");
                StopPagTestPlayback(pagIndex, stopComboIfActive: false);
                yield break;
            }

            SetPagTestCoroutine(pagIndex, null);
            Debug.Log($"{PagLogPrefix} PAG{pagIndex}: intro->loop sequence started, scale={config.DisplayScale}");
        }

        /// <summary>
        /// PAG9~12 NPC 序列播放协程
        /// </summary>
        /// <param name="pagIndex"> PAG 按钮下标（0~11）</param>
        /// <param name="config"> 播放配置</param>
        private IEnumerator StartPagTestNpcSequencePlayback(int pagIndex, PagTestPlaybackConfig config)
        {
            string[] sequence = config.Sequence;
            string label = config.Label;
            if (sequence == null || sequence.Length == 0)
            {
                yield break;
            }

            if (!GetPagTestCacheWarmed(pagIndex))
            {
                for (int i = 0; i < sequence.Length; i++)
                {
                    yield return PagController.PreloadCompositionCoroutine(sequence[i], GamePagFolder);
                }

                SetPagTestCacheWarmed(pagIndex, true);
            }

            if (!IsPagTestShowing(pagIndex))
            {
                SetPagTestCoroutine(pagIndex, null);
                yield break;
            }

            PagSlotBinding slot = GetPagTestSlotByPagIndex(pagIndex);
            PagController controller = slot?.Controller;
            if (controller == null)
            {
                Debug.LogError($"{PagLogPrefix} PAG{pagIndex} controller missing: {label}");
                StopPagTestPlayback(pagIndex, stopComboIfActive: false);
                yield break;
            }

            string positionType = "center";
            string layoutExtra = string.Empty;
            if (PagTestDebugFullScreen)
            {
                positionType = "full";
            }
            else if (TryBuildPagTestLayoutExtra(out layoutExtra, out string layoutDebug))
            {
                Debug.Log($"{PagLogPrefix} PAG{pagIndex} layout extra: {layoutExtra} ({layoutDebug})");
            }
            else
            {
                Debug.LogWarning($"{PagLogPrefix} PAG{pagIndex} layout fallback turntable");
                controller.LayoutPagAuto("turntable");
            }

            slot.SetFguiDisplayScale(PagTestDisplayScale);
            slot.SetFguiClampDisplayToHolder(PagTestClampDisplayToHolder);
            if (!slot.PreparePlay(true, PagTestFguiMaxDisplaySide, PagTestFguiFps))
            {
                Debug.LogError($"{PagLogPrefix} PAG{pagIndex} PreparePlay failed: {label}");
                StopPagTestPlayback(pagIndex, stopComboIfActive: false);
                yield break;
            }

            PagSegment[] segments = BuildPagTestNpcSegments(sequence);
            if (!controller.PlayFguiGpuSequence(segments, positionType, layoutExtra, PagTestUseGpuSyncGroup))
            {
                Debug.LogError($"{PagLogPrefix} PAG{pagIndex} PlayFguiGpuSequence failed: {label}");
                StopPagTestPlayback(pagIndex, stopComboIfActive: false);
                yield break;
            }

            yield return WaitPagTestPlayStarted(slot, PagTestPlayStartedTimeoutSec);
            controller = slot?.Controller;
            if (controller == null || !controller.PlayStarted)
            {
                Debug.LogError($"{PagLogPrefix} PAG{pagIndex} sequence did not start within {PagTestPlayStartedTimeoutSec}s: {label}");
                StopPagTestPlayback(pagIndex, stopComboIfActive: false);
                yield break;
            }

            float totalTimeout = 0f;
            for (int i = 0; i < sequence.Length; i++)
            {
                totalTimeout += controller.GetCompositionDurationSecWithFallback(PagTestNpcSegmentDurationFallbackSec) + 1f;
            }

            totalTimeout += 3f;
            totalTimeout = Mathf.Max(totalTimeout, sequence.Length * PagTestNpcSegmentDurationFallbackSec + 5f);
            yield return controller.WaitForFguiGpuSequenceFinished(totalTimeout);

            SetPagTestShowing(pagIndex, false);
            slot?.Stop(PagTestUseFguiTexture);
            SetPagTestCoroutine(pagIndex, null);
            Debug.Log($"{PagLogPrefix} PAG{pagIndex} npc sequence finished: {label}");
        }

        /// <summary>
        /// 将 NPC PAG 文件名数组转为 PagSegment 列表
        /// </summary>
        /// <param name="sequence"> PAG 文件序列</param>
        /// <returns> PAG 分段数组</returns>
        private static PagSegment[] BuildPagTestNpcSegments(string[] sequence)
        {
            var segments = new PagSegment[sequence.Length];
            for (int i = 0; i < sequence.Length; i++)
            {
                segments[i] = new PagSegment(sequence[i], 1);
            }

            return segments;
        }

        /// <summary>
        /// 绑定单个 Spine 测试按钮的 onClick
        /// </summary>
        /// <param name="buttonName"> FGUI 按钮名</param>
        /// <param name="handler"> 点击回调</param>
        private void BindPagTestSpineButton(string buttonName, EventCallback0 handler)
        {
            GButton btn = contentPane.GetChild(buttonName)?.asButton;
            if (btn != null)
            {
                btn.onClick.Clear();
                btn.onClick.Add(handler);
            }
            else
            {
                Debug.LogWarning($"{PagLogPrefix} button missing: {buttonName}");
            }
        }

        /// <summary>
        /// OnClose / OnLanguageChange 前清除 PAG 与 Spine 测试按钮点击监听
        /// </summary>
        private void ClearPagTestButtons()
        {
            if (!_pagTestButtonsBound || contentPane == null)
            {
                return;
            }

            contentPane.GetChild("PAG1")?.asButton?.onClick.Clear();
            contentPane.GetChild("PAG2")?.asButton?.onClick.Clear();
            contentPane.GetChild("PAG3")?.asButton?.onClick.Clear();
            contentPane.GetChild("PAG4")?.asButton?.onClick.Clear();
            contentPane.GetChild("PAG5")?.asButton?.onClick.Clear();
            contentPane.GetChild("PAG6")?.asButton?.onClick.Clear();
            contentPane.GetChild("PAG7")?.asButton?.onClick.Clear();
            contentPane.GetChild("PAG8")?.asButton?.onClick.Clear();
            contentPane.GetChild("PAG9")?.asButton?.onClick.Clear();
            contentPane.GetChild("PAG10")?.asButton?.onClick.Clear();
            contentPane.GetChild("PAG11")?.asButton?.onClick.Clear();
            contentPane.GetChild("PAG12")?.asButton?.onClick.Clear();
            contentPane.GetChild("Spine1")?.asButton?.onClick.Clear();
            contentPane.GetChild("Spine2")?.asButton?.onClick.Clear();
            contentPane.GetChild("Spine3")?.asButton?.onClick.Clear();
            contentPane.GetChild("Spine4")?.asButton?.onClick.Clear();
            contentPane.GetChild("Spine5")?.asButton?.onClick.Clear();
            _pagTestButtonsBound = false;
        }

        /// <summary>
        /// 在 _clonePagTest 上查找并初始化 Spine1~5 对照节点
        /// </summary>
        private void EnsurePagTestSpines()
        {
            if (_clonePagTest == null)
            {
                return;
            }

            for (int i = 0; i < PagTestSpineCount; i++)
            {
                int spineIndex = i + 1;
                EnsurePagTestSpine(ref _pagTestSpineAnimators[i], ref _pagTestSpineMecanims[i], PagTestSpineConfigs[i].NodeName, spineIndex);
            }
        }

        /// <summary>
        /// 按 spineIndex 返回 Spine 对照节点的 Animator 与 SkeletonMecanim
        /// </summary>
        /// <param name="spineIndex"> Spine 按钮下标（1~5）</param>
        /// <param name="animator"> 输出Animator 引用</param>
        /// <param name="mecanim"> 输出SkeletonMecanim 引用</param>
        private bool TryGetPagTestSpine(int spineIndex, out Animator animator, out SkeletonMecanim mecanim)
        {
            animator = null;
            mecanim = null;
            int i = spineIndex - 1;
            if (i < 0 || i >= PagTestSpineCount)
            {
                return false;
            }

            animator = _pagTestSpineAnimators[i];
            mecanim = _pagTestSpineMecanims[i];
            return true;
        }

        /// <summary>
        /// 查询 Spine 对照节点当前是否处于显示播放状态
        /// </summary>
        /// <param name="spineIndex"> Spine 按钮下标（1~5）</param>
        private bool GetSpineTestShowing(int spineIndex)
        {
            int i = spineIndex - 1;
            return i >= 0 && i < PagTestSpineCount && _spineTestShowing[i];
        }

        /// <summary>
        /// 设置 Spine 对照节点 showing 标志
        /// </summary>
        /// <param name="spineIndex"> Spine 按钮下标（1~5）</param>
        /// <param name="showing"> 是否正在播放/显示</param>
        private void SetSpineTestShowing(int spineIndex, bool showing)
        {
            int i = spineIndex - 1;
            if (i >= 0 && i < PagTestSpineCount)
            {
                _spineTestShowing[i] = showing;
            }
        }

        /// <summary>
        /// 懒加载单个 Spine 节点组件并默认隐藏
        /// </summary>
        /// <param name="animator"> Animator 引用</param>
        /// <param name="mecanim"> SkeletonMecanim 引用</param>
        /// <param name="nodeName"> Spine 节点名</param>
        /// <param name="spineIndex"> Spine 按钮下标（1~5）</param>
        private void EnsurePagTestSpine(ref Animator animator, ref SkeletonMecanim mecanim, string nodeName, int spineIndex)
        {
            if (animator != null)
            {
                return;
            }

            Transform spineTransform = _clonePagTest.transform.Find($"Anchor/{nodeName}");
            if (spineTransform == null)
            {
                spineTransform = FindChildRecursiveByName(_clonePagTest.transform, nodeName);
            }

            if (spineTransform == null)
            {
                Debug.LogWarning($"{PagLogPrefix} spine not found on _clonePagTest: {nodeName}");
                return;
            }

            animator = spineTransform.GetComponent<Animator>();
            mecanim = spineTransform.GetComponent<SkeletonMecanim>();
            if (animator == null)
            {
                Debug.LogWarning($"{PagLogPrefix} Animator missing on spine: {nodeName}");
                return;
            }

            HidePagTestSpine(spineIndex);
        }

        /// <summary>
        /// 隐藏 Spine 对照动画（ClearState + SetActive false，清 mesh 并停止更新与渲染）
        /// </summary>
        /// <param name="spineIndex"> Spine 按钮下标（1~5）</param>
        private void HidePagTestSpine(int spineIndex)
        {
            if (!TryGetPagTestSpine(spineIndex, out Animator animator, out SkeletonMecanim mecanim) || animator == null)
            {
                return;
            }

            if (mecanim != null)
            {
                mecanim.ClearState();
            }

            animator.gameObject.SetActive(false);
            SetSpineTestShowing(spineIndex, false);
        }

        /// <summary>
        /// 显示并播放 Spine 对照动画
        /// </summary>
        /// <param name="spineIndex"> Spine 按钮下标（1~5）</param>
        /// <param name="animName"> Spine 动画名</param>
        private void ShowPagTestSpine(int spineIndex, string animName)
        {
            EnsurePagTestSpines();

            if (!TryGetPagTestSpine(spineIndex, out Animator animator, out SkeletonMecanim mecanim) || animator == null)
            {
                Debug.LogWarning($"{PagLogPrefix} Spine{spineIndex} show failed: animator is null");
                return;
            }

            animator.gameObject.SetActive(true);

            if (mecanim != null)
            {
                mecanim.ClearState();
                mecanim.Skeleton.SetColor(new Color(1f, 1f, 1f, 1f));
            }

            animator.enabled = true;
            animator.speed = 1f;
            animator.Rebind();
            animator.Update(0f);
            animator.Play(animName, 0, 0f);
            animator.Update(0f);

            if (mecanim != null)
            {
                mecanim.Update();
                mecanim.LateUpdate();
            }

            SetSpineTestShowing(spineIndex, true);

            Debug.Log($"{PagLogPrefix} Spine{spineIndex} show {animName}");
        }

        /// <summary>
        /// Spine1 对照按钮：切换 jp_pup_grand / GRAND_in
        /// </summary>
        private void OnClickSpineTest1Button()
        {
            TogglePagTestSpine(1, PagTestSpineConfigs[0].PlayAnim);
        }

        /// <summary>
        /// Spine2 对照按钮：切换 ng_pop_bigWin / bigwin_start
        /// </summary>
        private void OnClickSpineTest2Button()
        {
            TogglePagTestSpine(2, PagTestSpineConfigs[1].PlayAnim);
        }

        /// <summary>
        /// Spine3 对照按钮：切换 jp_pup_GRAND / in
        /// </summary>
        private void OnClickSpineTest3Button()
        {
            TogglePagTestSpine(3, PagTestSpineConfigs[2].PlayAnim);
        }

        /// <summary>
        /// Spine4 对照按钮：切换 ng_bor_boom1 / start
        /// </summary>
        private void OnClickSpineTest4Button()
        {
            TogglePagTestSpine(4, PagTestSpineConfigs[3].PlayAnim);
        }

        /// <summary>
        /// Spine5 对照按钮：切换 ng_ic_bigwin / bigwin_start
        /// </summary>
        private void OnClickSpineTest5Button()
        {
            TogglePagTestSpine(5, PagTestSpineConfigs[4].PlayAnim);
        }

        /// <summary>
        /// Spine 对照显示/隐藏切换
        /// </summary>
        /// <param name="spineIndex"> Spine 按钮下标（1~5）</param>
        /// <param name="animName"> Spine 动画名</param>
        private void TogglePagTestSpine(int spineIndex, string animName)
        {
            if (GetSpineTestShowing(spineIndex))
            {
                Debug.Log($"{PagLogPrefix} Spine{spineIndex} clicked, hide");
                HidePagTestSpine(spineIndex);
                return;
            }

            ShowPagTestSpine(spineIndex, animName);
        }

        /// <summary>
        /// 在 anchorPagTest 上实例化 BorderMegaWin 特效并挂到 holder GoWrapper
        /// </summary>
        private void EnsureBorderMegaWinEffects()
        {
            if (_anchorPagTest == null)
            {
                return;
            }

            for (int i = 0; i < BorderMegaWinPrefabNames.Length; i++)
            {
                if (_goBorderMegaWinPrefabs[i] == null)
                {
                    Debug.LogWarning($"{BorderMegaWinLogPrefix} prefab missing: {BorderMegaWinPrefabNames[i]}");
                    continue;
                }

                if (_cloneBorderMegaWinEffects[i] != null)
                {
                    continue;
                }

                _cloneBorderMegaWinEffects[i] = GameObject.Instantiate(_goBorderMegaWinPrefabs[i]);
                _borderMegaWinEffectRoots[i] = _cloneBorderMegaWinEffects[i].transform;

                GGraph holder = _anchorPagTest.GetChild(BorderMegaWinHolderNames[i])?.asGraph;
                if (holder == null)
                {
                    Debug.LogWarning($"{BorderMegaWinLogPrefix} holder missing: {BorderMegaWinHolderNames[i]}");
                    continue;
                }

                _cloneBorderMegaWinEffects[i].transform.localPosition = Vector3.zero;
                _cloneBorderMegaWinEffects[i].transform.localScale = Vector3.one;
                GoWrapper wrapper = new GoWrapper(_cloneBorderMegaWinEffects[i]);
                holder.SetNativeObject(wrapper);
                holder.SetPivot(0.5f, 0.5f, true);
                holder.visible = false;
                StopBorderMegaWinChildEffectAnim(_borderMegaWinEffectRoots[i]);
            }
        }

        /// <summary>
        /// 绑定 Effect1~5 边框 MegaWin 测试按钮
        /// </summary>
        private void BindBorderMegaWinButtons()
        {
            if (_borderMegaWinButtonsBound || contentPane == null)
            {
                return;
            }

            for (int i = 0; i < BorderMegaWinPrefabNames.Length; i++)
            {
                string buttonName = $"Effect{i + 1}";
                int capturedIndex = i;
                GButton btn = contentPane.GetChild(buttonName)?.asButton;
                if (btn != null)
                {
                    btn.onClick.Clear();
                    btn.onClick.Add(() => OnClickBorderMegaWinButton(capturedIndex));
                }
                else
                {
                    Debug.LogWarning($"{BorderMegaWinLogPrefix} button missing: {buttonName}");
                }
            }

            _borderMegaWinButtonsBound = true;
        }

        /// <summary>
        /// 清除 BorderMegaWin 测试按钮点击监听
        /// </summary>
        private void ClearBorderMegaWinButtons()
        {
            if (!_borderMegaWinButtonsBound || contentPane == null)
            {
                return;
            }

            for (int i = 0; i < BorderMegaWinPrefabNames.Length; i++)
            {
                contentPane.GetChild($"Effect{i + 1}")?.asButton?.onClick.Clear();
            }

            _borderMegaWinButtonsBound = false;
        }

        /// <summary>
        /// Effect 按钮点击：切换对应 BorderMegaWin 特效播放/停止
        /// </summary>
        /// <param name="index"> 数组下标</param>
        private void OnClickBorderMegaWinButton(int index)
        {
            if (index < 0 || index >= BorderMegaWinPrefabNames.Length)
            {
                return;
            }

            if (_borderMegaWinShowing[index])
            {
                Debug.Log($"{BorderMegaWinLogPrefix} Effect{index + 1} clicked, stop {BorderMegaWinPrefabNames[index]}");
                StopBorderMegaWinEffect(index);
                return;
            }

            Debug.Log($"{BorderMegaWinLogPrefix} Effect{index + 1} clicked, play {BorderMegaWinPrefabNames[index]}");
            PlayBorderMegaWinEffect(index);
        }

        /// <summary>
        /// 显示并播放指定索引的 BorderMegaWin 粒子特效
        /// </summary>
        /// <param name="index"> 数组下标</param>
        private void PlayBorderMegaWinEffect(int index)
        {
            if (index < 0 || index >= BorderMegaWinPrefabNames.Length)
            {
                return;
            }

            EnsureBorderMegaWinEffects();

            Transform effectRoot = _borderMegaWinEffectRoots[index];
            if (effectRoot == null)
            {
                Debug.LogWarning($"{BorderMegaWinLogPrefix} Effect{index + 1} root is null");
                return;
            }

            GGraph holder = _anchorPagTest?.GetChild(BorderMegaWinHolderNames[index])?.asGraph;
            if (holder == null)
            {
                return;
            }

            holder.visible = true;
            PlayBorderMegaWinChildEffectAnim(effectRoot);
            _borderMegaWinShowing[index] = true;
        }

        /// <summary>
        /// 停止并隐藏指定索引的 BorderMegaWin 特效
        /// </summary>
        /// <param name="index"> 数组下标</param>
        private void StopBorderMegaWinEffect(int index)
        {
            if (index < 0 || index >= BorderMegaWinPrefabNames.Length)
            {
                return;
            }

            Transform effectRoot = _borderMegaWinEffectRoots[index];
            if (effectRoot != null)
            {
                StopBorderMegaWinChildEffectAnim(effectRoot);
            }

            GGraph holder = _anchorPagTest?.GetChild(BorderMegaWinHolderNames[index])?.asGraph;
            if (holder != null)
            {
                holder.visible = false;
            }

            _borderMegaWinShowing[index] = false;
        }

        /// <summary>
        /// 按 spineIndex 返回组合测试用的 Spine 动画名
        /// </summary>
        /// <param name="spineIndex"> Spine 按钮下标（1~5）</param>
        private string GetPagTestSpinePlayAnim(int spineIndex)
        {
            switch (spineIndex)
            {
                case 1: return PagTestSpineConfigs[0].PlayAnim;
                case 2: return PagTestSpineConfigs[1].PlayAnim;
                case 3: return PagTestSpineConfigs[2].PlayAnim;
                case 4: return PagTestSpineConfigs[3].PlayAnim;
                case 5: return PagTestSpineConfigs[4].PlayAnim;
                default: return string.Empty;
            }
        }

        /// <summary>
        /// 绑定 Combo 下拉与播放按钮
        /// </summary>
        private void BindComboPlayControls()
        {
            if (_comboPlayControlsBound || contentPane == null)
            {
                return;
            }

            _comboPagBox = contentPane.GetChild("comboPag")?.asComboBox;
            _comboSpineBox = contentPane.GetChild("comboSpine")?.asComboBox;
            _comboEffectBox = contentPane.GetChild("comboEffect")?.asComboBox;
            if (_comboPagBox == null || _comboSpineBox == null || _comboEffectBox == null)
            {
                Debug.LogWarning($"{PagLogPrefix} combo dropdown missing, publish FGUI first");
                return;
            }

            _comboPagBox.items = ComboPagDropdownItems;
            _comboSpineBox.items = ComboSpineDropdownItems;
            _comboEffectBox.items = ComboEffectDropdownItems;
            _comboPagBox.selectedIndex = 0;
            _comboSpineBox.selectedIndex = 0;
            _comboEffectBox.selectedIndex = 0;
            _comboPagBox.title = ComboPagDropdownItems[0];
            _comboSpineBox.title = ComboSpineDropdownItems[0];
            _comboEffectBox.title = ComboEffectDropdownItems[0];
            EnsureComboDropdownLabel("lblComboPag", _comboPagBox, "PAG");
            EnsureComboDropdownLabel("lblComboSpine", _comboSpineBox, "Spine");
            EnsureComboDropdownLabel("lblComboEffect", _comboEffectBox, "Effect");

            GButton btnCombo = contentPane.GetChild("btnCombo")?.asButton;
            if (btnCombo != null)
            {
                btnCombo.onClick.Clear();
                btnCombo.onClick.Add(OnClickComboPlayButton);
            }
            else
            {
                Debug.LogWarning($"{PagLogPrefix} button missing: btnCombo");
            }

            _comboPlayControlsBound = true;
        }

        /// <summary>
        /// 为 Combo 下拉补旁注（FGUI 未 Publish label 时由代码兜底）
        /// </summary>
        /// <param name="labelName"> 标签控件名</param>
        /// <param name="comboBox"> 下拉框控件</param>
        /// <param name="labelText"> 标签文本</param>
        private void EnsureComboDropdownLabel(string labelName, GComboBox comboBox, string labelText)
        {
            if (comboBox == null || contentPane == null)
            {
                return;
            }

            GObject labelObject = contentPane.GetChild(labelName);
            if (labelObject != null)
            {
                if (labelObject is GTextField existingLabel)
                {
                    existingLabel.text = labelText;
                }

                return;
            }

            GTextField runtimeLabel = new GTextField();
            runtimeLabel.name = labelName;
            runtimeLabel.text = labelText;
            runtimeLabel.touchable = false;
            TextFormat textFormat = runtimeLabel.textFormat;
            textFormat.size = 20;
            textFormat.color = Color.black;
            runtimeLabel.textFormat = textFormat;
            runtimeLabel.SetSize(42, comboBox.height);
            runtimeLabel.SetXY(comboBox.x - 42, comboBox.y);
            runtimeLabel.verticalAlign = VertAlignType.Middle;
            contentPane.AddChild(runtimeLabel);
        }

        /// <summary>
        /// OnClose / OnLanguageChange 前清除 Combo 控件监听并停止播放
        /// </summary>
        private void ClearComboPlayControls()
        {
            if (!_comboPlayControlsBound || contentPane == null)
            {
                return;
            }

            contentPane.GetChild("btnCombo")?.asButton?.onClick.Clear();
            _comboPlayControlsBound = false;
            StopComboPlayback();
        }

        /// <summary>
        /// Combo 按钮：根据三项下拉选择播放或停止组合测试
        /// </summary>
        private void OnClickComboPlayButton()
        {
            if (_comboPlayActive)
            {
                StopComboPlayback();
                return;
            }

            PlayComboFromSelection();
        }

        /// <summary>
        /// 读取下拉选择并启动组合播放协程
        /// </summary>
        private void PlayComboFromSelection()
        {
            int pagIndex = _comboPagBox != null ? _comboPagBox.selectedIndex : 0;
            int spineIndex = _comboSpineBox != null ? _comboSpineBox.selectedIndex : 0;
            int effectIndex = _comboEffectBox != null ? _comboEffectBox.selectedIndex : 0;
            if (pagIndex <= 0 && spineIndex <= 0 && effectIndex <= 0)
            {
                Debug.LogWarning($"{PagLogPrefix} Combo play skipped: select PAG, Spine or Effect first");
                return;
            }

            if (mono == null)
            {
                Debug.LogWarning($"{PagLogPrefix} Combo play skipped: mono is null");
                return;
            }

            string comboLabel = BuildComboSelectionLabel(pagIndex, spineIndex, effectIndex);
            Debug.Log($"{PagLogPrefix} Combo clicked, play {comboLabel}");
            _comboActivePagIndex = pagIndex;
            _comboActiveSpineIndex = spineIndex;
            _comboActiveEffectIndex = effectIndex;
            _comboPlayActive = true;
            CancelComboPlayCoroutine();
            _comboPlayCoroutine = mono.StartCoroutine(PlayComboFromSelectionCoroutine(pagIndex, spineIndex, effectIndex, comboLabel));
        }

        /// <summary>
        /// 停止当前 Combo 组合播放并清理 PAG / Spine / Effect 状态
        /// </summary>
        private void StopComboPlayback()
        {
            if (!_comboPlayActive)
            {
                CancelComboPlayCoroutine();
                return;
            }

            int pagIndex = _comboActivePagIndex;
            int spineIndex = _comboActiveSpineIndex;
            int effectIndex = _comboActiveEffectIndex;
            _comboPlayActive = false;
            _comboActivePagIndex = 0;
            _comboActiveSpineIndex = 0;
            _comboActiveEffectIndex = 0;
            CancelComboPlayCoroutine();

            Debug.Log($"{PagLogPrefix} Combo clicked, stop");
            StopComboPagPlayback(pagIndex);
            if (spineIndex > 0)
            {
                HidePagTestSpine(spineIndex);
            }

            if (effectIndex > 0)
            {
                StopBorderMegaWinEffect(effectIndex - 1);
            }
        }

        /// <summary>
        /// 取消 Combo 主协程
        /// </summary>
        private void CancelComboPlayCoroutine()
        {
            if (_comboPlayCoroutine != null && mono != null)
            {
                mono.StopCoroutine(_comboPlayCoroutine);
                _comboPlayCoroutine = null;
            }
        }

        /// <summary>
        /// 根据 Combo 下拉选项生成组合标签字符串
        /// </summary>
        /// <param name="pagIndex"> PAG 按钮下标（1~12）</param>
        /// <param name="spineIndex"> Spine 按钮下标（1~5）</param>
        /// <param name="effectIndex"> Effect 下标（0~4）</param>
        private static string BuildComboSelectionLabel(int pagIndex, int spineIndex, int effectIndex)
        {
            var parts = new List<string>(3);
            if (pagIndex > 0)
            {
                parts.Add($"PAG{pagIndex}");
            }

            if (spineIndex > 0)
            {
                parts.Add($"Spine{spineIndex}");
            }

            if (effectIndex > 0)
            {
                parts.Add($"Effect{effectIndex}");
            }

            return string.Join("+", parts);
        }

        /// <summary>
        /// 按 pagIndex 停止 Combo 触发的 PAG 播放（不递归调用 StopComboPlayback）
        /// </summary>
        /// <param name="pagIndex"> PAG 按钮下标（1~12）</param>
        private void StopComboPagPlayback(int pagIndex)
        {
            StopPagTestPlayback(pagIndex, stopComboIfActive: false);
        }

        /// <summary>
        /// Combo 主分发协程：按 PAG / Spine / Effect 组合复用既有播放逻辑
        /// </summary>
        /// <param name="pagIndex"> PAG 按钮下标（1~12）</param>
        /// <param name="spineIndex"> Spine 按钮下标（1~5）</param>
        /// <param name="effectIndex"> Effect 下标（0~4）</param>
        /// <param name="comboLabel"> Combo 组合标签</param>
        private IEnumerator PlayComboFromSelectionCoroutine(int pagIndex, int spineIndex, int effectIndex, string comboLabel)
        {
            if (pagIndex > 0 && spineIndex > 0)
            {
                if (pagIndex == 2)
                {
                    yield return PlayComboPag2SpineCoroutine(spineIndex, comboLabel);
                }
                else if (pagIndex == 3)
                {
                    yield return PlayComboPag3SpineCoroutine(spineIndex, comboLabel);
                }
                else if (pagIndex == 4)
                {
                    StartPagTestPlayback(4);
                    ShowPagTestSpine(spineIndex, GetPagTestSpinePlayAnim(spineIndex));
                    Debug.Log($"{PagLogPrefix} {comboLabel} PAG4+Spine started");
                }
                else if (pagIndex >= 5 && pagIndex <= 9)
                {
                    if (!StartPagTestPlayback(pagIndex))
                    {
                        _comboPlayActive = false;
                        yield break;
                    }

                    ShowPagTestSpine(spineIndex, GetPagTestSpinePlayAnim(spineIndex));
                    Debug.Log($"{PagLogPrefix} {comboLabel} PAG{pagIndex}+Spine started");
                }
                else
                {
                    yield return PlayComboPagSpineCoroutine(pagIndex, spineIndex, comboLabel);
                }
            }
            else if (pagIndex > 0)
            {
                yield return PlayComboPagOnlyCoroutine(pagIndex, comboLabel);
            }
            else if (spineIndex > 0)
            {
                ShowPagTestSpine(spineIndex, GetPagTestSpinePlayAnim(spineIndex));
                Debug.Log($"{PagLogPrefix} {comboLabel} Spine only");
            }

            if (!_comboPlayActive)
            {
                _comboPlayCoroutine = null;
                yield break;
            }

            if (effectIndex > 0)
            {
                PlayBorderMegaWinEffect(effectIndex - 1);
                Debug.Log($"{BorderMegaWinLogPrefix} {comboLabel} Effect{effectIndex}");
            }

            _comboPlayCoroutine = null;
        }

        /// <summary>
        /// Combo 仅 PAG：与单按钮播放路径一致
        /// </summary>
        /// <param name="pagIndex"> PAG 按钮下标（1~12）</param>
        /// <param name="comboLabel"> Combo 组合标签</param>
        private IEnumerator PlayComboPagOnlyCoroutine(int pagIndex, string comboLabel)
        {
            if (!StartPagTestPlayback(pagIndex))
            {
                _comboPlayActive = false;
            }

            Debug.Log($"{PagLogPrefix} {comboLabel} PAG{pagIndex} only");
            yield break;
        }

        /// <summary>
        /// PAG2+Spine 组合协程：等 PAG2 纹理就绪后同步显示 Spine
        /// </summary>
        /// <param name="spineIndex"> Spine 按钮下标（1~5）</param>
        /// <param name="comboLabel"> Combo 组合标签</param>
        private IEnumerator PlayComboPag2SpineCoroutine(int spineIndex, string comboLabel)
        {
            if (!StartPagTestPlayback(2))
            {
                _comboPlayActive = false;
                yield break;
            }

            PagController pagController = _pagTestSlotBindings[1]?.Controller;
            if (pagController != null)
            {
                yield return pagController.WaitForGpuDisplayReady(PagTestPlayStartedTimeoutSec);
            }

            if (!_comboPlayActive || _comboActivePagIndex != 2 || _comboActiveSpineIndex != spineIndex)
            {
                yield break;
            }

            ShowPagTestSpine(spineIndex, GetPagTestSpinePlayAnim(spineIndex));
            Debug.Log($"{PagLogPrefix} {comboLabel} spine synced at texture display ready");
        }

        /// <summary>
        /// PAG3+Spine 组合协程：等 PAG3 纹理就绪后同步显示 Spine
        /// </summary>
        /// <param name="spineIndex"> Spine 按钮下标（1~5）</param>
        /// <param name="comboLabel"> Combo 组合标签</param>
        private IEnumerator PlayComboPag3SpineCoroutine(int spineIndex, string comboLabel)
        {
            if (!StartPagTestPlayback(3))
            {
                _comboPlayActive = false;
                yield break;
            }

            PagController pagController = _pagTestSlotBindings[2]?.Controller;
            if (pagController != null)
            {
                yield return pagController.WaitForGpuDisplayReady(PagTestPlayStartedTimeoutSec);
            }

            if (!_comboPlayActive || _comboActivePagIndex != 3 || _comboActiveSpineIndex != spineIndex)
            {
                yield break;
            }

            ShowPagTestSpine(spineIndex, GetPagTestSpinePlayAnim(spineIndex));
            Debug.Log($"{PagLogPrefix} {comboLabel} spine synced at texture display ready");
        }

        /// <summary>
        /// PAG1 / PAG10~12 + Spine：单槽或 NPC 播放后等 GPU 就绪再显示 Spine
        /// </summary>
        /// <param name="pagIndex"> PAG 按钮下标（0~11）</param>
        /// <param name="spineIndex"> Spine 按钮下标（1~5）</param>
        /// <param name="comboLabel"> Combo 组合标签</param>
        private IEnumerator PlayComboPagSpineCoroutine(int pagIndex, int spineIndex, string comboLabel)
        {
            PagSlotBinding pagSlot = null;
            if (pagIndex == 1 || (pagIndex >= 10 && pagIndex <= 12))
            {
                if (!StartPagTestPlayback(pagIndex))
                {
                    _comboPlayActive = false;
                    yield break;
                }

                pagSlot = GetPagTestSlotByPagIndex(pagIndex);
            }
            else
            {
                Debug.LogWarning($"{PagLogPrefix} {comboLabel} unsupported PAG+Spine pagIndex={pagIndex}");
                _comboPlayActive = false;
                yield break;
            }

            PagController pagController = pagSlot?.Controller;
            if (pagController != null)
            {
                yield return pagController.WaitForGpuDisplayReady(PagTestPlayStartedTimeoutSec);
            }

            if (!_comboPlayActive || _comboActivePagIndex != pagIndex || _comboActiveSpineIndex != spineIndex)
            {
                yield break;
            }

            ShowPagTestSpine(spineIndex, GetPagTestSpinePlayAnim(spineIndex));
            Debug.Log($"{PagLogPrefix} {comboLabel} spine synced at texture display ready");
        }

        /// <summary>
        /// 递归播放 BorderMegaWin 子节点粒子
        /// </summary>
        /// <param name="effect"> Transform 节点</param>
        private void PlayBorderMegaWinChildEffectAnim(Transform effect)
        {
            if (effect == null)
            {
                return;
            }

            foreach (Transform child in effect)
            {
                PlayBorderMegaWinEffectAnim(child);
            }
        }

        /// <summary>
        /// 递归播放 Transform 树中的 ParticleSystem
        /// </summary>
        /// <param name="effect"> Transform 节点</param>
        private void PlayBorderMegaWinEffectAnim(Transform effect)
        {
            if (effect == null)
            {
                return;
            }

            ParticleSystem particle = effect.GetComponent<ParticleSystem>();
            if (particle != null)
            {
                particle.Play();
            }

            foreach (Transform child in effect)
            {
                PlayBorderMegaWinEffectAnim(child);
            }
        }

        /// <summary>
        /// 递归停止 BorderMegaWin 子节点粒子
        /// </summary>
        /// <param name="effect"> Transform 节点</param>
        private void StopBorderMegaWinChildEffectAnim(Transform effect)
        {
            if (effect == null)
            {
                return;
            }

            foreach (Transform child in effect)
            {
                StopBorderMegaWinEffectAnim(child);
            }
        }

        /// <summary>
        /// 递归停止 Transform 树中的 ParticleSystem
        /// </summary>
        /// <param name="effect"> Transform 节点</param>
        private void StopBorderMegaWinEffectAnim(Transform effect)
        {
            if (effect == null)
            {
                return;
            }

            ParticleSystem particle = effect.GetComponent<ParticleSystem>();
            if (particle != null)
            {
                particle.Stop(true);
                particle.Clear(true);
            }

            foreach (Transform child in effect)
            {
                StopBorderMegaWinEffectAnim(child);
            }
        }

        /// <summary>
        /// 停止全部 BorderMegaWin 特效并隐藏 holder
        /// </summary>
        private void StopAllBorderMegaWinEffects()
        {
            for (int i = 0; i < BorderMegaWinPrefabNames.Length; i++)
            {
                if (_borderMegaWinEffectRoots[i] != null)
                {
                    StopBorderMegaWinChildEffectAnim(_borderMegaWinEffectRoots[i]);
                }

                if (_anchorPagTest != null)
                {
                    GGraph holder = _anchorPagTest.GetChild(BorderMegaWinHolderNames[i])?.asGraph;
                    if (holder != null)
                    {
                        holder.visible = false;
                    }
                }

                _borderMegaWinShowing[i] = false;
            }
        }

        /// <summary>
        /// OnClose 时销毁 BorderMegaWin clone 与 GoWrapper
        /// </summary>
        private void DisposeBorderMegaWinEffects()
        {
            if (_anchorPagTest != null)
            {
                for (int i = 0; i < BorderMegaWinHolderNames.Length; i++)
                {
                    GGraph holder = _anchorPagTest.GetChild(BorderMegaWinHolderNames[i])?.asGraph;
                    if (holder == null)
                    {
                        continue;
                    }

                    GoWrapper wrapper = holder.displayObject as GoWrapper;
                    if (wrapper != null)
                    {
                        wrapper.Dispose();
                    }
                    else
                    {
                        holder.SetNativeObject(null);
                    }

                    holder.visible = false;
                }
            }

            for (int i = 0; i < _cloneBorderMegaWinEffects.Length; i++)
            {
                if (_cloneBorderMegaWinEffects[i] != null)
                {
                    GameObject.Destroy(_cloneBorderMegaWinEffects[i]);
                    _cloneBorderMegaWinEffects[i] = null;
                }

                _borderMegaWinEffectRoots[i] = null;
                _borderMegaWinShowing[i] = false;
            }
        }

        /// <summary>
        /// 在 Transform 子树中按名称递归查找节点
        /// </summary>
        /// <param name="parent"> 父 Transform 节点</param>
        /// <param name="targetName"> 目标节点名</param>
        private Transform FindChildRecursiveByName(Transform parent, string targetName)
        {
            if (parent == null || string.IsNullOrEmpty(targetName))
            {
                return null;
            }

            if (parent.name == targetName)
            {
                return parent;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                Transform result = FindChildRecursiveByName(child, targetName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// 从本地快照恢复未完成的免费局（不自动请求 Spin，由玩家点转）
        /// </summary>
        void TryRestoreFreeSpinSession()
        {
            if (ApplicationSettings.Instance.isMock || slotMachineCtrl == null) return;
            if (!SQLitePlayerPrefs03.Instance.isInit) return;
            if (!isOpen) return;

            int pid = SBoxModel.Instance.pid;
            var snap = FreeSpinSessionStoreG1700.TryLoad(pid);
            if (snap == null) return;

            bool sessionStillValid = snap.FreeSpinTotalTimes > 0
                && (snap.FreeSpinPlayTimes < snap.FreeSpinTotalTimes
                    || (snap.FreeSpinPlayTimes == 0 && snap.NextReelStripsIndex == "FS"));
            if (!sessionStillValid)
            {
                FreeSpinSessionStoreG1700.Clear(pid);
                return;
            }

            var cm = ContentModel.Instance;
            cm.freeSpinTotalTimes = snap.FreeSpinTotalTimes;
            cm.freeSpinPlayTimes = snap.FreeSpinPlayTimes;
            cm.freeSpinTotalWinCredit = snap.FreeSpinTotalWinCredit;
            cm.curReelStripsIndex = snap.CurReelStripsIndex;
            cm.nextReelStripsIndex = snap.NextReelStripsIndex;
            cm.gameNumberFreeSpinTrigger = snap.GameNumberFreeSpinTrigger;
            cm.isFreeSpinTrigger = false;
            cm.isFreeSpinResult = false;
            cm.isFreeSpinAdd = false;
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
                SetUIFreeTimeBox(cm.freeSpinPlayTimes, cm.freeSpinTotalTimes);
            }


            slotMachineCtrl.SendTotalWinCreditEvent(cm.freeSpinTotalWinCredit);
            DebugUtils.Log( $"[G1700] 已恢复免费局快照：剩余 {cm.showFreeSpinRemainTime} / 总 {cm.freeSpinTotalTimes}，待首局 Spin 与算法校验。");
        }

        //普通滚动一次
        /// <summary>
        /// 普通局单次 Spin 主流程协程
        /// </summary>
        /// <param name="successCallback"> 成功回调</param>
        /// <param name="errorCallback"> 失败回调</param>
        IEnumerator GameOnce(Action successCallback, Action<string> errorCallback)
        {
            /*检查机器是否激活
            检查玩家余额是否足够支付当前投注
            如果条件不满足，调用错误回调并终止协程
            */
            if (!SBoxModel.Instance.isMachineActive)
            {
                errorCallback?.Invoke("<size=24>Machine not activated!</size>");
                yield break;
            }

            if (ContentModel.Instance.freeSpinTotalTimes > 0&& ContentModel.Instance.nextReelStripsIndex == "FS")
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

            // 检查余额通过后，立即扣除积分（提前扣分）
            if (ContentModel.Instance.gameState != GameState.FreeSpin)
            {
                MainBlackboardController.Instance.MinusMyTempCredit(TotalBet, true, false);
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
            string errMsg = "";

            //展会模式
            if (ApplicationSettings.Instance.IsExpoMode()&&MainModel.Instance.isExhibitionModeMode)
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
                        DebugUtils.LogError($"[G1700] 设置展会模式结果失败，deck={currentDeck}");
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
                },(err)=>
                {
                    errMsg = err;
                    isNext = true;
                    isBreak = true;
                });
            }

            yield return new WaitUntil(()=> isNext == true);
            isNext = false;

            //请求结果失败
            if (isBreak)
            {
                // 退还之前扣除的积分
                if (ContentModel.Instance.gameState != GameState.FreeSpin)
                {
                    MainBlackboardController.Instance.AddMyTempCredit(TotalBet, true, false);
                }

                if (errorCallback != null)
                    errorCallback.Invoke(errMsg);
                yield break;
            }

            //检查是否启用在线彩金,请求彩金数据
            if (SBoxModel.Instance.isJackpotOnLine && ClientWS.Instance.CurNetStatus == NET_STATUS.NET_STATUS_CONNECTED)
            {
                RequestOnlineJackpotBetByCurrentBet();
            }

            //开始滚动
            slotMachineCtrl.BeginSpin();
            //是否加速滚动
            if (ContentModel.Instance.isReelsSlowMotion)
            {
                //if (corEffectSlowMotion != null) mono.StopCoroutine(corEffectSlowMotion);
                //corEffectSlowMotion = mono.StartCoroutine(ShowEffectReelsSlowMotion());
                //slotMachineCtrl.ShowSymbolAppearEffectAfterReelStop(true);
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
            if (winList.Count > 0 || ContentModel.Instance.bonusResult != null)
            {
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
                    //yield return ShowWinListOnceAtNormalSpin(winList);
                }

                //检查bigwin类型
                WinLevelType winLevelType = GetBigWinType();
                //bigwi弹窗
                if (winLevelType != WinLevelType.None)
                {
                    //显示全部中奖图标和中奖线
                   // slotMachineCtrl.ShowSymbolWinDeck(slotMachineCtrl.GetTotalSymbolWin(winList), true);
                    //bigwin弹窗
                    yield return BigWinPopup(winLevelType, ContentModel.Instance.baseGameWinCredit);

                    slotMachineCtrl.CloseSlotCover();
                    slotMachineCtrl.SkipWinLine(false);
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
                MainBlackboardController.Instance.AddMyTempCredit(totalWinLineCredit, true, isAddCreditAnim);

                // 本剧同步玩家金钱
                MainBlackboardController.Instance.SyncMyTempCreditToReal(true);
            }
            #endregion

            #region Free
            //免费奖
            if (ContentModel.Instance.isFreeSpinTrigger)
            {
                //显示中奖动画
                slotMachineCtrl.ShowSymbolEffect(TagPoolObject.SymbolHit, new List<int>() { 10 }, true, 10, true);
                yield return slotMachineCtrl.SlotWaitForSeconds(1.5f);
                slotMachineCtrl.SkipWinLine(false);
           
                //切换背景和边框
                ChangeBGPanel(1);
                SetUIFreeTimeBox(ContentModel.Instance.freeSpinPlayTimes, ContentModel.Instance.freeSpinTotalTimes);
                yield return slotMachineCtrl.SlotWaitForSeconds(2.0f);
                yield return FreeSpinTrigger(null, errorCallback);
                ChangeBGPanel(0);
                MainBlackboardController.Instance.SyncMyTempCreditToReal(true);
            }
            #endregion

            #region Bonus

            #endregion

            #region JpOnline
            while (ContentModel.Instance.jpOnlineWin.Count > 0)
            {
                WinJackpotInfo data = ContentModel.Instance.jpOnlineWin[0];
                ContentModel.Instance.jpOnlineWin.RemoveAt(0);

                long winCredit = data.win;
                allWinCredit += winCredit;

                // 总线赢分（同步？？）
                slotMachineCtrl.SendTotalWinCreditEvent(allWinCredit);

                MainBlackboardController.Instance.AddMyTempCredit(winCredit, true, isAddCreditAnim);
            }
            #endregion


            //test核对前后端积分
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
                        DebugUtils.LogError($" 算法卡 :[0]= {credit}   前端:[0]={SBoxModel.Instance.myCredit}");
                    }
                    isNext = true;
                }

            });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            // 本剧同步玩家金钱
            MainBlackboardController.Instance.SyncMyTempCreditToReal(true);
            // 即中即退
            // yield return CoinOutImmediately(allWinCredit);
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
        //免费游戏滚动一次
        /// <summary>
        /// 免费局单次 Spin 主流程协程
        /// </summary>
        /// <param name="successCallback"> 成功回调</param>
        /// <param name="errorCallback"> 失败回调</param>
        IEnumerator GameFreeSpinOnce(Action successCallback, Action<string> errorCallback)
        {
            OnGameReset();
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

            //免费次数UI
            SetUIFreeTimeBox(ContentModel.Instance.freeSpinPlayTimes, ContentModel.Instance.freeSpinTotalTimes);
            //开始转动
            slotMachineCtrl.BeginSpin();
            if (ContentModel.Instance.isReelsSlowMotion)
            {
                if (corEffectSlowMotion != null) mono.StopCoroutine(corEffectSlowMotion);
                corEffectSlowMotion = mono.StartCoroutine(ShowEffectReelsSlowMotion());

                slotMachineCtrl.ShowSymbolAppearEffectAfterReelStop(true);
            }
            else
            {
                slotMachineCtrl.ShowSymbolAppearEffectAfterReelStop(ContentModel.Instance.winList.Count == 0);
            }

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

            List<SymbolWin> winList = ContentModel.Instance.winList;
            long allWinCredit = 0;
            #region Win

            if (winList.Count > 0)
            {
                long totalWinLineCredit = slotMachineCtrl.GetTotalWinCredit(winList);
                allWinCredit = totalWinLineCredit;

                if (winList.Count > 0)
                {
                    yield return ShowWinListOnceAtNormalSpin(winList);
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



                // 免费游戏中赢票栏显示累计值，不即时入余额
                slotMachineCtrl.SendTotalWinCreditEvent(ContentModel.Instance.freeSpinTotalWinCredit);

                //加钱动画
                //MainBlackboardController.Instance.AddMyTempCredit(totalWinLineCredit, true, isAddCreditAnim);
                ContentModel.Instance.freeOnceCredit = totalWinLineCredit;


            }

            #endregion


            // 免费游戏中不逐局同步余额，等待免费结束后统一结算
            ContentModel.Instance.gameState = GameState.Idle;

            if (successCallback != null)
                successCallback.Invoke();
        }
        //请求模拟结果
        /// <summary>
        /// Mock 模式请求 Spin 结果协程
        /// </summary>
        /// <param name="null"> null</param>
        /// <param name="null"> null</param>
        IEnumerator RequestSlotSpinFromMock(Action successCallback = null, Action<string> errorCallback = null)
        {
            bool isNext = false;
            bool isBreak = false;
            long totalBet = TotalBet;
            JSONNode resNode = null;
            
            //请求结果
            MachineDataG1700Controller.Instance.RequestSlotSpinFromMock(totalBet, (res) =>
            {
                resNode = res;
                isNext = true;
            },(err)=>
            {
                errorCallback?.Invoke(err.msg);
                isNext = true;
                isBreak = true;
            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;
            if (isBreak) yield break;

            SBoxJackpotData sboxJackpotData = null;

            ////赠送局不用扣分
            //if (ContentModel.Instance.gameState != GameState.FreeSpin)
            //{
            //    MainBlackboardController.Instance.MinusMyTempCredit(totalBet, true, false);
            //}

            // 解析数据
            MachineDataG1700Controller.Instance.ParseSlotSpin(totalBet, resNode, sboxJackpotData);
           
            // 数据入库

            // 游戏彩金滚轮
            //SetUIJackpotGameReel();

            if (successCallback != null)
                successCallback.Invoke();
        }
        //请求算法结果
        /// <summary>
        /// 真机模式请求 Spin 结果协程
        /// </summary>
        /// <param name="null"> null</param>
        /// <param name="null"> null</param>
        IEnumerator RequestSlotSpinFromMachine(Action successCallback = null, Action<string> errorCallback = null)
        {
            bool isBreak = false;
            bool isNext = false;
            bool isGetMyCredit = false;

            JSONNode resNode = null;
            int myCredit = -1;

            //请求算法结果
            ERPushMachineDataManager02.Instance.RequestCoinPushSpin((res) =>
            {
                Debug.Log("请求算法结果");
                resNode = JSONNode.Parse((string)res);
                isNext = true;
            });

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            //初始化本地彩金数据
            SBoxJackpotData sboxJackpotData =new SBoxJackpotData();
            sboxJackpotData.Lottery = new int[3];
            sboxJackpotData.JackpotOut = new int[3];
            sboxJackpotData.Jackpotlottery = new int[3];
            sboxJackpotData.JackpotOld = new int[3];
            //获取本地彩金贡献值
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
                int miniBet =   (int)data["mini"];

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

         
            // 解析数据
            MachineDataG1700Controller.Instance.ParseSlotSpin(TotalBet, resNode, sboxJackpotData);
            // 数据入库
            //MachineDataG1700Controller.Instance.Record();
            // ui 彩金
            SetUIJackpotGameReel();
            Debug.Log("获取滚轮成功");

            if (successCallback != null)
                successCallback.Invoke();
        }

        /// <summary>
        /// 下注时向大厅彩金主机上报当前 TotalBet
        /// </summary>
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

                //下注失败需要可以累计压分,最多10次
                DebugUtils.LogError($"请求大厅彩金下注失败: {ex.Message}");
            }
        }

        private readonly HashSet<long> _handledOnlineJackpotOrderIds = new HashSet<long>();
        /// <summary>
        /// 将联网彩金 jackpotId 映射为显示名称
        /// </summary>
        /// <param name="jackpotId"> jackpotId</param>
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

        /// <summary>
        /// 处理大厅联网彩金中奖下发：去重、入库并通知算法卡加分
        /// </summary>
        /// <param name="winInfo"> winInfo</param>
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
                long creditBefore = MainBlackboardController.Instance.myRealCredit;
                long creditAfter = MainBlackboardController.Instance.myRealCredit+ winCredit;
                string gameUID = string.IsNullOrEmpty(ContentModel.Instance.curGameGuid) ? "0" : ContentModel.Instance.curGameGuid;
                long createdAt = winInfo.time;
                TableJackpotRecordAsyncManager.Instance.AddJackpotRecord(jpLevel,jpName,winCredit,creditBefore,creditAfter,gameUID,createdAt);

                //通知算法卡赢得联网彩金
                SBoxWinNetJackpotInfo sBoxWinNetJackpotInfo = new SBoxWinNetJackpotInfo()
                {
                    MachineId = int.Parse(SBoxModel.Instance.MachineId),
                    PlayerId = SBoxModel.Instance.SboxPlayerAccount.PlayerId,
                    JackpotType = jpLevel,
                    JackpotWins = winCredit,
                };
                MachineDataManager02.Instance.RequestJackpotOnline(sBoxWinNetJackpotInfo,(res) =>
                {
                    //算法卡加分后同步分数
                    Debug.Log("通知算法卡赢得联网彩金");
                    JSONNode data = JSONNode.Parse((string)res);
                  
                    long creditBefore = MainBlackboardController.Instance.myRealCredit;
                    long JackpotWins = (long)data["JackpotWins"]; ;
                    creditAfter = creditBefore + JackpotWins;
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

        /// <summary>
        /// 普通 Spin 结束后按配置展示总线或单线中奖特效
        /// </summary>
        /// <param name="winList"> 中奖线列表</param>
        IEnumerator ShowWinListOnceAtNormalSpin(List<SymbolWin> winList)
        {
            //总线
            if (_spinWEMD.Instance.isTotalWin)
            {
                yield return slotMachineCtrl.ShowSymbolWinBySetting(slotMachineCtrl.GetTotalSymbolWin(winList), true, SpinWinEvent.TotalWinLine);
            }
            else
            {
                //单线
                slotMachineCtrl.SkipWinLine(false);
                int idx = 0;
                while (idx<winList.Count)
                {
                    SymbolWin curSymvolWin = winList[idx];
                    yield return slotMachineCtrl.ShowSymbolWinBySetting(curSymvolWin, true, SpinWinEvent.SingleWinLine);
                    ++idx;
                }

                //停止特效显示
                slotMachineCtrl.SkipWinLine(false);
                slotMachineCtrl.CloseSlotCover();
            }
        }

        /// <summary>
        /// 游戏状态重置：停止闲置协程并恢复转轮与遮罩默认状态
        /// </summary>
        private void OnGameReset()
        {
            if (corGameIdel != null) mono.StopCoroutine(corGameIdel);
            //mono.StopCoroutine(corEffectSlowMotion);
            slotMachineCtrl.isStopImmediately = false;
            slotMachineCtrl.CloseSlotCover();
            slotMachineCtrl.SkipWinLine(true);
        }

        /// <summary>
        /// 游戏闲置阶段循环展示中奖线特效
        /// </summary>
        /// <param name="winList"> 中奖线列表</param>
        private IEnumerator GameIdle(List<SymbolWin> winList)
        {
            if (winList.Count == 0)
            {
                yield break;
            }

            SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.GameIdle);

            //yield return new WaitForSeconds(3f);

            yield return slotMachineCtrl.ShowWinListAwayDuringIdle(winList);
        }

        //bigwin弹窗
        /// <summary>
        /// 打开并等待 BigWin 弹窗关闭
        /// </summary>
        /// <param name="winLevelType"> BigWin 等级类型</param>
        /// <param name="winCredit"> 赢分额度</param>
        IEnumerator BigWinPopup(WinLevelType winLevelType, long winCredit)
        {
            bool isNext = false;
            PageManager.Instance.OpenPage(PageName.SlotZhuZaiJinBiPopupBigWin,
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

        //免费弹窗
        /// <summary>
        /// 免费局触发流程协程
        /// </summary>
        /// <param name="successCallback"> 成功回调</param>
        /// <param name="errorCallback"> 失败回调</param>
        IEnumerator FreeSpinTrigger(Action successCallback, Action<string> errorCallback)
        {
            bool isNext = false;
            PageManager.Instance.OpenPageAsync(PageName.SlotZhuZaiJinBiPopupFreeSpinTrigger,
              new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object>()
                    {
                        //["autoCloseTimeS"] = 3f,
                        ["freeSpinCount"] = ContentModel.Instance.freeSpinTotalTimes,
                    }),
                (ed) =>
                {
                    Debug.Log("回调执行！isNext = true"); // 加日志
                    isNext = true;
                });
           
            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            yield return GameFreeSpin(null, errorCallback);

            // 免费游戏结束后统一把累计赢分加到余额
            long freeSpinTotalWinCredit = ContentModel.Instance.freeSpinTotalWinCredit;
            if (freeSpinTotalWinCredit > 0)
            {
                MainBlackboardController.Instance.AddMyTempCredit(freeSpinTotalWinCredit, true, isAddCreditAnim);
            }
        }

        /// <summary>
        /// 免费局完整循环协程
        /// </summary>
        /// <param name="successCallback"> 成功回调</param>
        /// <param name="errorCallback"> 失败回调</param>
        IEnumerator GameFreeSpin(Action successCallback, Action<string> errorCallback)
        {
            while (ContentModel.Instance.nextReelStripsIndex == "FS")
            {

                yield return GameFreeSpinOnce(null, errorCallback);
                yield return slotMachineCtrl.SlotWaitForSeconds(1);
            }

            if (successCallback != null)
                successCallback.Invoke();
        }

        /// <summary>
        /// /// 断电重连恢复免费局：点击一次开始后自动跑完整段免费，并统一结算与切回普通游戏。 ///
        /// </summary>
        /// <param name="successCallback"> 成功回调</param>
        /// <param name="errorCallback"> 失败回调</param>
        IEnumerator GameFreeSpinFromReconnect(Action successCallback, Action<string> errorCallback)
        {
            yield return GameFreeSpin(null, errorCallback);

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

        //bigwin类型
        /// <summary>
        /// 根据赢分倍率判定 BigWin 等级
        /// </summary>
        WinLevelType GetBigWinType()
        {
            long baseGameWinCredit = ContentModel.Instance.baseGameWinCredit;
            List<WinMultiple> winMultipleList = CustomModel.Instance.winLevelMultiple;
            long totalBet=ContentModel.Instance.totalBet;
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

        /// <summary>
        /// 异步加载 game_info_g1700.json 并写入 MainModel 基础配置
        /// </summary>
        private void ReadJsonBet()
        {
            //资源加载
            ResourceManager02.Instance.LoadAsset<TextAsset>(
                "Assets/GameRes/_Common/Game Maker/ABs/G1700/Datas/game_info_g1700.json", (txt) =>
                {
                    //JSON解析与错误处理
                    GameConfigRoot config = JsonConvert.DeserializeObject<GameConfigRoot>(txt.text);
                    if (config?.SymbolPaytable == null)
                    {
                        Debug.LogError("解析symbol_paytable失败，数据为空");
                        return;
                    }

                    MainModel.Instance.gameID = config.GameId;
                    MainModel.Instance.gameName = config.GameName;
                    MainModel.Instance.displayName = config.DisplayName;
                    MainModel.Instance.lineNum = config.LineNum;
                });
        }

        /// <summary>
        /// 转轮停止事件回调（当前为空实现，预留扩展）
        /// </summary>
        /// <param name="res"> 事件数据</param>
        private void OnStopSlot(EventData res)
        {

        }

        /// <summary>
        /// 底部 Panel Spin 按钮点击：单次/自动/立即停止等状态机分发
        /// </summary>
        /// <param name="res"> 事件数据</param>
        private void OnClickSpinButton(EventData res)
        {

            if (res.name == "SpinButtonClick")
            {
                bool isLongClick = (bool)res.value;
                switch (ContentModel.Instance.btnSpinState)
                {
                    case SpinButtonState.Stop:
                        if (ContentModel.Instance.isSpin) return; //已经开始玩直接退出？
                        ContentModel.Instance.isSpin = true;

                        Action successCallback = () =>
                        {
                            ContentModel.Instance.isSpin = false;
                            ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
                            ContentModel.Instance.gameState = GameState.Idle;
                            DebugUtils.Log("游戏结束");
                        };

                        if (isLongClick)
                        {
                            Debug.Log("机器按钮开始滚动 :Long");
                            ContentModel.Instance.isAuto = true;
                            ContentModel.Instance.btnSpinState = SpinButtonState.Auto;
                            StartGameAuto(successCallback, StopGameWhenError); //自动玩
                        }
                        else
                        {
                            Debug.Log("机器按钮开始滚动:Short");
                            ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                            StartGameOnce(successCallback, StopGameWhenError); //开始玩
                        }
                        break;
                    case SpinButtonState.Spin:
                        // 已经在游戏时，去停止游戏
                        if (!ContentModel.Instance.isSpin) return; // 已经停止直接退出
                        slotMachineCtrl.isStopImmediately = true; // 去停止游戏  
                        break;
                    case SpinButtonState.Auto:
                        //停止自动玩
                        //停止自动玩
                        ContentModel.Instance.isSpin = true;
                        ContentModel.Instance.isAuto = false;
                        ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
                        break;
                }
            }

            if (res.name == "ColUpButtonClick")
            {
                int col = (int)res.value;
                mono.StartCoroutine(slotMachineCtrl.NudgeReelOneStep(col, null, false, ReelNudgeDirection.Up));
            }

            if (res.name == "ColDownButtonClick")
            {
                int col = (int)res.value;
                mono.StartCoroutine(slotMachineCtrl.NudgeReelOneStep(col, null, false, ReelNudgeDirection.Down));
            }

        }

        /// <summary>
        /// 启动单次 Spin 游戏协程
        /// </summary>
        /// <param name="null"> null</param>
        /// <param name="null"> null</param>
        void StartGameOnce(Action successCallback = null, Action<string> errorCallback = null)
        {
            ContentModel.Instance.totalPlaySpins = 1;
            ContentModel.Instance.remainPlaySpins = 1;
            corGameOnce = mono.StartCoroutine(GameOnce(successCallback, errorCallback));
        }

        /// <summary>
        /// 启动自动 Spin 游戏协程
        /// </summary>
        /// <param name="null"> null</param>
        /// <param name="null"> null</param>
        void StartGameAuto(Action successCallback = null, Action<string> errorCallback = null)
        {
            if (coGameAuto != null) mono.StopCoroutine(coGameAuto);
            coGameAuto = mono.StartCoroutine(GameAuto(successCallback, errorCallback));
        }

        /// <summary>
        /// 自动 Spin 循环：重复执行 GameOnce 直至取消或出错
        /// </summary>
        /// <param name="successCallback"> 成功回调</param>
        /// <param name="errorCallback"> 失败回调</param>
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

        /// <summary>
        /// 切换普通局/免费局背景与边框 FGUI 可见性（0=普通，非0=免费）
        /// </summary>
        /// <param name="type"> 背景类型（0 普通 / 1 免费）</param>
        private void ChangeBGPanel(int type )
        {
            if (type == 0)
            {
                gFreeBg.visible = false;
                gFreeGameFrame.visible = false;
                gFreeInnerFrame.visible = false;
                gFreeTimeBox.visible = false;
                gNormalBg.visible = true;
                gNormalGameFrame.visible = true;
                gNormalInnerFrame.visible = true;
     
            }
            else
            {
                gNormalBg.visible = false;
                gNormalGameFrame.visible = false;
                gNormalInnerFrame.visible = false;
                gFreeTimeBox.visible = true;

                gFreeBg.visible = true;
                gFreeGameFrame.visible = true;
                gFreeInnerFrame.visible = true;
            }
        }

        //显示加速框
        /// <summary>
        /// 滚轴慢动作特效协程
        /// </summary>
        public IEnumerator ShowEffectReelsSlowMotion()
        {
            yield return new WaitUntil(() => isEffectSlowMotion2 == true);
        }

        /// <summary>
        /// 游戏异常时重置 Spin/Auto 状态并提示错误
        /// </summary>
        /// <param name="msg"> msg</param>
        private void StopGameWhenError(string msg)
        {
            ContentModel.Instance.isSpin = false;
            ContentModel.Instance.isAuto = false;
            ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
            ContentModel.Instance.gameState = GameState.Idle;

            // 有好酷优先用好酷
            if (false && SBoxModel.Instance.isUseIot && tipCoinIn)
            {

            }
            else
            {
                string massage = I18nMgr.T(msg);
                TipPopupHandler.Instance.OpenPopupOnce(I18nMgr.T(msg));
            }
        }


        /// <summary>
        /// 将彩金滚轮数据同步到 UI 与 ContentModel
        /// </summary>
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

        /// <summary>
        /// 更新免费次数 UI 显示
        /// </summary>
        /// <param name="freeSpinPlayTimes"> freeSpinPlayTimes</param>
        /// <param name="freeSpinTotalTimes"> freeSpinTotalTimes</param>
        protected void SetUIFreeTimeBox(int freeSpinPlayTimes, int freeSpinTotalTimes)
        {
            gFreeTimeBox.visible = true;
            gFreeTimeBox.GetChild("numberGreen").asTextField.text= freeSpinPlayTimes.ToString();
            gFreeTimeBox.GetChild("numberYellow").asTextField.text = freeSpinTotalTimes.ToString();
        }

        //读取当前滚轴显示的图标
        /// <summary>
        /// 获取当前可见滚轴盘面字符串
        /// </summary>
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
    }
}

