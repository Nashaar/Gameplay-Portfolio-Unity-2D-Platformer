using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TeleportFadeController : MonoBehaviour
{
    #region VARIABLEs
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.2f;
    #endregion

    #region UNITY FUNCTIONS
    private void OnDestroy()
    {
        StopAllCoroutines();
    }
    #endregion

    #region LOGIC
    public void TeleportWithFade(Transform target, Vector2 newPosition, Color color)
    {
        StartCoroutine(FadeAndTeleport(target, newPosition, color));
    }

    private IEnumerator FadeAndTeleport(Transform target, Vector2 newPosition, Color color)
    {
        yield return StartCoroutine(Fade(0f, 1f, color));
        target.position = newPosition;
        yield return StartCoroutine(Fade(1f, 0f, color));
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, Color color)
    {
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
