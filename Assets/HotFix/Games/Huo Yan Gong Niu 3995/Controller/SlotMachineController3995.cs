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

namespace HuoYanGongNiu_3995
{


    public partial class SlotMachineController3995 : SlotMachineBaseController
    {
        /// <summary> the anchor for "symbol hit" or "symbol appear"</summary>
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
                if (_spinWEMD.Instance.isBigger)
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

        public override void ShowSymbolEffect(TagPoolObject tp, List<SymbolBase> symbols, bool isAmin, int symbolNumber, bool isUseMySelfSymbolNumber)
        {
            CloseSlotCover();
            foreach (Symbol01 symbol in symbols)
            {
                GComponent goSymbol = symbol.goOwnerSymbol;

                int symNumber = isUseMySelfSymbolNumber ? symbol.number : symbolNumber;

                if (tp == TagPoolObject.SymbolHit)
                {
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


        /// <summary>
        /// 轮播显示单条赢线 或 显示所有赢线，发送事件，并延时等待
        /// </summary>
        /// <param name="winList"></param>
        /// <returns></returns>
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
                    while (times < 3)
                    {
                        times++;
                        yield return ShowSymbolWinBySetting(winList[idx], true, SpinWinEvent.SingleWinLine);

                        if (isStopImmediately && _spinWEMD.Instance.isSkipAtStopImmediately)
                            break;
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


        #region 滚轮滚动接口




        #endregion


        //彩金游戏中可以被记为彩金奖的元素列表
        List<int> symbolNumbers = new List<int>() { 12 };

        /// <summary>
        /// 彩金游戏完成后遍历所有的格子统计分数
        /// </summary>
        public IEnumerator JackpotWinCredit(Action successCallback = null)
        {
            for (int c = 0; c < column; c++)
            {
                for (int r = bufferTop; r < row + bufferTop; r++)
                {
                    ReelBase reel = reels[c];
                    SymbolBase symbol = reel.symbolList[r];
                    if (symbolNumbers.Contains(symbol.number))
                    {
                        ReturnTempSortingOrder(reels[c].symbolList[r].goOwnerSymbol);
                        fguiPoolHelper.ReturnAllToPool(reel.goSymbols, new string[] { });

                        string symbolName = CustomModel.Instance.symbolHitEffect["12"];

                        // 图标动画
                        GComponent goSymbolHit = fguiPoolHelper.GetObject(TagPoolObject.SymbolHit, symbolName).asCom;
                        symbol.AddSymbolEffect(goSymbolHit, true);

                        yield return new WaitForSeconds(1f);

                        // 设置层级
                        FguiSortingOrderManager.Instance.ChangeSortingOrder(symbol.goOwnerSymbol, goExpectation);

                        ContentModel.Instance.jackpotSpinWinCredit = int.Parse(symbol.goOwnerSymbol.GetChild("socre").asTextField.text);
                        EventCenter.Instance.EventTrigger<EventData>("JackpotWinCredit",
                                new EventData<Dictionary<int, int>>("FreeRewardEffect", new Dictionary<int, int>
                                {
                                    {c, r - bufferTop}
                                }));
                        symbol.goOwnerSymbol.GetChild("socre").asTextField.visible = false;

                        yield return new WaitForSeconds(1.5f);

                        SkipWinLine(true);
                    }

                }
            }

            successCallback?.Invoke();
        }


        public void ShowSymbolIdle(List<int> symbolNumbers, bool isAmin, int symbolNumber, bool isUseMySelfSymbolNumber)
            => ShowSymbolIdle(TagPoolObject.SymbolAppear, GetSymbol(symbolNumbers), isAmin, symbolNumber, isUseMySelfSymbolNumber);

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

                string symbolName = CustomModel.Instance.symbolAppearEffect[$"{symNumber + 100}"];

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