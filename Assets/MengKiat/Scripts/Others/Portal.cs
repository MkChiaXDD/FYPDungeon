using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private Canvas PortalInteractionCanvas;
    private void OnTriggerStay(Collider other)
    {
        if (!PortalInteractionCanvas.isActiveAndEnabled)
        {
            PortalInteractionCanvas.gameObject.SetActive(true);
        }


        if (other.CompareTag("PlayerBody"))
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
                //OnLevelEnd?.Invoke();
                //BuffSelectionUI.Select();
                //BuffSelectionUI.CreateBuffCardUI();
                FindFirstObjectByType<FarthestRoom>().NextLevel();
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {

        if (PortalInteractionCanvas.isActiveAndEnabled)
        {
            PortalInteractionCanvas.gameObject.SetActive(false);
        }
    }
}
