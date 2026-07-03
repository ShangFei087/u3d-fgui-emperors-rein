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

    public class PageGameMain : MachinePageBase
    {
        public const string pkgName = "SlotZhuZaiJinBi1700";
        public const string resName = "PageGameMain";

        private bool isInitPool = false; //资源池是否初始化
        private bool tipCoinIn = false; //提示硬币输入
        bool isAddCreditAnim => !(slotMachineCtrl.isStopImmediately == true || SBoxModel.Instance.isCoinOutImmediately);
        Coroutine corReelsTurn,corGameIdel, corGameOnce, corEffectSlowMotion, coGameAuto;
        /// <summary>PAG1~PAG12 按钮各自持有的播放协程；OnClose 或再次点击时须 Stop。</summary>
        Coroutine _corPagTest1, _corPagTest2, _corPagTest3, _corPagTest4;
        Coroutine _corPagTest5, _corPagTest6, _corPagTest7, _corPagTest8, _corPagTest9;
        Coroutine _corPagTest10, _corPagTest11, _corPagTest12;
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
        /// <summary>FGUI GLoader 名，对应 PAG1 / pagEffect1。</summary>
        private const string PagTestLoader1 = "pagEffect1";
        /// <summary>FGUI GLoader 名，对应 PAG2 / pagEffect2。</summary>
        private const string PagTestLoader2 = "pagEffect2";
        /// <summary>FGUI GLoader 名，对应 PAG3 / pagEffect3。</summary>
        private const string PagTestLoader3 = "pagEffect3";
        /// <summary>FGUI GLoader 名，对应 PAG4 三槽组播左槽 / pagEffect4。</summary>
        private const string PagTestLoader4 = "pagEffect4";
        /// <summary>FGUI GLoader 名，对应 PAG4 三槽组播中槽 / pagEffect5。</summary>
        private const string PagTestLoader5 = "pagEffect5";
        /// <summary>FGUI GLoader 名，对应 PAG4 三槽组播右槽 / pagEffect6。</summary>
        private const string PagTestLoader6 = "pagEffect6";
        /// <summary>FGUI GLoader 名，PAG5 glow / pagEffect7。</summary>
        private const string PagTestLoaderGlow5 = "pagEffect7";
        /// <summary>FGUI GLoader 名，PAG6 glow / pagEffect8。</summary>
        private const string PagTestLoaderGlow6 = "pagEffect8";
        /// <summary>FGUI GLoader 名，PAG7 glow / pagEffect9。</summary>
        private const string PagTestLoaderGlow7 = "pagEffect9";
        /// <summary>FGUI GLoader 名，PAG8 glow / pagEffect10。</summary>
        private const string PagTestLoaderGlow8 = "pagEffect10";
        /// <summary>FGUI GLoader 名，PAG9 glow / pagEffect11。</summary>
        private const string PagTestLoaderGlow9 = "pagEffect11";
        /// <summary>FGUI GLoader 名，PAG10 FreeNPC / pagEffect12。</summary>
        private const string PagTestLoaderNpc10 = "pagEffect12";
        /// <summary>FGUI GLoader 名，PAG11 NormalNPC / pagEffect13。</summary>
        private const string PagTestLoaderNpc11 = "pagEffect13";
        /// <summary>FGUI GLoader 名，PAG12 RewardNPC / pagEffect14。</summary>
        private const string PagTestLoaderNpc12 = "pagEffect14";
        private const int MaxPagTestNpcCount = 3;
        /// <summary>预制体内 Spine 节点名，与 PAG 同场景对照（jp_pup_grand）。</summary>
        private const string PagTestSpine1Node = "Spine Mecanim GameObject (jp_pup_grand)";
        /// <summary>预制体内 Spine 节点名，与 PAG 同场景对照（ng_pop_bigWin）。</summary>
        private const string PagTestSpine2Node = "Spine Mecanim GameObject (ng_pop_bigWin)";
        /// <summary>Spine1 按钮播放的动画名。</summary>
        private const string PagTestSpine1PlayAnim = "GRAND_in";
        /// <summary>Spine2 按钮播放的动画名。</summary>
        private const string PagTestSpine2PlayAnim = "bigwin_start";
        /// <summary>预制体内 Spine 节点名（jp_pup_GRAND）。</summary>
        private const string PagTestSpine3Node = "Spine Mecanim GameObject (jp_pup_GRAND)";
        /// <summary>预制体内 Spine 节点名（ng_bor_boom1）。</summary>
        private const string PagTestSpine4Node = "Spine Mecanim GameObject (ng_bor_boom1)";
        /// <summary>预制体内 Spine 节点名（ng_ic_bigwin）。</summary>
        private const string PagTestSpine5Node = "Spine Mecanim GameObject (ng_ic_bigwin)";
        /// <summary>Spine3 按钮播放的动画名。</summary>
        private const string PagTestSpine3PlayAnim = "in";
        /// <summary>Spine4 按钮播放的动画名。</summary>
        private const string PagTestSpine4PlayAnim = "start";
        /// <summary>Spine5 按钮播放的动画名。</summary>
        private const string PagTestSpine5PlayAnim = "bigwin_start";
        /// <summary>PAG1 槽位绑定，pagEffect1。</summary>
        private PagSlotBinding _pagTestSlot1;
        /// <summary>PAG2 槽位绑定，pagEffect2。</summary>
        private PagSlotBinding _pagTestSlot2;
        /// <summary>PAG3 槽位绑定，pagEffect3。</summary>
        private PagSlotBinding _pagTestSlot3;
        /// <summary>PAG4 BigWin 顺序播放槽，pagEffect4。</summary>
        private PagSlotBinding _pagTestSlot4;
        /// <summary>PAG4 组播中槽，pagEffect5。</summary>
        private PagSlotBinding _pagTestSlot5;
        /// <summary>PAG4 组播右槽，pagEffect6。</summary>
        private PagSlotBinding _pagTestSlot6;
        /// <summary>PAG5 glow 槽，pagEffect7。</summary>
        private PagSlotBinding _pagTestGlowSlot5;
        /// <summary>PAG6 glow 槽，pagEffect8。</summary>
        private PagSlotBinding _pagTestGlowSlot6;
        /// <summary>PAG7 glow 槽，pagEffect9。</summary>
        private PagSlotBinding _pagTestGlowSlot7;
        /// <summary>PAG8 glow 槽，pagEffect10。</summary>
        private PagSlotBinding _pagTestGlowSlot8;
        /// <summary>PAG9 glow 槽，pagEffect11。</summary>
        private PagSlotBinding _pagTestGlowSlot9;
        /// <summary>PAG10 FreeNPC 槽，pagEffect12。</summary>
        private PagSlotBinding _pagTestNpcSlot10;
        /// <summary>PAG11 NormalNPC 槽，pagEffect13。</summary>
        private PagSlotBinding _pagTestNpcSlot11;
        /// <summary>PAG12 RewardNPC 槽，pagEffect14。</summary>
        private PagSlotBinding _pagTestNpcSlot12;
        /// <summary>PAG1 是否正在播放（按钮二次点击为停止）。</summary>
        private bool _pagTest1Showing;
        /// <summary>PAG2 是否正在播放。</summary>
        private bool _pagTest2Showing;
        /// <summary>PAG3 是否正在播放。</summary>
        private bool _pagTest3Showing;
        /// <summary>PAG4 BigWin 顺序播放是否进行中。</summary>
        private bool _pagTest4Showing;
        /// <summary>PAG5~9 glow 是否正在播放。</summary>
        private bool _pagTest5Showing;
        private bool _pagTest6Showing;
        private bool _pagTest7Showing;
        private bool _pagTest8Showing;
        private bool _pagTest9Showing;
        private bool _pagTest10Showing;
        private bool _pagTest11Showing;
        private bool _pagTest12Showing;
        private readonly int[] _pagTestNpcSessionId = new int[MaxPagTestNpcCount];
        /// <summary>PAG1 对应 composition 是否已预热，避免重复磁盘+解码。</summary>
        private bool _pagTest1CacheWarmed;
        /// <summary>PAG2 对应 composition 是否已预热。</summary>
        private bool _pagTest2CacheWarmed;
        /// <summary>PAG3 对应 composition 是否已预热。</summary>
        private bool _pagTest3CacheWarmed;
        /// <summary>PAG4 对应 composition 是否已预热。</summary>
        private bool _pagTest4CacheWarmed;
        private bool _pagTest5CacheWarmed;
        private bool _pagTest6CacheWarmed;
        private bool _pagTest7CacheWarmed;
        private bool _pagTest8CacheWarmed;
        private bool _pagTest9CacheWarmed;
        private Animator _pagTestSpine1Animator;
        private SkeletonMecanim _pagTestSpine1Mecanim;
        private Animator _pagTestSpine2Animator;
        private SkeletonMecanim _pagTestSpine2Mecanim;
        private Animator _pagTestSpine3Animator;
        private SkeletonMecanim _pagTestSpine3Mecanim;
        private Animator _pagTestSpine4Animator;
        private SkeletonMecanim _pagTestSpine4Mecanim;
        private Animator _pagTestSpine5Animator;
        private SkeletonMecanim _pagTestSpine5Mecanim;
        /// <summary>Spine1 对照动画是否可见。</summary>
        private bool _spineTest1Showing;
        /// <summary>Spine2 对照动画是否可见。</summary>
        private bool _spineTest2Showing;
        /// <summary>Spine3 对照动画是否可见。</summary>
        private bool _spineTest3Showing;
        /// <summary>Spine4 对照动画是否可见。</summary>
        private bool _spineTest4Showing;
        /// <summary>Spine5 对照动画是否可见。</summary>
        private bool _spineTest5Showing;
        /// <summary>PAG2 / 进局过渡用 PAG 文件名。</summary>
        private const string PagTestName1 = "BigWin_1080.pag";
        /// <summary>PAG1 按钮播放的 PAG 文件名。</summary>
        private const string PagTestName2 = "Fade.pag";
        /// <summary>PAG3 按钮播放的 PAG 文件名。</summary>
        private const string PagTestName3 = "XingXing2.pag";
        /// <summary>进局自动序列（TryPlayPagTestOnEnter）的播放顺序。</summary>
        private static readonly string[] PagTestLoopSequence = { PagTestName1, PagTestName2, PagTestName3 };
        /// <summary>PAG4 BigWin 升级链顺序播放（各 repeat=1）。</summary>
        private static readonly string[] PagTestBigWinSequence =
        {
            "BigWin/bigwin_start1.pag",
            "BigWin/bigwin_idle1.pag",
            "BigWin/supwin_start1.pag",
            "BigWin/supwin_idle1.pag",
            "BigWin/megawin_start1.pag",
            "BigWin/megawin_idle1.pag",
        };
        private const string NpcPagFolderPrefix = "3997Npc/";
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
            NpcFreeSequence, NpcNormalSequence, NpcRewardSequence,
        };
        private static readonly string[] PagTestNpcLabels =
        {
            "freeNpc", "normalNpc", "rewardNpc",
        };
        private const string PagGlowLoop720 = "Lopp/glow_loop_720.pag";
        private const string PagGlowLoopHalf = "Lopp/glow_loop_half_1920.pag";
        private const string PagGlowLoopFull = "Lopp/glow_loop_full_1920.pag";
        private const string PagGlowInFull = "Lopp/glow_in_full_1920.pag";
        /// <summary>PAG5 glow_loop_720 FGUI 显示倍率（720 合成放大 1.5 倍至 1080 宽）。</summary>
        private const float PagGlow720DisplayScale = 1.5f;
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
        private bool _comboTestButtonsBound;
        private bool _comboP2S1Showing;
        private bool _comboP2S2Showing;
        private bool _comboP2S3Showing;
        private bool _comboP2S4Showing;
        private bool _comboP2S5Showing;
        private bool _comboP3S1Showing;
        private bool _comboP3S2Showing;
        private bool _comboP3S3Showing;
        private bool _comboP3S4Showing;
        private bool _comboP3S5Showing;
        private bool _comboS2E1E2Showing;
        private bool _comboS1E1E2Showing;
        private bool _comboP9S1Showing;
        private bool _comboP9S2Showing;
        private bool _comboP9S3Showing;
        private bool _comboP9S4Showing;
        private bool _comboP9S5Showing;
        private bool _comboEffectAllShowing;
        private bool _comboSpineAllShowing;
        private bool _comboP4S1Showing;
        private bool _comboP4S2Showing;
        private bool _comboP4S3Showing;
        private bool _comboP4S4Showing;
        private bool _comboP4S5Showing;
        private Coroutine _corComboPag2Spine;
        private Coroutine _corComboPag3Spine;
        private static readonly string[] ComboTestButtonNames =
        {
            "P2S1", "P2S2", "P2S3", "P2S4", "P2S5",
            "P3S1", "P3S2", "P3S3", "P3S4", "P3S5",
            "S2E1E2", "S1E1E2",
            "P9S1", "P9S2", "P9S3", "P9S4", "P9S5",
            "Effect1_5", "Spine1_5",
            "P4S1", "P4S2", "P4S3", "P4S4", "P4S5",
        };
        //免费组件
        private GComponent gFreeTimeBox, gFreeWinBox;
        private GComponent gFreeSlotMachine;
        //彩金
        MiniReelGroup uiJPMajorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMinorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMiniCtrl = new MiniReelGroup();
        long TotalBet => (long)MainModel.Instance.contentMD.totalBet;

        /// <summary>1700：底部 Panel 异步就绪后触发 PageManager 的 preLoadedCallback。</summary>
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
        protected override void OnLanguageChange(I18nLang lang)
        {
            ClearBorderMegaWinButtons();
            ClearComboTestButtons();
            ClearPagTestButtons(); // 语言切换重建 UI 前解绑 PAG 测试按钮
            FguiI18nTextAssistant.Instance.DisposeAllTranslate(this.contentPane);
            this.contentPane.Dispose(); // 释放当前UI
            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            InitParam(null);
            //FguiI18nTextAssistant.Instance.TranslateComponent(this.contentPane);
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
            EventCenter.Instance.AddEventListener<WinJackpotInfo>(GlobalEvent.JackpotOnlineWin, OnJackpotOnLine);
            GameSoundHelper.Instance.PlayMusicSingle(SoundKey.RegularBG);
            InitParam(data);
        }
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
            ClearComboTestButtons();
            ClearPagTestButtons();
            DisposeBorderMegaWinEffects();
            DisposePagTestResources();
            if (goGameCtrl != null && goGameCtrl.activeSelf)
            {
                goGameCtrl.SetActive(false);
            }
            base.OnClose(data);
        }
        private void OnCoinPushSpinResultParse(CoinPushSpinParseEventArgs e)
        {
            e.Result = MachineDataG1700Controller.ParseCoinPushSpinPayload(e.Data, e.StartPos);
        }

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
                _pagTestSlot1?.Dispose();
                _pagTestSlot1 = null;
                _pagTestSlot2?.Dispose();
                _pagTestSlot2 = null;
                _pagTestSlot3?.Dispose();
                _pagTestSlot3 = null;
                _pagTestSlot4?.Dispose();
                _pagTestSlot4 = null;
                _pagTestSlot5?.Dispose();
                _pagTestSlot5 = null;
                _pagTestSlot6?.Dispose();
                _pagTestSlot6 = null;
                _pagTestGlowSlot5?.Dispose();
                _pagTestGlowSlot5 = null;
                _pagTestGlowSlot6?.Dispose();
                _pagTestGlowSlot6 = null;
                _pagTestGlowSlot7?.Dispose();
                _pagTestGlowSlot7 = null;
                _pagTestGlowSlot8?.Dispose();
                _pagTestGlowSlot8 = null;
                _pagTestGlowSlot9?.Dispose();
                _pagTestGlowSlot9 = null;
                _pagTestNpcSlot10?.Dispose();
                _pagTestNpcSlot10 = null;
                _pagTestNpcSlot11?.Dispose();
                _pagTestNpcSlot11 = null;
                _pagTestNpcSlot12?.Dispose();
                _pagTestNpcSlot12 = null;
                _pagTestSpine1Animator = null;
                _pagTestSpine1Mecanim = null;
                _pagTestSpine2Animator = null;
                _pagTestSpine2Mecanim = null;
                _pagTestSpine3Animator = null;
                _pagTestSpine3Mecanim = null;
                _pagTestSpine4Animator = null;
                _pagTestSpine4Mecanim = null;
                _pagTestSpine5Animator = null;
                _pagTestSpine5Mecanim = null;
                _spineTest1Showing = false;
                _spineTest2Showing = false;
                _spineTest3Showing = false;
                _spineTest4Showing = false;
                _spineTest5Showing = false;
                _pagTest3Showing = false;
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
            BindComboTestButtons();

            uiJPMajorCtrl.Init("Major", contentPane.GetChild("jpMajor").asCom.GetChild("reels").asList, "N0");
            uiJPMinorCtrl.Init("Minor", contentPane.GetChild("jpMinor").asCom.GetChild("reels").asList, "N0");
            uiJPMiniCtrl.Init("Mini", contentPane.GetChild("jpMini").asCom.GetChild("reels").asList, "N0");

            uiJPMajorCtrl.SetData(0);
            uiJPMinorCtrl.SetData(0);
            uiJPMiniCtrl.SetData(0);
            ChangeBGPanel(0);
            ContentModel.Instance.totalBet = SBoxModel.Instance.betList[ContentModel.Instance.betIndex];

            TryRestoreFreeSpinSession();
        }

        /// <summary>InitParam 时创建 6 个 PagSlotBinding 并 Attach 到 anchorPagTest 下 pagEffect1~6。</summary>
        private void EnsurePagTestSlots()
        {
            PagConcurrentPlayback.Enabled = PagTestUseFguiTexture;
            PagController.AutoConcurrentGpuSync = PagTestUseGpuSyncGroup;

            GComponent anchor = GetPagTestAnchor();
            if (anchor == null)
            {
                Debug.LogWarning($"{PagLogPrefix} EnsurePagTestSlots skipped: anchor null");
                return;
            }

            if (_pagTestSlot1 == null)
            {
                _pagTestSlot1 = new PagSlotBinding("PagTest1");
                Debug.Log($"{PagLogPrefix} PagSlotBinding created for PagTest1");
            }

            if (_pagTestSlot2 == null)
            {
                _pagTestSlot2 = new PagSlotBinding("PagTest2");
                Debug.Log($"{PagLogPrefix} PagSlotBinding created for PagTest2");
            }

            if (_pagTestSlot3 == null)
            {
                _pagTestSlot3 = new PagSlotBinding("PagTest3");
                Debug.Log($"{PagLogPrefix} PagSlotBinding created for PagTest3");
            }

            EnsurePagTestSlot(_pagTestSlot1, PagTestLoader1, "PagTest1");
            EnsurePagTestSlot(_pagTestSlot2, PagTestLoader2, "PagTest2");
            EnsurePagTestSlot(_pagTestSlot3, PagTestLoader3, "PagTest3");

            if (_pagTestSlot4 == null)
            {
                _pagTestSlot4 = new PagSlotBinding("PagTest4");
                Debug.Log($"{PagLogPrefix} PagSlotBinding created for PagTest4");
            }

            if (_pagTestSlot5 == null)
            {
                _pagTestSlot5 = new PagSlotBinding("PagTest5");
                Debug.Log($"{PagLogPrefix} PagSlotBinding created for PagTest5");
            }

            if (_pagTestSlot6 == null)
            {
                _pagTestSlot6 = new PagSlotBinding("PagTest6");
                Debug.Log($"{PagLogPrefix} PagSlotBinding created for PagTest6");
            }

            EnsurePagTestSlot(_pagTestSlot4, PagTestLoader4, "PagTest4");
            EnsurePagTestSlot(_pagTestSlot5, PagTestLoader5, "PagTest5");
            EnsurePagTestSlot(_pagTestSlot6, PagTestLoader6, "PagTest6");

            EnsurePagTestGlowSlot(5, ref _pagTestGlowSlot5, PagTestLoaderGlow5, "PagTestGlow5");
            EnsurePagTestGlowSlot(6, ref _pagTestGlowSlot6, PagTestLoaderGlow6, "PagTestGlow6");
            EnsurePagTestGlowSlot(7, ref _pagTestGlowSlot7, PagTestLoaderGlow7, "PagTestGlow7");
            EnsurePagTestGlowSlot(8, ref _pagTestGlowSlot8, PagTestLoaderGlow8, "PagTestGlow8");
            EnsurePagTestGlowSlot(9, ref _pagTestGlowSlot9, PagTestLoaderGlow9, "PagTestGlow9");

            EnsurePagTestNpcSlot(0, ref _pagTestNpcSlot10, PagTestLoaderNpc10, "PagTestNpc10");
            EnsurePagTestNpcSlot(1, ref _pagTestNpcSlot11, PagTestLoaderNpc11, "PagTestNpc11");
            EnsurePagTestNpcSlot(2, ref _pagTestNpcSlot12, PagTestLoaderNpc12, "PagTestNpc12");
        }

        private void EnsurePagTestNpcSlot(int npcIndex, ref PagSlotBinding slot, string loaderName, string instanceLabel)
        {
            if (slot == null)
            {
                slot = new PagSlotBinding(instanceLabel);
                Debug.Log($"{PagLogPrefix} PagSlotBinding created for {instanceLabel} (PAG{npcIndex + 10} NPC)");
            }

            EnsurePagTestSlot(slot, loaderName, instanceLabel);
        }

        private void EnsurePagTestNpcSlotByIndex(int npcIndex)
        {
            PagConcurrentPlayback.Enabled = PagTestUseFguiTexture;
            PagController.AutoConcurrentGpuSync = PagTestUseGpuSyncGroup;

            if (GetPagTestAnchor() == null)
            {
                Debug.LogWarning($"{PagLogPrefix} EnsurePagTestNpcSlotByIndex skipped: anchor null npcIndex={npcIndex}");
                return;
            }

            switch (npcIndex)
            {
                case 0:
                    EnsurePagTestNpcSlot(0, ref _pagTestNpcSlot10, PagTestLoaderNpc10, "PagTestNpc10");
                    break;
                case 1:
                    EnsurePagTestNpcSlot(1, ref _pagTestNpcSlot11, PagTestLoaderNpc11, "PagTestNpc11");
                    break;
                case 2:
                    EnsurePagTestNpcSlot(2, ref _pagTestNpcSlot12, PagTestLoaderNpc12, "PagTestNpc12");
                    break;
                default:
                    Debug.LogWarning($"{PagLogPrefix} EnsurePagTestNpcSlotByIndex skipped: invalid npcIndex={npcIndex}");
                    break;
            }
        }

        private PagSlotBinding GetPagTestNpcSlot(int npcIndex)
        {
            switch (npcIndex)
            {
                case 0: return _pagTestNpcSlot10;
                case 1: return _pagTestNpcSlot11;
                case 2: return _pagTestNpcSlot12;
                default: return null;
            }
        }

        private void EnsurePagTestGlowSlot(int glowIndex, ref PagSlotBinding slot, string loaderName, string instanceLabel)
        {
            if (slot == null)
            {
                slot = new PagSlotBinding(instanceLabel);
                Debug.Log($"{PagLogPrefix} PagSlotBinding created for {instanceLabel} (PAG{glowIndex} glow)");
            }

            EnsurePagTestSlot(slot, loaderName, instanceLabel);
        }

        /// <summary>Play 前只绑定当前 glow 槽，避免全量 EnsurePagTestSlots re-Attach 误伤同屏已播成员。</summary>
        private void EnsurePagTestGlowSlotByIndex(int glowIndex)
        {
            PagConcurrentPlayback.Enabled = PagTestUseFguiTexture;
            PagController.AutoConcurrentGpuSync = PagTestUseGpuSyncGroup;

            if (GetPagTestAnchor() == null)
            {
                Debug.LogWarning($"{PagLogPrefix} EnsurePagTestGlowSlotByIndex skipped: anchor null glowIndex={glowIndex}");
                return;
            }

            switch (glowIndex)
            {
                case 5:
                    EnsurePagTestGlowSlot(5, ref _pagTestGlowSlot5, PagTestLoaderGlow5, "PagTestGlow5");
                    break;
                case 6:
                    EnsurePagTestGlowSlot(6, ref _pagTestGlowSlot6, PagTestLoaderGlow6, "PagTestGlow6");
                    break;
                case 7:
                    EnsurePagTestGlowSlot(7, ref _pagTestGlowSlot7, PagTestLoaderGlow7, "PagTestGlow7");
                    break;
                case 8:
                    EnsurePagTestGlowSlot(8, ref _pagTestGlowSlot8, PagTestLoaderGlow8, "PagTestGlow8");
                    break;
                case 9:
                    EnsurePagTestGlowSlot(9, ref _pagTestGlowSlot9, PagTestLoaderGlow9, "PagTestGlow9");
                    break;
                default:
                    Debug.LogWarning($"{PagLogPrefix} EnsurePagTestGlowSlotByIndex skipped: invalid glowIndex={glowIndex}");
                    break;
            }
        }

        /// <summary>PAG5~9 glowIndex 返回对应 PagSlotBinding；无效 index 返回 null。</summary>
        private PagSlotBinding GetPagTestGlowSlot(int glowIndex)
        {
            switch (glowIndex)
            {
                case 5: return _pagTestGlowSlot5;
                case 6: return _pagTestGlowSlot6;
                case 7: return _pagTestGlowSlot7;
                case 8: return _pagTestGlowSlot8;
                case 9: return _pagTestGlowSlot9;
                default: return null;
            }
        }

        /// <summary>将 PagSlotBinding 挂到 anchor 上指定 loaderName 的 GLoader。</summary>
        private void EnsurePagTestSlot(PagSlotBinding slot, string loaderName, string instanceLabel)
        {
            GComponent anchor = GetPagTestAnchor();
            if (anchor == null)
            {
                Debug.LogWarning($"{PagLogPrefix} EnsurePagTestSlot skipped: anchor null, instance={instanceLabel}");
                return;
            }

            if (slot == null)
            {
                Debug.LogWarning($"{PagLogPrefix} EnsurePagTestSlot skipped: slot null, instance={instanceLabel}");
                return;
            }

            slot.Attach(anchor, loaderName);
        }

        /// <summary>OnClose 时释放 PagSlotBinding 与 Spine 引用，重置播放/预热状态。</summary>
        private void DisposePagTestResources()
        {
            _pagTestSlot1?.Dispose();
            _pagTestSlot1 = null;
            _pagTestSlot2?.Dispose();
            _pagTestSlot2 = null;
            _pagTestSlot3?.Dispose();
            _pagTestSlot3 = null;
            _pagTestSlot4?.Dispose();
            _pagTestSlot4 = null;
            _pagTestSlot5?.Dispose();
            _pagTestSlot5 = null;
            _pagTestSlot6?.Dispose();
            _pagTestSlot6 = null;
            _pagTestGlowSlot5?.Dispose();
            _pagTestGlowSlot5 = null;
            _pagTestGlowSlot6?.Dispose();
            _pagTestGlowSlot6 = null;
            _pagTestGlowSlot7?.Dispose();
            _pagTestGlowSlot7 = null;
            _pagTestGlowSlot8?.Dispose();
            _pagTestGlowSlot8 = null;
            _pagTestGlowSlot9?.Dispose();
            _pagTestGlowSlot9 = null;
            _pagTestNpcSlot10?.Dispose();
            _pagTestNpcSlot10 = null;
            _pagTestNpcSlot11?.Dispose();
            _pagTestNpcSlot11 = null;
            _pagTestNpcSlot12?.Dispose();
            _pagTestNpcSlot12 = null;
            _pagTestSpine1Animator = null;
            _pagTestSpine1Mecanim = null;
            _pagTestSpine2Animator = null;
            _pagTestSpine2Mecanim = null;
            _pagTestSpine3Animator = null;
            _pagTestSpine3Mecanim = null;
            _pagTestSpine4Animator = null;
            _pagTestSpine4Mecanim = null;
            _pagTestSpine5Animator = null;
            _pagTestSpine5Mecanim = null;
            _pagTest1Showing = false;
            _pagTest2Showing = false;
            _pagTest3Showing = false;
            _pagTest4Showing = false;
            _pagTest5Showing = false;
            _pagTest6Showing = false;
            _pagTest7Showing = false;
            _pagTest8Showing = false;
            _pagTest9Showing = false;
            _pagTest10Showing = false;
            _pagTest11Showing = false;
            _pagTest12Showing = false;
            for (int i = 0; i < MaxPagTestNpcCount; i++)
            {
                _pagTestNpcSessionId[i] = 0;
            }
            _spineTest1Showing = false;
            _spineTest2Showing = false;
            _spineTest3Showing = false;
            _spineTest4Showing = false;
            _spineTest5Showing = false;
            ResetComboTestShowingFlags();
            _pagTest1CacheWarmed = false;
            _pagTest2CacheWarmed = false;
            _pagTest3CacheWarmed = false;
            _pagTest4CacheWarmed = false;
            _pagTest5CacheWarmed = false;
            _pagTest6CacheWarmed = false;
            _pagTest7CacheWarmed = false;
            _pagTest8CacheWarmed = false;
            _pagTest9CacheWarmed = false;
        }

        /// <summary>中断 PAG4 顺序播放协程并停止 pagEffect4。</summary>
        private void StopPagTest4Playback()
        {
            if (_corPagTest4 != null && mono != null)
            {
                Debug.Log($"{PagLogPrefix} PAG4 sequence aborted");
                mono.StopCoroutine(_corPagTest4);
                _corPagTest4 = null;
            }

            StopPagTest(_pagTestSlot4, ref _pagTest4Showing);
        }

        /// <summary>中断 PAG4 静态组播遗留并停止 slot5~6；不 EndGroup 动态合组（PAG1~3 同屏）。</summary>
        private void StopPagTestGroupPlayback()
        {
            StopPagTest4Playback();

            PagGpuSyncGroup.EndStaticGroupIfActive("StopPagTestGroupPlayback");
            _pagTestSlot5?.Stop(PagTestUseFguiTexture);
            _pagTestSlot6?.Stop(PagTestUseFguiTexture);
        }

        /// <summary>停止 PAG4 组播三槽（pagEffect5~6）的 Native/FGUI 播放。</summary>
        private void StopPagTestGroupSlots()
        {
            _pagTestSlot5?.Stop(PagTestUseFguiTexture);
            _pagTestSlot6?.Stop(PagTestUseFguiTexture);
        }

        /// <summary>停止单路 PAG5~9 glow 协程与对应 pagEffect 播放。</summary>
        private void StopPagTestGlow(int glowIndex)
        {
            switch (glowIndex)
            {
                case 5:
                    StopPagTestGlowCoroutine(ref _corPagTest5, ref _pagTest5Showing);
                    break;
                case 6:
                    StopPagTestGlowCoroutine(ref _corPagTest6, ref _pagTest6Showing);
                    break;
                case 7:
                    StopPagTestGlowCoroutine(ref _corPagTest7, ref _pagTest7Showing);
                    break;
                case 8:
                    StopPagTestGlowCoroutine(ref _corPagTest8, ref _pagTest8Showing);
                    break;
                case 9:
                    StopPagTestGlowCoroutine(ref _corPagTest9, ref _pagTest9Showing);
                    break;
                default:
                    return;
            }

            GetPagTestGlowSlot(glowIndex)?.Stop(PagTestUseFguiTexture);
        }

        /// <summary>停止 PAG5~9 全部 glow 协程与 pagEffect7~11 播放。</summary>
        private void StopPagTestGlowPlayback()
        {
            for (int glowIndex = 5; glowIndex <= 9; glowIndex++)
            {
                StopPagTestGlow(glowIndex);
            }
        }

        private void StopPagTestGlowCoroutine(ref Coroutine coroutine, ref bool showingFlag)
        {
            if (coroutine != null && mono != null)
            {
                mono.StopCoroutine(coroutine);
                coroutine = null;
            }

            showingFlag = false;
        }

        /// <summary>组播前为单槽设置统一 displayScale 与 clampToHolder。</summary>
        private void ConfigurePagTestGroupSlot(PagSlotBinding slot)
        {
            if (slot == null)
            {
                return;
            }

            slot.SetFguiDisplayScale(PagTestDisplayScale);
            slot.SetFguiClampDisplayToHolder(PagTestClampDisplayToHolder);
        }

        private static readonly PagSlotBinding[] PagTestGroupTripleSlots = new PagSlotBinding[3];

        /// <summary>返回 PAG4 三槽组播用的 slot4~6 数组（复用静态缓冲避免 GC）。</summary>
        private PagSlotBinding[] GetPagTestGroupSlots()
        {
            PagTestGroupTripleSlots[0] = _pagTestSlot4;
            PagTestGroupTripleSlots[1] = _pagTestSlot5;
            PagTestGroupTripleSlots[2] = _pagTestSlot6;
            return PagTestGroupTripleSlots;
        }

        /// <summary>PagGroupPlayer 布局回调包装，当前与全局 TryBuildPagTestLayoutExtra 一致。</summary>
        private bool TryBuildPagTestLayoutExtraForAnchor(GComponent anchor, out string extra, out string debugReason)
        {
            return TryBuildPagTestLayoutExtra(out extra, out debugReason);
        }

        /// <summary>OnClose 时停止全部 PAG 协程、组播与 slot1~3 播放。</summary>
        private void StopAllPagTest()
        {
            if (_corPagTest1 != null && mono != null)
            {
                Debug.Log($"{PagLogPrefix} sequence aborted reason=OnClose slot=1");
                mono.StopCoroutine(_corPagTest1);
                _corPagTest1 = null;
            }

            if (_corPagTest2 != null && mono != null)
            {
                Debug.Log($"{PagLogPrefix} sequence aborted reason=OnClose slot=2");
                mono.StopCoroutine(_corPagTest2);
                _corPagTest2 = null;
            }

            if (_corPagTest3 != null && mono != null)
            {
                Debug.Log($"{PagLogPrefix} sequence aborted reason=OnClose slot=3");
                mono.StopCoroutine(_corPagTest3);
                _corPagTest3 = null;
            }

            if (_corPagTest4 != null && mono != null)
            {
                Debug.Log($"{PagLogPrefix} sequence aborted reason=OnClose slot=4");
                mono.StopCoroutine(_corPagTest4);
                _corPagTest4 = null;
            }

            StopPagTestGroupPlayback();

            StopPagTestGlowPlayback();

            StopAllPagTestNpcPlayback();

            StopPagTest(_pagTestSlot1, ref _pagTest1Showing);
            StopPagTest(_pagTestSlot2, ref _pagTest2Showing);
            StopPagTest(_pagTestSlot3, ref _pagTest3Showing);
        }

        /// <summary>获取 anchorPagTest；优先缓存的 _anchorPagTest，否则从 contentPane 查找。</summary>
        private GComponent GetPagTestAnchor()
        {
            if (_anchorPagTest != null)
            {
                return _anchorPagTest;
            }

            return contentPane?.GetChild("anchorPagTest")?.asCom;
        }

        /// <summary>
        /// 将 anchorPagTest 区域换算为 Native overlay 的 extra（x,y,w,h 为相对屏幕 0~1）。
        /// </summary>
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

        /// <summary>Overlay 模式下按 PAG 文件名返回等待时长兜底（秒）。</summary>
        private float GetPagTestDurationFallback(string pagFileName)
        {
            return pagFileName == PagTestName2 ? PagTestNezaPagDuration : PagTestDuration;
        }

        /// <summary>Play 开始后读取 Native composition frameRate，与 Unity 出帧节流对齐。</summary>
        private IEnumerator TryAlignPagTestFpsAfterPlayStarted(PagSlotBinding slot)
        {
            PagController controller = slot?.Controller;
            if (controller == null)
            {
                yield break;
            }

            // 同屏多 PAG 时统一 PagTestFguiFps，避免各路 Java tick 与 SyncGroup 组节流错位导致 batch stall。
            if (PagConcurrentPlayback.Enabled)
            {
                yield break;
            }

            yield return controller.WaitForPlayStarted(PagTestPlayStartedTimeoutSec);
            int nativeFps = controller.GetCompositionFrameRate();
            if (nativeFps <= 0)
            {
                yield break;
            }

            if (nativeFps == PagTestFguiFps)
            {
                Debug.Log($"{PagLogPrefix} FGUI fps aligned with composition: {nativeFps}, instance={slot.InstanceKey}");
                yield break;
            }

            slot.ConfigureFgui(PagTestFguiMaxDisplaySide, nativeFps);
            Debug.Log($"{PagLogPrefix} aligned FGUI fps {PagTestFguiFps}->{nativeFps}, instance={slot.InstanceKey}");
        }

        /// <summary>检查 PagCache 磁盘文件与 Java composition 缓存是否均已就绪。</summary>
        private static bool IsPagCompositionReady(string pagFileName)
        {
            if (!PagPathHelper.IsCached(pagFileName))
            {
                return false;
            }

            string absPath = PagController.ResolvePagPath(pagFileName, PagPathHelper.DefaultGamePagFolder);
            return PagController.IsCompositionCached(absPath);
        }

        /// <summary>Loading 已预热则秒过；否则磁盘 + Java composition 兜底预加载。</summary>
        private IEnumerator EnsurePagTestCompositionReady(string pagFileName, bool alreadyWarmed, Action<bool> onDone)
        {
            if (alreadyWarmed && IsPagCompositionReady(pagFileName))
            {
                onDone?.Invoke(true);
                yield break;
            }

            if (IsPagCompositionReady(pagFileName))
            {
                onDone?.Invoke(true);
                yield break;
            }

            yield return PagController.PreloadCompositionCoroutine(pagFileName);
            onDone?.Invoke(IsPagCompositionReady(pagFileName));
        }

        /// <summary>
        /// 单槽 PAG 播放入口（调用前须已通过 EnsurePagTestSlots 完成 Attach）：
        /// 解析路径 → 计算 Overlay layoutExtra → FGUI 或 Overlay 分支 → PlayPag。
        /// </summary>
        private void PlayPagTest(PagSlotBinding slot, string pagFileName, int repeatCount = 1, float displayScale = PagTestDisplayScale)
        {
            Debug.Log($"{PagLogPrefix} PlayPagTest start: instance={slot?.InstanceKey}, {pagFileName}, repeat={repeatCount}, scale={displayScale}");
            PagController controller = slot?.Controller;
            if (controller == null)
            {
                Debug.LogError($"{PagLogPrefix} PlayPagTest failed: PagController is null, instance={slot?.InstanceKey}");
                return;
            }

            string resolvedPath = controller.ResolvePagPath(pagFileName);
            if (string.IsNullOrEmpty(resolvedPath))
            {
                Debug.LogError($"{PagLogPrefix} PlayPagTest failed: resolve path null, file={pagFileName}, instance={slot.InstanceKey}");
                return;
            }

            Debug.Log($"{PagLogPrefix} resolved path: {resolvedPath}, exists={System.IO.File.Exists(resolvedPath)}, instance={slot.InstanceKey}");

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

        /// <summary>停止单槽 PAG 播放并清除 showing 标志。</summary>
        private void StopPagTest(PagSlotBinding slot, ref bool showingFlag)
        {
            if (slot?.Controller == null)
            {
                Debug.LogWarning($"{PagLogPrefix} StopPagTest skipped: PagController is null, instance={slot?.InstanceKey}");
                showingFlag = false;
                return;
            }

            slot.Stop(PagTestUseFguiTexture);
            showingFlag = false;

            Debug.Log($"{PagLogPrefix} StopPagTest instance={slot.InstanceKey}");
        }

        /// <summary>进局自动播 PAG 序列入口（当前未被调用）；PagTestLoop 控制是否无限交替循环。</summary>
        private void TryPlayPagTestOnEnter()
        {
            if (!isInit || mono == null || slotMachineCtrl == null)
            {
                Debug.LogWarning($"{PagLogPrefix} TryPlayPagTestOnEnter skipped: isInit={isInit}, mono={mono != null}, slotMachineCtrl={slotMachineCtrl != null}");
                return;
            }

            Debug.Log($"{PagLogPrefix} TryPlayPagTestOnEnter: loop={PagTestLoop}, sequence=[{string.Join(", ", PagTestLoopSequence)}]");

            if (_corPagTest1 != null)
            {
                Debug.Log($"{PagLogPrefix} sequence aborted reason=restart");
                mono.StopCoroutine(_corPagTest1);
            }

            _corPagTest1 = mono.StartCoroutine(PlayPagTestEnterSequence());
        }

        /// <summary>进局协程：预加载序列 → 可选循环 / 单次 BigWin_1024 → 停止并卸资源。</summary>
        private IEnumerator PlayPagTestEnterSequence()
        {
            Debug.Log($"{PagLogPrefix} PlayPagTestEnterSequence start");

            for (int i = 0; i < PagTestLoopSequence.Length; i++)
            {
                yield return PagController.PreloadCompositionCoroutine(PagTestLoopSequence[i]);
            }

            if (PagTestLoop)
            {
                int loopIndex = 0;
                while (true)
                {
                    string pagFileName = PagTestLoopSequence[0];
                    PlayPagTest(_pagTestSlot1, pagFileName, -1);
                    yield return WaitPagTestPlayStarted(_pagTestSlot1, PagTestPlayStartedTimeoutSec);
                    PagController controller = _pagTestSlot1?.Controller;
                    if (controller == null || !controller.PlayStarted)
                    {
                        Debug.LogError($"{PagLogPrefix} {pagFileName} play did not start within {PagTestPlayStartedTimeoutSec}s");
                        Debug.Log($"{PagLogPrefix} sequence aborted reason=pag_play_started_timeout pag={pagFileName}");
                        _corPagTest1 = null;
                        yield break;
                    }

                    if (PagTestUseFguiTexture)
                    {
                        float durationFallback = GetPagTestDurationFallback(pagFileName);
                        float pagTimeout = controller.GetCompositionDurationSecWithFallback(durationFallback) + 3f;
                        yield return controller.WaitForPlaybackFinished(pagTimeout);
                    }
                    else
                    {
                        yield return slotMachineCtrl.SlotWaitForSeconds(GetPagTestDurationFallback(pagFileName));
                    }

                    loopIndex = (loopIndex + 1) % PagTestLoopSequence.Length;
                    Debug.Log($"{PagLogPrefix} loop next: {PagTestLoopSequence[loopIndex]}");
                }
            }

            PlayPagTest(_pagTestSlot1, PagTestName1);
            yield return WaitPagTestPlayStarted(_pagTestSlot1, PagTestPlayStartedTimeoutSec);
            PagController transitionController = _pagTestSlot1?.Controller;
            if (transitionController == null || !transitionController.PlayStarted)
            {
                Debug.LogError($"{PagLogPrefix} {PagTestName1} play did not start within {PagTestPlayStartedTimeoutSec}s");
                Debug.Log($"{PagLogPrefix} sequence aborted reason=pag_play_started_timeout");
                _corPagTest1 = null;
                yield break;
            }

            if (PagTestUseFguiTexture)
            {
                float pagTimeout = transitionController.GetCompositionDurationSecWithFallback(PagTestDuration) + 3f;
                yield return transitionController.WaitForPlaybackFinished(pagTimeout);
            }
            else
            {
                yield return slotMachineCtrl.SlotWaitForSeconds(PagTestDuration);
            }

            StopPagTest(_pagTestSlot1, ref _pagTest1Showing);
            yield return PagPathHelper.DeferredUnloadUnusedAssets();
            _corPagTest1 = null;
            Debug.Log($"{PagLogPrefix} PlayPagTestEnterSequence finished");
        }

        /// <summary>轮询直到 PagController.PlayStarted 或超时。</summary>
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

        /// <summary>绑定 PageGameMain 上 PAG1~12、Spine1~5 测试按钮（InitParam / 语言切换后）。</summary>
        private void BindPagTestButtons()
        {
            if (_pagTestButtonsBound || contentPane == null)
            {
                return;
            }

            GButton btnPag1 = contentPane.GetChild("PAG1")?.asButton;
            if (btnPag1 != null)
            {
                btnPag1.onClick.Clear();
                btnPag1.onClick.Add(OnClickPagTest1Button);
            }
            else
            {
                Debug.LogWarning($"{PagLogPrefix} button missing: PAG1");
            }

            GButton btnPag2 = contentPane.GetChild("PAG2")?.asButton;
            if (btnPag2 != null)
            {
                btnPag2.onClick.Clear();
                btnPag2.onClick.Add(OnClickPagTest2Button);
            }
            else
            {
                Debug.LogWarning($"{PagLogPrefix} button missing: PAG2");
            }

            GButton btnPag3 = contentPane.GetChild("PAG3")?.asButton;
            if (btnPag3 != null)
            {
                btnPag3.onClick.Clear();
                btnPag3.onClick.Add(OnClickPagTest3Button);
            }
            else
            {
                Debug.LogWarning($"{PagLogPrefix} button missing: PAG3");
            }

            GButton btnPag4 = contentPane.GetChild("PAG4")?.asButton;
            if (btnPag4 != null)
            {
                btnPag4.onClick.Clear();
                btnPag4.onClick.Add(OnClickPagTest4Button);
            }
            else
            {
                Debug.LogWarning($"{PagLogPrefix} button missing: PAG4");
            }

            BindPagTestGlowButton("PAG5", OnClickPagTest5Button);
            BindPagTestGlowButton("PAG6", OnClickPagTest6Button);
            BindPagTestGlowButton("PAG7", OnClickPagTest7Button);
            BindPagTestGlowButton("PAG8", OnClickPagTest8Button);
            BindPagTestGlowButton("PAG9", OnClickPagTest9Button);
            BindPagTestGlowButton("PAG10", OnClickPagTest10Button);
            BindPagTestGlowButton("PAG11", OnClickPagTest11Button);
            BindPagTestGlowButton("PAG12", OnClickPagTest12Button);

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

        /// <summary>OnClose / OnLanguageChange 前清除 PAG 与 Spine 测试按钮点击监听。</summary>
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

        /// <summary>在 _clonePagTest 上查找并初始化 Spine1~5 对照节点。</summary>
        private void EnsurePagTestSpines()
        {
            if (_clonePagTest == null)
            {
                return;
            }

            EnsurePagTestSpine(ref _pagTestSpine1Animator, ref _pagTestSpine1Mecanim, PagTestSpine1Node, 1);
            EnsurePagTestSpine(ref _pagTestSpine2Animator, ref _pagTestSpine2Mecanim, PagTestSpine2Node, 2);
            EnsurePagTestSpine(ref _pagTestSpine3Animator, ref _pagTestSpine3Mecanim, PagTestSpine3Node, 3);
            EnsurePagTestSpine(ref _pagTestSpine4Animator, ref _pagTestSpine4Mecanim, PagTestSpine4Node, 4);
            EnsurePagTestSpine(ref _pagTestSpine5Animator, ref _pagTestSpine5Mecanim, PagTestSpine5Node, 5);
        }

        private bool TryGetPagTestSpine(int spineIndex, out Animator animator, out SkeletonMecanim mecanim)
        {
            animator = null;
            mecanim = null;
            switch (spineIndex)
            {
                case 1:
                    animator = _pagTestSpine1Animator;
                    mecanim = _pagTestSpine1Mecanim;
                    break;
                case 2:
                    animator = _pagTestSpine2Animator;
                    mecanim = _pagTestSpine2Mecanim;
                    break;
                case 3:
                    animator = _pagTestSpine3Animator;
                    mecanim = _pagTestSpine3Mecanim;
                    break;
                case 4:
                    animator = _pagTestSpine4Animator;
                    mecanim = _pagTestSpine4Mecanim;
                    break;
                case 5:
                    animator = _pagTestSpine5Animator;
                    mecanim = _pagTestSpine5Mecanim;
                    break;
                default:
                    return false;
            }

            return true;
        }

        private bool GetSpineTestShowing(int spineIndex)
        {
            switch (spineIndex)
            {
                case 1: return _spineTest1Showing;
                case 2: return _spineTest2Showing;
                case 3: return _spineTest3Showing;
                case 4: return _spineTest4Showing;
                case 5: return _spineTest5Showing;
                default: return false;
            }
        }

        private void SetSpineTestShowing(int spineIndex, bool showing)
        {
            switch (spineIndex)
            {
                case 1: _spineTest1Showing = showing; break;
                case 2: _spineTest2Showing = showing; break;
                case 3: _spineTest3Showing = showing; break;
                case 4: _spineTest4Showing = showing; break;
                case 5: _spineTest5Showing = showing; break;
            }
        }

        /// <summary>懒加载单个 Spine 节点组件并默认隐藏。</summary>
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

        /// <summary>隐藏 Spine 对照动画（ClearState + SetActive false，清 mesh 并停止更新与渲染）。</summary>
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

        /// <summary>显示并播放 Spine 对照动画。</summary>
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

        /// <summary>PAG1 切换：XingXing1.pag 单槽 repeat=-1（pagEffect1）。</summary>
        private void OnClickPagTest1Button()
        {
            StopPagTestGroupPlayback();

            if (_pagTest1Showing)
            {
                Debug.Log($"{PagLogPrefix} PAG1 clicked, stop {PagTestName2}");
                _pagTest1Showing = false;
                if (_corPagTest1 != null && mono != null)
                {
                    mono.StopCoroutine(_corPagTest1);
                    _corPagTest1 = null;
                }

                StopPagTest(_pagTestSlot1, ref _pagTest1Showing);
                return;
            }

            Debug.Log($"{PagLogPrefix} PAG1 clicked, play {PagTestName2}");
            if (_corPagTest1 != null && mono != null)
            {
                mono.StopCoroutine(_corPagTest1);
                _corPagTest1 = null;
            }

            StopPagTest(_pagTestSlot1, ref _pagTest1Showing);
            if (mono == null)
            {
                Debug.LogWarning($"{PagLogPrefix} PAG1 play skipped: mono is null");
                return;
            }

            _pagTest1Showing = true;
            _corPagTest1 = mono.StartCoroutine(StartPagTest1ButtonPlayback());
        }

        /// <summary>PAG2 切换：BigWin_1024.pag 单槽 repeat=-1（pagEffect2）。</summary>
        private void OnClickPagTest2Button()
        {
            StopPagTestGroupPlayback();

            if (_pagTest2Showing)
            {
                Debug.Log($"{PagLogPrefix} PAG2 clicked, stop {PagTestName1}");
                StopPagTestSlotPlayback(2);
                return;
            }

            Debug.Log($"{PagLogPrefix} PAG2 clicked, play {PagTestName1}");
            StartPagTestSlotPlayback(2);
        }

        /// <summary>PAG3 切换：XingXing2.pag 单槽 repeat=-1（pagEffect3），独立 scale/clamp。</summary>
        private void OnClickPagTest3Button()
        {
            StopPagTestGroupPlayback();

            if (_pagTest3Showing)
            {
                Debug.Log($"{PagLogPrefix} PAG3 clicked, stop {PagTestName3}");
                StopPagTestSlotPlayback(3);
                return;
            }

            Debug.Log($"{PagLogPrefix} PAG3 clicked, play {PagTestName3}");
            StartPagTestSlotPlayback(3);
        }

        /// <summary>启动 PAG2 或 PAG3 单槽循环播放（slotIndex 仅支持 2/3）。</summary>
        private bool StartPagTestSlotPlayback(int slotIndex)
        {
            StopPagTestGroupPlayback();

            if (slotIndex == 2)
            {
                if (_corPagTest2 != null && mono != null)
                {
                    mono.StopCoroutine(_corPagTest2);
                    _corPagTest2 = null;
                }

                StopPagTest(_pagTestSlot2, ref _pagTest2Showing);
                if (mono == null)
                {
                    Debug.LogWarning($"{PagLogPrefix} PAG2 play skipped: mono is null");
                    return false;
                }

                _pagTest2Showing = true;
                _corPagTest2 = mono.StartCoroutine(StartPagTest2ButtonPlayback());
                return true;
            }

            if (slotIndex == 3)
            {
                if (_corPagTest3 != null && mono != null)
                {
                    mono.StopCoroutine(_corPagTest3);
                    _corPagTest3 = null;
                }

                StopPagTest(_pagTestSlot3, ref _pagTest3Showing);
                if (mono == null)
                {
                    Debug.LogWarning($"{PagLogPrefix} PAG3 play skipped: mono is null");
                    return false;
                }

                _pagTest3Showing = true;
                _corPagTest3 = mono.StartCoroutine(StartPagTest3ButtonPlayback());
                return true;
            }

            Debug.LogWarning($"{PagLogPrefix} StartPagTestSlotPlayback unsupported slotIndex={slotIndex}");
            return false;
        }

        /// <summary>停止 PAG2 或 PAG3 单槽播放（slotIndex 仅支持 2/3）。</summary>
        private void StopPagTestSlotPlayback(int slotIndex)
        {
            if (slotIndex == 2)
            {
                _pagTest2Showing = false;
                CancelComboPag2SpineCoroutine();
                if (_corPagTest2 != null && mono != null)
                {
                    mono.StopCoroutine(_corPagTest2);
                    _corPagTest2 = null;
                }

                StopPagTest(_pagTestSlot2, ref _pagTest2Showing);
                return;
            }

            if (slotIndex == 3)
            {
                _pagTest3Showing = false;
                CancelComboPag3SpineCoroutine();
                if (_corPagTest3 != null && mono != null)
                {
                    mono.StopCoroutine(_corPagTest3);
                    _corPagTest3 = null;
                }

                StopPagTest(_pagTestSlot3, ref _pagTest3Showing);
            }
        }

        /// <summary>PAG4 切换：pagEffect4 顺序播 BigWin 六段 PAG（各 repeat=1）。</summary>
        private void OnClickPagTest4Button()
        {
            if (_pagTest4Showing)
            {
                Debug.Log($"{PagLogPrefix} PAG4 clicked, stop BigWin sequence");
                StopPagTest4Playback();
                return;
            }

            string pag4Files = string.Join(" -> ", PagTestBigWinSequence);
            Debug.Log($"{PagLogPrefix} PAG4 clicked, play BigWin sequence [{pag4Files}]");

            PagGpuSyncGroup.EndStaticGroupIfActive("PagTest4Button");
            _pagTestSlot5?.Stop(PagTestUseFguiTexture);
            _pagTestSlot6?.Stop(PagTestUseFguiTexture);

            if (_corPagTest4 != null && mono != null)
            {
                mono.StopCoroutine(_corPagTest4);
                _corPagTest4 = null;
            }

            StopPagTest(_pagTestSlot4, ref _pagTest4Showing);
            if (mono == null)
            {
                Debug.LogWarning($"{PagLogPrefix} PAG4 play skipped: mono is null");
                return;
            }

            _pagTest4Showing = true;
            _corPagTest4 = mono.StartCoroutine(StartPagTest4ButtonPlayback());
        }

        /// <summary>预热缓存后单次 Play + repeat=-1，由纹理模式 Native 路径无缝循环，避免圈间重开 Play 空窗。</summary>
        private IEnumerator StartPagTest1ButtonPlayback()
        {
            yield return EnsurePagTestCompositionReady(PagTestName2, _pagTest1CacheWarmed,
                ok => _pagTest1CacheWarmed = ok);

            if (!_pagTest1Showing)
            {
                _corPagTest1 = null;
                yield break;
            }

            PlayPagTest(_pagTestSlot1, PagTestName2, -1);
            yield return TryAlignPagTestFpsAfterPlayStarted(_pagTestSlot1);
            _corPagTest1 = null;
            Debug.Log($"{PagLogPrefix} StartPagTest1ButtonPlayback: native loop repeat=-1");
        }

        /// <summary>PAG2 按钮协程：预热 BigWin_1024 → PlayPagTest slot2 repeat=-1 → 对齐帧率。</summary>
        private IEnumerator StartPagTest2ButtonPlayback()
        {
            yield return EnsurePagTestCompositionReady(PagTestName1, _pagTest2CacheWarmed,
                ok => _pagTest2CacheWarmed = ok);

            if (!_pagTest2Showing)
            {
                _corPagTest2 = null;
                yield break;
            }

            PlayPagTest(_pagTestSlot2, PagTestName1, -1);
            yield return TryAlignPagTestFpsAfterPlayStarted(_pagTestSlot2);
            _corPagTest2 = null;
            Debug.Log($"{PagLogPrefix} StartPagTest2ButtonPlayback: native loop repeat=-1");
        }

        /// <summary>PAG3 按钮协程：预热 XingXing2 → PlayPagTest slot3 repeat=-1 → 对齐帧率。</summary>
        private IEnumerator StartPagTest3ButtonPlayback()
        {
            yield return EnsurePagTestCompositionReady(PagTestName3, _pagTest3CacheWarmed,
                ok => _pagTest3CacheWarmed = ok);

            if (!_pagTest3Showing)
            {
                _corPagTest3 = null;
                yield break;
            }

            PlayPagTest(_pagTestSlot3, PagTestName3, -1);
            yield return TryAlignPagTestFpsAfterPlayStarted(_pagTestSlot3);
            _corPagTest3 = null;
            Debug.Log($"{PagLogPrefix} StartPagTest3ButtonPlayback: native loop repeat=-1");
        }

        /// <summary>
        /// PAG4 按钮协程：pagEffect4 Native 播放列表无缝播 BigWin 六段（Phase4E），播完自动结束。
        /// </summary>
        private IEnumerator StartPagTest4ButtonPlayback()
        {
            if (!_pagTest4CacheWarmed)
            {
                for (int i = 0; i < PagTestBigWinSequence.Length; i++)
                {
                    yield return PagController.PreloadCompositionCoroutine(PagTestBigWinSequence[i]);
                }

                _pagTest4CacheWarmed = true;
            }

            if (!_pagTest4Showing)
            {
                _corPagTest4 = null;
                yield break;
            }

            EnsurePagTestSlots();
            PagController controller = _pagTestSlot4?.Controller;
            if (controller == null)
            {
                Debug.LogError($"{PagLogPrefix} PAG4: controller missing");
                StopPagTest4Playback();
                _corPagTest4 = null;
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
                Debug.Log($"{PagLogPrefix} PAG4 layout extra: {layoutExtra} ({layoutDebug})");
            }
            else
            {
                Debug.LogWarning($"{PagLogPrefix} PAG4 layout fallback turntable");
                controller.LayoutPagAuto("turntable");
            }

            _pagTestSlot4.SetFguiDisplayScale(PagTestDisplayScale);
            _pagTestSlot4.SetFguiClampDisplayToHolder(PagTestClampDisplayToHolder);
            if (!_pagTestSlot4.PreparePlay(true, PagTestFguiMaxDisplaySide, PagTestFguiFps))
            {
                Debug.LogError($"{PagLogPrefix} PAG4 PreparePlay failed");
                StopPagTest4Playback();
                _corPagTest4 = null;
                yield break;
            }

            PagSegment[] segments = PagTestBigWinSequence
                .Select(p => new PagSegment(p, 1))
                .ToArray();

            if (!controller.PlayFguiGpuSequence(segments, positionType, layoutExtra, PagTestUseGpuSyncGroup))
            {
                Debug.LogError($"{PagLogPrefix} PAG4 PlayFguiGpuSequence failed");
                StopPagTest4Playback();
                _corPagTest4 = null;
                yield break;
            }

            yield return WaitPagTestPlayStarted(_pagTestSlot4, PagTestPlayStartedTimeoutSec);
            controller = _pagTestSlot4?.Controller;
            if (controller == null || !controller.PlayStarted)
            {
                Debug.LogError($"{PagLogPrefix} PAG4 sequence did not start within {PagTestPlayStartedTimeoutSec}s");
                StopPagTest4Playback();
                _corPagTest4 = null;
                yield break;
            }

            float totalTimeout = 0f;
            for (int i = 0; i < PagTestBigWinSequence.Length; i++)
            {
                totalTimeout += controller.GetCompositionDurationSecWithFallback(PagTestDuration) + 1f;
            }

            totalTimeout += 3f;
            totalTimeout = Mathf.Max(totalTimeout, PagTestBigWinSequence.Length * PagTestDuration + 5f);
            yield return controller.WaitForFguiGpuSequenceFinished(totalTimeout);

            if (!_pagTest4Showing)
            {
                _corPagTest4 = null;
                yield break;
            }

            StopPagTest(_pagTestSlot4, ref _pagTest4Showing);
            _corPagTest4 = null;
            Debug.Log($"{PagLogPrefix} StartPagTest4ButtonPlayback: BigWin sequence finished (4E playlist)");
        }

        private void BindPagTestGlowButton(string buttonName, EventCallback0 handler)
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

        private void OnClickPagTestGlowButton(int glowIndex, ref bool showingFlag, ref Coroutine coroutine, Func<IEnumerator> playbackFactory, string pagLabel)
        {
            StopPagTestGroupPlayback();

            if (showingFlag)
            {
                Debug.Log($"{PagLogPrefix} {pagLabel} clicked, stop glow");
                StopPagTestGlow(glowIndex);
                return;
            }

            Debug.Log($"{PagLogPrefix} {pagLabel} clicked, play glow");
            if (mono == null)
            {
                Debug.LogWarning($"{PagLogPrefix} {pagLabel} play skipped: mono is null");
                return;
            }

            showingFlag = true;
            coroutine = mono.StartCoroutine(playbackFactory());
        }

        private void OnClickPagTest5Button()
        {
            OnClickPagTestGlowButton(5, ref _pagTest5Showing, ref _corPagTest5, StartPagTest5ButtonPlayback, "PAG5");
        }

        private void OnClickPagTest6Button()
        {
            OnClickPagTestGlowButton(6, ref _pagTest6Showing, ref _corPagTest6, StartPagTest6ButtonPlayback, "PAG6");
        }

        private void OnClickPagTest7Button()
        {
            OnClickPagTestGlowButton(7, ref _pagTest7Showing, ref _corPagTest7, StartPagTest7ButtonPlayback, "PAG7");
        }

        private void OnClickPagTest8Button()
        {
            OnClickPagTestGlowButton(8, ref _pagTest8Showing, ref _corPagTest8, StartPagTest8ButtonPlayback, "PAG8");
        }

        private void OnClickPagTest9Button()
        {
            OnClickPagTestGlowButton(9, ref _pagTest9Showing, ref _corPagTest9, StartPagTest9ButtonPlayback, "PAG9");
        }

        private IEnumerator StartPagTest5ButtonPlayback()
        {
            yield return StartPagTestGlowLoopPlayback(5, PagGlowLoop720, PagTestDisplayScale);
        }

        private IEnumerator StartPagTest6ButtonPlayback()
        {
            yield return StartPagTestGlowLoopPlayback(6, PagGlowLoop720, PagGlow720DisplayScale);
        }

        private IEnumerator StartPagTest7ButtonPlayback()
        {
            yield return StartPagTestGlowLoopPlayback(7, PagGlowLoopHalf, PagTestDisplayScale);
        }

        private IEnumerator StartPagTest8ButtonPlayback()
        {
            yield return StartPagTestGlowLoopPlayback(8, PagGlowLoopFull, PagTestDisplayScale);
        }

        private IEnumerator StartPagTest9ButtonPlayback()
        {
            yield return StartPagTestGlowIntroLoopPlayback(9, PagGlowInFull, PagGlowLoopFull, PagTestDisplayScale);
        }

        private IEnumerator StartPagTestGlowLoopPlayback(int glowIndex, string pagFileName, float displayScale)
        {
            bool cacheWarmed = GetPagTestGlowCacheWarmed(glowIndex);
            yield return EnsurePagTestCompositionReady(pagFileName, cacheWarmed,
                ok => SetPagTestGlowCacheWarmed(glowIndex, ok));

            if (!IsPagTestGlowShowing(glowIndex))
            {
                ClearPagTestGlowCoroutine(glowIndex);
                yield break;
            }

            EnsurePagTestGlowSlotByIndex(glowIndex);
            PagSlotBinding glowSlot = GetPagTestGlowSlot(glowIndex);
            PlayPagTest(glowSlot, pagFileName, -1, displayScale);
            yield return TryAlignPagTestFpsAfterPlayStarted(glowSlot);
            if (!Mathf.Approximately(displayScale, PagTestDisplayScale))
            {
                glowSlot?.Controller?.SyncFguiDisplayLayoutFromComposition();
            }

            ClearPagTestGlowCoroutine(glowIndex);
            Debug.Log($"{PagLogPrefix} PAG{glowIndex}: glow loop repeat=-1, scale={displayScale}");
        }

        private IEnumerator StartPagTestGlowIntroLoopPlayback(
            int glowIndex,
            string introFileName,
            string loopFileName,
            float displayScale)
        {
            bool cacheWarmed = GetPagTestGlowCacheWarmed(glowIndex);
            if (!cacheWarmed || !IsPagCompositionReady(introFileName))
            {
                yield return PagController.PreloadCompositionCoroutine(introFileName);
            }

            if (!cacheWarmed || !IsPagCompositionReady(loopFileName))
            {
                yield return PagController.PreloadCompositionCoroutine(loopFileName);
            }

            SetPagTestGlowCacheWarmed(glowIndex,
                IsPagCompositionReady(introFileName) && IsPagCompositionReady(loopFileName));

            if (!IsPagTestGlowShowing(glowIndex))
            {
                ClearPagTestGlowCoroutine(glowIndex);
                yield break;
            }

            EnsurePagTestGlowSlotByIndex(glowIndex);
            PagSlotBinding glowSlot = GetPagTestGlowSlot(glowIndex);
            PagController controller = glowSlot?.Controller;
            if (controller == null)
            {
                Debug.LogError($"{PagLogPrefix} PAG{glowIndex}: controller missing");
                StopPagTestGlow(glowIndex);
                ClearPagTestGlowCoroutine(glowIndex);
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
                Debug.Log($"{PagLogPrefix} PAG{glowIndex} layout extra: {layoutExtra} ({layoutDebug})");
            }
            else
            {
                Debug.LogWarning($"{PagLogPrefix} PAG{glowIndex} layout fallback turntable");
                controller.LayoutPagAuto("turntable");
            }

            glowSlot.SetFguiDisplayScale(displayScale);
            glowSlot.SetFguiClampDisplayToHolder(PagTestClampDisplayToHolder);
            if (!glowSlot.PreparePlay(true, PagTestFguiMaxDisplaySide, PagTestFguiFps))
            {
                Debug.LogError($"{PagLogPrefix} PAG{glowIndex} PreparePlay failed");
                StopPagTestGlow(glowIndex);
                ClearPagTestGlowCoroutine(glowIndex);
                yield break;
            }

            PagSegment[] segments =
            {
                new PagSegment(introFileName, 1),
                new PagSegment(loopFileName, -1),
            };
            if (!controller.PlayFguiGpuSequence(segments, positionType, layoutExtra, PagTestUseGpuSyncGroup))
            {
                Debug.LogError($"{PagLogPrefix} PAG{glowIndex} PlayFguiGpuSequence failed");
                StopPagTestGlow(glowIndex);
                ClearPagTestGlowCoroutine(glowIndex);
                yield break;
            }

            yield return WaitPagTestPlayStarted(glowSlot, PagTestPlayStartedTimeoutSec);
            controller = glowSlot?.Controller;
            if (controller == null || !controller.PlayStarted)
            {
                Debug.LogError($"{PagLogPrefix} PAG{glowIndex} sequence did not start within {PagTestPlayStartedTimeoutSec}s");
                StopPagTestGlow(glowIndex);
                ClearPagTestGlowCoroutine(glowIndex);
                yield break;
            }

            yield return TryAlignPagTestFpsAfterPlayStarted(glowSlot);
            ClearPagTestGlowCoroutine(glowIndex);
            Debug.Log($"{PagLogPrefix} PAG{glowIndex}: intro->loop sequence started (4E playlist), scale={displayScale}");
        }

        private bool IsPagTestGlowShowing(int glowIndex)
        {
            switch (glowIndex)
            {
                case 5: return _pagTest5Showing;
                case 6: return _pagTest6Showing;
                case 7: return _pagTest7Showing;
                case 8: return _pagTest8Showing;
                case 9: return _pagTest9Showing;
                default: return false;
            }
        }

        private bool GetPagTestGlowCacheWarmed(int glowIndex)
        {
            switch (glowIndex)
            {
                case 5: return _pagTest5CacheWarmed;
                case 6: return _pagTest6CacheWarmed;
                case 7: return _pagTest7CacheWarmed;
                case 8: return _pagTest8CacheWarmed;
                case 9: return _pagTest9CacheWarmed;
                default: return false;
            }
        }

        private void SetPagTestGlowCacheWarmed(int glowIndex, bool warmed)
        {
            switch (glowIndex)
            {
                case 5: _pagTest5CacheWarmed = warmed; break;
                case 6: _pagTest6CacheWarmed = warmed; break;
                case 7: _pagTest7CacheWarmed = warmed; break;
                case 8: _pagTest8CacheWarmed = warmed; break;
                case 9: _pagTest9CacheWarmed = warmed; break;
            }
        }

        private void ClearPagTestGlowCoroutine(int glowIndex)
        {
            switch (glowIndex)
            {
                case 5: _corPagTest5 = null; break;
                case 6: _corPagTest6 = null; break;
                case 7: _corPagTest7 = null; break;
                case 8: _corPagTest8 = null; break;
                case 9: _corPagTest9 = null; break;
            }
        }

        private void OnClickPagTest10Button()
        {
            OnClickPagTestNpcButton(0);
        }

        private void OnClickPagTest11Button()
        {
            OnClickPagTestNpcButton(1);
        }

        private void OnClickPagTest12Button()
        {
            OnClickPagTestNpcButton(2);
        }

        private void OnClickPagTestNpcButton(int npcIndex)
        {
            if (npcIndex < 0 || npcIndex >= MaxPagTestNpcCount)
            {
                return;
            }

            string label = PagTestNpcLabels[npcIndex];
            if (IsPagTestNpcShowing(npcIndex))
            {
                Debug.Log($"{PagLogPrefix} PAG{npcIndex + 10} clicked, stop {label}");
                StopPagTestNpcPlayback(npcIndex);
                return;
            }

            string[] sequence = PagTestNpcSequences[npcIndex];
            Debug.Log($"{PagLogPrefix} PAG{npcIndex + 10} clicked, play {label} syncGroup={PagTestUseGpuSyncGroup}");
            StopPagTestNpcPlayback(npcIndex);
            if (mono == null)
            {
                Debug.LogWarning($"{PagLogPrefix} PAG{npcIndex + 10} play skipped: mono is null");
                return;
            }

            SetPagTestNpcShowing(npcIndex, true);
            int sessionId = ++_pagTestNpcSessionId[npcIndex];
            SetPagTestNpcCoroutine(npcIndex,
                mono.StartCoroutine(StartPagTestNpcSequencePlayback(npcIndex, sequence, label, sessionId)));
        }

        private void StopAllPagTestNpcPlayback()
        {
            for (int i = 0; i < MaxPagTestNpcCount; i++)
            {
                StopPagTestNpcPlayback(i);
            }
        }

        private void StopPagTestNpcPlayback(int npcIndex)
        {
            if (npcIndex < 0 || npcIndex >= MaxPagTestNpcCount)
            {
                return;
            }

            _pagTestNpcSessionId[npcIndex]++;
            SetPagTestNpcShowing(npcIndex, false);

            Coroutine coroutine = GetPagTestNpcCoroutine(npcIndex);
            if (coroutine != null && mono != null)
            {
                mono.StopCoroutine(coroutine);
            }

            SetPagTestNpcCoroutine(npcIndex, null);
            GetPagTestNpcSlot(npcIndex)?.Stop(PagTestUseFguiTexture);
        }

        private IEnumerator StartPagTestNpcSequencePlayback(int npcIndex, string[] sequence, string label, int sessionId)
        {
            if (sequence == null || sequence.Length == 0)
            {
                StopPagTestNpcPlayback(npcIndex);
                yield break;
            }

            for (int i = 0; i < sequence.Length; i++)
            {
                yield return EnsurePagTestCompositionReady(sequence[i], false, _ => { });

                if (sessionId != _pagTestNpcSessionId[npcIndex])
                {
                    yield break;
                }
            }

            if (!IsPagTestNpcShowing(npcIndex))
            {
                SetPagTestNpcCoroutine(npcIndex, null);
                yield break;
            }

            EnsurePagTestNpcSlotByIndex(npcIndex);
            PagSlotBinding slot = GetPagTestNpcSlot(npcIndex);
            PagController controller = slot?.Controller;
            if (controller == null)
            {
                Debug.LogError($"{PagLogPrefix} PAG{npcIndex + 10} controller missing: {label}");
                StopPagTestNpcPlayback(npcIndex);
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
                Debug.Log($"{PagLogPrefix} PAG{npcIndex + 10} layout extra: {layoutExtra} ({layoutDebug})");
            }
            else
            {
                Debug.LogWarning($"{PagLogPrefix} PAG{npcIndex + 10} layout fallback turntable");
                controller.LayoutPagAuto("turntable");
            }

            slot.SetFguiDisplayScale(PagTestDisplayScale);
            slot.SetFguiClampDisplayToHolder(PagTestClampDisplayToHolder);
            if (!slot.PreparePlay(true, PagTestFguiMaxDisplaySide, PagTestFguiFps))
            {
                Debug.LogError($"{PagLogPrefix} PAG{npcIndex + 10} PreparePlay failed: {label}");
                StopPagTestNpcPlayback(npcIndex);
                yield break;
            }

            PagSegment[] segments = BuildPagTestNpcSegments(sequence);
            if (!controller.PlayFguiGpuSequence(segments, positionType, layoutExtra, PagTestUseGpuSyncGroup))
            {
                Debug.LogError($"{PagLogPrefix} PAG{npcIndex + 10} PlayFguiGpuSequence failed: {label}");
                StopPagTestNpcPlayback(npcIndex);
                yield break;
            }

            yield return WaitPagTestPlayStarted(slot, PagTestPlayStartedTimeoutSec);
            controller = slot?.Controller;
            if (controller == null || !controller.PlayStarted)
            {
                Debug.LogError($"{PagLogPrefix} PAG{npcIndex + 10} sequence did not start within {PagTestPlayStartedTimeoutSec}s: {label}");
                StopPagTestNpcPlayback(npcIndex);
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

            if (sessionId != _pagTestNpcSessionId[npcIndex])
            {
                yield break;
            }

            SetPagTestNpcShowing(npcIndex, false);
            slot?.Stop(PagTestUseFguiTexture);
            SetPagTestNpcCoroutine(npcIndex, null);
            Debug.Log($"{PagLogPrefix} PAG{npcIndex + 10} npc sequence finished: {label}");
        }

        private static PagSegment[] BuildPagTestNpcSegments(string[] sequence)
        {
            var segments = new PagSegment[sequence.Length];
            for (int i = 0; i < sequence.Length; i++)
            {
                segments[i] = new PagSegment(sequence[i], 1);
            }

            return segments;
        }

        private bool IsPagTestNpcShowing(int npcIndex)
        {
            switch (npcIndex)
            {
                case 0: return _pagTest10Showing;
                case 1: return _pagTest11Showing;
                case 2: return _pagTest12Showing;
                default: return false;
            }
        }

        private void SetPagTestNpcShowing(int npcIndex, bool showing)
        {
            switch (npcIndex)
            {
                case 0: _pagTest10Showing = showing; break;
                case 1: _pagTest11Showing = showing; break;
                case 2: _pagTest12Showing = showing; break;
            }
        }

        private Coroutine GetPagTestNpcCoroutine(int npcIndex)
        {
            switch (npcIndex)
            {
                case 0: return _corPagTest10;
                case 1: return _corPagTest11;
                case 2: return _corPagTest12;
                default: return null;
            }
        }

        private void SetPagTestNpcCoroutine(int npcIndex, Coroutine coroutine)
        {
            switch (npcIndex)
            {
                case 0: _corPagTest10 = coroutine; break;
                case 1: _corPagTest11 = coroutine; break;
                case 2: _corPagTest12 = coroutine; break;
            }
        }

        /// <summary>Spine1 对照按钮：切换 jp_pup_grand / GRAND_in。</summary>
        private void OnClickSpineTest1Button()
        {
            TogglePagTestSpine(1, PagTestSpine1PlayAnim);
        }

        /// <summary>Spine2 对照按钮：切换 ng_pop_bigWin / bigwin_start。</summary>
        private void OnClickSpineTest2Button()
        {
            TogglePagTestSpine(2, PagTestSpine2PlayAnim);
        }

        /// <summary>Spine3 对照按钮：切换 jp_pup_GRAND / in。</summary>
        private void OnClickSpineTest3Button()
        {
            TogglePagTestSpine(3, PagTestSpine3PlayAnim);
        }

        /// <summary>Spine4 对照按钮：切换 ng_bor_boom1 / start。</summary>
        private void OnClickSpineTest4Button()
        {
            TogglePagTestSpine(4, PagTestSpine4PlayAnim);
        }

        /// <summary>Spine5 对照按钮：切换 ng_ic_bigwin / bigwin_start。</summary>
        private void OnClickSpineTest5Button()
        {
            TogglePagTestSpine(5, PagTestSpine5PlayAnim);
        }

        /// <summary>Spine 对照显示/隐藏切换。</summary>
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

        private void PlayComboBorderMegaWinEffects12()
        {
            PlayBorderMegaWinEffect(0);
            PlayBorderMegaWinEffect(1);
        }

        private void StopComboBorderMegaWinEffects12()
        {
            StopBorderMegaWinEffect(0);
            StopBorderMegaWinEffect(1);
        }

        private void ResetComboTestShowingFlags()
        {
            CancelComboPag2SpineCoroutine();
            CancelComboPag3SpineCoroutine();
            _comboP2S1Showing = false;
            _comboP2S2Showing = false;
            _comboP2S3Showing = false;
            _comboP2S4Showing = false;
            _comboP2S5Showing = false;
            _comboP3S1Showing = false;
            _comboP3S2Showing = false;
            _comboP3S3Showing = false;
            _comboP3S4Showing = false;
            _comboP3S5Showing = false;
            _comboS2E1E2Showing = false;
            _comboS1E1E2Showing = false;
            _comboP9S1Showing = false;
            _comboP9S2Showing = false;
            _comboP9S3Showing = false;
            _comboP9S4Showing = false;
            _comboP9S5Showing = false;
            _comboEffectAllShowing = false;
            _comboSpineAllShowing = false;
            _comboP4S1Showing = false;
            _comboP4S2Showing = false;
            _comboP4S3Showing = false;
            _comboP4S4Showing = false;
            _comboP4S5Showing = false;
        }

        private string GetPagTestSpinePlayAnim(int spineIndex)
        {
            switch (spineIndex)
            {
                case 1: return PagTestSpine1PlayAnim;
                case 2: return PagTestSpine2PlayAnim;
                case 3: return PagTestSpine3PlayAnim;
                case 4: return PagTestSpine4PlayAnim;
                case 5: return PagTestSpine5PlayAnim;
                default: return string.Empty;
            }
        }

        private void StartPagTest9PlaybackForCombo()
        {
            StopPagTestGroupPlayback();
            StopPagTestGlow(9);
            if (mono == null)
            {
                Debug.LogWarning($"{PagLogPrefix} PAG9 combo play skipped: mono is null");
                return;
            }

            _pagTest9Showing = true;
            _corPagTest9 = mono.StartCoroutine(StartPagTest9ButtonPlayback());
        }

        private void StartPagTest4PlaybackForCombo()
        {
            StopPagTestGroupPlayback();
            if (mono == null)
            {
                Debug.LogWarning($"{PagLogPrefix} PAG4 combo play skipped: mono is null");
                return;
            }

            _pagTest4Showing = true;
            _corPagTest4 = mono.StartCoroutine(StartPagTest4ButtonPlayback());
        }

        private void ShowAllPagTestSpines()
        {
            ShowPagTestSpine(1, PagTestSpine1PlayAnim);
            ShowPagTestSpine(2, PagTestSpine2PlayAnim);
            ShowPagTestSpine(3, PagTestSpine3PlayAnim);
            ShowPagTestSpine(4, PagTestSpine4PlayAnim);
            ShowPagTestSpine(5, PagTestSpine5PlayAnim);
        }

        private void HideAllPagTestSpines()
        {
            HidePagTestSpine(1);
            HidePagTestSpine(2);
            HidePagTestSpine(3);
            HidePagTestSpine(4);
            HidePagTestSpine(5);
        }

        private void PlayAllBorderMegaWinEffects()
        {
            for (int i = 0; i < BorderMegaWinPrefabNames.Length; i++)
            {
                PlayBorderMegaWinEffect(i);
            }
        }

        /// <summary>绑定组合测试按钮（PAG+Spine / Effect / 批量）。</summary>
        private void BindComboTestButtons()
        {
            if (_comboTestButtonsBound || contentPane == null)
            {
                return;
            }

            BindComboTestButton("P2S1", OnClickComboP2S1);
            BindComboTestButton("P2S2", OnClickComboP2S2);
            BindComboTestButton("P2S3", OnClickComboP2S3);
            BindComboTestButton("P2S4", OnClickComboP2S4);
            BindComboTestButton("P2S5", OnClickComboP2S5);
            BindComboTestButton("P3S1", OnClickComboP3S1);
            BindComboTestButton("P3S2", OnClickComboP3S2);
            BindComboTestButton("P3S3", OnClickComboP3S3);
            BindComboTestButton("P3S4", OnClickComboP3S4);
            BindComboTestButton("P3S5", OnClickComboP3S5);
            BindComboTestButton("S2E1E2", OnClickComboS2E1E2);
            BindComboTestButton("S1E1E2", OnClickComboS1E1E2);
            BindComboTestButton("P9S1", OnClickComboP9S1);
            BindComboTestButton("P9S2", OnClickComboP9S2);
            BindComboTestButton("P9S3", OnClickComboP9S3);
            BindComboTestButton("P9S4", OnClickComboP9S4);
            BindComboTestButton("P9S5", OnClickComboP9S5);
            BindComboTestButton("Effect1_5", OnClickComboEffectAll);
            BindComboTestButton("Spine1_5", OnClickComboSpineAll);
            BindComboTestButton("P4S1", OnClickComboP4S1);
            BindComboTestButton("P4S2", OnClickComboP4S2);
            BindComboTestButton("P4S3", OnClickComboP4S3);
            BindComboTestButton("P4S4", OnClickComboP4S4);
            BindComboTestButton("P4S5", OnClickComboP4S5);
            _comboTestButtonsBound = true;
        }

        private void BindComboTestButton(string buttonName, EventCallback0 handler)
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

        /// <summary>OnClose / OnLanguageChange 前清除组合测试按钮点击监听。</summary>
        private void ClearComboTestButtons()
        {
            if (!_comboTestButtonsBound || contentPane == null)
            {
                return;
            }

            for (int i = 0; i < ComboTestButtonNames.Length; i++)
            {
                contentPane.GetChild(ComboTestButtonNames[i])?.asButton?.onClick.Clear();
            }

            _comboTestButtonsBound = false;
            ResetComboTestShowingFlags();
        }

        private void CancelComboPag2SpineCoroutine()
        {
            if (_corComboPag2Spine != null && mono != null)
            {
                mono.StopCoroutine(_corComboPag2Spine);
                _corComboPag2Spine = null;
            }
        }

        private void CancelComboPag3SpineCoroutine()
        {
            if (_corComboPag3Spine != null && mono != null)
            {
                mono.StopCoroutine(_corComboPag3Spine);
                _corComboPag3Spine = null;
            }
        }

        private void StopComboPag2SpinePlayback(int spineIndex, ref bool showingFlag, string comboLabel)
        {
            Debug.Log($"{PagLogPrefix} {comboLabel} clicked, stop");
            showingFlag = false;
            CancelComboPag2SpineCoroutine();
            StopPagTestSlotPlayback(2);
            HidePagTestSpine(spineIndex);
        }

        private void StopComboPag3SpinePlayback(int spineIndex, ref bool showingFlag, string comboLabel)
        {
            Debug.Log($"{PagLogPrefix} {comboLabel} clicked, stop");
            showingFlag = false;
            CancelComboPag3SpineCoroutine();
            StopPagTestSlotPlayback(3);
            HidePagTestSpine(spineIndex);
        }

        private bool IsComboPag2SpineShowing(int spineIndex)
        {
            switch (spineIndex)
            {
                case 1: return _comboP2S1Showing;
                case 2: return _comboP2S2Showing;
                case 3: return _comboP2S3Showing;
                case 4: return _comboP2S4Showing;
                case 5: return _comboP2S5Showing;
                default: return false;
            }
        }

        private void ClearComboPag2SpineShowing(int spineIndex)
        {
            switch (spineIndex)
            {
                case 1: _comboP2S1Showing = false; break;
                case 2: _comboP2S2Showing = false; break;
                case 3: _comboP2S3Showing = false; break;
                case 4: _comboP2S4Showing = false; break;
                case 5: _comboP2S5Showing = false; break;
            }
        }

        private bool IsComboPag3SpineShowing(int spineIndex)
        {
            switch (spineIndex)
            {
                case 1: return _comboP3S1Showing;
                case 2: return _comboP3S2Showing;
                case 3: return _comboP3S3Showing;
                case 4: return _comboP3S4Showing;
                case 5: return _comboP3S5Showing;
                default: return false;
            }
        }

        private void ClearComboPag3SpineShowing(int spineIndex)
        {
            switch (spineIndex)
            {
                case 1: _comboP3S1Showing = false; break;
                case 2: _comboP3S2Showing = false; break;
                case 3: _comboP3S3Showing = false; break;
                case 4: _comboP3S4Showing = false; break;
                case 5: _comboP3S5Showing = false; break;
            }
        }

        private IEnumerator PlayComboPag2SpineCoroutine(int spineIndex, string comboLabel)
        {
            if (!StartPagTestSlotPlayback(2))
            {
                ClearComboPag2SpineShowing(spineIndex);
                _corComboPag2Spine = null;
                yield break;
            }

            PagController pagController = _pagTestSlot2?.Controller;
            if (pagController != null)
            {
                yield return pagController.WaitForGpuDisplayReady(PagTestPlayStartedTimeoutSec);
            }

            if (!IsComboPag2SpineShowing(spineIndex))
            {
                _corComboPag2Spine = null;
                yield break;
            }

            ShowPagTestSpine(spineIndex, GetPagTestSpinePlayAnim(spineIndex));
            Debug.Log($"{PagLogPrefix} {comboLabel} spine synced at texture display ready");
            _corComboPag2Spine = null;
        }

        private IEnumerator PlayComboPag3SpineCoroutine(int spineIndex, string comboLabel)
        {
            if (!StartPagTestSlotPlayback(3))
            {
                ClearComboPag3SpineShowing(spineIndex);
                _corComboPag3Spine = null;
                yield break;
            }

            PagController pagController = _pagTestSlot3?.Controller;
            if (pagController != null)
            {
                yield return pagController.WaitForGpuDisplayReady(PagTestPlayStartedTimeoutSec);
            }

            if (!IsComboPag3SpineShowing(spineIndex))
            {
                _corComboPag3Spine = null;
                yield break;
            }

            ShowPagTestSpine(spineIndex, GetPagTestSpinePlayAnim(spineIndex));
            Debug.Log($"{PagLogPrefix} {comboLabel} spine synced at texture display ready");
            _corComboPag3Spine = null;
        }

        private void OnClickComboP2S1()
        {
            OnClickComboPag2Spine(1, ref _comboP2S1Showing, "P2S1");
        }

        private void OnClickComboP2S2()
        {
            OnClickComboPag2Spine(2, ref _comboP2S2Showing, "P2S2");
        }

        private void OnClickComboP2S3()
        {
            OnClickComboPag2Spine(3, ref _comboP2S3Showing, "P2S3");
        }

        private void OnClickComboP2S4()
        {
            OnClickComboPag2Spine(4, ref _comboP2S4Showing, "P2S4");
        }

        private void OnClickComboP2S5()
        {
            OnClickComboPag2Spine(5, ref _comboP2S5Showing, "P2S5");
        }

        private void OnClickComboPag2Spine(int spineIndex, ref bool showingFlag, string comboLabel)
        {
            if (showingFlag)
            {
                StopComboPag2SpinePlayback(spineIndex, ref showingFlag, comboLabel);
                return;
            }

            Debug.Log($"{PagLogPrefix} {comboLabel} clicked, play");
            showingFlag = true;
            if (mono == null)
            {
                Debug.LogWarning($"{PagLogPrefix} {comboLabel} play skipped: mono is null");
                showingFlag = false;
                return;
            }

            CancelComboPag2SpineCoroutine();
            _corComboPag2Spine = mono.StartCoroutine(PlayComboPag2SpineCoroutine(spineIndex, comboLabel));
        }

        private void OnClickComboP3S1()
        {
            OnClickComboPag3Spine(1, ref _comboP3S1Showing, "P3S1");
        }

        private void OnClickComboP3S2()
        {
            OnClickComboPag3Spine(2, ref _comboP3S2Showing, "P3S2");
        }

        private void OnClickComboP3S3()
        {
            OnClickComboPag3Spine(3, ref _comboP3S3Showing, "P3S3");
        }

        private void OnClickComboP3S4()
        {
            OnClickComboPag3Spine(4, ref _comboP3S4Showing, "P3S4");
        }

        private void OnClickComboP3S5()
        {
            OnClickComboPag3Spine(5, ref _comboP3S5Showing, "P3S5");
        }

        private void OnClickComboPag3Spine(int spineIndex, ref bool showingFlag, string comboLabel)
        {
            if (showingFlag)
            {
                StopComboPag3SpinePlayback(spineIndex, ref showingFlag, comboLabel);
                return;
            }

            Debug.Log($"{PagLogPrefix} {comboLabel} clicked, play");
            showingFlag = true;
            if (mono == null)
            {
                Debug.LogWarning($"{PagLogPrefix} {comboLabel} play skipped: mono is null");
                showingFlag = false;
                return;
            }

            CancelComboPag3SpineCoroutine();
            _corComboPag3Spine = mono.StartCoroutine(PlayComboPag3SpineCoroutine(spineIndex, comboLabel));
        }

        private void OnClickComboS2E1E2()
        {
            if (_comboS2E1E2Showing)
            {
                Debug.Log($"{PagLogPrefix} S2E1E2 clicked, stop");
                HidePagTestSpine(2);
                StopComboBorderMegaWinEffects12();
                _comboS2E1E2Showing = false;
                return;
            }

            Debug.Log($"{PagLogPrefix} S2E1E2 clicked, play");
            ShowPagTestSpine(2, PagTestSpine2PlayAnim);
            PlayComboBorderMegaWinEffects12();
            _comboS2E1E2Showing = true;
        }

        private void OnClickComboS1E1E2()
        {
            if (_comboS1E1E2Showing)
            {
                Debug.Log($"{PagLogPrefix} S1E1E2 clicked, stop");
                HidePagTestSpine(1);
                StopComboBorderMegaWinEffects12();
                _comboS1E1E2Showing = false;
                return;
            }

            Debug.Log($"{PagLogPrefix} S1E1E2 clicked, play");
            ShowPagTestSpine(1, PagTestSpine1PlayAnim);
            PlayComboBorderMegaWinEffects12();
            _comboS1E1E2Showing = true;
        }

        private void OnClickComboP9S1()
        {
            OnClickComboPag9Spine(1, ref _comboP9S1Showing, "P9S1");
        }

        private void OnClickComboP9S2()
        {
            OnClickComboPag9Spine(2, ref _comboP9S2Showing, "P9S2");
        }

        private void OnClickComboP9S3()
        {
            OnClickComboPag9Spine(3, ref _comboP9S3Showing, "P9S3");
        }

        private void OnClickComboP9S4()
        {
            OnClickComboPag9Spine(4, ref _comboP9S4Showing, "P9S4");
        }

        private void OnClickComboP9S5()
        {
            OnClickComboPag9Spine(5, ref _comboP9S5Showing, "P9S5");
        }

        private void OnClickComboPag9Spine(int spineIndex, ref bool showingFlag, string comboLabel)
        {
            if (showingFlag)
            {
                Debug.Log($"{PagLogPrefix} {comboLabel} clicked, stop");
                StopPagTestGlow(9);
                HidePagTestSpine(spineIndex);
                showingFlag = false;
                return;
            }

            Debug.Log($"{PagLogPrefix} {comboLabel} clicked, play");
            StartPagTest9PlaybackForCombo();
            ShowPagTestSpine(spineIndex, GetPagTestSpinePlayAnim(spineIndex));
            showingFlag = true;
        }

        private void OnClickComboEffectAll()
        {
            if (_comboEffectAllShowing)
            {
                Debug.Log($"{BorderMegaWinLogPrefix} Effect1-5 clicked, stop all");
                StopAllBorderMegaWinEffects();
                _comboEffectAllShowing = false;
                return;
            }

            Debug.Log($"{BorderMegaWinLogPrefix} Effect1-5 clicked, play all");
            PlayAllBorderMegaWinEffects();
            _comboEffectAllShowing = true;
        }

        private void OnClickComboSpineAll()
        {
            if (_comboSpineAllShowing)
            {
                Debug.Log($"{PagLogPrefix} Spine1-5 clicked, stop all");
                HideAllPagTestSpines();
                _comboSpineAllShowing = false;
                return;
            }

            Debug.Log($"{PagLogPrefix} Spine1-5 clicked, play all");
            ShowAllPagTestSpines();
            _comboSpineAllShowing = true;
        }

        private void OnClickComboP4S1()
        {
            OnClickComboPag4Spine(1, ref _comboP4S1Showing, "P4S1");
        }

        private void OnClickComboP4S2()
        {
            OnClickComboPag4Spine(2, ref _comboP4S2Showing, "P4S2");
        }

        private void OnClickComboP4S3()
        {
            OnClickComboPag4Spine(3, ref _comboP4S3Showing, "P4S3");
        }

        private void OnClickComboP4S4()
        {
            OnClickComboPag4Spine(4, ref _comboP4S4Showing, "P4S4");
        }

        private void OnClickComboP4S5()
        {
            OnClickComboPag4Spine(5, ref _comboP4S5Showing, "P4S5");
        }

        private void OnClickComboPag4Spine(int spineIndex, ref bool showingFlag, string comboLabel)
        {
            if (showingFlag)
            {
                Debug.Log($"{PagLogPrefix} {comboLabel} clicked, stop");
                StopPagTest4Playback();
                HidePagTestSpine(spineIndex);
                showingFlag = false;
                return;
            }

            Debug.Log($"{PagLogPrefix} {comboLabel} clicked, play");
            StartPagTest4PlaybackForCombo();
            ShowPagTestSpine(spineIndex, GetPagTestSpinePlayAnim(spineIndex));
            showingFlag = true;
        }

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

        /// <summary> 从本地快照恢复未完成的免费局（不自动请求 Spin，由玩家点转）。 </summary>
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

                //下注失败需要可以累计压分,最多10次
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

        //显示线和中奖图标
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

        //游戏状态重置
        private void OnGameReset()
        {
            if (corGameIdel != null) mono.StopCoroutine(corGameIdel);
            //mono.StopCoroutine(corEffectSlowMotion);
            slotMachineCtrl.isStopImmediately = false;
            slotMachineCtrl.CloseSlotCover();
            slotMachineCtrl.SkipWinLine(true);
        }

        //游戏状态闲置
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
        /// 断电重连恢复免费局：点击一次开始后自动跑完整段免费，并统一结算与切回普通游戏。
        /// </summary>
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

        //读取游戏配置
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

        private void OnStopSlot(EventData res)
        {

        }

        //机器按钮开始滚动
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

        //开始游戏
        void StartGameOnce(Action successCallback = null, Action<string> errorCallback = null)
        {
            ContentModel.Instance.totalPlaySpins = 1;
            ContentModel.Instance.remainPlaySpins = 1;
            corGameOnce = mono.StartCoroutine(GameOnce(successCallback, errorCallback));
        }

        //开始自动玩
        void StartGameAuto(Action successCallback = null, Action<string> errorCallback = null)
        {
            if (coGameAuto != null) mono.StopCoroutine(coGameAuto);
            coGameAuto = mono.StartCoroutine(GameAuto(successCallback, errorCallback));
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
        public IEnumerator ShowEffectReelsSlowMotion()
        {
            yield return new WaitUntil(() => isEffectSlowMotion2 == true);
        }

        //错误提示
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

        void GetMyCredit(Action<int> onSuccessCallback, Action<string> onErrorCallback)
        {

        }

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

        protected void SetUIFreeTimeBox(int freeSpinPlayTimes, int freeSpinTotalTimes)
        {
            gFreeTimeBox.visible = true;
            gFreeTimeBox.GetChild("numberGreen").asTextField.text= freeSpinPlayTimes.ToString();
            gFreeTimeBox.GetChild("numberYellow").asTextField.text = freeSpinTotalTimes.ToString();
        }

        //读取当前滚轴显示的图标
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

