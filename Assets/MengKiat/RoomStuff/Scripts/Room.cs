using System.Collections.Generic;
using UnityEngine;

namespace RMG
{
    public class Room : MonoBehaviour
    {
        [SerializeField] private Bounds _bounds;
        public Bounds bounds => _bounds;

        public RoomSpawn[] spawns { get; private set; }
        public List<RoomSpawn> openSpawns { get; set; }

        // Categorized spawn points by direction (top/bottom/left/right)
        public Dictionary<Dir, List<RoomSpawn>> sortedSpawns { get; private set; }

        // Rooms this one is directly connected to
        public List<Room> connections { get; private set; }

        public int distanceFromHome = 0; // Used for pathfinding/distance scoring

        [Header("Walls & Doorways")]
        public GameObject topWall;
        public GameObject topDoor;
        public GameObject bottomWall, bottomDoor;
        public GameObject leftWall, leftDoor;
        public GameObject rightWall, rightDoor;

        public void Init()
        {
            spawns = GetComponentsInChildren<RoomSpawn>(true);
            connections = new List<Room>();
            openSpawns = new List<RoomSpawn>(spawns);
            sortedSpawns = new Dictionary<Dir, List<RoomSpawn>> {
                { Dir.top, new List<RoomSpawn>() },
                { Dir.bottom, new List<RoomSpawn>() },
                { Dir.left, new List<RoomSpawn>() },
                { Dir.right, new List<RoomSpawn>() }
            };

            foreach (RoomSpawn spawn in spawns)
            {
                spawn.Clear();
                spawn.position = spawn.transform.localPosition;
                Dir dir = Utils.Vector3ToDir(spawn.position);
                sortedSpawns[dir].Add(spawn);
            }
        }

        // Marks a spawn as connected and removes it from open list
        public void CloseSpawn(RoomSpawn spawn, Room connection)
        {
            spawn.Connect(connection);
            openSpawns.Remove(spawn);
        }

        public void AddConnection(Room room)
        {
            connections.Add(room);
        }

        public bool HasExit(Dir dir)
        {
            return sortedSpawns[dir].Count > 0;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + bounds.center, bounds.size);
        }

        public void UpdateAllWalls()
        {
            UpdateWall(Dir.top);
            UpdateWall(Dir.bottom);
            UpdateWall(Dir.left);
            UpdateWall(Dir.right);
        }

        private void UpdateWall(Dir direction)
        {
            bool hasConnection = HasConnectionInDirection(direction);

            switch (direction)
            {
                case Dir.top:
                    topWall.SetActive(!hasConnection);
                    topDoor.SetActive(hasConnection);
                    break;
                case Dir.bottom:
                    bottomWall.SetActive(!hasConnection);
                    bottomDoor.SetActive(hasConnection);
                    break;
                case Dir.left:
                    leftWall.SetActive(!hasConnection);
                    leftDoor.SetActive(hasConnection);
                    break;
                case Dir.right:
                    rightWall.SetActive(!hasConnection);
                    rightDoor.SetActive(hasConnection);
                    break;
            }
        }

        public bool HasConnectionInDirection(Dir direction)
        {
            // Check if any spawn point in this direction is connected
            foreach (RoomSpawn spawn in sortedSpawns[direction])
            {
                if (spawn.connectedTo != null)
                    return true;
            }

            // Additionally check all connected rooms to see if they're in this direction
            Vector3 dirVector = DirectionToVector(direction);
            foreach (Room connectedRoom in connections)
            {
                Vector3 relativePos = connectedRoom.transform.position - transform.position;
                if (Vector3.Dot(relativePos.normalized, dirVector) > 0.9f) // Roughly in same direction
                {
                    return true;
                }
            }

            return false;
        }

        private Vector3 DirectionToVector(Dir direction)
        {
            switch (direction)
            {
                case Dir.top: return Vector3.forward;
                case Dir.bottom: return Vector3.back;
                case Dir.left: return Vector3.left;
                case Dir.right: return Vector3.right;
                default: return Vector3.zero;
            }
        }
    }
}
