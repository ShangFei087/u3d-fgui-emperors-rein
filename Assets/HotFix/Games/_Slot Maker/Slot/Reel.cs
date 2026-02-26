using System.Collections;
using System.Collections.Generic;
using PusherEmperorsRein;
using UnityEngine;
using UnityEngine.Events;
using FairyGUI;




namespace SlotMaker
{


    public class Reel : ReelBase
    {


        /// <summary> 已经滚动圈数 </summary>
        private int curRollTime = 0;
        /// <summary> 需要滚动圈数 </summary>
        private int needRollTime = 0;
        /// <summary> 滚轮停止回调 </summary>
        private UnityAction reelStopCallback = null;
        /// <summary> 滚轮显示结果 </summary>
        private List<int> columnResult = new List<int>() { 0, 0, 0 };


        /// <summary> 
        /// 被点中中 
        /// </summary>
        /// <remark> 
        /// * 被点中时只能跑一圈.
        /// </remark>
        protected bool isReelPointering = false;


        protected virtual int deckUpStartIndex => 2;
        protected virtual int deckUpEndIndex => customModel.row;
        protected virtual int deckDownStartIndex => customModel.row + 1;
        protected virtual int deckDownEndIndex => customModel.row + customModel.row;


        Coroutine coReelTurn = null;
        Coroutine coReelToStop = null;

        GTweener _curTweener;


        public override void Init(ICustomModel customModel,GComponent gReel, GComponent gExpectation)
        {
            if (goOwnerReel == gReel) return; // 避免同个对象重复初始化

            this.customModel = customModel;
            goExpectation = gExpectation;
            goOwnerReel = gReel;
            goSymbols = goOwnerReel.GetChild("symbols").asCom;
            symbolList = new List<SymbolBase>();
            for (int i = 0; i < goSymbols.numChildren; i++)
            {
                Symbol symbol = new Symbol();
                //##symbol.Init(goSymbols.GetChildAt(i).asCom);
                symbol.Init(customModel,goSymbols.GetChild($"symbol{i}").asCom);
                symbolList.Add(symbol);
            }

            for (int i = 0; i < symbolList.Count; i++)
            {
                symbolList[i].SetSymbolImage(customModel.symbolNumber[Random.Range(0, customModel.symbolCount)]);
            }

            goSymbols.y = 0;


            goOwnerReel.onRollOver.Clear();
            goOwnerReel.onRollOver.Add(OnSymbolPointerEnter);

            goOwnerReel.onRollOut.Clear();
            goOwnerReel.onRollOut.Add(OnSymbolPointerExit);

            isReelPointering = false;
        }



        public override void StartTurn(int targetRollTime, UnityAction reelStopCallback)
        {
            this.needRollTime = isReelPointering ? 1 : targetRollTime;
            this.curRollTime = 0;
            this.reelStopCallback = reelStopCallback;

            ClearReelTween();
            if (coReelTurn != null)StopCoroutine(coReelTurn);
            coReelTurn = StartCoroutine(_ReelTurn());
        }



        /// <summary> 修改滚轮图标 </summary>
        protected void ResetIconData(bool isUseStartRebound = false)
        {
            for (int i = deckDownStartIndex; i <= deckDownEndIndex; i++) 
            {
                int number = symbolList[i - customModel.row].GetSymbolNumber(); 
                symbolList[i].SetSymbolImage(number);
                symbolList[i].SetBtnInteractableState(true);
            }

            goSymbols.y = - customModel.reelMaxOffsetY;  // 拉上去 (这里的方向和ugui是相反的)

            for (int i = deckUpStartIndex; i <= deckUpEndIndex; i++) 
            {
                symbolList[i].SetSymbolImage(customModel.symbolNumber[Random.Range(0, customModel.symbolCount)]);
                symbolList[i].SetBtnInteractableState(true);
            }

            /*
            // 考虑到开始回弹时，看到了最顶第一行的图标
            if (isUseStartRebound)
            {
                int index = symbolList[0].GetSymbolIndex();
                symbolList[deckUpEndIndex].SetSymbolImage(index);
                symbolList[0].SetSymbolImage(Random.Range(0, customModel.symbolCount));
            } */
        }


        public override void SetReelDeck(string reelValue = null)
        {
            for (int i = deckDownStartIndex; i <= deckDownEndIndex; i++) 
            {
                int index = symbolList[i - customModel.row].GetSymbolNumber();
                symbolList[i].SetSymbolImage(index);
                symbolList[i].SetBtnInteractableState(true);
            }
            //这里开始设置结果
            SetRollEndResult();
            goSymbols.y = 0; 
        }


        protected IEnumerator _ReelTurn(bool isOnce = false)
        {
            if (needRollTime == 0)
                yield break;

            bool isNext = false;
            state = ReelState.StartTurn;


            if (ReelSettingModel.Instance.GetTimeReboundStart(reelIndex) > 0)
            {
                yield return Rebound(
                    ReelSettingModel.Instance.GetOffsetYReboundStart(reelIndex),
                    ReelSettingModel.Instance.GetTimeReboundStart(reelIndex)
                );
            }

            while (curRollTime < needRollTime)
            {
                ResetIconData();


                if (curRollTime == needRollTime - 1)
                {
                    state = ReelState.StartStop;
                    //这里开始设置结果
                    SetRollEndResult();
                }

                _curTweener = TweenUtils.DOLocalMoveY(goSymbols, 0, ReelSettingModel.Instance.GetTimeTurnOnce(reelIndex), EaseType.Linear, () => { isNext = true; });

                yield return new WaitUntil(() => isNext);
                isNext = false;

                //if (_curTweener != null) _curTweener.Kill();
                _curTweener = null;

                if (++curRollTime >= needRollTime)
                {
                    break;
                }
            }

            if (ReelSettingModel.Instance.GetTimeReboundEnd(reelIndex) > 0)
            {
                yield return Rebound(
                    ReelSettingModel.Instance.GetOffsetYReboundEnd(reelIndex),
                    ReelSettingModel.Instance.GetTimeReboundEnd(reelIndex)
                );
            }
            state = ReelState.EndStop;
            reelStopCallback?.Invoke();
        }


        public override void SetReelState(ReelState state = ReelState.Idle)
        {
            this.state = state;
        }

        /// <summary>
        /// 鼠标或手指，点击
        /// </summary>
        protected void OnSymbolPointerEnter()
        {
            //DebugUtils.Log($"【Touch Enter】i am reel({reelIndex})");
            isReelPointering = true;
            ReelToStop();
        }
        /// <summary>
        /// 鼠标或手指，不点击
        /// </summary>
        protected void OnSymbolPointerExit()
        {
            //DebugUtils.Log($"【Touch Exit】i am reel({reelIndex})");
            isReelPointering = false;
        }

        /// <summary>
        /// 停止滚动
        /// </summary>
        protected void ReelToStop()
        {
            if (state == ReelState.StartTurn)
            {
                state = ReelState.StartStop;
                ClearReelTween();

                if (coReelToStop != null) StopCoroutine(coReelToStop);
                coReelToStop = StartCoroutine(_ReelToStop());
            }
        }





        protected void ClearReelTween()
        {
            ClearTween();
            ClearCorReel();
        }
        protected void ClearTween()
        {
            if (_curTweener != null)
            {
                _curTweener.Kill();
                //Debug.LogError($"清除 tween {reelIndex}");
            }

            _curTweener = null;
        }


        protected void ClearCorReel()
        {
            if (coReelTurn != null) StopCoroutine(coReelTurn);
            coReelTurn = null;

            if (coReelToStop != null) StopCoroutine(coReelToStop);
            coReelToStop = null;
        }


        protected IEnumerator _ReelToStop()
        {
            bool isNext = false;
            state = ReelState.StartStop;
            SetRollEndResult();

            _curTweener = TweenUtils.DOLocalMoveY(goSymbols, 0, ReelSettingModel.Instance.GetTimeTurnOnce(reelIndex), EaseType.Linear, () => { isNext = true; });

            yield return new WaitUntil(() => isNext);
            isNext = false;
            _curTweener = null;

            if (ReelSettingModel.Instance.GetTimeReboundEnd(reelIndex) > 0)
            {
                yield return Rebound(
                    ReelSettingModel.Instance.GetOffsetYReboundEnd(reelIndex),
                    ReelSettingModel.Instance.GetTimeReboundEnd(reelIndex)
                );
            }

            state = ReelState.EndStop;

            reelStopCallback?.Invoke();

        }



        /// <summary>
        /// 滚轮滚动至少一次
        /// </summary>
        /// <param name="action"></param>
        /// <remarks>
        /// * 如果滚轮还没滚动，则正常滚动一次。<br/>
        /// * 如果滚轮已经在滚动，则里面停止。<br/>
        /// * 当滚轮已经停止，则直接退出，且不调用回调函数<br/>
        /// </remarks>
        public override void ReelToStopOrTurnOnce(UnityAction action = null)
        {
            if (action != null)
                this.reelStopCallback = action;

            if (state == ReelState.StartStop) //开始停止
                return;

            if (state == ReelState.EndStop) //已经停止
                return;


            if (state == ReelState.Idle)
            {
                StartTurn(1, action);
            }
            else if (state == ReelState.StartTurn)
            {
                ReelToStop();
            }
        }




        /// <summary>
        /// 滚轮立马停止滚动，显示最终结果
        /// </summary>
        public override void ReelSetEndImmediately(UnityAction action = null)
        {
            if (action != null)
                this.reelStopCallback = action;

            if (state != ReelState.EndStop)
            {
                //Debug.LogError($"i am here ReelSetEndImmediately {reelIndex}");

                ClearReelTween();

                state = ReelState.EndStop;

                SetRollEndResult();

                goSymbols.y = 0;

                reelStopCallback?.Invoke();
            }
        }



        /// <summary> 回弹效果（这里方向相反）</summary>
        public IEnumerator Rebound(float yTo = 80, float durationS = 0.05f)
        {

            bool isNext = false;
            _curTweener = TweenUtils.DOLocalMoveY(goSymbols, yTo, durationS, EaseType.Linear, () =>
            {
                _curTweener = TweenUtils.DOLocalMoveY(goSymbols, 0, durationS, EaseType.Linear, () =>
                {
                    //Debug.LogError($"调用 Rebound！！{yTo} {System.Enum.GetName(typeof(ReelState), state) } {reelIndex}");
                    isNext = true;
                });
            });
            yield return new WaitUntil(() => isNext == true);
            isNext = false;
            _curTweener = null;
        }



        /// <summary> 特殊 Symbol Effect </summary>
        public override void SymbolAppearEffect()
        {

            for (int i = deckUpStartIndex; i <= deckUpEndIndex; i++) 
            {

                SymbolBase symble = symbolList[i];

                string symbleNumber = $"{symble.number}";

                bool isHashSymbolAppearNumber = false;
                foreach (KeyValuePair<string, string> kv in customModel.symbolAppearEffect)
                {
                    if (kv.Key == symbleNumber)
                    {
                        isHashSymbolAppearNumber = true;
                        break;
                    }
                }

                if (isHashSymbolAppearNumber)
                {
                    string symbolName = customModel.symbolAppearEffect[symbleNumber];
                    GComponent anchorSymbolEffect = FguiPoolManager.Instance.GetObject(TagPoolObject.SymbolAppear, symbolName).asCom;
                    symble.AddSymbolEffect(anchorSymbolEffect);
                    
                    FguiSortingOrderManager.Instance.ChangeSortingOrder(symble.goOwnerSymbol, goExpectation); 
                    
                    /*
                    int rowIndex = i;
                    // 设置层级
                    FguiSortingOrderManager.Instance.ChangeSortingOrder(symble.goOwnerSymbol, goExpectation, null,null, 
                        (self) => rowIndex + deckUpStartIndex); 
                    */    
                }
            }
        }


        /// <summary> 设置停止图标 </summary>
        protected void SetRollEndResult()
        {
            int index = 0;
            for (int i = deckUpStartIndex; i < deckUpEndIndex; i++) 
            {
                symbolList[i].SetSymbolImage(columnResult[index]);
                index++;
            }
        }

        /// <summary> 游戏开始的时候设定最终显示的结果  </summary>
        public override void SetResult(List<int> result)
        {
            columnResult = result;
        }
    }
}