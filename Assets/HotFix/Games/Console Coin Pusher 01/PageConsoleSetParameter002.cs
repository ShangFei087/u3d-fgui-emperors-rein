using FairyGUI;
using GameMaker;
using SBoxApi;
using System;
using System.CodeDom;
using System.Collections.Generic;
using UnityEngine;




namespace ConsoleCoinPusher01
{

    public class PageConsoleSetParameter002 : MachinePageBase
    {
        public const string pkgName = "ConsoleCoinPusher01";
        public const string resName = "PageConsoleSetParameter002";
        public override PageType pageType => PageType.Overlay;


        KeyBoard01Controller kbNumberCtrl = new KeyBoard01Controller();
        KeyBoard02Controller kbCtrl = new KeyBoard02Controller();
        InfoBaseController baseCtrl = new InfoBaseController();

        IKeyboard curKB;

        GComponent goMenu;
        int curIndexMenuItem = 0;

        bool is1;

        private string _set1;

        public string Set1
        {
            get => _set1;
            set
            {
                _set1 = value;
                goMenu.GetChildAt(0).asCom.GetChild("value").asRichTextField.text = _set1;
            }
        }

        private string _set2;

        public string Set2
        {
            get => _set2;
            set
            {
                _set2 = value;
                goMenu.GetChildAt(1).asCom.GetChild("value").asRichTextField.text = _set2;
            }
        }

        bool isShuRu,isChangPassword;

        Dictionary<int, string> menuMap;
        GObject tip;

 
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

                        if (isShuRu)
                            curKB.ClickConfirm();
                        else
                            OnClickConfirm();
                    },
                    [MachineButtonKey.BtnTicketOut] = (info) =>
                    {
                        if (isShuRu)
                            curKB.ClickNext();
                        else
                            OnClickNext();
                    },
                    [MachineButtonKey.BtnSwitch] = (info) =>
                    {
                        if (isShuRu)
                            curKB.ClickPrev();
                        else
                            OnClickPrev();
                    },
                    [MachineButtonKey.BtnDown] = (info) =>
                    {
                        if (isShuRu)
                            curKB.ClickDown();
                        else
                            OnClickNext();
                    },
                    [MachineButtonKey.BtnUp] = (info) =>
                    {
                        if (isShuRu)
                            curKB.ClickUp();
                        else
                            OnClickPrev();
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
            if (data != null)
            {
                Dictionary<string, object> argDic = (Dictionary<string, object>)data.value;
                if (argDic.ContainsKey("title"))
                {
                    PageTitleManager.Instance.AddPageNode((string)argDic["title"]);
                }
            }
            base.OnOpen(name, data);
            InitParam();
        }
        public override void OnClose(EventData data = null)
        {
            PageTitleManager.Instance.RemoveLastPageNode();
            base.OnClose(data);
        }

        public override void InitParam()
        {

      
            if (!isInit) return;

            if (!isOpen) return;

            baseCtrl.InitParam(this.contentPane.GetChild("base").asCom, PageTitleManager.Instance.GetPagePathName());


            goMenu = this.contentPane.GetChild("items").asCom;
            tip = this.contentPane.GetChild("tip");
            tip.text = "";


            kbNumberCtrl.InitParam(this.contentPane.GetChild("keyboard01").asCom, true, (res) =>
            {
                DebugUtils.Log($"获取到的数据： {res}");

                goMenu.grayed = false;
                goMenu.touchable = true;
                isShuRu = false;
                //kbNumberCtrl.Clear(true);
                kbNumberCtrl.Disable();
                if (string.IsNullOrEmpty(res))
                {
                    return;
                }
                if (is1)
                {
                    Set1 = res.ToString();
                    if (isChangPassword)
                    {
                        tip.text = I18nMgr.T("Please enter your password confirmation");
                    }
                }
                else
                {
                    Set2 = res.ToString();
                }

                SetAllow();
            },
            () =>
            {
                goMenu.grayed = false;
                goMenu.touchable = true;
                isShuRu = false;
                //kbNumberCtrl.Clear(true);
                kbNumberCtrl.Disable();
            });



            kbCtrl.InitParam(this.contentPane.GetChild("keyboard").asCom, true, (res) =>
            {
                DebugUtils.Log($"获取到的数据： {res}");

                goMenu.grayed = false;
                goMenu.touchable = true;
                isShuRu = false;
                //kbCtrl.Clear(true);
                kbCtrl.Disable();
                if (string.IsNullOrEmpty(res))
                {
                    return;
                }
                if (is1)
                {
                    Set1 = res.ToString();
                    if (isChangPassword)
                    {
                        tip.text = I18nMgr.T("Please enter your password confirmation");
                    }
                }
                else
                {
                    Set2 = res.ToString();
                }

                SetAllow();

            },
            () =>
            {
                goMenu.grayed = false;
                goMenu.touchable = true;
                isShuRu = false;
                //kbCtrl.Clear(true);
                kbCtrl.Disable();
            });


            kbCtrl.goOwnerKeyboard.visible = true;
            curKB = kbCtrl;





            AddClickEvent();
            isShuRu = false;
            isChangPassword = false;
            goMenu.grayed = false;
            goMenu.touchable = true;
            Clear(false);

            Dictionary<string, object> argDic = null;
            if (inParams != null)
            {
                argDic = (Dictionary<string, object>)inParams.value;

                if (argDic.ContainsKey("parameter1"))
                {
                    goMenu.GetChildAt(0).asCom.GetChild("title").asRichTextField.text = (string)argDic["parameter1"];
                }
                if (argDic.ContainsKey("parameter2"))
                {
                    goMenu.GetChildAt(1).asCom.GetChild("title").asRichTextField.text = (string)argDic["parameter2"];
                }

                if ((string)argDic["parameter1"] == I18nMgr.T("input new password:"))
                {
                    isChangPassword = true;
                    Set1 = "";
                    Set2 = "";
                    tip.text = I18nMgr.T("Please enter your new password first");
                }
                else
                {
                    if (argDic.ContainsKey("parameter1Value"))
                    {
                        Set1 = (string)argDic["parameter1Value"];
                    }

                    if (argDic.ContainsKey("parameter2Value"))
                    {
                        Set2 = (string)argDic["parameter2Value"];
                    }
                }

                if (argDic.ContainsKey("isNumber") && (bool)argDic["isNumber"])
                {
                    kbNumberCtrl.goOwnerKeyboard.visible = true;
                    kbCtrl.goOwnerKeyboard.visible = false;

                    curKB = kbNumberCtrl;
                }
                else
                {
                    kbNumberCtrl.goOwnerKeyboard.visible = false;
                    kbCtrl.goOwnerKeyboard.visible = true;
           
                    curKB = kbCtrl;
                }

            }


            curKB.Disable();

        }





        void Clear(bool isClearAllArrow)
        {

            menuMap = new Dictionary<int, string>();
            curIndexMenuItem = 0;
            goMenu = this.contentPane.GetChild("items").asCom;
            for (int i = 0; i < goMenu.numChildren; i++)
            {
                GComponent goItem = goMenu.GetChildAt(i).asCom;
                if (isClearAllArrow)
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
            if (--curIndexMenuItem < 0)
                curIndexMenuItem = goMenu.numChildren - 1;
            SetAllow();
        }

        void OnClickConfirm()
        {
            if (menuMap.ContainsKey(curIndexMenuItem))
            {

                switch (menuMap[curIndexMenuItem])
                {
                    case "setting1":
                        {
                            Setting1();
                        }
                        return;
                    case "setting2":
                        {
                            Setting2();
                        }
                        return;
                    case "sure":
                        {
                            Sure();
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
            for (int i = 0; i < goMenu.numChildren; i++)
            {
                int index = i;
                contentPane.GetChild("items").asCom.GetChildAt(index).asCom.GetChild("title").onClick.Clear();
                contentPane.GetChild("items").asCom.GetChildAt(index).asCom.GetChild("title").onClick.Add(() =>
                {
                    curIndexMenuItem = index;
                    SetAllow();
                    OnClickConfirm();

                });
                contentPane.GetChild("items").asCom.GetChildAt(index).asCom.GetChild("value").onClick.Clear();
                contentPane.GetChild("items").asCom.GetChildAt(index).asCom.GetChild("value").onClick.Add(() =>
                {
                    curIndexMenuItem = index;
                    SetAllow();
                    if (curIndexMenuItem != goMenu.numChildren - 1 && curIndexMenuItem != goMenu.numChildren - 2)
                    {
                        OnClickConfirm();
                    }
                });
            }
        }

        void Setting1()
        {
            goMenu.grayed = true;
            goMenu.touchable = false;
            curKB.Enable();
            is1 = true;
            isShuRu = true;
        }

        void Setting2()
        {
            goMenu.grayed = true;
            goMenu.touchable = false;
            curKB.Enable();
            is1 = false;
            isShuRu = true;
        }





        void Sure()
        {
            if (isChangPassword)
            {
                if (Set1 == Set2)
                {
                    //处理相关修改新密码逻辑

                    CloseSelf(null);
                }
                else
                {
                    tip.text = I18nMgr.T("The passwords entered twice are not the same");
                }
            }
            else
            {
                CloseSelf(new EventData<Dictionary<string, object>>("",
                           new Dictionary<string, object>()
                           {
                               ["value1"] = Set1,
                               ["value2"] = Set2,
                           }
                           ));

            }
        }

    }
}

