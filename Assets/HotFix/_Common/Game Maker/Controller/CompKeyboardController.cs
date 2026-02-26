using FairyGUI;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CY.GameFramework
{
    public class CompKeyboardController 
    {
        /// <summary> 当前使用的键盘 </summary>
        static public CompKeyboardController curKeyboard = null;

        static public CompInputController curInput;

        GObject goRoot, goKB123, goKBabc, goKBABC, goKBOperator;

        public List<GButton> keyboardButtons;

        public void Init(GObject goRoot) => Init(goRoot, null, null, null, null);
        public void Init(GObject goRoot, GObject goKB123, GObject goKBabc, GObject goKBABC, GObject goKBOperator)
        {
            curKeyboard = this;

            this.goRoot = goRoot;
            this.goKB123 = goKB123;
            this.goKBabc = goKBabc;
            this.goKBABC = goKBABC;
            this.goKBOperator = goKBOperator;


            keyboardButtons = new List<GButton>();

            List<GButton> buttons = FguiUtils.GetAllNode<GButton>(goRoot.asCom);

            keyboardButtons.AddRange(buttons);

            foreach (GButton btn in keyboardButtons)
            {
                if (btn.name.StartsWith("No."))
                {
                    string name = btn.name.Replace("No.", "");
                    if(btn.name == "No._DAT")
                    {
                        //DebugUtils.Log($" data: {btn.data}   baseUserData: {btn.baseUserData}");
                        JObject temp = JObject.Parse((string)btn.data);
                        name = temp["key"].ToObject<string>().Replace("No.", "");
                    }
                    btn.onClick.Clear();
                    btn.onClick.Add(() => { OnClickKeyboard(name); });
                }
            }

            if(goKB123 != null)
                OnClickKeyboard("123");
        }



        public void SetFocus(CompInputController input)
        {
            if (curInput != null &&  curInput != input) {
                curInput.LostFocus();
            }
            curInput = input;
        }



        void OnClickKeyboard(string key)
        {
            //string key = name.Replace("No.", "");
            switch (key)
            {
                case "OK":
                    {
                        curInput.LostFocus();
                        curInput.OK();
                        curInput = null;
                    }
                    break;
                case "Clear":
                    {
                        if (curInput != null)
                            curInput.Clear();
                    }
                    break;
                case "Cancel":
                case "Exit":
                    {
                        curInput.Exit();
                    }
                    break;
                case "ABC":
                    {
                        goKB123.visible = false;
                        goKBabc.visible = false;
                        goKBABC.visible = true;
                        goKBOperator.visible = false;
                    }
                    break;
                case "abc":
                    {
                        goKB123.visible = false;
                        goKBabc.visible = true;
                        goKBABC.visible = false;
                        goKBOperator.visible = false;
                    }
                    break;
                case "123":
                    {
                        goKB123.visible = true;
                        goKBabc.visible = false;
                        goKBABC.visible = false;
                        goKBOperator.visible = false;
                    }
                    break;
                case "#+=":
                    {
                        goKB123.visible = false;
                        goKBabc.visible = false;
                        goKBABC.visible = false;
                        goKBOperator.visible = true;
                    }
                    break;
                case "Up":
                    {
                        if (goKBabc.visible)
                        {
                            goKBabc.visible = false;
                            goKBABC.visible = true;
                        }
                        else
                        {
                            goKBabc.visible = true;
                            goKBABC.visible = false;
                        }
                    }
                    break;
                case "Space":
                    {
                        if (curInput != null)
                            curInput.Add(" ");
                    }
                    break;
                case "Delete":
                    if (curInput != null)
                        curInput.Delete();
                    break;
                default:
                    if (curInput != null)
                        curInput.Add(key);
                    break;
            }
        }

    }
}
