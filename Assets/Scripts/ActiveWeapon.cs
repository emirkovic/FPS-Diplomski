using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;
using Unity.Cinemachine;

[RequireComponent(typeof(Animator))]
public class ActiveWeapon : MonoBehaviour
{
    [SerializeField] WeaponSO weaponSO;
    [SerializeField] CinemachineVirtualCamera playerFollowCamera;
    [SerializeField] GameObject zoomReticle;
    Animator gunAnimator;
    StarterAssetsInputs starterAssetsInputs;
    Weapon currentWeapon;
    readonly Dictionary<int, WeaponSO> weaponSlots = new Dictionary<int, WeaponSO>();
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
        EquipWeapon(weaponSO);
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
        if (nextTimeToFire >= weaponSO.fireRate)
        {
            currentWeapon.Shoot(weaponSO);
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

    void HandleZoom()
    {
        if (weaponSO == null || starterAssetsInputs == null) return;
        if (!weaponSO.canZoom) return;
        if (starterAssetsInputs.zoom)
        {
            playerFollowCamera.m_Lens.FieldOfView = weaponSO.zoomFOV;
            zoomReticle.SetActive(true);
        }
        else
        {
            playerFollowCamera.m_Lens.FieldOfView = originalFOV;
            zoomReticle.SetActive(false);
        }
    }
}
