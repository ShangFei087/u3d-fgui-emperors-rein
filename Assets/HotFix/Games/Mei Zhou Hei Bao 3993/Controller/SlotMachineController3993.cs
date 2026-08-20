using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _spinWEMD = SlotMaker.SpinWinEffectSettingModel;
using _reelSetMD = SlotMaker.ReelSettingModel;
using PusherEmperorsRein;

namespace MeiZhouHeiBao_3993
{

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
        public void Init(GComponent gSlotCover, GComponent gPayLines, GComponent gReels, GComponent gExpectation,FguiPoolHelper fGuiPoolHelper, FguiGObjectPoolHelper gObjectPoolHelper)
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
                reel.fguiPoolHelper = this.fguiPoolHelper;
            }

            bufferTop = 2; // 滚轴上方有几个图标
        }

        public override IEnumerator ShowWinListAwayDuringIdle(List<SymbolWin> winList)
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

        public override IEnumerator ShowSymbolWinBySetting(SymbolWin symbolWin, bool isUseMySelfSymbolNumber,SpinWinEvent eventType)
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

                RecycleSymbolAppear(symbol);
                GComponent goSymbolHit = fguiPoolHelper.GetObject(TagPoolObject.SymbolHit, symbolName).asCom;
                symbol.AddSymbolEffect(goSymbolHit, isSymbolAnim);
                PlayEffectAnim(goSymbolHit, ResolveWildAnim(symbol, cel.row, cel.column, isWin: true));
                TryBindBonusScore(symbol, cel.row, cel.column);

                // 设置层级
                FguiSortingOrderManager.Instance.ChangeSortingOrder(symbol.goOwnerSymbol, goExpectation);

                // 边框
                if (_spinWEMD.Instance.isFrame)
                {
                    string borderEffect = CustomModel.Instance.borderEffect;
                    GComponent goBorderEffect =fguiPoolHelper.GetObject(TagPoolObject.SymbolBorder, borderEffect).asCom;
                    symbol.AddBorderEffect(goBorderEffect);
                    if (goBorderEffect.parent != null) goBorderEffect.parent.SetChildIndex(goBorderEffect, goBorderEffect.parent.numChildren - 1);

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

        public override IEnumerator TurnReelsNormal(string strDeckRowCol = "1,1,1,1,1#2,2,6,2,2#3,3,3,3,3", Action finishCallback = null)
        {
            //停止特效显示
            ClearBonusScoreBinds();
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
                    if (syb == ScatterSymbolId)
                    {
                        if (freeIconCols.Count == 0 || freeIconCols[freeIconCols.Count - 1] != col)
                            freeIconCols.Add(col);
                    }
                    else if (syb == BonusSymbolId)
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
        protected override IEnumerator StartTurnReels()
        {
            int reelsCount = this.column;
            bool isNext = false;
            ContentModel.Instance.isFreeSlotTip = false;

            for (int reelIdx = 0; reelIdx < this.column; reelIdx++)
            {
                int index = reelIdx;
                int extraReelTimes = 0;
                bool isTrigger = false;
                int extraReelTimesReel = 0;

                bool scatterExpect = freeIconCols.Count >= ScatterExpectCount
                                     && reelIdx >= freeIconCols[ScatterExpectCount - 1];
                bool bonusExpect = jackpotIconCols.Count >= BonusExpectCount
                                   && reelIdx >= jackpotIconCols[BonusExpectCount - 1];

                if (scatterExpect || bonusExpect)
                {
                    extraReelTimes = 15;
                    isTrigger = true;
                    if (scatterExpect)
                        ContentModel.Instance.isFreeSlotTip = true;

                    extraReelTimesReel = scatterExpect
                        ? reelIdx - freeIconCols[ScatterExpectCount - 1]
                        : reelIdx - jackpotIconCols[BonusExpectCount - 1];
                }

                reels[reelIdx].StartTurn(
                    _reelSetMD.Instance.GetNumReelTurn(reelIdx) +
                    reelIdx * _reelSetMD.Instance.GetNumReelTurnGap(reelIdx) + extraReelTimes * extraReelTimesReel,
                    () =>
                    {
                        EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT,new EventData<int>(SlotMachineEvent.ReelColumnStopSound, index));
                        ComputeScatterBonusColumnStopFlags(reels[index], index, out bool scatterCol, out bool bonusCol);
                        EventCenter.Instance.EventTrigger<EventData>(
                            SlotMachineEvent.ON_SLOT_DETAIL_EVENT,
                            new EventData<ScatterBonusColumnStopPayload>(SlotMachineEvent.ScatterBonusColumnStopSound,
                            new ScatterBonusColumnStopPayload { column0Based = index, hasScatter = scatterCol, hasBonus = bonusCol, }));
                        if (isTrigger)
                        {
                            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_SLOT_DETAIL_EVENT,
                                new EventData<int>(SlotMachineEvent.PrepareStoppedReel, index + 1));
                        }

                        if (isSymbolAppearEffectWhenReelStop)
                            ShowReelSymbolAppearEffect(index);

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
        public override IEnumerator ReelsToStopOrTurnOnce(Action finishCallback)
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

                        if (isSymbolAppearEffectWhenReelStop) ShowReelSymbolAppearEffect(_reelIdx);

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

        /// <summary> 扫描单列可视区是否含 Scatter/Bonus，供 ScatterBonusColumnStopSound 载荷。 </summary>
        private void ComputeScatterBonusColumnStopFlags(ReelBase reel, int column0Based, out bool hasScatter, out bool hasBonus)
        {
            hasScatter = false;
            hasBonus = false;
            if (reel?.symbolList == null)
                return;

            int row = CustomModel.Instance.row;
            if (column0Based < 0 || column0Based >= CustomModel.Instance.column)
                return;

            int scatterId = ScatterSymbolId;
            int bonusId = BonusSymbolId;
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

        public  override void ShowSymbolWinDeck(SymbolWin symbolWin, bool isUseMySelfSymbolNumber)
        {
            SkipWinLine(false);
           
            SetSlotCover(_spinWEMD.Instance.isShowCover);

            foreach (Cell cel in symbolWin.cells)
            {
                SymbolBase symbol = GetVisibleSymbolFromDeck(cel.column, cel.row);
                int symbolNumber = isUseMySelfSymbolNumber ? symbol.number : symbolWin.symbolNumber;
                RecycleSymbolAppear(symbol);
                string symbolName = CustomModel.Instance.symbolHitEffect[$"{symbolNumber}"];
                GComponent goSymbolHit = fguiPoolHelper.GetObject(TagPoolObject.SymbolHit, symbolName).asCom;
                FguiSortingOrderManager.Instance.ChangeSortingOrder(symbol.goOwnerSymbol, goExpectation);

                symbol.AddSymbolEffect(goSymbolHit, isSymbolAnim);
                if (symbolNumber == WildSymbolId)
                {
                    PlayEffectAnim(goSymbolHit, ResolveWildAnim(symbol, cel.row, cel.column, isWin: true));
                }
                if (symbolNumber == BonusSymbolId)
                {
                    TryBindBonusScore(symbol, cel.row, cel.column);
                }

                if (_spinWEMD.Instance.isFrame)
                {
                    string borderEffect = CustomModel.Instance.borderEffect;
                    GComponent goBorderEffect = fguiPoolHelper.GetObject(TagPoolObject.SymbolBorder, borderEffect).asCom;
                    symbol.AddBorderEffect(goBorderEffect);
                }
            }
        }
        public override void ShowReelSymbolAppearEffect(int colIndex)
        {
            base.ShowReelSymbolAppearEffect(colIndex);
            PlayWildAnimsOnColumn(colIndex, isWin: false);
            BindBonusScoreOnColumn(colIndex);
        }

        public override void ShowSymbolEffect(TagPoolObject tp, List<SymbolBase> symbols, bool isAmin, int symbolNumber, bool isUseMySelfSymbolNumber)
        {
            bool isHit = tp == TagPoolObject.SymbolHit;
            if (isHit)
            {
                for (int i = 0; i < symbols.Count; i++)
                    RecycleSymbolAppear(symbols[i]);
            }

            base.ShowSymbolEffect(tp, symbols, isAmin, symbolNumber, isUseMySelfSymbolNumber);

            if (tp != TagPoolObject.SymbolHit && tp != TagPoolObject.SymbolAppear)
                return;

            for (int col = 0; col < this.column; col++)
            {
                for (int row = 0; row < this.row; row++)
                {
                    SymbolBase symbol = GetVisibleSymbolFromDeck(col, row);
                    if (symbol == null || !symbols.Contains(symbol))
                        continue;

                    PlayAttachedSymbolAnim(symbol, ResolveWildAnim(symbol, row, col, isHit));
                    if (isHit && symbol.number == BonusSymbolId)
                        TryBindBonusScore(symbol, row, col);
                }
            }
        }

        public override void BeginSpin()
        {
            ClearBonusScoreBinds();
            base.BeginSpin();
        }

        #region Bonus图标
        private const string Bonus12BonePath ="Anchor/Spine Mecanim GameObject (ng_sym14_Bonus)/SkeletonUtility-SkeletonRoot/root/All/coin/number";

        private const int ScatterSymbolId = 11;
        private const int BonusSymbolId = 12;
        /// <summary> Scatter 出现满该数量后，后续列进入免费加速听牌。 </summary>
        private const int ScatterExpectCount = 2;
        /// <summary> Bonus 出现满该数量后，后续列进入大奖加速听牌（大奖需 6 个）。 </summary>
        private const int BonusExpectCount = 3;

        private sealed class BonusScoreBind
        {
            public AnimPlayer Anim;
            public GComponent Num;
            public int Row;
            public int Col;
        }

        private readonly List<BonusScoreBind> _bonusScoreBinds = new List<BonusScoreBind>();

        private void BindBonusScoreOnColumn(int colIndex)
        {
            if (colIndex < 0 || colIndex >= reels.Count)
                return;

            for (int row = 0; row < this.row; row++)
                TryBindBonusScore(GetVisibleSymbolFromDeck(colIndex, row), row, colIndex);
        }

        private void TryBindBonusScore(SymbolBase symbol, int row, int col)
        {
            if (symbol == null || symbol.number != BonusSymbolId)
                return;

            GComponent animator = symbol.goOwnerSymbol.GetChild("animator")?.asCom;
            if (animator == null || animator.numChildren <= 0)
                return;

            GComponent effectCom = animator.GetChildAt(animator.numChildren - 1)?.asCom;
            if (effectCom == null)
                return;

            GameObject goRoot = GameCommon.FguiUtils.GetWrapperTarget(effectCom);
            if (goRoot == null)
                return;

            int score = ResolveBonusScore(row, col);
            if (score <= 0)
                return;

            RemoveBonusScoreBindAt(row, col);

            GComponent numCom = UIPackage.CreateObject("MeiZhouHeiBao", "SmallGameNum")?.asCom;
            if (numCom == null)
                return;

            effectCom.AddChild(numCom);
            numCom.SetXY(0, 0);

            GTextField txt = numCom.GetChild("txtScore")?.asTextField;
            if (txt != null)
                txt.text = score.ToString();

            AnimPlayer anim = new AnimPlayer(goRoot);
            bool ok = anim.Attach(
                numCom,
                Bonus12BonePath,
                localPos: Vector3.zero,
                localScale: new Vector3(0.01f, 0.01f, 0.01f),
                localRot: Quaternion.identity);

            if (ok)
                _bonusScoreBinds.Add(new BonusScoreBind { Anim = anim, Num = numCom, Row = row, Col = col });
            else
                numCom.Dispose();
        }

        private void RemoveBonusScoreBindAt(int row, int col)
        {
            for (int i = _bonusScoreBinds.Count - 1; i >= 0; i--)
            {
                BonusScoreBind bind = _bonusScoreBinds[i];
                if (bind.Row != row || bind.Col != col)
                    continue;

                bind.Anim?.DetachAll();
                bind.Num?.Dispose();
                _bonusScoreBinds.RemoveAt(i);
            }
        }

        private int ResolveBonusScore(int row, int col)
        {
            int index = row * this.column + col; // row*5 + col
            int[] data = ContentModel.Instance.BonusData;
            if (data != null && index >= 0 && index < data.Length && data[index] > 0)
                return data[index];

            // 普通局没有 BonusData：随机 10~40 倍
            int multiple = UnityEngine.Random.Range(10, 41);
            return multiple * (int)ContentModel.Instance.totalBet;
        }

        public void ClearBonusScoreBinds()
        {
            for (int i = 0; i < _bonusScoreBinds.Count; i++)
            {
                _bonusScoreBinds[i].Anim?.DetachAll();
                _bonusScoreBinds[i].Num?.Dispose();
            }
            _bonusScoreBinds.Clear();
        }
        #endregion

        #region Wild图标
        private const int WildSymbolId = 10;

        private void RecycleSymbolAppear(SymbolBase symbol)
        {
            if (symbol?.goOwnerSymbol == null || fguiPoolHelper == null)
                return;
            fguiPoolHelper.ReturnToPool(TagPoolObject.SymbolAppear, symbol.goOwnerSymbol);
        }

        private void PlayWildAnimsOnColumn(int colIndex, bool isWin)
        {
            for (int row = 0; row < this.row; row++)
            {
                SymbolBase symbol = GetVisibleSymbolFromDeck(colIndex, row);
                PlayAttachedSymbolAnim(symbol, ResolveWildAnim(symbol, row, colIndex, isWin));
            }
        }

        private string ResolveWildAnim(SymbolBase symbol, int row, int col, bool isWin)
        {
            if (symbol == null || symbol.number != WildSymbolId)
                return null;
            return ContentModel.GetWildAnimName(ContentModel.Instance.GetWildMul(col, row), isWin);
        }

        private void PlayAttachedSymbolAnim(SymbolBase symbol, string animName)
        {
            if (symbol == null || string.IsNullOrEmpty(animName))
                return;

            GComponent animator = symbol.goOwnerSymbol.GetChild("animator")?.asCom;
            if (animator == null || animator.numChildren <= 0)
                return;

            PlayEffectAnim(animator.GetChildAt(animator.numChildren - 1)?.asCom, animName);
        }

        private void PlayEffectAnim(GComponent effectCom, string animName)
        {
            if (effectCom == null || string.IsNullOrEmpty(animName))
                return;

            GameObject goRoot = GameCommon.FguiUtils.GetWrapperTarget(effectCom);
            if (goRoot == null)
                return;

            new AnimPlayer(goRoot).Play(animName);
        }

        #endregion
    }
}