using UnityWebSocket;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

using MyTimer = GameUtil.Timer;
using GameUtil;
using SBoxApi;

public class ClientWS : MonoSingleton<ClientWS>
{
    private WebSocket mSocket;
    private string curLocalIP;

    // Udp相关
    private int mBroadcastPort;
    private UdpClient mUdpClient = null;
    private IPEndPoint endpoint;
    private volatile bool isStop = false;

    public bool GetHost = false;

    private NET_STATUS _curNetStatus = NET_STATUS.NET_STATUS_DISCONNECTED;
    public NET_STATUS CurNetStatus
    {
        get { return _curNetStatus; }
        set
        {
            if (_curNetStatus != value)
            {
                _curNetStatus = value;
                EventCenter.Instance.EventTrigger(EventHandle.NETWORK_STATUS_CHANGE);
            }
        }
    }

    private Thread rcvThread = null;
    private ServerInfo serverinfo;
    public bool canHeart = false;
    public string mAddress;
    public float LastHeartHeatTime = 0.0f;
    public int HeartHeatDelta = 5; // 心跳间隔
    private MyTimer heartHeatTimer;
    private MyTimer checkSrvTimer;

    public void StartUdp(int broadcastPort)
    {
        mBroadcastPort = broadcastPort;
        curLocalIP = Utils.LocalIP();
        mUdpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(curLocalIP), 0));
        endpoint = new IPEndPoint(IPAddress.Broadcast, broadcastPort);
        isStop = false;

        rcvThread = new Thread(ReciveUdpMsg)
        {
            IsBackground = true
        };
        rcvThread.Start();

        if (checkSrvTimer != null)
            checkSrvTimer.Resume();
        else
            checkSrvTimer = MyTimer.LoopAction(3.0f, CheckHostServerInfo);
    }

    public void Reconnect()
    {
        endpoint = new IPEndPoint(IPAddress.Broadcast, mBroadcastPort);
        if (rcvThread == null || !rcvThread.IsAlive)
        {
            isStop = false;
            rcvThread = new Thread(ReciveUdpMsg)
            {
                IsBackground = true
            };
            rcvThread.Start();
        }
        checkSrvTimer?.Resume();
    }

    private void ReciveUdpMsg()
    {
        Debug.LogWarning("ReciveUdpMsg IN");
        while (!isStop && mUdpClient != null)
        {
            try
            {
                Debug.LogWarning("ReciveUdpMsg BeginReceive");
                byte[] buf = mUdpClient.Receive(ref endpoint);
                if (buf != null)
                {
                    string msg = Encoding.UTF8.GetString(buf);
                    Debug.Log($"ReciveUdpMsg:{msg}");

                    if (!string.IsNullOrEmpty(msg) && !GetHost)
                    {
                        serverinfo = JsonConvert.DeserializeObject<ServerInfo>(msg);
                        GetHost = true;
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // 兼容旧逻辑，线程被中止时直接退出
                break;
            }
            catch (SocketException se)
            {
                // 关闭 socket 或网络切换时常见，不当成致命错误
                if (isStop || mUdpClient == null)
                    break;

                Debug.LogWarning($"UDP Receive SocketException: {se.Message}");
            }
            catch (ObjectDisposedException)
            {
                // UdpClient 被关闭，线程正常退出
                break;
            }
            catch (Exception e)
            {
                Debug.LogError($"UDP Receive Exception: {e}");
            }
        }

        isStop = true;
        Debug.LogWarning($"ReciveUdpMsg OUT, isStop = {isStop}, mUdpClient = {mUdpClient}");
    }

    // 使用udp发送消息
    public void SendUpdMsg(string strMsg)
    {
        try
        {
            string localIp = Utils.LocalIP();
            if (mUdpClient != null && curLocalIP == localIp)
            {
                byte[] bf = Encoding.UTF8.GetBytes(strMsg);
                mUdpClient.Send(bf, bf.Length, endpoint);
            }
            else if (curLocalIP != localIp)
            {
                // 停止旧接收线程
                isStop = true;

                // 关闭旧 UdpClient，让接收线程退出
                if (mUdpClient != null)
                {
                    try
                    {
                        mUdpClient.Close();
                    }
                    catch { }
                    mUdpClient = null;
                }

                if (rcvThread != null && rcvThread.IsAlive)
                {
                    try
                    {
                        rcvThread.Join(100);
                    }
                    catch { }
                    rcvThread = null;
                }

                // 获取新 IP 并重建 UdpClient
                curLocalIP = Utils.LocalIP();
                endpoint = new IPEndPoint(IPAddress.Broadcast, mBroadcastPort);
                mUdpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(curLocalIP), 0));
                isStop = false;

                // 重新启动接收线程
                rcvThread = new Thread(ReciveUdpMsg)
                {
                    IsBackground = true
                };
                rcvThread.Start();
            }
        }
        catch (Exception e)
        {
            //PopTips.Instance.ShowTips(Utils.GetLanguage("RebootWaring"));
            //SBoxModel.Instance.rebootFlag = true;
            Debug.LogError($"SendUpdMsg Error:{e.Message}");
        }
    }

    void CheckHostServerInfo(int loopTimes)
    {
        if (_curNetStatus == NET_STATUS.NET_STATUS_DISCONNECTED && serverinfo != null/*&&SBoxModel.Instance.isJackpotOnLine*/)
        {
            InitSocket(serverinfo.IP, serverinfo.port);
            StopUdp();
        }
        else if (serverinfo == null)
        {

            ClientInfo clientInfo = new ClientInfo
            {
                IP = Utils.LocalIP(),
                port = mBroadcastPort,
                macId = SBoxModel.Instance.macId
            };
            SendUpdMsg(JsonConvert.SerializeObject(clientInfo));
        }
    }

    void StopUdp()
    {
        isStop = true;

        // 先关闭 socket，让 Receive 退出阻塞
        if (mUdpClient != null)
        {
            try
            {
                mUdpClient.Close();
            }
            catch { }
            mUdpClient = null;
        }

        // 等待线程短暂退出
        if (rcvThread != null && rcvThread.IsAlive)
        {
            try
            {
                rcvThread.Join(100);
            }
            catch { }
        }

        rcvThread = null;
    }

    public void InitSocket(string server_ip, int port)
    {
        if (mSocket != null)
        {
            mSocket.OnOpen -= SocketOnOpen;
            mSocket.OnMessage -= SocketOnMessage;
            mSocket.OnClose -= SocketOnClose;
            mSocket.OnError -= SocketOnError;
            mSocket.CloseAsync();
            mSocket = null;
        }
        Debug.Log("InitSocket----> ip = " + server_ip + " and port = " + port);
        try
        {
            mAddress = string.Format("ws://{0}:{1}", server_ip, port);
            mSocket = new WebSocket(mAddress);
            mSocket.OnOpen += SocketOnOpen;
            mSocket.OnMessage += SocketOnMessage;
            mSocket.OnClose += SocketOnClose;
            mSocket.OnError += SocketOnError;
            mSocket.ConnectAsync();
            Messenger.Broadcast<int>(MessageName.Event_NetworkErr, 1);

            LastHeartHeatTime = Time.time;
            heartHeatTimer ??= MyTimer.LoopAction(3.0f, ClientHeartHeat);
        }
        catch (Exception ex)
        {
            Debug.LogError($"InitSocket Exception: {ex.Message}");
        }
    }

    // 给服务器发送心跳
    public void SendHeartHeat()
    {
        MsgInfo msgInfo = new MsgInfo
        {
            cmd = (int)C2S_CMD.C2S_JackpotHeartHeat,
            id = int.Parse(SBoxModel.Instance.MachineId)//SBoxModel.Instance.macId
        };
        SendToServer(JsonConvert.SerializeObject(msgInfo));
    }

    // 给服务器发数据
    public void SendToServer(string strData)
    {
        try
        {
            if (mSocket != null && mSocket.ReadyState != WebSocketState.Closed)
            {
                //直接发给服务器了，不需要放进队列里等待发送。
                mSocket.SendAsync(strData);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("发送失败1 " + e.Message);
        }
    }

    // 每3秒发一次心跳
    void ClientHeartHeat(int ck)
    {
        if (canHeart)
        {
            float delta = Time.time - LastHeartHeatTime;
            if (delta > HeartHeatDelta) // 心跳超时了,重新连接服务器
            {
                Debug.LogWarning("心跳超时，重新连接服务器");
                GetHost = false;
                if (CurNetStatus == NET_STATUS.NET_STATUS_CONNECTED)
                {
                    CurNetStatus = NET_STATUS.NET_STATUS_DISCONNECTED;
                    serverinfo = null;
                }
                Reconnect();
            }
            SendHeartHeat();
        }
    }
    private void SocketOnOpen(object sender, OpenEventArgs e)
    {
        Debug.Log(string.Format("Connected: {0}", mAddress));
        CurNetStatus = NET_STATUS.NET_STATUS_CONNECTED;
        canHeart = true;
        SendHeartHeat();
    }

    private void SocketOnMessage(object sender, MessageEventArgs e)
    {
        NetMgr.Instance.SetLastHeartHeat();
        if (_curNetStatus == NET_STATUS.NET_STATUS_DISCONNECTED)
            CurNetStatus = NET_STATUS.NET_STATUS_CONNECTED;
        if (e.IsBinary)
        {
            Debug.Log(string.Format("Receive Bytes ({1}): {0}", e.Data, e.RawData.Length));
        }
        else if (e.IsText)
            Messenger.Broadcast<byte[]>(MessageName.Event_NetworkClientData, Encoding.UTF8.GetBytes(e.Data));
    }

    private void SocketOnClose(object sender, CloseEventArgs e)
    {
        Debug.LogError("call SocketOnClose");
        Debug.Log(string.Format("Closed: StatusCode: {0}, Reason: {1}", e.StatusCode, e.Reason));
        serverinfo = null;
        CurNetStatus = NET_STATUS.NET_STATUS_DISCONNECTED;
        Reconnect();
    }

    private void SocketOnError(object sender, ErrorEventArgs e)
    {
        Debug.LogError("call SocketOnError");
        Debug.Log(string.Format("Error: {0}", e.Message));
        serverinfo = null;
        CurNetStatus = NET_STATUS.NET_STATUS_DISCONNECTED;
        Reconnect();
    }

    public void CloseSocket()
    {
        serverinfo = null;
        GetHost = false;
        CurNetStatus = NET_STATUS.NET_STATUS_DISCONNECTED;
        canHeart = false;
        heartHeatTimer?.Cancel();
        heartHeatTimer = null;
        if (mSocket != null)
        {
            mSocket.OnOpen -= SocketOnOpen;
            mSocket.OnMessage -= SocketOnMessage;
            mSocket.OnClose -= SocketOnClose;
            mSocket.OnError -= SocketOnError;
            mSocket.CloseAsync();
            mSocket = null;
        }

        Debug.Log("CloseSocket");
    }

    /// <summary>
    /// 停止分机网络（关闭WS并停止UDP自动搜索）
    /// </summary>
    public void StopClientNetwork()
    {
        isStop = true;
        checkSrvTimer?.Pause();
        CloseSocket();

        if (mUdpClient != null)
        {
            try
            {
                mUdpClient.Close();
            }
            catch { }
            mUdpClient = null;
        }
    }

    private new void OnDestroy()
    {
        canHeart = false;
        StopUdp();
        MyTimer.CancelAllRegisteredTimers();
        if (mSocket != null)
        {
            mSocket.CloseAsync();
            mSocket = null;
        }

        if (mUdpClient != null)
        {
            mUdpClient.Close();
            mUdpClient = null;
        }
    }
}
