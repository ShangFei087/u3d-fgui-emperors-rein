using GameUtil;
using GameMaker;
using Newtonsoft.Json;
using SBoxApi;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WebSockets;

public class NetMessageController : BaseManager<NetMessageController>
{
    private const float ConsoleJackpotDataRequestTimeoutSeconds = 15f;
    private bool _isInitialized;
    private bool _isConsoleJackpotDataRequestedThisSession;
    private bool _isConsoleJackpotDataRequesting;
    private DelayTimer _consoleJackpotDataTimeoutTimer;
    private Dictionary<int, List<JackpotDeviceBetData>> _cachedConsoleJackpotData;

    public void Init()
    {
        if (_isInitialized)
        {
            Debug.LogWarning("NetMessageController.Init called repeatedly, ignored.");
            return;
        }

        EventCenter.Instance.AddEventListener(EventHandle.NETWORK_STATUS_CHANGE, OnNetworkStatus);
        Messenger.AddListener<MsgInfo, ClientConnection>(MessageName.Event_ServerNetworkRecv, OnServerNetworkRecv);
        Messenger.AddListener<byte[]>(MessageName.Event_ClientNetworkRecv, OnClientNetworkRecv);
        _isInitialized = true;
    }

    public void DeInit()
    {
        if (!_isInitialized)
            return;

        EventCenter.Instance.RemoveEventListener(EventHandle.NETWORK_STATUS_CHANGE, OnNetworkStatus);
        Messenger.RemoveListener<MsgInfo, ClientConnection>(MessageName.Event_ServerNetworkRecv, OnServerNetworkRecv);
        Messenger.RemoveListener<byte[]>(MessageName.Event_ClientNetworkRecv, OnClientNetworkRecv);
        CancelConsoleJackpotDataTimeoutTimer();
        _isConsoleJackpotDataRequesting = false;
        _isInitialized = false;
    }

    private void OnNetworkStatus()
    {
        if (ClientWS.Instance.CurNetStatus == NET_STATUS.NET_STATUS_CONNECTED)
            LoginJackDevice();
    }

    public void LoginJackDevice()
    {
        LoginInfo loginInfo = new LoginInfo()
        {
            gameType = 300,//IOCanvasModel.GameType,
            macId = int.Parse(SBoxModel.Instance.MachineId),//IOCanvasModel.Instance.CfgData.MachineId,
        };

        MsgInfo msgInfo = new MsgInfo()
        {
            cmd = (int)C2S_CMD.C2S_Login,
            id = int.Parse(SBoxModel.Instance.MachineId),//IOCanvasModel.Instance.CfgData.MachineId,
            jsonData = JsonConvert.SerializeObject(loginInfo)
        };
        string msg = JsonConvert.SerializeObject(msgInfo);
        NetMgr.Instance.SendToServer(msg);
    }

    /// <summary>
    /// 下注时向大厅彩金主机发送当前下注
    /// </summary>
    public void SendJackBet(List<JackBetInfo> betInfoList)
    {
        if (betInfoList == null)
            return;

        MsgInfo msgInfo = new MsgInfo()
        {
            cmd = (int)C2S_CMD.C2S_JackBet,
            id = int.Parse(SBoxModel.Instance.MachineId),
            jsonData = JsonConvert.SerializeObject(betInfoList)
        };
        NetMgr.Instance.SendToServer(JsonConvert.SerializeObject(msgInfo));
    }

    /// <summary>
    /// 每次打开管理后台时调用，重置“仅请求一次”的会话状态
    /// </summary>
    public void ResetConsoleJackpotDataRequestSession()
    {
        _isConsoleJackpotDataRequestedThisSession = false;
        _isConsoleJackpotDataRequesting = false;
        _cachedConsoleJackpotData = null;
        CancelConsoleJackpotDataTimeoutTimer();
    }

    /// <summary>
    /// 管理后台请求大厅彩金统计数据（每次打开后台仅请求一次）
    /// </summary>
    public void RequestConsoleJackpotDataOncePerSession()
    {
        if (_isConsoleJackpotDataRequestedThisSession)
        {
            // 已请求过：直接复用缓存，避免重复请求大厅彩金主机
            if (_cachedConsoleJackpotData != null)
            {
                EventCenter.Instance.EventTrigger(EventHandle.GET_NET_JACKPOT_DATA, _cachedConsoleJackpotData);
            }
            return;
        }

        if (ClientWS.Instance == null || ClientWS.Instance.CurNetStatus != NET_STATUS.NET_STATUS_CONNECTED)
        {
            Debug.LogError("RequestConsoleJackpotDataOncePerSession failed: jackpot host not connected.");
            EventCenter.Instance.EventTrigger(EventHandle.GET_NET_JACKPOT_DATA_REQUEST_TIMEOUT);
            return;
        }

        LoginInfo loginInfo = new LoginInfo()
        {
            gameType = 300,
            macId = int.Parse(SBoxModel.Instance.MachineId),
        };

        MsgInfo msgInfo = new MsgInfo
        {
            cmd = (int)C2S_CMD.C2S_GetJackpotData,
            id = int.Parse(SBoxModel.Instance.MachineId),
            jsonData = JsonConvert.SerializeObject(loginInfo)
        };

        _isConsoleJackpotDataRequestedThisSession = true;
        _isConsoleJackpotDataRequesting = true;
        EventCenter.Instance.EventTrigger(EventHandle.GET_NET_JACKPOT_DATA_REQUEST_START);
        StartConsoleJackpotDataTimeoutTimer();

        NetMgr.Instance.SendToServer(JsonConvert.SerializeObject(msgInfo));
    }

    //当本机作为主机时接收到分机句柄的相应
    private void OnServerNetworkRecv(MsgInfo info, ClientConnection client)
    {
        
        switch ((C2S_CMD)info.cmd)
        {
            default:
                Debug.LogError($"未设置 {info.cmd} 响应");
                break;
        }
    }

    private void OnClientNetworkRecv(byte[] data)
    {
        if (data.Length == 0)
            return;

        string singlePacket = Encoding.UTF8.GetString(data);
        //Debug.Log($"raw:{singlePacket}");
        MsgInfo info = JsonConvert.DeserializeObject<MsgInfo>(singlePacket);

        if (info == null)
            return;
        int macId = info.id;
        S2C_CMD netCmd = (S2C_CMD)info.cmd;

        //todo
        //实际使用时用本机机台号替换
        if (macId != -1 &&
            macId != int.Parse(SBoxModel.Instance.MachineId))
            return;

        switch (netCmd)
        {
            case S2C_CMD.S2C_JackpotHearHeat:
                NetMgr.Instance.SetLastHeartHeat();
                break;
            case S2C_CMD.S2C_WinJackpot:
                WinJackpot(info.jsonData);
                break;
            case S2C_CMD.S2C_JackpotError:
                MessageError(info.jsonData);
                break;
            case S2C_CMD.S2C_GetJackpotData:
                OnGetJackpotData(info);
                break;
            case S2C_CMD.S2C_JackpotMinBet:
                //IOCanvasModel.Instance.netJackpotMinBets = JsonConvert.DeserializeObject<List<int>>(info.jsonData);
                //IOCanvasModel.Instance.netJackpotMinBets.Reverse();
                //for (int i = 0; i < IOCanvasModel.Instance.netJackpotMinBets.Count; i++)
                //    IOCanvasModel.Instance.netJackpotMinBets[i] = IOCanvasModel.Instance.netJackpotMinBets[i] / 100;
                break;
            default:
                Debug.LogError($"未设置 {info.cmd} 响应");
                break;
        }
    }

    private void WinJackpot(string jsonData)
    {
        var winJackpotInfo = JsonConvert.DeserializeObject<WinJackpotInfo>(jsonData);
        if (winJackpotInfo == null)
            return;

        Debug.Log($"{winJackpotInfo.macId}号机  {winJackpotInfo.seat}分机 彩金id: {winJackpotInfo.jackpotId}  中奖金额：{winJackpotInfo.win}");

        string jackpotTitle = "";
        switch (winJackpotInfo.jackpotId)
        {
            case 0:
                jackpotTitle = "大彩金";
                break;
            case 1:
                jackpotTitle = "中彩金";
                break;
            case 2:
                jackpotTitle = "小彩金";
                break;
            case 3:
                jackpotTitle = "迷你彩金";
                break;
        }

        winJackpotInfo.win = winJackpotInfo.win / 100 * SBoxModel.Instance.CoinInScale;
        Debug.Log($"{winJackpotInfo.seat}号分机 中!{jackpotTitle}：{float.Parse(winJackpotInfo.win.ToString())} ");

        ReceiveJackpotInfo receiveJackpotInfo = new ReceiveJackpotInfo
        {
            gameType = 300,
            orderId = winJackpotInfo.orderId,
        };

        //回复彩金主机, 已收到中奖信息
        NetMgr.Instance.SendToServer(JsonConvert.SerializeObject(new MsgInfo()
        {
            cmd = (int)C2S_CMD.C2S_ReceiveJackpot,
            id = int.Parse(SBoxModel.Instance.MachineId),
            jsonData = JsonConvert.SerializeObject(receiveJackpotInfo),
        }));

        // 统一转发到业务层事件入口（新事件）
        EventCenter.Instance.EventTrigger<string>(GlobalEvent.JackpotOnlineWin, jsonData);
    }
    private void MessageError(string jsonData)
    {
        var errorInfo = JsonConvert.DeserializeObject<ErrorInfo>(jsonData);
        Debug.Log($"错误码：{errorInfo.errCode}，错误信息：{errorInfo.errString}");
    }

    private void OnGetJackpotData(MsgInfo info)
    {
        CancelConsoleJackpotDataTimeoutTimer();
        _isConsoleJackpotDataRequesting = false;

        try
        {
            var jackpotData = JsonConvert.DeserializeObject<Dictionary<int, List<JackpotDeviceBetData>>>(info.jsonData);
            _cachedConsoleJackpotData = jackpotData;
            EventCenter.Instance.EventTrigger(EventHandle.GET_NET_JACKPOT_DATA, jackpotData);
        }
        catch (Exception ex)
        {
            Debug.LogError($"OnGetJackpotData parse failed: {ex.Message}");
            EventCenter.Instance.EventTrigger(EventHandle.GET_NET_JACKPOT_DATA_REQUEST_TIMEOUT);
        }
    }

    private void StartConsoleJackpotDataTimeoutTimer()
    {
        CancelConsoleJackpotDataTimeoutTimer();
        _consoleJackpotDataTimeoutTimer = Timer.DelayAction(ConsoleJackpotDataRequestTimeoutSeconds, OnConsoleJackpotDataRequestTimeout);
    }

    private void CancelConsoleJackpotDataTimeoutTimer()
    {
        _consoleJackpotDataTimeoutTimer?.Cancel();
        _consoleJackpotDataTimeoutTimer = null;
    }

    private void OnConsoleJackpotDataRequestTimeout()
    {
        if (!_isConsoleJackpotDataRequesting)
            return;

        _isConsoleJackpotDataRequesting = false;
        EventCenter.Instance.EventTrigger(EventHandle.GET_NET_JACKPOT_DATA_REQUEST_TIMEOUT);
    }

}
