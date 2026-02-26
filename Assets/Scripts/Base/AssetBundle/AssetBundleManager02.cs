using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System.IO;
using JetBrains.Annotations;

/// <summary>
/// 
/// </summary>
/// <remark>
/// * ab包名（小写）：games/a/b/c/efg.unity3d 
/// * 一个资源对应一个ab包，该资源名（小写，且不带后缀）: efg ; 该资源名（原名-大写，且不带后缀）: Efg 
/// * 资源路劲名：Assets/GameRes/Games/A/B/C/Efg.prefab  对应资源名： games/a/b/c/efg.unity3d 
/// 
/// # 概念：
/// * 资源路劲名："Assets/GameRes/Games/PssOn00152/Prefabs/SpriteIcon.prefab"
/// * 资源所在ab包名："games/psson00152/prefabs/spriteicon.unity3d"
/// * 本地资源缓存路劲： Application.persistentDataPath + "/GameRes" + "/" + "games/psson00152/prefabs/spriteicon.unity3d"
/// * 该资源从ab包名加载时，使用的名称："spriteicon"
/// </remark>
public class AssetBundleManager02 : MonoSingleton<AssetBundleManager02>
{

    /// <summary>  存储已经加载的 AB 包 （k:v == ab包名：ab包）</summary>
    private Dictionary<string, AssetBundle> loadedAssetBundles = new Dictionary<string, AssetBundle>();

    /// <summary>  存储每个 AB 包的引用计数 </summary>
    private Dictionary<string, int> referenceCounts = new Dictionary<string, int>();


    AssetBundleManifest _assetBundleManifest;
    /// <summary>  存储 AssetBundleManifest </summary>
    private AssetBundleManifest assetBundleManifest
    {
        get
        {
            if (_assetBundleManifest == null)
                LoadManifestBundle();
            return _assetBundleManifest;
        }
    }



    /// <summary> ab包根路径 </summary>
    // string abRootFolder => Application.persistentDataPath + "/GameRes";
    string abRootFolder => PathHelper.abDirLOCPTH;

    /// <summary> 主Manifest文件名称 </summary>
    string manifestBundleName => "GameRes";
    private void LoadManifestBundle()
    {
        _assetBundleManifest = null;

        string localManifestPath = Path.Combine(abRootFolder, manifestBundleName);

        if (File.Exists(localManifestPath))
        {

            AssetBundle manifestBundle = AssetBundle.LoadFromFile(localManifestPath);
            if (manifestBundle != null)
            {
                // 从加载的 AB 包中使用 LoadAsset 方法获取 AssetBundleManifest 对象。
                _assetBundleManifest = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                //调用 Unload(false) 方法卸载 AB 包，但保留已加载的 AssetBundleManifest 对象。
                manifestBundle.Unload(false);
            }
            else
            {
                DebugUtils.LogError("Failed to load manifest bundle.");
            }
        }
        else
        {
            DebugUtils.LogError("can not find manifest bundle.");
        }
    }

    /// <summary>
    /// 避免异步或协程，导致的ab并发加载问题。
    /// </summary>
    Dictionary<string,float> loadingBundles = new Dictionary<string, float>();

    //bool isLoadBundleing = false;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bundleName">games/a/b/c/efg.unity3d</param>
    /// <returns></returns>
    private IEnumerator _LoadAssetBundleAsync(string bundleName, UnityAction<AssetBundle> callback)
    {
        //#seaweed# 新增 - 避免并发（异步 或 协程 ，会出现并发加载同个ab包的问题）
        //可能存在依赖包，并发加载（例如：A包需依赖包C ; B包需依赖包C）
      while (loadingBundles.ContainsKey(bundleName) && Time.unscaledTime - loadingBundles[bundleName] > 30)
        {
            DebugUtils.Save($"避免并发加载 bundleName:{bundleName}");
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.03f, 0.1f));
        }
        loadingBundles.Remove(bundleName);
        loadingBundles.Add(bundleName, Time.unscaledTime);
        System.Action<AssetBundle> _onComplete = (ab) => {
            loadingBundles.Remove(bundleName);
            callback?.Invoke(ab);
        };

        /* 
       //#seaweed# 新增 - 避免并发（异步 或 协程 ，会出现并发加载同个ab包的问题）有bug??
       while (isLoadBundleing == true)
       {
           //DebugUtils.LogWarning($"避免并发加载 bundleName:{bundleName}");
           yield return new WaitForSeconds(UnityEngine.Random.Range(0.03f, 0.1f));
       }
       isLoadBundleing = true;
       System.Action<AssetBundle> _onComplete = (ab) => {
           isLoadBundleing = false;
           callback?.Invoke(ab);
       };
      */




        // 增加引用计数
        if (!referenceCounts.ContainsKey(bundleName))
        {
            referenceCounts[bundleName] = 0;
        }
        referenceCounts[bundleName]++;

        // 加载依赖包
        string[] dependencies = assetBundleManifest.GetAllDependencies(bundleName);
        foreach (string dependency in dependencies)
        {
            if (!loadedAssetBundles.ContainsKey(dependency))
            {
                if (!referenceCounts.ContainsKey(dependency))
                {
                    referenceCounts[dependency] = 0;
                }
                referenceCounts[dependency]++;

                // AssetBundle dependencyBundle = AssetBundle.LoadFromFile(getRootFolder + "/" + dependency); //同步

                string path = abRootFolder + "/" + dependency;
                AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);  // 异步
                yield return request;
                AssetBundle dependencyBundle = request.assetBundle;


                if (dependencyBundle != null)
                {
                    loadedAssetBundles.Add(dependency, dependencyBundle);
                    DebugUtils.Log("Loaded dependency: " + dependency);
                }
                else
                {
                    DebugUtils.LogError("Failed to load dependency: " + dependency);
                }
            }
            else
            {
                referenceCounts[dependency]++;
            }
        }

        // 加载目标 AB 包
        if (!loadedAssetBundles.ContainsKey(bundleName))
        {
            // AssetBundle targetBundle = AssetBundle.LoadFromFile(getRootFolder + "/" + bundleName);

            string path = abRootFolder + "/" + bundleName;
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);
            yield return request;
            AssetBundle targetBundle = request.assetBundle;

            if (targetBundle != null)
            {
                loadedAssetBundles.Add(bundleName, targetBundle);
                DebugUtils.Log("Loaded asset bundle: " + bundleName);
            }
            else
            {
                DebugUtils.LogError("[1] Failed to load asset bundle: " + bundleName);
            }
        }

        //callback?.Invoke(loadedAssetBundles.ContainsKey(bundleName) ? loadedAssetBundles[bundleName] : null);
        _onComplete.Invoke(loadedAssetBundles.ContainsKey(bundleName) ? loadedAssetBundles[bundleName] : null);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="assetPathOrBundleName">
    /// * "Assets/GameRes/Games/PssOn00152/Prefabs/SpriteIcon.prefab" 或 
    /// * "games/psson00152/prefabs/spriteicon.unity3d"
    /// </param>
    /// <returns>"games/psson00152/prefabs/spriteicon.unity3d"</returns>
    /// <remarks>
    /// suffix ：后缀
    /// postfix : 前缀
    /// </remarks>
    public string GetBundleName(string assetPathOrBundleName)
    {
        string result = assetPathOrBundleName.ToLower();

        string prefixToRemove = "Assets/GameRes/".ToLower();

        if (result.StartsWith(prefixToRemove))
        {
            result = result.Substring(prefixToRemove.Length);  //去掉 "assets/gameres/"
        }
        string[] str = result.Split('/');
        string fileNameSuffix = str[str.Length - 1];
        if (fileNameSuffix.Contains("."))
        {
            string[] str01 = fileNameSuffix.Split('.');
            int leg = str01[str01.Length - 1].Length + 1;  //".png 或 .prefab"
            result = result.Substring(0, result.Length - leg);
        }
        result += ".unity3d";

        return result;
    }


    public AssetBundle LoadAssetBundle(string bundleName)
    {

        // 增加引用计数
        if (!referenceCounts.ContainsKey(bundleName))
        {
            referenceCounts[bundleName] = 0;
        }
        referenceCounts[bundleName]++;

        // 加载依赖包
        string[] dependencies = assetBundleManifest.GetAllDependencies(bundleName);
        foreach (string dependency in dependencies)
        {
            if (!loadedAssetBundles.ContainsKey(dependency))
            {
                if (!referenceCounts.ContainsKey(dependency))
                {
                    referenceCounts[dependency] = 0;
                }
                referenceCounts[dependency]++;

                string path = abRootFolder + "/" + dependency;
                AssetBundle dependencyBundle = AssetBundle.LoadFromFile(path); //同步

                if (dependencyBundle != null)
                {
                    loadedAssetBundles.Add(dependency, dependencyBundle);
                    DebugUtils.Log("Loaded dependency: " + dependency);
                }
                else
                {
                    DebugUtils.LogError("Failed to load dependency: " + dependency);
                }
            }
            else
            {
                referenceCounts[dependency]++;
            }
        }

        // 加载目标 AB 包
        if (!loadedAssetBundles.ContainsKey(bundleName))
        {
            string path = abRootFolder + "/" + bundleName;
            AssetBundle targetBundle = AssetBundle.LoadFromFile(path); //同步（这里会并发加载同个ab包？  因为是接口是同步应该不会出现！！） 

            if (targetBundle != null)
            {
                loadedAssetBundles.Add(bundleName, targetBundle);
                DebugUtils.Log("Loaded asset bundle: " + bundleName);
            }
            else
            {
                DebugUtils.LogError("[2] Failed to load asset bundle: " + bundleName);
            }
        }

        return loadedAssetBundles.ContainsKey(bundleName) ? loadedAssetBundles[bundleName] : null;
    }




    /// <summary>
    /// 
    /// </summary>
    /// <param name="bundleName"></param>
    /// <param name="unloadAllLoadedObjects">卸载 AssetBundle 本身，但不会卸载已经从该 AssetBundle 中加载的资源。</param>
    public void UnloadAssetBundle(string bundleName, bool unloadAllLoadedObjects = false)
    {
        if (referenceCounts.ContainsKey(bundleName))
        {
            referenceCounts[bundleName]--;
            if (referenceCounts[bundleName] <= 0)
            {
                // 卸载依赖包
                string[] dependencies = assetBundleManifest.GetAllDependencies(bundleName);
                foreach (string dependency in dependencies)
                {
                    if (referenceCounts.ContainsKey(dependency))
                    {
                        referenceCounts[dependency]--;
                        if (referenceCounts[dependency] <= 0)
                        {
                            if (loadedAssetBundles.ContainsKey(dependency))
                            {
                                loadedAssetBundles[dependency].Unload(unloadAllLoadedObjects);
                                loadedAssetBundles.Remove(dependency);
                                referenceCounts.Remove(dependency);
                                DebugUtils.Log("Unloaded dependency: " + dependency);
                            }
                        }
                    }
                }

                // 卸载目标 AB 包
                if (loadedAssetBundles.ContainsKey(bundleName))
                {
                    loadedAssetBundles[bundleName].Unload(unloadAllLoadedObjects);
                    loadedAssetBundles.Remove(bundleName);
                    referenceCounts.Remove(bundleName);
                    DebugUtils.Log("Unloaded asset bundle: " + bundleName);
                }
            }
        }
    }


    protected override void OnDestroy()
    {
        // 卸载所有已加载的 AB 包
        foreach (KeyValuePair<string, AssetBundle> pair in loadedAssetBundles)
        {
            pair.Value.Unload(true);
        }
        loadedAssetBundles.Clear();
        referenceCounts.Clear();

        base.OnDestroy();
    }



    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="bundleName">"games/a/b/c/efg.unity3d"</param>
    /// <param name="assetName">"efg"</param>
    /// <returns></returns>
    public T GetAssetBundleObject<T>(string bundleName, string assetName) where T : UnityEngine.Object
    {
        // DebugUtil.Log("拿AB key name = " + key);
        if (loadedAssetBundles.ContainsKey(bundleName))
        {
            return loadedAssetBundles[bundleName].LoadAsset<T>(assetName);
            DebugUtils.Log($"ab包：{bundleName} 没有资源 ： {assetName}");
        }
        DebugUtils.Log($" 当获取资源：{assetName}时，没有先加载ab包：{bundleName}。");
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="bundleName">"games/a/b/c/efg.unity3d"</param>
    /// <returns></returns>
    public T GetAssetBundleObject<T>(string bundleName) where T : UnityEngine.Object
    {
        string[] str = bundleName.Split('/');
        string assetName = str[str.Length - 1].Replace(".unity3d", "");

        if (loadedAssetBundles.ContainsKey(bundleName))
        {
            return loadedAssetBundles[bundleName].LoadAsset<T>(assetName);
        }
        DebugUtils.Log($" 当获取资源：{assetName}时，没有先加载ab包：{bundleName}。");
        return null;
    }

    public void GetAssetBundleObjectAsync<T>(string bundleName, Action<T> onFinishCallback) where T : UnityEngine.Object
    {
        StartCoroutine(_GetAssetBundleObjectAsync<T>(bundleName, onFinishCallback));
    }

    IEnumerator _GetAssetBundleObjectAsync<T>(string bundleName, Action<T> onFinishCallback = null) where T : UnityEngine.Object
    {
        string[] str = bundleName.Split('/');
        string assetName = str[str.Length - 1].Replace(".unity3d", "");
        T loadedAsset = null;
        if (loadedAssetBundles.ContainsKey(bundleName))
        {
            // 2. 异步加载指定类型的资源
            AssetBundleRequest request = loadedAssetBundles[bundleName].LoadAssetAsync<T>(assetName);

            // 3. 等待加载完成
            yield return request;

            // 获取加载的资源
            //UnityEngine.Object loadedAsset = request.asset as T;
            loadedAsset = request.asset as T;
        }
        //DebugUtil.Log($" 当获取资源：{assetName}时，没有先加载ab包：{bundleName}。");
        onFinishCallback?.Invoke(loadedAsset);
        yield return loadedAsset;
    }


    public void GetAssetBundleAsync(string bundleName, UnityAction<AssetBundle> callback)
    {
        StartCoroutine(_LoadAssetBundleAsync(bundleName, callback));
    }

}