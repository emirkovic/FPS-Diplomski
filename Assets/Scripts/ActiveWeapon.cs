using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using StarterAssets;
using Unity.Cinemachine;
using TMPro;

[RequireComponent(typeof(Animator))]
public class ActiveWeapon : MonoBehaviour
{
    [SerializeField] WeaponSO weaponSO;
    [SerializeField] CinemachineVirtualCamera playerFollowCamera;
    [SerializeField] Camera weaponCamera;
    [SerializeField] GameObject zoomReticle;
    [SerializeField] Image ammoImage;
    [SerializeField] TMP_Text ammoText;
    Animator gunAnimator;
    StarterAssetsInputs starterAssetsInputs;
    Weapon currentWeapon;
    readonly Dictionary<int, WeaponSO> weaponSlots = new Dictionary<int, WeaponSO>();
    readonly Dictionary<WeaponSO, int> ammoPerWeapon = new Dictionary<WeaponSO, int>();
    const string SHOOT_ANIMATION = "Shoot";
    float nextTimeToFire = 0f;
    float originalFOV;
    void Awake()
    {
        starterAssetsInputs = GetComponentInParent<StarterAssetsInputs>();
        gunAnimator = GetComponent<Animator>();
        originalFOV = playerFollowCamera.m_Lens.FieldOfView;

        if (starterAssetsInputs == null)
        {
            Debug.LogError("ActiveWeapon requires a StarterAssetsInputs component on its parent.", this);
            enabled = false;
        }
    }
    void Start()
    {
        currentWeapon = GetComponentInChildren<Weapon>();
        if (weaponSO != null)
        {
            ammoPerWeapon[weaponSO] = weaponSO.magazineSize;
        }
        UpdateAmmoUI();
    }
    void Update()
    {
        nextTimeToFire += Time.deltaTime;
        HandleShoot();
        HandleZoom();
        HandleWeaponSwitchInput();
    }

    public void SwitchWeapon(WeaponSO weaponSO)
    {
        weaponSlots[weaponSO.weaponSlot] = weaponSO;
        ammoPerWeapon[weaponSO] = GetAmmo(weaponSO) + weaponSO.magazineSize;
        EquipWeapon(weaponSO);
    }

    public void AddAmmo(WeaponSO weaponToRefill, int amount)
    {
        if (weaponToRefill == null) return;
        ammoPerWeapon[weaponToRefill] = GetAmmo(weaponToRefill) + amount;
        UpdateAmmoUI();
    }

    void EquipWeapon(WeaponSO weaponToEquip)
    {
        if (currentWeapon)
        {
            Destroy(currentWeapon.gameObject);
        }
        Weapon newWeapon = Instantiate(weaponToEquip.weaponPrefab, transform).GetComponent<Weapon>();
        currentWeapon = newWeapon;
        weaponSO = weaponToEquip;
        UpdateAmmoUI();
    }

    void HandleWeaponSwitchInput()
    {
        if (Keyboard.current == null) return;

        for (int slot = 1; slot <= 9; slot++)
        {
            Key key = Key.Digit1 + (slot - 1);
            if (Keyboard.current[key].wasPressedThisFrame)
            {
                TrySwitchToSlot(slot);
                break;
            }
        }
    }

    void TrySwitchToSlot(int slot)
    {
        if (weaponSlots.TryGetValue(slot, out WeaponSO weaponToEquip) && weaponToEquip != weaponSO)
        {
            EquipWeapon(weaponToEquip);
        }
    }

    void HandleShoot()
    {
        if (starterAssetsInputs == null) return;
        if (!starterAssetsInputs.shoot) return;
        if (!currentWeapon || weaponSO == null) return;
        if (nextTimeToFire >= weaponSO.fireRate && GetCurrentAmmo() > 0)
        {
            bool isZoomed = weaponSO.canZoom && starterAssetsInputs.zoom;
            currentWeapon.Shoot(weaponSO, isZoomed);
            ammoPerWeapon[weaponSO]--;
            UpdateAmmoUI();
            if (gunAnimator != null)
            {
                gunAnimator.Play(SHOOT_ANIMATION, 0, 0f);
            }
            nextTimeToFire = 0f;
        }

        if (!weaponSO.IsAutomatic)
        {
            starterAssetsInputs.ShootInput(false);
        }
    }

    int GetCurrentAmmo()
    {
        return GetAmmo(weaponSO);
    }

    int GetAmmo(WeaponSO weapon)
    {
        return ammoPerWeapon.TryGetValue(weapon, out int ammo) ? ammo : 0;
    }

    void UpdateAmmoUI()
    {
        bool hasWeapon = weaponSO != null;
        if (ammoImage != null)
        {
            ammoImage.gameObject.SetActive(hasWeapon);
        }
        if (ammoText != null)
        {
            ammoText.gameObject.SetActive(hasWeapon);
            if (hasWeapon)
            {
                ammoText.text = GetCurrentAmmo().ToString();
            }
        }
    }

    void HandleZoom()
    {
        if (weaponSO == null || starterAssetsInputs == null) return;
        if (!weaponSO.canZoom) return;
        if (starterAssetsInputs.zoom)
        {
            playerFollowCamera.m_Lens.FieldOfView = weaponSO.zoomFOV;
            weaponCamera.fieldOfView = weaponSO.zoomFOV;
            zoomReticle.SetActive(true);
        }
        else
        {
            playerFollowCamera.m_Lens.FieldOfView = originalFOV;
            weaponCamera.fieldOfView = originalFOV;
            zoomReticle.SetActive(false);
        }
    }
}
