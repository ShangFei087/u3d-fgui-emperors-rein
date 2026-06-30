using GameMaker;
using SlotMaker;

namespace MeiZhouHeiBao_3993
{
    public class PanelController3993 : PanelBaseController
    {
        protected override string PanelPackagePath => "Assets/GameRes/Panel/Panel3993/FGUIs";
        
        protected override void OnPropertyGameState(EventData res = null)
        {
            string gameState = (string)res?.value;

            if (gameState == GameState.Spin)
                win.text = 0.ToString();
            ClearSingleLineText();
        }
    }
}