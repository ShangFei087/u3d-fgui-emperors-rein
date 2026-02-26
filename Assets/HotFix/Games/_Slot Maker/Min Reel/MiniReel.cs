using FairyGUI;
using System;
using System.Collections.Generic;
using UnityEngine;


// MiniReel
// MiniReelSymbol
public class MiniReel 
{
    static List<string> codes = new List<string>() { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

    public GComponent goOwnerReel;


    public float width = 46;

    public void Init(GComponent reel)
    {
        goOwnerReel = reel;
        if(_tran!=null) 
            _tran.Stop();
        _tran = null;
    }

    public void Stop()
    {
        if (_tran != null)
            _tran.Stop();
    }


    Transition _tran;
    Transition tran
    {
        get
        {
            if (_tran == null && goOwnerReel != null)
                _tran = goOwnerReel.GetTransition("animTurn");
            return _tran;
        }
    }


    void SetSymbol(int index)
    {
        string value = codes[index];
        SetSymbol(value);
    }


    public void SetSymbol(string value)
    {
        CheckSymbol(value);

        tran?.Stop();
        GObject[] child = goOwnerReel.GetChildren();
        foreach (GObject obj in child)
        {
            obj.visible = false;
        }
        GTextField cur = goOwnerReel.GetChildAt(0).asTextField;
        cur.visible = true;
        cur.text = value;
        data = value;
    }

    string data;



    void CheckSymbol(string value)
    {
        if (!goOwnerReel.visible)
            goOwnerReel.visible = true;

        goOwnerReel.width = codes.Contains(value) ? width : width / 2f;
    }

    float _speed = 1f;
    public void TurnOrKeepSymbol(string value, Action cb = null, float speed = 1f) 
    {
        _speed = speed;

        CheckSymbol(value);

        if (data == value || !codes.Contains(value))
        {
            SetSymbol(value);
            if (cb != null)
                cb();
            return;
        }
        //Debug.LogError($"@@ {data} - {value}");
        int nowIdx = 0;
        if (codes.Contains(data))
            nowIdx = codes.IndexOf(data);

        int idx = codes.IndexOf(value);

        TurnSymbol(nowIdx, idx, cb);
    }

    void TurnSymbol(int startIdx = 0, int endIdx = -1, Action cb = null)
    {
        tran?.Stop();
        _TurnSymbolOnce(startIdx, endIdx,cb);
    }



    void _TurnSymbolOnce(int startIdx = 0, int endIdx = -1, Action cb = null)
    {
        int nowIdx = startIdx;

        data = codes[nowIdx];  // 新加

        GObject[] child = goOwnerReel.GetChildren();
        foreach (GObject obj in child)
        {
            obj.visible = false;
        }
        goOwnerReel.GetChild("step1_0").visible = true;

        int lastIdx = nowIdx + 1;

        if (lastIdx >= codes.Count)
            lastIdx = 0;

        goOwnerReel.GetChild("step1_0").asTextField.text = $"{codes[nowIdx]}";
        goOwnerReel.GetChild("step2_0").asTextField.text = $"{codes[nowIdx]}";
        goOwnerReel.GetChild("step2_0").asTextField.text = $"{codes[lastIdx]}";
        goOwnerReel.GetChild("step3_0").asTextField.text = $"{codes[nowIdx]}";
        goOwnerReel.GetChild("step3_1").asTextField.text = $"{codes[lastIdx]}";
        goOwnerReel.GetChild("step4_0").asTextField.text = $"{codes[lastIdx]}";

        if ((endIdx == -1)
        || (endIdx != -1 && endIdx == nowIdx))
        {
            data = codes[endIdx];  // 新加
            cb?.Invoke();
            return;
        }

        tran.timeScale = _speed;

        //  times: 播放动画 1 次，延迟 delay 秒后开始，
        // public void Play(int times, float delay, PlayCompleteCallback onComplete)

        tran.Play(() =>
        {
            _TurnSymbolOnce(lastIdx, endIdx,cb);
        });  
    }
}
