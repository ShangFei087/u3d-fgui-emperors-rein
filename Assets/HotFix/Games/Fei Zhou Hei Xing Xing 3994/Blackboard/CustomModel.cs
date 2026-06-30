using GameMaker;
using SlotMaker;
using System.Collections.Generic;

namespace FeiZhouHeiXingXing_3994
{
    public class CustomModel : MonoSingleton<CustomModel>, ICustomModel
    {
        public float symbolWidth { get; }
        public float symbolHeight { get; }
        public int column { get; }
        public int row { get; }
        public float reelMaxOffsetY { get; }
        public string[] payTable { get; }
        public List<int> symbolNumber { get; }
        public int symbolCount { get; }
        public Dictionary<string, string> symbolHitEffect { get; }
        public List<int> specialHitSymbols { get; }
        public Dictionary<string, string> symbolAppearEffect { get; }
        public Dictionary<string, string> symbolExpectationEffect { get; }
        public string borderEffect { get; }
        public Dictionary<string, string> symbolIcon { get; }
        public List<PayTableSymbolInfo> payTableSymbolWin { get; set; }
        public List<List<int>> payLines { get; set; }
        public List<WinMultiple> winLevelMultiple { get; set; }
    }
}
