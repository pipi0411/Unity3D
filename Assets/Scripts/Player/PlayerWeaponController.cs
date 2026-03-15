using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    private Player player;
    [SerializeField] private Weapon currentWeapon;
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

        if (currentWeapon == null && weaponSlots.Count > 0)
        {
            currentWeapon = weaponSlots[0];
        }

        if (currentWeapon != null)
        {
            currentWeapon.ammo = Mathf.Max(currentWeapon.ammo, 0);
        }
    }

    private void AssignInputEvents()
    {
        PlayerControls controls = player.controls;
        controls.Character.Fire.performed += ctx => Shoot();
        controls.Character.EquipSlot1.performed += ctx => EquipWeapon(0);
        controls.Character.EquipSlot2.performed += ctx => EquipWeapon(1);
        controls.Character.DropCurrentWeapon.performed += ctx => DropWeapon();
    }

    private void EquipWeapon(int i)
    {
        if (i < 0 || i >= weaponSlots.Count || weaponSlots[i] == null)
        {
            return;
        }

        currentWeapon = weaponSlots[i];
    }
    public void PickupWeapon(Weapon newWeapon)
    {
        if (weaponSlots.Count >= maxWeaponSlots)
        {
            Debug.Log("Inventory full! Cannot pick up ");
            return; // Không thể nhặt thêm vũ khí nếu đã đầy
        }
        weaponSlots.Add(newWeapon);
    }
    private void DropWeapon()
    {
        if (weaponSlots.Count <= 1)
        {
            return;
        }
        weaponSlots.Remove(currentWeapon);
        currentWeapon = weaponSlots[0];
    }
    

    private void Shoot()
    {
        if (currentWeapon.CanShoot() == false)
        {
            return;
        }

        if (bulletPrefab == null || gunPoint == null)
        {
            Debug.LogWarning("Missing bulletPrefab or gunPoint reference.");
            return;
        }

        GameObject newBullet = Instantiate(bulletPrefab, gunPoint.position, Quaternion.LookRotation(gunPoint.forward));
        newBullet.GetComponent<Rigidbody>().linearVelocity = gunPoint.forward * bulletSpeed;
        Destroy(newBullet, .75f);
        var animator = GetComponentInChildren<Animator>();
        animator.SetTrigger("Fire");
    }
}
