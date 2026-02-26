using FairyGUI;
using GameMaker;
using SlotMaker;
using UnityEngine;
using UnityEngine.Events;
using SoundKey = GameMaker.SoundKey;

public class SpinController02 : SpinButtonBaseController //,IContorller
{




    public SpinController02()
    {
        Init();
    }

    public void Init()
    {
        // 注册事件监听
        EventCenter.Instance.AddEventListener<EventData>(
            Observer.ON_PROPERTY_CHANGED_EVENT,
            ChangRemainPlaySpins);
    }

   // public void InitParam(params object[] parameters) { }


    // Update is called once per frame
    void Dispose()
    {
        EventCenter.Instance.RemoveEventListener<EventData>(
           Observer.ON_PROPERTY_CHANGED_EVENT,
           ChangRemainPlaySpins);
    }

    GTextField gTxt;

    public new void InitParam(GComponent spin, string state, UnityAction<bool> onClick, GameObject gameObject)
    {
        goOwnerSpin = spin;

        onClickCallblack.RemoveAllListeners();
        onClickCallblack.AddListener(onClick);
        float timeS = Time.unscaledTime;
        GameCommon.FguiUtils.DeleteWrapper(goOwnerSpin.GetChild("anchorSpin").asCom);
        GameObject goBtnEffect = GameObject.Instantiate(gameObject);
        animator = goBtnEffect.transform.GetChild(1).GetChild(0).GetComponent<Animator>();
        GameCommon.FguiUtils.AddWrapper(goOwnerSpin.GetChild("anchorSpin").asCom, goBtnEffect);
        gTxt = goOwnerSpin.GetChild("playTotalSpins").asTextField;
        gTxt.text = "";
        State = state;
        goOwnerSpin.onTouchBegin.Clear();
        goOwnerSpin.onTouchBegin.Add(() =>
        {
            startTimeS = Time.unscaledTime;
            if (MainModel.Instance.contentMD.btnSpinState == SpinButtonState.Stop)
                Timers.inst.Add(0.4f, 1, ToAutoEffect);
        });
        goOwnerSpin.onTouchEnd.Clear();
        goOwnerSpin.onTouchEnd.Add(() =>
        {
            Timers.inst.Remove(ToAutoEffect);
            //Timers.inst.Remove(DoAuto);
            GlobalSoundHelper.Instance.PlaySoundEff(SoundKey.SpinClick);
            float difTimeS = Time.unscaledTime - startTimeS;
            onClickCallblack?.Invoke(difTimeS > 1.2f ? true : false);
        });

    }

    protected virtual void ToAutoEffect(object param)
    {
        animator.Play("hold", -1, 0);
        GlobalSoundHelper.Instance.PlaySoundEff(SoundKey.SpinAutoClick);

        Timers.inst.Add(1, 1, ToAuto);

    }

    void ToAuto(object param)
    {
        goOwnerSpin.GetController("button").selectedPage = "auto";

    }

    //string _state;
    public new  string State
    {
        get => _state;
        set
        {
            if (goOwnerSpin == null)
            {
                return;
            }

            Timers.inst.Remove(ToAuto);

            _state = value;
            animator.Rebind(); // 重新绑定动画控制器，确保状态刷新
            switch (_state)
            {

                case SpinButtonState.Stop:
                    goOwnerSpin.GetController("button").selectedPage = "stop";
                    animator.Play("idle_blue",-1,0);
                    gTxt.visible = true;
                    break;
                case SpinButtonState.Hui:
                    goOwnerSpin.GetController("button").selectedPage = "hui";
                    animator.Play("idle_white", -1, 0);
                    break;
                case SpinButtonState.Auto:
                    animator.Play("idle_blue2", -1, 0);
                    ToAuto(null);
                    gTxt.visible = false;
                    break;
                case SpinButtonState.Spin:
                    goOwnerSpin.GetController("button").selectedPage = "spin";
                    animator.Play("idle_red", -1, 0);
                    break;

            }

        }

    }

    void ChangRemainPlaySpins(EventData res)
    {
        if (goOwnerSpin == null)
            return;

        if (res.name == "ContentModel/remainPlaySpins")
        {
            if (MainModel.Instance.contentMD.totalPlaySpins != 1)
            {
                gTxt.text = ((int)res.value).ToString();
                gTxt.visible = true;
            }
            else
            {
                goOwnerSpin.GetChild("playTotalSpins").asTextField.text = "";
            }
        }

    }

    /*
    public void Init(GObject goTarget)
    {
        //throw new NotImplementedException();
    }

    void IContorller.Dispose()
    {
        Dispose();
    }*/
}
