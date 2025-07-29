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

    float attackTimer;
    Vector3 spawnPosition;
    Vector3 repositionTarget;
    Transform player;

    protected override void Awake()
    {
        base.Awake();
        spawnPosition = transform.position;
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
                if (Vector3.Distance(transform.position, player.position) <= data.attackRange
                    && attackTimer >= attackCooldown)
                    state = State.Attack;
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
                    state = State.Idle;
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
                b.Initialize(dir);
                b.SetDamage(damage);
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