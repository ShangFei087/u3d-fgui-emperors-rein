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

    public class SBoxPrizeData
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

    public class SBoxPrizePrepareData
    {
        public int result;

        public int State;                          // 算法卡状态，enum SBOX_IDEA_STATE
        public int Jackpot;                        // 当前彩金池
        public int rfu;                            // 保留
    }

    public class SBoxPrizePlayerData
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
		 *  @brief          预开奖，开始押注前调用，算法卡会告知一些特定的信息，如算法卡的状态，需要在押注过程中有所表现的小游戏，比如：明牌等等
		 *  @param          无
		 *  @return         SBoxBaseData.value[0] = 0：成功
		 *                  SBoxBaseData.value[0] < 0：发送参数错误
		 *                  SBoxBaseData.value[0] > 0：状态码
		 *  @details        
		 */
        public static void GetPrizePrepare()
        {
            //SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20009, source: 1, target: 2, size: 4);

            //sBoxPacket.data[0] = 0;
            //sBoxPacket.data[1] = 0;
            //sBoxPacket.data[2] = 0;
            //sBoxPacket.data[3] = 0;

            //SBoxIOEvent.AddListener(sBoxPacket.cmd, GetPrizePrepareR);
            //SBoxIOStream.Write(sBoxPacket);
        }
        private static void GetPrizePrepareR(SBoxPacket sBoxPacket)
        {
            //SBoxPrizePrepareData sBoxPrizePrepareData = new SBoxPrizePrepareData()
            //{
            //    result = sBoxPacket.data[0],
            //    State = sBoxPacket.data[1],
            //    Jackpot = sBoxPacket.data[2],
            //};
            //EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_GET_PRIZE_PREPARE, sBoxPrizePrepareData);
        }

        /**
		 *  @brief          开奖
		 *  @return         SBoxPrizeData.result = 0：成功
		 *                  SBoxPrizeData.result < 0：发送参数错误
		 *                  SBoxPrizeData.result > 0：状态码
		 *  @details        
		 */
        public static void GetPrize()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20010, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, GetPrizeR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void GetPrizeR(SBoxPacket sBoxPacket)
        {
            SBoxPrizeData sBoxPrizeData = new SBoxPrizeData();

            if(sBoxPacket.data.Length > 10)
            {
                sBoxPrizeData.result = sBoxPacket.data[0];
                sBoxPrizeData.PersonId = sBoxPacket.data[1];
                sBoxPrizeData.FeatureId = sBoxPacket.data[2];

                sBoxPrizeData.CoinToTable[0] = sBoxPacket.data[4];
                sBoxPrizeData.CoinToTable[1] = sBoxPacket.data[5];
                sBoxPrizeData.CoinToTable[2] = sBoxPacket.data[6];
                sBoxPrizeData.CoinToTable[3] = sBoxPacket.data[7];
                sBoxPrizeData.CoinToTable[4] = sBoxPacket.data[8];

                sBoxPrizeData.Data = new int[sBoxPacket.data.Length - 10];
                for(int i = 10;i < sBoxPacket.data.Length; i++)
                {
                    sBoxPrizeData.Data[i - 10] = sBoxPacket.data[i];
                }
            }
            else
            {
                sBoxPrizeData.result = -1;
            }
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_GET_PRIZE, sBoxPrizeData);
        }

        /**
		 *  @brief          读取指定玩家的中奖数据
		 *  @param          无
		 *  @return         SBoxPrizePlayerData.result = 0：成功
		 *                  SBoxPrizePlayerData.result < 0：发送参数错误
		 *                  SBoxPrizePlayerData.result > 0：状态码
		 *  @details        
		 */
        public static void GetPrizePlayer(int PlayerId)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20011, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = PlayerId;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, GetPrizePlayerR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void GetPrizePlayerR(SBoxPacket sBoxPacket)
        {
            SBoxPrizePlayerData sBoxPrizePlayerData = new SBoxPrizePlayerData();

            sBoxPrizePlayerData.result = sBoxPacket.data[0];
            sBoxPrizePlayerData.PlayerId = sBoxPacket.data[1];
            sBoxPrizePlayerData.Win = sBoxPacket.data[2];
            sBoxPrizePlayerData.WinDragon = sBoxPacket.data[3];
            sBoxPrizePlayerData.WinJackpot = sBoxPacket.data[4];
            sBoxPrizePlayerData.WinBoxGame = sBoxPacket.data[5];
            sBoxPrizePlayerData.PointsMax = sBoxPacket.data[6];
            sBoxPrizePlayerData.BoxPoints = sBoxPacket.data[7];
            sBoxPrizePlayerData.OpenBoxID = sBoxPacket.data[8];
            sBoxPrizePlayerData.BoxOpenFlags[0] = (sBoxPacket.data[9] >> 0) & 1;
            sBoxPrizePlayerData.BoxOpenFlags[1] = (sBoxPacket.data[9] >> 1) & 1;
            sBoxPrizePlayerData.BoxOpenFlags[2] = (sBoxPacket.data[9] >> 2) & 1;
            sBoxPrizePlayerData.BoxOpenFlags[3] = (sBoxPacket.data[9] >> 3) & 1;

            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_GET_PRIZE_PLAYER, sBoxPrizePlayerData);
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
        public static void SetCoinToHoleCount(int[] coinToHoleCount)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20013, source: 1, target: 2, size: 5);

            sBoxPacket.data[0] = coinToHoleCount[0];
            sBoxPacket.data[1] = coinToHoleCount[1];
            sBoxPacket.data[2] = coinToHoleCount[2];
            sBoxPacket.data[3] = coinToHoleCount[3];
            sBoxPacket.data[4] = coinToHoleCount[4];

            SBoxIOEvent.AddListener(sBoxPacket.cmd, SetCoinToHoleCountR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void SetCoinToHoleCountR(SBoxPacket sBoxPacket)
        {
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SET_COIN_TO_HOLE_COUNT, sBoxPacket.data[0]);
        }
    }
}