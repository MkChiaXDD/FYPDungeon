// Example elemental effect component
using System.Collections;
using UnityEngine;

public class HydroApplier : MonoBehaviour
{
    public ElementType elementType = ElementType.Hydro;
    public float elementDuration = 1.0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IDamageable>(out var hitTargets) && !other.CompareTag("Player"))
        {
            if (other.CompareTag("Object"))
            {
                hitTargets.TakeElementalDamage(0, elementType);
            }
            else
            {
                //hitEnemies.TakeDamage(damageAmount);
                ApplyElementalEffects(other.gameObject);

                
            }
        }
    }

    private void ApplyElementalEffects(GameObject target)
    {
        // Apply elemental effect
        if (target.TryGetComponent<ElementalStatus>(out var status))
        {
            status.ApplyElement(elementType, elementDuration);
            ElementalReactionManager.Instance.CheckReactions(
                status,
                elementType,
                transform.position,
                0
            );
            Debug.Log("dealt " + elementType + "element to " + target);
        }
        else
        {


            target.AddComponent<ElementalStatus>().ApplyElement(elementType, elementDuration);
            ElementalReactionManager.Instance.CheckReactions(
                status,
                elementType,
                transform.position,
                0
            );
            Debug.Log("applied " + elementType + "element to " + target);

        }
    }

}