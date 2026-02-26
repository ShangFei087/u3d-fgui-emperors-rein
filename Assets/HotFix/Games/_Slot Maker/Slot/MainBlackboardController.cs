using GameMaker;
using PusherEmperorsRein;
using System.Collections;
using System;
using UnityEngine;
using SlotMaker;

public class MainBlackboardController : MonoSingleton<MainBlackboardController>
{

    /*
    private CorController _corCtrl;
    private CorController corCtrl
    {
        get
        {
            if (_corCtrl == null)
                _corCtrl = new CorController(this);
            return _corCtrl;
        }
    }
    public void ClearCo(string name) => corCtrl.ClearCo(name);

    public void DoCo(string name, IEnumerator routine) => corCtrl.DoCo(name, routine);

    public bool IsCo(string name) => corCtrl.IsCo(name);

    public IEnumerator DoTask(Action cb, int ms) => corCtrl.DoTask(cb, ms);



    const string COR_MY_CREDIT_AINM = "COR_MY_CREDIT_AINM";
    const string COR_AUTO_SYNC_MY_REAL_CREDIT = "COR_AUTO_SYNC_MY_REAL_CREDIT";
    */

    Coroutine coMyCreditAnim, coAutoSyncMyRealCredit;

    IEnumerator DelayTask(Action task, int timeMS)
    {
        yield return new WaitForSeconds((float)timeMS / 1000f);
        task?.Invoke();
    }
    void ClearCo(Coroutine co)
    {
        if (co != null)
            StopCoroutine(co);
        co = null;
    }



    /// <summary> 玩家当下金币  </summary>
    public long myTempCredit
    {
        get => MainModel.Instance.myCredit;
    }

    /// <summary> 玩家真实金币  </summary>
    public long myRealCredit
    {
        get => SBoxModel.Instance.myCredit;
    }

    /// <summary>
    /// 加钱动画标志位
    /// </summary>
    /// <param name="isAnim"></param>
    public bool SyncCreditAnim(bool isAnim)
    {
        /*
        if (isAnim)
            DoCo(COR_MY_CREDIT_AINM, DoTask(() => { }, 4000));
        else
            ClearCo(COR_MY_CREDIT_AINM);
        */

        ClearCo(coMyCreditAnim);

        if (isAnim)
        {
            coMyCreditAnim = StartCoroutine(DelayTask(() =>
            {
                coMyCreditAnim = null;
            }, 4000));
        }

        return isAnim;
    }


    /// <summary>
    /// 设置玩家真实金币
    /// </summary>
    /// <param name="crefit"></param>
    public void SetMyRealCredit(long crefit)
    {
        SBoxModel.Instance.myCredit = crefit;
    }

    /// <summary>
    /// 当下玩家金额减少
    /// </summary>
    /// <param name="credit">要减少的金额</param>
    /// <param name="isEvent">是否发送事件</param>
    /// <param name="isAnim">是否需要加钱动画</param>
    public void MinusMyTempCredit(long credit, bool isEvent = true, bool isAnim = false)
    {
        long fromCredit = MainModel.Instance.myCredit;
        MainModel.Instance.myCredit -= credit;

        if (isEvent)
        {


            /* 停掉“加钱动画”，设置到当前金额
            EventCenter.Instance.EventTrigger<EventData>(MetaUIEvent.ON_CREDIT_EVENT,
                new EventData<bool>(MetaUIEvent.UpdateNaviCredit, true));
            */

            // 停掉“加钱动画”，设置到当前金额
            EventCenter.Instance.EventTrigger<EventData>(MetaUIEvent.ON_CREDIT_EVENT,
            new EventData<UpdateNaviCredit>(MetaUIEvent.UpdateNaviCredit,
            new UpdateNaviCredit()
            {
                isAnim = SyncCreditAnim(isAnim),
                fromCredit = fromCredit,
                toCredit = MainModel.Instance.myCredit
            }));
        }

    }



    /// <summary>
    /// 同步“Temp金额”到“真实金额”
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// * 在没有“加减钱动画”和玩游中，进行同步！<br/>
    /// * 同步成功，并通知UI刷新<br/>
    /// * MyCredit = MyUICredit + MyTempCredit<br/>
    /// </remarks>
    public bool TrySyncMyCreditToReel()
    {
        //if (IsCo(COR_MY_CREDIT_AINM)) return false;

        if(coMyCreditAnim != null) return false;

        if (MainModel.Instance.isSpin)
        {
            return false;
        }
        
        SyncMyTempCreditToReal(true);// 允许同步
        return true;
    }

    /// <summary>
    /// 自动同步玩家金额（等待游戏结束后，才同步）
    /// </summary>
    public void AutoSyncMyCreditToReel()
    {
        //DoCo(COR_AUTO_SYNC_MY_REAL_CREDIT, _AutoSyncMyCreditToReal());

        ClearCo(coAutoSyncMyRealCredit);
        coAutoSyncMyRealCredit = StartCoroutine(_AutoSyncMyCreditToReal());
    }

    IEnumerator _AutoSyncMyCreditToReal()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (TrySyncMyCreditToReel())
                break;
        }
    }


    /// <summary>
    /// 直接同步到真实玩家金币,或边播放动画边加上金额
    /// </summary>
    /// <param name="minusCredit"></param>
    /// <remarks>
    /// * 播放动画时，即使加了金额，也会被动画冲掉？？
    /// </remarks>
    public void AddOrSyncMyCreditToReal(long addCredit)
    {
        if (TrySyncMyCreditToReel())
            return;

        // my_ui_credit 在播放动画时，my_ui_credit 边播放动画，边加上金额
        AddMyTempCredit(addCredit, true, false);
    }

    /// <summary>
    /// 直接同步到真实玩家金币,或边播放动画边减去金额
    /// </summary>
    /// <param name="minusCredit"></param>
    /// <remarks>
    /// * 播放动画时，即使加了金额，也会被动画冲掉？？
    /// </remarks>
    public void MinusOrSyncMyCreditToReal(long minusCredit)
    {
        if (TrySyncMyCreditToReel())
            return;

        // my_ui_credit 在播放动画时，my_ui_credit 边播放动画，边减去金额
        MinusMyTempCredit(minusCredit, true, false);
    }



    /// <summary>
    /// 当下玩家金额增加
    /// </summary>
    /// <param name="credit">要减少的金额</param>
    /// <param name="isEvent">是否发送事件</param>
    /// <param name="isAnim">是否需要加钱动画</param>
    public void AddMyTempCredit(long credit, bool isEvent = true, bool isAnim = false)
    {
        //DebugUtils.LogWarning($"通知UI-Credit 加钱： {credit}");
        long fromCredit = MainModel.Instance.myCredit;
        MainModel.Instance.myCredit += credit;


        if (isEvent)
        {

            /* 
            EventCenter.Instance.EventTrigger<EventData>(MetaUIEvent.ON_CREDIT_EVENT,
                new EventData<bool>(MetaUIEvent.UpdateNaviCredit, true));
            */
            // 停掉“加钱动画”，设置到当前金额
            EventCenter.Instance.EventTrigger<EventData>(MetaUIEvent.ON_CREDIT_EVENT,
                new EventData<UpdateNaviCredit>(MetaUIEvent.UpdateNaviCredit,
                new UpdateNaviCredit()
                {
                    isAnim = SyncCreditAnim(isAnim),
                    fromCredit = fromCredit,
                    toCredit = MainModel.Instance.myCredit
                }));
        }

    }

    // 加钱并同步


    /// <summary>
    /// 同步到玩家的真实金额
    /// </summary>
    /// <param name="isEvent">是否发送事件</param>
    public void SyncMyTempCreditToReal(bool isEvent)
    {
        MainModel.Instance.myCredit = SBoxModel.Instance.myCredit;

        if (isEvent)
        {

            // 停止之前的“加钱动画”，同步到当前的金额
            EventCenter.Instance.EventTrigger<EventData>(MetaUIEvent.ON_CREDIT_EVENT,
                new EventData<UpdateNaviCredit>(MetaUIEvent.UpdateNaviCredit,
                new UpdateNaviCredit()
                {
                    isAnim = SyncCreditAnim(false),
                    toCredit = MainModel.Instance.myCredit
                }));
        }
    }

    /// <summary>
    /// 同步到当下的玩家金额
    /// </summary>
    public void SyncMyUICreditToTemp()
    {

        // 停止之前的“加钱动画”，同步到当前的金额
        EventCenter.Instance.EventTrigger<EventData>(MetaUIEvent.ON_CREDIT_EVENT,
            new EventData<UpdateNaviCredit>(MetaUIEvent.UpdateNaviCredit,
            new UpdateNaviCredit()
            {
                isAnim = SyncCreditAnim(false),
                toCredit = MainModel.Instance.myCredit
            }));
    }



    /// <summary>
    /// 停止之前的所有金币动画，并设置玩家金额
    /// </summary>
    /// <param name="isEvent">是否发送事件</param>
    public void SetMyTempCredit(long credit, bool isEvent = true)
    {

        MainModel.Instance.myCredit = credit;

        if (isEvent)
        {

            // 停止之前的“加钱动画”，同步到当前的金额
            EventCenter.Instance.EventTrigger<EventData>(MetaUIEvent.ON_CREDIT_EVENT,
                new EventData<UpdateNaviCredit>(MetaUIEvent.UpdateNaviCredit,
                new UpdateNaviCredit()
                {
                    isAnim = SyncCreditAnim(false),
                    toCredit = MainModel.Instance.myCredit
                }));

        }
    }


    /// <summary>
    /// “每局游戏编号”归零
    /// </summary>
    public void ClearGameNumber()
    {
        MainModel.Instance.gameNumberNode = null;
        SQLitePlayerPrefs03.Instance.SetString(MainModel.PARAM_GAME_NUMBER,"{}");
    }

    /// <summary>
    /// “每局游戏上报编号”归零
    /// </summary>
    public void ClearReportId()
    {
        MainModel.Instance.reportIdNode = null;
        SQLitePlayerPrefs03.Instance.SetString(MainModel.PARAM_REPORT_ID, "{}");
    }
}
