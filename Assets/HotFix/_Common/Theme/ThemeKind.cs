/// <summary>
/// 产品主题种类。公共层只认枚举，不依赖具体大厅/子游戏类型。
/// </summary>
public enum ThemeKind
{
    /// <summary>财富主题（TreasuryHall，游戏 3996/3997/3998）</summary>
    Treasury = 0,

    /// <summary>Savage 主题（Hall01，游戏 3997/3998/3999）</summary>
    Savage = 1,

    /// <summary>测试大厅（TestHall，可进全部已配置子游戏；仅调试用）</summary>
    Test = 2,
}
