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
using Hal;
using SimpleJSON;
using System;
using UnityEngine;

namespace SBoxApi
{
    /*
    public enum SBOX_IDEA_COIN_PUSH_KEYVALUE
    {
        KV_SCORE_UP = (1<<0),
        KV_SCORE_DOWN = (1<<1),
        KV_SET = (1<<2),
        KV_ACCOUNT = (1<<3),
        KV_AUTO = (1<<4),
        KV_PAYOUT = (1<<5),
        KV_HELP = (1<<6),
        KV_SWITCH = (1<<7),
        KV_SPIN = (1<<8),
    }*/




    public class SBoxDebugControlModeData
    {
        public int mode;           //调试模式
        public int resType;       //结果类型
        public int bonusType;      //大奖类型
        public int jpType;       //大奖类型
    }


    public partial class SBoxIdea
    {
        static string version = "1.0.0";
        public static SBoxIdeaInfo SBoxInfo => sBoxInfo;

        public static void CoinPushReset(int pid)
        {
            
           // Debug.LogError($"调用 CoinPushReset pid={pid}");
            
            
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20103, source: 1, target: 2, size: 1);
            sBoxPacket.data[0] = pid;
            
            SBoxIOEvent.AddListener(sBoxPacket.cmd, CoinPushResetR);
            SBoxIOStream.Write(sBoxPacket);
        }

        public static void CoinPushResetR(SBoxPacket sBoxPacket)
        {
            
        }
        
        
		/*
		 * 玩家按下按钮时调用一次
		 * pid:玩家id
		 * coin:玩家的投币数
		 */
		public static void CoinPushSpin(int pid,int coin)
		{
            //Debug.LogError($"调用 CoinPushSpin pid={pid}  coin={coin}");
            Debug.Log("SBoxIdea 20100");
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20100, source: 1, target: 2, size: 2);
            sBoxPacket.data[0] = pid;
            sBoxPacket.data[1] = coin;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, CoinPushSpinR);
            SBoxIOStream.Write(sBoxPacket);
        }

        private static void CoinPushSpinR(SBoxPacket sBoxPacket)
        {
            /*
             * ret:0表示成功，-1表示传参失败，-2表示分数不足,-3表示发币失败
             */
            int ret = sBoxPacket.data[0];
            //TODO 根据ret做相应的处理


            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_BEGIN_TURN, ret);
        }

        /*
         * 获取滚轮结果
         * pid:玩家id
         */
        public static void CoinPushGetSpinResult(int pid)
        {
            Debug.Log("SBoxIdea 20101");
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20101, source: 1, target: 2, size: 1);
            sBoxPacket.data[0] = pid;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, CoinPushGetSpinResultR);
            SBoxIOStream.Write(sBoxPacket);
        }

        private static void CoinPushGetSpinResultR(SBoxPacket sBoxPacket)
        {
            //Debug.LogError($"CoinPushGetSpinResultR 函数被调用");
            JSONNode result = JSONNode.Parse("{}");
            /*
             * ret:0表示成功，-1表示传参失败
             */
            int pos = 0;
            int ret = sBoxPacket.data[pos++];
            if(ret !=-1)
            {

                /*
                 * 当前游戏状态
                     #define GS_NORMAL		0	//什么都没有
                    #define GS_START		1
                    #define GS_END			2
                    #define GS_WINLINE		3	//赢线
                    #define GS_FREEGAME		4   //免费游戏
                    #define GS_BONUS		5	//送球
                    #define GS_JPSMALM		6	//中了小中彩金
                    #define GS_OPERATER		7
                 */
                int OpenType = sBoxPacket.data[pos++];
                int ResultType = sBoxPacket.data[pos++];      
                int lineNum = sBoxPacket.data[pos++];      
                int TotalBet = sBoxPacket.data[pos++];
                int MatrixLength = sBoxPacket.data[pos++];
                int TotalFreeTime = sBoxPacket.data[pos++];
                int TotalFreeBet = sBoxPacket.data[pos++];
                int BonusBet = sBoxPacket.data[pos++];
                int BonusType = sBoxPacket.data[pos++];

                Debug.Log("OpenType==" + OpenType);
                Debug.Log("ResultType==" + ResultType);
                Debug.Log("lineNum==" + lineNum);
                Debug.Log("TotalBet==" + TotalBet);
                Debug.Log("MatrixLength==" + MatrixLength);
                Debug.Log("TotalFreeTime==" + TotalFreeTime);
                Debug.Log("TotalFreeBet==" + TotalFreeBet);
                Debug.Log("BonusBet==" + TotalFreeBet);
                Debug.Log("nBonusType==" + TotalFreeBet);

                result["OpenType"] = OpenType;
                result["ResultType"] = ResultType;
                result["lineNum"] = lineNum;
                result["TotalBet"] = TotalBet;
                result["TotalFreeTime"] = TotalFreeTime;
                result["TotalFreeBet"] = TotalFreeBet;
                result["BonusBet"] = BonusBet;
                result["BonusType"] = BonusType;

                //IDVec
                result["IDVec"] = new JSONArray();
                for(int i = 0;i < lineNum; i++)
                {
                    int id = sBoxPacket.data[pos++];
                    result["IDVec"].Add(id); 
                }

                //Matrix
                result["Matrix"] = new JSONArray();
                for (int i = 0; i < MatrixLength; i++)
                {
                    int id = sBoxPacket.data[pos++];
                    result["Matrix"].Add(id);  
                }

                //FreeBetArray
                result["FreeBetArray"] = new JSONArray();
                for (int i = 0; i < TotalFreeTime; i++)
                {
                    int id = sBoxPacket.data[pos++];
                    result["FreeBetArray"].Add(id);
                }

                //BonusData
                result["BonusData"] = new JSONArray();
                for (int i = 0; i < MatrixLength; i++)
                {
                    int id = sBoxPacket.data[pos++];
                    result["BonusData"].Add(id);
                }

                int BlindSymbol = sBoxPacket.data[pos++]; //当大奖为神秘游戏就是BlindSymbol ,乘数游戏或者彩金游戏时就是BonusMultiply
                result["BlindSymbol"] = BlindSymbol;
            }

            result["code"] = ret;
            Debug.LogError($"------code： {ret}----------");
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_SPIN,result.ToString());
        }
        
        
        /*
         * 滚轮停止后发送
         */
        public static void CoinPushGetSpinEnd(int pid)
        {
            Debug.Log("SBoxIdea 20102：" + pid);

            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20102, source: 1, target: 2, size: 1);
            sBoxPacket.data[0] = pid;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, CoinPushGetSpinEndR);
            SBoxIOStream.Write(sBoxPacket);
        }

        private static void CoinPushGetSpinEndR(SBoxPacket sBoxPacket)
        {
            /*
             * ret:0表示成功，-1表示传参失败
             */
            int ret = sBoxPacket.data[0];
            int credit = sBoxPacket.data[1]; //玩家的分数
            //TODO 根据ret做相应的处理

            JSONNode result = new JSONObject();
            result["code"] = ret;
            result["credit"] = credit;
            

            foreach (SBoxPlayerScoreInfo item in sBoxInfo.PlayerScoreInfoList)
            {
                if (item.PlayerId == 1)
                {
                    result["playerCredit"] = item.Score;
                }
            }
            
            
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_SPIN_END,result.ToString());
        }


        /// <summary>
        /// app获取major和grand的贡献值
        /// </summary>
        /// <param name="pid"></param>
        public static void GetJpMajorGrandContribution(int pid)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20104, source: 1, target: 2, size: 1);
            sBoxPacket.data[0] = pid;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, GetJpMajorGrandContributionR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void GetJpMajorGrandContributionR(SBoxPacket sBoxPacket)
        {
            /*
             * ret:0表示成功，-1表示传参失败
             */
            int ret = sBoxPacket.data[0];

            JSONNode result = new JSONObject();
            result["code"] = ret;

            if(ret == 0)
            {
                int major = sBoxPacket.data[1];
                int grand = sBoxPacket.data[2];

                result["major"] = major;
                result["grand"] = grand;
            }

            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_GET_JP_CONTRIBUTION, result.ToString());
        }

        /// <summary>
        /// app获取本地彩金贡献值
        /// </summary>
        /// <param name="pid"></param>
        public static void GetJpContribution(int pid)
        {
            Debug.Log("SBoxIdea 20201");
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20201, source: 1, target: 2, size: 1);
            sBoxPacket.data[0] = pid;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, GetJpContributionR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void GetJpContributionR(SBoxPacket sBoxPacket)
        {

           
            /*
             * ret:0表示成功，-1表示传参失败
             */
            int ret = sBoxPacket.data[0];
            //JSONNode result = new JSONObject();
            JSONNode result = JSONNode.Parse("{}");
            result["code"] = ret;

            if (ret == 0)
            {
                int major = sBoxPacket.data[1];
                int minor = sBoxPacket.data[2];
                int mini = sBoxPacket.data[3];
             
                result["major"] = major;
                result["minor"] = minor;
                result["mini"] = mini;
            }

            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_GET_LOCAL_JP_CONTRIBUTION, result.ToString());
        }




        /// <summary>
        /// 获取到彩金服中奖金额(Major 或 Grand)，存入算法卡
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="wins"></param>
        public static void SetJpMajorGrandWin(int pid, int wins)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20105, source: 1, target: 2, size: 2);
            sBoxPacket.data[0] = pid;
            sBoxPacket.data[1] = wins;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, SetJpMajorGrandWinR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void SetJpMajorGrandWinR(SBoxPacket sBoxPacket)
        {
            int ret = sBoxPacket.data[0];
            //JSONNode result = new JSONObject();
            //result["code"] = ret;

            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_SET_MAJOR_GRAND_WIN, ret);
        }



        /// <summary>
        /// 返还Major、Grand彩金贡献值
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="credit"></param>
        /// <remarks>
        /// * 本地累计 大于10,则放回彩金贡献值
        /// </remarks>
        public static void ReturnJpMajorGrandContribution(int pid, int majorCredit, int grandCredit)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20106, source: 1, target: 2, size: 3);
            sBoxPacket.data[0] = pid;
            sBoxPacket.data[1] = majorCredit;
            sBoxPacket.data[2] = grandCredit;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, ReturnJpMajorGrandContributionR);
            SBoxIOStream.Write(sBoxPacket);
        }

        private static void ReturnJpMajorGrandContributionR(SBoxPacket sBoxPacket)
        {
            int ret = sBoxPacket.data[0];
            //JSONNode result = new JSONObject();
            //result["code"] = ret;

            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_RETURN_JP_CONTRIBUTION, ret);
        }





        /// <summary>
        /// 推币机硬件测试“开始”或“停止”
        /// </summary>
        /// <param name="oper">
        /// 1:发币测试
        /// 2:发球测试
        /// 3:推盘测试
        /// 4:雨刷测试
        /// 5:铃铛测试
        /// 6:雨刷测试
        /// 7:铃铛测试
        /// </param>
        /// <remarks>
        /// *
        /// </remarks>
        public static void CheckCoinPushHardware(int oper)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20107, source: 1, target: 2, size: 1);
            sBoxPacket.data[0] = oper;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, CheckCoinPushHardwareR);
            SBoxIOStream.Write(sBoxPacket);
        }

        private static void CheckCoinPushHardwareR(SBoxPacket sBoxPacket)
        {
            int ret = sBoxPacket.data[0];

            // 新加
            int coinPushTestCoins = sBoxPacket.data[1];  // 当前发币总个数
            int coinPushTestBalls = sBoxPacket.data[2];  // 当前发球总个数
            int coinPushTestPlate = sBoxPacket.data[3]; // 是否测试推盘 1:是 0:否
            int coinPushTestWiper = sBoxPacket.data[4]; // 是否测试雨刷 1:是 0:否


            JSONNode result = new JSONObject();
            result["code"] = ret;
            result["coinPushTestCoins"] = coinPushTestCoins;
            result["coinPushTestBalls"] = coinPushTestBalls;
            result["coinPushTestPlate"] = coinPushTestPlate;
            result["coinPushTestWiper"] = coinPushTestWiper;


            Version currentVersion = new Version(version);
            Version targetVersion = new Version("1.0.6");
            if (currentVersion >= targetVersion)
            {
                int coinPushTestRegainCoins = sBoxPacket.data[5];  // 回币总个数
                int coinPushTestRegainBalls = sBoxPacket.data[6];  // 回球总个数

                result["coinPushTestRegainCoins"] = coinPushTestRegainCoins;
                result["coinPushTestRegainBalls"] = coinPushTestRegainBalls;
            }


            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_HARDWARE_TEST_START_END, result.ToString());

        }








        /// <summary>
        /// 获取硬件状态
        /// </summary>
        public static void GetHardwareFlag()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20108, source: 1, target: 2, size: 1);

            sBoxPacket.data[0] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, GetHardwareFlagR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void GetHardwareFlagR(SBoxPacket sBoxPacket)
        {
            int ret = sBoxPacket.data[0];

            //返回两个值，data[0]表示成功，data[1]是标志位。

            JSONNode result = new JSONObject();
            result["code"] = ret; 
            result["flag"] = sBoxPacket.data[1]; //1bit 发币，2bit 发球

            // int bit0 = number & 1;  bool isCoinDown = bit0 != 0;
            // int bit1 = number & 2;  bool isBallDown = bit1 != 0;
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_HARDWARE_FLAG, result.ToString());
        }



        /// <summary>
        /// 获取硬件状态
        /// </summary>
        public static void GetHardwareResult()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20109, source: 1, target: 2, size: 1);
            sBoxPacket.data[0] = 0;
            SBoxIOEvent.AddListener(sBoxPacket.cmd, GetHardwareResultR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void GetHardwareResultR(SBoxPacket sBoxPacket)
        {
            JSONNode result = new JSONObject();
            result["code"] = sBoxPacket.data[0]; //data[0]表示成功
            result["coinPushTestCoins"] = sBoxPacket.data[1];
            result["coinPushTestBalls"] = sBoxPacket.data[2];
            result["coinPushTestPlate"] = sBoxPacket.data[3];
            result["coinPushTestWiper"] = sBoxPacket.data[4];

            Version currentVersion = new Version(version);
            Version targetVersion = new Version("1.0.6");
            if (currentVersion >= targetVersion)
            {
                int coinPushTestRegainCoins = sBoxPacket.data[5];  // 回币总个数
                int coinPushTestRegainBalls = sBoxPacket.data[6];  // 回球总个数

                result["coinPushTestRegainCoins"] = coinPushTestRegainCoins;
                result["coinPushTestRegainBalls"] = coinPushTestRegainBalls;
            }

            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_HARDWARE_RESULT, result.ToString());
        }



        /// <summary>
        /// 是否进入后台
        /// </summary>
        /// <param name="isIn">1:进入， 0：退出</param>
        public static void IntoConsolePage(int isIn)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20110, source: 1, target: 2, size: 1);
            sBoxPacket.data[0] = isIn;
            SBoxIOEvent.AddListener(sBoxPacket.cmd, IntoConsolePageR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void IntoConsolePageR(SBoxPacket sBoxPacket)
        {
            JSONNode result = new JSONObject();
            result["code"] = sBoxPacket.data[0]; //data[0]表示成功
           // EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_COIN_PUSH_CONSOLE_HARDWARE_RESULT, result.ToString());
        }



        /**
        *  @brief         切换游戏
        *  @param         gameid 为游戏id
        *  @return          
        *  @details                          
        */
        public static void GameSwitch(int gameid)
        {
            Debug.LogError("SBoxIdea 20200:" + gameid);
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20200, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = gameid;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, GameSwitchR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void GameSwitchR(SBoxPacket sBoxPacket)
        {
            Debug.Log("算法切换游戏成功:" + sBoxPacket.data[0]);
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SWITCH_GAME, sBoxPacket.data[0]);
        }


        /**
        *  @brief         调试模式
        *  @param        
        *  @return          
        *  @details                          
        */
        public static void DebugControlMode(SBoxDebugControlModeData sBoxDCM)
        {
            Debug.LogError("SBoxIdea 20202");

            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20202, source: 1, target: 2, size: 4);

            sBoxPacket.data[0] = sBoxDCM.mode;
            sBoxPacket.data[1] = sBoxDCM.resType;
            sBoxPacket.data[2] = sBoxDCM.bonusType;
            sBoxPacket.data[3] = sBoxDCM.jpType;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, DebugControlModeR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void DebugControlModeR(SBoxPacket sBoxPacket)
        {
            Debug.Log("算法调试模式设置成功" + sBoxPacket.data[0]);
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_DEBUG_CONTROL_MODE, sBoxPacket.data[0]);
        }

    }
}
