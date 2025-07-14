using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] private Door door;
    private bool isOn = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOn = true;
            ToggleDoor();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOn = false;
            ToggleDoor();
        }
    }

    private void ToggleDoor()
    {
        door.ToggleDoor(isOn);
    }
}
