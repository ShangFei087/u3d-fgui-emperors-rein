using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using UnityEngine.XR;






public partial class AssetBundleBuilder05 : EditorWindow
{

    [MenuItem("NewBuild/拷贝DLL到StreamingAssets目录GameDll 下并更改后缀名")]
    public static void CopyDllAndReName002()
    {

        CopyDll();

        // 刷新版本号
        UpdateVersionData__002();

        AssetDatabase.Refresh();
    }



    static void CopyDll()
    {
        string toDirPath = PathHelper.dllDirSAPTH;

        // 删除所有文件
        ClearHotfixDll_002(toDirPath);


        if (Directory.Exists(toDirPath) == false)  // 判断是否存在，不存在创建
        {
            Directory.CreateDirectory(toDirPath); // 根据文件夹路径创建文件夹
        }


        string dataPath = Application.dataPath;
        string projectRootPath = Directory.GetParent(dataPath).FullName;
        string targetPath = Path.Combine(projectRootPath, "HybridCLRData/HotUpdateDlls/" + EditorUserBuildSettings.activeBuildTarget.ToString());//D:\work\u3d-po\HybridCLRData/HotUpdateDlls/Android
        List<string> list = DllHelper.Instance.DllNameList;
        for (int i = 0; i < list.Count; i++)
        {
            string sourcePath = Path.Combine(targetPath, list[i] + ".dll");
            string destinaltionPaht = Path.Combine(toDirPath, list[i] + ".dll.bytes");
            if (File.Exists(sourcePath))
            {
                try
                {
                    File.Copy(sourcePath, destinaltionPaht, overwrite: true);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
            else
            {
                Debug.Log("要拷贝的文件不存在" + sourcePath);
            }
        }
    }




    [MenuItem("NewBuild/打包1001")]
    public static void BuildPigSlotGameResource002()
    {
        CopyDll();
        CopyAssetBackup();

        string toPath = PathHelper.abDirSAPTH;


        if (Directory.Exists(toPath) == false)  // 判断是否存在，不存在创建
        {
            Directory.CreateDirectory(toPath); // 根据文件夹路径创建文件夹
        }

        GetNopkDir();

        ResetABName_002();

        MarkResourceNameEx_002();

        MarkLuBanJson_003();

        //MarkLuBanJson_002();

        MarkResoueceABs_002();

        MarkResourceSounds_002();

        MarkResoueceFGUIs_002();

        BuildPipeline.BuildAssetBundles(toPath, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);


        ClearUnuseABAndManifest_002();
        ClearUnuseFolderAndMeta_002();

        // 刷新版本号
        UpdateVersionData__002();

        AssetDatabase.Refresh();

        Debug.Log("恭喜,打包成功");
    }



    [MenuItem("NewBuild/拷贝资源StreamingAssets目录GameBackup 下并修改版本文件")]
    public static void AssetBackup()
    {
        CopyAssetBackup();

        // 刷新版本号
        UpdateVersionData__002();

        AssetDatabase.Refresh();
    }
    public static void CopyAssetBackup()
    {
        string toDirPath = PathHelper.backupDirSAPTH;

        // 递归删除文件夹及其所有内容
        if (Directory.Exists(toDirPath))
        {
            Directory.Delete(toDirPath, recursive: true);
        }

        if (Directory.Exists(toDirPath) == false)  // 判断是否存在，不存在创建
        {
            Directory.CreateDirectory(toDirPath); // 根据文件夹路径创建文件夹
        }

        string folderPthLst = PathHelper.gameBackupDirPROJPTH; //E:/work4/u3d-dll-po/Assets

        List<string> targetPathLst = new List<string>();
        targetPathLst.AddRange(GetTargetFilePath(PathHelper.gameBackupDirPROJPTH, ".*"));

        foreach (var pth in targetPathLst)
        {
            string sourcePath = pth;
            string destinaltionPaht = PathHelper.GetAssetBackupSAPTH(pth);
            if (File.Exists(sourcePath))
            {
                try
                {
                    string destDirectory = Path.GetDirectoryName(destinaltionPaht);
                    if (destDirectory == null)
                    {
                        Debug.LogError("错误：无法解析目标目录路径！");
                        return;
                    }

                    if (!Directory.Exists(destDirectory))
                    {
                        Directory.CreateDirectory(destDirectory);
                    }

                    File.Copy(sourcePath, destinaltionPaht, overwrite: true);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            else
            {
                Debug.Log("要拷贝的文件不存在" + sourcePath);
            }
            Debug.Log(destinaltionPaht);
        }
    }





    [MenuItem("NewBuild/【测试】-显示本地路劲")]
    private static void TestShowLocalPath()
    {
        // 删除所有文件
        Debug.Log(Application.persistentDataPath);
    }




    /*
    [MenuItem("NewBuild/清除所有ab包名")]
    public static void ClearAllAbName()
    {
        ResetABName_002();
    }*/


    /*
    static void MarkLuBanJson_002()
    {
        string luBanJsonPath = Application.dataPath + "/GameRes/LuBan/GenerateDatas/bytes";
        foreach (string path in Directory.GetFiles(luBanJsonPath))
        {
            if (System.IO.Path.GetExtension(path) == ".json")
            {
                string p = path.Substring(path.IndexOf("Assets"));
                string bundelName = path.Substring(path.IndexOf("LuBan"));//LuBan/GenerateDatas/bytes/xxxx.json
                if (bundelName.EndsWith("json"))
                {
                    bundelName = bundelName.Replace("json", "unity3d");
                }
                AssetImporter.GetAtPath(p).assetBundleName = bundelName;
            }
        }
    }
    */

    static void MarkLuBanJson_003()
    {
        string targetDir = PathHelper.gameResDirPROJPTH;

        List<string> folderPthLst = GetAllFolderPath(targetDir, "\\LuBan");
        List<string> targetPathLst = new List<string>();
        foreach (var pth in folderPthLst)
        {
            targetPathLst.AddRange(GetTargetFilePath(pth, ".json"));
        }
        foreach (string pth in targetPathLst)
        {
            //Debug.Log($"@ ABs =={pth}");
            SetBundleName(pth);
        }
    }





    /// <summary>
    /// ABs文件加下的所有文件都打包
    /// </summary>
    static void MarkResoueceABs_002()
    {

        string rootFolderPth = PathHelper.gameResDirPROJPTH; // Application.dataPath + "/GameRes";
        List<string> folderPthLst = GetAllFolderPath(rootFolderPth, "\\ABs");  //原有ABs

        List<string> targetPathLst = new List<string>();
        foreach (var pth in folderPthLst)
        {
            targetPathLst.AddRange(GetTargetFilePath(pth, ".*"));
        }

        foreach (string pth in targetPathLst)
        {
            //Debug.Log($"@ ABs =={pth}");
            SetBundleName(pth);
        }
    }

    /// <summary>
    /// 删除路劲下的所有ab包名
    /// </summary>
    /// <param name="folderPath"></param>
    private static void ResetABName_002()//(string folderPath = "GameRes")
    {
        List<string> assetPaths = new List<string>();
        // 获取指定文件夹的完整路径
        //string fullFolderPath = Path.Combine(Application.dataPath, folderPath);

        string fullFolderPath = PathHelper.gameResDirPROJPTH;

        if (!Directory.Exists(fullFolderPath))
        {
            Debug.LogError($"指定的文件夹 {fullFolderPath} 不存在。");
            return;
        }

        // 获取指定文件夹下的所有文件
        string[] allFiles = Directory.GetFiles(fullFolderPath, "*", SearchOption.AllDirectories);

        foreach (string filePath in allFiles)
        {
            //string p = filePath.Substring(filePath.IndexOf("Assets"));//Assets/GameRes/Games\666.prefab
            //AssetImporter.GetAtPath(p).assetBundleName = null;


            if (filePath.EndsWith(".cs") || filePath.EndsWith(".meta"))
            {
                continue;
            }

            // 将文件的绝对路径转换为相对于 Assets 目录的路径
            string assetPath = "Assets" + filePath.Replace(Application.dataPath, "").Replace('\\', '/');
            // 获取该资源的导入器
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer != null && importer.assetBundleName != null)
            {
                importer.assetBundleName = null;
            }
        }
    }
    static void MarkResourceNameEx_002()
    {

        string targetDir = PathHelper.gameResDirPROJPTH; // Application.dataPath + "/GameRes";
        //##ResetAssetBundleName_002(targetDir);

        List<string> folderPthLst = GetAllFolderPath(targetDir, "\\Prefabs");
        List<string> targetPathLst = new List<string>();
        foreach (var pth in folderPthLst)
        {
            targetPathLst.AddRange(GetTargetFilePath(pth, ".prefab"));
        }
        foreach (string pth in targetPathLst)
        {
            //Debug.Log($"@ ABs =={pth}");
            SetBundleName(pth);
        }
    }

    /*
    private static void ResetAssetBundleName_002(string dirPath)
    {
        foreach (string path in Directory.GetFiles(dirPath))
        {
            //获取所有文件夹中包含后缀为 .prefab 的路径
            if (System.IO.Path.GetExtension(path) == ".prefab")
            {
                string p = path.Substring(path.IndexOf("Assets"));//Assets/GameRes/Games\666.prefab
                AssetImporter.GetAtPath(p).assetBundleName = null;
            }
        }

        if (Directory.GetDirectories(dirPath).Length > 0)  //遍历所有文件夹
        {
            foreach (string path in Directory.GetDirectories(dirPath))
            {
                ResetAssetBundleName_002(path);
            }
        }
    }*/


    /*
    static void MarkResourceDatas_002()
    {
        string rootFolderPth = PathHelper.gameResDirPROJPTH; // Application.dataPath + "/GameRes";
        List<string> folderPthLst = GetAllFolderPath(rootFolderPth, "\\Datas");  //原有Datas

        List<string> targetPathLst = new List<string>();
        foreach (var pth in folderPthLst)
        {
            targetPathLst.AddRange(GetTargetFilePath(pth, ".json"));
        }

        foreach (string pth in targetPathLst)
        {
            //Debug.Log($"@=={pth}");
            SetBundleName(pth);
        }
    }*/

    /// <summary>
    /// 打包所有Sounds文件夹下声音
    /// </summary>
    static void MarkResourceSounds_002()
    {
        string rootFolderPth = PathHelper.gameResDirPROJPTH; // Application.dataPath + "/GameRes";
        List<string> folderPthLst = GetAllFolderPath(rootFolderPth, "\\Sounds");  //原有Datas

        List<string> targetPathLst = new List<string>();
        foreach (var pth in folderPthLst)
        {
            targetPathLst.AddRange(GetTargetFilePath(pth, ".mp3"));
            targetPathLst.AddRange(GetTargetFilePath(pth, ".wav"));
        }

        foreach (string pth in targetPathLst)
        {
            //Debug.Log($"@=={pth}");
            SetBundleName(pth);
        }
    }





    static void MarkResoueceFGUIs_002()
    {

        string rootFolderPth = PathHelper.gameResDirPROJPTH; // Application.dataPath + "/GameRes";
        List<string> folderPthLst = GetAllFolderPath(rootFolderPth, "\\FGUIs");

        List<string> targetPathLst = new List<string>();
        foreach (var pth in folderPthLst)
        {
            targetPathLst.AddRange(GetTargetFilePath(pth, ".*"));
        }

        foreach (string pth in targetPathLst)
        {
            //Debug.Log($"@ ABs =={pth}");
            SetBundleNameByFolder(pth, "FGUIs");
        }
    }





    /// <summary>
    /// 遍历所有预制体，设置预制体名
    /// </summary>
    /// <param name="rootFolderPath"></param>
    private static List<string> GetTargetFilePath(string rootFolderPath, string extension = ".png")
    {
        List<string> paths = new List<string>();
        foreach (string path in Directory.GetFiles(rootFolderPath))
        {

            string pth = path.Replace("\\", "/");

            bool isIgnore = false;
            for (int i = 0; i < nopkDir.Count; i++)
            {
                if (pth.StartsWith(nopkDir[i]))
                {
                    isIgnore = true;
                    break;
                }
            }
            if (isIgnore) continue;  //不参与打包

            //if (extension.EndsWith(".cs") || pth.Contains("/Editor/")) continue;       

            //if (path.Contains("nopk__")) continue; //不参与打包

            //获取所有文件夹中包含后缀为 .prefab 的路径
            if (extension == ".*") // path System.IO.Path.GetExtension(path) != ".meta"
            {
                if (!path.EndsWith(".meta") && !path.EndsWith(".cs"))
                    paths.Add(pth);
            }
            //else if (System.IO.Path.GetExtension(path) == extension)
            else if (path.EndsWith(extension))
            {
                paths.Add(pth);
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


    static List<string> GetAllFolderPath(string pathRoot, string targetFolder)
    {
        List<string> res = new List<string>();
        string[] chdPath = Directory.GetDirectories(pathRoot);

        if (chdPath.Length > 0)  //遍历所有文件夹
        {
            foreach (string path in chdPath)
            {
                if (path.EndsWith(targetFolder))
                {
                    res.Add(path);
                }
                res.AddRange(GetAllFolderPath(path, targetFolder));
            }
        }
        return res;
    }



    static void SetBundleName(List<string> pathLst)
    {
        foreach (var pth in pathLst)
        {
            SetBundleName(pth);
        }
    }

    static void SetBundleName(string pth)
    {
        string extension = Path.GetExtension(pth);

        string p = pth.Substring(pth.IndexOf("Assets"));
        //string bundelName = pth.Substring(pth.IndexOf("GameRes")); //  GameRes\Games/Game Maker\Datas\page.json
        string bundelName = pth.Substring(pth.IndexOf("GameRes") + "GameRes".Length + 1);  //  Games/Game Maker\Datas\page.json
        bundelName = bundelName.Replace(extension, ".unity3d");
        //Debug.Log($"{bundelName}");
        AssetImporter.GetAtPath(p).assetBundleName = bundelName;
    }

    static void SetBundleNameByFolder(string pth, string folderName = "FGUIs")
    {

        //Assets/GameRes/Games/Console/FGUIs/Console_atlas0.png
        //games/console/fguis
        //games/console/fguis.unity3d

        int gameResIndex = pth.IndexOf("GameRes");
        string pathAfterGameRes = pth.Substring(gameResIndex + "GameRes".Length + 1);

        int folderIndex = pathAfterGameRes.IndexOf(folderName);
        string bundelName = pathAfterGameRes.Substring(0, folderIndex + folderName.Length);

        bundelName += ".unity3d";

#if true
        string p = pth.Substring(pth.IndexOf("Assets"));
        AssetImporter.GetAtPath(p).assetBundleName = bundelName;

#else
        string p = "";
        try
        {
            p = pth.Substring(pth.IndexOf("Assets"));
            AssetImporter.GetAtPath(p).assetBundleName = bundelName;
        }
        catch (Exception e)
        {
            Debug.LogError($" pth={pth}  p={p}   bundelName={bundelName}");
            throw e;
        }
#endif

    }

    /// <summary>
    /// 删除不用的ab包
    /// </summary>
    static void ClearUnuseABAndManifest_002()
    {
        List<string> unusePths = GetUnuseAB();
        foreach (string pth in unusePths)
        {
            if (File.Exists(pth))
            {
                Debug.Log($"删除不用的ab包 : {pth}");
                File.Delete(pth);
            }
        }

    }



    static void ClearUnuseFolderAndMeta_002() // dir = Application.streamingAssetsPath
    {

        // 要查找子文件夹的根目录路径，这里以 Application.dataPath 为例，即 Assets 文件夹
        string rootDirectory = PathHelper.abDirSAPTH;
        List<string> allSubDirectories = GetAllSubFolders(rootDirectory);

        // 对文件夹路径按字符长度从长到短进行排序（越长的路劲排在越前面）
        allSubDirectories.Sort((a, b) => b.Length - a.Length);

        /*
        // 输出所有子文件夹的名称
        foreach (string directory in allSubDirectories)
        {
            Debug.Log(directory);
        }*/

        // 遍历排序后的目录路径
        foreach (string directory in allSubDirectories)
        {
            if (ShouldDeleteDirectory(directory))
            {
                DeleteDirectoryAndMeta(directory);
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
    static List<string> GetUnuseAB()
    {
        //string manifestBundleName = "GameRes"; // 假设 manifest 文件所在的 AssetBundle 名称
        string manifestAssetName = "AssetBundleManifest"; // 假设 manifest 文件的资源名称

        //string assetBundlePath = Application.streamingAssetsPath + "/GameRes/" + manifestBundleName;
        string assetBundlePath = PathHelper.mainfestSAPTH;
        AssetBundle manifestBundle = AssetBundle.LoadFromFile(assetBundlePath);
        AssetBundleManifest manifest = manifestBundle.LoadAsset<AssetBundleManifest>(manifestAssetName);
        manifestBundle.Unload(false);

        string[] allAssetBundleNames = manifest.GetAllAssetBundles();

        List<string> bundlePathLst = new List<string>();
        foreach (string assetBundleName in allAssetBundleNames)
        {
            // Debug.Log("[1] AssetBundle Name: " + assetBundleName);
            // assetBundleName =  "luban/generatedatas/bytes/i18n_console001.unity3d";
            string pth1 = Path.Combine(PathHelper.abDirSAPTH, assetBundleName);
            bundlePathLst.Add(pth1.Replace("\\", "/"));

        }

        // 主包ab包，是不带".unity3d"结尾的。（"GameRes" 和 “GameRes.manifest”）
        List<string> targetPathLst = new List<string>();  //获取普通包路劲 xxx.unity3d  和  xxx.unity3d.manifest
        targetPathLst.AddRange(GetTargetFilePath(PathHelper.abDirSAPTH, ".unity3d"));
        targetPathLst.AddRange(GetTargetFilePath(PathHelper.abDirSAPTH, ".unity3d.manifest"));

        for (int i = 0; i < targetPathLst.Count; i++)
        {
            targetPathLst[i] = targetPathLst[i].Replace("\\", "/");
        }

        List<string> unusePths = new List<string>();
        foreach (string pth002 in targetPathLst)
        {
            string tempPth = pth002.EndsWith(".unity3d.manifest") ? pth002.Replace(".unity3d.manifest", ".unity3d") : pth002;
            if (!bundlePathLst.Contains(tempPth))
            {
                unusePths.Add(pth002);
            }
        }

        Dictionary<string, object> jsonObject = new Dictionary<string, object>();
        jsonObject.Add("bundlePaths", bundlePathLst);
        jsonObject.Add("allPaths", targetPathLst); //一个包一个资源，所以这里的资源名就是包名？
        jsonObject.Add("unusePaths", unusePths);

        string res = JsonConvert.SerializeObject(jsonObject);

        string pth = PathHelper.hotfixDirSAPTH + "/test_clear_file_info.json";
        // WriteAllText(pth, res);
        File.WriteAllText(pth, res); //会覆盖写入

        Debug.Log($"完成信息：清理文件 {pth}");

        return unusePths;
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

            Debug.Log($"已删除无用的目录: {directoryPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"删除目录 {directoryPath} 时出错: {e.Message}");
        }
    }

    /// <summary>
    /// 删除热更dll
    /// </summary>
    /// <param name="rootDirectory"></param>
    static void ClearHotfixDll_002(string rootDirectory)
    {
        List<string> targetPathLst = new List<string>();  //获取普通包路劲 xxx.unity3d  和  xxx.unity3d.manifest
        targetPathLst.AddRange(GetTargetFilePath(rootDirectory, ".dll.bytes"));

        for (int i = 0; i < targetPathLst.Count; i++)
        {
            File.Delete(targetPathLst[i]);
        }
    }

    /// <summary>
    /// 刷新版本号
    /// </summary>
    static void UpdateVersionData__002()
    {
        //string mainfestSAPTH = Application.streamingAssetsPath + "/GameRes/GameRes";
        //string hotfixDllDirSAPTH = Application.streamingAssetsPath + "/GameDll";
        // string versionSAPTH = Application.streamingAssetsPath + "/GameDll/version_0.json";

        string mainfestSAPTH = PathHelper.mainfestSAPTH;
        Debug.Log("@@@ = " + mainfestSAPTH);
        //string hotfixDllDirSAPTH = PathHelper.hotfixDllDirSAPTH;
        string versionSAPTH = PathHelper.versionSAPTH;

        JObject versionFileSA = JObject.Parse(File.ReadAllText(versionSAPTH));

        string manifestHash = FileUtils.CalculateFileMD5(mainfestSAPTH);
        versionFileSA["asset_bundle"]["manifest"]["hash"] = manifestHash;
        versionFileSA["asset_bundle"]["manifest"]["size_bytes"] = FileUtils.CalculateFileBytes(mainfestSAPTH);
        
        var localManifestAB = AssetBundle.LoadFromFile(mainfestSAPTH);
        AssetBundleManifest localManifest = localManifestAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        localManifestAB.Unload(false);
        string[] abList = localManifest.GetAllAssetBundles();
        JObject bundleCrc = new JObject();

        foreach (var iter in abList)
        {

            string pth = Path.Combine(PathHelper.abDirSAPTH, iter);


            /* crc 无法使用
            uint hash = 0;
            BuildPipeline.GetCRCForAssetBundle(pth, out hash);// 计算本地保存的AB包的CRC值
            */

#if true
            string hash = FileUtils.CalculateFileMD5(pth);
            bundleCrc.Add(iter, hash);

#else
            string hash = FileUtils.CalculateFileMD5(pth);
            long bytes = FileUtils.CalculateFileBytes(pth);
            JObject item = new JObject();
            item.Add("hash", hash);
            item.Add("size_bytes", bytes);
            bundleCrc.Add(iter, item);
#endif
        }

        versionFileSA["asset_bundle"]["bundle_hash"] = bundleCrc;


        JObject hotfixDll = new JObject();

        List<string> targetPathLst = new List<string>();  //获取普通包路劲 xxx.unity3d  和  xxx.unity3d.manifest
        targetPathLst.AddRange(GetTargetFilePath(PathHelper.dllDirSAPTH, ".dll.bytes"));


        for (int i = 0; i < targetPathLst.Count; i++)
        {
            string[] pths = targetPathLst[i].Replace("\\", "/").Split('/');
            string name = pths[pths.Length - 1].Replace(".dll.bytes", "");
            string hash = FileUtils.CalculateFileMD5(targetPathLst[i]);
            long bytes = FileUtils.CalculateFileBytes(targetPathLst[i]);
            JObject item = new JObject();
            item.Add("hash", hash);
            item.Add("size_bytes", bytes);
            hotfixDll.Add(name, item);
        }

        versionFileSA["hotfix_dll"] = hotfixDll;

        versionFileSA["hotfix_dll_load_order"] = JArray.Parse(JsonConvert.SerializeObject(DllHelper.Instance.DllNameList));



        // 资源备份
        JObject assetsBackup = new JObject();
        List<string> backupPathLst = new List<string>();  //获取普通包路劲 xxx.unity3d  和  xxx.unity3d.manifest
        backupPathLst.AddRange(GetTargetFilePath(PathHelper.backupDirSAPTH, ".*"));

        for (int i = 0; i < backupPathLst.Count; i++)
        {
            string name = PathHelper.GetAssetBackupNodeName(backupPathLst[i]);
            string hash = FileUtils.CalculateFileMD5(backupPathLst[i]);
            long bytes = FileUtils.CalculateFileBytes(backupPathLst[i]);
            JObject item = new JObject();
            item.Add("hash", hash);
            item.Add("size_bytes", bytes);
            assetsBackup.Add(name, item);
        }
        versionFileSA["asset_backup"] = assetsBackup;



        #region 修改版本号

        string hotfixVer = versionFileSA["hotfix_version"].Value<string>();
        string targetHFID = versionFileSA["hotfix_key"].Value<string>();

        string appType = ApplicationSettings.Instance.isRelease ? "release" : "debug";
        string hfIDPrefix = $"hf_{appType}_{EditorUserBuildSettings.activeBuildTarget.ToString().ToLower()}";

        //bool isCreatNode = false;

        string[] appVers = ApplicationSettings.Instance.appVersion.Split('.');
        string[] hotfixVers = hotfixVer.Split('.');

        string targetVer;

        if (!targetHFID.StartsWith(hfIDPrefix)
            || hotfixVers[0] != appVers[0]
            || hotfixVers[1] != appVers[1])
        {
            targetVer = $"{appVers[0]}.{appVers[1]}.0";

            long timeMS = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            DateTime localDateTime01 = DateTimeOffset.UtcNow.LocalDateTime;
            string yyyyMMddHHmmss = localDateTime01.ToString("yyyyMMddHHmmss");
            //hf_版本_包类_创建时间
            targetHFID = hfIDPrefix + "_" + yyyyMMddHHmmss;
            //isCreatNode = true;
        }
        else
        {
            int miniVer = int.Parse(hotfixVers[2]) + 1;
            targetVer = $"{hotfixVers[0]}.{hotfixVers[1]}.{miniVer}";
        }


        versionFileSA["hotfix_version"] = targetVer;
        versionFileSA["hotfix_key"] = targetHFID;
        // 修改版本号
        versionFileSA["updated_at"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        #endregion

        Debug.Log($"最新版本号： {versionFileSA["hotfix_version"].Value<string>()}");
        string content = versionFileSA.ToString();
        File.WriteAllText(versionSAPTH, content);
    }



}




public partial class AssetBundleBuilder05 : EditorWindow
{


    public class NopkConfig
    {
        public string[] ignore { get; set; }
    }

    [MenuItem("NewBuild/当前热更版本")]
    private static void ReadCurVersion()
    {

        string content = File.ReadAllText(PathHelper.versionSAPTH);

        JObject verObj = JObject.Parse(content);

        Debug.Log($"当前热更版本号 {verObj["hotfix_version"].Value<string>()}");
    }



    [MenuItem("NewBuild/【测试】-读取yaml文件")]
    private static void TestShowYaml()
    {
        //GetNopkDir();

        Debug.Log(((int)123456.7f).ToString());
    }


    static List<string> nopkDir = new List<string>();

    public static void GetNopkDir()
    {
        nopkDir = new List<string>();

        // 读取 YAML 文件内容
        string yamlFilePath = Application.dataPath + "/" + "nopk.yaml";
        string yamlContent = File.ReadAllText(yamlFilePath);

        // 创建反序列化器
        var deserializer = new DeserializerBuilder()
           .WithNamingConvention(CamelCaseNamingConvention.Instance)
           .Build();

        // 反序列化 YAML 内容到对象
        NopkConfig node = deserializer.Deserialize<NopkConfig>(yamlContent);

        Debug.Log($" NopkConfig : {JsonConvert.SerializeObject(node)}");
        // 输出反序列化后的对象信息

        //Debug.Log($" pth 000 : {Application.dataPath}");
        foreach (string item in node.ignore)
        {
            if (item.EndsWith("/"))
            {
                if (item.StartsWith("./") || item.StartsWith("../"))
                {
                    nopkDir.Add(FileUtils.GetDirWebUrl(yamlFilePath, item).Replace("file:///", "").Replace("\\", "/"));
                    //Debug.Log($" pth : {FileUtils.GetDirWebUrl(yamlFilePath, item).Replace("file:///", "").Replace("\\", "/")}");
                }
            }
        }
    }


}

public partial class AssetBundleBuilder05 : EditorWindow
{



    [MenuItem("NewBuild/【测试】打印StreamingAssets所有AB包名")]
    private static void TestShowStreamingAssetsABInfo()
    {
        ShowStreamingAssetsABInfo();
    }

    public static void ShowStreamingAssetsABInfo() // dir = Application.streamingAssetsPath
    {

        string manifestAssetName = "AssetBundleManifest"; // 假设 manifest 文件的资源名称
        string assetBundlePath = PathHelper.mainfestSAPTH;
        AssetBundle manifestBundle = AssetBundle.LoadFromFile(assetBundlePath);
        AssetBundleManifest manifest = manifestBundle.LoadAsset<AssetBundleManifest>(manifestAssetName);
        manifestBundle.Unload(false);


        Dictionary<string, string> bundleNameLst = new Dictionary<string, string>();
        Dictionary<string, string> AssetNameLst = new Dictionary<string, string>();
        // 获取所有 AssetBundle 名称
        string[] allAssetBundleNames = manifest.GetAllAssetBundles();

        int i = 0;
        foreach (string assetBundleName in allAssetBundleNames)
        {
            // Debug.Log("[1] AssetBundle Name: " + assetBundleName);

            i++;
            bundleNameLst.Add($"[{i}]", $"{assetBundleName}");

            // 加载该 AssetBundle
            string abSAPTH = Path.Combine(PathHelper.abDirSAPTH, assetBundleName);
            AssetBundle subAssetBundle = AssetBundle.LoadFromFile(abSAPTH);


            // 获取该 AssetBundle 中的所有资源名称
            string[] allAssetNames = subAssetBundle.GetAllAssetNames();
            foreach (string assetName in allAssetNames)
            {
                //Debug.Log("[2] Asset Name: " + assetName);
                AssetNameLst.Add($"#[{i}]", $"{assetName}");
            }

            // 卸载该 AssetBundle
            subAssetBundle.Unload(false);
        }

        Dictionary<string, object> jsonObject = new Dictionary<string, object>();

        jsonObject.Add("bundleNames", bundleNameLst);
        jsonObject.Add("assetNames", AssetNameLst); //一个包一个资源，所以这里的资源名就是包名？

        string res = JsonConvert.SerializeObject(jsonObject);


        string localAllABJsonPath = Path.Combine(PathHelper.hotfixDirSAPTH, "test_all_ab_info.json");
        WriteAllText(localAllABJsonPath, res);
        //Debug.Log($"遍历完成：  {res}");
        Debug.Log($"已生成所有ab包信息： {localAllABJsonPath}");
    }


    static void WriteAllText(string path, string content)
    {
        string directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllText(path, content); //会覆盖写入

    }


    [MenuItem("NewBuild/【测试】打印要清除的文件")]
    private static void TestCleatFile()
    {
        GetUnuseAB();
    }









    [MenuItem("NewBuild/【测试】-打印ab包calendar.unity3d的CRC")]
    private static void TestShowAbCrc()
    {

        string iter = "games/console001/calendar bundle/prefabs/calendar.unity3d";

        string mainfestSAPTH = PathHelper.mainfestSAPTH;
        var serverManifestAB = AssetBundle.LoadFromFile(mainfestSAPTH);
        AssetBundleManifest serverManifest = serverManifestAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        serverManifestAB.Unload(false);

        Hash128 serverHash = serverManifest.GetAssetBundleHash(iter);


        Debug.Log(Application.streamingAssetsPath);
        string targetFilePth = PathHelper.GetAssetBundleSAPTH(iter);

        //string targetFilePth = Path.Combine(Application.streamingAssetsPath, "/GameRes/games/console001/calendar bundle/prefabs/calendar.unity3d"); 这样写有问题
        //string targetFilePth = Application.streamingAssetsPath + "/GameRes/games/console001/calendar bundle/prefabs/calendar.unity3d";
        if (File.Exists(targetFilePth))
        {

            AssetBundle ab = AssetBundle.LoadFromFile(targetFilePth);
            //AssetBundle ab = AssetBundle.LoadFromMemory(result);
            int hash = ab.GetHashCode();
            ab.Unload(false);

            // 【待完成】这里需要进行下载数据校验
            uint calculatedCRC;
            // 计算本地保存的AB包的CRC值
            BuildPipeline.GetCRCForAssetBundle(targetFilePth, out calculatedCRC);
            Debug.Log($"BuildPipeline.GetCRCForAssetBundle: {calculatedCRC} -- AssetBundle.GetHashCode: {hash} -- Manifest.GetAssetBundleHash: {serverHash}");

        }
        else
        {
            Debug.Log("不存在文件 ：" + targetFilePth);
        }

        /*
        Hash128 serverHash = serverManifest.GetAssetBundleHash(iter);
        uint calculatedCRC;
        BuildPipeline.GetCRCForAssetBundle(targetFilePth, out calculatedCRC);
        serverHash 和 calculatedCRC 如何比较*/
    }

}