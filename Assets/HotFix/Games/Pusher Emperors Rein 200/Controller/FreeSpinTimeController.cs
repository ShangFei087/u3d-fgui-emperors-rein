using FairyGUI;
using GameMaker;
using SlotMaker;

namespace PusherEmperorsRein
{


    public class FreeSpinTimeController  : IContorller
    {
        GTextField FreeSpinTime, FreeOnceCredit;


        /*
        private float _score; // 私有字段，实际存储分数值

        public float Score
        {
            get { return _score; } // 获取私有字段的值
            set
            {
                _score = value; // 将值存入私有字段（关键：避免递归）

                // 当分数为0时更新UI
                if (_score == 0)
                {
                    FreeOnceCredit.text = _score.ToString();
                }
            }
        }
        */

        //public void Init()  {  }
        //public void InitParam(params object[] parameters) { };

        public FreeSpinTimeController()
        {
            Init();
        }

        public void Init()
        {

            Dispose();
            // 注册事件监听
            EventCenter.Instance.AddEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, OnPropertyChangedEvent);

            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_WIN_EVENT, OnFreeSpinWin);

            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_CONTENT_EVENT, OContentEvent);
        }

        public void InitParam(params object[] parameters) { }

        public void InitParam(GTextField gTextField, GTextField FreeOnceCredit)
        {
            FreeSpinTime = gTextField;
            FreeSpinTime.text = "0";

            this.FreeOnceCredit = FreeOnceCredit;
            this.FreeOnceCredit.text = "0";
        }
        // Start is called before the first frame update


        // Update is called once per frame
        public void Dispose()
        {
            EventCenter.Instance.RemoveEventListener<EventData>(  Observer.ON_PROPERTY_CHANGED_EVENT, OnPropertyChangedEvent);

            EventCenter.Instance.RemoveEventListener<EventData>( SlotMachineEvent.ON_WIN_EVENT, OnFreeSpinWin);

            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_CONTENT_EVENT, OContentEvent);
        }

        public void OnPropertyChangedEvent(EventData eventData = null)
        {
            if (eventData.name == "ContentModel/showFreeSpinRemainTime")
            {
                FreeSpinTime.text = eventData.value.ToString();
            }
        }

        public void OnFreeSpinWin(EventData res)
        {

            if (ContentModel.Instance.isFreeSpin &&  res.name == SlotMachineEvent.TotalWinCredit)
            {
                //DebugUtils.LogError($" i am here 11111 {ContentModel.Instance.freeSpinTotalWinCoins}");
                FreeOnceCredit.text = $"{ContentModel.Instance.freeSpinTotalWinCoins}";
                //NumberAnimation.Instance.AnimateNumber(FreeOnceCredit, Score, (float)eventData.value + Score);
                //Score += (float)eventData.value;
            }
        }

        public void OContentEvent(EventData res)
        {
            if (res.name == SlotMachineEvent.BeginBonus && (string)res.value == "FreeSpin")
            {
                FreeOnceCredit.text = "0";
            }
        }

        void IContorller.Dispose()
        {
            Dispose();
        }

    }

}