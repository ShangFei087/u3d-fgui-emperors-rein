using System;
using System.Collections.Generic;
using UnityEngine;

public class TaskQueueHelper
{
    private readonly  object _lockObject  = new object();
    
    Queue<Action<Action>> taskQueue = new Queue<Action<Action>>();
    bool isDoTasking = false;
    int endlessLoop = 0;

    private int idx = 0;
    public void DoTask(Action<Action> taskFunc = null)
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
                //DebugUtils.LogError($" i am nextFunc finish !  idx: {++idx}");
            }
        }
        else
        {
            //时间延时？
            taskQueue.Enqueue(taskFunc);
#if true
            if (!isDoTasking)
            {
                isDoTasking = true;
                //DebugUtils.LogError($" i am nextFunc ! idx: {++idx}");
                nextFunc();
            }     
#else
            lock (_lockObject )
            {
                if (!isDoTasking)
                {
                    isDoTasking = true;
                    //DebugUtils.LogError($" i am nextFunc ! idx: {++idx}");
                    nextFunc();
                }                
            }
#endif
        }
    }
    
    
    
    
    
    
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
    
    
    
    
}
