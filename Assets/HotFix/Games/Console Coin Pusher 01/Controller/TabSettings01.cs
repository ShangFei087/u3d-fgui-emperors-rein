using FairyGUI;
using GameMaker;
using SBoxApi;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using Newtonsoft.Json;

public class TabSettings01: ConsoleMenuBase
{
    string str;

    public TabSettings01()
    {
        Init();
    }
    public override void Init() 
    {
        EventCenter.Instance.AddEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, changOnLine);
    }

    public override void Dispose() 
    {
        EventCenter.Instance.RemoveEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, changOnLine);
    }



    GComponent goItemUser, goItemManager, goItemAdmin;

    public new void InitParam(GComponent comp, Action onClickPrev,  Action onClickNext, Action onClickExitCallback) //频繁调用-多语言
    {
        base.InitParam(comp,  onClickPrev,  onClickNext,  onClickExitCallback);


        goOwnerMenu.GetChild("lineID").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.LineId;
        goOwnerMenu.GetChild("machineID").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.MachineId;

        goOwnerMenu.GetChild("pid").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.seatId.ToString();

        goOwnerMenu.GetChild("groupId").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.groupId.ToString();

        

        goOwnerMenu.GetChild("remoteCtrlEnable").asCom.GetChild("value").text = SBoxModel.Instance.isUseRemoteControl ? "ON" : "OFF";

        goOwnerMenu.GetChild("remoteCtrlIP").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.remoteControlSetting;
        if (SBoxModel.Instance.isConnectRemoteControl)
        {
            // 绿色已连接（使用十六进制绿色值 #00FF00）
            goOwnerMenu.GetChild("remoteCtrlIP").asCom.GetChild("value").asRichTextField.text += " [color=#00FF00](已连接)[/color]";
        }
        else
        {
            // 红色未连接（使用十六进制红色值 #FF0000）
            goOwnerMenu.GetChild("remoteCtrlIP").asCom.GetChild("value").asRichTextField.text += " [color=#FF0000](未连接)[/color]";
        }
        goOwnerMenu.GetChild("remoteCtrlAccount").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.remoteControlAccount + "/" + SBoxModel.Instance.remoteControlPassword;

        //goOwnerMenu.GetChild("JPBet").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.JPBetIndex.ToString();
        //goOwnerMenu.GetChild("BallPerCoin").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.JPBet[SBoxModel.Instance.BallPerCoinIndex].ToString();

        #region 动态添加选项

        GComponent goMenu = goOwnerMenu;


        GComponent goItem = goMenu.GetChild("user")?.asCom ?? null;
        if (goItem != null)
        {
            goMenu.RemoveChild(goItem);
            goItem.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().mark = "consoleSetting-menu-user";
            goItem.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().InitParam(goItem);
            if (goItemUser != null && goItemUser.displayObject.gameObject != null)
                goItemUser.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount--;
            goItemUser = goItem;
            goItemUser.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount++;
        }

        goItem = goMenu.GetChild("manager")?.asCom ?? null;
        if (goItem != null)
        {
            goMenu.RemoveChild(goItem);
            goItem.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().mark = "consoleSetting-menu-Manager";
            goItem.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().InitParam(goItem);
            if (goItemManager != null && goItemManager.displayObject.gameObject != null)
                goItemManager.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount--;
            goItemManager = goItem;
            goItemManager.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount++;
        }

        goItem = goMenu.GetChild("admin")?.asCom ?? null;
        if (goItem != null)
        {
            goMenu.RemoveChild(goItem);
            goItem.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().mark = "consoleSetting-menu-admin";
            goItem.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().InitParam(goItem);
            if (goItemAdmin != null && goItemAdmin.displayObject.gameObject != null)
                goItemAdmin.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount--;
            goItemAdmin = goItem;
            goItemAdmin.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount++;
        }

        if (PlayerPrefsUtils.isUseAllConsolePage)
        {
            goMenu.AddChildAt(goItemUser, 5);
            goMenu.AddChildAt(goItemManager, 5);
            goMenu.AddChildAt(goItemAdmin, 5);
        }
        #endregion


        ResetItem(false);
        AddClickEvent();


        CheckAgentIDMachineIDActive();
    }


    void changOnLine( EventData res)
    {
        if (res.name != "SBoxModel/isConnectRemoteControl")
        {
            return;
        }
        var richText = goOwnerMenu.GetChild("remoteCtrlIP").asCom.GetChild("value").asRichTextField;

        // 移除已有的状态文本（适配方括号格式）
        richText.text = richText.text
            .Replace(" [color=#00FF00](已连接)[/color]", "")
            .Replace(" [color=#FF0000](未连接)[/color]", "")
            .Replace(" (已连接)", "")
            .Replace(" (未连接)", "");

        // 根据状态添加新的带颜色文本
        if ((bool)(res.value))
        {
            richText.text += " [color=#00FF00](已连接)[/color]";
        }
        else
        {
            richText.text += " [color=#FF0000](未连接)[/color]";
        }
    }


    public override void OnClickConfirm()
    {
        if (menuMap.ContainsKey(curIndexMenuItem))
        {

            switch (menuMap[curIndexMenuItem])
            {
                case "lineID":
                    {
                        OnClickMachineIDAndAgentID();
                    }
                    return;
                case "machineID":
                    {
                        OnClickMachineIDAndAgentID();
                    }
                    return;
                case "pid":
                    {
                        OnClickSeatId();
                    }
                    return;
                case "groupId":
                    {
                        OnClickGroupId();
                    }
                    return;
                case "active":
                    {
                        OnClickCoder();
                    }
                    return;
                case "user":
                    {
                        OnClickUser(0);
                    }
                    return;
                case "manager":
                    {
                        OnClickUser(1);
                    }
                    return;
                case "admin":
                    {
                        OnClickUser(2);
                    }
                    return;
                case "remoteCtrlEnable":
                    {
                        onClickButtonOnline();
                    }
                    return;
                case "remoteCtrlIP":
                    {
                        OnClickIP();
                    }
                    return;
                case "remoteCtrlAccount":
                    {
                        OnClickAccount();
                    }
                    return;
                case "JPBet":
                    {
                        OnClickJPBet();
                    }
                    return;
                case "BallPerCoin":
                    {
                        OnClickBallPerCoin();
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



    async void OnClickMachineIDAndAgentID()
    {
        EventData res = await PageManager.Instance.OpenPageAsync(PageName.ConsolePusher01PageConsoleSetParameter002,
        new EventData<Dictionary<string, object>>("",
            new Dictionary<string, object>()
            {
                ["title"] = I18nMgr.T("Agent ID") + " & " + I18nMgr.T("Machine ID"),
                ["parameter1"] = I18nMgr.T("Agent ID:"),
                ["parameter1Value"] = SBoxModel.Instance.LineId,
                ["parameter2"] = I18nMgr.T("Machine ID:"),
                ["parameter2Value"] = $"{SBoxModel.Instance.MachineId}",
                ["isNumber"] = true,
            }
        ));

        if (res == null)
            return;
        if (res.value != null)
        {
            bool isErr = true;
            Dictionary<string, object> argDic = res.value as Dictionary<string, object>;

            string lineIdStr = argDic["value1"].ToString();
            string machineIdStr = argDic["value2"].ToString();

            if (machineIdStr.Substring(0, 4) != lineIdStr)
            {
                TipPopupHandler.Instance.OpenPopup(I18nMgr.T("The agent ID must be a 4-digit number"));
                return;
            }

            if (machineIdStr == SBoxModel.Instance.MachineId)
            {
                TipPopupHandler.Instance.OpenPopup(I18nMgr.T("The settings have not changed and do not need to be saved"));
                return;
            }
            try
            {
                int lindId = int.Parse(lineIdStr);

                int machineId = int.Parse(machineIdStr);


                UnityAction OnConfirmModify = () =>
                {
                    MachineDataUtils.RequestSetLineIDMachineID(lindId, machineId,
                    (res) =>
                    {
                        SBoxPermissionsData data = res as SBoxPermissionsData;
                        if (data.result == 0)
                            TipPopupHandler.Instance.OpenPopup(I18nMgr.T("Successfully saved"));
                        else
                            TipPopupHandler.Instance.OpenPopup(I18nMgr.T("Save failed"));

                        //要延时？
                        CheckAgentIDMachineIDActive();
                    },
                    (err) =>
                    {
                        TipPopupHandler.Instance.OpenPopup(I18nMgr.T(err.msg));

                        CheckAgentIDMachineIDActive();
                    });
                };


                /*##
                if (SBoxModel.Instance.isCurAdministrator)
                {
                    OnConfirmModify();
                }
                else
                {
                    CommonPopupHandler.Instance.OpenPopup(new CommonPopupInfo()
                    {
                        // 只能修改一次线号机台号，确定要修改？
                        type = CommonPopupType.YesNo,
                        text = I18nMgr.T("You can only modify the Agent ID and Machine ID once. Are you sure you want to modify it?"),
                        buttonText1 = I18nMgr.T("Cancel"),
                        buttonText2 = I18nMgr.T("OK"),
                        callback1 = null,
                        callback2 = OnConfirmModify,
                    });
                }
                */

                OnConfirmModify();

                isErr = false;
            }
            catch (Exception e)
            {

            }

            if (isErr)
                TipPopupHandler.Instance.OpenPopup(I18nMgr.T("Setting failed"));
        }
    }


    async void OnClickSeatId()
    {
        EventData res = await PageManager.Instance.OpenPageAsync(PageName.ConsolePusher01PageConsoleSetParameter001,
        new EventData<Dictionary<string, object>>("",
            new Dictionary<string, object>()
            {
                ["title"] = I18nMgr.T("SeatId"),
                ["parameter1"] = I18nMgr.T("SeatId:"),
                ["parameter1Value"] = $"{SBoxModel.Instance.seatId}",
                ["isNumber"] = true,
            }
        ));

        if (res == null) return;

        if (res.value != null)
        {   
            bool isErr = true;

            try
            {

                Dictionary<string, object> argDic = res.value as Dictionary<string, object>;


                DebugUtils.LogError($"输入内容 argDic： {JsonConvert.SerializeObject(argDic)}");


                string seatIdStr = argDic["value1"].ToString();

                DebugUtils.LogError($"输入内容： {seatIdStr}");

                int seatId = int.Parse(seatIdStr);

                if (seatId < 0)
                {
                    TipPopupHandler.Instance.OpenPopup(I18nMgr.T("Setting failed")); // 输入不合法
                    return;
                }

                SBoxModel.Instance.seatId = seatId;
                goOwnerMenu.GetChild("pid").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.seatId.ToString();

                isErr = false;
            }
            catch (Exception e)
            {
                DebugUtils.LogError(e.Data);
            }

            if (isErr)
                TipPopupHandler.Instance.OpenPopup(I18nMgr.T("Setting failed"));
        }

    }


    async void OnClickGroupId()
    {
        EventData res = await PageManager.Instance.OpenPageAsync(PageName.ConsolePusher01PageConsoleSetParameter001,
        new EventData<Dictionary<string, object>>("",
            new Dictionary<string, object>()
            {
                ["title"] = I18nMgr.T("Group ID"),
                ["parameter1"] = I18nMgr.T("Group ID:"),
                ["parameter1Value"] = $"{SBoxModel.Instance.groupId}",
                ["isNumber"] = true,
            }
        ));

        if (res == null) return;

        if (res.value != null)
        {
            bool isErr = true;

            try
            {

                Dictionary<string, object> argDic = res.value as Dictionary<string, object>;

                string groupIdStr = argDic["value1"].ToString();

                int groupId = int.Parse(groupIdStr);

                if (groupId < 0)
                {
                    TipPopupHandler.Instance.OpenPopup(I18nMgr.T("Setting failed")); // 输入不合法
                    return;
                }

                SBoxModel.Instance.groupId = groupId;

                goOwnerMenu.GetChild("groupId").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.groupId.ToString();

                isErr = false;
            }
            catch (Exception e)
            {
            }

            if (isErr)
                TipPopupHandler.Instance.OpenPopup(I18nMgr.T("Setting failed"));
        }

    }




    void CheckAgentIDMachineIDActive()
    {

        Action callback = () =>
        {
            goOwnerMenu.GetChild("lineID").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.LineId;
            goOwnerMenu.GetChild("machineID").asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.MachineId;

            if (SBoxModel.Instance.isCurPermissionsAdmin)
            {
                SetItemTouchable("lineID", true);
                SetItemTouchable("machineID", true);
            }
            else
            {
                // SBoxIdea.IsMachineIdReady() 只正对666666、88888888 有效
                SetItemTouchable("lineID", !SBoxIdea.IsMachineIdReady());
                SetItemTouchable("machineID", !SBoxIdea.IsMachineIdReady());
            }
        };

        callback();

        Timers.inst.Add(0.5f, 1, (obj) => { callback(); });
    }


    async void OnClickUser(int n)
    {

        string title;
        string title2;
        switch (n)
        {
            case 1:
                title = I18nMgr.T("Enter Password: Manager");
                title2 = I18nMgr.T("Reset Password: Manager");
                str = "";
                break;
            case 2:
                title = I18nMgr.T("Enter Password: Admin");
                title2 = I18nMgr.T("Reset Password: Admin");
                str = "";
                break;
            default:
                title = I18nMgr.T("Enter Password: Shift");
                title2 = I18nMgr.T("Reset Password: Shift");
                str = "11111111";
                break;
        }
        EventData res = await PageManager.Instance.OpenPageAsync(PageName.ConsolePusher01PageConsoleSetParameter001,
        new EventData<Dictionary<string, object>>("",
            new Dictionary<string, object>()
            {

                ["title"] = title,
                ["title2"] = title2,
                ["parameter1"] = I18nMgr.T("Enter Password"),
                ["parameter1Value"] = str,
            }
        ));

    }



    void onClickButtonOnline()
    {
        SBoxModel.Instance.isUseRemoteControl = !SBoxModel.Instance.isUseRemoteControl;
        goOwnerMenu.GetChildAt(curIndexMenuItem).asCom.GetChild("value").text = SBoxModel.Instance.isUseRemoteControl ? "ON" : "OFF";
        MachineDeviceCommonBiz.Instance.CheckMqttRemoteButtonController();
        // 获取文本组件（建议缓存以提高性能）
        var richText = goOwnerMenu.GetChild("remoteCtrlIP").asCom.GetChild("value").asRichTextField;

        // 移除已有的状态文本（适配方括号格式）
        richText.text = richText.text
            .Replace(" [color=#00FF00](已连接)[/color]", "")
            .Replace(" [color=#FF0000](未连接)[/color]", "")
            .Replace(" (已连接)", "")
            .Replace(" (未连接)", "");

        // 根据状态添加新的带颜色文本
        if (SBoxModel.Instance.isConnectRemoteControl)
        {
            richText.text += " [color=#00FF00](已连接)[/color]";
        }
        else
        {
            richText.text += " [color=#FF0000](未连接)[/color]";
        }
    }
    async void OnClickIP()
    {
        str = SBoxModel.Instance.remoteControlSetting;
        string[] parts = str.Split(':');
        EventData res = await PageManager.Instance.OpenPageAsync(PageName.ConsolePusher01PageConsoleSetParameter002,
        new EventData<Dictionary<string, object>>("",
            new Dictionary<string, object>()
            {
                ["title"] = I18nMgr.T("Configure Remote Server Connection"),
                ["parameter1"] = I18nMgr.T("Server IP Address:"),
                ["parameter1Value"] = parts[0],
                ["parameter2"] = I18nMgr.T("Server Port Number:"),
                ["parameter2Value"] = parts[1],
            }
        ));

        if (res == null)
            return;
        if (res.value != null)
        {
            bool isErr = true;
            Dictionary<string, object> argDic = null;
            try
            {
                argDic = (Dictionary<string, object>)res.value;
                if (argDic.ContainsKey("value1"))
                {
                    if (argDic.ContainsKey("value2"))
                    {
                        SBoxModel.Instance.remoteControlSetting = argDic["value1"] + ":" + argDic["value2"];
                        goOwnerMenu.GetChildAt(curIndexMenuItem).asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.remoteControlSetting;

                    }
                }

                isErr = false;
            }
            catch { }

            //if (isErr) TipPopupHandler.Instance.OpenPopup(string.Format(I18nMgr.T("The {0} must be between {1} and {2}"), I18nMgr.T("Level"), minBet, maxBet));

            if (isErr)
                TipPopupHandler.Instance.OpenPopup(I18nMgr.T("Setting failed"));
        }
    }


    async void OnClickAccount()
    {
        string[] parts = new string[2] { SBoxModel.Instance.remoteControlAccount, SBoxModel.Instance.remoteControlPassword };
        EventData res = await PageManager.Instance.OpenPageAsync(PageName.ConsolePusher01PageConsoleSetParameter002,
        new EventData<Dictionary<string, object>>("",
            new Dictionary<string, object>()
            {
                ["title"] = I18nMgr.T("Set Remote Control Account"),
                ["parameter1"] = I18nMgr.T("Account:"),
                ["parameter1Value"] = parts[0],
                ["parameter2"] = I18nMgr.T("Password:"),
                ["parameter2Value"] = parts[1],
            }
        ));

        if (res == null)
            return;
        if (res.value != null)
        {
            bool isErr = true;
            Dictionary<string, object> argDic = null;
            try
            {
                argDic = (Dictionary<string, object>)res.value;
                if (argDic.ContainsKey("value1"))
                {
                    SBoxModel.Instance.remoteControlAccount = argDic["value1"].ToString();
                    if (argDic.ContainsKey("value2"))
                    {
                        SBoxModel.Instance.remoteControlPassword = argDic["value2"].ToString();
                        goOwnerMenu.GetChildAt(curIndexMenuItem).asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.remoteControlAccount + "/" + SBoxModel.Instance.remoteControlPassword;

                    }
                }




                isErr = false;
            }
            catch { }

            //if (isErr) TipPopupHandler.Instance.OpenPopup(string.Format(I18nMgr.T("The {0} must be between {1} and {2}"), I18nMgr.T("Level"), minBet, maxBet));

            if (isErr)
                TipPopupHandler.Instance.OpenPopup(I18nMgr.T("Setting failed"));
        }
    }



    void OnClickCoder()
    {
        EventCenter.Instance.EventTrigger<EventData>(MachineUIEvent.ON_MACHINE_UI_EVENT,
            new EventData<PageName>(MachineUIEvent.ShowCoding, PageName.ConsolePusher01PageConsoleCoder));
        //new EventData<string>(MachineUIEvent.ShowCoding, "ConsolePusher01PageConsoleCoder"));
    }

    async void OnClickJPBet()
    {
       SBoxModel.Instance.JPBetIndex++;
       if (SBoxModel.Instance.JPBetIndex>6)
       {
            SBoxModel.Instance.JPBetIndex = 0;
           
       }
       goOwnerMenu.GetChildAt(curIndexMenuItem).asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.JPBetIndex.ToString();
    }

    async void OnClickBallPerCoin()
    {
        SBoxModel.Instance.BallPerCoinIndex++;
        if (SBoxModel.Instance.BallPerCoinIndex > 6)
        {
            SBoxModel.Instance.BallPerCoinIndex = 0;
        }
        goOwnerMenu.GetChildAt(curIndexMenuItem).asCom.GetChild("value").asRichTextField.text = SBoxModel.Instance.JPBet[SBoxModel.Instance.BallPerCoinIndex].ToString();

    }

}
