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

            // 免费游戏期间保留累计赢分，只在普通Spin开始时清空win
            if (gameState == GameState.Spin)
            {
                win.text = 0.ToString();
                ClearSingleLineText();
            }
        }
    }
}