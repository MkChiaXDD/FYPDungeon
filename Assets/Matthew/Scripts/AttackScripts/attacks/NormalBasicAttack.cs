using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBasicAttack : BaseAttackScript
{

    private int SlowStatusEffectID = 0;
    protected override void ProcessTargetHit(Collider hit, IDamageable target, int damage, PhysicalAttackType physicalType, float intensity)
    {
        base.ProcessTargetHit(hit, target, damage, physicalType, intensity);
       // ApplyStatusEffect(hit.gameObject, statusEffects[SlowStatusEffectID]);
    }

    public override void ExecuteLightAttack()
    {
        //base presets
        Vector3 position = FindObjectOfType<PlayerMovement>().GetPosition() + transform.forward * 4f + transform.up * 2;
        Quaternion rotation = FindObjectOfType<PlayerMovement>().GetDirectionQuaternion() * Quaternion.Euler(-90, 0, 0);

        ParticleSystem vfx = Instantiate(lightAttackVFX, position, rotation);
        if (vfx.TryGetComponent<ParticleSystem>(out var ps)) ps.Play();
        Destroy(vfx, 2f);

        SoundManager.Instance.PlaySFX("BasicAttack");

        Debug.Log("Basic attack: " + baseAttackType);

        ApplyAttack(position, attackRadius, damageAmount, baseAttackType);
    }

    public override void ExecuteLightAttack(ElementType attackElement)
    {
        //base presets
        Vector3 position = FindObjectOfType<PlayerMovement>().GetPosition() + transform.forward * 4f + transform.up * 2;
        Quaternion rotation = FindObjectOfType<PlayerMovement>().GetDirectionQuaternion() * Quaternion.Euler(-90, 0, 0);

        ParticleSystem vfx = Instantiate(lightAttackVFX, position, rotation);
        if (vfx.TryGetComponent<ParticleSystem>(out var ps)) ps.Play();
        Destroy(vfx, 2f);

        SoundManager.Instance.PlaySFX("BasicAttack");

        Debug.Log("Basic attack: " + baseAttackType);

        ApplyAttack(position, attackRadius, damageAmount, attackElement);
    }
}
