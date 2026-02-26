using FairyGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace ConsoleCoinPusher01
{
    public class KeyBoard02Controller : IKeyboard
    {
        public KeyBoard02Controller(){ Init(); }
        void Init() { }

        int num = 10;

        public void ClickNext()
        {
            if (!isCanOnClick)
                return;

            if (++curIndexKeyboard >= glst.numChildren)
                curIndexKeyboard = 0;

            for (int i = 0; i < glst.numChildren; i++)
            {
                glst.GetChildAt(i).asCom.GetChild("icon").asImage.visible = i == curIndexKeyboard;
            }

        }
        public void ClickPrev()
        {
            if (!isCanOnClick)
                return;

            if (--curIndexKeyboard < 0)
                curIndexKeyboard = glst.numChildren - 1;

            for (int i = 0; i < glst.numChildren; i++)
            {
                glst.GetChildAt(i).asCom.GetChild("icon").asImage.visible = i == curIndexKeyboard;
            }

        }


        public void ClickDown()
        {

            if (!isCanOnClick)
                return;

            curIndexKeyboard += num;
            if (curIndexKeyboard >= glst.numChildren)
                curIndexKeyboard -= num;

            for (int i = 0; i < glst.numChildren; i++)
            {
                glst.GetChildAt(i).asCom.GetChild("icon").asImage.visible = i == curIndexKeyboard;
            }

        }

        public void ClickUp()
        {

            if (!isCanOnClick)
                return;

            curIndexKeyboard -= num;
            if (curIndexKeyboard < 0)
                curIndexKeyboard += num;

            for (int i = 0; i < glst.numChildren; i++)
            {
                glst.GetChildAt(i).asCom.GetChild("icon").asImage.visible = i == curIndexKeyboard;
            }

        }


        public void ClickConfirm()
        {
            if (!isCanOnClick)
                return;

            GList keyboard = new GList();
            switch (UseWhatKeyboard)
            {
                case 0:
                    keyboard=glstkeyboard;
                    break;
                case 1:
                    keyboard = glstkeyboardZiFu;
                    break;
                case 2:
                    keyboard = glstkeyboardDaXie;
                    break;
                case 3:
                    keyboard = glstkeyboardXiaoXie;
                    break;
            }


            if (curIndexKeyboard == keyboard.numChildren - 1) // exit
            {
                onClickExitCallback?.Invoke();
            }
            else if (curIndexKeyboard == keyboard.numChildren - 2) // ok
            {
                onClickOKCallback?.Invoke(inputResult);
            }
            else if (curIndexKeyboard == keyboard.numChildren - 3) //delete
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
            else if (curIndexKeyboard == keyboard.numChildren - 4) //clear
            {
                Clear(false);
            }
            else  
            {

                if (curIndexInput == glstInput.numChildren)
                {
                    return;
                }
                else
                {
                    string data = keyboard.GetChildAt(curIndexKeyboard).asCom.GetChild("title").text;
                    switch (data)
                    {
                        case "空格":
                            data = "";
                            break;
                        case "字符":
                            OpenWhatKeyborad(1);
                            return;
                        case "字母":
                            OpenWhatKeyborad(3);
                            return;
                        case "数字":
                            OpenWhatKeyborad(0);
                            return;
                        case "小写":
                            OpenWhatKeyborad(3);
                            return;
                        case "大写":
                            OpenWhatKeyborad(2);
                            return;
                    }
                    glstInput.GetChildAt(curIndexInput).text = isPlaintext ? data :
                        "<img src='ui://ConsoleCoinPusher01/icon-asterisk' />";
                    inputResult += data;
                    curIndexInput++;
                }
            }

        }


        Action<string> onClickOKCallback;
        Action onClickExitCallback;


        public GComponent goOwnerKeyboard;
        GList glstkeyboard, glstInput, glstkeyboardZiFu, glstkeyboardDaXie, glstkeyboardXiaoXie,glst;
        GLabel labTip;


        int curIndexKeyboard = 0;
        int curIndexInput = 0;

        int UseWhatKeyboard = 0;

        bool isPlaintext = true;
        public  bool isCanOnClick;
        string inputResult = "";

        public void Clear(bool isClearAllArrow)
        {
            curIndexKeyboard = 0;
            curIndexInput = 0;
            inputResult = "";

            for (int i = 0; i < glstkeyboard.numChildren; i++)
            {
                GComponent item = glstkeyboard.GetChildAt(i).asCom;

                if (isClearAllArrow)
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
        public void InitParam(GComponent gKB, bool isPlaintext, Action<string> onClickOKCallback, Action onClickExitCallback)
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
            glstkeyboardZiFu = goOwnerKeyboard.GetChild("buttonsZiFu").asList;
            glstkeyboardDaXie = goOwnerKeyboard.GetChild("buttonsDaXie").asList;
            glstkeyboardXiaoXie = goOwnerKeyboard.GetChild("buttonsXiaoXie").asList;
            glstInput = goOwnerKeyboard.GetChild("input").asList;
            GComponent compTip = goOwnerKeyboard.GetChild("tip").asCom;
            GRichTextField rtxtTip = compTip.GetChild("title").asRichTextField;
            rtxtTip.text = "";
            labTip = compTip.asLabel;
            labTip.title = "";

            AddButtonEventAll();


            Clear(true);
            UseWhatKeyboard = 0;
            OpenSpecifyCursor(0);
            glstkeyboard.visible = true;
            glstkeyboardZiFu.visible = false;
            glstkeyboardDaXie.visible = false;
            glstkeyboardXiaoXie.visible = false;
            isCanOnClick = false;
        }



        public void OpenSpecifyCursor(int index)
        {
            GList keyboard = new GList();
            switch (UseWhatKeyboard)
            {
                case 0:
                    keyboard = glstkeyboard;
                    break;
                case 1:
                    keyboard = glstkeyboardZiFu;
                    break;
                case 2:
                    keyboard = glstkeyboardDaXie;
                    break;
                case 3:
                    keyboard = glstkeyboardXiaoXie;
                    break;
            }
            glst = keyboard;

            for (int i = 0; i < keyboard.numChildren; i++)
            {
                if (i==0)
                {
                    keyboard.GetChildAt(i).asCom.GetChild("icon").asImage.visible = true;
                }
                else
                {
                    keyboard.GetChildAt(i).asCom.GetChild("icon").asImage.visible = false;
                }

            }
            curIndexKeyboard = 0;
        }



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


        public void AddButtonEvent(int n)
        {
            GList keyboard = new GList();
            switch (n)
            {
                case 0:
                    keyboard = glstkeyboard;
                    break;
                case 1:
                    keyboard = glstkeyboardZiFu;
                    break;
                case 2:
                    keyboard = glstkeyboardDaXie;
                    break;
                case 3:
                    keyboard = glstkeyboardXiaoXie;
                    break;
            }
            glst = keyboard;

            for (int i = 0; i < keyboard.numChildren; i++)
            {
                int index = i;


                if (index == keyboard.numChildren - 4)
                {
                    keyboard.GetChildAt(index).onClick.Clear();
                    keyboard.GetChildAt(index).onClick.Add(() =>
                    {
                        if (!isCanOnClick)
                            return;
                        Clear(false);
                        OpenSpecifyCursor(index);
                    });
                }
                else if (index == keyboard.numChildren - 3)
                {
                    keyboard.GetChildAt(index).onClick.Clear();
                    keyboard.GetChildAt(index).onClick.Add(() =>
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
                        OpenSpecifyCursor(index);
                    });
                }
                else if (index == keyboard.numChildren - 2)
                {
                    keyboard.GetChildAt(index).onClick.Clear();
                    keyboard.GetChildAt(index).onClick.Add(() =>
                    {
                        if (!isCanOnClick)
                            return;

                        string res = inputResult;
                        Disable();
                        onClickOKCallback?.Invoke(res);
                    });
                }
                else if (index == keyboard.numChildren - 1)
                {
                    keyboard.GetChildAt(index).onClick.Clear();
                    keyboard.GetChildAt(index).onClick.Add(() =>
                    {
                        if (!isCanOnClick)
                            return;
                        OpenSpecifyCursor(index);
                        isCanOnClick = false;
                        onClickExitCallback?.Invoke();
                    });
                }else
                {
                    keyboard.GetChildAt(index).onClick.Clear();
                    keyboard.GetChildAt(index).onClick.Add(() =>
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
                            string data = keyboard.GetChildAt(curIndexKeyboard).asCom.GetChild("title").text;
                            for (int i = 0; i < glst.numChildren; i++)
                            {
                                glst.GetChildAt(i).asCom.GetChild("icon").asImage.visible = i == curIndexKeyboard;
                            }
                            switch (data)
                            {
                                case "空格":
                                    data = "";
                                    break;
                                case "字符":
                                    OpenWhatKeyborad(1);
                                    return;
                                case "字母":
                                    OpenWhatKeyborad(3);
                                    return;
                                case "数字":
                                    OpenWhatKeyborad(0);
                                    return;
                                case "小写":
                                    OpenWhatKeyborad(3);
                                    return;
                                case "大写":
                                    OpenWhatKeyborad(2);
                                    return;
                            }
                            glstInput.GetChildAt(curIndexInput).text = isPlaintext ? data :
                                "<img src='ui://ConsoleCoinPusher01/icon-asterisk' />";
                            inputResult += data;
                            curIndexInput++;
                        
                        }
                    });
                }

            }
        }

        void AddButtonEventAll()
        {
            AddButtonEvent(0);
            AddButtonEvent(1);
            AddButtonEvent(2);
            AddButtonEvent(3);
        }


        void OpenWhatKeyborad(int n)
        {
            glstkeyboard.visible = n == 0 ? true : false;
            glstkeyboardZiFu.visible = n == 1 ? true : false;
            glstkeyboardDaXie.visible = n == 2 ? true : false;
            glstkeyboardXiaoXie.visible = n == 3 ? true : false;

            UseWhatKeyboard = n;
            OpenSpecifyCursor(curIndexKeyboard);
        }


        public void End()
        {

        }

        /// <summary> 反复调用，变更绑定对象 </summary>
        //void InitParam(params object[] parameters);

        /// <summary> 销毁时调用一次 </summary>
        public void Dispose()
        {

        }


    }

}
