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
    public class SBoxSicBoData
    {
        public int result;                    // 返回时：指令的执行状态码

        public int[] Number = new int[3 + 5]; // 3：3个色子的点数，5：是保留用

        public int[] Data = new int[50 + 8];  // 50：每个类型游戏的押注数（返回时：每个类型游戏的翻倍数），8：是保留用
    }


    public class SBoxIdeaSummarySicBoData
    {
        public List<SummaryItem> waveUp = new List<SummaryItem>();          // 波动上拉
        public List<SummaryItem> waveDown = new List<SummaryItem>();        // 波动下拉
        public List<SummaryItem> role = new List<SummaryItem>();            // 12门角色
        public List<SummaryItem> dragon = new List<SummaryItem>();          // 三条龙

        public List<SummaryItem> totalWave = new List<SummaryItem>();       // 波动 + 15门
        public List<SummaryItem> littleGame = new List<SummaryItem>();      // 小游戏

        public List<SummaryItem> superTimes = new List<SummaryItem>();      // 超级加倍
        public List<SummaryItem> JP = new List<SummaryItem>();              // 超级翻倍
        public List<SummaryItem> freeRole = new List<SummaryItem>();        // 欢乐送灯
        public List<SummaryItem> allPick = new List<SummaryItem>();         // 大满贯

        public SummaryItemExtra jackpot = new SummaryItemExtra();           // 彩金
        public SummaryItemExtra proposeScore = new SummaryItemExtra();      // 提拨分数

        public List<SummaryItem> box = new List<SummaryItem>();             // 宝箱
        public List<SummaryItem> reward = new List<SummaryItem>();          // 开奖局数

        public SummaryWave wave = new SummaryWave();                        // 波动级数

        public SummaryBetsInfo betsInfo = new SummaryBetsInfo();            // 机率，自备款，比例，20局押分

        public SummarySegValue segValue = new SummarySegValue();            // 段值，800局计数器

        public List<SummaryLittleScore> littleScore = new List<SummaryLittleScore>(); // 加倍，翻倍，送灯，满贯分数表
    }



    public partial class SBoxIdea
    {
        public static SBoxIdeaSummarySicBoData sBoxSummaryData = new SBoxIdeaSummarySicBoData();

        /**
		 *  @brief          倒时间结束时调用本函数请求小游戏
		 *  @param          sBoxSicBoData.Number: 任意值，无效
		 *                  sBoxSicBoData.Data[0~49]: 每一门的押分
		 *  @return         sBoxSicBoData.result = 0：成功
		 *                  sBoxSicBoData.result < 0：发送参数错误
		 *                  sBoxSicBoData.result > 0：状态码
		 *                  sBoxSicBoData.Number[0]: 返分限值
		 *                  sBoxSicBoData.Number[1]: 当前返分值
		 *                  sBoxSicBoData.Number[2]: 本局返分值
		 *                  sBoxSicBoData.Data[0~49]: 每一门的翻倍数，0时表示没有翻倍
		 *  @details        
		 */
        public static void SicBoRequstGoodLuck(SBoxSicBoData sBoxSicBoData)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20063, source: 1, target: 2, size: 67);
            int pos = 0;

            sBoxPacket.data[pos++] = sBoxSicBoData.result;
            for (int i = 0; i < sBoxSicBoData.Number.Length; i++)
            {
                sBoxPacket.data[pos++] = sBoxSicBoData.Number[i];
            }
            for (int i = 0; i < sBoxSicBoData.Data.Length; i++)
            {
                sBoxPacket.data[pos++] = sBoxSicBoData.Data[i];
            }

            SBoxIOEvent.AddListener(sBoxPacket.cmd, SicBoRequstGoodLuckR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void SicBoRequstGoodLuckR(SBoxPacket sBoxPacket)
        {
            SBoxSicBoData sBoxSicBoData = new SBoxSicBoData();
            int pos = 0;

            sBoxSicBoData.result = sBoxPacket.data[pos++];
            for(int i = 0;i < sBoxSicBoData.Number.Length; i++)
            {
                sBoxSicBoData.Number[i] = sBoxPacket.data[pos++];
            }
            for (int i = 0; i < sBoxSicBoData.Data.Length; i++)
            {
                sBoxSicBoData.Data[i] = sBoxPacket.data[pos++];
            }

            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SICBO_REQUEST_GOODLUCK, sBoxSicBoData);
        }

        /**
		 *  @brief          色子出结果后调用给算法卡核算
		 *  @param          sBoxSicBoData.Number[0~2]: 三个色子的点数
		 *                  sBoxSicBoData.Data[0~49]: 每一门的押分
		 *  @return         sBoxSicBoData.result = 0：成功
		 *                  sBoxSicBoData.result < 0：发送参数错误
		 *                  sBoxSicBoData.result > 0：状态码
		 *                  sBoxSicBoData.Data[0~49]: 每一门是否中奖
		 *  @details        
		 */
        public static void SicBoCalculate(SBoxSicBoData sBoxSicBoData)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20064, source: 1, target: 2, size: 67);
            int pos = 0;

            sBoxPacket.data[pos++] = sBoxSicBoData.result;
            for (int i = 0; i < sBoxSicBoData.Number.Length; i++)
            {
                sBoxPacket.data[pos++] = sBoxSicBoData.Number[i];
            }
            for (int i = 0; i < sBoxSicBoData.Data.Length; i++)
            {
                sBoxPacket.data[pos++] = sBoxSicBoData.Data[i];
            }

            SBoxIOEvent.AddListener(sBoxPacket.cmd, SicBoCalculateR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void SicBoCalculateR(SBoxPacket sBoxPacket)
        {
            SBoxSicBoData sBoxSicBoData = new SBoxSicBoData();
            int pos = 0;

            sBoxSicBoData.result = sBoxPacket.data[pos++];
            for (int i = 0; i < sBoxSicBoData.Number.Length; i++)
            {
                sBoxSicBoData.Number[i] = sBoxPacket.data[pos++];
            }
            for (int i = 0; i < sBoxSicBoData.Data.Length; i++)
            {
                sBoxSicBoData.Data[i] = sBoxPacket.data[pos++];
            }
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SICBO_CALCULATE, sBoxSicBoData);
        }


        /**
		 *  @brief          读取算法统计数据，会有多个数据包返回，TOO BAD !!!
		 *  @param          无
		 *  @return         SBoxBaseData.value[0] = 0：成功
		 *                  SBoxBaseData.value[0] < 0：发送参数错误
		 *                  SBoxBaseData.value[0] > 0：状态码
		 *  @details        
		 */
        public static void GetSummary()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20014, source: 1, target: 2, size: 4);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;
            sBoxPacket.data[2] = 0;
            sBoxPacket.data[3] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, GetSummaryR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void GetSummaryR(SBoxPacket sBoxPacket)
        {
            bool bTrigger = false;
            int pos = 0;

            if (sBoxPacket.data[0] != 0)
            {
                //sBoxSummaryData.result = sBoxPacket.data[0];
                bTrigger = true;
            }
            else
            {
                switch (sBoxPacket.data[1])
                {
                    // -------------------------------------------------------------------------------------
                    // 波动上拉
                    case 1:
                        pos = 2;
                        sBoxSummaryData.waveUp.Clear();
                        for (int i = 0; i < 13; i++)
                        {
                            SummaryItem data = new SummaryItem()
                            {
                                index = i,
                                counter = sBoxPacket.data[pos++],
                                bets = sBoxPacket.data[pos++],
                                wins = sBoxPacket.data[pos++],
                                odds = (double)sBoxPacket.data[pos++] / 1000000.0,
                            };
                            sBoxSummaryData.waveUp.Add(data);
                        }
                        break;

                    // -------------------------------------------------------------------------------------
                    // 波动下拉
                    case 2:
                        pos = 2;
                        sBoxSummaryData.waveDown.Clear();
                        for (int i = 0; i < 13; i++)
                        {
                            SummaryItem data = new SummaryItem()
                            {
                                index = i,
                                counter = sBoxPacket.data[pos++],
                                bets = sBoxPacket.data[pos++],
                                wins = sBoxPacket.data[pos++],
                                odds = (double)sBoxPacket.data[pos++] / 1000000.0,
                            };
                            sBoxSummaryData.waveDown.Add(data);
                        }
                        break;

                    // -------------------------------------------------------------------------------------
                    // 基础角色，龙
                    case 3:
                        pos = 2;
                        sBoxSummaryData.role.Clear();
                        for (int i = 0; i < 15; i++)
                        {
                            SummaryItem data = new SummaryItem()
                            {
                                index = i,
                                counter = sBoxPacket.data[pos++],
                                bets = sBoxPacket.data[pos++],
                                wins = sBoxPacket.data[pos++],
                                odds = (double)sBoxPacket.data[pos++] / 1000000.0,
                            };
                            sBoxSummaryData.role.Add(data);
                        }

                        sBoxSummaryData.dragon.Clear();
                        for (int i = 0; i < 4; i++)
                        {
                            SummaryItem data = new SummaryItem()
                            {
                                index = i,
                                counter = sBoxPacket.data[pos++],
                                bets = sBoxPacket.data[pos++],
                                wins = sBoxPacket.data[pos++],
                                odds = (double)sBoxPacket.data[pos++] / 1000000.0,
                            };
                            sBoxSummaryData.dragon.Add(data);
                        }
                        break;

                    // -------------------------------------------------------------------------------------
                    // 超级加倍，超级翻倍，欢乐送灯，大满贯
                    case 4:
                        pos = 2;
                        sBoxSummaryData.superTimes.Clear();
                        for (int i = 0; i < 6; i++)
                        {
                            SummaryItem data = new SummaryItem()
                            {
                                index = i,
                                counter = sBoxPacket.data[pos++],
                                bets = 0,
                                wins = sBoxPacket.data[pos++],
                                odds = (double)sBoxPacket.data[pos++] / 1000000.0,
                            };
                            sBoxSummaryData.superTimes.Add(data);
                        }

                        sBoxSummaryData.JP.Clear();
                        for (int i = 0; i < 6; i++)
                        {
                            SummaryItem data = new SummaryItem()
                            {
                                index = i,
                                counter = sBoxPacket.data[pos++],
                                bets = 0,
                                wins = sBoxPacket.data[pos++],
                                odds = (double)sBoxPacket.data[pos++] / 1000000.0,
                            };
                            sBoxSummaryData.JP.Add(data);
                        }

                        sBoxSummaryData.freeRole.Clear();
                        for (int i = 0; i < 12; i++)
                        {
                            SummaryItem data = new SummaryItem()
                            {
                                index = i,
                                counter = sBoxPacket.data[pos++],
                                bets = 0,
                                wins = sBoxPacket.data[pos++],
                                odds = (double)sBoxPacket.data[pos++] / 1000000.0,
                            };
                            sBoxSummaryData.freeRole.Add(data);
                        }

                        sBoxSummaryData.allPick.Clear();
                        for (int i = 0; i < 1; i++)
                        {
                            SummaryItem data = new SummaryItem()
                            {
                                index = i,
                                counter = sBoxPacket.data[pos++],
                                bets = 0,
                                wins = sBoxPacket.data[pos++],
                                odds = (double)sBoxPacket.data[pos++] / 1000000.0,
                            };
                            sBoxSummaryData.allPick.Add(data);
                        }

                        break;

                    // -------------------------------------------------------------------------------------
                    // 彩金，提拨分数，宝箱，
                    case 5:
                        pos = 2;
                        sBoxSummaryData.jackpot.list.Clear();
                        for (int i = 0; i < 5; i++)
                        {
                            SummaryItem data = new SummaryItem()
                            {
                                index = i,
                                counter = sBoxPacket.data[pos++],
                                bets = 0,
                                wins = sBoxPacket.data[pos++],
                                odds = (double)sBoxPacket.data[pos++] / 1000000.0,
                            };
                            sBoxSummaryData.jackpot.list.Add(data);
                        }
                        sBoxSummaryData.jackpot.extraData.Clear();
                        for (int i = 0; i < 1; i++)
                        {
                            sBoxSummaryData.jackpot.extraData.Add(sBoxPacket.data[pos++]);
                        }

                        sBoxSummaryData.proposeScore.list.Clear();
                        for (int i = 0; i < 3; i++)
                        {
                            SummaryItem data = new SummaryItem()
                            {
                                index = i,
                                counter = 0,
                                bets = 0,
                                wins = sBoxPacket.data[pos++],
                                odds = (double)sBoxPacket.data[pos++] / 1000000.0,
                            };
                            sBoxSummaryData.proposeScore.list.Add(data);
                        }
                        sBoxSummaryData.proposeScore.extraData.Clear();
                        for (int i = 0; i < 6; i++)
                        {
                            sBoxSummaryData.proposeScore.extraData.Add(sBoxPacket.data[pos++]);
                        }

                        sBoxSummaryData.box.Clear();
                        for (int i = 0; i < 8; i++)
                        {
                            SummaryItem data = new SummaryItem()
                            {
                                index = i,
                                counter = sBoxPacket.data[pos++],
                                bets = 0,
                                wins = sBoxPacket.data[pos++],
                                odds = (double)sBoxPacket.data[pos++] / 1000000.0,
                            };
                            sBoxSummaryData.box.Add(data);
                        }

                        sBoxSummaryData.reward.Clear();
                        for (int i = 0; i < 6; i++)
                        {
                            SummaryItem data = new SummaryItem()
                            {
                                index = i,
                                counter = sBoxPacket.data[pos++],
                                bets = sBoxPacket.data[pos++],
                                wins = sBoxPacket.data[pos++],
                                odds = 0.0,
                            };
                            sBoxSummaryData.reward.Add(data);
                        }
                        break;

                    // -------------------------------------------------------------------------------------
                    // 波动级数1
                    case 6:
                        pos = 2;
                        for (int i = 0; i < sBoxSummaryData.wave.array.Length; i++)
                        {
                            sBoxSummaryData.wave.array[i] = sBoxPacket.data[pos++];
                        }

                        for (int i = 0; i < sBoxSummaryData.wave.shuffle.Length; i++)
                        {
                            sBoxSummaryData.wave.shuffle[i] = sBoxPacket.data[pos++];
                        }
                        break;

                    // 波动级数2
                    case 7:
                        pos = 2;

                        for (int i = 0; i < sBoxSummaryData.wave.supersede.Length; i++)
                        {
                            sBoxSummaryData.wave.supersede[i] = sBoxPacket.data[pos++];
                        }

                        for (int i = 0; i < sBoxSummaryData.wave.target.Length; i++)
                        {
                            sBoxSummaryData.wave.target[i] = sBoxPacket.data[pos++];
                        }

                        for (int i = 0; i < sBoxSummaryData.wave.extraData.Length; i++)
                        {
                            sBoxSummaryData.wave.extraData[i] = sBoxPacket.data[pos++];
                        }
                        break;

                    // -------------------------------------------------------------------------------------
                    // 机率，比例，自备款，20局押分
                    case 8:
                        pos = 2;
                        for (int i = 0; i < sBoxSummaryData.betsInfo.odds.Length; i++)
                        {
                            sBoxSummaryData.betsInfo.odds[i] = (double)sBoxPacket.data[pos++] / 1000000.0;
                        }

                        for (int i = 0; i < sBoxSummaryData.betsInfo.extraData.Length; i++)
                        {
                            sBoxSummaryData.betsInfo.extraData[i] = sBoxPacket.data[pos++];
                        }

                        //sBoxSummaryData.betsInfo.extraData2 = sBoxPacket.data[pos++] / 1000000.0;
                        sBoxSummaryData.betsInfo.extraData2 = sBoxSummaryData.betsInfo.extraData[7] / 1000000.0;

                        sBoxSummaryData.betsInfo.list.Clear();
                        for (int i = 0; i < 21; i++)
                        {
                            SummaryItem2 data = new SummaryItem2();
                            data.index = i;
                            data.bets = sBoxPacket.data[pos++];
                            data.needWins = (int)((double)data.bets * sBoxSummaryData.betsInfo.odds[2]);
                            data.wins = sBoxPacket.data[pos++];
                            data.proposeScore = sBoxPacket.data[pos++];

                            sBoxSummaryData.betsInfo.list.Add(data);
                        }

                        break;

                    // -------------------------------------------------------------------------------------
                    // 段值，800局计数器
                    case 9:
                        pos = 2;
                        for (int i = 0; i < sBoxSummaryData.segValue.array20.Length; i++)
                        {
                            sBoxSummaryData.segValue.array20[i] = sBoxPacket.data[pos++];
                        }

                        for (int i = 0; i < sBoxSummaryData.segValue.array40.Length; i++)
                        {
                            sBoxSummaryData.segValue.array40[i] = sBoxPacket.data[pos++];
                        }

                        for (int i = 0; i < sBoxSummaryData.segValue.counter800.Length; i++)
                        {
                            sBoxSummaryData.segValue.counter800[i] = sBoxPacket.data[pos++];
                        }

                        break;

                    // -------------------------------------------------------------------------------------
                    // 加倍，翻倍
                    case 10:
                        pos = 2;
                        sBoxSummaryData.littleScore.Clear();
                        for (int i = 0; i < 6; i++)
                        {
                            SummaryLittleScore data = new SummaryLittleScore();
                            for (int j = 0; j < 9; j++)
                            {
                                data.score[j] = sBoxPacket.data[pos++];
                            }
                            sBoxSummaryData.littleScore.Add(data);
                        }
                        break;

                    // -------------------------------------------------------------------------------------
                    // 送灯，满贯分数表
                    case 11:
                        pos = 2;
                        for (int i = 0; i < 6; i++)
                        {
                            SummaryLittleScore data = new SummaryLittleScore();
                            for (int j = 0; j < 9; j++)
                            {
                                data.score[j] = sBoxPacket.data[pos++];
                            }
                            sBoxSummaryData.littleScore.Add(data);
                        }

                        break;

                    // -------------------------------------------------------------------------------------
                    // 波动 + 15门，小游戏，彩金，宝箱，账单合计
                    case 12:
                        pos = 2;
                        sBoxSummaryData.totalWave.Clear();
                        for (int i = 0; i < 5; i++)
                        {
                            SummaryItem data = new SummaryItem()
                            {
                                index = i,
                                counter = sBoxPacket.data[pos++],
                                bets = sBoxPacket.data[pos++],
                                wins = sBoxPacket.data[pos++],
                                odds = (double)sBoxPacket.data[pos++] / 1000000.0,
                            };
                            sBoxSummaryData.totalWave.Add(data);
                        }
                        sBoxSummaryData.littleGame.Clear();
                        for (int i = 0; i < 8; i++)
                        {
                            SummaryItem data = new SummaryItem()
                            {
                                index = i,
                                counter = sBoxPacket.data[pos++],
                                bets = sBoxPacket.data[pos++],
                                wins = sBoxPacket.data[pos++],
                                odds = (double)sBoxPacket.data[pos++] / 1000000.0,
                            };
                            sBoxSummaryData.littleGame.Add(data);
                        }
                        bTrigger = true;
                        break;
                }
            }
            if (bTrigger == true)
            {
                EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_GET_SUMMARY_SICBO, sBoxSummaryData);
            }
        }
    }
}
