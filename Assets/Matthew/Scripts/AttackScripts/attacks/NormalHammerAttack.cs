using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalHammerAttack : BaseAttackScript
{
    private readonly int StunStatusEffectID = 0;
    protected override void ProcessTargetHit(Collider hit, IDamageable target, int damage, PhysicalAttackType physicalType, float intensity)
    {
        base.ProcessTargetHit(hit, target, damage, physicalType, intensity);

        if (!statusEffects[StunStatusEffectID])
            return;
        ApplyStatusEffect(hit.gameObject, statusEffects[StunStatusEffectID]);
    }

    public override void ExecuteLightAttack()
    {
        //hammer presets
        Vector3 position = FindObjectOfType<PlayerMovement>().GetPosition() + transform.forward * 1.5f;
        Quaternion rotation = FindObjectOfType<PlayerMovement>().GetDirectionQuaternion() * Quaternion.Euler(-90, 0, 0);

        ParticleSystem vfx = Instantiate(lightAttackVFX, position, rotation);
        if (vfx.TryGetComponent<ParticleSystem>(out var ps)) ps.Play();
        Destroy(vfx, 2f);

        SoundManager.Instance.PlaySFX("HammerSlam");



        ApplyAttack(position, attackRadius, damageAmount, baseAttackType);

    }

    public override void ExecuteHeavyAttack(Vector3 center, float damageMultiplier, float radius)
    {
        SoundManager.Instance.PlaySFX("HammerHitSFX");
        base.ExecuteHeavyAttack(center, damageMultiplier, radius);
    }


    public override void ExecuteLightAttack(ElementType attackElement)
    {
        //hammer presets
        Vector3 position = FindObjectOfType<PlayerMovement>().GetPosition() + transform.forward * 1.5f;
        Quaternion rotation = FindObjectOfType<PlayerMovement>().GetDirectionQuaternion() * Quaternion.Euler(-90, 0, 0);

        ParticleSystem vfx = Instantiate(lightAttackVFX, position, rotation);
        if (vfx.TryGetComponent<ParticleSystem>(out var ps)) ps.Play();
        Destroy(vfx, 2f);

        SoundManager.Instance.PlaySFX("HammerSlam");



        ApplyAttack(position, attackRadius, damageAmount, attackElement);

    }

    public override void ExecuteHeavyAttack(Vector3 center, float damageMultiplier, float radius, ElementType attackElement)
    {
        SoundManager.Instance.PlaySFX("HammerHitSFX");

        base.ExecuteHeavyAttack(center, damageMultiplier, radius, attackElement);
    }

}