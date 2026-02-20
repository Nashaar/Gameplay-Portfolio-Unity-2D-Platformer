using UnityEngine;
using UnityEngine.InputSystem;

public class MouseAiming : MonoBehaviour
{
    #region VARIABLES
    public Camera mainCam;
    public Vector2 mousePos;
    public Vector3 mouseWorld;

    [SerializeField] private Shooting shooting;
    private float aimFreezeTimer = 0f;
    #endregion

    #region MOUSE FUNCTIONS
    public void FreezeAim(float aimFreezeDuration)
    {
        aimFreezeTimer = aimFreezeDuration;
    }
    public void MouseCalculation(bool shootingPlayerShooting)
    {
        ReadMousePosition();
        RotateMouse(shootingPlayerShooting);
    }
    private void ReadMousePosition()
    {
        if(aimFreezeTimer > 0f)
        {
            aimFreezeTimer -= Time.deltaTime;
            return;
        }

        // lire la souris en screen space
        mousePos = Mouse.current.position.ReadValue();

        // calculer la distance Z entre l'objet et la caméra (nécessaire pour ScreenToWorldPoint)
        float zDistance = transform.position.z - mainCam.transform.position.z;

        // convertir en world space
        //Cette fonction prend un Vector3 en entrée -> on crée un nouveau Vector3 avec les coordonnées x et y de la souris et la distance z calculée
        mouseWorld = mainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, zDistance));
    }

    private void RotateMouse(bool playerShooting)
    {
        // calculer rotation basée sur la position dans la GameView (world space)
        Vector3 rotation = mouseWorld - transform.position;
        float rotZ = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
        if(playerShooting)
        {
            return;
        }
        transform.rotation = Quaternion.Euler(0, 0, rotZ);
    }
    #endregion
}
