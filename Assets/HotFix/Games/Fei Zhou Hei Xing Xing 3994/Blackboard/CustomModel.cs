using GameMaker;
using SlotMaker;
using System.Collections.Generic;

namespace FeiZhouHeiXingXing_3994
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

        public string borderEffect =>
            "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Prefabs/Symbols/Border/JackpotFrame.prefab";

        public string[] payTable => new[] { "ui://FeiZhouHeiXingXing/PayTable1", "ui://FeiZhouHeiXingXing/PayTable2", "ui://FeiZhouHeiXingXing/PayTable3", "ui://FeiZhouHeiXingXing/PayTable4", "ui://FeiZhouHeiXingXing/PayTable5" };

        public List<int> symbolNumber => new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, };

        public Dictionary<string, string> symbolHitEffect => new Dictionary<string, string>()
        {
            { "0", "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Prefabs/Symbols/SymbolAppear/2_J.prefab" },
            { "1", "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Prefabs/Symbols/SymbolAppear/3_Q.prefab" },
            { "2", "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Prefabs/Symbols/SymbolAppear/4_K.prefab" },
            { "3", "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Prefabs/Symbols/SymbolAppear/1_A.prefab" },
            { "4", "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Prefabs/Symbols/SymbolAppear/5_Frog.prefab" },
            { "5", "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Prefabs/Symbols/SymbolAppear/6_Snake.prefab" },
            { "6", "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Prefabs/Symbols/SymbolAppear/7_Parrot.prefab" },
            { "7", "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Prefabs/Symbols/SymbolAppear/8_Leopard.prefab" },
            { "8", "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Prefabs/Symbols/SymbolAppear/9_Chameleon.prefab" },
            { "9", "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Prefabs/Symbols/SymbolAppear/10_Wild.prefab" },
            { "10", "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Prefabs/Symbols/SymbolAppear/11_Scatter.prefab" },
            { "11", "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Prefabs/Symbols/SymbolAppear/12_Bonus.prefab" },
        };

        public Dictionary<string, string> symbolIcon => new Dictionary<string, string>()
        {
            { "0", "ui://FeiZhouHeiXingXing/ng_sym02_j" },
            { "1", "ui://FeiZhouHeiXingXing/ng_sym03_q" },
            { "2", "ui://FeiZhouHeiXingXing/ng_sym04_k" },
            { "3", "ui://FeiZhouHeiXingXing/ng_sym01_a" },
            { "4", "ui://FeiZhouHeiXingXing/ng_sym05_frog" },
            { "5", "ui://FeiZhouHeiXingXing/ng_sym06_snake" },
            { "6", "ui://FeiZhouHeiXingXing/ng_sym07_parrot" },
            { "7", "ui://FeiZhouHeiXingXing/ng_sym08_leopard" },
            { "8", "ui://FeiZhouHeiXingXing/ng_sym09_chameleon" },
            { "9", "ui://FeiZhouHeiXingXing/ng_sym10_WILD_com" },
            { "10", "ui://FeiZhouHeiXingXing/ng_sym12_scatter_com" },
            { "11", "ui://FeiZhouHeiXingXing/ng_sym13_bonus_com" },
        };

        public List<List<int>> payLines { get; set; } = new List<List<int>>()
        {
            new List<int>() // 0
            {
                1, 1, 1, 1, 1
            },
            new List<int>() // 1
            {
                0, 0, 0, 0, 0
            },
            new List<int>() // 2
            {
                2, 2, 2, 2, 2
            },
            new List<int>() // 3
            {
                0, 0, 1, 2, 2
            },
            new List<int>() // 4
            {
                2, 2, 1, 0, 0
            },
            new List<int>() // 5
            {
                0, 2, 0, 2, 0
            },
            new List<int>() // 6
            {
                2, 0, 2, 0, 2
            },
            new List<int>() // 7
            {
                1, 0, 2, 0, 1
            },
            new List<int>() // 8
            {
                1, 2, 0, 2, 1
            },
            new List<int>() // 9
            {
                0, 2, 2, 2, 0
            },
            new List<int>() // 10
            {
                2, 0, 0, 0, 2
            },
            new List<int>() // 11
            {
                0, 1, 2, 1, 0
            },
            new List<int>() // 12
            {
                2, 1, 0, 1, 2
            },
            new List<int>() // 13
            {
                1, 0, 1, 2, 1
            },
            new List<int>() // 14
            {
                1, 2, 1, 0, 1
            },
            new List<int>() // 15
            {
                0, 1, 0, 1, 0
            },
            new List<int>() // 16
            {
                2, 1, 2, 1, 2
            },
            new List<int>() // 17
            {
                1, 0, 0, 0, 1
            },
            new List<int>() // 18
            {
                1, 2, 2, 2, 1
            },
            new List<int>() // 19
            {
                1, 1, 0, 1, 1
            },
            new List<int>() // 20
            {
                1, 1, 2, 1, 1
            },
            new List<int>() // 21
            {
                0, 1, 1, 1, 0
            },
            new List<int>() // 22
            {
                2, 1, 1, 1, 2
            },
            new List<int>() // 23
            {
                0, 2, 1, 2, 0
            },
            new List<int>() // 24
            {
                2, 0, 1, 0, 2
            },
        };

        public List<WinMultiple> winLevelMultiple { get; set; } = new List<WinMultiple>() { new WinMultiple("BIG", 5), new WinMultiple("HUGE", 10), new WinMultiple("MASSIVE", 20), };

        public List<PayTableSymbolInfo> payTableSymbolWin { get; set; } = new List<PayTableSymbolInfo>()
        {
            new PayTableSymbolInfo() // J
            {
                symbol = 0, x5 = 20, x4 = 10, x3 = 5,
            },
            new PayTableSymbolInfo() // Q
            {
                symbol = 1, x5 = 30, x4 = 15, x3 = 5,
            },
            new PayTableSymbolInfo() // K
            {
                symbol = 2, x5 = 35, x4 = 20, x3 = 8,
            },
            new PayTableSymbolInfo() // A
            {
                symbol = 3, x5 = 40, x4 = 20, x3 = 8,
            },
            new PayTableSymbolInfo() // frog
            {
                symbol = 4, x5 = 45, x4 = 25, x3 = 10,
            },
            new PayTableSymbolInfo() // snake
            {
                symbol = 5, x5 = 50, x4 = 30, x3 = 20,
            },
            new PayTableSymbolInfo() // parrot
            {
                symbol = 6, x5 = 60, x4 = 30, x3 = 20,
            },
            new PayTableSymbolInfo() // leopard
            {
                symbol = 7, x5 = 70, x4 = 40, x3 = 30,
            },
            new PayTableSymbolInfo() // chameleon
            {
                symbol = 8, x5 = 80, x4 = 45, x3 = 35,
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

        public FreeGameConfig FreeGameConfig { get; } = new FreeGameConfig()
        {
            IsUseCommonFreeTimes = false, //是否使用公共的免费次数框
            IsHasFreeGame = true, //是否有免费奖
            FreeGameType = MakeFreeGameType.OnScatter, //触发免费奖方式
            IsScatterInLine = false, //Scatter图标是否依赖中奖线
            Make2FreeGameCount = new int[] { 3, 4, 5 }, //触发免费奖所需数量(Scatter图标/充能)
            FreeGameTime = new int[] { 8, 9, 11 }, //免费次数
        };

        public BonusGameConfig BonusGameConfig { get; } = new BonusGameConfig()
        {
            IsHasBonusGame = true, //是否有大奖
            BonusGameType = MakeBonusGameType.OnBonus, // 触发大奖方式
            IsBonusInLine = false, //Bonus图标是否依赖中奖线
            Make2BonusGameCount = 3, //触发大奖所需数量(Bonus图标)
        };
    }
}