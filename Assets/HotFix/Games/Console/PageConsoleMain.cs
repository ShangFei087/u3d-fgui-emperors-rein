using FairyGUI;
using GameMaker;
using System.Collections.Generic;
using SBoxApi;
using System;



namespace ConsoleSlot01
{
    /// <summary>
    /// 后台控制台主菜单页。
    /// 负责密码校验、按权限显示菜单项，并跳转到各功能子页面。
    /// </summary>
    public class PageConsoleMain : PageBase
    {
        public const string pkgName = "Console";
        public const string resName = "PageConsoleMain";
        public override PageType pageType => PageType.Overlay;

        /// <summary>
        /// 页面初始化：等待资源就绪后执行 InitParam。
        /// </summary>
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

        /// <summary>
        /// 页面置顶时回调。
        /// </summary>
        public override void OnTop()
        {
            DebugUtils.Log($"i am top ConsoleMainPage {this.name}");
        }

        /// <summary>
        /// 打开页面：关闭通用弹窗、初始化控件，并弹出密码校验。
        /// </summary>
        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            CommonPopupHandler.Instance.ClosePopup();

            InitParam();
            OnChenkUser();
        }

        /// <summary>
        /// 语言切换时重建 UI，并重新绑定控件。
        /// </summary>
        protected override void OnLanguageChange(I18nLang lang)
        {
            FguiI18nTextAssistant.Instance.DisposeAllTranslate(this.contentPane);
            this.contentPane.Dispose(); // 释放当前UI
            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            InitParam();
            //FguiI18nTextAssistant.Instance.TranslateComponent(this.contentPane);
        }

        /// <summary>左侧菜单列表</summary>
        GList glstMenu;

        /// <summary>各功能入口按钮</summary>
        GButton btnGameInfo, btnBusinessRecord, btnGameHistory, btnLogRecord,
                btnSettings, btnVolumeSetting, btnHardwareTest, btnTouchCallbrate,
                btnTimeAndDate, btnLanguage, btnAdmin, btnExit;

        /// <summary>密码校验期间的遮罩，防止误点菜单</summary>
        GObject goMaskDontClick;

        /// <summary>当前权限：-1 未校验；1 普通；2 管理员；3 超级管理员</summary>
        int permissions = -1;
        /// <summary>
        /// 绑定菜单按钮事件。
        /// Settings / Admin 先从列表移除，待权限通过后再按需加回。
        /// </summary>
        public override void InitParam()
        {

            if (!isInit) return;

            if (!isOpen) return;

            goMaskDontClick = this.contentPane.GetChild("mask");

            glstMenu = this.contentPane.GetChild("menu").asList;



            // Settings：从菜单取出缓存，权限校验通过后再显示
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


            // Admin：从菜单取出缓存，仅超级管理员可见
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

            NetMessageController.Instance.ResetConsoleJackpotDataRequestSession();
        }

        /// <summary>
        /// 弹出密码键盘并校验权限。
        /// 校验成功后按权限显示 Settings / Admin；取消则退出控制台。
        /// </summary>
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
                // 取消输入密码，直接退出控制台
                OnClickExit();
            }
        }

        /// <summary>
        /// 根据当前语言设置语言按钮图标。
        /// </summary>
        void SetLanguageIcon()
        {
            string url = "ui://Console/icon_lang_cn";
            switch (SBoxModel.Instance.language)
            {
                case "en":
                    url = "ui://Console/icon_lang_en";
                    break;
                case "cn":
                    url = "ui://Console/icon_lang_cn";
                    break;
                case "tw":
                    url = "ui://Console/icon_lang_cn";
                    break;
            }
            btnLanguage.GetChild("icon2").asLoader.url = url;
        }

        /// <summary>
        /// 按权限把 Settings / Admin 加回菜单（插在 Exit 之前）。
        /// permissions &gt;= 2 显示 Settings；== 3 显示 Admin。
        /// </summary>
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

        /// <summary>
        /// 密码错误：提示后重新弹出密码键盘。
        /// </summary>
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

        /// <summary>打开游戏信息页</summary>
        void OnClickGameInfo() => PageManager.Instance.OpenPage(PageName.ConsolePageConsoleGameInformation);

        /// <summary>打开营业记录页</summary>
        void OnClickBusinessRecord() => PageManager.Instance.OpenPage(PageName.ConsolePageConsoleBusinessRecord);

        /// <summary>打开机器设置页</summary>
        void OnClickSettings() => PageManager.Instance.OpenPage(PageName.ConsolePageConsoleMachineSettings);

        /// <summary>打开硬件测试页</summary>
        void OnClickHardwareTest() => PageManager.Instance.OpenPage(PageName.ConsolePopupConsoleAdmin);

        /// <summary>管理员入口</summary>
        void OnClickAdmin() => PageManager.Instance.OpenPage(PageName.ConsolePageConsoleHardware);

        /// <summary>打开触屏校准页</summary>
        void OnClickTouchCallbrate() => PageManager.Instance.OpenPage(PageName.ConsolePageDrawLine);

        /// <summary>
        /// 退出控制台：清空权限并关闭本页。
        /// </summary>
        void OnClickExit()
        {
            SBoxModel.Instance.curPermissions = -1;
            MachineDeviceController.Instance.ExitConsoleMode();
            PageManager.Instance.ClosePage(this);
        }

        /// <summary>
        /// 打开日历弹窗设置日期时间。
        /// </summary>
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

        /// <summary>打开音量设置弹窗</summary>
        void OnClickSound()=> PageManager.Instance.OpenPage(PageName.ConsolePopupConsoleSound);

        /// <summary>打开日志记录页</summary>
        void OnClickLogRecord() => PageManager.Instance.OpenPage(PageName.ConsolePageConsoleLogRecord);

        /// <summary>打开游戏历史页</summary>
        void OnClickGameHistory() => PageManager.Instance.OpenPage(PageName.ConsolePageConsoleGameHistory);

        /// <summary>
        /// 打开语言选择弹窗；切换语言后刷新权限菜单与语言图标。
        /// </summary>
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
                    //CommonPopupHandler.Instance.CloseAllPopup();
                    
                     CommonPopupHandler.Instance.ClosePopup();

                    SBoxModel.Instance.language = selectNumber; 
                    MachineDeviceCommonBiz.Instance.CheckLanguage();

                    // 等待语言资源切换完成后再刷新 UI
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
