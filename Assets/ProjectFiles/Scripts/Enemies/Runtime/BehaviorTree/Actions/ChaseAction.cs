//Hostile enemies pursue the player while alerted, whether they're currently seen or only heard/remembered.
public class ChaseAction : BTNode
{
    private readonly EnemyBrain brain;
    private readonly EnemyDetection detection;

    public ChaseAction(EnemyBrain brain, EnemyDetection detection)
    {
        this.brain = brain;
        this.detection = detection;
    }

    public override NodeStatus Tick()
    {
        if (brain.Data.identity.category != EnemyCategory.Hostile)
            return NodeStatus.Failure;

        if (!detection.IsAlerted || !detection.LastKnownPlayerPosition.HasValue)
            return NodeStatus.Failure;

        brain.Mover.SetSpeed(brain.MovementData.chaseSpeed);
        brain.Mover.MoveTo(detection.LastKnownPlayerPosition.Value);
        brain.Mover.FaceTarget(detection.LastKnownPlayerPosition.Value);
        return NodeStatus.Running;
    }
}

