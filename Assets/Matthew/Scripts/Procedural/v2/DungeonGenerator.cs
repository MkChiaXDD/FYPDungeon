using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DungeonGenerator : MonoBehaviour
{
    public GameObject startingRoomPrefab;
    public List<GameObject> roomPrefabs;
    public GameObject wallPrefab;
    public float validationDelay = 0.1f;

    private Queue<DungeonRoom> roomsToProcess = new Queue<DungeonRoom>();
    private List<DungeonRoom> allRooms = new List<DungeonRoom>();

    void Start()
    {
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        // Step 1: Spawn initial room
        GameObject startRoom = Instantiate(startingRoomPrefab, Vector3.zero, Quaternion.identity);
        DungeonRoom startDungeonRoom = startRoom.GetComponent<DungeonRoom>();
        startDungeonRoom.Initialize(this);
        allRooms.Add(startDungeonRoom);
        roomsToProcess.Enqueue(startDungeonRoom);

        StartCoroutine(ProcessRooms());
    }

    IEnumerator ProcessRooms()
    {
        while (roomsToProcess.Count > 0)
        {
            // Step 2: Get next room with unconnected openings
            DungeonRoom currentRoom = roomsToProcess.Dequeue();

            if (currentRoom.HasUnconnectedOpenings())
            {
                // Step 3: Spawn adjacent chambers
                yield return StartCoroutine(currentRoom.SpawnAdjacentChambers());

                // Step 4: Wait for room completion
                yield return new WaitUntil(() => currentRoom.IsSpawningComplete);

                // Step 5: Add new rooms to processing queue
                foreach (DungeonRoom newRoom in currentRoom.ConnectedRooms)
                {
                    if (!allRooms.Contains(newRoom))
                    {
                        allRooms.Add(newRoom);
                        roomsToProcess.Enqueue(newRoom);
                    }
                }
            }
        }
        Debug.Log("Dungeon Generation Complete!");
    }

    public void RegenerateDungeon()
    {
        GenerateDungeon();
    }

    public GameObject GetRandomRoomPrefab()
    {
        return roomPrefabs[Random.Range(0, roomPrefabs.Count)];
        //return roomPrefabs[1];
    }


}

#if UNITY_EDITOR
[CustomEditor(typeof(DungeonGenerator))]
public class DungeonGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DungeonGenerator generator = (DungeonGenerator)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Generate Dungeon", GUILayout.Height(30)))
        {
            generator.RegenerateDungeon();
        }
    }
}
#endif

