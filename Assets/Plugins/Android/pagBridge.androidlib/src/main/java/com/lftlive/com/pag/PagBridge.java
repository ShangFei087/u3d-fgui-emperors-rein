package com.lftlive.com.pag;

import android.app.Activity;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;

import org.libpag.PAGFile;

import java.io.File;
import java.util.HashMap;
import java.util.Map;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.atomic.AtomicBoolean;

/**
 * Unity 与 libpag 之间的 JNI 桥接层，对应 pagDemo 中的各项能力。
 * 支持按 instanceKey（Unity GameObject 名）多实例并行播放。
 */
public final class PagBridge {
    private static final String TAG = "PagBridge";
    private static final String DEFAULT_INSTANCE = "_default";
    private static final long UI_SYNC_TIMEOUT_MS = 3000L;

    private static Activity sActivity;
    private static final Handler sMainHandler = new Handler(Looper.getMainLooper());
    private static final Map<String, PagOverlayManager> sManagers = new HashMap<>();
    private static final Map<String, InstanceConfig> sConfigs = new HashMap<>();

    private static final class InstanceConfig {
        int renderMode = PagOverlayManager.RENDER_MODE_OVERLAY;
        int fguiMaxDisplaySide = 0;
        int fguiFps = 60;
        String gpuTextureRequestGo;
        String gpuTextureRequestMethod;
        String gpuRenderGo;
        String gpuRenderMethod;
        String playbackFinishedGo;
        String playbackFinishedMethod;
    }

    private PagBridge() {
    }

    private static String normalizeKey(String instanceKey) {
        return (instanceKey == null || instanceKey.isEmpty()) ? DEFAULT_INSTANCE : instanceKey;
    }

    private static InstanceConfig getOrCreateConfig(String instanceKey) {
        instanceKey = normalizeKey(instanceKey);
        InstanceConfig config = sConfigs.get(instanceKey);
        if (config == null) {
            config = new InstanceConfig();
            sConfigs.put(instanceKey, config);
        }
        return config;
    }

    private static void applyConfig(String instanceKey, PagOverlayManager manager) {
        if (manager == null) {
            return;
        }
        InstanceConfig config = getOrCreateConfig(instanceKey);
        manager.setUnityInstanceKey(instanceKey);
        manager.setRenderTarget(config.renderMode);
        manager.setFguiFrameConfig(config.fguiMaxDisplaySide, config.fguiFps);
        if (config.gpuTextureRequestGo != null && config.gpuTextureRequestMethod != null) {
            manager.setGpuTextureRequestCallback(config.gpuTextureRequestGo, config.gpuTextureRequestMethod);
        }
        if (config.gpuRenderGo != null && config.gpuRenderMethod != null) {
            manager.setGpuRenderCallback(config.gpuRenderGo, config.gpuRenderMethod);
        }
        if (config.playbackFinishedGo != null && config.playbackFinishedMethod != null) {
            manager.setPlaybackFinishedCallback(config.playbackFinishedGo, config.playbackFinishedMethod);
        }
    }

    private static PagOverlayManager getManager(String instanceKey) {
        return sManagers.get(normalizeKey(instanceKey));
    }

    private static boolean ensureManager(String instanceKey) {
        if (sActivity == null) {
            Log.e(TAG, "ensureManager: activity is null");
            return false;
        }
        instanceKey = normalizeKey(instanceKey);
        PagOverlayManager manager = sManagers.get(instanceKey);
        if (manager == null) {
            manager = new PagOverlayManager(sActivity);
            sManagers.put(instanceKey, manager);
            applyConfig(instanceKey, manager);
            Log.i(TAG, "PagOverlayManager created for " + instanceKey);
        }
        return true;
    }

    public static void Init(Activity activity) {
        if (activity == null) {
            Log.e(TAG, "Init: activity is null");
            return;
        }
        sActivity = activity;
        activity.runOnUiThread(() -> {
            ensureManager(DEFAULT_INSTANCE);
            Log.i(TAG, "Init done, managers=" + sManagers.size());
        });
    }

    public static void Play(String path, String positionType, String extra) {
        Play(path, positionType, extra, null, null);
    }

    public static void Play(String path, String positionType, String extra,
                           String callbackGameObject, String callbackMethod) {
        Play(path, positionType, extra, callbackGameObject, callbackGameObject, callbackMethod);
    }

    public static void Play(String path, String positionType, String extra,
                           String instanceKey, String callbackGameObject, String callbackMethod) {
        final String key = normalizeKey(instanceKey);
        runOnUi(() -> {
            if (!ensureManager(key)) {
                return;
            }
            PagOverlayManager manager = getManager(key);
            Log.i(TAG, "Play: instance=" + key + ", path=" + path
                    + ", position=" + positionType + ", extra=" + extra
                    + ", callback=" + callbackGameObject + "." + callbackMethod);
            manager.play(path, positionType, extra, callbackGameObject, callbackMethod);
        });
    }

    public static void Stop() {
        Stop(DEFAULT_INSTANCE);
    }

    public static void Stop(String instanceKey) {
        final String key = normalizeKey(instanceKey);
        runOnUiSync("Stop instance=" + key, () -> {
            PagOverlayManager manager = getManager(key);
            if (manager != null) {
                Log.i(TAG, "Stop: instance=" + key);
                manager.stop();
            }
        });
    }

    public static void Pause() {
        Pause(DEFAULT_INSTANCE);
    }

    public static void Pause(String instanceKey) {
        final String key = normalizeKey(instanceKey);
        runOnUi(() -> {
            PagOverlayManager manager = getManager(key);
            if (manager != null) {
                manager.pause();
            }
        });
    }

    public static void Resume() {
        Resume(DEFAULT_INSTANCE);
    }

    public static void Resume(String instanceKey) {
        final String key = normalizeKey(instanceKey);
        runOnUi(() -> {
            PagOverlayManager manager = getManager(key);
            if (manager != null) {
                manager.resume();
            }
        });
    }

    public static void SetRightAdaptive(float w, float h) {
        SetRightAdaptive(DEFAULT_INSTANCE, w, h);
    }

    public static void SetRightAdaptive(String instanceKey, float w, float h) {
        final String key = normalizeKey(instanceKey);
        runOnUi(() -> {
            if (ensureManager(key)) {
                getManager(key).setRightAdaptive(w, h);
            }
        });
    }

    public static void LayoutPagAuto(String place) {
        LayoutPagAuto(DEFAULT_INSTANCE, place);
    }

    public static void LayoutPagAuto(String instanceKey, String place) {
        final String key = normalizeKey(instanceKey);
        runOnUi(() -> {
            if (ensureManager(key)) {
                Log.i(TAG, "LayoutPagAuto: instance=" + key + ", place=" + place);
                getManager(key).layoutPagAuto(place);
            }
        });
    }

    public static void SetRepeatCount(int count) {
        SetRepeatCount(DEFAULT_INSTANCE, count);
    }

    public static void SetRepeatCount(String instanceKey, int count) {
        final String key = normalizeKey(instanceKey);
        runOnUi(() -> {
            if (ensureManager(key)) {
                Log.i(TAG, "SetRepeatCount: instance=" + key + ", count=" + count);
                getManager(key).setRepeatCount(count);
            }
        });
    }

    public static void SetGpuPlayerRecycleEveryLoop(String instanceKey, int everyLoop) {
        final String key = normalizeKey(instanceKey);
        runOnUi(() -> {
            if (ensureManager(key)) {
                Log.i(TAG, "SetGpuPlayerRecycleEveryLoop: instance=" + key + ", everyLoop=" + everyLoop);
                getManager(key).setGpuPlayerRecycleEveryLoop(everyLoop);
            }
        });
    }

    /** Phase4D：true 时 play 立即走 ImageView 软件出帧，不等待 TextureView Surface。 */
    public static void SetForceBitmapOverlayFallback(boolean force) {
        PagOverlayManager.setForceBitmapOverlayFallback(force);
        Log.i(TAG, "SetForceBitmapOverlayFallback: " + force);
    }

    /** 0=浮层模式（Overlay WM）；1=纹理模式（FGUI ExternalTexture，Spine 可盖在上层）。 */
    public static void SetRenderTarget(int mode) {
        SetRenderTarget(DEFAULT_INSTANCE, mode);
    }

    public static void SetRenderTarget(String instanceKey, int mode) {
        instanceKey = normalizeKey(instanceKey);
        InstanceConfig config = getOrCreateConfig(instanceKey);
        config.renderMode = mode == PagOverlayManager.RENDER_MODE_FGUI_TEXTURE
                ? PagOverlayManager.RENDER_MODE_FGUI_TEXTURE
                : PagOverlayManager.RENDER_MODE_OVERLAY;
        PagOverlayManager manager = getManager(instanceKey);
        if (manager != null) {
            manager.setRenderTarget(config.renderMode);
        }
        Log.i(TAG, "SetRenderTarget: instance=" + instanceKey + ", mode=" + config.renderMode);
    }

    public static void SetFguiFrameConfig(int maxDisplaySide, int fps) {
        SetFguiFrameConfig(DEFAULT_INSTANCE, maxDisplaySide, fps);
    }

    public static void SetFguiFrameConfig(String instanceKey, int maxDisplaySide, int fps) {
        instanceKey = normalizeKey(instanceKey);
        InstanceConfig config = getOrCreateConfig(instanceKey);
        config.fguiMaxDisplaySide = Math.max(0, maxDisplaySide);
        if (fps > 0) {
            config.fguiFps = fps;
        }
        PagOverlayManager manager = getManager(instanceKey);
        if (manager != null) {
            manager.setFguiFrameConfig(config.fguiMaxDisplaySide, config.fguiFps);
        }
        Log.i(TAG, "SetFguiFrameConfig: instance=" + instanceKey
                + ", maxSide=" + config.fguiMaxDisplaySide + ", fps=" + config.fguiFps);
    }

    public static void SetGpuTextureRequestCallback(String callbackGameObject, String callbackMethod) {
        SetGpuTextureRequestCallback(DEFAULT_INSTANCE, callbackGameObject, callbackMethod);
    }

    public static void SetGpuTextureRequestCallback(String instanceKey, String callbackGameObject, String callbackMethod) {
        instanceKey = normalizeKey(instanceKey);
        InstanceConfig config = getOrCreateConfig(instanceKey);
        config.gpuTextureRequestGo = callbackGameObject;
        config.gpuTextureRequestMethod = callbackMethod;
        PagOverlayManager manager = getManager(instanceKey);
        if (manager != null) {
            manager.setGpuTextureRequestCallback(callbackGameObject, callbackMethod);
        }
        Log.i(TAG, "SetGpuTextureRequestCallback: instance=" + instanceKey
                + ", " + callbackGameObject + "." + callbackMethod);
    }

    public static void SetGpuRenderCallback(String callbackGameObject, String callbackMethod) {
        SetGpuRenderCallback(DEFAULT_INSTANCE, callbackGameObject, callbackMethod);
    }

    public static void SetGpuRenderCallback(String instanceKey, String callbackGameObject, String callbackMethod) {
        instanceKey = normalizeKey(instanceKey);
        InstanceConfig config = getOrCreateConfig(instanceKey);
        config.gpuRenderGo = callbackGameObject;
        config.gpuRenderMethod = callbackMethod;
        PagOverlayManager manager = getManager(instanceKey);
        if (manager != null) {
            manager.setGpuRenderCallback(callbackGameObject, callbackMethod);
        }
        Log.i(TAG, "SetGpuRenderCallback: instance=" + instanceKey + ", " + callbackGameObject + "." + callbackMethod);
    }

    public static void SetPlaybackFinishedCallback(String callbackGameObject, String callbackMethod) {
        SetPlaybackFinishedCallback(DEFAULT_INSTANCE, callbackGameObject, callbackMethod);
    }

    public static void SetPlaybackFinishedCallback(String instanceKey, String callbackGameObject, String callbackMethod) {
        instanceKey = normalizeKey(instanceKey);
        InstanceConfig config = getOrCreateConfig(instanceKey);
        config.playbackFinishedGo = callbackGameObject;
        config.playbackFinishedMethod = callbackMethod;
        PagOverlayManager manager = getManager(instanceKey);
        if (manager != null) {
            manager.setPlaybackFinishedCallback(callbackGameObject, callbackMethod);
        }
        Log.i(TAG, "SetPlaybackFinishedCallback: instance=" + instanceKey
                + ", " + callbackGameObject + "." + callbackMethod);
    }

    public static void BindGpuTexture(int textureId, int width, int height) {
        BindGpuTexture(DEFAULT_INSTANCE, textureId, width, height);
    }

    public static void BindGpuTexture(String instanceKey, int textureId, int width, int height) {
        final String key = normalizeKey(instanceKey);
        runOnUi(() -> {
            if (ensureManager(key)) {
                getManager(key).bindGpuTexture(textureId, width, height);
            }
        });
    }

    /** 同步组：阻塞直到 UI 线程完成 bindGpuTexture。 */
    public static boolean BindGpuTextureSync(String instanceKey, int textureId, int width, int height) {
        final String key = normalizeKey(instanceKey);
        final AtomicBoolean ok = new AtomicBoolean(false);
        boolean completed = runOnUiSync("BindGpuTextureSync instance=" + key, () -> {
            if (ensureManager(key)) {
                PagOverlayManager manager = getManager(key);
                manager.bindGpuTexture(textureId, width, height);
                ok.set(manager.getGpuTextureWidth() > 0 && manager.getGpuTextureHeight() > 0);
            }
        });
        if (!completed || !ok.get()) {
            Log.e(TAG, "BindGpuTextureSync failed: instance=" + key
                    + " completed=" + completed + " ok=" + ok.get());
        }
        return completed && ok.get();
    }

    public static boolean IsFguiGpuSurfaceReady(String instanceKey) {
        PagOverlayManager manager = getManager(instanceKey);
        return manager != null && manager.isFguiGpuSurfaceReadyForReuse();
    }

    public static void StartFguiGpuPlayback() {
        StartFguiGpuPlayback(DEFAULT_INSTANCE);
    }

    public static void StartFguiGpuPlayback(String instanceKey) {
        final String key = normalizeKey(instanceKey);
        runOnUi(() -> {
            PagOverlayManager manager = getManager(key);
            if (manager != null) {
                manager.startFguiGpuPlaybackFromUnity();
            }
        });
    }

    /** P0：Unity GPU 预热完成后开始播放墙钟。 */
    public static void ArmFguiGpuPlaybackClock() {
        ArmFguiGpuPlaybackClock(DEFAULT_INSTANCE);
    }

    public static void ArmFguiGpuPlaybackClock(String instanceKey) {
        final String key = normalizeKey(instanceKey);
        runOnUi(() -> {
            PagOverlayManager manager = getManager(key);
            if (manager != null) {
                manager.armFguiGpuPlaybackClock();
            }
        });
    }

    /** 同步组：阻塞直到 UI 线程完成 startFguiGpuPlayback 且 player 就绪。 */
    public static boolean StartFguiGpuPlaybackSync(String instanceKey) {
        final String key = normalizeKey(instanceKey);
        final AtomicBoolean ok = new AtomicBoolean(false);
        boolean completed = runOnUiSync("StartFguiGpuPlaybackSync instance=" + key, () -> {
            PagOverlayManager manager = getManager(key);
            if (manager != null) {
                manager.startFguiGpuPlaybackFromUnity();
                ok.set(manager.isFguiGpuPlayerReady());
            }
        });
        if (!completed || !ok.get()) {
            Log.e(TAG, "StartFguiGpuPlaybackSync failed: instance=" + key
                    + " completed=" + completed + " ok=" + ok.get());
        }
        return completed && ok.get();
    }

    /** Phase4E：登记 Native 播放列表（同尺寸纹理模式）；Play 首段前调用。 */
    public static void SetFguiGpuPlaylist(String instanceKey, String[] paths, int[] repeats) {
        final String key = normalizeKey(instanceKey);
        runOnUiSync("SetFguiGpuPlaylist instance=" + key, () -> {
            PagOverlayManager manager = getManager(key);
            if (manager != null) {
                manager.setFguiGpuPlaylist(paths, repeats);
            }
        });
    }

    public static void ClearFguiGpuPlaylist(String instanceKey) {
        final String key = normalizeKey(instanceKey);
        runOnUiSync("ClearFguiGpuPlaylist instance=" + key, () -> {
            PagOverlayManager manager = getManager(key);
            if (manager != null) {
                manager.clearFguiGpuPlaylist();
            }
        });
    }

    /** Phase4E：打断循环段并无缝切到下一段。 */
    public static void AdvanceFguiGpuPlaylist(String instanceKey) {
        final String key = normalizeKey(instanceKey);
        runOnUi(() -> {
            PagOverlayManager manager = getManager(key);
            if (manager != null) {
                manager.advanceFguiGpuPlaylist();
            }
        });
    }

    public static boolean IsFguiGpuPlaylistActive(String instanceKey) {
        PagOverlayManager manager = getManager(normalizeKey(instanceKey));
        return manager != null && manager.isFguiGpuPlaylistActive();
    }

    public static boolean IsFguiGpuPlaybackActive(String instanceKey) {
        PagOverlayManager manager = getManager(normalizeKey(instanceKey));
        return manager != null && manager.isFguiGpuPlaybackActive();
    }

    /** 当前 GPU 段播放进度 0~1；-1 表示不可用。 */
    public static float GetFguiGpuPlaybackProgress(String instanceKey) {
        PagOverlayManager manager = getManager(normalizeKey(instanceKey));
        return manager != null ? manager.getFguiGpuPlaybackProgress() : -1f;
    }

    /** Phase2 A'：playlist 段切期间 Unity 侧是否应跳过 FGUI present。 */
    public static boolean ShouldDeferFguiGpuPresent(String instanceKey) {
        PagOverlayManager manager = getManager(normalizeKey(instanceKey));
        return manager != null && manager.shouldDeferFguiGpuPresent();
    }

    public static void SetFguiGpuExternalPump(String instanceKey, boolean externalPump) {
        final String key = normalizeKey(instanceKey);
        runOnUi(() -> {
            PagOverlayManager manager = getManager(key);
            if (manager != null) {
                manager.setFguiGpuExternalPump(externalPump);
            }
        });
    }

    public static int GetGpuTextureWidth() {
        return GetGpuTextureWidth(DEFAULT_INSTANCE);
    }

    public static int GetGpuTextureWidth(String instanceKey) {
        PagOverlayManager manager = getManager(instanceKey);
        return manager != null ? manager.getGpuTextureWidth() : 0;
    }

    public static int GetGpuTextureHeight() {
        return GetGpuTextureHeight(DEFAULT_INSTANCE);
    }

    public static int GetGpuTextureHeight(String instanceKey) {
        PagOverlayManager manager = getManager(instanceKey);
        return manager != null ? manager.getGpuTextureHeight() : 0;
    }

    public static int GetCompositionWidth() {
        return GetCompositionWidth(DEFAULT_INSTANCE);
    }

    public static int GetCompositionWidth(String instanceKey) {
        PagOverlayManager manager = getManager(instanceKey);
        return manager != null ? manager.getCompositionWidth() : 0;
    }

    public static int GetCompositionHeight() {
        return GetCompositionHeight(DEFAULT_INSTANCE);
    }

    public static int GetCompositionHeight(String instanceKey) {
        PagOverlayManager manager = getManager(instanceKey);
        return manager != null ? manager.getCompositionHeight() : 0;
    }

    public static long GetCompositionDurationUs() {
        return GetCompositionDurationUs(DEFAULT_INSTANCE);
    }

    public static long GetCompositionDurationUs(String instanceKey) {
        PagOverlayManager manager = getManager(instanceKey);
        return manager != null ? manager.getCompositionDurationUs() : 0L;
    }

    public static float GetCompositionFrameRate() {
        return GetCompositionFrameRate(DEFAULT_INSTANCE);
    }

    public static float GetCompositionFrameRate(String instanceKey) {
        PagOverlayManager manager = getManager(instanceKey);
        return manager != null ? manager.getCompositionFrameRate() : 0f;
    }

    /** 由 Unity 渲染线程（PagUnityGlBridge JNI）调用，勿改名。 */
    public static boolean nativeSetupGpuSurfaceOnRenderThread(int textureId, int width, int height) {
        return nativeSetupGpuSurfaceOnRenderThread(textureId, width, height, DEFAULT_INSTANCE);
    }

    public static boolean nativeSetupGpuSurfaceOnRenderThread(int textureId, int width, int height, String instanceKey) {
        PagOverlayManager manager = getManager(instanceKey);
        if (manager == null) {
            Log.e(TAG, "nativeSetupGpuSurfaceOnRenderThread: manager null, instance=" + instanceKey);
            return false;
        }
        return manager.setupGpuSurfaceOnRenderThread(textureId, width, height);
    }

    /** 由 Unity 渲染线程（PagUnityGlBridge JNI）调用，勿改名。 */
    public static boolean nativeFlushGpuFrameOnRenderThread(double progress) {
        return nativeFlushGpuFrameOnRenderThread(progress, DEFAULT_INSTANCE);
    }

    public static boolean nativeFlushGpuFrameOnRenderThread(double progress, String instanceKey) {
        PagOverlayManager manager = getManager(instanceKey);
        if (manager == null) {
            Log.e(TAG, "nativeFlushGpuFrameOnRenderThread: manager null, instance=" + instanceKey);
            return false;
        }
        return manager.flushGpuFrameOnRenderThread(progress);
    }

    /** Phase3c：段末 GL 线程在同 FBO 内 chain 并 flush frame0；由 PagUnityGlBridge 调用，勿改名。 */
    public static boolean nativeTryChainAndFlushFrame0OnRenderThread(double finishedProgress) {
        return nativeTryChainAndFlushFrame0OnRenderThread(finishedProgress, DEFAULT_INSTANCE);
    }

    public static boolean nativeTryChainAndFlushFrame0OnRenderThread(double finishedProgress, String instanceKey) {
        PagOverlayManager manager = getManager(instanceKey);
        if (manager == null) {
            Log.e(TAG, "nativeTryChainAndFlushFrame0OnRenderThread: manager null, instance=" + instanceKey);
            return false;
        }
        return manager.tryChainPendingAndFlushFrame0OnRenderThread(finishedProgress);
    }

    /** Phase3 P0：GL tryChain/flush + glFinish 完成后由渲染线程回调，触发 Unity present。 */
    public static void nativeNotifyGpuFlushPresentReady(String instanceKey, boolean segmentChained) {
        final String key = normalizeKey(instanceKey);
        PagOverlayManager manager = getManager(key);
        if (manager != null) {
            manager.notifyGpuFlushPresentReady(segmentChained);
        }
    }

    /** 由 Unity 渲染线程 DestroyTexture 前调用，勿改名。 */
    public static boolean nativeTeardownGpuSurfaceOnRenderThread() {
        return nativeTeardownGpuSurfaceOnRenderThread(DEFAULT_INSTANCE);
    }

    public static boolean nativeTeardownGpuSurfaceOnRenderThread(String instanceKey) {
        PagOverlayManager manager = getManager(instanceKey);
        if (manager == null) {
            Log.e(TAG, "nativeTeardownGpuSurfaceOnRenderThread: manager null, instance=" + instanceKey);
            return false;
        }
        return manager.teardownGpuSurfaceOnRenderThread();
    }

    /** Unity GL flush 完成后回写 native 播放状态。 */
    public static void OnGpuFlushCompleted() {
        OnGpuFlushCompleted(DEFAULT_INSTANCE);
    }

    public static void OnGpuFlushCompleted(String instanceKey) {
        final String key = normalizeKey(instanceKey);
        Runnable action = () -> {
            PagOverlayManager manager = getManager(key);
            if (manager != null) {
                manager.onGpuFrameFlushedAfterPresent();
            }
        };
        if (Looper.myLooper() == Looper.getMainLooper()) {
            action.run();
        } else {
            sMainHandler.post(action);
        }
    }

    /** Unity 主线程在 present 后请求下一帧 PAG flush；已在主 Looper 则直调，否则 post 到主 Looper（避免 runOnUiThread 额外排队）。 */
    public static void RequestNextGpuFrame() {
        RequestNextGpuFrame(DEFAULT_INSTANCE);
    }

    public static void RequestNextGpuFrame(String instanceKey) {
        final String key = normalizeKey(instanceKey);
        Runnable action = () -> {
            PagOverlayManager manager = getManager(key);
            if (manager != null) {
                manager.requestNextGpuFrame();
            }
        };
        if (Looper.myLooper() == Looper.getMainLooper()) {
            action.run();
        } else {
            sMainHandler.post(action);
        }
    }

    /** SyncGroup 批量要帧：单次 JNI 调用，主 Looper 内顺序 requestNextGpuFrame。 */
    public static void RequestNextGpuFrameBatch(String[] instanceKeys) {
        if (instanceKeys == null || instanceKeys.length == 0) {
            return;
        }
        Runnable action = () -> {
            for (String instanceKey : instanceKeys) {
                if (instanceKey == null) {
                    continue;
                }
                String key = normalizeKey(instanceKey);
                PagOverlayManager manager = getManager(key);
                if (manager != null) {
                    manager.requestNextGpuFrame();
                }
            }
        };
        if (Looper.myLooper() == Looper.getMainLooper()) {
            action.run();
        } else {
            sMainHandler.post(action);
        }
    }

    public static void ReplaceText(int index, String text) {
        ReplaceText(DEFAULT_INSTANCE, index, text);
    }

    public static void ReplaceText(String instanceKey, int index, String text) {
        final String key = normalizeKey(instanceKey);
        runOnUi(() -> {
            if (ensureManager(key)) {
                getManager(key).replaceText(index, text);
            }
        });
    }

    public static void ReplaceImage(int index, String imagePath) {
        ReplaceImage(DEFAULT_INSTANCE, index, imagePath);
    }

    public static void ReplaceImage(String instanceKey, int index, String imagePath) {
        final String key = normalizeKey(instanceKey);
        runOnUi(() -> {
            if (ensureManager(key)) {
                getManager(key).replaceImage(index, imagePath);
            }
        });
    }

    public static void PlayInterval(String path, long startTimeUs, long durationUs,
                                     String positionType, String extra) {
        PlayInterval(DEFAULT_INSTANCE, path, startTimeUs, durationUs, positionType, extra);
    }

    public static void PlayInterval(String instanceKey, String path, long startTimeUs, long durationUs,
                                     String positionType, String extra) {
        final String key = normalizeKey(instanceKey);
        runOnUi(() -> {
            if (!ensureManager(key)) {
                return;
            }
            getManager(key).playInterval(path, startTimeUs, durationUs, positionType, extra);
        });
    }

    public static void PlayMultiPag(String basePath, int count, int colNum,
                                    String positionType, String extra) {
        PlayMultiPag(DEFAULT_INSTANCE, basePath, count, colNum, positionType, extra);
    }

    public static void PlayMultiPag(String instanceKey, String basePath, int count, int colNum,
                                    String positionType, String extra) {
        final String key = normalizeKey(instanceKey);
        runOnUi(() -> {
            if (!ensureManager(key)) {
                return;
            }
            getManager(key).playMultiPag(basePath, count, colNum, positionType, extra);
        });
    }

    public static void ExportVideo(String pagPath, String outputName,
                                   String callbackGameObject, String callbackMethod) {
        ExportVideo(DEFAULT_INSTANCE, pagPath, outputName, callbackGameObject, callbackMethod);
    }

    public static void ExportVideo(String instanceKey, String pagPath, String outputName,
                                  String callbackGameObject, String callbackMethod) {
        final String key = normalizeKey(instanceKey);
        runOnUi(() -> {
            if (!ensureManager(key)) {
                return;
            }
            getManager(key).exportVideo(pagPath, outputName, callbackGameObject, callbackMethod);
        });
    }

    /** 全局预加载 PAG composition 到 LRU 缓存（任意 instanceKey 共享）。 */
    public static void PreloadComposition(String path) {
        if (path == null || path.isEmpty()) {
            Log.w(TAG, "PreloadComposition: empty path");
            return;
        }
        Log.i(TAG, "PreloadComposition: " + path);
        PagCompositionCache.preloadAsync(path, PagBridge::loadPagFileStatic);
    }

    public static boolean IsCompositionCached(String path) {
        return path != null && !path.isEmpty() && PagCompositionCache.contains(path);
    }

    /** 切游戏时清空全局 PAGFile LRU，把坑位留给下一局 Loading 预热。 */
    public static void EvictCompositionCache() {
        Log.i(TAG, "EvictCompositionCache");
        PagCompositionCache.evictAll();
    }

    /**
     * C# Dispose 且 GPU teardown 完成后调用：stop 并移除 instance 的 Manager/Config，
     * 避免 sManagers 在 Unity 侧 Dispose 后仍残留。
     */
    public static void ReleaseInstance(String instanceKey) {
        final String key = normalizeKey(instanceKey);
        runOnUiSync("ReleaseInstance instance=" + key, () -> {
            PagOverlayManager manager = sManagers.remove(key);
            if (manager != null) {
                Log.i(TAG, "ReleaseInstance: stop manager for " + key);
                manager.stop();
            } else {
                Log.i(TAG, "ReleaseInstance: manager already absent for " + key);
            }
            sConfigs.remove(key);
            Log.i(TAG, "ReleaseInstance done, managers=" + sManagers.size());
        });
    }

    private static PAGFile loadPagFileStatic(String path) {
        if (path == null || path.isEmpty()) {
            return null;
        }
        if (path.startsWith("assets://")) {
            if (sActivity == null) {
                Log.e(TAG, "loadPagFileStatic: activity null for assets path");
                return null;
            }
            String assetPath = path.substring("assets://".length());
            return PAGFile.Load(sActivity.getAssets(), assetPath);
        }
        File file = new File(path);
        if (!file.exists()) {
            Log.e(TAG, "loadPagFileStatic: file missing " + path);
            return null;
        }
        return PAGFile.Load(path);
    }

    private static void runOnUi(Runnable action) {
        if (sActivity == null) {
            Log.e(TAG, "runOnUi: activity is null, call Init first");
            return;
        }
        sActivity.runOnUiThread(action);
    }

    private static boolean runOnUiSync(String tag, Runnable action) {
        if (sActivity == null) {
            Log.e(TAG, tag + ": activity is null, call Init first");
            return false;
        }
        final CountDownLatch latch = new CountDownLatch(1);
        sActivity.runOnUiThread(() -> {
            try {
                action.run();
            } catch (Exception e) {
                Log.e(TAG, tag + " exception: " + e.getMessage());
            } finally {
                latch.countDown();
            }
        });
        try {
            if (!latch.await(UI_SYNC_TIMEOUT_MS, TimeUnit.MILLISECONDS)) {
                Log.e(TAG, tag + " timeout after " + UI_SYNC_TIMEOUT_MS + "ms");
                return false;
            }
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
            Log.e(TAG, tag + " interrupted");
            return false;
        }
        return true;
    }
}
