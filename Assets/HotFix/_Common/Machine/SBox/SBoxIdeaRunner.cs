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

namespace SBoxApi
{

    public class SBoxRunnerPrizeData
    {
        public int result;

        public int PersonId;                        // 基础游戏中奖人物
                                                    // 0	红唐僧
                                                    // 1	绿唐僧
                                                    // 2	黄唐僧
                                                    // 3	红孙悟空
                                                    // 4	绿孙悟空
                                                    // 5	黄孙悟空
                                                    // 6	红猪八戒
                                                    // 7	绿猪八戒
                                                    // 8	黄猪八戒
                                                    // 9	红沙和尚
                                                    // 10	绿沙和尚
                                                    // 11	黄沙和尚

        public int FeatureId;                       // 小游戏中奖类型
                                                    // 0	不开奖，笑脸
                                                    // 1	红龙
                                                    // 2	金龙
                                                    // 3	绿龙
                                                    // 4	加倍
                                                    // 5	翻倍
                                                    // 6	送灯
                                                    // 7	大满贯
                                                    // 8	彩金

        public int[] CoinToTable = new int[5];      // 落到桌面上各类型金币数量：
                                                    // [0]: 银币数量（1分）
                                                    // [1]: 金币数量（2分）
                                                    // [2]: 元宝数量（5分）
                                                    // [3]: 彩球数量（10分）
                                                    // [4]: 彩盘数量（20分）
        public int[] Data;                          // 小游戏相应参数，不同类型有不同的内容定义
                                                    // 1，2，3: Data[0]为相应龙角色的倍率
                                                    // 4: Data[0~11]为每个基础角色的加倍倍数
                                                    // 5: Data[0~11]为每个基础角色的翻倍倍数
                                                    // 6: Data[0~11]为每个基础角色的送灯次数，0：不送，其它：次数
                                                    // 8: Data[0]为本局总出彩金的值
    }


    public class SBoxRunnerPrizePlayerData
    {
        public int result;

        public int PlayerId;                        // 玩家ID
        public int Win;                             // 本局总赢分（包括：押中的基础游戏，龙，宝箱积分，彩金，所有相应小游戏的得分）
        public int WinDragon;                       // 本局龙得分
        public int WinJackpot;                      // 本局赢的彩金金额
        public int WinBoxGame;                      // 本局箱子游戏的赢钱金额
        public int PointsMax;                       // 宝箱进度条最大值
        public int BoxPoints;                       // 宝箱累加积分
        public int OpenBoxID;                       // 本局要开的箱子编号，0：没有开，1：第一个宝箱，2：第二个，3：第三个，4：第四个
        public int[] BoxOpenFlags = new int[4];
    }

    public partial class SBoxIdea
    {
        /**
		 *  @brief          开奖
		 *  @return         SBoxPrizeData.result = 0：成功
		 *                  SBoxPrizeData.result < 0：发送参数错误
		 *                  SBoxPrizeData.result > 0：状态码
		 *  @details        
		 */
        public static void GetRunnerPrize()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20010, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, GetRunnerPrizeR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void GetRunnerPrizeR(SBoxPacket sBoxPacket)
        {
            SBoxRunnerPrizeData sBoxRunnerPrizeData = new SBoxRunnerPrizeData();

            if(sBoxPacket.data.Length > 10)
            {
                sBoxRunnerPrizeData.result = sBoxPacket.data[0];
                sBoxRunnerPrizeData.PersonId = sBoxPacket.data[1];
                sBoxRunnerPrizeData.FeatureId = sBoxPacket.data[2];

                sBoxRunnerPrizeData.CoinToTable[0] = sBoxPacket.data[4];
                sBoxRunnerPrizeData.CoinToTable[1] = sBoxPacket.data[5];
                sBoxRunnerPrizeData.CoinToTable[2] = sBoxPacket.data[6];
                sBoxRunnerPrizeData.CoinToTable[3] = sBoxPacket.data[7];
                sBoxRunnerPrizeData.CoinToTable[4] = sBoxPacket.data[8];

                sBoxRunnerPrizeData.Data = new int[sBoxPacket.data.Length - 10];
                for(int i = 10;i < sBoxPacket.data.Length; i++)
                {
                    sBoxRunnerPrizeData.Data[i - 10] = sBoxPacket.data[i];
                }
            }
            else
            {
                sBoxRunnerPrizeData.result = -1;
            }
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_GET_PRIZE, sBoxRunnerPrizeData);
        }

        /**
		 *  @brief          读取指定玩家的中奖数据
		 *  @param          无
		 *  @return         SBoxPrizePlayerData.result = 0：成功
		 *                  SBoxPrizePlayerData.result < 0：发送参数错误
		 *                  SBoxPrizePlayerData.result > 0：状态码
		 *  @details        
		 */
        public static void GetRunnerPrizePlayer(int PlayerId)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20011, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = PlayerId;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, GetRunnerPrizePlayerR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void GetRunnerPrizePlayerR(SBoxPacket sBoxPacket)
        {
            SBoxRunnerPrizePlayerData sBoxRunnerPrizePlayerData = new SBoxRunnerPrizePlayerData();

            sBoxRunnerPrizePlayerData.result = sBoxPacket.data[0];
            sBoxRunnerPrizePlayerData.PlayerId = sBoxPacket.data[1];
            sBoxRunnerPrizePlayerData.Win = sBoxPacket.data[2];
            sBoxRunnerPrizePlayerData.WinDragon = sBoxPacket.data[3];
            sBoxRunnerPrizePlayerData.WinJackpot = sBoxPacket.data[4];
            sBoxRunnerPrizePlayerData.WinBoxGame = sBoxPacket.data[5];
            sBoxRunnerPrizePlayerData.PointsMax = sBoxPacket.data[6];
            sBoxRunnerPrizePlayerData.BoxPoints = sBoxPacket.data[7];
            sBoxRunnerPrizePlayerData.OpenBoxID = sBoxPacket.data[8];
            sBoxRunnerPrizePlayerData.BoxOpenFlags[0] = (sBoxPacket.data[9] >> 0) & 1;
            sBoxRunnerPrizePlayerData.BoxOpenFlags[1] = (sBoxPacket.data[9] >> 1) & 1;
            sBoxRunnerPrizePlayerData.BoxOpenFlags[2] = (sBoxPacket.data[9] >> 2) & 1;
            sBoxRunnerPrizePlayerData.BoxOpenFlags[3] = (sBoxPacket.data[9] >> 3) & 1;

            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_GET_PRIZE_PLAYER, sBoxRunnerPrizePlayerData);
        }

        /**
		 *  @brief          设定掉到坑里的各种金币的数量
		 *  @param          coinToHoleCount 掉到坑里的各种金币的数量
		 *                  [0]: 银币数量-1分
		 *                  [1]: 金币数量-2分
		 *                  [2]: 元宝数量-5分
		 *                  [3]: 彩球数量-10分
		 *                  [4]: 彩盘数量-20分
		 *  @return         SBoxBaseData.value[0] = 0：成功
		 *                  SBoxBaseData.value[0] < 0：发送参数错误
		 *                  SBoxBaseData.value[0] > 0：状态码
		 *  @details        
		 */
        public static void SetRunnerCoinToHoleCount(int[] coinToHoleCount)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20013, source: 1, target: 2, size: 5);

            sBoxPacket.data[0] = coinToHoleCount[0];
            sBoxPacket.data[1] = coinToHoleCount[1];
            sBoxPacket.data[2] = coinToHoleCount[2];
            sBoxPacket.data[3] = coinToHoleCount[3];
            sBoxPacket.data[4] = coinToHoleCount[4];

            SBoxIOEvent.AddListener(sBoxPacket.cmd, SetRunnerCoinToHoleCountR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void SetRunnerCoinToHoleCountR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SET_COIN_TO_HOLE_COUNT, sBoxPacket.data[0]);
        }
    }
}