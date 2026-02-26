using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using GameUtil;
using GameMaker;
using SimpleJSON;

public class Observer
{
    string clsName;

    public const string ON_PROPERTY_CHANGED_EVENT = "ON_PROPERTY_CHANGED_EVENT";
    public Observer(string clsName)
    {
        this.clsName = clsName;
    }
    public bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
    {
        if (Equals(storage, value)) return false;
        storage = value;

        /*
        try
        {
            DebugUtils.Log($"{clsName}/{propertyName} - {JsonConvert.SerializeObject(value)}");
        }
        catch (Exception ex)
        {        
            DebugUtils.Log($"{clsName}/{propertyName} - {value}");
        }*/

        /*Messenger.Broadcast<T>(Messenger.EventEnum.Logic, ON_PROPERTY_CHANGED_EVENT,
         new EventData<object>($"{clsName}/{propertyName}", value));
        */

        EventCenter.Instance.EventTrigger<EventData>(ON_PROPERTY_CHANGED_EVENT,
             new EventData<object>($"{clsName}/{propertyName}", value));
        return true;
    }


    private Dictionary<string, Action> checkProperty = new Dictionary<string, Action>();
    public T GetProperty<T>(System.Func<T> getStorage, [CallerMemberName] String propertyName = null)
    {
        if (!checkProperty.ContainsKey(propertyName))
        {
            string last = JsonConvert.SerializeObject(getStorage());
            Action checkFunc = () =>
            {
                checkProperty.Remove(propertyName);
                if (last != JsonConvert.SerializeObject(getStorage()))
                    /*Messenger.Broadcast<T>(Messenger.EventEnum.Logic, ON_PROPERTY_CHANGED_EVENT,
                     new EventData<object>($"{clsName}/{propertyName}", getStorage()));
                    */
                    EventCenter.Instance.EventTrigger<EventData>(ON_PROPERTY_CHANGED_EVENT,
                         new EventData<object>($"{clsName}/{propertyName}", getStorage()));
            };
            checkProperty.Add(propertyName, checkFunc);

            Timer.DelayAction(1f, () =>
            {
                if (checkProperty.ContainsKey(propertyName))
                {
                    checkProperty[propertyName].Invoke();
                }
            });
        }
        return getStorage();
    }

    public void checkImmediately()
    {
        List<string> keys = checkProperty.Keys.ToList();
        for (int i = 0; i < keys.Count; i++)
            checkProperty[keys[i]].Invoke();
    }
}
