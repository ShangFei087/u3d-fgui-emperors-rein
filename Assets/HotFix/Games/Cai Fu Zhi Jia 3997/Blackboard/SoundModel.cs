using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaiFuZhiJia_3997
{
    public enum SoundKey
    {
        //----------------------------------------- NormalGame--------------------------------
        /// <summary> 滚轮1,2,3,4,5停止 </summary>
        ReelStop1, ReelStop2, ReelStop3, ReelStop4, ReelStop5,
        /// <summary> 低分值中奖 </summary>
        win_lv1,
        /// <summary> 中分值中奖 </summary>
        win_lv2,
        /// <summary> 高分值中奖 </summary>
        win_lv3,
        /// <summary> SCATTER图标停止 </summary>
        ScatterDown,
        /// <summary> SCATTER图标中奖 </summary>
        ScatterWin,
        /// <summary> BONUS图标停止 </summary>
        BonusDown1, BonusDown2, BonusDown3, BonusDown4, BonusDown5,
        /// <summary>  BONUS图标中奖 </summary>
        BonusWin,
        /// <summary> 蓝色加速框使用:scatter加速框 </summary>
        FreeRollingBox,
        /// <summary> 金色加速框使用：bonus加速框 </summary>
        BonusRollingBox,
        /// <summary> 进入、退出免费游戏过场 </summary>
        FadeFree,
        /// <summary> 进入、退出彩金（Bonus）小游戏过场 </summary>
        FadeBonus,
        //----------------------------------------- FreePopup--------------------------------
        /// <summary> 免费游戏提示框、结算框出现 </summary>
        FreePopupAppear,
        /// <summary> 免费游戏提示框、结算框消失 </summary>
        FreePopupDisappear,
        //----------------------------------------- BonusPopup--------------------------------
        /// <summary>  Bonus游戏提示框、结算框出现 </summary>
        BonusPopupAppear,
        /// <summary>  Bonus游戏提示框、结算框消失 </summary>
        BonusPopupDisappear,
        //----------------------------------------- BGM--------------------------------
        /// <summary> 正常游戏背景音乐 </summary>
        RegularBG,
        /// <summary> 免费游戏背景音乐 </summary>
        FreeSpinBG,
        /// <summary> 大奖背景音乐 </summary>
        BonusBG,
        /// <summary> 免费游戏触发界面，背景音乐 </summary>
        FreeSpinTriggerBG,
        /// <summary> 免费游戏结束界面，背景音乐 </summary>
        FreeSpinResultBG,
        /// <summary> 大奖触发弹窗显示 </summary>
        BonusTriggerBG,
        /// <summary> 大奖接受弹窗显示 </summary>
        BonusResultBG,
    }

    public class SoundModel : MonoSingleton<SoundModel>
    {
        public Dictionary<SoundKey, GSHandler> gsHandlers = new Dictionary<SoundKey, GSHandler>
        {
            //----------------------------------------- NormalGame--------------------------------
            [SoundKey.ReelStop1] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Effect/NormalGame/RollDown.mp3",
            },
            [SoundKey.ReelStop2] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Effect/NormalGame/RollDown.mp3",
            },
            [SoundKey.ReelStop3] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Effect/NormalGame/RollDown.mp3",
            },
            [SoundKey.ReelStop4] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Effect/NormalGame/RollDown.mp3",
            },
            [SoundKey.ReelStop5] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Effect/NormalGame/RollDown.mp3",
            },
            [SoundKey.win_lv1] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Effect/NormalGame/win1.mp3",
            },
            [SoundKey.win_lv2] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Effect/NormalGame/win2.mp3",
            },
            [SoundKey.win_lv3] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Effect/NormalGame/win3.mp3",
            },
            [SoundKey.ScatterDown] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Effect/NormalGame/ScatterDown.mp3",
            },
            [SoundKey.ScatterWin] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Effect/NormalGame/ScatterWin.mp3",
            },
            [SoundKey.BonusDown1] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Effect/NormalGame/BonusDown.mp3",
            },
            [SoundKey.BonusDown2] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Effect/NormalGame/BonusDown.mp3",
            },
            [SoundKey.BonusDown3] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Effect/NormalGame/BonusDown.mp3",
            },
            [SoundKey.BonusDown4] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Effect/NormalGame/BonusDown.mp3",
            },
            [SoundKey.BonusDown5] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Effect/NormalGame/BonusDown.mp3",
            },
            [SoundKey.BonusWin] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Effect/NormalGame/BonusWin.mp3",
            },
            [SoundKey.FreeRollingBox] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Effect/NormalGame/LabelRolling.mp3",
            },
            [SoundKey.BonusRollingBox] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Effect/NormalGame/LabelRolling.mp3",
            },
            [SoundKey.FadeFree] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Effect/NormalGame/FadeFree.mp3",
            },
            [SoundKey.FadeBonus] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Effect/NormalGame/FadeBonus.mp3",
            },
            //----------------------------------------- FreePopup--------------------------------
            [SoundKey.FreePopupAppear] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Effect/FreePopup/FreePopupAppear.mp3",
            },
            [SoundKey.FreePopupDisappear] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Effect/FreePopup/FreePopupDisappear.mp3",
            },
            //----------------------------------------- BonusPopup--------------------------------
            [SoundKey.BonusPopupAppear] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Effect/BonusPopup/BonusPopupAppear.mp3",
            },
            [SoundKey.BonusPopupDisappear] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Effect/BonusPopup/BonusPopupDisappear.mp3",
            },
            //----------------------------------------- BGM--------------------------------
            [SoundKey.RegularBG] =
                new GSHandler()
                {
                    assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Music/NormalBGM.mp3",
                    outputType = GSOutType.Music,
                    loop = true,
                },
            [SoundKey.FreeSpinBG] =
                new GSHandler()
                {
                    assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Music/FreeBGM.mp3",
                    outputType = GSOutType.Music,
                    loop = true,
                },
            [SoundKey.BonusBG] =
                new GSHandler()
                {
                    assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Music/BonusBGM.mp3",
                    outputType = GSOutType.Music,
                    loop = true,
                },
            [SoundKey.FreeSpinTriggerBG] =
                new GSHandler()
                {
                    assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Music/FreeTriggerBGM.mp3",
                    outputType = GSOutType.Music,
                    loop = true,
                },
            [SoundKey.FreeSpinResultBG] =
                new GSHandler()
                {
                    assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Music/FreeTriggerBGM.mp3",
                    outputType = GSOutType.Music,
                    loop = true,
                },
            [SoundKey.BonusTriggerBG] =
                new GSHandler()
                {
                    assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Music/BonusTriggerBGM.mp3",
                    outputType = GSOutType.Music,
                    loop = true,
                },
            [SoundKey.BonusResultBG] =
                new GSHandler()
                {
                    assetPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Sounds/Music/BonusTriggerBGM.mp3",
                    outputType = GSOutType.Music,
                    loop = true,
                },
        };
    }
}