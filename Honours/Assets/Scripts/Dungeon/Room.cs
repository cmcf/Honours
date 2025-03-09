using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public int width;
    public int height;
    public int x;
    public int y;
    public GridController gridController;
    public ObjectRoomSpawner spawner;
    public RoomSO roomSO;

    public Door leftDoor;
    public Door rightDoor;
    public Door topDoor;
    public Door bottomDoor;

    public bool isBossRoom = false;
    public bool hasSpawnedEnemies = false;
    public bool isCompleted { get; set; } = false;

    public List<Door> doorList = new List<Door>();

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

    private Door.DoorType GetOppositeDoor(Door.DoorType doorType)
    {
        switch (doorType)
        {
            case Door.DoorType.right: return Door.DoorType.left;
            case Door.DoorType.left: return Door.DoorType.right;
            case Door.DoorType.top: return Door.DoorType.bottom;
            case Door.DoorType.bottom: return Door.DoorType.top;
            default: return doorType;
        }
    }

    public void SpawnEnemies()
    {
        if (spawner != null)
        {
            spawner.StartSpawningEnemies(this);
        }
    }

    public void SpawnPickups()
    {
        if (spawner != null)
        {
            spawner.StartSpawningPickups(this);
        }
    }

    public void CheckRoomCompletion()
    {
        if (AreAllEnemiesDefeated() && !isCompleted)
        {
            isCompleted = true;
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
