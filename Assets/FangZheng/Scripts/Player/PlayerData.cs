
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PlayerData : MonoBehaviour, IDamageable
{
    [SerializeField] private Animator _animator;
    [Space, Header("Base Stats")]
    [SerializeField] private float _MaxHealth = 100;
    [SerializeField] private int _Dmg = 5;
    [SerializeField] private float _Speed = 20;
    [SerializeField] private float _Dash = 40;
    [SerializeField] private float _ParryDuration = 4;
    [SerializeField] private float _ParryThreshold = 0.5f;
    [SerializeField] private float _DashCooldown = 0.5f;
    [SerializeField] private int _MimicAmount = 1;
    [SerializeField, Range(0f, 1f)] private float _MimicSpawnChance = 0.05f;
    [SerializeField] private PlayerHitEffect _HitEffect;


    public bool _InVin;
    public float _LifeStealAmount;
    public float _DmgStoreAmount;
    public float _CritChance;
    public float _EvadeChance;
    public bool _Sacrifice;

    public bool _Mimic;


    public bool _Influence;
    public bool _Link;
    public bool _Perfection;

    public MimicSpawner mimic;

    [Space, Header("Buffs")]
    [SerializeField] public List<BuffData> _BuffObtain;

    public UnityEvent DataChange;

    public float CurrentHealth { get;  set; }
    public float MaxHealth { get; private set; }
    public bool _isInvulnerable { get; private set; }
    public int Damage { get; private set; }
    public float Speed { get; private set; }
    public float Dash { get; private set; }
    public float ParryDuration { get; private set; }
    public float ParryThreshhold { get; private set; }
    public float Health { get; private set; }
    public float MimicSpawnChance { get; private set; }
    public int MimicCount { get; private set; }

    public float DashCooldown { get; private set; }
    public static PlayerData Instance { get; private set; }
    // Elemental status effects
    private Dictionary<ElementType, float> activeElementalEffects = new Dictionary<ElementType, float>();

    [SerializeField] private Canvas DeathCanvas;

    private bool playerCheating = false;

    [SerializeField] private DamagedVFX damagedVFX;


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
     


        CurrentHealth = _MaxHealth;
        ResetToBaseStats();
    }


    public void TakeDamage(float damage)
    {
        if (_InVin)
        {
            return;
        }
        if (_isInvulnerable == false)
        {
            _animator.SetTrigger("Hurt");
            CurrentHealth = CurrentHealth - damage;
            Debug.Log("ouch");
            // Trigger flash effect
            if (damagedVFX != null)
            {
                damagedVFX.TriggerDamageFlash();
            }
            SoundManager.Instance.PlaySFX("HitSFX", this.gameObject);

            if (_HitEffect != null)
            {
                _HitEffect.TriggerHit();
            }
        }

        if (CurrentHealth <= 0)
        {
            Die();
        }

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            SetInv();
        }

        if (Input.GetKeyDown(KeyCode.F12))
        {
            playerCheating = !playerCheating;
            PlayerGeCheat();
        }
    }

    public void SetInv()
    {

        if (_InVin == false)
        {
            _InVin = true;
        }
        else
        {
            _InVin = false;
        }

    }

    public virtual void Heal(float healAmount)
    {
        if (CurrentHealth < MaxHealth)
        {
            CurrentHealth += healAmount;

            Debug.Log("healed " + healAmount);
            if (CurrentHealth > MaxHealth)
            {
                CurrentHealth = MaxHealth;
            }
        }
    }

    public void Die()
    {

        Debug.Log("die");
        DeathCanvas.gameObject.SetActive(true);
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
        ParryDuration = _ParryDuration;
        ParryThreshhold = _ParryThreshold;
        MimicSpawnChance = _MimicSpawnChance;
        MimicCount = _MimicAmount;
        DashCooldown = _DashCooldown;

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
                            //CurrentHealth += (_MaxHealth * effect.ModifierValue) - _MaxHealth;
                        }
                        else
                        {
                            MaxHealth += effect.ModifierValue;
                            //CurrentHealth += effect.ModifierValue;
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
                            ParryDuration += (_ParryDuration * effect.ModifierValue) - _ParryDuration;
                        }
                        else
                        {
                            ParryDuration += effect.ModifierValue;
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
                        //PlayerCombat.Instance.EnableMimic(_Mimic);
                        if (mimic == null)
                        {
                            mimic = this.AddComponent<MimicSpawner>();
                            PlayerCombat.Instance.SetUpMimic(mimic);
                        }
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
                    case Effect.EffectType.MimicCastChance:
                        if (effect.ValueModifierType == Effect.ModifierType.MultiplierValue)
                        {
                            MimicSpawnChance += (_MimicSpawnChance * effect.ModifierValue) - _MimicSpawnChance;
                        }
                        else
                        {
                            MimicSpawnChance += effect.ModifierValue;
                        }

                        break;
                    case Effect.EffectType.MimicCastAmount:
                        MimicCount += (int)effect.ModifierValue;
                        break;
                }
            }
        }
        if (MaxHealth <= 0)
        {
            MaxHealth = 1;

        }

        if (CurrentHealth > MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }

        if (Damage <= 0)
        {
            Damage = 1;
        }

        if (Speed <= 0)
        {
            Speed = 0.1f;
        }

        if (Dash <= 0)
        {
            Dash = 1;
        }

        if (ParryDuration <= 0)
        {
            ParryDuration = 0.1f;
        }

        //if (MimicSpawnChance > 0.75f)
        //{
        //    MimicSpawnChance = 0.75f;
        //}
        DataChange?.Invoke();

    }





    public void TakeElementalDamage(float damage, ElementType elementType)
    {
        if (_InVin == false)
        {
            return;
        }
        // Apply elemental effect (burning, electrocution, etc.)
        ApplyElementalEffect(elementType, damage);
        TakeDamage(damage);

    }

    private void ApplyElementalEffect(ElementType elementType, float damageAmount)
    {

        // Example: Apply burning effect for Pyro damage
        if (elementType == ElementType.Pyro)
        {
            // Start or refresh burning effect*
            if (TryGetComponent<BurningEffect>(out var burning))
            {
                burning.RefreshEffect(damageAmount);
            }
            else
            {

                burning = gameObject.AddComponent<BurningEffect>();
                burning.Initialize(damageAmount, this);
            }
        }

        // Add similar effects for other elements:
        // - Hydro: Wet status (increased Electro damage)
        // - Electro: Stun effect
        // - Cryo: Slow movement

        // Track elemental effect for visual feedback
        activeElementalEffects[elementType] = Time.time + 3f; // Effect lasts 3 seconds

    }

    public void TakePhysicalDamage(float damage, PhysicalAttackType attackType) => TakeDamage(damage);


    private void PlayerGeCheat()
    {
        if (!playerCheating)
        {
            ResetToBaseStats();
            return;
        }

        MaxHealth = 1000000;
        CurrentHealth = 1000000;



    }
}
