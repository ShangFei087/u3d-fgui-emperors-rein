using GameMaker;
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

        public List<PayTableSymbolInfo> payTableSymbolWin { get; set; } = new List<PayTableSymbolInfo>()
        {
            new PayTableSymbolInfo() // 10
            {
                symbol = 0, x5 = 25, x4 = 10, x3 = 3,
            },
            new PayTableSymbolInfo() // J
            {
                symbol = 1, x5 = 30, x4 = 15, x3 = 4,
            },
            new PayTableSymbolInfo() // Q
            {
                symbol = 2, x5 = 40, x4 = 15, x3 = 5,
            },
            new PayTableSymbolInfo() // K
            {
                symbol = 3, x5 = 50, x4 = 15, x3 = 3,
            },
            new PayTableSymbolInfo() // A
            {
                symbol = 4, x5 = 70, x4 = 20, x3 = 5,
            },
            new PayTableSymbolInfo() // SCATTER
            {
                symbol = 5, x5 = 0, x4 = 0, x3 = 0,
            },
            new PayTableSymbolInfo() // WILD
            {
                symbol = 6, x5 = 0, x4 = 0, x3 = 0,
            },
            new PayTableSymbolInfo() // Crocodile
            {
                symbol = 7, x5 = 150, x4 = 30, x3 = 7,
            },
            new PayTableSymbolInfo() // BlackPanther
            {
                symbol = 8, x5 = 250, x4 = 50, x3 = 10,
            },
            new PayTableSymbolInfo() // orangutan
            {
                symbol = 9, x5 = 150, x4 = 30, x3 = 7,
            },
            new PayTableSymbolInfo() // GoldCoin
            {
                symbol = 10, x5 = 0, x4 = 0, x3 = 0,
            },
            new PayTableSymbolInfo() // Bird
            {
                symbol = 11, x5 = 100, x4 = 25, x3 = 7,
            },
            new PayTableSymbolInfo() // Snake
            {
                symbol = 12, x5 = 100, x4 = 25, x3 = 7,
            },
            new PayTableSymbolInfo() // WILD X2
            {
                symbol = 13, x5 = 0, x4 = 0, x3 = 0,
            },
            new PayTableSymbolInfo() // WILD X3
            {
                symbol = 14, x5 = 0, x4 = 0, x3 = 0,
            },
            new PayTableSymbolInfo() // WILD X5
            {
                symbol = 15, x5 = 0, x4 = 0, x3 = 0,
            },
        };

        public List<List<int>> payLines { get; set; } = new List<List<int>>()
        {
            new List<int>()
            {
                1,
                1,
                1,
                1,
                1
            },
            new List<int>()
            {
                0,
                0,
                0,
                0,
                0
            },
            new List<int>()
            {
                2,
                2,
                2,
                2,
                2
            },
            new List<int>()
            {
                0,
                1,
                2,
                1,
                0
            },
            new List<int>()
            {
                2,
                1,
                0,
                1,
                2
            },
            new List<int>()
            {
                1,
                0,
                0,
                0,
                1
            },
            new List<int>()
            {
                1,
                2,
                2,
                2,
                1
            },
            new List<int>()
            {
                0,
                0,
                1,
                2,
                2
            },
            new List<int>()
            {
                2,
                2,
                1,
                0,
                0
            },
            new List<int>()
            {
                1,
                2,
                1,
                0,
                1
            },
            new List<int>()
            {
                1,
                0,
                1,
                2,
                1
            },
            new List<int>()
            {
                0,
                1,
                1,
                1,
                0
            },
            new List<int>()
            {
                2,
                1,
                1,
                1,
                2
            },
            new List<int>()
            {
                0,
                1,
                0,
                1,
                0
            },
            new List<int>()
            {
                2,
                1,
                2,
                1,
                2
            },
            new List<int>()
            {
                1,
                1,
                0,
                1,
                1
            },
            new List<int>()
            {
                1,
                1,
                2,
                1,
                1
            },
            new List<int>()
            {
                0,
                0,
                2,
                0,
                0
            },
            new List<int>()
            {
                2,
                2,
                0,
                2,
                2
            },
            new List<int>()
            {
                0,
                2,
                2,
                2,
                0
            },
            new List<int>()
            {
                2,
                0,
                0,
                0,
                2
            },
            new List<int>()
            {
                1,
                2,
                0,
                2,
                1
            },
            new List<int>()
            {
                1,
                0,
                2,
                0,
                1
            },
            new List<int>()
            {
                0,
                2,
                0,
                2,
                0
            },
            new List<int>()
            {
                2,
                0,
                2,
                0,
                2
            },
        };

        public List<WinMultiple> winLevelMultiple { get; set; } = new List<WinMultiple>()
        {
            new WinMultiple("BIG", 15),
            new WinMultiple("HUGE", 30),
            new WinMultiple("MASSIVE", 50),
            new WinMultiple("LEGENDARY", 100)
        };

        public FreeGameConfig FreeGameConfig { get; } = new FreeGameConfig()
        {
            IsUseCommonFreeTimes = false, //是否使用公共的免费次数框
            IsHasFreeGame = true, //是否有免费奖
            FreeGameType = MakeFreeGameType.OnScatter, //触发免费奖方式
            IsScatterInLine = false, //Scatter图标是否依赖中奖线
            Make2FreeGameCount = new int[] { 3, 4, 5 }, //触发免费奖所需数量(Scatter图标/充能)
            FreeGameTime = new int[] { 8, 15, 20 }, //免费次数
        };

        public BonusGameConfig BonusGameConfig { get; } = new BonusGameConfig()
        {
            IsHasBonusGame = true, //是否有大奖
            BonusGameType = MakeBonusGameType.OnBonus, // 触发大奖方式
            IsBonusInLine = false, //Bonus图标是否依赖中奖线
            Make2BonusGameCount = 6, //触发大奖所需数量(Bonus图标)
        };
    }
}