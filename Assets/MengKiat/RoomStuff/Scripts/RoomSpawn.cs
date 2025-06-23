using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RMG
{
    public class RoomSpawn : MonoBehaviour
    {
        [HideInInspector] public Vector3 position;
        [SerializeField] private GameObject wall;
        [SerializeField] private GameObject doorway;
        public bool spawned
        {
            get; private set;
        }
        public Room connectedTo
        {
            get; private set;
        }

        public void Clear()
        {
            spawned = false;
            connectedTo = null;
            UpdateWalls();
        }

        public void Connect(Room room)
        {
            spawned = true;
            connectedTo = room;
            UpdateWalls();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = connectedTo != null ? Color.green : Color.grey;
            Gizmos.DrawSphere(transform.position, 0.5f);
        }

        private void UpdateWalls()
        {
            if (doorway == null || wall == null) return;

            bool hasConnection = connectedTo != null;
            doorway.SetActive(hasConnection);
            wall.SetActive(!hasConnection);
        }

    }
}