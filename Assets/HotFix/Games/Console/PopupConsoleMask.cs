using FairyGUI;
using GameMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ConsoleSlot01
{
    public class PopupConsoleMask : PageBase
    {
        public const string pkgName = "Console";
        public const string resName = "PopupConsoleMask";
        public override PageType pageType => PageType.Overlay;
        protected override void OnInit()
        {
            
            base.OnInit();
        }


        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            InitParam();
        }

        public override void InitParam()
        {
        }
    }
}