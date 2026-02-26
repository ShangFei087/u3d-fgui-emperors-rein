using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResponseInfo02
{
    public int seqID;
    public Action<object> successCallback;

    /// <summary>
    /// 失败回调
    /// </summary>
    /// <remark>
    /// * 失败函数可能是接口自定义的失败数据。
    /// * 超时、重复调用也会调用失败函数！
    /// </remark>
    public Action<BagelCodeError> errorCallback;
    public float runTimeS;

    /// <summary> 使用标签来删除请求 </summary>
    public string mark;
}

public  class ProxyHelper<T> : MonoBehaviour where T : MonoBehaviour
{

    #region 单例

    private static T _instance;
    private static object _mutex = new object();
    private static bool applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            if (applicationIsQuitting)
            {
                Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                    "' already destroyed on application quit." +
                    " Won't create again - returning null.");
                return null;
            }

            lock (_mutex)
            {
                if (_instance == null)
                {
                    var founds = FindObjectsOfType(typeof(T));
                    if (founds.Length > 1)
                    {
                        Debug.LogError("[Singleton] Singlton '" + typeof(T) +
                            "' should never be more than 1!");
                        return null;
                    }
                    else if (founds.Length > 0)
                    {
                        _instance = (T)founds[0];

                        if (_instance.transform.parent == null) // 判断是否是根节点，
                            DontDestroyOnLoad(_instance.gameObject);
                    }
                    else
                    {
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<T>();
                        singleton.name = "(Singleton) " + typeof(T).ToString();

                        DontDestroyOnLoad(singleton);
                    }
                }

                return _instance;
            }
        }
    }

    protected virtual void OnDestroy()
    {
        applicationIsQuitting = true;
    }

    #endregion






    protected string prefix;
    protected bool isDebugLog = true;
    protected float receiveOvertimeS = 1000;

    protected Dictionary<string, ResponseInfo02> dicResponse = new Dictionary<string, ResponseInfo02>();
    private int seqID = 0;
    protected int CreatSeqID()
    {
        List<int> temp = new List<int>();
        foreach (KeyValuePair<string, ResponseInfo02> kv in this.dicResponse)
            temp.Add(kv.Value.seqID);
        do
        {
            if (++this.seqID > 10000)
                this.seqID = 1;
        } while (temp.Contains(seqID));
        return seqID;
    }

    public void RemoveRequestAt(int seqID)
    {
        int idx = dicResponse.Count;
        while (--idx >= 0)
        {
            KeyValuePair<string, ResponseInfo02> item = dicResponse.ElementAt(idx);
            if (item.Value.seqID == seqID)
            {
                dicResponse.Remove(item.Key);
            }
        }
    }

    public void RemoveRequestAt(string mark)
    {
        int idx = dicResponse.Count;
        while (--idx >= 0)
        {
            KeyValuePair<string, ResponseInfo02> item = dicResponse.ElementAt(idx);
            if (item.Value.mark == mark)
            {
                dicResponse.Remove(item.Key);
            }
        }
    }





    bool isDirty = true;
    public void Update()
    {
        if (!isDirty) return;
        isDirty = false;

        CheckRequestOvertime();

        isDirty = true;
    }

    public void CheckRequestOvertime()
    {
        if (receiveOvertimeS == -1)
            return;

        int idx = dicResponse.Count;
        while (--idx >= 0)
        {
            KeyValuePair<string, ResponseInfo02> item = dicResponse.ElementAt(idx);
            if ( Time.unscaledTime - item.Value.runTimeS > receiveOvertimeS)
            {
                dicResponse.Remove(item.Key);
                BagelCodeError res = new BagelCodeError() { msg = "Request is overtime", };
                DebugUtils.LogWarning($"==@ {prefix}<color=red>error</color>:  {item.Key} ; Request is overtime ; {receiveOvertimeS}s");
                item.Value.errorCallback?.Invoke(res);
            }
        }
    }



    protected void OnDebugRpcDown(string eventName, object res)
    {
        if (isDebugLog)
            DebugUtils.LogWarning($"==@ {prefix}<color=yellow>rpc down</color>: {eventName}  data: {JsonConvert.SerializeObject(res)}");
    }
    protected void OnDebugRpcUp(string eventName, object req)
    {
        if (isDebugLog)
            DebugUtils.LogWarning($"==@ {prefix}<color=green>rpc up</color>: {eventName}  data: {JsonConvert.SerializeObject(req)}");
    }



    public int OnRequestBefore(string eventName, object req, Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark)
    {
        OnDebugRpcUp(eventName, req);

        int seqId = CreatSeqID();

        if (!dicResponse.ContainsKey(eventName))
            dicResponse.Add(eventName, new ResponseInfo02() { });
        else
        { //已有的删除
            BagelCodeError res = new BagelCodeError() { msg = "Request is repeated", };
            DebugUtils.LogWarning($"==@ {prefix}:<color=red>warn</color>:  {eventName} ; Request is repeated");
            dicResponse[eventName].errorCallback?.Invoke(res);
        }
        dicResponse[eventName].successCallback = successCallback;
        dicResponse[eventName].errorCallback = errorCallback;
        dicResponse[eventName].seqID = seqId;
        dicResponse[eventName].runTimeS = Time.unscaledTime;
        dicResponse[eventName].mark = mark;

        return seqId;
    }

    public void OnResponse(string eventName, object res)
    {
        OnDebugRpcDown(eventName, res);
        if (dicResponse.ContainsKey(eventName))
        {
            ResponseInfo02 info = dicResponse[eventName];
            dicResponse.Remove(eventName);
            info.successCallback?.Invoke(res);
        }
    }



}
