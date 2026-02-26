using SpringGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

class IViewManager : Singleton<IViewManager>
{

#if false
    public T GetIV<T>() where T : class
    {
        string ivName = typeof(T).Name;

        if (ivs.ContainsKey(ivName))
            return (T)( ivs[ivName]() );
        return null;
    }

    Dictionary<string, Func<object>> ivs = new Dictionary<string, Func<object>>()
    {
        [nameof(IVCalendar)] = ()=> new CalendarView(),
    };

#else

    public T GetIV<T>() where T : class
    {
        string ivName = typeof(T).Name;

        if (_ivs.ContainsKey(ivName))
        {
            object newInstance = Activator.CreateInstance(_ivs[ivName]);
            return newInstance as T;
        }
        return null;
    }

    Dictionary<string, Type> _ivs = new Dictionary<string, Type>()
    {
        [nameof(IVCalendar)] = typeof(CalendarView),
    };
#endif
}