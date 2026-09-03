using GameMaker;
using SlotMaker;
using System.Collections.Generic;

namespace CaiShenFaFaFa_3992
{
    public class CustomModel : MonoSingleton<CustomModel>, ICustomModel
    {
        public int row => 3;
        public int column => 5;
        public float symbolWidth => 192;
        public float symbolHeight => 182;
        public int symbolCount => symbolNumber.Count;
        public float reelMaxOffsetY => symbolHeight * row;

        public List<int> specialHitSymbols => new List<int>() { };
        public Dictionary<string, string> symbolAppearEffect => new Dictionary<string, string>() { };
        public Dictionary<string, string> symbolExpectationEffect => new Dictionary<string, string>() { };
        public string borderEffect => "Assets/GameRes/Games/Cai Shen Fa Fa Fa 3992/Prefabs/Symbols/Border/JackpotFrame.prefab";

        public string[] payTable => new[] { "ui://CaiShenFaFaFa/1", "ui://CaiShenFaFaFa/2", "ui://CaiShenFaFaFa/3", "ui://CaiShenFaFaFa/4", "ui://CaiShenFaFaFa/5" };

        public List<int> symbolNumber => new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

        public Dictionary<string, string> symbolHitEffect => new Dictionary<string, string>()
        {
            { "0", "Assets/GameRes/Games/Cai Shen Fa Fa Fa 3992/Prefabs/Symbols/SymbolAppear/0_.prefab" },
            { "1", "Assets/GameRes/Games/Cai Shen Fa Fa Fa 3992/Prefabs/Symbols/SymbolAppear/1_.prefab" },
            { "2", "Assets/GameRes/Games/Cai Shen Fa Fa Fa 3992/Prefabs/Symbols/SymbolAppear/2_.prefab" },
            { "3", "Assets/GameRes/Games/Cai Shen Fa Fa Fa 3992/Prefabs/Symbols/SymbolAppear/3_.prefab" },
            { "4", "Assets/GameRes/Games/Cai Shen Fa Fa Fa 3992/Prefabs/Symbols/SymbolAppear/4_.prefab" },
            { "5", "Assets/GameRes/Games/Cai Shen Fa Fa Fa 3992/Prefabs/Symbols/SymbolAppear/5_.prefab" },
            { "6", "Assets/GameRes/Games/Cai Shen Fa Fa Fa 3992/Prefabs/Symbols/SymbolAppear/6_.prefab" },
            { "7", "Assets/GameRes/Games/Cai Shen Fa Fa Fa 3992/Prefabs/Symbols/SymbolAppear/7_.prefab" },
            { "8", "Assets/GameRes/Games/Cai Shen Fa Fa Fa 3992/Prefabs/Symbols/SymbolAppear/8_.prefab" },
            { "9", "Assets/GameRes/Games/Cai Shen Fa Fa Fa 3992/Prefabs/Symbols/SymbolAppear/9_.prefab" },
            { "10", "Assets/GameRes/Games/Cai Shen Fa Fa Fa 3992/Prefabs/Symbols/SymbolAppear/10_Wild.prefab" },
            { "11", "Assets/GameRes/Games/Cai Shen Fa Fa Fa 3992/Prefabs/Symbols/SymbolAppear/11_Scatter.prefab" },
            { "12", "Assets/GameRes/Games/Cai Shen Fa Fa Fa 3992/Prefabs/Symbols/SymbolAppear/12_Bonus.prefab" },
        };

        public Dictionary<string, string> symbolIcon => new Dictionary<string, string>()
        {
            { "0", "ui://CaiShenFaFaFa/" },
            { "1", "ui://CaiShenFaFaFa/" },
            { "2", "ui://CaiShenFaFaFa/" },
            { "3", "ui://CaiShenFaFaFa/" },
            { "4", "ui://CaiShenFaFaFa/" },
            { "5", "ui://CaiShenFaFaFa/" },
            { "6", "ui://CaiShenFaFaFa/" },
            { "7", "ui://CaiShenFaFaFa/" },
            { "8", "ui://CaiShenFaFaFa/" },
            { "9", "ui://CaiShenFaFaFa/" },
            { "10", "ui://CaiShenFaFaFa/" },
            { "11", "ui://CaiShenFaFaFa/" },
            { "12", "ui://CaiShenFaFaFa/" },
        };

        public List<List<int>> payLines { get; set; } = new List<List<int>>() { };
        public List<WinMultiple> winLevelMultiple { get; set; } = new List<WinMultiple>() { new WinMultiple("BIG", 5), new WinMultiple("HUGE", 10), new WinMultiple("MASSIVE", 20), };

        public List<PayTableSymbolInfo> payTableSymbolWin { get; set; } = new List<PayTableSymbolInfo>()
        {
            new PayTableSymbolInfo() // 
            {
                symbol = 0, x5 = 20, x4 = 10, x3 = 5,
            },
            new PayTableSymbolInfo() // 
            {
                symbol = 1, x5 = 30, x4 = 15, x3 = 5,
            },
            new PayTableSymbolInfo() // 
            {
                symbol = 2, x5 = 35, x4 = 20, x3 = 8,
            },
            new PayTableSymbolInfo() // 
            {
                symbol = 3, x5 = 40, x4 = 20, x3 = 8,
            },
            new PayTableSymbolInfo() // 
            {
                symbol = 4, x5 = 45, x4 = 25, x3 = 10,
            },
            new PayTableSymbolInfo() // 
            {
                symbol = 5, x5 = 50, x4 = 30, x3 = 20,
            },
            new PayTableSymbolInfo() // 
            {
                symbol = 6, x5 = 60, x4 = 30, x3 = 20,
            },
            new PayTableSymbolInfo() // 
            {
                symbol = 7, x5 = 70, x4 = 40, x3 = 30,
            },
            new PayTableSymbolInfo() // 
            {
                symbol = 8, x5 = 80, x4 = 45, x3 = 35,
            },
            new PayTableSymbolInfo() // Wild
            {
                symbol = 10, x5 = 0, x4 = 0, x3 = 0,
            },
            new PayTableSymbolInfo() // Scatter
            {
                symbol = 11, x5 = 0, x4 = 0, x3 = 0,
            },
            new PayTableSymbolInfo() // Bonus
            {
                symbol = 12, x5 = 0, x4 = 0, x3 = 0,
            },
        };

        public FreeGameConfig FreeGameConfig { get; } = new FreeGameConfig()
        {
            IsUseCommonFreeTimes = false, //是否使用公共的免费次数框
            IsHasFreeGame = true, //是否有免费奖
            FreeGameType = MakeFreeGameType.OnScatter, //触发免费奖方式
            IsScatterInLine = false, //Scatter图标是否依赖中奖线
            Make2FreeGameCount = new int[] { 3, 4, 5 }, //触发免费奖所需数量(Scatter图标/充能)
            FreeGameTime = new int[] { 5, 10, 20 }, //免费次数
        };

        public CustomBonusGameConfig BonusGameConfig { get; } = new CustomBonusGameConfig()
        {
            IsHasBonusGame = true, //是否有大奖
            BonusGameType = MakeBonusGameType.OnBonus, // 触发大奖方式
            IsBonusInLine = false, //Bonus图标是否依赖中奖线
            BonusGameDic = new Dictionary<int, int>() { { 3, 6 }, { 4, 8 }, { 5, 10 } }, //大奖游戏次数字典
        };
    }

    public class CustomBonusGameConfig : BonusGameConfig
    {
        /// <summary> Bonus游戏次数字典 </summary>
        public Dictionary<int, int> BonusGameDic { get; set; } = new Dictionary<int, int>();
    }
}