using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
   [SerializeField] GameObject deathEffectPrefab;
   [SerializeField] private int startingHealth = 3;
   [SerializeField] AudioClip deathSFX;
   [SerializeField] [Range(0f, 1f)] float deathSFXVolume = 1f;
    private int currentHealth;
    GameManager gameManager;
    bool isDead;

    public event System.Action Damaged;
    public float HealthPercent => startingHealth > 0 ? Mathf.Clamp01((float)currentHealth / startingHealth) : 0f;

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
        Damaged?.Invoke();
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
        if (deathSFX)
        {
            // Played from a temporary object because this enemy is destroyed straight away.
            AudioSource.PlayClipAtPoint(deathSFX, effectPosition, deathSFXVolume);
        }
        Destroy(this.gameObject);
    }
}
