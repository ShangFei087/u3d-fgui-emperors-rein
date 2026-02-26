using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using GameMaker;
using SlotMaker;


namespace PusherEmperorsRein
{


    public class WinTipController : IContorller
    {
        GLabel labOwnerWinTip;

        public WinTipController()
        {
            Init();
        }

        public void Init()
        {
            //Dispose();
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_WIN_EVENT, onWinEvent);
        }

        public void InitParam(params object[] parameters) { }

        public void InitParam(GObject goWinTip)
        {
            labOwnerWinTip = goWinTip.asLabel;
            labOwnerWinTip.visible = false;
        }

        /// <summary>
        /// 销毁时调用
        /// </summary>
        public void Dispose()
        {
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_WIN_EVENT, onWinEvent);
        }


        void onWinEvent(EventData res)
        {
            switch (res.name)
            {
                case SlotMachineEvent.SkipWinLine:
                    OnSkipWin(res);
                    break;
                case SlotMachineEvent.TotalWinLine:
                case SlotMachineEvent.SingleWinLine:
                    OnSingleWin(res);
                    break;
            }
        }

        void OnSkipWin(EventData receivedEvent = null)
        {
            labOwnerWinTip.visible = false;
        }

        void OnSingleWin(EventData receivedEvent = null)
        {
            if (!SpinWinEffectSettingModel.Instance.isShowWinCredit)
                return;

            SymbolWin sw = receivedEvent.value as SymbolWin;

            labOwnerWinTip.visible = true;
            labOwnerWinTip.title = $"{sw.earnCredit}";
            Transition transition = labOwnerWinTip.GetTransition("animBigger");
            transition.Play();
        }
    }
}