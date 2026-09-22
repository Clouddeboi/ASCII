using UnityEngine;

[RequireComponent(typeof(EnemyInstance))]
public class FlyingEnemyMovement : MonoBehaviour, IEnemyMover
{
    [SerializeField] private float stoppingDistance = 0.5f;

    private EnemyInstance enemyInstance;
    private Vector3 destination;
    private bool hasDestination;
    private float currentSpeed;

    public bool IsAtDestination => !hasDestination || Vector3.Distance(transform.position, destination) <= stoppingDistance;

    private void Awake()
    {
        enemyInstance = GetComponent<EnemyInstance>();
        destination = transform.position;
        currentSpeed = enemyInstance.Data.movement.moveSpeed;
    }

    private void Update()
    {
        if (!hasDestination) return;

        transform.position = Vector3.MoveTowards(transform.position, destination, currentSpeed * Time.deltaTime);
        FaceTarget(destination);

        if (IsAtDestination)
            hasDestination = false;
    }

    public void MoveTo(Vector3 targetDestination)
    {
        destination = targetDestination;
        hasDestination = true;
    }

    public void Stop()
    {
        hasDestination = false;
    }

    public void SetSpeed(float speed)
    {
        currentSpeed = speed;
    }

    public void FaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        if (direction.sqrMagnitude <= 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        float rotationSpeed = enemyInstance.Data.movement.rotationSpeed;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
