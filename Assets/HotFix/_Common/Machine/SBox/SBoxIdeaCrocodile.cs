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
using UnityEngine;

namespace SBoxApi
{
    public class SBoxCrocodileRewardInfo
    {
        public int result;
        public int SubGameId;               //小游戏id  0: 未中小游戏 1: 送灯 2: 满贯 3: 彩金
        public int[] Data;                  //相应参数，不同类型有不同的内容定义 0~149  是10个玩家15门的得分,
                                            //当是送灯时 150 ~ 155 是送灯时的送灯信息
                                            //当是开彩金时，150 ~ 159 是10个玩家的彩金得分
    }

    public class SBoxCrocodileOpenInfo
    {
        public int result;
        public int color;                   //开出的颜色 1 红色 2 绿色 3黄色
        /*
         *  Data的值为以下值，为0是表示没翻倍
            CROCODILE_RED		1		//鳄鱼红色		
            CROCODILE_GREEN		2		//鳄鱼绿色		
            CROCODILE_YELLOW	3		//鳄鱼黄色		

            ELEPHANT_RED		5		//大象红色		
            ELEPHANT_GREEN		6		//大象绿色		
            ELEPHANT_YELLOW		7		//大象黄色		

            LION_RED			9		//狮子红色		
            LION_GREEN			10		//狮子绿色		
            LION_YELLOW			11		//狮子黄色		

            PANDA_RED			13		//熊猫红色		
            PANDA_GREEN			14		//熊猫绿色		
            PANDA_YELLOW		15		//熊猫黄色		
         */
        public int[] Data;                  //翻倍结果
    }

    public partial class SBoxIdea
    {
        /**
		 *  @brief          读取当局开出的颜色
		 *  @param          无
		 *  @return         sBoxPacket.data[0] = 0：成功
		 *                  sBoxPacket.data[0] < 0：发送参数错误
		 *                  sBoxPacket.data[0] > 0：状态码
		 *  				sBoxPacket.data[1~100]: 结果
		 *  				
		 *  @details        
		 */
        public static void CrocodileGetOpenInfo()
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20010, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = 0;
            sBoxPacket.data[1] = 0;

            SBoxIOEvent.AddListener(sBoxPacket.cmd, CrocodileGetOpenInfoR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void CrocodileGetOpenInfoR(SBoxPacket sBoxPacket)
        {
            SBoxCrocodileOpenInfo sBoxCrocodileOpenInfo = new SBoxCrocodileOpenInfo();
            sBoxCrocodileOpenInfo.result = sBoxPacket.data[0];
            if(sBoxCrocodileOpenInfo.result == 0)
            {
                sBoxCrocodileOpenInfo.color = sBoxPacket.data[1];
                sBoxCrocodileOpenInfo.Data = new int[120];
                int pos = 2;
                for(int i = 0; i < sBoxCrocodileOpenInfo.Data.Length; i++)
                {
                    sBoxCrocodileOpenInfo.Data[i] = sBoxPacket.data[pos++];
                }
            }

            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_CROCODILE_OPEN_INFO, sBoxCrocodileOpenInfo);
        }

        /**
		 *  @brief          请求普通开奖结果，返回本局开奖结果
		 *  @param          holeIndex 球的进洞索引
		 *  @return         sBoxPacket.data 对应的奖项数据
		 *                  
		 *  @details        
		 */
        public static void CrocodileGetReward(int holeIndex)
        {
            SBoxPacket sBoxPacket = new SBoxPacket(cmd: 20011, source: 1, target: 2, size: 2);

            sBoxPacket.data[0] = holeIndex;
            sBoxPacket.data[1] = 0;
            SBoxIOEvent.AddListener(sBoxPacket.cmd, CrocodileGetRewardR);
            SBoxIOStream.Write(sBoxPacket);
        }
        private static void CrocodileGetRewardR(SBoxPacket sBoxPacket)
        {
            SBoxCrocodileRewardInfo sBoxCrocodileRewardInfo = new SBoxCrocodileRewardInfo();
            sBoxCrocodileRewardInfo.result = sBoxPacket.data[0];
            sBoxCrocodileRewardInfo.SubGameId = sBoxPacket.data[1];
            if(sBoxCrocodileRewardInfo.result == 0)
            {
                int pos = 2;
                sBoxCrocodileRewardInfo.Data = new int[160];
                for(int i = 0; i < 160; i++)
                {
                    sBoxCrocodileRewardInfo.Data[i] = sBoxPacket.data[pos++];
                }
            }
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_CROCODILE_GET_REWARD, sBoxCrocodileRewardInfo);
        }
    }
}
