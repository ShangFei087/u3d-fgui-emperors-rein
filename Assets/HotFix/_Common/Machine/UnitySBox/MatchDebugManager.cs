using Hal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class MatchHandle
{
    public const string CheckConnect = "CheckConnect";
    public const string Packet = "Packet";
}

public class UdpData
{
    private readonly UdpClient udpClient;
    public UdpClient UdpClient => udpClient;
    private readonly IPEndPoint endPoint;
    public IPEndPoint EndPoint => endPoint;

    public UdpData(IPEndPoint endPoint, UdpClient udpClient)
    {
        this.endPoint = endPoint;
        this.udpClient = udpClient;
    }
}

public class MatchDebugMsg
{
    public string handle;
    public string data;
}

public class CheckConnectMsg
{
    public string ip;
    public int address;
}

public class ConnectState
{
    public int address;
    public bool isConnect;
}

public class MatchDebugManager : MonoSingleton<MatchDebugManager>
{
    private UdpClient udpReceive;
    private IPEndPoint remoteEndPoint;

    private UdpClient udpClient;
    private IPEndPoint sendEndPoint;

    private string mMatchIp;

    private Queue<string> reciveQueue = new Queue<string>();

    public void InitUdpNet(string strIp)
    {
        mMatchIp = strIp;
        ThreadRecive();
    }

    private void Update()
    {
        if (reciveQueue.Count > 0)
        {
            string msg = reciveQueue.Dequeue();
            var matchDebugMsg = JsonConvert.DeserializeObject<MatchDebugMsg>(msg);
            switch (matchDebugMsg.handle)
            {
                case MatchHandle.CheckConnect:
                    var connectState = JsonConvert.DeserializeObject<ConnectState>(matchDebugMsg.data);
                    SBoxIOStream.connectDic[connectState.address] = connectState.isConnect;
                    break;
                case MatchHandle.Packet:
                    var packet = JsonConvert.DeserializeObject<SBoxPacket>(matchDebugMsg.data);
                    SBoxIOEvent.SendEvent(packet.cmd, packet);
                    break;
                default:
                    break;
            }
        }
    }

    private void ThreadRecive()
    {

        if (udpClient != null)
        {
            udpClient.Close();
            udpClient = null;
        }
        sendEndPoint = new IPEndPoint(IPAddress.Parse(mMatchIp), 8098);
        udpClient = new UdpClient();

        if (udpReceive != null)
        {
            udpReceive.Close();
            udpReceive = null;
        }
        remoteEndPoint = new IPEndPoint(IPAddress.Any, 8097);
        udpReceive = new UdpClient(remoteEndPoint);
        udpReceive.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        UdpData data = new UdpData(remoteEndPoint, udpReceive);
        udpReceive.BeginReceive(CallBackRecive, data);
    }

    private void CallBackRecive(IAsyncResult ar)
    {
        if (udpReceive == null) return;
        try
        {
            UdpData state = ar.AsyncState as UdpData;
            IPEndPoint iPEndPoint = state.EndPoint;
            byte[] bytes = udpReceive.EndReceive(ar, ref iPEndPoint);
            reciveQueue.Enqueue(System.Text.Encoding.UTF8.GetString(bytes));
            udpReceive.BeginReceive(CallBackRecive, state);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            throw;
        }
    }

    public void SendUdpMessage(string data)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);

        udpClient?.Send(bytes, bytes.Length, sendEndPoint);
    }

    private void OnDisable()
    {
        if (udpClient != null)
        {
            udpClient.Close(); udpClient = null;
        }

        if (udpReceive != null)
        {
            udpReceive.Close(); udpReceive = null;
        }
    }
}
