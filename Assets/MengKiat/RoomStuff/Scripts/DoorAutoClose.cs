using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAutoClose : MonoBehaviour
{
    [SerializeField] private List<Door> doors;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("PlayerBody"))
        {
            foreach (Door door in doors)
            {
                door.ToggleDoor(false);
                hasTriggered = true;
            }
        }
    }
}
