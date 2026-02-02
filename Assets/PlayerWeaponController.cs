using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    private Player player;
    private void Start()
    {
        player = GetComponent<Player>();
        player.controls.Character.Fire.performed += ctx => Shoot();
    }
    private void Shoot()
    {
        Debug.Log("Pew Pew");
        var animator = GetComponentInChildren<Animator>();
        animator.SetTrigger("Fire");
    }
}
