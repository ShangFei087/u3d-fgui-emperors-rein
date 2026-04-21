using FairyGUI;
using GameMaker;
using Newtonsoft.Json;
using PusherEmperorsRein;
using SBoxApi;
using System;
using System.Collections.Generic;
using TestHall;
using UnityEngine;
using static NetButtonManager;
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
        // 当前弹窗状态（设置、帮助、赔付表等）
        PopState popState = PopState.None;
        // 面板根节点与常用子面板引用
        protected GComponent gOwnerPanel, gIntroducePanel, setPanel, btnSound, btnHelp, Introduce;
        // Spin 按钮控制器
        protected SpinButtonBaseController spinBtnCtrl = new SpinButtonBaseController();
        // 赔付表翻页与导航按钮
        protected GButton btnPayTable, btnPrev, btnNext,btnHome, btnBackGame;
        // 常用文本显示：下注、总赢分、单线赢分
        protected GTextField bet,win, singleLine;
        //声音滑动条
        protected GSlider silderSound;
        // 当前是否处于设置弹窗状态
        protected bool isSet;
        // 赔付表总页数
        protected int PayTableLength =0;
        // 标记音量按钮是否处于按下状态（用于全局抬起恢复）
        protected bool _isSoundBtnPressed;
        // 记录最近一次非静音音量，用于按钮恢复声音时回滚
        protected float _lastNonMuteVolume = 1f;
        
        //下注按钮So
        protected GButton btnBetDown, btnBetUp;
        protected int curBetIndex = 0;
        protected int curBetListCount = 1;

        // Spin 预制体实例引用
        GameObject goSpin;
        // 是否已完成初始化
        bool isInit;
        public int IntroduceIndex;
        public int VolumeLevel;

        protected virtual int IntroduceIndexMax => 6;
        protected virtual string PanelPackageName => "Panel01";
        protected virtual string PanelPackagePath => "Assets/GameRes/Games/Panel01/FGUIs";
        protected virtual string PanelUrl => "ui://Panel01/Panel";
        protected virtual string SpinPrefabPath => "Assets/GameRes/Games/Panel01/Prefabs/Slot_btn_Spin.prefab";

        /// <summary>
        /// 面板启用：注册事件并初始化 UI。
        /// </summary>
        protected virtual void OnEnable()
        {
            EventCenter.Instance.AddEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, OnPropertyChange);
            EventCenter.Instance.AddEventListener<EventData>(SlotMachineEvent.ON_WIN_EVENT, OnTotalWinCredit);
            EventCenter.Instance.AddEventListener<EventData>(MetaUIEvent.ON_CREDIT_EVENT, OnUpdateNaviCredit);
            EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_EVENT, OnPanelEventAnchorPanelChange);

            MainModel.Instance.panel = this;

            Init();
        }

        /// <summary>
        /// 面板禁用：移除事件并重置按钮状态。
        /// </summary>
        protected virtual void OnDisable()
        {
            EventCenter.Instance.RemoveEventListener<EventData>(Observer.ON_PROPERTY_CHANGED_EVENT, OnPropertyChange);
            EventCenter.Instance.RemoveEventListener<EventData>(SlotMachineEvent.ON_WIN_EVENT, OnTotalWinCredit);
            EventCenter.Instance.RemoveEventListener<EventData>(MetaUIEvent.ON_CREDIT_EVENT, OnUpdateNaviCredit);
            EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_EVENT, OnPanelEventAnchorPanelChange);
            Stage.inst.onTouchEnd.Remove(OnStageTouchEndResetSoundButton);
            if (silderSound != null)
            {
                silderSound.onChanged.Clear();
            }
            _isSoundBtnPressed = false;
            if (btnSound != null)
            {
                btnSound.SetScale(1f, 1f);
            }
            gOwnerPanel.visible = false;
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

            int count = 2;
            Action loadComplete = () =>
            {
                // 两个异步资源都完成后再进行参数初始化
                if (--count == 0)
                {
                    isInit = true;
                    InitParam();
                }
            };


            if (gOwnerPanel != _goAnchorPanel && _goAnchorPanel != null)
            {
                if (UIPackage.GetByName(PanelPackageName) == null)
                {
                    // 首次进入时先加载 FairyGUI 包
                    ResourceManager02.Instance.LoadAssetBundleAsync(PanelPackagePath, (ab) =>
                    {
                        UIPackage.AddPackage(ab);
                        GLoader anchorPanel = _goAnchorPanel.GetChild("icon").asLoader;
                        anchorPanel.url = PanelUrl;
                        gOwnerPanel = _goAnchorPanel.GetChild("icon").asLoader.component;
                        gOwnerPanel.visible = true;
                        loadComplete();
                    });

                }
                else
                {
                    // 已加载过包时直接复用
                    GLoader anchorPanel = _goAnchorPanel.GetChild("icon").asLoader;
                    anchorPanel.url = PanelUrl;

                    gOwnerPanel = _goAnchorPanel.GetChild("icon").asLoader.component;
                    loadComplete();
                }
            }

            // 异步加载 Spin 按钮预制体
            ResourceManager02.Instance.LoadAsset<GameObject>(SpinPrefabPath,
              (GameObject clone) =>
              {
                  goSpin = clone;
                  loadComplete();
              });


        }

        /// <summary>
        /// 绑定 UI、注册按钮事件并同步初始数据。
        /// </summary>
        protected virtual void InitParam()
        {
            Debug.Log("初始化菜单Ui");
            gOwnerPanel = MainModel.Instance.contentMD.goAnthorPanel.asCom.GetChild("icon").asLoader.component;
            setPanel = gOwnerPanel.GetChild("setPanel").asCom;
            setPanel.visible = false;
            gOwnerPanel.GetChild("credit").asTextField.text = MainModel.Instance.myCredit.ToString(); //SBoxModel.Instance.myCredit.ToString();
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

            singleLine= gOwnerPanel.GetChild("singleLine").asTextField;
            singleLine.text = "";

            // 初始化时将当前下注同步到机台
            SBoxPlayerBetsData sBoxPlayerBetsData = new SBoxPlayerBetsData()
            {
                PlayerId = SBoxModel.Instance.pid,
                balance = 0,
                rfu = 0
            };

            sBoxPlayerBetsData.Bets[0] =(int) SBoxModel.Instance.betList[MainModel.Instance.contentMD.betIndex];
            // 设置押注
            ERPushMachineDataManager02.Instance.RequestSetBet(sBoxPlayerBetsData, (res) =>
            {
                ChangeBetButtonInteractable(MainModel.Instance.contentMD.betIndex, SBoxModel.Instance.betList.Count);
            });

            spinBtnCtrl.InitParam(gOwnerPanel.GetChild("btnSpin").asCom, "Stop", OnClickSpinButton, goSpin);

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
            btnHome =setPanel.GetChild("btnHome").asButton;
            btnHome.onClick.Clear();
            btnHome.onClick.Add(() =>
            {
                setPanel.visible = false;
                Help();
                BackHall();
            });
            OnPropertyChangeBetList();
            OnPropertyChangeTotalBet();
            OnPropertyChangeBtnSpinState();
            OnPropertyIsConnectMoneyBox();
            SyncSoundUIFromCurrentState();

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
        /// 打开/关闭设置面板，同时切换蒙层与 Spin 按钮可交互状态。
        /// </summary>
        protected virtual void Help()
        {
            btnHelp.SetScale(1f, 1f);
            isSet = !isSet;
            if (isSet)
            {
                setPanel.visible = true;
                gOwnerPanel.GetChild("mash").asGraph.visible = true;
                spinBtnCtrl.goOwnerSpin.GetController("button").selectedPage = "hui";
                spinBtnCtrl.goOwnerSpin.touchable = false;
            }
            else
            {

                setPanel.visible = false;
                gIntroducePanel.visible = false;
                gOwnerPanel.GetChild("mash").asGraph.visible = false;
                spinBtnCtrl.goOwnerSpin.GetController("button").selectedPage = "stop";
                spinBtnCtrl.goOwnerSpin.touchable = true;
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
            gIntroducePanel.visible = false;
            setPanel.visible = false;
            gIntroducePanel.GetChild("mask").asGraph.visible = false;

            spinBtnCtrl.goOwnerSpin.GetController("button").selectedPage = "stop";
            spinBtnCtrl.goOwnerSpin.touchable = true;
            btnHelp.touchable = true;
            btnBetDown.touchable = true;
            btnBetUp.touchable = true;
        }

        /// <summary>
        /// 返回大厅
        /// </summary>
        protected virtual void BackHall()
        {
            Debug.Log("返回大厅:");
            Debug.Log(MainModel.Instance.gameID);

            switch (MainModel.Instance.gameID)
            {
                case 1700:
                    PageManager.Instance.ClosePage(PageName.SlotZhuZaiJinBiPageGameMain);
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
            }

            if (!ApplicationSettings.Instance.isMock)
            {
                PageManager.Instance.OpenPage(PageName.Hall01);
            }
            else
            {
                PageManager.Instance.OpenPage(PageName.Hall01);
            }
              
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
                    SBoxPlayerBetsData sBoxPlayerBetsData = new SBoxPlayerBetsData()
                    {
                        PlayerId = SBoxModel.Instance.pid,
                        balance = 0,
                        rfu = 0
                    };
                    sBoxPlayerBetsData.Bets[0] = (int)MainModel.Instance.contentMD.totalBet;

                    ERPushMachineDataManager02.Instance.RequestSetBet(sBoxPlayerBetsData, (callbackRes) =>
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
                        spinBtnCtrl.State = "Stop";
                        ChangButtonNo(false);
                    }
                    break;
                case SpinButtonState.Spin:
                    {
                        spinBtnCtrl.State = "Spin";
                      
                        ChangButtonNo(true);
                    }
                    break;
                case SpinButtonState.Auto:
                    {
                        spinBtnCtrl.State = "Auto";
                       
                        ChangButtonNo(true);
                    }
                    break;
            }


        }
        /// <summary>
        /// 游戏状态变更处理：进入 Spin 时清空赢分展示。
        /// </summary>
        protected virtual void OnPropertyGameState(EventData res = null)
        {
            string gameState = (string)res?.value;

            if (gameState == GameState.Spin )
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
            if (res.name == PanelEvent.AnchorPanelChange)
            {
                // 锚点面板切换后重建 UI 绑定
                Init();
            }
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
                NumberAnimation.Instance.AnimateNumber(win,long.Parse(win.text),totalWinCredit + long.Parse(win.text), 0.4f);
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
                singleLine.text = $"Line : {symbolWin.lineNumber}  symbolType : {symbolWin.symbolNumber}   Win :{symbolWin.earnCredit}";
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
                NumberAnimation.Instance.AnimateNumber(gOwnerPanel.GetChild("credit").asTextField, fromCredit, toCredit);
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
        // Spin鎸夐挳
        //public void OnLongClickSpinButton(string customDataOrState) => OnClickSpinButton(true);
        //public void OnShortClickSpinButton(string customDataOrState) => OnClickSpinButton(false);

        public void OnClickSpinButton(bool isLong)
        {

            // 向面板输入事件总线派发 Spin 按钮点击（长按/短按）
            EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT,
               new EventData<bool>(PanelEvent.SpinButtonClick, isLong));

        }

        #region 置灰
        public virtual void ChangButtonNo(bool can)
        {
            if (can)
            {
                //gOwnerPanel.GetChild("ButtonPRIZE").asButton.GetChild("n1").visible = true;
                //gOwnerPanel.GetChild("ButtonPRIZE").asButton.touchable = false;
                btnHelp.GetChild("untouch").visible = true;
                btnHelp.touchable = false;
                btnBetUp.GetChild("untouch").visible = true;
                btnBetUp.touchable = false;
                btnBetDown.GetChild("untouch").visible = true;
                btnBetDown.touchable = false;

                // ChangeBetButtonInteractable(MainModel.Instance.contentMD.betIndex, SBoxModel.Instance.betList.Count);
            }
            else
            {
                //gOwnerPanel.GetChild("ButtonPRIZE").asButton.GetChild("n1").visible = false;
                //gOwnerPanel.GetChild("ButtonPRIZE").asButton.touchable = true;
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
            }

        }
        #endregion

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
            SBoxPlayerBetsData sBoxPlayerBetsData = new SBoxPlayerBetsData()
            {
                PlayerId = SBoxModel.Instance.pid,
                balance = 0,
                rfu = 0
            };

            sBoxPlayerBetsData.Bets[0] = (int)MainModel.Instance.contentMD.totalBet;
            // 设置押注
            ERPushMachineDataManager02.Instance.RequestSetBet(sBoxPlayerBetsData, (res) =>
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
            SBoxPlayerBetsData sBoxPlayerBetsData = new SBoxPlayerBetsData()
            {
                PlayerId = SBoxModel.Instance.pid,
                balance = 0,
                rfu = 0
            };

            sBoxPlayerBetsData.Bets[0] = (int)MainModel.Instance.contentMD.totalBet;
            // 设置押注
            ERPushMachineDataManager02.Instance.RequestSetBet(sBoxPlayerBetsData, (res) =>
            {
                ChangeBetButtonInteractable(betIndex, betList.Count);
            });
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
            MainModel.Instance.contentMD.betmultiple = (int)MainModel.Instance.contentMD.totalBet / MainModel.Instance.lineNum;
            bet.text = MainModel.Instance.contentMD.totalBet.ToString();
            btnBetDown.touchable = curBetIndex > 0;
            btnBetDown.GetChild("untouch").visible = btnBetDown.touchable ? false : true;
            btnBetUp.touchable = curBetIndex < curBetListCount - 1;
            btnBetUp.GetChild("untouch").visible = btnBetUp.touchable ? false : true;
        }

        public virtual void OnLongClickHandler(MachineButtonKey machineButtonKey) { }

        public virtual void OnShortClickHandler(MachineButtonKey machineButtonKey) { }

        public virtual void OnDownClickHandler(MachineButtonKey machineButtonKey)
        {
            switch (machineButtonKey)
            {
                case MachineButtonKey.BtnSpin:
                    {

                    }
                    break;
            }
        }

        public virtual void OnUpClickHandler(MachineButtonKey machineButtonKey)
        {
            switch (machineButtonKey)
            {
                case MachineButtonKey.BtnSpin:
                    {

                    }
                    break;
            }
        }
    }
}
