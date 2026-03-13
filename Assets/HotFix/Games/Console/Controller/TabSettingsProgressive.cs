using FairyGUI;
using GameUtil;
using SBoxApi;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TabSettingsProgressive
{
    GComponent goOwnerTab;

    GButton tgJpOlineValidator;
    GLoader Icon;
  
    GRichTextField rtxtJackpotScoreRate,rtxtJackpotPercent;
    public void InitParam(GComponent comp)
    {
        goOwnerTab = comp;
        //联网彩金开关
        tgJpOlineValidator = goOwnerTab.GetChild("useJpOnlineValidator").asCom.GetChild("switch").asButton;
        tgJpOlineValidator.onChanged.Clear();
        tgJpOlineValidator.onChanged.Add(OnChangeIsJackpotOnLine);
        //联网彩金图标
        Icon = goOwnerTab.GetChild("jackpotOnlineIcon").asLoader;
        //分值比
        rtxtJackpotScoreRate = goOwnerTab.GetChild("jackpotScoreRate").asCom.GetChild("value").asRichTextField;
        //分机彩金百分比，每次押分贡献给彩金的比例
        rtxtJackpotPercent = goOwnerTab.GetChild("jackpotPercent").asCom.GetChild("value").asRichTextField;
        
      
        RefreshUI();
        OnClickRequestJackpotData();
        Disable(); //释放掉旧的事件
        Enable();
    }

    public void Enable()
    {
        //EventCenter.Instance.RemoveEventListener(EventHandle.NETWORK_STATUS_CHANGE, OnNetworkStatusChange);
        EventCenter.Instance.AddEventListener(EventHandle.NETWORK_STATUS_CHANGE, OnNetworkStatusChange);
        EventCenter.Instance.AddEventListener<Dictionary<int, List<JackpotDeviceBetData>>>(EventHandle.GET_NET_JACKPOT_DATA, RefreshJackpotTextValue);
        RefreshIconState();
    }
    public void Disable()
    {
        EventCenter.Instance.RemoveEventListener(EventHandle.NETWORK_STATUS_CHANGE, OnNetworkStatusChange);
        EventCenter.Instance.RemoveEventListener<Dictionary<int, List<JackpotDeviceBetData>>>(EventHandle.GET_NET_JACKPOT_DATA, RefreshJackpotTextValue);
    }

    public void OnChangeIsJackpotOnLine(EventContext context)
    {
        GButton toggle = context.sender as GButton;
        if (toggle == null)
            return;

        SBoxModel.Instance.isJackpotOnLine = toggle.selected;
        ApplyJackpotOnlineState(toggle.selected);
        RefreshUI();
    }

    void RefreshUI()
    {
        if (tgJpOlineValidator != null)
            tgJpOlineValidator.selected = SBoxModel.Instance.isJackpotOnLine;

        RefreshIconState();
    }

    void ApplyJackpotOnlineState(bool isOpen)
    {
        if (isOpen)
        {
            NetMgr.Instance.SetNetAutoConnect(false);
        }
        else
        {
            if (ClientWS.Instance != null)
            {
                ClientWS.Instance.StopClientNetwork();
            }
        }
    }

    void OnNetworkStatusChange()
    {
        RefreshIconState();
    }

    void RefreshIconState()
    {
        if (Icon == null)
            return;

        bool isOpen = IsJackpotOnlineOpen();
        bool isConnected = IsJackpotOnlineConnected();
        Icon.visible = isOpen;
        Icon.grayed = isOpen && !isConnected;
    }

    void RefreshJackpotTextValue(Dictionary<int, List<JackpotDeviceBetData>> jackpotData)
    {
        if (rtxtJackpotScoreRate == null || rtxtJackpotPercent == null)
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
                rtxtJackpotScoreRate.text = "--";
                rtxtJackpotPercent.text = "--";
                return;
            }

            rtxtJackpotScoreRate.text = $"{targetData.scoreRate}";
            rtxtJackpotPercent.text = $"{targetData.jpPercent}";
        }
        catch (Exception ex)
        {
            Debug.LogError($"RefreshJackpotTextValue failed: {ex.Message}");
            rtxtJackpotScoreRate.text = "--";
            rtxtJackpotPercent.text = "--";
        }
    }

    bool IsJackpotOnlineOpen()
    {
        return SBoxModel.Instance.isJackpotOnLine;
    }

    bool IsJackpotOnlineConnected()
    {
        return ClientWS.Instance != null &&
               ClientWS.Instance.CurNetStatus == NET_STATUS.NET_STATUS_CONNECTED;
    }

    void OnClickRequestJackpotData()
    {
        NetMessageController.Instance.RequestConsoleJackpotDataOncePerSession();
    }
}
