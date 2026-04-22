using GameMaker;
using SlotMaker;

namespace CaiFuZhiJia_3997
{
    public class PanelController3997 : PanelBaseController
    {
        protected override string PanelPackagePath => "Assets/GameRes/Panel/Cai Fu Zhi Jia 3997/FGUIs";

        protected override void OnPropertyGameState(EventData res = null)
        {
            string gameState = (string)res?.value;

            if (gameState == GameState.Spin)
                win.text = 0.ToString();
            ClearSingleLineText();
        }
    }
}