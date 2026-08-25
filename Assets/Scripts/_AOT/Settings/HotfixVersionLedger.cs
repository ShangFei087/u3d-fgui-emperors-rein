#if UNITY_EDITOR

using System;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 按产品线保存完整 version.json，避免 1.2 / 1.4 共用一份清单时热更号和 hash 被覆盖。
/// 账本目录：
///   Tools/HotfixDeploy/ledger/{key}/version.json   上次打包（续号）
///   Tools/HotfixDeploy/ledger/{key}/uploaded.json  上次成功上传（增量对比，由 save_hotfix_baseline 写入）
/// </summary>
public static class HotfixVersionLedger
{
    static readonly Regex InvalidFileChars = new Regex(@"[^A-Za-z0-9._-]", RegexOptions.Compiled);

    public static string LedgerRoot
    {
        get
        {
            string dataPath = Application.dataPath.Replace('\\', '/');
            string repoRoot = Path.GetFullPath(Path.Combine(dataPath, ".."));
            return Path.Combine(repoRoot, "Tools", "HotfixDeploy", "ledger");
        }
    }

    public static string CurrentPointerPath => Path.Combine(LedgerRoot, "current.json");

    public static string SanitizeSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "unknown";
        string cleaned = InvalidFileChars.Replace(value.Trim(), "");
        return string.IsNullOrEmpty(cleaned) ? "unknown" : cleaned;
    }

    public static string BuildKey(ApplicationSettings settings)
    {
        if (settings == null)
            return "unknown";

        string platform = SanitizeSegment(settings.platformName);
        string appType = settings.isRelease ? "release" : "debug";
        string buildTarget = settings.isMachine
            ? "machine"
            : EditorUserBuildSettings.activeBuildTarget.ToString().ToLowerInvariant();
        string folder = (settings.appVersion ?? "0.0.0").Replace('.', '_');
        return $"{platform}_{appType}_{buildTarget}_{folder}";
    }

    public static string GetLedgerVersionPath(string key)
    {
        return Path.Combine(LedgerRoot, SanitizeSegment(key), "version.json");
    }

    public static bool TryReadCurrentKey(out string key)
    {
        key = null;
        if (!File.Exists(CurrentPointerPath))
            return false;

        try
        {
            JObject obj = JObject.Parse(File.ReadAllText(CurrentPointerPath));
            key = obj["key"]?.ToObject<string>();
            return !string.IsNullOrWhiteSpace(key);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[HotfixLedger] 读取 current.json 失败: {ex.Message}");
            return false;
        }
    }

    public static void WriteCurrentKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        Directory.CreateDirectory(LedgerRoot);
        var obj = new JObject { ["key"] = key };
        File.WriteAllText(CurrentPointerPath, obj.ToString());
    }

    /// <summary>
    /// 盘上 version.json 应对应哪条产品线。
    /// 大版本与 Settings 一致 → 当前 Settings；
    /// 不一致且 pointer 仍指向旧线 → 旧线；
    /// pointer 已是新线则返回 null（避免把旧清单误存进新目录，归档已在切版本时完成）。
    /// </summary>
    public static string ResolveKeyForOnDiskVersionJson()
    {
        var settings = ApplicationSettings.Instance;
        if (settings == null)
            return null;

        string targetKey = BuildKey(settings);
        string saVersion = TryReadStreamingHotfixVersion();
        if (!string.IsNullOrEmpty(saVersion) && IsSameMajorMinor(saVersion, settings.appVersion))
            return targetKey;

        string pointerKey = null;
        if (TryReadCurrentKey(out pointerKey) && pointerKey != targetKey)
            return pointerKey;

        if (!string.IsNullOrEmpty(saVersion) && !IsSameMajorMinor(saVersion, settings.appVersion))
        {
            if (pointerKey == targetKey)
                return null;

            string[] hv = saVersion.Split('.');
            if (hv.Length >= 2)
            {
                string platform = SanitizeSegment(settings.platformName);
                string appType = settings.isRelease ? "release" : "debug";
                string buildTarget = settings.isMachine
                    ? "machine"
                    : EditorUserBuildSettings.activeBuildTarget.ToString().ToLowerInvariant();
                string folder = $"{hv[0]}_{hv[1]}_0";
                Debug.LogWarning(
                    $"[HotfixLedger] Settings 已是 {settings.appVersion}，清单仍是 {saVersion}，按 {folder} 归档");
                return $"{platform}_{appType}_{buildTarget}_{folder}";
            }
        }

        return targetKey;
    }

    public static void SaveActiveToLedger(string key = null)
    {
        string versionPath = PathHelper.versionSAPTH;
        if (!File.Exists(versionPath))
        {
            Debug.LogWarning($"[HotfixLedger] 跳过保存：找不到 {versionPath}");
            return;
        }

        if (string.IsNullOrWhiteSpace(key))
            key = ResolveKeyForOnDiskVersionJson();

        if (string.IsNullOrWhiteSpace(key))
        {
            Debug.Log("[HotfixLedger] 当前清单与 Settings 产品线不一致且已归档，跳过重复保存");
            return;
        }

        string dest = GetLedgerVersionPath(key);
        Directory.CreateDirectory(Path.GetDirectoryName(dest) ?? LedgerRoot);
        File.Copy(versionPath, dest, overwrite: true);
        Debug.Log($"[HotfixLedger] 已保存完整 version.json → {dest}");
    }

    /// <summary>
    /// 只把账本里的 hotfix_version / hotfix_key 写回包内清单，hash 仍由这次打包重算。
    /// </summary>
    public static bool TryRestoreVersionIdentity(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        string ledgerPath = GetLedgerVersionPath(key);
        string versionPath = PathHelper.versionSAPTH;
        if (!File.Exists(ledgerPath) || !File.Exists(versionPath))
            return false;

        try
        {
            JObject ledger = JObject.Parse(File.ReadAllText(ledgerPath));
            JObject sa = JObject.Parse(File.ReadAllText(versionPath));
            string ver = ledger["hotfix_version"]?.ToObject<string>();
            string hfKey = ledger["hotfix_key"]?.ToObject<string>();
            if (string.IsNullOrEmpty(ver))
                return false;

            sa["hotfix_version"] = ver;
            if (!string.IsNullOrEmpty(hfKey))
                sa["hotfix_key"] = hfKey;
            File.WriteAllText(versionPath, sa.ToString());
            Debug.Log($"[HotfixLedger] 已恢复热更号 {ver}（{key}），hash 表保持当前包内文件");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[HotfixLedger] 恢复热更号失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 切换 ApplicationSettings 前：把当前清单整份归档。
    /// </summary>
    public static void OnBeforeSwitchSettings()
    {
        SaveActiveToLedger();
    }

    /// <summary>
    /// 切换 ApplicationSettings 后：按新产品线恢复热更号，并更新 current.json。
    /// </summary>
    public static void OnAfterSwitchSettings()
    {
        string newKey = BuildKey(ApplicationSettings.Instance);
        TryRestoreVersionIdentity(newKey);
        WriteCurrentKey(newKey);
    }

    /// <summary>
    /// 打包改号前：归档当前清单，再按目标 Settings 恢复热更号，避免从 .0 重起。
    /// </summary>
    public static void PrepareVersionJsonForPack()
    {
        SaveActiveToLedger();
        string targetKey = BuildKey(ApplicationSettings.Instance);
        TryRestoreVersionIdentity(targetKey);
        WriteCurrentKey(targetKey);
    }

    /// <summary>
    /// 打包改号后：把新的完整 version.json 写入当前产品线账本。
    /// </summary>
    public static void SavePackedVersionJson()
    {
        string key = BuildKey(ApplicationSettings.Instance);
        SaveActiveToLedger(key);
        WriteCurrentKey(key);
    }

    public static string DescribeCurrent()
    {
        string key = TryReadCurrentKey(out string pointer) ? pointer : BuildKey(ApplicationSettings.Instance);
        string ledgerPath = GetLedgerVersionPath(key);
        string ver = "-";
        if (File.Exists(ledgerPath))
        {
            try
            {
                ver = JObject.Parse(File.ReadAllText(ledgerPath))["hotfix_version"]?.ToObject<string>() ?? "-";
            }
            catch
            {
                ver = "(读取失败)";
            }
        }
        else
        {
            ver = TryReadStreamingHotfixVersion() ?? "(尚无账本)";
        }

        return $"{key}  /  hotfix {ver}";
    }

    [MenuItem("NewBuild/保存当前热更账本")]
    public static void SaveFromMenu()
    {
        SaveActiveToLedger();
        WriteCurrentKey(BuildKey(ApplicationSettings.Instance));
        EditorUtility.DisplayDialog("热更账本", $"已保存:\n{DescribeCurrent()}", "确定");
    }

    static string TryReadStreamingHotfixVersion()
    {
        string path = PathHelper.versionSAPTH;
        if (!File.Exists(path))
            return null;

        try
        {
            return JObject.Parse(File.ReadAllText(path))["hotfix_version"]?.ToObject<string>();
        }
        catch
        {
            return null;
        }
    }

    static bool IsSameMajorMinor(string hotfixVersion, string appVersion)
    {
        string[] a = (hotfixVersion ?? "").Split('.');
        string[] b = (appVersion ?? "").Split('.');
        return a.Length >= 2 && b.Length >= 2 && a[0] == b[0] && a[1] == b[1];
    }
}

#endif
