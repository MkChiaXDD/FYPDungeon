using UnityEngine;

public class AnimatedPickupItem : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 30f; // Degrees per second
    [SerializeField] private Vector3 rotationAxis = Vector3.up; // Default to Y-axis rotation

    [Header("Bobbing Settings")]
    [SerializeField] private float bobHeight = 0.5f; // How high the bob goes
    [SerializeField] private float bobSpeed = 1f; // How fast the bob is
    [SerializeField] private float bobOffset = 0f; // Phase offset for multiple items

    private Vector3 startPosition;
    private float timer;

    private void Start()
    {
        startPosition = transform.position;
        timer = bobOffset;
    }

    private void Update()
    {
        Rotation();

        BobbingMotion();
    }   

    private void Rotation()
    {
        // Rotate the object
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }

    private void BobbingMotion()
    {
        // Calculate bobbing motion
        timer += Time.deltaTime * bobSpeed;
        float newY = startPosition.y + Mathf.Sin(timer) * bobHeight;

        // Apply position with bobbing
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
}