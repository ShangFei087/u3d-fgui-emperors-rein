using FairyGUI;
using GameMaker;
using PusherEmperorsRein;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _spinWEMD = SlotMaker.SpinWinEffectSettingModel;

namespace CaiFuZhiMen_3999
{
    public class SlotMachineController3999 : SlotMachineBaseController
    {
        public void Init(GComponent gSlotCover, GComponent gPayLines, GComponent gReels, GComponent gExpectation,
            FguiPoolHelper fGuiPoolHelper, FguiGObjectPoolHelper gObjectPoolHelper)
        {
            base.Init(CustomModel.Instance, gSlotCover, gPayLines, gReels, fGuiPoolHelper, gObjectPoolHelper);
            goExpectation = gExpectation;

            this.column = CustomModel.Instance.column;
            this.row = CustomModel.Instance.row;

            Transform tfmReels = transform.Find("Reels");
            reels = new List<ReelBase>();
            for (int i = 0; i < this.column; i++)
            {
                Reel01 reel = tfmReels.GetChild(i).GetComponent<Reel01>();
                reel.reelIndex = i;
                reels.Add(reel);

                reel.Init(CustomModel.Instance, goReels.GetChildAt(i).asCom, gExpectation);
            }

            bufferTop = 2; // 滚轴上方有几个图标
        }

        private new IEnumerator ShowWinListBySetting(List<SymbolWin> winList, Action callback)
        {
            // 立马停止时，不播放赢分环节？
            if (isStopImmediately && _spinWEMD.Instance.isSkipAtStopImmediately)
                yield break;

            if (_spinWEMD.Instance.isTotalWin)
            {
                yield return ShowSymbolWinBySetting(GetTotalSymbolWin(winList), true, SpinWinEvent.TotalWinLine);
            }
            else
            {
                int idx = 0;
                while (idx < winList.Count)
                {
                    yield return ShowSymbolWinBySetting(winList[idx], true, SpinWinEvent.SingleWinLine, callback);
                    ++idx;
                    if (isStopImmediately && _spinWEMD.Instance.isSkipAtStopImmediately)
                        break;
                }
            }

            //关闭遮罩
            CloseSlotCover();

            //停止特效显示
            SkipWinLine(false);
        }

        public new IEnumerator ShowWinListAwayDuringIdle(List<SymbolWin> winList, Action callback = null)
        {
            while (winList.Count > 0) //while (idx < winList.Count)
            {
                yield return ShowWinListBySetting(winList, callback);
            }
        }

        private new IEnumerator ShowSymbolWinBySetting(SymbolWin symbolWin, bool isUseMySelfSymbolNumber,
            SpinWinEvent eventType, Action callback = null)
        {
            SkipWinLine(false);
            callback?.Invoke();
            
            if (isStopImmediately && _spinWEMD.Instance.isSkipAtStopImmediately)
                yield break;

            SetSlotCover(_spinWEMD.Instance.isShowCover);

            foreach (Cell cel in symbolWin.cells)
            {
                SymbolBase symbolBase = GetVisibleSymbolFromDeck(cel.column, cel.row);
                int symbolNumber = isUseMySelfSymbolNumber ? symbolBase.number : symbolWin.symbolNumber;
                string symbolName = CustomModel.Instance.symbolHitEffect[$"{symbolNumber}"]; // wild  or symbol;
                GComponent goSymbolHit = fguiPoolHelper.GetObject(TagPoolObject.SymbolHit, symbolName).asCom;
                symbolBase.AddSymbolEffect(goSymbolHit, isSymbolAnim);
                FguiSortingOrderManager.Instance.ChangeSortingOrder(symbolBase.goOwnerSymbol, goExpectation); //goPayLines

                if (_spinWEMD.Instance.isFrame)
                {
                    string borderEffect = CustomModel.Instance.borderEffect;
                    GComponent
                        goBorderEffect =
                            fguiPoolHelper.GetObject(TagPoolObject.SymbolBorder, borderEffect).asCom;
                    symbolBase.AddBorderEffect(goBorderEffect);
                }
            }

            if (_spinWEMD.Instance.isShowLine)
            {
                if (symbolWin is TotalSymbolWin)
                {
                    TotalSymbolWin totalSymbolWin = symbolWin as TotalSymbolWin;

                    foreach (int payLineNumber in totalSymbolWin.lineNumbers)
                    {
                        int lineIndex = GetPayLineIndex(payLineNumber);
                        if (lineIndex >= 0 && lineIndex < goPayLines.numChildren)
                        {
                            goPayLines.GetChildAt(lineIndex).visible = true;
                        }
                    }
                }
                else
                {
                    int lineIndex = GetPayLineIndex(symbolWin.lineNumber);
                    if (lineIndex >= 0
                        && lineIndex < goPayLines.numChildren)
                    {
                        goPayLines.GetChildAt(lineIndex).visible = true;
                    }
                }
            }

            if (eventType == SpinWinEvent.TotalWinLine)
            {
                EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_WIN_EVENT,
                    new EventData<SymbolWin>(SlotMachineEvent.TotalWinLine, symbolWin));
            }
            else if (eventType == SpinWinEvent.SingleWinLine)
            {
                EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_WIN_EVENT,
                    new EventData<SymbolWin>(SlotMachineEvent.SingleWinLine, symbolWin));
            }

            yield return SlotWaitForSeconds(_spinWEMD.Instance.timeS);
        }
    }
}