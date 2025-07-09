using System.Collections;
using UnityEngine;

public class EnemyShield : MonoBehaviour
{
    private float maxHp;
    private float currentHp;
    [SerializeField] private float stunDuration;
    [SerializeField] private ParticleSystem breakParticle;
    [SerializeField] private MeshRenderer mesh;

    public void Init(float maxHp)
    {
        this.maxHp = maxHp;
        currentHp = this.maxHp;
    }

    public void HitShield(float damage, PhysicalAttackType physicalAttackType)
    {
        if (physicalAttackType != PhysicalAttackType.Blunt)
            currentHp -= damage;
        else currentHp = 0;

        if (currentHp <= 0)
        {
            mesh.enabled = false;
            if (breakParticle != null && !breakParticle.isPlaying)
            {
                breakParticle.Play();
                float timer = breakParticle.main.duration;
                StartCoroutine(SetInactive(timer));
                var nig = GetComponentInParent<Enemy>();
                nig.ApplyStun(stunDuration);
            }
        }
    }


    public float GetShieldHp()
    {
        return currentHp;
    }

    private IEnumerator SetInactive(float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }
}
