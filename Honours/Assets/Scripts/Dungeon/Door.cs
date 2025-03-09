using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    Vector2Int gridPosition;

    void Start()
    {
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
        // Only trigger interaction when the player collides with the door
        if (other.CompareTag("Player"))
        {
            // Check if all enemies in the current room are defeated
            if (parentRoom.AreAllEnemiesDefeated())
            {
                MovePlayer();
                RoomController.Instance.LoadNextRoom(this);


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
