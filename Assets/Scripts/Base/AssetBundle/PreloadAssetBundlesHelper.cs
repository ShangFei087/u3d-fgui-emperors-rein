using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ab包预加载
/// </summary>
public class PreloadAssetBundlesHelper 
{
    /// <summary> 存储需要预加载的 AB 包列表 </summary>
    
     public string markBundle;
     public  List<object[]> preloadBundleNames = new List<object[]>()
     {
         new object[]{"Assets/GameRes/Games/PssOn00152 (1080x1920)/Prefabs/Page Game Main.prefab" },  //资源路径名（ab包）
         new object[]{"games/psson00152 (1080x1920)/prefabs/page game main.unity3d" }, // ab包名
     };

     public List<object[]> preloadAssetAtPath= new List<object[]>()
     {
         new object[]{"Assets/GameRes/Games/PssOn00152 (1080x1920)/Prefabs/Page Game Main.prefab" },  //资源路径名
     };

    public void UnloadPreloadBundle()
    {
        ResourceManager.Instance.UnloadAssetBundle(markBundle);
    }
    public void LoadPreloadBundleAsync(Action<string> onProgressCallback, Action onFinishCallback)
    {
        _LoadPreloadBundle(0, onProgressCallback, onFinishCallback);
    }

    private void _LoadPreloadBundle(int index, Action<string> onProgressCallback, Action onFinishCallback)
    {
        if (index >= preloadBundleNames.Count)
        {
            onFinishCallback?.Invoke();
            return;
        }
        string assetPathOrBundleName = (string)preloadBundleNames[index][0];
        string bundleName = AssetBundleManager02.Instance.GetBundleName(assetPathOrBundleName);

        onProgressCallback?.Invoke("preload bundle : " + bundleName);

        ResourceManager.Instance.LoadAssetBundleAsync(assetPathOrBundleName, markBundle, (ab) => {
            _LoadPreloadBundle(++index, onProgressCallback, onFinishCallback);
        });
    }

    public void LoadPreloadAssetAsync(Action<string> onProgressCallback, Action onFinishCallback)
    {
        _LoadPreloadAsset(0, onProgressCallback, onFinishCallback);
    }

    private void _LoadPreloadAsset(int index, Action<string> onProgressCallback, Action onFinishCallback)
    {
        if (index >= preloadAssetAtPath.Count)
        {
            onFinishCallback?.Invoke();
            return;
        }
        string assetPath = (string)preloadAssetAtPath[index][0];

        onProgressCallback?.Invoke("preload asset : " + assetPath);

        ResourceManager.Instance.LoadPreloadAssetAsync(assetPath, (obj) => {
            _LoadPreloadAsset(++index, onProgressCallback, onFinishCallback);
        });
    }

    public void UnloadPreloadAsset()
    {
 
        foreach (object[] item in preloadAssetAtPath)
        {
            string assetPath = (string)item[0];
            ResourceManager.Instance.UnloadPreloadAsset(markBundle);
        }
    }

}
