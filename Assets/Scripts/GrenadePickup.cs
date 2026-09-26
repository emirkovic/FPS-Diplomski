using UnityEngine;

public class GrenadePickup : Pickup
{
    [SerializeField] int grenadeAmount = 2;

    protected override void OnPickup(ActiveWeapon activeWeapon)
    {
        GrenadeThrower grenadeThrower = activeWeapon.GetComponent<GrenadeThrower>();
        if (grenadeThrower)
        {
            grenadeThrower.AddGrenades(grenadeAmount);
        }
    }
}
