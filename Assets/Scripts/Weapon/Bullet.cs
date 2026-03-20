using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private GameObject bulletImpactFX;
    private Rigidbody rb => GetComponent<Rigidbody>();
    private float damage;

    public void SetDamage(float value)
    {
        damage = Mathf.Max(0f, value);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryApplyDamage(collision);
        CreateImpactFx(collision);
        ObjectPool.Instance.ReturnBullet(gameObject);
    }

    private void TryApplyDamage(Collision collision)
    {
        if (collision.collider == null)
        {
            return;
        }

        IDamageable damageable = collision.collider.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }
    }

    private void CreateImpactFx(Collision collision)
    {
        if (collision.contacts.Length > 0)
        {
            ContactPoint contact = collision.contacts[0];
            GameObject newImpactFX = Instantiate(bulletImpactFX, contact.point, Quaternion.LookRotation(contact.normal));
            Destroy(newImpactFX, 1f);
        }
    }
}
