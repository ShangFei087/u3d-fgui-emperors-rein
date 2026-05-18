using UnityEngine;

/// <summary>
/// 封装 <see cref="Animator"/> 的常用操作（播放、暂停、按归一化时间定位、配合 Spine Mecanim 的 mlp 倒放等）。无 Animator 时各方法安全空操作。
/// </summary>
public class AnimAnimatorHelper : IAnimUI
{
    private GameObject goOwner;

    /// <param name="go">挂载 <see cref="Animator"/> 的物体（可与 SkeletonMecanim 挂在同一物体上）</param>
    public AnimAnimatorHelper(GameObject go)
    {
        goOwner = go;
    }

    private Animator animator => goOwner == null ? null : goOwner.GetComponent<Animator>();

    /// <summary>Animator 全局播放倍率；设为 0 即等价暂停。</summary>
    public float speed
    {
        get => animator == null ? 0f : animator.speed;
        set
        {
            if (animator != null)
                animator.speed = value;
        }
    }

    /// <summary>恢复播放：将 Animator.speed 置为 1（与 <see cref="Pause"/> 配对）。</summary>
    public void play()
    {
        if(animator ==null) return;
        animator.speed = 1f;
    }

    /// <summary>暂停：将 Animator.speed 置为 0。</summary>
    public void Pause()
    {
        if(animator ==null) return;
        animator.speed = 0f;
    }

    /// <summary>按状态名切入动画；是否循环由 Controller 中该状态配置决定。</summary>
    public void Play(string animName, bool loop = false)
    {
        if (animator == null || string.IsNullOrEmpty(animName))
            return;
        if (!animator.HasState(0, Animator.StringToHash(animName)))
            return;
        // loop 由 Animator Controller 里该状态的循环设置决定；此处仅负责切入状态
        animator.Play(animName, -1, 0f);
        _ = loop;
    }

    /// <summary>在指定时间 <paramref name="time"/> 开始播放状态 <paramref name="animName"/>。</summary>
    public void Play(string animName, int layer, float time)
    {
        if (animator == null) return;
        // 在指定层从归一化时间 time 播放状态 animName
        animator.Play(animName, layer, time);
    }

    /// <summary>使用短名哈希在指定层、归一化时间点播放状态。</summary>
    public void Play(int Hash, int layer, float time)
    {
        if (animator == null) return;
        // 使用状态短名哈希定位，避免字符串查找
        animator.Play(Hash, layer, time);
    }

    /// <summary>先跳到指定状态时间，再暂停（speed=0）。</summary>
    public void Pause(string name, float normalizedTime)
    {
        if (animator == null || string.IsNullOrEmpty(name))
            return;
        if (!animator.HasState(0, Animator.StringToHash(name)))
            return;
        animator.Play(name, -1, normalizedTime);
        animator.speed = 0f;
    }

    /// <summary>指定层是否存在该短名状态。</summary>
    public bool HasState(int index, string name)
    {
        if (animator == null || string.IsNullOrEmpty(name))
            return false;
        return animator.HasState(index, Animator.StringToHash(name));
    }

    /// <summary>当前层正在播放的片段信息（无 Animator 时返回空数组）。</summary>
    public AnimatorClipInfo[] GetCurrentAnimatorClipInfo(int layer)
    {
        if (animator == null)
            return System.Array.Empty<AnimatorClipInfo>();
        return animator.GetCurrentAnimatorClipInfo(layer);
    }

    /// <summary>当前层的状态信息（时长、归一化时间、短名哈希等）。</summary>
    public AnimatorStateInfo GetCurrentAnimatorStateInfo(int layer)
    {
        if (animator == null)
            return default;
        return animator.GetCurrentAnimatorStateInfo(layer);
    }

    /// <summary>写入 Animator 参数（需 Controller 中存在同名 float 参数）。</summary>
    public void SetFloat(string name,float value)
    {
        if (animator == null)
            return;
        animator.SetFloat(name, value);
    }

    /// <summary>倒放：播放状态名 "Auto"，内部通过 mlp=-1 与归一化起点 1 实现。</summary>
    public void PlayReverse()
    {
        Play(true, "Auto");
    }

    /// <summary>
    /// 正播/倒播：依赖 Controller 中 float 参数 <c>mlp</c>（正 1 / 负 -1）配合归一化时间 0 或 1。
    /// </summary>
    /// <param name="isReverse">true 为倒播，false 为正播</param>
    /// <param name="name">状态短名；为空时使用 "Auto"</param>
    public void Play(bool isReverse, string name = null)
    {
            if (animator == null) return;
            if (string.IsNullOrEmpty(name))
                name = "Auto";

            // Animator 中需存在 float 参数 mlp：正播为 1，倒播为 -1（与 Spine Mecanim 示例一致）
            if (!isReverse)
            {
                    // 正播：从归一化时间 0 起播
                    animator.speed = 1f;
                    animator.SetFloat("mlp", 1f);
                    animator.Play(name, -1, 0f);
            }
            else
            {
                // 倒播：从归一化时间 1 起播，mlp 为负
                animator.speed = 1f;
                animator.SetFloat("mlp", -1f);
                animator.Play(name, -1, 1f);
            }
        
    }
}
