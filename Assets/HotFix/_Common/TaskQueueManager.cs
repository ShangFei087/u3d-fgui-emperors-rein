using GameMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class  TaskQueueInfo
{
    public Queue<Func<Action, bool?>> taskQueue;
    public int endlessLoop = 0;
    public bool isDoTasking = false;
}


/// <summary>
/// 待测试
/// </summary>
public class TaskQueueManager : Singleton<TaskQueueManager>
{

    Dictionary<string, TaskQueueInfo> taskQueueInfos = new Dictionary<string, TaskQueueInfo>();

    public void DoTask(string name, Queue<Func<Action,bool?>> taskQueue = null)
    {
        if (taskQueue != null)
        {
            if (taskQueueInfos.ContainsKey(name))
            {
                DebugUtils.LogError($"重复添加： {name}");
                return;
            }
            taskQueueInfos.Add(name, new TaskQueueInfo()
            {
                taskQueue = taskQueue,
            });
        }

        Action nextFunc = () =>
        {
            if (++taskQueueInfos[name].endlessLoop > 1000)
                DebugUtils.LogError("【Err】FguiPoolManager 死循环");
            DoTask(name, null);
        };

        if (taskQueue == null)
        {
            if (taskQueueInfos[name].taskQueue.Count > 0)
            {
                var task = taskQueueInfos[name].taskQueue.Dequeue();
                bool isBreak = task.Invoke(nextFunc) == true;
                taskQueueInfos.Remove(name);
            }
            else
            {
                taskQueueInfos[name].isDoTasking = false;
                taskQueueInfos[name].endlessLoop = 0;
                taskQueueInfos.Remove(name);
            }
        }
        else
        {
            if (!taskQueueInfos[name].isDoTasking)
            {
                taskQueueInfos[name].isDoTasking = true;
                nextFunc();
            }
        }
    }


    public void DoTask02(string name, Func<Action, bool?>[] taskQueue = null)
    {
        if (taskQueue != null)
        {
            if (taskQueueInfos.ContainsKey(name))
            {
                DebugUtils.LogError($"重复添加： {name}");
                return;
            }
            taskQueueInfos.Add(name, new TaskQueueInfo());
            //taskQueueInfos[name].Add(taskQueue);
            foreach (var task in taskQueue)
            {
                taskQueueInfos[name].taskQueue.Enqueue(task);
            }
        }

        Action nextFunc = () =>
        {
            if (++taskQueueInfos[name].endlessLoop > 1000)
                DebugUtils.LogError("【Err】FguiPoolManager 死循环");
            DoTask02(name, null);
        };

        if (taskQueue == null)
        {
            if (taskQueueInfos[name].taskQueue.Count > 0)
            {
                var task = taskQueueInfos[name].taskQueue.Dequeue();
                bool isBreak = task.Invoke(nextFunc) == true;
                taskQueueInfos.Remove(name);
            }
            else
            {
                taskQueueInfos[name].isDoTasking = false;
                taskQueueInfos[name].endlessLoop = 0;
                taskQueueInfos.Remove(name);
            }
        }
        else
        {
            if (!taskQueueInfos[name].isDoTasking)
            {
                taskQueueInfos[name].isDoTasking = true;
                nextFunc();
            }
        }
    }






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



}

class Test01
{
    void DoTest()
    {
        TaskQueueManager.Instance.DoTask("任务1",
             new Queue<Func<Action,bool?>>(new Func<Action,bool?>[]
             {
                 (next) => {
                     if (true)
                     {
                         return true;  // 跳出任务队列
                     }
                     else
                     {
                          next(); // 同步方法
                         // AsyncFunc(XXX, ()=>{ next();  })  // 异步方法
                     }
                 },
                 (next) => {

                     if (true)
                     {
                         return true;  // 跳出任务队列
                     }
                     else
                     {
                          next(); // 同步方法

                         // AsyncFunc(XXX, ()=>{ next();  })  // 异步方法
                     }
                 },
             })
         );


        TaskQueueManager.Instance.DoTask02("任务1",
             new Func<Action, bool?>[]
             {
                (next) => {
                    if (true)
                    {
                        return true;  // 跳出任务队列
                    }
                    else
                    {
                        next(); // 同步方法
                        // AsyncFunc(XXX, ()=>{ next();  })  // 异步方法
                    }
                },
                (next) => {

                    int count = 3;
                    Action next1 = () =>
                    {
                        if(--count == 0)
                        {
                            next();
                        }
                    };

                    next1(); // 并行
                    next1(); // 并行
                    next1(); // 并行
                    
                    return null;
                },
             }
         );
    }

}