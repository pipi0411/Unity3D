using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerAnimationEvents : MonoBehaviour
{
    private WeaponVisualController visualController;
    private PlayerWeaponController weaponController;
    private void Start()
    {
        visualController = GetComponentInParent<WeaponVisualController>();
        weaponController = GetComponentInParent<PlayerWeaponController>();
    }
    public void ReloadIsOver()
    {
        visualController.MaximizeRigWeight();
        weaponController.GetCurrentWeapon().RefillBullets();
        weaponController.OnReloadFinished();
    }
    public void ReturnRig()
    {
        visualController.MaximizeRigWeight();
        visualController.MaximizeLeftHandWeight();
    }
    public void WeaponGrabIsOver()
    {
        visualController.SetBusyEquippingWeaponTo(false);
    }
    public void SwitchOnWeaponModel()
    {
        visualController.SwitchOnCurrentWeaponModel();
    }
}
