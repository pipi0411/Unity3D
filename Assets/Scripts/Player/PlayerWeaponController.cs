using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    [System.Serializable]
    public class WeaponSaveEntry
    {
        public WeaponType weaponType;
        public int bulletsInMagazine;
        public int totalReserveAmmo;
        public bool isCurrent;
    }

    private Player player;
    [SerializeField] private Weapon currentWeapon;
    private bool isShooting;
    private bool isReloading;
    private float reloadStartUnscaledTime = -1f;
    [Header("Bullet Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed;
    [SerializeField, Min(0f)] private float muzzleOffset = 0.2f;
    [SerializeField] private Transform gunPoint;
    [Header("Inventory")]
    [SerializeField] private int maxWeaponSlots = 2; // Giới hạn số lượng vũ khí có thể mang theo
    [SerializeField] private List<Weapon> weaponSlots;

    [Header("Reload Safety")]
    [SerializeField, Min(0.1f)] private float reloadFailSafeSeconds = 2.2f;

    [Header("Pickup Feedback")]
    [SerializeField] private TMP_Text slotFullText;
    [SerializeField] private string slotFullMessage = "Slot đầy";
    [SerializeField, Min(0f)] private float slotFullTextDuration = 1.5f;

    private Coroutine slotFullTextRoutine;

    private void Start()
    {
        player = GetComponent<Player>();
        InitializeWeapons();
        AssignInputEvents();

        if (slotFullText != null)
        {
            slotFullText.gameObject.SetActive(false);
        }

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
        HandleReloadFailSafe();

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
                reloadStartUnscaledTime = Time.unscaledTime;
                isShooting = false;
                player.weaponVisualController.PlayReloadAnimation();
            }
        };
    }

    private void HandleReloadFailSafe()
    {
        if (!isReloading)
        {
            return;
        }

        float elapsed = Time.unscaledTime - reloadStartUnscaledTime;
        if (elapsed < Mathf.Max(0.1f, reloadFailSafeSeconds))
        {
            return;
        }

        // Fallback when reload animation event is missed after load/scene transitions.
        if (currentWeapon != null && currentWeapon.CanReload())
        {
            currentWeapon.RefillBullets();
        }

        OnReloadFinished();
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
            ShowSlotFullFeedback();
            return false; // Không thể nhặt thêm vũ khí nếu đã đầy
        }

        Weapon runtimeWeapon = newWeapon.CreateRuntimeCopy();
        runtimeWeapon.InitializeFromDataIfNeeded();
        weaponSlots.Add(runtimeWeapon);
        player.weaponVisualController.SwitchOnBackupWeaponModel();
        return true;
    }

    private void ShowSlotFullFeedback()
    {
        if (slotFullText == null)
        {
            return;
        }

        slotFullText.text = string.IsNullOrWhiteSpace(slotFullMessage) ? "Slot đầy" : slotFullMessage;
        slotFullText.gameObject.SetActive(true);

        if (slotFullTextRoutine != null)
        {
            StopCoroutine(slotFullTextRoutine);
        }

        slotFullTextRoutine = StartCoroutine(HideSlotFullTextAfterDelay());
    }

    private IEnumerator HideSlotFullTextAfterDelay()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, slotFullTextDuration));

        if (slotFullText != null)
        {
            slotFullText.gameObject.SetActive(false);
        }

        slotFullTextRoutine = null;
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
        reloadStartUnscaledTime = -1f;
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

    public List<WeaponSaveEntry> CreateSaveSnapshot()
    {
        List<WeaponSaveEntry> entries = new List<WeaponSaveEntry>();

        for (int i = 0; i < weaponSlots.Count; i++)
        {
            Weapon weapon = weaponSlots[i];
            if (weapon == null)
            {
                continue;
            }

            weapon.InitializeFromDataIfNeeded();
            entries.Add(new WeaponSaveEntry
            {
                weaponType = weapon.weaponType,
                bulletsInMagazine = Mathf.Max(0, weapon.bulletsInMagazine),
                totalReserveAmmo = Mathf.Max(0, weapon.totalReserveAmmo),
                isCurrent = weapon == currentWeapon
            });
        }

        return entries;
    }

    public void ApplySaveSnapshot(List<WeaponSaveEntry> entries)
    {
        if (entries == null || entries.Count == 0)
        {
            return;
        }

        // Prevent delayed startup auto-equip from overriding restored save state.
        CancelInvoke(nameof(EquipStartingWeapon));

        List<Weapon> templates = GatherWeaponTemplatesFromScene();
        List<Weapon> rebuiltSlots = new List<Weapon>();
        Weapon selectedCurrent = null;

        for (int i = 0; i < entries.Count && rebuiltSlots.Count < maxWeaponSlots; i++)
        {
            WeaponSaveEntry entry = entries[i];
            Weapon template = FindTemplateByType(templates, entry.weaponType);
            if (template == null)
            {
                continue;
            }

            Weapon runtimeWeapon = template.CreateRuntimeCopy();
            runtimeWeapon.InitializeFromDataIfNeeded();
            runtimeWeapon.bulletsInMagazine = Mathf.Clamp(entry.bulletsInMagazine, 0, Mathf.Max(1, runtimeWeapon.magazineCapacity));
            runtimeWeapon.totalReserveAmmo = Mathf.Max(0, entry.totalReserveAmmo);

            rebuiltSlots.Add(runtimeWeapon);

            if (entry.isCurrent)
            {
                selectedCurrent = runtimeWeapon;
            }
        }

        if (rebuiltSlots.Count == 0)
        {
            return;
        }

        weaponSlots = rebuiltSlots;
        currentWeapon = selectedCurrent != null ? selectedCurrent : weaponSlots[0];
        isReloading = false;
        reloadStartUnscaledTime = -1f;
        isShooting = false;
        player.weaponVisualController.PlayWeaponEquipAnimation();
    }

    private List<Weapon> GatherWeaponTemplatesFromScene()
    {
        List<Weapon> templates = new List<Weapon>();

        for (int i = 0; i < weaponSlots.Count; i++)
        {
            if (weaponSlots[i] != null)
            {
                templates.Add(weaponSlots[i]);
            }
        }

        if (currentWeapon != null)
        {
            templates.Add(currentWeapon);
        }

        Item_Pickup[] pickups = FindObjectsByType<Item_Pickup>(FindObjectsSortMode.None);
        for (int i = 0; i < pickups.Length; i++)
        {
            Weapon template = pickups[i].PickupWeaponTemplate;
            if (template != null)
            {
                templates.Add(template);
            }
        }

        return templates;
    }

    private static Weapon FindTemplateByType(List<Weapon> templates, WeaponType weaponType)
    {
        for (int i = 0; i < templates.Count; i++)
        {
            Weapon template = templates[i];
            if (template == null)
            {
                continue;
            }

            WeaponType type = template.weaponData != null ? template.weaponData.weaponType : template.weaponType;
            if (type == weaponType)
            {
                return template;
            }
        }

        return null;
    }
}
