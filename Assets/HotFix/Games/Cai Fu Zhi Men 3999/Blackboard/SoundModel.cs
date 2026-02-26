using System.Collections.Generic;

namespace CaiFuZhiMen_3999
{
    public enum SoundKey
    {
        /// <summary> 正常游戏背景音乐 </summary>
        RegularBG,

        /// <summary> 免费游戏背景音乐 </summary>
        FreeSpinBG,

        /// <summary> 游戏空闲音乐 </summary>
        SpinBGIdle,

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
        SlowBG,

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

        /// <summary> 免费游戏触发界面，背景音乐 </summary>
        FreeSpinTriggerBG,

        /// <summary> 免费游戏结束界面，背景音乐 </summary>
        FreeSpinResultBG,

        /// <summary> 免费游戏修改背景音乐 </summary>
        FreeSpinChangeSymbol,

        /// <summary> 增加免费游戏 </summary>
        FreeSpinAdd,

        /// <summary> 免费游戏npc音乐 </summary>
        FreeGameNpc,

        /// <summary> 免费游戏特效音乐 </summary>
        FreeGameEffect,

        /// <summary> 5连线 </summary>
        FiveLine,

        /// <summary> 主游戏赢钱，金币滚动 </summary>
        WinRolling,

        /// <summary> 大奖弹窗分数增加背景音乐 </summary>
        PopupWinBgNumberAdd,

        /// <summary> 大奖弹窗金币音效 </summary>
        PopupWinCoin,

        /// <summary> 大奖弹窗进入 </summary>
        PopupWinOn,

        /// <summary> 大奖弹窗特效 </summary>
        PopupWinEffect,

        /// <summary> 大奖弹窗特效 </summary>
        PopupWinEnd,
        
        /// <summary> 大奖弹窗关闭后,祝贺语 </summary>
        PopupWinAfterCloseCongratulate01,

        /// <summary> 大奖弹窗关闭后,祝贺语 </summary>
        PopupWinAfterCloseCongratulate02,

        /// <summary> 大奖 </summary>
        BigWin,

        /// <summary> 巨奖 </summary>
        MegaWin,

        /// <summary> 超级大奖 </summary>
        SuperWin,
        
        /// <summary> 彩金弹窗，金币滚落 </summary>
        JackpotFlow,

        /// <summary> 彩金弹窗，背景音乐 </summary>
        JackpotBG,

        /// <summary> 彩金弹窗结束音乐 </summary>
        JackpotEnd,
    }

    public class SoundModel : MonoSingleton<SoundModel>
    {
        public Dictionary<SoundKey, GSHandler> gsHandlers = new Dictionary<SoundKey, GSHandler>
        {
            [SoundKey.RegularBG] =
                new GSHandler()
                {
                    assetPath = "Assets/GameRes/Games/Cai Fu Zhi Men 3999/ArtRes/Sounds/Music/NormalBGM.mp3",
                    outputType = GSOutType.Music,
                    loop = true,
                },
            [SoundKey.FreeSpinBG] =
                new GSHandler()
                {
                    assetPath = "Assets/GameRes/Games/Cai Fu Zhi Men 3999/ArtRes/Sounds/Music/FreeBGM.mp3",
                    outputType = GSOutType.Music,
                    loop = true,
                },
            [SoundKey.JackpotBG] =
                new GSHandler()
                {
                    assetPath = "Assets/GameRes/Games/Cai Fu Zhi Men 3999/ArtRes/Sounds/Music/BonusBGM.mp3",
                    outputType = GSOutType.Music,
                    loop = true,
                },
        };
    }
}