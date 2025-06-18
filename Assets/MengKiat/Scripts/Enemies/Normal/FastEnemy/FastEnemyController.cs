using UnityEngine;

public class FastEnemyController : Enemy
{
    [SerializeField] private float chaseRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Avoidance")]
    [SerializeField] private float feelerLength = 10f;
    [SerializeField] private float feelerRadius = 0.2f;
    [SerializeField] private float avoidWeight = 200f;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Smoothing")]
    [SerializeField] private float turnSpeed = 30f;
    [SerializeField, Tooltip("Higher = snappier, Lower = smoother")]
    private float smoothing = 1f;

    private Transform player;
    private float attackTimer;
    private Vector3 currentDir;

    private enum State { Idle, Chase, Attack }
    private State state;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        state = State.Idle;
        currentDir = transform.forward;
    }

    void Update()
    {
        // distance only on XZ
        float dist = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(player.position.x, 0, player.position.z)
        );

        if (dist <= attackRange) state = State.Attack;
        else if (dist <= chaseRange) state = State.Chase;
        else state = State.Idle;

        switch (state)
        {
            case State.Idle:
                attackTimer = attackCooldown;
                break;
            case State.Chase:
                attackTimer = attackCooldown;
                ChaseWithAvoidance();
                break;
            case State.Attack:
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0f)
                {
                    Attack();
                    attackTimer = attackCooldown;
                }
                break;
        }
    }

    private void ChaseWithAvoidance()
    {
        // 1) pure XZ seek
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0;
        Vector3 seekDir = toPlayer.normalized;

        // 2) avoidance with SphereCasts
        Vector3 avoidDir = Vector3.zero;
        Vector3[] feelers = new Vector3[]
        {
            transform.forward,
            (transform.forward + transform.right).normalized,
            (transform.forward - transform.right).normalized
        };

        foreach (var f in feelers)
        {
            Vector3 dir = f;
            dir.y = 0;
            dir.Normalize();

            if (Physics.SphereCast(transform.position, feelerRadius, dir, out RaycastHit hit, feelerLength, obstacleMask))
            {
                Vector3 n = hit.normal;
                n.y = 0;
                n.Normalize();
                float strength = (feelerLength - hit.distance) / feelerLength;
                avoidDir += n * strength;
            }
        }

        // 3) desired
        Vector3 desired = seekDir + avoidDir * avoidWeight;
        desired.y = 0;
        if (desired == Vector3.zero) desired = transform.forward;
        desired.Normalize();

        // 4) smooth
        currentDir = Vector3.Slerp(currentDir, desired, smoothing * Time.deltaTime);

        // 5) move & rotate
        transform.position += currentDir * data.moveSpeed * Time.deltaTime;
        Quaternion targetRot = Quaternion.LookRotation(currentDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
    }

    private void Attack()
    {
        Debug.Log("NORMALENEMY: Attack!");
        // …your attack logic…
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3[] feelers = new Vector3[]
        {
            transform.forward,
            (transform.forward + transform.right).normalized,
            (transform.forward - transform.right).normalized
        };

        foreach (var f in feelers)
        {
            Vector3 dir = f;
            dir.y = 0;
            dir.Normalize();
            Gizmos.DrawLine(transform.position, transform.position + dir * feelerLength);
        }

        // draw a little sphere at origin to show radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, feelerRadius);
    }
}
