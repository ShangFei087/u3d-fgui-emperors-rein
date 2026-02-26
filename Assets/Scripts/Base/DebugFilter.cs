using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DebugFilter  
{
    static bool isFilter = true;

    static List<string> targetToFilter = new List<string>()
    {
        "RpcNameIsCodingActive", // 激活
        "SBOX_IDEA_INFO", // 过滤20001
        "ON_WIN_EVENT", // 赢线循环显示
        "【OrderReship】", //订单补发
#if UNITY_EDITOR
       "【UDP-WS】UDP/C2S : {\"IP\"",
#endif
    };

    public static bool CheckFilter(object obj)
    {
        if(obj is string content)
        {
            foreach (string item in targetToFilter)
            {
                if (content.Contains(item))
                    return true;
            }
        }
        return false;
    }
}
