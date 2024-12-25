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

    public void RemoveUnConnectedDoors()
    {
        foreach(Door door in doorList)
        {
            switch(door.doorType)
            {
                case Door.DoorType.right:
                    if (GetRight() == null) 
                        door.gameObject.SetActive(false);
                    rightDoor = door;
                    break;
                case Door.DoorType.left:
                    if (GetLeft() == null)
                        door.gameObject.SetActive(false);
                    leftDoor = door;
                    break;
                case Door.DoorType.bottom:
                    if (GetBottom() == null)
                        door.gameObject.SetActive(false);
                    bottomDoor = door;
                    break;
                case Door.DoorType.top:
                    if (GetTop() == null)
                        door.gameObject.SetActive(false);
                    topDoor = door;
                    break;
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            RoomController.Instance.OnEnterRoom(this);
        }
    }
}
