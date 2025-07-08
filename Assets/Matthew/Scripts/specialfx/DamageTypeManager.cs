using UnityEngine;
using System.Collections.Generic;

public class DamageTypeManager : MonoBehaviour
{
    public static DamageTypeManager Instance;

    [Header("Damage Multipliers")]
    [SerializeField] private float _sharpVsSoftMultiplier = 2f;
    [SerializeField] private float _sharpVsHardMultiplier = 0.5f;
    [SerializeField] private float _bluntVsSoftMultiplier = 0.5f;
    [SerializeField] private float _bluntVsHardMultiplier = 2f;

    void Awake()
    {
        Instance = this;
        Debug.Log("[DamageSystem] Resistance Manager Initialized");
    }

  

    public void ApplyDamage(ResistanceProfile target, DamageType damageType, float baseDamage)
    {
        Debug.Log($"[DamageSystem] Applying {damageType} damage to {target.gameObject.name}. Base DMG: {baseDamage}");

        ResistanceType resistance = target.GetPrimaryResistance();
        float damageMultiplier = GetDamageMultiplier(damageType, resistance);
        float finalDamage = baseDamage * damageMultiplier;

        Debug.Log($"[DamageSystem] Target resistance: {resistance}. " +
                 $"Multiplier: {damageMultiplier}x. Final DMG: {finalDamage}");

        target.ApplyDamage(finalDamage);
    }

    private float GetDamageMultiplier(DamageType damageType, ResistanceType resistance)
    {
        return (damageType, resistance) switch
        {
            (DamageType.Sharp, ResistanceType.Soft) => _sharpVsSoftMultiplier,
            (DamageType.Sharp, ResistanceType.Hard) => _sharpVsHardMultiplier,
            (DamageType.Blunt, ResistanceType.Soft) => _bluntVsSoftMultiplier,
            (DamageType.Blunt, ResistanceType.Hard) => _bluntVsHardMultiplier,
            _ => 1f
        };
    }
}

public enum DamageType
{
    Sharp,
    Blunt
}

public enum ResistanceType
{
    Soft,
    Hard
}

public class ResistanceProfile : MonoBehaviour
{
    [SerializeField] private ResistanceType _primaryResistance;
    [SerializeField] private bool _showDebug = true;

    public ResistanceType GetPrimaryResistance() => _primaryResistance;

    public void ApplyDamage(float amount)
    {
        Debug.Log($"[Damage] Applying {amount} damage to {gameObject.name}");

        if (TryGetComponent<Enemy>(out var enemy))
        {
            enemy.TakeDamage(amount);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!_showDebug) return;

        Color resistanceColor = _primaryResistance == ResistanceType.Soft
            ? Color.blue
            : Color.gray;

        Gizmos.color = resistanceColor;
        Gizmos.DrawWireSphere(transform.position + Vector3.up, 0.5f);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 1.5f,
            $"Resistance: {_primaryResistance}"
        );
#endif
    }
}