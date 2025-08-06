using System.Collections;
using UnityEngine;

public class RangedEnemyController : Enemy
{
    enum State { Idle, Attack, Reposition }
    State state;

    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float fireOffset = 1f;
    [SerializeField] float attackCooldown = 2f;
    [SerializeField] float repositionRadius = 5f;

    [Header("Difficulty Scaling")]
    [SerializeField] int roundForScaling = 1;
    [SerializeField] float shootDelay = 0.5f;
    [SerializeField] int amountToShoot = 3;

    [Header("Movement Tilt")]
    [SerializeField] float maxTiltAngle = 15f;
    [SerializeField] float tiltSmoothness = 5f;
    [SerializeField] float returnToNeutralSpeed = 3f;

    float attackTimer;
    Vector3 spawnPosition;
    Vector3 repositionTarget;
    Transform player;
    Quaternion baseRotation;
    Vector3 lastPosition;

    private bool hasSeenPlayer = false;
    private float timeSinceLastSeen = 0f;
    [SerializeField] private float forgetTime = 10f;

    protected override void Awake()
    {
        base.Awake();
        spawnPosition = transform.position;
        lastPosition = transform.position;
        baseRotation = transform.rotation;
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        state = State.Idle;
    }

    void Update()
    {
        if (player == null || isStunned) return;

        FacePlayer();

        if (state != State.Reposition)
        {
            attackTimer += Time.deltaTime;
        }

        switch (state)
        {
            case State.Idle:
                // Smoothly return to base rotation when idle
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    baseRotation,
                    returnToNeutralSpeed * Time.deltaTime
                );

                float distToPlayer = Vector3.Distance(transform.position, player.position);

                if (!hasSeenPlayer && distToPlayer <= data.attackRange)
                {
                    hasSeenPlayer = true;
                    timeSinceLastSeen = 0f;
                }

                if (hasSeenPlayer)
                {
                    if (distToPlayer <= data.attackRange)
                    {
                        timeSinceLastSeen = 0f;
                    }
                    else
                    {
                        timeSinceLastSeen += Time.deltaTime;
                        if (timeSinceLastSeen >= forgetTime)
                        {
                            hasSeenPlayer = false;
                            timeSinceLastSeen = 0f;
                        }
                    }

                    if (attackTimer >= attackCooldown)
                        state = State.Attack;
                }
                break;

            case State.Attack:
                int amtToShoot = currentRound < roundForScaling ? 1 : amountToShoot;
                StartCoroutine(Shoot(amtToShoot));
                attackTimer = 0f;
                ChooseRepositionTarget();
                state = State.Reposition;
                break;

            case State.Reposition:
                Vector3 horizontalTarget = new Vector3(
                    repositionTarget.x,
                    transform.position.y,
                    repositionTarget.z
                );

                // Calculate movement direction
                Vector3 moveDirection = (horizontalTarget - transform.position).normalized;

                // Apply tilt if moving
                if (moveDirection != Vector3.zero)
                {
                    // Calculate tilt angle based on movement direction (sideways)
                    float tiltAngle = -Vector3.Dot(transform.right, moveDirection) * maxTiltAngle;

                    // Create tilted rotation while maintaining base y-rotation
                    Quaternion targetRotation = Quaternion.Euler(
                        transform.rotation.eulerAngles.x,
                        transform.rotation.eulerAngles.y,
                        tiltAngle
                    );

                    // Smoothly apply tilt
                    transform.rotation = Quaternion.Lerp(
                        transform.rotation,
                        targetRotation,
                        tiltSmoothness * Time.deltaTime
                    );
                }

                // Move towards target
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    horizontalTarget,
                    CurrentMoveSpeed * Time.deltaTime
                );

                if (Vector3.Distance(transform.position, horizontalTarget) < 0.1f)
                {
                    state = State.Idle;
                }
                break;
        }
    }

    void FacePlayer()
    {
        // Store current tilt before facing player
        float currentZTilt = transform.rotation.eulerAngles.z;

        Vector3 dir = player.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            // Only rotate around Y axis
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Euler(
                0,
                targetRotation.eulerAngles.y,
                currentZTilt // Maintain current tilt
            );
        }
    }

    private IEnumerator Shoot(int amtToShoot)
    {
        for (int i = 0; i < amtToShoot; i++)
        {
            Vector3 spawnPos = transform.position + transform.forward * fireOffset;
            var go = Instantiate(bulletPrefab, spawnPos, transform.rotation);
            var b = go.GetComponent<EnemyBullet>();

            if (b != null)
            {
                Vector3 dir = player.position - transform.position;
                dir = new Vector3(dir.x, 0, dir.z);
                b.Initialize(dir, 10, data.damage);
            }

            yield return new WaitForSeconds(shootDelay);
        }
    }

    void ChooseRepositionTarget()
    {
        Vector2 rnd = Random.insideUnitCircle * repositionRadius;
        repositionTarget = new Vector3(
            spawnPosition.x + rnd.x,
            transform.position.y,
            spawnPosition.z + rnd.y
        );
    }
}