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

[Serializable]
public class AttackDefinition
{
    public string attackName = "Attack";
    public AttackType type = AttackType.Melee;
    public float range = 2f;
    public float cooldown = 1.5f;
    public float damage = 10f;
    public DamageType damageType = DamageType.Physical;
    [Tooltip("Seconds after the attack starts before damage is applied, to line up with an animation's hit frame.")]
    public float hitDelay = 0.3f;
    [Tooltip("Animator trigger name, used once an animation controller is wired up in a later stage.")]
    public string animationTrigger;
    [Tooltip("Used only when type is AreaOfEffect.")]
    public float aoeRadius = 3f;
    [Tooltip("Used only when type is Ranged. If left empty, the attack falls back to an instant hit.")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 12f;
}

[Serializable]
public class EnemyCombatData
{
    public List<AttackDefinition> attacks = new List<AttackDefinition>();
}

[Serializable]
public class EnemyDetectionData
{
    public bool useVision = true;
    public bool useHearing = false;
    public float visionRange = 15f;
    [Range(0f, 360f)] public float fieldOfViewAngle = 110f;
    public LayerMask lineOfSightObstruction = ~0;
    public float hearingRange = 8f;
    [Tooltip("Player flat movement speed required to be heard at hearingRange.")]
    public float hearingSensitivity = 2f;
    [Tooltip("Seconds of continuous line of sight required before the enemy becomes alerted.")]
    public float detectionDelay = 0.5f;
    [Tooltip("How long the enemy keeps investigating a last known position after losing track of the player.")]
    public float searchMemoryDuration = 5f;
}

[Serializable]
public class EnemyTerminalData
{
    [Tooltip("Whether /ping can reveal this enemy's display name and count.")]
    public bool pingable = true;
    [Tooltip("Whether automatic anomaly detection can flag this enemy's presence.")]
    public bool anomalyDetectable = true;
    [Tooltip("Danger level reported by /ping and anomaly detection for this enemy.")]
    public ThreatLevel dangerClassification = ThreatLevel.Low;
    [Tooltip("How far the terminal's scanner can detect this specific enemy (independent of the terminal's own scan radius).")]
    public float terminalDetectionRange = 20f;
    [Tooltip("Reserved for a future terminal-upgrade requirement - not yet enforced.")]
    public string requiresUpgradeId;

    [Header("Hacking")]
    public bool hackable = false;
    [Tooltip("Hacking range - no line of sight required.")]
    public float hackRange = 10f;
    [Range(0f, 1f)] public float hackSuccessRate = 0.5f;
    [Tooltip("Item ID required in the player's inventory to attempt this hack (e.g. a hacking tool). Leave empty for none.")]
    public string requiredItemId;
    public HackSuccessResponse successResponse = HackSuccessResponse.Stun;
    [Tooltip("Used only when successResponse is Stun.")]
    public float stunDuration = 5f;
    public HackFailureResponse failureResponse = HackFailureResponse.RevealLocation;
}

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "ASCII/Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public EnemyIdentityData identity = new EnemyIdentityData();
    public EnemyHealthData health = new EnemyHealthData();
    public EnemyMovementData movement = new EnemyMovementData();
    public EnemyDetectionData detection = new EnemyDetectionData();
    public EnemyCombatData combat = new EnemyCombatData();
    public EnemyTerminalData terminal = new EnemyTerminalData();

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
