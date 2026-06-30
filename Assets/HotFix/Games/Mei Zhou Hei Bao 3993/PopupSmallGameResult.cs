using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    public class PopupSmallGameResult : MachinePageBase
    {
        public new const string pkgName = "MeiZhouHeiBao";
        public new const string resName = "PopupSmallGameResult";
        private const string PrefabPath = "Assets/GameRes/Games/Mei Zhou Hei Bao 3993/Prefabs/PopupSmallGameResult/";
        
        private int _totalCount;
        private GButton _closeBtn;
        private bool _isClicked;
        private GTextField _scoreText;
        private GameSoundController3993 _gameSoundController;
        
        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

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
            if (!isInit) return;
            preLoadedCallback?.Invoke();
            if (!isOpen) return;
            _isClicked = false;

            _closeBtn = contentPane.GetChild("closeBtn").asButton;
            _scoreText = contentPane.GetChild("scoreText").asTextField;
            _closeBtn.onClick.Clear();
            _closeBtn.onClick.Add(() => OnCloseBtn());
        }
        
        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);

            _gameSoundController = new GameSoundController3993();
            EventCenter.Instance.EventTrigger(SlotMachineEvent.ON_AUDIO_EVENT, new EventData(Game3993AudioEvent.BgmBonusResult));
            InitParam(eventData);
        }
        
        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);

            _gameSoundController?.Dispose();
            _gameSoundController = null;
        }
        
        private void ResLoadCallback()
        {
            if (--_totalCount != 0) return;

            isInit = true;
            InitParam(null);
        }

        private void OnCloseBtn(EventData eventData = null)
        {
            if (_isClicked) return;
            _isClicked = true;

            CloseSelf(eventData);
        }
    }
}