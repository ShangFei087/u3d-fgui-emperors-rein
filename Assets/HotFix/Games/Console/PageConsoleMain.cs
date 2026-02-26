using FairyGUI;
using GameMaker;
using System.Collections.Generic;
using SBoxApi;
using System;



namespace ConsoleSlot01
{
    public class PageConsoleMain : PageBase
    {
        public const string pkgName = "Console";
        public const string resName = "PageConsoleMain";
        public override PageType pageType => PageType.Overlay;


        protected override void OnInit()
        {
            
            //this.contentPane = UIPackage.CreateObject("Console", "PageConsoleMain").asCom;
            base.OnInit();


            int count = 1;

            Action callback = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam();
                }
            };
            callback();
        }


        public override void OnTop()
        {
            DebugUtils.Log($"i am top ConsoleMainPage {this.name}");
        }

        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);

            CommonPopupHandler.Instance.ClosePopup();

            InitParam();
            OnChenkUser();
        }


        GList glstMenu;

        GButton btnGameInfo, btnBusinessRecord, btnGameHistory, btnLogRecord,
                btnSettings, btnVolumeSetting, btnHardwareTest, btnTouchCallbrate,
                btnTimeAndDate, btnLanguage, btnAdmin, btnExit;

        GObject goMaskDontClick;

        int permissions = -1; //1：普通密码权限，2：管理员密码权限，3：超级管理员密码权限


        //GButton btnSettingsCache, btnAdminCache;





        

        public override void InitParam()
        {

            if (!isInit) return;

            if (!isOpen) return;

            goMaskDontClick = this.contentPane.GetChild("mask");

            glstMenu = this.contentPane.GetChild("menu").asList;



            GButton _btnSettings = glstMenu.GetChild("settings")?.asButton ?? null;
            if (_btnSettings != null)
            {
                glstMenu.RemoveChild(_btnSettings);

                if (btnSettings != null && _btnSettings != btnSettings)
                {
                    btnSettings.Dispose();
                }
                btnSettings = _btnSettings;
                btnSettings.onClick.Clear();
                btnSettings.onClick.Add(OnClickSettings);
            }


            GButton _btnAdmin = glstMenu.GetChild("admin")?.asButton ?? null;
            if (_btnAdmin != null)
            {
                glstMenu.RemoveChild(_btnAdmin);

                if (btnAdmin != null && _btnAdmin != btnAdmin)
                {
                    btnAdmin.Dispose();
                }
                btnAdmin = _btnAdmin;
                btnAdmin.onClick.Set(OnClickAdmin);
            }



            btnGameInfo = glstMenu.GetChild("gameInfo").asButton;
            btnGameInfo.onClick.Clear();
            btnGameInfo.onClick.Add(OnClickGameInfo);

            btnBusinessRecord = glstMenu.GetChild("businessRecord").asButton;
            btnBusinessRecord.onClick.Clear();
            btnBusinessRecord.onClick.Add(OnClickBusinessRecord);

            btnGameHistory = glstMenu.GetChild("gameHistory").asButton;
            btnGameHistory.onClick.Clear();
            btnGameHistory.onClick.Add(OnClickGameHistory);


            btnLogRecord = glstMenu.GetChild("logRecord").asButton;
            btnLogRecord.onClick.Clear();
            btnLogRecord.onClick.Add(OnClickLogRecord);

            btnTimeAndDate = glstMenu.GetChild("date").asButton;
            btnTimeAndDate.onClick.Clear();
            btnTimeAndDate.onClick.Add(OnClickTimeDate);

            btnVolumeSetting = glstMenu.GetChild("sound").asButton;
            btnVolumeSetting.onClick.Clear();
            btnVolumeSetting.onClick.Add(OnClickSound);


            btnLanguage = glstMenu.GetChild("language").asButton;
            btnLanguage.onClick.Clear();
            btnLanguage.onClick.Add(OnClickLanguage);
            SetLanguageIcon();



            btnHardwareTest = glstMenu.GetChild("hardware").asButton;
            btnHardwareTest.onClick.Clear();
            btnHardwareTest.onClick.Add(OnClickHardwareTest);



            btnTouchCallbrate = glstMenu.GetChild("touch").asButton;
            btnTouchCallbrate.onClick.Clear();
            btnTouchCallbrate.onClick.Add(OnClickTouchCallbrate);







            btnExit = glstMenu.GetChild("exit").asButton;
            btnExit.onClick.Clear();
            btnExit.onClick.Add(OnClickExit);
        }


        async void OnChenkUser()
        {
            goMaskDontClick.visible = true;

            EventData res = await PageManager.Instance.OpenPageAsync(PageName.ConsolePopupConsoleKeyboard001,
                new EventData<Dictionary<string, object>>("",
                    new Dictionary<string, object>()
                    {
                        ["title"] = I18nMgr.T("Enter Password"),
                        ["isPlaintext"] = false,
                    }));

            permissions = -1;

            if (res != null && res.value != null)
            {
                string pwdStr = (string)res.value;
                DebugUtils.Log($"键盘输入结果 ：{pwdStr}");

                try
                {
                    int pwd = int.Parse(pwdStr); //这里有可能失败

                    MachineDataManager02.Instance.RequestCheckPassword(pwd,
                    (res) =>
                    {

                        SBoxPermissionsData data = res as SBoxPermissionsData;
                        if (data.result == 0 && data.permissions > 0)
                        {
                            goMaskDontClick.visible = false;

                            permissions = data.permissions;//1：普通密码权限，2：管理员密码权限，3：超级管理员密码权限

                            //btnSettings.visible = permissions >= 2;
                            //btnAdmin.visible =  permissions == 3;

                            /*
                            if (permissions >= 2)
                            {
                                glstMenu.AddChildAt(btnSettings, glstMenu.numChildren - 1);
                            }
                            if (permissions == 3)
                            {
                                glstMenu.AddChildAt(btnAdmin, glstMenu.numChildren - 1);
                            }*/

                            //glstMenu.RefreshVirtualList();  这有问题



                            SBoxModel.Instance.curPermissions = permissions;

                            CheckPermissions();

                            if (SBoxModel.Instance.isCurPermissionsAdmin)
                                SBoxModel.Instance.passwordAdmin = pwd;


                            /*
                                            case UserType.Admin:
                                SBoxModel.Instance.passwordAdmin = pwd;
                                return;
                            case UserType.Manager:
                                SBoxModel.Instance.passwordManager = pwd;
                                return;
                            case UserType.Shift:
                                SBoxModel.Instance.passwordShift = pwd;

                            */


                            DebugUtils.Log($"当前用户权限{SBoxModel.Instance.curPermissions}; 密码: {pwd}");
                        }
                        else
                        {
                            OnCheckUserError();
                        }

                    }, (error) =>
                    {
                        OnCheckUserError();
                    });
                }
                catch
                {
                    OnCheckUserError();
                }
            }
            else
            {
                OnClickExit();
            }
        }



        void SetLanguageIcon()
        {
            string url = "ui://Console/icon_lang_cn";
            switch (SBoxModel.Instance.language)
            {
                case "en":
                    url = "ui://Console/icon_lang_en";
                    break;
                case "cn":
                case "tw":
                    url = "ui://Console/icon_lang_cn";
                    break;
            }
            btnLanguage.GetChild("icon2").asLoader.url = url;
        }

        void CheckPermissions()
        {
            if (SBoxModel.Instance.curPermissions >= 2)
            {
                glstMenu.AddChildAt(btnSettings, glstMenu.numChildren - 1);
            }
            if (SBoxModel.Instance.curPermissions == 3)
            {
                glstMenu.AddChildAt(btnAdmin, glstMenu.numChildren - 1);
            }
        }

        void OnCheckUserError()
        {
            OnChenkUser();
            CommonPopupHandler.Instance.OpenPopupSingle(
            new CommonPopupInfo()
            {
                isUseXButton = false,
                buttonAutoClose1 = true,
                buttonAutoClose2 = true,
                type = CommonPopupType.YesNo,
                text = I18nMgr.T("Error Password"),
                buttonText1 = I18nMgr.T("Cancel"),
                callback1 = () =>
                {
                    //DebugUtils.LogError("i am callback1");
                },
                buttonText2 = I18nMgr.T("Confirm"),
                callback2 = () =>
                {
                    //DebugUtils.LogError("i am callback2");
                }
            });
        }

        void OnClickGameInfo() => PageManager.Instance.OpenPage(PageName.ConsolePageConsoleGameInformation);

        void OnClickBusinessRecord() => PageManager.Instance.OpenPage(PageName.ConsolePageConsoleBusinessRecord);

        void OnClickSettings() => PageManager.Instance.OpenPage(PageName.ConsolePageConsoleMachineSettings);
        //void OnClickSettings() => PageManager.Instance.OpenPage(PageName.ConsolePageConsoleSettingsMenu); 

        void OnClickHardwareTest(){}


        void OnClickAdmin() { }
        void OnClickTouchCallbrate() => PageManager.Instance.OpenPage(PageName.ConsolePageDrawLine);

        void OnClickExit()
        {
            SBoxModel.Instance.curPermissions = -1;
            PageManager.Instance.ClosePage(this);
        }

        async void OnClickTimeDate()
        {
            EventData res = await PageManager.Instance.OpenPageAsync(PageName.ConsolePopupConsoleCalendar);

            if (res != null && res.value != null)
            {
                try
                {
                    Dictionary<string, object> data = res.value as Dictionary<string, object>;
                    long timestamp = (long)data["timestamp"];
                    string date = (string)data["date"];
                    DebugUtils.LogError($"获得时间戳： {timestamp}  对应日期：{date}");
                }
                catch (Exception ex)
                {
                }
            }
        }


        void OnClickSound()=> PageManager.Instance.OpenPage(PageName.ConsolePopupConsoleSound);

        void OnClickLogRecord() => PageManager.Instance.OpenPage(PageName.ConsolePageConsoleLogRecord);

        void OnClickGameHistory() { }


        async void OnClickLanguage()
        {

            Dictionary<string, string> selectLst = new Dictionary<string, string>();
            foreach(TableSupportLanguageItem item in SBoxModel.Instance.supportLanguage)
            {
                selectLst.Add(item.number, item.name);
            }

            Func<string, string> getSelectedDes = (number) =>
                    {
                        if (selectLst.ContainsKey(number))
                            return string.Format(I18nMgr.T("Selected language: {0}"), I18nMgr.T(selectLst[number]));  
                        return number;
                    };

            EventData res = await PageManager.Instance.OpenPageAsync(PageName.ConsolePopupConsoleChoose001,
                new EventData<Dictionary<string,object>>("",
                new Dictionary<string, object>()
                {
                    ["title"] = I18nMgr.T("Choose Language"),
                    ["selectLst"] = selectLst,
                    ["selectNumber"] = SBoxModel.Instance.language,
                    ["getSelectedDes"] = getSelectedDes,
                }));

            if (res != null && res.value != null)
            {
                try
                {
                    string selectNumber = (string)res.value;

                    if (SBoxModel.Instance.language == selectNumber)
                        return;

                    //关闭所有弹窗
                    CommonPopupHandler.Instance.CloseAllPopup();

                    SBoxModel.Instance.language = selectNumber; 
                    MachineDeviceCommonBiz.Instance.CheckLanguage();


                    MaskPopupHandler.Instance.OpenPopup();
                    Timers.inst.Add(2, 1, (data) =>
                    {

                        CheckPermissions();
                        SetLanguageIcon();
                        goMaskDontClick.visible = false;

                        MaskPopupHandler.Instance.ClosePopup();
                    });

                }
                catch (Exception ex)
                {
                }
            }
            
        }

    }
}