using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private Canvas PortalInteractionCanvas;
    [SerializeField] private Canvas WinCanvas;

    private bool hasWon = true;
    private void OnTriggerStay(Collider other)
    {    
        if (!other.CompareTag("PlayerBody"))
        {
            return; //not player go away
        }

        if (!PortalInteractionCanvas.isActiveAndEnabled)
        {
            PortalInteractionCanvas.gameObject.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            //OnLevelEnd?.Invoke();
            //BuffSelectionUI.Select();
            //BuffSelectionUI.CreateBuffCardUI();
            if (!hasWon)
            {
                ProceedNextLevel();
            }
            else
                ProceedToWinscreen();


                Destroy(gameObject);
        }

    }

    private void OnTriggerExit(Collider other)
    {

        if (PortalInteractionCanvas.isActiveAndEnabled)
        {
            PortalInteractionCanvas.gameObject.SetActive(false);
        }
    }

    private void ProceedNextLevel()
    {
        FindFirstObjectByType<FarthestRoom>().NextLevel();
    }

    private void ProceedToWinscreen()
    {
       if (!FindObjectOfType<EndingScript>(true))
        {
            Debug.Log("Canvas with EndingScript is not found, please rememeber to make it XFZ");
        }

        FindObjectOfType<EndingScript>(true).ProceedToWinscreen();
    }
}
