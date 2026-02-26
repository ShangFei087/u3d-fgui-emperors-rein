using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Net.WebRequestMethods;

namespace IOT
{
    //需要注册响应的消息
    public class IOTEventHandle
    {
        public const string REGISTER_DEV = "REGISTER_DEV";                  //响应设备注册    返回List<QrCodeData>
        public const string COINT_IN = "IOT_COIN_IN";                       //响应平台投币
        public const string TICKET_OUT = "TICKET_OUT";                      //响应设备退彩
        public const string NOTICE = "NOTICE";                              //响应平台消息
    }
    //这个结构需要存储在本地
    public class IoTDevInfo
    {
        public String ProductKey { get; set; }
        public String DeviceName { get; set; }
        public String FirmwareName { get; set; }
        public String DeviceSecret { get; set; }
        public String IotInstanceId { get; set; }
    }

    public class MessageType
    {
        public static string C2S = "c2s";
        public static string S2C = "s2c";
    }

    public class MessageCmd
    {
        //1.设备注册
        public const string ONLINE = "firmware-online";                        //上行
        public const string ONLINE_REPLY = "firmware-online-reply";            //下行

        //2.平台投币
        public const string INSERT_COIN = "insert-coin";                       //下行
        public const string INSERT_COIN_REPLY = "insert-coin-reply";           //上行

        //3.设备退彩
        public const string DEVICE_PRIZE = "device-prize";                     //上行
        public const string DEVICE_PRIZE_REPLY = "device-prize-reply";         //下行

        //4.平台消息通知
        public const string NOTICE = "NOTICE";                                 //下行
    }

    /*
     * 测试环境：http://i.test.gzhaoku.com/admin/resp/firmware/query
     * 正式环境：http://i.gzhaoku.com/admin/resp/firmware/query
     */
    public class IoTConst
    {
        public static string Secret = "EoZ2mFUdWngwE2s2JgutswVtp4RZFtma";
        //public static string GetDevParamURL =  ApplicationSettings.Instance.isRelease?  "http://i.gzhaoku.com/admin/resp/firmware/query" : "http://i.test.gzhaoku.com/admin/resp/firmware/query";

        public static string GetDevParamURL => "";
            // PlayerPrefsUtils.isUseReleaseIot ? "http://i.gzhaoku.com/admin/resp/firmware/query" :  "http://i.test.gzhaoku.com/admin/resp/firmware/query";
    }

    public class DevParamReq
    {
        /*
         * 硬件 id，由设备端生成，可以为32位guid，保证全局唯一
         */
        public string hardwareId { get; set; }
        public string secret { get; set; }
    }

    public class DevParamRsp
    {
        public int code { get; set; }
        public IoTDevInfo data { get; set; }
        public string message { get; set; }
        public bool success { get; set; }
    }

    public class NoticeData
    {
        /*
         *  消息代码
         *  -1：表示后台只创建了固件，没有创建设备
         */
        public int code { get; set; }
        /*
         *  消息内容
         */
        public string message { get; set; }
    }

    public class QrCodeData
    {
        public int portid { get; set; }
        public string qrcodeUrl { get; set; }
    }

    /*
     * 特殊奖励参数示例：{"type":56,"ext":"{\\"name\\":\\"三等奖\\",\\"note\\":\\"全盘奖\\"}","seq":1733277248,"num":1,"orderNum":"2024101214554301000001"}
     */
    public class TicketOutData
    {
        public int code { get; set; }
        public bool success { get; set; }
        public string message { get; set; }
        /*
         * 出奖类型：*1.实体币,单位:个*51.礼品,单位:个*52.彩票,单位:张*53.积分,*56.特殊奖励
         */
        public int type { get; set; }

        /*
         * ext: 扩展信息：json字符串（无值则传空），对应出奖类型会有不同含义，当type为56，则
         * 上传特殊奖励名称，比如：{"name":"一等奖","note":"全盘奖"}，name对应管理后台抽奖活动的名
         * 称，note对应的是机器上的中奖名称，特殊奖励对应的出奖名称有：一等奖、二等奖、三等奖到
         * 十等奖，需要在管理后台->抽奖活动建立对应的活动名称（对应name）
         */
        public string ext { get; set; }

        /*
         * 序号，对应一个订单下多次分，序号从1开始增长（也可以定义为时间戳，但时间戳只
         * 能精确到秒，不能超过10位），不能重复
         */
        public string seq { get; set; }

        /*
         * 为数量，当type=56时，数量一般设置为1，表示中了一个特殊奖励，数量为2，则表示中奖2次，会打印2张中奖小票
         */
        public int num { get; set; }

        /*
         * 订单号，对应投币的订单号
         */
        public string orderNum { get; set; }
    }

    public class CoinData
    {
        /// <summary>
        /// 投币数量
        /// </summary>
        public int Num { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string orderNum { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public string memberId { get; set; }
    }

    public class IoTData
    {
        public string ext { get; set; }
        public int type { get; set; }

        public string seq { get; set; }
        //版本号
        public int version { get; set; }

        //第1次启动的设备名称会自动绑定
        public string hardwareId { get; set; }

        //设备接入方式，1：公众号(默认为1)2：小程序 3：约战平台
        public int accessType { get; set; }

        //信号值，介于0~32之间，32：信号最好，一般接网线为32，如果是wifi，按下面的规则：
        //信号值13-16为强；信号值9-12为中；信号值8以下为弱
        public int signal { get; set; }

        /*
         * 一般在data里边，表示约定的值，目前没有用，都为0
         */
        public int code { get; set; }

        /*
         * 一般在data里边，表示操作的状态，true为成功，false为失败
         * 当为false时，一般需要记录或者显示message内容，便于调试
         */
        public bool success { get; set; }

        /*
         * 当success为true时，为“操作成功”当success为false时，为失败的描述 比如提示：硬件id不一致
         */
        public string message { get; set; }

        /*
         * 对应设备二维码，如果设备有多个端口，则返回每个端口对应的二维码
         */
        public List<QrCodeData> qrcodeUrls { get; set; }

        /*
        * 投币数量
        */
        public int num { get; set; }
        /*
         * 订单号
         */
        public string orderNum { get; set; }

        /*
         * 用户id
         */
        public string memberId { get; set; }
    }

    //通用消息结构体，发送和接收都使用这个
    public class IoTMsg
    {
        /*
         *  消息唯一标识
         *  由消息发送方生成32位guid,消息重发时Messageid保持不变
         */
        public string messageid { get; set; }

        /*
         *  消息类型
         *  s2c:代表服务器发送到终端
         *  c2s:代表终端发送到服务器
         */
        public string messagetype { get; set; }

        /*
         *  消息指令
         *  比如firmware-online代表设备启动，firmware-online-reply表示回复
         */
        public string messagecmd { get; set; }

        /*
         *  消息内容
         *  具体消息内容不同,data可  以为空，如果是回复消息，一 般带下面3个参数：
                success:操作是否成功
                message:操作信息返回
                code:操作代码返回（目前没
                有用到，都为0）
         */
        public IoTData data { get; set; }

        /*
         *  指令发送时的时间戳
         *  精确到秒，比如日期：2024-02-2817:18:59，转换为时间戳为：1709111939
         */
        public string timestamp { get; set; }

        /*
         *  设备对应端口
         *  如果多个端口，则1、2、3类推，如果只有1个端口，则为1
         */
        public int portid { get; set; }

    }
}
