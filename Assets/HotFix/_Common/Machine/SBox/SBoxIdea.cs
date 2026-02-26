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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBoxApi
{
    public enum SBOX_IDEA_PLAYER_STATE_IN
    {
        STATE_IN_KEY_A = (1 << 0),
        STATE_IN_KEY_B = (1 << 1),
        STATE_IN_KEY_C = (1 << 2),
        STATE_IN_KEY_D = (1 << 3),
        STATE_IN_KEY_E = (1 << 4),
        STATE_IN_KEY_PAYOUT = (1 << 5),
        STATE_IN_KEY_SCORE_UP = (1 << 6),
        STATE_IN_KEY_SCORE_DOWN = (1 << 7),
        STATE_IN_KEY_ROOT = (1 << 8),
        STATE_IN_KEY_SWITCH = (1 << 9),
        STATE_IN_KEY_ESC = (1 << 10),
        STATE_IN_KEY_SET = (1 << 11),

        STATE_IN_STATE_COIN_OUT_TIMEOUT = (1 << 14), // 退币器超时置1，否则置0
        STATE_IN_STATE_EXIST = (1 << 15),            // 本玩家在线时，这位置1，否则置0

        STATE_IN_KEY_F = (1 << 16),
        STATE_IN_KEY_G = (1 << 17),
        STATE_IN_KEY_H = (1 << 18),
        STATE_IN_KEY_I = (1 << 19),
        STATE_IN_KEY_J = (1 << 20),
        STATE_IN_KEY_K = (1 << 21),
        STATE_IN_KEY_L = (1 << 22),
        STATE_IN_KEY_M = (1 << 23),
        STATE_IN_KEY_N = (1 << 24),
        STATE_IN_KEY_O = (1 << 25),
    }

    public class SBoxIdeaPlayerInState
    {
        public int PlayerId;                        // 玩家ID
        public int LIndex;                          // 0~255的循环索引号
                                                    // 在BattlePlayerInState函数主动调用发送本玩家数据时，LIndex循环累加
                                                    // 在BattlePlayerInState函数每300毫秒被动调用本玩家数据时，LIndex不改变
        public int rfu;
        public int state;                           // 玩家状态信息，见SBOX_BATTLE_PLAYER_STATE_IN
        public int LNumberCoinIn;                   // 0~255的循环投币计数器
        public int LNumberCoinOut;                  // 0~255的循环退币计数器
    }

    public class SBoxIdeaPlayerOutState
    {
        public int PlayerId;                        // 玩家ID
        public int LIndex;                          // 0~255的循环索引号
        public int rfu;                             // 玩家中小游戏得分
        public int credit;                          // 玩家账户分
        public int wins;                            // 玩家得分
        public int state;                           // 玩家状态信息
        public int seconds;                         // 倒计时（单位：秒）
        public int[] bets = new int[SBoxIdea.MAX_SELECT];            // 玩家押分数据
        public int[] odds = new int[SBoxIdea.MAX_SELECT];            // 玩家倍率表，需要除10  注意:这个从跑酷开始不再设置值,app端不需要从这里读取所需的值
    }

    public class SBoxConfData
    {
        public int result;
        public int PwdType;                         // 0：无任何修改参数的权限，1：普通密码权限，2：管理员密码权限，3：超级管理员密码权限
        public int PlaceType;                       // 场地类型，0：普通，1：技巧，2：专家
        public int difficulty;                      // 难度，0~8
        public int odds;                            // 倍率，0：低倍率，1：高倍率，2：随机

        public int WinLock;                         // 盈利宕机
        public int MachineId;                       // 机台编号，8位有效十进制数
        public int LineId;                          // 线号，4位有效十进制数

        public int TicketMode;                      // 退票模式，0：即中即退，1：退票
        public int TicketValue;                     // 1票对应几分（彩票比例）
        public int scoreTicket;                     // 1分对应几票
        public int CoinValue;                       // 投币比例
        public int MaxBet;                          // 最大押注
        public int MinBet;                          // 最小押注
        public int CountDown;                       // 例计时
        public int MachineIdLock;                   // 1：机台号已锁定，除超级管理员外，无法更改，0：机台号未锁定
        public int BetsMinOfJackpot;                // 中彩金最小押分值
        public int JackpotStartValue;               // 彩金初始值
        public int LimitBetsWins;                   // 限红值，默认3000
        public int ReturnScore;                     // 返分值，500

        public int SwitchBetsUnitMin;               // 切换单位小，默认10
        public int SwitchBetsUnitMid;               // 切换单位中，默认50
        public int SwitchBetsUnitMax;               // 切换单位大，默认100

        public int ScoreUpUnit;                     // 上分单位
        public int PrintMode;                       // 打单模式，0：不打印，1：正常打印，2：伸缩打印
        public int ShowMode;                        // 显示模式，0：角色显示，1：花色显示
        public int CheckTime;                       // 对单时间
        public int OpenBoxTime;                     // 开箱时间
        public int PrintLevel;                      // 打印深度，0，1，2三级，0时最
        public int PlayerWinLock;                   // 分机爆机分数：默认100000
        public int LostLock;                        // 全台爆机分数：默认500000
        public int LostLockCustom;                  // 当轮爆机分数：默认300000
        public int PulseValue;                      // 脉冲比例
        public int NewGameMode;                     // 开始新一轮游戏模式，0：自动开始，1：手动开始
        public int NetJackpot;                      // 是否启用联网彩金 0:关闭 1:开启
        public int JackpotLevel;                    // 彩金等级，(0-6)
    }

    public class SBoxCoderData
    {
        public int result;

        public long Bets;                           // 玩家总押分
        public long Wins;                           // 玩家总赢分
        public int MachineId;                       // 机台编号，8位有效十进制数
        public int CoderCount;                      // 打码次数
        public int CheckValue;                      // 校验码
        public int RemainMinute;                    // 当前剩余时间（分钟）
    }

    public class SBoxPlayerBetsData
    {
        public int PlayerId;                        // 玩家ID
        public int balance;                         // 当前余分
        public int rfu;                             // 保留
        public int[] Bets = new int[SBoxIdea.MAX_SELECT];            // 本局15门押分
    }

    public class SBoxPlayerCoinInfo
    {
        public int PlayerId;                        // 玩家ID
        public int CoinIn;                          // 进币分数
        public int CoinOut;                         // 退币分数
        public int CoinOutWin;                      // 退币分数(即中即退，赢分部分)
        public int ScoreUp;                         // 上分数
        public int ScoreDown;                       // 下分数
    }

    public class SBoxWinNetJackpotInfo
    {
        public int PlayerId;                        // 玩家ID
        public int JackpotType;                     // 彩金类型 
        public int JackpotWins;                     // 彩金赢分
    }

    public class SBoxPlayerScoreInfo
    {
        public int PlayerId;                        // 玩家ID
        public int Score;                           // 当前分数
        public int Wins;                            // 当前赢分
        public int PointsMax;                       // 宝箱进度条最大值
        public int BoxPoints;                       // 宝箱累加积分
        public int[] BoxOpenFlags = new int[4];     // 宝箱状态
    }

    public class SBoxPermissionsData
    {
        public int result;
        public int permissions;                     // 0：无任何修改参数的权限，1：普通密码权限，2：管理员密码权限，3：超级管理员密码权限
    }

    public class SBoxPlayerAccount
    {
        public int PlayerId;                        // 玩家ID
        public int CoinIn;                          // 投币分
        public int CoinOut;                         // 退币分
        public int ScoreIn;                         // 上分
        public int ScoreOut;                        // 下分
        public int Credit;                          // 余额分

        public int Bets;                            // 总押分
        public int Wins;                            // 总赢分
    }

    public class SBoxAccount
    {
        public int result;

        public List<SBoxPlayerAccount> PlayerAccountList = new List<SBoxPlayerAccount>();
        public int DelayBets;                       // 延时打码总押
        public int DelayWins;                       // 延时打码总赢
    }

    public class SBoxIdeaInfo
    {
        public bool Ready;                         // 
        public int Jackpot;                        // 当前彩金值
        public int ActivatedCode;                  // 激活提示码，0：已激活，其它：需要激活
        public bool IsMachineIdReady;              // 机台号是否已设定
        public int RemainMinute;                   // 剩余分钟数
        public int WinLockBalance;                 // 盈利当机余额
        public int PrinterState;                   // 自定义打印机状态

        public bool PlayerScoreInfoLock;
        public List<SBoxPlayerScoreInfo> PlayerScoreInfoList = new List<SBoxPlayerScoreInfo>();
    }

    public class SBoxResetData
    {
        public int result;

        public int[] CoinToTable = new int[5];      // 落到桌面上各类型金币数量：
                                                    // [0]: 银币数量（1分）
                                                    // [1]: 金币数量（2分）
                                                    // [2]: 元宝数量（5分）
                                                    // [3]: 彩球数量（10分）
                                                    // [4]: 彩盘数量（20分）
    }

    public partial class SBoxIdea
    {
        private static bool bIdeaOutStateListener = false;

        public const int MAX_SELECT = 15;

        public enum SBOX_IDEA_STATE
        {
            STATE_NALMAL = 0,
            STATE_INACTIVE,          // 运行时间到
            STATE_WINLOCK,           // 盈利宕机
            STATE_DATA_LIMMIT,       // 数据量过大，超出可计算范围
            STATE_LOSTLOCK,          // 全台爆机
            STATE_LOSTLOCK_CUSTOM    // 自定义（本轮）爆机
        };
        private static SBoxIdeaInfo sBoxInfo = new SBoxIdeaInfo();
        private static SBoxAccount sBoxAccount = new SBoxAccount();

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
            SBoxIOEvent.AddListener(20001, MessageR);

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

        }

        /**
           *  @brief          算法主动上发的消息
           *  @param          sBoxPacket
           *  @return         无
           *  @details        
           */
        private static void MessageR(SBoxPacket sBoxPacket)
        {
            int tmp = 0;
            int pos = 0;

            sBoxInfo.Ready = true;

            // 算法卡系统状态1
            tmp = sBoxPacket.data[pos++];
            // 激活码
            sBoxInfo.ActivatedCode = tmp & 0x0f;
            if((tmp & (1 << 4)) != 0)
            {
                sBoxInfo.IsMachineIdReady = true;
            }
            else
            {
                sBoxInfo.IsMachineIdReady = false;
            }

            // 算法卡系统状态2
            tmp = sBoxPacket.data[pos++];
            sBoxInfo.PrinterState = tmp & 0xff;


            // 当前彩金
            sBoxInfo.Jackpot = sBoxPacket.data[pos++];
            // 剩余分钟数
            sBoxInfo.RemainMinute = sBoxPacket.data[pos++];
            // 盈利当机余额
            sBoxInfo.WinLockBalance = sBoxPacket.data[pos++];

            // 保留
            tmp = sBoxPacket.data[pos++];
            tmp = sBoxPacket.data[pos++];
            tmp = sBoxPacket.data[pos++];

            // 每个玩家的当前的积分信息
            while (pos < sBoxPacket.data.Length)
            {
                int playerIndex = (pos - 8) / 6;
                // playerIndex: 8, 14, 20, 26, 32, 38, 44, 50
                SBoxPlayerScoreInfo info;
                if (sBoxInfo.PlayerScoreInfoList.Count < playerIndex + 1)
                {
                    info = new SBoxPlayerScoreInfo();
                    sBoxInfo.PlayerScoreInfoList.Add(info);
                }
                else
                    info = sBoxInfo.PlayerScoreInfoList[playerIndex];
                info.PlayerId = sBoxPacket.data[pos++];
                info.Score = sBoxPacket.data[pos++];
                info.Wins = sBoxPacket.data[pos++];
                info.PointsMax = sBoxPacket.data[pos++];
                info.BoxPoints = sBoxPacket.data[pos++];
                tmp = sBoxPacket.data[pos++];
                for (int i = 0; i < 4; i++)
                    info.BoxOpenFlags[i] = (tmp >> i) & 1;
            }
        }

        /**
          *  @brief          检查算法卡是否连接
          *  @param          无
          *  @return         true or false
          *  @details        
          */
        public static bool Ready()
        {
            return SBoxIOStream.Connected((int)SBoxIOStream.SBoxIODevice.SBOX_DEVICE_IDEA) && sBoxInfo.Ready;
        }

        /**
          *  @brief          自定义打印机状态
          *  @param          无
          *  @return         自定义打印机状态
          *                  0：打印机空闲
          *                  1：打印机正在打印
          *                  2：打印机缺纸
          *                  3：打印机未连接
          *                  4：打印机切刀故障
          *                  5: 打印机门未关闭
          *                  其它：打印机未知故障
          *  @details        
          */
        public static int PrinterStateCustom()
        {
            return sBoxInfo.PrinterState;
        }

        /**
          *  @brief          当前彩金值
          *  @param          无
          *  @return         当前彩金值
          *  @details        
          */
        public static int Jackpot()
        {
            return sBoxInfo.Jackpot;
        }

        /**
          *  @brief          盈利当机余额
          *  @param          无
          *  @return         盈利当机余额，如果返回-1，无限制
          *  @details        
          */
        public static int WinLockBalance()
        {
            return sBoxInfo.WinLockBalance;
        }

        /**
          *  @brief          机器状态，非0时，需要激活
          *  @param          无
          *  @return         0时，正常，其它值为激活参数
          *  @details        
          */
        public static int NeedActivated()
        {
            return sBoxInfo.ActivatedCode;
        }

        /**
          *  @brief          机台号是否锁定，锁定状态下，需要超级管理员才可以修改
          *  @param          无
          *  @return         当前彩金值
          *  @details        
          */
        public static bool IsMachineIdReady()
        {
            return sBoxInfo.IsMachineIdReady;
        }

        /**
          *  @brief          剩余分钟数
          *  @param          无
          *  @return         剩余分钟数
          *  @details        
          */
        public static int RemainMinute()
        {
            return sBoxInfo.RemainMinute;
        }

        /**
          *  @brief          返回玩家的当前分数数据
          *  @param          
          *  @return         
          *  @details        
          */
        public static List<SBoxPlayerScoreInfo> PlayerScoreInfo
        {
            get { return sBoxInfo.PlayerScoreInfoList; }
        }

        /**
		 *  @brief          读取idea软件版本
		 *  @param          无
		 *  @return         版本字符串
		 *                  
		 *  @details        
		 */
        public static void Version()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 54, source: 1, target: 2, size: 1);

            sBoxPacket.data[0] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, VersionR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void VersionR(SBoxPacket sBoxPacket)
        {
            string version = "";

            for(int i = 0; i < sBoxPacket.data.Length; i++)
            {
                version += Convert.ToChar(sBoxPacket.data[i]);
            }

            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_IDEA_VERSION, version);
        }

        /**
		 *  @brief          app重启动后，重置算法卡本次启动的运行数据
		 *                  用于IDEA程序做一些必要的初始化操作
		 *  @param          无
		 *  @return         sBoxPacket.data[0] = 0：成功
		 *                  sBoxPacket.data[0] < 0：发送参数错误
		 *                  sBoxPacket.data[0] > 0：状态码, 1: 数据重置
		 *                  sBoxPacket.data[1]：推盘上银币数量
		 *                  sBoxPacket.data[2]：推盘上金币数量
		 *                  sBoxPacket.data[3]：推盘上元宝数量
		 *                  sBoxPacket.data[4]：推盘上彩球数量
		 *                  sBoxPacket.data[5]：推盘上彩盘数量
		 *  @details        
		 */
        public static void Reset()
        {
            Debug.Log("SBoxIdea 20000");
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20000, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, ResetR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void ResetR(SBoxPacket sBoxPacket)
        {
            int[] cointToTable = new int[5];
            for (int i = 0; i < 5; i++)
                cointToTable[i] = sBoxPacket.data[i + 1];
            SBoxResetData sBoxResetData = new SBoxResetData
            {
                result = sBoxPacket.data[0],
                CoinToTable = cointToTable
            };
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_RESET, sBoxResetData);
        }

        /**
		 *  @brief          读取游戏配置数据
		 *  @param          无
		 *  @return         SBoxConfData.result = 0：成功
		 *                  SBoxConfData.result < 0：发送参数错误
		 *                  SBoxConfData.result > 0：状态码
		 *  @details        
		 */
        public static void ReadConf()
        {
            Debug.Log("SBoxIdea 20002");
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20002, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, ReadConfR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void ReadConfR(SBoxPacket sBoxPacket)
        {
            SBoxConfData sBoxConfData = new SBoxConfData();
            int pos = 0;

            sBoxConfData.result = sBoxPacket.data[pos++];
            sBoxConfData.PwdType = sBoxPacket.data[pos++];
            sBoxConfData.PlaceType = sBoxPacket.data[pos++];
            sBoxConfData.difficulty = sBoxPacket.data[pos++];
            sBoxConfData.odds = sBoxPacket.data[pos++];
            sBoxConfData.WinLock = sBoxPacket.data[pos++];
            sBoxConfData.MachineId = sBoxPacket.data[pos++];
            sBoxConfData.LineId = sBoxPacket.data[pos++];
            sBoxConfData.TicketMode = sBoxPacket.data[pos++];
            sBoxConfData.TicketValue = sBoxPacket.data[pos++];
            sBoxConfData.scoreTicket = sBoxPacket.data[pos++];
            sBoxConfData.CoinValue = sBoxPacket.data[pos++];
            sBoxConfData.MaxBet = sBoxPacket.data[pos++];
            sBoxConfData.MinBet = sBoxPacket.data[pos++];
            sBoxConfData.CountDown = sBoxPacket.data[pos++];
            sBoxConfData.MachineIdLock = sBoxPacket.data[pos++];
            sBoxConfData.BetsMinOfJackpot = sBoxPacket.data[pos++];
            sBoxConfData.JackpotStartValue = sBoxPacket.data[pos++];
            sBoxConfData.LimitBetsWins = sBoxPacket.data[pos++];
            sBoxConfData.ReturnScore = sBoxPacket.data[pos++];

            sBoxConfData.SwitchBetsUnitMin = sBoxPacket.data[pos++];
            sBoxConfData.SwitchBetsUnitMid = sBoxPacket.data[pos++];
            sBoxConfData.SwitchBetsUnitMax = sBoxPacket.data[pos++];

            sBoxConfData.ScoreUpUnit = sBoxPacket.data[pos++];
            sBoxConfData.PrintMode = sBoxPacket.data[pos++];
            sBoxConfData.ShowMode = sBoxPacket.data[pos++];
            sBoxConfData.CheckTime = sBoxPacket.data[pos++];
            sBoxConfData.OpenBoxTime = sBoxPacket.data[pos++];
            sBoxConfData.PrintLevel = sBoxPacket.data[pos++];
            sBoxConfData.PlayerWinLock = sBoxPacket.data[pos++];
            sBoxConfData.LostLock = sBoxPacket.data[pos++];
            sBoxConfData.PulseValue = sBoxPacket.data[pos++];
            sBoxConfData.NewGameMode = sBoxPacket.data[pos++];
            sBoxConfData.NetJackpot = sBoxPacket.data[pos++];
            sBoxConfData.JackpotLevel = sBoxPacket.data[pos++];
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_READ_CONF, sBoxConfData);
        }

        /**
		 *  @brief          设定游戏配置数据
		 *  @param          sBoxIdeaConfData 游戏参数 
		 *  @return         SBoxBaseData.value[0] = 0：成功
		 *                  SBoxBaseData.value[0] < 0：发送参数错误
		 *                  SBoxBaseData.value[0] = 1：需验证密码
		 *                  SBoxBaseData.value[0] = 2：需打清0码后，再重新设定参数
		 *                  SBoxBaseData.value[1] = 0：无任何修改参数的权限
		 *                  SBoxBaseData.value[1] = 1：普通密码权限
		 *                  SBoxBaseData.value[1] = 2：管理员密码权限
		 *                  SBoxBaseData.value[1] = 3：超级管理员密码权限
		 *                  SBoxBaseData.value[1] = 4：需打清0码
		 *  @details        
		 */
        public static void WriteConf(SBoxConfData sBoxConfData)
        {
            Debug.Log("SBoxIdea 20003");
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20003, source: 1, target: 2, size: 64);
            int pos = 0;

            // pos = 0;
            sBoxPacket.data[pos++] = sBoxConfData.PlaceType;
            sBoxPacket.data[pos++] = sBoxConfData.difficulty;
            sBoxPacket.data[pos++] = sBoxConfData.odds;
            sBoxPacket.data[pos++] = sBoxConfData.WinLock;
            sBoxPacket.data[pos++] = sBoxConfData.MachineId;
            sBoxPacket.data[pos++] = sBoxConfData.LineId;
            sBoxPacket.data[pos++] = sBoxConfData.TicketMode;
            sBoxPacket.data[pos++] = sBoxConfData.TicketValue;
            sBoxPacket.data[pos++] = sBoxConfData.scoreTicket;
            sBoxPacket.data[pos++] = sBoxConfData.CoinValue;
            // pos = 10
            sBoxPacket.data[pos++] = sBoxConfData.MaxBet;
            sBoxPacket.data[pos++] = sBoxConfData.MinBet;
            sBoxPacket.data[pos++] = sBoxConfData.CountDown;
            sBoxPacket.data[pos++] = sBoxConfData.BetsMinOfJackpot;
            sBoxPacket.data[pos++] = sBoxConfData.JackpotStartValue;
            sBoxPacket.data[pos++] = sBoxConfData.LimitBetsWins;
            sBoxPacket.data[pos++] = sBoxConfData.ReturnScore;
            sBoxPacket.data[pos++] = sBoxConfData.SwitchBetsUnitMin;
            sBoxPacket.data[pos++] = sBoxConfData.SwitchBetsUnitMid;
            sBoxPacket.data[pos++] = sBoxConfData.SwitchBetsUnitMax;
            // pos = 20
            sBoxPacket.data[pos++] = sBoxConfData.ScoreUpUnit;
            sBoxPacket.data[pos++] = sBoxConfData.PrintMode;
            sBoxPacket.data[pos++] = sBoxConfData.ShowMode;
            sBoxPacket.data[pos++] = sBoxConfData.CheckTime;
            sBoxPacket.data[pos++] = sBoxConfData.OpenBoxTime;
            sBoxPacket.data[pos++] = sBoxConfData.PrintLevel;
            sBoxPacket.data[pos++] = sBoxConfData.PlayerWinLock;
            sBoxPacket.data[pos++] = sBoxConfData.LostLock;
            sBoxPacket.data[pos++] = sBoxConfData.PulseValue;
            sBoxPacket.data[pos++] = sBoxConfData.NewGameMode;
            // pos = 30
            sBoxPacket.data[pos++] = sBoxConfData.NetJackpot;
            sBoxPacket.data[pos++] = sBoxConfData.JackpotLevel;
            SBoxIOEvent.AddListener(sBoxPacket.cmd, WriteConfR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void WriteConfR(SBoxPacket sBoxPacket)
        {
            SBoxPermissionsData sBoxPermissionsData = new SBoxPermissionsData()
            {
                result = sBoxPacket.data[0],
                permissions = sBoxPacket.data[1]
            };
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_WRITE_CONF, sBoxPermissionsData);
        }

        /**
		 *  @brief          密码验证
		 *  @param[in]      password 密码值（十进制），
		 *                  6位有效数字：普通密码，
		 *                  8位有效数字：管理员密码，
		 *                  9位有效数字：超级管理员密码
		 *  @return         SBoxBaseData.value[0] = 0：成功
		 *                  SBoxBaseData.value[0] < 0：发送参数错误
		 *                  SBoxBaseData.value[0] = 1：需验证密码
		 *                  SBoxBaseData.value[1] = 0：无任何修改参数的权限
		 *                  SBoxBaseData.value[1] = 1：普通密码权限
		 *                  SBoxBaseData.value[1] = 2：管理员密码权限
		 *                  SBoxBaseData.value[1] = 3：超级管理员密码权限
		 *  @details        1、IDEA需实现防止穷举的功能，连续5次失败后，就要等机器连续运行30分钟后才能再次验证密码。
         *                  2、需要验证密码的数据操作，一定要在通过了密码验证后才可以操作，并有20分钟限制，超时失效，需再次验证。
		 */
        public static void CheckPassword(int password)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20004, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = password;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, CheckPasswordR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void CheckPasswordR(SBoxPacket sBoxPacket)
        {
            SBoxPermissionsData sBoxPermissionsData = new SBoxPermissionsData()
            {
                result = sBoxPacket.data[0],
                permissions = sBoxPacket.data[1]
            };
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_CHECK_PASSWORD, sBoxPermissionsData);
        }

        /**
		 *  @brief          修改密码
		 *  @param[in]      password 密码值（十进制），
		 *                  6位有效数字：普通密码，
		 *                  8位有效数字：管理员密码，
		 *                  9位有效数字：超级管理员密码
		 *  @return         SBoxBaseData.value[0] = 0：成功
		 *                  SBoxBaseData.value[0] < 0：发送参数错误
		 *                  SBoxBaseData.value[0] = 1：需验证密码
		 *                  SBoxBaseData.value[1] = 0：无任何修改参数的权限
		 *                  SBoxBaseData.value[1] = 1：普通密码权限
		 *                  SBoxBaseData.value[1] = 2：管理员密码权限
		 *                  SBoxBaseData.value[1] = 3：超级管理员密码权限
		 *  @details        1、IDEA需实现防止穷举的功能，连续5次失败后，就要等机器连续运行30分钟后才能再次验证密码。
         *                  2、需要验证密码的数据操作，一定要在通过了密码验证后才可以操作，并有20分钟限制，超时失效，需再次验证。
		 */
        public static void ChangePassword(int password)
        {
            Debug.Log("SBoxIdea 20005");
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20005, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = password;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, ChangePasswordR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void ChangePasswordR(SBoxPacket sBoxPacket)
        {
            SBoxPermissionsData sBoxPermissionsData = new SBoxPermissionsData()
            {
                result = sBoxPacket.data[0],
                permissions = sBoxPacket.data[1]
            };
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_CHANGE_PASSWORD, sBoxPermissionsData);
        }

        /**
		 *  @brief          请求打码
		 *  @param          flag 打码的类似
		 *  @return         SBoxCoderData.result = 0：成功
		 *                  SBoxCoderData.result < 0：发送参数错误
		 *                  SBoxCoderData.result = 1：需验证密码
		 *  @details        
		 */
        public static void RequestCoder(int flag)
        {
            Debug.Log("SBoxIdea 20006");
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20006, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = flag;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, RequestCoderR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void RequestCoderR(SBoxPacket sBoxPacket)
        {
            SBoxCoderData sBoxCoderData = new SBoxCoderData();
            sBoxCoderData.result = sBoxPacket.data[0];

            sBoxCoderData.Bets = (long)sBoxPacket.data[1];
            sBoxCoderData.Bets <<= 32;
            sBoxCoderData.Bets += (long)sBoxPacket.data[2];

            sBoxCoderData.Wins = (long)sBoxPacket.data[3];
            sBoxCoderData.Wins <<= 32;
            sBoxCoderData.Wins += (long)sBoxPacket.data[4];

            sBoxCoderData.MachineId = sBoxPacket.data[5];
            sBoxCoderData.CoderCount = sBoxPacket.data[6];
            sBoxCoderData.CheckValue = sBoxPacket.data[7];
            sBoxCoderData.RemainMinute = sBoxPacket.data[8];

            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_REQUEST_CODER, sBoxCoderData);
        }

        /**
		 *  @brief          打码
		 *  @param[in]      flag 打码的类似
		 *  @param[in]      Code 无符号64位整型数据码
		 *  @return         SBoxBaseData.value[0] = 0：成功
		 *                  SBoxBaseData.value[0] < 0：发送参数错误
		 *                  SBoxBaseData.value[0] = 1：需验证密码
		 *                  SBoxBaseData.value[1] = 0：无任何修改参数的权限
		 *                  SBoxBaseData.value[1] = 1：普通密码权限
		 *                  SBoxBaseData.value[1] = 2：管理员密码权限
		 *                  SBoxBaseData.value[1] = 3：超级管理员密码权限
		 *  @details        
		 */
        public static void Coder(int flag, ulong Code)
        {
            //Debug.LogError($"调用 SBoxIdea.Coder = flag:{flag} Code:{Code}");
            Debug.Log("SBoxIdea 20007");
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20007, source: 1, target: 2, size: 4);
            uint tmp = (uint)Code;

            sBoxPacket.data[0] = flag;
            sBoxPacket.data[1] = (int)tmp;
            Code >>= 32;
            tmp = (uint)Code;
            sBoxPacket.data[2] = (int)tmp;
            sBoxPacket.data[3] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, CoderR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void CoderR(SBoxPacket sBoxPacket)
        {
            SBoxPermissionsData sBoxPermissionsData = new SBoxPermissionsData()
            {
                result = sBoxPacket.data[0],
                permissions = sBoxPacket.data[1]
            };
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_CODER, sBoxPermissionsData);
        }
        /**
		 *  @brief          获取每一门的倍率
		 *  @param          无
		 *  @return         data[0] = 0：成功, < 0: 发送参数错误, > 0: 状态码
		 *                  不同的游戏，值的数量和内容不一样，具体请根据相应的游戏来解释相关的数据
		 *                  如：飞龙在天的如下：
		 *  				[ 1] 红唐僧，    [ 2] 绿唐僧,   [ 3] 黄唐僧
		 *  		        [ 4] 红孙悟空，  [ 5] 绿孙悟空，[ 6] 黄孙悟空
		 *  		        [ 7] 红猪八戒，  [ 8] 绿猪八戒，[ 9] 黄猪八戒
		 *  		        [10] 红沙和尚，  [11] 绿沙和尚，[12] 黄沙和尚
		 *  		        [13] 红龙最小值，[14] 红龙最大值
		 *  		        [15] 金龙最小值，[16] 金龙最大值
		 *  		        [17] 绿龙最小值，[18] 绿龙最大值
		 *  @details        
		 */
        public static void GetOdds()
        {
            Debug.Log("SBoxIdea 20008");
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20008, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, GetOddsR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void GetOddsR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_GET_ODDS, sBoxPacket.data);
        }

        /**
		 *  @brief          设定玩家押分数据
		 *  @param          sBoxPlayerBetsData 玩家的押注数据
		 *  @return         SBoxBaseData.value[0] = 0：成功
		 *                  SBoxBaseData.value[0] < 0：发送参数错误
		 *                  SBoxBaseData.value[0] > 0：状态码
		 *  @details        
		 */
        public static void SetPlayerBets(SBoxPlayerBetsData sBoxPlayerBetsData)
        {
            Debug.Log("SBoxIdea 20012");
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20012, source: 1, target: 2, size: sBoxPlayerBetsData.Bets.Length + 3);

            sBoxPacket.data[0] = sBoxPlayerBetsData.PlayerId;
            sBoxPacket.data[1] = sBoxPlayerBetsData.balance;
            sBoxPacket.data[2] = sBoxPlayerBetsData.rfu;
            for(int i = 3; i < sBoxPacket.data.Length;i++)
            {
                sBoxPacket.data[i] = sBoxPlayerBetsData.Bets[i - 3];
            }

            SBoxIOEvent.AddListener(sBoxPacket.cmd, SetPlayerBetsR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void SetPlayerBetsR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SET_PLAYER_BETS, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          玩家进币和退币信息，注意：不要有重复的玩家编号，函数使用完记得要清空列表
		 *  @param          sBoxPlayerCoinInfos 玩家信息列表
		 *                  sBoxPlayerCoinInfos.PlayerId: 玩家索引编号，从0开始算
		 *                  sBoxPlayerCoinInfos.CoinIn: 玩家进币分数
		 *                  sBoxPlayerCoinInfos.CoinOut: 玩家退币分数
		 *                  sBoxPlayerCoinInfos.CoinOutWin: 玩家退币分数(即中即退，赢分部分)
		 *                  sBoxPlayerCoinInfos.UpScore: 玩家上分数
		 *                  sBoxPlayerCoinInfos.DownScore: 玩家下分数
		 *  @return         无返回
		 *  @details        
		 */
        public static void SetPlayerCoinInfo(List<SBoxPlayerCoinInfo> sBoxPlayerCoinInfos)
        {
            Debug.Log("SBoxIdea 20015");
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20015, source: 1, target: 2, size: sBoxPlayerCoinInfos.Count * 6);
            int pos = 0;
            int i;

            // 太多了，不处理，以免把算法卡搞崩了。在保证玩家编号不重复的情况下，保留最多16个玩家
            if(sBoxPlayerCoinInfos.Count > 16)
            {
                return;
            }

            for (i = 0; i < sBoxPlayerCoinInfos.Count; i++)
            {
                SBoxPlayerCoinInfo info = sBoxPlayerCoinInfos[i];

                sBoxPacket.data[pos++] = info.PlayerId;
                sBoxPacket.data[pos++] = info.CoinIn; 
                sBoxPacket.data[pos++] = info.CoinOut;
                sBoxPacket.data[pos++] = info.CoinOutWin;
                sBoxPacket.data[pos++] = info.ScoreUp;
                sBoxPacket.data[pos++] = info.ScoreDown;
            }
            SBoxIOStream.Write(sBoxPacket);
        }

        /**
		 *  @brief          读取账目
		 *  @param          无
		 *  @return         SBoxPrizePlayerData.result = 0：成功
		 *                  SBoxPrizePlayerData.result < 0：发送参数错误
		 *                  SBoxPrizePlayerData.result > 0：状态码
		 *  @details        
		 */
        public static void GetAccount()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20016, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, GetAccountR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void GetAccountR(SBoxPacket sBoxPacket)
        {
            int pos = 0;

            sBoxAccount.result = sBoxPacket.data[pos++];

            sBoxAccount.PlayerAccountList.Clear();
            while (pos < sBoxPacket.data.Length - 2)
            {
                SBoxPlayerAccount Account = new SBoxPlayerAccount()
                {
                    PlayerId = sBoxPacket.data[pos++],
                    CoinIn = sBoxPacket.data[pos++],
                    CoinOut = sBoxPacket.data[pos++],
                    ScoreIn = sBoxPacket.data[pos++],
                    ScoreOut = sBoxPacket.data[pos++],
                    Credit = sBoxPacket.data[pos++],
                    Bets = sBoxPacket.data[pos++],
                    Wins = sBoxPacket.data[pos++],
                };
                sBoxAccount.PlayerAccountList.Add(Account);
            }
            //sBoxAccount.DelayBets = sBoxPacket.data[pos++];
           //sBoxAccount.DelayWins = sBoxPacket.data[pos++];
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_GET_ACCOUNT, sBoxAccount);
        }

        /**
		 *  @brief          把玩家得分移到总分
		 *  @param          PlayerId 玩家的编号, 编号从0开始
		 *  @return         SBoxBaseData.value[0] = 0：成功
		 *                  SBoxBaseData.value[0] < 0：发送参数错误
		 *                  SBoxBaseData.value[0] > 0：状态码
		 *  @details        
		 */
        public static void MovePlayerScore(List<int> PlayerIdList)
        {
            Debug.Log("SBoxIdea 20017");
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20017, source: 1, target: 2, size: PlayerIdList.Count);
            int pos = 0;

            // 太多了，不处理，以免把算法卡搞崩了。在保证玩家编号不重复的情况下，保留最多16个玩家
            if (PlayerIdList.Count > 16)
            {
                return;
            }

            for (int i = 0; i < PlayerIdList.Count; i++)
            {
                sBoxPacket.data[pos++] = PlayerIdList[i];
            }

            SBoxIOEvent.AddListener(sBoxPacket.cmd, MovePlayerScoreR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void MovePlayerScoreR(SBoxPacket sBoxPacket)
        {
            if(sBoxPacket.data.Length > 0)
            {
                List<int> PlayerIdList = new List<int>();
                for (int i = 1; i < sBoxPacket.data.Length; i++)
                {
                    PlayerIdList.Add(sBoxPacket.data[i]);
                }
                EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_MOVE_PLAYER_SCORE, PlayerIdList);
            }
        }

        /**
		 *  @brief          请求是否可以开始新的一局
		 *  @param          无
		 *  @return         SBoxBaseData.value[0] = 0：成功
		 *                  SBoxBaseData.value[0] < 0：发送参数错误
		 *                  SBoxBaseData.value[0] > 0：激活状态码，等同NeedActivated函数返回值
		 *  @details        
		 */
        public static void RequestStart()
        {
            Debug.Log("SBoxIdea 20018");
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20018, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, RequestStartR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void RequestStartR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_REQUEST_START, sBoxPacket.data[0]);
        }


        /**
		 *  @brief          开始押注
		 *  @param          seconds 倒计时秒
		 *  @return         sBoxPacket.data[0] = 0：成功，开始押注
		 *                  sBoxPacket.data[0] < 0：发送参数错误
		 *                  sBoxPacket.data[0] > 0：状态码
		 *                  
		 *  @details        
		 */
        public static void BetsStart(int seconds)
        {
            Debug.Log("SBoxIdea 20019");
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20019, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = seconds;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BetsStartR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BetsStartR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BETS_START, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          停止押注
		 *  @param          无
		 *  @return         sBoxPacket.data[0] = 0：成功
		 *                  sBoxPacket.data[0] < 0：发送参数错误
		 *                  sBoxPacket.data[0] > 0：状态码
		 *  @details        
		 */
        public static void BetsStop()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20020, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BetsStopR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BetsStopR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BETS_STOP, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          倒计时
		 *  @param          seconds 当前剩余秒
		 *  @return         sBoxPacket.data[0] = 0：成功
		 *                  sBoxPacket.data[0] < 0：发送参数错误
		 *                  sBoxPacket.data[0] > 0：状态码
		 *  @details        
		 */
        public static void BetsCountDown(int seconds)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20021, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = seconds;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BetsCountDownR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BetsCountDownR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATS_COUNT_DOWN, sBoxPacket.data[0]);
        }

        /**
        *  @brief          玩家赢得彩金中心的彩金信息
        *  @param          sBoxWinNetJackpotInfo.PlayerId: 玩家索引编号
        *                  sBoxWinNetJackpotInfo.JackpotType: 获得的彩金类型
        *                  sBoxWinNetJackpotInfo.JackpotWins: 玩家彩金的赢分(从彩金中心传过来的是币的数量, 需要乘币值)
        *  @return         无返回
        *  @details        
        */
        public static void SetPlayerWinNetJackpotInfo(SBoxWinNetJackpotInfo sBoxWinNetJackpotInfo)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20030, source: 1, target: 2, size: 3);
            int pos = 0;

            sBoxPacket.data[pos++] = sBoxWinNetJackpotInfo.PlayerId;
            sBoxPacket.data[pos++] = sBoxWinNetJackpotInfo.JackpotType;
            sBoxPacket.data[pos++] = sBoxWinNetJackpotInfo.JackpotWins;

            SBoxIOStream.Write(sBoxPacket);
        }

        /**
        *  @brief          设置基础游戏的上下拉局数
        *  @param          operate 0:读取上下拉局数
        *                  GameCount 1:设置上下拉局数
        *                  sBoxWinNetJackpotInfo.JackpotWins: 玩家彩金的赢分(从彩金中心传过来的是币的数量, 需要乘币值)
        *  @return         无返回
        *  @details        
        */
        public static void WaveGameCount(int operate, int GameCount)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20031, source: 1, target: 2, size: 2);
            int pos = 0;
            sBoxPacket.data[pos++] = operate;
            sBoxPacket.data[pos++] = GameCount;
            SBoxIOEvent.AddListener(sBoxPacket.cmd, WaveGameCountR);
            SBoxIOStream.Write(sBoxPacket);
        }

        private static void WaveGameCountR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_WAVE_GAME_COUNT, sBoxPacket.data);
        }

        /*
         * 以下函数从单挑那里移到此处，后续单挑对应的20052和20053将使用这里的接口
         */
        /**
       *  @brief          玩家产生的状态数据
       *  @param          sBoxPlayerInState 见SBoxIdeaPlayerInState的描述
       *  @return          
       *  @details        本函数，在对应玩家（多个玩家时，需要拼接数据）数据变化时直接调用
       *                  同时需要设定一个定时器，平时没变化时，每300毫秒自动调用
       */
        public static void IdeaPlayerInState(List<SBoxIdeaPlayerInState> sBoxPlayerInState)
        {
            //Debug.Log($"IdeaPlayerInState:{JsonConvert.SerializeObject(sBoxPlayerInState)}");
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20052, source: 1, target: 2, size: sBoxPlayerInState.Count * 2);

            int pos = 0;
            int i;

            // 太多了，不处理，以免把算法卡搞崩了。在保证玩家编号不重复的情况下，保留最多20个玩家
            if (sBoxPlayerInState.Count > 20)
            {
                return;
            }
            
            for (i = 0; i < sBoxPlayerInState.Count; i++)
            {
                SBoxIdeaPlayerInState state = sBoxPlayerInState[i];
                sBoxPacket.data[pos++] = (((state.rfu << 8) | (state.LIndex << 4) | state.PlayerId) << 16) | (state.LNumberCoinIn | (state.LNumberCoinOut << 8));
                sBoxPacket.data[pos++] = state.state;
            }

            // No return
            //SBoxIOEvent.AddListener(sBoxPacket.cmd, BattlePlayerStateR);
            SBoxIOStream.Write(sBoxPacket);
        }

        /**
       *  @brief          玩家产生的状态数据
       *  @param          sBoxPlayerInState 见SBoxPlayerInState的描述
       *  @return          
       *  @details        本函数，在对应玩家（多个玩家时，需要拼接数据）数据变化时直接调用
       *                  同时需要设定一个定时器，平时没变化时，每300毫秒自动调用
       */
        public static void IdeaPlayerOutStateListener()
        {
            if (bIdeaOutStateListener != true)
            {
                bIdeaOutStateListener = true;
                SBoxIOEvent.AddListener(20053, IdeaOutStateMessageR);
            }
        }

        private static void IdeaOutStateMessageR(SBoxPacket sBoxPacket)
        {
            List<SBoxIdeaPlayerOutState> sBoxIdeaPlayerOutState = new List<SBoxIdeaPlayerOutState>();

            sBoxIdeaPlayerOutState.Clear();

            int dataSize = MAX_SELECT * 2 + 4; 
            if ((sBoxPacket.data.Length > 0) && ((sBoxPacket.data.Length % dataSize) == 0))
            {
                int size = sBoxPacket.data.Length;
                int pos = 0;
                int i;
                int outData = 0;
                while (size > 0)
                {
                    SBoxIdeaPlayerOutState state = new SBoxIdeaPlayerOutState();
                    outData = sBoxPacket.data[pos++];
                    state.PlayerId = outData & 0x0F; // 取出玩家编号
                    state.LIndex = (outData >> 4) & 0x0F;
                    state.rfu = (outData >> 8) & 0x0F; // 保留位
                    state.seconds = (outData >> 16) & 0xFF; // 保留位
                    state.credit = sBoxPacket.data[pos++];
                    state.wins = sBoxPacket.data[pos++];
                    state.state = sBoxPacket.data[pos++];
                 

                    for (i = 0; i < MAX_SELECT; i++)
                    {
                        state.bets[i] = sBoxPacket.data[pos++];
                    }
                    for (i = 0; i < MAX_SELECT; i++)
                    {
                        state.odds[i] = sBoxPacket.data[pos++];
                    }
                    size -= dataSize;
                    sBoxIdeaPlayerOutState.Add(state);
                }
            }

            if (sBoxIdeaPlayerOutState.Count > 0)
            {
                //Debug.LogWarning($"SBOX_PLAYER_OUT_STATE:{JsonConvert.SerializeObject(sBoxIdeaPlayerOutState)}");
                EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_PLAYER_OUT_STATE, sBoxIdeaPlayerOutState);
            }
        }
    }
}