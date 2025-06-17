using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonRoom : MonoBehaviour
{
    [HideInInspector] public List<DungeonRoom> ConnectedRooms = new List<DungeonRoom>();
    public List<RoomOpening> openings = new List<RoomOpening>();

    private DungeonGenerator dungeonGenerator;
    private int openingsToProcess;
    private bool isSpawning;

    public bool IsSpawningComplete => !isSpawning && openingsToProcess == 0;

    public void Initialize(DungeonGenerator generator)
    {
        dungeonGenerator = generator;
        openings = new List<RoomOpening>(GetComponentsInChildren<RoomOpening>());
        foreach (RoomOpening opening in openings)
        {
            opening.Initialize(this);
        }
    }

    public bool HasUnconnectedOpenings()
    {
        return openings.Exists(o => !o.IsConnected && !o.IsProcessing);
    }

    public IEnumerator SpawnAdjacentChambers()
    {
        isSpawning = true;
        openingsToProcess = 0;

        foreach (RoomOpening opening in openings)
        {
            if (!opening.IsConnected && !opening.IsProcessing)
            {
                openingsToProcess++;
                StartCoroutine(opening.TrySpawnChamber());
            }
        }

        yield return new WaitUntil(() => openingsToProcess == 0);
        isSpawning = false;
    }

    public void NotifyChamberSpawned(DungeonRoom newRoom)
    {
        ConnectedRooms.Add(newRoom);
        openingsToProcess--;
    }

    public void NotifyChamberFailed()
    {
        openingsToProcess--;
    }
}