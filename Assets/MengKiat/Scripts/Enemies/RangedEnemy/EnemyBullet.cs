using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 5f;
    public int damage;
    public Vector3 direction;
    public enum Type
    {
        Enemy,
        Player,
        Other
    };
    [SerializeField] private Type _type;
    [SerializeField] private Renderer renderer;
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Initialize(Vector3 dir , Type type = Type.Enemy)
    {
        direction = dir.normalized;
        _type = type;
        //speed = spd;
        //damage = dmg;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += speed * Time.deltaTime * direction;
    }

    public void SetDamage(int dmg)
    {
        damage = dmg;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && _type != Type.Player)
        {
            Debug.Log("RANGEDENEMY: HIT PLAYER");
            Destroy(gameObject);
            //Destroy(gameObject);
        }
        if (other.CompareTag("Parry"))
        {
            //other.transform.parent.gameObject.transform.parent.GetComponent<PlayerController>().GetDirection();
            BounceBack(other.transform.parent.gameObject.transform.parent.GetComponent<PlayerController>().GetDirection());
            other.transform.parent.gameObject.transform.parent.GetComponent<PlayerController>().resetParryCooldown();
            Debug.Log("Parry");
            //return;
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
            //Debug.Log("RANGEDENEMY: HIT Enemy");
            if (other.TryGetComponent(out IDamageable damageable))
            {

                damageable.TakeDamage(damage);
                Destroy(gameObject);
                Debug.Log("RANGEDENEMY: HIT Enemy");
            }
        }
/*            damageable.TakeDamage(damage);
        Debug.Log("RANGEDENEMY: HIT Something");*/
        //Destroy(gameObject);
    }

    public void BounceBack(Vector3 dir)
    {
        direction = new Vector3(dir.x, direction.y, dir.z);
        _type = Type.Player;
        renderer.material.color = Color.yellow;
    }
}
