using UnityEngine;
using StarterAssets;

public class Weapon : MonoBehaviour
{
    [SerializeField] private int damageAmount = 1;
    StarterAssetsInputs StarterAssetsInputs;
    void Awake()
    {
        StarterAssetsInputs = GetComponentInParent<StarterAssetsInputs>();
    }      
    void Update()
    {
        HandleShoot();
    }

    void HandleShoot()
    {
        if (!StarterAssetsInputs.shoot) return;

        RaycastHit hit;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, Mathf.Infinity))
        {
            EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
            enemyHealth?.TakeDamage(damageAmount);

            StarterAssetsInputs.ShootInput(false);
        }
    }
}
