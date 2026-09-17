public enum EnemyCategory
{
    Passive,
    Neutral,
    Hostile
}

public enum ThreatLevel
{
    Low,
    Medium,
    High,
    Critical
}

public enum DamageType
{
    Physical,
    Electrical,
    Fire,
    Explosive,
    Energy
}

public enum DeathBehavior
{
    Destroy,
    Disable,
    PlayAnimationThenDestroy
}

public enum MovementType
{
    Static,
    Ground,
    Flying
}

public enum AttackType
{
    Melee,
    Ranged,
    AreaOfEffect
}

public enum HackSuccessResponse
{
    Stun,
    Kill,
    ShutDown
}

public enum HackFailureResponse
{
    RevealLocation,
    AlertNearbyEnemies,
    TriggerAlarm
}
