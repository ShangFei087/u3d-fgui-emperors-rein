using FairyGUI;
using GameMaker;
using GameUtil;
using SlotMaker;
using System;
using UnityEngine;
using UnityEngine.Events;

public class SpinButtonBaseController // : IContorller
{
    public  GComponent goOwnerSpin;
    protected GameObject goSpin;
    protected Animator animator;
    protected float startTimeS = 0;
    protected int playTotalSpins = 0;
    public UnityEvent<bool> onClickCallblack = new UnityEvent<bool>();

    public virtual void InitParam(GComponent spin, string state, UnityAction<bool> onClick)
    {
        goOwnerSpin = spin;
        State = state;
        onClickCallblack.RemoveAllListeners();
        onClickCallblack.AddListener(onClick);
        float timeS = Time.unscaledTime;

        goOwnerSpin.onTouchBegin.Clear();
        goOwnerSpin.onTouchBegin.Add(() =>
        {
            startTimeS = Time.unscaledTime;
            if(MainModel.Instance.contentMD.btnSpinState == SpinButtonState.Stop)
                Timers.inst.Add(0.4f, 1, DoAutoEffect);
        });
        goOwnerSpin.onTouchEnd.Clear();
        goOwnerSpin.onTouchEnd.Add(() =>
        {
            Timers.inst.Remove(DoAutoEffect);   
            float difTimeS = Time.unscaledTime - startTimeS;
            onClickCallblack?.Invoke(difTimeS > 1.2f? true: false);
        });
    }



    public virtual void InitParam(GComponent spin, string state, UnityAction<bool> onClick,GameObject gameObject)
    {
        goOwnerSpin = spin;

        onClickCallblack.RemoveAllListeners();
        onClickCallblack.AddListener(onClick);
        float timeS = Time.unscaledTime;
        GameCommon.FguiUtils.DeleteWrapper(goOwnerSpin.GetChild("anchorSpin").asCom);
        GameObject goBtnEffect = GameObject.Instantiate(gameObject);
        //animator = goBtnEffect.transform.GetChild(1).GetChild(0).GetComponent<Animator>();
        GameCommon.FguiUtils.AddWrapper(goOwnerSpin.GetChild("anchorSpin").asCom, goBtnEffect);
        State = state;
        goOwnerSpin.onTouchBegin.Clear();
        goOwnerSpin.onTouchBegin.Add(() =>
        {
            startTimeS = Time.unscaledTime;
            if (MainModel.Instance.contentMD.btnSpinState == SpinButtonState.Stop)
                Timers.inst.Add(0.4f, 1, DoAutoEffect);
        });
        goOwnerSpin.onTouchEnd.Clear();
        goOwnerSpin.onTouchEnd.Add(() =>
        {
            Timers.inst.Remove(DoAutoEffect);
            GlobalSoundHelper.Instance.PlaySoundEff(SoundKey.SpinClick);
            float difTimeS = Time.unscaledTime - startTimeS;
            onClickCallblack?.Invoke(difTimeS > 1.2f ? true : false);
        });
    }

    protected virtual void DoAutoEffect(object param)
    {
        //animator.Play("hold",-1,0);
        GlobalSoundHelper.Instance.PlaySoundEff(SoundKey.SpinAutoClick);
        
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
            _state = value;
            switch (_state)
            {
                case SpinButtonState.Stop:
                    goOwnerSpin.GetController("button").selectedPage = "stop";
                    //animator.Play("play", -1, 0);
                    break;
                case SpinButtonState.Auto:
                    goOwnerSpin.GetController("button").selectedPage = "auto";
                   // animator.Play("auto", -1, 0);
                    break;
                case SpinButtonState.Spin:
                    goOwnerSpin.GetController("button").selectedPage = "spin";
                    //animator.Play("stop", -1, 0);
                    break;

            }

        }

    } 


    public virtual void Init(GObject goTarget)
    {
        //throw new NotImplementedException();
    }

}
