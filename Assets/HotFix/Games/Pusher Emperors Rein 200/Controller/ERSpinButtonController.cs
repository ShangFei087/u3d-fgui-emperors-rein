using SlotMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ERSpinButtonController : SpinButtonBaseController
{

    public override string State
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
                    animator.Play("idle_blue", -1, 0);
                    break;
                case SpinButtonState.Hui:
                    goOwnerSpin.GetController("button").selectedPage = "hui";
                    animator.Play("idle_white", -1, 0);
                    break;
                case SpinButtonState.Spin:
                    goOwnerSpin.GetController("button").selectedPage = "spin";
                    animator.Play("idle_red", -1, 0);
                    break;

            }

        }

    }


}
