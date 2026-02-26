using FairyGUI;
using GameMaker;
using System;

using UnityEngine;


namespace SlotZhuZaiJinBi1700
{
    public class PopupGameLoading : MachinePageBase
    {
        public new const string pkgName = "SlotZhuZaiJinBi1700";
        public new const string resName = "PopupGameLoading";

        //预制体
        private GameObject goLoading_bg, goLoading_Title;
        private GameObject CloneLoading_bg, CloneLoading_Title;
        //UI组件
        private GComponent anchorBG, anchorTitle;
        private GProgressBar ProgressBar;
        private float duration = 5f;
        //数据
         bool isInit = false;
        EventData _data = null;
        GTweener tweener = null;
        private Animator animtorLoading_Title;
        protected override void OnInit()
        {
            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            int count = 2;

            Action loadComplete = () =>
            {
                if (--count == 0)
                {
                    isInit = true;
                    InitParam(null);
                }
            };

            //加载预制体
            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/PopupGameLoading/Loading_bg",
            (GameObject clone) =>
            {
                goLoading_bg = clone;
                loadComplete();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/PopupGameLoading/Loading_Title",
                (GameObject clone) =>
                {
                    goLoading_Title = clone;
                    loadComplete();
                });
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
            //加载组件
          
            // 初始化UI锚点
            GComponent LocalAnchorLoadingBG = contentPane.GetChild("anchorLoadingBG").asCom;
            if (anchorBG != LocalAnchorLoadingBG)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorBG);
                CloneLoading_bg = GameObject.Instantiate(goLoading_bg);
                anchorBG = LocalAnchorLoadingBG;
                GameCommon.FguiUtils.AddWrapper(anchorBG, CloneLoading_bg);
            }

            GComponent LocalAnchorLoadingTitle = contentPane.GetChild("anchorLoadingTitle").asCom;
            if (anchorTitle != LocalAnchorLoadingTitle)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorTitle);
                CloneLoading_Title = GameObject.Instantiate(goLoading_Title);
                animtorLoading_Title= CloneLoading_Title.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                animtorLoading_Title.enabled = false;
                anchorTitle = LocalAnchorLoadingTitle;
                GameCommon.FguiUtils.AddWrapper(anchorTitle, CloneLoading_Title);
            }

            ProgressBar = contentPane.GetChild("Slider").asProgress;
            StartLoadingAnimation();
        }
        public void StartLoadingAnimation()
        {
            //预加载
            PageManager.Instance.PreloadPage(PageName.SlotZhuZaiJinBiPageGameMain, null);
            animtorLoading_Title.enabled = true;
            if (tweener != null) tweener.Kill();
            tweener = GTween.To(0, 100, duration)
            .SetEase(EaseType.Linear) // 线性过渡，匀速增长
            .OnUpdate((tween) =>
            {
                double progress = tween.value.x;
                ProgressBar.value = progress;
            })
            .OnComplete(() =>
            {
                CloseSelf(null);
                PageManager.Instance.OpenPage(PageName.SlotZhuZaiJinBiPageGameMain, null);
            });
        }
    }
}

