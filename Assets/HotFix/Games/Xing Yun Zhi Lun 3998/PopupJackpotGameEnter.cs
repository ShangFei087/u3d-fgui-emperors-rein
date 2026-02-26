using FairyGUI;
using GameMaker;
using SlotMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace XingYunZhiLun_3998
{
    public class PopupJackpotGameEnter : MachinePageBase
    {
        public new const string pkgName = "XingYunZhiLun_3998";
        public new const string resName = "PopupJackpotGameEnter";

        private GameObject goFgCloneEnter, go;

        private GComponent loadAnchorBG;

        private EventData _data;
        private bool isInit = false;

        protected override void OnInit()
        {
            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/PopupGameJackpot/PushJackpotEnter",
                (GameObject clone) =>
                {
                    goFgCloneEnter = clone;
                    isInit = true;
                });
        }

        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            InitParam(null);
        }


        public void InitParam(EventData data)
        {
            if (data != null) _data = data;

            if (!isInit) return;

            //初始化菜单ui
            //GComponent gOwnerPanel = this.contentPane.GetChild("panel").asCom;
            //ContentModel.Instance.goAnthorPanel = gOwnerPanel;
            //MainModel.Instance.contentMD.goAnthorPanel = gOwnerPanel;
            //// 事件放出
            ////goGameCtrl.transform.Find("Panel").GetComponent<PanelController01>().Init();
            //EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
            //    new EventData<GComponent>(PanelEvent.AnchorPanelChange, gOwnerPanel));
            //ContentModel.Instance.btnSpinState = SpinButtonState.Stop;

            GComponent loadAnchorBGTip = contentPane.GetChild("anchorDoor").asCom;
            if (loadAnchorBG != loadAnchorBGTip)
            {
                GameCommon.FguiUtils.DeleteWrapper(loadAnchorBG);
                loadAnchorBG = loadAnchorBGTip;
                go = GameObject.Instantiate(goFgCloneEnter);
                GameCommon.FguiUtils.AddWrapper(loadAnchorBG, go);
            }

            ContentModel.Instance.btnSpinState = ContentModel.Instance.curBtnSpinState;


            Timers.inst.Add(2f, 1, (object obj) =>
            {
                CloseSelf(null);
            });
        }
    }
}
