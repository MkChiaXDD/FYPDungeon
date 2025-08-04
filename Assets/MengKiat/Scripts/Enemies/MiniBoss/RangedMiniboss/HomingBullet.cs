using UnityEngine;

public class HomingBullet : MonoBehaviour
{
    [Header("Targeting")]
    private Transform target;

    [Header("Bullet Stats")]
    private float damage = 10f;
    private float speed = 10f;
    private float homingForce = 5f;
    private float lifetime = 5f;

    private Rigidbody rb;
    private float timer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            target = player.transform;
        else
            Debug.LogWarning("Player not found! Make sure your player has the 'Player' tag.");

        rb.velocity = transform.forward * speed;
    }

    void FixedUpdate()
    {
        if (target == null) return;

        // Keep the bullet's Y position fixed
        Vector3 targetPos = target.position;
        targetPos.y = transform.position.y;

        // Steering behavior (XZ only)
        Vector3 desiredVelocity = (targetPos - transform.position).normalized * speed;
        Vector3 steering = desiredVelocity - rb.velocity;
        steering.y = 0f; // ignore vertical steering
        steering = Vector3.ClampMagnitude(steering, homingForce);

        Vector3 newVelocity = rb.velocity + steering * Time.fixedDeltaTime;
        newVelocity.y = 0f; // lock Y movement
        rb.velocity = Vector3.ClampMagnitude(newVelocity, speed);

        if (rb.velocity.sqrMagnitude > 0.001f)
            transform.forward = rb.velocity.normalized;

        timer += Time.fixedDeltaTime;
        if (timer >= lifetime)
            Destroy(gameObject);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Replace this with your damage logic
            Debug.Log("Player hit for " + damage + " damage.");
            Destroy(gameObject);
        }

        if (!other.TryGetComponent<IDamageable>(out var damageable)) return;
        damageable.TakeDamage(damage);
    }

    /// <summary>
    /// Call this after Instantiate() to initialize bullet values.
    /// </summary>
    public void Init(float _damage, float _speed, float _homingForce, float _lifetime)
    {
        damage = _damage;
        speed = _speed;
        homingForce = _homingForce;
        lifetime = _lifetime;
    }
}
