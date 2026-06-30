using FairyGUI;
using GameMaker;
using UnityEngine;

namespace CaiFuZhiJia_3997
{
    public class FreeSpinTimeController : IContorller
    {
        private GTextField freeSpinTime; // 免费游戏次数和免费游戏数据倍数

        public FreeSpinTimeController()
        {
            Init();
        }

        public void Init()
        {
            Dispose();
            EventCenter.Instance.AddEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, ChangFreeSpinTime);
        }

        public void InitParam(params object[] parameters) { }

        public void InitParam(GTextField gFreeSpinTimeTextField)
        {
            freeSpinTime = gFreeSpinTimeTextField;
        }

        public void Dispose()
        {
            EventCenter.Instance.RemoveEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, ChangFreeSpinTime);
        }

        private void ChangFreeSpinTime(EventData eventData = null)
        {
            if (eventData is { name: "ContentModel/ShowFreeSpinRemainTime" })
            {
                freeSpinTime.text = eventData.value.ToString();
            }
        }

        void IContorller.Dispose()
        {
            Dispose();
        }
    }
}