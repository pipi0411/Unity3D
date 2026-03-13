using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerAnimationEvents : MonoBehaviour
{
    private WeaponVisualController visualController;
    private void Start()
    {
        visualController = GetComponentInParent<WeaponVisualController>();
    }
    public void ReloadIsOver()
    {
        visualController.MaximizeRigWeight();
    }
    public void ReturnRig()
    {
        visualController.MaximizeRigWeight();
        visualController.MaximizeLeftHandWeight();
    }
    public void WeaponGrabIsOver()
    {
        visualController.SetBusyGrabbingWeaponTo(false);
    }
}
