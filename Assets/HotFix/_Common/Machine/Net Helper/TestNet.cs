using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

public class TestNet : MonoBehaviour
{
    public bool IsHost = false;
    void Start()
    {
        NetMgr.Instance.SetNetAutoConnect(IsHost);

        if (IsHost){
            Messenger.AddListener<MsgInfo, WebSockets.ClientConnection>(MessageName.Event_ClientNetworkRecv, OnWSServerData);
        }
        else
        {
            Messenger.AddListener<byte[]>(MessageName.Event_ClientNetworkRecv, OnClientData);        
        }

    }

    void OnDestroy()
    {
        Messenger.RemoveListener<MsgInfo, WebSockets.ClientConnection > (MessageName.Event_ClientNetworkRecv, OnWSServerData);
        Messenger.RemoveListener<byte[]>(MessageName.Event_ClientNetworkRecv, OnClientData);
    }


    
    void OnClientData(byte[] data)
    {
        Debug.Log($"收到的数据： {data}");
    }


    void OnWSServerData(MsgInfo info, WebSockets.ClientConnection client)
    {
        Debug.Log($"收到的数据： {JsonConvert.SerializeObject(info)}  -- {JsonConvert.SerializeObject(client)}");
    }

    [Button]
    void SendDataToSever(string data)
    {
        /*
         * 
        MsgInfo msgInfo = new MsgInfo
        {
            cmd = (int)S2C_CMD.S2C_ConnectFail,
            id = -1,
        };
        NetMgr.Instance.SendToClient(client, JsonConvert.SerializeObject(msgInfo));
        */

        int machineId = int.Parse(SBoxModel.Instance.MachineId);
        LoginInfo loginInfo = new LoginInfo()
        {
            gameType = -1,
            macId = machineId,
        };
        MsgInfo msgInfo00 = new MsgInfo
        {
            cmd = (int)C2S_CMD.C2S_Login,
            id = machineId,
            jsonData = JsonConvert.SerializeObject(loginInfo),
        };
        NetMgr.Instance.SendToServer(JsonConvert.SerializeObject(msgInfo00));

        MsgInfo msgInfo01 = new MsgInfo
        {
            cmd = (int)C2S_CMD.C2S_JackBet,
            id = machineId,
        };
        NetMgr.Instance.SendToServer(JsonConvert.SerializeObject(msgInfo01));
    }


    //  S2C_CMD.S2C_JackpotMinBet,





}
