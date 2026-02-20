using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class EscapeManager : MonoBehaviour
{
    #region VARIABLES
    [SerializeField] private GameObject escapeMenu,textMeshPro;

    public InputAction escapeAction;
    public bool cancel = false;
    #endregion

    #region UNITY FUNCTIONS
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        escapeMenu.SetActive(false);
        textMeshPro.SetActive(false);
        escapeAction = InputSystem.actions.FindAction("Menu");
    }

    // Update is called once per frame
    void Update()
    {
        if(cancel)
        {
            return;
        }
        EscapeListen(); 
    }
    #endregion

    #region MENU FUNCTIONS
    private void EscapeListen()
    {
        if(SceneController.sceneInstance.currentSceneName == "Menu")
        {
            return;
        }

        bool escapePressed = escapeAction.WasPressedThisFrame();
        if(!escapePressed)
        {
            return;
        }

        bool isPaused = Time.timeScale == 0;

        Time.timeScale = isPaused ? 1 : 0;
        
        textMeshPro.SetActive(!isPaused);
        escapeMenu.SetActive(!isPaused);
    }

    public void OnResumePressed()
    {
        Time.timeScale = 1;
        escapeMenu.SetActive(false);
        textMeshPro.SetActive(false);
    }

    public void OnReloadPressed()
    {
        Time.timeScale = 1;
        escapeMenu.SetActive(false);
        textMeshPro.SetActive(false);

        SaveSystem.saveInstance.CheckCheckpoints(true, SceneManager.GetActiveScene().name);

        SceneController.sceneInstance.LoadScene(
            SceneManager.GetActiveScene().name,
            SceneController.SceneLoadReason.Reload
        );
    }

    public void OnMenuPressed()
    {
        Time.timeScale = 1;
        escapeMenu.SetActive(false);
        textMeshPro.SetActive(false);
        SceneController.sceneInstance.LoadScene(
            "Menu",
            SceneController.SceneLoadReason.None
        );
    }

    public void OnQuitPressed()
    {
        Application.Quit();
    }
    #endregion
}
