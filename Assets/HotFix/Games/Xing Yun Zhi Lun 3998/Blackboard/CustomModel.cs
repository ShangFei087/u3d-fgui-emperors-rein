using GameMaker;
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


            //{"70", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X70.prefab"},
            //{"80", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X80.prefab"},
            //{"90", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X90.prefab"},
            //{"100", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X100.prefab"},
            //{"120", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X120.prefab"},
            {"140", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X140.prefab"},
            {"160", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X160.prefab"},
            {"180", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X180.prefab"},
            {"200", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X200.prefab"},
            {"220", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X220.prefab"},
            {"240", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X240.prefab"}
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
            //{"0", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/JackpotHit/JackpotHit0.prefab" },
            //{"1", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/JackpotHit/JackpotHit1.prefab" },
            //{"2", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/JackpotHit/JackpotHit2.prefab" },
            //{"3", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/JackpotHit/JackpotHit3.prefab" },
            //{"4", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/JackpotHit/JackpotHit4.prefab" },
            //{"5", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/JackpotHit/JackpotHit5.prefab" },
            //{"6", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/JackpotHit/JackpotHit6.prefab" },
            //{"7", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/JackpotHit/JackpotHit7.prefab" },
            //{"8", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/JackpotHit/JackpotHit8.prefab" },
            //{"9", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/JackpotHit/JackpotHit9.prefab" },
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
                {"8", "ui://XingYunZhiLun_3998/symbol_diamond" },
                {"9", "ui://XingYunZhiLun_3998/symbol_9" },
                {"10", "ui://XingYunZhiLun_3998/symbol_11" }
        };

        public readonly int[] lowWheelIndex = new int[] { 4, 0, 5, 1, 6, 2, 7, 3};
        public readonly int[] midWheelIndex = new int[] { 5, 0, 6, 1, 7, 2, 8, 3};
        public readonly int[] highWheelIndex = new int[] { 6, 0, 7, 1, 8, 2, 9, 3};

        public Dictionary<string, string> wheelSymbolIcon => new Dictionary<string, string>
        {
            {"0", "ui://XingYunZhiLun_3998/symbol_bouns"},
            {"1", "ui://XingYunZhiLun_3998/symbol_lipinghe"},
            {"2", "ui://XingYunZhiLun_3998/symbol_scatter"},
            {"3", "ui://XingYunZhiLun_3998/symbol_wild" },
            //{"4", "ui://XingYunZhiLun_3998/symbol_70x" },
            //{"5", "ui://XingYunZhiLun_3998/symbol_80x" },
            //{"6", "ui://XingYunZhiLun_3998/symbol_90x" },
            //{"7", "ui://XingYunZhiLun_3998/symbol_100x" },
            //{"8", "ui://XingYunZhiLun_3998/symbol_120x" },

            {"4", "ui://XingYunZhiLun_3998/symbol_140x" },
            {"5", "ui://XingYunZhiLun_3998/symbol_160x" },
            {"6", "ui://XingYunZhiLun_3998/symbol_180x" },
            {"7", "ui://XingYunZhiLun_3998/symbol_200x" },
            {"8", "ui://XingYunZhiLun_3998/symbol_220x" },
            {"9", "ui://XingYunZhiLun_3998/symbol_240x" },
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

            //{"8", "ui://XingYunZhiLun_3998/ListSymbol8"},
            //{"9", "ui://XingYunZhiLun_3998/ListSymbol9"},
        };


        /// <summary> 倍率中奖的预制体 </summary>
        public Dictionary<string, string> multipleSymbols => new Dictionary<string, string>
        {
            {"140", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X140.prefab"},
            {"160", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X160.prefab"},
            {"180", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X180.prefab"},
            {"200", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X200.prefab"},
            {"220", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X220.prefab"},
            {"240", "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/Symbols/MultipleHit/X240.prefab"}
        };

        public Dictionary<string, string> SlotBgURL => new Dictionary<string, string>
        {
            { "normalSlotBg", "ui://XingYunZhiLun_3998/slotNormal"},
            { "freeSlotBg", "ui://XingYunZhiLun_3998/slotFree" },
        };

        public readonly string[] jackpotResultBtnUrl ={
            "ui://XingYunZhiLun_3998/jp_btn_major",
            "ui://XingYunZhiLun_3998/jp_btn_minor",
            "ui://XingYunZhiLun_3998/jp_btn_mini"
        };

        //<summary> 不同等级下的轮盘图标 </summary>
        public Dictionary<string, string> wheelState => new Dictionary<string, string>
        {
            { "low", "ui://XingYunZhiLun_3998/LowWheel"},
            { "mid", "ui://XingYunZhiLun_3998/MidWheel"},
            { "high","ui://XingYunZhiLun_3998/HighWheel"},
        };

        #region 赔付线与赔付表
        private List<PayTableSymbolInfo> m_PayTableSymbolWin = new List<PayTableSymbolInfo>()
        {
            new PayTableSymbolInfo(){symbol = 0, x5 = 30, x4 = 15, x3 = 10, },
            new PayTableSymbolInfo(){symbol = 1, x5 = 30, x4 = 15, x3 = 10, },
            new PayTableSymbolInfo(){symbol = 2, x5 = 30, x4 = 15, x3 = 10, },
            new PayTableSymbolInfo(){symbol = 3, x5 = 30, x4 = 15, x3 = 10, },
            new PayTableSymbolInfo(){symbol = 4, x5 = 50, x4 = 30, x3 = 20, },
            new PayTableSymbolInfo(){symbol = 5, x5 = 80, x4 = 40, x3 = 30, },
            new PayTableSymbolInfo(){symbol = 6, x5 = 100, x4 = 50, x3 = 40, },
            new PayTableSymbolInfo(){symbol = 7, x5 = 120, x4 = 60, x3 = 50, },
            new PayTableSymbolInfo(){symbol = 8, x5 = 140, x4 = 70, x3 = 60, },
            new PayTableSymbolInfo(){symbol = 9, x5 = 0, x4 = 0, x3 = 0, },
            new PayTableSymbolInfo(){symbol = 10, x5 = 0, x4 = 0, x3 = 0, },
        };

        public List<PayTableSymbolInfo> payTableSymbolWin
        {
            get => m_PayTableSymbolWin;
            set => m_PayTableSymbolWin = value;
        }

        List<List<int>> m_payLines = new List<List<int>>()
        {
            new List<int> { 1, 1, 1, 1, 1 },
            new List<int> { 0, 0, 0, 0, 0 },
            new List<int> { 2, 2, 2, 2, 2 },
            new List<int> { 0, 1, 2, 1, 0 },
            new List<int> { 2, 1, 0, 1, 2 },
            new List<int> { 0, 0, 1, 0, 0 },
            new List<int> { 2, 2, 1, 2, 2 },
            new List<int> { 1, 2, 2, 2, 1 },
            new List<int> { 1, 0, 0, 0, 1 },
            new List<int> { 0, 1, 1, 1, 0 },
            new List<int> { 2, 1, 1, 1, 2 },
            new List<int> { 0, 1, 0, 1, 0 },
            new List<int> { 2, 1, 2, 1, 2 },
            new List<int> { 1, 0, 1, 0, 1 },
            new List<int> { 1, 2, 1, 2, 1 },
            new List<int> { 1, 1, 0, 1, 1 },
            new List<int> { 1, 1, 2, 1, 1 },
            new List<int> { 0, 2, 0, 2, 0 },
            new List<int> { 2, 0, 2, 0, 2 },
            new List<int> { 1, 0, 2, 0, 1 }
        };
        public List<List<int>> payLines
        {
            get => m_payLines;
            set => m_payLines = value;
        }


        List<WinMultiple> _winMultipleList = new List<WinMultiple>()
        {
            new WinMultiple("BIG", 5),
            new WinMultiple("HUGE", 10),
            new WinMultiple("MASSIVE", 20),
        };
        public List<WinMultiple> winLevelMultiple
        {
            get => _winMultipleList;
            set => _winMultipleList = value;
        }

        #endregion

        public FreeGameConfig freeGameConfig
        {
            get => _freeGameConfig;

        }

        public FreeGameConfig _freeGameConfig = new FreeGameConfig()
        {
            IsUseCommonFreeTimes = false,                          //是否使用公共的免费次数框
            IsHasFreeGame = true,                        
            //是否有免费奖
            FreeGameType = MakeFreeGameType.OnScatter,             //触发免费奖方式
            IsScatterInLine = false,                               //Scatter图标是否依赖中奖线
            Make2FreeGameCount = new int[] { 3, 4, 5 },            //触发免费奖所需数量(Scatter图标/充能)
            FreeGameTime = new int[] { 4, 5, 6 },                  //免费次数
        };

        public BonusGameConfig bonusGameconfig
        {
            get => _bonusGameconfig;

        }

        public BonusGameConfig _bonusGameconfig = new BonusGameConfig()
        {
            IsHasBonusGame = true,                                 //是否有大奖
            BonusGameType = MakeBonusGameType.OnBonus,            // 触发大奖方式
            IsBonusInLine = false,                                  //Bonus图标是否依赖中奖线
            Make2BonusGameCount = 3,                               //触发大奖所需数量(Bonus图标)
        };
    }
}
