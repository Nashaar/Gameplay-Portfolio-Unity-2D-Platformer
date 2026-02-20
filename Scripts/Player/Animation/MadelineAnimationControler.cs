using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UIElements;

public class MadelineAnimationControler : MonoBehaviour
{
    #region VARIABLES
    [Header("Références")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMouvement playerMovement;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Shooting shooting;
    [SerializeField] private CrosshairAnimationControler crosshairAnimationControler;
    [SerializeField] private LaserWeapon laserWeapon;


    [Header("États internes")]
    private bool isMoving;
    public bool activateLaser = false;
    public bool regenCrosshair = false;
    public bool isShootingBeam;
    public bool shouldBlockMovement;
    public bool isSleeping;
    public bool canSleep = true;

    private AnimatorStateInfo info;

    public enum PlayerState
    {
        Idle = 0,
        Move = 1,
        Sit = 2,
        Sleep = 3,
        GetUp = 4,
        Jump = 5,
        Fall = 6,
        ChargBeam = 7,
        ShootBeam = 8,
        Dash = 9,
        Grab = 10,
        Monster = 11
    }
    public PlayerState currentState = PlayerState.Idle;
    const string STATE_PARAM = "State";

    [SerializeField] private InputAction moveAction,jumpAction, restAction;
    private float jumpInput, originalGravity;
    private Vector2 moveInput;
    private Coroutine sitCoroutine;
    #endregion

    #region UNITY FUNCTIONS
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        restAction = InputSystem.actions.FindAction("Rest");
    }

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
        regenCrosshair = false;
        activateLaser = false;
        isShootingBeam = false;
        isSleeping = false;
        originalGravity = rb.gravityScale;
    }

    // Update is called once per frame
    void Update()
    {
        if(moveAction == null || jumpAction == null || animator == null || playerMovement == null || rb == null)
        {
            return;
            
        }
        if(Time.timeScale == 0)
        {
            return;
        }
            
        moveInput = moveAction.ReadValue<UnityEngine.Vector2>();
        isMoving = moveInput.x != 0;
        jumpInput = jumpAction.ReadValue<float>();
        BeamCharge();
        Idling();
        Jumping();
        Falling();   
        
        Moving();

        WakingUp();
        Dashing();
        RestingCounter();
        ToggleLaser();
        AllowMovement();
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        restAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        restAction.Disable();
    }
    #endregion

    #region SIT / GET UP / REST
    private void ResetSitCoroutine()
    {
        if(sitCoroutine != null && (currentState == PlayerState.Sit))
        {
            return;
        }
        if(sitCoroutine != null)
        {
            StopCoroutine(sitCoroutine);
            sitCoroutine = null;
        }
    }

    public IEnumerator Sit()
    {
        yield return new WaitForSeconds(30f);
        MadelineAnimationStateMachine(PlayerState.Sit);
        
        yield return new WaitUntil(() =>
        {
            info = animator.GetCurrentAnimatorStateInfo(0);
            return info.IsTag("Sit") && info.normalizedTime >= 0.98f;
        });
        isSleeping = true;
        MadelineAnimationStateMachine(PlayerState.Sleep);
        sitCoroutine = null;
    }

    public IEnumerator InstantSit()
    {
        MadelineAnimationStateMachine(PlayerState.Sit);
        
        yield return new WaitUntil(() =>
        {
            info = animator.GetCurrentAnimatorStateInfo(0);
            return info.IsTag("Sit") && info.normalizedTime >= 0.98f;
        });
        isSleeping = true;
        MadelineAnimationStateMachine(PlayerState.Sleep);
        sitCoroutine = null;
    }
    
    public IEnumerator GetUp()
    {
        isSleeping = false;
        yield return new WaitUntil(() =>
        {
            info = animator.GetCurrentAnimatorStateInfo(0);
            return info.IsTag("GetUp") && info.normalizedTime >= 0.98f;
        });
        
        MadelineAnimationStateMachine(PlayerState.Idle);
    }

    private void WakingUp()
    {
        if((currentState == PlayerState.Sleep) && (isMoving || jumpInput > 0))
        {
            MadelineAnimationStateMachine(PlayerState.GetUp);
        }
        if((currentState == PlayerState.Sleep) && playerMovement.isDashing)
        {
            MadelineAnimationStateMachine(PlayerState.Dash);
        }
    }

    private void RestingCounter()
    {
        bool restPressed = restAction.WasPressedThisFrame();

        bool isInactive =   currentState != PlayerState.Move &&
                            currentState != PlayerState.Jump &&
                            currentState != PlayerState.Dash &&
                            currentState != PlayerState.ChargBeam &&
                            currentState != PlayerState.ShootBeam &&
                            currentState != PlayerState.Sleep ;


        if(restPressed && currentState == PlayerState.Idle && canSleep)
        {
            StartCoroutine(InstantSit());
        }
        
        if(isInactive && canSleep)
        {
            if(sitCoroutine == null)
            {
                sitCoroutine = StartCoroutine(Sit());
            }
            
        }
        else
        {
            ResetSitCoroutine();
        }
    }
    #endregion
    
    #region BEAM
    public IEnumerator ShootBeam()
    {
        laserWeapon.laserCompt = 0;
        isShootingBeam = true;
        
        yield return new WaitUntil(() =>
        {
            info = animator.GetCurrentAnimatorStateInfo(0);
            return info.IsTag("ShootBeam");
        });
        
        yield return new WaitUntil(() =>
        {
            info = animator.GetCurrentAnimatorStateInfo(0);
            return info.IsTag("ShootBeam") && info.normalizedTime >= 0.98f;
        });
        
        rb.gravityScale = originalGravity;
        activateLaser = false;
        isShootingBeam = false;
        shooting.gameObject.transform.position -= new UnityEngine.Vector3(0, 2, 0);
        regenCrosshair = true;
        shooting.isAlreadyShooting = false;
        shooting.wantsToShoot = false;
        playerMovement.canMove = true;
        MadelineAnimationStateMachine(PlayerState.Idle);
    }

    private void BeamCharge()
    {
        if(playerMovement.isGrabing)
        {
            return;
        }
        if(shooting.isCharging && !(currentState == PlayerState.ChargBeam))
        {
            shooting.isAlreadyShooting = true;
            MadelineAnimationStateMachine(PlayerState.ChargBeam);
        }
        if(shooting.wantsToShoot && (currentState == PlayerState.ChargBeam) && !(currentState == PlayerState.ShootBeam))
        {
            MadelineAnimationStateMachine(PlayerState.ShootBeam);
        }
    }

    private void ToggleLaser()
    {
        info = animator.GetCurrentAnimatorStateInfo(0);
        if(info.IsName("BeamLock"))
        {
            activateLaser = true;
        }
    }
    #endregion
    
    #region IDLE
    private void Idling()
    {
        if(currentState == PlayerState.ChargBeam || currentState == PlayerState.ShootBeam || currentState == PlayerState.Sit || currentState == PlayerState.Sleep || currentState == PlayerState.GetUp || currentState == PlayerState.Dash)
        {
            return;
        }
        if(!isMoving && jumpInput == 0 && rb.linearVelocity.y == 0f)
        {
            MadelineAnimationStateMachine(PlayerState.Idle);
        }
    }
    #endregion

    #region JUMP / FALL
    private void Jumping()
    {
        if(currentState == PlayerState.ChargBeam || currentState == PlayerState.ShootBeam || currentState == PlayerState.Sit || currentState == PlayerState.Sleep || currentState == PlayerState.GetUp || currentState == PlayerState.Dash)
        {
            return;
        }
        if(!(currentState == PlayerState.Sit) && !(currentState == PlayerState.ChargBeam) && !(currentState == PlayerState.ShootBeam))
        {
            if(jumpInput > 0 && rb.linearVelocity.y > 0f)
            {
                MadelineAnimationStateMachine(PlayerState.Jump);
            }
        }

        if(playerMovement.isGrabing)
        {
            MadelineAnimationStateMachine(PlayerState.Grab);
            ResetSitCoroutine();
            return;
        }
        if(currentState == PlayerState.Grab && !playerMovement.isGrabing)
        {
            MadelineAnimationStateMachine(PlayerState.Fall);
        }
        else if(playerMovement.wasGrabing && jumpInput > 0 && rb.linearVelocity.y > 0f)
        {
            MadelineAnimationStateMachine(PlayerState.Jump);
        }
    }

    private void Falling()
    {
        if(currentState == PlayerState.ChargBeam || currentState == PlayerState.ShootBeam || currentState == PlayerState.Sit || currentState == PlayerState.Sleep || currentState == PlayerState.GetUp || currentState == PlayerState.Dash)
        {
            return;
        }
        if(rb.linearVelocity.y < 0f && !playerMovement.isGrounded)
        {
            ResetSitCoroutine();
            MadelineAnimationStateMachine(PlayerState.Fall);
        }
        else if(playerMovement.isGrounded && (currentState == PlayerState.Fall))
        {
            ResetSitCoroutine();
            MadelineAnimationStateMachine(PlayerState.Idle);
        }

        if(playerMovement.isGrabing)
        {
            MadelineAnimationStateMachine(PlayerState.Grab);
            ResetSitCoroutine();
            return;
        }
        if(currentState == PlayerState.Grab && !playerMovement.isGrabing)
        {
            MadelineAnimationStateMachine(PlayerState.Fall);
        }
    }
    #endregion

    #region MOVE
    private void AllowMovement()
    {
        shouldBlockMovement =   (currentState == PlayerState.Sit) 
                                || (currentState == PlayerState.Sleep) 
                                || (currentState == PlayerState.GetUp) 
                                || (currentState == PlayerState.ChargBeam) 
                                || (currentState == PlayerState.ShootBeam) 
                                || (currentState == PlayerState.Dash);
        playerMovement.canMove = !shouldBlockMovement;
    }

    private void Moving()
    {
        if(currentState == PlayerState.ChargBeam || currentState == PlayerState.ShootBeam || currentState == PlayerState.Sit || currentState == PlayerState.Sleep || currentState == PlayerState.GetUp || currentState == PlayerState.Dash)
        {
            return;
        }
        if(isMoving && !(currentState == PlayerState.Sleep))
        {
            if(jumpInput > 0 && rb.linearVelocity.y > 0f)
            {
                Jumping();
            }
            else if(rb.linearVelocity.y < 0f && !playerMovement.isGrounded)
            {
                Falling();
            }
            else if(playerMovement.isGrabing)
            {
                MadelineAnimationStateMachine(PlayerState.Grab);
                ResetSitCoroutine();
            }
            else if(playerMovement.wasGrabing)
            {
                MadelineAnimationStateMachine(PlayerState.Fall);
                ResetSitCoroutine();
            }
            else
            {
                MadelineAnimationStateMachine(PlayerState.Move);
                ResetSitCoroutine();
            }
        }
    }

    private void Dashing()
    {
        if(currentState == PlayerState.ChargBeam || currentState == PlayerState.ShootBeam)
        {
            return;
        }

        if(playerMovement.isDashing)
        {
            ResetSitCoroutine();
            MadelineAnimationStateMachine(PlayerState.Dash);
        }
        else if(currentState == PlayerState.Dash && !playerMovement.isDashing)
        {
            MadelineAnimationStateMachine(PlayerState.Idle);
        }
    }
    #endregion

    #region STATE MACHINE
    public void MadelineAnimationStateMachine(PlayerState newState)
    {
        if(newState == currentState)
        {
            return;
        }

        currentState = newState;
        EnterState(currentState);
        animator.SetInteger(STATE_PARAM, (int)currentState);
    }

    private void EnterState(PlayerState state)
    {
        switch(state)
        {
            case PlayerState.ChargBeam :
                crosshairAnimationControler.EraseCrosshair();
                playerMovement.canMove = false;
                rb.linearVelocity = Vector2.zero;
                if(!playerMovement.isGrounded)
                {
                    rb.gravityScale = 0;
                }
                animator.SetTrigger("BeamTransform");
                break;
            
            case PlayerState.ShootBeam :
                StartCoroutine(ShootBeam());
                break;

            case PlayerState.Sleep :
                break;

            case PlayerState.GetUp :
                StartCoroutine(GetUp());
                break;

            case PlayerState.Dash :
                animator.SetTrigger("DashTrigger");
                break;

            case PlayerState.Monster : 
                animator.SetTrigger("Monster");
                break;

            default:
                break;
        }
    }
    #endregion
}