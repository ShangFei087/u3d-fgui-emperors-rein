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

        private int _totalCount = -1;
        private bool _isInitialized = false;
        private MonoHelper _monoHelper = null;
        private SlotMachineController3997 _slotMachineController = null;


        // Spine
        private GameObject
            _reelBgSpineObj = null, _jackpotTreeSpineObj = null, _jackpotSpineObj = null; // 第三个是钻石Spine动画

        private GameObject _cloneReelBgSpineObj = null, _cloneJackpotTreeSpineObj = null;
        private GComponent _compareReelBgSpineGCom = null, _compareJackpotTreeSpineGCom = null;

        private readonly List<GameObject> _cloneJackpotSpineList = new List<GameObject>();
        private readonly List<Animator> _cloneAnimators = new List<Animator>(); // 预制体上的动画集合
        private readonly List<GComponent> _compareJackpotSpineGComList = new List<GComponent>();

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
        private List<string> _rollRewardList = new List<string>(); // 所有滚轮的中奖金额集合

        private bool _isStart = false; // 开始按钮只能点击一次

        private List<int> _bonusIsNotZeroList = new List<int>(); // 彩金游戏中数据不为0的格子的索引集合

        private readonly List<SingleReelController>
            _singleReelControllers = new List<SingleReelController>(); // 所有滚轮控制器

        private readonly List<Transform> _effects = new List<Transform>(); // 钻石在彩金游戏结束之后的结算特效

        /// <summary>
        /// 每个滚轴的旋转速度
        /// </summary>
        private readonly List<int> _moveSpeedList = new List<int>()
        {
            100,
            110,
            130,
            90,
            150,
            130,
            110,
            100,
            120,
            110,
            130,
            150,
            160,
            100,
            150
        };


        //彩金
        //MiniReelGroup uiJPGrandCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMajorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMinorCtrl = new MiniReelGroup();
        MiniReelGroup uiJPMiniCtrl = new MiniReelGroup();

        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();
            InitUI();
            InitCanSpinReels();


            _totalCount = 3;
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
                // _rollRewardList.Add(testReelController.Wheeleward);
                _singleReelControllers.Add(testReelController);
            }

            _rollRewardList = ContentModel.Instance.currentBonusDataList;
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

            ResourceManager02.Instance.LoadAsset<GameObject>(
                SpinePrefabPath + "jackpotTreeSpine.prefab",
                (clone) =>
                {
                    _jackpotTreeSpineObj = clone;
                    ResLoadedCallback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                SpinePrefabPath + "jackpotSpine.prefab",
                (clone) =>
                {
                    _jackpotSpineObj = clone;
                    ResLoadedCallback();
                });
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

            currentGCom = contentPane.GetChild("jackpotTreeSpine").asCom;
            if (currentGCom != _compareJackpotTreeSpineGCom)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareJackpotTreeSpineGCom);
                _compareJackpotTreeSpineGCom = currentGCom;
                _cloneJackpotTreeSpineObj = Object.Instantiate(_jackpotTreeSpineObj);
                GameCommon.FguiUtils.AddWrapper(_compareJackpotTreeSpineGCom, _cloneJackpotTreeSpineObj);
            }

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
                    GameObject jackpotSpineCloneObj = Object.Instantiate(_jackpotSpineObj);
                    jackpotSpineCloneObj.SetActive(false);
                    _cloneJackpotSpineList.Add(jackpotSpineCloneObj);
                    _cloneAnimators.Add(jackpotSpineCloneObj.GetComponentInChildren<Animator>());
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

        // Todo：随机给每个滚轮的两个图标设置奖励；当中奖的时候，从当前滚轴索引中随机出一个中奖的图标；设置指定图标来进行显示

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

            // 动态调整：元素越少，中奖率越低（让游戏自然结束）
            // 但保证在轮数危险时提高中奖率
            double itemRatio = (double)count / (count + 5); // 元素比例因子
            double roundPressure = Math.Max(0, (3 - _totalPlayRounds) * 0.15); // 轮数压力
            rate = 0.7 + roundPressure - (itemRatio * 0.3);

            // 确保在危险情况下概率不会太低
            if (_totalPlayRounds <= 2 && count > 0)
            {
                rate = Math.Max(rate, 0.85);
            }

            return Math.Max(0.1, Math.Min(1.0, rate));
        }
        
        void GetCurrentWinningDiamondList()
        {
            if (_bonusIsNotZeroList.Count == 0) return;
    
            // 确定中奖个数：1 ~ 剩余元素数
            int count = _random.Next(1, _bonusIsNotZeroList.Count + 1);
    
            for (int i = 0; i < count; i++)
            {
                // 每次从当前剩余元素中随机取（范围随 i 缩小）
                int randomIndex = _random.Next(_bonusIsNotZeroList.Count);
                int selectedValue = _bonusIsNotZeroList[randomIndex];
        
                _winSpineIndexList.Add(selectedValue);
                _canSpinReelIndexList.Remove(selectedValue);
        
                // 移除已选中的，保证下次不重复
                _bonusIsNotZeroList.RemoveAt(randomIndex);
            }
        }

        private void ShowWinningSpine()
        {
            for (int i = 0; i < _winSpineIndexList.Count; i++)
            {
                if (!_cloneJackpotSpineList[_winSpineIndexList[i]].activeSelf)
                {
                    _diamondTextList[_winSpineIndexList[i]].text = _rollRewardList[_winSpineIndexList[i]];
                    _cloneJackpotSpineList[_winSpineIndexList[i]].SetActive(true);
                    PlayAnimationByName(_cloneAnimators[_winSpineIndexList[i]], "start");
                    int index = _winSpineIndexList[i];
                    Timers.inst.Add(1, 1, (obj) =>
                    {
                        PlayAnimationByName(_cloneAnimators[index], "idle");
                    });
                }
            }
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

        private IEnumerator GameResultCoroutine()
        {
            List<Coroutine> coroutines = new List<Coroutine>();

            _winSpineIndexList.Sort();
            for (int i = 0; i < _winSpineIndexList.Count; i++)
            {
                int index = _winSpineIndexList[i];
                coroutines.Add(_monoHelper.StartCoroutine(ProcessSingleResult(index)));

                yield return new WaitForSeconds(2f); // 调整这个间隔来控制逐个出现的速度
            }

            foreach (var coroutine in coroutines)
            {
                yield return coroutine;
            }

            yield return new WaitForSeconds(3);
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

                currentObj.SetActive(false);
                _diamondTextList[index].visible = false;

                _singleReelControllers[index].RollElements[3].visible = true;
                _singleReelControllers[index].RewardTexts[3].visible = true;
                _singleReelControllers[index].RewardTexts[3].text = _rollRewardList[index];

                // 加钱
                if (_rollRewardList != null)
                {
                    ContentModel.Instance.totalBonusReward += long.Parse(_rollRewardList[index]);
                    _slotMachineController.SendTotalWinCreditEvent(ContentModel.Instance.totalBonusReward);
                }
            }
        }


        IEnumerator GameOnceCoroutine()
        {
            _isWinning = RandomIsWinThisRound();

            if (!_isWinning)
            {
                Debug.LogError("没中奖");
                for (int i = 0; i < _canSpinReelIndexList.Count; i++)
                {
                    int reelIndex = _canSpinReelIndexList[i];
                    _singleReelControllers[reelIndex].StartRoll(_monoHelper, _moveSpeedList[i]);
                }

                yield return new WaitForSeconds(5f);

                for (int i = 0; i < _canSpinReelIndexList.Count; i++)
                {
                    int reelIndex = _canSpinReelIndexList[i];
                    _singleReelControllers[reelIndex].StopRoll(_monoHelper, _winSpineIndexList);
                }

                _totalPlayRounds--;
                _freeCountText.text = _totalPlayRounds.ToString();
            }
            else
            {
                Debug.LogError("中奖了");
                for (int i = 0; i < _canSpinReelIndexList.Count; i++)
                {
                    int reelIndex = _canSpinReelIndexList[i];
                    _singleReelControllers[reelIndex].StartRoll(_monoHelper, _moveSpeedList[i]);
                }

                yield return new WaitForSeconds(5f);

                for (int i = 0; i < _canSpinReelIndexList.Count; i++)
                {
                    int reelIndex = _canSpinReelIndexList[i];
                    _singleReelControllers[reelIndex].StopRoll(_monoHelper, _winSpineIndexList);
                }

                GetCurrentWinningDiamondList();
                ShowWinningSpine();
                // 重置局数
                _freeCountText.text = "3";
                _totalPlayRounds = 3;
            }

            yield return new WaitForSeconds(2);
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
            // 清理 Spine 动画对象
            for (int i = 0; i < _cloneJackpotSpineList.Count; i++)
            {
                if (_cloneJackpotSpineList[i] != null)
                    Object.Destroy(_cloneJackpotSpineList[i]);
            }

            _cloneJackpotSpineList.Clear();

            Object.Destroy(_cloneReelBgSpineObj);
            Object.Destroy(_cloneJackpotTreeSpineObj);

            _cloneReelBgSpineObj = null;
            _cloneJackpotTreeSpineObj = null;

            for (int i = 0; i < _compareJackpotSpineGComList.Count; i++)
            {
                GameCommon.FguiUtils.DeleteWrapper(_compareJackpotSpineGComList[i]);
                _compareJackpotSpineGComList[i] = null;
            }

            _compareJackpotSpineGComList.Clear();

            GameCommon.FguiUtils.DeleteWrapper(_compareReelBgSpineGCom);
            GameCommon.FguiUtils.DeleteWrapper(_compareJackpotTreeSpineGCom);
            _compareReelBgSpineGCom = null;
            _compareJackpotTreeSpineGCom = null;

            // 清理动画控制器列表
            _cloneAnimators.Clear();

            // 清理特效列表
            _effects.Clear();

            // 重置核心逻辑状态
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

            // 清理中奖索引列表
            _winSpineIndexList.Clear();

            // 重置可以旋转的滚轴列表（重新初始化）
            InitCanSpinReels();

            // 清理滚轮控制器
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