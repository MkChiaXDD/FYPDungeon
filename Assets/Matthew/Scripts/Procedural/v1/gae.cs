using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum RoomType
{
    Start,
    Middle,
    Boss
}

public class Room
{
    public Vector2Int GridPosition { get; set; }
    public RoomType Type { get; set; }
    public bool[] Doors { get; set; } // [North, East, South, West]
    public GameObject PrefabInstance { get; set; }
    public List<Room> ConnectedRooms { get; } = new List<Room>();
}

public class gae : MonoBehaviour
{
    [Header("Generation Settings")]
    [SerializeField] private int _minRooms = 6;
    [SerializeField] private int _maxRooms = 10;
    [SerializeField] private int _maxAttempts = 100;

    [Header("Room Prefabs")]
    [SerializeField] private GameObject _startRoomPrefab;
    [SerializeField] private List<GameObject> _middleRoomPrefabs;
    [SerializeField] private GameObject _bossRoomPrefab;

    [Header("Layout Settings")]
    [SerializeField] private float _roomSpacing = 20f;

    private List<Room> _rooms = new List<Room>();
    private Vector2Int[] _directions = {
        Vector2Int.up,    // North
        Vector2Int.right, // East
        Vector2Int.down,  // South
        Vector2Int.left   // West
    };

    void Start()
    {
        GenerateDungeon();
    }

    public void GenerateDungeon()
    {
        ClearExistingDungeon();
        GenerateRoomLayout();
        EnsurePathToBoss();
        EnsureAllRoomsReachable(); // New connectivity check
        SetRoomDoors();
        InstantiateRooms();
        SpawnPlayer();

        Debug.Log($"Generated dungeon with {_rooms.Count} rooms");
    }



    private void ClearExistingDungeon()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        _rooms.Clear();
    }

    private void GenerateRoomLayout()
    {
        int roomCount = Random.Range(_minRooms, _maxRooms + 1);
        Vector2Int currentPosition = Vector2Int.zero;
        int attempts = 0;

        // Create start room
        Room startRoom = new Room
        {
            GridPosition = currentPosition,
            Type = RoomType.Start,
            Doors = new bool[4]
        };
        _rooms.Add(startRoom);

        // Generate path
        for (int i = 1; i < roomCount; i++)
        {
            Vector2Int nextDirection = GetValidDirection(currentPosition, ref attempts);
            currentPosition += nextDirection;

            RoomType roomType = (i == roomCount - 1) ? RoomType.Boss : RoomType.Middle;

            Room newRoom = new Room
            {
                GridPosition = currentPosition,
                Type = roomType,
                Doors = new bool[4]
            };
            _rooms.Add(newRoom);

            // Connect rooms bi-directionally
            Room previousRoom = _rooms[_rooms.Count - 2];
            previousRoom.ConnectedRooms.Add(newRoom);
            newRoom.ConnectedRooms.Add(previousRoom);
        }
    }

    private Vector2Int GetValidDirection(Vector2Int currentPosition, ref int attempts)
    {
        Vector2Int direction;
        Vector2Int nextPosition;
        bool positionValid;

        do
        {
            direction = _directions[Random.Range(0, _directions.Length)];
            nextPosition = currentPosition + direction;
            positionValid = !_rooms.Any(r => r.GridPosition == nextPosition);
            attempts++;
        } while (!positionValid && attempts < _maxAttempts);

        if (attempts >= _maxAttempts)
        {
            // Fallback to any valid direction
            foreach (var dir in _directions)
            {
                nextPosition = currentPosition + dir;
                if (!_rooms.Any(r => r.GridPosition == nextPosition))
                    return dir;
            }
            // If no valid direction, return up as default
            return Vector2Int.up;
        }

        return direction;
    }

    private void EnsurePathToBoss()
    {
        Room startRoom = _rooms.First(r => r.Type == RoomType.Start);
        Room bossRoom = _rooms.FirstOrDefault(r => r.Type == RoomType.Boss);

        if (bossRoom == null)
        {
            Debug.LogError("Boss room not found!");
            return;
        }

        // Check connectivity
        if (!IsConnected(startRoom, bossRoom, new HashSet<Room>()))
        {
            Debug.LogWarning("Boss room not connected! Forcing connection...");
            ForceConnection(startRoom, bossRoom);
        }
    }

    // NEW: Ensure all rooms are reachable from start
    private void EnsureAllRoomsReachable()
    {
        Room startRoom = _rooms.First(r => r.Type == RoomType.Start);
        HashSet<Room> reachableRooms = FindReachableRooms(startRoom);

        // Identify unreachable rooms
        List<Room> unreachableRooms = _rooms.Where(r => !reachableRooms.Contains(r)).ToList();

        foreach (Room unreachableRoom in unreachableRooms)
        {
            Debug.LogWarning($"Room at {unreachableRoom.GridPosition} is unreachable! Connecting to main path...");
            ConnectToNearestReachable(unreachableRoom, reachableRooms);
        }
    }

    private HashSet<Room> FindReachableRooms(Room start)
    {
        HashSet<Room> visited = new HashSet<Room>();
        Queue<Room> queue = new Queue<Room>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Room current = queue.Dequeue();
            foreach (Room neighbor in current.ConnectedRooms)
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return visited;
    }

    private void ConnectToNearestReachable(Room target, HashSet<Room> reachableRooms)
    {
        Room nearestRoom = null;
        float minDistance = float.MaxValue;

        // Find closest reachable room
        foreach (Room candidate in reachableRooms)
        {
            float distance = Vector2Int.Distance(target.GridPosition, candidate.GridPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestRoom = candidate;
            }
        }

        if (nearestRoom != null)
        {
            // Create direct connection
            target.ConnectedRooms.Add(nearestRoom);
            nearestRoom.ConnectedRooms.Add(target);
            Debug.Log($"Connected room {target.GridPosition} to {nearestRoom.GridPosition}");
        }
        else
        {
            Debug.LogError("No reachable room found to connect to!");
        }
    }

    private bool IsConnected(Room current, Room target, HashSet<Room> visited)
    {
        if (current == target) return true;
        if (visited.Contains(current)) return false;

        visited.Add(current);

        foreach (Room neighbor in current.ConnectedRooms)
        {
            if (IsConnected(neighbor, target, visited))
                return true;
        }

        return false;
    }

    private void ForceConnection(Room start, Room end)
    {
        Vector2Int currentPos = start.GridPosition;
        List<Vector2Int> path = new List<Vector2Int>();

        // Create direct path between start and end
        while (currentPos != end.GridPosition)
        {
            Vector2Int direction = GetDirectionTowards(currentPos, end.GridPosition);
            currentPos += direction;
            path.Add(currentPos);
        }

        // Create rooms along the path
        foreach (Vector2Int pos in path)
        {
            if (!_rooms.Any(r => r.GridPosition == pos))
            {
                Room newRoom = new Room
                {
                    GridPosition = pos,
                    Type = RoomType.Middle,
                    Doors = new bool[4]
                };
                _rooms.Add(newRoom);
            }
        }

        // Connect all rooms along the path
        ConnectPath(_rooms.OrderBy(r => r.GridPosition).ToList());
    }

    private Vector2Int GetDirectionTowards(Vector2Int from, Vector2Int to)
    {
        Vector2Int difference = to - from;
        Vector2Int direction = Vector2Int.zero;

        if (difference.x != 0)
            direction.x = difference.x > 0 ? 1 : -1;
        else if (difference.y != 0)
            direction.y = difference.y > 0 ? 1 : -1;

        return direction;
    }

    private void ConnectPath(List<Room> pathRooms)
    {
        for (int i = 0; i < pathRooms.Count - 1; i++)
        {
            Room current = pathRooms[i];
            Room next = pathRooms[i + 1];

            if (!current.ConnectedRooms.Contains(next))
                current.ConnectedRooms.Add(next);

            if (!next.ConnectedRooms.Contains(current))
                next.ConnectedRooms.Add(current);
        }
    }

    private void SetRoomDoors()
    {
        foreach (Room room in _rooms)
        {
            for (int i = 0; i < _directions.Length; i++)
            {
                Vector2Int neighborPos = room.GridPosition + _directions[i];
                Room neighbor = _rooms.FirstOrDefault(r => r.GridPosition == neighborPos);

                // Door exists if rooms are connected
                room.Doors[i] = (neighbor != null) && room.ConnectedRooms.Contains(neighbor);
            }
        }
    }

    private void InstantiateRooms()
    {
        foreach (Room room in _rooms)
        {
            GameObject prefab = GetRoomPrefab(room.Type);
            Vector3 worldPosition = GridToWorldPosition(room.GridPosition);

            GameObject roomInstance = Instantiate(prefab, worldPosition, Quaternion.identity, transform);
            room.PrefabInstance = roomInstance;
            roomInstance.name = $"{room.Type} Room [{room.GridPosition.x},{room.GridPosition.y}]";

            // Configure doors
            RoomController roomController = roomInstance.GetComponent<RoomController>();
            if (roomController != null)
            {
                roomController.InitializeDoors(room.Doors);
            }
        }
    }

    private Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x * _roomSpacing, 0, gridPosition.y * _roomSpacing);
    }

    private GameObject GetRoomPrefab(RoomType type)
    {
        return type switch
        {
            RoomType.Start => _startRoomPrefab,
            RoomType.Boss => _bossRoomPrefab,
            RoomType.Middle => _middleRoomPrefabs[Random.Range(0, _middleRoomPrefabs.Count)],
            _ => _middleRoomPrefabs[0]
        };
    }

    private void SpawnPlayer()
    {
        Room startRoom = _rooms.FirstOrDefault(r => r.Type == RoomType.Start);
        if (startRoom != null && startRoom.PrefabInstance != null)
        {
            Transform spawnPoint = startRoom.PrefabInstance.transform.Find("SpawnPoint");
            if (spawnPoint != null)
            {
                Debug.Log($"Player spawned at {spawnPoint.position}");
                // Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
            }
        }
    }
}

