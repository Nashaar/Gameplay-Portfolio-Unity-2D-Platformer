using UnityEngine;

[System.Serializable]
public class SaveData
{
    public bool checkPointTriggered;
    public int checkpointNumber;
    public string sceneName;
    public Vector2 respawnPosition;
    public int playerHealth;
    public int laserMunition;
    public bool dashObtained;
    public bool bulletObtained;
    public bool laserObtained;

    public SaveData(SaveData other)
    {
        respawnPosition = other.respawnPosition;
        playerHealth = other.playerHealth;
        laserMunition = other.laserMunition;
        laserObtained = other.laserObtained;
        bulletObtained = other.bulletObtained;
        dashObtained = other.dashObtained;
        sceneName = other.sceneName;
        checkpointNumber = other.checkpointNumber;
        checkPointTriggered = other.checkPointTriggered;
    }

    public SaveData() {}
}

[System.Serializable]
public class RunData
{
    public bool wasDamaged;
}