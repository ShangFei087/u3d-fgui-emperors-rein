using GameMaker;
using ConsoleSlot01;
public class TipPopupHelper01 : ITipPopupHandel
{
    PopupConsoleTip popup;
    public void OpenPopup(string msg)
    {

        int index = PageManager.Instance.IndexOf(PageName.ConsolePopupConsoleTip);
        if (index  == -1)
        {
            PageManager.Instance.OpenPage(PageName.ConsolePopupConsoleTip,
                new EventData<string>("Null", msg),
                (PageBase win) => {
                    popup = win as PopupConsoleTip;
                }
            );
            return;
        }
        if(index != 0)
            popup.BringToFront();
        popup.ShowTip(new EventData<string>("Null", msg));
    }
    public void OpenPopupOnce(string msg)
    {
        int index = PageManager.Instance.IndexOf(PageName.ConsolePopupConsoleTip);
        if (index == -1)
        {
            PageManager.Instance.OpenPage(PageName.ConsolePopupConsoleTip,
                new EventData<string>("Null", msg),
                (PageBase win) => {
                    popup = win as PopupConsoleTip;
                }
            );
            return;
        }

        if (index != 0)
            popup.BringToFront();

        if (!popup.Contains(msg))
            popup.ShowTip(new EventData<string>("Null", msg));
    }
    public void ClosePopup()
    {
        PageManager.Instance.ClosePage(PageName.ConsolePopupConsoleTip);
    }
}
