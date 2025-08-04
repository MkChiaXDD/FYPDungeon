using UnityEngine;

public class RangedMiniController : Enemy
{
    enum State { Idle, Attack, Reposition }
    State state;

    [Header("Scaling Settings")]
    [SerializeField] private int roundForScaling = 2;

    [Header("Attack Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireOffset = 1f;
    [SerializeField] private float baseAttackCooldown = 2f;
    private float attackCooldown;

    [SerializeField] private int baseSplit = 1;
    [SerializeField] private Vector2Int increasedSplit = new Vector2Int(3, 6);

    [SerializeField] private GameObject homingBulletPrefab;
    [SerializeField] private float homingBulletSpeed = 10f;
    [SerializeField] private float homingForce = 5f;
    [SerializeField] private float homingLifetime = 5f;
    [SerializeField] private float homingDelay = 0.3f;

    [Header("Rage Mode Settings")]
    [SerializeField] private float rageSpeedMultiplier = 1.5f;
    [SerializeField] private float rageRepositionDuration = 1.5f;


    [Header("Reposition Settings")]
    [SerializeField] private float repositionRadius = 5f;
    [SerializeField] private float repositionDuration = 3f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 5f;

    private float attackTimer;
    private float repositionTimer;
    private Vector3 spawnPosition;
    private Vector3 repositionTarget;
    private Transform player;
    private bool homingReady = false;
    private int bulletSplitAmt;

    void Start()
    {
        spawnPosition = transform.position;
        player = GameObject.FindWithTag("Player").transform;
        state = State.Idle;
    }

    void Update()
    {
        SmoothFacePlayer();
        attackTimer += Time.deltaTime;
        attackCooldown = baseAttackCooldown;

        switch (state)
        {
            case State.Idle:
                if (Vector3.Distance(transform.position, player.position) <= data.attackRange &&
                    attackTimer >= attackCooldown)
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
                    ChooseRepositionTarget();
                    repositionTimer = 0f;
                    state = State.Reposition;
                }
                break;

            case State.Reposition:
                repositionTimer += Time.deltaTime;

                Vector3 horizontalTarget = new Vector3(
                    repositionTarget.x,
                    transform.position.y,
                    repositionTarget.z
                );

                float moveSpeed = currentMoveSpeed;
                float currentRepositionDuration = repositionDuration;

                // Rage mode if HP < 50%
                if (currentHealth / maxHealth < 0.5f)
                {
                    moveSpeed *= rageSpeedMultiplier;
                    currentRepositionDuration = rageRepositionDuration;
                }

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    horizontalTarget,
                    moveSpeed * Time.deltaTime
                );

                if (Vector3.Distance(transform.position, horizontalTarget) < 0.1f ||
                    repositionTimer >= currentRepositionDuration)
                {
                    state = State.Attack;
                }
                break;
        }
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
            if (healthPercent > 0.5f)
                bulletSplitAmt = baseSplit;
            else
                bulletSplitAmt = Random.Range(increasedSplit.x, increasedSplit.y + 1);
        }

        Vector3 shootDir = (player.position - transform.position).normalized;
        Vector3 spawnPos = transform.position + transform.forward * fireOffset;

        var go = Instantiate(bulletPrefab, spawnPos, Quaternion.LookRotation(shootDir));
        if (go.TryGetComponent<RangedMiniBullet>(out var b))
        {
            b.Initialize(shootDir, bulletSplitAmt);
            b.SetDamage(data.damage / bulletSplitAmt);
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

    private void ChooseRepositionTarget()
    {
        Vector2 rnd = Random.insideUnitCircle * repositionRadius;
        repositionTarget = new Vector3(
            spawnPosition.x + rnd.x,
            transform.position.y,
            spawnPosition.z + rnd.y
        );
    }
}
