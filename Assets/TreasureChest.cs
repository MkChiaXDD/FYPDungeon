using UnityEngine;

public class SpringChest : MonoBehaviour
{
    [Header("Chest Parts")]
    [SerializeField] private Transform chestTop;    // Assign the top part in Inspector
    [SerializeField] private Transform chestBase;   // Assign the bottom part in Inspector

    [Header("Spring Settings")]
    [SerializeField] private float openAngle = -90f;
    [SerializeField] private float springStrength = 100f;
    [SerializeField] private float damping = 10f;
    [SerializeField] private KeyCode interactKey = KeyCode.Space;

    [Header("Interaction")]
    [SerializeField] private float interactionRadius = 1f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Drop System")]
    [SerializeField] private ItemDropSystem dropSystem;

    private float currentVelocity;
    private float currentAngle;
    private bool isOpen = false;

    private void Update()
    {


        if (CheckPlayerProximity() && Input.GetKeyDown(interactKey) /*&& !isOpen*/)
        {
            isOpen = !isOpen;
            dropSystem.SpawnDropItem();
        }

        ApplySpringMotion();
    }

    private bool CheckPlayerProximity()
    {

        return Physics.CheckSphere(chestBase.position, interactionRadius, playerLayer);
    }

    private void ApplySpringMotion()
    {
        float targetAngle = isOpen ? openAngle : 0f;

        // Spring physics calculation
        float displacement = targetAngle - currentAngle;
        float acceleration = (displacement * springStrength) - (currentVelocity * damping);

        currentVelocity += acceleration * Time.deltaTime;
        currentAngle += currentVelocity * Time.deltaTime;

        // Apply rotation to top part (X-axis only)
        chestTop.localRotation = Quaternion.Euler(currentAngle, 0f, 0f);
    }

    // Visualize interaction radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(chestBase.position, interactionRadius);
    }
}