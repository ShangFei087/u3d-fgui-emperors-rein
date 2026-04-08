using FairyGUI;
using GameMaker;

namespace MeiZhouHeiBao_3993
{
    public class FreeSpinTimeController3993 : IContorller
    {
        private GTextField _freeSpinTime;
        
        public FreeSpinTimeController3993()
        {
            Init();
        }

        public void Init()
        {
            Dispose();
            EventCenter.Instance.AddEventListener<EventData>(
                Observer.ON_PROPERTY_CHANGED_EVENT,
                ChangFreeSpinTime);
        }

        public void InitParam(params object[] parameters)
        {
            throw new System.NotImplementedException();
        }
        
        public void InitParam(GTextField gFreeSpinTimeTextField)
        {
            _freeSpinTime = gFreeSpinTimeTextField;
        }

        public void Dispose()
        {
            EventCenter.Instance.RemoveEventListener<EventData>(
                Observer.ON_PROPERTY_CHANGED_EVENT,
                ChangFreeSpinTime);
        }
        
        public void ChangFreeSpinTime(EventData eventData = null)
        {
            if (eventData.name == "ContentModel/ShowFreeSpinRemainTime")
            {
                _freeSpinTime.text = eventData.value.ToString();
            }
        }
        
        void IContorller.Dispose()
        {
            Dispose();
        }
    }
}