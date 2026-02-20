/*using UnityEngine;

public class PlayerAnimationDriver : MonoBehaviour
{
    #region VARIABLES
    [SerializeField] private AnimationController anim;
    [SerializeField] private PlayerMouvement movement;
    [SerializeField] private Shooting shooting;
    [SerializeField] private PlayerRestController rest;
    [SerializeField] private AnimationData animationData;
    [SerializeField] private Animator animator;
    #endregion

    #region UNITY FUNCTIONS
    // Update is called once per frame
    void Update()
    {
        StateUpdate();
    }
    #endregion

    #region LOGIC
    private void StateUpdate()
    {
        if(movement.isDashing && anim.Current?.animationName != "Dash")
        {
            animator.SetTrigger("DashTrigger");
            anim.Play("Dash",animationData,animator);
        }
        else if(shooting.isCharging)
        {
            animator.SetTrigger("BeamTransform");
            anim.Play("ChargeBeam",animationData,animator);
        }
        else if(shooting.wantsToShoot)
        {
            anim.Play("ShootBeam",animationData,animator);
        }
        else if(movement.isGrabing)
        {
            anim.Play("Grab",animationData,animator);
        }
        else if(!movement.isGrounded && movement.rb.linearVelocity.y > 0f)
        {
            anim.Play("Jump",animationData,animator);
        }
        else if(!movement.isGrounded)
        {
            anim.Play("Fall",animationData,animator);
        }
        else if(movement.isMoving)
        {
            anim.Play("Move",animationData,animator);
        }
        else if(rest.isGettingUp)
        {
            anim.Play("GetUp",animationData,animator);
        }
        else if(rest.isSleeping)
        {
            anim.Play("Sleep",animationData,animator);
        }
        else if(rest.isSitting)
        {
            anim.Play("Sit",animationData,animator);
        }
        else
        {
            anim.Play("Idle",animationData,animator);
        }
    }
    #endregion
}
*/