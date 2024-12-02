using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;
    public GameObject[] rooms; 
    private int currentRoomIndex = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateRoomVisibility();
    }

    public void OnRoomCleared()
    {
        // Ensure the room is cleared of enemies
        if (EnemyManager.Instance.enemyCount <= 0) 
        {
            if (currentRoomIndex < rooms.Length - 1)
            {
                currentRoomIndex++;
                UpdateRoomVisibility();
            }
            else
            {
                Debug.Log("All rooms clear!");
            }
        }
    }

    void UpdateRoomVisibility()
    {
        // Disable previous room enemies
        foreach (var room in rooms)
        {
            if (room != rooms[currentRoomIndex])
            {
                // Disable any active room
                room.SetActive(false); 
            }
        }
        // Only show the current room
        rooms[currentRoomIndex].SetActive(true); 
    }
}
