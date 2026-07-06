using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FairyGUI;
using UnityEngine;
using UnityEngine.Profiling;

/// <summary>Native 播放列表单段（path 为相对 Pag 文件名，repeat=-1 无限循环）。</summary>
public readonly struct PagSegment
{
    public string PagFileName { get; }
    public int RepeatCount { get; }

    public PagSegment(string pagFileName, int repeatCount)
    {
        PagFileName = pagFileName;
        RepeatCount = repeatCount;
    }
}

/// <summary>
/// PAG 播放控制器（纯 C# 类）。通过 Attach 绑定 FGUI 锚点（内部查找 pagEffect）；
/// 回调经全局 PagCallbackHub + PagControllerRegistry 路由。
/// </summary>
public class PagController : IDisposable
{
    public enum PagRenderTarget
    {
        Overlay = 0,
        FguiTexture = 1,
    }

    private const string BridgeClass = "com.lftlive.com.pag.PagBridge";
    private const string JniLogTag = "PagBridgeUnity";
    /// <summary>P1：绑纹理后静默 flush 次数，预热 GPU 后再开播放墙钟。</summary>
    internal const int GpuWarmupFlushCount = 2;
    /// <summary>Late join 路径 warmup 次数（低于首组，减轻第 2 路点开卡顿）。</summary>
    internal const int LateJoinGpuWarmupFlushCount = 1;

    private static AndroidJavaClass _pagBridge;
    private static bool _initialized;
    private static int _nextTextureSlotId = 1000;

    /// <summary>每完成多少次播放触发 UnloadUnusedAssets；0=关闭。</summary>
    public static int UnloadAssetsEveryPlayCount = 40;

    /// <summary>repeat=-1 无限循环时每 N 圈软重启 GPU Player；0=关闭（默认，长播依赖 libpag 自然 loop）。</summary>
    public static int GpuPlayerRecycleEveryLoop = 0;

    /// <summary>纹理模式多实例同屏时自动纳入 PagGpuSyncGroup；默认开启。</summary>
    public static bool AutoConcurrentGpuSync
    {
        get => PagGpuSyncGroup.AutoConcurrentEnabled;
        set => PagGpuSyncGroup.AutoConcurrentEnabled = value;
    }

    private const int GlQueueBacklogWarnThreshold = 32;

    public event Action<string> OnExportFinished;
    public event Action OnPlayStarted;
    public event Action OnPlaybackFinished;

    private readonly string _gamePagFolder;
    private readonly int _textureSlotId;
    private readonly PagFguiGpuPresenter _fguiPresenter = new PagFguiGpuPresenter();

    private bool _disposed;
    private bool _attached;
    private bool _playStartedSignal;
    private bool _gpuDisplayReadySignal;
    private bool _playbackFinished;
    private string _lastPlayPagLeaf;
    private PagRenderTarget _renderTarget = PagRenderTarget.Overlay;
    private Coroutine _gpuBindCoroutine;
    private float _fguiDisplayScale = 1f;
    private int _fguiTargetFps = 30;
    private int _gpuFlushPresentCount;
    private int _boundGpuTexId;
    private int _boundGpuTexW;
    private int _boundGpuTexH;
    private IntPtr _boundGpuTexPtr = IntPtr.Zero;
    private int _completedPlayCount;
    private Coroutine _maintenanceCoroutine;
    private Coroutine _destroyGpuCoroutine;
    private bool _skipAutoSyncJoinForPlaylist;

    private GComponent _fguiAnchor;
    private string _fguiLoaderName = PagFguiGpuPresenter.DefaultLoaderName;

    public string InstanceKey { get; private set; }

    public GLoader FguiLoader => _fguiPresenter.Loader;

    internal int TextureSlotId => _textureSlotId;

    public bool PlayStarted => _playStartedSignal;

    public bool GpuDisplayReady => _gpuDisplayReadySignal;

    public bool PlaybackFinished => _playbackFinished;

    public PagController(string instanceKey, string gamePagFolder = null)
    {
        InstanceKey = string.IsNullOrEmpty(instanceKey)
            ? $"Pag_{Guid.NewGuid():N}"
            : instanceKey;
        if (string.IsNullOrEmpty(gamePagFolder))
        {
            Debug.LogError($"[PAG] PagController({InstanceKey}): gamePagFolder is required");
            _gamePagFolder = string.Empty;
        }
        else
        {
            _gamePagFolder = gamePagFolder;
        }
        _textureSlotId = System.Threading.Interlocked.Increment(ref _nextTextureSlotId);
        EnsureInit();
    }

    /// <summary>兼容旧代码；Attach 前可改 key，Attach 后无效。</summary>
    public void SetBridgeInstanceKey(string key)
    {
        if (_attached || string.IsNullOrEmpty(key))
        {
            return;
        }

        InstanceKey = key;
    }

    public void Attach(GComponent fguiAnchor, string loaderName = null)
    {
        if (_disposed)
        {
            Debug.LogWarning("[PAG] Attach skipped: controller disposed");
            return;
        }

        if (fguiAnchor == null)
        {
            Debug.LogWarning($"[PAG] Attach skipped: fguiAnchor is null, instance={InstanceKey}");
            return;
        }

        if (!string.IsNullOrEmpty(loaderName))
        {
            _fguiLoaderName = loaderName;
        }

        if (_attached && _fguiAnchor == fguiAnchor)
        {
            EnsureFguiBinding();
            return;
        }

        DetachHostOnly();
        _fguiAnchor = fguiAnchor;

        PagCallbackHub.EnsureInstance();
        PagControllerRegistry.Register(InstanceKey, this);
        _attached = true;
        EnsureFguiBinding();
        Debug.Log($"[PAG] Attached instance={InstanceKey}, fgui={_fguiAnchor.name}, loader={_fguiLoaderName}");
    }

    public void Attach(GComponent fguiAnchor)
    {
        Attach(fguiAnchor, null);
    }

    private void EnsureFguiBinding()
    {
        if (_fguiAnchor == null)
        {
            return;
        }

        GLoader pagEffect = PagFguiGpuPresenter.TryGetPagEffectLoader(_fguiAnchor, _fguiLoaderName);
        if (pagEffect == null)
        {
            Debug.LogError($"[PAG] FGUI 绑定失败: instance={InstanceKey}, anchor={_fguiAnchor.name}");
            return;
        }

        if (_fguiPresenter.Loader == pagEffect)
        {
            _fguiPresenter.ConfigureAnchor(_fguiAnchor, _fguiLoaderName);
            return;
        }

        BindFguiLoader(pagEffect);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        StopPag();
        DetachHostOnly();
        _fguiAnchor = null;
        _disposed = true;
    }

    private void DetachHostOnly()
    {
        PagControllerRegistry.Unregister(InstanceKey);
        _attached = false;
    }

    public static void EnsureInit()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_initialized)
        {
            return;
        }

        try
        {
            LogJni("EnsureInit: creating PagBridge...");
            _pagBridge = new AndroidJavaClass(BridgeClass);
            AndroidJavaObject activity = GetUnityActivity();
            if (activity == null)
            {
                Debug.LogError("[PAG JNI] EnsureInit failed: currentActivity is null");
                return;
            }

            _pagBridge.CallStatic("Init", activity);
            _initialized = true;
            LogJni("EnsureInit: Init called OK");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PAG JNI] EnsureInit exception: {ex}");
            _initialized = false;
            _pagBridge = null;
        }
#endif
    }

    private static AndroidJavaObject GetUnityActivity()
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        return unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    }

    private const float PreloadCompositionPollIntervalSec = 0.05f;
    private const float PreloadCompositionTimeoutSec = 30f;

    public static void PreloadComposition(string pagName, string gamePagFolder)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        string absPath = ResolvePagPath(pagName, gamePagFolder);
        if (string.IsNullOrEmpty(absPath) || !PagPathHelper.IsValidPagFile(absPath))
        {
            Debug.LogWarning($"[PAG] PreloadComposition skipped: invalid path for {pagName}");
            return;
        }

        EnsureInit();
        if (_pagBridge == null)
        {
            Debug.LogWarning("[PAG] PreloadComposition skipped: PagBridge not initialized");
            return;
        }

        if (IsCompositionCached(absPath))
        {
            Debug.Log($"[PAG] PreloadComposition already cached: {absPath}");
            return;
        }

        _pagBridge.CallStatic("PreloadComposition", absPath);
        Debug.Log($"[PAG] PreloadComposition dispatched: {absPath}");
#else
        Debug.LogWarning($"[PAG] PreloadComposition skipped (non-Android/Editor): {pagName}");
#endif
    }

    public static bool IsCompositionCached(string absPath)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (string.IsNullOrEmpty(absPath) || _pagBridge == null)
        {
            return false;
        }

        try
        {
            return _pagBridge.CallStatic<bool>("IsCompositionCached", absPath);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[PAG] IsCompositionCached: {ex.Message}");
            return false;
        }
#else
        return false;
#endif
    }

    public static IEnumerator PreloadCompositionCoroutine(
        string pagName,
        string gamePagFolder,
        Action<bool> onDone = null)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        string absPath = ResolvePagPath(pagName, gamePagFolder);
        if (string.IsNullOrEmpty(absPath) || !PagPathHelper.IsValidPagFile(absPath))
        {
            Debug.LogWarning($"[PAG] PreloadCompositionCoroutine failed: invalid path for {pagName}");
            onDone?.Invoke(false);
            yield break;
        }

        if (IsCompositionCached(absPath))
        {
            Debug.Log($"[PAG] PreloadCompositionCoroutine already cached: {absPath}");
            onDone?.Invoke(true);
            yield break;
        }

        yield return PagPathHelper.WarmupPagCacheCoroutine(pagName, gamePagFolder);

        absPath = ResolvePagPath(pagName, gamePagFolder);
        if (string.IsNullOrEmpty(absPath) || !PagPathHelper.IsValidPagFile(absPath))
        {
            onDone?.Invoke(false);
            yield break;
        }

        PreloadComposition(pagName, gamePagFolder);

        float deadline = Time.unscaledTime + PreloadCompositionTimeoutSec;
        while (!IsCompositionCached(absPath) && Time.unscaledTime < deadline)
        {
            yield return new WaitForSecondsRealtime(PreloadCompositionPollIntervalSec);
        }

        bool ready = IsCompositionCached(absPath);
        if (!ready)
        {
            Debug.LogWarning($"[PAG] PreloadCompositionCoroutine timeout: {pagName}, path={absPath}");
        }
        else
        {
            Debug.Log($"[PAG] PreloadCompositionCoroutine ready: {absPath}");
        }

        onDone?.Invoke(ready);
#else
        onDone?.Invoke(false);
        yield break;
#endif
    }

    public void ResetPlayStartedSignal()
    {
        _playStartedSignal = false;
    }

    public void ResetGpuDisplayReadySignal()
    {
        _gpuDisplayReadySignal = false;
    }

    public void ResetPlaybackFinished()
    {
        _playbackFinished = false;
    }

    internal void HandlePlayStarted(string message)
    {
        _playStartedSignal = true;
        string pagLeaf = string.IsNullOrEmpty(_lastPlayPagLeaf) ? InstanceKey : _lastPlayPagLeaf;
        Debug.Log($"[PAG] Play started: pag={pagLeaf}, instance={InstanceKey}");
        if (_fguiPresenter.NeedsDisplayLayoutResync)
        {
            ResyncFguiDisplayLayout();
        }

        OnPlayStarted?.Invoke();
    }

    internal void HandleGpuTextureRequest(string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_renderTarget != PagRenderTarget.FguiTexture)
        {
            return;
        }

        if (_gpuBindCoroutine != null)
        {
            PagCallbackHub.Instance.StopRunCoroutine(_gpuBindCoroutine);
        }

        _gpuBindCoroutine = PagCallbackHub.Instance.RunCoroutine(BindGpuTextureAndStartPlayback(message));
#endif
    }

    internal void HandleGpuRenderFrame(string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
#if DEVELOPMENT_BUILD
        Profiler.BeginSample("PAG.GpuRenderFrame");
#endif
        if (_renderTarget != PagRenderTarget.FguiTexture)
        {
#if DEVELOPMENT_BUILD
            Profiler.EndSample();
#endif
            return;
        }

        double progress = 0.0;
        if (!string.IsNullOrEmpty(message))
        {
            double.TryParse(message, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out progress);
        }

        if (PagGpuSyncGroup.Contains(InstanceKey))
        {
            PagGpuSyncGroup.OnGpuRenderRequested(InstanceKey, progress);
#if DEVELOPMENT_BUILD
            Profiler.EndSample();
#endif
            return;
        }

        PagUnityGlBridge.IssueFlushPagGpuEvent(_textureSlotId, InstanceKey, progress);
#if DEVELOPMENT_BUILD
        Profiler.EndSample();
#endif
#endif
    }

    internal void HandleGpuFrameReady(string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
#if DEVELOPMENT_BUILD
        Profiler.BeginSample("PAG.GpuFrameReady");
#endif
        if (_renderTarget != PagRenderTarget.FguiTexture)
        {
#if DEVELOPMENT_BUILD
            Profiler.EndSample();
#endif
            return;
        }

        if (PagGpuSyncGroup.Contains(InstanceKey))
        {
            PagGpuSyncGroup.OnGpuFramePresented(InstanceKey);
            SafeCall("OnGpuFlushCompleted", () => _pagBridge.CallStatic("OnGpuFlushCompleted", InstanceKey));
#if DEVELOPMENT_BUILD
            Profiler.EndSample();
#endif
            return;
        }

        bool deferPresent = TryCallBool("ShouldDeferFguiGpuPresent", () =>
            _pagBridge.CallStatic<bool>("ShouldDeferFguiGpuPresent", InstanceKey));

        if (!deferPresent)
        {
            _gpuFlushPresentCount++;
            _fguiPresenter.OnGpuFrameReady();
        }
#if DEVELOPMENT_BUILD
        else
        {
            Debug.Log($"[PAG] defer FGUI present instance={InstanceKey}");
        }
#endif

        SafeCall("OnGpuFlushCompleted", () => _pagBridge.CallStatic("OnGpuFlushCompleted", InstanceKey));
#if DEVELOPMENT_BUILD
        Profiler.EndSample();
#endif
#endif
    }

    internal void HandleGpuSyncFlushFrame0()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        PagCallbackHub.Instance.RunCoroutine(SyncFlushFrame0Coroutine());
#endif
    }

    private IEnumerator SyncFlushFrame0Coroutine()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        yield return PagUnityGlBridge.FlushChainFrame0Coroutine(_textureSlotId, InstanceKey);
#else
        yield break;
#endif
    }

    internal void OnGpuFramePresentedForFgui(bool skipInvalidate = false)
    {
        if (skipInvalidate)
        {
            _fguiPresenter.UpdateGpuFrameTexture();
        }
        else
        {
            _fguiPresenter.OnGpuFrameReady();
        }
    }

    internal void InvalidateFguiBatchingFromSyncGroup()
    {
        _fguiPresenter.InvalidateBatchingOnce();
    }

    internal void RequestNextGpuFrameFromSyncGroup()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        SafeCall("RequestNextGpuFrame", () => _pagBridge.CallStatic("RequestNextGpuFrame", InstanceKey));
#endif
    }

    internal static void RequestNextGpuFrameBatch(IEnumerable<string> instanceKeys)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_pagBridge == null || instanceKeys == null)
        {
            return;
        }

        var keys = new List<string>();
        foreach (string key in instanceKeys)
        {
            if (!string.IsNullOrEmpty(key))
            {
                keys.Add(key);
            }
        }

        if (keys.Count == 0)
        {
            return;
        }

        if (keys.Count == 1)
        {
            PagControllerRegistry.Resolve(keys[0])?.RequestNextGpuFrameFromSyncGroup();
            return;
        }

        SafeCall("RequestNextGpuFrameBatch", () =>
            _pagBridge.CallStatic("RequestNextGpuFrameBatch", (object)keys.ToArray()));
#endif
    }

    internal bool StartFguiGpuPlaybackFromSyncGroup()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        bool ok = SafeCallBool("StartFguiGpuPlaybackSync", () =>
            _pagBridge.CallStatic<bool>("StartFguiGpuPlaybackSync", InstanceKey));
        return ok;
#else
        return false;
#endif
    }

    internal void ArmFguiGpuPlaybackClock()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        SafeCall("ArmFguiGpuPlaybackClock", () => _pagBridge.CallStatic("ArmFguiGpuPlaybackClock", InstanceKey));
#endif
    }

    /// <summary>P1：静默 flush progress=0，再 arm 墙钟并显示。</summary>
    internal IEnumerator RunGpuWarmupAndArmPlaybackCoroutine()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        for (int i = 0; i < GpuWarmupFlushCount; i++)
        {
            yield return PagUnityGlBridge.FlushCoroutine(_textureSlotId, InstanceKey, 0.0);
        }

        ArmFguiGpuPlaybackClock();
        SetFguiVisible(true);
        MarkGpuDisplayReady();
#endif
        yield break;
    }

    internal void MarkGpuDisplayReady()
    {
        _gpuDisplayReadySignal = true;
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        Debug.Log($"[PAG] texture display ready instance={InstanceKey}");
#endif
    }

    internal void SetFguiGpuExternalPump(bool externalPump)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        SafeCall("SetFguiGpuExternalPump", () =>
            _pagBridge.CallStatic("SetFguiGpuExternalPump", InstanceKey, externalPump));
#endif
    }

    internal void HandlePlaybackFinished(string message)
    {
        _playbackFinished = true;

        _completedPlayCount++;
        TrySchedulePeriodicMaintenance();
        Debug.Log($"[PAG] Playback finished instance={InstanceKey} playCount={_completedPlayCount}"
            + $" gpuStillActive={IsFguiGpuPlaybackStillActive()}");
        OnPlaybackFinished?.Invoke();
    }

    private bool IsFguiGpuPlaybackStillActive()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_renderTarget != PagRenderTarget.FguiTexture || _pagBridge == null)
        {
            return false;
        }

        try
        {
            return _pagBridge.CallStatic<bool>("IsFguiGpuPlaybackActive", InstanceKey);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[PAG] IsFguiGpuPlaybackActive: {ex.Message}");
            return false;
        }
#else
        return false;
#endif
    }

    private void TrySchedulePeriodicMaintenance()
    {
        if (UnloadAssetsEveryPlayCount <= 0)
        {
            return;
        }

        if (_completedPlayCount % UnloadAssetsEveryPlayCount != 0)
        {
            return;
        }

        if (_maintenanceCoroutine != null)
        {
            return;
        }

        _maintenanceCoroutine = PagCallbackHub.Instance.RunCoroutine(PeriodicMaintenanceCoroutine());
    }

    private IEnumerator PeriodicMaintenanceCoroutine()
    {
        int pendingOps = PagUnityGlBridge.GetPendingOpCount();
        if (pendingOps >= GlQueueBacklogWarnThreshold)
        {
            Debug.LogError($"[PAG] GL queue backlog={pendingOps} instance={InstanceKey} playCount={_completedPlayCount}");
        }

        Debug.Log($"[PAG] periodic maintenance instance={InstanceKey} playCount={_completedPlayCount}");
        yield return PagPathHelper.DeferredUnloadUnusedAssets();
        _maintenanceCoroutine = null;
    }

    /// <summary>循环播放下一份 PAG 前清理 GPU 协程，避免帧请求堆积。</summary>
    public void PrepareBetweenPlaybackCycles()
    {
        StopGpuFrameCoroutines();
    }

    private void StopGpuFrameCoroutines()
    {
        if (_gpuBindCoroutine != null)
        {
            PagCallbackHub.Instance.StopRunCoroutine(_gpuBindCoroutine);
            _gpuBindCoroutine = null;
        }
    }

    private IEnumerator DestroyGpuTextureCoroutine()
    {
        yield return PagUnityGlBridge.DestroyTextureCoroutine(_textureSlotId, InstanceKey);
        _destroyGpuCoroutine = null;
    }

    private void ResetBoundGpuTexture()
    {
        _boundGpuTexId = 0;
        _boundGpuTexW = 0;
        _boundGpuTexH = 0;
        _boundGpuTexPtr = IntPtr.Zero;
    }

    internal void HandleExportFinished(string message)
    {
        Debug.Log($"PAG export finished: {message}");
        OnExportFinished?.Invoke(message);
    }

    public void SetRenderTarget(PagRenderTarget target)
    {
        _renderTarget = target;
#if UNITY_ANDROID && !UNITY_EDITOR
        SafeCall("SetRenderTarget", () => _pagBridge.CallStatic("SetRenderTarget", InstanceKey, (int)target));
#endif
    }

    public void ConfigureFguiFrame(int maxDisplaySide, int fps)
    {
        _fguiTargetFps = fps > 0 ? fps : 30;
#if UNITY_ANDROID && !UNITY_EDITOR
        SafeCall("ConfigureFguiFrame", () => _pagBridge.CallStatic("SetFguiFrameConfig", InstanceKey, maxDisplaySide, fps));
#endif
    }

    /// <summary>FGUI pagEffect 相对 PAG 合成尺寸的显示倍率（1=原尺寸，2=1024 合成显示为 2048）。</summary>
    public void SetFguiDisplayScale(float scale)
    {
        _fguiDisplayScale = scale > 0f ? scale : 1f;
    }

    public void SetFguiClampDisplayToHolder(bool clamp)
    {
        _fguiPresenter.ClampDisplayToHolder = clamp;
    }

    public void BindFguiLoader(GLoader loader)
    {
        _fguiPresenter.ConfigureAnchor(_fguiAnchor, _fguiLoaderName);
        _fguiPresenter.Bind(loader);
    }

    public void SetFguiVisible(bool visible)
    {
        _fguiPresenter.SetVisible(visible);
    }

    public void PrepareFguiLayoutBeforePlay()
    {
        if (_fguiAnchor == null)
        {
            return;
        }

        EnsureFguiBinding();
        _fguiPresenter.RefreshDisplayLayout();
    }

    /// <summary>合成尺寸就绪后，按当前 displayScale 重算 FGUI pagEffect 显示尺寸。</summary>
    public void SyncFguiDisplayLayoutFromComposition()
    {
        ResyncFguiDisplayLayout();
    }

    public void ClearFguiPresentation()
    {
        _fguiPresenter.Clear();
    }

    public PagRenderTarget CurrentRenderTarget => _renderTarget;

    public IEnumerator WaitForPlaybackFinished(float timeoutSec)
    {
        ResetPlaybackFinished();
        float deadline = Time.unscaledTime + timeoutSec;
        while (!_playbackFinished && Time.unscaledTime < deadline)
        {
            yield return null;
        }

        if (!_playbackFinished)
        {
            Debug.LogWarning($"[PAG] WaitForPlaybackFinished timeout after {timeoutSec}s instance={InstanceKey}");
        }
    }

    public IEnumerator WaitForPlayStarted(float timeoutSec)
    {
        ResetPlayStartedSignal();
        float deadline = Time.unscaledTime + timeoutSec;
        while (!_playStartedSignal && Time.unscaledTime < deadline)
        {
            yield return null;
        }
    }

    /// <summary>P1 预热完成且 pagEffect 已 SetVisible 后可与 Spine 同步起跑（早于 PlayStarted）。</summary>
    public IEnumerator WaitForGpuDisplayReady(float timeoutSec)
    {
        float deadline = Time.unscaledTime + timeoutSec;
        while (!_gpuDisplayReadySignal && Time.unscaledTime < deadline)
        {
            yield return null;
        }

        if (!_gpuDisplayReadySignal)
        {
            Debug.LogWarning($"[PAG] WaitForGpuDisplayReady timeout after {timeoutSec}s instance={InstanceKey}");
        }
    }

    public float GetCompositionDurationSecWithFallback(float fallbackSec)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_pagBridge == null)
        {
            return fallbackSec;
        }

        try
        {
            long durationUs = _pagBridge.CallStatic<long>("GetCompositionDurationUs", InstanceKey);
            if (durationUs > 0)
            {
                return durationUs / 1_000_000f + 0.5f;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[PAG] GetCompositionDurationSecWithFallback: {ex.Message}");
        }
#endif
        return fallbackSec;
    }

    public int GetCompositionFrameRate()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_pagBridge == null)
        {
            return 0;
        }

        try
        {
            float frameRate = _pagBridge.CallStatic<float>("GetCompositionFrameRate", InstanceKey);
            if (frameRate > 0f)
            {
                return Mathf.RoundToInt(frameRate);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[PAG] GetCompositionFrameRate: {ex.Message}");
        }
#endif
        return 0;
    }

    private void SetupGpuCallbacksBeforePlay()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_renderTarget != PagRenderTarget.FguiTexture)
        {
            return;
        }

        SafeCall("SetGpuTextureRequestCallback", () =>
            _pagBridge.CallStatic("SetGpuTextureRequestCallback", InstanceKey, PagCallbackHub.HubObjectName, nameof(PagCallbackHub.OnPagGpuTextureRequest)));
        SafeCall("SetGpuRenderCallback", () =>
            _pagBridge.CallStatic("SetGpuRenderCallback", InstanceKey, PagCallbackHub.HubObjectName, nameof(PagCallbackHub.OnPagGpuRenderFrame)));
        SafeCall("SetPlaybackFinishedCallback", () =>
            _pagBridge.CallStatic("SetPlaybackFinishedCallback", InstanceKey, PagCallbackHub.HubObjectName, nameof(PagCallbackHub.OnPagPlaybackFinished)));
#endif
    }

    private IEnumerator BindGpuTextureAndStartPlayback(string sizeMessage)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_pagBridge == null || !PagUnityGlBridge.IsSupported)
        {
            Debug.LogError($"[PAG Texture] BindGpuTexture failed: bridge unavailable instance={InstanceKey}");
            yield break;
        }

        yield return PagUnityGlBridge.WaitForSlotDestroyComplete(_textureSlotId);

        int texW = 512;
        int texH = 512;
        if (!string.IsNullOrEmpty(sizeMessage))
        {
            string[] parts = sizeMessage.Split(',');
            if (parts.Length >= 2)
            {
                int.TryParse(parts[0], out texW);
                int.TryParse(parts[1], out texH);
            }
        }

        int boundTexId = 0;
        IntPtr boundTexPtr = IntPtr.Zero;
        bool reuseGpuTexture = _boundGpuTexId > 0 && _boundGpuTexPtr != IntPtr.Zero
            && texW == _boundGpuTexW && texH == _boundGpuTexH;
        if (reuseGpuTexture)
        {
            boundTexId = _boundGpuTexId;
            boundTexPtr = _boundGpuTexPtr;
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.Log($"[PAG Texture] reuse texture instance={InstanceKey} slot={_textureSlotId} size={texW}x{texH}");
#endif
        }
        else
        {
            yield return PagUnityGlBridge.EnsureTextureCoroutine(_textureSlotId, texW, texH, (texId, texPtr) =>
            {
                boundTexId = texId;
                boundTexPtr = texPtr;
            });
            _boundGpuTexId = boundTexId;
            _boundGpuTexW = texW;
            _boundGpuTexH = texH;
            _boundGpuTexPtr = boundTexPtr;
        }

        if (boundTexId <= 0 || boundTexPtr == IntPtr.Zero)
        {
            Debug.LogError($"[PAG Texture] CreateExternalTexture failed instance={InstanceKey} slot={_textureSlotId} size={texW}x{texH}");
            yield break;
        }

        TrySyncFguiDisplaySizeFromNative();
        _fguiPresenter.BindExternalTexture(boundTexPtr, texW, texH);
        _fguiPresenter.RefreshDisplayLayout();
        _fguiPresenter.SetVisible(false);

        if (PagGpuSyncGroup.Contains(InstanceKey))
        {
            if (!BindGpuTextureSync(boundTexId, texW, texH))
            {
                Debug.LogError($"[PAG Texture] BindGpuTextureSync failed instance={InstanceKey} id={boundTexId} slot={_textureSlotId} size={texW}x{texH}");
                _gpuBindCoroutine = null;
                yield break;
            }

            PagGpuSyncGroup.OnGpuBound(InstanceKey);
            LogJni($"GPU texture bound (sync defer start) instance={InstanceKey} id={boundTexId} slot={_textureSlotId} size={texW}x{texH}");
            _gpuBindCoroutine = null;
            yield break;
        }

        SafeCall("BindGpuTexture", () => _pagBridge.CallStatic("BindGpuTexture", InstanceKey, boundTexId, texW, texH));

        SafeCall("StartFguiGpuPlayback", () => _pagBridge.CallStatic("StartFguiGpuPlayback", InstanceKey));
        yield return null;
        yield return null;
        yield return PagUnityGlBridge.SetupBatchCoroutine(new[] { (_textureSlotId, InstanceKey) });
        yield return RunGpuWarmupAndArmPlaybackCoroutine();
        RequestNextGpuFrameFromSyncGroup();
        LogJni($"GPU texture bound instance={InstanceKey} id={boundTexId} slot={_textureSlotId} size={texW}x{texH} reuse={reuseGpuTexture}");
        _gpuBindCoroutine = null;
#else
        _gpuBindCoroutine = null;
        yield break;
#endif
    }

    private void ResyncFguiDisplayLayout()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_renderTarget != PagRenderTarget.FguiTexture)
        {
            return;
        }

        TrySyncFguiDisplaySizeFromNative();
        _fguiPresenter.RefreshDisplayLayout();
#endif
    }

    private void TrySyncFguiDisplaySizeFromNative()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_renderTarget != PagRenderTarget.FguiTexture || _pagBridge == null)
        {
            return;
        }

        try
        {
            int cw = _pagBridge.CallStatic<int>("GetCompositionWidth", InstanceKey);
            int ch = _pagBridge.CallStatic<int>("GetCompositionHeight", InstanceKey);
            if (cw > 0 && ch > 0)
            {
                int displayW = Mathf.Max(1, Mathf.RoundToInt(cw * _fguiDisplayScale));
                int displayH = Mathf.Max(1, Mathf.RoundToInt(ch * _fguiDisplayScale));
                _fguiPresenter.SetDisplaySize(displayW, displayH);
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                Debug.Log($"[PAG Texture] composition display size {displayW}x{displayH} "
                    + $"(composition {cw}x{ch} scale={_fguiDisplayScale:F2}) instance={InstanceKey}");
#endif
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[PAG Texture] TrySyncFguiDisplaySizeFromNative: {ex.Message}");
        }
#endif
    }

    public bool PlayPag(string pagName, string positionType, string extra = "")
    {
        if (!_attached)
        {
            Debug.LogError($"[PAG] Play failed: not attached, instance={InstanceKey}");
            return false;
        }

        PrepareBetweenPlaybackCycles();
        ResetPlayStartedSignal();
        ResetGpuDisplayReadySignal();
        ResetPlaybackFinished();
        _lastPlayPagLeaf = pagName;
        string pagPath = ResolvePagPath(pagName);
        if (pagPath == null)
        {
            Debug.LogError($"[PAG] Play failed, path not found: {pagName}");
            return false;
        }

        if (_renderTarget == PagRenderTarget.FguiTexture && _fguiPresenter.Loader == null)
        {
            Debug.LogError($"[PAG] Play failed: pagEffect not bound, instance={InstanceKey}, anchor={_fguiAnchor?.name}");
            return false;
        }

        if (!PagPathHelper.IsValidPagFile(pagPath))
        {
            long bytes = 0;
            try
            {
                if (File.Exists(pagPath))
                {
                    bytes = new FileInfo(pagPath).Length;
                }
            }
            catch
            {
                // ignored
            }

            Debug.LogError($"[PAG] Play failed, invalid pag file: {pagName}, path={pagPath}, bytes={bytes}");
            return false;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            EnsureInit();
            if (_pagBridge == null)
            {
                Debug.LogError("[PAG JNI] Play failed: PagBridge not initialized");
                return false;
            }

            if (_renderTarget == PagRenderTarget.FguiTexture)
            {
                _fguiPresenter.ResetDisplaySizeForNewComposition();
                if (AutoConcurrentGpuSync && !_skipAutoSyncJoinForPlaylist)
                {
                    PagGpuSyncGroup.TryJoin(InstanceKey, _fguiTargetFps);
                }
            }

            SetupGpuCallbacksBeforePlay();
            LogJni($"Play instance={InstanceKey}: {pagName}, path={pagPath}, extra={extra ?? ""}, renderTarget={_renderTarget}");
            _pagBridge.CallStatic("Play", pagPath, positionType ?? "center", extra ?? "",
                InstanceKey, PagCallbackHub.HubObjectName, nameof(PagCallbackHub.OnPagOverlayPlayStarted));
            Debug.Log($"[PAG] Play JNI dispatched instance={InstanceKey}: {pagName}, path={pagPath}, position={positionType}, extra={extra ?? ""}");
            LogJni($"Play JNI returned instance={InstanceKey}: {pagName}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PAG JNI] Play exception: {pagName}, {ex}");
            return false;
        }
#else
        Debug.LogWarning($"[PAG] Play skipped (non-Android/Editor): {pagName}, path={pagPath}");
        return true;
#endif
    }

    public void StopPag()
    {
        _skipAutoSyncJoinForPlaylist = false;
#if UNITY_ANDROID && !UNITY_EDITOR
        if (PagGpuSyncGroup.Contains(InstanceKey))
        {
            PagGpuSyncGroup.TryLeave(InstanceKey);
        }

        StopGpuFrameCoroutines();
        ResetPlayStartedSignal();
        ResetGpuDisplayReadySignal();
        if (_maintenanceCoroutine != null)
        {
            PagCallbackHub.Instance.StopRunCoroutine(_maintenanceCoroutine);
            _maintenanceCoroutine = null;
        }

        try
        {
            EnsureInit();
            if (_pagBridge != null)
            {
                ClearFguiGpuPlaylist();
                _pagBridge.CallStatic("Stop", InstanceKey);
                LogJni($"Stop instance={InstanceKey}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PAG JNI] Stop exception: {ex}");
        }

        if (_renderTarget == PagRenderTarget.FguiTexture)
        {
            _fguiPresenter.Clear();
            ResetBoundGpuTexture();
            if (_destroyGpuCoroutine != null)
            {
                PagCallbackHub.Instance.StopRunCoroutine(_destroyGpuCoroutine);
                _destroyGpuCoroutine = null;
            }

            if (PagCallbackHub.Instance != null)
            {
                _destroyGpuCoroutine = PagCallbackHub.Instance.RunCoroutine(DestroyGpuTextureCoroutine());
            }
            else
            {
                PagUnityGlBridge.DestroyTexture(_textureSlotId, InstanceKey);
            }
        }

        Debug.Log($"[PAG] Stop instance={InstanceKey}");
#endif
    }

    public void PausePag()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        SafeCall("Pause", () => _pagBridge.CallStatic("Pause", InstanceKey));
#endif
    }

    public void ResumePag()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        SafeCall("Resume", () => _pagBridge.CallStatic("Resume", InstanceKey));
#endif
    }

    public void SetRepeatCount(int count)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        SafeCall("SetRepeatCount", () => _pagBridge.CallStatic("SetRepeatCount", InstanceKey, count));
#endif
    }


    /// <summary>Phase4E：登记 Native 播放列表；Play 首段前调用。</summary>
    public bool SetFguiGpuPlaylist(IReadOnlyList<PagSegment> segments)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_renderTarget != PagRenderTarget.FguiTexture)
        {
            return false;
        }

        if (segments == null || segments.Count == 0)
        {
            Debug.LogWarning($"[PAG] SetFguiGpuPlaylist: empty segments instance={InstanceKey}");
            return false;
        }

        var paths = new string[segments.Count];
        var repeats = new int[segments.Count];
        for (int i = 0; i < segments.Count; i++)
        {
            PagSegment segment = segments[i];
            string pagPath = ResolvePagPath(segment.PagFileName);
            if (string.IsNullOrEmpty(pagPath))
            {
                Debug.LogWarning($"[PAG] SetFguiGpuPlaylist path null: {segment.PagFileName}, instance={InstanceKey}");
                return false;
            }

            paths[i] = pagPath;
            repeats[i] = segment.RepeatCount;
        }

        try
        {
            EnsureInit();
            if (_pagBridge == null)
            {
                return false;
            }

            _pagBridge.CallStatic("SetFguiGpuPlaylist", InstanceKey, paths, repeats);
            LogJni($"SetFguiGpuPlaylist: count={segments.Count}, instance={InstanceKey}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[PAG] SetFguiGpuPlaylist: {ex.Message}");
            return false;
        }
#else
        return false;
#endif
    }

    public void ClearFguiGpuPlaylist()
    {
        _skipAutoSyncJoinForPlaylist = false;
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            EnsureInit();
            if (_pagBridge == null)
            {
                return;
            }

            _pagBridge.CallStatic("ClearFguiGpuPlaylist", InstanceKey);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[PAG] ClearFguiGpuPlaylist: {ex.Message}");
        }
#endif
    }

    /// <summary>Phase4E：打断循环段并无缝切到下一段（用法 3）。</summary>
    public void AdvanceFguiGpuSequence()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            EnsureInit();
            if (_pagBridge == null)
            {
                return;
            }

            _pagBridge.CallStatic("AdvanceFguiGpuPlaylist", InstanceKey);
            LogJni($"AdvanceFguiGpuPlaylist instance={InstanceKey}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[PAG] AdvanceFguiGpuSequence: {ex.Message}");
        }
#endif
    }

    /// <summary>
    /// Native 播放列表无缝连播；C# 仅 Play 首段并等待整链 PlaybackFinished。
    /// </summary>
    /// <param name="useGpuSyncGroup">
    /// true：Play 时 TryJoin 动态合组（多 NPC 同屏防整屏闪）；false：退组独立出帧（默认，PAG4 单槽链等）。
    /// </param>
    public bool PlayFguiGpuSequence(IReadOnlyList<PagSegment> segments, string positionType, string extra = "",
        bool useGpuSyncGroup = false)
    {
        if (segments == null || segments.Count == 0)
        {
            Debug.LogWarning($"[PAG] PlayFguiGpuSequence: empty segments instance={InstanceKey}");
            return false;
        }

        if (!SetFguiGpuPlaylist(segments))
        {
            return false;
        }

        SetRepeatCount(segments[0].RepeatCount);
#if UNITY_ANDROID && !UNITY_EDITOR
        if (useGpuSyncGroup)
        {
            _skipAutoSyncJoinForPlaylist = false;
            LogJni($"PlayFguiGpuSequence: SyncGroup join enabled instance={InstanceKey} segments={segments.Count}");
        }
        else
        {
            PagGpuSyncGroup.TryLeave(InstanceKey);
            _skipAutoSyncJoinForPlaylist = true;
            LogJni($"PlayFguiGpuSequence: skip SyncGroup join instance={InstanceKey} segments={segments.Count}");
        }
#endif
        return PlayPag(segments[0].PagFileName, positionType, extra);
    }

    public IEnumerator WaitForFguiGpuSequenceFinished(float timeoutSec)
    {
        ResetPlaybackFinished();
        float deadline = Time.unscaledTime + timeoutSec;
        while (!PlaybackFinished && Time.unscaledTime < deadline)
        {
            yield return null;
        }

        if (!PlaybackFinished)
        {
            Debug.LogWarning($"[PAG] WaitForFguiGpuSequenceFinished timeout after {timeoutSec}s instance={InstanceKey}");
        }
    }

    /// <summary>纹理模式是否仍在出帧（段末 chain switch 成功后为 true）。</summary>
    public bool IsFguiGpuPlaybackActive()
    {
        return IsFguiGpuPlaybackStillActive();
    }

    public void SetForceBitmapOverlayFallback(bool force)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        SafeCall("SetForceBitmapOverlayFallback", () => _pagBridge.CallStatic("SetForceBitmapOverlayFallback", force));
        LogJni($"SetForceBitmapOverlayFallback: {force}");
#endif
    }

    public void SetRightAdaptive(float w, float h)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        SafeCall("SetRightAdaptive", () => _pagBridge.CallStatic("SetRightAdaptive", InstanceKey, w, h));
#endif
    }

    public void LayoutPagAuto(string place)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        SafeCall("LayoutPagAuto", () => _pagBridge.CallStatic("LayoutPagAuto", InstanceKey, place ?? "center"));
#endif
    }

    public void ReplaceText(int index, string text)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        SafeCall("ReplaceText", () => _pagBridge.CallStatic("ReplaceText", InstanceKey, index, text ?? ""));
#endif
    }

    public void ReplaceImage(int index, string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
        {
            Debug.LogWarning($"ReplaceImage: file not found {imagePath}");
            return;
        }
#if UNITY_ANDROID && !UNITY_EDITOR
        SafeCall("ReplaceImage", () => _pagBridge.CallStatic("ReplaceImage", InstanceKey, index, imagePath));
#endif
    }

    public void PlayPagInterval(string pagName, long startTimeUs, long durationUs,
        string positionType = "center", string extra = "")
    {
        string pagPath = ResolvePagPath(pagName);
        if (pagPath == null)
        {
            return;
        }
#if UNITY_ANDROID && !UNITY_EDITOR
        SafeCall("PlayInterval", () => _pagBridge.CallStatic("PlayInterval", InstanceKey, pagPath, startTimeUs, durationUs,
            positionType ?? "center", extra ?? ""));
#endif
    }

    public void PlayMultiPag(string baseDirectory, int count, int colNum = 4,
        string positionType = "full", string extra = "")
    {
        if (string.IsNullOrEmpty(baseDirectory))
        {
            return;
        }
        if (!baseDirectory.EndsWith("/"))
        {
            baseDirectory += "/";
        }
#if UNITY_ANDROID && !UNITY_EDITOR
        SafeCall("PlayMultiPag", () => _pagBridge.CallStatic("PlayMultiPag", InstanceKey, baseDirectory, count, colNum,
            positionType ?? "full", extra ?? ""));
#endif
    }

    public void ExportPagVideo(string pagName, string outputName)
    {
        string pagPath = ResolvePagPath(pagName);
        if (pagPath == null)
        {
            return;
        }
#if UNITY_ANDROID && !UNITY_EDITOR
        SafeCall("ExportVideo", () => _pagBridge.CallStatic("ExportVideo", InstanceKey, pagPath, outputName ?? "pag_export",
            PagCallbackHub.HubObjectName, nameof(PagCallbackHub.OnPagExportFinished)));
#endif
    }

    public string ResolvePagPath(string fileName)
    {
        return PagPathHelper.Resolve(fileName, _gamePagFolder);
    }

    public static string ResolvePagPath(string fileName, string gamePagFolder)
    {
        return PagPathHelper.Resolve(fileName, gamePagFolder);
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private bool BindGpuTextureSync(int boundTexId, int texW, int texH)
    {
        return SafeCallBool("BindGpuTextureSync", () =>
            _pagBridge.CallStatic<bool>("BindGpuTextureSync", InstanceKey, boundTexId, texW, texH));
    }

    private static void SafeCall(string name, Action action)
    {
        try
        {
            EnsureInit();
            if (_pagBridge == null)
            {
                Debug.LogError($"[PAG JNI] {name} failed: PagBridge not initialized");
                return;
            }

            action();
            LogJni($"{name} OK");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PAG JNI] {name} exception: {ex}");
        }
    }

    private bool SafeCallBool(string name, Func<bool> action)
    {
        try
        {
            EnsureInit();
            if (_pagBridge == null)
            {
                Debug.LogError($"[PAG JNI] {name} failed: PagBridge not initialized");
                return false;
            }

            bool ok = action();
            if (ok)
            {
                LogJni($"{name} OK");
            }
            else
            {
                Debug.LogError($"[PAG JNI] {name} returned false instance={InstanceKey}");
            }

            return ok;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PAG JNI] {name} exception: {ex}");
            return false;
        }
    }

    /** 查询型 JNI：false 为正常结果，不记 error。 */
    private bool TryCallBool(string name, Func<bool> action)
    {
        try
        {
            EnsureInit();
            if (_pagBridge == null)
            {
                return false;
            }

            return action();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PAG JNI] {name} exception: {ex}");
            return false;
        }
    }

    private static void LogJni(string message)
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        Debug.Log($"[PAG JNI] {message}");
        try
        {
            using (AndroidJavaClass log = new AndroidJavaClass("android.util.Log"))
            {
                log.CallStatic<int>("i", JniLogTag, message);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[PAG JNI] android.util.Log failed: {ex.Message}");
        }
#endif
    }
#endif
}
