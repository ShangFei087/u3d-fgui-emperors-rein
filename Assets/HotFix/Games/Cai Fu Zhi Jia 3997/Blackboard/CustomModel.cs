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
            { "0", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/0_Gold.prefab" }, // 4, 10, 20
            { "1", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/1_Sliver.prefab" }, // 4, 10, 20
            { "2", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/2_Bar.prefab" }, // 4, 10, 20
            { "3", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/3_Watch.prefab" }, // 6, 20, 40
            { "4", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/4_Dollar.prefab" }, // 6, 20, 40
            { "5", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/5_Ring.prefab" }, // 10, 40, 60
            { "6", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/6_Bottle.prefab" }, // 10, 40, 80 
            { "7", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/7_Key.prefab" }, // 20, 60, 100 
            { "8", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/8_Car.prefab" }, // 40, 80, 120 
            { "9", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/9_Wild.prefab" },
            { "10", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/10_Scatter.prefab" },
            { "11", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/11_Bonus.prefab" },
        };

        public List<int> specialHitSymbols => new List<int>() { };

        public Dictionary<string, string> symbolAppearEffect => new Dictionary<string, string>() { };

        public Dictionary<string, string> symbolExpectationEffect => new Dictionary<string, string>() { };

        public string borderEffect =>
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/Border/normalFrame.prefab";

        public Dictionary<string, string> symbolIcon => new Dictionary<string, string>()
        {
            { "0", "ui://CaiFuZhiJia/ng_sym01_gold" }, // 单条金砖
            { "1", "ui://CaiFuZhiJia/ng_sym02_silver" }, // 堆叠银砖
            { "2", "ui://CaiFuZhiJia/ng_sym01_bar" }, // 堆叠金砖
            { "3", "ui://CaiFuZhiJia/ng_sym04_watch" }, // 金表
            { "4", "ui://CaiFuZhiJia/ng_sym05_mani" }, // 美元
            { "5", "ui://CaiFuZhiJia/ng_sym06_jezhi" }, // 传家宝戒
            { "6", "ui://CaiFuZhiJia/ng_sym07_jiu" }, // 水晶酒瓶
            { "7", "ui://CaiFuZhiJia/ng_sym08_yaos" }, // 豪宅钥匙
            { "8", "ui://CaiFuZhiJia/ng_sym09_che" }, // 名贵跑车
            { "9", "ui://CaiFuZhiJia/ng_sym10_wild_com" }, // WILD
            { "10", "ui://CaiFuZhiJia/ng_sym11_scatter_com" }, // Scatter
            { "11", "ui://CaiFuZhiJia/ng_sym12_bonus_com" }, // Bonus
        };

        public List<PayTableSymbolInfo> payTableSymbolWin { get; set; } = new List<PayTableSymbolInfo>()
        {
            new PayTableSymbolInfo()// 单条金砖
            {
                symbol = 0,
                x5 = 20,
                x4 = 10,
                x3 = 4,
                x2 = 0,
            },
            new PayTableSymbolInfo()// 堆叠银砖
            {
                symbol = 1,
                x5 = 20,
                x4 = 10,
                x3 = 4,
                x2 = 0,
            },
            new PayTableSymbolInfo()// 堆叠金砖
            {
                symbol = 2,
                x5 = 20,
                x4 = 10,
                x3 = 4,
                x2 = 0,
            },
            new PayTableSymbolInfo()// 金表
            {
                symbol = 3,
                x5 = 40,
                x4 = 20,
                x3 = 6,
                x2 = 0,
            },
            new PayTableSymbolInfo()// 美元
            {
                symbol = 4,
                x5 = 40,
                x4 = 20,
                x3 = 6,
                x2 = 0,
            },
            new PayTableSymbolInfo()// 传家宝戒
            {
                symbol = 5,
                x5 = 60,
                x4 = 40,
                x3 = 10,
                x2 = 0,
            },
            new PayTableSymbolInfo()// 水晶酒瓶
            {
                symbol = 6,
                x5 = 80,
                x4 = 40,
                x3 = 10,
                x2 = 0,
            },
            new PayTableSymbolInfo()// 豪宅钥匙
            {
                symbol = 7,
                x5 = 100,
                x4 = 60,
                x3 = 20,
                x2 = 0,
            },
            new PayTableSymbolInfo()// 名贵跑车
            {
                symbol = 8,
                x5 = 120,
                x4 = 80,
                x3 = 40,
                x2 = 0,
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