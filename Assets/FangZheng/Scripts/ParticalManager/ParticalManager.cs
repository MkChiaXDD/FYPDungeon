using UnityEngine;

public class ParticalManager : MonoBehaviour
{
    public GameObject BleedPartical;
    public static ParticalManager Instance { get; private set; }

    public void Bleed(Transform pos)
    {
        GameObject bleed =  Instantiate(BleedPartical, pos.position, Quaternion.identity);
        Destroy(bleed , 1f);
    }

    public void PlayVFX(GameObject vfx , Transform Pos)
    {
        if (vfx != null)
        {
            Instantiate(vfx, Pos.position, Quaternion.identity);
            ParticleSystem particleSystem = vfx.GetComponent<ParticleSystem>();

            if (particleSystem != null)
            {
                float totalDuration = particleSystem.main.duration + particleSystem.main.startLifetime.constantMax;
                Destroy(vfx, totalDuration);
            }
            else
            {
                Destroy(vfx, 1f);
            }
        }
    }

}
