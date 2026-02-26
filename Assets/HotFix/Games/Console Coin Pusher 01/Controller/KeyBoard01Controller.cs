using FairyGUI;
using System;
using UnityEngine;



namespace ConsoleCoinPusher01
{


    public interface IKeyboard
    {
        void Disable();
        void Enable();
        void ClickNext();
        void ClickPrev();
        void ClickUp();

        void ClickDown();
        void ClickConfirm();
    }

    public class KeyBoard01Controller : IKeyboard //:IContorller
    {
        /// <summary>
        /// 一行多少个
        /// </summary>
        int num=10;
        public KeyBoard01Controller()
        {
            Init();
        }


        void Init()
        {

        }


        public void ClickNext(){

            if (!isCanOnClick)
                return;

            if (++curIndexKeyboard >= glstkeyboard.numChildren)
                curIndexKeyboard = 0;

            for (int i = 0; i < glstkeyboard.numChildren; i++)
            {
                glstkeyboard.GetChildAt(i).asCom.GetChild("icon").asImage.visible = i == curIndexKeyboard;
            }

        }

        public void ClickDown()
        {

            if (!isCanOnClick)
                return;

            curIndexKeyboard += num;
            if (curIndexKeyboard >= glstkeyboard.numChildren)
                curIndexKeyboard -= num;

            for (int i = 0; i < glstkeyboard.numChildren; i++)
            {
                glstkeyboard.GetChildAt(i).asCom.GetChild("icon").asImage.visible = i == curIndexKeyboard;
            }

        }

        public void ClickUp()
        {

            if (!isCanOnClick)
                return;

            curIndexKeyboard -= num;
            if (curIndexKeyboard < 0)
                curIndexKeyboard += num;

            for (int i = 0; i < glstkeyboard.numChildren; i++)
            {
                glstkeyboard.GetChildAt(i).asCom.GetChild("icon").asImage.visible = i == curIndexKeyboard;
            }

        }

        public void ClickPrev()
        {

            if (!isCanOnClick)
                return;

            if (--curIndexKeyboard <0)
                curIndexKeyboard = glstkeyboard.numChildren-1;

            for (int i = 0; i < glstkeyboard.numChildren; i++)
            {
                glstkeyboard.GetChildAt(i).asCom.GetChild("icon").asImage.visible = i == curIndexKeyboard;
            }

        }

        public void ClickConfirm()
        {

            if (!isCanOnClick)
                return;


            if (curIndexKeyboard == glstkeyboard.numChildren - 1) // exit
            {
                onClickExitCallback?.Invoke();
            }
            else if (curIndexKeyboard == glstkeyboard.numChildren - 2) // ok
            {
                onClickOKCallback?.Invoke(inputResult);
            }
            else if( curIndexKeyboard == glstkeyboard.numChildren - 3) //delete
            {
                if (curIndexInput >= 0)
                {
                    if (--curIndexInput < 0)
                    {
                        curIndexInput = 0;
                        return;
                    }
                    else
                    {
                        inputResult = inputResult.Substring(0, inputResult.Length - 1);
                        glstInput.GetChildAt(curIndexInput).text = "";
                    }

                }
            }
            else if (curIndexKeyboard == glstkeyboard.numChildren - 4) //clear
            {
                Clear(false);
            }
            else  // 0 ~ 9
            {
                if(curIndexKeyboard>=0 && curIndexKeyboard <= 9)
                {
                    if (curIndexInput == glstInput.numChildren)
                    {
                        return;
                    }
                    else
                    {
                        string data = $"{curIndexKeyboard}";
                        glstInput.GetChildAt(curIndexInput).text = isPlaintext? data:
                            "<img src='ui://ConsoleCoinPusher01/icon-asterisk' />";
                        inputResult += data;
                        curIndexInput++;
                    }
                }
            }

        }


        Action<string> onClickOKCallback;
        Action onClickExitCallback;


        public GComponent goOwnerKeyboard;
        GList glstkeyboard, glstInput;
        GLabel labTip;


        int curIndexKeyboard = 0;
        int curIndexInput = 0;
        bool isPlaintext = true;

        string inputResult = "";

        public void Clear(bool isClearAllArrow)
        {
            curIndexKeyboard = 0;
            curIndexInput = 0;
            inputResult = "";

            for (int i = 0; i < glstkeyboard.numChildren; i++)
            {
                GComponent item = glstkeyboard.GetChildAt(i).asCom;

                if(isClearAllArrow)
                    item.GetChild("icon").asImage.visible = false;
                else
                    item.GetChild("icon").asImage.visible = i == curIndexKeyboard;

                //DebugUtils.Log($"name: {item.name}");
            }

            for (int i = 0; i < glstInput.numChildren; i++)
            {
                glstInput.GetChildAt(i).asLabel.title = "";
            }
        }
        public void InitParam(GComponent gKB , bool isPlaintext , Action<string> onClickOKCallback, Action onClickExitCallback)
        {
            if (gKB == null) return;

            goOwnerKeyboard = gKB;
            this.isPlaintext = isPlaintext;
            this.onClickOKCallback = onClickOKCallback;
            this.onClickExitCallback = onClickExitCallback;

            GObject[] childs = goOwnerKeyboard.GetChildren();
            foreach (GObject child in childs)
            {
                DebugUtils.Log("--" + child.name);
            }

            glstkeyboard = goOwnerKeyboard.GetChild("buttons").asList;
            glstInput = goOwnerKeyboard.GetChild("input").asList;
            GComponent compTip = goOwnerKeyboard.GetChild("tip").asCom;
            GRichTextField rtxtTip = compTip.GetChild("title").asRichTextField;
            rtxtTip.text = "";
            labTip = compTip.asLabel;
            labTip.title = "";



            for (int i = 0; i < 10; i++)
            {
                int index = i;
                glstkeyboard.GetChild($"No.{index}").onClick.Clear();
                glstkeyboard.GetChild($"No.{index}").onClick.Add(() =>
                {

                    if (!isCanOnClick)
                        return;

                    curIndexKeyboard = index;
                    if (curIndexInput == glstInput.numChildren)
                    {
                        return;
                    }
                    else
                    {
                        string data = $"{curIndexKeyboard}";
                        glstInput.GetChildAt(curIndexInput).text = isPlaintext ? data :
                            "<img src='ui://ConsoleCoinPusher01/icon-asterisk' />";
                        inputResult += data;
                        curIndexInput++;

                    }
                    OpenSpecifyCursor($"No.{index}");
                });
            }




            glstkeyboard.GetChild("clear").onClick.Clear();
            glstkeyboard.GetChild("clear").onClick.Add(() =>
            {
                if (!isCanOnClick)
                    return;

                Clear(false);
                OpenSpecifyCursor("clear");

            });

            glstkeyboard.GetChild("delete").onClick.Clear();
            glstkeyboard.GetChild("delete").onClick.Add(() =>
            {
                if (!isCanOnClick)
                    return;



                if (curIndexInput >= 0)
                {
                    if (--curIndexInput < 0)
                    {
                        curIndexInput = 0;
                        return;
                    }
                    else
                    {
                        inputResult = inputResult.Substring(0, inputResult.Length - 1);
                        glstInput.GetChildAt(curIndexInput).text = "";

                    }
                }
                OpenSpecifyCursor("delete");

            });

            glstkeyboard.GetChild("ok").onClick.Clear();
            glstkeyboard.GetChild("ok").onClick.Add(() =>
            {
                if (!isCanOnClick)
                    return;

                OpenSpecifyCursor("ok");
                onClickOKCallback?.Invoke(inputResult);

            });

            glstkeyboard.GetChild("exit").onClick.Clear();
            glstkeyboard.GetChild("exit").onClick.Add(() =>
            {
                if (!isCanOnClick)
                    return;


                OpenSpecifyCursor("exit");
                onClickExitCallback?.Invoke();
            });

            Clear(false);

        }


        public void OpenSpecifyCursor(string name)
        {
            for (int i = 0; i < glstkeyboard.numChildren; i++)
            {
                glstkeyboard.GetChildAt(i).asCom.GetChild("icon").asImage.visible = false;
            }
            glstkeyboard.GetChild(name).asCom.GetChild("icon").asImage.visible = true;
        }

        bool isCanOnClick = false;
        public void Enable()
        {
            Clear(false);
            isCanOnClick = true;
        }

        public void Disable()
        {
            Clear(true);
            isCanOnClick = false;
        }




        /// <summary> 反复调用，变更绑定对象 </summary>
        //void InitParam(params object[] parameters);

        /// <summary> 销毁时调用一次 </summary>
        public void Dispose()
        {

        }
    }
}
