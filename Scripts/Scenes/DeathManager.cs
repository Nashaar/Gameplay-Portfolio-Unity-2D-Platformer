using System.Collections;
using TMPro;
using UnityEngine;

public class DeathManager : MonoBehaviour
{
    #region VARIABLES
    [SerializeField] private FadeLevelController fadeLevelController;
    [SerializeField] private TextMeshProUGUI deathText;
    [SerializeField] private AmbianceManager ambianceManager;
    [SerializeField] private TextMeshProUGUI tutoText;

    private Coroutine death = null;
    #endregion

    #region UNITY FUNCTIONS
    void Start()
    {
       deathText.gameObject.SetActive(false); 
    }
    #endregion

    #region LOGIC
    public void PlayerDied()
    {
        if(death != null)
        {
            return;
        }
        death = StartCoroutine(ShowDeath());
    }

    private IEnumerator ShowDeath()
    {
        yield return new WaitForSeconds(1f);

        tutoText.enabled = false;

        deathText.gameObject.SetActive(true);
        ambianceManager.PlaySFX(9);
        StartCoroutine(fadeLevelController.Fade(0f, 0.7f, Color.black));
        StartCoroutine(fadeLevelController.FadeText(0f, 1f, deathText));

        yield return new WaitForSeconds(3f);

        SceneController.sceneInstance.startFadeColor = 0.7f;
        StartCoroutine(fadeLevelController.FadeText(1f, 0f, deathText));

        yield return new WaitForSeconds(2f);
        SceneController.sceneInstance.LoadScene(
                SceneController.sceneInstance.currentSceneName,
                SceneController.SceneLoadReason.Respawn
            );

        SceneController.sceneInstance.startFadeColor = 0f;
        deathText.gameObject.SetActive(false);
        death = null;
    }
    #endregion
}
