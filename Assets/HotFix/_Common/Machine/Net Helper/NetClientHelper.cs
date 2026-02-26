
#define NEW_NET_01





#if NEW_NET_01



using Newtonsoft.Json;
using SBoxApi;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;



namespace GlobalJackpotConsole
{

    /// <summary>
    ///  【即将弃用】后台彩金，客户端的业务逻辑
    /// </summary>
    /// <remarks>
    /// * 处理接收到服务器下行数据
    /// * 添加假数据方便调试
    /// </remarks>
    public class NetClientHelper : MonoSingleton<NetClientHelper>
    {

        void Start()
        {
            //NetMgr.Instance.SetNetAutoConnect(false);
            Messenger.AddListener<byte[]>(MessageName.Event_ClientNetworkRecv, OnRpcDownClientData);

        }

        protected override void OnDestroy()
        {
            Messenger.RemoveListener<byte[]>(MessageName.Event_ClientNetworkRecv, OnRpcDownClientData);

            base.OnDestroy();
        }

        protected SeverHelper severHelper;


        const string Prefix = "【JP Console】";
        protected virtual void Awake()
        {
            severHelper = new SeverHelper()
            {
                receiveOvertimeMS = 1000,
                requestFunc = requestFunc,
                isDebug = true,
                prefix = Prefix,
            };
        }

        public void Update()
        {
            severHelper.Update();
        }



        /// <summary> 即使当前是调试模式,也是用后台彩金的真实数据 </summary>
        public bool isUseReelData
        {
            get
            {
                if (!ApplicationSettings.Instance.isMock)  // if (!Application.isEditor)
                    return true;

                return isUseReelDataWhenMock;
            }
        }
        bool isUseReelDataWhenMock = true;


        /// <summary> 获取彩金数据 </summary>
        public int RequestLogin(LoginInfo req, Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null) =>
            severHelper.RequestData( C2S_CMD.C2S_Login.ToString(),  req, successCallback, errorCallback, mark);
        /// 不同的上下行协议，转成同个key
        void OnResponseLogin(LoginInfoR res) => severHelper.OnSuccessResponseData(C2S_CMD.C2S_Login.ToString(), res);


        /// <summary> 获取后台服务器配置 </summary>
        public int RequestReadConf(RequestBase req, Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null) =>
            severHelper.RequestData(C2S_CMD.C2S_ReadConf.ToString(), req, successCallback, errorCallback, mark);
        void OnResponseReadConf(ReadConfR res) => severHelper.OnSuccessResponseData(C2S_CMD.C2S_ReadConf.ToString(), res);


        /// <summary> 获取彩金数据 </summary>
        public int RequestJackBetMajorGrand(JackBetInfoCoinPush req, Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null) =>
            severHelper.RequestData( C2S_CMD.C2S_JackBet.ToString(), req, successCallback, errorCallback, mark);
        void OnResponseJackBetMajorGrand(JackBetInfoCoinPushR res) => severHelper.OnSuccessResponseData(C2S_CMD.C2S_JackBet.ToString(), res);


        /// <summary> 获取彩金显示数据 </summary>
        public int RequestGetJackpotShowValue(RequestBase req, Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null) =>
            severHelper.RequestData(C2S_CMD.C2S_GetJackpotShowValue.ToString(), req, successCallback, errorCallback, mark);
        void OnResponseGetJackpotShowValue(JackpotGameShowInfoR res) 
            => severHelper.OnSuccessResponseData(C2S_CMD.C2S_GetJackpotShowValue.ToString(), res);





        /*
        /// <summary> 心跳 【暂时不用】</summary>
        public int RequestPing(RequestBaseInfo req, Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null) =>
            severHelper.RequestData(RpcNameHeartHeat, req, successCallback, errorCallback, mark);
        //??
        void OnResponsePing(WinJackpotInfo res) => severHelper.OnSuccessResponseData(RpcNameJackpotMajorGrand, res);
        */


        public virtual object[] requestFunc(string rpcName, object req)
        {
            object[] resault = new object[] { 0 };


            //打开彩金主机：

            if (isUseReelData)
                return requestFunc02(rpcName, req);
            // if (!ApplicationSettings.Instance.isMock ) return requestFunc02(rpcName, req); // 真实数据

            C2S_CMD cmd =  (C2S_CMD)Enum.Parse(typeof(C2S_CMD), rpcName);

            //RequestBaseInfo reqInfo = new RequestBaseInfo();
            switch (cmd)
            {
                case C2S_CMD.C2S_Login: //登录彩金服务器
                    {
                        
                        LoginInfoR jsonData = new LoginInfoR();
                        jsonData.seqId = (req as RequestBase).seqId;

                        MsgInfo msgInfo = new MsgInfo
                        {
                            cmd = (int)S2C_CMD.S2C_LoginR,
                            id = MachineId,
                            jsonData = JsonConvert.SerializeObject(jsonData)
                        };
                        string res = JsonConvert.SerializeObject(msgInfo);
                        byte[] resDtat = Encoding.UTF8.GetBytes(res);
                        Messenger.Broadcast<byte[]>(MessageName.Event_ClientNetworkRecv, resDtat);
                    }
                    return resault;


                case C2S_CMD.C2S_ReadConf:
                    {
                        SBoxConfData mockData = new SBoxConfData();
                        mockData.CoinValue = 99;
                        mockData.scoreTicket = 88;
                        mockData.TicketValue = 77;
                        mockData.MachineId = 22222222;
                        mockData.LineId = 2222;


                        ReadConfR jsonData = new ReadConfR();
                        jsonData.seqId = (req as RequestBase).seqId;
                        jsonData.sboxConfData = mockData;

                        MsgInfo msgInfo = new MsgInfo
                        {
                            cmd = (int)S2C_CMD.S2C_ReadConfR,
                            id = MachineId,
                            jsonData = JsonConvert.SerializeObject(jsonData)
                        };
                        string res = JsonConvert.SerializeObject(msgInfo);
                        byte[] resDtat = Encoding.UTF8.GetBytes(res);
                        Messenger.Broadcast<byte[]>(MessageName.Event_ClientNetworkRecv, resDtat);
                    }
                    return resault;

                case C2S_CMD.C2S_JackBet: //下注
                    {

                        JackBetInfoCoinPush data = req as JackBetInfoCoinPush;
                        int index = UnityEngine.Random.Range(0, 200);
                        int jackpotId = -1;
                        int win = 0;

                        int winGrand = 0;
                        int winMajor = 0;

                        bool isHitGrand = index == 0 || testIsHitJpGrandNext;
                        bool isHitMahor = index == 1 || testIsHitJpMajorNext;

                        if (isHitGrand)
                        {
                            jackpotId = 0;
                            win = data.grandBet * 1;
                            winGrand = win;
                        }

                        if (isHitMahor)
                        {
                            jackpotId = 1;
                            win = data.majorBet * 1;
                            winMajor = win;
                        }

                        testIsHitJpGrandNext = false;
                        testIsHitJpMajorNext = false;

                        testGrandCredit += UnityEngine.Random.Range(5, 20);
                        testMajorCredit += UnityEngine.Random.Range(5, 20);

                        int testGrandCreditOld = isHitGrand ? testGrandCredit : 0;
                        int testMajorCreditOld = isHitMahor ? testMajorCredit : 0;

                        if (isHitGrand || isHitMahor)
                        {
                            testGrandCredit -= winGrand;
                            testMajorCredit -= winMajor;
                        }
                        else
                        {
                            if (testGrandCredit > 4000 || testGrandCredit < 0) testGrandCredit = 3000;
                            if (testMajorCredit > 3000 || testMajorCredit < 0) testMajorCredit = 2000;
                        }


                        SBoxJackpotData sboxJackpotData = new SBoxJackpotData()
                        {
                            result = 0,
                            MachineId = MachineId,
                            SeatId = SBoxModel.Instance.seatId,
                            ScoreRate = 1 * 1000,
                            JpPercent = 1 * 1000,

                            // 0:表示没有开出彩金，1:表示已开出彩金
                            Lottery = new int[] {
                                isHitGrand? 1:0,
                                isHitMahor? 1:0,
                                0,
                                0 },

                            // 开出的彩金注意:此处的单位是钱的单位，而且是乘以了100的，分机收到这个值要根据分机的分值比来转成成对应的分数，而且还要将此值除以100
                            Jackpotlottery = new int[]
                            {
                                winGrand * 100 ,
                                winMajor * 100 ,
                                0 ,
                                0
                            },

                            // 彩金显示积累分,用于显示当前的彩金值
                            JackpotOut = new int[] {
                                testGrandCredit * 100 ,
                                testMajorCredit * 100 ,
                                0,
                                0
                            },

                            // 开出彩金前的显示积累分
                            JackpotOld = new int[] {
                                testGrandCreditOld * 100 ,
                                testMajorCreditOld * 100 ,
                                0,
                                0
                            },

                        };


                        JackBetInfoCoinPushR jsonData = new JackBetInfoCoinPushR();
                        jsonData.seqId = data.seqId;
                        jsonData.sboxJackpotData = sboxJackpotData;


                        MsgInfo msgInfo = new MsgInfo
                        {
                            cmd = (int)S2C_CMD.S2C_JackBetR,
                            id = MachineId,
                            jsonData = JsonConvert.SerializeObject(jsonData)
                        };
                        string res = JsonConvert.SerializeObject(msgInfo);
                        byte[] resDtat = Encoding.UTF8.GetBytes(res);
                        Messenger.Broadcast<byte[]>(MessageName.Event_ClientNetworkRecv, resDtat);

                    }
                    return resault;
                case C2S_CMD.C2S_GetJackpotShowValue:
                    {
                        JackpotGameShowInfoR jsonData = new JackpotGameShowInfoR();
                        jsonData.seqId = (req as RequestBase).seqId;
                        jsonData.curJackpotOut = new int[] {
                            testGrandCredit * 100 ,
                            testMajorCredit * 100 ,
                            0,
                            0
                        };

                        MsgInfo msgInfo = new MsgInfo
                        {
                            cmd = (int)S2C_CMD.S2C_GetJackpotShowValueR,
                            id = MachineId,
                            jsonData = JsonConvert.SerializeObject(jsonData)
                        };
                        string res = JsonConvert.SerializeObject(msgInfo);
                        byte[] resDtat = Encoding.UTF8.GetBytes(res);
                        Messenger.Broadcast<byte[]>(MessageName.Event_ClientNetworkRecv, resDtat); 

                    }
                    return resault;
                case C2S_CMD.C2S_HeartHeat:
                    {
                        // 发送心跳
                        // 模拟接受到心跳
                        //Messenger.Broadcast<byte[]>(MessageName.Event_ClientNetworkRecv, resDtat);
                    }
                    return resault;
            }
            return new object[] { 408, $"【{Prefix} - Mock】没有实现方法：{rpcName}" };
        }

        int testGrandCredit = 3000;
        int testMajorCredit = 2000;

        public bool testIsHitJpGrandNext = false;
        public bool testIsHitJpMajorNext = false;

        int MachineId => int.Parse(SBoxModel.Instance.MachineId);
        public virtual object[] requestFunc02(string rpcName, object req)
        {
            object[] resault = new object[] { 0 };

            C2S_CMD cmd = (C2S_CMD)Enum.Parse(typeof(C2S_CMD), rpcName);

            switch (cmd)
            {
                case C2S_CMD.C2S_HeartHeat:
                    {
                        // 发送心跳
                    }
                    return resault;
                case C2S_CMD.C2S_Login:
                    {
                        /*
                        int machineId = int.Parse(SBoxModel.Instance.MachineId);
                        LoginInfo info = new LoginInfo()
                        {
                            gameType = 1,
                            macId = machineId,
                        };*/
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
                    return resault;

                case C2S_CMD.C2S_ReadConf: //登录彩金服务器
                    {
                        MsgInfo msgInfo = new MsgInfo
                        {
                            cmd = (int)C2S_CMD.C2S_ReadConf,
                            id = MachineId,
                            jsonData = JsonConvert.SerializeObject(req),
                        };
                        NetMgr.Instance.SendToServer(JsonConvert.SerializeObject(msgInfo));
                    }
                    return resault;

                case C2S_CMD.C2S_JackBet: //下注
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
                    return resault;
                case C2S_CMD.C2S_GetJackpotShowValue:
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
                    return resault;
            }

            return new object[] { 408, $"【MachineDataMgr - Real】没有实现方法：{rpcName}" };
        }


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
                        //DebugUtils.Log("接受到心跳包");
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
                        //SBoxConfData res = JsonConvert.DeserializeObject<SBoxConfData>(info.jsonData);
                        OnResponseReadConf(res);
                    }
                    return;
                case S2C_CMD.S2C_JackBetR:
                    {
                        JackBetInfoCoinPushR res = JsonConvert.DeserializeObject<JackBetInfoCoinPushR>(info.jsonData);
                        //SBoxJackpotData res = JsonConvert.DeserializeObject<SBoxJackpotData>(info.jsonData);
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



        /*
        void OnWSServerData(MsgInfo info, WebSockets.ClientConnection client)
        {
            DebugUtils.Log($"收到的数据： {JsonConvert.SerializeObject(info)}  -- {JsonConvert.SerializeObject(client)}");
        }
        */
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


            LoginInfo loginInfo = new LoginInfo()
            {
                gameType = -1,
                macId = MachineId,
            };
            MsgInfo msgInfo00 = new MsgInfo
            {
                cmd = (int)C2S_CMD.C2S_Login,
                id = MachineId,
                jsonData = JsonConvert.SerializeObject(loginInfo),
            };
            NetMgr.Instance.SendToServer(JsonConvert.SerializeObject(msgInfo00));

            MsgInfo msgInfo01 = new MsgInfo
            {
                cmd = (int)C2S_CMD.C2S_JackBet,
                id = MachineId,
            };
            NetMgr.Instance.SendToServer(JsonConvert.SerializeObject(msgInfo01));
        }


        //  S2C_CMD.S2C_LoginR,


    }
}






#else







using Newtonsoft.Json;
using SBoxApi;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;



namespace GlobalJackpotConsole
{

    /// <summary>
    ///  后台彩金，客户端的业务逻辑
    /// </summary>
    /// <remarks>
    /// * 处理接收到服务器下行数据
    /// * 添加假数据方便调试
    /// </remarks>
    public class NetClientHelper : MonoSingleton<NetClientHelper>
    {

        void Start()
        {
            //NetMgr.Instance.SetNetAutoConnect(false);
            Messenger.AddListener<byte[]>(MessageName.Event_ClientNetworkRecv, OnRpcDownClientData);

        }

        protected override void OnDestroy()
        {
            Messenger.RemoveListener<byte[]>(MessageName.Event_ClientNetworkRecv, OnRpcDownClientData);

            base.OnDestroy();   
        }

        protected SeverHelper severHelper;


        const string Prefix = "【JP Console】";
        protected virtual void Awake()
        {
            severHelper = new SeverHelper()
            {
                receiveOvertimeMS = 1000,
                requestFunc = requestFunc,
                isDebug = true,
                prefix = Prefix,
            };
        }

        public void Update()
        {
            severHelper.Update();
        }


        /// <summary>
        /// 不同的上下行协议，转成同个key
        /// </summary>
        const string RpcNameHeartHeat = "RpcNameHeartHeat";
        const string RpcNameLogin = "RpcNameLogin";
        const string RpcNameReadConf = "RpcNameReadConf";
        const string RpcNameJackpotMajorGrand = "RpcNameJackpotMajorGrand";


        /// <summary>
        /// 上下行，必须是同个字符串名！
        /// </summary>
        /// <returns></returns>
        string GetMapRpcUpName(C2S_CMD data)
        {
            switch (data)
            {
                case C2S_CMD.C2S_Login:
                    return RpcNameLogin;
                case C2S_CMD.C2S_JackBet:
                    return RpcNameJackpotMajorGrand;
                case C2S_CMD.C2S_ReadConf:
                    return RpcNameReadConf;
                case C2S_CMD.C2S_HeartHeat:
                    return RpcNameHeartHeat;
            }
            DebugUtils.LogError($"can not find RpcName for {Enum.GetName(typeof(C2S_CMD), data)}");
            return null;
        }
        string GetMapRpcDownName(S2C_CMD data)
        {
            switch (data)
            {
                case S2C_CMD.S2C_LoginR:
                    return RpcNameLogin;
                case S2C_CMD.S2C_JackBetR:
                    return RpcNameJackpotMajorGrand;
                case S2C_CMD.S2C_ReadConfR:
                    return RpcNameReadConf;
                case S2C_CMD.S2C_HeartHeat:
                    return RpcNameHeartHeat;
            }
            DebugUtils.LogError($"can not find RpcName for {Enum.GetName(typeof(S2C_CMD), data)}");
            return null;
        }




        /// <summary> 即使当前是调试模式,也是用后台彩金的真实数据 </summary>
        public bool isUseReelData
        {
            get
            {
                if(!ApplicationSettings.Instance.isMock)  // if (!Application.isEditor)
                    return true;

                return isUseReelDataWhenMock;
            }
        }
        bool isUseReelDataWhenMock = true;


        /// <summary> 获取彩金数据 </summary>
        public int RequestLogin(LoginInfo req, Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null) =>
            severHelper.RequestData(RpcNameLogin, req, successCallback, errorCallback, mark);
        void OnResponseLogin(List<int> res) => severHelper.OnSuccessResponseData(RpcNameLogin, res);


        /// <summary> 获取后台服务器配置 </summary>
        public int RequestReadConf(RequestBaseInfo req,Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null) =>
            severHelper.RequestData(RpcNameReadConf, req, successCallback, errorCallback, mark);
        void OnResponseReadConf(SBoxConfData res) => severHelper.OnSuccessResponseData(RpcNameReadConf, res);


        /// <summary> 获取彩金数据 </summary>
        public int RequestJackMajorGrand(JackBetInfoCoinPush req, Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null)=>
            severHelper.RequestData(RpcNameJackpotMajorGrand, req, successCallback, errorCallback, mark);
        void OnResponseJackMajorGrand(SBoxJackpotData res) => severHelper.OnSuccessResponseData(RpcNameJackpotMajorGrand, res);



        /*
        /// <summary> 心跳 【暂时不用】</summary>
        public int RequestPing(RequestBaseInfo req, Action<object> successCallback, Action<BagelCodeError> errorCallback, string mark = null) =>
            severHelper.RequestData(RpcNameHeartHeat, req, successCallback, errorCallback, mark);
        //??
        void OnResponsePing(WinJackpotInfo res) => severHelper.OnSuccessResponseData(RpcNameJackpotMajorGrand, res);
        */


        public virtual object[] requestFunc(string rpcName, object req)
        {
            object[] resault = new object[] { 0 };

           
             //打开彩金主机：

            if(isUseReelData)
                return requestFunc02(rpcName, req);

            // if (!ApplicationSettings.Instance.isMock ) return requestFunc02(rpcName, req); // 真实数据

            switch (rpcName)
            {
                case RpcNameLogin: //登录彩金服务器
                    {
                        /*
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
                        };*/
     
                        MsgInfo msgInfo = new MsgInfo
                        {
                            cmd = (int)S2C_CMD.S2C_LoginR,
                            id = MachineId,
                            jsonData = JsonConvert.SerializeObject(new List<int>(){ 1,2,3,4})
                        };
                        string res = JsonConvert.SerializeObject(msgInfo);
                        byte[] resDtat = Encoding.UTF8.GetBytes(res);
                        Messenger.Broadcast<byte[]>(MessageName.Event_ClientNetworkRecv, resDtat);
                    }
                    return resault;


                case RpcNameReadConf: 
                    {
                        SBoxConfData mockData = new SBoxConfData();
                        mockData.CoinValue = 99;
                        mockData.scoreTicket = 88;
                        mockData.TicketValue = 77;
                        mockData.MachineId = 22222222;
                        mockData.LineId = 2222;

                        MsgInfo msgInfo = new MsgInfo
                        {
                            cmd = (int)S2C_CMD.S2C_ReadConfR,
                            id = MachineId,
                            jsonData = JsonConvert.SerializeObject(mockData)
                        };
                        string res = JsonConvert.SerializeObject(msgInfo);
                        byte[] resDtat = Encoding.UTF8.GetBytes(res);
                        Messenger.Broadcast<byte[]>(MessageName.Event_ClientNetworkRecv, resDtat);
                    }
                    return resault;

                case RpcNameJackpotMajorGrand: //下注
                    {
                        
                        JackBetInfoCoinPush data = req as JackBetInfoCoinPush;
                        int index = UnityEngine.Random.Range(0, 200);
                        int jackpotId = -1;
                        int win = 0;

                        int winGrand = 0;
                        int winMajor = 0;

                        bool isHitGrand = index == 0 || testIsHitJpGrandNext;
                        bool isHitMahor = index == 1 || testIsHitJpMajorNext;

                        if (isHitGrand)
                        {
                            jackpotId = 0;
                            win = data.grandBet * 1;
                            winGrand = win;
                        }

                        if(isHitMahor)
                        {
                            jackpotId = 1;
                            win = data.majorBet * 1;
                            winMajor = win;
                        }

                        testIsHitJpGrandNext = false;
                        testIsHitJpMajorNext = false;

                        testGrandCredit += UnityEngine.Random.Range(5, 20);
                        testMajorCredit += UnityEngine.Random.Range(5, 20);

                        int testGrandCreditOld = isHitGrand? testGrandCredit : 0;
                        int testMajorCreditOld = isHitMahor? testMajorCredit : 0;

                        if (isHitGrand || isHitMahor)
                        {
                            testGrandCredit -= winGrand;
                            testMajorCredit -= winMajor;
                        }
                        else
                        {
                            if (testGrandCredit > 4000 || testGrandCredit <0) testGrandCredit = 3000;
                            if (testMajorCredit > 3000 || testMajorCredit <0) testMajorCredit = 2000;
                        }


                        SBoxJackpotData data001 = new SBoxJackpotData()
                        {
                            result = 0,
                            MachineId = MachineId,
                            SeatId = SBoxModel.Instance.seatId,
                            ScoreRate = 1 * 1000,
                            JpPercent = 1 * 1000,

                            // 0:表示没有开出彩金，1:表示已开出彩金
                            Lottery =  new int[] {
                                isHitGrand? 1:0,
                                isHitMahor? 1:0, 
                                0,
                                0 },   
                            
                            // 开出的彩金注意:此处的单位是钱的单位，而且是乘以了100的，分机收到这个值要根据分机的分值比来转成成对应的分数，而且还要将此值除以100
                            Jackpotlottery = new int[]
                            {
                                winGrand * 100 ,
                                winMajor * 100 , 
                                0 , 
                                0
                            },	

                            // 彩金显示积累分,用于显示当前的彩金值
                            JackpotOut = new int[] {
                                testGrandCredit * 100 ,
                                testMajorCredit * 100 ,
                                0, 
                                0
                            }, 
                            
                            // 开出彩金前的显示积累分
                            JackpotOld = new int[] {
                                testGrandCreditOld * 100 ,
                                testMajorCreditOld * 100 ,
                                0, 
                                0
                            },                      

                        };

                        MsgInfo msgInfo = new MsgInfo
                        {
                            cmd = (int)S2C_CMD.S2C_JackBetR,
                            id = MachineId,
                            jsonData = JsonConvert.SerializeObject(data001)
                        };
                        string res = JsonConvert.SerializeObject(msgInfo);
                        byte[] resDtat = Encoding.UTF8.GetBytes(res);
                        Messenger.Broadcast<byte[]>(MessageName.Event_ClientNetworkRecv, resDtat);

                    }
                    return resault;
                case RpcNameHeartHeat :
                    {
                        // 发送心跳
                        // 模拟接受到心跳
                        //Messenger.Broadcast<byte[]>(MessageName.Event_ClientNetworkRecv, resDtat);
                    }
                    return resault;
            }
            return new object[] { 408, $"【{Prefix} - Mock】没有实现方法：{rpcName}" };
        }

        int testGrandCredit = 3000;
        int testMajorCredit = 2000;

        public bool testIsHitJpGrandNext = false;
        public bool testIsHitJpMajorNext = false;

        int MachineId => int.Parse(SBoxModel.Instance.MachineId);
        public virtual object[] requestFunc02(string rpcName, object req)
        {
            object[] resault = new object[] { 0 };
            switch (rpcName)
            {
                case RpcNameHeartHeat:
                    {
                       // 发送心跳
                    }
                    return resault;
                case RpcNameLogin:
                    {
                        /*
                        int machineId = int.Parse(SBoxModel.Instance.MachineId);
                        LoginInfo info = new LoginInfo()
                        {
                            gameType = 1,
                            macId = machineId,
                        };*/

                        LoginInfo info = req as LoginInfo;
                        MsgInfo msgInfo = new MsgInfo
                        {
                            cmd = (int)C2S_CMD.C2S_Login,
                            id = MachineId,
                            jsonData = JsonConvert.SerializeObject(info)
                        };
                        NetMgr.Instance.SendToServer(JsonConvert.SerializeObject(msgInfo));
                    }
                    return resault;

                case RpcNameReadConf: //登录彩金服务器
                    {

                        /*
                        ReceiveBaseInfo info = new ReceiveBaseInfo()
                        {
                            gameType = 1,
                        };*/

                        RequestBaseInfo info = req as RequestBaseInfo;

                        MsgInfo msgInfo = new MsgInfo
                        {
                            cmd = (int)C2S_CMD.C2S_ReadConf,
                            id = MachineId,
                            jsonData = JsonConvert.SerializeObject(info),
                        };
                        NetMgr.Instance.SendToServer(JsonConvert.SerializeObject(msgInfo));
                    }
                    return resault;

                case RpcNameJackpotMajorGrand: //下注
                    {
                        /*
                        JackBetInfoCoinPush info = new JackBetInfoCoinPush()
                        {
                            gameType = 1,
                            seat = SBoxModel.Instance.pid,
                            bet = 1 * 100,
                            betPercent = SBoxModel.Instance.CoinInScale * 100,
                            scoreRate = 1 * 1000,
                            JPPercent = 1000,
                            majorBet = 1,
                            grandBet = 1,
                        };
                        */

                        JackBetInfoCoinPush info = req as JackBetInfoCoinPush;

                        MsgInfo msgInfo = new MsgInfo
                        {
                            cmd = (int)C2S_CMD.C2S_JackBet,
                            id = MachineId,
                            jsonData = JsonConvert.SerializeObject(info)
                        };
                        NetMgr.Instance.SendToServer(JsonConvert.SerializeObject(msgInfo));
                    }
                    return resault;
            }

            return new object[] { 408, $"【MachineDataMgr - Real】没有实现方法：{rpcName}" };
        }


        /// <summary>
        /// 服务器下行数据
        /// </summary>
        /// <param name="data"></param>
        void OnRpcDownClientData(byte[] data)
        {
            string result = Encoding.UTF8.GetString(data);
            MsgInfo info = JsonConvert.DeserializeObject<MsgInfo>(result);

            //DebugUtils.LogWarning($"接受到彩金后台的数据：{result}");

            OnDebug(result,false);

            S2C_CMD cmd = (S2C_CMD)info.cmd;

            string rpcName = GetMapRpcDownName(cmd);
            switch (rpcName)
            {

                case RpcNameHeartHeat:
                    {
                        NetMgr.Instance.SetLastHeartHeat();  // 设置心跳时间
                        //DebugUtils.Log("接受到心跳包");
                    }
                    return;
                case RpcNameLogin:
                    {
                        List<int> res = JsonConvert.DeserializeObject<List<int>>(info.jsonData);
                        OnResponseLogin(res);
                    }
                    return;
                case RpcNameReadConf:
                    {
                        SBoxConfData res = JsonConvert.DeserializeObject<SBoxConfData>(info.jsonData);
                        OnResponseReadConf(res);
                    }
                    return;
                case RpcNameJackpotMajorGrand:
                    {
                        SBoxJackpotData res = JsonConvert.DeserializeObject<SBoxJackpotData>(info.jsonData);
                        OnResponseJackMajorGrand(res);
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



        /*
        void OnWSServerData(MsgInfo info, WebSockets.ClientConnection client)
        {
            DebugUtils.Log($"收到的数据： {JsonConvert.SerializeObject(info)}  -- {JsonConvert.SerializeObject(client)}");
        }
        */
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

      
            LoginInfo loginInfo = new LoginInfo()
            {
                gameType = -1,
                macId = MachineId,
            };
            MsgInfo msgInfo00 = new MsgInfo
            {
                cmd = (int)C2S_CMD.C2S_Login,
                id = MachineId,
                jsonData = JsonConvert.SerializeObject(loginInfo),
            };
            NetMgr.Instance.SendToServer(JsonConvert.SerializeObject(msgInfo00));

            MsgInfo msgInfo01 = new MsgInfo
            {
                cmd = (int)C2S_CMD.C2S_JackBet,
                id = MachineId,
            };
            NetMgr.Instance.SendToServer(JsonConvert.SerializeObject(msgInfo01));
        }


        //  S2C_CMD.S2C_LoginR,


    }
}



#endif


