using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SlotMaker;

namespace CaiFuHuoChe_3996
{
    public class CustomModel : MonoSingleton<CustomModel>, ICustomModel
    {

        /// <summary> 图标宽 </summary>
        public float symbolWidth => 194;

        /// <summary> 图标高 </summary>
        public float symbolHeight => 194;

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
            "ui://EmperorsRein/Paytable021",
            "ui://EmperorsRein/Paytable022",
            "ui://EmperorsRein/Paytable023",
            "ui://EmperorsRein/Paytable024",
            "ui://EmperorsRein/Paytable025",
            "ui://EmperorsRein/Paytable026",
            "ui://EmperorsRein/Paytable027",
        };


        /// <summary> 通过图标索引，获取图标真实编号 </summary>
        public List<int> symbolNumber => new List<int>() {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};

        /// <summary> 所有图标个数 </summary>
        public int symbolCount => symbolNumber.Count;

        /// <summary> 资源根目录路径 </summary>
        //public string gameAssetsRootFolder = "Assets/GameRes/Games/PssOn00152 (1080x1920)";

        /// <summary> 预制体名称 - 图标中奖特效</summary>
        public Dictionary<string, string> symbolHitEffect => new Dictionary<string, string>
        {
            {"0", "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/Symbols/SymbolHit/SymbolHit0.prefab" },
            {"1", "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/Symbols/SymbolHit/SymbolHit1.prefab" },
            {"2", "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/Symbols/SymbolHit/SymbolHit2.prefab" },
            {"3", "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/Symbols/SymbolHit/SymbolHit3.prefab" },
            {"4", "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/Symbols/SymbolHit/SymbolHit4.prefab" },
            {"5", "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/Symbols/SymbolHit/SymbolHit5.prefab" },
            {"6", "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/Symbols/SymbolHit/SymbolHit6.prefab" },
            {"7", "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/Symbols/SymbolHit/SymbolHit7.prefab" },
            {"8", "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/Symbols/SymbolHit/SymbolHit8.prefab" },
            {"9", "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/Symbols/SymbolHit/SymbolHit9.prefab" },
            {"10", "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/Symbols/SymbolHit/SymbolHit10.prefab" },
            {"11", "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/Symbols/SymbolHit/SymbolHit11.prefab" },
            {"12", "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/Symbols/SymbolHit/SymbolHit12.prefab" },
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
            {"9", "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/Symbols/SymbolAppear/SymbolAppear10.prefab" },
            {"10", "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/Symbols/SymbolAppear/SymbolAppear11.prefab" },
            {"11", "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/Symbols/SymbolAppear/SymbolAppear12.prefab" },
            {"12", "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/Symbols/SymbolAppear/SymbolAppear13.prefab" },
            {"112", "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/Symbols/SymbolAppear/SymbolAppear113.prefab" },
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
        public string borderEffect => "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/Symbols/Border/AnchorBorder.prefab";

        /// <summary> 图片 - 默认图标</summary>
        public Dictionary<string, string> symbolIcon => new Dictionary<string, string>
        {
                {"0", "ui://CaiFuHuoChe_3996/symbol_1" },
                {"1", "ui://CaiFuHuoChe_3996/symbol_2" },
                {"2", "ui://CaiFuHuoChe_3996/symbol_3" },
                {"3", "ui://CaiFuHuoChe_3996/symbol_4" },
                {"4", "ui://CaiFuHuoChe_3996/symbol_5" },
                {"5", "ui://CaiFuHuoChe_3996/symbol_6" },
                {"6", "ui://CaiFuHuoChe_3996/symbol_7" },
                {"7", "ui://CaiFuHuoChe_3996/symbol_8" },
                {"8", "ui://CaiFuHuoChe_3996/symbol_9" },
                {"9", "ui://CaiFuHuoChe_3996/symbol_10" },
                {"10", "ui://CaiFuHuoChe_3996/symbol_11" },
                {"11", "ui://CaiFuHuoChe_3996/symbol_12" },
                {"12", "ui://CaiFuHuoChe_3996/symbol_12" },
        };
    }
}