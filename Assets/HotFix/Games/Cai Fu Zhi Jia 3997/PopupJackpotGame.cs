using FairyGUI;
using GameMaker;
using SlotMaker;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
        private List<Animator> _cloneAnimators = new List<Animator>(); // 预制体上的动画集合
        private readonly List<GComponent> _compareJackpotSpineGComList = new List<GComponent>();

        // Fairy GUI
        private readonly List<GComponent> _rollReels = new List<GComponent>();
        private GComponent _jackpotReelsGCom, _jackpotDiamondSpinesGCom = null; //彩金游戏滚轴的父物体 he 中奖红钻石锚点的的父物体
        List<GTextField> _diamondTextList = new List<GTextField>();
        private GComponent _gOwnerPanel; //panel界面初始化

        // 核心逻辑
        private GTextField _freeCountText = null;
        private int _totalPlayRounds = 3;
        private bool _isWinning; // 本局是否中奖
        private Random _random = new Random(); // 用作判断本局是否中奖
        private List<int> _winSpineIndexList = new List<int>(); // 记录当前所有中奖的格子
        private List<int> _canSpinReelIndexList = new List<int>(); // 当前可以旋转的滚轴
        private List<string> _rollRewardList = new List<string>(); // 所有滚轮的中奖金额集合

        private bool _isStart = false; // 开始按钮只能点击一次

        private List<Vector3> _boxCenterPosList = new List<Vector3>(); // 每个滚轴的锚点，作为特效的起点

        private List<SingleReelController> _singleReelControllers = new List<SingleReelController>(); // 所有滚轮控制器
        private List<Transform> _effects = new List<Transform>(); // 钻石在彩金游戏结束之后的结算特效

        /// <summary>
        /// 每个滚轴的旋转速度
        /// </summary>
        private List<int> _moveSpeedList = new List<int>()
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
            
            if (_monoHelper == null)
                _monoHelper = GameObject.Find("Game Main Controller").GetComponent<MonoHelper>();
            if (_slotMachineController == null)
                _slotMachineController =
                    GameObject.Find("Game Main Controller").GetComponentInChildren<SlotMachineController3997>();

            _totalCount = 3;
            LoadAsyncRes();
        }

        public override void InitParam()
        {
            if (!_isInitialized) return;
            ResetView();

            // 加载Panel面板
            _gOwnerPanel = contentPane.GetChild("panel").asCom;
            MainModel.Instance.contentMD = ContentModel.Instance;
            ContentModel.Instance.goAnthorPanel = _gOwnerPanel;
            MainModel.Instance.contentMD.goAnthorPanel = _gOwnerPanel;
            EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
                new EventData<GComponent>(PanelEvent.AnchorPanelChange, _gOwnerPanel));

            for (int i = 0; i < _canSpinReelIndexList.Count; i++)
            {
                SingleReelController testReelController = new SingleReelController(_rollReels[i], i);
                _rollRewardList.Add(testReelController.Wheeleward);
                _singleReelControllers.Add(testReelController);
            }


            ContentModel.Instance.btnSpinState = SpinButtonState.Stop;
            BindPrefabsToUI();
            
            //彩金
            //uiJPGrangCtrl.Init("Grand", this.contentPane.GetChild("jpGrand").asCom.GetChild("reels").asList, "N0");
            uiJPMajorCtrl.Init("Major", this.contentPane.GetChild("jpMajor").asCom.GetChild("n1").asList, "N0");
            uiJPMinorCtrl.Init("Minor", this.contentPane.GetChild("jpMinor").asCom.GetChild("n1").asList, "N0");
            uiJPMiniCtrl.Init("Mini", this.contentPane.GetChild("jpMini").asCom.GetChild("n1").asList, "N0");
            
            uiJPMajorCtrl.SetReelWidth(30);
            uiJPMinorCtrl.SetReelWidth(30);
            uiJPMiniCtrl.SetReelWidth(30);

            //uiJPGrangCtrl.SetData(50000);
            uiJPMajorCtrl.SetData(ContentModel.Instance.uiMajorJP.nowCredit);
            uiJPMinorCtrl.SetData(ContentModel.Instance.uiMinorJP.nowCredit);
            uiJPMiniCtrl.SetData(ContentModel.Instance.uiMiniJP.nowCredit);
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);
            
            // InitUI();
            // InitCanSpinReels();
            // InitController();
            
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

        private void InitController()
        {
            if (_monoHelper == null)
                _monoHelper = GameObject.Find("Game Main Controller").GetComponent<MonoHelper>();
            if (_slotMachineController == null)
                _slotMachineController =
                    GameObject.Find("Game Main Controller").GetComponentInChildren<SlotMachineController3997>();
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
                _boxCenterPosList.Add(currentGCom.position); //添加初始Spine动画锚点
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
            // Debug.LogError("_canSpinReelIndexList.Count:" + _canSpinReelIndexList.Count);
            if (_canSpinReelIndexList.Count > 13) // 保证至少中两个图标
            {
                return true;
            }
            else if (_canSpinReelIndexList.Count > 5) // 限制最多中奖次数为多少个 避免疯狂中奖
            {
                return _random.Next(100) < 10;
            }
            else
            {
                return false;
            }
        }

        void GetCurrentWinningDiamondList()
        {
            Random tempRandom = new Random();

            // _winSpineIndexList.Clear();
            for (int i = 0; i < 2; i++)
            {
                int randomIndex = tempRandom.Next(_canSpinReelIndexList.Count);
                int selectedValue = _canSpinReelIndexList[randomIndex];
                if (!_winSpineIndexList.Contains(selectedValue))
                {
                    _winSpineIndexList.Add(selectedValue);
                    _canSpinReelIndexList.RemoveAt(randomIndex);
                }
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

            _winSpineIndexList.Sort(); // 排序，保证钻石关闭是按顺序关闭
            Debug.LogError("_winSpineIndexList.Count:" + _winSpineIndexList.Count);
            // 为每个对象启动独立的协程
            for (int i = 0; i < _winSpineIndexList.Count /*_cloneJackpotSpineList.Count*/; i++)
            {
                int index = _winSpineIndexList[i]; // 创建局部变量，避免闭包问题
                coroutines.Add(_monoHelper.StartCoroutine(ProcessSingleResult(index)));

                // 每个对象之间等待一段时间
                yield return new WaitForSeconds(2f); // 调整这个间隔来控制逐个出现的速度
            }

            // 等待所有协程完成（可选）
            foreach (var coroutine in coroutines)
            {
                yield return coroutine;
            }

            yield return new WaitForSeconds(3);
            // CloseSelf(null);
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
                ContentModel.Instance.totalBonusReward += long.Parse(_rollRewardList[index]);
                _slotMachineController.SendTotalWinCreditEvent(ContentModel.Instance.totalBonusReward);
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
                _compareJackpotSpineGComList[i] = null;
            }

            _compareJackpotSpineGComList.Clear();

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

            _gOwnerPanel = null;
        }
    }
}