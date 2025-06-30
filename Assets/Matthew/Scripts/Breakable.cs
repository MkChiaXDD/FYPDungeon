using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject brokenObject;
    [SerializeField] private ItemDropSystem dropSystem;

    [SerializeField] float explodeRange = 1.5f;
    [SerializeField] float explosionRadius = 3f;
    [SerializeField] float explosionForce = 5f;
    [SerializeField] private float explosionUpwardModifier = 1f;
    [SerializeField] LayerMask everyMask;
    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(nameof(BreakObject));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(nameof(BreakObject));
        }
    }

    // Using OnTriggerEnter (for trigger collisions)
    void OnTriggerEnter(Collider other)
    {
        // Check if the collider's tag is "Enemy"
        if (other.gameObject.CompareTag("EarthShatterAttack"))
        {
            StartCoroutine(nameof(BreakObject));
            Debug.Log("Triggered by EarthShatter!");
        }
    }

    private IEnumerator BreakObject()
    {
        yield return Instantiate(brokenObject, transform.position, Quaternion.Euler(0, 0, 0));
        Explode();
        if (!dropSystem)
        { 
            dropSystem.SpawnDropItem();
        }     
        Destroy(gameObject);
    }

    void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position,transform.localScale.x,~everyMask);
        foreach (var hit in hits)
        {
       

            if (hit.attachedRigidbody != null)
            {
                hit.attachedRigidbody.AddExplosionForce(
                    explosionForce,            // base force
                    transform.position,        // origin
                    explosionRadius,           // radius
                    explosionUpwardModifier,   // upwards modifier
                    ForceMode.Impulse          // instant burst
                );
            }
        }

        Destroy(gameObject);
    }

    public void TakeDamage(float damage) => Die();
    public void Die() => StartCoroutine(nameof(BreakObject));

    public void DropItem() => dropSystem.SpawnDropItem();

}
