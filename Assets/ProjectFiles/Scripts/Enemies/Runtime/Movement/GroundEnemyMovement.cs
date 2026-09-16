using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyInstance))]
public class GroundEnemyMovement : MonoBehaviour, IEnemyMover
{
    private NavMeshAgent agent;

    public bool IsAtDestination => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        EnemyMovementData movement = GetComponent<EnemyInstance>().Data.movement;
        agent.speed = movement.moveSpeed;
        agent.acceleration = movement.acceleration;
        agent.angularSpeed = movement.rotationSpeed;
    }

    public void MoveTo(Vector3 destination)
    {
        if (agent.isOnNavMesh)
            agent.SetDestination(destination);
    }

    public void Stop()
    {
        if (agent.isOnNavMesh)
            agent.ResetPath();
    }

    public void FaceTarget(Vector3 targetPosition)
    {
        // NavMeshAgent already turns the enemy along its path toward the destination.
    }
}
