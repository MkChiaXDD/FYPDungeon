using UnityEngine;

public class AnimatedPickupItem : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 30f; // degrees per second
    [SerializeField] private Vector3 rotationAxis = Vector3.up; // direction to rotate

    [Header("Bobbing Settings")]
    [SerializeField] private float bobHeight = 0.5f; // how high the bob goes
    [SerializeField] private float bobSpeed = 1f; // how fast the bob is
    [SerializeField] private float bobOffset = 0f; // put offset for multiple items
    [SerializeField] private float minVelocityForBobbing = 0.1f; // when start bob

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

        if (rb != null && rb.velocity.magnitude < minVelocityForBobbing)
        {
            BobbingMotion();
        }
    }

    private void Rotation()
    {
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }

    private void BobbingMotion()
    {
        // Calculate bobbing motion
        timer += Time.deltaTime * bobSpeed;
        float offsetY = Mathf.Sin(timer) * bobHeight;

         Vector3 pos = transform.position;
        pos.y = baseY + offsetY;
        transform.position = pos;
    }
}
