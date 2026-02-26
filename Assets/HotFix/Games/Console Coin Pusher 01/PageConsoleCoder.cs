using FairyGUI;
using System.Collections.Generic;
using System;
using GameMaker;



namespace ConsoleCoinPusher01
{
    public class PageConsoleCoder : MachinePageBase
    {

        public const string pkgName = "ConsoleCoinPusher01";
        public const string resName = "PageConsoleCoder";
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
                        kbCtrl.ClickConfirm();
                    },
                    [MachineButtonKey.BtnTicketOut] = (info) =>
                    {
                        kbCtrl.ClickNext();
                    },
                    [MachineButtonKey.BtnSwitch] = (info) =>
                    {
                        kbCtrl.ClickPrev();
                    },
                    [MachineButtonKey.BtnDown] = (info) =>
                    {
                        kbCtrl.ClickDown();
                    },
                    [MachineButtonKey.BtnUp] = (info) =>
                    {
                        kbCtrl.ClickUp();
                    },
                    [MachineButtonKey.BtnConsole] = (info) =>
                    {
                        CloseSelf(new EventData("Exit"));
                    }
                }
            };

        }

  
        public override void OnOpen(PageName name, EventData data)
        {
            PageTitleManager.Instance.AddPageNode("激活报码");
            base.OnOpen(name, data);
            InitParam();
        }
        public override void OnClose(EventData data = null)
        {
            PageTitleManager.Instance.RemoveLastPageNode();
            base.OnClose(data);
        }


        


        KeyBoard01Controller kbCtrl = new KeyBoard01Controller();
        InfoBaseController baseCtrl = new InfoBaseController();

        GLabel labTip;


        string A, B, C, D, E, day, hour, minute;
        GTextField txtA, txtB, txtC, txtD, txtE, txtTime;
        public override void InitParam()
        {

            if (!isInit) return;

            if (!isOpen) return;

            baseCtrl.InitParam(this.contentPane.GetChild("base").asCom, PageTitleManager.Instance.GetPagePathName());

            labTip = this.contentPane.GetChild("tip").asLabel;
            labTip.text = "";

            kbCtrl.InitParam(this.contentPane.GetChild("keyboard").asCom, true, (res) =>
            {
                //DebugUtils.Log($"@@获取到的数据： {res}");

                try
                {
                    if (string.IsNullOrEmpty(res))
                    {
                        labTip.text = string.Format(I18nMgr.T("The {0} cannot be empty"),
                            I18nMgr.T("Password"));
                    }
                    else
                    {
                        ulong pwd = ulong.Parse(res); //!!!!!

                        CloseSelf(new EventData<string>("Result", $"{pwd}"));

                    }
                }
                catch (Exception e)
                {
                    labTip.text = I18nMgr.T("The input value must be a number");
                }

            },
            () =>
            {
                CloseSelf(new EventData("Exit"));
            });


            txtA = this.contentPane.GetChild("totalWin").asCom.GetChild("value").asRichTextField;
            txtB = this.contentPane.GetChild("totalBet").asCom.GetChild("value").asRichTextField;
            txtC = this.contentPane.GetChild("machineId").asCom.GetChild("value").asRichTextField;
            txtD = this.contentPane.GetChild("activeCount").asCom.GetChild("value").asRichTextField;
            txtE = this.contentPane.GetChild("checkCode").asCom.GetChild("value").asRichTextField;
            txtTime = this.contentPane.GetChild("remianTime").asCom.GetChild("title").asRichTextField;


            Dictionary<string, object> argDic = null;
            if (inParams != null)
            {
                argDic = (Dictionary<string, object>)inParams.value;
                A = (string)argDic["A"];
                B = (string)argDic["B"];
                C = (string)argDic["C"];
                D = (string)argDic["D"];
                E = (string)argDic["E"];
                day = (string)argDic["Day"];
                hour = (string)argDic["Hour"];
                minute = (string)argDic["Minute"];
            }
            txtA.text = $"A: {A}";
            txtB.text = $"B: {B}";
            txtC.text = $"C: {C}";
            txtD.text = $"D: {D}";
            txtE.text = $"E: {E}";
            txtTime.text = string.Format(I18nMgr.T("remaining time: {0} days; {1} hours; {2} minute;"), day, hour, minute);


            kbCtrl.Enable();
        }





    }
}