using System;
using GameMaker;
using UnityEngine;

public class DebugUtils
{
    private static DebugUtils instance;
    private bool openDebugLog = !ApplicationSettings.Instance.isRelease; //true;
    public static DebugUtils Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new DebugUtils();
                EventCenter.Instance.RemoveEventListener<EventData>("ON_PROPERTY_CHANGED_EVENT", OnPropertyChangedEventIsDebug);
                EventCenter.Instance.AddEventListener<EventData>("ON_PROPERTY_CHANGED_EVENT", OnPropertyChangedEventIsDebug);
            }
            return instance;
        }
    }

    /**/
    /// <summary>
    /// 是否显示日志
    /// </summary>
    /// <param name="isDebug"></param>
    public static void SetOpenDebugLog(bool isDebugLog)
    {
        Instance.openDebugLog = isDebugLog;
    } 
    

    public static void OnPropertyChangedEventIsDebug(EventData res)
    {
        if (res.name == "SBoxModel/isDebugLog")
        {
            Instance.openDebugLog = (bool)res.value;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="msg"></param>
    /// <remarks>
    /// * 受到日志开关的影响。
    /// </remarks>
    public static void Log(object msg)
    {
        //return;
        if (Instance.openDebugLog == false)
            return;

        if (DebugFilter.CheckFilter(msg))
            return;

        Debug.Log(msg);
    }

    public static void LogFormat(string format, params object[] args)
    {
        if (Instance.openDebugLog == false)
            return;
        Debug.LogFormat(format, args);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="msg"></param>
    /// <remarks>
    /// * 受到日志开关的影响。
    /// </remarks>
    public static void LogWarning(object msg)
    {
        if (Instance.openDebugLog == false)
            return;

        if(DebugFilter.CheckFilter(msg))
            return;

        Debug.LogWarning(msg);
    }

    public static void LogError(object msg)
    {
        Debug.LogError(msg);
    }
    public static void LogErrorFormat(string format, params object[] args)
    {
        Debug.LogErrorFormat(format, args);
    }
    public static void LogException(Exception exception)
    {
        Debug.LogException(exception);
    }

    const string SAVE_LOG = "【Log】";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="type"></param>
    /// <remarks>
    /// * 不受日志开关的影响。
    /// </remarks>
    public static void Save(object msg , LogType type = LogType.Log)
    {
        try
        {
            string str = (string)msg;

            if (!str.StartsWith(SAVE_LOG))
                str = $"{SAVE_LOG}{str}";

            switch (type)
            {
                case LogType.Log:
                    {
                        Debug.Log(str);
                    }
                    break;
                case LogType.Warning:
                    {
                        Debug.LogWarning(str);
                    }
                    break;
            }
        }
        catch (Exception e) { }
    }

  
}
