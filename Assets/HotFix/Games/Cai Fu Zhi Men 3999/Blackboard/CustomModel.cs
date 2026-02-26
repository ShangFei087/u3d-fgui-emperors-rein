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
        public string[] payTable => new string[5];

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
            {
                "5", "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/Symbols/SymbolAppear/YellowBox_5.prefab"
            },
            { "6", "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/Symbols/SymbolAppear/BlueBox_6.prefab" },
            {
                "7", "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/Symbols/SymbolAppear/GoldenCup_7.prefab"
            },
            {
                "8", "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/Symbols/SymbolAppear/GoldenKey_8.prefab"
            },
            { "9", "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/Symbols/SymbolAppear/Wild_9.prefab" },
            {
                "10", "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/Symbols/SymbolAppear/Scatter_10.prefab"
            },
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
    }
}