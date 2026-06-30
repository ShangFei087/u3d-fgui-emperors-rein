using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// PAG 资源 Reimport 与命令行打包入口。
/// Unity -batchmode -quit -projectPath ... -executeMethod PagBuildHelper.BuildFromCommandLine
/// </summary>
public static class PagBuildHelper
{
    private const string PagFolder = "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Pag";

    [MenuItem("NewBuild/Reimport PAG 资源")]
    public static void ReimportPagAssets()
    {
        if (!Directory.Exists(PagFolder))
        {
            Debug.LogError($"PAG 目录不存在: {PagFolder}");
            return;
        }

        AssetDatabase.ImportAsset(PagFolder, ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();
        Debug.Log($"PAG Reimport 完成: {PagFolder}");
    }

    [MenuItem("NewBuild/Reimport PAG 并打包 StreamingAssets")]
    public static void ReimportAndBuild()
    {
        ReimportPagAssets();
        AssetBundleBuilder05.BuildPigSlotGameResource002();
    }

    public static void BuildFromCommandLine()
    {
        ReimportAndBuild();
        EditorApplication.Exit(0);
    }
}

