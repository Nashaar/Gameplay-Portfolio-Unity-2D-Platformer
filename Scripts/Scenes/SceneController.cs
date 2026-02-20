using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    #region VARIABLES
    [SerializeField] private AmbianceManager ambianceManager;
    public static SceneController sceneInstance;
    public event Action<string, SceneLoadReason> SceneLoaded;
    public string currentSceneName 
    {
        get;
        private set;
    }
    public string nexSceneName;
    public enum SceneLoadReason
    {
        None,
        StartGame,
        ContinueGame,
        NextLevel,
        LoadLevel,
        Respawn,
        Reload,
        Enraged
    }
    public float startFadeColor;

    [SerializeField] private FadeLevelController fadeLevelController;
    private SceneLoadReason currentLoadReason;
    #endregion

    #region UNITY FUNCTIONS
    void Awake()
    {
        if(sceneInstance != null)
        {
            Destroy(gameObject);
            return;
        }
        sceneInstance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += HandleSceneLoaded;
        startFadeColor = 0f;
    }
    #endregion

    #region LOAD
    public void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        SceneLoaded?.Invoke(scene.name, currentLoadReason);
    }

    public void LoadScene(string sceneName, SceneLoadReason reason)
    {
        currentLoadReason = reason;
        nexSceneName = sceneName;
        fadeLevelController.StartCoroutine(fadeLevelController.Fade(startFadeColor, 1f, Color.black));
        StartCoroutine(LoadRoutine(sceneName));
    }

    private IEnumerator LoadRoutine(string loadScene)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(loadScene);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        fadeLevelController.StartCoroutine(fadeLevelController.Fade(1f, 0f, Color.black));
        ambianceManager.PlaySound(currentSceneName);
    }
    #endregion
}
