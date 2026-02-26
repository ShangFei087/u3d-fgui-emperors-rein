using FairyGUI;
using System.Collections.Generic;
using System;
using GameMaker;
using SBoxApi;
using Newtonsoft.Json;

namespace ConsoleCoinPusher01
{
    public class PageConsoleMain : MachinePageBase
    {

        public const string pkgName = "ConsoleCoinPusher01";
        public const string resName = "PageConsoleMain";
        public override PageType pageType => PageType.Overlay;
        protected override void OnInit()
        {
            
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
            /*
            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Game Controller/Push Game Main Controller.prefab",
            (GameObject clone) =>
            {
                callback();
            });
            */
            callback();

            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {

                        if(!isPermissions)
                            kbCtrl.ClickConfirm();
                        else
                            OnClickConfirm();
                    },
                    [MachineButtonKey.BtnTicketOut] = (info) =>
                    {
                        if (!isPermissions)
                            kbCtrl.ClickNext();
                        else
                            OnClickNext();
                    },
                    [MachineButtonKey.BtnSwitch] = (info) =>
                    {
                        if (!isPermissions)
                            kbCtrl.ClickPrev();
                        else
                            OnClickPrev();
                    },
                    [MachineButtonKey.BtnUp] = (info) =>
                    {
                        if (!isPermissions)
                            kbCtrl.ClickUp();
                        else
                            OnClickPrev();
                    },
                    [MachineButtonKey.BtnDown] = (info) =>
                    {
                        if (!isPermissions)
                            kbCtrl.ClickDown();
                        else
                            OnClickNext();
                    },

                    [MachineButtonKey.BtnConsole] = (info) =>
                    {
                        CloseSelf(null);
                    }
                }
            };

        }


        public override void OnOpen(PageName name, EventData data)
        {
            PageTitleManager.Instance.AddPageNode("/");
            base.OnOpen(name, data);
            InitParam();

            SBoxIdea.IntoConsolePage(1);
        }


        public override void OnClose(EventData data = null)
        {
            PageTitleManager.Instance.ClearPageNode();
            SBoxIdea.IntoConsolePage(0);

            permissions = -1;
            curIndexMenuItem = 0;

            base.OnClose(data);
        }



        

        GList glst;


        KeyBoard01Controller kbCtrl = new KeyBoard01Controller();
        InfoBaseController baseCtrl = new InfoBaseController();

        GComponent goMenu;
        GLabel labTip;




        Dictionary<int, string> menuMap;



        int curIndexMenuItem = 0;

        // 权限
        int permissions = -1;

        bool isPermissions => permissions >=1 && permissions <= 3;


        GComponent goItemSettings, goItemAdmin, goItemCheckCoinPush,goRecord;

        GComponent goLanguage;
        public override void InitParam()
        {

            //DebugUtils.LogError("i am here PageConsoleMain !!!!! ");


 
            if (!isInit) return;

            if (!isOpen) return;

            baseCtrl.InitParam(this.contentPane.GetChild("base").asCom, PageTitleManager.Instance.GetPagePathName());


            labTip = this.contentPane.GetChild("tip").asLabel;
            labTip.text = "";


            goMenu = this.contentPane.GetChild("items").asCom;


            ChangeMenuItem();


            kbCtrl.InitParam(this.contentPane.GetChild("keyboard").asCom, false, (Action<string>)((res) =>
            {
                DebugUtils.Log($"获取到的数据： {res}");

                try
                {
                    if (string.IsNullOrEmpty(res))
                    {
                        labTip.text = string.Format(I18nMgr.T("The {0} cannot be empty"),
                            I18nMgr.T("Password")) ;
                    }
                    else
                    {
                        int pwd = int.Parse(res);
                        MachineDataManager02.Instance.RequestCheckPassword(pwd, (Action<object>)((res) =>
                        {
                            SBoxPermissionsData data = res as SBoxPermissionsData;

                            if (data.result == 0 && data.permissions > 0)
                            {

                                permissions = data.permissions;//1：普通密码权限，2：管理员密码权限，3：超级管理员密码权限
                                SBoxModel.Instance.curPermissions = permissions;

                                if (SBoxModel.Instance.isCurPermissionsAdmin)
                                    SBoxModel.Instance.passwordAdmin = pwd;
                                else if (SBoxModel.Instance.isCurPermissionsManager)
                                    SBoxModel.Instance.passwordManager = pwd;
                                else if (SBoxModel.Instance.isCurPermissionsShift)
                                    SBoxModel.Instance.passwordShift = pwd;
                                
 
                                OnInterMenu(permissions);
                            }
                            else
                            {
                                labTip.text = I18nMgr.T("Error Password");
                            }

                        }), (err) =>
                        {
                            labTip.text = I18nMgr.T("The input value must be a number");
                        });
                    }
                }
                catch (Exception e)
                {
                    labTip.text = I18nMgr.T("The input value must be a number");
                }

            }),
            () =>
            {
                CloseSelf(null);
            });



            if (permissions == -1)
                OnInterKeyboard();
            else
                OnInterMenu(permissions);
        }



        void OnInterKeyboard()
        {
            kbCtrl.Enable();
            AddClickEvent();
            ClearArrow(true);
        }
        void OnInterMenu(int permissions)
        {
            if (permissions >= 2)
            {
                goMenu.AddChildAt(goItemSettings, goMenu.numChildren - 1);
            }

            if (permissions >= 3)
            {
                goMenu.AddChildAt(goItemAdmin, goMenu.numChildren - 1);
            }

            AddClickEvent();

            labTip.text = "";
            kbCtrl.Disable();
            ClearArrow(false);
        }


        void ChangeMenuItem()
        {
            goMenu = this.contentPane.GetChild("items").asCom;


            GComponent goItem = goMenu.GetChild("settings")?.asCom ?? null;
            if (goItem != null)
            {
                goMenu.RemoveChild(goItem);
                goItem.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().mark = "consoleMain-menu-language";
                goItem.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().InitParam(goItem);
                if (goItemSettings != null && goItemSettings.displayObject.gameObject != null)
                    goItemSettings.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount--;
                goItemSettings = goItem;
                goItemSettings.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount++;
            }



            goItem = goMenu.GetChild("language")?.asCom ?? null;
            if (goItem != null)
            {
                goMenu.RemoveChild(goItem);
                goItem.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().mark = "consoleMain-menu-language";
                goItem.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().InitParam(goItem);
                if (goLanguage != null && goLanguage.displayObject.gameObject != null)
                    goLanguage.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount--;
                goLanguage = goItem;
                goLanguage.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount++;
                goLanguage.GetChild("value").asRichTextField.text = SBoxModel.Instance.languageName;
            }

            goItem = goMenu.GetChild("admin")?.asCom ?? null;
            if (goItem != null)
            {
                goMenu.RemoveChild(goItem);
                goItem.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().mark = "consoleMain-menu-admin";
                goItem.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().InitParam(goItem);
                if (goItemAdmin != null && goItemAdmin.displayObject.gameObject != null)
                    goItemAdmin.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount--;
                goItemAdmin = goItem;
                goItemAdmin.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount++;
            }


            goItem = goMenu.GetChild("record")?.asCom ?? null;
            if (goItem != null)
            {
                goMenu.RemoveChild(goItem);
                goItem.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().mark = "consoleMain-menu-record";
                goItem.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().InitParam(goItem);
                if (goRecord != null&& goRecord.displayObject.gameObject!=null)
                    goRecord.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount--;
                goRecord = goItem;
                goRecord.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount++;
            }
            


            goItem = goMenu.GetChild("checkCoinPush")?.asCom ?? null;
            if (goItem != null)
            {
                goMenu.RemoveChild(goItem);
                goItem.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().mark = "consoleMain-menu-checkCoinPush";
                goItem.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().InitParam(goItem);
                if (goItemCheckCoinPush != null && goItemCheckCoinPush.displayObject.gameObject != null)
                    goItemCheckCoinPush.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount--;
                goItemCheckCoinPush = goItem;
                goItemCheckCoinPush.displayObject.gameObject.GetOrAddComponent<GOResidualMark>().referenceCount++;
            }


            if (PlayerPrefsUtils.isUseAllConsolePage)
            {
                goMenu.AddChildAt(goItemCheckCoinPush, 2);
            }

            goMenu.AddChildAt(goRecord, 0);
            goMenu.AddChildAt(goLanguage, 1);
        }


        void ClearArrow(bool isClearAllArrow)
        {

            menuMap = new Dictionary<int, string>();
            //curIndexMenuItem = 0;
            for (int i = 0; i < goMenu.numChildren; i++)
            {
                GComponent goItem = goMenu.GetChildAt(i).asCom;
                if(isClearAllArrow)
                    goItem.GetChild("icon").visible = false;
                else
                    goItem.GetChild("icon").visible = i == curIndexMenuItem;

                menuMap.Add(i, goItem.name);
            }

        }


        void SetAllow()
        {
            for (int i = 0; i < goMenu.numChildren; i++)
            {
                goMenu.GetChildAt(i).asCom.GetChild("icon").visible = i == curIndexMenuItem;
            }
        }
        void OnClickNext()
        {
            if (++curIndexMenuItem >= goMenu.numChildren)
                curIndexMenuItem = 0;
            SetAllow();
        }

        void OnClickPrev()
        {
            if (--curIndexMenuItem < 0 )
                curIndexMenuItem = goMenu.numChildren - 1;
            SetAllow();
        }

        void OnClickConfirm()
        {
            if (menuMap.ContainsKey(curIndexMenuItem))
            {

                switch (menuMap[curIndexMenuItem]) {
                    case "record":
                        {
                            PageManager.Instance.OpenPage(PageName.ConsolePusher01PageConsoleRecordChoose);
                        }
                        return;
                    case "checkHardware":
                        {
                            PageManager.Instance.OpenPage(PageName.ConsolePusher01PageConsoleCheckHardware02);
                        }
                        return;
                    case "checkCoinPush":
                        {
                            PageManager.Instance.OpenPage(PageName.ConsolePusher01PageConsoleTestCoinPush);
                        }
                        return;
                    case "settings":
                        {
                            PageManager.Instance.OpenPage(PageName.ConsolePusher01PageConsoleSettings);
                        }
                        return;
                    case "admin":
                        {
                            PageManager.Instance.OpenPage(PageName.ConsolePusher01PageConsoleAdmin);
                        }
                        return;
                    case "language":
                        {
                            OnClickLanguage();
                        }
                        return;
                    case "exit":
                        {
                            CloseSelf(null);
                        }
                        return;
                }
            }
        }


        void AddClickEvent()
        {
            for (int i = 0; i < contentPane.GetChild("items").asCom.numChildren; i++)
            {
                int index = i;
                contentPane.GetChild("items").asCom.GetChildAt(index).onClick.Clear();
                contentPane.GetChild("items").asCom.GetChildAt(index).onClick.Add(() =>
                {
                    if (!isPermissions)
                        return;
                    curIndexMenuItem = index;
                    SetAllow();
                    OnClickConfirm();
                });
            }
        }


        void OnClickLanguage()
        {
            int curIndex = 0;
            for(int i=0; i< SBoxModel.Instance.supportLanguage.Length; i++)
            {
                if (SBoxModel.Instance.language == SBoxModel.Instance.supportLanguage[i].number)
                {
                    curIndex = i;
                    break;
                }
            }
            if (++curIndex >= SBoxModel.Instance.supportLanguage.Length)
                curIndex = 0;
            SBoxModel.Instance.language = SBoxModel.Instance.supportLanguage[curIndex].number;
            goLanguage.GetChild("value").asRichTextField.text = SBoxModel.Instance.languageName;
            MachineDeviceCommonBiz.Instance.CheckLanguage();
        }
    }
}