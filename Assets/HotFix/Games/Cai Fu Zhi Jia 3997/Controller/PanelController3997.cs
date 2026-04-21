using FairyGUI;
using GameMaker;
using PusherEmperorsRein;
using SBoxApi;
using SlotMaker;
using System;
using UnityEngine;

namespace CaiFuZhiJia_3997
{
    public class PanelController3997 : PanelBaseController
    {
        protected override string PanelPackagePath => "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/FGUIs";

         protected override string PanelUrl => "ui://CaiFuZhiJia_Panel/Panel";

        public override void Init(EventData res = null)
        {
            base.Init(res);
            
            // GComponent _goAnchorPanel = null;
            // if (res != null)
            //     _goAnchorPanel = res.value as GComponent;
            // else if (MainModel.Instance.contentMD != null)
            //     _goAnchorPanel = MainModel.Instance.contentMD.goAnthorPanel;
            //
            // if (_goAnchorPanel == null)
            // {
            //     return;
            // }
            //
            // int count = 2;
            // Action loadComplete = () =>
            // {
            //     // 两个异步资源都完成后再进行参数初始化
            //     if (--count == 0)
            //     {
            //         isInit = true;
            //         InitParam();
            //     }
            // };
            //
            //
            // if (gOwnerPanel != _goAnchorPanel && _goAnchorPanel != null)
            // {
            //     if (UIPackage.GetByName(PanelPackageName) == null)
            //     {
            //         // 首次进入时先加载 FairyGUI 包
            //         ResourceManager02.Instance.LoadAssetBundleAsync("Assets/GameRes/Games/Cai Fu Zhi Jia 3997/FGUIs", (ab) =>
            //         {
            //             UIPackage.AddPackage(ab);
            //             GLoader anchorPanel = _goAnchorPanel.GetChild("icon").asLoader;
            //             anchorPanel.url = "ui://Panel01/Panel";
            //             gOwnerPanel = _goAnchorPanel.GetChild("icon").asLoader.component;
            //             gOwnerPanel.visible = true;
            //             loadComplete();
            //         });
            //     }
            //     else
            //     {
            //         // 已加载过包时直接复用
            //         GLoader anchorPanel = _goAnchorPanel.GetChild("icon").asLoader;
            //         anchorPanel.url = "ui://Panel01/Panel";
            //
            //         gOwnerPanel = _goAnchorPanel.GetChild("icon").asLoader.component;
            //         loadComplete();
            //     }
            // }
            //
            // // 异步加载 Spin 按钮预制体
            // ResourceManager02.Instance.LoadAsset<GameObject>(SpinPrefabPath,
            //     (GameObject clone) =>
            //     {
            //         goSpin = clone;
            //         loadComplete();
            //     });
        }

        protected override void OnPropertyGameState(EventData res = null)
        {
            string gameState = (string)res?.value;

            if (gameState == GameState.Spin)
                win.text = 0.ToString();
            ClearSingleLineText();
        }
    }
}