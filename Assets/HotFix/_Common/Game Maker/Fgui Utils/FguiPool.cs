using FairyGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;

public enum TagPoolObject
{
    SymbolHit = 1000,
    SymbolAppear,
    SymbolExpectation,
    SymbolTrigger,
    SymbolGoldEffect,
    SymbolBorder,
    SymbolText,

}

[System.Serializable]  //Inspector窗口可见
public class FguiPool : MonoBehaviour
{

    public Queue<GObject> pool = new Queue<GObject>();

    public int maxCount = 10;

    public Func<GObject> onCreatObject;

    public TagPoolObject gtag = TagPoolObject.SymbolHit;

    public string itemName;

    public string TagVal => GetTagVal((int)gtag); //"{\"tag\":" + (int)tag + "}";  // k:v#k:v#
    public static string GetTagVal(int tg) => $"{tgPrefix}:{(int)tg}#";

    public const string tgPrefix = "pool_prefab";

    public GObject GetObject()
    {
        GObject item = null;
        if (pool.Count > 0)
        {
            item = pool.Dequeue();
        }
        else
        {
            item = onCreatObject();
        }
        item.visible = true;
        item.displayObject.gameObject.GetOrAddComponent<GOUseMark>().ToUse();
        //item.displayObject.gameObject.SetActive(true);

        item.data += TagVal;  


        return item;
    }

    /// <summary>
    /// 预加载
    /// </summary>
    public void PreLoad()
    {
        while (pool.Count < maxCount)
        {
            GObject item = onCreatObject();
            pool.Enqueue(item);
        }
    }

    public  void ReturnToPool(GObject item)
    {
        // 从父容器移除
        item.RemoveFromParent();
        item.visible = false;

        if (pool.Count < maxCount)
        {
            pool.Enqueue(item);
        }
        else
        {
            GameCommon.FguiUtils.DeleteWrapperTarget(item.asCom);
            item.Dispose();
        }
    }


    /// <summary>
    /// 清理所有对象池
    /// </summary>
    public void ClearAll()
    {
        foreach (var item in pool)
        {
            item.Dispose();
        }
        pool.Clear();
    }

}
