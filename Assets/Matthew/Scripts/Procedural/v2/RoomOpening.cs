using System.Collections;
using UnityEngine;

public class RoomOpening : MonoBehaviour
{
    public bool IsConnected { get; private set; }
    public bool IsProcessing { get; private set; }

    private DungeonRoom parentRoom;
    private DungeonGenerator dungeonGenerator;
    

    public void Initialize(DungeonRoom room)
    {
        parentRoom = room;
        dungeonGenerator = FindFirstObjectByType<DungeonGenerator>();
    }

    public IEnumerator TrySpawnChamber()
    {
        IsProcessing = true;
        GameObject[] pieces = {
            dungeonGenerator.GetRandomRoomPrefab(),
            dungeonGenerator.wallPrefab
        };

        foreach (GameObject piece in pieces)
        {
            // Spawn chamber at opening position/orientation
            GameObject newChamber = Instantiate(
                piece,
                new Vector3(transform.position.x,0,transform.position.z),
                transform.rotation
            );

            

            // Check validity
            DungeonRoom newRoom = newChamber.GetComponent<DungeonRoom>();
            RoomValidator validator = newChamber.GetComponent<RoomValidator>();

            if (validator && validator.IsValid)
            {
                // Valid connection
                newRoom.Initialize(dungeonGenerator);
                parentRoom.NotifyChamberSpawned(newRoom);
                IsConnected = true;
                IsProcessing = false;
                yield break;
            }

            // Step 3: Wait for validation
            yield return new WaitForSeconds(dungeonGenerator.validationDelay);

            // Invalid - destroy and try next
            Destroy(newChamber);
        }

        // All pieces failed - mark as closed
        parentRoom.NotifyChamberFailed();
        IsProcessing = false;
    }
}