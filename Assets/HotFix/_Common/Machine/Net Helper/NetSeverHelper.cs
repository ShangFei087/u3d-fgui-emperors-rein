using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GlobalJackpotConsole
{

    public class NetSeverHelper : MonoSingleton<NetSeverHelper>
    {
        void Start()
        {
            //NetMgr.Instance.SetNetAutoConnect(true);
            Messenger.AddListener<MsgInfo, WebSockets.ClientConnection>(MessageName.Event_ServerNetworkRecv, OnWSServerData);
        }

        protected override void OnDestroy()
        {
            Messenger.RemoveListener<MsgInfo, WebSockets.ClientConnection>(MessageName.Event_ServerNetworkRecv, OnWSServerData);

            base.OnDestroy();
        }

        /*
        protected SeverHelper severHelper;

        protected virtual void Awake()
        {
            severHelper = new SeverHelper()
            {
                receiveOvertimeMS = 1000,
                requestFunc = requestFunc,
                isDebug = true,
                prefix = "【Net】",
            };
        }


        string ToString(C2S_CMD data) => Enum.GetName(typeof(C2S_CMD), data);


        /// <summary> 是否激活 </summary>
        public int RequestJackBet(Action<object> successCallback) =>
            severHelper.RequestData(ToString(C2S_CMD.C2S_JackBet), null, successCallback, null);
        void OnResponseJackBet(int code) => severHelper.OnSuccessResponseData(RpcNameIsCodingActive, code);





        public virtual object[] requestFunc(string rpcName, object req)
        {
            object[] resault = new object[] { 0 };
            switch (rpcName)
            {
                case "C2S_JackBet":
                    {

                    }
                    return resault;
            }
        }

        */


        void OnWSServerData(MsgInfo info, WebSockets.ClientConnection client)
        {
            DebugUtils.Log($"收到的数据： {JsonConvert.SerializeObject(info)}  -- {JsonConvert.SerializeObject(client)}");
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
}