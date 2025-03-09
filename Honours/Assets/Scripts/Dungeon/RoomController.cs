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
    public SceneTransition sceneTransition;
    public static RoomController Instance;
    public List<Weapon> availableWeapons;
    public List<RoomSO> availableRooms;
    public RoomSO bossRoom;
    public RoomSO spawnRoom;

    public Room currentRoom;
    GameObject previousRoom;
    GameObject nextRoom; // Store next room reference
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

        // Pick a random room
        RoomSO nextRoomSO = availableRooms[Random.Range(0, availableRooms.Count)];

        LoadRoom(nextRoomSO, door.doorType);
    }

    public void StartRoomTransition()
    {
        // Step 1: Fade out
        sceneTransition.FadeOut(() =>
        {
            // Step 2: Deactivate the previous room
            if (previousRoom != null)
            {
                Destroy(previousRoom); // Or Destroy(previousRoom) if you want to destroy it
            }

            // Step 3: Activate the next room
            if (nextRoom != null)
            {
                nextRoom.SetActive(true);
            }

            // Step 4: Fade in the new room
            sceneTransition.FadeIn(() =>
            {
                // You can call additional logic after fade-in completes
                Debug.Log("Room Transition Complete");
            });
        });
    }

    public void LoadRoom(RoomSO roomSO, Door.DoorType previousDoor)
    {
        if (roomSO == null || roomSO.roomPrefab == null)
        {
            Debug.LogError("RoomSO or Room Prefab is null!");
            return;
        }

        // Create new room and initialize it
        Room newRoom = Instantiate(roomSO.roomPrefab).GetComponent<Room>();
        newRoom.InitializeRoom(roomsCompleted);

        // Set the correct position based on the previous door
        Vector3 spawnPosition = currentRoom.transform.position;
        switch (previousDoor)
        {
            case Door.DoorType.left: spawnPosition.x -= currentRoom.width; break;
            case Door.DoorType.right: spawnPosition.x += currentRoom.width; break;
            case Door.DoorType.top: spawnPosition.y += currentRoom.height; break;
            case Door.DoorType.bottom: spawnPosition.y -= currentRoom.height; break;
        }
        newRoom.transform.position = spawnPosition;

        // Enable a single exit door that does NOT allow backtracking
        newRoom.EnableSingleExitDoor(previousDoor);

        // Set previous room reference for transition
        previousRoom = currentRoom.gameObject;
        nextRoom = newRoom.gameObject; // Set the next room reference
        currentRoom = newRoom;
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
