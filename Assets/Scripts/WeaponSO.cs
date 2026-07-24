using UnityEngine;
[CreateAssetMenu(fileName = "WeaponSO", menuName = "ScriptableObjects/WeaponSO")]
public class WeaponSO : ScriptableObject
{
    public GameObject weaponPrefab;
    public int damageAmount = 1;
    public float fireRate = 0.5f;
    public GameObject hitVFX;
    public bool IsAutomatic = false;

}
