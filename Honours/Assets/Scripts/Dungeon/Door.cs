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
    public GameObject doorCollider;

    public int roomIndexToLoad;
    float widthOffset = 4f;
    GameObject player;
    private Room parentRoom;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        parentRoom = GetComponentInParent<Room>(); // Assuming the door is a child of the room
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Only trigger interaction when the player collides with the door
        if (other.CompareTag("Player"))
        {
            // Check if all enemies in the current room are defeated
            if (parentRoom.AreAllEnemiesDefeated())
            {
                // Move the player to the other side of the door
                switch (doorType)
                {
                    case DoorType.left:
                        player.transform.position = new Vector2(transform.position.x - widthOffset, transform.position.y);
                        break;
                    case DoorType.right:
                        player.transform.position = new Vector2(transform.position.x + widthOffset, transform.position.y);
                        break;
                    case DoorType.top:
                        player.transform.position = new Vector2(transform.position.x, transform.position.y + widthOffset);
                        break;
                    case DoorType.bottom:
                        player.transform.position = new Vector2(transform.position.x, transform.position.y - widthOffset);
                        break;
                }
            }
            else
            {
                Debug.Log("Defeat all enemies before proceeding!");
            }
        }
    }
}
