using FairyGUI;
using GameMaker;
using SBoxApi;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public class TestManager : Singleton<TestManager>
{
    GComponent goOwnerTestMgr,goGM, goPages,goCustomButtons, goKV, goAnalysis,goDebugMode;


    GButton btnMenu;

    GComponent goMenu;
    GList glstMenu;
    GRichTextField rtxtTip;

    public const string POP_GMS = "POP_GMS";
    public const string POP_PAGES = "POP_PAGES";
    public const string POP_BUTTONS = "POP_BUTTONS";
    public const string POP_DEBUGMODE = "POP_DEBUGMODE";

    string softwareVersion;

    //调试模式
    int DebugMode;
    GButton btnNormal, btnPointResData;
    int DebugResult;
    GButton btnLose, btnWin, btnFree, btnBonus, btnJP;
    int DebugBonusType,DebugJpType;
    GTextInput TInputBonusType, TInputJpType;
    GButton btnApply;

    //bool isEnableTestTool = true;

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

    public void LoadAssetBundleAsync(string pth, UnityAction<AssetBundle> onFinishCallback)
    {
        ResourceManager02.Instance.LoadAssetBundleAsync(pth, (bundle) =>
        {
            onFinishCallback?.Invoke(bundle);
        });
    }
    public void LoadAsset<T>(string pth, UnityAction<T> onFinishCallback) where T : UnityEngine.Object
    {
        ResourceManager02.Instance.LoadAsset<T>(pth, (asset) =>
        {
            onFinishCallback?.Invoke(asset);
        });
    }


    void InitParam()
    {
        pops.Clear();
        pops.Add(POP_GMS, goOwnerTestMgr.GetChild("popupGMs").asCom);
        pops.Add(POP_PAGES, goOwnerTestMgr.GetChild("popupPages").asCom);
        pops.Add(POP_BUTTONS, goOwnerTestMgr.GetChild("popupButtons").asCom);
        pops.Add(POP_DEBUGMODE, goOwnerTestMgr.GetChild("popupDebugMode").asCom);
        ChosePop();

        goMenu = goOwnerTestMgr.GetChild("menu").asCom;
        goMenu.visible = false;
        glstMenu = goMenu.GetChild("menu").asList;


        btnMenu = goOwnerTestMgr.GetChild("btnMenu").asButton;
        btnMenu.onClick.Clear();
        btnMenu.onClick.Add(OnClickBase);

        glstMenu.GetChildAt(0).asLabel.title = softwareVersion;// $"Ver {ApplicationSettings.Instance.appVersion}/{"--"}";
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

        btnLose= popupDebugMode.GetChild("Lose").asButton;
        btnLose.onClick.Clear();
        btnLose.onClick.Add(() => {OnClickResult(0);});

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

        btnApply= popupDebugMode.GetChild("apply").asButton;
        btnApply.onClick.Clear();
        btnApply.onClick.Add(OnClickApplyDebug);

        //goOwnerTestMgr.visible = isEnableTestTool;
        //goOwnerTestMgr.visible = false;

        goOwnerTestMgr.visible = !ApplicationSettings.Instance.isRelease;
    }

    public void ShowTip(string content)
    {
        if (rtxtTip != null)
            rtxtTip.text = content;
    }
    public void SetToolActive(bool active)
    {
        //return;
        //isEnableTestTool = active;
        if (goOwnerTestMgr != null)
            goOwnerTestMgr.visible = active;
    }

    public void OnFPSChange(string value)
    {
        glstMenu.GetChildAt(1).asLabel.title = value;
    }

    void OnClickSpeedX10()
    {
        Time.timeScale = 10;
    }
  
    void OnClickSpeedX2()
    {
        Time.timeScale = 2;
    }

    void OnClickSpeedX1()
    {
        Time.timeScale = 1;
    }

    void OnClickBase()
    {
        goMenu.visible = !goMenu.visible;
        ChosePop();

        //if (!goMenu.visible) OnCloseAll();

    }

    //void OnCloseAll(){ }

    #region KV

    Dictionary<string, string> customKV = new Dictionary<string, string>();

    public const string DATA_CUSTOM_BUTTON = "DATA_CUSTOM_BUTTON";
    public const string DATA_PAGES = "DATA_PAGES";
    public void SetKV(string key, string value)
    {
        if (!customKV.ContainsKey(key))
            customKV.Add(key, value);
        else
            customKV[key] = value;
    }

    public bool HasKey(string key) => customKV.ContainsKey(key);
    public bool HasKeyOnce(string key)
    {
        bool isHas = customKV.ContainsKey(key);
        customKV.Remove(key);
        return isHas;
    }
    public string GetValue(string key)
    {
        if (!customKV.ContainsKey(key))
        {
            return "";
        }
        return customKV[key];
    }
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

    Dictionary<string,GComponent> pops = new Dictionary<string,GComponent>();
    void ChosePop() => ChangePop("");
    private GComponent ChangePop(string popName = "")
    {
        GComponent goPop = null;
        foreach (KeyValuePair<string,GComponent> kv in pops)
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
            if(glst.numChildren < gmNode.Count)
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
                glst.GetChildAt(i).visible =  false;
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

    private void OnClickPageItem(string pageName, string data)
    {
        if (!string.IsNullOrEmpty(data))
        {
            EventCenter.Instance.EventTrigger<EventData>(GlobalEvent.ON_TOOL_EVENT,
               new EventData<Dictionary<string, object>>(GlobalEvent.PageButton,
                    new Dictionary<string, object>()
                    {
                        ["pageName"] = pageName,
                        ["pageData"] = data
                    }
                )
              );
        }
        ChosePop();
        OnClickBase();
    }

    #endregion

    bool isAnalysis = false;
    private void OnClickAnalysis()
    {
        isAnalysis = !isAnalysis;
        EventCenter.Instance.EventTrigger<EventData>(GlobalEvent.ON_TOOL_EVENT,
           new EventData<bool>(GlobalEvent.AnalysisTest, isAnalysis));
    }

    #region DebugMode
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
        }
    }

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

    public void OnClickApplyDebug()
    {
        DebugBonusType = Convert.ToInt32(TInputBonusType.text);
        DebugJpType = Convert.ToInt32(TInputJpType.text);
        SBoxDebugControlModeData sBoxDCM=new SBoxDebugControlModeData();
        sBoxDCM.mode = DebugMode;
        sBoxDCM.resType = DebugResult;
        sBoxDCM.bonusType = DebugBonusType;
        sBoxDCM.jpType = DebugJpType;
        ChosePop();
        Debug.Log("DebugMode:"+ sBoxDCM.mode+ ",  DebugResult:" + sBoxDCM.resType + ",  DebugBonusType:" + sBoxDCM.bonusType + ",  DebugJpType:" + sBoxDCM.jpType );
        SBoxIdea.DebugControlMode(sBoxDCM);
    }
    #endregion

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
                    OnClickCustomButtonItem((string)item.Value["event_type"], (string)item.Value["event_name"], (string)item.Value["event_data"]);
                });

                idx++;
            }
        }
    }

    private void OnClickCustomButtonItem(string eventType, string eventName, string eventData)
    {
        EventCenter.Instance.EventTrigger<EventData>(eventType, new EventData<string>(eventName, eventData));
    }

    #region GM

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
                LoadAsset<TextAsset>(ConfigUtils.curGameGMURL, (asset)=>
                {
                    SetKV(keyDataGM,asset.text);

                    CreatGMPop(goPop, asset.text);

                });                
            }
            else
            {
                CreatGMPop(goPop,str);
            }

        }
    }


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


}

