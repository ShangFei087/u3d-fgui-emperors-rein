#if UNITY_EDITOR
using Sirenix.OdinInspector;
using System;

#endif
using System.Collections.Generic;
using UnityEngine.UI;

using GameMaker;
using System.Reflection;


public interface IComomonPopupHandler
{
    void OpenPopup(CommonPopupInfo info);
    void SetContent(CommonPopupInfo info);
    void ClosePopup();
    void SetPopupToTop();
    bool IsOpen();

}

public partial class CommonPopupHandler : MonoSingleton<CommonPopupHandler>
{
    public List<CommonPopupInfo> errorStack = new List<CommonPopupInfo>();


    public bool isOpen(string mark)
    {
        if (errorStack.Count > 0)
        {
            for (int i = 0; i < errorStack.Count; i++)
            {
                if (errorStack[i].mark == mark)
                    return true;
            }
        }
        return false;
    }

    public CommonPopupType curPopupType => errorStack.Count > 0 ? errorStack[0].type : CommonPopupType.None;



    IComomonPopupHandler _iPopup;
    public IComomonPopupHandler iPopup
    {
        get
        {
            if(_iPopup == null)
            {
                _iPopup = new CommonPopupHelper01(); 
            }
            return _iPopup;
        }
        set
        {
            if (_iPopup != null)
            {
                _iPopup.ClosePopup();
            }
            _iPopup = value;
        }
    }

    public void OpenPopup(CommonPopupInfo info)
    {
        if (!string.IsNullOrEmpty(info.mark))  //过滤重复消息，并置顶显示
        {
            int i = errorStack.Count;
            while (--i >= 0)
            {
                if (errorStack[i].mark == info.mark)
                    errorStack.RemoveAt(i);
            }
        }

        errorStack.Insert(0, info);
        if (!iPopup.IsOpen())
        {
            iPopup.OpenPopup(info);
            //iPopup = PageManager.Instance.OpenPage(popupCommon, new EventData<CommonPopupInfo>("Null", info));
        }
        else
        {
            //pageSystemPoup.SetContent(new EventData<ErrorPopupInfo>("Null", info));
            //iPopup.SetContent(new EventData<CommonPopupInfo>("Null", info));
            iPopup.SetContent(info);
            iPopup.SetPopupToTop();
        }


        /*
        if (index != 0)
            popup.BringToFront();
        */
    }

    public void OpenPopupSingle(CommonPopupInfo info)
    {
        if (string.IsNullOrEmpty(info.mark))
            info.mark = info.text;
        OpenPopup(info);
    }

    public void ClosePopup()
    {
        if (!iPopup.IsOpen())
        {
            errorStack.Clear(); 
            return;        
        }


        errorStack.RemoveAt(0);

        if (errorStack.Count == 0)
        {
            //curButtons.Clear();
            //PageManager.Instance.ClosePage(pageSystemPoup);
            //pageSystemPoup = null;

            iPopup.ClosePopup();
        }
        else
        {
            CommonPopupInfo info = errorStack[0];
            //pageSystemPoup.SetContent(new EventData<ErrorPopupInfo>("Null", info));
            //iPopup.SetContent(new EventData<CommonPopupInfo>("Null", info));

            iPopup.SetContent(info);
        }
    }


    public void ClosePopup(string mark)
    {
        if (!iPopup.IsOpen())
        {
            errorStack.Clear();
            return;
        }

        int idx = -1;
        for (int i = 0; i < errorStack.Count; i++)
        {
            if (errorStack[i].mark == mark)
            {
                idx = i;
                break;
            }
        }

        if (idx == 0)
            ClosePopup();
        else if (idx > 0)
            errorStack.RemoveAt(idx);
    }


    public void CloseAllPopup()
    {
        errorStack.Clear();
        iPopup.ClosePopup();

    }



#if UNITY_EDITOR


    [Button]
    public void TestErrorPoup1()
    {
        CommonPopupHandler.Instance.OpenPopupSingle(new CommonPopupInfo()
        {
            text = I18nMgr.T("<size=24>打印机未连接</size>"),
            type = CommonPopupType.OK,
            buttonText1 = I18nMgr.T("OK"),
            buttonAutoClose1 = true,
            callback1 = delegate
            {
            },
            isUseXButton = false,
        });
    }



    [Button]
    public void TestErrorPoup(CommonPopupType type, string msg = "请输入内容", bool isUseXButton = true)
    {

        CommonPopupInfo info = new CommonPopupInfo();
        info.text = $"<size=24>{msg}</size>";
        info.type = type;

        info.buttonText1 = "No";
        info.buttonAutoClose1 = true;
        info.callback1 = delegate
        {
            DebugUtils.Log($"i am btn1 {msg}");
        };

        info.buttonText2 = "Yes";
        info.buttonAutoClose2 = true;
        info.callback2 = delegate
        {
            DebugUtils.Log($"i am btn2 {msg}");
        };

        info.isUseXButton = isUseXButton;
        info.callbackX = delegate
        {
            DebugUtils.Log($"i am btnX {msg}");
        };
        CommonPopupHandler.Instance.OpenPopupSingle(info);
    }
#endif


}




