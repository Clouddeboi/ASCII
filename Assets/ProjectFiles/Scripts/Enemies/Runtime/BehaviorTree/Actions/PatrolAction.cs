// Cycles through the brain's patrol waypoints; fails (falls through) when none are assigned.
public class PatrolAction : BTNode
{
    private readonly EnemyBrain brain;

    public PatrolAction(EnemyBrain brain)
    {
        this.brain = brain;
    }

    public override NodeStatus Tick()
    {
        var waypoints = brain.PatrolWaypoints;
        if (waypoints == null || waypoints.Length == 0)
            return NodeStatus.Failure;

        if (brain.PatrolIndex < 0 || brain.Mover.IsAtDestination)
        {
            brain.PatrolIndex = (brain.PatrolIndex + 1) % waypoints.Length;
            brain.Mover.MoveTo(waypoints[brain.PatrolIndex].position);
        }

        return NodeStatus.Running;
    }
}
