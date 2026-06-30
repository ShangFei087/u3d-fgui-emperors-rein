using GameMaker;
using SlotMaker;
using System.Collections.Generic;

namespace MeiZhouHeiBao_3993
{
    public class CustomModel : MonoSingleton<CustomModel>, ICustomModel
    {
        public int row => 3;
        public int column => 5;
        public float symbolWidth => 145;
        public float symbolHeight => 133;
        public int symbolCount => symbolNumber.Count;
        public List<int> specialHitSymbols => new List<int>() { };
        public Dictionary<string, string> symbolAppearEffect => new Dictionary<string, string>() { };
        public Dictionary<string, string> symbolExpectationEffect => new Dictionary<string, string>() { };
        public float reelMaxOffsetY => symbolHeight * row;
        public string[] payTable => new[] { "ui://MeiZhouHeiBao/PayTable1", "ui://MeiZhouHeiBao/PayTable1", "ui://MeiZhouHeiBao/PayTable1", "ui://MeiZhouHeiBao/PayTable1", "ui://MeiZhouHeiBao/PayTable1" };
        public List<int> symbolNumber { get; }
        public Dictionary<string, string> symbolHitEffect { get; }

        public string borderEffect { get; }
        public Dictionary<string, string> symbolIcon { get; }
        public List<PayTableSymbolInfo> payTableSymbolWin { get; set; }
        public List<List<int>> payLines { get; set; }
        public List<WinMultiple> winLevelMultiple { get; set; }
    }
}