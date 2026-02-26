using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMaker
{
    
    public enum SoundKey
    {
        /// <summary> 减注 </summary>
        BetDown,
        /// <summary> 最大注 </summary>
        BetMax,
        /// <summary> 加注 </summary>
        BetUp,
        /// <summary> 正常点击 </summary>
        NormalClick,
        /// <summary> 帮助页面 </summary>
        Tab,
        /// <summary> 关闭弹窗 </summary>
        PopupClose,
        /// <summary> 打开弹窗 </summary>
        PopupOpen,
        /// <summary> 自动玩 点击 </summary>
        SpinAutoClick,
        /// <summary> spin 点击 </summary>
        SpinClick,
        /// <summary> 机台进钱 </summary>
        MachineCoinIn,
        
        TestBGM,
    }
    public class GlobalSoundModel : MonoSingleton<GlobalSoundModel>
    {
        public Dictionary<SoundKey,GSHandler> gsHandlers = new Dictionary<SoundKey,GSHandler> 
        {
            [SoundKey.BetDown] = new GSHandler()
            {
                assetPath = "Assets/GameRes/_Common/Game Maker/Sounds/UI_Betting_Down.wav",
            },
            [SoundKey.BetMax] = new GSHandler()
            {
                assetPath = "Assets/GameRes/_Common/Game Maker/Sounds/UI_Betting_Max.wav",
            },
            [SoundKey.BetUp] = new GSHandler()
            {
                assetPath = "Assets/GameRes/_Common/Game Maker/Sounds/UI_Betting_Up.wav",
            },
            [SoundKey.NormalClick] = new GSHandler()
            {
                assetPath = "Assets/GameRes/_Common/Game Maker/Sounds/UI_Button_Normal.wav",
            },
            [SoundKey.Tab] = new GSHandler()
            {
                assetPath = "Assets/GameRes/_Common/Game Maker/Sounds/UI_Button_Tab.wav",
            },
            [SoundKey.PopupClose] = new GSHandler()
            {
                assetPath = "Assets/GameRes/_Common/Game Maker/Sounds/UI_Popup_Close.wav",
            },
            [SoundKey.PopupOpen] = new GSHandler()
            {
                assetPath = "Assets/GameRes/_Common/Game Maker/Sounds/UI_Popup_Open.wav",
            },
            [SoundKey.SpinAutoClick] = new GSHandler()
            {
                assetPath = "Assets/GameRes/_Common/Game Maker/Sounds/UI_Spin_Auto.wav",
            },
            [SoundKey.SpinClick] = new GSHandler()
            {
                assetPath = "Assets/GameRes/_Common/Game Maker/Sounds/UI_Spin_Start.wav",
            },
            [SoundKey.MachineCoinIn] = new GSHandler()
            {
                assetPath = "Assets/GameRes/_Common/Game Maker/Sounds/Machine_Coin_In.wav",
            },     
            [SoundKey.TestBGM] = new GSHandler()
            {
                assetPath = "Assets/GameRes/_Common/Game Maker/Sounds/BaseBackground.mp3",
                outputType = GSOutType.Music,
                fadeIn = new  GSFadeInOut()
                {
                    easeType = GSEaseType.Linear,
                    time = 2f,
                },
                fadeOut = new  GSFadeInOut()
                {
                    easeType = GSEaseType.Linear,
                    time = 2f,
                }
            },     
        };
    }
}