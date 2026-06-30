#if UNITY_EDITOR

using System;

using System.Text.RegularExpressions;

using UnityEditor;

using UnityEditor.Build;

using UnityEditor.Build.Reporting;

using UnityEngine;



/// <summary>

/// 根据 ApplicationSettings 同步 Unity Player Settings：

/// Product Name = platformName；

/// Android applicationId = com.lftlive.{platformName}.{release|debug}.{machine|android}.v{version}。

/// </summary>

public static class ApplicationSettingsPlayerSync

{

    private const string AndroidCompanyPrefix = "com.lftlive";

    private static readonly Regex ValidSegmentRegex = new Regex(@"[^A-Za-z0-9_]", RegexOptions.Compiled);



    [MenuItem("NewBuild/同步 Player Settings")]

    public static void SyncFromMenu()

    {

        TrySync(logAlways: true);

    }



    public static string SanitizeSegment(string value)

    {

        if (string.IsNullOrWhiteSpace(value))

        {

            return string.Empty;

        }



        return ValidSegmentRegex.Replace(value.Trim(), string.Empty);

    }



    /// <summary>

    /// 保证 Android 包名每一段以字母开头（不能以数字或 _ 开头）。

    /// </summary>

    private static string NormalizeSegment(string value)

    {

        string segment = SanitizeSegment(value).ToLowerInvariant();

        if (string.IsNullOrEmpty(segment))

        {

            return string.Empty;

        }



        if (char.IsDigit(segment[0]) || segment[0] == '_')

        {

            segment = "p" + segment;

        }



        return segment;

    }



    private static string BuildVersionSegment(string appVersion)

    {

        // 1.4.0 -> v1_4_0，避免段以数字开头

        string raw = SanitizeSegment(appVersion.Replace('.', '_'));

        if (string.IsNullOrEmpty(raw))

        {

            return "v0";

        }



        return raw.StartsWith("v", StringComparison.Ordinal) ? raw : "v" + raw;

    }



    public static string BuildAndroidApplicationId(ApplicationSettings settings)

    {

        string platformName = NormalizeSegment(settings.platformName);

        string appType = settings.isRelease ? "release" : "debug";

        string buildTarget = settings.isMachine

            ? "machine"

            : NormalizeSegment(EditorUserBuildSettings.activeBuildTarget.ToString());

        string versionSegment = BuildVersionSegment(settings.appVersion);



        return $"{AndroidCompanyPrefix}.{platformName}.{appType}.{buildTarget}.{versionSegment}";

    }



    private static bool IsValidAndroidApplicationId(string applicationId)

    {

        if (string.IsNullOrEmpty(applicationId))

        {

            return false;

        }



        string[] segments = applicationId.Split('.');

        if (segments.Length < 3 || segments[0] != "com")

        {

            return false;

        }



        foreach (string segment in segments)

        {

            if (string.IsNullOrEmpty(segment))

            {

                return false;

            }



            if (char.IsDigit(segment[0]) || segment[0] == '_')

            {

                return false;

            }

        }



        return true;

    }



    public static bool TrySync(bool logAlways = false)

    {

        var settings = ApplicationSettings.Instance;

        if (settings == null)

        {

            Debug.LogError("[PlayerSync] ApplicationSettings.Instance 为空，请确认 Resources/ApplicationSettings.asset 存在。");

            return false;

        }



        string targetProductName = SanitizeSegment(settings.platformName);

        if (string.IsNullOrEmpty(targetProductName))

        {

            Debug.LogError("[PlayerSync] platformName 为空或清洗后无效，跳过同步。");

            return false;

        }



        string targetAndroidId = BuildAndroidApplicationId(settings);

        if (!IsValidAndroidApplicationId(targetAndroidId))

        {

            Debug.LogError($"[PlayerSync] 生成的 applicationId 无效: {targetAndroidId}");

            return false;

        }



        string currentProductName = PlayerSettings.productName;

        string currentAndroidId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);

        bool changed = currentProductName != targetProductName || currentAndroidId != targetAndroidId;



        if (changed)

        {

            PlayerSettings.productName = targetProductName;

            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, targetAndroidId);

            AssetDatabase.SaveAssets();

            Debug.Log(

                $"[PlayerSync] 已同步 Player Settings\n" +

                $"  Product Name: {currentProductName} -> {targetProductName}\n" +

                $"  Android Id: {currentAndroidId} -> {targetAndroidId}");

        }

        else if (logAlways)

        {

            Debug.Log(

                $"[PlayerSync] Player Settings 已是目标值\n" +

                $"  Product Name: {targetProductName}\n" +

                $"  Android Id: {targetAndroidId}");

        }



        return changed;

    }

}



public class ApplicationSettingsPlayerSyncPreprocessor : IPreprocessBuildWithReport

{

    public int callbackOrder => 0;



    public void OnPreprocessBuild(BuildReport report)

    {

        ApplicationSettingsPlayerSync.TrySync();

    }

}

#endif

