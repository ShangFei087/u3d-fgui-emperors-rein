using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 从热更 AB（persistent 优先，StreamingAssets 回退）解析 PAG，
/// 解压到 persistentDataPath/PagCache 供 Android PagBridge 读取绝对路径。
/// </summary>
public static class PagPathHelper
{
    private const string LogPrefix = "[PAG Path]";

    /// <summary>相对 GameRes 的 PAG 目录，可按游戏修改。</summary>
    public const string DefaultGamePagFolder = "Games/Slot Zhu Zai Jin Bi 1700/Pag";

    private const string CacheFolderName = "PagCache";
    private const string AbCacheFolderName = "_ab";

    /// <summary>合法 .pag 缓存最小体积（过小多为 TextAsset 损坏或旧缓存）。</summary>
    public const int MinValidPagFileBytes = 256;

    private const long LargePagExtractGcThresholdBytes = 8L * 1024 * 1024;

    /// <summary>
    /// Dragon 播完、大体积 PAG 开播前再卸未用资源，避免与 Dragon 播放叠加触发 OOM。
    /// </summary>
    public static IEnumerator DeferredUnloadUnusedAssets()
    {
        Debug.Log($"{LogPrefix} deferred UnloadUnusedAssets before large PAG play");
        Resources.UnloadUnusedAssets();
        yield return null;
        yield return null;
    }

    public static bool IsValidPagFile(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            return false;
        }

        try
        {
            return new FileInfo(path).Length >= MinValidPagFileBytes;
        }
        catch
        {
            return false;
        }
    }

    public static string CacheRoot =>
        Path.Combine(Application.persistentDataPath, CacheFolderName);

    private static string AbCacheRoot =>
        Path.Combine(CacheRoot, AbCacheFolderName);

    /// <summary>
    /// 解析 PAG 本地绝对路径：优先 PagCache，否则从热更 AB 解压。
    /// </summary>
    public static string Resolve(string fileName, string gamePagFolder = DefaultGamePagFolder)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogWarning($"{LogPrefix} Resolve failed: fileName is empty");
            return null;
        }

        if (File.Exists(fileName))
        {
            Debug.Log($"{LogPrefix} hit absolute path: {fileName}");
            return fileName;
        }

        if (!fileName.EndsWith(".pag", StringComparison.OrdinalIgnoreCase))
        {
            fileName += ".pag";
        }

        string leaf = Path.GetFileName(fileName);
        string cachePath = Path.Combine(CacheRoot, gamePagFolder.Replace("\\", "/"), leaf);
        cachePath = cachePath.Replace("\\", "/");
        if (File.Exists(cachePath))
        {
            long cacheSize = new FileInfo(cachePath).Length;
            if (cacheSize >= MinValidPagFileBytes)
            {
                Debug.Log($"{LogPrefix} hit PagCache: {cachePath}, bytes={cacheSize}");
                return cachePath;
            }

            Debug.LogWarning($"{LogPrefix} stale PagCache removed (bytes={cacheSize}): {cachePath}");
            try
            {
                File.Delete(cachePath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"{LogPrefix} delete stale cache failed: {ex.Message}");
            }
        }

        string assetPath = $"Assets/GameRes/{gamePagFolder}/{leaf}";
#if UNITY_EDITOR
        if (!ApplicationSettings.Instance.IsUseStreamingAssetsBundle())
        {
            string editorPath = Path.Combine(Application.dataPath, "GameRes", gamePagFolder, leaf);
            if (File.Exists(editorPath))
            {
                Debug.Log($"{LogPrefix} hit Editor GameRes: {editorPath}");
                return editorPath;
            }
        }
#endif

        if (TryExtractPagToCache(assetPath, cachePath))
        {
            return cachePath;
        }

        Debug.LogError($"{LogPrefix} Resolve failed: {leaf}, assetPath={assetPath}");
        return null;
    }

    private static byte[] LoadPagBytesFromBundle(AssetBundle bundle, string assetName)
    {
        if (bundle == null || string.IsNullOrEmpty(assetName))
        {
            return null;
        }

        PagBinaryAsset pagAsset = bundle.LoadAsset<PagBinaryAsset>(assetName);
        if (pagAsset != null && pagAsset.data != null && pagAsset.data.Length > 0)
        {
            Debug.Log($"{LogPrefix} loaded PagBinaryAsset: {assetName}, bytes={pagAsset.data.Length}");
            return pagAsset.data;
        }

        TextAsset textAsset = bundle.LoadAsset<TextAsset>(assetName);
        if (textAsset == null)
        {
            string[] names = bundle.GetAllAssetNames();
            Debug.LogWarning($"{LogPrefix} PagBinaryAsset '{assetName}' not found, fallback TextAsset, " +
                             $"all names=[{string.Join(", ", names ?? Array.Empty<string>())}]");
            if (names != null && names.Length > 0)
            {
                pagAsset = bundle.LoadAsset<PagBinaryAsset>(names[0]);
                if (pagAsset?.data != null && pagAsset.data.Length > 0)
                {
                    return pagAsset.data;
                }

                textAsset = bundle.LoadAsset<TextAsset>(names[0]);
            }
        }

        if (textAsset?.bytes == null || textAsset.bytes.Length == 0)
        {
            return null;
        }

        if (textAsset.bytes.Length < MinValidPagFileBytes)
        {
            Debug.LogWarning($"{LogPrefix} TextAsset bytes too small ({textAsset.bytes.Length}), " +
                             "rebuild AB with PagBinaryImporter v2 / PagBinaryAsset");
        }

        return textAsset.bytes;
    }

    /// <summary>是否已在 PagCache 且体积合法（不触发 AB 解压）。</summary>
    public static bool IsCached(string fileName, string gamePagFolder = DefaultGamePagFolder)
    {
        string cachePath = GetCachePathForLeaf(fileName, gamePagFolder);
        return IsValidPagFile(cachePath);
    }

    public static void WarmupPagCache(
        MonoBehaviour host,
        string fileName,
        string gamePagFolder = DefaultGamePagFolder,
        Action<bool> onDone = null)
    {
        if (host == null)
        {
            Debug.LogWarning($"{LogPrefix} WarmupPagCache skipped: host is null");
            onDone?.Invoke(false);
            return;
        }

        host.StartCoroutine(WarmupPagCacheCoroutine(fileName, gamePagFolder, onDone));
    }

    public static IEnumerator WarmupPagCacheCoroutine(
        string fileName,
        string gamePagFolder = DefaultGamePagFolder,
        Action<bool> onDone = null)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            onDone?.Invoke(false);
            yield break;
        }

        string cachePath = GetCachePathForLeaf(fileName, gamePagFolder);
        if (IsValidPagFile(cachePath))
        {
            Debug.Log($"{LogPrefix} warmup already cached: {cachePath}");
            onDone?.Invoke(true);
            yield break;
        }

        yield return null;

        string leaf = Path.GetFileName(NormalizePagLeaf(fileName));
        string assetPath = $"Assets/GameRes/{gamePagFolder}/{leaf}";
        bool ok = false;
        try
        {
            ok = TryExtractPagToCache(assetPath, cachePath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"{LogPrefix} warmup extract exception: {ex.Message}");
        }

        yield return null;

        if (ok && IsValidPagFile(cachePath))
        {
            try
            {
                long bytes = new FileInfo(cachePath).Length;
                if (bytes > LargePagExtractGcThresholdBytes)
                {
                    Debug.Log($"{LogPrefix} warmup large pag cached ({bytes} bytes), GC deferred until before play");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"{LogPrefix} warmup size log failed: {ex.Message}");
            }
        }

        bool ready = IsValidPagFile(cachePath);
        if (!ready)
        {
            Debug.LogWarning($"{LogPrefix} warmup failed: {leaf}");
        }

        onDone?.Invoke(ready);
    }

    private static string NormalizePagLeaf(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return fileName;
        }

        if (!fileName.EndsWith(".pag", StringComparison.OrdinalIgnoreCase))
        {
            fileName += ".pag";
        }

        return Path.GetFileName(fileName);
    }

    private static string GetCachePathForLeaf(string fileName, string gamePagFolder)
    {
        string leaf = NormalizePagLeaf(fileName);
        string cachePath = Path.Combine(CacheRoot, gamePagFolder.Replace("\\", "/"), leaf);
        return cachePath.Replace("\\", "/");
    }

    private static bool TryExtractPagToCache(string assetPath, string cachePath)
    {
        if (!ApplicationSettings.Instance.IsUseHotfixBundle())
        {
            Debug.LogWarning($"{LogPrefix} skipped AB extract: IsUseHotfixBundle=false");
            return false;
        }

        string bundleName = GetBundleName(assetPath);
        string bundleFilePath = ResolveBundleFilePath(bundleName);
        if (string.IsNullOrEmpty(bundleFilePath))
        {
            Debug.LogError($"{LogPrefix} AB not found, bundle={bundleName}, " +
                           $"persistent={PathHelper.GetAssetBundleLOCPTH(bundleName)}, " +
                           $"streaming={PathHelper.GetAssetBundleSAPTH(bundleName)}");
            return false;
        }

        Debug.Log($"{LogPrefix} load AB: {bundleFilePath}");

        AssetBundle bundle = null;
        try
        {
            bundle = AssetBundle.LoadFromFile(bundleFilePath);
            if (bundle == null)
            {
                Debug.LogError($"{LogPrefix} LoadFromFile failed: {bundleFilePath}");
                return false;
            }

            string assetName = GetAssetNameFromBundle(bundleName);
            byte[] pagBytes = LoadPagBytesFromBundle(bundle, assetName);
            if (pagBytes == null || pagBytes.Length < MinValidPagFileBytes)
            {
                string[] names = bundle.GetAllAssetNames();
                Debug.LogError($"{LogPrefix} pag bytes invalid, bundle={bundleName}, asset={assetName}, " +
                               $"all names=[{string.Join(", ", names ?? Array.Empty<string>())}]");
                return false;
            }

            string directory = Path.GetDirectoryName(cachePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(cachePath, pagBytes);
            Debug.Log($"{LogPrefix} extracted: {cachePath}, bytes={pagBytes.Length}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"{LogPrefix} extract exception: {ex.Message}");
            return false;
        }
        finally
        {
            bundle?.Unload(true);
        }
    }

    /// <summary>
    /// persistent Hotfix/GameRes 优先；不存在时从 StreamingAssets 落盘到 PagCache/_ab。
    /// </summary>
    private static string ResolveBundleFilePath(string bundleName)
    {
        string localPath = PathHelper.GetAssetBundleLOCPTH(bundleName);
        if (File.Exists(localPath))
        {
            Debug.Log($"{LogPrefix} AB from persistent: {localPath}");
            return localPath;
        }

        string cachedAbPath = Path.Combine(AbCacheRoot, bundleName.Replace("\\", "/"));
        if (File.Exists(cachedAbPath))
        {
            Debug.Log($"{LogPrefix} AB from PagCache/_ab: {cachedAbPath}");
            return cachedAbPath;
        }

        string streamingPath = PathHelper.GetAssetBundleSAPTH(bundleName);
#if UNITY_EDITOR
        if (File.Exists(streamingPath))
        {
            Debug.Log($"{LogPrefix} AB from StreamingAssets (Editor): {streamingPath}");
            return streamingPath;
        }
#endif

        if (!ApplicationSettings.Instance.IsUseStreamingAssetsBundle())
        {
            Debug.LogWarning($"{LogPrefix} StreamingAssets fallback disabled");
            return null;
        }

        if (!TryCopyStreamingAssetToLocalSync(streamingPath, cachedAbPath))
        {
            return null;
        }

        Debug.Log($"{LogPrefix} AB copied from StreamingAssets: {cachedAbPath}");
        return cachedAbPath;
    }

    private static bool TryCopyStreamingAssetToLocalSync(string srcPath, string tarPath)
    {
        try
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using (UnityWebRequest request = UnityWebRequest.Get(srcPath))
            {
                request.SendWebRequest();
                while (!request.isDone)
                {
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"{LogPrefix} SA copy failed: {srcPath}, error={request.error}");
                    return false;
                }

                byte[] bytes = request.downloadHandler.data;
                if (bytes == null || bytes.Length == 0)
                {
                    Debug.LogWarning($"{LogPrefix} SA copy empty: {srcPath}");
                    return false;
                }

                WriteAllBytes(tarPath, bytes);
                return true;
            }
#else
            if (!File.Exists(srcPath))
            {
                Debug.LogWarning($"{LogPrefix} SA file missing: {srcPath}");
                return false;
            }

            WriteAllBytes(tarPath, File.ReadAllBytes(srcPath));
            return true;
#endif
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"{LogPrefix} SA copy exception: {srcPath}, {ex.Message}");
            return false;
        }
    }

    private static void WriteAllBytes(string path, byte[] bytes)
    {
        string directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllBytes(path, bytes);
    }

    private static string GetBundleName(string assetPath)
    {
        if (AssetBundleManager02.Instance != null)
        {
            return AssetBundleManager02.Instance.GetBundleName(assetPath);
        }

        string result = assetPath.ToLower();
        const string prefix = "assets/gameres/";
        if (result.StartsWith(prefix))
        {
            result = result.Substring(prefix.Length);
        }

        int lastDot = result.LastIndexOf('.');
        if (lastDot > 0)
        {
            result = result.Substring(0, lastDot);
        }

        return result + ".unity3d";
    }

    private static string GetAssetNameFromBundle(string bundleName)
    {
        string[] parts = bundleName.Split('/');
        return parts[parts.Length - 1].Replace(".unity3d", "");
    }
}
