using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;
using UnityEditor;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public partial class ResourceManager02 : MonoSingleton<ResourceManager02>
{
    int _number;
    int getNumber()
    {
        if (++_number > 10000) // Time.unscaledTime * 1000 +  _number
            _number = 0;
        return _number;
    }
}

public partial class ResourceManager02 : MonoSingleton<ResourceManager02>
{
    /// <summary> 预加载对象 </summary>
    Dictionary<string, UnityEngine.Object> preloadAssetAtPath = new Dictionary<string, UnityEngine.Object>();

    /// <summary>
    /// 删除预加载的对象
    /// </summary>
    /// <param name="assetPath"></param>
    public void UnloadPreloadAsset(string assetPath)
    {
        if (preloadAssetAtPath.ContainsKey(assetPath))
            preloadAssetAtPath.Remove(assetPath);
    }

    /// <summary>
    /// 预加载对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assetPath"></param>
    /// <param name="callback"></param>
    public void LoadPreloadAssetAsync(string assetPath, UnityAction<UnityEngine.Object> callback)// where T : UnityEngine.Object
    {
        string markBundle = $"{assetPath}#{getNumber()}";
        LoadAssetAtPathAsync<UnityEngine.Object>(assetPath, (obj) =>
        {
            UnloadAssetBundle(markBundle);
            if (!preloadAssetAtPath.ContainsKey(assetPath))
                preloadAssetAtPath.Add(assetPath, null);
            preloadAssetAtPath[assetPath] = obj;

            callback.Invoke(obj);
        }, markBundle);
    }


    T UsePreloadAsset<T>(string assetPath) where T : UnityEngine.Object
    {
        if (preloadAssetAtPath.ContainsKey(assetPath))
        {
            if (preloadAssetAtPath[assetPath] == null)
            {
                preloadAssetAtPath.Remove(assetPath);
            }
            else
            {
                T res = preloadAssetAtPath[assetPath] as T;
                //if (IsGameObjectType<T>()) res = Object.Instantiate(res);
                return res;
            }
        }
        return null;
    }
}



public partial class ResourceManager02 : MonoSingleton<ResourceManager02>
{

    /// <summary> 标记加载的ab包 </summary>
    Dictionary<string, List<string>> markLoadBundle = new Dictionary<string, List<string>>();


    /// <summary>
    /// 删除mark标记到的所有ab包
    /// </summary>
    /// <param name="markBundle"></param>
    public void UnloadAssetBundle(string markBundle)
    {

        if (ApplicationSettings.Instance.IsUseHotfixBundle()) //加载热更新ab包资源
        {
            if (markLoadBundle.ContainsKey(markBundle))
            {
                List<string> target = markLoadBundle[markBundle];
                markLoadBundle.Remove(markBundle);
                foreach (string assetPath in target)
                {
                    string bundleName = AssetBundleManager02.Instance.GetBundleName(assetPath);
                    AssetBundleManager02.Instance.UnloadAssetBundle(bundleName);
                }
            }
        }
        else if (ApplicationSettings.Instance.IsUseStreamingAssetsBundle())
        {
            if (markLoadBundle.ContainsKey(markBundle))
            {
                List<string> target = markLoadBundle[markBundle];
                markLoadBundle.Remove(markBundle);
                foreach (string assetPath in target)
                {
                    StreamingAssetsBundleLoader.Instance.UnloadBundle(assetPath, false);
                }
            }
        }
    }


    /// <summary>
    /// 异步加载ab包
    /// </summary>
    /// <param name="assetPathOrBundleName"></param>
    /// <param name="markBundle"></param>
    /// <param name="callback"></param>
    public void LoadAssetBundleAsync(string assetPathOrBundleName, UnityAction<AssetBundle> callback, string markBundle = "Default")
    {
        if (ApplicationSettings.Instance.IsUseHotfixBundle()) //加载热更新ab包资源
        {
            string bundleName = AssetBundleManager02.Instance.GetBundleName(assetPathOrBundleName);
            AssetBundleManager02.Instance.GetAssetBundleAsync(bundleName, (ab) =>
            {
                MarkBundle(markBundle, assetPathOrBundleName);
                callback?.Invoke(ab);
            });
        }
        else if (ApplicationSettings.Instance.IsUseStreamingAssetsBundle())
        {
            StreamingAssetsBundleLoader.Instance.LoadAssetBundleAsync(assetPathOrBundleName, (ab) =>
            {
                MarkBundle(markBundle, assetPathOrBundleName);
                callback?.Invoke(ab);
            });
        }
        else
        {
            Debug.LogError("直接读项目里非StreamAssets里的ab包，没有实现！");
        }
    }



    [Button]
    string GetAssetBundleFilePathByAssetBundleName(string assetBundleName = "games/psson00152 (1080x1920)/abs/sprites/symbol icon/symbol_00.unity3d")
    {
        string tareget = null;

#if UNITY_EDITOR
        // 检查输入的 AssetBundle 名称是否为空
        if (string.IsNullOrEmpty(assetBundleName))
        {
            Debug.LogError("AssetBundle name is null or empty.");
            return null;
        }

        // 获取该 AssetBundle 包含的所有资源的路径
        string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);

        // 如果没有找到相关资源，输出错误信息并返回 null
        if (assetPaths.Length == 0)
        {
            Debug.LogError($"No assets found in AssetBundle: {assetBundleName}");
            return null;
        }
        else if (assetPaths.Length == 1)
        {
            tareget = assetPaths[0];
            Debug.Log($"Asset Path: {tareget}");
        }
        else
        {
            Debug.LogError($"This is not a Asset corresponding to an AssetBundle : {assetBundleName}");
        }

#endif
        return tareget;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="assetPathOrBundleName"></param>
    /// <returns></returns>
    /// <remark>
    /// 针对模糊路劲 "Games/PssOn00152 (1080x1920)/ABs/Sprites/Symbol Icon/symbol_00"
    /// 或 "Assets/GameRes/Games/PssOn00152 (1080x1920)/ABs/Sprites/Symbol Icon/symbol_00.png"
    /// 或 "Assets/GameRes/Games/PssOn00152 (1080x1920)/ABs/Sprites/Symbol Icon/symbol_00"
    /// 或 "games/psson00152 (1080x1920)/abs/sprites/symbol icon/symbol_00.unity3d"
    /// </remark>
    string GetAssetBundleFilePath(string assetPathOrBundleName)
    {
        string assetFullPath = null;
#if UNITY_EDITOR
        if (assetPathOrBundleName.EndsWith(".unity3d")) // "games/psson00152 (1080x1920)/abs/sprites/symbol icon/symbol_00.unity3d"
        {
            assetFullPath = GetAssetBundleFilePathByAssetBundleName(assetPathOrBundleName);
            if (assetFullPath == null)
                return null;
        }
        else
        {
            // 针对模糊路劲 "Games/PssOn00152 (1080x1920)/ABs/Sprites/Symbol Icon/symbol_00"
            // 或  "Assets/GameRes/Games/PssOn00152 (1080x1920)/ABs/Sprites/Symbol Icon/symbol_00.png"
            // 或 "Assets/GameRes/Games/PssOn00152 (1080x1920)/ABs/Sprites/Symbol Icon/symbol_00"
            string assetPath = assetPathOrBundleName;

            if (!assetPath.StartsWith("Assets/GameRes/"))
                assetPath = assetPath + "Assets/GameRes/";

            string assetNameNoSuffix = assetPath.Split('/')[assetPath.Split('/').Length - 1];
            string directoryName = assetPath.Substring(0, assetPath.Length - $"/{assetNameNoSuffix}".Length); //删除最后的$"/{assetName}"
            if (assetNameNoSuffix.Contains("."))
            {
                string[] str01 = assetNameNoSuffix.Split('.');
                int leg = str01[str01.Length - 1].Length + 1;  //".png 或 .prefab"
                assetNameNoSuffix = assetNameNoSuffix.Substring(0, assetNameNoSuffix.Length - leg);
            }

            //Debug.Log($"加载u3d编辑器中的ab包资源：name {name}   searchInFolders = {directoryName}");
            string[] assets = UnityEditor.AssetDatabase.FindAssets(assetNameNoSuffix, new[] { directoryName });

            //作为精确文件名过滤字符，避免name包含subName的资源被锁定。
            string namefilter = assetNameNoSuffix + ".";
            foreach (var temp in assets)
            {
                var fullPath = UnityEditor.AssetDatabase.GUIDToAssetPath(temp);
                if (fullPath.Contains(namefilter))
                {
                    assetFullPath = fullPath;
                    break;
                }
            }
        }
#endif
        return assetFullPath;
    }

    /// <summary>
    ///  标记加载的ab包
    /// </summary>
    /// <param name="markBundle"></param>
    /// <param name="assetPathOrBundleName"></param>
    void MarkBundle(string markBundle, string assetPathOrBundleName)
    {

        if (!string.IsNullOrEmpty(markBundle))  // 标记加载的资源
        {
            if (!markLoadBundle.ContainsKey(markBundle))
                markLoadBundle.Add(markBundle, new List<string>());
            markLoadBundle[markBundle].Add(assetPathOrBundleName);
        }
        else
        {
            Debug.LogError($"【ResourceManager】mark is null; when ab = {assetPathOrBundleName}");
        }
    }


    public bool IsGameObjectType<T>()
    {
        // 使用 typeof 操作符获取 GameObject 的类型
        // 使用 typeof(T) 获取泛型 T 的类型
        // 使用 IsAssignableFrom 方法判断 T 是否可以赋值给 GameObject 类型
        return typeof(GameObject).IsAssignableFrom(typeof(T));
    }

    public void LoadAsset<T>(string assetPathOrBundleName, System.Action<T> onFinishCallback, string markBundle = "Default") where T : UnityEngine.Object
    {
        LoadAssetAtPathAsync(assetPathOrBundleName, onFinishCallback, markBundle);
    }

    public void LoadAssetAtPathAsync<T>(string assetPathOrBundleName, System.Action<T> onFinishCallback, string markBundle = "Default") where T : UnityEngine.Object
    {
        T res = UsePreloadAsset<T>(assetPathOrBundleName);
        if (res != null)
        {
            onFinishCallback.Invoke(res);
            return;
        }

        if (ApplicationSettings.Instance.IsUseHotfixBundle()) //加载热更新ab包资源
        {
            string bundleName = AssetBundleManager02.Instance.GetBundleName(assetPathOrBundleName);
            AssetBundleManager02.Instance.GetAssetBundleAsync(bundleName, (ab) =>
            {
                MarkBundle(markBundle, bundleName);
                AssetBundleManager02.Instance.GetAssetBundleObjectAsync<T>(bundleName, (target) =>
                {
                    //if (IsGameObjectType<T>())  target = Object.Instantiate(target);
                    onFinishCallback.Invoke(target);
                });
            });
        }
        else if (ApplicationSettings.Instance.IsUseStreamingAssetsBundle())
        {

            StreamingAssetsBundleLoader.Instance.LoadAsset<T>(assetPathOrBundleName, (target) =>
            {
                MarkBundle(markBundle, assetPathOrBundleName);
                //if (IsGameObjectType<T>()) target = Object.Instantiate(target);
                onFinishCallback.Invoke(target);
            });
        }
        else
        {
#if UNITY_EDITOR

            T target = null;
            string assetPath = GetAssetBundleFilePath(assetPathOrBundleName);
            if (assetPath != null)
                target = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);

            //if (IsGameObjectType<T>())  target = Object.Instantiate(target);
            onFinishCallback.Invoke(target);

#endif
        }
    }



}



public partial class ResourceManager02 : MonoSingleton<ResourceManager02>
{

    public async Task<T> LoadAssetAsync<T>(string assetPathOrBundleName, string markBundle = "Default") where T : Object
    {
        T res = UsePreloadAsset<T>(assetPathOrBundleName);
        if (res != null)
        {
            return res;
        }

        if (ApplicationSettings.Instance.IsUseHotfixBundle())
        {
            string bundleName = AssetBundleManager02.Instance.GetBundleName(assetPathOrBundleName);
            var tcs = new TaskCompletionSource<T>();

            AssetBundleManager02.Instance.GetAssetBundleAsync(bundleName, (ab) =>
            {
                MarkBundle(markBundle, bundleName);
                AssetBundleManager02.Instance.GetAssetBundleObjectAsync<T>(bundleName, (target) =>
                {
                    //if (IsGameObjectType<T>()) target = Object.Instantiate(target);

                    if (target != null)
                        tcs.SetResult(target);
                    else
                        tcs.SetException(new System.Exception($"Failed to load Asset: {assetPathOrBundleName}"));

                });
            });
            
            T target01 = await tcs.Task;
            return target01;

        }
        else if (ApplicationSettings.Instance.IsUseStreamingAssetsBundle())
        {
            T target01 = await StreamingAssetsBundleLoader.Instance.LoadAssetAsync<T>(assetPathOrBundleName);
            
            string bundleName = StreamingAssetsBundleLoader.Instance.GetBundleName(assetPathOrBundleName);
            MarkBundle(markBundle, bundleName);

            //if (IsGameObjectType<T>()) target01 = Object.Instantiate(target01);
            return target01;
        }
        else
        {
            Debug.LogError("未实现"); 
            return null;
        }
    }


    public async Task<AssetBundle> LoadAssetBundleAsync(string assetPathOrBundleName,string markBundle="Default")
    {
        if (ApplicationSettings.Instance.IsUseHotfixBundle())
        {
            string bundleName = AssetBundleManager02.Instance.GetBundleName(assetPathOrBundleName);
            var tcs = new TaskCompletionSource<AssetBundle>();

            AssetBundleManager02.Instance.GetAssetBundleAsync(bundleName, (ab) =>
            {
                MarkBundle(markBundle, bundleName);
                if (ab != null)
                    tcs.SetResult(ab);
                else
                    tcs.SetException(new System.Exception($"Failed to load Asset: {assetPathOrBundleName}"));
            });

            AssetBundle target01 = await tcs.Task;
            return target01;

        }
        else if (ApplicationSettings.Instance.IsUseStreamingAssetsBundle())
        {
            AssetBundle target01 = await StreamingAssetsBundleLoader.Instance.LoadAssetBundleAsync(assetPathOrBundleName);
            MarkBundle(markBundle, assetPathOrBundleName);
            return target01;
        }
        else
        {
            Debug.LogError("未实现");
            return null;
        }

    }


}
