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
            { "0", "ui://CaiFuZhiJia/ng_sym01_gold" },
            { "1", "ui://CaiFuZhiJia/ng_sym02_silver" },
            { "2", "ui://CaiFuZhiJia/ng_sym03_bar" },
            { "3", "ui://CaiFuZhiJia/ng_sym04_watch" },
            { "4", "ui://CaiFuZhiJia/ng_sym05_dollar" },
            { "5", "ui://CaiFuZhiJia/ng_sym06_ring" },
            { "6", "ui://CaiFuZhiJia/ng_sym07_car" },
            { "7", "ui://CaiFuZhiJia/ng_sym08_ships" },
            { "8", "ui://CaiFuZhiJia/ng_sym09_planes" },
            { "9", "ui://CaiFuZhiJia/ng_sym10_wild" },
            { "10", "ui://CaiFuZhiJia/ng_sym11_scatter" },
            { "11", "ui://CaiFuZhiJia/ng_sym_bonus_com" },
        };
    }
}