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

    [Tooltip("是否开启展会模式")]
    public bool isExpoMode = false;

    [Tooltip("是否开启防护功能")]
    public bool isUseProtectApplication = false;

    [Tooltip("平台名称")]
    public string platformName = "Treasury";

    [Tooltip("主题名称：仅 Treasury / Savage / Test")]
    public string gameTheme = "Treasury";

    [Tooltip("启动页 FGUI 组件名（Native 包内，如 TreasuryPageLaunch / PageLaunch）；空则按 gameTheme 推断")]
    public string launchFguiName = "TreasuryPageLaunch";

    [Tooltip("代理商名")]
    public string agentName = "Treasury";

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

    [Tooltip("在编辑器，测试展会模式")]
    public bool isTestExpoModeAtEditor = false;
    public bool IsExpoMode()
    {
        if (Application.isEditor && isTestExpoModeAtEditor)
        {
            return true;
        }
        return isExpoMode;
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
    private int selectedVersionIndex;
    private bool versionIndexInited;
    private bool applyThemeTogether = true;



    string GetHotfixUrl()  {
        string appType = ApplicationSettings.Instance.isRelease ? "release" : "debug";
        string buildTarget = ApplicationSettings.Instance.isMachine
            ? "machine"
            : EditorUserBuildSettings.activeBuildTarget.ToString().ToLower();
        string[] vers = ApplicationSettings.Instance.appVersion.Split('.');
        string rootFolder = string.Join("_", vers); // 例：1.2.0 → 1_2_0
        return $"./{appType}/{buildTarget}/{rootFolder}";
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
        GUILayout.Label("同步 Player Settings", EditorStyles.boldLabel);
        GUI.contentColor = originalColor;

        if (GUILayout.Button("确定"))
        {
            ApplicationSettingsPlayerSync.TrySync(logAlways: true);
        }

        EditorGUILayout.EndVertical();


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
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        originalColor = GUI.contentColor;
        GUI.contentColor = Color.green;
        GUILayout.Label("版本选择", EditorStyles.boldLabel);
        GUI.contentColor = originalColor;

        DrawVersionSelectGui();

        EditorGUILayout.EndVertical();


        // ===============================================================
        // 开始一个垂直布局组
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        originalColor = GUI.contentColor;
        GUI.contentColor = Color.green;
        GUILayout.Label("同步app版本", EditorStyles.boldLabel);
        GUI.contentColor = originalColor;

        EditorGUILayout.HelpBox("始终应用 total_version.json 的第一条（最新）。", MessageType.None);

        // 创建一个按钮
        if (GUILayout.Button("确定"))
        {
            GetVersion();
        }
        // 结束垂直布局组
        EditorGUILayout.EndVertical();

    }

    void DrawVersionSelectGui()
    {
        if (!TryGetTotalVersionArray(out _, out JArray lst, showError: false))
        {
            EditorGUILayout.HelpBox($"无法读取 {PathHelper.totalVersionSAPTH}", MessageType.Warning);
            if (GUILayout.Button("刷新"))
                versionIndexInited = false;
            return;
        }

        string[] labels = new string[lst.Count];
        for (int i = 0; i < lst.Count; i++)
            labels[i] = FormatVersionLabel(lst[i] as JObject, i);

        if (!versionIndexInited)
        {
            selectedVersionIndex = FindVersionIndexByAppKey(lst, ApplicationSettings.Instance.appKey);
            versionIndexInited = true;
        }

        selectedVersionIndex = Mathf.Clamp(selectedVersionIndex, 0, lst.Count - 1);
        selectedVersionIndex = EditorGUILayout.Popup("选择版本", selectedVersionIndex, labels);
        applyThemeTogether = EditorGUILayout.Toggle("同时切主题", applyThemeTogether);
        EditorGUILayout.HelpBox(
            applyThemeTogether
                ? "应用后回填 appKey / appVersion / agentName，并按条目同步 platformName、gameTheme、launchFguiName。不修改 total_version.json。"
                : "应用后只回填 appKey / appVersion / agentName / isMachine / isRelease。不修改 total_version.json。",
            MessageType.None);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("刷新列表"))
            versionIndexInited = false;
        if (GUILayout.Button("应用选中版本"))
            ApplySelectedVersion();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox($"热更账本: {HotfixVersionLedger.DescribeCurrent()}", MessageType.None);
    }




    public void CreatVersion(bool isChangeAot = false)
    {
        HotfixVersionLedger.OnBeforeSwitchSettings();

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

        ApplicationSettingsPlayerSync.TrySync();
        HotfixVersionLedger.OnAfterSwitchSettings();
        versionIndexInited = false;
        AssetDatabase.Refresh();
    }



    public void GobackVersion()
    {
        if (!EditorUtility.DisplayDialog(
                "回滚app版本",
                "将删除 total_version.json 中的最新一条，且不可从 Inspector 撤销。确定回滚？",
                "确定",
                "取消"))
        {
            return;
        }

        if (!TryGetTotalVersionArray(out JObject totalVersionSAFile, out JArray lst, showError: true))
            return;

        if (lst.Count < 2)
        {
            EditorUtility.DisplayDialog("回滚app版本", "至少需要两条版本记录才能回滚。", "确定");
            return;
        }

        lst.RemoveAt(0);//回滚

        ApplyVersionItem(lst[0] as JObject, applyTheme: false);

        string content = totalVersionSAFile.ToString();
        File.WriteAllText(PathHelper.totalVersionSAPTH, content);

        versionIndexInited = false;
        AssetDatabase.Refresh();
    }



    public void GetVersion()
    {
        if (!TryGetTotalVersionArray(out _, out JArray lst, showError: true))
            return;

        ApplyVersionItem(lst[0] as JObject, applyTheme: false);
        versionIndexInited = false;
        AssetDatabase.Refresh();
    }

    void ApplySelectedVersion()
    {
        if (!TryGetTotalVersionArray(out _, out JArray lst, showError: true))
            return;

        int index = Mathf.Clamp(selectedVersionIndex, 0, lst.Count - 1);
        ApplyVersionItem(lst[index] as JObject, applyThemeTogether);
        ApplicationSettingsPlayerSync.TrySync();
        AssetDatabase.Refresh();
        Debug.Log($"[ApplicationSettings] 已应用选中版本: {FormatVersionLabel(lst[index] as JObject, index)}");
    }

    static bool TryGetTotalVersionArray(out JObject file, out JArray lst, bool showError)
    {
        file = null;
        lst = null;
        string path = PathHelper.totalVersionSAPTH;
        if (!File.Exists(path))
        {
            if (showError)
                EditorUtility.DisplayDialog("版本选择", $"找不到 {path}", "确定");
            return false;
        }

        try
        {
            file = JObject.Parse(File.ReadAllText(path));
            lst = file["data"] as JArray;
            if (lst == null || lst.Count == 0)
            {
                if (showError)
                    EditorUtility.DisplayDialog("版本选择", "total_version.json 没有 data 条目", "确定");
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            if (showError)
                EditorUtility.DisplayDialog("版本选择", $"读取 total_version.json 失败: {ex.Message}", "确定");
            return false;
        }
    }

    static int FindVersionIndexByAppKey(JArray lst, string appKey)
    {
        if (string.IsNullOrEmpty(appKey) || lst == null)
            return 0;

        for (int i = 0; i < lst.Count; i++)
        {
            JObject item = lst[i] as JObject;
            if (item == null)
                continue;
            if (item["app_key"]?.ToObject<string>() == appKey)
                return i;
        }
        return 0;
    }

    static string FormatVersionLabel(JObject item, int index)
    {
        if (item == null)
            return $"[{index}] (invalid)";

        string agent = item["agent_name"]?.ToObject<string>();
        if (string.IsNullOrEmpty(agent))
            agent = "-";
        string ver = item["app_version"]?.ToObject<string>();
        if (string.IsNullOrEmpty(ver))
            ver = "-";
        string key = item["app_key"]?.ToObject<string>() ?? "";
        string[] parts = key.Split('_');
        string type = parts.Length >= 3 ? $"{parts[1]}/{parts[2]}" : "-";
        return $"[{index}] {agent}  {ver}  ({type})";
    }

    static void ApplyVersionItem(JObject target, bool applyTheme)
    {
        if (target == null)
        {
            EditorUtility.DisplayDialog("版本选择", "选中的版本条目无效", "确定");
            return;
        }

        var settings = ApplicationSettings.Instance;
        Undo.RecordObject(settings, "Apply App Version");

        HotfixVersionLedger.OnBeforeSwitchSettings();

        string appVersion = target["app_version"]?.ToObject<string>() ?? "";
        string appKey = target["app_key"]?.ToObject<string>() ?? "";
        string agentName = target["agent_name"]?.ToObject<string>() ?? "";
        string[] appKeyInfos = appKey.Split('_');

        settings.appKey = appKey;
        if (appKeyInfos.Length >= 3)
        {
            settings.isMachine = appKeyInfos[2] == "machine";
            settings.isRelease = appKeyInfos[1] == "release";
        }
        if (!string.IsNullOrEmpty(agentName))
            settings.agentName = agentName;
        settings.appVersion = appVersion;

        if (applyTheme)
            ApplyThemeFromVersion(settings, agentName, appKeyInfos);

        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
        HotfixVersionLedger.OnAfterSwitchSettings();
    }

    static void ApplyThemeFromVersion(ApplicationSettings settings, string agentName, string[] appKeyInfos)
    {
        string theme = agentName;
        if (string.IsNullOrWhiteSpace(theme) && appKeyInfos != null && appKeyInfos.Length > 0)
            theme = appKeyInfos[0];
        if (string.IsNullOrWhiteSpace(theme))
            return;

        theme = theme.Trim();
        settings.platformName = theme;
        settings.gameTheme = theme;
        settings.agentName = theme;

        string launch = ResolveLaunchFguiNameForTheme(theme);
        if (!string.IsNullOrEmpty(launch))
            settings.launchFguiName = launch;
    }

    /// <summary>
    /// AOT 编辑器不能引用热更 ThemeProfile，映射与 PageLaunch.ResolveLaunchFguiName 保持一致。
    /// </summary>
    static string ResolveLaunchFguiNameForTheme(string theme)
    {
        if (string.Equals(theme, "Savage", StringComparison.OrdinalIgnoreCase))
            return "SavagePageLaunch";
        if (string.Equals(theme, "Test", StringComparison.OrdinalIgnoreCase))
            return "TreasuryPageLaunch";
        if (string.Equals(theme, "Treasury", StringComparison.OrdinalIgnoreCase))
            return "TreasuryPageLaunch";
        return null;
    }

}
#endif