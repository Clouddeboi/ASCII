using UnityEngine;

public class StaticEnemyMovement : MonoBehaviour, IEnemyMover
{
    public bool IsAtDestination => true;

    public void MoveTo(Vector3 destination)
    {
        // Static enemies never relocate.
    }

    public void Stop()
    {
    }

    public void SetSpeed(float speed)
    {
    }

    public void FaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(direction);
    }
}
