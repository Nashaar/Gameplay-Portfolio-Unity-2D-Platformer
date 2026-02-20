using UnityEngine;
using UnityEngine.InputSystem;

public class BulletScript : MonoBehaviour
{
    private Rigidbody2D rb;
    public float force;
    public float destroyTime;
    public float pushForce;
    public float jumpForce;
    public float bulletKnockbackDuration;
    public int bulletDamage;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.linearVelocity = transform.right * force;

        Destroy(gameObject, destroyTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Checkpoint"))
        {
            return;
        }

        if(collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Badeline"))
        {
            Health enemyHealth = collision.GetComponent<Health>();
            Rigidbody2D enemyRb = collision.GetComponent<Rigidbody2D>();

            if(enemyHealth.currentHealth > 0)
            {
                enemyHealth.Damage(bulletDamage);
                enemyHealth.isPushed = true;

                float dirX = Mathf.Sign(collision.transform.position.x - transform.position.x);
                Vector2 push = new Vector2(dirX * pushForce, jumpForce);
                enemyRb.AddForce(push, ForceMode2D.Impulse);

                enemyHealth.StartCoroutine(enemyHealth.ReleasePushFlag(bulletKnockbackDuration));
            }
            else
            {
              Destroy(gameObject, 0);
                return;  
            }
        }
        Destroy(gameObject, 0);
    }
}
