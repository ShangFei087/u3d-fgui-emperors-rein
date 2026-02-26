using Newtonsoft.Json;
using System;

//using System.Text.RegularExpressions;
//using System;
//using System.Threading;
using UnityEngine;


/// <summary>
/// 局域网 udp + websocket 链接
/// </summary>
public class NetMgr : MonoSingleton<NetMgr>
{
    private readonly int port = 6222;
    public int broadcastPort = 50122; //10999; //  1220 >> 10122
    private bool IsHost = false;

    //WebSocket
    ServerWS serverWS;
    ClientWS clientWS;


    private void Awake()
    {
        serverWS = this.transform.GetComponent<ServerWS>();
        clientWS = this.transform.GetComponent<ClientWS>();

        Messenger.AddListener<WSSrvMsgData>(MessageName.Event_NetworkWSServerData, OnWSServerData); // 客户端发给服务器的数据
        Messenger.AddListener<byte[]>(MessageName.Event_NetworkClientData, OnClientData);  // 服务器发给客户端的数据
    }

    /// <summary>
    /// 新加的接口，发心跳
    /// </summary>
    /*public void SendHeartHeatToServer()
    {
        if (clientWS != null)
            clientWS.SendHeartHeat();
    }*/

    public void SetLastHeartHeat()
    {
        if (clientWS != null)
            clientWS.LastHeartHeatTime = Time.time;
    }


    /// <summary>
    /// 服务器发给客户端的数据
    /// </summary>
    /// <param name="data"></param>
    void OnClientData(byte[] data)
    {
        Messenger.Broadcast<byte[]>(MessageName.Event_ClientNetworkRecv, data);  // 直接广播出去
    }

    public void SetNetAutoConnect(bool Host)
    {
        //DebugUtils.LogError($"【UDP-WS】  SetNetAutoConnect: Host = {Host}");

        IsHost = Host;

        if (IsHost)  // 主机
        {
            if (serverWS == null)
                serverWS = gameObject.AddComponent<ServerWS>();
            serverWS.StartServer(port, broadcastPort);
        }
        else // 分机
        {
            if (clientWS == null)
                clientWS = gameObject.AddComponent<ClientWS>();
            clientWS.StartUdp(broadcastPort);
        }
    }

    //客户端发送数据给服务器
    public void SendToServer(string strMsg)
    {
        clientWS?.SendToServer(strMsg);

        //OnDebug(strMsg, true);
    }

    //服务器发送数据给客户端
    public void SendToClient(WebSockets.ClientConnection client,string strMsg)
    {
        serverWS?.SendToClient(client, strMsg);

        //OnDebug(strMsg, false);
    }

    //服务器给所有客户端发送消息
    public void SendToAllClient(string strMsg)
    {
        serverWS?.SendToAllClient(strMsg);

        //OnDebug(strMsg,false);
    }

    /*
    public void OnDebug(string strMsg, bool C2S)
    {
        try
        {
            string cmdValue = strMsg.Split(new[] { "\"cmd\":" }, StringSplitOptions.None)[1].Split(',')[0].Trim();
            string rpcName = C2S ?
               $"{Enum.GetName(typeof(C2S_CMD), (C2S_CMD)(int.Parse(cmdValue)))} -" :
                $"{Enum.GetName(typeof(S2C_CMD), (S2C_CMD)(int.Parse(cmdValue)))} -";

            DebugUtils.LogWarning($"【UDP-WS】WS/{rpcName} -  {strMsg}");
        }
        catch (Exception ex) { }
    }
    */



    /// <summary>
    /// 客户端发给服务器的数据,响应函数
    /// </summary>
    /// <param name="data"></param>
    void OnWSServerData(WSSrvMsgData data)
    {
        if (data.Data.Length == 0)
            return;
        string singlePacket = data.Data;
        MsgInfo info = null;
        try
        {
            info = JsonConvert.DeserializeObject<MsgInfo>(singlePacket);
        }
        catch (System.Exception ex)
        {
            DebugUtils.LogError("【UDP-WS】MsgInfo error : " + ex.Message);
            return;
        }
        if (info != null)
        {
            switch ((C2S_CMD)info.cmd)
            {
                case C2S_CMD.C2S_HeartHeat:
                    info.cmd = (int)S2C_CMD.S2C_HeartHeatR;
                    info.id = info.id;
                    SendToClient(data.Client, JsonConvert.SerializeObject(info));  // 心跳统一处理
                    break;
                default:
                    Messenger.Broadcast(MessageName.Event_ServerNetworkRecv, info, data.Client);  // 其他请求广播出去
                    break;
            }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Messenger.RemoveListener<WSSrvMsgData>(MessageName.Event_NetworkWSServerData, OnWSServerData);
        Messenger.RemoveListener<byte[]>(MessageName.Event_NetworkClientData, OnClientData);
    }
}