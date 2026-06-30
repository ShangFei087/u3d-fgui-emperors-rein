using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 多 Pag 实例 GPU 帧同步组：批量 flush、统一 RequestNextGpuFrame、动态 Join/Leave。
/// PagGroupPlayer 使用静态 BeginGroup；单实例/后加入实例通过 TryJoin 动态合组。
/// </summary>
public static class PagGpuSyncGroup
{
    private static readonly HashSet<string> s_members = new HashSet<string>();
    private static readonly HashSet<string> s_boundMembers = new HashSet<string>();
    /// <summary>已完成 TryStartAllPlayback / IntegrateLateMember，可参与 batch 屏障的成员。</summary>
    private static readonly HashSet<string> s_syncReadyMembers = new HashSet<string>();
    private static readonly HashSet<string> s_lateJoinPending = new HashSet<string>();
    private static readonly Dictionary<string, double> s_pendingRenderRequests = new Dictionary<string, double>();
    private static readonly HashSet<string> s_presentedMembers = new HashSet<string>();
    /// <summary>当前 flush 批次成员；present 屏障按此计数，而非 live syncReady 人数。</summary>
    private static readonly HashSet<string> s_activeFlushMembers = new HashSet<string>();
    /// <summary>Late join 成员：首帧 present 后再 SetFguiVisible，避免 progress=0 空纹理闪屏。</summary>
    private static readonly HashSet<string> s_deferVisibleUntilPresent = new HashSet<string>();

    private static bool s_active;
    private static bool s_staticGroupMode;
    private static bool s_playbackStarted;
    private static int s_expectedPresentCount;
    private static float s_frameInterval = 1f / 30f;
    private static float s_lastGroupFrameTime;
    private static Coroutine s_advanceCoroutine;
    private static Coroutine s_autoStartCoroutine;
    private static Coroutine s_watchdogCoroutine;
    private static float s_batchStallSinceTime;

    private const float StallTimeoutMinSeconds = 0.25f;

    /// <summary>FGUI GPU 多实例同屏时自动 TryJoin；PagGroupPlayer 静态组播不受影响。</summary>
    public static bool AutoConcurrentEnabled { get; set; } = true;

    public static bool IsActive => s_active;

    public static bool IsStaticGroupMode => s_staticGroupMode;

    public static int MemberCount => s_members.Count;

    /// <summary>已有实例 syncReady 播放时，GL 勿插入 FinishFrame，避免 FGUI 整屏闪空白。</summary>
    public static bool ShouldDeferUnityFinishFrame =>
        s_active && s_playbackStarted && s_syncReadyMembers.Count > 0;

    public static bool Contains(string instanceKey)
    {
        return s_active && !string.IsNullOrEmpty(instanceKey) && s_members.Contains(instanceKey);
    }

    /// <summary>仅结束 PagGroupPlayer 静态组播，不影响 PAG1~3 动态合组。</summary>
    public static void EndStaticGroupIfActive(string reason = "static")
    {
        if (s_active && s_staticGroupMode)
        {
            Debug.Log($"[PAG Sync] EndStaticGroupIfActive reason={reason} members={s_members.Count}");
            EndGroupInternal();
        }
    }

    /// <summary>PagGroupPlayer 静态组播：固定成员列表，等全部 bound 后统一起播。</summary>
    public static void BeginGroup(IReadOnlyList<string> instanceKeys, int fps)
    {
        EndGroup();

        if (instanceKeys == null || instanceKeys.Count == 0)
        {
            return;
        }

        s_active = true;
        s_staticGroupMode = true;
        s_playbackStarted = false;
        s_frameInterval = ResolveFrameInterval(fps);
        s_lastGroupFrameTime = 0f;

        for (int i = 0; i < instanceKeys.Count; i++)
        {
            string key = instanceKeys[i];
            if (!string.IsNullOrEmpty(key))
            {
                s_members.Add(key);
            }
        }

        SetGroupExternalPump(true);

#if DEVELOPMENT_BUILD
        Debug.Log($"[PAG Sync] BeginGroup static members={s_members.Count} fps={fps}");
#endif
    }

    /// <summary>动态加入同屏组；首实例创建组，后续实例在播放中合组。</summary>
    public static void TryJoin(string instanceKey, int fps)
    {
        if (string.IsNullOrEmpty(instanceKey))
        {
            return;
        }

        if (s_members.Contains(instanceKey))
        {
            SetExternalPumpFor(instanceKey, true);
            return;
        }

        if (!s_active)
        {
            s_active = true;
            s_staticGroupMode = false;
            s_playbackStarted = false;
            s_frameInterval = ResolveFrameInterval(fps);
            s_lastGroupFrameTime = 0f;
            s_members.Add(instanceKey);
            SetExternalPumpFor(instanceKey, true);
            Debug.Log($"[PAG Sync] TryJoin new group {instanceKey} fps={fps}");
            return;
        }

        if (s_staticGroupMode)
        {
            Debug.LogWarning($"[PAG Sync] TryJoin ignored during static group: {instanceKey}");
            return;
        }

        s_members.Add(instanceKey);
        SetExternalPumpFor(instanceKey, true);

        if (s_playbackStarted)
        {
            s_lateJoinPending.Add(instanceKey);
            CancelAdvanceCoroutine();
            RequestNextFrameForSyncReadyMembers();
            Debug.Log($"[PAG Sync] TryJoin late {instanceKey} members={s_members.Count} syncReady={s_syncReadyMembers.Count}");
        }
        else
        {
            Debug.Log($"[PAG Sync] TryJoin pending start {instanceKey} members={s_members.Count}");
        }
    }

    /// <summary>停止单实例时退出组；最后一名成员离开则 EndGroup。</summary>
    public static void TryLeave(string instanceKey)
    {
        if (!s_active || string.IsNullOrEmpty(instanceKey) || !s_members.Contains(instanceKey))
        {
            return;
        }

        SetExternalPumpFor(instanceKey, false);
        s_members.Remove(instanceKey);
        s_boundMembers.Remove(instanceKey);
        s_syncReadyMembers.Remove(instanceKey);
        s_pendingRenderRequests.Remove(instanceKey);
        s_presentedMembers.Remove(instanceKey);
        s_activeFlushMembers.Remove(instanceKey);
        s_lateJoinPending.Remove(instanceKey);
        s_deferVisibleUntilPresent.Remove(instanceKey);

        if (s_members.Count == 0)
        {
            EndGroup();
            return;
        }

        Debug.Log($"[PAG Sync] TryLeave {instanceKey} remaining={s_members.Count} syncReady={s_syncReadyMembers.Count}");
        KickGroupFrameIfIdle();
    }

    public static void EndGroup()
    {
        if (s_active)
        {
            Debug.Log($"[PAG Sync] EndGroup members={s_members.Count} static={s_staticGroupMode}");
        }

        EndGroupInternal();
    }

    private static void EndGroupInternal()
    {
        CancelAdvanceCoroutine();
        StopWatchdog();

        if (s_autoStartCoroutine != null)
        {
            PagCallbackHub.Instance.StopRunCoroutine(s_autoStartCoroutine);
            s_autoStartCoroutine = null;
        }

        SetGroupExternalPump(false);

        s_members.Clear();
        s_boundMembers.Clear();
        s_syncReadyMembers.Clear();
        s_lateJoinPending.Clear();
        s_pendingRenderRequests.Clear();
        s_presentedMembers.Clear();
        s_activeFlushMembers.Clear();
        s_deferVisibleUntilPresent.Clear();
        s_expectedPresentCount = 0;
        s_active = false;
        s_staticGroupMode = false;
        s_playbackStarted = false;
        s_lastGroupFrameTime = 0f;
    }

    public static void OnGpuBound(string instanceKey)
    {
        if (!Contains(instanceKey))
        {
            return;
        }

        bool wasAlreadyBound = s_boundMembers.Contains(instanceKey);
        s_boundMembers.Add(instanceKey);
        Debug.Log($"[PAG Sync] OnGpuBound {instanceKey} ({s_boundMembers.Count}/{s_members.Count})");

        if (s_playbackStarted && !wasAlreadyBound && s_lateJoinPending.Contains(instanceKey))
        {
            s_lateJoinPending.Remove(instanceKey);
            PagCallbackHub.Instance.RunCoroutine(IntegrateLateMember(instanceKey));
            return;
        }

        if (s_playbackStarted || s_staticGroupMode)
        {
            return;
        }

        if (s_boundMembers.Count < s_members.Count)
        {
            return;
        }

        if (s_autoStartCoroutine != null)
        {
            PagCallbackHub.Instance.StopRunCoroutine(s_autoStartCoroutine);
        }

        s_autoStartCoroutine = PagCallbackHub.Instance.RunCoroutine(TryStartAllPlayback());
    }

    public static IEnumerator WaitUntilAllGpuBoundOrTimeout(float timeoutSec, string logPrefix = "[PAG Sync]")
    {
        if (!s_active || s_members.Count == 0)
        {
            yield break;
        }

        float deadline = Time.unscaledTime + timeoutSec;
        while (s_boundMembers.Count < s_members.Count && Time.unscaledTime < deadline)
        {
            yield return null;
        }

        if (s_boundMembers.Count < s_members.Count)
        {
            Debug.LogError($"{logPrefix} WaitAllGpuBound timeout: bound={s_boundMembers.Count}/{s_members.Count}");
            EndGroup();
        }
    }

    public static IEnumerator TryStartAllPlayback()
    {
        if (!s_active || s_members.Count == 0 || s_boundMembers.Count < s_members.Count)
        {
            s_autoStartCoroutine = null;
            yield break;
        }

        s_playbackStarted = true;
        s_syncReadyMembers.Clear();

        var setupItems = new List<(int slotId, string instanceKey)>(s_members.Count);
        foreach (string key in s_members)
        {
            PagController controller = PagControllerRegistry.Resolve(key);
            if (controller == null)
            {
                Debug.LogError($"[PAG Sync] TryStartAllPlayback: controller missing {key}");
                EndGroup();
                s_autoStartCoroutine = null;
                yield break;
            }

            if (!controller.StartFguiGpuPlaybackFromSyncGroup())
            {
                Debug.LogError($"[PAG Sync] StartFguiGpuPlaybackSync failed: {key}");
                EndGroup();
                s_autoStartCoroutine = null;
                yield break;
            }

            setupItems.Add((controller.TextureSlotId, key));
        }

        yield return new WaitForEndOfFrame();
        Debug.Log($"[PAG Sync] setupBatch begin count={setupItems.Count}");
        yield return PagUnityGlBridge.SetupBatchCoroutine(setupItems);

        var warmupFlushItems = new List<(int slotId, string instanceKey, double progress)>(setupItems.Count);
        for (int i = 0; i < setupItems.Count; i++)
        {
            (int slotId, string instanceKey) item = setupItems[i];
            warmupFlushItems.Add((item.slotId, item.instanceKey, 0.0));
        }

        for (int warmup = 0; warmup < PagController.GpuWarmupFlushCount; warmup++)
        {
            yield return PagUnityGlBridge.FlushBatchCoroutine(warmupFlushItems);
        }

        foreach (string key in s_members)
        {
            PagController controller = PagControllerRegistry.Resolve(key);
            if (controller == null)
            {
                continue;
            }

            controller.ArmFguiGpuPlaybackClock();
            controller.SetFguiVisible(true);
            controller.MarkGpuDisplayReady();
            s_syncReadyMembers.Add(key);
        }

        s_lastGroupFrameTime = 0f;
        ClearInFlightBatchState();
        RequestNextFrameForSyncReadyMembers();

        Debug.Log($"[PAG Sync] TryStartAllPlayback done members={s_members.Count} syncReady={s_syncReadyMembers.Count}");
        StartWatchdogIfNeeded();
        s_autoStartCoroutine = null;
    }

    private static IEnumerator IntegrateLateMember(string instanceKey)
    {
        PagController controller = PagControllerRegistry.Resolve(instanceKey);
        if (controller == null || !Contains(instanceKey))
        {
            yield break;
        }

        if (!controller.StartFguiGpuPlaybackFromSyncGroup())
        {
            Debug.LogError($"[PAG Sync] IntegrateLateMember StartFguiGpuPlaybackSync failed: {instanceKey}");
            TryLeave(instanceKey);
            yield break;
        }

        yield return new WaitForEndOfFrame();
        var setupItems = new List<(int slotId, string instanceKey)>
        {
            (controller.TextureSlotId, instanceKey)
        };
        yield return PagUnityGlBridge.SetupBatchCoroutine(setupItems);

        var warmupFlushItems = new List<(int slotId, string instanceKey, double progress)>
        {
            (controller.TextureSlotId, instanceKey, 0.0)
        };
        for (int warmup = 0; warmup < PagController.GpuWarmupFlushCount; warmup++)
        {
            yield return PagUnityGlBridge.FlushBatchCoroutine(warmupFlushItems);
        }

        controller.ArmFguiGpuPlaybackClock();
        controller.MarkGpuDisplayReady();
        s_deferVisibleUntilPresent.Add(instanceKey);

        yield return WaitUntilFlushPresentIdle();
        s_syncReadyMembers.Add(instanceKey);

        Debug.Log($"[PAG Sync] IntegrateLateMember done {instanceKey} members={s_members.Count} syncReady={s_syncReadyMembers.Count} deferVisible=true");
        KickGroupFrameIfIdle();
    }

    private static IEnumerator WaitUntilFlushPresentIdle()
    {
        while (s_expectedPresentCount > 0 && s_presentedMembers.Count < s_expectedPresentCount)
        {
            yield return null;
        }

        while (s_pendingRenderRequests.Count > 0)
        {
            yield return null;
        }
    }

    private static void ClearInFlightBatchState()
    {
        s_pendingRenderRequests.Clear();
        s_presentedMembers.Clear();
        s_activeFlushMembers.Clear();
        s_expectedPresentCount = 0;
    }

    private static void CancelAdvanceCoroutine()
    {
        if (s_advanceCoroutine != null)
        {
            PagCallbackHub.Instance.StopRunCoroutine(s_advanceCoroutine);
            s_advanceCoroutine = null;
        }
    }

    private static void KickGroupFrameIfIdle()
    {
        if (!s_active || !s_playbackStarted || s_syncReadyMembers.Count == 0)
        {
            return;
        }

        if (s_pendingRenderRequests.Count > 0 || s_presentedMembers.Count > 0 || s_expectedPresentCount > 0)
        {
            return;
        }

        if (s_advanceCoroutine != null)
        {
            return;
        }

        RequestNextFrameForSyncReadyMembers();
    }

    private static void RequestNextFrameForSyncReadyMembers()
    {
        foreach (string key in s_syncReadyMembers)
        {
            PagController controller = PagControllerRegistry.Resolve(key);
            controller?.RequestNextGpuFrameFromSyncGroup();
        }
    }

    private static bool TryBuildFlushBatch(out List<(int slotId, string instanceKey, double progress)> flushItems)
    {
        flushItems = null;
        if (s_syncReadyMembers.Count == 0)
        {
            return false;
        }

        foreach (string key in s_syncReadyMembers)
        {
            if (!s_pendingRenderRequests.ContainsKey(key))
            {
                return false;
            }
        }

        flushItems = new List<(int slotId, string instanceKey, double progress)>(s_syncReadyMembers.Count);
        foreach (string key in s_syncReadyMembers)
        {
            if (!s_pendingRenderRequests.TryGetValue(key, out double prog))
            {
                flushItems = null;
                return false;
            }

            PagController controller = PagControllerRegistry.Resolve(key);
            if (controller == null)
            {
                flushItems = null;
                return false;
            }

            flushItems.Add((controller.TextureSlotId, key, prog));
        }

        return true;
    }

    private static void BeginFlushBatchTracking(IReadOnlyList<(int slotId, string instanceKey, double progress)> flushItems)
    {
        s_activeFlushMembers.Clear();
        s_presentedMembers.Clear();
        s_expectedPresentCount = flushItems.Count;
        for (int i = 0; i < flushItems.Count; i++)
        {
            s_activeFlushMembers.Add(flushItems[i].instanceKey);
        }
    }

    private static void SetGroupExternalPump(bool externalPump)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        foreach (string key in s_members)
        {
            SetExternalPumpFor(key, externalPump);
        }
#endif
    }

    private static void SetExternalPumpFor(string instanceKey, bool externalPump)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        PagController controller = PagControllerRegistry.Resolve(instanceKey);
        controller?.SetFguiGpuExternalPump(externalPump);
#endif
    }

    public static void OnGpuRenderRequested(string instanceKey, double progress)
    {
        if (!s_active || !s_playbackStarted || !Contains(instanceKey))
        {
            return;
        }

        if (!s_syncReadyMembers.Contains(instanceKey))
        {
            return;
        }

        s_pendingRenderRequests[instanceKey] = progress;

        if (!TryBuildFlushBatch(out List<(int slotId, string instanceKey, double progress)> flushItems))
        {
            return;
        }

        s_pendingRenderRequests.Clear();
        BeginFlushBatchTracking(flushItems);
        PagUnityGlBridge.IssueFlushPagGpuBatch(flushItems);
        Debug.Log($"[PAG Sync] flushBatch count={flushItems.Count} members=[{string.Join(",", s_activeFlushMembers)}] syncReady={s_syncReadyMembers.Count}");
    }

    public static void OnGpuFramePresented(string instanceKey)
    {
        if (!s_active || !s_playbackStarted || !Contains(instanceKey))
        {
            return;
        }

        if (!s_syncReadyMembers.Contains(instanceKey))
        {
            return;
        }

        if (s_expectedPresentCount <= 0 || !s_activeFlushMembers.Contains(instanceKey))
        {
            return;
        }

        PagController controller = PagControllerRegistry.Resolve(instanceKey);
        controller?.OnGpuFramePresentedForFgui();
        TryRevealDeferredVisible(instanceKey);

        s_presentedMembers.Add(instanceKey);
        if (s_presentedMembers.Count < s_expectedPresentCount)
        {
            return;
        }

        s_presentedMembers.Clear();
        s_activeFlushMembers.Clear();
        s_expectedPresentCount = 0;

        Debug.Log($"[PAG Sync] present tick syncReady={s_syncReadyMembers.Count}");

        CancelAdvanceCoroutine();
        s_advanceCoroutine = PagCallbackHub.Instance.RunCoroutine(AdvanceGroupFrame());
    }

    private static IEnumerator AdvanceGroupFrame()
    {
        if (s_lastGroupFrameTime > 0f)
        {
            float elapsed = Time.unscaledTime - s_lastGroupFrameTime;
            if (elapsed < s_frameInterval)
            {
                yield return new WaitForSecondsRealtime(s_frameInterval - elapsed);
            }
        }

        s_lastGroupFrameTime = Time.unscaledTime;

        RequestNextFrameForSyncReadyMembers();

        s_advanceCoroutine = null;
    }

    private static void TryRevealDeferredVisible(string instanceKey)
    {
        if (!s_deferVisibleUntilPresent.Remove(instanceKey))
        {
            return;
        }

        PagController controller = PagControllerRegistry.Resolve(instanceKey);
        controller?.SetFguiVisible(true);
        Debug.Log($"[PAG Sync] reveal deferred visible {instanceKey}");
    }

    private static float ResolveFrameInterval(int fps)
    {
        return fps > 0 ? 1f / fps : 1f / 30f;
    }

    private static float StallTimeoutSeconds => Mathf.Max(StallTimeoutMinSeconds, s_frameInterval * 3f);

    private static void StartWatchdogIfNeeded()
    {
        if (s_watchdogCoroutine != null)
        {
            return;
        }

        s_watchdogCoroutine = PagCallbackHub.Instance.RunCoroutine(SyncGroupWatchdogCoroutine());
    }

    private static void StopWatchdog()
    {
        if (s_watchdogCoroutine != null)
        {
            PagCallbackHub.Instance.StopRunCoroutine(s_watchdogCoroutine);
            s_watchdogCoroutine = null;
        }

        s_batchStallSinceTime = 0f;
    }

    private static bool IsBatchStalled()
    {
        if (!s_active || !s_playbackStarted || s_syncReadyMembers.Count == 0)
        {
            return false;
        }

        if (s_pendingRenderRequests.Count > 0
            && s_pendingRenderRequests.Count < s_syncReadyMembers.Count
            && s_expectedPresentCount <= 0)
        {
            return true;
        }

        if (s_expectedPresentCount > 0 && s_presentedMembers.Count < s_expectedPresentCount)
        {
            return true;
        }

        return false;
    }

    private static IEnumerator SyncGroupWatchdogCoroutine()
    {
        while (s_active && s_playbackStarted)
        {
            if (IsBatchStalled())
            {
                if (s_batchStallSinceTime <= 0f)
                {
                    s_batchStallSinceTime = Time.unscaledTime;
                }
                else if (Time.unscaledTime - s_batchStallSinceTime >= StallTimeoutSeconds)
                {
                    RecoverFromStall();
                }
            }
            else
            {
                s_batchStallSinceTime = 0f;
            }

            yield return null;
        }

        s_watchdogCoroutine = null;
    }

    private static void RecoverFromStall()
    {
        Debug.LogWarning($"[PAG Sync] RecoverFromStall pending={s_pendingRenderRequests.Count} "
            + $"present={s_presentedMembers.Count}/{s_expectedPresentCount} syncReady={s_syncReadyMembers.Count}");
        s_batchStallSinceTime = 0f;
        ClearInFlightBatchState();
        CancelAdvanceCoroutine();
        RequestNextFrameForSyncReadyMembers();
    }
}
