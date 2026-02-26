using FairyGUI;
using GameMaker;
using System;
using UnityEngine;


namespace PusherEmperorsRein
{


    public class PopupGameLoading : PageBase
    {
        public const string pkgName = "EmperorsRein";
        public const string resName = "PopupGameLoading";

        GTextField Load, version;
        GSlider ProgressBar;
        private float duration = 5f;

        protected override void OnInit()
        {
            
            base.OnInit();
        }



        public override void OnOpen(PageName name, EventData data)
        {


            DebugUtils.LogError($"启动游戏");

            base.OnOpen(name, data);
            InitParam();
        }

        public override void InitParam()
        {

            if (!isOpen) return;


            Load = this.contentPane.GetChild("load").asTextField;
            version = this.contentPane.GetChild("version").asTextField;
            version.text = GlobalData.hotfixVersion;
            ProgressBar = this.contentPane.GetChild("Slider").asSlider;
            if (PageManager.Instance.IndexOf(PageName.PusherEmperorsReinPopupERGameLoading) == 0)
            {
                 StartLoadingAnimation();
                 StartLoadingAnimation2();
            }



            /*
            try
            {
                // 正确的sorce：符号需在1-12范围内
                // 例如：第二条线符号=2，第三条线符号=3（第一条线不中奖）
                int sorce = 20002; // 解析后：第二条线=2，第三条线=3


                List<WinningLineInfo> winningLines = new List<WinningLineInfo> { new WinningLineInfo { LineNumber=1, SymbolNumber=1, WinCount=3 },
                                                                                 new WinningLineInfo { LineNumber=2, SymbolNumber=2, WinCount=3 },
                                                                                 new WinningLineInfo { LineNumber=3, SymbolNumber=3, WinCount=3 }};

                DebugUtils.LogError(AdvancedLineGenerator.Instance.GenerateGameArray(allLines, symbolNumber, 9, winningLines));
                //AdvancedLineGenerator.Instance.GenerateGameArray(allLines, symbolNumber,
                //    () =>
                //    {
                //        // 生成成功后，在回调中获取结果
                //        DebugUtils.Log("生成成功：" + AdvancedLineGenerator.Instance.strDeckRowCol);
                //    },
                //    (error) =>
                //    {
                //        // 打印错误信息
                //        DebugUtils.LogError("生成失败：" + error.msg);
                //    });

            }
            catch (Exception e)
            {
                DebugUtils.LogError($"调用出错：{e.Message}");
            }

    */

        }



        GTweener tweener = null;
        GTweener tweener2 = null;

        private void StartLoadingAnimation()
        {
            // 推币机-帝国之辉-预加载界面：
            PageManager.Instance.PreloadPage(PageName.PusherEmperorsReinPageERGameMain, null);
            PageManager.Instance.PreloadPage(PageName.PusherEmperorsReinPopupBigWin, null);
            PageManager.Instance.PreloadPage(PageName.PusherEmperorsReinPopupFreeSpinTrigger, null);
            PageManager.Instance.PreloadPage(PageName.PusherEmperorsReinPopupFreeSpinResult, null);
            PageManager.Instance.PreloadPage(PageName.PusherEmperorsReinPopupJackpotGame, null);




            //PageManager.Instance.PreloadPage(PageName.SlotEmperorsReinPageFreeBonusGame1, null);
            //PageManager.Instance.PreloadPage(PageName.SlotEmperorsReinPageERGameMain, null);



            // 使用GTween实现0到100的平滑过渡，时长2秒
            if (tweener != null) tweener.Kill();
            tweener = GTween.To(0, 100, duration)
                .SetEase(EaseType.Linear) // 线性过渡，匀速增长
                .OnUpdate((tween) =>
                {
                    // 获取当前进度值（四舍五入为整数）
                    int progress = Mathf.RoundToInt(tween.value.x);
                    // 更新文本显示
                    Load.text = $"加载中：{progress}/100";
                })
                .OnComplete(() =>
                {

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
                       PageManager.Instance.OpenPage(PageName.PusherEmperorsReinPageERGameMain);
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


    }

}
