using FairyGUI;
using GameMaker;
using SlotMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelController1700 : SlotMaker.PanelBaseController
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

        // 免费游戏期间保留面板累计赢分，只在普通Spin开始时清空
        if (gameState == GameState.Spin)
        {
            win.text = 0.ToString();
            ClearSingleLineText();
        }
    }
  
}
