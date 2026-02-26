using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ForceGC : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DoGC());
    }

    float timeS =  60 * 20;
    IEnumerator DoGC()
    {
        while (true)
        {
           // 强制触发垃圾回收
           GC.Collect();
           // 等待所有终结器执行完毕（可选，确保回收彻底）
           GC.WaitForPendingFinalizers();
           // 再次触发一次回收（可选，处理终结器执行后产生的新垃圾）
           GC.Collect();

            DebugUtils.Log("强制GC");

            yield return new WaitForSecondsRealtime(timeS);
        }
    }
}
