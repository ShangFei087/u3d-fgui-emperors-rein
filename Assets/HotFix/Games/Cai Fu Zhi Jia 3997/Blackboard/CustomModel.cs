using GameMaker;
using SlotMaker;
using System.Collections.Generic;

namespace CaiFuZhiJia_3997
{
    public class CustomModel : MonoSingleton<CustomModel>, ICustomModel
    {
        public float symbolWidth => 171;
        public float symbolHeight => 169;
        public int column => 5;
        public int row => 3;
        public float reelMaxOffsetY => symbolHeight * row;

        /// <summary>
        /// 说明书路径 在FairyGUI中的路径
        /// </summary>
        public string[] payTable => new[] { "ui://CaiFuZhiJia/PayTable1", "ui://CaiFuZhiJia/PayTable2", "ui://CaiFuZhiJia/PayTable3", "ui://CaiFuZhiJia/PayTable4", "ui://CaiFuZhiJia/PayTable5", "ui://CaiFuZhiJia/PayTable6" };

        /// <summary>
        /// 显示在滚轴上的图标索引
        /// </summary>
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
            11
        };

        public int symbolCount => symbolNumber.Count;

        public Dictionary<string, string> symbolHitEffect => new Dictionary<string, string>()
        {
            { "0", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/Gold_01.prefab" }, // 15 30 90
            { "1", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/Sliver_02.prefab" }, // 15 30 90
            { "2", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/Bar_03.prefab" }, // 15 30 90
            { "3", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/Watch_04.prefab" }, // 20 60 150 
            { "4", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/Dollar_05.prefab" }, // 20 60 150 
            { "5", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/Ring_06.prefab" }, // 30 150 600 
            { "6", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/Car_07.prefab" }, // 30 150 600 
            { "7", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/Ships_08.prefab" }, // 60 600 1500 
            { "8", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/Plane_09.prefab" }, // 300 1500 3000 
            { "9", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/Wild_10.prefab" },
            { "10", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/Scatter_11.prefab" },
            { "11", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/Bonus_12.prefab" },
        };

        public List<int> specialHitSymbols => new List<int>() { };

        public Dictionary<string, string> symbolAppearEffect => new Dictionary<string, string>() { };

        public Dictionary<string, string> symbolExpectationEffect => new Dictionary<string, string>() { };

        public string borderEffect =>
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/Border/JackpotFrame.prefab";

        public Dictionary<string, string> symbolIcon => new Dictionary<string, string>()
        {
            { "0", "ui://CaiFuZhiJia/ng_sym01_gold" }, // 金砖
            { "1", "ui://CaiFuZhiJia/ng_sym02_silver" }, // 银砖
            { "2", "ui://CaiFuZhiJia/ng_sym03_bar" }, // 一堆金砖
            { "3", "ui://CaiFuZhiJia/ng_sym04_watch" }, // 怀表
            { "4", "ui://CaiFuZhiJia/ng_sym05_dollar" }, // 纸币
            { "5", "ui://CaiFuZhiJia/ng_sym06_ring" }, // 钻戒
            { "6", "ui://CaiFuZhiJia/ng_sym07_car" }, // 跑车
            { "7", "ui://CaiFuZhiJia/ng_sym08_ships" }, // 游艇
            { "8", "ui://CaiFuZhiJia/ng_sym09_planes" }, // 飞机
            { "9", "ui://CaiFuZhiJia/ng_sym10_wild" }, // WILD
            { "10", "ui://CaiFuZhiJia/ng_sym11_scatter" }, // Scatter
            { "11", "ui://CaiFuZhiJia/ng_sym_bonus" }, // Bonus
        };

        public List<PayTableSymbolInfo> payTableSymbolWin { get; set; } = new List<PayTableSymbolInfo>()
        {
            new PayTableSymbolInfo()
            {
                symbol = 0,
                x5 = 30,
                x4 = 10,
                x3 = 4,
                x2 = 0,
            },
            new PayTableSymbolInfo()
            {
                symbol = 1,
                x5 = 30,
                x4 = 10,
                x3 = 4,
                x2 = 0,
            },
            new PayTableSymbolInfo()
            {
                symbol = 2,
                x5 = 30,
                x4 = 10,
                x3 = 4,
                x2 = 0,
            },
            new PayTableSymbolInfo()
            {
                symbol = 3,
                x5 = 50,
                x4 = 20,
                x3 = 6,
                x2 = 0,
            },
            new PayTableSymbolInfo()
            {
                symbol = 4,
                x5 = 50,
                x4 = 20,
                x3 = 6,
                x2 = 0,
            },
            new PayTableSymbolInfo()
            {
                symbol = 5,
                x5 = 200,
                x4 = 40,
                x3 = 10,
                x2 = 4,
            },
            new PayTableSymbolInfo()
            {
                symbol = 6,
                x5 = 200,
                x4 = 40,
                x3 = 10,
                x2 = 4,
            },
            new PayTableSymbolInfo()
            {
                symbol = 7,
                x5 = 500,
                x4 = 400,
                x3 = 20,
                x2 = 6,
            },
            new PayTableSymbolInfo()
            {
                symbol = 8,
                x5 = 1000,
                x4 = 800,
                x3 = 40,
                x2 = 8,
            },
            new PayTableSymbolInfo() // wild
            {
                symbol = 9,
                x5 = 0,
                x4 = 0,
                x3 = 0,
                x2 = 0,
            },
            new PayTableSymbolInfo() // scatter
            {
                symbol = 10,
                x5 = 0,
                x4 = 0,
                x3 = 0,
                x2 = 0,
            },
            new PayTableSymbolInfo() // bonus
            {
                symbol = 11,
                x5 = 0,
                x4 = 0,
                x3 = 0,
                x2 = 0,
            },
        };

        public List<List<int>> payLines { get; set; } = new List<List<int>>()
        {
            new List<int>() // 0
            {
                1,
                1,
                1,
                1,
                1
            },
            new List<int>() // 1
            {
                0,
                0,
                0,
                0,
                0
            },
            new List<int>() // 2
            {
                2,
                2,
                2,
                2,
                2
            },
            new List<int>() // 3
            {
                0,
                1,
                2,
                1,
                0
            },
            new List<int>() // 4
            {
                2,
                1,
                0,
                1,
                2
            },
            new List<int>() // 5
            {
                1,
                0,
                0,
                0,
                1
            },
            new List<int>() // 6
            {
                1,
                2,
                2,
                2,
                1
            },
            new List<int>() // 7
            {
                0,
                0,
                1,
                2,
                2
            },
            new List<int>() // 8
            {
                2,
                2,
                1,
                0,
                0
            },
            new List<int>() // 9
            {
                1,
                0,
                1,
                0,
                1
            },
            new List<int>() // 10
            {
                1,
                2,
                1,
                2,
                1
            },
            new List<int>() // 11
            {
                0,
                1,
                1,
                1,
                0
            },
            new List<int>() // 12
            {
                2,
                1,
                1,
                1,
                0
            },
            new List<int>() // 13
            {
                1,
                1,
                0,
                1,
                2
            },
            new List<int>()
            {
                1,
                1,
                2,
                1,
                0
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
                1,
                0,
                2,
                0,
                1
            },
        };

        public List<WinMultiple> winLevelMultiple { get; set; } = new List<WinMultiple>() { new WinMultiple("BIG", 5), new WinMultiple("HUGE", 10), new WinMultiple("MASSIVE", 20), };

        public FreeGameConfig FreeGameConfig { get; } = new FreeGameConfig()
        {
            IsUseCommonFreeTimes = false, //是否使用公共的免费次数框
            IsHasFreeGame = true, //是否有免费奖
            FreeGameType = MakeFreeGameType.OnScatter, //触发免费奖方式
            IsScatterInLine = false, //Scatter图标是否依赖中奖线
            Make2FreeGameCount = new int[] { 3 }, //触发免费奖所需数量(Scatter图标/充能)
            FreeGameTime = new int[] { 10 }, //免费次数
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