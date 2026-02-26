using FairyGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FguiPoolHelper : MonoBehaviour
{
    Dictionary<string, FguiPool> pools = new Dictionary<string, FguiPool>();

    FguiPool CreatPool(TagPoolObject tg, string name, int maxCount, Func<GObject> onCreatObject)
    {
        GameObject goPool = new GameObject();
        goPool.transform.SetParent(transform);
        goPool.name = GetKey(tg, name);
        FguiPool cmp = goPool.AddComponent<FguiPool>();
        cmp.itemName = name;
        cmp.maxCount = maxCount;
        cmp.onCreatObject = onCreatObject;
        cmp.gtag = tg;
        return cmp;
    }

    public string GetKey(TagPoolObject tp, string name)
    {
        return Enum.GetName(typeof(TagPoolObject), tp) + "#" + name;
    }

    public string GetKey(string name)
    {
        foreach (string key in pools.Keys)
        {
            if (key.EndsWith($"#{name}"))
            {
                return key;
            }
        }
        return null;
    }

    public GObject GetObject(TagPoolObject tp, string name)
    {
        try
        {
            string[] strs = name.Split('/');
            name = strs[strs.Length - 1].Split('.')[0];

            GObject gobj = pools[GetKey(tp, name)].GetObject();
            gobj.name = name;
            return gobj;
        }
        catch (Exception e)
        {
            DebugUtils.LogError("【ERR】" + GetKey(tp, name));
        }
        return null;
    }


    public void ReturnToPool(TagPoolObject tp, string name, GComponent root)
    {
        string tag = FguiPool.GetTagVal((int)tp);
        Func<GObject, bool> where = (gobj) =>
        {
            if (gobj == null || string.IsNullOrEmpty((string)gobj.data)) return false;
            //return ((string)gobj.data) == tag;
            return ((string)gobj.data).Contains(tag);
        };
        List<GObject> items = FguiUtils.GetAllNode(root, where);

        foreach (GObject item in items)
        {
            pools[GetKey(tp, name)].ReturnToPool(item);
        }
    }


    public void ReturnToPool(TagPoolObject tp, GComponent root, string[] exclude)
    {
        string tag = FguiPool.GetTagVal((int)tp);
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
            pools[GetKey(tp, name)].ReturnToPool(item);
        }
    }

    public void ReturnToPool(TagPoolObject tp, GComponent root)
    {
        string tag = FguiPool.GetTagVal((int)tp);
        Func<GObject, bool> where = (gobj) =>
        {
            if (gobj == null || string.IsNullOrEmpty((string)gobj.data)) return false;
            return ((string)gobj.data).Contains(tag);
        };
        List<GObject> items = FguiUtils.GetAllNode(root, where);

        foreach (GObject item in items)
        {
            pools[GetKey(tp, item.name)].ReturnToPool(item);
        }
    }

    public void ReturnAllToPool(GComponent root, string[] exclude)
    {

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
            return dataStr.Contains(FguiPool.tgPrefix);
        };
        List<GObject> items = FguiUtils.GetAllNode(root, where);


        foreach (GObject item in items)
        {
            try
            {
                pools[GetKey(item.name)].ReturnToPool(item);
            }
            catch (Exception e)
            {
                DebugUtils.LogError($" {item==null} ");
                DebugUtils.LogError($" {item==null} item.name:{item.name} GetKey: {GetKey(item.name)}");
                throw e;
            }
        }

    }


    public void ClearAll()
    {
        int idx = pools.Count;

        while (--idx >= 0)
        {
            KeyValuePair<string, FguiPool> kv =  pools.ElementAt(idx);
            kv.Value.ClearAll();
            pools.Remove(kv.Key);
        }
    }

    public void CreatPoolFuntion(TagPoolObject tp, string path, string tags = "" , int maxCount = 10)
    {
        string[] strs = path.Split('/');
        string name = strs[strs.Length - 1].Split('.')[0];

        pools.Add(GetKey(tp, name), CreatPool(tp, name, maxCount, () =>
        {
            GComponent comp = UIPackage.CreateObject(pkgName, resName).asCom;
            comp.displayObject.gameObject.GetOrAddComponent<GOUseMark>().InitParam(tags);

            DoTask((next) =>
            {
                if (clonePrefabs.ContainsKey(path))
                {
                    //DebugUtils.LogWarning($"@@ I am here {path}");
                    GameObject go = GameObject.Instantiate(clonePrefabs[path]);
                    GameCommon.FguiUtils.AddWrapper(comp, go);
                    next();
                }
                else
                {
                    ResourceManager02.Instance.LoadAsset<GameObject>(path,
                    (GameObject clone) =>
                    {
                        //DebugUtils.LogWarning($"@@ Add {path}");
                        clonePrefabs.Add(path, clone);

                        GameObject go = GameObject.Instantiate(clone);
                        GameCommon.FguiUtils.AddWrapper(comp, go);
                        next();
                    });
                }
            });
            /*【会导致api并发】
            ResourceManager02.Instance.LoadAsset<GameObject>(path,
            (GameObject clone) =>
            {
                GameObject go = GameObject.Instantiate(clone);

                GameCommon.FguiUtils.AddSpine(comp, go);
            });
            */
                
            
            /* [k:v#k:v#]
             symbol:appear#pool:1#  border#pool:2#  symbol:hit#pool:3  
             expectation:reel#pool:2# 
             */
            if (tags==null) tags = "";
            if (!string.IsNullOrEmpty(tags) && !tags.EndsWith("#"))
                tags += "#";
            comp.data = tags;
            return comp;
        }));
    }

    
    Dictionary<string, GameObject> clonePrefabs = new Dictionary<string, GameObject>();
    
    Queue<Action<Action>> taskQueue = new Queue<Action<Action>>();
    bool isDoTasking = false;
    int endlessLoop = 0;
    void DoTask(Action<Action> taskFunc = null)
    {
        Action nextFunc = () =>
        {
            if (++endlessLoop > 1000)
                DebugUtils.LogError("【Err】FguiPoolManager 死循环");
            DoTask(null);
        };

        if (taskFunc == null)
        {
            if (taskQueue.Count > 0)
            {
                var task = taskQueue.Dequeue();
                task.Invoke(nextFunc);
            }
            else
            {
                isDoTasking = false;
                endlessLoop = 0;
            }
        }
        else
        {
            taskQueue.Enqueue(taskFunc);
            if (!isDoTasking)
            {
                isDoTasking = true;
                nextFunc();
            }
        }
    }

    string pkgName = "Common"; //"EmperorsRein";
    string resName = "AnchorRootDefault";//"AnchorSymbolEffect";
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tp"></param>
    /// <param name="pathLst"></param>
    /// <param name="tags"> "" 或者 "symbol_hit#" 或者 "border#AA:BB#" </param>
    /// <param name="maxCount"></param>
    public void Add(TagPoolObject tp, List<string> pathLst, string tags = "", int maxCount=10)
    {
        foreach (string nameOrPath in pathLst)
        {
            Add(tp, nameOrPath,tags,maxCount);
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tp"></param>
    /// <param name="path"></param>
    /// <param name="tags"> "" 或者 "symbol_hit#" 或者 "border#AA:BB#" </param>
    /// <param name="maxCount"></param>
    public void Add(TagPoolObject tp, string path, string tags = "", int maxCount=10)
    {
        CreatPoolFuntion(tp, path,tags,maxCount);
    }
    
    public void PreLoad( TagPoolObject tp)
    {
        string tagName = Enum.GetName(typeof(TagPoolObject), tp) + "#";
        foreach (KeyValuePair<string, FguiPool> kv in pools)
        {
            if (kv.Key.StartsWith(tagName))
            {
                kv.Value.PreLoad();
            }
        }            
    }
    
    public void PreLoad(TagPoolObject tp, string nameOrPath)
    {
        string[] strs = nameOrPath.Split('/');
        nameOrPath = strs[strs.Length - 1].Split('.')[0];
        string key = GetKey(tp, name);
        if (pools.ContainsKey(key))
        {
            pools[key].PreLoad();
        }
    }
}

