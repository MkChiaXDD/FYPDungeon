using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticalManager : MonoBehaviour
{
    public static ParticalManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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
                Destroy(vfx, 2f);
            }
        }
    }

}
