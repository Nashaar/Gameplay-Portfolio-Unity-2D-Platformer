using UnityEngine;

public class BadelineTeleporter : MonoBehaviour
{
    #region VARIABLES
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private TeleportFadeController teleportFadeController;

    public Vector2 GetRandomGroundPosition(Transform target, Vector2 center, float range, int maxAttempts = 10)
    {
        CircleCollider2D col = target.GetComponent<CircleCollider2D>();
        float radius = col.radius * Mathf.Max(target.localScale.x, target.localScale.y);

        for (int i = 0; i < maxAttempts; i++)
        {
            //part de (0,0)
            Vector2 randomOffset = Random.insideUnitCircle * range;
            //centre la valeur autour du centre
            Vector2 randomPos = center + randomOffset;

            //calcul la normale de la position random sous forme de vecteur
            Vector2 direction = (randomPos - center).normalized;
            //calcul sa longueur
            float distance = (randomPos - center).magnitude;

            //cast du raycast vers la position
            RaycastHit2D hit = Physics2D.Raycast(center, direction, distance, groundLayer.value);

            //si le sol ou une plateforme est touchée
            if(hit.collider != null)
            {
                //calcul la position finale (au-dessus de la surface)
                Vector2 finalPos = hit.point + Vector2.up * (radius + 0.1f);

                // vérification si on est pas dans le sol
                Collider2D overlap = Physics2D.OverlapCircle(finalPos, radius, groundLayer.value);
                if(overlap == null)
                {
                    return finalPos;
                }
            }

        }

        return target.position;
    }

    public void Teleport(Transform target, Vector2 center, float range, Color color)
    {
        Vector2 newPos = GetRandomGroundPosition(target, center, range);
        teleportFadeController.TeleportWithFade(target, newPos, color);
    }
    #endregion
}
