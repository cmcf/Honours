using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoomDirection
{
    Up,
    Down,
    Left,
    Right
}

public class RoomController : MonoBehaviour
{
    public static RoomController Instance;
    public List<Weapon> availableWeapons;
    public List<RoomSO> availableRooms;
    public RoomSO bossRoom;
    public RoomSO spawnRoom;

    public Room currentRoom;
    public Vector3 currentRoomPosition; // Track the position of the last room
    public RoomDirection currentDirection; // Track the current direction to spawn rooms in

    int roomsCompleted = 0;
    const int roomsBeforeBoss = 2;
    bool leftSpawnRoom = false;

    void Awake()
    {
        Instance = this;

        // Load the spawn room when the game starts
        LoadSpawnRoom(); 
    }

    void Update()
    {
        UpdateRooms();
    }

    void LoadSpawnRoom()
    {
        if (spawnRoom == null || spawnRoom.roomPrefab == null)
        {
            Debug.LogError("Spawn room or its prefab is missing!");
            return;
        }

        // Instantiate the spawn room at a fixed position
        Room newRoom = Instantiate(spawnRoom.roomPrefab, Vector3.zero, Quaternion.identity).GetComponent<Room>();

        newRoom.roomSO = spawnRoom;
        newRoom.InitializeRoom(roomsCompleted);

        currentRoom = newRoom;
        currentRoomPosition = newRoom.transform.position; 
    }


    public void LoadNextRoom(Door door)
    {
        if (availableRooms.Count == 0)
        {
            Debug.LogError("No available rooms to spawn!");
            return;
        }

        RoomSO nextRoomSO = availableRooms[Random.Range(0, availableRooms.Count)]; // Pick a random room

        LoadRoom(nextRoomSO, door.doorType); 
    }


    public void LoadRoom(RoomSO roomSO, Door.DoorType doorType)
    {
        if (DoesRoomExist(roomSO))
        {
            return;
        }

        if (roomSO == null || roomSO.roomPrefab == null)
        {
            Debug.LogError("RoomSO or Room Prefab is null!");
            return;
        }

        Room newRoom = Instantiate(roomSO.roomPrefab).GetComponent<Room>();

        // Calculate spawn position based on doorType
        Vector3 spawnPosition = currentRoom.transform.position;

        switch (doorType)
        {
            case Door.DoorType.left:
                spawnPosition.x -= currentRoom.width;
                break;
            case Door.DoorType.right:
                spawnPosition.x += currentRoom.width;
                break;
            case Door.DoorType.top:
                spawnPosition.y += currentRoom.height;
                break;
            case Door.DoorType.bottom:
                spawnPosition.y -= currentRoom.height;
                break;
        }

        newRoom.transform.position = spawnPosition;
        newRoom.roomSO = roomSO;
        newRoom.InitializeRoom(roomsCompleted);
        currentRoom = newRoom;
        currentRoomPosition = newRoom.transform.position;
    }


    public void OnRoomCompleted()
    {
        roomsCompleted++;

        if (roomsCompleted >= roomsBeforeBoss)
        {
            StartCoroutine(SpawnBossRoom());
        }


        currentRoom.SpawnPickups(); // Spawn pickups for the current room
    }

    public bool DoesRoomExist(RoomSO roomSO)
    {
        return currentRoom != null && currentRoom.roomSO == roomSO;
    }

    public void OnEnterRoom(Room room)
    {
        currentRoom = room;

        if (room.isBossRoom)
        {
            Invoke(nameof(DelayedActivateBoss), 1.5f);
        }

        StartCoroutine(RoomCoroutine());
    }

    void DelayedActivateBoss()
    {
        ActivateBoss(currentRoom);
    }

    void ActivateBoss(Room bossRoom)
    {
        BossFormManager boss = bossRoom.GetComponentInChildren<BossFormManager>();
        if (boss != null)
        {
            boss.gameObject.SetActive(true);
            boss.StartBossBattle();
        }
    }

    IEnumerator RoomCoroutine()
    {
        yield return new WaitForSeconds(0.2f);
        UpdateRooms();
    }

    void UpdateRooms()
    {
        if (currentRoom != null)
        {
            currentRoom.CheckRoomCompletion();
        }
    }

    IEnumerator SpawnBossRoom()
    {
        yield return new WaitForSeconds(1f);
        RoomSO bossRoomSO = bossRoom;
        //LoadRoom(bossRoomSO);
    }
}
