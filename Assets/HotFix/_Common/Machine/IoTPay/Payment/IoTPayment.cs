using M2MqttUnity;
using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine.Events;

namespace IOT
{
    /*
     * 设备生成在阿里云物联网平台的企业实例中，对应mqtt链接域名（brokerHostName）：
     * iot-06****.mqtt.iothub.aliyuncs.com端口(brokerPort)：1883,iot-06****为iotInstanceId，
     * 由获取设备阿里云参数接口返回
     */
    public class IoTPayment : MonoSingleton<IoTPayment>
    {
        private IoTDevInfo mDevInfo;
        private List<string> eventMessages = new List<string>();
        private bool _isConnected;
        public bool IsConnected { set
            {
                _isConnected = value;
            }
            get
            { return _isConnected; } }

        private string mTopicSend;  //消息发布
        private string mTopicReply; //消息订阅

        void Start()
        {
            M2MqttUnityClient.Instance.ConnectionSucceeded += onConnectionSucceeded;
            M2MqttUnityClient.Instance.ConnectionFailed += onConnectionFailed;

            M2MqttUnityClient.Instance.ActionSubscribeTopics += onSubscribeTopics;
            M2MqttUnityClient.Instance.ActionUnsubscribeTopics += onUnsubscribeTopics;

            M2MqttUnityClient.Instance.ActionDecodeMessage += onDecodeMessage;
        }

        private void StoreMessage(string eventMsg)
        {
            eventMessages.Add(eventMsg);
        }

        void onDecodeMessage(string topic, byte[] message)
        {
            string msg = System.Text.Encoding.UTF8.GetString(message);
            DebugUtils.Log("Received: " + msg);
            StoreMessage(msg);
        }

        void onSubscribeTopics()
        {
            M2MqttUnityClient.Instance.AddSubscribeTopics(mTopicReply);
        }

        void onUnsubscribeTopics()
        {
            M2MqttUnityClient.Instance.RemoveSubscribeTopics(mTopicReply);
        }

        void onConnectionSucceeded()
        {
            DebugUtils.LogWarning("【IOT】好酷已链接");
            IsConnected = true;
        }

        void onConnectionFailed()
        {
            DebugUtils.LogWarning("【IOT】好酷链接关闭");
            IsConnected = false;
            IOTModel.Instance.LinkIOT = false;
            IOTModel.Instance.LinkId = null;
        }

        private void ProcessMessage(string strMsg)
        {
            IoTMsg msg = JsonConvert.DeserializeObject<IoTMsg>(strMsg);
            if (msg == null || msg.data == null /*|| !msg.data.success*/) return;
            switch (msg.messagecmd)
            {
                case MessageCmd.ONLINE_REPLY:           //设备注册返回
                    EventCenter.Instance.EventTrigger(IOTEventHandle.REGISTER_DEV, msg.data.qrcodeUrls);
                    break;
                case MessageCmd.INSERT_COIN:      //平台投币返回
                    CoinData coinData = new CoinData();
                    coinData.Num = msg.data.num;
                    coinData.orderNum = msg.data.orderNum;
                    coinData.memberId = msg.data.memberId;
                    IOTModel.Instance.LinkId = msg.data.orderNum;
                    EventCenter.Instance.EventTrigger(IOTEventHandle.COINT_IN, coinData);
                    break;
                case MessageCmd.DEVICE_PRIZE_REPLY:     //设备退彩
                    TicketOutData tod = new TicketOutData();
                    tod.code = msg.data.code;
                    tod.success = msg.data.success;
                    tod.message = msg.data.message;
                    tod.num = msg.data.num;
                    tod.orderNum = msg.data.orderNum;
                    tod.type = msg.data.type;
                    tod.seq = msg.data.seq;
                    IOTModel.Instance.LinkId = null;
                    EventCenter.Instance.EventTrigger(IOTEventHandle.TICKET_OUT, tod);
                    break;
                case MessageCmd.NOTICE:                 //平台消息
                    NoticeData noticeData = new NoticeData
                    {
                        code = msg.data.code,
                        message = msg.data.message
                    };
                    EventCenter.Instance.EventTrigger(IOTEventHandle.NOTICE, noticeData);
                    break;
                default: break;
            }
        }


        void Update()
        {
            if (eventMessages.Count > 0)
            {
                foreach (string msg in eventMessages)
                {
                    ProcessMessage(msg);
                }
                eventMessages.Clear();
            }
        }

        string MakeMsgId()
        {
            return System.Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="macId">主机机台号</param>
        /// <param name="portId">分机机台号</param>
        /// <param name="accessType">accessType：设备接入方式，1：公众号(默认为1) 2：小程序 3：约战平台</param>
        /// <param name="signal">信号值，介于0~32之间，32：信号最好，一般接网线为32，如果是wifi，按下面的规则：信号值13-16为强；信号值9-12为中；信号值8以下为弱</param>
        /// <param name="version"></param>
        /// <param name="isConnectTestSever">是否连接正式服</param>
        /// <param name="onErrorCallback">报错回调</param>
        public void Init(string macId, int portId, int accessType, int signal, int version ,Action<string> onErrorCallback)
        {

            onConnectionFailed();

            DebugUtils.LogWarning($"【IOT】connect iot;  machineID: {macId} - portId: {portId} - accessType: {accessType} - signal: {signal} - version: {version}");

            bool checkSuccess = CheckDeviceInfo(macId, IoTConst.Secret,out string msg);
            if (checkSuccess) {
                StartCoroutine(RegistDevcie(macId, portId, accessType, signal, version));
            }
            else
            {
                //DebugUtils.LogError($"msg: {msg}");
                onErrorCallback?.Invoke(msg);
            }
        }

        public void Disconnect(){
            //M2MqttUnityClient.Instance.ConnectionSucceeded -= onConnectionSucceeded;
            //M2MqttUnityClient.Instance.ConnectionFailed -= onConnectionFailed;
            //M2MqttUnityClient.Instance.ActionSubscribeTopics -= onSubscribeTopics;
            //M2MqttUnityClient.Instance.ActionUnsubscribeTopics -= onUnsubscribeTopics;
            //M2MqttUnityClient.Instance.ActionDecodeMessage -= onDecodeMessage;
            M2MqttUnityClient.Instance.Disconnect();
        }

        private IEnumerator RegistDevcie(string macId, int portId, int accessType, int signal, int version)
        {
            DebugUtils.LogWarning("【IOT】: 等待Mqtt连接...");
            yield return new WaitUntil(() => IsConnected);
            DebugUtils.LogWarning("【IOT】: Mqtt连接上，开始注册!");
            RegisterDevice(macId, portId, accessType, signal, version);
        }

        /// <summary>
        /// 设备每次开机查询一次，查询成功后保存设备参数，如果查询到新的设备参数，则用新参数替换旧参数
        /// </summary>
        /// <param name="hid"></param>
        /// <param name="secret"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool CheckDeviceInfo(string hid,string secret, out string msg)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param["hardwareId"] = hid;
            param["secret"] = secret;

            string url = IoTConst.GetDevParamURL;
            string strRsp = Utils.Post(url, param); // psot请求
            //string strRsp = Utils.Post("http://i.gzhaoku.com/admin/resp/firmware/query", param); // psot请求
            DevParamRsp rsp = JsonConvert.DeserializeObject<DevParamRsp>(strRsp);
            DebugUtils.Log($"【IOT】post 请求: {url}， {JsonConvert.SerializeObject(param)}");
            DebugUtils.Log($"【IOT】post 响应: {strRsp}");
            if (rsp.success)
            {
                DebugUtils.LogWarning("【IOT】: post 成功，开始mqtt链接 ");
                SaveDeviceInfo(rsp);  // post成功后，开始mqtt链接
            }
            else
            {
                DebugUtils.LogError($"【IOT】: post 失败， strRsp: {strRsp} ");
            }
            msg = rsp.message;
            return rsp.success;
        }

        private void SaveDeviceInfo(DevParamRsp rsp)
        {
            mDevInfo = rsp.data;

            mTopicReply = String.Format("/{0}/{1}/user/s2c/play", mDevInfo.ProductKey,mDevInfo.FirmwareName);
            mTopicSend = String.Format("/{0}/{1}/user/c2s/play", mDevInfo.ProductKey, mDevInfo.FirmwareName);
            //启动mqtt
            M2MqttUnityClient.Instance.InitMqtt(mDevInfo);
        }

        public IoTDevInfo GetDeviceInfo()
        {
            return mDevInfo;
        }

        /// <summary>
        /// 注册设备
        /// </summary>
        /// <param name="hid">主机机台号</param>
        /// <param name="portid">分机机台号</param>
        /// <param name="accessType">accessType：设备接入方式，1：公众号(默认为1) 2：小程序 3：约战平台</param>
        /// <param name="signal">信号值，介于0~32之间，32：信号最好，一般接网线为32，如果是wifi，按下面的规则：信号值13-16为强；信号值9-12为中；信号值8以下为弱</param>
        /// <param name="version">版本号为整数</param>
        public void RegisterDevice(string hid,int portid,int accessType,int signal,int version)
        {
            IoTData data = new IoTData
            {
                accessType = accessType,
                signal = signal,
                hardwareId = hid,
                version = version
            };

            IoTMsg msg = new IoTMsg
            {
                data = data,
                messageid = MakeMsgId(),
                messagetype = MessageType.C2S,
                messagecmd = MessageCmd.ONLINE,
                portid = portid,
                timestamp = (Utils.GetTimeStamp() / 1000).ToString()
            };

            string jsonMsg = JsonConvert.SerializeObject(msg);
            M2MqttUnityClient.Instance.PublishMsg(mTopicSend, jsonMsg);
        }

        /// <summary>
        /// 回复平台投币(该指令设备必须回复，如果回复失败 或者 不回复，则退币)
        /// </summary>
        /// <param name="portid">主机机台号</param>
        /// <param name="num">实际投币的数量</param>
        /// <param name="orderNum">对应的订单号</param>
        /// <param name="bSuccess">成功还是失败</param>
        /// <param name="strMessage">失败原因 如果成功可以写入 "操作成功！"</param>
        public void ReplyCoinIn(int portid,int num,string orderNum,bool bSuccess,string strMessage)
        {
            IoTData data = new IoTData
            {
                code = 0,
                success = bSuccess,
                message = strMessage,
                num = num,
                orderNum = orderNum
            };

            IoTMsg msg = new IoTMsg
            {
                data = data,
                messageid = MakeMsgId(),
                messagetype = MessageType.C2S,
                messagecmd = MessageCmd.INSERT_COIN_REPLY,
                portid = portid
            };

            string jsonMsg = JsonConvert.SerializeObject(msg);
            M2MqttUnityClient.Instance.PublishMsg(mTopicSend, jsonMsg);
        }

        /// <summary>
        /// 设备退彩
        /// 平台回复表示下分记录已收到，如果没有收到平台回复（一般是网络不通），则把下分记录
        /// 保存起来，网络正常时再上传，再次上传时订单号orderNum 和 seq不能变，平台以这2个字段来排重。
        /// </summary>
        /// <param name="portid">主机机台号</param>
        /// <param name="ticketOutData">退币数据</param>
        public void DeviceTicketOut(int portid, TicketOutData ticketOutData, UnityAction<int> errorAction)
        {
            if (string.IsNullOrEmpty(IOTModel.Instance.LinkId))
                errorAction(ticketOutData.num);

            IoTData data = new IoTData
            {
                type = ticketOutData.type,
                ext = ticketOutData.ext,
                seq = ticketOutData.seq,
                num = ticketOutData.num,
                orderNum = IOTModel.Instance.LinkId
            };

            IoTMsg msg = new IoTMsg
            {
                data = data,
                messageid = MakeMsgId(),
                messagetype = MessageType.C2S,
                messagecmd = MessageCmd.DEVICE_PRIZE,
                portid = portid
            };

            string jsonMsg = JsonConvert.SerializeObject(msg);
            M2MqttUnityClient.Instance.PublishMsg(mTopicSend, jsonMsg);
        }
    }

}
