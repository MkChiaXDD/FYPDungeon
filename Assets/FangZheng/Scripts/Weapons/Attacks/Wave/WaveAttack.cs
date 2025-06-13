using UnityEngine;
using System.Collections;

public class WaveAttack : Weapon
{
    [SerializeField] private float waveSpeed = 15f;
    [SerializeField] private float waveLifetime = 1.5f;
    [SerializeField] private float waveRadius = 1f;

    private PlayerController player;

    protected override void Awake()
    {
        base.Awake();
        player = GetComponentInParent<PlayerController>();
    }

    public override void Attack()
    {
        StartCoroutine(WaveRoutine());
    }

    private IEnumerator WaveRoutine()
    {
        Debug.Log("ATTACK!");
        float t = 0f;
        Vector3 dir = player.GetDirection();
        Vector3 pos = transform.position;

        while (t < waveLifetime)
        {
            pos += dir * waveSpeed * Time.deltaTime;
            var hits = Physics.OverlapSphere(pos, waveRadius, data.hitLayers);
            foreach (var c in hits)
                if (c.TryGetComponent<IDamageable>(out var d))
                    d.TakeDamage(damage);

            t += Time.deltaTime;
            yield return null;
        }
    }

    // ←–––––––––– ADD THIS ––––––––––→
    void OnDrawGizmosSelected()
    {
        // pick a direction in-editor if we can't get the player one
        Vector3 dir = Application.isPlaying && player != null
            ? player.GetDirection()
            : transform.forward;

        dir.y = 0;
        dir.Normalize();

        Vector3 start = transform.position;
        Vector3 end = start + dir * waveSpeed * waveLifetime;

        Gizmos.color = Color.cyan;
        // draw the path
        Gizmos.DrawLine(start, end);
        // draw the impact sphere at the end
        Gizmos.DrawWireSphere(end, waveRadius);
    }
    // ←––––––––––––––––––––––––––––––→
}
