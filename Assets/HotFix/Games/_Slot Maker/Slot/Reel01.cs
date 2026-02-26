using FairyGUI;
using SlotMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reel01 : Reel
{
    //// 子类可以修改返回值
    protected override int deckUpStartIndex => 2; 
    protected override int deckUpEndIndex => customModel.row+2;
    protected override int deckDownStartIndex => customModel.row + 1;
    protected override int deckDownEndIndex => customModel.row + customModel.row;
    /// <summary>
    /// fgui动画用
    /// </summary>
    /// <param name="customModel"></param>
    /// <param name="gReel"></param>
    /// <param name="gExpectation"></param>
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
            Symbol01 symbol = new Symbol01();
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

    public override void SymbolAppearEffect()
    {
        for (int i = deckUpStartIndex; i <= deckUpEndIndex; i++)
        {

            Symbol01 symble = (Symbol01)symbolList[i];

            string symbleNumber = $"{symble.number}";

            bool isHashSymbolAppearNumber = false;
            foreach (KeyValuePair<string, string> kv in customModel.symbolAppearEffect)
            {
                if (kv.Key == symbleNumber)
                {
                    isHashSymbolAppearNumber = true;
                    break;
                }
            }

            if (isHashSymbolAppearNumber)
            {
                string symbolName = customModel.symbolAppearEffect[symbleNumber];
                GComponent anchorSymbolEffect = FguiGObjectPoolHelper.Instance.GetObject(symbolName).asCom;
                symble.AddSymbolEffect(anchorSymbolEffect);

                FguiSortingOrderManager.Instance.ChangeSortingOrder(symble.goOwnerSymbol, goExpectation);

                /*
                int rowIndex = i;
                // 设置层级
                FguiSortingOrderManager.Instance.ChangeSortingOrder(symble.goOwnerSymbol, goExpectation, null,null, 
                    (self) => rowIndex + deckUpStartIndex); 
                */
            }
        }
    }
}
