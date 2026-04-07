using GameMaker;
using SlotMaker;
using System.Collections.Generic;

namespace CaiFuZhiMen_3999
{
    public class CustomModel : MonoSingleton<CustomModel>, ICustomModel
    {
        public float symbolWidth => 194;
        public float symbolHeight => 172;
        public int column => 5;
        public int row => 3;
        public float reelMaxOffsetY => symbolHeight * row;

        public string[] payTable => new string[5]
        {
            "ui://CaiFuZhiMen/Paytable021", "ui://CaiFuZhiMen/Paytable022", "ui://CaiFuZhiMen/Paytable023",
            "ui://CaiFuZhiMen/Paytable024", "ui://CaiFuZhiMen/Paytable025"
        };

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
        };

        public int symbolCount => symbolNumber.Count;

        public Dictionary<string, string> symbolHitEffect => new Dictionary<string, string>
        {
            { "0", "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/Symbols/SymbolAppear/ScoreTen_0.prefab" },
            { "1", "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/Symbols/SymbolAppear/ScoreJ_1.prefab" },
            { "2", "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/Symbols/SymbolAppear/ScoreQ_2.prefab" },
            { "3", "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/Symbols/SymbolAppear/ScoreK_3.prefab" },
            { "4", "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/Symbols/SymbolAppear/ScoreA_4.prefab" },
            { "5", "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/Symbols/SymbolAppear/YellowBox_5.prefab" },
            { "6", "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/Symbols/SymbolAppear/BlueBox_6.prefab" },
            { "7", "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/Symbols/SymbolAppear/GoldenCup_7.prefab" },
            { "8", "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/Symbols/SymbolAppear/GoldenKey_8.prefab" },
            { "9", "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/Symbols/SymbolAppear/Wild_9.prefab" },
            { "10", "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/Symbols/SymbolAppear/Scatter_10.prefab" },
            { "11", "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/Symbols/SymbolAppear/Bonus_11.prefab" }
        };

        public List<int> specialHitSymbols => new List<int>();
        public Dictionary<string, string> symbolAppearEffect => new Dictionary<string, string>();
        public Dictionary<string, string> symbolExpectationEffect => new Dictionary<string, string>();

        public string borderEffect =>
            "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/Symbols/Border/WinBorder.prefab";

        public Dictionary<string, string> symbolIcon => new Dictionary<string, string>
        {
            { "0", "ui://CaiFuZhiMen/symbol_1" },
            { "1", "ui://CaiFuZhiMen/symbol_2" },
            { "2", "ui://CaiFuZhiMen/symbol_3" },
            { "3", "ui://CaiFuZhiMen/symbol_4" },
            { "4", "ui://CaiFuZhiMen/symbol_5" },
            { "5", "ui://CaiFuZhiMen/symbol_6" },
            { "6", "ui://CaiFuZhiMen/symbol_7" },
            { "7", "ui://CaiFuZhiMen/symbol_8" },
            { "8", "ui://CaiFuZhiMen/symbol_9" },
            { "9", "ui://CaiFuZhiMen/symbol_10" },
            { "10", "ui://CaiFuZhiMen/symbol_11" },
            { "11", "ui://CaiFuZhiMen/symbol_12" }
        };

        public List<PayTableSymbolInfo> payTableSymbolWin { get; set; } = new List<PayTableSymbolInfo>()
        {
            new PayTableSymbolInfo()
            {
                symbol = 0, x5 = 100, x4 = 50, x3 = 25,
            },
            new PayTableSymbolInfo()
            {
                symbol = 1, x5 = 100, x4 = 50, x3 = 25,
            },
            new PayTableSymbolInfo()
            {
                symbol = 2, x5 = 100, x4 = 50, x3 = 25,
            },
            new PayTableSymbolInfo()
            {
                symbol = 3, x5 = 100, x4 = 50, x3 = 25,
            },
            new PayTableSymbolInfo()
            {
                symbol = 4, x5 = 100, x4 = 50, x3 = 25,
            },
            new PayTableSymbolInfo()
            {
                symbol = 5, x5 = 250, x4 = 100, x3 = 50,
            },
            new PayTableSymbolInfo()
            {
                symbol = 6, x5 = 250, x4 = 100, x3 = 50,
            },
            new PayTableSymbolInfo()
            {
                symbol = 7, x5 = 400, x4 = 250, x3 = 100,
            },
            new PayTableSymbolInfo()
            {
                symbol = 8, x5 = 500, x4 = 300, x3 = 150,
            },
            new PayTableSymbolInfo() // wild
            {
                symbol = 9, x5 = 0, x4 = 0, x3 = 0,
            },
            new PayTableSymbolInfo() // scatter
            {
                symbol = 10, x5 = 0, x4 = 0, x3 = 0,
            },
            new PayTableSymbolInfo() // bonus
            {
                symbol = 11, x5 = 0, x4 = 0, x3 = 0,
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
            FreeGameTime = new int[] { 8, 10, 12 }, //免费次数
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