using RMG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCreation : MonoBehaviour
{
    [SerializeField] private Transform Player;
    [SerializeField] private GameObject RoomeContainer;
    [SerializeField] private GameObject MiniMapContainer;
    [SerializeField] private List<Room> Rooms = new List<Room>();
    [SerializeField] private GameObject HiddenRoomPrefab;
    [SerializeField] private GameObject RoomPrefab;
    [SerializeField] private float iconScale = 0.1f;

    private Dictionary<Room, GameObject> discoveredRooms = new Dictionary<Room, GameObject>();
    private Dictionary<Room, GameObject> undiscoveredRooms = new Dictionary<Room, GameObject>();
    public static MiniMapCreation Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }


    }

    private void Start()
    {
        CheckForRooms();

        foreach (Room room in Rooms)
        {
            //GameObject UndiscoverRoom = Instantiate(HiddenRoomPrefab, MiniMapContainer.transform);
            //UndiscoverRoom.GetComponent<RectTransform>().anchoredPosition = WorldSpaceToPosition(room.transform.position);
            //undiscoveredRooms.Add(room , UndiscoverRoom);
        }


    }

    private void CheckForRooms()
    {
        Rooms.Clear();
        for (int i = 0; i < RoomeContainer.transform.childCount; i++)
        {
            Room room = RoomeContainer.transform.GetChild(i).GetComponent<Room>();
            Debug.Log(room);
            if (room != null)
            {
                Rooms.Add(room);
            }
        }
    }
    private void Update()
    {
        if (Rooms.Count <= 0 ) {
            CheckForRooms();

            foreach (Room room in Rooms)
            {
                GameObject UndiscoverRoom = Instantiate(HiddenRoomPrefab, MiniMapContainer.transform);
                UndiscoverRoom.GetComponent<RectTransform>().anchoredPosition = WorldSpaceToPosition(room.transform.position);
                Debug.Log(WorldSpaceToPosition(room.transform.position));
                //undiscoveredRooms.Add(room, UndiscoverRoom);
            }

        }
    }
    public void DiscoverRoom(Room room)
    {
        if (undiscoveredRooms.ContainsKey(room))
        {
            Destroy(undiscoveredRooms[room]);
            undiscoveredRooms.Remove(room);
        }

        //discoveredRooms.Add(room, roomIcon);
    }

    private Vector2 WorldSpaceToPosition(Vector3 worldPos)
    {
        return new Vector2(worldPos.x , worldPos.z) * iconScale;
    }
}
