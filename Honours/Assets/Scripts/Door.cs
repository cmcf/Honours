using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{

    RoomManager roomManager;
    public int roomIndexToLoad;      

    void Start()
    {
        if (roomManager == null)
        {
            roomManager = RoomManager.Instance; 
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Only trigger interaction when player collides with door
        if (other.CompareTag("Player"))
        {
            // Move the player to the next room when they interact with the door
            if (roomManager != null)
            {
                roomManager.MoveToNextRoom();
            }
        }
    }
}
