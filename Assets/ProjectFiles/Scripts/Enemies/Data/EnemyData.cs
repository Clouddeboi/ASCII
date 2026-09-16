using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyIdentityData
{
    [Tooltip("Internal identifier, never shown to the player (e.g. sentry_basic_01).")]
    public string internalId = "enemy_id";
    [Tooltip("Player-facing name used in /ping, /hack enemy, and terminal output.")]
    public string displayName = "Enemy";
    [TextArea]
    public string description;
    public EnemyCategory category = EnemyCategory.Hostile;
    public ThreatLevel threatLevel = ThreatLevel.Low;
}

[Serializable]
public struct DamageModifier
{
    public DamageType type;
    [Tooltip("0 = immune, <1 = resistant, 1 = normal, >1 = vulnerable.")]
    public float multiplier;
}

[Serializable]
public class EnemyHealthData
{
    public float maxHealth = 100f;
    public List<DamageModifier> damageModifiers = new List<DamageModifier>();
    [Tooltip("Cumulative damage required within a short window to trigger a stagger.")]
    public float staggerThreshold = 0f;
    public float knockbackForce = 0f;
    public DeathBehavior deathBehavior = DeathBehavior.Destroy;
}

[Serializable]
public class EnemyMovementData
{
    public MovementType movementType = MovementType.Ground;
    public float moveSpeed = 3.5f;
    public float acceleration = 8f;
    public float rotationSpeed = 120f;
    public float chaseSpeed = 5f;
    [Tooltip("Radius around the enemy's spawn point used for roaming when no patrol waypoints are assigned.")]
    public float roamRadius = 0f;
}

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "ASCII/Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public EnemyIdentityData identity = new EnemyIdentityData();
    public EnemyHealthData health = new EnemyHealthData();
    public EnemyMovementData movement = new EnemyMovementData();

    public float GetDamageMultiplier(DamageType type)
    {
        for (int i = 0; i < health.damageModifiers.Count; i++)
        {
            if (health.damageModifiers[i].type == type)
                return health.damageModifiers[i].multiplier;
        }

        return 1f;
    }
}
