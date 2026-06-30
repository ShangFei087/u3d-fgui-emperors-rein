using UnityEngine;

/// <summary>
/// AB 内 PAG 原始二进制载体（避免 TextAsset UTF-8 破坏 .pag）。
/// </summary>
public class PagBinaryAsset : ScriptableObject
{
    public byte[] data;
}
