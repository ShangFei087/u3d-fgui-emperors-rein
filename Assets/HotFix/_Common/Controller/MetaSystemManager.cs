using System.Collections.Generic;
using UnityEngine;
using GameMaker;
using System;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using FairyGUI;



public class MetaSystemManager : MonoSingleton<MetaSystemManager>
{
    // Start is called before the first frame update
    void OnEnable()
    {

        EventCenter.Instance.AddEventListener<EventData>(GlobalEvent.ON_TOOL_EVENT, OnToolEvent);

        //TestManager.Instance.Init();

        ResourceManager02.Instance.LoadAsset<TextAsset>("Assets/GameRes/_Common/Game Maker/ABs/Datas/tmg_page.json", (TextAsset txt) =>
        {
            TestManager.Instance.SetKV(TestManager.DATA_PAGES, txt.text);
        });  

        ResourceManager02.Instance.LoadAsset<TextAsset>("Assets/GameRes/_Common/Game Maker/ABs/Datas/tmg_custom_button.json", (TextAsset txt) =>
        {
            TestManager.Instance.SetKV(TestManager.DATA_CUSTOM_BUTTON, txt.text);
        });

        AnalysisTest(null);
    }

    // Update is called once per frame
    void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener<EventData>(GlobalEvent.ON_TOOL_EVENT, OnToolEvent);
    }


    void OnToolEvent(EventData res)
    {
        switch (res.name)
        {
            case GlobalEvent.AnalysisTest:
                {
                    AnalysisTest(res);
                }
                break;
            case GlobalEvent.PageButton:
                {
                    OnClickPageBtn(res);
                }
                break;
            case GlobalEvent.ApplicationQuit:
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false; // 编辑器中退出播放模式
#else
                    Application.Quit(); // 构建后退出应用
#endif
                }
                break;

            case GlobalEvent.CustomButtonCoinIn:
                {
                    OnClickCustomButtonCoinIn(res);
                }
                break;
            case GlobalEvent.CustomButtonTicketOut:
                {
                    OnClickCustomButtonTicketOut(res);
                }
                break;
            case GlobalEvent.CustomButtonCreditUp:
                {
                    OnClickCustomButtonCreditUp(res);
                }
                break;
            case GlobalEvent.CustomButtonCreditDown:
                {
                    OnClickCustomButtonCreditDown(res);
                }
                break;
            case GlobalEvent.DeviceTestPrintTicket:
                OnClickDeviceTestPrintTicket(res);
                break;

        }
    }

    private void OnClickDeviceTestPrintTicket(EventData res)
    {
        DevicePrinterOut.Instance.DoPrinterOut();
    }

    public void OnClickCustomButtonCoinIn(EventData data)
    {
        EventCenter.Instance.EventTrigger<CoinInData>(SBoxSanboxEventHandle.COIN_IN,
            new CoinInData()
            {
                id = 0,
                coinNum = 1,
            });
    }
    public void OnClickCustomButtonTicketOut(EventData data)
    {
        MachineDeviceCommonBiz.Instance.TestTicketOut();
    }

    public void OnClickCustomButtonCreditUp(EventData data)
    {
        DeviceCreditUpDown.Instance.CreditUp();
    }
    public void OnClickCustomButtonCreditDown(EventData data)
    {
        DeviceCreditUpDown.Instance.CreditDown();
    }



    public void AnalysisTest(EventData res = null)
    {
        GCMonitorPro comp = GetComponentInChildren<GCMonitorPro>();
        if(comp != null)
        {
            if(res == null)
                comp.enabled = false;
            else 
                comp.enabled = (bool)res.value;
        }
    }


    public void OnClickPageBtn(EventData data)
    {

        Dictionary<string, object> res = (Dictionary<string, object>)data.value;

        string pgName = (string)res["pageName"];
        string pgData = (string)res["pageData"];

        DebugUtils.Log($" name = {pgName}   value = { JsonConvert.SerializeObject(pgData)} ");

        PageName pageName = (PageName)Enum.Parse(typeof(PageName), pgName);

        /*if (pageName == PageName.ConsolePageConsoleMain)
        {
            MachineDeviceCommonBiz.Instance.OpenConsole();
        }
        else
        {
            if (PageManager.Instance.IndexOf(pageName) != -1)
            {
                PageManager.Instance.ClosePage(pageName);
            }
            else
            {
                PageManager.Instance.OpenPage(pageName);
            }
        }*/

        if (PageManager.Instance.IndexOf(pageName) != -1)
        {
            PageManager.Instance.ClosePage(pageName);
        }
        else
        {
            PageManager.Instance.OpenPage(pageName);
        }

    }


    GTweener tweener;

    [Button]
    void TestTween()
    {
        // 这里可能换成Dotween
        tweener = GTween.To(1, 10, 3)
            .SetEase(EaseType.Linear)  // 设置缓动函数
            .OnUpdate((GTweener tweener) =>
            {
                // 每次更新时调用
                //target.y = tweener.value.x;

                DebugUtils.Log($"[Tween] cur:{tweener.value.x}");
            })
            .OnComplete(() =>
            {
                //action?.Invoke();
               DebugUtils.Log("Tween complete!");
            });
    }

    [Button]
    void TestStopTween()
    {
        // if (tweener != null) 
        tweener.Kill();
        //GTween.Kill(tweener);   
    }


    [Button]
    void TestEnum()
    {
        S2C_CMD cmd = S2C_CMD.S2C_LoginR;
        string cmdStr = cmd.ToString();
        DebugUtils.Log(cmdStr);

        S2C_CMD res = (S2C_CMD)Enum.Parse(typeof(S2C_CMD), cmdStr);

        DebugUtils.Log($"res = {res.ToString()}");
    }
}
