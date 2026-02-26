using SBoxApi;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SBoxInit : MonoSingleton<SBoxInit>
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
        StartCoroutine(CheckSBoxReady());
    }

    private void AddEventListener()
    {
        EventCenter.Instance.AddEventListener<SBoxResetData>(SBoxEventHandle.SBOX_RESET, OnSBoxReset);
        EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_SADNBOX_RESET, OnSBoxSanboxRest);
    }

    private void OnSBoxReset(SBoxResetData resetData)
    {
        if (Application.isEditor)
            onComplete?.Invoke();
        else
            StartCoroutine(CheckSanboxReady());
    }

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
        SBoxSandboxListener.Instance.Init();
        SBoxSandbox.Reset();
    }

    private IEnumerator CheckSBoxReady()
    {   
        while (!SBoxModel.Instance.isSboxReady)
        {
            Debug.Log("Check SBoxIdea ready...");
            SBoxModel.Instance.isSboxReady = SBoxIdea.Ready();
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("SBoxIdea Reset...");
        SBoxIdea.Reset();
    }
}
