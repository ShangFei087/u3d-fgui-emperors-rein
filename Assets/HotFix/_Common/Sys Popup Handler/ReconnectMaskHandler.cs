using GameMaker;

/// <summary>
/// 断电重连遮罩开关。不自动关闭，由进游戏恢复完成后显式 Close。
/// </summary>
public static class ReconnectMaskHandler
{
    public static void Open()
    {
        if (PageManager.Instance.IndexOf(PageName.CommonPopupReconnectMask) != -1)
        {
            return;
        }

        PageManager.Instance.OpenPage(PageName.CommonPopupReconnectMask);
    }

    public static void Close()
    {
        if (PageManager.Instance.IndexOf(PageName.CommonPopupReconnectMask) == -1)
        {
            return;
        }

        PageManager.Instance.ClosePage(PageName.CommonPopupReconnectMask);
    }
}
