using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    public bool isSwingingDoor = true;
    public bool isActive = false;

    [Header("Swinging Door Rotation")]
    [SerializeField] private Vector3 closedRotation = Vector3.zero;
    [SerializeField] private Vector3 openRotation = new Vector3(0f, -90f, 0f);

    [Header("Sliding Door Settings")]
    [SerializeField] private float liftAmount = 2f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.localRotation;
    }

    public void ToggleDoor(bool isOpen)
    {
        if (isSwingingDoor)
        {
            transform.position = originalPosition;

            Vector3 targetRotation = isOpen ? openRotation : closedRotation;
            transform.localRotation = Quaternion.Euler(targetRotation);
        }
        else
        {
            transform.localRotation = Quaternion.Euler(closedRotation); // Or originalRotation if needed

            float yOffset = isOpen ? liftAmount : 0f;
            transform.position = originalPosition + new Vector3(0f, yOffset, 0f);
        }
    }
}
