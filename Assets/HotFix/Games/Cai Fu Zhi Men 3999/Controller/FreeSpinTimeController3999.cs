using FairyGUI;
using GameMaker;

namespace CaiFuZhiMen_3999
{
    public class FreeSpinTimeController3999 : IContorller
    {
        private GTextField _freeSpinTime;

        public FreeSpinTimeController3999()
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

        public void InitParam(params object[] parameters) { }

        public void InitParam(GTextField gTextField)
        {
            _freeSpinTime = gTextField;
        }

        public void Dispose()
        {
            EventCenter.Instance.RemoveEventListener<EventData>(
                Observer.ON_PROPERTY_CHANGED_EVENT,
                ChangFreeSpinTime);
        }

        private void ChangFreeSpinTime(EventData eventData = null)
        {
            if (eventData is { name: "ContentModel/showFreeSpinRemainTime" })
            {
                _freeSpinTime.text =
                    eventData.value.ToString() + "/" + ContentModel.Instance.FreeSpinTotalTimes;
            }
        }

        void IContorller.Dispose()
        {
            Dispose();
        }
    }
}