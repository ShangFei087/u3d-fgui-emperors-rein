using GameMaker;
using System;
using System.Collections;
using UnityEngine;

public class MachineDeviceCommonBiz : MonoSingleton<MachineDeviceCommonBiz>
{
    /// <summary>
    /// 打开后台设置界面
    /// </summary>
    public void OpenConsole()
    {

        // 受到保护时，不允许打开管理后台
        if (GlobalData.isProtectApplication) return;

        if (coOpenConsole != null)
            StopCoroutine(coOpenConsole);
        coOpenConsole = StartCoroutine(DoDelayToOpenConsole());
    }

    #region 温馨提示即将进入设置界面
    Coroutine coOpenConsole;
    IEnumerator DoDelayToOpenConsole()
    {
        if (PageManager.Instance.IndexOf(PageName.ConsolePageConsoleMain) != -1)
            yield break;

        if (MainModel.Instance.isSpin )
        {
            /*
            if (PageManager.Instance.IndexOf(PageName.PageSysMessage) == -1)
                PageManager.Instance.OpenPage(PageName.PageSysMessage, new EventData<Dictionary<string, object>>("", new Dictionary<string, object>()
                {
                    ["autoCloseTimeS"] = 3f,
                    ["message"] = I18nMgr.T("<color=red>[Note]: We are about to enter the settings page.</color>"),
                }));
            */


            if (MainModel.Instance.isSpin)
            {
                TipPopupHandler02.Instance.OpenPopup(I18nMgr.T("<color=red>[Note]: We are about to enter the settings page.</color>"));
            }


            MainModel.Instance.contentMD.isRequestToStop = true;
        }
        else
        {
            PageManager.Instance.OpenPage(PageName.ConsolePageConsoleMain);
        }

        yield return new WaitUntil(() => MainModel.Instance.contentMD == null ||  MainModel.Instance.contentMD.isSpin == false);

        MainModel.Instance.contentMD.isRequestToStop = false;

        if (PageManager.Instance.IndexOf(PageName.ConsolePageConsoleMain) == -1)
            PageManager.Instance.OpenPage(PageName.ConsolePageConsoleMain);

        coOpenConsole = null;
    }
    #endregion

    #region 测试模拟

    public void TestTicketOut()
    {
        DeviceCoinOut.Instance.DoCoinOut();  //开始退币

        //return;  // 加上这个显示“退票超时”

        // 模拟退票的机台硬件信号
        if(coTestTicketOut != null)
        {
            StopCoroutine(coTestTicketOut);
            coTestTicketOut = null;
        }
        coTestTicketOut = StartCoroutine(TestToTicketOut());
    }
    Coroutine coTestTicketOut;
     IEnumerator TestToTicketOut()
    {
        // 计算能退多少个币
        int targetCoinOutNum = DeviceUtils.GetCoinOutNum();
        yield return new WaitForSeconds(1f);
        Debug.Log($"退票数量： {targetCoinOutNum} ");
        while (--targetCoinOutNum >= 0)
        {
            yield return new WaitForSeconds(0.1f);

            EventCenter.Instance.EventTrigger<int>(SBoxSanboxEventHandle.COIN_OUT, 1);
        }

        coTestTicketOut = null;
    }

    #endregion
    #region Mqtt 远端控制逻辑
    public void CheckMqttRemoteButtonController() => DeviceRemoteControl.Instance.CheckMqttRemoteControl();

    #endregion


    /// <summary> 重复初始化打印机，直到初始化成功 </summary>
    public void InitPrinter(Action successCallback, Action<string> errorCallback) => DevicePrinterOut.Instance.InitPrinter(successCallback, errorCallback);

    /// <summary> 重复初始化纸钞机，直到初始化成功 </summary>
    public void InitBiller(Action successCallback, Action<string> errorCallback) => DeviceBillIn.Instance.InitBiller(successCallback, errorCallback);




    public void CheckLanguage(Action onFinishCallback = null)
    {
        DebugUtils.LogError($"当前语言： {SBoxModel.Instance.language} ");
        FguiI18nManager.Instance.ChangeLanguage((I18nLang)Enum.Parse(typeof(I18nLang), SBoxModel.Instance.language), onFinishCallback);
    }

}
