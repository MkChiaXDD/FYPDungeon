using UnityEngine;

public class RangedMiniController : Enemy
{
    enum State { Idle, Attack, Reposition }
    State state;

    [Header("Diff Scaling Settings")]
    [SerializeField] private int roundForScaling = 1;
    [SerializeField] private int baseSplit;
    [SerializeField] private Vector2Int increasedSplit;
    [SerializeField] private int moveSpeedMultiplier;
    [SerializeField] private float increasedAttackCooldown = 1f;

    [Header("Attack Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireOffset = 1f;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float baseAttackCooldown = 2f;
    private float attackCooldown;
    private int bulletSplitAmt;

    [Header("Reposition Settings")]
    [SerializeField] private float repositionRadius = 5f;
    [SerializeField] private float repositionDuration = 3f; // max time to reposition

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 5f; // higher = faster turn

    private float attackTimer;
    private float repositionTimer;
    private Vector3 spawnPosition;
    private Vector3 repositionTarget;
    private Transform player;

    void Start()
    {
        spawnPosition = transform.position;
        player = GameObject.FindWithTag("Player").transform;
        state = State.Idle;
    }

    void Update()
    {
        // always smooth-look at the player
        SmoothFacePlayer();

        attackTimer += Time.deltaTime;

        if (currentRound >= roundForScaling)
        {
            attackCooldown = increasedAttackCooldown;
        }
        else
        {
            attackCooldown = baseAttackCooldown;
        }

        switch (state)
        {
            case State.Idle:
                if (Vector3.Distance(transform.position, player.position) <= attackRange
                    && attackTimer >= attackCooldown)
                {
                    state = State.Attack;
                }
                break;

            case State.Attack:
                if (attackTimer >= attackCooldown)
                {
                    Shoot();
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

                float moveSpeed;
                if (currentRound >= roundForScaling)
                {
                    moveSpeed = data.moveSpeed * moveSpeedMultiplier;
                }
                else
                {
                    moveSpeed = data.moveSpeed;
                }

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    horizontalTarget,
                    moveSpeed * Time.deltaTime
                );

                if (Vector3.Distance(transform.position, horizontalTarget) < 0.1f
                    || repositionTimer >= repositionDuration)
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
        if (currentRound >= roundForScaling)
        {
            bulletSplitAmt = Random.Range(increasedSplit.x, increasedSplit.y);
        }
        else
        {
            bulletSplitAmt = baseSplit;
        }
        Vector3 spawnPos = transform.position + transform.forward * fireOffset;
        var go = Instantiate(bulletPrefab, spawnPos, transform.rotation);
        if (go.TryGetComponent<RangedMiniBullet>(out var b))
        {
            b.Initialize(player.position - transform.position, bulletSplitAmt);
            b.SetDamage(data.damage);
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
