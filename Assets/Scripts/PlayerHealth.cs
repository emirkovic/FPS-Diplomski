using UnityEngine;
using Unity.Cinemachine;
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int startingHealth = 10;
    [SerializeField] CinemachineVirtualCamera deathVirtualCamera;
    [SerializeField] Transform weaponCamera;
    private int currentHealth;
    int gameOverCameraPriority = 20;

    void Start()
    {
        currentHealth = startingHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            weaponCamera.parent = null;
            deathVirtualCamera.Priority = gameOverCameraPriority;
            Destroy(this.gameObject);
        }
    }
}