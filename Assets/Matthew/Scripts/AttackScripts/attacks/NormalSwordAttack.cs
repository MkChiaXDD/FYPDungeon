using Unity.VisualScripting;
using UnityEngine;

public class NormalSwordAttack : BaseAttackScript
{
    protected override void ProcessTargetHit(Collider hit, IDamageable target, int damage, PhysicalAttackType physicalType, float intensity)
    {
        base.ProcessTargetHit(hit, target, damage, physicalType, intensity);
    }

    public override void ExecuteLightAttack()
    {
        //hammer presets
        Vector3 position = FindObjectOfType<PlayerMovement>().GetPosition() + transform.forward * 1.5f + transform.up * 3;
        Quaternion rotation = FindObjectOfType<PlayerMovement>().GetDirectionQuaternion() * Quaternion.Euler(-90, 0, 0);

        ParticleSystem vfx = Instantiate(lightAttackVFX, position, rotation);
        if (vfx.TryGetComponent<ParticleSystem>(out var ps)) ps.Play();
        Destroy(vfx, 2f);

        SoundManager.Instance.PlaySFX("SwordSlash", this.gameObject);

        ApplyAttack(position, attackRadius, damageAmount, baseAttackType);
    }

    public override void ExecuteLightAttack(ElementType attackElement)
    {
        //hammer presets
        Vector3 position = FindObjectOfType<PlayerMovement>().GetPosition() + transform.forward * 1.5f + transform.up * 3;
        Quaternion rotation = FindObjectOfType<PlayerMovement>().GetDirectionQuaternion() * Quaternion.Euler(-90, 0, 0);

        ParticleSystem vfx = Instantiate(lightAttackVFX, position, rotation);
        if (vfx.TryGetComponent<ParticleSystem>(out var ps)) ps.Play();
        Destroy(vfx, 2f);

        SoundManager.Instance.PlaySFX("SwordSlash", this.gameObject);

        ApplyAttack(position, attackRadius, damageAmount, attackElement);
    }
}
