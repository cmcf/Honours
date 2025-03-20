using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static GridController;

public class Room : MonoBehaviour
{
    public int width;
    public int height;
    public int x;
    public int y;

    public Chest chest;
    public ObjectRoomSpawner spawner;
    public RoomSO roomSO;
    public Transform[] spawnPoints;

    public Transform spawnPointLeft;
    public Transform spawnPointRight;
    public Transform spawnPointTop;
    public Transform spawnPointBottom;

    public Door leftDoor;
    public Door rightDoor;
    public Door topDoor;
    public Door bottomDoor;

    public bool isBossRoom = false;
    public bool hasSpawnedEnemies = false;

    public Transform[] enemySpawnPoints;
    public bool isCompleted { get; set; } = false;

    public List<Door> doorList = new List<Door>();

    void Start()
    {
        SetDimensions();
       
    }

    public Transform GetSpawnPoint(Door.DoorType enteringFrom)
    {
        switch (enteringFrom)
        {
            case Door.DoorType.right: return spawnPointLeft; // Entering from the right, spawn at left
            case Door.DoorType.left: return spawnPointRight; // Entering from the left, spawn at right
            case Door.DoorType.top: return spawnPointBottom; // Entering from the top, spawn at bottom
            case Door.DoorType.bottom: return spawnPointTop; // Entering from the bottom, spawn at top
            default: return transform;  // Default to the current rooms transform
        }
    }


    public Door GetMatchingDoor(Door.DoorType enteringFrom)
    {
        foreach (Door door in GetComponentsInChildren<Door>())
        {
            if (door.doorType == GetOppositeDoor(enteringFrom))
            {
                return door;
            }
        }
        return null;
    }

    public Door.DoorType GetOppositeDoor(Door.DoorType door)
    {
        switch (door)
        {
            case Door.DoorType.left: return Door.DoorType.right;
            case Door.DoorType.right: return Door.DoorType.left;
            case Door.DoorType.top: return Door.DoorType.bottom;
            case Door.DoorType.bottom: return Door.DoorType.top;
            default: return door;
        }
    }


    public void InitializeRoom(int roomNumber)
    {
        // Find all doors and assign them
        Door[] doors = GetComponentsInChildren<Door>();
        foreach (Door door in doors)
        {
            doorList.Add(door);
            switch (door.doorType)
            {
                case Door.DoorType.right: rightDoor = door; break;
                case Door.DoorType.left: leftDoor = door; break;
                case Door.DoorType.bottom: bottomDoor = door; break;
                case Door.DoorType.top: topDoor = door; break;
            }
        }

        // Disable all doors if it's the boss room
        if (isBossRoom)
        {
            DisableAllDoors();
        }
    }

    public void SetDimensions()
    {
        BoxCollider2D roomCollider = GetComponent<BoxCollider2D>();

        if (roomCollider != null)
        {
            // Get the size of the room based on the collider's bounds
            width = (int)roomCollider.bounds.size.x; // Automatically set the width
            height = (int)roomCollider.bounds.size.y; // Automatically set the height
        }
    }

    public void DisableAllDoors()
    {
        foreach (Door door in doorList)
        {
            door.gameObject.SetActive(false);
        }
    }

    public void EnableSingleExitDoor(Door.DoorType previousDoor)
    {
        // List to store possible doors - excluding previous entrance
        List<Door> possibleDoors = new List<Door>();

        // Disable all doors first
        foreach (Door door in doorList)
        {
            door.gameObject.SetActive(false);
        }

        // Add doors that are not in the same position as the previous room's entrance
        foreach (Door door in doorList)
        {
            if (door.doorType != GetOppositeDoor(previousDoor))
            {
                possibleDoors.Add(door);
            }
        }

        // Choose a random available door
        if (possibleDoors.Count > 0)
        {
            Door chosenDoor = possibleDoors[Random.Range(0, possibleDoors.Count)];
            chosenDoor.gameObject.SetActive(true);
        }
    }

    public void SpawnEnemies()
    {
        if (spawner != null)
        {
            spawner.StartSpawningEnemies(this);
        }
    }

    public void CheckRoomCompletion()
    {
        if (AreAllEnemiesDefeated() && !isCompleted)
        {
            isCompleted = true;
            // Only call SetEnemiesDefeated if a chest exists
            if (chest != null)
            {
                chest.SetEnemiesDefeated(true);  // Notify the chest that the enemies are defeated
            }
            RoomController.Instance.OnRoomCompleted();
        }
    }

    public bool AreAllEnemiesDefeated()
    {
        return GetComponentsInChildren<Enemy>().Length == 0;
    }

    public Vector3 GetRoomCentre()
    {
        return new Vector3(x * width, y * height);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            RoomController.Instance.OnEnterRoom(this);
        }
    }
}
