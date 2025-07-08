using System.Collections;
using UnityEngine;

public class Destroy : MonoBehaviour
{
    [SerializeField] private float DestroyCountdownDuration = 5.0f;
    // Start is called before the first frame update
    void Start()
    { 
        StartCoroutine(DestroySelf());      
    }
    private IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(DestroyCountdownDuration);
        Destroy(gameObject);
    }
}
