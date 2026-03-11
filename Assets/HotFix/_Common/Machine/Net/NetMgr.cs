using Newtonsoft.Json;
using UnityEngine;


public class NetMgr : MonoSingleton<NetMgr>
{
    private readonly int port = 33653;
    public int serverBroadcastPort = 10119;
    public int clientBroadcastPort = 10999;

    //WebSocket
    ServerWS serverWS;
    ClientWS clientWS;

    private bool hasBeServer;

    private void Awake()
    {
        serverWS = this.transform.GetComponent<ServerWS>();
        clientWS = this.transform.GetComponent<ClientWS>();

        Messenger.AddListener<WSSrvMsgData>(MessageName.Event_NetworkWSServerData, OnWSServerData);
        Messenger.AddListener<byte[]>(MessageName.Event_NetworkClientData, OnClientData);
    }


    public void SetLastHeartHeat()
    {
        if (clientWS != null)
            clientWS.LastHeartHeatTime = Time.time;
    }

    void OnClientData(byte[] data)
    {
        Messenger.Broadcast<byte[]>(MessageName.Event_ClientNetworkRecv, data);
    }

    public void SetNetAutoConnect(bool Host)
    {
        if (Host)
        {
            if (!hasBeServer)
            {
                if (serverWS == null)
                    serverWS = gameObject.AddComponent<ServerWS>();
                serverWS.StartServer(port, serverBroadcastPort);
                hasBeServer = true;
            }
        }
        else
        {
            if (clientWS == null)
                clientWS = gameObject.AddComponent<ClientWS>();
            clientWS.StartUdp(clientBroadcastPort);
        }
    }

    //客户端发送数据给服务器
    public void SendToServer(string strMsg)
    {
        clientWS?.SendToServer(strMsg);
    }

    //服务器发送数据给客户端
    public void SendToClient(WebSockets.ClientConnection client,string strMsg)
    {
        serverWS?.SendToClient(client, strMsg);
    }

    //服务器给所有客户端发送消息
    public void SendToAllClient(string strMsg)
    {
        serverWS?.SendToAllClient(strMsg);
    }

    //处理WS服务器收到的消息
    void OnWSServerData(WSSrvMsgData data)
    {
        if (data.Data.Length == 0)
        {
            Debug.LogError("OnWSServerData: data.Data.Length == 0");
            return;
        }
        string singlePacket = data.Data;
        MsgInfo info = null;
        try
        {
            info = JsonConvert.DeserializeObject<MsgInfo>(singlePacket);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("JSON : " + ex.Message);
        }
        if (info != null)
        {
            switch ((C2S_CMD)info.cmd)
            {
                case C2S_CMD.C2S_JackpotHeartHeat:
                    info.cmd = (int)S2C_CMD.S2C_JackpotHearHeat;
                    info.id = info.id;
                    SendToClient(data.Client, JsonConvert.SerializeObject(info));
                    break;
                default:
                    Messenger.Broadcast(MessageName.Event_ServerNetworkRecv, info, data.Client);
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