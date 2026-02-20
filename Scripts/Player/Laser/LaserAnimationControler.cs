using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
public class LaserAnimationControler : MonoBehaviour 
{
    #region VARIABLES
    [Header("Références")] 
    [SerializeField] private AmbianceManager ambianceManager;
    [SerializeField] private List<Audio> audiosList;
    [SerializeField] private AudioSource laserSource;
    private Audio currentClip;

    public Animator animator; 
    public Shooting shooting; 
    public MadelineAnimationControler madelineAnimationControler; 

    [Header("Internes")]
    private bool isShootingLaser = false, playSound = false; 
    private Coroutine laserFadeCoroutine = null;
    private bool laserJustActivated = false;

    public enum LaserState 
    {
        Charge = 1,
        Lock = 2,
        Shoot = 3,
        None = 4
    }
    public LaserState currentState = LaserState.None;
    public float fadeDuration;

    const string STATE_PARAM = "State"; 
    #endregion
    
    #region UNITY FUNCTIONS
    // Start is called once before the first execution of Update after the MonoBehaviour is created 
    void Start()
    { 
        if(SceneController.sceneInstance != null)
        {
            ambianceManager = SceneController.sceneInstance.GetComponent<AmbianceManager>();
        }
        
        currentState = LaserState.None;
        
        isShootingLaser = false;
        currentClip = null;
    }
    
    //Update is called once per frame 
    void Update() 
    {
        ActiveLaser(); 

        if(laserJustActivated)
        {
            laserJustActivated = false;

            if(shooting.onReach)
                LaserAnimationStateMachine(LaserState.Lock);
            else
                LaserAnimationStateMachine(LaserState.Charge);
        }

        ShootingLaser(); 
        ReachLaser(); 
    } 

    private void OnDisable()
    {
        if(laserFadeCoroutine != null)
        {
            StopCoroutine(laserFadeCoroutine);
            laserFadeCoroutine = null;
        }

        if(laserSource != null)
        {
            laserSource.Stop();
        }
    }
    #endregion
    
    #region LOGIC
    private void ActiveLaser()
    { 
        if(madelineAnimationControler.activateLaser) 
        {
            shooting.spriteRendererLaser.enabled = true;

            if(currentState == LaserState.None)
            {
                laserJustActivated = true;
            }
        } 
        else
        {
            shooting.spriteRendererLaser.enabled = false;
            if(currentState != LaserState.None)
            {
                animator.SetTrigger("EndLaser");
                LaserAnimationStateMachine(LaserState.None);
            }

            if(laserSource != null)
            {
                laserSource.Stop();
            }  
        }
    } 
    
    private void ShootingLaser() 
    {
        if(madelineAnimationControler.isShootingBeam && !isShootingLaser)
        { 
            isShootingLaser = true; 
            LaserAnimationStateMachine(LaserState.Shoot); 
        } 
        else if(!madelineAnimationControler.isShootingBeam && isShootingLaser)
        { 
            isShootingLaser = false;
            playSound = false;
            LaserAnimationStateMachine(LaserState.None);
        } 
    }
    
    private void ReachLaser()
    { 
        if(!(currentState == LaserState.Shoot))
        { 
            if(shooting.onReach)
            { 
                LaserAnimationStateMachine(LaserState.Lock);
            } 
            else 
            { 
                LaserAnimationStateMachine(LaserState.Charge);
            } 
        } 
    } 
    
    private void LaserAnimationStateMachine(LaserState newState) 
    {
        if(newState == currentState && currentState != LaserState.None) 
        { 
            return;
        } 
        
        currentState = newState; 
        EnterState(currentState); 
        animator.SetInteger(STATE_PARAM, (int)currentState);
    } 
    
    private void EnterState(LaserState state)
    { 
        switch(state) 
        {
            case LaserState.None:
                playSound = false;

                if(laserFadeCoroutine != null)
                {
                    StopCoroutine(laserFadeCoroutine);
                    laserFadeCoroutine = null;
                }

                if(laserSource != null)
                {
                    laserSource.Stop();
                    laserSource.volume = 0f;
                }
                break;

            case LaserState.Charge:
                playSound = false;
                FadeLaser(0);
                break;

            case LaserState.Lock:
                playSound = false;
                FadeLaser(1);
                break;

            case LaserState.Shoot : 
                animator.SetTrigger("Shoot"); 
                if(!playSound)
                {
                    playSound = true;
                    if(laserFadeCoroutine != null)
                    {
                        StopCoroutine(laserFadeCoroutine);
                        laserFadeCoroutine = null;
                    }

                    laserSource.loop = false;
                    laserSource.Stop();

                    ambianceManager.PlaySFX(8);
                }
                break;
        }
    }

    private void FadeLaser(int clipID)
    {
        if(laserSource == null)
        {
            Debug.LogError("LaserAnimationController : laserSource est NULL");
            return;
        }

        if(audiosList == null || clipID < 0 || clipID >= audiosList.Count)
        {
            Debug.LogError($"LaserAnimationController : audiosList invalide ou clipID {clipID}");
            return;
        }

        if(audiosList[clipID] == null || audiosList[clipID].audioClip == null)
        {
            Debug.LogError($"LaserAnimationController : Audio ou AudioClip NULL pour l’ID {clipID}");
            return;
        }

        if(currentClip == audiosList[clipID] && laserSource.isPlaying)
            return;

        if(laserFadeCoroutine != null)
            StopCoroutine(laserFadeCoroutine);

        laserFadeCoroutine = StartCoroutine(FadeLaserRoutine(clipID));
    }

    private IEnumerator FadeLaserRoutine(int newMusicID)
    {
        if(laserSource == null || audiosList == null || newMusicID < 0 || newMusicID >= audiosList.Count)
        {
            yield break;
        }

        Audio clip = audiosList[newMusicID];
        
        if(clip == null || clip.audioClip == null)
        {
            yield break;
        }

        float targetVolume = clip.audioVolume;

        float startVolume = laserSource.volume;
        float t = 0f;

        while(t < fadeDuration)
        {
            if(laserSource == null)
            {
                yield break;
            }

            t += Time.deltaTime;
            laserSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        laserSource.volume = 0f;

        if(laserSource == null)
        {
            yield break;
        }

        laserSource.Stop();
        laserSource.clip = clip.audioClip;
        laserSource.loop = true;
        laserSource.Play();

        t = 0f;
        while(t < fadeDuration)
        {
            if(laserSource == null)
            {
                yield break;
            }

            t += Time.deltaTime;
            laserSource.volume = Mathf.Lerp(0f, targetVolume, t / fadeDuration);
            yield return null;
        }
        laserSource.volume = targetVolume;
    }
    #endregion
}