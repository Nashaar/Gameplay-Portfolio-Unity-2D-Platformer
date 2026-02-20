using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class LaserWeapon : MonoBehaviour
{
    #region VARIABLES
    [Header("Reference")] 
    [SerializeField] private Shooting shooting;
    [SerializeField] private Transform crosshairTransform, laserTransform;
    [SerializeField] private PlayerMouvement playerMovement;
    [SerializeField] private UIController uiController;
    [SerializeField] private MadelineAnimationControler madelineAnimationControler;
    [SerializeField] private CrosshairAnimationControler crosshairAnimationControler;
    [SerializeField] private float laserDamageCooldown = 0.1f, laserDamageTimer = 0f;

    [Header("Internes")] 
    public LayerMask layerHitMaks;
    public int laserCompt = 0;
    public bool isShootingBeam;
    public bool activateLaser;
    public bool regenCrosshair;
    #endregion

    #region UNITY FUNCTIONS
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        uiController.UpdateLaserAmmo(shooting.laserAmmo);
        activateLaser = false;
        isShootingBeam = false;
        regenCrosshair = false;
    }
    #endregion

    #region LASER CALCULATION
    public void LaserRaycast(float maxLaserDistance, Shooting.FiringMode mode, float baseLaserSpriteLenght)
    {
        if(!(mode == Shooting.FiringMode.Laser) || !shooting.laserGet)
        {
            return;
        }

        
        Vector2 laserDirection = transform.right;
        Vector2 startLaser = crosshairTransform.position;
        RaycastHit2D ray = Physics2D.Raycast(startLaser, laserDirection, maxLaserDistance, layerHitMaks);

        float laserLenght;
        laserDamageTimer -= Time.fixedDeltaTime;

        if(ray.collider != null)
        {
            if(ray.collider.CompareTag("Player") || ray.collider.CompareTag("Checkpoint"))
            {
                return;
            }

            laserLenght = ray.distance;
            
            shooting.onReach = true;
            if(ray.collider.CompareTag("Enemy") || ray.collider.CompareTag("Badeline"))
            {
                LaserDamage(madelineAnimationControler.isShootingBeam, ray.collider, mode);
            }
        }
        else
        {
            laserLenght = maxLaserDistance;
            shooting.onReach = false;
        }
        float scaleX = laserLenght / baseLaserSpriteLenght;

        laserTransform.localScale = new Vector3(scaleX, laserTransform.localScale.y, laserTransform.localScale.z);
        
        laserTransform.position = startLaser;
    }

    public void LaserShoot(Shooting.FiringMode mode, InputAction shootInput)
    {
        if(!(mode == Shooting.FiringMode.Laser))
        {
            return;
        }
        if(shootInput == null)
        {
            return;
        }

        var state = madelineAnimationControler.currentState; 

        if(state == MadelineAnimationControler.PlayerState.GetUp 
            || state == MadelineAnimationControler.PlayerState.Sleep 
            || state == MadelineAnimationControler.PlayerState.Sit 
            || playerMovement.isDashing 
            || playerMovement.isGrabing) 
        { 
            return;
        }

        if(shooting.laserAmmo <= 0)
        {
            shooting.laserAmmo = 0;
            return;
        }

        bool shootLaserPressed = shootInput.WasPressedThisFrame();
        bool shootLaserReleased = shootInput.WasReleasedThisFrame();

        if(shootLaserPressed && !shooting.isAlreadyShooting)
        {
            transform.position += new Vector3(0, 2, 0);
            shooting.isCharging = true;
            shooting.wantsToShoot = false;
            uiController.UpdateSpell(false, UIController.SpellType.Laser);
        }
        if(shootLaserReleased && shooting.isCharging)
        {
            shooting.isCharging = false;
            shooting.wantsToShoot = true;
            shooting.laserAmmo--;

            
            uiController.UpdateSpell(true, UIController.SpellType.Laser);
            uiController.UpdateLaserAmmo(shooting.laserAmmo);
        }
    }

    private void LaserDamage(bool madelineShooting, Collider2D raycastRb, Shooting.FiringMode damageMode)
    {
        if(!(damageMode == Shooting.FiringMode.Laser))
        {
            return;
        }
        if(!madelineShooting)
        {
            return;
        }

        if(laserDamageTimer > 0)
        {
            return;
        }

        laserDamageTimer = laserDamageCooldown;
        Health objectHealth = raycastRb.GetComponent<Health>();
        if(objectHealth.currentHealth > 0 && laserCompt < 4)
        {
            objectHealth.Damage(25);
            laserCompt++;
        }   
        else
        {
            return;
        }
    }
    #endregion
}
