using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private DamageType damageType = DamageType.Physical;
    [SerializeField] private float attackCooldown = 0.6f;
    [SerializeField] private LayerMask enemyLayer = ~0;

    private float cooldownRemaining;

    private void Awake()
    {
        if (attackOrigin == null)
            Debug.LogWarning("[PlayerCombat] attackOrigin is not assigned - attacks will never hit anything.", this);
    }

    private void Update()
    {
        if (cooldownRemaining > 0f)
            cooldownRemaining -= Time.deltaTime;

        if (InputManager.Instance != null && InputManager.Instance.AttackAction.WasPressedThisFrame())
            TryAttack();
    }

    private void TryAttack()
    {
        if (cooldownRemaining > 0f || attackOrigin == null) return;

        if (Physics.Raycast(attackOrigin.position, attackOrigin.forward, out RaycastHit hit, attackRange, enemyLayer))
        {
            Debug.Log($"[PlayerCombat] Raycast hit '{hit.collider.name}'.", this);

            EnemyHealth enemyHealth = hit.collider.GetComponentInParent<EnemyHealth>();
            if (enemyHealth == null)
            {
                Debug.Log($"[PlayerCombat] '{hit.collider.name}' has no EnemyHealth in its parent hierarchy.", this);
                return;
            }

            enemyHealth.TakeDamage(attackDamage, damageType);
            cooldownRemaining = attackCooldown;
        }
        else
        {
            Debug.Log("[PlayerCombat] Attack raycast hit nothing.", this);
        }
    }
}
