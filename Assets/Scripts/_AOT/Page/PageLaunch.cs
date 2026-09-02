#define DISABLE_DELAY
using FairyGUI;
using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class LoadingProgress
{

    /// <summary> 复制包体中的hotfix dll 到缓存</summary>
    public const string COPY_SA_HOTFIX_DLL = "COPY_SA_HOTFIX_DLL";
    /// <summary> 复制包体中AB包到缓存 </summary>
    public const string COPY_SA_ASSET_BUNDLE = "COPY_SA_ASSET_BUNDLE";
    /// <summary> 复制备份文件 </summary>
    public const string COPY_SA_ASSET_BACKUP = "COPY_SA_ASSET_BACKUP";

    /// <summary> 检查有无待拷贝的文件 </summary>
    public const string CHECK_COPY_TEMP_HOTFIX_FILE = "CHECK_COPY_TEMP_HOTFIX_FILE";

    /// <summary> 检查网络热更版本 </summary>
    public const string CHECK_WEB_VERSION = "CHECK_WEB_VERSION";

    /// <summary> 下载hotfix dll </summary>
    public const string DOWNLOAD_HOTFIX_DLL = "DOWNLOAD_HOTFIX_DLL";

    /// <summary> 下载热更AB包 </summary>
    public const string DOWNLOAD_ASSET_BUNDLE = "DOWNLOAD_ASSET_BUNDLE";

    /// <summary> 下载"资源备份" </summary>
    public const string DOWNLOAD_ASSET_BACKUP = "DOWNLOAD_ASSET_BACKUP";


    /// <summary> 拷贝下载的文件 </summary>
    public const string COPY_TEMP_HOTFIX_FILE = "COPY_TEMP_HOTFIX_FILE";


    /// <summary> 删除无用的ab包 </summary>
    public const string DELETE_UNUSE_ASSET_BUNDLE = "DELETE_UNUSE_ASSET_BUNDLE";

    /// <summary> 删除无用的hotfix dll </summary>
    public const string DELETE_UNUSE_HOTFIX_DLL = "DELETE_UNUSE_HOTFIX_DLL";

    /// <summary> 加载AOT dll到内存 </summary>
    //public const string LOAD_AOT_DLL = "LOAD_AOT_DLL";

    /// <summary> 补充元数据给AOT,而不是给热更新dll补充元数据</summary>
    public const string LOAD_AOT_META_DATA = "LOAD_AOT_META_DATA";

    /// <summary> 加载hotfix dll到内存 </summary>
    public const string LOAD_HOTFIX_DLL = "LOAD_HOTFIX_DLL";


    /// <summary> 预加载AB包到内存 </summary>
    public const string PRELOAD_ASSET_BUNDLE = "PRELOAD_ASSET_BUNDLE";

    /// <summary> 预加载资源到内存 </summary>
    public const string PRELOAD_ASSET = "PRELOAD_ASSET";

    /// <summary> 链接机台（获取参数） </summary>
    public const string CONNECT_MACHINE = "CONNECT_MACHINE";

    /// <summary> 初始化参数设置 </summary>
    public const string INIT_SETTINGS = "INIT_SETTINGS";

    /// <summary> 进入游戏(游戏加载界面) </summary>
    public const string ENTER_GAME = "ENTER_GAME";
}

public class PageLaunch 
{
    private static PageLaunch _instance;


    public static PageLaunch Instance
    {
        get
        {
            if (_instance == null)
            {
                if(UIPackage.GetByName("Native") == null)
                    UIPackage.AddPackage("Native/FGUIs/Native"); // Native/FGUIs/ == Resources/Native/FGUIs
                _instance = new PageLaunch();
                // AOT 层只读 ApplicationSettings，不依赖热更 Theme 程序集
                _instance.goOwnerPage = UIPackage.CreateObject("Native", ResolveLaunchFguiName()).asCom;

                GRoot.inst.AddChild(_instance.goOwnerPage);
                ScreenFlipUtils.Apply();
              
                _instance.goOwnerPage.sortingOrder = 99;
            }
            return _instance;
        }
    }

    GComponent goOwnerPage;

    /// <summary>
    /// 从 ApplicationSettings.launchFguiName 读取；为空时按 gameTheme 推断，默认 TreasuryPageLaunch。
    /// </summary>
    static string ResolveLaunchFguiName()
    {
        var settings = ApplicationSettings.Instance;
        if (settings == null)
            return "TreasuryPageLaunch";

        if (!string.IsNullOrWhiteSpace(settings.launchFguiName))
            return settings.launchFguiName.Trim();

        string theme = settings.gameTheme;
        if (!string.IsNullOrWhiteSpace(theme))
        {
            theme = theme.Trim();
            if (string.Equals(theme, "Savage", StringComparison.OrdinalIgnoreCase))
            {
                return "SavagePageLaunch";
            }

            // Test 主题无独立启动页，复用 Treasury
            if (string.Equals(theme, "Test", StringComparison.OrdinalIgnoreCase))
            {
                return "TreasuryPageLaunch";
            }
        }

        return "TreasuryPageLaunch";
    }

    GLoader lodBG, lodLogo;

    GProgressBar pbLoading;

    GTextField txtMsg;

    GButton btnQuit;
    //财富主题动效
    GameObject goTLoadBG;
    GComponent anchorTLoadBG;
    GameObject cloneGoTLoadBG;
    const string PlatLoadTBgResPath = "Native/Prefabs/Treasury/plat_load_bg";

    //动物主题动效
    GTextField titleProgress;
    GameObject goSavageLogoTitle, goSavageLogoBG, goSavageProgressBar;
    GComponent anchorSavageLogoTitle, anchorSavageLogoBG, anchorSavageProgressBar;
    GameObject cloneGoSavageLogoTitle, cloneGoSavageLogoBG, cloneGoSavageProgressBar;
    const string PlatLoadSavageLogoTitle = "Native/Prefabs/Savage/SavageLoadingLogoTitle";
    const string PlatLoadSavageLogoBG = "Native/Prefabs/Savage/SavageLoadingLogoBG";
    const string PlatLoadSavageProgressBar = "Native/Prefabs/Savage/SavageLoadingProgressbar";
    private ParticleSystem PSSavageLogoTitleCn, PSSavageLogoTitleEn;

    Dictionary<string, int> allProgress = new Dictionary<string, int>();
    Dictionary<string, int> curProgress = new Dictionary<string, int>();

    public class ShowMsgInfo
    {
        public string msg;
        public float progress = 0;
    }
    List<ShowMsgInfo> msgLst = new List<ShowMsgInfo>();


    private void InitParam()
    {
        btnQuit = goOwnerPage.GetChild("btnQuit").asButton;
        btnQuit.onClick.Clear();
        btnQuit.onClick.Add(DoAplicationQuit);
        btnQuit.visible = !ApplicationSettings.Instance.isRelease;


        pbLoading = goOwnerPage.GetChild("progress").asProgress;
        txtMsg = goOwnerPage.GetChild("title").asTextField;
        lodLogo = goOwnerPage.GetChild("logo").asLoader;
        titleProgress = goOwnerPage.GetChild("titleProgress")?.asTextField;
        //lodLogo.url = ApplicationSettings.Instance.logoUrl;

        msgLst.Clear();
        isError = false;
        allProgress.Clear();
        curProgress.Clear();
        pbLoading.value = 0;
        txtMsg.text = "";
        SyncTitleProgress(0f);

        Type t = typeof(LoadingProgress);
        var fields = t.GetFields();
        foreach (var fieldInfo in fields)
        {
            allProgress.Add((string)fieldInfo.GetRawConstantValue(), 0);
            curProgress.Add((string)fieldInfo.GetRawConstantValue(), 0);
        }

        // 按主题加载/挂载启动页动效
        string theme = ApplicationSettings.Instance?.gameTheme;
        if (!string.IsNullOrWhiteSpace(theme)
            && string.Equals(theme.Trim(), "Savage", StringComparison.OrdinalIgnoreCase))
            LoadAndBindSavage();
        else
            LoadAndBindTreasury();

        Timers.inst.Remove(Update);
        Timers.inst.Add(0.1f, 0, Update);
    }

    /// <summary>有锚点才加载 Resources Spine；无锚点直接跳过。</summary>
    void LoadAndBindTreasury()
    {
        if (goOwnerPage.GetChild("anchorBG") == null)
            return;

        if (goTLoadBG != null)
            BindTreasury();
        else
            LoadPlatTreasury(BindTreasury);
    }

    /// <summary>有 Savage 锚点才加载 Resources 预制体。</summary>
    void LoadAndBindSavage()
    {
        if (goOwnerPage.GetChild("anchorLogoBG") == null
            && goOwnerPage.GetChild("anchorLogoTitile") == null
            && goOwnerPage.GetChild("anchorProgressBar") == null)
            return;

        bool allLoaded = goSavageLogoTitle != null
            && goSavageLogoBG != null
            && goSavageProgressBar != null;
        if (allLoaded)
            BindSavage();
        else
            LoadPlatSavage(BindSavage);
    }

    /// <summary>从 Resources 异步加载启动页预制体。</summary>
    void LoadPlatTreasury(Action onDone)
    {
        var req = Resources.LoadAsync<GameObject>(PlatLoadTBgResPath);
        req.completed += _ =>
        {
            goTLoadBG = req.asset as GameObject;
            if (goTLoadBG == null)
            {
                Debug.LogError($"[PageLaunch] Resources 加载失败: {PlatLoadTBgResPath}");
                return;
            }
            onDone?.Invoke();
        };
    }

    /// <summary>从 Resources 异步加载 Savage 启动页预制体。</summary>
    void LoadPlatSavage(Action onDone)
    {
        int remain = 3;
        void OnOneDone()
        {
            if (--remain > 0)
                return;
            if (goSavageLogoTitle == null || goSavageLogoBG == null || goSavageProgressBar == null)
                return;
            onDone?.Invoke();
        }

        void LoadOne(string path, Action<GameObject> assign)
        {
            var req = Resources.LoadAsync<GameObject>(path);
            req.completed += _ =>
            {
                var go = req.asset as GameObject;
                if (go == null)
                {
                    Debug.LogError($"[PageLaunch] Resources 加载失败: {path}");
                    OnOneDone();
                    return;
                }
                assign(go);
                OnOneDone();
            };
        }

        LoadOne(PlatLoadSavageLogoTitle, go => goSavageLogoTitle = go);
        LoadOne(PlatLoadSavageLogoBG, go => goSavageLogoBG = go);
        LoadOne(PlatLoadSavageProgressBar, go => goSavageProgressBar = go);
    }

    /// <summary>财富主题挂到 FGUI 锚点（AOT 内联，不依赖 HotFix FguiUtils）。</summary>
    void BindTreasury()
    {
        var localAnchor = goOwnerPage.GetChild("anchorBG")?.asCom;
        if (localAnchor == null || goTLoadBG == null)
            return;

        if (anchorTLoadBG == localAnchor && cloneGoTLoadBG != null)
            return;

        DisposeTreasuryWrappers();

        var holder = localAnchor.GetChild("holder")?.asGraph;
        var example = localAnchor.GetChild("example")?.asLoader;
        if (holder == null)
        {
            Debug.LogError("[PageLaunch] anchorBG 缺少 holder");
            return;
        }

        cloneGoTLoadBG = UnityEngine.Object.Instantiate(goTLoadBG);
        cloneGoTLoadBG.transform.localPosition = Vector3.zero;
        cloneGoTLoadBG.transform.localScale = Vector3.one;

        holder.SetNativeObject(new GoWrapper(cloneGoTLoadBG));
        holder.SetPivot(0.5f, 0.5f, true);
        if (example != null)
        {
            holder.size = example.size;
            holder.scale = example.scale;
        }
        holder.xy = Vector2.zero;
        holder.visible = true;
        anchorTLoadBG = localAnchor;
    }

    /// <summary>动物主题挂到 FGUI 锚点（AOT 内联，不依赖 HotFix FguiUtils）。</summary>
    void BindSavage()
    {
        // FGUI 节点名为 anchorLogoTitile（拼写如此）
        var aTitle = goOwnerPage.GetChild("anchorLogoTitile")?.asCom;
        var aBG = goOwnerPage.GetChild("anchorLogoBG")?.asCom;
        var aBar = goOwnerPage.GetChild("anchorProgressBar")?.asCom;

        if (aTitle == null || aBG == null || aBar == null)
            return;
        if (goSavageLogoTitle == null || goSavageLogoBG == null || goSavageProgressBar == null)
            return;
        // 同一套锚点且已绑定则跳过
        if (anchorSavageLogoTitle == aTitle && cloneGoSavageLogoTitle != null
            && anchorSavageLogoBG == aBG && cloneGoSavageLogoBG != null
            && anchorSavageProgressBar == aBar && cloneGoSavageProgressBar != null)
            return;
        DisposeSavageWrappers();
        cloneGoSavageLogoTitle = BindOne(aTitle, goSavageLogoTitle, "anchorLogoTitile");
        cloneGoSavageLogoBG = BindOne(aBG, goSavageLogoBG, "anchorLogoBG");
        cloneGoSavageProgressBar = BindOne(aBar, goSavageProgressBar, "anchorProgressBar");
        if (cloneGoSavageLogoTitle == null || cloneGoSavageLogoBG == null || cloneGoSavageProgressBar == null)
            return;

        anchorSavageLogoTitle = aTitle;
        anchorSavageLogoBG = aBG;
        anchorSavageProgressBar = aBar;

        PSSavageLogoTitleCn = cloneGoSavageLogoTitle.transform.GetChild(0).GetChild(0).GetComponent<ParticleSystem>();
        PSSavageLogoTitleEn = cloneGoSavageLogoTitle.transform.GetChild(0).GetChild(1).GetComponent<ParticleSystem>();
        RefreshSavageLogoByLanguage("en");
        float progress01 = pbLoading != null ? (float)pbLoading.value : 0f;
        SyncSavageProgressBarTip(progress01);
        SyncTitleProgress(progress01);
    }

    /// <summary>
    /// 按语言切换 Savage Logo 中/英粒子。AOT 无法引用 SBoxModel，由外部传入；空则默认 cn。
    /// </summary>
    public void RefreshSavageLogoByLanguage(string lang = null)
    {
        if (string.IsNullOrEmpty(lang))
            lang = "cn";

        bool isEn = string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase);
        if (PSSavageLogoTitleCn != null)
            PSSavageLogoTitleCn.gameObject.SetActive(!isEn);
        if (PSSavageLogoTitleEn != null)
            PSSavageLogoTitleEn.gameObject.SetActive(isEn);
    }

    /// <summary>进度 tip 锚点 x 跟随进度条填充末端。</summary>
    void SyncSavageProgressBarTip(float progress01)
    {
        if (anchorSavageProgressBar == null || pbLoading == null)
            return;

        float p = Mathf.Clamp01(progress01);
        anchorSavageProgressBar.x = pbLoading.x + pbLoading.width * p;
    }

    /// <summary>titleProgress 显示整数百分比。</summary>
    void SyncTitleProgress(float progress01)
    {
        if (titleProgress == null)
            return;

        int percent = Mathf.RoundToInt(Mathf.Clamp01(progress01) * 100f);
        titleProgress.text = $"{percent}%";
    }

    GameObject BindOne(GComponent localAnchor, GameObject prefab, string anchorName)
    {
        var holder = localAnchor.GetChild("holder")?.asGraph;
        var example = localAnchor.GetChild("example")?.asLoader;
        if (holder == null)
        {
            Debug.LogError($"[PageLaunch] {anchorName} 缺少 holder");
            return null;
        }
        var clone = UnityEngine.Object.Instantiate(prefab);
        clone.transform.localPosition = Vector3.zero;
        clone.transform.localScale = Vector3.one;
        holder.SetNativeObject(new GoWrapper(clone));
        holder.SetPivot(0.5f, 0.5f, true);
        if (example != null)
        {
            holder.size = example.size;
            holder.scale = example.scale;
        }
        holder.xy = Vector2.zero;
        holder.visible = true;
        return clone;
    }

    /// <summary>释放 GoWrapper 与 clone，避免隐藏后仍占用渲染。</summary>
    void DisposeWrapper()
    {
        DisposeTreasuryWrappers();
        DisposeSavageWrappers();
    }

    void DisposeTreasuryWrappers()
    {
        if (anchorTLoadBG != null)
        {
            var holder = anchorTLoadBG.GetChild("holder")?.asGraph;
            // GoWrapper.Dispose 会 Destroy wrapTarget（即 cloneGoLoadBG）
            if (holder?.displayObject is GoWrapper wrapper)
                wrapper.Dispose();
        }

        cloneGoTLoadBG = null;
        anchorTLoadBG = null;
    }

    void DisposeSavageWrappers()
    {
        DisposeOneWrapper(ref anchorSavageLogoTitle, ref cloneGoSavageLogoTitle);
        DisposeOneWrapper(ref anchorSavageLogoBG, ref cloneGoSavageLogoBG);
        DisposeOneWrapper(ref anchorSavageProgressBar, ref cloneGoSavageProgressBar);
        PSSavageLogoTitleCn = null;
        PSSavageLogoTitleEn = null;
    }

    void DisposeOneWrapper(ref GComponent anchor, ref GameObject clone)
    {
        if (anchor != null)
        {
            var holder = anchor.GetChild("holder")?.asGraph;
            if (holder?.displayObject is GoWrapper wrapper)
                wrapper.Dispose();
        }
        clone = null;
        anchor = null;
    }

    void DoAplicationQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 编辑器中退出播放模式
#else
                    Application.Quit(); // 构建后退出应用
#endif
    }

    /// <summary>
    /// <p>获取进度条的值</p>
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// * 将进度条的加载分成多段。<br/>
    /// * 每段进度，再分成多个小段任务。<br/>
    /// </remarks>
    float GetProgressValue()
    {
        float partOne = 1f / (float)(allProgress.Count);

        float A = (float)(allProgress.Count - curProgress.Count) * partOne;

        float B = 0f;

        foreach (KeyValuePair<string, int> kv in curProgress)
        {
            int hasDoNum = allProgress[kv.Key] - kv.Value;
            if (hasDoNum > 0)
            {
                B += (float)hasDoNum * (partOne / (float)allProgress[kv.Key]);
            }
        }
        //Debug.Log($"@ A = {A} , B = {B} , C = {A + B} , partOne = {partOne}");
        return A + B;
    }

    /// <summary>
    /// <p>添加某个进度任务</p>
    /// </summary>
    /// <param name="mark">进度"mark"</param>
    /// <param name="count">该进度的任务个数</param>
    public void AddProgressCount(string mark, int count)
    {
        //新写法(支持重复校验-热更版本文件 - 避免网络连接延时)
        if (!curProgress.ContainsKey(mark))
        {
            curProgress.Add(mark, 0);
        }
        curProgress[mark] += count;
        allProgress[mark] += count;
    }


    /// <summary>
    /// <p>删除某个进度任务</p>
    /// </summary>
    /// <param name="mark">进度"mark"</param>
    public void RemoveProgress(string mark)
    {
        if (curProgress.ContainsKey(mark))
            curProgress.Remove(mark);
    }

    /// <summary>
    /// <p>显示加载进度和信息</p>
    /// </summary>
    /// <param name="mark">进度"mark"</param>
    /// <param name="str">显示的信息</param>
    /// <remarks>
    /// * 仅仅是界面的显示。<br/>
    /// * 不做任何的数据修改。<br/>
    /// </remarks>
    public void Next(string mark, string str)
    {
        if (isError) return;

        if (curProgress.ContainsKey(mark))
        {
            if (--curProgress[mark] < 0)
                curProgress[mark] = 0;
        }
        float val = GetProgressValue();

        msgLst.Add(new ShowMsgInfo()
        {
            msg = CreatStr(str, val),
            progress = val,
        });

#if DISABLE_DELAY
        ShowProgressUIMsg();
#endif
    }

    bool isError = false;
    public void Error(string str)
    {
        isError = true;
        txtMsg.text = str;
    }

    public void Finish(string str)
    {
        msgLst.Add(new ShowMsgInfo()
        {
            msg = CreatStr(str, 1),
            progress = 1,
        });
#if DISABLE_DELAY
        ShowProgressUIMsg();
#endif

    }

    string CreatStr(string str, float pg)
    {
        string _pg = (pg * 100f).ToString("N1");
        return ApplicationSettings.Instance.isRelease ? $"{_pg}%" : $"{str}  ({_pg})%";
    }

    public void Open()
    {
        goOwnerPage.visible = true;
        InitParam();
    }

    Coroutine corClose = null;
    public void Close(float delayS = -1)
    {
        if (delayS > 0)
        {
            DelayToClose(delayS);
            return;
        }
        CloseSelf(null);
    }

    void DelayToClose(float delayS)
    {
        Timers.inst.Remove(CloseSelf);
        Timers.inst.Add(delayS, 1, CloseSelf);
    }

    void CloseSelf(object data)
    {
        Timers.inst.Remove(Update);
        DisposeWrapper();
        goOwnerPage.visible = false;
    }

    float lastRunTimeS = 0;
    public void Update(object data)
    {
#if DISABLE_DELAY
        return;
#endif
        if (isError)
            return;

        float nowRunTimeS = Time.unscaledTime;
        if (nowRunTimeS - lastRunTimeS > 0.2f)
        {
            lastRunTimeS = nowRunTimeS;
            ShowProgressUIMsg();
        }
    }

    float curShowProgress = 0f;
    string curShowMsg = "";
    void ShowProgressUIMsg()
    {
        if (isError)
            return;

        if (msgLst.Count > 0)
        {
            ShowMsgInfo msgInfo = msgLst[0];
            msgLst.RemoveAt(0);

            curShowProgress = msgInfo.progress;
            curShowMsg = msgInfo.msg;

            pbLoading.value = curShowProgress;
            txtMsg.text = curShowMsg;
            SyncSavageProgressBarTip(curShowProgress);
            SyncTitleProgress(curShowProgress);
        }
    }

    /// <summary>
    /// 只显示内容
    /// </summary>
    /// <param name="msg"></param>
    public void RefreshProgressUIMsg(string msg)
    {
        if (isError)
            return;

        curShowMsg = CreatStr(msg, curShowProgress);

        pbLoading.value = curShowProgress;
        txtMsg.text = curShowMsg;
        SyncSavageProgressBarTip(curShowProgress);
        SyncTitleProgress(curShowProgress);
    }
}
