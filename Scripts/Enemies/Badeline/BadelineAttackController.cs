using System.Collections;
using UnityEngine;

public class BadelineAttackController : MonoBehaviour
{
    #region VARIABLES
    [Header("References")]
    [SerializeField] private BadelinePhaseController phaseController;
    [SerializeField] private GameObject player;
    [SerializeField] private Health playerHealth, badelineHealth;
    [SerializeField] private PlayerMouvement playerMouvement;
    [SerializeField] private Rigidbody2D playerRB, badelineRB;
    [SerializeField] private BadelineAiming aiming;
    [SerializeField] private BadelineTeleporter badelineTeleporter;
    [SerializeField] private GameObject badelineBullet;
    [SerializeField] private Transform badelineCrosshairTransform;
    [SerializeField] private BadelineMovement badelineMovement;
    [SerializeField] private BadelineLaser badelineLaser;
    [SerializeField] private BadelineAnimationController badelineAnimationController;
    [SerializeField] private TeleportFadeController teleportFadeController;
    [SerializeField] private AmbianceManager ambianceManager;
    [SerializeField] private DeathManager deathManager;
    [SerializeField] private BadelineCinematicManager badelineCinematicManager;

    [Header("Internes")] 
    public bool activateLaser;
    public bool isPreparingAttack;
    public BossAttack currentPreparedAttack;

    private float telegraphTimer, bulletSpeed;
    private Vector3 bulletSize;

    [Header("Attack Cooldowns")]
    public Cooldown attackWindow = new Cooldown { cooldownTime = 1.5f };

    [Header("Phase Attacks")]
    public BossAttack[] phase1Attacks;
    public BossAttack[] phase2Attacks;
    public BossAttack[] phase3Attacks;
    public BossAttack[] currentPhaseAttacks;

    [Header("Phase Attacks")]
    public BossStats phase1Stats;
    public BossStats phase2Stats;
    public BossStats phase3Stats;
    public bool enterCinematic = false;
    public Coroutine cinematicCoroutine = null;

    private int lastAttackIndex = -1;
    private bool mustAlternate = false;
    #endregion

    #region UNITY FUNCTIONS
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        phase1Attacks = new BossAttack[]
        {
            new BossAttack { 
                name = "Bullet", 
                cooldown=new Cooldown{cooldownTime = 2f}, 
                action = ShootBullet,
                useAlternate = false
            }
        };

        phase2Attacks = new BossAttack[]
        {
            new BossAttack { 
                name = "Bullet", 
                cooldown=new Cooldown{cooldownTime = 1f}, 
                action = ShootBullet,
                useAlternate = false
            },
            new BossAttack { 
                name = "TeleportPlayer", 
                cooldown=new Cooldown{cooldownTime = 2f}, 
                action =()=> TeleportPlayer(15f),
                useAlternate = true
            }
        };

        phase3Attacks = new BossAttack[]
        {
            new BossAttack { 
                name = "Bullet", 
                cooldown=new Cooldown{cooldownTime = 0.5f}, 
                action = ShootBullet,
                useAlternate = false
            },
            new BossAttack { 
                name = "TeleportPlayer", 
                cooldown=new Cooldown{cooldownTime = 1f}, 
                action =()=> TeleportPlayer(15f),
                useAlternate = true
            },
            new BossAttack { 
                name = "TeleportBoss", 
                cooldown=new Cooldown{cooldownTime = 1.7f}, 
                action =()=> TeleportBoss(15f),
                useAlternate = false
            },
            new BossAttack { 
                name = "BossLaser", 
                cooldown=new Cooldown{cooldownTime = 2f}, 
                action = ShootLaser,
                useAlternate = false
            }
        };

        phase1Stats = new BossStats
        {
            bossSpeed = 10f,
            bulletSpeed = 15f,
            attackCooldown = 1.5f,
            bulletSize = new Vector3(2.5f, 2.5f, 2.5f),
            telegraphDuration = 1f
        };

        phase2Stats = new BossStats
        {
            bossSpeed = 15f,
            bulletSpeed = 20f,
            attackCooldown = 1f,
            bulletSize = new Vector3(5f, 5f, 5f),
            telegraphDuration = 0.8f
        };

        phase3Stats = new BossStats
        {
            bossSpeed = 20f,
            bulletSpeed = 25f,
            attackCooldown = 0.8f,
            bulletSize = new Vector3(7.5f, 7.5f, 7.5f),
            telegraphDuration = 0.6f
        };
    }
    void Start()
    {
        if(SceneController.sceneInstance != null)
        {
            ambianceManager = SceneController.sceneInstance.GetComponent<AmbianceManager>();
            deathManager = SceneController.sceneInstance.GetComponent<DeathManager>();
        }

        player = GameObject.FindGameObjectWithTag("Player");
        playerHealth = player.GetComponent<Health>();
        playerMouvement = player.GetComponent<PlayerMouvement>();
        playerRB = player.GetComponent<Rigidbody2D>();
        currentPhaseAttacks = phase1Attacks;
        ApplyStats(phase1Stats);
        activateLaser = false;
        badelineHealth.cantDie = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(badelineMovement.isInCinematic)
        {
            StopAllAttacks();
            return;
        }

        if(player == null || !badelineMovement.badelineAwaken)
        {
            return;
        }
        
        HandleCinematics();

        aiming.RotateTowardPlayer(player.transform.position);

        if(enterCinematic)
        {
            return;
        }
        
        if(!enterCinematic)
        {
            HandleTimers();
        }

        if(!badelineMovement.canAttack)
        {
            return;
        }

        if(!enterCinematic)
        {
            HandleAttacks();
        }
    }

    private void OnEnable()
    {
        phaseController.OnPhaseChanged += HandlePhaseChanged;
        badelineAnimationController.TriggerAttack += ExecuteCurrentAttack;
    }

    private void OnDisable()
    {
        phaseController.OnPhaseChanged -= HandlePhaseChanged;
        badelineAnimationController.TriggerAttack -= ExecuteCurrentAttack;
    }
    #endregion

    #region LOGIC
    public void StopAllAttacks()
    {
        badelineMovement.isInCinematic = true;
        enterCinematic = true;
        badelineMovement.canAttack = false;
        isPreparingAttack = false;
        currentPreparedAttack = null;
        StopAllCoroutines();
        isPreparingAttack = false;
        currentPreparedAttack = null;
    }

    private void HandleCinematics()
    {
        ReturnBegin();
        BossDefeated();
    }
    private void ReturnBegin()
    {
        if( playerHealth.currentHealth > 10
            || badelineMovement.isInCinematic
            || cinematicCoroutine != null)
        {
            return;
        }

        badelineCinematicManager.PlayReturnBegin(
            playerHealth,
            badelineHealth,
            badelineMovement,
            playerMouvement,
            this,
            deathManager,
            playerRB,
            player,
            badelineAnimationController,
            teleportFadeController
        );
    }

    private void BossDefeated()
    {
        if( badelineHealth.currentHealth > 10
            || badelineMovement.isInCinematic
            || cinematicCoroutine != null)
        {
            return;
        }

        badelineCinematicManager.PlayerDefeatedBoss(
            playerHealth,
            badelineHealth,
            playerMouvement,
            badelineMovement,
            playerRB,
            badelineRB,
            player,
            this,
            deathManager,
            teleportFadeController,
            badelineAnimationController
        );
    }

    private void ApplyStats(BossStats stats)
    {
        badelineMovement.speed = stats.bossSpeed;
        bulletSpeed = stats.bulletSpeed;
        attackWindow.cooldownTime = stats.attackCooldown;
        bulletSize = stats.bulletSize;
        telegraphTimer = stats.telegraphDuration;
    }
    private void ExecuteCurrentAttack()
    {
        if(badelineMovement.isInCinematic)
        {
            return;
        }
        
        if(currentPreparedAttack == null)
        {
            return;
        }

        currentPreparedAttack.action?.Invoke();
        isPreparingAttack = false;
    }
    private void  HandleAttacks()
    {
        if(!attackWindow.isReady || isPreparingAttack || enterCinematic)
        {
            return;
        }

        BossAttack selectedAttack = ChooseAttack();

        if(selectedAttack == null)
        {
            return;               
        }
        
        StartCoroutine(PrepareAndExecuteAttack(selectedAttack));
    }

    private void HandleTimers()
    {
        if(!attackWindow.isReady)
        {
            attackWindow.UpdateTimer();
            return;
        }

        foreach(BossAttack attack in currentPhaseAttacks)
        {
            attack.cooldown.UpdateTimer();
        }
    }

    private BossAttack ChooseAttack()
    {
        for(int i = 0; i < currentPhaseAttacks.Length; i++)
        {
            int index = (lastAttackIndex + 1 + i) % currentPhaseAttacks.Length;
            BossAttack attack = currentPhaseAttacks[index];

            if(!attack.cooldown.isReady)
            {
                continue;
            }

            if(mustAlternate && attack.useAlternate)
            {
                continue;
            }

            lastAttackIndex = index;

            mustAlternate = attack.useAlternate;

            return attack;
        }
        return null;
    }

    private void HandlePhaseChanged(BadelinePhaseController.Phase newPhase)
    {
        lastAttackIndex = -1;
        mustAlternate = false;
        
        switch (newPhase)
        {
            case BadelinePhaseController.Phase.Phase1:
                currentPhaseAttacks = phase1Attacks;
                ApplyStats(phase1Stats);
                break;

            case BadelinePhaseController.Phase.Phase2:
                currentPhaseAttacks = phase2Attacks;
                ApplyStats(phase2Stats);
                ambianceManager.FadeMusic(4);
                break;

            case BadelinePhaseController.Phase.Phase3:
                currentPhaseAttacks = phase3Attacks;
                ApplyStats(phase3Stats);
                ambianceManager.FadeMusic(5);
                break;
        }
    }

    private IEnumerator PrepareAndExecuteAttack(BossAttack attack)
    {
        isPreparingAttack = true;
        currentPreparedAttack = attack;

        badelineAnimationController.PlayTelegraph(attack.name);

        yield return new WaitForSeconds(telegraphTimer);

        attack.attackTriggered = true;
        
        attack.cooldown.Trigger();   
        
        attackWindow.Trigger();
    }
    #endregion

    #region ATTACKS FUNCTIONS
    private void ShootBullet()
    {
        ambianceManager.PlayAttackSound(7);
        GameObject bulletInstance = Instantiate(badelineBullet, badelineCrosshairTransform.position, badelineCrosshairTransform.rotation);
        BadelineBullet badelineBulletStats = bulletInstance.GetComponent<BadelineBullet>();

        badelineBulletStats.boss = gameObject;

        Collider2D bulletCollider = bulletInstance.GetComponent<Collider2D>();
        Collider2D bossCollider = badelineMovement.GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(bulletCollider, bossCollider, true);

        badelineBulletStats.force = bulletSpeed;
        bulletInstance.transform.localScale = bulletSize;
    }

    private void ShootLaser()
    {
        badelineLaser.StartLaser();
    }

    private void TeleportPlayer(float range)
    {
        ambianceManager.PlayAttackSound(6);
        badelineTeleporter.Teleport(player.transform, player.transform.position, range, Color.white);
    }

    private void TeleportBoss(float range)
    {
        ambianceManager.PlayAttackSound(6);
        badelineTeleporter.Teleport(transform, transform.position, range, Color.purple);
    }
    #endregion
}
