#define NEW_VER_DLL
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleJSON;
using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;


/// <summary>
/// 
/// </summary>
/// <remarks>
/// * 问题：total_version文件文件内容为空（下载并写入total_version文件，写到一半突然断电。）
/// </remarks>

public class VersionCheck002 : MonoBehaviour
{

    static VersionCheck002 instance;
    public static VersionCheck002 Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType(typeof(VersionCheck002)) as VersionCheck002;
            }
            return instance;
        }
    }


    private void OnDestroy()
    {
        CloseCoCheckHotFixThrowErr();
    }


    const string PREFIX_TIP = "【Hotfix Step】";


    const string HOTFIX_STATE = "HOTFIX_STATE";
    const string HotfixStateCompleted = "Completed";
    const string HotfixStateCopying = "Copying";
    const string HotfixDownloading = "HotfixDownloading";
    const string HotfixDownloadFail = "HotfixDownloadFail";
    /// <summary> 总版本管理-子节点数据 </summary>

    //byte[] versionDataRemoteByte;
    JObject versionFileRemoteNode;


    //bool hasInternet = false;



    /// <summary> 热更重复请求次数 </summary>
    int hotfixRequestCount
    {
        get
        {
            // 受到保护时默认开启联网
            if (GlobalData.isFirstInstall && ApplicationSettings.Instance.isUseProtectApplication)
            {
                // 旧包已经有了字段  HOTFIX_REQUEST_COUNT
                // 第一次装包时，要强制保存，开启联网
                PlayerPrefs.SetInt(GlobalData.HOTFIX_REQUEST_COUNT_01, 10);  
                PlayerPrefs.Save();
                return 3;
            }

            //#seaweed# int defaultCount = ApplicationSettings.Instance.isUseProtectApplication ? 10 : 1;
            int defaultCount = 3;
            if (!PlayerPrefs.HasKey(GlobalData.HOTFIX_REQUEST_COUNT_01))
                PlayerPrefs.SetInt(GlobalData.HOTFIX_REQUEST_COUNT_01, defaultCount);
            int count = PlayerPrefs.GetInt(GlobalData.HOTFIX_REQUEST_COUNT_01, defaultCount);
            return count < 1 ? 1 : count;
         
        }
    }



    /// <summary> 获取远端资源备份的hash /// </summary>
    string GetWebAssetBackupHash(string nodeName) => versionFileRemoteNode["asset_backup"][nodeName]["hash"].ToObject<string>();

    /// <summary> 获取本地dll文件的hash  </summary>
    string GetLocalAssetBackupHash(string nodeName) => GlobalData.version["asset_backup"][nodeName]["hash"].ToObject<string>();

    /// <summary>
    /// 获取远端dll文件的hash
    /// </summary>
    /// <param name="dllName"></param>
    /// <returns></returns>
    string GetWebHotfixDllHash(string dllName) => versionFileRemoteNode["hotfix_dll"][dllName]["hash"].ToObject<string>();



    /// <summary>
    /// 获取本地dll文件的hash
    /// </summary>
    /// <param name="dllName"></param>
    /// <returns></returns>
    string GetLocalHotfixDllHash(string dllName) => GlobalData.version["hotfix_dll"][dllName]["hash"].ToObject<string>();



    void GetLocalVersionInfo()
    {
        string showMsg = $"read local version file   path: {PathHelper.versionLOCPTH}";
        PageLaunch.Instance.RefreshProgressUIMsg(showMsg);

        Debug.LogWarning($"{PREFIX_TIP} {showMsg}");

        string versionText = "";
        string totalVersionText = "";

        try
        {
            versionText = File.ReadAllText(PathHelper.versionLOCPTH);
            GlobalData.version = JObject.Parse(versionText);
        }
        catch (Exception ex)
        {
            if (File.Exists(PathHelper.totalVersionLOCPTH))
            {
                Debug.LogError($"can not find file: {PathHelper.versionLOCPTH}");
            }

            throwErrMsg = $"read local version file  err: {ex.Message} ; path: {PathHelper.versionLOCPTH} ; version text: {versionText}";
            PageLaunch.Instance.Error(throwErrMsg);
            Debug.LogError(throwErrMsg);
            throw ex;
        }

        GlobalData.totalVersion = null;
        GlobalData.autoHotfixUrl = "";

        if (File.Exists(PathHelper.totalVersionLOCPTH))
        {
            try
            {
                totalVersionText = File.ReadAllText(PathHelper.totalVersionLOCPTH);  //【存在bug】 这里total文件是个空！！
                GlobalData.totalVersion = JObject.Parse(totalVersionText);

                JObject targetTotalVersionItem = null;
                JArray lst = GlobalData.totalVersion["data"] as JArray;
                for (int i = 0; i < lst.Count; i++)
                {
                    string appKey = lst[i]["app_key"].ToObject<string>();
                    if (appKey == ApplicationSettings.Instance.appKey)
                    {
                        targetTotalVersionItem = lst[i] as JObject;
                        break;
                    }
                }
                if (targetTotalVersionItem != null)
                {
                    GlobalData.autoHotfixUrl = FileUtils.GetDirWebUrl(PathHelper.totalVersionWEBURL, targetTotalVersionItem["hotfix_url"].ToObject<string>());
                }
                else
                {
                    Debug.LogWarning($"cant not find app key:{ApplicationSettings.Instance.appKey} in local total version");
                }
            }
            catch (Exception ex)
            {
                throwErrMsg = $"read local total version file  err: {ex.Message} ; path: {PathHelper.totalVersionLOCPTH} ; version text: {totalVersionText}";
                PageLaunch.Instance.Error(throwErrMsg);
                Debug.LogError(throwErrMsg);
                throw ex;
            }
        }

    }

    #region 检查热更异常失败卡死
    const string HOTFIX_THROW_ERR = "HOTFIX_THROW_ERR";
    string throwErrMsg = "";
    Coroutine coCheckHotfixThrowErr;
    IEnumerator CheckHotfixThrowErr()
    {
        yield return new WaitForSecondsRealtime(100f);  // 5 * 60 （20分钟）
        PlayerPrefs.SetString(HOTFIX_THROW_ERR, throwErrMsg);
        PlayerPrefs.Save();
        Debug.LogError($"save hotfix throw err : {throwErrMsg}");

        if(!ApplicationSettings.Instance.isRelease)
            PageLaunch.Instance.Error(throwErrMsg);
    }
    void StartCheckHotfixThrowErr()
    {
        CloseCoCheckHotFixThrowErr();
        coCheckHotfixThrowErr = StartCoroutine(CheckHotfixThrowErr());
    }
    void CloseCheckHotfixThrowErr()
    {
        CloseCoCheckHotFixThrowErr();
        ClearHotfixThrowErrFlg();
    }
    /// <summary> 清除热更标志位 </summary>
    void ClearHotfixThrowErrFlg()
    {
        PlayerPrefs.DeleteKey(HOTFIX_THROW_ERR);
        PlayerPrefs.Save();
    }
    void CloseCoCheckHotFixThrowErr()
    {
        if (coCheckHotfixThrowErr != null) 
            StopCoroutine(coCheckHotfixThrowErr);
        coCheckHotfixThrowErr = null;
    }
    #endregion




    /// <summary>
    /// 检查并热更新资源
    /// </summary>
    /// <param name="callback"></param>
    /// <returns></returns>
    public IEnumerator DoHotfix(UnityAction callback)
    {

        bool isLastHotfixThrowErr = PlayerPrefs.HasKey(HOTFIX_THROW_ERR);
        if (isLastHotfixThrowErr)
        {
            Debug.LogError($"last hotfix throw err: {PlayerPrefs.GetString(HOTFIX_THROW_ERR)}");
        }

        // 【解決热更异常】
        StartCheckHotfixThrowErr();
       
        UnityAction proxyCallback = () =>
        {
            CloseCheckHotfixThrowErr();
            callback?.Invoke();
        };


        if (PlayerPrefs.HasKey(HOTFIX_STATE))
        {
            Debug.LogWarning($"last hotfix state: {PlayerPrefs.GetString(HOTFIX_STATE)}");
        }

        ClearRemoteInfo();

        // 获取包内版本
        yield return GetStreamingAssetsVersion();

        // 编辑器不使用热更
        if (!ApplicationSettings.Instance.IsUseHotfixBundle())
        {
            yield return FileUtils.ReadStreamingAsset<string>(
                PathHelper.versionSAPTH,
                (obj) =>
                {
                    GlobalData.version = JObject.Parse((string)obj);
                }, null);
            Debug.Log($"is use streaming assets version");
            // 回调
            proxyCallback?.Invoke();
            yield break;
        }


        // 判断是否是首次安装
        bool isFirstInstall = GlobalData.isFirstInstall;

        // 首次装包拷贝文件
        if (isFirstInstall || !File.Exists(PathHelper.versionLOCPTH) || isLastHotfixThrowErr)
        {

            ClearHotfixThrowErrFlg();

            yield return CopyStreamingAssetsToPersistentDataPath();
        }

        PageLaunch.Instance.AddProgressCount(LoadingProgress.CHECK_COPY_TEMP_HOTFIX_FILE, 1);
        PageLaunch.Instance.Next(LoadingProgress.CHECK_COPY_TEMP_HOTFIX_FILE, $"check cache : temp hotfix file");

        // 检查要拷贝的文件
        yield return CopyTempWebHotfixFileToTargetDir();

        PageLaunch.Instance.RemoveProgress(LoadingProgress.CHECK_COPY_TEMP_HOTFIX_FILE);

        // 获取本地配置文件
        GetLocalVersionInfo();



        /// 【获取版本信息】多次请求，避免机台上电后，wifi要延时才链接。
        Debug.LogWarning($"{PREFIX_TIP} getting web version files fails");
        bool isGetWebVersionSuccess = false;
        int count = hotfixRequestCount;
        int i = 0;
        while (true)
        {
            yield return GetWebTotalVersionAndVersion(() => isGetWebVersionSuccess = true, () => isGetWebVersionSuccess = false);

            if (isGetWebVersionSuccess || --count < 1)
                break;
            else {
                Debug.LogWarning($"getting web version files fails, count: {++i}  time: {Time.unscaledTime}");
                yield return new WaitForSecondsRealtime(2.5f);
            }
        }

        Debug.Log($"is get web version file = {isGetWebVersionSuccess}");


        if (!isGetWebVersionSuccess)
        {
            // 回调
            proxyCallback?.Invoke();
            yield break;
        }
        else
        {

            string localVersionVER = GlobalData.version["hotfix_version"].Value<string>();
            string webVersionVER = versionFileRemoteNode["hotfix_version"].Value<string>();
            Debug.Log($"local version file ver:{localVersionVER}  --  web version file ver:{webVersionVER}");
            int[] localVersions = GetVersions(localVersionVER);
            int[] serverVersions = GetVersions(webVersionVER);

            if (localVersions[0] == serverVersions[0])
            {

                if (localVersions[1] != serverVersions[1] || localVersions[2] != serverVersions[2])
                {
                    PlayerPrefs.SetString(HOTFIX_STATE, HotfixDownloading);

                    if (PlayerPrefs.GetString(HOTFIX_STATE) != HotfixDownloadFail)
                        yield return StartDownloadAllHotfixDll(); //下载包Dll

                    if (PlayerPrefs.GetString(HOTFIX_STATE) != HotfixDownloadFail)
                        yield return HotfixABResources(); //下载AB 和 mainfest文件

                    if (PlayerPrefs.GetString(HOTFIX_STATE) != HotfixDownloadFail)
                        yield return StartDownloadAllAssetBackup(); //下载 备份资源
                    

                    if (PlayerPrefs.GetString(HOTFIX_STATE) != HotfixDownloadFail)
                    {

                        // 写入版本文件
                        //FileUtils.WriteAllBytes(PathHelper.tmpVersionLOCPTH, versionDataRemoteByte);
                        FileUtils.WriteAllText(PathHelper.tmpVersionLOCPTH, versionFileRemoteNode.ToString());
                        //Debug.Log($"@@ 写入版本内容：{versionFileRemoteNode.ToString()}");


                        PageLaunch.Instance.AddProgressCount(LoadingProgress.COPY_TEMP_HOTFIX_FILE, 1);
                        PageLaunch.Instance.Next(LoadingProgress.COPY_TEMP_HOTFIX_FILE, $"copy cache : temp hotfix file");

                        // 检查要拷贝的文件
                        PlayerPrefs.SetString(HOTFIX_STATE, HotfixStateCopying);
                        yield return CopyTempWebHotfixFileToTargetDir();


                        PageLaunch.Instance.RemoveProgress(LoadingProgress.COPY_TEMP_HOTFIX_FILE);


                        // 重新获取本地配置文件
                        GlobalData.version = JObject.Parse(File.ReadAllText(PathHelper.versionLOCPTH));

                        // 删除多余文件
                        yield return DeleteUnuseABAndManifest();


                        // 删除无用dll
                        yield return DeleteUnuseHotfixDll();

                    }

                }
                else
                {
                    Debug.Log("no need for hotfix");
                }

                // 回调
                proxyCallback?.Invoke();
                yield break;
            }
            else // 网路主版本号大于当前主版本号
            {
                // 回调 - 后卡主 - 提醒需要下载更新的app安装包
                Debug.LogError("The local master version number and the remote master version number are not equal");
                proxyCallback?.Invoke();
                yield break;
            }
        }

    }



    #region 删除用不到的ab数据 dll数据
    /// <summary>
    /// 遍历所有预制体，设置预制体名
    /// </summary>
    /// <param name="rootFolderPath"></param>
    List<string> GetTargetFilePath(string rootFolderPath, string extension = ".png")
    {
        List<string> paths = new List<string>();
        foreach (string path in Directory.GetFiles(rootFolderPath))
        {
            //获取所有文件夹中包含后缀为 .prefab 的路径
            if (extension == ".*") // path System.IO.Path.GetExtension(path) != ".meta"
            {
                if (!path.EndsWith(".meta"))
                    paths.Add(path);
            }
            else if (path.EndsWith(extension))
            {
                paths.Add(path);
            }
        }
        if (Directory.GetDirectories(rootFolderPath).Length > 0)  //遍历所有文件夹
        {
            foreach (string path in Directory.GetDirectories(rootFolderPath))
            {
                paths.AddRange(GetTargetFilePath(path, extension));
            }
        }
        return paths;
    }

    List<string> GetUnuseAB()
    {

        string manifestAssetName = "AssetBundleManifest"; // 假设 manifest 文件的资源名称
        AssetBundle manifestBundle = AssetBundle.LoadFromFile(PathHelper.mainfestLOCPTH);
        AssetBundleManifest manifest = manifestBundle.LoadAsset<AssetBundleManifest>(manifestAssetName);
        manifestBundle.Unload(false);

        string[] allAssetBundleNames = manifest.GetAllAssetBundles();

        List<string> bundlePathLst = new List<string>();
        foreach (string assetBundleName in allAssetBundleNames)
        {
            string pth1 = Path.Combine(PathHelper.abDirLOCPTH, assetBundleName);
            bundlePathLst.Add(pth1.Replace("\\", "/"));
        }

        // 主包ab包，是不带".unity3d"结尾的。（"GameRes" 和 “GameRes.manifest”）
        List<string> targetPathLst = new List<string>();  //获取普通包路劲 xxx.unity3d  和  xxx.unity3d.manifest
        targetPathLst.AddRange(GetTargetFilePath(PathHelper.abDirLOCPTH, ".unity3d"));
        //targetPathLst.AddRange(GetTargetFilePath(abDirLOCPTH, ".unity3d.manifest"));

        for (int i = 0; i < targetPathLst.Count; i++)
        {
            targetPathLst[i] = targetPathLst[i].Replace("\\", "/");
        }

        List<string> unusePths = new List<string>();
        foreach (string pth002 in targetPathLst)
        {
            /* string tempPth = pth002.EndsWith(".unity3d.manifest") ? pth002.Replace(".unity3d.manifest", ".unity3d") : pth002;
             if (!bundlePathLst.Contains(tempPth))
             {
                 unusePths.Add(pth002);
             }*/
            if (!bundlePathLst.Contains(pth002))
            {
                unusePths.Add(pth002);
            }
        }

        return unusePths;
    }

    /// <summary>
    /// 删除不用的ab包和manifest文件
    /// </summary>
    IEnumerator DeleteUnuseABAndManifest()
    {
        List<string> unusePths = GetUnuseAB();

        PageLaunch.Instance.AddProgressCount(LoadingProgress.DELETE_UNUSE_ASSET_BUNDLE, unusePths.Count);

        int i = 0;
        foreach (string pth in unusePths)
        {
            i++;
            if (File.Exists(pth))
            {
                PageLaunch.Instance.Next(LoadingProgress.DELETE_UNUSE_ASSET_BUNDLE, $"delete unuse ab {i}/{unusePths.Count}: {pth}  ");

                Debug.Log($"delete unuse ab {i}/{unusePths.Count}: {pth}");
                File.Delete(pth);
                yield return null;
            }
        }

        PageLaunch.Instance.Next(LoadingProgress.DELETE_UNUSE_ASSET_BUNDLE, $"delete unuse ab folder");

        yield return DeleteUnuseABFolderAndMeta();

        PageLaunch.Instance.RemoveProgress(LoadingProgress.DELETE_UNUSE_ASSET_BUNDLE);
    }


    /// <summary>
    /// 删除不用的文件夹
    /// </summary>
    /// <returns></returns>
    IEnumerator DeleteUnuseABFolderAndMeta()
    {

        List<string> allSubDirectories = GetAllSubFolders(PathHelper.abDirLOCPTH);

        // 对文件夹路径按字符长度从长到短进行排序（越长的路劲排在越前面）
        allSubDirectories.Sort((a, b) => b.Length - a.Length);

        /*
        // 输出所有子文件夹的名称
        foreach (string directory in allSubDirectories)
            Debug.Log(directory);
        */

        // 遍历排序后的目录路径
        foreach (string directory in allSubDirectories)
        {
            if (ShouldDeleteDirectory(directory))
            {
                DeleteDirectoryAndMeta(directory);
                yield return null;
            }
        }
    }

    static List<string> GetAllSubFolders(string directoryPath)
    {
        List<string> allFolders = new List<string>();
        try
        {
            // 获取当前目录下的所有子文件夹
            string[] subDirectories = Directory.GetDirectories(directoryPath);
            foreach (string subDirectory in subDirectories)
            {
                // 将当前子文件夹添加到结果列表中
                allFolders.Add(subDirectory);
                // 递归调用该方法，获取当前子文件夹下的所有子文件夹
                allFolders.AddRange(GetAllSubFolders(subDirectory));
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"访问目录 {directoryPath} 时出错: {e.Message}");
        }
        return allFolders;
    }


    /// <summary>
    /// 如果该路劲存在，且没有子级文件夹，或子级文件都是 .meta文件， 则把这个目录及其里面的内都删除
    /// </summary>
    /// <param name="directoryPath"></param>
    static bool ShouldDeleteDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            return false;
        }

        string[] subDirectories = Directory.GetDirectories(directoryPath);
        if (subDirectories.Length > 0)
        {
            return false;
        }

        string[] files = Directory.GetFiles(directoryPath);
        foreach (string file in files)
        {
            if (Path.GetExtension(file).ToLower() != ".meta")
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 删除目录，和目录的.meta文件，及目录里的所有内容
    /// </summary>
    /// <param name="directoryPath"></param>
    static void DeleteDirectoryAndMeta(string directoryPath)
    {
        try
        {
            // 删除目录及其内容
            Directory.Delete(directoryPath, true);

            // 删除对应的 .meta 文件
            string metaFilePath = directoryPath + ".meta";
            if (File.Exists(metaFilePath))
            {
                File.Delete(metaFilePath);
            }

            Debug.Log($"delete unuse dir: {directoryPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"delete unuse dir: {directoryPath} error: {e.Message}");
        }
    }


    private IEnumerator DeleteUnuseHotfixDll()
    {

        JObject hotfixDlls = GlobalData.version["hotfix_dll"] as JObject;

        List<string> targetPathLst = new List<string>();  //获取普通包路劲 xxx.unity3d  和  xxx.unity3d.manifest
        targetPathLst.AddRange(GetTargetFilePath(PathHelper.hotfixDirLOCPTH, ".dll.bytes"));

        int idx = targetPathLst.Count - 1;
        while (idx >= 0)
        {
            string[] pths = targetPathLst[idx].Replace("\\", "/").Split('/');
            string name = pths[pths.Length - 1].Replace(".dll.bytes", "");

            if (hotfixDlls.ContainsKey(name))
            {
                targetPathLst.RemoveAt(idx);
            }
            idx--;
        }

        PageLaunch.Instance.AddProgressCount(LoadingProgress.DELETE_UNUSE_HOTFIX_DLL, targetPathLst.Count);
        int i = 0;
        foreach (string s in targetPathLst)
        {
            i++;
            if (File.Exists(s))
            {
                PageLaunch.Instance.Next(LoadingProgress.DELETE_UNUSE_HOTFIX_DLL, $"delete unuse dll {i}/{targetPathLst.Count}: {s}  ");

                Debug.Log($"delete unuse dll {i}/{targetPathLst.Count}: {s}");

                File.Delete(s);
                yield return null;
            }
        }

        PageLaunch.Instance.RemoveProgress(LoadingProgress.DELETE_UNUSE_HOTFIX_DLL);

    }



    #endregion





    #region 拷贝本包文件

    private IEnumerator GetStreamingAssetsVersion()
    {
        // 拷贝所有dll
        GlobalData.streamingAssetsVersion = null;
        yield return FileUtils.ReadStreamingAsset<string>(PathHelper.versionSAPTH, (obj) =>
        {
            GlobalData.streamingAssetsVersion = JObject.Parse((string)obj);
        }, (err) =>
        {
            throw new System.Exception(err);
        });
    }


    private IEnumerator CopyStreamingAssetsToPersistentDataPath()
    {
        Debug.LogWarning($"{PREFIX_TIP} copy streaming assets");

        // 删除临时文件目录
        if (Directory.Exists(PathHelper.tmpHotfixDirLOCPTH))
        {
            Debug.LogWarning("delete temp hotfix dir");
            yield return FileUtils.DeleteDirectoryAsync(PathHelper.tmpHotfixDirLOCPTH);
        }
        // 删除目录
        if (Directory.Exists(PathHelper.hotfixDirLOCPTH))
        {
            Debug.LogWarning("delete hotfix dir");
            yield return FileUtils.DeleteDirectoryAsync(PathHelper.hotfixDirLOCPTH);
        }

        // 删除total version 文件
        if (File.Exists(PathHelper.totalVersionLOCPTH))
        {
            Debug.LogWarning("delete total version file");
            File.Delete(PathHelper.totalVersionLOCPTH);
        }


        // 先加载manifest文件，读取所有ab资源
        using (var manifestWWW = new UnityWebRequest(PathHelper.mainfestSAPTH))
        {
            manifestWWW.downloadHandler = new DownloadHandlerAssetBundle(PathHelper.mainfestSAPTH, 0);
            yield return manifestWWW.SendWebRequest();

            if (manifestWWW.isNetworkError || manifestWWW.isHttpError)
            {
                throw new Exception("StreamingAssets中没有manifest文件，仅拷贝version文件");
            }
            else
            {
                // 拷贝ab资源到持久目录
                var manifestAB = DownloadHandlerAssetBundle.GetContent(manifestWWW);
                var abManifest = manifestAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                manifestAB.Unload(false);
                string[] abList = abManifest.GetAllAssetBundles();
                int totalCount = abList.Length;// 一个是版本配置文件，一个是manifest文件
                int completedCount = 0;

                PageLaunch.Instance.AddProgressCount(LoadingProgress.COPY_SA_ASSET_BUNDLE, totalCount);

                foreach (var abName in abList)
                {
                    string srcPath = PathHelper.GetAssetBundleSAPTH(abName);
                    string tarPath = PathHelper.GetAssetBundleLOCPTH(abName);
                    Debug.Log($"{srcPath} - {tarPath}");
                    completedCount++;

                    PageLaunch.Instance.Next(LoadingProgress.COPY_SA_ASSET_BUNDLE,
                        $"copy assetbundle to cache: {abName} {completedCount}/{totalCount}");

                    Debug.Log(string.Format("copy asset bundle {0}/{1}, bundle:{2}", completedCount, totalCount, abName));


                    yield return FileUtils.CopyStreamingAssetToLocal(srcPath, tarPath);

                }
                PageLaunch.Instance.Next(LoadingProgress.COPY_SA_ASSET_BUNDLE,
                    $"copy manifest to cache: {PathHelper.mainfestBundleName}");

                Debug.Log("copy manifest");
                yield return FileUtils.CopyStreamingAssetToLocal(PathHelper.mainfestSAPTH, PathHelper.mainfestLOCPTH);

            }
        }

        PageLaunch.Instance.RemoveProgress(LoadingProgress.COPY_SA_ASSET_BUNDLE);


        /* 拷贝所有dll
        JObject versionSAObj = null;
        yield return FileUtils.ReadStreamingAsset<string>(PathHelper.versionSAPTH, (obj) =>
        {
            versionSAObj = JObject.Parse((string)obj); 

        }, (err) =>
        {
            throw new System.Exception(err);
        });*/

        JObject versionSAObj = GlobalData.streamingAssetsVersion;

        if (versionSAObj != null)
        {

            JObject hotfixDll = versionSAObj["hotfix_dll"] as JObject;

            PageLaunch.Instance.AddProgressCount(LoadingProgress.COPY_SA_HOTFIX_DLL, hotfixDll.Count);

            int i = 0;
            foreach (KeyValuePair<string, JToken> kv in hotfixDll)
            {
                string saPth = PathHelper.GetDllSAPTH(kv.Key);
                string tarPath = PathHelper.GetDllLOCPTH(kv.Key);

                i++;
                PageLaunch.Instance.Next(LoadingProgress.COPY_SA_HOTFIX_DLL,
                    $"copy dll to cache: {kv.Key} {i}/{hotfixDll.Count}");

                Debug.Log(string.Format("copy dll {0}/{1}, dll:{2}", i, hotfixDll.Count, kv.Key));

                yield return FileUtils.CopyStreamingAssetToLocal(saPth, tarPath);
            }

            //PageLaunch.Instance.Next(LoadingProgress.COPY_SA_HOTFIX_DLL,  $"copy version file to cache: {PathHelper.versionName}");
            //Debug.Log("copy version");
            //yield return FileUtils.CopyStreamingAssetToLocal(PathHelper.versionSAPTH, PathHelper.versionLOCPTH);
        }

        PageLaunch.Instance.RemoveProgress(LoadingProgress.COPY_SA_HOTFIX_DLL);


        if (versionSAObj != null)
        {
            JObject assetBackup = versionSAObj["asset_backup"] as JObject;

            PageLaunch.Instance.AddProgressCount(LoadingProgress.COPY_SA_ASSET_BACKUP, assetBackup.Count);

            int i = 0;
            foreach (KeyValuePair<string, JToken> kv in assetBackup)
            {
                string saPth = PathHelper.GetAssetBackupSAPTH(kv.Key); //PathHelper.GetDllSAPTH(kv.Key);
                string tarPath = PathHelper.GetAssetBackupLOCPTH(kv.Key); // PathHelper.GetDllLOCPTH(kv.Key);

                i++;
                PageLaunch.Instance.Next(LoadingProgress.COPY_SA_ASSET_BACKUP,
                    $"copy asset backup to cache: {kv.Key} {i}/{assetBackup.Count}");

                Debug.Log(string.Format("copy asset backup {0}/{1}, backup:{2}", i, assetBackup.Count, kv.Key));

                yield return FileUtils.CopyStreamingAssetToLocal(saPth, tarPath);
            }
        }
        PageLaunch.Instance.RemoveProgress(LoadingProgress.COPY_SA_ASSET_BACKUP);

        if (versionSAObj != null)
        {
            //PageLaunch.Instance.Next(LoadingProgress.COPY_SA_ASSET_BACKUP, $"copy version file to cache: {PathHelper.versionName}");
            PageLaunch.Instance.RefreshProgressUIMsg($"copy version file to cache: {PathHelper.versionName}");
            Debug.Log("copy version");
            yield return FileUtils.CopyStreamingAssetToLocal(PathHelper.versionSAPTH, PathHelper.versionLOCPTH);
        }
        else
        {
            Debug.LogError("the version in StreamingAssets can not find!! ");
        }
    }

    #endregion



    #region 下载资源拷贝

    /// <summary>
    /// 查看是否有新热更完整的资源缓存
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// * 每次最后，都有删除临时缓存
    /// </remarks>
    private IEnumerator CopyTempWebHotfixFileToTargetDir()
    {

        // 是否有热更新文件需要拷贝
        if (PlayerPrefs.HasKey(HOTFIX_STATE) && PlayerPrefs.GetString(HOTFIX_STATE) == HotfixStateCopying)
        {
            Debug.LogWarning($"{PREFIX_TIP}  copy temp file to target dir");

            //开始拷贝
            yield return FileUtils.CopyDirectoryAsync(PathHelper.tmpHotfixDirLOCPTH, PathHelper.hotfixDirLOCPTH);

            PlayerPrefs.SetString(HOTFIX_STATE, HotfixStateCompleted);
        }

        // 删除缓存
        if (Directory.Exists(PathHelper.tmpHotfixDirLOCPTH))
        {
            yield return FileUtils.DeleteDirectoryAsync(PathHelper.tmpHotfixDirLOCPTH);
        }

    }

    #endregion



    #region 检查远端版本

    /// <summary>
    /// 删除远程的参数
    /// </summary>
    void ClearRemoteInfo()
    {
        versionFileRemoteNode = null;
        //versionDataRemoteByte = null;
    }


    private IEnumerator GetWebTotalVersionAndVersion(UnityAction onSuccessCallback, UnityAction onErrorCallback)
    {

        string tvUrl = PathHelper.totalVersionWEBURL + $"?t={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        Debug.Log($"download total version： {tvUrl}");
       
        PageLaunch.Instance.AddProgressCount(LoadingProgress.CHECK_WEB_VERSION, 2);
        PageLaunch.Instance.Next(LoadingProgress.CHECK_WEB_VERSION, $"get web total version");

        using (UnityWebRequest reqTotalVersion = UnityWebRequest.Get(tvUrl))
        {
            yield return reqTotalVersion.SendWebRequest();

            if (reqTotalVersion.result == UnityWebRequest.Result.Success)
            {
                string tvStr = reqTotalVersion.downloadHandler.text;

                GlobalData.totalVersion = JObject.Parse(tvStr);

                // 拷贝版本文件(写入文件时，可能断电重启，导致写入数据有问题)
                FileUtils.WriteAllBytes(PathHelper.totalVersionLOCPTH, reqTotalVersion.downloadHandler.data);

                JObject targetTotalVersionItem = null;
                if (GlobalData.totalVersion != null)
                {
                    JArray lst = GlobalData.totalVersion["data"] as JArray;
                    for (int i = 0; i < lst.Count; i++)
                    {
                        string appKey = lst[i]["app_key"].ToObject<string>();
                        if (appKey == ApplicationSettings.Instance.appKey)
                        {
                            targetTotalVersionItem = lst[i] as JObject;
                            break;
                        }
                    }
                }

                if (targetTotalVersionItem != null)
                {
                    GlobalData.autoHotfixUrl = FileUtils.GetDirWebUrl(PathHelper.totalVersionWEBURL, targetTotalVersionItem["hotfix_url"].ToObject<string>());

                    PageLaunch.Instance.Next(LoadingProgress.CHECK_WEB_VERSION, $"get web version");

                    yield return GetWebVersion(onSuccessCallback, onErrorCallback);
                }
                else
                {
                    Debug.LogError($"web total version file cant not find node at app_key: {ApplicationSettings.Instance.appKey}");
                    onErrorCallback?.Invoke();
                }
            }
            else//服务器版本文件加载失败
            {
                Debug.LogError(reqTotalVersion.error);
                Debug.Log("没有网络直接复制文件");
                onErrorCallback?.Invoke();
            }
        }
        PageLaunch.Instance.RemoveProgress(LoadingProgress.CHECK_WEB_VERSION);
    }


    private IEnumerator GetWebVersion(UnityAction onSuccessCallback, UnityAction onErrorCallback)
    {

        string vUrl = PathHelper.versionFileWEBURL + $"?t={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        Debug.Log($"remote version url： {vUrl}");
        using (UnityWebRequest reqVersion = UnityWebRequest.Get(vUrl))
        {
            yield return reqVersion.SendWebRequest();

            if (reqVersion.result == UnityWebRequest.Result.Success)
            {
                string jsonStr = reqVersion.downloadHandler.text;

                //versionDataRemoteByte = reqVersion.downloadHandler.data;

                versionFileRemoteNode = JObject.Parse(jsonStr);

                Debug.Log($"web version file ; ver: {versionFileRemoteNode["hotfix_version"].ToObject<string>()}");

                //Debug.Log($"version data :  {jsonStr}");
                onSuccessCallback?.Invoke();
            }
            else//服务器版本文件加载失败
            {
                Debug.LogError(reqVersion.error);
                Debug.Log("没有网络直接复制文件");
                onErrorCallback?.Invoke();
            }

        }
    }




    #endregion





    #region 远端下载备份资源
    private IEnumerator DownloadRemoteAssetBackupOnce(string nodeName)
    {
        if (PlayerPrefs.GetString(HOTFIX_STATE) == HotfixDownloadFail)
            yield break;

        string hashRemote = GetWebAssetBackupHash(nodeName);
        string assetBackupDownloadUrl = PathHelper.GetAssetBackupWEBURL(nodeName);

        // cdn 加载
        UnityWebRequest req = UnityWebRequest.Get(assetBackupDownloadUrl); //ApplicationSettings.Instance.hotfixUrl

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string md5 = FileUtils.CalculateMD5(req.downloadHandler.data);

            if (md5 == hashRemote)
            {
                string writePath = PathHelper.GetTempAssetBackupLOCPTH(nodeName);
                FileUtils.WriteAllBytes(writePath, req.downloadHandler.data);

                //【这块先隐藏】 s_assetDatas[dallame] = req.downloadHandler.data;
                yield break;
            }
        }

        // 非cdn加载
        UnityWebRequest req01 = UnityWebRequest.Get(assetBackupDownloadUrl + $"?t={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
        yield return req01.SendWebRequest();
        if (req01.result == UnityWebRequest.Result.Success)
        {
            string md5 = FileUtils.CalculateMD5(req01.downloadHandler.data);

            if (md5 == hashRemote)
            {
                string writePath = PathHelper.GetTempAssetBackupLOCPTH(nodeName);
                FileUtils.WriteAllBytes(writePath, req01.downloadHandler.data);

                //【这块先隐藏】 s_assetDatas[dallame] = req01.downloadHandler.data;

                yield break;
            }
        }

        PlayerPrefs.SetString(HOTFIX_STATE, HotfixDownloadFail);

        throwErrMsg = $"down asset backup '{assetBackupDownloadUrl}' is fail";
        throw new Exception(throwErrMsg);
    }




    private IEnumerator StartDownloadAllAssetBackup()
    {
        Debug.LogWarning($"{PREFIX_TIP} start download asset backup");

        JObject localBackupNode = GlobalData.version["asset_backup"] as JObject;
        JObject serverBackupNode = versionFileRemoteNode["asset_backup"] as JObject;


        PageLaunch.Instance.AddProgressCount(LoadingProgress.DOWNLOAD_ASSET_BACKUP, serverBackupNode.Count);

        int i = 0;
        //从资源服务器下载DLL写入到本地
        foreach (KeyValuePair<string, JToken> item in serverBackupNode)
        {

            string name = item.Key;
            if (!localBackupNode.ContainsKey(name)
                || (localBackupNode[name]["hash"].ToObject<string>() != serverBackupNode[name]["hash"].ToObject<string>()))
            {

                PageLaunch.Instance.Next(LoadingProgress.DOWNLOAD_ASSET_BACKUP, $"download asset backup: {name}  {i + 1}/{serverBackupNode.Count}");

                Debug.Log($"download asset backup: {name}  {i + 1}/{serverBackupNode.Count}");

                yield return DownloadRemoteAssetBackupOnce(name);
            }
            i++;
        }

        PageLaunch.Instance.RemoveProgress(LoadingProgress.DOWNLOAD_ASSET_BACKUP);

    }





    #endregion















    #region 远端资源下载
    private IEnumerator DownLoadRemoteHotfixDllOnce(string dallame)
    {
        if (PlayerPrefs.GetString(HOTFIX_STATE) == HotfixDownloadFail)
            yield break;

        string hashRemote = GetWebHotfixDllHash(dallame);
        string hotfixDllDownloadUrl = PathHelper.GetDllWEBURL(dallame);

        Debug.Log($"hotfixDirWEBURL = {PathHelper.hotfixDirWEBURL}");
        
        Debug.Log($"下载远端dll： {hotfixDllDownloadUrl}");

        hotfixDllDownloadUrl = hotfixDllDownloadUrl.Replace("//GameDll", "/GameDll");

        // cdn 加载
        UnityWebRequest req = UnityWebRequest.Get(hotfixDllDownloadUrl); //ApplicationSettings.Instance.hotfixUrl

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string md5 = FileUtils.CalculateMD5(req.downloadHandler.data);

            if (md5 == hashRemote)
            {
                string writePath = PathHelper.GetTempDllLOCPTH(dallame);
                FileUtils.WriteAllBytes(writePath, req.downloadHandler.data);

                //【这块先隐藏】 s_assetDatas[dallame] = req.downloadHandler.data;
                yield break;
            }
        }


        string dllWebUrl = hotfixDllDownloadUrl + $"?t={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

        Debug.Log($"下载远端dll： {dllWebUrl}");

        // 非cdn加载
        UnityWebRequest req01 = UnityWebRequest.Get(dllWebUrl);
        yield return req01.SendWebRequest();
        if (req01.result == UnityWebRequest.Result.Success)
        {
            string md5 = FileUtils.CalculateMD5(req01.downloadHandler.data);

            if (md5 == hashRemote)
            {
                string writePath = PathHelper.GetTempDllLOCPTH(dallame);
                FileUtils.WriteAllBytes(writePath, req01.downloadHandler.data);

                //【这块先隐藏】 s_assetDatas[dallame] = req01.downloadHandler.data;

                yield break;
            }
        }

        PlayerPrefs.SetString(HOTFIX_STATE, HotfixDownloadFail);

        throwErrMsg = $"down dll '{hotfixDllDownloadUrl}' is fail";
        throw new Exception(throwErrMsg);
    }



    private IEnumerator StartDownloadAllHotfixDll()
    {
        Debug.LogWarning($"{PREFIX_TIP} start download all hotfix dll");

        JObject localHotfixNode = GlobalData.version["hotfix_dll"] as JObject;
        JObject serverHotfixNode = versionFileRemoteNode["hotfix_dll"] as JObject;


        PageLaunch.Instance.AddProgressCount(LoadingProgress.DOWNLOAD_HOTFIX_DLL, serverHotfixNode.Count);

        int i = 0;
        //从资源服务器下载DLL写入到本地
        foreach (KeyValuePair<string, JToken> item in serverHotfixNode)
        {

            string name = item.Key;
            if (!localHotfixNode.ContainsKey(name)
                || (localHotfixNode[name]["hash"].ToObject<string>() != serverHotfixNode[name]["hash"].ToObject<string>()))
            {

                PageLaunch.Instance.Next(LoadingProgress.DOWNLOAD_HOTFIX_DLL, $"download hotfix dll: {name}  {i + 1}/{serverHotfixNode.Count}");

                Debug.Log($"download hotfix dll: {name}  {i + 1}/{serverHotfixNode.Count}");

                yield return DownLoadRemoteHotfixDllOnce(name);
            }
            i++;
        }

        PageLaunch.Instance.RemoveProgress(LoadingProgress.DOWNLOAD_HOTFIX_DLL);

    }



    // 热更AB资源
    private IEnumerator HotfixABResources()
    {
        Debug.LogWarning($"{PREFIX_TIP} start download asset bundle");

        using (UnityWebRequest req = UnityWebRequest.Get(PathHelper.mainfestWEBURL))
        {
            yield return req.SendWebRequest();
            if (req.isNetworkError || req.isHttpError)
            {
                Debug.LogError(req.error);
                yield break;
            }
            else
            {
                JObject bundleHash = versionFileRemoteNode["asset_bundle"]["bundle_hash"] as JObject;

                var serverManifestAB = AssetBundle.LoadFromMemory(req.downloadHandler.data);
                AssetBundleManifest serverManifest = serverManifestAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                serverManifestAB.Unload(false);

                List<string> downloadFiles;

                if (File.Exists(PathHelper.mainfestLOCPTH))
                {
                    var localManifestAB = AssetBundle.LoadFromFile(PathHelper.mainfestLOCPTH);
                    AssetBundleManifest localManifest = localManifestAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                    localManifestAB.Unload(false);
                    downloadFiles = GetDownFileName(localManifest, serverManifest);
                }
                else
                {
                    // 本地没有manifest文件，所有AB资源全部从服务器下载
                    downloadFiles = new List<string>(serverManifest.GetAllAssetBundles());
                }

                yield return DownloadABFiles(downloadFiles, bundleHash);

                //更新最新manifest到本地缓存
                FileUtils.WriteAllBytes(PathHelper.tmpMainfestLOCPTH, req.downloadHandler.data);

                yield return new WaitForEndOfFrame();
            }
        }
    }


    // 得到需要下载的AB文件名
    private List<string> GetDownFileName(AssetBundleManifest localManifest, AssetBundleManifest serverManifest)
    {
        List<string> tempList = new List<string>();
        var localHashCode = localManifest.GetHashCode();
        var serverHashCode = serverManifest.GetHashCode();
        if (localHashCode != serverHashCode)
        {
            string[] localABList = localManifest.GetAllAssetBundles();
            string[] serverABList = serverManifest.GetAllAssetBundles();
            Dictionary<string, Hash128> localHashDic = new Dictionary<string, Hash128>();
            foreach (var iter in localABList)
            {
                localHashDic.Add(iter, localManifest.GetAssetBundleHash(iter));
            }
            foreach (var iter in serverABList)
            {
                //如果之前也包含了此AB 则对比哈希看下是否有更新
                if (localHashDic.ContainsKey(iter))
                {
                    Hash128 serverHash = serverManifest.GetAssetBundleHash(iter);
                    if (serverHash != localHashDic[iter])
                    {
                        tempList.Add(iter);
                    }
                }
                //之前不包含则一定是新的
                else
                {
                    tempList.Add(iter);
                }
            }
        }
        return tempList;
    }


    // 下载需要更新的文件
    private IEnumerator DownloadABFiles(List<string> downloadFiles, JObject hash)
    {
       // int completedCount = 0;
        int totalCount = downloadFiles.Count;
        if (totalCount == 0)
        {
            Debug.Log("没有需要跟新的AB资源");
            yield break;
        }
        else
        {
            PageLaunch.Instance.AddProgressCount(LoadingProgress.DOWNLOAD_ASSET_BUNDLE, totalCount);
            int i = 0;

            foreach (var iter in downloadFiles)
            {
                i++;
                PageLaunch.Instance.Next(LoadingProgress.DOWNLOAD_ASSET_BUNDLE, $"download ab: {iter} {i}/{totalCount}");
                Debug.Log($"download ab: {iter} {i}/{totalCount}");

                yield return DownloadABFileOnce(iter,hash);
            }
        }

        PageLaunch.Instance.RemoveProgress(LoadingProgress.DOWNLOAD_ASSET_BUNDLE);
    }

    private IEnumerator DownloadABFileOnce(string iter, JObject hash)
    {
        if (PlayerPrefs.GetString(HOTFIX_STATE) == HotfixDownloadFail)
            yield break;
        
        string path = Path.Combine(PathHelper.abDirWEBURL, iter);
        string downloadPath = Path.Combine(PathHelper.tmpABDirLOCPTH, iter);
        //uint calculatedHash = 0;
        string calculatedHash = "";

        using (UnityWebRequest webReq = UnityWebRequest.Get(path))
        {
            yield return webReq.SendWebRequest();
            if (webReq.result == UnityWebRequest.Result.Success)
            {
                byte[] result = webReq.downloadHandler.data;
                FileUtils.WriteAllBytes(downloadPath, result);

                // 这个一直在变
                //AssetBundle ab = AssetBundle.LoadFromFile(downloadPath);
                //AssetBundle ab = AssetBundle.LoadFromMemory(result);
                //int dat = ab.GetHashCode();


                // 这个只能在编辑器中使用
                //UnityEditor.BuildPipeline.GetCRCForAssetBundle(downloadPath, out calculatedHash);// 计算本地保存的AB包的CRC值

                calculatedHash = FileUtils.CalculateMD5(result);

                yield return new WaitForEndOfFrame();
            }
            else
            {
                Debug.LogError($"download bundle: {iter}  error: {webReq.error}");
                yield return null;
            }
        }
        // 解决缓存问题
        if (hash != null && hash.ContainsKey(iter) && hash[iter].ToObject<string>() != calculatedHash)
        {   
            using (UnityWebRequest webReq = UnityWebRequest.Get($"{path}?t={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"))
            {
                yield return webReq.SendWebRequest();
                if (webReq.result == UnityWebRequest.Result.Success)
                {
                    byte[] result = webReq.downloadHandler.data;
                    FileUtils.WriteAllBytes(downloadPath, result);
                    //yield return new WaitForEndOfFrame();

                    yield break;
                }
                else
                {

                    PlayerPrefs.SetString(HOTFIX_STATE, HotfixDownloadFail);
                    //throw new Exception($"download bundle: {iter} is fail");

                    throwErrMsg = $"download bundle: {iter} ; path: '{path}' ; error: {webReq.error}";
                    Debug.LogError(throwErrMsg);
                    yield return null;
                }
            }
        }


    }

#endregion


    private int[] GetVersions(string version)
    {
        string[] str = version.Split('.');

        List<int> vers = new List<int>();
        for (int i = 0; i < str.Length; i++)
        {
            vers.Add(int.Parse(str[i]));
        }
        // 解析主版本号和次版本号
        return vers.ToArray();
    }





}
