using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;



public static partial class FileUtils 
{
    public static void WriteFile(string path, byte[] data)
    {
        FileInfo fi = new FileInfo(path);
        DirectoryInfo dir = fi.Directory;
        if (!dir.Exists)
        {
            dir.Create();
        }
        FileStream fs = fi.Create();
        fs.Write(data, 0, data.Length);
        fs.Flush();
        fs.Close();
    }



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


    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    /// <remarks>
    /// * 如果需要频繁调用该方法，可考虑使用 StringBuilder 来构建十六进制字符串，避免频繁的字符串拼接操作，从而提高性能。
    /// </remarks>
    public static string CalculateMD5_01(byte[] data)
    {

        try
        {
            // 创建MD5实例
            using (MD5 md5 = MD5.Create())
            {
                // 计算MD5哈希值
                byte[] hashBytes = md5.ComputeHash(data);

                // 将字节数组转换为十六进制字符串
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"计算MD5码时发生错误: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    /// <remarks>
    /// * CalculateMD5 和 CalculateMD5_01 结算结果一致
    /// * CalculateMD5_01 比较耗性能。
    /// </remarks>
    public static string CalculateMD5(byte[] data)
    {
        try
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(data);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"计算MD5码时发生错误: {ex.Message}");
            return null;
        }
    }



    

    public static long CalculateFileBytes(string filePath)
    {
 
            // 2. 创建 FileInfo 对象
            FileInfo fileInfo = new FileInfo(filePath);
            // 3. 检查文件是否存在
            if (!fileInfo.Exists)
            {
                Console.WriteLine($"文件 '{filePath}' 不存在。");
                return -1;
            }

            long fileSizeInBytes = fileInfo.Length;

            // 格式化为更易读的单位 (KB, MB, GB)
            //string formattedSize = FormatFileSize(fileSizeInBytes);

            return fileSizeInBytes;
    }

    private static string FormatFileSize(long bytes)
    {
        // 如果文件大小为0或不存在
        if (bytes < 0)
            return "未知大小";
        if (bytes == 0)
            return "0 B";

        // 定义单位数组
        string[] units = { "B", "KB", "MB", "GB", "TB" };

        // 计算单位索引
        int unitIndex = 0;
        double size = bytes;
        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        // 格式化输出，保留两位小数
        return $"{size:F2} {units[unitIndex]}";
    }

    public static string CalculateFileMD5(string filePath)
    {
        try
        {
            // 检查文件是否存在
            if (!File.Exists(filePath))
            {
                Debug.LogError($"文件 {filePath} 不存在！");
                return null;
            }

            // 读取文件内容为字节数组
            byte[] fileBytes = File.ReadAllBytes(filePath);

            // 创建MD5实例
            using (MD5 md5 = MD5.Create())
            {
                // 计算MD5哈希值
                byte[] hashBytes = md5.ComputeHash(fileBytes);

                // 将字节数组转换为十六进制字符串
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"计算MD5码时发生错误: {ex.Message}");
            return null;
        }
    }

    static int chunkSize = 1024 * 1024; // 1MB 块大小


    /// <summary>
    /// 大文件 - 分块校验
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string CalculateChunkedMD5(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"文件 {filePath} 不存在！");
                return null;
            }

            using (FileStream fileStream = File.OpenRead(filePath))
            using (MD5 md5 = MD5.Create())
            {
                byte[] buffer = new byte[chunkSize];
                int bytesRead;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    byte[] chunk = new byte[bytesRead];
                    Array.Copy(buffer, chunk, bytesRead);
                    md5.TransformBlock(chunk, 0, chunk.Length, null, 0);
                }
                md5.TransformFinalBlock(new byte[0], 0, 0);
                byte[] hashBytes = md5.Hash;
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"计算分块 MD5 哈希值时发生错误: {ex.Message}");
            return null;
        }
    }

    public static uint CalculateFileCRC32(string filePath) => CRC32Calculator.CalculateFileCRC32(filePath);
}





public class CRC32Calculator
{
    // CRC32 多项式，标准的 CRC32 多项式值
    private const uint Polynomial = 0xEDB88320;
    // CRC 表，用于存储预先计算好的 CRC 值
    private static readonly uint[] CrcTable = new uint[256];

    static CRC32Calculator()
    {
        // 初始化 CRC 表
        for (uint i = 0; i < 256; i++)
        {
            uint crc = i;
            for (int j = 8; j > 0; j--)
            {
                if ((crc & 1) == 1)
                    crc = (crc >> 1) ^ Polynomial;
                else
                    crc >>= 1;
            }
            CrcTable[i] = crc;
        }
    }

    /// <summary>
    /// 计算文件的 CRC32 校验值
    /// </summary>
    /// <param name="filePath">文件的路径</param>
    /// <returns>文件的 CRC32 校验值</returns>
    /// <remark>
    /// * 这里的CRC32算法，和u3d的AB包CRC算法结果是不一样的！
    /// </remark>
    public static uint CalculateFileCRC32(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"文件 {filePath} 不存在！");
                return 0;
            }

            uint crcValue = 0xFFFFFFFF;
            using (FileStream fileStream = File.OpenRead(filePath))
            {
                int byteRead;
                while ((byteRead = fileStream.ReadByte()) != -1)
                {
                    crcValue = (crcValue >> 8) ^ CrcTable[(crcValue ^ (byte)byteRead) & 0xFF];
                }
            }
            return crcValue ^ 0xFFFFFFFF;
        }
        catch (Exception ex)
        {
            Debug.LogError($"计算 CRC32 校验值时发生错误: {ex.Message}");
            return 0;
        }
    }


    /// <summary>
    /// 转为8位的16进制字符串
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string CalculateFileCRC32Str(string filePath)
    {

        uint crc32 = CalculateFileCRC32(filePath);

        /*
        uint crc1 = 3911944193;
        uint crc2 = 470612471;

        // 转换为十六进制字符串
        string hexCRC1 = crc1.ToString("X8");
        string hexCRC2 = crc2.ToString("X8");

        Debug.Log($"十进制 CRC1: {crc1}, 十六进制 CRC1: {hexCRC1}");
        Debug.Log($"十进制 CRC2: {crc2}, 十六进制 CRC2: {hexCRC2}");

        Debug.Log($"文件 {filePath} 的 CRC32 校验值是: {crc32:X8} -- {crc32}");
        // 使用 string.Format 方法
        string result2 = string.Format("CRC32 值是: {0:X8}", crc32);
        Debug.Log(result2);

        */

        /*
假设 crc32 的值为 0x12345678，那么使用 {crc32:X8} 格式化后的输出结果为 "12345678"；如果 crc32 的值为 0x12，格式化后的输出结果为 "00000012"，保证了输出的十六进制字符串始终是 8 位长度。
综上所述，{crc32:X8} 的作用是将 crc32 这个无符号整数变量转换为 8 位长度的大写十六进制字符串进行输出。
        */

        return crc32.ToString("X8");  //十六进制
    }


}



public static partial class FileUtils
{
    /*
    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    /// <remarks>
    /// </remarks>
    public static string CalculateMD5(byte[] data)
    {
        try
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(data);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"计算MD5码时发生错误: {ex.Message}");
            return null;
        }
    }

    public static string CalculateFileMD5(string filePath)
    {
        try
        {
            // 检查文件是否存在
            if (!File.Exists(filePath))
            {
                Debug.LogError($"文件 {filePath} 不存在！");
                return null;
            }

            // 读取文件内容为字节数组
            byte[] fileBytes = File.ReadAllBytes(filePath);

            // 创建MD5实例
            return CalculateMD5(fileBytes);
        }
        catch (Exception ex)
        {
            Debug.LogError($"计算MD5码时发生错误: {ex.Message}");
            return null;
        }
    }*/




    /// <summary>
    /// 获取网路文件路劲
    /// </summary>
    /// <param name="baseUriStr">参考路劲</param>
    /// <param name="relativeUriStr">相对路劲（可以传入绝对路劲）</param>
    /// <returns></returns>
    public static string GetFileWebUrl(string baseUriStr, string relativeUriStr)
    {
        string targetFileUrl = relativeUriStr;

        if (!targetFileUrl.StartsWith("http://") && !targetFileUrl.StartsWith("https://"))
        {
            Uri baseUri = new Uri(baseUriStr);
            Uri combinedUri = new Uri(baseUri, relativeUriStr);
            targetFileUrl = combinedUri.ToString();

            /*
            string[] paths = { 
                "/PssOn00152/debug/android/1", // http://8.138.140.180:8124/PssOn00152/debug/android/1
                "PssOn00152/debug/android/1", // http://8.138.140.180:8124/a/b/c/PssOn00152/debug/android/1
               "../PssOn00152/debug/android/1",//  http://8.138.140.180:8124/a/b/PssOn00152/debug/android/1
                "./PssOn00152/debug/android/1",//  http://8.138.140.180:8124/a/b/c/PssOn00152/debug/android/1
               "../../../PssOn00152/debug/android/1" // http://8.138.140.180:8124/PssOn00152/debug/android/1
            };                        
            foreach (string path in paths)
            {
                Uri beUri = new Uri("http://8.138.140.180:8124/a/b/c/");
                Uri curl = new Uri(beUri, path);
                Debug.Log($"热更路劲： {curl.ToString()}");
            }*/

            /*
            //totalVersionUrl = http://8.138.140.180:8124/PssOn00152/total_version.json
            string[] paths = {
                "/PssOn00152/debug/android/1", // http://8.138.140.180:8124/PssOn00152/debug/android/1
                "PssOn00152/debug/android/1", // http://8.138.140.180:8124/PssOn00152/PssOn00152/debug/android/1
               "./PssOn00152/debug/android/1",//  http://8.138.140.180:8124/PssOn00152/PssOn00152/debug/android/1
               "../PssOn00152/debug/android/1" //  http://8.138.140.180:8124/PssOn00152/debug/android/1
            };
            foreach (string path in paths)
            {
                Uri beUri = new Uri(totalVersionUrl);
                Uri curl = new Uri(beUri, path);
                Debug.Log($"热更路劲： {curl.ToString()}");
            }*/
        }

        return targetFileUrl;
    }


    /// <summary>
    /// 获取网路目录路劲
    /// </summary>
    /// <param name="baseUriStr">参考路劲</param>
    /// <param name="relativeUriStr">相对路劲（可以传入绝对路劲）</param>
    /// <returns></returns>
    public static string GetDirWebUrl(string baseUriStr, string relativeUriStr)
    {
        string targetDirUrl = relativeUriStr;

        if (!targetDirUrl.StartsWith("http://") && !targetDirUrl.StartsWith("https://"))
        {
            Uri baseUri = new Uri(baseUriStr);
            Uri combinedUri = new Uri(baseUri, relativeUriStr);
            targetDirUrl = combinedUri.ToString();
        }

        if (!targetDirUrl.EndsWith("/"))
            targetDirUrl += "/";
        return targetDirUrl;
    }






    /// <summary>
    /// 拷贝StreamingAsset里的资源到本地
    /// </summary>
    /// <param name="srcPath"></param>
    /// <param name="tarPath"></param>
    /// <returns></returns>
    public static IEnumerator CopyStreamingAssetToLocal(string srcPath, string tarPath)
    {

#if UNITY_ANDROID
        using (UnityWebRequest reqSAAsset = UnityWebRequest.Get(srcPath))
        {
            yield return reqSAAsset.SendWebRequest();

            if (reqSAAsset.result == UnityWebRequest.Result.Success)
            {
                WriteAllBytes(tarPath, reqSAAsset.downloadHandler.data);
                yield return new WaitForEndOfFrame();
            }
            else
            {
                Debug.LogError($"Copy File Fail: '{srcPath}' ;  error: {reqSAAsset.error}");
                yield break;
            }
        }
#else
        byte[] bytes = File.ReadAllBytes(srcPath);
        WriteAllBytes(tarPath, bytes);
        yield return new WaitForEndOfFrame();
#endif
    }


    public static IEnumerator ReadStreamingAsset<T>(string srcPath , Action<object> onSuccessCallback, Action<string> onErrorCallback)
    {
#if UNITY_ANDROID
        using (UnityWebRequest reqSAAsset = UnityWebRequest.Get(srcPath))
        {
            yield return reqSAAsset.SendWebRequest();

            if (reqSAAsset.result == UnityWebRequest.Result.Success)
            {
                Type type = typeof(T);

                if (type == typeof(string))
                {
                    onSuccessCallback?.Invoke(reqSAAsset.downloadHandler.text);
                }
                else if (type == typeof(byte[]))
                {
                    onSuccessCallback?.Invoke(reqSAAsset.downloadHandler.data);
                }
                else
                {
                    Debug.LogError("T must string or byte[]");
                }
            }
            else
            {
                Debug.LogError($"Copy File Fail: {srcPath} error: {reqSAAsset.error}");
                onErrorCallback?.Invoke(reqSAAsset.error);
            }
        }
#else
        byte[] bytes = File.ReadAllBytes(srcPath);
        onSuccessCallback?.Invoke(bytes);
        yield return new WaitForEndOfFrame();
#endif
    }



    public static void ReadStreamingAssetSync<T>(string srcPath, System.Action<object> onSuccessCallback, System.Action<string> onErrorCallback)
    {
#if UNITY_ANDROID
        using (UnityWebRequest reqSAAsset = UnityWebRequest.Get(srcPath))
        {
            reqSAAsset.SendWebRequest();
            while (!reqSAAsset.isDone)
            {
                // 等待请求完成
            }

            if (reqSAAsset.result == UnityWebRequest.Result.Success)
            {
                System.Type type = typeof(T);

                if (type == typeof(string))
                {
                    onSuccessCallback?.Invoke(reqSAAsset.downloadHandler.text);
                }
                else if (type == typeof(byte[]))
                {
                    onSuccessCallback?.Invoke(reqSAAsset.downloadHandler.data);
                }
                else
                {
                    Debug.LogError("T must string or byte[]");
                }
            }
            else
            {
                Debug.LogError($"Copy File Fail: {srcPath} error: {reqSAAsset.error}");
                onErrorCallback?.Invoke(reqSAAsset.error);
            }
        }
#else
        try
        {
            byte[] bytes = File.ReadAllBytes(srcPath);
            System.Type type = typeof(T);

            if (type == typeof(string))
            {
                onSuccessCallback?.Invoke(System.Text.Encoding.UTF8.GetString(bytes));
            }
            else if (type == typeof(byte[]))
            {
                onSuccessCallback?.Invoke(bytes);
            }
            else
            {
                Debug.LogError("T must string or byte[]");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Copy File Fail: {srcPath} error: {ex.Message}");
            onErrorCallback?.Invoke(ex.Message);
        }
#endif
    }

    public static void WriteAllBytes(string path, byte[] bytes)
    {
        string directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllBytes(path, bytes);
    }


    public static void WriteAllText(string path, string contents)
    {
        string directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllText(path, contents);
    }

    /// <summary>
    /// 删除目录下的文件以及文件夹
    /// </summary>
    /// <param name="directory"></param>
    /// <returns></returns>
    public static IEnumerator DeleteDirectoryAsync(string directory)
    {
        // 删除所有文件
        string[] files = Directory.GetFiles(directory);
        foreach (string file in files)
        {
            try
            {
                File.Delete(file);
            }
            catch (Exception e)
            {
                Debug.LogError($"1 Failed to delete file {file}: {e.Message}");
            }
            yield return null;
        }

        // 删除所有子文件夹
        string[] directories = Directory.GetDirectories(directory);
        foreach (string dir in directories)
        {
            yield return DeleteDirectoryAsync(dir);
           /* try
            {
                Directory.Delete(dir);
            }
            catch (Exception e)
            {
                Debug.LogError($"2 Failed to delete directory {dir}: {e.Message}");
            }
            yield return null;
           */
        }

        // 删除当前目录
        try
        {
            Directory.Delete(directory);
        }
        catch (Exception e)
        {
            Debug.LogError($"3 Failed to delete directory {directory}: {e.Message}");
        }
    }



    /// <summary>
    /// 拷贝本地目录下的文件和文件夹到目标目录下
    /// </summary>
    /// <param name="sourceDir"></param>
    /// <param name="destDir"></param>
    /// <returns></returns>
    public static IEnumerator CopyDirectoryAsync(string sourceDir, string destDir)
    {
        if (!Directory.Exists(sourceDir))
        {
            Debug.LogError($"Source directory {sourceDir} does not exist.");
            yield break;
        }

        if (!Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }

        string[] files = Directory.GetFiles(sourceDir);
        foreach (string file in files)
        {
            Debug.Log($"copy temp hotfix file: {file}");
            yield return CopyFileAsync(file, Path.Combine(destDir, Path.GetFileName(file)));
        }

        string[] directories = Directory.GetDirectories(sourceDir);
        foreach (string dir in directories)
        {
            string dirName = Path.GetFileName(dir);
            string destSubDir = Path.Combine(destDir, dirName);
            yield return CopyDirectoryAsync(dir, destSubDir);
        }
    }


    /// <summary>
    /// 拷贝本地文件
    /// </summary>
    /// <param name="sourceFile"></param>
    /// <param name="destFile"></param>
    /// <returns></returns>
    public static IEnumerator CopyFileAsync(string sourceFile, string destFile)
    {
        // 检查目标文件是否存在，如果存在则删除
        if (File.Exists(destFile))
        {
            try
            {
                File.Delete(destFile);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete file {destFile}: {e.Message}");
                yield break;
            }
        }

        /*
        using (FileStream sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
        using (FileStream destStream = new FileStream(destFile, FileMode.Create, FileAccess.Write))
        {
            //byte[] buffer = new byte[4096]; //4KB
            byte[] buffer = new byte[65536]; // 64KB
            int bytesRead;
            while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                destStream.Write(buffer, 0, bytesRead);
                yield return null;
            }
        }*/


        // 获取文件大小
        long fileSize = new FileInfo(sourceFile).Length;
        int bufferSize = (int)Math.Min(fileSize, 65536); // 64KB

        // 使用异步文件流进行文件拷贝
        using (FileStream sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.Asynchronous))
        using (FileStream destStream = new FileStream(destFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.Asynchronous))
        {
            //byte[] buffer = new byte[4096]; //4KB
            byte[] buffer = new byte[bufferSize];
            int bytesRead;
            while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                destStream.Write(buffer, 0, bytesRead);
                yield return null;
            }
        }

        /*
        System.Threading.Tasks.Task copyTask = null;
        // 获取文件大小
        long fileSize = new FileInfo(sourceFile).Length;
        int bufferSize = (int)Math.Min(fileSize, 65536);

        // 使用异步文件流进行文件拷贝
        using (FileStream sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.Asynchronous))
        using (FileStream destStream = new FileStream(destFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.Asynchronous))
        {
            copyTask = sourceStream.CopyToAsync(destStream);
            while (!copyTask.IsCompleted)
            {
                yield return null;
            }
        }
        if (copyTask.IsFaulted)
        {
            Debug.LogError($"Failed to copy file: {copyTask.Exception.Message}");
        }
        */
    }
}