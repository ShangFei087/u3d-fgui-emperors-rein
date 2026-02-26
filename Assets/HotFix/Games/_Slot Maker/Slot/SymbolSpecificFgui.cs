using FairyGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using _spinWEMD = SlotMaker.SpinWinEffectSettingModel;
namespace SlotMaker
{
    public class SymbolSpecificFgui : Symbol
    {
        public override GComponent AddSymbolEffect(GComponent anchorSymbolEffect, bool isAmin = true)
        {
            anchorSymbolEffect.visible = true;
            GComponent goAnimator = goOwnerSymbol.GetChild("animator").asCom;
            goAnimator.AddChild(anchorSymbolEffect); //边长为1的点
            anchorSymbolEffect.xy = new Vector2(0, 0);
            anchorSymbolEffect.SetPivot(0.5f, 0.5f);
            // 是否隐藏原有图标
            if (_spinWEMD.Instance.isHideBaseIcon)
            {
                HideBaseSymbolIcon(true);
            }

            return anchorSymbolEffect;
            // 播放动画
        }
    }
}
