#define TEST_USE_REMOTE_AB
using UnityEngine;
using System;
using System.IO;
using Sirenix.OdinInspector;
using Newtonsoft.Json.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif


[Flags]
public enum LogFilter
{
    System 		= (1 << 0),
    Unity		= (1 << 1),
    NodeCanvas	= (1 << 2),
    Bundle		= (1 << 3),
    Scene		= (1 << 4),
    Network		= (1 << 5),
    Analytics	= (1 << 6),
    Performance	= (1 << 7),
    TestSuite	= (1 << 8),
    Test		= (1 << 9)
};

/*[Serializable]
public class DesignResolutionInfo
{
    public float height = 1280f;
    public float width = 720f;
}*/


[Serializable]
//u3d编辑器，右键点击Create/SlotMaker/ScriptableObject/ApplicationSettings 创建 ApplicationSettings.asset文件
[CreateAssetMenu(fileName = "ApplicationSettings", menuName = "GameMaker/ScriptableObject/ApplicationSettings")]
public partial class ApplicationSettings : ScriptableObjectSingleton<ApplicationSettings>//public class ApplicationSettings : ScriptableObject//
{



    [Space]
    [Title("客户端设置")]


    [Tooltip("是否是机台包")]
    public bool isMachine;


    [Tooltip("是否是正式包")]
    public bool isRelease = false;

    /*
     * 在Build Settings 里定义一个宏RELEASE 来决定是否是Release包，是不可取的！
     * RELEASE 不会在包体里。只是在打包编译时，确定放出那一块代码！
     * 如果热更代码中有，#if RELEASE ...  #else ... #endif 。 则编译时 打开RELEASE和 关闭RELEASE，热更代码这两块都可能有效。 
     */

    [Tooltip("是否是测试数据")]
    public bool isMock;

    [Tooltip("是否开启防护功能")]
    public bool isUseProtectApplication = false;

    [Tooltip("平台名称")]
    public string platformName = "EmperorsRein200";

    [Tooltip("主题名称")]
    public string gameTheme = "EMPERORS REIN";

    [Tooltip("代理商名")]
    public string agentName = "EmperorsRein200";

    // 平台yyyddmmhhmmss + 6为随机码？？
    // 平台_yyyddmmhhmms
    //appkey是唯一的，（即使是同个clientVersion的苹果、安卓、机台、PC包，appkey都是唯一的）
    [Tooltip("app包key")]
    public string appKey;


    [Tooltip("客户端版本")]
    public string appVersion = "1.0.0";


    [Tooltip("资源服务器")]
    public string resourceServer = "http://8.138.140.180:8124";

    public string platformResourceServerUrl => $"{resourceServer}/{platformName}";


    [Space]
    [Title("机台设置")]


    [Tooltip("机台调试url")]
    public string machineDebugUrl = "192.168.3.82";//"192.168.3.82:8092";

    [Space]
    [Title("游戏配置")]

    [Tooltip("数据上报url")]
    public string reportUrl = "http://192.168.3.152/api/game_log/send";

    

    [Tooltip("启动页海报路劲")]
    public string posterUrl = "";

    [Tooltip("启动页logo路劲")]
    public string logoUrl = "Assets/Resources/Common/Sprites/g152_icon.png";
    // public string logoUrl = "Assets/Resources/Common/Sprites/g152_icon.png";

    [Tooltip("游戏数据库名")]
    public  string dbName = "Games.db";

    [Space]
    [Title("测试")]
    [Tooltip("在编辑器，测试热更功能")]
    public bool isTestUseHotfixBundleAtEditor = false;
    public bool IsUseHotfixBundle()
    {
        if (Application.isEditor && isTestUseHotfixBundleAtEditor)
        {
            return true;
        }
        return !Application.isEditor;
    }


    [Tooltip("在编辑器，测试StreamingAssets Bundle功能")]
    public bool isTestUseStreamingAssetsBundleAtEditor = false;
    public bool IsUseStreamingAssetsBundle()
    {
        if (Application.isEditor && isTestUseStreamingAssetsBundleAtEditor)
        {
            return true;
        }
        return !Application.isEditor;
    }


    /**/
    [Tooltip("在编辑器，测试机台按钮")]
    public bool isTestMachineButtonAtEditor = false;
    public bool IsMachine()
    {
        if (Application.isEditor && isTestMachineButtonAtEditor)
        {
            return true;
        }
        return isMachine;
    }

    [Title("其他")]

    public LogFilter logFilter { get; set; }

    public static int GetClientVersionNumber()
    {
    	string[] versions = Instance.appVersion.Split(new char[]{ '.' });
    	return Int32.Parse(versions[0]) * 10000 + Int32.Parse(versions[1]) * 100 + Int32.Parse(versions[2]);
    }
    public static int GetClientMajorVersionNumber()
    {
    	string[] versions = Instance.appVersion.Split(new char[]{ '.' });
    	return Int32.Parse(versions[0]);
    }

    public static string GetPlatformName()
    {
#if UNITY_EDITOR
        return GetPlatformName(EditorUserBuildSettings.activeBuildTarget);
#else
        return GetPlatformName(Application.platform);
#endif
    }

#if UNITY_EDITOR
    private static string GetPlatformName(BuildTarget buildTarget)
    {
        switch (buildTarget)
        {
        case BuildTarget.Android:
    		return "Android";
    	case BuildTarget.iOS:
    		return "iOS";
    	case BuildTarget.WebGL:
    		return "Canvas";
        case BuildTarget.WSAPlayer:
    		return "Windows";
    	case BuildTarget.StandaloneWindows:
    	case BuildTarget.StandaloneWindows64:
            return "Gameroom";
    	case BuildTarget.StandaloneOSX:
    		return "OSX_Standalone";
    		// Add more build targets for your own.
    		// If you add more targets, don't forget to add the same platforms to GetPlatform(RuntimePlatform) function.
    	default:
    		return null;
    	}
    }
#endif

    private static string GetPlatformName(RuntimePlatform runtimePlatform)
    {
        switch (runtimePlatform)
        {
        case RuntimePlatform.Android:
#if PLATFORM_AMAZON
            return "Amazon";
#else
            return "Android";
#endif
        case RuntimePlatform.IPhonePlayer:
            return "iOS";
        case RuntimePlatform.WebGLPlayer:
            return "Canvas";
        case RuntimePlatform.WSAPlayerARM:
        case RuntimePlatform.WSAPlayerX64:
        case RuntimePlatform.WSAPlayerX86:
            return "Windows";
        case RuntimePlatform.WindowsPlayer:
            return "Gameroom";
        case RuntimePlatform.OSXPlayer:
            return "OSX_Standalone";
            // Add more build targets for your own.
            // If you add more targets, don't forget to add the same platforms to GetPlatform(RuntimePlatform) function.
        default:
            return "UNKNOWN";
        }
    }

    public static string GetStreamingAssetsPath()
    {
        return Application.streamingAssetsPath;
    }



    public static string GetDeviceModel()
    {
        string deviceModel = SystemInfo.deviceModel;
        if (string.IsNullOrEmpty (deviceModel)) {
            deviceModel = "ModelUnknown";
        }
        return deviceModel;
    }

    public static string GetOperatingSystem()
    {
        string operatingSystem = SystemInfo.operatingSystem;
        if (string.IsNullOrEmpty (operatingSystem)) {
            operatingSystem = "Unknown";
        }

        return operatingSystem;
    }

    public static string GetDeviceType()
    {
#if UNITY_IPHONE && !UNITY_EDITOR
        if(SystemInfo.deviceModel.Contains("iPad"))
        {
            return "IPAD";
        }
        else
        {
            return "IPHONE";
        }
#elif UNITY_ANDROID && PLATFORM_AMAZON && !UNITY_EDITOR
        return "KINDLE";
#elif UNITY_ANDROID && !UNITY_EDITOR
        return "GOOGLE";
#elif UNITY_WSA && !UNITY_EDITOR
        return "WSA";
#elif UNITY_STANDALONE_WIN && !UNITY_EDITOR
        return "GAMEROOM";
#elif UNITY_WEBGL && !UNITY_EDITOR
        return "CANVAS";
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
        return "IPHONE"; // OSX is treated as iOS in server logic. Thus, treat this test platform like iOS.
#else
        string platform = GetPlatformName();

        if(platform == "Android")
        {
            return "GOOGLE";
        }
        else
        {
            return "IPHONE";
        }
#endif
    }

    public static string GetCachePath()
    {
#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
        return Application.dataPath;
#else
        return Application.temporaryCachePath;
#endif
    }

    public static string GetApplicationStage()
    {
#if BUILD_RROD
        return "prod";
#elif BUILD_ST
        return "st";
#elif BUILD_QA
        return "qa";
#elif BUILD_QA_DEV
        return "qa_dev";
#else
        return "dev";
#endif
    }

    public static bool LogSystem()
    {
    	return (Instance.logFilter & LogFilter.System) == LogFilter.System;
    }

    public static bool LogUnity()
    {
    	return (Instance.logFilter & LogFilter.Unity) == LogFilter.Unity;
    }

    public static bool LogNodeCanvas()
    {
    	return (Instance.logFilter & LogFilter.NodeCanvas) == LogFilter.NodeCanvas;
    }

    public static bool LogBundle()
    {
        return  (Instance.logFilter & LogFilter.Bundle) == LogFilter.Bundle;
    }

    public static bool LogScene()
    {
    	return (Instance.logFilter & LogFilter.Scene) == LogFilter.Scene;
    }

    public static bool LogNetwork()
    {
    	return (Instance.logFilter & LogFilter.Network) == LogFilter.Network;
    }

    public static bool LogAnalytics()
    {
    	return (Instance.logFilter & LogFilter.Analytics) == LogFilter.Analytics;
    }

    public static bool LogPerformance()
    {
    	return (Instance.logFilter & LogFilter.Performance) == LogFilter.Performance;
    }

    public static bool LogTestSuite()
    {
    	return (Instance.logFilter & LogFilter.TestSuite) == LogFilter.TestSuite;
    }

    public static bool LogTest()
    {
        return (Instance.logFilter & LogFilter.Test) == LogFilter.Test;
    }


}



#if UNITY_EDITOR
// 自定义编辑器脚本，用于修改 ExampleScript 在 Inspector 面板的显示
[CustomEditor(typeof(ApplicationSettings))]
public class ApplicationSettingsEditor : Editor
{

    private bool boolParam;
    private int intParam;
    private string stringParam;
    private bool isAot;
    private bool isAutoHotfixUrl=true;
    private string hotfixUrl = "./";



    string GetHotfixUrl()  {
        string[] vers = ApplicationSettings.Instance.appVersion.Split('.');
        string rootFolder = "";
        if (ApplicationSettings.Instance.isRelease)
        {
            rootFolder = string.Join("_", vers);
        }
        else
        {
            rootFolder = vers[0];
        }
        string appType = ApplicationSettings.Instance.isRelease ? "release" : "debug";
        return $"./{appType}/{EditorUserBuildSettings.activeBuildTarget.ToString().ToLower()}/{rootFolder}";
    }


    Color originalColor;

    public override void OnInspectorGUI()
    {
        // 绘制默认的 Inspector 内容
        DrawDefaultInspector();

        // 转换目标对象为 ExampleScript 类型
        //ApplicationSettings exampleScript = (ApplicationSettings)target;


        // ===============================================================

        /*
        // 开始一个垂直布局组
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("测试测试", EditorStyles.boldLabel);

        // 创建布尔类型的输入字段
        boolParam = EditorGUILayout.Toggle("布尔参数", boolParam);
        // 创建整数类型的输入字段
        intParam = EditorGUILayout.IntField("整数参数", intParam);
        // 创建字符串类型的输入字段
        stringParam = EditorGUILayout.TextField("字符串参数", stringParam);

        // 创建一个按钮
        if (GUILayout.Button("确定"))
        {
            //target.CreatVersion();
        }
        // 结束垂直布局组
        EditorGUILayout.EndVertical();
        */


        // ===============================================================


        // 开始一个垂直布局组
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        originalColor = GUI.contentColor;
        GUI.contentColor = Color.green;
        GUILayout.Label("创建app版本", EditorStyles.boldLabel);
        GUI.contentColor = originalColor;


        isAot = EditorGUILayout.Toggle("是否修改AOT代码", isAot);

        isAutoHotfixUrl = EditorGUILayout.Toggle("是否自动计算远程热更目录", isAutoHotfixUrl);
        hotfixUrl = EditorGUILayout.TextField("默认远程热更目录", hotfixUrl);


        // 创建一个按钮
        if (GUILayout.Button("确定"))
        {
            CreatVersion(isAot);
        }
        // 结束垂直布局组
        EditorGUILayout.EndVertical();



        // ===============================================================
        // 开始一个垂直布局组
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        originalColor = GUI.contentColor;
        GUI.contentColor = Color.green;
        GUILayout.Label("回滚app版本", EditorStyles.boldLabel);
        GUI.contentColor = originalColor;

        // 创建一个按钮
        if (GUILayout.Button("确定"))
        {
            GobackVersion();
        }
        // 结束垂直布局组
        EditorGUILayout.EndVertical();


        // ===============================================================
        // 开始一个垂直布局组
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        originalColor = GUI.contentColor;
        GUI.contentColor = Color.green;
        GUILayout.Label("同步app版本", EditorStyles.boldLabel);
        GUI.contentColor = originalColor;

        // 创建一个按钮
        if (GUILayout.Button("确定"))
        {
            GetVersion();
        }
        // 结束垂直布局组
        EditorGUILayout.EndVertical();

    }




    public void CreatVersion(bool isChangeAot = false)
    {
        DateTime localDateTime = DateTimeOffset.UtcNow.LocalDateTime;
        string ms = localDateTime.ToString("yyyyMMddHHmmss");

        string appType = ApplicationSettings.Instance.isRelease ? "release" : "debug";

        string buildTarget = ApplicationSettings.Instance.isMachine ? "machine" : EditorUserBuildSettings.activeBuildTarget.ToString().ToLower();

        ApplicationSettings.Instance.appKey = $"{ApplicationSettings.Instance.platformName}_{appType}_{buildTarget}_{ms}";



        #region 修改 total_version

        JObject totalVersionSAFile = JObject.Parse(File.ReadAllText(PathHelper.totalVersionSAPTH));
        JArray lst = totalVersionSAFile["data"] as JArray;



        string lastAppKey = (lst[0] as JObject)["app_key"].ToObject<string>();
        string lastAppVersion = (lst[0] as JObject)["app_version"].ToObject<string>();


        string[] lastAppKeyInfos = lastAppKey.Split('_');
        string[] lastAppVerInfos = lastAppVersion.Split('.');

        string targetAppVer = "";

        if (isChangeAot)
        {
            string v1 = ApplicationSettings.Instance.isRelease ? "1" : "0";
            targetAppVer = $"{int.Parse(lastAppVerInfos[0]) + 1}.{v1}.0";
        }
        else
        {
            string v1 = lastAppVerInfos[1];
            int v1d = int.Parse(v1) + 1;
            //是否是偶数
            bool isEvenNumber = v1d % 2 == 0;
            if (ApplicationSettings.Instance.isRelease && isEvenNumber)
                v1d++;
            else if (!ApplicationSettings.Instance.isRelease && !isEvenNumber)
                v1d++;
            targetAppVer = $"{lastAppVerInfos[0]}.{v1d}.0";
        }
        ApplicationSettings.Instance.appVersion = targetAppVer;

        JObject nodeItem = new JObject();
        nodeItem.Add("agent_name", ApplicationSettings.Instance.agentName);
        //nodeItem.Add("app", $"{ApplicationSettings.Instance.appKey}.apk");
        nodeItem.Add("app", $"--");
        nodeItem.Add("app_key", ApplicationSettings.Instance.appKey);
        nodeItem.Add("app_version", ApplicationSettings.Instance.appVersion);
        nodeItem.Add("version_suggest", null);

        if (isAutoHotfixUrl)
        {
            nodeItem.Add("hotfix_url", GetHotfixUrl());
        }
        else
        {
            nodeItem.Add("hotfix_url", hotfixUrl);
        }

        lst.Insert(0, nodeItem);

        totalVersionSAFile["updated_at"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        string content = totalVersionSAFile.ToString();
        File.WriteAllText(PathHelper.totalVersionSAPTH, content);
        #endregion

        AssetDatabase.Refresh();
    }



    public void GobackVersion()
    {

        JObject totalVersionSAFile = JObject.Parse(File.ReadAllText(PathHelper.totalVersionSAPTH));
        JArray lst = totalVersionSAFile["data"] as JArray;

        lst.RemoveAt(0);//回滚

        JObject target = lst[0] as JObject;
        string appVersion = target["app_version"].ToObject<string>();


        string appKey = target["app_key"].ToObject<string>();
        string[] appKeyInfos = appKey.Split('_');

        ApplicationSettings.Instance.appKey = appKey;
        ApplicationSettings.Instance.isMachine = appKeyInfos[2] == "machine";
        ApplicationSettings.Instance.isRelease = appKeyInfos[1] == "release";
        ApplicationSettings.Instance.agentName = target["agent_name"].ToObject<string>();
        ApplicationSettings.Instance.appVersion = appVersion;

        string content = totalVersionSAFile.ToString();
        File.WriteAllText(PathHelper.totalVersionSAPTH, content);

        AssetDatabase.Refresh();
    }



    public void GetVersion()
    {

        JObject totalVersionSAFile = JObject.Parse(File.ReadAllText(PathHelper.totalVersionSAPTH));
        JArray lst = totalVersionSAFile["data"] as JArray;

        JObject target = lst[0] as JObject;
        string appVersion = target["app_version"].ToObject<string>();

        string appKey = target["app_key"].ToObject<string>();
        string[] appKeyInfos = appKey.Split('_');

        ApplicationSettings.Instance.appKey = appKey;
        ApplicationSettings.Instance.isMachine = appKeyInfos[2] == "machine";
        ApplicationSettings.Instance.isRelease = appKeyInfos[1] == "release";
        ApplicationSettings.Instance.agentName = target["agent_name"].ToObject<string>();
        ApplicationSettings.Instance.appVersion = appVersion;

        AssetDatabase.Refresh();
    }

}
#endif