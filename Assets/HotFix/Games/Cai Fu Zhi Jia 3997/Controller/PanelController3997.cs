using GameMaker;
using SlotMaker;

namespace CaiFuZhiJia_3997
{
    public class PanelController3997 : PanelBaseController
    {
        protected override string PanelPackagePath => "Assets/GameRes/Panel/Panel3997/FGUIs";
        protected override string ShortSpinPrefabPath => "Assets/GameRes/Panel/Panel3997/Prefabs/Eff_ShortSpin.prefab"; 
        protected override string LongSpinPrefabPath => "Assets/GameRes/Panel/Panel3997/Prefabs/Eff_LongSpin.prefab";
        
        protected override void OnPropertyGameState(EventData res = null)
        {
            string gameState = (string)res?.value;

            if (gameState == GameState.Spin)
                win.text = 0.ToString();
            ClearSingleLineText();
        }
    }
}