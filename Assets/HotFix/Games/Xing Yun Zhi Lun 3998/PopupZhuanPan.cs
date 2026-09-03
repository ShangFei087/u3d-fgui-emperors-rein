using FairyGUI;
using GameMaker;
using PusherEmperorsRein;
using SlotMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
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
        private GLoader gWheelLoad;

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
        private string jackpotType = String.Empty;

        private float segmentAngle = 18f; //     360 / 20 = 18°
        private float rotateSpeed = 15f;
        private float extralyAngle = 9f;  //因为转盘分区角度不同，可能需要额外补充一些角度

        private int wheelIndex;
        private TimerCallback _closeTimer, _canSpinTimer;

        private readonly string[] animStartNames = { "01_start", "02_start", "03_start" };
        private readonly string[] animEndNames = { "01_end", "02_end", "03_end" };

        private Transform[] idels, wins, stages;

        private Action callback = null;
        private bool canSpin = false;

        protected override void OnInit()
        {
            this.contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            int count = 1;

            Action callback = () =>
            {
                if (--count <= 0)
                {
                    isInit = true;
                    InitParam(null);
                }
            };

            //ResourceManager02.Instance.LoadAsset<GameObject>(
            //    "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/PopupZhuanPan/ZhuanPanPoint.prefab",
            //    (GameObject clone) =>
            //    {
            //        goSpinEffect = clone;
            //        callback();
            //    });

            //ResourceManager02.Instance.LoadAsset<GameObject>(
            //    "Assets/GameRes/Games/Xing Yun Zhi Lun 3998/Prefabs/PopupZhuanPan/ZhuanPanReward.prefab",
            //    (GameObject clone) =>
            //    {
            //        goRawardEffect = clone;
            //        callback();
            //    });

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

                            CloseSelf(null);
                        });
                    },
                },
            };
        }

        public override void OnOpen(PageName name, EventData data)
        {
            base.OnOpen(name, data);
            InitParam(data);
            mono = GameObject.Find("Slot Game Main Controller3998").GetComponent<MonoHelper>();
            mono.updateHandle.AddListener(WheelTrun);

            ChooseWheelSkin();

            _canSpinTimer = (object obj) =>
            {
                canSpin = true;
            };
            Timers.inst.Add(1, 1, _canSpinTimer);
        }

        public override void OnClose(EventData data = null)
        {
            base.OnClose(data);

            isClose = false;
            wins[wheelIndex].gameObject.SetActive(false);
            //EventCenter.Instance.RemoveEventListener<EventData>(PanelEvent.ON_PANEL_INPUT_EVENT, OnClickSpinButton);
        }

        public void InitParam(EventData data)
        {
            if (data != null) _data = data;

            if (!isInit) return;
            canSpin = false;

            gWheel = this.contentPane.GetChild("zhuanPan").asCom;
            WheelInit(CustomModel.Instance.lowWheelIndex);

            GComponent loadAnchorZhuanPanBg = contentPane.GetChild("anchorBg").asCom;
            if (gWheelBg != loadAnchorZhuanPanBg)
            {
                GameCommon.FguiUtils.DeleteWrapper(gWheelBg);
                gWheelBg = loadAnchorZhuanPanBg;
                gWheelLoad = gWheel.GetChild("Wheel").asCom.GetChild("wheelBg").asLoader;
                wheelBgObj = GameObject.Instantiate(wheelBgPref);
                animator = wheelBgObj.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                GameCommon.FguiUtils.AddWrapper(gWheelBg, wheelBgObj);

                idels = new Transform[3];
                wins = new Transform[3];
                stages = new Transform[3];

                idels[0] = wheelBgObj.transform.Find("Anchor/Spine Mecanim GameObject (Lucky_ng_img_turntable)/SkeletonUtility-SkeletonRoot/root/01/1/1").GetChild(0);
                stages[0] = wheelBgObj.transform.Find("Anchor/Spine Mecanim GameObject (Lucky_ng_img_turntable)/SkeletonUtility-SkeletonRoot/root/01/1/1").GetChild(1);
                wins[0] = wheelBgObj.transform.Find("Anchor/Spine Mecanim GameObject (Lucky_ng_img_turntable)/SkeletonUtility-SkeletonRoot/root/01/1/1").GetChild(2);
                wins[0].gameObject.SetActive(false);

                idels[1] = wheelBgObj.transform.Find("Anchor/Spine Mecanim GameObject (Lucky_ng_img_turntable)/SkeletonUtility-SkeletonRoot/root/02/2").GetChild(0);
                stages[1] = wheelBgObj.transform.Find("Anchor/Spine Mecanim GameObject (Lucky_ng_img_turntable)/SkeletonUtility-SkeletonRoot/root/02/2").GetChild(1);
                wins[1] = wheelBgObj.transform.Find("Anchor/Spine Mecanim GameObject (Lucky_ng_img_turntable)/SkeletonUtility-SkeletonRoot/root/02/2").GetChild(2);
                wins[1].gameObject.SetActive(false);

                idels[2] = wheelBgObj.transform.Find("Anchor/Spine Mecanim GameObject (Lucky_ng_img_turntable)/SkeletonUtility-SkeletonRoot/root/03/3").GetChild(0);
                stages[2] = wheelBgObj.transform.Find("Anchor/Spine Mecanim GameObject (Lucky_ng_img_turntable)/SkeletonUtility-SkeletonRoot/root/03/3").GetChild(1);
                wins[2] = wheelBgObj.transform.Find("Anchor/Spine Mecanim GameObject (Lucky_ng_img_turntable)/SkeletonUtility-SkeletonRoot/root/03/3").GetChild(2);
                wins[2].gameObject.SetActive(false);

                ChangeParent(gWheel, wheelBgObj, "Anchor/Spine Mecanim GameObject (Lucky_ng_img_turntable)/SkeletonUtility-SkeletonRoot/root/tx01");
            }


            spinButton = contentPane.GetChild("spinBtn").asButton;
            spinButton.onClick.Clear();
            spinButton.onClick.Add(() => StartGameOnce(() =>
            {
                ContentModel.Instance.wheelIsSpin = false;
                ContentModel.Instance.wheelBtnSpinState = SpinButtonState.Stop;

                //ContentModel.Instance.gameState = GameState.Idle;
                CloseSelf(null);
                //DebugUtils.Log("游戏结束");
            }));
            spinButton.visible = false;

            //转动时在播放
            //StopEffectAnim(effectSpin);

            //确定获得奖励后再播放特效
            //StopEffectAnim(effectRaward);

            if (isOpen && ContentModel.Instance.isAuto)
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

            preLoadedCallback?.Invoke();
        }


        void StartGameOnce(Action successCallback = null, Action<string> errorCallback = null)
        {
            if (isClose || !canSpin) return;
            isClose = true; 
            spinButton.visible = false;
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT, new EventData(Game3998AudioEvent.WheelButton));

            ContentModel.Instance.totalPlaySpins = 1;
            ContentModel.Instance.remainPlaySpins = 1;
            jackpotType = String.Empty;

            if (_data != null)
            {
                Dictionary<string, object> a = _data.value as Dictionary<string, object>;
                jackpotType = a["jackpotType"] as String;
                SetTargetIndex(a["jackpotType"]);

                if (a.ContainsKey("callback"))
                {
                    callback = (Action)a["callback"];
                }
            }

            corGameOnce = mono.StartCoroutine(GameOnce(successCallback, errorCallback));
        }

        IEnumerator GameOnce(Action successCallback, Action<string> errorCallback)
        {
            mono.updateHandle.RemoveListener(WheelTrun);
            StopEffectAnim(idels[wheelIndex]);
            StopEffectAnim(stages[wheelIndex]);

            bool isNext = false;

            //播放转动特效
            //PlayEffectAnim(effectSpin);

            mono.StartCoroutine(SpinWheelToTarget(targetIndex + 5, () =>
            {
                isNext = true;
            }, errorCallback));

            yield return new WaitUntil(() => isNext == true);
            isNext = false;

            //StopEffectAnim(effectSpin);

            callback?.Invoke();

            yield return new WaitForSeconds(0.5f);

            StopEffectAnim(wins[wheelIndex]); 
            wins[wheelIndex].gameObject.SetActive(false);
            PlayAnim(animEndNames[ContentModel.Instance.scatterCount - 3]);
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT, new EventData(Game3998AudioEvent.WheelBGMEnding));

            yield return new WaitForSeconds(1f);

            //PlayEffectAnim(effectRaward);
            //yield return new WaitForSeconds(2f);

            if (successCallback != null)
                successCallback.Invoke();
        }


        private void SetTargetIndex(object str)
        {
            string index = str.ToString();
            switch (index)
            {
                case "FreeGame":
                    targetIndex = UnityEngine.Random.Range(0, 2) * 8 + 5;
                    break;
                case "mini":
                    targetIndex = UnityEngine.Random.Range(0, 3) * 8 + 1;
                    break;
                case "minor":
                    targetIndex = UnityEngine.Random.Range(0, 2) * 8 + 1;
                    break;
                case "major":
                    targetIndex = UnityEngine.Random.Range(0, 3) * 8 + 1;
                    break;
                case "Lihe":
                    targetIndex = UnityEngine.Random.Range(0, 2) * 8 + 3;
                    break;
                case "Wild":
                    targetIndex = UnityEngine.Random.Range(0, 2) * 8 + 7;
                    break;
                case "Multiple":
                    //根据免费游戏的图标来确定是什么轮盘判断倍率位置
                    switch (ContentModel.Instance.multiple - (140 + (ContentModel.Instance.scatterCount - 3) * 20))
                    {
                        case 0:
                            targetIndex = UnityEngine.Random.Range(0, 2) * 8 + 0;
                            break;
                        case 20:
                            targetIndex = UnityEngine.Random.Range(0, 2) * 8 + 2;
                            break;
                        case 40:
                            targetIndex = UnityEngine.Random.Range(0, 2) * 8 + 4;
                            break;
                        case 60:
                            targetIndex = UnityEngine.Random.Range(0, 2) * 8 + 6;
                            break;
                    }
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
        private void WheelInit(int[] wheelSymbolsIndex)
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
                    gLoader.url = CustomModel.Instance.wheelSymbolIcon[wheelSymbolsIndex[i % 8].ToString()];
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
            float remainingRotation = Math.Abs(totalRotation - rotated);
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

            wins[wheelIndex].gameObject.SetActive(true);
            wins[wheelIndex].GetChild(wins[wheelIndex].childCount - 1).GetComponent<Canvas>().sortingOrder = wins[wheelIndex].GetChild(wins[wheelIndex].childCount - 2).GetComponent<ParticleSystem>().GetComponent<Renderer>().sortingOrder - 2;
            PlayEffectAnim(wins[wheelIndex]);
            PlayEFX();

            yield return new WaitForSeconds(1f);

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
            if(particle != null) particle.Play();

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
            if(particle != null) particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

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
                t.localPosition = new Vector3(-275.98f, -274.33f, 0);
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
            string wheelIndexStr = ContentModel.Instance.scatterCount > 3 ? "mid" : "low";
            int wheelBgIndex = ContentModel.Instance.scatterCount - 3;
            gWheel.GetChild("Wheel").asCom.GetChild("wheelBg").asLoader.url = CustomModel.Instance.wheelState[wheelIndexStr];
            PlayAnim(animStartNames[wheelBgIndex]);
            wheelIndex = wheelBgIndex;
            EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT, new EventData(Game3998AudioEvent.WheelRaiseUp));

            ChangeWheelURL(wheelBgIndex);

            switch (wheelBgIndex)
            {
                case 0:
                    WheelInit(CustomModel.Instance.lowWheelIndex);
                    break;
                case 1:
                    WheelInit(CustomModel.Instance.midWheelIndex);
                    break;
                case 2:
                    WheelInit(CustomModel.Instance.highWheelIndex);
                    break;
            }

            _closeTimer = (object obj) =>
            {
                EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT, new EventData(Game3998AudioEvent.WheelBgm));
                spinButton.visible = true;
                PlayEffectAnim(idels[wheelBgIndex]);
                PlayEffectAnim(stages[wheelBgIndex]);
            };
            Timers.inst.Add(1, 1, _closeTimer);
        }

        private void PlayEFX()
        {
            if(jackpotType == "Wild")
            {
                EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT, new EventData(Game3998AudioEvent.WildExtend));
            }
            else if (jackpotType == "mini" || jackpotType == "minor" || jackpotType == "major")
            {
                EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT, new EventData(Game3998AudioEvent.BonusWin));
            }
            else if(jackpotType == "FreeGame" )
            {
                EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT, new EventData(Game3998AudioEvent.ScatterWin));
            }
            else
            {
                EventCenter.Instance.EventTrigger<EventData>(SlotMachineEvent.ON_AUDIO_EVENT, new EventData(Game3998AudioEvent.WheellItWin));
            }
        }

        private void ChangeWheelURL(int state)
        {
            switch (state)
            {
                case 0:
                    gWheelLoad.url = CustomModel.Instance.wheelState["low"];
                    break;
                case 1:
                    gWheelLoad.url = CustomModel.Instance.wheelState["mid"];
                    break;
                case 2:
                    gWheelLoad.url = CustomModel.Instance.wheelState["high"];
                    break;

            }
        }
    }

    
}