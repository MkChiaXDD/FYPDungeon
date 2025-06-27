using UnityEngine;
using System.Collections;

public class BomberEnemyController : Enemy
{
    enum State { Roam, Chase, Attack }

    [Header("Ranges & Forces")]
    [SerializeField] float roamRadius = 5f;
    [SerializeField] float chaseRange = 7f;
    [SerializeField] float explodeRange = 1.5f;
    [SerializeField] float explosionRadius = 3f;
    [SerializeField] float explosionForce = 500f;
    [SerializeField] float explosionGrowDuration = 1f;
    [SerializeField] float explodeGrowScale = 2f;
    [SerializeField] private float explosionUpwardModifier = 1f;

    [Header("Diff Scaling Settings")]
    [SerializeField] private int roundForScaling = 1;
    [SerializeField] private float explodingSizeMultiplier = 1.5f;
    private float currentExplosionRadius;

    [Header("Roam Settings")]
    [SerializeField] float roamDelay = 3f;

    [Header("Avoidance")]
    [SerializeField] float feelerLength = 2f;
    [SerializeField] float feelerRadius = 0.2f;
    [SerializeField] float avoidWeight = 5f;
    [SerializeField] LayerMask obstacleMask;

    [Header("Smoothing")]
    [SerializeField] float turnSpeed = 10f;
    [SerializeField] float smoothing = 5f;

    Vector3 spawnPosition;
    Vector3 roamTarget;
    float roamTimer;
    Transform player;
    Vector3 currentDir;
    State state;
    bool isExploding;
    public bool isPickedup = false;

    void Start()
    {
        spawnPosition = transform.position;
        player = GameObject.FindWithTag("Player").transform;
        state = State.Roam;
        ChooseRoamTarget();
        currentDir = transform.forward;

        if (currentRound < roundForScaling)
        {
            currentExplosionRadius = explosionRadius;
        }
        else
        {
            currentExplosionRadius = explosionRadius * explodingSizeMultiplier;
        }
    }

    void Update()
    {
        if (isPickedup) return;
        float distToPlayerXZ = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(player.position.x, 0, player.position.z)
        );

        if (state != State.Attack)
        {
            if (distToPlayerXZ <= explodeRange) state = State.Attack;
            else if (distToPlayerXZ <= chaseRange) state = State.Chase;
            else state = State.Roam;
        }

        switch (state)
        {
            case State.Roam:
                roamTimer += Time.deltaTime;
                MoveWithAvoidance(roamTarget);
                if (Vector3.Distance(transform.position, roamTarget) < 0.2f || roamTimer >= roamDelay)
                {
                    roamTimer = 0f;
                    ChooseRoamTarget();
                }
                break;

            case State.Chase:
                MoveWithAvoidance(player.position);
                break;

            case State.Attack:
                if (!isExploding)
                    StartCoroutine(ExplosionSequence());
                break;
        }
    }

    void MoveWithAvoidance(Vector3 target)
    {
        Vector3 toTarget = target - transform.position;
        toTarget.y = 0;
        Vector3 seekDir = toTarget.normalized;

        Vector3 avoidDir = Vector3.zero;
        Vector3[] feelers = {
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

        Vector3 desired = seekDir + avoidDir * avoidWeight;
        desired.y = 0;
        if (desired == Vector3.zero)
            desired = transform.forward;
        desired.Normalize();

        currentDir = Vector3.Slerp(currentDir, desired, smoothing * Time.deltaTime);

        transform.position += currentDir * data.moveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(currentDir),
            Time.deltaTime * turnSpeed
        );
    }

    IEnumerator ExplosionSequence()
    {
        isExploding = true;
        float t = 0f;
        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = initialScale * explodeGrowScale;

        while (t < explosionGrowDuration)
        {
            transform.localScale = Vector3.Lerp(initialScale, targetScale, t / explosionGrowDuration);
            t += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
        Explode();
    }

    void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player") && hit.attachedRigidbody != null)
            {
                Debug.Log("Explosion Radius: " + currentExplosionRadius);
                hit.attachedRigidbody.AddExplosionForce(
                    explosionForce,            // base force
                    transform.position,        // origin
                    currentExplosionRadius,           // radius
                    explosionUpwardModifier,   // upwards modifier
                    ForceMode.Impulse          // instant burst
                );

                if (hit.TryGetComponent<IDamageable>(out var dmg))
                    dmg.TakeDamage(data.damage);
            }
        }

        Destroy(gameObject);
    }


    void ChooseRoamTarget()
    {
        Vector2 rnd = Random.insideUnitCircle * roamRadius;
        roamTarget = new Vector3(
            spawnPosition.x + rnd.x,
            transform.position.y,
            spawnPosition.z + rnd.y
        );
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        // Show base explosion radius
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, currentExplosionRadius);
        }
#endif
    }

}
