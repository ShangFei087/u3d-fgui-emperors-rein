using GameMaker;
using SlotMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    public class PanelController3993 : SlotMaker.PanelBaseController
    {
        public override void Init(EventData res = null)
        {
            base.Init(res);
        }

        protected override void InitParam()
        {
            base.InitParam();
        }

        protected override void OnPropertyGameState(EventData res = null)
        {
            string gameState = (string)res?.value;

            if (gameState == GameState.Spin)
            {
                win.text = 0.ToString();
                ClearSingleLineText();
            }
            else if (gameState == GameState.FreeSpin)
            {
                ClearSingleLineText();
            }
        }
    }
}