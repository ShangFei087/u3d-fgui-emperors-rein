using FairyGUI;
using Mono.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class JackpotOnlineWinsController
{
    GRichTextField rtxtMacId, rtxtSeatId,
                   rtxtBets, rtxtBetPercent,rtxtScoreRate,
                   rtxtJpPercent, rtxtWins,
                   rtxtGrandWin, rtxtGrandTimes,
                   rtxtMajorWin, rtxtMajorTimes,
                   rtxtMinorWin, rtxtMinorTimes,
                   rtxtMiniWin, rtxtMiniTimes;
    public void InitParam(
        GRichTextField _rtxtMacId,
        GRichTextField _rtxtSeatId,
        GRichTextField _rtxtBets,
        GRichTextField _rtxtBetPercent,
        GRichTextField _rtxtScoreRate,
        GRichTextField _rtxtJpPercent,
        GRichTextField _rtxtWins,
        GRichTextField _rtxtGrandWin,
        GRichTextField _rtxtGrandTimes,
        GRichTextField _rtxtMajorWin,
        GRichTextField _rtxtMajorTimes,
        GRichTextField _rtxtMinorWin,
        GRichTextField _rtxtMinorTimes,
        GRichTextField _rtxtMiniWin,
        GRichTextField _rtxtMiniTimes)
    {
        rtxtMacId = _rtxtMacId;
        rtxtSeatId = _rtxtSeatId;
        rtxtBets = _rtxtBets;
        rtxtBetPercent = _rtxtBetPercent;
        rtxtScoreRate = _rtxtScoreRate;
        rtxtJpPercent = _rtxtJpPercent;
        rtxtWins = _rtxtWins;
        rtxtGrandWin = _rtxtGrandWin;
        rtxtGrandTimes = _rtxtGrandTimes;
        rtxtMajorWin = _rtxtMajorWin;
        rtxtMajorTimes = _rtxtMajorTimes;
        rtxtMinorWin = _rtxtMinorWin;
        rtxtMinorTimes = _rtxtMinorTimes;
        rtxtMiniWin = _rtxtMiniWin;
        rtxtMiniTimes = _rtxtMiniTimes;

        NetMessageController.Instance.RequestConsoleJackpotDataOncePerSession();
        ClearAllUI();
        NetMessageController.Instance.RequestConsoleJackpotDataOncePerSession();
        Disable();
        Enable();

    }

    private void ClearAllUI()
    {
        rtxtMacId.text = "0";
        rtxtSeatId.text = "0";
        rtxtBets.text = "0";

        rtxtBetPercent.text = "0";
        rtxtScoreRate.text = "0";
        rtxtJpPercent.text = "0";

        rtxtWins.text = "0";
        rtxtGrandWin.text = "0";
        rtxtGrandTimes.text = "0";
        rtxtMajorWin.text = "0";
        rtxtMajorTimes.text = "0";
        rtxtMinorWin.text = "0";
        rtxtMinorTimes.text = "0";
        rtxtMiniWin.text = "0";
        rtxtMiniTimes.text = "0";
    }


    public void Enable()
    {
        //EventCenter.Instance.RemoveEventListener(EventHandle.NETWORK_STATUS_CHANGE, OnNetworkStatusChange);
        //EventCenter.Instance.AddEventListener(EventHandle.NETWORK_STATUS_CHANGE, OnNetworkStatusChange);
        EventCenter.Instance.AddEventListener<Dictionary<int, List<JackpotDeviceBetData>>>(EventHandle.GET_NET_JACKPOT_DATA, RefreshJackpotTextValue);
    }
    public void Disable()
    {
        //EventCenter.Instance.RemoveEventListener(EventHandle.NETWORK_STATUS_CHANGE, OnNetworkStatusChange);
        EventCenter.Instance.RemoveEventListener<Dictionary<int, List<JackpotDeviceBetData>>>(EventHandle.GET_NET_JACKPOT_DATA, RefreshJackpotTextValue);
    }

    private void RefreshJackpotTextValue(Dictionary<int, List<JackpotDeviceBetData>> jackpotData)
    {
        if (IsComplete())
            return;

        try
        {
            JackpotDeviceBetData targetData = null;
            int machineId = 0;
            int.TryParse(SBoxModel.Instance.MachineId, out machineId);

            if (jackpotData != null && jackpotData.Count > 0)
            {
                if (jackpotData.TryGetValue(machineId, out List<JackpotDeviceBetData> machineDataList) &&
                    machineDataList != null &&
                    machineDataList.Count > 0)
                {
                    targetData = machineDataList[0];
                }
                else
                {
                    foreach (var item in jackpotData)
                    {
                        if (item.Value != null && item.Value.Count > 0)
                        {
                            targetData = item.Value[0];
                            break;
                        }
                    }
                }
            }

            if (targetData == null)
            {
                rtxtMacId.text = "--";
                rtxtSeatId.text = "--";
                rtxtBets.text = "--";
                rtxtBetPercent.text = "--";
                rtxtScoreRate.text = "--";
                rtxtJpPercent.text = "--";
                rtxtWins.text = "--";
                rtxtGrandWin.text = "--";
                rtxtGrandTimes.text = "--";
                rtxtMajorWin.text = "--";
                rtxtMajorTimes.text = "--";
                rtxtMinorWin.text = "--";
                rtxtMinorTimes.text = "--";
                rtxtMiniWin.text = "--";
                rtxtMiniTimes.text = "--";
                return;
            }

            rtxtMacId.text = $"{targetData.macId}";
            rtxtSeatId.text = $"{targetData.seatId}";
            rtxtBets.text = $"{targetData.bet}";
            rtxtBetPercent.text = $"{targetData.betPercent}";
            rtxtScoreRate.text = $"{targetData.scoreRate}";
            rtxtJpPercent.text = $"{targetData.jpPercent}";
            rtxtWins.text = $"{targetData.win}";
            rtxtGrandWin.text = $"{targetData.grandWin}";
            rtxtGrandTimes.text = $"{targetData.grandTimes}";
            rtxtMajorWin.text = $"{targetData.majorWin}";
            rtxtMajorTimes.text = $"{targetData.majorTimes}";
            rtxtMinorWin.text = $"{targetData.miniWin}";
            rtxtMinorTimes.text = $"{targetData.miniTimes}";
            rtxtMiniWin.text = $"{targetData.miniWin}";
            rtxtMiniTimes.text = $"{targetData.miniTimes}";
        }
        catch (Exception ex)
        {
            Debug.LogError($"RefreshJackpotTextValue failed: {ex.Message}");
            rtxtMacId.text = "--";
            rtxtSeatId.text = "--";
            rtxtBets.text = "--";
            rtxtBetPercent.text = "--";
            rtxtScoreRate.text = "--";
            rtxtJpPercent.text = "--";
            rtxtWins.text = "--";
            rtxtGrandWin.text = "--";
            rtxtGrandTimes.text = "--";
            rtxtMajorWin.text = "--";
            rtxtMajorTimes.text = "--";
            rtxtMinorWin.text = "--";
            rtxtMinorTimes.text = "--";
            rtxtMiniWin.text = "--";
            rtxtMiniTimes.text = "--";
        }
    }

    private bool IsComplete()
    {
        return (rtxtMacId == null ||
    rtxtSeatId == null ||
    rtxtBets == null ||
    rtxtBetPercent == null ||
    rtxtScoreRate == null ||
    rtxtJpPercent == null ||
    rtxtWins == null ||
    rtxtGrandWin == null ||
    rtxtGrandTimes == null ||
    rtxtMajorWin == null ||
    rtxtMajorTimes == null ||
    rtxtMinorWin == null ||
    rtxtMinorTimes == null ||
    rtxtMiniWin == null ||
    rtxtMiniTimes == null);
    }
}
