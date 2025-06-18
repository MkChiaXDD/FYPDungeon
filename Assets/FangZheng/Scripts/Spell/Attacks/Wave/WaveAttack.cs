using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveAttack : Weapon
{

    [SerializeField] private GameObject wavePrefab;

    [Header("Wave Settings")]
    [SerializeField] private float waveSpeed = 1f;
    [SerializeField] private float waveLifetime = 133.5f;
    [SerializeField] private float waveRadius = 3f;
    [SerializeField] private float startOffset = 0.5f;  // Start in front of player

    private PlayerController player;
    private readonly HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();
    [SerializeField] private float knockbackForce = 5f;

    [SerializeField] private float attackRange = 1f;
    [SerializeField] private LayerMask enemyLayer;

    protected override void Awake()
    {
        base.Awake();
        player = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Attack();
        }
    }

    public override void Attack()
    {
        StartCoroutine(WaveRoutine());
    }

    private IEnumerator WaveRoutine()
    {
        damagedTargets.Clear();

        Vector3 direction = player.GetDirection();
        Vector3 newDir = new Vector3(direction.x, 0, direction.z);
        Vector3 startPosition = transform.position + newDir * startOffset;
        float elapsed = 0f;

        while (elapsed < waveLifetime)
        {
            Vector3 currentPosition = startPosition + newDir * (waveSpeed * elapsed);
             
            // Visual effect would go here (e.g., moving sphere)
            DebugVisualization(currentPosition);

            CheckDamage(currentPosition);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void CheckDamage(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(position,
                                                waveRadius,
                                                data.hitLayers);

        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent(out IDamageable damageable))
            {
                // Skip if we've already damaged this target
                if (damagedTargets.Contains(damageable)) continue;
                // Skip if it's the player themselves
                if (damageable == player.GetComponent<IDamageable>()) continue;

                ApplyKnockBack(hit);
                damageable.TakeDamage(damage);
                damagedTargets.Add(damageable);

                // Optional: Visual feedback
                Debug.Log($"Hit: {hit.gameObject.name}", hit.gameObject);
            }
        }
    }

    private void DebugVisualization(Vector3 position)
    {
        // Only show in editor

        Debug.DrawLine(position, position + Vector3.up, Color.cyan, 0.1f);

    }

    void OnDrawGizmosSelected()
    {
        Vector3 dir = player != null ? player.GetDirection() : transform.forward;
        dir.y = 0;
        dir.Normalize();

        Vector3 start = transform.position + dir * startOffset;
        Vector3 end = start + dir * (waveSpeed * waveLifetime);

        Gizmos.color = new Color(0, 1, 1, 0.4f);
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(end, waveRadius);

        // Draw start position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(start, 0.2f);
    }

    public void ApplyKnockBack(Collider hit) 
    {
        Rigidbody enemyRb = hit.GetComponent<Rigidbody>();
        if (enemyRb != null)
        {
            // Calculate knockback direction
            Vector3 knockbackDirection = hit.transform.position - transform.position;
            knockbackDirection.y = hit.transform.position.y; // Keep the knockback horizontal

            // Apply force to the enemy
            enemyRb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode.Impulse);
        }
    }
}