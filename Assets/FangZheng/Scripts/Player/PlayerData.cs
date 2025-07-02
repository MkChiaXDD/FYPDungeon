using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PlayerData : MonoBehaviour, IDamageable
{
    [Space, Header("Base Stats")]
    [SerializeField] private float _MaxHealth = 100;
    [SerializeField] private int _Dmg = 5;
    [SerializeField] private float _Speed = 20;
    [SerializeField] private float _Dash = 40;
    [SerializeField] private float _ParryDuration = 4;
    [SerializeField] private float _ParryThreshold = 0.5f;

    public float _LifeStealAmount;
    public float _DmgStoreAmount;
    public float _CritChance;
    public float _EvadeChance;
    public bool _Sacrifice;
    public bool _Mimic;
    public bool _Influence;
    public bool _Link;
    public bool _Perfection;

    public MimicClone mimic;

    [Space, Header("Buffs")]
    [SerializeField] private List<BuffData> _BuffObtain;

    public UnityEvent DataChange;

    public float CurrentHealth { get; private set; }
    public float MaxHealth { get; private set; }
    public bool _isInvulnerable { get; private set; }
    public int Damage { get; private set; }
    public float Speed { get; private set; }
    public float Dash { get; private set; }
    public float ParryTime { get; private set; }
    public float ParryThreshhold { get; private set; }
    public float Health { get; private set; }

    public static PlayerData Instance { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(this.gameObject);

        CurrentHealth = _MaxHealth;
        ResetToBaseStats();
    }


    public void TakeDamage(float damage)
    {
        if (_isInvulnerable == false)
        {
            CurrentHealth = CurrentHealth - damage;
            Debug.Log("ouch");
        }
    }

    public virtual void Heal(float healAmount)
    {
        if (CurrentHealth < MaxHealth)
        {
            CurrentHealth += healAmount;
            if (CurrentHealth > MaxHealth)
            {
                CurrentHealth = MaxHealth;
            }
        }
    }

    public void Die()
    {
        Debug.Log("die");
    }

    public void SetInv(bool state)
    {
        _isInvulnerable = state;
    }

    private void ResetToBaseStats()
    {
        MaxHealth = _MaxHealth;
        Damage = _Dmg;
        Speed = _Speed;
        Dash = _Dash;
        ParryTime = _ParryDuration;
        ParryThreshhold = _ParryThreshold;

        _LifeStealAmount = 0f;
        _CritChance = 0f;
        _EvadeChance = 0f;
        _Sacrifice = false;
        _Mimic = false;
        _Influence = false;
        _Link = false;
        _Perfection = false;
    }

    public void AddBuff(BuffData buff)
    {
        _BuffObtain.Add(buff);
        ApplyModifiers();
    }

    public void ApplyModifiers()
    {
        ResetToBaseStats();
        float BeforeEverythingHealth = MaxHealth;
        float CurHealth = CurrentHealth;
        foreach (var buff in _BuffObtain)
        {
            foreach (Effect effect in buff.EffectList)
            {
                switch (effect.Type)
                {
                    case Effect.EffectType.Health:
                        if (effect.ValueModifierType == Effect.ModifierType.MultiplierValue)
                        {
                            MaxHealth += (_MaxHealth * effect.ModifierValue) - _MaxHealth;
                            CurrentHealth += (_MaxHealth * effect.ModifierValue) - _MaxHealth;
                        }
                        else
                        {
                            MaxHealth += effect.ModifierValue;
                            CurrentHealth += effect.ModifierValue;
                        }
                        break;
                    case Effect.EffectType.Damage:
                        if (effect.ValueModifierType == Effect.ModifierType.MultiplierValue)
                        {
                            Damage += (int)(_Dmg * effect.ModifierValue) - _Dmg;
                        }
                        else
                        {
                            Damage += (int)effect.ModifierValue;
                        }
                        break;
                    case Effect.EffectType.MovementSpeed:
                        if (effect.ValueModifierType == Effect.ModifierType.MultiplierValue)
                        {
                            Speed += (_Speed * effect.ModifierValue) - _Speed;
                        }
                        else
                        {
                            Speed += effect.ModifierValue;
                        }

                        break;
                    case Effect.EffectType.DashSpeed:
                        if (effect.ValueModifierType == Effect.ModifierType.MultiplierValue)
                        {
                            Dash += (_Dash * effect.ModifierValue) - _Dash;
                        }
                        else
                        {
                            Dash += effect.ModifierValue;
                        }
                        break;
                    case Effect.EffectType.ParryCooldown:
                        if (effect.ValueModifierType == Effect.ModifierType.MultiplierValue)
                        {
                            ParryTime += (_ParryDuration * effect.ModifierValue) - _ParryDuration;
                        }
                        else
                        {
                            ParryTime += effect.ModifierValue;
                        }
                        break;
                    case Effect.EffectType.LifeSteal:
                        _LifeStealAmount += effect.ModifierValue;
                        break;
                    case Effect.EffectType.Evade:
                        _EvadeChance = Mathf.Clamp(_EvadeChance + effect.ModifierValue, 0f, 0.9f);
                        break;
                    case Effect.EffectType.CritChance:
                        _CritChance = Mathf.Clamp(_CritChance + effect.ModifierValue, 0f, 1f);
                        break;
                    case Effect.EffectType.Sacrifice:
                        _Sacrifice = true;
                        break;
                    case Effect.EffectType.Mimic:
                        _Mimic = true;
                        mimic = this.AddComponent<MimicClone>();
                        break;
                    case Effect.EffectType.Influence:
                        _Influence = true;
                        break;
                    case Effect.EffectType.Link:
                        _Link = true;
                        break;
                    case Effect.EffectType.Perfection:
                        _Perfection = true;
                        break;
                }
            }
        }

        DataChange?.Invoke();

    }

    public void TakeElementalDamage(float damage, ElementType element)
    {
        TakeDamage(damage);
    }
}
