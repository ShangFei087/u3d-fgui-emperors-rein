using SlotMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XingYunZhiLun_3998
{
    public class CustomModel : MonoSingleton<CustomModel>, ICustomModel
    {
        /// <summary> 图标宽 </summary>
        public float symbolWidth => 181;

        /// <summary> 图标高 </summary>
        public float symbolHeight => 177;

        /// <summary> 列 </summary>
        public int column => 5;

        /// <summary> 行 </summary>
        public int row => 3;

        public float reelMaxOffsetY
        {
            get => symbolHeight * row;
        }

        /// <summary> 说明页 </summary>
        public string[] payTable => new string[7]
        {
            "ui://XingYunZhiLun_3998/Paytable01",
            "ui://XingYunZhiLun_3998/Paytable02",
            "ui://XingYunZhiLun_3998/Paytable03",
            "ui://XingYunZhiLun_3998/Paytable04",
            "ui://XingYunZhiLun_3998/Paytable05",
            "ui://XingYunZhiLun_3998/Paytable06",
            "ui://XingYunZhiLun_3998/Paytable07"
        };


        /// <summary> 通过图标索引，获取图标真实编号 </summary>
        public List<int> symbolNumber => new List<int>(){ 0, 1, 2, 3, 4, 5, 6, 7, 8, 9};

        /// <summary> 所有图标个数 </summary>
        public int symbolCount => symbolNumber.Count;

        /// <summary> 资源根目录路径 </summary>
        //public string gameAssetsRootFolder = "Assets/GameRes/Games/PssOn00152 (1080x1920)";

        /// <summary> 预制体名称 - 图标中奖特效</summary>
        public Dictionary<string, string> symbolHitEffect => new Dictionary<string, string>
        {
            {"0", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/SymbolHit/SymbolHit0.prefab" },
            {"1", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/SymbolHit/SymbolHit1.prefab" },
            {"2", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/SymbolHit/SymbolHit2.prefab" },
            {"3", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/SymbolHit/SymbolHit3.prefab" },
            {"4", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/SymbolHit/SymbolHit4.prefab" },
            {"5", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/SymbolHit/SymbolHit5.prefab" },
            {"6", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/SymbolHit/SymbolHit6.prefab" },
            {"7", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/SymbolHit/SymbolHit7.prefab" },
            {"9", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/SymbolHit/SymbolHit8.prefab" },
            {"8", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/SymbolHit/SymbolHit9.prefab" },
            {"10", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/SymbolHit/SymbolHit10.prefab" },

            //中奖数字倍率时键值为"100 + 倍率大小"
            {"103", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X3.prefab"},
            {"104", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X4.prefab"},
            {"105", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X5.prefab"},
            {"106", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X6.prefab"},
            {"107", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X7.prefab"},
            {"108", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X8.prefab"},
            {"109", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X9.prefab"},
            {"1010", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X10.prefab"},
            {"1011", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X11.prefab"},
            {"1015", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X15.prefab"}
        };

        /// <summary>
        /// 特殊图标
        /// </summary>
        /// <param name="index"></param>
        /// <remarks>
        /// * 中线时，播放的动画效果和普通的牌不一样。
        /// </remarks>
        /// <returns></returns>
        public List<int> specialHitSymbols => new List<int> { 0, 1, 2, 3, 4, 5, 6 };


        /// <summary> 特效图标 - 预制体名称</summary>
        /// <remarks>
        /// * 特效图标，滚轮停止时，会播放动画特效的图标。
        /// </remarks>
        public Dictionary<string, string> symbolAppearEffect => new Dictionary<string, string>
        {
            {"6", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/SymbolAppear/SymbolAppear6.prefab" },
            {"7", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/SymbolAppear/SymbolAppear7.prefab" },
            {"9", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/SymbolAppear/SymbolAppear8.prefab" },
            {"8", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/SymbolAppear/SymbolAppear9.prefab" },
            {"10", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/SymbolAppear/SymbolAppear10.prefab" }
        };

        public Dictionary<string, string> jackpotHitEffect => new Dictionary<string, string>
        {
            {"0", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/JackpotHit/JackpotHit0.prefab" },
            {"1", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/JackpotHit/JackpotHit1.prefab" },
            {"2", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/JackpotHit/JackpotHit2.prefab" },
            {"3", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/JackpotHit/JackpotHit3.prefab" },
            {"4", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/JackpotHit/JackpotHit4.prefab" },
            {"5", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/JackpotHit/JackpotHit5.prefab" },
            {"6", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/JackpotHit/JackpotHit6.prefab" },
            {"7", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/JackpotHit/JackpotHit7.prefab" },
            {"8", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/JackpotHit/JackpotHit8.prefab" },
            {"9", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/JackpotHit/JackpotHit9.prefab" },
            {"10", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/JackpotHit/JackpotHit10.prefab" },
        };


        /// <summary> 预制体名称 - 图标中奖粒子特效</summary>
        public Dictionary<string, string> symbolExpectationEffect => new Dictionary<string, string>
        {
            //{0, "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Symbols/SymbolHit/SymbolHit0 Wild.prefab" },
            {"1", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Effects/SmallGame/Art/Effects/Prefabs/ng_eff_sym01_King.prefab" },
            {"2", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Effects/SmallGame/Art/Effects/Prefabs/ng_eff_sym02_MaleWarrior.prefab" },
            {"3", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Effects/SmallGame/Art/Effects/Prefabs/ng_eff_sym03_WomanWarrior.prefab" },
            {"4", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Effects/SmallGame/Art/Effects/Prefabs/ng_eff_sym04_bugle.prefab" },
            {"5", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Effects/SmallGame/Art/Effects/Prefabs/ng_eff_sym05_Laurel.prefab" },
           //{"6", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Symbols/SymbolHit/SymbolHit6.prefab" },
           //{"7", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Symbols/SymbolHit/SymbolHit7.prefab" },
           //{"8", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Symbols/SymbolHit/SymbolHit8.prefab" },
           //{"9", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Symbols/SymbolHit/SymbolHit9.prefab" },
            {"10", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Effects/SmallGame/Art/Effects/Prefabs/ng_eff_sym10_Chest.prefab" },
            {"11", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Effects/SmallGame/Art/Effects/Prefabs/ng_eff_sym11_purse.prefab" },  // 球 bonus
            {"12", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Symbols/SymbolHit/SymbolHit0 Wild.prefab" } // jackpot
        };

        /// <summary> 预制体名称 - 边框特效</summary>
        public string borderEffect => "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/Border/AnchorBorder.prefab";

        /// <summary> 图片 - 默认图标</summary>
        public Dictionary<string, string> symbolIcon => new Dictionary<string, string>
        {
                {"0", "ui://XingYunZhiLun_3998/symbol_1" },
                {"1", "ui://XingYunZhiLun_3998/symbol_2" },
                {"2", "ui://XingYunZhiLun_3998/symbol_3" },
                {"3", "ui://XingYunZhiLun_3998/symbol_4"},
                {"4", "ui://XingYunZhiLun_3998/symbol_5" },
                {"5", "ui://XingYunZhiLun_3998/symbol_6"},
                {"6", "ui://XingYunZhiLun_3998/symbol_7" },
                {"7", "ui://XingYunZhiLun_3998/symbol_8" },
                {"9", "ui://XingYunZhiLun_3998/symbol_9" },
                {"8", "ui://XingYunZhiLun_3998/symbol_diamond" },
                {"10", "ui://XingYunZhiLun_3998/symbol_11" }
        };

        public Dictionary<string, string> wheelSymbolIcon => new Dictionary<string, string>
        {
            {"0", "ui://XingYunZhiLun_3998/symbol_bouns"},
            {"1", "ui://XingYunZhiLun_3998/symbol_lipinghe"},
            {"2", "ui://XingYunZhiLun_3998/symbol_scatter"},
            {"3", "ui://XingYunZhiLun_3998/symbol_wild" },
            {"4", "ui://XingYunZhiLun_3998/symbol_3x" },
            {"5", "ui://XingYunZhiLun_3998/symbol_4x" },
            {"6", "ui://XingYunZhiLun_3998/symbol_5x" },
            {"7", "ui://XingYunZhiLun_3998/symbol_6x" },
            {"8", "ui://XingYunZhiLun_3998/symbol_7x" },
            {"9", "ui://XingYunZhiLun_3998/symbol_8x" },
            {"10", "ui://XingYunZhiLun_3998/symbol_9x" },
            {"11", "ui://XingYunZhiLun_3998/symbol_10x" },
            {"12", "ui://XingYunZhiLun_3998/symbol_11x" },
            {"13", "ui://XingYunZhiLun_3998/symbol_15x" },
        };

        public Dictionary<string, string> wheelSpinPointIcon => new Dictionary<string, string>
        {
            {"mini" , "ui://XingYunZhiLun_3998/sg_img_NormalWheel"},
            {"minor" , "ui://XingYunZhiLun_3998/sg_img_UpgradeWhee" },
            {"major" , "ui://XingYunZhiLun_3998/sg_img_SuperWheel" },
            {"grand" , "ui://XingYunZhiLun_3998/sg_img_SuperWheel" },
        };

        public Dictionary<string, string> ListSymbolsIcon => new Dictionary<string, string>
        {
            {"0", "ui://XingYunZhiLun_3998/ListSymbol0"},
            {"1", "ui://XingYunZhiLun_3998/ListSymbol1"},
            {"2", "ui://XingYunZhiLun_3998/ListSymbol2"},
            {"3", "ui://XingYunZhiLun_3998/ListSymbol3"},
            {"4", "ui://XingYunZhiLun_3998/ListSymbol4"},
            {"5", "ui://XingYunZhiLun_3998/ListSymbol5"},
            {"6", "ui://XingYunZhiLun_3998/ListSymbol6"},
            {"7", "ui://XingYunZhiLun_3998/ListSymbol7"},
            {"8", "ui://XingYunZhiLun_3998/ListSymbol8"},
            {"9", "ui://XingYunZhiLun_3998/ListSymbol9"},
        };


        /// <summary> 倍率中奖的预制体 </summary>
        public Dictionary<string, string> multipleSymbols => new Dictionary<string, string>
        {
            {"3", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X3.prefab"},
            {"4", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X4.prefab"},
            {"5", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X5.prefab"},
            {"6", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X6.prefab"},
            {"7", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X7.prefab"},
            {"8", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X8.prefab"},
            {"9", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X9.prefab"},
            {"10", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X10.prefab"},
            {"11", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X11.prefab"},
            {"15", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X15.prefab"}
        };
    }
}
