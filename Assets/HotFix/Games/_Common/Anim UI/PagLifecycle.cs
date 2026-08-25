using UnityEngine;

/// <summary>
/// 切游戏时统一释放当前局 PAG：Dispose 所有 Controller，并清空 Native PAGFile LRU。
/// </summary>
public static class PagLifecycle
{
    public static void ReleaseCurrentGame()
    {
        Debug.Log($"[PAG] ReleaseCurrentGame registryCount={PagControllerRegistry.ActiveCount}");
        PagControllerRegistry.DisposeAll();
        PagController.EvictCompositionCache();
    }
}
