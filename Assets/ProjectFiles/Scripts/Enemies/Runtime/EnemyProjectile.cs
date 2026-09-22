using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;

    private Vector3 direction;
    private float speed;
    private float damage;
    private DamageType damageType;

    public void Init(Vector3 travelDirection, float projectileSpeed, float projectileDamage, DamageType type)
    {
        direction = travelDirection.normalized;
        speed = projectileSpeed;
        damage = projectileDamage;
        damageType = type;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null) return;

        playerHealth.TakeDamage(damage);
        Destroy(gameObject);
    }
}
