using FairyGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 多 Pag 实例同步开播：统一 Stop → 等一帧 → 同帧 Play。
/// </summary>
public static class PagGroupPlayer
{
    public delegate bool LayoutExtraBuilder(GComponent anchor, out string extra, out string debugReason);

    public static Coroutine PlayOnSlots(
        string pagFile,
        IReadOnlyList<PagSlotBinding> slots,
        LayoutExtraBuilder layoutBuilder,
        bool useFguiTexture,
        int maxDisplaySide,
        int fps,
        string logPrefix = "[PAG Group]",
        int repeatCount = 1,
        Action<string, string> onPlayFailed = null)
    {
        return PlayOnSlots(
            new[] { pagFile },
            slots,
            layoutBuilder,
            useFguiTexture,
            maxDisplaySide,
            fps,
            logPrefix,
            repeatCount,
            onPlayFailed);
    }

    public static Coroutine PlayOnSlots(
        IReadOnlyList<string> pagFilesPerSlot,
        IReadOnlyList<PagSlotBinding> slots,
        LayoutExtraBuilder layoutBuilder,
        bool useFguiTexture,
        int maxDisplaySide,
        int fps,
        string logPrefix = "[PAG Group]",
        int repeatCount = 1,
        Action<string, string> onPlayFailed = null)
    {
        return PagCallbackHub.Instance.RunCoroutine(PlayCoroutine(
            pagFilesPerSlot, slots, layoutBuilder, useFguiTexture, maxDisplaySide, fps, repeatCount, logPrefix, onPlayFailed));
    }

    private static IEnumerator PlayCoroutine(
        IReadOnlyList<string> pagFilesPerSlot,
        IReadOnlyList<PagSlotBinding> slots,
        LayoutExtraBuilder layoutBuilder,
        bool useFguiTexture,
        int maxDisplaySide,
        int fps,
        int repeatCount,
        string logPrefix,
        Action<string, string> onPlayFailed)
    {
        if (pagFilesPerSlot == null || pagFilesPerSlot.Count == 0 || slots == null || slots.Count == 0)
        {
            yield break;
        }

        if (pagFilesPerSlot.Count != 1 && pagFilesPerSlot.Count != slots.Count)
        {
            Debug.LogError($"{logPrefix} pagFilesPerSlot count ({pagFilesPerSlot.Count}) must be 1 or match slots ({slots.Count})");
            yield break;
        }

        Debug.Log($"{logPrefix} Play on {slots.Count} slot(s), files={string.Join(", ", pagFilesPerSlot)}");

        PagGpuSyncGroup.EndGroup();

        for (int i = 0; i < slots.Count; i++)
        {
            slots[i]?.Stop();
        }

        yield return null;
        yield return new WaitForEndOfFrame();

        var instanceKeys = new List<string>(slots.Count);
        for (int i = 0; i < slots.Count; i++)
        {
            PagSlotBinding slot = slots[i];
            if (slot != null && !string.IsNullOrEmpty(slot.InstanceKey))
            {
                instanceKeys.Add(slot.InstanceKey);
            }
        }

        if (useFguiTexture && instanceKeys.Count > 1)
        {
            PagGpuSyncGroup.BeginGroup(instanceKeys, fps);
        }

        string positionType = "center";
        for (int i = 0; i < slots.Count; i++)
        {
            PagSlotBinding slot = slots[i];
            if (slot == null)
            {
                continue;
            }

            string pagFile = pagFilesPerSlot.Count == 1 ? pagFilesPerSlot[0] : pagFilesPerSlot[i];

            if (!slot.PreparePlay(useFguiTexture, maxDisplaySide, fps))
            {
                string msg = $"PreparePlay failed on {slot.InstanceKey}";
                Debug.LogError($"{logPrefix} {msg}");
                onPlayFailed?.Invoke(slot.InstanceKey, msg);
                continue;
            }

            string layoutExtra = string.Empty;
            string layoutDebug = "no builder";
            if (layoutBuilder != null && slot.FguiAnchor != null
                && layoutBuilder(slot.FguiAnchor, out layoutExtra, out layoutDebug))
            {
                Debug.Log($"{logPrefix} {slot.InstanceKey} layout: {layoutExtra} ({layoutDebug})");
            }
            else
            {
                Debug.LogWarning($"{logPrefix} {slot.InstanceKey} layout fallback turntable ({layoutDebug})");
                slot.Controller.LayoutPagAuto("turntable");
            }

            if (!slot.Play(pagFile, positionType, layoutExtra, repeatCount))
            {
                string msg = $"Play failed: {pagFile} on {slot.InstanceKey}";
                Debug.LogError($"{logPrefix} {msg}");
                onPlayFailed?.Invoke(slot.InstanceKey, msg);
            }
            else
            {
                Debug.Log($"{logPrefix} Play: {pagFile} on {slot.InstanceKey}");
            }
        }

        if (PagGpuSyncGroup.IsActive)
        {
            yield return PagGpuSyncGroup.WaitUntilAllGpuBoundOrTimeout(10f, logPrefix);
            if (PagGpuSyncGroup.IsActive)
            {
                yield return PagGpuSyncGroup.TryStartAllPlayback();
            }
        }
    }
}
