using System.Collections;
using UnityEngine;
using UnityEngine.Events;


public class MonoHelper : MonoBehaviour
{
    public UnityEvent updateHandle = new UnityEvent();

    public UnityEvent fixeUpdateHandle = new UnityEvent();


    // Update is called once per frame
    void Update()
    {
        updateHandle?.Invoke();
    }

    private void FixedUpdate()
    {
        fixeUpdateHandle?.Invoke();
    }

    public void DoCo(Coroutine co, IEnumerator ie)
    {
        if (co != null)
            StopCoroutine(co);
        co = StartCoroutine(ie);
    }
    /*
    IEnumerator func(Coroutine cor, IEnumerator ie)
    {
        yield return ie;
        co = null;
    }
    IEnumerator func()
    {
        yield return null;
    }*/
}
