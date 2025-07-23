using RMG;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private Canvas PortalInteractionCanvas;
    [SerializeField] private Canvas WinCanvas;
    private bool playerHere = false;

    private void Update()
    {
        if (playerHere)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                ProceedNextLevel();
                Destroy(gameObject);
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {    
        if (other.CompareTag("PlayerBody"))
        {
            playerHere = true;

            if (!PortalInteractionCanvas.isActiveAndEnabled)
            {
                PortalInteractionCanvas.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerBody"))
        {
            playerHere = false;

            if (PortalInteractionCanvas.isActiveAndEnabled)
            {
                PortalInteractionCanvas.gameObject.SetActive(false);
            }
        }
    }

    private void ProceedNextLevel()
    {
        FindFirstObjectByType<MapGenerator>().NextLevel();
    }

    private void ProceedToWinscreen()
    {
       //if (!FindObjectOfType<EndingScript>(true))
       // {
       //     Debug.Log("Canvas with EndingScript is not found, please rememeber to make it XFZ");
       // }

       // FindObjectOfType<EndingScript>(true).ProceedToWinscreen();
    }
}
