using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public class AesManager
{
    private static AesManager instance;


    /// <summary> 原始秘钥 </summary>
    private byte[] localKeyBytes;
    private byte[] localIvBytes;

    /// <summary> 服务器动态修改 </summary>
    private byte[] KeyBytes;
    private byte[] IvBytes;

    private AesManager()
    {
        ResetAesKeyIv();
    }

    public void ResetAesKeyIv()
    {
        string localKey = "a4d58c5f125f2c16dd65edd0d4ab45e2485a95042741a740e5012f88e6e1df64";
        string localIv = "be7369b599f7d5bc2e102a3db2a4bfdd";

        localKeyBytes = StringToByteArray(localKey);
        localIvBytes = StringToByteArray(localIv).Take(16).ToArray();

        initAesKey(localKey);
        initAesIv(localIv);
    }

    public void initAesKey(string key)
    {
        KeyBytes = StringToByteArray(key);
    }

    public void initAesIv(string iv)
    {
        IvBytes = StringToByteArray(iv).Take(16).ToArray();
    }

    public static AesManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new AesManager();
            }
            return instance;
        }
    }


    private string ToMD5(byte[] byteArray)
    {
        string md5String = "";
        // 创建MD5的实例  
        using (MD5 md5 = MD5.Create())
        {
            // 计算MD5哈希值  
            byte[] hash = md5.ComputeHash(byteArray);

            // 将哈希值转换为十六进制字符串  
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            md5String = sb.ToString();

            // 输出MD5哈希值的十六进制字符串  
            //Console.WriteLine(md5String);
        }
        return md5String;
    }

    /// <summary>
    /// 加密
    /// </summary>
    /// <param name="plaintext">明文</param>
    /// <returns></returns>
    public string TryLocalEncrypt(string plaintext)
    {
#if UNITY_EDITOR
        //Console.WriteLine($"TryEncrypt             plainText ====== {plainText} KeyBytes==={localKeyBytes}          IvBytes = {localIvBytes}");
        //Debug.Log($"TryLocalEncrypt    plainText ====== {plainText}  KeyBytes==={ToMD5(localKeyBytes)}    IvBytes = {ToMD5(localIvBytes)}");
#endif
        return UnityEncrypt(plaintext, localKeyBytes, localIvBytes);
    }



    /// <summary>
    /// 解密
    /// </summary>
    /// <param name="ciphertext">密文</param>
    /// <returns></returns>
    public string TryLocalDecrypt(string ciphertext)
    {
#if UNITY_EDITOR
        //Console.WriteLine($"TryDecrypt             cipherText ====== {cipherText} KeyBytes==={localKeyBytes}          IvBytes = {localIvBytes}");
        // Debug.Log($"TryLocalDecrypt    plainText ====== {cipherText}  KeyBytes==={ToMD5(localKeyBytes)}    IvBytes = {ToMD5(localIvBytes)}");
#endif
        return UnityDecrypt(ciphertext, localKeyBytes, localIvBytes);
    }

    public string TryEncrypt(string plainText)
    {
#if UNITY_EDITOR
        //Console.WriteLine($"TryEncrypt             plainText ====== {plainText} KeyBytes==={KeyBytes}          IvBytes = {IvBytes}");
        //Debug.Log($"TryEncrypt    plainText ====== {plainText}  KeyBytes==={ToMD5(KeyBytes)}    IvBytes = {ToMD5(IvBytes)}");
#endif
        return UnityEncrypt(plainText, KeyBytes, IvBytes);
    }

    public string TryDecrypt(string cipherText)
    {
#if UNITY_EDITOR
        //Console.WriteLine($"TryDecrypt             cipherText ====== {cipherText} KeyBytes==={KeyBytes}          IvBytes = {IvBytes}");
        //Debug.Log($"TryDecrypt    plainText ====== {cipherText}  KeyBytes==={ToMD5(KeyBytes)}    IvBytes = {ToMD5(IvBytes)}");
#endif
        return UnityDecrypt(cipherText, KeyBytes, IvBytes);
    }

    public string UnityEncrypt(string plainText, byte[] keyBytes, byte[] ivBytes)
    {
        using (AesManaged aesAlg = new AesManaged())
        {
            aesAlg.Key = keyBytes;
            aesAlg.IV = ivBytes;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                }

                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }

    public string UnityDecrypt(string cipherText, byte[] keyBytes, byte[] ivBytes)
    {
        using (AesManaged aesAlg = new AesManaged())
        {
            aesAlg.Key = keyBytes;
            aesAlg.IV = ivBytes;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }

    private byte[] StringToByteArray(string hex)
    {
        int numberChars = hex.Length;
        byte[] bytes = new byte[numberChars / 2];
        for (int i = 0; i < numberChars; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return bytes;
    }
}
