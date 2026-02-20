using UnityEngine;

public class BadelineBullet : MonoBehaviour
{
    [Header("Références")]
    public GameObject player;
    public GameObject boss;
    private Rigidbody2D rb;

    [Header("Internes")]
    public float force;
    public float destroyTime;
    public float pushForce;
    public float jumpForce;
    public float badelineBulletKnockbackDuration;
    public int bulletDamage;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");

        if(player == null)
        {
            return;
        }

        Vector2 dir = player.transform.position - transform.position;
        float rotZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        rb.linearVelocity = new Vector2(dir.x, dir.y).normalized * force;

        transform.rotation = Quaternion.Euler(0, 0, rotZ + 90);

        Destroy(gameObject, destroyTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject == boss)
        {
            return;
        }
        if(collision.gameObject.CompareTag("Checkpoint") || collision.gameObject.CompareTag("Badeline"))
        {
            return;
        }

        if(collision.gameObject.CompareTag("Player"))
        {
            Health playerHealth = collision.GetComponent<Health>();
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();

            if(playerHealth.currentHealth > 0)
            {
                playerHealth.Damage(bulletDamage);
                playerHealth.isPushed = true;

                float dirX = Mathf.Sign(collision.transform.position.x - transform.position.x);
                Vector2 push = new Vector2(dirX * pushForce, jumpForce);
                playerRb.AddForce(push * pushForce, ForceMode2D.Impulse);

                playerHealth.StartCoroutine(playerHealth.ReleasePushFlag(badelineBulletKnockbackDuration));
            }
        }
        Destroy(gameObject, 0);
    }
}
