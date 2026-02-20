using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FadeLevelController : MonoBehaviour
{
    #region VARIABLEs
    public Image fadeImage;

    public float fadeDuration = 0.2f;
    #endregion

    #region UNITY FUNCTIONS
    #endregion

    #region LOGIC
    public IEnumerator FadeText(float startAlpha, float endAlpha, TextMeshProUGUI text)
    {
        if(
            SceneController.sceneInstance.currentSceneName == "Menu" 
            && SceneController.sceneInstance.nexSceneName != "Menu")
        {
            StopAllCoroutines();
            yield break;
        }

        float timer = 0f;

        Color color = text.color;

        if(!this || text == null || color == null)
        {
            StopAllCoroutines();
            yield break;
        }

        while(timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            if(text != null)
            {
                text.color = new Color(color.r, color.g, color.b, alpha);
            }
            else
            {
                StopAllCoroutines();
                yield break;
            }
            yield return null;
        }

        text.color = new Color(color.r, color.g, color.b, endAlpha);
    }

    public IEnumerator Fade(float startAlpha, float endAlpha, Color color)
    {
        if(
            SceneController.sceneInstance.currentSceneName == "Menu" 
            && SceneController.sceneInstance.nexSceneName != "Menu")
        {
            StopAllCoroutines();
            yield break;
        }

        float timer = 0f;
        Color c = color;

        if(!this || fadeImage == null)
        {
            StopAllCoroutines();
            yield break;
        }

        while(timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            if(fadeImage != null)
            {
                fadeImage.color = new Color(c.r, c.g, c.b, alpha);
            }
            else
            {
                StopAllCoroutines();
                yield break;
            }
            yield return null;
        }

        fadeImage.color = new Color(c.r, c.g, c.b, endAlpha);
    }
    #endregion
}
