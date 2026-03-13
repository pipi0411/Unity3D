using UnityEngine;

public class Item_Pickup : MonoBehaviour
{
    [SerializeField] private Weapon weapon; // Vũ khí mà người chơi sẽ nhặt được
    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<PlayerWeaponController>()?.PickupWeapon(weapon);
    }
}
