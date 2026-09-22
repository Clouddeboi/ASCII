using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyInstance))]
public class EnemyCombat : MonoBehaviour
{
    private List<AttackDefinition> attacks;
    private float[] cooldownRemaining;

    public bool HasAttacks => attacks != null && attacks.Count > 0;
    public bool IsAttacking { get; private set; }

    private void Awake()
    {
        attacks = GetComponent<EnemyInstance>().Data.combat.attacks;
        cooldownRemaining = new float[attacks.Count];

        if (attacks.Count == 0)
            Debug.LogWarning($"[EnemyCombat] '{name}' has an EnemyCombat component but no attacks configured in its EnemyData.", this);
    }

    private void Update()
    {
        for (int i = 0; i < cooldownRemaining.Length; i++)
        {
            if (cooldownRemaining[i] > 0f)
                cooldownRemaining[i] -= Time.deltaTime;
        }
    }

    public bool TryAttack(Transform target)
    {
        if (IsAttacking || target == null) return false;

        float distance = Vector3.Distance(transform.position, target.position);

        for (int i = 0; i < attacks.Count; i++)
        {
            if (cooldownRemaining[i] > 0f) continue;
            if (distance > attacks[i].range) continue;

            cooldownRemaining[i] = attacks[i].cooldown;
            StartCoroutine(PerformAttack(attacks[i], target));
            return true;
        }

        return false;
    }

    private IEnumerator PerformAttack(AttackDefinition attack, Transform target)
    {
        IsAttacking = true;

        if (attack.hitDelay > 0f)
            yield return new WaitForSeconds(attack.hitDelay);

        ResolveAttack(attack, target);
        IsAttacking = false;
    }

    private void ResolveAttack(AttackDefinition attack, Transform target)
    {
        switch (attack.type)
        {
            case AttackType.Ranged:
                SpawnProjectile(attack, target);
                break;

            case AttackType.AreaOfEffect:
                if (target != null && Vector3.Distance(transform.position, target.position) <= attack.aoeRadius)
                    PlayerHealth.Instance?.TakeDamage(attack.damage);
                break;

            case AttackType.Melee:
            default:
                if (target != null && Vector3.Distance(transform.position, target.position) <= attack.range * 1.5f)
                {
                    Debug.Log($"[EnemyCombat] '{name}' hit the player for {attack.damage} ({attack.damageType}).", this);
                    PlayerHealth.Instance?.TakeDamage(attack.damage);
                }
                break;
        }
    }

    private void SpawnProjectile(AttackDefinition attack, Transform target)
    {
        if (attack.projectilePrefab == null || target == null)
        {
            // No prefab configured, fall back to an instant hit so the attack still does something.
            PlayerHealth.Instance?.TakeDamage(attack.damage);
            return;
        }

        Vector3 direction = target.position - transform.position;
        GameObject instance = Instantiate(attack.projectilePrefab, transform.position, Quaternion.LookRotation(direction));
        EnemyProjectile projectile = instance.GetComponent<EnemyProjectile>();
        if (projectile == null)
            projectile = instance.AddComponent<EnemyProjectile>();

        projectile.Init(direction, attack.projectileSpeed, attack.damage, attack.damageType);
    }
}
