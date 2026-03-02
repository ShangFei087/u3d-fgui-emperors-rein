using FairyGUI;
using GameMaker;
using PusherEmperorsRein;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace XingYunZhiLun_3998
{
    public class PopupZhuanPan : MachinePageBase
    {
        public new const string pkgName = "XingYunZhiLun_3998";
        public new const string resName = "PopupZhuanPan";

        private GComponent gOwnerPanel, gWheel, gSpinEffectBg, gRawardEffectBg;
        private GameObject goSpinEffect, goSpin, goRawardEffect, goRaward;

        //获取粒子系统
        private Transform effectSpin, effectRaward;

        //开始旋转的按钮
        private GButton spinButton = null;

        //获奖特效绑定的GComponent，部分奖励需要临时改变位置
        private GComponent rewardEffect;

        private MonoHelper mono;
        private Coroutine corGameOnce;

        private bool isClose;

        private EventData _data;
        private bool isInit = false;

        private int targetIndex = 2; // 免费游戏在转盘上的位置（0-19）
        private float segmentAngle = 18f; //     360 / 20 = 18°
        private float rotateSpeed = 15f;
        private float extralyAngle = 0;  //因为转盘分区角度不同，可能需要额外补充一些角度

        protected override void OnInit()
        {
            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            mono = GameObject.Find("Slot Game Main Controller3998").GetComponent<MonoHelper>();
            base.OnInit();

            int count = 2;

            Action callback = () =>
            {
                if(--count <= 0)
                {
                    isInit = true;
                    InitParam(null);
                }
            };

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/PopupZhuanPan/ZhuanPanPoint.prefab",
                (GameObject clone) =>
                {
                    goSpinEffect = clone;
                    callback();
                });

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/PopupZhuanPan/ZhuanPanReward.prefab",
                (GameObject clone) =>
                {
                    goRawardEffect = clone;
                    callback();
                });
        }

        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            //EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnClickSpinButton);
            InitParam(data);
        }

        public override void OnClose(EventData data = null)
        {
            base.OnClose(data);
            //EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnClickSpinButton);
        }

        public void InitParam(EventData data)
        {
            if (data != null) _data = data;

            if (!isInit) return;

            //gOwnerPanel = this.contentPane.GetChild("panel").asCom;
            ////初始化菜单ui
            //ContentModel.Instance.goAnthorPanel = gOwnerPanel;
            //MainModel.Instance.contentMD.goAnthorPanel = gOwnerPanel;
            //// 事件放出
            ////goGameCtrl.transform.Find("Panel").GetComponent<PanelController01>().Init();
            //EventCenter.Instance.EventTrigger<EventData>(PanelEvent.ON_PANEL_EVENT,
            //    new EventData<GComponent>(PanelEvent.AnchorPanelChange, gOwnerPanel));

            GComponent gSpinEffectTip = contentPane.GetChild("zhuanPan").asCom.GetChild("SpinPoint").asCom.GetChild("ZhuanPanPoint").asCom.GetChild("anchorEffect").asCom;
            if(gSpinEffectBg != gSpinEffectTip)
            {
                GameCommon.FguiUtils.DeleteWrapper(gSpinEffectBg);
                gSpinEffectBg = gSpinEffectTip;
                goSpin = GameObject.Instantiate(goSpinEffect);
                effectSpin = goSpin.transform.GetChild(0).GetChild(0);

                GameCommon.FguiUtils.AddWrapper(gSpinEffectBg, goSpin);
            }

            GComponent gRawardEffectTip = contentPane.GetChild("zhuanPan").asCom.GetChild("anchorEffect").asCom;
            if(gRawardEffectBg != gRawardEffectTip)
            {
                GameCommon.FguiUtils.DeleteWrapper(gRawardEffectBg);
                gRawardEffectBg = gRawardEffectTip;
                goRaward = GameObject.Instantiate(goRawardEffect);
                effectRaward = goRaward.transform.GetChild(0).GetChild(0).GetChild(0);
                GameCommon.FguiUtils.AddWrapper(gRawardEffectBg, goRaward);
            }

            rewardEffect = contentPane.GetChild("zhuanPan").asCom.GetChild("anchorEffect").asCom;
            rewardEffect.position = new Vector3(713, 459, 0);

            goRaward.transform.GetChild(0).localScale = new Vector3(1.5f, 1.2f);

            spinButton = contentPane.GetChild("spinBtn").asButton;
            spinButton.onClick.Clear();
            spinButton.onClick.Add(() => StartGameOnce(() =>
            {
                ContentModel.Instance.wheelIsSpin = false;
                ContentModel.Instance.wheelBtnSpinState = SpinButtonState.Stop;
                isClose = false;

                //ContentModel.Instance.gameState = GameState.Idle;
                CloseSelf(null);
                //DebugUtils.Log("游戏结束");
            }));

            //转动时在播放
            StopEffectAnim(effectSpin);

            //确定获得奖励后再播放特效
            StopEffectAnim(effectRaward);

            mono.updateHandle.AddListener(WheelTrun);

            gWheel = this.contentPane.GetChild("zhuanPan").asCom.GetChild("Wheel").asCom;
            WheelInit(new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 });

            if (ContentModel.Instance.isAuto)
            {
                Timers.inst.Add(1.5f, 1, (object obj) =>
                {
                    ContentModel.Instance.wheelIsSpin = true;

                    Action successCallback = () =>
                    {
                        ContentModel.Instance.wheelIsSpin = false;
                        ContentModel.Instance.wheelBtnSpinState = SpinButtonState.Stop;

                        //ContentModel.Instance.gameState = GameState.Idle;
                        CloseSelf(null);
                        //DebugUtils.Log("游戏结束");
                    };

                    ContentModel.Instance.wheelBtnSpinState = SpinButtonState.Hui;
                    StartGameOnce(successCallback, StopGameWhenError); //开始玩
                });
            }
        }


        void StartGameOnce(Action successCallback = null, Action<string> errorCallback = null)
        {
            if (isClose) return;
            isClose = true;

            ContentModel.Instance.totalPlaySpins = 1;
            ContentModel.Instance.remainPlaySpins = 1;

            if(_data != null)
            {
                Dictionary<string, object> a = _data.value as Dictionary<string, object>;
                SetTargetIndex(a["jackpotType"]);
            }

            corGameOnce = mono.StartCoroutine(GameOnce(successCallback, errorCallback));
        }

        IEnumerator GameOnce(Action successCallback, Action<string> errorCallback)
        {
            mono.updateHandle.RemoveListener(WheelTrun);

            bool isNext = false;

            //播放转动特效
            PlayEffectAnim(effectSpin);

            mono.StartCoroutine(SpinWheelToTarget(targetIndex, () =>
            {
                isNext = true;
            }, errorCallback));

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            StopEffectAnim(effectSpin);

            yield return new WaitForSeconds(0.5f);

            PlayEffectAnim(effectRaward);
            yield return new WaitForSeconds(2f);

            if (successCallback != null)
                successCallback.Invoke();
        }

        private void OnClickSpinButton(EventData res)
        {
            switch (ContentModel.Instance.wheelBtnSpinState)
            {
                case SpinButtonState.Stop:
                    if (ContentModel.Instance.wheelIsSpin) return; //已经开始玩直接退出？
                    ContentModel.Instance.wheelIsSpin = true;

                    Action successCallback = () =>
                    {
                        ContentModel.Instance.wheelIsSpin = false;
                        ContentModel.Instance.wheelBtnSpinState = SpinButtonState.Stop;
                        isClose = false;

                        //ContentModel.Instance.gameState = GameState.Idle;
                        CloseSelf(null);
                        //DebugUtils.Log("游戏结束");
                    };

                    ContentModel.Instance.wheelBtnSpinState = SpinButtonState.Hui;
                    StartGameOnce(successCallback, StopGameWhenError); //开始玩
                    break;
                case SpinButtonState.Hui:
                    {
                        // 已经在游戏时，去停止游戏
                        if (!ContentModel.Instance.wheelIsSpin) return; // 已经停止直接退出

                        //slotMachineCtrl.isStopImmediately = true; // 去停止游戏  

                        //SlotGameEffectManager.Instance.SetEffect(SlotGameEffect.StopImmediately);
                    }
                    break;
            }
        }

        private void SetTargetIndex(object str)
        {
            string index = str.ToString();
            switch (index)
            {
                case "FreeGame":
                    targetIndex = 2;
                    break;
                case "mini":
                    targetIndex = 0;
                    break;
                case "minor":
                    targetIndex = 14;
                    break;
                case "major":
                    targetIndex = 0;
                    break;
                case "grand":
                    targetIndex = 14;
                    break;
                case "Lihe":
                    targetIndex = 1;
                    break;
                case "Wild":
                    targetIndex = 3;
                    break;
                case "Multiple":
                    if(ContentModel.Instance.multiple <= 11)
                    {
                        targetIndex = ContentModel.Instance.multiple + 1;
                    }
                    else
                    {
                        targetIndex = 13;
                    }
                    break;
            }

            switch (targetIndex)
            {
                case 0:
                    extralyAngle = -0.65f;
                    goRaward.transform.GetChild(0).localScale = new Vector3(1.5f, 1.3f);
                    break;
                case 1:
                    extralyAngle = -1.55f;
                    rewardEffect.position = new Vector3(713, 458, 0);
                    break;
                case 2:
                    extralyAngle = -0.4f;
                    break;
                case 3:
                    extralyAngle = -1.3f;
                    rewardEffect.position = new Vector3(713, 452, 0);
                    break;
                case 4:
                    extralyAngle = 0;
                    rewardEffect.position = new Vector3(713, 456, 0);
                    break;
                case 5:
                    extralyAngle = -1.2f;
                    rewardEffect.position = new Vector3(713, 451, 0);
                    goRaward.transform.GetChild(0).localScale = new Vector3(1.5f, 1.3f);
                    break;
                case 6:
                    extralyAngle = -1.4f;
                    rewardEffect.position = new Vector3(713, 451, 0);
                    goRaward.transform.GetChild(0).localScale = new Vector3(1.5f, 1.27f);
                    break;
                case 7:
                    extralyAngle = -0.2f;
                    rewardEffect.position = new Vector3(719, 452, 0);
                    goRaward.transform.GetChild(0).localScale = new Vector3(1.5f, 1.1f);
                    break;
                case 8:
                    extralyAngle = 0.8f;
                    rewardEffect.position = new Vector3(719, 450.5f, 0);
                    goRaward.transform.GetChild(0).localScale = new Vector3(1.5f, 1.25f);
                    break;
                case 9:
                    extralyAngle = 0.5f;
                    rewardEffect.position = new Vector3(719, 447, 0);
                    break;
                case 10:
                    extralyAngle = 0.1f;
                    rewardEffect.position = new Vector3(713, 449, 0);
                    goRaward.transform.GetChild(0).localScale = new Vector3(1.5f, 1.3f);
                    break;
                case 11:
                    rewardEffect.position = new Vector3(713, 451, 0);
                    break;
                case 12:
                    rewardEffect.position = new Vector3(713, 451, 0);
                    break;
                case 13:
                    rewardEffect.position = new Vector3(713, 452, 0);
                    break;
                case 14:
                    extralyAngle = 1.7f;
                    break;
                default:
                    extralyAngle = 0;
                    break;
            }
        }


        private void StopGameWhenError(string msg)
        {
            ContentModel.Instance.isSpin = false;
            ContentModel.Instance.isAuto = false;
            ContentModel.Instance.wheelBtnSpinState = SpinButtonState.Stop;

            //ContentModel.Instance.gameState = GameState.Idle;
            //string massage = I18nMgr.T(msg);
            //TipPopupHandler.Instance.OpenPopupOnce(I18nMgr.T(msg));

        }


        //轮盘初始化图片
        private void WheelInit(List<int> wheelSymbolsIndex)
        {
            GComponent symbols = gWheel.GetChild("Symbols").asCom;
            for (int i = 0; i < symbols.numChildren; i++)
            {
                // 使用 GetChildAt 按索引获取，不需要知道具体名称
                GObject child = symbols.GetChildAt(i);

                if (child.asCom != null) // 确保是 GComponent
                {
                    GComponent symbol = child.asCom;
                    // 在这里处理每个 symbol
                    GLoader gLoader = symbol.GetChild("animator").asCom.GetChild("icon").asLoader;
                    gLoader.url = CustomModel.Instance.wheelSymbolIcon[(i % 14).ToString()];
                }
            }
        }

        //转盘转动控制
        private void WheelTrun()
        {
            gWheel.rotation += rotateSpeed * Time.deltaTime;
            if (gWheel.rotation >= 360)
            {
                gWheel.rotation = 0;
            }
        }




        private IEnumerator SpinWheelToTarget(int targetIndex,Action successCallback, Action<string> errorCallback)
        {
            // 当前角度
            float currentAngle = NormalizeAngle(gWheel.rotation);

            // 目标角度（免费游戏分区的中心位置）
            float targetAngleCenter = 360 - (targetIndex * segmentAngle - (segmentAngle / 2f));

            // 总旋转角度 = 至少2圈 + 随机额外圈数
            int minCircles = 2;
            int extraCircles = UnityEngine.Random.Range(1, 3); // 总共3-4圈
            int totalCircles = minCircles + extraCircles;

            float totalRotation = totalCircles * 360f + (targetAngleCenter - currentAngle);
            if (totalRotation < 0) totalRotation += 360f;

            // 旋转参数
            float speed = 100f;      // 起始速度
            float maxSpeed = 1280f;   // 最大速度
            float accelerateTime = 1f;  // 加速到最大速度的时间
            float decelerateTime = 2f;  // 减速到0的时间

            float elapsed = 0f;
            float rotated = 0f;

            // 阶段1：加速
            while (elapsed < accelerateTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / accelerateTime;

                // 缓动加速
                speed = Mathf.Lerp(100f, maxSpeed, t * t);

                float deltaRot = speed * Time.deltaTime;
                gWheel.rotation += deltaRot;
                rotated += deltaRot;

                yield return null;
            }

            speed = maxSpeed;

            // 阶段2：计算匀速阶段需要的时间
            // 总角度 = 加速阶段角度 + 匀速阶段角度 + 减速阶段角度
            float accelerateDistance = 0.5f * (100f + maxSpeed) * accelerateTime;
            float decelerateDistance = 0.5f * maxSpeed * decelerateTime;
            float constantDistance = totalRotation - accelerateDistance - decelerateDistance;
            float constantTime = constantDistance / maxSpeed;

            elapsed = 0f;
            while (elapsed < constantTime)
            {
                elapsed += Time.deltaTime;

                float deltaRot = speed * Time.deltaTime;
                gWheel.rotation += deltaRot;
                rotated += deltaRot;

                yield return null;
            }

            // 阶段3：减速（关键修改）
            elapsed = 0f;
            float remainingRotation = totalRotation - rotated;
            float startDecelerateSpeed = speed;

            // 计算需要的减速度，确保正好停在目标位置
            float deceleration = (startDecelerateSpeed * startDecelerateSpeed) / (2 * remainingRotation);

            // 匀减速到停
            while (speed > 1f && remainingRotation > 0.1f)
            {
                elapsed += Time.deltaTime;

                // 匀减速
                speed -= deceleration * Time.deltaTime;
                if (speed < 0) speed = 0;

                // 计算这一帧转动的角度
                float deltaRot = speed * Time.deltaTime;

                // 确保不会转过头
                if (remainingRotation < deltaRot)
                {
                    deltaRot = remainingRotation;
                    speed = 0;
                }

                gWheel.rotation += deltaRot;
                rotated += deltaRot;
                remainingRotation -= deltaRot;

                yield return null;
            }

            // 微小的最终调整（如果有需要）
            float finalAngle = NormalizeAngle(gWheel.rotation);
            float angleDiff = targetAngleCenter - finalAngle;
            if (Mathf.Abs(angleDiff) > 0.5f)
            {
                // 非常缓慢地调整最后一点角度
                gWheel.rotation += angleDiff * 0.3f + 0.8f + extralyAngle;
                yield return new WaitForSeconds(0.2f);
            }

            // 完成
            successCallback?.Invoke();

        }

        // 辅助函数：规范化角度到0-360
        private float NormalizeAngle(float angle)
        {
            angle %= 360f;
            if (angle < 0) angle += 360f;
            return angle;
        }


        //播放某一个特定的特效
        private void PlayEffectAnim(Transform effect)
        {
            ParticleSystem particle = effect.GetComponent<ParticleSystem>();
            particle.Play();

            // 递归播放所有子物体的粒子系统
            foreach (Transform child in effect)
            {
                PlayEffectAnim(child);
            }
        }

        //停止某一个特定的特效
        private void StopEffectAnim(Transform effect)
        {
            ParticleSystem particle = effect.GetComponent<ParticleSystem>();
            particle.Stop();

            // 递归播放所有子物体的粒子系统
            foreach (Transform child in effect)
            {
                StopEffectAnim(child);
            }
        }
    }

    
}