using FairyGUI;
using GameMaker;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public enum PageType
{
    Page = 1,
    Popup = 2,
    Overlay = 3,
}
public partial class PageManager : MonoBehaviour
{
    private static object _mutex = new object();
    private static PageManager _instance;

    public static PageManager Instance
    {
        get
        {
            lock (_mutex)
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType(typeof(PageManager)) as PageManager;
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject("PageManager");
                        _instance = obj.AddComponent<PageManager>();
                        if (_instance.transform.parent == null)
                            DontDestroyOnLoad(_instance.gameObject);
                    }
                }

                return _instance;
            }
        }
    }


    // Update is called once per frame
    //void Update(){ }

    private readonly GRoot _root;


    /// <summary> 界面的缓存字典 </summary>
    public Dictionary<PageName, PageBase> pageCacheDict = new Dictionary<PageName, PageBase>();


    /// <summary> 页面堆栈 </summary>
    List<PageBase> pagesStack = new List<PageBase>();

    public void OpenPage(PageName pageName, EventData data = null, Action<PageBase> onFinishCalllback = null)
    {
        PageBase window = null;

        if (pageCacheDict.TryGetValue(pageName, out window))
        {

            if (window.IsOpen())
            {
                Debug.LogError($"{pageName} is open, can not repeat open");
                onFinishCalllback?.Invoke(null);
                return;
            }

            window.Show();
            InputPageStack(window);
            window.OnOpen(pageName, data);
            onFinishCalllback?.Invoke(window);
            return;
        }

        // 检查路径是否配置
        object[] confg = null;
        if (!UIConst.Instance.pathDict.TryGetValue(pageName, out confg))
        {
            Debug.LogError("界面名称错误，或未配置路径: " + pageName);
            onFinishCalllback?.Invoke(null);
            return;
        }

        string pth = (string)confg[0];
        // Type windowCtrl = (Type)confg[1]; 
        string typeName = (string)confg[1];
        Type windowCtrl = Type.GetType((string)confg[1]);
        if (windowCtrl == null)
        {
            Assembly hotfixAssembly = Assembly.Load("HotFix");
            windowCtrl = hotfixAssembly.GetType(typeName);
        }

        string pkgName = null;
        FieldInfo fieldInfo = windowCtrl.GetField("pkgName", BindingFlags.Public | BindingFlags.Static);
        if (fieldInfo != null)
        {
            pkgName = (string)fieldInfo.GetValue(null); // 使用 null 因为它是静态字段
        }



        Action func = () =>
        {
            window = Activator.CreateInstance(windowCtrl) as PageBase;
            //window = new ConsoleMainPage();

            if (!pageCacheDict.ContainsKey(pageName))
            {
                pageCacheDict.Add(pageName, window);
            }


            window.Show();
            InputPageStack(window);
            window.OnOpen(pageName, data);

            onFinishCalllback?.Invoke(window);
        };

        if (!string.IsNullOrEmpty(pkgName) && UIPackage.GetByName(pkgName) == null)
        {
            //bool isNext = false;

            // 加载资源
            ResourceManager02.Instance.LoadAssetBundleAsync(pth, (bundle) =>
            {
                UIPackage.AddPackage(bundle);
                //isNext = true;
                func();
                return;
            });

            //await WaitUntil(() => isNext == true);

            // 这样写会造成阻塞主线程
            //Task.Run(() => WaitUntil(() => isNext == true)).GetAwaiter().GetResult();
        }
        else
        {
            func();
            return;
        }

        /*
        window = Activator.CreateInstance(windowCtrl) as PageBase;
        //window = new ConsoleMainPage();

        pageCacheDict.Add(pageName, window);


        window.Show();
        InputPageStack(window);
        window.OnOpen(pageName, data);
        return window;
        */
    }

    /* 待完成   */

    public void SetPageToTop(PageName pageName)
    {
        PageBase window = null;

        if (pageCacheDict.TryGetValue(pageName, out window))
        {

            if (!window.IsOpen())
            {
                Debug.LogError($"{pageName} is not open, can not set page to top");
                return;
            }


            if (window.isTop)
                return;


            window.Show();
            InputPageStack(window); // 避免并发"设置为置顶"


            return;
        }
    }
    public void SetPageToTop(PageBase window)
    {

        if (window != null && pageCacheDict.ContainsValue(window))
        {

            if (!window.IsOpen())
            {
                Debug.LogError($"{window.pageName} is not open, can not set page to top");
                return;
            }


            if (window.isTop)
                return;


            window.Show();
            InputPageStack(window); // 避免并发"设置为置顶"

            return;
        }
    }

    /*[废弃使用]
    public async Task<EventData> OpenPageAsync(PageName pageName, EventData data, Action<EventData> onCallBack)
    {
        var res = await OpenPageAsync(pageName, data);
        onCallBack?.Invoke(res);
        return res;
    }*/
    public async void OpenPageAsync(PageName pageName, EventData data, Action<EventData> onCallBack)
    {
        var res = await OpenPageAsync(pageName, data);
        onCallBack?.Invoke(res);
    }

    public async Task<EventData> OpenPageAsync(PageName pageName, EventData data = null)
    {

        PageBase window = null;

        if (pageCacheDict.TryGetValue(pageName, out window))
        {
            if (!window.IsOpen())
            {
                window.Show();
                InputPageStack(window);
                return await window.OnOpenAsync(pageName, data);
            }

            return new EventData("IsOpen");
        }

        // 检查路径是否配置
        object[] confg = null;
        if (!UIConst.Instance.pathDict.TryGetValue(pageName, out confg))
        {
            Debug.LogError("界面名称错误，或未配置路径: " + pageName);
            return null;
        }

        string pth = (string)confg[0];
        string typeName = (string)confg[1];
        Type windowCtrl = Type.GetType((string)confg[1]);
        if (windowCtrl == null)
        {
            Assembly hotfixAssembly = Assembly.Load("HotFix");
            windowCtrl = hotfixAssembly.GetType(typeName);
        }


        // 使用反射获取 pkgName 常量的值
        string pkgName = null;
        FieldInfo fieldInfo = windowCtrl.GetField("pkgName", BindingFlags.Public | BindingFlags.Static);
        if (fieldInfo != null)
        {
            pkgName = (string)fieldInfo.GetValue(null); // 使用 null 因为它是静态字段
        }

        //Debug.LogError($"获取到的fgui包名： {pkgName}");

        if (!string.IsNullOrEmpty(pkgName) && UIPackage.GetByName(pkgName) == null)
        {
            bool isNext = false;

            // 加载资源
            ResourceManager02.Instance.LoadAssetBundleAsync(pth, (bundle) =>
            {
                UIPackage.AddPackage(bundle);

                isNext = true;
            });

            await WaitUntil(() => isNext == true);
        }

        window = Activator.CreateInstance(windowCtrl) as PageBase;
        //window = new ConsoleMainPage();

        pageCacheDict.Add(pageName, window);

        window.Show();
        InputPageStack(window);
        return await window.OnOpenAsync(pageName, data);
    }

    public void PreloadPage(PageName pageName,Action onLoadedCallback)
    {
        PageBase window = null;

        if (pageCacheDict.TryGetValue(pageName, out window))
        {
            onLoadedCallback?.Invoke();
            return;
        }


        object[] confg = null;
        if (!UIConst.Instance.pathDict.TryGetValue(pageName, out confg))
        {
            Debug.LogError("界面名称错误，或未配置路径: " + pageName);
            return;
        }

        string pth = (string)confg[0];
        string typeName = (string)confg[1];
        Type windowCtrl = Type.GetType((string)confg[1]);
        if (windowCtrl == null)
        {
            Assembly hotfixAssembly = Assembly.Load("HotFix");
            windowCtrl = hotfixAssembly.GetType(typeName);
        }

        string pkgName = null;
        FieldInfo fieldInfo = windowCtrl.GetField("pkgName", BindingFlags.Public | BindingFlags.Static);
        if (fieldInfo != null)
        {
            pkgName = (string)fieldInfo.GetValue(null); 
        }

        Action func = () =>
        {
            window = Activator.CreateInstance(windowCtrl) as PageBase;
            
            if (onLoadedCallback != null)
            {
                UnityAction onloaded = null;
                onloaded = () =>
                {
                    onLoadedCallback?.Invoke();
                    window.preLoadedCallback.RemoveListener(onloaded);
                };
                window.preLoadedCallback.AddListener(onloaded);
            }
            
            pageCacheDict.Add(pageName, window);
            GRoot.inst.AddChild(window);
            window.Hide();
            //window.Show();
            //window.Hide();
        };

        if (!string.IsNullOrEmpty(pkgName) && UIPackage.GetByName(pkgName) == null)
        {
            ResourceManager02.Instance.LoadAssetBundleAsync(pth, (bundle) =>
            {
                UIPackage.AddPackage(bundle);
                //isNext = true;
                func();
            });
        }
        else
        {
            func();
        }
    }


    public bool ClosePage(PageName name, EventData data = null)
    {
        PageBase window = null;
        if (!pageCacheDict.TryGetValue(name, out window))
        {
            Debug.LogWarning("界面未打开: " + name);
            return false;
        }

        int indexStackPage = IndexOf(window);
        window.Hide();
        window.OnClose(data);
        OutputPageStack(window, indexStackPage);
        //panelDict.Remove(name);
        return true;
    }

    public bool ClosePage(PageBase window, EventData data = null)
    {

        int indexStackPage = IndexOf(window);
        window.Hide();
        window.OnClose(data);
        OutputPageStack(window, indexStackPage);
        return true;
    }

    private static async Task WaitUntil(Func<bool> condition)
    {
        while (!condition())
        {
            await Task.Delay(10); // 每10ms检查一次

            // 避免“编辑器-非运行”模式下，死循环导致u3d编辑器卡死
            if (Application.isEditor && !Application.isPlaying)  return;
        }
    }






    /// <summary>
    /// 
    /// </summary>
    /// <param name="window"></param>
    /// <returns></returns>
    /// <remarks>
    /// * -1 : 不在页面堆栈
    /// * 0 : 在最顶步（最顶层）
    /// * GRoot 底下挂的对象，不一定全部是Window，有可能是其他的GObject
    /// </remarks>
    public int IndexOf(PageBase window)
    {
        /*
        // 1. 检查窗口是否打开（contentPane 是否在 GRoot 上）
        if (window == null || window.parent == null)
            return -1;

        //检查是否是 GRoot 的直接子对象中的最后一个（最顶层）
        GObject[] children = GRoot.inst.GetChildren();
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] == window)
            {
                return children.Length - 1 - i : -1;
            }
        }
        return -1;
        */

        if (!window.isShowing)
            return -1;

        GObject[] children = GRoot.inst.GetChildren();
        List<PageBase> pageStack = new List<PageBase>();
        for (int i = children.Length - 1; i >= 0; i--)  // window越靠后的越在前
        {
            PageBase wd = children[i] as PageBase;
            if (wd != null)
            {
                pageStack.Add(wd);
            }
        }
        // 按堆栈顺序获取所有PageBase对象，得到当前窗体的层级
        int index = pageStack.IndexOf(window);
        return index;
    }


    /// <summary>
    /// 页面在堆栈中的索引
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <remarks>
    /// * -1 : 不在页面堆栈
    /// * 0 : 在最顶步（最顶层）
    /// </remarks>
    public int IndexOf(PageName name)
    {
        if (!pageCacheDict.ContainsKey(name))
            return -1;
        return IndexOf(pageCacheDict[name]);
    }






    Coroutine coToTop;
    /// <summary>
    /// 延时到OnOpen接口调用后,才调用OnTop接口
    /// </summary>
    /// <param name="toTopPage"></param>
    /// <returns></returns>
    IEnumerator _SetPageToTop(PageBase toTopPage)
    {
        yield return new WaitForSecondsRealtime(0.4f);
        coToTop = null;
        // toTopPage.OnTop();
        toTopPage.BringToFront(); // 该接口 已经包含 toTopPage.OnTop();

        // 页面置顶事件通知(避免某些界面监听长按时，弹出其他界面，导致长按特效关不掉)
        EventCenter.Instance.EventTrigger<EventData>("ON_PAGE_EVENT", new EventData<PageName>("PageOnTopChange", toTopPage.pageName));
    }


    IEnumerator _NextPageToTop(PageBase toTopPage)
    {
        yield return new WaitForSecondsRealtime(0.4f);
        coToTop = null;
        toTopPage.OnTop();

        // 页面置顶事件通知(避免某些界面监听长按时，弹出其他界面，导致长按特效关不掉)
        EventCenter.Instance.EventTrigger<EventData>("ON_PAGE_EVENT", new EventData<PageName>("PageOnTopChange", toTopPage.pageName));
    }


    public PageBase GetPageBase(GameObject go)
    {
        foreach (KeyValuePair<PageName, PageBase> kv in pageCacheDict)
        {
            if (kv.Value.contentPane.displayObject.gameObject == go)
            {
                return kv.Value;
            }
        }
        return null;
    }




#if !true


    int topPageNumb = -1;

    public void InputPageStack(PageBase window)
    {
        int pageNumb = ++topPageNumb;
        window.pageNumb = pageNumb;
        
       /* if(pagesStack.Contains(window))
            pagesStack.Remove(window);
        pagesStack.Insert(0,window);*/
        
        if (coToTop != null)
            StopCoroutine(coToTop);
        coToTop = StartCoroutine(_SetPageToTop(window));
    }

    public void OutputPageStack(PageBase window, int indexStackPage)
    {
        //Debug.LogError($"on output page stack ;  index: {indexStackPage}");
        
        /*if(pagesStack.Contains(window))
            pagesStack.Remove(window);*/
        
        if (indexStackPage == 0)  //是最顶页
        {
            GObject[] children = GRoot.inst.GetChildren();
            for (int i = children.Length - 1; i >= 0; i--)
            {
                PageBase topWindow = children[i] as PageBase;  // as Window (可能存在不属于PageBase 类 的window)
                if (topWindow != null && topWindow.isShowing)
                {
                    //Debug.LogError($"topWindow name {topWindow.name}");
                    if (coToTop != null)
                        StopCoroutine(coToTop);
                    coToTop = StartCoroutine(_NextPageToTop(topWindow));

                    topPageNumb = topWindow.pageNumb;
                    return;
                }
            }

            topPageNumb = -1;
        }
    }
#else

    // 多类页面

    Dictionary<PageType, int> topPageNumbers = new Dictionary<PageType, int>()
    {
        [PageType.Page] = -1,
        [PageType.Popup] = -1,
        [PageType.Overlay] = -1,
    };

    List<PageBase> GetPagesStack()
    {
        List<PageBase> pagesStack = new List<PageBase>();
        int cnt = GRoot.inst.numChildren;
        for (int i = cnt - 1; i >= 0; i--)
        {
            PageBase theWindow = GRoot.inst.GetChildAt(i) as PageBase;
            if ((theWindow is PageBase) && theWindow.isShowing)
            {
                pagesStack.Add(theWindow);
            }
        }
        return pagesStack;
    }


    int GetPageIndex(PageBase p) => (int)p.pageType * 1000 + p.pageNumb;
    private void FGUI_AdjustModalLayer()
    {
        if (GRoot.inst.modalLayer == null || GRoot.inst.isDisposed)
            return;

        int cnt = GRoot.inst.numChildren;

        for (int i = cnt - 1; i >= 0; i--)
        {
            GObject g = GRoot.inst.GetChildAt(i);
            if ((g is Window) && (g as Window).modal)
            {
                if (GRoot.inst.modalLayer.parent == null)
                    GRoot.inst.AddChildAt(GRoot.inst.modalLayer, i);
                else
                    GRoot.inst.SetChildIndexBefore(GRoot.inst.modalLayer, i);
                return;
            }
        }

        if (GRoot.inst.parent != null)
            GRoot.inst.RemoveChild(GRoot.inst.modalLayer);
    }

    public void InputPageStack(PageBase window)
    {
        int pageNumb = ++topPageNumbers[window.pageType];// ++topPageNumb;
        window.pageNumb = pageNumb;



        int pageIndex = GetPageIndex(window);
        GObject[] children = GRoot.inst.GetChildren();

        PageBase beforeWindow = null;
        for (int i = children.Length - 1; i >= 0; i--)
        {
            PageBase theWindow = children[i] as PageBase;  // as Window (可能存在不属于PageBase 类 的window)
            if (theWindow != null && theWindow != window && theWindow.isShowing)
            {
                int thePageIndex = GetPageIndex(theWindow);
                //DebugUtils.LogWarning($"{window.pageName} -  pageIndex:{pageIndex}  {window.pageType} - {window.pageNumb}");
                //DebugUtils.LogWarning($"{theWindow.pageName} - thePageIndex:{thePageIndex}  {theWindow.pageType} - {theWindow.pageNumb}");
                if (pageIndex > thePageIndex)
                {
                    break;
                }
                else
                {
                    beforeWindow = theWindow;
                }
            }
        }


        // 非置顶，排在beforewindow后面
        if (beforeWindow != null)   // window的index越靠后的越在前
        {
            int index = GRoot.inst.GetChildIndex(beforeWindow);  
            //GRoot.inst.SetChildIndex(window, index); // 排在beforeWindow后
            GRoot.inst.SetChildIndexBefore(window, index);  // 排在index后
            FGUI_AdjustModalLayer();
            return;
        }


        // 调用置顶接口OnTop
        if (coToTop != null)
            StopCoroutine(coToTop);
        coToTop = StartCoroutine(_SetPageToTop(window));
    }

    public void OutputPageStack(PageBase window, int indexStackPage)
    {

        // 多个topPageNumb队列
        if (indexStackPage == 0)  //是最顶页
        {
            GObject[] children = GRoot.inst.GetChildren();

            int topPageNumb = -1;
            for (int i = children.Length - 1; i >= 0; i--)
            {
                PageBase clsWindow = children[i] as PageBase;  // as Window (可能存在不属于PageBase 类 的window)
                if (clsWindow != null && clsWindow.isShowing && clsWindow.pageType == window.pageType)
                {
                    topPageNumb = clsWindow.pageNumb;
                    break;
                }
            }
            topPageNumbers[window.pageType] = topPageNumb;

            for (int i = children.Length - 1; i >= 0; i--)
            {
                PageBase topWindow = children[i] as PageBase;  // as Window (可能存在不属于PageBase 类 的window)
                if (topWindow != null && topWindow.isShowing)
                {
                    //Debug.LogError($"topWindow name {topWindow.name}");
                    if (coToTop != null)
                        StopCoroutine(coToTop);
                    coToTop = StartCoroutine(_NextPageToTop(topWindow));
                    return;
                }
            }
        }
    }

#endif




}



public partial class PageManager
{
    // Dictionary<PageName, PageBase> pages = new Dictionary<PageName, PageBase>();

    [Button]
    void TestDoPage(PageName pageName)
    {
        if (pageCacheDict.ContainsKey(pageName) && pageCacheDict[pageName].IsOpen())
        {
            PageManager.Instance.ClosePage(pageName);
        }
        else
        {
            PageManager.Instance.OpenPage(pageName);
        }
    }

    /*
    [Button]
    void TestGetPage()
    {
        if (pageCacheDict.ContainsKey(PageName.ConsolePageConsoleMachineSettings)) 
            (pageCacheDict[PageName.ConsolePageConsoleMachineSettings] as PageConsoleMachineSettings).TestGetPage();
    }
    */

}









/*
UIPackage.AddPackage("UI/Console");//文件路径Assets\Resources\UI\Console_fui.bytes文件，FGUI编辑器 "
                                   //UIPackage.AddPackage("tavern/tavern");//文件路径

#endregion //加载依赖的包

FairyGUI.UIPanel panel = this.gameObject.AddComponent<FairyGUI.UIPanel>();// UnityEngine.UIPanel同名，因此要前缀 

panel.packageName = "Console";

panel.componentName = "PageConsoleMain";
*/

/*
    文件路径，
    包名，

 */

/*
// 文件路劲，包名，页面名称, 页面对象 ，绑定的控制器


文件路劲: Assets/Test/Resources/UI/Console_fui.bytes

包名: "Console";

页面名称: "PageConsoleMain";

页面对象: GComponent

绑定的控制器:

//Assets/Test/Resources/UI/Console_fui.bytes

*/