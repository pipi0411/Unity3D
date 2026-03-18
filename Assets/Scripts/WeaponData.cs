using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Game/Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public WeaponType weaponType;

    [Header("Shooting specifics")]
    public ShootType shootType = ShootType.Single;
    [Min(0.1f)] public float fireRate = 1f;
    [Min(1)] public int projectilesPerShot = 1;
    [Range(0f, 25f)] public float spreadAngle = 0f;
    [Min(1)] public int ammoPerShot = 1;
    [Min(0f)] public float damage = 10f;

    [Header("Magazine details")]
    [Min(1)] public int magazineCapacity = 12;
    [Min(0)] public int startBulletsInMagazine = 12;
    [Min(0)] public int startReserveAmmo = 48;

    [Header("Animation")]
    [Range(1, 3)] public float reloadSpeed = 1f;
    [Range(1, 3)] public float equipementSpeed = 1f;
}
