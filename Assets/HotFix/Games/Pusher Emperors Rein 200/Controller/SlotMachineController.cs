using System.Collections;
using System.Collections.Generic;
using GameMaker;
using FairyGUI;
//using _customMD = PusherEmperorsRein.CustomModel;
using _spinWEMD = SlotMaker.SpinWinEffectSettingModel;
using _reelSetMD = SlotMaker.ReelSettingModel;
using UnityEngine;
using SlotMaker;
using System;

namespace PusherEmperorsRein
{

    public enum SpinWinEvent
    {
        None,
        TotalWinLine,
        SingleWinLine,
    }

    public partial class SlotMachineController : SlotMachineBaseController
    {
        /// <summary> the anchor for "symbol hit" or "symbol appear"</summary>
  
        public  void Init(GComponent gSlotCover, GComponent gPayLines,GComponent gReels, GComponent gExpectation,FguiPoolHelper fguiPoolHelper, FguiGObjectPoolHelper gObjectPoolHelper)
        {
            base.Init(CustomModel.Instance, gSlotCover, gPayLines,gReels,fguiPoolHelper, gObjectPoolHelper);
            goExpectation = gExpectation;

            this.column = CustomModel.Instance.column;
            this.row = CustomModel.Instance.row;

            Transform tfmReels = transform.Find("Reels");
            reels = new List<ReelBase>();
            for (int i = 0; i < this.column; i++) {
                Reel reel = tfmReels.GetChild(i).GetComponent<Reel>();
                reel.reelIndex = i;
                reels.Add(reel);

                reel.Init(CustomModel.Instance, goReels.GetChildAt(i).asCom, gExpectation);
            }

            //gPayLines.visible = false;
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
                SymbolBase symble = GetVisibleSymbolFromDeck(item.colIdx+1, item.rowIdx+1);
                string symbolName = CustomModel.Instance.symbolAppearEffect[$"{item.symbolNumber}"];  // wild  or symbol;

                // 图标动画  
                GComponent goSymbolAppear = fguiPoolHelper.GetObject(TagPoolObject.SymbolAppear, symbolName).asCom;
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



        public IEnumerator ShowSymbolBonusBySetting_LaBa (List<BonusWin> bonusWin, bool isUseMySelfSymbolNumber)
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

                reels[reelIdx].ReelSetEndImmediately( // reels[reelIdx].ReelToStopOrTurnOnce(
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



#if false

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

                BaseSymbol symble = GetVisibleSymbolFromDeck(cel.column, cel.row);

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


        public bool CheckHasSymbolChange(SymbolWin curSymbolWin)
        {
            List<Cell> cells = curSymbolWin.cells;
            for (int i = 0; i < cells.Count; i++)
            {
                Cell cel = cells[i];
                BaseSymbol symble = GetVisibleSymbolFromDeck(cel.column, cel.row);
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


#endif



    }
}