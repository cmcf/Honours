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
    [Header("References")]
    public SceneTransition sceneTransition;
    public static RoomController Instance;

    [Header("Weapons")]
    public List<Weapon> availableWeapons;

    [Header("Rooms")]
    public List<RoomSO> availableRooms;
    public RoomSO bossRoom;
    public RoomSO spawnRoom;
    public Room currentRoom;
    GameObject previousRoom;
    GameObject nextRoom; // Store next room reference

    [Header("Room position")]
    public Vector3 currentRoomPosition; // Track the position of the last room
    public RoomDirection currentDirection; // Track the current direction to spawn rooms in

    int roomsCompleted = 0;
    [SerializeField] int roomsBeforeBoss = 6;
    bool leftSpawnRoom = false;
    bool hasBossRoomSpawned = false;
    public bool startBossAttack = false;

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
        // Fade out
        sceneTransition.FadeOut(() =>
        {
            // Deactivate the previous room
            if (previousRoom != null)
            {
                Destroy(previousRoom); 
            }

            // Activate the next room
            if (nextRoom != null)
            {
                nextRoom.SetActive(true);
            }

            // Fade in the new room
            sceneTransition.FadeIn(() =>
            { // Check if SpawnEnemies is called
                if (currentRoom != null)
                {
                    Debug.Log("Spawning enemies in the new room");
                    currentRoom.SpawnEnemies();
                }
            });
        });
    }

    public void LoadRoom(RoomSO roomSO, Door.DoorType previousDoor)
    {
        if (roomsCompleted >= roomsBeforeBoss && !hasBossRoomSpawned)
        {
            ReplaceWithBossRoom(previousDoor);
            return;
        }

        if (roomSO == null || roomSO.roomPrefab == null)
        {
            return;
        }

        // Create new room and initialize it
        Room newRoom = Instantiate(roomSO.roomPrefab).GetComponent<Room>();
        newRoom.InitializeRoom(roomsCompleted);

        // Set position and update direction based on previous door
        Vector3 spawnPosition = GetSpawnPosition(previousDoor);
        newRoom.transform.position = spawnPosition;

        // Enable a single exit door that does NOT allow backtracking
        newRoom.EnableSingleExitDoor(previousDoor);

        previousRoom = currentRoom.gameObject;
        nextRoom = newRoom.gameObject;
        currentRoom = newRoom;
    }

    void ReplaceWithBossRoom(Door.DoorType previousDoor)
    {
        if (bossRoom == null || bossRoom.roomPrefab == null)
        {
            Debug.LogError("Boss room or its prefab is missing!");
            return;
        }

        // Set the boss room to spawn where the next room should be
        Vector3 bossRoomPosition = GetSpawnPosition(previousDoor);

        Room newRoom = Instantiate(bossRoom.roomPrefab, bossRoomPosition, Quaternion.identity).GetComponent<Room>();
        newRoom.roomSO = bossRoom;
        newRoom.InitializeRoom(roomsCompleted);

        previousRoom = currentRoom.gameObject;
        nextRoom = newRoom.gameObject;
        currentRoom = newRoom;
        hasBossRoomSpawned = true;

        Invoke("StartBossBattle", 2f);
    }

    Vector3 GetSpawnPosition(Door.DoorType previousDoor)
    {
        Vector3 spawnPosition = currentRoom.transform.position;
        switch (previousDoor)
        {
            case Door.DoorType.left: spawnPosition.x -= currentRoom.width; break;
            case Door.DoorType.right: spawnPosition.x += currentRoom.width; break;
            case Door.DoorType.top: spawnPosition.y += currentRoom.height; break;
            case Door.DoorType.bottom: spawnPosition.y -= currentRoom.height; break;
        }
        return spawnPosition;
    }



    public void OnRoomCompleted()
    {
        roomsCompleted++;
        currentRoom.SpawnPickups(); // Spawn pickups for the current room
    }


    public bool DoesRoomExist(RoomSO roomSO)
    {
        return currentRoom != null && currentRoom.roomSO == roomSO;
    }

    public void OnEnterRoom(Room room)
    {
        currentRoom = room;
        Debug.Log("Entered room");
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

    void SpawnBossRoom()
    {
        if (bossRoom == null || bossRoom.roomPrefab == null)
        {
            Debug.LogError("Boss room or its prefab is missing!");
            return;
        }

        // Spawn position based on last used door direction
        Vector3 bossRoomPosition = currentRoom.transform.position;

        switch (currentDirection) // Use the last movement direction
        {
            case RoomDirection.Up: bossRoomPosition += new Vector3(0, currentRoom.height, 0); break;
            case RoomDirection.Down: bossRoomPosition += new Vector3(0, -currentRoom.height, 0); break;
            case RoomDirection.Left: bossRoomPosition += new Vector3(-currentRoom.width, 0, 0); break;
            case RoomDirection.Right: bossRoomPosition += new Vector3(currentRoom.width, 0, 0); break;
        }

        // Instantiate the boss room
        Room newRoom = Instantiate(bossRoom.roomPrefab, bossRoomPosition, Quaternion.identity).GetComponent<Room>();
        newRoom.roomSO = bossRoom;
        newRoom.InitializeRoom(roomsCompleted);
        newRoom.isBossRoom = true; 

        // Set up previous and current room references
        previousRoom = currentRoom.gameObject;
        nextRoom = newRoom.gameObject;
        currentRoom = newRoom;
        currentRoomPosition = newRoom.transform.position;
        hasBossRoomSpawned = true;

        newRoom.gameObject.SetActive(true); 

        Invoke("StartBossBattle", 0.5f);
    }



    void StartBossBattle()
    {
        startBossAttack = true;
    }

}
