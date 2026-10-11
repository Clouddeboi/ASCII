public class IdleAction : BTNode
{
    private readonly EnemyBrain brain;

    public IdleAction(EnemyBrain brain)
    {
        this.brain = brain;
    }

    public override NodeStatus Tick()
    {
        brain.IsMoving = false;
        return NodeStatus.Success;
    }
}
