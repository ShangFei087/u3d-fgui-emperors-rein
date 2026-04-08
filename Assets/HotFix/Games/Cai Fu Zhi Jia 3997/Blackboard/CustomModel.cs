using GameMaker;
using SlotMaker;
using System.Collections.Generic;

namespace CaiFuZhiJia_3997
{
    public class CustomModel : MonoSingleton<CustomModel>, ICustomModel
    {
        // 卷轴上图标的宽高
        public float symbolWidth => 170;
        public float symbolHeight => 170;

        // 卷轴行列
        public int column => 5;
        public int row => 3;

        public float reelMaxOffsetY => symbolHeight * row;

        /// <summary>
        /// 说明书路径 在FairyGUI中的路径
        /// </summary>
        public string[] payTable => new[]
        {
            "ui://CaiFuZhiJia/", "ui://CaiFuZhiJia/", "ui://CaiFuZhiJia/", "ui://CaiFuZhiJia/", "ui://CaiFuZhiJia/"
        };

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

        /// <summary>
        /// 所有图标的个数
        /// </summary>
        public int symbolCount => symbolNumber.Count;

        /// <summary>
        /// 图标中奖Spine动画字典 key是图标索引 value是预制体路径
        /// </summary>
        public Dictionary<string, string> symbolHitEffect => new Dictionary<string, string>()
        {
            { "0", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/Gold_01.prefab" },
            { "1", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/Sliver_02.prefab" },
            { "2", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/Bar_03.prefab" },
            { "3", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/Watch_04.prefab" },
            { "4", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/Dollar_05.prefab" },
            { "5", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/Ring_06.prefab" },
            { "6", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/Car_07.prefab" },
            { "7", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/Ships_08.prefab" },
            { "8", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/Plane_09.prefab" },
            { "9", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/Wild_10.prefab" },
            { "10", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/Scatter_11.prefab" },
            { "11", "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/SymbolAppear/Bonus_12.prefab" },
        };

        /// <summary>
        /// 中线时，播放动画效果和普通牌不一样的图标索引集合
        /// </summary>
        public List<int> specialHitSymbols => new List<int>() { };

        /// <summary>
        /// 滚轮停止时需要播放动画的图标
        /// </summary>
        public Dictionary<string, string> symbolAppearEffect => new Dictionary<string, string>() { };

        /// <summary>
        /// 图标中奖时，播放的粒子特效字典
        /// </summary>
        public Dictionary<string, string> symbolExpectationEffect => new Dictionary<string, string>() { };

        /// <summary>
        /// 边框的路径
        /// </summary>
        public string borderEffect =>
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/Symbols/Border/JackpotFrame.prefab";

        /// <summary>
        /// 滚轴上默认图标路径
        /// </summary>
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
            { "11", "ui://CaiFuZhiJia/ng_sym_bonus_com" }, // Bonus
        };

        public List<PayTableSymbolInfo> payTableSymbolWin { get; set; } = new List<PayTableSymbolInfo>()
        {
            new PayTableSymbolInfo()
            {
                symbol = 0, x5 = 90, x4 = 30, x3 = 15,
            },
            new PayTableSymbolInfo()
            {
                symbol = 1, x5 = 90, x4 = 30, x3 = 15,
            },
            new PayTableSymbolInfo()
            {
                symbol = 2, x5 = 90, x4 = 30, x3 = 15,
            },
            new PayTableSymbolInfo()
            {
                symbol = 3, x5 = 150, x4 = 60, x3 = 20,
            },
            new PayTableSymbolInfo()
            {
                symbol = 4, x5 = 150, x4 = 60, x3 = 20,
            },
            new PayTableSymbolInfo()
            {
                symbol = 5, x5 = 600, x4 = 150, x3 = 30,
            },
            new PayTableSymbolInfo()
            {
                symbol = 6, x5 = 600, x4 = 150, x3 = 30,
            },
            new PayTableSymbolInfo()
            {
                symbol = 7, x5 = 1500, x4 = 600, x3 = 60,
            },
            new PayTableSymbolInfo()
            {
                symbol = 8, x5 = 3000, x4 = 1500, x3 = 300,
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
                0,
                1,
                0,
                1
            },
            new List<int>()
            {
                1,
                2,
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
                0
            },
            new List<int>()
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