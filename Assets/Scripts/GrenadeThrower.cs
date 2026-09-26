using UnityEngine;
using UnityEngine.InputSystem;

public class GrenadeThrower : MonoBehaviour
{
    [SerializeField] Rigidbody grenadePrefab;
    [SerializeField] int startingGrenades = 0;
    [SerializeField] Key throwKey = Key.G;
    [SerializeField] float throwForce = 15f;
    [SerializeField] float upwardForce = 3f;
    [SerializeField] float throwCooldown = 0.5f;
    [SerializeField] float spawnDistance = 0.8f;
    PlayerHealth playerHealth;
    int grenadesLeft;
    float nextThrowTime;

    void Start()
    {
        playerHealth = GetComponentInParent<PlayerHealth>();
        grenadesLeft = startingGrenades;
        if (grenadesLeft > 0)
        {
            UpdateGrenadeUI();
        }
    }

    void Update()
    {
        if (Keyboard.current == null || !grenadePrefab) return;
        if (!Keyboard.current[throwKey].wasPressedThisFrame) return;
        if (grenadesLeft <= 0 || Time.time < nextThrowTime) return;

        ThrowGrenade();
    }

    public void AddGrenades(int amount)
    {
        grenadesLeft += amount;
        UpdateGrenadeUI();
    }

    void ThrowGrenade()
    {
        Transform cameraTransform = Camera.main.transform;
        Vector3 spawnPosition = cameraTransform.position + cameraTransform.forward * spawnDistance;

        Rigidbody grenade = Instantiate(grenadePrefab, spawnPosition, cameraTransform.rotation);
        ShooterCollision.Ignore(grenade.gameObject, this);
        grenade.AddForce(cameraTransform.forward * throwForce + Vector3.up * upwardForce, ForceMode.VelocityChange);
        grenade.AddTorque(Random.insideUnitSphere * 5f, ForceMode.VelocityChange);

        grenadesLeft--;
        nextThrowTime = Time.time + throwCooldown;
        UpdateGrenadeUI();
    }

    void UpdateGrenadeUI()
    {
        if (playerHealth)
        {
            playerHealth.UpdateGrenadeUI(grenadesLeft);
        }
    }
}
