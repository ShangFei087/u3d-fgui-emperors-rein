using GameMaker;
using SlotMaker;
using UnityEngine;

namespace CaiFuZhiMen_3999
{
    public class PanelController3999 : SlotMaker.PanelBaseController
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