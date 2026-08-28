using FairyGUI;
using GameMaker;
using GameUtil;
using SlotMaker;
using UnityEngine;
using UnityEngine.Events;

public class SpinButtonBaseController // : IContorller
{
    public GComponent goOwnerSpin;
    protected GameObject goSpin;
    protected Animator animator;
    protected float startTimeS = 0;
    protected int playTotalSpins = 0;
    public UnityEvent<bool> onClickCallblack = new UnityEvent<bool>();

    /// <summary> 短按特效锚点（FGUI ButtonSpin/anchorShortSpin），旧面板没有该节点时为 null。 </summary>
    protected GComponent _anchorShortSpin;
    /// <summary> 长按特效锚点（FGUI ButtonSpin/anchorLongSpin），旧面板没有该节点时为 null。 </summary>
    protected GComponent _anchorLongSpin;
    /// <summary> 短按粒子实例，一次性播放。 </summary>
    protected GameObject _goShortSpin;
    /// <summary> 长按粒子实例，循环播放，松手后必须关闭。 </summary>
    protected GameObject _goLongSpin;

    public virtual void InitParam(GComponent spin, string state, UnityAction<bool> onClick)
    {
        goOwnerSpin = spin;
        onClickCallblack.RemoveAllListeners();
        onClickCallblack.AddListener(onClick);
        State = state;
        BindTouch(false);
    }

    public virtual void InitParam(GComponent spin, string state, UnityAction<bool> onClick, GameObject gameObject,
        GameObject shortSpinPrefab = null, GameObject longSpinPrefab = null)
    {
        goOwnerSpin = spin;
        onClickCallblack.RemoveAllListeners();
        onClickCallblack.AddListener(onClick);

        // 默认 Spin 装饰特效仍挂到 anchorSpin；短按/长按特效挂到独立锚点。
        goSpin = WrapToAnchor(goOwnerSpin?.GetChild("anchorSpin")?.asCom, gameObject);
        _anchorShortSpin = goOwnerSpin?.GetChild("anchorShortSpin")?.asCom;
        _anchorLongSpin = goOwnerSpin?.GetChild("anchorLongSpin")?.asCom;
        _goShortSpin = WrapToAnchor(_anchorShortSpin, shortSpinPrefab);
        _goLongSpin = WrapToAnchor(_anchorLongSpin, longSpinPrefab);

        // 预制体多为 playOnAwake，进入游戏时必须先关掉，只在按下时再播。
        HideAllPressEffects();

        State = state;
        BindTouch(true);
    }

    /// <summary> 绑定屏幕 Spin 按钮的按下 / 抬起。 </summary>
    protected virtual void BindTouch(bool playClickSound)
    {
        if (goOwnerSpin == null)
            return;

        goOwnerSpin.onTouchBegin.Clear();
        goOwnerSpin.onTouchBegin.Add(OnPressBegin);
        goOwnerSpin.onTouchEnd.Clear();
        goOwnerSpin.onTouchEnd.Add(() => OnOwnerTouchEnd(playClickSound));
    }

    /// <summary> 按下：记录时间；Stop 态下 0.4s 后预览长按循环特效。 </summary>
    public virtual void OnPressBegin()
    {
        startTimeS = Time.unscaledTime;
        if (MainModel.Instance?.contentMD?.btnSpinState == SpinButtonState.Stop)
            Timers.inst.Add(0.4f, 1, OnHoldToAuto);
    }

    /// <summary> 抬起：取消长按预览并关闭循环特效。 </summary>
    public virtual void OnPressEnd()
    {
        Timers.inst.Remove(OnHoldToAuto);
        StopLongPressEffect();
    }

    /// <summary> 屏幕按钮抬起：短按播一次性特效，长按只关预览并回调业务。 </summary>
    protected virtual void OnOwnerTouchEnd(bool playClickSound)
    {
        OnPressEnd();
        bool isLong = Time.unscaledTime - startTimeS > 1.2f;
        if (!isLong)
            PlayShortPressEffect();
        if (playClickSound)
            GlobalSoundHelper.Instance.PlaySoundEff(SoundKey.SpinClick);
        onClickCallblack?.Invoke(isLong);
    }

    /// <summary> 按住达到预览时长：播放长按循环特效，并走自动相关表现。 </summary>
    protected virtual void OnHoldToAuto(object param)
    {
        PlayLongPressEffect();
        DoAutoEffect(param);
    }

    protected virtual void DoAutoEffect(object param)
    {
        //animator.Play("hold",-1,0);
        GlobalSoundHelper.Instance.PlaySoundEff(SoundKey.SpinAutoClick);
    }

    /// <summary> 播放短按一次性粒子。未配置预制体时为空操作。 </summary>
    public virtual void PlayShortPressEffect()
    {
        ShowPressEffect(_anchorShortSpin, _goShortSpin);
    }

    /// <summary> 播放长按循环粒子。未配置预制体时为空操作。 </summary>
    public virtual void PlayLongPressEffect()
    {
        ShowPressEffect(_anchorLongSpin, _goLongSpin);
    }

    /// <summary> 停止并隐藏长按循环粒子。 </summary>
    public virtual void StopLongPressEffect()
    {
        HidePressEffect(_anchorLongSpin, _goLongSpin);
    }

    /// <summary> 进入游戏或面板关闭时，短按/长按特效都应处于关闭状态。 </summary>
    public virtual void HideAllPressEffects()
    {
        HidePressEffect(_anchorShortSpin, _goShortSpin);
        HidePressEffect(_anchorLongSpin, _goLongSpin);
    }

    /// <summary> 将预制体挂到 FGUI 锚点。实例先设为隐藏，避免 playOnAwake 在进游戏时闪一下。 </summary>
    protected virtual GameObject WrapToAnchor(GComponent anchor, GameObject prefab)
    {
        if (anchor == null || prefab == null)
            return null;

        GameCommon.FguiUtils.DeleteWrapper(anchor);
        GameObject instance = GameObject.Instantiate(prefab);
        instance.SetActive(false);
        GameCommon.FguiUtils.AddWrapper(anchor, instance);
        SetHolderVisible(anchor, false);
        return instance;
    }

    /// <summary> 显示锚点并重播粒子。 </summary>
    protected virtual void ShowPressEffect(GComponent anchor, GameObject go)
    {
        if (go == null)
            return;

        SetHolderVisible(anchor, true);
        go.SetActive(true);
        if (anchor != null)
            GameCommon.FguiUtils.RefreshWrapper(anchor);

        ParticleSystem[] particles = go.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particles.Length; i++)
            particles[i].Play(true);
    }

    /// <summary> 停粒子、清空残留，并隐藏实例与锚点。 </summary>
    protected virtual void HidePressEffect(GComponent anchor, GameObject go)
    {
        if (go != null)
        {
            ParticleSystem[] particles = go.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particles[i].Clear(true);
            }
            go.SetActive(false);
        }

        SetHolderVisible(anchor, false);
    }

    protected virtual void SetHolderVisible(GComponent anchor, bool visible)
    {
        GGraph holder = anchor?.GetChild("holder")?.asGraph;
        if (holder != null)
            holder.visible = visible;
    }

    protected string _state;
    public virtual string State
    {
        get => _state;
        set
        {
            if (goOwnerSpin == null)
            {
                return;
            }
            //Spin状态下再按Stop，Stop置灰
            switch (_state)
            {
                case SpinButtonState.Spin:
                    goOwnerSpin.GetController("button").selectedPage = "hui";
                    break;
            }
            _state = value;
            switch (_state)
            {
                case SpinButtonState.Stop:
                    goOwnerSpin.GetController("button").selectedPage = "stop";
                    break;
                case SpinButtonState.Auto:
                    goOwnerSpin.GetController("button").selectedPage = "auto";
                    break;
                case SpinButtonState.Spin:
                    goOwnerSpin.GetController("button").selectedPage = "spin";
                    break;
            }

            // 离开可长按的 Stop 态后关闭循环特效，避免 Auto/Spin 期间一直亮着。
            if (_state != SpinButtonState.Stop)
                StopLongPressEffect();
        }
    }

    public virtual void Init(GObject goTarget)
    {
        //throw new NotImplementedException();
    }
}
