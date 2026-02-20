using UnityEngine;

public class EnemySeeker : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private GameObject player;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private ShootingSeeker shootingSeeker;
    [SerializeField] private Transform seekerCrosshairTransform;
    [SerializeField] private Health seekerHealth;
    [SerializeField] private SpriteRenderer seekerSr;

    [Header("Internes")]
    public float aggroRange;
    public float maxCloseUp;
    public float minCloseUp;
    public float speed;
    public float wanderCountdown;
    public float timeBetweenFiringBullet = 1.2f;
    public float bulletTimer;
    public float seekerKnockbackDuration;
    public enum SeekerType 
    {
        Contact, Distance
    }
    public float pushForce = 15f;
    public float jumpForce;
     
    [SerializeField] private SeekerType seekerType;
    [SerializeField] private bool isSeekerGrounded;
    private float wanderTimer;
    private bool isSeekerFacingRight = true, canFire = true;
    private Vector2 wanderDirection;

    #region UNITY FUNCTIONS
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb.gravityScale = 0;
        player = GameObject.FindGameObjectWithTag("Player");
        seekerHealth.sr = seekerSr;
    }

    // Update is called once per frame
    void Update()
    {
        if(seekerHealth.isPushed)
        {
            return;
        }

        if(IsPlayerInAggroRange())
        {
            AggroPlayer(); 
        }
        else
        {
            Wandering();
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
                playerHealth.Damage(10);
                playerHealth.isPushed = true; 

                float dirX = Mathf.Sign(collision.transform.position.x - transform.position.x);
                Vector2 push = new Vector2(dirX * pushForce, jumpForce);
                playerRb.AddForce(push * pushForce, ForceMode2D.Impulse);

                playerHealth.StartCoroutine(playerHealth.ReleasePushFlag(seekerKnockbackDuration));
            }
        }

        if(collision.gameObject.CompareTag("Ground"))
        {
            isSeekerGrounded = true;
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

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            isSeekerGrounded = false;
        }
    }
    #endregion

    #region TYPE
    public void SetSeekerType(SeekerType type)
    {
        seekerType = type;
    }
    #endregion

    #region  Movement
    private bool IsPlayerInAggroRange()
    {
        return Vector2.Distance(player.transform.position, transform.position) <= aggroRange;
    }

    private void AggroPlayer()
    {
        switch (seekerType)
        {
            case SeekerType.Contact:
                AggroContact();
                break;

            case SeekerType.Distance:
                AggroDistance();
                break;
        }
    }

    private void AggroContact()
    {
        Vector2 dir = player.transform.position - transform.position;

        if(isSeekerGrounded)
        {
            dir.y = 0f;
        }

        if(seekerHealth.currentHealth == seekerHealth.maxHealth)
        {
            Wandering();
            return;
        }

        dir.Normalize();
        rb.linearVelocity = dir * speed;
    }

    private void AggroDistance()
    {
        Vector2 dir = player.transform.position - transform.position;

        if(isSeekerGrounded)
        {
            dir.y = 0f;
        }

        float distance = dir.magnitude;
        dir.Normalize();

        Vector2 seekerVelocity = rb.linearVelocity;

        if(distance > maxCloseUp)
        {
            seekerVelocity = dir * speed;
            ShotBullet(player.transform.position);
        }
        else if(distance < minCloseUp)
        {
            seekerVelocity = -dir * speed;
        }
        else
        {
            seekerVelocity = Vector2.zero;
            ShotBullet(player.transform.position);
        }

        rb.linearVelocity = seekerVelocity;
        
        if(Mathf.Abs(dir.x) > 0.01f)
        {
            Flip(dir.x > 0);
        }
    }

    private void Flip(bool facingRight)
    {
        if(facingRight != isSeekerFacingRight)
        {
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
            seekerCrosshairTransform.localScale = localScale;

            isSeekerFacingRight = facingRight;
        }
    }

    private void Wandering()
    {
        wanderTimer -= Time.deltaTime;

        if(wanderTimer <= 0)
        {
            wanderDirection = Random.insideUnitCircle.normalized;
            wanderTimer = wanderCountdown;
        }

        if(isSeekerGrounded)
        {
            wanderDirection.y = 0f;
        }

        rb.linearVelocity = wanderDirection * speed;
    }
    #endregion

    #region Shotting
    private void ShotBullet(Vector2 playerPosition)
    {
        if(!canFire)
        {
            bulletTimer += Time.deltaTime;
            if(bulletTimer > timeBetweenFiringBullet)
            {
                canFire = true;
                bulletTimer = 0;
            }
        }
        else
        {
            canFire = false;
        }
        shootingSeeker.RotateTowardPlayer(playerPosition, canFire);
    }
    #endregion
}
