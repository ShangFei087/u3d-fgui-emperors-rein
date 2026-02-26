/**
 * @file    
 * @author  Huang Wen <Email:ww1383@163.com, QQ:214890094, WeChat:w18926268887>
 * @version 1.0
 *
 * @section LICENSE
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 *
 * @section DESCRIPTION
 *
 * This file is ...
 */
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace Hal
{
    public static class SBoxIOEvent
    {
        public delegate void EventCallback(SBoxPacket packet);
        static Dictionary<int, EventCallback> EventDictionary = new Dictionary<int, EventCallback>();

        /**
          *  @brief          增加事件回调，只能加一个EventId的回调，重复的加不进去
          *  @param[in]      EventId 事件ID
          *  @param[in]      Callback 事件回调
          *  @return         无
          *  @details        
          */
        public static void AddListener(int EventId, EventCallback Callback)
        {
            if (EventDictionary.ContainsKey(EventId))
            {

            }
            else
            {
                EventDictionary.Add(EventId, Callback);
            }
        }

        /**
          *  @brief          删除事件回调
          *  @param[in]      EventId 事件ID
          *  @return         无
          *  @details        
          */
        public static void RemoveListener(int EventId)
        {
            if (EventDictionary.ContainsKey(EventId))
            {
                EventDictionary.Remove(EventId);
            }
        }

        /**
          *  @brief          触发事件
          *  @param[in]      EventId 事件ID
          *  @param[in]      packet 事件回调的参数
          *  @return         无
          *  @details        
          */
        public static void SendEvent(int EventId, SBoxPacket packet)
        {
            EventCallback Callback;

            //if (EventId != 20001
            //    && EventId != 20021
            //    && EventId != 20040
            //    && EventId != 20053
            //    && EventId != 40368
            //    && EventId != 40001)
            //    Debug.LogWarning($"<color=#FF8000>SBoxRead:{JsonConvert.SerializeObject(packet)}</color>");

            if (EventDictionary.TryGetValue(EventId, out Callback))
            {
                Callback(packet);
            }
        }
    }
}

