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
using GameUtil;
using Hal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SBoxApi
{
    public class SBoxDate
    {
        public int result;

        public int year;
        public int month;
        public int day;
        public int hours;
        public int minutes;
        public int seconds;
    }

    public class SBoxBridgingConf
    {
        public int DeviceId;
        public int DeviceType;
        public int BaudRate;
        public int DataBits;
        public int StopBits;
        public int parity;
    }

    public class SBoxBridgingHandle
    {
        public int DeviceId;
        public int DeviceType;
        public int BaudRate;
        public int DataBits;
        public int StopBits;
        public int parity;
    }

    public class SBOX_SWITCH
    {
        // 以下为SwitchState函数返的状态值固定定义的bit，不同的硬件项目，对应的bit不一定有效，
        // 以下定义以外的bit，由项目情况再决定其作用
        public const ulong SWITCH_UP = ((ulong)1 << 0);
        public const ulong SWITCH_DOWN = ((ulong)1 << 1);
        public const ulong SWITCH_LEFT = ((ulong)1 << 2);
        public const ulong SWITCH_RIGHT = ((ulong)1 << 3);
        public const ulong SWITCH_ROOT_SET = ((ulong)1 << 4);
        public const ulong SWITCH_SET = ((ulong)1 << 5);
        public const ulong SWITCH_DOOR_SWITCH = ((ulong)1 << 6);
        public const ulong SWITCH_PAYOUT = ((ulong)1 << 7);
        public const ulong SWITCH_ENTER = ((ulong)1 << 8);
        public const ulong SWITCH_RULE = ((ulong)1 << 9);
        public const ulong SWITCH_SWITCH = ((ulong)1 << 10);
        public const ulong SWITCH_SCORE_UP = ((ulong)1 << 11);
        public const ulong SWITCH_SCORE_DOWN = ((ulong)1 << 12);
        public const ulong SWITCH_A = ((ulong)1 << 13);
        public const ulong SWITCH_B = ((ulong)1 << 14);
        public const ulong SWITCH_C = ((ulong)1 << 15);
        public const ulong SWITCH_D = ((ulong)1 << 16);
        public const ulong SWITCH_E = ((ulong)1 << 17);
        public const ulong SWITCH_ESC = ((ulong)1 << 18);

        public const ulong SWITCH_KEYBOARD_DOWN = ((ulong)1 << 32);
        public const ulong SWITCH_KEYBOARD_CONFIRM = ((ulong)1 << 33);
        public const ulong SWITCH_KEYBOARD_LITTLE_GAME = ((ulong)1 << 34);
        public const ulong SWITCH_KEYBOARD_RIGHT = ((ulong)1 << 35);
        public const ulong SWITCH_KEYBOARD_LEFT = ((ulong)1 << 36);
        public const ulong SWITCH_KEYBOARD_CANCLE = ((ulong)1 << 37);
        public const ulong SWITCH_KEYBOARD_UP = ((ulong)1 << 38);
        public const ulong SWITCH_KEYBOARD_NOUSE = ((ulong)1 << 39);
    };

    public partial class SBoxSandbox
    {

        private class SBoxInfo
        {
            public bool Ready;
            public int ResetCounter;
            public int DeviceId;
            public int PrinterState;
            public int PrinterState2;
            public int BillState;
            public int credit;
            public byte[] BillBarCodeData = new byte[32];

            public bool IsBillStacked;
            public bool IsMotorBusy;
            public bool IsMotorConnected;

            public int[] NumberOfCoinOut = new int[2];
            public int[] NumberOfCoinIn = new int[4];

            public int[] CounterOfCoinOut = new int[2];
            public int[] CounterOfCoinIn = new int[4];

            public bool[] IsCoinOutTimeout = new bool[2];
            public int InStateTimeout;
            public ulong InState;
            public ulong InStateEx;

            public ulong OutStateMaskBk;
            public ulong OutStateMask;
            public ulong OutStateExMaskBk;
            public ulong OutStateExMask;
        }

        public enum SBOX_ROULETTE_STATE
        {
            ROULETTE_NO_CONNECTED = (1 << 0),         // 未连接
            ROULETTE_IN_BALL_ERR = (1 << 1),          // 进洞感应光眼故障
            ROULETTE_FIRE_BALL_ERR_0 = (1 << 2),      // 发球光眼故障（开机自检时）
            ROULETTE_FIRE_BALL_ERR_1 = (1 << 3),      // 发球光眼故障（正常使用）
            ROULETTE_IN_BALL_BOARD_ERR_0 = (1 << 4),  // 进洞感应控制板通信故障（开机自检时）
            ROULETTE_IN_BALL_BOARD_ERR_1 = (1 << 5),  // 进洞感应控制板通信故障（正常使用）
            ROULETTE_FIRE_BALL_ERR_2 = (1 << 6),      // 有发球但无进洞故障
            ROULETTE_SHAKED_ERR = (1 << 7),           // 防摇故障
            ROULETTE_SELF_CHECK = (1 << 8),           // 正在开机自检
        };

        public enum SBOX_EJECT_STATE
        {
            EJECT_NO_CONNECTED = (1 << 0),            // 未连接
            EJECT_SET_NUMBER_STATE = (1 << 1),        // 处于准备接收球号状态
            EJECT_OPEN_STATE = (1 << 2),              // 开牌命令状态
            EJECT_CLOSE_STATE = (1 << 3),             // 收牌命令状态
            EJECT_BUSY_STATE = (1 << 4),              // 动作正在执行之中
            EJECT_ERR_0 = (1 << 5),                   // 顶球下光电开关故障
            EJECT_ERR_1 = (1 << 6),                   // 推球左光电开关故障
            EJECT_ERR_2 = (1 << 7),                   // 拨球光电开关故障
            EJECT_ERR_3 = (1 << 8),                   // 球盘光电开关故障
            EJECT_ERR_4 = (1 << 9),                   // 落球光电开关故障
            EJECT_ERR_5 = (1 << 10),                  // 顶球上光电开关故障
            EJECT_ERR_6 = (1 << 11),                  // 推球右光电开关故障
        };

        private static SBoxInfo sBoxInfo = new SBoxInfo();
        private static bool bPrinterBridgeListener = false;
        private static List<byte> bPrinterBridgeData = new List<byte>();
        private static int iPrinterId = -1;

        // --------------------------------------------------
        //
        //  init(); exit(); 两函数由本SDK调用，APP层禁止调用
        //
        // --------------------------------------------------
        /**
          *  @brief          
          *  @param          无
          *  @return         true or false
          *  @details        
          */
        public static void Init()
        {
            DataReset();

            SBoxIOEvent.AddListener(40001, MessageR);
            sBoxInfo.Ready = false;
        }

        /**
		 *  @brief          
		 *  @param          无
		 *  @return         无
		 *  @details        
		 */
        public static void Exit()
        {

        }

        /**
		 *  @brief          
		 *  @param          Millisecond 每个周期的时间差，毫秒为单位
		 *  @return         无
		 *  @details        
		 */
        public static void Exec(int Millisecond)
        {
            SwitchInStateExec(Millisecond);

            SwitchOutStateExec(Millisecond);
        }

        private static void SwitchInStateExec(int Millisecond)
        {
            if (sBoxInfo.InStateTimeout != 0)
            {
                if (sBoxInfo.InStateTimeout > Millisecond)
                {
                    sBoxInfo.InStateTimeout -= Millisecond;
                }
                else
                {
                    sBoxInfo.InStateTimeout = 0;
                }
                if (sBoxInfo.InStateTimeout == 0)
                {
                    sBoxInfo.InState = 0;
                    sBoxInfo.InStateEx = 0;
                }
            }
        }

        private static void SwitchOutStateExec(int Millisecond)
        {
            ulong xor = 0;
            ulong OnMask = 0;
            ulong OffMask = 0;
            ulong OnExMask = 0;
            ulong OffExMask = 0;

            if (sBoxInfo.OutStateMask != sBoxInfo.OutStateMaskBk)
            {
                xor = sBoxInfo.OutStateMask ^ sBoxInfo.OutStateMaskBk;
                OnMask = xor & sBoxInfo.OutStateMask;
                OffMask = xor & sBoxInfo.OutStateMaskBk;
                sBoxInfo.OutStateMaskBk = sBoxInfo.OutStateMask;
            }

            if (sBoxInfo.OutStateExMask != sBoxInfo.OutStateExMaskBk)
            {
                xor = sBoxInfo.OutStateExMask ^ sBoxInfo.OutStateExMaskBk;
                OnExMask = xor & sBoxInfo.OutStateExMask;
                OffExMask = xor & sBoxInfo.OutStateExMaskBk;
                sBoxInfo.OutStateExMaskBk = sBoxInfo.OutStateExMask;
            }

            if ((OnMask != 0) || (OnExMask != 0))
            {
                SwitchOn(OnMask, OnExMask);
            }

            if ((OffMask != 0) || (OffExMask != 0))
            {
                SwitchOff(OffMask, OffExMask);
            }
        }

        private static void DataReset()
        {
            sBoxInfo.ResetCounter = 0;
            for (int i = 0; i < 2; i++)
            {
                sBoxInfo.NumberOfCoinOut[i] = 0;
            }
            for (int i = 0; i < 4; i++)
            {
                sBoxInfo.NumberOfCoinIn[i] = 0;
            }

            for (int i = 0; i < 2; i++)
            {
                sBoxInfo.CounterOfCoinOut[i] = 0;
            }
            for (int i = 0; i < 4; i++)
            {
                sBoxInfo.CounterOfCoinIn[i] = 0;
            }

            sBoxInfo.OutStateMask = 0;
            sBoxInfo.OutStateMaskBk = 0;
            sBoxInfo.OutStateExMask = 0;
            sBoxInfo.OutStateExMaskBk = 0;

            for (int i = 0; i < 2; i++)
            {
                sBoxInfo.IsCoinOutTimeout[i] = false;
            }
        }


        /**
          *  @brief          底板主动上发的消息
          *  @param          sBoxPacket
          *  @return         无
          *  @details        
          */
        private static void MessageR(SBoxPacket sBoxPacket)
        {
            //if (sBoxPacket.data.Length != 24)
            //{
            //    return;
            //}
            ulong tmp64 = 0;
            int tmp = 0;
            uint utmp = 0;
            int pos = 0;

            sBoxInfo.Ready = true;

            tmp = sBoxPacket.data[0];
            // 纸币是否存入钱箱
            if ((tmp & (1 << 4)) != 0)
            {
                sBoxInfo.IsBillStacked = true;
            }

            // 退币机状态1
            if ((tmp & (1 << 5)) != 0)
            {
                sBoxInfo.IsCoinOutTimeout[0] = true;
            }
            // 退币机状态2
            if ((tmp & (1 << 6)) != 0)
            {
                sBoxInfo.IsCoinOutTimeout[1] = true;
            }
            // 电机忙状态
            if ((tmp & (1 << 7)) != 0)
            {
                sBoxInfo.IsMotorBusy = true;
            }
            else
            {
                sBoxInfo.IsMotorBusy = false;
            }
            // 电机连线状态
            if ((tmp & (1 << 8)) != 0)
            {
                sBoxInfo.IsMotorConnected = true;
            }
            else
            {
                sBoxInfo.IsMotorConnected = false;
            }


            // 设备ID
            sBoxInfo.DeviceId = sBoxPacket.data[4];

            // 打印机状态
            sBoxInfo.PrinterState = sBoxPacket.data[5];

            // 纸钞机状态
            sBoxInfo.BillState = sBoxPacket.data[6];

            // 纸钞接收器当前币值
            sBoxInfo.credit = sBoxPacket.data[7];

            // PrinterState2
            sBoxInfo.PrinterState2 = sBoxPacket.data[8];

            // 
            tmp64 = (uint)sBoxPacket.data[11];
            tmp64 <<= 32;
            tmp64 |= (uint)sBoxPacket.data[10];
            sBoxInfo.InState = tmp64;
            // 
            tmp64 = (uint)sBoxPacket.data[13];
            tmp64 <<= 32;
            tmp64 |= (uint)sBoxPacket.data[12];
            sBoxInfo.InStateEx = tmp64;
            sBoxInfo.InStateTimeout = 1000;

            if (sBoxInfo.ResetCounter < 5)
            {
                sBoxInfo.ResetCounter++;
            }
            // 退币计数器：0~255
            for (int i = 0; i < 2; i++)
            {
                tmp = sBoxPacket.data[16 + i];
                if (sBoxInfo.ResetCounter >= 5)
                {
                    if (tmp > sBoxInfo.CounterOfCoinOut[i])
                    {
                        sBoxInfo.NumberOfCoinOut[i] += (tmp - sBoxInfo.CounterOfCoinOut[i]);
                    }
                    else if (tmp < sBoxInfo.CounterOfCoinOut[i])
                    {
                        sBoxInfo.NumberOfCoinOut[i] += (256 - sBoxInfo.CounterOfCoinOut[i] + tmp);
                    }
                }
                sBoxInfo.CounterOfCoinOut[i] = tmp;
            }

            // 投币计数器：0~255
            for (int i = 0; i < 4; i++)
            {
                tmp = sBoxPacket.data[18 + i];
                if (sBoxInfo.ResetCounter >= 5)
                {
                    if (tmp > sBoxInfo.CounterOfCoinIn[i])
                    {
                        sBoxInfo.NumberOfCoinIn[i] += (tmp - sBoxInfo.CounterOfCoinIn[i]);
                    }
                    else if (tmp < sBoxInfo.CounterOfCoinIn[i])
                    {
                        sBoxInfo.NumberOfCoinIn[i] += (256 - sBoxInfo.CounterOfCoinIn[i] + tmp);
                    }
                }
                sBoxInfo.CounterOfCoinIn[i] = tmp;
            }

            // ---------------------------------------
            if (sBoxPacket.data.Length > 24)
            {
                for (int i = 0; i < 8; i++)
                {
                    utmp = (uint)sBoxPacket.data[24 + i];

                    sBoxInfo.BillBarCodeData[pos++] = (byte)((utmp >> 24) & 0x0ff);
                    sBoxInfo.BillBarCodeData[pos++] = (byte)((utmp >> 16) & 0x0ff);
                    sBoxInfo.BillBarCodeData[pos++] = (byte)((utmp >> 8) & 0x0ff);
                    sBoxInfo.BillBarCodeData[pos++] = (byte)((utmp >> 0) & 0x0ff);
                }
            }

            // ---------------------------------------
        }

        /**
          *  @brief          检查底板是否连接
          *  @param          无
          *  @return         true or false
          *  @details        
          */
        public static bool Ready()
        {
            return SBoxIOStream.Connected((int)SBoxIOStream.SBoxIODevice.SBOX_DEVICE_SANDBOX) && sBoxInfo.Ready;
        }

        /**
          *  @brief          设备ID号
          *  @param          无
          *  @return         设备ID号
          *  @details        
          */
        public static int DeviceId()
        {
            return sBoxInfo.DeviceId;
        }

        /**
          *  @brief          电机是否忙状态
          *  @param          无
          *  @return         true: 电机正在工作
          *  @details        
          */
        public static bool IsMotorBusy()
        {
            return sBoxInfo.IsMotorBusy;
        }

        /**
          *  @brief          电机是连线状态
          *  @param          无
          *  @return         true: 电机连线
          *  @details        
          */
        public static bool IsMotorConnected()
        {
            return sBoxInfo.IsMotorConnected;
        }

        /**
          *  @brief          纸币是否存入钱箱
          *  @param          无
          *  @return         true: 纸币存入钱箱
          *  @details        
          */
        public static bool IsBillStacked()
        {
            bool bIsBillStacked = sBoxInfo.IsBillStacked;
            sBoxInfo.IsBillStacked = false;
            return bIsBillStacked;
        }

        /**
          *  @brief          纸币面额
          *  @param          无
          *  @return         纸币面额，非0面额只返回一次
          *  @details        
          */
        public static int BillCredit()
        {
            int credit = sBoxInfo.credit;
            sBoxInfo.credit = 0;
            return credit;
        }

        /**
          *  @brief          纸币条码
          *  @param          无
          *  @return         纸币条码，只返回一次
          *  @details        
          */
        public static byte[] BillBarCodeData()
        {
            if (sBoxInfo.BillBarCodeData[0] != 0)
            {
                byte[] data = new byte[sBoxInfo.BillBarCodeData.Length];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = sBoxInfo.BillBarCodeData[i];
                    sBoxInfo.BillBarCodeData[i] = 0;
                }
                return data;
            }
            return null;
        }

        /**
          *  @brief          打印机状态
          *  @param          无
          *  @return         返回打印机状态，-1时未指定打印机，-2未连接，>=0 打印机编号
          *  @details        
          */
        public static int PrinterState()
        {
            int state = sBoxInfo.PrinterState;

            if (state < 0)
            {
                state *= -1;
                state = state & 0x0ff;
                state *= -1;
            }

            return state;
        }

        /**
          *  @brief          打印机状态
          *  @param          无
          *  @return         返回打印机状态
          *                  bit0: 打印机掉线
          *                  bit1: 打印过程中由于无纸而中止
          *                  bit2: 打印机未知错误
          *                  bit3: 纸不足
          *                  bit4: 无纸
          *  @details        
          */
        public static byte PrinterStateEx()
        {
            int state = sBoxInfo.PrinterState;
            byte data = 0;
            int s0 = 0;
            int s1 = 0;

            if (state < 0)
            {
                state *= -1;
                s0 = state & 0x0ff;
                s1 = (state >> 8) & 0x0ff;

                if (s0 == 1)
                {
                    data |= 1;
                }
                else
                {
                    if (s1 == 0)
                    {
                        data |= 1;
                    }
                    else
                    {
                        data = (byte)s1;
                    }
                }
            }
            //else
            {
                if ((sBoxInfo.PrinterState2 & (1 << 0)) == 1)
                {
                    data |= (1 << 1);
                }

                if ((sBoxInfo.PrinterState2 & (1 << 1)) == 2)
                {
                    data |= (1 << 3);
                }
            }
            return data;
        }

        /**
          *  @brief          纸钞机状态
          *  @param          无
          *  @return         返回纸钞机状态，-1时未指定纸钞机，-2未连接，>=0 纸钞机编号
          *  @details        
          */
        public static int BillState()
        {
            return sBoxInfo.BillState;
        }

        /**
          *  @brief          读取外部输入端口的状态位，1：ON, 0：OFF
          *                  部分bit的定义，参考SBOX_SWITCH
          *  @param          无
          *  @return         设备ID号
          *  @details        
          */
        public static ulong SwitchInState()
        {
            return sBoxInfo.InState;
        }

        /**
          *  @brief          读取外部拓展输入端口的状态位，1：ON, 0：OFF
          *  @param          无
          *  @return         设备ID号
          *  @details        
          */
        public static ulong SwitchInStateEx()
        {
            return sBoxInfo.InStateEx;
        }

        /**
		 *  @brief          输出端口开启，参数中每个bit代表着一个端口，对应bit为1时，开启端口，对应bit为0时，不起作用
		 *  @param          state 端口状态位
		 *  @return         无
		 *  @details        
		 */
        public static void SwitchOutStateOn(ulong state)
        {
            sBoxInfo.OutStateMask |= state;
        }

        /**
		 *  @brief          输出端口关闭，参数中每个bit代表着一个端口，对应bit为1时，开启端口，对应bit为0时，不起作用
		 *  @param          state 端口状态位
		 *  @return         无
		 *  @details        
		 */
        public static void SwitchOutStateOff(ulong state)
        {
            sBoxInfo.OutStateMask &= (~state);
        }

        /**
		 *  @brief          拓屏输出端口开启，参数中每个bit代表着一个端口，对应bit为1时，开启端口，对应bit为0时，不起作用
		 *  @param          state 端口状态位
		 *  @return         无
		 *  @details        
		 */
        public static void SwitchOutStateExOn(ulong state)
        {
            sBoxInfo.OutStateExMask |= state;
        }

        /**
		 *  @brief          拓展输出端口关闭，参数中每个bit代表着一个端口，对应bit为1时，开启端口，对应bit为0时，不起作用
		 *  @param          state 端口状态位
		 *  @return         无
		 *  @details        
		 */
        public static void SwitchOutStateExOff(ulong state)
        {
            sBoxInfo.OutStateExMask &= (~state);
        }

        /**
          *  @brief          检查退币是否超时，画面上，可以实现提示退币超时几秒钟后自动取消提示
          *                  注意，当超时返回true时，只返回一次true，再次读取时状态已自动还原为false
          *  @param          id 退币器编号：0~1
          *  @return         true：超时，false：正常
          *  @details        
          */
        public static bool IsCoinOutTimeout(int id)
        {
            if (id > 1)
            {
                return false;
            }
            bool bIsCoinOutTimeout = sBoxInfo.IsCoinOutTimeout[id];
            sBoxInfo.IsCoinOutTimeout[id] = false;
            return bIsCoinOutTimeout;
        }

        /**
          *  @brief          获取相对于上次读取后的退币数量
          *  @param          id 退币器编号，<= 1，正常用用id=0即可，其它只在一些特别的项目用
          *  @return         退币数量
          *  @details        
          */
        public static int NumberOfCoinOut(int id)
        {
            if (id <= 1)
            {
                int result = sBoxInfo.NumberOfCoinOut[id];
                sBoxInfo.NumberOfCoinOut[id] = 0;
                return result;
            }
            else
            {
                return 0;
            }

        }

        /**
          *  @brief          获取相对于上次读取后的投币数量
          *  @param          id 投币器编号，<= 3，正常用用id=0即可，其它只在一些特别的项目用
          *  @return         投币数量
          *  @details        
          */
        public static int NumberOfCoinIn(int id)
        {
            if (id <= 3)
            {
                int result = sBoxInfo.NumberOfCoinIn[id];
                sBoxInfo.NumberOfCoinIn[id] = 0;
                return result;
            }
            else
            {
                return 0;
            }
        }

        /**
		 *  @brief          读取sandbox软件版本
		 *  @param          无
		 *  @return         版本字符串
		 *                  
		 *  @details        
		 */
        public static void Version()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 64, source: 1, target: 4, size: 1);

            sBoxPacket.data[0] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, VersionR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void VersionR(SBoxPacket sBoxPacket)
        {
            string version = "";

            for (int i = 0; i < sBoxPacket.data.Length; i++)
            {
                version += Convert.ToChar(sBoxPacket.data[i]);
            }

            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SANDBOX_VERSION, version);
        }

        /**
		 *  @brief          读取sandbox硬件唯一序列号
		 *  @param          无
		 *  @return         版本字符串
		 *                  
		 *  @details        
		 */
        public static void USN()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 65, source: 1, target: 4, size: 2);

            sBoxPacket.data[0] = 398075641;
            sBoxPacket.data[1] = sBoxPacket.cmd;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, USNR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void USNR(SBoxPacket sBoxPacket)
        {
            string usn = "";

            for (int i = 0; i < sBoxPacket.data.Length; i++)
            {
                usn += Convert.ToString(sBoxPacket.data[i], 16);
            }

            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SANDBOX_USN, usn);
        }


        /**
		 *  @brief          读取日期时间
		 *  @param          DataTime
		 *  @return         版本字符串
		 *                  
		 *  @details        
		 */
        public static void GetDateTime()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 81, source: 1, target: 4, size: 1);

            sBoxPacket.data[0] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, GetDateTimeR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void GetDateTimeR(SBoxPacket sBoxPacket)
        {
            if (sBoxPacket.data[0] == 0)
            {
                DateTime dateTime = new DateTime(
                    sBoxPacket.data[1],
                    sBoxPacket.data[2],
                    sBoxPacket.data[3],
                    sBoxPacket.data[5],
                    sBoxPacket.data[6],
                    sBoxPacket.data[7],
                    0);

                EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SANDBOX_GET_DATETIME, dateTime);
            }
        }

        /**
		 *  @brief          调定日期时间
		 *  @param          DataTime
		 *  @return         版本字符串
		 *                  
		 *  @details        
		 */
        public static void SetDateTime(DateTime dateTime)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 80, source: 1, target: 4, size: 8);

            sBoxPacket.data[0] = dateTime.Year;
            sBoxPacket.data[1] = dateTime.Month;
            sBoxPacket.data[2] = dateTime.Day;
            sBoxPacket.data[3] = 0;
            sBoxPacket.data[4] = dateTime.Hour;
            sBoxPacket.data[5] = dateTime.Minute;
            sBoxPacket.data[6] = dateTime.Second;
            sBoxPacket.data[7] = dateTime.Millisecond;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, SetDateTimeR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void SetDateTimeR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SANDBOX_SET_DATETIME, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          app重启动后，重置SANDBOX本次启动的运行数据
		 *                  用于SANDBOX程序做一些必要的初始化操作
		 *  @param          无
		 *  @return         result = 0：成功
		 *                  result < 0：发送参数错误
		 *                  result > 0：状态码
		 *  @details        
		 */
        public static void Reset()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40000, source: 1, target: 4, size: 2);

            DataReset();

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, ResetR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void ResetR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_RESET, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          输出端口开启，参数中每个bit代表着一个端口，对应bit为1时，开启端口，对应bit为0时，不起作用
		 *                  对于有多个bit需要设定时，请一起处理好参数中的所有bit后，统一调用一次本函数，以免反复多点调用
		 *  @param          state 端口状态位
		 *  @param          stateex 端口状态位拓展
		 *  @return         无
		 *  @details        
		 */
        private static void SwitchOn(ulong state, ulong stateex)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40002, source: 1, target: 4, size: 6);

            sBoxPacket.data[0] = (int)(state & 0x0ffffffff);
            state >>= 32;
            sBoxPacket.data[1] = (int)(state & 0x0ffffffff);

            sBoxPacket.data[2] = (int)(stateex & 0x0ffffffff);
            stateex >>= 32;
            sBoxPacket.data[3] = (int)(stateex & 0x0ffffffff);

            sBoxPacket.data[4] = 0;
            sBoxPacket.data[5] = 0;

            //SBoxIOEvent.AddListener(sBoxPacket.cmd, SwitchOnR);
            SBoxIOStream.Write(sBoxPacket);
        }
        //private static void SwitchOnR(SBoxPacket sBoxPacket)
        //{
        //    EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_SWITCH_ON, sBoxPacket.data[0]);
        //}

        /**
		 *  @brief          输出端口关闭，参数中每个bit代表着一个端口，对应bit为1时，开启端口，对应bit为0时，不起作用
		 *                  对于有多个bit需要设定时，请一起处理好参数中的所有bit后，统一调用一次本函数，以免反复多点调用
		 *  @param          state 端口状态位
		 *  @param          stateex 端口状态位拓展
		 *  @return         无
		 *  @details        
		 */
        private static void SwitchOff(ulong state, ulong stateex)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40003, source: 1, target: 4, size: 6);

            sBoxPacket.data[0] = (int)(state & 0x0ffffffff);
            state >>= 32;
            sBoxPacket.data[1] = (int)(state & 0x0ffffffff);

            sBoxPacket.data[2] = (int)(stateex & 0x0ffffffff);
            stateex >>= 32;
            sBoxPacket.data[3] = (int)(stateex & 0x0ffffffff);

            sBoxPacket.data[4] = 0;
            sBoxPacket.data[5] = 0;

            //SBoxIOEvent.AddListener(sBoxPacket.cmd, SwitchOffR);
            SBoxIOStream.Write(sBoxPacket);
        }
        //private static void SwitchOffR(SBoxPacket sBoxPacket)
        //{
        //    EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_SWITCH_OFF, sBoxPacket.data[0]);
        //}

        /**
		 *  @brief          启动退币
		 *  @param          id 退币器编号，0~1
		 *  @param          counts 退币数量
		 *  @param          type 退币类型，0：退币，1：脉冲打印
		 *  @return         result = 0：成功
		 *                  result< 0：发送参数错误
		 *                  result > 0：状态码
		 *  @details        
		 */
        public static void CoinOutStart(int id, int counts, int type)
        {
            SBoxPacket sBoxPacket;
            if (id > 1)
            {
                return;
            }

            sBoxPacket = new SBoxPacket(cmd: 40005, source: 1, target: 4, size: 3);
            sBoxPacket.data[0] = id;
            sBoxPacket.data[1] = counts;
            sBoxPacket.data[2] = type;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, CoinOutStartR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void CoinOutStartR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_COIN_OUT_START, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          中止退币
		 *  @param          id 退币器编号，0~1
		 *  @return         result = 0：成功
		 *                  result< 0：发送参数错误
		 *                  result > 0：状态码
		 *  @details        
		 */
        public static void CoinOutStop(int id)
        {
            SBoxPacket sBoxPacket;
            if (id > 1)
            {
                return;
            }

            sBoxPacket = new SBoxPacket(cmd: 40006, source: 1, target: 4, size: 1);

            sBoxPacket.data[0] = id;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, CoinOutStopR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void CoinOutStopR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_COIN_OUT_STOP, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          走码表
		 *  @param          id 码表编号，0：投币码表，1：退币码表，2：上分码表，3：下分码表
		 *  @param          counts 码表走数
		 *  @param          type 走数类型，0：无䇅，1：counts为绝对值，2：counts为追加值，3：中止走数，
		 *  @return         result = 0：成功
		 *                  result < 0：发送参数错误
		 *                  result > 0：状态码
		 *  @details        
		 */
        public static void MeterSet(int id, int counts, int type)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40007, source: 1, target: 4, size: 18);

            if (id > 3)
            {
                return;
            }
            for (int i = 0; i < sBoxPacket.data.Length; i++)
            {
                sBoxPacket.data[i] = 0;
            }
            sBoxPacket.data[id * 2 + 0] = type;
            sBoxPacket.data[id * 2 + 1] = counts;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, MeterSetR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void MeterSetR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_METER_SET, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          开启或停止电机振动
		 *  @param          millisecond 电机振动时间，本时间只针对整个振动周期中的主时间，启动时间和结束时间为固定不可设.
		 *  @return         result = 0：成功
		 *                  result < 0：发送参数错误
		 *                  result > 0：状态码
		 *  @details        
		 */
        public static void MotorTouch(int millisecond)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40008, source: 1, target: 4, size: 2);

            sBoxPacket.data[0] = millisecond;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, MotorTouchR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void MotorTouchR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_MOTOR_TOUCH, sBoxPacket.data[0]);
        }


        /**
		 *  @brief          读取所支持的纸钞机型号列表
		 *  @param          无
		 *  @return         data[0] = 0：成功
		 *                  data[0] < 0：发送参数错误
		 *                  data[0] > 0：状态码
		 *                  data[1~]: data数据，需要先转成char字符串，然后每个型号之间以符号（.）分隔
		 *  @details        
		 */
        public static void BillListGet()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40100, source: 1, target: 4, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BillListGetR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BillListGetR(SBoxPacket sBoxPacket)
        {
            if (sBoxPacket.data[0] != 0)
            {
                return;
            }
            string str = "";
            for (int i = 1; i < sBoxPacket.data.Length; i++)
            {
                int temp = sBoxPacket.data[i];
                byte[] bytes = BitConverter.GetBytes(temp);
                string tempStr = Encoding.Default.GetString(bytes, 0, 4);
                if (string.IsNullOrEmpty(tempStr) || tempStr == "\0")
                    continue;
                str += tempStr;
            }
            List<string> billList = str.Split('.').ToList();
            billList.RemoveAt(billList.Count - 1);
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_BILL_LIST_GET, billList);
        }


        /**
		 *  @brief          选择纸钞机型号
		 *  @param          id 纸钞机型号编号，与BillListGet获取到的列表编号一致，从0开始，-1时停用纸钞机
		 *  @return         data[0] = 0：成功
		 *                  data[0] < 0：发送参数错误
		 *                  data[0] > 0：状态码
		 *  @details        
		 */
        public static void BillSelect(int id)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40101, source: 1, target: 4, size: 2);

            sBoxPacket.data[0] = id;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BillSelectR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BillSelectR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_BILL_SELECT, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          核准收币
		 *  @param          无
		 *  @return         data[0] = 0：成功
		 *                  data[0] < 0：发送参数错误
		 *                  data[0] > 0：状态码
		 *  @details        
		 */
        public static void BillApprove()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40102, source: 1, target: 4, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BillApproveR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BillApproveR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_BILL_APPROVE, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          拒收纸币
		 *  @param          无
		 *  @return         data[0] = 0：成功
		 *                  data[0] < 0：发送参数错误
		 *                  data[0] > 0：状态码
		 *  @details        
		 */
        public static void BillReject()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40103, source: 1, target: 4, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BillRejectR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BillRejectR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_BILL_REJECT, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          读取所支持的打印机型号列表
		 *  @param          无
		 *  @return         data[0] = 0：成功
		 *                  data[0] < 0：发送参数错误
		 *                  data[0] > 0：状态码
		 *                  data[1~]: data数据，需要先转成char字符串，然后每个型号之间以符号（.）分隔
		 *  @details        
		 */
        public static void PrinterListGet()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40150, source: 1, target: 4, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, PrinterListGetR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void PrinterListGetR(SBoxPacket sBoxPacket)
        {
            if (sBoxPacket.data[0] != 0)
            {
                return;
            }
            string str = "";
            for (int i = 1; i < sBoxPacket.data.Length; i++)
            {
                int temp = sBoxPacket.data[i];
                byte[] bytes = BitConverter.GetBytes(temp);
                string tempStr = Encoding.Default.GetString(bytes, 0, 4);
                if (string.IsNullOrEmpty(tempStr) || tempStr == "\0")
                    continue;
                str += tempStr;
            }
            List<string> printerList = str.Split('.').ToList();
            printerList.RemoveAt(printerList.Count - 1);
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_PRINTER_LIST_GET, printerList);
        }

        /**
		 *  @brief          选择打印机型号
		 *  @param          id 打印机型号编号，与PrinterListGet获取到的列表编号一致，从0开始，-1时停用打印机
		 *  @return         data[0] = 0：成功
		 *                  data[0] < 0：发送参数错误
		 *                  data[0] > 0：状态码
		 *  @details        
		 */
        public static void PrinterSelect(int id)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40151, source: 1, target: 4, size: 2);

            iPrinterId = id;

            sBoxPacket.data[0] = id;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, PrinterSelectR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void PrinterSelectR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_PRINTER_SELECT, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          重置打印机，恢复打印机初始化设定，如字体大小，对齐方式等等
		 *  @param          无
		 *  @return         data[0] = 0：成功
		 *                  data[0] < 0：发送参数错误
		 *                  data[0] > 0：状态码
		 *  @details        
		 */
        public static void PrinterReset()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40152, source: 1, target: 4, size: 1);

            sBoxPacket.data[0] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, PrinterResetR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void PrinterResetR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_PRINTER_RESET, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          设定文字大小, 默认为：5
		 *  @param          FontSize 
		 *  @return         data[0] = 0：成功
		 *                  data[0] < 0：发送参数错误
		 *                  data[0] > 0：状态码
		 *  @details        
		 */
        public static void PrinterFontSize(int FontSize)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40153, source: 1, target: 4, size: 1);

            sBoxPacket.data[0] = FontSize;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, PrinterFontSizeR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void PrinterFontSizeR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_PRINTER_FONTSIZE, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          切纸
		 *  @param          无 
		 *  @return         data[0] = 0：成功
		 *                  data[0] < 0：发送参数错误
		 *                  data[0] > 0：状态码
		 *  @details        
		 */
        public static void PrinterPaperCut()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40154, source: 1, target: 4, size: 1);

            sBoxPacket.data[0] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, PrinterPaperCutR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void PrinterPaperCutR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_PRINTER_PAPERCUT, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          打印文字内容并切纸
		 *  @param          message 
		 *  @return         data[0] = 0：成功
		 *                  data[0] < 0：发送参数错误
		 *                  data[0] > 0：状态码
		 *  @details        
		 */
        public static void PrinterMessage(string message)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40155, source: 1, target: 4, size: ((message.Length + 3) / 4) + 1);

            sBoxPacket.data[0] = message.Length;

            List<string> strList = Utils.SplitLength(message, 4);
            string endStr = strList[strList.Count - 1];
            endStr = endStr.PadRight(4, '\0');
            strList[strList.Count - 1] = endStr;

            List<int> data = new List<int>();
            for (int i = 0; i < strList.Count; i++)
            {
                byte[] bytes = Encoding.Default.GetBytes(strList[i]);
                data.Add(BitConverter.ToInt32(bytes, 0));
            }
            int index = 0;
            for (int i = 1; i < sBoxPacket.data.Length - 1; i++)
            {
                if (index < data.Count)
                {
                    sBoxPacket.data[i] = data[index];
                    index++;
                }
                else
                    sBoxPacket.data[i] = 0;
            }

            SBoxIOEvent.AddListener(sBoxPacket.cmd, PrinterMessageR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void PrinterMessageR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_PRINTER_MESSAGE, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          设置打印机内部时间
		 *  @param          Date 日期时间
		 *  @return         data[0] = 0：成功
		 *                  data[0] < 0：发送参数错误
		 *                  data[0] > 0：状态码
		 *  @details        
		 */
        public static void PrinterDateSet(SBoxDate Date)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40156, source: 1, target: 4, size: 6);

            sBoxPacket.data[0] = Date.year;
            sBoxPacket.data[1] = Date.month;
            sBoxPacket.data[2] = Date.day;
            sBoxPacket.data[3] = Date.hours;
            sBoxPacket.data[4] = Date.minutes;
            sBoxPacket.data[5] = Date.seconds;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, PrinterTimeSetR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void PrinterTimeSetR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_PRINTER_DATESET, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          读取打印机内部时间
		 *  @param          Date 日期时间
		 *  @return         data[0] = 0：成功
		 *                  data[0] < 0：发送参数错误
		 *                  data[0] > 0：状态码
		 *  @details        
		 */
        public static void PrinterDateGet()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40157, source: 1, target: 4, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, PrinterDateGetR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void PrinterDateGetR(SBoxPacket sBoxPacket)
        {
            SBoxDate sBoxDate = new SBoxDate();

            sBoxDate.result = sBoxPacket.data[0];
            sBoxDate.year = sBoxPacket.data[1];
            sBoxDate.month = sBoxPacket.data[2];
            sBoxDate.day = sBoxPacket.data[3];
            sBoxDate.hours = sBoxPacket.data[4];
            sBoxDate.minutes = sBoxPacket.data[5];
            sBoxDate.seconds = sBoxPacket.data[6];

            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_PRINTER_DATEGET, sBoxDate);
        }

        /**
		 *  @brief          打开/关闭桥接打印机
		 *  @param          id 打印机型号编号，与PrinterListGet获取到的列表编号一致，从0开始，-1时停用打印机
		 *  @param          BaudRate 波特率 
		 *  @param          DataBits 数据位（8位） 
		 *  @param          StopBits 停止位（1位）
		 *  @param          parity 检验位，0：无校验，1：奇校验，2：偶校验，3：MARK，4：空格
		 *  @return         
		 *  @details        
		 */
        public static void PrinterBridgeSetup(int id, int BaudRate, int DataBits, int StopBits, int parity)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40158, source: 1, target: 4, size: 8);

            bPrinterBridgeData.Clear();

            // 直接用Select函数指令的ID号
            id = iPrinterId;

            sBoxPacket.data[0] = id;
            sBoxPacket.data[1] = BaudRate;
            sBoxPacket.data[2] = DataBits;
            sBoxPacket.data[3] = StopBits;
            sBoxPacket.data[4] = parity;
            sBoxPacket.data[5] = 0;
            sBoxPacket.data[6] = 0;
            sBoxPacket.data[7] = 0;

            //SBoxIOEvent.AddListener(sBoxPacket.cmd, PrinterBridgeOpenR);
            SBoxIOStream.Write(sBoxPacket);
        }

        /**
          *  @brief          写数据到桥接打印机
          *  @param          payload 数据
          *  @return          
          *  @details        
          */
        public static void PrinterBridgeWrite(byte[] payload)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40159, source: 1, target: 4, size: (payload.Length / 4) + 2);
            uint value = 0;
            uint temp;
            int pos = 0;
            int t = 0;

            sBoxPacket.data[pos++] = payload.Length;

            for (int i = 0; i < payload.Length; i++)
            {
                temp = (uint)payload[i];
                t = i % 4;
                if (t == 0)
                {
                    value = 0;
                }
                value |= (temp << (8 * t));
                if (t == 3)
                {
                    sBoxPacket.data[pos++] = (int)value;
                }
            }
            if (t != 3)
            {
                sBoxPacket.data[pos++] = (int)value;
            }
            SBoxIOStream.Write(sBoxPacket);

            bPrinterBridgeData.Clear();
            if (bPrinterBridgeListener != true)
            {
                bPrinterBridgeListener = true;
                SBoxIOEvent.AddListener(40159, PrinterBridgeMessageR);
            }
        }
        private static void PrinterBridgeMessageR(SBoxPacket sBoxPacket)
        {
            if (sBoxPacket.data.Length > 1)
            {
                int pos = 0;
                int size = sBoxPacket.data[pos++];

                if (bPrinterBridgeData.Count > 2048)
                {
                    bPrinterBridgeData.Clear();
                }

                if (size > 0)
                {
                    byte data;
                    int len = 0;
                    uint value = 0;

                    while (pos < sBoxPacket.data.Length)
                    {
                        value = (uint)sBoxPacket.data[pos++];
                        data = (byte)(value & 0x0ff);
                        bPrinterBridgeData.Add(data);
                        len++;
                        if (len >= size)
                        {
                            break;
                        }

                        value >>= 8;
                        data = (byte)(value & 0x0ff);
                        bPrinterBridgeData.Add(data);
                        len++;
                        if (len >= size)
                        {
                            break;
                        }

                        value >>= 8;
                        data = (byte)(value & 0x0ff);
                        bPrinterBridgeData.Add(data);
                        len++;
                        if (len >= size)
                        {
                            break;
                        }

                        value >>= 8;
                        data = (byte)(value & 0x0ff);
                        bPrinterBridgeData.Add(data);
                        len++;
                        if (len >= size)
                        {
                            break;
                        }
                    }

                }
            }
        }


        /**
          *  @brief          从桥接打印机读取数据
          *  @param          count 数量
          *  @return         返回读到的指定数量的数据
          *  @details        
          */
        public static byte[] PrinterBridgeRead()
        {
            if (bPrinterBridgeData == null)
            {
                return new byte[0];
            }
            else
            {
                if (bPrinterBridgeData.Count > 0)
                {
                    byte[] data = new byte[bPrinterBridgeData.Count];
                    for (int i = 0; i < bPrinterBridgeData.Count; i++)
                    {
                        data[i] = bPrinterBridgeData[0];
                        bPrinterBridgeData.RemoveAt(0);
                    }
                    return data;
                }
                else
                {
                    return new byte[0];
                }
            }
        }

        // --------------------------------------------------------------------------------------------------

        /**
		 *  @brief          轮盘机芯游戏流程命令
		 *  @param          CtrlId 控制功能号：=1 开始押分，=2 停止押分，=3 开奖结束
		 *                  CtrlData 开始押分时设定进洞颜色组号（未使用）
		 *  @return         data[0] >=0：成功，返回设定的颜色组号，=-1：执行失败，=-2：指令超时
		 *  @details        
		 */
        public static void RouletteCtrl(int CtrlId, int CtrlData)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40360, source: 1, target: 4, size: 4);

            sBoxPacket.data[0] = CtrlId;
            sBoxPacket.data[1] = CtrlData;
            sBoxPacket.data[2] = 0;
            sBoxPacket.data[3] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, RouletteCtrlR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void RouletteCtrlR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_ROULETTE_CTRL, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          轮盘机芯电机运行模式
		 *  @param          mode 控制功能号：=1 加速-减速
		 *                                  =2 加速至最快速度，保持最后的速度值运行
		 *                                  =3 减速至最慢速度，保持最后的速度值运行
		 *  @return         data[0] =0：成功，=-1：执行失败，=-2：指令超时
		 *  @details        
		 */
        public static void RouletteMotorMode(int mode)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40361, source: 1, target: 4, size: 4);

            sBoxPacket.data[0] = mode;
            sBoxPacket.data[1] = 0;
            sBoxPacket.data[2] = 0;
            sBoxPacket.data[3] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, RouletteMotorModeR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void RouletteMotorModeR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_ROULETTE_MOTOR_MODE, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          轮盘启停命令
		 *  @param          bIsRun =false 停止，=true 启动
		 *  @return         data[0] =0：成功，=-1：执行失败，=-2：指令超时
		 *  @details        
		 */
        public static void RouletteRun(bool bIsRun)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40362, source: 1, target: 4, size: 4);

            if (bIsRun == true)
            {
                sBoxPacket.data[0] = 1;
            }
            else
            {
                sBoxPacket.data[0] = 0;
            }
            sBoxPacket.data[1] = 0;
            sBoxPacket.data[2] = 0;
            sBoxPacket.data[3] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, RouletteRunR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void RouletteRunR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_ROULETTE_RUN, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          轮盘LED Demo
		 *  @param          DemoIndex Demo索引，1~47种
		 *                  level 亮度等级，1~15种
		 *  @return         data[0] =0：成功，=-1：执行失败，=-2：指令超时
		 *  @details        
		 *  
		 *  
		 *                 
		 */
        public static void RouletteLedDemo(int DemoIndex, int level)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40363, source: 1, target: 4, size: 4);

            sBoxPacket.data[0] = DemoIndex;
            sBoxPacket.data[1] = level;
            sBoxPacket.data[2] = 0;
            sBoxPacket.data[3] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, RouletteLedDemoR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void RouletteLedDemoR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_ROULETTE_LED_DEMO, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          送灯跑灯命令
		 *  @param          DemoIndex 跑灯中奖洞号，0~23
		 *                  back 跑灯背景色，1~7种（未使用）
		 *                  back 跑灯背景色，1~7种（未使用）
		 *  @return         data[0] =0：成功，=-1：执行失败，=-2：指令超时
		 *  @details        
		 */
        public static void RouletteLedMode(int index, int back, int front)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40364, source: 1, target: 4, size: 4);

            sBoxPacket.data[0] = index;
            sBoxPacket.data[1] = back;
            sBoxPacket.data[2] = front;
            sBoxPacket.data[3] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, RouletteLedModeR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void RouletteLedModeR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_ROULETTE_LED_MODE, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          启动主游戏开球命令
		 *  @param          
		 *  @return         data[0] =0：成功，=-1：执行失败，=-2：指令超时
		 *  @details        
		 */
        public static void RouletteTouch()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40365, source: 1, target: 4, size: 4);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;
            sBoxPacket.data[2] = 0;
            sBoxPacket.data[3] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, RouletteTouchR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void RouletteTouchR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_ROULETTE_TOUCH, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          查询开球结果命令
		 *  @param          
		 *  @return         data[0] >=0：成功，有效中奖球号（0~23，255时，未出结果），=-1：执行失败，=-2：指令超时
		 *  @details        
		 */
        public static void RouletteResult()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40366, source: 1, target: 4, size: 4);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;
            sBoxPacket.data[2] = 0;
            sBoxPacket.data[3] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, RouletteResultR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void RouletteResultR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_ROULETTE_RESULT, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          查询跑灯动作状态命令
		 *  @param          
		 *  @return         data[0] =0：跑灯动作结束，=1：正在跑灯动作，=-1：执行失败，=-2：指令超时
		 *  @details        
		 */
        public static void RouletteLedState()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40367, source: 1, target: 4, size: 4);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;
            sBoxPacket.data[2] = 0;
            sBoxPacket.data[3] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, RouletteLedStateR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void RouletteLedStateR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_ROULETTE_LED_STATE, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          读取轮盘机芯的状态
		 *  @param          
		 *  @return         data[0] 参考SBOX_ROULETTE_STATE
		 *  @details        
		 */
        public static void RouletteState()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40368, source: 1, target: 4, size: 4);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;
            sBoxPacket.data[2] = 0;
            sBoxPacket.data[3] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, RouletteStateR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void RouletteStateR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_ROULETTE_STATE, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          开奖颜色命令
		 *  @param          color 开奖颜色： =1 红色
		 *                                  =2 绿色
		 *                                  =3 黄色
		 *  @return         data[0] =0：成功，=-1：执行失败，=-2：指令超时
		 *  @details        
		 */
        public static void RouletteResultColor(int color)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40373, source: 1, target: 4, size: 4);

            sBoxPacket.data[0] = color;
            sBoxPacket.data[1] = 0;
            sBoxPacket.data[2] = 0;
            sBoxPacket.data[3] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, RouletteResultColorR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void RouletteResultColorR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_ROULETTE_RESULT_COLOR, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          读取顶球机芯的状态
		 *  @param          
		 *  @return         data[0] 参考SBOX_EJECT_STATE
		 *  @details        
		 */
        public static void EjectState()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40400, source: 1, target: 4, size: 4);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;
            sBoxPacket.data[2] = 0;
            sBoxPacket.data[3] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, EjectStateR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void EjectStateR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_EJECT_STATE, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          顶出指定开奖球号
		 *  @param          index 开奖球号：1~10
		 *  @return         data[0] =0：成功，=-1：执行失败，=-2：指令超时
		 *  @details        
		 */
        public static void EjectResultNumber(int number)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40402, source: 1, target: 4, size: 4);

            sBoxPacket.data[0] = number;
            sBoxPacket.data[1] = 0;
            sBoxPacket.data[2] = 0;
            sBoxPacket.data[3] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, EjectResultNumberR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void EjectResultNumberR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_EJECT_RESULT_NUMBER, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          开牌
		 *  @param          
		 *  @return         data[0] =0：成功，=-1：执行失败，=-2：指令超时
		 *  @details        
		 */
        public static void EjectOpen()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40403, source: 1, target: 4, size: 4);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;
            sBoxPacket.data[2] = 0;
            sBoxPacket.data[3] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, EjectOpenR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void EjectOpenR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_EJECT_OPEN, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          收牌
		 *  @param          
		 *  @return         data[0] =0：成功，=-1：执行失败，=-2：指令超时
		 *  @details        
		 */
        public static void EjectClose()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40404, source: 1, target: 4, size: 4);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;
            sBoxPacket.data[2] = 0;
            sBoxPacket.data[3] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, EjectCloseR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void EjectCloseR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_EJECT_CLOSE, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          复位
		 *  @param          
		 *  @return         data[0] =0：成功，=-1：执行失败，=-2：指令超时
		 *  @details        
		 */
        public static void EjectReset()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40405, source: 1, target: 4, size: 4);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;
            sBoxPacket.data[2] = 0;
            sBoxPacket.data[3] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, EjectResetR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void EjectResetR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_EJECT_RESET, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          配置球号
		 *  @param          index 球号：-1：开启配置球位的球号流程，球号是从第一个球位开始的，同一个球号不能重复设置
		 *                              0：本球位留空
		 *                              1~10：对应球位的球号
		 *  @return         data[0] =0：成功，=-1：执行失败，=-2：指令超时
		 *  @details        
		 */
        public static void EjectNumberSet(int number)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 40408, source: 1, target: 4, size: 4);

            sBoxPacket.data[0] = number;
            sBoxPacket.data[1] = 0;
            sBoxPacket.data[2] = 0;
            sBoxPacket.data[3] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, EjectNumberSetR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void EjectNumberSetR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SADNBOX_EJECT_SET, sBoxPacket.data[0]);
        }

    }
}
