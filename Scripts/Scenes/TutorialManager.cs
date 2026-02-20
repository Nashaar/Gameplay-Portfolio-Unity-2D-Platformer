using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    #region VARIABLES
    [SerializeField] private TextMeshProUGUI tutorialTextUI;
    [SerializeField] private FadeLevelController fadeLevelController;
    [SerializeField] private List<String> tutorialAdvices;
    #endregion

    #region UNITY FUNCTIONS
    void Start()
    {
        if(SceneController.sceneInstance.currentSceneName == "Menu")
        {
            tutorialTextUI.color = Color.white;
        }
        tutorialTextUI.gameObject.SetActive(false);
    }
    #endregion

    #region LOGIC
    public void ShowAdvice(int adviceID)
    {
        tutorialTextUI.text = tutorialAdvices[adviceID];
        StartCoroutine(AdviceCoroutine(tutorialTextUI, adviceID));
    }

    private IEnumerator Run(TextMeshProUGUI runText)
    {
        tutorialTextUI.text = tutorialAdvices[4];
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(AdviceCoroutine(runText, 4));
    }

    private IEnumerator AdviceCoroutine(TextMeshProUGUI adviceText, int adviceIDforRun)
    {
        if(SceneController.sceneInstance.currentSceneName == "Menu")
        {
            tutorialTextUI.gameObject.SetActive(false);
            yield break;
        }
        
        yield return new WaitForSeconds (2f);
        
        fadeLevelController.fadeDuration = 3f;
        tutorialTextUI.gameObject.SetActive(true);
        StartCoroutine(fadeLevelController.FadeText(0f, 1f, adviceText));

        yield return new WaitForSeconds (3f);

        StartCoroutine(fadeLevelController.FadeText(1f, 0f, adviceText));

        yield return new WaitForSeconds (3f);
        
        tutorialTextUI.gameObject.SetActive(false);
        fadeLevelController.fadeDuration = 1f;

        if(SceneController.sceneInstance.currentSceneName == "Level1" && adviceIDforRun < 4)
        {
            StartCoroutine(Run(adviceText));
        }
    }

    public void SetTextColo(Color textColor)
    {
        tutorialTextUI.color = textColor;
    }
    #endregion
}
