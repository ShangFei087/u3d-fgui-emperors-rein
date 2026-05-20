using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 粒子系统统计工具类
/// 用于统计场景中粒子系统的总粒子数
/// </summary>
public static class ParticleSystemCounter
{
    // 缓存配置
    private static ParticleSystem[] cachedParticleSystems;
    private static int lastUpdateFrame = -1;
    private static float lastUpdateTime = -1f;

    // 自动刷新间隔（秒），-1表示不自动刷新
    private static float autoRefreshInterval = -1f;

    /// <summary>
    /// 统计模式
    /// </summary>
    public enum CountMode
    {
        /// <summary>仅统计正在播放的粒子系统</summary>
        OnlyPlaying,
        /// <summary>统计所有激活且有粒子的粒子系统（包括暂停的）</summary>
        AllActive,
        /// <summary>统计所有粒子系统（包括未激活的GameObject）</summary>
        AllIncludingInactive
    }

    /// <summary>
    /// 粒子系统统计信息
    /// </summary>
    public struct ParticleSystemStats
    {
        public int totalParticleCount;
        public int activeSystemCount;
        public int playingSystemCount;
        public int inactiveSystemCount;

        public override string ToString()
        {
            return $"总粒子数: {totalParticleCount}, 激活系统数: {activeSystemCount}, " +
                   $"播放中系统数: {playingSystemCount}, 未激活系统数: {inactiveSystemCount}";
        }
    }

    /// <summary>
    /// 获取总粒子数（默认模式：仅统计正在播放的）
    /// </summary>
    public static int GetTotalParticleCount()
    {
        return GetTotalParticleCount(CountMode.OnlyPlaying);
    }

    /// <summary>
    /// 根据指定模式获取总粒子数
    /// </summary>
    public static int GetTotalParticleCount(CountMode mode)
    {
        RefreshCacheIfNeeded();

        int count = 0;

        if (cachedParticleSystems == null) return 0;

        foreach (var ps in cachedParticleSystems)
        {
            if (ps == null) continue;

            bool shouldInclude = false;

            switch (mode)
            {
                case CountMode.OnlyPlaying:
                    shouldInclude = ps.gameObject.activeInHierarchy && ps.isPlaying;
                    break;
                case CountMode.AllActive:
                    shouldInclude = ps.gameObject.activeInHierarchy && ps.particleCount > 0;
                    break;
                case CountMode.AllIncludingInactive:
                    shouldInclude = ps.particleCount > 0;
                    break;
            }

            if (shouldInclude)
            {
                count += ps.particleCount;
            }
        }

        return count;
    }

    /// <summary>
    /// 获取详细的统计信息
    /// </summary>
    public static ParticleSystemStats GetDetailedStats()
    {
        RefreshCacheIfNeeded();

        ParticleSystemStats stats = new ParticleSystemStats();

        if (cachedParticleSystems == null) return stats;

        foreach (var ps in cachedParticleSystems)
        {
            if (ps == null) continue;

            if (ps.gameObject.activeInHierarchy)
            {
                stats.activeSystemCount++;
                if (ps.isPlaying)
                {
                    stats.playingSystemCount++;
                    stats.totalParticleCount += ps.particleCount;
                }
                else if (ps.particleCount > 0)
                {
                    // 暂停但有粒子的情况也算入总数
                    stats.totalParticleCount += ps.particleCount;
                }
            }
            else
            {
                stats.inactiveSystemCount++;
            }
        }

        return stats;
    }

    /// <summary>
    /// 强制刷新缓存
    /// </summary>
    public static void ForceRefresh()
    {
        cachedParticleSystems = GameObject.FindObjectsOfType<ParticleSystem>(true);
        lastUpdateFrame = Time.frameCount;
        lastUpdateTime = Time.realtimeSinceStartup;
    }

    /// <summary>
    /// 设置自动刷新间隔（秒）
    /// </summary>
    /// <param name="interval">间隔秒数，-1表示不自动刷新（仅每帧自动刷新一次）</param>
    public static void SetAutoRefreshInterval(float interval)
    {
        autoRefreshInterval = interval;
    }

    /// <summary>
    /// 清除缓存，下次调用时会重新查找
    /// </summary>
    public static void ClearCache()
    {
        cachedParticleSystems = null;
        lastUpdateFrame = -1;
        lastUpdateTime = -1f;
    }

    /// <summary>
    /// 判断是否需要刷新缓存
    /// </summary>
    private static void RefreshCacheIfNeeded()
    {
        if (cachedParticleSystems == null)
        {
            ForceRefresh();
            return;
        }

        // 每帧最多刷新一次
        if (Time.frameCount == lastUpdateFrame) return;

        // 检查时间间隔
        if (autoRefreshInterval > 0)
        {
            float currentTime = Time.realtimeSinceStartup;
            if (currentTime - lastUpdateTime >= autoRefreshInterval)
            {
                ForceRefresh();
            }
        }
        else
        {
            // 默认每帧刷新一次（但避免重复刷新）
            ForceRefresh();
        }
    }
}

/// <summary>
/// 带有自动清理功能的粒子系统管理器
/// 用于需要持续监控粒子数量的场景
/// </summary>
public class ParticleSystemMonitor : MonoBehaviour
{
    [Header("统计设置")]
    [SerializeField] private ParticleSystemCounter.CountMode countMode = ParticleSystemCounter.CountMode.OnlyPlaying;
    [SerializeField] private bool logOnUpdate = false;
    [SerializeField] private float autoRefreshInterval = 0.5f;

    [Header("阈值警报")]
    [SerializeField] private int warningThreshold = 10000;
    [SerializeField] private int criticalThreshold = 50000;

    [Header("动态清理")]
    [SerializeField] private bool autoCleanupExcessParticles = false;
    [SerializeField] private int targetParticleCount = 5000;

    private int lastParticleCount = 0;
    private float lastLogTime = 0f;

    private void Awake()
    {
        ParticleSystemCounter.SetAutoRefreshInterval(autoRefreshInterval);
    }

    private void Update()
    {
        int currentCount = ParticleSystemCounter.GetTotalParticleCount(countMode);

        // 检查阈值
        if (currentCount > criticalThreshold)
        {
            Debug.LogError($"[粒子系统监控] 临界阈值！当前粒子数: {currentCount}");

            if (autoCleanupExcessParticles)
            {
                CleanupExcessParticles(targetParticleCount);
            }
        }
        else if (currentCount > warningThreshold)
        {
            Debug.LogWarning($"[粒子系统监控] 超过警告阈值！当前粒子数: {currentCount}");
        }

        // 定期日志
        if (logOnUpdate && Time.time - lastLogTime >= 1f)
        {
            var stats = ParticleSystemCounter.GetDetailedStats();
            Debug.Log($"[粒子系统监控] {stats}");
            lastLogTime = Time.time;
        }

        lastParticleCount = currentCount;
    }

    /// <summary>
    /// 清理超出目标数量的粒子
    /// </summary>
    private void CleanupExcessParticles(int targetCount)
    {
        var stats = ParticleSystemCounter.GetDetailedStats();
        if (stats.totalParticleCount <= targetCount) return;

        int toRemove = stats.totalParticleCount - targetCount;
        Debug.Log($"[粒子系统监控] 尝试清理 {toRemove} 个粒子...");

        // 找到所有激活的粒子系统，按粒子数排序
        var allSystems = GameObject.FindObjectsOfType<ParticleSystem>(true);
        var activeSystems = new List<ParticleSystem>();

        foreach (var ps in allSystems)
        {
            if (ps != null && ps.gameObject.activeInHierarchy && ps.particleCount > 0)
            {
                activeSystems.Add(ps);
            }
        }

        // 按粒子数降序排序
        activeSystems.Sort((a, b) => b.particleCount.CompareTo(a.particleCount));

        int removed = 0;
        foreach (var ps in activeSystems)
        {
            if (removed >= toRemove) break;

            int countToRemove = Mathf.Min(ps.particleCount, toRemove - removed);
            if (countToRemove > 0)
            {
                // 清除整个粒子系统（简单处理）
                ps.Clear();
                removed += countToRemove;
                Debug.Log($"[粒子系统监控] 清除了 {countToRemove} 个粒子从 {ps.name}");
            }
        }

        Debug.Log($"[粒子系统监控] 已清理 {removed} 个粒子");
    }

    /// <summary>
    /// 手动强制刷新缓存
    /// </summary>
    public void RefreshNow()
    {
        ParticleSystemCounter.ForceRefresh();
    }

    /// <summary>
    /// 手动获取当前粒子数并输出日志
    /// </summary>
    public void LogCurrentCount()
    {
        var stats = ParticleSystemCounter.GetDetailedStats();
        Debug.Log($"[粒子系统监控] 当前统计: {stats}");
    }

    private void OnDestroy()
    {
        ParticleSystemCounter.ClearCache();
    }
}

/// <summary>
/// 简单的单例模式粒子计数器（如果需要在全局快速访问）
/// </summary>
public class ParticleCounter : MonoBehaviour
{
    private static ParticleCounter instance;

    public static ParticleCounter Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("ParticleCounter");
                instance = go.AddComponent<ParticleCounter>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [SerializeField] private ParticleSystemCounter.CountMode defaultMode = ParticleSystemCounter.CountMode.OnlyPlaying;

    /// <summary>
    /// 获取当前总粒子数（使用默认模式）
    /// </summary>
    public static int TotalParticleCount => ParticleSystemCounter.GetTotalParticleCount(Instance.defaultMode);

    /// <summary>
    /// 获取当前总粒子数（指定模式）
    /// </summary>
    public static int GetCount(ParticleSystemCounter.CountMode mode = ParticleSystemCounter.CountMode.OnlyPlaying)
    {
        return ParticleSystemCounter.GetTotalParticleCount(mode);
    }

    /// <summary>
    /// 获取详细统计
    /// </summary>
    public static ParticleSystemCounter.ParticleSystemStats GetStats()
    {
        return ParticleSystemCounter.GetDetailedStats();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}