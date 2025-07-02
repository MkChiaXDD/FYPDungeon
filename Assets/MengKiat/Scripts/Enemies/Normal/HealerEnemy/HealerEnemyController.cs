using UnityEngine;
using UnityEngine.ProBuilder;

public class HealerEnemyController : Enemy
{
    [Header("Healing Player Settings")]
    [SerializeField] private float playerHealingTick = 1f;
    [SerializeField] private float playerHealingAmount = 1f;

    [Header("Healing Enemy Settings")]
    [SerializeField] private float enemyHealingTick = 0.01f;
    [SerializeField] private float enemyHealingAmount = 100f;

    [Header("Avoidance")]
    [SerializeField] private float feelerLength = 10f;
    [SerializeField] private float feelerRadius = 0.2f;
    [SerializeField] private float avoidWeight = 200f;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Smoothing")]
    [SerializeField] private float turnSpeed = 30f;
    [SerializeField, Tooltip("Higher = snappier, Lower = smoother")]
    private float smoothing = 1f;

    private bool isHealingPlayer = true;
    private Transform player;
    private float timer = 0;
    private Vector3 currentDir;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null) return;

        if (currentHealth < data.maxHealth && isHealingPlayer == true)
        {
            isHealingPlayer = false;
        }

        timer += Time.deltaTime;

        if (isHealingPlayer)
        {
            float dist = Vector3.Distance(
               new Vector3(transform.position.x, 0, transform.position.z),
               new Vector3(player.position.x, 0, player.position.z)
            );

            if (dist <= data.detectionRange && dist > data.attackRange)
            {
                FollowTarget(player);
            }

            if (dist <= data.attackRange)
            {
                if (timer >= playerHealingTick)
                {
                    HealPlayer(playerHealingAmount);
                    timer = 0;
                }
            }
            else
            {
                timer = 0;
            }
        }
        else
        {
            Enemy enemy = FindClosestEnemy();
            if (enemy != null)
            {
                float dist = Vector3.Distance(transform.position, enemy.transform.position);

                if (dist > data.attackRange && dist <= data.detectionRange)
                {
                    FollowTarget(enemy.transform);
                }

                if (dist <= data.attackRange && timer >= enemyHealingTick)
                {
                    HealEnemy(enemy, enemyHealingAmount);
                    timer = 0;
                }
            }
        }
    }

    private void FollowTarget(Transform target)
    {
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0;
        Vector3 seekDir = toTarget.normalized;

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

        transform.position += currentDir * data.moveSpeed * Time.deltaTime;
        Quaternion targetRot = Quaternion.LookRotation(currentDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
    }

    private void HealPlayer(float healAmount)
    {
        IDamageable damageable = player.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.Heal(healAmount);
        }
    }

    private Enemy FindClosestEnemy()
    {
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        Enemy closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Enemy e in allEnemies)
        {
            if (e == this || e == null || e.currentHealth >= e.maxHealth)
                continue;

            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestEnemy = e;
            }
        }

        return closestEnemy;
    }

    private void HealEnemy(Enemy target, float healAmount)
    {
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.Heal(healAmount);
        }
    }
}
