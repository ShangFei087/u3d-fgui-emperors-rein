using FairyGUI;
using GameMaker;
using PusherEmperorsRein;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using _reelSetMD = SlotMaker.ReelSettingModel;
using _spinWEMD = SlotMaker.SpinWinEffectSettingModel;

namespace XingYunZhiLun_3998
{
    public class SlotMachineController3998 : SlotMachineBaseController
    {
        public void Init(GComponent gSlotCover, GComponent gPayLines, GComponent gReels, GComponent gExpectation, FguiPoolHelper fguiPoolHelper, FguiGObjectPoolHelper gObjectPoolHelper)
        {
            base.Init(CustomModel.Instance, gSlotCover, gPayLines, gReels, fguiPoolHelper, gObjectPoolHelper);
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

            bufferTop = 2;
            //gPayLines.visible = false;

        }



        #region 开奖动画

        public override IEnumerator ShowSymbolWinBySetting(SymbolWin symbolWin, bool isUseMySelfSymbolNumber, SpinWinEvent eventType)
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

                Symbol01 symble = (Symbol01)GetVisibleSymbolFromDeck(cel.column, cel.row);

                int symbolNumber = isUseMySelfSymbolNumber ? symble.number : symbolWin.symbolNumber;

                string symbolName = CustomModel.Instance.symbolHitEffect[$"{symbolNumber}"];  // wild  or symbol;

                // 图标动画  
                GComponent goSymbolHit = fguiPoolHelper.GetObject(TagPoolObject.SymbolHit, symbolName).asCom;
                symble.AddSymbolEffect(goSymbolHit, isSymbolAnim);



                // 设置层级
                FguiSortingOrderManager.Instance.ChangeSortingOrder(symble.goOwnerSymbol, goExpectation); //goPayLines


                // 边框
                if (_spinWEMD.Instance.isFrame)
                {
                    string borderEffect = CustomModel.Instance.borderEffect;
                    GComponent goBorderEffect = //FguiPoolManager.Instance.GetObject(TagPoolObject.SymbolBorder, borderEffect).asCom;
                        fguiPoolHelper.GetObject(TagPoolObject.SymbolBorder, borderEffect).asCom;
                    symble.AddBorderEffect(goBorderEffect);
                }

                // 整体变大特效
                //if (_spinWEMD.Instance.isTwinkle)
                //    symble.ShowTwinkleEffect();
                //else if (_spinWEMD.Instance.isBigger)
                //    symble.ShowBiggerEffect();

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

            yield return SlotWaitForSeconds(0.5f);
        }



        public override void ShowSymbolWinDeck(SymbolWin symbolWin, bool isUseMySelfSymbolNumber)
        {
            //停止特效显示
            SkipWinLine(false);

            //显示遮罩
            SetSlotCover(_spinWEMD.Instance.isShowCover);

            foreach (Cell cel in symbolWin.cells)
            {
                SymbolBase symble = GetVisibleSymbolFromDeck(cel.column, cel.row);

                int symbolNumber = isUseMySelfSymbolNumber ? symble.number : symbolWin.symbolNumber;

                string symbolName = CustomModel.Instance.symbolHitEffect[$"{symbolNumber}"];  // wild  or symbol;

                // 图标动画
                GComponent goSymbolHit = fguiPoolHelper.GetObject(TagPoolObject.SymbolHit, symbolName).asCom;
                symble.AddSymbolEffect(goSymbolHit, isSymbolAnim);
                // 设置层级
                FguiSortingOrderManager.Instance.ChangeSortingOrder(symble.goOwnerSymbol, goExpectation); //goPayLines


                // 边框
                if (_spinWEMD.Instance.isFrame)
                {
                    string borderEffect = CustomModel.Instance.borderEffect;
                    GComponent goBorderEffect = //FguiPoolManager.Instance.GetObject(TagPoolObject.SymbolBorder, borderEffect).asCom;
                        fguiPoolHelper.GetObject(TagPoolObject.SymbolBorder, borderEffect).asCom;
                    symble.AddBorderEffect(goBorderEffect);
                }

                // 整体变大特效
                //if (_spinWEMD.Instance.isTwinkle)
                //    symble.ShowTwinkleEffect();
                //else if (_spinWEMD.Instance.isBigger)
                //    symble.ShowBiggerEffect();
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

        public override void ShowSymbolEffect(TagPoolObject tp, List<SymbolBase> symbols, bool isAmin, int symbolNumber, bool isUseMySelfSymbolNumber)
        {
            CloseSlotCover();
            foreach (SymbolBase symbol in symbols)
            {
                GComponent goSymbol = symbol.goOwnerSymbol;

                int symNumber = isUseMySelfSymbolNumber ? symbol.number : symbolNumber;

                if (tp == TagPoolObject.SymbolHit)
                {
                    //显示遮罩
                    SetSlotCover(_spinWEMD.Instance.isShowCover);

                    string symbolName = CustomModel.Instance.symbolHitEffect[$"{symNumber}"];

                    // 图标动画
                    GComponent goSymbolHit = fguiPoolHelper.GetObject(TagPoolObject.SymbolHit, symbolName).asCom;
                    symbol.AddSymbolEffect(goSymbolHit, isAmin);

                    // 设置层级
                    FguiSortingOrderManager.Instance.ChangeSortingOrder(symbol.goOwnerSymbol, goExpectation);

                    // 边框
                    if (_spinWEMD.Instance.isFrame)
                    {
                        string borderEffect = CustomModel.Instance.borderEffect;
                        GComponent goBorderEffect = //FguiPoolManager.Instance.GetObject(TagPoolObject.SymbolBorder, borderEffect).asCom;
                            fguiPoolHelper.GetObject(TagPoolObject.SymbolBorder, borderEffect).asCom;
                        symbol.AddBorderEffect(goBorderEffect);
                    }
                }
                else if (tp == TagPoolObject.SymbolAppear)
                {
                    string symbolName = CustomModel.Instance.symbolAppearEffect[$"{symNumber}"];

                    // 图标动画
                    GComponent goSymbolHit = fguiPoolHelper.GetObject(TagPoolObject.SymbolAppear, symbolName).asCom;
                    symbol.AddSymbolEffect(goSymbolHit, isAmin);

                    // 设置层级
                    FguiSortingOrderManager.Instance.ChangeSortingOrder(symbol.goOwnerSymbol, goExpectation);
                }
            }

        }

        int symbolNumber = 0;
        public void ShowSymbolEffect(TagPoolObject tp, List<SymbolBase> symbols, bool isAmin, int symbolNumber, bool isUseMySelfSymbolNumber, int maxSymbolNumber = 15)
        {
            symbolNumber = 0;
            CloseSlotCover();
            foreach (SymbolBase symbol in symbols)
            {
                if (symbolNumber >= maxSymbolNumber)
                {
                    break;
                }
                symbolNumber++;
                GComponent goSymbol = symbol.goOwnerSymbol;

                int symNumber = isUseMySelfSymbolNumber ? symbol.number : symbolNumber;

                if (tp == TagPoolObject.SymbolHit)
                {
                    //显示遮罩
                    SetSlotCover(_spinWEMD.Instance.isShowCover);

                    string symbolName = CustomModel.Instance.symbolHitEffect[$"{symNumber}"];

                    // 图标动画
                    GComponent goSymbolHit = fguiPoolHelper.GetObject(TagPoolObject.SymbolHit, symbolName).asCom;
                    symbol.AddSymbolEffect(goSymbolHit, isAmin);

                    // 设置层级
                    FguiSortingOrderManager.Instance.ChangeSortingOrder(symbol.goOwnerSymbol, goExpectation);

                    // 边框
                    if (_spinWEMD.Instance.isFrame)
                    {
                        string borderEffect = CustomModel.Instance.borderEffect;
                        GComponent goBorderEffect = //FguiPoolManager.Instance.GetObject(TagPoolObject.SymbolBorder, borderEffect).asCom;
                            fguiPoolHelper.GetObject(TagPoolObject.SymbolBorder, borderEffect).asCom;
                        symbol.AddBorderEffect(goBorderEffect);
                    }
                }
                else if (tp == TagPoolObject.SymbolAppear)
                {
                    string symbolName = CustomModel.Instance.symbolAppearEffect[$"{symNumber}"];

                    // 图标动画
                    GComponent goSymbolHit = fguiPoolHelper.GetObject(TagPoolObject.SymbolAppear, symbolName).asCom;
                    symbol.AddSymbolEffect(goSymbolHit, isAmin);

                    // 设置层级
                    FguiSortingOrderManager.Instance.ChangeSortingOrder(symbol.goOwnerSymbol, goExpectation);
                }
            }
        }
        #endregion

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
                    int times = 0;
                    while(times < 3)
                    {
                        times++;
                        yield return ShowSymbolWinBySetting(winList[idx], true, SpinWinEvent.SingleWinLine);

                        if (isStopImmediately && _spinWEMD.Instance.isSkipAtStopImmediately)
                            break;
                        yield return new WaitForSeconds(0.5f);
                    }

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


        #region 切换指定图标变为特定图标

        /// <summary>
        /// 将特定索引图标变为指定索引图标
        /// </summary>
        /// <param name="symbolNumbers"> 要改变的图标索引 </param>
        /// <param name="target"> 指定的图标索引 </param>
        public void ChangeSymbolIcon(int symbolNumbers, int target)
        {
            for (int r = bufferTop; r < row + bufferTop; r++)
            {
                for (int c = 0; c < column; c++)
                {
                    ReelBase reel = reels[c];
                    SymbolBase symbol = reel.symbolList[r];
                    if (symbol.number == symbolNumbers)
                    {
                        symbol.number = target;
                        symbol.SetSymbolImage(target);
                    }
                }
            }
        }

        /// <summary>
        /// 礼盒游戏中根据中奖信息将位置更换为指定的图标
        /// </summary>
        /// <param name="winningLine"> 中奖信息 </param>
        /// <param name="targetIndex"> 想要更换的图标索引 </param>
        public void ChangeSymbolIcon(List<WinningLineInfo> winningLines, int targetIndex)
        {
            List<List<int>> allLines = ContentModel.Instance.payLines;
            foreach (WinningLineInfo item in winningLines)
            {
                int lineIndex = item.LineNumber - 1;

                List<int> line = allLines[lineIndex];

                for (int cIndex = 0; cIndex < item.WinCount; cIndex++)
                {
                    ReelBase reel = reels[cIndex];
                    int rIndex = line[cIndex];
                    SymbolBase symbol = reel.symbolList[rIndex + bufferTop];
                    symbol.number = targetIndex;
                    symbol.SetSymbolImage(targetIndex);
                }
            }
        }

        /// <summary>
        /// 礼盒游戏中根据中奖信息将位置更换为指定的图标
        /// </summary>
        /// <param name="targetIndex"> 想要更换的图标索引 </param>
        public void ChangeSymbolIcon(int targetIndex)
        {
            for (int r = bufferTop; r < row + bufferTop; r++)
            {
                for (int c = 0; c < column; c++)
                {
                    int index = (r - bufferTop) * column + c;
                    if (ContentModel.Instance.changeLiheIcon[index] == 1)
                    {
                        ReelBase reel = reels[c];
                        SymbolBase symbol = reel.symbolList[r];
                        symbol.SetSymbolImage(targetIndex);
                    }
                }
            }
        }

        #endregion

        public List<SymbolBase> IsWildGetSymbol(List<int> symbolNumbers)
        {
            int[] cols = ContentModel.Instance.cols.ToArray();
            List<SymbolBase> symbols = new List<SymbolBase>();
            for (int r = bufferTop; r < row + bufferTop; r++)
            {
                for (int c = 0; c < column; c++)
                {
                    if (cols.Contains(c)) continue;
                    ReelBase reel = reels[c];
                    SymbolBase symbol = reel.symbolList[r];
                    if (symbolNumbers.Contains(symbol.number))
                        symbols.Add(symbol);
                }
            }
            return symbols;
        }

        private List<GComponent> tempSymbols = new List<GComponent>();
        //特殊添加的播放转化的函数
        public void ShowSymbolTransform(List<int> symbolNumbers, bool isAmin, int symbolNumber, bool isUseMySelfSymbolNumber)
            => ShowSymbolTransform(TagPoolObject.SymbolAppear, GetSymbol(symbolNumbers), isAmin, symbolNumber, isUseMySelfSymbolNumber);

        public void ShowSymbolIdle(List<int> symbolNumbers, bool isAmin, int symbolNumber, bool isUseMySelfSymbolNumber)
            => ShowSymbolIdle(TagPoolObject.SymbolAppear, GetSymbol(symbolNumbers), isAmin, symbolNumber, isUseMySelfSymbolNumber);

        public void IsWildShowSymbolEffect(TagPoolObject tp, List<int> symbolNumbers, bool isAmin, int symbolNumber, bool isUseMySelfSymbolNumber)
            => ShowSymbolEffect(tp, IsWildGetSymbol(symbolNumbers), isAmin, symbolNumber, isUseMySelfSymbolNumber);

        #region 添加的切换动画的函数
        public void ShowSymbolTransform(TagPoolObject tp, List<SymbolBase> symbols, bool isAmin, int symbolNumber, bool isUseMySelfSymbolNumber)
        {
            foreach (SymbolBase symbol in symbols)
            {
                string symbolName = CustomModel.Instance.symbolAppearEffect["7"];

                // 图标动画
                GComponent goSymbolHit = fguiPoolHelper.GetObject(TagPoolObject.SymbolAppear, symbolName).asCom;
                symbol.AddSymbolEffect(goSymbolHit, isAmin);

                // 设置层级
                FguiSortingOrderManager.Instance.ChangeSortingOrder(symbol.goOwnerSymbol, goExpectation);
            }
        }

        #endregion

        public void ShowSymbolIdle(TagPoolObject tp, List<SymbolBase> symbols, bool isAmin, int symbolNumber, bool isUseMySelfSymbolNumber)
        {
            //关闭遮罩
            CloseSlotCover();

            //停止特效显示
            SkipWinLine(true);

            foreach (SymbolBase symbol in symbols)
            {
                GComponent goSymbol = symbol.goOwnerSymbol;

                int symNumber = isUseMySelfSymbolNumber ? symbol.number : symbolNumber;

                string symbolName = CustomModel.Instance.symbolAppearEffect[$"{symNumber}"];

                // 图标动画
                GComponent goSymbolHit = fguiPoolHelper.GetObject(TagPoolObject.SymbolAppear, symbolName).asCom;
                symbol.AddSymbolEffect(goSymbolHit, isAmin);

                // 设置层级
                TempSortOrder(symbol.goOwnerSymbol, goExpectation);
            }
        }


        public void SkipIdle(bool isIncludeTag)
        {
            List<SymbolBase> excludeSymbol = isIncludeTag ? new List<SymbolBase>()
                : GetHasEffectSymbols(new string[] { "symbol_appear#" });

            //Debug.LogError($" SkipWinLine: {isIncludeTag} : {excludeSymbol.Count} ");

            foreach (ReelBase reel in reels)
            {
                foreach (SymbolBase sb in reel.symbolList)
                {
                    if (excludeSymbol.Contains(sb))
                        continue;

                    sb.StopSymbolEffectBiggerTwinkle();
                    sb.HideBaseSymbolIcon(false);
                }
            }

            // 去除层级功能
            ReturnAllTempSortingOrder();

            foreach (ReelBase reel in reels)
            {
                string[] exclude = isIncludeTag ? new string[] { } : new string[] { "symbol_appear#" };// 

                fguiPoolHelper.ReturnAllToPool(reel.goSymbols, exclude);

            }

            fguiGObjectPoolHelper.ReturnAllToPool(goReels, new string[] { });


            GObject[] payLines = goPayLines.asCom.GetChildren();
            // 关掉所有线
            foreach (GObject line in payLines)
            {
                line.visible = false;
            }

            //EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_WIN_EVENT,
            //    new EventData(SlotMachineEvent.SkipWinLine));
        }


        public void ShowMultipleHit(List<int> symbolNumbers, bool isAmin, int symbolNumber, bool isUseMySelfSymbolNumber)
    => ShowMultipleHit(GetSymbol(symbolNumbers), isAmin, symbolNumber, isUseMySelfSymbolNumber);
        public void ShowMultipleHit(List<SymbolBase> symbols, bool isAmin, int symbolNumber, bool isUseMySelfSymbolNumber)
        {
            foreach (SymbolBase symbol in symbols)
            {
                GComponent goSymbol = symbol.goOwnerSymbol;

                int symNumber = isUseMySelfSymbolNumber ? symbol.number : symbolNumber;

                string symbolName = CustomModel.Instance.symbolHitEffect[$"{symNumber}"];

                // 图标动画
                GComponent goSymbolHit = fguiPoolHelper.GetObject(TagPoolObject.SymbolHit, symbolName).asCom;
                symbol.AddSymbolEffect(goSymbolHit, isAmin);

                // 设置层级
                FguiSortingOrderManager.Instance.ChangeSortingOrder(symbol.goOwnerSymbol, goExpectation);

                // 倍率字体
                string multipleEffect = CustomModel.Instance.multipleSymbols[ContentModel.Instance.multiple.ToString()];
                GComponent goBorderEffect = fguiPoolHelper.GetObject(TagPoolObject.SymbolHit, multipleEffect).asCom;
                symbol.AddBorderEffect(goBorderEffect);
            }
        }


        /// <summary>
        /// 滚轮正常滚动
        /// </summary>
        /// <param name="strDeckRowCol"></param>
        /// <param name="finishCallback"></param>
        /// <returns></returns>
        public new IEnumerator TurnReelsNormal(string strDeckRowCol = "1,1,1,1,1#2,2,6,2,2#3,3,3,3,3", Action finishCallback = null)
        {
            //停止特效显示
            SkipWinLine(false);

            int[] deckColRow = SlotTool.GetDeckColRow(strDeckRowCol).ToArray();
            List<List<int>> colrowLsts = GetDeckColRow(deckColRow,
                this.column,
                this.row);

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
        public List<List<int>> GetDeckColRow(int[] deckColRow, int colCount, int rowCount) 
        {
            if (ContentModel.Instance.isReelsSlowMotion) slowCols.Clear();

            List<List<int>> colrowLsts = new List<List<int>>();
            for (int col = 0; col < colCount; col++)
            {
                List<int> colLst = new List<int>();
                for (int row = 0; row < rowCount; row++)
                {
                    int syb = deckColRow[col * rowCount + row];
                    if (ContentModel.Instance.isReelsSlowMotion && syb == 9)
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
                    //   Debug.LogError(_reelSetMD.Instance.GetTimeTurnStartDelay(reelIdx));
                    //yield return new WaitForSeconds(_reelSetMD.Instance.GetTimeTurnStartDelay(reelIdx));
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
                    _reelSetMD.Instance.GetNumReelTurn(reelIdx) + reelIdx * _reelSetMD.Instance.GetNumReelTurnGap(reelIdx) + extraReelTimes * (reelIdx - (slowCols.Count < 2? reelIdx : slowCols[1])),
                    () =>
                    {
                        if (isTrriger)
                        {
                            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT,
                            new EventData<int>(SlotMachineEvent.PrepareStoppedReel, _reelIdx + 1));
                        }

                        //if (isSymbolAppearEffectWhenReelStop)
                        //    ShowReelSymbolAppearEffect(_reelIdx);

                        if (--reelsCount <= 0)
                        {
                            isNext = true;
                        }

                        if (ContentModel.Instance.isFreeSpin && ContentModel.Instance.wildPos.ContainsKey(_reelIdx))
                        {
                            for (int i = 2; i < 2 + CustomModel.Instance.row; i++)
                            {
                                if (reels[_reelIdx].symbolList[i].number == 8 && ContentModel.Instance.wildPos[_reelIdx].Contains(i - 2))
                                {
                                    string symbolName = CustomModel.Instance.symbolAppearEffect["6"];

                                    // 图标动画
                                    GComponent goSymbolHit = fguiPoolHelper.GetObject(TagPoolObject.SymbolAppear, symbolName).asCom;
                                    reels[_reelIdx].symbolList[i].AddSymbolEffect(goSymbolHit, true);

                                    // 设置层级
                                    FguiSortingOrderManager.Instance.ChangeSortingOrder(reels[_reelIdx].symbolList[i].goOwnerSymbol, goExpectation);
                                }
                            }
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

        }

        /// <summary>
        /// 滚轮滚动单次
        /// </summary>
        /// <param name="strDeckRowCol"></param>
        /// <param name="finishCallback"></param>
        /// <returns></returns>
        public new IEnumerator TurnReelsOnce(string strDeckRowCol = "1,1,1,1,1#2,2,6,2,2#3,3,3,3,3", Action finishCallback = null)
        {

            SkipWinLine(false);

            int[] deckColRow = SlotTool.GetDeckColRow(strDeckRowCol).ToArray();
            List<List<int>> colrowLsts = GetDeckColRow(deckColRow,
                this.column,
                this.row);

            List<int>[] colrow = colrowLsts.ToArray();

            //这个还要判断特殊图标 如果有还需要改变滚轮滚的次数 还有特殊表现效果
            //模拟图标
            for (int i = 0; i < this.column; i++)
            {
                reels[i].SetResult(colrow[i]);
            }

            yield return ReelsToStopOrTurnOnce(null);
            // 算分

            finishCallback?.Invoke();
        }

        /// <summary>
        /// 已滚动的滚轮立马停止、未滚动的滚轮滚动一次
        /// </summary>
        /// <param name="finishCallback"></param>
        /// <returns></returns>
        public new IEnumerator ReelsToStopOrTurnOnce(Action finishCallback)
        {

            // EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_SLOT_EVENT,
            //    new EventData(SlotMachineEvent.SpinSlotMachine));

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
                            new EventData<int>(SlotMachineEvent.PrepareStoppedReel, _reelIdx));

                        //if (isSymbolAppearEffectWhenReelStop)
                        //    ShowReelSymbolAppearEffect(_reelIdx);

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


        #region 临时添加的可以持续保持在界面上不被停止特效影响的方法
        Dictionary<GObject, SortingOrderInfo> tempNodes = new Dictionary<GObject, SortingOrderInfo>();

        public void TempSortOrder(GObject goTarget, GComponent toNode, string mark = "", Func<GComponent, int> funcToChildIndex = null, Func<GComponent, int> funcFromChildIndex = null)
        {
            SortingOrderInfo info = new SortingOrderInfo()
            {
                mark = mark,
                fromeNode = goTarget.parent,
                toNode = toNode,
                fromLocalPos = new Vector2(goTarget.x, goTarget.y),
                funcFromChildIndex = funcFromChildIndex,
                funcToChildIndex = funcToChildIndex,
                //fromChildIndex = goTarget.parent.GetChildIndex(goTarget),
            };

            if (!tempNodes.ContainsKey(goTarget))
                tempNodes.Add(goTarget, info);
            else
                tempNodes[goTarget] = info;

            Vector2 worldPos = LocalToGlobal(goTarget);
            Vector2 localPos = GlobalToLocal(toNode, worldPos);

            // 这里要加个延时！！
            goTarget.RemoveFromParent();
            toNode.AddChildAt(goTarget, funcToChildIndex != null ? funcToChildIndex(toNode) : toNode.numChildren);

            //goTarget.xy = localPos; // 适合父节点轴线在左上角(0,0)

            // 父节点fromeNode设置了轴心会影响到最终的位置（需要矫正位置！）
            goTarget.xy = new Vector2(localPos.x - info.fromeNode.pivotX * info.fromeNode.width,
                localPos.y - info.fromeNode.pivotY * info.fromeNode.height);

            //DebugUtils.Log($"{info.fromLocalPos.x},{info.fromLocalPos.y}  -- {worldPos.x},{worldPos.y} -- {localPos.x},{localPos.y}");

            // 延时设置索引！！
        }

        public void ReturnAllTempSortingOrder(bool isUseCurPos = false)
        {
            int i = tempNodes.Count;
            while (--i >= 0)
            {
                KeyValuePair<GObject, SortingOrderInfo> item = tempNodes.ElementAt(i);
                ReturnTempSortingOrder(item.Key, isUseCurPos);
            }
        }

        public void ReturnTempSortingOrder(GObject goTarget, bool isUseCurPos = false)
        {
            if (!tempNodes.ContainsKey(goTarget)) return;

            SortingOrderInfo info = tempNodes[goTarget];
            tempNodes.Remove(goTarget);

            Vector2 localPos = info.fromLocalPos;
            if (isUseCurPos)
            {
                Vector2 worldPos = LocalToGlobal(goTarget);
                localPos = GlobalToLocal(info.fromeNode, worldPos);
            }
            goTarget.RemoveFromParent();
            //info.fromeNode.AddChildAt(goTarget, info.funcFromChildIndex != null ? info.funcFromChildIndex(info.fromeNode) : info.fromeNode.numChildren);
            info.fromeNode.AddChildAt(goTarget, info.fromeNode.numChildren);
            RequestSetIndex(goTarget,
                info.funcFromChildIndex != null ? info.funcFromChildIndex(info.fromeNode) : info.fromeNode.numChildren);

            //goTarget.xy = localPos; // 适合父节点轴线在左上角(0,0)

            // 矫正位置
            goTarget.xy = new Vector2(localPos.x - info.toNode.pivotX * info.toNode.width,
                localPos.y - info.toNode.pivotY * info.toNode.height);
        }


        Vector2 LocalToGlobal(GObject go)
        {
            //go.parent.LocalToRoot
            Vector2 worldPos = go.parent.LocalToGlobal(go.xy);
            return worldPos;
        }

        Vector2 GlobalToLocal(GObject toParent, Vector2 worldPos)
        {
            Vector2 localPos = toParent.GlobalToLocal(worldPos);
            return localPos;
        }


        private Dictionary<GComponent, List<object[]>> requestIndexTasks = new Dictionary<GComponent, List<object[]>>();
        void RequestSetIndex(GObject goChild, int index)
        {
            //DebugUtils.LogError($"i am set index: {goChild.parent.parent.name}  index: {index}");
            if (!requestIndexTasks.ContainsKey(goChild.parent))
            {
                requestIndexTasks.Add(goChild.parent, new List<object[]>());
            }

            List<object[]> lst = requestIndexTasks[goChild.parent];
            bool isAdd = false;
            for (int i = 0; i < lst.Count; i++)
            {
                object[] item = lst[i];

                int idxExpect = (int)item[0];
                if (index <= idxExpect)
                {
                    try
                    {
                        lst.Insert(i, new object[] { index, goChild });
                        isAdd = true;
                    }
                    catch (Exception e)
                    {
                        DebugUtils.LogError($" idx: {idxExpect} lst.count: {lst.Count} index: {index} name: {goChild.parent.parent.name}");
                        throw e;
                    }
                    break;
                }
            }
            if (!isAdd)
                lst.Add(new object[] { index, goChild });

            Timers.inst.Remove(DoRequestSetIndex);
            Timers.inst.Add(0.02f, 1, DoRequestSetIndex);
        }


        void DoRequestSetIndex(object param)
        {
            while (requestIndexTasks.Count > 0)
            {
                KeyValuePair<GComponent, List<object[]>> task = requestIndexTasks.ElementAt(requestIndexTasks.Count - 1);
                requestIndexTasks.Remove(task.Key);

                for (int i = 0; i < task.Value.Count; i++)
                {
                    object[] item = task.Value[i];
                    int index = (int)item[0];
                    GObject goNode = (GObject)item[1];
                    goNode.parent.SetChildIndex(goNode, index);
                }
            }
        }
        #endregion
    }
}
