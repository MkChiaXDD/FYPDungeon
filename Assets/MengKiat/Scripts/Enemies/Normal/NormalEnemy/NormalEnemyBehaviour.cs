using UnityEngine;

// Normal enemy with Idle, Chase, Attack states
public class NormalEnemyController : Enemy
{
    [SerializeField] private float chaseRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1f;

    private Transform player;
    private float attackTimer;

    private enum State { Idle, Chase, Attack }
    private State state;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        state = State.Idle;
    }

    void Update()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= attackRange)
            state = State.Attack;
        else if (dist <= chaseRange)
            state = State.Chase;
        else
            state = State.Idle;

        switch (state)
        {
            case State.Idle:
                Debug.Log("NORMALENEMY: Idle!");
                break;
            case State.Chase:
                Debug.Log("NORMALENEMY: Chase!");
                Chase();
                break;
            case State.Attack:
                Attack();
                break;
        }

        attackTimer -= Time.deltaTime;
    }

    private void Chase()
    {
        Vector3 dir = (player.position - transform.position).normalized;

        // Move
        transform.position += dir * data.moveSpeed * Time.deltaTime;

        // Rotate toward movement direction
        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f); // Adjust speed as needed
        }
    }

    private void Attack()
    {
        if (attackTimer > 0f) return;

        //var health = player.GetComponent<PlayerHealth>();
        //if (health != null)
        //    health.TakeDamage(data.damage);
        Debug.Log("NORMALENEMY: Attack!");
        attackTimer = attackCooldown;
    }
}
