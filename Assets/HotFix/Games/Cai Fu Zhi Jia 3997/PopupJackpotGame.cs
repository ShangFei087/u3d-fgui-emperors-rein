using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace CaiFuZhiJia_3997
{
    public class PopupJackpotGame : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiJia";
        public new const string resName = "PopupJackpotGame";

        private const string SpinePrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupJackpotGame/SpinePrefabs/";
        private const string ModelPrefabPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupJackpotGame/ModelPrefabs/";

        private int _totalCount = -1;
        private bool _isInitialized = false;
        private MonoHelper _monoHelper = null;
        private SlotMachineController3997 _slotMachineController = null;

        // Spine
        private GameObject
            _reelBgSpineObj = null, /*_bonusTreeSpineObj = null,*/ _bonusSpineObj = null; // 第三个是钻石Spine动画

        private GameObject _cloneReelBgSpineObj = null/*, _cloneBonusTreeSpineObj = null*/;
        private GComponent _compareReelBgSpineGCom = null, _compareBonusTreeSpineGCom = null;

        private readonly List<GameObject> _cloneJackpotSpineList = new List<GameObject>();
        private readonly List<Animator> _cloneAnimators = new List<Animator>(); // 预制体上的动画集合
        private readonly List<GComponent> _compareJackpotSpineGComList = new List<GComponent>();
        
        // // 3D Model
        // private GComponent _compareNpcCom;
        // private GameObject _npcObj, _cloneNpcObj;
        

        // Fairy GUI
        private readonly List<GComponent> _rollReels = new List<GComponent>();
        private GComponent _jackpotReelsGCom, _jackpotDiamondSpinesGCom = null; //彩金游戏滚轴的父物体 he 中奖红钻石锚点的的父物体
        private readonly List<GTextField> _diamondTextList = new List<GTextField>();
        private GComponent _gOwnerPanel; //panel界面初始化

        // 核心逻辑
        private GTextField _freeCountText = null;
        private int _totalPlayRounds = 3;
        private bool _isWinning; // 本局是否中奖
        private readonly Random _random = new Random(); // 用作判断本局是否中奖
        private readonly List<int> _winSpineIndexList = new List<int>(); // 记录当前所有中奖的格子
        private readonly List<int> _canSpinReelIndexList = new List<int>(); // 当前可以旋转的滚轴
        private readonly List<string> _rollRewardList = new List<string>(); // 所有滚轮的中奖金额集合

        private bool _isStart = false; // 开始按钮只能点击一次

        private List<int> _bonusIsNotZeroList = new List<int>(); // 彩金游戏中数据不为0的格子的索引集合

        private readonly List<SingleReelController>
            _singleReelControllers = new List<SingleReelController>(); // 所有滚轮控制器

        private readonly List<Transform> _effects = new List<Transform>(); // 钻石在彩金游戏结束之后的结算特效

        /// <summary>
        /// 每个滚轴的旋转速度
        /// </summary>
        private readonly List<float> _moveSpeedList = new List<float>()
        {
            1.26f,
            1.52f,
            1.33f,
            1.45f,
            1.66f,
            1.76f,
            1.88f,
            1.77f,
            1.82f,
            1.75f,
            1.66f,
            1.8f,
            1.58f,
            1.65f,
            1.67f
        };


        //彩金
        MiniReelGroup uiJPMajorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMinorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMiniCtrl = new MiniReelGroup();

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();
            InitUI();
            InitCanSpinReels();

            _totalCount = 2;
            LoadAsyncRes();
        }

        public override void InitParam()
        {
            if (!_isInitialized) return;
            preLoadedCallback?.Invoke();
            if (!isOpen) return;

            // 加载Panel面板
            _gOwnerPanel = contentPane.GetChild("panel").asCom;
            MainModel.Instance.contentMD = ContentModel.Instance;
            MainModel.Instance.cutomMD = CustomModel.Instance;
            ContentModel.Instance.goAnthorPanel = _gOwnerPanel;
            MainModel.Instance.contentMD.goAnthorPanel = _gOwnerPanel;
            EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
                new EventData<GComponent>(PanelEvent.AnchorPanelChange, _gOwnerPanel));

            for (int i = 0; i < _canSpinReelIndexList.Count; i++)
            {
                SingleReelController testReelController = new SingleReelController(_rollReels[i], i);
                _singleReelControllers.Add(testReelController);
            }

            GetRewardBet();
            _bonusIsNotZeroList = _rollRewardList
                .Select((value, index) => new { value, index })
                .Where(item => item.value != "0")
                .Select(item => item.index)
                .ToList();
            ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
            BindPrefabsToUI();

            //彩金
            uiJPMajorCtrl.Init("Major", this.contentPane.GetChild("jpMajor").asCom.GetChild("n1").asList, "N0");
            uiJPMinorCtrl.Init("Minor", this.contentPane.GetChild("jpMinor").asCom.GetChild("n1").asList, "N0");
            uiJPMiniCtrl.Init("Mini", this.contentPane.GetChild("jpMini").asCom.GetChild("n1").asList, "N0");

            uiJPMajorCtrl.SetReelWidth(30);
            uiJPMinorCtrl.SetReelWidth(30);
            uiJPMiniCtrl.SetReelWidth(30);

            uiJPMajorCtrl.SetData(ContentModel.Instance.uiMajorJP.nowCredit);
            uiJPMinorCtrl.SetData(ContentModel.Instance.uiMinorJP.nowCredit);
            uiJPMiniCtrl.SetData(ContentModel.Instance.uiMiniJP.nowCredit);
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);

            if (_monoHelper == null)
                _monoHelper = GameObject.Find("Slot Game Main Controller 3997").GetComponent<MonoHelper>();
            if (_slotMachineController == null)
                _slotMachineController =
                    GameObject.Find("Slot Game Main Controller 3997")
                        .GetComponentInChildren<SlotMachineController3997>();

            InitParam();
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnPanelInputEvent);
        }

        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);
            ResetView();
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnPanelInputEvent);
        }

        private void ResLoadedCallback()
        {
            if (--_totalCount == 0)
            {
                _isInitialized = true;
                InitParam();
            }
        }

        private void InitUI()
        {
            _jackpotReelsGCom = contentPane.GetChild("rewardRoll").asCom;
            _jackpotDiamondSpinesGCom = contentPane.GetChild("jackpotDiamondSpines").asCom;

            for (int i = 0; i < _jackpotDiamondSpinesGCom.numChildren; i++)
            {
                GComponent diamondGCom = _jackpotDiamondSpinesGCom.GetChildAt(i).asCom;
                _diamondTextList.Add(diamondGCom.GetChild("rewardText").asTextField);
            }

            _freeCountText = contentPane.GetChild("jackpotFrame").asCom.GetChild("freeCount").asTextField;

            for (int i = 0; i < _jackpotReelsGCom.numChildren; i++)
            {
                GComponent reelGCom = _jackpotReelsGCom.GetChild("rollReel_" + i).asCom.GetChild("elementBox")
                    .asCom;
                _rollReels.Add(reelGCom);
            }
        }

        private void LoadAsyncRes()
        {
            ResourceManager02.Instance.LoadAsset<GameObject>(
                SpinePrefabPath + "reelBgSpine.prefab",
                (clone) =>
                {
                    _reelBgSpineObj = clone;
                    ResLoadedCallback();
                });

            // ResourceManager02.Instance.LoadAsset<GameObject>(
            //     SpinePrefabPath + "jackpotTreeSpine.prefab",
            //     (clone) =>
            //     {
            //         _bonusTreeSpineObj = clone;
            //         ResLoadedCallback();
            //     });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                SpinePrefabPath + "jackpotSpine.prefab",
                (clone) =>
                {
                    _bonusSpineObj = clone;
                    ResLoadedCallback();
                });
            
            // ResourceManager02.Instance.LoadAsset<GameObject>(
            //     ModelPrefabPath + "NPC_Obj.prefab",
            //     (clone) =>
            //     {
            //         _npcObj = clone;
            //         ResLoadedCallback();
            //     });
        }

        private void BindPrefabsToUI()
        {
            GComponent currentGCom = contentPane.GetChild("reelBgSpine").asCom;
            if (currentGCom != _compareReelBgSpineGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareReelBgSpineGCom);
                _compareReelBgSpineGCom = currentGCom;
                _cloneReelBgSpineObj = Object.Instantiate(_reelBgSpineObj);
                GameCommon.FguiUtils.AddWrapper(_compareReelBgSpineGCom, _cloneReelBgSpineObj);
            }

            // currentGCom = contentPane.GetChild("jackpotTreeSpine").asCom;
            // if (currentGCom != _compareBonusTreeSpineGCom)
            // {
            //     GameCommon.FguiUtils.DeleteWrapper(_compareBonusTreeSpineGCom);
            //     _compareBonusTreeSpineGCom = currentGCom;
            //     _cloneBonusTreeSpineObj = Object.Instantiate(_bonusTreeSpineObj);
            //     GameCommon.FguiUtils.AddWrapper(_compareBonusTreeSpineGCom, _cloneBonusTreeSpineObj);
            // }
            
            // currentGCom = contentPane.GetChild("anchorNpcModel").asCom;
            // if (currentGCom != _compareNpcCom)
            // {
            //     GameCommon.FguiUtils.DeleteWrapper(_compareNpcCom);
            //     _compareNpcCom = currentGCom;
            //     _cloneNpcObj = Object.Instantiate(_npcObj);
            //     GameCommon.FguiUtils.AddWrapper(_compareNpcCom, _cloneNpcObj);
            // }

            for (int i = 0; i < _jackpotDiamondSpinesGCom.numChildren; i++)
            {
                _compareJackpotSpineGComList.Add(null);
            }

            for (int i = 0; i < _jackpotDiamondSpinesGCom.numChildren; i++)
            {
                currentGCom = _jackpotDiamondSpinesGCom.GetChild("jackpotSpine_" + i).asCom;
                if (currentGCom != _compareJackpotSpineGComList[i])
                {
                    GameCommon.FguiUtils.DeleteWrapper(_compareJackpotSpineGComList[i]);
                    _compareJackpotSpineGComList[i] = currentGCom;
                    GameObject jackpotSpineCloneObj = Object.Instantiate(_bonusSpineObj);
                    GameObject currentObj;
                    int currentBet = int.Parse(ContentModel.Instance.currentBonusDataList[i]);
                    if (currentBet > 4000)
                    {
                        int index = currentBet % 10;
                        currentObj = jackpotSpineCloneObj.transform.GetChild(1).GetChild(index).gameObject;
                    }
                    else
                    {
                        currentObj = jackpotSpineCloneObj.transform.GetChild(1).GetChild(0).gameObject;
                    }

                    currentObj.SetActive(true);
                    jackpotSpineCloneObj.SetActive(false);
                    _cloneJackpotSpineList.Add(jackpotSpineCloneObj);
                    _cloneAnimators.Add(currentObj.GetComponentInChildren<Animator>());
                    _effects.Add(jackpotSpineCloneObj.transform.Find("Effect"));
                    GameCommon.FguiUtils.AddWrapper(_compareJackpotSpineGComList[i], jackpotSpineCloneObj);
                }
            }
        }

        #region 彩金游戏核心逻辑

        void OnPanelInputEvent(EventData eventData)
        {
            if (_isStart) return;
            _isStart = true;
            ContentModel.Instance.btnSpinState = SpinButtonState.Spin;
            _monoHelper.StartCoroutine(PlayMultipleRounds());
        }

        /// <summary>
        /// 本局是否中奖
        /// </summary>
        /// <returns></returns>
        bool RandomIsWinThisRound()
        {
            if (_bonusIsNotZeroList.Count <= 0)
                return false;

            // 计算当前中奖概率
            double winRate = CalculateWinRate();

            // 随机判定
            bool isWin = _random.NextDouble() < winRate;
            return isWin;
        }

        /// <summary>
        ///  计算中奖概率
        /// </summary>
        /// <returns></returns>
        double CalculateWinRate()
        {
            int count = _bonusIsNotZeroList.Count;
            double rate = 0.7;

            if (_totalPlayRounds == 1 && count > 0)
            {
                return 1;
            }

            double itemRatio = (double)count / (count + 5); // 元素比例因子
            double roundPressure = Math.Max(0, (3 - _totalPlayRounds) * 0.15); // 轮数压力
            rate = 0.7 + roundPressure - (itemRatio * 0.3);

            if (_totalPlayRounds <= 2 && count > 0)
            {
                rate = Math.Max(rate, 0.85);
            }

            return Math.Max(0.1, Math.Min(1.0, rate));
        }

        void GetCurrentWinningDiamondList()
        {
            if (_bonusIsNotZeroList.Count == 0) return;
            _currentWinIndexList.Clear();
            int maxCount = Math.Min(3, _bonusIsNotZeroList.Count);
            int count = _random.Next(1, maxCount + 1);

            for (int i = 0; i < count; i++)
            {
                int randomIndex = _random.Next(_bonusIsNotZeroList.Count);
                int selectedValue = _bonusIsNotZeroList[randomIndex];

                _winSpineIndexList.Add(selectedValue);
                _currentWinIndexList.Add(selectedValue);
                _bonusIsNotZeroList.RemoveAt(randomIndex);
            }
        }

        /// <summary>
        /// 在指定索引处播放中奖Spine动画
        /// </summary>
        private void PlayWinningSpineAt(int reelIndex)
        {
            if (_cloneJackpotSpineList[reelIndex].activeSelf) return;

            if (int.Parse(ContentModel.Instance.currentBonusDataList[reelIndex]) < 4000)
                _diamondTextList[reelIndex].text = _rollRewardList[reelIndex];

            _cloneJackpotSpineList[reelIndex].SetActive(true);
            PlayAnimationByName(_cloneAnimators[reelIndex], "start");

            Timers.inst.Add(1, 1, (obj) =>
            {
                PlayAnimationByName(_cloneAnimators[reelIndex], "idle");
            });
        }

        /// <summary>
        /// 初始化可以旋转的Reels
        /// </summary>
        void InitCanSpinReels()
        {
            _winSpineIndexList.Clear();
            _canSpinReelIndexList.Clear();
            for (int i = 0; i < _rollReels.Count; i++)
            {
                _canSpinReelIndexList.Add(i);
            }
        }

        private void PlayAnimationByName(Animator animator, string aniName)
        {
            animator.Rebind();
            animator.Play(aniName);
            animator.Update(0f);
        }

        /// <summary>
        /// 获取大奖中奖分数
        /// </summary>
        /// <returns></returns>
        private void GetRewardBet()
        {
            for (int i = 0; i < ContentModel.Instance.currentBonusDataList.Count; i++)
            {
                int type = int.Parse(ContentModel.Instance.currentBonusDataList[i]) / 1000;
                if (type != 4)
                {
                    int reward = int.Parse(ContentModel.Instance.currentBonusDataList[i]) % 1000;
                    _rollRewardList.Add(reward.ToString());
                }
                else
                {
                    for (int j = 0; j < ContentModel.Instance.currentJpIndexList.Count; j++)
                    {
                        if (i == ContentModel.Instance.currentJpIndexList[j])
                        {
                            _rollRewardList.Add(ContentModel.Instance.JpBetDic[ContentModel.Instance.jpTypeArray[j]]);
                        }
                    }
                }
            }
        }

        private IEnumerator GameResultCoroutine()
        {
            _winSpineIndexList.Sort();
            for (int i = 0; i < _winSpineIndexList.Count; i++)
            {
                int index = _winSpineIndexList[i];
                yield return _monoHelper.StartCoroutine(ProcessSingleResult(index));
            }

            yield return new WaitForSeconds(2f);
            PageManager.Instance.OpenPage(PageName.CaiFuZhiJiaPopupJackpotResult);
        }

        private IEnumerator ProcessSingleResult(int index)
        {
            GameObject currentObj = _cloneJackpotSpineList[index];

            if (currentObj.activeSelf)
            {
                PlayAnimationByName(_cloneAnimators[index], "win");
                _effects[index].gameObject.SetActive(true);

                // 每个对象独立等待2秒
                yield return new WaitForSeconds(2f);

                int currentBet = int.Parse(ContentModel.Instance.currentBonusDataList[index]);
                if (currentBet > 4000)
                {
                    ContentModel.Instance.currentJpSpineIndex = currentBet % 10;
                    for (int i = 0; i < ContentModel.Instance.currentJpIndexList.Count; i++)
                    {
                        if (index == ContentModel.Instance.currentJpIndexList[i])
                        {
                            ContentModel.Instance.currentShowJpBet =
                                int.Parse(ContentModel.Instance.JpBetDic[ContentModel.Instance.jpTypeArray[i]]);
                        }
                    }

                    bool isNext = false;

                    PageManager.Instance.OpenPageAsync(PageName.CaiFuZhiJiaPopupJackpotWin,
                        new EventData<Dictionary<string, object>>("",
                            new Dictionary<string, object>() { }),
                        (ed) =>
                        {
                            isNext = true;
                        });
                    yield return new WaitUntil(() => isNext == true);
                }

                SingleReelController parentCom = _singleReelControllers[index];
                parentCom.RollElements[3].GetChild("element").asLoader.url = CustomModel.Instance.JackpotBgPath[2];
                parentCom.ResetTransition.Play();
                if (currentBet > 4000)
                {
                    GComponent tempCom = parentCom.RollElements[3];
                    tempCom.GetChild("element").asLoader.url =
                        CustomModel.Instance.JackpotTypePath[ContentModel.Instance.currentJpSpineIndex];
                    parentCom.RewardTexts[3].visible = false;
                    tempCom.visible = true;
                }
                else
                {
                    parentCom.RollElements[3].visible = true;
                    parentCom.RewardTexts[3].visible = true;
                    parentCom.RewardTexts[3].text = _rollRewardList[index];
                }

                currentObj.SetActive(false);
                _diamondTextList[index].visible = false;

                // 加钱
                if (_rollRewardList != null)
                {
                    ContentModel.Instance.totalBonusReward += long.Parse(_rollRewardList[index]);
                    _slotMachineController.SendTotalWinCreditEvent(ContentModel.Instance.totalBonusReward);
                }
            }
        }

        private readonly List<int> _currentWinIndexList = new List<int>(); // 当前中的索引List 主要用作存储本局中奖索引，播放回弹动画

        IEnumerator GameOnceCoroutine()
        {
            _isWinning = RandomIsWinThisRound();

            for (int i = 0; i < _canSpinReelIndexList.Count; i++)
            {
                int reelIndex = _canSpinReelIndexList[i];
                _singleReelControllers[reelIndex].StartRoll(_moveSpeedList[i]);
            }

            yield return new WaitForSeconds(2f);

            if (!_isWinning)
            {
                Debug.LogError("没中奖");
                // for (int i = 0; i < _canSpinReelIndexList.Count; i++)
                // {
                //     int reelIndex = _canSpinReelIndexList[i];
                //     _singleReelControllers[reelIndex].StartRoll(_moveSpeedList[i]);
                // }
                //
                // yield return new WaitForSeconds(2f);

                for (int i = 0; i < _canSpinReelIndexList.Count; i++)
                {
                    int reelIndex = _canSpinReelIndexList[i];
                    _singleReelControllers[reelIndex].StopRoll(_currentWinIndexList, null);
                }

                _totalPlayRounds--;
                _freeCountText.text = _totalPlayRounds.ToString();
            }
            else
            {
                Debug.LogError("中奖了");
                GetCurrentWinningDiamondList();

                var stopCallbacks = new List<Action>();
                int completedCount = 0;
                int totalToComplete = _canSpinReelIndexList.Count;

                // 先收集所有需要停止的滚轴
                foreach (var reelIndex in _canSpinReelIndexList)
                {
                    bool isWin = _currentWinIndexList.Contains(reelIndex);
                    int capturedReelIndex = reelIndex; // 闭包捕获

                    _singleReelControllers[reelIndex].StopRoll(
                        _currentWinIndexList, () =>
                        {
                            if (isWin)
                            {
                                int tempBet = int.Parse(ContentModel.Instance.currentBonusDataList[capturedReelIndex]);
                                if (tempBet > 4000)
                                {
                                    int type = tempBet % 10;
                                    OnReelStopped(capturedReelIndex, () => completedCount++, type);
                                }
                                else
                                {
                                    OnReelStopped(capturedReelIndex, () => completedCount++,tempBet.ToString());
                                }
                            }
                            else
                                completedCount++;
                        }
                    );
                }

                yield return new WaitUntil(() => completedCount >= totalToComplete);

                // 统一从可旋转列表中移除中奖项
                foreach (var reelIndex in _currentWinIndexList)
                {
                    _canSpinReelIndexList.Remove(reelIndex);
                }

                _freeCountText.text = "3";
                _totalPlayRounds = 3;
            }

            yield return new WaitForSeconds(2);
        }

        private void OnReelStopped(int reelIndex, Action onComplete, string normalBet)
        {
            _singleReelControllers[reelIndex].PlayBack(() =>
            {
                PlayWinningSpineAt(reelIndex);
                onComplete?.Invoke();
            }, normalBet);
        }

        private void OnReelStopped(int reelIndex, Action onComplete, int winType)
        {
            _singleReelControllers[reelIndex].PlayBack(() =>
            {
                PlayWinningSpineAt(reelIndex);
                onComplete?.Invoke();
            }, winType);
        }

        // 调用方式
        IEnumerator PlayMultipleRounds()
        {
            while (_totalPlayRounds > 0)
            {
                yield return _monoHelper.StartCoroutine(GameOnceCoroutine());
            }

            _monoHelper.StartCoroutine(GameResultCoroutine());
        }

        #endregion


        private void ResetView()
        {
            for (int i = 0; i < _cloneJackpotSpineList.Count; i++)
            {
                if (_cloneJackpotSpineList[i] != null)
                    Object.Destroy(_cloneJackpotSpineList[i]);
            }

            _cloneJackpotSpineList.Clear();

            // Object.Destroy(_cloneNpcObj);
            Object.Destroy(_cloneReelBgSpineObj);
            // Object.Destroy(_cloneBonusTreeSpineObj);

            // _cloneNpcObj = null;
            _cloneReelBgSpineObj = null;
            // _cloneBonusTreeSpineObj = null;

            for (int i = 0; i < _compareJackpotSpineGComList.Count; i++)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareJackpotSpineGComList[i]);
                _compareJackpotSpineGComList[i] = null;
            }

            _compareJackpotSpineGComList.Clear();

            GameCommon.FguiUtils.DeleteWrapper(_compareReelBgSpineGCom);
            GameCommon.FguiUtils.DeleteWrapper(_compareBonusTreeSpineGCom);
            _compareReelBgSpineGCom = null;
            _compareBonusTreeSpineGCom = null;

            _cloneAnimators.Clear();
            _effects.Clear();

            _totalPlayRounds = 3;
            _isWinning = false;
            _isStart = false;

            for (int i = 0; i < _winSpineIndexList.Count; i++)
            {
                int index = _winSpineIndexList[i];
                _singleReelControllers[index].RollElements[3].visible = false;
                _singleReelControllers[index].RewardTexts[3].visible = false;
                _singleReelControllers[index].RewardTexts[3].text = "";
            }

            _winSpineIndexList.Clear();
            InitCanSpinReels();
            if (_singleReelControllers != null && _singleReelControllers.Count > 0)
            {
                foreach (var controller in _singleReelControllers)
                {
                    if (controller is System.IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }

                _singleReelControllers.Clear();
            }

            _rollRewardList.Clear();
            _bonusIsNotZeroList.Clear();

            foreach (var text in _diamondTextList)
            {
                if (text != null)
                {
                    text.text = "";
                    text.visible = true; // 重置为可见状态
                }
            }

            if (_freeCountText != null)
            {
                _freeCountText.text = _totalPlayRounds.ToString();
            }

            foreach (var reel in _rollReels)
            {
                if (reel != null)
                {
                    reel.visible = true;
                }
            }

            if (_monoHelper != null)
            {
                _monoHelper.StopAllCoroutines();
            }

            _monoHelper = null;
            _slotMachineController = null;

            _gOwnerPanel = null;
        }
    }
}