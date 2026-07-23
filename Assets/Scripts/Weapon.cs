using UnityEngine;
using StarterAssets;

public class Weapon : MonoBehaviour
{
    [SerializeField] ParticleSystem muzzleFlash;
    [SerializeField] Animator gunAnimator;
    [SerializeField] private int damageAmount = 1;
    StarterAssetsInputs starterAssetsInputs;
    const string SHOOT_ANIMATION = "Shoot";
    void Awake()
    {
        starterAssetsInputs = GetComponentInParent<StarterAssetsInputs>();
    }      
    void Update()
    {
        HandleShoot();
    }

    void HandleShoot()
    {
        if (!starterAssetsInputs.shoot) return;

        muzzleFlash.Play();
        gunAnimator.Play(SHOOT_ANIMATION, 0, 0f);
        starterAssetsInputs.ShootInput(false);

        RaycastHit hit;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, Mathf.Infinity))
        {
            EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
            enemyHealth?.TakeDamage(damageAmount);
        }
    }
}
