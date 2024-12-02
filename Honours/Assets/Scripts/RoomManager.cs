using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public GameObject[] rooms; // Assign room GameObjects in the inspector
    private int currentRoomIndex = 0;

    void Start()
    {
        UpdateRoomVisibility();
    }

    public void OnRoomCleared()
    {
        if (currentRoomIndex < rooms.Length - 1)
        {
            currentRoomIndex++;
            UpdateRoomVisibility();
        }
        else
        {
            Debug.Log("All rooms cleared!");
        }
    }

    void UpdateRoomVisibility()
    {
        for (int i = 0; i < rooms.Length; i++)
        {
            rooms[i].SetActive(i == currentRoomIndex); // Only show the current room
        }
    }
}
