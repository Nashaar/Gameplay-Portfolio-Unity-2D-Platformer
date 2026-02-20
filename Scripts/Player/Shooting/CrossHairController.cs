using UnityEngine;

public class CrossHairController : MonoBehaviour
{
    #region VARIABLES
    public SpriteRenderer spriteRendererCrosshair;
    public SpriteRenderer spriteRendererLaser;
    #endregion

    #region UNITY FUNCTIONS
    void Start()
    {
        spriteRendererLaser.enabled = false;
        spriteRendererCrosshair.enabled = true;
    }
    #endregion

    #region UPDATE FUNCTIONS
    public float CrossHairUpdate(Shooting.FiringMode mode, PlayerMouvement playerMouvement, bool wantsToShoot, bool isCharging)
    {
        if((!playerMouvement.canMove && !wantsToShoot && !isCharging) || (mode == Shooting.FiringMode.None))
        {
            spriteRendererCrosshair.enabled = false;
            spriteRendererLaser.enabled = false;
        }
        else if(isCharging || wantsToShoot || playerMouvement.isDashing)
        {
            spriteRendererCrosshair.enabled = false;
        }
        else
        {
            spriteRendererCrosshair.enabled = true;
        }
        
        float spriteWidth = spriteRendererLaser.sprite != null ? spriteRendererLaser.sprite.bounds.size.x : 1f;

        return spriteWidth;
    }
    #endregion
}