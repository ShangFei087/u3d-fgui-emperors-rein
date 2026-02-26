using FairyGUI;
using GameMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SBoxApi;
using System;

/// <summary>
/// 【这个即将弃用】
/// </summary>
public class TabSettingsInOutController
{
    GComponent owner;


    GButton btnCoinInScale, btnCoinOutScale, btnScoreScale,
            btnFlipScreen,
            btnBillValidatorModel, btnPrinterModel;

    GButton tgBillValidator, tgPrinter;
    public void InitParam(GComponent comp)
    {
        Disable(); //释放掉旧的事件
        Enable();

        owner = comp;


        btnCoinInScale = owner.GetChild("coinInScale").asCom.GetChild("value").asButton;
        btnCoinInScale.onClick.Clear();
        btnCoinInScale.onClick.Add(OnClickCoinInScale);
        btnCoinInScale.title = $"1:{SBoxModel.Instance.CoinInScale}";

        btnCoinOutScale = owner.GetChild("coinOutScale").asCom.GetChild("value").asButton;
        btnCoinOutScale.onClick.Clear();
        btnCoinOutScale.onClick.Add(OnClickCoinOutScale);
        long perCredit2Ticket = SBoxModel.Instance.CoinOutScaleTicketPerCredit;
        long perTicket2Credit = SBoxModel.Instance.CoinOutScaleCreditPerTicket;
        string str = perCredit2Ticket > 1 ? $"{perCredit2Ticket}:1" : $"1:{perTicket2Credit}";
        btnCoinOutScale.title = str;


        btnScoreScale = owner.GetChild("scoreScale").asCom.GetChild("value").asButton;
        btnScoreScale.onClick.Clear();
        btnScoreScale.onClick.Add(OnClickScoreScale);
        btnScoreScale.title = $"1:{SBoxModel.Instance.ScoreUpDownScale}";

        /*
        btnFlipScreen = _comp.GetChild("flipScreen").asCom.GetChild("btn").asButton;
        btnFlipScreen.onClick.Clear();
        btnFlipScreen.onClick.Add(null);
*/


        btnBillValidatorModel = owner.GetChild("billValidatorModel").asCom.GetChild("value").asButton;
        btnBillValidatorModel.onClick.Clear();
        btnBillValidatorModel.onClick.Add(OnClickBillValidatorModel);

        btnPrinterModel = owner.GetChild("printerModel").asCom.GetChild("value").asButton;
        btnPrinterModel.onClick.Clear();
        btnPrinterModel.onClick.Add(OnClickPrinterModel);


        //billValidator
        tgBillValidator = owner.GetChild("useBillValidator").asCom.GetChild("switch").asButton;
        tgBillValidator.onChanged.Clear();
        tgBillValidator.onChanged.Add(OnChangeIsUseBiller);
        tgBillValidator.selected = SBoxModel.Instance.isUseBiller;



        tgPrinter = owner.GetChild("usePrinter").asCom.GetChild("switch").asButton;
        tgPrinter.onChanged.Clear();
        tgPrinter.onChanged.Add(OnChangeIsUsePrinter);
        tgPrinter.selected = SBoxModel.Instance.isUsePrinter;



        OnPropertyChangeIsConnectPrinter();
        OnPropertyChangeIsConnectBiller();
    }


    public void Enable()
    {
        EventCenter.Instance.AddEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, OnPropertyChange);
    }
    public void Disable()
    {
        EventCenter.Instance.RemoveEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, OnPropertyChange);
    }



    void OnPropertyChange(EventData res = null)
    {
        string name = res.name;
        switch (name)
        {
            case "SBoxModel/IsConnectPrinter":
                OnPropertyChangeIsConnectPrinter(res);
                break;
            case "SBoxModel/IsConnectBiller":
                OnPropertyChangeIsConnectBiller(res);
                break;
        }
    }





    async void OnClickCoinOutScale()
    {
        //int curValue = SBoxModel.Instance.CoinOutScaleTicketPerCredit > 1?

        Func<int, string> onChangeUI = (int val) => {
            string str = val < 0 ? $"{-val}:1" : $"1:{val}";

            DebugUtils.Log(str + $"{val}");
            return str;
        };

        int curValue = SBoxModel.Instance.CoinOutScaleTicketPerCredit > 1 ? -SBoxModel.Instance.CoinOutScaleTicketPerCredit :
            SBoxModel.Instance.CoinOutScaleCreditPerTicket;

        EventData res = await PageManager.Instance.OpenPageAsync(PageName.ConsolePopupConsoleSlideSetting,
            new EventData<Dictionary<string, object>>("",
                new Dictionary<string, object>()
                {
                    ["title"] = I18nMgr.T("Coin Out Scale(Ticket : Credit):"),
                    ["valueMin"] = -DefaultSettingsUtils.maxCoinOutTicketPerCredit,//50; // 1分多少票
                    ["valueMax"] = DefaultSettingsUtils.maxCoinOutCreditPerTicket,//200;// 1票多少分
                    ["valueCur"] = curValue, // 1币多少分
                    ["onChangeUI"] = onChangeUI,
                    ["isUseKeyboard"] = false,
                })
        );

        if (res.value != null)
        {
            int data = (int)res.value;
            //DebugUtil.Log($"@@ 1分几票   {data["valueLeft"]};  1票多少分 {data["valueRight"]}");

            int perCredit2Ticket = SBoxModel.Instance.CoinOutScaleTicketPerCredit;
            int perTicket2Credit = SBoxModel.Instance.CoinOutScaleCreditPerTicket;

            if (data < 0)
            {
                perCredit2Ticket = -data;
                perTicket2Credit = 1;
            }
            else
            {
                perCredit2Ticket = 1;
                perTicket2Credit = data;
            }

            if (perCredit2Ticket == SBoxModel.Instance.CoinOutScaleTicketPerCredit
                && perTicket2Credit == SBoxModel.Instance.CoinOutScaleCreditPerTicket)
                return;

            MachineDataUtils.RequestSetCoinInCoinOutScale(null, perTicket2Credit, perCredit2Ticket, null,
            (res01) => {
                SBoxPermissionsData data = res01 as SBoxPermissionsData;
                if (data.result == 0)
                {

                    string str = SBoxModel.Instance.CoinOutScaleTicketPerCredit > 1 ?
                    $"{SBoxModel.Instance.CoinOutScaleTicketPerCredit}:1" : $"1:{SBoxModel.Instance.CoinOutScaleCreditPerTicket}";

                    btnCoinOutScale.title = str;
                }
                else
                    TipPopupHandler.Instance.OpenPopup(I18nMgr.T("Save failed, clear first with a reset code."));
            },
            (err) =>
            {
                TipPopupHandler.Instance.OpenPopup(I18nMgr.T(err.msg));
            });

        }

    }

    async void OnClickCoinInScale()
    {
        EventData res = await PageManager.Instance.OpenPageAsync(PageName.ConsolePopupConsoleSlideSetting,
            new EventData<Dictionary<string, object>>("",
                new Dictionary<string, object>()
                {
                    ["title"] = I18nMgr.T("Coin In Scale(Coin : Credit):"),
                    ["valueMax"] = DefaultSettingsUtils.maxCoinInScale,//200;
                    ["valueMin"] = DefaultSettingsUtils.minCoinInScale,//200;
                    ["valueCur"] = SBoxModel.Instance.CoinInScale, // 1币多少分
                })
        );

        if (res.value != null)
        {
            int data = (int)res.value;

            int coinInScale = data;

            //SBoxModel.Instance.coinInScale = coinInScale;
            //btnCoinInScale.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = $"1:{coinInScale}";

            if (coinInScale == SBoxModel.Instance.CoinInScale) return;

            MachineDataUtils.RequestSetCoinInCoinOutScale(coinInScale, null, null, null,
            (res) => {
                SBoxPermissionsData data = res as SBoxPermissionsData;
                if (data.result == 0)
                {
                    btnCoinInScale.title = $"1:{SBoxModel.Instance.CoinInScale}";
                }
                else
                    TipPopupHandler.Instance.OpenPopup(I18nMgr.T("Save failed, clear first with a reset code."));
            },
            (err) =>
            {
                TipPopupHandler.Instance.OpenPopup(I18nMgr.T(err.msg));
            });
        }

    }

    async void OnClickScoreScale()
    {

        EventData res = await PageManager.Instance.OpenPageAsync(PageName.ConsolePopupConsoleSlideSetting,
            new EventData<Dictionary<string, object>>("",
                new Dictionary<string, object>()
                {
                    ["title"] = I18nMgr.T("Score Scale(Time : Credit):"),
                    ["valueMax"] = DefaultSettingsUtils.maxScoreUpDownScale,
                    ["valueMin"] = DefaultSettingsUtils.minScoreUpDownScale,
                    ["valueCur"] = SBoxModel.Instance.ScoreUpDownScale, // 1次多少分
                })
        );


        if (res.value != null)
        {
            int data = (int)res.value;

            int scoreUpDownScale = data;

            if (scoreUpDownScale == SBoxModel.Instance.ScoreUpDownScale) return;

            MachineDataUtils.RequestSetCoinInCoinOutScale(null, null, null, scoreUpDownScale,
            (res) => {
                SBoxPermissionsData data = res as SBoxPermissionsData;
                if (data.result == 0)
                {
                    btnScoreScale.title = $"1:{SBoxModel.Instance.ScoreUpDownScale}";
                }
                else
                    TipPopupHandler.Instance.OpenPopup(I18nMgr.T("Save failed, clear first with a reset code."));
            },
            (err) =>
            {
                TipPopupHandler.Instance.OpenPopup(I18nMgr.T(err.msg));
            });
        }

    }


    void OnChangeIsUseBiller(EventContext context)
    {
        GButton toggle = context.sender as GButton;

        SBoxModel.Instance.isUseBiller = toggle.selected;

        MachineDeviceCommonBiz.Instance.InitBiller(() => { }, (err) => { });
    }


    void OnChangeIsUsePrinter(EventContext context)
    {
        GButton toggle = context.sender as GButton;

        SBoxModel.Instance.isUsePrinter = toggle.selected;

        MachineDeviceCommonBiz.Instance.InitPrinter(() => { }, (err) => { });
    }


    async void OnClickPrinterModel()
    {

        Dictionary<string, string> selectLst = new Dictionary<string, string>();
        for (int i = 0; i < SBoxModel.Instance.supportPrinters.Count; i++)
        {
            DeviceInfo item = SBoxModel.Instance.supportPrinters[i];
            selectLst.Add(item.number.ToString(), $"{item.manufacturer} : {item.model}");
        }


        Func<string, string> getSelectedDes = (number) =>
        {
            return string.Format("{0} : {1}", I18nMgr.T("Manufacturer"), I18nMgr.T("Model"));
        };


        EventData res = await PageManager.Instance.OpenPageAsync(PageName.ConsolePopupConsoleChoose001,
            new EventData<Dictionary<string, object>>("",
                new Dictionary<string, object>()
                {
                    ["title"] = I18nMgr.T("Choose Printer Model"),
                    ["selectLst"] = selectLst,
                    ["selectNumber"] = $"{SBoxModel.Instance.selectPrinterNumber}",
                    ["getSelectedDes"] = getSelectedDes,
                }));

        if (res.value != null)
        {
            string selectNumber = (string)res.value;
            int number = int.Parse(selectNumber);
            SBoxModel.Instance.selectPrinterNumber = number;
            OnPropertyChangeIsConnectPrinter();

            MachineDeviceCommonBiz.Instance.InitPrinter(() =>
            {
                TipPopupHandler.Instance.OpenPopupOnce(I18nMgr.T("Printer setup successful"));
            }, (err) =>
            {
                TipPopupHandler.Instance.OpenPopupOnce(I18nMgr.T("Printer setup failed"));
            });
        }
    }




    async void OnClickBillValidatorModel()
    {

        Dictionary<string, string> selectLst = new Dictionary<string, string>();
        for (int i = 0; i < SBoxModel.Instance.suppoetBillers.Count; i++)
        {
            DeviceInfo item = SBoxModel.Instance.suppoetBillers[i];
            selectLst.Add(item.number.ToString(), $"{item.manufacturer} : {item.model}");
        }



        Func<string, string> getSelectedDes = (number) =>
        {
            return string.Format("{0} : {1}", I18nMgr.T("Manufacturer"), I18nMgr.T("Model"));
        };

        EventData res = await PageManager.Instance.OpenPageAsync(PageName.ConsolePopupConsoleChoose001,
            new EventData<Dictionary<string, object>>("",
                new Dictionary<string, object>()
                {
                    ["title"] = I18nMgr.T("Choose Bill Validator Model"),
                    ["selectLst"] = selectLst,
                    ["selectNumber"] = $"{SBoxModel.Instance.selectBillerNumber}",
                    ["getSelectedDes"] = getSelectedDes,
                }));

        if (res.value != null)
        {
            string selectNumber = (string)res.value;
            int number = int.Parse(selectNumber);

            SBoxModel.Instance.selectBillerNumber = number;
            OnPropertyChangeIsConnectBiller();

            MachineDeviceCommonBiz.Instance.InitBiller(() =>
            {
                TipPopupHandler.Instance.OpenPopupOnce(I18nMgr.T("Biller setup successful"));
            }, (err) =>
            {
                TipPopupHandler.Instance.OpenPopupOnce(I18nMgr.T("Biller setup failed"));
            });
        }
    }


    void OnPropertyChangeIsConnectPrinter(EventData data = null)
    {
        //Debug.LogError("OnPropertyChangeIsConnectPrinter " + SBoxModel.Instance.isConnectPrinter);
        //[color=#666666]666[/color]99<img src='ui://xp0b9yqdos7y4i' width='20' height='20'/>
        btnPrinterModel.text =
             SBoxModel.Instance.selectPrinterModel + " " +
            (SBoxModel.Instance.IsConnectPrinter ? "<img src='ui://Console/icon_link4aff00' width='20' height='20'/>" : "<img src='ui://Console/icon_link666666' width='20' height='20'/>");
    }


    void OnPropertyChangeIsConnectBiller(EventData data = null)
    {
        //Debug.LogError("OnPropertyChangeIsConnectBiller " + SBoxModel.Instance.isConnectBiller);
        btnBillValidatorModel.text =
            SBoxModel.Instance.selectBillerModel + " " +
            (SBoxModel.Instance.IsConnectBiller ? "<img src='ui://Console/icon_link4aff00' width='20' height='20'/>" : "<img src='ui://Console/icon_link666666' width='20' height='20'/>");
    }


}

//ui://xp0b9yqdos7y4j
// ui://xp0b9yqdos7y4i
// ui://xp0b9yqdos7y4j
// ui://xp0b9yqdos7y3w
// ui://Console/icon_link4aff00
// ui://Console/icon_link666666