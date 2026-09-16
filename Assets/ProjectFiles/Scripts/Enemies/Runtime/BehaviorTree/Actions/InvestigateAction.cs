//Investigates the last known player position (from sight or sound) while the enemy is alerted but can't currently see the player.
public class InvestigateAction : BTNode
{
    private readonly EnemyBrain brain;
    private readonly EnemyDetection detection;

    public InvestigateAction(EnemyBrain brain, EnemyDetection detection)
    {
        this.brain = brain;
        this.detection = detection;
    }

    public override NodeStatus Tick()
    {
        if (!detection.IsAlerted || detection.CanSeePlayer || !detection.LastKnownPlayerPosition.HasValue)
            return NodeStatus.Failure;

        brain.Mover.SetSpeed(brain.MovementData.moveSpeed);
        brain.Mover.MoveTo(detection.LastKnownPlayerPosition.Value);
        brain.Mover.FaceTarget(detection.LastKnownPlayerPosition.Value);
        return NodeStatus.Running;
    }
}
