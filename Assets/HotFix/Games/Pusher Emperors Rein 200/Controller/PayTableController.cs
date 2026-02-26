using FairyGUI;
using GameMaker;
using Newtonsoft.Json;
using SlotMaker;
using System.Collections.Generic;
using UnityEngine;


namespace PusherEmperorsRein
{
    public class PayTableController
    {

       

        List<GComponent> goOwnerPayTableLst;
        GComponent paytable;
        float Lastmultiplier = 1;

        public void Init(List<GComponent> gPayTableLst)
        {
            Dispose();
            goOwnerPayTableLst = gPayTableLst;
            EventCenter.Instance.AddEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, OnPropertyChange);
        }

        public void Dispose()
        {
            EventCenter.Instance.RemoveEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, OnPropertyChange);
        }
        void OnPropertyChange(EventData res)
        {
            string name = res.name;
            switch (name)
            {
                case "ContentModel/totalBet":
                    OnPropertyChangeTotalBet(res);
                    break;
            }
        }

        public void OnPropertyChangeTotalBet(EventData res = null)
        {
            PayTable1Update();
        }


        void PayTable1Update()
        {
            return;
            for (int i = 1; i < 10; i++)
            {
                GComponent symbol = goOwnerPayTableLst[1].GetChild("n" + i).asCom;
                symbol.GetChild("n1").asTextField.text = (MainModel.Instance.contentMD.payTableSymbolWin[i].x5).ToString();
                symbol.GetChild("n2").asTextField.text = (MainModel.Instance.contentMD.payTableSymbolWin[i].x4).ToString();
                symbol.GetChild("n3").asTextField.text = (MainModel.Instance.contentMD.payTableSymbolWin[i].x3).ToString();

            }
        }


        /*
        public void OnPropertyChangeTotalBet(EventData res=null)
        {
            float multiplier = MainModel.Instance.contentMD.totalBet / 50f;
            // 不再修改 symbolInfo.x3、x4、x5 的值
            Lastmultiplier = multiplier;
            PayTable1Update();
        }


        void PayTable1Update()
        {
            float multiplier = MainModel.Instance.contentMD.totalBet / 50f;
            for (int i = 1; i < 10; i++)
            {
                GComponent symbol = goOwnerPayTableLst[1].GetChild("n" + i).asCom;
                symbol.GetChild("n1").asTextField.text = (MainModel.Instance.contentMD.payTableSymbolWin[i].x5 / Lastmultiplier * multiplier).ToString();
                symbol.GetChild("n2").asTextField.text = (MainModel.Instance.contentMD.payTableSymbolWin[i].x4 / Lastmultiplier * multiplier).ToString();
                symbol.GetChild("n3").asTextField.text = (MainModel.Instance.contentMD.payTableSymbolWin[i].x3 / Lastmultiplier * multiplier).ToString();

            }
        }
        */
    }

}