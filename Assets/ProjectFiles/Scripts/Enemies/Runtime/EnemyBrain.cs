using UnityEngine;

[RequireComponent(typeof(EnemyInstance))]
public class EnemyBrain : MonoBehaviour
{
    [Tooltip("Optional patrol route. If empty, the enemy roams instead (when roamRadius > 0), or idles.")]
    [SerializeField] private Transform[] patrolWaypoints;
    [SerializeField] private float tickInterval = 0.2f;

    private IEnemyMover mover;
    private EnemyData data;
    private BTNode root;
    private float tickTimer;

    public IEnemyMover Mover => mover;
    public EnemyMovementData MovementData => data.movement;
    public Transform[] PatrolWaypoints => patrolWaypoints;
    public Vector3 RoamOrigin { get; private set; }
    public int PatrolIndex { get; set; } = -1;
    public Vector3? RoamTarget { get; set; }

    private void Awake()
    {
        data = GetComponent<EnemyInstance>().Data;
        mover = GetComponent<IEnemyMover>();
        RoamOrigin = transform.position;

        if (mover == null)
            Debug.LogError($"[EnemyBrain] '{name}' has no IEnemyMover component (Static/Ground/Flying movement).", this);

        root = new Selector(
            new PatrolAction(this),
            new RoamAction(this),
            new IdleAction()
        );
    }

    private void Update()
    {
        if (mover == null) return;

        tickTimer += Time.deltaTime;
        if (tickTimer < tickInterval) return;
        tickTimer = 0f;

        root.Tick();
    }
}
