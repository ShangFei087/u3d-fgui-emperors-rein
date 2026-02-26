using FairyGUI;
using GameMaker;
using SBoxApi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace TestHall
{
    public class TestHallMain : MachinePageBase
    {
        public const string pkgName = "TestHall";
        public const string resName = "TestHallMain";

        GButton btn1,btn2,btn3, btn4, btn5;
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

            // 添加事件监听
            GameSoundHelper.Instance.PlayMusicSingle(SoundKey.RegularBG);
            InitParam();
        }

        public override void OnClose(EventData data = null)
        {
            // 删除事件监听
            GameSoundHelper.Instance.StopMusic();
            base.OnClose(data);
        }

        public override void InitParam()
        {

            if (!isInit) return;
            Debug.Log("InitParam TestHall");
            btn1 = this.contentPane.GetChild("n1").asButton;
            btn1.onClick.Clear();
            btn1.onClick.Add(() => {
                if (!ApplicationSettings.Instance.isMock)
                {
                    SBoxIdea.GameSwitch(1700);
                }
                PageManager.Instance.OpenPage(PageName.SlotZhuZaiJinBiPopupGameLoading);
                CloseSelf(null);
            });

            btn2 = this.contentPane.GetChild("n4").asButton;
            btn2.onClick.Clear();
            btn2.onClick.Add(() => {
                if (!ApplicationSettings.Instance.isMock)
                {
                    SBoxIdea.GameSwitch(3999);
                }
                PageManager.Instance.OpenPage(PageName.CaiFuZhiMenPopupGameLoading);
                CloseSelf(null);
            });

            btn3 = this.contentPane.GetChild("n5").asButton;
            btn3.onClick.Clear();
            btn3.onClick.Add(() => {
                if (!ApplicationSettings.Instance.isMock)
                {
                    SBoxIdea.GameSwitch(3998);
                }
                PageManager.Instance.OpenPage(PageName.XingYunZhiLunPopupGameLoading);
                CloseSelf(null);
            });

            btn4 = this.contentPane.GetChild("n6").asButton;
            btn4.onClick.Clear();
            btn4.onClick.Add(() => {
                if (!ApplicationSettings.Instance.isMock)
                {
                    SBoxIdea.GameSwitch(3997);
                }
                PageManager.Instance.OpenPage(PageName.CaiFuZhiJiaPopupGameLoading);
                CloseSelf(null);
            });

            btn5 = this.contentPane.GetChild("n7").asButton;
            btn5.onClick.Clear();
            btn5.onClick.Add(() => {
                if (!ApplicationSettings.Instance.isMock)
                {
                    SBoxIdea.GameSwitch(3996);
                }
                PageManager.Instance.OpenPage(PageName.CaiFuHuoChePopupGameLoading);
                CloseSelf(null);
            });
        }
    }
}

