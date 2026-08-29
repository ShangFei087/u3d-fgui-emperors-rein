using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XingYunZhiLun_3998
{
    public enum SoundKey
    {
        /// <summary> 正常游戏背景音乐 </summary>
        RegularBG,

        /// <summary> 免费游戏背景音乐 </summary>
        FreeSpinBG,
        /// <summary> 免费游戏空闲音乐 </summary>
        FreeSpinBGIdle,
        /// <summary> 免费游戏触发界面，背景音乐 </summary>
        FreeSpinTriggerBG,
        /// <summary> 免费游戏结束界面，背景音乐 </summary>
        FreeSpinResultBG,


        /// <summary> 免费奖次数提示牌弹出Bgm, 在提示牌从下面升起时出 </summary>
        FgBoarderInBGM,
        /// <summary> 免费奖次数提示牌弹出Bgm, 在用户点“开始”时出，此时“FgBoarderInBGM.ogg”停止播放 </summary>
        FgBoarderInBGMEnding,


        /// <summary> 免费游戏结算提示牌弹出音效, 在提示牌弹出时出 </summary>
        FgSetBoarderIn,
        /// <summary> 进入免费游戏Wild图标出现并固定音效 </summary>
        WildShow,
        /// <summary> 进入免费游戏Wild图标出现并固定音效 </summary>
        ScatterDown,



        /// <summary> 转盘界面背景音乐 </summary>
        WheelBg,
        /// <summary> 转盘游戏BGM结束 </summary>
        WheelBGMEnding,
        /// <summary> 转盘游戏开始按键音效 </summary>
        WheelButton,
        /// <summary> 转盘游戏开始按键音效 </summary>
        WheelRaiseUp,
        /// <summary> 转盘开始转动+中奖停止音效 </summary>
        WheelSpin,
        /// <summary> 转盘一般图标中奖音效（翻倍、保险箱、Wild） </summary>
        WheellItWin,
        /// <summary> 转盘抽中Bounus奖音效, 在抽中Bonus图标时出 </summary>
        BonusWin,
        /// <summary> 转盘Scatter图标中奖音效 </summary>
        ScatterWin,
        /// <summary> 转盘游戏抽中Wild图标中奖音效 </summary>
        WildExtend,


        /// <summary> 滚轮1停止 </summary>
        ReelStop1,
        /// <summary> 滚轮1停止 </summary>
        ReelStop2,
        /// <summary> 滚轮1停止 </summary>
        ReelStop3,
        /// <summary> 滚轮1停止 </summary>
        ReelStop4,
        /// <summary> 滚轮1停止 </summary>
        ReelStop5,

        /// <summary> 滚轮背景音乐 </summary>
        ReelRolling,
        /// <summary> 普通中奖音效（小分值奖） </summary>
        WinPrize1,
        /// <summary> 普通中奖音效（中分值奖） </summary>
        WinPrize2,
        /// <summary> 普通中奖音效（大分值奖） </summary>
        WinPrize3,

        /// <summary> 主游戏连线动画音乐 </summary>
        MainWinAnim,
        /// <summary> 主游戏连线特效音乐 </summary>
        MainWinEffect,
        /// <summary> 总赢线 </summary>
        TotalWinLine,

        /// <summary> 1、2、3列，每当出现财神图标（滚轮缓动特效才有） </summary>
        SlowMotionReal123MeetGod,
        /// <summary> 1、2、3列都有财神图标（滚轮缓动特效才有） </summary>
        SlowMotionReal123HasGod,
        /// <summary>  1、2列出现财神图标，祝贺语 （滚轮缓动特效才有） </summary>
        SlowMotionCongratulate,
        /// <summary>  滚轮缓动 </summary>
        SlowMotionEffect,

        
        /// <summary> 免费游戏修改背景音乐 </summary>
        FreeSpinChangeSymbol,
        /// <summary> 免费游戏特效音乐 </summary>
        FreeGameEffect,
        /// <summary> 5连线 </summary>
        FiveLine,

        /// <summary> 主游戏赢钱，金币滚动 </summary>
        WinRolling,

        /// <summary> BigWin奖提示牌弹出音效 </summary>
        BigWinStart,
        /// <summary> BigWin奖BGM </summary>
        BigWin,
        /// <summary> SuperWin奖BGM </summary>
        SuperWin,
        /// <summary> MegaWin音效, 在MegaWin字在上下弹出时出 </summary>
        MegaWin,
        /// <summary> BigWin奖BGM结束音, 在BigWin、SuperWin或MegaWin字收走时出 </summary>
        BigWinEnd,


        /// <summary> JackPot大奖提示牌音效 </summary>
        JackpotTipStart,
        /// <summary> JackPot大奖中奖音效 </summary>
        JackpotTip,
        /// <summary> JackPot大奖提示牌收走音效, 在点“领取”时出，此时BGM音量1秒内回到1 </summary>
        JackpotTipEnd,


        /// <summary> 彩金弹窗，背景音乐 </summary>
        JackpotBG,

        /// <summary> 彩金奖转动音效, 在彩金奖奖项目转动时出 </summary>
        JackpotSpin,


        /// <summary> 提示牌飞入时出彩金游戏次数提示牌音效 </summary>
        JackpotBoarderIN,
        /// <summary> 彩金游戏次数提示牌收走和转场动画时出音效------(进入和退出通用) </summary>
        JackpotBoarderOut,
    }

    public class SoundModel : MonoSingleton<SoundModel>
    {


        public Dictionary<SoundKey, GSHandler> gsHandlers = new Dictionary<SoundKey, GSHandler>
        {
            [SoundKey.RegularBG] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Music/Normal/NormalBGM.ogg",
                outputType = GSOutType.Music,
                loop = true,
            },

            [SoundKey.FreeSpinBG] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Music/FreeGame/FgBGM.ogg",
                outputType = GSOutType.Music,
                loop = true,
            },

            [SoundKey.FreeSpinTriggerBG] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Music/FreeGame/FgSetBoarderBGM.ogg",
                outputType = GSOutType.Music,
                loop = true,
            },

            [SoundKey.FreeSpinResultBG] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Music/FreeGame/FgSetBoarderBGMEnding.ogg",
                outputType = GSOutType.Music,
                loop = true,
            },

            [SoundKey.JackpotBG] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Music/Jackpot/JpBGM.ogg",
                outputType = GSOutType.Music,
                loop = true,
            },

            [SoundKey.WheelBg] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Music/Normal/WheelBGM.ogg",
                outputType = GSOutType.Music,
                loop = true,
            },

            [SoundKey.ReelRolling] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Normal/ReelRolling.ogg",
            },

            [SoundKey.WinPrize1] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Normal/WinPrize1.ogg",
            },

            [SoundKey.WinPrize2] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Normal/WinPrize2.ogg",
            },

            [SoundKey.WinPrize3] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Normal/WinPrize3.ogg",
            },

            [SoundKey.ReelStop1] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Normal/ReelStop.ogg",
            },

            [SoundKey.ReelStop2] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Normal/ReelStop.ogg",
            },

            [SoundKey.ReelStop3] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Normal/ReelStop.ogg",
            },

            [SoundKey.ReelStop4] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Normal/ReelStop.ogg",
            },

            [SoundKey.ReelStop5] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Normal/ReelStop.ogg",
            },

            [SoundKey.WheelBGMEnding] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Normal/WheelBGMEnding.ogg",
            },

            [SoundKey.WheelButton] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Normal/WheelButton.ogg",
            },

            [SoundKey.WheelRaiseUp] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Normal/WheelRaiseUp.ogg",
            },

            [SoundKey.WheelSpin] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Normal/WheelSpin.ogg",
            },

            [SoundKey.WheellItWin] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Normal/WheellItWin.ogg",
            },

            [SoundKey.BonusWin] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Normal/BonusWin.ogg",
            },

            [SoundKey.ScatterWin] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Normal/ScatterWin.ogg",
            },

            [SoundKey.WildExtend] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Normal/WildExtend.ogg",
            },


            [SoundKey.BigWinStart] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Normal/BigWin.ogg",
            },

            [SoundKey.BigWin] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Normal/BigWinBGM.ogg",
            },

            [SoundKey.SuperWin] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Normal/SuperWin.ogg",
            },

            [SoundKey.MegaWin] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Normal/MegaWin.ogg",
            },

            [SoundKey.BigWinEnd] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Normal/BigWinBGMEnding.ogg",
            },

            [SoundKey.FgBoarderInBGM] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Xing Yun Zhi Lun 3998/Sounds/Sounds/FreeGame/FgBoarderInBGM.ogg",
            },

            [SoundKey.FgSetBoarderIn] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Xing Yun Zhi Lun 3998/Sounds/Sounds/FreeGame/FgSetBoarderIn.ogg",
            },

            [SoundKey.FgBoarderInBGMEnding] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Xing Yun Zhi Lun 3998/Sounds/Sounds/FreeGame/FgBoarderInBGM.ogg",
            },

            [SoundKey.WildShow] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Xing Yun Zhi Lun 3998/Sounds/Sounds/FreeGame/WildShow.ogg",
            },

            [SoundKey.ScatterDown] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Xing Yun Zhi Lun 3998/Sounds/Sounds/FreeGame/WheelStop1.ogg",
            },

            [SoundKey.JackpotTipStart] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Jackpot/JpBoarder.ogg",
            },

            [SoundKey.JackpotTip] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Jackpot/JpWin.ogg",
            },

            [SoundKey.JackpotTipEnd] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Jackpot/BoarderOut.ogg",
            },

            [SoundKey.JackpotSpin] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Jackpot/BonusSpin.ogg",
            },

            [SoundKey.JackpotBoarderIN] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Jackpot/BgBoarderIN.ogg",
            },

            [SoundKey.JackpotBoarderOut] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Sounds/Effect/Jackpot/BgBoarderOut.ogg",
            },



        };
    }
}