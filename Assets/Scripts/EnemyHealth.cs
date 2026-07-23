using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
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
            Destroy(this.gameObject);
        }
    }
}
