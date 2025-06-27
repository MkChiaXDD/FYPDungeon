using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class TankEnemyController : Enemy
{
    [SerializeField] private float chaseRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Diff Scaling Settings")]
    [SerializeField] private float rushToBomberSpeedMultiplier = 5f;
    [SerializeField] private Transform carryZone;
    private bool isCarrying;

    [Header("Avoidance")]
    [SerializeField] private float feelerLength = 2f;
    [SerializeField] private float feelerRadius = 0.2f;
    [SerializeField] private float avoidWeight = 5f;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Smoothing")]
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField, Tooltip("Higher = snappier, Lower = smoother")]
    private float smoothing = 5f;

    private Transform player;
    private float attackTimer;
    private Vector3 currentDir;

    [Header("Throwing Settings")]
    [SerializeField] private float throwingForce = 25f;
    private BomberEnemyController carriedBomber;
    private bool hasThrown = false;

    private enum State { Idle, Chase, Attack, RushToBomber }
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

        state = State.RushToBomber;
        
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
            case State.RushToBomber:
                Debug.Log("TANKENEMY: Rushing to bomber");
                RushToBomber();
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
        float dist = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(player.position.x, 0, player.position.z)
        );

        if (dist <= attackRange + 0.5f)
        {
            IDamageable damageable = player.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
        }
    }

    private void RushToBomber()
    {
        if (!isCarrying)
        {
            BomberEnemyController[] bombers = FindObjectsOfType<BomberEnemyController>();
            if (bombers.Length == 0) return;

            Transform closestBomber = null;
            float closestDist = Mathf.Infinity;

            foreach (BomberEnemyController bomber in bombers)
            {
                float dist = Vector3.Distance(transform.position, bomber.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestBomber = bomber.transform;
                    carriedBomber = bomber;
                }
            }

            if (closestBomber == null || carriedBomber == null) return;

            // Move toward the closest bomber
            Vector3 toBomber = closestBomber.position - transform.position;
            toBomber.y = 0;
            Vector3 dir = toBomber.normalized;

            currentDir = Vector3.Slerp(currentDir, dir, smoothing * Time.deltaTime);
            transform.position += currentDir * (data.moveSpeed * rushToBomberSpeedMultiplier) * Time.deltaTime;

            Quaternion targetRot = Quaternion.LookRotation(currentDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);

            float distanceToBomber = Vector3.Distance(transform.position, closestBomber.position);

            if (distanceToBomber <= 1f && !hasThrown)
            {
                Debug.Log("Tank reached bomber!");

                carriedBomber.transform.position = carryZone.position;
                carriedBomber.transform.SetParent(carryZone);

                Rigidbody bomberRb = carriedBomber.GetComponent<Rigidbody>();
                if (bomberRb != null)
                {
                    bomberRb.useGravity = false;
                }

                isCarrying = true;
                hasThrown = true; // ✅ prevent multiple launches

                StartCoroutine(ThrowBomberAfterDelay(1.5f));
            }

        }
    }

    private IEnumerator ThrowBomberAfterDelay(float delay)
    {
        carriedBomber.isPickedup = true;

        yield return new WaitForSeconds(delay);

        if (carriedBomber == null) yield break;

        carriedBomber.transform.SetParent(null);

        Rigidbody bomberRb = carriedBomber.GetComponent<Rigidbody>();
        carriedBomber.isPickedup = false;
        bomberRb.useGravity = true;
        if (bomberRb != null)
        {
            Vector3 throwDir = (player.position - carryZone.position).normalized;

            bomberRb.AddForce(throwDir * throwingForce, ForceMode.Impulse);
        }

        Debug.Log("Tank threw the bomber!");

        carriedBomber = null;
        isCarrying = false;
        StartCoroutine(thrownTimer());
    }

    IEnumerator thrownTimer()
    {
        yield return new WaitForSeconds(1f);
        hasThrown = false;
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
