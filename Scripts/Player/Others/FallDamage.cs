using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerMouvement))]
public class FallDamage : MonoBehaviour
{

    
    public  float minVelocity = -15f;
    public int damageAmount = 20;
    public float velocity;

    public Rigidbody2D rb;
    public Health health;
    public PlayerMouvement playerMouvement;

    public bool alreadyGrounded = false;
    


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        playerMouvement = GetComponent<PlayerMouvement>();
        alreadyGrounded = false;
    }

    void FixedUpdate()
    {
        FallUpdate();
    }

    private void FallUpdate()
    {
        if(playerMouvement.iswallJumping)
        {
            return;
        }
        velocity = rb.linearVelocityY;

        if(playerMouvement.isGrounded == false)
        {
            alreadyGrounded = false;
        }

        if(playerMouvement.isGrounded && velocity < minVelocity && alreadyGrounded == false) 
        {
            health.Damage(damageAmount);
            alreadyGrounded = true;
        }
    }
}
