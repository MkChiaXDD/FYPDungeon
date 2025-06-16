using UnityEngine;

public class BomberEnemyController : Enemy
{
    enum State { Roam, Chase, Attack }
    State state;

    [Header("Ranges & Forces")]
    [SerializeField] float roamRadius = 5f;
    [SerializeField] float chaseRange = 7f;
    [SerializeField] float explodeRange = 1.5f;
    [SerializeField] float explosionRadius = 3f;
    [SerializeField] float explosionForce = 500f;

    [Header("Roam Settings")]
    [SerializeField] float roamDelay = 3f;

    Vector3 spawnPosition;
    Vector3 roamTarget;
    float roamTimer;
    Transform player;

    void Start()
    {
        spawnPosition = transform.position;
        player = GameObject.FindWithTag("Player").transform;
        state = State.Roam;
        ChooseRoamTarget();
    }

    void Update()
    {
        switch (state)
        {
            case State.Roam:
                RoamUpdate();
                break;
            case State.Chase:
                ChaseUpdate();
                break;
            case State.Attack:
                Explode();
                break;
        }
    }

    void RoamUpdate()
    {
        roamTimer += Time.deltaTime;
        MoveTowards(roamTarget);

        // pick new roam target when reached or timeout
        if (Vector3.Distance(transform.position, roamTarget) < 0.2f || roamTimer >= roamDelay)
        {
            roamTimer = 0f;
            ChooseRoamTarget();
        }

        // transition to chase
        if (Vector3.Distance(transform.position, player.position) <= chaseRange)
            state = State.Chase;
    }

    void ChaseUpdate()
    {
        MoveTowards(player.position);

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= explodeRange)
            state = State.Attack;
        else if (dist > chaseRange)
        {
            state = State.Roam;
            ChooseRoamTarget();
        }
    }

    void MoveTowards(Vector3 target)
    {
        // Create horizontal target (same Y as current position)
        Vector3 horizontalTarget = new Vector3(
            target.x,
            transform.position.y,  // Maintain current height
            target.z
        );

        Vector3 dir = (horizontalTarget - transform.position).normalized;
        transform.position += dir * data.moveSpeed * Time.deltaTime;
    }

    void ChooseRoamTarget()
    {
        Vector2 rnd = Random.insideUnitCircle * roamRadius;
        // Create roam target at current height
        roamTarget = new Vector3(
            spawnPosition.x + rnd.x,
            transform.position.y,  // Use current height
            spawnPosition.z + rnd.y
        );
    }

    void Explode()
    {
        // apply damage & knockback
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var dmg))
                dmg.TakeDamage(data.damage);

            if (hit.CompareTag("Player") && hit.attachedRigidbody != null)
                hit.attachedRigidbody.AddExplosionForce(
                    explosionForce,
                    transform.position,
                    explosionRadius
                );
        }

        Destroy(gameObject);
    }

    // (Optional) gizmos to visualize ranges in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, roamRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explodeRange);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}