using System.Collections;
using UnityEngine;

public class RoomValidator : MonoBehaviour
{
    public bool IsValid { get; private set; } = true;
    public float checkRadius = 5f;
    public LayerMask roomLayer;

    void Start()
    {
        ValidatePosition();
    }

    void ValidatePosition()
    {
        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            checkRadius,
            roomLayer
        );

        foreach (Collider col in colliders)
        {
            if (col.gameObject != gameObject && col.GetComponent<RoomValidator>())
            {
                IsValid = false;
                StartCoroutine(NotifyInvalid());
                return;
            }
        }
    }

    IEnumerator NotifyInvalid()
    {
        Debug.Log("validation failed fuck u");
        // Delay to ensure destruction completes
        yield return new WaitForEndOfFrame();
        Destroy(gameObject);

        
    }
}