using FairyGUI;
using SlotMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace slotEmperorsRein
{
    public class SpinButtonController : SpinButtonBaseController
    {
        protected override void DoAutoEffect(object param)
        {
            goOwnerSpin.GetController("button").selectedPage = "toAuto";
        }
        public override string State { get => base.State; set
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
                        break;
                    case SpinButtonState.Hui:
                        goOwnerSpin.GetController("button").selectedPage = "hui";
                        break;
                    case SpinButtonState.Spin:
                        goOwnerSpin.GetController("button").selectedPage = "spin";
                        break;

                }

            }
        }
    }
}

