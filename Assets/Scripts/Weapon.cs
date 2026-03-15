using NUnit.Framework;

public enum WeaponType
{
    Pistol,
    Revolver,
    AutoRifle,
    Shotgun,
    Rifle
}
[System.Serializable] // Makes the class visible in the Unity Inspector
public class Weapon
{
    public WeaponType weaponType;
    public int ammo;
    public int maxAmmo;
    public bool CanShoot()
    {
        return HaveEnoughBullets();
    }
    private bool HaveEnoughBullets ()
    {
        if (ammo > 0)
        {
            ammo--;
            return true;
        }
        return false;
    }
}
