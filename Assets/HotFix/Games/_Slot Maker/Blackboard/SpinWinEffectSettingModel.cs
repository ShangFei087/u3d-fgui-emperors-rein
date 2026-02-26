using System.Collections.Generic;
using UnityEngine;


namespace SlotMaker
{

    public enum SpinWinEffect
    {
        /// <summary> 边框 </summary>
        Border,
        /// <summary> 显示线 </summary>
        Line,
        /// <summary> 图标变大 </summary>
        Bigger,
        /// <summary> 图标闪烁 </summary>
        Twinkle,
        /// <summary> 动画 </summary>
        Anim,
        /// <summary> 总线赢 </summary>
        TotalWinLine,
        /// <summary> 单线赢 </summary>
        SingleWinLine,
        /// <summary> 得分 </summary>
        Credit,
        /// <summary> 遮罩 </summary>
        Cover,
        /// <summary> 隐藏原图标 </summary>
        HideBaseSymbol,
        /// <summary> bonus中奖信息/// </summary>
        Text,


        /// <summary> 按下停止滚轮时，不播放结果 </summary>
        SkipAtStopImmediately

    }

    public class SpinWinEffectSettingModel : MonoWeakSelectSingleton<SpinWinEffectSettingModel>
    {
        /// <summary> 默认spin中奖效果 </summary>
        public const string SPIN_WIN_EFFECT_DEFAULT = "Spin Win Effect Setting Default";

        /// <summary>游戏空闲时，spin中奖效果 </summary>
        public const string SPIN_WIN_EFFECT_IDLE = "Spin Win Effect Setting Idle";

        /// <summary>滚轮立马停，spin中奖效果 </summary>
        public const string SPIN_WIN_EFFECT_STOP_IMMEDIATELY = "Spin Win Effect Setting Stop Immediately";

        /// <summary>滚轮自动滚，spin中奖效果 </summary>
        public const string SPIN_WIN_EFFECT_AUTO = "Spin Win Effect Setting Auto";

        /// <summary>Bonus中奖效果 </summary>
        public const string SPIN_WIN_EFFECT_BONUS= "Spin Win Effect Setting Bonus";

        /// <summary>滚轮免费游戏，spin中奖效果 </summary>
        public const string SPIN_WIN_EFFECT_FREE_SPIN = "Spin Win Effect Setting Free Spin";

        /// <summary>改变图标</summary>
        public const string SPIN_WIN_EFFECT_CHANGE_SYMBOL = "Spin Win Effect Change Symbol";
        
        /// <summary>免费游戏触发</summary>
        public const string SPIN_WIN_EFFECT_FREE_SPIN_TRIGGER = "Spin Win Effect Free Spin Trigger";

        public List<SpinWinEffect> winEffectSetting = new List<SpinWinEffect> {
            SpinWinEffect.Cover, SpinWinEffect.Credit, SpinWinEffect.Border,
            SpinWinEffect.SingleWinLine, SpinWinEffect.Bigger};


        public bool isFrame => winEffectSetting.Contains(SpinWinEffect.Border);

        public bool isText => winEffectSetting.Contains(SpinWinEffect.Text);

        public bool isBigger => winEffectSetting.Contains(SpinWinEffect.Bigger);

        public bool isTwinkle => winEffectSetting.Contains(SpinWinEffect.Twinkle);

        public bool isShowLine => winEffectSetting.Contains(SpinWinEffect.Line);

        public bool isHideBaseIcon => winEffectSetting.Contains(SpinWinEffect.HideBaseSymbol);

        public bool isShowCover => winEffectSetting.Contains(SpinWinEffect.Cover);

        public bool isSymbolAnim => winEffectSetting.Contains(SpinWinEffect.Anim);

        public bool isShowWinCredit => winEffectSetting.Contains(SpinWinEffect.Credit);

        public bool isTotalWin => winEffectSetting.Contains(SpinWinEffect.TotalWinLine);

        public bool isSingleWin => winEffectSetting.Contains(SpinWinEffect.SingleWinLine);

        public bool isSkipAtStopImmediately => winEffectSetting.Contains(SpinWinEffect.SkipAtStopImmediately);

        //public float minTimeS = 3f;//
        public float timeS = 0.8f;//空闲时【 空闲时: 3f  # 播放结果单线: 0.8f 】

    }



    /*
List<SpinWinEffect> weDefaultSpin = new List<SpinWinEffect> {SpinWinEffect.Cover, SpinWinEffect.Credit, SpinWinEffect.Frame, SpinWinEffect.SingleWin, SpinWinEffect.Bigger};
List<SpinWinEffect> weDefaultSpinStopImmediately = new List<SpinWinEffect> { SpinWinEffect.Cover, SpinWinEffect.Credit, SpinWinEffect.Frame, SpinWinEffect.SingleWin, SpinWinEffect.Bigger };
List<SpinWinEffect> weNormalSpin;
List<SpinWinEffect> weNormalSpinIdel;
List<SpinWinEffect> weNormalSpinStopImmediately;
List<SpinWinEffect> weNormalSpinAuto;
List<SpinWinEffect> weFreeSpin;
*/
}