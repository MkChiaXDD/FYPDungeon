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

    [SerializeField] private float chaseTimeBeforeSearchingBomber = 5f;
    private float chaseTimer = 0f;

    private bool hasSeenPlayer = false;

    private enum State { Idle, Chase, Attack, RushToBomber }
    private State state;

    [SerializeField] private TankAnim tankanim;

    private bool isMoving = false;

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
                chaseTimer = 0f;
                attackTimer = attackCooldown;
                if (isMoving)
                {
                    isMoving = false;
                    tankanim.PlayWalkingAnimation(isMoving, false);
                }
                ResetSpeed();
                break;

            case State.Chase:
                attackTimer = attackCooldown;

                ChaseWithAvoidance();

                chaseTimer += Time.deltaTime;

                if (chaseTimer >= chaseTimeBeforeSearchingBomber)
                {
                    chosenBomber = FindClosestBomber();
                    chaseTimer = 0f;

                    if (chosenBomber != null && currentRound >= roundForScaling)
                    {
                        carriedBomber = chosenBomber;
                        state = State.RushToBomber;
                        hasEvaluatedThrowChance = false;
                        return;
                    }
                    else
                    {
                        state = State.Chase;
                    }
                }
                break;

            case State.Attack:
                chaseTimer = 0f;
                FacePlayer();
                attackTimer -= Time.deltaTime;
                if (isMoving)
                {
                    isMoving = false;
                    tankanim.PlayWalkingAnimation(isMoving, false);
                }
                if (attackTimer <= 0f)
                {
                    Attack();
                    attackTimer = attackCooldown;
                }
                break;

            case State.RushToBomber:
                if (carriedBomber == null)
                {
                    isCarrying = false;
                    hasThrown = false;
                    hasEvaluatedThrowChance = false;
                    state = State.Chase;
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
        if (distToPlayer > data.detectionRange * 0.5f && !isCarrying)
        {
            MultiplySpeed(2f); // Chase faster when far away
            isMoving = true;
            tankanim.PlayWalkingAnimation(isMoving, true);
        }
        else
        {
            MultiplySpeed(0.75f); // Slow down when getting close
            isMoving = true;
            tankanim.PlayWalkingAnimation(isMoving, false);
        }

        transform.position += currentDir * CurrentMoveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(currentDir), Time.deltaTime * turnSpeed);
    }

    private void Attack()
    {
        tankanim.PlayAttackAnim();

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
        if (chosenBomber == null || isCarrying)
        {
            state = State.Idle;
            return;
        }

        smoothing = 15f;
        MultiplySpeed(rushToBomberSpeedMultiplier);
        if (!isMoving)
        {
            isMoving = true;
            tankanim.PlayWalkingAnimation(isMoving, true);
        }

        Transform bomberPos = chosenBomber.transform;

        // Calculate seek direction
        Vector3 seekDir = bomberPos.position - transform.position;
        seekDir.y = 0;
        seekDir.Normalize();

        // Calculate avoidance vector
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

        if (avoidDir != Vector3.zero)
            avoidDir.Normalize();

        float avoidWeightRush = 0.5f; // Less avoidance during rush to bomber

        Vector3 desiredDir = (seekDir + avoidDir * avoidWeightRush).normalized;

        // Use higher smoothing speed for faster correction
        currentDir = Vector3.Slerp(currentDir, desiredDir, smoothing * 5f * Time.deltaTime);

        transform.position += currentDir * CurrentMoveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(currentDir), Time.deltaTime * turnSpeed);

        float distanceToBomber = Vector3.Distance(transform.position, bomberPos.position);

        if (distanceToBomber <= 1f && !hasThrown)
        {
            if (carriedBomber == null)
            {
                state = State.Idle;
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
            tankanim.PlayCarryBomber();
            hasThrown = true;
            smoothing = originalSmoothing;
            ResetSpeed();

            // Stop tank movement just before throwing
            StopMovement();

            StartCoroutine(ThrowBomberAfterDelay(1.5f));
        }

    }

    private void StopMovement()
    {
        CurrentMoveSpeed = 0f; // Assuming CurrentMoveSpeed controls translation
    }


    private BomberEnemyController FindClosestBomber()
    {
        BomberEnemyController[] bombers = FindObjectsOfType<BomberEnemyController>();
        BomberEnemyController closestBomber = null;
        float closestDist = Mathf.Infinity;

        foreach (BomberEnemyController bomber in bombers)
        {
            if (bomber.isPickedup) continue;
            if (bomber.isExploding) continue;

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

        tankanim.PlayThrowBomber();
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

        // Restore tank's original speed after throw
        ResetSpeed();

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