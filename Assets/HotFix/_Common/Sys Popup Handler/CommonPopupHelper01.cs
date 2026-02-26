using GameMaker;
using ConsoleSlot01;
public class CommonPopupHelper01 : IComomonPopupHandler
{
    PopupConsoleCommon popup;
    public void OpenPopup(CommonPopupInfo info) {
        PageManager.Instance.OpenPage(PageName.ConsolePopupConsoleCommon, 
            new EventData<CommonPopupInfo>("Null", info) ,
            (PageBase win) => {
                popup = win as PopupConsoleCommon;
            }
        );
    }
    public void SetContent(CommonPopupInfo info) {
        if (popup == null)
        {
            DebugUtils.LogError("popup is null");
            return;
        }
        popup.SetContent(new EventData<CommonPopupInfo>("Null", info));
    }
    public void ClosePopup() {
        PageManager.Instance.ClosePage(popup,null);
        popup = null;
    }

    public bool IsOpen()
    {
        return popup != null && popup.isShowing;
    }

    /*
    public bool isTop
    {
        get
        {
            return popup != null && popup.isTop;
        }
    }
    */
    public void SetPopupToTop()
    {
        popup.SetPopupToTop();
    }
}
