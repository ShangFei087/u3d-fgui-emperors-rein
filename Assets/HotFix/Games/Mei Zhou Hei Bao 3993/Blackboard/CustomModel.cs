using SlotMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    public class CustomModel : MonoSingleton<CustomModel>, ICustomModel
    {
        public float symbolWidth => 145;
        public float symbolHeight => 133;
        public int column => 5;
        public int row => 3;
        public float reelMaxOffsetY => symbolHeight * row;
        public string[] payTable => new string[] { "" };

        public List<int> symbolNumber => new List<int>()
        {
            0,
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            9,
            10,
            11,
            12,
            13,
            14,
            15,
        };

        public int symbolCount => symbolNumber.Count;

        public Dictionary<string, string> symbolHitEffect => new Dictionary<string, string>
        {
            { "0", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolAppear/ng_sym01_10.prefab" },
            { "1", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolAppear/ng_sym02_J.prefab" },
            { "2", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolAppear/ng_sym03_Q.prefab" },
            { "3", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolAppear/ng_sym04_K.prefab" },
            { "4", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolAppear/ng_sym05_A.prefab" },
            { "5", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolAppear/ng_sym06_SCATTER.prefab" },
            { "6", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolAppear/ng_sym07_WILD.prefab" },
            {
                "7", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolAppear/ng_sym08_Crocodile.prefab"
            },
            {
                "8",
                "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolAppear/ng_sym09_BlackPanther.prefab"
            },
            {
                "9", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolAppear/ng_sym10_orangutan.prefab"
            },
            {
                "10", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolAppear/ng_sym11_GoldCoin.prefab"
            },
            { "11", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolAppear/ng_sym12_Bird.prefab" },
            { "12", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolAppear/ng_sym13_Snake.prefab" },
            { "13", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolAppear/ng_sym14_WILD_X2.prefab" },
            { "14", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolAppear/ng_sym15_WILD_X3.prefab" },
            { "15", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolAppear/ng_sym16_WILD_X5.prefab" },
        };

        public List<int> specialHitSymbols => new List<int>();
        public Dictionary<string, string> symbolAppearEffect => new Dictionary<string, string>();
        public Dictionary<string, string> symbolExpectationEffect => new Dictionary<string, string>();

        public string borderEffect =>
            "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/Border/NormalWinBorder.prefab";

        public Dictionary<string, string> symbolIcon => new Dictionary<string, string>
        {
            { "0", "ui://MeiZhouHeiBao/ng_sym01_10" },
            { "1", "ui://MeiZhouHeiBao/ng_sym02_J" },
            { "2", "ui://MeiZhouHeiBao/ng_sym03_Q" },
            { "3", "ui://MeiZhouHeiBao/ng_sym04_K" },
            { "4", "ui://MeiZhouHeiBao/ng_sym05_A" },
            { "5", "ui://MeiZhouHeiBao/ng_sym06_SCATTER" },
            { "6", "ui://MeiZhouHeiBao/ng_sym07_WILD" },
            { "7", "ui://MeiZhouHeiBao/ng_sym08_Crocodile" },
            { "8", "ui://MeiZhouHeiBao/ng_sym09_BlackPanther" },
            { "9", "ui://MeiZhouHeiBao/ng_sym10_orangutan" },
            { "10", "ui://MeiZhouHeiBao/ng_sym11_GoldCoin" },
            { "11", "ui://MeiZhouHeiBao/ng_sym12_Bird" },
            { "12", "ui://MeiZhouHeiBao/ng_sym13_Snake" },
            { "13", "ui://MeiZhouHeiBao/ng_sym07_WILD_X2" },
            { "14", "ui://MeiZhouHeiBao/ng_sym07_WILD_X3" },
            { "15", "ui://MeiZhouHeiBao/ng_sym07_WILD_X5" },
        };
    }
}