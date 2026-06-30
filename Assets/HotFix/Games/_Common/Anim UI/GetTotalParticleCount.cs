using System.Collections.Generic;
using FairyGUI;
using UnityEngine;

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
    static readonly List<(string name, int count)> s_topScratch = new List<(string, int)>(32);

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
            if (ps == null || !HasLiveParticles(ps))
                continue;

            stats.totalParticleCount += ps.particleCount;
            if (ps.isPlaying)
                stats.playingSystemCount++;
            if (ps.gameObject.activeInHierarchy)
                stats.activeSystemCount++;
            else
                stats.inactiveSystemCount++;
        }

        return stats;
    }

    /// <summary>
    /// 按粒子数降序填充 Top N 粒子系统（仅激活且有粒子的系统）。
    /// </summary>
    public static void FillTopSystems(List<(string name, int count)> results, int topN = 3)
    {
        if (results == null)
            return;

        results.Clear();
        if (topN <= 0)
            return;

        RefreshCacheIfNeeded();
        if (cachedParticleSystems == null)
            return;

        s_topScratch.Clear();
        foreach (var ps in cachedParticleSystems)
        {
            if (ps == null || !HasLiveParticles(ps))
                continue;

            int count = ps.particleCount;
            if (count <= 0 && ps.isPlaying)
                count = 1;

            s_topScratch.Add((GetEffectRootName(ps.transform), count));
        }

        s_topScratch.Sort((a, b) => b.count.CompareTo(a.count));
        int take = Mathf.Min(topN, s_topScratch.Count);
        for (int i = 0; i < take; ++i)
            results.Add(s_topScratch[i]);
    }

    /// <summary>
    /// 获取当前缓存的粒子系统列表（调用前会先按需刷新缓存）。
    /// </summary>
    public static ParticleSystem[] GetCachedParticleSystems()
    {
        RefreshCacheIfNeeded();
        return cachedParticleSystems;
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
        // autoRefreshInterval <= 0：仅依赖已有缓存，不每帧全量查找
    }

    internal static bool HasLiveParticles(ParticleSystem ps)
    {
        if (ps == null)
            return false;
        if (ps.particleCount > 0)
            return true;
        if (!ps.isPlaying)
            return false;
        return ps.IsAlive(true);
    }

    internal static string GetEffectRootName(Transform transform)
    {
        if (transform == null)
            return "unknown";

        Transform root = transform.root;
        if (root != null && !IsGenericRootName(root.name))
            return root.name;

        return transform.name;
    }

    static bool IsGenericRootName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return true;

        return name == "GoWrapper"
               || name.Contains("AnchorRoot")
               || name == "Stage"
               || name == "GRoot"
               || name == "Pool";
    }
}

public enum EffectCategory
{
    SymbolHit = 0,
    Transition = 1,
    Other = 2,
    Count = 3
}

public struct CategoryEffectStats
{
    public int particleCount;
    public int particleSystemCount;
    public int spineActiveCount;
}

public struct LiveParticleStats
{
    public int totalParticles;
    public int liveSystemCount;
    public int playingSystemCount;
}

/// <summary>
/// FGUI/GoWrapper 特效分类统计（符号中奖 / 转场 / 其他）。
/// </summary>
public static class EffectStatsCounter
{
    const int topPerCategory = 3;

    static readonly HashSet<int> s_countedParticleSystems = new HashSet<int>();
    static readonly HashSet<int> s_countedSpineAnimators = new HashSet<int>();
    static readonly HashSet<int> s_wrapTargetIds = new HashSet<int>();
    static readonly List<GameObject> s_wrapTargets = new List<GameObject>(64);
    static readonly Dictionary<string, int>[] s_particleTopByCategory = CreateTopMaps();
    static readonly Dictionary<string, int>[] s_spineTopByCategory = CreateTopMaps();
    static FguiPool[] s_cachedPools;
    static AnimBaseUI[] s_cachedAnimBaseUis;

    static Dictionary<string, int>[] CreateTopMaps()
    {
        var maps = new Dictionary<string, int>[(int)EffectCategory.Count];
        for (int i = 0; i < maps.Length; ++i)
            maps[i] = new Dictionary<string, int>();
        return maps;
    }

    public static void Collect(CategoryEffectStats[] categoryStats, List<(string name, int count)>[] categoryTops)
    {
        if (categoryStats == null || categoryStats.Length < (int)EffectCategory.Count)
            return;

        for (int i = 0; i < (int)EffectCategory.Count; ++i)
            categoryStats[i] = default;

        s_countedParticleSystems.Clear();
        s_countedSpineAnimators.Clear();
        ClearTopMaps();

        ParticleSystemCounter.ForceRefresh();
        s_cachedPools = Object.FindObjectsOfType<FguiPool>(true);
        s_cachedAnimBaseUis = Object.FindObjectsOfType<AnimBaseUI>(true);

        CollectWrapTargets();
        for (int i = 0; i < s_wrapTargets.Count; ++i)
            AccumulateGameObject(s_wrapTargets[i], categoryStats);

        for (int i = 0; i < s_cachedAnimBaseUis.Length; ++i)
            AccumulateAnimBaseUi(s_cachedAnimBaseUis[i], categoryStats);

        ParticleSystem[] particleSystems = ParticleSystemCounter.GetCachedParticleSystems();
        if (particleSystems != null)
        {
            for (int i = 0; i < particleSystems.Length; ++i)
                AccumulateParticleSystem(particleSystems[i], categoryStats);
        }

        if (categoryTops != null && categoryTops.Length >= (int)EffectCategory.Count)
        {
            for (int i = 0; i < (int)EffectCategory.Count; ++i)
                FillCategoryTop((EffectCategory)i, categoryTops[i]);
        }
    }

    /// <summary>
    /// 统计场景全部 live 粒子（GoWrapper + FguiPool + 全局 ParticleSystem，去重，不分类）。
    /// </summary>
    public static LiveParticleStats GetTotalLiveParticles()
    {
        s_countedParticleSystems.Clear();
        LiveParticleStats stats = default;

        ParticleSystemCounter.ForceRefresh();
        s_cachedPools = Object.FindObjectsOfType<FguiPool>(true);

        CollectWrapTargets();
        for (int i = 0; i < s_wrapTargets.Count; ++i)
            SumLiveParticlesOnGameObject(s_wrapTargets[i], ref stats);

        ParticleSystem[] particleSystems = ParticleSystemCounter.GetCachedParticleSystems();
        if (particleSystems != null)
        {
            for (int i = 0; i < particleSystems.Length; ++i)
                TryAddLiveParticle(particleSystems[i], ref stats);
        }

        return stats;
    }

    public static void ClearCache()
    {
        s_cachedPools = null;
        s_cachedAnimBaseUis = null;
        s_wrapTargets.Clear();
        s_countedParticleSystems.Clear();
        s_countedSpineAnimators.Clear();
        ClearTopMaps();
        ParticleSystemCounter.ClearCache();
    }

    static void ClearTopMaps()
    {
        for (int i = 0; i < s_particleTopByCategory.Length; ++i)
            s_particleTopByCategory[i].Clear();
        for (int i = 0; i < s_spineTopByCategory.Length; ++i)
            s_spineTopByCategory[i].Clear();
    }

    static void CollectWrapTargets()
    {
        s_wrapTargets.Clear();
        s_wrapTargetIds.Clear();

        if (GRoot.inst != null && GRoot.inst.displayObject is Container rootContainer)
        {
            IEnumerator<DisplayObject> descendants = rootContainer.GetDescendants(false);
            while (descendants.MoveNext())
            {
                DisplayObject displayObject = descendants.Current;
                if (displayObject is GoWrapper wrapper)
                    TryAddWrapTarget(wrapper.wrapTarget);
            }
        }

        if (s_cachedPools == null)
            return;

        for (int i = 0; i < s_cachedPools.Length; ++i)
        {
            FguiPool pool = s_cachedPools[i];
            if (pool == null || pool.pool == null)
                continue;

            foreach (GObject poolItem in pool.pool)
            {
                if (poolItem == null || (!poolItem.visible && poolItem.parent == null))
                    continue;

                GComponent comp = poolItem.asCom;
                if (comp == null)
                    continue;

                TryAddWrapTarget(GameCommon.FguiUtils.GetWrapperTarget(comp));
            }
        }
    }

    static void TryAddWrapTarget(GameObject target)
    {
        if (target == null)
            return;

        int id = target.GetInstanceID();
        if (!s_wrapTargetIds.Add(id))
            return;

        s_wrapTargets.Add(target);
    }

    static void SumLiveParticlesOnGameObject(GameObject root, ref LiveParticleStats stats)
    {
        if (root == null)
            return;

        ParticleSystem[] systems = root.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < systems.Length; ++i)
            TryAddLiveParticle(systems[i], ref stats);
    }

    static bool TryAddLiveParticle(ParticleSystem ps, ref LiveParticleStats stats)
    {
        if (ps == null || !ParticleSystemCounter.HasLiveParticles(ps))
            return false;

        int psId = ps.GetInstanceID();
        if (!s_countedParticleSystems.Add(psId))
            return false;

        stats.totalParticles += ps.particleCount;
        stats.liveSystemCount++;
        if (ps.isPlaying)
            stats.playingSystemCount++;
        return true;
    }

    static void AccumulateGameObject(GameObject root, CategoryEffectStats[] categoryStats)
    {
        if (root == null)
            return;

        ParticleSystem[] systems = root.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < systems.Length; ++i)
            AccumulateParticleSystem(systems[i], categoryStats);

        AnimBaseUI[] anims = root.GetComponentsInChildren<AnimBaseUI>(true);
        for (int i = 0; i < anims.Length; ++i)
            AccumulateAnimBaseUi(anims[i], categoryStats);
    }

    static void AccumulateParticleSystem(ParticleSystem ps, CategoryEffectStats[] categoryStats)
    {
        if (ps == null || !ParticleSystemCounter.HasLiveParticles(ps))
            return;

        int psId = ps.GetInstanceID();
        if (!s_countedParticleSystems.Add(psId))
            return;

        string rootName = ParticleSystemCounter.GetEffectRootName(ps.transform);
        EffectCategory category = Categorize(rootName);
        int catIndex = (int)category;
        int count = ps.particleCount;

        categoryStats[catIndex].particleCount += count;
        categoryStats[catIndex].particleSystemCount++;
        AddTopScore(s_particleTopByCategory[catIndex], rootName, count);
    }

    static void AccumulateAnimBaseUi(AnimBaseUI anim, CategoryEffectStats[] categoryStats)
    {
        if (anim == null || !IsAnimBaseUiActive(anim))
            return;

        GameObject animRoot = anim.goAnim != null ? anim.goAnim : anim.gameObject;
        Animator animator = animRoot.GetComponent<Animator>();
        if (animator == null)
            return;

        int animatorId = animator.GetInstanceID();
        if (!s_countedSpineAnimators.Add(animatorId))
            return;

        string rootName = ParticleSystemCounter.GetEffectRootName(animRoot.transform);
        EffectCategory category = Categorize(rootName);
        int catIndex = (int)category;

        categoryStats[catIndex].spineActiveCount++;
        AddTopScore(s_spineTopByCategory[catIndex], rootName, 1);
    }

    static bool IsAnimBaseUiActive(AnimBaseUI anim)
    {
        if (anim == null)
            return false;

        GameObject animRoot = anim.goAnim != null ? anim.goAnim : anim.gameObject;
        if (animRoot == null || !animRoot.activeInHierarchy)
            return false;

        Animator animator = animRoot.GetComponent<Animator>();
        return animator != null && animator.enabled && animator.speed > 0f;
    }

    static void AddTopScore(Dictionary<string, int> topMap, string rootName, int score)
    {
        if (topMap == null || string.IsNullOrEmpty(rootName) || score <= 0)
            return;

        if (topMap.TryGetValue(rootName, out int existing))
            topMap[rootName] = existing + score;
        else
            topMap[rootName] = score;
    }

    static void FillCategoryTop(EffectCategory category, List<(string name, int count)> results)
    {
        if (results == null)
            return;

        results.Clear();
        int catIndex = (int)category;
        AppendTopEntries(s_particleTopByCategory[catIndex], results);
        AppendTopEntries(s_spineTopByCategory[catIndex], results);

        results.Sort((a, b) => b.count.CompareTo(a.count));
        if (results.Count > topPerCategory)
            results.RemoveRange(topPerCategory, results.Count - topPerCategory);
    }

    static void AppendTopEntries(Dictionary<string, int> source, List<(string name, int count)> results)
    {
        foreach (KeyValuePair<string, int> kv in source)
        {
            if (kv.Value <= 0)
                continue;

            int existingIndex = results.FindIndex(item => item.name == kv.Key);
            if (existingIndex >= 0)
            {
                var existing = results[existingIndex];
                results[existingIndex] = (existing.name, existing.count + kv.Value);
            }
            else
            {
                results.Add((kv.Key, kv.Value));
            }
        }
    }

    public static EffectCategory Categorize(string rootName)
    {
        if (string.IsNullOrEmpty(rootName))
            return EffectCategory.Other;

        string name = rootName.ToLowerInvariant();
        if (name.Contains("gold_")
            || name.Contains("wild_")
            || name.Contains("scatter_")
            || name.Contains("bonus_")
            || name.Contains("sliver_")
            || name.Contains("silver_")
            || name.Contains("bar_")
            || name.Contains("watch_")
            || name.Contains("dollar_")
            || name.Contains("ring_")
            || name.Contains("car_")
            || name.Contains("ships_")
            || name.Contains("plane_")
            || name.Contains("ng_sym")
            || name.Contains("symbol_hit")
            || name.Contains("jackpotframe"))
        {
            return EffectCategory.SymbolHit;
        }

        if (name.Contains("mask")
            || name.Contains("frameloss")
            || name.Contains("gameborder")
            || name.Contains("fade")
            || name.Contains("settlement")
            || name.Contains("fireeffect"))
        {
            return EffectCategory.Transition;
        }

        return EffectCategory.Other;
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