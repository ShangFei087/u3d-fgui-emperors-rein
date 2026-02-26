using System.Collections;
using System;
using UnityEngine;

/// <summary>
/// 普通脚本都可用上协程
/// </summary>
public class CoroutineAssistant : MonoSingleton<CoroutineAssistant>
{
    private CoController _corCtrl;
    private CoController corCtrl
    {
        get
        {
            if (_corCtrl == null)
            {
                _corCtrl = new CoController(this);
            }
            return _corCtrl;
        }
    }

   static public void ClearCo(string name) => Instance?.corCtrl.ClearCo(name);

   //static public void ClearAllCor() => Instance.corCtrl.ClearAllCor();

   static public void DoCo(string name, IEnumerator routine) => Instance?.corCtrl.DoCo(name, routine);

   static public bool IsCo(string name) => Instance == null? false: Instance.corCtrl.IsCo(name);

   static public IEnumerator DoTaskRepeat(Action cb, int ms) => Instance?.corCtrl.DoTaskRepeat(cb, ms);

   static public IEnumerator DoTask(Action cb, int ms) => Instance?.corCtrl.DoTask(cb, ms);



    void ClearAllCorSelf() => corCtrl.ClearAllCo();
    protected override void OnDestroy()
    {
        ClearAllCorSelf();
        base.OnDestroy();
    }

}
