using SimpleJSON;
using SlotMaker;
using System.Collections;
using System.Collections.Generic;
using FairyGUI;
using PusherEmperorsRein;
using UnityEngine;
using UnityEngine.Events;
using GameMaker;
using System.Linq;

public class SlotMachineHelper:MonoBehaviour
{

    FguiPoolHelper fguiPoolHelper;


    JSONNode config;
    GComponent slotMachine,goSlotCover, goPayLines ,goReels, goExpectation;
    

    List<GComponent> anchorSymbolsLst;
    private List<List<GComponent>> symbolsColRow;
    protected bool isSymbolAppearEffectWhenReelStop;
    List<bool> isReelPointeringLst;
    List<ReelState> reelStateLst;
    List<GTweener> reelTweenLst;
    List<Coroutine> corReelTurnLst;
    List<Coroutine> corReelToStopLst;



    List<int> curRollTimeLst;
    List<int> needRollTimeLst;

    List<List<int>> deckColRowNumber;

    List<UnityAction> reelStopCallbackLst;

    List<List<int>> reelsResult;


    string curReelSetting = "regular";
    string curWinEffectSetting = "default";

    
    
    private Dictionary<GComponent, Transition> transitionBiggerLst = new Dictionary<GComponent, Transition>();
    private Dictionary<GComponent, Transition> transitionTwinkleLst = new Dictionary<GComponent, Transition>();
    
    public void Init(string configStr, GComponent slotMachine, GComponent gExpectation, FguiPoolHelper fguiPoolHelper)
    {
        if (string.IsNullOrEmpty(maskSortOrder))
            maskSortOrder = $"MASK_SORT_ORDER-{Time.unscaledTime}-{UnityEngine.Random.Range(0,100)}";
                
        config = JSONNode.Parse(configStr);

        this.fguiPoolHelper = fguiPoolHelper;

        this.goExpectation = gExpectation;
        this.slotMachine = slotMachine;
        goReels = slotMachine.GetChild("reels").asCom;
        goSlotCover = slotMachine.GetChild("slotCover").asCom;
        goPayLines = slotMachine.GetChild("playLines").asCom;


        goSlotCover.visible = false;
        goPayLines.visible = true;


        anchorSymbolsLst = new List<GComponent>();
        isReelPointeringLst = new List<bool>();
        reelStateLst = new List<ReelState>();
        reelTweenLst = new List<GTweener>();
        corReelTurnLst = new List<Coroutine>();
        corReelToStopLst = new List<Coroutine>();
        curRollTimeLst  = new List<int>();
        needRollTimeLst = new List<int>();
        reelStopCallbackLst = new List<UnityAction>();


        for (int i=0; i< Column; i++ )
        {
            GComponent goReel = goReels.GetChild($"reel{i+1}").asCom;
            anchorSymbolsLst.Add(goReel.GetChild("symbols").asCom);

            reelTweenLst.Add(null);
            corReelTurnLst.Add(null);
            corReelToStopLst.Add(null);
            reelStopCallbackLst.Add(null);
            curRollTimeLst.Add(0);
            needRollTimeLst.Add(0);

            reelStateLst.Add(ReelState.Idle);

            isReelPointeringLst.Add(false);
            int index = i;
            goReel.onRollOver.Clear();
            goReel.onRollOver.Add(() => { OnSymbolPointerEnter(index); });
            goReel.onRollOut.Clear();
            goReel.onRollOut.Add(() => { OnSymbolPointerExit(index); });
        }

        
        symbolsColRow = new List<List<GComponent>>();
        deckColRowNumber = new List<List<int>>();        
        for (int c = 0; c < anchorSymbolsLst.Count; c++)
        {
            List<GComponent> reelSymbols = new List<GComponent>();
            List<int> reelDeck = new List<int>();
            for (int r = 0; r < anchorSymbolsLst[c].numChildren; r++)
            {
                GComponent goSymbol = anchorSymbolsLst[c].GetChildAt(r).asCom;
                reelSymbols.Add(goSymbol);
                int symbolNumber = SymbolNumbers[UnityEngine.Random.Range(1, SymbolCount)];       
                
                SetSymbolIcon(goSymbol, symbolNumber);
                reelDeck.Add(symbolNumber);
            }
            symbolsColRow.Add(reelSymbols);
            deckColRowNumber.Add(reelDeck);
        }


        GObject[] playLines = goPayLines.GetChildren();
        foreach(GObject line in playLines)
        {
            line.visible = false;
        }
        
        if (fguiPoolHelper!= null &&  isInitPool == false)
        {
            isInitPool = true;
            
            fguiPoolHelper.Add(TagPoolObject.SymbolHit, 
                GetSymbolHitEffect().Values.ToList(),"symbol_hit");
            fguiPoolHelper.PreLoad(TagPoolObject.SymbolHit );
            fguiPoolHelper.Add(TagPoolObject.SymbolAppear,
               GetSymbolAppearEffect().Values.ToList(), "symbol_appear");
            fguiPoolHelper.PreLoad(TagPoolObject.SymbolAppear);
        }
    }

    private bool isInitPool = false;
    
    #region Config

    int Row => (int)config["custom"]["row"];
    int Column => (int)config["custom"]["column"];
    public float ReelMaxOffsetY  => (int)config["custom"]["symbol_height"] * Row;
    int DeckUpStartIndex => 1;
    int DeckUpEndIndex => (int)config["custom"]["row"];
    int DeckDownStartIndex => DeckUpEndIndex + 1;
    int DeckDownEndIndex => 2* DeckUpEndIndex;
    
    int SymbolCount => config["custom"]["symbol_number"].Count;

    List<int> _symbolNumbers = null;
    List<int> SymbolNumbers
    {
        get
        {
            if (_symbolNumbers == null)
            {
                _symbolNumbers = new List<int>();
                foreach (JSONNode node in config["custom"]["symbol_number"])
                {
                    _symbolNumbers.Add((int)node);
                }
            }
            return _symbolNumbers;
        }
    }

    string BorderEffect => (string)config["custom"]["border_effect"];

    JSONNode GetReelSettingValue(int reelIdx, string key)
    {
        string nodeName = "reel_setting";
        JSONNode target = (config[nodeName].HasKey(curReelSetting) 
                           && config[nodeName][curReelSetting].HasKey($"reel{reelIdx + 1}")) ?
          config[nodeName][curReelSetting][$"reel{reelIdx + 1}"] : config[nodeName][curReelSetting]["default"];

        if (target.HasKey(key))
            return target[key];
        else
            return config[nodeName][curReelSetting]["default"][key];
    }
    float GetTimeReboundStart(int reelIdx) => (float)GetReelSettingValue(reelIdx, "time_rebound_start");
    int GetOffsetYReboundStart(int reelIdx) => (int)GetReelSettingValue(reelIdx, "offset_y_rebound_start");

    float GetTimeTurnOnce(int reelIdx) => (float)GetReelSettingValue(reelIdx, "time_turn_once");
    float GetTimeReboundEnd(int reelIdx) => (float)GetReelSettingValue(reelIdx, "time_rebound_end");

    int GetOffsetYReboundEnd(int reelIdx) => (int)GetReelSettingValue(reelIdx, "offset_y_rebound_end");
    int GetNumReelTurn(int reelIdx) => (int)GetReelSettingValue(reelIdx, "num_reel_turn");
    int GetNumReelTurnGap(int reelIdx) => (int)GetReelSettingValue(reelIdx, "num_reel_turn_gap");

    float GetTimeTurnStartDelay(int reelIdx) => (int)GetReelSettingValue(reelIdx, "time_turn_start_delay");

    Dictionary<string , string> GetSymbolAppearEffect()
    {
        Dictionary<string, string> dic = new Dictionary<string , string>();
        foreach (KeyValuePair<string,JSONNode>  kv in config["custom"]["symbol_appear_effect"])
        {
            dic.Add(kv.Key, (string)kv.Value);
        }
        return dic;
    }
    Dictionary<string, string> GetSymbolHitEffect()
    {
        Dictionary<string, string> dic = new Dictionary<string, string>();
        foreach (KeyValuePair<string, JSONNode> kv in config["custom"]["symbol_hit_effect"])
        {
            dic.Add(kv.Key, (string)kv.Value);
        }
        return dic;
    }




    // bool isWinEffect  config["custom"][curWinEffectSetting]

    JSONNode GetWinEffectSettingValue(string key)
    {
        string nodeName = "win_effect_setting";
        JSONNode target = (config[nodeName].HasKey(curWinEffectSetting) 
                           && config[nodeName][curWinEffectSetting].HasKey(key)) ?
          config[nodeName][curWinEffectSetting][key] : config[nodeName]["default"][key];
        
        return target;
    }

    public bool IsWEFrame => (bool)GetWinEffectSettingValue("frame");
    public bool IsWEShowLine => (bool)GetWinEffectSettingValue("line");
    public bool IsWEBigger => (bool)GetWinEffectSettingValue("bigger");
    public bool IsWETwinkle => (bool)GetWinEffectSettingValue("twinkle");

    public bool IsWESymbolAnim => IsWETwinkle? false: (bool)GetWinEffectSettingValue("anim");
    public bool IsWETotalWinLine => (bool)GetWinEffectSettingValue("total_win_line");
    public bool IsWESingleWinLine => (bool)GetWinEffectSettingValue("single_win_line");
    public bool IsWECredit => (bool)GetWinEffectSettingValue("credit");
    public bool IsWEShowCover => (bool)GetWinEffectSettingValue("cover");
    public bool IsWEHideBaseSymbol => (bool)GetWinEffectSettingValue("hide_base_symbol");

    public bool IsWESkipAtStopImmediately => (bool)GetWinEffectSettingValue("skip_at_stop_immediately");
    public float WETimeS => (float)GetWinEffectSettingValue("time_s");

    #endregion


    #region Reel
    /// <summary>
    /// 设置图标
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="symbolIdx"></param>
    void SetSymbolIcon(GComponent symbol, int symbolNumber)
    {
        string symbolUrl = config["custom"]["symbol_icon"][$"{symbolNumber}"];
        symbol.GetChild("animator").asCom.GetChild("image").asLoader.url = symbolUrl;
    }

    void OnSymbolPointerEnter(int reelIndex)
    {
        isReelPointeringLst[reelIndex] = true;
        ReelToStop(reelIndex);
    }
    void OnSymbolPointerExit(int reelIndex)
    {
        isReelPointeringLst[reelIndex] = false;
    }


    void ReelToStop(int reelIndex)
    {
        if (reelStateLst[reelIndex] == ReelState.StartTurn)
        {
            reelStateLst[reelIndex] = ReelState.StartStop;
            ClearReelTween(reelIndex);

            if (corReelToStopLst[reelIndex] != null)
                StopCoroutine(corReelToStopLst[reelIndex]);
            corReelToStopLst[reelIndex] = StartCoroutine(_ReelToStop(reelIndex));
        }
    }

    void ClearReelTween(int reelIndex)
    {
        ClearTween(reelIndex);
        ClearCorReel(reelIndex);
    }
    void ClearTween(int reelIndex)
    {
        if (reelTweenLst[reelIndex] != null)
            GTween.Kill(reelTweenLst[reelIndex]);
        reelTweenLst[reelIndex] = null;
    }


    void ClearCorReel(int reelIndex)
    {
        if (corReelToStopLst[reelIndex] != null) StopCoroutine(corReelToStopLst[reelIndex]);
        corReelToStopLst[reelIndex] = null;

        if (corReelTurnLst[reelIndex] != null) StopCoroutine(corReelTurnLst[reelIndex]);
        corReelTurnLst[reelIndex] = null;
    }



    /// <summary> 回弹效果（这里方向相反）</summary>
    IEnumerator Rebound(int reelIndex,  float yTo = 80, float durationS = 0.05f)
    {
        bool isNext = false; 

        reelTweenLst[reelIndex] = TweenUtils.DOLocalMoveY(anchorSymbolsLst[reelIndex], yTo, durationS, EaseType.Linear, () =>
        {
            reelTweenLst[reelIndex] = TweenUtils.DOLocalMoveY(anchorSymbolsLst[reelIndex], 0, durationS, EaseType.Linear, () =>
            {
                isNext = true;
            });
        });
        
        yield return new WaitUntil(() => isNext == true);
        isNext = false;
        reelTweenLst[reelIndex] = null;
    }

    IEnumerator _ReelTurn(int reelIndex, bool isOnce = false)
    {
        if (needRollTimeLst[reelIndex] == 0) yield break;

        bool isNext = false;
        reelStateLst[reelIndex] = ReelState.StartTurn;

        if (GetTimeReboundStart(reelIndex) > 0)
        {
            yield return Rebound(reelIndex,
                GetOffsetYReboundStart(reelIndex),
                GetTimeReboundStart(reelIndex)
            );
        }

        while (curRollTimeLst[reelIndex] < needRollTimeLst[reelIndex])
        {
            ResetIconData(reelIndex);

            if (curRollTimeLst[reelIndex] == needRollTimeLst[reelIndex] - 1)
            {
                reelStateLst[reelIndex] = ReelState.StartStop;
                //这里开始设置结果
                SetRollEndResult(reelIndex);
            }

            reelTweenLst[reelIndex] = TweenUtils.DOLocalMoveY(anchorSymbolsLst[reelIndex], 0,
                GetTimeTurnOnce(reelIndex), EaseType.Linear, () => { isNext = true; });

            yield return new WaitUntil(() => isNext);
            isNext = false;

            //if (reelTweenLst[reelIndex] != null) GTween.Kill(reelTweenLst[reelIndex]);
            reelTweenLst[reelIndex] = null;

            if (++curRollTimeLst[reelIndex] >= needRollTimeLst[reelIndex])
            {
                break;
            }
        }

        if (GetTimeReboundEnd(reelIndex) > 0)
        {
            yield return Rebound(
                reelIndex,
                GetOffsetYReboundEnd(reelIndex),
                GetTimeReboundEnd(reelIndex)
            );
        }
        reelStateLst[reelIndex] = ReelState.EndStop;
        if (reelIndex < reelStopCallbackLst.Count) reelStopCallbackLst[reelIndex]?.Invoke();
    }


    /// <summary> 修改滚轮图标 </summary>
    private void ResetIconData(int reelIndex)
    {
        for (int i = DeckDownStartIndex; i <= DeckDownEndIndex; i++)
        {
            int symbolNumber = deckColRowNumber[reelIndex][i - Row];
            SetSymbolIcon(symbolsColRow[reelIndex][i].asCom, symbolNumber);
            deckColRowNumber[reelIndex][i] = symbolNumber;
            //symbolList[i].SetBtnInteractableState(true);
        }

        anchorSymbolsLst[reelIndex].y = - ReelMaxOffsetY;  // 拉上去 (这里的方向和ugui是相反的)

        for (int i = DeckUpStartIndex; i <= DeckUpEndIndex; i++)
        {
            int symbolNumber = SymbolNumbers[UnityEngine.Random.Range(0, SymbolCount)];      
            SetSymbolIcon(symbolsColRow[reelIndex][i].asCom, symbolNumber); 
            deckColRowNumber[reelIndex][i] = symbolNumber;
            //symbolList[i].SetBtnInteractableState(true);
        }
    }

    int GetSymbolIndex(int symbolNumber) => SymbolNumbers.IndexOf(symbolNumber);

    int GetSymbolNumber(int symbolIndex) => SymbolNumbers[symbolIndex];
    /// <summary> 设置最终结果 </summary>
    private void SetRollEndResult(int reelIndex)
    {
        for (int i = DeckUpStartIndex; i <= DeckUpEndIndex; i++)
        {
            int symbolNumber = reelsResult[reelIndex][i- DeckUpStartIndex];
            SetSymbolIcon(symbolsColRow[reelIndex][i].asCom, symbolNumber);
            deckColRowNumber[reelIndex][i] = symbolNumber;
        }
    }

    IEnumerator _ReelToStop(int reelIndex)
    {
        bool isNext = false;
        reelStateLst[reelIndex] = ReelState.StartStop;
        SetRollEndResult(reelIndex);

        reelTweenLst[reelIndex] = TweenUtils.DOLocalMoveY(anchorSymbolsLst[reelIndex], 0, 
            GetTimeTurnOnce(reelIndex), EaseType.Linear, () => { isNext = true; });

        yield return new WaitUntil(() => isNext);
        isNext = false;
        reelTweenLst[reelIndex] = null;

        if (GetTimeReboundEnd(reelIndex) > 0)
        {
            yield return Rebound(
                reelIndex,
                GetOffsetYReboundEnd(reelIndex),
                GetTimeReboundEnd(reelIndex)
            );
        }

        reelStateLst[reelIndex] = ReelState.EndStop;
        if(reelIndex < reelStopCallbackLst.Count) reelStopCallbackLst[reelIndex]?.Invoke();
    }


    void SetReelDeck(int reelIndex)
    {
        for (int i = DeckDownStartIndex; i <= DeckDownEndIndex; i++)
        {
            int symbolNumber = deckColRowNumber[reelIndex][i - Row];
            SetSymbolIcon(symbolsColRow[reelIndex][i].asCom, symbolNumber);
            deckColRowNumber[reelIndex][i] = symbolNumber;
            // symbolList[i].SetBtnInteractableState(true);
        }
        //这里开始设置结果
        SetRollEndResult(reelIndex);
        anchorSymbolsLst[reelIndex].y = 0;
    }


    void StartTurn(int reelIndex, int targetRollTime, UnityAction reelStopCallback)
    {
        this.needRollTimeLst[reelIndex] = isReelPointeringLst[reelIndex] ? 1 : targetRollTime;
        this.curRollTimeLst[reelIndex] = 0;
        this.reelStopCallbackLst[reelIndex] = reelStopCallback;

        ClearReelTween(reelIndex);
        if (corReelTurnLst[reelIndex] != null) StopCoroutine(corReelTurnLst[reelIndex]);
        corReelTurnLst[reelIndex] = StartCoroutine(_ReelTurn(reelIndex));
    }


    public  void ReelToStopOrTurnOnce(int reelIndex, UnityAction action = null)
    {
        this.reelStopCallbackLst[reelIndex] = action;

        if (reelStateLst[reelIndex] == ReelState.StartStop) 
            return;

        if (reelStateLst[reelIndex] == ReelState.EndStop)  
            return;


        if (reelStateLst[reelIndex] == ReelState.Idle)
        {
            StartTurn(reelIndex, 1, action);
        }
        else if (reelStateLst[reelIndex] == ReelState.StartTurn)
        {
            ReelToStop(reelIndex);
        }
    }

    public virtual void HideBaseSymbolIcon(GComponent goSymbol, bool isHide)
    {
        goSymbol.GetChild("animator").asCom.GetChild("image").visible = !isHide;
    }

    public virtual GComponent AddSymbolEffect(GComponent goSymbol,GComponent anchorSymbolEffect, bool isAmin = true)
    {
        /*
        Animator animatorSpine = null;  //【待完成】  获取Spine的
        if (animatorSpine != null)
        {
            if (isAmin)
                animatorSpine.speed = 1f;  // 播放
            else
                animatorSpine.speed = 0f;  //暂停
        }*/

        GComponent goAnimator = goSymbol.GetChild("animator").asCom;
        goAnimator.AddChild(anchorSymbolEffect);  
        anchorSymbolEffect.xy = new Vector2(goAnimator.width / 2, goAnimator.height / 2);

        // 是否隐藏原有图标
        if (IsWEHideBaseSymbol)
        {
            HideBaseSymbolIcon(goSymbol, true);
        }

        return anchorSymbolEffect;
        // 播放动画
    }



    /// <summary> 特殊 Symbol Effect </summary>
    public  void SymbolAppearEffect(int reelIndex)
    {

        for (int i = DeckUpStartIndex; i <= DeckUpEndIndex; i++)
        {

            string symbolNumber = $"{deckColRowNumber[reelIndex][i]}";

            Dictionary<string, string> symbolAppearEffect  = GetSymbolAppearEffect();
            DebugUtils.LogError(symbolNumber);
            bool isHashSymbolAppearNumber = symbolAppearEffect.ContainsKey(symbolNumber);   

            if (isHashSymbolAppearNumber)
            {
                string symbolName = symbolAppearEffect[symbolNumber];
                GComponent anchorSymbolEffect = fguiPoolHelper.GetObject(TagPoolObject.SymbolAppear, symbolName).asCom;
                GComponent goSymbol = symbolsColRow[reelIndex][i].asCom;
                AddSymbolEffect(goSymbol,anchorSymbolEffect);
                
                //FguiSortingOrderManager.Instance.ChangeSortingOrder(goSymbol, goExpectation, maskSortOrder); 
                
                int rowIndex = i;
                // 设置层级
                FguiSortingOrderManager.Instance.ChangeSortingOrder(goSymbol, goExpectation, maskSortOrder,null, 
                    (self) => rowIndex + DeckUpStartIndex); 
            }
        }



    }



    #endregion


    #region Symbol

    public virtual GComponent AddBorderEffect(GComponent goSymbol, GComponent anchorBorderEffect)
    {
        GComponent goAnimator = goSymbol.GetChild("animator").asCom;
        goAnimator.AddChild(anchorBorderEffect);  //边长为1的点
        anchorBorderEffect.xy = new Vector2(goAnimator.width / 2, goAnimator.height / 2);
        // 播放动画
        return anchorBorderEffect;
    }
    
    public  void ShowBiggerEffect(GComponent goSymbol)
    {
        if(!transitionBiggerLst.ContainsKey(goSymbol)) 
            transitionBiggerLst.Add(goSymbol,null);
        transitionBiggerLst[goSymbol] = goSymbol.GetTransition("animBigger");
        transitionBiggerLst[goSymbol].Play();
    }

    public void ShowTwinkleEffect(GComponent goSymbol)
    {
        if(!transitionTwinkleLst.ContainsKey(goSymbol)) 
            transitionTwinkleLst.Add(goSymbol,null);
        transitionTwinkleLst[goSymbol] = goSymbol.GetTransition("animTwinkle");
        transitionTwinkleLst[goSymbol].Play();
    }


    public void StopSymbolEffect(GComponent goSymbol)
    {
        if(transitionBiggerLst.ContainsKey(goSymbol) && transitionBiggerLst[goSymbol] != null)
            transitionBiggerLst[goSymbol].Stop();
        transitionBiggerLst[goSymbol] = null;

        if(transitionTwinkleLst.ContainsKey(goSymbol) && transitionTwinkleLst[goSymbol] != null)
            transitionTwinkleLst[goSymbol].Stop();
        transitionTwinkleLst[goSymbol] = null;
    }

    #endregion


    #region SlotMachine

    public UnityEvent<EventData> onWinEvent;
    public UnityEvent<EventData> onSlotDetailEvent;
    public UnityEvent<EventData> onSlotEvent;
    public UnityEvent<EventData> onPrepareTotalWinCreditEvent;
    public UnityEvent<EventData> onTotalWinCreditEvent;

    public bool isStopImmediately = false;

    private string maskSortOrder = null;
    public int GetPayLineIndex(int payLineNumber) => payLineNumber - 1;
    public void SetReelsDeck(string strDeckRowCol = "1,1,1,1,1#2,2,2,2,2#3,3,3,3,3")
    {
        //停止特效显示
        SkipWinLine(false);

        reelsResult = SlotTool.GetDeckColRow02(strDeckRowCol);

        //这个还要判断特殊图标 如果有还需要改变滚轮滚的次数 还有特殊表现效果
        //模拟图标
        for (int col = 0; col < this.Column; col++)
        {
            SetReelDeck(col);
        }
    }

    public void SkipWinLine(bool isIncludeTag)
    {

        // 打开基础图标
        for (int c = 0; c < symbolsColRow.Count; c++)
        {
            for (int r = DeckUpStartIndex; r <= DeckUpEndIndex; r++)
            {
                StopSymbolEffect(symbolsColRow[c][r]);
                HideBaseSymbolIcon(symbolsColRow[c][r], false);
            }
        }
        
        // 去除层级功能
        FguiSortingOrderManager.Instance.ReturnSortingOrder(maskSortOrder);
        //FguiSortingOrderManager.Instance.ReturnAllSortingOrder();
        
        foreach (GComponent symbols in anchorSymbolsLst)
        {
            string[] exclude = isIncludeTag ? new string[] { } : new string[] { };

            fguiPoolHelper.ReturnAllToPool(symbols, exclude);
        }


        GObject[] payLines = goPayLines.GetChildren();
        // 关掉所有线
        foreach (GObject line in payLines)
        {
            line.visible = false;
        }

        onWinEvent?.Invoke(new EventData(SlotMachineEvent.SkipWinLine));
    }



    public SymbolWin GetTotalSymbolWin(List<SymbolWin> winList)
    {
        List<string> bsLst = new List<string>();

        long earnCredit = 0;
        List<Cell> cells = new List<Cell>();

        List<int> lineNumbers = new List<int>();

        foreach (SymbolWin sw in winList)
        {
            foreach (Cell cel in sw.cells)
            {
                int symbolNumber = deckColRowNumber[cel.column][cel.row + DeckUpStartIndex];
                string mark = $"{cel.column}-{cel.row}#{symbolNumber}";

                if (bsLst.Contains(mark))
                    continue;
                cells.Add(new Cell(cel.column, cel.row));
                bsLst.Add(mark);
            }

            // 获得所有赢线的线号
            if (!lineNumbers.Contains(sw.lineNumber))
                lineNumbers.Add(sw.lineNumber);

            earnCredit += sw.earnCredit;
        }

        TotalSymbolWin totalWin = new TotalSymbolWin()
        {
            lineNumbers = lineNumbers,
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



    public IEnumerator ShowSymbolWinBySetting(SymbolWin symbolWin, bool isUseMySelfSymbolNumber, SpinWinEvent eventType)
    {

        //停止特效显示
        SkipWinLine(false);


        // 立马停止时，不播放赢分环节？
        if (isStopImmediately && IsWESkipAtStopImmediately)
            yield break;

        //显示遮罩
        SetSlotCover(IsWEShowCover);

        Dictionary<string, string> symbolHitEffect =   GetSymbolHitEffect();

        foreach (Cell cel in symbolWin.cells)
        {

            int symbolNumberSelf = GetVisibleSymbolNumberFromDeck(cel.column, cel.row);

            int symbolNumber = isUseMySelfSymbolNumber ? symbolNumberSelf : symbolWin.symbolNumber;
            
            string symbolName =  symbolHitEffect[$"{symbolNumber}"];  // wild  or symbol;
            
            // 图标动画  
            GComponent goSymbolHit = fguiPoolHelper.GetObject(TagPoolObject.SymbolHit, symbolName).asCom;
            GComponent goSymbol = GetVisibleSymbolFromDeck(cel.column, cel.row);

            AddSymbolEffect(goSymbol,goSymbolHit, IsWESymbolAnim);

            int rowIndex = cel.row;
            // 设置层级
            FguiSortingOrderManager.Instance.ChangeSortingOrder(goSymbol, goExpectation, maskSortOrder,null, 
                (self) => rowIndex + DeckUpStartIndex); 

            // 边框
            if (IsWEFrame)
            {
                GComponent goBorderEffect = fguiPoolHelper.GetObject(TagPoolObject.SymbolBorder, BorderEffect).asCom;
                AddBorderEffect(goSymbol,goBorderEffect);
            }

            // 整体变大特效
            if (IsWETwinkle)
                ShowTwinkleEffect(goSymbol);            
            else if (IsWEBigger)
                ShowBiggerEffect(goSymbol);

        }


        // 是否显示线
        if (IsWEShowLine)
        {
            if (symbolWin is TotalSymbolWin)
            {
                TotalSymbolWin totalSymbolWin = symbolWin as TotalSymbolWin;

                foreach (int payLineNumber in totalSymbolWin.lineNumbers)
                {
                    int payLineIndex = GetPayLineIndex(payLineNumber);
                    if (payLineIndex >= 0 && payLineIndex < goPayLines.numChildren) 
                    {
                        goPayLines.GetChildAt(payLineIndex).visible = true;
                    }
                }
            }
            else
            {
                int paylineIndex = GetPayLineIndex(symbolWin.lineNumber);
                if ( paylineIndex>= 0 && paylineIndex < goPayLines.numChildren)
                {
                    goPayLines.GetChildAt(paylineIndex).visible = true;
                }
            }
        }


        // 事件
        if (eventType == SpinWinEvent.TotalWinLine)
        {
            onWinEvent?.Invoke(new EventData<SymbolWin>(SlotMachineEvent.TotalWinLine, symbolWin));
        }
        else if (eventType == SpinWinEvent.SingleWinLine)
        {
            onWinEvent?.Invoke(new EventData<SymbolWin>(SlotMachineEvent.SingleWinLine, symbolWin));
        }

        yield return SlotWaitForSeconds(WETimeS);
    }


    public IEnumerator ShowWinListBySetting(List<SymbolWin> winList)
    {

        // 立马停止时，不播放赢分环节？
        if (isStopImmediately && IsWESkipAtStopImmediately)
            yield break;

        if (IsWETotalWinLine)
        {
            yield return ShowSymbolWinBySetting(GetTotalSymbolWin(winList), true, SpinWinEvent.TotalWinLine);
        }
        else
        {
            int idx = 0;
            while (idx < winList.Count)
            {
                yield return ShowSymbolWinBySetting(winList[idx], true, SpinWinEvent.SingleWinLine);

                ++idx;

                // 立马停止时，不播放赢分环节？
                if (isStopImmediately && IsWESkipAtStopImmediately)
                    break;
            }
        }

        //关闭遮罩
        CloseSlotCover();

        //停止特效显示
        SkipWinLine(false);
    }


    public void ShowSymbolWinDeck(List<BonusWin> symbolWin, bool isUseMySelfSymbolNumber)
    {
        //停止特效显示
        SkipWinLine(false);

        //显示遮罩
        SetSlotCover(IsWEShowCover);

        Dictionary<string, string> symbolHitEffect = GetSymbolHitEffect();

        foreach (BonusWin item in symbolWin)
        {
            Cell cel = item.cell;
       
            int symbolNumberSelf = GetVisibleSymbolNumberFromDeck(cel.column, cel.row);

            int symbolNumber = isUseMySelfSymbolNumber ? symbolNumberSelf : item.symbolNumber;


            string symbolName = symbolHitEffect[$"{symbolNumber}"];  // wild  or symbol;

            // 图标动画  
            GComponent goSymbolHit = fguiPoolHelper.GetObject(TagPoolObject.SymbolHit, symbolName).asCom;
            GComponent goSymbol =   GetVisibleSymbolFromDeck(cel.column, cel.row);
            
            AddSymbolEffect(goSymbol, goSymbolHit, IsWESymbolAnim);

            
            int rowIndex = cel.row;
            // 设置层级
            FguiSortingOrderManager.Instance.ChangeSortingOrder(goSymbol, goExpectation, maskSortOrder,null,
                (self)=>rowIndex + DeckUpStartIndex);


            // 边框
            if (IsWEFrame)
            {
                GComponent goBorderEffect = fguiPoolHelper.GetObject(TagPoolObject.SymbolBorder, BorderEffect).asCom;
                AddBorderEffect(goSymbol, goBorderEffect);
            }

            // 整体变大特效
            if (IsWETwinkle)
                ShowTwinkleEffect(goSymbol);            
            else if (IsWEBigger)
                ShowBiggerEffect(goSymbol);

        }
    }

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


    public IEnumerator TurnReelsOnce(string strDeckRowCol = "1,1,1,1,1#2,2,6,2,2#3,3,3,3,3", UnityAction finishCallback = null)
    {

        SkipWinLine(false);

        reelsResult = SlotTool.GetDeckColRow02(strDeckRowCol);

        yield return ReelsToStopOrTurnOnce(null);
        // 算分

        finishCallback?.Invoke();
    }
    public IEnumerator ReelsToStopOrTurnOnce(UnityAction finishCallback)
    {

        int reelsCount = this.Column;

        bool isNext = false;

        for (int reelIdx = 0; reelIdx < this.Column; reelIdx++)
        {
            if (reelStateLst[reelIdx] == ReelState.EndStop)
            {
                reelsCount--;
                continue;
            }

            if (reelStateLst[reelIdx] == ReelState.Idle)
            {
                if (GetTimeTurnStartDelay(reelIdx) > 0)
                {
                    yield return new WaitForSeconds(GetTimeTurnStartDelay(reelIdx));
                }
            }

            int _reelIdx = reelIdx;

            ReelToStopOrTurnOnce(reelIdx,
                () =>
                {
                    onSlotDetailEvent?.Invoke(new EventData<int>(SlotMachineEvent.PrepareStoppedReel, _reelIdx));

                    if (isSymbolAppearEffectWhenReelStop)
                        SymbolAppearEffect(reelIdx);

                    if (--reelsCount <= 0)
                    {
                        isNext = true;
                    }
                }
            );
        }

        yield return new WaitUntil(() => isNext == true);
        isNext = false;


        for  (int i=0; i< reelStateLst.Count; i++)
        {
            reelStateLst[i] = ReelState.Idle;
        }

        onSlotEvent?.Invoke(new EventData(SlotMachineEvent.StoppedSlotMachine));

        finishCallback?.Invoke();
    }


    IEnumerator StartTurnReels()
    {

        int reelsCount = this.Column;

        bool isNext = false;

        for (int reelIdx = 0; reelIdx < this.Column; reelIdx++)
        {
            if (GetTimeTurnStartDelay(reelIdx) > 0)
            {
                yield return new WaitForSeconds(GetTimeTurnStartDelay(reelIdx));
            }

            int _reelIdx = reelIdx;

            StartTurn(
                reelIdx,
                GetNumReelTurn(reelIdx) + reelIdx * GetNumReelTurnGap(reelIdx),
                () =>
                {
                    onSlotDetailEvent?.Invoke(new EventData<int>(SlotMachineEvent.PrepareStoppedReel, _reelIdx));

                    if (isSymbolAppearEffectWhenReelStop)
                        SymbolAppearEffect(_reelIdx);

                    if (--reelsCount <= 0)
                    {
                        isNext = true;
                    }

                }
            );
        }

        yield return new WaitUntil(() => isNext == true);
        isNext = false;

        for (int i = 0; i < reelStateLst.Count; i++)
        {
            reelStateLst[i] = ReelState.Idle;
        }

        onSlotEvent?.Invoke(new EventData(SlotMachineEvent.StoppedSlotMachine));

    }


    public IEnumerator TurnReelsNormal(string strDeckRowCol = "1,1,1,1,1#2,2,6,2,2#3,3,3,3,3", UnityAction finishCallback = null)
    {
        //停止特效显示
        SkipWinLine(false);

        reelsResult = SlotTool.GetDeckColRow02(strDeckRowCol);


        yield return StartTurnReels();

        finishCallback?.Invoke();
    }

    UnityEvent<EventData> onContentEvent;
    public void BeginTurn() => onContentEvent?.Invoke(new EventData(SlotMachineEvent.BeginTurn));

    public void BeginSpin()
    {
        onSlotEvent?.Invoke(new EventData(SlotMachineEvent.SpinSlotMachine));
        onContentEvent?.Invoke(new EventData(SlotMachineEvent.BeginSpin));
    }


    public void SendPrepareTotalWinCreditEvent(long totalWinCredit, bool isAddToCredit = false)
    {
        PrepareTotalWinCredit res = new PrepareTotalWinCredit()
        {
            totalWinCredit = totalWinCredit,
            isAddToCredit = isAddToCredit,
        };
        onPrepareTotalWinCreditEvent?.Invoke(new EventData<PrepareTotalWinCredit>(SlotMachineEvent.PrepareTotalWinCredit, res));
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
        SendTotalWinCreditEvent( totalWinCredit);
    }
    public void SendTotalWinCreditEvent(long totalWinCredit)
    {
        onWinEvent?.Invoke(new EventData<long>(SlotMachineEvent.TotalWinCredit, totalWinCredit));
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

    public GComponent GetVisibleSymbolFromDeck(int colIndex, int rowIndex)
    {
        return symbolsColRow[colIndex][rowIndex + DeckUpStartIndex];
    }
    public int GetVisibleSymbolNumberFromDeck(int colIndex, int rowIndex)
    {
        return deckColRowNumber[colIndex][rowIndex + DeckUpStartIndex];
    }

    public void SelectWinEffectSetting(string key)
    {
        if (!config["win_effect_setting"].HasKey(key))
        {
            DebugUtils.LogError($"can not find key:\"{key}\" in  node \"win_effect_setting\"");
            return;
        }
       curWinEffectSetting = key; 
    } 
    
    public void SelectReelSetting(string key)
    {
        if (!config["reel_setting"].HasKey(key))
        {
            DebugUtils.LogError($"can not find key:\"{key}\" in  node \"reel_setting\"");
            return;
        }
        curReelSetting = key;
    }

    public Vector2 GetVisibleSymbolCenterWordPos(int colIndex, int rowIndex)
    {
        GComponent goSymbol = GetVisibleSymbolFromDeck(colIndex, rowIndex);

        Vector2 centerlocalPos = new Vector2(goSymbol.width / 2, goSymbol.height / 2);
        
        Vector2 worldPos = goSymbol.LocalToGlobal(centerlocalPos);
        
        // Vector2 worldPos02 = new Vector2(worldPos.x - goSymbol.pivotX * goSymbol.width,  worldPos.y - goSymbol.pivotY * goSymbol.height); 
        
        return worldPos; // worldPos02
    }
    
    public Vector2 SymbolCenterToNodeLocalPos(int colIndex, int rowIndex, GComponent toNode)
    {
        GComponent goSymbol = GetVisibleSymbolFromDeck(colIndex, rowIndex);

        Vector2 centerlocalPos = new Vector2(goSymbol.width / 2, goSymbol.height / 2);
        
        Vector2 worldPos = goSymbol.LocalToGlobal(centerlocalPos);
        
        Vector2  localPos = toNode.GlobalToLocal(worldPos);
        
        return new Vector2(localPos.x - goSymbol.pivotX * goSymbol.width,
            localPos.y - goSymbol.pivotY * goSymbol.height);
    }


    #endregion



    public void SendTotalBonusCreditEvent(long BonusCredit)
    {
        EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_WIN_EVENT,
            new EventData<long>(SlotMachineEvent.SingleWinBonus, BonusCredit));
    }
}
