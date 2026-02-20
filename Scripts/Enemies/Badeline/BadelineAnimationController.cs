using UnityEngine;

public class BadelineAnimationController : MonoBehaviour
{
    #region VARIABLES
    [Header("Références")]
    [SerializeField ] private BadelineAttackController badelineAttackController;
    [SerializeField] private BadelineLaser badelineLaser;
    [SerializeField] private BadelinePhaseController badelinePhaseController;
    [SerializeField] private BadelineMovement badelineMovement;
    [SerializeField] private Animator badelineAnimator, BadelineLaserAnimator;

    public enum BadelineState
    {
        Awekening,
        Idle ,
        PrepareBullet,
        PrepareLaser,
        PrepareTeleportPlayer,
        PrepareTeleport,
        Bullet,
        TeleportPlayer,
        Teleport,
        Laser,
        LaserShoot,
        LaserLock,
        ChangePhase,
        Die
    }

    private enum LaserState
    {
        None,
        LaserAim,
        LaserLock,
        LaserShoot
    }
    private BadelineState currentState = BadelineState.Idle;
    private LaserState currentLaserState = LaserState.None;

    public event System.Action TriggerAttack;
    #endregion

    #region UNITY FUNCTIONS

    // Update is called once per frame
    void Update()
    {
        ResolveState();
        ResolveLaserState();
    }

    private void OnEnable()
    {
        badelineLaser.OnLaserLock += HandleLaserLock;
        badelineLaser.OnLaserFired += HandleLaserFire;
        badelinePhaseController.OnPhaseChanged += HandlePhaseChanged;
        badelineMovement.Awekening += () => FinalStateMachine(BadelineState.Awekening);
        badelineMovement.OnAwakeningFinished += () => FinalStateMachine(BadelineState.Idle);
    }

    private void OnDisable()
    {
        badelineLaser.OnLaserLock -= HandleLaserLock;
        badelineLaser.OnLaserFired -= HandleLaserFire;
        badelinePhaseController.OnPhaseChanged -= HandlePhaseChanged;
        badelineMovement.Awekening -= HandleAwekening;
    }
    #endregion

    #region LOGIC
    private void HandleAwekening()
    {
        FinalStateMachine(BadelineState.Awekening);
    }
    private void HandleLaserLock()
    {
        FinalStateMachine(BadelineState.LaserLock);
    }

    private void HandleLaserFire()
    {
        FinalStateMachine(BadelineState.LaserShoot);
    }

    private void HandlePhaseChanged(BadelinePhaseController.Phase newPhase)
    {
        FinalStateMachine(BadelineState.ChangePhase);
    }

    public void PlayTelegraph(string attackName)
    {
        BadelineState telegrapheState;
        switch (attackName)
        {
            case "Bullet":
                telegrapheState = BadelineState.PrepareBullet;
                break;

            case "TeleportPlayer":
                telegrapheState = BadelineState.PrepareTeleportPlayer;
                break;

            case "TeleportBoss":
                telegrapheState = BadelineState.PrepareTeleport;
                break;

            case "BossLaser":
                telegrapheState = BadelineState.PrepareLaser;
                break;
            
            default :
                telegrapheState = BadelineState.Idle;
                break;
        }
        FinalStateMachine(telegrapheState);
    }

    private void ResolveState()
    {
        if(!badelineAttackController.isPreparingAttack || badelineAttackController.currentPreparedAttack == null)
        {
            return;
        }

        if(badelineAttackController. currentPreparedAttack.attackTriggered)
        {
            BadelineState resolveState;
            switch(badelineAttackController.currentPreparedAttack.name)
            {
                case "Bullet":
                    resolveState = BadelineState.Bullet;
                    break;

                case "TeleportPlayer":
                    resolveState = BadelineState.TeleportPlayer;
                    break;

                case "TeleportBoss":
                    resolveState = BadelineState.Teleport;
                    break;

                case "BossLaser":
                    resolveState = BadelineState.Laser;
                    break;

                default : 
                    resolveState = BadelineState.Idle;
                    break;
            }

            badelineAttackController.currentPreparedAttack.attackTriggered = false;
            FinalStateMachine(resolveState);
        }
    }

    public void FinalStateMachine(BadelineState newState)
    {
        if( newState == currentState)
        {
            return;
        }

        switch(newState)
        {
            case BadelineState.PrepareBullet :
                badelineAnimator.SetTrigger("PrepareBullet");
                break;

            case BadelineState.Bullet :
                badelineAnimator.SetTrigger("Bullet");
                TriggerAttack?.Invoke();
                break;

            case BadelineState.PrepareTeleportPlayer :
                badelineAnimator.SetTrigger("PrepareTeleportPlayer");
                break;

            case BadelineState.TeleportPlayer :
                badelineAnimator.SetTrigger("TeleportPlayer");
                TriggerAttack?.Invoke();
                break;

            case BadelineState.PrepareTeleport :
                badelineAnimator.SetTrigger("PrepareTeleport");
                break;

            case BadelineState.Teleport :
                badelineAnimator.SetTrigger("Teleport");
                TriggerAttack?.Invoke();
                break;

            case BadelineState.PrepareLaser :
                badelineAnimator.SetTrigger("PrepareLaser");
                break;

            case BadelineState.Laser : 
                badelineAnimator.SetTrigger("Laser");
                TriggerAttack?.Invoke();
                break;

            case BadelineState.LaserShoot : 
                badelineAnimator.SetTrigger("LaserShoot");
                break;

            case BadelineState.LaserLock : 
                badelineAnimator.SetTrigger("LaserLock");
                break;

            case BadelineState.ChangePhase : 
                badelineAnimator.SetTrigger("ChangePhase");
                break;

            case BadelineState.Awekening : 
                badelineAnimator.SetTrigger("Awekening");
                break;

            case BadelineState.Die : 
                badelineAnimator.SetTrigger("BadelineDeath");
                break;

            case BadelineState.Idle :
                break;

            default :
                break;            
        }
        currentState = newState;
    }
    
    private void ResolveLaserState()
    {
        LaserState resolveLaserState;

        if(badelineLaser.canAim)
        {
            resolveLaserState = LaserState.LaserAim;
        }
        else if(badelineLaser.laserLocked)
        {
            resolveLaserState = LaserState.LaserLock;
        }
        else if(badelineLaser.isShootingBeam)
        {
            resolveLaserState = LaserState.LaserShoot;
        }
        else
        {
            resolveLaserState = LaserState.None;
        }

        LaserFinalStateMachine(resolveLaserState);
    }
    
    private void LaserFinalStateMachine(LaserState newLaserState)
    {
        if( newLaserState == currentLaserState)
        {
            return;
        }

        switch(newLaserState)
        {
            case LaserState.None :
                break;

            case LaserState.LaserAim :
                BadelineLaserAnimator.SetTrigger("LaserAim");
                break;

            case LaserState.LaserLock :
                BadelineLaserAnimator.SetTrigger("LaserLock");
                break;

            case LaserState.LaserShoot :
                BadelineLaserAnimator.SetTrigger("LaserShoot");
                break;

            default :
                break;            
        }
        currentLaserState = newLaserState;
    }
    #endregion
}
