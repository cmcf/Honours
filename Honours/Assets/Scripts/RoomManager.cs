using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        // Set doors to inactive except for the first room
        for (int i = 0; i < rooms.Length; i++)
        {
            Transform doorTransform = rooms[i].transform.Find("Door");
            GameObject doorObject = null;

            if (doorTransform != null)
            {
                doorObject = doorTransform.gameObject;
            }

            if (doorObject != null)
            {
                // Always show the door for the first room
                if (i == 0)
                {
                    doorObject.SetActive(true);
                }
                else
                {
                    doorObject.SetActive(false);
                }
            }
        }

        // Update room visibility
        UpdateRoomVisibility();
    }

    public void OnRoomCleared()
    {
        // Ensure the room is cleared of enemies
        if (EnemyManager.Instance.enemyCount <= 0)
        {

            // Activate the door after clearing the room
            GameObject doorObject = rooms[currentRoomIndex].transform.Find("Door")?.gameObject;
            if (doorObject != null)
            {
                if (!doorObject.activeInHierarchy)
                {
                    // Activate the door's GameObject
                    doorObject.SetActive(true);  
                }
            }
        }
    }

    public void MoveToNextRoom()
    {
        if (currentRoomIndex < rooms.Length - 1)
        {
            
            currentRoomIndex++;
            // Spawn player at the room spawn point
            Transform nextRoomSpawn = rooms[currentRoomIndex].transform.Find("SpawnPoint");

            if (nextRoomSpawn != null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    player.transform.position = nextRoomSpawn.position;
                }
            }
            UpdateRoomVisibility();
        }
        else
        {
            SceneManager.LoadScene("WinScene");
        }
    }

    void UpdateRoomVisibility()
    {
        // Disable previous room enemies and deactivate the room
        foreach (var room in rooms)
        {
            if (room != rooms[currentRoomIndex])
            {
                room.SetActive(false);
            }
        }

        // Only show the current room
        rooms[currentRoomIndex].SetActive(true);
    }
}
