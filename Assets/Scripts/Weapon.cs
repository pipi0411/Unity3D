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
    public WeaponType weaponType;
    [Header("Shooting specifics")]
    public ShootType shootType;
    public float fireRate = 1; //bullets per second
    private float lastShootTime;
    [Header("Magazine details")]
    public int bulletsInMagazine;
    public int magazineCapacity;
    public int totalReserveAmmo;
    [Range(1,3)]
    public float reloadSpeed = 1; // how fast the player can reload this weapon, affects the speed of reload animation
    [Range(1,3)]
    public float equipementSpeed = 1; // how fast the player can equip this weapon, affects the speed of equip animation
    
    public bool CanShoot()
    {
        if (HaveEnoughBullets() && ReadyToFire())
        {
            bulletsInMagazine--;
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
    private bool HaveEnoughBullets () => bulletsInMagazine > 0;
    public bool CanReload()
    {
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
