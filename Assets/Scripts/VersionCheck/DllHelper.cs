using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Linq;

//主工程使用
public class DllHelper
{
    private static DllHelper _instance;
    public static DllHelper Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new DllHelper();
            }
            return _instance;
        }
    }

    /// <summary> 需要热更的dll (排序先后有要求)</summary>
    public  List<string> DllNameList = new List<string>() { "Base", "HotFix" };

    /// <summary> 
    /// 针对后期热更dll个数变多的情况
    /// </summary>
    public List<string> GetDllNameList(JObject node)
    {
        List<string> lst = DllNameList;
        try
        {
            if (node.ContainsKey("hotfix_dll_load_order"))
            {
                JArray arr = node["hotfix_dll_load_order"] as JArray;
                if (arr.Count > 0)
                {
                    lst = new List<string>();
                    foreach (JToken item in arr)
                    {
                        lst.Add(item.ToObject<string>());
                    }
                }
            }

        }
        catch (Exception ex)
        {
            Debug.LogError($"节点hotfix_dll_load_order 解析错误：{node.ToString()}");
        }

        return lst;
    }



    private Dictionary<string, Assembly> DllDic = new Dictionary<string, Assembly>();
    public void SethotUpdateAss(string name, Assembly ass)
    {
        if (!DllDic.ContainsKey(name))
        {
            DllDic.Add(name, ass);
        }
    }

    public Assembly GetAss(string AssName)
    {
        if (DllDic.ContainsKey(AssName))
        {
            return DllDic[AssName];
        }
        return null;
    }

}
