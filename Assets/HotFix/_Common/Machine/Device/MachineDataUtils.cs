using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using SBoxApi;
using System;
public static class MachineDataUtils
{




    /// <summary>
    /// 设置线号机台号
    /// </summary>
    /// <param name="LineId"></param>
    /// <param name="MachineId"></param>
    public static void RequestSetLineIDMachineID(int LineId, int MachineId, Action<object> successCallback, Action<BagelCodeError> errorCallback)
    {
        string str = JsonConvert.SerializeObject(SBoxModel.Instance.SboxConfData);
        SBoxConfData req = JsonConvert.DeserializeObject<SBoxConfData>(str);
        req.LineId = LineId;
        req.MachineId = MachineId;

        MachineDataManager02.Instance.RequestWriteConf(req, (res) =>
        {
            SBoxPermissionsData data = res as SBoxPermissionsData;
            if (data.result == 0)
            {
                SBoxModel.Instance.SboxConfData.LineId = LineId;
                SBoxModel.Instance.SboxConfData.MachineId = MachineId;
            }
            successCallback?.Invoke(data);
            /**/
            MachineDataManager02.Instance.RequestReadConf((data) =>
            {
                DebugUtils.Log($"!!重新读取的单数 ： {JsonConvert.SerializeObject(data)}");
                SBoxConfData res = (SBoxConfData)data;
                SBoxModel.Instance.SboxConfData = res;
            }, (BagelCodeError err) =>
            {
                DebugUtils.LogError(err.msg);
            });
        }, (BagelCodeError err) =>
        {
            errorCallback?.Invoke(err);
        });
    }




    /// <summary>
    /// 设置投退比例
    /// </summary>
    /// <param name="coinInScale"></param>
    /// <param name="perTicket2Credit"></param>
    /// <param name="perCredit2Ticket"></param>
    /// <param name="scoreUpDownScale"></param>
    /// <param name="successCallback"></param>
    /// <param name="errorCallback"></param>
    public static void RequestSetCoinInCoinOutScale(int? coinInScale, int? perTicket2Credit, int? perCredit2Ticket, int? scoreUpDownScale,
    Action<object> successCallback, Action<BagelCodeError> errorCallback)
    {
        string str = JsonConvert.SerializeObject(SBoxModel.Instance.SboxConfData);
        SBoxConfData req = JsonConvert.DeserializeObject<SBoxConfData>(str);

        if (coinInScale != null)
            req.CoinValue = (int)coinInScale;


        if (perCredit2Ticket != null && (int)perCredit2Ticket >= 1)
        {
            req.scoreTicket = (int)perCredit2Ticket;
            req.TicketValue = 1;
        }
        if (perTicket2Credit != null && (int)perTicket2Credit >= 1)
        {
            req.scoreTicket = 1;
            req.TicketValue = (int)perTicket2Credit;
        }

        if (scoreUpDownScale != null)
        {
            req.ScoreUpUnit = (int)scoreUpDownScale;
        }

        MachineDataManager02.Instance.RequestWriteConf(req, (res) =>
        {
            SBoxPermissionsData data = res as SBoxPermissionsData;

            if (data.result == 0)
            {
                SBoxModel.Instance.SboxConfData.CoinValue = req.CoinValue;
                SBoxModel.Instance.SboxConfData.scoreTicket = req.scoreTicket;
                SBoxModel.Instance.SboxConfData.TicketValue = req.TicketValue;
                SBoxModel.Instance.SboxConfData.ScoreUpUnit = req.ScoreUpUnit;
            }
            successCallback?.Invoke(data);
        }, (BagelCodeError err) =>
        {
            DebugUtils.LogError($"RequestWriteConf : {err}");
            errorCallback?.Invoke(err);
        });
    }

}
