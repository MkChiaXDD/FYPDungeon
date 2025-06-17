using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthShatterAttack : MonoBehaviour
{

    // New serialized fields for damage system
    [SerializeField] private float damageRadius = 5f;
    [SerializeField] private int damageAmount = 10;
   // [SerializeField] private LayerMask damageableLayers;

    // New function to check for IDamageable objects
    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<IDamageable>(out var damageable))
        {
            return;
        }

        damageable.TakeDamage(damageAmount);
    }
}
