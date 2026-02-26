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
using System.Collections.Generic;

namespace SBoxApi
{
    public class SBoxBattleLeadData
    {
        public int result;

        public int LuckPlayerId;                    // 幸运玩家ID（1~10）
        public int LuckId;                          // 小游戏中奖类型
                                                    // 0：未中小游戏
                                                    // 1：保留
                                                    // 2：保留
                                                    // 3：保留
                                                    // 4：加倍
                                                    // 5：翻倍
                                                    // 6：明牌
                                                    // 7：大满贯（押分全中）
                                                    // 8：彩金

        public int[] Data;                          // 小游戏相应参数，不同类型有不同的内容定义
                                                    // 1，2，3: Data[0~15]保留
                                                    // 4: Data[0~15]为每个基础角色的加倍倍数
                                                    // 5: Data[0~15]为每个基础角色的翻倍倍数
                                                    // 6: Data[0]为明牌的值
                                                    // 8: Data[0~9]为本局每个玩家所中的彩金值
    }

    public class SBoxBattleStateData
    {
        public int result;                          // = 0: 正常
        public int[] TotalBets = new int[15];        // 5门当前总押分
        public int LuckPlayerId;                    // 非负时，幸运玩家ID
        public int LuckSelectIndex;                 // 非负时，玩家幸运选择索引
        public int PlayerWinLock;                   // 分机爆机，从0 bit开始，对应为玩家1，为1时表示对应玩机的爆机，
    }

    // 
    public enum SBOX_BATTLE_PLAYER_STATE_IN
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

    public class SBoxBattlePlayerInState
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

    public class SBoxBattlePlayerOutState
    {
        public int PlayerId;                        // 玩家ID
        public int LIndex;                          // 0~255的循环索引号
        public int rfu;                             // 玩家中小游戏得分
        public int credit;                          // 玩家账户分
        public int wins;                            // 玩家得分
        public int state;                           // 玩家状态信息
        public int seconds;                         // 倒计时（单位：秒）
        public int[] bets = new int[15];            // 玩家押分数据
        public int[] odds = new int[15];            // 玩家倍率表，需要除10
    }

    public partial class SBoxIdea
    {
        private static bool bOutStateListener = false;

        /**
		 *  @brief          读取游戏状态
		 *  @param          无
		 *  @return         sBoxPacket.data[0] = 0：成功
		 *                  sBoxPacket.data[0] < 0：发送参数错误
		 *                  sBoxPacket.data[0] > 0：状态码
		 *  				                        1：正在打印，其它：故障
		 *  @details        
		 */
        public static void BattleGetState()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20040, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BattleGetStateR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BattleGetStateR(SBoxPacket sBoxPacket)
        {
            SBoxBattleStateData sBoxBattleStateData = new SBoxBattleStateData();
            int pos = 0;
            int i = 0;
            sBoxBattleStateData.result = sBoxPacket.data[pos++];

            sBoxBattleStateData.LuckPlayerId = sBoxPacket.data[pos++];
            sBoxBattleStateData.LuckSelectIndex = sBoxPacket.data[pos++];
            sBoxBattleStateData.PlayerWinLock = sBoxPacket.data[pos++];

            while((pos < sBoxPacket.data.Length) && (i < sBoxBattleStateData.TotalBets.Length))
            {
                sBoxBattleStateData.TotalBets[i++] = sBoxPacket.data[pos++];
            }

            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_GET_STATE, sBoxBattleStateData);
        }

        /**
		 *  @brief          读取当前轮数和局数
		 *  @param          无
		 *  @return         sBoxPacket.data[0] = 0：成功
		 *                  sBoxPacket.data[0] < 0：发送参数错误
		 *                  sBoxPacket.data[0] > 0：状态码
		 *                  sBoxPacket.data[1]：当前是第几轮游戏
		 *                  sBoxPacket.data[2]：当前是第几局游戏
		 *  @details        
		 */
        public static void BattleGameNumber()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20041, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BattleGameNumberR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BattleGameNumberR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_GAME_NUMBER, sBoxPacket.data);
        }

        /**
		 *  @brief          读取已开奖的结果
		 *  @param          无
		 *  @return         sBoxPacket.data[0] = 0：成功
		 *                  sBoxPacket.data[0] < 0：发送参数错误
		 *                  sBoxPacket.data[0] > 0：状态码
		 *  				sBoxPacket.data[1~100]: 结果
		 *  				
		 *  @details        
		 */
        public static void BattleGetCompletedGame()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20042, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BattleGetCompletedGameR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BattleGetCompletedGameR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_GET_COMPLETED_GAME, sBoxPacket.data);
        }



        /**
		 *  @brief          重置指令，用于复原押分控台的显示等操作，本指令回返回每个角色的倍率表
		 *  @param          无
		 *  @return         odds[]
		 *  				[ 0] 黑桃,  [ 1] 红心,  [ 2] 梅花,  [ 3] 方块,  [ 4] 王
		 *  @details        
		 */
        public static void BattleResetGame()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20043, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BattleResetGameR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BattleResetGameR(SBoxPacket sBoxPacket)
        {
            double[] odds = new double[sBoxPacket.data.Length - 1];

            for(int i = 0; i < odds.Length; i++)
            {
                odds[i] = (double)sBoxPacket.data[i + 1] / (double)10.0;
            }
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_RESET_GAME, odds);
        }


        /**
		 *  @brief          开启新的一轮（100局），这时候算法卡会启动打印路单的过程
		 *  @param          无
		 *  @return         SBoxPacket.data[0] = 0：成功
		 *                  SBoxPacket.data[0] < 0：发送参数错误
		 *                  SBoxPacket.data[0] > 0：状态码
		 *  @details        
		 */
        public static void BattleNewRound()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20044, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BattleNewRoundR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BattleNewRoundR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_NEW_ROUND, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          中止当轮，同时会把当轮的所有开奖结果返回（100局），用于显示出来
		 *  @param          无
		 *  @return         SBoxPacket.data[0] = 0：成功
		 *                  SBoxPacket.data[0] < 0：发送参数错误
		 *                  SBoxPacket.data[0] > 0：状态码
		 *                  SBoxPacket.data[1~100]：开奖结果
		 *  @details        
		 */
        public static void BattleEndRound()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20045, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BattleEndRoundR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BattleEndRoundR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_END_ROUND, sBoxPacket.data);
        }

        /**
		 *  @brief          请求开奖结果，返回本局开奖结果和明牌信息
		 *  @param          seconds 倒计时秒
		 *  @return         sBoxPacket.data[0] = 0：成功，正常开押注开启
		 *                  sBoxPacket.data[0] < 0：发送参数错误
		 *                  sBoxPacket.data[0] > 0：状态码
		 *                  sBoxPacket.data[0] = 0：
		 *                  sBoxPacket.data[1]：结果，高4bits为花色：1：黑桃，2：红心，3：梅花，4：方块，5：王
		 *                                           低4bits为点数：1~13：A-2~10-J~K，14：小王，15：大王
		 *                  sBoxPacket.data[2]：当前是第几轮游戏
		 *                  sBoxPacket.data[3]：当前是第几局游戏
		 *                  sBoxPacket.data[4]：是否明牌
		 *                  
		 *  @details        
		 */
        public static void BattleRequestResult(int rfu)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20046, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BattleRequestResultR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BattleRequestResultR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_REQUEST_RESULT, sBoxPacket.data);
        }


        /**
		 *  @brief          开牌结束，由于以上开牌有动画过程，需在这里发出开牌结束指令，用于控台进行开牌，中奖的显示
		 *  @return         sBoxPacket.data[0] = 0：成功
		 *                  sBoxPacket.data[0] < 0：发送参数错误
		 *                  sBoxPacket.data[0] > 0：状态码
		 *                  
		 *  @details        
		 */
        public static void BattleLeadStop()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20048, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BattleLeadStopR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BattleLeadStopR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_LEAD_STOP, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          开牌
		 *  @return         sBoxBattleLeadData.result = 0：成功
		 *                  sBoxBattleLeadData.result < 0：发送参数错误
		 *                  sBoxBattleLeadData.result > 0：状态码
		 *                  
		 *  @details        
		 */
        public static void BattleLeadStart()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20047, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BattleLeadStartR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BattleLeadStartR(SBoxPacket sBoxPacket)
        {
            SBoxBattleLeadData sBoxBattleLeadData = new SBoxBattleLeadData();

            sBoxBattleLeadData.result = sBoxPacket.data[0];
            // = sBoxPacket.data[1]; 保留
            sBoxBattleLeadData.LuckId = sBoxPacket.data[2];
            sBoxBattleLeadData.LuckPlayerId = sBoxPacket.data[3];
            // = sBoxPacket.data[4~9]; 保留

            sBoxBattleLeadData.Data = new int[sBoxPacket.data.Length - 10];
            for (int i = 10; i < sBoxPacket.data.Length; i++)
            {
                sBoxBattleLeadData.Data[i - 10] = sBoxPacket.data[i];
            }

            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_LEAD_START, sBoxBattleLeadData);
        }

        /**
		 *  @brief          显示小游戏
		 *  @param          
		 *  @return         sBoxPacket.data[0] = 0：成功
		 *                  sBoxPacket.data[0] < 0：发送参数错误
		 *                  sBoxPacket.data[0] > 0：状态码
		 *                  
		 *  @details        
		 */
        public static void BattLuckShow()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20049, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BattLuckShowR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BattLuckShowR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_LUCK_SHOW, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          开小游戏
		 *  @return         sBoxPacket.data[0] = 0：成功
		 *                  sBoxPacket.data[0] < 0：发送参数错误
		 *                  sBoxPacket.data[0] > 0：状态码
		 *                  
		 *  @details        
		 */
        public static void BattLuckPrize()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20050, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BattLuckPrizeR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BattLuckPrizeR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_LUCK_PRIZE, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          打印机开箱
		 *  @param          PaperDoor 换纸门，true：开门，false：不动作
		 *                  TicketDoor 取单门，true：开门，false：不动作
		 *  @return         sBoxPacket.data[0] = 0：成功
		 *                  sBoxPacket.data[0] < 0：发送参数错误
		 *                  sBoxPacket.data[0] > 0：状态码
		 *                  
		 *  @details        
		 */
        public static void BattPrinterOpenBox(bool PaperDoor, bool TicketDoor)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20051, source: 1, target: 2, size: 2);

            if(PaperDoor == true)
            {
                sBoxPacket.data[0] = 1;
            }
            else
            {
                sBoxPacket.data[0] = 0;
            }

            if (TicketDoor == true)
            {
                sBoxPacket.data[1] = 1;
            }
            else
            {
                sBoxPacket.data[1] = 0;
            }

            SBoxIOEvent.AddListener(sBoxPacket.cmd, BattPrinterOpenBoxR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void BattPrinterOpenBoxR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_PRINTER_OPEN_BOX, sBoxPacket.data[0]);
        }

        /**
		 *  @brief          玩家产生的状态数据
		 *  @param          sBoxPlayerInState 见SBoxPlayerInState的描述
		 *  @return          
		 *  @details        本函数，在对应玩家（多个玩家时，需要拼接数据）数据变化时直接调用
		 *                  同时需要设定一个定时器，平时没变化时，每300毫秒自动调用
		 */
        public static void BattlePlayerInState(List<SBoxBattlePlayerInState> sBoxPlayerInState)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20052, source: 1, target: 2, size: sBoxPlayerInState.Count * 6);

            int pos = 0;
            int i;

            // 太多了，不处理，以免把算法卡搞崩了。在保证玩家编号不重复的情况下，保留最多20个玩家
            if (sBoxPlayerInState.Count > 20)
            {
                return;
            }

            for (i = 0; i < sBoxPlayerInState.Count; i++)
            {
                SBoxBattlePlayerInState state = sBoxPlayerInState[i];

                sBoxPacket.data[pos++] = state.PlayerId;
                sBoxPacket.data[pos++] = state.LIndex;
                sBoxPacket.data[pos++] = state.rfu;
                sBoxPacket.data[pos++] = state.state;
                sBoxPacket.data[pos++] = state.LNumberCoinIn;
                sBoxPacket.data[pos++] = state.LNumberCoinOut;
            }

            // No return
            //SBoxIOEvent.AddListener(sBoxPacket.cmd, BattlePlayerStateR);
            SBoxIOStream.Write(sBoxPacket);
        }
        // No return
        //private static void BattlePlayerStateR(SBoxPacket sBoxPacket)
        //{
        //}

        /**
		 *  @brief          玩家产生的状态数据
		 *  @param          sBoxPlayerInState 见SBoxPlayerInState的描述
		 *  @return          
		 *  @details        本函数，在对应玩家（多个玩家时，需要拼接数据）数据变化时直接调用
		 *                  同时需要设定一个定时器，平时没变化时，每300毫秒自动调用
		 */
        public static void BattlePlayerOutStateListener()
        {
            if(bOutStateListener != true)
            {
                bOutStateListener = true;
                SBoxIOEvent.AddListener(20053, OutStateMessageR);
            }
        }

        private static void OutStateMessageR(SBoxPacket sBoxPacket)
        {
            List<SBoxBattlePlayerOutState> sBoxBattlePlayerOutState = new List<SBoxBattlePlayerOutState>();

            sBoxBattlePlayerOutState.Clear();

            if((sBoxPacket.data.Length > 0) && ((sBoxPacket.data.Length % (15 * 2 + 7)) == 0))
            {
                int size = sBoxPacket.data.Length;
                int pos = 0;
                int i;

                while(size > 0)
                {
                    SBoxBattlePlayerOutState state = new SBoxBattlePlayerOutState();
                    state.PlayerId = sBoxPacket.data[pos++];
                    state.LIndex = sBoxPacket.data[pos++];
                    state.rfu = sBoxPacket.data[pos++];
                    state.credit = sBoxPacket.data[pos++];
                    state.wins = sBoxPacket.data[pos++];
                    state.state = sBoxPacket.data[pos++];
                    state.seconds = sBoxPacket.data[pos++];

                    for(i = 0;i < 15;i++)
                    {
                        state.bets[i] = sBoxPacket.data[pos++];
                    }
                    for (i = 0; i < 15; i++)
                    {
                        state.odds[i] = sBoxPacket.data[pos++];
                    }
                    size -= (15 * 2 + 7);
                    sBoxBattlePlayerOutState.Add(state);
                }
            }

            if(sBoxBattlePlayerOutState.Count > 0)
            {
                EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_BATTLE_PLAYER_OUT_STATE, sBoxBattlePlayerOutState);
            }
        }
    }
}
