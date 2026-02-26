using FairyGUI;
using GameMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SoundKey = PusherEmperorsRein.SoundKey;


namespace PusherEmperorsRein
{

    public class PopupFreeSpinTrigger : MachinePageBase
    {
        public const string pkgName = "EmperorsRein";
        public const string resName = "PopupFreeSpinTrigger";

        protected override void OnInit()
        {
            
            base.OnInit();

            //GameSoundHelper.Instance.PlayMusicSingle(SoundKey.FreeSpinTriggerBG);
            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Emperors Rein 200/Prefabs/PopupFreeSpinTrigger/FgFadeTips.prefab",
                (GameObject clone) =>
                {

                    goAnchorSpineFg = clone;

                    //GameObject fguiContainer = lodAnchorBG.displayObject.gameObject;
                    //go.transform.SetParent(fguiContainer.transform, false);

                    isInit = true;
                    InitParam();
                });

            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                downClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        btnStrat.SetScale(0.8f, 0.8f);
                        //  PageManager.Instance.OpenPage(PageName.EmperorsReinPageERGameMain);
                    }
                },
                upClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        btnStrat.SetScale(1f, 1f);
                        btnStrat.FireClick(false, true);

                    }
                },
            };
        }



        public override void OnOpen(PageName name, EventData data)
        {
            //if (GameSoundHelper.Instance.IsPlaySound(SoundKey.RegularBG))
            //{
            //    GameSoundHelper.Instance.StopSound(SoundKey.RegularBG);
            //}
            //GameSoundHelper.Instance.PlayMusicSingle(SoundKey.FreeSpinTriggerBG);

            base.OnOpen(name, data);
            InitParam();
        }

        // public override void OnTop() { DebugUtils.Log($"i am top {this.name}"); }

        /// <summary> 抬头 </summary>
        string title = "";

        /// <summary> 是否明文 </summary>
        bool isPlaintext = false;

        string inputText = "";


        private GameObject goAnchorSpineFg;

        GComponent lodAnchorQi;
        GButton btnStrat;

        public override void InitParam()
        {

            if (!isInit) return;

            if (!isOpen) return;

            GComponent lodAnchortip = this.contentPane.GetChild("anchorSpine").asCom;
            if (lodAnchorQi != lodAnchortip)
            {
                GameCommon.FguiUtils.DeleteWrapper(lodAnchorQi);
                lodAnchorQi = lodAnchortip;
                GameCommon.FguiUtils.AddWrapper(lodAnchorQi, GameObject.Instantiate(goAnchorSpineFg));

                lodAnchorQi.visible = true;
            }



            btnStrat = this.contentPane.GetChild("ButtonStart").asButton;
            btnStrat.onClick.Clear();
            btnStrat.onClick.Add(() =>
            {
                GameSoundHelper.Instance.StopSound(SoundKey.FreeSpinTriggerBG);
                GameSoundHelper.Instance.PlayMusicSingle(SoundKey.FreeSpinBG);
                CloseSelf(new EventData<string>("Result", "i am here 1"));

            });


            Dictionary<string, object> argDic = null;
            if (inParams != null)
            {
                argDic = (Dictionary<string, object>)inParams.value;
                if (argDic.ContainsKey("freeSpinCount"))
                {
                    int count = (int)argDic["freeSpinCount"];
                    this.contentPane.GetChild("n4").visible = true;
                    this.contentPane.GetChild("n4").asTextField.text = $"{count}";
                }
            }

            if (!PlayerPrefsUtils.isPauseAtPopupFreeSpinTrigger)
            {
                Timers.inst.Remove(AutoCloseSelf);
                Timers.inst.Add(3f, 1, AutoCloseSelf);
            }



        }

        void AutoCloseSelf(object param)
        {
            GameSoundHelper.Instance.StopSound(SoundKey.FreeSpinTriggerBG);
            GameSoundHelper.Instance.PlayMusicSingle(SoundKey.FreeSpinBG);
            CloseSelf(null);
        }

       

        void OnButtonStart(int value)
        {
            //PageManager.Instance.ClosePage(this, new EventData<string>("Result", value));
            CloseSelf(new EventData<int>("Result", value));
        }

    }

}