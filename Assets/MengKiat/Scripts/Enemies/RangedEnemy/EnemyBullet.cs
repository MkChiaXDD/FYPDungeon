using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 5f;
    public float damage;
    public Vector3 direction;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Initialize(Vector3 dir )
    {
        direction = dir.normalized;
        //speed = spd;
        //damage = dmg;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    public void SetDamage(float dmg)
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
            direction = new Vector3(-direction.x, direction.y, -direction.z);
            Debug.Log("Parry");
            return;
        }
        Destroy(gameObject);
    }
}
