using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed;
    public float damage;
    public Vector3 direction;
    public float lifetime = 3f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Initialize(Vector3 dir, float spd, float dmg)
    {
        direction = dir.normalized;
        speed = spd;
        damage = dmg;

        // Face movement direction
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("BULLET: HIT PLAYER");
            if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage);
            }
        }

        Debug.Log(other.name);

        Destroy(gameObject);
    }

    //void BounceBack()
    //{
    //    direction = -direction;
    //    // Rotate to face new direction
    //    if (direction != Vector3.zero)
    //        transform.rotation = Quaternion.LookRotation(direction);
    //}
}
