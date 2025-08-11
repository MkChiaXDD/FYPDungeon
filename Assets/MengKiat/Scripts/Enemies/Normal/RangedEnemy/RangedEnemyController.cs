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
    [SerializeField] float tiltResponseSpeed = 8f;
    [SerializeField] float tiltReturnSpeed = 5f;
    [SerializeField] float velocityThreshold = 0.1f;

    [SerializeField] private Transform shootingPoint;

    float attackTimer;
    Vector3 spawnPosition;
    Vector3 repositionTarget;
    Transform player;
    Vector3 velocity;
    Vector3 lastPosition;
    Quaternion targetRotation;

    private bool hasSeenPlayer = false;
    private float timeSinceLastSeen = 0f;
    [SerializeField] private float forgetTime = 10f;
    [SerializeField] private RangedAnimation rangeAnim;

    protected override void Awake()
    {
        base.Awake();
        spawnPosition = transform.position;
        lastPosition = transform.position;
        targetRotation = transform.rotation;
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        state = State.Idle;
    }

    void Update()
    {
        if (player == null || isStunned) return;

        // Calculate velocity
        velocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        FacePlayer();
        UpdateTilt();

        if (state != State.Reposition)
        {
            attackTimer += Time.deltaTime;
        }

        switch (state)
        {
            case State.Idle:
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

    void UpdateTilt()
    {
        if (velocity.magnitude > velocityThreshold)
        {
            // Calculate tilt based on movement direction relative to forward
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);
            float tiltAmount = Mathf.Clamp(-localVelocity.x * 2f, -maxTiltAngle, maxTiltAngle);

            // Create target rotation with tilt
            targetRotation = Quaternion.Euler(
                transform.rotation.eulerAngles.x,
                transform.rotation.eulerAngles.y,
                tiltAmount
            );
        }
        else
        {
            // Return to no tilt when not moving
            targetRotation = Quaternion.Euler(
                transform.rotation.eulerAngles.x,
                transform.rotation.eulerAngles.y,
                0
            );
        }

        // Smoothly apply rotation
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            (velocity.magnitude > velocityThreshold ? tiltResponseSpeed : tiltReturnSpeed) * Time.deltaTime
        );
    }

    void FacePlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            // Only rotate around Y axis while preserving tilt
            float currentTilt = transform.rotation.eulerAngles.z;
            Quaternion yRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Euler(
                0,
                yRotation.eulerAngles.y,
                currentTilt
            );
        }
    }

    private IEnumerator Shoot(int amtToShoot)
    {
        for (int i = 0; i < amtToShoot; i++)
        {
            rangeAnim.PlayAttack();
            var go = Instantiate(bulletPrefab, shootingPoint.position, transform.rotation);
            var b = go.GetComponent<EnemyBullet>();

            if (b != null)
            {
                Vector3 dir = player.position - transform.position;
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