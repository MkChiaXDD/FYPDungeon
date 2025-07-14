using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalHammerAttack : BaseAttackScript
{
    
    protected override void ProcessTargetHit(Collider hit, IDamageable target, int damage, PhysicalAttackType physicalType, float intensity)
    {
        base.ProcessTargetHit(hit, target, damage, physicalType, intensity);
        ApplyStatusEffect(hit.gameObject, statusEffects[0]);
    }

    public override void ExecuteLightAttack()
    {
        //hammer presets
        Vector3 position = FindObjectOfType<PlayerMovement>().GetPosition() + transform.forward * 1.5f;
        Quaternion rotation = FindObjectOfType<PlayerMovement>().GetDirectionQuaternion() * Quaternion.Euler(-90, 0, 0);

        ParticleSystem vfx = Instantiate(lightAttackVFX, position, rotation);
        if (vfx.TryGetComponent<ParticleSystem>(out var ps)) ps.Play();
        Destroy(vfx, 2f);

        ApplyAttack(position, attackRadius, damageAmount, baseAttackType);
    }

    
}
