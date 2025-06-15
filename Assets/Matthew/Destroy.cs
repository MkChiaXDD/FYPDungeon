using System.Collections;
using UnityEngine;

public class Destroy : MonoBehaviour
{

    [SerializeField] private float DestroyCountdownDuration = 5.0f;
    [SerializeField] private ParticleSystem EarthShatterVFX;
    // New serialized fields for damage system
    [SerializeField] private float damageRadius = 5f;
    [SerializeField] private int damageAmount = 10;
    [SerializeField] private LayerMask damageableLayers;


    // Start is called before the first frame update
    void Start()
    {

        EarthShatterVFX.Play();
        StartCoroutine(DestroySelf());
        
    }
    private IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(DestroyCountdownDuration);
        Destroy(gameObject);
    }

    private void Update()
    {
        CheckForDamageableObjects();
    }

    private void OnCollisionEnter(Collision collision)
    {

        foreach (Collider collider in collision)
        {
            DamageableEntity entity = collider.GetComponentInParent<DamageableEntity>();
            // Try to get IDamageable component from the object


            // Apply damage to the damageable object
            entity.TakeDamage(damageAmount);


            // Optional: Add visual feedback here
            Debug.Log($"Damaged {collider.name}");

        }
    }

    // New function to check for IDamageable objects
    private void CheckForDamageableObjects()
    {
        // Calculate attack position with offset
        Vector3 attackPosition = transform.position + transform.forward;

        // Detect all colliders in the specified radius
        Collider[] hitColliders = Physics.OverlapSphere(
            attackPosition,
            damageRadius,
            damageableLayers
        );

        
    }


}
