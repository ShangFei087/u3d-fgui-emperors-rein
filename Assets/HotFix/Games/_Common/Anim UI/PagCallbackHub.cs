using System.Collections;
using UnityEngine;

/// <summary>
/// 全局 UnitySendMessage 回调 Hub；payload 格式：instanceKey + '\x1f' + data。
/// 同时承载协程运行（合并原 PagCoroutineRunner）。
/// </summary>
public sealed class PagCallbackHub : MonoBehaviour
{
    public const string HubObjectName = "PagCallbackHub";
    public const char PayloadSeparator = '\u001f';

    private static PagCallbackHub _instance;

    public static PagCallbackHub Instance => EnsureInstance();

    public static PagCallbackHub EnsureInstance()
    {
        if (_instance != null)
        {
            return _instance;
        }

        GameObject existing = GameObject.Find(HubObjectName);
        GameObject go = existing != null ? existing : new GameObject(HubObjectName);
        if (existing == null)
        {
            DontDestroyOnLoad(go);
        }

        _instance = go.GetComponent<PagCallbackHub>();
        if (_instance == null)
        {
            _instance = go.AddComponent<PagCallbackHub>();
        }

        return _instance;
    }

    public Coroutine RunCoroutine(IEnumerator routine)
    {
        return routine == null ? null : StartCoroutine(routine);
    }

    public void StopRunCoroutine(Coroutine routine)
    {
        if (routine != null)
        {
            StopCoroutine(routine);
        }
    }

    private static bool TryParse(string message, out string instanceKey, out string data)
    {
        instanceKey = null;
        data = message ?? string.Empty;
        if (string.IsNullOrEmpty(message))
        {
            return false;
        }

        int sep = message.IndexOf(PayloadSeparator);
        if (sep < 0)
        {
            return false;
        }

        instanceKey = message.Substring(0, sep);
        data = sep + 1 < message.Length ? message.Substring(sep + 1) : string.Empty;
        return !string.IsNullOrEmpty(instanceKey);
    }

    private static PagController Resolve(string message, out string data)
    {
        if (!TryParse(message, out string instanceKey, out data))
        {
            return null;
        }

#if DEVELOPMENT_BUILD
        Debug.Log($"[PAG Hub] route -> {instanceKey}");
#endif
        return PagControllerRegistry.Resolve(instanceKey);
    }

    public void OnPagOverlayPlayStarted(string message)
    {
        PagController controller = Resolve(message, out _);
        controller?.HandlePlayStarted(string.Empty);
    }

    public void OnPagGpuTextureRequest(string message)
    {
        PagController controller = Resolve(message, out string data);
        controller?.HandleGpuTextureRequest(data);
    }

    public void OnPagGpuRenderFrame(string message)
    {
        PagController controller = Resolve(message, out string data);
        controller?.HandleGpuRenderFrame(data);
    }

    public void OnPagGpuFrameReady(string message)
    {
        PagController controller = Resolve(message, out _);
        controller?.HandleGpuFrameReady(string.Empty);
    }

    public void OnPagPlaybackFinished(string message)
    {
        PagController controller = Resolve(message, out _);
        controller?.HandlePlaybackFinished(string.Empty);
    }

    public void OnPagExportFinished(string message)
    {
        PagController controller = Resolve(message, out string data);
        controller?.HandleExportFinished(data);
    }
}
