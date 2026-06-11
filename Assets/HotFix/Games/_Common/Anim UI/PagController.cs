using System;
using System.Collections;
using System.IO;
using FairyGUI;
using UnityEngine;

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

    private static AndroidJavaClass _pagBridge;
    private static bool _initialized;
    private static int _nextTextureSlotId = 1000;

    /// <summary>每完成多少次播放触发 UnloadUnusedAssets；0=关闭。</summary>
    public static int UnloadAssetsEveryPlayCount = 40;

    /// <summary>repeat=-1 无限循环时每 N 圈软重启 GPU Player；0=关闭。</summary>
    public static int GpuPlayerRecycleEveryLoop = 100;

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
    private bool _playbackFinished;
    private string _lastPlayPagLeaf;
    private PagRenderTarget _renderTarget = PagRenderTarget.Overlay;
    private Coroutine _gpuBindCoroutine;
    private Coroutine _gpuNextFrameCoroutine;
    private float _fguiTargetFrameInterval = 1f / 30f;
    private float _lastGpuFramePresentTime;
    private int _boundGpuTexId;
    private int _boundGpuTexW;
    private int _boundGpuTexH;
    private IntPtr _boundGpuTexPtr = IntPtr.Zero;
    private int _completedPlayCount;
    private Coroutine _maintenanceCoroutine;

    private GComponent _fguiAnchor;

    public string InstanceKey { get; private set; }

    public GLoader FguiLoader => _fguiPresenter.Loader;

    internal int TextureSlotId => _textureSlotId;

    public bool PlayStarted => _playStartedSignal;

    public bool PlaybackFinished => _playbackFinished;

    public PagController(string instanceKey, string gamePagFolder = null)
    {
        InstanceKey = string.IsNullOrEmpty(instanceKey)
            ? $"Pag_{Guid.NewGuid():N}"
            : instanceKey;
        _gamePagFolder = string.IsNullOrEmpty(gamePagFolder)
            ? PagPathHelper.DefaultGamePagFolder
            : gamePagFolder;
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

    public void Attach(GComponent fguiAnchor)
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
        Debug.Log($"[PAG] Attached instance={InstanceKey}, fgui={_fguiAnchor.name}");
    }

    private void EnsureFguiBinding()
    {
        if (_fguiAnchor == null)
        {
            return;
        }

        GLoader pagEffect = PagFguiGpuPresenter.TryGetPagEffectLoader(_fguiAnchor);
        if (pagEffect == null)
        {
            Debug.LogError($"[PAG] FGUI 绑定失败: instance={InstanceKey}, anchor={_fguiAnchor.name}");
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

    public void ResetPlayStartedSignal()
    {
        _playStartedSignal = false;
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
        if (_renderTarget != PagRenderTarget.FguiTexture)
        {
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
            return;
        }

        PagUnityGlBridge.IssueFlushPagGpuEvent(_textureSlotId, InstanceKey, progress);
#endif
    }

    internal void HandleGpuFrameReady(string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_renderTarget != PagRenderTarget.FguiTexture)
        {
            return;
        }

        if (PagGpuSyncGroup.Contains(InstanceKey))
        {
            PagGpuSyncGroup.OnGpuFramePresented(InstanceKey);
            return;
        }

        _fguiPresenter.OnGpuFrameReady();
        ScheduleNextGpuFrameAfterPresent();
#endif
    }

    internal void OnGpuFramePresentedForFgui()
    {
        _fguiPresenter.OnGpuFrameReady();
    }

    internal bool StartFguiGpuPlaybackFromSyncGroup()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        bool ok = SafeCallBool("StartFguiGpuPlaybackSync", () =>
            _pagBridge.CallStatic<bool>("StartFguiGpuPlaybackSync", InstanceKey));
        if (ok)
        {
            _lastGpuFramePresentTime = 0f;
        }

        return ok;
#else
        return false;
#endif
    }

    internal void RequestNextGpuFrameFromSyncGroup()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        SafeCall("RequestNextGpuFrame", () => _pagBridge.CallStatic("RequestNextGpuFrame", InstanceKey));
#endif
    }

    internal void HandlePlaybackFinished(string message)
    {
        _playbackFinished = true;
        StopGpuFrameCoroutines();
        _completedPlayCount++;
        TrySchedulePeriodicMaintenance();
        Debug.Log($"[PAG] Playback finished instance={InstanceKey} playCount={_completedPlayCount}");
        OnPlaybackFinished?.Invoke();
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

        if (_gpuNextFrameCoroutine != null)
        {
            PagCallbackHub.Instance.StopRunCoroutine(_gpuNextFrameCoroutine);
            _gpuNextFrameCoroutine = null;
        }
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
        if (fps > 0)
        {
            _fguiTargetFrameInterval = 1f / fps;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        SafeCall("ConfigureFguiFrame", () => _pagBridge.CallStatic("SetFguiFrameConfig", InstanceKey, maxDisplaySide, fps));
#endif
    }

    public void BindFguiLoader(GLoader loader)
    {
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

    private void SetupGpuCallbacksBeforePlay()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_renderTarget != PagRenderTarget.FguiTexture)
        {
            return;
        }

        SafeCall("SetGpuFrameCallback", () =>
            _pagBridge.CallStatic("SetGpuFrameCallback", InstanceKey, PagCallbackHub.HubObjectName, nameof(PagCallbackHub.OnPagGpuFrameReady)));
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
            Debug.LogError($"[PAG GPU] BindGpuTexture failed: bridge unavailable instance={InstanceKey}");
            yield break;
        }

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
            Debug.Log($"[PAG GPU] reuse texture instance={InstanceKey} slot={_textureSlotId} size={texW}x{texH}");
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
            Debug.LogError($"[PAG GPU] CreateExternalTexture failed instance={InstanceKey} slot={_textureSlotId} size={texW}x{texH}");
            yield break;
        }

        TrySyncFguiDisplaySizeFromNative();
        _fguiPresenter.BindExternalTexture(boundTexPtr, texW, texH);
        _fguiPresenter.RefreshDisplayLayout();
        _fguiPresenter.SetVisible(true);

        if (PagGpuSyncGroup.Contains(InstanceKey))
        {
            if (!BindGpuTextureSync(boundTexId, texW, texH))
            {
                Debug.LogError($"[PAG GPU] BindGpuTextureSync failed instance={InstanceKey} id={boundTexId} slot={_textureSlotId} size={texW}x{texH}");
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
        _lastGpuFramePresentTime = 0f;
        yield return null;
        yield return null;
        PagUnityGlBridge.IssueSetupPagGpuEvent(_textureSlotId, InstanceKey);
        yield return new WaitForEndOfFrame();
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

    private void ScheduleNextGpuFrameAfterPresent()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_gpuNextFrameCoroutine != null)
        {
            PagCallbackHub.Instance.StopRunCoroutine(_gpuNextFrameCoroutine);
        }

        _gpuNextFrameCoroutine = PagCallbackHub.Instance.RunCoroutine(RequestNextGpuFrameAfterPresent());
#endif
    }

    private IEnumerator RequestNextGpuFrameAfterPresent()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        yield return new WaitForEndOfFrame();

        if (_lastGpuFramePresentTime > 0f)
        {
            float elapsed = Time.unscaledTime - _lastGpuFramePresentTime;
            if (elapsed < _fguiTargetFrameInterval)
            {
                yield return new WaitForSecondsRealtime(_fguiTargetFrameInterval - elapsed);
            }
        }

        _lastGpuFramePresentTime = Time.unscaledTime;
        SafeCall("RequestNextGpuFrame", () => _pagBridge.CallStatic("RequestNextGpuFrame", InstanceKey));
        _gpuNextFrameCoroutine = null;
#else
        yield break;
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
                _fguiPresenter.SetDisplaySize(cw, ch);
                Debug.Log($"[PAG FGUI GPU] composition display size {cw}x{ch} instance={InstanceKey}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[PAG FGUI GPU] TrySyncFguiDisplaySizeFromNative: {ex.Message}");
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
#if UNITY_ANDROID && !UNITY_EDITOR
        if (PagGpuSyncGroup.Contains(InstanceKey))
        {
            PagGpuSyncGroup.EndGroup();
        }

        StopGpuFrameCoroutines();
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
            PagUnityGlBridge.DestroyTexture(_textureSlotId);
            ResetBoundGpuTexture();
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
        if (count < 0 && GpuPlayerRecycleEveryLoop > 0)
        {
            SafeCall("SetGpuPlayerRecycleEveryLoop", () =>
                _pagBridge.CallStatic("SetGpuPlayerRecycleEveryLoop", InstanceKey, GpuPlayerRecycleEveryLoop));
        }
#endif
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

    private static void LogJni(string message)
    {
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
    }
#endif
}
