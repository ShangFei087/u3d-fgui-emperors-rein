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

    public class SummaryItem
    {
        public int index;
        public int counter;
        public int bets;
        public int wins;
        public int mul;
        public double odds;
    }
    public class SummaryItem2
    {
        public int index;
        public int bets;
        public int needWins;
        public int wins;
        public int proposeScore;
    }

    public class SummaryItemExtra
    {
        public List<SummaryItem> list = new List<SummaryItem>();
        public List<int> extraData = new List<int>();
    }

    public class SummaryWave
    {
        public int[] array = new int[32];         // 上下拉阵列 
        public int[] shuffle = new int[32];       // 下下拉洗牌
        public int[] supersede = new int[32];     // 上下交替
        public int[] target = new int[32];        // 目标等级
        public int[] extraData = new int[3];      // 目前序列，波动分数，波动方向
    }
    
    public class SummaryBetsInfo
    {
        public double[] odds = new double[6];     // 设定机率，应用机率，使用机率，自然机率, 送灯比例, 满贯比例
        public int[] extraData = new int[11];      // 累计提拨分数, 提拨分数, 一段库, 两段库, 自备款，20局总押，20局总赢，使用机率分数，提拔分数，平均押分，20局计数器
        public double extraData2;                 // 提拔余数
        public List<SummaryItem2> list = new List<SummaryItem2>(); // 20局押信息
    }

    // 段值，800局计数器
    public class SummarySegValue
    {
        public int[] array20 = new int[23];       // 20局段值，初始段值
        public int[] array40 = new int[42];       // 40阵列段值
        public int[] counter800 = new int[8];     // 800局计数器
    }

    public class SummaryLittleScore
    {
        public int[] score = new int[9];
    }

    public class SummaryCrocodileNew
    {
        public int index;
        public int hotScoreCounter;
        public int hotScoreIdx;
        public int hotUpScoreCount;
        public int hotUpScoreIdx;
    }

    public class SBoxIdeaSummaryData
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

        public List<SummaryLittleScore> littleScore = new List<SummaryLittleScore>(); // 送灯，满贯分数表

        public SummaryCrocodileNew crocodileData = new SummaryCrocodileNew();   // 鳄鱼数据
    }

    public class SBoxIdeaSummary
    {
        public static SBoxIdeaSummaryData sBoxSummaryData = new SBoxIdeaSummaryData();

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
                                mul = sBoxPacket.data[pos++],
                                odds = (double)sBoxPacket.data[pos++] / 1000000.0,
                            };
                            sBoxSummaryData.waveUp.Add(data);
                        }
                        break;
                    // 波动下拉
                    case 2:
                        pos = 2;
                        sBoxSummaryData.waveDown.Clear();
                        for (int i = 0; i < 14; i++)
                        {
                            SummaryItem data = new SummaryItem()
                            {
                                index = i,
                                counter = sBoxPacket.data[pos++],
                                bets = sBoxPacket.data[pos++],
                                wins = sBoxPacket.data[pos++],
                                mul = sBoxPacket.data[pos++],
                                odds = (double)sBoxPacket.data[pos++] / 1000000.0,
                            };
                            sBoxSummaryData.waveDown.Add(data);
                        }
                        break;
                    // 基础角色
                    case 3:
                        pos = 2;
                        sBoxSummaryData.role.Clear();
                        for (int i = 0; i < 13; i++)
                        {
                            SummaryItem data = new SummaryItem()
                            {
                                index = i,
                                counter = sBoxPacket.data[pos++],
                                bets = sBoxPacket.data[pos++],
                                wins = sBoxPacket.data[pos++],
                                mul = sBoxPacket.data[pos++],
                                odds = (double)sBoxPacket.data[pos++] / 1000000.0,
                            };
                            sBoxSummaryData.role.Add(data);
                        }
                        //关要求的调整位置
                        SummaryItem data1 = new SummaryItem()
                        {
                            index = 13,
                            bets = sBoxPacket.data[pos++],
                            wins = sBoxPacket.data[pos++],
                            counter = sBoxPacket.data[pos++],
                            mul = sBoxPacket.data[pos++],
                            odds = (double)sBoxPacket.data[pos++] / 1000000.0,
                        };
                        sBoxSummaryData.role.Add(data1);
                        break;
                    //送灯,大满贯
                    case 4:
                        pos = 2;
                        sBoxSummaryData.freeRole.Clear();
                        for (int i = 0; i < 7; i++)
                        {
                            SummaryItem data = new SummaryItem()
                            {
                                index = i,
                                counter = sBoxPacket.data[pos++],
                                bets = sBoxPacket.data[pos++],
                                wins = sBoxPacket.data[pos++],
                                mul = sBoxPacket.data[pos++],
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
                                bets = sBoxPacket.data[pos++],
                                wins = sBoxPacket.data[pos++],
                                mul = sBoxPacket.data[pos++],
                                odds = (double)sBoxPacket.data[pos++] / 1000000.0,
                            };
                            sBoxSummaryData.allPick.Add(data);
                        }
                        break;
                    //彩金
                    case 5:
                        pos = 2;
                        sBoxSummaryData.jackpot.list.Clear();
                        for (int i = 0; i < 4; i++)
                        {
                            SummaryItem data = new SummaryItem()
                            {
                                index = i,
                                counter = sBoxPacket.data[pos++],
                                bets = sBoxPacket.data[pos++],
                                wins = sBoxPacket.data[pos++],
                                mul = sBoxPacket.data[pos++],
                                odds = (double)sBoxPacket.data[pos++] / 1000000.0,
                            };
                            sBoxSummaryData.jackpot.list.Add(data);
                        }
                        sBoxSummaryData.jackpot.extraData.Clear();
                        sBoxSummaryData.jackpot.extraData.Add(sBoxPacket.data[pos++]);
                        sBoxSummaryData.jackpot.extraData.Add(sBoxPacket.data[pos++]);

                        sBoxSummaryData.reward.Clear();
                        for (int i = 0; i < 3; i++)
                        {
                            SummaryItem data = new SummaryItem()
                            {
                                index = i,
                                counter = sBoxPacket.data[pos++],
                                bets = sBoxPacket.data[pos++],
                                wins = sBoxPacket.data[pos++],
                                mul = sBoxPacket.data[pos++],
                                odds = 0,
                            };
                            sBoxSummaryData.reward.Add(data);
                        }
                        break;
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
                    // 机率，比例，自备款，20局押分
                    case 8:
                        pos = 2;
                        for (int i = 0; i < sBoxSummaryData.betsInfo.odds.Length - 2; i++)
                        {
                            sBoxSummaryData.betsInfo.odds[i] = (double)sBoxPacket.data[pos++] / 1000000.0;
                        }
                        sBoxSummaryData.betsInfo.odds[4] = (double)0.75f;
                        sBoxSummaryData.betsInfo.odds[5] = (double)0.25f;

                        for (int i = 0; i < sBoxSummaryData.betsInfo.extraData.Length; i++)
                        {   
                            sBoxSummaryData.betsInfo.extraData[i] = sBoxPacket.data[pos++];
                            switch (i)
                            {
                                case 1:
                                case 2:
                                case 3:
                                    sBoxSummaryData.betsInfo.extraData[i] /= 1000;
                                    break;
                                case 7:
                                case 8:
                                    sBoxSummaryData.betsInfo.extraData[i] /= 100;
                                    break;
                            }
                        }

                        sBoxSummaryData.betsInfo.extraData2 = sBoxPacket.data[pos++] / 1000000.0;

                        sBoxSummaryData.betsInfo.list.Clear();
                        for (int i = 0; i < 20; i++)
                        {
                            SummaryItem2 data = new SummaryItem2();
                            data.index = i;
                            data.bets = sBoxPacket.data[pos++];
                            data.wins = sBoxPacket.data[pos++];
                            data.proposeScore = sBoxPacket.data[pos++] / 100;
                            data.needWins = sBoxPacket.data[pos++] / 100;
                            sBoxSummaryData.betsInfo.list.Add(data);
                        }

                        break;
                    // 段值，800局计数器
                    case 9:
                        pos = 2;
                        for (int i = 0; i < sBoxSummaryData.segValue.array20.Length; i++)
                            sBoxSummaryData.segValue.array20[i] = sBoxPacket.data[pos++];
                        for (int i = 0; i < sBoxSummaryData.segValue.array40.Length; i++)
                            sBoxSummaryData.segValue.array40[i] = sBoxPacket.data[pos++];
                        for (int i = 0; i < sBoxSummaryData.segValue.counter800.Length; i++)
                            sBoxSummaryData.segValue.counter800[i] = sBoxPacket.data[pos++];
                        break;
                    // 送灯,满贯
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
                    // 波动 + 15门，小游戏，彩金，账单合计  共9个
                    case 11:
                        pos = 2;
                        sBoxSummaryData.totalWave.Clear();
                        for (int i = 0; i < 4; i++)
                        {
                            SummaryItem data = new SummaryItem()
                            {
                                index = i,
                                counter = sBoxPacket.data[pos++],
                                bets = sBoxPacket.data[pos++],
                                wins = sBoxPacket.data[pos++],
                                mul = sBoxPacket.data[pos++],
                                odds = (double)sBoxPacket.data[pos++] / 1000000.0,
                            };
                            sBoxSummaryData.totalWave.Add(data);
                        }
                        sBoxSummaryData.littleGame.Clear();
                        for (int i = 0; i < 5; i++)
                        {
                            SummaryItem data = new SummaryItem()
                            {
                                index = i,
                                counter = sBoxPacket.data[pos++],
                                bets = sBoxPacket.data[pos++],
                                wins = sBoxPacket.data[pos++],
                                mul = sBoxPacket.data[pos++],
                                odds = (double)sBoxPacket.data[pos++] / 1000000.0,
                            };
                            sBoxSummaryData.littleGame.Add(data);
                        }
                        bTrigger = true;
                        break;
                    case 12:
                        pos = 2;
                        sBoxSummaryData.crocodileData = new SummaryCrocodileNew()
                        {
                            hotScoreCounter = sBoxPacket.data[pos++],
                            hotScoreIdx = sBoxPacket.data[pos++],
                            hotUpScoreCount = sBoxPacket.data[pos++],
                            hotUpScoreIdx = sBoxPacket.data[pos++],
                        };
                        break;
                }
            }
            if (bTrigger == true)
            {
                EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_GET_SUMMARY, sBoxSummaryData);
            }
        }

    }
}