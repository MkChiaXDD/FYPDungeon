using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalHammerAttack : BaseAttackScript
{
    // Replace magic number with safer reference
    private const int StunStatusEffectID = 0;

    private PlayerMovement _playerMovement;
    private SoundManager _soundManager;

    protected void Start()
    {
        Debug.LogWarning(FindObjectOfType<PlayerMovement>());
        Invoke(nameof(Initialise), 1);
    }

    private void Initialise()
    {
        _playerMovement = FindObjectOfType<PlayerMovement>();
        _soundManager = SoundManager.Instance;
    }

    protected override void ProcessTargetHit(Collider hit, IDamageable target, int damage, PhysicalAttackType physicalType, float intensity)
    {
        base.ProcessTargetHit(hit, target, damage, physicalType, intensity);
        ApplyStatusEffect(hit.gameObject, statusEffects[StunStatusEffectID]);
    }

    public override void ExecuteLightAttack()
    {
        // Null checks for safety
        if (_playerMovement == null)
        {
            Debug.LogError("Missing references!");
            return;
        }

        if (_soundManager == null)
        {
            Debug.LogError("Missing refsdfsdfserences!");
            return;
        }

        // Calculate position/rotation
        Vector3 position = _playerMovement.GetPosition() + transform.forward * 1.5f;
        Quaternion rotation = _playerMovement.GetDirectionQuaternion() * Quaternion.Euler(-90, 0, 0);

        // Instantiate and handle VFX
        ParticleSystem vfx = Instantiate(lightAttackVFX, position, rotation);
        vfx.Play();

        // Smart destruction using particle duration
        float destroyDelay = vfx.main.duration + vfx.main.startLifetime.constantMax;
        Destroy(vfx.gameObject, destroyDelay);

        // Play sound
        _soundManager.PlaySFX("HammerSlam", gameObject);

        ApplyAttack(position, attackRadius, damageAmount, baseAttackType);
    }
}