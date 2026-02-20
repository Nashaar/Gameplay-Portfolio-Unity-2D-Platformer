using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManagment : MonoBehaviour
{
    [SerializeField] private string firstLevelName;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject levelMenu;
    
    #region UNITY FUNCTIONS
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        levelMenu.SetActive(false);
    }
    #endregion

    #region MAIN MENU
    public void OnStartPressed()
    {
        SceneController.sceneInstance.LoadScene(
            firstLevelName,
            SceneController.SceneLoadReason.StartGame
        );
    }

    public void OnContinuePressed()
    {
        SceneController.sceneInstance.LoadScene(
            GetLastSave(),
            SceneController.SceneLoadReason.ContinueGame
        );
    }

    public void OnSpecificPressed()
    {
        mainMenu.SetActive(false);
        levelMenu.SetActive(true);
    }

    public void OnQuitPressed()
    {
        Application.Quit();
    }

    private string GetLastSave()
    {
        SaveData lastData = SaveSystem.saveInstance.LoadLastCheckpoint();
        if(lastData != null)
        {
            return lastData.sceneName;
        }
        else
        {
            Debug.LogWarning("No last checkpoint found, returning firstLevelName");
            return firstLevelName;
        }
    }
    #endregion

    #region LEVEL MENU
    public void LoadLevel(string levelName)
    {
        if(!Application.CanStreamedLevelBeLoaded(levelName))
        {
            levelName = firstLevelName;
        }
        SceneController.sceneInstance.LoadScene(
            levelName,
            SceneController.SceneLoadReason.LoadLevel
        );
    }

    public void GoBackToMain()
    {
        levelMenu.SetActive(false);
        mainMenu.SetActive(true);
    }
    #endregion
}
