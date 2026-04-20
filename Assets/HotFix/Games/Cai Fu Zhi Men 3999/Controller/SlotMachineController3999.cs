using FairyGUI;
using GameMaker;
using PusherEmperorsRein;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _spinWEMD = SlotMaker.SpinWinEffectSettingModel;
using _reelSetMD = SlotMaker.ReelSettingModel;

namespace CaiFuZhiMen_3999
{
    public class SlotMachineController3999 : SlotMachineBaseController
    {
        #region 初始化面板

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

        #endregion

        #region 解决Symbol播放变大特效问题

        public new void ShowSymbolWinDeck(SymbolWin symbolWin, bool isUseMySelfSymbolNumber, Action callback = null)
        {
            //停止特效显示
            SkipWinLine(false);
            callback?.Invoke();
            //显示遮罩
            SetSlotCover(_spinWEMD.Instance.isShowCover);

            foreach (Cell cel in symbolWin.cells)
            {
                SymbolBase symble = GetVisibleSymbolFromDeck(cel.column, cel.row);

                int symbolNumber = isUseMySelfSymbolNumber ? symble.number : symbolWin.symbolNumber;

                // 增加wild判断 
                if (symbolNumber == 9)
                {
                    if (ContentModel.Instance.smallWildList[cel.column].activeSelf ||
                        ContentModel.Instance.bigWildList[cel.column].activeSelf)
                        continue;
                }

                string symbolName = CustomModel.Instance.symbolHitEffect[$"{symbolNumber}"]; // wild  or symbol;

                // 图标动画
                GComponent goSymbolHit = fguiPoolHelper.GetObject(TagPoolObject.SymbolHit, symbolName).asCom;
                symble.AddSymbolEffect(goSymbolHit, isSymbolAnim);


                // 设置层级
                FguiSortingOrderManager.Instance.ChangeSortingOrder(symble.goOwnerSymbol, goExpectation); //goPayLines


                // 边框
                if (_spinWEMD.Instance.isFrame)
                {
                    string borderEffect = CustomModel.Instance.borderEffect;
                    GComponent
                        goBorderEffect = //FguiPoolManager.Instance.GetObject(TagPoolObject.SymbolBorder, borderEffect).asCom;
                            fguiPoolHelper.GetObject(TagPoolObject.SymbolBorder, borderEffect).asCom;
                    symble.AddBorderEffect(goBorderEffect);
                }

                // // 整体变大特效
                // if (_spinWEMD.Instance.isTwinkle)
                //     symble.ShowTwinkleEffect();
                // else if (_spinWEMD.Instance.isBigger)
                //     symble.ShowBiggerEffect();
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
        }

        #endregion

        #region 重写Idle状态方法

        public new IEnumerator ShowWinListAwayDuringIdle(List<SymbolWin> winList,
            Action callback = null)
        {
            while (winList.Count > 0) //while (idx < winList.Count)
            {
                yield return ShowWinListBySetting(winList, callback);
            }
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
                if (symbolNumber == 9)
                {
                    if (ContentModel.Instance.smallWildList[cel.column].activeSelf ||
                        ContentModel.Instance.bigWildList[cel.column].activeSelf)
                        continue;
                }

                string symbolName = CustomModel.Instance.symbolHitEffect[$"{symbolNumber}"]; // wild  or symbol;
                GComponent goSymbolHit = fguiPoolHelper.GetObject(TagPoolObject.SymbolHit, symbolName).asCom;
                symbolBase.AddSymbolEffect(goSymbolHit, isSymbolAnim);
                FguiSortingOrderManager.Instance.ChangeSortingOrder(symbolBase.goOwnerSymbol,
                    goExpectation); //goPayLines

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

        public override IEnumerator ShowSymbolWinBySetting(SymbolWin symbolWin, bool isUseMySelfSymbolNumber,
            SpinWinEvent eventType)
        {
            SkipWinLine(false);

            if (isStopImmediately && _spinWEMD.Instance.isSkipAtStopImmediately)
                yield break;

            SetSlotCover(_spinWEMD.Instance.isShowCover);

            foreach (Cell cel in symbolWin.cells)
            {
                SymbolBase symbolBase = GetVisibleSymbolFromDeck(cel.column, cel.row);
                int symbolNumber = isUseMySelfSymbolNumber ? symbolBase.number : symbolWin.symbolNumber;
                if (symbolNumber == 9)
                {
                    if (ContentModel.Instance.smallWildList[cel.column].activeSelf ||
                        ContentModel.Instance.bigWildList[cel.column].activeSelf)
                        continue;
                }

                string symbolName = CustomModel.Instance.symbolHitEffect[$"{symbolNumber}"]; // wild  or symbol;
                GComponent goSymbolHit = fguiPoolHelper.GetObject(TagPoolObject.SymbolHit, symbolName).asCom;
                symbolBase.AddSymbolEffect(goSymbolHit, isSymbolAnim);
                FguiSortingOrderManager.Instance.ChangeSortingOrder(symbolBase.goOwnerSymbol,
                    goExpectation); //goPayLines

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

        #endregion

        #region 重写加速框方法

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

        private readonly List<int> _slowCols = new List<int>();

        private List<List<int>> GetDeckColRow(int[] deckColRow, int colCount, int rowCount, /*int symbolIndex*/
            List<int> specialSymbols) // 修改参数，传入特殊图标数组
        {
            if (ContentModel.Instance.isReelsSlowMotion) _slowCols.Clear();

            List<List<int>> colrowLsts = new List<List<int>>();
            for (int col = 0; col < colCount; col++)
            {
                List<int> colLst = new List<int>();
                for (int row = 0; row < rowCount; row++)
                {
                    int syb = deckColRow[col * rowCount + row];
                    if (ContentModel.Instance.isReelsSlowMotion && syb == specialSymbols[0] &&
                        !ContentModel.Instance.IsBonusTrigger) // 新增判断，是否使彩金游戏
                    {
                        _slowCols.Add(col);
                    }
                    else if (ContentModel.Instance.isReelsSlowMotion && syb == specialSymbols[1] &&
                             ContentModel.Instance.IsBonusTrigger)
                    {
                        _slowCols.Add(col);
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

            for (int reelIdx = 0; reelIdx < this.column; reelIdx++)
            {
                // 每次旋转都会至少转一圈，取消等待时间就可以实现急停
                // if (_reelSetMD.Instance.GetTimeTurnStartDelay(reelIdx) > 0)
                // {
                //     yield return new WaitForSeconds(_reelSetMD.Instance.GetTimeTurnStartDelay(reelIdx));
                // }
                
                int _reelIdx = reelIdx;
                int extraReelTimes = 0;
                bool isTrriger = false;

                if (ContentModel.Instance.isReelsSlowMotion && _slowCols.Count > 1 && reelIdx >= _slowCols[1])
                {
                    extraReelTimes = 15;
                    isTrriger = true;
                }

                reels[reelIdx].StartTurn(
                    _reelSetMD.Instance.GetNumReelTurn(reelIdx) +
                    reelIdx * _reelSetMD.Instance.GetNumReelTurnGap(reelIdx) +
                    extraReelTimes * (reelIdx - (_slowCols.Count < 2 ? reelIdx : _slowCols[1])),
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
                reel.SetReelState();
            }

            EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_SLOT_EVENT,
                new EventData(SlotMachineEvent.StoppedSlotMachine));
        }

        #endregion
    }
}