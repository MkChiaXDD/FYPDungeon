using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedMiniController : Enemy
{
    enum State { Idle, Attack, Reposition }
    State state;

    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float fireOffset = 1f;
    [SerializeField] float attackRange = 10f;
    [SerializeField] float attackCooldown = 2f;
    [SerializeField] float repositionRadius = 5f;

    float attackTimer;
    Vector3 spawnPosition;
    Vector3 repositionTarget;
    Transform player;

    void Start()
    {
        spawnPosition = transform.position;
        player = GameObject.FindWithTag("Player").transform;
        state = State.Idle;
    }

    void Update()
    {
        FacePlayer();
        attackTimer += Time.deltaTime;

        switch (state)
        {
            case State.Idle:
                if (Vector3.Distance(transform.position, player.position) <= attackRange
                    && attackTimer >= attackCooldown)
                    state = State.Attack;
                break;

            case State.Attack:
                Shoot();
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
                    data.moveSpeed * Time.deltaTime
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

    void Shoot()
    {
        Vector3 spawnPos = transform.position + transform.forward * fireOffset;
        var go = Instantiate(bulletPrefab, spawnPos, transform.rotation);
        var b = go.GetComponent<RangedMiniBullet>();
        if (b != null) b.SetDamage(data.damage);
        Vector3 dir = player.position - transform.position;
        if (b != null)
        {
            b.Initialize(dir);
            b.SetDamage(data.damage);
        }
        //if (b != null) b.SetDamage(data.damage);

        //Initialize();
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
