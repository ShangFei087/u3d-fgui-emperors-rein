using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using GameMaker;
using System.Threading;
using UnityEngine.Networking;
using UnityEditor;
using Sirenix.OdinInspector;
using UnityEngine.UI;


public partial class ResourceManager : MonoSingleton<ResourceManager>
{
    int _number;
    int getNumber()
    {
        if (++_number > 10000) // Time.unscaledTime * 1000 +  _number
            _number = 0;
        return _number;
    }

    public void OnEnable()
    {
        EventCenter.Instance.AddEventListener<EventData>(EventHandle.ON_RESOURCE_EVENT, OnResourceEvent);
    }

    public void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener<EventData>(EventHandle.ON_RESOURCE_EVENT, OnResourceEvent);
    }

    void OnResourceEvent(EventData res)
    {
        if (res.name == "TestShowMarkAB")
            TestShowMark();
    }


    [Button]
    void TestShowMark()
    {
        foreach (KeyValuePair<string, List<string>> kv in markLoadBundle)
        {
            foreach (string bundleName in kv.Value)
            {
                Debug.Log($" * mark = {kv.Key} : {bundleName}");
            }
        }
    }
}

public partial class ResourceManager : MonoSingleton<ResourceManager> {
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
        LoadAssetAtPathAsync<UnityEngine.Object>(assetPath, markBundle, (obj) =>
        {
            UnloadAssetBundle(markBundle);
            if (!preloadAssetAtPath.ContainsKey(assetPath))
                preloadAssetAtPath.Add(assetPath, null);
            preloadAssetAtPath[assetPath] = obj;

            callback.Invoke(obj);
        });
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
                if (IsGameObjectType<T>())  //if (res is GameObject)
                    res = Object.Instantiate(res);
                return res;
            }
        }
        return null;
    }
}



public partial class ResourceManager : MonoSingleton<ResourceManager> { 

    /// <summary> 标记加载的ab包 </summary>
    Dictionary<string, List<string>> markLoadBundle = new Dictionary<string, List<string>>();


    /// <summary>
    /// 删除mark标记到的所有ab包
    /// </summary>
    /// <param name="markBundle"></param>
    public void UnloadAssetBundle(string markBundle)
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

  
    /// <summary>
    /// 异步加载ab包
    /// </summary>
    /// <param name="assetPathOrBundleName"></param>
    /// <param name="markBundle"></param>
    /// <param name="callback"></param>
    public void LoadAssetBundleAsync(string assetPathOrBundleName, string markBundle, UnityAction<AssetBundle> callback)
    {
        string bundleName = AssetBundleManager02.Instance.GetBundleName(assetPathOrBundleName);
        AssetBundleManager02.Instance.GetAssetBundleAsync(bundleName, (ab)=>
        {
            MarkBundle(markBundle, assetPathOrBundleName);
            callback?.Invoke(ab);
        });
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
        else if(assetPaths.Length == 1)
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
    void MarkBundle(string markBundle,string assetPathOrBundleName)
    {

        if (!string.IsNullOrEmpty(markBundle))  // 标记加载的资源
        {
            if (!markLoadBundle.ContainsKey(markBundle))
                markLoadBundle.Add(markBundle, new List<string>());
            markLoadBundle[markBundle].Add(assetPathOrBundleName);
        }
        else
        {
            Debug.LogError ($"【ResourceManager】mark is null; when ab = {assetPathOrBundleName}");
        }
    }


    /* 
     public T Load<T>(string name) where T : Object
     {
         T res = null;

         if (Application.isEditor) //加载U3d编辑器中的ab包资源
         {
 #if UNITY_EDITOR 

             string path = "Assets/GameRes/" + name;

             string assetName = path.Split('/')[path.Split('/').Length - 1];
             string directoryName = path.Substring(0, path.Length - $"/{assetName}".Length); //删除最后的$"/{assetName}"
             //string directoryName = path.Replace($"/{assetName}", "");

             //Debug.Log($"加载u3d编辑器中的ab包资源：name {name}   searchInFolders = {directoryName}");

             string[] assets = UnityEditor.AssetDatabase.FindAssets(assetName, new[] { directoryName });
             string assetFullPath = "";

             //作为精确文件名过滤字符，避免name包含subName的资源被锁定。
             string namefilter = assetName + ".";
             foreach (var temp in assets)
             {
                 var fullPath = UnityEditor.AssetDatabase.GUIDToAssetPath(temp);
                 if (fullPath.Contains(namefilter))
                 {
                     assetFullPath = fullPath;
                     break;
                 }
             }
             res = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetFullPath);
 #endif

         }
         else //加载热更新ab包资源
         {
             string p = "";
             name = name.ToLower();
             p = Path.Combine(Application.persistentDataPath + "/GameRes", name);
             //Debug.Log("path = " + path);
             //Debug.Log("pppppppppppppp = " + p);
             string[] str = p.Split('/');
             string key = str[str.Length - 1];
             p = p + ".unity3d";
             //p = p.ToLower();
             Debug.Log($"加载热更新ab包资源: key = {key}, p = {p}");
             AssetBundleManager.Instance.AddAssetBundle(key, p);
             res = AssetBundleManager.Instance.GetAssetBundleObject<T>(key);
         }

         //如果对象是一个GameObject类型的 我把他实例化后 再返回出去 外部 直接使用即可
         if (res is GameObject)
             return Object.Instantiate(res);
         else//TextAsset AudioClip
             return res;
     }*/


    public  bool IsGameObjectType<T>()
    {
        // 使用 typeof 操作符获取 GameObject 的类型
        // 使用 typeof(T) 获取泛型 T 的类型
        // 使用 IsAssignableFrom 方法判断 T 是否可以赋值给 GameObject 类型
        return typeof(GameObject).IsAssignableFrom(typeof(T));
    }

    [Button]
    public void TestShowObject()
    {
        Debug.Log($"IsGameObjectType :{IsGameObjectType<UnityEngine.Object>()}");  // False
    }

    [Button]
    public void TestShowGameObject()
    {
        Debug.Log($"IsGameObjectType :{IsGameObjectType<UnityEngine.GameObject>()}"); //True
    }

    public void LoadAssetAtPathAsync<T>(string assetPathOrBundleName, string markBundle, System.Action<T> onFinishCallback) where T : UnityEngine.Object
    {
        T res =  UsePreloadAsset<T>(assetPathOrBundleName);
        if (res != null){
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
                    if (IsGameObjectType<T>())  //if (target is GameObject)
                        target = Object.Instantiate(target);
                    onFinishCallback.Invoke(target);
                });
            });
        }
        else
        {
#if UNITY_EDITOR
            
            T target = null;
            string assetPath = GetAssetBundleFilePath(assetPathOrBundleName);
            if (assetPath != null)
                target = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);

            if(IsGameObjectType<T>())  //if (target is GameObject)
                target = Object.Instantiate(target);
            onFinishCallback.Invoke(target);

#endif
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assetPathOrBundleName">"Assets/GameRes/Games/PssOn00152/Prefabs/SpriteIcon.prefab"</param>
    /// <returns></returns>
    public T LoadAssetAtPath<T>(string assetPathOrBundleName, string markBundle) where T : UnityEngine.Object
    {
        T res = UsePreloadAsset<T>(assetPathOrBundleName);
        if (res != null)
        {
            return res;
        }

        T target = null;
        if (ApplicationSettings.Instance.IsUseHotfixBundle())
        {
            string bundleName = AssetBundleManager02.Instance.GetBundleName(assetPathOrBundleName);
            AssetBundleManager02.Instance.LoadAssetBundle(bundleName);
            MarkBundle(markBundle, assetPathOrBundleName);
            target = AssetBundleManager02.Instance.GetAssetBundleObject<T>(bundleName);
        }
        else
        {
#if UNITY_EDITOR
            /*target = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);*/

            string assetPath = GetAssetBundleFilePath(assetPathOrBundleName);
            if (assetPath != null)
                target = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
#endif
        }
        if (IsGameObjectType<T>())  //if (target is GameObject)
            return Object.Instantiate(target);
        else//TextAsset AudioClip
            return target;
    }




    public T LoadAssetAtPathOnce<T>(string assetPath) where T : Object
    {
        T res = UsePreloadAsset<T>(assetPath);
        if (res != null)
        {
            return res;
        }

        string markBundle = $"{assetPath}#{getNumber()}";
        T target = LoadAssetAtPath<T>(assetPath, markBundle);
        if (ApplicationSettings.Instance.IsUseHotfixBundle())
        {
            string bundleName = AssetBundleManager02.Instance.GetBundleName(assetPath);
            AssetBundleManager02.Instance.UnloadAssetBundle(bundleName);
            markLoadBundle.Remove(markBundle);
        }
        return target;
    }


    /// <summary>
    /// 这个还么测试通过
    /// </summary>
    /// <param name="name"></param>
    /// <param name="action"></param>
    public void DD_LoadABOnSubThread(string name, UnityAction<AssetBundle> action)
    {
        string p = "";
        name = name.ToLower();
        p = Path.Combine(Application.persistentDataPath + "/GameRes", name);
        string[] str = p.Split('/');
        string key = str[str.Length - 1];
        p = p + ".unity3d";

        Loom.RunAsync(() =>
        {
            UnityWebRequest request = UnityWebRequest.Get((string)p);
            request.SendWebRequest();
            while (!request.isDone)
            {
                Thread.Sleep(100);
            }
            AssetBundle assetBundle = AssetBundle.LoadFromMemory(request.downloadHandler.data);
            Loom.QueueOnMainThread((res) =>
            {
                action(res as AssetBundle);
            }, assetBundle);
        });
    }


}
