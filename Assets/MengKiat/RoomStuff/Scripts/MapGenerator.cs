using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RMG
{
    /// <summary>
    /// This class is responsible for procedurally generating a dungeon map by
    /// spawning and connecting room prefabs based on directional exits.
    /// </summary>
    public class MapGenerator : MonoBehaviour
    {
        // Range for how many rooms to generate (random within min/max)
        public int minRooms = 20;
        public int maxRooms = 40;

        // Reference to the special start room prefab (placed first)
        [SerializeField] private Room startRoom;

        // Pool of all available room prefabs (excluding start room)
        [SerializeField] private Room[] rooms;

        // Pre-sorted dictionary for quick access to rooms that have exits in specific directions
        private Dictionary<Dir, List<Room>> sortedRooms = new Dictionary<Dir, List<Room>>() {
            {Dir.bottom, new List<Room>()},
            {Dir.top, new List<Room>()},
            {Dir.left, new List<Room>()},
            {Dir.right, new List<Room>()}
        };

        // Final list of rooms spawned in this dungeon run
        public List<Room> spawnedRooms { get; private set; }

        // System.Random instance used to control all randomness
        public System.Random rng { get; private set; }

        // The seed used for the random generator (useful for debugging/reproducibility)
        public int seed { get; private set; }

        [SerializeField] private Material farthestRoomMaterial;
        [SerializeField] private float roomSizeMultiplier = 2;
        [SerializeField] private int BigBossRounds = 2;

        /// <summary>
        /// Called before Start(). It initializes and categorizes all room prefabs
        /// into directional buckets (top, bottom, left, right).
        /// </summary>
        private void Awake()
        {
            foreach (Room room in rooms)
            {
                room.Init(); // Resets internal state
                if (room.HasExit(Dir.top)) sortedRooms[Dir.top].Add(room);
                if (room.HasExit(Dir.bottom)) sortedRooms[Dir.bottom].Add(room);
                if (room.HasExit(Dir.left)) sortedRooms[Dir.left].Add(room);
                if (room.HasExit(Dir.right)) sortedRooms[Dir.right].Add(room);
            }

            spawnedRooms = new List<Room>();
        }

        /// <summary>
        /// Automatically generate a map when the scene starts.
        /// </summary>
        private void Start()
        {
            Generate();
        }

        /// <summary>
        /// For testing: press 'X' to regenerate the map at runtime.
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                Generate();
            }
        }

        /// <summary>
        /// Generate the map using a random seed based on system time.
        /// </summary>
        public void Generate()
        {
            Generate(System.DateTime.Now.Millisecond);
        }

        /// <summary>
        /// Main generation logic using a specific seed.
        /// </summary>
        public void Generate(int newSeed)
        {
            Clear(); // Destroy old rooms

            // Spawn and initialize the start room
            Room start = Instantiate(startRoom, transform);
            start.Init();

            seed = newSeed;
            rng = new System.Random(newSeed);
            int targetNumRooms = rng.Next(minRooms, maxRooms); // How many rooms to generate

            List<Room> openRooms = new List<Room>(); // Rooms that still have open spawns
            spawnedRooms.Add(start);
            openRooms.Add(start);

            // Keep adding rooms until we reach the target count
            while (openRooms.Count > 0 && spawnedRooms.Count < targetNumRooms)
            {
                Room rndRoom = openRooms[rng.Next(openRooms.Count)];

                // Skip if this room has no open spawns left
                if (rndRoom.openSpawns.Count == 0)
                {
                    openRooms.Remove(rndRoom);
                    continue;
                }

                // Pick a random spawn point and direction
                RoomSpawn rndSpawn = rndRoom.openSpawns[rng.Next(rndRoom.openSpawns.Count)];
                Dir dir = Utils.FlipDir(Utils.Vector3ToDir(rndSpawn.position));

                // Try to find a valid room to connect in that direction
                Room newRoom = GetRndRoom(dir, rndRoom, rndSpawn);
                if (newRoom != null)
                {
                    // Make connection and add to the map
                    rndRoom.AddConnection(newRoom);
                    newRoom.AddConnection(rndRoom);
                    spawnedRooms.Add(newRoom);

                    if (newRoom.openSpawns.Count > 0)
                        openRooms.Add(newRoom);
                }
            }

            CalculateScores(); // Calculate distance from start for each room
            transform.localScale = Vector3.one * roomSizeMultiplier;
            var enemySpawner = FindFirstObjectByType<EnemySpawner>();
            enemySpawner.GetAllRoomSpawnPoint();
        }

        /// <summary>
        /// Destroy all previously spawned rooms and reset the list.
        /// </summary>
        private void Clear()
        {
            transform.localScale = Vector3.one;
            foreach (Room spawned in spawnedRooms)
            {
                spawned.gameObject.SetActive(false);
                Destroy(spawned.gameObject); // TODO: Replace with object pooling
            }
            spawnedRooms.Clear();
        }

        /// <summary>
        /// Tries to get a random room prefab that can connect at a given direction and fit without overlapping.
        /// </summary>
        private Room GetRndRoom(Dir dir, Room parent, RoomSpawn parentSpawn)
        {
            Room newRoom = null;
            List<Room> validRooms = new List<Room>(sortedRooms[dir]);
            HashSet<Room> collidedRooms = new HashSet<Room>();

            while (validRooms.Count > 0)
            {
                Room curr = validRooms[rng.Next(validRooms.Count)];
                validRooms.Remove(curr);

                // Get world position of where the new room would be placed
                Vector3 pos = parent.transform.position + parentSpawn.position;
                List<RoomSpawn> validSpawns = new List<RoomSpawn>(curr.sortedSpawns[dir]);

                bool succeeded = false;
                RoomSpawn childSpawn = null;

                int spawnI = rng.Next(validSpawns.Count);
                int spawnIStart = spawnI;

                // Try all matching spawn points until one fits
                while (true)
                {
                    childSpawn = validSpawns[spawnI];
                    Vector3 pos2 = pos - childSpawn.position;

                    // Check if new room would overlap any existing rooms
                    List<Room> hitRooms = RoomCollisionCheck(pos2, curr.bounds);
                    foreach (Room hitRoom in hitRooms)
                        collidedRooms.Add(hitRoom);

                    if (hitRooms.Count == 0)
                    {
                        succeeded = true;
                        pos = pos2;
                        break;
                    }

                    spawnI = (spawnI + 1) % validSpawns.Count;
                    if (spawnI == spawnIStart)
                        break; // Tried all, none fit
                }

                // If placement succeeded, instantiate and place room
                if (succeeded)
                {
                    newRoom = Instantiate(curr, transform);
                    newRoom.Init();
                    newRoom.transform.position = pos;

                    // Close spawn points on both sides
                    newRoom.CloseSpawn(newRoom.sortedSpawns[dir][spawnI], parent);
                    parent.CloseSpawn(parentSpawn, newRoom);
                    break;
                }
            }

            // If no room fit, try to connect overlapping spawns instead
            if (newRoom == null)
            {
                ConnectOverlapSpawns(parent, parentSpawn, collidedRooms);
            }

            return newRoom;
        }

        /// <summary>
        /// Check whether a room at the given position would collide with any spawned room.
        /// </summary>
        private List<Room> RoomCollisionCheck(Vector3 pos, Bounds bounds)
        {
            List<Room> rooms = new List<Room>();
            Bounds bounds1 = new Bounds(bounds.center + pos, bounds.size);

            foreach (Room room in spawnedRooms)
            {
                Bounds bounds2 = new Bounds(room.bounds.center + room.transform.position, room.bounds.size);
                if (bounds1.Intersects(bounds2))
                {
                    rooms.Add(room);
                }
            }

            return rooms;
        }

        /// <summary>
        /// If normal room placement fails due to overlap, try to connect with nearby rooms manually.
        /// </summary>
        private void ConnectOverlapSpawns(Room parent, RoomSpawn parentSpawn, HashSet<Room> collidedRooms)
        {
            Vector3 pos1 = parent.transform.position + parentSpawn.position;
            parent.CloseSpawn(parentSpawn, null); // Mark spawn as closed

            foreach (Room room in collidedRooms)
            {
                if (room == parent)
                    continue;

                Vector3 pos2 = room.transform.position;
                foreach (RoomSpawn spawn in room.spawns)
                {
                    if (pos2 + spawn.position == pos1)
                    {
                        room.CloseSpawn(spawn, parent);
                        parent.CloseSpawn(parentSpawn, room);
                        parent.AddConnection(room);
                        room.AddConnection(parent);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// After generation, this uses BFS to calculate the distance of each room from the start room.
        /// This is useful for detecting which is the furthest room (e.g. for placing end/goal room).
        /// </summary>
        private void CalculateScores()
        {
            Queue<Room> openRooms = new Queue<Room>();
            HashSet<Room> closedRooms = new HashSet<Room>();

            openRooms.Enqueue(spawnedRooms[0]);
            spawnedRooms[0].distanceFromHome = 0;

            while (openRooms.Count > 0)
            {
                Room current = openRooms.Dequeue();
                closedRooms.Add(current);

                foreach (Room child in current.connections)
                {
                    int score = current.distanceFromHome + 1;
                    bool beenChecked = closedRooms.Contains(child);

                    if (!beenChecked || (beenChecked && child.distanceFromHome > score))
                    {
                        child.distanceFromHome = score;
                    }

                    if (!beenChecked)
                    {
                        openRooms.Enqueue(child);
                    }
                }
            }

            // ✅ Highlight the farthest room
            Room farthestRoom = null;
            int maxDistance = -1;
            foreach (Room room in spawnedRooms)
            {
                if (room.distanceFromHome > maxDistance)
                {
                    maxDistance = room.distanceFromHome;
                    farthestRoom = room;
                }
            }

            if (farthestRoom != null && farthestRoomMaterial != null)
            {
                // Change material of all renderers in that room
                MeshRenderer[] renderers = farthestRoom.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer renderer in renderers)
                {
                    renderer.material = farthestRoomMaterial;
                }
            }

            farthestRoom.gameObject.name = "Farthest Room";
            farthestRoom.gameObject.AddComponent<FarthestRoom>();
            int round = DifficultyManager.Instance.GetRound();
            if (round % BigBossRounds != 0)
            {
                FindFirstObjectByType<EnemySpawner>()?.ChooseMiniBoss();
            }
            else
            {
                FindFirstObjectByType<EnemySpawner>()?.ChooseBigBoss();
            }
        }
    }
}
