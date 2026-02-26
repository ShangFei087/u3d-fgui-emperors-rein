using System.Collections;
using UnityEngine;
using System;

public static class CoUtils 
{
    static public void StartCo(MonoBehaviour mono, Coroutine co, IEnumerator routine)
    {
        ClearCo(mono,co);
        co = mono.StartCoroutine(routine);
    }
    static public void ClearCo(MonoBehaviour mono, Coroutine co)
    {
        if (co != null)
            mono.StopCoroutine(co);
        co = null;
    }
    static public IEnumerator DelayTask(Action task, int timeMS)
    {
        yield return new WaitForSeconds((float)timeMS / 1000f);
        task?.Invoke();
    }
    static public IEnumerator RepeatTask(Action task, int timeMS)
    {
        while (true)
        {
            yield return new WaitForSeconds((float)timeMS / 1000f);
            task?.Invoke();
        }
    }
}
