package com.lftlive.com.pag;

import android.app.Activity;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Matrix;
import android.graphics.Rect;
import android.media.MediaCodec;
import android.media.MediaCodecInfo;
import android.media.MediaFormat;
import android.media.MediaMuxer;
import android.os.Environment;
import android.os.Handler;
import android.os.HandlerThread;
import android.os.Looper;
import android.graphics.Color;
import android.graphics.SurfaceTexture;
import android.util.DisplayMetrics;
import android.util.Log;
import android.view.Gravity;
import android.view.SurfaceView;
import android.view.View;
import android.view.ViewGroup;
import android.view.ViewTreeObserver;
import android.view.WindowManager;
import android.widget.FrameLayout;
import android.widget.ImageView;
import android.widget.Toast;

import com.unity3d.player.UnityPlayer;

import org.libpag.PAGComposition;
import org.libpag.PAGFile;
import org.libpag.PAGLayer;
import org.libpag.PAGImage;
import org.libpag.PAGPlayer;
import org.libpag.PAGSurface;
import org.libpag.PAGText;
import org.libpag.PAGTimeStretchMode;
import org.libpag.PAGView;

import java.io.File;
import java.io.IOException;
import java.nio.ByteBuffer;
import java.util.ArrayList;

/**
 * 在 Unity Activity 上层管理 PAGView overlay，实现 pagDemo 中的核心能力。
 */
final class PagOverlayManager {
    private static final String TAG = "PagOverlayManager";
    /** 下调 Unity SurfaceView Z-order，避免盖住 PAGView（TextureView）。 */
    private static final boolean TUNE_UNITY_SURFACE_Z_ORDER = true;
    /** Phase0 红底：overlay 露出诊断；Phase1 通过后关闭。 */
    private static final boolean DEBUG_OVERLAY_RED_BG = false;
    /** Phase1 诊断：pagView 区域半透明蓝底，区分占位与 libpag 出帧。 */
    private static final boolean DEBUG_PAGVIEW_BLUE_BG = false;
    /** DecorView overlay 仍不可见时，改用 WindowManager 独立浮层（模拟器仍黑屏时可改为 true A/B）。 */
    private static final boolean USE_WM_OVERLAY_FALLBACK = true;
    /** Phase2：Surface 就绪轮询间隔与上限（约 2s）。 */
    private static final int SURFACE_READY_POLL_MS = 32;
    private static final int SURFACE_READY_MAX_ATTEMPTS = 60;
    /** Phase2：模拟器上透明 TextureView 可能不出帧，默认 true。 */
    private static final boolean PAG_VIEW_OPAQUE = true;
    /** 与 Unity 同屏冲突时可改为 false。 */
    private static final boolean PAG_VIEW_HARDWARE_LAYER = false;
    /** Phase3C：全程走离屏 PAGSurface + ImageView（默认关；真机优先 TextureView）。 */
    private static final boolean USE_BITMAP_OVERLAY_FALLBACK = false;
    /** Phase4B：Surface 轮询超时且仍不可用时自动软件出帧（模拟器 TextureView 不就绪时）。 */
    private static final boolean AUTO_BITMAP_ON_SURFACE_TIMEOUT = true;
    private static final int BITMAP_FALLBACK_FPS = 30;
    private static final long BITMAP_FALLBACK_FRAME_MS = 1000L / BITMAP_FALLBACK_FPS;
    static final int RENDER_MODE_OVERLAY = 0;
    /** Unity 仍传 1；内部映射为纹理模式。 */
    static final int RENDER_MODE_FGUI_TEXTURE = 1;
    static final int RENDER_MODE_FGUI_GPU = 2;
    /** 0 = 不缩放，使用 PAG 合成原始像素尺寸出帧。 */
    private static final int DEFAULT_FGUI_MAX_DISPLAY_SIDE = 0;
    private static final String MIME_TYPE = "video/avc";
    private static final int FRAME_RATE = 30;
    private static final int IFRAME_INTERVAL = 10;
    private static final int BIT_RATE = 8000000;
    private static final int TIMEOUT_USEC = 10000 * 60 / FRAME_RATE;

    private final Activity activity;
    private final Handler mainHandler = new Handler(Looper.getMainLooper());

    private FrameLayout overlayRoot;
    private View debugBackdropView;
    private PAGView pagView;
    private PAGFile pagFile;

    private FrameLayout wmOverlayRoot;
    private WindowManager.LayoutParams wmLayoutParams;
    private boolean wmOverlayAttached;

    private boolean isPlaying = true;
    private String positionType = "center";
    private String layoutPlace = "center";
    private float rightAdaptiveW = 1f;
    private float rightAdaptiveH = 1f;
    private int repeatCount = 0;

    private HandlerThread exportThread;
    private Handler exportHandler;

    private PAGView.PAGViewListener pagViewListener;
    private int pagListenerUpdateLogCount;
    private String pendingPlaybackCaller;

    private ImageView bitmapFallbackImageView;
    private PAGPlayer bitmapFallbackPlayer;
    private PAGSurface bitmapFallbackSurface;
    private Bitmap bitmapFallbackBitmap;
    private Runnable bitmapFallbackTickRunnable;
    private long bitmapFallbackStartMs;
    private boolean bitmapFallbackActive;

    private String playStartedCallbackGo;
    private String playStartedCallbackMethod;
    private String currentPlayPath;
    private String pendingPlayExtra;
    private int loadGeneration;
    private long bitmapFallbackFrameMs = BITMAP_FALLBACK_FRAME_MS;

    private int renderMode = RENDER_MODE_OVERLAY;
    private int fguiMaxDisplaySide = DEFAULT_FGUI_MAX_DISPLAY_SIDE;
    private String gpuTextureRequestGo;
    private String gpuTextureRequestMethod;
    private String gpuRenderCallbackGo;
    private String gpuRenderCallbackMethod;
    private String playbackFinishedCallbackGo;
    private String playbackFinishedCallbackMethod;

    private int gpuTextureId;
    private int gpuTexW;
    private int gpuTexH;
    private PAGPlayer fguiGpuPlayer;
    private PAGSurface fguiGpuSurface;
    private Runnable fguiGpuTickRunnable;
    private boolean fguiGpuActive;
    private boolean fguiGpuSurfaceReady;
    private volatile long fguiGpuPlaybackStartMs;
    /** P0：Surface/预热完成前不计播放墙钟，避免首帧 progress 超前跳帧。 */
    private volatile boolean fguiGpuPlaybackClockArmed;
    private volatile double fguiGpuPendingProgress;
    private boolean fguiGpuPlayStartedNotified;
    private int fguiGpuInfiniteLoopCount;
    private long fguiGpuLastCompletedLoops;
    private int gpuPlayerRecycleEveryLoop;
    /** Phase4B：PagGpuSyncGroup 由 C# 统一节流要帧，Java 不自动 schedule tick。 */
    private volatile boolean fguiGpuExternalPump;
    /** Phase3e：UI 线程在 request progress=1.0 时 arm，GL chain 消费快照避免跨线程读 pending 为 null。 */
    private volatile GlChainSnapshot glChainSnapshotArmed;
    /** Phase3f：arm/take 快照与 GL chain 段末投递同步。 */
    private final Object _glChainLock = new Object();
    /** Phase4C：GPU tick 状态机。 */
    private enum FguiGpuTickPhase {
        STOPPED,
        PLAYING,
        /** progress=1.0 已发出且 playlist 仍有下一段；等 GL tryChain */
        SEGMENT_END_FLUSHING,
        /** GL tryChain 完成，deliver 待主线程投递 */
        CHAIN_DELIVER_PENDING
    }

    private volatile FguiGpuTickPhase fguiGpuTickPhase = FguiGpuTickPhase.STOPPED;
    /** Phase3k：composition 切换后下一次 flush 前先 clearAll，避免旧段残留 alpha 叠加。 */
    private volatile boolean fguiGpuClearBeforeNextFlush;
    /** Phase2 A'：playlist 段切期间延迟 FGUI UpdateExternalTexture，保留上一段末帧直至 frame0 写入。 */
    private volatile boolean fguiGpuDeferFguiPresent;

    private static final float DEFAULT_COMPOSITION_FRAME_RATE = 30f;
    /** Phase3j：段末 armed 刷新阈值，覆盖 0.984 等末帧采样。 */
    private static final double GL_CHAIN_ARM_PROGRESS_THRESHOLD = 0.98;

    private static final class FguiGpuProgressSnapshot {
        double progress;
        long frameInLoop;
        long totalFrames;
        long completedLoops;
    }

    /** UI 线程段末 flush 前拷贝的 pending 段，供 GL tryChainRenderThread 消费。 */
    private static final class GlChainSnapshot {
        String path;
        int repeat;
        PAGFile pagFile;
    }

    /** Phase4E：Native 播放列表条目（path/repeat/preload）。 */
    private static final class FguiGpuPlaylistEntry {
        String path;
        int repeat;
        PAGFile pagFile;
    }

    /** Phase4E：单槽内多段无缝连播；段切换由 Native tryChain 完成，C# 仅等整链结束。 */
    private final ArrayList<FguiGpuPlaylistEntry> fguiGpuPlaylist = new ArrayList<>();
    private int fguiGpuPlaylistIndex = -1;
    private volatile boolean fguiGpuPlaylistActive;

    private static final String UNITY_CALLBACK_HUB = "PagCallbackHub";
    private static final String UNITY_SYNC_FLUSH_FRAME0_METHOD = "OnPagGpuSyncFlushFrame0";
    private static final String UNITY_FLUSH_PRESENT_READY_METHOD = "OnPagGpuFlushPresentReady";
    private static final char UNITY_PAYLOAD_SEP = '\u001f';

    private String unityInstanceKey = "";

    /** Phase4D：由 PagBridge.SetForceBitmapOverlayFallback 从 C# 设置，跳过 TextureView 等待。 */
    private static boolean sForceBitmapOverlayFallback;

    static void setForceBitmapOverlayFallback(boolean force) {
        sForceBitmapOverlayFallback = force;
    }

    private static boolean useBitmapOverlayAtPlayStart() {
        return USE_BITMAP_OVERLAY_FALLBACK || sForceBitmapOverlayFallback;
    }

    PagOverlayManager(Activity activity) {
        this.activity = activity;
    }

    void setUnityInstanceKey(String key) {
        unityInstanceKey = key == null ? "" : key;
        Log.i(TAG, "setUnityInstanceKey: " + unityInstanceKey);
    }

    private String buildHubMessage(String payload) {
        String body = payload == null ? "" : payload;
        if (unityInstanceKey == null || unityInstanceKey.isEmpty()) {
            return body;
        }
        return unityInstanceKey + UNITY_PAYLOAD_SEP + body;
    }

    private void sendToUnityHub(String method, String payload) {
        if (method == null || method.isEmpty()) {
            return;
        }
        String message = buildHubMessage(payload);
        Log.i(TAG, "sendToUnityHub: " + method + " instance=" + unityInstanceKey + " payload=" + payload);
        UnityPlayer.UnitySendMessage(UNITY_CALLBACK_HUB, method, message);
    }

    void setRenderTarget(int mode) {
        renderMode = (mode == RENDER_MODE_FGUI_TEXTURE || mode == RENDER_MODE_FGUI_GPU)
                ? RENDER_MODE_FGUI_GPU : RENDER_MODE_OVERLAY;
        Log.i(TAG, "setRenderTarget: " + (renderMode == RENDER_MODE_FGUI_GPU
                ? "FguiGpu" : "Overlay"));
    }

    void setFguiFrameConfig(int maxDisplaySide, int fps) {
        // maxDisplaySide == 0：不限制，resolveFguiTextureDimensions 使用合成原尺寸
        fguiMaxDisplaySide = Math.max(0, maxDisplaySide);
        if (fps > 0) {
            bitmapFallbackFrameMs = 1000L / fps;
        }
        Log.i(TAG, "setFguiFrameConfig: maxSide=" + fguiMaxDisplaySide + " fps="
                + (bitmapFallbackFrameMs > 0 ? (1000L / bitmapFallbackFrameMs) : BITMAP_FALLBACK_FPS));
    }

    void setGpuTextureRequestCallback(String callbackGo, String callbackMethod) {
        gpuTextureRequestGo = callbackGo;
        gpuTextureRequestMethod = callbackMethod;
        Log.i(TAG, "setGpuTextureRequestCallback: " + callbackGo + "." + callbackMethod);
    }

    void setGpuRenderCallback(String callbackGo, String callbackMethod) {
        gpuRenderCallbackGo = callbackGo;
        gpuRenderCallbackMethod = callbackMethod;
        Log.i(TAG, "setGpuRenderCallback: " + callbackGo + "." + callbackMethod);
    }

    void setPlaybackFinishedCallback(String callbackGo, String callbackMethod) {
        playbackFinishedCallbackGo = callbackGo;
        playbackFinishedCallbackMethod = callbackMethod;
        Log.i(TAG, "setPlaybackFinishedCallback: " + callbackGo + "." + callbackMethod);
    }

    void bindGpuTexture(int textureId, int width, int height) {
        gpuTextureId = textureId;
        gpuTexW = width;
        gpuTexH = height;
        Log.i(TAG, "bindGpuTexture: id=" + textureId + " size=" + width + "x" + height);
    }

    /** Sync JNI：Start 之后、GL Setup 之前校验 player 与纹理已就绪。 */
    boolean isFguiGpuPlayerReady() {
        return fguiGpuPlayer != null && gpuTextureId > 0 && gpuTexW > 0 && gpuTexH > 0;
    }

    int getGpuTextureWidth() {
        return gpuTexW;
    }

    int getGpuTextureHeight() {
        return gpuTexH;
    }

    long getCompositionDurationUs() {
        return pagFile != null ? pagFile.duration() : 0L;
    }

    float getCompositionFrameRate() {
        if (pagFile == null) {
            return 0f;
        }
        float frameRate = pagFile.frameRate();
        return frameRate > 0f ? frameRate : 0f;
    }

    /** PAG 合成原尺寸（未应用 maxDisplaySide 缩放）。 */
    int getCompositionWidth() {
        return pagFile != null ? pagFile.width() : 0;
    }

    int getCompositionHeight() {
        return pagFile != null ? pagFile.height() : 0;
    }

    void startFguiGpuPlaybackFromUnity() {
        if (Looper.myLooper() == mainHandler.getLooper()) {
            startFguiGpuPlayback("unityBind");
        } else {
            mainHandler.post(() -> startFguiGpuPlayback("unityBind"));
        }
    }

    void setRightAdaptive(float w, float h) {
        rightAdaptiveW = w;
        rightAdaptiveH = h;
    }

    void layoutPagAuto(String place) {
        layoutPlace = place == null ? "center" : place;
        if (overlayRoot != null && overlayRoot.getVisibility() == View.VISIBLE) {
            mainHandler.post(() -> applyLayout(positionType, null));
        }
    }

    /**
     * 放弃 PAGFile 引用。libpag 4.4.x 无 PAGFile.release()，native 内存随引用断开由 GC 回收。
     * 调用方须同步清空 pagFile 字段或确保 loaded 局部变量不再被持有。
     */
    private static void releasePagFileSafe(PAGFile file) {
        if (file == null) {
            return;
        }
        // libpag 无显式 release API；此处仅表达「主动丢弃」语义，实际释放依赖 GC。
    }

    private void releaseCurrentPagFile() {
        releasePagFileSafe(pagFile);
        pagFile = null;
    }

    /** libpag 4.4.x 无 PAGFile/PAGComposition.release()，断开 View 引用后由 GC 回收 native 对象。 */
    private void releasePagViewComposition() {
        if (pagView == null) {
            return;
        }
        if (pagView.getComposition() == null) {
            return;
        }
        pagView.setComposition(null);
        pagView.freeCache();
    }

    /** 切换新 PAG 前释放上一份 composition/bitmap，避免 Dragon+UFO 双份 native 内存叠加。 */
    private void prepareForNewPlay() {
        stopFguiGpuPlayback();
        stopBitmapFallback();
        showPagViewAfterBitmapFallback();
        if (pagView != null) {
            pagView.stop();
            releasePagViewComposition();
        }
        releaseCurrentPagFile();
        fguiGpuInfiniteLoopCount = 0;
        fguiGpuLastCompletedLoops = 0L;
        isPlaying = false;
        pendingPlaybackCaller = null;
        resetFguiGpuChainState();
    }

    void play(String path, String position, String extra) {
        play(path, position, extra, null, null);
    }

    void play(String path, String position, String extra, String callbackGo, String callbackMethod) {
        if (path == null || path.isEmpty()) {
            Log.e(TAG, "play: empty path");
            return;
        }
        positionType = position == null ? "center" : position;
        File pagFileOnDisk = new File(path);
        Log.i(TAG, "play: path=" + path + ", exists=" + pagFileOnDisk.exists()
                + ", size=" + (pagFileOnDisk.exists() ? pagFileOnDisk.length() : 0)
                + ", position=" + positionType + ", layoutPlace=" + layoutPlace);
        final String go = callbackGo;
        final String method = callbackMethod;
        final String pathFinal = path;
        final String extraFinal = extra;
        final int loadGen = ++loadGeneration;
        mainHandler.post(() -> {
            playStartedCallbackGo = go;
            playStartedCallbackMethod = method;
            prepareForNewPlay();
            currentPlayPath = pathFinal;
            pendingPlayExtra = extraFinal;
            if (renderMode == RENDER_MODE_FGUI_GPU) {
                hideOverlay();
                return;
            }
            ensureOverlay();
            if (pagView == null) {
                Log.e(TAG, "play: pagView null before async load, path=" + pathFinal);
                clearPlayStartedCallback();
                return;
            }
        });
        ensureExportThread();
        exportHandler.post(() -> {
            long t0 = System.currentTimeMillis();
            boolean cacheHit = PagCompositionCache.contains(pathFinal);
            PAGFile loaded = PagCompositionCache.loadOrGetCached(pathFinal, this::loadPagFile);
            long elapsedMs = System.currentTimeMillis() - t0;
            Log.i(TAG, "play: load cache " + (cacheHit ? "HIT" : "MISS")
                    + " path=" + pathFinal + " elapsedMs=" + elapsedMs);
            mainHandler.post(() -> finishPlayAfterLoad(loadGen, pathFinal, loaded));
        });
    }

    void preloadComposition(String path) {
        if (path == null || path.isEmpty()) {
            Log.w(TAG, "preloadComposition: empty path");
            return;
        }
        PagCompositionCache.preloadAsync(path, this::loadPagFile);
    }

    private void finishPlayAfterLoad(int loadGen, String path, PAGFile loaded) {
        if (loadGen != loadGeneration) {
            Log.i(TAG, "play: stale load discarded, gen=" + loadGen + " current=" + loadGeneration
                    + " path=" + path);
            releasePagFileSafe(loaded);
            return;
        }
        if (path == null || !path.equals(currentPlayPath)) {
            Log.i(TAG, "play: stale load discarded, path mismatch expected=" + currentPlayPath);
            releasePagFileSafe(loaded);
            return;
        }
        pagFile = loaded;
        if (pagFile == null) {
            Log.e(TAG, "play: load failed, path=" + path);
            clearPlayStartedCallback();
            return;
        }
        Log.i(TAG, "play: loaded ok, size=" + pagFile.width() + "x" + pagFile.height()
                + ", durationUs=" + pagFile.duration() + " renderMode=" + renderMode);

        if (renderMode == RENDER_MODE_FGUI_GPU) {
            requestGpuTextureBind();
            return;
        }

        if (pagView == null) {
            Log.e(TAG, "play: pagView null after load, path=" + path);
            clearPlayStartedCallback();
            return;
        }
        pagView.setComposition(pagFile);
        pagView.setRepeatCount(repeatCount);
        applyLayoutFromComposition(positionType, pendingPlayExtra);
        bringOverlayToFront();
        applyDebugOverlayBackground();
        showOverlay();
        if (useBitmapOverlayAtPlayStart()) {
            stopBitmapFallback();
            hidePagViewForBitmapFallback();
            startBitmapFallback("play");
            Log.i(TAG, "play: bitmap fallback started, wmOverlay=" + wmOverlayAttached
                    + " forceBitmap=" + sForceBitmapOverlayFallback);
        } else {
            showPagViewAfterBitmapFallback();
            recreatePagViewIfNeeded("play:afterShowOverlay");
            schedulePagPlaybackAfterLayout("play");
            Log.i(TAG, "play: started, overlay visible, wmOverlay=" + wmOverlayAttached);
        }
    }

    void stop() {
        loadGeneration++;
        final boolean deferGpuSurfaceTeardown = renderMode == RENDER_MODE_FGUI_GPU
                && (fguiGpuPlayer != null || fguiGpuSurface != null);
        Runnable body = () -> {
            if (deferGpuSurfaceTeardown) {
                stopFguiGpuTickScheduling();
                resetFguiGpuChainState();
            } else {
                stopFguiGpuPlayback();
            }
            gpuTextureId = 0;
            gpuTexW = 0;
            gpuTexH = 0;
            stopBitmapFallback();
            showPagViewAfterBitmapFallback();
            if (pagView != null) {
                pagView.stop();
            }
            hideOverlay();
            removeWmOverlay();
            releaseCurrentPagFile();
            isPlaying = false;
            pendingPlaybackCaller = null;
            currentPlayPath = null;
            clearFguiGpuPlaylist();
            clearPlayStartedCallback();
        };
        if (Looper.myLooper() == Looper.getMainLooper()) {
            body.run();
        } else {
            mainHandler.post(body);
        }
    }

    private void clearPlayStartedCallback() {
        playStartedCallbackGo = null;
        playStartedCallbackMethod = null;
    }

    private void notifyPlayStarted() {
        if (playStartedCallbackGo == null || playStartedCallbackGo.isEmpty()
                || playStartedCallbackMethod == null || playStartedCallbackMethod.isEmpty()) {
            return;
        }
        String go = playStartedCallbackGo;
        String method = playStartedCallbackMethod;
        clearPlayStartedCallback();
        String pathHint = currentPlayPath != null ? currentPlayPath : "(unknown)";
        Log.i(TAG, "notifyPlayStarted: " + go + "." + method + " path=" + pathHint);
        sendToUnityHub(method, "");
    }

    void pause() {
        mainHandler.post(() -> {
            if (pagView != null && isPlaying) {
                pagView.pause();
                isPlaying = false;
            }
        });
    }

    void resume() {
        mainHandler.post(() -> {
            if (pagView != null && !isPlaying) {
                pagView.play();
                isPlaying = true;
            }
        });
    }

    void setRepeatCount(int count) {
        repeatCount = count;
        fguiGpuInfiniteLoopCount = 0;
        fguiGpuLastCompletedLoops = 0L;
        mainHandler.post(() -> {
            if (pagView != null) {
                pagView.setRepeatCount(count);
            }
        });
    }

    void setGpuPlayerRecycleEveryLoop(int everyLoop) {
        gpuPlayerRecycleEveryLoop = Math.max(0, everyLoop);
        fguiGpuInfiniteLoopCount = 0;
        fguiGpuLastCompletedLoops = 0L;
    }

    void replaceText(int index, String text) {
        mainHandler.post(() -> {
            if (pagFile == null || pagFile.numTexts() <= index) {
                return;
            }
            PAGText textData = pagFile.getTextData(index);
            textData.text = text == null ? "" : text;
            pagFile.replaceText(index, textData);
            if (pagView != null) {
                pagView.setComposition(pagFile);
                if (isPlaying) {
                    pagView.play();
                }
            }
        });
    }

    void replaceImage(int index, String imagePath) {
        if (imagePath == null || imagePath.isEmpty()) {
            return;
        }
        mainHandler.post(() -> {
            if (pagFile == null || pagFile.numImages() <= index) {
                return;
            }
            Bitmap bitmap = BitmapFactory.decodeFile(imagePath);
            if (bitmap == null) {
                Log.e(TAG, "replaceImage: decode failed, path=" + imagePath);
                return;
            }
            PAGImage image = PAGImage.FromBitmap(bitmap);
            pagFile.replaceImage(index, image);
            if (pagView != null) {
                pagView.setComposition(pagFile);
                if (isPlaying) {
                    pagView.play();
                }
            }
        });
    }

    void playInterval(String path, long startTimeUs, long durationUs, String position, String extra) {
        if (path == null || path.isEmpty()) {
            return;
        }
        positionType = position == null ? "center" : position;
        mainHandler.post(() -> {
            ensureOverlay();
            pagFile = loadPagFile(path);
            if (pagFile == null) {
                return;
            }
            pagFile.setTimeStretchMode(PAGTimeStretchMode.None);
            pagFile.setStartTime(startTimeUs);
            pagFile.setDuration(durationUs);

            PAGComposition composition = PAGComposition.Make(pagFile.width(), pagFile.height());
            composition.addLayer(pagFile);

            pagView.setComposition(composition);
            pagView.setRepeatCount(repeatCount);
            applyLayout(positionType, extra);
            bringOverlayToFront();
            applyDebugOverlayBackground();
            showOverlay();
            schedulePagPlaybackAfterLayout("playInterval");
            isPlaying = true;
        });
    }

    void playMultiPag(String basePath, int count, int colNum, String position, String extra) {
        if (basePath == null || basePath.isEmpty() || count <= 0) {
            return;
        }
        positionType = position == null ? "full" : position;
        int columns = colNum > 0 ? colNum : 4;
        float itemHeight = 400f;

        mainHandler.post(() -> {
            ensureOverlay();
            releasePagViewComposition();
            WindowManager manager = activity.getWindowManager();
            DisplayMetrics metrics = new DisplayMetrics();
            manager.getDefaultDisplay().getMetrics(metrics);
            int width = metrics.widthPixels;
            int height = metrics.heightPixels;

            PAGComposition composition = PAGComposition.Make(width, height);
            float itemWidth = width * 1f / columns;

            for (int i = 0; i < count; i++) {
                int row = i / columns;
                int col = i % columns;
                String path = basePath + i + ".pag";
                PAGFile file = loadPagFile(path);
                if (file == null) {
                    continue;
                }
                Matrix matrix = new Matrix();
                float scaleX = itemWidth / file.width();
                matrix.preScale(scaleX, scaleX);
                matrix.postTranslate(itemWidth * col, row * itemHeight);
                file.setMatrix(matrix);
                file.setDuration(10000000);
                composition.addLayer(file);
            }

            pagFile = null;
            pagView.setComposition(composition);
            pagView.setRepeatCount(repeatCount);
            applyLayout(positionType, extra);
            bringOverlayToFront();
            applyDebugOverlayBackground();
            showOverlay();
            schedulePagPlaybackAfterLayout("playMultiPag");
            isPlaying = true;
        });
    }

    void exportVideo(String pagPath, String outputName, String callbackGo, String callbackMethod) {
        ensureExportThread();
        exportHandler.post(() -> {
            String resultPath = "";
            try {
                resultPath = encodePagToMp4(pagPath, outputName);
            } catch (Exception e) {
                Log.e(TAG, "exportVideo failed: " + e.getMessage());
            }
            final String path = resultPath;
            mainHandler.post(() -> {
                if (path != null && !path.isEmpty()) {
                    Toast.makeText(activity, "PAG export finished", Toast.LENGTH_SHORT).show();
                }
                if (callbackMethod != null && !callbackMethod.isEmpty()) {
                    sendToUnityHub(callbackMethod, path == null ? "" : path);
                }
            });
        });
    }

    private void ensureOverlay() {
        if (USE_WM_OVERLAY_FALLBACK) {
            ensureWmOverlay();
            return;
        }
        if (overlayRoot != null) {
            bringOverlayToFront();
            bringPagViewToFront();
            scheduleTuneUnitySurfaceZOrder();
            return;
        }

        ViewGroup decor = (ViewGroup) activity.getWindow().getDecorView();
        scheduleTuneUnitySurfaceZOrder();

        overlayRoot = new FrameLayout(activity);
        overlayRoot.setLayoutParams(new FrameLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT,
                ViewGroup.LayoutParams.MATCH_PARENT));
        overlayRoot.setClickable(false);
        overlayRoot.setFocusable(false);
        overlayRoot.setBackgroundColor(Color.TRANSPARENT);

        debugBackdropView = new View(activity);
        overlayRoot.addView(debugBackdropView, new FrameLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT,
                ViewGroup.LayoutParams.MATCH_PARENT));

        pagView = new PAGView(activity);
        configurePagView(pagView);
        attachPagViewListener(pagView);
        overlayRoot.addView(pagView, new FrameLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT,
                ViewGroup.LayoutParams.MATCH_PARENT));

        decor.addView(overlayRoot);
        overlayRoot.setVisibility(View.GONE);
        bringOverlayToFront();
        bringPagViewToFront();
        applyDebugOverlayBackground();
        scheduleTuneUnitySurfaceZOrder();
        Log.i(TAG, "ensureOverlay: decor overlay created");
    }

    private void ensureWmOverlay() {
        if (wmOverlayAttached && pagView != null) {
            return;
        }
        removeWmOverlay();

        WindowManager wm = activity.getWindowManager();
        wmOverlayRoot = new FrameLayout(activity);
        wmOverlayRoot.setBackgroundColor(Color.TRANSPARENT);

        pagView = new PAGView(activity);
        configurePagView(pagView);
        attachPagViewListener(pagView);
        wmOverlayRoot.addView(pagView, new FrameLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT,
                ViewGroup.LayoutParams.MATCH_PARENT));

        wmLayoutParams = new WindowManager.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT,
                ViewGroup.LayoutParams.MATCH_PARENT,
                WindowManager.LayoutParams.TYPE_APPLICATION_PANEL,
                WindowManager.LayoutParams.FLAG_NOT_FOCUSABLE
                        | WindowManager.LayoutParams.FLAG_NOT_TOUCH_MODAL
                        | WindowManager.LayoutParams.FLAG_LAYOUT_IN_SCREEN,
                android.graphics.PixelFormat.TRANSLUCENT);
        wmLayoutParams.gravity = Gravity.TOP | Gravity.START;
        wmLayoutParams.token = activity.getWindow().getDecorView().getWindowToken();

        wm.addView(wmOverlayRoot, wmLayoutParams);
        wmOverlayAttached = true;
        overlayRoot = wmOverlayRoot;
        overlayRoot.setVisibility(View.GONE);
        scheduleTuneUnitySurfaceZOrder();
        Log.i(TAG, "ensureWmOverlay: WindowManager overlay attached, wmOverlay=true");
    }

    private void removeWmOverlay() {
        if (!wmOverlayAttached || wmOverlayRoot == null) {
            return;
        }
        FrameLayout root = wmOverlayRoot;
        try {
            activity.getWindowManager().removeView(root);
        } catch (Exception e) {
            Log.w(TAG, "removeWmOverlay: " + e.getMessage());
        }
        wmOverlayAttached = false;
        wmLayoutParams = null;
        wmOverlayRoot = null;
        if (overlayRoot == root) {
            overlayRoot = null;
            pagView = null;
            debugBackdropView = null;
        }
        Log.i(TAG, "removeWmOverlay: detached");
    }

    private void configurePagView(PAGView view) {
        view.setOpaque(PAG_VIEW_OPAQUE);
        view.setVisibility(View.VISIBLE);
        if (PAG_VIEW_HARDWARE_LAYER) {
            view.setLayerType(View.LAYER_TYPE_HARDWARE, null);
        } else {
            view.setLayerType(View.LAYER_TYPE_NONE, null);
        }
        Log.i(TAG, "configurePagView: opaque=" + PAG_VIEW_OPAQUE
                + " hardwareLayer=" + PAG_VIEW_HARDWARE_LAYER
                + " (PAGView owns SurfaceTextureListener, do not override)");
    }

    private boolean isPagSurfaceReady() {
        if (pagView == null) {
            return false;
        }
        SurfaceTexture st = pagView.getSurfaceTexture();
        return pagView.getWidth() > 0 && pagView.getHeight() > 0
                && (pagView.isAvailable() || st != null);
    }

    private void recreatePagViewIfNeeded(String reason) {
        if (pagView == null || overlayRoot == null) {
            return;
        }
        if (overlayRoot.getVisibility() != View.VISIBLE) {
            return;
        }
        if (pagView.isAvailable()) {
            return;
        }
        Log.i(TAG, "recreatePagView: reason=" + reason);
        PAGComposition composition = pagView.getComposition();
        FrameLayout.LayoutParams oldParams = null;
        ViewGroup.LayoutParams lp = pagView.getLayoutParams();
        if (lp instanceof FrameLayout.LayoutParams) {
            oldParams = new FrameLayout.LayoutParams((FrameLayout.LayoutParams) lp);
        }
        ViewGroup parent = (ViewGroup) pagView.getParent();
        if (parent == null) {
            parent = overlayRoot;
        }
        if (parent != null) {
            parent.removeView(pagView);
        }
        pagView = new PAGView(activity);
        configurePagView(pagView);
        attachPagViewListener(pagView);
        if (parent != null) {
            if (oldParams != null) {
                parent.addView(pagView, oldParams);
            } else {
                parent.addView(pagView, new FrameLayout.LayoutParams(
                        ViewGroup.LayoutParams.MATCH_PARENT,
                        ViewGroup.LayoutParams.MATCH_PARENT));
            }
        }
        if (composition != null) {
            pagView.setComposition(composition);
        } else if (pagFile != null) {
            pagView.setComposition(pagFile);
        }
        pagView.setRepeatCount(repeatCount);
        pagView.requestLayout();
        overlayRoot.requestLayout();
        logPagViewState("recreatePagView:after");
    }

    private void attachPagViewListener(PAGView view) {
        if (view == null) {
            return;
        }
        if (pagViewListener != null) {
            view.removeListener(pagViewListener);
        }
        pagListenerUpdateLogCount = 0;
        pagViewListener = new PAGView.PAGViewListener() {
            @Override
            public void onAnimationStart(PAGView pagView) {
                Log.i(TAG, "pagListener: onAnimationStart");
            }

            @Override
            public void onAnimationEnd(PAGView pagView) {
                Log.i(TAG, "pagListener: onAnimationEnd");
            }

            @Override
            public void onAnimationCancel(PAGView pagView) {
                Log.i(TAG, "pagListener: onAnimationCancel");
            }

            @Override
            public void onAnimationRepeat(PAGView pagView) {
                Log.i(TAG, "pagListener: onAnimationRepeat");
            }

            @Override
            public void onAnimationUpdate(PAGView pagView) {
                if (pagListenerUpdateLogCount < 3) {
                    pagListenerUpdateLogCount++;
                    Log.i(TAG, "pagListener: onAnimationUpdate #" + pagListenerUpdateLogCount);
                }
            }
        };
        view.addListener(pagViewListener);
        Log.i(TAG, "attachPagViewListener: registered");
    }

    private void showOverlay() {
        if (overlayRoot != null) {
            overlayRoot.setVisibility(View.VISIBLE);
        }
    }

    private void hideOverlay() {
        if (overlayRoot != null) {
            overlayRoot.setVisibility(View.GONE);
        }
    }

    private void bringPagViewToFront() {
        if (pagView != null) {
            pagView.bringToFront();
        }
    }

    private void schedulePagPlaybackAfterLayout(String caller) {
        if (pagView == null) {
            return;
        }
        pendingPlaybackCaller = caller;
        pagView.requestLayout();
        if (overlayRoot != null) {
            overlayRoot.requestLayout();
            ViewTreeObserver observer = overlayRoot.getViewTreeObserver();
            if (observer.isAlive()) {
                observer.addOnGlobalLayoutListener(new ViewTreeObserver.OnGlobalLayoutListener() {
                    @Override
                    public void onGlobalLayout() {
                        ViewTreeObserver o = overlayRoot.getViewTreeObserver();
                        if (o.isAlive()) {
                            o.removeOnGlobalLayoutListener(this);
                        }
                        logPagViewState(caller + ":afterGlobalLayout");
                        pagView.post(() -> startPagPlaybackWhenSurfaceReady(caller, 0));
                    }
                });
                return;
            }
        }
        pagView.post(() -> startPagPlaybackWhenSurfaceReady(caller, 0));
    }

    private void startPagPlaybackWhenSurfaceReady(String caller, int attempt) {
        if (pagView == null) {
            return;
        }
        logPagViewState(caller + ":wait#" + attempt);
        if (isPagSurfaceReady()) {
            pagView.play();
            isPlaying = true;
            pendingPlaybackCaller = null;
            Log.i(TAG, "play: surface ready, attempt=" + attempt + ", caller=" + caller);
            notifyPlayStarted();
            return;
        }
        if (attempt >= SURFACE_READY_MAX_ATTEMPTS) {
            Log.e(TAG, "play: surface not ready after timeout, caller=" + caller
                    + ", try onResume/flush/play");
            logPagViewState(caller + ":timeoutBeforeFallback");
            try {
                pagView.onResume();
            } catch (Exception e) {
                Log.w(TAG, "play: onResume failed: " + e.getMessage());
            }
            pagView.flush();
            pagView.play();
            isPlaying = true;
            pendingPlaybackCaller = null;
            logPagViewState(caller + ":timeoutAfterFallback");
            if (pagView.isAvailable()) {
                notifyPlayStarted();
            }
            if (!pagView.isAvailable()) {
                if (useBitmapOverlayAtPlayStart() || AUTO_BITMAP_ON_SURFACE_TIMEOUT) {
                    hidePagViewForBitmapFallback();
                    startBitmapFallback(caller + ":surfaceTimeout");
                    Log.i(TAG, "play: auto bitmap fallback after surface timeout, caller=" + caller);
                } else {
                    Log.w(TAG, "play: still not available after timeout, enable AUTO_BITMAP_ON_SURFACE_TIMEOUT or force bitmap from C#");
                }
            }
            return;
        }
        pagView.postDelayed(
                () -> startPagPlaybackWhenSurfaceReady(caller, attempt + 1),
                SURFACE_READY_POLL_MS);
    }

    private void logPagViewState(String stage) {
        if (pagView == null) {
            return;
        }
        SurfaceTexture st = pagView.getSurfaceTexture();
        Log.i(TAG, "pagViewState[" + stage + "]: w=" + pagView.getWidth()
                + " h=" + pagView.getHeight()
                + " visible=" + (pagView.getVisibility() == View.VISIBLE)
                + " opaque=" + pagView.isOpaque()
                + " isAvailable=" + pagView.isAvailable()
                + " hasSurfaceTexture=" + (st != null)
                + " wmOverlay=" + wmOverlayAttached
                + " bitmapFallback=" + bitmapFallbackActive);
    }

    private void hidePagViewForBitmapFallback() {
        if (pagView != null) {
            pagView.setVisibility(View.GONE);
        }
    }

    private void showPagViewAfterBitmapFallback() {
        if (pagView != null) {
            pagView.setVisibility(View.VISIBLE);
        }
        detachBitmapFallbackImageView();
    }

    /** Clear ImageView drawable before recycle to avoid "recycled bitmap" crash on draw. */
    private void detachBitmapFallbackImageView() {
        if (bitmapFallbackImageView == null) {
            return;
        }
        bitmapFallbackImageView.setImageDrawable(null);
        bitmapFallbackImageView.setVisibility(View.GONE);
        bitmapFallbackImageView.invalidate();
    }

    private int[] resolveFguiTextureDimensionsForFile(PAGFile file) {
        if (file == null) {
            return new int[] {0, 0};
        }
        int w = file.width();
        int h = file.height();
        if (w <= 0 || h <= 0) {
            return new int[] {0, 0};
        }
        int maxSide = Math.max(w, h);
        if (fguiMaxDisplaySide > 0 && maxSide > fguiMaxDisplaySide) {
            float scale = (float) fguiMaxDisplaySide / maxSide;
            w = Math.max(1, Math.round(w * scale));
            h = Math.max(1, Math.round(h * scale));
        }
        return new int[] {w, h};
    }

    private int[] resolveFguiTextureDimensions() {
        int[] size = resolveFguiTextureDimensionsForFile(pagFile);
        if (size[0] <= 0 || size[1] <= 0) {
            return new int[] {200, 200};
        }
        Log.i(TAG, "resolveFguiTextureDimensions: composition=" + pagFile.width() + "x"
                + pagFile.height() + " render=" + size[0] + "x" + size[1]
                + " maxSideCap=" + fguiMaxDisplaySide);
        return size;
    }

    private int[] resolveBitmapFallbackDimensions(int layoutW, int layoutH) {
        if (pagFile != null) {
            int w = pagFile.width();
            int h = pagFile.height();
            if (w > 0 && h > 0) {
                Log.i(TAG, "resolveBitmapFallbackDimensions: compositionNative "
                        + w + "x" + h + " fps=" + BITMAP_FALLBACK_FPS);
                return new int[] {w, h};
            }
        }
        int w = layoutW > 0 ? layoutW : 200;
        int h = layoutH > 0 ? layoutH : 200;
        Log.i(TAG, "resolveBitmapFallbackDimensions: layoutFallback " + w + "x" + h
                + " fps=" + BITMAP_FALLBACK_FPS);
        return new int[] {w, h};
    }

    private void requestGpuTextureBind() {
        if (pagFile == null) {
            Log.e(TAG, "requestGpuTextureBind: pagFile null");
            clearPlayStartedCallback();
            return;
        }
        int[] size = resolveFguiTextureDimensions();
        gpuTexW = size[0];
        gpuTexH = size[1];
        if (gpuTextureRequestGo == null || gpuTextureRequestGo.isEmpty()
                || gpuTextureRequestMethod == null || gpuTextureRequestMethod.isEmpty()) {
            Log.e(TAG, "requestGpuTextureBind: no Unity texture request callback");
            clearPlayStartedCallback();
            return;
        }
        String payload = gpuTexW + "," + gpuTexH;
        Log.i(TAG, "requestGpuTextureBind: " + payload
                + (gpuTextureId > 0 ? " (existingTex=" + gpuTextureId + ")" : ""));
        sendToUnityHub(gpuTextureRequestMethod, payload);
    }

    private void startFguiGpuPlayback(String caller) {
        if (pagFile == null) {
            Log.e(TAG, "startFguiGpuPlayback: pagFile null, caller=" + caller);
            clearPlayStartedCallback();
            return;
        }
        final int texId = gpuTextureId;
        final int texW = gpuTexW;
        final int texH = gpuTexH;
        if (texId <= 0 || texW <= 0 || texH <= 0) {
            Log.e(TAG, "startFguiGpuPlayback: invalid gpu texture id=" + texId
                    + " size=" + texW + "x" + texH);
            clearPlayStartedCallback();
            return;
        }
        releaseFguiGpuPlayerOnly();
        stopBitmapFallback();
        fguiGpuSurfaceReady = false;
        try {
            fguiGpuPlayer = new PAGPlayer();
            fguiGpuPlayer.setComposition(pagFile);
            fguiGpuPlayer.setProgress(0);
        } catch (Exception e) {
            Log.e(TAG, "startFguiGpuPlayback: init failed: " + e.getMessage());
            releaseFguiGpuResources();
            clearPlayStartedCallback();
            return;
        }
        fguiGpuActive = true;
        isPlaying = true;
        setFguiGpuTickPhase(FguiGpuTickPhase.PLAYING);
        fguiGpuPlaybackStartMs = 0L;
        fguiGpuPlaybackClockArmed = false;
        fguiGpuPendingProgress = 0.0;
        fguiGpuPlayStartedNotified = false;
        fguiGpuLastCompletedLoops = 0L;
        long durationUs = resolveCompositionDurationUs();
        long nativeFrames = Math.max(1L, (long) (durationUs * pagFile.frameRate() / 1_000_000L));
        Log.i(TAG, "startFguiGpuPlayback: tex=" + texId + " " + texW + "x" + texH
                + " durationUs=" + durationUs + " nativeFrames=" + nativeFrames
                + " frameRate=" + pagFile.frameRate() + " caller=" + caller
                + " (surface setup deferred to render thread)");
        if (fguiGpuPlaylistActive) {
            syncFguiGpuPlaylistIndexToCurrentPath();
            armGlChainSnapshotFromPlaylistNext();
        }
    }

    boolean setupGpuSurfaceOnRenderThread(int texId, int texW, int texH) {
        if (fguiGpuPlayer == null || pagFile == null) {
            Log.e(TAG, "setupGpuSurfaceOnRenderThread: player null");
            return false;
        }
        if (texId <= 0 || texW <= 0 || texH <= 0) {
            Log.e(TAG, "setupGpuSurfaceOnRenderThread: invalid tex=" + texId
                    + " size=" + texW + "x" + texH);
            return false;
        }
        if (fguiGpuSurface != null) {
            fguiGpuPlayer.setSurface(null);
            fguiGpuSurface.release();
            fguiGpuSurface = null;
        }
        fguiGpuSurfaceReady = false;
        try {
            PAGSurface.SetupFromTexture(texId, texW, texH, true, true);
            fguiGpuSurface = PAGSurface.FromTexture(texId, texW, texH, true);
            if (fguiGpuSurface == null) {
                Log.e(TAG, "setupGpuSurfaceOnRenderThread: FromTexture null");
                return false;
            }
            fguiGpuPlayer.setSurface(fguiGpuSurface);
            fguiGpuSurfaceReady = true;
            Log.i(TAG, "setupGpuSurfaceOnRenderThread: tex=" + texId + " " + texW + "x" + texH
                    + " (tick deferred to Unity warmup/arm)");
            return true;
        } catch (Exception e) {
            Log.e(TAG, "setupGpuSurfaceOnRenderThread failed: " + e.getMessage());
            return false;
        }
    }

    boolean flushGpuFrameOnRenderThread(double progress) {
        if (!fguiGpuSurfaceReady || fguiGpuPlayer == null || pagFile == null) {
            Log.e(TAG, "flushGpuFrameOnRenderThread: not ready surfaceReady="
                    + fguiGpuSurfaceReady);
            return false;
        }
        try {
            if (fguiGpuClearBeforeNextFlush) {
                fguiGpuClearBeforeNextFlush = false;
                clearFguiGpuSurfaceOnRenderThread();
            }
            fguiGpuPlayer.setProgress(progress);
            fguiGpuPlayer.flush();
            return true;
        } catch (Exception e) {
            Log.e(TAG, "flushGpuFrameOnRenderThread failed: " + e.getMessage());
            return false;
        }
    }

    /** Phase3k：渲染线程清空 FBO，避免段切换时旧帧 alpha 残留。 */
    private boolean clearFguiGpuSurfaceOnRenderThread() {
        if (!fguiGpuSurfaceReady || fguiGpuSurface == null) {
            return false;
        }
        try {
            fguiGpuSurface.clearAll();
            Log.i(TAG, "clearFguiGpuSurfaceOnRenderThread: ok path=" + currentPlayPath);
            return true;
        } catch (Exception e) {
            Log.w(TAG, "clearFguiGpuSurfaceOnRenderThread failed: " + e.getMessage());
            return false;
        }
    }

    /** Stop/DestroyTexture 前在渲染线程 detach Surface 并释放 PAGPlayer，避免 EGL_BAD_ACCESS。 */
    boolean teardownGpuSurfaceOnRenderThread() {
        try {
            if (fguiGpuPlayer != null) {
                fguiGpuPlayer.setSurface(null);
            }
            if (fguiGpuSurface != null) {
                fguiGpuSurface.release();
                fguiGpuSurface = null;
            }
            fguiGpuSurfaceReady = false;
            if (fguiGpuPlayer != null) {
                fguiGpuPlayer.release();
                fguiGpuPlayer = null;
            }
            fguiGpuActive = false;
            Log.i(TAG, "teardownGpuSurfaceOnRenderThread: ok path=" + currentPlayPath);
            return true;
        } catch (Exception e) {
            Log.w(TAG, "teardownGpuSurfaceOnRenderThread failed: " + e.getMessage());
            return false;
        }
    }

    /** Unity HandleGpuFrameReady 在 GL 同步后回调；Java schedule 下一 tick。 */
    void onGpuFrameFlushedAfterPresent() {
        if (!fguiGpuActive || fguiGpuPlayer == null || pagFile == null) {
            return;
        }
        onGpuFrameFlushed();
    }

    void setFguiGpuExternalPump(boolean externalPump) {
        fguiGpuExternalPump = externalPump;
        Log.i(TAG, "setFguiGpuExternalPump: " + externalPump + " path=" + currentPlayPath);
    }

    /** SyncGroup 外部泵：C# 节流后调用，等价于立即 tick 一次。 */
    void requestNextGpuFrame() {
        if (!fguiGpuActive || !fguiGpuSurfaceReady || fguiGpuPlayer == null || pagFile == null) {
            return;
        }
        if (isChainPhaseBlockingTick()) {
            return;
        }
        requestGpuRenderFrame();
    }

    private boolean hasChainedSegmentPending() {
        if (fguiGpuPlaylistActive && hasNextInFguiGpuPlaylist()) {
            return true;
        }
        synchronized (_glChainLock) {
            return glChainSnapshotArmed != null;
        }
    }

    private boolean isChainPhaseBlockingTick() {
        return fguiGpuTickPhase == FguiGpuTickPhase.SEGMENT_END_FLUSHING
                || fguiGpuTickPhase == FguiGpuTickPhase.CHAIN_DELIVER_PENDING;
    }

    private void setFguiGpuTickPhase(FguiGpuTickPhase phase) {
        if (fguiGpuTickPhase != phase) {
            Log.d(TAG, "fguiGpuTickPhase: " + fguiGpuTickPhase + " -> " + phase
                    + " path=" + currentPlayPath);
        }
        fguiGpuTickPhase = phase;
    }

    private void finishFinalSegment() {
        if (fguiGpuPlaylistActive) {
            finishFguiGpuSequence();
            return;
        }
        Log.i(TAG, "finalSegmentPlaybackFinished path=" + currentPlayPath);
        notifyPlaybackFinished();
        stopFguiGpuTickScheduling();
    }

    private void finishFguiGpuSequence() {
        Log.i(TAG, "fguiGpuSequenceFinished path=" + currentPlayPath
                + " segments=" + fguiGpuPlaylist.size());
        clearFguiGpuPlaylist();
        notifyPlaybackFinished();
        stopFguiGpuTickScheduling();
    }

    /** P0：Unity GPU 预热完成后调用，开始墙钟并允许 tick 调度。 */
    void armFguiGpuPlaybackClock() {
        fguiGpuPlaybackStartMs = System.currentTimeMillis();
        fguiGpuPlaybackClockArmed = true;
        Log.i(TAG, "armFguiGpuPlaybackClock: path=" + currentPlayPath);
    }

    private void onGpuFrameFlushed() {
        if (!fguiGpuActive || fguiGpuPlayer == null || pagFile == null) {
            return;
        }
        if (!fguiGpuPlaybackClockArmed) {
            return;
        }
        if (!fguiGpuPlayStartedNotified) {
            fguiGpuPlayStartedNotified = true;
            notifyPlayStarted();
        }
        if (isChainPhaseBlockingTick()) {
            return;
        }
        boolean isSegmentEndProgress = fguiGpuPendingProgress >= 0.999;
        if (repeatCount < 0 && isSegmentEndProgress) {
            scheduleFguiGpuTickAfterDelay(resolveFrameIntervalMs());
            return;
        }
        if (isSegmentEndProgress) {
            if (hasChainedSegmentPending()) {
                return;
            }
            finishFinalSegment();
            return;
        }
        scheduleFguiGpuTickAfterDelay(resolveFrameIntervalMs());
    }

    /** GL tryChain 失败时 UI 线程 fallback */
    private void onGpuFrameFlushedAfterGlSegmentTryChain() {
        if (!fguiGpuActive || fguiGpuPlayer == null || pagFile == null) {
            return;
        }
        if (fguiGpuTickPhase != FguiGpuTickPhase.SEGMENT_END_FLUSHING) {
            return;
        }
        if (fguiGpuPendingProgress < 0.999) {
            return;
        }
        if (repeatCount < 0) {
            setFguiGpuTickPhase(FguiGpuTickPhase.PLAYING);
            return;
        }
        boolean chained = tryChainPendingFguiGpuCompositionSwitch();
        if (chained) {
            if (!fguiGpuPlaylistActive) {
                notifyPlaybackFinished();
            } else {
                syncFguiGpuPlaylistIndexToCurrentPath();
                armGlChainSnapshotFromPlaylistNext();
            }
            setFguiGpuTickPhase(FguiGpuTickPhase.PLAYING);
            fguiGpuActive = true;
            requestGpuSyncFlushFrame0AfterUiFallback();
        } else {
            Log.w(TAG, "onGpuFrameFlushedAfterGlTryChain: chain failed path=" + currentPlayPath
                    + " progress=" + fguiGpuPendingProgress);
            finishFinalSegment();
        }
    }

    /** UI fallback 已切 composition；经 Unity GL 队列同步 flush frame0 再上屏。 */
    private void requestGpuSyncFlushFrame0AfterUiFallback() {
        fguiGpuDeferFguiPresent = false;
        fguiGpuClearBeforeNextFlush = false;
        armFguiGpuPlaybackFrameIndex(1);
        FguiGpuProgressSnapshot snap = snapshotFguiGpuProgress();
        fguiGpuPendingProgress = snap.progress;
        Log.i(TAG, "requestGpuSyncFlushFrame0AfterUiFallback: path=" + currentPlayPath
                + " nextProgress=" + snap.progress);
        sendToUnityHub(UNITY_SYNC_FLUSH_FRAME0_METHOD, "");
    }

    private void resetFguiGpuChainState() {
        synchronized (_glChainLock) {
            glChainSnapshotArmed = null;
        }
        if (fguiGpuActive && fguiGpuTickPhase != FguiGpuTickPhase.STOPPED) {
            setFguiGpuTickPhase(FguiGpuTickPhase.PLAYING);
        }
        fguiGpuClearBeforeNextFlush = false;
        fguiGpuDeferFguiPresent = false;
    }

    boolean shouldDeferFguiGpuPresent() {
        return fguiGpuDeferFguiPresent;
    }

    /** Phase3 P0：GL flush + glFinish 完成后通知 Unity present；段切成功时续 deliver。 */
    void notifyGpuFlushPresentReady(boolean segmentChained) {
        Runnable action = () -> {
            fguiGpuDeferFguiPresent = false;
            sendToUnityHub(UNITY_FLUSH_PRESENT_READY_METHOD, "");
            if (segmentChained) {
                deliverGpuSegmentEndAfterRenderChain();
            }
        };
        if (Looper.myLooper() == Looper.getMainLooper()) {
            action.run();
        } else {
            mainHandler.post(action);
        }
    }

    /** GL tryChain 成功后在主线程投递段末并续 tick（Phase4E-B：直接 PLAYING，无 CHAIN_SKIP）。 */
    private void deliverGpuSegmentEndAfterRenderChain() {
        if (!fguiGpuActive || fguiGpuPlayer == null || pagFile == null) {
            setFguiGpuTickPhase(FguiGpuTickPhase.PLAYING);
            return;
        }
        if (fguiGpuTickPhase != FguiGpuTickPhase.CHAIN_DELIVER_PENDING) {
            return;
        }
        setFguiGpuTickPhase(FguiGpuTickPhase.PLAYING);
        Log.i(TAG, "deliverGpuSegmentEndAfterRenderChain: path=" + currentPlayPath);
        if (!fguiGpuPlaylistActive) {
            notifyPlaybackFinished();
        } else {
            syncFguiGpuPlaylistIndexToCurrentPath();
            armGlChainSnapshotFromPlaylistNext();
            Log.i(TAG, "deliverGpuSegmentEndAfterRenderChain: playlist internal chain index="
                    + fguiGpuPlaylistIndex);
        }
        FguiGpuProgressSnapshot snap = snapshotFguiGpuProgress();
        fguiGpuPendingProgress = snap.progress;
        Log.i(TAG, "deliverGpuSegmentEndAfterRenderChain: resume tick"
                + " nextProgress=" + snap.progress + " frameInLoop=" + snap.frameInLoop
                + "/" + snap.totalFrames);
        fguiGpuActive = true;
        scheduleFguiGpuTickAfterDelay(resolveFrameIntervalMs());
    }

    /** Phase4E：登记 Native 播放列表（须同尺寸）；Play 首段前调用。 */
    void setFguiGpuPlaylist(String[] paths, int[] repeats) {
        clearFguiGpuPlaylist();
        if (paths == null || repeats == null || paths.length == 0 || paths.length != repeats.length) {
            Log.e(TAG, "setFguiGpuPlaylist: invalid args paths="
                    + (paths == null ? "null" : paths.length)
                    + " repeats=" + (repeats == null ? "null" : repeats.length));
            return;
        }
        int refW = 0;
        int refH = 0;
        for (int i = 0; i < paths.length; i++) {
            String path = paths[i];
            if (path == null || path.isEmpty()) {
                Log.e(TAG, "setFguiGpuPlaylist: empty path at index=" + i);
                clearFguiGpuPlaylist();
                return;
            }
            PAGFile file = PagCompositionCache.loadOrGetCached(path, this::loadPagFile);
            if (file == null) {
                Log.e(TAG, "setFguiGpuPlaylist: preload failed path=" + path);
                clearFguiGpuPlaylist();
                return;
            }
            int[] size = resolveFguiTextureDimensionsForFile(file);
            if (size[0] <= 0 || size[1] <= 0) {
                Log.e(TAG, "setFguiGpuPlaylist: invalid size path=" + path);
                clearFguiGpuPlaylist();
                return;
            }
            if (i == 0) {
                refW = size[0];
                refH = size[1];
            } else if (size[0] != refW || size[1] != refH) {
                Log.e(TAG, "setFguiGpuPlaylist: size mismatch path=" + path
                        + " render=" + size[0] + "x" + size[1]
                        + " expected=" + refW + "x" + refH);
                clearFguiGpuPlaylist();
                return;
            }
            FguiGpuPlaylistEntry entry = new FguiGpuPlaylistEntry();
            entry.path = path;
            entry.repeat = repeats[i];
            entry.pagFile = file;
            fguiGpuPlaylist.add(entry);
        }
        fguiGpuPlaylistActive = true;
        fguiGpuPlaylistIndex = 0;
        armGlChainSnapshotFromPlaylistNext();
        Log.i(TAG, "setFguiGpuPlaylist: count=" + paths.length + " first=" + paths[0]);
    }

    void clearFguiGpuPlaylist() {
        fguiGpuPlaylist.clear();
        fguiGpuPlaylistIndex = -1;
        fguiGpuPlaylistActive = false;
        fguiGpuDeferFguiPresent = false;
    }

    /** Phase4E：打断当前循环段并无缝切到下一段（用法 3）。 */
    void advanceFguiGpuPlaylist() {
        if (!fguiGpuPlaylistActive) {
            Log.w(TAG, "advanceFguiGpuPlaylist: playlist inactive");
            return;
        }
        if (!hasNextInFguiGpuPlaylist()) {
            Log.w(TAG, "advanceFguiGpuPlaylist: no next segment index=" + fguiGpuPlaylistIndex);
            return;
        }
        int nextIdx = fguiGpuPlaylistIndex + 1;
        FguiGpuPlaylistEntry next = fguiGpuPlaylist.get(nextIdx);
        GlChainSnapshot snap = new GlChainSnapshot();
        snap.path = next.path;
        snap.repeat = next.repeat;
        snap.pagFile = next.pagFile;
        if (!applyPendingChainCompositionFromSnapshot(snap, false)) {
            Log.e(TAG, "advanceFguiGpuPlaylist: apply failed path=" + next.path);
            return;
        }
        fguiGpuPlaylistIndex = nextIdx;
        armFguiGpuPlaybackFrameIndex(1);
        armGlChainSnapshotFromPlaylistNext();
        setFguiGpuTickPhase(FguiGpuTickPhase.PLAYING);
        fguiGpuActive = true;
        isPlaying = true;
        requestGpuRenderFrameAtProgress(0.0);
        Log.i(TAG, "advanceFguiGpuPlaylist: index=" + nextIdx + " path=" + next.path);
    }

    boolean isFguiGpuPlaylistActive() {
        return fguiGpuPlaylistActive;
    }

    private boolean hasNextInFguiGpuPlaylist() {
        return fguiGpuPlaylistActive
                && fguiGpuPlaylistIndex >= 0
                && fguiGpuPlaylistIndex + 1 < fguiGpuPlaylist.size();
    }

    private void armGlChainSnapshotFromPlaylistNext() {
        if (!hasNextInFguiGpuPlaylist()) {
            synchronized (_glChainLock) {
                glChainSnapshotArmed = null;
            }
            return;
        }
        armGlChainSnapshotFromPlaylistEntry(fguiGpuPlaylistIndex + 1);
    }

    private void armGlChainSnapshotFromPlaylistEntry(int index) {
        synchronized (_glChainLock) {
            if (!fguiGpuPlaylistActive || index < 0 || index >= fguiGpuPlaylist.size()) {
                glChainSnapshotArmed = null;
                return;
            }
            FguiGpuPlaylistEntry entry = fguiGpuPlaylist.get(index);
            GlChainSnapshot snap = new GlChainSnapshot();
            snap.path = entry.path;
            snap.repeat = entry.repeat;
            snap.pagFile = entry.pagFile;
            glChainSnapshotArmed = snap;
        }
    }

    private void syncFguiGpuPlaylistIndexToCurrentPath() {
        if (!fguiGpuPlaylistActive || currentPlayPath == null) {
            return;
        }
        for (int i = 0; i < fguiGpuPlaylist.size(); i++) {
            if (currentPlayPath.equals(fguiGpuPlaylist.get(i).path)) {
                fguiGpuPlaylistIndex = i;
                return;
            }
        }
    }

    /** UI 线程段末 request flush 前 arm，供 GL tryChain 消费。 */
    private void armGlChainSnapshotIfSegmentEnd(double progress) {
        synchronized (_glChainLock) {
            if (progress < GL_CHAIN_ARM_PROGRESS_THRESHOLD) {
                return;
            }
            if (fguiGpuPlaylistActive) {
                armGlChainSnapshotFromPlaylistEntry(fguiGpuPlaylistIndex + 1);
                return;
            }
            glChainSnapshotArmed = null;
        }
    }

    private GlChainSnapshot takeGlChainSnapshotArmed() {
        synchronized (_glChainLock) {
            GlChainSnapshot snap = glChainSnapshotArmed;
            glChainSnapshotArmed = null;
            return snap;
        }
    }

    boolean isFguiGpuPlaybackActive() {
        return fguiGpuActive;
    }

    /**
     * Phase3c：段末 GL 线程在同一次 flush 内 chain 并立即写入 frame0，避免主线程二次往返黑帧。
     */
    boolean tryChainPendingAndFlushFrame0OnRenderThread(double finishedProgress) {
        if (finishedProgress < 0.999) {
            return false;
        }
        boolean chainDeliverPosted = false;
        long t0 = System.currentTimeMillis();
        try {
            GlChainSnapshot armed = takeGlChainSnapshotArmed();
            if (armed == null || armed.path == null || armed.path.isEmpty()) {
                if (fguiGpuPlaylistActive && hasNextInFguiGpuPlaylist()) {
                    synchronized (_glChainLock) {
                        armGlChainSnapshotFromPlaylistEntry(fguiGpuPlaylistIndex + 1);
                    }
                    armed = takeGlChainSnapshotArmed();
                    if (armed != null && armed.path != null && !armed.path.isEmpty()) {
                        Log.i(TAG, "tryChainRenderThread: re-armed snapshot from playlist index="
                                + (fguiGpuPlaylistIndex + 1) + " path=" + armed.path);
                    }
                }
            }
            if (armed == null || armed.path == null || armed.path.isEmpty()) {
                Log.w(TAG, "tryChainRenderThread: skip no armed snapshot");
                return false;
            }
            if (armed.pagFile == null) {
                Log.w(TAG, "tryChainRenderThread: skip not preloaded path=" + armed.path);
                return false;
            }
            int[] newSize = resolveFguiTextureDimensionsForFile(armed.pagFile);
            if (newSize[0] != gpuTexW || newSize[1] != gpuTexH) {
                Log.w(TAG, "tryChainRenderThread: skip size mismatch render=" + newSize[0] + "x" + newSize[1]
                        + " bound=" + gpuTexW + "x" + gpuTexH + " path=" + armed.path);
                return false;
            }
            if (!applyPendingChainCompositionFromSnapshot(armed, false)) {
                Log.w(TAG, "tryChainRenderThread: apply failed path=" + armed.path
                        + " elapsedMs=" + (System.currentTimeMillis() - t0));
                return false;
            }
            fguiGpuClearBeforeNextFlush = false;
            // 同尺寸 playlist：progress=0 flush 全量覆盖，勿 pre-clear（偶发空窗）
            if (!flushGpuFrameOnRenderThread(0.0)) {
                Log.e(TAG, "tryChainRenderThread: flush frame0 failed path=" + currentPlayPath
                        + " elapsedMs=" + (System.currentTimeMillis() - t0));
                return false;
            }
            armFguiGpuPlaybackFrameIndex(1);
            FguiGpuProgressSnapshot snap = snapshotFguiGpuProgress();
            fguiGpuPendingProgress = snap.progress;
            fguiGpuActive = true;
            isPlaying = true;
            if (fguiGpuPlaylistActive) {
                syncFguiGpuPlaylistIndexToCurrentPath();
                armGlChainSnapshotFromPlaylistNext();
            }
            setFguiGpuTickPhase(FguiGpuTickPhase.CHAIN_DELIVER_PENDING);
            Log.i(TAG, "tryChainRenderThread: armed snapshot chained + flushed frame0 path=" + currentPlayPath
                    + " repeat=" + repeatCount
                    + " nextProgress=" + snap.progress + " frameInLoop=" + snap.frameInLoop
                    + "/" + snap.totalFrames
                    + " elapsedMs=" + (System.currentTimeMillis() - t0));
            chainDeliverPosted = true;
            return true;
        } finally {
            if (!chainDeliverPosted) {
                mainHandler.post(this::onGpuFrameFlushedAfterGlSegmentTryChain);
            }
        }
    }

    /** 段末 flush 同线程链式切下一段；UI fallback 用 apply(false)+requestGpuRenderFrame 代替 scheduleTick。 */
    private boolean tryChainPendingFguiGpuCompositionSwitch() {
        GlChainSnapshot armed = takeGlChainSnapshotArmed();
        if (armed != null && applyPendingChainCompositionFromSnapshot(armed, false)) {
            Log.i(TAG, "tryChainPending: armed snapshot chained path=" + currentPlayPath
                    + " repeat=" + repeatCount);
            return true;
        }
        return false;
    }

    /** 从 armed 快照链式切换 playlist 下一段。 */
    private boolean applyPendingChainCompositionFromSnapshot(GlChainSnapshot snap,
                                                             boolean scheduleTickAfterSwitch) {
        if (snap == null || snap.path == null || snap.path.isEmpty()) {
            return false;
        }
        if (renderMode != RENDER_MODE_FGUI_GPU || gpuTextureId <= 0
                || fguiGpuSurface == null || !fguiGpuSurfaceReady) {
            Log.w(TAG, "tryChainPending: gpu not ready, skip chain");
            resetFguiGpuChainState();
            return false;
        }

        PAGFile loaded = snap.pagFile;
        if (loaded == null) {
            loaded = PagCompositionCache.loadOrGetCached(snap.path, this::loadPagFile);
        }
        if (loaded == null) {
            Log.e(TAG, "tryChainPending: load failed path=" + snap.path);
            return false;
        }

        int[] newSize = resolveFguiTextureDimensionsForFile(loaded);
        if (newSize[0] != gpuTexW || newSize[1] != gpuTexH) {
            Log.w(TAG, "tryChainPending: size mismatch render=" + newSize[0] + "x" + newSize[1]
                    + " bound=" + gpuTexW + "x" + gpuTexH + " path=" + snap.path);
            return false;
        }

        resetFguiGpuChainState();
        repeatCount = snap.repeat;
        fguiGpuInfiniteLoopCount = 0;
        fguiGpuLastCompletedLoops = 0L;
        pagFile = loaded;
        currentPlayPath = snap.path;

        applyFguiGpuCompositionSwitchInPlace(scheduleTickAfterSwitch);
        if (!scheduleTickAfterSwitch && !fguiGpuPlaylistActive) {
            fguiGpuClearBeforeNextFlush = true;
        }
        isPlaying = true;
        return true;
    }

    /** tryChain 已 flush frame0 后，将播放时钟对齐到指定帧索引。 */
    private void armFguiGpuPlaybackFrameIndex(int frameIndex) {
        if (frameIndex <= 0) {
            return;
        }
        float frameRate = resolveCompositionFrameRate();
        if (frameRate > 0f) {
            long frameMs = Math.max(1L, (long) Math.ceil(1000.0 / frameRate));
            fguiGpuPlaybackStartMs = System.currentTimeMillis() - frameMs * frameIndex;
            fguiGpuPlaybackClockArmed = true;
        }
    }

    /** frame0 已在 GL 线程 flush 后，将播放时钟推进若干帧以便下次 request 跳过重复 frame0。 */
    private void advanceFguiGpuPlaybackFrames(int frameCount) {
        if (frameCount <= 0) {
            return;
        }
        float frameRate = resolveCompositionFrameRate();
        if (frameRate > 0f) {
            long frameMs = Math.max(1L, (long) (1000.0f / frameRate));
            fguiGpuPlaybackStartMs = System.currentTimeMillis() - frameMs * frameCount;
            fguiGpuPlaybackClockArmed = true;
        }
    }

    private void advanceFguiGpuPlaybackOneFrame() {
        advanceFguiGpuPlaybackFrames(1);
    }

    private long resolveFrameIntervalMs() {
        return Math.max(1L, (long) (1000.0f / resolveCompositionFrameRate()));
    }

    private void cancelFguiGpuTick() {
        if (fguiGpuTickRunnable != null) {
            mainHandler.removeCallbacks(fguiGpuTickRunnable);
            fguiGpuTickRunnable = null;
        }
    }

    private void scheduleFguiGpuTickAfterDelay(long delayMs) {
        if (fguiGpuExternalPump) {
            return;
        }
        if (!fguiGpuActive || !fguiGpuSurfaceReady || fguiGpuPlayer == null || pagFile == null) {
            return;
        }
        cancelFguiGpuTick();
        fguiGpuTickRunnable = () -> {
            fguiGpuTickRunnable = null;
            if (!fguiGpuActive || !fguiGpuSurfaceReady || fguiGpuPlayer == null || pagFile == null) {
                return;
            }
            if (isChainPhaseBlockingTick()) {
                return;
            }
            requestGpuRenderFrame();
        };
        mainHandler.postDelayed(fguiGpuTickRunnable, delayMs);
    }

    private void scheduleFguiGpuTickImmediate() {
        scheduleFguiGpuTickAfterDelay(0L);
    }

    private void scheduleFguiGpuTick() {
        scheduleFguiGpuTickImmediate();
    }

    private void requestGpuRenderFrame() {
        if (!fguiGpuActive || !fguiGpuSurfaceReady || fguiGpuPlayer == null || pagFile == null) {
            return;
        }
        if (gpuRenderCallbackGo == null || gpuRenderCallbackGo.isEmpty()
                || gpuRenderCallbackMethod == null || gpuRenderCallbackMethod.isEmpty()) {
            Log.e(TAG, "requestGpuRenderFrame: no Unity render callback");
            return;
        }
        FguiGpuProgressSnapshot snap = snapshotFguiGpuProgress();
        if (repeatCount < 0) {
            handleInfiniteLoopProgressSnapshot(snap);
            return;
        }
        dispatchGpuRenderFrameRequest(snap.progress, snap.frameInLoop, snap.totalFrames);
    }

    /** UI fallback / advance：指定 progress 立即要帧，playlist 段切勿 defer clear。 */
    private void requestGpuRenderFrameAtProgress(double progress) {
        if (!fguiGpuActive || !fguiGpuSurfaceReady || fguiGpuPlayer == null || pagFile == null) {
            return;
        }
        if (gpuRenderCallbackGo == null || gpuRenderCallbackGo.isEmpty()
                || gpuRenderCallbackMethod == null || gpuRenderCallbackMethod.isEmpty()) {
            Log.e(TAG, "requestGpuRenderFrameAtProgress: no Unity render callback");
            return;
        }
        if (repeatCount < 0) {
            handleInfiniteLoopProgressSnapshot(snapshotFguiGpuProgress());
            return;
        }
        dispatchGpuRenderFrameRequest(progress, 0L, resolveCompositionTotalFrames());
    }

    private void dispatchGpuRenderFrameRequest(double progress, long frameInLoop, long totalFrames) {
        fguiGpuPendingProgress = progress;
        armGlChainSnapshotIfSegmentEnd(progress);
        // Phase3 P1：段末 0.98~0.999 不写纹理，仅等 progress>=0.999 的 tryChain flush。
        if (fguiGpuPlaylistActive && progress >= GL_CHAIN_ARM_PROGRESS_THRESHOLD
                && progress < 0.999 && hasChainedSegmentPending()) {
            Log.d(TAG, "dispatchGpuRenderFrameRequest: skip tail flush progress=" + progress
                    + " frameInLoop=" + frameInLoop + "/" + totalFrames
                    + " path=" + currentPlayPath);
            scheduleFguiGpuTickAfterDelay(resolveFrameIntervalMs());
            return;
        }
        if (progress >= 0.999 && hasChainedSegmentPending()) {
            setFguiGpuTickPhase(FguiGpuTickPhase.SEGMENT_END_FLUSHING);
        } else if (fguiGpuTickPhase == FguiGpuTickPhase.SEGMENT_END_FLUSHING
                || fguiGpuTickPhase == FguiGpuTickPhase.CHAIN_DELIVER_PENDING) {
            setFguiGpuTickPhase(FguiGpuTickPhase.PLAYING);
        }
        if (fguiGpuPlaylistActive && progress >= GL_CHAIN_ARM_PROGRESS_THRESHOLD
                && hasChainedSegmentPending()) {
            fguiGpuDeferFguiPresent = true;
            Log.d(TAG, "defer FGUI present: progress=" + progress
                    + " path=" + currentPlayPath + " phase=" + fguiGpuTickPhase);
        }
        Log.d(TAG, "dispatchGpuRenderFrameRequest: progress=" + progress
                + " frameInLoop=" + frameInLoop + "/" + totalFrames
                + " phase=" + fguiGpuTickPhase
                + " deferPresent=" + fguiGpuDeferFguiPresent);
        sendToUnityHub(gpuRenderCallbackMethod, Double.toString(progress));
    }

    private void handleInfiniteLoopProgressSnapshot(FguiGpuProgressSnapshot snap) {
        if (snap.completedLoops > fguiGpuLastCompletedLoops) {
            fguiGpuLastCompletedLoops = snap.completedLoops;
            Log.i(TAG, "requestGpuRenderFrame: loopBoundary completedLoops=" + snap.completedLoops
                    + " progress=" + snap.progress + " frameInLoop=" + snap.frameInLoop
                    + "/" + snap.totalFrames);
        }
        fguiGpuPendingProgress = snap.progress;
        Log.d(TAG, "requestGpuRenderFrame: progress=" + snap.progress
                + " frameInLoop=" + snap.frameInLoop + "/" + snap.totalFrames);
        sendToUnityHub(gpuRenderCallbackMethod, Double.toString(snap.progress));
    }

    private void notifyPlaybackFinished() {
        if (playbackFinishedCallbackGo == null || playbackFinishedCallbackGo.isEmpty()
                || playbackFinishedCallbackMethod == null
                || playbackFinishedCallbackMethod.isEmpty()) {
            return;
        }
        Log.i(TAG, "notifyPlaybackFinished");
        sendToUnityHub(playbackFinishedCallbackMethod, "");
    }

    private void stopFguiGpuTickScheduling() {
        cancelFguiGpuTick();
        fguiGpuActive = false;
        setFguiGpuTickPhase(FguiGpuTickPhase.STOPPED);
    }

    /** 仅释放 PAGPlayer/Surface 与 tick；保留 Unity 侧 gpuTextureId 绑定。 */
    private void releaseFguiGpuPlayerOnly() {
        stopFguiGpuTickScheduling();
        releaseFguiGpuResources();
        fguiGpuPlaybackStartMs = 0L;
        fguiGpuPlaybackClockArmed = false;
        fguiGpuPendingProgress = 0.0;
        fguiGpuLastCompletedLoops = 0L;
        fguiGpuPlayStartedNotified = false;
        fguiGpuSurfaceReady = false;
        resetFguiGpuChainState();
    }

    /** 同尺寸段切换：优先原地 setComposition，避免 detach Surface 清空共享纹理。 */
    private void applyFguiGpuCompositionSwitchInPlace(boolean scheduleTick) {
        if (pagFile == null) {
            return;
        }
        if (fguiGpuPlayer != null && fguiGpuSurfaceReady && fguiGpuSurface != null) {
            try {
                fguiGpuPlayer.setComposition(pagFile);
                fguiGpuPlayer.setProgress(0);
                fguiGpuActive = true;
                setFguiGpuTickPhase(FguiGpuTickPhase.PLAYING);
                armFguiGpuPlaybackClock();
                fguiGpuPendingProgress = 0.0;
                fguiGpuLastCompletedLoops = 0L;
                if (scheduleTick) {
                    scheduleFguiGpuTick();
                }
                Log.i(TAG, "applyFguiGpuCompositionSwitchInPlace: in-place swap");
                return;
            } catch (Exception e) {
                Log.w(TAG, "applyFguiGpuCompositionSwitchInPlace failed, fallback recycle: "
                        + e.getMessage());
            }
        }
        recycleFguiGpuPlayerKeepingSurface();
    }

    /** repeat=-1 长播：重建 PAGPlayer，保留 Surface/纹理绑定，无需 Unity 重新 Setup。 */
    private void recycleFguiGpuPlayerKeepingSurface() {
        if (pagFile == null) {
            return;
        }
        stopFguiGpuTickScheduling();
        if (fguiGpuPlayer != null) {
            fguiGpuPlayer.setSurface(null);
            fguiGpuPlayer.release();
            fguiGpuPlayer = null;
        }
        try {
            fguiGpuPlayer = new PAGPlayer();
            fguiGpuPlayer.setComposition(pagFile);
            fguiGpuPlayer.setProgress(0);
            if (fguiGpuSurface != null) {
                fguiGpuPlayer.setSurface(fguiGpuSurface);
                fguiGpuSurfaceReady = true;
            }
        } catch (Exception e) {
            Log.e(TAG, "recycleFguiGpuPlayerKeepingSurface failed: " + e.getMessage());
            releaseFguiGpuResources();
            return;
        }
        fguiGpuActive = true;
        setFguiGpuTickPhase(FguiGpuTickPhase.PLAYING);
        armFguiGpuPlaybackClock();
        fguiGpuPendingProgress = 0.0;
        fguiGpuLastCompletedLoops = 0L;
        scheduleFguiGpuTick();
    }

    private long resolveCompositionDurationUs() {
        if (pagFile == null) {
            return 3_000_000L;
        }
        long durationUs = pagFile.duration();
        return durationUs > 0 ? durationUs : 3_000_000L;
    }

    private float resolveCompositionFrameRate() {
        if (pagFile == null) {
            return DEFAULT_COMPOSITION_FRAME_RATE;
        }
        float frameRate = pagFile.frameRate();
        return frameRate > 0f ? frameRate : DEFAULT_COMPOSITION_FRAME_RATE;
    }

    private long resolveCompositionTotalFrames() {
        long durationUs = resolveCompositionDurationUs();
        float frameRate = resolveCompositionFrameRate();
        return Math.max(1L, (long) (durationUs * frameRate / 1_000_000L));
    }

    /** 帧对齐 progress；repeat=-1 时用 modulo 避免墙钟 >=1.0 硬跳 0。 */
    private FguiGpuProgressSnapshot snapshotFguiGpuProgress() {
        FguiGpuProgressSnapshot snap = new FguiGpuProgressSnapshot();
        if (!fguiGpuPlaybackClockArmed) {
            snap.progress = 0.0;
            snap.frameInLoop = 0L;
            snap.totalFrames = resolveCompositionTotalFrames();
            snap.completedLoops = 0L;
            return snap;
        }
        long durationUs = resolveCompositionDurationUs();
        long elapsedMs = Math.max(0L, System.currentTimeMillis() - fguiGpuPlaybackStartMs);
        float frameRate = resolveCompositionFrameRate();
        long totalFrames = resolveCompositionTotalFrames();
        snap.totalFrames = totalFrames;

        if (pagFile == null || frameRate <= 0f) {
            double raw = (elapsedMs * 1000.0) / durationUs;
            if (repeatCount < 0) {
                snap.progress = raw % 1.0;
                if (snap.progress < 0.0) {
                    snap.progress += 1.0;
                }
                snap.completedLoops = (long) Math.floor(raw);
            } else {
                snap.progress = Math.min(1.0, raw);
                snap.completedLoops = 0L;
            }
            snap.frameInLoop = 0L;
            return snap;
        }

        long elapsedFrame = (long) (elapsedMs * frameRate / 1000.0);
        snap.completedLoops = elapsedFrame / totalFrames;
        if (repeatCount < 0) {
            snap.frameInLoop = elapsedFrame % totalFrames;
            snap.progress = (double) snap.frameInLoop / totalFrames;
        } else if (elapsedFrame >= totalFrames) {
            snap.frameInLoop = totalFrames - 1;
            snap.progress = 1.0;
        } else {
            snap.frameInLoop = elapsedFrame;
            snap.progress = (double) elapsedFrame / totalFrames;
        }
        return snap;
    }

    private void stopFguiGpuPlayback() {
        releaseFguiGpuPlayerOnly();
        // 保留 gpuTextureId 绑定，供同尺寸循环播放下一份 PAG 复用 Unity GL 纹理。
    }

    private void releaseFguiGpuResources() {
        if (fguiGpuPlayer != null) {
            fguiGpuPlayer.setSurface(null);
            fguiGpuPlayer.release();
            fguiGpuPlayer = null;
        }
        if (fguiGpuSurface != null) {
            fguiGpuSurface.release();
            fguiGpuSurface = null;
        }
    }

    private void startBitmapFallback(String caller) {
        if (pagFile == null || overlayRoot == null) {
            Log.e(TAG, "startBitmapFallback: pagFile or overlay null, caller=" + caller);
            return;
        }
        bitmapFallbackFrameMs = BITMAP_FALLBACK_FRAME_MS;
        int layoutW = pagView != null ? pagView.getWidth() : 0;
        int layoutH = pagView != null ? pagView.getHeight() : 0;
        int[] size = resolveBitmapFallbackDimensions(layoutW, layoutH);
        int w = size[0];
        int h = size[1];
        stopBitmapFallback();
        try {
            bitmapFallbackSurface = PAGSurface.MakeOffscreen(w, h);
            bitmapFallbackPlayer = new PAGPlayer();
            bitmapFallbackPlayer.setSurface(bitmapFallbackSurface);
            bitmapFallbackPlayer.setComposition(pagFile);
            bitmapFallbackPlayer.setProgress(0);
            bitmapFallbackBitmap = Bitmap.createBitmap(w, h, Bitmap.Config.ARGB_8888);
        } catch (Exception e) {
            Log.e(TAG, "startBitmapFallback: init failed: " + e.getMessage());
            releaseBitmapFallbackResources();
            return;
        }
        ensureBitmapFallbackImageView(w, h);
        detachBitmapFallbackImageView();
        bitmapFallbackImageView.setVisibility(View.VISIBLE);
        bitmapFallbackActive = true;
        isPlaying = true;
        bitmapFallbackStartMs = System.currentTimeMillis();
        Log.i(TAG, "startBitmapFallback: " + w + "x" + h + " caller=" + caller);
        scheduleBitmapFallbackTick();
        notifyPlayStarted();
    }

    private void ensureBitmapFallbackImageView(int w, int h) {
        if (overlayRoot == null) {
            return;
        }
        if (bitmapFallbackImageView == null) {
            bitmapFallbackImageView = new ImageView(activity);
            bitmapFallbackImageView.setScaleType(ImageView.ScaleType.FIT_XY);
            overlayRoot.addView(bitmapFallbackImageView, new FrameLayout.LayoutParams(w, h));
        }
        FrameLayout.LayoutParams ivParams =
                (FrameLayout.LayoutParams) bitmapFallbackImageView.getLayoutParams();
        if (pagView != null && pagView.getLayoutParams() instanceof FrameLayout.LayoutParams) {
            FrameLayout.LayoutParams pagParams = (FrameLayout.LayoutParams) pagView.getLayoutParams();
            ivParams.width = pagParams.width;
            ivParams.height = pagParams.height;
            ivParams.gravity = pagParams.gravity;
            ivParams.setMargins(pagParams.leftMargin, pagParams.topMargin,
                    pagParams.rightMargin, pagParams.bottomMargin);
        } else {
            ivParams.width = w;
            ivParams.height = h;
        }
        bitmapFallbackImageView.setLayoutParams(ivParams);
    }

    private void scheduleBitmapFallbackTick() {
        if (!bitmapFallbackActive || bitmapFallbackPlayer == null) {
            return;
        }
        if (bitmapFallbackTickRunnable != null) {
            mainHandler.removeCallbacks(bitmapFallbackTickRunnable);
        }
        bitmapFallbackTickRunnable = () -> {
            if (!bitmapFallbackActive || bitmapFallbackPlayer == null || pagFile == null) {
                return;
            }
            long durationUs = pagFile.duration();
            if (durationUs <= 0) {
                durationUs = 3_000_000L;
            }
            long durationMs = Math.max(1L, durationUs / 1000L);
            long elapsed = System.currentTimeMillis() - bitmapFallbackStartMs;
            double progress = Math.min(1.0, elapsed / (double) durationMs);
            if (repeatCount < 0 && progress >= 1.0) {
                bitmapFallbackStartMs = System.currentTimeMillis();
                progress = 0;
            }
            bitmapFallbackPlayer.setProgress(progress);
            bitmapFallbackPlayer.flush();
            if (bitmapFallbackSurface != null && bitmapFallbackBitmap != null
                    && !bitmapFallbackBitmap.isRecycled()) {
                bitmapFallbackSurface.copyPixelsTo(bitmapFallbackBitmap);
                if (bitmapFallbackImageView != null
                        && bitmapFallbackImageView.getVisibility() == View.VISIBLE) {
                    bitmapFallbackImageView.setImageBitmap(bitmapFallbackBitmap);
                }
            }
            if (repeatCount >= 0 && progress >= 0.999) {
                stopBitmapFallback();
                return;
            }
            mainHandler.postDelayed(bitmapFallbackTickRunnable, bitmapFallbackFrameMs);
        };
        mainHandler.post(bitmapFallbackTickRunnable);
    }

    private void stopBitmapFallback() {
        if (bitmapFallbackTickRunnable != null) {
            mainHandler.removeCallbacks(bitmapFallbackTickRunnable);
            bitmapFallbackTickRunnable = null;
        }
        bitmapFallbackActive = false;
        detachBitmapFallbackImageView();
        releaseBitmapFallbackResources();
    }

    private void releaseBitmapFallbackResources() {
        if (bitmapFallbackPlayer != null) {
            bitmapFallbackPlayer.setSurface(null);
            bitmapFallbackPlayer.release();
            bitmapFallbackPlayer = null;
        }
        if (bitmapFallbackSurface != null) {
            bitmapFallbackSurface.release();
            bitmapFallbackSurface = null;
        }
        if (bitmapFallbackBitmap != null) {
            bitmapFallbackBitmap.recycle();
            bitmapFallbackBitmap = null;
        }
    }

    private void scheduleTuneUnitySurfaceZOrder() {
        if (!TUNE_UNITY_SURFACE_Z_ORDER) {
            return;
        }
        View decor = activity.getWindow().getDecorView();
        tuneUnitySurfaceZOrder(decor);
        decor.post(() -> tuneUnitySurfaceZOrder(decor));
    }

    private void tuneUnitySurfaceZOrder(View root) {
        if (!TUNE_UNITY_SURFACE_Z_ORDER || root == null) {
            return;
        }
        if (overlayRoot != null && root == overlayRoot) {
            return;
        }
        if (root instanceof ViewGroup) {
            ViewGroup group = (ViewGroup) root;
            int childCount = group.getChildCount();
            for (int i = 0; i < childCount; i++) {
                View child = group.getChildAt(i);
                if (child == overlayRoot || child == wmOverlayRoot) {
                    continue;
                }
                tuneUnitySurfaceZOrder(child);
            }
        }
        if (!(root instanceof SurfaceView)) {
            return;
        }
        String className = root.getClass().getName();
        if (className.contains("PAGView")) {
            return;
        }
        SurfaceView surfaceView = (SurfaceView) root;
        try {
            surfaceView.setZOrderOnTop(false);
            surfaceView.setZOrderMediaOverlay(false);
            Log.i(TAG, "tuneUnitySurface: class=" + className
                    + " setZOrderOnTop(false) setZOrderMediaOverlay(false)");
        } catch (Exception e) {
            Log.w(TAG, "tuneUnitySurface failed: class=" + className + ", " + e.getMessage());
        }
    }

    private void applyDebugOverlayBackground() {
        if (overlayRoot == null) {
            return;
        }
        if (!DEBUG_OVERLAY_RED_BG && !DEBUG_PAGVIEW_BLUE_BG) {
            overlayRoot.setBackgroundColor(Color.TRANSPARENT);
            if (debugBackdropView != null) {
                debugBackdropView.setBackgroundColor(Color.TRANSPARENT);
            }
            if (pagView != null) {
                pagView.setBackgroundColor(Color.TRANSPARENT);
            }
            return;
        }
        if (debugBackdropView != null) {
            debugBackdropView.setBackgroundColor(
                    DEBUG_OVERLAY_RED_BG ? 0x44FF0000 : Color.TRANSPARENT);
        } else if (DEBUG_OVERLAY_RED_BG) {
            overlayRoot.setBackgroundColor(0x44FF0000);
        } else {
            overlayRoot.setBackgroundColor(Color.TRANSPARENT);
        }
        if (pagView != null) {
            pagView.setBackgroundColor(DEBUG_PAGVIEW_BLUE_BG ? 0x440000FF : Color.TRANSPARENT);
        }
        Log.i(TAG, "applyDebugOverlayBackground: red=" + DEBUG_OVERLAY_RED_BG
                + " bluePagView=" + DEBUG_PAGVIEW_BLUE_BG);
    }

    private void bringOverlayToFront() {
        if (overlayRoot == null) {
            return;
        }
        if (!wmOverlayAttached) {
            ViewGroup decor = (ViewGroup) activity.getWindow().getDecorView();
            overlayRoot.setElevation(10000f);
            decor.bringChildToFront(overlayRoot);
        }
        bringPagViewToFront();
        scheduleTuneUnitySurfaceZOrder();
    }

    private PAGFile loadPagFile(String path) {
        if (path.startsWith("assets://")) {
            String assetPath = path.substring("assets://".length());
            return PAGFile.Load(activity.getAssets(), assetPath);
        }
        return PAGFile.Load(path);
    }

    private void applyLayout(String position, String extra) {
        if (pagView == null) {
            return;
        }
        if (pagFile != null && pagFile.width() > 0 && pagFile.height() > 0) {
            applyLayoutFromComposition(position, extra);
            return;
        }
        applyLayoutPreset(position, extra);
    }

    /** 按 PAG 合成原始像素尺寸布局；仅当超出屏幕时等比缩小。 */
    private void applyLayoutFromComposition(String position, String extra) {
        if (pagView == null || pagFile == null) {
            return;
        }
        WindowManager manager = activity.getWindowManager();
        DisplayMetrics metrics = new DisplayMetrics();
        manager.getDefaultDisplay().getMetrics(metrics);

        int compW = pagFile.width();
        int compH = pagFile.height();
        float scale = 1f;
        if (compW > metrics.widthPixels || compH > metrics.heightPixels) {
            scale = Math.min(metrics.widthPixels / (float) compW,
                    metrics.heightPixels / (float) compH);
        }
        int w = Math.max(1, Math.round(compW * scale));
        int h = Math.max(1, Math.round(compH * scale));

        FrameLayout.LayoutParams params = (FrameLayout.LayoutParams) pagView.getLayoutParams();
        Rect customRect = PagLayoutHelper.parseCustomRect(metrics, extra);
        if (customRect != null) {
            params.gravity = android.view.Gravity.TOP | android.view.Gravity.START;
            params.width = w;
            params.height = h;
            int left = customRect.centerX() - w / 2;
            int top = customRect.centerY() - h / 2;
            left = Math.max(0, Math.min(left, metrics.widthPixels - w));
            top = Math.max(0, Math.min(top, metrics.heightPixels - h));
            params.setMargins(left, top, 0, 0);
            Log.i(TAG, "applyLayoutFromComposition: metrics=" + metrics.widthPixels + "x"
                    + metrics.heightPixels + " comp=" + compW + "x" + compH + " scale=" + scale
                    + " anchorCenter=" + customRect.centerX() + "," + customRect.centerY()
                    + " pagView=" + params.width + "x" + params.height);
        } else {
            params.gravity = android.view.Gravity.CENTER;
            params.width = w;
            params.height = h;
            params.setMargins(0, 0, 0, 0);
            Log.i(TAG, "applyLayoutFromComposition: metrics=" + metrics.widthPixels + "x"
                    + metrics.heightPixels + " comp=" + compW + "x" + compH + " scale=" + scale
                    + " pagView=" + params.width + "x" + params.height + " position=" + position);
        }
        pagView.setLayoutParams(params);
    }

    private void applyLayoutPreset(String position, String extra) {
        if (pagView == null) {
            return;
        }
        WindowManager manager = activity.getWindowManager();
        DisplayMetrics metrics = new DisplayMetrics();
        manager.getDefaultDisplay().getMetrics(metrics);

        FrameLayout.LayoutParams params = (FrameLayout.LayoutParams) pagView.getLayoutParams();
        Rect customRect = PagLayoutHelper.parseCustomRect(metrics, extra);
        if (customRect != null) {
            params.gravity = android.view.Gravity.TOP | android.view.Gravity.START;
            params.width = customRect.width();
            params.height = customRect.height();
            params.setMargins(customRect.left, customRect.top, 0, 0);
            Log.i(TAG, "applyLayout: metrics=" + metrics.widthPixels + "x" + metrics.heightPixels
                    + " custom=" + customRect.left + "," + customRect.top + ","
                    + customRect.right + "," + customRect.bottom
                    + " pagView=" + params.width + "x" + params.height);
        } else {
            PagLayoutHelper.LayoutSpec spec = PagLayoutHelper.resolve(
                    metrics, position, layoutPlace, rightAdaptiveW, rightAdaptiveH);
            PagLayoutHelper.apply(params, spec);
            Log.i(TAG, "applyLayout: metrics=" + metrics.widthPixels + "x" + metrics.heightPixels
                    + " preset position=" + position + " layoutPlace=" + layoutPlace
                    + " pagView=" + params.width + "x" + params.height);
        }
        pagView.setLayoutParams(params);
    }

    private void ensureExportThread() {
        if (exportThread == null) {
            exportThread = new HandlerThread("pag-worker");
            exportThread.start();
            exportHandler = new Handler(exportThread.getLooper());
        }
    }

    private String encodePagToMp4(String pagPath, String outputName) {
        PAGFile file = loadPagFile(pagPath);
        if (file == null) {
            throw new RuntimeException("load pag failed: " + pagPath);
        }

        MediaCodec encoder = null;
        MediaMuxer muxer = null;
        PAGPlayer player = null;
        String outputPath = null;
        try {
            encoder = MediaCodec.createEncoderByType(MIME_TYPE);
            MediaFormat format = createVideoFormat(file);
            encoder.configure(format, null, null, MediaCodec.CONFIGURE_FLAG_ENCODE);

            PAGSurface pagSurface = PAGSurface.FromSurface(encoder.createInputSurface());
            player = new PAGPlayer();
            player.setSurface(pagSurface);
            player.setComposition(file);
            player.setProgress(0);
            encoder.start();

            File outDir = activity.getExternalFilesDir(Environment.DIRECTORY_MOVIES);
            String safeName = (outputName == null || outputName.isEmpty()) ? "pag_export" : outputName;
            if (!safeName.endsWith(".mp4")) {
                safeName += ".mp4";
            }
            outputPath = new File(outDir, safeName).getAbsolutePath();
            muxer = new MediaMuxer(outputPath, MediaMuxer.OutputFormat.MUXER_OUTPUT_MPEG_4);

            MediaCodec.BufferInfo bufferInfo = new MediaCodec.BufferInfo();
            final int[] trackIndex = {-1};
            final boolean[] muxerStarted = {false};
            int totalFrames = (int) (file.duration() * file.frameRate() / 1000000);
            if (totalFrames <= 0) {
                totalFrames = FRAME_RATE;
            }

            for (int i = 0; i < totalFrames; i++) {
                drainEncoder(encoder, muxer, bufferInfo, trackIndex, muxerStarted, false);
                float progress = i % totalFrames * 1.0f / totalFrames;
                player.setProgress(progress);
                player.flush();
            }
            drainEncoder(encoder, muxer, bufferInfo, trackIndex, muxerStarted, true);
            Log.i(TAG, "export path: " + outputPath);
            return outputPath;
        } catch (IOException e) {
            throw new RuntimeException(e);
        } finally {
            if (encoder != null) {
                try {
                    encoder.stop();
                } catch (Exception ignored) {
                }
                encoder.release();
            }
            if (muxer != null) {
                try {
                    muxer.stop();
                } catch (Exception ignored) {
                }
                muxer.release();
            }
        }
    }

    private MediaFormat createVideoFormat(PAGFile file) {
        int width = file.width();
        int height = file.height();
        if (width % 2 == 1) {
            width--;
        }
        if (height % 2 == 1) {
            height--;
        }
        MediaFormat format = MediaFormat.createVideoFormat(MIME_TYPE, width, height);
        format.setInteger(MediaFormat.KEY_COLOR_FORMAT,
                MediaCodecInfo.CodecCapabilities.COLOR_FormatSurface);
        format.setInteger(MediaFormat.KEY_BIT_RATE, BIT_RATE);
        format.setInteger(MediaFormat.KEY_FRAME_RATE, FRAME_RATE);
        format.setInteger(MediaFormat.KEY_I_FRAME_INTERVAL, IFRAME_INTERVAL);
        return format;
    }

    private void drainEncoder(MediaCodec encoder, MediaMuxer muxer, MediaCodec.BufferInfo bufferInfo,
                              int[] trackIndex, boolean[] muxerStarted, boolean endOfStream) {
        if (endOfStream) {
            encoder.signalEndOfInputStream();
        }
        ByteBuffer[] outputBuffers = encoder.getOutputBuffers();
        while (true) {
            int index = encoder.dequeueOutputBuffer(bufferInfo, TIMEOUT_USEC);
            if (index == MediaCodec.INFO_TRY_AGAIN_LATER) {
                if (!endOfStream) {
                    break;
                }
            } else if (index == MediaCodec.INFO_OUTPUT_BUFFERS_CHANGED) {
                outputBuffers = encoder.getOutputBuffers();
            } else if (index == MediaCodec.INFO_OUTPUT_FORMAT_CHANGED) {
                if (muxerStarted[0]) {
                    throw new RuntimeException("format changed twice");
                }
                trackIndex[0] = muxer.addTrack(encoder.getOutputFormat());
                muxer.start();
                muxerStarted[0] = true;
            } else if (index >= 0) {
                ByteBuffer buffer = outputBuffers[index];
                if ((bufferInfo.flags & MediaCodec.BUFFER_FLAG_CODEC_CONFIG) != 0) {
                    bufferInfo.size = 0;
                }
                if (bufferInfo.size != 0) {
                    if (!muxerStarted[0]) {
                        throw new RuntimeException("muxer hasn't started");
                    }
                    buffer.position(bufferInfo.offset);
                    buffer.limit(bufferInfo.offset + bufferInfo.size);
                    muxer.writeSampleData(trackIndex[0], buffer, bufferInfo);
                }
                encoder.releaseOutputBuffer(index, false);
                if ((bufferInfo.flags & MediaCodec.BUFFER_FLAG_END_OF_STREAM) != 0) {
                    break;
                }
            }
        }
    }
}
