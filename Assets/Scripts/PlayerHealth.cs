using UnityEngine;
using Unity.Cinemachine;
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int startingHealth = 10;
    [SerializeField] CinemachineCamera deathVirtualCamera;
    [SerializeField] Transform weaponCamera;
    [SerializeField] GameObject crosshair;
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
            if (crosshair != null)
            {
                crosshair.SetActive(false);
            }
            Destroy(this.gameObject);
        }
    }
}