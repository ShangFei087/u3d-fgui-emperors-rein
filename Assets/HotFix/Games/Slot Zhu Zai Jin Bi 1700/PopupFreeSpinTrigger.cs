using FairyGUI;
using GameMaker;
using PusherEmperorsRein;
using Spine.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlotZhuZaiJinBi1700
{
    public class PopupFreeSpinTrigger : MachinePageBase
    {
        public new const string pkgName = "SlotZhuZaiJinBi1700";
        public new const string resName = "PopupFreeSpinTrigger";
        //UI组件
        GComponent anchorPopup;
        Animator animatorPopup;
        GTextField freeNumber;
        //预制体
        private GameObject goPopup;
        private GameObject ClonegoPopup;
        //数据
        bool isInit = false;
        EventData _data = null;
        protected override void OnInit()
        {
            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            int count = 1;

            Action callback = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam(null);
                }
            };

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/PopupFreeSpinTrigger/ng_Popup.prefab",
            (GameObject clone) =>
            {
                goPopup = clone;
                callback();
            });

            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        Debug.LogError("游戏接受到机台短按的数据：Spin");
                        //SpinDown();
                    }
                },
            };

        }

        protected override void OnLanguageChange(I18nLang lang)
        {
            FguiI18nTextAssistant.Instance.DisposeAllTranslate(this.contentPane);
            this.contentPane.Dispose(); // 释放当前UI
            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            InitParam(null);
            //FguiI18nTextAssistant.Instance.TranslateComponent(this.contentPane);
        }
        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            InitParam(data);
        }

        // public override void OnTop() { Debug.Log($"i am top {this.name}"); }
        public void InitParam(EventData data)
        {

            if (data != null) _data = data;
            if (!isInit) return;

            GComponent LocalAnchoPopup = this.contentPane.GetChild("anchorPopup").asCom;
            if (anchorPopup != LocalAnchoPopup)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorPopup);
                ClonegoPopup = GameObject.Instantiate(goPopup);
                animatorPopup = ClonegoPopup.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                anchorPopup = LocalAnchoPopup;
                GameCommon.FguiUtils.AddWrapper(anchorPopup, ClonegoPopup);
                anchorPopup.visible = false;
            }
            freeNumber = this.contentPane.GetChild("number").asTextField;
            freeNumber.visible = false;
            Dictionary<string, object> argDic = null;
            if (_data != null)
            {
                argDic = (Dictionary<string, object>)_data.value;
                if (argDic.ContainsKey("freeSpinCount"))
                {
                    int count = (int)argDic["freeSpinCount"];
                    //freeNumber.text = $"{count}";
                }
            }


            ShowAni();
        }

        private void ShowAni()
        {
            anchorPopup.visible = true;
            animatorPopup.Play("open");
            freeNumber.visible = true;

            Timers.inst.Add(3, 1, (object obj) =>
            {
                freeNumber.visible = false;
                animatorPopup.Play("close");
                Timers.inst.Add(1f, 1, (object obj) => { CloseSelf(null); });
            });
        }
    }
}

