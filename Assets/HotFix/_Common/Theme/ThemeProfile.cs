using System;
using System.Collections.Generic;

/// <summary>
/// 主题静态配置：大厅页、启动页 FGUI、本主题 gameId 与 Loading 映射等。
/// 公共层通过本配置做导航，不硬编码主题大厅类型。
/// 打包 AB 也只认本类：gameTheme → Profile → GameIds / HallResFolder。
/// </summary>
public sealed class ThemeProfile
{
    public ThemeKind Kind { get; }
    public PageName HallPageName { get; }
    /// <summary>Native 包内启动页组件名（如 TreasuryPageLaunch），供 AOT PageLaunch 读取。</summary>
    public string LaunchFguiName { get; }
    public IReadOnlyList<int> GameIds { get; }
    public IReadOnlyDictionary<int, PageName> LoadingPageByGameId { get; }
    /// <summary>关闭大厅时使用的 PageName（通常与 HallPageName 相同）。</summary>
    public PageName CloseHallPageName { get; }
    /// <summary>本主题大厅资源目录，相对 Assets（如 GameRes/Halls/TreasuryHall/）。</summary>
    public string HallResFolder { get; }

    ThemeProfile(
        ThemeKind kind,
        PageName hallPageName,
        string launchFguiName,
        int[] gameIds,
        Dictionary<int, PageName> loadingPageByGameId,
        PageName closeHallPageName,
        string hallResFolder)
    {
        Kind = kind;
        HallPageName = hallPageName;
        LaunchFguiName = launchFguiName;
        GameIds = gameIds;
        LoadingPageByGameId = loadingPageByGameId;
        CloseHallPageName = closeHallPageName;
        HallResFolder = NormalizeFolder(hallResFolder);
    }

    public bool TryGetLoadingPage(int gameId, out PageName loadingPage)
    {
        return LoadingPageByGameId.TryGetValue(gameId, out loadingPage);
    }

    public PageName GetLoadingPage(int gameId)
    {
        if (TryGetLoadingPage(gameId, out var page))
            return page;
        throw new ArgumentException($"ThemeProfile[{Kind}] 未配置 gameId={gameId} 的 Loading PageName", nameof(gameId));
    }

    /// <summary>当前主题应打进包的资源目录（大厅 + GameIds 对应目录）。</summary>
    public IEnumerable<string> EnumerateIncludedResFolders()
    {
        yield return HallResFolder;
        for (int i = 0; i < GameIds.Count; i++)
        {
            int id = GameIds[i];
            if (!GameResFolders.TryGetValue(id, out var dirs) || dirs == null)
                continue;
            for (int d = 0; d < dirs.Length; d++)
                yield return NormalizeFolder(dirs[d]);
        }
    }

    /// <summary>GameIds 中未在 GameResFolders 登记的 id（打包应报错，避免其它主题漏裁）。</summary>
    public List<int> GetGameIdsMissingResFolders()
    {
        var missing = new List<int>();
        for (int i = 0; i < GameIds.Count; i++)
        {
            int id = GameIds[i];
            if (!GameResFolders.TryGetValue(id, out var dirs) || dirs == null || dirs.Length == 0)
                missing.Add(id);
        }
        return missing;
    }

    public static ThemeProfile Get(ThemeKind kind)
    {
        switch (kind)
        {
            case ThemeKind.Treasury:
                return Treasury;
            case ThemeKind.Savage:
                return Savage;
            case ThemeKind.Test:
                return Test;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "未知 ThemeKind");
        }
    }

    /// <summary>
    /// 全部已登记的主题资源目录（各主题大厅 + 全部 gameId 目录 + 遗留目录）。
    /// 打包忽略 = 本集合 − 当前 Profile 的 include。
    /// </summary>
    public static IEnumerable<string> EnumerateAllRegisteredResFolders()
    {
        for (int i = 0; i < All.Count; i++)
            yield return All[i].HallResFolder;

        foreach (var pair in GameResFolders)
        {
            string[] dirs = pair.Value;
            if (dirs == null)
                continue;
            for (int d = 0; d < dirs.Length; d++)
                yield return NormalizeFolder(dirs[d]);
        }

        for (int i = 0; i < ExtraRegisteredResFolders.Length; i++)
            yield return NormalizeFolder(ExtraRegisteredResFolders[i]);
    }

    /// <summary>
    /// gameId → 资源目录（相对 Assets）。路径字典，不是第二份游戏列表；打不打只看各 Profile.GameIds。
    /// </summary>
    public static IReadOnlyDictionary<int, string[]> GameResFolders { get; } = new Dictionary<int, string[]>
    {
        [3993] = new[] { "GameRes/Games/Mei Zhou Hei Bao 3993/", "GameRes/Panel/Panel3993/" },
        [3994] = new[] { "GameRes/Games/Fei Zhou Hei Xing Xing 3994/", "GameRes/Panel/Panel3994/" },
        [3995] = new[] { "GameRes/Games/Huo Yan Gong Niu 3995/", "GameRes/Panel/Panel3995/" },
        [3996] = new[] { "GameRes/Games/Cai Fu Huo Che 3996/", "GameRes/Panel/Panel3996/" },
        [3997] = new[] { "GameRes/Games/Cai Fu Zhi Jia 3997/", "GameRes/Panel/Panel3997/" },
        [3998] = new[] { "GameRes/Games/Xing Yun Zhi Lun 3998/", "GameRes/Panel/Panel3998/" },
        [3999] = new[] { "GameRes/Games/Cai Fu Zhi Men 3999/" },
        [1700] = new[] { "GameRes/Games/Slot Zhu Zai Jin Bi 1700/" },
        [200] = new[] { "GameRes/Games/Emperors Rein 200/" },
    };

    /// <summary>无独立 gameId、任意正式/测试主题都不带的遗留目录。</summary>
    public static readonly string[] ExtraRegisteredResFolders =
    {
        "GameRes/Games/BonusGame2/",
        "GameRes/Games/Panel/",
        "GameRes/Games/Panel01/",
    };

    static string NormalizeFolder(string folder)
    {
        if (string.IsNullOrWhiteSpace(folder))
            return string.Empty;
        string value = folder.Trim().Replace('\\', '/');
        if (!value.EndsWith("/"))
            value += "/";
        return value;
    }

    /// <summary>财富主题：大厅 TreasuryHallMain，游戏 3999/3998/3997/3996。</summary>
    public static ThemeProfile Treasury { get; } = new ThemeProfile(
        ThemeKind.Treasury,
        PageName.TreasuryHallMain,
        "TreasuryPageLaunch",
        new[] { 3999, 3998, 3997, 3996 },
        new Dictionary<int, PageName>
        {
            [3999] = PageName.CaiFuZhiMenPopupGameLoading,
            [3998] = PageName.XingYunZhiLunPopupGameLoading,
            [3997] = PageName.CaiFuZhiJiaPopupGameLoading,
            [3996] = PageName.CaiFuHuoChePopupGameLoading,
        },
        PageName.TreasuryHallMain,
        "GameRes/Halls/TreasuryHall/");

    /// <summary>Savage 主题：大厅 SavageHallMain，游戏 3995/3994/3993。</summary>
    public static ThemeProfile Savage { get; } = new ThemeProfile(
        ThemeKind.Savage,
        PageName.SavageHallMain,
        "SavagePageLaunch",
        new[] { 3995, 3994, 3993 },
        new Dictionary<int, PageName>
        {
            [3995] = PageName.HuoYanGongNiuPopupGameLoading,
            [3994] = PageName.FeiZhouHeiXingXingPopupGameLoading,
            [3993] = PageName.MeiZhouHeiBaoPopupGameLoading,
        },
        PageName.SavageHallMain,
        "GameRes/Halls/SavageHall/");

    /// <summary>测试大厅：可进全部已配置子游戏（调试用）。</summary>
    public static ThemeProfile Test { get; } = new ThemeProfile(
        ThemeKind.Test,
        PageName.TestHallMain,
        "TreasuryPageLaunch",
        new[] { 3993, 3994, 3995, 3996, 3997, 3998, 3999, 1700 },
        new Dictionary<int, PageName>
        {
            [3993] = PageName.MeiZhouHeiBaoPopupGameLoading,
            [3994] = PageName.FeiZhouHeiXingXingPopupGameLoading,
            [3995] = PageName.HuoYanGongNiuPopupGameLoading,
            [3996] = PageName.CaiFuHuoChePopupGameLoading,
            [3997] = PageName.CaiFuZhiJiaPopupGameLoading,
            [3998] = PageName.XingYunZhiLunPopupGameLoading,
            [3999] = PageName.CaiFuZhiMenPopupGameLoading,
            [1700] = PageName.SlotZhuZaiJinBiPopupGameLoading,
        },
        PageName.TestHallMain,
        "GameRes/Halls/TestHall/");

    public static IReadOnlyList<ThemeProfile> All { get; } = new[] { Treasury, Savage, Test };
}
