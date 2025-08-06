using System.Collections;
using UnityEngine;

public class TankEnemyController : Enemy
{
    [Header("Stats")]
    [SerializeField] private float attackCooldown = 1f;

    [Header("Diff Scaling Settings")]
    [SerializeField] private int roundForScaling = 1;
    [SerializeField, Range(0f, 1f)] private float chanceToGoThrow = 0.5f;
    [SerializeField] private float detectionRange = 10f;
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
    private float originalSmoothing;
    private float originalSpeed;

    [Header("Throwing Settings")]
    [SerializeField] private float throwingForce = 25f;
    private BomberEnemyController chosenBomber;
    private BomberEnemyController carriedBomber;
    private bool hasThrown = false;
    private bool hasEvaluatedThrowChance = false;

    private bool hasSeenPlayer = false;

    private enum State { Idle, Chase, Attack, RushToBomber }
    private State state;

    protected override void Awake()
    {
        base.Awake();
        originalSpeed = CurrentMoveSpeed;
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        state = State.Idle;
        currentDir = transform.forward;
        originalSmoothing = smoothing;
    }

    void Update()
    {
        if (player == null || isStunned) return;

        float dist = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(player.position.x, 0, player.position.z)
        );

        if (state != State.RushToBomber)
        {
            if (dist <= data.detectionRange)
            {
                hasSeenPlayer = true;
            }

            if (hasSeenPlayer)
            {
                if (dist <= data.attackRange) state = State.Attack;
                else state = State.Chase;

                if (state != State.Attack)
                    hasEvaluatedThrowChance = false;
            }
            else
            {
                state = State.Idle;
            }
        }


        switch (state)
        {
            case State.Idle:
                attackTimer = attackCooldown;
                ResetSpeed(); // Return to normal speed when idle
                break;

            case State.Chase:
                attackTimer = attackCooldown;
                ChaseWithAvoidance();
                break;

            case State.Attack:
                if (!hasEvaluatedThrowChance)
                {
                    float rand = Random.value;
                    chosenBomber = FindClosestBomber();
                    hasEvaluatedThrowChance = true;

                    if (rand <= chanceToGoThrow && currentRound >= roundForScaling && chosenBomber != null)
                    {
                        state = State.RushToBomber;
                        carriedBomber = chosenBomber;
                        hasEvaluatedThrowChance = false;
                        return;
                    }
                }

                FacePlayer();
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0f)
                {
                    Attack();
                    attackTimer = attackCooldown;
                }
                break;

            case State.RushToBomber:
                if (carriedBomber == null)
                {
                    // The bomber was destroyed/exploded, reset state and flags
                    isCarrying = false;
                    hasThrown = false;
                    hasEvaluatedThrowChance = false;
                    state = State.Idle; // Or Chase if you want
                    break;
                }
                RushToBomber(carriedBomber);
                break;

        }
    }

    void FacePlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    private void ChaseWithAvoidance()
    {
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0;
        Vector3 seekDir = toPlayer.normalized;

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

        Vector3 desired = seekDir + avoidDir * avoidWeight;
        desired.y = 0;
        if (desired == Vector3.zero) desired = transform.forward;
        desired.Normalize();

        currentDir = Vector3.Slerp(currentDir, desired, smoothing * Time.deltaTime);

        // Dynamic speed adjustment based on distance
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        if (distToPlayer > data.detectionRange * 0.7f)
        {
            MultiplySpeed(2f); // Chase faster when far away
        }
        else
        {
            MultiplySpeed(0.75f); // Slow down when getting close
        }

        transform.position += currentDir * CurrentMoveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(currentDir), Time.deltaTime * turnSpeed);
    }

    private void Attack()
    {
        float dist = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(player.position.x, 0, player.position.z)
        );

        if (dist <= data.attackRange)
        {
            IDamageable damageable = player.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
        }
    }

    private void RushToBomber(BomberEnemyController chosenBomber)
    {
        // If no bomber or already carrying, go back to Idle or Chase
        if (chosenBomber == null || isCarrying)
        {
            state = State.Idle; // or State.Chase if you want it to keep chasing player
            return;
        }

        smoothing = 15f;
        MultiplySpeed(rushToBomberSpeedMultiplier); // Use speed multiplier for rushing

        Transform bomberPos = chosenBomber.transform;
        Vector3 toBomber = bomberPos.position - transform.position;
        toBomber.y = 0;
        Vector3 dir = toBomber.normalized;

        currentDir = Vector3.Slerp(currentDir, dir, smoothing * Time.deltaTime);
        transform.position += currentDir * CurrentMoveSpeed * Time.deltaTime;

        Quaternion targetRot = Quaternion.LookRotation(currentDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);

        float distanceToBomber = Vector3.Distance(transform.position, bomberPos.position);

        if (distanceToBomber <= 1f && !hasThrown)
        {
            // Check again if carriedBomber is not null (it could have been destroyed)
            if (carriedBomber == null)
            {
                state = State.Idle; // reset state to safe one
                return;
            }

            carriedBomber.transform.position = carryZone.position;
            carriedBomber.transform.SetParent(carryZone);

            Rigidbody bomberRb = carriedBomber.GetComponent<Rigidbody>();
            if (bomberRb != null)
            {
                bomberRb.useGravity = false;
            }

            isCarrying = true;
            hasThrown = true;
            smoothing = originalSmoothing;
            ResetSpeed(); // Return to normal speed when carrying
            StartCoroutine(ThrowBomberAfterDelay(1.5f));
        }
    }


    private BomberEnemyController FindClosestBomber()
    {
        BomberEnemyController[] bombers = FindObjectsOfType<BomberEnemyController>();
        BomberEnemyController closestBomber = null;
        float closestDist = Mathf.Infinity;

        foreach (BomberEnemyController bomber in bombers)
        {
            if (bomber.isPickedup) continue;

            float dist = Vector3.Distance(transform.position, bomber.transform.position);
            if (dist <= detectionRange && dist < closestDist)
            {
                closestDist = dist;
                closestBomber = bomber;
            }
        }

        return closestBomber;
    }

    private IEnumerator ThrowBomberAfterDelay(float delay)
    {
        carriedBomber.isPickedup = true;
        yield return new WaitForSeconds(delay);

        if (carriedBomber == null) yield break;

        carriedBomber.transform.SetParent(null);

        Rigidbody bomberRb = carriedBomber.GetComponent<Rigidbody>();
        carriedBomber.isPickedup = false;
        if (bomberRb != null)
        {
            bomberRb.useGravity = true;
            Vector3 throwDir = (player.position - carryZone.position).normalized;
            bomberRb.AddForce(throwDir * throwingForce, ForceMode.Impulse);
        }

        carriedBomber = null;
        isCarrying = false;
        StartCoroutine(ThrownTimer());
    }

    private IEnumerator ThrownTimer()
    {
        yield return new WaitForSeconds(1f);
        hasThrown = false;
        state = State.Idle;
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
    }
}