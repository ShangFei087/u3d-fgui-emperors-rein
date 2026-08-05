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

    /// <summary>
    /// 创建绑定实例并初始化内部 <see cref="PagController"/>。
    /// </summary>
    /// <param name="instanceKey">PAG 实例唯一键，用于回调路由与日志。</param>
    /// <param name="gamePagFolder">游戏侧 PAG 资源目录；为空则走控制器默认路径。</param>
    public PagSlotBinding(string instanceKey, string gamePagFolder = null)
    {
        Controller = new PagController(instanceKey, gamePagFolder);
    }

    /// <summary>
    /// Attach 到 FGUI 锚点并按全局默认参数 Prepare 一次；锚点/loader/缩放变化时才重复 Prepare。
    /// </summary>
    /// <param name="fguiAnchor">FGUI 锚点组件，内部查找 pagEffect loader。</param>
    /// <param name="loaderName">锚点下 GLoader 名称，默认 <see cref="PagFguiGpuPresenter.DefaultLoaderName"/>。</param>
    /// <param name="displayScale">FGUI 显示缩放，默认 <see cref="PagPresentationDefaults.DisplayScale"/>。</param>
    /// <returns>Prepare 成功返回 true；锚点为空或 Prepare 失败返回 false。</returns>
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
    /// <param name="displayScale">目标显示缩放；与上次不一致时会重新 Prepare。</param>
    /// <returns>已就绪或 Prepare 成功返回 true。</returns>
    private bool EnsurePrepared(float displayScale = PagPresentationDefaults.DisplayScale)
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

    /// <summary>
    /// 绑定到 FGUI 锚点，并失效已缓存的 Prepare 状态。
    /// </summary>
    /// <param name="fguiAnchor">FGUI 锚点组件。</param>
    /// <param name="loaderName">锚点下 GLoader 名称。</param>
    private void Attach(GComponent fguiAnchor, string loaderName = PagFguiGpuPresenter.DefaultLoaderName)
    {
        FguiAnchor = fguiAnchor;
        _boundLoaderName = loaderName;
        InvalidatePrepared();
        Controller.Attach(fguiAnchor, loaderName);
    }

    /// <summary>
    /// 绑定到 FGUI 锚点，使用默认 loader 名。
    /// </summary>
    /// <param name="fguiAnchor">FGUI 锚点组件。</param>
    private void Attach(GComponent fguiAnchor)
    {
        Attach(fguiAnchor, PagFguiGpuPresenter.DefaultLoaderName);
    }

    /// <summary>单文件同步播放（调用前须 EnsureSlot）。</summary>
    /// <param name="pagFile">PAG 文件名（相对游戏 PAG 目录）。</param>
    /// <param name="repeatCount">重复次数；默认 1。</param>
    /// <returns>启动播放成功返回 true。</returns>
    public bool Play(string pagFile, int repeatCount = 1)
    {
        return Play(pagFile, "center", string.Empty, repeatCount);
    }

    /// <summary>单文件同步播放，带布局与可选回调。</summary>
    /// <param name="pagFile">PAG 文件名。</param>
    /// <param name="repeatCount">重复次数。</param>
    /// <param name="layout">播放布局（位置类型 / layoutExtra / turntable 回退）。</param>
    /// <param name="displayScale">显示缩放；变化时会先 EnsurePrepared。</param>
    /// <param name="callbacks">Started/Finished/Failed 回调与超时配置。</param>
    /// <returns>启动播放成功返回 true；失败会触发 OnFailed。</returns>
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
    /// <param name="spec">序列播放规格（分段、布局、缩放、GPU 合组、回调）。</param>
    /// <returns>启动序列成功返回 true。</returns>
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
    /// <param name="segments">播放分段列表；为空则跳过并触发失败回调。</param>
    /// <param name="layout">播放布局；PositionType 为空时回退 Center。</param>
    /// <param name="displayScale">显示缩放。</param>
    /// <param name="useGpuSyncGroup">true：多路 GPU 动态合组防闪；false：独立出帧。</param>
    /// <param name="callbacks">播放生命周期回调。</param>
    /// <returns>启动序列成功返回 true。</returns>
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

    /// <summary>
    /// 配置 FGUI 纹理渲染目标与帧参数。
    /// </summary>
    /// <param name="maxDisplaySide">FGUI 显示长边上限（像素）。</param>
    /// <param name="fps">目标出帧帧率。</param>
    private void ConfigureFgui(int maxDisplaySide, int fps)
    {
        Controller.SetRenderTarget(PagController.PagRenderTarget.FguiTexture);
        Controller.ConfigureFguiFrame(maxDisplaySide, fps);
    }

    /// <summary>
    /// 设置 FGUI 显示缩放。
    /// </summary>
    /// <param name="scale">显示缩放系数。</param>
    private void SetFguiDisplayScale(float scale)
    {
        Controller.SetFguiDisplayScale(scale);
    }

    /// <summary>
    /// 设置是否将显示尺寸钳制到 holder。
    /// </summary>
    /// <param name="clamp">true 时钳制到 holder 尺寸。</param>
    private void SetFguiClampDisplayToHolder(bool clamp)
    {
        Controller.SetFguiClampDisplayToHolder(clamp);
    }

    /// <summary>
    /// 应用全局默认展示参数（缩放 + clamp）。
    /// </summary>
    /// <param name="displayScale">显示缩放，默认全局常量。</param>
    private void ApplyPresentationDefaults(float displayScale = PagPresentationDefaults.DisplayScale)
    {
        SetFguiDisplayScale(displayScale);
        SetFguiClampDisplayToHolder(PagPresentationDefaults.ClampDisplayToHolder);
    }

    /// <summary>
    /// 按全局默认策略 Prepare（FGUI 纹理或 Overlay 回退）。
    /// </summary>
    /// <param name="displayScale">FGUI 模式下的显示缩放。</param>
    /// <returns>Prepare 成功或 Overlay 路径锚点有效时返回 true。</returns>
    private bool PreparePlayWithDefaults(float displayScale = PagPresentationDefaults.DisplayScale)
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

    /// <summary>
    /// 按全局默认策略停止播放并清理回调。
    /// </summary>
    public void StopWithDefaults()
    {
        ClearPlayCallbacks();
        Stop(PagPresentationDefaults.UseFguiTexture);
    }

    /// <summary>
    /// 按指定渲染模式 Prepare 播放环境。
    /// </summary>
    /// <param name="useFguiTexture">true：FGUI 纹理；false：Overlay。</param>
    /// <param name="maxDisplaySide">FGUI 显示长边上限。</param>
    /// <param name="fps">FGUI 出帧帧率。</param>
    /// <returns>锚点有效且（FGUI 模式下）loader 绑定成功时返回 true。</returns>
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

    /// <summary>
    /// 停止当前播放并清理回调。
    /// </summary>
    /// <param name="hideFgui">true 时同时隐藏 FGUI loader。</param>
    public void Stop(bool hideFgui = true)
    {
        ClearPlayCallbacks();
        Controller.StopPag();
        if (hideFgui)
        {
            Controller.SetFguiVisible(false);
        }
    }

    /// <summary>
    /// 播放周期之间的轻量准备（转发至 Controller）。
    /// </summary>
    private void PrepareBetweenPlaybackCycles()
    {
        Controller.PrepareBetweenPlaybackCycles();
    }

    /// <summary>内部单文件播放入口。</summary>
    /// <param name="pagFile">PAG 文件名。</param>
    /// <param name="positionType">布局位置类型（如 center / full）。</param>
    /// <param name="layoutExtra">额外布局参数；null 视为空串。</param>
    /// <param name="repeatCount">重复次数。</param>
    /// <returns>启动播放成功返回 true。</returns>
    private bool Play(string pagFile, string positionType, string layoutExtra, int repeatCount = 1)
    {
        Controller.SetRepeatCount(repeatCount);
        return Controller.PlayPag(pagFile, positionType, layoutExtra ?? string.Empty);
    }

    /// <summary>
    /// 释放控制器与回调资源。
    /// </summary>
    public void Dispose()
    {
        ClearPlayCallbacks();
        Controller.Dispose();
        FguiAnchor = null;
        InvalidatePrepared();
    }

    /// <summary>解析布局；PositionType 为空时回退 Center。</summary>
    /// <param name="layout">原始布局。</param>
    /// <returns>有效布局。</returns>
    private static PagPlayLayout ResolveLayout(in PagPlayLayout layout)
    {
        return string.IsNullOrEmpty(layout.PositionType) ? PagPlayLayout.Center : layout;
    }

    /// <summary>应用布局；必要时走 turntable 自动布局。</summary>
    /// <param name="layout">目标布局。</param>
    private void ApplyLayout(in PagPlayLayout layout)
    {
        PagPlayLayout resolved = ResolveLayout(layout);
        if (resolved.UseTurntableFallback
            && string.IsNullOrEmpty(resolved.LayoutExtra))
        {
            Controller.LayoutPagAuto("turntable");
        }
    }

    /// <summary>注册一次播放的 Started/Finished 回调与超时定时器。</summary>
    /// <param name="callbacks">回调与超时配置。</param>
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

    /// <summary>Native 已开始播放信号回调。</summary>
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

    /// <summary>Native 播放结束信号回调。</summary>
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

    /// <summary>Started 超时：未收到开始信号则触发失败并清理。</summary>
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

    /// <summary>Finished 超时：未收到结束信号则触发失败并清理。</summary>
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

    /// <summary>清理当前播放回调、事件订阅与超时定时器。</summary>
    private void ClearPlayCallbacks()
    {
        RemoveStartedTimeoutTimer();
        RemoveFinishedTimeoutTimer();
        UnsubscribePlayEvents();
        _activeCallbacks = default;
        _startedFired = false;
        _finishedFired = false;
    }

    /// <summary>取消 Started/Finished 事件订阅。</summary>
    private void UnsubscribePlayEvents()
    {
        Controller.OnPlayStarted -= HandlePlayStarted;
        Controller.OnPlaybackFinished -= HandlePlaybackFinished;
    }

    /// <summary>移除 Started 超时定时器。</summary>
    private void RemoveStartedTimeoutTimer()
    {
        if (_startedTimeoutTimer != null)
        {
            Timers.inst.Remove(_startedTimeoutTimer);
            _startedTimeoutTimer = null;
        }
    }

    /// <summary>移除 Finished 超时定时器。</summary>
    private void RemoveFinishedTimeoutTimer()
    {
        if (_finishedTimeoutTimer != null)
        {
            Timers.inst.Remove(_finishedTimeoutTimer);
            _finishedTimeoutTimer = null;
        }
    }

    /// <summary>触发失败回调（若已配置）。</summary>
    /// <param name="callbacks">当前播放回调集。</param>
    private static void InvokeFailed(in PagPlayCallbacks callbacks)
    {
        callbacks.OnFailed?.Invoke();
    }

    /// <summary>使已缓存的 Prepare 状态失效。</summary>
    private void InvalidatePrepared()
    {
        _slotPrepared = false;
        _preparedDisplayScale = float.NaN;
    }
}
