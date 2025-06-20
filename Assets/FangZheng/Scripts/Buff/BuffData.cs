using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Buff", menuName = "Buffs")]
public class BuffData : ScriptableObject
{
    public string Name;
    public string Description;
    public Sprite Icon;
    public List<Effect> EffectList;
}


[System.Serializable]
public class Effect
{
    public enum EffectType
    {
        None,
        Damage,
        MovementSpeed,
        DashSpeed,
        ParryCooldown,
        Health
    }

    public enum ModifierType
    {
        MultiplierValue,
        FlatValue
    }

    public EffectType Type;
    public ModifierType ValueModifierType;
    public float ModifierValue;
}