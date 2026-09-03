using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SlotMaker;
using GameMaker;

namespace HuoYanGongNiu_3995
{
    public class CustomModel : MonoSingleton<CustomModel>, ICustomModel
    {

        /// <summary> 图标宽 </summary>
        public float symbolWidth => 202;

        /// <summary> 图标高 </summary>
        public float symbolHeight => 197;

        /// <summary> 列 </summary>
        public int column => 5;

        /// <summary> 行 </summary>
        public int row => 3;


        public float reelMaxOffsetY
        {
            get => symbolHeight * row;
        }

        /// <summary> 说明页 </summary>
        public string[] payTable => new string[6]
        {
            "ui://HuoYanGongNiu_3995/Paytable1",
            "ui://HuoYanGongNiu_3995/Paytable2",
            "ui://HuoYanGongNiu_3995/Paytable3",
            "ui://HuoYanGongNiu_3995/Paytable4",
            "ui://HuoYanGongNiu_3995/Paytable5",
            "ui://HuoYanGongNiu_3995/Paytable6",
        };


        /// <summary> 通过图标索引，获取图标真实编号 </summary>
        public List<int> symbolNumber => new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 22, 33};

        /// <summary> 所有图标个数 </summary>
        public int symbolCount => symbolNumber.Count;

        /// <summary> 资源根目录路径 </summary>
        //public string gameAssetsRootFolder = "Assets/GameRes/Games/PssOn00152 (1080x1920)";

        /// <summary> 预制体名称 - 图标中奖特效</summary>
        public Dictionary<string, string> symbolHitEffect => new Dictionary<string, string>
        {
            {"0", "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Symbols/SymbolHit/SymbolHit0.prefab" },
            {"1", "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Symbols/SymbolHit/SymbolHit1.prefab" },
            {"2", "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Symbols/SymbolHit/SymbolHit2.prefab" },
            {"3", "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Symbols/SymbolHit/SymbolHit3.prefab" },
            {"4", "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Symbols/SymbolHit/SymbolHit4.prefab" },
            {"5", "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Symbols/SymbolHit/SymbolHit5.prefab" },
            {"6", "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Symbols/SymbolHit/SymbolHit6.prefab" },
            {"7", "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Symbols/SymbolHit/SymbolHit7.prefab" },
            {"8", "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Symbols/SymbolHit/SymbolHit8.prefab" },
            {"9", "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Symbols/SymbolHit/SymbolHit9.prefab" },
            {"10", "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Symbols/SymbolHit/SymbolHit10.prefab" },
            {"11", "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Symbols/SymbolHit/SymbolHit11.prefab" },
            {"12", "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Symbols/SymbolHit/SymbolHit12.prefab" },
            {"13", "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Symbols/SymbolHit/SymbolHit13.prefab" },
            {"14", "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Symbols/SymbolHit/SymbolHit14.prefab" },
            {"22", "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Symbols/SymbolHit/SymbolHit22.prefab" },
            {"33", "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Symbols/SymbolHit/SymbolHit33.prefab" },
            //{"12", "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Symbols/SymbolHit/SymbolHit12.prefab" },
        };

        /// <summary>
        /// 特殊图标
        /// </summary>
        /// <param name="index"></param>
        /// <remarks>
        /// * 中线时，播放的动画效果和普通的牌不一样。
        /// </remarks>
        /// <returns></returns>
        public List<int> specialHitSymbols => new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 22, 33};


        /// <summary> 特效图标 - 预制体名称</summary>
        /// <remarks>
        /// * 特效图标，滚轮停止时，会播放动画特效的图标。
        /// </remarks>
        public Dictionary<string, string> symbolAppearEffect => new Dictionary<string, string>
        {
            {"16", "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Symbols/SymbolAppear/SymbolAppear16.prefab" },
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
        public string borderEffect => "Assets/GameRes/Games/Huo Yan Gong Niu 3995/Prefabs/Symbols/Border/AnchorBorder.prefab";

        /// <summary> 图片 - 默认图标</summary>
        public Dictionary<string, string> symbolIcon => new Dictionary<string, string>
        {
                {"0", "ui://HuoYanGongNiu_3995/ng_sym_9" },
                {"1", "ui://HuoYanGongNiu_3995/ng_sym_10" },
                {"2", "ui://HuoYanGongNiu_3995/ng_sym_J" },
                {"3", "ui://HuoYanGongNiu_3995/ng_sym_Q" },
                {"4", "ui://HuoYanGongNiu_3995/ng_sym_K" },
                {"5", "ui://HuoYanGongNiu_3995/ng_sym_A" },
                {"6", "ui://HuoYanGongNiu_3995/ng_sym_Deer" },
                {"7", "ui://HuoYanGongNiu_3995/ng_sym_Wolf" },
                {"8", "ui://HuoYanGongNiu_3995/ng_sym_Leopard" },
                {"9", "ui://HuoYanGongNiu_3995/ng_sym_Eagle" },
                {"10", "ui://HuoYanGongNiu_3995/ng_sym_Bull" },


                {"11", "ui://HuoYanGongNiu_3995/ng_sym_WILD" },

                {"12", "ui://HuoYanGongNiu_3995/ng_sym_Scatter" },

                {"13", "ui://HuoYanGongNiu_3995/ng_sym_Bonus" },
                {"14", "ui://HuoYanGongNiu_3995/ng_sym_Taurus" },

                {"15", "ui://HuoYanGongNiu_3995/ng_sym_Bonus" },

                {"22", "ui://HuoYanGongNiu_3995/ng_sym_WILD_X2" },
                {"33", "ui://HuoYanGongNiu_3995/ng_sym_WILD_X3" },
        };

        //转盘金牛图标
        public Dictionary<int, string> wheelGoldBull => new Dictionary<int, string>
        {
            {0,  "ui://HuoYanGongNiu_3995/OneGoldBull"},
            {1,  "ui://HuoYanGongNiu_3995/TwoGoldBull"},
            {2,  "ui://HuoYanGongNiu_3995/ThreeGoldBull"},
            {3,  "ui://HuoYanGongNiu_3995/FourGoldBull"},
            {4,  "ui://HuoYanGongNiu_3995/FiveBull"},
        };


        public readonly List<List<int>> wheelCredit = new List<List<int>>()
        {
            new List<int> { 2, 3, 4, 5, 2, 3},
            new List<int> { 3, 4, 5, 6, 3, 4},
            new List<int> { 4, 5, 6, 7, 4, 5},
            new List<int> { 5, 6, 7, 8, 5, 6},
            new List<int> { 6, 7, 8, 9, 6, 7},
        };


        //List<PayTableSymbolInfo> ICustomModel.payTableSymbolWin { get => payTableSymbolWin; set => throw new System.NotImplementedException(); }
        //List<List<int>> ICustomModel.payLines { get => payLines; set => throw new System.NotImplementedException(); }
        //List<WinMultiple> ICustomModel.winLevelMultiple { get => winLevelMultiple; set => throw new System.NotImplementedException(); }

        //public List<PayTableSymbolInfo> payTableSymbolWin => new List<PayTableSymbolInfo>
        //{
        //    new PayTableSymbolInfo()
        //    {
        //        symbol = 0,
        //        x5 = 0,
        //        x4 = 0,
        //        x3 = 0,
        //    },
        //    new PayTableSymbolInfo()
        //    {
        //        symbol = 1,
        //        x5 = 0,
        //        x4 = 0,
        //        x3 = 2,
        //    },
        //    new PayTableSymbolInfo()
        //    {
        //        symbol = 2,
        //        x5 = 10,
        //        x4 = 4,
        //        x3 = 1,
        //    },
        //    new PayTableSymbolInfo()
        //    {
        //        symbol = 3,
        //        x5 = 6,
        //        x4 = 2,
        //        x3 = 6,
        //    },
        //    new PayTableSymbolInfo()
        //    {
        //        symbol = 4,
        //        x5 = 2,
        //        x4 = 1,
        //        x3 = 5,
        //    },
        //    new PayTableSymbolInfo()
        //    {
        //        symbol = 5,
        //        x5 = 4,
        //        x4 = 5,
        //        x3 = 6,
        //    },
        //    new PayTableSymbolInfo()
        //    {
        //        symbol = 6,
        //        x5 = 1,
        //        x4 = 4,
        //        x3 = 6,
        //    },
        //    new PayTableSymbolInfo()
        //    {
        //        symbol = 7,
        //        x5 = 5,
        //        x4 = 4,
        //        x3 = 4,
        //    },
        //    new PayTableSymbolInfo()
        //    {
        //        symbol = 8,
        //        x5 = 1,
        //        x4 = 5,
        //        x3 = 8,
        //    },
        //    new PayTableSymbolInfo()
        //    {
        //        symbol = 9,
        //        x5 = 4,
        //        x4 = 5,
        //        x3 = 2,
        //    },
        //    new PayTableSymbolInfo()
        //    {
        //        symbol = 10,
        //        x5 = 5,
        //        x4 = 5,
        //        x3 = 5,
        //    },
        //    new PayTableSymbolInfo()
        //    {
        //        symbol = 11,
        //        x5 = 5,
        //        x4 = 5,
        //        x3 = 5,
        //    },
        //};
        //public List<List<int>> payLines => new List<List<int>> 
        //{
        //    new List<int>{0,0,0,0,0},
        //    new List<int>{0,0,1,0,0},
        //    new List<int>{0,1,1,1,0},
        //    new List<int>{0,1,2,1,0},
        //    new List<int>{0,1,0,1,0},
        //    new List<int>{0,0,0,1,0},
        //    new List<int>{0,1,0,0,0},
        //    new List<int>{0,0,1,1,0},
        //    new List<int>{0,1,1,0,0},
        //    new List<int>{0,0,2,0,0},
        //    new List<int>{0,0,2,1,0},
        //    new List<int>{0,1,2,0,0},
        //    new List<int>{1,1,1,1,1},
        //    new List<int>{1,1,2,1,1},
        //    new List<int>{1,1,0,1,1},
        //    new List<int>{1,2,2,2,1},
        //    new List<int>{1,0,0,0,1},
        //    new List<int>{1,2,1,2,1},
        //    new List<int>{1,0,1,0,1},
        //    new List<int>{1,1,1,2,1},
        //    new List<int>{1,1,1,0,1},
        //    new List<int>{1,2,1,1,1},
        //    new List<int>{1,0,1,1,1},
        //    new List<int>{1,2,0,2,1},
        //    new List<int>{2,2,2,2,2},
        //    new List<int>{2,2,3,2,2},
        //    new List<int>{2,2,1,2,2},
        //    new List<int>{2,3,3,3,2},
        //    new List<int>{2,1,1,1,2},
        //    new List<int>{2,3,2,3,2},
        //    new List<int>{2,1,2,1,2},
        //    new List<int>{2,2,2,3,2},
        //    new List<int>{2,2,2,1,2},
        //    new List<int>{2,3,2,2,2},
        //    new List<int>{2,1,2,2,2},
        //    new List<int>{2,3,1,3,2},
        //    new List<int>{2,1,3,1,2},
        //    new List<int>{3,3,3,3,3},
        //    new List<int>{3,3,2,3,3},
        //    new List<int>{3,2,2,2,3},
        //    new List<int>{3,2,1,2,3},
        //    new List<int>{3,2,3,2,3},
        //    new List<int>{3,3,3,2,3},
        //    new List<int>{3,2,3,3,3},
        //    new List<int>{3,3,2,2,3},
        //    new List<int>{3,2,2,3,3},
        //    new List<int>{3,3,1,3,3},
        //    new List<int>{3,3,1,2,3},
        //    new List<int>{3,2,1,3,3}
        //};
        //public List<WinMultiple> winLevelMultiple => new List<WinMultiple> 
        //{
        //     new WinMultiple("BIG", 5),
        //     new WinMultiple("HUGE", 10),
        //     new WinMultiple("MASSIVE", 20),
        //};.

        #region 赔付线与赔付表

        private List<PayTableSymbolInfo> m_PayTableSymbolWin = new List<PayTableSymbolInfo>(){
            new PayTableSymbolInfo(){symbol = 0, x5 = 100, x4 = 20, x3 = 5, x2 = 2 },
            new PayTableSymbolInfo(){symbol = 1, x5 = 100, x4 = 20, x3 = 5, x2 = 0 },
            new PayTableSymbolInfo(){symbol = 2, x5 = 120, x4 = 40, x3 = 5, x2 = 0 },
            new PayTableSymbolInfo(){symbol = 3, x5 = 120, x4 = 40, x3 = 5, x2 = 0 },
            new PayTableSymbolInfo(){symbol = 4, x5 = 140, x4 = 60, x3 = 10, x2 = 0 },
            new PayTableSymbolInfo(){symbol = 5, x5 = 140, x4 = 60, x3 = 10, x2 = 0 },
            new PayTableSymbolInfo(){symbol = 6, x5 = 150, x4 = 100, x3 = 40, x2 = 10 },
            new PayTableSymbolInfo(){symbol = 7, x5 = 150, x4 = 100, x3 = 40, x2 = 10 },
            new PayTableSymbolInfo(){symbol = 8, x5 = 200, x4 = 150, x3 = 80, x2 = 20 },
            new PayTableSymbolInfo(){symbol = 9, x5 = 2500, x4 = 200, x3 = 80, x2 = 20 },
            new PayTableSymbolInfo(){symbol = 10, x5 = 2500, x4 = 200, x3 = 80, x2 = 20 },
            new PayTableSymbolInfo(){symbol = 11, x5 = 0, x4 = 0, x3 = 0, x2 = 0 },
            new PayTableSymbolInfo(){symbol = 12, x5 = 0, x4 = 0, x3 = 0, x2 = 0 },
            new PayTableSymbolInfo(){symbol = 13, x5 = 0, x4 = 0, x3 = 0, x2 = 0 },
            new PayTableSymbolInfo(){symbol = 14, x5 = 0, x4 = 0, x3 = 0, x2 = 0 },
        };

        public List<PayTableSymbolInfo> payTableSymbolWin
        {
            get => m_PayTableSymbolWin;
            set => m_PayTableSymbolWin = value;
        }

        public List<List<int>> payLines
        {
            get => m_payLines;
            set => m_payLines = value;
        }
        List<List<int>> m_payLines = new List<List<int>>()
        {
                new List<int> { 1,1,1,1,1 },
                new List<int> { 0,0,0,0,0 },
                new List<int> { 2,2,2,2,2 },
                new List<int> { 0,1,2,1,0 },
                new List<int> { 2,1,0,1,2 },
                new List<int> { 1,0,0,0,1 },
                new List<int> { 1,2,2,2,1 },
                new List<int> { 0,0,1,2,2 },
                new List<int> { 2,2,1,0,0 },
                new List<int> { 1,2,1,0,1 },
                new List<int> { 1,0,1,2,1 },
                new List<int> { 0,1,1,1,0 },
                new List<int> { 2,1,1,1,2 },
                new List<int> { 0,1,0,1,0 },
                new List<int> { 2,1,2,1,2 },
                new List<int> { 1,1,0,1,1 },
                new List<int> { 1,1,2,1,1 },
                new List<int> { 0,0,2,0,0 },
                new List<int> { 2,2,0,2,2 },
                new List<int> { 0,2,2,2,0 },
                new List<int> { 2,0,0,0,2 },
                new List<int> { 1,2,0,2,1 },
                new List<int> { 1,0,2,0,1 },
                new List<int> { 0,2,0,2,0 },
                new List<int> { 2,0,2,0,2 },
                new List<int> { 2,0,1,2,0 },
                new List<int> { 0,2,1,0,2 },
                new List<int> { 0,2,1,2,0 },
                new List<int> { 2,0,1,0,2 },
                new List<int> { 2,1,0,0,1 },
                new List<int> { 0,1,2,2,1 },
                new List<int> { 0,0,2,2,2 },
                new List<int> { 2,2,0,0,0 },
                new List<int> { 1,0,2,1,2 },
                new List<int> { 1,2,0,1,0 },
                new List<int> { 0,1,0,1,2 },
                new List<int> { 2,1,2,1,0 },
                new List<int> { 1,2,2,0,0 },
                new List<int> { 0,0,1,1,2 },
                new List<int> { 2,2,1,1,0 },
                new List<int> { 2,0,0,0,0 },
                new List<int> { 0,2,2,2,2 },
                new List<int> { 2,2,2,2,0 },
                new List<int> { 0,0,0,0,2 },
                new List<int> { 1,0,1,0,1 },
                new List<int> { 1,2,1,2,1 },
                new List<int> { 0,1,2,2,2 },
                new List<int> { 2,1,0,0,0 },
                new List<int> { 0,1,1,1,1 },
                new List<int> { 2,1,1,1,1 },
        };

        public List<WinMultiple> winLevelMultiple
        {
            get => _winMultipleList;
            set => _winMultipleList = value;
        }
        List<WinMultiple> _winMultipleList = new List<WinMultiple>()
        {
                new WinMultiple("BIG", 5),
                new WinMultiple("HUGE", 10),
                new WinMultiple("MASSIVE", 20),
        };

        public FreeGameConfig freeGameConfig
        {
            get => _freeGameConfig;

        }

        public FreeGameConfig _freeGameConfig = new FreeGameConfig()
        {
            IsUseCommonFreeTimes = false,                          //是否使用公共的免费次数框
            IsHasFreeGame = true,                                  //是否有免费奖
            FreeGameType = MakeFreeGameType.OnScatter,             //触发免费奖方式
            IsScatterInLine = false,                               //Scatter图标是否依赖中奖线
            Make2FreeGameCount = new int[] { 3, 4, 5 },            //触发免费奖所需数量(Scatter图标/充能)
            FreeGameTime = new int[] { 8, 15, 20 },                  //免费次数
        };

        public FreeGameConfig jackpotGameConfig
        {
            get => new FreeGameConfig()
            {
                IsUseCommonFreeTimes = false,                          //是否使用公共的免费次数框
                IsHasFreeGame = true,                                  //是否有免费奖
                FreeGameType = MakeFreeGameType.OnScatter,             //触发免费奖方式
                IsScatterInLine = false,                               //Scatter图标是否依赖中奖线
                Make2FreeGameCount = new int[] { 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 },            //触发免费奖所需数量(Scatter图标/充能)
                FreeGameTime = new int[] { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 },                  //免费次数
            };

        }

        public BonusGameConfig bonusGameconfig
        {
            get => new BonusGameConfig()
            {
                IsHasBonusGame = true,                                 //是否有大奖
                BonusGameType = MakeBonusGameType.OnBonus,            // 触发大奖方式
                IsBonusInLine = false,                                  //Bonus图标是否依赖中奖线
                Make2BonusGameCount = 3,                               //触发大奖所需数量(Bonus图标)
            };

        }

        #endregion
    }
}