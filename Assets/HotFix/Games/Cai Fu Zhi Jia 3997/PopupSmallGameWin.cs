using FairyGUI;
using GameMaker;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CaiFuZhiJia_3997
{
    public class PopupSmallGameWin : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiJia";
        public new const string resName = "PopupSmallGameWin";

        private const string PrefabPath = "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupSmallGameWin/";

        private int _totalCount = -1;
        private bool _isInitialized;

        private GComponent _compareJackpotWinGCom;
        private GameObject _smallWinObj, _cloneSmallWinObj;

        private GButton _collectBtn;
        private GTextField _winBetText;

        private bool _isClicked;
        private EventData _jackpotData;
        private TimerCallback _delayVisibleCallback;

        // 记录UI初始位置
        private Vector3 _collectBtnLocalScale, _numTextLocalScale, _collectBtnLocalPos, _numTextLocalPos;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            _totalCount = 1;
            // 加载Spine
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "BonusWin.prefab",
                (clone) =>
                {
                    _smallWinObj = clone;
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
            RemoveTimer(ref _delayVisibleCallback);

            // --------------------- 获取UI组件 ------------------------
            _collectBtn = contentPane.GetChild("winCollectBtn").asButton;
            _winBetText = contentPane.GetChild("jackpotWinBet").asTextField;

            // -------------------------- 记录UI初始位置 -----------------------
            Transform collectBtnTran = _collectBtn.displayObject.gameObject.transform;
            Transform numTxt = _winBetText.displayObject.gameObject.transform;
            _collectBtnLocalPos = collectBtnTran.localPosition;
            _collectBtnLocalScale = collectBtnTran.localScale;
            _numTextLocalPos = numTxt.localPosition;
            _numTextLocalScale = numTxt.localScale;

            // ----------------------- 绑定Prefab到UI -------------------------
            GComponent currentGCom = contentPane.GetChild("BonusWin").asCom;
            if (currentGCom != _compareJackpotWinGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareJackpotWinGCom);
                _compareJackpotWinGCom = currentGCom;
                _cloneSmallWinObj = Object.Instantiate(_smallWinObj);
                GameCommon.FguiUtils.AddWrapper(_compareJackpotWinGCom, _cloneSmallWinObj);
            }

            // ------------------ 将UI组件挂点到对应的Spine节点上 -----------------------
            int type = -1;
            if (_jackpotData is { value: Dictionary<string, object> args })
            {
                for (int i = 0; i < _cloneSmallWinObj.transform.GetChild(0).childCount; i++)
                {
                    _cloneSmallWinObj.transform.GetChild(0).GetChild(i).gameObject
                        .SetActive(i == int.Parse(args["jackpotWinType"].ToString()));
                }

                type = int.Parse(args["jackpotWinType"].ToString());
                _winBetText.text = args["jackpotWinBet"].ToString();
            }

            GameObject parentObj = _cloneSmallWinObj.transform.GetChild(0).GetChild(type).gameObject;
            string rootPath = "SkeletonUtility-SkeletonRoot/root/all/";
            Transform btnTran = parentObj.transform.Find(rootPath + "btn");
            collectBtnTran.SetParent(btnTran, false);
            collectBtnTran.localPosition = new Vector3(-1.95f, 0.8f, 0);
            collectBtnTran.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            Transform numTran = parentObj.transform.Find(rootPath + "lankuang2/num");
            numTxt.SetParent(numTran, false);
            numTxt.localPosition = new Vector3(-5.32f,1.26f, 0);
            numTxt.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            // ----------------------- 按钮点击事件 -------------------------
            _delayVisibleCallback = (obj) =>
            {
                if (TestManager.Instance.IsAutoModeRunning && _collectBtn is { visible: true })
                {
                    _collectBtn.onClick.Call();
                }

                _delayVisibleCallback = null;
            };
            Timers.inst.Add(0.5f, 1, _delayVisibleCallback);
            _collectBtn.onClick.Clear();
            _collectBtn.onClick.Add(() => OnClickSpinButton(eventData));
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);
            InitParam(eventData);
        }

        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);
            
            // 清除回调
            RemoveTimer(ref _delayVisibleCallback);
            
            // 还原UI位置
            Transform startBtnTran = _collectBtn.displayObject.gameObject.transform;
            Transform numTxt = _winBetText.displayObject.gameObject.transform;
            Transform parentTran = contentPane.displayObject.gameObject.transform;
            startBtnTran.SetParent(parentTran, false);
            startBtnTran.localPosition = _collectBtnLocalPos;
            startBtnTran.localScale = _collectBtnLocalScale;
            numTxt.SetParent(parentTran, false);
            numTxt.localPosition = _numTextLocalPos;
            numTxt.localScale = _numTextLocalScale;
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