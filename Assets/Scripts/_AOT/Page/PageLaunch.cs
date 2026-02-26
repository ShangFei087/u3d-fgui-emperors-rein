#define DISABLE_DELAY
using System.Collections.Generic;
using UnityEngine;
using System;
using FairyGUI;
using UnityEditor;


public class LoadingProgress
{

    /// <summary> 复制包体中的hotfix dll 到缓存</summary>
    public const string COPY_SA_HOTFIX_DLL = "COPY_SA_HOTFIX_DLL";
    /// <summary> 复制包体中AB包到缓存 </summary>
    public const string COPY_SA_ASSET_BUNDLE = "COPY_SA_ASSET_BUNDLE";
    /// <summary> 复制备份文件 </summary>
    public const string COPY_SA_ASSET_BACKUP = "COPY_SA_ASSET_BACKUP";

    /// <summary> 检查有无待拷贝的文件 </summary>
    public const string CHECK_COPY_TEMP_HOTFIX_FILE = "CHECK_COPY_TEMP_HOTFIX_FILE";

    /// <summary> 检查网络热更版本 </summary>
    public const string CHECK_WEB_VERSION = "CHECK_WEB_VERSION";

    /// <summary> 下载hotfix dll </summary>
    public const string DOWNLOAD_HOTFIX_DLL = "DOWNLOAD_HOTFIX_DLL";

    /// <summary> 下载热更AB包 </summary>
    public const string DOWNLOAD_ASSET_BUNDLE = "DOWNLOAD_ASSET_BUNDLE";

    /// <summary> 下载"资源备份" </summary>
    public const string DOWNLOAD_ASSET_BACKUP = "DOWNLOAD_ASSET_BACKUP";


    /// <summary> 拷贝下载的文件 </summary>
    public const string COPY_TEMP_HOTFIX_FILE = "COPY_TEMP_HOTFIX_FILE";


    /// <summary> 删除无用的ab包 </summary>
    public const string DELETE_UNUSE_ASSET_BUNDLE = "DELETE_UNUSE_ASSET_BUNDLE";

    /// <summary> 删除无用的hotfix dll </summary>
    public const string DELETE_UNUSE_HOTFIX_DLL = "DELETE_UNUSE_HOTFIX_DLL";

    /// <summary> 加载AOT dll到内存 </summary>
    //public const string LOAD_AOT_DLL = "LOAD_AOT_DLL";

    /// <summary> 补充元数据给AOT,而不是给热更新dll补充元数据</summary>
    public const string LOAD_AOT_META_DATA = "LOAD_AOT_META_DATA";

    /// <summary> 加载hotfix dll到内存 </summary>
    public const string LOAD_HOTFIX_DLL = "LOAD_HOTFIX_DLL";


    /// <summary> 预加载AB包到内存 </summary>
    public const string PRELOAD_ASSET_BUNDLE = "PRELOAD_ASSET_BUNDLE";

    /// <summary> 预加载资源到内存 </summary>
    public const string PRELOAD_ASSET = "PRELOAD_ASSET";

    /// <summary> 链接机台（获取参数） </summary>
    public const string CONNECT_MACHINE = "CONNECT_MACHINE";

    /// <summary> 初始化参数设置 </summary>
    public const string INIT_SETTINGS = "INIT_SETTINGS";

    /// <summary> 进入游戏(游戏加载界面) </summary>
    public const string ENTER_GAME = "ENTER_GAME";
}

public class PageLaunch 
{
    private static PageLaunch _instance;
    public static PageLaunch Instance
    {
        get
        {
            if (_instance == null)
            {
                if(UIPackage.GetByName("Native") == null)
                    UIPackage.AddPackage("Native/FGUIs/Native"); // Native/FGUIs/ == Resources/Native/FGUIs
                _instance = new PageLaunch();
                _instance.goOwnerPage = UIPackage.CreateObject("Native", "PageLaunch").asCom;
                GRoot.inst.AddChild(_instance.goOwnerPage);
                _instance.goOwnerPage.sortingOrder = 99;
            }
            return _instance;
        }
    }

    GComponent goOwnerPage;

    GLoader lodBG, lodLogo;

    GProgressBar pbLoading;

    GTextField txtMsg;

    GButton btnQuit;

    Dictionary<string, int> allProgress = new Dictionary<string, int>();

    Dictionary<string, int> curProgress = new Dictionary<string, int>();


    public class ShowMsgInfo
    {
        public string msg;
        public float progress = 0;
    }
    List<ShowMsgInfo> msgLst = new List<ShowMsgInfo>();


    private void InitParam()
    {

        btnQuit = goOwnerPage.GetChild("btnQuit").asButton;
        btnQuit.onClick.Clear();
        btnQuit.onClick.Add(DoAplicationQuit);
        btnQuit.visible = !ApplicationSettings.Instance.isRelease;


        pbLoading = goOwnerPage.GetChild("progress").asProgress;
        txtMsg = goOwnerPage.GetChild("title").asTextField;
        lodLogo = goOwnerPage.GetChild("logo").asLoader;
        lodLogo.url = ApplicationSettings.Instance.logoUrl;

        msgLst.Clear();
        isError = false;
        allProgress.Clear();
        curProgress.Clear();
        pbLoading.value = 0;
        txtMsg.text = "";

        Type t = typeof(LoadingProgress);
        var fields = t.GetFields();
        foreach (var fieldInfo in fields)
        {
            allProgress.Add((string)fieldInfo.GetRawConstantValue(), 0);
            curProgress.Add((string)fieldInfo.GetRawConstantValue(), 0);
        }


        Timers.inst.Remove(Update);
        Timers.inst.Add(0.1f, 0, Update);
    }


    void DoAplicationQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 编辑器中退出播放模式
#else
                    Application.Quit(); // 构建后退出应用
#endif
    }

    /// <summary>
    /// <p>获取进度条的值</p>
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// * 将进度条的加载分成多段。<br/>
    /// * 每段进度，再分成多个小段任务。<br/>
    /// </remarks>
    float GetProgressValue()
    {
        float partOne = 1f / (float)(allProgress.Count);

        float A = (float)(allProgress.Count - curProgress.Count) * partOne;

        float B = 0f;

        foreach (KeyValuePair<string, int> kv in curProgress)
        {
            int hasDoNum = allProgress[kv.Key] - kv.Value;
            if (hasDoNum > 0)
            {
                B += (float)hasDoNum * (partOne / (float)allProgress[kv.Key]);
            }
        }
        //Debug.Log($"@ A = {A} , B = {B} , C = {A + B} , partOne = {partOne}");
        return A + B;
    }

    /// <summary>
    /// <p>添加某个进度任务</p>
    /// </summary>
    /// <param name="mark">进度"mark"</param>
    /// <param name="count">该进度的任务个数</param>
    public void AddProgressCount(string mark, int count)
    {
        //新写法(支持重复校验-热更版本文件 - 避免网络连接延时)
        if (!curProgress.ContainsKey(mark))
        {
            curProgress.Add(mark, 0);
        }
        curProgress[mark] += count;
        allProgress[mark] += count;
    }


    /// <summary>
    /// <p>删除某个进度任务</p>
    /// </summary>
    /// <param name="mark">进度"mark"</param>
    public void RemoveProgress(string mark)
    {
        if (curProgress.ContainsKey(mark))
            curProgress.Remove(mark);
    }



    /// <summary>
    /// <p>显示加载进度和信息</p>
    /// </summary>
    /// <param name="mark">进度"mark"</param>
    /// <param name="str">显示的信息</param>
    /// <remarks>
    /// * 仅仅是界面的显示。<br/>
    /// * 不做任何的数据修改。<br/>
    /// </remarks>
    public void Next(string mark, string str)
    {
        if (isError) return;

        if (curProgress.ContainsKey(mark))
        {
            if (--curProgress[mark] < 0)
                curProgress[mark] = 0;
        }
        float val = GetProgressValue();

        msgLst.Add(new ShowMsgInfo()
        {
            msg = CreatStr(str, val),
            progress = val,
        });

#if DISABLE_DELAY
        ShowProgressUIMsg();
#endif
    }


    bool isError = false;
    public void Error(string str)
    {
        isError = true;
        txtMsg.text = str;
    }

    public void Finish(string str)
    {
        msgLst.Add(new ShowMsgInfo()
        {
            msg = CreatStr(str, 1),
            progress = 1,
        });
#if DISABLE_DELAY
        ShowProgressUIMsg();
#endif

    }


    string CreatStr(string str, float pg)
    {
        string _pg = (pg * 100f).ToString("N1");
        return ApplicationSettings.Instance.isRelease ? $"{_pg}%" : $"{str}  ({_pg})%";
    }

    public void Open()
    {
        goOwnerPage.visible = true;
        InitParam();
    }


    Coroutine corClose = null;
    public void Close(float delayS = -1)
    {
        if (delayS > 0)
        {
            DelayToClose(delayS);
            return;
        }
        CloseSelf(null);
    }

  
    void DelayToClose(float delayS)
    {
        Timers.inst.Remove(CloseSelf);
        Timers.inst.Add(delayS, 1, CloseSelf);
    }

    void CloseSelf(object data)
    {
        Timers.inst.Remove(Update);
        goOwnerPage.visible = false;
    }






    float lastRunTimeS = 0;
    public void Update(object data)
    {
#if DISABLE_DELAY
        return;
#endif
        if (isError)
            return;

        float nowRunTimeS = Time.unscaledTime;
        if (nowRunTimeS - lastRunTimeS > 0.2f)
        {
            lastRunTimeS = nowRunTimeS;
            ShowProgressUIMsg();
        }
    }


    float curShowProgress = 0f;
    string curShowMsg = "";
    void ShowProgressUIMsg()
    {
        if (isError)
            return;

        if (msgLst.Count > 0)
        {
            ShowMsgInfo msgInfo = msgLst[0];
            msgLst.RemoveAt(0);

            curShowProgress = msgInfo.progress;
            curShowMsg = msgInfo.msg;

            pbLoading.value = curShowProgress;
            txtMsg.text = curShowMsg;
        }
    }

    /// <summary>
    /// 只显示内容
    /// </summary>
    /// <param name="msg"></param>
    public void RefreshProgressUIMsg(string msg)
    {
        if (isError)
            return;

        curShowMsg = CreatStr(msg, curShowProgress);

        pbLoading.value = curShowProgress;
        txtMsg.text = curShowMsg;
    }
}
