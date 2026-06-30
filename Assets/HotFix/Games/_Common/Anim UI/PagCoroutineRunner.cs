using System.Collections;
using UnityEngine;

/// <summary>
/// 兼容入口：协程实际由 PagCallbackHub 单节点承载。
/// </summary>
public sealed class PagCoroutineRunner : MonoBehaviour
{
    public static PagCallbackHub Instance => PagCallbackHub.EnsureInstance();

    public Coroutine RunCoroutine(IEnumerator routine) => Instance.RunCoroutine(routine);

    public void StopRunCoroutine(Coroutine routine) => Instance.StopRunCoroutine(routine);
}
