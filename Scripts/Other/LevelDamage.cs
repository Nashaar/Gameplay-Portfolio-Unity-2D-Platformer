using UnityEngine;

public class LevelDamage : MonoBehaviour
{
    [SerializeField] Health playerHealth;
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if(!collision.gameObject.CompareTag("Player"))
        {
            return;
        }
        playerHealth.Die();       
    }
}
