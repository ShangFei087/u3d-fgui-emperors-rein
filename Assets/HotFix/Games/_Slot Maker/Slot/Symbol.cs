using FairyGUI;
using UnityEngine;
using PusherEmperorsRein;

namespace  SlotMaker
{
    [System.Serializable]
    public class Symbol : SymbolBase
    {

        public override void SetSymbolImage(int symbolNumber, bool needNativeSize = false)
        {
            this.number = symbolNumber;
            imgBase.url = customModel.symbolIcon[$"{symbolNumber}"];
        }

        public override int GetSymbolNumber()
        {
            return number;
        }

        public override int GetSymbolIndex()
        {
            return customModel.symbolNumber[number]; // == return index;
        }

        /*
        private void OnClickSymbol()
        {
            DebugUtils.Log("i am here 123456");
        }*/


        /// <summary>  是否是特殊图标 /// </summary>
        public override bool IsSpecailHitSymbol()
        {
            return customModel.specialHitSymbols.Contains(number);
        }


        /// <summary>
        /// 添加边框特效
        /// </summary>
        /// <param name="borderEffect"></param>
        /// <returns></returns>
        public override GComponent AddBorderEffect(GComponent borderEffect)
        {

            GComponent gBorder = base.AddBorderEffect(borderEffect);

            //设置边框在底部
            gBorder.parent.SetChildIndex(gBorder,0);
            
            // 播放动画

            // if (IsSpecailHitSymbol())  {}

            return gBorder;
        }

        Transition transitionBigger, transitionTwinkle;

        public override void ShowBiggerEffect()
        {
            transitionBigger = goOwnerSymbol.GetTransition("animBigger");
            transitionBigger.Play();
        }
        public override void ShowTwinkleEffect()
        {
            transitionTwinkle = goOwnerSymbol.GetTransition("animTwinkle");
            transitionTwinkle.Play();
        }
        
        public override void StopSymbolEffectBiggerTwinkle()
        {
            if(transitionBigger != null)
                transitionBigger.Stop();
            transitionBigger = null;

            if (transitionTwinkle != null)
                transitionTwinkle.Stop();
            transitionTwinkle = null;
        }
    }
}
