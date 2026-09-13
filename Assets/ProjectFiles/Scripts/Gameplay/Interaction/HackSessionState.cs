//Tracks an interactable currently awaiting a "/code <code>" response from /hack.
public static class HackSessionState
{
    public static IInteractable PendingTarget { get; private set; }

    public static bool HasPendingChallenge => PendingTarget != null;

    public static void BeginCodeChallenge(IInteractable target)
    {
        PendingTarget = target;
    }

    public static void Clear()
    {
        PendingTarget = null;
    }
}
