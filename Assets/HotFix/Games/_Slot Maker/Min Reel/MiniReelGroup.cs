using FairyGUI;
using System;
using System.Collections.Generic;
using UnityEngine;


public class MiniReelGroup 
{
    static int number = 0;
    public MiniReelGroup()
    {
        this.guid = Time.unscaledTime.ToString() + $"-{++number}";
    }
    public float nowData = 0f;

    public string format = "N2";

    public GList glstOwnerReels;


    string guid = null;

    string name;

    List<MiniReel> miniReels = new List<MiniReel>();


    public void Init(string name, GList reels, string fmt)
    {
        this.name = name;
        format = fmt;
        glstOwnerReels = reels;
        //DebugUtils.LogError($"{name}-{guid}  - {glstOwnerReels.displayObject.name}  - {glstOwnerReels.displayObject.id} - {glstOwnerReels.container.name} - {glstOwnerReels.container.id}");
        foreach (MiniReel reel in miniReels)
        {
            reel.Stop();
        }

        //#xuhen#多语言对象残留#

        foreach (MiniReel item in miniReels)
        {
            GComponent goReel = item.goOwnerReel;
            if (goReel.displayObject.gameObject != null)
            {
                GOResidualMark grm = goReel.displayObject.gameObject.GetOrAddComponent<GOResidualMark>();
                grm.InitParam(goReel);
                grm.referenceCount--;
            }
            //DebugUtils.LogError($"{Time.unscaledTime}: MiniReel 对象残留: name ={name}-{guid}   {goReel.id}  -  {goReel.displayObject.id} - {goReel.displayObject.gameObject.name}");
        }

        miniReels = new List<MiniReel>();
        for (int i=0; i< glstOwnerReels.numChildren; i++)
        {
            miniReels.Add(new MiniReel());

            GComponent goReel = glstOwnerReels.GetChildAt(i).asCom;
            GOResidualMark grm = goReel.displayObject.gameObject.GetOrAddComponent<GOResidualMark>();
            grm.InitParam(goReel);
            grm.referenceCount++;
            miniReels[i].Init(goReel);

            //#xuhen#多语言对象残留#
            //DebugUtils.LogError($"{Time.unscaledTime}: MiniReel 对象: name ={name}-{guid}  {goReel.id}  -  {goReel.displayObject.id} - {goReel.displayObject.gameObject.name}");
        }
    }




    public string GetDataStr()
    {
        string res = "";

        GObject[] childs = glstOwnerReels.GetChildren();

        foreach (GObject child in childs)
        {
            if (child.visible)
            {
                res += child.asCom.GetChildAt(0).asTextField.text;
            }
        }
        return res;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="callBack"></param>
    /// <param name="isForceTurn">新数值和之前数组相同也强行转动</param>
    public void SetData(float value, Action callBack = null , bool isForceTurn = true)
    {
        if (nowData == value && !isForceTurn)
            return;
        //DebugUtils.LogError($"nowData: {nowData} {value}");
       _SetData(value, callBack);
    } 
    private void _SetData(float value, Action callBack = null)
    {
        nowData = value;
        char[] charArray = nowData.ToString(format).ToCharArray();
        List<char> dataStr = new List<char>(charArray);
        // dataStr.Reverse();  //数组方向
        SetNumb(dataStr, callBack);
    }


    void SetNumb(List<char> dataStr, Action callBack = null)
    {
        /*for (int i = dataStr.Count; i < glstOwnerReels.numChildren; i++)
        {
            glstOwnerReels.GetChildAt(i).visible =  false;
        }*/

        while (glstOwnerReels.numChildren > dataStr.Count)
        {
            if (miniReels.Count > glstOwnerReels.numChildren - 1)
            {
                miniReels[glstOwnerReels.numChildren - 1].Stop();
                MiniReel item = miniReels[glstOwnerReels.numChildren - 1];
                miniReels.RemoveAt(glstOwnerReels.numChildren - 1);

                //#xuhen#多语言对象残留#
                GComponent goReel = item.goOwnerReel;
                GOResidualMark grm = goReel.displayObject.gameObject.GetOrAddComponent<GOResidualMark>();
                grm.InitParam(goReel);
                grm.referenceCount--;
                //DebugUtils.LogError($"{Time.unscaledTime}: MiniReel 动态删除: name ={name}-{guid}  {goReel.id}  -  {goReel.displayObject.id} - {goReel.displayObject.gameObject.name}");
                //goReel.Dispose();
            }
            glstOwnerReels.RemoveChildAt(glstOwnerReels.numChildren-1);
        }

        if (glstOwnerReels.numChildren < dataStr.Count)
        {
            // 初始化渲染逻辑（绑定数据到item）
            glstOwnerReels.itemRenderer = (int index, GObject obj)=> {
                if(index >= miniReels.Count)
                {
                    for (int i = miniReels.Count; i <= index; i++ )
                    {
                        miniReels.Add(new MiniReel());
                    }
                }
                //DebugUtils.LogError($"itemRenderer = {index}");
                miniReels[index].Init(obj.asCom);
            };
            glstOwnerReels.numItems = dataStr.Count;// 更新列表项数量（关键：触发重新渲染）
        }

        int count = dataStr.Count;
        Action cb = () =>
        {
            if (--count <= 0)
                callBack?.Invoke();
        };

        for (int i = 0; i < dataStr.Count; i++)
        {
            try
            {
                miniReels[i].TurnOrKeepSymbol(dataStr[i].ToString(), cb, 2f);
            }
            catch (Exception e)
            {
                DebugUtils.LogError($"name: {name} - i: {i}  miniReels.Count: {miniReels.Count}");
            }
        }
    }

    public void SetReelWidth(int width)
    {
        for (int i = 0; i < miniReels.Count; ++i)
        {
            miniReels[i].width=width;
        }
    }
}
