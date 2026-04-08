using FairyGUI;
using GameMaker;
using SlotMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SlotZhuZaiJinBi1700
{

    public class CustomModel : MonoSingleton<CustomModel> ,ICustomModel
    {
        /// <summary> 图标宽 </summary>
        public float symbolWidth => 127;

        /// <summary> 图标高 </summary>
        public float symbolHeight => 127;

        /// <summary> 列 </summary>
        public int column => 5;

        /// <summary> 行 </summary>
        public int row => 3;


        public float reelMaxOffsetY
        {
            get => symbolHeight * row;
        }

        /// <summary> 说明页 </summary>
        public string[] payTable => new string[1]
        {
            "ui://SlotZhuZaiJinBi1700/payTable1",
        };


        /// <summary> 通过图标索引，获取图标真实编号 </summary>
        public List<int> symbolNumber => new List<int>() {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10};

        /// <summary> 所有图标个数 </summary>
        public int symbolCount => symbolNumber.Count;

        /// <summary> 资源根目录路径 </summary>
        //public string gameAssetsRootFolder = "Assets/GameRes/Games/PssOn00152 (1080x1920)";

        /// <summary> 预制体名称 - 图标中奖特效</summary>
        public Dictionary<string, string> symbolHitEffect => new Dictionary<string, string>
        {
           
            {"0", "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/Symbols/SymbolHit/SymbolHit1.prefab" },
            {"1", "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/Symbols/SymbolHit/SymbolHit2.prefab" },
            {"2", "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/Symbols/SymbolHit/SymbolHit3.prefab" },
            {"3", "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/Symbols/SymbolHit/SymbolHit4.prefab" },
            {"4", "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/Symbols/SymbolHit/SymbolHit5.prefab" },
            {"5", "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/Symbols/SymbolHit/SymbolHit6.prefab" },
            {"6", "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/Symbols/SymbolHit/SymbolHit7.prefab" },
            {"7", "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/Symbols/SymbolHit/SymbolHit8.prefab" },
            {"8", "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/Symbols/SymbolHit/SymbolHit9.prefab" },
            {"9", "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/Symbols/SymbolHit/SymbolHit0 Wild.prefab" },
            {"10", "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/Symbols/SymbolHit/SymbolHit10 FreeSpin.prefab" },  // free
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
            //{"10", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Symbols/SymbolAppear/SymbolAppear10 FreeSpin.prefab" },
            //{"11", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Symbols/SymbolAppear/SymbolAppear11 Bonus_ball.prefab" },
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
            {"10", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Effects/SmallGame/Art/Effects/Prefabs/ng_eff_sym10_Chest.prefab" },
            {"11", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Effects/SmallGame/Art/Effects/Prefabs/ng_eff_sym11_purse.prefab" },  // 球 bonus
            {"12", "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Symbols/SymbolHit/SymbolHit0 Wild.prefab" } // jackpot
        };

        /// <summary> 预制体名称 - 边框特效</summary>
        public string borderEffect => "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Symbols/Border/AnchorBorder.prefab";

        /// <summary> 图片 - 默认图标</summary>
        public Dictionary<string, string> symbolIcon => new Dictionary<string, string>
        {
              
                {"0", "ui://SlotZhuZaiJinBi1700/ng_sym_9" },
                {"1", "ui://SlotZhuZaiJinBi1700/ng_sym_10" },
                {"2", "ui://SlotZhuZaiJinBi1700/ng_sym_J"},
                {"3", "ui://SlotZhuZaiJinBi1700/ng_sym_Q" },
                {"4", "ui://SlotZhuZaiJinBi1700/ng_sym_K"},
                {"5", "ui://SlotZhuZaiJinBi1700/ng_sym_A" },
                {"6", "ui://SlotZhuZaiJinBi1700/ng_sym_card" },
                {"7", "ui://SlotZhuZaiJinBi1700/ng_sym_wallet" },
                {"8", "ui://SlotZhuZaiJinBi1700/ng_sym_safe" },
                {"9", "ui://SlotZhuZaiJinBi1700/ng_sym_ptycoon" },
                {"10", "ui://SlotZhuZaiJinBi1700/ng_sym_treasury" },        
        };

        #region 赔付线与赔付表
        public List<PayTableSymbolInfo> payTableSymbolWin
        {
            get => m_PayTableSymbolWin;
            set => m_PayTableSymbolWin = value;
        }
        public List<PayTableSymbolInfo> m_PayTableSymbolWin = new List<PayTableSymbolInfo>()
        {
            new PayTableSymbolInfo(){ symbol = 0,x3 = 40,x4 = 50,x5 = 250,},
            new PayTableSymbolInfo(){ symbol = 1,x3 = 30,x4 = 40,x5 = 125,},
            new PayTableSymbolInfo(){ symbol = 2,x3 = 25,x4 = 35,x5 = 100,},
            new PayTableSymbolInfo(){ symbol = 3,x3 = 20,x4 = 30,x5 = 75,},
            new PayTableSymbolInfo(){ symbol = 4,x3 = 15,x4 = 25,x5 = 60,},
            new PayTableSymbolInfo(){ symbol = 5,x3 = 12,x4 = 20,x5 = 45,},
            new PayTableSymbolInfo(){ symbol = 6,x3 = 9,x4 = 15,x5 = 35,},
            new PayTableSymbolInfo(){ symbol = 7,x3 = 6,x4 = 10,x5 = 30,},
            new PayTableSymbolInfo(){ symbol = 8,x3 = 3,x4 = 6,x5 = 25,},
            new PayTableSymbolInfo(){ symbol = 9,x3 = 0,x4 = 0,x5 = 0,}, //WILD
            new PayTableSymbolInfo(){ symbol = 10,x3 = 0,x4 = 0,x5 = 0,},//SCATTER
        };

        public List<List<int>> payLines
        {
            get => m_payLines;
            set => m_payLines = value;
        }
        List<List<int>> m_payLines = new List<List<int>>()
        {
            new List<int> { 1, 1, 1, 1, 1 },
            new List<int> { 0, 0, 0, 0, 0 },
            new List<int> { 2, 2, 2, 2, 2 },
            new List<int> { 0, 1, 2, 1, 0 },
            new List<int> { 2, 1, 0, 1, 2 },
            new List<int> { 0, 0, 1, 2, 2 },
            new List<int> { 2, 2, 1, 0, 0 },
            new List<int> { 1, 2, 2, 2, 1 },
            new List<int> { 1, 0, 0, 0, 1 },
            new List<int> { 0, 1, 1, 1, 0 },
            new List<int> { 2, 1, 1, 1, 2 },
            new List<int> { 0, 1, 0, 1, 0 },
            new List<int> { 2, 1, 2, 1, 2 },
            new List<int> { 1, 1, 0, 1, 1 },
            new List<int> { 1, 1, 2, 1, 1 }
        };

        public List<WinMultiple> winLevelMultiple
        {
            get => _winMultipleList;
            set => _winMultipleList = value;
        }
        List<WinMultiple> _winMultipleList = new List<WinMultiple>()
        {
            new WinMultiple("BIG", 15),
            new WinMultiple("HUGE", 30),
            new WinMultiple("MASSIVE", 50),
            new WinMultiple("LEGENDARY", 100)
        };

        #endregion

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
            FreeGameTime = new int[] { 3, 3, 3 },                  //免费次数
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