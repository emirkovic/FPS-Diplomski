using UnityEngine;

public class GrenadePickup : Pickup
{
    [SerializeField] int grenadeAmount = 2;
    [SerializeField] AudioClip pickupSFX;
    [SerializeField] [Range(0f, 1f)] float pickupSFXVolume = 1f;

    protected override void OnPickup(ActiveWeapon activeWeapon)
    {
        GrenadeThrower grenadeThrower = activeWeapon.GetComponent<GrenadeThrower>();
        if (grenadeThrower)
        {
            grenadeThrower.AddGrenades(grenadeAmount);
        }
        if (pickupSFX)
        {
            // Played from a temporary object because the pickup is destroyed straight away.
            AudioSource.PlayClipAtPoint(pickupSFX, transform.position, pickupSFXVolume);
        }
    }
}
