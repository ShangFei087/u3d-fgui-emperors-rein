using Newtonsoft.Json;
using System;
using System.Text;

namespace GlobalJackpotConsole
{
    /// <summary>
    /// </summary>
    /// <remarks>
    /// * 简化的封装，让代码更加直观。
    /// </remarks>
    public partial class NetClientHelper02 : ProxyHelper<NetClientHelper02>
    {
        public bool isUseReelData
        {
            get
            {
                if (!ApplicationSettings.Instance.isMock)  // if (!Application.isEditor)
                    return true;

                return isUseReelDataWhenMock;
            }
        }
        bool isUseReelDataWhenMock = false;

        int MachineId => int.Parse(SBoxModel.Instance.MachineId);




        protected void Awake()
        {
            receiveOvertimeS = 1.5f;
            isDebugLog = true;
            prefix = "【JP Console】";
        }

        void Start()
        {
            Messenger.AddListener<byte[]>(MessageName.Event_ClientNetworkRecv, OnRpcDownClientData);
        }

        protected override void OnDestroy()
        {
            Messenger.RemoveListener<byte[]>(MessageName.Event_ClientNetworkRecv, OnRpcDownClientData);
            base.OnDestroy();
        }




        /// <summary> 获取彩金数据 </summary>
        public int RequestLogin(LoginInfo req, Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null)
        {
            int seqId = OnRequestBefore(C2S_CMD.C2S_Login.ToString(), req, successCallback, errorCallback, mark);

            if (!isUseReelData)
            {
                OnMockLogin(req);
            }
            else
            {
                LoginInfo info = req as LoginInfo;
                info.gameType = (int)GameType.CoinPusher;

                MsgInfo msgInfo = new MsgInfo
                {
                    cmd = (int)C2S_CMD.C2S_Login,
                    id = MachineId,
                    jsonData = JsonConvert.SerializeObject(info)
                };
                NetMgr.Instance.SendToServer(JsonConvert.SerializeObject(msgInfo));
            }
            return seqId;
        }
        /// 不同的上下行协议，转成同个key
        void OnResponseLogin(LoginInfoR res) => OnResponse(C2S_CMD.C2S_Login.ToString(), res);


        /// <summary> 获取后台服务器配置 </summary>
        public int RequestReadConf(RequestBase req, Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null)
        {
            int seqId = OnRequestBefore(C2S_CMD.C2S_ReadConf.ToString(), req, successCallback, errorCallback, mark);

            if (!isUseReelData)
            {
                OnMockReadConf(req);
            }
            else
            {
                MsgInfo msgInfo = new MsgInfo
                {
                    cmd = (int)C2S_CMD.C2S_ReadConf,
                    id = MachineId,
                    jsonData = JsonConvert.SerializeObject(req),
                };
                NetMgr.Instance.SendToServer(JsonConvert.SerializeObject(msgInfo));
            }
            return seqId;
        }
        void OnResponseReadConf(ReadConfR res) => OnResponse(C2S_CMD.C2S_ReadConf.ToString(), res);


        /// <summary> 获取彩金数据 </summary>
        public int RequestJackBetMajorGrand(JackBetInfoCoinPush req, Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null)
        {
            int seqId = OnRequestBefore(C2S_CMD.C2S_JackBet.ToString(), req, successCallback, errorCallback, mark);

            if (!isUseReelData)
            {
                OnMockJackBet(req);
            }
            else
            {
                JackBetInfoCoinPush info = req as JackBetInfoCoinPush;
                info.gameType = (int)GameType.CoinPusher;

                MsgInfo msgInfo = new MsgInfo
                {
                    cmd = (int)C2S_CMD.C2S_JackBet,
                    id = MachineId,
                    jsonData = JsonConvert.SerializeObject(info)
                };
                NetMgr.Instance.SendToServer(JsonConvert.SerializeObject(msgInfo));
            }
            return seqId;
        }
        void OnResponseJackBetMajorGrand(JackBetInfoCoinPushR res) => OnResponse(C2S_CMD.C2S_JackBet.ToString(), res);


        /// <summary> 获取彩金显示数据 </summary>
        public int RequestGetJackpotShowValue(RequestBase req, Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null)
        {
            int seqId = OnRequestBefore(C2S_CMD.C2S_GetJackpotShowValue.ToString(), req, successCallback, errorCallback, mark);

            if (!isUseReelData)
            {
                OnMockGetJackpotShowValue(req);
            }
            else
            {
                RequestBase info = req as RequestBase;
                info.gameType = (int)GameType.CoinPusher;

                MsgInfo msgInfo = new MsgInfo
                {
                    cmd = (int)C2S_CMD.C2S_GetJackpotShowValue,
                    id = MachineId,
                    jsonData = JsonConvert.SerializeObject(info)
                };
                NetMgr.Instance.SendToServer(JsonConvert.SerializeObject(msgInfo));
            }
            return seqId;
        }
        void OnResponseGetJackpotShowValue(JackpotGameShowInfoR res) => OnResponse(C2S_CMD.C2S_GetJackpotShowValue.ToString(), res);






        /// <summary>
        /// 服务器下行数据
        /// </summary>
        /// <param name="data"></param>
        void OnRpcDownClientData(byte[] data)
        {
            string result = Encoding.UTF8.GetString(data);
            MsgInfo info = JsonConvert.DeserializeObject<MsgInfo>(result);

            //DebugUtils.LogWarning($"接受到彩金后台的数据：{result}");

            OnDebug(result, false);
            S2C_CMD cmd = (S2C_CMD)info.cmd;

            switch (cmd)
            {

                case S2C_CMD.S2C_HeartHeatR:
                    {
                        NetMgr.Instance.SetLastHeartHeat();  // 设置心跳时间
                    }
                    return;
                case S2C_CMD.S2C_LoginR:
                    {
                        LoginInfoR res = JsonConvert.DeserializeObject<LoginInfoR>(info.jsonData);
                        OnResponseLogin(res);
                    }
                    return;
                case S2C_CMD.S2C_ReadConfR:
                    {
                        ReadConfR res = JsonConvert.DeserializeObject<ReadConfR>(info.jsonData);
                        OnResponseReadConf(res);
                    }
                    return;
                case S2C_CMD.S2C_JackBetR:
                    {
                        JackBetInfoCoinPushR res = JsonConvert.DeserializeObject<JackBetInfoCoinPushR>(info.jsonData);
                        OnResponseJackBetMajorGrand(res);
                    }
                    return;
                case S2C_CMD.S2C_GetJackpotShowValueR:
                    {
                        JackpotGameShowInfoR res = JsonConvert.DeserializeObject<JackpotGameShowInfoR>(info.jsonData);
                        OnResponseGetJackpotShowValue(res);
                    }
                    return;
            }
        }

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

    }
}