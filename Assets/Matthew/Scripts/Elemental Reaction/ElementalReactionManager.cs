using System.Collections;
using UnityEngine;

public class ElementalReactionManager : MonoBehaviour
{
    public static ElementalReactionManager Instance;

    public GameObject FireVFX;

    [Header("Reaction Multipliers")]
    [SerializeField] private float _vaporizeMultiplier = 1.5f;
    [SerializeField] private float _meltMultiplier = 2f;
    [SerializeField] private float _overloadMultiplier = 1.2f;
    [SerializeField] private float _superconductDuration = 5f;

    void Awake() => Instance = this;

    // Check for possible reactions
    public void CheckReactions(ElementalStatus target, ElementType triggerElement, Vector3 position, float baseDamage)
    {
        foreach (var existingElement in target.GetActiveElements())
        {
            ReactionType reaction = GetReactionType(triggerElement, existingElement.Key);

            if (reaction != ReactionType.None)
            {
                
                TriggerReaction(reaction, target, triggerElement, existingElement.Key, position, baseDamage);
                Debug.Log("Triggered " + reaction);
                return; // Only trigger one reaction per attack
            }
        }
    }

    // Define reaction rules
    private ReactionType GetReactionType(ElementType trigger, ElementType existing)
    {
        if (trigger == ElementType.Pyro)
        {
            if (existing == ElementType.Hydro) return ReactionType.Vaporize;
            if (existing == ElementType.Electro) return ReactionType.Overload;
            if (existing == ElementType.Cryo) return ReactionType.Melt;
        }
        else if (trigger == ElementType.Hydro)
        {
            if (existing == ElementType.Pyro) return ReactionType.Vaporize;
            if (existing == ElementType.Electro) return ReactionType.ElectroCharged;
            if (existing == ElementType.Cryo) return ReactionType.Frozen;
        }
        else if (trigger == ElementType.Electro)
        {
            if (existing == ElementType.Pyro) return ReactionType.Overload;
            if (existing == ElementType.Hydro) return ReactionType.ElectroCharged;
            if (existing == ElementType.Cryo) return ReactionType.Superconduct;
        }
        else if (trigger == ElementType.Cryo)
        {
            if (existing == ElementType.Pyro) return ReactionType.Melt;
            if (existing == ElementType.Hydro) return ReactionType.Frozen;
            if (existing == ElementType.Electro) return ReactionType.Superconduct;
        }

        return ReactionType.None;
    }

    // Execute reaction effects
    private void TriggerReaction(ReactionType reaction, ElementalStatus target,
                               ElementType trigger, ElementType existing,
                               Vector3 position, float baseDamage)
    {
        // Consume elements
        target.ApplyElement(existing, -1f);
        target.ApplyElement(trigger, -0.5f);

        // Apply reaction effects
        switch (reaction)
        {
            case ReactionType.Vaporize:
                ApplyDamage(target, baseDamage * _vaporizeMultiplier, ElementType.Pyro);
                Debug.Log("Vaporized, Dealt: " + (baseDamage * _vaporizeMultiplier) + " Damage");
                break;

            case ReactionType.Melt:
                ApplyDamage(target, baseDamage * _meltMultiplier, ElementType.Pyro);
                Debug.Log("Melt, Dealt: " + (baseDamage * _meltMultiplier) + " Damage");
                break;

            case ReactionType.Overload:
                ApplyDamage(target, baseDamage * _overloadMultiplier, ElementType.Pyro);
                Debug.Log("Overload, Dealt: " + (baseDamage * _meltMultiplier) + " Damage");
                ApplyAOE(position, 3f, baseDamage * 0.7f);
                Debug.Log("Overload AOE, Dealt: " + (baseDamage * 0.7f) + " Damage");

                break;

            case ReactionType.ElectroCharged:
                StartCoroutine(ElectroChargedEffect(target, baseDamage));
                break;

            case ReactionType.Frozen:
                ApplyFrozenEffect(target);
                break;

            case ReactionType.Superconduct:
                ApplySuperconductEffect(target, baseDamage * 0.8f);
                break;
        }

        // Play VFX/SFX
        //ReactionVFXManager.Instance.PlayReactionVFX(reaction, position);
    }

    // Special reaction effects
    private IEnumerator ElectroChargedEffect(ElementalStatus target, float baseDamage)
    {
        for (int i = 0; i < 3; i++)
        {
            ApplyDamage(target, baseDamage * 0.3f, ElementType.Electro);
            yield return new WaitForSeconds(0.5f);
            Debug.Log("ElectroChargedEffect, Dealt: " + (baseDamage * 0.3) + " Damage");
        }
    }

    private void ApplyFrozenEffect(ElementalStatus target)
    {
        //if (target.TryGetComponent<Movement>(out var movement))
        //{
        //    movement.ApplyFreeze(2f); // Freeze for 2 seconds
        //}

        Debug.Log(" your ass got frozen");
    }

    private void ApplySuperconductEffect(ElementalStatus target, float damage)
    {
        //ApplyDamage(target, damage, ElementType.Cryo);

        //if (target.TryGetComponent<Defense>(out var defense))
        //{
        //    defense.ApplyDefenseDebuff(0.7f, _superconductDuration); // 30% defense reduction
        //}

        Debug.Log(" your superconducgted ");
    }

    // Helper methods
    private void ApplyDamage(ElementalStatus target, float amount, ElementType element)
    {
        if (target.TryGetComponent<Enemy>(out var health))
        {
            // health.TakeElementalDamage(amount, element);
        }
    }

    private void ApplyAOE(Vector3 center, float radius, float damage)
    {
        Collider[] hits = Physics.OverlapSphere(center, radius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<Enemy>(out var health))
            {
                health.TakeDamage(damage);
            }
        }
    }
}

// ElementType.cs
public enum ElementType
{
    Pyro,       // Fire
    Hydro,      // Water
    Electro,    // Electricity
    Cryo,       // Ice
    None        // Neutral
}

// ReactionType.cs
public enum ReactionType
{
    Vaporize,       // Pyro + Hydro
    Overload,       // Pyro + Electro
    ElectroCharged, // Hydro + Electro
    Frozen,         // Hydro + Cryo
    Melt,           // Pyro + Cryo
    Superconduct,   // Cryo + Electro
    None
}