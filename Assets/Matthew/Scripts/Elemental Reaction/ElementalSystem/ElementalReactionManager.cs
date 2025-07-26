using System.Collections;
using UnityEngine;

public class ElementalReactionManager : MonoBehaviour
{
    public static ElementalReactionManager Instance;

    [Header("Elemental VFX References")]
    public GameObject FireVFX;
    public GameObject ElectricVFX;
    public GameObject WaterVFX;
    [Header("Elemental Reaction VFX References")]
    public GameObject OverloadedVFX;

    [Header("OverloadConfig")]
    [SerializeField] private float OverloadAOERange = 10;

    [Header("Reaction Multipliers")]
    [SerializeField] private float _vaporizeMultiplier = 1.5f;
    [SerializeField] private float _meltMultiplier = 2f;
    [SerializeField] private float _overloadMultiplier = 1.2f;
    [SerializeField] private float _superconductDuration = 5f;

    void Awake()
    {
        Instance = this;
        Debug.Log("[ElementalSystem] Reaction Manager Initialized");
    }

    // Check for possible reactions
    public void CheckReactions(ElementalStatus target, ElementType triggerElement, Vector3 position, float baseDamage)
    {
        Debug.Log($"[ElementalSystem] Checking reactions on {target.gameObject.name}. Trigger: {triggerElement}, Base DMG: {baseDamage}");
        Debug.Log($"[ElementalSystem] Active elements on target:");

        bool reactionFound = false;
        foreach (var existingElement in target.GetActiveElements())
        {
            Debug.Log($"  - {existingElement.Key} (Gauge: {existingElement.Value})");
            ReactionType reaction = GetReactionType(triggerElement, existingElement.Key);

            if (reaction != ReactionType.None)
            {
                Debug.Log($"[ElementalSystem] REACTION DETECTED: {triggerElement} + {existingElement.Key} = {reaction}");
                TriggerReaction(reaction, target, triggerElement, existingElement.Key, position, baseDamage);
                reactionFound = true;
                break; // Only trigger one reaction per attack
            }
        }

        if (!reactionFound)
        {
            Debug.Log($"[ElementalSystem] No reaction triggered for {triggerElement} on target elements");
        }
    }

    // Define reaction rules
    private ReactionType GetReactionType(ElementType trigger, ElementType existing)
    {
        Debug.Log($"[ElementalSystem] Checking reaction between TRIGGER: {trigger} and EXISTING: {existing}");

        

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

        Debug.Log($"[ElementalSystem] No valid reaction between {trigger} and {existing}");
        return ReactionType.None;
    }

    // Execute reaction effects
    private void TriggerReaction(ReactionType reaction, ElementalStatus target,
                               ElementType trigger, ElementType existing,
                               Vector3 position, float baseDamage)
    {
        Debug.Log($"[Reaction] STARTING {reaction} reaction on {target.gameObject.name}");

        // Log element consumption
        Debug.Log($"[Reaction] Consuming elements: {trigger} and {existing}");
        target.ApplyElement(existing, -5f);
        target.ApplyElement(trigger, -5f);

        // Apply reaction effects
        switch (reaction)
        {
            case ReactionType.Vaporize:
                float vaporizeDamage = baseDamage * _vaporizeMultiplier;
                Debug.Log($"[Vaporize] Applying {vaporizeDamage} damage (Base: {baseDamage} x {_vaporizeMultiplier})");
                ApplyDamage(target, vaporizeDamage, ElementType.Pyro);
                break;

            case ReactionType.Melt:
                float meltDamage = baseDamage * _meltMultiplier;
                Debug.Log($"[Melt] Applying {meltDamage} damage (Base: {baseDamage} x {_meltMultiplier})");
                ApplyDamage(target, meltDamage, ElementType.Pyro);
                break;

            case ReactionType.Overload:
                Debug.Log($"[Overload] Creating explosion at {position}");
                Instantiate(OverloadedVFX, target.gameObject.transform);

                float overloadDamage = baseDamage * _overloadMultiplier;
                Debug.Log($"[Overload] Main target damage: {overloadDamage} (Base: {baseDamage} x {_overloadMultiplier})");
                ApplyDamage(target, overloadDamage, ElementType.Pyro);

                float aoeDamage = baseDamage * 0.7f;
                Debug.Log($"[Overload] Applying AOE damage: {aoeDamage} in 3m radius");
                ApplyAOE(position, OverloadAOERange, aoeDamage);
                break;

            case ReactionType.ElectroCharged:
                Debug.Log($"[Electro-Charged] Starting chain reaction on {target.gameObject.name}");
                StartCoroutine(ElectroChargedEffect(target, baseDamage));
                break;

            case ReactionType.Frozen:
                Debug.Log($"[Frozen] Applying freeze effect to {target.gameObject.name}");
                ApplyFrozenEffect(target);
                break;

            case ReactionType.Superconduct:
                float scDamage = baseDamage * 0.8f;
                Debug.Log($"[Superconduct] Applying {scDamage} damage + defense debuff for {_superconductDuration}s");
                ApplySuperconductEffect(target, scDamage);
                break;
        }

        Debug.Log($"[Reaction] COMPLETED {reaction} reaction");
    }

    // Special reaction effects
    private IEnumerator ElectroChargedEffect(ElementalStatus target, float baseDamage)
    {
        Debug.Log($"[Electro-Charged] Starting 3-tick damage sequence");
        float tickDamage = baseDamage * 0.3f;

        for (int i = 0; i < 3; i++)
        {
            Debug.Log($"[Electro-Charged] Tick {i + 1}: Applying {tickDamage} damage");
            ApplyDamage(target, tickDamage, ElementType.Electro);
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log($"[Electro-Charged] Chain reaction completed");
    }

    private void ApplyFrozenEffect(ElementalStatus target)
    {
        Debug.Log($"[Frozen] Freezing {target.gameObject.name} for 2 seconds");
        // Actual implementation would go here
    }

    private void ApplySuperconductEffect(ElementalStatus target, float damage)
    {
        Debug.Log($"[Superconduct] Applying {damage} cryo damage to {target.gameObject.name}");
        Debug.Log($"[Superconduct] Reducing defense by 30% for {_superconductDuration} seconds");
        // Actual implementation would go here
    }

    // Helper methods
    private void ApplyDamage(ElementalStatus target, float amount, ElementType element)
    {
        Debug.Log($"[Damage] Applying {amount} {element} damage to {target.gameObject.name}");
        // Actual damage implementation would go here
    }

    private void ApplyAOE(Vector3 center, float radius, float damage)
    {
        Debug.Log($"[AOE] Checking for targets in {radius}m radius at {center}");
        Collider[] hits = Physics.OverlapSphere(center, radius);
        Debug.Log($"[AOE] Found {hits.Length} potential targets");

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<Enemy>(out var health))
            {
                Debug.Log($"[AOE] Applying {damage} damage to {hit.gameObject.name}");
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

public enum PhysicalAttackType
{
    Sharp, //sharp
    Blunt, //not sharp
    None
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