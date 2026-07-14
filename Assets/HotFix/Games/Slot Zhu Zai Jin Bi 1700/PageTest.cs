using FairyGUI;
using GameMaker;
using Spine.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlotZhuZaiJinBi1700
{
    /// <summary>
    /// 1700 PAG / Spine / Effect 测试页：从 PageGameMain 迁出的媒体调试区。
    /// </summary>
    public class PageTest : PageBase
    {
        public const string pkgName = "SlotZhuZaiJinBi1700";
        public const string resName = "PageTest";

        private bool _backButtonBound;
        private bool _prefabsLoaded;
        private bool _prefabsLoading;
        private Action _pendingPrefabsReady;

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

        /// <summary>等待 Native PlayStarted 回调的超时（秒）。</summary>
        private const float PagTestPlayStartedTimeoutSec = 45f;
        /// <summary>NPC 序列单段时长兜底（秒）。</summary>
        private const float PagTestNpcSegmentDurationFallbackSec = 8f;
        /// <summary>相对 GameRes 的本游戏 PAG 目录（与 PopupGameLoading.GamePagFolder 保持一致）。</summary>
        private const string GamePagFolder = "Games/Slot Zhu Zai Jin Bi 1700/Pag";
        private const string PagLogPrefix = "[1700 PageTest]";
        /// <summary>Phase0 A/B：true 时全屏播 PAG；Phase1 通过后保持 false，走 FGUI extra 对齐。</summary>
        private const bool PagTestDebugFullScreen = false;
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
        private int _comboActivePagIndex;
        private int _comboActiveSpineIndex;
        private int _comboActiveEffectIndex;
        private PagController _comboGpuReadyController;
        private Action _comboGpuReadyHandler;
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

        /// <summary>
        /// 绑定 PageTest 上 PAG1~12、Spine1~5 测试按钮（InitParam / 语言切换后）
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();
            isInit = true;
            InitParam();
        }

        /// <summary>TestBigWin + BorderMegaWin：Loading 预加载阶段后台预热；首次打开时若未完成则等待加载。</summary>
        private void EnsurePrefabsLoaded(Action onReady)
        {
            if (_prefabsLoaded)
            {
                onReady?.Invoke();
                return;
            }

            if (_prefabsLoading)
            {
                if (onReady != null)
                {
                    _pendingPrefabsReady += onReady;
                }

                return;
            }

            _prefabsLoading = true;
            if (onReady != null)
            {
                _pendingPrefabsReady += onReady;
            }

            int count = 1 + BorderMegaWinPrefabNames.Length;
            Action callback = () =>
            {
                if (--count != 0)
                {
                    return;
                }

                _prefabsLoaded = true;
                _prefabsLoading = false;
                Debug.Log("[1700 PageTest] prefabs warmup finished");
                Action pending = _pendingPrefabsReady;
                _pendingPrefabsReady = null;
                pending?.Invoke();
            };

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
        }

        protected override void OnLanguageChange(I18nLang lang)
        {
            ClearBorderMegaWinButtons();
            ClearComboPlayControls();
            ClearPagTestButtons();
            ClearBackButton();
            FguiI18nTextAssistant.Instance.DisposeAllTranslate(contentPane);
            contentPane.Dispose();
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            InitParam();
        }

        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            InitParam();
        }

        public override void OnClose(EventData data = null)
        {
            StopAllPagTest();
            StopAllBorderMegaWinEffects();
            ClearBorderMegaWinButtons();
            ClearComboPlayControls();
            ClearPagTestButtons();
            ClearBackButton();
            DisposeBorderMegaWinEffects();
            DisposePagTestResources();
            TestUtils.CheckTestManager();
            base.OnClose(data);
        }

        public override void OnTop()
        {
            DebugUtils.Log($"i am top {name}");
        }

        public override void InitParam()
        {
            if (!isInit)
            {
                return;
            }

            if (!isOpen)
            {
                EnsurePrefabsLoaded(null);
                preLoadedCallback?.Invoke();
                return;
            }

            EnsurePrefabsLoaded(InitParamAfterPrefabsLoaded);
        }

        private void InitParamAfterPrefabsLoaded()
        {
            if (!isInit || !isOpen)
            {
                return;
            }

            PagBootstrap.EnsureReady();

            GComponent localPagTestAnchor = contentPane.GetChild("anchorPagTest")?.asCom;
            if (localPagTestAnchor != null && _anchorPagTest != localPagTestAnchor)
            {
                GameCommon.FguiUtils.DeleteWrapper(_anchorPagTest);
                _clonePagTest = GameObject.Instantiate(_goPagTestPrefab);
                _anchorPagTest = localPagTestAnchor;
                GameCommon.FguiUtils.AddWrapper(_anchorPagTest, _clonePagTest);
                DisposeAllPagTestSlotBindings();
                ResetPagTestSpineRefs();
                DisposeBorderMegaWinEffects();
                EnsurePagTestSpines();
            }

            EnsurePagTestSlots();
            EnsurePagTestSpines();
            BindPagTestButtons();
            EnsureBorderMegaWinEffects();
            BindBorderMegaWinButtons();
            BindComboPlayControls();
            BindBackButton();
        }

        private void BindBackButton()
        {
            if (_backButtonBound || contentPane == null)
            {
                return;
            }

            GButton btnBack = contentPane.GetChild("btnBack")?.asButton;
            if (btnBack == null)
            {
                Debug.LogWarning($"{PagLogPrefix} button missing: btnBack");
                return;
            }

            btnBack.onClick.Clear();
            btnBack.onClick.Add(OnClickBackButton);
            _backButtonBound = true;
        }

        private void ClearBackButton()
        {
            if (!_backButtonBound || contentPane == null)
            {
                _backButtonBound = false;
                return;
            }

            contentPane.GetChild("btnBack")?.asButton?.onClick.Clear();
            _backButtonBound = false;
        }

        private void OnClickBackButton()
        {
            PageManager.Instance.ClosePage(PageName.SlotZhuZaiJinPageTest);
        }

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
                case 0:
                    config = new PagTestPlaybackConfig
                    {
                        Kind = PagTestPlaybackKind.SingleLoop,
                        PagFile = PagTestName2,
                        DisplayScale = PagPresentationDefaults.DisplayScale,
                        LazyBindSlot = false,
                    };
                    return true;
                case 1:
                    config = new PagTestPlaybackConfig
                    {
                        Kind = PagTestPlaybackKind.SingleLoop,
                        PagFile = PagTestName1,
                        DisplayScale = PagPresentationDefaults.DisplayScale,
                        LazyBindSlot = false,
                    };
                    return true;
                case 2:
                    config = new PagTestPlaybackConfig
                    {
                        Kind = PagTestPlaybackKind.SingleLoop,
                        PagFile = PagTestName3,
                        DisplayScale = PagPresentationDefaults.DisplayScale,
                        LazyBindSlot = false,
                    };
                    return true;
                case 3:
                    config = new PagTestPlaybackConfig
                    {
                        Kind = PagTestPlaybackKind.SingleLoop,
                        PagFile = PagGlowLoop720,
                        DisplayScale = PagPresentationDefaults.DisplayScale,
                        LazyBindSlot = true,
                    };
                    return true;
                case 4:
                    config = new PagTestPlaybackConfig
                    {
                        Kind = PagTestPlaybackKind.SingleLoop,
                        PagFile = PagGlowLoop720,
                        DisplayScale = PagPresentationDefaults.DisplayScale,
                        LazyBindSlot = true,
                    };
                    return true;
                case 5:
                    config = new PagTestPlaybackConfig
                    {
                        Kind = PagTestPlaybackKind.SingleLoop,
                        PagFile = PagGlowLoopHalf,
                        DisplayScale = PagPresentationDefaults.DisplayScale,
                        LazyBindSlot = true,
                    };
                    return true;
                case 6:
                    config = new PagTestPlaybackConfig
                    {
                        Kind = PagTestPlaybackKind.SingleLoop,
                        PagFile = PagGlowLoopFull,
                        DisplayScale = PagPresentationDefaults.DisplayScale,
                        LazyBindSlot = true,
                    };
                    return true;
                case 7:
                    config = new PagTestPlaybackConfig
                    {
                        Kind = PagTestPlaybackKind.IntroLoop,
                        IntroFile = PagGlowInFull,
                        LoopFile = PagGlowLoopFull,
                        DisplayScale = PagPresentationDefaults.DisplayScale,
                        LazyBindSlot = true,
                    };
                    return true;
                case 8:
                    config = new PagTestPlaybackConfig
                    {
                        Kind = PagTestPlaybackKind.Sequence,
                        Sequence = NpcBigWinSequence,
                        DisplayScale = PagPresentationDefaults.DisplayScale,
                        Label = "BigWin",
                    };
                    return true;
                case 9:
                    config = new PagTestPlaybackConfig
                    {
                        Kind = PagTestPlaybackKind.Sequence,
                        Sequence = NpcFreeSequence,
                        DisplayScale = PagPresentationDefaults.DisplayScale,
                        LazyBindSlot = true,
                        Label = PagTestNpcLabels[0],
                    };
                    return true;
                case 10:
                    config = new PagTestPlaybackConfig
                    {
                        Kind = PagTestPlaybackKind.Sequence,
                        Sequence = NpcNormalSequence,
                        DisplayScale = PagPresentationDefaults.DisplayScale,
                        LazyBindSlot = true,
                        Label = PagTestNpcLabels[1],
                    };
                    return true;
                case 11:
                    config = new PagTestPlaybackConfig
                    {
                        Kind = PagTestPlaybackKind.Sequence,
                        Sequence = NpcRewardSequence,
                        DisplayScale = PagPresentationDefaults.DisplayScale,
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

        private void StopPagTestPlaybackFailed(int pagIndex)
        {
            StopPagTestPlayback(pagIndex, stopComboIfActive: false);
        }

        private PagPlayLayout BuildPagTestPlayLayout()
        {
            if (PagTestDebugFullScreen)
            {
                return PagPlayLayout.Fullscreen;
            }

            if (TryBuildPagTestLayoutExtra(out string layoutExtra, out string layoutDebug))
            {
                Debug.Log($"{PagLogPrefix} layout extra: {layoutExtra} ({layoutDebug})");
                return new PagPlayLayout("center", layoutExtra, false);
            }

            return PagPlayLayout.Center;
        }

        private float CalcNpcSequenceFinishedTimeout(PagController controller, string[] sequence)
        {
            if (sequence == null || sequence.Length == 0 || controller == null)
            {
                return PagTestNpcSegmentDurationFallbackSec + 5f;
            }

            float totalTimeout = 3f;
            for (int i = 0; i < sequence.Length; i++)
            {
                totalTimeout += controller.GetCompositionDurationSecWithFallback(PagTestNpcSegmentDurationFallbackSec) + 1f;
            }

            return Mathf.Max(totalTimeout, sequence.Length * PagTestNpcSegmentDurationFallbackSec + 5f);
        }

        private bool RunPagTestPlayback(int pagIndex, PagTestPlaybackConfig config)
        {
            if (!IsPagTestShowing(pagIndex))
            {
                return false;
            }

            PagSlotBinding slot = GetPagTestSlotByPagIndex(pagIndex);
            if (slot == null)
            {
                Debug.LogError($"{PagLogPrefix} PAG{pagIndex} slot missing");
                StopPagTestPlaybackFailed(pagIndex);
                return false;
            }

            PagPlayLayout layout = BuildPagTestPlayLayout();
            int capturedPagIndex = pagIndex;

            switch (config.Kind)
            {
                case PagTestPlaybackKind.SingleLoop:
                    bool singleOk = slot.Play(config.PagFile, -1, layout, config.DisplayScale);
                    if (!singleOk)
                    {
                        StopPagTestPlaybackFailed(capturedPagIndex);
                        return false;
                    }

                    if (!Mathf.Approximately(config.DisplayScale, PagPresentationDefaults.DisplayScale))
                    {
                        slot.Controller?.SyncFguiDisplayLayoutFromComposition();
                    }

                    Debug.Log($"{PagLogPrefix} PAG{capturedPagIndex}: native loop repeat=-1, {config.PagFile}, scale={config.DisplayScale}");
                    return true;

                case PagTestPlaybackKind.IntroLoop:
                    var introCallbacks = new PagPlayCallbacks(
                        onStarted: () => Debug.Log($"{PagLogPrefix} PAG{capturedPagIndex}: intro->loop sequence started, scale={config.DisplayScale}"),
                        onFailed: () => StopPagTestPlaybackFailed(capturedPagIndex),
                        startedTimeoutSec: PagTestPlayStartedTimeoutSec);
                    bool introOk = slot.Play(new PagSequencePlay(
                        PagPlaySpecs.IntroLoop(config.IntroFile, config.LoopFile),
                        layout,
                        config.DisplayScale,
                        PagPresentationDefaults.UseGpuSyncGroup,
                        introCallbacks));
                    if (!introOk)
                    {
                        StopPagTestPlaybackFailed(capturedPagIndex);
                    }

                    return introOk;

                case PagTestPlaybackKind.Sequence:
                    string label = config.Label;
                    float finishedTimeout = CalcNpcSequenceFinishedTimeout(slot.Controller, config.Sequence);
                    var seqCallbacks = new PagPlayCallbacks(
                        onFinished: () =>
                        {
                            SetPagTestShowing(capturedPagIndex, false);
                            Debug.Log($"{PagLogPrefix} PAG{capturedPagIndex} npc sequence finished: {label}");
                        },
                        onFailed: () => StopPagTestPlaybackFailed(capturedPagIndex),
                        finishedTimeoutSec: finishedTimeout,
                        stopAfterFinished: true);
                    bool seqOk = slot.Play(new PagSequencePlay(
                        PagPlaySpecs.FromFiles(config.Sequence),
                        layout,
                        config.DisplayScale,
                        PagPresentationDefaults.UseGpuSyncGroup,
                        seqCallbacks));
                    if (!seqOk)
                    {
                        StopPagTestPlaybackFailed(capturedPagIndex);
                    }

                    return seqOk;

                default:
                    return false;
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

            _pagTestSlotBindings[loaderIndex].EnsureSlot(anchor, PagTestLoaderNames[loaderIndex]);
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

            StopPagTest(pagIndex);
            SetPagTestShowing(pagIndex, true);
            return RunPagTestPlayback(pagIndex, config);
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
            StopPagTest(pagIndex);
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

            slot.StopWithDefaults();
            SetPagTestShowing(pagIndex, false);

            Debug.Log($"{PagLogPrefix} StopPagTest pagIndex={pagIndex} instance={slot.InstanceKey}");
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
        /// 读取下拉选择并启动组合播放（事件驱动，无协程）
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

            string comboLabel = BuildComboSelectionLabel(pagIndex, spineIndex, effectIndex);
            Debug.Log($"{PagLogPrefix} Combo clicked, play {comboLabel}");
            _comboActivePagIndex = pagIndex;
            _comboActiveSpineIndex = spineIndex;
            _comboActiveEffectIndex = effectIndex;
            _comboPlayActive = true;
            UnsubscribeComboGpuDisplayReady();
            PlayComboDispatch(pagIndex, spineIndex, effectIndex, comboLabel);
        }

        /// <summary>
        /// 停止当前 Combo 组合播放并清理 PAG / Spine / Effect 状态
        /// </summary>
        private void StopComboPlayback()
        {
            UnsubscribeComboGpuDisplayReady();

            if (!_comboPlayActive)
            {
                return;
            }

            int pagIndex = _comboActivePagIndex;
            int spineIndex = _comboActiveSpineIndex;
            int effectIndex = _comboActiveEffectIndex;
            _comboPlayActive = false;
            _comboActivePagIndex = 0;
            _comboActiveSpineIndex = 0;
            _comboActiveEffectIndex = 0;

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
        /// 按 pagIndex 停止 Combo 触发的 PAG 播放（不递归调用 StopComboPlayback）
        /// </summary>
        /// <param name="pagIndex"> PAG 按钮下标（1~12）</param>
        private void StopComboPagPlayback(int pagIndex)
        {
            StopPagTestPlayback(pagIndex, stopComboIfActive: false);
        }

        /// <summary>
        /// Combo 主分发：按 PAG / Spine / Effect 组合复用同步播放逻辑
        /// </summary>
        private void PlayComboDispatch(int pagIndex, int spineIndex, int effectIndex, string comboLabel)
        {
            if (pagIndex > 0 && spineIndex > 0)
            {
                if (pagIndex == 2 || pagIndex == 3 || pagIndex == 1 || (pagIndex >= 10 && pagIndex <= 12))
                {
                    PlayComboPagSpineGpuSync(pagIndex, spineIndex, comboLabel);
                }
                else if (pagIndex == 4 || (pagIndex >= 5 && pagIndex <= 9))
                {
                    if (!StartPagTestPlayback(pagIndex))
                    {
                        _comboPlayActive = false;
                        return;
                    }

                    ShowPagTestSpine(spineIndex, GetPagTestSpinePlayAnim(spineIndex));
                    Debug.Log($"{PagLogPrefix} {comboLabel} PAG{pagIndex}+Spine started");
                }
                else
                {
                    Debug.LogWarning($"{PagLogPrefix} {comboLabel} unsupported PAG+Spine pagIndex={pagIndex}");
                    _comboPlayActive = false;
                    return;
                }
            }
            else if (pagIndex > 0)
            {
                if (!StartPagTestPlayback(pagIndex))
                {
                    _comboPlayActive = false;
                    return;
                }

                Debug.Log($"{PagLogPrefix} {comboLabel} PAG{pagIndex} only");
            }
            else if (spineIndex > 0)
            {
                ShowPagTestSpine(spineIndex, GetPagTestSpinePlayAnim(spineIndex));
                Debug.Log($"{PagLogPrefix} {comboLabel} Spine only");
            }

            if (!_comboPlayActive)
            {
                return;
            }

            if (effectIndex > 0)
            {
                PlayBorderMegaWinEffect(effectIndex - 1);
                Debug.Log($"{BorderMegaWinLogPrefix} {comboLabel} Effect{effectIndex}");
            }
        }

        /// <summary>
        /// PAG+Spine：等 GPU 纹理就绪后同步显示 Spine
        /// </summary>
        private void PlayComboPagSpineGpuSync(int pagIndex, int spineIndex, string comboLabel)
        {
            if (!StartPagTestPlayback(pagIndex))
            {
                _comboPlayActive = false;
                return;
            }

            PagController controller = GetPagTestSlotByPagIndex(pagIndex)?.Controller;
            int capturedPag = pagIndex;
            int capturedSpine = spineIndex;

            if (controller == null)
            {
                ShowPagTestSpine(spineIndex, GetPagTestSpinePlayAnim(spineIndex));
                Debug.Log($"{PagLogPrefix} {comboLabel} spine shown (no controller)");
                return;
            }

            if (controller.GpuDisplayReady)
            {
                if (_comboPlayActive && _comboActivePagIndex == capturedPag && _comboActiveSpineIndex == capturedSpine)
                {
                    ShowPagTestSpine(capturedSpine, GetPagTestSpinePlayAnim(capturedSpine));
                    Debug.Log($"{PagLogPrefix} {comboLabel} spine synced at texture display ready");
                }

                return;
            }

            UnsubscribeComboGpuDisplayReady();
            _comboGpuReadyController = controller;
            _comboGpuReadyHandler = () =>
            {
                if (!_comboPlayActive || _comboActivePagIndex != capturedPag || _comboActiveSpineIndex != capturedSpine)
                {
                    UnsubscribeComboGpuDisplayReady();
                    return;
                }

                UnsubscribeComboGpuDisplayReady();
                ShowPagTestSpine(capturedSpine, GetPagTestSpinePlayAnim(capturedSpine));
                Debug.Log($"{PagLogPrefix} {comboLabel} spine synced at texture display ready");
            };
            controller.OnGpuDisplayReady += _comboGpuReadyHandler;
        }

        /// <summary>
        /// 取消 Combo 对 OnGpuDisplayReady 的订阅
        /// </summary>
        private void UnsubscribeComboGpuDisplayReady()
        {
            if (_comboGpuReadyController != null && _comboGpuReadyHandler != null)
            {
                _comboGpuReadyController.OnGpuDisplayReady -= _comboGpuReadyHandler;
            }

            _comboGpuReadyController = null;
            _comboGpuReadyHandler = null;
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
    }
}
