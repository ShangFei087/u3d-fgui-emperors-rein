using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using FairyGUI;
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

    public bool showOnScreen = false;
    public bool showGraphs = false;

    // 移除收起功能，UI始終展開
    public bool isUIScaled = false; // 控制UI是否放大2倍

    const int displayGraphPoints = 60;
    const float graphRedrawInterval = 0.3f;
    const float panelBaseWidth = 440f;
    const float panelBaseHeight = 1400f;
    const float panelBaseHeightWithGraphs = 1680f;
    const float panelGraphHeight = 140f;

    GUIStyle _labelStyle;
    GUIStyle _buttonStyle;
    bool _guiStylesReady;
    bool _guiStylesScaled;

    float _nextGraphDrawTime;
    int _displayGcCount;
    float _displayMemoryMB;
    long _displayTotalMem;
    bool _displayIncrementalGc;
    string _safetyLevelText;
    string _safetyColorTag;
    string _textTitle;
    string _textBasicPerf;
    string _textCpu;
    string _textManagedMemory;
    string _textPhysicalMemory;
    string _textAndroidDevice;
    string _textPcDetail;
    string _textRenderContext;
    string _textJank;
    string _textPagFgui;
    string _textParticles;
    string _textSpine;
    string _textGcDetail;
    string _textGcEvents;
    string _textSafety;
    string _textLeakWarning;

    const int maxGcEventLines = 5;
    const float jankWindowSeconds = 10f;
    readonly string[] _gcEventLines = new string[maxGcEventLines];
    int _gcEventWriteIndex;
    int _mildJankCount;
    int _severeJankCount;
    float _jankWindowTimer;
    int _displayMildJank;
    int _displaySevereJank;
    long _lastSampleAlloc;
    float _displayGcAllocKB;
    float _displayGpuMemoryMB;

    const int particleWarningThreshold = 5000;
    const int particleCriticalThreshold = 15000;
    const int spineVertexWarningThreshold = 20000;
    const int spineVertexCriticalThreshold = 50000;
    int _displayTotalParticles;
    int _displayPlayingSystems;
    int _displayLiveSystems;
    int _displaySpineInstances;
    int _displaySpineVertices;
    int _displaySpineBones;
    int _displaySpineMeshUpdates;
    float _nextEffectSampleTime;
    const float effectSampleInterval = 0.2f;

#if UNITY_ANDROID && !UNITY_EDITOR
    AndroidJavaObject _cachedActivity;
    AndroidJavaObject _cachedActivityManager;
    bool _androidJniCached;
#endif

    private Vector2 scrollPosition = Vector2.zero; // 滚动视图位置
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

    // CPU 监控
    private float processCpuPercent;
    private float systemCpuPercent;
    private float frameTimeMs;
    private float frameLoadPercent;
    private int processorCount = 1;
    private TimeSpan lastProcessCpuTime;
    private DateTime lastCpuSampleUtc;
    private bool cpuSampleInitialized;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    private PerformanceCounter systemCpuCounter;
#endif
    private float[] cpuHistory = new float[maxGraphPoints];
    private int cpuHistoryIndex = 0;

    // Android /proc/stat 采样
    private long androidLastIdleJiffies;
    private long androidLastTotalJiffies;
    private long androidLastProcessJiffies;
    private bool androidSystemCpuInitialized;
    private bool androidProcessCpuInitialized;

    public float ProcessCpuPercent => processCpuPercent;
    public float SystemCpuPercent => systemCpuPercent;
    public float FrameTimeMs => frameTimeMs;
    public float FrameLoadPercent => frameLoadPercent;

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this; DontDestroyOnLoad(gameObject); stopwatch.Start();
        lastMemory = GC.GetTotalMemory(false); lastGCCount = GC.CollectionCount(0);

        // 检测平台类型
        DetectPlatform();

        // 初始化物理内存监控
        InitializePhysicalMemoryMonitoring();

        processorCount = Mathf.Max(1, SystemInfo.processorCount);
        InitializeCpuMonitoring();

#if UNITY_ANDROID && !UNITY_EDITOR
        updateInterval = 1.0f;
#endif
        RefreshDisplayTexts();
    }

    private void OnDestroy()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_cachedActivityManager != null)
        {
            _cachedActivityManager.Dispose();
            _cachedActivityManager = null;
        }
        if (_cachedActivity != null)
        {
            _cachedActivity.Dispose();
            _cachedActivity = null;
        }
        _androidJniCached = false;
#endif
        if (showOnScreen)
        {
            EffectStatsCounter.ClearCache();
            SpineStatsCounter.ClearCache();
        }
        if (instance == this)
            instance = null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            showOnScreen = !showOnScreen;
            if (showOnScreen)
            {
                ParticleSystemCounter.SetAutoRefreshInterval(updateInterval);
                ParticleSystemCounter.ForceRefresh();
                SpineStatsCounter.EnableHooks();
                _nextEffectSampleTime = 0f;
                UpdateEffectStatus();
                RefreshDisplayTexts();
            }
            else
            {
                EffectStatsCounter.ClearCache();
                SpineStatsCounter.ClearCache();
            }
        }

        if (!showOnScreen)
        {
            return;
        }

        if (!SpineStatsCounter.HooksEnabled)
            SpineStatsCounter.EnableHooks();

        UpdateJankStats();

        if (Time.unscaledTime >= _nextEffectSampleTime)
        {
            _nextEffectSampleTime = Time.unscaledTime + effectSampleInterval;
            UpdateEffectStatus();
            RefreshDisplayTexts();
        }

        if (Time.time >= nextUpdateTime)
        {
            nextUpdateTime = Time.time + updateInterval;
            UpdateMemoryStatus();
            UpdatePhysicalMemoryStatus();
            UpdateCpuStatus();
            RefreshDisplayTexts();
        }
    }

    private void UpdateEffectStatus()
    {
        LiveParticleStats particleStats = EffectStatsCounter.GetTotalLiveParticles();
        _displayTotalParticles = particleStats.totalParticles;
        _displayPlayingSystems = particleStats.playingSystemCount;
        _displayLiveSystems = particleStats.liveSystemCount;

        LiveSpineStats spineStats = SpineStatsCounter.GetLiveSpineStats();
        _displaySpineInstances = spineStats.displayedInstanceCount;
        _displaySpineVertices = spineStats.totalVertices;
        _displaySpineBones = spineStats.totalBones;
        _displaySpineMeshUpdates = spineStats.meshUpdatesInWindow;
    }

    private void UpdateMemoryStatus()
    {
        long currentMemory = GC.GetTotalMemory(false); int currentGCCount = GC.CollectionCount(0);
        float memoryMB = currentMemory / (1024f * 1024f);
        // GC触发检测
        if (currentGCCount > lastGCCount)
        {
            float gcInterval = stopwatch.ElapsedMilliseconds / 1000f - lastGCTime;
            lastGCTime = stopwatch.ElapsedMilliseconds / 1000f;
            RecordGcEvent(currentGCCount, gcInterval, memoryMB);
            lastGCCount = currentGCCount;
        }
        // 泄漏趋势检测
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

        long currentAlloc = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
        if (_lastSampleAlloc > 0L)
            _displayGcAllocKB = (currentAlloc - _lastSampleAlloc) / 1024f;
        _lastSampleAlloc = currentAlloc;

        _displayGcCount = currentGCCount;
        _displayMemoryMB = memoryMB;
        _displayTotalMem = currentAlloc;
        _displayIncrementalGc = UnityEngine.Scripting.GarbageCollector.isIncremental;
        _displayGpuMemoryMB = UnityEngine.Profiling.Profiler.GetAllocatedMemoryForGraphicsDriver() / (1024f * 1024f);
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

        CacheAndroidJniObjects();
        RefreshAndroidMemoryOnce();

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
            CacheAndroidJniObjects();
            if (_cachedActivityManager != null)
            {
                using (AndroidJavaObject memoryInfo = new AndroidJavaObject("android.app.ActivityManager$MemoryInfo"))
                {
                    _cachedActivityManager.Call("getMemoryInfo", memoryInfo);

                    long availMem = memoryInfo.Get<long>("availMem");
                    long totalMem = 0;
                    try
                    {
                        totalMem = memoryInfo.Get<long>("totalMem");
                    }
                    catch
                    {
                        totalMem = (long)(systemTotalMemoryMB * 1024 * 1024);
                    }

                    if (totalMem <= 0 && systemTotalMemoryMB > 0)
                        totalMem = (long)(systemTotalMemoryMB * 1024 * 1024);

                    if (totalMem > 0 && availMem >= 0 && availMem <= totalMem)
                    {
                        long usedMem = totalMem - availMem;
                        float usedMB = usedMem / (1024f * 1024f);
                        if (usedMB > 0 && usedMB <= systemTotalMemoryMB * 1.1f)
                            return usedMB;
                    }
                }
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning($"<color=orange>[GCMonitorPro]</color> \u83b7\u53d6Android\u7cfb\u7edf\u5df2\u7528\u5185\u5b58\u5931\u8d25: {e.Message}");
        }
#endif
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
            RefreshAndroidMemoryOnce();

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

    private void InitializeCpuMonitoring()
    {
        lastCpuSampleUtc = DateTime.UtcNow;
        cpuSampleInitialized = false;
        androidSystemCpuInitialized = false;
        androidProcessCpuInitialized = false;

        try
        {
            if (currentProcess == null)
            {
                currentProcess = Process.GetCurrentProcess();
            }
            if (currentProcess != null)
            {
                currentProcess.Refresh();
                lastProcessCpuTime = currentProcess.TotalProcessorTime;
            }
        }
        catch
        {
            // 部分平台 Process API 不可用
        }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        try
        {
            systemCpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            systemCpuCounter.NextValue();
        }
        catch
        {
            systemCpuCounter = null;
        }
#endif
    }

    private void UpdateJankStats()
    {
        float frameMs = Time.unscaledDeltaTime * 1000f;
        float budgetMs = Application.targetFrameRate > 0
            ? 1000f / Application.targetFrameRate
            : 1000f / 30f;
        float mildThreshold = budgetMs;
        float severeThreshold = budgetMs * 2f;

        _jankWindowTimer += Time.unscaledDeltaTime;
        if (frameMs > mildThreshold)
            ++_mildJankCount;
        if (frameMs > severeThreshold)
            ++_severeJankCount;

        if (_jankWindowTimer < jankWindowSeconds)
            return;

        _displayMildJank = _mildJankCount;
        _displaySevereJank = _severeJankCount;
        _mildJankCount = 0;
        _severeJankCount = 0;
        _jankWindowTimer = 0f;
    }

    private void RecordGcEvent(int gcCount, float gcInterval, float managedMemoryMB)
    {
        string line =
            $"GC#{gcCount} 间隔{gcInterval:F1}s 帧{frameTimeMs:F0}ms 托管{managedMemoryMB:F1}MB Alloc+{_displayGcAllocKB:F0}KB";
        _gcEventLines[_gcEventWriteIndex % maxGcEventLines] = line;
        ++_gcEventWriteIndex;
    }

    private void UpdateCpuStatus()
    {
        frameTimeMs = Time.unscaledDeltaTime * 1000f;
        float frameBudgetMs = Application.targetFrameRate > 0
            ? 1000f / Application.targetFrameRate
            : 1000f / 30f;
        frameLoadPercent = frameBudgetMs > 0f
            ? Mathf.Clamp(frameTimeMs / frameBudgetMs * 100f, 0f, 999f)
            : 0f;

        UpdateProcessCpuPercent();

        if (isAndroidPlatform)
        {
            systemCpuPercent = GetAndroidSystemCpuUsage();
        }
        else if (isPCPlatform)
        {
            systemCpuPercent = GetPCSystemCpuUsage();
        }
        else
        {
            systemCpuPercent = 0f;
        }

        float cpuSample = processCpuPercent > 0f ? processCpuPercent : frameLoadPercent;
        cpuHistory[cpuHistoryIndex] = cpuSample;
        cpuHistoryIndex = (cpuHistoryIndex + 1) % maxGraphPoints;
        lastCpuSampleUtc = DateTime.UtcNow;
    }

    private void UpdateProcessCpuPercent()
    {
        try
        {
            if (isAndroidPlatform)
            {
                processCpuPercent = GetAndroidProcessCpuUsage();
                return;
            }

            if (currentProcess == null)
            {
                try { currentProcess = Process.GetCurrentProcess(); }
                catch { processCpuPercent = 0f; return; }
            }

            currentProcess.Refresh();
            TimeSpan cpuTime = currentProcess.TotalProcessorTime;
            double elapsedSec = (DateTime.UtcNow - lastCpuSampleUtc).TotalSeconds;

            if (cpuSampleInitialized && elapsedSec >= 0.1)
            {
                double deltaCpuMs = (cpuTime - lastProcessCpuTime).TotalMilliseconds;
                processCpuPercent = (float)(deltaCpuMs / (elapsedSec * 1000.0 * processorCount) * 100.0);
                processCpuPercent = Mathf.Clamp(processCpuPercent, 0f, 100f);
            }

            lastProcessCpuTime = cpuTime;
            lastCpuSampleUtc = DateTime.UtcNow;
            cpuSampleInitialized = true;
        }
        catch
        {
            processCpuPercent = 0f;
        }
    }

    private float GetPCSystemCpuUsage()
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        if (systemCpuCounter == null) return 0f;
        try
        {
            return Mathf.Clamp(systemCpuCounter.NextValue(), 0f, 100f);
        }
        catch
        {
            return 0f;
        }
#else
        return 0f;
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static bool TryParseAndroidCpuLine(string line, out long user, out long nice, out long system, out long idle, out long iowait)
    {
        user = nice = system = idle = iowait = 0;
        if (string.IsNullOrEmpty(line) || !line.StartsWith("cpu ")) return false;

        string[] parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 5) return false;

        long.TryParse(parts[1], out user);
        long.TryParse(parts[2], out nice);
        long.TryParse(parts[3], out system);
        long.TryParse(parts[4], out idle);
        if (parts.Length > 5) long.TryParse(parts[5], out iowait);
        return true;
    }

    private float GetAndroidSystemCpuUsage()
    {
        try
        {
            string stat = System.IO.File.ReadAllText("/proc/stat");
            int lineEnd = stat.IndexOf('\n');
            if (lineEnd <= 0) return systemCpuPercent;

            string cpuLine = stat.Substring(0, lineEnd);
            if (!TryParseAndroidCpuLine(cpuLine, out long user, out long nice, out long sys, out long idle, out long iowait))
                return systemCpuPercent;

            long total = user + nice + sys + idle + iowait;
            if (!androidSystemCpuInitialized)
            {
                androidLastIdleJiffies = idle + iowait;
                androidLastTotalJiffies = total;
                androidSystemCpuInitialized = true;
                return 0f;
            }

            long totalDelta = total - androidLastTotalJiffies;
            long idleDelta = (idle + iowait) - androidLastIdleJiffies;
            androidLastIdleJiffies = idle + iowait;
            androidLastTotalJiffies = total;

            if (totalDelta <= 0) return systemCpuPercent;
            return Mathf.Clamp((totalDelta - idleDelta) * 100f / totalDelta, 0f, 100f);
        }
        catch
        {
            return systemCpuPercent;
        }
    }

    private float GetAndroidProcessCpuUsage()
    {
        try
        {
            string stat = System.IO.File.ReadAllText("/proc/self/stat");
            int closeParen = stat.LastIndexOf(')');
            if (closeParen < 0 || closeParen + 2 >= stat.Length) return processCpuPercent;

            string[] parts = stat.Substring(closeParen + 2).Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 15) return processCpuPercent;

            long utime = long.Parse(parts[11]);
            long stime = long.Parse(parts[12]);
            long processJiffies = utime + stime;

            if (!androidProcessCpuInitialized)
            {
                androidLastProcessJiffies = processJiffies;
                androidProcessCpuInitialized = true;
                return 0f;
            }

            long processDelta = processJiffies - androidLastProcessJiffies;
            androidLastProcessJiffies = processJiffies;

            double elapsedSec = updateInterval;
            const int userHz = 100;

            double processCpuMs = processDelta * 1000.0 / userHz;
            return Mathf.Clamp((float)(processCpuMs / (elapsedSec * 1000.0 * processorCount) * 100.0), 0f, 100f);
        }
        catch
        {
            return processCpuPercent;
        }
    }
#else
    private float GetAndroidSystemCpuUsage() => 0f;
    private float GetAndroidProcessCpuUsage() => 0f;
#endif

    private static string GetCpuColorTag(float percent)
    {
        if (percent < 50f) return "#00FF00";
        if (percent < 80f) return "#FFFF00";
        return "#FF0000";
    }

    private void EnsureGuiStyles()
    {
        if (Event.current == null)
            return;

        if (_guiStylesReady && _guiStylesScaled == isUIScaled)
            return;

        float scaleFactor = isUIScaled ? 2f : 1f;
        int fontSize = Mathf.RoundToInt(28 * scaleFactor);
        _labelStyle = new GUIStyle(GUI.skin.label) { richText = true, fontSize = fontSize };
        _buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = fontSize };
        _guiStylesReady = true;
        _guiStylesScaled = isUIScaled;
    }

    private void RefreshDisplayTexts()
    {
        if (!showOnScreen)
            return;

        _safetyLevelText = GetMemorySafetyLevel(_displayMemoryMB, out _safetyColorTag);

        _textTitle = "<b><color=#00FFFF>\ud83e\udde0 GCMonitor Pro</color></b>";

        string fpsLimitText = Application.targetFrameRate > 0 ? $"{Application.targetFrameRate}" : "\u65e0\u9650\u5236";
        FPS fpsSource = FPS.Instance;
        string fpsLine;
        if (fpsSource != null && fpsSource.DisplayFps >= 0)
        {
            float targetFps = Application.targetFrameRate > 0 ? Application.targetFrameRate : 30f;
            float fpsValue = fpsSource.DisplayFps;
            string fpsColor = fpsValue >= targetFps * 0.9f
                ? "#00FF00"
                : fpsValue >= targetFps * 0.7f ? "#FFFF00" : "#FF0000";
            fpsLine = fpsSource.DisplayRenderAvailable
                ? $"<color={fpsColor}>{fpsSource.DisplayFormat}</color>"
                : $"<color={fpsColor}>{fpsSource.DisplayFps}FPS | \u903b\u8f91 {fpsSource.DisplayLogicMs}ms | \u6e32\u67d3 --</color>";
        }
        else
        {
            fpsLine = "<color=#888888>--</color>";
        }

        _textBasicPerf =
            $"<b><color=#87CEEB>\ud83d\udcca \u57fa\u7840\u6027\u80fd</color></b>\n" +
            $"{fpsLine} <color=#888888>(\u9650\u5236 {fpsLimitText})</color>\n" +
            $"GC\u6b21\u6570: <color=orange>{_displayGcCount}</color>\n" +
            $"\u589e\u91cfGC: {(_displayIncrementalGc ? "<color=#00FF00>\u5f00\u542f</color>" : "<color=#FF0000>\u5173\u95ed</color>")}";

        float budgetMs = Application.targetFrameRate > 0
            ? 1000f / Application.targetFrameRate
            : 1000f / 30f;
        _textJank =
            $"<b><color=#FFB6C1>\u23f1 \u5361\u987f Jank ({jankWindowSeconds:F0}s)</color></b>\n" +
            $">\u6389\u5e27({budgetMs:F0}ms): <color=yellow>{_displayMildJank}</color> | " +
            $">\u4e25\u91cd({budgetMs * 2f:F0}ms): <color=orange>{_displaySevereJank}</color>";

        string qualityName = QualitySettings.names.Length > 0
            ? QualitySettings.names[QualitySettings.GetQualityLevel()]
            : "Unknown";
        _textRenderContext =
            $"<b><color=#B0C4DE>\ud83c\udfa8 \u6e32\u67d3\u4e0a\u4e0b\u6587</color></b>\n" +
            $"Quality: <color=white>{qualityName}({QualitySettings.GetQualityLevel()})</color> | " +
            $"vSync: <color=white>{QualitySettings.vSyncCount}</color>\n" +
            $"GPU\u5185\u5b58: <color=white>{_displayGpuMemoryMB:F1} MB</color>";

        int pagCount = PagControllerRegistry.ActiveCount;
        int pagQueue = PagUnityGlBridge.GetPendingOpCount();
        string pagQueueColor = pagQueue >= 32 ? "#FF5555" : "white";
        _textPagFgui =
            $"<b><color=#DDA0DD>\ud83c\udfae PAG / FGUI</color></b>\n" +
            $"PAG\u5b9e\u4f8b: <color=white>{pagCount}</color> | GL\u961f\u5217: <color={pagQueueColor}>{pagQueue}</color>\n" +
            $"FGUI\u5bf9\u8c61: <color=white>{Stats.ObjectCount}</color> | \u56fe\u5143: <color=white>{Stats.GraphicsCount}</color>";

        string particleCountColor = _displayTotalParticles >= particleCriticalThreshold
            ? "#FF5555"
            : _displayTotalParticles >= particleWarningThreshold ? "#FFFF00" : "white";
        _textParticles =
            $"<b><color=#87CEFA>\u2728 \u7279\u6548\u76d1\u63a7</color></b>\n" +
            $"\u5408\u8ba1\u7c92\u5b50: <color={particleCountColor}>{_displayTotalParticles}</color>\n" +
            $"<color=#888888>\u64ad\u653e\u7cfb\u7edf: {_displayPlayingSystems} | live\u7cfb\u7edf: {_displayLiveSystems}</color>";

        string spineVertexColor = _displaySpineVertices >= spineVertexCriticalThreshold
            ? "#FF5555"
            : _displaySpineVertices >= spineVertexWarningThreshold ? "#FFFF00" : "white";
        _textSpine =
            $"<b><color=#DDA0DD>\ud83e\uddb4 Spine \u76d1\u63a7</color></b>\n" +
            $"\u5b9e\u4f8b: <color=white>{_displaySpineInstances}</color>\n" +
            $"\u9876\u70b9: <color={spineVertexColor}>{_displaySpineVertices}</color> | \u9aa8\u9abc: <color=white>{_displaySpineBones}</color>\n" +
            $"<color=#888888>Mesh\u66f4\u65b0(0.2s): {_displaySpineMeshUpdates}</color>";

        _textGcDetail =
            $"<b><color=#F0E68C>\ud83d\udcc8 GC \u91c7\u6837</color></b>\n" +
            $"\u5468\u671f Alloc: <color=white>{_displayGcAllocKB:F1} KB</color>";

        var gcEventsBuilder = new StringBuilder();
        gcEventsBuilder.Append("<b><color=#F0E68C>\ud83d\udccb \u6700\u8fd1 GC \u4e8b\u4ef6</color></b>");
        bool hasGcEvent = false;
        int start = Mathf.Max(0, _gcEventWriteIndex - maxGcEventLines);
        for (int i = start; i < _gcEventWriteIndex; ++i)
        {
            string line = _gcEventLines[i % maxGcEventLines];
            if (string.IsNullOrEmpty(line))
                continue;
            hasGcEvent = true;
            gcEventsBuilder.Append('\n').Append(line);
        }
        _textGcEvents = hasGcEvent ? gcEventsBuilder.ToString() : string.Empty;

        string cpuFrameTag = GetCpuColorTag(frameLoadPercent);
        _textCpu =
            $"<b><color=#FFA07A>\u26a1 CPU \u5206\u6790</color></b>\n" +
            $"\u903b\u8f91\u6838\u5fc3: <color=white>{processorCount}</color> | <color=#888888>{SystemInfo.processorType}</color>\n" +
            $"\u5e27\u8017\u65f6: <color={cpuFrameTag}>{frameTimeMs:F2} ms</color> | \u5e27\u8d1f\u8f7d: <color={cpuFrameTag}>{frameLoadPercent:F0}%</color>";
        if (processCpuPercent > 0f)
            _textCpu += $"\n\u8fdb\u7a0b CPU: <color={GetCpuColorTag(processCpuPercent)}>{processCpuPercent:F1}%</color>";
        if (systemCpuPercent > 0f)
            _textCpu += $"\n\u7cfb\u7edf CPU: <color={GetCpuColorTag(systemCpuPercent)}>{systemCpuPercent:F1}%</color>";
        else if (!isPCPlatform && !isAndroidPlatform)
            _textCpu += "\n\u7cfb\u7edf CPU: <color=#888888>\u5f53\u524d\u5e73\u53f0\u4e0d\u652f\u6301</color>";

        _textManagedMemory =
            $"<b><color=#98FB98>\ud83d\udcbe \u6258\u7ba1\u5185\u5b58</color></b>\n" +
            $"\u5f53\u524d\u5185\u5b58: <color=white>{_displayMemoryMB:F1} MB</color>\n" +
            $"\u603b\u5206\u914d: <color=white>{(_displayTotalMem / (1024f * 1024f)):F1} MB</color>";

        string platformIcon = isPCPlatform ? "\ud83d\udda5\ufe0f" : isAndroidPlatform ? "\ud83d\udcf1" : "\ud83d\udcbb";
        string systemUsedText = systemUsedMemoryMB > 0f
            ? $"\u7cfb\u7edf\u5df2\u7528: <color=cyan>{systemUsedMemoryMB:F1} MB</color>"
            : "\u7cfb\u7edf\u5df2\u7528: <color=orange>\u65e0\u6cd5\u83b7\u53d6</color>";
        string systemUsageText = systemMemoryUsagePercent > 0f
            ? $"\u7cfb\u7edf\u4f7f\u7528\u7387: <color={GetUsageColorTag(systemMemoryUsagePercent)}>{systemMemoryUsagePercent:F1}%</color>\n"
            : string.Empty;
        _textPhysicalMemory =
            $"<b><color=#FFD700>{platformIcon} \u7269\u7406\u5185\u5b58 ({Application.platform})</color></b>\n" +
            $"\u8fdb\u7a0b\u5185\u5b58: <color=cyan>{physicalMemoryMB:F1} MB</color>\n" +
            $"{systemUsedText}\n" +
            $"\u7cfb\u7edf\u603b\u5185\u5b58: <color=white>{systemTotalMemoryMB} MB</color>\n" +
            systemUsageText +
            $"\u8fdb\u7a0b\u4f7f\u7528\u7387: <color={GetUsageColorTag(physicalMemoryUsagePercent)}>{physicalMemoryUsagePercent:F1}%</color>";

        _textPcDetail = string.Empty;
        if (isPCPlatform && currentProcess != null)
        {
            _textPcDetail =
                $"<b><color=#87CEEB>\ud83d\udcca PC\u8be6\u7ec6\u7edf\u8ba1</color></b>\n" +
                $"\u79c1\u6709\u5185\u5b58: <color=white>{privateMemoryMB:F1} MB</color>\n" +
                $"\u865a\u62df\u5185\u5b58: <color=white>{virtualMemoryMB:F1} MB</color>\n" +
                $"\u5cf0\u503c\u5de5\u4f5c\u96c6: <color=white>{peakWorkingSetMB:F1} MB</color>\n" +
                $"\u5cf0\u503c\u865a\u62df: <color=white>{peakVirtualMemoryMB:F1} MB</color>";
        }

        _textAndroidDevice = string.Empty;
        if (isAndroidPlatform)
        {
            _textAndroidDevice =
                $"<b><color=#87CEEB>\ud83d\udcf1 Android\u8bbe\u5907\u4fe1\u606f</color></b>\n" +
                $"\u8bbe\u5907\u578b\u53f7: <color=white>{SystemInfo.deviceModel}</color>\n" +
                $"\u8bbe\u5907\u540d\u79f0: <color=white>{SystemInfo.deviceName}</color>\n" +
                $"\u5904\u7406\u5668: <color=white>{SystemInfo.processorType}</color>";
            if (androidRuntimeTotalMemoryMB > 0f)
            {
                _textAndroidDevice +=
                    $"\n<b><color=#87CEEB>\ud83d\udcbe \u8fdb\u7a0b\u5185\u5b58</color></b>\n" +
                    $"\u7269\u7406\u5185\u5b58\u5360\u7528: <color=white>{androidRuntimeTotalMemoryMB:F1} MB</color>\n" +
                    "<color=#888888><size=10>\u6ce8\uff1a\u8fdb\u7a0b\u5b9e\u9645\u5360\u7528\u7684\u7269\u7406RAM\uff08totalPss\uff09</size></color>";
            }
        }

        _textSafety =
            $"<b><color=#FFA500>\ud83d\udee1\ufe0f \u5185\u5b58\u5b89\u5168\u7b49\u7ea7</color></b>\n" +
            $"\u72b6\u6001: <b><color={_safetyColorTag}>{_safetyLevelText}</color></b>";
        _textLeakWarning = leakSuspected ? "<b><color=#FF5555>\u26a0 \u7591\u4f3c\u5185\u5b58\u6cc4\u6f0f\u8d8b\u52bf\uff01</color></b>" : string.Empty;
    }

    private static string GetUsageColorTag(float percent)
    {
        if (percent < 50f) return "#00FF00";
        if (percent < 80f) return "#FFFF00";
        return "#FF0000";
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void CacheAndroidJniObjects()
    {
        if (_androidJniCached)
            return;

        try
        {
            using (AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                _cachedActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
            }

            if (_cachedActivity != null)
            {
                using (AndroidJavaClass contextClass = new AndroidJavaClass("android.content.Context"))
                {
                    string activityService = contextClass.GetStatic<string>("ACTIVITY_SERVICE");
                    _cachedActivityManager = _cachedActivity.Call<AndroidJavaObject>("getSystemService", activityService);
                }
            }

            _androidJniCached = _cachedActivity != null && _cachedActivityManager != null;
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning($"<color=orange>[GCMonitorPro]</color> Android JNI \u7f13\u5b58\u5931\u8d25: {e.Message}");
            _androidJniCached = false;
        }
    }

    private void RefreshAndroidMemoryOnce()
    {
        CacheAndroidJniObjects();

        if (_cachedActivityManager != null)
        {
            try
            {
                int pid = Process.GetCurrentProcess().Id;
                AndroidJavaObject[] memoryInfos = _cachedActivityManager.Call<AndroidJavaObject[]>(
                    "getProcessMemoryInfo", new int[] { pid });
                if (memoryInfos != null && memoryInfos.Length > 0)
                {
                    int totalPss = memoryInfos[0].Call<int>("getTotalPss");
                    if (totalPss > 0)
                    {
                        float memoryMB = totalPss / 1024f;
                        physicalMemoryMB = memoryMB;
                        androidRuntimeTotalMemoryMB = memoryMB;
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"<color=orange>[GCMonitorPro]</color> Android PSS \u8bfb\u53d6\u5931\u8d25: {e.Message}");
            }
        }

        physicalMemoryMB = GetAndroidProcessMemoryFallback();
        androidRuntimeTotalMemoryMB = 0f;
    }
#else
    private void CacheAndroidJniObjects() { }
    private void RefreshAndroidMemoryOnce() { }
#endif

    private void OnGUI()
    {
        if (!showOnScreen)
            return;

        if (string.IsNullOrEmpty(_textTitle))
            RefreshDisplayTexts();

        EnsureGuiStyles();

        float scaleFactor = isUIScaled ? 2f : 1f;
        float baseX = Screen.width - panelBaseWidth;
        float uiWidth = panelBaseWidth * scaleFactor;
        float uiX = baseX - (uiWidth - panelBaseWidth) / 2f;
        float expandedWidth = panelBaseWidth * scaleFactor;
        float expandedHeight = (showGraphs ? panelBaseHeightWithGraphs : panelBaseHeight) * scaleFactor;

        GUILayout.BeginArea(new Rect(uiX, 10f, expandedWidth, expandedHeight), GUI.skin.box);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(expandedHeight - 20f * scaleFactor));

        GUILayout.Label(_textTitle, _labelStyle);
        GUILayout.Space(8f * scaleFactor);
        GUILayout.Label(_textBasicPerf, _labelStyle);
        GUILayout.Space(8f * scaleFactor);
        GUILayout.Label(_textJank, _labelStyle);
        GUILayout.Space(8f * scaleFactor);
        GUILayout.Label(_textRenderContext, _labelStyle);
        GUILayout.Space(8f * scaleFactor);
        GUILayout.Label(_textPagFgui, _labelStyle);
        GUILayout.Space(8f * scaleFactor);
        GUILayout.Label(_textParticles, _labelStyle);
        GUILayout.Space(8f * scaleFactor);
        GUILayout.Label(_textSpine, _labelStyle);
        GUILayout.Space(8f * scaleFactor);
        GUILayout.Label(_textGcDetail, _labelStyle);
        if (!string.IsNullOrEmpty(_textGcEvents))
        {
            GUILayout.Space(5f * scaleFactor);
            GUILayout.Label(_textGcEvents, _labelStyle);
        }
        GUILayout.Space(8f * scaleFactor);
        GUILayout.Label(_textCpu, _labelStyle);
        GUILayout.Space(8f * scaleFactor);
        GUILayout.Label(_textManagedMemory, _labelStyle);
        GUILayout.Space(8f * scaleFactor);
        GUILayout.Label(_textPhysicalMemory, _labelStyle);

        if (!string.IsNullOrEmpty(_textPcDetail))
        {
            GUILayout.Space(5f * scaleFactor);
            GUILayout.Label(_textPcDetail, _labelStyle);
        }

        if (!string.IsNullOrEmpty(_textAndroidDevice))
        {
            GUILayout.Space(5f * scaleFactor);
            GUILayout.Label(_textAndroidDevice, _labelStyle);
        }

        GUILayout.Space(8f * scaleFactor);
        GUILayout.Label(_textSafety, _labelStyle);
        if (!string.IsNullOrEmpty(_textLeakWarning))
            GUILayout.Label(_textLeakWarning, _labelStyle);
        GUILayout.Space(8f * scaleFactor);

        GUILayout.Label("<b><color=#DDA0DD>\ud83d\udd27 \u64cd\u4f5c\u5de5\u5177</color></b>", _labelStyle);

        GUI.backgroundColor = Color.Lerp(Color.white, showGraphs ? Color.green : Color.gray, 0.3f);
        if (GUILayout.Button(showGraphs ? "\ud83d\udcc9 \u9690\u85cf\u66f2\u7ebf" : "\ud83d\udcc8 \u663e\u793a\u66f2\u7ebf",
                _buttonStyle, GUILayout.Height(32f * scaleFactor)))
        {
            showGraphs = !showGraphs;
            if (showGraphs)
                _nextGraphDrawTime = 0f;
        }

        if (isUIScaled)
        {
            GUI.backgroundColor = Color.Lerp(Color.white, Color.magenta, 0.3f);
            if (GUILayout.Button("\ud83d\udd0d \u7f29\u5c0fUI (1x)", _buttonStyle, GUILayout.Height(32f * scaleFactor)))
            {
                isUIScaled = false;
                _guiStylesReady = false;
            }
        }
        else
        {
            GUI.backgroundColor = Color.Lerp(Color.white, Color.cyan, 0.3f);
            if (GUILayout.Button("\ud83d\udd0d \u653e\u5927UI (2x)", _buttonStyle, GUILayout.Height(32f * scaleFactor)))
            {
                isUIScaled = true;
                _guiStylesReady = false;
            }
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();

        if (!showGraphs)
            return;

        if (Time.unscaledTime < _nextGraphDrawTime)
            return;

        _nextGraphDrawTime = Time.unscaledTime + graphRedrawInterval;

        float graphWidth = panelBaseWidth * scaleFactor;
        float graphHeight = panelGraphHeight * scaleFactor;
        float graphGap = 10f * scaleFactor;
        float baseGraphY = 10f + expandedHeight + graphGap;
        DrawMemoryGraph(new Rect(uiX, baseGraphY, graphWidth, graphHeight));
        DrawCpuGraph(new Rect(uiX, baseGraphY + graphHeight + graphGap, graphWidth, graphHeight * 0.75f));
    }

    private void DrawCpuGraph(Rect rect)
    {
        GUI.Box(rect, "CPU / \u5e27\u8d1f\u8f7d\u8d8b\u52bf (%)");
        float maxCpu = 100f;
        int pointCount = displayGraphPoints;
        int historyStep = maxGraphPoints / pointCount;
        float stepX = rect.width / (pointCount - 1);
        float scaleY = rect.height / maxCpu;
        Vector2 prev = Vector2.zero;
        for (int i = 0; i < pointCount; i++)
        {
            int index = (cpuHistoryIndex + i * historyStep) % maxGraphPoints;
            float value = cpuHistory[index];
            float x = rect.x + i * stepX;
            float y = rect.yMax - value * scaleY;
            if (i > 0)
                DrawLine(prev, new Vector2(x, y), Color.cyan, 2f);
            prev = new Vector2(x, y);
        }
    }

    /// <summary>    /// 绘制内存趋势曲线    /// </summary> 
    private void DrawMemoryGraph(Rect rect)
    {
        GUI.Box(rect, "\u5185\u5b58\u8d8b\u52bf (MB)");
        float maxMemory = 1200f;
        int pointCount = displayGraphPoints;
        int historyStep = maxGraphPoints / pointCount;
        float stepX = rect.width / (pointCount - 1);
        float scaleY = rect.height / maxMemory;
        Vector2 prev = Vector2.zero;
        for (int i = 0; i < pointCount; i++)
        {
            int index = (historyIndex + i * historyStep) % maxGraphPoints;
            float mem = memoryHistory[index];
            float x = rect.x + i * stepX;
            float y = rect.yMax - mem * scaleY;
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