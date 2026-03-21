using UnityEngine;

public class Item_Pickup : MonoBehaviour
{
    [SerializeField] private Weapon weapon; // Vũ khí mà người chơi sẽ nhặt được
    [SerializeField] private GameObject objectToRemoveOnPickup;

    public Weapon PickupWeaponTemplate => weapon;

    public string GetSaveId()
    {
        Transform target = objectToRemoveOnPickup != null ? objectToRemoveOnPickup.transform : transform;
        return BuildHierarchyPath(target);
    }

    public void RemoveForLoadState()
    {
        Destroy(objectToRemoveOnPickup != null ? objectToRemoveOnPickup : gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerWeaponController playerWeaponController = other.GetComponent<PlayerWeaponController>();
        if (playerWeaponController == null)
        {
            return;
        }

        bool pickedUpSuccessfully = playerWeaponController.PickupWeapon(weapon);
        if (pickedUpSuccessfully)
        {
            Destroy(objectToRemoveOnPickup != null ? objectToRemoveOnPickup : gameObject);
        }
    }

    private static string BuildHierarchyPath(Transform target)
    {
        string path = target.name;
        Transform current = target.parent;

        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return path;
    }
}
