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

        private TimerCallback _closeTimer;

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
                    InitParam(null);
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

            GComponent loadAnchorBGTip = contentPane.GetChild("anchorDoor").asCom;
            if (loadAnchorBG != loadAnchorBGTip)
            {
                GameCommon.FguiUtils.DeleteWrapper(loadAnchorBG);
                loadAnchorBG = loadAnchorBGTip;
                go = GameObject.Instantiate(goFgCloneEnter);
                GameCommon.FguiUtils.AddWrapper(loadAnchorBG, go);
            }

            ContentModel.Instance.btnSpinState = ContentModel.Instance.curBtnSpinState;

            preLoadedCallback?.Invoke();

            if (isOpen)
            {
                // 泄漏：匿名 lambda 关页清不掉。
                // Timers.inst.Add(2f / Time.timeScale, 1, (object obj) =>
                // {
                //     CloseSelf(null);
                // });

                if (_closeTimer != null)
                    Timers.inst.Remove(_closeTimer);
                _closeTimer = (object obj) =>
                {
                    _closeTimer = null;
                    CloseSelf(null);
                };
                Timers.inst.Add(2f / Time.timeScale, 1, _closeTimer);
            }
        }
    }
}
