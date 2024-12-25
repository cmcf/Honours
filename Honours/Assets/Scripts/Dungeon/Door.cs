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

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        // Only trigger interaction when player collides with door
        if (other.CompareTag("Player"))
        {
            // Players transform is set to the door position 
            switch(doorType)
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
    }
}
