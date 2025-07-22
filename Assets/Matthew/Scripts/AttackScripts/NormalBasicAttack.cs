using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBasicAttack : BaseAttackScript
{
    protected override void ProcessTargetHit(Collider hit, IDamageable target, int damage, PhysicalAttackType physicalType, float intensity)
    {
        base.ProcessTargetHit(hit, target, damage, physicalType, intensity);  
    }

    public override void ExecuteLightAttack()
    {
        //base presets
        Vector3 position = FindObjectOfType<PlayerMovement>().GetPosition() + transform.forward * 1.5f;
        Quaternion rotation = FindObjectOfType<PlayerMovement>().GetDirectionQuaternion() * Quaternion.Euler(-90, 0, 0);

        ParticleSystem vfx = Instantiate(lightAttackVFX, position, rotation);
        if (vfx.TryGetComponent<ParticleSystem>(out var ps)) ps.Play();
        Destroy(vfx, 2f);

        SoundManager.Instance.PlaySFX("BasicAttack", this.gameObject);

        ApplyAttack(position, attackRadius, damageAmount, baseAttackType);
    }

    
   


}
