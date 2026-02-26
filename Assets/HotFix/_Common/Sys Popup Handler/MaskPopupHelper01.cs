using GameMaker;
using ConsoleSlot01;

public class MaskPopupHelper01 : IMaskPopupHandel
{

    PopupConsoleMask popup;

    public void OpenPopup(string info)
    {
        PageManager.Instance.OpenPage(PageName.ConsolePopupConsoleMask,
            new EventData<string>("Null", info),
            (PageBase win) => {
                popup = win as PopupConsoleMask;
            }
        );
    }
    public void SetContent(string info)
    {
    }
    public void ClosePopup()
    {
        if (popup != null)
        {
            PageManager.Instance.ClosePage(popup, null);
        }
        popup = null;
    }
}
