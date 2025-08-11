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

    private bool isYLocked = false; // Add this at class level
    private float lockedY = 0f; // Store the locked Y height

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

        float bulletY = transform.position.y;
        float targetY = target.position.y;
        float yThreshold = 0.5f;

        if (!isYLocked && Mathf.Abs(bulletY - targetY) <= yThreshold)
        {
            isYLocked = true;
            lockedY = bulletY; // Store locked height
        }

        Vector3 targetPos;

        if (isYLocked)
        {
            targetPos = new Vector3(target.position.x, lockedY, target.position.z);
        }
        else
        {
            targetPos = target.position;
        }

        Vector3 desiredVelocity = (targetPos - transform.position).normalized * speed;
        Vector3 steering = desiredVelocity - rb.velocity;

        if (isYLocked)
        {
            steering.y = 0f;
        }

        steering = Vector3.ClampMagnitude(steering, homingForce);

        Vector3 newVelocity = rb.velocity + steering * Time.fixedDeltaTime;

        if (isYLocked)
        {
            newVelocity.y = 0f; // Force vertical velocity zero to prevent downward movement
        }

        rb.velocity = Vector3.ClampMagnitude(newVelocity, speed);

        if (rb.velocity.sqrMagnitude > 0.001f)
        {
            Vector3 forwardDir = rb.velocity.normalized;

            if (isYLocked)
            {
                forwardDir.y = 0f;
                forwardDir.Normalize();
            }

            transform.forward = forwardDir;
        }

        if (isYLocked)
        {
            // Lock Y position exactly
            Vector3 pos = transform.position;
            pos.y = lockedY;
            transform.position = pos;
        }

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
