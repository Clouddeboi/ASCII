using UnityEngine;

public interface IEnemyMover
{
    bool IsAtDestination { get; }
    void MoveTo(Vector3 destination);
    void Stop();
    void FaceTarget(Vector3 targetPosition);
}
