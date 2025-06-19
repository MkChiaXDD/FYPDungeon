using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerators : MonoBehaviour
{
    public List<GameObject> RoomPrefabs;
    public int maxRooms = 10;
    public float roomSize = 5f;
    public LayerMask roomLayerMask;

    private List<Transform> availableExits = new List<Transform>();
    private List<GameObject> spawnedRooms = new List<GameObject>();

    private void Start()
    {
        GenerateDungeon();
    }

    [ContextMenu("Regenerate Dungeon")]
    public void GenerateDungeon()
    {
        // Clear old dungeon
        foreach (GameObject room in spawnedRooms)
        {
            if (room != null)
                DestroyImmediate(room);
        }
        spawnedRooms.Clear();
        availableExits.Clear();

        // Spawn first room
        GameObject startRoom = Instantiate(RoomPrefabs[Random.Range(0, RoomPrefabs.Count)], Vector3.zero, Quaternion.identity, transform);
        Rooms startRoomScript = startRoom.GetComponent<Rooms>();
        spawnedRooms.Add(startRoom);
        availableExits.AddRange(startRoomScript.exitPoints);

        int attempts = 0;
        int placedRooms = 1;

        while (placedRooms < maxRooms && attempts < 100)
        {
            if (availableExits.Count == 0) break;

            Transform exitToConnect = availableExits[0];
            availableExits.RemoveAt(0);

            GameObject candidatePrefab = RoomPrefabs[Random.Range(0, RoomPrefabs.Count)];
            Rooms candidateScript = candidatePrefab.GetComponent<Rooms>();

            List<Transform> shuffledEntrances = new List<Transform>(candidateScript.exitPoints);
            ShuffleList(shuffledEntrances);

            bool placed = false;

            foreach (Transform entrance in shuffledEntrances)
            {
                // Get world offset between entrance and prefab origin
                Vector3 localOffset = entrance.localPosition;
                Vector3 spawnPosition = exitToConnect.position - localOffset;

                // Check overlap before instantiation
                if (!CheckOverlap(spawnPosition, Quaternion.identity, roomSize))
                {
                    GameObject newRoom = Instantiate(candidatePrefab, spawnPosition, Quaternion.identity, transform);
                    spawnedRooms.Add(newRoom);

                    // Add all exits except the one we connected
                    Rooms newRoomScript = newRoom.GetComponent<Rooms>();
                    foreach (Transform newExit in newRoomScript.exitPoints)
                    {
                        if (Vector3.Distance(newExit.position, exitToConnect.position) > 0.1f)
                        {
                            availableExits.Add(newExit);
                        }
                    }

                    placedRooms++;
                    placed = true;
                    break;
                }
            }

            if (!placed)
            {
                attempts++;
            }
        }
    }

    private bool CheckOverlap(Vector3 position, Quaternion rotation, float size)
    {
        Collider[] colliders = Physics.OverlapBox(
            position,
            new Vector3(size / 2f, 1f, size / 2f),
            rotation,
            roomLayerMask
        );

        return colliders.Length > 0;
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
