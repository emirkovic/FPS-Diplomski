using UnityEngine;
using StarterAssets;

public class ActiveWeapon : MonoBehaviour
{
    [SerializeField] WeaponSO weaponSO;
    Animator gunAnimator;
    StarterAssetsInputs starterAssetsInputs;
    Weapon currentWeapon;
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
    }

    public void SwitchWeapon(WeaponSO weaponSO)
    {
        if (currentWeapon)
        {
            Destroy(currentWeapon.gameObject);
        }
        Weapon newWeapon = Instantiate(weaponSO.weaponPrefab, transform).GetComponent<Weapon>();
        currentWeapon = newWeapon;
        this.weaponSO = weaponSO;
    }

    void HandleShoot()
    {
        if (!starterAssetsInputs.shoot) return;
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
