using FairyGUI;
using System.Collections.Generic;
using GameMaker;
using System;
using UnityEngine.UI;

namespace ConsoleSlot01
{
    public class PopupConsoleSound : PageBase
    {
        public const string pkgName = "Console";
        public const string resName = "PopupConsoleSound";
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
        }

        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            InitParam();
        }

        // public override void OnTop() { DebugUtils.Log($"i am top {this.name}"); }

        GButton btnClose;

        GSlider sldSoundEff, sldMusic;

        GButton tgIsMute;

        


        float musicVal=0, soundEffVal=0;
        bool isMute = false;
        public override void InitParam()
        {


            if (!isInit) return;

            if (!isOpen) return;

            btnClose =  this.contentPane.GetChild("btnExit").asButton;
            btnClose.onClick.Clear();
            btnClose.onClick.Add(() => {
                CloseSelf(new EventData("Exit"));
            });

            sldSoundEff = this.contentPane.GetChild("soundSlider").asSlider;
            sldSoundEff.onChanged.Clear();

            sldMusic = this.contentPane.GetChild("musicSlider").asSlider;
            sldMusic.onChanged.Clear();

            tgIsMute = this.contentPane.GetChild("isMute").asButton;
            tgIsMute.onChanged.Clear();

            sldSoundEff.value = SBoxModel.Instance.sound;
            sldMusic.value = SBoxModel.Instance.music;
            tgIsMute.selected = SBoxModel.Instance.isMute;

            sldSoundEff.onChanged.Add(OnSoundEffChange);
            sldMusic.onChanged.Add(OnMusicChange);
            tgIsMute.onChanged.Add(OnIsMuteChange);
        }


        void OnSoundEffChange(EventContext context)
        {
            GSlider sld = context.sender as GSlider;
            DebugUtils.Log((float)Math.Round(sld.value, 3));

            SBoxModel.Instance.sound = (float)Math.Round(sld.value, 3);
        }
        void OnMusicChange(EventContext context)
        {
            GSlider sld = context.sender as GSlider;
            DebugUtils.Log((float)Math.Round(sld.value, 3));

            SBoxModel.Instance.music = (float)Math.Round(sld.value, 3);
        }


        void OnIsMuteChange(EventContext context)
        {
            GButton toggle = context.sender as GButton;
            DebugUtils.Log(toggle.selected);

            SBoxModel.Instance.isMute = toggle.selected;
        }
    }

}
