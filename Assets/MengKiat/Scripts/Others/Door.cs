using System.Collections;
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

    [Header("Smooth Transition")]
    [SerializeField] private float transitionDuration = 1f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private Coroutine doorCoroutine;

    private void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.localRotation;
        ToggleDoor(isActive);
    }

    public void ToggleDoor(bool isOpen)
    {
        if (doorCoroutine != null)
            StopCoroutine(doorCoroutine);

        doorCoroutine = StartCoroutine(AnimateDoor(isOpen));
    }

    private IEnumerator AnimateDoor(bool isOpen)
    {
        float elapsedTime = 0f;

        if (isSwingingDoor)
        {
            Vector3 startRotation = transform.localEulerAngles;
            Vector3 endRotation = isOpen ? openRotation : closedRotation;

            while (elapsedTime < transitionDuration)
            {
                float t = elapsedTime / transitionDuration;
                Vector3 currentRotation = Vector3.Lerp(startRotation, endRotation, t);
                transform.localEulerAngles = currentRotation;
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.localEulerAngles = endRotation;
        }
        else
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = originalPosition + (isOpen ? new Vector3(0f, liftAmount, 0f) : Vector3.zero);

            while (elapsedTime < transitionDuration)
            {
                float t = elapsedTime / transitionDuration;
                transform.position = Vector3.Lerp(startPos, endPos, t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = endPos;
        }

        doorCoroutine = null;
    }
}
