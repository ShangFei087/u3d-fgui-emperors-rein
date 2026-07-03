using FairyGUI;
using GameMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SlotZhuZaiJinBi1700
{
    public class PageTest : PageBase
    {
        public const string pkgName = "SlotZhuZaiJinBi1700";
        public const string resName = "PageTest";

        private struct PagPresetConfig
        {
            public string PagFile;
            public float DisplayScale;
            public int HolderW;
            public int HolderH;
            public BlendMode BlendMode;
            public string Label;
        }

        private const string PagLogPrefix = "[1700 PageTest]";
        private const bool PagUseFguiTexture = true;
        private const int PagFguiMaxDisplaySide = 0;
        private const int PagFguiFps = 30;
        private const bool PagClampDisplayToHolder =false;
        private const int MaxPlaySlots = 8;
        private const int MaxPagGroupCount = 4;
        private const int NpcSlotIndexBase = 4;
        private const int MaxNpcSequenceCount = 4;

        /// <summary>为 true 时多路播放每 slot 使用不同 .pag（判别「只显示一路」vs「同素材叠加」）。</summary>
        private const bool PagDebugDistinctFilesPerSlot = false;

        /// <summary>为 true 时在 holder 上绘制彩色边框，便于肉眼确认四格布局。</summary>
        private const bool PagDebugShowHolderBorder = true;

        /// <summary>异文件调试：slot0=BigWin_1024, slot1=XingXing2, slot2=glow_loop_720, slot3=BigWin_1080。</summary>
        private static readonly int[] DistinctFilePresetIndices = { 0, 2, 4, 1 };

        private static readonly Color[] HolderDebugLineColors =
        {
            new Color(0f, 1f, 0f, 1f),
            new Color(0f, 1f, 1f, 1f),
            new Color(1f, 1f, 0f, 1f),
            new Color(1f, 0f, 1f, 1f),
            new Color(1f, 0.5f, 0f, 1f),
            new Color(0.5f, 0.8f, 1f, 1f),
            new Color(1f, 0.4f, 0.4f, 1f),
            new Color(0.6f, 1f, 0.4f, 1f),
        };

        private static readonly string[] SlotAnchorNames =
        {
            "PT1", "PT2", "PT3", "PT4", "PT5", "PT6", "PT7", "PT8",
        };

        private const int PagPresetCount = 5;

        private static readonly PagPresetConfig[] PagPresets =
        {
            new PagPresetConfig
            {
                PagFile = "BigWin_1024.pag",
                DisplayScale = 1f,
                HolderW = 500,
                HolderH = 500,
                BlendMode = BlendMode.Add,
                Label = "BigWin_1024",
            },
            new PagPresetConfig
            {
                PagFile = "BigWin_1080.pag",
                DisplayScale = 1f,
                HolderW = 1080,
                HolderH = 1920,
                BlendMode = BlendMode.Add,
                Label = "BigWin_1080",
            },
            new PagPresetConfig
            {
                PagFile = "XingXing2.pag",
                DisplayScale = 1f,
                HolderW = 500,
                HolderH = 500,
                BlendMode = BlendMode.Normal,
                Label = "XingXing2",
            },
            new PagPresetConfig
            {
                PagFile = "Lopp/glow_loop_full_1920.pag",
                DisplayScale = 1f,
                HolderW = 1080,
                HolderH = 1920,
                BlendMode = BlendMode.Normal,
                Label = "glow_loop_full_1920",
            },
            new PagPresetConfig
            {
                PagFile = "Lopp/glow_loop_720.pag",
                DisplayScale = 1f,
                HolderW = 1080,
                HolderH = 1920,
                BlendMode = BlendMode.Normal,
                Label = "glow_loop_720",
            },
        };

        private static readonly string[] SelPagButtonNames =
        {
            "btnBigWin_1024", "btnBigWin_1080", "btnXingXing2", "btnGlowLoopFull1920", "btnGlowLoop720",
        };

        private static readonly string[] PagGroupButtonNames =
        {
            "btnPagGroup1", "btnPagGroup2", "btnPagGroup3", "btnPagGroup4",
        };

        private static readonly string[] NpcSequenceButtonNames =
        {
            "btnBigwinNpc", "btnFreeNpc", "btnNormalNpc", "btnRewardNpc",
        };

        private const string NpcPagFolderPrefix = "3997Npc/";
        private const float NpcSequencePlayStartedTimeoutSec = 45f;
        private const float NpcSequenceSegmentDurationFallbackSec = 8f;
        /// <summary>NPC 多路同屏正式默认：纳入 PagGpuSyncGroup（≥28 FPS / 防整屏闪）。勿改 false。</summary>
        private const bool NpcSequenceUseGpuSyncGroup = true;

        private static readonly PagPresetConfig NpcSequenceSlotPreset = new PagPresetConfig
        {
            PagFile = string.Empty,
            DisplayScale = 1f,
            HolderW = 250,
            HolderH = 250,
            BlendMode = BlendMode.Normal,
            Label = "NPC",
        };

        private static readonly string[] NpcBigWinSequence =
        {
            $"{NpcPagFolderPrefix}BigWinNPC/bigwin_start1.pag",
            $"{NpcPagFolderPrefix}BigWinNPC/bigwin_idle1.pag",
            $"{NpcPagFolderPrefix}BigWinNPC/supwin_start1.pag",
            $"{NpcPagFolderPrefix}BigWinNPC/supwin_idle1.pag",
            $"{NpcPagFolderPrefix}BigWinNPC/megawin_start1.pag",
            $"{NpcPagFolderPrefix}BigWinNPC/megawin_idle_gq.pag",
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

        private static readonly string[][] NpcSequencePlaylists =
        {
            NpcBigWinSequence, NpcFreeSequence, NpcNormalSequence, NpcRewardSequence,
        };

        private static readonly string[] NpcSequenceLabels =
        {
            "bigwinNpc", "freeNpc", "normalNpc", "rewardNpc",
        };

        private readonly PagSlotBinding[] _slots = new PagSlotBinding[MaxPlaySlots];
        private readonly bool[] _slotPlaying = new bool[MaxPlaySlots];
        private readonly bool[] _cacheWarmed = new bool[PagPresetCount];

        private int _selectedPagIndex;
        private int _selectedPlayCount = 1;
        private int _playSessionId;
        private bool _pagButtonsBound;
        private GTextField _txtPagStatus;
        private Coroutine _corMultiPlay;
        private Coroutine _corDumpAfterPlay;
        private readonly Coroutine[] _corNpcSequence = new Coroutine[MaxNpcSequenceCount];
        private readonly bool[] _npcSequenceShowing = new bool[MaxNpcSequenceCount];
        private readonly int[] _npcSequenceSessionId = new int[MaxNpcSequenceCount];
        private bool _pagGroupPlaying;
        private int _activePagGroupCount;

        protected override void OnInit()
        {
            base.OnInit();
            int count = 1;
            Action callback = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam();
                }
            };
            callback();
        }

        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            InitParam();
        }

        public override void OnClose(EventData data = null)
        {
            StopAllPlayback();
            DisposePagResources();
            UnbindPagButtons();
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

            // PreloadPage 阶段 isOpen=false，须通知 PageManager 预加载完成。
            if (!isOpen)
            {
                preLoadedCallback?.Invoke();
                return;
            }

            PagCallbackHub.EnsureInstance();
            PagController.EnsureInit();
            PagConcurrentPlayback.Enabled = PagUseFguiTexture;

            _txtPagStatus = contentPane?.GetChild("txtPagStatus")?.asTextField;
            BindPagButtons();
            RefreshSelectionVisuals();
            RefreshPagStatusText();
        }

        private void BindPagButtons()
        {
            if (_pagButtonsBound || contentPane == null)
            {
                return;
            }

            for (int i = 0; i < SelPagButtonNames.Length; i++)
            {
                int index = i;
                BindButton(SelPagButtonNames[i], () => OnSelectPag(index));
            }

            for (int i = 0; i < PagGroupButtonNames.Length; i++)
            {
                int groupCount = i + 1;
                BindButton(PagGroupButtonNames[i], () => OnSelectPagGroup(groupCount));
            }

            BindButton("btnDumpPagSlots", () => DumpPagSlots("manual"));

            for (int i = 0; i < NpcSequenceButtonNames.Length; i++)
            {
                int index = i;
                BindButton(NpcSequenceButtonNames[i], () => OnClickNpcSequence(index));
            }

            _pagButtonsBound = true;
        }

        private void UnbindPagButtons()
        {
            if (!_pagButtonsBound || contentPane == null)
            {
                _pagButtonsBound = false;
                return;
            }

            for (int i = 0; i < SelPagButtonNames.Length; i++)
            {
                UnbindButton(SelPagButtonNames[i]);
            }

            for (int i = 0; i < PagGroupButtonNames.Length; i++)
            {
                UnbindButton(PagGroupButtonNames[i]);
            }

            UnbindButton("btnDumpPagSlots");

            for (int i = 0; i < NpcSequenceButtonNames.Length; i++)
            {
                UnbindButton(NpcSequenceButtonNames[i]);
            }

            _pagButtonsBound = false;
        }

        private void BindButton(string name, EventCallback0 handler)
        {
            GButton btn = contentPane.GetChild(name)?.asButton;
            if (btn == null)
            {
                Debug.LogWarning($"{PagLogPrefix} button missing: {name}");
                return;
            }

            btn.onClick.Clear();
            btn.onClick.Add(handler);
        }

        private void UnbindButton(string name)
        {
            contentPane.GetChild(name)?.asButton?.onClick.Clear();
        }

        private void OnSelectPag(int index)
        {
            if (index < 0 || index >= PagPresetCount)
            {
                return;
            }

            _selectedPagIndex = index;
            RefreshSelectionVisuals();
            RefreshPagStatusText();
            Debug.Log($"{PagLogPrefix} select pag={PagPresets[index].Label}");
        }

        private void OnSelectPagGroup(int groupCount)
        {
            if (groupCount < 1 || groupCount > MaxPagGroupCount)
            {
                return;
            }

            if (_pagGroupPlaying && _activePagGroupCount == groupCount)
            {
                StopPagGroupPlayback();
                RefreshSelectionVisuals();
                RefreshPagStatusText();
                Debug.Log($"{PagLogPrefix} PagGroup{groupCount} toggle off");
                return;
            }

            _selectedPlayCount = groupCount;
            _activePagGroupCount = groupCount;
            _pagGroupPlaying = true;
            RefreshSelectionVisuals();
            RefreshPagStatusText();
            Debug.Log($"{PagLogPrefix} PagGroup{groupCount} toggle on");
            StartMultiPlayback();
        }

        private void OnClickNpcSequence(int index)
        {
            if (index < 0 || index >= NpcSequencePlaylists.Length)
            {
                return;
            }

            string label = NpcSequenceLabels[index];
            int slotIndex = NpcSlotIndexBase + index;
            if (_npcSequenceShowing[index])
            {
                StopNpcSequencePlayback(index);
                RefreshPagStatusText();
                Debug.Log($"{PagLogPrefix} npc sequence stop: {label} on {SlotAnchorNames[slotIndex]}");
                return;
            }

            StopNpcSequencePlayback(index);
            _npcSequenceShowing[index] = true;
            int sessionId = ++_npcSequenceSessionId[index];
            RefreshPagStatusText();
            Debug.Log($"{PagLogPrefix} npc sequence start: {label} on {SlotAnchorNames[slotIndex]} "
                + $"syncGroup={NpcSequenceUseGpuSyncGroup}");
            _corNpcSequence[index] = PagCallbackHub.Instance.RunCoroutine(
                NpcSequencePlaybackCoroutine(NpcSequencePlaylists[index], label, slotIndex, index, sessionId));
        }

        private void StopAllNpcSequencePlayback()
        {
            for (int i = 0; i < MaxNpcSequenceCount; i++)
            {
                StopNpcSequencePlayback(i);
            }
        }

        private void StopNpcSequencePlayback(int npcIndex)
        {
            if (npcIndex < 0 || npcIndex >= MaxNpcSequenceCount)
            {
                return;
            }

            _npcSequenceSessionId[npcIndex]++;
            _npcSequenceShowing[npcIndex] = false;

            if (_corNpcSequence[npcIndex] != null)
            {
                PagCallbackHub.Instance.StopRunCoroutine(_corNpcSequence[npcIndex]);
                _corNpcSequence[npcIndex] = null;
            }

            StopSlot(NpcSlotIndexBase + npcIndex);
        }

        private IEnumerator NpcSequencePlaybackCoroutine(
            string[] sequence,
            string label,
            int slotIndex,
            int npcIndex,
            int sessionId)
        {
            if (sequence == null || sequence.Length == 0)
            {
                StopNpcSequencePlayback(npcIndex);
                yield break;
            }

            for (int i = 0; i < sequence.Length; i++)
            {
                yield return EnsurePagCompositionReady(sequence[i], false, _ => { });

                if (sessionId != _npcSequenceSessionId[npcIndex])
                {
                    yield break;
                }
            }

            if (!PrepareSlot(slotIndex, NpcSequenceSlotPreset))
            {
                Debug.LogError($"{PagLogPrefix} npc sequence {SlotAnchorNames[slotIndex]} prepare failed: {label}");
                StopNpcSequencePlayback(npcIndex);
                RefreshPagStatusText();
                yield break;
            }

            PagSlotBinding slot = _slots[slotIndex];
            PagController controller = slot?.Controller;
            if (controller == null)
            {
                Debug.LogError($"{PagLogPrefix} npc sequence controller missing: {label} on {SlotAnchorNames[slotIndex]}");
                StopNpcSequencePlayback(npcIndex);
                RefreshPagStatusText();
                yield break;
            }

            _slotPlaying[slotIndex] = true;
            RefreshPagStatusText();

            string positionType = "center";
            string layoutExtra = string.Empty;
            if (!BuildPagTestLayout(slot.FguiAnchor, out layoutExtra, out string layoutDebug))
            {
                Debug.LogWarning($"{PagLogPrefix} npc sequence layout fallback turntable ({layoutDebug})");
                controller.LayoutPagAuto("turntable");
            }
            else
            {
                Debug.Log($"{PagLogPrefix} npc sequence layout: {layoutExtra} ({layoutDebug})");
            }

            if (!slot.PreparePlay(PagUseFguiTexture, PagFguiMaxDisplaySide, PagFguiFps))
            {
                Debug.LogError($"{PagLogPrefix} npc sequence PreparePlay failed: {label}");
                StopNpcSequencePlayback(npcIndex);
                RefreshPagStatusText();
                yield break;
            }

            PagSegment[] segments = BuildPagSegments(sequence);

            if (!controller.PlayFguiGpuSequence(segments, positionType, layoutExtra, NpcSequenceUseGpuSyncGroup))
            {
                Debug.LogError($"{PagLogPrefix} npc sequence PlayFguiGpuSequence failed: {label}");
                StopNpcSequencePlayback(npcIndex);
                RefreshPagStatusText();
                yield break;
            }

            yield return WaitPagPlayStarted(slot, NpcSequencePlayStartedTimeoutSec);
            controller = slot?.Controller;
            if (controller == null || !controller.PlayStarted)
            {
                Debug.LogError($"{PagLogPrefix} npc sequence did not start within {NpcSequencePlayStartedTimeoutSec}s: {label}");
                StopNpcSequencePlayback(npcIndex);
                RefreshPagStatusText();
                yield break;
            }

            float totalTimeout = 0f;
            for (int i = 0; i < sequence.Length; i++)
            {
                totalTimeout += controller.GetCompositionDurationSecWithFallback(NpcSequenceSegmentDurationFallbackSec) + 1f;
            }

            totalTimeout += 3f;
            totalTimeout = Mathf.Max(totalTimeout, sequence.Length * NpcSequenceSegmentDurationFallbackSec + 5f);
            yield return controller.WaitForFguiGpuSequenceFinished(totalTimeout);

            if (sessionId != _npcSequenceSessionId[npcIndex])
            {
                yield break;
            }

            StopNpcSequencePlayback(npcIndex);
            RefreshPagStatusText();
            Debug.Log($"{PagLogPrefix} npc sequence finished: {label} on {SlotAnchorNames[slotIndex]}");
            _corNpcSequence[npcIndex] = null;
        }

        private static PagSegment[] BuildPagSegments(string[] sequence)
        {
            var segments = new PagSegment[sequence.Length];
            for (int i = 0; i < sequence.Length; i++)
            {
                segments[i] = new PagSegment(sequence[i], 1);
            }

            return segments;
        }

        private static IEnumerator WaitPagPlayStarted(PagSlotBinding slot, float timeoutSec)
        {
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

            if (!controller.PlayStarted)
            {
                Debug.LogWarning($"{PagLogPrefix} pag play started timeout ({timeoutSec}s), instance={slot.InstanceKey}");
            }
        }

        private void RefreshSelectionVisuals()
        {
            if (contentPane == null)
            {
                return;
            }

            for (int i = 0; i < SelPagButtonNames.Length; i++)
            {
                GButton btn = contentPane.GetChild(SelPagButtonNames[i])?.asButton;
                if (btn != null)
                {
                    btn.selected = i == _selectedPagIndex;
                }
            }

            for (int i = 0; i < PagGroupButtonNames.Length; i++)
            {
                GButton btn = contentPane.GetChild(PagGroupButtonNames[i])?.asButton;
                if (btn != null)
                {
                    btn.selected = _pagGroupPlaying && i + 1 == _activePagGroupCount;
                }
            }
        }

        private void RefreshPagStatusText()
        {
            if (_txtPagStatus == null)
            {
                return;
            }

            PagPresetConfig config = PagPresets[_selectedPagIndex];
            int playingCount = GetActiveSlotCount();
            string debugPart = PagDebugDistinctFilesPerSlot && _selectedPlayCount > 1 ? " 异文件=开" : string.Empty;
            string pagGroupPart = _pagGroupPlaying
                ? $" PagGroup{_activePagGroupCount}=播放中"
                : " PagGroup=关";
            var activeNpcLabels = new List<string>();
            for (int i = 0; i < MaxNpcSequenceCount; i++)
            {
                if (_npcSequenceShowing[i])
                {
                    activeNpcLabels.Add($"{NpcSequenceLabels[i]}@{SlotAnchorNames[NpcSlotIndexBase + i]}");
                }
            }

            string npcPart = activeNpcLabels.Count > 0 ? $" NPC=[{string.Join(",", activeNpcLabels)}]" : string.Empty;
            string playingPart = playingCount > 0
                ? $" 播放中={playingCount} SyncActive={PagGpuSyncGroup.IsActive} SyncMembers={PagConcurrentPlayback.ActiveMemberCount}{debugPart}{npcPart}"
                : npcPart;
            _txtPagStatus.text =
                $"PAG={config.Label}{pagGroupPart}{playingPart}";
        }

        /// <summary>每个 PT 实例仅含 pagEffect1；多路靠不同 PT 锚点区分。</summary>
        private static string GetPagLoaderName(int slotIndex) => "pagEffect1";

        private static string GetPagHolderName(int slotIndex) => "holder1";

        private PagPresetConfig ResolveSlotPreset(int slotIndex, PagPresetConfig selectedPreset, int playCount)
        {
            if (PagDebugDistinctFilesPerSlot && playCount > 1)
            {
                int presetIndex = DistinctFilePresetIndices[slotIndex % DistinctFilePresetIndices.Length];
                return PagPresets[presetIndex];
            }

            return selectedPreset;
        }

        private void StartMultiPlayback()
        {
            StopPagGroupPlaybackInternal(clearPlayingState: false);
            int sessionId = _playSessionId;
            PagPresetConfig config = PagPresets[_selectedPagIndex];

            for (int i = 0; i < _selectedPlayCount; i++)
            {
                _slotPlaying[i] = true;
            }

            for (int i = _selectedPlayCount; i < MaxPagGroupCount; i++)
            {
                _slotPlaying[i] = false;
            }

            RefreshPagStatusText();
            string mode = PagDebugDistinctFilesPerSlot && _selectedPlayCount > 1 ? "distinct-files" : "same-file";
            Debug.Log($"{PagLogPrefix} play {config.Label} x{_selectedPlayCount}, mode={mode}");
            _corMultiPlay = PagCallbackHub.Instance.RunCoroutine(
                StartMultiPlaybackCoroutine(sessionId, _selectedPagIndex, _selectedPlayCount));
        }

        private IEnumerator StartMultiPlaybackCoroutine(int sessionId, int presetIndex, int playCount)
        {
            PagPresetConfig selectedPreset = PagPresets[presetIndex];

            if (PagDebugDistinctFilesPerSlot && playCount > 1)
            {
                var warmedIndices = new HashSet<int>();
                for (int slotIndex = 0; slotIndex < playCount; slotIndex++)
                {
                    int warmIndex = DistinctFilePresetIndices[slotIndex % DistinctFilePresetIndices.Length];
                    if (!warmedIndices.Add(warmIndex))
                    {
                        continue;
                    }

                    PagPresetConfig slotPreset = PagPresets[warmIndex];
                    bool warmed = _cacheWarmed[warmIndex];
                    yield return EnsurePagCompositionReady(slotPreset.PagFile, warmed, ok => _cacheWarmed[warmIndex] = ok);

                    if (sessionId != _playSessionId)
                    {
                        yield break;
                    }
                }
            }
            else
            {
                bool warmed = _cacheWarmed[presetIndex];
                yield return EnsurePagCompositionReady(selectedPreset.PagFile, warmed, ok => _cacheWarmed[presetIndex] = ok);

                if (sessionId != _playSessionId)
                {
                    yield break;
                }
            }

            var activeSlots = new List<PagSlotBinding>(playCount);
            var pagFilesPerSlot = new List<string>(playCount);

            for (int slotIndex = 0; slotIndex < playCount; slotIndex++)
            {
                if (sessionId != _playSessionId || slotIndex >= MaxPlaySlots || !_slotPlaying[slotIndex])
                {
                    continue;
                }

                PagPresetConfig slotPreset = ResolveSlotPreset(slotIndex, selectedPreset, playCount);
                if (PrepareSlot(slotIndex, slotPreset))
                {
                    activeSlots.Add(_slots[slotIndex]);
                    pagFilesPerSlot.Add(slotPreset.PagFile);
                }
                else
                {
                    _slotPlaying[slotIndex] = false;
                }
            }

            if (sessionId != _playSessionId || activeSlots.Count == 0)
            {
                _corMultiPlay = null;
                RefreshPagStatusText();
                yield break;
            }

            Debug.Log($"{PagLogPrefix} group-play {activeSlots.Count}/{playCount}, preset={selectedPreset.Label}, files=[{string.Join(", ", pagFilesPerSlot)}]");

            if (PagDebugDistinctFilesPerSlot && playCount > 1)
            {
                _corMultiPlay = PagGroupPlayer.PlayOnSlots(
                    pagFilesPerSlot,
                    activeSlots,
                    BuildPagTestLayout,
                    PagUseFguiTexture,
                    PagFguiMaxDisplaySide,
                    PagFguiFps,
                    PagLogPrefix,
                    -1,
                    OnSlotPlayFailed);
            }
            else
            {
                _corMultiPlay = PagGroupPlayer.PlayOnSlots(
                    selectedPreset.PagFile,
                    activeSlots,
                    BuildPagTestLayout,
                    PagUseFguiTexture,
                    PagFguiMaxDisplaySide,
                    PagFguiFps,
                    PagLogPrefix,
                    -1,
                    OnSlotPlayFailed);
            }

            RefreshPagStatusText();
            ScheduleDumpAfterPlay(sessionId);
        }

        private void ScheduleDumpAfterPlay(int sessionId)
        {
            if (_corDumpAfterPlay != null)
            {
                PagCallbackHub.Instance.StopRunCoroutine(_corDumpAfterPlay);
                _corDumpAfterPlay = null;
            }

            _corDumpAfterPlay = PagCallbackHub.Instance.RunCoroutine(DumpAfterPlayCoroutine(sessionId));
        }

        private IEnumerator DumpAfterPlayCoroutine(int sessionId)
        {
            yield return new WaitForSeconds(0.5f);
            if (sessionId == _playSessionId)
            {
                DumpPagSlots("post-play-0.5s");
            }

            yield return new WaitForSeconds(1.5f);
            if (sessionId == _playSessionId)
            {
                DumpPagSlots("post-play-2s");
            }

            _corDumpAfterPlay = null;
        }

        private void DumpPagSlots(string reason)
        {
            Debug.Log($"{PagLogPrefix} === DumpPagSlots ({reason}) SyncActive={PagGpuSyncGroup.IsActive} SyncMembers={PagGpuSyncGroup.MemberCount} ===");

            for (int slotIndex = 0; slotIndex < MaxPlaySlots; slotIndex++)
            {
                PagSlotBinding slot = _slots[slotIndex];
                GComponent anchor = GetSlotAnchor(slotIndex);
                string loaderName = GetPagLoaderName(slotIndex);
                GLoader anchorLoader = anchor?.GetChild(loaderName)?.asLoader;
                GLoader boundLoader = slot?.Controller?.FguiLoader;
                GLoader reportLoader = boundLoader ?? anchorLoader;

                float globalX = 0f;
                float globalY = 0f;
                if (reportLoader != null)
                {
                    Vector2 globalPos = reportLoader.LocalToGlobal(Vector2.zero);
                    globalX = globalPos.x;
                    globalY = globalPos.y;
                }

                bool hasTexture = reportLoader?.texture != null;
                bool loadersMatch = anchorLoader == boundLoader;
                Debug.Log($"{PagLogPrefix} slot[{slotIndex}] playing={_slotPlaying[slotIndex]} "
                    + $"key={slot?.InstanceKey ?? "null"} anchor={anchor?.name ?? "null"} "
                    + $"loader={loaderName} visible={reportLoader?.visible} hasTex={hasTexture} "
                    + $"size={reportLoader?.width:F0}x{reportLoader?.height:F0} global=({globalX:F0},{globalY:F0}) "
                    + $"boundMatch={loadersMatch} gpuReady={slot?.Controller?.GpuDisplayReady} "
                    + $"playStarted={slot?.Controller?.PlayStarted}");
            }
        }

        private bool PrepareSlot(int slotIndex, PagPresetConfig config)
        {
            GComponent anchor = GetSlotAnchor(slotIndex);
            if (anchor == null)
            {
                Debug.LogError($"{PagLogPrefix} slot={slotIndex} anchor missing: {SlotAnchorNames[slotIndex]}");
                return false;
            }

            ApplyHolderSize(anchor, slotIndex, config.HolderW, config.HolderH);
            ApplyPagLoaderBlendMode(anchor, slotIndex, config.BlendMode);
            EnsurePagSlot(slotIndex, anchor);
            _slots[slotIndex].SetFguiDisplayScale(config.DisplayScale);
            _slots[slotIndex].SetFguiClampDisplayToHolder(PagClampDisplayToHolder);
            return true;
        }

        private bool BuildPagTestLayout(GComponent anchor, out string extra, out string debugReason)
        {
            extra = string.Empty;
            debugReason = "turntable";

            for (int i = 0; i < MaxPlaySlots; i++)
            {
                PagSlotBinding slot = _slots[i];
                if (slot?.FguiAnchor != anchor)
                {
                    continue;
                }

                slot.Controller.LayoutPagAuto("turntable");
                return true;
            }

            return false;
        }

        private void OnSlotPlayFailed(string instanceKey, string message)
        {
            Debug.LogError($"{PagLogPrefix} slot failed: {instanceKey}, {message}");
            for (int i = 0; i < MaxPlaySlots; i++)
            {
                if (_slots[i]?.InstanceKey == instanceKey)
                {
                    _slotPlaying[i] = false;
                    break;
                }
            }

            RefreshPagStatusText();
        }

        private void StopSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MaxPlaySlots)
            {
                return;
            }

            if (_slots[slotIndex]?.Controller != null)
            {
                _slots[slotIndex].Stop(PagUseFguiTexture);
            }

            GComponent anchor = GetSlotAnchor(slotIndex);
            if (anchor != null)
            {
                ApplyPagLoaderBlendMode(anchor, slotIndex, BlendMode.Normal);
                ResetHolderDebugBorder(anchor, slotIndex);
            }

            _slotPlaying[slotIndex] = false;
        }

        private void StopPagGroupPlaybackInternal(bool clearPlayingState)
        {
            _playSessionId++;

            if (clearPlayingState)
            {
                _pagGroupPlaying = false;
                _activePagGroupCount = 0;
            }

            if (_corDumpAfterPlay != null)
            {
                PagCallbackHub.Instance.StopRunCoroutine(_corDumpAfterPlay);
                _corDumpAfterPlay = null;
            }

            if (_corMultiPlay != null)
            {
                PagCallbackHub.Instance.StopRunCoroutine(_corMultiPlay);
                _corMultiPlay = null;
            }

            for (int i = 0; i < MaxPagGroupCount; i++)
            {
                StopSlot(i);
            }
        }

        private void StopPagGroupPlayback()
        {
            StopPagGroupPlaybackInternal(clearPlayingState: true);
            RefreshPagStatusText();
        }

        private void StopAllPlayback()
        {
            StopAllNpcSequencePlayback();
            StopPagGroupPlaybackInternal(clearPlayingState: true);

            for (int i = MaxPagGroupCount; i < MaxPlaySlots; i++)
            {
                StopSlot(i);
            }

            RefreshPagStatusText();
        }

        private int GetActiveSlotCount()
        {
            int count = 0;
            for (int i = 0; i < MaxPlaySlots; i++)
            {
                if (_slotPlaying[i])
                {
                    count++;
                }
            }

            return count;
        }

        private GComponent GetSlotAnchor(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= SlotAnchorNames.Length)
            {
                return null;
            }

            return contentPane?.GetChild(SlotAnchorNames[slotIndex])?.asCom;
        }

        private static void ApplyHolderSize(GComponent anchor, int slotIndex, int width, int height)
        {
            GGraph holder = anchor?.GetChild(GetPagHolderName(slotIndex))?.asGraph;
            if (holder == null)
            {
                Debug.LogWarning($"{PagLogPrefix} holder missing on anchor={anchor?.name}, holder={GetPagHolderName(slotIndex)}");
                return;
            }

            holder.SetSize(width, height);
            ApplyHolderDebugBorder(holder, slotIndex, width, height);
        }

        private static void ApplyHolderDebugBorder(GGraph holder, int slotIndex, int width, int height)
        {
            if (!PagDebugShowHolderBorder || holder == null)
            {
                return;
            }

            Color lineColor = HolderDebugLineColors[slotIndex % HolderDebugLineColors.Length];
            holder.visible = true;
            holder.DrawRect(width, height, 2, lineColor, new Color(0f, 0f, 0f, 0f));
        }

        private static void ResetHolderDebugBorder(GComponent anchor, int slotIndex)
        {
            if (!PagDebugShowHolderBorder)
            {
                return;
            }

            GGraph holder = anchor?.GetChild(GetPagHolderName(slotIndex))?.asGraph;
            if (holder != null)
            {
                holder.visible = false;
            }
        }

        private static void ApplyPagLoaderBlendMode(GComponent anchor, int slotIndex, BlendMode blendMode)
        {
            GLoader loader = anchor?.GetChild(GetPagLoaderName(slotIndex))?.asLoader;
            if (loader == null)
            {
                Debug.LogWarning($"{PagLogPrefix} pagEffect loader missing on anchor={anchor?.name}, loader={GetPagLoaderName(slotIndex)}");
                return;
            }

            if (loader.blendMode != blendMode)
            {
                loader.blendMode = blendMode;
            }
        }

        private void EnsurePagSlot(int slotIndex, GComponent anchor)
        {
            if (_slots[slotIndex] == null)
            {
                _slots[slotIndex] = new PagSlotBinding($"PageTestSlot{slotIndex}");
            }

            _slots[slotIndex].Attach(anchor, GetPagLoaderName(slotIndex));
        }

        private static bool IsPagCompositionReady(string pagFileName)
        {
            if (!PagPathHelper.IsCached(pagFileName))
            {
                return false;
            }

            string absPath = PagController.ResolvePagPath(pagFileName, PagPathHelper.DefaultGamePagFolder);
            return PagController.IsCompositionCached(absPath);
        }

        private static IEnumerator EnsurePagCompositionReady(string pagFileName, bool alreadyWarmed, Action<bool> onDone)
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

        private void DisposePagResources()
        {
            for (int i = 0; i < MaxPlaySlots; i++)
            {
                _slots[i]?.Dispose();
                _slots[i] = null;
                _slotPlaying[i] = false;
            }

            Array.Clear(_cacheWarmed, 0, _cacheWarmed.Length);
        }
    }
}
