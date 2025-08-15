using UnityEngine;

public class RangedMiniBullet : MonoBehaviour
{
    public float speed = 20f;
    private float timer = 0f;
    public float lifetime = 5f;
    public float damage;
    private Vector3 direction;
    public GameObject minibulletPrefab;
    private int splitAmount;
    public float minibulletSpeed = 10f;

    private Vector3 lockOnPosition;
    private bool reachedTarget = false;
    private float splitDistanceThreshold = 0.2f;

    public void Initialize(Vector3 dir, int splitAmount, float _speed, float _lifeTime, float _damage, Vector3 targetPosition)
    {
        speed = _speed;
        lifetime = _lifeTime;
        damage = _damage;
        this.splitAmount = splitAmount;

        lockOnPosition = targetPosition;

        direction = (lockOnPosition - transform.position).normalized;

        direction = direction.normalized;

        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    void Update()
    {
        if (reachedTarget)
        {
            return;
        }


        Vector3 move = direction * speed * Time.deltaTime;
        transform.position += move;

        float dist = Vector3.Distance(transform.position, lockOnPosition);
        if (dist <= splitDistanceThreshold)
        {
            reachedTarget = true;
            SplitAttack();
            return;
        }

        timer += Time.deltaTime;

        if (timer >= lifetime)
        {
            SplitAttack();
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
            float angle = angleStep * i;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 minibulletDir = rot * startDirection;

            GameObject minibullet = Instantiate(minibulletPrefab, transform.position, Quaternion.LookRotation(minibulletDir));

            MiniBullet controller = minibullet.GetComponent<MiniBullet>();
            controller.Initialize(minibulletDir, minibulletSpeed, damage / 3);
        }

        Destroy(gameObject);
    }
}
