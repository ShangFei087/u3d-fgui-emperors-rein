using FairyGUI;
using GameMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace CaiFuHuoChe_3996
{
    public class PopupGameLoading : PageBase
    {
        public const string pkgName = "CaiFuHuoChe_3996";
        public const string resName = "PopupGameLoading";

        GTextField Load, version;
        GSlider ProgressBar;
        private float duration = 5f;
        private string[] dots = new string[]
        {
            "",
            ".",
            "..",
            "..."
        };

        //预制体
        private GameObject go_clone, loadTitleGameObject, loadBarEffect, go_loadBar;
        private GComponent anchorLoadText, anchorEffect;
        private new bool isInit = false;
        private Animator animator;

        protected override void OnInit()
        {
            base.OnInit();

            int count = 2;
            Action callback = () =>
            {
                if(--count <= 0)
                {
                    isInit = true;
                    InitParam();
                }
            };

            ResourceManager02.Instance.LoadAsset<GameObject>(
            "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/PopupGameLoading/Loading_Title",
            (GameObject clone) =>
            {
                go_clone = clone;
                callback();
            });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Cai Fu Huo Che 3996/Prefabs/PopupGameLoading/Loading_Bar.prefab",
                (GameObject clone) =>
                {
                    go_loadBar = clone;
                    callback();
                });
        }



        public override void OnOpen(PageName name, EventData data)
        {
            DebugUtils.LogError($"启动游戏");

            base.OnOpen(name, data);
            InitParam();
        }

        public override void InitParam()
        {
            if (!isInit) return;

            Load = this.contentPane.GetChild("load").asTextField;
            //version = this.contentPane.GetChild("version").asTextField;
            //version.text = GlobalData.hotfixVersion;
            ProgressBar = this.contentPane.GetChild("Slider").asSlider;

            //初始化UI锚点
            GComponent LocalAnchorLoadingText = contentPane.GetChild("title").asCom;
            if (anchorLoadText != LocalAnchorLoadingText)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorLoadText);
                loadTitleGameObject = GameObject.Instantiate(go_clone);
                animator = loadTitleGameObject.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                anchorLoadText = LocalAnchorLoadingText;
                GameCommon.FguiUtils.AddWrapper(anchorLoadText, loadTitleGameObject);
            }

            GComponent localAnchorEffect = contentPane.GetChild("Slider").asSlider.GetChild("anchorEffect").asCom;
            if (anchorEffect != localAnchorEffect)
            {
                GameCommon.FguiUtils.DeleteWrapper(anchorEffect);
                anchorEffect = localAnchorEffect;
                loadBarEffect = GameObject.Instantiate(go_loadBar);
                GameCommon.FguiUtils.AddWrapper(anchorEffect, loadBarEffect);
            }


            if (PageManager.Instance.IndexOf(PageName.CaiFuHuoChePopupGameLoading) == 0)
            {
                StartLoadingAnimation();
                StartLoadingAnimation2();

                PlayAnim("start");
            }
        }



        GTweener tweener = null;
        GTweener tweener2 = null;

        private void StartLoadingAnimation()
        {
            // 预加载界面：
            PageManager.Instance.PreloadPage(PageName.CaiFuHuoChePageGameMain, null);
            //PageManager.Instance.PreloadPage(PageName.PusherEmperorsReinPopupBigWin, null);
            //PageManager.Instance.PreloadPage(PageName.PusherEmperorsReinPopupFreeSpinTrigger, null);
            //PageManager.Instance.PreloadPage(PageName.PusherEmperorsReinPopupFreeSpinResult, null);
            //PageManager.Instance.PreloadPage(PageName.PusherEmperorsReinPopupJackpotGame, null);



            // 使用GTween实现0到100的平滑过渡，时长2秒
            if (tweener != null) tweener.Kill();
            tweener = GTween.To(0, 100, duration)
                .SetEase(EaseType.Linear) // 线性过渡，匀速增长
                .OnUpdate((tween) =>
                {
                    // 获取当前进度值（四舍五入为整数）
                    int progress = Mathf.RoundToInt(tween.value.x);
                    // 更新文本显示
                    Load.text = $"加载中{dots[(progress / 4) % 4]}";
                })
                .OnComplete(() =>
                {
                    Load.text = $"加载完成";
                    CloseSelf(null);


                    Action onJPPoolSubCredit = () =>
                    {
                        DebugUtils.Log("i am here123");
                    };



                    if (PlayerPrefsUtils.isPauseAtPopupGameLoadingOnce)
                    {
                        PlayerPrefsUtils.isPauseAtPopupGameLoadingOnce = false;
                    }
                    else
                    {
                        PageManager.Instance.OpenPage(PageName.CaiFuHuoChePageGameMain);
                    }

                });
        }

        private void StartLoadingAnimation2()
        {


            if (tweener2 != null) tweener2.Kill();
            tweener2 = GTween.To(0, 1, duration)
                .SetEase(EaseType.Linear) // 线性过渡，匀速增长
                .OnUpdate((tween) =>
                {
                    // 获取当前进度值（四舍五入为整数）
                    double progress = tween.value.x;
                    ProgressBar.value = progress;
                })
                .OnComplete(() =>
                {
                    CloseSelf(null);
                });
        }

        //播放指定动画
        private void PlayAnim(string animName)
        {
            animator.Rebind();
            animator.Play(animName);
            animator.Update(0f);
        }
    }
}
