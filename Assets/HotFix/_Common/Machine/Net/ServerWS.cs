using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using UnityEngine;


public class ServerWS : MonoBehaviour
{
    private WebSockets.WebSocketServer mServer;

    //主机相关
    private UdpClient mUdpclient = null; //主机和分机的udpclient
    private IPEndPoint endpoint;
    ServerInfo serverinfo;

    private bool IsStop = false;
    private Thread RcvThread = null;



    /// <summary>
    /// 开启udp 和 websocket
    /// </summary>
    /// <param name="port"></param>
    /// <param name="broadcastPort"></param>
    public void StartServer(int port, int broadcastPort)
    {
        DebugUtils.Log($"【UDP-WS】StartServer;  port:{port}  broadcastPort:{broadcastPort}");
        serverinfo = new ServerInfo();
        serverinfo.IP = Utils.LocalIP();
        serverinfo.port = port;
        StartUdp(broadcastPort);
        InitSocket(port);
    }

    public void StopServer()
    {
        if(mServer != null)
        {
            mServer.Stop();
            mServer = null;
        }
    }


    /// <summary>
    /// 开启udp
    /// </summary>
    /// <param name="broadcastPort"></param>
    protected void StartUdp(int broadcastPort)
    {
        mUdpclient = new UdpClient(new IPEndPoint(IPAddress.Any, broadcastPort));
        endpoint = new IPEndPoint(IPAddress.Any, 0);
        IsStop = false;
        RcvThread = new Thread(new ThreadStart(ReciveUdpMsg))
        {
            IsBackground = true
        };
        RcvThread.Start();
    }


    /// <summary>
    /// 开启websocket
    /// </summary>
    /// <param name="port"></param>
    public void InitSocket(int port)
    {
        StopServer();
        mServer = new WebSockets.WebSocketServer(IPAddress.Any,port);
        mServer.OnClientConnected += OnClientConnected;
        mServer.Start();
    }


    /// <summary>
    /// 当服务器收到客户端udp数据（请求ip地址和端口）
    /// </summary>
    private void ReciveUdpMsg()
    {
        while (!IsStop && mUdpclient != null)
        {
            //IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] buf = mUdpclient.Receive(ref endpoint);
            if (buf != null)
            {
                string msg = Encoding.UTF8.GetString(buf);
                DebugUtils.Log($"【UDP-WS】ReciveUdpMsg(C2S): {msg}");
                if (!string.IsNullOrEmpty(msg))
                {
                    ServerInfo srvInfo = new ServerInfo
                    {
                        IP = serverinfo.IP,
                        port = serverinfo.port
                    };
                    SendUpdMsg(JsonConvert.SerializeObject(srvInfo), endpoint);
                }
            }
            Thread.Sleep(500);
        }
    }

    /// <summary>
    /// 使用udp发送消息
    /// </summary>
    /// <param name="strMsg"></param>
    /// <param name="endPoint"></param>
    public void SendUpdMsg(string strMsg, IPEndPoint endPoint)
    {
        if (mUdpclient != null)
        {
            byte[] bf = Encoding.UTF8.GetBytes(strMsg);
            mUdpclient.Send(bf, bf.Length, endPoint);
        }
    }


    /// <summary>
    /// 当websocket连接成功
    /// </summary>
    /// <param name="client"></param>
    private void OnClientConnected(WebSockets.ClientConnection client)
    {
        client.ReceivedTextualData += OnReceivedTextualData;
        client.Disconnected += OnClientDisconnected;
        client.StartReceiving();

        DebugUtils.Log(string.Format("【UDP-WS】Client {0} Connected...", client.Id));
    }
    /// <summary>
    /// 当websocket关闭
    /// </summary>
    /// <param name="client"></param>
    private void OnClientDisconnected(WebSockets.ClientConnection client)
    {
        client.ReceivedTextualData -= OnReceivedTextualData;
        client.Disconnected -= OnClientDisconnected;
        DebugUtils.Log(string.Format("【UDP-WS】Client {0} Disconnected...", client.Id));
        EventCenter.Instance.EventTrigger(EventHandle.PLAYER_DISCONNECT, client);
    }

    /// <summary>
    /// 当收到websocket数据
    /// </summary>
    /// <param name="client"></param>
    /// <param name="data"></param>
    private void OnReceivedTextualData(WebSockets.ClientConnection client, string data)
    {
        WSSrvMsgData wmd = new WSSrvMsgData
        {
            Client = client,
            Data = data
        };
        Loom.QueueOnMainThread((wmd) =>
        {
            Messenger.Broadcast<WSSrvMsgData>(MessageName.Event_NetworkWSServerData, (WSSrvMsgData)wmd);
            wmd = null;
        }, wmd);
    }


    /// <summary>
    /// 走websocket发送数据给客户端
    /// </summary>
    /// <param name="client"></param>
    /// <param name="msg"></param>
    public void SendToClient(WebSockets.ClientConnection client,string msg)
    {
        client.Send(msg);
        OnDebug(msg, false);
    }


    /// <summary>
    /// 走websocket发送数据给所有客户端
    /// </summary>
    /// <param name="client"></param>
    /// <param name="msg"></param>
    public void SendToAllClient(string msg)
    {
        if(mServer != null)
        {
            mServer.SendToAllClient(msg);
            OnDebug(msg, false);
        }
    }




    private void OnDestroy()
    {
        IsStop = true;
        if (RcvThread != null)
        {
            RcvThread.Abort();
            RcvThread = null;
        }
        // StopCoroutine(CheckHostServerInfo(3.0f));
        if (mUdpclient != null)
        {
            mUdpclient.Close();
            mUdpclient = null;
        }
        StopServer();
    }

    public void OnDebug(string strMsg, bool C2S = true)
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

}
