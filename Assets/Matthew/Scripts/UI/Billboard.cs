using UnityEngine;

public class Billboard : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool useStaticBillboard = true;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found! Billboard disabled.", this);
            //enabled = false;
        }
    }

    void LateUpdate()
    {
        if (useStaticBillboard)
        {
            // Static billboard (faces camera but ignores rotation)
            transform.forward = mainCamera.transform.forward;
        }
        else
        {
            // Dynamic billboard (always face camera position)
            Vector3 lookDirection = transform.position - mainCamera.transform.position;
            transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        }
    }
}