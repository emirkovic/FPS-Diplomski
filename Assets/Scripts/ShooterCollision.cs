using UnityEngine;

public static class ShooterCollision
{
    // Stops a spawned projectile or grenade from hitting the player who fired it.
    public static void Ignore(GameObject projectile, Component shooter)
    {
        CharacterController player = shooter.GetComponentInParent<CharacterController>();
        if (!player) return;

        Collider[] playerColliders = player.GetComponentsInChildren<Collider>();
        foreach (Collider projectileCollider in projectile.GetComponentsInChildren<Collider>())
        {
            foreach (Collider playerCollider in playerColliders)
            {
                Physics.IgnoreCollision(projectileCollider, playerCollider);
            }
        }
    }
}
