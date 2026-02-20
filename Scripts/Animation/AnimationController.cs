using System;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    #region VARIABLES
    public AnimationData.Animation Current { get; private set; }

    public event Action<AnimationData.Animation> OnAnimationStart;
    public event Action<AnimationData.Animation> OnAnimationEnd;
    #endregion

    #region ANIMATION FUNCTIONS
    public void Play(string animationName, AnimationData animationData, Animator animator)
    {
        AnimationData.Animation anim = animationData.Get(animationName);
        if(anim == null || anim == Current) 
        {
            return;
        }

        Current = anim;
        
        animator.SetInteger("State", anim.animationID);

        if(!string.IsNullOrEmpty(anim.triggerName))
        {
            animator.SetTrigger(anim.triggerName);
        }

        OnAnimationStart?.Invoke(anim);
    }
    
    public void AnimationEndEvent(string animationName, AnimationData eventAnimationData)
    {
        AnimationData.Animation anim = eventAnimationData.Get(animationName);
        if(anim != null)
        {
            OnAnimationEnd?.Invoke(anim);
        }
    }
    #endregion
}
