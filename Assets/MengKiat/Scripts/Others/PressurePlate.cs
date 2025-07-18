using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [Header("Insert Door")]
    [SerializeField] private List<Door> doors;
    private bool isOn = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOn = !isOn;
            ToggleDoor();
        }
    }

    private void ToggleDoor()
    {
        foreach (Door door in doors)
        {
            door.ToggleDoor(isOn);
        }
    }
}
