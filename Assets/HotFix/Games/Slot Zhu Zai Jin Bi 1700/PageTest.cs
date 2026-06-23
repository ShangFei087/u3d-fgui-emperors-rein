using FairyGUI;
using GameMaker;
using System;

namespace SlotZhuZaiJinBi1700
{

    public class PageTest : PageBase

    {
        public const string pkgName = "SlotZhuZaiJinBi1700";

        public const string resName = "PageTest";


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
        }

        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            //TestManager.Instance.SetToolActive(false);
            InitParam();
        }

        public override void OnClose(EventData data = null)
        {
            TestUtils.CheckTestManager();
            base.OnClose(data);
        }

        public override void OnTop()
        {

            DebugUtils.Log($"i am top {name}");

        }

        public override void InitParam()
        {
            if (!isInit || !isOpen)
            {
                return;
            }
        }
    }
}


