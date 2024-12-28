using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Room : MonoBehaviour
{
    public int width;
    public int height;
    public int x;
    public int y;

    bool updatedDoors = false;
    public Room(int X, int Y)
    {
        X = x;
        Y = y;
    }

    public Door leftDoor;
    public Door rightDoor;
    public Door topDoor;
    public Door bottomDoor;

    public List <Door> doorList = new List<Door>();
    void Start()
    {
        if (RoomController.Instance == null)
        {
            return;
        }
        Door[] doors = GetComponentsInChildren<Door>();
        foreach(Door door in doors)
        {
            doorList.Add(door);
            switch (door.doorType)
            {
                case Door.DoorType.right:
                    rightDoor = door;
                    break;
                    case Door.DoorType.left:
                    leftDoor = door;
                    break;
                    case Door.DoorType.bottom:
                    bottomDoor = door;
                    break;
                    case Door.DoorType.top:
                    topDoor = door;
                    break;
            }
        }
        RoomController.Instance.RegisterRoom(this);
    }

    void Update()
    {
        if (name.Contains("GameBoss") && !updatedDoors)
        {
            RemoveUnConnectedDoors();
            updatedDoors = true;
        }
    }

    public bool AreAllEnemiesDefeated()
    {
        Enemy[] enemies = GetComponentsInChildren<Enemy>();
        return enemies.Length == 0;
    }

  public void RemoveUnConnectedDoors()
    {
        foreach (Door door in GetComponentsInChildren<Door>())
        {
            // Get the grid position of the neighboring room
            Vector2Int doorPosition = door.GetGridPosition();

            // Use RoomController to find if the neighbor exists
            Room neighbor = RoomController.Instance.FindRoom(doorPosition.x, doorPosition.y);

            if (neighbor == null)
            {
                // No neighbor exists, block this door
                AddWallCollider(door);

                // Optionally disable the door to prevent interaction
                door.gameObject.SetActive(false);
            }
            else
            {
                // Ensure the door is active if a neighbor exists
                door.gameObject.SetActive(true);
            }
        }
    }

    void AddWallCollider(Door door)
    {
        // Create a wall GameObject
        GameObject wall = new GameObject("WallCollider");
        wall.transform.SetParent(transform);
        wall.transform.position = door.transform.position;

        // Add a BoxCollider2D to block the player
        BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();

        // Adjust collider size and alignment based on the door type
        switch (door.doorType)
        {
            case Door.DoorType.left:
            case Door.DoorType.right:
                collider.size = new Vector2(0.5f, height);
                break;
            case Door.DoorType.top:
            case Door.DoorType.bottom:
                collider.size = new Vector2(width, 0.5f);
                break;
        }

        collider.isTrigger = false; // Ensure the collider blocks movement
    }
    public Room GetRight()
    {
        if (RoomController.Instance.DoesRoomExist(x + 1, y))
        {
            return RoomController.Instance.FindRoom(x + 1, y);
        }
        return null;
    }
    public Room GetLeft()
    {
        if (RoomController.Instance.DoesRoomExist(x - 1, y))
        {
            return RoomController.Instance.FindRoom(x - 1, y);
        }
        return null;
    }
    public Room GetTop()
    {
        if (RoomController.Instance.DoesRoomExist(x, y + 1))
        {
            return RoomController.Instance.FindRoom(x, y + 1);
        }
        return null;
    }
    public Room GetBottom()
    {
        if (RoomController.Instance.DoesRoomExist(x, y - 1))
        {
            return RoomController.Instance.FindRoom(x, y -1);
        }
        return null;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(width, height, 0));
    }

    public Vector3 GetRoomCentre()
    {
        return new Vector3(x * width, y * height);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            RoomController.Instance.OnEnterRoom(this);
        }
    }
}
