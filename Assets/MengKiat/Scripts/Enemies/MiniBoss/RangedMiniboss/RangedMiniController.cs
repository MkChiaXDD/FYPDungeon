using System.Collections;
using UnityEngine;

public class RangedMiniController : Enemy
{
    enum State { Idle, Attack, Reposition, Dodge, Melee }
    State state;

    [Header("Scaling Settings")]
    [SerializeField] private int roundForScaling = 2;

    [Header("Attack Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireOffset = 1f;
    [SerializeField] private float baseAttackCooldown = 2f;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float bulletLifetime = 3f;
    [SerializeField] private int baseSplit = 1;
    [SerializeField] private Vector2Int increasedSplit = new Vector2Int(3, 6);

    [Header("Homing Bullet Settings")]
    [SerializeField] private GameObject homingBulletPrefab;
    [SerializeField] private float homingBulletSpeed = 10f;
    [SerializeField] private float homingForce = 5f;
    [SerializeField] private float homingLifetime = 5f;
    [SerializeField] private float homingDelay = 0.3f;

    [Header("Melee Settings")]
    [SerializeField] private float meleeRange = 2f;
    [SerializeField] private float meleeTriggerTime = 2f;
    [SerializeField] private float meleeCooldown = 5f;
    [SerializeField] private float meleeKnockbackForce = 20f;
    [SerializeField] private float knockbackDuration = 1f;
    [SerializeField] private int meleeDamage = 30;

    [Header("Reposition Settings")]
    [SerializeField] private float repositionDuration = 3f;
    [SerializeField] private float repositionRadius = 5f;

    [Header("Dodge Settings")]
    [SerializeField] private float dodgeChance = 0.25f;
    [SerializeField] private float dodgeRadius = 6f;
    [SerializeField] private float rageRepositionDuration = 1.5f;

    [Header("Movement Multipliers")]
    [SerializeField] private float repositionSpeedMultiplier = 1.2f;
    [SerializeField] private float dodgeSpeedMultiplier = 2f;
    [SerializeField] private float rageSpeedMultiplier = 1.5f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 5f;

    // Runtime/Private
    private Rigidbody rb;
    private Transform player;
    private Vector3 spawnPosition;
    private Vector3 repositionTarget;

    private float attackCooldown;
    private float attackTimer;
    private float repositionTimer;
    private float meleeCooldownTimer;
    private float closeRangeTimer;

    private int bulletSplitAmt;
    private bool homingReady = false;

    void Start()
    {
        spawnPosition = transform.position;
        player = GameObject.FindWithTag("Player").transform;
        rb = GetComponent<Rigidbody>();
        state = State.Idle;
    }

    void Update()
    {
        SmoothFacePlayer();
        attackTimer += Time.deltaTime;
        meleeCooldownTimer += Time.deltaTime;

        attackCooldown = IsRaging() ? baseAttackCooldown * 0.5f : baseAttackCooldown;

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        // Track time in melee range
        if (distToPlayer <= meleeRange)
            closeRangeTimer += Time.deltaTime;
        else
            closeRangeTimer = 0f;

        if (state != State.Melee && closeRangeTimer >= meleeTriggerTime && meleeCooldownTimer >= meleeCooldown)
        {
            state = State.Melee;
            meleeCooldownTimer = 0f;
            return;
        }

        switch (state)
        {
            case State.Idle:
                if (distToPlayer <= data.attackRange && attackTimer >= attackCooldown)
                {
                    state = State.Attack;
                }
                break;

            case State.Attack:
                if (attackTimer >= attackCooldown)
                {
                    Shoot();

                    if (currentRound >= roundForScaling)
                    {
                        homingReady = true;
                        Invoke(nameof(ShootHoming), homingDelay);
                    }

                    attackTimer = 0f;

                    if (Random.value < dodgeChance)
                    {
                        ChooseDodgeTarget();
                        repositionTimer = 0f;
                        state = State.Dodge;
                    }
                    else
                    {
                        ChooseRandomRepositionTarget();
                        repositionTimer = 0f;
                        state = State.Reposition;
                    }
                }
                break;

            case State.Reposition:
                repositionTimer += Time.deltaTime;
                MoveTowardTarget(repositionSpeedMultiplier, repositionDuration);
                break;

            case State.Dodge:
                repositionTimer += Time.deltaTime;
                MoveTowardTarget(dodgeSpeedMultiplier, rageRepositionDuration);
                break;

            case State.Melee:
                PerformMeleeAttack();
                state = State.Attack;
                break;
        }
    }

    private void MoveTowardTarget(float speedMultiplier, float duration)
    {
        // Check if currently overlapping an obstacle
        float checkRadius = 0.5f; // adjust based on your enemy collider size
        bool isColliding = Physics.CheckSphere(transform.position, checkRadius, LayerMask.GetMask("Obstacle"));

        if (isColliding)
        {
            // Stop moving and reset state
            state = State.Attack;
            repositionTarget = transform.position;
            return;
        }

        Vector3 horizontalTarget = new Vector3(
            repositionTarget.x,
            transform.position.y,
            repositionTarget.z
        );

        float moveSpeed = data.moveSpeed * speedMultiplier;
        if (IsRaging()) moveSpeed *= rageSpeedMultiplier;

        Vector3 moveDir = (horizontalTarget - transform.position).normalized;
        float moveDist = moveSpeed * Time.deltaTime;

        if (!Physics.Raycast(transform.position, moveDir, moveDist + 0.1f, LayerMask.GetMask("Wall")))
        {
            rb.MovePosition(transform.position + moveDir * moveDist);
        }
        else
        {
            state = State.Attack;
            repositionTarget = transform.position;
            return;
        }

        if (Vector3.Distance(transform.position, horizontalTarget) < 0.1f || repositionTimer >= duration)
        {
            state = State.Attack;
            repositionTarget = transform.position;
        }
    }


    private void PerformMeleeAttack()
    {
        if (player.TryGetComponent<Rigidbody>(out var prb))
        {
            Vector3 knockDir = (player.position - transform.position).normalized;
            knockDir.y = 0f;
            StartCoroutine(lowDrag(prb, 1));
            prb.AddForce(knockDir * meleeKnockbackForce, ForceMode.Impulse);
        }

        if (player.TryGetComponent<IDamageable>(out var dmg))
        {
            dmg.TakeDamage(meleeDamage);
        }
    }

    private IEnumerator lowDrag(Rigidbody playerRb, float duration)
    {
        float originalDrag = playerRb.drag;
        playerRb.drag = 0f;

        // Wait the specified duration with drag = 0
        yield return new WaitForSeconds(duration);

        // Smoothly interpolate drag back to original over 1 second
        float elapsed = 0f;
        float lerpDuration = 0.5f; // time to go back to original drag

        while (elapsed < lerpDuration)
        {
            elapsed += Time.deltaTime;
            playerRb.drag = Mathf.Lerp(0f, originalDrag, elapsed / lerpDuration);
            yield return null;
        }

        // Make sure drag is exactly original at the end
        playerRb.drag = originalDrag;
    }


    private void SmoothFacePlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private void Shoot()
    {
        float healthPercent = currentHealth / maxHealth;

        if (currentRound >= roundForScaling)
        {
            bulletSplitAmt = Random.Range(increasedSplit.x, increasedSplit.y + 1);
        }
        else
        {
            bulletSplitAmt = (healthPercent > 0.5f)
                ? baseSplit
                : Random.Range(increasedSplit.x, increasedSplit.y + 1);
        }

        Vector3 shootDir = (player.position - transform.position).normalized;
        Vector3 spawnPos = transform.position + transform.forward * fireOffset;

        var go = Instantiate(bulletPrefab, spawnPos, Quaternion.LookRotation(shootDir));
        if (go.TryGetComponent<RangedMiniBullet>(out var b))
        {
            b.Initialize(shootDir, bulletSplitAmt, bulletSpeed, bulletLifetime, data.damage / bulletSplitAmt);
        }
    }

    private void ShootHoming()
    {
        if (!homingReady) return;
        homingReady = false;

        Vector3 shootDir = (player.position - transform.position).normalized;
        Vector3 spawnPos = transform.position + transform.forward * fireOffset;

        var go = Instantiate(homingBulletPrefab, spawnPos, Quaternion.LookRotation(shootDir));
        if (go.TryGetComponent<HomingBullet>(out var b))
        {
            b.Init(data.damage, homingBulletSpeed, homingForce, homingLifetime);
        }
    }

    private void ChooseDodgeTarget()
    {
        Vector3 dirAway = (transform.position - player.position).normalized;
        if (dirAway.sqrMagnitude < 0.01f) dirAway = transform.right;

        Vector2 offset = Random.insideUnitCircle * 1.5f;
        Vector3 lateral = new Vector3(offset.x, 0f, offset.y);

        repositionTarget = transform.position + dirAway * dodgeRadius + lateral;
    }

    private void ChooseRandomRepositionTarget()
    {
        for (int i = 0; i < 10; i++) // Try 10 times to find a valid spot
        {
            Vector2 offset = Random.insideUnitCircle * repositionRadius;
            Vector3 potentialTarget = transform.position + new Vector3(offset.x, 0f, offset.y);

            // Check if position is not inside a wall using an OverlapCapsule
            Vector3 point1 = potentialTarget + Vector3.up * (1f - 0.5f); // example height
            Vector3 point2 = potentialTarget + Vector3.down * (1f - 0.5f);
            if (!Physics.CheckCapsule(point1, point2, 0.5f, LayerMask.GetMask("Obstacle")))
            {
                repositionTarget = potentialTarget;
                return;
            }
        }

        // Fallback if all attempts fail
        repositionTarget = transform.position + transform.right * 2f;
    }


    private bool IsRaging()
    {
        return currentRound < roundForScaling && currentHealth / maxHealth < 0.5f;
    }
}
