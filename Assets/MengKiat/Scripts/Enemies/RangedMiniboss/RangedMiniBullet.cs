using UnityEngine;

public class RangedMiniBullet : MonoBehaviour
{
    public float speed = 20f;
    private float timer = 0f;
    public float lifetime = 5f;
    public int damage;
    public Vector3 direction;
    public GameObject minibulletPrefab; // Renamed for clarity
    public int splitAmount;
    public float minibulletSpeed = 15f; // Separate speed for minibullets
    public int minibulletDamage = 5; // Separate damage for minibullets


    public void Initialize(Vector3 dir)
    {
        dir = new Vector3(dir.x, 0, dir.z);
        direction = dir.normalized;

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

    public void SetDamage(int dmg)
    {
        damage = dmg;
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
        Vector3 startDirection = transform.forward;

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
            controller.Initialize(minibulletDir, minibulletSpeed, minibulletDamage);
        }

        Destroy(gameObject);
    }
}