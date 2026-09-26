using Unity.Cinemachine;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] ParticleSystem muzzleFlash;
    [SerializeField] LayerMask interactableLayerMask;

    [Header("Projectile weapons only")]
    [SerializeField] Transform projectileSpawnPoint;
    [SerializeField] GameObject muzzleVFX;
    [SerializeField] AudioSource fireAudio;

    const float MAX_AIM_DISTANCE = 1000f;
    const float MUZZLE_VFX_LIFETIME = 2f;

    CinemachineImpulseSource impulseSource;

    void Awake()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void Shoot(WeaponSO weaponSO, bool isZoomed)
    {
        if (muzzleFlash) muzzleFlash.Play();
        if (impulseSource) impulseSource.GenerateImpulse();
        if (fireAudio) fireAudio.Play();

        if (weaponSO.projectilePrefab)
        {
            FireProjectile(weaponSO);
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, Mathf.Infinity, interactableLayerMask, QueryTriggerInteraction.Ignore))
        {
            Instantiate(weaponSO.hitVFX, hit.point, Quaternion.identity);

            int damage = weaponSO.GetDamage(hit.distance, isZoomed);
            if (damage <= 0) return;

            EnemyHealth enemyHealth = hit.collider.GetComponentInParent<EnemyHealth>();
            enemyHealth?.TakeDamage(damage);
        }
    }

    void FireProjectile(WeaponSO weaponSO)
    {
        Transform cameraTransform = Camera.main.transform;
        Vector3 aimPoint = cameraTransform.position + cameraTransform.forward * MAX_AIM_DISTANCE;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, MAX_AIM_DISTANCE, interactableLayerMask, QueryTriggerInteraction.Ignore))
        {
            aimPoint = hit.point;
        }

        Vector3 spawnPosition = projectileSpawnPoint ? projectileSpawnPoint.position : cameraTransform.position + cameraTransform.forward;
        Quaternion spawnRotation = Quaternion.LookRotation(aimPoint - spawnPosition);

        GameObject projectile = Instantiate(weaponSO.projectilePrefab, spawnPosition, spawnRotation);
        ShooterCollision.Ignore(projectile, this);

        if (muzzleVFX)
        {
            Destroy(Instantiate(muzzleVFX, spawnPosition, spawnRotation), MUZZLE_VFX_LIFETIME);
        }
    }
}
