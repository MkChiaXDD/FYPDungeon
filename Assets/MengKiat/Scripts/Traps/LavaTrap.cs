using UnityEngine;

public class LavaTrap : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float tickrate = 1f;
    private float timer;

    // Update is called once per frame
    void Update()
    {
        if (timer <= tickrate)
        {
            timer += Time.deltaTime;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (timer < tickrate) return;

        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(damage);
                Debug.Log("LAVATRAP: HIT Enemy or Player");

                timer = 0;
            }
        }
    }
}
