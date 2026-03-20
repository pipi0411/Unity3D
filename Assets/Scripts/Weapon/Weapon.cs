using UnityEngine;

public enum WeaponType
{
    Pistol,
    Revolver,
    AutoRifle,
    Shotgun,
    Rifle
}
public enum ShootType
{
    Single,
    Auto
}
[System.Serializable] // Makes the class visible in the Unity Inspector
public class Weapon
{
    [Header("Data source")]
    public WeaponData weaponData;

    public WeaponType weaponType;
    [Header("Shooting specifics")]
    public ShootType shootType;
    public float fireRate = 1; //bullets per second
    [Min(1)] public int projectilesPerShot = 1;
    [Range(0f, 25f)] public float spreadAngle = 0f;
    [Min(1)] public int ammoPerShot = 1;
    [Min(0f)] public float damage = 10f;

    private float lastShootTime;
    private bool hasInitializedFromData;

    [Header("Magazine details")]
    public int bulletsInMagazine;
    public int magazineCapacity;
    public int totalReserveAmmo;

    [Range(1,3)]
    public float reloadSpeed = 1; // how fast the player can reload this weapon, affects the speed of reload animation
    [Range(1,3)]
    public float equipementSpeed = 1; // how fast the player can equip this weapon, affects the speed of equip animation

    public void InitializeFromDataIfNeeded()
    {
        if (hasInitializedFromData)
        {
            return;
        }

        ApplyDataFromAsset(resetRuntimeAmmo: true);
        bulletsInMagazine = Mathf.Clamp(bulletsInMagazine, 0, Mathf.Max(1, magazineCapacity));
        totalReserveAmmo = Mathf.Max(0, totalReserveAmmo);
        hasInitializedFromData = true;
    }

    public Weapon CreateRuntimeCopy()
    {
        Weapon runtimeCopy = (Weapon)MemberwiseClone();
        runtimeCopy.lastShootTime = 0f;
        runtimeCopy.hasInitializedFromData = false;
        return runtimeCopy;
    }

    private void ApplyDataFromAsset(bool resetRuntimeAmmo)
    {
        if (weaponData == null)
        {
            return;
        }

        weaponType = weaponData.weaponType;
        shootType = weaponData.shootType;
        fireRate = Mathf.Max(0.1f, weaponData.fireRate);
        projectilesPerShot = Mathf.Max(1, weaponData.projectilesPerShot);
        spreadAngle = Mathf.Clamp(weaponData.spreadAngle, 0f, 25f);
        ammoPerShot = Mathf.Max(1, weaponData.ammoPerShot);
        damage = Mathf.Max(0f, weaponData.damage);
        magazineCapacity = Mathf.Max(1, weaponData.magazineCapacity);
        reloadSpeed = Mathf.Clamp(weaponData.reloadSpeed, 1f, 3f);
        equipementSpeed = Mathf.Clamp(weaponData.equipementSpeed, 1f, 3f);

        if (resetRuntimeAmmo)
        {
            bulletsInMagazine = Mathf.Clamp(weaponData.startBulletsInMagazine, 0, magazineCapacity);
            totalReserveAmmo = Mathf.Max(0, weaponData.startReserveAmmo);
        }
        else
        {
            bulletsInMagazine = Mathf.Clamp(bulletsInMagazine, 0, magazineCapacity);
        }
    }
    
    public bool CanShoot()
    {
        InitializeFromDataIfNeeded();

        int requiredAmmo = Mathf.Max(1, ammoPerShot);
        if (HaveEnoughBullets(requiredAmmo) && ReadyToFire())
        {
            bulletsInMagazine -= requiredAmmo;
            return true;
        }
        return false;
    }
    private bool ReadyToFire()
    {
        if (Time.time > lastShootTime + 1 / fireRate)
        {
            lastShootTime = Time.time;
            return true;
        }
        return false;
    }
    private bool HaveEnoughBullets(int requiredAmmo) => bulletsInMagazine >= requiredAmmo;

    public bool CanReload()
    {
        InitializeFromDataIfNeeded();

        if (bulletsInMagazine == magazineCapacity)
        {
            return false;
        }
        if (totalReserveAmmo > 0)
        {
            return true;
        }
        return false;
    }
    public void RefillBullets()
    {
        InitializeFromDataIfNeeded();

        totalReserveAmmo += bulletsInMagazine;
        int bulletsToReload = magazineCapacity;
        if (bulletsToReload > totalReserveAmmo)
        {
            bulletsToReload = totalReserveAmmo;
        }
        totalReserveAmmo -= bulletsToReload;
        bulletsInMagazine = bulletsToReload;
        if (totalReserveAmmo < 0)
        {
            totalReserveAmmo = 0;
        }
    }
}
