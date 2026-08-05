/// <summary>
/// 主题侧入口：大厅预载/打开/返回/关闭。由各主题目录实现并注册到 ThemeRuntime。
/// </summary>
public interface IThemeEntry
{
    void PreloadHall();
    void OpenHall();
    void ReturnToHall();
    void CloseHall();
}
