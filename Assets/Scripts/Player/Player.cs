using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerControls controls { get; private set; }
    public PlayerMovement movement { get; private set; }
    public PlayerWeaponController weaponController { get; private set; }
    public WeaponVisualController weaponVisualController { get; private set; }

    private void Awake()
    {
        controls = new PlayerControls();
        movement = GetComponent<PlayerMovement>();
        weaponController = GetComponent<PlayerWeaponController>();
        weaponVisualController = GetComponentInChildren<WeaponVisualController>();
    }
    private void OnEnable()
    {
        controls.Enable();
    }
    private void OnDisable()
    {
        controls.Disable();
    }
}
