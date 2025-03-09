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
        // Room initialization logic
        // Example: You can use roomNumber to load specific room configurations
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
        bool enemiesDefeated = AreAllEnemiesDefeated();
        if (enemiesDefeated && !isCompleted)
        {
            isCompleted = true;
            RoomController.Instance.OnRoomCompleted();
        }
    }

    public bool AreAllEnemiesDefeated()
    {
        Enemy[] enemies = GetComponentsInChildren<Enemy>();
        return enemies.Length == 0;
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
