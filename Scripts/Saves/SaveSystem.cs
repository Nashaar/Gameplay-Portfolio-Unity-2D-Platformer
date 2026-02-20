using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class SaveSystem : MonoBehaviour
{
    #region VARIABLES
    [Header("References")]
    [SerializeField] private GameObject _player;
    public GameObject player
    {
        get => _player;
        set => _player = value;
    }
    [SerializeField] private UIController _playerUIController; 
    public UIController playerUIController
    {
        get => _playerUIController;
        set => _playerUIController = value;
    }

    public BackgroundController backgroundController;
    public bool wasDamagedThisRun;

    [Header("Internes")]
    public static SaveSystem saveInstance;
    public Vector2 level1PlayerPosition, level2PlayerPosition, level3PlayerPosition, enragePlayerPosition;
    public int level1PlayerMaxHealth, level2PlayerMaxHealth, level3PlayerMaxHealth;
    public int level1PlayerLaserAmmo, level2PlayerLaserAmmo, level3PlayerLaserAmmo;
    public bool resetCp = false;
    public enum LoadStatsMode
    {
        None,
        Default,
        LastCheckpoint,
        NextStatsMode,
        LoadLevel,
        ReloadLevel
    }
    public SaveData loadSceneData;

    private List<CheckPoint> checkpoints;
    private SaveData lastCurrentLevelCpData = null;
    public Dictionary<string, SaveData> levelStats { get; private set; } = new Dictionary<string, SaveData>();

    private string RunSavePath => Application.persistentDataPath + "/Saves/run.json";
    #endregion


    #region UNITY FUNCTIONS
    void Awake()
    {
        if(saveInstance != null)
        {
            Destroy(gameObject);
            return;
        }
        saveInstance = this;
        DontDestroyOnLoad(gameObject);
        if(SceneController.sceneInstance == null)
        {
            return;
        }
        SceneController.sceneInstance.SceneLoaded += OnSceneLoaded;

        checkpoints = new List<CheckPoint>();

        levelStats["Level1"] = new SaveData {
            respawnPosition = level1PlayerPosition, 
            playerHealth = level1PlayerMaxHealth, 
            laserMunition = level1PlayerLaserAmmo,
            sceneName = "Level1",
            dashObtained = false,
            bulletObtained = false,
            laserObtained = false
        };
        levelStats["Level2"] = new SaveData {
            respawnPosition = level2PlayerPosition, 
            playerHealth = level2PlayerMaxHealth, 
            laserMunition = level2PlayerLaserAmmo,
            sceneName = "Level2",
            dashObtained = true,
            bulletObtained = false,
            laserObtained = false
        };
        levelStats["Level3"] = new SaveData {
            respawnPosition = level3PlayerPosition, 
            playerHealth = level3PlayerMaxHealth, 
            laserMunition = level3PlayerLaserAmmo,
            sceneName = "Level3",
            dashObtained = true,
            bulletObtained = true,
            laserObtained = false
        };

        LoadRunData();
    }
    #endregion

    #region SAVE
    public void SaveData(SaveData data)
    {
        string directory = Application.persistentDataPath + "/Saves/" + data.sceneName;
        if(!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        string savePath = directory + "/checkpoint_" + data.checkpointNumber + ".json";
        string json = JsonUtility.ToJson(data,true);
        File.WriteAllText(savePath, json);
    }

    public void SaveLastCheckpoint(SaveData data)
    {
        string directory = Application.persistentDataPath + "/Saves";
        if(!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        string lastPath = directory + "/lastCheckpoint.json";
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(lastPath, json);
    }

    public void NotifyPlayerDamaged()
    {
        if(wasDamagedThisRun)
            return;

        wasDamagedThisRun = true;
        SaveRunData();
    }

    public void SaveRunData()
    {
        RunData data = new RunData
        {
            wasDamaged = wasDamagedThisRun
        };

        File.WriteAllText(RunSavePath, JsonUtility.ToJson(data, true));
    }
    #endregion

    #region LOAD
    private void OnSceneLoaded(string actualSceneName, SceneController.SceneLoadReason loadReason)
    {
        if(loadReason == SceneController.SceneLoadReason.None)
        {
            return;
        }

        if(player == null)
        {
            _player = GameObject.FindWithTag("Player");
            if(_player == null)
            {
                Debug.LogWarning("Player not found, spawn aborted.");
                return;
            }
            player = _player;
        }

        switch(loadReason)
        {
            case SceneController.SceneLoadReason.StartGame :
                wasDamagedThisRun = false;
                SaveRunData();

                resetCp = true;
                EraseCheckPointsFiles(actualSceneName);
                EraseLastCheckpoint();
                spawnPlayerOnStart();
                break;

            case SceneController.SceneLoadReason.LoadLevel :
                resetCp = true;
                EraseCheckPointsFiles(actualSceneName);
                spawnPlayerOnLoad(actualSceneName);
                break;

            case SceneController.SceneLoadReason.NextLevel :
                resetCp = true;
                EraseCheckPointsFiles(actualSceneName);
                spawnPlayerOnNextLevel();
                break;

            case SceneController.SceneLoadReason.ContinueGame :
                spawnPlayerOnContinue();
                break;

            case SceneController.SceneLoadReason.Respawn :
                spawnPlayerOnContinue();
                break;

            case SceneController.SceneLoadReason.Reload :
                spawnPlayerOnReload();
                break;

            case SceneController.SceneLoadReason.Enraged :
                spawnPlayerOnEnraged();
                break;
        }

        if(backgroundController != null)
        {
            backgroundController.Recenter();
        } 
    }
    public SaveData LoadCheckpoint(string loadSceneName, int loadCheckpointNumber)
    {
        string loadPath = Application.persistentDataPath + "/Saves/" + loadSceneName + "/checkpoint_" + loadCheckpointNumber + ".json";
        if(!File.Exists(loadPath)) 
        {
            return null;
        }
        string json = File.ReadAllText(loadPath);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public SaveData LoadLastCheckpoint()
    {
        string lastLoadPath = Application.persistentDataPath + "/Saves/lastCheckpoint.json";
        if(!File.Exists(lastLoadPath))
        {
            return null;
        }
        string json = File.ReadAllText(lastLoadPath);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public void LoadRunData()
    {
        if(!File.Exists(RunSavePath))
            return;

        RunData data = JsonUtility.FromJson<RunData>(File.ReadAllText(RunSavePath));

        wasDamagedThisRun = data.wasDamaged;
    }
    #endregion

    #region SPAWN
    public void spawnPlayerOnStart()
    {
        SaveData startData = ApplyStatsForLevel(LoadStatsMode.Default,"Level1");
        player.transform.position = startData.respawnPosition;
        player.GetComponent<Health>().maxHealth = startData.playerHealth;

        Shooting playerShooting =  player.GetComponentInChildren<Shooting>();
        PlayerMouvement playerMouvement = player.GetComponentInChildren<PlayerMouvement>();
        
        playerShooting.laserAmmo = startData.laserMunition;
        playerShooting.laserGet = startData.laserObtained;
        playerShooting.bulletGet = startData.bulletObtained;
        playerMouvement.dashGet = startData.dashObtained;

        loadSceneData = startData;
    }

    public void spawnPlayerOnLoad(string loadMenuScenename)
    {
        SaveData loadData = ApplyStatsForLevel(LoadStatsMode.LoadLevel,loadMenuScenename);
        player.transform.position = loadData.respawnPosition;
        player.GetComponent<Health>().maxHealth = loadData.playerHealth;

        Shooting playerShooting =  player.GetComponentInChildren<Shooting>();
        PlayerMouvement playerMouvement = player.GetComponentInChildren<PlayerMouvement>();
        
        playerShooting.laserAmmo = loadData.laserMunition;
        playerShooting.laserGet = loadData.laserObtained;
        playerShooting.bulletGet = loadData.bulletObtained;
        playerMouvement.dashGet = loadData.dashObtained;

        loadSceneData = loadData;
    }

    public void spawnPlayerOnNextLevel()
    {
        string nextScene = SceneController.sceneInstance.currentSceneName;

        SaveData nextData = ApplyStatsForLevel(LoadStatsMode.NextStatsMode,nextScene);
        player.transform.position = nextData.respawnPosition;
        player.GetComponent<Health>().maxHealth = nextData.playerHealth;

        Shooting playerShooting =  player.GetComponentInChildren<Shooting>();
        PlayerMouvement playerMouvement = player.GetComponentInChildren<PlayerMouvement>();
        
        playerShooting.laserAmmo = nextData.laserMunition;
        playerShooting.laserGet = nextData.laserObtained;
        playerShooting.bulletGet = nextData.bulletObtained;
        playerMouvement.dashGet = nextData.dashObtained;

        loadSceneData = nextData;
    }

    public void spawnPlayerOnContinue()
    {
        string nextScene = SceneController.sceneInstance.currentSceneName;

        SaveData continueData = ApplyStatsForLevel(LoadStatsMode.LastCheckpoint,nextScene);
        player.transform.position = continueData.respawnPosition;
        player.GetComponent<Health>().maxHealth = continueData.playerHealth;

        Shooting playerShooting =  player.GetComponentInChildren<Shooting>();
        PlayerMouvement playerMouvement = player.GetComponentInChildren<PlayerMouvement>();
        
        playerShooting.laserAmmo = continueData.laserMunition;
        playerShooting.laserGet = continueData.laserObtained;
        playerShooting.bulletGet = continueData.bulletObtained;
        playerMouvement.dashGet = continueData.dashObtained;

        loadSceneData = continueData;
    }

    public void spawnPlayerOnReload()
    {
        string reloadScene =
            !string.IsNullOrEmpty(lastCurrentLevelCpData?.sceneName)
                ? lastCurrentLevelCpData.sceneName
                : SceneController.sceneInstance.currentSceneName;

        SaveData reloadData = ApplyStatsForLevel(LoadStatsMode.ReloadLevel,reloadScene);
        player.transform.position = reloadData.respawnPosition;
        player.GetComponent<Health>().maxHealth = reloadData.playerHealth;

        Shooting playerShooting =  player.GetComponentInChildren<Shooting>();
        PlayerMouvement playerMouvement = player.GetComponentInChildren<PlayerMouvement>();
        
        playerShooting.laserAmmo = reloadData.laserMunition;
        playerShooting.laserGet = reloadData.laserObtained;
        playerShooting.bulletGet = reloadData.bulletObtained;
        playerMouvement.dashGet = reloadData.dashObtained;

        loadSceneData = reloadData;
    }

    public void spawnPlayerOnEnraged()
    {
        player.transform.position = enragePlayerPosition;
    }
    #endregion

    #region CHEKCPOINTS
    public void CheckCheckpoints(bool storeForNextLevel, string checkSceneName)
    {
        CheckPoint latest = null;

        foreach (CheckPoint cp in checkpoints)
        {
            if(!cp.isActivated)
                continue;

            if(latest == null || cp.checkPointNumber > latest.checkPointNumber)
            {
                latest = cp;
            }
        }
        if(latest == null)
        {
            lastCurrentLevelCpData = new SaveData(levelStats[checkSceneName]);
            lastCurrentLevelCpData.sceneName = checkSceneName;
            return;
            
        }
        if(!storeForNextLevel)
        {
            SaveLastCheckpoint(latest.BuildSaveData());
        }
        lastCurrentLevelCpData = latest.BuildSaveData();
    }

    public void EraseCheckPointsFiles(string eraseSceneName)
    {
        if(!resetCp)
        {
            return;
        }
        string erasePath = Application.persistentDataPath + "/Saves/" + eraseSceneName + "/";
        if(!Directory.Exists(erasePath))
        {
            resetCp = false;
            return;
        }
        foreach(var file in Directory.GetFiles(erasePath))
        {
            File.Delete(file);
        }
        resetCp = false;
    }

    public void EraseLastCheckpoint()
    {
        string erasePath = Application.persistentDataPath + "/Saves/lastCheckpoint.json";
        if(!File.Exists(erasePath))
        {
            return;
        }
        File.Delete(erasePath);
    }

    public bool IsCheckpointActivated(int checkNumber, string checkSceneName)
    {
        SaveData data = LoadCheckpoint(checkSceneName, checkNumber);
        return data != null;
    }

    public void RegisterCheckpoint(CheckPoint cp)
    {
        if(checkpoints.Contains(cp))
        {
            return;
        }
        checkpoints.Add(cp);
    }

    public void UnregisterCheckpoint(CheckPoint cp)
    {
        checkpoints.Remove(cp);
    }
    #endregion

    #region DEFINE STATS
    public SaveData ApplyStatsForLevel(LoadStatsMode mode, string statsScene)
    {
        SaveData statsData = new SaveData();

        switch(mode)
        {
            case LoadStatsMode.Default :
                statsData.respawnPosition = level1PlayerPosition;
                statsData.playerHealth = level1PlayerMaxHealth;
                statsData.laserMunition = level1PlayerLaserAmmo;
                statsData.laserObtained = false;
                statsData.bulletObtained = false;
                statsData.dashObtained = false;
                break;

            case LoadStatsMode.LastCheckpoint : 
                statsData = LoadLastCheckpoint();

                if(statsData == null || statsData.sceneName != statsScene)
                {
                    statsData = new SaveData(levelStats[statsScene]);
                }
                break;

            case LoadStatsMode.NextStatsMode :
                statsData = new SaveData(levelStats[statsScene]);

                if(lastCurrentLevelCpData != null)
                {
                    statsData.playerHealth   = lastCurrentLevelCpData.playerHealth;
                    statsData.laserMunition  = lastCurrentLevelCpData.laserMunition;
                    statsData.laserObtained  = lastCurrentLevelCpData.laserObtained;
                    statsData.bulletObtained = lastCurrentLevelCpData.bulletObtained;
                    statsData.dashObtained   = lastCurrentLevelCpData.dashObtained;
                }
                
                lastCurrentLevelCpData = null;
                break;

            case LoadStatsMode.ReloadLevel :
                statsData = new SaveData(levelStats[statsScene]);

                if(lastCurrentLevelCpData != null)
                {
                    statsData.respawnPosition = lastCurrentLevelCpData.respawnPosition;
                    statsData.playerHealth = lastCurrentLevelCpData.playerHealth;
                    statsData.laserMunition = lastCurrentLevelCpData.laserMunition;
                    statsData.laserObtained = lastCurrentLevelCpData.laserObtained;
                    statsData.bulletObtained = lastCurrentLevelCpData.bulletObtained;
                    statsData.dashObtained = lastCurrentLevelCpData.dashObtained;
                }
                
                lastCurrentLevelCpData = null;
                break;

            case LoadStatsMode.LoadLevel : 
                statsData = new SaveData(levelStats[statsScene]);
                break;
        }
        return statsData;
    }

    public void UpdateUISpells(SaveData spellData)
    {
        if(spellData.dashObtained) 
        {
            playerUIController.EnableSpell(UIController.SpellType.Dash);
        }
        if(spellData.bulletObtained) 
        {
            playerUIController.EnableSpell(UIController.SpellType.Bullet);
        }
        if(spellData.laserObtained) 
        {
            playerUIController.EnableSpell(UIController.SpellType.Laser);
            playerUIController.UpdateLaserAmmo(spellData.laserMunition);
        }
    }
    #endregion 
}