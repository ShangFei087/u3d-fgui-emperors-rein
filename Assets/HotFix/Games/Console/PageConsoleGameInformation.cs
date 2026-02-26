using FairyGUI;
using GameMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConsoleSlot01
{
    public class PageConsoleGameInformation : PageBase
    {
        public const string pkgName = "Console";
        public const string resName = "PageConsoleGameInformation";
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

        // public override void OnTop() {  DebugUtils.Log($"i am top {this.name}"); }

        GButton btnClose;

        GComponent cmpProducedBy, cmpMachineName, cmpSoftwareVer, cmpHardwareVer, cmpAlgorithmVer;
        public override void InitParam()
        {
            btnClose = this.contentPane.GetChild("navBottom").asCom.GetChild("btnExit").asButton;
            btnClose.onClick.Clear();
            btnClose.onClick.Add(() =>
            {
                CloseSelf(null);
            });


            cmpMachineName = this.contentPane.GetChild("machineName").asCom;
            cmpSoftwareVer = this.contentPane.GetChild("softwareVer").asCom;
            cmpHardwareVer = this.contentPane.GetChild("hardwareVer").asCom;
            cmpProducedBy = this.contentPane.GetChild("producedBy").asCom;
            cmpAlgorithmVer = this.contentPane.GetChild("algorithmVer").asCom;

            cmpMachineName.GetChild("value").asRichTextField.text = I18nMgr.T(ApplicationSettings.Instance.gameTheme);
            cmpSoftwareVer.GetChild("value").asRichTextField.text = $"{ApplicationSettings.Instance.appVersion}/{GlobalData.hotfixVersion}";
            cmpHardwareVer.GetChild("value").asRichTextField.text = SBoxModel.Instance.HardwareVer;
            cmpAlgorithmVer.GetChild("value").asRichTextField.text = SBoxModel.Instance.AlgorithmVer;

#if false
        StreamingAssetsBundleLoader.Instance.LoadAsset<Texture2D>(ConfigUtils.curGameAvatarURL,
            //ResourceManager02.Instance.LoadAsset<Texture2D>(HotfixSettings.Instance.curGameAvatarURL,
            //"Assets/GameRes/_Common/Game Maker/ABs/152/Game Icon/PssOn00152.png",
            (Texture2D texture) =>
            {
                NTexture nTexture = new NTexture(texture);

                GLoader icon = cmpProducedBy.GetChild("icon").asLoader;
                icon.texture = nTexture;

                //icon.fill = FillType.Scale;                                  
                icon.fill = FillType.ScaleFree;      // 等比缩放，可能留白                                       
                //icon.fill = FillType.ScaleNoBorder;  // 等比缩放，完全填充（可能裁剪）

                /*
                NTexture nTexture = new NTexture(texture);
                //cmpProducedBy.GetChild("icon").asImage.texture = nTexture;
                */
            });
#endif


            string pth = Application.isEditor ?
                PathHelper.GetAssetBackupSAPTH(ConfigUtils.curGameAvatarURL) :
                PathHelper.GetAssetBackupLOCPTH(ConfigUtils.curGameAvatarURL);

            FileLoaderManager.Instance.LoadImageAsTexture(pth, (Texture2D texture) =>
            {
                NTexture nTexture = new NTexture(texture);

                GLoader icon = cmpProducedBy.GetChild("icon").asLoader;
                icon.texture = nTexture;

                //icon.fill = FillType.Scale;                                  
                icon.fill = FillType.ScaleFree;      // 等比缩放，可能留白                                       
                                                     //icon.fill = FillType.ScaleNoBorder;  // 等比缩放，完全填充（可能裁剪）

                /*
                NTexture nTexture = new NTexture(texture);
                //cmpProducedBy.GetChild("icon").asImage.texture = nTexture;
                */
            });
        }
    }
}