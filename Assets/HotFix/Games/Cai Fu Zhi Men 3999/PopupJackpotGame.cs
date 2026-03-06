using FairyGUI;
using GameMaker;
using PusherEmperorsRein;
using SimpleJSON;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace CaiFuZhiMen_3999
{
    public class PopupJackpotGame : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiMen";
        public new const string resName = "PopupJackpotGame";

        private const string SpinePrefabsPath =
            "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/PopupJackpotGame/SpinePrefabs/";

        private const string EffectPrefabsPath =
            "Assets/GameRes/Games/Cai Fu Zhi Men 3999/Prefabs/PopupJackpotGame/EffectPrefabs/";

        private int _resCount = -1;
        private bool _isInitialized = false;
        private bool _isCanClick = true;

        private GGraph _coverMask;

        private readonly MiniReelGroup _uiJpMajorCtrl = new MiniReelGroup();
        private readonly MiniReelGroup _uiJpMinorCtrl = new MiniReelGroup();
        private readonly MiniReelGroup _uiJpMiniCtrl = new MiniReelGroup();

        private readonly List<ClickIcon> _clickIconList = new List<ClickIcon>();
        private readonly List<GComponent> _winList = new List<GComponent>();

        private GameObject _grandObj, _majorObj, _miniObj, _minorObj, _goldCoinObj;

        private readonly List<GComponent> _compareGoldCoinList = new List<GComponent>();
        private readonly List<GComponent> _compareJackpotObjList = new List<GComponent>();
        private readonly List<GComponent> _compareWinList = new List<GComponent>();

        private readonly List<GameObject> _cloneJackpotObjs = new List<GameObject>();
        private readonly List<GameObject> _cloneWinObjs = new List<GameObject>();

        private Dictionary<string, CountClick> _countClickDic;

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            LoadResAsync();
        }

        public override void InitParam()
        {
            if (!_isInitialized) return;
            preLoadedCallback?.Invoke();
            if (!isOpen) return;
            BindPrefabsToUI();
            ShowJackpotData();
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            GameSoundHelper3999.Instance.StopSound(SoundKey.RegularBG);
            GameSoundHelper3999.Instance.PlayMusicSingle(SoundKey.JackpotBG);
            _countClickDic = new Dictionary<string, CountClick>()
            {
                { "Grand(Clone)", new CountClick(0, new List<int>()) },
                { "Major(Clone)", new CountClick(0, new List<int>()) },
                { "Minor(Clone)", new CountClick(0, new List<int>()) },
                { "Mini(Clone)", new CountClick(0, new List<int>()) }
            };

            base.OnOpen(currentPageName, eventData);
            InitUICom();
            RandomSetWinningType();
            GameMain();
            InitParam();
        }

        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);
            ResetPage();
            GameSoundHelper3999.Instance.StopSound(SoundKey.JackpotBG);
        }

        private void ResLoadedCallback()
        {
            if (--_resCount == 0)
            {
                _isInitialized = true;
                InitParam();
            }
        }

        private void LoadResAsync()
        {
            _resCount = 5;

            // 加载Spine动画
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "Grand.prefab", (cloneObj) =>
            {
                _grandObj = cloneObj;
                ResLoadedCallback();
            });
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "Major.prefab", (cloneObj) =>
            {
                _majorObj = cloneObj;
                ResLoadedCallback();
            });
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "Mini.prefab", (cloneObj) =>
            {
                _miniObj = cloneObj;
                ResLoadedCallback();
            });
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinePrefabsPath + "Minor.prefab", (cloneObj) =>
            {
                _minorObj = cloneObj;
                ResLoadedCallback();
            });

            // 加载Effect
            ResourceManager02.Instance.LoadAsset<GameObject>(EffectPrefabsPath + "GoldCoin.prefab", (cloneObj) =>
            {
                _goldCoinObj = cloneObj;
                ResLoadedCallback();
            });
        }

        private void InitUICom()
        {
            _coverMask = contentPane.GetChild("cover").asGraph;

            for (int i = 0; i < 12; i++)
            {
                // 获取面板中的 ClickIcon结构
                GComponent clickIcon = contentPane.asCom.GetChild("clickIcon_" + i).asCom;
                GComponent anchorLight = clickIcon.GetChild("anchor_Light").asCom; // 按钮上面的金币特效
                GComponent anchorWinningType = clickIcon.GetChild("anchor_WinningType").asCom; // 随机出来的大奖类型
                GButton gameBtn = clickIcon.GetChild("gameBtn").asButton;

                // 添加按钮事件
                gameBtn.onRollOver.Add(() => OnBatchBtnRollOver(gameBtn));
                gameBtn.onRollOut.Add(() => OnBatchBtnRollOut(gameBtn));


                // 存储UI信息 按钮，点击播放特效以及对应的Spine动画
                ClickIcon tempIcon = new ClickIcon
                {
                    anchorLight = anchorLight, anchorWinningType = anchorWinningType, gameBtn = gameBtn
                };
                _clickIconList.Add(tempIcon);
                _compareGoldCoinList.Add(null);
                _compareJackpotObjList.Add(null);
                _compareWinList.Add(null);

                // 存储中奖播放特效锚点，主要用作播放中奖特效
                GComponent currentCom = contentPane.asCom.GetChild("anchor_Winning_" + i).asCom;
                _winList.Add(currentCom);
            }
        }

        private void BindPrefabsToUI()
        {
            // 绑定每个彩金点击前的金币特效
            for (int i = 0; i < _clickIconList.Count; i++)
            {
                GComponent currentCom = _clickIconList[i].anchorLight;
                if (currentCom == _compareGoldCoinList[i])
                {
                    continue;
                }

                GameCommon.FguiUtils.DeleteWrapper(_compareGoldCoinList[i]);
                _compareGoldCoinList[i] = currentCom;
                GameObject tempObj = Object.Instantiate(_goldCoinObj);
                GameCommon.FguiUtils.AddWrapper(_compareGoldCoinList[i], tempObj);
            }
        }

        /// <summary>
        /// 随机设置四个奖项的位置
        /// </summary>
        private void RandomSetWinningType()
        {
            int[] prefabRemainingCount = new int[4] { 3, 3, 3, 3 };
            List<GameObject> jackpotObjList = new List<GameObject>() { _grandObj, _majorObj, _minorObj, _miniObj };
            Random random = new Random();
            for (int i = 0; i < _clickIconList.Count; i++)
            {
                List<int> availablePrefabIndices = new List<int>();
                for (int j = 0; j < prefabRemainingCount.Length; j++)
                {
                    if (prefabRemainingCount[j] > 0)
                    {
                        availablePrefabIndices.Add(j);
                    }
                }

                if (availablePrefabIndices.Count == 0)
                {
                    Debug.LogError("无可用预制体可分配，跳过位置" + i);
                    continue;
                }

                int randomPrefabIndex = availablePrefabIndices[random.Next(0, availablePrefabIndices.Count)];
                GameObject selectedPrefab = jackpotObjList[randomPrefabIndex];
                prefabRemainingCount[randomPrefabIndex]--;

                // 本局点击按钮显示的Spine动画
                if (_clickIconList[i].anchorWinningType != _compareJackpotObjList[i])
                {
                    GameCommon.FguiUtils.DeleteWrapper(_compareJackpotObjList[i]);
                    _compareJackpotObjList[i] = _clickIconList[i].anchorWinningType;

                    GameObject currentObj = Object.Instantiate(selectedPrefab);
                    currentObj.SetActive(false);
                    _cloneJackpotObjs.Add(currentObj);

                    GameCommon.FguiUtils.AddWrapper(_compareJackpotObjList[i], currentObj);
                }

                // 中奖之后显示在遮罩上层的Spine动画
                if (_winList[i] != _compareWinList[i])
                {
                    GameCommon.FguiUtils.DeleteWrapper(_compareWinList[i]);
                    _compareWinList[i] = _winList[i];

                    GameObject currentObj = Object.Instantiate(selectedPrefab);
                    currentObj.SetActive(false);
                    _cloneWinObjs.Add(currentObj);

                    GameCommon.FguiUtils.AddWrapper(_compareWinList[i], currentObj);
                }
            }
        }

        private void GameMain()
        {
            for (int i = 0; i < _clickIconList.Count; i++)
            {
                int currentIndex = i;
                _clickIconList[i].gameBtn.onClick.Add(() =>
                {
                    if (!_isCanClick) return;

                    _cloneJackpotObjs[currentIndex].SetActive(true);
                    _clickIconList[currentIndex].gameBtn.visible = false;
                    _isCanClick = false;
                    Timers.inst.Add(1f, 1, (obj =>
                    {
                        _isCanClick = true;
                    }));
                    HandleJackpotClick(_cloneJackpotObjs[currentIndex], currentIndex);
                });
            }
        }

        private void HandleJackpotClick(GameObject currentObj, int index)
        {
            string objName = currentObj.name;
            if (!_countClickDic.ContainsKey(objName))
            {
                Debug.LogError($"未找到克隆体名称 {objName} 对应的配置");
                return;
            }

            _countClickDic[objName].clickCount++;
            _countClickDic[objName].clickIndexList.Add(index);
            if (_countClickDic[objName].clickCount > 3) return;
            if (_countClickDic[objName].clickCount == 3)
            {
                List<string> temp = new List<string>()
                {
                    "Grand(Clone)", "Major(Clone)", "Minor(Clone)", "Mini(Clone)"
                };
                ContentModel.Instance.bonusIndex = temp.IndexOf(objName);
                PlayWinSpine(objName);
            }
        }

        private void PlayWinSpine(string objName)
        {
            _coverMask.visible = true;
            for (int i = 0; i < _clickIconList.Count; i++)
            {
                _clickIconList[i].gameBtn.touchable = false;
                Animator ani = _cloneJackpotObjs[i].GetComponentInChildren<Animator>();
                ani.speed = 0;
            }

            for (int i = 0; i < _countClickDic[objName].clickIndexList.Count; i++)
            {
                int index = _countClickDic[objName].clickIndexList[i];
                _cloneJackpotObjs[index].SetActive(false);

                GameObject obj = _cloneWinObjs[index];
                obj.SetActive(true);
                Animator ani = obj.GetComponentInChildren<Animator>();
                PlayAnimationByName(ani, "win");
            }


            Timers.inst.Add(4f, 1, (obj =>
            {
                // CloseSelf(null);
                _coverMask.visible = false;
                for (int i = 0; i < _countClickDic[objName].clickIndexList.Count; i++)
                {
                    int index = _countClickDic[objName].clickIndexList[i];

                    Animator ani = _cloneWinObjs[index].GetComponentInChildren<Animator>();
                    ani.speed = 0;
                }

                PageManager.Instance.OpenPage(PageName.CaiFuZhiMenPopupJackpotResult);
            }));
        }

        private void ShowJackpotData()
        {
            _uiJpMajorCtrl.Init("Major",
                this.contentPane.GetChild("jpMajor").asCom.GetChild("reels").asCom.GetChild("n0").asList, "N0");
            _uiJpMinorCtrl.Init("Minor",
                this.contentPane.GetChild("jpMinor").asCom.GetChild("reels").asCom.GetChild("n0").asList, "N0");
            _uiJpMiniCtrl.Init("Mini",
                this.contentPane.GetChild("jpMini").asCom.GetChild("reels").asCom.GetChild("n0").asList, "N0");

            _uiJpMajorCtrl.SetReelWidth(20);
            _uiJpMinorCtrl.SetReelWidth(20);
            _uiJpMiniCtrl.SetReelWidth(20);

            if (ApplicationSettings.Instance.isMock)
            {
                _uiJpMajorCtrl.SetData(ContentModel.Instance.uiMajorJP.nowCredit);
                _uiJpMinorCtrl.SetData(ContentModel.Instance.uiMinorJP.nowCredit);
                _uiJpMiniCtrl.SetData(ContentModel.Instance.uiMiniJP.nowCredit);
            }
            else
            {
                ERPushMachineDataManager02.Instance.RequestGetJpContribution((res) =>
                {
                    JSONNode jsonNode = JSONNode.Parse((string)res);
                    Debug.Log(jsonNode);
                    int code = (int)jsonNode["code"];
                    if (0 != code)
                    {
                        DebugUtils.LogError($"请求贡献值报错。 code: {code}");
                        return;
                    }

                    int majorBet = (int)jsonNode["major"];
                    int minorBet = (int)jsonNode["minor"];
                    int miniBet = (int)jsonNode["mini"];

                    _uiJpMajorCtrl.SetData(minorBet);
                    _uiJpMinorCtrl.SetData(majorBet);
                    _uiJpMiniCtrl.SetData(miniBet);
                });
            }
        }

        private void OnBatchBtnRollOver(GButton btn)
        {
            if (_isCanClick)
            {
                btn.scaleX = 1.1f;
                btn.scaleY = 1.1f;
            }
        }

        private void OnBatchBtnRollOut(GButton btn)
        {
            btn.scaleX = 1f;
            btn.scaleY = 1f;
        }

        private void PlayAnimationByName(Animator animator, string aniName)
        {
            animator.Rebind();
            animator.Play(aniName);
            animator.Update(0f);
        }

        private void ResetPage()
        {
            for (int i = 0; i < _cloneJackpotObjs.Count; i++)
            {
                Object.Destroy(_cloneJackpotObjs[i]);
                Object.Destroy(_cloneWinObjs[i]);
            }

            _cloneWinObjs.Clear();
            _cloneJackpotObjs.Clear();

            _coverMask.visible = false;

            _winList.Clear();

            for (int i = 0; i < _compareGoldCoinList.Count; i++)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareWinList[i]);
                GameCommon.FguiUtils.DeleteWrapper(_compareGoldCoinList[i]);
                GameCommon.FguiUtils.DeleteWrapper(_compareJackpotObjList[i]);
            }

            _compareGoldCoinList.Clear();
            _compareJackpotObjList.Clear();
            _compareWinList.Clear();

            for (int i = 0; i < _clickIconList.Count; i++)
            {
                _clickIconList[i].gameBtn.touchable = true;
                _clickIconList[i].gameBtn.visible = true;
                _clickIconList[i].gameBtn.onRollOver.Clear();
                _clickIconList[i].gameBtn.onRollOut.Clear();
                _clickIconList[i].gameBtn.onClick.Clear();
            }

            _clickIconList.Clear();
            _countClickDic.Clear();
        }
    }

    public class ClickIcon
    {
        public GComponent anchorLight;
        public GComponent anchorWinningType;
        public GButton gameBtn;
    }

    /// <summary>
    /// 记录点击数据类
    /// </summary>
    public class CountClick
    {
        public int clickCount; // 记录点击次数
        public List<int> clickIndexList; // 记录点击索引

        public CountClick(int clickCount, List<int> clickIndexList)
        {
            this.clickCount = clickCount;
            this.clickIndexList = clickIndexList;
        }
    }
}