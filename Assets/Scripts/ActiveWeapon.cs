using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

public class ActiveWeapon : MonoBehaviour
{
    [SerializeField] WeaponSO weaponSO;
    Animator gunAnimator;
    StarterAssetsInputs starterAssetsInputs;
    Weapon currentWeapon;
    readonly Dictionary<int, WeaponSO> weaponSlots = new Dictionary<int, WeaponSO>();
    const string SHOOT_ANIMATION = "Shoot";
    float nextTimeToFire = 0f;
    void Awake()
    {
        starterAssetsInputs = GetComponentInParent<StarterAssetsInputs>();
        gunAnimator = GetComponent<Animator>();
    }
    void Start()
    {
        currentWeapon = GetComponentInChildren<Weapon>();
    }
    void Update()
    {
        nextTimeToFire += Time.deltaTime;
        HandleShoot();
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
        if (!starterAssetsInputs.shoot) return;
        if (!currentWeapon || weaponSO == null) return;
        if (nextTimeToFire >= weaponSO.fireRate)
        {
            currentWeapon.Shoot(weaponSO);
            gunAnimator.Play(SHOOT_ANIMATION, 0, 0f);
            nextTimeToFire = 0f;
        }

        if (!weaponSO.IsAutomatic)
        {
            starterAssetsInputs.ShootInput(false);
        }
    }
}
