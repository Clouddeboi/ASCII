//Hostile enemies attack once alerted (seen or heard) and within one of their configured attack ranges.
using UnityEngine;

public class AttackAction : BTNode
{
    private readonly EnemyBrain brain;
    private readonly EnemyDetection detection;
    private readonly EnemyCombat combat;

    public AttackAction(EnemyBrain brain, EnemyDetection detection, EnemyCombat combat)
    {
        this.brain = brain;
        this.detection = detection;
        this.combat = combat;
    }

    public override NodeStatus Tick()
    {
        if (brain.Data.identity.category != EnemyCategory.Hostile)
            return NodeStatus.Failure;

        if (!detection.IsAlerted || detection.PlayerTransform == null)
            return NodeStatus.Failure;

        Transform playerTarget = detection.PlayerTransform;

        // Currently actively performing an attack animation
        if (combat.IsAttacking)
        {
            if (combat.CurrentAttackStopsMovement)
                brain.Mover.Stop();
            brain.FaceTargetPosition = playerTarget.position;
            brain.IsMoving = !combat.CurrentAttackStopsMovement;
            return NodeStatus.Running;
        }

        // Try to trigger an attack if in range and off cooldown
        if (combat.TryAttack(playerTarget))
        {
            if (combat.CurrentAttackStopsMovement)
                brain.Mover.Stop();
            brain.FaceTargetPosition = playerTarget.position;
            brain.IsMoving = !combat.CurrentAttackStopsMovement;
            return NodeStatus.Running;
        }

        // Reached the player (in attack range), but attack is currently on cooldown
        if (combat.IsInRange(playerTarget))
        {
            brain.FaceTargetPosition = playerTarget.position;

            // Hold position facing player while waiting for cooldown instead of running/sliding into them
            if (combat.CurrentAttackStopsMovement)
            {
                brain.Mover.Stop();
                brain.IsMoving = false;
                return NodeStatus.Running;
            }
        }

        // Out of attack range, or mobile attack: fall through to ChaseAction
        return NodeStatus.Failure;
    }
}
