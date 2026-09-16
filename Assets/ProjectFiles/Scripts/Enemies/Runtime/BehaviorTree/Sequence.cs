// Ticks children in order, returning the first result that isn't a success.
public class Sequence : BTNode
{
    private readonly BTNode[] children;

    public Sequence(params BTNode[] children)
    {
        this.children = children;
    }

    public override NodeStatus Tick()
    {
        foreach (BTNode child in children)
        {
            NodeStatus status = child.Tick();
            if (status != NodeStatus.Success)
                return status;
        }

        return NodeStatus.Success;
    }
}
