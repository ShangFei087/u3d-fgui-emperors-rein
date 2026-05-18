using Sirenix.OdinInspector;
using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;
using Animation = UnityEngine.Animation;

/*
* == 动画：
* Spine（SkeletonGraphic / SkeletonAnimation / SkeletonMecanim）
* Animation
* Animator
* Tween
* Timer
* Cor
* GameObject
*/


/// <summary>Animator 侧动画控制的统一接口（由 <see cref="AnimAnimatorHelper"/> 实现）。</summary>
public interface IAnimUI
{
    /// <summary>恢复播放（全局 speed）。</summary>
    void play();
    /// <summary>暂停（全局 speed）。</summary>
    void Pause();
    /// <summary>按状态名播放；loop 对 Mecanim 由 Controller 决定。</summary>
    void Play(string animName, bool loop = false);
    /// <summary>跳到归一化时间后保持暂停。</summary>
    void Pause(string name, float normalizedTime);
}

/// <summary>
/// 多后端 UI 动画入口：按 <see cref="goAnim"/> 上实际组件依次选用
/// SkeletonGraphic → SkeletonAnimation → SkeletonMecanim → Animator → animList → Legacy Animation。
/// 子类可重写 <see cref="Play(string, bool)"/> / <see cref="OnValueChagne"/> 等扩展行为。
/// </summary>
public partial class AnimBaseUI : MonoBehaviour
{
    /// <summary>实际驱动动画的物体；未赋值时在 <see cref="Awake"/> 中默认为本组件所在物体。</summary>
    public GameObject goAnim;

    // 以下按 goAnim 上缓存的组件类型分流（每帧 GetComponent，勿在 Update 里频繁依赖）
    SkeletonGraphic skelGrap => goAnim ==null ? null: goAnim.GetComponent<SkeletonGraphic>();

    Animator animator => goAnim == null ? null : goAnim.GetComponent<Animator>();

    SkeletonAnimation skelAnim => goAnim == null ? null : goAnim.GetComponent<SkeletonAnimation>();

    /// <summary>Spine Mecanim：与 Animator 同体，由 Mecanim 状态机驱动骨骼。</summary>
    SkeletonMecanim skelMec => goAnim ==null ? null: goAnim.GetComponent<SkeletonMecanim>();

    Animation anim => goAnim == null ? null : goAnim.GetComponent<Animation>();

    /// <summary>最近一次按名称播放时记录的动画名（供暂停等接口使用）。</summary>
    string animName = null;
    /// <summary>与 <see cref="goAnim"/> 上 Animator 配套的辅助封装。</summary>
    AnimAnimatorHelper animatorHelper;

    private void Awake()
    {
        // 未指定则动画根节点即本物体
        if (goAnim == null)
            goAnim = gameObject;

        // 始终创建 Helper：无 Animator 时内部调用为空操作
        animatorHelper = new AnimAnimatorHelper(goAnim);
    }
    /// <summary>多子物体切换：用不同 GameObject 表示不同动画片段时，按名称显隐。</summary>
    /// <summary>无参播放：恢复 Animator 全局 speed（依赖 <see cref="AnimAnimatorHelper"/>）。</summary>
    public virtual void Play()
    {
        animatorHelper.play();
    }

    /// <summary>
    /// 按名称播放动画：根据 <see cref="goAnim"/> 上存在的组件类型走不同后端（优先级见类说明）。
    public virtual void Play(string animName, bool loop = false)
    {
        this.animName = animName;

        if (skelGrap != null)
        {
            // 1) UI Spine（SkeletonGraphic）：轨道 0 切换动画
            skelGrap.AnimationState.SetAnimation(0, animName, loop);
        }
        else if (skelAnim != null)
        {
            // 2) 场景 SkeletonAnimation：AnimationState 驱动
            skelAnim.AnimationState.SetAnimation(0, animName, loop);
        }
        else if (skelMec != null)
        {
            // 3) SkeletonMecanim：与 Animator 同体，按状态短名切入
            if (animator != null && animator.HasState(0, Animator.StringToHash(animName)))
            {
                animator.Play(animName, -1, 0f);
            }
        }
        else if (animator != null) 
        { 
            // 4) 纯 Mecanim（无 Spine 组件）
            if (animator.HasState(0, Animator.StringToHash(animName)))
            {
                animator.Play(animName);
            }
        }
        else if (anim != null)
        {
            // 6) Legacy Animation
            anim.Play(animName);
        }
    }

    /// <summary>
    /// 在「当前层当前片段」上按帧下标定位：可选先用 <paramref name="name"/> 切入状态，
    /// </summary>
    public void PlayFrame(string name, int frame)
    {
        if (animator == null || animatorHelper == null)
            return;

        if (!string.IsNullOrEmpty(name) && animator.HasState(0, Animator.StringToHash(name)))
        {
            // 先切入目标状态，再 Update(0) 以便本帧能取到正确 ClipInfo
            animatorHelper.Play(name, 0, 0f);
            animator.Update(0f);
        }

        // 当前层正在混合的片段信息（通常取 [0]）
        var infos = animatorHelper.GetCurrentAnimatorClipInfo(0);
        if (infos == null || infos.Length == 0 || infos[0].clip == null)
            return;

        var clip = infos[0].clip;
        float length = clip.length;
        float frameRate = clip.frameRate;
        if (length <= 0f || frameRate <= 0f)
            return;

        float totalFrame = length * frameRate;
        if (totalFrame <= 1f)
            return;

        int maxFrame = Mathf.Max(0, Mathf.FloorToInt(totalFrame) - 1);
        int clampedFrame = Mathf.Clamp(frame, 0, maxFrame);
        float normalizedFrameTime = clampedFrame / totalFrame;
        // 用 clip 名在层 0 定位到算出的归一化时间并恢复播放
        animatorHelper.Play(clip.name, 0, Mathf.Clamp01(normalizedFrameTime));
        animatorHelper.speed = 1f;
    }

    /// <summary>当前 Animator：将全局 speed 置 0 以暂停（不指定具体状态）。</summary>
    public void Pause()
    {
        if (animatorHelper != null)
        {
            // 全局暂停，不切换状态
            animatorHelper.speed = 0;
        }
    }

    /// <summary>将指定状态定位到归一化时间 <paramref name="normalizedTime"/> 后暂停（Animator 或 Legacy Animation）。</summary>
    public void Pause(string name, float normalizedTime)
    {
        this.animName = name;
        if (animatorHelper != null)
        {
            // 先跳到归一化时刻，再 speed=0 冻结
            animatorHelper.Play(animName, 0, normalizedTime);
            animatorHelper.speed = 0;
        }
        else if (anim != null)
        {
            anim.Play(animName);
            anim[animName].speed = 0f;
        }
    }

    /// <summary>
    /// 将动画停在接近结束的一帧：可指定 <paramref name="name"/>，否则取当前状态短名哈希，
    /// 在归一化时间 0.99 处 <see cref="AnimAnimatorHelper.speed"/> 置 0。
    /// </summary>
    public void PauseAtLast(string name = null)
    {
        if (animatorHelper != null)
        {
            if (name != null)
            {
                animatorHelper.Play(name, 0, 0.99f);
                animatorHelper.speed = 0;
            }
            else
            {
                AnimatorStateInfo currentState = animatorHelper.GetCurrentAnimatorStateInfo(0);

                int currentAnimHash = currentState.shortNameHash;

                animatorHelper.Play(currentAnimHash, 0, 0.99f);

                animatorHelper.speed = 0;
            }

        }

    }

    /// <summary>停止或回到默认：Spine 切到指定名且不循环；Mecanim 播对应状态；animList 全部隐藏。</summary>
    protected virtual void _AnimStop(string animName)
    {
        if (skelGrap != null)
        {
            skelGrap.AnimationState.SetAnimation(0, animName, false);
        }
        else if (skelAnim != null)
        {
            skelAnim.AnimationState.SetAnimation(0, animName, false);
        }
        else if (skelMec != null)
        {
            if (animator != null && animator.HasState(0, Animator.StringToHash(animName)))
            {
                animator.Play(animName, -1, 0f);
            }
        }
        else if (animator != null)
        {
            if (animator.HasState(0, Animator.StringToHash(animName)))
            {
                animator.Play(animName);
            }
        }
    }

    /// <summary> <paramref name="pre"/>（0~1）播放指定状态。</summary>
    public void PlayPre(string name, float pre)
    {
        if (pre < 0 || pre > 1)
            DebugUtils.LogError("pre must between 0 - 1");

        if (animatorHelper != null)
        {
            animatorHelper.Play(name, 0, pre);
        }
    }

    /// <summary>倒放：内部固定状态名 "Auto"，依赖 Animator 参数 mlp。</summary>
    public void PlayReverse()
    {
        animatorHelper.PlayReverse();

    }

    /// <summary>显式正播/倒放：状态名缺省为 "Auto"。</summary>
    public void PlayReverse(bool isReverse, string name = null)
    {
        if (animator == null)
            return;
        var stateName = string.IsNullOrEmpty(name) ? "Auto" : name;
        animatorHelper.Play(isReverse, stateName);
    }

}
