using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class RoomController : MonoBehaviour
{
    public static RoomController Instance;
    public Canvas tutorialTextCanvas;
    public List<Weapon> availableWeapons;
    public RoomSO bossRoom;
    public RoomSO spawnRoom;


    public Room currentRoom;
    int roomsCompleted = 0;
    const int roomsBeforeBoss = 2;
    bool leftSpawnRoom = false;

    void Awake()
    {
        Instance = this;
        tutorialTextCanvas.enabled = true;

        // Load the spawn room when the game starts
        LoadRoom(spawnRoom);
    }

    void Update()
    {
        UpdateRooms();
    }

    public void LoadRoom(RoomSO roomSO)
    {
        // Check if the room is already loaded
        if (DoesRoomExist(roomSO))
        {
            return;
        }

        // Instantiate and initialize the room
        Room newRoom = Instantiate(roomSO.roomPrefab).GetComponent<Room>();

        // Initialize new room and update the current room reference
        newRoom.roomSO = roomSO;
        newRoom.InitializeRoom(roomsCompleted);
        currentRoom = newRoom;
    }

    public void OnRoomCompleted()
    {
        roomsCompleted++;

        if (roomsCompleted >= roomsBeforeBoss)
        {
            StartCoroutine(SpawnBossRoom());
        }

        if (!leftSpawnRoom)
        {
            tutorialTextCanvas.enabled = false;
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
        else
        {
            if (leftSpawnRoom)
            {
                currentRoom.SpawnEnemies();
                tutorialTextCanvas.enabled = false;
            }
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
        LoadRoom(bossRoomSO); 
    }
}
