using System.Collections;
using System.Collections.Generic;
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
    public float Health{ get; private set; }

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
                }
            }
        }

        DataChange?.Invoke();

    }
}
