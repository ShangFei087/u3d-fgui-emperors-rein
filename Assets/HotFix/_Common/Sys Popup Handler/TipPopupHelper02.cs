using System.Collections.Generic;
using Common;
using GameMaker;
using FairyGUI;
public class TipPopupHelper02 :ITipPopupHandel
{

    PopupSystemTip popup;
    public void OpenPopup(string msg)
    {
        Timers.inst.Remove(AutoClose);
        Timers.inst.Add(2f, 1, AutoClose);

        int index = PageManager.Instance.IndexOf(PageName.CommonPopupSystemTip);
        if (index == -1)
        {
            PageManager.Instance.OpenPage(PageName.CommonPopupSystemTip,
                new EventData<Dictionary<string,object>>("Null", 
                new Dictionary<string, object>(){
                    ["content"] = msg,
                }),
                (PageBase win) => {
                    popup = win as PopupSystemTip;
                }
            );
            return;
        }

        if (index != 0)
            popup.BringToFront();
        popup.ShowTip(new EventData<Dictionary<string, object>>("Null",
                new Dictionary<string, object>()
                {
                    ["content"] = msg,
                }));
    }

    public void OpenPopupOnce(string msg) => OpenPopup(msg);

    public void ClosePopup()
    {
        PageManager.Instance.ClosePage(PageName.CommonPopupSystemTip);
    }

    void AutoClose(object param)
    {
        Timers.inst.Remove(AutoClose);
        ClosePopup();
    }
}
