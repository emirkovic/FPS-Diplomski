using UnityEngine;

public class Grenade : MonoBehaviour
{
    [SerializeField] float fuseTime = 2.5f;
    [SerializeField] float explosionRadius = 5f;
    [SerializeField] int explosionDamage = 8;
    [SerializeField] bool damagesPlayer = true;
    [SerializeField] GameObject explosionVFX;

    const float EXPLOSION_VFX_LIFETIME = 5f;

    void Start()
    {
        Invoke(nameof(Explode), fuseTime);
    }

    void Explode()
    {
        if (explosionVFX)
        {
            GameObject explosion = Instantiate(explosionVFX, transform.position, explosionVFX.transform.rotation);
            Destroy(explosion, EXPLOSION_VFX_LIFETIME);
        }
        AreaDamage.Apply(transform.position, explosionRadius, explosionDamage, damagesPlayer);
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
