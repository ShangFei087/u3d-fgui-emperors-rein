using FairyGUI;
using GameMaker;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CaiFuZhiJia_3997
{
    public class PopupJackpotWin : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiJia";
        public new const string resName = "PopupJackpotWin";

        private const string SpinePrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupJackpotWin/SpinePrefabs/";

        private int _totalCount = -1;
        private bool _isInitialized = false;

        private GComponent _compareJackpotWinGCom;
        private GameObject _jackpotWinObj, _cloneJackpotWinObj;

        private GButton _collectBtn;
        private GTextField _winBetText;

        // ========== 新增：记录原始父节点，用于还原 ==========
        private Transform _winCollectBtnOriginalParent = null;
        private Vector3 _winCollectBtnOriginalPos;
        private Vector3 _winCollectBtnOriginalScale;
        private Transform _jackpotWinBetOriginalParent = null;
        private Vector3 _jackpotWinBetOriginalPos;
        private Vector3 _jackpotWinBetOriginalScale;
        // ========== 新增结束 ==========

        private bool _isClicked = false;

        private EventData _jackpotData;
        private TimerCallback _delayVisibleCallback;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            _totalCount = 1;
            // 加载Spine
            ResourceManager02.Instance.LoadAsset<GameObject>(
                SpinePrefabPath + "JackpotWin.prefab",
                (clone) =>
                {
                    _jackpotWinObj = clone;
                    ResLoadedCallback();
                });

            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        if (SlotMaker.PanelBaseController.ShouldBlockPhysicalSpinInput)
                            return;

                        Debug.LogError("游戏接受到机台短按的数据：Spin");
                        EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, false);
                        OnClickSpinButton(res);
                    },
                }
            };
        }

        public void InitParam(EventData eventData)
        {
            if (eventData != null) _jackpotData = eventData;
            if (!_isInitialized) return;
            preLoadedCallback?.Invoke();
            if (!isOpen) return;
            _isClicked = false;

            _collectBtn = contentPane.GetChild("winCollectBtn").asButton;
            _winBetText = contentPane.GetChild("jackpotWinBet").asTextField;
            _collectBtn.visible = false;
            _winBetText.visible = false;
            
            // Spine
            GComponent currentGCom = contentPane.GetChild("anchor_JackpotWin").asCom;
            if (currentGCom != _compareJackpotWinGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareJackpotWinGCom);
                _compareJackpotWinGCom = currentGCom;
                _cloneJackpotWinObj = Object.Instantiate(_jackpotWinObj);
                GameCommon.FguiUtils.AddWrapper(_compareJackpotWinGCom, _cloneJackpotWinObj);
            }

            int type = -1;
            if (_jackpotData is { value: Dictionary<string, object> args })
            {
                for (int i = 0; i < _cloneJackpotWinObj.transform.GetChild(0).childCount; i++)
                {
                    _cloneJackpotWinObj.transform.GetChild(0).GetChild(i).gameObject
                        .SetActive(i == int.Parse(args["jackpotWinType"].ToString()));
                }

                type = int.Parse(args["jackpotWinType"].ToString());
                _winBetText.text = args["jackpotWinBet"].ToString();
            }

            RemoveTimer(ref _delayVisibleCallback);

            _delayVisibleCallback = (obj) =>
            {
                _winBetText.visible = true;
                _collectBtn.visible = true;
                if (TestManager.Instance.IsAutoModeRunning && _collectBtn != null && _collectBtn.visible)
                {
                    _collectBtn.onClick.Call();
                }

                _delayVisibleCallback = null;
            };
            Timers.inst.Add(0.5f, 1, _delayVisibleCallback);
            _collectBtn.onClick.Clear();
            _collectBtn.onClick.Add(() => OnClickSpinButton(eventData));

            

            // ========== 修改：绑定前先记录原始状态，方便后续还原 ==========
            GameObject parentObj = _cloneJackpotWinObj.transform.GetChild(0).GetChild(type).gameObject;
            string parentPath = $"anim/01/btn";
            Transform animatorParent = parentObj.transform.Find(parentPath);
            GObject gObject = contentPane.GetChild("winCollectBtn");
            if (gObject?.displayObject?.gameObject != null)
            {
                Transform t = gObject.displayObject.gameObject.transform;

                if (_winCollectBtnOriginalParent == null)
                {
                    _winCollectBtnOriginalParent = t.parent;
                    _winCollectBtnOriginalPos = t.localPosition;
                    _winCollectBtnOriginalScale = t.localScale;
                }

                t.SetParent(animatorParent, false);
                t.localPosition = new Vector3(-1.61f, 0.34f, 0);
                t.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            }

            parentPath = $"anim/01/num";
            animatorParent = parentObj.transform.Find(parentPath);
            gObject = contentPane.GetChild("jackpotWinBet");
            if (gObject?.displayObject?.gameObject != null)
            {
                Transform t = gObject.displayObject.gameObject.transform;

                if (_jackpotWinBetOriginalParent == null)
                {
                    _jackpotWinBetOriginalParent = t.parent;
                    _jackpotWinBetOriginalPos = t.localPosition;
                    _jackpotWinBetOriginalScale = t.localScale;
                }

                t.SetParent(animatorParent, false);
                t.localPosition = new Vector3(-1.76f, 0.85f, 0);
                t.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            }
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);
            InitParam(eventData);
        }

        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);
            RemoveTimer(ref _delayVisibleCallback);

            if (contentPane == null) return;
            GObject gObject = contentPane.GetChild("winCollectBtn");
            if (gObject?.displayObject?.gameObject != null && _winCollectBtnOriginalParent != null)
            {
                Transform t = gObject.displayObject.gameObject.transform;
                t.SetParent(_winCollectBtnOriginalParent, false);
                t.localPosition = _winCollectBtnOriginalPos;
                t.localScale = _winCollectBtnOriginalScale;
            }

            gObject = contentPane.GetChild("jackpotWinBet");
            if (gObject?.displayObject?.gameObject != null && _jackpotWinBetOriginalParent != null)
            {
                Transform t = gObject.displayObject.gameObject.transform;
                t.SetParent(_jackpotWinBetOriginalParent, false);
                t.localPosition = _jackpotWinBetOriginalPos;
                t.localScale = _jackpotWinBetOriginalScale;
            }

            _winCollectBtnOriginalParent = null;
            _jackpotWinBetOriginalParent = null;
        }

        private void OnClickSpinButton(EventData res)
        {
            if (_isClicked) return;
            _isClicked = true;
            CloseSelf(null);
        }

        private void ResLoadedCallback()
        {
            if (--_totalCount == 0)
            {
                _isInitialized = true;
                InitParam(null);
            }
        }

        private void RemoveTimer(ref TimerCallback timerCallback)
        {
            if (timerCallback == null) return;

            Timers.inst.Remove(timerCallback);
            timerCallback = null;
        }
    }
}