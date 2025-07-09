using UnityEngine;

public class Portal : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
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
}
