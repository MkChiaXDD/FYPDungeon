
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Buff", menuName = "Buffs")]
public class BuffData : ScriptableObject
{
    public enum Rarity
    {
        Common,
        UnCommon,
        Rare,
        Epic,
        Legendary
    }
    public Rarity rarity;

    public string Name;
    public string Description;
    public Sprite Icon;
    public List<Effect> EffectList;

    [Header("Dependencies")]
    public List<BuffData> RequiredBuffs;
    public List<BuffData> CorrespondingBuffs;
    public bool IsHiddenIfLocked = true;
    public bool OneTimeUnlock = false;

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
        Health,
        DashCoolDown,

        Fire_Element,
        Electric_Element,
 

        LifeSteal,
        DamageStore,
        CritChance,
        StatusEffectSpread,
        Evade,
        Sacrifice, //Sacrifice Health To get buff
        Mimic, //Copy move
        Influence, // temp turn enemy to friend
        Link, // 
        Perfection,// Do more dmg whem max hp
        MimicDuration,
        MimicCastAmount,
        MimicCastChance,
        
        End
    }

    public enum ModifierType
    {
        MultiplierValue,
        FlatValue,
        TimedEffect,
        ChanceBased,
        Ability,
        
    }

    public EffectType Type;
    public ModifierType ValueModifierType;
    public float ModifierValue;
    public float Duration;
    public float Chance;

}