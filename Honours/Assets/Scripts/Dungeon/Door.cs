using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Door : MonoBehaviour
{
    public enum DoorType
    {
        left, right, top, bottom
    }

    public DoorType doorType;
    public GameObject wallCollider;

    public int roomIndexToLoad;
    float widthOffset = 4f;
    GameObject player;
    private Room parentRoom;
    Switcher switcher;

    Vector2Int gridPosition;

    void Start()
    {
        switcher = FindObjectOfType<Switcher>();
        player = GameObject.FindGameObjectWithTag("Player");
        parentRoom = GetComponentInParent<Room>();

        // Get grid position from parent room
        if (parentRoom != null)
        {
            gridPosition = new Vector2Int(parentRoom.x, parentRoom.y); 
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (parentRoom.AreAllEnemiesDefeated())
            {
                RoomController.Instance.SetLastUsedDoor(doorType); // Set the last used door
                RoomController.Instance.StartRoomTransition(other.gameObject);
                Invoke("LoadLevelAfterADelay", 0.5f);
            }
            else
            {
                Debug.Log("Defeat all enemies before proceeding!");
            }
        }
    }

    void MovePlayer()
    {
        // Find the currently active player character
        GameObject activeCharacter = GameObject.FindGameObjectWithTag("Player"); 

        if (activeCharacter != null)
        {
            switch (doorType)
            {
                case DoorType.left:
                    activeCharacter.transform.position = new Vector2(transform.position.x - widthOffset, transform.position.y);
                    break;
                case DoorType.right:
                    activeCharacter.transform.position = new Vector2(transform.position.x + widthOffset, transform.position.y);
                    break;
                case DoorType.top:
                    activeCharacter.transform.position = new Vector2(transform.position.x, transform.position.y + widthOffset);
                    break;
                case DoorType.bottom:
                    activeCharacter.transform.position = new Vector2(transform.position.x, transform.position.y - widthOffset);
                    break;
            }
        }
    }

    void LoadLevelAfterADelay()
    {
        RoomController.Instance.LoadNextRoom(this);
    }

    public Vector2Int GetGridPosition()
    {
        switch (doorType)
        {
            case DoorType.left:
                return new Vector2Int(parentRoom.x - 1, parentRoom.y);
            case DoorType.right:
                return new Vector2Int(parentRoom.x + 1, parentRoom.y);
            case DoorType.top:
                return new Vector2Int(parentRoom.x, parentRoom.y + 1);
            case DoorType.bottom:
                return new Vector2Int(parentRoom.x, parentRoom.y - 1);
            default:
                return Vector2Int.zero;
        }
    }
}
