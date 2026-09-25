using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
   [SerializeField] GameObject deathEffectPrefab;
   [SerializeField] private int startingHealth = 3;
    private int currentHealth;

    void Awake()
    {
        currentHealth = startingHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            SelfDestruct();
        }
    }

    public void SelfDestruct()
    {
        Collider enemyCollider = GetComponent<Collider>();
        Vector3 effectPosition = enemyCollider ? enemyCollider.bounds.center : transform.position;
        Instantiate(deathEffectPrefab, effectPosition, Quaternion.identity);
        Destroy(this.gameObject);
    }
}
