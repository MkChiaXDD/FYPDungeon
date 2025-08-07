using UnityEngine;

public class RangedMiniBullet : MonoBehaviour
{
    public float speed = 20f;
    private float timer = 0f;
    public float lifetime = 5f;
    public float damage;
    public Vector3 direction;
    public GameObject minibulletPrefab; // Renamed for clarity
    private int splitAmount;
    public float minibulletSpeed = 10f; // Separate speed for minibullets

    public void Initialize(Vector3 dir, int splitAmount, float _speed, float _lifeTime, float _damage)
    {
        speed = _speed;
        lifetime = _lifeTime;
        damage = _damage;
        dir = new Vector3(dir.x, dir.y, dir.z);
        dir.y -= 0.2f;
        direction = dir.normalized;
        this.splitAmount = splitAmount;

        // Make bullet face its direction
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        timer += Time.deltaTime;

        if (timer >= lifetime)
        {
            if (splitAmount > 0) SplitAttack();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("RANGEDENEMY: HIT PLAYER");
        }
        else if (other.CompareTag("Parry"))
        {
            BounceBack();
            Debug.Log("Parry");
            return;
        }

        if (!other.TryGetComponent<IDamageable>(out var damageable)) return;
        damageable.TakeDamage(damage);
    }

    public void BounceBack()
    {
        direction = new Vector3(-direction.x, direction.y, -direction.z);
        // Rotate to face new direction
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    private void SplitAttack()
    {
        if (minibulletPrefab == null || splitAmount <= 0) return;

        float angleStep = 360f / splitAmount;
        Vector3 startDirection = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;

        for (int i = 0; i < splitAmount; i++)
        {
            // Calculate direction for each minibullet
            float angle = angleStep * i;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 minibulletDir = rot * startDirection;

            // Create and initialize minibullet
            GameObject minibullet = Instantiate(
                minibulletPrefab,
                transform.position,
                Quaternion.LookRotation(minibulletDir)
            );

            Debug.Log("Instantiated bullet");

            MiniBullet controller = minibullet.GetComponent<MiniBullet>();
            controller.Initialize(minibulletDir, minibulletSpeed, damage / 2);
        }

        Destroy(gameObject);
    }
}