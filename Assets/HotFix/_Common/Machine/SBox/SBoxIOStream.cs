/**
 * @file    
 * @author  Huang Wen <Email:ww1383@163.com, QQ:214890094, WeChat:w18926268887>
 * @version 1.0
 *
 * @section LICENSE
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 *
 * @section DESCRIPTION
 *
 * This file is ...
 */
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UdpData;

namespace Hal
{

    /*
	 * BoxPacket类
	 */
    [Serializable]
    public class SBoxPacket
    {
        public int cmd;
        public int source;
        public int target;
        public int[] data;

        public SBoxPacket() { }

        public SBoxPacket(int cmd, int source, int target, int size)
        {
            this.cmd = cmd;
            this.source = source;
            this.target = target;
            this.data = new int[size];
        }
    }


    /*
	 * Sandbox类，只能存在一个全局对象
	 */
    public class SBoxIOStream
    {

        public enum SBoxIODevice
        {
            SBOX_DEVICE_IDEA = 2,
            SBOX_DEVICE_SANDBOX = 4,
            SBOX_DEVICE_CONF = 8,
            SBOX_DEVICE_AUTH = 16,
            SBOX_DEVICE_OTHER = 32,
        };

        /*
		 * 引入android plugin
		 */
        private static AndroidJavaClass m_jc;
        private static AndroidJavaObject m_jo;
        //private static AndroidJavaObject m_jo = new AndroidJavaObject("com.unity3d.player.UnityPlayer");	

        public static Dictionary<int, bool> connectDic = new Dictionary<int, bool>();

        /**
		 *  @brief          初始化Sandbox模块
		 *  @param          无
		 *  @return         true or false
		 *  @details        
		 */
        public static bool Init()
        {
            if (Application.isEditor)
            {
                connectDic[(int)SBoxIODevice.SBOX_DEVICE_IDEA] = false;
                connectDic[(int)SBoxIODevice.SBOX_DEVICE_SANDBOX] = false;
                return true;
            }
            else
            {
                m_jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                m_jo = m_jc.GetStatic<AndroidJavaObject>("currentActivity");
                // 调用java函数sandboxInit初始化
                bool result = m_jo.Call<bool>("sandboxInit");

                return result;
            }
        }

        /**
		 *  @brief          退出Sandbox模块
		 *  @param          无
		 *  @return         无
		 *  @details        
		 */
        public static void Exit()
        {
            m_jo.Call("sandboxExit");
        }

        /**
		 *  @brief          获取sandbox服务模块的版本号
		 *  @param          无
		 *  @return         返回版本号字符串，如：1.0.0
		 *  @details        
		 */
        public static string Version()
        {
            string version = m_jo.Call<string>("sandboxVersion");

            return version;
        }

        /**
		 *  @brief          设备是否已连接
		 *  @param[in]      address 设备地址
		 *  @return         已连接：true，未连接：false
		 *  @details        
		 */
        public static bool Connected(int address)
        {
            if (Application.isEditor)
            {
                CheckConnectMsg checkConnectMsg = new CheckConnectMsg()
                {
                    ip = Utils.LocalIP(),
                    address = address
                };
                MatchDebugMsg debugMsg = new MatchDebugMsg()
                {
                    handle = MatchHandle.CheckConnect,
                    data = JsonConvert.SerializeObject(checkConnectMsg)
                };
                MatchDebugManager.Instance.SendUdpMessage(JsonConvert.SerializeObject(debugMsg));
            }
            else
                return m_jo.Call<bool>("sandboxConnected", address);

            return connectDic[address];
        }

        /**
		 *  @brief          需要周期性调用
		 *  @param          无
		 *  @return         无
		 *  @details        
		 */
        public static void Exec()
        {
            //m_jo.Call("sandboxExec");
        }

        /**
		 *  @brief          读取数据包
		 *  @param          无
		 *  @return         数据包对象或null
		 *  @details        
		 */
        public static SBoxPacket Read()
        {
            string json = m_jo.Call<string>("sandboxRead");

            if (json != null)
                return JsonConvert.DeserializeObject<SBoxPacket>(json);
            return null;
        }

        /**
		 *  @brief          发送数据包
		 *  @param          packet BoxPacket对象
		 *  @return         true or false
		 *  @details        
		 */
        public static bool Write(SBoxPacket packet)
        {
            bool result = false;
            string json = JsonConvert.SerializeObject(packet);
            //if (packet.cmd != 20052
            //    && packet.cmd != 20040
            //    && packet.cmd != 40368)
            //    //Debug.Log("<color=#0000FF>SBoxWrite:" + json + "</color>");
            //    Debug.Log("SBoxWrite:" + json + "");
            if (Application.isEditor)
            {
                MatchDebugMsg debugMsg = new MatchDebugMsg()
                {
                    handle = MatchHandle.Packet,
                    data = json
                };
                MatchDebugManager.Instance.SendUdpMessage(JsonConvert.SerializeObject(debugMsg));
            }
            else
                result = m_jo.Call<bool>("sandboxWrite", json);

            return result;
        }
    }
}
