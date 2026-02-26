using Sirenix.OdinInspector;
using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using Animation = UnityEngine.Animation;

/*
* == 动画：
* Spine
* Animation
* Animator
* Tween
* Timer
* Cor
* GameObject
*/



public interface IAnimUI
{

    void play();
    
    void Pause();
    void Play(string animName, bool loop = false);

    void Pause(string name, float normalizedTime);
}

/// <summary>
/// # 动画ui组件
/// * 在父类写好通用方法
/// * 在子类写事件监听
/// * 父级的方法或属性是virtual类型，供子级复写
/// </summary>
public partial class AnimBaseUI : MonoBehaviour
{

    public GameObject goAnim;

    SkeletonGraphic skelGrap => goAnim ==null ? null: goAnim.GetComponent<SkeletonGraphic>();

    Animator animator => goAnim == null ? null : goAnim.GetComponent<Animator>();

    SkeletonAnimation skelAnim => goAnim == null ? null : goAnim.GetComponent<SkeletonAnimation>();



    //SkeletonMecanim skelMec => goAnim ==null ? null: goAnim.GetComponent<SkeletonMecanim>();


    Animation anim => goAnim == null ? null : goAnim.GetComponent<Animation>();


    string animName = null;


    AnimAnimatorHelper animatorHelper;

    private void Awake()
    {
        if (goAnim == null)
            goAnim = gameObject;

        animatorHelper = new AnimAnimatorHelper(goAnim);
    }
    [System.Serializable]
    public class AnimInfo
    {
        public string animName;
        public GameObject goAnim;
    }

    public List<AnimInfo> animList = new List<AnimInfo>();

    public virtual void Play(string animName, bool loop = false)
    {
        this.animName = animName;

        if (skelGrap != null)
        {
            //skel.AnimationState.SetAnimation(0, "ide", false);
            skelGrap.AnimationState.SetAnimation(0, animName, loop);
        }
        else if (skelAnim != null)
        {
            skelAnim.AnimationState.SetAnimation(0, animName, loop);
        }
        /*else if (skelMec != null)
        {

        }*/
        else if (animator != null) { 
            if (animator.HasState(0, Animator.StringToHash(animName)))
            {
                animator.Play(animName);
                //animator.speed = 1;
            }
        }
        else if (animList != null)
        {
            for (int i=0; i< animList.Count; i++)
            {
                animList[i].goAnim.SetActive(animList[i].animName == animName);
            }
        }
        else if (anim != null)
        {
            // 播放
            //animition.Play("ani_name");
            // 暂停
            //animition["ani_name"].speed = 0;
            // 继续播放
            //animition["ani_name"].speed = 1;

            anim.Play(animName);
            //anim[animName].speed = 1;
        }
    }


    string m_State = "";
    public virtual string state
    {
        get => m_State;
        set
        {
            if (m_State != value)
            {
                OnValueChagne(value);
            }
            m_State = value;
        }
    }

    public const string STOP = "Defalut";
    protected virtual void OnValueChagne(string state)
    {
        if (state == STOP)
        {
            _AnimStop();
        }
        else
        {
            Play(state, true);
        }
    }

    protected virtual void _AnimStop(string animName = STOP)
    {

        if (skelGrap != null)
        {
            skelGrap.AnimationState.SetAnimation(0, animName, false);
        }
        else if (skelAnim != null)
        {
            skelAnim.AnimationState.SetAnimation(0, animName, false);
        }
        /*else if (skelMec != null)
        {

        }*/

        else if (animatorHelper != null)
        {
            if (animatorHelper.HasState(0, animName))
            {
                animatorHelper.Play(animName);
            }
        }
        else if (animList != null)
        {
            for (int i = 0; i < animList.Count; i++)
            {
                animList[i].goAnim.SetActive(false);
            }
        }
    }



    [Button]
    void TestRunAnim(string name, bool loop = false)
    {
        Play(name, loop);
    }

}


public partial class AnimBaseUI : MonoBehaviour
{

    public void Kill()
    {

    }
    public virtual void Play()
    {

        animatorHelper.play();


        /*else if (anim != null && !string.IsNullOrEmpty(animName))
        {
            // 继续播放
            //[Bug]
            anim[animName].speed = 1f;
        }*/
    }

    public void Pause(string name,float normalizedTime)
    {
        this.animName = name;
        if (animatorHelper != null)
        {
            //暂停
            animatorHelper.Play(animName, 0,normalizedTime);
            // 暂停
            animatorHelper.speed=0;
        }
        else if (anim != null)
        {
            // 播放
            anim.Play(animName);
            // 暂停
            anim[animName].speed = 0f;
        }
    }
    public void Pause()
    {
        if (animatorHelper != null)
        {
            //暂停
            animatorHelper.speed = 0;
            //继续播放
            //animator.speed = 1f;
        }
       /*else if (anim != null && !string.IsNullOrEmpty(animName))
        {
            // 播放
            //anim.Play("ani_name");
            // 暂停
            //anim["ani_name"].speed = 0f;
            // 继续播放
            //anim["ani_name"].speed = 1f;


            //[Bug]
            anim.Play(animName);
            anim[animName].speed = 0; 
        }*/
    }

    public void PlayFrame(string name, int frame)
    {
        if (animatorHelper.GetCurrentAnimatorClipInfo(0)[0].clip.name != null)
            return;

        //当前动画机播放时长
        float currentTime = animatorHelper.GetCurrentAnimatorStateInfo(0).normalizedTime;
        //动画片段长度
        float length = animatorHelper.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        //获取动画片段帧频
        float frameRate = animatorHelper.GetCurrentAnimatorClipInfo(0)[0].clip.frameRate;
        //计算动画片段总帧数
        float totalFrame = length / (1 / frameRate);
        //计算当前播放的动画片段运行至哪一帧
        int currentFrame = (int)(Mathf.Floor(totalFrame * currentTime) % totalFrame);
        DebugUtils.Log(" Frame: " + currentFrame + "/" +totalFrame);

       // while (frame > totalFrame)
            //frame = frame - totalFrame;

        float normalizedFrameTime = frame / totalFrame;
        animatorHelper.Play(animatorHelper.GetCurrentAnimatorClipInfo(0)[0].clip.name, 0, normalizedFrameTime);
        animatorHelper.speed = 1;

    }

    public void PlayPre(string name, float pre)
    {
        if (pre < 0 || pre > 1)
            DebugUtils.LogError("pre must between 0 - 1");

        if (animatorHelper != null)
        {
            animatorHelper.Play(name,0,pre);
        }
    }

    public void PlayReverse()
    {
        animatorHelper.PlayReverse();

    }
    public void PlayReverse(bool isReverse, string name=null)
    {
        animatorHelper.PlayReverse();
        /*
        if (animatorHelper != null)
        {
            //Speed-Multiplier添加SpeedOpen
            if (isReverse)
            {
                //正播
                animatorHelper.SetFloat("Multiplier", 1f);
                animatorHelper.Play(name, -1, 0f);
            }
            else
            {
                // 倒播
                animatorHelper.SetFloat("Multiplier", -1f);
                animatorHelper.Play(name, -1, 1f);
            }
        }*/
    }

    public void PauseAtLast(string name=null)
    {
        if (animatorHelper != null)
        {
            if (name != null)
            {
                animatorHelper.Play(name, 0, 0.99f);
                animatorHelper.speed = 0;
            }
            else
            {
                // 获取当前正在播放的动画状态信息
                AnimatorStateInfo currentState = animatorHelper.GetCurrentAnimatorStateInfo(0);

                // 直接使用哈希值（推荐，性能更好）
                int currentAnimHash = currentState.shortNameHash;

                DebugUtils.LogError(currentAnimHash);
                // 播放当前动画，并跳转到接近最后一帧的位置
                animatorHelper.Play(currentAnimHash, 0, 0.99f);

                // 暂停动画
                animatorHelper.speed = 0;
            }

        }

    }


    public void ReversePlay()
    {
        animatorHelper.PlayReverse();
        DebugUtils.LogError("fanxiang");
    }
}