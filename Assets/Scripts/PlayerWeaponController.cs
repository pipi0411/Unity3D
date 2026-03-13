using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    private Player player;
    [SerializeField] private Weapon currentWeapon;
    [Header("Bullet Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private Transform gunPoint;
    private void Start()
    {
        player = GetComponent<Player>();
        player.controls.Character.Fire.performed += ctx => Shoot();

        currentWeapon.ammo = currentWeapon.maxAmmo; // Initialize ammo to max at the start
    }
    private void Shoot()
    {
        if (currentWeapon.ammo-- <= 0)
        {
            Debug.Log("Out of ammo!");
            return;
        }
        GameObject newBullet = Instantiate(bulletPrefab, gunPoint.position, Quaternion.LookRotation(gunPoint.forward));
        newBullet.GetComponent<Rigidbody>().linearVelocity = gunPoint.forward * bulletSpeed;
        Destroy(newBullet, .75f);
        var animator = GetComponentInChildren<Animator>();
        animator.SetTrigger("Fire");
    }
}
