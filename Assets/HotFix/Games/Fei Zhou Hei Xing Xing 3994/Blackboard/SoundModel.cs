using System.Collections.Generic;

namespace FeiZhouHeiXingXing_3994
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

        /// <summary> BONUS图标停止1,2,3,4,5 </summary>
        BonusDown1, BonusDown2, BonusDown3, BonusDown4, BonusDown5,

        /// <summary>  BONUS图标中奖 </summary>
        BonusWin,

        /// <summary> 转轴1:普通滚轮开始转动 </summary>
        NormalRolling,

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

        /// <summary> 免费奖出现框START </summary>
        FreeStartBtn,

        /// <summary> 免费游戏提示框、结算框消失 </summary>
        FreePopupDisappear,

        /// <summary> 免费奖结算框COLLECT </summary>
        FreeCollectBtn,

        //----------------------------------------- BonusPopup--------------------------------
        /// <summary>  Bonus游戏提示框、结算框出现 </summary>
        BonusPopupAppear,

        /// <summary>  Bonus奖出现框START </summary>
        BonusStartBtn,

        /// <summary>  Bonus游戏提示框、结算框消失 </summary>
        BonusPopupDisappear,

        /// <summary>  Bonus奖结算框COLLECT </summary>
        BonusCollectBtn,

        //----------------------------------------- JackpotPopup--------------------------------
        /// <summary>  JACKPOT提示框出现</summary>
        JackpotPopupAppear,

        /// <summary>  JACKPOT提示框关闭 </summary>
        JackpotPopupDisappear,

        //----------------------------------------- FreeGame--------------------------------
        /// <summary> 免费奖期间wild图标发射光的声音 </summary>
        WildTail,

        /// <summary> 箭头1:这个箭头没有积满的声音 </summary>
        arrow1,

        /// <summary> 箭头2:这个箭头积累满的声音 </summary>
        arrow2,

        /// <summary> 齿轮:这个齿轮的声音 </summary>
        Gear,

        //-----------------------------------------BonusGame--------------------------------
        /// <summary> Bonus 小游戏内出现 Bonus 图标相关特效时（停稳展示等）。 </summary>
        BonusSymbolAppear,

        /// <summary> 彩金金币结算：Bonus 小游戏内 Bonus 图标收集。 </summary>
        BonusSymbolCollect,

        /// <summary> 转轴2：Bonus 小游戏转轴滚动。 </summary>
        BonusRolling,

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
            //----------------------------------------- NormalGame --------------------------------
            [SoundKey.ReelStop1] =
                new GSHandler()
                {
                    assetPath =
                        "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/NormalGame/RollDown.mp3",
                },
            [SoundKey.ReelStop2] =
                new GSHandler()
                {
                    assetPath =
                        "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/NormalGame/RollDown.mp3",
                },
            [SoundKey.ReelStop3] =
                new GSHandler()
                {
                    assetPath =
                        "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/NormalGame/RollDown.mp3",
                },
            [SoundKey.ReelStop4] =
                new GSHandler()
                {
                    assetPath =
                        "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/NormalGame/RollDown.mp3",
                },
            [SoundKey.ReelStop5] =
                new GSHandler()
                {
                    assetPath =
                        "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/NormalGame/RollDown.mp3",
                },
            [SoundKey.win_lv1] =
                new GSHandler()
                {
                    assetPath =
                        "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/NormalGame/win1.mp3",
                },
            [SoundKey.win_lv2] =
                new GSHandler()
                {
                    assetPath =
                        "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/NormalGame/win1.mp3",
                },
            [SoundKey.win_lv3] =
                new GSHandler()
                {
                    assetPath =
                        "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/NormalGame/win1.mp3",
                },
            [SoundKey.ScatterWin] =
                new GSHandler()
                {
                    assetPath =
                        "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/NormalGame/ScatterWin.mp3",
                },
            [SoundKey.BonusWin] =
                new GSHandler()
                {
                    assetPath =
                        "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/NormalGame/BonusWin.mp3",
                },
            [SoundKey.NormalRolling] =
                new GSHandler()
                {
                    assetPath =
                        "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/NormalGame/NormalRolling.mp3",
                },
            // [SoundKey.FreeRollingBox] =
            //     new GSHandler()
            //     {
            //         assetPath =
            //             "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/NormalGame/FreeRollingBox.mp3",
            //     },
            // [SoundKey.BonusRollingBox] = new GSHandler()
            // {
            //     assetPath =
            //         "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/NormalGame/FreeRollingBox.mp3",
            // },
            //----------------------------------------- FreePopup --------------------------------
            [SoundKey.FreePopupAppear] =
                new GSHandler()
                {
                    assetPath =
                        "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/FreePopup/FreePopupAppear.mp3",
                },
            [SoundKey.FreeStartBtn] =
                new GSHandler()
                {
                    assetPath =
                        "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/FreePopup/FreeStartBtn.mp3",
                },
            [SoundKey.FreePopupDisappear] =
                new GSHandler()
                {
                    assetPath =
                        "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/FreePopup/FreePopupDisappear.mp3",
                },
            [SoundKey.FreeCollectBtn] = new GSHandler()
            {
                assetPath =
                    "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/FreePopup/FreeCollectBtn.mp3",
            },
            //----------------------------------------- BonusPopup --------------------------------
            [SoundKey.BonusPopupAppear] =
                new GSHandler()
                {
                    assetPath =
                        "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/BonusPopup/BonusPopupAppear.mp3",
                },
            [SoundKey.BonusStartBtn] =
                new GSHandler()
                {
                    assetPath =
                        "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/BonusPopup/BonusStartBtn.mp3",
                },
            [SoundKey.BonusPopupDisappear] =
                new GSHandler()
                {
                    assetPath =
                        "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/BonusPopup/BonusPopupDisappear.mp3",
                },
            [SoundKey.BonusCollectBtn] = new GSHandler()
            {
                assetPath =
                    "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/BonusPopup/BonusCollectBtn.mp3",
            },
            //----------------------------------------- JackpotPopup --------------------------------
            [SoundKey.JackpotPopupAppear] =
                new GSHandler()
                {
                    assetPath =
                        "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/JackpotPopup/JackpotPopupAppear.mp3",
                },
            [SoundKey.JackpotPopupDisappear] = new GSHandler()
            {
                assetPath =
                    "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/JackpotPopup/JackpotPopupDisappear.mp3",
            },
            //----------------------------------------- FreeGame --------------------------------
            [SoundKey.WildTail] =
                new GSHandler()
                {
                    assetPath =
                        "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/FreeGame/WildTail.mp3",
                },
            [SoundKey.arrow1] =
                new GSHandler()
                {
                    assetPath =
                        "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/FreeGame/arrow1.mp3",
                },
            [SoundKey.arrow2] =
                new GSHandler()
                {
                    assetPath =
                        "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/FreeGame/arrow2.mp3",
                },
            [SoundKey.Gear] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/FreeGame/Gear.mp3",
            },
            //----------------------------------------- BonusGame --------------------------------
            [SoundKey.BonusSymbolAppear] =
                new GSHandler()
                {
                    assetPath =
                        "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/BonusGame/BonusSymbolAppear.mp3",
                },
            [SoundKey.BonusSymbolCollect] =
                new GSHandler()
                {
                    assetPath =
                        "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/BonusGame/BonusSymbolCollect.mp3",
                },
            [SoundKey.BonusRolling] = new GSHandler()
            {
                assetPath =
                    "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Effect/BonusGame/BonusRolling.mp3",
            },
            //----------------------------------------- BGM --------------------------------
            [SoundKey.RegularBG] =
                new GSHandler()
                {
                    assetPath = "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Music/NormalBGM.mp3",
                    outputType = GSOutType.Music,
                    loop = true,
                },
            [SoundKey.FreeSpinBG] =
                new GSHandler()
                {
                    assetPath = "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Music/FreeBGM.mp3",
                    outputType = GSOutType.Music,
                    loop = true,
                },
            [SoundKey.BonusTriggerBG] =
                new GSHandler()
                {
                    assetPath = "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Music/FreeTriggerBGM.mp3",
                    outputType = GSOutType.Music,
                    loop = true,
                },
            [SoundKey.FreeSpinTriggerBG] =
                new GSHandler()
                {
                    assetPath = "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Music/FreeTriggerBGM.mp3",
                    outputType = GSOutType.Music,
                    loop = true,
                },
            [SoundKey.FreeSpinResultBG] =
                new GSHandler()
                {
                    assetPath = "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Music/FreeTriggerBGM.mp3",
                    outputType = GSOutType.Music,
                    loop = true,
                },
            [SoundKey.BonusResultBG] =
                new GSHandler()
                {
                    assetPath = "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Music/FreeTriggerBGM.mp3",
                    outputType = GSOutType.Music,
                    loop = true,
                },
            [SoundKey.BonusBG] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Sounds/Music/FreeBGM.mp3",
                outputType = GSOutType.Music,
                loop = true,
            },
        };
    }
}