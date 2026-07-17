using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FeiZhouHeiXingXing_3994
{
    public class PopupFreeSpinTrigger : MachinePageBase
    {
        public new const string pkgName = "FeiZhouHeiXingXing";
        public new const string resName = "PopupFreeSpinTrigger";

        private const string PrefabPath =
            "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Prefabs/PopupFreeSpinTrigger/";

        private int _totalCount;
        private GButton _closeBtn;
        private GTextField _spinCountText;
        private bool _isClicked;
        private GameSoundController3994 _gameSoundController;

        private EventData _openData;
        private TimerCallback _delayCloseCallback;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();
            
            // 测试使用
            isInit = true;

            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        if (PanelBaseController.ShouldBlockPhysicalSpinInput)
                            return;

                        Debug.LogError("游戏接受到机台短按的数据：Spin");
                        EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, false);
                        OnCloseBtn(res);
                    },
                }
            };
        }

        private void InitParam(EventData eventData)
        {
            if (eventData != null) _openData = eventData;
            if (!isInit) return;
            preLoadedCallback?.Invoke();
            if (!isOpen) return;
            _isClicked = false;

            _closeBtn = contentPane.GetChild("closeBtn").asButton;
            _spinCountText = contentPane.GetChild("spinCountText").asTextField;
            if (_openData is { value: Dictionary<string, object> args })
            {
                _spinCountText.text = args["freeSpinCount"].ToString();
            }

            _closeBtn.onClick.Clear();
            _closeBtn.onClick.Add(() => OnCloseBtn(_openData));
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);
            _gameSoundController = new GameSoundController3994();
            EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT,
                new EventData(Game3994AudioEvent.BgmFreeSpinTrigger));
            InitParam(eventData);
        }

        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);
            _gameSoundController?.Dispose();
            _gameSoundController = null;
        }

        private void ResLoadCallback(EventData eventData = null)
        {
            if (--_totalCount != 0) return;
            isInit = true;
            InitParam(eventData);
        }

        private void OnCloseBtn(EventData eventData = null)
        {
            if (_isClicked) return;
            _isClicked = true;
            if (_delayCloseCallback != null)
            {
                Timers.inst.Remove(_delayCloseCallback);
                _delayCloseCallback = null;
            }

            _delayCloseCallback = (obj) =>
            {
                if (eventData is { value: Dictionary<string, object> args })
                {
                    Action changePage = args["changeFreePage"] as Action;
                    changePage?.Invoke();
                }
                if (isOpen) CloseSelf(null);
                _delayCloseCallback = null;
            };
            Timers.inst.Add(2f, 1, _delayCloseCallback);
            CloseSelf(eventData);
        }
    }
}