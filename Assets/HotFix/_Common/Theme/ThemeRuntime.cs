using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 主题运行时：按 ApplicationSettings 解析当前主题，持有 Profile 与已注册的 IThemeEntry。
/// 公共层（Main / Panel / TestManager）只依赖本类，不引用具体大厅命名空间。
/// </summary>
public static class ThemeRuntime
{
    static readonly Dictionary<ThemeKind, IThemeEntry> _entries = new Dictionary<ThemeKind, IThemeEntry>();

    /// <summary>已知主题 Entry 全名（同程序集反射 Register，避免公共层 using 主题命名空间）。</summary>
    static readonly string[] EntryTypeNames =
    {
        "TreasuryHall.TreasuryThemeEntry",
        "SavageHall.SavageThemeEntry",
        "TestHall.TestThemeEntry",
    };

    static ThemeKind _selectedKind = ThemeKind.Treasury;
    static ThemeProfile _profile = ThemeProfile.Treasury;
    static IThemeEntry _current;

    /// <summary>当前已选中的主题入口；未 Register 对应实现时为 null。</summary>
    public static IThemeEntry Current => _current;

    /// <summary>当前主题配置（即使尚未 Register Entry 也可读）。</summary>
    public static ThemeProfile Profile => _profile;

    public static ThemeKind SelectedKind => _selectedKind;

    public static bool HasCurrent => _current != null;

    /// <summary>
    /// 反射调用各主题 Entry.Register（HybridCLR 热更 DLL 可能不触发 RuntimeInitialize）。
    /// </summary>
    public static void EnsureEntriesRegistered()
    {
        for (int i = 0; i < EntryTypeNames.Length; i++)
            TryInvokeStaticRegister(EntryTypeNames[i]);
    }

    static void TryInvokeStaticRegister(string typeName)
    {
        Type type = FindType(typeName);
        if (type == null)
            return;

        MethodInfo register = type.GetMethod("Register", BindingFlags.Public | BindingFlags.Static);
        if (register == null)
            return;

        register.Invoke(null, null);
    }

    static Type FindType(string typeName)
    {
        Type type = Type.GetType(typeName);
        if (type != null)
            return type;

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            type = assemblies[i].GetType(typeName);
            if (type != null)
                return type;
        }

        return null;
    }

    /// <summary>
    /// 注册主题入口。同一 ThemeKind 后注册覆盖先前实现。
    /// </summary>
    public static void Register(ThemeKind kind, IThemeEntry entry)
    {
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        _entries[kind] = entry;

        // 若当前已选中该主题，立即刷新 Current
        if (_selectedKind == kind)
            _current = entry;
    }

    /// <summary>
    /// 根据 ApplicationSettings.gameTheme / platformName 解析并选中主题。
    /// 应在主题 Entry 完成 Register 之后、打开大厅之前调用。
    /// </summary>
    public static void SelectFromSettings()
    {
        EnsureEntriesRegistered();
        Select(ResolveKindFromSettings());
    }

    public static void Select(ThemeKind kind)
    {
        _selectedKind = kind;
        _profile = ThemeProfile.Get(kind);

        if (_entries.TryGetValue(kind, out var entry))
        {
            _current = entry;
        }
        else
        {
            _current = null;
            Debug.LogWarning($"[ThemeRuntime] 主题 {kind} 尚未 Register IThemeEntry，Profile 已切换为 {_profile.HallPageName}");
        }
    }

    /// <summary>
    /// 优先读 gameTheme，空则回退 platformName；无法识别时默认 Treasury。
    /// </summary>
    public static ThemeKind ResolveKindFromSettings()
    {
        var settings = ApplicationSettings.Instance;
        if (settings == null)
            return ThemeKind.Treasury;

        if (TryParseKind(settings.gameTheme, out var kind))
            return kind;

        if (TryParseKind(settings.platformName, out kind))
            return kind;

        Debug.LogWarning(
            $"[ThemeRuntime] 无法从 gameTheme='{settings.gameTheme}' / platformName='{settings.platformName}' 解析主题，默认 Treasury");
        return ThemeKind.Treasury;
    }

    /// <summary>仅认 Treasury / Savage / Test，无别名。</summary>
    public static bool TryParseKind(string value, out ThemeKind kind)
    {
        kind = ThemeKind.Treasury;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        string key = value.Trim();

        if (EqualsIgnoreCase(key, "Treasury"))
        {
            kind = ThemeKind.Treasury;
            return true;
        }

        if (EqualsIgnoreCase(key, "Savage"))
        {
            kind = ThemeKind.Savage;
            return true;
        }

        if (EqualsIgnoreCase(key, "Test"))
        {
            kind = ThemeKind.Test;
            return true;
        }

        return false;
    }

    static bool EqualsIgnoreCase(string a, string b)
    {
        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }
}
