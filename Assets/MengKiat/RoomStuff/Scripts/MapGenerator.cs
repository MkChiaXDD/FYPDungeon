using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RMG
{
    public class MapGenerator : MonoBehaviour
    {
        public int minRooms = 20;
        public int maxRooms = 40;

        [SerializeField] private Room startRoom;
        [SerializeField] private Room bossRoomPrefab;
        [SerializeField] private Room[] rooms;
        [SerializeField] private Room[] uniqueRooms; // NEW: rooms allowed only once

        private HashSet<Room> usedUniqueRooms = new HashSet<Room>(); // NEW: track used unique rooms

        private Dictionary<Dir, List<Room>> sortedRooms = new Dictionary<Dir, List<Room>>() {
            {Dir.bottom, new List<Room>()},
            {Dir.top, new List<Room>()},
            {Dir.left, new List<Room>()},
            {Dir.right, new List<Room>()}
        };

        public List<Room> spawnedRooms { get; private set; }
        public System.Random rng { get; private set; }
        public int seed { get; private set; }

        [SerializeField] private Material farthestRoomMaterial;
        [SerializeField] private float roomSizeMultiplier = 2;
        [SerializeField] private int BigBossRounds = 2;
        [SerializeField] private GameObject portal;

        private void Awake()
        {
            foreach (Room room in rooms)
            {
                room.Init();
                if (room.HasExit(Dir.top)) sortedRooms[Dir.top].Add(room);
                if (room.HasExit(Dir.bottom)) sortedRooms[Dir.bottom].Add(room);
                if (room.HasExit(Dir.left)) sortedRooms[Dir.left].Add(room);
                if (room.HasExit(Dir.right)) sortedRooms[Dir.right].Add(room);
            }

            foreach (Room room in uniqueRooms)
            {
                room.Init();
                if (room.HasExit(Dir.top)) sortedRooms[Dir.top].Add(room);
                if (room.HasExit(Dir.bottom)) sortedRooms[Dir.bottom].Add(room);
                if (room.HasExit(Dir.left)) sortedRooms[Dir.left].Add(room);
                if (room.HasExit(Dir.right)) sortedRooms[Dir.right].Add(room);
            }

            spawnedRooms = new List<Room>();
        }

        private void Start()
        {
            Generate();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                Generate();
            }
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
            rng = new System.Random(newSeed);
            int targetNumRooms = rng.Next(minRooms, maxRooms);

            List<Room> openRooms = new List<Room>();
            spawnedRooms.Add(start);
            openRooms.Add(start);

            while (openRooms.Count > 0 && spawnedRooms.Count < targetNumRooms)
            {
                Room rndRoom = openRooms[rng.Next(openRooms.Count)];

                if (rndRoom.openSpawns.Count == 0)
                {
                    openRooms.Remove(rndRoom);
                    continue;
                }

                RoomSpawn rndSpawn = rndRoom.openSpawns[rng.Next(rndRoom.openSpawns.Count)];
                Dir dir = Utils.FlipDir(Utils.Vector3ToDir(rndSpawn.position));

                Room newRoom = GetRndRoom(dir, rndRoom, rndSpawn);
                if (newRoom != null)
                {
                    rndRoom.AddConnection(newRoom);
                    newRoom.AddConnection(rndRoom);
                    spawnedRooms.Add(newRoom);

                    if (newRoom.openSpawns.Count > 0)
                        openRooms.Add(newRoom);
                }
            }

            CalculateScores();
            transform.localScale = Vector3.one * roomSizeMultiplier;
            var enemySpawner = FindFirstObjectByType<EnemySpawner>();
            enemySpawner.GetAllRoomSpawnPoint();
        }

        private void Clear()
        {
            transform.localScale = Vector3.one;
            foreach (Room spawned in spawnedRooms)
            {
                spawned.gameObject.SetActive(false);
                Destroy(spawned.gameObject);
            }
            spawnedRooms.Clear();
            usedUniqueRooms.Clear(); // Reset unique room tracking
        }

        private Room GetRndRoom(Dir dir, Room parent, RoomSpawn parentSpawn)
        {
            Room newRoom = null;
            List<Room> validRooms = new List<Room>();

            foreach (Room r in sortedRooms[dir])
            {
                if (!uniqueRooms.Contains(r) || !usedUniqueRooms.Contains(r))
                {
                    validRooms.Add(r);
                }
            }

            HashSet<Room> collidedRooms = new HashSet<Room>();

            while (validRooms.Count > 0)
            {
                Room curr = validRooms[rng.Next(validRooms.Count)];
                validRooms.Remove(curr);

                Vector3 pos = parent.transform.position + parentSpawn.position;
                List<RoomSpawn> validSpawns = new List<RoomSpawn>(curr.sortedSpawns[dir]);

                bool succeeded = false;
                RoomSpawn childSpawn = null;

                int spawnI = rng.Next(validSpawns.Count);
                int spawnIStart = spawnI;

                while (true)
                {
                    childSpawn = validSpawns[spawnI];
                    Vector3 pos2 = pos - childSpawn.position;

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
                        break;
                }

                if (succeeded)
                {
                    newRoom = Instantiate(curr, transform);
                    newRoom.Init();
                    newRoom.transform.position = pos;

                    newRoom.CloseSpawn(newRoom.sortedSpawns[dir][spawnI], parent);
                    parent.CloseSpawn(parentSpawn, newRoom);

                    if (uniqueRooms.Contains(curr))
                    {
                        usedUniqueRooms.Add(curr); // Mark as used
                    }

                    break;
                }
            }

            if (newRoom == null)
            {
                ConnectOverlapSpawns(parent, parentSpawn, collidedRooms);
            }

            return newRoom;
        }

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

        private void ConnectOverlapSpawns(Room parent, RoomSpawn parentSpawn, HashSet<Room> collidedRooms)
        {
            Vector3 pos1 = parent.transform.position + parentSpawn.position;
            parent.CloseSpawn(parentSpawn, null);

            foreach (Room room in collidedRooms)
            {
                if (room == parent) continue;

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

            if (farthestRoom != null)
            {
                Vector3 farthestRoomPosition = farthestRoom.transform.position;
                Quaternion farthestRoomRotation = farthestRoom.transform.rotation;
                List<Room> connectedRooms = new List<Room>(farthestRoom.connections);

                spawnedRooms.Remove(farthestRoom);
                Destroy(farthestRoom.gameObject);

                Room bossRoom = Instantiate(bossRoomPrefab, farthestRoomPosition, farthestRoomRotation, transform);
                bossRoom.Init();
                spawnedRooms.Add(bossRoom);

                foreach (Room connectedRoom in connectedRooms)
                {
                    bossRoom.AddConnection(connectedRoom);
                    connectedRoom.AddConnection(bossRoom);
                    connectedRoom.connections.Remove(farthestRoom);
                }

                bossRoom.gameObject.name = "Boss Room";
                bossRoom.distanceFromHome = maxDistance;

                Transform spawnPoint = bossRoom.transform.Find("EnemySpawnPoint");
                if (spawnPoint != null)
                {
                    var portalObj = Instantiate(portal, spawnPoint.position * roomSizeMultiplier, Quaternion.identity);
                    portalObj.SetActive(false);
                }

                int round = DifficultyManager.Instance.GetRound();
                var enemySpawner = FindFirstObjectByType<EnemySpawner>();
                if (enemySpawner != null)
                {
                    if (round % BigBossRounds != 0)
                    {
                        enemySpawner.ChooseMiniBoss();
                    }
                    else
                    {
                        enemySpawner.ChooseBigBoss();
                    }
                }
            }
        }
    }
}
