using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.UI;
public class PlayerHealth : MonoBehaviour
{
    [Range(1, 10)]
    [SerializeField] private int startingHealth = 10;
    [SerializeField] CinemachineCamera deathVirtualCamera;
    [SerializeField] Transform weaponCamera;
    [SerializeField] GameObject crosshair;
    [SerializeField] GameObject shieldContainer;
    [SerializeField] GameObject ammoContainer;
    [SerializeField] Image[] shieldBars;
    [SerializeField] GameObject gameOverContauiner;
    private int currentHealth;
    int gameOverCameraPriority = 20;

    void Start()
    {
        currentHealth = startingHealth;
        AdjustShieldBars();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        AdjustShieldBars();
        if (currentHealth <= 0)
        {
            weaponCamera.parent = null;
            deathVirtualCamera.Priority = gameOverCameraPriority;
            if (crosshair != null)
            {
                crosshair.SetActive(false);
            }
            if (shieldContainer != null)
            {
                shieldContainer.SetActive(false);
            }
            if (ammoContainer != null)
            {
                ammoContainer.SetActive(false);
            }
            gameOverContauiner.SetActive(true);
            StarterAssets.StarterAssetsInputs starterAssetsInputs = FindFirstObjectByType<StarterAssets.StarterAssetsInputs>();
            starterAssetsInputs.enabled = false;
            Destroy(this.gameObject);
        }
    }

    void AdjustShieldBars()
    {
        for (int i = 0; i < shieldBars.Length; i++)
        {
            if (i < currentHealth)
            {
                shieldBars[i].gameObject.SetActive(true);
            }
            else
            {
                shieldBars[i].gameObject.SetActive(false);
            }
        }
    }
}