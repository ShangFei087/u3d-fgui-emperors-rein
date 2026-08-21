using FairyGUI;
using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 可 <c>new</c> 的 Spine / Animator 播放器，无需往物体上挂脚本。
/// 构造时在自身及子节点缓存组件，播放按优先级分流：
/// <see cref="SkeletonGraphic"/> → <see cref="SkeletonAnimation"/> → <see cref="SkeletonMecanim"/> / <see cref="Animator"/>。
/// 同时支持把 FGUI 节点挂到 Spine 骨骼，关闭时 <see cref="DetachAll"/> 还原。
/// </summary>
public class AnimPlayer
{
    /// <summary>FGUI 像素相对 Spine 世界单位的默认缩放。</summary>
    const float DefaultUiScale = 0.01f;

    /// <summary>一次挂点记录：用于关闭时把 UI 还原到原父节点和变换。</summary>
    class BoneBind
    {
        public Transform uiTran;
        public Transform originParent;
        public Vector3 pos;
        public Vector3 scale;
        public Quaternion rot;
    }

    readonly GameObject _go;
    readonly SkeletonGraphic _skelGrap;
    readonly SkeletonAnimation _skelAnim;
    readonly SkeletonMecanim _skelMec;
    readonly Animator _animator;
    readonly List<BoneBind> _binds = new List<BoneBind>();

    /// <summary>Mecanim 排队播下一段时的定时器；Spine 走轨道排队，不使用此项。</summary>
    TimerCallback _sequenceCallback;

    /// <param name="go">Spine 预制体根或带 Animator 的节点；组件也可在子节点上。</param>
    public AnimPlayer(GameObject go)
    {
        _go = go;
        if (go == null) return;

        _skelGrap = go.GetComponent<SkeletonGraphic>() ?? go.GetComponentInChildren<SkeletonGraphic>(true);
        _skelAnim = go.GetComponent<SkeletonAnimation>() ?? go.GetComponentInChildren<SkeletonAnimation>(true);
        _skelMec = go.transform.GetChild(0).GetChild(0).GetComponent<SkeletonMecanim>();
        //_skelMec = go.GetComponent<SkeletonMecanim>() ?? go.GetComponentInChildren<SkeletonMecanim>(true);
        _animator= go.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
        //_animator = go.GetComponent<Animator>() ?? go.GetComponentInChildren<Animator>(true);
    }

    /// <summary>构造时传入的根物体。</summary>
    public GameObject Owner => _go;

    /// <summary>缓存的 Animator（Mecanim / SkeletonMecanim 共用）。</summary>
    public Animator Animator => _animator;

    /// <summary>
    /// 按名称播放动画。会取消尚未执行的 <see cref="PlayThen"/> 排队。
    /// Spine 的 <paramref name="loop"/> 生效；Mecanim 是否循环由 Controller 该状态决定。
    /// <paramref name="normalizedTime"/> 为 0~1 相位，用于多实例对齐。
    /// </summary>
    public void Play(string animName, bool loop = false, float normalizedTime = 0f)
    {
        RemoveSequenceTimer();
        if (string.IsNullOrEmpty(animName)) return;

        if (_skelGrap != null)
        {
            ApplySpineNormalizedTime(_skelGrap.AnimationState.SetAnimation(0, animName, loop), normalizedTime);
            return;
        }
        if (_skelAnim != null)
        {
            ApplySpineNormalizedTime(_skelAnim.AnimationState.SetAnimation(0, animName, loop), normalizedTime);
            return;
        }
        if (_skelMec != null || _animator != null)
        {
            if (_animator == null) return;
            if (!_animator.HasState(0, Animator.StringToHash(animName))) return;
            _animator.Play(animName, -1, normalizedTime);
            if (normalizedTime > 0f)
                _animator.Update(0f);
        }
    }

    static void ApplySpineNormalizedTime(Spine.TrackEntry entry, float normalizedTime)
    {
        if (entry?.Animation == null || entry.Animation.Duration <= 0f)
            return;
        entry.TrackTime = normalizedTime * entry.Animation.Duration;
    }

    /// <summary>
    /// 先播 <paramref name="first"/>（不循环），结束后立刻播 <paramref name="next"/>。
    /// <paramref name="loop"/> 只作用于第二段。
    /// Spine 用轨道 <c>AddAnimation</c>；Mecanim 按第一段时长用 Timer 切换。
    /// </summary>
    public void PlayThen(string first, string next, bool loop = false)
    {
        RemoveSequenceTimer();
        if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(next)) return;

        if (_skelGrap != null)
        {
            PlaySpineThen(_skelGrap.AnimationState, first, next, loop);
            return;
        }
        if (_skelAnim != null)
        {
            PlaySpineThen(_skelAnim.AnimationState, first, next, loop);
            return;
        }
        if (_animator == null) return;
        if (!_animator.HasState(0, Animator.StringToHash(first))) return;

        _animator.Play(first, -1, 0f);
        _animator.Update(0f);
        float duration = _animator.GetCurrentAnimatorStateInfo(0).length;
        if (_animator.speed > 0.0001f)
            duration /= _animator.speed;
        if (duration <= 0f)
        {
            Play(next, loop);
            return;
        }

        _sequenceCallback = obj =>
        {
            _sequenceCallback = null;
            Play(next, loop);
        };
        Timers.inst.Add(duration, 1, _sequenceCallback);
    }

    /// <summary>Spine 轨道：当前段播完后无延迟接上一段。</summary>
    static void PlaySpineThen(Spine.AnimationState state, string first, string next, bool loop)
    {
        state.SetAnimation(0, first, false);
        state.AddAnimation(0, next, loop, 0f);
    }

    /// <summary>取消 Mecanim 的下一段定时，避免关页或改播后仍切到排队动画。</summary>
    void RemoveSequenceTimer()
    {
        if (_sequenceCallback == null) return;
        Timers.inst.Remove(_sequenceCallback);
        _sequenceCallback = null;
    }

    /// <summary>暂停 Mecanim（speed = 0）。Spine AnimationState 不受影响。</summary>
    public void Pause()
    {
        if (_animator != null)
            _animator.speed = 0f;
    }

    /// <summary>恢复 Mecanim（speed = 1）。</summary>
    public void Resume()
    {
        if (_animator != null)
            _animator.speed = 1f;
    }

    /// <summary>
    /// 把 FGUI 节点挂到 Spine 骨骼下，并记下还原数据。
    /// 同一 UI 再次 Attach 只换父节点，不覆盖第一次记录的原父节点与变换。
    /// </summary>
    /// <param name="ui">FGUI 按钮、文本等。</param>
    /// <param name="bonePath">相对构造根物体的骨骼路径，可用 <c>Transform.Find</c> 找到。</param>
    /// <param name="localPos">挂上后的本地坐标；默认 <see cref="Vector3.zero"/>。</param>
    /// <param name="localScale">挂上后的本地缩放；默认 0.01。</param>
    /// <param name="localRot">挂上后的本地旋转；默认单位四元数。</param>
    public bool Attach(GObject ui, string bonePath, Vector3? localPos = null, Vector3? localScale = null, Quaternion? localRot = null)
    {
        if (ui?.displayObject == null || _go == null || string.IsNullOrEmpty(bonePath))
            return false;

        Transform bone = _go.transform.Find(bonePath);
        if (bone == null)
        {
            Debug.LogWarning($"AnimPlayer: bone not found: {bonePath}");
            return false;
        }

        Transform uiTran = ui.displayObject.gameObject.transform;
        for (int i = 0; i < _binds.Count; i++)
        {
            if (_binds[i].uiTran == uiTran)
                return Reparent(_binds[i], bone, localPos, localScale, localRot);
        }

        var bind = new BoneBind
        {
            uiTran = uiTran,
            originParent = uiTran.parent,
            pos = uiTran.localPosition,
            scale = uiTran.localScale,
            rot = uiTran.localRotation,
        };
        _binds.Add(bind);
        return Reparent(bind, bone, localPos, localScale, localRot);
    }

    /// <summary>还原所有挂点，并取消尚未执行的排队动画。关页时必须调用。</summary>
    public void DetachAll()
    {
        RemoveSequenceTimer();
        for (int i = 0; i < _binds.Count; i++)
        {
            BoneBind b = _binds[i];
            if (b.uiTran == null) continue;
            b.uiTran.SetParent(b.originParent, false);
            b.uiTran.localPosition = b.pos;
            b.uiTran.localScale = b.scale;
            b.uiTran.localRotation = b.rot;
        }
        _binds.Clear();
    }

    /// <summary>把 UI 设为骨骼子节点，并写入本地变换。</summary>
    static bool Reparent(BoneBind bind, Transform bone, Vector3? localPos, Vector3? localScale, Quaternion? localRot)
    {
        bind.uiTran.SetParent(bone, false);
        bind.uiTran.localPosition = localPos ?? Vector3.zero;
        bind.uiTran.localScale = localScale ?? Vector3.one * DefaultUiScale;
        bind.uiTran.localRotation = localRot ?? Quaternion.identity;
        return true;
    }
}
