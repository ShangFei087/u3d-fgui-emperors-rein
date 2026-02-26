using System.Collections.Generic;
using UnityEngine.Events;
using GameMaker;
using UnityEngine;
using System;
using Newtonsoft.Json;
public interface IEventInfo
{
    Type parameterType { get; } // 定义为只读属性
}
public class EventInfo<T> : IEventInfo
{
    public Type parameterType => _parameterType;
    Type _parameterType;  // 存储事件参数类型

    //public UnityAction<T> actions;  //【？？】 这里会爆空

    public UnityAction<T> actions = delegate { };  
    public EventInfo(UnityAction<T> action)
    {
        _parameterType = typeof(T);
        actions += action;
    }
}
public class EventInfo : IEventInfo
{
    public Type parameterType => null;

    //public UnityAction actions;    //【？？】 这里会爆空

    public UnityAction actions = delegate { };
    public EventInfo(UnityAction action)
    {
        actions += action;
    }
}
/// <summary>
/// 事件中心 单例模式对象
/// 1.Dictionary
/// 2.委托
/// 3.观察者设计模式
/// 4.泛型
/// </summary>
public class EventCenter
{
    private static EventCenter instance;
    public static EventCenter Instance
    {
        get
        {
            if (instance == null)
                instance = new EventCenter();
            return instance;
        }
    }


    //key —— 事件的名字（比如：怪物死亡，玩家死亡，通关 等等）
    //value —— 对应的是 监听这个事件 对应的委托函数们
    private Dictionary<string, IEventInfo> eventDic = new Dictionary<string, IEventInfo>();




    /// <summary>
    /// 添加事件监听
    /// </summary>
    /// <param name="name">事件的名字</param>
    /// <param name="action">准备用来处理事件 的委托函数</param>
    public void AddEventListener<T>(string name, UnityAction<T> action)
    {
        Type eventType = typeof(T);

        if (eventDic.ContainsKey(name))
        {
            var existingEvent = eventDic[name];

            // 检查现有事件参数类型与传入类型的兼容性
            if (existingEvent is EventInfo<T>)
            {
                // 类型完全匹配，直接添加
                (existingEvent as EventInfo<T>).actions += action;
            }
            else if (existingEvent.parameterType.IsAssignableFrom(eventType))
            {

                /*
                // 现有类型是传入类型的基类，可以安全转换
                // 创建适配器将子类参数转换为基类参数 （这样做会导致参数数据缺失（子类比基类多的哪些字段缺失数据），而却方法不好注销）
                UnityAction<T> adaptedAction = (arg) => {
                    (existingEvent as dynamic).actions(arg);
                };

                (existingEvent as dynamic).actions += adaptedAction;  
                */

                // 传入类型是现有类型的基类，需要特殊处理
                //Debug.LogError($"无法添加事件: {name} - 传入的参数类型 {eventType} 是已注册类型 {existingEvent.parameterType} 的子类，可能导致类型安全问题");
                return;
            }
            else if (eventType.IsAssignableFrom(existingEvent.parameterType))
            {
                // 传入类型是现有类型的基类，需要特殊处理
               // Debug.LogError($"无法添加事件: {name} - 传入的参数类型 {eventType} 是已注册类型 {existingEvent.parameterType} 的基类，可能导致类型安全问题");
                return;
            }
            else
            {
               // Debug.LogError($"无法添加事件: {name} - 参数类型不兼容 {eventType} vs {existingEvent.parameterType}");
                return;
            }
        }
        //没有的情况
        else
        {
            eventDic.Add(name, new EventInfo<T>(action));
        }
    }

    /// <summary>
    /// 监听不需要参数传递的事件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="action"></param>
    public void AddEventListener(string name, UnityAction action)
    {
        //有没有对应的事件监听
        //有的情况
        if (eventDic.ContainsKey(name))
        {
            (eventDic[name] as EventInfo).actions += action;
        }
        //没有的情况
        else
        {
            eventDic.Add(name, new EventInfo(action));
        }
    }


    /// <summary>
    /// 移除对应的事件监听
    /// </summary>
    /// <param name="name">事件的名字</param>
    /// <param name="action">对应之前添加的委托函数</param>
    public void RemoveEventListener<T>(string name, UnityAction<T> action)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as EventInfo<T>).actions -= action;
    }

    /// <summary>
    /// 移除不需要参数的事件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="action"></param>
    public void RemoveEventListener(string name, UnityAction action)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as EventInfo).actions -= action;
    }


#if false
    /// <summary>
    /// 【Bug】事件触发
    /// </summary>
    /// <param name="name">哪一个名字的事件触发了</param>
    public void EventTrigger02<T>(string name, T info)
    {

#if UNITY_EDITOR || true
        try
        {
            string data = typeof(T).IsClass ? JsonConvert.SerializeObject(info) : $"{info}";
            DebugUtils.Log($"【EventCenter】 name：{name} info：{data}");
        }
        catch (Exception e)
        {
            DebugUtils.LogWarning($"【EventCenter】 name：{name} data error");
        }
#endif
        if (eventDic.ContainsKey(name))
        {
            var eventInfo = eventDic[name];

            // 检查类型兼容性
            if (eventInfo is EventInfo<T> )
            {
                (eventInfo as EventInfo<T>).actions?.Invoke(info);
            }
            else if (typeof(T).IsSubclassOf(eventInfo.parameterType)) 
            {
                // 子类参数可以安全转换为基类参数
                var baseTypeEvent = eventInfo as dynamic;   // 这里有问题！！！
                baseTypeEvent.actions?.Invoke((dynamic)info);
            }
            else
            {
                DebugUtils.LogError($"无法触发事件: {name} - 参数类型不兼容 {typeof(T)} vs {eventInfo.parameterType}");
            }

            //DebugUtils.Log("1 = " + typeof(T).IsSubclassOf(eventInfo.parameterType)); // 子列
            //DebugUtils.Log("2 = " + typeof(T).IsAssignableFrom(eventInfo.parameterType)); // 用于检查一个类型是否可以被赋值给另一个类型
            //DebugUtils.Log("3 = " + eventInfo.parameterType.IsAssignableFrom(typeof(T))); // eventInfo.parameterType 是 typeof(T) 的 基类
            //DebugUtils.Log("4 = " + typeof(object).IsAssignableFrom(eventInfo.parameterType));  // object 
            //DebugUtils.Log("5 = " + typeof(object).IsSubclassOf(eventInfo.parameterType));  // object 
            //DebugUtils.Log("6 = " + eventInfo.parameterType.IsSubclassOf(typeof(object)));
        }
    }

#endif



    /// <summary>
    /// 事件触发
    /// </summary>
    /// <param name="name">哪一个名字的事件触发了</param>
    public void EventTrigger<T>(string name, T info)
    {

#if UNITY_EDITOR || true
        try
        {
            string data = typeof(T).IsClass ? JsonConvert.SerializeObject(info) : $"{info}";
            DebugUtils.Log($"【EventCenter】 name：{name} info：{data}");
        }
        catch (Exception e)
        {
            DebugUtils.LogWarning($"【EventCenter】 name：{name} data error");
        }
#endif

        //有没有对应的事件监听
        //有的情况
        if (eventDic.ContainsKey(name))
        {

            EventInfo<T> req = eventDic[name] as EventInfo<T>;
            if (req == null)
            {
                // EventData 类型 被自动识别为 EventData<T> 
                DebugUtils.LogError($"event name:{name}; EventTrigger<T> 方法没有 填写<T>; 例如： EventTrigger<EventData> 类型 被自动识别为 EventTrigger<EventData<T>>");
                /*
                 * EventInfo<T> req = eventDic[name] as EventInfo<T>;
                 * 【监听父类 , 用子类作为 EventTrigger<T>的T,来发送信息时就会报错！】

                 EventCenter.Instance.AddEventListener<EventData>("123", func); // 监听父类
                 EventCenter.Instance.EventTrigger<EventData>("123", new EventData<string>("1", "2"));   // 能用
                 EventCenter.Instance.EventTrigger<EventData<string>>("123", new EventData<string>("1", "2"));   //报错

                */
            }
            if (req.actions != null)
                req.actions.Invoke(info);
        }
    }






    /// <summary>
    /// 事件触发（不需要参数的）
    /// </summary>
    /// <param name="name"></param>
    public void EventTrigger(string name)
    {
        //有没有对应的事件监听
        //有的情况
        if (eventDic.ContainsKey(name))
        {
            if ((eventDic[name] as EventInfo).actions != null)
                (eventDic[name] as EventInfo).actions.Invoke();
        }
    }
    /// <summary>
    /// 清空事件中心
    /// 主要用在 场景切换时
    /// </summary>
    public void Clear()
    {
        eventDic.Clear();
    }
}
