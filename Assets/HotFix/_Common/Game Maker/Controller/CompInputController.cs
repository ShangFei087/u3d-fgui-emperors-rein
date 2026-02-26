using CY.GameFramework;
using FairyGUI;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using GameUtil;
using System;

public class CompInputController
{
    GTextField txtInput;
    GButton btn;
    Action onExitCallback;
    Action<string> onOKCallback;

    public bool isPlaintext = true;
    public void Init(GButton btn, GTextField txtInput, Action<string> onOKCallback,  Action onExitCallback, string content = "", bool isPlaintext = true)
    {
        this.onExitCallback = onExitCallback;
        this.onOKCallback = onOKCallback;
        this.inputStr = content;
        this.isPlaintext = isPlaintext;
        this.btn = btn;
        this.txtInput = txtInput;
        this.btn.onClick.Clear();
        this.btn.onClick.Add(OnClick);

        isShowTip = false;
        this.txtInput.text = GetContent();
    }

  //  private void OnEnable() { InitParam(); }

    
    public void Disable(){

        if (timInp != null)
            timInp.Cancel();
        timInp = null;    
    }

    void OnClick()
    {
        GetFocus();
    }

    string GetContent()
    {
        if (isPlaintext)
        {
            return isShowTip ? $"{inputStr}|" : $"{inputStr} ";
        }
        else
        {
            string res = "";
            for (int i = 0; i < inputStr.Length; i++)
            {
                res += "*";
            }
            return isShowTip ? $"{res}|" : $"{res} ";
        }
    }

    public void SetPlaintext(bool isPlt)
    {
        isPlaintext = isPlt;
        txtInput.text = GetContent();
    }

    string inputStr = "";

    public string value
    {
        get => inputStr;
        set => inputStr = value;
    }

    bool isShowTip = false;

    public void Delete()
    {
        if (inputStr.Length > 0)
        {
            inputStr = inputStr.Substring(0, inputStr.Length - 1);
        }
        txtInput.text = GetContent();
    }
    public void Clear()
    {
        inputStr = "";
        txtInput.text = GetContent();
    }

    public void Add(string data)
    {
        inputStr += data;
        txtInput.text = GetContent();
    }

    public void Exit()
    {
        this.onExitCallback?.Invoke();
    }

    public void OK()
    {
        this.onOKCallback?.Invoke(value);
    }

    public void LostFocus()
    {
        if (timInp != null)
            timInp.Cancel();
        timInp = null;
        isShowTip = false;
        txtInput.text = GetContent();
    }

    LoopTimer timInp = null;
    public void GetFocus()
    {

        if(timInp != null)
            timInp.Cancel();
        timInp = Timer.LoopAction(0.5f, (data) =>
        {
            isShowTip = !isShowTip;
            txtInput.text = GetContent();
        });


        if (CompKeyboardController.curKeyboard != null)
            CompKeyboardController.curKeyboard.SetFocus(this);
        
    }


}
