using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "WeaponSO", menuName = "ScriptableObjects/WeaponSO")]
public class WeaponSO : ScriptableObject
{
    public GameObject weaponPrefab;
    public float fireRate = 0.5f;
    public GameObject hitVFX;
    public bool IsAutomatic = false;
    public int weaponSlot = 1;
    public bool canZoom = false;
    public float zoomFOV = 10f;
    public int magazineSize = 12;

    [Header("Damage by distance")]
    [Tooltip("Hits closer than this many metres count as close range.")]
    public float closeRange = 10f;
    [Tooltip("Hits closer than this many metres (but past close range) count as mid range. Anything further is long range.")]
    public float midRange = 25f;
    [FormerlySerializedAs("damageAmount")]
    public int closeDamage = 1;
    public int midDamage = 0;
    public int longDamage = 0;
    [Tooltip("Damage at any range while zoomed in. Only used when Can Zoom is on.")]
    public int zoomedDamage = 0;

    [Header("Projectile")]
    [Tooltip("Leave empty for hitscan weapons. When set, the weapon fires this prefab instead (e.g. a rocket), and the prefab deals its own damage.")]
    public GameObject projectilePrefab;

    public int GetDamage(float distance, bool isZoomed)
    {
        if (canZoom && isZoomed) return zoomedDamage;
        if (distance <= closeRange) return closeDamage;
        if (distance <= midRange) return midDamage;
        return longDamage;
    }
}
