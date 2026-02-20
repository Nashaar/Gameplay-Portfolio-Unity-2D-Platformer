using System.Collections;
using UnityEngine;
public class CrosshairAnimationControler : MonoBehaviour
{
    [Header("Références")] 
    public Animator animator; 
    public Shooting shooting; 
    public MadelineAnimationControler madelineAnimationControler; 

    public enum CrosshairState 
    { Idle = 0, Regen = 1 } 
    public CrosshairState currentState = CrosshairState.Idle; 
    const string STATE_PARAM = "State"; 
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created 
    void Start()
    { 
        if(animator == null) 
        { 
            animator = GetComponent<Animator>();
        }
        if(animator != null)
        {
            animator.SetInteger(STATE_PARAM, (int)currentState);
        } 
    } 
    
    // Update is called once per frame 
    void Update()
    {
        if(madelineAnimationControler.regenCrosshair) 
        { 
            CrosshairAnimationStateMachine(CrosshairState.Regen);
        }
        else
        { 
            CrosshairAnimationStateMachine(CrosshairState.Idle);
        } 
    }
    
    public IEnumerator EraseCrosshair()
    {
        yield return new WaitUntil(() =>
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0); 
            return info.IsTag("EraseCrosshair");
        }); 
            
        yield return new WaitUntil(() =>
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0); 
            return info.IsTag("EraseCrosshair") && info.normalizedTime >= 0.98f;
        }); 
        
        shooting.spriteRendererCrosshair.enabled = false;
    } 
    
    private void CrosshairAnimationStateMachine(CrosshairState newState)
    { 
        if(newState == currentState) 
        { 
            return;
        } 
        currentState = newState; 
        EnterState(currentState); 
        animator.SetInteger(STATE_PARAM, (int)currentState); 
    } 
    
    private void EnterState(CrosshairState state) 
    { 
        switch(state)
        { 
            case CrosshairState.Regen : shooting.spriteRendererCrosshair.enabled = true;
            madelineAnimationControler.regenCrosshair = false;
            break;
        } 
    }
}