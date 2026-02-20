using UnityEngine;
using UnityEngine.InputSystem;

public class ShootingModeManager : MonoBehaviour
{
    #region VARIABLES
    [SerializeField] private InputAction firingBullet, firingLaser;
    #endregion

    #region UNITY FUNCTIONS
    private void Awake()
    {
        firingBullet = InputSystem.actions.FindAction("Bullet");
        firingLaser  = InputSystem.actions.FindAction("Laser");
    }

     private void OnEnable()
    {
        firingBullet.Enable();
        firingLaser.Enable();
    }

    private void OnDisable()
    {
        firingBullet.Disable();
        firingLaser.Disable();
    }
    #endregion

    #region UPDATE FUNCTIONS
    public Shooting.FiringMode FiringModeUpdate(Shooting.FiringMode mode, bool bulletObtained, bool laserObtained)
    {
        if(firingBullet == null || firingLaser == null)
        {
            return Shooting.FiringMode.None;
        }
        bool bulletPressed = firingBullet.WasPressedThisFrame();
        bool laserPressed  = firingLaser.WasPressedThisFrame();

        if(bulletPressed && (mode == Shooting.FiringMode.None || mode == Shooting.FiringMode.Laser))
        {
            if(!bulletObtained)
            {
                return Shooting.FiringMode.None;
            }
            mode = Shooting.FiringMode.Bullet;
            bulletPressed = false;
        }
        if(laserPressed && (mode == Shooting.FiringMode.None || mode == Shooting.FiringMode.Bullet))
        {
            if(!laserObtained)
            {
                return Shooting.FiringMode.None;
            }
            mode = Shooting.FiringMode.Laser;
            laserPressed = false;
        }
        if((bulletPressed && mode == Shooting.FiringMode.Bullet) || (laserPressed && mode == Shooting.FiringMode.Laser))
        {
            mode = Shooting.FiringMode.None;
        }
        return mode;
    }
    #endregion
}