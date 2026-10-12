using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyInstance))]
public class EnemyCombat : MonoBehaviour
{
    private List<AttackDefinition> attacks;
    private float[] cooldownRemaining;
    private EnemyAnimationController animationController;
    private EnemyAudio enemyAudio;

    public bool HasAttacks => attacks != null && attacks.Count > 0;
    public bool IsAttacking { get; private set; }
    public bool CurrentAttackStopsMovement { get; private set; } = true;

    private void Awake()
    {
        attacks = GetComponent<EnemyInstance>().Data.combat.attacks;
        cooldownRemaining = new float[attacks.Count];
        animationController = GetComponent<EnemyAnimationController>();
        enemyAudio = GetComponent<EnemyAudio>();

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

    public float GetEffectiveDistance(Transform target)
    {
        if (target == null) return float.MaxValue;
        Vector3 diff = target.position - transform.position;
        float horizontalDistance = new Vector2(diff.x, diff.z).magnitude;
        float verticalDiff = Mathf.Abs(diff.y);

        // Target on a higher/lower ledge: include vertical difference if outside normal height range
        if (verticalDiff > 2.5f)
            return Mathf.Sqrt(horizontalDistance * horizontalDistance + verticalDiff * verticalDiff);

        return horizontalDistance;
    }

    public bool IsInRange(Transform target)
    {
        if (target == null || attacks == null || attacks.Count == 0) return false;
        float distance = GetEffectiveDistance(target);
        for (int i = 0; i < attacks.Count; i++)
        {
            if (distance <= attacks[i].range)
                return true;
        }
        return false;
    }

    public bool TryAttack(Transform target)
    {
        if (IsAttacking || target == null || attacks == null) return false;

        float distance = GetEffectiveDistance(target);

        // Picks the most appropriate attack in range (smallest range that still covers the distance), not just the first match.
        int bestIndex = -1;
        float bestRange = float.MaxValue;

        for (int i = 0; i < attacks.Count; i++)
        {
            if (cooldownRemaining[i] > 0f) continue;
            if (distance > attacks[i].range) continue;
            if (attacks[i].range >= bestRange) continue;

            bestRange = attacks[i].range;
            bestIndex = i;
        }

        if (bestIndex < 0) return false;

        cooldownRemaining[bestIndex] = attacks[bestIndex].cooldown;
        CurrentAttackStopsMovement = attacks[bestIndex].stopsMovement;
        StartCoroutine(PerformAttack(bestIndex, target));
        return true;
    }

    private IEnumerator PerformAttack(int attackIndex, Transform target)
    {
        AttackDefinition attack = attacks[attackIndex];

        IsAttacking = true;
        animationController?.PlayAttack(attack.animationTrigger, attackIndex);
        enemyAudio?.PlayAttack();

        float totalDuration = Mathf.Max(attack.attackDuration, attack.hitDelay);

        if (attack.hitDelay > 0f)
            yield return new WaitForSeconds(attack.hitDelay);

        ResolveAttack(attack, target);

        float remainingDuration = totalDuration - attack.hitDelay;
        if (remainingDuration > 0f)
            yield return new WaitForSeconds(remainingDuration);

        IsAttacking = false;
    }

    private void ResolveAttack(AttackDefinition attack, Transform target)
    {
        float distance = GetEffectiveDistance(target);

        switch (attack.type)
        {
            case AttackType.Ranged:
                SpawnProjectile(attack, target);
                break;

            case AttackType.AreaOfEffect:
                if (target != null && distance <= attack.aoeRadius)
                    PlayerHealth.Instance?.TakeDamage(attack.damage);
                break;

            case AttackType.Melee:
            default:
                if (target != null && distance <= attack.range * 1.5f)
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
