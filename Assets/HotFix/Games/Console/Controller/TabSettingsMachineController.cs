using FairyGUI;
using GameMaker;
using System;
using System.Collections.Generic;
using UnityEngine;
using GameUtil;
using UnityEngine.Events;
using SBoxApi;
using ConsoleSlot01;
using static ConsoleSlot01.ChangePasswordController;
using System.Globalization;

public class TabSettingsMachineController
{
    GComponent _comp;

    bool _isSubscribed;

    GButton btnAgentID, btnMachineID, btnGameDifficulty,
        btnChangePwdShift, btnChangePwdManager, btnChangePwdAdmin,
        btnMaxCoinInOutRecord, btnMaxGameRecord, btnMaxEventRecord, btnMaxErrorRecord, btnMaxBusinessDayRecord,
        btnCoding;


    GComponent cmpBetAllowed;

    //GRichTextField rtxtDifficulty;

    /// 修改用户密码
    ChangePasswordController adminChangePwdCtrl = new ChangePasswordController(UserType.Admin);
    ChangePasswordController managerChangePwdCtrl = new ChangePasswordController(UserType.Manager);
    ChangePasswordController shiftChangePwdCtrl = new ChangePasswordController(UserType.Shift);

    /// <summary>
    /// 初始化 Tab 页面所需的 UI 组件引用，并绑定按钮点击/开关变更事件。
    /// </summary>
    /// <param name="comp">Tab 根组件。</param>
    public  void InitParam(GComponent comp)
    {
        _comp = comp;


        //rtxtDifficulty = _comp.GetChild("difficulty").asCom.GetChild("value").asRichTextField;
        //rtxtDifficulty.text = SBoxModel.Instance.DifficultyName;


        btnAgentID = _comp.GetChild("agentID").asCom.GetChild("value").asButton;
        btnAgentID.onClick.Clear();
        btnAgentID.onClick.Add(OnClickAgentIDMachineID);

        btnMachineID = _comp.GetChild("machineID").asCom.GetChild("value").asButton;
        btnMachineID.onClick.Clear();
        btnMachineID.onClick.Add(OnClickAgentIDMachineID);

        btnGameDifficulty = _comp.GetChild("gameDifficulty").asCom.GetChild("value").asButton;
        btnGameDifficulty.onClick.Clear();
        btnGameDifficulty.onClick.Add(OnClickGameDifficulty);
        RefreshGameDifficultyDisplay();
        RequestRefreshAlgoMeta();

        btnMaxCoinInOutRecord = _comp.GetChild("maxCoinInOutRecord").asCom.GetChild("value").asButton;
        btnMaxCoinInOutRecord.onClick.Clear();
        btnMaxCoinInOutRecord.onClick.Add(OnClickMaxCoinInOutRecord);
        btnMaxCoinInOutRecord.title = SBoxModel.Instance.coinInOutRecordMax.ToString();

        btnMaxGameRecord = _comp.GetChild("maxGameRecord").asCom.GetChild("value").asButton;
        btnMaxGameRecord.onClick.Clear();
        btnMaxGameRecord.onClick.Add(OnClickMaxGameRecord);
        RefreshMaxGameRecordCapacityDisplay();

        btnMaxEventRecord = _comp.GetChild("maxEventRecord").asCom.GetChild("value").asButton;
        btnMaxEventRecord.onClick.Clear();
        btnMaxEventRecord.onClick.Add(OnClickMaxEventRecord);
        btnMaxEventRecord.title = SBoxModel.Instance.eventRecordMax.ToString();

        btnMaxErrorRecord = _comp.GetChild("maxErrorRecord").asCom.GetChild("value").asButton;
        btnMaxErrorRecord.onClick.Clear();
        btnMaxErrorRecord.onClick.Add(OnClickMaxErrorRecord);
        btnMaxErrorRecord.title = SBoxModel.Instance.errorRecordMax.ToString();

        btnMaxBusinessDayRecord = _comp.GetChild("maxBusinessDayRecord").asCom.GetChild("value").asButton;
        btnMaxBusinessDayRecord.onClick.Clear();
        btnMaxBusinessDayRecord.onClick.Add(OnClickMaxBusinessDayRecord);
        btnMaxBusinessDayRecord.title = SBoxModel.Instance.businiessDayRecordMax.ToString();



        btnCoding = _comp.GetChild("active").asCom.GetChild("value").asButton;
        btnCoding.onClick.Clear();
        btnCoding.onClick.Add(OnClickCoder);

        

        btnChangePwdShift = _comp.GetChild("changeShiftPassword").asCom.GetChild("btn").asButton;
        btnChangePwdShift.onClick.Clear();
        btnChangePwdShift.onClick.Add(() =>
        {
            shiftChangePwdCtrl.OnClickSetPassword();
        });


        btnChangePwdManager = _comp.GetChild("changeManagerPassword").asCom.GetChild("btn").asButton;
        btnChangePwdManager.onClick.Clear();
        btnChangePwdManager.onClick.Add(() =>
        {
            managerChangePwdCtrl.OnClickSetPassword();
        });


        btnChangePwdAdmin = _comp.GetChild("changeAdminPassword").asCom.GetChild("btn").asButton;
        btnChangePwdAdmin.onClick.Clear();
        btnChangePwdAdmin.onClick.Add(() =>
        {
            adminChangePwdCtrl.OnClickSetPassword();
        });


        cmpBetAllowed = _comp.GetChild("betAllowed").asCom.GetChild("value").asCom;




        for (int i = SBoxModel.Instance.betAllowList.Count; i < cmpBetAllowed.numChildren; i++)
        {
            GComponent cmpBet = cmpBetAllowed.GetChildAt(i).asCom;
            cmpBet.visible = false;
        }

        List<BetAllow> betAllowList = SBoxModel.Instance.betAllowList;
        for (int i = 0; i < betAllowList.Count; i++)
        {
            GComponent cmpBet = cmpBetAllowed.GetChildAt(i).asCom;
            cmpBet.visible = true;

            GButton toggle = cmpBet.GetChild("toggle").asButton;
            toggle.selected = betAllowList[i].allowed == 1;
            toggle.onChanged.Clear();
            int index = i;
            toggle.onChanged.Add((EventContext context) =>
            {
                OnValueChangeBetAllowed(index, toggle.selected, toggle);
            });

            GTextField title = cmpBet.GetChild("title").asTextField;
            title.text = betAllowList[i].value.ToString();
        }

        if (!_isSubscribed)
        {
            EventCenter.Instance.AddEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, OnPropertyChange);
            _isSubscribed = true;
        }

        //RefreshUIBetLst();

        CheckAgentIDMachineIDActive();
    }

    bool isNeedRefreshUIBetLst = false;

    void RefreshUIBetLst()
    {
        isNeedRefreshUIBetLst = true;
        try
        {
            List<BetAllow> betAllowList = SBoxModel.Instance.betAllowList;
            if (betAllowList == null || cmpBetAllowed == null)
                return;

            // 先根据新列表长度控制可见性
            for (int i = 0; i < cmpBetAllowed.numChildren; i++)
            {
                GComponent cmpBet = cmpBetAllowed.GetChildAt(i).asCom;
                cmpBet.visible = i < betAllowList.Count;
            }

            // 再刷新每一项的 toggle/title
            for (int i = 0; i < betAllowList.Count; i++)
            {
                GComponent cmpBet = cmpBetAllowed.GetChildAt(i).asCom;

                GButton toggle = cmpBet.GetChild("toggle").asButton;
                toggle.selected = betAllowList[i].allowed == 1;

                GTextField title = cmpBet.GetChild("title").asTextField;
                title.text = betAllowList[i].value.ToString();
            }
        }
        finally
        {
            isNeedRefreshUIBetLst = false;
        }
    }

    void OnPropertyChange(EventData res = null)
    {
        if (res == null || string.IsNullOrEmpty(res.name))
            return;

        // betAllowList 切换游戏时会重新加载
        if (res.name == "SBoxModel/betAllowList")
        {
            RefreshUIBetLst();
        }

        if (res.name == "SBoxModel/tableSysSetting")
            RefreshMaxGameRecordCapacityDisplay();
    }

    public void OnClose()
    {
        if (!_isSubscribed)
            return;

        EventCenter.Instance.RemoveEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, OnPropertyChange);
        _isSubscribed = false;
    }
    
    /// <summary>
    /// 赌注允许列表（BetAllow）开关变更回调。
    /// </summary>
    /// <param name="index">当前变更项在列表中的索引。</param>
    /// <param name="isOn">开关是否开启。</param>
    /// <param name="btn">对应的开关按钮。</param>
    void OnValueChangeBetAllowed(int index, bool isOn, GButton btn)
    {
        if (isNeedRefreshUIBetLst)
            return;

        DebugUtils.Log("@ 押注列表发生变化");
        List<BetAllow> betAllowList = SBoxModel.Instance.betAllowList;
        int _index = -1;
        int num = 0;
        for (int i = 0; i < betAllowList.Count; i++)
        {
            if (betAllowList[i].allowed == 1)
            {
                _index = i;
                num++;
            }
        }
        if (index == _index && isOn == false && num == 1)
        {
            CommonPopupHandler.Instance.OpenPopupSingle(new CommonPopupInfo()
            {
                text = I18nMgr.T("The betting list option does not support closing all options."),
                type = CommonPopupType.OK,
                buttonText1 = I18nMgr.T("OK"),
                buttonAutoClose1 = true,
                callback1 = delegate
                {

                },
                isUseXButton = false,
            });

            btn.selected = true;
            //RefreshUIBetLst();
            return;
        }
        betAllowList[index].allowed = isOn ? 1 : 0;
    }

    /// <summary>
    /// 点击修改线路号/机台号（Agent ID / Machine ID）。
    /// 会弹出输入页进行校验；通过权限控制是否允许保存，并在保存后刷新按钮可编辑状态。
    /// </summary>
    async void OnClickAgentIDMachineID()
    {

        // 参数校验：Agent ID 要求为 4 位纯数字
        Func<string, string> checkAgnetIDFunc = (res) =>
        {
            if (string.IsNullOrEmpty(res))
                return string.Format(I18nMgr.T("The {0} cannot be empty"), I18nMgr.T("Agent ID"));

            try
            {
                int num = int.Parse(res);
            }catch(Exception ex)
            {
                return I18nMgr.T("The input value must be a number");
            }

            if (res.Length != 4)
                return string.Format(I18nMgr.T("The {0} must be {1} digits long"), I18nMgr.T("Agent ID"),4);

            return null;
        };

        // 参数校验：Machine ID 要求为 8 位纯数字
        Func<string, string> checkMachineIDFunc = (res) =>
        {
            if (string.IsNullOrEmpty(res))
                return string.Format(I18nMgr.T("The {0} cannot be empty"), I18nMgr.T("Machine ID"));

            try
            {
                int num = int.Parse(res);
            }
            catch (Exception ex)
            {
                return I18nMgr.T("The input value must be a number");
            }

            if (res.Length != 8)
                return string.Format(I18nMgr.T("The {0} must be {1} digits long"), I18nMgr.T("Machine ID"), 8);

            return null;
        };

        EventData res = await PageManager.Instance.OpenPageAsync(PageName.ConsolePopupConsoleSetParameter002,
                new EventData<Dictionary<string, object>>("",
                new Dictionary<string, object>()
                {
                    ["title"] = I18nMgr.T("Set Machine ID"),
                    ["paramName1"] = I18nMgr.T("Agent ID:"),
                    ["paramName2"] = I18nMgr.T("Machine ID:"),
                    ["checkParam1Func"] = checkAgnetIDFunc,
                    ["checkParam2Func"] = checkMachineIDFunc,
                }
            ));

        // 弹窗返回值约定：res.value 为 List<string>，顺序为 [agentId, machineId]
        if (res.value!= null)
        {     
            List<string> lst = (List<string>)res.value;
            string machineId = lst[1];
            string agentId = lst[0];  //machineId.Substring(0, 4);
            if (machineId == SBoxModel.Instance.MachineId)
            {
                TipPopupHandler.Instance.OpenPopup(I18nMgr.T("The settings have not changed and do not need to be saved"));
            }
            else if (!machineId.StartsWith(agentId))
            {
                TipPopupHandler.Instance.OpenPopup(I18nMgr.T("Machine ID must start with Agent ID"));
            }
            else
            {

                UnityAction OnConfirmModify = () =>
                {
                    MachineDataUtils.RequestSetLineIDMachineID(int.Parse(agentId), int.Parse(machineId),
                    (res) =>
                    {
                        SBoxPermissionsData data = res as SBoxPermissionsData;
                        if (data.result == 0)
                            TipPopupHandler.Instance.OpenPopup(I18nMgr.T("Successfully saved"));
                        else
                            TipPopupHandler.Instance.OpenPopup(I18nMgr.T("Save failed"));

                        //要延时？
                        CheckAgentIDMachineIDActive();
                    },
                    (err) =>
                    {
                        TipPopupHandler.Instance.OpenPopup(I18nMgr.T(err.msg));

                        CheckAgentIDMachineIDActive();
                    });
                };
                

                if (SBoxModel.Instance.isCurPermissionsAdmin)
                {
                    OnConfirmModify();
                }
                else
                {
                    CommonPopupHandler.Instance.OpenPopup(new CommonPopupInfo()
                    {
                        // 只能修改一次线号机台号，确定要修改？
                        type = CommonPopupType.YesNo,
                        text = I18nMgr.T("You can only modify the Agent ID and Machine ID once. Are you sure you want to modify it?"),
                        buttonText1 = I18nMgr.T("Cancel"),
                        buttonText2 = I18nMgr.T("OK"),
                        callback1 = null,
                        callback2 = OnConfirmModify,
                    });
                }

            }
        }


    }

    /// <summary>
    /// 点击修改游戏难度。区域只跟随算法，前端不可改；仅可改 level(1~5)。
    /// </summary>
    async void OnClickGameDifficulty()
    {
        var names = SBoxModel.Instance.CurrentDifficultyNames;
        var selectLst = new Dictionary<string, string>();
        for (int i = 0; i < names.Count; i++)
            selectLst.Add((i + 1).ToString(), names[i]); // key = level 1~5

        string cur = SBoxModel.Instance.algoLevel.ToString();

        Func<string, string> getSelectedDes = (number) =>
        {
            if (selectLst.ContainsKey(number))
                return string.Format(I18nMgr.T("Selected: {0}"), selectLst[number]);
            return number;
        };

        EventData res = await PageManager.Instance.OpenPageAsync(
            PageName.ConsolePopupConsoleChoose001,
            new EventData<Dictionary<string, object>>("",
                new Dictionary<string, object>()
                {
                    ["title"] = I18nMgr.T("Game Difficulty"),
                    ["selectLst"] = selectLst,
                    ["selectNumber"] = cur,
                    ["getSelectedDes"] = getSelectedDes,
                }));

        if (res?.value == null)
            return;

        if (!int.TryParse((string)res.value, out int level) || level < 1 || level > 5)
            return;

        if (level == SBoxModel.Instance.algoLevel)
            return;

        // 区域固定用当前 algoRegion，仅提交 level
        MachineDataManager02.Instance.RequestSetAlgoLevel(level,
            (retObj) =>
            {
                int ret = 0;
                if (retObj is int)
                    ret = (int)retObj;
                if (ret == 1)
                {
                    SBoxModel.Instance.algoLevel = level;
                    RefreshGameDifficultyDisplay();
                    TipPopupHandler.Instance.OpenPopup(I18nMgr.T("Successfully saved"));
                }
                else
                {
                    TipPopupHandler.Instance.OpenPopup(I18nMgr.T("Save failed"));
                }
            },
            (err) =>
            {
                TipPopupHandler.Instance.OpenPopup(I18nMgr.T(err?.msg ?? "Save failed"));
            });
    }

    void RefreshGameDifficultyDisplay()
    {
        if (btnGameDifficulty != null)
            btnGameDifficulty.title = SBoxModel.Instance.DifficultyName;
    }

    /// <summary>从算法刷新区域/难度展示；区域只读跟随算法</summary>
    void RequestRefreshAlgoMeta()
    {
        MachineDataManager02.Instance.RequestGetAlgoMetaInfo((res) =>
        {
            SBoxModel.Instance.ApplyAlgoMetaInfo(res as AlgoMetaInfo);
            RefreshGameDifficultyDisplay();
        }, (err) =>
        {
            DebugUtils.LogError($"{SBoxEventHandle.SBOX_ALGO_META_INFO} : {err?.msg}");
            RefreshGameDifficultyDisplay();
        });
    }

    /// <summary>
    /// 刷新线路号/机台号按钮的显示内容与可编辑状态（基于当前权限）。
    /// </summary>
    void CheckAgentIDMachineIDActive()
    {

        Action callback = () =>
        {
            btnAgentID.text = SBoxModel.Instance.LineId;
            btnMachineID.text = SBoxModel.Instance.MachineId;

            if (SBoxModel.Instance.isCurPermissionsAdmin)
            {
                btnAgentID.touchable = true;
                btnMachineID.touchable = true;

                btnAgentID.GetChild("untouchable").visible = !btnAgentID.touchable;
                btnMachineID.GetChild("untouchable").visible = !btnMachineID.touchable;
            }
            else
            {
                btnAgentID.touchable = false;// SBoxIdea.IsMachineIdReady() ? false : true;
                btnMachineID.touchable = false;// SBoxIdea.IsMachineIdReady() ? false : true;

                btnAgentID.GetChild("untouchable").visible = !btnAgentID.touchable;
                btnMachineID.GetChild("untouchable").visible = !btnMachineID.touchable;
            }
        };

        callback();

        Timer.DelayAction(0.5f, callback);
        // DoCo(COR_DELAY_CHECK_ID, DoTask(callback, 500));
    }

    /// <summary>
    /// 设置“出入账记录上限”（Max Coin In Out Record）。
    /// </summary>
    async void OnClickMaxCoinInOutRecord()
    {
        EventData res = await PageManager.Instance.OpenPageAsync(PageName.ConsolePopupConsoleKeyboard002,
        new EventData<Dictionary<string, object>>("",
            new Dictionary<string, object>()
            {
                ["title"] = I18nMgr.T("Max Coin In Out Record"),
                ["isPlaintext"] = true,
            }));

        if (res.value != null)
        {
            bool isErr = true;

            int minMaxCoinInOutRecord = DefaultSettingsUtils.minMaxCoinInOutRecord;
            int maxMaxCoinInOutRecord = DefaultSettingsUtils.maxMaxCoinInOutRecord;
            try
            {
                int val = int.Parse((string)res.value);  // (long)res.value;

                if (val >= minMaxCoinInOutRecord
                    && val <= maxMaxCoinInOutRecord
                    )
                {
                    isErr = false;
                    SBoxModel.Instance.coinInOutRecordMax = val;
                    btnMaxCoinInOutRecord.title = val.ToString();
                }

            }
            catch { }

            if (isErr)
                TipPopupHandler.Instance.OpenPopup(string.Format(I18nMgr.T("The {0} must be between {1} and {2}"),
                    I18nMgr.T("Max Coin In Out Record"),
                    minMaxCoinInOutRecord, maxMaxCoinInOutRecord));
        }
    }

    /// <summary>
    /// 游戏记录扩容：选择手动输入 / 扩至默认 / 翻倍 / 立即整理。
    /// </summary>
    void OnClickMaxGameRecord()
    {
        SlotGameRecordCapacityUtils.QuerySlotGameRecordCount((count) =>
        {
            OpenMaxGameRecordExpandMenu(count);
        });
    }

    void RefreshMaxGameRecordCapacityDisplay()
    {
        if (btnMaxGameRecord == null)
            return;

        long max = SBoxModel.Instance.gameRecordMax;
        SlotGameRecordCapacityUtils.QuerySlotGameRecordCount((count) =>
        {
            if (btnMaxGameRecord == null)
                return;

            if (count >= 0)
                btnMaxGameRecord.title = $"{count}/{max}";
            else
                btnMaxGameRecord.title = max.ToString(CultureInfo.InvariantCulture);
        });
    }

    async void OpenMaxGameRecordExpandMenu(int currentCount)
    {
        long curMax = SBoxModel.Instance.gameRecordMax;
        int expandDefault = SlotGameRecordCapacityUtils.CalcExpandToDefault();
        int expandDouble = SlotGameRecordCapacityUtils.CalcExpandDouble();

        string countTip = currentCount >= 0
            ? string.Format(CultureInfo.InvariantCulture, "当前 {0} 条 / 上限 {1}", currentCount, curMax)
            : string.Format(CultureInfo.InvariantCulture, "当前上限 {0}", curMax);

        var selectLst = new Dictionary<string, string>
        {
            [SlotGameRecordCapacityUtils.ExpandMenuManual] = "手动输入上限",
            [SlotGameRecordCapacityUtils.ExpandMenuToDefault] =
                string.Format(CultureInfo.InvariantCulture, "扩容至默认 ({0})", expandDefault),
            [SlotGameRecordCapacityUtils.ExpandMenuDouble] =
                string.Format(CultureInfo.InvariantCulture, "扩容为 2 倍 ({0})", expandDouble),
            [SlotGameRecordCapacityUtils.ExpandMenuTrimNow] = "立即整理溢出记录",
        };

        Func<string, string> getSelectDes = (key) =>
        {
            switch (key)
            {
                case SlotGameRecordCapacityUtils.ExpandMenuManual:
                    return countTip + "\n输入 " + DefaultSettingsUtils.minMaxGameRecord + " ~ " +
                           DefaultSettingsUtils.maxMaxGameRecord + " 之间的整数";
                case SlotGameRecordCapacityUtils.ExpandMenuToDefault:
                    return countTip + "\n将上限设为 " + expandDefault + "，并删除超出部分";
                case SlotGameRecordCapacityUtils.ExpandMenuDouble:
                    return countTip + "\n将上限由 " + curMax + " 调整为 " + expandDouble + "，并删除超出部分";
                case SlotGameRecordCapacityUtils.ExpandMenuTrimNow:
                    return countTip + "\n不修改上限，仅按当前上限 " + curMax + " 删除最旧记录";
                default:
                    return countTip;
            }
        };

        EventData res = await PageManager.Instance.OpenPageAsync(PageName.ConsolePopupConsoleChoose001,
            new EventData<Dictionary<string, object>>("",
                new Dictionary<string, object>()
                {
                    ["title"] = I18nMgr.T("Max Game Record") + " · 扩容",
                    ["selectLst"] = selectLst,
                    ["selectNumber"] = SlotGameRecordCapacityUtils.ExpandMenuToDefault,
                    ["getSelectedDes"] = getSelectDes,
                }));

        if (res?.value == null)
            return;

        string choice = res.value as string;
        switch (choice)
        {
            case SlotGameRecordCapacityUtils.ExpandMenuManual:
                await OnClickMaxGameRecordManualInputAsync();
                break;
            case SlotGameRecordCapacityUtils.ExpandMenuToDefault:
                ApplyMaxGameRecordExpand(expandDefault, trimOverflow: true);
                break;
            case SlotGameRecordCapacityUtils.ExpandMenuDouble:
                if (expandDouble <= curMax)
                {
                    TipPopupHandler.Instance.OpenPopup(
                        string.Format(CultureInfo.InvariantCulture,
                            "当前上限已为 {0}，已达可翻倍上限（最大 {1}）", curMax, DefaultSettingsUtils.maxMaxGameRecord));
                    return;
                }

                ApplyMaxGameRecordExpand(expandDouble, trimOverflow: true);
                break;
            case SlotGameRecordCapacityUtils.ExpandMenuTrimNow:
                SlotGameRecordCapacityUtils.TrimSlotGameRecordOverflowNow(
                    (ok, err) => OnExpandCapacityFinished(ok, err, false));
                break;
        }
    }

    void ApplyMaxGameRecordExpand(int newMax, bool trimOverflow)
    {
        SlotGameRecordCapacityUtils.ApplyMaxGameRecord(newMax, trimOverflow,
            (ok, err) => OnExpandCapacityFinished(ok, err, true));
    }

    void OnExpandCapacityFinished(bool ok, string errMsg, bool capacityChanged)
    {
        if (!ok)
        {
            TipPopupHandler.Instance.OpenPopup(string.IsNullOrEmpty(errMsg)
                ? "游戏记录扩容失败"
                : "游戏记录扩容失败：" + errMsg);
            return;
        }

        RefreshMaxGameRecordCapacityDisplay();
        string tip = capacityChanged
            ? string.Format(CultureInfo.InvariantCulture,
                "游戏记录上限已设为 {0}", SBoxModel.Instance.gameRecordMax)
            : "已按当前上限整理游戏记录";
        TipPopupHandler.Instance.OpenPopup(tip);
    }

    /// <summary>
    /// 手动设置“游戏记录上限”（Max Game Record）。
    /// </summary>
    async System.Threading.Tasks.Task OnClickMaxGameRecordManualInputAsync()
    {
        EventData res = await PageManager.Instance.OpenPageAsync(PageName.ConsolePopupConsoleKeyboard002,
            new EventData<Dictionary<string, object>>("",
                new Dictionary<string, object>()
                {
                    ["title"] = I18nMgr.T("Max Game Record"),
                    ["isPlaintext"] = true,
                }));

        if (res.value == null)
            return;

        bool isErr = true;
        int minMaxGameRecord = DefaultSettingsUtils.minMaxGameRecord;
        int maxMaxGameRecord = DefaultSettingsUtils.maxMaxGameRecord;
        try
        {
            int val = int.Parse((string)res.value, CultureInfo.InvariantCulture);

            if (val >= minMaxGameRecord && val <= maxMaxGameRecord)
            {
                isErr = false;
                bool trim = val < SBoxModel.Instance.gameRecordMax;
                ApplyMaxGameRecordExpand(val, trimOverflow: trim);
            }
        }
        catch { }

        if (isErr)
            TipPopupHandler.Instance.OpenPopup(string.Format(I18nMgr.T("The {0} must be between {1} and {2}"),
                I18nMgr.T("Max Game Record"),
                minMaxGameRecord, maxMaxGameRecord));
    }

    /// <summary>
    /// 设置“事件记录上限”（Max Event Record）。
    /// </summary>
    async void OnClickMaxEventRecord()
    {
        EventData res = await PageManager.Instance.OpenPageAsync(PageName.ConsolePopupConsoleKeyboard002,
            new EventData<Dictionary<string, object>>("",
                new Dictionary<string, object>()
                {
                    ["title"] = I18nMgr.T("Max Event Record"),
                    ["isPlaintext"] = true,
                }));

        if (res.value != null)
        {
            bool isErr = true;

            int minMaxRecord = DefaultSettingsUtils.minMaxEventRecord;
            int maxMaxRecord = DefaultSettingsUtils.maxMaxEventRecord;
            try
            {
                int val = int.Parse((string)res.value);  // (long)res.value;

                if (val >= minMaxRecord
                    && val <= maxMaxRecord
                    )
                {
                    isErr = false;
                    SBoxModel.Instance.eventRecordMax = val;
                    btnMaxEventRecord.title = val.ToString();
                }
            }
            catch { }

            if (isErr)
                TipPopupHandler.Instance.OpenPopup(string.Format(I18nMgr.T("The {0} must be between {1} and {2}"),
                    I18nMgr.T("Max Event Record"),
                    minMaxRecord, maxMaxRecord));
        }
    }






    /// <summary>
    /// 设置“警告/错误记录上限”（Max Warning Record / Max Error Record）。
    /// </summary>
    async void OnClickMaxErrorRecord()
    {
        EventData res = await PageManager.Instance.OpenPageAsync(PageName.ConsolePopupConsoleKeyboard002,
            new EventData<Dictionary<string, object>>("",
                new Dictionary<string, object>()
                {
                    ["title"] = I18nMgr.T("Max Warning Record"),
                    ["isPlaintext"] = true,
                }));

        if (res.value != null)
        {
            bool isErr = true;

            int minMaxRecord = DefaultSettingsUtils.minMaxErrorRecord;
            int maxMaxRecord = DefaultSettingsUtils.maxMaxErrorRecord;
            try
            {
                int val = int.Parse((string)res.value);  // (long)res.value;

                if (val >= minMaxRecord
                    && val <= maxMaxRecord
                    )
                {
                    isErr = false;
                    SBoxModel.Instance.errorRecordMax = val;
                    btnMaxErrorRecord.title = val.ToString();
                }
            }
            catch { }

            if (isErr)
                TipPopupHandler.Instance.OpenPopup(string.Format(I18nMgr.T("The {0} must be between {1} and {2}"),
                    I18nMgr.T("Max Warning Record"),
                    minMaxRecord, maxMaxRecord));
        }
    }


    /// <summary>
    /// 设置“营业日记录上限”（Max Business Day Record）。
    /// </summary>
    async void OnClickMaxBusinessDayRecord()
    {
        EventData res = await PageManager.Instance.OpenPageAsync(PageName.ConsolePopupConsoleKeyboard002,
            new EventData<Dictionary<string, object>>("",
                new Dictionary<string, object>()
                {
                    ["title"] = I18nMgr.T("Max Business Day Record"),
                    ["isPlaintext"] = true,
                }));

        if (res.value != null)
        {
            bool isErr = true;

            int minMaxRecord = DefaultSettingsUtils.minMaxBusinessDayRecord;
            int maxMaxRecord = DefaultSettingsUtils.maxMaxBusinessDayRecord;
            try
            {
                int val = int.Parse((string)res.value);  // (long)res.value;

                if (val >= minMaxRecord
                    && val <= maxMaxRecord
                    )
                {
                    isErr = false;
                    SBoxModel.Instance.businiessDayRecordMax = val;
                    btnMaxBusinessDayRecord.title = val.ToString();
                }
            }
            catch { }

            if (isErr)
                TipPopupHandler.Instance.OpenPopup(string.Format(I18nMgr.T("The {0} must be between {1} and {2}"),
                    I18nMgr.T("Max Business Day Record"),
                    minMaxRecord, maxMaxRecord));
        }
    }


    /// <summary>
    /// 点击“编码/开发”按钮，触发事件以显示编码弹窗。
    /// </summary>
    void OnClickCoder()
    {
        EventCenter.Instance.EventTrigger<EventData>(MachineUIEvent.ON_MACHINE_UI_EVENT,
            new EventData<PageName>(MachineUIEvent.ShowCoding, PageName.ConsolePopupConsoleCoder));
    }



}
