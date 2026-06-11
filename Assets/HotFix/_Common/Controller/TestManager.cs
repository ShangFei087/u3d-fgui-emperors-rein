using FairyGUI;
using GameMaker;
using SBoxApi;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 测试工具入口管理器，负责测试菜单、调试模式与 GM 面板逻辑。
/// </summary>
public class TestManager : Singleton<TestManager>
{
    enum AutoModeState
    {
        Idle = 0,
        Running = 1,
    }

    // TestManager 根节点与功能入口组件
    GComponent goOwnerTestMgr, goGM, goPages, goCustomButtons, goKV, goAnalysis, goDebugMode, goSelectProject,goAutoMode,goTableControl;
    // 菜单展开按钮
    GButton btnMenu;
    // 菜单容器、菜单列表与提示文本
    GComponent goMenu;
    GList glstMenu;
    GRichTextField rtxtTip;
    // 各功能弹窗标识
    public const string POP_GMS = "POP_GMS";
    public const string POP_PAGES = "POP_PAGES";
    public const string POP_BUTTONS = "POP_BUTTONS";
    public const string POP_DEBUGMODE = "POP_DEBUGMODE";
    public const string POP_SELECTPROJECT = "POP_SELECTPROJECT";
    public const string POP_TABLECONTROL = "POP_TABLECONTROL";
    // 当前软件版本显示文本
    string softwareVersion;
    // 调试模式状态与控件
    int DebugMode;
    GButton btnNormal, btnPointResData;
    int DebugResult;
    GButton btnLose, btnWin, btnFree, btnBonus, btnJP;
    int DebugBonusType, DebugJpType;
    GTextInput TInputBonusType, TInputJpType;
    GButton btnApply;
    GRichTextField rtxtTotalPlayTime, rtxtWinScore, rtxtPlayScore, rtxtWinBase, rtxtWinFree, rtxtWinBonus, rtxtWinJackpotBonus, rtxtWinJackpot, rtxtWinNetJackpot,
     rtxtTotalProb, rtxtLoseProb, rtxtWinProb, rtxtFreeGameProb, rtxtBonusGamesProb, rtxtJackpotProb, rtxtJackpotOnlineProb,
     rtxtTotalRTP, rtxtTotalRTPByParts, rtxtBaseRTP, rtxtFreeRTP, rtxtBounsRTP, rtxtJackpotRTP, rtxtJackpotOnlineRTP,
     rtxttotalPass, rtxttotalReject, rtxtwinRejectByTargetPool, rtxtfreeRejectByTargetPool, rtxtbonusRejectByTargetPool, rtxtjackpotRejectByTargetPool, rtxtfreeRejectByRange,
        rtxtbonusRejectByRange, rtxtjackpotRejectByRange, rtxtfreeRejectByPassRate, rtxtbonusRejectByPassRate, rtxtjackpotRejectByPassRate,
        rtxtBasePool, rtxtFreePool, rtxtBonusPool, rtxtJackpotPool, rtxtNetJackpotPool;


    //bool isEnableTestTool = true;

    // 初始化测试工具并异步加载 UI 资源
    public void Init(string softwareVersion)
    {
        this.softwareVersion = softwareVersion;
        LoadAssetBundleAsync("Assets/GameRes/Games/Common/FGUIs", (bundle) =>
        {
            UIPackage.AddPackage(bundle);

            goOwnerTestMgr = UIPackage.CreateObject("Common", "TestManager").asCom;
            GRoot.inst.AddChild(goOwnerTestMgr);
            goOwnerTestMgr.sortingOrder = 100;
            goOwnerTestMgr.y = GRoot.inst.height / 4;
            goOwnerTestMgr.x = 5;

            InitParam();
        });
    }

    // 异步加载 AssetBundle
    public void LoadAssetBundleAsync(string pth, UnityAction<AssetBundle> onFinishCallback)
    {
        ResourceManager02.Instance.LoadAssetBundleAsync(pth, (bundle) =>
        {
            onFinishCallback?.Invoke(bundle);
        });
    }

    // 按类型异步加载资源
    public void LoadAsset<T>(string pth, UnityAction<T> onFinishCallback) where T : UnityEngine.Object
    {
        ResourceManager02.Instance.LoadAsset<T>(pth, (asset) =>
        {
            onFinishCallback?.Invoke(asset);
        });
    }

    // 初始化菜单、弹窗和点击事件
    void InitParam()
    {
        pops.Clear();
        pops.Add(POP_GMS, goOwnerTestMgr.GetChild("popupGMs").asCom);
        pops.Add(POP_PAGES, goOwnerTestMgr.GetChild("popupPages").asCom);
        pops.Add(POP_BUTTONS, goOwnerTestMgr.GetChild("popupButtons").asCom);
        pops.Add(POP_DEBUGMODE, goOwnerTestMgr.GetChild("popupDebugMode").asCom);
        pops.Add(POP_SELECTPROJECT, goOwnerTestMgr.GetChild("selectProject").asCom); // CWY新增
        pops.Add(POP_TABLECONTROL, goOwnerTestMgr.GetChild("popupTableControl").asCom); // CWY新增
        ChosePop();

        goMenu = goOwnerTestMgr.GetChild("menu").asCom;
        goMenu.visible = false;
        glstMenu = goMenu.GetChild("menu").asList;


        btnMenu = goOwnerTestMgr.GetChild("btnMenu").asButton;
        btnMenu.onClick.Clear();
        btnMenu.onClick.Add(OnClickBase);

        glstMenu.GetChildAt(0).asLabel.title =softwareVersion; // $"Ver {ApplicationSettings.Instance.appVersion}/{"--"}";
        glstMenu.GetChildAt(1).asLabel.title = $"FPS {"--"}";

        goKV = glstMenu.GetChildAt(2).asCom;


        rtxtTip = goOwnerTestMgr.GetChild("tip").asRichTextField;
        rtxtTip.text = "";
        //rtxtTip.visible = !ApplicationSettings.Instance.isRelease;

        GComponent goSpeed = glstMenu.GetChildAt(3).asCom;

        GButton btnX1 = goSpeed.GetChild("x1").asButton;
        btnX1.onClick.Clear();
        btnX1.onClick.Add(OnClickSpeedX1);

        GButton btnX2 = goSpeed.GetChild("x2").asButton;
        btnX2.onClick.Clear();
        btnX2.onClick.Add(OnClickSpeedX2);

        GButton btnX10 = goSpeed.GetChild("x10").asButton;
        btnX10.onClick.Clear();
        btnX10.onClick.Add(OnClickSpeedX10);


        goGM = glstMenu.GetChildAt(4).asCom;
        goGM.onClick.Clear();
        goGM.onClick.Add(OnClickGMBaseButton);


        goPages = glstMenu.GetChildAt(5).asCom;
        goPages.onClick.Clear();
        goPages.onClick.Add(OnClickPages);

        goCustomButtons = glstMenu.GetChildAt(6).asCom;
        goCustomButtons.onClick.Clear();
        goCustomButtons.onClick.Add(OnClickCustomButons);

        FPS.Instance.onFPSChange.RemoveAllListeners();
        FPS.Instance.onFPSChange.AddListener(OnFPSChange);


        goAnalysis = glstMenu.GetChild("analysis").asCom;
        goAnalysis.onClick.Clear();
        goAnalysis.onClick.Add(OnClickAnalysis);

        goDebugMode = glstMenu.GetChild("debugMode").asCom;
        goDebugMode.onClick.Clear();
        goDebugMode.onClick.Add(OnClickDebugMode);

        GComponent popupDebugMode = goOwnerTestMgr.GetChild("popupDebugMode").asCom;
        btnNormal = popupDebugMode.GetChild("Normal").asButton;
        btnNormal.onClick.Clear();
        btnNormal.onClick.Add(() => { OnClickMode(0); });
        btnPointResData = popupDebugMode.GetChild("PointResData").asButton;
        btnPointResData.onClick.Clear();
        btnPointResData.onClick.Add(() => { OnClickMode(1); });

        btnLose = popupDebugMode.GetChild("Lose").asButton;
        btnLose.onClick.Clear();
        btnLose.onClick.Add(() => { OnClickResult(0); });

        btnWin = popupDebugMode.GetChild("Win").asButton;
        btnWin.onClick.Clear();
        btnWin.onClick.Add(() => { OnClickResult(1); });

        btnFree = popupDebugMode.GetChild("Free").asButton;
        btnFree.onClick.Clear();
        btnFree.onClick.Add(() => { OnClickResult(2); });

        btnBonus = popupDebugMode.GetChild("Bonus").asButton;
        btnBonus.onClick.Clear();
        btnBonus.onClick.Add(() => { OnClickResult(3); });

        btnJP = popupDebugMode.GetChild("Jp").asButton;
        btnJP.onClick.Clear();
        btnJP.onClick.Add(() => { OnClickResult(4); });

        TInputBonusType = popupDebugMode.GetChild("BonusType").asTextInput;
        TInputJpType = popupDebugMode.GetChild("JpType").asTextInput;

        btnApply = popupDebugMode.GetChild("apply").asButton;
        btnApply.onClick.Clear();
        btnApply.onClick.Add(OnClickApplyDebug);

        // cwy 新增
        GComponent selectProjectMenu = goOwnerTestMgr.GetChild("selectProject").asCom;
        GList lstProject = selectProjectMenu.GetChild("menu").asList;
        List<int> projectNumber = new List<int>()
        {
            1700,
            3996,
            3997,
            3998,
            3999
        };
        List<PageName> openPageNames = new List<PageName>()
        {
            PageName.SlotZhuZaiJinBiPopupGameLoading,
            PageName.CaiFuHuoChePopupGameLoading,
            PageName.CaiFuZhiJiaPopupGameLoading,
            PageName.XingYunZhiLunPopupGameLoading,
            PageName.CaiFuZhiMenPopupGameLoading
        };
        List<int> openPageId = new List<int>()
        {
            1700,
            3996,
            3997,
            3998,
            3999
        };

        List<PageName> resetPageNames = new List<PageName>()
        {
            // 1700
            PageName.SlotZhuZaiJinBiPageGameMain,
            PageName.SlotZhuZaiJinBiPopupBigWin,
            PageName.SlotZhuZaiJinBiPopupGameLoading,
            PageName.SlotZhuZaiJinBiPopupFreeSpinTrigger,

            // 3996
            PageName.CaiFuHuoChePopupGameLoading,
            PageName.CaiFuHuoChePopupFreeSpinTrigger,
            PageName.CaiFuHuoChePopupJackpotGameTrigger,
            PageName.CaiFuHuoChePopupJackpotGameExit,
            PageName.CaiFuHuoChePopupFreeSpinResult,
            PageName.CaiFuHuoChePageGameMain,

            // 3997
            PageName.CaiFuZhiJiaPopupGameLoading,
            PageName.CaiFuZhiJiaPageGameMain,
            PageName.CaiFuZhiJiaPopupOverWin,
            PageName.CaiFuZhiJiaPopupJackpotWin,
            PageName.CaiFuZhiJiaPopupFreeSpinTrigger,
            PageName.CaiFuZhiJiaPopupFreeSpinResult,
            PageName.CaiFuZhiJiaPopupSmallGameTrigger,
            PageName.CaiFuZhiJiaPopupSmallGameResult,

            // 3998
            PageName.XingYunZhiLunPopupGameLoading,
            PageName.XingYunZhiLunPageGameMain,
            PageName.XingYunZhiLunPopupJackpotGameResult,
            PageName.XingYunZhiLunPopupFreeSpinTrigger,
            PageName.XingYunZhiLunPopupFreeSpinResult,
            PageName.XingYunZhiLunPopupJackpotGameTrigger,
            PageName.XingYunZhiLunPopupJackpotGameExit,
            PageName.XingYunZhiLunPopupJackpotGameEnter,
            PageName.XingYunZhiLunPopupJackpotGameQuit,
            PageName.XingYunZhiLunPopupZhuanPan,

            // 3999
            PageName.CaiFuZhiMenPopupGameLoading,
            PageName.CaiFuZhiMenPageGameMain,
            PageName.CaiFuZhiMenPopupFreeSpinTrigger,
            PageName.CaiFuZhiMenPopupJackpotGame,
            PageName.CaiFuZhiMenPopupJackpotResult,
            PageName.CaiFuZhiMenPopupFreeSpinResult,
            PageName.CaiFuZhiMenPopupJackpotTrigger,
            PageName.CaiFuZhiMenPopupJackpotLoad,
        };

        goSelectProject = glstMenu.GetChildAt(9).asCom;
        goSelectProject.onClick.Clear();
        goSelectProject.onClick.Add(() => { selectProjectMenu.visible = true; });
        for (int i = 0; i < lstProject.numItems; i++)
        {
            int index = i;
            GButton btn = lstProject.GetChildAt(i).asButton;
            if (i > openPageId.Count-1)
            {
                btn.visible = false;
                continue;
            }
            btn.GetChild("title").asTextField.text = projectNumber[i].ToString();
            btn.onClick.Add((() =>
            {
                for (int j = 0; j < resetPageNames.Count; j++)
                {
                    if (PageManager.Instance.pageCacheDict.ContainsKey(resetPageNames[j]) &&
                        PageManager.Instance.pageCacheDict[resetPageNames[j]].IsOpen())
                        PageManager.Instance.ClosePage(resetPageNames[j]);
                }

                selectProjectMenu.visible = false;
                if (!ApplicationSettings.Instance.isMock)
                    SBoxIdea.GameSwitch(openPageId[index]);
                PageManager.Instance.OpenPage(openPageNames[index]);
            }));

            
        }

        goAutoMode = glstMenu.GetChildAt(10).asCom;
        goAutoMode.onClick.Clear();
        goAutoMode.onClick.Add(OnClickAutoMode);

        //调控窗口
        goTableControl = glstMenu.GetChild("tablecontrol").asCom;
        goTableControl.onClick.Clear();
        goTableControl.onClick.Add(OnClickTableControl);
        GComponent popupTableControl = goOwnerTestMgr.GetChild("popupTableControl").asCom;
        //汇总测试信息
        rtxtTotalPlayTime = popupTableControl.GetChild("TotalPlayTime").asCom.GetChild("value").asRichTextField;
        rtxtWinScore = popupTableControl.GetChild("WinScore").asCom.GetChild("value").asRichTextField;
        rtxtPlayScore = popupTableControl.GetChild("PlayScore").asCom.GetChild("value").asRichTextField;
        rtxtWinBase = popupTableControl.GetChild("paidBase").asCom.GetChild("value").asRichTextField;
        rtxtWinFree = popupTableControl.GetChild("paidFree").asCom.GetChild("value").asRichTextField;
        rtxtWinBonus = popupTableControl.GetChild("paidBonus").asCom.GetChild("value").asRichTextField;
        rtxtWinJackpotBonus = popupTableControl.GetChild("paidJackpotBonus").asCom.GetChild("value").asRichTextField;
        rtxtWinJackpot = popupTableControl.GetChild("paidJackpot").asCom.GetChild("value").asRichTextField;
        rtxtWinNetJackpot = popupTableControl.GetChild("paidNetJackpot").asCom.GetChild("value").asRichTextField;
        rtxtTotalPlayTime.text = "0";
        rtxtWinScore.text = "0";
        rtxtPlayScore.text = "0";
        rtxtWinBase.text = "0";
        rtxtWinFree.text = "0";
        rtxtWinBonus.text = "0";
        rtxtWinJackpotBonus.text = "0";
        rtxtWinJackpot.text = "0";
        rtxtWinNetJackpot.text = "0";
        //输赢概率
        rtxtTotalProb = popupTableControl.GetChild("TotalProb").asCom.GetChild("value").asRichTextField;
        rtxtLoseProb = popupTableControl.GetChild("LoseProb").asCom.GetChild("value").asRichTextField;
        rtxtWinProb= popupTableControl.GetChild("WinProb").asCom.GetChild("value").asRichTextField;
        rtxtFreeGameProb = popupTableControl.GetChild("FreeGameProb").asCom.GetChild("value").asRichTextField;
        rtxtBonusGamesProb = popupTableControl.GetChild("BonusGamesProb").asCom.GetChild("value").asRichTextField;
        rtxtJackpotProb = popupTableControl.GetChild("JackpotProb").asCom.GetChild("value").asRichTextField;
        rtxtJackpotOnlineProb = popupTableControl.GetChild("JackpotOnlineProb").asCom.GetChild("value").asRichTextField;
        rtxtTotalProb.text = "0";
        rtxtLoseProb.text = "0";
        rtxtWinProb.text = "0";
        rtxtFreeGameProb.text = "0";
        rtxtBonusGamesProb.text = "0";
        rtxtJackpotProb.text = "0";
        rtxtJackpotOnlineProb.text = "0";
        //返还率
        rtxtTotalRTP = popupTableControl.GetChild("TotalRTP").asCom.GetChild("value").asRichTextField;
        rtxtTotalRTPByParts = popupTableControl.GetChild("TotalRTPByParts").asCom.GetChild("value").asRichTextField;
        rtxtBaseRTP = popupTableControl.GetChild("BaseRTP").asCom.GetChild("value").asRichTextField;
        rtxtFreeRTP = popupTableControl.GetChild("FreeRTP").asCom.GetChild("value").asRichTextField;
        rtxtBounsRTP = popupTableControl.GetChild("BounsRTP").asCom.GetChild("value").asRichTextField;
        rtxtJackpotRTP = popupTableControl.GetChild("JackpotRTP").asCom.GetChild("value").asRichTextField;
        rtxtJackpotOnlineRTP = popupTableControl.GetChild("JackpotOnlineRTP").asCom.GetChild("value").asRichTextField;
        rtxtTotalRTP.text = "0";
        rtxtTotalRTPByParts.text = "0";
        rtxtBaseRTP.text = "0";
        rtxtFreeRTP.text = "0";
        rtxtBounsRTP.text = "0";
        rtxtJackpotRTP.text = "0";
        rtxtJackpotOnlineRTP.text = "0";
        //调控信息
        rtxttotalPass = popupTableControl.GetChild("totalPass").asCom.GetChild("value").asRichTextField;
        rtxttotalReject = popupTableControl.GetChild("totalReject").asCom.GetChild("value").asRichTextField;
        rtxtwinRejectByTargetPool = popupTableControl.GetChild("winRejectByTargetPool").asCom.GetChild("value").asRichTextField;
        rtxtfreeRejectByTargetPool = popupTableControl.GetChild("freeRejectByTargetPool").asCom.GetChild("value").asRichTextField;
        rtxtbonusRejectByTargetPool = popupTableControl.GetChild("bonusRejectByTargetPool").asCom.GetChild("value").asRichTextField;
        rtxtjackpotRejectByTargetPool = popupTableControl.GetChild("jackpotRejectByTargetPool").asCom.GetChild("value").asRichTextField;
        rtxtfreeRejectByRange = popupTableControl.GetChild("freeRejectByRange").asCom.GetChild("value").asRichTextField;
        rtxtbonusRejectByRange = popupTableControl.GetChild("bonusRejectByRange").asCom.GetChild("value").asRichTextField;
        rtxtjackpotRejectByRange = popupTableControl.GetChild("jackpotRejectByRange").asCom.GetChild("value").asRichTextField;
        rtxtfreeRejectByPassRate = popupTableControl.GetChild("freeRejectByPassRate").asCom.GetChild("value").asRichTextField;
        rtxtbonusRejectByPassRate = popupTableControl.GetChild("bonusRejectByPassRate").asCom.GetChild("value").asRichTextField;
        rtxtjackpotRejectByPassRate = popupTableControl.GetChild("jackpotRejectByPassRate").asCom.GetChild("value").asRichTextField;
        rtxttotalPass.text = "0";
        rtxttotalReject.text = "0";
        rtxtwinRejectByTargetPool.text = "0";
        rtxtfreeRejectByTargetPool.text = "0";
        rtxtbonusRejectByTargetPool.text="0";
        rtxtjackpotRejectByTargetPool.text = "0";
        rtxtfreeRejectByRange.text = "0";
        rtxtbonusRejectByRange.text = "0";
        rtxtjackpotRejectByRange.text = "0";
        rtxtfreeRejectByPassRate.text = "0";
        rtxtbonusRejectByPassRate.text = "0";
        rtxtjackpotRejectByPassRate.text = "0";
        //算法调控五池信息
        rtxtBasePool = popupTableControl.GetChild("gBasePool").asCom.GetChild("value").asRichTextField;
        rtxtFreePool = popupTableControl.GetChild("gFreePool").asCom.GetChild("value").asRichTextField;
        rtxtBonusPool = popupTableControl.GetChild("gBonusPool").asCom.GetChild("value").asRichTextField;
        rtxtJackpotPool = popupTableControl.GetChild("gJackpotPool").asCom.GetChild("value").asRichTextField; 
        rtxtNetJackpotPool = popupTableControl.GetChild("gNetJackpotPool").asCom.GetChild("value").asRichTextField;
        rtxtBasePool.text = "0";
        rtxtFreePool.text = "0";
        rtxtBonusPool.text = "0";
        rtxtJackpotPool.text = "0";
        rtxtNetJackpotPool.text = "0";

        // 监听算法卡调试信息回包（由 SBoxIdea.GetDebugInfoR 触发）
        EventCenter.Instance.RemoveEventListener<SBoxDebugInfo>(SBoxEventHandle.SBOX_DEBUG_INFO, OnSBoxDebugInfoChanged);
        EventCenter.Instance.AddEventListener<SBoxDebugInfo>(SBoxEventHandle.SBOX_DEBUG_INFO, OnSBoxDebugInfoChanged);
        // 监听算法卡调控信息回包（由 SBoxIdea.GetTableControlR 触发）
        EventCenter.Instance.RemoveEventListener<TableControlStats>(SBoxEventHandle.SBOX_TABLECONTROL_INFO, OnSBoxTableControlChanged);
        EventCenter.Instance.AddEventListener<TableControlStats>(SBoxEventHandle.SBOX_TABLECONTROL_INFO, OnSBoxTableControlChanged);
        // 监听算法卡调控五池信息回包（由 SBoxIdea.GetTableControlR 触发）
        EventCenter.Instance.RemoveEventListener<TableControlPool>(SBoxEventHandle.SBOX_TABLECONTROLPOOL_INFO, OnSBoxTableControlPool);
        EventCenter.Instance.AddEventListener<TableControlPool>(SBoxEventHandle.SBOX_TABLECONTROLPOOL_INFO, OnSBoxTableControlPool);

        //goOwnerTestMgr.visible = isEnableTestTool;
        //goOwnerTestMgr.visible = false;

        goOwnerTestMgr.visible = !ApplicationSettings.Instance.isRelease;
    }

    // 显示顶部提示文本
    public void ShowTip(string content)
    {
        if (rtxtTip != null)
            rtxtTip.text = content;
    }

    // 外部控制测试工具显示状态
    public void SetToolActive(bool active)
    {
        //return;
        //isEnableTestTool = active;
        if (goOwnerTestMgr != null)
            goOwnerTestMgr.visible = active;
    }

    // FPS 变化回调
    public void OnFPSChange(string value)
    {
        glstMenu.GetChildAt(1).asLabel.title = value;
    }

    // 设置 10 倍速
    void OnClickSpeedX10()
    {
        Time.timeScale = 10;
    }

    // 设置 2 倍速
    void OnClickSpeedX2()
    {
        Time.timeScale = 2;
    }

    // 设置 1 倍速
    void OnClickSpeedX1()
    {
        Time.timeScale = 1;
    }

    // 主菜单按钮点击：切换菜单并重置弹窗显示
    void OnClickBase()
    {
        goMenu.visible = !goMenu.visible;
        ChosePop();

        //if (!goMenu.visible) OnCloseAll();
    }

    #region KV

    // 运行时 KV 缓存
    Dictionary<string, string> customKV = new Dictionary<string, string>();

    // 自定义按钮配置键
    public const string DATA_CUSTOM_BUTTON = "DATA_CUSTOM_BUTTON";
    // 页面配置键
    public const string DATA_PAGES = "DATA_PAGES";

    // 设置 KV（存在则覆盖）
    public void SetKV(string key, string value)
    {
        if (!customKV.ContainsKey(key))
            customKV.Add(key, value);
        else
            customKV[key] = value;
    }

    // 检查键是否存在
    public bool HasKey(string key) => customKV.ContainsKey(key);

    // 检查键是否存在并移除（一次性）
    public bool HasKeyOnce(string key)
    {
        bool isHas = customKV.ContainsKey(key);
        customKV.Remove(key);
        return isHas;
    }

    // 获取键值，不存在返回空字符串
    public string GetValue(string key)
    {
        if (!customKV.ContainsKey(key))
        {
            return "";
        }

        return customKV[key];
    }

    // 获取键值并移除（一次性）
    public string GetValueOnce(string key)
    {
        string res = "";
        if (customKV.ContainsKey(key))
        {
            res = customKV[key];
            customKV.Remove(key);
        }

        return res;
    }

    #endregion

    // 弹窗缓存字典
    Dictionary<string, GComponent> pops = new Dictionary<string, GComponent>();
    // 关闭当前所有弹窗
    void ChosePop() => ChangePop("");

    // 切换目标弹窗显示并关闭其他弹窗
    private GComponent ChangePop(string popName = "")
    {
        GComponent goPop = null;
        foreach (KeyValuePair<string, GComponent> kv in pops)
        {
            if (kv.Key == popName)
            {
                goPop = kv.Value;
                kv.Value.visible = !kv.Value.visible;
            }
            else
            {
                kv.Value.visible = false;
            }
        }

        return goPop;
    }


    #region Button Page

    // 页面功能按钮点击：动态生成页面列表
    public void OnClickPages()
    {
        if (goPages == null || !HasKey(DATA_PAGES))
            return;

        GComponent goPop = ChangePop(POP_PAGES);

        if (goPop != null && goPop.visible)
        {
            string str = GetValue(DATA_PAGES);

            JSONNode _gmNode = JSONNode.Parse(str);

            JSONNode gmNode = JSONNode.Parse("{}");
            foreach (KeyValuePair<string, JSONNode> item in _gmNode)
            {
                if (item.Key.StartsWith("//"))
                    continue;
                gmNode.Add(item.Key, item.Value);
            }

            GList glst = goPop.GetChild("menu").asList;
            if (glst.numChildren < gmNode.Count)
            {
                glst.numItems = gmNode.Count;
            }

            GObject[] items = glst.GetChildren();
            foreach (GObject item in items)
            {
                item.asButton.onClick.Clear();
            }

            for (int i = gmNode.Count; i < glst.numChildren; i++)
            {
                glst.GetChildAt(i).visible = false;
            }

            int idx = 0;
            foreach (KeyValuePair<string, JSONNode> item in gmNode)
            {
                GButton tfm = glst.GetChildAt(idx).asButton;
                tfm.visible = true;
                string showName = item.Value.HasKey("nick_name") ? (string)item.Value["nick_name"] : item.Key;
                tfm.title = showName;

                tfm.onClick.Add(() =>
                {
                    OnClickPageItem((string)item.Value["page_name"], (string)item.Value["data"]);
                });
                idx++;
            }
        }
    }

    // 页面项点击：派发页面打开事件
    private void OnClickPageItem(string pageName, string data)
    {
        if (!string.IsNullOrEmpty(data))
        {
            EventCenter.Instance.EventTrigger<EventData>(GlobalEvent.ON_TOOL_EVENT,
                new EventData<Dictionary<string, object>>(GlobalEvent.PageButton,
                    new Dictionary<string, object>() { ["pageName"] = pageName, ["pageData"] = data }
                )
            );
        }

        ChosePop();
        OnClickBase();
    }

    #endregion

    #region Analysis
    // 分析模式开关状态
    bool isAnalysis = false;
    AutoModeState autoModeState = AutoModeState.Idle;
    const float AUTO_SPIN_INTERVAL_SECONDS = 1.0f;
    // 分析按钮点击：切换分析模式并广播事件
    private void OnClickAnalysis()
    {
        isAnalysis = !isAnalysis;
        EventCenter.Instance.EventTrigger<EventData>(GlobalEvent.ON_TOOL_EVENT,
            new EventData<bool>(GlobalEvent.AnalysisTest, isAnalysis));
    }
    #endregion

    #region DebugMode


    // 调试模式按钮点击：打开面板并重置状态
    public void OnClickDebugMode()
    {
        if (ApplicationSettings.Instance.isMock || goDebugMode == null)
        {
            return;
        }

        GComponent goPop = ChangePop(POP_DEBUGMODE);

        if (goPop != null && goPop.visible)
        {
            btnPointResData.selected = false;
            btnPointResData.selected = false;
            btnLose.selected = false;
            btnWin.selected = false;
            btnFree.selected = false;
            btnBonus.selected = false;
            btnJP.selected = false;
            DebugMode = 0;
            DebugResult = 0;
            DebugBonusType = 0;
            DebugJpType = 0;
            TInputBonusType.text = "0";
            TInputJpType.text = "0";

            // 打开面板时请求一次算法调试统计
            SBoxIdea.GetDebugInfo();
        }
    }

    // 选择调试模式
    public void OnClickMode(int index)
    {
        DebugMode = index;
        if (index == 0)
        {
            btnPointResData.selected = false;
        }
        else
        {
            btnNormal.selected = false;
        }
    }

    // 选择调试结果类型
    public void OnClickResult(int index)
    {
        DebugResult = index;
        switch (index)
        {
            case 0:
                btnWin.selected = false;
                btnFree.selected = false;
                btnBonus.selected = false;
                btnJP.selected = false;
                break;
            case 1:
                btnLose.selected = false;
                btnFree.selected = false;
                btnBonus.selected = false;
                btnJP.selected = false;
                break;
            case 2:
                btnLose.selected = false;
                btnWin.selected = false;
                btnBonus.selected = false;
                btnJP.selected = false;
                break;
            case 3:
                btnLose.selected = false;
                btnWin.selected = false;
                btnFree.selected = false;
                btnJP.selected = false;
                break;
            case 4:
                btnLose.selected = false;
                btnWin.selected = false;
                btnFree.selected = false;
                btnBonus.selected = false;
                break;
        }
    }

    // 应用调试参数并发送到底层调试接口
    public void OnClickApplyDebug()
    {
        DebugBonusType = Convert.ToInt32(TInputBonusType.text);
        DebugJpType = Convert.ToInt32(TInputJpType.text);
        SBoxDebugControlModeData sBoxDCM = new SBoxDebugControlModeData();
        sBoxDCM.mode = DebugMode;
        sBoxDCM.resType = DebugResult;
        sBoxDCM.bonusType = DebugBonusType;
        sBoxDCM.jpType = DebugJpType;
        ChosePop();
        Debug.Log("DebugMode:" + sBoxDCM.mode + ",  DebugResult:" + sBoxDCM.resType + ",  DebugBonusType:" +
                  sBoxDCM.bonusType + ",  DebugJpType:" + sBoxDCM.jpType);
        SBoxIdea.DebugControlMode(sBoxDCM);
    }

    //调控弹窗打开
    public void OnClickTableControl ()
    {
        if (ApplicationSettings.Instance.isMock || goDebugMode == null)
        {
            return;
        }

        GComponent goPop = ChangePop(POP_TABLECONTROL);

        if (goPop != null && goPop.visible)
        {
            // 打开面板时请求一次算法调试统计
            SBoxIdea.GetDebugInfo();
            SBoxIdea.GetTableControlInfo();
            SBoxIdea.GetTableControlPoolInfo();
        }
    }

    // 接收算法调试统计信息并刷新 Debug 面板
    private void OnSBoxDebugInfoChanged(SBoxDebugInfo debugInfo)
    {
        if (debugInfo == null)
        {
            return;
        }

        long totalPlayTime = debugInfo.dwTotalPlayTime;
        long playScore = debugInfo.dwPlayScore;
        long winScore = debugInfo.dwWinScore;

        rtxtTotalPlayTime.text = totalPlayTime.ToString();
        rtxtWinScore.text = winScore.ToString();
        rtxtPlayScore.text = playScore.ToString();

        rtxtTotalProb.text = FormatPercent(CalcRatio(totalPlayTime, totalPlayTime));
        rtxtLoseProb.text = FormatPercent(CalcRatio(debugInfo.dwLooseTime, totalPlayTime));
        rtxtWinProb.text = FormatPercent(CalcRatio(debugInfo.dwNormalWinTime, totalPlayTime));
        rtxtFreeGameProb.text = FormatPercent(CalcRatio(debugInfo.dwFreeGameTime, totalPlayTime));
        rtxtBonusGamesProb.text = FormatPercent(CalcRatio(debugInfo.dwBonusTime, totalPlayTime));
        rtxtJackpotProb.text = FormatPercent(CalcRatio(debugInfo.dwJackpotTime, totalPlayTime));
        rtxtJackpotOnlineProb.text = FormatPercent(CalcRatio(debugInfo.dwJackpotOnlineTime, totalPlayTime));

        double baseRtp = CalcRatio(debugInfo.dwBaseWinScore, playScore);
        double freeRtp = CalcRatio(debugInfo.dwFreeWinScore, playScore);
        double bonusRtp = CalcRatio(debugInfo.dwBonusWinScore, playScore);
        double jackpotRtp = CalcRatio(debugInfo.dwJackpotWinScore, playScore);
        double jackpotOnlineRtp = CalcRatio(debugInfo.dwJackpotOnlineWinScore, playScore);

        rtxtTotalRTP.text = FormatPercent(CalcRatio(winScore, playScore));
        rtxtTotalRTPByParts.text = FormatPercent(baseRtp + freeRtp + bonusRtp + jackpotRtp + jackpotOnlineRtp);
        rtxtBaseRTP.text = FormatPercent(baseRtp);
        rtxtFreeRTP.text = FormatPercent(freeRtp);
        rtxtBounsRTP.text = FormatPercent(bonusRtp);
        rtxtJackpotRTP.text = FormatPercent(jackpotRtp);
        rtxtJackpotOnlineRTP.text = FormatPercent(jackpotOnlineRtp);
    }

    // 接收算法调控统计信息并刷新 Debug 面板
    private void OnSBoxTableControlChanged(TableControlStats tablecontrolInfo)
    {
        if (tablecontrolInfo == null)
        {
            return;
        }
        rtxtWinBase.text = tablecontrolInfo.paidBase.ToString();
        rtxtWinFree.text = tablecontrolInfo.paidFree.ToString();
        rtxtWinBonus.text = tablecontrolInfo.paidBonus.ToString();
        rtxtWinJackpotBonus.text = tablecontrolInfo.paidJackpotBonus.ToString();
        rtxtWinJackpot.text = tablecontrolInfo.paidJackpot.ToString();
        rtxtWinNetJackpot.text = tablecontrolInfo.paidNetJackpot.ToString();

        rtxttotalPass.text = tablecontrolInfo.totalPass.ToString();
        rtxttotalReject.text = tablecontrolInfo.totalReject.ToString();
        rtxtwinRejectByTargetPool.text = tablecontrolInfo.winRejectByTargetPool.ToString();
        rtxtfreeRejectByTargetPool.text = tablecontrolInfo.freeRejectByTargetPool.ToString();
        rtxtbonusRejectByTargetPool.text= tablecontrolInfo.bonusRejectByTargetPool.ToString();
        rtxtjackpotRejectByTargetPool.text = tablecontrolInfo.jackpotRejectByTargetPool.ToString();

        rtxtfreeRejectByRange.text = tablecontrolInfo.freeRejectByRange.ToString();
        rtxtbonusRejectByRange.text = tablecontrolInfo.bonusRejectByRange.ToString();
        rtxtjackpotRejectByRange.text = tablecontrolInfo.jackpotRejectByRange.ToString();
        rtxtfreeRejectByPassRate.text = tablecontrolInfo.freeRejectByPassRate.ToString();
        rtxtbonusRejectByPassRate.text = tablecontrolInfo.bonusRejectByPassRate.ToString();
        rtxtjackpotRejectByPassRate.text = tablecontrolInfo.jackpotRejectByPassRate.ToString();
    }

    // 接收算法调控统计信息并刷新 Debug 面板
    private void OnSBoxTableControlPool(TableControlPool tablecontrolpoolInfo)
    {
        if (tablecontrolpoolInfo == null)
        {
            return;
        }
     
        rtxtBasePool.text = tablecontrolpoolInfo.BasePool.ToString();
        rtxtFreePool.text = tablecontrolpoolInfo.FreePool.ToString();
        rtxtBonusPool.text = tablecontrolpoolInfo.BonusPool.ToString();
        rtxtJackpotPool.text = tablecontrolpoolInfo.JackpotPool.ToString();
        rtxtNetJackpotPool.text = tablecontrolpoolInfo.NetJackpotPool.ToString();
;
    }

    private double CalcRatio(long numerator, long denominator)
    {
        if (denominator <= 0)
        {
            return 0d;
        }

        return numerator * 1.0d / denominator;
    }

    private string FormatPercent(double ratio)
    {
        return $"{ratio * 100d:F2}%";
    }

    #endregion

    // 自定义按钮入口点击：动态生成按钮列表
    public void OnClickCustomButons()
    {
        if (goCustomButtons == null || !HasKey(DATA_CUSTOM_BUTTON))
            return;

        GComponent goPop = ChangePop(POP_BUTTONS);

        if (goPop != null && goPop.visible)
        {
            string str = GetValue(DATA_CUSTOM_BUTTON);

            JSONNode _gmNode = JSONNode.Parse(str);

            JSONNode gmNode = JSONNode.Parse("{}");
            foreach (KeyValuePair<string, JSONNode> item in _gmNode)
            {
                if (item.Key.StartsWith("//"))
                    continue;
                gmNode.Add(item.Key, item.Value);
            }

            GList glst = goPop.GetChild("menu").asList;
            if (glst.numChildren < gmNode.Count)
            {
                glst.numItems = gmNode.Count;
            }

            GObject[] items = glst.GetChildren();
            foreach (GObject item in items)
            {
                item.asButton.onClick.Clear();
            }

            for (int i = gmNode.Count; i < glst.numChildren; i++)
            {
                glst.GetChildAt(i).visible = false;
            }


            int idx = 0;
            foreach (KeyValuePair<string, JSONNode> item in gmNode)
            {
                GButton tfm = glst.GetChildAt(idx).asButton;
                tfm.visible = true;
                string showName = item.Value.HasKey("nick_name") ? (string)item.Value["nick_name"] : item.Key;
                tfm.title = showName;

                tfm.onClick.Add(() =>
                {
                    OnClickCustomButtonItem((string)item.Value["event_type"], (string)item.Value["event_name"],
                        (string)item.Value["event_data"]);
                });

                idx++;
            }
        }
    }

    // 自定义按钮项点击：派发自定义事件
    private void OnClickCustomButtonItem(string eventType, string eventName, string eventData)
    {
        EventCenter.Instance.EventTrigger<EventData>(eventType, new EventData<string>(eventName, eventData));
    }

    #region GM

    // GM 按钮点击：加载并展示当前游戏 GM 列表
    public void OnClickGMBaseButton()
    {
        if (goGM == null || MainModel.Instance.gameID == -1)
            return;

        GComponent goPop = ChangePop(POP_GMS);

        if (goPop != null && goPop.visible)
        {
            string keyDataGM = $"DATA_GM_{ConfigUtils.curGameId}";

            string str = GetValue(keyDataGM);

            if (string.IsNullOrEmpty(str))
            {
                LoadAsset<TextAsset>(ConfigUtils.curGameGMURL, (asset) =>
                {
                    SetKV(keyDataGM, asset.text);

                    CreatGMPop(goPop, asset.text);
                });
            }
            else
            {
                CreatGMPop(goPop, str);
            }
        }
    }


    // 根据 GM 配置创建按钮并绑定事件
    void CreatGMPop(GComponent goPop, string jsn)
    {
        JSONNode _gmNode = JSONNode.Parse(jsn);

        int gameId = (int)_gmNode["game_id"];

        JSONNode gmNode = JSONNode.Parse("{}");
        foreach (KeyValuePair<string, JSONNode> item in _gmNode["gm_event"])
        {
            if (item.Key.StartsWith("//"))
                continue;
            gmNode.Add(item.Key, item.Value);
        }

        GList glst = goPop.GetChild("menu").asList;
        if (glst.numChildren < gmNode.Count)
        {
            glst.numItems = gmNode.Count;
        }

        GObject[] items = glst.GetChildren();
        foreach (GObject item in items)
        {
            item.asButton.onClick.Clear();
        }

        for (int i = gmNode.Count; i < glst.numChildren; i++)
        {
            glst.GetChildAt(i).visible = false;
        }


        int idx = 0;
        foreach (KeyValuePair<string, JSONNode> item in gmNode)
        {
            GButton tfm = glst.GetChildAt(idx).asButton;
            tfm.visible = true;
            string showName = item.Value.HasKey("nick_name") ? (string)item.Value["nick_name"] : item.Key;
            tfm.title = showName;

            string name = (string)item.Value["event_name"];
            string val = (string)item.Value["value"];
            tfm.onClick.Add(() =>
            {
                EventData data = new EventData<string>(name, gameId, val);
                EventCenter.Instance.EventTrigger<EventData>(GlobalEvent.ON_GM_EVENT, data);
                OnClickBase();
            });

            idx++;
        }
    }

    #endregion

    #region AutoMode
    public void OnClickAutoMode()
    {
        if (autoModeState == AutoModeState.Running)
        {
            autoModeState = AutoModeState.Idle;
            ShowTip("AutoMode: Stop");
            StopAutoSpinTicker();
            return;
        }

        autoModeState = AutoModeState.Running;
        ShowTip("AutoMode: Running");
        StartAutoSpinTicker();
    }

    public bool IsAutoModeRunning
    {
        get
        {
            return autoModeState == AutoModeState.Running;
        }
    }

    private void StartAutoSpinTicker()
    {
        Timers.inst.Remove(DoAutoSpinTick);
        Timers.inst.Add(0.1f, 1, DoAutoSpinTick);
        Timers.inst.Add(AUTO_SPIN_INTERVAL_SECONDS, 0, DoAutoSpinTick);
    }

    private void StopAutoSpinTicker()
    {
        Timers.inst.Remove(DoAutoSpinTick);
    }

    private void DoAutoSpinTick(object _)
    {
        if (autoModeState != AutoModeState.Running)
        {
            return;
        }
        EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT,
            new EventData<bool>(PanelEvent.SpinButtonClick, false));
    }
    #endregion
}