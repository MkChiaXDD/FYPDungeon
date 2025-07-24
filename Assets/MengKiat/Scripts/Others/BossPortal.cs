using UnityEngine;

public class BossPortal : MonoBehaviour
{
    public bool playerInRange = false;

    private void Update()
    {
        if (playerInRange)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("Player Teleport");
                GameObject playerObject = GameObject.FindWithTag("Player");
                GameObject bossRoomSpawn = GameObject.Find("PlayerSpawn");
                if (playerObject != null && bossRoomSpawn != null)
                {
                    playerObject.transform.position = new Vector3(bossRoomSpawn.transform.position.x, playerObject.transform.position.y, bossRoomSpawn.transform.position.z);
                    Destroy(gameObject);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerBody"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerBody"))
        {
            playerInRange = false;
        }
    }
}
