using FairyGUI;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace PusherEmperorsRein
{


    public class CollectGoldCoinsController : IContorller
    {
        GTextField gTextField;
        GComponent win;

        public void Dispose()
        {

        }

        public void Init() { }
        public void InitParam(params object[] parameters) => InitParam(parameters[0] as GObject);

        public void InitParam(GObject win)
        {
            this.win = win.asCom;
            this.win.visible = false;
            this.gTextField = this.win.GetChild("title").asTextField;
        }



        public void DiaoJinBin(int gold)
        {
            win.visible = true;
            gTextField.text = gold.ToString();
        }

        public void Close()
        {
            win.visible = false;
        }
    }

}