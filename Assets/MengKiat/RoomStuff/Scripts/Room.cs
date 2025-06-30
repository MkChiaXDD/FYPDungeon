using System.Collections.Generic;
using UnityEngine;

namespace RMG
{
    public class Room : MonoBehaviour
    {
        // Serialized bounds of the room used for collision detection and layout
        [SerializeField] private Bounds _bounds;

        // Public property to access room bounds
        public Bounds bounds
        {
            get { return _bounds; }
        }

        // All RoomSpawn points in this room (exit/entry points)
        public RoomSpawn[] spawns { get; private set; }

        // List of spawn points that are not yet connected to other rooms
        public List<RoomSpawn> openSpawns { get; set; }

        // Sorted dictionary of spawn points based on direction (top, bottom, left, right)
        public Dictionary<Dir, List<RoomSpawn>> sortedSpawns { get; private set; }

        // List of rooms that are connected to this room via valid paths
        public List<Room> connections { get; private set; }

        // How far this room is from the start room (used for pathfinding / scoring)
        public int distanceFromHome = 0;

        // Initializes the room when it is spawned or reset
        public void Init()
        {
            // Get all RoomSpawn components in this room, including disabled ones
            spawns = GetComponentsInChildren<RoomSpawn>(true);

            // Prepare connection and open spawn lists
            connections = new List<Room>();
            openSpawns = new List<RoomSpawn>(spawns);

            // Initialize dictionary to categorize spawns by direction
            sortedSpawns = new Dictionary<Dir, List<RoomSpawn>>()
            {
                {Dir.top, new List<RoomSpawn>()},
                {Dir.bottom, new List<RoomSpawn>()},
                {Dir.left, new List<RoomSpawn>()},
                {Dir.right, new List<RoomSpawn>()}
            };

            // Iterate over each spawn to assign direction and setup
            foreach (RoomSpawn spawn in spawns)
            {
                // Clear existing connection state
                spawn.Clear();

                // Cache local position for alignment calculations
                spawn.position = spawn.transform.position;

                // Determine the direction this spawn is facing
                Dir dir = Utils.Vector3ToDir(spawn.position);

                // Add spawn into the appropriate directional list
                sortedSpawns[dir].Add(spawn);
            }
        }

        // Closes off a spawn and marks it connected to another room
        public void CloseSpawn(RoomSpawn spawn, Room connection)
        {
            // Mark the spawn as connected to the specified room
            spawn.Connect(connection);

            // Remove it from open spawn list since it's now used
            openSpawns.Remove(spawn);
        }

        // Adds a connection between this room and another room
        public void AddConnection(Room room)
        {
            // Track room connection to allow navigation, scoring, etc.
            connections.Add(room);
        }

        // Returns true if the room has at least one spawn in the specified direction
        public bool HasExit(Dir dir)
        {
            return sortedSpawns[dir].Count > 0;
        }

        // Draws a wireframe cube around the room when selected in the editor
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;

            // Draw the bounds as a visual aid in scene view
            Gizmos.DrawWireCube(transform.position + bounds.center, bounds.size);
        }
    }
}
