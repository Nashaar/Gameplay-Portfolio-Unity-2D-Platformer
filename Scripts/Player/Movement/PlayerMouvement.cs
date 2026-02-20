using System.Collections; 
using UnityEngine; 
using UnityEngine.InputSystem; 
[RequireComponent(typeof(Rigidbody2D))] 
public class PlayerMouvement : MonoBehaviour 
{ 
    #region VARIABLES 
    [Header("Reference")] 
    [SerializeField] private Health playerHealth; 
    [SerializeField] private BackgroundController backgroundController; 
    [SerializeField] private UIController uiController; 
    [SerializeField] private Shooting shooting; 
    [SerializeField] private AmbianceManager ambianceManager;
    
    [Header("Paramètres internes")] 
    public Rigidbody2D rb; 

    [SerializeField] private int playerLayer; 
    [SerializeField] private int enemyLayer; 
    [SerializeField] private Transform crosshairTransform; 
    private float originalGravity; 
    private Vector2 moveInput; 
    
    [Header("Paramètres de mouvement")] 
    public float speed = 5f; 
    public float jumpForce = 9f; 
    public float dashingPower = 35f; 
    public float dashingTime = 0.3f; 
    public float dashingCooldown = 1f; 
    public bool canMove = true; 
    public bool canDash = true; 
    public bool isDashing; 
    public bool dashGet = false;

    private bool isFacingRight = true;  
    
    [Header("Input")] 
    public InputAction moveAction, jumpAction, dashAction; 
    public bool isMoving;
    public bool jumpPressed;
    public bool jumpHeld;
    public bool dashPressed;

    [Header("Vérification du sol")] 
    public float checkRadius = 0.2f; 
    public bool isGrounded; 

    [SerializeField] private Transform groundCheck; 
    [SerializeField] private LayerMask groundLayer; 
    
    [Header("Vérification du grab")] 
    public bool isGrabing, wasGrabing, endGrabingTimer, iswallJumping;

    [SerializeField] private Transform wallCheck; 
    [SerializeField] private LayerMask wallLayer;
    private float grabCountdown = 3f, grabTimer = 0f, grabReenableDelay = 0.1f, grabReenableTimer = 0f, verticalDeadZone = 0.3f;
    
    [Header("Composant enfant")] 
    public SpriteRenderer spriteRenderer; 
    #endregion 
    
    #region UNITY FUNCTIONS 
    private void Awake()
    { 
        rb = GetComponent<Rigidbody2D>(); 
        playerLayer = LayerMask.NameToLayer("Player"); 
        enemyLayer = LayerMask.NameToLayer("Enemy"); 
    }
    
    private void Start() 
    { 
        if(SceneController.sceneInstance != null)
        {
            ambianceManager = SceneController.sceneInstance.GetComponent<AmbianceManager>();
        }

        moveAction = InputSystem.actions.FindAction("Move"); 
        jumpAction = InputSystem.actions.FindAction("Jump"); 
        dashAction = InputSystem.actions.FindAction("Dash"); 
        canMove = true; canDash = true; 
        originalGravity = rb.gravityScale; 
        grabTimer = grabCountdown; 
    } 
    
    private void Update() 
    { 
        if(Time.timeScale == 0) 
        {
            return;
        } 
        if(isDashing) 
        {
             return;
        }

        if(isGrounded)
        {
            ReleaseGrab();
        }
        
        InputReader();
        SpaceState(); 
        MoveFunction(); 
        Flip(); 
        JumpFunction(); 
        DashFunction(); 
        ReAbleGrab(); 
    } 
    
    private void FixedUpdate() 
    { 
        if(!canMove || isDashing) 
        { 
            return; 
        } 
        
        if(playerHealth.isPushed) 
        { 
            ReleaseGrab(); 
            return; 
        } 
        
        if(isGrabing) 
        { 
            float wallSlideSpeed = -1f; 
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, wallSlideSpeed)); 
            return;
        } 
        
        if(!wasGrabing && !iswallJumping) 
        { 
            rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y); 
        }
    }
    
    private void OnEnable() 
    { 
        moveAction.Enable(); 
        jumpAction.Enable(); 
        dashAction.Enable();
    } 
    
    private void OnDisable() 
    { 
        moveAction.Disable(); 
        jumpAction.Disable(); 
        dashAction.Disable();
    } 
    #endregion 

    #region INPUT READER
    private void InputReader()
    {
        jumpPressed = jumpAction.WasPressedThisFrame();
        jumpHeld = jumpAction.IsPressed();
        dashPressed = dashAction.WasPressedThisFrame(); 
    }
    #endregion
    
    #region MOVE 
    private void MoveFunction() 
    { 
        moveInput = moveAction.ReadValue<Vector2>(); 
        isMoving = moveInput.x != 0;
        if(isGrabing && !isGrounded) 
        { 
            GrabTimer();
        }
    } 
    
    private void Flip() 
    { 
        if(iswallJumping) 
        { 
            return;
        } 
        if((isFacingRight && moveInput.x < 0f) || (!isFacingRight && moveInput.x > 0f)) 
        { 
            isFacingRight = !isFacingRight; 
            Vector3 localScale = transform.localScale; localScale.x *= -1f; 
            transform.localScale = localScale; 
            crosshairTransform.localScale = localScale; 
        } 
    } 
    #endregion 
    
    #region JUMP 
    private void JumpFunction() 
    { 
        if(!jumpPressed) 
        {
             return; 
        } 
        else
        { 
            if(isGrabing) 
            { 
                WallJump(); 
                return;
            } 
            if(isGrounded && canMove) 
            { 
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
        }
    } 
    #endregion 
    
    #region GRAB 
    private void SpaceState() 
    { 
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer); 
        if(!wasGrabing && !isGrounded) 
        { 
            isGrabing = Physics2D.OverlapCircle(wallCheck.position, checkRadius, wallLayer);
        }

        if(isGrounded) 
        { 
            wasGrabing = false; 
            iswallJumping = false; 
            endGrabingTimer = false;
        }

        if(isGrabing && iswallJumping) 
        { 
            iswallJumping = false;
        }
    } 
    
    private void GrabTimer() 
    { 
        if(!isGrabing) 
        { 
            return;
        } 
        grabTimer -= Time.deltaTime; 
        grabTimer = Mathf.Max(grabTimer,0f); 
        if(grabTimer <= 0) 
        { 
            endGrabingTimer = true;
            ReleaseGrab(); 
        } 
    }
    private void ReleaseGrab() 
    { 
        isGrabing = false; 
        wasGrabing = true; 
        grabReenableTimer = grabReenableDelay; 
        grabTimer = grabCountdown;
    } 
    
    private void ReAbleGrab() 
    { 
        if(wasGrabing && !endGrabingTimer)
        { 
            grabReenableTimer -= Time.deltaTime;
            if(grabReenableTimer <= 0f) 
            { 
                wasGrabing = false;
            }
        }
    } 
    
    private void WallJump() 
    { 
        if(endGrabingTimer) 
        { 
            return;
        } 
        rb.linearVelocity = Vector2.zero; 
        int verticalIntent; 
        if(moveInput.y < -verticalDeadZone) 
        { 
            verticalIntent = -1;
        } 
        else 
        { 
            verticalIntent = 1;
        } 
        ReleaseGrab(); 
        iswallJumping = true; 
        isFacingRight = !isFacingRight; 
        Vector3 localScale = transform.localScale; localScale.x *= -1f; 
        transform.localScale = localScale; 
        crosshairTransform.localScale = localScale; 
        float wallJumpDir = isFacingRight ? 1f : -1f; 
        rb.linearVelocity = new Vector2(wallJumpDir * speed * 1.2f, jumpForce * verticalIntent);
    } 
    #endregion 
    
    #region DASH 
    private void DashFunction() 
    {
        if(!dashGet)
        {
            return;
        } 
        uiController.UpdateSpell(canDash, UIController.SpellType.Dash); 
        
        if(
            dashPressed 
            && canDash 
            && !isGrabing 
            && !shooting.isCharging 
            && !shooting.wantsToShoot) 
        { 
            StartCoroutine(Dash());
        }
    } 
    private IEnumerator Dash() 
    { 
        canDash = false;
        canMove = false;
        isDashing = true; 
        playerHealth.cantBeDamaged = true;
        
        Physics2D.IgnoreLayerCollision(playerLayer,enemyLayer,true); 
        originalGravity = rb.gravityScale; 
        rb.gravityScale = 0; 
        rb.linearVelocity = Vector2.zero; 

        ambianceManager.PlaySFX(17);

        rb.linearVelocity = new Vector2(transform.localScale.x * dashingPower,0f); 

        if(backgroundController != null) 
        { 
            backgroundController.Recenter();
        } 
        yield return new WaitForSeconds(dashingTime); 
        rb.gravityScale = originalGravity; 
        isDashing = false; 
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false); 
        rb.linearVelocity = Vector2.zero; 
        yield return new WaitForSeconds(dashingCooldown); 
        canDash = true; 
        canMove = true;
        playerHealth.cantBeDamaged = false;
    } 
    #endregion
}