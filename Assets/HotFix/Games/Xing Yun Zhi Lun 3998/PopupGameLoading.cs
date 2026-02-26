using FairyGUI;
using GameMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;


namespace XingYunZhiLun_3998
{
    public class PopupGameLoading : MachinePageBase
    {
        public new const string pkgName = "XingYunZhiLun_3998";
        public new const string resName = "PopupGameLoading";

        //预制体
        private GameObject go_clone, loadTextGameObject;

        //FGUI组件
        private GSlider progressBar;
        private GTextField loadText;
        private GComponent anchorLoadText, gOwnerPanel;


        private bool isInit = false;
        private float duration = 3.0f;       //持续时间
        
        private Animator animator = null;

        private GTweener tweener = null;
        private GTweener tweener2 = null;

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
                    this.InitParam(null);
                }
            };

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/PopupGameLoading/Loading_Title",
            (GameObject clone) =>
            {
                go_clone = clone;
                callback();
            });
        }

        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            this.InitParam(data);
        }

        public void InitParam(EventData data)
        {
            if (!isInit) return;

            //loadText = contentPane.GetChild("load").asTextField;
            progressBar = contentPane.GetChild("n11").asSlider;

            //初始化UI锚点
            GComponent LocalAnchorLoadingText = contentPane.GetChild("n1").asCom;
            if (anchorLoadText != LocalAnchorLoadingText)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorLoadText);
                loadTextGameObject = GameObject.Instantiate(go_clone);
                anchorLoadText = LocalAnchorLoadingText;
                GameCommon.FguiUtils.AddWrapper(anchorLoadText, loadTextGameObject);
            }


            if (PageManager.Instance.IndexOf(PageName.XingYunZhiLunPopupGameLoading) == 0)
            {
                //StartLoadingAnimation();
                StartLoadingAnimation2();
            }
        }

        //private void StartLoadingAnimation()
        //{
        //    // 预加载界面：
        //    PageManager.Instance.PreloadPage(PageName.XingYunZhiLunPageGameMain, null);

        //    if (tweener != null) tweener.Kill();
        //    tweener = GTween.To(0, 100, duration)
        //        .SetEase(EaseType.Linear) // 线性过渡，匀速增长
        //        .OnUpdate((tween) =>
        //        {
        //            // 获取当前进度值（四舍五入为整数）
        //            int progress = Mathf.RoundToInt(tween.value.x);
        //            // 更新文本显示
        //            loadText.text = $"{progress} %";
        //        })
        //        .OnComplete(() =>
        //        {
        //            CloseSelf(null);

        //            //执行完毕之后销毁tweener防止多次调用OnComplete
        //            if (tweener != null)
        //            {
        //                tweener.Kill();
        //                tweener = null;
        //            }

        //        
        //        });

        //}

        private void StartLoadingAnimation2()
        {
            // 预加载界面：
            PageManager.Instance.PreloadPage(PageName.XingYunZhiLunPageGameMain, null);
            PageManager.Instance.PreloadPage(PageName.XingYunZhiLunPopupJackpotGameTrigger, null); 
            PageManager.Instance.PreloadPage(PageName.XingYunZhiLunPopupJackpotGameResult, null);
            PageManager.Instance.PreloadPage(PageName.XingYunZhiLunPopupJackpotGameExit, null);
            PageManager.Instance.PreloadPage(PageName.XingYunZhiLunPopupJackpotGameQuit, null);
            PageManager.Instance.PreloadPage(PageName.XingYunZhiLunPopupJackpotGameEnter, null);
            PageManager.Instance.PreloadPage(PageName.XingYunZhiLunPopupFreeSpinTrigger, null);
            PageManager.Instance.PreloadPage(PageName.XingYunZhiLunPopupFreeSpinResult, null);

            if (tweener2 != null) tweener2.Kill();
            tweener2 = GTween.To(0, 100, duration)
                .SetEase(EaseType.Linear) // 线性过渡，匀速增长
                .OnUpdate((tween) =>
                {
                    // 获取当前进度值（四舍五入为整数）
                    double progressValue = tween.value.x;
                    progressBar.value = progressValue;
                })
                .OnComplete(() =>
                {
                    CloseSelf(null);

                    //打开主界面
                    PageManager.Instance.OpenPage(PageName.XingYunZhiLunPageGameMain, null);
                });
        }

        

    }         
}
