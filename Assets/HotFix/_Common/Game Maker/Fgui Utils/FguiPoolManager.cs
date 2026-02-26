using PusherEmperorsRein;
using FairyGUI;
using System;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]  //Inspector���ڿɼ�
public class FguiPoolManager : MonoSingleton<FguiPoolManager>
{
     Dictionary<string, FguiPool> pools = new Dictionary<string, FguiPool>();

    FguiPool CreatPool(TagPoolObject tg ,string name, int maxCount , Func<GObject> onCreatObject)
    {
        GameObject goPool = new GameObject();
        goPool.transform.SetParent(transform);
        goPool.name = GetKey(tg,name);
        FguiPool cmp = goPool.AddComponent<FguiPool>();
        cmp.itemName = name;
        cmp.maxCount = maxCount;
        cmp.onCreatObject = onCreatObject;
        cmp.gtag = tg;
        return cmp;
    }

    public string GetKey(TagPoolObject tp, string name)
    {
        return  Enum.GetName(typeof(TagPoolObject), tp) + "#" + name;
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
        catch(Exception e)
        {
            DebugUtils.LogError("��ERR��"+GetKey(tp, name));
        }
        return null;
    }


    public void ReturnToPool(TagPoolObject tp,string name,GComponent root)
    {
        string tag = FguiPool.GetTagVal((int)tp);
        Func<GObject, bool> where = (gobj) => {
            if (gobj == null || string.IsNullOrEmpty((string)gobj.data))  return false;
            return ((string)gobj.data) == tag;
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
        Func<GObject, bool> where = (gobj) => {
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
        Func<GObject, bool> where = (gobj) => {
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
        Func<GObject, bool> where = (gobj) => {
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
            pools[GetKey(item.name)].ReturnToPool(item);
        }
    }

    public void CreatPoolFuntion(TagPoolObject tp,string path, string tags = "")
    {
        string[] strs = path.Split('/');
        string name = strs[strs.Length - 1].Split('.')[0];

        pools.Add(GetKey(tp, name), CreatPool(tp, name, 10, () =>
        {
            GComponent comp = UIPackage.CreateObject("EmperorsRein", "AnchorSymbolEffect").asCom;

            DoTask((next) =>
            {
                if (clonePrefabs.ContainsKey(path))
                {
                    DebugUtils.LogWarning($"@@ I am here {path}");
                    GameObject go = GameObject.Instantiate(clonePrefabs[path]);
                    GameCommon.FguiUtils.AddWrapper(comp, go);
                    next();
                }
                else
                {
                    ResourceManager02.Instance.LoadAsset<GameObject>(path,
                    (GameObject clone) =>
                    {
                        DebugUtils.LogWarning($"@@ Add {path}");
                        clonePrefabs.Add(path, clone);  

                        GameObject go = GameObject.Instantiate(clone);
                        GameCommon.FguiUtils.AddWrapper(comp, go);
                        next();
                    });
                }
            });
            /*���ᵼ��api������
            ResourceManager02.Instance.LoadAsset<GameObject>(path,
            (GameObject clone) =>
            {
                GameObject go = GameObject.Instantiate(clone);

                GameCommon.FguiUtils.AddSpine(comp, go);
            });
            */

            comp.data = tags;
            return comp;
        }));
    }

    

    Dictionary<string,GameObject> clonePrefabs = new Dictionary<string,GameObject>();


#if false
    Queue<Func<Task>> taskQueue02 = new Queue<Func<Task>>();
    async void DoTask02(Func<Task> taskFunc)
    {
        if (taskFunc != null)
            taskQueue02.Enqueue(taskFunc);
        if (taskQueue02.Count > 0)
        {
            Func<Task> task = taskQueue02.Dequeue();
            await task.Invoke();
            DoTask02(null);
        }
    }
    void Test02(string path)
    {
        DoTask02(async () =>
        {
            if (clonePrefabs.ContainsKey(path))
            {
                GameObject go = GameObject.Instantiate(clonePrefabs[path]);
            }
            else
            {
                var tcs = new TaskCompletionSource<object[]>();

                ResourceManager02.Instance.LoadAsset<GameObject>(path,
                (GameObject clone) =>
                {
                    clonePrefabs.Add(path, clone);
                    GameObject go = GameObject.Instantiate(clone);

                    tcs.SetResult(new object[] {0});
                });
                await tcs.Task;
            }

        });
    }
#endif


    Queue<Action<Action>> taskQueue = new Queue<Action<Action>>();
    bool isDoTasking = false;
    int endlessLoop = 0;
    void DoTask(Action<Action> taskFunc = null)
    {
        Action nextFunc = () =>
        {
            if (++endlessLoop > 1000)
                DebugUtils.LogError("��Err��FguiPoolManager ��ѭ��");
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


    private void Awake()
    {
        foreach (KeyValuePair<string,string>  kv in CustomModel.Instance.symbolHitEffect)
        {
            CreatPoolFuntion(TagPoolObject.SymbolHit, kv.Value);
        }

        foreach (KeyValuePair<string, string> kv in CustomModel.Instance.symbolAppearEffect)
        {
            CreatPoolFuntion(TagPoolObject.SymbolAppear, kv.Value);
        }
    }

}
