using FairyGUI;
using GameMaker;
using PusherEmperorsRein;
using SBoxApi;
using System;
using System.Collections.Generic;
using UnityEngine;
using SoundKey = GameMaker.SoundKey;

enum PopState
{
    None,
    Change,
    Help,
    Bet,
    payTable,
}

namespace SlotMaker
{
    public class PanelBaseController : MonoBehaviour, IPanel
    {
        private static PanelBaseController _activeInstance;

        /// <summary>
        /// 设置菜单或说明书（赔付表）可见时，机台物理 Spin 应与屏幕 Spin 一致被忽略。
        /// </summary>
        public static bool ShouldBlockPhysicalSpinInput =>
            _activeInstance != null &&
            ((IsGObjectAlive(_activeInstance.setPanel) && _activeInstance.setPanel.visible) ||
             (IsGObjectAlive(_activeInstance.gIntroducePanel) && _activeInstance.gIntroducePanel.visible));

        // 当前弹窗状态（设置、帮助、赔付表等）
        PopState popState = PopState.None;

        // 面板根节点与常用子面板引用
        protected GComponent gOwnerPanel, gIntroducePanel, setPanel, btnSound, btnHelp, Introduce;
        protected GGraph mash;
        private GComponent _cachedAnchorPanel;
 
        // Spin 按钮控制器
        protected SpinButtonBaseController spinBtnCtrl = new SpinButtonBaseController();
        protected bool _isSpinStopButtonLocked;

        // 赔付表翻页与导航按钮
        protected GButton btnPayTable, btnPrev, btnNext, btnHome, btnBackGame;

        // 常用文本显示：下注、总赢分、单线赢分
        protected GTextField bet, win, singleLine;

        //声音滑动条
        protected GSlider silderSound;

        //展会模式
        protected GComponent ExhibitionPanel;
        protected List<GButton> btnColUps, btnColDowns;
        protected GButton btnExhibition;

        // 当前是否处于设置弹窗状态
        protected bool isSet;

        // 赔付表总页数
        protected int PayTableLength = 0;

        // 标记音量按钮是否处于按下状态（用于全局抬起恢复）
        protected bool _isSoundBtnPressed;

        // 记录最近一次非静音音量，用于按钮恢复声音时回滚
        protected float _lastNonMuteVolume = 1f;

        //下注按钮So
        protected GButton btnBetDown, btnBetUp;
        protected int curBetIndex = 0;
        protected int curBetListCount = 1;
        private int _lastRequestedBet = int.MinValue;
        private bool _isRequestSetBetInFlight;
        private int? _pendingRequestBet;
        private Action<object> _pendingRequestSetBetCallback;
  
        // Spin 装饰预制体，以及可选的短按/长按特效预制体
        GameObject goSpin;
        GameObject goShortSpin;
        GameObject goLongSpin;

        // 是否已完成初始化
        bool isInit;
        private bool _isInitializing;
        private int _initSequence;
        private GComponent _pendingAnchorPanel;
        public int IntroduceIndex;
        public int VolumeLevel;

        protected virtual int IntroduceIndexMax => 6;
        protected virtual string PanelPackageName => "Panel01";
        protected virtual string PanelPackagePath => "Assets/GameRes/Panel/Panel01/FGUIs";
        protected virtual string PanelComponentName => "Panel";
        protected virtual string SpinPrefabPath => "Assets/GameRes/Panel/Panel01/Prefabs/Slot_btn_Spin.prefab";
        /// <summary> 短按特效预制体。默认空字符串表示本机台不启用，子类覆盖路径即可接入。 </summary>
        protected virtual string ShortSpinPrefabPath => string.Empty;
        /// <summary> 长按特效预制体。默认空字符串表示本机台不启用，子类覆盖路径即可接入。 </summary>
        protected virtual string LongSpinPrefabPath => string.Empty;
        // 记录当前已加载的面板包路径，切换不同游戏路径时用于强制重载
        private string _loadedPanelPackagePath;
        // 记录当前已加载的面板包名，切换时用于精准卸载
        private string _loadedPanelPackageName;
        /// <summary>
        /// 面板启用：注册事件；不在此处 Init，避免机台预制体 OnEnable 早于 PageGameMain 写好锚点/线数。
        /// UI 初始化由 <see cref="PanelEvent.AnchorPanelChange"/>（如 TryTriggerAnchorPanelChange）触发 <see cref="Init"/>.
        /// </summary>
        protected virtual void OnEnable()
        {
            if (_activeInstance != null && _activeInstance != this)
            {
                _activeInstance.CleanupLifecycleListeners();
                Debug.Log($"PanelBaseController handover. oldId={_activeInstance.GetInstanceID()}, newId={GetInstanceID()}");
            }
            _activeInstance = this;

            EventCenter.Instance.AddEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, OnPropertyChange);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_WIN_EVENT, OnTotalWinCredit);
            EventCenter.Instance.AddEventListener<EventData>(MetaUIEvent.ON_CREDIT_EVENT, OnUpdateNaviCredit);
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_EVENT, OnPanelEventAnchorPanelChange);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_CONTENT_EVENT, OnContentChang);
            MainModel.Instance.panel = this;
            // OnDisable 会把 Panel 藏掉；二次进局只 SetActive(true) 时须对称恢复，否则底部栏不可见。
            RestoreOwnerPanelVisible();
        }

        /// <summary>
        /// 面板禁用：移除事件并重置按钮状态。
        /// </summary>
        protected virtual void OnDisable()
        {
            CleanupLifecycleListeners();

            if (silderSound != null)
            {
                silderSound.onChanged.Clear();
            }

            _isSoundBtnPressed = false;
            if (btnSound != null)
            {
                btnSound.SetScale(1f, 1f);
            }

            _lastRequestedBet = int.MinValue;
            _isRequestSetBetInFlight = false;
            _pendingRequestBet = null;
            _pendingRequestSetBetCallback = null;
            _isSpinStopButtonLocked = false;
            _isInitializing = false;
            _pendingAnchorPanel = null;
            _initSequence++;
            // 面板关闭时关掉短按/长按特效，避免循环粒子残留。
            HideSpinPressEffects();

            if (IsGObjectAlive(gOwnerPanel))
                gOwnerPanel.visible = false;
            else
                gOwnerPanel = null;
        }

        /// <summary>
        /// 二次进局时 OnDisable 可能已把 Panel 藏掉；复用同一锚点/包时须显式恢复。
        /// 大厅切语言会 Dispose 旧 contentPane，gOwnerPanel 仍非 null 但 displayObject.gameObject 已空。
        /// </summary>
        private void RestoreOwnerPanelVisible()
        {
            if (!IsGObjectAlive(gOwnerPanel))
            {
                gOwnerPanel = null;
                return;
            }
            gOwnerPanel.visible = true;
        }

        /// <summary>GObject 已 Dispose 时 displayObject 仍可能非 null，不能直接设 visible。</summary>
        private static bool IsGObjectAlive(GObject go)
        {
            if (go == null || go.isDisposed)
                return false;
            DisplayObject dobj = go.displayObject;
            return dobj != null && !dobj.isDisposed;
        }

        protected virtual void OnDestroy()
        {
            CleanupLifecycleListeners();
            if (_activeInstance == this)
            {
                _activeInstance = null;
            }
        }

        private void CleanupLifecycleListeners()
        {
            EventCenter.Instance.RemoveEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, OnPropertyChange);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_WIN_EVENT, OnTotalWinCredit);
            EventCenter.Instance.RemoveEventListener<EventData>(MetaUIEvent.ON_CREDIT_EVENT, OnUpdateNaviCredit);
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_EVENT, OnPanelEventAnchorPanelChange);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_CONTENT_EVENT, OnContentChang);
            Stage.inst.onTouchEnd.Remove(OnStageTouchEndResetSoundButton);
        }

        /// <summary>
        /// 初始化入口：加载面板资源与 Spin 按钮预制体。
        /// </summary>
        public virtual void Init(EventData res = null)
        {
            GComponent _goAnchorPanel = null;
            if (res != null)
                _goAnchorPanel = res.value as GComponent;
            else if (MainModel.Instance.contentMD != null)
                _goAnchorPanel = MainModel.Instance.contentMD.goAnthorPanel;

            if (_goAnchorPanel == null)
            {
                return;
            }

            if (_isInitializing)
            {
                _pendingAnchorPanel = _goAnchorPanel;
                return;
            }

            // 同一锚点且已初始化完成时不重复执行，避免重复触发 InitParam。
            // 机台侧仍会等 BottomPanelReady（如 PageGameMain preLoadedCallback），此处须补发，否则会永远等不到。
            if (isInit && ReferenceEquals(_cachedAnchorPanel, _goAnchorPanel))
            {
                Debug.Log("Skip Init: same anchor panel and already initialized.");
                RestoreOwnerPanelVisible();
                int readyGameId = MainModel.Instance != null ? MainModel.Instance.gameID : 0;
                EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
                    new EventData<int>(PanelEvent.BottomPanelReady, readyGameId));
                return;
            }

            _cachedAnchorPanel = _goAnchorPanel;
            _isInitializing = true;
            int currentInitSequence = ++_initSequence;

            // 默认等待：FGUI 包 + 装饰 Spin 预制体。短按/长按路径非空时再计入加载数。
            bool hasShortSpinPrefab = !string.IsNullOrEmpty(ShortSpinPrefabPath);
            bool hasLongSpinPrefab = !string.IsNullOrEmpty(LongSpinPrefabPath);
            int count = 2;
            if (hasShortSpinPrefab) count++;
            if (hasLongSpinPrefab) count++;

            Action loadComplete = () =>
            {
                if (currentInitSequence != _initSequence || _activeInstance != this || !isActiveAndEnabled)
                {
                    return;
                }

                // 面板包与 Spin 预制体（含可选短按/长按特效）都完成后再进行参数初始化
                if (--count == 0)
                {
                    isInit = true;
                    InitParam();
                    _isInitializing = false;

                    if (_pendingAnchorPanel != null && !ReferenceEquals(_pendingAnchorPanel, _cachedAnchorPanel))
                    {
                        GComponent nextAnchorPanel = _pendingAnchorPanel;
                        _pendingAnchorPanel = null;
                        Init(new EventData<GComponent>(PanelEvent.AnchorPanelChange, nextAnchorPanel));
                        return;
                    }

                    _pendingAnchorPanel = null;
                }
            };

            void LoadSpinPrefab(string path, Action<GameObject> onLoaded)
            {
                ResourceManager02.Instance.LoadAsset<GameObject>(path,
                    (GameObject clone) =>
                    {
                        if (currentInitSequence != _initSequence || _activeInstance != this || !isActiveAndEnabled)
                        {
                            return;
                        }

                        onLoaded?.Invoke(clone);
                        loadComplete();
                    });
            }


           
            if (_goAnchorPanel != null )
            {
                // 是否已有历史记录的面板包路径
                bool hasTrackedPackagePath = !string.IsNullOrEmpty(_loadedPanelPackagePath);
                // 当前游戏面板路径与历史路径不一致，说明发生了跨游戏切换
                bool isPackagePathChanged = hasTrackedPackagePath &&!string.Equals(_loadedPanelPackagePath, PanelPackagePath, StringComparison.Ordinal);
                if (isPackagePathChanged)
                {
                    // 跨游戏切换时先移除当前已绑定包，避免误复用到其他游戏 Panel
                    if (!string.IsNullOrEmpty(_loadedPanelPackageName))
                    {
                        UIPackage.RemovePackage(_loadedPanelPackageName);
                    }
                }

                // 当前记录的包名是否仍然有效（防止包被外部移除）
                bool hasCurrentPackage = !string.IsNullOrEmpty(_loadedPanelPackageName) &&UIPackage.GetByName(_loadedPanelPackageName) != null;
                // 首次进入、路径变化、或包失效时，统一重新加载当前游戏的 FairyGUI 包
                if (isPackagePathChanged || !hasCurrentPackage)
                {
                    ResourceManager02.Instance.LoadAssetBundleAsync(PanelPackagePath, (ab) =>
                    {
                        if (currentInitSequence != _initSequence || _activeInstance != this || !isActiveAndEnabled)
                        {
                            return;
                        }

                        UIPackage loadedPackage = UIPackage.AddPackage(ab);
                        if (loadedPackage == null)
                        {
                            // 加载失败也必须回调，避免初始化计数卡住
                            loadComplete();
                            return;
                        }

                        // 记录当前生效的包名与路径，供下次切换时判断
                        _loadedPanelPackageName = loadedPackage.name;
                        _loadedPanelPackagePath = PanelPackagePath;
                        GLoader anchorPanel = _goAnchorPanel.GetChild("icon").asLoader;
                        // 使用“实际包名 + 组件名”拼 URL，确保显示当前游戏对应的 Panel
                        anchorPanel.url = $"ui://{_loadedPanelPackageName}/{PanelComponentName}";
                        gOwnerPanel = _goAnchorPanel.GetChild("icon").asLoader.component;
                        RestoreOwnerPanelVisible();
                        loadComplete();
                    });
                }
                else
                {
                    // 包有效且路径一致时直接复用，避免重复加载
                    _loadedPanelPackagePath = PanelPackagePath;
                    GLoader anchorPanel = _goAnchorPanel.GetChild("icon").asLoader;
                    anchorPanel.url = $"ui://{_loadedPanelPackageName}/{PanelComponentName}";
                    gOwnerPanel = _goAnchorPanel.GetChild("icon").asLoader.component;
                    RestoreOwnerPanelVisible();
                    loadComplete();
                }
            }
            

            LoadSpinPrefab(SpinPrefabPath, clone => goSpin = clone);
            if (hasShortSpinPrefab)
                LoadSpinPrefab(ShortSpinPrefabPath, clone => goShortSpin = clone);
            if (hasLongSpinPrefab)
                LoadSpinPrefab(LongSpinPrefabPath, clone => goLongSpin = clone);
        }

        /// <summary>
        /// 绑定 UI、注册按钮事件并同步初始数据。
        /// </summary>
        protected virtual void InitParam()
        {
            if (_cachedAnchorPanel == null)
            {
                Debug.LogError("InitParam failed: _cachedAnchorPanel is null.");
                _isInitializing = false;
                return;
            }

            GLoader anchorLoader = _cachedAnchorPanel.GetChild("icon")?.asLoader;
            if (anchorLoader == null || anchorLoader.component == null)
            {
                Debug.LogError("InitParam failed: anchor icon loader/component is null.");
                _isInitializing = false;
                return;
            }

            gOwnerPanel = anchorLoader.component;
            setPanel = gOwnerPanel.GetChild("setPanel").asCom;
            setPanel.visible = false;
            mash = gOwnerPanel.GetChild("mash").asGraph;
            mash.onClick.Clear();
            mash.onClick.Add(OnClickMashCloseSetPanel);
            mash.visible = false;
            gOwnerPanel.GetChild("credit").asTextField.text =
                MainModel.Instance.myCredit.ToString(); //SBoxModel.Instance.myCredit.ToString();
            win = gOwnerPanel.GetChild("win").asTextField;
            win.text = 0.ToString();
            btnBetUp = gOwnerPanel.GetChild("btnBetUp").asButton;
            btnBetUp.onClick.Clear();
            btnBetUp.onClick.Add(OnClickButtonBetUp);
            btnBetDown = gOwnerPanel.GetChild("btnBetDown").asButton;
            btnBetDown.touchable = false;
            btnBetDown.GetChild("untouch").visible = true;
            btnBetDown.onClick.Clear();
            btnBetDown.onClick.Add(OnClickButtonBetDown);
            bet = gOwnerPanel.GetChild("bet").asTextField;
            bet.text = SBoxModel.Instance.betList[MainModel.Instance.contentMD.betIndex].ToString();

            singleLine = gOwnerPanel.GetChild("singleLine").asTextField;
            singleLine.text = "";

            // 初始化时将当前下注同步到机台（去重处理由统一方法负责）
            int initBet = (int)SBoxModel.Instance.betList[MainModel.Instance.contentMD.betIndex];
            RequestSetBetWithDedup(initBet, (res) =>
            {
                ChangeBetButtonInteractable(MainModel.Instance.contentMD.betIndex, SBoxModel.Instance.betList.Count);
            });

            // 短按/长按预制体可为 null；控制器内部会挂到独立锚点，并在进游戏时先隐藏。
            spinBtnCtrl.InitParam(gOwnerPanel.GetChild("btnSpin").asCom, "Stop", OnClickSpinButton, goSpin, goShortSpin, goLongSpin);

            gIntroducePanel = gOwnerPanel.GetChild("instructions").asCom;
            Introduce = gIntroducePanel.GetChild("introduce").asCom;
            btnPrev = gIntroducePanel.asCom.GetChild("btnPrev").asButton;
            btnPrev.onClick.Clear();
            btnPrev.onClick.Add(OnClickIntroduceL);
            btnNext = gIntroducePanel.asCom.GetChild("btnNect").asButton;
            btnNext.onClick.Clear();
            btnNext.onClick.Add(OnClickIntroduceR);
            btnBackGame = gIntroducePanel.GetChild("btnBackGame").asButton;
            btnBackGame.onClick.Clear();
            btnBackGame.onClick.Add(OnClickBackGame);
            PayTableLength = MainModel.Instance.contentMD.goPayTableLst.Length;
            gIntroducePanel.visible = false;
            //菜单
            btnHelp = gOwnerPanel.GetChild("btnHelp").asCom;
            btnHelp.onTouchBegin.Clear();
            btnHelp.onTouchBegin.Add(() => { btnHelp.SetScale(0.8f, 0.8f); });
            btnHelp.onClick.Clear();
            btnHelp.onClick.Add(() =>
            {
                Help();
            });
            //说明书
            btnPayTable = setPanel.GetChild("btnPayTable").asButton;
            btnPayTable.onClick.Clear();
            btnPayTable.onClick.Add(() =>
            {
                gIntroducePanel.visible = true;
                setPanel.visible = false;
                btnHelp.touchable = false;
                btnBetDown.touchable = false;
                btnBetUp.touchable = false;
                gOwnerPanel.GetChild("mash").asGraph.visible = false;
                gIntroducePanel.GetChild("mask").asGraph.visible = true;
                IntroduceInit();
                GlobalSoundHelper.Instance.PlaySoundEff(GameMaker.SoundKey.PopupOpen);
            });
            //声音
            btnSound = setPanel.GetChild("btnSound").asCom;
            btnSound.onTouchBegin.Clear();
            btnSound.onTouchBegin.Add(() =>
            {
                _isSoundBtnPressed = true;
                btnSound.SetScale(0.8f, 0.8f);
            });
            btnSound.onTouchEnd.Clear();
            btnSound.onTouchEnd.Add(() =>
            {
                _isSoundBtnPressed = false;
                btnSound.SetScale(1f, 1f);
            });
            btnSound.onClick.Clear();
            btnSound.onClick.Add(OnClickSoundButton);
            // 监听舞台抬起事件，避免拖出按钮后缩放状态残留
            Stage.inst.onTouchEnd.Remove(OnStageTouchEndResetSoundButton);
            Stage.inst.onTouchEnd.Add(OnStageTouchEndResetSoundButton);
            //声音滑动条
            silderSound = setPanel.GetChild("silderSound").asSlider;
            silderSound.visible = true;
            silderSound.onChanged.Clear();
            silderSound.onChanged.Add(OnSoundSliderChanged);

            //返回大厅
            btnHome = setPanel.GetChild("btnHome").asButton;
            btnHome.onClick.Clear();
            btnHome.onClick.Add(() =>
            {
                setPanel.visible = false;
                Help();
                BackHall();
            });

            //展会模式
            ExhibitionPanel = gOwnerPanel.GetChild("ExhibitionPanel").asCom;
            btnColUps = btnColUps ?? new List<GButton>();
            btnColDowns = btnColDowns ?? new List<GButton>();
            btnColUps.Clear();
            btnColDowns.Clear();
            for (int i = 0; i < ExhibitionPanel.numChildren-1; ++i)
            {
                btnColUps.Add(ExhibitionPanel.GetChildAt(i).asCom.GetChildAt(0).asButton);
                btnColDowns.Add(ExhibitionPanel.GetChildAt(i).asCom.GetChildAt(1).asButton);
            }
            btnExhibition = ExhibitionPanel.GetChild("btnExhibition").asButton;
            btnExhibition.onClick.Clear();
            btnExhibition.onClick.Add(OnClickExhibition);
            if (!ApplicationSettings.Instance.IsExpoMode())
            {
                btnExhibition.visible = false;
            }

            BindColumnButtons();
        
            isSet = false;
            OnPropertyChangeBetList();
            OnPropertyChangeTotalBet();
            OnPropertyChangeBtnSpinState();
            OnPropertyIsConnectMoneyBox();
            SyncSoundUIFromCurrentState();

            Debug.Log("初始化菜单Ui完成");
            EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,new EventData<int>(PanelEvent.BottomPanelReady, MainModel.Instance.gameID));
        }

        /// <summary>
        /// 音量按钮点击：开启/关闭声音。
        /// </summary>
        protected virtual void OnClickSoundButton()
        {
            if (silderSound == null)
            {
                return;
            }

            float maxValue = Mathf.Max(1f, (float)silderSound.max);
            float currentVolume = Mathf.Clamp01((float)silderSound.value / maxValue);
            bool isMute = GSManager.Instance.IsMute || currentVolume <= 0.001f;
            if (isMute)
            {
                float restoreVolume = Mathf.Clamp01(_lastNonMuteVolume);
                GSManager.Instance.SetMute(false);
                GSManager.Instance.SetVolume(restoreVolume);
                silderSound.value = restoreVolume * silderSound.max;
                UpdateSoundButtonState(restoreVolume, false);
            }
            else
            {
                _lastNonMuteVolume = Mathf.Clamp01(currentVolume);
                GSManager.Instance.SetVolume(0f);
                GSManager.Instance.SetMute(true);
                silderSound.value = 0f;
                UpdateSoundButtonState(0f, true);
            }

            GlobalSoundHelper.Instance.PlaySoundEff(GameMaker.SoundKey.NormalClick);
        }

        /// <summary>
        /// 音量滑动条变化：同步音量、静音状态和按钮控制器。
        /// </summary>
        protected virtual void OnSoundSliderChanged()
        {
            if (silderSound == null)
            {
                return;
            }

            float maxValue = Mathf.Max(1f, (float)silderSound.max);
            float volume = Mathf.Clamp01((float)silderSound.value / maxValue);
            bool isMute = volume <= 0.001f;
            if (!isMute)
            {
                _lastNonMuteVolume = volume;
            }

            GSManager.Instance.SetMute(isMute);
            GSManager.Instance.SetVolume(volume);
            UpdateSoundButtonState(volume, isMute);
        }

        /// <summary>
        /// 从当前声音设置同步 UI（滑动条与按钮状态）。
        /// </summary>
        protected virtual void SyncSoundUIFromCurrentState()
        {
            if (silderSound == null)
            {
                return;
            }

            float currentVolume = GSManager.Instance.TotalVolumeMusic;
            bool isMute = GSManager.Instance.IsMute;
            float uiVolume = isMute ? 0f : Mathf.Clamp01(currentVolume);
            if (!isMute && uiVolume > 0.001f)
            {
                _lastNonMuteVolume = uiVolume;
            }

            silderSound.value = uiVolume * silderSound.max;
            UpdateSoundButtonState(uiVolume, isMute);
        }

        /// <summary>
        /// 更新声音按钮控制器：0=开启，1=关闭。
        /// </summary>
        protected virtual void UpdateSoundButtonState(float volume, bool isMute)
        {
            if (btnSound == null)
            {
                return;
            }

            int selectedIndex = (isMute || volume <= 0.001f) ? 1 : 0;
            btnSound.GetController("button").selectedIndex = selectedIndex;
        }

        /// <summary>
        /// 舞台触摸抬起时重置音量按钮缩放状态。
        /// </summary>
        protected virtual void OnStageTouchEndResetSoundButton()
        {
            if (!_isSoundBtnPressed)
            {
                return;
            }

            _isSoundBtnPressed = false;
            if (btnSound != null)
            {
                btnSound.SetScale(1f, 1f);
            }
        }

        /// <summary>
        /// SetPanel 弹出时点击蒙层关闭设置面板，不改变 mash 层级。
        /// </summary>
        protected virtual void OnClickMashCloseSetPanel()
        {
            if (setPanel == null || !setPanel.visible)
            {
                return;
            }

            Help();
        }

        /// <summary>
        /// 打开/关闭设置面板，同时切换蒙层与 Spin 按钮可交互状态。
        /// </summary>
        protected virtual void Help()
        {
            GlobalSoundHelper.Instance.PlaySoundEff(GameMaker.SoundKey.Tab);
            btnHelp.SetScale(1f, 1f);
            isSet = !isSet;
            if (isSet)
            {
                setPanel.visible = true;
                gOwnerPanel.GetChild("mash").asGraph.visible = true;
                spinBtnCtrl.goOwnerSpin.GetController("button").selectedPage = "hui";
                spinBtnCtrl.goOwnerSpin.touchable = false;
                //隐藏展会模式UI
                SetExhibitionUIState(false);
                SetBetUIState(false);
            }
            else
            {
                setPanel.visible = false;
                gIntroducePanel.visible = false;
                gOwnerPanel.GetChild("mash").asGraph.visible = false;
                spinBtnCtrl.goOwnerSpin.GetController("button").selectedPage = "stop";
                spinBtnCtrl.goOwnerSpin.touchable = true;
                //显示展会模式UI
                SetExhibitionUIState(true);
                SetBetUIState(true);
            }
        }

        /// <summary>
        /// 赔付表初始化到第一页并刷新翻页按钮状态。
        /// </summary>
        protected virtual void IntroduceInit()
        {
            IntroduceIndex = 0;
            SetIntroducePage(IntroduceIndex);
            btnPrev.touchable = false;
            btnPrev.GetChild("untouch").visible = true;
            btnNext.touchable = true;
            btnNext.GetChild("untouch").visible = false;

            //gIntroducePanel.GetChild("btnController").asCom.GetController("c1").selectedIndex = IntroduceIndex;
        }

        /// <summary>
        /// 向左翻页并更新边界按钮状态
        /// </summary>
        protected virtual void OnClickIntroduceL()
        {
            // 向左翻页并更新边界按钮状态
            IntroduceChange(false);
            GlobalSoundHelper.Instance.PlaySoundEff(GameMaker.SoundKey.NormalClick);
            if (IntroduceIndex == 0)
            {
                btnPrev.touchable = false;
                btnPrev.GetChild("untouch").visible = true;
            }
            else
            {
                btnNext.GetChild("untouch").visible = false;
                btnNext.touchable = true;
            }
        }

        /// <summary>
        /// 向右翻页并更新边界按钮状态
        /// </summary>
        protected virtual void OnClickIntroduceR()
        {
            // 向右翻页并更新边界按钮状态
            IntroduceChange(true);
            GlobalSoundHelper.Instance.PlaySoundEff(GameMaker.SoundKey.NormalClick);
            if (IntroduceIndex == PayTableLength - 1)
            {
                btnNext.touchable = false;
                btnNext.GetChild("untouch").visible = true;
            }
            else
            {
                btnPrev.touchable = true;
                btnPrev.GetChild("untouch").visible = false;
            }
        }

        /// <summary>
        /// 赔付表翻页核心逻辑。
        /// </summary>
        protected virtual void IntroduceChange(bool jia)
        {
            if (jia)
            {
                IntroduceIndex += 1;
            }
            else
            {
                IntroduceIndex -= 1;
            }

            if (IntroduceIndex < PayTableLength)
            {
                // 切换展示页并同步底部页码控制器
                SetIntroducePage(IntroduceIndex);
            }
        }

        /// <summary>
        /// 说明书页通常是纯展示内容，禁用触摸避免遮挡翻页按钮点击区域。
        /// </summary>
        protected virtual void SetIntroducePage(int pageIndex)
        {
            if (Introduce == null || MainModel.Instance?.contentMD?.goPayTableLst == null)
            {
                return;
            }

            if (pageIndex < 0 || pageIndex >= MainModel.Instance.contentMD.goPayTableLst.Length)
            {
                return;
            }

            GComponent page = MainModel.Instance.contentMD.goPayTableLst[pageIndex];
            if (page == null)
            {
                return;
            }

            page.touchable = false;
            Introduce.RemoveChildren();
            Introduce.AddChild(page);
        }

        /// <summary>
        /// 说明书页返回游戏主页面。
        /// </summary>
        protected virtual void OnClickBackGame()
        {
            GlobalSoundHelper.Instance.PlaySoundEff(GameMaker.SoundKey.PopupClose);
            gIntroducePanel.visible = false;
            setPanel.visible = false;
            gIntroducePanel.GetChild("mask").asGraph.visible = false;

            spinBtnCtrl.goOwnerSpin.GetController("button").selectedPage = "stop";
            spinBtnCtrl.goOwnerSpin.touchable = true;
            btnHelp.touchable = true;
            btnBetDown.touchable = true;
            btnBetUp.touchable = true;
            SetExhibitionUIState(true);
            SetBetUIState(true);
        }

        /// <summary>
        /// 返回大厅
        /// </summary>
        protected virtual void BackHall()
        {
            Debug.Log("返回大厅:"+MainModel.Instance.gameID);
            PagLifecycle.ReleaseCurrentGame();
            switch (MainModel.Instance.gameID)
            {
                case 1700:
                    PageManager.Instance.ClosePage(PageName.SlotZhuZaiJinBiPageGameMain);
                    break;
                case 3993:
                    PageManager.Instance.ClosePage(PageName.MeiZhouHeiBaoPageGameMain);
                    break;
                case 3994:
                    PageManager.Instance.ClosePage(PageName.FeiZhouHeiXingXingPageGameMain);
                    break;
                case 3995:
                    PageManager.Instance.ClosePage(PageName.HuoYanGongNiuPageGameMain);
                    break;
                case 3999:
                    PageManager.Instance.ClosePage(PageName.CaiFuZhiMenPageGameMain);
                    break;
                case 3998:
                    PageManager.Instance.ClosePage(PageName.XingYunZhiLunPageGameMain);
                    break;
                case 3997:
                    PageManager.Instance.ClosePage(PageName.CaiFuZhiJiaPageGameMain);
                    break;
                case 3996:
                    PageManager.Instance.ClosePage(PageName.CaiFuHuoChePageGameMain);
                    break;
                case 4000:
                    PageManager.Instance.ClosePage(PageName.SlotFanBeiChaoRenPageGameMain);
                    break;
                case 4001:
                    PageManager.Instance.ClosePage(PageName.SlotCkmTestPageGameMain);
                    break;
            }

            if (ThemeRuntime.HasCurrent)
                ThemeRuntime.Current.ReturnToHall();
            else
                Debug.LogError($"[PanelBaseController] ThemeRuntime 无当前 IThemeEntry: {ThemeRuntime.SelectedKind}，无法返回大厅");
        }

        protected virtual void OnPropertyChange(EventData res = null)
        {
            // 根据属性名分发对应刷新逻辑
            string name = res.name;
            switch (name)
            {
                case "ContentModel/totalBet":
                    OnPropertyChangeTotalBet(res);
                    break;
                case "SBoxModel/betList":
                    OnPropertyChangeBetList(res);
                    break;
                case "ContentModel/btnSpinState":
                    OnPropertyChangeBtnSpinState(res);
                    break;
                case "ContentModel/gameState":
                    OnPropertyGameState(res);
                    break;
                case "SBoxModel/isConnectMoneyBox":
                    OnPropertyIsConnectMoneyBox(res);
                    break;
            }
        }

        /// <summary>
        /// UI状态改变:状态
        /// </summary>
        public virtual void OnContentChang(EventData res = null)
        {
            if (res == null) return;
            if (res.value == null) return;
           string value = res.value.ToString();
           
            switch (value)
            {
                case "BeginBonusFreeSpin":
                    {
                        SetExhibitionUIState(false);
                        SetBetUIState(false);
                    }
                    break;

                case "EndBonusFreeSpin":
                    {
                        SetExhibitionUIState(true);
                        SetBetUIState(true);
                    }
                    break;
            }
        }

        //  panel ctl  --> game ctl  --> model -->  panel ctl
        protected virtual void OnPropertyChangeTotalBet(EventData res = null)
        {
            //long totalBet = (long)res?.value;

            //if (totalBet == null)
            //    totalBet = MainModel.Instance.contentMD.totalBet;
        }

        protected virtual void OnPropertyChangeBetList(EventData res = null)
        {
            // betList 变化后校正下注索引并刷新 UI
            List<long> betList = (List<long>)res?.value;

            if (betList == null)
                betList = SBoxModel.Instance.betList;

            if (betList == null || betList.Count == 0 || MainModel.Instance?.contentMD == null)
                return;

            int betIndex = MainModel.Instance.contentMD.betIndex;
            if (betIndex < 0)
                betIndex = 0;
            if (betIndex >= betList.Count)
                betIndex = betList.Count - 1;

            // 同步下注金额与界面文案
            MainModel.Instance.contentMD.betIndex = betIndex;
            MainModel.Instance.contentMD.totalBet = betList[betIndex];

            if (gOwnerPanel != null && bet != null && btnBetDown != null && btnBetUp != null)
            {
                ChangeBetButtonInteractable(betIndex, betList.Count);
            }

            // 如果当前还没开始旋转，重新下发当前下注到机台
            if (MainModel.Instance.contentMD.gameState == GameState.Idle)
            {
                try
                {
                    RequestSetBetWithDedup((int)MainModel.Instance.contentMD.totalBet, (callbackRes) =>
                    {
                        // 这里只需要保证下注值刷新，不做额外 UI 变更
                    });
                }
                catch (Exception ex)
                {
                    // 异常仅记录日志，避免中断主流程
                    DebugUtils.LogError($"[PanelBaseController] RequestSetBet after betList refresh failed: {ex}");
                }
            }
        }

        public virtual void SetBetUIState(bool Stete)
        {
            if (Stete)
            {
                btnBetUp.GetChild("untouch").visible = false;
                btnBetUp.touchable = true;
                btnBetDown.GetChild("untouch").visible = false;
                btnBetDown.touchable = true;

                if (MainModel.Instance.contentMD.betIndex == 0)
                {
                    btnBetDown.GetChild("untouch").visible = true;
                    btnBetDown.touchable = false;
                }

                if (MainModel.Instance.contentMD.betIndex == 7)
                {
                    btnBetUp.GetChild("untouch").visible = true;
                    btnBetUp.touchable = false;
                }
            }
            else
            {
                btnBetUp.GetChild("untouch").visible = true;
                btnBetUp.touchable = false;
                btnBetDown.GetChild("untouch").visible = true;
                btnBetDown.touchable = false;
            }
        }

        /// <summary>
        /// 根据 Spin 状态切换按钮样式及其他按钮可交互状态。
        /// </summary>
        protected virtual void OnPropertyChangeBtnSpinState(EventData res = null)
        {
            string changeSpinState = (string)res?.value;

            if (changeSpinState == null)
                changeSpinState = "Stop";

            if (gOwnerPanel == null) return;


            switch (changeSpinState)
            {
                case SpinButtonState.Stop:
                    {
                        if (!_isSpinStopButtonLocked)
                        {
                            spinBtnCtrl.State = "Stop";
                        }
                        ChangButtonNo(false);
                    }
                    break;
                case SpinButtonState.Spin:
                    {
                        if (!_isSpinStopButtonLocked)
                        {
                            spinBtnCtrl.State = "Spin";
                        }

                        ChangButtonNo(true);
                    }
                    break;
                case SpinButtonState.Auto:
                    {
                        if (!_isSpinStopButtonLocked)
                        {
                            spinBtnCtrl.State = "Auto";
                        }

                        ChangButtonNo(true);
                    }
                    break;
            }
        }

        public virtual bool IsSpinStopButtonLocked => _isSpinStopButtonLocked;

        public virtual void SetSpinButtonLocked(bool locked)
        {
            _isSpinStopButtonLocked = locked;
            if (spinBtnCtrl?.goOwnerSpin == null)
            {
                return;
            }

            if (locked)
            {
                spinBtnCtrl.goOwnerSpin.touchable = false;
                spinBtnCtrl.goOwnerSpin.GetController("button").selectedPage = "hui";
                return;
            }

            spinBtnCtrl.goOwnerSpin.touchable = true;
            string spinState = MainModel.Instance?.contentMD?.btnSpinState ?? SpinButtonState.Stop;
            spinBtnCtrl.State = spinState;
        }

        /// <summary>
        /// 游戏状态变更处理：进入 Spin 时清空赢分展示。
        /// </summary>
        protected virtual void OnPropertyGameState(EventData res = null)
        {
            string gameState = (string)res?.value;

            if (gameState == GameState.Spin)
            {
                win.text = 0.ToString();
                ClearSingleLineText();
            }
        }

        protected virtual void OnPropertyIsConnectMoneyBox(EventData res = null)
        {
        }

        protected virtual void OnPanelEventAnchorPanelChange(EventData res = null)
        {
            if (res == null || res.name != PanelEvent.AnchorPanelChange)
            {
                return;
            }

            GComponent newAnchorPanel = res.value as GComponent;
            if (newAnchorPanel == null)
            {
                return;
            }

            // 锚点面板切换后重建 UI 绑定
            Init(res);
        }

        /// <summary>
        /// 处理总赢分、单次奖励、单线赢分等事件并更新文本。
        /// </summary>
        protected virtual void OnTotalWinCredit(EventData receivedEvent)
        {
            if (receivedEvent.name == SlotMachineEvent.TotalWinCredit)
            {
                long totalWinCredit = (long)receivedEvent.value;
                win.text = totalWinCredit.ToString();
                ClearSingleLineText();
            }
            else if (receivedEvent.name == SlotMachineEvent.SingleWinBonus)
            {
                long totalWinCredit = (long)receivedEvent.value;
                NumberAnimation.Instance.AnimateNumber(win, long.Parse(win.text), totalWinCredit + long.Parse(win.text),
                    0.4f);
            }
            else if (receivedEvent.name == SlotMachineEvent.SingleWinLine)
            {
                SymbolWin symbolWin = receivedEvent.value as SymbolWin;
                if (symbolWin == null)
                {
                    ClearSingleLineText();
                    return;
                }

                // 单线
                singleLine.text =
                    $"Line : {symbolWin.lineNumber}  symbolType : {symbolWin.symbolNumber}   Win :{symbolWin.earnCredit}";
            }
            else if (receivedEvent.name == SlotMachineEvent.SkipWinLine)
            {
                ClearSingleLineText();
            }
        }

        protected virtual void ClearSingleLineText()
        {
            if (singleLine != null)
            {
                singleLine.text = string.Empty;
            }
        }

        protected virtual void OnUpdateNaviCredit(EventData receivedEvent = null)
        {
            if (gOwnerPanel == null) return;

            bool isAmin = false;
            long fromCredit = 0;
            long toCredit = 0;
            if (receivedEvent == null || receivedEvent.value == null)
            {
                isAmin = false;
                toCredit = MainBlackboardController.Instance.myTempCredit;
            }
            else
            {
                UpdateNaviCredit data = (UpdateNaviCredit)receivedEvent.value;

                isAmin = data.isAnim;
                fromCredit = data.fromCredit;
                toCredit = data.toCredit;
            }


            if (isAmin)
            {
                // 需要动画时做数字滚动
                NumberAnimation.Instance.AnimateNumber(gOwnerPanel.GetChild("credit").asTextField, fromCredit,
                    toCredit);
            }
            else
            {
                // 不需要动画时直接设置最终值
                NumberAnimation.Instance.PauseTextFieldAnimation(gOwnerPanel.GetChild("credit").asTextField);
                if (gOwnerPanel.GetChild("credit").asTextField != null)
                {
                    gOwnerPanel.GetChild("credit").asTextField.text = toCredit.ToString();
                }
            }
        }

        // 向面板输入事件总线派发 Spin 按钮点击（长按/短按）
        public void OnClickSpinButton(bool isLong)
        {
            EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT,new EventData<bool>(PanelEvent.SpinButtonClick, isLong));
        }

        /// <summary> 播放屏幕/机台 Spin 短按特效。 </summary>
        public void PlaySpinShortPressEffect() => spinBtnCtrl?.PlayShortPressEffect();
        /// <summary> 播放屏幕/机台 Spin 长按循环特效。 </summary>
        public void PlaySpinLongPressEffect() => spinBtnCtrl?.PlayLongPressEffect();
        /// <summary> 停止 Spin 长按循环特效。 </summary>
        public void StopSpinLongPressEffect() => spinBtnCtrl?.StopLongPressEffect();
        /// <summary> 隐藏短按与长按特效，进入游戏或关面板时使用。 </summary>
        public void HideSpinPressEffects() => spinBtnCtrl?.HideAllPressEffects();
        /// <summary> 机台物理键按下，开始长按预览计时。 </summary>
        public void NotifySpinPressBegin() => spinBtnCtrl?.OnPressBegin();
        /// <summary> 机台物理键抬起，取消长按预览并关闭循环特效。 </summary>
        public void NotifySpinPressEnd() => spinBtnCtrl?.OnPressEnd();

        //展会模式按钮
        private void BindColumnButtons()
        {
            if (btnColUps == null || btnColDowns == null)
            {
                return;
            }

            for (int i = 0; i < btnColUps.Count; i++)
            {
                int colIndex = i;
                btnColUps[i].onClick.Clear();
                btnColUps[i].onClick.Add(() => OnClickButtonColUp(colIndex));
                if (!MainModel.Instance.isExhibitionModeMode)
                {
                    btnColUps[i].visible = false;
                }
            }
            for (int i = 0; i < btnColDowns.Count; i++)
            {
                int colIndex = i;
                btnColDowns[i].onClick.Clear();
                btnColDowns[i].onClick.Add(() => OnClickButtonColDown(colIndex));
                if (!MainModel.Instance.isExhibitionModeMode)
                {

                    btnColDowns[i].visible = false;
                }
            }
            if (!MainModel.Instance.isExhibitionModeMode)
            {
                btnExhibition.GetChild("closeGroup").asGroup.visible = false;
            }
        }

        //滚轴上移一格
        public void OnClickButtonColUp(int col)
        {
            EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT,new EventData<int>(PanelEvent.ColUpButtonClick, col));
        }

        //滚轴下移一格
        public void OnClickButtonColDown(int col)
        {
            EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, new EventData<int>(PanelEvent.ColDownButtonClick, col));
        }
        //开启关闭展会模式
        public void OnClickExhibition()
        {
            MainModel.Instance.isExhibitionModeMode = !MainModel.Instance.isExhibitionModeMode;
            btnExhibition.GetChild("closeGroup").asGroup.visible = MainModel.Instance.isExhibitionModeMode;
            for (int i = 0; i < btnColUps.Count; i++)
            {
                btnColUps[i].visible = MainModel.Instance.isExhibitionModeMode;
            }

            for (int i = 0; i < btnColDowns.Count; i++)
            {
                btnColDowns[i].visible = MainModel.Instance.isExhibitionModeMode;
            }
        }

        public void SetExhibitionUIState(bool state)
        {
            if (MainModel.Instance.isExhibitionModeMode)
            {
                for (int i = 0; i < btnColUps.Count; i++)
                {
                    btnColUps[i].visible = state;
                }

                for (int i = 0; i < btnColDowns.Count; i++)
                {
                    btnColDowns[i].visible = state;
                }
            }

            // SetPanel 打开时关掉展会层热区，避免挡住 mash 点击
            if (ExhibitionPanel != null)
            {
                ExhibitionPanel.touchable = state;
            }
         
            btnExhibition.visible = state;
        }

        //置灰
        public virtual void ChangButtonNo(bool can)
        {
            Color normalColor = Color.white;
            Color disableColor = new Color(0.5f, 0.5f, 0.5f, 1f);

            if (can)
            {
                btnHelp.GetChild("untouch").visible = true;
                btnHelp.touchable = false;
                btnBetUp.GetChild("untouch").visible = true;
                btnBetUp.touchable = false;
                btnBetDown.GetChild("untouch").visible = true;
                btnBetDown.touchable = false;

                for (int i = 0; i < btnColUps.Count; i++)
                {
                    btnColUps[i].touchable = false;
                    btnColUps[i].GetChildAt(0).asImage.color = disableColor;
                }
                for (int i = 0; i < btnColDowns.Count; i++)
                {
                    btnColDowns[i].touchable = false;
                    btnColDowns[i].GetChildAt(0).asImage.color = disableColor;
                }
            }
            else
            {
                btnHelp.GetChild("untouch").visible = false;
                btnHelp.touchable = true;
                btnBetUp.GetChild("untouch").visible = false;
                btnBetUp.touchable = true;
                btnBetDown.GetChild("untouch").visible = false;
                btnBetDown.touchable = true;

                if (MainModel.Instance.contentMD.betIndex == 0)
                {
                    btnBetDown.GetChild("untouch").visible = true;
                    btnBetDown.touchable = false;
                }

                if (MainModel.Instance.contentMD.betIndex == 7)
                {
                    btnBetUp.GetChild("untouch").visible = true;
                    btnBetUp.touchable = false;
                }

                for (int i = 0; i < btnColUps.Count; i++)
                {
                    btnColUps[i].touchable = true;
                    btnColUps[i].GetChildAt(0).asImage.color = normalColor;
                }
                for (int i = 0; i < btnColDowns.Count; i++)
                {
                    btnColDowns[i].touchable = true;
                    btnColDowns[i].GetChildAt(0).asImage.color = normalColor;
                }
            }
        }

        protected virtual void OnClickButtonBetUp()
        {
            GlobalSoundHelper.Instance.PlaySoundEff(SoundKey.BetUp);

            //soundHelper.PlaySoundEff(GameMaker.SoundKey.BetUp);
            List<long> betList = SBoxModel.Instance.betList;
            int betIndex = MainModel.Instance.contentMD.betIndex;
            if (++betIndex >= betList.Count)
            {
                betIndex = betList.Count - 1;
            }

            MainModel.Instance.contentMD.totalBet = betList[betIndex];
            // 下注变更后同步机台，并在回调内刷新加减注按钮状态
            RequestSetBetWithDedup((int)MainModel.Instance.contentMD.totalBet, (res) =>
            {
                ChangeBetButtonInteractable(betIndex, betList.Count);
            });
        }

        protected virtual void OnClickButtonBetDown()
        {
            GlobalSoundHelper.Instance.PlaySoundEff(SoundKey.BetDown);

            //soundHelper.PlaySoundEff(GameMaker.SoundKey.BetDown);
            List<long> betList = SBoxModel.Instance.betList;

            int betIndex = MainModel.Instance.contentMD.betIndex;
            if (--betIndex < 0)
            {
                betIndex = 0;
            }

            MainModel.Instance.contentMD.totalBet = betList[betIndex];
            // 下注变更后同步机台，并在回调内刷新加减注按钮状态
            RequestSetBetWithDedup((int)MainModel.Instance.contentMD.totalBet, (res) =>
            {
                ChangeBetButtonInteractable(betIndex, betList.Count);
            });
        }

        /// <summary>
        /// 下发下注值：去重同值请求，并在请求进行中合并为“最后一次下注”。
        /// </summary>
        protected virtual void RequestSetBetWithDedup(int betValue, Action<object> finishCallback = null)
        {
            // 当前无请求进行，且下注值和上次一致时直接跳过
            if (!_isRequestSetBetInFlight && betValue == _lastRequestedBet)
            {
                finishCallback?.Invoke(null);
                return;
            }

            if (_isRequestSetBetInFlight)
            {
                // 请求中仅保留最后一次下注，避免短时间重复下发
                _pendingRequestBet = betValue;
                _pendingRequestSetBetCallback = finishCallback;
                return;
            }

            SendSetBetRequest(betValue, finishCallback);
        }

        protected virtual void SendSetBetRequest(int betValue, Action<object> finishCallback = null)
        {
            _isRequestSetBetInFlight = true;
            _lastRequestedBet = betValue;

            try
            {
                SBoxPlayerBetsData sBoxPlayerBetsData = new SBoxPlayerBetsData()
                {
                    PlayerId = SBoxModel.Instance.pid, balance = 0, rfu = 0
                };
                sBoxPlayerBetsData.Bets[0] = betValue;

                ERPushMachineDataManager02.Instance.RequestSetBet(sBoxPlayerBetsData, (res) =>
                {
                    _isRequestSetBetInFlight = false;
                    finishCallback?.Invoke(res);

                    if (_pendingRequestBet.HasValue)
                    {
                        int pendingBet = _pendingRequestBet.Value;
                        Action<object> pendingCallback = _pendingRequestSetBetCallback;
                        _pendingRequestBet = null;
                        _pendingRequestSetBetCallback = null;

                        if (pendingBet != _lastRequestedBet)
                        {
                            SendSetBetRequest(pendingBet, pendingCallback);
                        }
                        else
                        {
                            pendingCallback?.Invoke(res);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _isRequestSetBetInFlight = false;
                _pendingRequestBet = null;
                _pendingRequestSetBetCallback = null;
                DebugUtils.LogError($"[PanelBaseController] RequestSetBet failed, bet={betValue}, ex={ex}");
            }
        }

        /// <summary>
        /// 统一处理加减注按钮可点击状态与下注文本刷新。
        /// </summary>
        protected virtual void ChangeBetButtonInteractable(int? betIndex01 = null, int? betListCount01 = null)
        {
            if (betIndex01 != null && betListCount01 != null)
            {
                curBetIndex = (int)betIndex01;
                curBetListCount = (int)betListCount01;
            }

            MainModel.Instance.contentMD.betIndex = curBetIndex;
            //下注倍数现在硬数据,之后在改动
            int lineNum = MainModel.Instance.lineNum;
            long totalBet = MainModel.Instance.contentMD.totalBet;
            if (lineNum <= 0)
            {
                DebugUtils.LogError(
                    $"[PanelBaseController] ChangeBetButtonInteractable: invalid lineNum={lineNum}, totalBet={totalBet}, curBetIndex={curBetIndex}. Call stack:\n{Environment.StackTrace}");
                MainModel.Instance.contentMD.betmultiple = 0;
            }
            else
            {
                MainModel.Instance.contentMD.betmultiple = (int)totalBet / lineNum;
            }
            bet.text = MainModel.Instance.contentMD.totalBet.ToString();
            btnBetDown.touchable = curBetIndex > 0;
            btnBetDown.GetChild("untouch").visible = btnBetDown.touchable ? false : true;
            btnBetUp.touchable = curBetIndex < curBetListCount - 1;
            btnBetUp.GetChild("untouch").visible = btnBetUp.touchable ? false : true;
        }

        public virtual void OnLongClickHandler(MachineButtonKey machineButtonKey) { }

        /// <summary> 机台短按回调：有短按特效的机台在此播放。 </summary>
        public virtual void OnShortClickHandler(MachineButtonKey machineButtonKey)
        {
            if (machineButtonKey == MachineButtonKey.BtnSpin)
                PlaySpinShortPressEffect();
        }

        /// <summary> 机台按下回调：Stop 态下启动长按预览。 </summary>
        public virtual void OnDownClickHandler(MachineButtonKey machineButtonKey)
        {
            switch (machineButtonKey)
            {
                case MachineButtonKey.BtnSpin:
                    {
                        NotifySpinPressBegin();
                    }
                    break;
            }
        }

        /// <summary> 机台抬起回调：关闭长按循环特效。 </summary>
        public virtual void OnUpClickHandler(MachineButtonKey machineButtonKey)
        {
            switch (machineButtonKey)
            {
                case MachineButtonKey.BtnSpin:
                    {
                        NotifySpinPressEnd();
                    }
                    break;
            }
        }
    }
}