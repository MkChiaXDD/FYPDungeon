using UnityEngine;

public class Door : MonoBehaviour
{
    public bool isSwingingDoor = true;
    public bool isActive = false;
    private Vector3 originalPos;

    public float lift = 2f;

    private void Start()
    {
        originalPos = transform.position;
    }

    public void ToggleDoor(bool isOpen)
    {
        if (isSwingingDoor)
        {
            transform.position = new Vector3(originalPos.x, originalPos.y, originalPos.z);
            if (isOpen)
            {
                transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            }
            else
            {
                transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }
        else
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

            if (isOpen)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y + lift, transform.position.z);
            }
            else
            {
                transform.position = new Vector3(transform.position.x, transform.position.y - lift, transform.position.z);
            }
        }
    }
}
