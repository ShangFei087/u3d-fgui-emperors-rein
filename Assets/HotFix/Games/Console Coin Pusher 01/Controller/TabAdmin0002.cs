using FairyGUI;
using System.Collections;
using System.Collections.Generic;
using System;
using GameMaker;

public class TabAdmin0002 : ConsoleMenuBase
{

    public override void Init() { }

    public override void Dispose() { }



    public override void InitParam(GComponent comp, Action onClickPrev, Action onClickNext, Action onClickExitCallback)
    {
        base.InitParam(comp, onClickPrev, onClickNext, onClickExitCallback);


        goOwnerMenu.GetChild("pauseAtPopupGameLoadingOnce").asCom.GetChild("value").asRichTextField.text = PlayerPrefsUtils.isPauseAtPopupGameLoadingOnce? "ON":"OFF";
        goOwnerMenu.GetChild("deletePanel").asCom.GetChild("value").asRichTextField.text = PlayerPrefsUtils.isTestDeletePanel ? "ON" : "OFF";
        goOwnerMenu.GetChild("deleteReels").asCom.GetChild("value").asRichTextField.text = PlayerPrefsUtils.isTestDeleteReels ? "ON" : "OFF";

        ResetItem(false);
        AddClickEvent();
    }

    public override void OnClickConfirm()
    {
        if (menuMap.ContainsKey(curIndexMenuItem))
        {

            switch (menuMap[curIndexMenuItem])
            {
                case "pauseAtPopupGameLoadingOnce":
                    {
                        OnPauseAtPopupGameLoadingOnce();
                    }
                    return;
                case "deletePanel":
                    {
                        OnDeletePanel();
                    }
                    return;
                case "deleteReels":
                    {
                        OnDeleteReels();
                    }
                    return;
                case "destroyPanel":
                    {
                        OnDestroyPanel();
                    }
                    return;
                case "destroyReels":
                    {
                        OnDestroyReels();
                    }
                    return;
                case "prev":
                    {
                        onClickPrev?.Invoke();
                    }
                    return;
                case "next":
                    {
                        onClickNext?.Invoke();
                    }
                    return;
                case "exit":
                    {
                        onClickExitCallback?.Invoke();
                    }
                    return;
            }
        }
    }



    void OnPauseAtPopupGameLoadingOnce()
    {
        PlayerPrefsUtils.isPauseAtPopupGameLoadingOnce = !PlayerPrefsUtils.isPauseAtPopupGameLoadingOnce;
        goOwnerMenu.GetChild("pauseAtPopupGameLoadingOnce").asCom.GetChild("value").asRichTextField.text = PlayerPrefsUtils.isPauseAtPopupGameLoadingOnce ? "ON" : "OFF";
    }
    void OnDeletePanel()
    {
        PlayerPrefsUtils.isTestDeletePanel = !PlayerPrefsUtils.isTestDeletePanel;
        goOwnerMenu.GetChild("deletePanel").asCom.GetChild("value").asRichTextField.text = PlayerPrefsUtils.isTestDeletePanel ? "ON" : "OFF";
    }
    void OnDeleteReels()
    {
        PlayerPrefsUtils.isTestDeleteReels = !PlayerPrefsUtils.isTestDeleteReels;
        goOwnerMenu.GetChild("deleteReels").asCom.GetChild("value").asRichTextField.text = PlayerPrefsUtils.isTestDeleteReels ? "ON" : "OFF";

    }

    private void OnDestroyPanel()
    {
        EventCenter.Instance.EventTrigger<string>(GlobalEvent.ON_TEST_EVENT, GlobalEvent.DestroyPanel);
    }
    private void OnDestroyReels()
    {
        EventCenter.Instance.EventTrigger<string>(GlobalEvent.ON_TEST_EVENT, GlobalEvent.DestroyReels);
    }

}
