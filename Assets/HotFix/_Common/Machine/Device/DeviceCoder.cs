using GameMaker;
using SBoxApi;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceCoder : MonoSingleton<DeviceCoder>
{


    const string MARK_IS_POPUP_CHECK_ACTIVE = "MARK_IS_POPUP_CHECK_ACTIVE";
    //const string COR_CHECK_CODER_REPEAT = "COR_CHECK_CODER_REPEAT";
    void OnEnable()
    {
        /*
        if (!ApplicationSettings.Instance.isMachine)
            return;
        */

        //UI--显示Licenen
        EventCenter.Instance.AddEventListener<EventData>(MachineUIEvent.ON_MACHINE_UI_EVENT, OnMachineUIEvent);


        EventCenter.Instance.AddEventListener<EventData>(GlobalEvent.ON_DEVICE_EVENT, OnDeviceEvent);


        //CoroutineAssistant.DoCo(COR_CHECK_CODER_REPEAT, CoroutineAssistant.DoTaskRepeat(CheckSBoxNeedActivated, 10000));


        //##ClearCo(corCheckCoderRepeat);
        //##corCheckCoderRepeat = StartCoroutine(RepeatTask(CheckSBoxNeedActivated, 10000));
    }

    private void OnDisable()
    {
        //CoroutineAssistant.ClearCo(COR_CHECK_CODER_REPEAT);
        ClearCo(coCheckCoderRepeat);

        EventCenter.Instance.RemoveEventListener<EventData>(MachineUIEvent.ON_MACHINE_UI_EVENT, OnMachineUIEvent);
        EventCenter.Instance.RemoveEventListener<EventData>(GlobalEvent.ON_DEVICE_EVENT, OnDeviceEvent);
    }


    void OnDeviceEvent(EventData res)
    {
        if(res.name == GlobalEvent.CheckMachineActiveRepeat)
        {
            ClearCo(coCheckCoderRepeat);
            coCheckCoderRepeat = StartCoroutine(RepeatTask(CheckSBoxNeedActivated, 10000));
            CheckSBoxNeedActivated();
        }
    }


    // 
    void ClearCo(Coroutine co)
    {
        if (co != null)
            StopCoroutine(co);
        co = null;
    }
    IEnumerator DelayTask(Action task, int timeMS)
    {
        yield return new WaitForSeconds((float)timeMS/1000f);
        task?.Invoke();
    }
    IEnumerator RepeatTask(Action task, int timeMS)
    {
        while (true)
        {
            yield return new WaitForSeconds((float)timeMS / 1000f);
            task?.Invoke();
        }
    }
    Coroutine coCheckCoderRepeat = null;

    Coroutine coCode = null;





    private void OnMachineUIEvent(EventData evt)
    {
        if (evt.name == MachineUIEvent.ShowCoding)
        {
            MachineDataManager02.Instance.RequestMachineCodingInfo((object res) =>
            {
                OnResponseShowCoder(res as SBoxCoderData, (PageName)evt.value);

            }, (BagelCodeError err) =>
            {

            });
        }
        else if (evt.name == MachineUIEvent.CheckCodeActive)
        {
            CheckSBoxNeedActivated();
        }
    }



    private void CheckSBoxNeedActivated()
    {
        //DebugUtils.LogError("检测打码");
        //假数据
        MachineDataManager02.Instance.RequestIsCodingActive((res) =>
        {
            int code = (int)res;
            bool isActive = code == 0;
            //////暂时先都激活
            isActive = true;
            //DebugUtils.Log($"check code; isActive = {isActive}");

            SBoxModel.Instance.isMachineActive = isActive;
            //BlackboardUtils.SetValue<bool>("@console/isMachineActive", isActive);

            if (!(bool)isActive)
            {
                //控台没有打开
                if (PageManager.Instance.IndexOf(PageName.ConsolePageConsoleMain) == -1  
                 && PageManager.Instance.IndexOf(PageName.ConsolePusher01PageConsoleMain) == -1)
                {

                    if (!CommonPopupHandler.Instance.isOpen(MARK_IS_POPUP_CHECK_ACTIVE))
                        CommonPopupHandler.Instance.OpenPopupSingle(new CommonPopupInfo()
                        {
                            text = string.Format(I18nMgr.T("<size=24>Please activate : {0}</size>"), code),
                            type = CommonPopupType.SystemTextOnly,
                            buttonAutoClose1 = false,
                            buttonAutoClose2 = false,
                            isUseXButton = false,
                            mark = MARK_IS_POPUP_CHECK_ACTIVE,
                        });
                }
            }
        });
    }


    /// <summary>
    /// 返回打码数据
    /// </summary>
    /// <param name="data"></param>
    private async void OnResponseShowCoder(SBoxCoderData data , PageName pageName)   // string pageName)
    {


        long totalBets = data.Bets;
        long totalWins = data.Wins;

        Dictionary<string, object> req = new Dictionary<string, object>()
        {
            ["A"] = $"{totalBets}", //data.Bets.ToString(),
            ["B"] = $"{totalWins}", //data.Wins.ToString(),
            ["C"] = data.MachineId.ToString(),
            ["D"] = data.CoderCount.ToString(),
            ["E"] = data.CheckValue.ToString(),
            ["Day"] = (data.RemainMinute / (60 * 24)).ToString(),//多少天
            ["Hour"] = ((data.RemainMinute % (60 * 24) / 60)).ToString(),//多少小时
            ["Minute"] = (data.RemainMinute % 60).ToString(),//多少分钟
        };

        EventData res = null;

        if (pageName == PageName.ConsolePopupConsoleCoder)//(pageName == "ConsolePopupConsoleCoder")
        {
            res = await PageManager.Instance.OpenPageAsync(PageName.ConsolePopupConsoleCoder,  new EventData<Dictionary<string, object>>("", req));
        }else if (pageName == PageName.ConsolePusher01PageConsoleCoder)//(pageName == "ConsolePusher01PageConsoleCoder")
        {
            res = await PageManager.Instance.OpenPageAsync(PageName.ConsolePusher01PageConsoleCoder, new EventData<Dictionary<string, object>>("", req));
        }
        else
        {
            DebugUtils.LogError($"pageName == {pageName}");
        }


        if (res != null && res.value != null)
        {

            MachineDataManager02.Instance.RequestSetCoding(ulong.Parse((string)res.value), (res) =>
            {
                OnCoder(res as SBoxPermissionsData);
            }, (err) =>
            {
                OnCoder(err.response as SBoxPermissionsData);
            });
        }

    }
    
    //const string COR_CODE = "COR_CODE";
    
    private void OnCoder(SBoxPermissionsData sBoxPermissionsData)
    {
        ClearCo(coCode);
        coCode = StartCoroutine(_OnCode(sBoxPermissionsData));
        //CoroutineAssistant.DoCo(COR_CODE, _OnCode(sBoxPermissionsData));
    }


    IEnumerator _OnCode(SBoxPermissionsData sBoxPermissionsData)
    {

        bool isNext = false;

        bool isSuccess = sBoxPermissionsData.result == 0;
        if (isSuccess) // 成功
        {
            DebugUtils.LogWarning("打码成功");

            if (sBoxPermissionsData.permissions / 1000 > 0)//2001
            {
                if (sBoxPermissionsData.permissions % 10 > 0) //清帐
                {
                    DebugUtils.LogWarning("清除游戏记录");
                    //清除“游戏记录”，“投退币记录”数据。

                    /*#seaweed#*/
                    try
                    {
                        MaskPopupHandler.Instance.OpenPopup();
                    }
                    catch (Exception e)
                    {
                        DebugUtils.LogWarning("遮罩打卡出问题了 "+ e.Message);
                    }


                    MachineRestoreManager.Instance.ClearRecordWhenCoding();

                    yield return new WaitForSeconds(5f);
                }
                else
                {
                    //不清帐
                    DebugUtils.LogWarning("不清帐");

                    //#seaweed# 打码返回有问题，默认打码就清数据库。(之后再拿掉这块代码！)
                    MachineRestoreManager.Instance.ClearRecordWhenCoding();
                }
            }
            else
            {
                //#seaweed# 打码返回有问题，默认打码就清数据库。(之后再拿掉这块代码！)
                MachineRestoreManager.Instance.ClearRecordWhenCoding();

                DebugUtils.LogError("打码返回数据有问题！！");
            }

            // 同步玩家金额
            //SyncPlayerCredit();

            // 已激活
            SBoxModel.Instance.isMachineActive = true;

            //关掉锁死弹窗
            CommonPopupHandler.Instance.ClosePopup(MARK_IS_POPUP_CHECK_ACTIVE);

            // 【新加】重新获取配置
            MachineDataManager02.Instance.RequestReadConf((data) =>
            {
                SBoxConfData res = (SBoxConfData)data;
                SBoxModel.Instance.SboxConfData = res;
                isNext = true;
            }, (BagelCodeError err) =>
            {
                DebugUtils.LogError(err.msg);
                isNext = true;
            });


            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            /*
            if (_consoleBB.Instance.betList.Count > 0)
            {
                //20012 失败要重发
                BlackboardUtils.SetValue<int>("./betIndex", 0);
                long totalBet = _consoleBB.Instance.betList[0];
                BlackboardUtils.SetValue<long>("./totalBet", totalBet);

                bool isBreak = false;
                do
                {
                    DebugUtils.LogWarning($"【Test】 设置压注： {totalBet}");
                    MachineDataManager02.Instance.RequestSetPlayerBets(0, totalBet, (res) =>
                    {
                        int result = (int)res;

                        if (result == 0)
                        {
                            isBreak = true;
                        }
                        else
                        {
                            DebugUtils.LogError($"set total bet for machine is err :{result}");
                        }

                        isNext = true;
                    });

                    yield return new WaitUntil(() => isNext == true);
                    isNext = false;

                } while (!isBreak);
            }*/


            /*#seaweed#*/
            try
            {
                MaskPopupHandler.Instance.ClosePopup();
            }
            catch (Exception e)
            {
                DebugUtils.LogWarning("遮罩关闭出问题了 " + e.Message);
            }
            


            // 通知重新获取彩金值
            EventCenter.Instance.EventTrigger<EventData>(GlobalEvent.ON_DEVICE_EVENT, new EventData(GlobalEvent.CodeCompleted));
        }
        else
        {
            DebugUtils.LogWarning("打码失败");
        }

        // 延时打开？？
        TipPopupHandler.Instance.OpenPopup(I18nMgr.T(isSuccess ? "Coding activation successful" : "Coding activation failed"));
    }
}
