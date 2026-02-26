using System;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

/// <summary>
/// 【这个脚本弃用】
/// </summary>
/// <remarks>
/// # 问题分析<br/>
/// * 在使用AssetBundle加载资源时，如果使用同步加载的方式，会使游戏在加载资源的过程中出现卡顿现象，因为同步加载会阻塞主线程的执行，直到资源加载完成才能继续执行后续的逻辑。<br/>
/// * 而异步加载则可以在后台进行资源加载，不会阻塞主线程的执行，但是如果同时加载多个资源，也会导致游戏掉帧，影响游戏的流畅性。<br/>
/// </remarks>
public class AssetBundleHelper  
{
    MonoBehaviour mon;
    public AssetBundleHelper(MonoBehaviour mon)
    {
        this.mon = mon;
    }

    public void LoadAssetBundleAsync(string url, Action<AssetBundle> OnfinishCallback)=>
        mon.StartCoroutine(LoadAssetBundleAsyncIE(url, OnfinishCallback));
    

    /// <summary>
    /// 方法1：解决AssetBundle异步加载的卡顿和掉帧问题:
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    /// <remarks>
    /// # 问题分析<br/>
    /// * Unity3D提供了异步加载AssetBundle的接口，我们可以利用这些接口来实现资源的异步加载。
    /// * 在加载资源时，可以使用UnityWebRequest来进行网络请求，并通过AssetBundle.LoadFromMemoryAsync方法将下载的资源加载到内存中。
    /// * 通过这种方式，可以在后台进行资源的加载，不会阻塞主线程的执行，从而避免卡顿现象的发生。
    /// </remarks>
    public IEnumerator LoadAssetBundleAsyncIE(string url, Action<AssetBundle> OnfinishCallback)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        AssetBundleCreateRequest assetBundleRequest = AssetBundle.LoadFromMemoryAsync(request.downloadHandler.data);
        yield return assetBundleRequest;
        AssetBundle assetBundle = assetBundleRequest.assetBundle;
        // 资源加载完成后的逻辑处理
        OnfinishCallback?.Invoke(assetBundle);
    }


    class AssetBundleTask
    {
        public Thread thread;
        public AssetBundle assetBundle;
        public ManualResetEvent loadCompleteEvent = new ManualResetEvent(false);

        public void LoadAssetBundleThread(object url)
        {
            UnityWebRequest request = UnityWebRequest.Get((string)url);
            request.SendWebRequest();
            while (!request.isDone)
            {
                Thread.Sleep(100);
            }
            assetBundle = AssetBundle.LoadFromMemory(request.downloadHandler.data);
            loadCompleteEvent.Set();
        }
    }

    public void LoadLoadAssetBundleAsync2(string url, Action<AssetBundle> OnfinishCallback) 
        => mon.StartCoroutine(LoadAssetBundleAsyncIE2(url, OnfinishCallback));


    /// <summary>
    /// 方法2：使用多线程加载资源
    /// </summary>
    /// <param name="url"></param>
    /// <param name="OnfinishCallback"></param>
    /// <returns></returns>
    /// <remarks>
    /// # 问题分析<br/>
    /// * 除了使用Unity3D提供的异步加载接口外，我们还可以通过多线程来加载资源，从而进一步提高资源加载的效率。
    /// 在多线程加载资源时，可以将资源的加载和解压缩等操作放在后台线程中进行，不会影响主线程的执行。在加载完成后，再将资源传递给主线程进行后续的逻辑处理。
    /// </remarks>
    public IEnumerator LoadAssetBundleAsyncIE2(string url, Action<AssetBundle> OnfinishCallback)
    {
        AssetBundleTask task = new AssetBundleTask();

        task.loadCompleteEvent.Reset();
        task.thread = new Thread(task.LoadAssetBundleThread);
        task.thread.Start(url);
        yield return new WaitUntil(() => task.loadCompleteEvent.WaitOne(0));
        // 资源加载完成后的逻辑处理
        OnfinishCallback?.Invoke(task.assetBundle);
    }


    /*
     * 
    Thread thread;
    AssetBundle assetBundle;
    ManualResetEvent loadCompleteEvent = new ManualResetEvent(false);

    void LoadAssetBundleThread(object url)
    {
        UnityWebRequest request = UnityWebRequest.Get((string)url);
        request.SendWebRequest();
        while (!request.isDone)
        {
            Thread.Sleep(100);
        }
        assetBundle = AssetBundle.LoadFromMemory(request.downloadHandler.data);
        loadCompleteEvent.Set();
    }

    IEnumerator LoadAssetBundleAsync(string url)
    {
        loadCompleteEvent.Reset();
        thread = new Thread(LoadAssetBundleThread);
        thread.Start(url);
        yield return new WaitUntil(() => loadCompleteEvent.WaitOne(0));
        // 资源加载完成后的逻辑处理
    }
    */




    class WriteTask
    {
        string path;
        byte[] bytes;

        public WriteTask(string path, byte[] bytes)
        {
            this.path = path;
            this.bytes = bytes;
        }

        public Thread thread;
        public ManualResetEvent loadCompleteEvent = new ManualResetEvent(false);
        public void WriteThread(object url)
        {
            File.WriteAllBytes(path, bytes);
            loadCompleteEvent.Set();
        }
    }


    public IEnumerator WriteAsyncIE(string path, byte[] bytes, Action<bool> OnfinishCallback = null)
    {
        WriteTask task = new WriteTask(path, bytes);
        task.loadCompleteEvent.Reset();
        task.thread = new Thread(task.WriteThread);
        task.thread.Start();
        yield return new WaitUntil(() => task.loadCompleteEvent.WaitOne(0));
        // 资源加载完成后的逻辑处理
        OnfinishCallback?.Invoke(true);
    }




    public  async Task WriteAllBytesAsync(string filePath, byte[] bytes, Action<bool> onFinishCallback)
    {
        // 使用 Task.Run 来在后台线程上执行同步的 File.WriteAllBytes 方法
        await Task.Run(() =>
        {
            try
            {
                File.WriteAllBytes(filePath, bytes);
                Debug.Log($"Data written to file successfully: {filePath}");
                onFinishCallback.Invoke(true);
            }
            catch (Exception e)
            {
                onFinishCallback.Invoke(false);
                // 在这里处理异常，例如记录日志或抛出一个新的异常
                Debug.LogError($"Error writing data to file: {e.Message}");
                throw; // 可选：重新抛出异常，以便调用者可以处理它
            }
        });
    }
    /*
     由于Unity的主线程是单线程的，并且负责处理渲染、物理模拟和GUI更新等，因此在主线程上进行长时间的同步操作（如大文件的I/O操作）可能会导致帧率下降。因此，将这类操作卸载到后台线程通常是一个好主意。
    */

}
