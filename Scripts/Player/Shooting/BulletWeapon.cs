using UnityEngine;
using UnityEngine.InputSystem;

public class BulletWeapon : MonoBehaviour
{
    #region VARIABLES
    [SerializeField] private Shooting shooting;
    [SerializeField] private PlayerMouvement playerMouvement;
    [SerializeField] private Transform crosshairTransform, laserTransform;
    [SerializeField] private UIController uiController;
    [SerializeField] private PlayerMouvement playerMovement;
    [SerializeField] private MadelineAnimationControler madelineAnimationControler;
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform bulletTransform;
    [SerializeField] private AmbianceManager ambianceManager;

    [SerializeField] private  float timeBetweenFiringBullet = 0.3f;
    private float bulletTimer;
    #endregion

    #region UNITY FUNCTIONS
    void Start()
    {
        if(SceneController.sceneInstance != null)
        {
            ambianceManager = SceneController.sceneInstance.GetComponent<AmbianceManager>();
        }
    }
    #endregion

    #region BULLET FUNCTIONS
    public void BulletInstance(Shooting.FiringMode mode, bool bulletObtained, InputAction shootInput)
    {
        if(!(mode == Shooting.FiringMode.Bullet) || !bulletObtained)
        {
            return;
        }

        uiController.UpdateSpell(shooting.canFireBullet, UIController.SpellType.Bullet);

        if(!shooting.canFireBullet)
        {
            bulletTimer += Time.deltaTime;
            if(bulletTimer > timeBetweenFiringBullet)
            {
                shooting.canFireBullet = true;
                bulletTimer = 0;
            }
            return;
        }

        MadelineAnimationControler.PlayerState state = madelineAnimationControler.currentState; 
        if(state == MadelineAnimationControler.PlayerState.GetUp 
            || state == MadelineAnimationControler.PlayerState.Sleep 
            || state == MadelineAnimationControler.PlayerState.Sit 
            || playerMovement.isDashing 
            || playerMovement.isGrabing) 
        { 
            return;
        }

        bool shootBulletPressed = shootInput.WasPressedThisFrame();
        if(!shootBulletPressed)
        {
            return;
        }
        shooting.canFireBullet = false;
        ambianceManager.PlayAttackSound(7);
        Instantiate(bullet, bulletTransform.position, transform.rotation);
    }
    #endregion
}