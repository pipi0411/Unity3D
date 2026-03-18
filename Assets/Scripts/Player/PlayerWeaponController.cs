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
    [SerializeField] private Transform gunPoint;
    [Header("Inventory")]
    [SerializeField] private int maxWeaponSlots = 2; // Giới hạn số lượng vũ khí có thể mang theo
    [SerializeField] private List<Weapon> weaponSlots;
    private void Start()
    {
        player = GetComponent<Player>();
        AssignInputEvents();

        Invoke(nameof(EquipStartingWeapon), .1f);
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

        currentWeapon = weaponSlots[i];
        player.weaponVisualController.PlayWeaponEquipAnimation();
    }
    public void PickupWeapon(Weapon newWeapon)
    {
        if (weaponSlots.Count >= maxWeaponSlots)
        {
            Debug.Log("Inventory full! Cannot pick up ");
            return; // Không thể nhặt thêm vũ khí nếu đã đầy
        }
        weaponSlots.Add(newWeapon);
        player.weaponVisualController.SwitchOnBackupWeaponModel();
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

        GameObject newBullet = ObjectPool.Instance.GetBullet();
        newBullet.transform.position = gunPoint.position;
        newBullet.transform.rotation = Quaternion.LookRotation(gunPoint.forward);

        Rigidbody rbNewBullet = newBullet.GetComponent<Rigidbody>();
        if (rbNewBullet != null)
        {
            rbNewBullet.linearVelocity = gunPoint.forward * bulletSpeed;
        }

        var animator = GetComponentInChildren<Animator>();
        animator?.SetTrigger("Fire");
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
