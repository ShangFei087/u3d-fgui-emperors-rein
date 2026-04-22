using FairyGUI;
using GameMaker;
using PusherEmperorsRein;
using SBoxApi;
using SlotMaker;
using System;
using UnityEngine;

namespace CaiFuZhiJia_3997
{
    public class PanelController3997 : PanelBaseController
    {
        // protected override string PanelUrl => "ui://CaiFuZhiJia_Panel/Panel";
        // protected override string PanelPackagePath => "Assets/GameRes/Games/Panel 3997";

        protected override void OnPropertyGameState(EventData res = null)
        {
            string gameState = (string)res?.value;

            if (gameState == GameState.Spin)
                win.text = 0.ToString();
            ClearSingleLineText();
        }
    }
}