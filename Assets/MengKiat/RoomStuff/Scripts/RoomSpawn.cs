using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RMG
{
    /// <summary>
    /// RoomSpawn represents a possible exit/entry point in a Room.
    /// It handles connection state, visual updates (wall vs doorway), and positional info for placement.
    /// </summary>
    public class RoomSpawn : MonoBehaviour
    {
        // The local position of this spawn point (set during Init)
        // Used to calculate placement and direction relative to its parent Room
        [HideInInspector]
        public Vector3 position;

        // Reference to the wall GameObject (used when there's no connection)
        [SerializeField]
        private GameObject wall;

        // Reference to the doorway GameObject (used when a connection is made)
        [SerializeField]
        private GameObject doorway;

        // Whether this spawn point has already been used to connect to another room
        public bool spawned { get; private set; }

        // Reference to the room this spawn is connected to (null if unconnected)
        public Room connectedTo { get; private set; }

        /// <summary>
        /// Clears the connection and resets the spawn to unused.
        /// Called when reinitializing the room or destroying a layout.
        /// </summary>
        public void Clear()
        {
            spawned = false;         // Mark this spawn as not yet used
            connectedTo = null;      // Remove any connection reference
            UpdateWalls();           // Update visuals (enable wall, disable doorway)
        }

        /// <summary>
        /// Connects this spawn to another room.
        /// Used during dungeon generation when rooms are linked.
        /// </summary>
        /// <param name="room">The Room that this spawn connects to.</param>
        public void Connect(Room room)
        {
            spawned = true;          // Mark this spawn as used
            connectedTo = room;      // Store the connected room reference
            UpdateWalls();           // Update visuals (disable wall, enable doorway)
        }

        /// <summary>
        /// Draws a visual gizmo in the scene view to indicate this spawn's connection state.
        /// Green = connected, Grey = unconnected.
        /// </summary>
        private void OnDrawGizmos()
        {
            // Choose color based on whether this spawn is connected
            Gizmos.color = connectedTo != null ? Color.green : Color.grey;

            // Draw a small sphere at the spawn point's position
            Gizmos.DrawSphere(transform.position, 0.5f);
        }

        /// <summary>
        /// Updates the visibility of the wall and doorway based on connection state.
        /// </summary>
        private void UpdateWalls()
        {
            // If either the wall or doorway GameObjects aren't assigned, exit early
            if (doorway == null || wall == null) return;

            // If connected, show the doorway and hide the wall. Otherwise, hide the doorway and show the wall.
            bool hasConnection = connectedTo != null;
            doorway.SetActive(hasConnection);
            wall.SetActive(!hasConnection);
        }
    }
}
