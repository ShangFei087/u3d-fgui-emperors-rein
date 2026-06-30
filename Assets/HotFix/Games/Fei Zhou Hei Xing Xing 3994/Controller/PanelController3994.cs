using GameMaker;
using SlotMaker;

namespace FeiZhouHeiXingXing_3994
{
    public class PanelController3994 : PanelBaseController
    {
        protected override string PanelPackagePath => "Assets/GameRes/Panel/Panel3994/FGUIs";
        
        protected override void OnPropertyGameState(EventData res = null)
        {
            string gameState = (string)res?.value;

            if (gameState == GameState.Spin)
                win.text = 0.ToString();
            ClearSingleLineText();
        }
    }
}

