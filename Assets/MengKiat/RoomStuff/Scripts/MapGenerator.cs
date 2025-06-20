using System.Collections.Generic;
using UnityEngine;

namespace RMG
{
    public class MapGenerator : MonoBehaviour
    {
        public int minRooms = 20;
        public int maxRooms = 40;
        [SerializeField] private Room startRoom;
        [SerializeField] private Room[] rooms;

        // Group rooms by which directions they have exits
        private Dictionary<Dir, List<Room>> sortedRooms = new Dictionary<Dir, List<Room>>() {
            {Dir.bottom, new List<Room>()},
            {Dir.top, new List<Room>()},
            {Dir.left, new List<Room>()},
            {Dir.right, new List<Room>()}
        };

        public List<Room> spawnedRooms { get; private set; }
        public System.Random rng { get; private set; }
        public int seed { get; private set; }

        private void Awake()
        {
            // Organize all room prefabs by direction
            foreach (Room room in rooms)
            {
                room.Init();
                foreach (Dir dir in sortedRooms.Keys)
                {
                    if (room.HasExit(dir))
                        sortedRooms[dir].Add(room);
                }
            }
            spawnedRooms = new List<Room>();
        }

        private void Update()
        {
            // Press X to generate new dungeon
            if (Input.GetKeyDown(KeyCode.X))
                Generate();
        }

        public void Generate()
        {
            Generate(System.DateTime.Now.Millisecond);
        }

        public void Generate(int newSeed)
        {
            Clear();

            Room start = Instantiate(startRoom, transform);
            start.Init();

            seed = newSeed;
            rng = new System.Random(seed);
            int targetNumRooms = rng.Next(minRooms, maxRooms);

            List<Room> openRooms = new List<Room> { start };
            spawnedRooms.Add(start);

            while (openRooms.Count > 0 && spawnedRooms.Count < targetNumRooms)
            {
                Room parent = openRooms[rng.Next(openRooms.Count)];
                if (parent.openSpawns.Count == 0)
                {
                    openRooms.Remove(parent);
                    continue;
                }

                RoomSpawn spawn = parent.openSpawns[rng.Next(parent.openSpawns.Count)];
                Dir dir = Utils.FlipDir(Utils.Vector3ToDir(spawn.position));

                Room newRoom = GetRndRoom(dir, parent, spawn);
                if (newRoom != null)
                {
                    parent.AddConnection(newRoom);
                    newRoom.AddConnection(parent);
                    spawnedRooms.Add(newRoom);
                    if (newRoom.openSpawns.Count > 0)
                        openRooms.Add(newRoom);
                }
            }

            foreach (Room room in spawnedRooms)
            {
                room.UpdateAllWalls();
            }

            CalculateScores(); // BFS scoring from start room
        }

        private void Clear()
        {
            foreach (Room room in spawnedRooms)
            {
                Destroy(room.gameObject); // TODO: Replace with pooling
            }
            spawnedRooms.Clear();
        }

        // Attempts to get a room prefab with matching entrance and no collision
        private Room GetRndRoom(Dir dir, Room parent, RoomSpawn parentSpawn)
        {
            List<Room> validRooms = new List<Room>(sortedRooms[dir]);
            HashSet<Room> collidedRooms = new HashSet<Room>();

            while (validRooms.Count > 0)
            {
                Room candidate = validRooms[rng.Next(validRooms.Count)];
                validRooms.Remove(candidate);

                List<RoomSpawn> childSpawns = candidate.sortedSpawns[dir];
                int i = rng.Next(childSpawns.Count);
                int startIndex = i;

                while (true)
                {
                    RoomSpawn childSpawn = childSpawns[i];
                    Vector3 newPos = parent.transform.position + parentSpawn.position - childSpawn.position;

                    if (RoomCollisionCheck(newPos, candidate.bounds).Count == 0)
                    {
                        Room newRoom = Instantiate(candidate, transform);
                        newRoom.Init();
                        newRoom.transform.position = newPos;

                        newRoom.CloseSpawn(childSpawn, parent);
                        parent.CloseSpawn(parentSpawn, newRoom);

                        return newRoom;
                    }

                    i = (i + 1) % childSpawns.Count;
                    if (i == startIndex)
                        break;
                }
            }

            // If failed, try soft-connect overlapping spawns
            ConnectOverlapSpawns(parent, parentSpawn, collidedRooms);
            return null;
        }

        private List<Room> RoomCollisionCheck(Vector3 pos, Bounds bounds)
        {
            List<Room> collisions = new();
            Bounds check = new(pos + bounds.center, bounds.size);
            foreach (Room room in spawnedRooms)
            {
                Bounds existing = new(room.bounds.center + room.transform.position, room.bounds.size);
                if (check.Intersects(existing))
                    collisions.Add(room);
            }
            return collisions;
        }

        // Handles special case: spawn collides with another but can connect logically
        private void ConnectOverlapSpawns(Room parent, RoomSpawn parentSpawn, HashSet<Room> collidedRooms)
        {
            Vector3 pos1 = parent.transform.position + parentSpawn.position;
            parent.CloseSpawn(parentSpawn, null);

            foreach (Room room in collidedRooms)
            {
                if (room == parent) continue;

                Vector3 basePos = room.transform.position;
                foreach (RoomSpawn spawn in room.spawns)
                {
                    if (basePos + spawn.position == pos1)
                    {
                        room.CloseSpawn(spawn, parent);
                        parent.CloseSpawn(parentSpawn, room);
                        room.AddConnection(parent);
                        parent.AddConnection(room);

                        // Update walls for both rooms
                        room.UpdateAllWalls();
                        parent.UpdateAllWalls();
                        return;
                    }
                }
            }
        }

        // Assigns distance-from-start scores using BFS
        private void CalculateScores()
        {
            Queue<Room> open = new();
            HashSet<Room> visited = new();

            Room start = spawnedRooms[0];
            start.distanceFromHome = 0;
            open.Enqueue(start);

            while (open.Count > 0)
            {
                Room current = open.Dequeue();
                visited.Add(current);

                foreach (Room neighbor in current.connections)
                {
                    int newDist = current.distanceFromHome + 1;
                    if (!visited.Contains(neighbor) || neighbor.distanceFromHome > newDist)
                    {
                        neighbor.distanceFromHome = newDist;
                        open.Enqueue(neighbor);
                    }
                }
            }
        }
    }
}
