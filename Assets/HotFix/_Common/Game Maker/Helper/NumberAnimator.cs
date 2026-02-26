using PusherEmperorsRein;
using FairyGUI;
using GameMaker;
using System.Collections.Generic;
using UnityEngine;

public class NumberAnimation : Singleton<NumberAnimation>
{
    // 存储所有运行中的动画实例（包含关联的文本组件）
    private List<AnimationInstance> _allTweeners = new List<AnimationInstance>();

    // 默认动画总时长
    private const float DefaultDuration = 1f;
    // 默认缓动类型
    private const EaseType DefaultEaseType = EaseType.Linear;

    /// <summary>
    /// 动画实例类：关联动画、文本组件和类型
    /// </summary>
    private class AnimationInstance
    {
        public GTweener tweener;          // 动画实例
        public GTextField fguiText;       // FGUI文本组件（可为null）
        public TextMesh textMesh;         // Unity文本组件（可为null）
        public System.Action onComplete;  // 动画完成回调
    }

    /// <summary>
    /// 为FGUI文本组件创建数字动画（带完成回调）
    /// </summary>
    public void AnimateNumber(GTextField textField, float start, float end,
                             float duration = DefaultDuration,
                             EaseType easeType = DefaultEaseType,
                             System.Action onComplete = null)
    {
        if (textField == null)
        {
            DebugUtils.LogError("FGUI文本组件不能为空！");
            return;
        }

        if (duration <= 0)
        {
            DebugUtils.LogWarning("动画时长必须大于0，已使用默认时长");
            duration = DefaultDuration;
        }

        // 立即显示起始值
        textField.text = Mathf.RoundToInt(start).ToString();

        // 起始值等于目标值，直接执行回调并返回
        if (Mathf.Approximately(start, end))
        {
            onComplete?.Invoke();
            return;
        }

        // 创建动画实例并存储
        var instance = new AnimationInstance
        {
            fguiText = textField,
            textMesh = null,
            onComplete = onComplete  // 保存回调
        };

        // 创建数字动画
        instance.tweener = GTween.To(start, end, duration)
            .SetEase(easeType)
            .OnUpdate(tweener => OnTweenUpdate(instance, tweener))
            .OnComplete(tweener => OnTweenComplete(instance));

        _allTweeners.Add(instance);
    }

    /// <summary>
    /// 为Unity TextMesh创建数字动画（带完成回调）
    /// </summary>
    public void AnimateNumber(TextMesh text, float start, float end,
                            float duration = DefaultDuration,
                            EaseType easeType = DefaultEaseType,
                            System.Action onComplete = null)
    {
        if (text == null)
        {
            DebugUtils.LogError("TextMesh组件不能为空！");
            return;
        }

        if (duration <= 0)
        {
            DebugUtils.LogWarning("动画时长必须大于0，已使用默认时长");
            duration = DefaultDuration;
        }

        // 立即显示起始值
        text.text = Mathf.RoundToInt(start).ToString();

        // 起始值等于目标值，直接执行回调并返回
        if (Mathf.Approximately(start, end))
        {
            onComplete?.Invoke();
            return;
        }

        // 创建动画实例并存储
        var instance = new AnimationInstance
        {
            fguiText = null,
            textMesh = text,
            onComplete = onComplete  // 保存回调
        };

        // 创建数字动画
        instance.tweener = GTween.To(start, end, duration)
            .SetEase(easeType)
            .OnUpdate(tweener => OnTweenUpdate(instance, tweener))
            .OnComplete(tweener => OnTweenComplete(instance));

        _allTweeners.Add(instance);
    }

    /// <summary>
    /// 动画更新回调
    /// </summary>
    private void OnTweenUpdate(AnimationInstance instance, GTweener tweener)
    {
        float currentValue = tweener.value.x;
        int roundedValue = Mathf.RoundToInt(currentValue);

        // 更新对应文本组件
        if (instance.fguiText != null)
            instance.fguiText.text = roundedValue.ToString();

        if (instance.textMesh != null)
            instance.textMesh.text = roundedValue.ToString();
    }

    /// <summary>
    /// 动画完成回调（从列表中移除已完成的动画并执行回调）
    /// </summary>
    private void OnTweenComplete(AnimationInstance instance)
    {
        float endValue = instance.tweener.endValue.x;
        int roundedValue = Mathf.RoundToInt(endValue);

        // 确保显示最终值
        if (instance.fguiText != null)
            instance.fguiText.text = roundedValue.ToString();

        if (instance.textMesh != null)
            instance.textMesh.text = roundedValue.ToString();

        // 执行完成回调
        instance.onComplete?.Invoke();

        // 从列表中移除
        _allTweeners.Remove(instance);
    }

    /// <summary>
    /// 暂停所有动画
    /// </summary>
    public void PauseAllAnimations()
    {
        foreach (var instance in _allTweeners)
        {
            instance.tweener?.SetPaused(true);
        }
    }

    /// <summary>
    /// 恢复所有动画
    /// </summary>
    public void ResumeAllAnimations()
    {
        foreach (var instance in _allTweeners)
        {
            instance.tweener?.SetPaused(false);
        }
    }

    /// <summary>
    /// 停止并清除所有动画（直接全部删除）
    /// </summary>
    public void StopAllAnimations()
    {
        foreach (var instance in _allTweeners)
        {
            instance.tweener?.Kill();
            instance.onComplete?.Invoke();
        }
        _allTweeners.Clear();
    }

    /// <summary>
    /// 强制所有动画显示最终值并停止
    /// </summary>
    public void ForceSetAllTargetValues()
    {
        foreach (var instance in _allTweeners)
        {
            if (instance.tweener == null) continue;

            float endValue = instance.tweener.endValue.x;
            int roundedValue = Mathf.RoundToInt(endValue);

            if (instance.fguiText != null)
                instance.fguiText.text = roundedValue.ToString();

            if (instance.textMesh != null)
                instance.textMesh.text = roundedValue.ToString();

            // 执行完成回调
            instance.onComplete?.Invoke();

            instance.tweener.Kill();
        }
        _allTweeners.Clear();
    }

    /// <summary>
    /// 暂停指定GTextField的动画
    /// </summary>
    /// <param name="textField">要暂停动画的文本组件</param>
    /// <returns>如果找到并暂停了动画则返回true，否则返回false</returns>
    public bool PauseTextFieldAnimation(GTextField textField)
    {
        if (textField == null)
        {
            DebugUtils.LogError("要暂停动画的GTextField不能为空！");
            return false;
        }

        // 查找该文本组件对应的动画实例
        var targetInstance = _allTweeners.Find(
            instance => instance.fguiText == textField && instance.tweener != null
        );

        if (targetInstance != null)
        {
            targetInstance.tweener.SetPaused(true);
            return true;
        }

        DebugUtils.LogWarning("未找到该GTextField对应的动画实例");
        return false;
    }

    private void OnDestroy()
    {
        StopAllAnimations();
    }
}
