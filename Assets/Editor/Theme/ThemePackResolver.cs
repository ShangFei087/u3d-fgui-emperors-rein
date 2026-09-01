using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 按 ApplicationSettings.gameTheme → ThemeProfile.GameIds 计算 AB 忽略目录。
/// 不维护第二份游戏列表。
/// </summary>
public static class ThemePackResolver
{
    public sealed class PackPlan
    {
        public string GameTheme;
        public ThemeKind Kind;
        public ThemeProfile Profile;
        public List<int> GameIds = new List<int>();
        public List<string> IncludeRelativeDirs = new List<string>();
        public List<string> IgnoreRelativeDirs = new List<string>();
        public List<int> MissingResFolderGameIds = new List<int>();
        public string Error;
        public bool ThemeFilterApplied => string.IsNullOrEmpty(Error);
    }

    [MenuItem("NewBuild/打印全部主题打包忽略目录")]
    public static void PrintAllPlansMenu()
    {
        Debug.Log(FormatPlan(BuildPlan(ThemeKind.Treasury)));
        Debug.Log(FormatPlan(BuildPlan(ThemeKind.Savage)));
        Debug.Log(FormatPlan(BuildPlan(ThemeKind.Test)));
        ValidateKnownPlans();
    }

    [MenuItem("NewBuild/打印当前主题打包忽略目录")]
    public static void PrintCurrentPlanMenu()
    {
        PackPlan plan = BuildPlanFromSettings();
        Debug.Log(FormatPlan(plan));
        if (!string.IsNullOrEmpty(plan.Error))
            Debug.LogError($"[ThemePack] {plan.Error}");
        for (int i = 0; i < plan.MissingResFolderGameIds.Count; i++)
            Debug.LogError($"[ThemePack] ThemeProfile[{plan.Kind}] gameId={plan.MissingResFolderGameIds[i]} 未配置 GameResFolders");
    }

    public static PackPlan BuildPlanFromSettings()
    {
        var plan = new PackPlan();
        var settings = ApplicationSettings.Instance;
        plan.GameTheme = settings != null ? settings.gameTheme : null;

        if (settings == null)
        {
            plan.Error = "ApplicationSettings.Instance 为空，跳过主题过滤";
            return plan;
        }

        if (!ThemeRuntime.TryParseKind(settings.gameTheme, out ThemeKind kind))
        {
            plan.Error =
                $"无法解析 gameTheme='{settings.gameTheme}'，跳过主题过滤（避免误裁）";
            return plan;
        }

        return BuildPlan(kind, plan.GameTheme);
    }

    public static PackPlan BuildPlan(ThemeKind kind, string gameTheme = null)
    {
        var plan = new PackPlan
        {
            GameTheme = gameTheme ?? kind.ToString(),
            Kind = kind,
            Profile = ThemeProfile.Get(kind)
        };
        for (int i = 0; i < plan.Profile.GameIds.Count; i++)
            plan.GameIds.Add(plan.Profile.GameIds[i]);

        plan.MissingResFolderGameIds = plan.Profile.GetGameIdsMissingResFolders();

        var include = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string dir in plan.Profile.EnumerateIncludedResFolders())
        {
            if (string.IsNullOrEmpty(dir))
                continue;
            include.Add(dir);
            plan.IncludeRelativeDirs.Add(dir);
        }
        plan.IncludeRelativeDirs.Sort(StringComparer.OrdinalIgnoreCase);

        var ignoreSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string dir in ThemeProfile.EnumerateAllRegisteredResFolders())
        {
            if (string.IsNullOrEmpty(dir) || include.Contains(dir))
                continue;
            ignoreSet.Add(dir);
        }

        plan.IgnoreRelativeDirs.AddRange(ignoreSet);
        plan.IgnoreRelativeDirs.Sort(StringComparer.OrdinalIgnoreCase);
        return plan;
    }

    /// <summary>相对 Assets 的目录 → 与 nopk.yaml 相同的绝对路径（正斜杠、末尾 /）。</summary>
    public static List<string> ToAbsoluteIgnoreDirs(IReadOnlyList<string> relativeDirs)
    {
        var result = new List<string>();
        if (relativeDirs == null)
            return result;

        string root = Application.dataPath.Replace('\\', '/');
        if (!root.EndsWith("/"))
            root += "/";

        for (int i = 0; i < relativeDirs.Count; i++)
        {
            string relative = relativeDirs[i];
            if (string.IsNullOrEmpty(relative))
                continue;
            relative = relative.Replace('\\', '/');
            if (relative.StartsWith("/"))
                relative = relative.TrimStart('/');
            result.Add(root + relative);
        }

        return result;
    }

    public static string FormatPlan(PackPlan plan)
    {
        var sb = new StringBuilder();
        sb.AppendLine("[ThemePack] 当前主题打包计划");
        sb.AppendLine($"  gameTheme: {plan.GameTheme}");
        if (!string.IsNullOrEmpty(plan.Error))
        {
            sb.AppendLine($"  error: {plan.Error}");
            sb.AppendLine("  themeFilter: skipped");
            return sb.ToString();
        }

        sb.AppendLine($"  kind: {plan.Kind}");
        sb.AppendLine($"  hall: {plan.Profile.HallResFolder}");
        sb.AppendLine($"  gameIds: {string.Join(", ", plan.GameIds)}");
        if (plan.MissingResFolderGameIds.Count > 0)
            sb.AppendLine($"  missingFolders: {string.Join(", ", plan.MissingResFolderGameIds)}");
        sb.AppendLine($"  include ({plan.IncludeRelativeDirs.Count}):");
        for (int i = 0; i < plan.IncludeRelativeDirs.Count; i++)
            sb.AppendLine($"    + {plan.IncludeRelativeDirs[i]}");
        sb.AppendLine($"  ignore ({plan.IgnoreRelativeDirs.Count}):");
        for (int i = 0; i < plan.IgnoreRelativeDirs.Count; i++)
            sb.AppendLine($"    - {plan.IgnoreRelativeDirs[i]}");
        return sb.ToString();
    }

    public static void ValidateKnownPlans()
    {
        int errors = 0;
        errors += ExpectContains(ThemeKind.Savage, true, "GameRes/Games/Huo Yan Gong Niu 3995/");
        errors += ExpectContains(ThemeKind.Savage, true, "GameRes/Halls/SavageHall/");
        errors += ExpectContains(ThemeKind.Savage, false, "GameRes/Games/Cai Fu Zhi Men 3999/");
        errors += ExpectContains(ThemeKind.Savage, false, "GameRes/Games/Cai Fu Huo Che 3996/");
        errors += ExpectContains(ThemeKind.Savage, false, "GameRes/Halls/TreasuryHall/");
        errors += ExpectContains(ThemeKind.Savage, false, "GameRes/Games/Slot Zhu Zai Jin Bi 1700/");

        errors += ExpectContains(ThemeKind.Treasury, true, "GameRes/Games/Cai Fu Zhi Men 3999/");
        errors += ExpectContains(ThemeKind.Treasury, true, "GameRes/Halls/TreasuryHall/");
        errors += ExpectContains(ThemeKind.Treasury, false, "GameRes/Games/Huo Yan Gong Niu 3995/");
        errors += ExpectContains(ThemeKind.Treasury, false, "GameRes/Halls/SavageHall/");
        errors += ExpectContains(ThemeKind.Treasury, false, "GameRes/Games/Slot Zhu Zai Jin Bi 1700/");

        errors += ExpectContains(ThemeKind.Test, true, "GameRes/Games/Slot Zhu Zai Jin Bi 1700/");
        errors += ExpectContains(ThemeKind.Test, true, "GameRes/Games/Cai Fu Zhi Men 3999/");
        errors += ExpectContains(ThemeKind.Test, true, "GameRes/Games/Mei Zhou Hei Bao 3993/");
        errors += ExpectContains(ThemeKind.Test, true, "GameRes/Halls/TestHall/");
        errors += ExpectContains(ThemeKind.Test, false, "GameRes/Halls/TreasuryHall/");
        errors += ExpectContains(ThemeKind.Test, false, "GameRes/Halls/SavageHall/");

        if (errors == 0)
            Debug.Log("[ThemePack] ValidateKnownPlans 通过");
        else
            Debug.LogError($"[ThemePack] ValidateKnownPlans 失败 {errors} 项");
    }

    /// <returns>失败条数 0 或 1。</returns>
    static int ExpectContains(ThemeKind kind, bool included, string relativeDir)
    {
        PackPlan plan = BuildPlan(kind);
        bool inInclude = plan.IncludeRelativeDirs.Contains(relativeDir);
        bool inIgnore = plan.IgnoreRelativeDirs.Contains(relativeDir);
        if (included)
        {
            if (inInclude && !inIgnore)
                return 0;
            Debug.LogError($"[ThemePack] {kind} 应变为 include: {relativeDir}");
            return 1;
        }

        if (inIgnore && !inInclude)
            return 0;
        Debug.LogError($"[ThemePack] {kind} 应变为 ignore: {relativeDir}");
        return 1;
    }
}
