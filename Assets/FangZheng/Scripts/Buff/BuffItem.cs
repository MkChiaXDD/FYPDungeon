using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffItem : MonoBehaviour
{

    [SerializeField] private GameObject CanCollectUI;
    private bool PlayerIsInBound;


    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is the player (you might want to add more specific checks)
        if (other.CompareTag("Player"))
        {
            PlayerIsInBound = true;
            CanCollectUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerIsInBound = false;
            CanCollectUI.SetActive(false);
        }
    }

    private void CollectBuff()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {

        }
    }
}
