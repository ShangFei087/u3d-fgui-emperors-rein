using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TreasuryHall
{

    public enum SoundKey
    {
        /// <summary> 正常游戏背景音乐 </summary>
        RegularBG,
        /// <summary> 滚轮1停止 </summary>
        TLClickGame,

    }

    public class SoundModel : MonoSingleton<SoundModel>
    {
        public Dictionary<SoundKey, GSHandler> gsHandlers = new Dictionary<SoundKey, GSHandler>
        {


            [SoundKey.RegularBG] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Halls/TreasuryHall/Sounds/Music/TLHallBGM.wav",
                outputType = GSOutType.Music,
                loop = true,
            },
            [SoundKey.TLClickGame] = new GSHandler()
            {
                assetPath = "Assets/GameRes/Halls/TreasuryHall/Sounds/Effect/TLClickGame.mp3",
            },

        };
    }
}