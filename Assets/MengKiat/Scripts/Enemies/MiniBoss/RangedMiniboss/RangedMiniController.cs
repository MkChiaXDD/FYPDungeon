using UnityEngine;

public class RangedMiniController : Enemy
{
    enum State { Idle, Attack, Reposition }
    State state;

    [Header("Attack Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireOffset = 1f;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float attackCooldown = 2f;

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
                Shoot();
                attackTimer = 0f;
                ChooseRepositionTarget();
                repositionTimer = 0f;
                state = State.Reposition;
                break;

            case State.Reposition:
                repositionTimer += Time.deltaTime;

                Vector3 horizontalTarget = new Vector3(
                    repositionTarget.x,
                    transform.position.y,
                    repositionTarget.z
                );

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    horizontalTarget,
                    data.moveSpeed * Time.deltaTime
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
        Vector3 spawnPos = transform.position + transform.forward * fireOffset;
        var go = Instantiate(bulletPrefab, spawnPos, transform.rotation);
        if (go.TryGetComponent<RangedMiniBullet>(out var b))
        {
            b.Initialize(player.position - transform.position);
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
