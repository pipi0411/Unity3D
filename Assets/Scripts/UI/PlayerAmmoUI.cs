using TMPro;
using UnityEngine;

public class PlayerAmmoUI : MonoBehaviour
{
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private PlayerWeaponController weaponController;
    [SerializeField] private string textPrefix = "Ammo: ";
    [SerializeField] private string noWeaponText = "-/-";

    private int lastMagazine = int.MinValue;
    private int lastReserve = int.MinValue;
    private Weapon lastWeapon;

    private void Awake()
    {
        if (ammoText == null)
        {
            ammoText = GetComponent<TMP_Text>();
        }
    }

    private void OnEnable()
    {
        ResolveWeaponController();
        RefreshAmmoText(force: true);
    }

    private void Update()
    {
        if (weaponController == null)
        {
            ResolveWeaponController();
            RefreshAmmoText(force: true);
            return;
        }

        RefreshAmmoText(force: false);
    }

    private void ResolveWeaponController()
    {
        if (weaponController != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            weaponController = playerObject.GetComponent<PlayerWeaponController>();
            if (weaponController != null)
            {
                return;
            }
        }

        weaponController = FindAnyObjectByType<PlayerWeaponController>();
    }

    private void RefreshAmmoText(bool force)
    {
        if (ammoText == null)
        {
            return;
        }

        if (weaponController == null)
        {
            if (force)
            {
                ammoText.text = textPrefix + noWeaponText;
            }
            return;
        }

        Weapon currentWeapon = weaponController.GetCurrentWeapon();
        if (currentWeapon == null)
        {
            if (force || lastWeapon != null)
            {
                ammoText.text = textPrefix + noWeaponText;
                lastWeapon = null;
                lastMagazine = int.MinValue;
                lastReserve = int.MinValue;
            }
            return;
        }

        int magazine = currentWeapon.bulletsInMagazine;
        int reserve = currentWeapon.totalReserveAmmo;

        if (!force && currentWeapon == lastWeapon && magazine == lastMagazine && reserve == lastReserve)
        {
            return;
        }

        ammoText.text = textPrefix + magazine + "/" + reserve;
        lastWeapon = currentWeapon;
        lastMagazine = magazine;
        lastReserve = reserve;
    }
}
