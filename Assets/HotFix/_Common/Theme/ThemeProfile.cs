using System;
using System.Collections.Generic;

/// <summary>
/// 主题静态配置：大厅页、启动页 FGUI、本主题 gameId 与 Loading 映射等。
/// 公共层通过本配置做导航，不硬编码主题大厅类型。
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

    ThemeProfile(
        ThemeKind kind,
        PageName hallPageName,
        string launchFguiName,
        int[] gameIds,
        Dictionary<int, PageName> loadingPageByGameId,
        PageName closeHallPageName)
    {
        Kind = kind;
        HallPageName = hallPageName;
        LaunchFguiName = launchFguiName;
        GameIds = gameIds;
        LoadingPageByGameId = loadingPageByGameId;
        CloseHallPageName = closeHallPageName;
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

    /// <summary>财富主题：大厅 TreasuryHallMain，游戏 3996/3997/3998。</summary>
    public static ThemeProfile Treasury { get; } = new ThemeProfile(
        ThemeKind.Treasury,
        PageName.TreasuryHallMain,
        "TreasuryPageLaunch",
        new[] { 3996, 3997, 3998 ,1700},
        new Dictionary<int, PageName>
        {
            [3996] = PageName.CaiFuHuoChePopupGameLoading,
            [3997] = PageName.CaiFuZhiJiaPopupGameLoading,
            [3998] = PageName.XingYunZhiLunPopupGameLoading,
            [1700] = PageName.SlotZhuZaiJinBiPopupGameLoading,
        },
        PageName.TreasuryHallMain);

    /// <summary>Savage 主题：大厅 Hall01，游戏按 Hall01 卡牌 3997/3998/3999。</summary>
    public static ThemeProfile Savage { get; } = new ThemeProfile(
        ThemeKind.Savage,
        PageName.Hall01,
        "PageLaunch",
        new[] { 3997, 3998, 3999 },
        new Dictionary<int, PageName>
        {
            [3997] = PageName.CaiFuZhiJiaPopupGameLoading,
            [3998] = PageName.XingYunZhiLunPopupGameLoading,
            [3999] = PageName.CaiFuZhiMenPopupGameLoading,
        },
        PageName.Hall01);

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
        PageName.TestHallMain);
}
