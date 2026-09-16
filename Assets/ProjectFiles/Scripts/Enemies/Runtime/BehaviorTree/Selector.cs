// Ticks children in order, returning the first result that isn't a failure.
public class Selector : BTNode
{
    private readonly BTNode[] children;

    public Selector(params BTNode[] children)
    {
        this.children = children;
    }

    public override NodeStatus Tick()
    {
        foreach (BTNode child in children)
        {
            NodeStatus status = child.Tick();
            if (status != NodeStatus.Failure)
                return status;
        }

        return NodeStatus.Failure;
    }
}
