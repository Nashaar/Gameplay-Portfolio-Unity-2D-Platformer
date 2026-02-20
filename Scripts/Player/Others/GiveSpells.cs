using UnityEngine;

public class GiveSpells : MonoBehaviour
{
    public enum GiverType 
    {
        Bullet,
        Dash,
        Laser
    }
    [SerializeField] private GiverType giverType;
    [SerializeField] private UIController uiController;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(!collision.gameObject.CompareTag("Player"))
        {
            return;
        }
        GiveSpellToPlayer(giverType, collision.gameObject);     
    }

    private void GiveSpellToPlayer(GiverType giveTypeSelected, GameObject player)
    {
        switch(giveTypeSelected)
        {
            case GiverType.Bullet :
                Shooting playerShootingBullet = player.GetComponentInChildren<Shooting>();
                playerShootingBullet.bulletGet = true;
                uiController.EnableSpell(UIController.SpellType.Bullet);
                break;

            case GiverType.Dash : 
                PlayerMouvement playerMouvement = player.GetComponent<PlayerMouvement>();
                playerMouvement.dashGet = true;
                uiController.EnableSpell(UIController.SpellType.Dash);
                break;

            case GiverType.Laser :
                Shooting playerShootingLaser = player.GetComponentInChildren<Shooting>();
                playerShootingLaser.laserGet = true;
                uiController.EnableSpell(UIController.SpellType.Laser);
                break;
        }
        Destroy(gameObject, 0);
    }
}