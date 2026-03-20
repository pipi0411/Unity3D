using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerControls controls { get; private set; }
    public PlayerMovement movement { get; private set; }
    public PlayerWeaponController weaponController { get; private set; }
    public WeaponVisualController weaponVisualController { get; private set; }
    public PlayerHealth health { get; private set; }
    public PlayerCoins coins { get; private set; }

    private void Awake()
    {
        controls = new PlayerControls();
        movement = GetComponent<PlayerMovement>();
        weaponController = GetComponent<PlayerWeaponController>();
        weaponVisualController = GetComponentInChildren<WeaponVisualController>();
        health = GetComponent<PlayerHealth>();
        coins = GetComponent<PlayerCoins>();

        if (coins == null)
        {
            coins = gameObject.AddComponent<PlayerCoins>();
        }
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
