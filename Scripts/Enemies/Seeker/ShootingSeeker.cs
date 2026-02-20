using UnityEngine;

public class ShootingSeeker : MonoBehaviour
{
    [Header("Références")]
    public GameObject sekkerBullet;
    public Transform sekkerTransform;
    [SerializeField] AmbianceManager ambianceManager;

    void Start()
    {
        if(SceneController.sceneInstance != null)
        {
            ambianceManager = SceneController.sceneInstance.GetComponent<AmbianceManager>();
        }
    }

    public void RotateTowardPlayer(Vector2 playerPosition, bool ableFiring)
    {
        Vector2 dir = playerPosition - (Vector2)transform.position;

        float rotZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        
        if(!ableFiring)
        {
            transform.rotation = Quaternion.Euler(0, 0, rotZ);
        }
        else
        {
            ambianceManager.PlaySFX(7);
            Instantiate(sekkerBullet, sekkerTransform.position, Quaternion.identity);
        }
    }
}
