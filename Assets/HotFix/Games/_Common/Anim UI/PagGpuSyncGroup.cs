using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 多 Pag 实例 GPU 帧同步组：延迟统一起播、批量 flush、统一 RequestNextGpuFrame。
/// 仅由 PagGroupPlayer 启用；单实例不走此路径。
/// </summary>
public static class PagGpuSyncGroup
{
    private static readonly HashSet<string> s_members = new HashSet<string>();
    private static readonly HashSet<string> s_boundMembers = new HashSet<string>();
    private static readonly Dictionary<string, double> s_pendingRenderRequests = new Dictionary<string, double>();
    private static readonly HashSet<string> s_presentedMembers = new HashSet<string>();

    private static bool s_active;
    private static bool s_playbackStarted;
    private static float s_frameInterval = 1f / 30f;
    private static float s_lastGroupFrameTime;
    private static Coroutine s_advanceCoroutine;

    public static bool IsActive => s_active;

    public static bool Contains(string instanceKey)
    {
        return s_active && !string.IsNullOrEmpty(instanceKey) && s_members.Contains(instanceKey);
    }

    public static void BeginGroup(IReadOnlyList<string> instanceKeys, int fps)
    {
        EndGroup();

        if (instanceKeys == null || instanceKeys.Count == 0)
        {
            return;
        }

        s_active = true;
        s_playbackStarted = false;
        s_frameInterval = fps > 0 ? 1f / fps : 1f / 30f;
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
        Debug.Log($"[PAG Sync] BeginGroup members={s_members.Count} fps={fps}");
#endif
    }

    public static void EndGroup()
    {
        if (s_advanceCoroutine != null)
        {
            PagCallbackHub.Instance.StopRunCoroutine(s_advanceCoroutine);
            s_advanceCoroutine = null;
        }

        SetGroupExternalPump(false);

        s_members.Clear();
        s_boundMembers.Clear();
        s_pendingRenderRequests.Clear();
        s_presentedMembers.Clear();
        s_active = false;
        s_playbackStarted = false;
        s_lastGroupFrameTime = 0f;
    }

    public static void OnGpuBound(string instanceKey)
    {
        if (!Contains(instanceKey))
        {
            return;
        }

        s_boundMembers.Add(instanceKey);
        Debug.Log($"[PAG Sync] OnGpuBound {instanceKey} ({s_boundMembers.Count}/{s_members.Count})");
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
            yield break;
        }

        s_playbackStarted = true;

        var setupItems = new List<(int slotId, string instanceKey)>(s_members.Count);
        foreach (string key in s_members)
        {
            PagController controller = PagControllerRegistry.Resolve(key);
            if (controller == null)
            {
                Debug.LogError($"[PAG Sync] TryStartAllPlayback: controller missing {key}");
                EndGroup();
                yield break;
            }

            if (!controller.StartFguiGpuPlaybackFromSyncGroup())
            {
                Debug.LogError($"[PAG Sync] StartFguiGpuPlaybackSync failed: {key}");
                EndGroup();
                yield break;
            }

            setupItems.Add((controller.TextureSlotId, key));
        }

        yield return new WaitForEndOfFrame();
        Debug.Log($"[PAG Sync] setupBatch begin count={setupItems.Count}");
        yield return PagUnityGlBridge.SetupBatchCoroutine(setupItems);
        foreach (string key in s_members)
        {
            PagController controller = PagControllerRegistry.Resolve(key);
            controller?.RequestNextGpuFrameFromSyncGroup();
        }

        Debug.Log($"[PAG Sync] TryStartAllPlayback done members={s_members.Count}");
    }

    private static void SetGroupExternalPump(bool externalPump)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        foreach (string key in s_members)
        {
            PagController controller = PagControllerRegistry.Resolve(key);
            controller?.SetFguiGpuExternalPump(externalPump);
        }
#endif
    }

    public static void OnGpuRenderRequested(string instanceKey, double progress)
    {
        if (!s_active || !s_playbackStarted || !Contains(instanceKey))
        {
            return;
        }

        s_pendingRenderRequests[instanceKey] = progress;

        if (s_pendingRenderRequests.Count < s_members.Count)
        {
            return;
        }

        var flushItems = new List<(int slotId, string instanceKey, double progress)>(s_members.Count);
        foreach (string key in s_members)
        {
            if (!s_pendingRenderRequests.TryGetValue(key, out double prog))
            {
                return;
            }

            PagController controller = PagControllerRegistry.Resolve(key);
            if (controller == null)
            {
                return;
            }

            flushItems.Add((controller.TextureSlotId, key, prog));
        }

        s_pendingRenderRequests.Clear();
        PagUnityGlBridge.IssueFlushPagGpuBatch(flushItems);
        Debug.Log($"[PAG Sync] flushBatch count={flushItems.Count}");
    }

    public static void OnGpuFramePresented(string instanceKey)
    {
        if (!s_active || !s_playbackStarted || !Contains(instanceKey))
        {
            return;
        }

        PagController controller = PagControllerRegistry.Resolve(instanceKey);
        controller?.OnGpuFramePresentedForFgui();

        s_presentedMembers.Add(instanceKey);
        if (s_presentedMembers.Count < s_members.Count)
        {
            return;
        }

        s_presentedMembers.Clear();

#if DEVELOPMENT_BUILD
        Debug.Log($"[PAG Sync] present tick members={s_members.Count}");
#endif

        if (s_advanceCoroutine != null)
        {
            PagCallbackHub.Instance.StopRunCoroutine(s_advanceCoroutine);
        }

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

        foreach (string key in s_members)
        {
            PagController controller = PagControllerRegistry.Resolve(key);
            controller?.RequestNextGpuFrameFromSyncGroup();
        }

        s_advanceCoroutine = null;
    }
}
