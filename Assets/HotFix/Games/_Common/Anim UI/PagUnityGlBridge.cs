using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Unity GL 渲染线程创建 RGBA 纹理，供 FGUI CreateExternalTexture 与 libpag FromTexture 共享。
/// 按 slotId 支持多路 PAG；GL 操作经全局队列串行化，native 侧按入队 slot/key 执行。
/// </summary>
public static class PagUnityGlBridge
{
#if UNITY_ANDROID && !UNITY_EDITOR
    private const string LibName = "pag_unity_gl_bridge";

    [DllImport(LibName)]
    private static extern IntPtr PagGl_GetRenderEventFunc();

    [DllImport(LibName)]
    private static extern int PagGl_GetCreateTextureEventId();

    [DllImport(LibName)]
    private static extern int PagGl_GetFinishFrameEventId();

    [DllImport(LibName)]
    private static extern int PagGl_GetSetupPagGpuEventId();

    [DllImport(LibName)]
    private static extern int PagGl_GetFlushPagGpuEventId();

    [DllImport(LibName)]
    private static extern void PagGl_SetActiveSlot(int slotId);

    [DllImport(LibName)]
    private static extern void PagGl_EnqueueCreateTexture(int slotId, int width, int height);

    [DllImport(LibName)]
    private static extern void PagGl_EnqueueSetup(int slotId, IntPtr instanceKeyUtf8);

    [DllImport(LibName)]
    private static extern void PagGl_EnqueueFlush(int slotId, IntPtr instanceKeyUtf8, double progress);

    [DllImport(LibName)]
    private static extern void PagGl_DestroyTexture();

    [DllImport(LibName)]
    private static extern int PagGl_GetTextureId();

    [DllImport(LibName)]
    private static extern IntPtr PagGl_GetTexturePointer();

    [DllImport(LibName)]
    private static extern int PagGl_GetPendingOpCount();

    private static readonly Queue<IEnumerator> s_glQueue = new Queue<IEnumerator>();
    private static readonly Dictionary<string, IntPtr> s_instanceKeyNativeCache = new Dictionary<string, IntPtr>();
    private static bool s_queueRunning;

    public static bool IsSupported => true;

    public static int GetPendingOpCount()
    {
        return s_glQueue.Count + PagGl_GetPendingOpCount();
    }

    private static IntPtr GetOrCreateInstanceKeyNativePtr(string instanceKey)
    {
        string key = string.IsNullOrEmpty(instanceKey) ? "_default" : instanceKey;
        if (!s_instanceKeyNativeCache.TryGetValue(key, out IntPtr ptr))
        {
            ptr = Marshal.StringToHGlobalAnsi(key);
            s_instanceKeyNativeCache[key] = ptr;
        }

        return ptr;
    }

    private static void WithInstanceKeyNative(string instanceKey, Action<IntPtr> action)
    {
        action(GetOrCreateInstanceKeyNativePtr(instanceKey));
    }

    private static void EnqueueGlOperation(IEnumerator operation)
    {
        s_glQueue.Enqueue(operation);
        if (!s_queueRunning)
        {
            s_queueRunning = true;
            PagCallbackHub.Instance.RunCoroutine(ProcessGlQueue());
        }
    }

    private static IEnumerator ProcessGlQueue()
    {
        while (s_glQueue.Count > 0)
        {
            IEnumerator op = s_glQueue.Dequeue();
            if (op != null)
            {
                yield return op;
            }
        }

        s_queueRunning = false;
    }

    private static IEnumerator RunExclusiveGlOperation(IEnumerator operation)
    {
        bool done = false;
        EnqueueGlOperation(WrapGlOperation(operation, () => done = true));
        while (!done)
        {
            yield return null;
        }
    }

    private static IEnumerator WrapGlOperation(IEnumerator operation, Action onComplete)
    {
        if (operation != null)
        {
            yield return operation;
        }

        onComplete?.Invoke();
    }

    private static IEnumerator WaitForRenderThreadIdle()
    {
        yield return new WaitForEndOfFrame();
        PagGl_SetActiveSlot(0);
        GL.IssuePluginEvent(PagGl_GetRenderEventFunc(), PagGl_GetFinishFrameEventId());
        yield return new WaitForEndOfFrame();
    }

    public static IEnumerator EnsureTextureCoroutine(int slotId, int width, int height, Action<int, IntPtr> onReady)
    {
        if (width <= 0 || height <= 0)
        {
            onReady?.Invoke(0, IntPtr.Zero);
            yield break;
        }

        int texId = 0;
        IntPtr texPtr = IntPtr.Zero;
        yield return RunExclusiveGlOperation(InternalEnsureTexture(slotId, width, height, (id, ptr) =>
        {
            texId = id;
            texPtr = ptr;
        }));
        onReady?.Invoke(texId, texPtr);
    }

    private static IEnumerator InternalEnsureTexture(int slotId, int width, int height, Action<int, IntPtr> onReady)
    {
        PagGl_EnqueueCreateTexture(slotId, width, height);
        GL.IssuePluginEvent(PagGl_GetRenderEventFunc(), PagGl_GetCreateTextureEventId());
        yield return WaitForRenderThreadIdle();

        PagGl_SetActiveSlot(slotId);
        int texId = PagGl_GetTextureId();
        IntPtr texPtr = PagGl_GetTexturePointer();
        onReady?.Invoke(texId, texPtr);
    }

    public static void IssueSetupPagGpuEvent(int slotId, string instanceKey)
    {
        IssueSetupPagGpuBatch(new[] { (slotId, instanceKey) });
    }

    public static void IssueSetupPagGpuBatch(IReadOnlyList<(int slotId, string instanceKey)> items)
    {
        if (items == null || items.Count == 0)
        {
            return;
        }

        EnqueueGlOperation(InternalIssueSetupBatch(items));
    }

    public static IEnumerator SetupBatchCoroutine(IReadOnlyList<(int slotId, string instanceKey)> items)
    {
        if (items == null || items.Count == 0)
        {
            yield break;
        }

        yield return RunExclusiveGlOperation(InternalIssueSetupBatch(items));
    }

    private static IEnumerator InternalIssueSetupBatch(IReadOnlyList<(int slotId, string instanceKey)> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            (int slotId, string instanceKey) item = items[i];
            WithInstanceKeyNative(item.instanceKey, ptr => PagGl_EnqueueSetup(item.slotId, ptr));
        }

        GL.IssuePluginEvent(PagGl_GetRenderEventFunc(), PagGl_GetSetupPagGpuEventId());
        yield return WaitForRenderThreadIdle();
    }

    public static void IssueFlushPagGpuEvent(int slotId, string instanceKey, double progress)
    {
        IssueFlushPagGpuBatch(new[] { (slotId, instanceKey, progress) });
    }

    public static void IssueFlushPagGpuBatch(IReadOnlyList<(int slotId, string instanceKey, double progress)> items)
    {
        if (items == null || items.Count == 0)
        {
            return;
        }

        EnqueueGlOperation(InternalIssueFlushBatch(items));
    }

    private static IEnumerator InternalIssueFlushBatch(IReadOnlyList<(int slotId, string instanceKey, double progress)> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            (int slotId, string instanceKey, double progress) item = items[i];
            WithInstanceKeyNative(item.instanceKey, ptr => PagGl_EnqueueFlush(item.slotId, ptr, item.progress));
        }

        GL.IssuePluginEvent(PagGl_GetRenderEventFunc(), PagGl_GetFlushPagGpuEventId());
        yield return WaitForRenderThreadIdle();
    }

    public static void IssueFinishFrameEvent(int slotId)
    {
        EnqueueGlOperation(InternalIssueFinishFrame(slotId));
    }

    private static IEnumerator InternalIssueFinishFrame(int slotId)
    {
        PagGl_SetActiveSlot(slotId);
        GL.IssuePluginEvent(PagGl_GetRenderEventFunc(), PagGl_GetFinishFrameEventId());
        yield return WaitForRenderThreadIdle();
    }

    public static void DestroyTexture(int slotId)
    {
        EnqueueGlOperation(InternalDestroyTexture(slotId));
    }

    private static IEnumerator InternalDestroyTexture(int slotId)
    {
        PagGl_SetActiveSlot(slotId);
        PagGl_DestroyTexture();
        yield return WaitForRenderThreadIdle();
    }
#else
    public static bool IsSupported => false;

    public static IEnumerator EnsureTextureCoroutine(int slotId, int width, int height, Action<int, IntPtr> onReady)
    {
        onReady?.Invoke(0, IntPtr.Zero);
        yield break;
    }

    public static void IssueSetupPagGpuEvent(int slotId, string instanceKey)
    {
    }

    public static void IssueSetupPagGpuBatch(IReadOnlyList<(int slotId, string instanceKey)> items)
    {
    }

    public static IEnumerator SetupBatchCoroutine(IReadOnlyList<(int slotId, string instanceKey)> items)
    {
        yield break;
    }

    public static void IssueFlushPagGpuEvent(int slotId, string instanceKey, double progress)
    {
    }

    public static void IssueFlushPagGpuBatch(IReadOnlyList<(int slotId, string instanceKey, double progress)> items)
    {
    }

    public static void IssueFinishFrameEvent(int slotId)
    {
    }

    public static void DestroyTexture(int slotId)
    {
    }

    public static int GetPendingOpCount() => 0;
#endif
}
