using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour
{

    [SerializeField] private float DestroyCountdownDuration = 5.0f;
    [SerializeField] private ParticleSystem EarthShatterVFX;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DestroySelf());
        EarthShatterVFX.Play();
    }
    private IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(DestroyCountdownDuration);
        Destroy(gameObject);
    }
}
