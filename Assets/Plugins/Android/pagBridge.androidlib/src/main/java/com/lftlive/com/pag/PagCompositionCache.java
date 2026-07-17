package com.lftlive.com.pag;

import android.os.Handler;
import android.os.HandlerThread;
import android.util.Log;

import org.libpag.PAGFile;

import java.util.LinkedHashMap;
import java.util.Map;

/**
 * 全局 PAGFile LRU 内存缓存。libpag 4.4.x 无显式 release，淘汰时仅移除 Map 引用。
 */
final class PagCompositionCache {
    private static final String TAG = "PagCompositionCache";
    private static final int MAX_CACHE_SIZE = 40;

    private static final Map<String, PAGFile> sCache = new LinkedHashMap<String, PAGFile>(MAX_CACHE_SIZE, 0.75f, true) {
        @Override
        protected boolean removeEldestEntry(Map.Entry<String, PAGFile> eldest) {
            if (size() > MAX_CACHE_SIZE) {
                Log.i(TAG, "evict LRU: " + eldest.getKey());
                return true;
            }
            return false;
        }
    };

    private static HandlerThread sWorkerThread;
    private static Handler sWorkerHandler;

    private PagCompositionCache() {
    }

    static synchronized PAGFile get(String absPath) {
        if (absPath == null || absPath.isEmpty()) {
            return null;
        }
        return sCache.get(absPath);
    }

    static synchronized boolean contains(String absPath) {
        if (absPath == null || absPath.isEmpty()) {
            return false;
        }
        return sCache.containsKey(absPath);
    }

    static synchronized void put(String absPath, PAGFile file) {
        if (absPath == null || absPath.isEmpty() || file == null) {
            return;
        }
        sCache.put(absPath, file);
        Log.i(TAG, "put: " + absPath + " size=" + file.width() + "x" + file.height()
                + " cacheCount=" + sCache.size());
    }

    static synchronized void evictAll() {
        sCache.clear();
        Log.i(TAG, "evictAll");
    }

    static void ensureWorker() {
        synchronized (PagCompositionCache.class) {
            if (sWorkerThread == null) {
                sWorkerThread = new HandlerThread("pag-composition-cache");
                sWorkerThread.start();
                sWorkerHandler = new Handler(sWorkerThread.getLooper());
            }
        }
    }

    static void postOnWorker(Runnable action) {
        ensureWorker();
        sWorkerHandler.post(action);
    }

    interface PagFileLoader {
        PAGFile load(String path);
    }

    /** worker 线程：命中则直接返回，否则 load + put。 */
    static PAGFile loadOrGetCached(String absPath, PagFileLoader loader) {
        if (absPath == null || absPath.isEmpty()) {
            return null;
        }
        PAGFile cached;
        synchronized (PagCompositionCache.class) {
            cached = sCache.get(absPath);
        }
        if (cached != null) {
            return cached;
        }
        long t0 = System.currentTimeMillis();
        PAGFile loaded = loader != null ? loader.load(absPath) : null;
        long elapsedMs = System.currentTimeMillis() - t0;
        if (loaded != null) {
            put(absPath, loaded);
            Log.i(TAG, "loadOrGetCached MISS loaded: " + absPath
                    + " size=" + loaded.width() + "x" + loaded.height()
                    + " elapsedMs=" + elapsedMs);
        } else {
            Log.e(TAG, "loadOrGetCached MISS failed: " + absPath + " elapsedMs=" + elapsedMs);
        }
        return loaded;
    }

    /** 异步预加载，不阻塞调用线程。 */
    static void preloadAsync(String absPath, PagFileLoader loader) {
        if (absPath == null || absPath.isEmpty()) {
            return;
        }
        postOnWorker(() -> {
            if (contains(absPath)) {
                Log.i(TAG, "preloadAsync: already cached " + absPath);
                return;
            }
            long t0 = System.currentTimeMillis();
            PAGFile loaded = loader != null ? loader.load(absPath) : null;
            long elapsedMs = System.currentTimeMillis() - t0;
            if (loaded != null) {
                put(absPath, loaded);
                Log.i(TAG, "preloadAsync: ok path=" + absPath
                        + " size=" + loaded.width() + "x" + loaded.height()
                        + " elapsedMs=" + elapsedMs);
            } else {
                Log.e(TAG, "preloadAsync: failed path=" + absPath + " elapsedMs=" + elapsedMs);
            }
        });
    }
}
