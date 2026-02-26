using FairyGUI;
using GameMaker;
using System.Collections.Generic;

namespace CaiFuZhiMen_3999
{
    public class PayTableController3999
    {
        private List<GComponent> _goOwnerPayTableLst;

        public void Init(List<GComponent> gPayTableLst)
        {
            Dispose();
            _goOwnerPayTableLst = gPayTableLst;
            EventCenter.Instance.AddEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, OnPropertyChange);
        }

        private void Dispose()
        {
            EventCenter.Instance.RemoveEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, OnPropertyChange);
        }

        private void OnPropertyChange(EventData res)
        {
            string resName = res.name;
            switch (resName)
            {
                case "ContentModel/totalBet":
                    OnPropertyChangeTotalBet(res);
                    break;
            }
        }

        private void OnPropertyChangeTotalBet(EventData res = null)
        {
            PayTable1Update();
        }


        private void PayTable1Update()
        {
            return;
            for (int i = 1; i < 10; i++)
            {
                GComponent symbol = _goOwnerPayTableLst[1].GetChild("n" + i).asCom;
                symbol.GetChild("n1").asTextField.text =
                    (MainModel.Instance.contentMD.payTableSymbolWin[i].x5).ToString();
                symbol.GetChild("n2").asTextField.text =
                    (MainModel.Instance.contentMD.payTableSymbolWin[i].x4).ToString();
                symbol.GetChild("n3").asTextField.text =
                    (MainModel.Instance.contentMD.payTableSymbolWin[i].x3).ToString();
            }
        }
    }
}