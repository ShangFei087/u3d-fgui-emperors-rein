using GameMaker;
using SlotMaker;
using System.Collections.Generic;

namespace MeiZhouHeiBao_3993
{
    public class CustomModel : MonoSingleton<CustomModel>, ICustomModel
    {
        public int row => 3;
        public int column => 5;
        public float symbolWidth => 200;
        public float symbolHeight => 200;
        public int symbolCount => symbolNumber.Count;
        public List<int> specialHitSymbols => new List<int>() { };
        public Dictionary<string, string> symbolAppearEffect => new Dictionary<string, string>() 
        {
            { "10", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolAppear/SymbolAppear10.prefab" },
            { "11", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolAppear/SymbolAppear11.prefab" },
            { "12", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolAppear/SymbolAppear12.prefab" },
        };
        public Dictionary<string, string> symbolExpectationEffect => new Dictionary<string, string>() { };
        public float reelMaxOffsetY => symbolHeight * row;

        public string[] payTable => new[]
        {
            "ui://MeiZhouHeiBao/PayTable1",
            "ui://MeiZhouHeiBao/PayTable2",
            "ui://MeiZhouHeiBao/PayTable3",
            "ui://MeiZhouHeiBao/PayTable4",
            "ui://MeiZhouHeiBao/PayTable5"
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
            12
        };

        //图标中奖特效
        public Dictionary<string, string> symbolHitEffect => new Dictionary<string, string>()
        {
            { "0", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolHit/SymbolHit0.prefab" },
            { "1", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolHit/SymbolHit1.prefab" },
            { "2", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolHit/SymbolHit2.prefab" },
            { "3", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolHit/SymbolHit3.prefab" },
            { "4", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolHit/SymbolHit4.prefab" },
            { "5", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolHit/SymbolHit5.prefab" },
            { "6", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolHit/SymbolHit6.prefab" },
            { "7", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolHit/SymbolHit7.prefab" },
            { "8", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolHit/SymbolHit8.prefab" },
            { "9", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolHit/SymbolHit9.prefab" },
            { "10", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolHit/SymbolHit10.prefab" },
            { "11", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolHit/SymbolHit11.prefab" },
            { "12", "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolHit/SymbolHit12.prefab" },
        };

        //中奖框
        public string borderEffect =>"Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/Border/NormalBorder1.prefab";

        /// <summary> 普通局豹头收集：豹头 / Bonus 中奖框。 </summary>
        public string pantherNormalBorderEffect =>
            "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/Border/PantherNormalBorder.prefab";

        /// <summary> 大奖滚轮 Bonus Spine（FguiPool AnchorRoot + Wrapper） </summary>
        public string symbolRewardBonusEffect =>
            "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolSmallGame/SymbolBonus.prefab";

        /// <summary> 彩金滚轮 GoldCoin Spine（MAJOR / MINOR / MINI 分预制体） </summary>
        public string symbolRewardJpMajorEffect =>
            "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolSmallGame/SymbolMajor.prefab";
        public string symbolRewardJpMinorEffect =>
            "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolSmallGame/SymbolMinor.prefab";
        public string symbolRewardJpMiniEffect =>
            "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/Symbols/SymbolSmallGame/SymbolMini.prefab";

        public string GetJackpotPrefab(int jpType)
        {
            if (jpType == 1) return symbolRewardJpMinorEffect;
            if (jpType == 2) return symbolRewardJpMiniEffect;
            return symbolRewardJpMajorEffect;
        }

        //图标UI
        public Dictionary<string, string> symbolIcon => new Dictionary<string, string>()
        {
            { "0", "ui://MeiZhouHeiBao/ng_sym01_10" },
            { "1", "ui://MeiZhouHeiBao/ng_sym02_J" },
            { "2", "ui://MeiZhouHeiBao/ng_sym03_Q" },
            { "3", "ui://MeiZhouHeiBao/ng_sym04_K" },
            { "4", "ui://MeiZhouHeiBao/ng_sym05_A" },
            { "5", "ui://MeiZhouHeiBao/ng_sym06_Snake" },
            { "6", "ui://MeiZhouHeiBao/ng_sym07_Bird" },
            { "7", "ui://MeiZhouHeiBao/ng_sym08_Monkey" },
            { "8", "ui://MeiZhouHeiBao/ng_sym09_Crocodile" },
            { "9", "ui://MeiZhouHeiBao/ng_sym10_BlackPanther" },
            { "10", "ui://MeiZhouHeiBao/ng_sym11_WILD" },
            { "11", "ui://MeiZhouHeiBao/ng_sym12_SCATTER" },
            { "12", "ui://MeiZhouHeiBao/ng_sym13_Bonus" },
            { "13", "ui://MeiZhouHeiBao/ng_sym14_GoldCoin_MINI" },
            { "14", "ui://MeiZhouHeiBao/ng_sym15_GoldCoin_MAJOR" },
            { "15", "ui://MeiZhouHeiBao/ng_sym16_GoldCoin_MINOR" },
        };

        //中奖线
        public List<List<int>> payLines { get; set; } = new List<List<int>>()
        {
            new List<int>() // 0
            {
                1,1,1,1,1
            },
            new List<int>() // 1
            {
                0,0,0,0,0
            },
            new List<int>() // 2
            {
                2,2,2,2,2
            },
            new List<int>() // 3
            {
                0,1,2,1,0
            },
            new List<int>() // 4
            {
                2,1,0,1,2
            },
            new List<int>() // 5
            {
                1,0,0,0,1
            },
            new List<int>() // 6
            {
                1,2,2,2,1
            },
            new List<int>() // 7
            {
                0,0,1,2,2
            },
            new List<int>() // 8
            {
                2,2,1,0,0
            },
            new List<int>() // 9
            {
                1,2,1,0,1
            },
            new List<int>() // 10
            {
                1,0,1,2,1
            },
            new List<int>() // 11
            {
                0,1,1,1,0
            },
            new List<int>() // 12
            {
                2,1,1,1,2
            },
            new List<int>() // 13
            {
                0,1,0,1,0
            },
            new List<int>() // 14
            {
                2,1,2,1,2
            },
            new List<int>() // 15
            {
                1,1,0,1,1
            },
            new List<int>() // 16
            {
                1,1,2,1,1
            },
            new List<int>() // 17
            {
                0, 0,2,0,0
            },
            new List<int>() // 18
            {
                2,2,0,2,2
            },
            new List<int>() // 19
            {
                0,2,2,2,0
            },
            new List<int>() // 20
            {
                2,0,0,0,2
            },
            new List<int>() // 21
            {
                1,2,0,2,1
            },
            new List<int>() // 22
            {
                1,0,2,0,1
            },
            new List<int>() // 23
            {
                0,2,0,2,0
            },
            new List<int>() // 24
            {
                2,0,2,0,2
            },
        };

        //图标赔率
        public List<PayTableSymbolInfo> payTableSymbolWin { get; set; } = new List<PayTableSymbolInfo>()
        {
            new PayTableSymbolInfo()
            {
                symbol = 0,
                x5 = 25,
                x4 = 10,
                x3 = 3,
                x2 = 0,
            },
            new PayTableSymbolInfo()
            {
                symbol = 1,
                x5 = 30,
                x4 = 15,
                x3 = 4,
                x2 = 0,
            },
            new PayTableSymbolInfo()
            {
                symbol = 2,
                x5 = 40,
                x4 = 15,
                x3 = 5,
                x2 = 0,
            },
            new PayTableSymbolInfo()
            {
                symbol = 3,
                x5 = 50,
                x4 = 15,
                x3 = 5,
                x2 = 0,
            },
            new PayTableSymbolInfo()
            {
                symbol = 4,
                x5 = 70,
                x4 = 20,
                x3 = 5,
                x2 = 0,
            },
            new PayTableSymbolInfo()
            {
                symbol = 5,
                x5 = 100,
                x4 = 25,
                x3 = 7,
                x2 = 0,
            },
            new PayTableSymbolInfo()
            {
                symbol = 6,
                x5 = 100,
                x4 = 25,
                x3 = 7,
                x2 = 0,
            },
            new PayTableSymbolInfo()
            {
                symbol = 7,
                x5 = 150,
                x4 = 30,
                x3 = 7,
                x2 = 0,
            },
            new PayTableSymbolInfo()
            {
                symbol = 8,
                x5 = 150,
                x4 = 30,
                x3 = 7,
                x2 = 0,
            },
            new PayTableSymbolInfo() 
            {
                symbol = 9,
                x5 = 250,
                x4 = 50,
                x3 = 10,
                x2 = 0,
            },
            new PayTableSymbolInfo()
            {
                symbol = 10,
                x5 = 0,
                x4 = 0,
                x3 = 0,
                x2 = 0,
            },
            new PayTableSymbolInfo() 
            {
                symbol = 11,
                x5 = 0,
                x4 = 0,
                x3 = 0,
                x2 = 0,
            },
            new PayTableSymbolInfo()
            {
                symbol = 12,
                x5 = 0,
                x4 = 0,
                x3 = 0,
                x2 = 0,
            },
        };

        //bigwin倍数
        public List<WinMultiple> winLevelMultiple { get; set; } = new List<WinMultiple>()
        {
            new WinMultiple("BIG", 5), new WinMultiple("HUGE", 10), new WinMultiple("MASSIVE", 20),
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
        //升级黑豹的图标数量
        public Dictionary<int,int> _upPantherLv => new Dictionary<int, int>()
        {
            { 4 ,5},
            { 10,6},
            { 18,7},
            { 28,8}
        };
    }
}