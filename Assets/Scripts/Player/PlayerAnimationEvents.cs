using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerAnimationEvents : MonoBehaviour
{
    private WeaponVisualController visualController;
    private PlayerWeaponController weaponController;

    private void Start()
    {
        CacheReferences();
    }

    private void CacheReferences()
    {
        if (visualController == null)
        {
            visualController = GetComponentInParent<WeaponVisualController>();
        }

        if (weaponController == null)
        {
            weaponController = GetComponentInParent<PlayerWeaponController>();
        }
    }

    public void ReloadIsOver()
    {
        CacheReferences();

        if (visualController != null)
        {
            visualController.MaximizeRigWeight();
        }

        if (weaponController == null)
        {
            return;
        }

        Weapon currentWeapon = weaponController.GetCurrentWeapon();
        if (currentWeapon != null)
        {
            currentWeapon.RefillBullets();
        }

        weaponController.OnReloadFinished();
    }

    public void ReturnRig()
    {
        CacheReferences();
        if (visualController == null)
        {
            return;
        }

        visualController.MaximizeRigWeight();
        visualController.MaximizeLeftHandWeight();
    }

    public void WeaponGrabIsOver()
    {
        CacheReferences();
        if (visualController == null)
        {
            return;
        }

        visualController.SetBusyEquippingWeaponTo(false);
    }

    public void SwitchOnWeaponModel()
    {
        CacheReferences();
        if (visualController == null)
        {
            return;
        }

        visualController.SwitchOnCurrentWeaponModel();
    }
}
