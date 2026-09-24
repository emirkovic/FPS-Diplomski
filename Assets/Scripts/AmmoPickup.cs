using UnityEngine;

public class AmmoPickup : Pickup
{
    [SerializeField] WeaponSO weaponSO;
    [SerializeField] int ammoAmount = 20;

    protected override void OnPickup(ActiveWeapon activeWeapon)
    {
        activeWeapon.AddAmmo(weaponSO, ammoAmount);
    }
}
