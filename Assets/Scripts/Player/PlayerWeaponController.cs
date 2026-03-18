using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    private Player player;
    [SerializeField] private Weapon currentWeapon;
    private bool isShooting;
    private bool isReloading;
    [Header("Bullet Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed;
    [SerializeField, Min(0f)] private float muzzleOffset = 0.2f;
    [SerializeField] private Transform gunPoint;
    [Header("Inventory")]
    [SerializeField] private int maxWeaponSlots = 2; // Giới hạn số lượng vũ khí có thể mang theo
    [SerializeField] private List<Weapon> weaponSlots;

    private void Start()
    {
        player = GetComponent<Player>();
        InitializeWeapons();
        AssignInputEvents();

        Invoke(nameof(EquipStartingWeapon), .1f);
    }

    private void InitializeWeapons()
    {
        for (int i = 0; i < weaponSlots.Count; i++)
        {
            if (weaponSlots[i] == null)
            {
                continue;
            }

            Weapon runtimeWeapon = weaponSlots[i].CreateRuntimeCopy();
            runtimeWeapon.InitializeFromDataIfNeeded();
            weaponSlots[i] = runtimeWeapon;
        }

        if (currentWeapon != null)
        {
            currentWeapon = currentWeapon.CreateRuntimeCopy();
            currentWeapon.InitializeFromDataIfNeeded();
        }
    }
    private void Update()
    {
        if (isShooting)
        {
            Shoot();
        }
    }
    private void EquipStartingWeapon() => EquipWeapon(0);
    public Weapon GetCurrentWeapon()
    {
        return currentWeapon;
    }
    private void AssignInputEvents()
    {
        PlayerControls controls = player.controls;
        controls.Character.Fire.performed += ctx => isShooting = true;
        controls.Character.Fire.canceled += ctx => isShooting = false;
        controls.Character.EquipSlot1.performed += ctx => EquipWeapon(0);
        controls.Character.EquipSlot2.performed += ctx => EquipWeapon(1);
        controls.Character.DropCurrentWeapon.performed += ctx => DropWeapon();
        controls.Character.Reload.performed += ctx =>
        {
            if (currentWeapon != null && isReloading == false && currentWeapon.CanReload())
            {
                isReloading = true;
                isShooting = false;
                player.weaponVisualController.PlayReloadAnimation();
            }
        };
    }

    private void EquipWeapon(int i)
    {
        if (i < 0 || i >= weaponSlots.Count || weaponSlots[i] == null)
        {
            return;
        }

        weaponSlots[i].InitializeFromDataIfNeeded();
        currentWeapon = weaponSlots[i];
        player.weaponVisualController.PlayWeaponEquipAnimation();
    }

    public bool PickupWeapon(Weapon newWeapon)
    {
        if (newWeapon == null)
        {
            return false;
        }

        if (weaponSlots.Count >= maxWeaponSlots)
        {
            Debug.Log("Inventory full! Cannot pick up ");
            return false; // Không thể nhặt thêm vũ khí nếu đã đầy
        }

        Weapon runtimeWeapon = newWeapon.CreateRuntimeCopy();
        runtimeWeapon.InitializeFromDataIfNeeded();
        weaponSlots.Add(runtimeWeapon);
        player.weaponVisualController.SwitchOnBackupWeaponModel();
        return true;
    }
    private void DropWeapon()
    {
        if (HasOnlyOneWeapon())
        {
            return; // Không thể bỏ vũ khí nếu chỉ có một
        }
        weaponSlots.Remove(currentWeapon);
        EquipWeapon(0);
    }
    

    private void Shoot()
    {
        if (isReloading)
        {
            return;
        }

        if (currentWeapon == null || currentWeapon.CanShoot() == false)
        {
            return;
        }

        if (gunPoint == null)
        {
            Debug.LogWarning("Missing gunPoint reference.");
            return;
        }

        if (ObjectPool.Instance == null)
        {
            Debug.LogWarning("ObjectPool.Instance is null.");
            return;
        }
        if (currentWeapon.shootType == ShootType.Single)
        {
            isShooting = false; // Đặt lại trạng thái bắn cho súng bắn từng viên
        }

        FireCurrentWeaponProjectiles();

        var animator = GetComponentInChildren<Animator>();
        animator?.SetTrigger("Fire");
    }

    private void FireCurrentWeaponProjectiles()
    {
        int projectileCount = Mathf.Max(1, currentWeapon.projectilesPerShot);

        for (int i = 0; i < projectileCount; i++)
        {
            Vector3 shootDirection = GetSpreadDirection(i, projectileCount, currentWeapon.spreadAngle);
            SpawnBullet(shootDirection);
        }
    }

    private Vector3 GetSpreadDirection(int projectileIndex, int projectileCount, float maxSpreadAngle)
    {
        if (projectileCount <= 1 || maxSpreadAngle <= 0f)
        {
            return gunPoint.forward;
        }

        float lerp = projectileIndex / (projectileCount - 1f);
        float yawAngle = Mathf.Lerp(-maxSpreadAngle, maxSpreadAngle, lerp);
        Quaternion spreadRotation = Quaternion.Euler(0f, yawAngle, 0f);
        return spreadRotation * gunPoint.forward;
    }

    private void SpawnBullet(Vector3 shootDirection)
    {
        GameObject newBullet = ObjectPool.Instance.GetBullet();
        if (newBullet == null)
        {
            return;
        }

        Bullet bulletComponent = newBullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.SetDamage(currentWeapon.damage);
        }

        newBullet.transform.position = gunPoint.position + shootDirection * muzzleOffset;
        newBullet.transform.rotation = Quaternion.LookRotation(shootDirection);

        Rigidbody rbNewBullet = newBullet.GetComponent<Rigidbody>();
        if (rbNewBullet != null)
        {
            rbNewBullet.useGravity = false;
            rbNewBullet.linearVelocity = shootDirection * bulletSpeed;
        }
    }

    public void OnReloadFinished()
    {
        isReloading = false;
    }

    public bool IsReloading()
    {
        return isReloading;
    }
    public Weapon BackupWeapon()
    {
        foreach (Weapon weapon in weaponSlots)
        {
            if (weapon != currentWeapon)
            {
                return weapon;
            }
        }
        return null;
    }
    public bool HasOnlyOneWeapon()
    {
        return weaponSlots.Count <= 1;
    }
}
