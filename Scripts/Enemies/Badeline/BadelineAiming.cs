using UnityEngine;

public class BadelineAiming : MonoBehaviour
{    
    [SerializeField] private BadelineAttackController badelineAttackController;
    [SerializeField] private BadelineLaser badelineLaser;
    public void RotateTowardPlayer(Vector2 playerPosition)
    {
        if(badelineLaser.isShootingBeam || badelineLaser.laserLocked)
        {
            return;
        }
        Vector2 dir = playerPosition - (Vector2)transform.position;

        float rotZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        
        transform.rotation = Quaternion.Euler(0, 0, rotZ);
    }
}
