using System.Collections;
using UnityEngine;

public class BadelineMovement : MonoBehaviour
{
    #region VARIABLES
    [Header("Références")]
    [SerializeField] private GameObject player;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform crosshairTransform;
    [SerializeField] private Health badelineHealth;
    [SerializeField] private SpriteRenderer badelinerSR;
    [SerializeField] private UIController badelineUI;
    [SerializeField] private AmbianceManager ambianceManager;
    [SerializeField] private BadelineAttackController badelineAttackController;

    [Header("Internes")]
    public float aggroRange, awakeRange;
    public float maxCloseUp;
    public float minCloseUp;
    public float speed;
    public float wanderCountdown;
    public float pushForce = 15f;
    public float jumpForce;
    public float badelineKnockbackDuration;
    public float awekeningDuration;
    public bool canAttack = false;
    public bool isInCinematic = false;
    public bool badelineAwaken = false;
    public bool allowCinematicMovement = false;
    public event System.Action Awekening;
    public event System.Action OnAwakeningFinished;

    private float wanderTimer;
    private bool isBadelineFacingRight = true, isGettingClose = false;
    private bool awakeningCoroutineRunning = false;
    private Vector2 wanderDirection;
    private Canvas uiCanva;
    #endregion

    #region UNITY FUNCTIONS
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(SceneController.sceneInstance != null)
        {
            ambianceManager = SceneController.sceneInstance.GetComponent<AmbianceManager>();
        }

        rb.gravityScale = 0;
        player = GameObject.FindGameObjectWithTag("Player");
        badelineHealth.sr = badelinerSR;
        uiCanva = badelineUI.GetComponentInChildren<Canvas>();
        uiCanva.enabled = false;
        badelineHealth.cantBeDamaged = true;
        badelineHealth.cantDie = true;
    }
    // Update is called once per frame
    void Update()
    {
        if(!badelineAwaken)
        {
            rb.linearVelocity = Vector2.zero;
            PlayerIsNear();
        }

        if(isInCinematic || !badelineAwaken)
        {
            uiCanva.enabled = false;
            return;
        }

        if(badelineAttackController.enterCinematic)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if(badelineHealth.isPushed)
        {
            return;
        }

        if(IsPlayerInAggroRange(aggroRange) && !isGettingClose)
        {
            Wandering();
        }
        else
        {
            GettingClose();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        bool isPlayer = collision.gameObject.CompareTag("Player");
        bool isGround = collision.gameObject.CompareTag("Ground");  
        if(!isPlayer && !isGround)
        {
            return;
        }

        if(collision.gameObject.CompareTag("Player"))
        {
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();

            if(playerHealth.currentHealth > 0)
            {
                playerHealth.Damage(100);
                playerHealth.isPushed = true; 

                float dirX = Mathf.Sign(collision.transform.position.x - transform.position.x);
                Vector2 push = new Vector2(dirX * pushForce, jumpForce);
                playerRb.AddForce(push * pushForce, ForceMode2D.Impulse);

                playerHealth.StartCoroutine(playerHealth.ReleasePushFlag(badelineKnockbackDuration));
            }
        }

        if(collision.gameObject.CompareTag("Ground"))
        {
            ContactPoint2D contact = collision.GetContact(0);

            if(Mathf.Abs(contact.normal.x) < 0.5f)
            {
                return;
            }
            wanderDirection = Vector2.Reflect(wanderDirection, contact.normal);
            wanderDirection.y = 0f;
            wanderDirection.Normalize();
        }
    }
    #endregion

    #region MOVEMENT LOGICS
    private void PlayerIsNear()
    {
        if(!IsPlayerInAggroRange(awakeRange))
        {
            return;
        }
        StartCoroutine(AwakenBadeline());
    }
    private IEnumerator AwakenBadeline()
    {
        if(awakeningCoroutineRunning)
        {
             yield break;
        }

        Vector2 dir = player.transform.position - transform.position;

        float distance = dir.magnitude;
        dir.Normalize();

        if(Mathf.Abs(dir.x) > 0.01f)
        {
            Flip(dir.x > 0);
        }
        
        awakeningCoroutineRunning = true;
        Awekening?.Invoke();

        yield return new WaitForSeconds(awekeningDuration);

        badelineAwaken = true;
        awakeningCoroutineRunning = false;
        badelineHealth.cantBeDamaged = false;

        uiCanva.enabled = true;
        OnAwakeningFinished?.Invoke();

        if(isInCinematic)
        {
            yield break;
        }
        ambianceManager.FadeMusic(3);
    }
    private bool IsPlayerInAggroRange(float range)
    {
        return Vector2.Distance(player.transform.position, transform.position) <= range;
    }

    private void GettingClose()
    {
        Vector2 dir = player.transform.position - transform.position;

        float distance = dir.magnitude;
        dir.Normalize();

        Vector2 badelineVelocity = rb.linearVelocity;

        if(distance > aggroRange)
        {
            isGettingClose = true;
            badelineVelocity = dir * speed;
        }
        else if(distance < minCloseUp)
        {
            isGettingClose = false;
            badelineVelocity = -dir * speed;
        }

        rb.linearVelocity = badelineVelocity;

        if(Mathf.Abs(dir.x) > 0.01f)
        {
            Flip(dir.x > 0);
        }
    }

    private void Wandering()
    {
        wanderTimer -= Time.deltaTime;
    
        Vector2 dir = player.transform.position - transform.position;
        float distance = dir.magnitude;
        dir.Normalize();

        canAttack = badelineAwaken && !isInCinematic && distance >= minCloseUp && distance <= aggroRange;

        if(wanderTimer <= 0)
        {
            wanderDirection = Random.insideUnitCircle.normalized;

            wanderTimer = wanderCountdown;
        }

        if(distance > minCloseUp)
        {
            rb.linearVelocity = wanderDirection * speed;
        }
        else
        {
            rb.linearVelocity = -dir * speed;
        }

        if(Mathf.Abs(dir.x) > 0.01f)
        {
            Flip(dir.x > 0);
        }
    }

    private void Flip(bool facingRight)
    {
        if(facingRight == isBadelineFacingRight)
        {
            return;
        }

        isBadelineFacingRight = facingRight;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (facingRight ? 1 : -1);
        scale.y = Mathf.Abs(scale.y);
        scale.z = Mathf.Abs(scale.z);

        transform.localScale = scale;
        crosshairTransform.localScale = scale;
    }
    #endregion
}
