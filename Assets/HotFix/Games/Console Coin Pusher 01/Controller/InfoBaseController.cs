using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameMaker;
using SlotMaker;
using FairyGUI;

namespace ConsoleCoinPusher01
{
    public class InfoBaseController
    {

        public InfoBaseController()
        {
            Init();
        }

        public void Init()
        {
            EventCenter.Instance.AddEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, OnPropertyChange);
        }

        public  void Dispose()
        {
            EventCenter.Instance.RemoveEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, OnPropertyChange);
        }




        public GComponent goOwnerBase;
        GRichTextField  rtxtTitle, rtxtSeatID, rtxtGroupId, rtxtHardwareVer, rtxtAlgorithmVer, rtxtSoftwareVer;



        public string title
        {
            get
            {
                if (rtxtTitle == null)
                    rtxtTitle = goOwnerBase.GetChild("title").asRichTextField;
                return rtxtTitle.text;
            }
            set
            {
                rtxtTitle.text = value; 
            }
        }
       


        public void InitParam(GComponent gBase , string title)
        {
            if (gBase == null) return;

            goOwnerBase = gBase;


            rtxtTitle = goOwnerBase.asCom.GetChild("title").asRichTextField;

            rtxtSeatID = goOwnerBase.asCom.GetChild("seatId").asRichTextField;
            rtxtGroupId = goOwnerBase.asCom.GetChild("groupId").asRichTextField;
            rtxtHardwareVer = goOwnerBase.asCom.GetChild("hardwareVer").asRichTextField;
            rtxtAlgorithmVer = goOwnerBase.asCom.GetChild("algorithmVer").asRichTextField;
            rtxtSoftwareVer = goOwnerBase.asCom.GetChild("softwareVer").asRichTextField;


            rtxtTitle.text = title;
            rtxtSeatID.text = string.Format("机台座位号: {0}", SBoxModel.Instance.seatId);
            rtxtGroupId.text = string.Format("机台组号: {0}", SBoxModel.Instance.groupId);
            rtxtHardwareVer.text = I18nMgr.T("Hardware Ver:") + SBoxModel.Instance.HardwareVer;

            rtxtAlgorithmVer.text = I18nMgr.T("Algorithm Ver:") + SBoxModel.Instance.AlgorithmVer;
            rtxtSoftwareVer.text = I18nMgr.T("Software Ver:") + GlobalData.hotfixVersion;

        }

        void OnPropertyChange(EventData res) {

            string name = res.name;
            switch (name)
            {
                case "SBoxModel/groupId":
                    rtxtGroupId.text = string.Format("机台组号: {0}", SBoxModel.Instance.groupId);
                    break;
                case "SBoxModel/seatId":
                    rtxtSeatID.text = string.Format("机台座位号: {0}", SBoxModel.Instance.seatId);
                    break;
            }
        }



    }
}
