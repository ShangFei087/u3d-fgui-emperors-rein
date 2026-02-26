
using FairyGUI;
using PusherEmperorsRein;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

using _spinWEMD = SlotMaker.SpinWinEffectSettingModel;

namespace SlotMaker
{



    [System.Serializable]  //Inspector窗口可见
    public partial class SymbolBase //: MonoBehaviour
    {
        protected ICustomModel customModel;

        public int number;
        public GLoader imgBase;
        public GButton btnBase;
        public GComponent goOwnerSymbol = null;


        public virtual void Init(ICustomModel customModel,GComponent goSbl)
        {
            this.customModel = customModel;
            goOwnerSymbol = goSbl;
            imgBase = goOwnerSymbol.GetChild("animator").asCom.GetChild("image").asLoader;
        }


        public virtual void SetSymbolActive(bool active)
        {
            imgBase.visible = active;
        }

        public virtual void SetSymbolImage(int symbolNumber, bool needNativeSize = false)
        {
            DebugUtils.Log("==@ 没实现 BaseIconItem - SetIconImage");
        }

        public virtual int GetSymbolNumber()
        {
            //DebugUtils.LogWarning("==@ 没实现 BaseIconItem - GetSymbolNumber");
            return number;
        }
        public virtual int GetSymbolIndex()
        {
            //DebugUtils.LogWarning("==@ 没实现 BaseIconItem - GetSymbolIndex");
            return number-1;
        }

        /// <summary>
        /// 设置图标是否可以点击？？？？
        /// </summary>
        /// <param name="state"></param>
        public virtual void SetBtnInteractableState(bool state)
        {
            if (btnBase != null)
                btnBase.touchable = true;
        }

        public virtual bool IsSpecailHitSymbol()
        {
            DebugUtils.LogWarning("==@ 没实现 BaseIconItem - IsSpecailIcon");
            return false;
        }


        /// <summary>
        /// 添加图标特效( 图标击中 、 图标出现、 图标触发)
        /// </summary>
        /// <param name="anchorSymbolEffect"></param>
        /// <param name="isAmin"></param>
        /// <returns></returns>
        public virtual GComponent AddSymbolEffect(GComponent anchorSymbolEffect, bool isAmin = true)
        {


            GameObject goTarget = GameCommon.FguiUtils.GetWrapperTarget(anchorSymbolEffect);
            if (goTarget != null)
            {
                AnimBaseUI animCtrl = goTarget.transform.GetComponent<AnimBaseUI>();
                
                if(animCtrl != null)
                {
                    if (isAmin)
                        animCtrl.Play();
                    else
                        animCtrl.Pause();//PlayReverse();
                    GameCommon.FguiUtils.RefreshWrapper(anchorSymbolEffect);
                }                
            }

            
            /*
            Animator animatorSpine = null;  //【待完成】  获取Spine的
            if (animatorSpine != null)
            {
                if (isAmin)
                    animatorSpine.speed = 1f;  // 播放
                else
                    animatorSpine.speed = 0f;  //暂停
            }*/

            GComponent goAnimator = goOwnerSymbol.GetChild("animator").asCom;
            goAnimator.AddChild(anchorSymbolEffect); //边长为1的点
            anchorSymbolEffect.xy = new Vector2(goAnimator.width / 2, goAnimator.height / 2);

            // 是否隐藏原有图标
            if (_spinWEMD.Instance.isHideBaseIcon)
            {
                HideBaseSymbolIcon(true);
            }

            return anchorSymbolEffect;
            // 播放动画
        }


        /// <summary>
        /// 添加图标特效( 图标击中 、 图标出现、 图标触发)
        /// </summary>
        /// <param name="anchorSymbolEffect"></param>
        /// <param name="isAmin"></param>
        /// <returns></returns>
      


        /// <summary>
        /// 添加边框特效
        /// </summary>
        /// <param name="anchorBorderEffect"></param>
        /// <returns></returns>
        public virtual GComponent AddBorderEffect(GComponent anchorBorderEffect)
        {
            GComponent goAnimator = goOwnerSymbol.GetChild("animator").asCom;
            goAnimator.AddChild(anchorBorderEffect);  //边长为1的点
                                                      //borderEffect.SetPivot(0.5f, 0.5f, true);
            anchorBorderEffect.xy = new Vector2(goAnimator.width / 2, goAnimator.height / 2);

            //DebugUtils.LogWarning("==@ 没实现 i am Base AddBorderEffect");
            return anchorBorderEffect;
            // 播放动画
        }

        /// <summary>
        /// 添加边框特效
        /// </summary>
        /// <param name="anchorBorderEffect"></param>
        /// <returns></returns>
        public virtual GComponent AddAnchor(GComponent text)
        {
            GComponent goAnimator = goOwnerSymbol.GetChild("animator").asCom;
            goAnimator.AddChild(text);  //边长为1的点
                                        //borderEffect.SetPivot(0.5f, 0.5f, true);
            text.xy = new Vector2(goAnimator.width / 2, goAnimator.height / 2);

            //DebugUtils.LogWarning("==@ 没实现 i am Base AddBorderEffect");
            return text;
            // 播放动画
        }

        public virtual void ShowBiggerEffect()
        {
            DebugUtils.Log("==@ i am Base ShowBiggerEffect");
        }

        public virtual void ShowTwinkleEffect()
        {
            DebugUtils.Log("==@ i am Base ShowTwinkleEffect");
        }

        public virtual void StopSymbolEffectBiggerTwinkle()
        {
            DebugUtils.Log("==@ i am Base StopSymbolEffect"); 
        }
        /// <summary>
        /// 击中图标时，是否隐藏原有的图标
        /// </summary>
        /// <param name="isHide"></param>
        public virtual void HideBaseSymbolIcon(bool isHide)
        {
            imgBase.visible = !isHide;
        }
    }
}