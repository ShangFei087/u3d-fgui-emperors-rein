using System.Collections;
using System.Collections.Generic;
using GameMaker;
using FairyGUI;
using _customMD = SlotEmperorsRein.CustomModel;
using _spinWEMD = SlotMaker.SpinWinEffectSettingModel;
using UnityEngine;
using SlotMaker;

namespace SlotEmperorsRein
{

    public enum SpinWinEvent
    {
        None,
        TotalWinLine,
        SingleWinLine,
    }

    public partial class SlotMachineController300 : SlotMachineBaseController
    {
        /// <summary> the anchor for "symbol hit" or "symbol appear"</summary>

        public void Init(GComponent gSlotCover, GComponent gPayLines, GComponent gReels, GComponent gExpectation, FguiPoolHelper fguiPoolHelper, FguiGObjectPoolHelper gObjectPoolHelper)
        {


            base.Init((ICustomModel)CustomModel.Instance, gSlotCover, gPayLines, gReels, fguiPoolHelper, gObjectPoolHelper);

            goExpectation = gExpectation;

            this.column = CustomModel.Instance.column;
            this.row = CustomModel.Instance.row;

            Transform tfmReels = transform.Find("Reels");
            reels = new List<ReelBase>();
            for (int i = 0; i < this.column; i++)
            {
                Reel reel = tfmReels.GetChild(i).GetComponent<Reel>();
                reel.reelIndex = i;
                reels.Add(reel);


                reel.Init((ICustomModel)CustomModel.Instance,goReels.GetChildAt(i).asCom, gExpectation);

            }


            //gPayLines.visible = false;

        }
        bool isSymbolAnim => _spinWEMD.Instance.isTwinkle ? false : _spinWEMD.Instance.isSymbolAnim;

        protected void OnDisable()
        {
            //ClearAllCor();
        }


        public bool CheckHasSymbolChange(SymbolWin curSymbolWin)
        {
            List<Cell> cells = curSymbolWin.cells;
            for (int i = 0; i < cells.Count; i++)
            {
                Cell cel = cells[i];
                SymbolBase symble = GetVisibleSymbolFromDeck(cel.column, cel.row);
                if (symble.number != 0 && symble.number != curSymbolWin.symbolNumber)
                {
                    return true;
                }
            }
            return false;
        }


        public bool Check5kind(SymbolWin curSymbolWin)
        {
            List<int> rowIndexLst = new List<int>();
            foreach (Cell cel in curSymbolWin.cells)
            {
                if (!rowIndexLst.Contains(cel.column))
                {
                    rowIndexLst.Add(cel.column);
                }
            }
            return rowIndexLst.Count == 5;
        }


        #region 开奖动画

        public IEnumerator ShowSymbolWinBySetting(SymbolWin symbolWin, bool isUseMySelfSymbolNumber, SpinWinEvent eventType)
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
                Symbol symble = (Symbol)GetVisibleSymbolFromDeck(cel.column, cel.row);

                int symbolNumber = isUseMySelfSymbolNumber ? symble.number : symbolWin.symbolNumber;

                string symbolName = CustomModel.Instance.symbolHitEffect[$"{symbolNumber}"];  // wild  or symbol;

                DebugUtils.Log(symbolName);
                // 图标动画  
                GComponent goSymbolAppear = fguiPoolHelper.GetObject(TagPoolObject.SymbolHit, symbolName).asCom;
                symble.AddSymbolEffect(goSymbolAppear, isSymbolAnim);


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
                if (_spinWEMD.Instance.isTwinkle)
                    symble.ShowTwinkleEffect();
                else if (_spinWEMD.Instance.isBigger)
                    symble.ShowBiggerEffect();

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


        public IEnumerator ShowSymbolBonusBySetting(List<SymbolInclude> bonusWin, bool isUseMySelfSymbolNumber)
        {
            DebugUtils.LogError(bonusWin.Count);
            //停止特效显示
            SkipWinLine(false);

            // 立马停止时，不播放赢分环节？
            if (isStopImmediately && _spinWEMD.Instance.isSkipAtStopImmediately)
                yield break;

            //显示遮罩
            SetSlotCover(_spinWEMD.Instance.isShowCover);

            foreach (var item in bonusWin)
            {
                Symbol symble = (Symbol)GetVisibleSymbolFromDeck(item.colIdx + 1, item.rowIdx + 1);
                string symbolName = CustomModel.Instance.symbolAppearEffect[$"{item.symbolNumber}"];  // wild  or symbol;

                // 图标动画  
                GComponent goSymbolAppear = fguiPoolHelper.GetObject(TagPoolObject.SymbolHit, symbolName).asCom;
                symble.AddSymbolEffect(goSymbolAppear, isSymbolAnim);



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
                if (_spinWEMD.Instance.isTwinkle)
                    symble.ShowTwinkleEffect();
                else if (_spinWEMD.Instance.isBigger)
                    symble.ShowBiggerEffect();

            }


            // yield return SlotWaitForSeconds(_spinWEMD.Instance.timeS);
        }



        public IEnumerator ShowSymbolBonusBySetting_LaBa(List<BonusWin> bonusWin, bool isUseMySelfSymbolNumber)
        {

            //停止特效显示
            SkipWinLine(false);

            // 立马停止时，不播放赢分环节？
            if (isStopImmediately && _spinWEMD.Instance.isSkipAtStopImmediately)
                yield break;

            //显示遮罩
            SetSlotCover(_spinWEMD.Instance.isShowCover);

            foreach (var item in bonusWin)
            {
                SymbolBase symble = GetVisibleSymbolFromDeck(item.cell.column, item.cell.row);
                string symbolName = CustomModel.Instance.symbolHitEffect[$"{item.symbolNumber}"];  // wild  or symbol;

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

                // 数字
                GComponent bonustext = fguiGObjectPoolHelper.GetObject("ui://EmperorsRein/BonusText").asCom;
                bonustext.visible = true;
                symble.AddAnchor(bonustext).GetChild("Text").asTextField.text = item.earnCredit.ToString();

                // 整体变大特效
                if (_spinWEMD.Instance.isTwinkle)
                    symble.ShowTwinkleEffect();
                else if (_spinWEMD.Instance.isBigger)
                    symble.ShowBiggerEffect();

            }


            // yield return SlotWaitForSeconds(_spinWEMD.Instance.timeS);
        }





        public SymbolWin GetTotalSymbolWin(List<SymbolWin> winList)
        {
            List<SymbolBase> bsLst = new List<SymbolBase>();

            long earnCredit = 0;
            List<Cell> cells = new List<Cell>();

            List<int> lineIndexs = new List<int>();

            foreach (SymbolWin sw in winList)
            {
                foreach (Cell cel in sw.cells)
                {
                    SymbolBase symble = GetVisibleSymbolFromDeck(cel.column, cel.row);
                    if (bsLst.Contains(symble))
                        continue;
                    cells.Add(new Cell(cel.column, cel.row));
                    bsLst.Add(symble);
                }

                // 获得所有赢线的线号
                if (!lineIndexs.Contains(sw.lineNumber))
                    lineIndexs.Add(sw.lineNumber);

                earnCredit += sw.earnCredit;
            }

            TotalSymbolWin totalWin = new TotalSymbolWin()
            {
                lineNumbers = lineIndexs,
                earnCredit = earnCredit,
                cells = cells,
            };

            return totalWin;
        }

        /// <summary> 关闭遮罩层 </summary>
        public void CloseSlotCover() => SetSlotCover(false);

        /// <summary> 打开遮罩层 </summary>
        public void OpenSlotCover() => SetSlotCover(true);

        public void SetSlotCover(bool isShow)
        {
            if (goSlotCover != null)
                goSlotCover.visible = isShow;
        }

        /// <summary>
        /// 显示所有赢线的图标，一次
        /// </summary>
        /// <param name="winList"></param>
        public IEnumerator ShowWinListBySetting(List<SymbolWin> winList)
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

        public IEnumerator ShowWinListAwayDuringIdle009(List<SymbolWin> winList)
        {
            while (winList.Count > 0) //while (idx < winList.Count)
            {
                yield return ShowWinListBySetting(winList);
            }
        }


        /// <summary>
        /// 播放“图标改变特效”，并改变图标的图标号
        /// </summary>
        /// <param name="symbolWin"></param>
        /// <param name="minS"></param>
        /// <returns></returns>
        public IEnumerator ShowSymbolChangeBySetting(SymbolWin symbolWin, string symbolEffectName) //"Symbol Change"
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

                SymbolBase symble = GetVisibleSymbolFromDeck(cel.column, cel.row);

                string symbolName = CustomModel.Instance.symbolHitEffect[$"{symbolWin.symbolNumber}"];

                GComponent goSymbolHit = fguiPoolHelper.GetObject(TagPoolObject.SymbolHit, symbolName).asCom;
                symble.AddSymbolEffect(goSymbolHit, isSymbolAnim);

                // 边框
                if (_spinWEMD.Instance.isFrame)
                {
                    string borderEffect = CustomModel.Instance.borderEffect;
                    GComponent goBorderEffect = //FguiPoolManager.Instance.GetObject(TagPoolObject.SymbolBorder, borderEffect).asCom;
                        fguiPoolHelper.GetObject(TagPoolObject.SymbolBorder, borderEffect).asCom;
                    symble.AddBorderEffect(goBorderEffect);
                }

                if (_spinWEMD.Instance.isTwinkle)
                    symble.ShowTwinkleEffect();
                else if (_spinWEMD.Instance.isBigger)
                    symble.ShowBiggerEffect();

            }

            yield return SlotWaitForSeconds(_spinWEMD.Instance.timeS);

            ChangeSymbol(symbolWin);

        }


        public void ShowSymbolWinDeck(SymbolWin symbolWin, bool isUseMySelfSymbolNumber)
        {
            //停止特效显示
            SkipWinLine(false);

            //显示遮罩
            SetSlotCover(_spinWEMD.Instance.isShowCover);

            foreach (Cell cel in symbolWin.cells)
            {
                Symbol symble = (Symbol)GetVisibleSymbolFromDeck(cel.column, cel.row);

                int symbolNumber = isUseMySelfSymbolNumber ? symble.number : symbolWin.symbolNumber;

                string symbolName = CustomModel.Instance.symbolHitEffect[$"{symbolNumber}"];  // wild  or symbol;
                DebugUtils.Log(symbolName);
                // 图标动画
                GComponent goSymbolAppear = fguiPoolHelper.GetObject(TagPoolObject.SymbolHit, symbolName).asCom;
                symble.AddSymbolEffect(goSymbolAppear, isSymbolAnim);



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
                if (_spinWEMD.Instance.isTwinkle)
                    symble.ShowTwinkleEffect();
                else if (_spinWEMD.Instance.isBigger)
                    symble.ShowBiggerEffect();

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


        public void ShowSymbolWinDeck(SymbolWin symbolWin, bool isUseMySelfSymbolNumber, SpinWinEvent eventType)
        {

            ShowSymbolWinDeck(symbolWin, isUseMySelfSymbolNumber);

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
        }



        #endregion

    }
}