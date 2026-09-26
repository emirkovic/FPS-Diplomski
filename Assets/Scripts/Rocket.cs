using UnityEngine;

public class Rocket : MonoBehaviour
{
    public float speed = 25f;
    public GameObject rocketExplosion;
    public MeshRenderer projectileMesh;
    public AudioSource inFlightAudioSource;
    public ParticleSystem disableOnHit;

    [SerializeField] float explosionRadius = 4f;
    [SerializeField] int explosionDamage = 10;
    [SerializeField] bool damagesPlayer = false;
    [SerializeField] float maxLifetime = 10f;

    const float EXPLOSION_VFX_LIFETIME = 5f;
    const float DESTROY_DELAY_AFTER_HIT = 5f;

    Rigidbody rb;
    bool hasExploded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, maxLifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;
        Explode();
    }

    void Explode()
    {
        hasExploded = true;

        if (rocketExplosion)
        {
            GameObject explosion = Instantiate(rocketExplosion, transform.position, rocketExplosion.transform.rotation);
            Destroy(explosion, EXPLOSION_VFX_LIFETIME);
        }
        AreaDamage.Apply(transform.position, explosionRadius, explosionDamage, damagesPlayer);

        // Hide the rocket but keep the object alive for a moment so the smoke trail can fade out.
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        foreach (Collider rocketCollider in GetComponentsInChildren<Collider>())
        {
            rocketCollider.enabled = false;
        }
        if (projectileMesh) projectileMesh.enabled = false;
        if (inFlightAudioSource) inFlightAudioSource.Stop();
        if (disableOnHit) disableOnHit.Stop();

        Destroy(gameObject, DESTROY_DELAY_AFTER_HIT);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
