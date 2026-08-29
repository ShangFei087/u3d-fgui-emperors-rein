using SlotMaker;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    /// <summary>免费局滚动中的 Wild 随机显示 X2/X3/X5 静态图；停轴后由 WildData 覆盖。</summary>
    public class Symbol3993 : Symbol01
    {
        private const int WildId = 10;
        private static readonly int[] FreeWildMuls = { 2, 3, 5 };

        public override void SetSymbolImage(int symbolNumber, bool needNativeSize = false)
        {
            base.SetSymbolImage(symbolNumber, needNativeSize);
            if (symbolNumber != WildId || imgBase == null)
                return;
            if (!ContentModel.Instance.isFreeSpin)
                return;

            int mul = FreeWildMuls[Random.Range(0, FreeWildMuls.Length)];
            imgBase.url = ContentModel.GetWildIconUrl(mul);
        }
    }
}
