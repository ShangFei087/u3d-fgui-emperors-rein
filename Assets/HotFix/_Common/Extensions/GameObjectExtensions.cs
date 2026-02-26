using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions
{
    /// <summary>
    /// 获取组件，如果不存在则添加并返回新组件
    /// </summary>
    /// <typeparam name="T">要获取或添加的组件类型</typeparam>
    /// <param name="gameObject">目标游戏对象</param>
    /// <returns>获取到或新添加的组件</returns>
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        // 尝试获取组件
        T component = gameObject.GetComponent<T>();

        // 如果组件不存在，则添加一个新的
        if (component == null)
        {
            component = gameObject.AddComponent<T>();
        }

        return component;
    }

    /// <summary>
    /// 通过类型获取组件，如果不存在则添加并返回新组件
    /// </summary>
    /// <param name="gameObject">目标游戏对象</param>
    /// <param name="type">要获取或添加的组件类型</param>
    /// <returns>获取到或新添加的组件</returns>
    public static Component GetOrAddComponent(this GameObject gameObject, Type type)
    {
        if (!typeof(Component).IsAssignableFrom(type))
        {
            throw new ArgumentException("类型必须是Component的派生类型", nameof(type));
        }

        // 尝试获取组件
        Component component = gameObject.GetComponent(type);

        // 如果组件不存在，则添加一个新的
        if (component == null)
        {
            component = gameObject.AddComponent(type);
        }

        return component;
    }
}
