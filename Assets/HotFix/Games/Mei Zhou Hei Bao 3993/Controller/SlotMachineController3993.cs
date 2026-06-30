using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _spinWEMD = SlotMaker.SpinWinEffectSettingModel;
using _reelSetMD = SlotMaker.ReelSettingModel;

namespace MeiZhouHeiBao_3993
{
    public enum SpinWinEvent
    {
        None,
        TotalWinLine,
        SingleWinLine,
    }

    public class SlotMachineController3993 : SlotMachineBaseController
    {
        /// <summary>
        /// 初始化UI对象池
        /// </summary>
        /// <param name="gSlotCover"></param>
        /// <param name="gPayLines"></param>
        /// <param name="gReels"></param>
        /// <param name="gExpectation"></param>
        /// <param name="fGuiPoolHelper"></param>
        /// <param name="gObjectPoolHelper"></param>
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

        public new IEnumerator ShowWinListAwayDuringIdle(List<SymbolWin> winList)
        {
            isIdleEffect = true;
            while (winList.Count > 0 && isIdleEffect) //while (idx < winList.Count)
            {
                yield return ShowWinListBySetting(winList);
            }
        }

        public override IEnumerator ShowWinListBySetting(List<SymbolWin> winList)
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
                    if (!isIdleEffect) break;

                    yield return ShowSymbolWinBySetting(winList[idx], true, SpinWinEvent.SingleWinLine);

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

        private IEnumerator ShowSymbolWinBySetting(SymbolWin symbolWin, bool isUseMySelfSymbolNumber,
            SpinWinEvent eventType)
        {
            //停止特效显示
            SkipWinLine(false);

            // 立马停止时，不播放赢分环节？
            if (isStopImmediately && _spinWEMD.Instance.isSkipAtStopImmediately)
                yield break;

            //显示遮罩
            SetSlotCover(_spinWEMD.Instance.isShowCover);

            foreach (Cell cel in symbolWin.cells)
            {
                SymbolBase symbol = GetVisibleSymbolFromDeck(cel.column, cel.row);

                int symbolNumber = isUseMySelfSymbolNumber ? symbol.number : symbolWin.symbolNumber;

                string symbolName = CustomModel.Instance.symbolHitEffect[$"{symbolNumber}"];

                // 图标动画  
                GComponent goSymbolHit = fguiPoolHelper.GetObject(TagPoolObject.SymbolHit, symbolName).asCom;
                symbol.AddSymbolEffect(goSymbolHit, isSymbolAnim);

                // 设置层级
                FguiSortingOrderManager.Instance.ChangeSortingOrder(symbol.goOwnerSymbol, goExpectation);

                // 边框
                if (_spinWEMD.Instance.isFrame)
                {
                    string borderEffect = CustomModel.Instance.borderEffect;
                    GComponent
                        goBorderEffect =
                            fguiPoolHelper.GetObject(TagPoolObject.SymbolBorder, borderEffect).asCom;
                    symbol.AddBorderEffect(goBorderEffect);
                }
            }


            // 是否显示线
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

            // 事件
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

        #region 新增滚轮加速方法

        public new IEnumerator TurnReelsNormal(string strDeckRowCol = "1,1,1,1,1#2,2,6,2,2#3,3,3,3,3", Action finishCallback = null)
        {
            //停止特效显示
            SkipWinLine(false);

            int[] deckColRow = SlotTool.GetDeckColRow(strDeckRowCol).ToArray();
            List<List<int>> colrowLsts = GetDeckColRow(deckColRow, column, row);

            List<int>[] colrow = colrowLsts.ToArray();

            //这个还要判断特殊图标 如果有还需要改变滚轮滚的次数 还有特殊表现效果
            //模拟图标
            for (int i = 0; i < this.column; i++)
            {
                reels[i].SetResult(colrow[i]);
            }

            yield return StartTurnReels();

            finishCallback?.Invoke();
        }

        private readonly List<int> freeIconCols = new List<int>();
        private readonly List<int> jackpotIconCols = new List<int>();

        private List<List<int>> GetDeckColRow(int[] deckColRow, int colCount, int rowCount) // 修改参数，传入特殊图标数组
        {
            if (freeIconCols.Count > 0) freeIconCols.Clear();
            if (jackpotIconCols.Count > 0) jackpotIconCols.Clear();

            List<List<int>> colrowLsts = new List<List<int>>();
            for (int col = 0; col < colCount; col++)
            {
                List<int> colLst = new List<int>();
                for (int row = 0; row < rowCount; row++)
                {
                    int syb = deckColRow[col * rowCount + row];
                    if (syb == 10)
                    {
                        freeIconCols.Add(col);
                    }
                    else if (syb == 11)
                    {
                        jackpotIconCols.Add(col);
                    }

                    colLst.Add(syb);
                }

                colrowLsts.Add(colLst);
            }

            return colrowLsts;
        }
        
        //滚轮滚动接口
        private new IEnumerator StartTurnReels()
        {
            int reelsCount = this.column;
            bool isNext = false;
            bool haveSlotTip = false;
            ContentModel.Instance.isFreeSlotTip = false;

            for (int reelIdx = 0; reelIdx < this.column; reelIdx++)
            {
                int index = reelIdx;
                int extraReelTimes = 0;
                bool isTrigger = false;
                int extraReelTimesReel = 0;

                if ((freeIconCols.Count > 1 && reelIdx >= freeIconCols[1]) || (jackpotIconCols.Count > 1 && reelIdx >= jackpotIconCols[1])) //ContentModel.Instance.isReelsSlowMotion && 
                {
                    extraReelTimes = 15;
                    isTrigger = true;
                    if (!haveSlotTip && freeIconCols.Count > 1 && reelIdx >= freeIconCols[1])
                    {
                        ContentModel.Instance.isFreeSlotTip = true;
                    }

                    haveSlotTip = true;

                    if (freeIconCols.Count > 1)
                    {
                        extraReelTimesReel = reelIdx - freeIconCols[1];
                    }
                    else
                    {
                        extraReelTimesReel = reelIdx - jackpotIconCols[1];
                    }
                }

                reels[reelIdx].StartTurn(
                    _reelSetMD.Instance.GetNumReelTurn(reelIdx) +
                    reelIdx * _reelSetMD.Instance.GetNumReelTurnGap(reelIdx) + extraReelTimes * extraReelTimesReel,
                    () =>
                    {
                        EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT,
                            new EventData<int>(SlotMachineEvent.ReelColumnStopSound, index));
                        ComputeScatterBonusColumnStopFlags(reels[index], index, out bool scatterCol, out bool bonusCol);
                        EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT,
                            new EventData<ScatterBonusColumnStopPayload>(SlotMachineEvent.ScatterBonusColumnStopSound,
                                new ScatterBonusColumnStopPayload { column0Based = index, hasScatter = scatterCol, hasBonus = bonusCol, }));
                        if (isTrigger)
                        {
                            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT,
                                new EventData<int>(SlotMachineEvent.PrepareStoppedReel, index + 1));
                        }

                        if (--reelsCount <= 0)
                        {
                            isNext = true;
                        }
                    }
                );
            }

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            foreach (ReelBase reel in reels)
            {
                reel.SetReelState(ReelState.Idle);
            }

            EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_SLOT_EVENT,
                new EventData(SlotMachineEvent.StoppedSlotMachine));
        }

        /// <summary>
        /// 已滚动的滚轮立马停止、未滚动的滚轮滚动一次
        /// </summary>
        /// <param name="finishCallback"></param>
        /// <returns></returns>
        public new IEnumerator ReelsToStopOrTurnOnce(Action finishCallback)
        {
            int reelsCount = this.column;

            bool isNext = false;

            for (int reelIdx = 0; reelIdx < this.column; reelIdx++)
            {
                if (reels[reelIdx].state == ReelState.EndStop)
                {
                    reelsCount--;
                    continue;
                }

                if (reels[reelIdx].state == ReelState.Idle)
                {
                    if (_reelSetMD.Instance.GetTimeTurnStartDelay(reelIdx) > 0)
                    {
                        yield return new WaitForSeconds(_reelSetMD.Instance.GetTimeTurnStartDelay(reelIdx));
                    }
                }

                int _reelIdx = reelIdx;

                reels[reelIdx].ReelToStopOrTurnOnce(
                    () =>
                    {
                        EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT,
                            new EventData<int>(SlotMachineEvent.ReelColumnStopSound, _reelIdx));
                        ComputeScatterBonusColumnStopFlags(reels[_reelIdx], _reelIdx, out bool scatterCol2, out bool bonusCol2);
                        EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT,
                            new EventData<ScatterBonusColumnStopPayload>(SlotMachineEvent.ScatterBonusColumnStopSound,
                                new ScatterBonusColumnStopPayload { column0Based = _reelIdx, hasScatter = scatterCol2, hasBonus = bonusCol2, }));
                        EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT,
                            new EventData<int>(SlotMachineEvent.PrepareStoppedReel, _reelIdx));

                        if (isSymbolAppearEffectWhenReelStop)
                            ShowReelSymbolAppearEffect(_reelIdx);

                        if (--reelsCount <= 0)
                        {
                            isNext = true;
                        }
                    }
                );
            }

            yield return new WaitUntil(() => isNext == true);
            isNext = false;


            foreach (ReelBase reel in reels)
            {
                reel.SetReelState(ReelState.Idle);
            }


            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_SLOT_EVENT,
                new EventData(SlotMachineEvent.StoppedSlotMachine));

            finishCallback?.Invoke();
        }

        /// <summary> 扫描单列可视区是否含 Scatter/Bonus（symbolNumber[10]/[11]），供 ScatterBonusColumnStopSound 载荷。 </summary>
        private void ComputeScatterBonusColumnStopFlags(ReelBase reel, int column0Based, out bool hasScatter, out bool hasBonus)
        {
            hasScatter = false;
            hasBonus = false;
            if (reel?.symbolList == null)
                return;

            int row = CustomModel.Instance.row;
            if (column0Based < 0 || column0Based >= CustomModel.Instance.column)
                return;

            int scatterId = CustomModel.Instance.symbolNumber[10];
            int bonusId = CustomModel.Instance.symbolNumber[11];
            for (int i = 2; i < 2 + row; i++)
            {
                if (i >= reel.symbolList.Count)
                    break;
                int n = reel.symbolList[i].number;
                if (n == scatterId)
                    hasScatter = true;
                if (n == bonusId)
                    hasBonus = true;
            }
        }

        #endregion

        #region 解决Symbol播放变大特效问题

        public new void ShowSymbolWinDeck(SymbolWin symbolWin, bool isUseMySelfSymbolNumber, Action callback = null)
        {
            SkipWinLine(false);
            callback?.Invoke();
            SetSlotCover(_spinWEMD.Instance.isShowCover);

            foreach (Cell cel in symbolWin.cells)
            {
                SymbolBase symble = GetVisibleSymbolFromDeck(cel.column, cel.row);

                int symbolNumber = isUseMySelfSymbolNumber ? symble.number : symbolWin.symbolNumber;
                string symbolName = CustomModel.Instance.symbolHitEffect[$"{symbolNumber}"];
                GComponent goSymbolHit = fguiPoolHelper.GetObject(TagPoolObject.SymbolHit, symbolName).asCom;
                symble.AddSymbolEffect(goSymbolHit, isSymbolAnim);
                FguiSortingOrderManager.Instance.ChangeSortingOrder(symble.goOwnerSymbol, goExpectation);
                if (_spinWEMD.Instance.isFrame)
                {
                    string borderEffect = CustomModel.Instance.borderEffect;
                    GComponent
                        goBorderEffect = fguiPoolHelper.GetObject(TagPoolObject.SymbolBorder, borderEffect).asCom;
                    symble.AddBorderEffect(goBorderEffect);
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
        }

        #endregion
    }
}