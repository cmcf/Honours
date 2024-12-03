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
        // Set doors to inactive
        foreach (var room in rooms)
        {
            Transform doorTransform = room.transform.Find("Door");
            GameObject doorObject = null;

            if (doorTransform != null)
            {
                doorObject = doorTransform.gameObject;
            }
            if (doorObject != null)
            {
                doorObject.SetActive(false);  
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
            // Trigger a random mechanic for the new room
            TriggerRandomMechanic();
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

    void TriggerRandomMechanic()
    {
        int randomEffect = Random.Range(0, 3);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        switch (randomEffect)
        {
            case 0:
                Debug.Log("Player state change");
       
                if (player != null)
                {
                    // Player changes state
                    player.GetComponent<Player>().ChangePlayerState();
                }
                break;
            case 1:
                Debug.Log("Transform enemy");
                StartCoroutine(WaitAndTransform(1f));
                break;
            case 2:
                
                Debug.Log("Change state!");
                // Player changes state
                player.GetComponent<Player>().ChangePlayerState();
                break;
        }
    }

    IEnumerator WaitAndTransform(float delay)
    {
        yield return new WaitForSeconds(delay);
        TransformPlayerIntoEnemy();
    }

    void TransformPlayerIntoEnemy()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Find all active enemy objects that inherit from the Enemy class
            Enemy[] enemies = FindObjectsOfType<Enemy>();

            if (enemies.Length > 0)
            {
                // Randomly select an enemy from the active enemies
                Enemy randomEnemy = enemies[Random.Range(0, enemies.Length)];
                player.GetComponent<Player>().TransformIntoEnemy(randomEnemy);
            }

        }
    }
}
