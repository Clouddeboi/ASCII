using UnityEngine;

[RequireComponent(typeof(EnemyInstance))]
public class EnemyBrain : MonoBehaviour
{
    [Tooltip("Optional patrol route. If empty, the enemy roams instead (when roamRadius > 0), or idles.")]
    [SerializeField] private Transform[] patrolWaypoints;
    [SerializeField] private float tickInterval = 0.2f;

    private IEnemyMover mover;
    private EnemyData data;
    private EnemyAnimationController animationController;
    private EnemyDetection detection;
    private BTNode root;
    private float tickTimer;

    public IEnemyMover Mover => mover;
    public EnemyData Data => data;
    public EnemyMovementData MovementData => data.movement;
    public Transform[] PatrolWaypoints => patrolWaypoints;
    public Vector3 RoamOrigin { get; private set; }
    public int PatrolIndex { get; set; } = -1;
    public Vector3? RoamTarget { get; set; }
    public bool IsMoving { get; set; }
    public Vector3? FaceTargetPosition { get; set; }

    private void Awake()
    {
        data = GetComponent<EnemyInstance>().Data;
        mover = GetComponent<IEnemyMover>();
        animationController = GetComponent<EnemyAnimationController>();
        RoamOrigin = transform.position;

        if (mover == null)
            Debug.LogError($"[EnemyBrain] '{name}' has no IEnemyMover component (Static/Ground/Flying movement).", this);

        detection = GetComponent<EnemyDetection>();
        EnemyCombat combat = GetComponent<EnemyCombat>();

        if (combat != null && detection == null)
            Debug.LogWarning($"[EnemyBrain] '{name}' has EnemyCombat but no EnemyDetection - it will never attack.", this);

        if (combat != null && data.identity.category != EnemyCategory.Hostile)
            Debug.LogWarning($"[EnemyBrain] '{name}' has EnemyCombat but its category is {data.identity.category}, not Hostile - Chase/Attack are Hostile-only.", this);

        if (detection != null && combat != null)
        {
            root = new Selector(
                new AttackAction(this, detection, combat),
                new ChaseAction(this, detection),
                new InvestigateAction(this, detection),
                new PatrolAction(this),
                new RoamAction(this),
                new IdleAction(this, detection));
        }
        else if (detection != null)
        {
            root = new Selector(
                new ChaseAction(this, detection),
                new InvestigateAction(this, detection),
                new PatrolAction(this),
                new RoamAction(this),
                new IdleAction(this, detection));
        }
        else
        {
            root = new Selector(
                new PatrolAction(this),
                new RoamAction(this),
                new IdleAction(this, null));
        }
    }

    private void Update()
    {
        if (mover == null) return;

        if (detection != null && detection.PlayerTransform != null)
            animationController?.SetDistanceToPlayer(Vector3.Distance(transform.position, detection.PlayerTransform.position));

        // Applied every frame (not gated by tickInterval) so rotation speed isn't throttled down to the BT's tick rate.
        if (FaceTargetPosition.HasValue)
            mover.FaceTarget(FaceTargetPosition.Value);

        tickTimer += Time.deltaTime;
        if (tickTimer < tickInterval) return;
        tickTimer = 0f;

        root.Tick();
        animationController?.SetMoving(IsMoving);
    }
}
