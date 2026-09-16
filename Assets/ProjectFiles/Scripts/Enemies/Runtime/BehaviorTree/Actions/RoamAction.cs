using UnityEngine;

//Wanders to random points within roamRadius of the enemy's spawn position, fails when radius is 0.
public class RoamAction : BTNode
{
    private readonly EnemyBrain brain;

    public RoamAction(EnemyBrain brain)
    {
        this.brain = brain;
    }

    public override NodeStatus Tick()
    {
        float radius = brain.MovementData.roamRadius;
        if (radius <= 0f)
            return NodeStatus.Failure;

        brain.Mover.SetSpeed(brain.MovementData.moveSpeed);

        if (!brain.RoamTarget.HasValue || brain.Mover.IsAtDestination)
        {
            Vector2 offset = Random.insideUnitCircle * radius;
            Vector3 point = brain.RoamOrigin + new Vector3(offset.x, 0f, offset.y);
            brain.RoamTarget = point;
            brain.Mover.MoveTo(point);
        }

        brain.Mover.FaceTarget(brain.RoamTarget.Value);
        return NodeStatus.Running;
    }
}
