using FairyGUI;
using SlotMaker;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace SlotEmperorsRein
{

    public class CustomModel : MonoSingleton<CustomModel>, ICustomModel
    {


        /// <summary> 图标宽 </summary>
        public float symbolWidth => 200;

        /// <summary> 图标高 </summary>
        public float symbolHeight => 212;

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
            "ui://EmperorsRein/Paytable1",
            "ui://EmperorsRein/Paytable2",
            "ui://EmperorsRein/Paytable3",
            "ui://EmperorsRein/Paytable4",
            "ui://EmperorsRein/Paytable5",
            "ui://EmperorsRein/Paytable6",
            "ui://EmperorsRein/Paytable7",
        };


        /// <summary> 通过图标索引，获取图标真实编号 </summary>
        public List<int> symbolNumber => new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11};

        /// <summary> 所有图标个数 </summary>
        public int symbolCount => symbolNumber.Count;

        /// <summary> 资源根目录路径 </summary>
        //public string gameAssetsRootFolder = "Assets/GameRes/Games/PssOn00152 (1080x1920)";

        /// <summary> 预制体名称 - 图标中奖特效</summary>
        public Dictionary<string, string> symbolHitEffect => new Dictionary<string, string>
        {
            //{"0", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Symbols/SymbolHit/SymbolHit0 Wild.prefab" },
            {"1", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Symbols/SymbolHit/SymbolHit1.prefab" },
            {"2", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Symbols/SymbolHit/SymbolHit2.prefab" },
            {"3", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Symbols/SymbolHit/SymbolHit3.prefab" },
            {"4", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Symbols/SymbolHit/SymbolHit4.prefab" },
            {"5", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Symbols/SymbolHit/SymbolHit5.prefab" },
            {"6", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Symbols/SymbolHit/SymbolHit6.prefab" },
            {"7", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Symbols/SymbolHit/SymbolHit7.prefab" },
            {"8", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Symbols/SymbolHit/SymbolHit8.prefab" },
            {"9", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Symbols/SymbolHit/SymbolHit9.prefab" },
            {"10", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Symbols/SymbolHit/SymbolHit10 FreeSpin.prefab" },
            {"11", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Symbols/SymbolHit/SymbolHit11 Bonus.prefab" },  // 球 bonus
            {"12", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Symbols/SymbolHit/SymbolHit_JP.prefab" } // jackpot
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
            {"10", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Symbols/SymbolAppear/SymbolAppear10 FreeSpin.prefab" },
            {"11", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Symbols/SymbolAppear/SymbolAppear11 Bonus_ball.prefab" },
            {"12", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Symbols/SymbolAppear/SymbolAppear_JP.prefab" },
        };

        /// <summary> 预制体名称 - 图标中奖特效</summary>
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
        public string borderEffect => "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Symbols/Border/AnchorBorder.prefab";

        /// <summary> 图片 - 默认图标</summary>
        public Dictionary<string, string> symbolIcon => new Dictionary<string, string>
        {
                //{0, "ui://EmperorsRein/symbol0_wild" },
                {"1", "ui://EmperorsRein/symbol1_king" },
                {"2", "ui://EmperorsRein/symbol2_man" },
                {"3", "ui://EmperorsRein/symbol3_woman"},
                {"4", "ui://EmperorsRein/symbol4_bugle" },
                {"5", "ui://EmperorsRein/symbol5_laure"},
                {"6", "ui://EmperorsRein/symbol6_a" },
                {"7", "ui://EmperorsRein/symbol7_k" },
                {"8", "ui://EmperorsRein/symbol8_q" },
                {"9", "ui://EmperorsRein/symbol9_j" },
                {"10", "ui://EmperorsRein/symbol10_free_spin" },
                {"11", "ui://EmperorsRein/symbol11_bonus_ball" },
                {"12", "ui://EmperorsRein/symbol12_jackpot" }

        };

    }
}