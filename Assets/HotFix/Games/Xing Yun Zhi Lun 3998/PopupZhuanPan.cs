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

        private GComponent gOwnerPanel, gWheel, gSpinEffectBg, gRawardEffectBg, gWheelBg;
        private GameObject goSpinEffect, goSpin, goRawardEffect, goRaward, wheelBgPref, wheelBgObj;
        private Animator animator;

        //获取粒子系统
        //private Transform effectSpin, effectRaward;

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
        private float extralyAngle = 9f;  //因为转盘分区角度不同，可能需要额外补充一些角度

        private readonly string[] animNames = { "01_idle" , "02_idle", "03_idle" };

        protected override void OnInit()
        {
            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            int count = 3;

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

            ResourceManager02.Instance.LoadAsset<GameObject>(
                "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/PopupZhuanPan/ZhuanPanGame.prefab",
                (GameObject clone) =>
                {
                    wheelBgPref = clone;
                    callback();
                });

            machineBtnClickHelper = new MachineButtonClickHelper()
            {
                shortClickHandler = new Dictionary<MachineButtonKey, Action<MachineButtonInfo>>()
                {
                    [MachineButtonKey.BtnSpin] = (info) =>
                    {
                        if (PanelBaseController.ShouldBlockPhysicalSpinInput) return;

                        Debug.LogError("游戏接受到机台短按的数据：Spin");
                        EventData<bool> res = new EventData<bool>(PanelEvent.SpinButtonClick, false); // isLongClick
                        StartGameOnce(() =>
                        {
                            ContentModel.Instance.wheelIsSpin = false;
                            ContentModel.Instance.wheelBtnSpinState = SpinButtonState.Stop;
                            isClose = false;

                            //ContentModel.Instance.gameState = GameState.Idle;
                            CloseSelf(null);
                            //DebugUtils.Log("游戏结束");
                        });
                    },
                },
            };
        }

        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            InitParam(data);
            //EventCenter.Instance.AddEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnClickSpinButton);
            mono = GameObject.Find("Slot Game Main Controller3998").GetComponent<MonoHelper>();
            mono.updateHandle.AddListener(WheelTrun);

            ChooseWheelSkin();
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

            //GComponent gSpinEffectTip = contentPane.GetChild("zhuanPan").asCom.GetChild("SpinPoint").asCom.GetChild("ZhuanPanPoint").asCom.GetChild("anchorEffect").asCom;
            //if(gSpinEffectBg != gSpinEffectTip)
            //{
            //    GameCommon.FguiUtils.DeleteWrapper(gSpinEffectBg);
            //    gSpinEffectBg = gSpinEffectTip;
            //    goSpin = GameObject.Instantiate(goSpinEffect);
            //    effectSpin = goSpin.transform.GetChild(0).GetChild(0);

            //    GameCommon.FguiUtils.AddWrapper(gSpinEffectBg, goSpin);
            //}

            //GComponent gRawardEffectTip = contentPane.GetChild("zhuanPan").asCom.GetChild("anchorEffect").asCom;
            //if(gRawardEffectBg != gRawardEffectTip)
            //{
            //    GameCommon.FguiUtils.DeleteWrapper(gRawardEffectBg);
            //    gRawardEffectBg = gRawardEffectTip;
            //    goRaward = GameObject.Instantiate(goRawardEffect);
            //    effectRaward = goRaward.transform.GetChild(0).GetChild(0).GetChild(0);
            //    GameCommon.FguiUtils.AddWrapper(gRawardEffectBg, goRaward);
            //}

            //rewardEffect = contentPane.GetChild("zhuanPan").asCom.GetChild("anchorEffect").asCom;
            //rewardEffect.position = new Vector3(713, 459, 0);

            //goRaward.transform.GetChild(0).localScale = new Vector3(1.5f, 1.2f);


            gWheel = this.contentPane.GetChild("zhuanPan").asCom;
            WheelInit(new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 });

            GComponent loadAnchorZhuanPanBg = contentPane.GetChild("anchorBg").asCom;
            if(gWheelBg != loadAnchorZhuanPanBg)
            {
                GameCommon.FguiUtils.DeleteWrapper(gWheelBg);
                gWheelBg = loadAnchorZhuanPanBg;
                wheelBgObj = GameObject.Instantiate(wheelBgPref);
                animator = wheelBgObj.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                GameCommon.FguiUtils.AddWrapper(gWheelBg, wheelBgObj);

                ChangeParent(gWheel, wheelBgObj, "Anchor/Spine Mecanim GameObject (ng_img_turntable)/SkeletonUtility-SkeletonRoot/root/c_circle");
            }

            spinButton = contentPane.GetChild("spinBtn").asButton;
            isClose = false;
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
            //StopEffectAnim(effectSpin);

            //确定获得奖励后再播放特效
            //StopEffectAnim(effectRaward);


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
            //PlayEffectAnim(effectSpin);

            mono.StartCoroutine(SpinWheelToTarget(targetIndex, () =>
            {
                isNext = true;
            }, errorCallback));

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            //StopEffectAnim(effectSpin);

            yield return new WaitForSeconds(0.5f);

            //PlayEffectAnim(effectRaward);
            //yield return new WaitForSeconds(2f);

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
                    targetIndex = UnityEngine.Random.Range(0, 1) > 0.5f ? 1 : 7;
                    break;
                case "mini":
                    targetIndex = targetIndex = UnityEngine.Random.Range(0, 1) > 0.5f ? 5 : 19 ;
                    break;
                case "minor":
                    targetIndex = 19;
                    break;
                case "major":
                    targetIndex = 5;
                    break;
                case "Lihe":
                    targetIndex = UnityEngine.Random.Range(0, 1) > 0.5f ? 0 : 6;
                    break;
                case "Wild":
                    targetIndex = UnityEngine.Random.Range(0, 1) > 0.5f ? 2 : 8;
                    break;
                case "Multiple":
                    if(ContentModel.Instance.multiple <= 11)
                    {
                        targetIndex = ContentModel.Instance.multiple + 6;
                    }
                    else
                    {
                        targetIndex = 18;
                    }
                    break;
            }

            //switch (targetIndex)
            //{
            //    case 0:
            //        extralyAngle = -0.65f;
            //        goRaward.transform.GetChild(0).localScale = new Vector3(1.5f, 1.3f);
            //        break;
            //    case 1:
            //        extralyAngle = -1.55f;
            //        rewardEffect.position = new Vector3(713, 458, 0);
            //        break;
            //    case 2:
            //        extralyAngle = -0.4f;
            //        break;
            //    case 3:
            //        extralyAngle = -1.3f;
            //        rewardEffect.position = new Vector3(713, 452, 0);
            //        break;
            //    case 4:
            //        extralyAngle = 0;
            //        rewardEffect.position = new Vector3(713, 456, 0);
            //        break;
            //    case 5:
            //        extralyAngle = -1.2f;
            //        rewardEffect.position = new Vector3(713, 451, 0);
            //        goRaward.transform.GetChild(0).localScale = new Vector3(1.5f, 1.3f);
            //        break;
            //    case 6:
            //        extralyAngle = -1.4f;
            //        rewardEffect.position = new Vector3(713, 451, 0);
            //        goRaward.transform.GetChild(0).localScale = new Vector3(1.5f, 1.27f);
            //        break;
            //    case 7:
            //        extralyAngle = -0.2f;
            //        rewardEffect.position = new Vector3(719, 452, 0);
            //        goRaward.transform.GetChild(0).localScale = new Vector3(1.5f, 1.1f);
            //        break;
            //    case 8:
            //        extralyAngle = 0.8f;
            //        rewardEffect.position = new Vector3(719, 450.5f, 0);
            //        goRaward.transform.GetChild(0).localScale = new Vector3(1.5f, 1.25f);
            //        break;
            //    case 9:
            //        extralyAngle = 0.5f;
            //        rewardEffect.position = new Vector3(719, 447, 0);
            //        break;
            //    case 10:
            //        extralyAngle = 0.1f;
            //        rewardEffect.position = new Vector3(713, 449, 0);
            //        goRaward.transform.GetChild(0).localScale = new Vector3(1.5f, 1.3f);
            //        break;
            //    case 11:
            //        rewardEffect.position = new Vector3(713, 451, 0);
            //        break;
            //    case 12:
            //        rewardEffect.position = new Vector3(713, 451, 0);
            //        break;
            //    case 13:
            //        rewardEffect.position = new Vector3(713, 452, 0);
            //        break;
            //    case 14:
            //        extralyAngle = 1.7f;
            //        break;
            //    default:
            //        extralyAngle = 0;
            //        break;
            //}
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
            GComponent symbols = gWheel.GetChild("Wheel").asCom.GetChild("wheelBg").asLoader.component.GetChild("Symbols").asCom;
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




        //轮盘旋转方法
        private IEnumerator SpinWheelToTarget(int targetIndex, Action successCallback, Action<string> errorCallback = null)
        {
            float currentAngle = NormalizeAngle(gWheel.rotation);
            float targetAngleCenter = 360 - (targetIndex * segmentAngle);

            int minCircles = 2;
            int extraCircles = UnityEngine.Random.Range(3, 7);
            int totalCircles = minCircles + extraCircles;

            // ========================
            // 重点：彻底删掉 +10，纯公式
            // ========================
            float totalRotation = totalCircles * 360f + (targetAngleCenter - currentAngle);
            if (totalRotation < 0) totalRotation += 360f;

            float speed = 100f;
            float maxSpeed = 1280f;
            float accelerateTime = 1f;
            float decelerateTime = 2f;

            float elapsed = 0f;
            float rotated = 0f;

            // 阶段1：加速（原样）
            while (elapsed < accelerateTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / accelerateTime;
                speed = Mathf.Lerp(100f, maxSpeed, t * t);

                float deltaRot = speed * Time.deltaTime;
                gWheel.rotation += deltaRot;
                rotated += deltaRot;

                yield return null;
            }

            speed = maxSpeed;

            // 阶段2：匀速（原样）
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

            // ================================
            // 阶段3：匀减速 → 但最后自动对齐
            // ================================
            float remainingRotation = totalRotation - rotated;
            float startSpeed = speed;
            float deceleration = (startSpeed * startSpeed) / (2 * remainingRotation);

            // 先减速到速度很低，但不追求完全走完
            while (speed > 200f)
            {
                speed -= deceleration * Time.deltaTime;
                float deltaRot = speed * Time.deltaTime;

                gWheel.rotation += deltaRot;
                remainingRotation -= deltaRot;

                yield return null;
            }

            // ================================
            // 关键：剩下角度平滑滑过去（跨设备稳定）
            // ================================
            float slideTime = 0.8f;
            float slideElapsed = 0f;
            float startRot = gWheel.rotation;
            float targetRot = startRot + remainingRotation + extralyAngle;

            while (slideElapsed < slideTime)
            {
                slideElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(slideElapsed / slideTime);

                // 关键：这个曲线就是真实转盘“越转越慢”的效果
                t = 1 - Mathf.Pow(1 - t, 3); // 缓动曲线：Out Cubic（最强物理感）

                gWheel.rotation = Mathf.Lerp(startRot, targetRot, t);
                yield return null;
            }

            gWheel.rotation = targetRot;

            successCallback?.Invoke();
        }

        // 辅助函数：规范化角度到0-360
        private float NormalizeAngle(float angle)
        {
            angle %= 360f;
            if (angle < 0) angle += 360f;
            return angle;
        }

        public Vector2 TransFormParentNode(GComponent node, GComponent targetNode)
        {
            Vector2 worldPos = node.LocalToGlobal(Vector2.zero);
            return targetNode.GlobalToLocal(worldPos);
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

        private void ChangeParent(GObject gComponent, GameObject go, string path)
        {
            Transform num01 = go.transform.Find(path);
            if (gComponent.displayObject?.gameObject != null)
            {
                Transform t = gComponent.displayObject.gameObject.transform;
                t.SetParent(num01, false);
                t.localPosition = new Vector3(-276f, -276.1f, 0);
                t.localScale = new Vector3(0.01f, 0.01f, 1);
            }
        }

        private void PlayAnim(string animName)
        {
            animator.Rebind();
            animator.Play(animName);
            animator.Update(0f);
        }

        private void ChooseWheelSkin()
        {
            string wheelIndex = ContentModel.Instance.scatterCount > 3 ? "mid" : "low";
            int wheelBgIndex = ContentModel.Instance.scatterCount - 3;
            gWheel.GetChild("Wheel").asCom.GetChild("wheelBg").asLoader.url = CustomModel.Instance.wheelState[wheelIndex];
            PlayAnim(animNames[wheelBgIndex]);

            WheelInit(new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 });
        }
    }

    
}