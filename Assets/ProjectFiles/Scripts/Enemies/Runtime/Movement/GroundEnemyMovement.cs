using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyInstance))]
public class GroundEnemyMovement : MonoBehaviour, IEnemyMover
{
    private NavMeshAgent agent;
    private float rotationSpeed;

    public bool IsAtDestination => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        EnemyMovementData movement = GetComponent<EnemyInstance>().Data.movement;
        agent.speed = movement.moveSpeed;
        agent.acceleration = movement.acceleration;
        agent.angularSpeed = movement.rotationSpeed;
        rotationSpeed = movement.rotationSpeed;

        // Rotation is driven manually via FaceTarget so facing holds even after the agent stops moving.
        agent.updateRotation = false;
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

    public void SetSpeed(float speed)
    {
        agent.speed = speed;
    }

    public void FaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
