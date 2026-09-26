using System.Collections.Generic;
using UnityEngine;

public static class AreaDamage
{
    // Damages every enemy (and optionally the player) inside the radius once.
    public static void Apply(Vector3 center, float radius, int damage, bool damagesPlayer)
    {
        HashSet<EnemyHealth> damagedEnemies = new HashSet<EnemyHealth>();
        bool playerDamaged = false;

        foreach (Collider hitCollider in Physics.OverlapSphere(center, radius, ~0, QueryTriggerInteraction.Collide))
        {
            EnemyHealth enemyHealth = hitCollider.GetComponentInParent<EnemyHealth>();
            if (enemyHealth && damagedEnemies.Add(enemyHealth))
            {
                enemyHealth.TakeDamage(damage);
            }

            if (!damagesPlayer || playerDamaged) continue;

            PlayerHealth playerHealth = hitCollider.GetComponentInParent<PlayerHealth>();
            if (playerHealth)
            {
                playerHealth.TakeDamage(damage);
                playerDamaged = true;
            }
        }
    }
}
