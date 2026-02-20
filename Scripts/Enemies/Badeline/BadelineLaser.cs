using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadelineLaser : MonoBehaviour
{
    #region VARIABLES
    [Header("Reference")] 
    [SerializeField] BadelineAttackController badelineAttackController;
    [SerializeField] BadelineMovement badelineMovement;
    [SerializeField] private Transform crosshairTransform, laserTransform;
    [SerializeField] private SpriteRenderer spriteRendererLaser;
    [SerializeField] private LayerMask layerHitMaks;
    [SerializeField] private AmbianceManager ambianceManager;
    [SerializeField] private List<Audio> audiosList;
    [SerializeField] private AudioSource laserSource;
    [SerializeField] private GameObject player;
    [SerializeField] private BadelineAiming aiming;
    private Audio currentClip;
    

    [Header("Internes")] 
    public float laserDamageTimer = 0f, laserTimer = 0f, aimTimer = 0f, lockTimer = 0f, fadeDuration;
    public int laserCompt = 0, laserDamage;
    public bool isShootingBeam, canAim, isPreparingLaser, laserLocked;

    [SerializeField] private float laserDamageCooldown = 0.1f;
    [SerializeField] private float maxLaserDistance;
    [SerializeField] private float laserDuration;
    private Health playerHealth = null;
    private Coroutine shootCoroutine = null;
    private Coroutine laserFadeCoroutine = null;

    public event System.Action OnLaserFired, OnLaserLock;
    #endregion

    #region UNITY FUNCTIONS
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(SceneController.sceneInstance != null)
        {
            ambianceManager = SceneController.sceneInstance.GetComponent<AmbianceManager>();
        }

        player = GameObject.FindGameObjectWithTag("Player");

        currentClip = audiosList[0];

        spriteRendererLaser.enabled = false;
        isShootingBeam = false;
        isPreparingLaser = false;
        laserLocked = false;
        canAim = true;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLaser();

        if(isShootingBeam  && laserDamageTimer > 0f)
        {
            laserDamageTimer -= Time.deltaTime;
        }
    }
    #endregion

    #region LOGIC
    public void StartLaser()
    {
        if(shootCoroutine != null)
        {
            StopCoroutine(shootCoroutine);
        }

        spriteRendererLaser.enabled = true;
        laserCompt = 0;

        playerHealth = null;

        canAim = true;
        laserLocked = false;
        isShootingBeam = false;

        shootCoroutine = StartCoroutine(FireLaser());
    }

    public void UpdateLaser()
    {
        
        Vector2 laserDirection = crosshairTransform.right;
        Vector2 startLaser = crosshairTransform.position;
        RaycastHit2D ray = Physics2D.Raycast(startLaser, laserDirection, maxLaserDistance, layerHitMaks);
        
        float laserLength = ray.collider != null ? ray.distance : maxLaserDistance;
        float scaleX = laserLength / spriteRendererLaser.size.x;

        laserTransform.localScale = new Vector3(scaleX, laserTransform.localScale.y, laserTransform.localScale.z);
        laserTransform.position = startLaser;

        if(canAim)
        {
            if(ray.collider != null && ray.collider.CompareTag("Player") && playerHealth == null)
            {
                playerHealth = ray.collider.GetComponent<Health>();
            }
        }
        if(isShootingBeam)
        {
            if(playerHealth != null && laserDamageTimer <= 0f && laserCompt < 4 && ray.collider.CompareTag("Player"))
            {
                playerHealth.Damage(laserDamage);
                laserDamageTimer = laserDamageCooldown;
                laserCompt++;
            }
        }
    }

    public void StopLaser()
    {
        transform.position -= new Vector3(0f, 1.17f, 0f);
        isShootingBeam = false;
        laserLocked = false;
        isPreparingLaser = false;
        spriteRendererLaser.enabled = false;

        if(shootCoroutine != null)
        {
            StopCoroutine(shootCoroutine);
            shootCoroutine = null;
        }
    }

    private IEnumerator FireLaser()
    {
        transform.position += new Vector3(0f, 1.17f, 0f);
        aiming.RotateTowardPlayer(player.transform.position);
        isPreparingLaser = true;
        
        FadeLaser(0);

        yield return new WaitForSeconds(aimTimer);
        
        canAim = false;
        isPreparingLaser = false;
        laserLocked = true;

        FadeLaser(1);

        OnLaserLock?.Invoke();

        yield return new WaitForSeconds(lockTimer);

        isShootingBeam = true;
        laserLocked = false;
        laserTimer = laserDuration;

        if(laserFadeCoroutine != null)
        {
            StopCoroutine(laserFadeCoroutine);
            laserFadeCoroutine = null;
        }
        laserSource.Stop(); 

        OnLaserFired?.Invoke();
        ambianceManager.PlaySFX(8);

        while (laserTimer > 0f)
        {
            laserTimer -= Time.deltaTime;
            yield return null;
        }

        StopLaser();
    }

    private void FadeLaser(int clipID)
    {
        if(laserFadeCoroutine != null)
            StopCoroutine(laserFadeCoroutine);

        laserFadeCoroutine = StartCoroutine(FadeLaserRoutine(clipID));
    }

    private IEnumerator FadeLaserRoutine(int newMusicID)
    {
        float startVolume = laserSource.volume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            laserSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        laserSource.volume = 0f;

        currentClip = audiosList[newMusicID];
        laserSource.clip = currentClip.audioClip;
        laserSource.Play();

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            laserSource.volume = Mathf.Lerp(0f, currentClip.audioVolume, t / fadeDuration);
            yield return null;
        }

        laserSource.volume = currentClip.audioVolume;
    }
    #endregion
}
