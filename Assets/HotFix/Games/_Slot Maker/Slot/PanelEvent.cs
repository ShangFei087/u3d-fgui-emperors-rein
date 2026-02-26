using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameMaker
{
    public class PanelEvent
    {
        public const string ON_PANEL_INPUT_EVENT = "ON_PANEL_INPUT_EVENT";
        /// <summary> spin 按钮按下 - value : bool (is long click)</summary>
        public const string SpinButtonClick = "SpinButtonClick"; //OnSpinButtonEvent
        /// <summary> spin BonusGame1按钮按下 - value : bool (is long click)</summary>
        public const string BonusGame1SpinButtonClick = "BonusGame1SpinButtonClick"; //OnSpinButtonEvent
        /// <summary> 钱箱按钮点击 </summary>
        public const string RedeemButtonClick = "RedeemButtonClick"; //OnSpinButtonEvent
        /// <summary> 连续玩局数  int -1 1 3 5 </summary>
        public const string TotalSpinsButtonClick = "TotalSpinsButtonClick"; //OnSpinButtonEvent




        public const string ON_PANEL_EVENT = "ON_PANEL_EVENT";
        /// <summary> 面板锚点变化事件(多语言！) </summary>
        public const string AnchorPanelChange = "AnchorPanelChange";
    }


    public class CreditEventData
    {
        public long nowCredit = -1;
        public long toCredit = -1;
        public bool isAnim = false;
    }
}

