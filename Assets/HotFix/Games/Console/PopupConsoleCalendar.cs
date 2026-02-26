using FairyGUI;
using GameMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SpringGUI;


namespace ConsoleSlot01
{
    public class PopupConsoleCalendar : PageBase
    {
        public const string pkgName = "Console";
        public const string resName = "PopupConsoleCalendar";
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
            callback();
            /*ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Emperors Rein 200/Prefabs/Game Controller/Push Game Main Controller.prefab",
            (GameObject clone) =>
            {
                callback();
            });*/

        }

        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            InitParam();
        }

        // public override void OnTop() { DebugUtils.Log($"i am top {this.name}"); }

        GButton btnClose, btnSave, btnCancel;



        IVCalendar calendarView = IViewManager.Instance.GetIV<IVCalendar>();
        //IVCalendar calendarView = new CalendarView();
        CalendarController calendarCtrl = new CalendarController();
        public override void InitParam()
        {


            if (!isInit) return;

            if (!isOpen) return;

            btnClose = this.contentPane.GetChild("btnExit").asButton;
            btnClose.onClick.Clear();
            btnClose.onClick.Add(() =>
            {
                CloseSelf(new EventData("Exit"));
            });

            btnCancel = this.contentPane.GetChild("button1").asButton;
            btnCancel.onClick.Clear();
            btnCancel.onClick.Add(() =>
            {
                CloseSelf(new EventData("Exit"));
            });


            btnSave = this.contentPane.GetChild("button2").asButton;
            btnSave.onClick.Clear();
            btnSave.onClick.Add(() =>
            {
                Dictionary<string, object> result = new Dictionary<string, object>()
                {
                    ["date"] = calendarCtrl.GetLastSelectDate(),
                    ["timestamp"] = calendarCtrl.GetLastSelectTimestamp(),
                };
                CloseSelf(new EventData<Dictionary<string, object>>("Result", result));
            });

            //calendarView.InitParam(this.contentPane); // 注入ui的实例
            //calendarCtrl.InitParam(calendarView);  // 注入iv的实例

            calendarView.InitParam(this.contentPane);
            calendarCtrl.InitParam(calendarView);
            
        }
    }
}


