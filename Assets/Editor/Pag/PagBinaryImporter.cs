using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

/// <summary>
/// 将 .pag 二进制导入为 PagBinaryAsset，打进 AB 后运行时按 byte[] 解压。
/// </summary>
[ScriptedImporter(2, "pag")]
public class PagBinaryImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        byte[] data = File.ReadAllBytes(ctx.assetPath);
        var asset = ScriptableObject.CreateInstance<PagBinaryAsset>();
        asset.data = data;
        ctx.AddObjectToAsset("main", asset);
        ctx.SetMainObject(asset);
    }
}
