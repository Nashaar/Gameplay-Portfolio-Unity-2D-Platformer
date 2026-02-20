using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbianceManager : MonoBehaviour
{
    #region VARIABLES
    [SerializeField] private AudioSource musicSource, sfxSource;
    [SerializeField] private List<Audio> audiosList;
    [SerializeField] private float fadeDuration = 1f;

    private Coroutine fadeCoroutine;
    private Audio currentClip;
    #endregion

    #region UNITY FUNCTIONS
    void Start()
    {
        FadeMusic(11);
    } 
    #endregion

    #region LOGIC
    public void PlaySound(string sceneName)
    {
        int idClip = GetMusicID(sceneName);
        if(idClip < 0) 
        {
            return;
        }

        FadeMusic(idClip);
    }
    public void PlaySFX(int soundID)
    {
        if(soundID < 0 || soundID >= audiosList.Count)
        {
            return;
        }

        Audio clip = audiosList[soundID];
        sfxSource.PlayOneShot(clip.audioClip, clip.audioVolume);
    }


    public void PlayAttackSound(int soundID)
    {
        currentClip = audiosList[soundID];
        sfxSource.PlayOneShot(currentClip.audioClip, currentClip.audioVolume);
    }

    public void FadeMusic(int newMusicID)
    {
        if(fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeMusicRoutine(newMusicID));
    }

    private IEnumerator FadeMusicRoutine(int newMusicID)
    {
        float startVolume = musicSource.volume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        musicSource.volume = 0f;

        currentClip = audiosList[newMusicID];
        musicSource.clip = currentClip.audioClip;
        musicSource.Play();

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, currentClip.audioVolume, t / fadeDuration);
            yield return null;
        }

        musicSource.volume = currentClip.audioVolume;
    }

    public int GetMusicID(string sceneIDName)
    {
        int id = -1;
        switch(sceneIDName)
        {
            case "Menu" :
                id = 11;
                break;

            case "Level1" :
                id = 0;
                break;

            case "Level2" :
                id = 1;
                break;

            case "Level3" :
                id = 2;
                break;

            default : 
                id = -1;
                break;
        }
        return id;       
    }

    public void StopFSX()
    {
        sfxSource.Stop();
    }
    #endregion
}
