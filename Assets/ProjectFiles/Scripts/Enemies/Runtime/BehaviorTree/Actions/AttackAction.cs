//Hostile enemies attack once alerted (seen or heard) and within one of their configured attack ranges.
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

        if (combat.IsAttacking)
        {
            brain.Mover.Stop();
            brain.Mover.FaceTarget(detection.PlayerTransform.position);
            return NodeStatus.Running;
        }

        brain.Mover.FaceTarget(detection.PlayerTransform.position);

        if (!combat.TryAttack(detection.PlayerTransform))
            return NodeStatus.Failure;

        brain.Mover.Stop();
        return NodeStatus.Running;
    }
}
