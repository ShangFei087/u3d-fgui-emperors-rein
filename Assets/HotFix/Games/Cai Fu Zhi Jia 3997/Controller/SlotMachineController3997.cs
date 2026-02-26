using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _spinWEMD = SlotMaker.SpinWinEffectSettingModel;
using _reelSetMD = SlotMaker.ReelSettingModel;

namespace CaiFuZhiJia_3997
{
    public enum SpinWinEvent
    {
        None,
        TotalWinLine,
        SingleWinLine,
    }

    public class SlotMachineController3997 : SlotMachineBaseController
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
            while (winList.Count > 0) //while (idx < winList.Count)
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
                //显示遮罩
                //goSlotCover?.SetActive(_spinWEBB.Instance.isShowCover);

                int idx = 0;
                while (idx < winList.Count)
                {
                    yield return ShowSymbolWinBySetting(winList[idx], true, SpinWinEvent.SingleWinLine);

                    ++idx;

                    // 立马停止时，不播放赢分环节？
                    if (isStopImmediately && _spinWEMD.Instance.isSkipAtStopImmediately)
                        break;
                }
            }

            //关闭遮罩
            CloseSlotCover();

            //停止特效显示
            SkipWinLine(false);
        }

        public IEnumerator ShowSymbolWinBySetting(SymbolWin symbolWin, bool isUseMySelfSymbolNumber,
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

        public new IEnumerator TurnReelsNormal( /*int symbolIndex*/ List<int> specialSymbols,
            string strDeckRowCol = "1,1,1,1,1#2,2,6,2,2#3,3,3,3,3",
            Action finishCallback = null)
        {
            //停止特效显示
            SkipWinLine(false);

            int[] deckColRow = SlotTool.GetDeckColRow(strDeckRowCol).ToArray();
            List<List<int>> colrowLsts = GetDeckColRow(deckColRow,
                this.column,
                this.row, specialSymbols);

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

        List<int> slowCols = new List<int>();

        public List<List<int>> GetDeckColRow(int[] deckColRow, int colCount, int rowCount, /*int symbolIndex*/
            List<int> specialSymbols)// 修改参数，传入特殊图标数组
        {
            if (ContentModel.Instance.isReelsSlowMotion) slowCols.Clear();

            List<List<int>> colrowLsts = new List<List<int>>();
            for (int col = 0; col < colCount; col++)
            {
                List<int> colLst = new List<int>();
                for (int row = 0; row < rowCount; row++)
                {
                    int syb = deckColRow[col * rowCount + row];
                    if (ContentModel.Instance.isReelsSlowMotion && syb == specialSymbols[0] &&
                        !ContentModel.Instance.IsBonusTrigger)// 新增判断，是否使彩金游戏
                    {
                        slowCols.Add(col);
                    }
                    else if (ContentModel.Instance.isReelsSlowMotion && syb == specialSymbols[1] &&
                             ContentModel.Instance.IsBonusTrigger)
                    {
                        slowCols.Add(col);
                    }

                    colLst.Add(syb);
                }

                colrowLsts.Add(colLst);
            }

            return colrowLsts;
        }


        //滚轮滚动接口
        protected new IEnumerator StartTurnReels()
        {
            int reelsCount = this.column;

            bool isNext = false;

            for (int reelIdx = 0; reelIdx < this.column; reelIdx++)
            {
                if (_reelSetMD.Instance.GetTimeTurnStartDelay(reelIdx) > 0)
                {
                    yield return new WaitForSeconds(_reelSetMD.Instance.GetTimeTurnStartDelay(reelIdx));
                }

                int _reelIdx = reelIdx;
                int extraReelTimes = 0;
                bool isTrriger = false;

                if (ContentModel.Instance.isReelsSlowMotion && slowCols.Count > 1 && reelIdx >= slowCols[1])
                {
                    extraReelTimes = 15;
                    isTrriger = true;
                }

                reels[reelIdx].StartTurn(
                    _reelSetMD.Instance.GetNumReelTurn(reelIdx) +
                    reelIdx * _reelSetMD.Instance.GetNumReelTurnGap(reelIdx) +
                    extraReelTimes * (reelIdx - (slowCols.Count < 2 ? reelIdx : slowCols[1])),
                    () =>
                    {
                        if (isTrriger)
                        {
                            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT,
                                new EventData<int>(SlotMachineEvent.PrepareStoppedReel, _reelIdx + 1));
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

        #endregion
    }
}