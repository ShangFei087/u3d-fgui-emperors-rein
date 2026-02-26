using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimAnimatorHelper : IAnimUI
{
    private GameObject goOwner;


    public AnimAnimatorHelper(GameObject go)
    {
        goOwner = go;
    }
    private Animator animator => goOwner == null ? null : goOwner.GetComponent<Animator>();
    public float speed
    {
        get
        {
            return animator.speed;
        }
        set
        {
            animator.speed = value;
        }
    }
    public void play()
    {
        if(animator ==null) return;
        // 继续播放
        animator.speed = 1f;
    }

    public void Pause()
    {
        if(animator ==null) return;
        // 继续播放
        animator.speed = 0f;
    }

    public void Play(string animName, bool loop = false)
    {
        if(animator ==null) return;
    }

    public void Play(string animName, int layer, float time)
    {
        if (animator == null) return;
        animator.Play(animName, layer, time);
    }

    public void Play(int Hash, int layer, float time)
    {
        if (animator == null) return;
        animator.Play(Hash, layer, time);
    }


    public void Pause(string name, float normalizedTime)
    {
        if(animator ==null) return;
    }

    public bool HasState(int index, string name)
    {
        return animator.HasState(0, Animator.StringToHash(name));
    }


    public AnimatorClipInfo[] GetCurrentAnimatorClipInfo(int layer)
    {
       return animator.GetCurrentAnimatorClipInfo(layer);
    }

    public AnimatorStateInfo GetCurrentAnimatorStateInfo(int layer)
    {
        return animator.GetCurrentAnimatorStateInfo(layer);
    }

    public void SetFloat(string name,float value)
    {
        animator.SetFloat(name, value);
    }

    public void PlayReverse()
    {
        Play(true, "Auto");
        //animator.speed = -1;
    }


    public void Play(bool isReverse, string name = null)
    {
            if (animator == null) return;

            //Speed-Multiplier 添加变量 mpl
            if (!isReverse)
            {
                    //正播
                    animator.speed = 1f;
                    animator.SetFloat("mlp", 1f);
                    animator.Play(name, -1, 0f);
            }
            else
            {
                // 倒播
                animator.speed = 1f;
                animator.SetFloat("mlp", -1f);
                animator.Play(name, -1, 1f);
            }
        
    }
}
