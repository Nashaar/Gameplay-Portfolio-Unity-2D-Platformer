using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BadelineCinematicAttack : MonoBehaviour
{
    #region VARIABLES
    [Header("Références")]
    [SerializeField] private BadelineAttackController attackController;
    [SerializeField] private BadelineMovement movementController;
    [SerializeField] private BadelineLaser laserController;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Health playerHealth;
    [SerializeField] private PlayerMouvement playerMovement;
    [SerializeField] private Rigidbody2D playerRB;
    [SerializeField] private MadelineAnimationControler madelineAnimationControler;
    [SerializeField] private BadelineAnimationController badelineAnimationController;
    [SerializeField] private TeleportFadeController teleportFadeController;
    [SerializeField] private BadelineAiming badelineAiming;
    [SerializeField] private AmbianceManager ambianceManager;

    [Header("Paramètres")]
    public static BadelineCinematicAttack cinematicInstance;

    [SerializeField] private int backCounterLimit = 3;
    private int backCounter = 0;
    private string scene;
    private bool cinematicPlayed = false;
    private bool isLoadingScene = false;
    #endregion

    #region UNITY FUNCTIONS
    void Awake()
    {
        if(cinematicInstance != null)
        {
            Destroy(gameObject);
            return;
        }
        cinematicInstance = this;
        DontDestroyOnLoad(gameObject);
        SceneController.sceneInstance.SceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        if(SceneController.sceneInstance != null)
        {
            ambianceManager = SceneController.sceneInstance.GetComponent<AmbianceManager>();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        BossEnraged();
        ReturnMenu();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if(!collider.gameObject.CompareTag("Player"))
        {
            return;
        }
        
        if(isLoadingScene)
        {
            return;
        }

        backCounter++;

        if(scene != "Menu")
        {
            playerTransform.position = SaveSystem.saveInstance.levelStats[scene].respawnPosition;
        }

        if(teleportFadeController != null)
        {
            ambianceManager.PlayAttackSound(10);
            ambianceManager.PlayAttackSound(6);
            teleportFadeController.TeleportWithFade(
                playerTransform,
                playerTransform.position,
                Color.white
            );
        }

        madelineAnimationControler.MadelineAnimationStateMachine(
            MadelineAnimationControler.PlayerState.Idle
        );
    }

    private void OnDestroy()
    {
        if(SceneController.sceneInstance != null)
        {
            SceneController.sceneInstance.SceneLoaded -= OnSceneLoaded;
        }
    }
    #endregion

    #region LOGIC
    private void BossEnraged()
    {
        if(isLoadingScene || backCounter < backCounterLimit)
        {
            return;
        }

        isLoadingScene = true;

        ambianceManager.StopFSX();

        ambianceManager.PlayAttackSound(6);

        SceneController.sceneInstance.LoadScene(
                "Level3",
                SceneController.SceneLoadReason.Enraged
        );
    }

    private void OnSceneLoaded(string actualSceneName, SceneController.SceneLoadReason loadReason)
    {
        scene = actualSceneName;
        if(scene == "Menu")
        {
            Destroy(gameObject);
        }
        else if(movementController != null
            && ambianceManager != null
            && SceneController.sceneInstance != null
            && scene == "Level3"
            && movementController.isInCinematic
            )
        {
            ambianceManager.PlayAttackSound(18);
        }

        GameObject player = GameObject.FindWithTag("Player");
        if(player != null)
        {
            playerTransform = player.transform;
            playerHealth = player.GetComponent<Health>();
            playerMovement = player.GetComponent<PlayerMouvement>();
            playerRB = player.GetComponent<Rigidbody2D>();
            madelineAnimationControler = player.GetComponentInChildren<MadelineAnimationControler>();
        }

        
        GameObject badeline = GameObject.FindWithTag("Badeline");
        if(badeline != null)
        {
            attackController = badeline.GetComponent<BadelineAttackController>();
            movementController = badeline.GetComponent<BadelineMovement>();
            laserController = badeline.GetComponentInChildren<BadelineLaser>();
            badelineAiming = badeline.GetComponentInChildren<BadelineAiming>();
            badelineAnimationController = badeline.GetComponentInChildren<BadelineAnimationController>();
            badeline.transform.position = new Vector3(430, 31, badeline.transform.position.z);
        }

        if(loadReason != SceneController.SceneLoadReason.Enraged || cinematicPlayed)
        {
            return;
        }

        cinematicPlayed = true;

        PlayLaserCinematic();
    }

    public void PlayLaserCinematic()
    {
        StartCoroutine(LaserCinematicRoutine());
    }

    private IEnumerator LaserCinematicRoutine()
    {
        movementController.isInCinematic = true;
        playerMovement.enabled = false;
        playerRB.linearVelocity = Vector2.zero;
        playerRB.gravityScale = 0;

        yield return new WaitUntil(() => movementController.badelineAwaken);

        badelineAnimationController.PlayTelegraph("BossLaser");

        badelineAiming.RotateTowardPlayer(playerTransform.position);

        yield return new WaitForSeconds(1f);

        playerHealth.cantDie = true;

        badelineAnimationController.FinalStateMachine(BadelineAnimationController.BadelineState.Laser);

        laserController.laserDamage = playerHealth.currentHealth - 10;

        laserController.StartLaser();
    }
    
    private void ReturnMenu()
    {
        if( playerHealth == null 
            || movementController == null
            || playerHealth.currentHealth > 10 
            || !movementController.isInCinematic)
        {
            return;
        }
        
        laserController.StopLaser();

        SceneController.sceneInstance.SceneLoaded -= OnSceneLoaded;
        
        SceneController.sceneInstance.LoadScene(
            "Menu",
            SceneController.SceneLoadReason.None
        );
        movementController.isInCinematic = true;

        Destroy(gameObject);
    }
    #endregion
}
