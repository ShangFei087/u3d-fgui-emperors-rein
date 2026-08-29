using UnityEngine;

/// <summary>
/// 全局 PAG 管线一次性初始化（Hub + JNI + 默认开关）。可重复调用，内部幂等。
/// </summary>
public static class PagBootstrap
{
    private static bool _ready;

    public static bool IsReady => _ready;

    /// <summary>确保 PAG 全局环境就绪；Main 启动或各游戏 Loading 均可调用。</summary>
    public static void EnsureReady()
    {
        if (_ready)
        {
            return;
        }

        PagCallbackHub.EnsureInstance();
        PagController.EnsureInit();
        PagPresentationDefaults.ApplyPipelineGlobals();

        _ready = true;
        PagCallbackHub.Instance.RunCoroutine(PagUnityGlBridge.WarmupCreateTextureCoroutine());
        Debug.Log("[PAG] PagBootstrap.EnsureReady OK");
    }
}
