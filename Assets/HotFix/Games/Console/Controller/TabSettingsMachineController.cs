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

public class TabSettingsMachineController
{
    GComponent _comp;



    GButton btnAgentID, btnMachineID,
        btnChangePwdShift, btnChangePwdManager, btnChangePwdAdmin,
        btnMaxCoinInOutRecord, btnMaxGameRecord, btnMaxEventRecord, btnMaxErrorRecord, btnMaxBusinessDayRecord,
        btnCoding;


    GComponent cmpBetAllowed;

    GRichTextField rtxtDifficulty;

    /// 修改用户密码
    ChangePasswordController adminChangePwdCtrl = new ChangePasswordController(UserType.Admin);
    ChangePasswordController managerChangePwdCtrl = new ChangePasswordController(UserType.Manager);
    ChangePasswordController shiftChangePwdCtrl = new ChangePasswordController(UserType.Shift);

    public  void InitParam(GComponent comp)
    {
        _comp = comp;


        rtxtDifficulty = _comp.GetChild("difficulty").asCom.GetChild("value").asRichTextField;
        rtxtDifficulty.text = SBoxModel.Instance.DifficultyName;


        btnAgentID = _comp.GetChild("agentID").asCom.GetChild("value").asButton;
        btnAgentID.onClick.Clear();
        btnAgentID.onClick.Add(OnClickAgentIDMachineID);

        btnMachineID = _comp.GetChild("machineID").asCom.GetChild("value").asButton;
        btnMachineID.onClick.Clear();
        btnMachineID.onClick.Add(OnClickAgentIDMachineID);

        btnMaxCoinInOutRecord = _comp.GetChild("maxCoinInOutRecord").asCom.GetChild("value").asButton;
        btnMaxCoinInOutRecord.onClick.Clear();
        btnMaxCoinInOutRecord.onClick.Add(OnClickMaxCoinInOutRecord);
        btnMaxCoinInOutRecord.title = SBoxModel.Instance.coinInOutRecordMax.ToString();

        btnMaxGameRecord = _comp.GetChild("maxGameRecord").asCom.GetChild("value").asButton;
        btnMaxGameRecord.onClick.Clear();
        btnMaxGameRecord.onClick.Add(OnClickMaxGameRecord);
        btnMaxGameRecord.title = SBoxModel.Instance.gameRecordMax.ToString();

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


        //RefreshUIBetLst();

        CheckAgentIDMachineIDActive();
    }

    bool isNeedRefreshUIBetLst = false;
    /*【这个暂时不用】
    void RefreshUIBetLst()
    {
        isNeedRefreshUIBetLst = true;

        List<BetAllow> betAllowList = SBoxModel.Instance.betAllowList;

        for (int i = 0; i < betAllowList.Count; i++)
        {
            GComponent cmpBet = cmpBetAllowed.GetChildAt(i).asCom;
            GButton toggle = cmpBet.GetChild("toggle").asButton;
            toggle.selected = betAllowList[i].allowed == 1;
        }
        isNeedRefreshUIBetLst = false;
    }*/
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
    async void OnClickAgentIDMachineID()
    {

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



    async void OnClickMaxGameRecord()
    {
        EventData res = await PageManager.Instance.OpenPageAsync(PageName.ConsolePopupConsoleKeyboard002,
            new EventData<Dictionary<string, object>>("",
                new Dictionary<string, object>()
                {
                    ["title"] = I18nMgr.T("Max Game Record"),
                    ["isPlaintext"] = true,
                }));

        if (res.value != null)
        {
            bool isErr = true;

            int minMaxGameRecord = DefaultSettingsUtils.minMaxGameRecord;
            int maxMaxGameRecord = DefaultSettingsUtils.maxMaxGameRecord;
            try
            {
                int val = int.Parse((string)res.value);  // (long)res.value;

                if (val >= minMaxGameRecord
                    && val <= maxMaxGameRecord
                    )
                {
                    isErr = false;
                    SBoxModel.Instance.gameRecordMax = val;
                    btnMaxGameRecord.title = val.ToString();
                }
            }
            catch { }

            if (isErr)
                TipPopupHandler.Instance.OpenPopup(string.Format(I18nMgr.T("The {0} must be between {1} and {2}"),
                    I18nMgr.T("Max Game Record"),
                    minMaxGameRecord, maxMaxGameRecord));
        }
    }



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


    void OnClickCoder()
    {
        EventCenter.Instance.EventTrigger<EventData>(MachineUIEvent.ON_MACHINE_UI_EVENT,
            new EventData<PageName>(MachineUIEvent.ShowCoding, PageName.ConsolePopupConsoleCoder));
    }



}
