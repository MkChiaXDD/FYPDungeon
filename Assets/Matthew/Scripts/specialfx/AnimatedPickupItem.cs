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
    [SerializeField] private float minVelocityForBobbing = 0.1f; // When to start bobbing

    private Rigidbody rb;
    private float baseY;
    private float timer;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        baseY = transform.position.y;
        timer = bobOffset;
    }

    private void Update()
    {
        Rotation();

        // Start bobbing only when object is almost still
        if (rb != null && rb.velocity.magnitude < minVelocityForBobbing)
        {
            BobbingMotion();
        }
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
        float offsetY = Mathf.Sin(timer) * bobHeight;

        // Apply bobbing only to Y position
        Vector3 pos = transform.position;
        pos.y = baseY + offsetY;
        transform.position = pos;
    }
}
