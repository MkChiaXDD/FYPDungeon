using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("References/Components")]
    [SerializeField] protected EnemyData data;
    [SerializeField] DynamicHealthBar healthBar;
    [SerializeField] public float currentHealth;
    [SerializeField] private GameObject shieldPrefab;
    private GameObject enemyShield;
    [SerializeField] private float shieldHp;

    [Header("Movement")]
    [SerializeField] protected float currentMoveSpeed;
    public float CurrentMoveSpeed
    {
        get => currentMoveSpeed;
        set => currentMoveSpeed = Mathf.Max(0, value); // Ensure speed doesn't go negative
    }

    // Take damage VFX
    private EnemyHitSquash hitSquashEffect;
    private Coroutine hitEffectCoroutine;
    private bool isHitEffectActive = false;

    [Header("Hit Effect")]
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float hitDuration = 1f;
    [SerializeField] private Renderer enemyRenderer;
    [SerializeField] private bool useEmission = true;
    private Color originalColor;
    private Color originalEmissionColor;
    [SerializeField] private int _totalFlashes = 1;
    [SerializeField] private float dmgFlashSpeed = 8;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    public float maxHealth => data.maxHealth;
    protected int currentRound;
    protected float damage;

    [Header("Elemental Resistances")]
    [Tooltip("1 = Normal, >1 = Resistant, <1 = Weak")]
    [Range(0, 2)] public float pyroResistance = 1f;
    [Range(0, 2)] public float hydroResistance = 1f;
    [Range(0, 2)] public float electroResistance = 1f;
    [Range(0, 2)] public float cryoResistance = 1f;

    // Elemental status effects
    private Dictionary<ElementType, float> activeElementalEffects = new Dictionary<ElementType, float>();

    [SerializeField] protected float stunDuration = 1f;
    protected bool isStunned = false;
    protected Coroutine stunCoroutine;

    protected virtual void Awake()
    {
        _totalFlashes = 1;
        dmgFlashSpeed = 8;
        hitSquashEffect = gameObject.AddComponent<EnemyHitSquash>();
        currentMoveSpeed = data.moveSpeed; // Initialize speed
        InitialiseDifficulty();
        InitialiseShield();
        Invoke(nameof(InitialiseHealthBar), 1f);
        transform.AddComponent<ElementalStatus>();

        enemyRenderer = this.GetComponent<Renderer>();
        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
            if (useEmission && enemyRenderer.material.HasProperty(EmissionColor))
            {
                originalEmissionColor = enemyRenderer.material.GetColor(EmissionColor);
            }
        }
    }

    #region Speed Control Methods
    public virtual void SetSpeed(float newSpeed)
    {
        currentMoveSpeed = Mathf.Max(0, newSpeed);
    }

    public virtual void MultiplySpeed(float multiplier)
    {
        currentMoveSpeed = Mathf.Max(0, data.moveSpeed * multiplier);
    }

    public virtual void ResetSpeed()
    {
        currentMoveSpeed = data.moveSpeed;
    }

    public virtual float GetOriginalSpeed()
    {
        return data.moveSpeed;
    }
    #endregion

    public void InitialiseShield()
    {
        if (shieldPrefab != null && enemyShield == null)
        {
            enemyShield = Instantiate(shieldPrefab, transform.position, Quaternion.identity);
            enemyShield.transform.SetParent(transform);
            enemyShield.transform.localPosition = new Vector3(0, 1.5f, 0);

            EnemyShield shield = enemyShield.GetComponent<EnemyShield>();
            if (shield != null)
            {
                shield.Init(shieldHp);
            }
            else
            {
                Debug.LogWarning("Spawned shield is missing EnemyShield component.");
            }
        }
    }

    private void ShieldTakeDamage(float amount, PhysicalAttackType physicalAttackType)
    {
        EnemyShield shield = enemyShield.GetComponent<EnemyShield>();
        float currentShieldHealth = shield.GetShieldHp();
        if (currentShieldHealth > 0)
        {
            shield.HitShield(amount, physicalAttackType);
            return;
        }
    }

    private void PlayDamagedVFX()
    {
        SoundManager.Instance.PlaySFX("HitSFX");
        hitSquashEffect.PlaySquashEffect();
        PlayHitEffects();
        PlayHitStopVFX();
        ParticalManager.Instance.Bleed(this.transform);


    }

    private void PlayHitEffects()
    {
        if (enemyRenderer == null) return;

        if (isHitEffectActive && hitEffectCoroutine != null)
        {
            StopCoroutine(hitEffectCoroutine);
        }

        hitEffectCoroutine = StartCoroutine(HitEffectAnimation());
    }

    private IEnumerator HitEffectAnimation()
    {
        isHitEffectActive = true;
        float timer = 0f;
        int flashCount = 0;
        int totalFlashes = _totalFlashes;
        float flashSpeed = dmgFlashSpeed;

        Material material = enemyRenderer.material;
        Color baseColor = originalColor;
        Color baseEmission = useEmission && material.HasProperty(EmissionColor) ?
                             material.GetColor(EmissionColor) : Color.black;

        while (timer < hitDuration && flashCount < totalFlashes * 2)
        {
            timer += Time.deltaTime;
            float pingPongValue = Mathf.PingPong(timer * flashSpeed, 1f);
            material.color = Color.Lerp(baseColor, hitColor, pingPongValue);

            if (useEmission && material.HasProperty(EmissionColor))
            {
                Color currentEmission = Color.Lerp(baseEmission, Color.red, pingPongValue);
                material.SetColor(EmissionColor, currentEmission);
                material.EnableKeyword("_EMISSION");
            }

            if (pingPongValue < 0.1f)
            {
                flashCount++;
            }

            yield return null;
        }

        material.color = baseColor;
        if (useEmission && material.HasProperty(EmissionColor))
        {
            material.SetColor(EmissionColor, baseEmission);
        }

        isHitEffectActive = false;
    }

    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;
        ShowDamageNumber(this.transform.position, amount);
        UpdateHealthBar();
        PlayDamagedVFX();
        if (PlayerData.Instance._LifeStealAmount > 0) {

            float healAmount = amount * PlayerData.Instance._LifeStealAmount;
            healAmount = Mathf.Max(healAmount, 0.1f);
            double rounded = System.Math.Round(healAmount, 1);
            float e = (float)rounded;
            Debug.Log(e);
            PlayerData.Instance.Heal(e);
        }
        if (currentHealth <= 0f)
            Die();
    }

    public void TakeElementalDamage(float amount, ElementType elementType)
    {
        float resistanceMultiplier = GetResistanceMultiplier(elementType);
        float finalDamage = amount / resistanceMultiplier;

        ApplyElementalEffect(elementType, finalDamage);
        TakeDamage(finalDamage);
    }

    public void TakePhysicalDamage(float damage, PhysicalAttackType attackType)
    {
        if (enemyShield != null && enemyShield.GetComponent<EnemyShield>().GetShieldHp() > 0)
        {
            if (enemyShield.GetComponent<EnemyShield>())
                ShieldTakeDamage(damage, attackType);
            ShowDamageNumber(this.transform.position, damage, Color.gray);
            return;
        }

        float resistanceMultiplier = GetResistanceMultiplier(attackType);
        float finalDamage = damage / resistanceMultiplier;

        TakeDamage(finalDamage);
    }

    private float GetResistanceMultiplier(ElementType elementType)
    {
        return elementType switch
        {
            ElementType.Pyro => pyroResistance,
            ElementType.Hydro => hydroResistance,
            ElementType.Electro => electroResistance,
            ElementType.Cryo => cryoResistance,
            _ => 1f
        };
    }

    private float GetResistanceMultiplier(PhysicalAttackType elementType)
    {
        return elementType switch
        {
            PhysicalAttackType.Sharp => pyroResistance,
            PhysicalAttackType.Blunt => hydroResistance,
            _ => 1f
        };
    }

    public virtual void Die()
    {
        //if (gameObject.GetComponent<BossCheckDeath>() != null)
        //{
        //    gameObject.GetComponent<BossCheckDeath>().SummonPortal();
        //    PopUpManager.ShowPopUp("Boss Killed, Carry on to the next level", 3, Color.green);
        //    FindFirstObjectByType<EnemyTracker>().SetCustomText("Proceed to the portal");
        //    Destroy(gameObject.GetComponent<BossCheckDeath>());
        //}

        FindFirstObjectByType<EnemyTracker>()?.IncreaseKills();
        StaticScreenShake.Shake(Camera.main, deathParams);
        Destroy(gameObject);
    }

    private void InitialiseHealthBar()
    {
        healthBar.SetMaxHealth(currentHealth);
        UpdateHealthBar();
    }

    private void InitialiseDifficulty()
    {
        DifficultyManager difficulty = FindFirstObjectByType<DifficultyManager>();

        float multiplier = 1f;
        currentRound = 1;

        if (difficulty != null)
        {
            currentRound = difficulty.GetRound();
            multiplier = difficulty.GetDifficultyMultiplier();
        }
        else
        {
            Debug.LogWarning("[Enemy] No DifficultyManager found. Using default values.");
        }

        int finalHealth = Mathf.RoundToInt(data.maxHealth * multiplier);
        currentHealth = finalHealth;
        damage = data.damage;

        InitialiseResistance();
    }

    private void ShowDamageNumber(Vector3 position, float damage, Color color)
    {
        if (DamageNumberManager.Instance)
        {
            DamageNumberManager.Instance.ShowDamage(position, damage, color);
        }
    }

    private void ShowDamageNumber(Vector3 position, float damage)
    {
        if (DamageNumberManager.Instance)
        {
            DamageNumberManager.Instance.ShowDamage(position, damage);
        }
    }

    private void InitialiseResistance()
    {
        pyroResistance = data.pyroResistance;
        cryoResistance = data.cryoResistance;
        electroResistance = data.electroResistance;
        hydroResistance = data.hydroResistance;
    }

    private void UpdateHealthBar()
    {
        healthBar.SetHealth(currentHealth);
    }

    private void ApplyElementalEffect(ElementType elementType, float damageAmount)
    {
        if (elementType == ElementType.Pyro)
        {
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
        if (elementType == ElementType.Electro)
        {
            if (TryGetComponent<ElectroEffect>(out var electro))
            {
                electro.RefreshEffect(damageAmount);
            }
            else
            {
                electro = gameObject.AddComponent<ElectroEffect>();
                electro.Initialize(damageAmount, this);
            }
        }

        activeElementalEffects[elementType] = Time.time + 3f;
    }

    public virtual void ApplyStun(float duration)
    {
        if (data.enemyType == EnemyData.EnemyType.notNormalEnemy) return;

        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);

        stunCoroutine = StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        yield return new WaitForSeconds(duration);
        isStunned = false;
        stunCoroutine = null;
    }

    public virtual void Heal(float healAmount)
    {
        if (currentHealth < data.maxHealth)
        {
            currentHealth += healAmount;
            if (currentHealth > data.maxHealth)
            {
                currentHealth = data.maxHealth;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, data.detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.attackRange);
    }



    private void PlayHitStopVFX()
    {
        HitStopManager.ActivateHitStopGlobal();
    }

    protected StaticScreenShake.ShakeParams deathParams = new()
    {
        ShakeType = ShakeType.Translational,
        ShakeDuration = 0.25f,
        ShakeMagnitude = 2.5f,
        DampingSpeed = 10f,
        RotationalNoiseSpeed = 20f,
        TranslationalShakeMagnitude = new Vector3(0.5f, 0.5f, 0f),
        TranslationalNoiseSpeed = 50f,
        UseSeparateNoiseForTranslation = true,
        EnableX = false,
        EnableY = true,
        EnableZ = false
    };

    protected StaticScreenShake.ShakeParams strongerShake = new()
    {
        ShakeType = ShakeType.Translational,
        ShakeDuration = 0.25f,
        ShakeMagnitude = 2.5f,
        DampingSpeed = 10f,
        RotationalNoiseSpeed = 20f,
        TranslationalShakeMagnitude = new Vector3(1f, 1f, 0f),
        TranslationalNoiseSpeed = 50f,
        UseSeparateNoiseForTranslation = true,
        EnableX = false,
        EnableY = true,
        EnableZ = false
    };
}