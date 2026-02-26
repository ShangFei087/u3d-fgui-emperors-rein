using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBoxApi
{
    /*
	* 	JACKPOT_MAX = 4
	* 	3 小彩金
	* 	2 中彩金
	* 	1 大彩金
	* 	0 巨大彩金
	*/

    //从硬件返回的彩金信息
    public class SBoxJackpotData
    {
        public int result;
        public int MachineId;                       // 机台号
        public int SeatId;                          // 分机号/座位号
        public int ScoreRate;                       // 分值比，1分多少钱.直接返回app传下来的值
        public int JpPercent;                       // 分机彩金百分比，每次押分贡献给彩金的比例。直接返回app传下来的值

        //
        public int[] Lottery;                       // 0:表示没有开出彩金，1:表示已开出彩金
        public int[] Jackpotlottery;				// 开出的彩金注意:此处的单位是钱的单位，而且是乘以了100的，分机收到这个值要根据分机的分值比来转成成对应的分数，而且还要将此值除以100
        public int[] JackpotOut;                    // 彩金显示积累分,用于显示当前的彩金值
        public int[] JackpotOld;					// 开出彩金前的显示积累分
    }
}