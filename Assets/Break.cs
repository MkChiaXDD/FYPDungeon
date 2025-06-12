using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Break : MonoBehaviour
{
    [SerializeField] private GameObject brokenObject;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(nameof(BreakObject));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Q"))
        {
            StartCoroutine(nameof(BreakObject));
        }
    }

    private IEnumerator BreakObject()
    {

        yield return Instantiate(brokenObject, transform.position, Quaternion.Euler(0, 0, 0));
        Destroy(gameObject);
    }
}
