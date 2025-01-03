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

    public bool isBossRoom = false;

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
            // Remove all doors for the boss room
            if (isBossRoom)
            {
                Destroy(door.gameObject);
            }
            else
            {
                // Get the grid position of the neighbouring room
                Vector2Int doorPosition = door.GetGridPosition();

                // Use RoomController to find if the room neighbour exists
                Room neighbor = RoomController.Instance.FindRoom(doorPosition.x, doorPosition.y);

                if (neighbor == null)
                {
                    door.gameObject.SetActive(false);
                }
                else
                {
                    door.gameObject.SetActive(true);
                }
            }
            
        }
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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            RoomController.Instance.OnEnterRoom(this);
        }
    }
}
