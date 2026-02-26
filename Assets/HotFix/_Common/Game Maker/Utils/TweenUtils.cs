//using DG.Tweening;
using FairyGUI;
using System;

public static class TweenUtils { 

    public static GTweener DOLocalMoveY(GObject target, float yTo, float duration, EaseType type = EaseType.Linear, Action action = null)
    {
        // 这里可能换成Dotween
        GTweener tweener = GTween.To(target.y, yTo, duration)
            .SetEase(type)  // 设置缓动函数
            .OnUpdate((GTweener tweener) =>
            {
                // 每次更新时调用
                target.y = tweener.value.x;

                //DebugUtils.Log($"[Tween] yTo: {yTo}  cur:{tweener.value.x}");
            })
            .OnComplete(() =>
            {
                action?.Invoke();
                //DebugUtils.Log("Tween complete!");
            });
        return tweener;
    }


   /*
    public static Tweener DOLocalMoveY(GObject target, float yFrom, float yTo, float duration, Ease type = Ease.Linear, Action action = null)
    {
        target.y = yFrom;
        Tweener  tween = DOTween.To(
            () => yFrom,          // 起始值（Getter）
            d => target.y = d,    // 更新值（Setter）
            yTo,                  // 目标值
            duration              // 持续时间
        )
        .SetEase(type)           // 线性过渡（可改其他缓动函数）
        .OnUpdate(() => {
            // 每帧更新时调用（可在这里刷新 UI 或执行逻辑）

            // DebugUtils.Log("Current Value: " + target.y);                
        })
        .OnComplete(() => {
            // 动画完成时调用
            action?.Invoke(); // 执行回调（如果有）
        });
        return tween;
    }
   */
}
