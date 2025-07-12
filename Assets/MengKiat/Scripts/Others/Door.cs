using UnityEngine;

public class Door : MonoBehaviour
{
    public bool isActive = false;
    
    public void ToggleDoor(bool isOpen)
    {
        if (isOpen)
        {
            transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
        }
        else
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }
}
