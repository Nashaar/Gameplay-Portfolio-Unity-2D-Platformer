using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckPoint : MonoBehaviour
{
    #region VARIABLES
    [SerializeField] private float checkPointX, checkPointY, triggerArea;
    [SerializeField] private int playerMaxHealth, playerLaserBullet;
    [SerializeField] private GameObject player;
    [SerializeField] private Health playerHealth;
    [SerializeField] private Shooting playerShooting;
    [SerializeField] private PlayerMouvement playerMouvement;
    [SerializeField] private Animator animator;
    [SerializeField] private UIController uiController;


    public int checkPointNumber;
    public bool isActivated;
    #endregion

    #region UNITY FUNCTIONS
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isActivated = false;
        checkPointX = transform.position.x;
        checkPointY = transform.position.y;

        if(SaveSystem.saveInstance != null && SaveSystem.saveInstance.IsCheckpointActivated(checkPointNumber, SceneManager.GetActiveScene().name))
        {
            isActivated = true;
            animator.SetTrigger("Activated");
        }
    }

    void OnEnable()
    {
        if(SaveSystem.saveInstance == null)
        {
            return;
        }
        SaveSystem.saveInstance.RegisterCheckpoint(this);
    }

    void OnDestroy()
    {
        if(SaveSystem.saveInstance == null)
        {
            return;
        }
        SaveSystem.saveInstance.UnregisterCheckpoint(this);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(isActivated)
        {
            return;
        }
        if(!col.CompareTag("Player"))
        {
            return;
        }
        Activate();
    }
    #endregion

    #region SAVE DATA
    public SaveData BuildSaveData()
    {
        return new SaveData
        {
            checkPointTriggered = true,
            checkpointNumber = checkPointNumber,
            sceneName = SceneManager.GetActiveScene().name,
            respawnPosition = transform.position,
            playerHealth = playerHealth.maxHealth,
            laserMunition = playerShooting.laserAmmo,
            dashObtained = playerMouvement.dashGet,
            laserObtained = playerShooting.laserGet,
            bulletObtained = playerShooting.bulletGet,
        };
    }

    private void Activate()
    {
        isActivated = true;
        animator.SetTrigger("Activated");
        playerShooting.laserAmmo = playerLaserBullet;
        playerHealth.maxHealth = playerMaxHealth;
        playerHealth.currentHealth = playerHealth.maxHealth;

        SaveData data = BuildSaveData();
        SaveSystem.saveInstance.SaveData(data);
        SaveSystem.saveInstance.SaveLastCheckpoint(data);
        SaveSystem.saveInstance.CheckCheckpoints(false, SceneManager.GetActiveScene().name);
        uiController.SetHealth(playerMaxHealth);
    }
    #endregion
}
