using SBoxApi;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SBoxInit02 : MonoSingleton<SBoxInit02>
{
    private UnityAction onComplete;

    public void Init(string matchIp, UnityAction onComplete = null)
    {
        MatchDebugManager.Instance.InitUdpNet(matchIp);
        this.onComplete = onComplete;
        if (!transform.TryGetComponent(out SBox Sbox))
            gameObject.AddComponent<SBox>();

        SBox.Init();
        AddEventListener();
        //StartCoroutine(CheckSBoxReady());
        StartCoroutine(CheckReady());
    }

    private void AddEventListener()
    {
        EventCenter.Instance.AddEventListener<SBoxResetData>(SBoxEventHandle.SBOX_RESET, OnSBoxReset);
        //EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_SADNBOX_RESET, OnSBoxSanboxRest);
    }

    private void OnSBoxReset(SBoxResetData resetData)
    {
        isSBoxIdeaReset = true;
        //StartCoroutine(CheckSanboxReady());
    }

    
     /* 
    private void OnSBoxSanboxRest(int result)
    {
        onComplete?.Invoke();
    }

    private IEnumerator CheckSanboxReady()
    {
        while (!SBoxModel.Instance.isSboxSandboxReady)
        {
            Debug.Log("Check SBoxSandbox ready...");
            SBoxModel.Instance.isSboxSandboxReady = SBoxSandbox.Ready();
            yield return new WaitForSeconds(0.5f);
        }
        
        Debug.Log("SBoxSandbox Reset...");
        SBoxSandbox.Reset();
        SandboxController.Instance.Init();
        SBoxSandboxListener.Instance.Init();
    }

    private IEnumerator CheckSBoxReady()
    {   
        while (!SBoxIdea.Ready())
        {
            Debug.Log("Check SBoxIdea ready...");
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("SBoxIdea Reset...");
        SBoxIdea.Reset();
    }*/


    private bool isSBoxIdeaReset = false;
    private IEnumerator CheckReady()
    {
        float lastTime;
        while (!SBoxIdea.Ready())
        {
            Debug.Log("Check SBoxIdea ready......");
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("SBoxIdea Reset...");

        isSBoxIdeaReset = false;
        SBoxIdea.Reset();

        yield return new WaitUntil(() => isSBoxIdeaReset == true);
        
#if false
        Debug.Log("Check Coin Push Ready ...");
        lastTime = Time.unscaledTime;
        while (SBoxIdea.IsCoinPushReady() != 1)
        {
            yield return new WaitForSeconds(0.5f);

            if (Time.unscaledTime - lastTime > 10f)
            {
                Debug.LogError($"推币机准备失败： {SBoxIdea.GetCoinPushErrors()}");
                yield break;
            }
        }
#endif
        onComplete?.Invoke();
    }
}