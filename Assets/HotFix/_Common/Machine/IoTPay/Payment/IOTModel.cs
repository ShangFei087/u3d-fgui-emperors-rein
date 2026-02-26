using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace IOT
{
    public class IOTModel : Singleton<IOTModel>
    {
        /// <summary>
        /// 出奖类型
        /// </summary>
        public enum TicketOutType
        {
            /// <summary>
            /// 实体币
            /// </summary>
            Coin = 1,
            /// <summary>
            /// 礼品
            /// </summary>
            Gift = 51,
            /// <summary>
            /// 彩票
            /// </summary>
            Ticket = 52,
            /// <summary>
            /// 积分
            /// </summary>
            Integral = 53,
            /// <summary>
            /// 特殊奖励
            /// </summary>
            Special = 56
        }

        public int PortId
        {
            get
            {
                return qrCodeDatas[0].portid;
            }
        }


        private bool _linkIOT;
        /// <summary>
        /// 是否登录好酷获取到二维码
        /// </summary>
        public bool LinkIOT
        {
            get
            {
                return _linkIOT;
            }
            set
            {
                _linkIOT = value;
                if (_linkIOT)
                    EventCenter.Instance.EventTrigger(EventHandle.REFRESH_QRCORD);
            }
        }

        private string _linkId;


        /// <summary>
        /// 是否绑定了微信号
        /// </summary>
        public string LinkId
        {
            get
            { 
                _linkId = PlayerPrefs.GetString("linkId", null);
                return _linkId;
            }
            set
            {
                _linkId = value;
                PlayerPrefs.SetString("linkId", _linkId);
            }
        }

        /// <summary>
        /// 出奖类型
        /// </summary>
        public TicketOutType ticketOutType = TicketOutType.Ticket;
        /// <summary>
        /// 未处理出彩缓存
        /// </summary>
        public int ticketOutFrame;
        /// <summary>
        /// 二维码数据(只有注册IOT成功才有返回)
        /// </summary>
        public List<QrCodeData> qrCodeDatas;
        /// <summary>
        /// 未完成出彩数据(已向IOT发送, 但未接到返回)
        /// </summary>
        public List<TicketOutData> unfinishTicketOutDatas = new List<TicketOutData>();

        public Texture GetQRTexture(Color color, UnityAction errorAction = null)
        {
            if (qrCodeDatas == null || qrCodeDatas.Count == 0)
            {
                errorAction?.Invoke();
                return null;
            }
            return Utils.GenerateQRImageWithColor(qrCodeDatas[0].qrcodeUrl, color);
        }
    }
}

