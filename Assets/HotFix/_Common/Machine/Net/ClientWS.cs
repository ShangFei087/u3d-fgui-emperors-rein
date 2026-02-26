using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityWebSocket;
using MyTimer = GameUtil.Timer;

public class ClientWS : MonoSingleton<ClientWS>
{
    WebSocket mSocket;

    //Udp相关
    private int mBroadcastPort;
    private UdpClient mUdpclient = null; //主机和分机的udpclient
    private IPEndPoint endpoint;
    // ServerInfo strSrvInfo = JsonConvert.SerializeObject(new ServerInfo());

    private bool IsStop = false;

    public bool GetHost = false;

    public bool IsConnected { get; set; }

    private Thread RcvThread = null;
    ServerInfo serverinfo;
    // 是否开始心跳
    public bool canHeart = false;
    public string mAddress;
    public float LastHeartHeatTime = 0.0f;
    public int HeartHeatDelta = 10; //心跳间隔
    private MyTimer heartHeatTimer;
    private MyTimer checkSrvTimer;

    private void Start()
    {

        IsConnected = false;
    }

    void SetServerInfo(ServerInfo si)
    {
        serverinfo = si;
    }

    public void StartUdp(int broadcastPort)
    {
        DebugUtils.LogWarning($"【UDP-WS】StartClinet;  broadcastPort:{broadcastPort}");


        // udp初始化
        mBroadcastPort = broadcastPort;
        mUdpclient = new UdpClient(new IPEndPoint(IPAddress.Parse(Utils.LocalIP()), 0));
        endpoint = new IPEndPoint(IPAddress.Broadcast, broadcastPort);
        IsStop = false;


        /*#seaweed#
        // 循环监听udp数据
        RcvThread = new Thread(new ThreadStart(ReciveUdpMsg))
        {
            IsBackground = true 
        };
        RcvThread.Start();
        */

        // 开启子线程，获取udp数据
        StartThreadReciveUdpMsg();


        // 定时走udp请求服务器的"IP"和"端口"
        if (checkSrvTimer != null)
        {
            checkSrvTimer.Resume();
        }
        else
        {
            // 循环走udp请求服务器的ws地址和端口
            checkSrvTimer = MyTimer.LoopAction(4.0f, CheckHostServerInfo); //3f
        }

    }


    /// <summary>
    /// 恢复线程 恢复定时器
    /// </summary>
    public void Reconnect()
    {

        DebugUtils.Log("【UDP-WS】Reconnect");

        //mUdpclient.Close();
        //mUdpclient = new UdpClient(new IPEndPoint(IPAddress.Parse(Utils.LocalIP()), 0));
        //endpoint = new IPEndPoint(IPAddress.Broadcast, mBroadcastPort);
#pragma warning disable CS0618 // Type or member is obsolete
        /*#seaweed#
        RcvThread?.Resume(); // 恢复线程监听udp信息
        */
#pragma warning restore CS0618 // Type or member is obsolete


        StartThreadReciveUdpMsg();
        checkSrvTimer?.Resume(); //恢复定时器，循环发送udp或连接websocket

    }







    /// <summary>
    /// 接受到主机udp数据
    /// </summary>
    private void ReciveUdpMsg(CancellationToken cancellationToken) //#seaweed
    {
        DebugUtils.LogWarning("【UDP-WS】ReciveUdpMsg IN");
        while (!IsStop && mUdpclient != null && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                byte[] buf = mUdpclient.Receive(ref endpoint);
                if (buf != null)
                {
                    string msg = Encoding.UTF8.GetString(buf);

                    DebugUtils.Log($"【UDP-WS】UDP/ReciveUdpMsg(S2C): {msg}");

                    if (!string.IsNullOrEmpty(msg) && !GetHost)
                    {
                        DebugUtils.Log($"【UDP-WS】UDP/S2C <color=green>GetHost</color>");

                        serverinfo = JsonConvert.DeserializeObject<ServerInfo>(msg);
                        GetHost = true;
                    }
                }
            }
            catch (Exception e)
            {
                DebugUtils.Log("【UDP-WS】"+ e.Message);
            }
        }
        DebugUtils.LogWarning("【UDP-WS】ReciveUdpMsg OUT");
    }


     //解决子线程失效的问题
    CancellationTokenSource _cts;
    void StartThreadReciveUdpMsg()
    {
        // 关闭上个线程
        _cts?.Cancel();


        // 初始化取消标记源
        _cts = new CancellationTokenSource();
        CancellationToken cancellationToken = _cts.Token;
        // 循环监听udp数据
        RcvThread = new Thread(() => ReciveUdpMsg(cancellationToken))
        {
            IsBackground = true
        };
        RcvThread.Start();
    }






    //使用udp发送消息
    public void SendUpdMsg(string strMsg)
    {

        //#seaweed# 网络重连

        try
        {
            if (mUdpclient != null)
            {
                byte[] bf = Encoding.UTF8.GetBytes(strMsg);
                mUdpclient.Send(bf, bf.Length, endpoint);
            }
        }
        catch (Exception ex)
        {

            throw ex;
        }
    }


    /// <summary>
    /// 走udp定时请求服务器"IP"和"端口"
    /// </summary>
    /// <param name="loopTimes"></param>
    void CheckHostServerInfo(int loopTimes)
    {

        //DebugUtils.Log($"【UDP-WS】 IsConnected: {IsConnected} ; serverinfo is null: {serverinfo == null}  ;  GetHost：{GetHost}");

        if (!IsConnected && serverinfo != null) // 没有链接websocket重新连接
        {
            DebugUtils.Log("【UDP-WS】<color=green>Init Socket</color> ");
            // 获取到端口和地址，建立ws连接
            InitSocket(serverinfo.IP, serverinfo.port);
            StopUdp();  // 【bug】如果一开始彩金已连接，之后彩金断开重连。能知道彩金的"IP"和"端口"（此时udp已经被关闭！）
        }
        else if (serverinfo == null) // 获取服务器ip和端口
        {
            string msg = "";
            try
            {
                ServerInfo clientInfo = new ServerInfo
                {
                    IP = Utils.LocalIP(), // 发送本机ip
                    port = mBroadcastPort
                };
                msg = JsonConvert.SerializeObject(clientInfo);
                DebugUtils.LogWarning($"【UDP-WS】UDP/C2S : {msg} ");
                SendUpdMsg(msg);
            }
            catch (Exception ex)
            {
                DebugUtils.Log($"【UDP-WS】UDP/C2S  send udp msg fail  : {msg}  ");
                // 这里可能因为断网导致一直报错。 直接过滤掉
            }
        }
    }

    void StopUdp()
    {
#pragma warning disable CS0618 // 类型或成员已过时
        RcvThread?.Suspend();
#pragma warning restore CS0618 // 类型或成员已过时
    }


    /// <summary>
    /// 连接websocket
    /// </summary>
    /// <param name="server_ip"></param>
    /// <param name="port"></param>
    /// <remarks>
    /// * 如果断线，链接失败，回重连？？
    /// </remarks>
    public void InitSocket(string server_ip, int port)
    {
        DebugUtils.Log("【UDP-WS】InitSocket----> ip = " + server_ip + " and port = " + port);
        if (mSocket != null)
        {
            mSocket.OnOpen -= SocketOnOpen;
            mSocket.OnMessage -= SocketOnMessage;
            mSocket.OnClose -= SocketOnClose;
            mSocket.OnError -= SocketOnError;
            mSocket.CloseAsync();
            mSocket = null;
            //StopCoroutine(ClientHeartHeat());
        }
        try
        {
            mAddress = string.Format("ws://{0}:{1}", server_ip, port);
            mSocket = new WebSocket(mAddress);
            mSocket.OnOpen += SocketOnOpen;  // 建立连接
            mSocket.OnMessage += SocketOnMessage;   //获取数据
            mSocket.OnClose += SocketOnClose;
            mSocket.OnError += SocketOnError;
            //mSocket.BinaryType = 
            mSocket.ConnectAsync();
            Messenger.Broadcast<int>(MessageName.Event_NetworkErr, 1);

            LastHeartHeatTime = Time.time;
            if (heartHeatTimer == null)
            {
                heartHeatTimer = MyTimer.LoopAction(2.8f, ClientHeartHeat);
            }
            else
            {
                heartHeatTimer.Resume(); //#seaweed# 新加
            }
        }
        catch (System.Exception ex)
        {
            DebugUtils.Log(ex.Message);
        }
    }

    //给服务器发送心跳
    public void SendHeartHeat()
    {
        MsgInfo cmd = new MsgInfo();
        cmd.cmd = (int)C2S_CMD.C2S_HeartHeat;
        //cmd.id = Model.Instance.macId;
        SendToServer(JsonConvert.SerializeObject(cmd));
    }

    //给服务器发数据
    public void SendToServer(string strData)
    {
        try
        {
            if (mSocket != null && mSocket.ReadyState != WebSocketState.Closed)
            {
                //直接发给服务器了，不需要放进队列里等待发送。
                mSocket.SendAsync(strData);

                OnDebug(strData,true);
            }

        }
        catch (Exception e)
        {
            //mClientSocket.Close();
            DebugUtils.Log("【UDP-WS】发送失败  " + e.Message);
        }
    }


    /// <summary>
    /// 走websocket 给服务器发送心跳
    /// </summary>
    /// <param name="ck"></param>
    void ClientHeartHeat(int ck)
    {
        if (canHeart)
        {
            float delta = Time.time - LastHeartHeatTime;
            if (delta > HeartHeatDelta) //心跳超时了,重新连接服务器
            {
                GetHost = false;
                IsConnected = false;
                serverinfo = null;
                DebugUtils.LogWarning("【UDP-WS】Heartbeat Lost");
                Reconnect();

                heartHeatTimer.Pause(); //#seaweed# 新加 (方法1： 可以用)

                //heartHeatTimer.Cancel(); //#seaweed# 新加(方法2： 可以用)
                //heartHeatTimer = null;
            }
            else //#seaweed# 新加
            {
                SendHeartHeat();
            }
        }
    }

    private void SocketOnOpen(object sender, OpenEventArgs e)
    {
        DebugUtils.Log(string.Format("【UDP-WS】Connected: {0}", mAddress));
        IsConnected = true;
        canHeart = true;
        SendHeartHeat();
        //## PopTips.Instance.ShowSystemTips("Connected");
        //DebugUtils.LogWarning("【UDP-WS】Connected");

        // 这里进行登录
        NetClineBiz.Instance.CheckLoginJpConsole();
    }

    private void SocketOnMessage(object sender, MessageEventArgs e)
    {
        if (e.IsBinary)
        {
            DebugUtils.Log(string.Format("【UDP-WS】Receive Bytes ({1}): {0}", e.Data, e.RawData.Length));
        }
        else if (e.IsText)
        {
            //DebugUtils.Log(string.Format("Receive: {0}", e.Data));
            //TODO 添加消息处理
            Messenger.Broadcast<byte[]>(MessageName.Event_NetworkClientData, Encoding.UTF8.GetBytes(e.Data));
        }
    }

    private void SocketOnClose(object sender, CloseEventArgs e)
    {
        DebugUtils.Log(string.Format("【UDP-WS】WS/ On Closed; StatusCode: {0}, Reason: {1}", e.StatusCode, e.Reason));
        serverinfo = null;
        IsConnected = false;
        GetHost = false;


        NetClineBiz.Instance.Clear();

        Reconnect();


    }

    private void SocketOnError(object sender, ErrorEventArgs e)
    {
        DebugUtils.Log(string.Format("【UDP-WS】WS/ On Error; {0}", e.Message));
        serverinfo = null;
        IsConnected = false;
        GetHost = false;

        NetClineBiz.Instance.Clear();

        Reconnect();
    }

    public void CloseSocket()
    {   
        DebugUtils.Log("【UDP-WS】CloseSocket");

        serverinfo = null;
        GetHost = false;
        IsConnected = false;
        canHeart = false;
        heartHeatTimer?.Cancel();
        if (mSocket != null)
        {
            mSocket.OnOpen -= SocketOnOpen;
            mSocket.OnMessage -= SocketOnMessage;
            mSocket.OnClose -= SocketOnClose;
            mSocket.OnError -= SocketOnError;
            mSocket.CloseAsync();
            mSocket = null;
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

        if (mUdpclient != null)
        {
            mUdpclient.Close();
            mUdpclient = null;
        }
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