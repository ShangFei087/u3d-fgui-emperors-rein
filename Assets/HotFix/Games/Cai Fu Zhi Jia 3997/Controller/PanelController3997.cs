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
                win.text = 0.ToString();
            ClearSingleLineText(); 
        }
    }
}