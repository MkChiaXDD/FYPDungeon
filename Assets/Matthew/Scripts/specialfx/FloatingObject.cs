using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    [Header("Floating Settings")]
    [Tooltip("Maximum height above starting position")]
    public float floatHeight = 0.5f;
    [Tooltip("Full oscillation cycles per second")]
    public float floatSpeed = 1f;

    private Vector3 startPosition;
    private float timer;

    void Start()
    {
        // Capture initial position
        startPosition = transform.position;
    }

    void Update()
    {
        // Calculate normalized time-based value using cosine for smooth oscillation
        timer += Time.deltaTime;
        float t = Mathf.Cos(timer * floatSpeed * 2 * Mathf.PI) * -0.5f + 0.5f;

        // Calculate target positions
        Vector3 targetPosition = startPosition + Vector3.up * floatHeight;

        // Smoothly interpolate between start and target positions
        transform.position = Vector3.Lerp(startPosition, targetPosition, t);
    }
}