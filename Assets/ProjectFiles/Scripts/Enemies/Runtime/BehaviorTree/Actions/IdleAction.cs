using UnityEngine;

public class IdleAction : BTNode
{
    private readonly EnemyBrain brain;
    private readonly EnemyDetection detection;

    public IdleAction(EnemyBrain brain, EnemyDetection detection)
    {
        this.brain = brain;
        this.detection = detection;
    }

    public override NodeStatus Tick()
    {
        brain.IsMoving = false;

        // Even enemies that won't chase/attack (Passive/Neutral, or no combat) still turn to face a detected player.
        brain.FaceTargetPosition = (detection != null && detection.IsAlerted && detection.PlayerTransform != null)
            ? detection.PlayerTransform.position
            : (Vector3?)null;

        return NodeStatus.Success;
    }
}

