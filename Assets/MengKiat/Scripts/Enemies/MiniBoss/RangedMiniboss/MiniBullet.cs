using UnityEngine;

public class MiniBullet : MonoBehaviour
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
        if (other.CompareTag("PlayerBody"))
        {
            Debug.Log("RANGEDBOSSENEMY: HIT PLAYER");
        }
        else if (other.CompareTag("Parry"))
        {
            BounceBack();
            Debug.Log("Minibullet Parry");
            return;
        }

        if (!other.TryGetComponent<IDamageable>(out var damageable)) return;
        damageable.TakeDamage(damage);
    }

    void BounceBack()
    {
        direction = -direction;
        // Rotate to face new direction
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);
    }
}
