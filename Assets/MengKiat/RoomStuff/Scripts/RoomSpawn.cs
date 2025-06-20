using UnityEngine;

namespace RMG
{
    public class RoomSpawn : MonoBehaviour
    {
        [HideInInspector] public Vector3 position; // Local position of the spawn point relative to its room

        // Whether this spawn has been used to connect another room
        public bool spawned { get; private set; }

        // The room this spawn connects to (can be null if not connected)
        public Room connectedTo { get; private set; }

        // Resets the spawn's connection
        public void Clear()
        {
            spawned = false;
            connectedTo = null;
        }

        // Mark this spawn as used and store the connected room
        public void Connect(Room room)
        {
            spawned = true;
            connectedTo = room;
        }

        // Visualize spawn point in editor
        private void OnDrawGizmos()
        {
            Gizmos.color = connectedTo != null ? Color.green : Color.grey;
            Gizmos.DrawSphere(transform.position, 0.5f);
        }
    }
}
