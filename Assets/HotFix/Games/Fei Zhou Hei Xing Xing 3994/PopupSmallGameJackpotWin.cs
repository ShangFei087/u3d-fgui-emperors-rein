using FairyGUI;
using GameMaker;
using HotFix.Games.Fei_Zhou_Hei_Xing_Xing_3994.Custom;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FeiZhouHeiXingXing_3994
{
    public class PopupSmallGameJackpotWin : MachinePageBase
    {
        public new const string pkgName = "FeiZhouHeiXingXing";
        public new const string resName = "PopupSmallGameJackpotWin";

        private const string PrefabPath =
            "Assets/GameRes/Games/Fei Zhou Hei Xing Xing 3994/Prefabs/PopupSmallGameJackpotWin/";

        private int _totalCount;
        private bool _isClicked;
        private GButton _collectBtn;
        private GTextField _scoreText;

        private Animator _jackpotAni;
        private GComponent _compareJackpot;
        private GameObject _miniWinObj, _minorWinObj, _majorWinObj, _cloneJackpotWinObj;

        // 挂点记录初始数据，方便后续还原
        private Quaternion _collectBtnQuaternion, _scoreQuaternion;
        private Vector3 _btnScale, _btnPos, _scoreScale, _scorePos;
        private Transform _collectBtnTran, _scoreTran, _parentTran;

        private Dictionary<BonusResultType, GameObject> _jackpotResultDic;

        BonusResultType _resultType;
        private int _winScore;

        /// <summary>当前实例绑定的语言，切语言时强制重绑。</summary>
        private I18nLang _boundLang;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            _totalCount = 3;
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Spine_MiniWin.prefab",
                (clone) =>
                {
                    _miniWinObj = clone;
                    ResLoadCallback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Spine_MinorWin.prefab",
                (clone) =>
                {
                    _minorWinObj = clone;
                    ResLoadCallback();
                });
            ResourceManager02.Instance.LoadAsset<GameObject>(PrefabPath + "Spine_MajorWin.prefab",
                (clone) =>
                {
                    _majorWinObj = clone;
                    ResLoadCallback();
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
                        OnCollectBtnClick(res);
                    },
                }
            };
        }

        private void InitParam(EventData eventData)
        {
            if (!isInit) return;
            preLoadedCallback?.Invoke();
            if (!isOpen) return;
            _isClicked = false; // 重置按钮点击状态

            _jackpotResultDic = new Dictionary<BonusResultType, GameObject>()
            {
                [BonusResultType.Mini] = _miniWinObj,
                [BonusResultType.Minor] = _minorWinObj,
                [BonusResultType.Major] = _majorWinObj,
            };

            // 获取UI组件
            _collectBtn = contentPane.GetChild("collectBtn").asButton;
            _scoreText = contentPane.GetChild("scoreText").asTextField;
            _scoreText.text = _winScore.ToString();

            // 保存初始信息
            _parentTran = contentPane.displayObject.gameObject.transform;
            _collectBtnTran = _collectBtn.displayObject.gameObject.transform;
            _scoreTran = _scoreText.displayObject.gameObject.transform;
            _btnScale = _collectBtnTran.localScale;
            _btnPos = _collectBtnTran.localPosition;
            _collectBtnQuaternion = _collectBtnTran.localRotation;
            _scoreScale = _scoreTran.localScale;
            _scorePos = _scoreTran.localPosition;
            _scoreQuaternion = _scoreTran.localRotation;

            // 绑定Spine
            GComponent currentCom = contentPane.GetChild("anchorJackpot").asCom;
            if (currentCom != _compareJackpot || _boundLang != PopupLang3994.CurrentLang)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareJackpot);
                _compareJackpot = currentCom;
                _cloneJackpotWinObj = Object.Instantiate(_jackpotResultDic[_resultType]);
                PopupLang3994.Apply(_cloneJackpotWinObj);
                _boundLang = PopupLang3994.CurrentLang;
                _jackpotAni = _cloneJackpotWinObj.GetComponentInChildren<Animator>();
                GameCommon.FguiUtils.AddWrapper(currentCom, _cloneJackpotWinObj);
            }

            // 将UI挂载在Spine动画上
            GameObject fatherObj = _cloneJackpotWinObj.transform.GetChild(0).gameObject;
            string path = "Spine Mecanim GameObject (sg_bor_congrats3)/SkeletonUtility-SkeletonRoot/root/bone/";
            Transform father = fatherObj.transform.Find(path + "collect");
            _collectBtnTran.SetParent(father, false);
            _collectBtnTran.localPosition = new Vector3(-2.12f, 1.04f, 0.01f);
            _collectBtnTran.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            _collectBtnTran.localRotation = Quaternion.Euler(0, 0, 0);

            father = fatherObj.transform.Find(path + "k/number");
            _scoreTran.SetParent(father, false);
            _scoreTran.localPosition = new Vector3(-4.39f, 1.02f, 0f);
            _scoreTran.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            _scoreTran.localRotation = Quaternion.Euler(0, 0, 0);

            _collectBtn.onClick.Clear();
            _collectBtn.onClick.Add(() => OnCollectBtnClick(null));
        }


        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);

            // 获取事件信息
            if (eventData is { value: Dictionary<string, object> args })
            {
                _resultType = (BonusResultType)args["resultType"];
                _winScore = (int)args["winScore"];
            }

            InitParam(eventData);
        }

        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);

            // 解除UI绑定
            _collectBtnTran.SetParent(_parentTran);
            _collectBtnTran.localPosition = _btnPos;
            _collectBtnTran.localScale = _btnScale;
            _collectBtnTran.localRotation = _collectBtnQuaternion;
            _scoreTran.SetParent(_parentTran);
            _scoreTran.localPosition = _scorePos;
            _scoreTran.localScale = _scoreScale;
            _scoreTran.localRotation = _scoreQuaternion;
        }

        private void ResLoadCallback()
        {
            if (--_totalCount != 0) return;

            isInit = true;
            InitParam(null);
        }

        private void OnCollectBtnClick(EventData eventData = null)
        {
            if (_isClicked) return;
            _isClicked = true;
            PlayAnimationByName(_jackpotAni, "end", () => CloseSelf(null));
        }

        private void PlayAnimationByName(Animator animator, string aniName, Action callback = null)
        {
            animator.Rebind();
            animator.Play(aniName);
            animator.Update(0f);
            callback?.Invoke();
        }
    }
}