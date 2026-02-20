using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BadelineCinematicManager : MonoBehaviour
{
    #region REFERENCES
    [SerializeField] private FadeLevelController fadeLevelController;
    [SerializeField] private List<Sprite> endingImages;
    [SerializeField] private AmbianceManager ambianceManager;
    [SerializeField] private UIController playerUI,badelineUI;
    [SerializeField] private TextMeshProUGUI advice;
    #endregion

    #region UNITY FUNCTIONS
    void Start()
    {
       if(SceneController.sceneInstance != null)
        {
            fadeLevelController = SceneController.sceneInstance.GetComponent<FadeLevelController>();
            ambianceManager = SceneController.sceneInstance.GetComponent<AmbianceManager>();
        } 
    }
    #endregion

    #region LOCK ENTITIES
    private void LockBossfight(
        Health playerHealthLock,
        Health badelineHealthLock,
        PlayerMouvement playerMouvementLock,
        Rigidbody2D playerRBLock,
        GameObject playerLock,
        TeleportFadeController teleportFadeControllerLock,
        BadelineAttackController badelineAttackControllerLock,
        BadelineMovement badelineMovementLock
    )
    {
        EscapeManager escapeManager = SceneController.sceneInstance.GetComponent<EscapeManager>();
        escapeManager.cancel = true;

        playerUI = playerLock.GetComponentInChildren<UIController>();
        badelineUI = badelineMovementLock.GetComponentInChildren<UIController>();
        playerUI.SetUIVisible(false);
        badelineUI.SetUIVisible(false);

        BadelineLaser badelineLaserLock = badelineHealthLock.GetComponentInChildren<BadelineLaser>();
        badelineLaserLock.gameObject.SetActive(false);

        playerHealthLock.cantDie = true;
        badelineHealthLock.cantDie = true;

        playerHealthLock.cantBeDamaged = true;
        badelineHealthLock.cantBeDamaged = true;

        Shooting playerShootingLock = playerLock.GetComponentInChildren<Shooting>();

        playerShootingLock.enabled = false;
        playerShootingLock.gameObject.SetActive(false);
        playerMouvementLock.enabled = false;

        playerRBLock.linearVelocity = Vector2.zero;
        playerRBLock.gravityScale = 0f;
        playerRBLock.angularVelocity = 0f;
        playerRBLock.bodyType = RigidbodyType2D.Kinematic;

        badelineAttackControllerLock.StopAllCoroutines();
        badelineAttackControllerLock.isPreparingAttack = false;
        badelineAttackControllerLock.currentPreparedAttack = null;
        badelineAttackControllerLock.enabled = false;
        badelineMovementLock.enabled = false;

        if(teleportFadeControllerLock != null)
        {
            ambianceManager.PlayAttackSound(6);

            teleportFadeControllerLock.TeleportWithFade(
                playerLock.transform,
                playerLock.transform.position,
                Color.white
            );
        }

        playerLock.transform.position = new Vector3(426,31, playerLock.transform.position.z);
        playerRBLock.linearVelocity = Vector2.zero;
        transform.position = new Vector3(430, 31, transform.position.z);
    }

    private void DestroyAllBullets()
    {
        BadelineBullet[] badelineBullets = FindObjectsByType<BadelineBullet>(FindObjectsSortMode.None);
        foreach (BadelineBullet bullet in badelineBullets)
        {
            Destroy(bullet.gameObject);
        }

        BulletScript[] playerBullets = FindObjectsByType<BulletScript>(FindObjectsSortMode.None);
        foreach (BulletScript bullet in playerBullets)
        {
            Destroy(bullet.gameObject);
        }
    }
    #endregion

    #region RETURN
    public void PlayReturnBegin(
        Health playerHealthReturn,
        Health badelineHealthReturn,
        BadelineMovement badelineMovementReturn,
        PlayerMouvement playerMouvementReturn,
        BadelineAttackController badelineAttackControllerReturn,
        DeathManager deathManagerReturn,
        Rigidbody2D playerRBReturn,
        GameObject playerReturn,
        BadelineAnimationController badelineAnimationControllerReturn,
        TeleportFadeController teleportFadeControllerReturn
    )
    {
        if( playerHealthReturn.currentHealth > 10
            || badelineMovementReturn.isInCinematic
            || badelineAttackControllerReturn.cinematicCoroutine != null)
        {
            return;
        }

        StopAllCoroutines();

        deathManagerReturn.StopAllCoroutines();
        fadeLevelController.StopAllCoroutines();
        
        badelineAttackControllerReturn.enterCinematic = true;

        playerHealthReturn.currentHealth = Mathf.Max(playerHealthReturn.currentHealth, 10);
        
        LockBossfight(
            playerHealthReturn,
            badelineHealthReturn,
            playerMouvementReturn,
            playerRBReturn,
            playerReturn,
            teleportFadeControllerReturn,
            badelineAttackControllerReturn,
            badelineMovementReturn
        );

        badelineAnimationControllerReturn.PlayTelegraph("TeleportPlayer");

        badelineAttackControllerReturn.cinematicCoroutine = StartCoroutine(SendPlayerToBeginning(
            badelineAnimationControllerReturn,
            teleportFadeControllerReturn,
            playerReturn
        ));
    }

    private IEnumerator SendPlayerToBeginning(
        BadelineAnimationController badelineAnimationControllerCoroutine,
        TeleportFadeController teleportFadeControllerCoroutine,
        GameObject playerCoroutine
    )
    {
        ambianceManager.PlaySFX(14);
        yield return new WaitForSeconds(2f);

        badelineAnimationControllerCoroutine.FinalStateMachine(BadelineAnimationController.BadelineState.Teleport);

        yield return new WaitForSeconds(1f);

        if(teleportFadeControllerCoroutine != null)
        {
            teleportFadeControllerCoroutine.TeleportWithFade(
                playerCoroutine.transform,
                playerCoroutine.transform.position,
                Color.white
            );
        }

        SceneController.sceneInstance.LoadScene(
            "Level1",
            SceneController.SceneLoadReason.StartGame
        );
    }
    #endregion

    #region BOSS DEFEAT
    public void PlayerDefeatedBoss(
        Health playerHealthBoss,
        Health badelineHealthBoss,
        PlayerMouvement playerMouvementBoss,
        BadelineMovement badelineMovementBoss,
        Rigidbody2D playerRBBoss,
        Rigidbody2D badelineRBBoss,
        GameObject playerBoss,
        BadelineAttackController badelineAttackControllerBoss,
        DeathManager deathManagerBoss,
        TeleportFadeController teleportFadeControllerBoss,
        BadelineAnimationController badelineAnimationControllerBoss
    )
    {
        if( badelineHealthBoss.currentHealth > 10
            || badelineMovementBoss.isInCinematic
            || badelineAttackControllerBoss.cinematicCoroutine != null)
        {
            return;
        }

        StopAllCoroutines();

        deathManagerBoss.StopAllCoroutines();
        fadeLevelController.StopAllCoroutines();
        
        badelineAttackControllerBoss.enterCinematic = true;

        MadelineAnimationControler madelineAnimationControler = playerBoss.GetComponentInChildren<MadelineAnimationControler>();

        badelineHealthBoss.currentHealth = Mathf.Max(badelineHealthBoss.currentHealth, 10);

        LockBossfight(
            playerHealthBoss,
            badelineHealthBoss,
            playerMouvementBoss,
            playerRBBoss,
            playerBoss,
            teleportFadeControllerBoss,
            badelineAttackControllerBoss,
            badelineMovementBoss
        );

        badelineMovementBoss.allowCinematicMovement = true;

        if(SaveSystem.saveInstance.wasDamagedThisRun)
        {
            badelineAttackControllerBoss.cinematicCoroutine = StartCoroutine(BadEnding(
                badelineAnimationControllerBoss,
                madelineAnimationControler,
                teleportFadeControllerBoss,
                playerBoss,
                badelineRBBoss,
                playerRBBoss
            ));
        }
        else
        {
            badelineAttackControllerBoss.cinematicCoroutine = StartCoroutine(PerfectEnding(
                badelineAnimationControllerBoss,
                madelineAnimationControler,
                badelineRBBoss,
                playerRBBoss,
                playerBoss
            ));
        }
    }
    #endregion

    #region GOOD ENDING
    private IEnumerator PerfectEnding(
        BadelineAnimationController badelineAnimationControllerGood,
        MadelineAnimationControler madelineAnimationControlerGood,
        Rigidbody2D badelineRBGood,
        Rigidbody2D madelineGood,
        GameObject playerGood
    )
    {
        ambianceManager.FadeMusic(13);

        badelineRBGood.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(2f);

        badelineAnimationControllerGood.FinalStateMachine(BadelineAnimationController.BadelineState.Die);

        playerGood.transform.position = new Vector3(426,31, playerGood.transform.position.z);
        madelineGood.linearVelocity = Vector2.zero;

        badelineRBGood.transform. position = new Vector3(435, 31, transform.position.z);

        badelineRBGood.linearVelocity = Vector2.zero;

        badelineRBGood.linearVelocity =  new Vector2(0f, 5f);
    
        yield return new WaitForSeconds(5f);


        madelineGood.gravityScale = 1.5f;

        ambianceManager.PlaySFX(16);

        yield return new WaitForSeconds(1f);

        StartCoroutine(madelineAnimationControlerGood.InstantSit());


        yield return new WaitForSeconds(5f);

        
        fadeLevelController.fadeImage.sprite = endingImages[1];
        StartCoroutine(fadeLevelController.Fade(0f, 1f, Color.white));

        TutorialManager tutorialManager = SceneController.sceneInstance.GetComponent<TutorialManager>();
        advice = tutorialManager.GetComponentInChildren<TextMeshProUGUI>();
        advice.enabled = true;
        tutorialManager.SetTextColo(Color.white);

        yield return new WaitForSeconds(10f);

        tutorialManager.ShowAdvice(6);

        SceneController.sceneInstance.startFadeColor = 1f;
        SceneController.sceneInstance.LoadScene(
            "Menu",
            SceneController.SceneLoadReason.None
        );
    }
    #endregion

    #region BAD ENDING
    private IEnumerator BadEnding(
        BadelineAnimationController badelineAnimationControllerBad,
        MadelineAnimationControler madelineAnimationControlerBad,
        TeleportFadeController teleportFadeControllerBad,
        GameObject playerBad,
        Rigidbody2D badelineRBBad,
        Rigidbody2D playerRBBad
    )
    {
        ambianceManager.FadeMusic(12);

        badelineRBBad.linearVelocity = Vector2.zero;

        playerRBBad.gravityScale = 0f;

        badelineAnimationControllerBad.FinalStateMachine(BadelineAnimationController.BadelineState.Die);

        yield return new WaitForSeconds(3f);

        playerBad.transform.position = new Vector3(426,31, playerBad.transform.position.z);
        playerRBBad.linearVelocity = Vector2.zero;

        badelineRBBad.transform. position = new Vector3(435, 31, transform.position.z);

        badelineRBBad.linearVelocity = Vector2.zero;

        badelineRBBad.linearVelocity =  new Vector2(0f, 5f);

        yield return new WaitForSeconds(5f);

        ambianceManager.PlayAttackSound(6);
        
        teleportFadeControllerBad.TeleportWithFade(
            playerBad.transform, 
            new Vector2(playerBad.transform.position.x, 
            playerBad.transform.position.y + 10), Color.white
        );

        ambianceManager.PlaySFX(15);

        yield return new WaitForSeconds(2f);
    
        madelineAnimationControlerBad.MadelineAnimationStateMachine(MadelineAnimationControler.PlayerState.Monster);

        yield return new WaitForSeconds(5f);
        
        fadeLevelController.fadeImage.sprite = endingImages[0];
        StartCoroutine(fadeLevelController.Fade(0f, 1f, Color.white));

        TutorialManager tutorialManager = SceneController.sceneInstance.GetComponent<TutorialManager>();
        advice = tutorialManager.GetComponentInChildren<TextMeshProUGUI>();
        advice.enabled = true;
        tutorialManager.SetTextColo(Color.red);

        yield return new WaitForSeconds(10f);

        tutorialManager.ShowAdvice(5);

        SceneController.sceneInstance.startFadeColor = 1f;
        SceneController.sceneInstance.LoadScene(
            "Menu",
            SceneController.SceneLoadReason.None
        );
    }
    #endregion
}
