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
        Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }
}
