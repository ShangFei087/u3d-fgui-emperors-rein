using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.IO;
using System.Text;
using TextAsset = UnityEngine.TextAsset;

public class FileLoaderManager : MonoSingleton<FileLoaderManager>
{
    // 超时时间设置（秒）
    [SerializeField] private int webRequestTimeout = 10;

    /// <summary>
    /// 统一的文件读取入口，自动判断路径类型并选择合适的加载方式
    /// </summary>
    /// <param name="pathOrUrl">本地路径或网络URL</param>
    /// <param name="onCallback">加载完成回调</param>
    public void LoadFile(string pathOrUrl, Action<byte[]> onCallback)
    {
        if (string.IsNullOrEmpty(pathOrUrl))
        {
            Debug.LogError("路径或URL不能为空");
            onCallback?.Invoke(null);
            return;
        }

        // 优先判断是否为网络路径
        if (IsWebUrl(pathOrUrl))
        {
            StartCoroutine(LoadFromWebUrl(pathOrUrl, onCallback));
        }
        // 其次判断是否为StreamingAssets路径
        else if (IsStreamingAssetsPath(pathOrUrl))
        {
            StartCoroutine(LoadFromStreamingAssets(pathOrUrl, onCallback));
        }
        // 最后视为本地文件路径
        else
        {
            LoadFromLocalPath(pathOrUrl, onCallback);
        }
    }

    /// <summary>
    /// 从本地路径加载文件（同步）
    /// </summary>
    private void LoadFromLocalPath(string path, Action<byte[]> onComplete)
    {
        try
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"本地文件不存在: {path}");
                onComplete?.Invoke(null);
                return;
            }

            // 使用using确保文件流正确释放
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[fileStream.Length];
                int bytesRead = fileStream.Read(buffer, 0, (int)fileStream.Length);

                if (bytesRead != fileStream.Length)
                {
                    Debug.LogError($"未能完整读取文件: {path}，读取了 {bytesRead}/{fileStream.Length} 字节");
                    onComplete?.Invoke(null);
                    return;
                }

                onComplete?.Invoke(buffer);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"加载本地文件出错: {ex.Message}\n路径: {path}");
            onComplete?.Invoke(null);
        }
    }

    /// <summary>
    /// 从网络URL加载文件（异步）
    /// </summary>
    private IEnumerator LoadFromWebUrl(string url, Action<byte[]> onComplete)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = webRequestTimeout;

            // 发送请求并等待完成
            yield return request.SendWebRequest();

            // 处理请求结果
            if (request.result == UnityWebRequest.Result.Success)
            {
                onComplete?.Invoke(request.downloadHandler.data);
            }
            else
            {
                Debug.LogError($"网络请求失败: {request.error}\nURL: {url}");
                onComplete?.Invoke(null);
            }
        }
    }

    /// <summary>
    /// 从StreamingAssets加载文件（异步，处理跨平台差异）
    /// </summary>
    private IEnumerator LoadFromStreamingAssets(string path, Action<byte[]> onComplete)
    {
        // 处理路径，确保跨平台兼容性
        string streamingPath = GetPlatformCompatibleStreamingPath(path);

        if (Application.platform == RuntimePlatform.Android)
        {
            // Android平台需要使用UnityWebRequest访问StreamingAssets
            using (UnityWebRequest request = UnityWebRequest.Get(streamingPath))
            {
                request.timeout = webRequestTimeout;
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    onComplete?.Invoke(request.downloadHandler.data);
                }
                else
                {
                    Debug.LogError($"Android加载StreamingAssets失败: {request.error}\n路径: {streamingPath}");
                    onComplete?.Invoke(null);
                }
            }
        }
        else
        {
            // 其他平台直接使用文件系统读取
            if (File.Exists(streamingPath))
            {
                LoadFromLocalPath(streamingPath, onComplete);
            }
            else
            {
                Debug.LogError($"StreamingAssets文件不存在: {streamingPath}");
                onComplete?.Invoke(null);
            }
            yield return null;
        }
    }

    /// <summary>
    /// 检查路径是否为网络URL
    /// </summary>
    private bool IsWebUrl(string path)
    {
        return path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 检查路径是否在StreamingAssets目录下
    /// </summary>
    private bool IsStreamingAssetsPath(string path)
    {
        return path.StartsWith(Application.streamingAssetsPath, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 获取平台兼容的StreamingAssets路径
    /// </summary>
    private string GetPlatformCompatibleStreamingPath(string originalPath)
    {
        // 如果是Android平台，需要转换为jar协议路径
        if (Application.platform == RuntimePlatform.Android &&
            !originalPath.StartsWith("jar:file://", StringComparison.OrdinalIgnoreCase))
        {
            string relativePath = originalPath.Replace(Application.streamingAssetsPath, "");
            return $"jar:file://{Application.dataPath}!/assets/{relativePath.TrimStart('/')}";
        }

        return originalPath;
    }

    /// <summary>
    /// 取消所有正在进行的加载操作
    /// </summary>
    public void CancelAllLoads()
    {
        StopAllCoroutines();
    }



    /// <summary>
    /// 加载图片并返回Texture2D
    /// </summary>
    public void LoadImageAsTexture(string pathOrUrl, Action<Texture2D> onComplete,
                                 FilterMode filterMode = FilterMode.Bilinear,
                                 TextureWrapMode wrapMode = TextureWrapMode.Clamp)
    {
        LoadFile(pathOrUrl, (byteData) =>
        {
            if (byteData == null)
            {
                onComplete?.Invoke(null);
                return;
            }

            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.filterMode = filterMode;
            texture.wrapMode = wrapMode;

            if (texture.LoadImage(byteData)) // 自动识别PNG/JPG格式
            {
                onComplete?.Invoke(texture);
            }
            else
            {
                Debug.LogError("无法解析图片数据，可能不是有效的图片格式");
                Destroy(texture);
                onComplete?.Invoke(null);
            }
        });
    }


    /// <summary>
    /// 加载图片并返回Sprite
    /// </summary>
    public void LoadImageAsSprite(string pathOrUrl, Action<Sprite> onComplete,
                                Vector2 pivot = default,
                                FilterMode filterMode = FilterMode.Bilinear)
    {
        LoadImageAsTexture(pathOrUrl, (texture) =>
        {
            if (texture == null)
            {
                onComplete?.Invoke(null);
                return;
            }

            // 默认中心点为中心
            Vector2 actualPivot = pivot == default ? new Vector2(0.5f, 0.5f) : pivot;

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                actualPivot,
                100, // 像素每单位
                0,   // 边框
                SpriteMeshType.Tight
            );

            onComplete?.Invoke(sprite);
        }, filterMode);
    }


    /// <summary>
    /// 加载文本文件并返回TextAsset实例
    /// </summary>
    public void LoadTextAsset(string pathOrUrl, Action<TextAsset> onComplete, Encoding customEncoding = null)
    {
        LoadFile(pathOrUrl, (byteData) =>
        {
            if (byteData == null)
            {
                onComplete?.Invoke(null);
                return;
            }

            try
            {
                // 获取目标编码
                Encoding targetEncoding = customEncoding ?? Encoding.GetEncoding("UTF-8");
                if (targetEncoding == null) targetEncoding = Encoding.UTF8;

                // 生成TextAsset（包含文件名）
                //string fileName = Path.GetFileName(pathOrUrl);
                string textContent = targetEncoding.GetString(byteData);
                TextAsset textAsset = new TextAsset(textContent);

                onComplete?.Invoke(textAsset);
            }
            catch (Exception ex)
            {
                Debug.LogError($"转换为TextAsset失败: {ex.Message}，路径: {pathOrUrl}");
                onComplete?.Invoke(null);
            }
        });
    }
}