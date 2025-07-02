using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 5f;
    public float damage;
    public Vector3 direction;
    public enum Type
    {
        Enemy,
        Player,
        Other
    };
    [SerializeField] private Type _type;
    [SerializeField] private new Renderer renderer;
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Initialize(Vector3 dir , Type type = Type.Enemy)
    {
        direction = dir.normalized;
        _type = type;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += speed * Time.deltaTime * direction;
    }

    public void SetDamage(float dmg)
    {
        damage = dmg;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && _type != Type.Player)
        {
            Debug.Log("RANGEDENEMY: HIT PLAYER");
            if (other.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        if (other.CompareTag("Parry"))
        {
            BounceBack(other.transform.parent.gameObject.transform.parent.GetComponent<PlayerMovement>().GetDirection());
            other.transform.parent.gameObject.transform.parent.GetComponent<PlayerCombat>().resetParryCooldown();
            Debug.Log("Parry");
        }

        if (other.CompareTag("Bullet") && other.GetComponent<EnemyBullet>() != null)
        {
            if(_type != other.GetComponent<EnemyBullet>()._type)
            {
                Destroy(other.gameObject);
                Destroy(gameObject);
            }

        }
        if (other.CompareTag("Enemy") && _type == Type.Player)
        {
            if (other.TryGetComponent(out IDamageable damageable))
            {

                damageable.TakeDamage(damage);
                Destroy(gameObject);
                Debug.Log("RANGEDENEMY: HIT Enemy");
            }
        }
    }

    public void BounceBack(Vector3 dir)
    {
        direction = new Vector3(dir.x, direction.y, dir.z);
        _type = Type.Player;
        renderer.material.color = Color.yellow;
    }
}
