using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineTestButton : MonoBehaviour
{
    bool isDownCtrl = false;
    bool isDownShift = false;
    void Update()
    {
        //硬件没有接口，先允许该脚本功能使用
        //if (!Application.isEditor) return;

        if (Input.GetKeyDown(KeyCode.RightControl) || Input.GetKeyDown(KeyCode.LeftControl))
        {
            isDownCtrl = true;
        }
        if (Input.GetKeyUp(KeyCode.RightControl) || Input.GetKeyUp(KeyCode.LeftControl))
        {
            isDownCtrl = false;
        }

        if (Input.GetKeyDown(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.LeftShift))
        {
            isDownShift = true;
        }
        if (Input.GetKeyUp(KeyCode.RightShift) || Input.GetKeyUp(KeyCode.LeftShift))
        {
            isDownShift = false;
        }

        // 投退币按钮
        if (isDownCtrl && isDownShift)
        {


            if (Input.GetKeyDown(KeyCode.KeypadPlus)|| Input.GetKeyDown(KeyCode.Plus)) // score up
            {
                MachineDeviceController.Instance.OnKeyDown(MachineButtonKey.BtnCreditUp);
            }
            if (Input.GetKeyUp(KeyCode.KeypadPlus)|| Input.GetKeyUp(KeyCode.Plus)) // score up
            {
                MachineDeviceController.Instance.OnKeyUp(MachineButtonKey.BtnCreditUp);
            }



            if (Input.GetKeyDown(KeyCode.KeypadMinus) || Input.GetKeyDown(KeyCode.Minus)) // score down
            {
                MachineDeviceController.Instance.OnKeyDown(MachineButtonKey.BtnCreditDown);
            }
            if (Input.GetKeyUp(KeyCode.KeypadMinus) || Input.GetKeyUp(KeyCode.Minus)) // score down
            {
                MachineDeviceController.Instance.OnKeyUp(MachineButtonKey.BtnCreditDown);
            }
            
      
            if (Input.GetKeyDown(KeyCode.F1)) // ticket out
            {
                MachineDeviceController.Instance.OnKeyDown(MachineButtonKey.BtnTicketOut);
            }
            if (Input.GetKeyUp(KeyCode.F1)) // ticket out
            {
                MachineDeviceController.Instance.OnKeyUp(MachineButtonKey.BtnTicketOut);
            }


            if (Input.GetKeyDown(KeyCode.F2)) // print out
            {
                //BtnPrint
            }
            if (Input.GetKeyUp(KeyCode.F2)) // print out
            {
    
            }


            if (Input.GetKeyDown(KeyCode.F3)) // Console
            {
                MachineDeviceController.Instance.OnKeyDown(MachineButtonKey.BtnConsole);
            }
            if (Input.GetKeyUp(KeyCode.F3)) // Console
            {
                MachineDeviceController.Instance.OnKeyUp(MachineButtonKey.BtnConsole);
            }
        }

        
        
        
     


        
        // 游戏按钮
        if (isDownCtrl && isDownShift)
        {
            
            
            if (Input.GetKeyDown(KeyCode.Return)|| Input.GetKeyDown(KeyCode.KeypadEnter)) //开始游戏 Spin
            {
                MachineDeviceController.Instance.OnKeyDown(MachineButtonKey.BtnSpin);

            }
        
            if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter)) //开始游戏 Spin
            {
                MachineDeviceController.Instance.OnKeyUp(MachineButtonKey.BtnSpin);
            }

            if (Input.GetKeyDown(KeyCode.F4)) 
            {

                MachineDeviceController.Instance.OnKeyDown(MachineButtonKey.BtnPayTable);

            }

            if (Input.GetKeyUp(KeyCode.F4)) 
            {
                MachineDeviceController.Instance.OnKeyUp(MachineButtonKey.BtnPayTable);
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {

                MachineDeviceController.Instance.OnKeyDown(MachineButtonKey.BtnExit);

            }

            if (Input.GetKeyUp(KeyCode.F5))
            {
                MachineDeviceController.Instance.OnKeyUp(MachineButtonKey.BtnExit);
            }


            if (Input.GetKeyDown(KeyCode.F6))
            {

                MachineDeviceController.Instance.OnKeyDown(MachineButtonKey.BtnPrev);

            }

            if (Input.GetKeyUp(KeyCode.F6))
            {
                MachineDeviceController.Instance.OnKeyUp(MachineButtonKey.BtnPrev);
            }


            if (Input.GetKeyDown(KeyCode.F7))
            {

                MachineDeviceController.Instance.OnKeyDown(MachineButtonKey.BtnNext);

            }

            if (Input.GetKeyUp(KeyCode.F7))
            {
                MachineDeviceController.Instance.OnKeyUp(MachineButtonKey.BtnNext);
            }

            if (Input.GetKeyDown(KeyCode.F8))
            {

                MachineDeviceController.Instance.OnKeyDown(MachineButtonKey.BtnPlayTime);

            }

            if (Input.GetKeyUp(KeyCode.F8))
            {
                MachineDeviceController.Instance.OnKeyUp(MachineButtonKey.BtnPlayTime);
            }



            if (Input.GetKeyDown(KeyCode.UpArrow)) // bet up
            {
                MachineDeviceController.Instance.OnKeyDown(MachineButtonKey.BtnBetUp);
            }
            if (Input.GetKeyUp(KeyCode.UpArrow)) // bet up
            {
                MachineDeviceController.Instance.OnKeyUp(MachineButtonKey.BtnBetUp);
            }

            if (Input.GetKeyDown(KeyCode.DownArrow)) // bet down
            {
                MachineDeviceController.Instance.OnKeyDown(MachineButtonKey.BtnBetDown);
            }
            if (Input.GetKeyUp(KeyCode.DownArrow)) // bet down
            {
                MachineDeviceController.Instance.OnKeyUp(MachineButtonKey.BtnBetDown);
            }

            if (Input.GetKeyDown(KeyCode.M)) // bet max
            {
                MachineDeviceController.Instance.OnKeyDown(MachineButtonKey.BtnBetMax);
            }
            if (Input.GetKeyUp(KeyCode.M)) // bet max
            {
                MachineDeviceController.Instance.OnKeyUp(MachineButtonKey.BtnBetMax);
            }

            /*
            if (Input.GetKeyDown(KeyCode.L)) // bet max
            {
                MachineDeviceController.Instance.OnKeyDown(MachineButtonKey.BtnSwitch);
            }
            if (Input.GetKeyUp(KeyCode.L)) // bet max
            {
                MachineDeviceController.Instance.OnKeyUp(MachineButtonKey.BtnSwitch);
            }


            if (Input.GetKeyDown(KeyCode.Z) )// bet max
            {
                MachineDeviceController.Instance.OnKeyDown(MachineButtonKey.BtnTicketOut);
            }
            if (Input.GetKeyUp(KeyCode.Z)) // bet max
            {
                MachineDeviceController.Instance.OnKeyUp(MachineButtonKey.BtnTicketOut);
            }

            if (Input.GetKeyDown(KeyCode.X))// bet max
            {
                MachineDeviceController.Instance.OnKeyDown(MachineButtonKey.BtnConsole);
            }
            if (Input.GetKeyUp(KeyCode.X)) // bet max
            {
                MachineDeviceController.Instance.OnKeyUp(MachineButtonKey.BtnConsole);
            }*/



            if (Input.GetKeyDown(KeyCode.RightArrow))
            {

            }
            if (Input.GetKeyUp(KeyCode.RightArrow))
            {

            }


            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {

            }
            if (Input.GetKeyUp(KeyCode.LeftArrow))
            {

            }

            if (Input.GetKeyDown(KeyCode.T))
            {

            }
            if (Input.GetKeyUp(KeyCode.T))
            {

            }

            if (Input.GetKeyDown(KeyCode.S))
            {

            }
            if (Input.GetKeyUp(KeyCode.S))
            {

            }



            #region 推币机后台按钮

            if (Input.GetKeyDown(KeyCode.Alpha5)) // bet max 推币机-选择按钮
            {
                MachineDeviceController.Instance.OnKeyDown(MachineButtonKey.BtnSpin);
            }
            if (Input.GetKeyUp(KeyCode.Alpha5)) // bet max 推币机-选择按钮
            {
                MachineDeviceController.Instance.OnKeyUp(MachineButtonKey.BtnSpin);
            }

            if (Input.GetKeyDown(KeyCode.Alpha6)) // bet max 推币机-选择按钮
            {
                MachineDeviceController.Instance.OnKeyDown(MachineButtonKey.BtnDown);
            }
            if (Input.GetKeyUp(KeyCode.Alpha6)) // bet max 推币机-选择按钮
            {
                MachineDeviceController.Instance.OnKeyUp(MachineButtonKey.BtnDown);
            }

            if (Input.GetKeyDown(KeyCode.Alpha7)) // bet max 推币机-选择按钮
            {
                MachineDeviceController.Instance.OnKeyDown(MachineButtonKey.BtnUp);
            }
            if (Input.GetKeyUp(KeyCode.Alpha7)) // bet max 推币机-选择按钮
            {
                MachineDeviceController.Instance.OnKeyUp(MachineButtonKey.BtnUp);
            }

            if (Input.GetKeyDown(KeyCode.Alpha8)) // BtnNext 推币机-下一个
            {
                MachineDeviceController.Instance.OnKeyDown(MachineButtonKey.BtnTicketOut);
            }
            if (Input.GetKeyUp(KeyCode.Alpha8)) // BtnNext 推币机-下一个
            {
                MachineDeviceController.Instance.OnKeyUp(MachineButtonKey.BtnTicketOut);
            }


            if (Input.GetKeyDown(KeyCode.Alpha9)) // BtnNext 推币机- 上一个
            {
                MachineDeviceController.Instance.OnKeyDown(MachineButtonKey.BtnSwitch);
            }
            if (Input.GetKeyUp(KeyCode.Alpha9)) // BtnNext 推币机-上一个
            {
                MachineDeviceController.Instance.OnKeyUp(MachineButtonKey.BtnSwitch);
            }


            if (Input.GetKeyDown(KeyCode.L)) // BtnNext 推币机- 退出
            {
                MachineDeviceController.Instance.OnKeyDown(MachineButtonKey.BtnConsole);
            }
            if (Input.GetKeyUp(KeyCode.L)) // BtnNext 推币机-退出
            {
                MachineDeviceController.Instance.OnKeyUp(MachineButtonKey.BtnConsole);
            }

            #endregion

        }
    }



}
