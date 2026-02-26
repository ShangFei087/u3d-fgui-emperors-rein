//using cfg.lan;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using ZXing.Common;
using ZXing;

public static partial class Utils
{
    static System.Random rand;
    public static void SetRandomSeed()
    {
        rand = new System.Random((int)System.DateTime.Now.Ticks);
    }
    public static int GetRandom(int from, int to)
    {
        return rand.Next(from, to);
    }

    public static byte[] ToByteArray(string str)
    {
        byte[] send = System.Text.Encoding.UTF8.GetBytes(str);
        byte[] old = send;
        send = new byte[old.Length + 5];
        System.BitConverter.GetBytes(old.Length + 1).CopyTo(send, 0);
        send[4] = 0;
        old.CopyTo(send, 5);
        return send;
    }

    public static string LocalIP()
    {
        string AddressIP = string.Empty;
        string IP = "";
        IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());   //Dns.GetHostName()获取本机名Dns.GetHostAddresses()根据本机名获取ip地址组
        foreach (IPAddress ip in ips)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                IP = ip.ToString();  //ipv4
            }
        }
        return IP;
    }

    /// <summary>
    /// 指定Post地址使用Get 方式获取全部字符串
    /// </summary>
    /// <param name="url">请求后台地址</param>
    /// <param name="dic">key:参数名,value:值</param>
    /// <returns></returns>
    public static string Post(string url, Dictionary<string, string> dic)
    {
        string result = "";
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
        req.Method = "POST";
        req.ContentType = "application/x-www-form-urlencoded";
        #region 添加Post 参数
        StringBuilder builder = new StringBuilder();
        int i = 0;
        foreach (var item in dic)
        {
            if (i > 0)
                builder.Append("&");
            builder.AppendFormat("{0}={1}", item.Key, item.Value);
            i++;
        }
        byte[] data = Encoding.UTF8.GetBytes(builder.ToString());
        req.ContentLength = data.Length;
        using (Stream reqStream = req.GetRequestStream())
        {
            reqStream.Write(data, 0, data.Length);
            reqStream.Close();
        }
        #endregion
        HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
        Stream stream = resp.GetResponseStream();
        //获取响应内容
        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
        {
            result = reader.ReadToEnd();
        }
        return result;
    }

    public static long GetTimeStamp()
    {
        DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
        long timeStamp = (long)(DateTime.Now - startTime).TotalMilliseconds;
        return timeStamp;
    }

    public static int GetEnumLength<T>(T enumType)
    {
        string[] enumLength = Enum.GetNames(enumType.GetType());
        return enumLength.Length;
    }

    /// <summary>
    /// 按长度分割字符串，汉字按一个字符算
    /// </summary>
    /// <param name="SourceString"></param>
    /// <param name="Length"></param>
    /// <returns></returns>
    public static List<string> SplitLength(string SourceString, int Length)
    {
        List<string> list = new List<string>();
        for (int i = 0; i < SourceString.Trim().Length; i += Length)
        {
            if ((SourceString.Trim().Length - i) >= Length)
                list.Add(SourceString.Trim().Substring(i, Length));
            else
                list.Add(SourceString.Trim().Substring(i, SourceString.Trim().Length - i));
        }
        return list;
    }

    public static void FileWriteByCreate(string content, string outFilePath)
    {
        FileStream fs = new FileStream(outFilePath, FileMode.Create);
        StreamWriter sw = new StreamWriter(fs);
        sw.Write(content);
        sw.Flush();
        sw.Close();
        fs.Close();
    }

    public static void SaveObjectToJsonFile<T>(T data, string path)
    {
        TextWriter tw = new StreamWriter(path);
        if (tw == null)
        {
            Debug.LogError("Cannot write to " + path);
            return;
        }

        string jsonStr = JsonConvert.SerializeObject(data);
        tw.Write(jsonStr);
        tw.Flush();
        tw.Close();
    }

    public static T LoadObjectFromJsonFile<T>(string path)
    {
        TextReader reader = new StreamReader(path);
        if (reader == null)
        {
            Debug.LogError("Cannot find " + path);
            reader.Close();
            return default(T);
        }

        T data = JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
        if (data == null)
        {
            Debug.LogError("Cannot read data from " + path);
        }

        reader.Close();
        return data;
    }

    /// <summary>
    /// 生成二维码
    /// 经测试：能生成任意尺寸的正方形
    /// </summary>
    /// <param name="content"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public static Texture2D GenerateQRImageWithColor(string content, Color color, int width = 512, int height = 512)
    {
        BitMatrix bitMatrix;
        Texture2D texture = GenerateQRImageWithColor(content, width, height, color, out bitMatrix);
        return texture;
    }

    /// <summary>
    /// 生成二维码
    /// 经测试：能生成任意尺寸的正方形
    /// </summary>
    /// <param name="content"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    private static Texture2D GenerateQRImageWithColor(string content, int width, int height, Color color, out BitMatrix bitMatrix)
    {
        // 编码成color32
        MultiFormatWriter writer = new MultiFormatWriter();
        Dictionary<EncodeHintType, object> hints = new Dictionary<EncodeHintType, object>();
        //设置字符串转换格式，确保字符串信息保持正确
        hints.Add(EncodeHintType.CHARACTER_SET, "UTF-8");
        // 设置二维码边缘留白宽度（值越大留白宽度大，二维码就减小）
        hints.Add(EncodeHintType.MARGIN, 1);
        hints.Add(EncodeHintType.ERROR_CORRECTION, ZXing.QrCode.Internal.ErrorCorrectionLevel.M);
        //实例化字符串绘制二维码工具
        bitMatrix = writer.encode(content, BarcodeFormat.QR_CODE, width, height, hints);

        // 转成texture2d
        int w = bitMatrix.Width;
        int h = bitMatrix.Height;
        Texture2D texture = new Texture2D(w, h);
        for (int x = 0; x < h; x++)
        {
            for (int y = 0; y < w; y++)
            {
                if (bitMatrix[x, y])
                {
                    texture.SetPixel(y, x, color);
                }
                else
                {
                    texture.SetPixel(y, x, Color.white);
                }
            }
        }
        texture.Apply();
        return texture;
    }
}
