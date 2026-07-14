using System;
using FairyGUI;
using UnityEngine;

/// <summary>
/// 封装 PagController 与 FGUI 锚点绑定。
/// </summary>
public sealed class PagSlotBinding : IDisposable
{
    public PagController Controller { get; }
    public string InstanceKey => Controller.InstanceKey;
    public GComponent FguiAnchor { get; private set; }

    private string _boundLoaderName = PagFguiGpuPresenter.DefaultLoaderName;
    private float _preparedDisplayScale = float.NaN;
    private bool _slotPrepared;

    private PagPlayCallbacks _activeCallbacks;
    private bool _startedFired;
    private bool _finishedFired;
    private TimerCallback _startedTimeoutTimer;
    private TimerCallback _finishedTimeoutTimer;

    public PagSlotBinding(string instanceKey, string gamePagFolder = null)
    {
        Controller = new PagController(instanceKey, gamePagFolder);
    }

    /// <summary>
    /// Attach 到 FGUI 锚点并按全局默认参数 Prepare 一次；锚点/loader/缩放变化时才重复 Prepare。
    /// </summary>
    public bool EnsureSlot(
        GComponent fguiAnchor,
        string loaderName = PagFguiGpuPresenter.DefaultLoaderName,
        float displayScale = PagPresentationDefaults.DisplayScale)
    {
        if (fguiAnchor == null)
        {
            Debug.LogWarning($"[PAG] EnsureSlot skipped: fguiAnchor is null, instance={InstanceKey}");
            return false;
        }

        bool bindingChanged = FguiAnchor != fguiAnchor || _boundLoaderName != loaderName;
        if (bindingChanged)
        {
            Attach(fguiAnchor, loaderName);
        }

        return EnsurePrepared(displayScale);
    }

    /// <summary>
    /// 已 Attach 后确保 Prepare 就绪；displayScale 变化时会重新 Prepare。
    /// </summary>
    public bool EnsurePrepared(float displayScale = PagPresentationDefaults.DisplayScale)
    {
        if (FguiAnchor == null)
        {
            Debug.LogWarning($"[PAG] EnsurePrepared skipped: FguiAnchor is null, instance={InstanceKey}");
            return false;
        }

        if (_slotPrepared && Mathf.Approximately(_preparedDisplayScale, displayScale))
        {
            return true;
        }

        if (!PreparePlayWithDefaults(displayScale))
        {
            _slotPrepared = false;
            _preparedDisplayScale = float.NaN;
            return false;
        }

        _slotPrepared = true;
        _preparedDisplayScale = displayScale;
        return true;
    }

    public void Attach(GComponent fguiAnchor, string loaderName = PagFguiGpuPresenter.DefaultLoaderName)
    {
        FguiAnchor = fguiAnchor;
        _boundLoaderName = loaderName;
        InvalidatePrepared();
        Controller.Attach(fguiAnchor, loaderName);
    }

    public void Attach(GComponent fguiAnchor)
    {
        Attach(fguiAnchor, PagFguiGpuPresenter.DefaultLoaderName);
    }

    /// <summary>单文件同步播放（调用前须 EnsureSlot / EnsurePrepared）。</summary>
    public bool Play(string pagFile, int repeatCount = 1)
    {
        return Play(pagFile, "center", string.Empty, repeatCount);
    }

    /// <summary>单文件同步播放，带布局与可选回调。</summary>
    public bool Play(string pagFile,int repeatCount,in PagPlayLayout layout,float displayScale = PagPresentationDefaults.DisplayScale,in PagPlayCallbacks callbacks = default)
    {
        if (string.IsNullOrEmpty(pagFile))
        {
            return false;
        }

        PagPlayLayout resolvedLayout = ResolveLayout(layout);
        if (!EnsurePrepared(displayScale))
        {
            InvokeFailed(callbacks);
            return false;
        }

        ApplyLayout(resolvedLayout);
        Controller.SetRepeatCount(repeatCount);
        bool ok = Controller.PlayPag(pagFile, resolvedLayout.PositionType, resolvedLayout.LayoutExtra);
        if (!ok)
        {
            InvokeFailed(callbacks);
            return false;
        }

        RegisterPlayCallbacks(callbacks);
        return true;
    }

    /// <summary>序列播放；C# 只 Play 首段，后续由 Java 链式切换。</summary>
    public bool Play(in PagSequencePlay spec)
    {
        return Play(
            spec.Segments,
            spec.Layout,
            spec.DisplayScale,
            spec.UseGpuSyncGroup,
            spec.Callbacks);
    }

    /// <summary>Native playlist 序列播放。</summary>
    public bool Play(PagSegment[] segments,in PagPlayLayout layout = default,float displayScale = PagPresentationDefaults.DisplayScale,bool useGpuSyncGroup = PagPresentationDefaults.UseGpuSyncGroup,in PagPlayCallbacks callbacks = default)
    {
        if (segments == null || segments.Length == 0)
        {
            Debug.LogWarning($"[PAG] Play sequence skipped: empty segments, instance={InstanceKey}");
            InvokeFailed(callbacks);
            return false;
        }

        PagPlayLayout resolvedLayout = ResolveLayout(layout);
        if (!EnsurePrepared(displayScale))
        {
            InvokeFailed(callbacks);
            return false;
        }

        ApplyLayout(resolvedLayout);
        bool ok = Controller.PlayFguiGpuSequence(
            segments,
            resolvedLayout.PositionType,
            resolvedLayout.LayoutExtra,
            useGpuSyncGroup);
        if (!ok)
        {
            InvokeFailed(callbacks);
            return false;
        }

        RegisterPlayCallbacks(callbacks);
        return true;
    }

    public void ConfigureFgui(int maxDisplaySide, int fps)
    {
        Controller.SetRenderTarget(PagController.PagRenderTarget.FguiTexture);
        Controller.ConfigureFguiFrame(maxDisplaySide, fps);
    }

    public void SetFguiDisplayScale(float scale)
    {
        Controller.SetFguiDisplayScale(scale);
    }

    public void SetFguiClampDisplayToHolder(bool clamp)
    {
        Controller.SetFguiClampDisplayToHolder(clamp);
    }

    public void ApplyPresentationDefaults(float displayScale = PagPresentationDefaults.DisplayScale)
    {
        SetFguiDisplayScale(displayScale);
        SetFguiClampDisplayToHolder(PagPresentationDefaults.ClampDisplayToHolder);
    }

    public bool PreparePlayWithDefaults(float displayScale = PagPresentationDefaults.DisplayScale)
    {
        if (!PagPresentationDefaults.UseFguiTexture)
        {
            Controller.SetRenderTarget(PagController.PagRenderTarget.Overlay);
            Controller.SetForceBitmapOverlayFallback(PagPresentationDefaults.OverlayFallback);
            return FguiAnchor != null;
        }

        ApplyPresentationDefaults(displayScale);
        return PreparePlay(
            PagPresentationDefaults.UseFguiTexture,
            PagPresentationDefaults.FguiMaxDisplaySide,
            PagPresentationDefaults.FguiFps);
    }

    public void StopWithDefaults()
    {
        ClearPlayCallbacks();
        Stop(PagPresentationDefaults.UseFguiTexture);
    }

    public bool PreparePlay(bool useFguiTexture, int maxDisplaySide, int fps)
    {
        if (FguiAnchor == null)
        {
            Debug.LogWarning($"[PAG] PreparePlay skipped: FguiAnchor is null, instance={InstanceKey}");
            return false;
        }

        if (useFguiTexture)
        {
            ConfigureFgui(maxDisplaySide, fps);
            Controller.PrepareFguiLayoutBeforePlay();
            if (Controller.FguiLoader == null)
            {
                Debug.LogError($"[PAG] PreparePlay failed: pagEffect not bound, instance={InstanceKey}, anchor={FguiAnchor.name}");
                return false;
            }
        }
        else
        {
            Controller.SetRenderTarget(PagController.PagRenderTarget.Overlay);
        }

        return true;
    }

    public void Stop(bool hideFgui = true)
    {
        ClearPlayCallbacks();
        Controller.StopPag();
        if (hideFgui)
        {
            Controller.SetFguiVisible(false);
        }
    }

    public void PrepareBetweenPlaybackCycles()
    {
        Controller.PrepareBetweenPlaybackCycles();
    }

    private bool Play(string pagFile, string positionType, string layoutExtra, int repeatCount = 1)
    {
        Controller.SetRepeatCount(repeatCount);
        return Controller.PlayPag(pagFile, positionType, layoutExtra ?? string.Empty);
    }

    public void Dispose()
    {
        ClearPlayCallbacks();
        Controller.Dispose();
        FguiAnchor = null;
        InvalidatePrepared();
    }

    private static PagPlayLayout ResolveLayout(in PagPlayLayout layout)
    {
        return string.IsNullOrEmpty(layout.PositionType) ? PagPlayLayout.Center : layout;
    }

    private void ApplyLayout(in PagPlayLayout layout)
    {
        PagPlayLayout resolved = ResolveLayout(layout);
        if (resolved.UseTurntableFallback
            && string.IsNullOrEmpty(resolved.LayoutExtra))
        {
            Controller.LayoutPagAuto("turntable");
        }
    }

    private void RegisterPlayCallbacks(in PagPlayCallbacks callbacks)
    {
        ClearPlayCallbacks();
        if (!callbacks.HasAnyCallback)
        {
            return;
        }

        _activeCallbacks = callbacks;
        _startedFired = false;
        _finishedFired = false;

        Controller.ResetPlayStartedSignal();
        Controller.ResetPlaybackFinished();

        if (callbacks.OnStarted != null || callbacks.StartedTimeoutSec > 0f)
        {
            Controller.OnPlayStarted += HandlePlayStarted;
        }

        if (callbacks.OnFinished != null || callbacks.FinishedTimeoutSec > 0f || callbacks.StopAfterFinished)
        {
            Controller.OnPlaybackFinished += HandlePlaybackFinished;
        }

        if (callbacks.StartedTimeoutSec > 0f)
        {
            _startedTimeoutTimer = _ => OnStartedTimeout();
            Timers.inst.Add(callbacks.StartedTimeoutSec, 1, _startedTimeoutTimer);
        }

        if (callbacks.FinishedTimeoutSec > 0f
            && (callbacks.OnFinished != null || callbacks.StopAfterFinished))
        {
            _finishedTimeoutTimer = _ => OnFinishedTimeout();
            Timers.inst.Add(callbacks.FinishedTimeoutSec, 1, _finishedTimeoutTimer);
        }
    }

    private void HandlePlayStarted()
    {
        if (_startedFired)
        {
            return;
        }

        _startedFired = true;
        RemoveStartedTimeoutTimer();
        _activeCallbacks.OnStarted?.Invoke();
        if (_activeCallbacks.UnsubscribeOnFire && _activeCallbacks.OnFinished == null && !_activeCallbacks.StopAfterFinished)
        {
            UnsubscribePlayEvents();
        }
    }

    private void HandlePlaybackFinished()
    {
        if (_finishedFired)
        {
            return;
        }

        _finishedFired = true;
        RemoveFinishedTimeoutTimer();
        _activeCallbacks.OnFinished?.Invoke();
        if (_activeCallbacks.StopAfterFinished)
        {
            Stop(PagPresentationDefaults.UseFguiTexture);
        }

        if (_activeCallbacks.UnsubscribeOnFire)
        {
            UnsubscribePlayEvents();
        }
    }

    private void OnStartedTimeout()
    {
        _startedTimeoutTimer = null;
        if (_startedFired || Controller.PlayStarted)
        {
            return;
        }

        Debug.LogWarning($"[PAG] Play started timeout instance={InstanceKey}");
        InvokeFailed(_activeCallbacks);
        ClearPlayCallbacks();
    }

    private void OnFinishedTimeout()
    {
        _finishedTimeoutTimer = null;
        if (_finishedFired || Controller.PlaybackFinished)
        {
            return;
        }

        Debug.LogWarning($"[PAG] Play finished timeout instance={InstanceKey}");
        InvokeFailed(_activeCallbacks);
        ClearPlayCallbacks();
    }

    private void ClearPlayCallbacks()
    {
        RemoveStartedTimeoutTimer();
        RemoveFinishedTimeoutTimer();
        UnsubscribePlayEvents();
        _activeCallbacks = default;
        _startedFired = false;
        _finishedFired = false;
    }

    private void UnsubscribePlayEvents()
    {
        Controller.OnPlayStarted -= HandlePlayStarted;
        Controller.OnPlaybackFinished -= HandlePlaybackFinished;
    }

    private void RemoveStartedTimeoutTimer()
    {
        if (_startedTimeoutTimer != null)
        {
            Timers.inst.Remove(_startedTimeoutTimer);
            _startedTimeoutTimer = null;
        }
    }

    private void RemoveFinishedTimeoutTimer()
    {
        if (_finishedTimeoutTimer != null)
        {
            Timers.inst.Remove(_finishedTimeoutTimer);
            _finishedTimeoutTimer = null;
        }
    }

    private static void InvokeFailed(in PagPlayCallbacks callbacks)
    {
        callbacks.OnFailed?.Invoke();
    }

    private void InvalidatePrepared()
    {
        _slotPrepared = false;
        _preparedDisplayScale = float.NaN;
    }
}
