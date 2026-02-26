using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using System;

public enum TagPoolGObject
{
    Default = 0,
}
public class FguiGObjectPoolHelper : MonoSingleton<FguiGObjectPoolHelper>
{
    private GObjectPool pool;
    
    string tgPrefix => "pool_gobj";

    void Start()
    {
        pool = new GObjectPool(transform);
    }

    public void ReturnObject(GObject obj) => pool.ReturnObject(obj);
    
    public GObject GetObject(string url) => GetObject(url, TagPoolGObject.Default, "");
    public GObject GetObject(string url, TagPoolGObject tag, string mask)
    {
        DebugUtils.Log(url);
        DebugUtils.Log(pool);
        GObject obj = pool.GetObject(url);

        obj.data = string.IsNullOrEmpty(mask)? $"{tgPrefix}:{(int)tag}#" : $"{tgPrefix}:{(int)tag}#{mask}#";

        return obj;
    }




  

    public void Clear() => pool.Clear();

    
    
    public void ReturnAllToPool(GComponent root, string[] exclude)
    {
        string tag = $"{tgPrefix}:";
        Func<GObject, bool> where = (gobj) =>
        {
            if (gobj == null || string.IsNullOrEmpty((string)gobj.data)) return false;
            string dataStr = ((string)gobj.data);
            foreach (string item in exclude)
            {
                if (dataStr.Contains(item))
                {
                    return false;
                }
            }
            return dataStr.Contains(tag);
        };
        List<GObject> items = FguiUtils.GetAllNode(root, where);

        foreach (GObject item in items)
        {
            item.RemoveFromParent();
            item.visible = false;
            ReturnObject(item) ;
        }
    }
    
    public void ReturnToPool(TagPoolGObject tp, GComponent root)
    {
        string tag = $"{tgPrefix}:{(int)tp}#";
        Func<GObject, bool> where = (gobj) =>
        {
            if (gobj == null || string.IsNullOrEmpty((string)gobj.data)) return false;
            return ((string)gobj.data).Contains(tag);
        };
        List<GObject> items = FguiUtils.GetAllNode(root, where);

        foreach (GObject item in items)
        {
            item.RemoveFromParent();
            item.visible = false;
            ReturnObject(item) ;
        }
    }
}
