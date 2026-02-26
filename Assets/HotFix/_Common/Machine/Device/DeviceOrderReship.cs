using System.Collections;
using UnityEngine;
using System;

/// <summary>
/// 订单补发
/// </summary>
/// <remarks>
/// * 发现正常投退币操作，则退出循环检查本地订单缓存的操作。
/// * 在正常投退操作完成后延时20秒，开始循环检查本地订单缓存。
/// </remarks>
public class DeviceOrderReship : MonoSingleton<DeviceOrderReship>
{

    /// <summary> 重复补发订单 </summary>
    public void DelayReshipOrderRepeat()
    {
        ClearCo(coDelayReshipOrderRepeat);
        coDelayReshipOrderRepeat = StartCoroutine(ReshipOrderRepeat());
    } 


    /// <summary> 停止重复补发订单 </summary>
    public void ClearReshipOrderRepeat() => ClearCo(coDelayReshipOrderRepeat);



    void ClearCo(Coroutine co)
    {
        if (co != null)
            StopCoroutine(co);
        co = null;
    }
    IEnumerator DelayTask(Action task, int timeMS)
    {
        yield return new WaitForSeconds((float)timeMS / 1000f);
        task?.Invoke();
    }

    Coroutine coReshipOrderOnce = null;
    Coroutine coDelayReshipOrderRepeat = null;


    void Start()
    {

        ClearCo(coReshipOrderOnce);
        coReshipOrderOnce = StartCoroutine(ReshipOrderOnce(() => coReshipOrderOnce = null));


        DelayReshipOrderRepeat();
    }

    protected override void OnDestroy()
    {
        //ClearCo(corReshipOrderOnce);
        //ClearCo(corDelayReshipOrderRepeat);
        StopAllCoroutines();
        base.OnDestroy();
    }


    IEnumerator ReshipOrderRepeat()
    {
        yield return new WaitForSecondsRealtime(25f); // 延时读算法卡

        while (true)
        {
            DebugUtils.LogWarning($"【OrderReship】:开始检查订单补发 time:{Time.unscaledTime}");

            yield return DeviceCoinOut.Instance.ReshipOrde();

            yield return DeviceCoinIn.Instance.ReshipOrde();

            //##yield return DeviceMoneyBox.Instance.ReshipOrde();

            yield return new WaitForSecondsRealtime(20f);
        }
    }


    IEnumerator ReshipOrderOnce(Action onFinishCallback)
    {
        yield return new WaitForSeconds(2);

        DebugUtils.LogWarning($"【OrderReship】:开始检查订单补发 time:{Time.unscaledTime}");

        yield return DeviceCoinOut.Instance.ReshipOrde();

        yield return DeviceCoinIn.Instance.ReshipOrde();

        //##yield return DeviceMoneyBox.Instance.ReshipOrde();

        onFinishCallback?.Invoke();
    }


}
