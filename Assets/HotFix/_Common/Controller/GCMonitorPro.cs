using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;


#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

public class GCMonitorPro : MonoBehaviour
{
    private static GCMonitorPro instance;

    public static GCMonitorPro Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("[GCMonitorPro]");
                instance = go.AddComponent<GCMonitorPro>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    public bool showOnScreen = true;

    // 移除收起功能，UI始終展開
    public bool isUIScaled = false; // 控制UI是否放大2倍

    private Vector2 scrollPosition = Vector2.zero; // 滚动视图位置
    private float deltaTime;
    private float fps;
    private float fpsTimer;
    private float fpsUpdateInterval = 0.5f;
    private int lastGCCount;
    private float lastGCTime;
    private Stopwatch stopwatch = new Stopwatch();
    private float nextUpdateTime;
    private float updateInterval = 0.5f;

    // 内存监控
    private long lastMemory;

    private float memoryWarningThreshold = 800f;

    // 物理内存监控 - 平台优化版本
    private Process currentProcess;

    private long lastPhysicalMemory;
    private float physicalMemoryMB;
    private int systemTotalMemoryMB;
    private float physicalMemoryUsagePercent; // Unity进程占系统总内存的百分比
    private float systemMemoryUsagePercent; // 系统整体内存使用率（所有进程）
    private float systemUsedMemoryMB; // 系统整体已使用内存（MB）

    // Android 进程内存信息（通过 ActivityManager 获取）
    private float androidRuntimeTotalMemoryMB; // 进程物理内存占用（MB）- 使用ActivityManager.getProcessMemoryInfo().getTotalPss()

    // 公共属性：用于外部访问内存数据
    public int SystemTotalMemoryMB => systemTotalMemoryMB;

    public float SystemUsedMemoryMB => systemUsedMemoryMB;
    public float SystemMemoryUsagePercent => systemMemoryUsagePercent;
    public float PhysicalMemoryMB => physicalMemoryMB;
    public float PhysicalMemoryUsagePercent => physicalMemoryUsagePercent;

    // Android 进程内存信息公共属性
    public float AndroidRuntimeTotalMemoryMB => androidRuntimeTotalMemoryMB;

    // 详细内存统计
    private float privateMemoryMB;

    private float virtualMemoryMB;
    private float peakWorkingSetMB;
    private float peakVirtualMemoryMB;
    private float pagedMemoryMB;
    private float nonPagedMemoryMB;

    // 平台特定内存信息
    private bool isAndroidPlatform;

    private bool isPCPlatform;
    private string platformMemoryInfo;

    // YooAsset 资源监控
    private int yooAssetLoadedCount;

    private float yooAssetMemoryUsage;

    // 泄漏检测 
    private const int trendSampleCount = 10;

    private float[] memorySamples = new float[trendSampleCount];
    private int sampleIndex = 0;
    private bool leakSuspected = false;
    // 曲线图数据 

    private const int maxGraphPoints = 120; // 最近120次采样（约60秒） 
    private float[] memoryHistory = new float[maxGraphPoints];
    private int historyIndex = 0;

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this; DontDestroyOnLoad(gameObject); stopwatch.Start();
        lastMemory = GC.GetTotalMemory(false); lastGCCount = GC.CollectionCount(0);

        // 检测平台类型
        DetectPlatform();

        // 初始化物理内存监控
        InitializePhysicalMemoryMonitoring();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            showOnScreen = !showOnScreen;
        }

        if (!showOnScreen)
        {
            return;
        }

        // FPS计算 - 使用Time.smoothDeltaTime来平滑计算，它受Application.targetFrameRate影响
        deltaTime += (Time.smoothDeltaTime - deltaTime) * 0.1f;
        fpsTimer += Time.smoothDeltaTime;
        if (fpsTimer >= fpsUpdateInterval)
        {
            fps = 1.0f / deltaTime;
            fpsTimer = 0f;
        }

        if (Time.time >= nextUpdateTime)
        {
            nextUpdateTime = Time.time + updateInterval;
            UpdateMemoryStatus();
            UpdatePhysicalMemoryStatus();
            UpdateYooAssetStatus();

        }
    }

    private void UpdateMemoryStatus()
    {
        long currentMemory = GC.GetTotalMemory(false); int currentGCCount = GC.CollectionCount(0);
        // GC触发检测     
        if (currentGCCount > lastGCCount)
        {
            float gcInterval = stopwatch.ElapsedMilliseconds / 1000f - lastGCTime;
            lastGCTime = stopwatch.ElapsedMilliseconds / 1000f;
            //  UnityEngine.Debug.Log($"<color=orange>[GC]</color> GC触发 | 间隔: {gcInterval:F2}s | 总次数: {currentGCCount}");
            lastGCCount = currentGCCount;
        }
        // 泄漏趋势检测 
        float memoryMB = currentMemory / (1024f * 1024f);
        memorySamples[sampleIndex] = memoryMB;
        sampleIndex = (sampleIndex + 1) % trendSampleCount;
        if (sampleIndex == 0)
        {
            leakSuspected = CheckLeakTrend();
            //if (leakSuspected)
            //    UnityEngine.Debug.LogError("<color=#FF5555>[GCMonitorPro]</color> ⚠ 检测到内存持续上升，可能存在泄漏！");
        }
        // 添加到历史数据   
        memoryHistory[historyIndex] = memoryMB;
        historyIndex = (historyIndex + 1) % maxGraphPoints;
        lastMemory = currentMemory;
    }

    private bool CheckLeakTrend()
    {
        float first = memorySamples[0];
        float last = memorySamples[trendSampleCount - 1];
        if (last - first > 50f)
        {
            int upCount = 0;
            for (int i = 1; i < trendSampleCount; i++)
                if (memorySamples[i] > memorySamples[i - 1])
                    upCount++;
            return
                upCount > trendSampleCount * 0.7f;
        }
        return false;
    }

    /// <summary>
    /// 检测当前运行平台
    /// </summary>
    private void DetectPlatform()
    {
        isAndroidPlatform = Application.platform == RuntimePlatform.Android;
        isPCPlatform = Application.platform == RuntimePlatform.WindowsPlayer ||
                      Application.platform == RuntimePlatform.WindowsEditor ||
                      Application.platform == RuntimePlatform.OSXPlayer ||
                      Application.platform == RuntimePlatform.OSXEditor ||
                      Application.platform == RuntimePlatform.LinuxPlayer ||
                      Application.platform == RuntimePlatform.LinuxEditor;

        platformMemoryInfo = $"Platform: {Application.platform} | Android: {isAndroidPlatform} | PC: {isPCPlatform}";
        //  UnityEngine.Debug.Log($"<color=cyan>[GCMonitorPro]</color> {platformMemoryInfo}");
    }

    /// <summary>
    /// 初始化物理内存监控
    /// </summary>
    private void InitializePhysicalMemoryMonitoring()
    {
        try
        {
            if (isPCPlatform)
            {
                InitializePCMemoryMonitoring();
            }
            else if (isAndroidPlatform)
            {
                InitializeAndroidMemoryMonitoring();
            }
            else
            {
                InitializeFallbackMemoryMonitoring();
            }
        }
        catch (Exception e)
        {
            // UnityEngine.Debug.LogError($"<color=red>[GCMonitorPro]</color> 物理内存监控初始化失败: {e.Message}");
            InitializeFallbackMemoryMonitoring();
        }
    }

    /// <summary>
    /// PC平台内存监控初始化（物理内存实际占用）
    /// </summary>
    private void InitializePCMemoryMonitoring()
    {
        // 获取系统总内存 - PC平台使用更准确的方法
        systemTotalMemoryMB = GetSystemTotalMemoryPC();

#if UNITY_EDITOR
        // Unity编辑器环境下优先使用Profiler API
        try
        {
            // 使用Unity Profiler API获取内存信息（编辑器环境下更可靠）
            long totalAllocatedMemory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
            long totalReservedMemory = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong();

            // 使用总预留内存作为物理内存占用（更接近实际物理内存）
            lastPhysicalMemory = totalReservedMemory;
            physicalMemoryMB = totalReservedMemory / (1024f * 1024f);

            // 尝试使用Process API获取更准确的值（如果可用）
            try
            {
                currentProcess = Process.GetCurrentProcess();
                currentProcess.Refresh();
                if (currentProcess.WorkingSet64 > 0)
                {
                    lastPhysicalMemory = currentProcess.WorkingSet64;
                    physicalMemoryMB = lastPhysicalMemory / (1024f * 1024f);
                }
            }
            catch
            {
                // Process API失败，继续使用Profiler API的值
                currentProcess = null;
            }
        }
        catch (Exception e)
        {
            //  UnityEngine.Debug.LogWarning($"<color=orange>[GCMonitorPro]</color> 编辑器环境下初始化失败: {e.Message}，使用备用方案");
            // 使用备用方案
            long managedMemory = GC.GetTotalMemory(false);
            lastPhysicalMemory = managedMemory;
            physicalMemoryMB = managedMemory / (1024f * 1024f);
        }
#else
        // 非编辑器环境使用Process API
        currentProcess = Process.GetCurrentProcess();

        // 获取详细内存统计
        UpdateDetailedMemoryStats();

        // WorkingSet64是进程实际占用的物理内存（运行内存）
        currentProcess.Refresh();
        lastPhysicalMemory = currentProcess.WorkingSet64;
        physicalMemoryMB = lastPhysicalMemory / (1024f * 1024f);
#endif

        // 获取详细内存统计
        UpdateDetailedMemoryStats();

        // 计算物理内存使用率（Unity进程占系统总内存的百分比）
        if (systemTotalMemoryMB > 0)
        {
            physicalMemoryUsagePercent = (physicalMemoryMB / systemTotalMemoryMB) * 100f;
        }
        else
        {
            physicalMemoryUsagePercent = 0f;
        }

        // 计算系统整体内存使用率（所有进程的总和）
        systemUsedMemoryMB = GetSystemUsedMemoryPC();
        if (systemUsedMemoryMB > 0 && systemTotalMemoryMB > 0)
        {
            systemMemoryUsagePercent = (systemUsedMemoryMB / systemTotalMemoryMB) * 100f;
        }
        else
        {
            // 如果无法获取系统整体内存使用，保持为0，不要使用进程内存
            systemMemoryUsagePercent = 0f;
            systemUsedMemoryMB = 0f;
        }

        //UnityEngine.Debug.Log($"<color=cyan>[GCMonitorPro]</color> PC平台物理内存监控初始化完成");
        //UnityEngine.Debug.Log($"<color=cyan>[GCMonitorPro]</color> 系统总内存: {systemTotalMemoryMB} MB | 进程物理内存: {physicalMemoryMB:F1} MB | 进程使用率: {physicalMemoryUsagePercent:F1}%");
        //UnityEngine.Debug.Log($"<color=cyan>[GCMonitorPro]</color> 系统已用: {systemUsedMemoryMB:F1} MB | 系统使用率: {systemMemoryUsagePercent:F1}%");
        //UnityEngine.Debug.Log($"<color=cyan>[GCMonitorPro]</color> 私有内存: {privateMemoryMB} MB | 虚拟内存: {virtualMemoryMB} MB");
        //UnityEngine.Debug.Log($"<color=cyan>[GCMonitorPro]</color> 峰值工作集: {peakWorkingSetMB} MB | 峰值虚拟内存: {peakVirtualMemoryMB} MB");
    }

    /// <summary>
    /// Android平台内存监控初始化（物理内存实际占用）
    /// </summary>
    private void InitializeAndroidMemoryMonitoring()
    {
        // Android平台使用Unity的SystemInfo获取系统总内存
        systemTotalMemoryMB = SystemInfo.systemMemorySize;

        // Android平台备用方案
        if (systemTotalMemoryMB <= 0)
        {
            // 根据设备类型估算内存
            systemTotalMemoryMB = EstimateAndroidMemory();
        }

        // 使用JNI调用获取真实的物理内存占用
        physicalMemoryMB = GetAndroidProcessMemory();

        // 获取Android Runtime内存信息（总内存、可用内存、最大可用内存）
        GetAndroidRuntimeMemoryInfo();

        // 计算物理内存使用率
        if (systemTotalMemoryMB > 0)
        {
            physicalMemoryUsagePercent = (physicalMemoryMB / systemTotalMemoryMB) * 100f;
        }
        else
        {
            physicalMemoryUsagePercent = 0f;
        }

        // 获取系统整体已使用内存（运行内存）
        systemUsedMemoryMB = GetSystemUsedMemoryAndroid();
        if (systemUsedMemoryMB > 0 && systemTotalMemoryMB > 0)
        {
            systemMemoryUsagePercent = (systemUsedMemoryMB / systemTotalMemoryMB) * 100f;
        }
        else
        {
            systemMemoryUsagePercent = 0f;
            systemUsedMemoryMB = 0f;
        }

        // 初始化lastPhysicalMemory
        lastPhysicalMemory = (long)(physicalMemoryMB * 1024 * 1024);

        //UnityEngine.Debug.Log($"<color=cyan>[GCMonitorPro]</color> Android平台物理内存监控初始化完成");
        //UnityEngine.Debug.Log($"<color=cyan>[GCMonitorPro]</color> 系统总内存: {systemTotalMemoryMB} MB | 进程物理内存: {physicalMemoryMB:F1} MB | 使用率: {physicalMemoryUsagePercent:F1}%");
        //UnityEngine.Debug.Log($"<color=cyan>[GCMonitorPro]</color> 设备型号: {SystemInfo.deviceModel} | 设备名称: {SystemInfo.deviceName}");
    }

    /// <summary>
    /// 备用内存监控初始化
    /// </summary>
    private void InitializeFallbackMemoryMonitoring()
    {
        systemTotalMemoryMB = SystemInfo.systemMemorySize;

        if (systemTotalMemoryMB <= 0)
        {
            systemTotalMemoryMB = 4096; // 默认4GB
        }

        // 使用托管内存作为近似值
        long managedMemory = GC.GetTotalMemory(false);
        physicalMemoryMB = managedMemory / (1024f * 1024f);
        physicalMemoryUsagePercent = (physicalMemoryMB / systemTotalMemoryMB) * 100f;

        //UnityEngine.Debug.Log($"<color=cyan>[GCMonitorPro]</color> 备用内存监控初始化完成");
        //UnityEngine.Debug.Log($"<color=cyan>[GCMonitorPro]</color> 系统总内存: {systemTotalMemoryMB} MB | 托管内存: {physicalMemoryMB:F1} MB | 使用率: {physicalMemoryUsagePercent:F1}%");
    }

    /// <summary>
    /// 获取PC平台系统总内存
    /// </summary>
    private int GetSystemTotalMemoryPC()
    {
        try
        {
            // 优先使用SystemInfo
            int systemMemory = SystemInfo.systemMemorySize;
            if (systemMemory > 0)
            {
                return systemMemory;
            }

            // 备用方案：使用Process获取系统信息
            if (currentProcess != null)
            {
                // 这是一个近似值，实际应该使用WMI或其他系统API
                return 8192; // 默认8GB
            }

            return 8192; // 默认值
        }
        catch
        {
            return 8192; // 默认值
        }
    }

    /// <summary>
    /// 获取PC平台系统整体已使用内存（所有进程的总和）
    /// </summary>
    private float GetSystemUsedMemoryPC()
    {
        try
        {
            // 方法1：尝试使用PerformanceCounter（如果可用）
            try
            {
                using (var pc = new PerformanceCounter("Memory", "Available MBytes"))
                {
                    // PerformanceCounter需要先调用一次NextValue()来初始化
                    pc.NextValue();
                    // 等待一小段时间让计数器稳定（在Update中会定期调用，这里不需要等待）
                    // 第二次调用获取实际值
                    float availableMB = pc.NextValue();
                    if (availableMB > 0 && systemTotalMemoryMB > 0)
                    {
                        float usedMB = systemTotalMemoryMB - availableMB;
                        if (usedMB > 0 && usedMB <= systemTotalMemoryMB * 1.1f) // 允许10%的误差
                        {
                            return usedMB;
                        }
                    }
                }
            }
            catch
            {
                // PerformanceCounter不可用，尝试其他方法
            }

            // 方法2：尝试使用WMI查询（如果可用）- 使用反射避免编译时依赖
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            try
            {
                // 使用反射动态加载 System.Management，避免编译时类型检查
                System.Reflection.Assembly managementAssembly = System.Reflection.Assembly.Load("System.Management");
                if (managementAssembly != null)
                {
                    Type searcherType = managementAssembly.GetType("System.Management.ManagementObjectSearcher");
                    Type objectType = managementAssembly.GetType("System.Management.ManagementObject");

                    if (searcherType != null && objectType != null)
                    {
                        object searcher = Activator.CreateInstance(searcherType, "SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");
                        object collection = searcherType.GetMethod("Get").Invoke(searcher, null);

                        // 使用反射遍历集合
                        System.Collections.IEnumerable enumerable = collection as System.Collections.IEnumerable;
                        if (enumerable != null)
                        {
                            foreach (object obj in enumerable)
                            {
                                object totalMemoryProp = objectType.GetProperty("Item", new Type[] { typeof(string) }).GetValue(obj, new object[] { "TotalVisibleMemorySize" });
                                object freeMemoryProp = objectType.GetProperty("Item", new Type[] { typeof(string) }).GetValue(obj, new object[] { "FreePhysicalMemory" });

                                ulong totalMemory = Convert.ToUInt64(totalMemoryProp);
                                ulong freeMemory = Convert.ToUInt64(freeMemoryProp);
                                ulong usedMemory = totalMemory - freeMemory;

                                // 转换为MB（WMI返回的是KB）
                                float usedMB = (float)(usedMemory / 1024.0);
                                if (usedMB > 0 && usedMB <= systemTotalMemoryMB * 1.1f) // 允许10%的误差
                                {
                                    return usedMB;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // WMI不可用，使用备用方案
            }
#endif

            // 方法3：备用方案 - 使用进程内存作为近似值（不准确，但总比没有好）
            // 实际上，我们可以尝试查询所有进程的内存使用情况
            // 但为了性能考虑，这里使用一个简化的估算
            // 如果系统总内存和Unity进程内存已知，可以粗略估算
            // 但这个方法不够准确，所以返回0表示无法获取

            return 0f;
        }
        catch
        {
            return 0f;
        }
    }

    /// <summary>
    /// 更新详细内存统计（PC平台）
    /// </summary>
    private void UpdateDetailedMemoryStats()
    {
#if UNITY_EDITOR
        // Unity编辑器环境下使用Profiler API
        try
        {
            long totalAllocatedMemory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
            long totalReservedMemory = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong();

            // 使用Profiler API的值作为近似值
            privateMemoryMB = totalAllocatedMemory / (1024f * 1024f);
            virtualMemoryMB = totalReservedMemory * 2f / (1024f * 1024f); // 虚拟内存通常是预留内存的2倍左右

            // 尝试使用Process API获取峰值信息（如果可用）
            if (currentProcess != null)
            {
                try
                {
                    currentProcess.Refresh();
                    if (currentProcess.PrivateMemorySize64 > 0)
                    {
                        privateMemoryMB = currentProcess.PrivateMemorySize64 / (1024f * 1024f);
                    }
                    if (currentProcess.VirtualMemorySize64 > 0)
                    {
                        virtualMemoryMB = currentProcess.VirtualMemorySize64 / (1024f * 1024f);
                    }
                    if (currentProcess.PeakWorkingSet64 > 0)
                    {
                        peakWorkingSetMB = currentProcess.PeakWorkingSet64 / (1024f * 1024f);
                    }
                    if (currentProcess.PeakVirtualMemorySize64 > 0)
                    {
                        peakVirtualMemoryMB = currentProcess.PeakVirtualMemorySize64 / (1024f * 1024f);
                    }
                    if (currentProcess.PagedMemorySize64 > 0)
                    {
                        pagedMemoryMB = currentProcess.PagedMemorySize64 / (1024f * 1024f);
                    }
                    if (currentProcess.NonpagedSystemMemorySize64 > 0)
                    {
                        nonPagedMemoryMB = currentProcess.NonpagedSystemMemorySize64 / (1024f * 1024f);
                    }
                }
                catch
                {
                    // Process API失败，使用Profiler API的值
                }
            }

            // 如果峰值信息仍为0，使用当前值作为峰值
            if (peakWorkingSetMB <= 0)
            {
                peakWorkingSetMB = physicalMemoryMB;
            }
            if (peakVirtualMemoryMB <= 0)
            {
                peakVirtualMemoryMB = virtualMemoryMB;
            }
        }
        catch (Exception e)
        {
            //  UnityEngine.Debug.LogWarning($"<color=orange>[GCMonitorPro]</color> 编辑器环境下更新详细内存统计失败: {e.Message}");
        }
#else
        // 非编辑器环境使用Process API
        if (currentProcess == null) return;

        try
        {
            // 使用浮点数除法确保精度
            privateMemoryMB = currentProcess.PrivateMemorySize64 / (1024f * 1024f);
            virtualMemoryMB = currentProcess.VirtualMemorySize64 / (1024f * 1024f);
            peakWorkingSetMB = currentProcess.PeakWorkingSet64 / (1024f * 1024f);
            peakVirtualMemoryMB = currentProcess.PeakVirtualMemorySize64 / (1024f * 1024f);
            pagedMemoryMB = currentProcess.PagedMemorySize64 / (1024f * 1024f);
            nonPagedMemoryMB = currentProcess.NonpagedSystemMemorySize64 / (1024f * 1024f);

            //// 添加调试信息
            //UnityEngine.Debug.Log($"<color=cyan>[GCMonitorPro]</color> 详细内存统计更新:");
            //UnityEngine.Debug.Log($"<color=cyan>[GCMonitorPro]</color> 私有内存: {privateMemoryMB:F1} MB | 虚拟内存: {virtualMemoryMB:F1} MB");
            //UnityEngine.Debug.Log($"<color=cyan>[GCMonitorPro]</color> 峰值工作集: {peakWorkingSetMB:F1} MB | 峰值虚拟: {peakVirtualMemoryMB:F1} MB");
        }
        catch (Exception e)
        {
           // UnityEngine.Debug.LogWarning($"<color=orange>[GCMonitorPro]</color> 更新详细内存统计失败: {e.Message}");
        }
#endif
    }

    /// <summary>
    /// 估算Android设备内存
    /// </summary>
    private int EstimateAndroidMemory()
    {
        string deviceModel = SystemInfo.deviceModel.ToLower();

        // 根据设备型号估算内存
        if (deviceModel.Contains("samsung"))
        {
            if (deviceModel.Contains("galaxy s") || deviceModel.Contains("galaxy note"))
                return 6144; // 6GB
            return 4096; // 4GB
        }
        else if (deviceModel.Contains("huawei") || deviceModel.Contains("honor"))
        {
            return 4096; // 4GB
        }
        else if (deviceModel.Contains("xiaomi") || deviceModel.Contains("redmi"))
        {
            return 4096; // 4GB
        }
        else if (deviceModel.Contains("oppo") || deviceModel.Contains("vivo"))
        {
            return 4096; // 4GB
        }

        // 默认值
        return 3072; // 3GB
    }

    /// <summary>
    /// 获取Android进程物理内存使用量（真实运行内存占用，单位：MB）
    /// 返回的是应用实际占用的物理RAM（运行内存），不是虚拟内存
    /// </summary>
    private float GetAndroidProcessMemory()
    {
        try
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // 方法1：使用ActivityManager.getProcessMemoryInfo().getTotalPss()获取物理内存占用
            // totalPss = Proportional Set Size，是实际占用的物理RAM（包括共享内存按比例分配）
            // 这是最准确的物理内存占用指标，单位：KB
            try
            {
                using (AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    AndroidJavaObject currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
                    if (currentActivity != null)
                    {
                        // 获取ActivityManager
                        using (AndroidJavaClass contextClass = new AndroidJavaClass("android.content.Context"))
                        {
                            string activityService = contextClass.GetStatic<string>("ACTIVITY_SERVICE");
                            using (AndroidJavaObject activityManager = currentActivity.Call<AndroidJavaObject>("getSystemService", activityService))
                            {
                                if (activityManager != null)
                                {
                                    // 获取当前进程ID
                                    int pid = Process.GetCurrentProcess().Id;
                                    int[] pids = new int[] { pid };

                                    // 获取进程内存信息
                                    AndroidJavaObject[] memoryInfos = activityManager.Call<AndroidJavaObject[]>("getProcessMemoryInfo", pids);
                                    if (memoryInfos != null && memoryInfos.Length > 0)
                                    {
                                        AndroidJavaObject memoryInfo = memoryInfos[0];
                                        // getTotalPss()返回实际占用的物理内存（运行内存），单位：KB
                                        int totalPss = memoryInfo.Call<int>("getTotalPss");
                                        if (totalPss > 0)
                                        {
                                            // 转换为MB（totalPss单位是KB）
                                            float memoryMB = totalPss / 1024f;
                                            return memoryMB;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e1)
            {
                UnityEngine.Debug.LogWarning($"<color=orange>[GCMonitorPro]</color> 方法1（getTotalPss）失败: {e1.Message}，尝试方法2");
            }

            // 方法2：通过读取/proc/self/status文件获取VmRSS（物理内存占用）
            // VmRSS = Virtual Memory Resident Set Size，表示进程实际驻留在物理RAM中的页面大小，单位：KB
            // 这也是准确的物理内存占用指标
            try
            {
                float memoryMB = /*GetAndroidMemoryFromProcStatus()*/0;
                if (memoryMB > 0)
                {
                    return memoryMB;
                }
            }
            catch (Exception e2)
            {
                UnityEngine.Debug.LogWarning($"<color=orange>[GCMonitorPro]</color> 方法2（VmRSS）失败: {e2.Message}，尝试方法3");
            }

            // 方法3：使用Debug.getNativeHeapAllocatedSize获取Native堆内存（估算值，不够准确）
            // 注意：这只是Native堆内存的估算，不是完整的物理内存占用，仅作为最后的备用方案
            try
            {
                using (AndroidJavaClass debugClass = new AndroidJavaClass("android.os.Debug"))
                {
                    long nativeHeapAllocated = debugClass.CallStatic<long>("getNativeHeapAllocatedSize");
                    // 使用Native堆分配大小作为参考（单位是字节）
                    if (nativeHeapAllocated > 0)
                    {
                        // Native内存 + 托管内存的估算（注意：这不是完整的物理内存占用）
                        long managedMemory = GC.GetTotalMemory(false);
                        float totalMemoryMB = (nativeHeapAllocated + managedMemory) / (1024f * 1024f);
                        return totalMemoryMB;
                    }
                }
            }
            catch (Exception e3)
            {
                UnityEngine.Debug.LogWarning($"<color=orange>[GCMonitorPro]</color> 方法3（Native堆估算）失败: {e3.Message}，使用备用方案");
            }

            // 所有方法都失败，使用备用方案
            return GetAndroidProcessMemoryFallback();
#else
            // 编辑器或非Android平台使用备用方案
            return GetAndroidProcessMemoryFallback();
#endif
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning($"<color=orange>[GCMonitorPro]</color> Android内存获取失败: {e.Message}，使用备用方案");
            return GetAndroidProcessMemoryFallback();
        }
    }



    [System.Serializable]
    public struct MemoryData
    {
        public long totalSystemMemory;      // 系统总内存(字节)
        public long availableSystemMemory;  // 系统可用内存(字节)
        public long lowMemoryThreshold;     // 低内存阈值(字节)
        public int processTotalMemory;      // 进程总内存(KB)
        public int processPrivateMemory;    // 进程私有内存(KB)
    }

    public MemoryData currentMemory;



    public static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB" };
        int counter = 0;
        decimal number = bytes;

        while (Math.Round(number / 1024) >= 1 && counter < suffixes.Length - 1)
        {
            number /= 1024;
            counter++;
        }

        return $"{number:F2} {suffixes[counter]}";
    }

    /// <summary>
    /// Android平台备用内存获取方案
    /// </summary>
    private float GetAndroidProcessMemoryFallback()
    {
        try
        {
            // 使用Unity的Profiler获取总分配内存（近似值）
            long totalAllocatedMemory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
            long totalReservedMemory = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong();

            // 使用总预留内存作为近似值（更接近物理内存占用）
            float memoryMB = totalReservedMemory / (1024f * 1024f);

            // 如果获取失败，使用托管内存作为最后备选
            if (memoryMB <= 0)
            {
                long managedMemory = GC.GetTotalMemory(false);
                memoryMB = managedMemory / (1024f * 1024f);
            }

            return memoryMB;
        }
        catch
        {
            // 最后的备选方案
            long managedMemory = GC.GetTotalMemory(false);
            return managedMemory / (1024f * 1024f);
        }
    }

    /// <summary>
    /// 获取Android进程物理内存占用
    /// 使用ActivityManager.getProcessMemoryInfo().getTotalPss()获取进程实际占用的物理RAM
    /// totalPss = Proportional Set Size，是实际占用的物理内存（包括共享内存按比例分配）
    /// </summary>
    private void GetAndroidRuntimeMemoryInfo()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
                if (currentActivity != null)
                {
                    // 获取ActivityManager
                    using (AndroidJavaClass contextClass = new AndroidJavaClass("android.content.Context"))
                    {
                        string activityService = contextClass.GetStatic<string>("ACTIVITY_SERVICE");
                        using (AndroidJavaObject activityManager = currentActivity.Call<AndroidJavaObject>("getSystemService", activityService))
                        {
                            if (activityManager != null)
                            {
                                // 获取当前进程ID
                                int pid = Process.GetCurrentProcess().Id;
                                int[] pids = new int[] { pid };

                                // 获取进程内存信息
                                AndroidJavaObject[] memoryInfos = activityManager.Call<AndroidJavaObject[]>("getProcessMemoryInfo", pids);
                                if (memoryInfos != null && memoryInfos.Length > 0)
                                {
                                    AndroidJavaObject memoryInfo = memoryInfos[0];
                                    // getTotalPss()返回实际占用的物理内存（运行内存），单位：KB
                                    int totalPss = memoryInfo.Call<int>("getTotalPss");
                                    if (totalPss > 0)
                                    {
                                        // 转换为MB（totalPss单位是KB）
                                        androidRuntimeTotalMemoryMB = totalPss / 1024f;
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // 如果获取失败，设置为0
            androidRuntimeTotalMemoryMB = 0f;
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning($"<color=orange>[GCMonitorPro]</color> 获取Android进程内存信息失败: {e.Message}");
            // 失败时保持默认值0
            androidRuntimeTotalMemoryMB = 0f;
        }
#else
        // 非Android平台或编辑器模式，设置为0
        androidRuntimeTotalMemoryMB = 0f;
#endif
    }

    /// <summary>
    /// 获取Android平台系统整体已使用内存（运行内存）
    /// 使用ActivityManager.getMemoryInfo()获取系统可用内存，然后计算已用内存
    ///
    /// 注意：不能使用 java.lang.Runtime 来获取系统内存，原因：
    /// 1. Runtime 只能获取当前应用进程的 JVM 内存，无法获取系统整体内存
    /// 2. ActivityManager.getMemoryInfo() 可以获取系统级别的内存信息（所有应用的内存总和）
    /// 3. ActivityManager 需要通过 Context.getSystemService() 获取
    /// 4. Context 需要通过 Activity 获取，而 Activity 需要通过 UnityPlayer.currentActivity 获取
    /// </summary>
    private float GetSystemUsedMemoryAndroid()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            // 必须通过 UnityPlayer 获取 Activity，因为：
            // 1. ActivityManager 需要 Context，而 Context 需要通过 Activity 获取
            // 2. Runtime 无法提供系统级别的内存信息，只能提供应用进程的内存信息
            using (AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
                if (currentActivity != null)
                {
                    // 获取ActivityManager（需要通过 Context.getSystemService() 获取）
                    using (AndroidJavaClass contextClass = new AndroidJavaClass("android.content.Context"))
                    {
                        string activityService = contextClass.GetStatic<string>("ACTIVITY_SERVICE");
                        using (AndroidJavaObject activityManager = currentActivity.Call<AndroidJavaObject>("getSystemService", activityService))
                        {
                            if (activityManager != null)
                            {
                                // 创建MemoryInfo对象
                                using (AndroidJavaObject memoryInfo = new AndroidJavaObject("android.app.ActivityManager$MemoryInfo"))
                                {
                                    // 获取系统内存信息
                                    activityManager.Call("getMemoryInfo", memoryInfo);

                                    // 获取系统可用内存（单位：字节）
                                    long availMem = memoryInfo.Get<long>("availMem");

                                    // 尝试获取系统总内存（API 16+）
                                    long totalMem = 0;
                                    try
                                    {
                                        totalMem = memoryInfo.Get<long>("totalMem");
                                    }
                                    catch
                                    {
                                        // 如果API不支持totalMem，使用systemTotalMemoryMB
                                        totalMem = (long)(systemTotalMemoryMB * 1024 * 1024);
                                    }

                                    // 如果totalMem为0，使用systemTotalMemoryMB
                                    if (totalMem <= 0 && systemTotalMemoryMB > 0)
                                    {
                                        totalMem = (long)(systemTotalMemoryMB * 1024 * 1024);
                                    }

                                    // 计算系统已用内存 = 系统总内存 - 系统可用内存
                                    if (totalMem > 0 && availMem >= 0 && availMem <= totalMem)
                                    {
                                        long usedMem = totalMem - availMem;
                                        float usedMB = usedMem / (1024f * 1024f);
                                        if (usedMB > 0 && usedMB <= systemTotalMemoryMB * 1.1f) // 允许10%的误差
                                        {
                                            return usedMB;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning($"<color=orange>[GCMonitorPro]</color> 获取Android系统已用内存失败: {e.Message}");
        }
#endif
        // 如果获取失败，返回0
        return 0f;
    }

    /// <summary>
    /// 更新物理内存状态
    /// </summary>
    private void UpdatePhysicalMemoryStatus()
    {
        try
        {
            if (isPCPlatform)
            {
                UpdatePCMemoryStatus();
            }
            else if (isAndroidPlatform)
            {
                UpdateAndroidMemoryStatus();
            }
            else
            {
                UpdateFallbackMemoryStatus();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"<color=red>[GCMonitorPro]</color> 物理内存状态更新失败: {e.Message}");
        }
    }

    /// <summary>
    /// 更新PC平台内存状态（物理内存实际占用）
    /// </summary>
    private void UpdatePCMemoryStatus()
    {
        long currentPhysicalMemory = 0;

        // Unity编辑器环境下优先使用Profiler API
#if UNITY_EDITOR
        try
        {
            // 使用Unity Profiler API获取内存信息（编辑器环境下更可靠）
            long totalAllocatedMemory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
            long totalReservedMemory = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong();

            // 使用总预留内存作为物理内存占用（更接近实际物理内存）
            currentPhysicalMemory = totalReservedMemory;
            physicalMemoryMB = totalReservedMemory / (1024f * 1024f);

            // 使用总分配内存作为私有内存的近似值
            if (currentProcess != null)
            {
                try
                {
                    currentProcess.Refresh();
                    // 如果Process API可用，尝试获取更准确的值
                    if (currentProcess.WorkingSet64 > 0)
                    {
                        currentPhysicalMemory = currentProcess.WorkingSet64;
                        physicalMemoryMB = currentPhysicalMemory / (1024f * 1024f);
                    }
                }
                catch
                {
                    // Process API失败，继续使用Profiler API的值
                }
            }

            // 确保系统总内存有效
            if (systemTotalMemoryMB <= 0)
            {
                systemTotalMemoryMB = GetSystemTotalMemoryPC();
            }

            // 计算物理内存使用率（Unity进程占系统总内存的百分比）
            if (systemTotalMemoryMB > 0)
            {
                physicalMemoryUsagePercent = (physicalMemoryMB / systemTotalMemoryMB) * 100f;
            }
            else
            {
                physicalMemoryUsagePercent = 0f;
            }

            // 计算系统整体内存使用率（所有进程的总和）
            systemUsedMemoryMB = GetSystemUsedMemoryPC();
            if (systemUsedMemoryMB > 0 && systemTotalMemoryMB > 0)
            {
                systemMemoryUsagePercent = (systemUsedMemoryMB / systemTotalMemoryMB) * 100f;
            }
            else
            {
                // 如果无法获取系统整体内存使用，保持为0，不要使用进程内存
                systemMemoryUsagePercent = 0f;
                systemUsedMemoryMB = 0f;
            }
        }
        catch (Exception e)
        {
            // UnityEngine.Debug.LogError($"<color=red>[GCMonitorPro]</color> 编辑器环境下物理内存状态更新失败: {e.Message}");
            // 使用备用方案
            long managedMemory = GC.GetTotalMemory(false);
            currentPhysicalMemory = managedMemory;
            physicalMemoryMB = managedMemory / (1024f * 1024f);
            if (systemTotalMemoryMB > 0)
            {
                physicalMemoryUsagePercent = (physicalMemoryMB / systemTotalMemoryMB) * 100f;
            }
            // 尝试获取系统整体内存使用率
            systemUsedMemoryMB = GetSystemUsedMemoryPC();
            if (systemUsedMemoryMB > 0 && systemTotalMemoryMB > 0)
            {
                systemMemoryUsagePercent = (systemUsedMemoryMB / systemTotalMemoryMB) * 100f;
            }
            else
            {
                // 如果无法获取系统整体内存使用，保持为0，不要使用进程内存
                systemMemoryUsagePercent = 0f;
                systemUsedMemoryMB = 0f;
            }
        }
#else
        // 非编辑器环境（构建版本）使用Process API
        if (currentProcess == null)
        {
           // UnityEngine.Debug.LogWarning($"<color=orange>[GCMonitorPro]</color> currentProcess 為 null，嘗試重新初始化");
            InitializePCMemoryMonitoring();
            return;
        }

        try
        {
            // 刷新进程信息以获取最新的内存数据
            currentProcess.Refresh();

            // WorkingSet64是进程实际占用的物理内存（运行内存）
            currentPhysicalMemory = currentProcess.WorkingSet64;

            // 检查进程内存是否有效
            if (currentPhysicalMemory <= 0)
            {
              //  UnityEngine.Debug.LogWarning($"<color=orange>[GCMonitorPro]</color> 进程物理内存为0或无效");
                // 尝试使用私有内存作为备选
                currentPhysicalMemory = currentProcess.PrivateMemorySize64;
            }

            physicalMemoryMB = currentPhysicalMemory / (1024f * 1024f);

            // 确保系统总内存有效
            if (systemTotalMemoryMB <= 0)
            {
                systemTotalMemoryMB = GetSystemTotalMemoryPC();
            }

            // 计算物理内存使用率（Unity进程占系统总内存的百分比）
            if (systemTotalMemoryMB > 0)
            {
                physicalMemoryUsagePercent = (physicalMemoryMB / systemTotalMemoryMB) * 100f;
            }
            else
            {
                physicalMemoryUsagePercent = 0f;
            }

            // 计算系统整体内存使用率（所有进程的总和）
            systemUsedMemoryMB = GetSystemUsedMemoryPC();
            if (systemUsedMemoryMB > 0 && systemTotalMemoryMB > 0)
            {
                systemMemoryUsagePercent = (systemUsedMemoryMB / systemTotalMemoryMB) * 100f;
            }
            else
            {
                // 如果无法获取系统整体内存使用，保持为0，不要使用进程内存
                systemMemoryUsagePercent = 0f;
                systemUsedMemoryMB = 0f;
            }
        }
        catch (Exception e)
        {
           // UnityEngine.Debug.LogError($"<color=red>[GCMonitorPro]</color> PC物理内存状态更新失败: {e.Message}");
            // 使用备用方案
            physicalMemoryMB = GC.GetTotalMemory(false) / (1024f * 1024f);
            if (systemTotalMemoryMB > 0)
            {
                physicalMemoryUsagePercent = (physicalMemoryMB / systemTotalMemoryMB) * 100f;
            }
            currentPhysicalMemory = (long)(physicalMemoryMB * 1024 * 1024); // 转换为字节
        }
#endif

        // 更新详细内存统计
        UpdateDetailedMemoryStats();

        // 检测物理内存异常增长
        if (lastPhysicalMemory > 0 && currentPhysicalMemory > lastPhysicalMemory + 50 * 1024 * 1024) // 增长超过50MB
        {
            float growthMB = (currentPhysicalMemory - lastPhysicalMemory) / (1024f * 1024f);
            // UnityEngine.Debug.LogWarning($"<color=orange>[GCMonitorPro]</color> PC平台物理内存异常增长: +{growthMB:F1} MB");
        }

        lastPhysicalMemory = currentPhysicalMemory;
    }

    /// <summary>
    /// 更新Android平台内存状态（物理内存实际占用）
    /// </summary>
    private void UpdateAndroidMemoryStatus()
    {
        try
        {
            // 使用JNI调用获取真实的物理内存占用
            physicalMemoryMB = GetAndroidProcessMemory();

            // 获取Android Runtime内存信息（总内存、可用内存、最大可用内存）
            GetAndroidRuntimeMemoryInfo();

            // 确保系统总内存有效
            if (systemTotalMemoryMB <= 0)
            {
                systemTotalMemoryMB = SystemInfo.systemMemorySize;
                if (systemTotalMemoryMB <= 0)
                {
                    systemTotalMemoryMB = EstimateAndroidMemory();
                }
            }

            // 计算物理内存使用率
            if (systemTotalMemoryMB > 0)
            {
                physicalMemoryUsagePercent = (physicalMemoryMB / systemTotalMemoryMB) * 100f;
            }
            else
            {
                physicalMemoryUsagePercent = 0f;
            }

            // 获取系统整体已使用内存（运行内存）
            systemUsedMemoryMB = GetSystemUsedMemoryAndroid();
            if (systemUsedMemoryMB > 0 && systemTotalMemoryMB > 0)
            {
                systemMemoryUsagePercent = (systemUsedMemoryMB / systemTotalMemoryMB) * 100f;
            }
            else
            {
                systemMemoryUsagePercent = 0f;
                systemUsedMemoryMB = 0f;
            }

            // Android平台内存增长检测（阈值较低，因为移动设备内存较小）
            long currentPhysicalMemoryBytes = (long)(physicalMemoryMB * 1024 * 1024);
            if (lastPhysicalMemory > 0 && currentPhysicalMemoryBytes > lastPhysicalMemory + 20 * 1024 * 1024) // 增长超过20MB
            {
                float growthMB = (currentPhysicalMemoryBytes - lastPhysicalMemory) / (1024f * 1024f);
                //  UnityEngine.Debug.LogWarning($"<color=orange>[GCMonitorPro]</color> Android平台物理内存异常增长: +{growthMB:F1} MB");
            }

            lastPhysicalMemory = currentPhysicalMemoryBytes;
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"<color=red>[GCMonitorPro]</color> Android物理内存状态更新失败: {e.Message}");
            // 使用备用方案
            physicalMemoryMB = GetAndroidProcessMemoryFallback();
            if (systemTotalMemoryMB > 0)
            {
                physicalMemoryUsagePercent = (physicalMemoryMB / systemTotalMemoryMB) * 100f;
            }
            lastPhysicalMemory = (long)(physicalMemoryMB * 1024 * 1024);
        }
    }

    /// <summary>
    /// 更新备用内存状态
    /// </summary>
    private void UpdateFallbackMemoryStatus()
    {
        // 使用托管内存作为近似值
        long managedMemory = GC.GetTotalMemory(false);
        physicalMemoryMB = managedMemory / (1024f * 1024f);
        physicalMemoryUsagePercent = (physicalMemoryMB / systemTotalMemoryMB) * 100f;

        lastPhysicalMemory = managedMemory;
    }

    /**/
    /// <summary>
    /// 更新YooAsset资源状态
    /// </summary>
    private void UpdateYooAssetStatus()
    {
        try
        {
            /*#seaweed#待完善 var assetInfos = YooAssetComponent.Instance.GetAllPackageAssetInfos();
            yooAssetLoadedCount = assetInfos?.Count ?? 0;
            yooAssetMemoryUsage = (assetInfos?.Count ?? 0) * 0.5f; // 假设每个资源平均0.5MB
            */
            yooAssetLoadedCount = 0;
            yooAssetMemoryUsage = 0;
        }
        catch (Exception e)
        {
            //  UnityEngine.Debug.LogError($"<color=red>[GCMonitorPro]</color> YooAsset状态更新失败: {e.Message}");
        }
    }
    

    private void OnGUI()
    {
        if (!showOnScreen) return;

        float memoryMB = GC.GetTotalMemory(false) / (1024f * 1024f);
        int gcCount = GC.CollectionCount(0);
        long totalMem = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
        bool incrementalGC = UnityEngine.Scripting.GarbageCollector.isIncremental;

        // 🔸计算内存安全等级
        string safetyLevel = GetMemorySafetyLevel(memoryMB, out string colorTag);

        // 根据缩放状态调整字体大小和UI尺寸
        float scaleFactor = isUIScaled ? 2f : 1f;
        int fontSize = Mathf.RoundToInt(28 * scaleFactor);
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label) { richText = true, fontSize = fontSize };
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = fontSize };

#if false //#seaweed#
        // 只能设配 2160 x 3840
        // 计算UI位置：放大时向左移动保持居中
        float baseX = 1800f; // 原始X位置
        float uiWidth = 360f * scaleFactor;
        float uiX = baseX - (uiWidth - 360f) / 2f; // 向左移动一半的宽度差
#else
        // 设配所有屏幕宽度
        float baseX = Screen.width - 360f; // 原始X位置
        float uiWidth = 360f * scaleFactor;
        float uiX = baseX - (uiWidth - 360f) / 2f; // 向左移动一半的宽度差
#endif

        // 始终显示完整UI，移除收起功能
        float expandedWidth = 360 * scaleFactor;
        float expandedHeight = 1000 * scaleFactor; // 进一步增加高度确保缩放按钮显示

        //// 调试信息：在控制台显示UI尺寸
        //if (Time.frameCount % 300 == 0) // 每5秒输出一次
        //{
        //    UnityEngine.Debug.Log($"<color=yellow>[GCMonitorPro]</color> UI尺寸: {expandedWidth}x{expandedHeight}, 缩放: {scaleFactor}x");
        //}

        GUILayout.BeginArea(new Rect(uiX, 10, expandedWidth, expandedHeight), GUI.skin.box);
        // 使用滚动视图确保所有内容都能显示
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(expandedHeight - 20 * scaleFactor));

        // 标题
        GUILayout.Label("<b><color=#00FFFF>🧠 GCMonitor Pro</color></b>", labelStyle);
        GUILayout.Space(8 * scaleFactor);

        // 基础性能信息
        GUILayout.Label($"<b><color=#87CEEB>📊 基础性能</color></b>", labelStyle);
        //string fpsLimitText = targetFPS > 0 ? $"{targetFPS} FPS" : "无限制";
        //GUILayout.Label($"FPS: <color={(fps >= 55 ? "#00FF00" : fps >= 40 ? "#FFFF00" : "#FF0000")}>{fps:F1}</color> / <color=cyan>{fpsLimitText}</color>", labelStyle);
        //GUILayout.Label($"FPS限制: <color=cyan>{fpsLimitText}</color>", labelStyle);
        GUILayout.Label($"GC次数: <color=orange>{gcCount}</color>", labelStyle);
        GUILayout.Label($"增量GC: {(incrementalGC ? "<color=#00FF00>开启</color>" : "<color=#FF0000>关闭</color>")}", labelStyle);
        GUILayout.Space(8 * scaleFactor);

        // 托管内存信息
        GUILayout.Label($"<b><color=#98FB98>💾 托管内存</color></b>", labelStyle);
        GUILayout.Label($"当前内存: <color=white>{memoryMB:F1} MB</color>", labelStyle);
        GUILayout.Label($"总分配: <color=white>{(totalMem / (1024f * 1024f)):F1} MB</color>", labelStyle);
        GUILayout.Space(8 * scaleFactor);

        // 物理内存信息 - 平台优化显示
        string platformIcon = isPCPlatform ? "🖥️" : isAndroidPlatform ? "📱" : "💻";
        GUILayout.Label($"<b><color=#FFD700>{platformIcon} 物理内存 ({Application.platform})</color></b>", labelStyle);
        GUILayout.Label($"进程内存: <color=cyan>{physicalMemoryMB:F1} MB</color>", labelStyle);
        if (systemUsedMemoryMB > 0)
        {
            GUILayout.Label($"系统已用: <color=cyan>{systemUsedMemoryMB:F1} MB</color>", labelStyle);
        }
        else
        {
            // 如果无法获取系统整体内存使用，显示提示信息
            GUILayout.Label($"系统已用: <color=orange>无法获取</color>", labelStyle);
        }
        GUILayout.Label($"系统总内存: <color=white>{systemTotalMemoryMB} MB</color>", labelStyle);
        // 显示系统整体内存使用率（所有进程的总和），如果无法获取则显示进程使用率作为参考
        if (systemMemoryUsagePercent > 0)
        {
            GUILayout.Label($"系统使用率: <color={(systemMemoryUsagePercent < 50 ? "#00FF00" : systemMemoryUsagePercent < 80 ? "#FFFF00" : "#FF0000")}>{systemMemoryUsagePercent:F1}%</color>", labelStyle);
        }
        GUILayout.Label($"进程使用率: <color={(physicalMemoryUsagePercent < 50 ? "#00FF00" : physicalMemoryUsagePercent < 80 ? "#FFFF00" : "#FF0000")}>{physicalMemoryUsagePercent:F1}%</color>", labelStyle);

        // PC平台显示详细内存统计
        if (isPCPlatform && currentProcess != null)
        {
            GUILayout.Space(5 * scaleFactor);
            GUILayout.Label($"<b><color=#87CEEB>📊 PC详细统计</color></b>", labelStyle);
            GUILayout.Label($"私有内存: <color=white>{privateMemoryMB:F1} MB</color>", labelStyle);
            GUILayout.Label($"虚拟内存: <color=white>{virtualMemoryMB:F1} MB</color>", labelStyle);
            GUILayout.Label($"峰值工作集: <color=white>{peakWorkingSetMB:F1} MB</color>", labelStyle);
            GUILayout.Label($"峰值虚拟: <color=white>{peakVirtualMemoryMB:F1} MB</color>", labelStyle);
        }

        // Android平台显示设备信息
        if (isAndroidPlatform)
        {
            GUILayout.Space(5 * scaleFactor);
            GUILayout.Label($"<b><color=#87CEEB>📱 Android设备信息</color></b>", labelStyle);
            GUILayout.Label($"设备型号: <color=white>{SystemInfo.deviceModel}</color>", labelStyle);
            GUILayout.Label($"设备名称: <color=white>{SystemInfo.deviceName}</color>", labelStyle);
            GUILayout.Label($"处理器: <color=white>{SystemInfo.processorType}</color>", labelStyle);

            // 显示进程内存信息
            if (androidRuntimeTotalMemoryMB > 0)
            {
                GUILayout.Space(3 * scaleFactor);
                GUILayout.Label($"<b><color=#87CEEB>💾 进程内存</color></b>", labelStyle);
                GUILayout.Label($"物理内存占用: <color=white>{androidRuntimeTotalMemoryMB:F1} MB</color>", labelStyle);
                GUILayout.Label($"<color=#888888><size=10>注：进程实际占用的物理RAM（totalPss）</size></color>", labelStyle);
            }
        }

        // YooAsset 资源信息
        GUILayout.Space(8 * scaleFactor);
        GUILayout.Label($"<b><color=#FF69B4>📦 YooAsset资源</color></b>", labelStyle);
        GUILayout.Label($"已加载资源: <color=yellow>{yooAssetLoadedCount}</color>", labelStyle);
        GUILayout.Label($"资源内存: <color=white>{yooAssetMemoryUsage:F1} MB</color>", labelStyle);
        GUILayout.Space(8 * scaleFactor);

        // 内存安全等级
        GUILayout.Label($"<b><color=#FFA500>🛡️ 内存安全等级</color></b>", labelStyle);
        GUILayout.Label($"状态: <b><color={colorTag}>{safetyLevel}</color></b>", labelStyle);
        if (leakSuspected) GUILayout.Label("<b><color=#FF5555>⚠ 疑似内存泄漏趋势！</color></b>", labelStyle);
        GUILayout.Space(8 * scaleFactor);

        // 操作按钮区域
        GUILayout.Label($"<b><color=#DDA0DD>🔧 操作工具</color></b>", labelStyle);

        //// 🔘 FPS限制调整按钮
        //GUI.backgroundColor = Color.Lerp(Color.white, Color.yellow, 0.3f);
        //string buttonText = targetFPS > 0 ? $"🎯 FPS限制: {targetFPS}" : "🎯 FPS限制: 无限制";
        //if (GUILayout.Button(buttonText, buttonStyle, GUILayout.Height(32 * scaleFactor)))
        //{
        //    // 循环切换FPS限制：30 -> 60 -> 120 -> 无限制 -> 30
        //    if (targetFPS == 30f)
        //    {
        //        targetFPS = 60f;
        //        Application.targetFrameRate = 60;
        //        // UnityEngine.Debug.Log($"<color=yellow>[GCMonitorPro]</color> FPS限制设置为60");
        //    }
        //    else if (targetFPS == 60f)
        //    {
        //        targetFPS = 120f;
        //        Application.targetFrameRate = 120;
        //        //  UnityEngine.Debug.Log($"<color=yellow>[GCMonitorPro]</color> FPS限制设置为120");
        //    }
        //    else if (targetFPS == 120f)
        //    {
        //        targetFPS = 0f; // 0表示无限制
        //        Application.targetFrameRate = -1;
        //        // UnityEngine.Debug.Log($"<color=yellow>[GCMonitorPro]</color> FPS限制已移除");
        //    }
        //    else
        //    {
        //        targetFPS = 30f;
        //        Application.targetFrameRate = 30;
        //        //  UnityEngine.Debug.Log($"<color=yellow>[GCMonitorPro]</color> FPS限制设置为30");
        //    }
        //}

        // 🔘 缩放切换按钮
        if (isUIScaled)
        {
            // 当前是放大状态，显示缩小按钮
            GUI.backgroundColor = Color.Lerp(Color.white, Color.magenta, 0.3f);
            if (GUILayout.Button("🔍 缩小UI (1x)", buttonStyle, GUILayout.Height(32 * scaleFactor)))
            {
                isUIScaled = false;
                //  UnityEngine.Debug.Log($"<color=magenta>[GCMonitorPro]</color> UI缩小到1倍");
            }
        }
        else
        {
            // 当前是缩小状态，显示放大按钮
            GUI.backgroundColor = Color.Lerp(Color.white, Color.cyan, 0.3f);
            if (GUILayout.Button("🔍 放大UI (2x)", buttonStyle, GUILayout.Height(32 * scaleFactor)))
            {
                isUIScaled = true;
                // UnityEngine.Debug.Log($"<color=cyan>[GCMonitorPro]</color> UI放大到2倍");
            }
        }

        GUILayout.EndScrollView();

        GUILayout.EndArea();

        // 🔹绘制内存曲线
        float graphWidth = 360 * scaleFactor;
        float graphHeight = 120 * scaleFactor;
        DrawMemoryGraph(new Rect(uiX, 10 + expandedHeight + 10, graphWidth, graphHeight));
    }

    /// <summary>    /// 绘制内存趋势曲线    /// </summary> 
    private void DrawMemoryGraph(Rect rect)
    {
        GUI.Box(rect, "内存趋势 (MB)");
        float maxMemory = 1200f; // 设定曲线图上限   
        float stepX = rect.width / (float)maxGraphPoints;
        float scaleY = rect.height / maxMemory;
        Vector2 prev = Vector2.zero;
        for (int i = 0; i < maxGraphPoints; i++)
        {
            int index = (historyIndex + i) % maxGraphPoints; float mem = memoryHistory[index]; float x = rect.x + i * stepX; float y = rect.yMax - mem * scaleY;
            if (i > 0)
                DrawLine(prev, new Vector2(x, y), Color.green, 2f);
            prev = new Vector2(x, y);
        }
    }

    private void DrawLine(Vector2 p1, Vector2 p2, Color color, float width)
    {
        Color oldColor = GUI.color;
        Matrix4x4 matrix = GUI.matrix;
        GUI.color = color;
        float angle = Vector3.Angle(p2 - p1, Vector2.right);
        if (p1.y > p2.y) angle = -angle; float length = (p2 - p1).magnitude;
        GUIUtility.RotateAroundPivot(angle, p1);
        GUI.DrawTexture(new Rect(p1.x, p1.y, length, width), Texture2D.whiteTexture);
        GUI.matrix = matrix;
        GUI.color = oldColor;
    }

    private string GetMemorySafetyLevel(float memoryMB, out string color)
    {
        // 平台特定的内存安全等级计算
        float physicalMemoryScore = physicalMemoryUsagePercent / 100f;
        float managedMemoryScore;

        // 根据平台调整阈值
        if (isAndroidPlatform)
        {
            // Android平台内存阈值较低
            managedMemoryScore = memoryMB / 500f; // Android设备内存较小，500MB为危险阈值
        }
        else if (isPCPlatform)
        {
            // PC平台内存阈值较高
            managedMemoryScore = memoryMB / 1500f; // PC设备内存较大，1500MB为危险阈值
        }
        else
        {
            // 其他平台使用默认阈值
            managedMemoryScore = memoryMB / 1000f; // 默认1000MB为危险阈值
        }

        float combinedScore = Mathf.Max(physicalMemoryScore, managedMemoryScore);

        if (combinedScore < 0.4f)
        {
            color = "#00FF00";
            return "🟢 安全";
        }
        else if (combinedScore < 0.8f)
        {
            color = "#FFFF00";
            return "🟡 偏高";
        }
        else
        {
            color = "#FF4040";
            return "🔴 危险";
        }
    }
}