using FairyGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SlotMaker
{
    public class ReelSpecificFgui : Reel
    {
        public override void Init(ICustomModel customModel, GComponent gReel, GComponent gExpectation)
        {
            if (goOwnerReel == gReel) return; // 避免同个对象重复初始化

            this.customModel = customModel;
            goExpectation = gExpectation;
            goOwnerReel = gReel;
            goSymbols = goOwnerReel.GetChild("symbols").asCom;
            symbolList = new List<SymbolBase>();
            for (int i = 0; i < goSymbols.numChildren; i++)
            {
                SymbolSpecificFgui symbol = new SymbolSpecificFgui();
                //##symbol.Init(goSymbols.GetChildAt(i).asCom);
                symbol.Init(customModel, goSymbols.GetChild($"symbol{i}").asCom);
                symbolList.Add(symbol);
            }

            for (int i = 0; i < symbolList.Count; i++)
            {
                symbolList[i].SetSymbolImage(customModel.symbolNumber[Random.Range(0, customModel.symbolCount)]);
            }

            goSymbols.y = 0;


            goOwnerReel.onRollOver.Clear();
            goOwnerReel.onRollOver.Add(OnSymbolPointerEnter);

            goOwnerReel.onRollOut.Clear();
            goOwnerReel.onRollOut.Add(OnSymbolPointerExit);

            isReelPointering = false;
        }

    }
}

