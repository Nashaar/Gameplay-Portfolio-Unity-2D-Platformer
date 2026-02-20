using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Health : MonoBehaviour
{
    #region VARIABLES
    public int maxHealth
    {
        get => _maxHealth;
        set => _maxHealth = value;
    }
    public SpriteRenderer sr;
    public int currentHealth = 100;
    public bool isPushed = false;
    public string tagName;
    public bool cantBeDamaged = false, cantDie = false, firstDamage = false;

    [SerializeField] private int _maxHealth = 100, heal;
    [SerializeField] private UIController uiController;
    [SerializeField] private MadelineAnimationControler madelineAnimationControler;
    private Color spriteColor;
    private float damageColorTimer;
    private bool isDamaged;
    private Coroutine healing = null;
    #endregion

    #region UNITY FUNCTIONS
    void Awake()
    {
        currentHealth = maxHealth;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.tag = tagName;
        spriteColor = sr.color;

        if(!gameObject.CompareTag("Player") && !gameObject.CompareTag("Badeline"))
        {
            return;
        }
        if(gameObject.CompareTag("Player"))
        {
            SaveSystem.saveInstance.player = gameObject;
        }
        currentHealth = maxHealth;
        uiController.SetHealth(maxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        if(madelineAnimationControler != null && madelineAnimationControler.isSleeping && healing == null)
        {
            healing = StartCoroutine(Healing());
        }

        if(!isDamaged)
        {
            return;
        }
        damageColorTimer -= Time.deltaTime;
        if(damageColorTimer <= 0f)
        {
            sr.color = spriteColor;
            isDamaged = false;
        }
    }
    #endregion

    #region LOGIC
    public void Damage(int value)
    {
        if(cantBeDamaged)
        {
            return;
        }

        if(gameObject.CompareTag("Player") && currentHealth == maxHealth && !firstDamage)
        {
            firstDamage = true;
            SaveSystem.saveInstance?.NotifyPlayerDamaged();
        }

        currentHealth -= value;
        sr.color = Color.red;
        isDamaged = true;
        damageColorTimer = 0.2f;

        if(currentHealth <= 0 && !cantDie)
        {
            Die();
        }

        if(!gameObject.CompareTag("Player") && !gameObject.CompareTag("Badeline"))
        {
            return;
        }
        uiController.HealthDamage(value);
    }

    public void Die()
    {
        if(cantDie)
        {
            return;
        }
        
        if(gameObject.CompareTag("Player"))
        {
            Shooting shooting = gameObject.GetComponentInChildren<Shooting>();
            shooting.enabled = false;

            PlayerMouvement playerMouvement = gameObject.GetComponent<PlayerMouvement>();
            playerMouvement.enabled = false;

            DeathManager deathManager = SceneController.sceneInstance.GetComponent<DeathManager>();
            deathManager.PlayerDied();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public IEnumerator ReleasePushFlag(float duration)
    {
        yield return new WaitForSeconds(duration);
        isPushed = false;
    }

    public IEnumerator Healing()
    {
        yield return new WaitForSeconds(1f);
        if(currentHealth < maxHealth)
        {
            currentHealth += heal;
            uiController.HealthHeal(heal);
        } 
        healing = null;
    }
    #endregion
}
