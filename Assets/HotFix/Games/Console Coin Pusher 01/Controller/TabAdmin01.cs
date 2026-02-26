using FairyGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class TabAdmin01 : ConsoleMenuBase
{

    public override void Init() { }

    public override void Dispose() { }



    public override void InitParam(GComponent comp, Action onClickPrev, Action onClickNext, Action onClickExitCallback)
    {
        base.InitParam(comp, onClickPrev, onClickNext, onClickExitCallback);


        goOwnerMenu.GetChild("installVer").asCom.GetChild("value").asRichTextField.text = $"{ApplicationSettings.Instance.appVersion}/{GlobalData.installHofixVersion}";
        goOwnerMenu.GetChild("sofwareVer").asCom.GetChild("value").asRichTextField.text = $"{ApplicationSettings.Instance.appVersion}/{GlobalData.hotfixVersion}";
        goOwnerMenu.GetChild("appKey").asCom.GetChild("value").asRichTextField.text = ApplicationSettings.Instance.appKey;
        goOwnerMenu.GetChild("hotfixKey").asCom.GetChild("value").asRichTextField.text = GlobalData.hotfixKey;
        goOwnerMenu.GetChild("severAddress").asCom.GetChild("value").asRichTextField.text = ApplicationSettings.Instance.resourceServer;
        goOwnerMenu.GetChild("autoHofixAddress").asCom.GetChild("value").asRichTextField.text = GlobalData.autoHotfixUrl;

        goOwnerMenu.GetChild("isDebugLog").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.isDebugLog? "ON" : "OFF";
        goOwnerMenu.GetChild("enableDebugTool").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.isUseTestTool ? "ON" : "OFF";
        goOwnerMenu.GetChild("enableReporterPage").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.isUseReporterPage ? "ON" : "OFF";
        goOwnerMenu.GetChild("pauseAtPopupFreeSpinTrigger").asCom.GetChild("value").asRichTextField.text = PlayerPrefsUtils.isPauseAtPopupFreeSpinTrigger ? "ON" : "OFF";
        goOwnerMenu.GetChild("pauseAtPopupJackpotGame").asCom.GetChild("value").asRichTextField.text = PlayerPrefsUtils.isPauseAtPopupJackpotGame ? "ON" : "OFF";
        goOwnerMenu.GetChild("pauseAtPopupJackpotOnline").asCom.GetChild("value").asRichTextField.text = PlayerPrefsUtils.isPauseAtPopupJackpotOnline ? "ON" : "OFF";
        goOwnerMenu.GetChild("pauseAtPopupFreeSpinResult").asCom.GetChild("value").asRichTextField.text = PlayerPrefsUtils.isPauseAtPopupFreeSpinResult ? "ON" : "OFF";

        goOwnerMenu.GetChild("isUseAllConsolePage").asCom.GetChild("value").asRichTextField.text = PlayerPrefsUtils.isUseAllConsolePage ? "ON" : "OFF";

        goOwnerMenu.GetChild("isUseMqttDefault").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.isUseRemoteDefault ? "ON" : "OFF";

        goOwnerMenu.GetChild("fguiInputMethod").asCom.GetChild("value").asRichTextField.text = Stage.touchScreen ? "触摸" : "鼠标";


        /*#seaweed#*/
        if (ApplicationSettings.Instance.isRelease)
        {
            SetItemTouchable("isDebugLog", false);
            SetItemTouchable("enableDebugTool", false);
            SetItemTouchable("enableReporterPage", false);
            SetItemTouchable("pauseAtPopupFreeSpinTrigger", false);
            SetItemTouchable("pauseAtPopupJackpotGame", false);
            SetItemTouchable("pauseAtPopupJackpotOnline", false);
            SetItemTouchable("pauseAtPopupFreeSpinResult", false);
            SetItemTouchable("isUseAllConsolePage", false);
        }
        

        ResetItem(false);
        AddClickEvent();


        // 初始化FGUI（基础触摸支持）
        //Stage.touchScreen = true;
    }


    public override void OnClickConfirm()
    {
        if (menuMap.ContainsKey(curIndexMenuItem))
        {

            switch (menuMap[curIndexMenuItem])
            {
                case "isDebugLog":
                    {
                        OnClickIsDebugLog();
                    }
                    return;
                case "enableDebugTool":
                    {
                        OnClickEnableDebugTool();
                    }
                    return;
                case "enableReporterPage":
                    {
                        OnClickEnableReporterPage();
                    }
                    return;
                case "pauseAtPopupFreeSpinTrigger":
                    {
                        OnClickPauseAtPopupFreeSpinTrigger();
                    }
                    return;
                case "pauseAtPopupFreeSpinResult":
                    {
                        OnClickPauseAtPopupFreeSpinResult();
                    }
                    return;
                case "pauseAtPopupJackpotGame":
                    {
                        OnClickPauseAtPopupJackpotGame();
                    }
                    return;
                case "pauseAtPopupJackpotOnline":
                    {
                        OnClickPauseAtPopupJackpotOnline();
                    }
                    return;
                case "isUseAllConsolePage":
                    {
                        OnClickIsUseAllConsolePage();
                    }
                    return;
                case "isUseMqttDefault":
                    {
                        OnClickIsUseMqttDefault();
                    }
                    return;
                case "fguiInputMethod":
                    {
                        OnClickFguiInputMethod();
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

    private void OnClickFguiInputMethod()
    {
        Stage.touchScreen = !Stage.touchScreen;
        goOwnerMenu.GetChild("fguiInputMethod").asCom.GetChild("value").asRichTextField.text = Stage.touchScreen ? "触摸" : "鼠标";
    }


    private void OnClickIsUseMqttDefault()
    {
        SBoxModel.Instance.isUseRemoteDefault = true;
        goOwnerMenu.GetChild("isUseMqttDefault").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.isUseRemoteDefault ? "ON" : "OFF";
        MachineDeviceCommonBiz.Instance.CheckMqttRemoteButtonController();
    }
    private void OnClickIsDebugLog()
    {
        SBoxModel.Instance.isDebugLog = !SBoxModel.Instance.isDebugLog;
        goOwnerMenu.GetChild("isDebugLog").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.isDebugLog ? "ON" : "OFF";
    }

    void OnClickEnableDebugTool() {

        SBoxModel.Instance.isUseTestTool = !SBoxModel.Instance.isUseTestTool;
        goOwnerMenu.GetChild("enableDebugTool").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.isUseTestTool ? "ON" : "OFF";
        TestUtils.CheckTestManager();
    }

        
    void OnClickEnableReporterPage()
    {
        SBoxModel.Instance.isUseReporterPage = !SBoxModel.Instance.isUseReporterPage;
        goOwnerMenu.GetChild("enableReporterPage").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.isUseReporterPage ? "ON" : "OFF";
        TestUtils.CheckReporter();
    }

    void OnClickPauseAtPopupFreeSpinTrigger()
    {
        PlayerPrefsUtils.isPauseAtPopupFreeSpinTrigger = !PlayerPrefsUtils.isPauseAtPopupFreeSpinTrigger;
        goOwnerMenu.GetChild("pauseAtPopupFreeSpinTrigger").asCom.GetChild("value").asRichTextField.text = PlayerPrefsUtils.isPauseAtPopupFreeSpinTrigger ? "ON" : "OFF";
    }

    void OnClickPauseAtPopupFreeSpinResult()
    {
        PlayerPrefsUtils.isPauseAtPopupFreeSpinResult = !PlayerPrefsUtils.isPauseAtPopupFreeSpinResult;
        goOwnerMenu.GetChild("pauseAtPopupFreeSpinResult").asCom.GetChild("value").asRichTextField.text = PlayerPrefsUtils.isPauseAtPopupFreeSpinResult ? "ON" : "OFF";
        
        //"pauseAtPopupFreeSpinResult"
    }

    void OnClickPauseAtPopupJackpotGame()
    {
        PlayerPrefsUtils.isPauseAtPopupJackpotGame = !PlayerPrefsUtils.isPauseAtPopupJackpotGame;
        goOwnerMenu.GetChild("pauseAtPopupJackpotGame").asCom.GetChild("value").asRichTextField.text = PlayerPrefsUtils.isPauseAtPopupJackpotGame ? "ON" : "OFF";
    }
    void OnClickPauseAtPopupJackpotOnline()
    {
        PlayerPrefsUtils.isPauseAtPopupJackpotOnline = !PlayerPrefsUtils.isPauseAtPopupJackpotOnline;
        goOwnerMenu.GetChild("pauseAtPopupJackpotOnline").asCom.GetChild("value").asRichTextField.text = PlayerPrefsUtils.isPauseAtPopupJackpotOnline ? "ON" : "OFF";
    }


    void OnClickIsUseAllConsolePage()
    {
        PlayerPrefsUtils.isUseAllConsolePage = !PlayerPrefsUtils.isUseAllConsolePage;
        goOwnerMenu.GetChild("isUseAllConsolePage").asCom.GetChild("value").asRichTextField.text = PlayerPrefsUtils.isUseAllConsolePage ? "ON" : "OFF";
    }



}
