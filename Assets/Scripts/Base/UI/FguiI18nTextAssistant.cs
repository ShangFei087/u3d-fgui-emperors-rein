using FairyGUI;
using FairyGUI.Utils;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public class FguiI18nTextInfo
{
    public string id;
    public string mz;
    public string data;
}
public class FguiI18nTextAssistant
{
    // 线程安全锁
    private static object _mutex = new object();
    static FguiI18nTextAssistant _instance;
    public static FguiI18nTextAssistant Instance
    {
        get
        {
            lock (_mutex)
            {
                if (_instance == null)
                {
                    _instance = new FguiI18nTextAssistant();
                    _instance.name = UnityEngine.Random.Range(1, 10).ToString();
                }
                return _instance;
            }
        }
    }
    string name;

    public static Dictionary<string, List<FguiI18nTextInfo>> strings;

    public  void LoadFromXML(XML source)
    {
        strings = new Dictionary<string, List<FguiI18nTextInfo>>();
        XMLList.Enumerator et = source.GetEnumerator("string");
        while (et.MoveNext())
        {
            XML cxml = et.Current;
            string mz = cxml.GetAttribute("mz");
            string key = cxml.GetAttribute("name");
            string text = cxml.text;
            int i = key.IndexOf("-");
            if (i == -1)
                continue;

            string pkgId = key.Substring(0, i);
            string key3 = key.Substring(i + 1);

            FguiI18nTextInfo info = new FguiI18nTextInfo()
            {
                id = key3,
                mz = mz,
                data = text,
            };


            List<FguiI18nTextInfo> col;
            if (!strings.TryGetValue(pkgId, out col))
            {
                col = new List<FguiI18nTextInfo>();
                strings[pkgId] = col;
            }
            col.Add(info);
        }

        //Debug.LogWarning($"1 == name: {name} strings: {JsonConvert.SerializeObject(strings)}");
    }


    public void TranslateComponent(GComponent contentPane)
    {
        
        PackageItem pi = UIPackage.GetItemByURL(contentPane.resourceURL);
        //Debug.Log($"{pi.owner.id}{pi.id}  -- {contentPane.resourceURL}");
        string pkgId = $"{pi.owner.id}{pi.id}";

        //Debug.LogWarning($"2 ==  name: {name} pkgId: {pkgId}");

        if (strings.ContainsKey(pkgId))
        {
            List<GObject> childs = new List<GObject>();  // 已经修改过的子元件

            foreach(FguiI18nTextInfo info in strings[pkgId])
            {
                string[] str = info.id.Split('-');

                if (str.Length > 1)
                {
                    string endStr = str[str.Length-1];
                    int index;

                    //[注意]： GRichTextField : GTextField

                    try
                    {
                        if (int.TryParse(endStr, out  index))
                        {
                            GObject chd = contentPane.GetChild(info.mz);
                            childs.Add(chd);

                            // GList   Debug.Log($"{index}");
                            (((chd as GList).GetChildAt(index) as GComponent).GetChild("title") as GTextField).text = info.data;
                        }
                        else
                        {
                            // 获取子级对象时，不用管fgui的“组合”
                            GObject chd = contentPane.GetChild(info.mz);
                            childs.Add(chd);

                            //自定义属性：(这写法有bug)
                            //((chd as GComponent).GetChild(endStr) as GTextField).text = info.data;


                            GObject obj = (chd as GComponent).GetChild(endStr);
                            if (obj is GButton gbtn)
                            {
                                (gbtn.GetChild("title") as GTextField).text = info.data;
                            }
                            else if(obj is GTextField gtxt)
                            {
                                gtxt.text = info.data;
                            }
                            else if (obj is GRichTextField grtxt)
                            {
                                grtxt.text = info.data;
                            }
                            else
                            {
                                Debug.LogError($"报错： {JsonConvert.SerializeObject(info)}"
                                    + $"isNull: {obj is null} isGbtn: {obj is GButton}  isGtxt: {obj is GTextField} " +
                                    $" isGrtxt: {obj is GRichTextField} isGComponent: {obj is GComponent}");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"报错： {JsonConvert.SerializeObject(info)}");
                        throw e;
                    }
                }
                else
                {
                    try
                    {

                        GObject chd = contentPane.GetChild(info.mz);
                        childs.Add(chd);

                        if (chd is GTextField)
                        {
                           (chd as GTextField).text = info.data;

                        } else if (chd is GComponent)  //自定义属性：
                        {                        
                            (chd as GComponent).GetChild("title").text = info.data;
                        }
                        else
                        {
                            throw new Exception("异常情况");
                        }

                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"报错： {JsonConvert.SerializeObject(info)}");
                        throw e;
                    }
                }
            }
            

            // 还没有翻译的组件
            for (int i = 0; i < contentPane.numChildren; i++)
            {
                GObject child = contentPane.GetChildAt(i);
                if (child is GList glst && !childs.Contains(child))
                {
                    for (int j = 0; j < glst.numChildren; j++)
                    {
                        // 针对glst 的item都不一样，比如pages
                        TranslateComponent(glst.GetChildAt(j) as GComponent);
                    }
                }
                /*else if (child is GComponent component)
                {
                }|*/
            }


        }
    }



    public void DisposeAllTranslate(GComponent comp)
    {
        List<GComponent> comps = FguiUtils.GetAllNode<GComponent>(comp);
        comps.Insert(0, comp);

        List<PackageItem> pkgItems = new List<PackageItem>();

        foreach (GComponent item in comps)
        {
            if (item.packageItem != null)
            {
                PackageItem pi = item.packageItem.getBranch();
                if (!pkgItems.Contains(pi))
                {
                    pkgItems.Add(pi);
                }
            }
        }

        foreach (PackageItem pi in pkgItems)
        {
            TranslationHelper.TranslateComponent(pi);
        }
    }
}
