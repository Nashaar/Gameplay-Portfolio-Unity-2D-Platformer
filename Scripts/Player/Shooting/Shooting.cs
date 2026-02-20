using UnityEngine;
using UnityEngine.InputSystem;

public class Shooting : MonoBehaviour
{
    #region VARIABLES
    [Header("Références")]
    public PlayerMouvement playerMovement;
    [SerializeField] private ShootingModeManager shootingModeManager;
    [SerializeField] private CrossHairController crossHairController;
    [SerializeField] private MouseAiming mouseAiming;
    [SerializeField] private LaserWeapon laserWeapon;
    [SerializeField] private BulletWeapon bulletWeapon;
    [SerializeField] private CrosshairAnimationControler crosshairAnim;

    public SpriteRenderer spriteRendererCrosshair, spriteRendererLaser;

    [Header("Internes")]
    public bool isCharging = false;
    public bool wantsToShoot = false;
    public bool isAlreadyShooting = false;
    public bool onReach = false;
    public bool canFireBullet = true;
    public bool laserGet;
    public bool bulletGet;
    public bool isShootingBeam;
    public float maxLaserDistance = 12, baseLaserSpriteLenght;
    [SerializeField] private int _laserAmmo = 3;
    public int laserAmmo
    {
        get => _laserAmmo;
        set => _laserAmmo = value;
    }
    public FiringMode firingModeChoosed = FiringMode.None;
    public InputAction shootAction;
    public enum FiringMode
    {
        None,
        Bullet,
        Laser
    }
    #endregion

    #region UNITY FUNCTIONS
    private void Awake()
    {
        shootAction   = InputSystem.actions.FindAction("Attack");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isCharging = false;
        wantsToShoot = false;
        isAlreadyShooting = false;
        onReach = false;
        maxLaserDistance = 12;
    }

    // Update is called once per frame
    void Update()
    {
        if(shootAction == null)
        {
            return;
        }
        if(Time.timeScale == 0)
        {
            return;
        }
            
        firingModeChoosed = shootingModeManager.FiringModeUpdate(firingModeChoosed, bulletGet, laserGet);
        baseLaserSpriteLenght = crossHairController.CrossHairUpdate(firingModeChoosed, playerMovement, wantsToShoot, isCharging);
        mouseAiming.MouseCalculation(wantsToShoot);
        laserWeapon.LaserShoot(firingModeChoosed, shootAction);
        bulletWeapon.BulletInstance(firingModeChoosed, bulletGet, shootAction);
    }

    private void FixedUpdate()
    {
        laserWeapon.LaserRaycast(maxLaserDistance, firingModeChoosed, baseLaserSpriteLenght);
    }

    private void OnEnable()
    {
        shootAction.Enable();
    }

    private void OnDisable()
    {
        shootAction.Disable();
    }
    #endregion
}