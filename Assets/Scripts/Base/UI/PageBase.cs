using UnityEngine;
using System.Threading.Tasks;
using FairyGUI;
using GameMaker;
using System;
using UnityEngine.Events;
using System.Reflection;



/// <summary>
/// 窗口基类，扩展FGUI Window提供栈管理支持
/// </summary>
public class PageBase : Window
{

    // 子类必须实现如下字段
    //public const string pkgName = "";
    //public const string resName = "";
    const string SUB_PKG_NAME = "pkgName";
    const string SUB_RES_NAME = "resName";


    public int pageNumb = 0;    

    public PageName pageName;

    //public PageType pageType = PageType.Page; (这样使用多态有问题)
    public virtual PageType pageType => PageType.Page;

    public UnityEvent preLoadedCallback = new UnityEvent();

    protected EventData inParams;

    private EventData returnParams;

    protected bool isOpen = false;

    protected bool isInit = false;

    /// <summary> 创建时，调用一次 </summary>
    protected override void OnInit()
    {
        _GetSubPkgNameResName();

          // 初始化窗口通用设置     
        this.contentPane = UIPackage.CreateObject(_subPkgName, _subResName).asCom;

        this.Center();
        this.modal = true;

        //#this.SetHideOnClose(true);

        EventCenter.Instance.AddEventListener<I18nLang>(I18nMgr.I18N, OnChangeLanguageBase);
    }

    string _subPkgName ,_subResName;
    void _GetSubPkgNameResName()
    {
        // 获取当前实例的类型
        Type currentType = this.GetType();
        // 反射获取静态字段 pkgName 的值
        FieldInfo pkgNameField = currentType.GetField(SUB_PKG_NAME, BindingFlags.Static | BindingFlags.Public);
        // 反射获取静态字段 resName 的值
        FieldInfo resNameField = currentType.GetField(SUB_RES_NAME, BindingFlags.Static | BindingFlags.Public);

        if (pkgNameField == null || resNameField == null)
        {
            throw new InvalidOperationException($"当前类型中未找到 {SUB_PKG_NAME} 或 {SUB_RES_NAME} 静态字段");
        }

        // 获取字段值（因为是静态字段，第一个参数传 null）
        _subPkgName = pkgNameField.GetValue(null) as string;
        _subResName = resNameField.GetValue(null) as string;
    }


    void OnChangeLanguageBase(I18nLang lang)
    {
#if false
        FguiI18nTextAssistant.Instance.DisposeAllTranslate(this.contentPane);
        this.contentPane.Dispose(); // 释放当前UI
        this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
        InitParam();
        //FguiI18nTextAssistant.Instance.TranslateComponent(this.contentPane);
#endif

        OnBeforetLanguageChange(lang);

        FguiI18nTextAssistant.Instance.DisposeAllTranslate(this.contentPane);
        this.contentPane.Dispose(); // 释放当前UI

        this.contentPane = UIPackage.CreateObject(_subPkgName, _subResName).asCom;

        OnLanguageChange(lang);
    }

    /// <summary>
    /// 初始化接口
    /// </summary>
    /// <param name="data"></param>
    /// <remarks>
    /// *创建界面，打开界面，多语言，等功能重复调用此接口。
    /// </remarks>
    public virtual void InitParam(){ }

    /// <summary> 多语言改变时调用 </summary>

    protected virtual void OnBeforetLanguageChange(I18nLang lang)  {  }

    protected virtual void OnLanguageChange(I18nLang lang) {
        InitParam();
    }


    /// <summary>
    /// 关闭当前窗口并弹出栈
    /// </summary>
    protected void CloseSelf(EventData data)
    {
        PageManager.Instance.ClosePage(this, data);
    }

    /// <summary>
    /// 置顶
    /// </summary>
    public new void BringToFront()
    {
        base.BringToFront();
        OnTop();
    }



    /// <summary> 页面销毁时，调用一次 </summary>
    public virtual void OnDispose()
    {
        EventCenter.Instance.RemoveEventListener<I18nLang>(I18nMgr.I18N, OnChangeLanguageBase);
    }
    override public void Dispose()
    {
        OnDispose();
        base.Dispose();
    }



    public virtual void OnOpen(PageName name, EventData data)
    {
        this.name = Enum.GetName(typeof(PageName), name);
        pageName = name;

        inParams = data;
        returnParams = null;
        isOpen = true;
    }


    public async Task<EventData> OnOpenAsync(PageName name, EventData data)
    {
        OnOpen(name, data);
        //DebugUtils.Log($"页面{name}开始等待关闭（isOpen = {isOpen}）"); // 加日志
        await WaitUntil(() => isOpen == false);
        return returnParams;
    }


    public virtual void OnClose(EventData data = null)
    {
        //DebugUtils.Log($"页面{pageName}执行OnClose，isOpen将设为false"); // 加日志

        inParams = null;
        returnParams = data;
        isOpen = false;
    }
    public virtual void OnTop() { DebugUtils.Log($"i am top  {this.name}"); }
    
    public bool IsOpen() => isOpen;


    private static async Task WaitUntil(Func<bool> condition)
    {
        while (!condition())
        {
            // 打印当前条件结果（确认是否检测到isOpen = false）
            //DebugUtils.Log($"WaitUntil轮询中，当前条件结果：{condition()}");
            await Task.Delay(300); // 原有的300ms延迟
            
            // 避免“编辑器-非运行”模式下，死循环导致u3d编辑器卡死
            if (Application.isEditor && !Application.isPlaying)  return;
        }
        // 打印等待结束的日志
        //DebugUtils.Log("WaitUntil等待结束，条件已满足！");
    }
    

    /*
    public string GetPkgName()
    {
        FieldInfo field = this.GetType().GetField(
            "pkgName",
            BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy
        );
        string value = (string)field.GetRawConstantValue(); 
        return value;
    }
    public string GetResName()
    {
        FieldInfo field = this.GetType().GetField(
            "resName",
            BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy
        );
        string value = (string)field.GetRawConstantValue(); 
        return value;
    }*/

}






