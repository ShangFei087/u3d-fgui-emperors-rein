using PusherEmperorsRein;
using GameMaker;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using _reelSetMD = SlotMaker.ReelSettingModel;
using _spinWEMD = SlotMaker.SpinWinEffectSettingModel;


namespace SlotMaker
{
    public class SlotMachineBaseController : MonoBehaviour
    {

        ICustomModel customModel;

        /// <summary> 遮罩 </summary>
        public GComponent goSlotCover;

        /// <summary> 线 </summary>
        public GComponent goPayLines;

        /// <summary> win线节点 </summary>
        protected GComponent goExpectation;

        /// <summary>【？？】 滚轮 </summary>
        public GComponent goReels;

        /// <summary>u3d预制体池 </summary>
        public FguiPoolHelper fguiPoolHelper;  // u3d: 3d模型、Spine、粒子特效

        /// <summary>fugi对象池 </summary>
        public FguiGObjectPoolHelper fguiGObjectPoolHelper; // fgui对象

        public virtual void Init(ICustomModel customModel , GComponent gSlotCover, GComponent gPayLines,GComponent gReels,FguiPoolHelper fguiPoolHelper,FguiGObjectPoolHelper gObjectPoolHelpe)
        {
            this.customModel = customModel;
            this.goPayLines = gPayLines;
            this.goSlotCover = gSlotCover;
            this.goReels = gReels;
            this.fguiPoolHelper = fguiPoolHelper;
            this.fguiGObjectPoolHelper = gObjectPoolHelpe;
            goPayLines.visible = true;
            goSlotCover.visible = false;

            for (int i = 0; i < goPayLines.numChildren; i++)
            {
                goPayLines.GetChildAt(i).visible = false;
            }

        }
        /// <summary>
        /// 所有滚轮
        /// </summary>
        public List<ReelBase> reels;

        /// <summary> 特殊图标播放动画 </summary>
        protected bool isSymbolAppearEffectWhenReelStop;


        /// <summary> 立马停止 </summary>
        public bool isStopImmediately = false;


        /// <summary> 滚轮静止时顶部预留图标个数 </summary>
        public int bufferTop = 1;

        /// <summary> 滚轮静止时底部预留图标个数 </summary>
        public int bufferButton = 4;

        [HideInInspector]
        public int column;

        [HideInInspector]
        public int row;


        #region 图标操作

        /// <summary>
        /// 获取甲板可见区域的第row行，第col列的图标对象
        /// </summary>
        /// <param name="colIndex"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        public SymbolBase GetVisibleSymbolFromDeck(int colIndex, int rowIndex)
        {
            int _row = rowIndex + bufferTop;

            try
            {
                ReelBase reel = reels[colIndex];
                SymbolBase symbol = reel.symbolList[_row];
                return symbol;
            }
            catch (Exception e)
            {
                DebugUtils.LogError($"col = {colIndex} row = {_row}");
                DebugUtils.LogException(e);
            }
            return null;
        }


        /// <summary>
        /// 改变SymbolWin所描述的中奖图标对象的图标图片
        /// </summary>
        /// <param name="symbolWin"></param>
        public void ChangeSymbol(SymbolWin symbolWin)
        {
            foreach (Cell cel in symbolWin.cells)
            {
                SymbolBase symble = GetVisibleSymbolFromDeck(cel.column, cel.row);
                symble.SetSymbolImage(symbolWin.symbolNumber);
            }
        }
        #endregion


        public IEnumerator SlotWaitForSeconds(float waitS)
        {
            if (waitS <= 0f)
                yield break;

            float targetTimeS = Time.time + waitS;
            while (targetTimeS > Time.time)
            {
                yield return null;
            }
        }

        /// <summary>
        /// 某列滚轮显示特殊图标﻿的动画
        /// </summary>
        /// <param name="colIndex"></param>
        protected void ShowReelSymbolAppearEffect(int colIndex)
        {
            ReelBase reel = reels[colIndex];
            reel.SymbolAppearEffect();
        }


        public void ShowSymbolAppearEffectAfterReelStop(bool isEnable = true)
        {
            isSymbolAppearEffectWhenReelStop = isEnable;
        }


        public virtual int GetPayLineIndex(int payLineNumber) => payLineNumber - 1;

        #region 甲板设置


        [Button]
        public void SetReelsDeck(string strDeckRowCol = "1,1,1,1,1#2,2,6,2,2#3,3,3,3,3")
        {
            //停止特效显示
            SkipWinLine(false);

            int[] deckColRow = SlotTool.GetDeckColRow(strDeckRowCol).ToArray();
            List<List<int>> colrowLsts = SlotTool.GetDeckColRow(deckColRow,
                this.column,
                this.row);

            List<int>[] colrow = colrowLsts.ToArray();

            //这个还要判断特殊图标 如果有还需要改变滚轮滚的次数 还有特殊表现效果
            //模拟图标
            for (int i = 0; i < this.column; i++)
            {
                reels[i].SetResult(colrow[i]);
                reels[i].SetReelDeck();
            }
        }

        #endregion


        #region 滚轮滚动接口



        protected IEnumerator StartTurnReels()
        {

            int reelsCount = this.column;

            bool isNext = false;

            for (int reelIdx = 0; reelIdx < this.column; reelIdx++)
            {
                if (_reelSetMD.Instance.GetTimeTurnStartDelay(reelIdx) > 0)
                {
                    //   DebugUtils.LogError(_reelSetMD.Instance.GetTimeTurnStartDelay(reelIdx));
                    yield return new WaitForSeconds(_reelSetMD.Instance.GetTimeTurnStartDelay(reelIdx));
                }

                int _reelIdx = reelIdx;

                reels[reelIdx].StartTurn(
                    _reelSetMD.Instance.GetNumReelTurn(reelIdx) + reelIdx * _reelSetMD.Instance.GetNumReelTurnGap(reelIdx),
                    () =>
                    {
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

        }


        /// <summary>
        /// 滚轮正常滚动
        /// </summary>
        /// <param name="strDeckRowCol"></param>
        /// <param name="finishCallback"></param>
        /// <returns></returns>
        public IEnumerator TurnReelsNormal(string strDeckRowCol = "1,1,1,1,1#2,2,6,2,2#3,3,3,3,3", Action finishCallback = null)
        {
            //停止特效显示
            SkipWinLine(false);

            int[] deckColRow = SlotTool.GetDeckColRow(strDeckRowCol).ToArray();
            List<List<int>> colrowLsts = SlotTool.GetDeckColRow(deckColRow,
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



        /// <summary>
        /// 滚轮滚动单次
        /// </summary>
        /// <param name="strDeckRowCol"></param>
        /// <param name="finishCallback"></param>
        /// <returns></returns>
        public IEnumerator TurnReelsOnce(string strDeckRowCol = "1,1,1,1,1#2,2,6,2,2#3,3,3,3,3", Action finishCallback = null)
        {

            SkipWinLine(false);

            int[] deckColRow = SlotTool.GetDeckColRow(strDeckRowCol).ToArray();
            List<List<int>> colrowLsts = SlotTool.GetDeckColRow(deckColRow,
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
        public IEnumerator ReelsToStopOrTurnOnce(Action finishCallback)
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


        #endregion



        #region 滚轮各阶段的事件


        /// <summary> 停止特效显示 </summary>
        public void SkipWinLine(bool isIncludeTag)
        {

            List<SymbolBase> excludeSymbol = isIncludeTag? new List<SymbolBase>()
                :GetHasEffectSymbols(new string[] { "symbol_appear#" });

            //DebugUtils.LogError($" SkipWinLine: {isIncludeTag} : {excludeSymbol.Count} ");

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
      
            FguiSortingOrderManager.Instance.ReturnAllSortingOrder();

            foreach (ReelBase reel in reels)
            {
                string[] exclude = isIncludeTag? new string[] {}: new string[] {"symbol_appear#"};// 
                
                fguiPoolHelper.ReturnAllToPool(reel.goSymbols, exclude);
            }
            
            fguiGObjectPoolHelper.ReturnAllToPool(goReels,new string[] {});            
            
            
            GObject[] payLines = goPayLines.asCom.GetChildren();
            // 关掉所有线
            foreach (GObject line in payLines)
            {
                line.visible = false;
            }

            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_WIN_EVENT,
                new EventData(SlotMachineEvent.SkipWinLine));
        }


        public long GetTotalWinCredit(List<SymbolWin> winList)
        {
            long totalWinCredit = 0;
            foreach (SymbolWin win in winList)
            {
                totalWinCredit += win.earnCredit;
            }
            return totalWinCredit;
        }




        public void SendPrepareTotalWinCreditEvent(long totalWinCredit, bool isAddToCredit = false)
        {
            PrepareTotalWinCredit res = new PrepareTotalWinCredit()
            {
                totalWinCredit = totalWinCredit,
                isAddToCredit = isAddToCredit,
            };

            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_WIN_EVENT,
                new EventData<PrepareTotalWinCredit>(SlotMachineEvent.PrepareTotalWinCredit, res));
        }

        /// <summary>
        /// 显示总赢(给控制面板)
        /// </summary>
        /// <param name="winList"></param>
        public void SendTotalWinCreditEvent(List<SymbolWin> winList)
        {
            long totalWinCredit = 0;
            foreach (SymbolWin win in winList)
            {
                totalWinCredit += win.earnCredit;
            }
            SendTotalWinCreditEvent(totalWinCredit);
        }
        public void SendTotalWinCreditEvent(long totalWinCredit)
        {
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_WIN_EVENT,
                new EventData<long>(SlotMachineEvent.TotalWinCredit, totalWinCredit));
        }


      

        /// <summary>
        /// 开始Spin
        /// </summary>
        /// <remarks>
        /// * Turn开始事件<br/>
        /// * 显示玩家金币（显示玩家上把金额）<br/>
        /// </remarks>
        public void BeginTurn()
        {
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_CONTENT_EVENT,
                        new EventData(SlotMachineEvent.BeginTurn));

            //同步到游戏最后的金额
            MainBlackboardController.Instance.SyncMyTempCreditToReal(true);
        }

        /// <summary>
        /// 开始Spin
        /// </summary>
        /// <remarks>
        /// * SlotMachine开始转动事件<br/>
        /// * Spin开始事件<br/>
        /// * 显示玩家金币（减去压注金额）<br/>
        /// </remarks>
        public void BeginSpin()
        {

            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_SLOT_EVENT,
                new EventData(SlotMachineEvent.SpinSlotMachine));


            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_CONTENT_EVENT,
                        new EventData(SlotMachineEvent.BeginSpin));

            //显示玩家金币（减去压注）
            MainBlackboardController.Instance.SyncMyUICreditToTemp();
        }


        public void BeginBonus(string name)
        {
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_CONTENT_EVENT,
                new EventData<string>(SlotMachineEvent.BeginBonus, name));
        }

        public void EndBonus(string name)
        {

            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_CONTENT_EVENT,
                new EventData<string>(SlotMachineEvent.EndBonus, name));
        }


        public void BeginBonusFreeSpin() => BeginBonus("FreeSpin");
        public void EndBonusFreeSpin() => EndBonus("FreeSpin");


        public void BeginBonusFreeSpinAdd() => BeginBonus("FreeSpinAdd");
        public void EndBonusFreeSpinAdd() => EndBonus("FreeSpinAdd");

        public void BeginBonusJackpot(string name) => BeginBonus($"JP{name}");
        public void EndBonusJackpot(string name) => EndBonus($"JP{name}");

        public void BeginBonusMiniGame(string name) => BeginBonus($"MiniGame{name}");
        public void EndBonusMiniGame(string name) => EndBonus($"MiniGame{name}");

        #endregion



        #region 获取图标位置

        public Vector2 GetVisibleSymbolCenterWordPos(int colIndex, int rowIndex)
        {
            /*
            GComponent goSymbol = GetVisibleSymbolFromDeck(colIndex, rowIndex).gOwnerSymbol;

            Vector2 centerlocalPos = new Vector2(goSymbol.width / 2, goSymbol.height / 2);
        
            Vector2 worldPos = goSymbol.LocalToGlobal(centerlocalPos);
        
            // Vector2 worldPos02 = new Vector2(worldPos.x - goSymbol.pivotX * goSymbol.width,  worldPos.y - goSymbol.pivotY * goSymbol.height); 
        
            return worldPos; // worldPos02
            */
            
            float centerlocalPosX = colIndex * customModel.symbolWidth + customModel.symbolWidth / 2;  // + gapX * colIndex
            
            float centerlocalPosY =  rowIndex * customModel.symbolWidth + customModel.symbolHeight / 2;  // + gapY * rowIndex
            
            Vector2 worldPos = goReels.LocalToGlobal(new Vector2(centerlocalPosX, centerlocalPosY));
            
            return worldPos;
        }
    
        public Vector2 SymbolCenterToNodeLocalPos(int colIndex, int rowIndex, GComponent toNode)
        {
            // 左上角为0,0

            float centerlocalPosX = colIndex * customModel.symbolWidth + customModel.symbolWidth / 2;  // + gapX * colIndex
            
            float centerlocalPosY =  rowIndex * customModel.symbolWidth + customModel.symbolHeight / 2;  // + gapY * rowIndex
            
            Vector2 worldPos = goReels.LocalToGlobal(new Vector2(centerlocalPosX, centerlocalPosY));
            
            Vector2  localPos = toNode.GlobalToLocal(worldPos);
            
            //return localPos;
            return new Vector2(localPos.x - goReels.pivotX * goReels.width, localPos.y - goReels.pivotY * goReels.height);

            /*
            GComponent goSymbol = GetVisibleSymbolFromDeck(colIndex, rowIndex).gOwnerSymbol;

            Vector2 centerlocalPos = new Vector2(goSymbol.width / 2, goSymbol.height / 2);
        
            Vector2 worldPos = goSymbol.LocalToGlobal(centerlocalPos);
            
             Vector2  localPos = toNode.GlobalToLocal(worldPos);
            return localPos;
            //return new Vector2(localPos.x - goSymbol.pivotX * goSymbol.width, localPos.y - goSymbol.pivotY * goSymbol.height);
            */
        }


        #endregion




        #region 【新增方法】显示图标特效

        public List<SymbolBase> GetSymbol(List<int> symbolNumbers)
        {
            List <SymbolBase> symbols = new List<SymbolBase>();
            for (int r = bufferTop; r <row + bufferTop; r++)
            {
                for (int c = 0; c < column; c++)
                {
                    ReelBase reel = reels[c];
                    SymbolBase symbol = reel.symbolList[r];
                    if (symbolNumbers.Contains(symbol.number))
                        symbols.Add(symbol);           
                }
            }       
            return symbols; 
        }
        public List<SymbolBase> GetSymbol(SymbolWin symbolWin)
        {
            List<SymbolBase> symbols = new List<SymbolBase>();
            foreach (Cell cel in symbolWin.cells)
            {
                symbols.Add(GetVisibleSymbolFromDeck(cel.column, cel.row));
            }
            return symbols;
        }

        public List<SymbolBase> GetSymbol(List<Cell> cells)
        {
            List<SymbolBase> symbols = new List<SymbolBase>();
            foreach (Cell cel in cells)
            {
                symbols.Add(GetVisibleSymbolFromDeck(cel.column, cel.row));
            }
            return symbols;
        }

        public  void ShowSymbolEffect(TagPoolObject tp, List<int> symbolNumbers,  bool isAmin,int symbolNumber,  bool isUseMySelfSymbolNumber)
            => ShowSymbolEffect(tp, GetSymbol(symbolNumbers), isAmin, symbolNumber, isUseMySelfSymbolNumber);

        public  void ShowSymbolEffect(TagPoolObject tp, SymbolWin symbolWin, bool isAmin, int symbolNumber, bool isUseMySelfSymbolNumber)
            => ShowSymbolEffect(tp, GetSymbol(symbolWin), isAmin, symbolWin.symbolNumber,isUseMySelfSymbolNumber);

        public void ShowSymbolEffect(TagPoolObject tp, List<Cell> cells, bool isAmin, int symbolNumber, bool isUseMySelfSymbolNumber)
            => ShowSymbolEffect(tp, GetSymbol(cells), isAmin, symbolNumber, isUseMySelfSymbolNumber);

        public virtual void ShowSymbolEffect(TagPoolObject tp, List<SymbolBase> symbols,  bool isAmin,int symbolNumber, bool isUseMySelfSymbolNumber)
        {

            foreach (SymbolBase symbol in symbols)
            {
                GComponent goSymbol = symbol.goOwnerSymbol;

                int symNumber = isUseMySelfSymbolNumber ? symbol.number : symbolNumber;

                if (tp == TagPoolObject.SymbolHit)
                {
                    string symbolName = customModel.symbolHitEffect[$"{symNumber}"];

                    // 图标动画
                    GComponent goSymbolHit = fguiPoolHelper.GetObject(TagPoolObject.SymbolHit, symbolName).asCom;
                    symbol.AddSymbolEffect(goSymbolHit, isAmin);

                    // 设置层级
                    FguiSortingOrderManager.Instance.ChangeSortingOrder(symbol.goOwnerSymbol, goExpectation);
                }
                else if (tp == TagPoolObject.SymbolAppear)
                {
                    string symbolName = customModel.symbolAppearEffect[$"{symNumber}"];

                    // 图标动画
                    GComponent goSymbolHit = fguiPoolHelper.GetObject(TagPoolObject.SymbolAppear, symbolName).asCom;
                    symbol.AddSymbolEffect(goSymbolHit, isAmin);

                    // 设置层级
                    FguiSortingOrderManager.Instance.ChangeSortingOrder(symbol.goOwnerSymbol, goExpectation);
                }
            }
        }

        #endregion









        #region 遮罩层

        /// <summary> 关闭遮罩层 </summary>
        public void CloseSlotCover() => SetSlotCover(false);

        /// <summary> 打开遮罩层 </summary>
        public void OpenSlotCover() => SetSlotCover(true);

        public virtual void SetSlotCover(bool isShow)
        {
            if (goSlotCover != null)
                goSlotCover.visible = isShow;
        }

        #endregion







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




        /*
        Func<GObject, bool> where = (gobj) =>
        {
            if (gobj == null || string.IsNullOrEmpty((string)gobj.data)) return false;
            string dataStr = ((string)gobj.data);
            foreach (string item in exclude)
            {
                if (dataStr.Contains(item))
                {
                    return false;
                }
            }
            return dataStr.Contains(FguiPool.tgPrefix);
        };
        List<GObject> items = FguiUtils.GetAllNode(root, where);
         */
        public List<SymbolBase> GetHasEffectSymbols(string[] includes)// {"symbol_appear#"}
        {
            List<string> includeLst = new List<string>(includes);
            List<SymbolBase> symbols = new List<SymbolBase>();
            for (int r = bufferTop; r < row + bufferTop; r++)
            {
                for (int c = 0; c < column; c++)
                {
                    ReelBase reel = reels[c];
                    SymbolBase symbol = reel.symbolList[r];

                    GObject[]  chds = symbol.goOwnerSymbol.GetChild("animator").asCom.GetChildren();
                    for (int i=0; i<chds.Length;i++)
                    {
                        GObject gobj = chds[i];
                        //DebugUtils.Log($"name = {gobj.name} {(string)gobj.data}"); // symbol_appear#pool_prefab:1001#
                        if (string.IsNullOrEmpty((string)gobj.data))
                            continue;
                        string dataStr = (string)gobj.data;
                        foreach (string item in includes)
                        {
                            if (dataStr.Contains(item))
                            {
                                symbols.Add(symbol);
                                break;
                            }
                        }
                    }
                }
            }
            return symbols;
        }








        #region 使用配置表显示赢线


        protected bool isSymbolAnim => _spinWEMD.Instance.isTwinkle ? false : _spinWEMD.Instance.isSymbolAnim;


        /// <summary>
        /// 显示单条或多条赢线
        /// </summary>
        /// <param name="symbolWin"></param>
        /// <param name="isUseMySelfSymbolNumber"></param>
        public virtual void ShowSymbolWinDeck(SymbolWin symbolWin, bool isUseMySelfSymbolNumber)
        {
            //停止特效显示
            SkipWinLine(false);

            //显示遮罩
            SetSlotCover(_spinWEMD.Instance.isShowCover);

            foreach (Cell cel in symbolWin.cells)
            {
                SymbolBase symble = GetVisibleSymbolFromDeck(cel.column, cel.row);

                int symbolNumber = isUseMySelfSymbolNumber ? symble.number : symbolWin.symbolNumber;

                string symbolName = customModel.symbolHitEffect[$"{symbolNumber}"];  // wild  or symbol;

                // 图标动画
                GComponent goSymbolHit = fguiPoolHelper.GetObject(TagPoolObject.SymbolHit, symbolName).asCom;
                symble.AddSymbolEffect(goSymbolHit, isSymbolAnim);


                // 设置层级
                FguiSortingOrderManager.Instance.ChangeSortingOrder(symble.goOwnerSymbol, goExpectation); //goPayLines


                // 边框
                if (_spinWEMD.Instance.isFrame)
                {
                    string borderEffect = customModel.borderEffect;
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


        /// <summary>
        /// 显示单条或多条赢线，并发送事件
        /// </summary>
        /// <param name="symbolWin"></param>
        /// <param name="isUseMySelfSymbolNumber"></param>
        /// <param name="eventType"></param>
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







        /// <summary>
        /// 空闲模式下-轮流显示线
        /// </summary>
        /// <param name="winList"></param>
        /// <returns></returns>
        public IEnumerator ShowWinListAwayDuringIdle(List<SymbolWin> winList)
        {
            while (winList.Count > 0) //while (idx < winList.Count)
            {
                yield return ShowWinListBySetting(winList);
            }
        }



        /// <summary>
        /// 显示单条或多条赢线，发送事件，并延时等待
        /// </summary>
        /// <param name="symbolWin"></param>
        /// <param name="isUseMySelfSymbolNumber"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public virtual IEnumerator ShowSymbolWinBySetting(SymbolWin symbolWin, bool isUseMySelfSymbolNumber, SpinWinEvent eventType)
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

                int symbolNumber = isUseMySelfSymbolNumber ? symble.number : symbolWin.symbolNumber;

                string symbolName = customModel.symbolHitEffect[$"{symbolNumber}"];  // wild  or symbol;

                // 图标动画  
                GComponent goSymbolHit = fguiPoolHelper.GetObject(TagPoolObject.SymbolHit, symbolName).asCom;
                symble.AddSymbolEffect(goSymbolHit, isSymbolAnim);

                // 设置层级
                FguiSortingOrderManager.Instance.ChangeSortingOrder(symble.goOwnerSymbol, goExpectation); //goPayLines

                // 边框
                if (_spinWEMD.Instance.isFrame)
                {
                    string borderEffect = customModel.borderEffect;
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





        /// <summary>
        /// 轮播显示单条赢线 或 显示所有赢线，发送事件，并延时等待
        /// </summary>
        /// <param name="winList"></param>
        /// <returns></returns>
        public virtual IEnumerator ShowWinListBySetting(List<SymbolWin> winList)
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







        #endregion
    }
}

