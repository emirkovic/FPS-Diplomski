using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
   [SerializeField] GameObject deathEffectPrefab;
   [SerializeField] private int startingHealth = 3;
    private int currentHealth;
    GameManager gameManager;
    bool isDead;

    void Awake()
    {
        currentHealth = startingHealth;
    }

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager) gameManager.AdjustEnemiesLeft(1);
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
        if (isDead) return;
        isDead = true;
        if (gameManager) gameManager.AdjustEnemiesLeft(-1);

        Collider enemyCollider = GetComponent<Collider>();
        Vector3 effectPosition = enemyCollider ? enemyCollider.bounds.center : transform.position;
        Instantiate(deathEffectPrefab, effectPosition, Quaternion.identity);
        Destroy(this.gameObject);
    }
}
