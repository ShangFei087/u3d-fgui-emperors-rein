using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace GameCommon
{
    public static class Utils 
    {


        public static string ComputeMD5ForStr(string rawData)
        {
            // 创建一个MD5实例
            using (MD5 md5 = MD5.Create())
            {
                // 将输入字符串转换为字节数组
                byte[] inputBytes = Encoding.ASCII.GetBytes(rawData);

                // 计算哈希值
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // 将哈希值转换为十六进制字符串
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }

                // 返回十六进制字符串
                return sb.ToString();
            }
        }


        public static Color ParseColor(string hexColor = "#cccccc")
        {
            Color color;
            // 尝试解析十六进制颜色
            if (ColorUtility.TryParseHtmlString(hexColor, out color))
            {
                DebugUtils.Log("转换成功: " + color);
            }
            else
            {
                // 解析失败
                DebugUtils.LogError("颜色转换失败，请检查十六进制格式");
            }
            return color;
        }




        /*
        public static bool CheckData(Action onSuccessCallback)
        {
            try
            {
                onSuccessCallback();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }*/


        public static T DeepClone<T>(object obj)
        {
            string str = JsonConvert.SerializeObject(obj);
            return JsonConvert.DeserializeObject<T>(str);
        }
    }
}

