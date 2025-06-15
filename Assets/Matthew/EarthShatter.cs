using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class EarthShatter : MonoBehaviour
{
    //[SerializeField] private ParticleSystem EarthShatterVFX;
    [SerializeField] private GameObject EarthShatterAttack;
   // [SerializeField] private GameObject AttackCollider;
    [SerializeField] private float AttackDuration;
    [SerializeField] private float AttackOffsetZ;

     // New serialized fields for damage system
    [SerializeField] private float damageRadius = 5f;
    [SerializeField] private int damageAmount = 10;
    [SerializeField] private LayerMask damageableLayers;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            SummonEarthShatterAttack();
            // PlayEarthShatterAttackVFX();
        }
    }

    private void PlayEarthShatterAttackVFX()
    {
      //  EarthShatterVFX.Play();
        HandleAttackCollider();
    }

    private void SummonEarthShatterAttack()
    {
        Instantiate(EarthShatterAttack, transform.position, transform.rotation);
        CheckForDamageableObjects();
    }

    private void HandleAttackCollider()
    {
        StartCoroutine(ActivateAttackCollider());
    }

    private IEnumerator ActivateAttackCollider()
    {
        //AttackCollider.SetActive(true);
        yield return new WaitForSeconds(AttackDuration);
        //AttackCollider.SetActive(false);
    }

    // New function to check for IDamageable objects
    private void CheckForDamageableObjects()
    {
        // Calculate attack position with offset
        Vector3 attackPosition = transform.position + transform.forward * AttackOffsetZ;

        // Detect all colliders in the specified radius
        Collider[] hitColliders = Physics.OverlapSphere(
            attackPosition,
            damageRadius,
            damageableLayers
        );

        foreach (Collider collider in hitColliders)
        {
            DamageableEntity entity = collider.GetComponentInParent<DamageableEntity>();
            // Try to get IDamageable component from the object

            if (!entity)
            {
                // Apply damage to the damageable object
                entity.TakeDamage(damageAmount);


                // Optional: Add visual feedback here
                Debug.Log($"Damaged {collider.name}");
            }
        }
    }
}
